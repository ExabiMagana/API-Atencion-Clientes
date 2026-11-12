using AtencionClientesAPI.Data;
using AtencionClientesAPI.DTOs;
using AtencionClientesAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AtencionClientesAPI.Services;

public class TurnoService
{
    private readonly AppDbContext _context;
    private readonly NotificacionService _notificaciones;

    public TurnoService(AppDbContext context, NotificacionService notificaciones)
    {
        _context = context;
        _notificaciones = notificaciones;
    }

    public async Task<TurnoRespuestaDto> CrearAsync(
        CrearTurnoDto dto,
        CancellationToken cancellationToken)
    {
        var existeCliente = await _context.Clientes
            .AnyAsync(
                cliente => cliente.Id == dto.ClienteId,
                cancellationToken);

        if (!existeCliente)
        {
            throw new ReglaNegocioException(
                404,
                "El cliente no existe.");
        }

        var turno = new Turno
        {
            ClienteId = dto.ClienteId,
            Estado = EstadoTurno.Pendiente,
            FechaCreacion = DateTimeOffset.UtcNow
        };

        _context.Turnos.Add(turno);

        await _context.SaveChangesAsync(cancellationToken);

        var respuesta = Mapear(turno);

        await _notificaciones.NotificarCambioAsync(
            respuesta,
            nuevo: true);

        return respuesta;
    }

    public async Task<List<TurnoRespuestaDto>> ObtenerPendientesAsync(
        CancellationToken cancellationToken)
    {
        var turnos = await _context.Turnos
            .AsNoTracking()
            .Where(turno => turno.Estado == EstadoTurno.Pendiente)
            .OrderBy(turno => turno.FechaCreacion)
            .ThenBy(turno => turno.Id)
            .ToListAsync(cancellationToken);

        return turnos.Select(Mapear).ToList();
    }

    public async Task<TurnoRespuestaDto> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var turno = await _context.Turnos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                turno => turno.Id == id,
                cancellationToken);

        if (turno is null)
        {
            throw new ReglaNegocioException(
                404,
                "El turno no existe.");
        }

        return Mapear(turno);
    }

    public async Task<PosicionTurnoDto> ObtenerPosicionAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var resultado = await _context.Turnos
            .AsNoTracking()
            .Where(turno => turno.Id == id)
            .Select(turno => new
            {
                turno.Id,
                turno.Estado,
                Posicion = turno.Estado == EstadoTurno.Pendiente
                    ? (int?)(_context.Turnos.Count(anterior =>
                        anterior.Estado == EstadoTurno.Pendiente &&
                        (
                            anterior.FechaCreacion < turno.FechaCreacion ||
                            (
                                anterior.FechaCreacion == turno.FechaCreacion &&
                                anterior.Id < turno.Id
                            )
                        )) + 1)
                    : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (resultado is null)
        {
            throw new ReglaNegocioException(
                404,
                "El turno no existe.");
        }

        return new PosicionTurnoDto
        {
            TurnoId = resultado.Id,
            Estado = resultado.Estado.ToString(),
            Posicion = resultado.Posicion
        };
    }

    public async Task<TurnoRespuestaDto> AsignarAsync(
        int id,
        AsignarTurnoDto dto,
        CancellationToken cancellationToken)
    {
        var turno = await BuscarParaModificarAsync(id, cancellationToken);

        if (turno.Estado != EstadoTurno.Pendiente)
        {
            throw new ReglaNegocioException(
                409,
                "Solo se pueden asignar turnos pendientes.");
        }

        var empleado = await _context.Empleados
            .AsNoTracking()
            .FirstOrDefaultAsync(
                empleado => empleado.Id == dto.EmpleadoId,
                cancellationToken);

        if (empleado is null)
        {
            throw new ReglaNegocioException(
                404,
                "El empleado no existe.");
        }

        if (!empleado.Activo)
        {
            throw new ReglaNegocioException(
                409,
                "El empleado está inactivo.");
        }

        var ocupado = await _context.Turnos
            .AnyAsync(
                otro => otro.EmpleadoId == dto.EmpleadoId &&
                        otro.Estado == EstadoTurno.EnAtencion,
                cancellationToken);

        if (ocupado)
        {
            throw new ReglaNegocioException(
                409,
                "El empleado ya está atendiendo otro turno.");
        }

        turno.EmpleadoId = dto.EmpleadoId;
        turno.Estado = EstadoTurno.EnAtencion;
        turno.FechaInicioAtencion = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var respuesta = Mapear(turno);

        await _notificaciones.NotificarCambioAsync(respuesta);

        return respuesta;
    }

    public async Task<TurnoRespuestaDto> CambiarEstadoAsync(
        int id,
        EstadoTurno nuevoEstado,
        CancellationToken cancellationToken)
    {
        var turno = await BuscarParaModificarAsync(id, cancellationToken);

        if (turno.Estado == EstadoTurno.Atendido ||
            turno.Estado == EstadoTurno.Cancelado)
        {
            throw new ReglaNegocioException(
                409,
                "Un turno atendido o cancelado no puede cambiar de estado.");
        }

        switch (nuevoEstado)
        {
            case EstadoTurno.Atendido:
                if (turno.Estado != EstadoTurno.EnAtencion)
                {
                    throw new ReglaNegocioException(
                        409,
                        "El turno debe estar en atención para finalizarlo.");
                }

                turno.Estado = EstadoTurno.Atendido;
                turno.FechaFinalizacion = DateTimeOffset.UtcNow;
                break;

            case EstadoTurno.Cancelado:
                turno.Estado = EstadoTurno.Cancelado;
                turno.FechaCancelacion = DateTimeOffset.UtcNow;
                break;

            case EstadoTurno.EnAtencion:
                throw new ReglaNegocioException(
                    409,
                    "Para iniciar la atención utiliza el endpoint de asignación e indica un empleado.");

            case EstadoTurno.Pendiente:
                throw new ReglaNegocioException(
                    409,
                    "No se permite devolver un turno al estado pendiente.");

            default:
                throw new ReglaNegocioException(
                    400,
                    "El estado indicado no es válido.");
        }

        await _context.SaveChangesAsync(cancellationToken);

        var respuesta = Mapear(turno);

        await _notificaciones.NotificarCambioAsync(respuesta);

        return respuesta;
    }

    private async Task<Turno> BuscarParaModificarAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var turno = await _context.Turnos
            .FirstOrDefaultAsync(
                turno => turno.Id == id,
                cancellationToken);

        if (turno is null)
        {
            throw new ReglaNegocioException(
                404,
                "El turno no existe.");
        }

        return turno;
    }

    private static TurnoRespuestaDto Mapear(Turno turno)
    {
        return new TurnoRespuestaDto
        {
            Id = turno.Id,
            ClienteId = turno.ClienteId,
            EmpleadoId = turno.EmpleadoId,
            Estado = turno.Estado.ToString(),
            FechaCreacion = turno.FechaCreacion,
            FechaInicioAtencion = turno.FechaInicioAtencion,
            FechaFinalizacion = turno.FechaFinalizacion,
            FechaCancelacion = turno.FechaCancelacion
        };
    }
}
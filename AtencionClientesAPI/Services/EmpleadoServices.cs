using AtencionClientesAPI.Data;
using AtencionClientesAPI.DTOs;
using AtencionClientesAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AtencionClientesAPI.Services;

public class EmpleadoService
{
    private readonly AppDbContext _context;

    public EmpleadoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmpleadoRespuestaDto> CrearAsync(
        CrearEmpleadoDto dto,
        CancellationToken cancellationToken)
    {
        var empleado = new Empleado
        {
            Nombre = dto.Nombre.Trim(),
            Activo = true
        };

        _context.Empleados.Add(empleado);

        await _context.SaveChangesAsync(cancellationToken);

        return new EmpleadoRespuestaDto
        {
            Id = empleado.Id,
            Nombre = empleado.Nombre,
            Activo = empleado.Activo,
            Disponible = true
        };
    }

    public async Task<EmpleadoRespuestaDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.Empleados
            .AsNoTracking()
            .Where(empleado => empleado.Id == id)
            .Select(empleado => new EmpleadoRespuestaDto
            {
                Id = empleado.Id,
                Nombre = empleado.Nombre,
                Activo = empleado.Activo,
                Disponible = empleado.Activo &&
                    !empleado.Turnos.Any(
                        turno => turno.Estado == EstadoTurno.EnAtencion)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<EmpleadoRespuestaDto>> ObtenerDisponiblesAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Empleados
            .AsNoTracking()
            .Where(empleado =>
                empleado.Activo &&
                !empleado.Turnos.Any(
                    turno => turno.Estado == EstadoTurno.EnAtencion))
            .OrderBy(empleado => empleado.Nombre)
            .ThenBy(empleado => empleado.Id)
            .Select(empleado => new EmpleadoRespuestaDto
            {
                Id = empleado.Id,
                Nombre = empleado.Nombre,
                Activo = empleado.Activo,
                Disponible = true
            })
            .ToListAsync(cancellationToken);
    }
}
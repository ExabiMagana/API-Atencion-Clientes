using AtencionClientesAPI.Data;
using AtencionClientesAPI.DTOs;
using AtencionClientesAPI.Hubs;
using AtencionClientesAPI.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AtencionClientesAPI.Services;

public class NotificacionService
{
    private readonly IHubContext<TurnosHub> _hub;
    private readonly AppDbContext _context;
    private readonly ILogger<NotificacionService> _logger;

    public NotificacionService(
        IHubContext<TurnosHub> hub,
        AppDbContext context,
        ILogger<NotificacionService> logger)
    {
        _hub = hub;
        _context = context;
        _logger = logger;
    }

    public async Task NotificarCambioAsync(
        TurnoRespuestaDto turno,
        bool nuevo = false)
    {
        using var limite = new CancellationTokenSource(
            TimeSpan.FromSeconds(5));

        try
        {
            var eventoEmpleados = nuevo
                ? "NuevoTurno"
                : "TurnoActualizado";

            await Task.WhenAll(
                _hub.Clients
                    .Group(GruposTurnos.Empleados)
                    .SendAsync(
                        eventoEmpleados,
                        turno,
                        limite.Token),

                _hub.Clients
                    .Group(GruposTurnos.Cliente(turno.ClienteId))
                    .SendAsync(
                        "TurnoActualizado",
                        turno,
                        limite.Token)
            );

            var pendientes = await _context.Turnos
                .AsNoTracking()
                .Where(t => t.Estado == EstadoTurno.Pendiente)
                .OrderBy(t => t.FechaCreacion)
                .ThenBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.ClienteId
                })
                .ToListAsync(limite.Token);

            var generadaEn = DateTimeOffset.UtcNow;

            var envios = pendientes.Select((pendiente, indice) =>
                _hub.Clients
                    .Group(GruposTurnos.Cliente(pendiente.ClienteId))
                    .SendAsync(
                        "PosicionActualizada",
                        new
                        {
                            TurnoId = pendiente.Id,
                            Posicion = indice + 1,
                            GeneradaEn = generadaEn
                        },
                        limite.Token));

            await Task.WhenAll(envios);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "El turno {TurnoId} quedó guardado, " +
                "pero no se completó el envío de notificaciones.",
                turno.Id);
        }
    }
}
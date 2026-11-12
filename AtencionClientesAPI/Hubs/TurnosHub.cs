using AtencionClientesAPI.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AtencionClientesAPI.Hubs;

public static class GruposTurnos
{
    public const string Empleados = "empleados";

    public static string Cliente(int clienteId)
    {
        return $"cliente-{clienteId}";
    }
}

public class TurnosHub : Hub
{
    private readonly AppDbContext _context;

    public TurnosHub(AppDbContext context)
    {
        _context = context;
    }

    public async Task UnirseEmpleados()
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GruposTurnos.Empleados);
    }

    public async Task UnirseCliente(int clienteId)
    {
        var existe = await _context.Clientes
            .AsNoTracking()
            .AnyAsync(
                cliente => cliente.Id == clienteId,
                Context.ConnectionAborted);

        if (!existe)
        {
            throw new HubException("El cliente no existe.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GruposTurnos.Cliente(clienteId));
    }
}
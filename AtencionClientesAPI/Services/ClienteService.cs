using AtencionClientesAPI.Data;
using AtencionClientesAPI.DTOs;
using AtencionClientesAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AtencionClientesAPI.Services;

public class ClienteService
{
    private readonly AppDbContext _context;

    public ClienteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClienteRespuestaDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Id)
            .Select(cliente => new ClienteRespuestaDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Telefono = cliente.Telefono,
                Correo = cliente.Correo
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ClienteRespuestaDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.Id == id)
            .Select(cliente => new ClienteRespuestaDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Telefono = cliente.Telefono,
                Correo = cliente.Correo
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClienteRespuestaDto> CrearAsync(
        GuardarClienteDto dto,
        CancellationToken cancellationToken)
    {
        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Telefono = LimpiarOpcional(dto.Telefono),
            Correo = LimpiarOpcional(dto.Correo)
        };

        _context.Clientes.Add(cliente);

        await _context.SaveChangesAsync(cancellationToken);

        return new ClienteRespuestaDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Telefono = cliente.Telefono,
            Correo = cliente.Correo
        };
    }

    public async Task<bool> ActualizarAsync(
        int id,
        GuardarClienteDto dto,
        CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(
                cliente => cliente.Id == id,
                cancellationToken);

        if (cliente is null)
        {
            return false;
        }

        cliente.Nombre = dto.Nombre.Trim();
        cliente.Telefono = LimpiarOpcional(dto.Telefono);
        cliente.Correo = LimpiarOpcional(dto.Correo);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string? LimpiarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
    }
}
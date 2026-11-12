using AtencionClientesAPI.DTOs;
using AtencionClientesAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AtencionClientesAPI.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly ClienteService _service;

    public ClientesController(ClienteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClienteRespuestaDto>>> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var clientes = await _service.ObtenerTodosAsync(cancellationToken);

        return Ok(clientes);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<ClienteRespuestaDto>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var cliente = await _service.ObtenerPorIdAsync(id, cancellationToken);

        if (cliente is null)
        {
            return NotFound(new
            {
                mensaje = "El cliente no existe."
            });
        }

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteRespuestaDto>> Crear(
        [FromBody] GuardarClienteDto dto,
        CancellationToken cancellationToken)
    {
        var cliente = await _service.CrearAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = cliente.Id },
            cliente);
    }

    [HttpPut("{id:int:min(1)}")]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] GuardarClienteDto dto,
        CancellationToken cancellationToken)
    {
        var actualizado = await _service.ActualizarAsync(
            id,
            dto,
            cancellationToken);

        if (!actualizado)
        {
            return NotFound(new
            {
                mensaje = "El cliente no existe."
            });
        }

        return NoContent();
    }
}
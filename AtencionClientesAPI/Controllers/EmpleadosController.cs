using AtencionClientesAPI.DTOs;
using AtencionClientesAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AtencionClientesAPI.Controllers;

[ApiController]
[Route("api/empleados")]
public class EmpleadosController : ControllerBase
{
    private readonly EmpleadoService _service;

    public EmpleadosController(EmpleadoService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<EmpleadoRespuestaDto>> Crear(
        [FromBody] CrearEmpleadoDto dto,
        CancellationToken cancellationToken)
    {
        var empleado = await _service.CrearAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = empleado.Id },
            empleado);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<EmpleadoRespuestaDto>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var empleado = await _service.ObtenerPorIdAsync(
            id,
            cancellationToken);

        if (empleado is null)
        {
            return NotFound(new
            {
                mensaje = "El empleado no existe."
            });
        }

        return Ok(empleado);
    }

    [HttpGet("disponibles")]
    public async Task<ActionResult<List<EmpleadoRespuestaDto>>> ObtenerDisponibles(
        CancellationToken cancellationToken)
    {
        var empleados = await _service.ObtenerDisponiblesAsync(
            cancellationToken);

        return Ok(empleados);
    }
}
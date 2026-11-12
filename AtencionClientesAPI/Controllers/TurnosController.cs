using AtencionClientesAPI.DTOs;
using AtencionClientesAPI.Models;
using AtencionClientesAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AtencionClientesAPI.Controllers;

[ApiController]
[Route("api/turnos")]
public class TurnosController : ControllerBase
{
    private readonly TurnoService _service;

    public TurnosController(TurnoService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<TurnoRespuestaDto>> Crear(
        [FromBody] CrearTurnoDto dto,
        CancellationToken cancellationToken)
    {
        var turno = await _service.CrearAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = turno.Id },
            turno);
    }

    [HttpGet("pendientes")]
    public async Task<ActionResult<List<TurnoRespuestaDto>>> ObtenerPendientes(
        CancellationToken cancellationToken)
    {
        var turnos = await _service.ObtenerPendientesAsync(cancellationToken);

        return Ok(turnos);
    }

    [HttpGet("{id:int:min(1)}")]
    public async Task<ActionResult<TurnoRespuestaDto>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var turno = await _service.ObtenerPorIdAsync(id, cancellationToken);

        return Ok(turno);
    }

    [HttpGet("{id:int:min(1)}/posicion")]
    public async Task<ActionResult<PosicionTurnoDto>> ObtenerPosicion(
        int id,
        CancellationToken cancellationToken)
    {
        var posicion = await _service.ObtenerPosicionAsync(
            id,
            cancellationToken);

        return Ok(posicion);
    }

    [HttpPut("{id:int:min(1)}/asignacion")]
    public async Task<ActionResult<TurnoRespuestaDto>> Asignar(
        int id,
        [FromBody] AsignarTurnoDto dto,
        CancellationToken cancellationToken)
    {
        var turno = await _service.AsignarAsync(id, dto, cancellationToken);

        return Ok(turno);
    }

    [HttpPatch("{id:int:min(1)}/estado")]
    public async Task<ActionResult<TurnoRespuestaDto>> CambiarEstado(
        int id,
        [FromBody] CambiarEstadoTurnoDto dto,
        CancellationToken cancellationToken)
    {
        var turno = await _service.CambiarEstadoAsync(
            id,
            dto.Estado!.Value,
            cancellationToken);

        return Ok(turno);
    }

    [HttpPost("{id:int:min(1)}/finalizar")]
    public async Task<ActionResult<TurnoRespuestaDto>> Finalizar(
        int id,
        CancellationToken cancellationToken)
    {
        var turno = await _service.CambiarEstadoAsync(
            id,
            EstadoTurno.Atendido,
            cancellationToken);

        return Ok(turno);
    }
}
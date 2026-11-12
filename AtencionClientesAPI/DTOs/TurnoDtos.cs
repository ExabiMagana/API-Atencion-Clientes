using System.ComponentModel.DataAnnotations;
using AtencionClientesAPI.Models;

namespace AtencionClientesAPI.DTOs;

public class CrearTurnoDto
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }
}

public class AsignarTurnoDto
{
    [Range(1, int.MaxValue)]
    public int EmpleadoId { get; set; }
}

public class CambiarEstadoTurnoDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [EnumDataType(typeof(EstadoTurno))]
    public EstadoTurno? Estado { get; set; }
}

public class TurnoRespuestaDto
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public int? EmpleadoId { get; set; }

    public string Estado { get; set; } = string.Empty;

    public DateTimeOffset FechaCreacion { get; set; }

    public DateTimeOffset? FechaInicioAtencion { get; set; }

    public DateTimeOffset? FechaFinalizacion { get; set; }

    public DateTimeOffset? FechaCancelacion { get; set; }
}

public class PosicionTurnoDto
{
    public int TurnoId { get; set; }

    public string Estado { get; set; } = string.Empty;

    public int? Posicion { get; set; }
}
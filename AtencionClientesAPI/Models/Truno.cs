namespace AtencionClientesAPI.Models;

public class Turno
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public int? EmpleadoId { get; set; }

    public Empleado? Empleado { get; set; }

    public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;

    public DateTimeOffset FechaCreacion { get; set; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset? FechaInicioAtencion { get; set; }

    public DateTimeOffset? FechaFinalizacion { get; set; }

    public DateTimeOffset? FechaCancelacion { get; set; }

    public byte[] Version { get; set; } = Array.Empty<byte>();
}
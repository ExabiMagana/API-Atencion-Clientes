namespace AtencionClientesAPI.Models;

public class Empleado
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
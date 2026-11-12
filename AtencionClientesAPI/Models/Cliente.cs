namespace AtencionClientesAPI.Models;

public class Cliente
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
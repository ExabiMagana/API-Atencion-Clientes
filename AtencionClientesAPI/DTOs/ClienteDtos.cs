using System.ComponentModel.DataAnnotations;

namespace AtencionClientesAPI.DTOs;

public class GuardarClienteDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre admite hasta 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(25, ErrorMessage = "El teléfono admite hasta 25 caracteres.")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(254, ErrorMessage = "El correo admite hasta 254 caracteres.")]
    public string? Correo { get; set; }
}

public class ClienteRespuestaDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }
}
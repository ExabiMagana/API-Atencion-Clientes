using System.ComponentModel.DataAnnotations;

namespace AtencionClientesAPI.DTOs;

public class CrearEmpleadoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre admite hasta 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}

public class EmpleadoRespuestaDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public bool Disponible { get; set; }
}
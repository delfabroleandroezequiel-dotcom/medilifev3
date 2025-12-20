using System.ComponentModel.DataAnnotations;

namespace MedilifeSaludV3.Web.Models;

public class Empleado : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string RazonSocial { get; set; } = "";

    [MaxLength(200)]
    public string? DescripcionUsuario { get; set; }

    [MaxLength(200)]
    public string? NombreEmpresa { get; set; }

    [MaxLength(20)]
    public string? Cuil { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(250)]
    public string? Direccion { get; set; }
}

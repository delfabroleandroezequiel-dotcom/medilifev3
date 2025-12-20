using System.ComponentModel.DataAnnotations;

namespace MedilifeSaludV3.Web.Models;

public class Institucion : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = "";

    [MaxLength(20)]
    public string? Cuil { get; set; }

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(250)]
    public string? Direccion { get; set; }
}

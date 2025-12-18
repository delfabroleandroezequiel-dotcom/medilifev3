using System.ComponentModel.DataAnnotations;

namespace MedilifeSaludV3.Web.Models;

public class Medico : AuditableEntity
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(200)]
    public string Apellido { get; set; } = "";

    public string? Comentario { get; set; }
}

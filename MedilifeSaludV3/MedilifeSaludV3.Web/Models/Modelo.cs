using System.ComponentModel.DataAnnotations;

namespace MedilifeSaludV3.Web.Models;

public class Modelo : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";
}

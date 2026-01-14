using System.ComponentModel.DataAnnotations;

namespace MedilifeSaludV3.Web.Models;

public class Remito : AuditableEntity
{
    public int Id { get; set; }

    [Required, StringLength(300)]
    public string Descripcion { get; set; } = "";
}

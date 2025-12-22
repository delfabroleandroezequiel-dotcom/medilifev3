using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedilifeSaludV3.Web.Models;

public class PresupuestoItem : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    public int PresupuestoId { get; set; }

    [Required]
    [MaxLength(400)]
    public string Detalle { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 1")]
    public int Cantidad { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    public int Orden { get; set; }

    [NotMapped]
    public decimal TotalLinea => Cantidad * PrecioUnitario;

    public Presupuesto Presupuesto { get; set; } = default!;
}

using System.ComponentModel.DataAnnotations;

namespace MedilifeSaludV3.Web.Models.ViewModels;

public class PresupuestoEditVm
{
    public int? Id { get; set; }

    [Required]
    public int Numero { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Required]
    public int EmpresaId { get; set; }

    public int? PrestadorId { get; set; }
    public int? InstitucionId { get; set; }
    public int? PacienteId { get; set; }
    public int? MedicoId { get; set; }

    [MaxLength(1000)]
    public string? DescripcionPago { get; set; } = "* El presupuesto tendra validez por 15 días al momento de ser emitido";

    public List<PresupuestoItemVm> Items { get; set; } = new();
}

public class PresupuestoItemVm
{
    public int? Id { get; set; }

    [MaxLength(400)]
    public string? Detalle { get; set; }

    public int Cantidad { get; set; } = 1;

    public decimal PrecioUnitario { get; set; }

    public int Orden { get; set; }
}

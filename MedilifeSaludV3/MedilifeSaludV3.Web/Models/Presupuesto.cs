using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedilifeSaludV3.Web.Models;

public class Presupuesto : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    public int Numero { get; set; }

    [Required]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Required]
    public int EmpresaId { get; set; }

    public int? PrestadorId { get; set; }
    public int? InstitucionId { get; set; }
    public int? PacienteId { get; set; }
    public int? MedicoId { get; set; }

    [MaxLength(1000)]
    public string? DescripcionPago { get; set; } = "* El presupuesto tendra validez por 15 días al momento de ser emitido";

    public Empleado? Empresa { get; set; }
    public ObraSocial? Prestador { get; set; }
    public Institucion? Institucion { get; set; }
    public Paciente? Paciente { get; set; }
    public Medico? Medico { get; set; }

    public List<PresupuestoItem> Items { get; set; } = new();

    [NotMapped]
    public decimal Total => Items.Sum(i => i.TotalLinea);

    [NotMapped]
    public string TotalEnLetras { get; set; } = string.Empty;
}

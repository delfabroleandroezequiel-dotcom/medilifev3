using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedilifeSaludV3.Web.Models;

public class Stock : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    public StockEstado Estado { get; set; } = StockEstado.D;

    public int? MarcaId { get; set; }
    public MarcaStock? Marca { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    public int? ModeloId { get; set; }
    public Modelo? Modelo { get; set; }

    public int? TipoMaterialId { get; set; }
    public TipoMaterial? TipoMaterial { get; set; }

    [StringLength(200)]
    public string? Lote { get; set; }

    [StringLength(200)]
    public string? Ref { get; set; }

    [StringLength(200)]
    public string? SN { get; set; } // SN/N° de serie

    [StringLength(200)]
    public string? GETIN { get; set; }

    public decimal? Diametro { get; set; }

    [StringLength(200)]
    public string? Largo { get; set; }

    public TipoMedida? TipoMedida { get; set; }

    public int? ProveedorCompraId { get; set; }
    public ObraSocial? ProveedorCompra { get; set; } // según requerimiento

    public DateTime? FechaCompra { get; set; }

    [StringLength(200)]
    public string? RemitoCompra { get; set; }

    [StringLength(200)]
    public string? FacturaCompra { get; set; }

    public string? DetalleCompra { get; set; }

    public DateTime? FechaIngreso { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaFacturaCompra { get; set; }
    public DateTime? FechaRemitoCompra { get; set; }

    public int? InstitucionActualId { get; set; }
    public Institucion? InstitucionActual { get; set; }

    public int? PacienteActualId { get; set; }
    public Paciente? PacienteActual { get; set; }

    public DateTime? FechaDevolucion { get; set; }

    public bool Consignado { get; set; }

    public string? Comentario { get; set; }

    public int? Copias { get; set; }

    public string? Descripcion { get; set; }

    [StringLength(200)]
    public string? MedidaViejoFormato { get; set; }

    [StringLength(200)]
    public string? MarcaDescripcion { get; set; }

    [StringLength(200)]
    public string? ModeloDescripcion { get; set; }

    public int? ViejoId { get; set; }

    public int? MedicoActualId { get; set; }
    public Medico? MedicoActual { get; set; }

    public int? French { get; set; }

    public ICollection<StockRemito> StockRemitos { get; set; } = new List<StockRemito>();
    public ICollection<StockFactura> StockFacturas { get; set; } = new List<StockFactura>();

    [NotMapped]
    public List<int> RemitoAsociadoIds { get; set; } = new();

    [NotMapped]
    public List<int> FacturaAsociadaIds { get; set; } = new();
}

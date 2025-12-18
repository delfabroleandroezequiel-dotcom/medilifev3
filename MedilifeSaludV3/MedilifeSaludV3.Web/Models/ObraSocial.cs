using System.ComponentModel.DataAnnotations;

namespace MedilifeSaludV3.Web.Models;

public class ObraSocial : AuditableEntity
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Nombre { get; set; } = ""; // "Obra Social"

    [StringLength(20)]
    public string? Cuit { get; set; }

    [StringLength(300)]
    public string? Direccion { get; set; }

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? TelFax { get; set; }

    /// <summary>
    /// Excento (tal cual pedido). Se muestra como switch en ABM y como Sí/No en consulta.
    /// </summary>
    public bool Excento { get; set; }

    [Required]
    public CondicionAnteIVA CondicionAnteIVA { get; set; } = CondicionAnteIVA.Inscripto;

    public string? Comentario { get; set; }
}
namespace MedilifeSaludV3.Web.Models
{
    public abstract class AuditableEntity
    {
        public DateTime Creado { get; set; }
        public string? CreadoPor { get; set; }
        public DateTime? Modificado { get; set; }
        public string? ModificadoPor { get; set; }
    }
}

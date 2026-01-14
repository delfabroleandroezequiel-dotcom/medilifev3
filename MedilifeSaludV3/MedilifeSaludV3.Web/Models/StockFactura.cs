namespace MedilifeSaludV3.Web.Models;

public class StockFactura
{
    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public int FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;
}

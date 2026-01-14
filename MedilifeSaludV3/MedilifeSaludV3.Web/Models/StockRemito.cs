namespace MedilifeSaludV3.Web.Models;

public class StockRemito
{
    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public int RemitoId { get; set; }
    public Remito Remito { get; set; } = null!;
}

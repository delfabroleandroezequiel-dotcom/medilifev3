namespace MedilifeSaludV3.Web.Services.Excel
{
    public class ExcelColumn<T>
    {
        public string Header { get; set; } = "";
        public Func<T, object?> Value { get; set; } = _ => null!;
    }
}

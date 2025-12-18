using ClosedXML.Excel;

namespace MedilifeSaludV3.Web.Services.Excel
{
    public class ExcelExportService
    {
        public byte[] Export<T>(
            IEnumerable<T> data,
            string sheetName,
            IEnumerable<ExcelColumn<T>> columns)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            var colIndex = 1;
            foreach (var col in columns)
            {
                ws.Cell(1, colIndex).Value = col.Header;
                ws.Cell(1, colIndex).Style.Font.Bold = true;
                colIndex++;
            }

            var row = 2;
            foreach (var item in data)
            {
                colIndex = 1;
                foreach (var col in columns)
                {
                    var cell = ws.Cell(row, colIndex);
                    var v = col.Value(item);

                    switch (v)
                    {
                        case null:
                            cell.Value = "";
                            break;

                        case DateTime dt:
                            cell.Value = dt;
                            break;

                        case DateTimeOffset dto:
                            cell.Value = dto.DateTime;
                            break;

                        case bool b:
                            cell.Value = b;
                            break;

                        case int i:
                            cell.Value = i;
                            break;

                        case long l:
                            cell.Value = l;
                            break;

                        case decimal dec:
                            cell.Value = (double)dec; // Excel trabaja mejor con double
                            break;

                        case double d:
                            cell.Value = d;
                            break;

                        case float f:
                            cell.Value = (double)f;
                            break;

                        default:
                            cell.Value = v.ToString() ?? "";
                            break;
                    }
                    colIndex++;
                }
                row++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}

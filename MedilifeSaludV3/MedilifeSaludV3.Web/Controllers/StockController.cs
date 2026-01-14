using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class StockController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelExportService _excel;

    public StockController(AppDbContext db, ExcelExportService excel)
    {
        _db = db;
        _excel = excel;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Stocks.AsNoTracking()
            .Include(x => x.Marca)
            .Include(x => x.Modelo)
            .Include(x => x.TipoMaterial)
            .Include(x => x.ProveedorCompra)
            .Include(x => x.InstitucionActual)
            .Include(x => x.PacienteActual)
            .Include(x => x.MedicoActual)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return View(list);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Stocks.AsNoTracking()
            .Include(x => x.Marca)
            .Include(x => x.Modelo)
            .Include(x => x.TipoMaterial)
            .Include(x => x.ProveedorCompra)
            .Include(x => x.InstitucionActual)
            .Include(x => x.PacienteActual)
            .Include(x => x.MedicoActual)
            .Include(x => x.StockRemitos).ThenInclude(sr => sr.Remito)
            .Include(x => x.StockFacturas).ThenInclude(sf => sf.Factura)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null) return NotFound();

        return View(entity);
    }

    public async Task<IActionResult> Create()
    {
        await LoadSelectListsAsync();
        return View(new Stock());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Stock entity)
    {
        await LoadSelectListsAsync();

        if (!ModelState.IsValid) return View(entity);

        // asociaciones many-to-many
        entity.StockRemitos = entity.RemitoAsociadoIds.Select(id => new StockRemito { RemitoId = id }).ToList();
        entity.StockFacturas = entity.FacturaAsociadaIds.Select(id => new StockFactura { FacturaId = id }).ToList();

        _db.Add(entity);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Stocks
            .Include(x => x.StockRemitos)
            .Include(x => x.StockFacturas)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null) return NotFound();

        entity.RemitoAsociadoIds = entity.StockRemitos.Select(x => x.RemitoId).ToList();
        entity.FacturaAsociadaIds = entity.StockFacturas.Select(x => x.FacturaId).ToList();

        await LoadSelectListsAsync();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Stock entity)
    {
        if (id != entity.Id) return NotFound();

        await LoadSelectListsAsync();
        if (!ModelState.IsValid) return View(entity);

        var current = await _db.Stocks
            .Include(x => x.StockRemitos)
            .Include(x => x.StockFacturas)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (current == null) return NotFound();

        // update scalar props
        _db.Entry(current).CurrentValues.SetValues(entity);

        // update remitos
        current.StockRemitos.Clear();
        foreach (var rid in entity.RemitoAsociadoIds.Distinct())
            current.StockRemitos.Add(new StockRemito { StockId = current.Id, RemitoId = rid });

        // update facturas
        current.StockFacturas.Clear();
        foreach (var fid in entity.FacturaAsociadaIds.Distinct())
            current.StockFacturas.Add(new StockFactura { StockId = current.Id, FacturaId = fid });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Stocks.AsNoTracking()
            .Include(x => x.Marca)
            .Include(x => x.Modelo)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null) return NotFound();

        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.Stocks
            .Include(x => x.StockRemitos)
            .Include(x => x.StockFacturas)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity != null)
        {
            _db.Stocks.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var data = await _db.Stocks.AsNoTracking()
            .Include(x => x.Marca)
            .Include(x => x.Modelo)
            .Include(x => x.TipoMaterial)
            .Include(x => x.ProveedorCompra)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        var bytes = _excel.Export(data, "Stock", GetColumns());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Stock.xlsx");
    }

    public async Task<IActionResult> ExportExcelFiltered(
        string? estado,
        string? title,
        int? marcaId,
        int? modeloId,
        int? tipoMaterialId,
        string? lote,
        string? refValue,
        string? sn,
        int? proveedorCompraId,
        string? consignado,
        DateTime? ingresoDesde,
        DateTime? ingresoHasta,
        DateTime? vencDesde,
        DateTime? vencHasta,
        DateTime? creadoDesde,
        DateTime? creadoHasta,
        DateTime? modDesde,
        DateTime? modHasta)
    {
        var q = _db.Stocks.AsNoTracking()
            .Include(x => x.Marca)
            .Include(x => x.Modelo)
            .Include(x => x.TipoMaterial)
            .Include(x => x.ProveedorCompra);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<StockEstado>(estado, out var est))
            q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.Estado == est);

        if (!string.IsNullOrWhiteSpace(title)) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.Title != null && x.Title.Contains(title));
        if (marcaId.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.MarcaId == marcaId.Value);
        if (modeloId.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.ModeloId == modeloId.Value);
        if (tipoMaterialId.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.TipoMaterialId == tipoMaterialId.Value);
        if (!string.IsNullOrWhiteSpace(lote)) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.Lote != null && x.Lote.Contains(lote));
        if (!string.IsNullOrWhiteSpace(refValue)) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.Ref != null && x.Ref.Contains(refValue));
        if (!string.IsNullOrWhiteSpace(sn)) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.SN != null && x.SN.Contains(sn));
        if (proveedorCompraId.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.ProveedorCompraId == proveedorCompraId.Value);

        if (!string.IsNullOrWhiteSpace(consignado))
        {
            if (consignado == "SI") q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.Consignado);
            if (consignado == "NO") q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => !x.Consignado);
        }

        if (ingresoDesde.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.FechaIngreso != null && x.FechaIngreso >= ingresoDesde.Value);
        if (ingresoHasta.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.FechaIngreso != null && x.FechaIngreso <= ingresoHasta.Value);
        if (vencDesde.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.FechaVencimiento != null && x.FechaVencimiento >= vencDesde.Value);
        if (vencHasta.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.FechaVencimiento != null && x.FechaVencimiento <= vencHasta.Value);

        if (creadoDesde.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.Creado >= creadoDesde.Value);
        if (creadoHasta.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.Creado <= creadoHasta.Value);
        if (modDesde.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.Modificado != null && x.Modificado >= modDesde.Value);
        if (modHasta.HasValue) q = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Stock, ObraSocial?>)q.Where(x => x.Modificado != null && x.Modificado <= modHasta.Value);

        var data = await q.OrderByDescending(x => x.Id).ToListAsync();
        var bytes = _excel.Export(data, "Stock", GetColumns());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Stock.xlsx");
    }

    private async Task LoadSelectListsAsync()
    {
        ViewBag.Marcas = new SelectList(await _db.Marcas.AsNoTracking().OrderBy(x => x.Marca).ToListAsync(), "Id", "Marca");
        ViewBag.Modelos = new SelectList(await _db.Modelos.AsNoTracking().OrderBy(x => x.Title).ToListAsync(), "Id", "Title");
        ViewBag.TiposMaterial = new SelectList(await _db.TiposMaterial.AsNoTracking().OrderBy(x => x.Title).ToListAsync(), "Id", "Title");
        ViewBag.Proveedores = new SelectList(await _db.ObrasSociales.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(), "Id", "Nombre");
        ViewBag.Instituciones = new SelectList(await _db.Instituciones.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(), "Id", "Nombre");

        var pacientes = await _db.Pacientes.AsNoTracking()
            .OrderBy(e => e.Apellido).ThenBy(e => e.Nombre)
            .Select(p => new { p.Id, Nombre = p.Apellido + ", " + p.Nombre })
            .ToListAsync();
        ViewBag.Pacientes = new SelectList(pacientes, "Id", "Nombre");

        var medicos = await _db.Medicos.AsNoTracking()
            .OrderBy(e => e.Apellido).ThenBy(e => e.Nombre)
            .Select(m => new { m.Id, Nombre = m.Apellido + ", " + m.Nombre })
            .ToListAsync();
        ViewBag.Medicos = new SelectList(medicos, "Id", "Nombre");

        ViewBag.Remitos = new MultiSelectList(await _db.Remitos.AsNoTracking().OrderBy(x => x.Descripcion).ToListAsync(), "Id", "Descripcion");
        ViewBag.Facturas = new MultiSelectList(await _db.Facturas.AsNoTracking().OrderBy(x => x.Descripcion).ToListAsync(), "Id", "Descripcion");
    }

    private static IEnumerable<ExcelColumn<Stock>> GetColumns()
    {
        return new List<ExcelColumn<Stock>>
    {
        new ExcelColumn<Stock>{ Header="Id", Value = x => x.Id },
        new ExcelColumn<Stock>{ Header="Estado", Value = x => x.Estado.ToString() },
        new ExcelColumn<Stock>{ Header="Marca", Value = x => x.Marca != null ? x.Marca.Marca : "" },
        new ExcelColumn<Stock>{ Header="Title", Value = x => x.Title ?? "" },
        new ExcelColumn<Stock>{ Header="Modelo", Value = x => x.Modelo != null ? x.Modelo.Title : "" },
        new ExcelColumn<Stock>{ Header="Tipo material", Value = x => x.TipoMaterial != null ? x.TipoMaterial.Title : "" },
        new ExcelColumn<Stock>{ Header="Lote", Value = x => x.Lote ?? "" },
        new ExcelColumn<Stock>{ Header="Ref", Value = x => x.Ref ?? "" },
        new ExcelColumn<Stock>{ Header="SN", Value = x => x.SN ?? "" },
        new ExcelColumn<Stock>{ Header="GETIN", Value = x => x.GETIN ?? "" },
        new ExcelColumn<Stock>{ Header="Diametro", Value = x => x.Diametro },
        new ExcelColumn<Stock>{ Header="Largo", Value = x => x.Largo ?? "" },
        new ExcelColumn<Stock>{ Header="Tipo medida", Value = x => x.TipoMedida != null ? x.TipoMedida.ToString() : "" },
        new ExcelColumn<Stock>{ Header="Proveedor compra", Value = x => x.ProveedorCompra != null ? x.ProveedorCompra.Nombre : "" },
        new ExcelColumn<Stock>{ Header="Fecha compra", Value = x => x.FechaCompra },
        new ExcelColumn<Stock>{ Header="Fecha ingreso", Value = x => x.FechaIngreso },
        new ExcelColumn<Stock>{ Header="Fecha vencimiento", Value = x => x.FechaVencimiento },
        new ExcelColumn<Stock>{ Header="Consignado", Value = x => x.Consignado ? "Sí" : "No" },
        new ExcelColumn<Stock>{ Header="Copias", Value = x => x.Copias },
        new ExcelColumn<Stock>{ Header="Creado", Value = x => x.Creado },
        new ExcelColumn<Stock>{ Header="Creado por", Value = x => x.CreadoPor ?? "" },
        new ExcelColumn<Stock>{ Header="Modificado", Value = x => x.Modificado },
        new ExcelColumn<Stock>{ Header="Modificado por", Value = x => x.ModificadoPor ?? "" }
    };
    }
}

using DocumentFormat.OpenXml.Wordprocessing;
using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class FacturaController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelExportService _excel;

    public FacturaController(AppDbContext db, ExcelExportService excel)
    {
        _db = db;
        _excel = excel;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Facturas.AsNoTracking()
            .OrderBy(x => x.Descripcion)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Facturas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    public IActionResult Create() => View(new Factura());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Descripcion")] Factura entity)
    {
        if (!ModelState.IsValid) return View(entity);
        _db.Add(entity);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Facturas.FindAsync(id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion")] Factura entity)
    {
        if (id != entity.Id) return NotFound();
        if (!ModelState.IsValid) return View(entity);

        try
        {
            _db.Update(entity);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Facturas.AnyAsync(e => e.Id == entity.Id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Facturas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.Facturas.FindAsync(id);
        if (entity != null)
        {
            _db.Facturas.Remove(entity);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var data = await _db.Facturas.AsNoTracking().OrderBy(x => x.Descripcion).ToListAsync();
        var bytes = _excel.Export(data, "Facturas", GetColumns());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facturas.xlsx");
    }

    public async Task<IActionResult> ExportExcelFiltered(
        string? descripcion,
        DateTime? creadoDesde,
        DateTime? creadoHasta,
        DateTime? modDesde,
        DateTime? modHasta)
    {
        var q = _db.Facturas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(descripcion)) q = q.Where(x => x.Descripcion.Contains(descripcion));

        if (creadoDesde.HasValue) q = q.Where(x => x.Creado >= creadoDesde.Value);
        if (creadoHasta.HasValue) q = q.Where(x => x.Creado <= creadoHasta.Value);
        if (modDesde.HasValue) q = q.Where(x => x.Modificado != null && x.Modificado >= modDesde.Value);
        if (modHasta.HasValue) q = q.Where(x => x.Modificado != null && x.Modificado <= modHasta.Value);

        var data = await q.OrderBy(x => x.Descripcion).ToListAsync();
        var bytes = _excel.Export(data, "Facturas", GetColumns());
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facturas.xlsx");
    }

    private static IEnumerable<ExcelColumn<Factura>> GetColumns()
    {
        return new List<ExcelColumn<Factura>>
        {
            new ExcelColumn<Factura>{ Header="Id", Value=x=> x.Id },
            new ExcelColumn<Factura>{ Header="Descripción", Value =x => x.Descripcion },
            new ExcelColumn<Factura>{ Header="Creado", Value=x => x.Creado },
            new ExcelColumn<Factura>{ Header="Creado por", Value= x => x.CreadoPor ?? "" },
            new ExcelColumn<Factura>{ Header="Modificado", Value=x=> x.Modificado },
            new ExcelColumn<Factura>{ Header="Modificado por", Value=x => x.ModificadoPor ?? "" }
        };
    }
}

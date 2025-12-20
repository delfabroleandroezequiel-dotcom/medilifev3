using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class TipoFacturaController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelExportService _excel;

    public TipoFacturaController(AppDbContext db, ExcelExportService excel)
    {
        _db = db;
        _excel = excel;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.TiposFactura.AsNoTracking().OrderBy(x => x.Title).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.TiposFactura.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    public IActionResult Create() => View(new TipoFactura());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title")] TipoFactura model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.TiposFactura.FindAsync(id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] TipoFactura model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var current = await _db.TiposFactura.FirstOrDefaultAsync(x => x.Id == id);
        if (current == null) return NotFound();
        current.Title = model.Title;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.TiposFactura.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.TiposFactura.FindAsync(id);
        if (entity != null)
        {
            _db.TiposFactura.Remove(entity);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var data = await _db.TiposFactura.AsNoTracking().OrderBy(x => x.Title).ToListAsync();
        var cols = new[]
        {
            new ExcelColumn<TipoFactura>{ Header="Title", Value=x=>x.Title },
            new ExcelColumn<TipoFactura>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<TipoFactura>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<TipoFactura>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<TipoFactura>{ Header="Modificado por", Value=x=>x.ModificadoPor },
        };
        var bytes = _excel.Export(data, "Tipos de Factura", cols);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"TipoFactura_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportExcelFiltered(string? title, DateTime? creadoDesde, DateTime? creadoHasta, DateTime? modDesde, DateTime? modHasta)
    {
        var query = _db.TiposFactura.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(title)) query = query.Where(x => x.Title.Contains(title));
        if (creadoDesde.HasValue) query = query.Where(x => x.Creado.Date >= creadoDesde.Value.Date);
        if (creadoHasta.HasValue) query = query.Where(x => x.Creado.Date <= creadoHasta.Value.Date);
        if (modDesde.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date >= modDesde.Value.Date);
        if (modHasta.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date <= modHasta.Value.Date);

        var data = await query.OrderBy(x => x.Title).ToListAsync();
        var cols = new[]
        {
            new ExcelColumn<TipoFactura>{ Header="Title", Value=x=>x.Title },
            new ExcelColumn<TipoFactura>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<TipoFactura>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<TipoFactura>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<TipoFactura>{ Header="Modificado por", Value=x=>x.ModificadoPor },
        };
        var bytes = _excel.Export(data, "Tipos de Factura", cols);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"TipoFactura_FILTRADO_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}

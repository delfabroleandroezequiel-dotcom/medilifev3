using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class ModeloController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelExportService _excel;

    public ModeloController(AppDbContext db, ExcelExportService excel)
    {
        _db = db;
        _excel = excel;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Modelos.AsNoTracking().OrderBy(x => x.Title).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Modelos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    public IActionResult Create() => View(new Modelo());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title")] Modelo model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Modelos.FindAsync(id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] Modelo model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var current = await _db.Modelos.FirstOrDefaultAsync(x => x.Id == id);
        if (current == null) return NotFound();
        current.Title = model.Title;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Modelos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.Modelos.FindAsync(id);
        if (entity != null)
        {
            _db.Modelos.Remove(entity);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var data = await _db.Modelos.AsNoTracking().OrderBy(x => x.Title).ToListAsync();
        var cols = new[]
        {
            new ExcelColumn<Modelo>{ Header="Title", Value=x=>x.Title },
            new ExcelColumn<Modelo>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<Modelo>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<Modelo>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<Modelo>{ Header="Modificado por", Value=x=>x.ModificadoPor },
        };
        var bytes = _excel.Export(data, "Modelos", cols);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Modelos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportExcelFiltered(string? title, DateTime? creadoDesde, DateTime? creadoHasta, DateTime? modDesde, DateTime? modHasta)
    {
        var query = _db.Modelos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(title)) query = query.Where(x => x.Title.Contains(title));
        if (creadoDesde.HasValue) query = query.Where(x => x.Creado.Date >= creadoDesde.Value.Date);
        if (creadoHasta.HasValue) query = query.Where(x => x.Creado.Date <= creadoHasta.Value.Date);
        if (modDesde.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date >= modDesde.Value.Date);
        if (modHasta.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date <= modHasta.Value.Date);

        var data = await query.OrderBy(x => x.Title).ToListAsync();

        var cols = new[]
        {
            new ExcelColumn<Modelo>{ Header="Title", Value=x=>x.Title },
            new ExcelColumn<Modelo>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<Modelo>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<Modelo>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<Modelo>{ Header="Modificado por", Value=x=>x.ModificadoPor },
        };

        var bytes = _excel.Export(data, "Modelos", cols);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Modelos_FILTRADO_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}

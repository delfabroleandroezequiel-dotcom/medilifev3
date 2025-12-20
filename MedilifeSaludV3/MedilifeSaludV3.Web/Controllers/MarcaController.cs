using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class MarcaController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelExportService _excel;

    public MarcaController(AppDbContext db, ExcelExportService excel)
    {
        _db = db;
        _excel = excel;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Marcas.AsNoTracking().OrderBy(x => x.Marca).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Marcas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    public IActionResult Create() => View(new MarcaStock());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Marca")] MarcaStock model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Marcas.FindAsync(id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Marca")] MarcaStock model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var current = await _db.Marcas.FirstOrDefaultAsync(x => x.Id == id);
        if (current == null) return NotFound();
        current.Marca = model.Marca;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Marcas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.Marcas.FindAsync(id);
        if (entity != null)
        {
            _db.Marcas.Remove(entity);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var data = await _db.Marcas.AsNoTracking().OrderBy(x => x.Marca).ToListAsync();
        var cols = new[]
        {
            new ExcelColumn<MarcaStock>{ Header="Marca", Value=x=>x.Marca },
            new ExcelColumn<MarcaStock>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<MarcaStock>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<MarcaStock>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<MarcaStock>{ Header="Modificado por", Value=x=>x.ModificadoPor },
        };
        var bytes = _excel.Export(data, "Marcas", cols);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Marcas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportExcelFiltered(string? marca, DateTime? creadoDesde, DateTime? creadoHasta, DateTime? modDesde, DateTime? modHasta)
    {
        var query = _db.Marcas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(marca)) query = query.Where(x => x.Marca.Contains(marca));
        if (creadoDesde.HasValue) query = query.Where(x => x.Creado.Date >= creadoDesde.Value.Date);
        if (creadoHasta.HasValue) query = query.Where(x => x.Creado.Date <= creadoHasta.Value.Date);
        if (modDesde.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date >= modDesde.Value.Date);
        if (modHasta.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date <= modHasta.Value.Date);

        var data = await query.OrderBy(x => x.Marca).ToListAsync();

        var cols = new[]
        {
            new ExcelColumn<MarcaStock>{ Header="Marca", Value=x=>x.Marca },
            new ExcelColumn<MarcaStock>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<MarcaStock>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<MarcaStock>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<MarcaStock>{ Header="Modificado por", Value=x=>x.ModificadoPor },
        };

        var bytes = _excel.Export(data, "Marcas", cols);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Marcas_FILTRADO_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}

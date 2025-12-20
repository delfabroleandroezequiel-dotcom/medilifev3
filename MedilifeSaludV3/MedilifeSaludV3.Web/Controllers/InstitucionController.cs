using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class InstitucionController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelExportService _excel;

    public InstitucionController(AppDbContext db, ExcelExportService excel)
    {
        _db = db;
        _excel = excel;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Instituciones.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Instituciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    public IActionResult Create() => View(new Institucion());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Cuil,Telefono,Email,Direccion")] Institucion model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Instituciones.FindAsync(id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Cuil,Telefono,Email,Direccion")] Institucion model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var current = await _db.Instituciones.FirstOrDefaultAsync(x => x.Id == id);
        if (current == null) return NotFound();

        current.Nombre = model.Nombre;
        current.Cuil = model.Cuil;
        current.Telefono = model.Telefono;
        current.Email = model.Email;
        current.Direccion = model.Direccion;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Instituciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.Instituciones.FindAsync(id);
        if (entity != null)
        {
            _db.Instituciones.Remove(entity);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var data = await _db.Instituciones.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();

        var cols = new[]
        {
            new ExcelColumn<Institucion>{ Header="Nombre", Value=x=>x.Nombre },
            new ExcelColumn<Institucion>{ Header="CUIL", Value=x=>x.Cuil },
            new ExcelColumn<Institucion>{ Header="Teléfono", Value=x=>x.Telefono },
            new ExcelColumn<Institucion>{ Header="Email", Value=x=>x.Email },
            new ExcelColumn<Institucion>{ Header="Dirección", Value=x=>x.Direccion },
            new ExcelColumn<Institucion>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<Institucion>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<Institucion>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<Institucion>{ Header="Modificado por", Value=x=>x.ModificadoPor },
        };

        var bytes = _excel.Export(data, "Instituciones", cols);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Instituciones_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportExcelFiltered(
        string? nombre,
        string? cuil,
        string? telefono,
        string? email,
        string? direccion,
        DateTime? creadoDesde,
        DateTime? creadoHasta,
        DateTime? modDesde,
        DateTime? modHasta)
    {
        var query = _db.Instituciones.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nombre)) query = query.Where(x => x.Nombre.Contains(nombre));
        if (!string.IsNullOrWhiteSpace(cuil)) query = query.Where(x => x.Cuil != null && x.Cuil.Contains(cuil));
        if (!string.IsNullOrWhiteSpace(telefono)) query = query.Where(x => x.Telefono != null && x.Telefono.Contains(telefono));
        if (!string.IsNullOrWhiteSpace(email)) query = query.Where(x => x.Email != null && x.Email.Contains(email));
        if (!string.IsNullOrWhiteSpace(direccion)) query = query.Where(x => x.Direccion != null && x.Direccion.Contains(direccion));

        if (creadoDesde.HasValue) query = query.Where(x => x.Creado.Date >= creadoDesde.Value.Date);
        if (creadoHasta.HasValue) query = query.Where(x => x.Creado.Date <= creadoHasta.Value.Date);
        if (modDesde.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date >= modDesde.Value.Date);
        if (modHasta.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date <= modHasta.Value.Date);

        var data = await query.OrderBy(x => x.Nombre).ToListAsync();

        var cols = new[]
        {
            new ExcelColumn<Institucion>{ Header="Nombre", Value=x=>x.Nombre },
            new ExcelColumn<Institucion>{ Header="CUIL", Value=x=>x.Cuil },
            new ExcelColumn<Institucion>{ Header="Teléfono", Value=x=>x.Telefono },
            new ExcelColumn<Institucion>{ Header="Email", Value=x=>x.Email },
            new ExcelColumn<Institucion>{ Header="Dirección", Value=x=>x.Direccion },
            new ExcelColumn<Institucion>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<Institucion>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<Institucion>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<Institucion>{ Header="Modificado por", Value=x=>x.ModificadoPor },
        };

        var bytes = _excel.Export(data, "Instituciones", cols);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Instituciones_FILTRADO_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}

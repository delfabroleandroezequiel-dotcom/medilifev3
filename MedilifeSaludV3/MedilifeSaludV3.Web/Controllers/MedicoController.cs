using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class MedicoController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelExportService _excel;

    public MedicoController(AppDbContext db, ExcelExportService excel)
    {
        _db = db;
        _excel = excel;
    }

    // GET: /Medico
    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Medicos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x =>
                x.Nombre.Contains(q) ||
                x.Apellido.Contains(q) ||
                (x.Comentario != null && x.Comentario.Contains(q)));
        }

        var list = await query
            .OrderBy(x => x.Apellido)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        ViewBag.Q = q ?? "";
        return View(list);
    }

    // GET: /Medico/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Medicos.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null) return NotFound();
        return View(entity);
    }

    // GET: /Medico/Create
    public IActionResult Create() => View(new Medico());

    // POST: /Medico/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Apellido,Comentario")] Medico medico)
    {
        if (!ModelState.IsValid) return View(medico);

        _db.Add(medico);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Medico/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Medicos.FindAsync(id);
        if (entity == null) return NotFound();

        return View(entity);
    }

    // POST: /Medico/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,Comentario")] Medico medico)
    {
        if (id != medico.Id) return NotFound();
        if (!ModelState.IsValid) return View(medico);

        try
        {
            var current = await _db.Medicos.FirstOrDefaultAsync(x => x.Id == id);
            if (current == null) return NotFound();

            current.Nombre = medico.Nombre;
            current.Apellido = medico.Apellido;
            current.Comentario = medico.Comentario;

            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _db.Medicos.AnyAsync(e => e.Id == medico.Id);
            if (!exists) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Medico/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Medicos.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null) return NotFound();
        return View(entity);
    }

    // POST: /Medico/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.Medicos.FindAsync(id);
        if (entity != null)
        {
            _db.Medicos.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Medico/ExportExcel
    public async Task<IActionResult> ExportExcel(string? q)
    {
        var query = _db.Medicos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x =>
                x.Nombre.Contains(q) ||
                x.Apellido.Contains(q) ||
                (x.Comentario != null && x.Comentario.Contains(q)));
        }

        var data = await query
            .OrderBy(x => x.Apellido)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        var columns = new[]
        {
            new ExcelColumn<Medico>{ Header="Apellido", Value=x=>x.Apellido },
            new ExcelColumn<Medico>{ Header="Nombre", Value=x=>x.Nombre },
            new ExcelColumn<Medico>{ Header="Comentario", Value=x=>x.Comentario },
            new ExcelColumn<Medico>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<Medico>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<Medico>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<Medico>{ Header="Modificado por", Value=x=>x.ModificadoPor }
        };

        var bytes = _excel.Export(data, "Médicos", columns);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Medicos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}

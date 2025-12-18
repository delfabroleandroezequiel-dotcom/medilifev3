using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class PacienteController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelExportService _excel;

    public PacienteController(AppDbContext db, ExcelExportService excel)
    {
        _db = db;
        _excel = excel;
    }

    // GET: /Paciente
    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Pacientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x => x.Nombre.Contains(q) || x.Apellido.Contains(q));
        }

        var list = await query
            .OrderBy(x => x.Apellido)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        ViewBag.Q = q ?? "";
        return View(list);
    }

    // GET: /Paciente/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null) return NotFound();
        return View(entity);
    }

    // GET: /Paciente/Create
    public IActionResult Create() => View(new Paciente());

    // POST: /Paciente/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Apellido")] Paciente paciente)
    {
        if (!ModelState.IsValid) return View(paciente);

        _db.Add(paciente);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Paciente/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Pacientes.FindAsync(id);
        if (entity == null) return NotFound();

        return View(entity);
    }

    // POST: /Paciente/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido")] Paciente paciente)
    {
        if (id != paciente.Id) return NotFound();
        if (!ModelState.IsValid) return View(paciente);

        try
        {
            var current = await _db.Pacientes.FirstOrDefaultAsync(x => x.Id == id);
            if (current == null) return NotFound();

            current.Nombre = paciente.Nombre;
            current.Apellido = paciente.Apellido;

            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _db.Pacientes.AnyAsync(e => e.Id == paciente.Id);
            if (!exists) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Paciente/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Pacientes.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null) return NotFound();
        return View(entity);
    }

    // POST: /Paciente/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.Pacientes.FindAsync(id);
        if (entity != null)
        {
            _db.Pacientes.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Paciente/ExportExcel
    public async Task<IActionResult> ExportExcel(string? q)
    {
        var query = _db.Pacientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x => x.Nombre.Contains(q) || x.Apellido.Contains(q));
        }

        var data = await query
            .OrderBy(x => x.Apellido)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        var columns = new[]
        {
            new ExcelColumn<Paciente>{ Header="Apellido", Value=x=>x.Apellido },
            new ExcelColumn<Paciente>{ Header="Nombre", Value=x=>x.Nombre },
            new ExcelColumn<Paciente>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<Paciente>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<Paciente>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<Paciente>{ Header="Modificado por", Value=x=>x.ModificadoPor }
        };

        var bytes = _excel.Export(data, "Pacientes", columns);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Pacientes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}

using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class ObraSocialController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelExportService _excel;

    public ObraSocialController(AppDbContext db, ExcelExportService excel)
    {
        _db = db;
        _excel = excel;
    }

    // GET: /ObraSocial
    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.ObrasSociales.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x =>
                x.Nombre.Contains(q) ||
                (x.Cuit != null && x.Cuit.Contains(q)) ||
                (x.Email != null && x.Email.Contains(q)));
        }

        var list = await query
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        ViewBag.Q = q ?? "";
        return View(list);
    }

    // GET: /ObraSocial/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.ObrasSociales.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null) return NotFound();

        return View(entity);
    }

    // GET: /ObraSocial/Create
    public IActionResult Create() => View(new ObraSocial());

    // POST: /ObraSocial/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Cuit,Direccion,Email,TelFax,Excento,CondicionAnteIVA,Comentario")] ObraSocial obraSocial)
    {
        if (!ModelState.IsValid) return View(obraSocial);

        // Auditoría: la setea AppDbContext automáticamente
        _db.Add(obraSocial);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: /ObraSocial/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.ObrasSociales.FindAsync(id);
        if (entity == null) return NotFound();

        return View(entity);
    }

    // POST: /ObraSocial/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Cuit,Direccion,Email,TelFax,Excento,CondicionAnteIVA,Comentario")] ObraSocial obraSocial)
    {
        if (id != obraSocial.Id) return NotFound();
        if (!ModelState.IsValid) return View(obraSocial);

        try
        {
            // Para NO pisar auditoría (Creado/CreadoPor), actualizamos sobre la entidad existente
            var current = await _db.ObrasSociales.FirstOrDefaultAsync(x => x.Id == id);
            if (current == null) return NotFound();

            current.Nombre = obraSocial.Nombre;
            current.Cuit = obraSocial.Cuit;
            current.Direccion = obraSocial.Direccion;
            current.Email = obraSocial.Email;
            current.TelFax = obraSocial.TelFax;
            current.Excento = obraSocial.Excento;
            current.CondicionAnteIVA = obraSocial.CondicionAnteIVA;
            current.Comentario = obraSocial.Comentario;

            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _db.ObrasSociales.AnyAsync(e => e.Id == obraSocial.Id);
            if (!exists) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /ObraSocial/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.ObrasSociales.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null) return NotFound();

        return View(entity);
    }

    // POST: /ObraSocial/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.ObrasSociales.FindAsync(id);
        if (entity != null)
        {
            _db.ObrasSociales.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> ExportExcelFiltered(
    string? nombre,
    string? cuit,
    string? email,
    string? telFax,
    string? excento,
    string? condicionAnteIVA,
    DateTime? creadoDesde,
    DateTime? creadoHasta,
    DateTime? modDesde,
    DateTime? modHasta)
    {
        var query = _db.ObrasSociales.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nombre)) query = query.Where(x => x.Nombre.Contains(nombre));
        if (!string.IsNullOrWhiteSpace(cuit)) query = query.Where(x => x.Cuit != null && x.Cuit.Contains(cuit));
        if (!string.IsNullOrWhiteSpace(email)) query = query.Where(x => x.Email != null && x.Email.Contains(email));
        if (!string.IsNullOrWhiteSpace(telFax)) query = query.Where(x => x.TelFax != null && x.TelFax.Contains(telFax));

        if (!string.IsNullOrWhiteSpace(excento))
        {
            // el filtro viene como "Sí" / "No"
            if (excento.Equals("Sí", StringComparison.OrdinalIgnoreCase) || excento.Equals("Si", StringComparison.OrdinalIgnoreCase))
                query = query.Where(x => x.Excento);
            else if (excento.Equals("No", StringComparison.OrdinalIgnoreCase))
                query = query.Where(x => !x.Excento);
        }

        if (!string.IsNullOrWhiteSpace(condicionAnteIVA))
        {
            // Mejor práctica: filtrar por el valor del enum (traducible por EF Core)
            // En la UI el combo envía el int del enum.
            if (int.TryParse(condicionAnteIVA, out var iva))
            {
                query = query.Where(x => (int)x.CondicionAnteIVA == iva);
            }
        }

        if (creadoDesde.HasValue) query = query.Where(x => x.Creado.Date >= creadoDesde.Value.Date);
        if (creadoHasta.HasValue) query = query.Where(x => x.Creado.Date <= creadoHasta.Value.Date);

        if (modDesde.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date >= modDesde.Value.Date);
        if (modHasta.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date <= modHasta.Value.Date);

        var data = await query.OrderBy(x => x.Nombre).ToListAsync();

        var columns = new[]
        {
        new ExcelColumn<ObraSocial>{ Header="Obra Social", Value=x=>x.Nombre },
        new ExcelColumn<ObraSocial>{ Header="Excento", Value=x=>x.Excento ? "Sí" : "No" },
        new ExcelColumn<ObraSocial>{ Header="Condición ante IVA", Value=x=>x.CondicionAnteIVA.GetDisplayName() },
        new ExcelColumn<ObraSocial>{ Header="CUIT", Value=x=>x.Cuit },
        new ExcelColumn<ObraSocial>{ Header="Email", Value=x=>x.Email },
        new ExcelColumn<ObraSocial>{ Header="Tel/Fax", Value=x=>x.TelFax },
        new ExcelColumn<ObraSocial>{ Header="Creado", Value=x=>x.Creado },
        new ExcelColumn<ObraSocial>{ Header="Creado por", Value=x=>x.CreadoPor },
        new ExcelColumn<ObraSocial>{ Header="Modificado", Value=x=>x.Modificado },
        new ExcelColumn<ObraSocial>{ Header="Modificado por", Value=x=>x.ModificadoPor }
    };

        var bytes = _excel.Export(data, "Obras Sociales", columns);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ObrasSociales_FILTRADO_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
    // GET: /ObraSocial/ExportExcel
    public async Task<IActionResult> ExportExcel(string? q)
    {
        var query = _db.ObrasSociales.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x =>
                x.Nombre.Contains(q) ||
                (x.Cuit != null && x.Cuit.Contains(q)) ||
                (x.Email != null && x.Email.Contains(q)));
        }

        var data = await query.OrderBy(x => x.Nombre).ToListAsync();

        var columns = new[]
        {
            new ExcelColumn<ObraSocial>{ Header="Obra Social", Value=x=>x.Nombre },
            new ExcelColumn<ObraSocial>{ Header="Excento", Value=x=>x.Excento ? "Sí" : "No" },
            new ExcelColumn<ObraSocial>{ Header="Condición ante IVA", Value=x=>x.CondicionAnteIVA.GetDisplayName() },
            new ExcelColumn<ObraSocial>{ Header="CUIT", Value=x=>x.Cuit },
            new ExcelColumn<ObraSocial>{ Header="Email", Value=x=>x.Email },
            new ExcelColumn<ObraSocial>{ Header="Tel/Fax", Value=x=>x.TelFax },
            new ExcelColumn<ObraSocial>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<ObraSocial>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<ObraSocial>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<ObraSocial>{ Header="Modificado por", Value=x=>x.ModificadoPor }
        };

        var bytes = _excel.Export(data, "Obras Sociales", columns);

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ObrasSociales_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}
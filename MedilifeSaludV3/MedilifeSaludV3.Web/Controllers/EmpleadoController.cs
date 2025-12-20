using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class EmpleadoController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelExportService _excel;

    public EmpleadoController(AppDbContext db, ExcelExportService excel)
    {
        _db = db;
        _excel = excel;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Empleados.AsNoTracking()
            .OrderBy(x => x.RazonSocial)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Empleados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    public IActionResult Create() => View(new Empleado());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("RazonSocial,DescripcionUsuario,NombreEmpresa,Cuil,Email,Telefono,Direccion")] Empleado model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Empleados.FindAsync(id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,RazonSocial,DescripcionUsuario,NombreEmpresa,Cuil,Email,Telefono,Direccion")] Empleado model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var current = await _db.Empleados.FirstOrDefaultAsync(x => x.Id == id);
        if (current == null) return NotFound();

        current.RazonSocial = model.RazonSocial;
        current.DescripcionUsuario = model.DescripcionUsuario;
        current.NombreEmpresa = model.NombreEmpresa;
        current.Cuil = model.Cuil;
        current.Email = model.Email;
        current.Telefono = model.Telefono;
        current.Direccion = model.Direccion;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var entity = await _db.Empleados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.Empleados.FindAsync(id);
        if (entity != null)
        {
            _db.Empleados.Remove(entity);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var data = await _db.Empleados.AsNoTracking().OrderBy(x => x.RazonSocial).ToListAsync();
        var cols = new[]
        {
            new ExcelColumn<Empleado>{ Header="Razón Social", Value=x=>x.RazonSocial },
            new ExcelColumn<Empleado>{ Header="Descripción Usuario", Value=x=>x.DescripcionUsuario },
            new ExcelColumn<Empleado>{ Header="Nombre Empresa", Value=x=>x.NombreEmpresa },
            new ExcelColumn<Empleado>{ Header="CUIL", Value=x=>x.Cuil },
            new ExcelColumn<Empleado>{ Header="Email", Value=x=>x.Email },
            new ExcelColumn<Empleado>{ Header="Teléfono", Value=x=>x.Telefono },
            new ExcelColumn<Empleado>{ Header="Dirección", Value=x=>x.Direccion },
            new ExcelColumn<Empleado>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<Empleado>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<Empleado>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<Empleado>{ Header="Modificado por", Value=x=>x.ModificadoPor },
        };
        var bytes = _excel.Export(data, "Empleados", cols);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Empleados_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> ExportExcelFiltered(
        string? razonSocial,
        string? descripcionUsuario,
        string? nombreEmpresa,
        string? cuil,
        string? email,
        string? telefono,
        string? direccion,
        DateTime? creadoDesde,
        DateTime? creadoHasta,
        DateTime? modDesde,
        DateTime? modHasta)
    {
        var query = _db.Empleados.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(razonSocial)) query = query.Where(x => x.RazonSocial.Contains(razonSocial));
        if (!string.IsNullOrWhiteSpace(descripcionUsuario)) query = query.Where(x => x.DescripcionUsuario != null && x.DescripcionUsuario.Contains(descripcionUsuario));
        if (!string.IsNullOrWhiteSpace(nombreEmpresa)) query = query.Where(x => x.NombreEmpresa != null && x.NombreEmpresa.Contains(nombreEmpresa));
        if (!string.IsNullOrWhiteSpace(cuil)) query = query.Where(x => x.Cuil != null && x.Cuil.Contains(cuil));
        if (!string.IsNullOrWhiteSpace(email)) query = query.Where(x => x.Email != null && x.Email.Contains(email));
        if (!string.IsNullOrWhiteSpace(telefono)) query = query.Where(x => x.Telefono != null && x.Telefono.Contains(telefono));
        if (!string.IsNullOrWhiteSpace(direccion)) query = query.Where(x => x.Direccion != null && x.Direccion.Contains(direccion));

        if (creadoDesde.HasValue) query = query.Where(x => x.Creado.Date >= creadoDesde.Value.Date);
        if (creadoHasta.HasValue) query = query.Where(x => x.Creado.Date <= creadoHasta.Value.Date);
        if (modDesde.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date >= modDesde.Value.Date);
        if (modHasta.HasValue) query = query.Where(x => x.Modificado.HasValue && x.Modificado.Value.Date <= modHasta.Value.Date);

        var data = await query.OrderBy(x => x.RazonSocial).ToListAsync();

        var cols = new[]
        {
            new ExcelColumn<Empleado>{ Header="Razón Social", Value=x=>x.RazonSocial },
            new ExcelColumn<Empleado>{ Header="Descripción Usuario", Value=x=>x.DescripcionUsuario },
            new ExcelColumn<Empleado>{ Header="Nombre Empresa", Value=x=>x.NombreEmpresa },
            new ExcelColumn<Empleado>{ Header="CUIL", Value=x=>x.Cuil },
            new ExcelColumn<Empleado>{ Header="Email", Value=x=>x.Email },
            new ExcelColumn<Empleado>{ Header="Teléfono", Value=x=>x.Telefono },
            new ExcelColumn<Empleado>{ Header="Dirección", Value=x=>x.Direccion },
            new ExcelColumn<Empleado>{ Header="Creado", Value=x=>x.Creado },
            new ExcelColumn<Empleado>{ Header="Creado por", Value=x=>x.CreadoPor },
            new ExcelColumn<Empleado>{ Header="Modificado", Value=x=>x.Modificado },
            new ExcelColumn<Empleado>{ Header="Modificado por", Value=x=>x.ModificadoPor },
        };

        var bytes = _excel.Export(data, "Empleados", cols);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Empleados_FILTRADO_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
}

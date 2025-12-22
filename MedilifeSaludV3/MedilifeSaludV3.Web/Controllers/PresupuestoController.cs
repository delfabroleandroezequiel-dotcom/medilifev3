using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Models.ViewModels;
using MedilifeSaludV3.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedilifeSaludV3.Web.Controllers;

public class PresupuestoController : Controller
{
    private readonly AppDbContext _db;

    public PresupuestoController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Presupuesto
    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Presupuestos
            .AsNoTracking()
            .Include(p => p.Empresa)
            .Include(p => p.Paciente)
            .Include(p => p.Prestador)
            .Include(p => p.Medico)
            .Include(p => p.Institucion);

        if (!string.IsNullOrWhiteSpace(q))
        {
            //q = q.Trim();
            //query = query.Where(p =>
            //    p.Numero.ToString().Contains(q)
            //    || (p.Paciente != null && (p.Paciente.Apellido.Contains(q) || p.Paciente.Nombre.Contains(q)))
            //    || (p.Empresa != null && p.Empresa.RazonSocial.Contains(q))
            //    || (p.Prestador != null && p.Prestador.Nombre.Contains(q)));
        }

        var list = await query
            .OrderByDescending(p => p.Fecha)
            .ThenByDescending(p => p.Numero)
            .ToListAsync();

        ViewBag.Q = q ?? "";
        return View(list);
    }

    // GET: /Presupuesto/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Presupuestos
            .AsNoTracking()
            .Include(p => p.Empresa)
            .Include(p => p.Paciente)
            .Include(p => p.Prestador)
            .Include(p => p.Medico)
            .Include(p => p.Institucion)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null) return NotFound();

        return View(entity);
    }

    // GET: /Presupuesto/Create
    public async Task<IActionResult> Create()
    {
        var vm = new PresupuestoEditVm
        {
            Fecha = DateTime.Today,
            Numero = await GetNextNumeroAsync(),
            EmpresaId = await GetDefaultEmpresaIdAsync(),
            Items = CreateEmptyItems(5)
        };

        await LoadSelectListsAsync();
        return View(vm);
    }

    // POST: /Presupuesto/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PresupuestoEditVm vm)
    {
        NormalizeItems(vm);

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return View(vm);
        }

        var entity = new Presupuesto
        {
            Numero = vm.Numero,
            Fecha = vm.Fecha.Date,
            EmpresaId = vm.EmpresaId,
            PrestadorId = vm.PrestadorId,
            InstitucionId = vm.InstitucionId,
            PacienteId = vm.PacienteId,
            MedicoId = vm.MedicoId,
            DescripcionPago = vm.DescripcionPago
        };

        foreach (var (itemVm, idx) in vm.Items.Select((x, i) => (x, i)))
        {
            if (string.IsNullOrWhiteSpace(itemVm.Detalle)) continue;
            if (itemVm.Cantidad <= 0) continue;

            entity.Items.Add(new PresupuestoItem
            {
                Detalle = itemVm.Detalle!.Trim(),
                Cantidad = itemVm.Cantidad,
                PrecioUnitario = itemVm.PrecioUnitario,
                Orden = itemVm.Orden == 0 ? idx + 1 : itemVm.Orden
            });
        }

        _db.Presupuestos.Add(entity);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    // GET: /Presupuesto/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Presupuestos
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null) return NotFound();

        var vm = new PresupuestoEditVm
        {
            Id = entity.Id,
            Numero = entity.Numero,
            Fecha = entity.Fecha.Date,
            EmpresaId = entity.EmpresaId,
            PrestadorId = entity.PrestadorId,
            InstitucionId = entity.InstitucionId,
            PacienteId = entity.PacienteId,
            MedicoId = entity.MedicoId,
            DescripcionPago = entity.DescripcionPago,
            Items = entity.Items
                .OrderBy(i => i.Orden)
                .Select(i => new PresupuestoItemVm
                {
                    Id = i.Id,
                    Detalle = i.Detalle,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Orden = i.Orden
                })
                .ToList()
        };

        // Siempre dejamos algunas filas vacías para cargar rápido
        while (vm.Items.Count < 5) vm.Items.Add(new PresupuestoItemVm { Cantidad = 1 });

        await LoadSelectListsAsync();
        return View(vm);
    }

    // POST: /Presupuesto/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PresupuestoEditVm vm)
    {
        if (vm.Id != id) return NotFound();

        NormalizeItems(vm);

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return View(vm);
        }

        var entity = await _db.Presupuestos
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null) return NotFound();

        entity.Numero = vm.Numero;
        entity.Fecha = vm.Fecha.Date;
        entity.EmpresaId = vm.EmpresaId;
        entity.PrestadorId = vm.PrestadorId;
        entity.InstitucionId = vm.InstitucionId;
        entity.PacienteId = vm.PacienteId;
        entity.MedicoId = vm.MedicoId;
        entity.DescripcionPago = vm.DescripcionPago;

        // Reconciliar items (simple, pero efectivo)
        var incoming = vm.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.Detalle) && i.Cantidad > 0)
            .Select((i, idx) => new
            {
                i.Id,
                Detalle = i.Detalle!.Trim(),
                i.Cantidad,
                i.PrecioUnitario,
                Orden = i.Orden == 0 ? idx + 1 : i.Orden
            })
            .ToList();

        // borrar los que no vienen
        var incomingIds = incoming.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();
        var toRemove = entity.Items.Where(x => !incomingIds.Contains(x.Id)).ToList();
        foreach (var rem in toRemove) _db.PresupuestoItems.Remove(rem);

        // actualizar / agregar
        foreach (var inc in incoming)
        {
            if (inc.Id.HasValue)
            {
                var existing = entity.Items.FirstOrDefault(x => x.Id == inc.Id.Value);
                if (existing == null) continue;

                existing.Detalle = inc.Detalle;
                existing.Cantidad = inc.Cantidad;
                existing.PrecioUnitario = inc.PrecioUnitario;
                existing.Orden = inc.Orden;
            }
            else
            {
                entity.Items.Add(new PresupuestoItem
                {
                    Detalle = inc.Detalle,
                    Cantidad = inc.Cantidad,
                    PrecioUnitario = inc.PrecioUnitario,
                    Orden = inc.Orden
                });
            }
        }

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    // GET: /Presupuesto/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Presupuestos
            .AsNoTracking()
            .Include(p => p.Empresa)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null) return NotFound();
        return View(entity);
    }

    // POST: /Presupuesto/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _db.Presupuestos.FindAsync(id);
        if (entity != null)
        {
            _db.Presupuestos.Remove(entity);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Presupuesto/Print/5
    public async Task<IActionResult> Print(int? id)
    {
        if (id == null) return NotFound();

        var entity = await _db.Presupuestos
            .AsNoTracking()
            .Include(p => p.Empresa)
            .Include(p => p.Paciente)
            .Include(p => p.Prestador)
            .Include(p => p.Medico)
            .Include(p => p.Institucion)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null) return NotFound();

        entity.TotalEnLetras = SpanishNumberToWords.PesosToWords(entity.Total);

        // Vista sin layout, pensada para window.print()
        return View(entity);
    }

    private async Task LoadSelectListsAsync()
    {
        ViewBag.Empresas = new SelectList(await _db.Empleados.AsNoTracking().OrderBy(e => e.RazonSocial).ToListAsync(), "Id", "RazonSocial");
        ViewBag.Prestadores = new SelectList(await _db.ObrasSociales.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync(), "Id", "Nombre");
        ViewBag.Instituciones = new SelectList(await _db.Instituciones.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync(), "Id", "Nombre");
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
    }

    private static List<PresupuestoItemVm> CreateEmptyItems(int count)
    {
        var list = new List<PresupuestoItemVm>();
        for (int i = 0; i < count; i++) list.Add(new PresupuestoItemVm { Cantidad = 1, Orden = i + 1 });
        return list;
    }

    private void NormalizeItems(PresupuestoEditVm vm)
    {
        if (vm.Items == null) vm.Items = new List<PresupuestoItemVm>();

        for (int i = 0; i < vm.Items.Count; i++)
        {
            vm.Items[i].Detalle = vm.Items[i].Detalle?.Trim();
            if (vm.Items[i].Orden == 0) vm.Items[i].Orden = i + 1;
        }
    }

    private async Task<int> GetNextNumeroAsync()
    {
        var max = await _db.Presupuestos.AsNoTracking().Select(x => (int?)x.Numero).MaxAsync();
        return (max ?? 0) + 1;
    }

    private async Task<int> GetDefaultEmpresaIdAsync()
    {
        var first = await _db.Empleados.AsNoTracking().OrderBy(x => x.RazonSocial).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        return first ?? 0;
    }
}

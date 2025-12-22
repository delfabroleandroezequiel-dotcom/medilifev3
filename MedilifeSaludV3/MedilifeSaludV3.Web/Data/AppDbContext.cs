using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<ObraSocial> ObrasSociales => Set<ObraSocial>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();

    public DbSet<TipoMaterial> TiposMaterial => Set<TipoMaterial>();
    public DbSet<Modelo> Modelos => Set<Modelo>();
    public DbSet<TipoFactura> TiposFactura => Set<TipoFactura>();
    public DbSet<MarcaStock> Marcas => Set<MarcaStock>();
    public DbSet<Institucion> Instituciones => Set<Institucion>();
    public DbSet<Empleado> Empleados => Set<Empleado>();

    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<PresupuestoItem> PresupuestoItems => Set<PresupuestoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Presupuesto>()
            .HasIndex(x => new { x.EmpresaId, x.Numero })
            .IsUnique();

        modelBuilder.Entity<PresupuestoItem>()
            .HasOne(i => i.Presupuesto)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.PresupuestoId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAudit()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Creado = DateTime.Now;
                entry.Entity.CreadoPor = _currentUser.GetUsername();
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Modificado = DateTime.Now;
                entry.Entity.ModificadoPor = _currentUser.GetUsername();
            }
        }
    }
}
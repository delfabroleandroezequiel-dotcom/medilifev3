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
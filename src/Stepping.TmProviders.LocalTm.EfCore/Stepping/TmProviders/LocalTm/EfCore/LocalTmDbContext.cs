using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Stepping.TmProviders.LocalTm.EfCore;

public class LocalTmDbContext : DbContext
{
    public DbSet<TmTransaction> TmTransactions { get; set; } = null!;

    public LocalTmDbContext(DbContextOptions<LocalTmDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TmTransaction>(builder =>
        {
            builder.ToTable("TmTransactions");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Gid).HasMaxLength(40).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(50).IsRequired();

            builder.HasIndex(x => x.Gid).IsUnique();
            builder.HasIndex(x => new { x.Status, x.NextRetryTime });
            builder.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
        });
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.State.HasFlag(EntityState.Added) || entry.State.HasFlag(EntityState.Modified) || entry.State.HasFlag(EntityState.Deleted))
            {
                UpdateConcurrencyStamp(entry);
            }
        }

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected virtual void UpdateConcurrencyStamp(EntityEntry entry)
    {
        var entity = entry.Entity as TmTransaction;
        if (entity == null)
        {
            return;
        }

        Entry(entity).Property(x => x.ConcurrencyStamp).OriginalValue = entity.ConcurrencyStamp;
        entity.ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }
}

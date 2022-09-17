using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Stepping.DbProviders.EfCore.Tests.Fakes;

public class FakeDbContext : DbContext
{
    public const string ConnectionString = "Data Source=:memory:";

    private SqliteConnection? _connection;

    public virtual DbSet<Book> Books { get; set; } = null!;

    public FakeDbContext(DbContextOptions<FakeDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _connection = new SqliteConnection(ConnectionString);
        _connection.Open();

        optionsBuilder.UseSqlite(_connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(b =>
        {
            b.ToTable("Books");
            b.Property(x => x.Id);
            b.Property(x => x.Name);
            b.HasKey(x => x.Id);
        });
    }
}
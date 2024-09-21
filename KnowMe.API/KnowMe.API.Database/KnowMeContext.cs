using KnowMe.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace KnowMe.API.Database;

public class KnowMeContext : DbContext
{
    public KnowMeContext(DbContextOptions<KnowMeContext> databaseOptions) : base(databaseOptions){ }
    protected KnowMeContext(DbContextOptions contextOptions) : base(contextOptions) { }

    public DbSet<Game> Quotes => Set<Game>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(tr =>
        {
            tr.HasKey(p => p.Id);
            tr.Property(p => p.Id).ValueGeneratedNever();
            tr.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSnakeCaseNamingConvention()
            .ConfigureWarnings(x => x.Log((CoreEventId.ExecutionStrategyRetrying, LogLevel.Warning)));

}


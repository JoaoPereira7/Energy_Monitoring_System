using Energy_Monitoring_System.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Energy_Monitoring_System.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Leitura> Leituras { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Leitura>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MedidorId).IsRequired();
            entity.HasIndex(e => new { e.MedidorId, e.Timestamp });
            
            entity.Property(e => e.Tensao).HasPrecision(18, 4);
            entity.Property(e => e.Corrente).HasPrecision(18, 4);
            entity.Property(e => e.PotenciaAtiva).HasPrecision(18, 4);
            entity.Property(e => e.PotenciaReativa).HasPrecision(18, 4);
            entity.Property(e => e.EnergiaAtivaDireta).HasPrecision(18, 4);
            entity.Property(e => e.EnergiaAtivaReversa).HasPrecision(18, 4);
            entity.Property(e => e.FatorPotencia).HasPrecision(18, 6);
            entity.Property(e => e.Frequencia).HasPrecision(18, 4);
        });
    }
}
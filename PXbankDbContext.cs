using Microsoft.EntityFrameworkCore;
using PXbankAPI.Domain.Entities;

namespace PXbankAPI.Infrastructure.Context
{
    public class PXbankDbContext : DbContext
    {
        public PXbankDbContext(DbContextOptions<PXbankDbContext> options) : base(options) { }

        public DbSet<Motorista> Motoristas => Set<Motorista>();
        public DbSet<Transacao> Transacoes => Set<Transacao>();
        public DbSet<Conta> Contas => Set<Conta>();
        public DbSet<MovimentoConta> MovimentosConta => Set<MovimentoConta>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplica todas as classes IEntityTypeConfiguration deste assembly.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PXbankDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PXbankAPI.Domain.Entities;

namespace PXbankAPI.Infrastructure.Context.Configurations
{
    public class ContaConfiguration : IEntityTypeConfiguration<Conta>
    {
        public void Configure(EntityTypeBuilder<Conta> builder)
        {
            builder.ToTable("Contas");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Titular).IsRequired().HasMaxLength(150);
            builder.Property(c => c.Documento).IsRequired().HasMaxLength(20);
            builder.Property(c => c.Saldo).HasPrecision(18, 2).IsRequired();
            builder.Property(c => c.CriadaEm).IsRequired();
            builder.Property(c => c.Ativa).IsRequired();

            builder.HasIndex(c => c.MotoristaId).IsUnique();

            builder.HasMany(c => c.Movimentos)
                   .WithOne()
                   .HasForeignKey(m => m.ContaId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata
                   .FindNavigation(nameof(Conta.Movimentos))!
                   .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
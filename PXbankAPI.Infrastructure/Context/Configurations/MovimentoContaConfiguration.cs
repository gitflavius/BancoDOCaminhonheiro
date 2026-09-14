using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PXbankAPI.Domain.Entities;

namespace PXbankAPI.Infrastructure.Context.Configurations
{
    public class MovimentoContaConfiguration : IEntityTypeConfiguration<MovimentoConta>
    {
        public void Configure(EntityTypeBuilder<MovimentoConta> builder)
        {
            builder.ToTable("MovimentosConta");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Valor).HasPrecision(18, 2).IsRequired();
            builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(m => m.Descricao).IsRequired().HasMaxLength(300);
            builder.Property(m => m.OcorridoEm).IsRequired();

            builder.HasIndex(m => new { m.ContaId, m.OcorridoEm });

            builder.HasOne<Transacao>()
                   .WithMany()
                   .HasForeignKey(m => m.TransacaoId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
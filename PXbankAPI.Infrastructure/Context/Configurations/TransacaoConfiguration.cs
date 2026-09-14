using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PXbankAPI.Domain.Entities;

namespace PXbankAPI.Infrastructure.Context.Configurations
{
    public class TransacaoConfiguration : IEntityTypeConfiguration<Transacao>
    {
        public void Configure(EntityTypeBuilder<Transacao> builder)
        {
            builder.ToTable("Transacoes");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Valor).HasPrecision(18, 2).IsRequired();
            builder.Property(t => t.ComissaoCalculada).HasPrecision(18, 2).IsRequired();

            builder.Property(t => t.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.Property(t => t.Descricao).IsRequired().HasMaxLength(300);
            builder.Property(t => t.Origem).HasMaxLength(150);
            builder.Property(t => t.Destino).HasMaxLength(150);
            builder.Property(t => t.ReferenciaExterna).HasMaxLength(100);
            builder.Property(t => t.DataCriacao).IsRequired();

            builder.HasIndex(t => t.MotoristaId);
            builder.HasIndex(t => t.Status);
            builder.HasIndex(t => t.DataCriacao);
        }
    }
}
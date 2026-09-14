using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PXbankAPI.Domain.Entities;

namespace PXbankAPI.Infrastructure.Context.Configurations
{
    public class MotoristaConfiguration : IEntityTypeConfiguration<Motorista>
    {
        public void Configure(EntityTypeBuilder<Motorista> builder)
        {
            builder.ToTable("Motoristas");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Nome).IsRequired().HasMaxLength(150);
            builder.Property(m => m.Cpf).IsRequired().HasMaxLength(11).IsFixedLength();
            builder.Property(m => m.Email).HasMaxLength(150);
            builder.Property(m => m.Telefone).HasMaxLength(20);
            builder.Property(m => m.Placa).HasMaxLength(10);
            builder.Property(m => m.Ativo).IsRequired();
            builder.Property(m => m.DataCriacao).IsRequired();

            // CPF nao pode repetir. Restricao no banco, nao so na validacao da
            // entidade: validacao na aplicacao nao impede duas requisicoes simultaneas.
            builder.HasIndex(m => m.Cpf).IsUnique();

            builder.HasMany(m => m.Transacoes)
                   .WithOne(t => t.Motorista)
                   .HasForeignKey(t => t.MotoristaId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1 motorista : 1 conta
            builder.HasOne(m => m.Conta)
                   .WithOne()
                   .HasForeignKey<Conta>(c => c.MotoristaId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
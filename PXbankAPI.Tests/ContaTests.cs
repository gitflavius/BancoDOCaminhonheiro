using System;
using System.Collections.Generic;
using System.Text;
using PXbankAPI.Domain.Entities;
using PXbankAPI.Domain.Enums;
using PXbankAPI.Domain.Exceptions;


namespace PXbankAPI.Tests
{
    public class ContaTests
    {
        private const int MotoristaId = 42;

        private static Conta NovaConta(int motoristaId = MotoristaId)
            => new(motoristaId, "Flavio Rodrigues", "12345678901");

        private static Transacao FreteConfirmado(
            int motoristaId = MotoristaId,
            decimal valor = 1000m,
            TipoFrete tipo = TipoFrete.Rodoviario)
        {
            var frete = new Transacao
            {
                Id = 7,
                MotoristaId = motoristaId,
                Valor = valor,
                Tipo = tipo,
                Descricao = "Carga de graos",
                Origem = "Belo Horizonte",
                Destino = "Uberlandia"
            };

            frete.CalcularComissao();
            frete.MarcarComoProcessada();
            return frete;
        }

        // ---------- Estado inicial ----------

        [Fact]
        public void NovaConta_DeveNascerComSaldoZeroEAtiva()
        {
            var conta = NovaConta();

            Assert.Equal(0m, conta.Saldo);
            Assert.True(conta.Ativa);
            Assert.Empty(conta.Movimentos);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void NovaConta_ComMotoristaInvalido_DeveLancarExcecao(int motoristaId)
        {
            Assert.Throws<ArgumentException>(() => NovaConta(motoristaId));
        }

        // ---------- Credito e debito ----------

        [Fact]
        public void Creditar_DeveAumentarSaldoERegistrarMovimento()
        {
            var conta = NovaConta();

            conta.Creditar(1500.50m, "Adiantamento");

            Assert.Equal(1500.50m, conta.Saldo);
            Assert.Single(conta.Movimentos);
            Assert.Equal(TipoMovimento.Credito, conta.Movimentos.First().Tipo);
        }

        [Fact]
        public void Debitar_ComSaldoSuficiente_DeveDiminuirSaldo()
        {
            var conta = NovaConta();
            conta.Creditar(1000m);

            conta.Debitar(400m, "Diesel");

            Assert.Equal(600m, conta.Saldo);
            Assert.Equal(2, conta.Movimentos.Count);
        }

        [Fact]
        public void Debitar_AcimaDoSaldo_DeveFalharSemAlterarNada()
        {
            var conta = NovaConta();
            conta.Creditar(100m);

            Assert.Throws<SaldoInsuficienteException>(() => conta.Debitar(100.01m));

            Assert.Equal(100m, conta.Saldo);
            Assert.Single(conta.Movimentos);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Creditar_ComValorNaoPositivo_DeveLancarExcecao(decimal valor)
        {
            Assert.Throws<ArgumentException>(() => NovaConta().Creditar(valor));
        }

        [Fact]
        public void ContaInativa_NaoAceitaMovimentacao()
        {
            var conta = NovaConta();
            conta.Creditar(500m);
            conta.Desativar();

            Assert.Throws<ContaInativaException>(() => conta.Creditar(10m));
            Assert.Throws<ContaInativaException>(() => conta.Debitar(10m));
        }

        [Fact]
        public void SomaDeCentavos_NaoPodePerderPrecisao()
        {
            var conta = NovaConta();

            for (var i = 0; i < 10; i++)
                conta.Creditar(0.10m);

            Assert.Equal(1.00m, conta.Saldo);
        }

        // ---------- ReceberFrete: a regra mais importante ----------

        [Fact]
        public void ReceberFrete_DeveCreditarOValorLiquidoENaoOBruto()
        {
            var conta = NovaConta();
            var frete = FreteConfirmado(valor: 1000m, tipo: TipoFrete.Rodoviario);

            conta.ReceberFrete(frete);

            // 1000 de frete - 50 de comissao (5%) = 950 liquidos
            Assert.Equal(950m, conta.Saldo);
        }

        [Fact]
        public void ReceberFrete_DeveLigarOMovimentoAoFreteQueOOriginou()
        {
            var conta = NovaConta();
            var frete = FreteConfirmado();

            conta.ReceberFrete(frete);

            Assert.Equal(frete.Id, conta.Movimentos.First().TransacaoId);
        }

        [Fact]
        public void ReceberFrete_ComFretePendente_DeveRecusar()
        {
            var conta = NovaConta();
            var frete = new Transacao
            {
                Id = 8,
                MotoristaId = MotoristaId,
                Valor = 1000m,
                Tipo = TipoFrete.Rodoviario,
                Descricao = "Carga de graos"
            };

            Assert.Throws<InvalidOperationException>(() => conta.ReceberFrete(frete));
            Assert.Equal(0m, conta.Saldo);
        }

        [Fact]
        public void ReceberFrete_DeOutroMotorista_DeveRecusar()
        {
            var conta = NovaConta(motoristaId: 42);
            var freteDeOutro = FreteConfirmado(motoristaId: 99);

            Assert.Throws<InvalidOperationException>(() => conta.ReceberFrete(freteDeOutro));
            Assert.Equal(0m, conta.Saldo);
        }

        [Fact]
        public void ReceberFrete_Nulo_DeveLancarExcecao()
        {
            Assert.Throws<ArgumentNullException>(() => NovaConta().ReceberFrete(null!));
        }
    }
}

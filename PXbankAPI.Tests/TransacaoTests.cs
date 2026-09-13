using System;
using System.Collections.Generic;
using System.Text;
using PXbankAPI.Domain.Entities;
using PXbankAPI.Domain.Enums;

namespace PXbankAPI.Tests;

public class TransacaoTests
{
    private static Transacao NovoFrete(
        TipoFrete tipo = TipoFrete.Rodoviario,
        decimal valor = 1000m) => new()
        {
            Id = 1,
            MotoristaId = 42,
            Valor = valor,
            Tipo = tipo,
            Descricao = "Carga de graos",
            Origem = "Belo Horizonte",
            Destino = "Uberlandia",
        };

    //----------Comissao ---------------
    [Theory]
    [InlineData(TipoFrete.Rodoviario, 1000, 50)] //5%
    [InlineData(TipoFrete.Aereo, 1000, 200)] //20%
    [InlineData(TipoFrete.Maritimo , 1000, 80)] //8%
    [InlineData(TipoFrete.Ferroviario, 1000, 60)] //6%
    public void CalcularComissao_DeveAplicarPercentualDoTipoDeFrete(
        TipoFrete tipo ,decimal valor , decimal comissaoEsperada)
    {
        var frete = NovoFrete(tipo, valor);
        var comissao = frete.CalcularComissao();

        Assert.Equal(comissaoEsperada, comissao);
    }
    [Fact]
    public void CalcularComissao_DeveGravarNaPropriedade()
    {
        var frete = NovoFrete(TipoFrete.Aereo, 500m);
        frete.CalcularComissao();
        Assert.Equal(100m, frete.ComissaoCalculada);
    }
    [Fact]
    public void CalcularComissao_ComCentavos_NaoPodePerderPrecisao()
    {
        var frete = NovoFrete(TipoFrete.Rodoviario, 1234.56m);

        var comissao = frete.CalcularComissao();

        Assert.Equal(61.728m, comissao);
    }
    //------------------ Status --------------
    [Fact]
    public void NovaTransacao_DeveNascerPendente()
    {
        Assert.Equal(StatusTransacao.Pendente, NovoFrete().Status);
    }
    [Fact]
    public void MarcarComoProcessada_DeveConfirmarEGravarDataProcessamento()
    {
        var frete = NovoFrete();

        frete.MarcarComoProcessada();

        Assert.Equal(StatusTransacao.Confirmada, frete.Status);
        Assert.NotNull(frete.DataProcessamento);
    }
    [Fact]
    public void TransacaoJaConfirmada_NaoPodeSerRecusada()
    {
        var frete = NovoFrete();
        frete.MarcarComoProcessada();

        Assert.Throws<InvalidOperationException>(
            () => frete.MarcarComoRecusada("documentacao irregular"));
    }
    [Fact]
    public void TransacaoJaRecusada_NaoPodeSerConfirmada()
    {
        var frete = NovoFrete();
        frete.MarcarComoRecusada("Carga Cancelada");

        Assert.Throws<InvalidOperationException>(() => frete.MarcarComoProcessada());
    }
    [Fact]
    public void MarcarComoRecusada_SemMotivo_DeveLancarExcecao()
    {
        var frete = NovoFrete();

        frete.MarcarComoRecusada("Carga cancelada");

        Assert.Contains("Carga cancelada", frete.Descricao);
        Assert.Contains("Carga de graos", frete.Descricao);
    }
    // -------------------- Validacao --------------
    [Fact]
    public void Validar_ComDadosCorretos_DeveRetornarTrue()
    {
        var valido = NovoFrete().Validar(out var erro);
        Assert.True(valido);
        Assert.Empty(erro);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_ComMotoristaInvalido_DeveRetornarFalse(int motoristaId)
    {
        var frete = NovoFrete();
        frete.MotoristaId = motoristaId;

        Assert.False(frete.Validar(out var erro));
        Assert.Contains("Motorista", erro);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Validar_ComValorNaoPositivo_DeveRetornarFalse(decimal valor)
    {
        var frete = NovoFrete();
        frete.Valor = valor;

        Assert.False(frete.Validar(out var erro));
        Assert.Contains("Valor", erro);
    }
    [Fact]
    public void Validar_SemDescricao_DeveRetornarFalse()
    {
        var frete = NovoFrete();
        frete.Descricao = "   ";

        Assert.False(frete.Validar(out var erro));
        Assert.Contains("Descricao", erro, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public void Validar_ComTipoDeFreteInexistente_DeveRetornarFalse()
    {
        var frete = NovoFrete();
        frete.Tipo = (TipoFrete)999;

        Assert.False(frete.Validar(out var erro));
        Assert.Contains("frete", erro, StringComparison.OrdinalIgnoreCase);
    }
}

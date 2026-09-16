using PXbankAPI.Domain.Enums;

namespace PXbankAPI.Application.DTOs
{
    /// <summary>
    /// ENTRADA: registrar um frete novo. Status e comissao ficam de fora:
    /// todo frete nasce Pendente e a comissao e calculada pelo dominio.
    /// </summary>
    public class CriarFreteDto
    {
        public int MotoristaId { get; set; }
        public decimal Valor { get; set; }
        public TipoFrete Tipo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string ReferenciaExterna { get; set; } = string.Empty;
    }

    /// <summary>
    /// ENTRADA: recusar um frete pendente. Exige motivo.
    /// Confirmar nao precisa de DTO - basta o id na rota.
    /// </summary>
    public class RecusarFreteDto
    {
        public int Id { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }

    /// <summary>
    /// SAIDA: o frete como a API devolve, ja com a comissao calculada
    /// e o valor liquido que o motorista recebe.
    /// </summary>
    public class FreteResponseDto
    {
        public int Id { get; set; }
        public int MotoristaId { get; set; }
        public string NomeMotorista { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal ComissaoCalculada { get; set; }
        public decimal ValorLiquido { get; set; }
        public TipoFrete Tipo { get; set; }
        public StatusTransacao Status { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataProcessamento { get; set; }
    }

    /// <summary>
    /// SAIDA: um movimento no extrato da conta.
    /// </summary>
    public class MovimentoResponseDto
    {
        public Guid Id { get; set; }
        public TipoMovimento Tipo { get; set; }
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public DateTime OcorridoEm { get; set; }
        public int? TransacaoId { get; set; }
    }

    /// <summary>
    /// SAIDA: extrato de um periodo, com saldo atual e os movimentos.
    /// </summary>
    public class ExtratoResponseDto
    {
        public Guid ContaId { get; set; }
        public string Titular { get; set; } = string.Empty;
        public decimal SaldoAtual { get; set; }
        public DateTime PeriodoInicio { get; set; }
        public DateTime PeriodoFim { get; set; }
        public List<MovimentoResponseDto> Movimentos { get; set; } = new();
    }

    /// <summary>
    /// SAIDA: relatorio consolidado de um periodo.
    /// </summary>
    public class RelatorioFretesDto
    {
        public int TotalFretes { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal ComissaoTotal { get; set; }
        public Dictionary<string, int> FretesPorTipo { get; set; } = new();
        public Dictionary<string, int> FretesPorStatus { get; set; } = new();
        public DateTime DataGeracao { get; set; }
    }
}
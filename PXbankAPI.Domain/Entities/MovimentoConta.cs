using PXbankAPI.Domain.Enums;

namespace PXbankAPI.Domain.Entities
{
    /// <summary>
    /// Entrada ou saida de dinheiro na conta de um motorista.
    /// Nao confundir com Transacao, que representa um frete.
    /// Um frete confirmado gera um MovimentoConta de credito.
    /// </summary>
    public class MovimentoConta
    {
        public Guid Id { get; private set; }
        public Guid ContaId { get; private set; }
        public TipoMovimento Tipo { get; private set; }
        public decimal Valor { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public DateTime OcorridoEm { get; private set; }

        // Opcional: liga o movimento ao frete que o originou.
        public int? TransacaoId { get; private set; }

        // Exigido pelo EF Core para materializar do banco.
        private MovimentoConta() { }
 
        // internal: so a propria Conta cria movimento. Ninguem cria um solto.
        internal MovimentoConta(Guid contaId, TipoMovimento tipo, decimal valor,
                                string? descricao, int? transacaoId = null)
        {
            Id = Guid.NewGuid();
            ContaId = contaId;
            Tipo = tipo;
            Valor = valor;
            Descricao = string.IsNullOrWhiteSpace(descricao) ? tipo.ToString() : descricao.Trim();
            OcorridoEm = DateTime.UtcNow;
            TransacaoId = transacaoId;
        }
    }
}
using PXbankAPI.Domain.Enums;

namespace PXbankAPI.Domain.Entities
{
    public class Transacao
    {
        public int Id { get; set; }
        public int MotoristaId { get; set; }
        public decimal Valor { get; set; }
        public TipoFrete Tipo { get; set; }
        public StatusTransacao Status { get; set; } = StatusTransacao.Pendente;
        public string Descricao { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public decimal ComissaoCalculada { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataProcessamento { get; set; }
        public string ReferenciaExterna { get; set; } = string.Empty;

        // Nullable: ao buscar uma transacao sem Include, o EF deixa a navegacao nula.
        public Motorista? Motorista { get; set; }

        public decimal CalcularComissao()
        {
            ComissaoCalculada = Tipo switch
            {
                TipoFrete.Rodoviario => Valor * 0.05m,
                TipoFrete.Aereo => Valor * 0.20m,
                TipoFrete.Maritimo => Valor * 0.08m,
                TipoFrete.Ferroviario => Valor * 0.06m,
                _ => 0m
            };

            return ComissaoCalculada;
        }

        public bool Validar(out string erro)
        {
            erro = string.Empty;

            if (MotoristaId <= 0)
            {
                erro = "Motorista invalido";
                return false;
            }

            if (Valor <= 0)
            {
                erro = "Valor deve ser maior que zero";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Descricao))
            {
                erro = "A descricao e obrigatoria";
                return false;
            }

            if (!Enum.IsDefined(typeof(TipoFrete), Tipo))
            {
                erro = "Tipo de frete invalido";
                return false;
            }

            return true;
        }

        public void MarcarComoProcessada()
        {
            GarantirQueEstaPendente();

            Status = StatusTransacao.Confirmada;
            DataProcessamento = DateTime.UtcNow;
        }

        public void MarcarComoRecusada(string motivo)
        {
            GarantirQueEstaPendente();

            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("O motivo da recusa e obrigatorio.", nameof(motivo));

            Status = StatusTransacao.Recusada;
            DataProcessamento = DateTime.UtcNow;
            Descricao = $"{Descricao} | Recusado: {motivo.Trim()}";
        }

        // Uma transacao ja confirmada ou recusada nao pode mudar de estado.
        private void GarantirQueEstaPendente()
        {
            if (Status != StatusTransacao.Pendente)
                throw new InvalidOperationException(
                    $"A transacao {Id} esta {Status} e nao pode mais ser alterada.");
        }
    }
}
using PXbankAPI.Domain.Enums;
using PXbankAPI.Domain.Exceptions;

namespace PXbankAPI.Domain.Entities
{
    public class Conta
    {
        public Guid Id { get; private set; }
        public int MotoristaId { get; private set; }
        public string Titular { get; private set; } = string.Empty;
        public string Documento { get; private set; } = string.Empty;
        public decimal Saldo { get; private set; }
        public DateTime CriadaEm { get; private set; }
        public bool Ativa { get; private set; }

        private readonly List<MovimentoConta> _movimentos = new();
        public IReadOnlyCollection<MovimentoConta> Movimentos => _movimentos.AsReadOnly();

        private Conta() { }

        public Conta(int motoristaId, string titular, string documento)
        {
            if (motoristaId <= 0)
                throw new ArgumentException("Motorista invalido.", nameof(motoristaId));

            if (string.IsNullOrWhiteSpace(titular))
                throw new ArgumentException("O titular e obrigatorio.", nameof(titular));

            if (string.IsNullOrWhiteSpace(documento))
                throw new ArgumentException("O documento e obrigatorio.", nameof(documento));

            Id = Guid.NewGuid();
            MotoristaId = motoristaId;
            Titular = titular.Trim();
            Documento = documento.Trim();
            Saldo = 0m;
            CriadaEm = DateTime.UtcNow;
            Ativa = true;
        }

        public MovimentoConta Creditar(decimal valor, string? descricao = null, int? transacaoId = null)
        {
            GarantirAtiva();
            GarantirValorPositivo(valor);

            Saldo += valor;
            var movimento = new MovimentoConta(Id, TipoMovimento.Credito, valor, descricao, transacaoId);
            _movimentos.Add(movimento);
            return movimento;
        }

        public MovimentoConta Debitar(decimal valor, string? descricao = null, int? transacaoId = null)
        {
            GarantirAtiva();
            GarantirValorPositivo(valor);

            if (valor > Saldo)
                throw new SaldoInsuficienteException(Id, Saldo, valor);

            Saldo -= valor;
            var movimento = new MovimentoConta(Id, TipoMovimento.Debito, valor, descricao, transacaoId);
            _movimentos.Add(movimento);
            return movimento;
        }

        /// <summary>
        /// Credita na conta o valor liquido de um frete confirmado
        /// (valor do frete menos a comissao).
        /// </summary>
        public MovimentoConta ReceberFrete(Transacao frete)
        {
            ArgumentNullException.ThrowIfNull(frete);

            if (frete.MotoristaId != MotoristaId)
                throw new InvalidOperationException(
                    $"O frete {frete.Id} nao pertence ao motorista desta conta.");

            if (frete.Status != StatusTransacao.Confirmada)
                throw new InvalidOperationException(
                    $"O frete {frete.Id} esta {frete.Status} e ainda nao pode ser creditado.");

            var liquido = frete.Valor - frete.ComissaoCalculada;

            return Creditar(liquido, $"Frete {frete.Origem} - {frete.Destino}", frete.Id);
        }

        public void Desativar() => Ativa = false;

        private void GarantirAtiva()
        {
            if (!Ativa) throw new ContaInativaException(Id);
        }

        private static void GarantirValorPositivo(decimal valor)
        {
            if (valor <= 0)
                throw new ArgumentException("O valor deve ser maior que zero.", nameof(valor));
        }
    }
}
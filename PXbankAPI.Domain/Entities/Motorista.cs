using System;
using System.Collections.Generic;
using System.Text;

namespace PXbankAPI.Domain.Entities
{
    public class Motorista
    {
        public int Id { get; set; }
        public string Nome {  get; set; }
        public string Cpf { get; set; }
        public string Email {  get; set; }
        public string Telefone { get; set; }
        public string Placa {  get; set; }
        public decimal SaldoDisponivel { get; set; }
        public bool Ativo {  get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public bool Validar(out string erro)
        {
            erro = string.Empty;
            if (string.IsNullOrWhiteSpace(Nome))
            {
                erro = "Nome é obrigatorio";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Cpf) ||  Cpf.Length != 11)
            {
                erro = "CPF Invalido";
                return false;
            }
            if  (SaldoDisponivel < 0)
            {
                erro = "Saldo não pode ser negativo";
                return false;
            }

            return true;
        }

        public bool DebitarSaldo(decimal valor)
        {
            if (valor > SaldoDisponivel)
                return false;
            SaldoDisponivel -= valor;
            DataAtualizacao = DateTime.UtcNow;
            return true;
        }

        public void CreditarSaldo(decimal valor)
        {
            SaldoDisponivel += valor;
            DataAtualizacao = DateTime.UtcNow;
        }

    }
}

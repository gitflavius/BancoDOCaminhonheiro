using System;
using System.Collections.Generic;
using System.Text;

namespace PXbankAPI.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string mensagem) : base (mensagem) { }
    }
    public sealed class SaldoInsuficienteException : DomainException
    {
        public SaldoInsuficienteException(Guid contaId, decimal saldo, decimal solicitado)
            : base ($"Saldo insuficiente na conta{contaId}. Saldo: {saldo:C},solicitado:{solicitado:C}") { }
    }
    public sealed class ContaInativaException : DomainException
    {
        public ContaInativaException(Guid contaId)
            : base($"A conta {contaId} está inativa e não aceita movimentação.") { }
    }
}

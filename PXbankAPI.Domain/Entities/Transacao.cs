using System;
using System.Collections.Generic;
using System.Text;
using PXbankAPI.Domain.Enums;

namespace PXbankAPI.Domain.Entities
{
    public class Transacao
    {
        public int Id { get; set; }
        public string motoristaId { get; set; }
        public decimal Valor {  get; set; }       
        public TipoFrete tipoFrete { get; set; }
        public StatusTransacao Status {  get; set; }
        public string Descricao {  get; set; }
        public string Origem { get; set; }
        public string Destino {  get; set; }
        public decimal ComissaoCalculada {  get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataProcessamento {  get; set; }
        public string ReferenciaExterna {  get; set; }
        public Motorista Motorista { get; set; }
        public decimal CalcularComissao()
        {
            return TipoFrete switch
            {
                TipoFrete.Rodoviario => Valor * 0.05m,
                TipoFrete.Aereo => Valor * 0.20m,
                TipoFrete.Maritimo => Valor * 0.08m,
                TipoFrete.Ferroviario => Valor * 0.06m
                _ => 0
            };
        }

        public bool Validar(out string erro)
        {
            erro = string.Empty;
            if (motoristaId <= 0)
            {
                erro = "Motorista inválido";
                return false;
            }
            if (Valor <= 0)
            {
                erro = "Valor deve ser maior que zero";
                return false;
            }
            if(string.IsNullOrWhiteSpace(Descricao))
            {
                erro = "A Descrição é Obrigatoria";
                return false;
            }
            if (Enum.IsDefined(typeof(TipoFrete), tipoFrete) == false)
            {
                erro = "Tipo de frete invalido";
                return false;
            }
            return true;
        }
        public void MarcarComoProcessada()
        {
            Status = StatusTransacao.Confirmada;
            DataProcessamento = DateTime.UtcNow;
        }

        public void MarcarComoRecusada(string motivo) 
        {
            Status = StatusTransacao.Recusada;
            Descricao = $"{Descricao}|Recusado: {motivo}";
        }
    }
}

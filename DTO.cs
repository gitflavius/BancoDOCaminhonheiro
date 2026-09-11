using System;
using System.Collections.Generics;
using PXBank.Domain.Enums;

namespace PXBankAPI.Application.DTOs
{ 

	public class CriarMotoristaDto
	{

		public string Nome {  get; set; }
		public string Cpf { get; set; }
		public string Email { get; set; }
		public string Telefone { get; set; }
		public string Placa { get; set; }
	}

	public class AtualizarMotoristaDto
	{
		public int Id {  get; set; }
		public string Nome { get; set; }
		public string Cpf { get; set; }
		public string Email { get; set; }
		public string Telefone { get; set; }
		public string Placa { get; set; }
		public decimal SaldoDisponivel {  get; set; }
		public bool Ativo {  get; set; }
		public DateTime DataCriacao {  get; set; }
		public int TotalTransacoes {  get; set; }
	}

	public class CriarTransacaoDto
	{
		public int Id { get; set; }
		public StatusTransacao Status { get; set; }
		public string Descricao { get; set; }
	}

	public class TransacaoResponseDto
	{
		public int Id { get; set; }
		public int MotoristId { get; set; }
		public string NomeMotorista {  get; set; }
		public decimal Valor { get; set; }
		public TipoFrete {  get; set; }
		public StatusTransacao Status { get; set; }
		public string Descricao { get; set; }
		public string Origem {  get; set; }
		public string Destino { get; set; }
		public decimal ComissaoCalculada { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataProcessamento { get; set; }

    }

	public class RelatorioPorMotoristDto
	{
		public int MotoristId { get; set; }
		public string NomeMotorista { get; set; }
		public int TotalTransacoes { get; set; }
		public decimal ValorTotal { get; set; }
		public decimal SaldoDisponivel { get; set; }
		public DateTime DataGeracao { get; set; }
	}

	public class RelatorioTransacoesDto
	{
		public int TotalTransacoes { get; set; }
		public decimal ValorTotal { get; set; }
		public decimal ComissaoTotal { get; set; }
		public Dictionary<TipoFrete, int> TransacoesPorTipo { get; set; }
		public Dictionary<StatusTransacao, int> TransacoesPorStatus { get; set; }
		public DateTime DataGeracao { get; set; }
	}

	public class ApiResponse<T>
	{
		public bool Sucesso {  get; set; }
		public string Mensagem {  get; set; }
		public T Dados { get; set; }
		public List<string> Erros { get; set; } = new();

		public static ApiResponse<T> ok(T dados, string mensagem= "Sucesso")
		{
			return new ApiResponse<T>
			{
				Sucesso = true,
				Mensagem = mensagem,
				Dados = dados,
			};
		}

		public static ApiResponse<T> Erro(string mensagem, List<string> erros = null)
		{
			return new ApiResponse<T>
			{
				Sucesso = false,
				Mensagem = mensagem,
				Erros = erros ?? new List<string> { mensagem }
			};
		}

	}

}

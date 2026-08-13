using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PXbankAPI.Domain.Entities;
using PXbankAPI.Domain.Enums;


namespace PXbankAPI.Domain.Interfaces
{
    public interface IRepositorio<T> where T : class
    {
        Task<T> ObterPorId(int id);
        Task<List<T>> ObterTodos();
        Task<List<T>> ObterPor(Expression<Func<T, bool>> filtro);
        Task<T> ObterUmPor(Expression<Func<T, bool>> filtro);
        Task Adicionar(T entidade);
        Task Atualizar(T entidade);
        Task Deletar(int id);
        Task Salvar();

    }
    public interface IRepositorioMotorista: IRepositorio<Motorista>
    {
        Task<Motorista> ObterPorCpf(string cpf);
        Task<Motorista> ObterPorEmail(string email);
        Task<List<Motorista>> ObterAtivos();
    }

    public interface IRepositorioTransacao : IRepositorio<Transacao>
    {
        Task<List<Transacao>> ObterPorMotorista(int motoristId);
        Task<List<Transacao>> ObterPorStatus(StatusTransacao status);
        Task<List<Transacao>> ObterPorPeriodo(DateTime dataInicio, DateTime dataFim);
        Task<decimal> ObterTotalPorMotorista(int motoristId);
        Task<decimal> ObterTotalComissaoPendente(int motoristId);

    }
    public interface IRepositorioAuditoria
    {
        Task RegistrarAcao(string acao, string usuario, string detalhes);
        Task<List<LogAuditoria>> ObterLogs(int ultimosDias = 30);
    }
    public class LogAuditoria 
    {
        public string Id { get; set;  } = Guid.NewGuid().ToString();
        public string Acao { get; set; }
        public string Usuario { get; set; }
        public string Detalhes {  get; set; }
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
        public string Ip {  get; set; }

    }
}

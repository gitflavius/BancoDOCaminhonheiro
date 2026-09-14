using Microsoft.EntityFrameworkCore;
using PXbankAPI.Domain.Entities;
using PXbankAPI.Domain.Enums;
using PXbankAPI.Domain.Interfaces;
using PXbankAPI.Infrastructure.Context;

namespace PXbankAPI.Infrastructure.Repositories
{
    public class RepositorioTransacao : RepositorioBase<Transacao>, IRepositorioTransacao
    {
        public RepositorioTransacao(PXbankDbContext context) : base(context) { }

        public async Task<List<Transacao>> ObterPorMotorista(int motoristId)
            => await _dbSet.AsNoTracking()
                           .Where(t => t.MotoristaId == motoristId)
                           .OrderByDescending(t => t.DataCriacao)
                           .ToListAsync();

        public async Task<List<Transacao>> ObterPorStatus(StatusTransacao status)
            => await _dbSet.AsNoTracking()
                           .Where(t => t.Status == status)
                           .OrderByDescending(t => t.DataCriacao)
                           .ToListAsync();

        public async Task<List<Transacao>> ObterPorPeriodo(DateTime dataInicio, DateTime dataFim)
            => await _dbSet.AsNoTracking()
                           .Where(t => t.DataCriacao >= dataInicio && t.DataCriacao <= dataFim)
                           .OrderByDescending(t => t.DataCriacao)
                           .ToListAsync();

        public async Task<decimal> ObterTotalPorMotorista(int motoristId)
            => await _dbSet.Where(t => t.MotoristaId == motoristId
                                    && t.Status == StatusTransacao.Confirmada)
                           .SumAsync(t => t.Valor);

        public async Task<decimal> ObterTotalComissaoPendente(int motoristId)
            => await _dbSet.Where(t => t.MotoristaId == motoristId
                                    && t.Status == StatusTransacao.Pendente)
                           .SumAsync(t => t.ComissaoCalculada);
    }
}
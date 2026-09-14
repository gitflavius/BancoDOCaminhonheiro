using Microsoft.EntityFrameworkCore;
using PXbankAPI.Domain.Entities;
using PXbankAPI.Domain.Interfaces;
using PXbankAPI.Infrastructure.Context;

namespace PXbankAPI.Infrastructure.Repositories
{
    public class ContaRepository : IContaRepository
    {
        private readonly PXbankDbContext _context;

        public ContaRepository(PXbankDbContext context) => _context = context;

        public async Task<Conta?> ObterPorId(Guid id)
            => await _context.Contas
                             .Include(c => c.Movimentos)
                             .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Conta?> ObterPorMotorista(int motoristaId)
            => await _context.Contas
                             .FirstOrDefaultAsync(c => c.MotoristaId == motoristaId);

        public async Task<Conta?> ObterPorDocumento(string documento)
            => await _context.Contas
                             .FirstOrDefaultAsync(c => c.Documento == documento);

        public async Task<bool> ExisteParaMotorista(int motoristaId)
            => await _context.Contas.AnyAsync(c => c.MotoristaId == motoristaId);

        public async Task Adicionar(Conta conta)
            => await _context.Contas.AddAsync(conta);

        public async Task<IReadOnlyList<MovimentoConta>> ObterExtrato(
            Guid contaId, DateTime inicio, DateTime fim)
            => await _context.MovimentosConta
                             .AsNoTracking()
                             .Where(m => m.ContaId == contaId
                                      && m.OcorridoEm >= inicio
                                      && m.OcorridoEm <= fim)
                             .OrderByDescending(m => m.OcorridoEm)
                             .ToListAsync();

        public async Task Salvar() => await _context.SaveChangesAsync();
    }
}
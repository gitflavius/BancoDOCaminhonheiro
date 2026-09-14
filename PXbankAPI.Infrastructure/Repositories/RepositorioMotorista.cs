using Microsoft.EntityFrameworkCore;
using PXbankAPI.Domain.Entities;
using PXbankAPI.Domain.Interfaces;
using PXbankAPI.Infrastructure.Context;

namespace PXbankAPI.Infrastructure.Repositories
{
    public class RepositorioMotorista : RepositorioBase<Motorista>, IRepositorioMotorista
    {
        public RepositorioMotorista(PXbankDbContext context) : base(context) { }

        public async Task<Motorista> ObterPorCpf(string cpf)
            => await ObterUmPor(m => m.Cpf == cpf);

        public async Task<Motorista> ObterPorEmail(string email)
            => await ObterUmPor(m => m.Email == email);

        public async Task<List<Motorista>> ObterAtivos()
            => await _dbSet.AsNoTracking()
                           .Where(m => m.Ativo)
                           .OrderBy(m => m.Nome)
                           .ToListAsync();
    }
}
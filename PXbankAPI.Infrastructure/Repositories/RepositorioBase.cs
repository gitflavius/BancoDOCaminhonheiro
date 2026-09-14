using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PXbankAPI.Domain.Interfaces;
using PXbankAPI.Infrastructure.Context;

namespace PXbankAPI.Infrastructure.Repositories
{
    public class RepositorioBase<T> : IRepositorio<T> where T : class
    {
        protected readonly PXbankDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositorioBase(PXbankDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T> ObterPorId(int id)
        {
            var entidade = await _dbSet.FindAsync(id);

            if (entidade is null)
                throw new KeyNotFoundException($"{typeof(T).Name} com id {id} nao encontrado.");

            return entidade;
        }

        public virtual async Task<List<T>> ObterTodos()
            => await _dbSet.AsNoTracking().ToListAsync();

        public virtual async Task<List<T>> ObterPor(Expression<Func<T, bool>> filtro)
            => await _dbSet.AsNoTracking().Where(filtro).ToListAsync();

        public virtual async Task<T> ObterUmPor(Expression<Func<T, bool>> filtro)
        {
            var entidade = await _dbSet.FirstOrDefaultAsync(filtro);

            if (entidade is null)
                throw new KeyNotFoundException($"{typeof(T).Name} nao encontrado para o filtro informado.");

            return entidade;
        }

        public virtual async Task Adicionar(T entidade)
            => await _dbSet.AddAsync(entidade);

        public virtual Task Atualizar(T entidade)
        {
            _dbSet.Update(entidade);
            return Task.CompletedTask;
        }

        public virtual async Task Deletar(int id)
        {
            var entidade = await ObterPorId(id);
            _dbSet.Remove(entidade);
        }

        public virtual async Task Salvar()
            => await _context.SaveChangesAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using SEG.Context;
using System.Linq.Expressions;

namespace SEG.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        // Usar o AsNoTracking pra não ficar rastreavel assim melhora o desempenho
        // Porém é bom usar só quando vai ter certeza que não vai reutilizar as informações que trouxe
        // Aqui no exemplo esta apenas consultando e mostrando as informações
        // Não preciso usar ela depois

        return await _context.Set<T>().AsNoTracking().ToListAsync();
    }
    public T Create(T entity)
    {
        _context.Set<T>().Add(entity);
        return entity;
    }

    public T Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
        return entity;
    }

    public T Update(T entity)
    {
        _context.Set<T>().Update(entity);
        return entity;
    }
}

using System.Linq.Expressions;

namespace Confiance.SEG.Infrastructure.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> GetAllAsync();
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
}

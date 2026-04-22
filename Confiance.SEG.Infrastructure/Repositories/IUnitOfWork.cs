using SEG.Context;

namespace Confiance.SEG.Infrastructure.Repositories;

public interface IUnitOfWork
{
    IUsuariosRepository UsuariosRepository { get; }
    Task CommitAsync();
    AppDbContext Context { get; }
}

using SEG.Context;

namespace Confiance.SEG.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private IUsuariosRepository? _usuarioRepo;
    public AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUsuariosRepository UsuariosRepository
    {
        get
        {
            return _usuarioRepo = _usuarioRepo ?? new UsuarioRepository(_context);
        }
    }

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public AppDbContext Context => _context;
}

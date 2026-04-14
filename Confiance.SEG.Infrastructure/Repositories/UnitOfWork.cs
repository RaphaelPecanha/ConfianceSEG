using SEG.Context;

namespace SEG.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private IUsuariosRepository? _usuarioRepo;
    public AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }


    //Essa implementação é necessária pra não precisar injetar no construtdor do UOW todas as classes e interfaces,
    // Assim evitando ser chamado de outros repositórios desnecessário
    // Só será chamado caso ela não exista, no momento que esta sendo utilizada
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
}

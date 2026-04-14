using SEG.Context;
using SEG.Models;

namespace SEG.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuariosRepository
{
    public UsuarioRepository(AppDbContext context) : base(context)
    {
    }
}

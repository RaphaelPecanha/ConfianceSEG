using Confiance.SEG.Domain;
using System.Linq.Expressions;
using SEG.Context;
using Microsoft.EntityFrameworkCore;

namespace Confiance.SEG.Infrastructure.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuariosRepository
{
    public UsuarioRepository(AppDbContext context) : base(context)
    {
    }

    // Additional user-specific queries can go here
}

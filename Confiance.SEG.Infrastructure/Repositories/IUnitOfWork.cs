namespace SEG.Repositories;

public interface IUnitOfWork
{
    IUsuariosRepository UsuariosRepository { get; }
    Task CommitAsync();
}

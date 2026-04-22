using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Confiance.SEG.Domain;
using SEG.Models;

namespace SEG.Context;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Usuario>? Usuarios { get; set; }
    public DbSet<PermUsuarioMenu>? PermUsuariosMenu { get; set; }
    public DbSet<QuadroAviso>? QuadroAvisos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure perm_usuarios_menu table
        builder.Entity<PermUsuarioMenu>(entity =>
        {
            entity.ToTable("perm_usuarios_menu");
            entity.HasIndex(p => new { p.IdUsuario, p.PathMenu }).IsUnique();
            entity.Property(p => p.PathMenu).HasMaxLength(255);
            entity.HasCheckConstraint("CK_PermUsuarioMenu_NivelPermissao", "nivel_permissao IN (1,2,3)");
            entity.HasOne(p => p.Usuario)
                  .WithMany()
                  .HasForeignKey(p => p.IdUsuario)
                  .HasConstraintName("FK_PermUsuarioMenu_Usuario")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure quadro_aviso table
        builder.Entity<QuadroAviso>(entity =>
        {
            entity.ToTable("quadro_aviso");
            entity.HasKey(e => e.Titulo);
            entity.Property(e => e.Titulo).HasMaxLength(500);
            entity.Property(e => e.Descricao).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.CtLink).HasMaxLength(3).IsRequired();
            entity.Property(e => e.Link).HasMaxLength(45);
        });
    }
}

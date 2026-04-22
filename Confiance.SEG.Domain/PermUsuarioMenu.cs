using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confiance.SEG.Domain;

[Table("perm_usuarios_menu")]
public class PermUsuarioMenu
{
    [Key]
    [Column("id")]
    public int? Id { get; set; }

    [Required]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Required]
    [Column("path_menu", TypeName = "VARCHAR(255)")]
    public string? PathMenu { get; set; }

    [Required]
    [Column("nivel_permissao")]
    public int NivelPermissao { get; set; }

    // Optional navigation property
    // Navigation properties should reference domain types
    public Usuario? Usuario { get; set; }
}

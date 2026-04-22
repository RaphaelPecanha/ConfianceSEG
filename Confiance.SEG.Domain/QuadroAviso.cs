using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Confiance.SEG.Domain;

[Table("quadro_aviso")]
public class QuadroAviso
{
    [Key]
    [Column("titulo", TypeName = "VARCHAR(500)")]
    public string? Titulo { get; set; }

    [Required]
    [Column("descricao", TypeName = "VARCHAR(1000)")]
    public string? Descricao { get; set; }

    [Required]
    [Column("ct_link", TypeName = "VARCHAR(3)")]
    public string CtLink { get; set; } = "N";

    [Column("link", TypeName = "VARCHAR(45)")]
    public string? Link { get; set; }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Validations;

namespace SEG.Models;

[Table("usuarios")]
public class Usuario
{
    [Key]
    [Column("id_usuario")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "Nome de usuário obrigatório")]
    [Column("nome", TypeName = "VARCHAR(45)")]
    public string? Nome { get; set; }

    [Required(ErrorMessage = "Login de usuário obrigatório")]
    [Column("login", TypeName = "VARCHAR(45)")]
    public string? Login { get; set; }

    [Required(ErrorMessage = "Senha de usuário obrigatório")]
    [Column("senha", TypeName = "VARCHAR(200)")]
    public string? Senha { get; set; }

    [Required(ErrorMessage = "Email de usuário obrigatório")]
    [Column("email", TypeName = "VARCHAR(200)")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "IdSetor de usuário obrigatório")]
    [Column("id_set", TypeName = "INT(10)")]
    public int? IdSetor { get; set; }

    [Required(ErrorMessage = "Data Cadastro de usuário obrigatório")]
    [Column("dtcad_us", TypeName = "TIMESTAMP")]
    public DateTime DataCadastro { get; set; }

    [Required(ErrorMessage = "Status de usuário obrigatório")]
    [Column("status_us", TypeName = "TINYINT(1)")]
    public bool Status { get; set; }

    [Column("de_primeironome", TypeName = "VARCHAR(100)")]
    public string? PrimeiroNome { get; set; }

    [Column("de_sobrenome", TypeName = "VARCHAR(100)")]
    public string? SobreNome { get; set; }

    // [JsonIgnore] ignora esse atributo na serialização e não aparece como opção
    [Column("id_sapiens", TypeName = "INT(11)")]
    public int? IdSapiens { get; set; }


    //Usar pra fazer validações nas propriedades
    //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
      
    
}
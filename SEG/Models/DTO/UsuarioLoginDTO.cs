using System.ComponentModel.DataAnnotations;

namespace SEG.Models.DTO
{
    public class UsuarioLoginDTO
    {
        [Required(ErrorMessage = "Login de usuário obrigatório")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Senha de usuário obrigatória")]
        public string Senha { get; set; }
    }
}

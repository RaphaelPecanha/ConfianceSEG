using System.Collections.Generic;

namespace Confiance.SEG.Application.DTOs;

public class UsuarioLoginResponseDTO
{
    public int Id { get; set; }
    public string? Login { get; set; }
    public string? Nome { get; set; }
    public string? SobreNome { get; set; }
    public string? NomeCompleto { get; set; }
    public int Setor { get; set; }
    public int Status { get; set; }
    public string? Email { get; set; }
    public string? CodSapiens { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? Expiration { get; set; }
    public List<List<object>> Menus { get; set; } = new List<List<object>>();
}

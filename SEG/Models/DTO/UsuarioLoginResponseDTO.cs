namespace SEG.Models.DTO
{
    public class UsuarioLoginResponseDTO
    {
        public int Id { get; set; }
        public string? Login { get; set; }
        // Nota: A senha **NÃO DEVE** ser retornada em um response de login.
        // Apenas para fins de representação, foi incluída no seu exemplo, mas será omitida aqui.
        public string? Nome { get; set; }
        public string? SobreNome { get; set; }
        public string? NomeCompleto { get; set; }
        public int Setor { get; set; }
        public int Status { get; set; }
        public string? Email { get; set; }
        public string? CodSapiens { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        // A data de expiração do token é importante para o cliente
        public DateTime? Expiration { get; set; }
        // Lista de menus: o banco retornará uma lista de pares [rota, id]
        public List<List<object>> Menus { get; set; } = new List<List<object>>();
    }
}
using System.Security.Cryptography;
using System.Text;

public static class MD5HashGenerator
{
    public static string GenerateMD5Hash(string input)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            // Converte a string de entrada para um array de bytes e calcula o hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Cria um novo StringBuilder para coletar os bytes e criar uma string.
            StringBuilder sBuilder = new StringBuilder();

            // Percorre cada byte do array de hash e formata cada um como uma string hexadecimal.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Retorna a string hexadecimal completa.
            return sBuilder.ToString();
        }
    }
}
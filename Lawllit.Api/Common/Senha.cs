namespace Lawllit.Api.Common;

public static class Senha
{
    public static string Gerar(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

    // BCrypt.Verify lança em hash malformado em vez de devolver falso, e isso virava 500 no login.
    public static bool Confere(string? senha, string? hash)
    {
        if (string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(hash)) return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}

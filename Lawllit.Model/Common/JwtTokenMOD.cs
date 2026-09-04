namespace Lawllit.Model.Common;

public sealed class JwtTokenMOD
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

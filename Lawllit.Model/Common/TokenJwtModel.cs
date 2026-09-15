namespace Lawllit.Model.Common;

public sealed class TokenJwtModel
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}

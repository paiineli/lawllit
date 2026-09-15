namespace Lawllit.Model.Common;

public static class Constantes
{
    // Indicador S/N das colunas SN_. Fica aqui para nenhuma camada escrever a letra solta.
    public const string Sim = "S";
    public const string Nao = "N";

    public static readonly string[] TemasValidos = ["dark", "light", "high-contrast"];
    public static readonly string[] TamanhosFonteValidos = ["normal", "large", "xlarge"];

    public const string CabecalhoChaveApi = "X-Api-Key";
    public const string ClaimTokenApi = "api_token";
}

namespace Lawllit.Site.Common;

public static class TurnstileChaves
{
    public const string CaminhoChavePublica = "Turnstile:SiteKey";
    public const string CaminhoChaveSecreta = "Turnstile:SecretKey";

    public static readonly string[] Obrigatorias = [CaminhoChavePublica, CaminhoChaveSecreta];

    // Chaves de teste da Cloudflare, que aprovam qualquer token. Não vão para o appsettings
    // porque lá virariam fallback silencioso se a variável faltasse na Railway.
    private const string ChavePublicaDesenvolvimento = "1x00000000000000000000AA";
    private const string ChaveSecretaDesenvolvimento = "1x0000000000000000000000000000000AA";

    // Só preenche o que falta, para quem já tem chave própria no ambiente local continuar com ela.
    public static void AplicarPadraoDesenvolvimento(IConfigurationManager configuration)
    {
        var padrao = new Dictionary<string, string?>();

        if (string.IsNullOrWhiteSpace(configuration[CaminhoChavePublica]))
            padrao[CaminhoChavePublica] = ChavePublicaDesenvolvimento;

        if (string.IsNullOrWhiteSpace(configuration[CaminhoChaveSecreta]))
            padrao[CaminhoChaveSecreta] = ChaveSecretaDesenvolvimento;

        if (padrao.Count > 0)
            configuration.AddInMemoryCollection(padrao);
    }
}

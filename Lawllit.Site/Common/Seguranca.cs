namespace Lawllit.Site.Common;

public static class Seguranca
{
    // Biblioteca de CDN nova exige origem nova aqui, senão o navegador recusa em silêncio.
    public const string PoliticaConteudo =
        "default-src 'self'; " +
        "base-uri 'self'; " +
        "object-src 'none'; " +
        "frame-ancestors 'none'; " +
        "form-action 'self'; " +
        "style-src 'self'; " +
        "font-src 'self'; " +

        // data: é o favicon embutido. blob: é o arquivo que o compressor e o unificador geram.
        "img-src 'self' data: blob: " +

            // Jogos: logo e arte oficiais, perk da wiki do DBD, Baro pelo jsDelivr e escudo de Dota na Steam.
            "https://cmsassets.rgpub.io https://ddragon.leagueoflegends.com " +
            "https://shared.fastly.steamstatic.com https://www-static.warframe.com " +
            "https://deadbydaylight.wiki.gg https://cdn.jsdelivr.net " +
            "https://cdn.steamusercontent.com https://steamcdn-a.akamaihd.net https://steamusercontent-a.akamaihd.net; " +

        // jsdelivr serve pdf-lib, pdf.js, xlsx e qrcodejs. challenges.cloudflare.com é o Turnstile.
        "script-src 'self' https://cdn.jsdelivr.net https://challenges.cloudflare.com; " +
        "frame-src https://challenges.cloudflare.com; " +

        // Worker de outra origem é recusado, então o pdf.js embrulha o do jsdelivr num blob.
        "worker-src blob:; " +

        // A consulta de CEP é a única chamada do navegador para fora.
        "connect-src 'self' https://viacep.com.br";
}

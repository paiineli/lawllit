using Lawllit.Model.Common.Enums;

namespace Lawllit.Site.Common;

// Num lugar só porque o hub, a página de cada jogo e o sitemap precisam da mesma lista.
public static class Jogos
{
    // Logo e arte oficiais, carregadas de onde cada jogo publica. Origem nova exige entrada no
    // img-src da CSP, em Seguranca.cs.
    public static readonly Jogo[] Todos =
    [
        new(JogoEnum.VALORANT, "Valorant", "valorant", "jogo-valorant",
            Logo: "https://cmsassets.rgpub.io/sanity/images/dsfx7636/news/7b76209193f1bfe190d3ae6ef8728328870be9c3-736x138.png",
            // A Riot só distribui o wallpaper em zip, então a arte mora no próprio site, em 1920x1080.
            Arte: "~/img/jogos/valorant.jpg"),

        new(JogoEnum.LOL, "Lol", "league of legends", "jogo-lol",
            Logo: "https://cmsassets.rgpub.io/sanity/images/dsfx7636/news/9eb028de391e65072d06e77f06d0955f66b9fa2c-736x316.png",
            Arte: "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Jinx_0.jpg"),

        new(JogoEnum.DOTA2, "Dota2", "dota 2", "jogo-dota",
            Logo: Steam(570, "logo.png"),
            Arte: Steam(570, "library_hero.jpg")),

        new(JogoEnum.DBD, "Dbd", "dead by daylight", "jogo-dbd",
            Logo: Steam(381210, "logo.png"),
            Arte: Steam(381210, "library_hero.jpg")),

        new(JogoEnum.WARFRAME, "Warframe", "warframe", "jogo-warframe",
            Logo: "https://www-static.warframe.com/images/logoWhite.png",
            Arte: Steam(230410, "library_hero.jpg")),
    ];

    // Glifo do Baro no catálogo aberto da WFCD, o mesmo de onde saem as imagens do estoque.
    public const string ImagemBaro = "https://cdn.jsdelivr.net/gh/wfcd/warframe-items@master/data/img/BaroKiteerAvatar.png";

    public static Jogo Por(JogoEnum codigo)
        => Todos.First(jogo => jogo.Codigo == codigo);

    // A Steam troca a arte a cada evento do jogo, e o endereço continua o mesmo. A arte é a "hero",
    // que vem sem logo, porque a "header" já traz a logo desenhada e ela sairia duplicada.
    private static string Steam(int appId, string arquivo)
        => $"https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/{appId}/{arquivo}";
}

// A action do controller anda junto do jogo, o que impede de renomear uma sem a outra. A classe
// CSS carrega as cores do jogo e o enquadramento da arte para a página inteira.
public sealed record Jogo(JogoEnum Codigo, string Acao, string Nome, string Classe, string Logo, string Arte);

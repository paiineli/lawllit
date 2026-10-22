namespace Lawllit.Site.Common;

// O que entra no sitemap e o que o robots.txt fecha. A montagem fica no Lawllit.Web.
public static class SeoCaminhos
{
    public static readonly string[] Indexaveis =
    [
        "/",
        "/Ferramentas",
        .. Ferramentas.Todas.Select(ferramenta => $"/Ferramentas/{ferramenta.Acao}"),
        "/Jogos",
        .. Jogos.Todos.Select(jogo => $"/Jogos/{jogo.Acao}"),
    ];

    public static readonly string[] Bloqueados =
    [
        "/Inicio/Erro",

        // Fragmento HTML da assinatura de e-mail, para copiar e colar. Não é página de busca.
        "/signature.html",
    ];
}

namespace Lawllit.Finance.Site.Common;

// O que entra no sitemap e o que o robots.txt fecha. A montagem fica no Lawllit.Web.
public static class SeoCaminhos
{
    // Só a entrada e os dois documentos legais são públicos, o resto exige login.
    public static readonly string[] Indexaveis =
    [
        "/",
        "/Legal/Privacidade",
        "/Legal/Termos",
    ];

    public static readonly string[] Bloqueados =
    [
        "/Conta",
        "/Painel",
        "/Transacao",
        "/Categoria",
        "/Perfil",
        "/Cotacoes",
        "/Erro",
    ];
}

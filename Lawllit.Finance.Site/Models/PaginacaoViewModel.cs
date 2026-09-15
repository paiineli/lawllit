namespace Lawllit.Finance.Site.Models;

// Os ValoresRota levam os filtros da tela para o link de página não perder mês, ano e busca.
public sealed class PaginacaoViewModel
{
    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public Dictionary<string, object?> ValoresRota { get; set; } = [];
}

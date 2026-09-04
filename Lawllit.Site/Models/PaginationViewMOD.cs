namespace Lawllit.Site.Models;

// Alimenta a partial _Pagination. Os RouteValues carregam os filtros da tela para
// o link de página não perder mês, ano, tipo e busca.
public sealed class PaginationViewMOD
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public Dictionary<string, object?> RouteValues { get; set; } = [];
}

namespace Lawllit.Model.Common;

public sealed class Paginacao
{
    public int PaginaAtual { get; set; } = 1;
    public int ItensPorPagina { get; set; } = 50;

    public int Offset => (Math.Max(PaginaAtual, 1) - 1) * ItensPorPagina;
}

public sealed class PaginacaoResposta<TItem>
{
    public List<TItem> Itens { get; set; } = [];
    public int TotalItens { get; set; }
    public int PaginaAtual { get; set; } = 1;
    public int ItensPorPagina { get; set; } = 50;

    public int TotalPaginas => ItensPorPagina > 0
        ? (int)Math.Ceiling(TotalItens / (double)ItensPorPagina)
        : 0;
}

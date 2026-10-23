namespace Lawllit.Model.Common;

public sealed class Paginacao
{
    // O teto de página segura o OFFSET dentro do int, senão ?pagina=99999999 virava 500.
    public int PaginaAtual { get; set => field = Math.Clamp(value, 1, 100_000); } = 1;

    // O teto impede que alguém peça a tabela inteira numa página só.
    public int ItensPorPagina { get; set => field = Math.Clamp(value, 1, 100); } = 50;

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

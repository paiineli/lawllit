using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Finance.Contratos;

public sealed class TransacaoFiltroModel
{
    public TipoTransacaoEnum? Tipo { get; set; }
    public int? Mes { get; set; }
    public int? Ano { get; set; }
    public string? Busca { get; set; }
    public Paginacao Paginacao { get; set; } = new();
}

// Os totais são do mês inteiro e não da página, senão o rodapé mudava a cada troca de página.
public sealed class TransacaoPaginaModel
{
    public PaginacaoResposta<TransacaoItemModel> Pagina { get; set; } = new();
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal TotalInvestimentos { get; set; }
    public int RecorrentesPendentes { get; set; }
}

public sealed class TransacaoItemModel
{
    public Guid Codigo { get; set; }
    public string? Descricao { get; set; }
    public decimal Valor { get; set; }
    public TipoTransacaoEnum Tipo { get; set; }
    public DateTime Data { get; set; }
    public bool Recorrente { get; set; }
    public Guid CodigoCategoria { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;

    public static TransacaoItemModel De(TransacaoModel transacao) => new()
    {
        Codigo = transacao.CdTransacao,
        Descricao = transacao.TxDescricao,
        Valor = transacao.VlTransacao,
        Tipo = transacao.TxTipo,
        Data = transacao.DtTransacao,
        Recorrente = transacao.Recorrente,
        CodigoCategoria = transacao.CdCategoria,
        NomeCategoria = transacao.Categoria?.NmCategoria ?? string.Empty,
    };
}

public sealed class TransacaoSalvarModel
{
    public Guid Codigo { get; set; }
    public string? Descricao { get; set; }
    public decimal Valor { get; set; }
    public TipoTransacaoEnum Tipo { get; set; }
    public DateTime Data { get; set; }
    public Guid CodigoCategoria { get; set; }
    public bool Recorrente { get; set; }

    // O teto é o que cabe no NUMERIC(18, 2) da coluna.
    public string? Validar()
    {
        if (Valor <= 0 || Valor >= 10_000_000_000_000_000m) return "Informe um valor maior que zero.";
        if (!Enum.IsDefined(Tipo)) return "Tipo inválido.";
        if (Data == default) return "Informe a data.";
        if (CodigoCategoria == Guid.Empty) return "Escolha uma categoria.";
        if ((Descricao?.Trim().Length ?? 0) > 200) return "A descrição passou de 200 caracteres.";
        return null;
    }
}

public sealed class ImportarRecorrentesModel
{
    public int Mes { get; set; }
    public int Ano { get; set; }

    public string? Validar() => Validacao.Periodo(Mes, Ano) ? null : "Período inválido.";
}

public sealed class ImportarRecorrentesResultadoModel
{
    public int QuantidadeImportada { get; set; }
}

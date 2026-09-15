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
    public PaginacaoResposta<TransacaoModel> Pagina { get; set; } = new();
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal TotalInvestimentos { get; set; }
    public int RecorrentesPendentes { get; set; }
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
}

public sealed class ImportarRecorrentesModel
{
    public int Mes { get; set; }
    public int Ano { get; set; }
}

public sealed class ImportarRecorrentesResultadoModel
{
    public int QuantidadeImportada { get; set; }
}

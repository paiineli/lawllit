using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using System.ComponentModel.DataAnnotations;

namespace Lawllit.Finance.Site.Models;

public sealed class TransacaoListaViewModel
{
    public PaginacaoResposta<TransacaoModel> Pagina { get; set; } = new();
    public List<CategoriaModel> Categorias { get; set; } = [];
    public TipoTransacaoEnum? FiltroTipo { get; set; }
    public string? FiltroBusca { get; set; }
    public int FiltroMes { get; set; }
    public int FiltroAno { get; set; }
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal TotalInvestimentos { get; set; }
    public int RecorrentesPendentes { get; set; }

    public List<TransacaoModel> Transacoes => Pagina.Itens;

    public bool TemFiltroAtivo => FiltroTipo.HasValue || !string.IsNullOrEmpty(FiltroBusca);

    public TransacaoFiltroRotaViewModel FiltroRota => new()
    {
        Tipo = FiltroTipo,
        Mes = FiltroMes,
        Ano = FiltroAno,
        Busca = FiltroBusca,
        Pagina = Pagina.PaginaAtual,
    };
}

// O filtro viaja no post e volta na querystring do redirect, senão a gravação cai num Index
// sem filtro, a API assume o mês atual e o usuário perde o mês que estava olhando.
public sealed class TransacaoFiltroRotaViewModel
{
    public TipoTransacaoEnum? Tipo { get; set; }
    public int? Mes { get; set; }
    public int? Ano { get; set; }
    public string? Busca { get; set; }
    public int? Pagina { get; set; }

    // Valor nulo não entra na URL, então filtro vazio continua gerando link limpo.
    public Dictionary<string, object?> ParaValoresRota() => new()
    {
        ["tipo"] = Tipo,
        ["mes"] = Mes,
        ["ano"] = Ano,
        ["busca"] = string.IsNullOrWhiteSpace(Busca) ? null : Busca,
        ["pagina"] = Pagina > 1 ? Pagina : null,
    };
}

public sealed class TransacaoFormViewModel
{
    public Guid Codigo { get; set; }

    [StringLength(200, ErrorMessage = "Descrição não pode exceder 200 caracteres")]
    public string? Descricao { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    public TipoTransacaoEnum Tipo { get; set; } = TipoTransacaoEnum.DESPESA;

    [Required(ErrorMessage = "Data é obrigatória")]
    public DateTime Data { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Categoria é obrigatória")]
    public Guid CodigoCategoria { get; set; }

    public bool Recorrente { get; set; }
}

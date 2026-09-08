using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using System.ComponentModel.DataAnnotations;

namespace Lawllit.Site.Models.Finance;

public sealed class TransactionListViewMOD
{
    public PagedResult<TransactionMOD> Page { get; set; } = new();
    public List<CategoryMOD> Categories { get; set; } = [];
    public TransactionTypeEnum? FilterType { get; set; }
    public string? FilterSearch { get; set; }
    public int FilterMonth { get; set; }
    public int FilterYear { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalInvestments { get; set; }
    public int PendingRecurringCount { get; set; }

    public List<TransactionMOD> Transactions => Page.Items;

    public bool HasActiveFilters => FilterType.HasValue || !string.IsNullOrEmpty(FilterSearch);

    public TransactionFilterRouteViewMOD FilterRoute => new()
    {
        Type = FilterType,
        Month = FilterMonth,
        Year = FilterYear,
        Search = FilterSearch,
        Page = Page.CurrentPage,
    };
}

// O filtro em tela viaja junto no post e volta na querystring do redirect. Sem isso a gravação
// cai num Index sem filtro, a API assume o mês atual e o usuário perde o mês que estava olhando.
public sealed class TransactionFilterRouteViewMOD
{
    public TransactionTypeEnum? Type { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? Search { get; set; }
    public int? Page { get; set; }

    // Valor nulo não entra na URL, então filtro vazio continua gerando link limpo.
    public Dictionary<string, object?> ToRouteValues() => new()
    {
        ["type"] = Type,
        ["month"] = Month,
        ["year"] = Year,
        ["search"] = string.IsNullOrWhiteSpace(Search) ? null : Search,
        ["page"] = Page > 1 ? Page : null,
    };
}

public sealed class TransactionFormViewMOD
{
    public Guid Id { get; set; }

    [StringLength(200, ErrorMessage = "Val_DescriptionMaxLength")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Val_AmountRange")]
    public decimal Amount { get; set; }

    public TransactionTypeEnum Type { get; set; } = TransactionTypeEnum.EXPENSE;

    [Required(ErrorMessage = "Val_DateRequired")]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Val_CategoryRequired")]
    public Guid CategoryId { get; set; }

    public bool IsRecurring { get; set; }
}

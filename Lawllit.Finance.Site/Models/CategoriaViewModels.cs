using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance.Contratos;
using System.ComponentModel.DataAnnotations;

namespace Lawllit.Finance.Site.Models;

public sealed class CategoriaListaViewModel
{
    public List<CategoriaItemModel> Categorias { get; set; } = [];
    public TipoTransacaoEnum? FiltroTipo { get; set; }
    public string? FiltroBusca { get; set; }
}

public sealed class CategoriaFormViewModel
{
    public Guid Codigo { get; set; }

    [Required(ErrorMessage = "Nome da categoria é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    public TipoTransacaoEnum Tipo { get; set; } = TipoTransacaoEnum.DESPESA;
}

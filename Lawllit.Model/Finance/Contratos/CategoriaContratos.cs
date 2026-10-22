using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Finance.Contratos;

public sealed class CategoriaFiltroModel
{
    public TipoTransacaoEnum? Tipo { get; set; }
    public string? Busca { get; set; }
}

public sealed class CategoriaSalvarModel
{
    public Guid Codigo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoTransacaoEnum Tipo { get; set; }

    public string? Validar()
    {
        if (!Validacao.Tamanho(Nome, 1, 100)) return "Informe um nome de até 100 caracteres.";
        if (!Enum.IsDefined(Tipo)) return "Tipo inválido.";
        return null;
    }
}

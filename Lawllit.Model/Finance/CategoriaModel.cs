using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Finance;

public sealed class CategoriaModel
{
    public Guid CdCategoria { get; set; }
    public string NmCategoria { get; set; } = string.Empty;
    public TipoTransacaoEnum TxTipo { get; set; }
    public Guid CdUsuario { get; set; }
    public string SnAtivo { get; set; } = Constantes.Sim;
    public DateTime DtCadastro { get; set; }
}

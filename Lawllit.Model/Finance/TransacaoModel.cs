using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Finance;

public sealed class TransacaoModel
{
    public Guid CdTransacao { get; set; }
    public string TxDescricao { get; set; } = string.Empty;
    public decimal VlTransacao { get; set; }
    public TipoTransacaoEnum TxTipo { get; set; }
    public DateTime DtTransacao { get; set; }
    public string SnRecorrente { get; set; } = Constantes.Nao;
    public Guid CdUsuario { get; set; }
    public Guid CdCategoria { get; set; }
    public string SnAtivo { get; set; } = Constantes.Sim;
    public DateTime DtCadastro { get; set; }

    public bool Recorrente => SnRecorrente == Constantes.Sim;

    // Preenchido pelo join da listagem. A transação sozinha não carrega a categoria.
    public CategoriaModel? Categoria { get; set; }
}

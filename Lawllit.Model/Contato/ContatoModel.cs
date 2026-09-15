using Lawllit.Model.Common;

namespace Lawllit.Model.Contato;

// O contato não é do app financeiro, é do portfólio, por isso fica fora de Finance.
public sealed class ContatoModel
{
    public Guid CdContato { get; set; }
    public string NmContato { get; set; } = string.Empty;
    public string TxEmail { get; set; } = string.Empty;
    public string TxMensagem { get; set; } = string.Empty;
    public string SnAtivo { get; set; } = Constantes.Sim;
    public DateTime DtCadastro { get; set; }
}

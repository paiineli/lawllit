using Lawllit.Model.Common;

namespace Lawllit.Model.Finance;

public sealed class UsuarioModel
{
    public Guid CdUsuario { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string TxEmail { get; set; } = string.Empty;
    public string? TxSenha { get; set; }
    public string SnEmailConfirmado { get; set; } = Constantes.Nao;
    public string? TxTokenConfirmacao { get; set; }
    public DateTime? DtExpiraConfirmacao { get; set; }
    public string? TxTokenSenha { get; set; }
    public DateTime? DtExpiraTokenSenha { get; set; }
    public string TxTema { get; set; } = "dark";
    public string TxTamanhoFonte { get; set; } = "normal";
    public string SnAtivo { get; set; } = Constantes.Sim;
    public DateTime DtCadastro { get; set; }

    public bool EmailConfirmado => SnEmailConfirmado == Constantes.Sim;
    public bool TemSenha => !string.IsNullOrEmpty(TxSenha);
}

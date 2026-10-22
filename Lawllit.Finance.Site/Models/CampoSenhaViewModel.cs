namespace Lawllit.Finance.Site.Models;

// Sem valor de propósito: senha nunca volta preenchida no HTML depois de um POST com erro.
public sealed record CampoSenhaViewModel(string Nome, string Id, string Autocompletar, string? Dica = null);

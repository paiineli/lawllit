namespace Lawllit.Finance.Site.Models;

public sealed record ErroViewModel(string? CodigoRequisicao)
{
    public bool MostrarCodigo => !string.IsNullOrEmpty(CodigoRequisicao);
}

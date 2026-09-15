namespace Lawllit.Model.Common;

// O Site converte de volta para Resultado e mostra a mensagem como veio, sem traduzir.
public sealed class ErroApiModel
{
    public string Mensagem { get; set; } = string.Empty;
}

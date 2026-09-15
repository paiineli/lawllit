namespace Lawllit.Model.Common;

// Erro de regra não vira exceção, então nenhuma camada precisa de try/catch para o previsível.
public class Resultado
{
    public bool Sucesso { get; }
    public string? Mensagem { get; }

    protected Resultado(bool sucesso, string? mensagem)
    {
        Sucesso = sucesso;
        Mensagem = mensagem;
    }

    public static Resultado Ok() => new(true, null);
    public static Resultado Falha(string mensagem) => new(false, mensagem);
}

public sealed class Resultado<TValor> : Resultado
{
    public TValor? Valor { get; }

    private Resultado(TValor? valor, bool sucesso, string? mensagem) : base(sucesso, mensagem)
    {
        Valor = valor;
    }

    public static Resultado<TValor> Ok(TValor valor) => new(valor, true, null);
    public new static Resultado<TValor> Falha(string mensagem) => new(default, false, mensagem);
}

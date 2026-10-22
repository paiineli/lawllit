using Lawllit.Model.Common.Enums;

namespace Lawllit.Finance.Site.Common;

// Aqui para as views não repetirem ternário aninhado a cada valor pintado.
public static class TipoTransacaoExtensions
{
    // A tela fala do jeito que a pessoa pensa no dinheiro. O CSV e o banco continuam nos nomes formais.
    public static string RotuloTela(this TipoTransacaoEnum tipo) => tipo switch
    {
        TipoTransacaoEnum.RECEITA => "Entrou",
        TipoTransacaoEnum.INVESTIMENTO => "Investiu",
        _ => "Saiu",
    };

    public static string Rotulo(this TipoTransacaoEnum tipo) => tipo switch
    {
        TipoTransacaoEnum.RECEITA => "Receita",
        TipoTransacaoEnum.INVESTIMENTO => "Investimento",
        _ => "Despesa",
    };

    public static string ClasseTexto(this TipoTransacaoEnum tipo) => tipo switch
    {
        TipoTransacaoEnum.RECEITA => "texto-receita",
        TipoTransacaoEnum.INVESTIMENTO => "texto-investimento",
        _ => "texto-despesa",
    };

    public static string ClasseCor(this TipoTransacaoEnum tipo) => tipo switch
    {
        TipoTransacaoEnum.RECEITA => "cor-receita",
        TipoTransacaoEnum.INVESTIMENTO => "cor-investimento",
        _ => "cor-despesa",
    };

    public static string Icone(this TipoTransacaoEnum tipo) => tipo switch
    {
        TipoTransacaoEnum.RECEITA => "cifrao",
        TipoTransacaoEnum.INVESTIMENTO => "grafico-sobe",
        _ => "sacola",
    };

    // Investimento não leva sinal: não é perda, é dinheiro guardado.
    public static string Sinal(this TipoTransacaoEnum tipo) => tipo switch
    {
        TipoTransacaoEnum.RECEITA => "+ ",
        TipoTransacaoEnum.INVESTIMENTO => "",
        _ => "− ",
    };
}

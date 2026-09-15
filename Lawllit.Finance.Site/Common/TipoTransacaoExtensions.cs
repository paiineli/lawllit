using Lawllit.Model.Common.Enums;

namespace Lawllit.Finance.Site.Common;

// Aqui para as views não repetirem ternário aninhado a cada badge.
public static class TipoTransacaoExtensions
{
    public static string ClasseBadge(this TipoTransacaoEnum tipo) => tipo switch
    {
        TipoTransacaoEnum.RECEITA => "badge-income",
        TipoTransacaoEnum.INVESTIMENTO => "badge-investment",
        _ => "badge-expense",
    };

    public static string ClasseTexto(this TipoTransacaoEnum tipo) => tipo switch
    {
        TipoTransacaoEnum.RECEITA => "text-green",
        TipoTransacaoEnum.INVESTIMENTO => "text-blue",
        _ => "text-red",
    };

    public static string Rotulo(this TipoTransacaoEnum tipo) => tipo switch
    {
        TipoTransacaoEnum.RECEITA => "Receita",
        TipoTransacaoEnum.INVESTIMENTO => "Investimento",
        _ => "Despesa",
    };

    public static string Sinal(this TipoTransacaoEnum tipo)
        => tipo == TipoTransacaoEnum.RECEITA ? "+" : "-";
}

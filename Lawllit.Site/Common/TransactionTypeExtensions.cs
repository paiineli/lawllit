using Lawllit.Model.Common.Enums;

namespace Lawllit.Site.Common;

// Cada tipo carrega sempre o mesmo par de classe visual e chave de tradução na UI.
// Centralizado aqui para as views não repetirem ternário aninhado a cada badge.
public static class TransactionTypeExtensions
{
    public static string BadgeClass(this TransactionTypeEnum type) => type switch
    {
        TransactionTypeEnum.INCOME => "badge-income",
        TransactionTypeEnum.INVESTMENT => "badge-investment",
        _ => "badge-expense",
    };

    public static string TextClass(this TransactionTypeEnum type) => type switch
    {
        TransactionTypeEnum.INCOME => "text-green",
        TransactionTypeEnum.INVESTMENT => "text-blue",
        _ => "text-red",
    };

    public static string LabelKey(this TransactionTypeEnum type) => type switch
    {
        TransactionTypeEnum.INCOME => "Lbl_Income",
        TransactionTypeEnum.INVESTMENT => "Lbl_Investment",
        _ => "Lbl_Expense",
    };

    public static string AmountSign(this TransactionTypeEnum type)
        => type == TransactionTypeEnum.INCOME ? "+" : "-";
}

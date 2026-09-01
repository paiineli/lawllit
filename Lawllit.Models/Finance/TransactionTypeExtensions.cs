namespace Lawllit.Models.Finance;

// Cada tipo carrega sempre o mesmo par de classe visual e chave de tradução na UI.
// Centralizado aqui para as views não repetirem ternário aninhado a cada badge.
public static class TransactionTypeExtensions
{
    public static string BadgeClass(this TransactionType type) => type switch
    {
        TransactionType.Income => "badge-income",
        TransactionType.Investment => "badge-investment",
        _ => "badge-expense",
    };

    public static string TextClass(this TransactionType type) => type switch
    {
        TransactionType.Income => "text-green",
        TransactionType.Investment => "text-blue",
        _ => "text-red",
    };

    public static string LabelKey(this TransactionType type) => type switch
    {
        TransactionType.Income => "Lbl_Income",
        TransactionType.Investment => "Lbl_Investment",
        _ => "Lbl_Expense",
    };

    public static string AmountSign(this TransactionType type)
        => type == TransactionType.Income ? "+" : "-";
}

namespace Lawllit.Models.Finance;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public Guid UserId { get; set; }
}

namespace Lawllit.Model.Jogos.Contratos;

public sealed class BaroModel
{
    public DateTime Chegada { get; set; }
    public DateTime Partida { get; set; }
    public string Local { get; set; } = string.Empty;

    // Vazio enquanto ele não chega.
    public List<ItemBaroModel> Itens { get; set; } = [];
}

public sealed class ItemBaroModel
{
    public string Nome { get; set; } = string.Empty;
    public int? Ducats { get; set; }
    public int? Creditos { get; set; }
    public string? Imagem { get; set; }
}

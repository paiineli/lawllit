namespace Lawllit.Model.Jogos.Contratos;

public sealed class ShrineModel
{
    public List<PerkShrineModel> Perks { get; set; } = [];
    public DateTime Fim { get; set; }
}

public sealed class PerkShrineModel
{
    public string Nome { get; set; } = string.Empty;
    public string Personagem { get; set; } = string.Empty;
    public int Fragmentos { get; set; }

    // Nulo quando a wiki não tem página para a perk, e aí a página mostra só o nome.
    public string? Icone { get; set; }
}

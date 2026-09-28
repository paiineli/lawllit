using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Jogos.Contratos;

public sealed class AtualizacaoModel
{
    public JogoEnum Jogo { get; set; }
    public string Titulo { get; set; } = string.Empty;

    // Nulo no LoL, no TFT e no Valorant, cujas fontes só dizem o número do patch.
    public DateTime? Data { get; set; }

    public string Link { get; set; } = string.Empty;
}

using Lawllit.Model.Common.Enums;

namespace Lawllit.Model.Jogos.Contratos;

public sealed class AgendaModel
{
    // Os únicos com agenda. O site só busca agenda para estes, e a API recusa qualquer outro.
    public static readonly JogoEnum[] JogosDisponiveis = [JogoEnum.VALORANT, JogoEnum.LOL, JogoEnum.DOTA2];

    public JogoEnum Jogo { get; set; }

    // Inclui as que estão ao vivo, que a fonte entrega junto das futuras.
    public List<PartidaModel> Proximas { get; set; } = [];
    public List<PartidaModel> Resultados { get; set; } = [];

    // Hora da última leitura boa. Com a fonte fora do ar a agenda fica a mesma e isto envelhece.
    public DateTime AtualizadoEm { get; set; }
}

public sealed class PartidaModel
{
    public string Campeonato { get; set; } = string.Empty;
    public EquipePartidaModel EquipeA { get; set; } = new();
    public EquipePartidaModel EquipeB { get; set; } = new();
    public DateTime Inicio { get; set; }

    // Texto como "Bo3", porque a fonte também usa formatos fora do padrão.
    public string? Formato { get; set; }

    public bool Concluida { get; set; }
    public string? LinkPartida { get; set; }

    // A fonte só publica a transmissão a partir de duas horas antes do jogo.
    public string? LinkTransmissao { get; set; }
}

public sealed class EquipePartidaModel
{
    public string Nome { get; set; } = string.Empty;

    // Só o Dota tem, pelo OpenDota. A Liquipedia bloqueia hotlink e não é dona dos escudos.
    public string? Logo { get; set; }

    // Texto e não número, porque reset de chave aparece como "W" e "L".
    public string? Placar { get; set; }

    public bool Vencedora { get; set; }
}

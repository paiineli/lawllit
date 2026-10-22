using Lawllit.Model.Common;
using Lawllit.Model.Jogos.Contratos;
using Lawllit.Site.Common;

namespace Lawllit.Site.Models;

// Tudo de um jogo. O hub mostra o resumo de cinco destes e a página do jogo mostra um inteiro,
// então a regra de exibição mora aqui e as duas views só percorrem o que chega pronto.
public sealed class PainelJogoViewModel
{
    public required Jogo Jogo { get; init; }

    // Horário de Brasília, o mesmo em que a API entrega as datas.
    public required DateTime Agora { get; init; }

    // Nulo quando o jogo não tem aquela parte, e não quando a fonte falhou.
    public Resultado<AgendaModel>? Agenda { get; init; }
    public Resultado<ShrineModel>? Shrine { get; init; }
    public Resultado<BaroModel>? Baro { get; init; }

    public List<AtualizacaoModel> Atualizacoes { get; init; } = [];

    public List<DiaAgendaViewModel> ProximasPorDia => PorDia(Agenda?.Valor?.Proximas);
    public List<DiaAgendaViewModel> ResultadosPorDia => PorDia(Agenda?.Valor?.Resultados);

    public int QuantidadeAoVivo => Agenda?.Valor?.Proximas.Count(AoVivo) ?? 0;

    // O card do hub mostra a partida que está rolando, ou a próxima se nenhuma estiver.
    public PartidaViewModel? Destaque => Agenda?.Valor?.Proximas
        .OrderByDescending(AoVivo)
        .Select(partida => new PartidaViewModel(partida, AoVivo(partida)))
        .FirstOrDefault();

    public AtualizacaoModel? Patch => Atualizacoes.FirstOrDefault(atualizacao => atualizacao.Jogo == Jogo.Codigo);

    public bool BaroPresente => Baro?.Valor is { } baro && baro.Chegada <= Agora && Agora < baro.Partida;

    public string Quando(DateTime data) => $"{RotuloDia(data.Date)} às {data:HH:mm}";

    private List<DiaAgendaViewModel> PorDia(List<PartidaModel>? partidas)
        => [.. (partidas ?? [])
            .GroupBy(partida => partida.Inicio.Date)
            .Select(dia => new DiaAgendaViewModel(
                Maiuscula(RotuloDia(dia.Key)),
                [.. dia.Select(partida => new PartidaViewModel(partida, AoVivo(partida)))]))];

    // A API lê a fonte de 15 em 15 minutos, então "ao vivo" é calculado agora e não lá.
    private bool AoVivo(PartidaModel partida) => !partida.Concluida && partida.Inicio <= Agora;

    // O rótulo do dia abre o grupo na agenda, mas no meio da frase, como em "troca hoje", fica minúsculo.
    private static string Maiuscula(string texto) => string.Concat(char.ToUpperInvariant(texto[0]).ToString(), texto[1..]);

    private string RotuloDia(DateTime dia) => (dia - Agora.Date).Days switch
    {
        0 => "hoje",
        1 => "amanhã",
        -1 => "ontem",

        // O pt-BR abrevia com ponto, "qua.", que ao lado da data vira pontuação sobrando.
        _ => $"{dia.ToString("ddd").TrimEnd('.')} {dia:dd/MM}",
    };
}

public sealed record DiaAgendaViewModel(string Rotulo, List<PartidaViewModel> Partidas);

public sealed record PartidaViewModel(PartidaModel Partida, bool AoVivo)
{
    public string Hora => Partida.Inicio.ToString("HH:mm");
}

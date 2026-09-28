using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Jogos.Contratos;
using System.Collections.Concurrent;

namespace Lawllit.Api.Jogos.Services;

// Guarda a última agenda boa de cada jogo. Quem enche é o AgendaSincronizacaoService, em
// segundo plano, então a requisição da página nunca espera a Liquipedia.
public sealed class AgendaService : IAgendaService
{
    private readonly ConcurrentDictionary<JogoEnum, AgendaModel> agendas = new();

    public Resultado<AgendaModel> Listar(JogoEnum jogo)
    {
        if (!AgendaModel.JogosDisponiveis.Contains(jogo))
            return Resultado<AgendaModel>.Falha("A agenda só existe para Valorant, LoL e Dota 2.");

        // Só acontece logo depois de subir, antes da primeira leitura de cada jogo terminar.
        return agendas.TryGetValue(jogo, out var agenda)
            ? Resultado<AgendaModel>.Ok(agenda)
            : Resultado<AgendaModel>.Falha("A agenda ainda está carregando. Volte daqui a pouco.");
    }

    public void Guardar(AgendaModel agenda) => agendas[agenda.Jogo] = agenda;
}

#region Interfaces

public interface IAgendaService
{
    Resultado<AgendaModel> Listar(JogoEnum jogo);
    void Guardar(AgendaModel agenda);
}

#endregion

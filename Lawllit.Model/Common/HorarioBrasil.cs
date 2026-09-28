namespace Lawllit.Model.Common;

// Toda fonte de jogo entrega horário em UTC, e a página mostra tudo no horário de Brasília.
public static class HorarioBrasil
{
    private static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTime Agora => DeUtc(DateTime.UtcNow);

    public static DateTime DeUtc(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Fuso);

    public static DateTime DeUnix(long segundos)
        => DeUtc(DateTimeOffset.FromUnixTimeSeconds(segundos).UtcDateTime);
}

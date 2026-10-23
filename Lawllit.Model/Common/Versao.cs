namespace Lawllit.Model.Common;

// O /health diz qual commit está no ar, para conferir um deploy sem abrir o painel da Railway.
public static class Versao
{
    public static readonly string Atual = Environment.GetEnvironmentVariable("RAILWAY_GIT_COMMIT_SHA") is { Length: >= 7 } sha
        ? sha[..7]
        : "local";
}

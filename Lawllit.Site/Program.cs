using Lawllit.Site;
using Lawllit.Site.Common;
using Lawllit.Web;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args).UseHospedagemLawllit();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

if (builder.Environment.IsDevelopment())
    TurnstileChaves.AplicarPadraoDesenvolvimento(builder.Configuration);

builder.Services.AddLawllitSite(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // Falha na subida em vez de deixar o formulário de contato aberto sem verificação.
    foreach (var chave in TurnstileChaves.Obrigatorias)
    {
        if (string.IsNullOrWhiteSpace(app.Configuration[chave]))
            throw new InvalidOperationException($"{chave.Replace(":", "__")} não configurada no ambiente.");
    }
}

app.UseLawllitWeb(Seguranca.PoliticaConteudo, "/Inicio/Erro");

app.UseRouting();

// Depois do UseRouting: o [EnableRateLimiting] é metadado do endpoint, e antes da rota
// resolvida o limitador não enxerga política nenhuma e deixa tudo passar.
app.UseRateLimiter();

app.MapSeoLawllit(SeoCaminhos.Indexaveis, SeoCaminhos.Bloqueados);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Index}/{id?}");

app.Run();

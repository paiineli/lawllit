using Lawllit.Finance.Site;
using Lawllit.Finance.Site.Common;
using Lawllit.Web;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args).UseHospedagemLawllit();

// Explícito porque a descoberta automática não pega as views compiladas do Lawllit.Web.
builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(WebExtensions).Assembly)
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddLawllitFinanceSite(builder.Configuration, builder.Environment.IsDevelopment());

var app = builder.Build();

app.UseLawllitWeb(Seguranca.PoliticaConteudo, "/Erro");

app.UseRouting();

// Depois do UseRouting: o [EnableRateLimiting] é metadado do endpoint, e antes da rota
// resolvida o limitador não enxerga política nenhuma e deixa tudo passar.
app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapSeoLawllit(SeoCaminhos.Indexaveis, SeoCaminhos.Bloqueados);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Index}/{id?}");

app.Run();

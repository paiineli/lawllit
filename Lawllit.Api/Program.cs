using System.Globalization;
using Lawllit.Api;
using Lawllit.Api.Common;
using Lawllit.Api.Contato.Endpoints;
using Lawllit.Api.Finance.Endpoints;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

// Sem isto a cultura cai na do processo, que num contêiner é invariante, e a data sai errada.
var culturaPadrao = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaPadrao;
CultureInfo.DefaultThreadCurrentUICulture = culturaPadrao;

// Liga CD_TRANSACAO em CdTransacao, no processo inteiro e não por consulta.
DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// A rede privada da Railway é IPv6: em 0.0.0.0 os sites não alcançam a API.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://[::]:{port}");

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddLawllitApi(builder.Configuration);

if (builder.Environment.IsDevelopment())
    builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Mantém a claim como 'sub' em vez de reescrever para o nome longo do ClaimTypes.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = ConfiguracaoJwt.Emissor,
            ValidateAudience = true,
            ValidAudience = ConfiguracaoJwt.Audiencia,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(ConfiguracaoJwt.LerChaveSecreta(builder.Configuration))),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = options.DefaultPolicy);

var app = builder.Build();

// Falha na subida em vez de aceitar requisição sem a chave compartilhada configurada.
if (string.IsNullOrWhiteSpace(app.Configuration["Api:Key"]))
    throw new InvalidOperationException("Api__Key não configurada no ambiente.");

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" }))
    .AllowAnonymous()
    .ExcludeFromDescription();

var api = app.MapGroup("api").AddEndpointFilter<ChaveApiFilter>();

api.MapGroup("financas/autenticacao").AllowAnonymous().MapAutenticacao();
api.MapGroup("financas/categorias").RequireAuthorization().MapCategorias();
api.MapGroup("financas/transacoes").RequireAuthorization().MapTransacoes();
api.MapGroup("financas/painel").RequireAuthorization().MapPainel();
api.MapGroup("financas/perfil").RequireAuthorization().MapPerfil();
api.MapGroup("financas/cotacoes").RequireAuthorization().MapCotacoes();

// Anônimo porque quem escreve não tem conta; a chave compartilhada acima continua valendo.
api.MapGroup("contato").AllowAnonymous().MapContato();

app.Run();

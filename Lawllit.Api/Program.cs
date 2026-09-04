using Lawllit.Api;
using Lawllit.Api.Common;
using Lawllit.Api.Finance.Endpoints;
using Lawllit.Model.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// A rede privada da Railway é IPv6, então o bind precisa ser em [::] e não em 0.0.0.0,
// senão o Site não alcança a API pelo domínio interno.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://[::]:{port}");

builder.Services.AddLocalization();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddOpenApi();
builder.Services.AddLawllitApi();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // MapInboundClaims desligado para a claim continuar se chamando 'sub',
        // em vez de ser reescrita para o nome longo do ClaimTypes.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(JwtSettings.ReadSecretKey(builder.Configuration))),
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

var api = app.MapGroup("api").AddEndpointFilter<ApiKeyEndpointFilter>();

api.MapGroup("auth").AllowAnonymous().MapAuth();
api.MapGroup("categories").RequireAuthorization().MapCategories();
api.MapGroup("transactions").RequireAuthorization().MapTransactions();
api.MapGroup("dashboard").RequireAuthorization().MapDashboard();
api.MapGroup("profile").RequireAuthorization().MapProfile();
api.MapGroup("quotes").RequireAuthorization().MapQuotes();
api.MapGroup("welcome").RequireAuthorization().MapWelcome();

app.Run();

using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Finance.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuth(this RouteGroupBuilder builder)
    {
        builder.MapPost("/login", LoginAsync)
            .WithSummary("Valida e-mail e senha e devolve o usuário com o token de acesso")
            .WithTags("Auth");

        builder.MapPost("/register", RegisterAsync)
            .WithSummary("Cria a conta e dispara o e-mail de confirmação")
            .WithTags("Auth");

        builder.MapGet("/confirm-email/{token}", ConfirmEmailAsync)
            .WithSummary("Confirma o e-mail pelo token e já autentica o usuário")
            .WithTags("Auth");

        builder.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithSummary("Gera o token de redefinição e envia o e-mail de recuperação")
            .WithTags("Auth");

        builder.MapGet("/reset-token/{token}", ValidateResetTokenAsync)
            .WithSummary("Informa se o token de redefinição ainda é válido")
            .WithTags("Auth");

        builder.MapPost("/reset-password", ResetPasswordAsync)
            .WithSummary("Troca a senha a partir do token de redefinição")
            .WithTags("Auth");

        builder.MapPost("/google", LoginWithGoogleAsync)
            .WithSummary("Autentica pelo Google, criando ou vinculando a conta")
            .WithTags("Auth");

        return builder;
    }

    private static async Task<Results<Ok<AuthResultMOD>, BadRequest<ApiErrorMOD>>> LoginAsync(
        LoginMOD login,
        IAuthService authService,
        CancellationToken cancellationToken)
        => (await authService.LoginAsync(login, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> RegisterAsync(
        RegisterMOD register,
        IAuthService authService,
        CancellationToken cancellationToken)
        => (await authService.RegisterAsync(register, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok<AuthResultMOD>, BadRequest<ApiErrorMOD>>> ConfirmEmailAsync(
        string token,
        IAuthService authService,
        CancellationToken cancellationToken)
        => (await authService.ConfirmEmailAsync(token, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> ForgotPasswordAsync(
        ForgotPasswordMOD forgotPassword,
        IAuthService authService,
        CancellationToken cancellationToken)
        => (await authService.ForgotPasswordAsync(forgotPassword, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok, NotFound>> ValidateResetTokenAsync(
        string token,
        IAuthService authService,
        CancellationToken cancellationToken)
        => await authService.IsPasswordResetTokenValidAsync(token, cancellationToken)
            ? TypedResults.Ok()
            : TypedResults.NotFound();

    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> ResetPasswordAsync(
        ResetPasswordMOD resetPassword,
        IAuthService authService,
        CancellationToken cancellationToken)
        => (await authService.ResetPasswordAsync(resetPassword, cancellationToken)).ToHttpResult();

    private static async Task<Ok<AuthResultMOD>> LoginWithGoogleAsync(
        GoogleUserMOD googleUser,
        IAuthService authService,
        CancellationToken cancellationToken)
        => TypedResults.Ok(await authService.LoginWithGoogleAsync(googleUser, cancellationToken));
}

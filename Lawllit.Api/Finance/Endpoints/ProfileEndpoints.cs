using Lawllit.Api.Common;
using Lawllit.Api.Finance.Services;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lawllit.Api.Finance.Endpoints;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfile(this RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetAsync)
            .WithSummary("Retorna os dados de perfil e as preferências do usuário")
            .WithTags("Profile");

        builder.MapPatch("/name", EditNameAsync)
            .WithSummary("Altera o nome de exibição")
            .WithTags("Profile");

        builder.MapPatch("/email", EditEmailAsync)
            .WithSummary("Altera o e-mail, confirmando a senha quando a conta tem senha")
            .WithTags("Profile");

        builder.MapPatch("/password", ChangePasswordAsync)
            .WithSummary("Troca a senha validando a senha atual")
            .WithTags("Profile");

        builder.MapPatch("/preference", SavePreferenceAsync)
            .WithSummary("Salva uma preferência de tema, fonte, idioma ou moeda")
            .WithTags("Profile");

        builder.MapDelete("/", DeleteAccountAsync)
            .WithSummary("Exclui a conta e todo o dado vinculado")
            .WithTags("Profile");

        return builder;
    }

    private static async Task<Results<Ok<ProfileMOD>, NotFound>> GetAsync(
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken)
    {
        var profile = await profileService.GetAsync(user.GetUserId(), cancellationToken);

        return profile is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(profile);
    }

    private static async Task<Results<Ok<UserMOD>, BadRequest<ApiErrorMOD>>> EditNameAsync(
        EditNameMOD editName,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken)
        => (await profileService.EditNameAsync(user.GetUserId(), editName, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok<UserMOD>, BadRequest<ApiErrorMOD>>> EditEmailAsync(
        EditEmailMOD editEmail,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken)
        => (await profileService.EditEmailAsync(user.GetUserId(), editEmail, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> ChangePasswordAsync(
        ChangePasswordMOD changePassword,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken)
        => (await profileService.ChangePasswordAsync(user.GetUserId(), changePassword, cancellationToken)).ToHttpResult();

    private static async Task<Results<Ok<UserMOD>, BadRequest<ApiErrorMOD>>> SavePreferenceAsync(
        PreferenceMOD preference,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken)
        => (await profileService.SavePreferenceAsync(user.GetUserId(), preference, cancellationToken)).ToHttpResult();

    // O [FromBody] é obrigatório aqui. Minimal API não infere corpo em DELETE,
    // e sem o atributo a aplicação nem sobe.
    private static async Task<Results<Ok, BadRequest<ApiErrorMOD>>> DeleteAccountAsync(
        [FromBody] DeleteAccountMOD deleteAccount,
        ClaimsPrincipal user,
        IProfileService profileService,
        CancellationToken cancellationToken)
        => (await profileService.DeleteAccountAsync(user.GetUserId(), deleteAccount, cancellationToken)).ToHttpResult();
}

using Lawllit.Model.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Common;

// Converte o Result de negócio em resposta HTTP. Falha de regra sai como 400 com a
// ErrorKey, e quem traduz é o Site, que conhece a cultura do usuário.
public static class ResultExtensions
{
    public static Results<Ok, BadRequest<ApiErrorMOD>> ToHttpResult(this Result result)
        => result.IsSuccess
            ? TypedResults.Ok()
            : TypedResults.BadRequest(new ApiErrorMOD { ErrorKey = result.ErrorKey! });

    public static Results<Ok<TValue>, BadRequest<ApiErrorMOD>> ToHttpResult<TValue>(this Result<TValue> result)
        => result.IsSuccess
            ? TypedResults.Ok(result.Value!)
            : TypedResults.BadRequest(new ApiErrorMOD { ErrorKey = result.ErrorKey! });
}

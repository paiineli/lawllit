using Lawllit.Model.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Lawllit.Api.Common;

public static class ResultadoExtensions
{
    public static Results<Ok, BadRequest<ErroApiModel>> ParaHttp(this Resultado resultado)
        => resultado.Sucesso
            ? TypedResults.Ok()
            : TypedResults.BadRequest(new ErroApiModel { Mensagem = resultado.Mensagem! });

    public static Results<Ok<TValor>, BadRequest<ErroApiModel>> ParaHttp<TValor>(this Resultado<TValor> resultado)
        => resultado.Sucesso
            ? TypedResults.Ok(resultado.Valor!)
            : TypedResults.BadRequest(new ErroApiModel { Mensagem = resultado.Mensagem! });
}

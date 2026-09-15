using Lawllit.Model.Contato.Contratos;
using Lawllit.Repository.Contato;
using Lawllit.Site.Common;
using Lawllit.Site.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lawllit.Site.Controllers;

public class ContatoController(
    IContatoRepository contatoRepositorio,
    ITurnstileValidador turnstileValidador) : Controller
{
    private const string Ancora = "contato";

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(DependencyContainer.PoliticaLimiteContato)]
    public async Task<IActionResult> Enviar(ContatoViewModel formulario, CancellationToken cancellationToken)
    {
        // Isca preenchida é robô. Devolve sucesso para não ensinar o robô a contornar.
        if (!string.IsNullOrWhiteSpace(formulario.Site))
            return Enviado();

        if (!ModelState.IsValid)
            return Voltar(formulario, PrimeiroErro());

        var ehGente = await turnstileValidador.Validar(
            Request.Form["cf-turnstile-response"],
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        if (!ehGente)
            return Voltar(formulario, "A verificação de segurança não passou. Tente enviar de novo.");

        var resultado = await contatoRepositorio.Enviar(new EnviarMensagemModel
        {
            Nome = formulario.Nome,
            Email = formulario.Email,
            Mensagem = formulario.Mensagem,
        }, cancellationToken);

        if (!resultado.Sucesso)
            return Voltar(formulario, resultado.Mensagem!);

        return Enviado();
    }

    // TempData porque a resposta é um redirect, senão um F5 reenviaria o formulário.
    private IActionResult Enviado()
    {
        TempData["ContatoSucesso"] = "Mensagem enviada. Respondo assim que possível.";
        return VoltarParaInicio();
    }

    private IActionResult Voltar(ContatoViewModel formulario, string erro)
    {
        TempData["ContatoErro"] = erro;
        TempData["ContatoNome"] = formulario.Nome;
        TempData["ContatoEmail"] = formulario.Email;
        TempData["ContatoMensagem"] = formulario.Mensagem;

        return VoltarParaInicio();
    }

    private IActionResult VoltarParaInicio()
        => RedirectToAction("Index", "Inicio", routeValues: null, fragment: Ancora);

    private string PrimeiroErro()
        => ModelState.Values
            .SelectMany(entrada => entrada.Errors)
            .Select(erro => erro.ErrorMessage)
            .FirstOrDefault(mensagem => !string.IsNullOrWhiteSpace(mensagem))
            ?? "Dados inválidos.";
}

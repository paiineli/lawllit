using Lawllit.Finance.Site.Models;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Finance;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Finance.Site.Controllers;

[Authorize]
public class PerfilController(IPerfilRepository perfilRepositorio) : BaseController
{
    private const string AbaSeguranca = "seguranca";
    private const string AbaConta = "conta";

    [HttpGet]
    public async Task<IActionResult> Index(string? aba, CancellationToken cancellationToken)
    {
        var perfil = await perfilRepositorio.Consultar(cancellationToken);

        if (perfil is null)
            return RedirectToAction("Sair", "Conta");

        return View(PerfilViewModel.De(perfil, aba));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarNome(AlterarNomeViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return VoltarComErro(PrimeiroErro("Nome inválido."), aba: null);

        var resultado = await perfilRepositorio.AlterarNome(
            new AlterarNomeModel { Nome = formulario.Nome },
            cancellationToken);

        if (!resultado.Sucesso)
            return VoltarComErro(resultado.Mensagem!, aba: null);

        await Reautenticar(resultado.Valor!);
        return VoltarComSucesso("Nome alterado.", aba: null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarEmail(AlterarEmailViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return VoltarComErro(PrimeiroErro("E-mail inválido."), AbaSeguranca);

        var resultado = await perfilRepositorio.AlterarEmail(
            new AlterarEmailModel { Email = formulario.Email, Senha = formulario.Senha },
            cancellationToken);

        if (!resultado.Sucesso)
            return VoltarComErro(resultado.Mensagem!, AbaSeguranca);

        await Reautenticar(resultado.Valor!);
        return VoltarComSucesso("E-mail alterado.", AbaSeguranca);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return VoltarComErro(PrimeiroErro("Dados inválidos."), AbaSeguranca);

        var resultado = await perfilRepositorio.AlterarSenha(new AlterarSenhaModel
        {
            SenhaAtual = formulario.SenhaAtual,
            NovaSenha = formulario.NovaSenha,
        }, cancellationToken);

        return resultado.Sucesso
            ? VoltarComSucesso("Senha alterada.", AbaSeguranca)
            : VoltarComErro(resultado.Mensagem!, AbaSeguranca);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarPreferencia(string chave, string valor, CancellationToken cancellationToken)
    {
        var resultado = await perfilRepositorio.SalvarPreferencia(
            new PreferenciaModel { Chave = chave ?? string.Empty, Valor = valor ?? string.Empty },
            cancellationToken);

        if (!resultado.Sucesso)
            return BadRequest();

        await Reautenticar(resultado.Valor!);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirConta(string? senha, CancellationToken cancellationToken)
    {
        var resultado = await perfilRepositorio.ExcluirConta(
            new ExcluirContaModel { Senha = senha },
            cancellationToken);

        if (!resultado.Sucesso)
            return VoltarComErro(resultado.Mensagem!, AbaConta);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Inicio");
    }

    private IActionResult VoltarComSucesso(string mensagem, string? aba)
    {
        TempData["Sucesso"] = mensagem;
        return VoltarParaIndex(aba);
    }

    private IActionResult VoltarComErro(string mensagem, string? aba)
    {
        TempData["Erro"] = mensagem;
        return VoltarParaIndex(aba);
    }

    private IActionResult VoltarParaIndex(string? aba)
        => aba is null
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Index), new { aba });

    // A mensagem já vem do DataAnnotations, a reserva só cobre o caso de não haver nenhuma.
    private string PrimeiroErro(string reserva)
        => ModelState.Values
            .SelectMany(entrada => entrada.Errors)
            .FirstOrDefault()?.ErrorMessage ?? reserva;
}

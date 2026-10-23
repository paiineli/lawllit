using Lawllit.Finance.Site.Models;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Finance;
using Lawllit.Web.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Finance.Site.Controllers;

public sealed class PerfilController(IPerfilRepository perfilRepositorio) : BaseController
{
    private const string AbaSeguranca = "seguranca";
    private const string AbaConta = "conta";

    [HttpGet]
    public async Task<IActionResult> Index(string? aba, CancellationToken cancellationToken)
    {
        var perfil = await perfilRepositorio.Consultar(cancellationToken);

        // Conta que sumiu com a sessão aberta: encerra aqui, porque o Sair da Conta só aceita POST.
        if (perfil is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Entrar", "Conta");
        }

        return View(PerfilViewModel.De(perfil, aba));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarNome(AlterarNomeViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return VoltarComErro(ModelState.PrimeiroErro("Nome inválido."));

        var resultado = await perfilRepositorio.AlterarNome(
            new AlterarNomeModel { Nome = formulario.Nome },
            cancellationToken);

        if (!resultado.Sucesso)
            return VoltarComErro(resultado.Mensagem!);

        await Reautenticar(resultado.Valor!);
        return VoltarComSucesso("Nome alterado.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarEmail(AlterarEmailViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return VoltarComErro(ModelState.PrimeiroErro("E-mail inválido."), new { aba = AbaSeguranca });

        var resultado = await perfilRepositorio.AlterarEmail(
            new AlterarEmailModel { Email = formulario.Email, Senha = formulario.Senha },
            cancellationToken);

        if (!resultado.Sucesso)
            return VoltarComErro(resultado.Mensagem!, new { aba = AbaSeguranca });

        await Reautenticar(resultado.Valor!);
        return VoltarComSucesso("E-mail alterado.", new { aba = AbaSeguranca });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return VoltarComErro(ModelState.PrimeiroErro("Dados inválidos."), new { aba = AbaSeguranca });

        var resultado = await perfilRepositorio.AlterarSenha(new AlterarSenhaModel
        {
            SenhaAtual = formulario.SenhaAtual,
            NovaSenha = formulario.NovaSenha,
        }, cancellationToken);

        return resultado.Sucesso
            ? VoltarComSucesso("Senha alterada.", new { aba = AbaSeguranca })
            : VoltarComErro(resultado.Mensagem!, new { aba = AbaSeguranca });
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
}

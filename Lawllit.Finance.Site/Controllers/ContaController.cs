using Lawllit.Finance.Site.Common;
using Lawllit.Finance.Site.Models;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Finance;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Lawllit.Finance.Site.Controllers;

[AllowAnonymous]
public class ContaController(IAutenticacaoRepository autenticacaoRepositorio, EnderecoFinance endereco) : BaseController
{
    [HttpGet]
    public IActionResult Entrar()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Painel");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(DependencyContainer.PoliticaLimiteAcesso)]
    public async Task<IActionResult> Entrar(EntrarViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(formulario);

        var resultado = await autenticacaoRepositorio.Entrar(
            new LoginModel { Email = formulario.Email, Senha = formulario.Senha },
            cancellationToken);

        if (!resultado.Sucesso)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensagem!);
            return View(formulario);
        }

        return await AutenticarERedirecionar(resultado.Valor!);
    }

    [HttpGet]
    public IActionResult Cadastrar()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Painel");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(DependencyContainer.PoliticaLimiteAcesso)]
    public async Task<IActionResult> Cadastrar(CadastrarViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(formulario);

        var resultado = await autenticacaoRepositorio.Cadastrar(new CadastroModel
        {
            Nome = formulario.Nome,
            Email = formulario.Email,
            Senha = formulario.Senha,
            MoldeUrlConfirmacao = MontarUrlAbsoluta(nameof(ConfirmarEmail)),
        }, cancellationToken);

        if (!resultado.Sucesso)
        {
            ModelState.AddModelError(nameof(formulario.Email), resultado.Mensagem!);
            return View(formulario);
        }

        TempData["Sucesso"] = "Conta criada! Verifique seu e-mail para ativar o acesso.";
        return RedirectToAction(nameof(Entrar));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmarEmail(string token, CancellationToken cancellationToken)
    {
        var resultado = await autenticacaoRepositorio.ConfirmarEmail(token, cancellationToken);

        if (!resultado.Sucesso)
        {
            TempData["Erro"] = resultado.Mensagem!;
            return RedirectToAction(nameof(Entrar));
        }

        return await AutenticarERedirecionar(resultado.Valor!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Inicio");
    }

    [HttpGet]
    public IActionResult EsqueciSenha() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(DependencyContainer.PoliticaLimiteAcesso)]
    public async Task<IActionResult> EsqueciSenha(EsqueciSenhaViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(formulario);

        // Resposta ignorada de propósito: mensagem igual nos dois casos, senão a tela vira
        // consulta de quem tem cadastro.
        await autenticacaoRepositorio.EsqueciSenha(new EsqueciSenhaModel
        {
            Email = formulario.Email,
            MoldeUrlRedefinicao = MontarUrlAbsoluta(nameof(RedefinirSenha)),
        }, cancellationToken);

        TempData["Sucesso"] = "Se o e-mail estiver cadastrado, você receberá as instruções em breve.";
        return RedirectToAction(nameof(Entrar));
    }

    [HttpGet]
    public async Task<IActionResult> RedefinirSenha(string token, CancellationToken cancellationToken)
    {
        if (!await autenticacaoRepositorio.TokenSenhaValido(token, cancellationToken))
        {
            TempData["Erro"] = "Link inválido ou expirado.";
            return RedirectToAction(nameof(Entrar));
        }

        return View(new RedefinirSenhaViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(DependencyContainer.PoliticaLimiteAcesso)]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(formulario);

        var resultado = await autenticacaoRepositorio.RedefinirSenha(
            new RedefinirSenhaModel { Token = formulario.Token, NovaSenha = formulario.Senha },
            cancellationToken);

        if (!resultado.Sucesso)
        {
            TempData["Erro"] = resultado.Mensagem!;
            return RedirectToAction(nameof(Entrar));
        }

        TempData["Sucesso"] = "Senha redefinida. Faça login.";
        return RedirectToAction(nameof(Entrar));
    }

    private async Task<IActionResult> AutenticarERedirecionar(AutenticacaoModel autenticacao)
    {
        await Autenticar(autenticacao.Usuario, autenticacao.Token, new DateTimeOffset(DateTime.SpecifyKind(autenticacao.ExpiraEm, DateTimeKind.Utc)));
        return RedirectToAction("Index", "Painel");
    }

    // Só o Site conhece as próprias rotas. O {token} é substituído do outro lado.
    // A origem vem da configuração, e não do Host da requisição, que quem chama controla.
    private string MontarUrlAbsoluta(string action)
        => endereco.Origem + Url.Action(action, "Conta", new { token = "__TOKEN__" })!
            .Replace("__TOKEN__", "{token}");
}

using Lawllit.Repository.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Finance.Site.Controllers;

[Authorize]
public class CotacoesController(ICotacaoRepository cotacaoRepositorio) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var resultado = await cotacaoRepositorio.Listar(cancellationToken);

        // A API já logou o motivo técnico da falha, aqui só sobra a mensagem ao usuário.
        if (!resultado.Sucesso)
            TempData["Erro"] = resultado.Mensagem!;

        return View(resultado.Valor ?? []);
    }
}

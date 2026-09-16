using Lawllit.Finance.Site.Models;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lawllit.Finance.Site.Controllers;

[Authorize]
public class CategoriaController(ICategoriaRepository categoriaRepositorio) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(TipoTransacaoEnum? tipo, string? busca, CancellationToken cancellationToken)
    {
        var filtro = new CategoriaFiltroModel { Tipo = tipo, Busca = busca };
        var categorias = await categoriaRepositorio.Listar(filtro, cancellationToken);

        return View(new CategoriaListaViewModel
        {
            Categorias = categorias,
            FiltroTipo = tipo,
            FiltroBusca = busca,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CategoriaFormViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return VoltarComErro("Nome da categoria é obrigatório.");

        var resultado = await categoriaRepositorio.Criar(
            new CategoriaSalvarModel { Nome = formulario.Nome, Tipo = formulario.Tipo },
            cancellationToken);

        return resultado.Sucesso
            ? VoltarComSucesso("Categoria criada.")
            : VoltarComErro(resultado.Mensagem!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alterar(Guid codigo, CategoriaFormViewModel formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return VoltarComErro("Nome da categoria é obrigatório.");

        var resultado = await categoriaRepositorio.Alterar(
            new CategoriaSalvarModel { Codigo = codigo, Nome = formulario.Nome, Tipo = formulario.Tipo },
            cancellationToken);

        return resultado.Sucesso
            ? VoltarComSucesso("Categoria alterada.")
            : VoltarComErro(resultado.Mensagem!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inativar(Guid codigo, CancellationToken cancellationToken)
    {
        var resultado = await categoriaRepositorio.Inativar(codigo, cancellationToken);

        return resultado.Sucesso
            ? VoltarComSucesso("Categoria removida.")
            : VoltarComErro(resultado.Mensagem!);
    }

    private IActionResult VoltarComSucesso(string mensagem)
    {
        TempData["Sucesso"] = mensagem;
        return RedirectToAction(nameof(Index));
    }

    private IActionResult VoltarComErro(string mensagem)
    {
        TempData["Erro"] = mensagem;
        return RedirectToAction(nameof(Index));
    }
}

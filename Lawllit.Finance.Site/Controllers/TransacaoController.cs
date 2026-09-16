using Lawllit.Finance.Site.Common;
using Lawllit.Finance.Site.Models;
using Lawllit.Model.Common;
using Lawllit.Model.Common.Enums;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;
using Lawllit.Repository.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace Lawllit.Finance.Site.Controllers;

[Authorize]
public class TransacaoController(
    ITransacaoRepository transacaoRepositorio,
    ICategoriaRepository categoriaRepositorio) : BaseController
{
    private const int ItensPorPagina = 50;

    [HttpGet]
    public async Task<IActionResult> Index(
        TipoTransacaoEnum? tipo,
        int? mes,
        int? ano,
        string? busca,
        int pagina,
        CancellationToken cancellationToken)
    {
        var filtro = new TransacaoFiltroModel
        {
            Tipo = tipo,
            Mes = mes,
            Ano = ano,
            Busca = busca,
            Paginacao = new Paginacao { PaginaAtual = Math.Max(pagina, 1), ItensPorPagina = ItensPorPagina },
        };

        var paginaTransacoes = await transacaoRepositorio.Listar(filtro, cancellationToken);
        var categorias = await categoriaRepositorio.Listar(new CategoriaFiltroModel(), cancellationToken);

        return View(new TransacaoListaViewModel
        {
            Pagina = paginaTransacoes.Pagina,
            Categorias = categorias,
            FiltroTipo = tipo,
            FiltroBusca = busca,
            FiltroMes = paginaTransacoes.Mes,
            FiltroAno = paginaTransacoes.Ano,
            TotalReceitas = paginaTransacoes.TotalReceitas,
            TotalDespesas = paginaTransacoes.TotalDespesas,
            TotalInvestimentos = paginaTransacoes.TotalInvestimentos,
            RecorrentesPendentes = paginaTransacoes.RecorrentesPendentes,
        });
    }

    [HttpGet]
    public async Task<IActionResult> Exportar(
        TipoTransacaoEnum? tipo,
        int? mes,
        int? ano,
        string? busca,
        CancellationToken cancellationToken)
    {
        var filtro = new TransacaoFiltroModel { Tipo = tipo, Mes = mes, Ano = ano, Busca = busca };
        var transacoes = await transacaoRepositorio.ListarTodas(filtro, cancellationToken);

        var nomeArquivo = $"lawllit-{ano ?? DateTime.Now.Year}-{(mes ?? DateTime.Now.Month):00}.csv";

        return File(MontarCsv(transacoes), "text/csv", nomeArquivo);
    }

    // Ponto e vírgula e vírgula decimal, que é o que o Excel em pt-BR espera, senão ele joga
    // tudo numa coluna só. O BOM na frente evita acento quebrado ao abrir o arquivo.
    private static byte[] MontarCsv(List<TransacaoModel> transacoes)
    {
        var cultura = CultureInfo.GetCultureInfo("pt-BR");
        var csv = new StringBuilder();

        csv.Append('﻿');
        csv.AppendLine(string.Join(';',
            "Data",
            "Descrição",
            "Categoria",
            "Tipo",
            "Transação recorrente",
            "Valor"));

        foreach (var transacao in transacoes)
        {
            csv.AppendLine(string.Join(';',
                transacao.DtTransacao.ToString("dd/MM/yyyy", cultura),
                Escapar(transacao.TxDescricao),
                Escapar(transacao.Categoria?.NmCategoria ?? string.Empty),
                transacao.TxTipo.Rotulo(),
                transacao.Recorrente ? "Sim" : "Não",
                transacao.VlTransacao.ToString("F2", cultura)));
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    // Campo com separador, aspas ou quebra vira campo entre aspas, e aspas interna dobra.
    private static string Escapar(string valor)
    {
        if (!valor.Contains(';') && !valor.Contains('"') && !valor.Contains('\n'))
            return valor;

        return $"\"{valor.Replace("\"", "\"\"")}\"";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(
        TransacaoFormViewModel formulario,
        [Bind(Prefix = "filtro")] TransacaoFiltroRotaViewModel filtro,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return VoltarComErro("Dados inválidos.", filtro);

        var resultado = await transacaoRepositorio.Criar(ParaSalvar(formulario), cancellationToken);

        return resultado.Sucesso
            ? VoltarComSucesso("Transação criada.", filtro)
            : VoltarComErro(resultado.Mensagem!, filtro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alterar(
        Guid codigo,
        TransacaoFormViewModel formulario,
        [Bind(Prefix = "filtro")] TransacaoFiltroRotaViewModel filtro,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return VoltarComErro("Dados inválidos.", filtro);

        formulario.Codigo = codigo;
        var resultado = await transacaoRepositorio.Alterar(ParaSalvar(formulario), cancellationToken);

        return resultado.Sucesso
            ? VoltarComSucesso("Transação alterada.", filtro)
            : VoltarComErro(resultado.Mensagem!, filtro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inativar(
        Guid codigo,
        [Bind(Prefix = "filtro")] TransacaoFiltroRotaViewModel filtro,
        CancellationToken cancellationToken)
    {
        var resultado = await transacaoRepositorio.Inativar(codigo, cancellationToken);

        return resultado.Sucesso
            ? VoltarComSucesso("Transação removida.", filtro)
            : VoltarComErro(resultado.Mensagem!, filtro);
    }

    // Importa no mês que está em tela, que é o do aviso de pendência, e não no mês corrente.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportarRecorrentes(
        [Bind(Prefix = "filtro")] TransacaoFiltroRotaViewModel filtro,
        CancellationToken cancellationToken)
    {
        var agora = DateTime.Now;
        var quantidade = await transacaoRepositorio.ImportarRecorrentes(
            new ImportarRecorrentesModel { Mes = filtro.Mes ?? agora.Month, Ano = filtro.Ano ?? agora.Year },
            cancellationToken);

        TempData["Sucesso"] = $"{quantidade} transação(ões) recorrente(s) importada(s).";
        return RedirectToAction(nameof(Index), filtro.ParaValoresRota());
    }

    private static TransacaoSalvarModel ParaSalvar(TransacaoFormViewModel formulario) => new()
    {
        Codigo = formulario.Codigo,
        Descricao = formulario.Descricao,
        Valor = formulario.Valor,
        Tipo = formulario.Tipo,
        Data = formulario.Data,
        CodigoCategoria = formulario.CodigoCategoria,
        Recorrente = formulario.Recorrente,
    };

    private IActionResult VoltarComSucesso(string mensagem, TransacaoFiltroRotaViewModel filtro)
    {
        TempData["Sucesso"] = mensagem;
        return RedirectToAction(nameof(Index), filtro.ParaValoresRota());
    }

    private IActionResult VoltarComErro(string mensagem, TransacaoFiltroRotaViewModel filtro)
    {
        TempData["Erro"] = mensagem;
        return RedirectToAction(nameof(Index), filtro.ParaValoresRota());
    }
}

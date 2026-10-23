using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;

namespace Lawllit.Api.Finance.Services;

public sealed class CategoriaService(ICategoriaRepository categoriaRepositorio) : ICategoriaService
{
    public async Task<List<CategoriaItemModel>> Listar(Guid cdUsuario, CategoriaFiltroModel filtro, CancellationToken cancellationToken)
        => [.. (await categoriaRepositorio.Listar(cdUsuario, filtro, cancellationToken)).Select(CategoriaItemModel.De)];

    public async Task<Resultado> Criar(Guid cdUsuario, CategoriaSalvarModel categoria, CancellationToken cancellationToken)
    {
        if (categoria.Validar() is { } erro)
            return Resultado.Falha(erro);

        var nome = categoria.Nome.Trim();

        if (await categoriaRepositorio.Existe(cdUsuario, nome, categoria.Tipo, cdIgnorar: null, cancellationToken))
            return Resultado.Falha("Já existe uma categoria com este nome e tipo.");

        await categoriaRepositorio.Criar(new CategoriaModel
        {
            CdCategoria = Guid.NewGuid(),
            NmCategoria = nome,
            TxTipo = categoria.Tipo,
            CdUsuario = cdUsuario,
        }, cancellationToken);

        return Resultado.Ok();
    }

    public async Task<Resultado> Alterar(Guid cdUsuario, CategoriaSalvarModel categoria, CancellationToken cancellationToken)
    {
        if (categoria.Validar() is { } erro)
            return Resultado.Falha(erro);

        var existente = await categoriaRepositorio.BuscarPorCodigo(cdUsuario, categoria.Codigo, cancellationToken);
        if (existente is null)
            return Resultado.Falha("Categoria não encontrada.");

        var nome = categoria.Nome.Trim();

        if (await categoriaRepositorio.Existe(cdUsuario, nome, categoria.Tipo, cdIgnorar: categoria.Codigo, cancellationToken))
            return Resultado.Falha("Já existe uma categoria com este nome e tipo.");

        existente.NmCategoria = nome;
        existente.TxTipo = categoria.Tipo;
        return await categoriaRepositorio.Alterar(existente, cancellationToken)
            ? Resultado.Ok()
            : Resultado.Falha("Categoria não encontrada.");
    }

    public async Task<Resultado> Inativar(Guid cdUsuario, Guid cdCategoria, CancellationToken cancellationToken)
    {
        var categoria = await categoriaRepositorio.BuscarPorCodigo(cdUsuario, cdCategoria, cancellationToken);
        if (categoria is null)
            return Resultado.Falha("Categoria não encontrada.");

        return await categoriaRepositorio.AlternarAtivo(cdUsuario, cdCategoria, Constantes.Nao, cancellationToken)
            ? Resultado.Ok()
            : Resultado.Falha("Categoria não encontrada.");
    }
}

#region Interfaces

public interface ICategoriaService
{
    Task<List<CategoriaItemModel>> Listar(Guid cdUsuario, CategoriaFiltroModel filtro, CancellationToken cancellationToken);
    Task<Resultado> Criar(Guid cdUsuario, CategoriaSalvarModel categoria, CancellationToken cancellationToken);
    Task<Resultado> Alterar(Guid cdUsuario, CategoriaSalvarModel categoria, CancellationToken cancellationToken);
    Task<Resultado> Inativar(Guid cdUsuario, Guid cdCategoria, CancellationToken cancellationToken);
}

#endregion

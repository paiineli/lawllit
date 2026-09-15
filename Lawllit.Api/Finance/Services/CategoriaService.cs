using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;

namespace Lawllit.Api.Finance.Services;

public sealed class CategoriaService(ICategoriaRepository categoriaRepositorio) : ICategoriaService
{
    public Task<List<CategoriaModel>> Listar(Guid cdUsuario, CategoriaFiltroModel filtro, CancellationToken cancellationToken)
        => categoriaRepositorio.Listar(cdUsuario, filtro, cancellationToken);

    public Task<CategoriaModel?> BuscarPorCodigo(Guid cdUsuario, Guid cdCategoria, CancellationToken cancellationToken)
        => categoriaRepositorio.BuscarPorCodigo(cdUsuario, cdCategoria, cancellationToken);

    public async Task<Resultado> Criar(Guid cdUsuario, CategoriaSalvarModel categoria, CancellationToken cancellationToken)
    {
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
        var existente = await categoriaRepositorio.BuscarPorCodigo(cdUsuario, categoria.Codigo, cancellationToken);
        if (existente is null)
            return Resultado.Falha("Categoria não encontrada.");

        var nome = categoria.Nome.Trim();

        if (await categoriaRepositorio.Existe(cdUsuario, nome, categoria.Tipo, cdIgnorar: categoria.Codigo, cancellationToken))
            return Resultado.Falha("Já existe uma categoria com este nome e tipo.");

        existente.NmCategoria = nome;
        existente.TxTipo = categoria.Tipo;
        await categoriaRepositorio.Alterar(existente, cancellationToken);

        return Resultado.Ok();
    }

    public async Task<Resultado> Inativar(Guid cdUsuario, Guid cdCategoria, CancellationToken cancellationToken)
    {
        var categoria = await categoriaRepositorio.BuscarPorCodigo(cdUsuario, cdCategoria, cancellationToken);
        if (categoria is null)
            return Resultado.Falha("Categoria não encontrada.");

        await categoriaRepositorio.AlternarAtivo(cdUsuario, cdCategoria, Constantes.Nao, cancellationToken);
        return Resultado.Ok();
    }
}

#region Interfaces

public interface ICategoriaService
{
    Task<List<CategoriaModel>> Listar(Guid cdUsuario, CategoriaFiltroModel filtro, CancellationToken cancellationToken);
    Task<CategoriaModel?> BuscarPorCodigo(Guid cdUsuario, Guid cdCategoria, CancellationToken cancellationToken);
    Task<Resultado> Criar(Guid cdUsuario, CategoriaSalvarModel categoria, CancellationToken cancellationToken);
    Task<Resultado> Alterar(Guid cdUsuario, CategoriaSalvarModel categoria, CancellationToken cancellationToken);
    Task<Resultado> Inativar(Guid cdUsuario, Guid cdCategoria, CancellationToken cancellationToken);
}

#endregion

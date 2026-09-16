using Lawllit.Api.Common;
using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contratos;

namespace Lawllit.Api.Finance.Services;

public sealed class PerfilService(IUsuarioRepository usuarioRepositorio) : IPerfilService
{
    // A validação fica aqui para o Site não ser a única barreira.
    private static readonly Dictionary<string, (string[] ValoresValidos, Action<UsuarioModel, string> Aplicar)> PreferenciasAceitas = new()
    {
        ["tema"] = (Constantes.TemasValidos, static (usuario, valor) => usuario.TxTema = valor),
        ["tamanhoFonte"] = (Constantes.TamanhosFonteValidos, static (usuario, valor) => usuario.TxTamanhoFonte = valor),
    };

    public async Task<PerfilModel?> Consultar(Guid cdUsuario, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.BuscarPorCodigo(cdUsuario, cancellationToken);
        if (usuario is null) return null;

        return new PerfilModel
        {
            Nome = usuario.NmUsuario,
            Email = usuario.TxEmail,
            TemSenha = usuario.TemSenha,
            MembroDesde = usuario.DtCadastro,
            Tema = usuario.TxTema,
            TamanhoFonte = usuario.TxTamanhoFonte,
        };
    }

    public async Task<Resultado<UsuarioModel>> AlterarNome(Guid cdUsuario, AlterarNomeModel alterar, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.BuscarPorCodigo(cdUsuario, cancellationToken);
        if (usuario is null)
            return Resultado<UsuarioModel>.Falha("Operação não permitida.");

        usuario.NmUsuario = Textos.ParaTitulo(alterar.Nome);
        await usuarioRepositorio.Alterar(usuario, cancellationToken);

        return Resultado<UsuarioModel>.Ok(usuario);
    }

    public async Task<Resultado<UsuarioModel>> AlterarEmail(Guid cdUsuario, AlterarEmailModel alterar, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.BuscarPorCodigo(cdUsuario, cancellationToken);
        if (usuario is null)
            return Resultado<UsuarioModel>.Falha("Operação não permitida.");

        if (!Senha.Confere(alterar.Senha, usuario.TxSenha))
            return Resultado<UsuarioModel>.Falha("Senha incorreta.");

        var email = alterar.Email.Trim().ToLower();

        if (usuario.TxEmail == email)
            return Resultado<UsuarioModel>.Falha("O novo e-mail é igual ao atual.");

        var existente = await usuarioRepositorio.BuscarPorEmail(email, cancellationToken);
        if (existente is not null)
            return Resultado<UsuarioModel>.Falha("Este e-mail já está em uso.");

        usuario.TxEmail = email;
        await usuarioRepositorio.Alterar(usuario, cancellationToken);

        return Resultado<UsuarioModel>.Ok(usuario);
    }

    public async Task<Resultado> AlterarSenha(Guid cdUsuario, AlterarSenhaModel alterar, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.BuscarPorCodigo(cdUsuario, cancellationToken);
        if (usuario is null || usuario.TxSenha is null)
            return Resultado.Falha("Operação não permitida.");

        if (!Senha.Confere(alterar.SenhaAtual, usuario.TxSenha))
            return Resultado.Falha("Senha atual incorreta.");

        usuario.TxSenha = Senha.Gerar(alterar.NovaSenha);
        await usuarioRepositorio.Alterar(usuario, cancellationToken);

        return Resultado.Ok();
    }

    public async Task<Resultado<UsuarioModel>> SalvarPreferencia(Guid cdUsuario, PreferenciaModel preferencia, CancellationToken cancellationToken)
    {
        if (!PreferenciasAceitas.TryGetValue(preferencia.Chave, out var aceita) || !aceita.ValoresValidos.Contains(preferencia.Valor))
            return Resultado<UsuarioModel>.Falha("Dados inválidos.");

        var usuario = await usuarioRepositorio.BuscarPorCodigo(cdUsuario, cancellationToken);
        if (usuario is null)
            return Resultado<UsuarioModel>.Falha("Operação não permitida.");

        aceita.Aplicar(usuario, preferencia.Valor);
        await usuarioRepositorio.Alterar(usuario, cancellationToken);

        return Resultado<UsuarioModel>.Ok(usuario);
    }

    public async Task<Resultado> ExcluirConta(Guid cdUsuario, ExcluirContaModel excluir, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepositorio.BuscarPorCodigo(cdUsuario, cancellationToken);
        if (usuario is null)
            return Resultado.Falha("Operação não permitida.");

        if (usuario.TxSenha is not null && !Senha.Confere(excluir.Senha, usuario.TxSenha))
            return Resultado.Falha("Senha incorreta.");

        await usuarioRepositorio.Excluir(cdUsuario, cancellationToken);

        return Resultado.Ok();
    }
}

#region Interfaces

public interface IPerfilService
{
    Task<PerfilModel?> Consultar(Guid cdUsuario, CancellationToken cancellationToken);
    Task<Resultado<UsuarioModel>> AlterarNome(Guid cdUsuario, AlterarNomeModel alterar, CancellationToken cancellationToken);
    Task<Resultado<UsuarioModel>> AlterarEmail(Guid cdUsuario, AlterarEmailModel alterar, CancellationToken cancellationToken);
    Task<Resultado> AlterarSenha(Guid cdUsuario, AlterarSenhaModel alterar, CancellationToken cancellationToken);
    Task<Resultado<UsuarioModel>> SalvarPreferencia(Guid cdUsuario, PreferenciaModel preferencia, CancellationToken cancellationToken);
    Task<Resultado> ExcluirConta(Guid cdUsuario, ExcluirContaModel excluir, CancellationToken cancellationToken);
}

#endregion

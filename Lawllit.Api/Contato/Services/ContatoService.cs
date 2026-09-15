using Lawllit.Api.Common;
using Lawllit.Api.Contato.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Contato;
using Lawllit.Model.Contato.Contratos;
using System.Net;

namespace Lawllit.Api.Contato.Services;

public sealed class ContatoService(
    IContatoRepository contatoRepositorio,
    IEnviadorEmail enviador,
    IConfiguration configuration,
    ILogger<ContatoService> logger) : IContatoService
{
    public async Task<Resultado> Enviar(EnviarMensagemModel requisicao, CancellationToken cancellationToken)
    {
        var nome = requisicao.Nome.Trim();
        var email = requisicao.Email.Trim();
        var texto = requisicao.Mensagem.Trim();

        if (nome.Length == 0 || email.Length == 0 || texto.Length == 0)
            return Resultado.Falha("Dados inválidos.");

        var contato = new ContatoModel
        {
            CdContato = Guid.NewGuid(),
            NmContato = nome,
            TxEmail = email,
            TxMensagem = texto,
        };

        // Grava antes de avisar, para o Resend fora do ar não custar a mensagem.
        await contatoRepositorio.Criar(contato, cancellationToken);

        try
        {
            await Notificar(contato, cancellationToken);
        }
        catch (Exception excecao)
        {
            // A mensagem já está salva, então falha de e-mail vira log e não erro na tela.
            logger.LogError(excecao, "Mensagem de contato {Codigo} foi salva mas o aviso por e-mail falhou.", contato.CdContato);
        }

        return Resultado.Ok();
    }

    private Task Notificar(ContatoModel contato, CancellationToken cancellationToken)
    {
        var destino = configuration["Contact:To"]
            ?? throw new InvalidOperationException("Contact__To não configurado no ambiente.");

        return enviador.Enviar(
            destino,
            $"lawllit, mensagem de {contato.NmContato}",
            MontarCorpo(contato),
            cancellationToken,
            responderPara: contato.TxEmail);
    }

    private static string MontarCorpo(ContatoModel contato)
    {
        // Tudo que veio do formulário entra escapado, e a quebra de linha vira <br> depois disso.
        // O e-mail vai como texto e não como mailto: link para outro domínio cheira a spam.
        var nome = WebUtility.HtmlEncode(contato.NmContato);
        var email = WebUtility.HtmlEncode(contato.TxEmail);
        var texto = WebUtility.HtmlEncode(contato.TxMensagem).Replace("\n", "<br>");

        var card = $"""
            <h2 style="margin:0 0 24px 0;font-size:16px;font-weight:600;color:#e5e7eb;">Nova mensagem pelo site</h2>

                          <p style="margin:0 0 4px 0;font-size:11px;color:#6b7280;letter-spacing:0.06em;">DE</p>
                          <p style="margin:0 0 18px 0;font-size:14px;color:#e5e7eb;">{nome}</p>

                          <p style="margin:0 0 4px 0;font-size:11px;color:#6b7280;letter-spacing:0.06em;">E-MAIL</p>
                          <p style="margin:0 0 18px 0;font-size:14px;color:#4ade80;">{email}</p>

                          <p style="margin:0 0 4px 0;font-size:11px;color:#6b7280;letter-spacing:0.06em;">MENSAGEM</p>
                          <p style="margin:0;font-size:14px;color:#e5e7eb;line-height:1.7;">{texto}</p>
            """;

        return ModeloEmail.Montar(ModeloEmail.MarcaLawllit, card);
    }
}

#region Interfaces

public interface IContatoService
{
    Task<Resultado> Enviar(EnviarMensagemModel requisicao, CancellationToken cancellationToken);
}

#endregion

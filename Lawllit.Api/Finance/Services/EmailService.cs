using Lawllit.Api.Common;
using Lawllit.Model.Finance;

namespace Lawllit.Api.Finance.Services;

public sealed class EmailService(IEnviadorEmail enviador) : IEmailService
{
    public Task EnviarConfirmacao(UsuarioModel usuario, string urlConfirmacao, CancellationToken cancellationToken)
        => Enviar(usuario, "Confirme seu e-mail", cancellationToken,
            saudacao: $"Olá, {usuario.NmUsuario}! Clique no botão abaixo para ativar sua conta no lawllit.finance.",
            urlAcao: urlConfirmacao,
            rotuloAcao: "Confirmar e-mail",
            rodape1: "Este link expira em 24 horas.",
            rodape2: "Se você não criou uma conta, ignore este e-mail.");

    public Task EnviarRedefinicaoSenha(UsuarioModel usuario, string urlRedefinicao, CancellationToken cancellationToken)
        => Enviar(usuario, "Redefinição de senha", cancellationToken,
            saudacao: $"Olá, {usuario.NmUsuario}! Clique no botão abaixo para redefinir sua senha no lawllit.finance.",
            urlAcao: urlRedefinicao,
            rotuloAcao: "Redefinir senha",
            rodape1: "Este link expira em 1 hora.",
            rodape2: "Se você não solicitou a redefinição, ignore este e-mail.");

    private Task Enviar(
        UsuarioModel usuario,
        string titulo,
        CancellationToken cancellationToken,
        string saudacao,
        string urlAcao,
        string rotuloAcao,
        string rodape1,
        string rodape2)
    {
        var card = $"""
            <h2 style="margin:0 0 8px 0;font-size:18px;font-weight:600;color:#e5e7eb;">{titulo}</h2>
                          <p style="margin:0 0 32px 0;font-size:14px;color:#6b7280;line-height:1.6;">{saudacao}</p>
                          <table width="100%" cellpadding="0" cellspacing="0">
                            <tr>
                              <td align="center">
                                <a href="{urlAcao}"
                                   style="display:inline-block;padding:13px 36px;background-color:#4ade80;color:#000000;font-weight:700;font-size:14px;text-decoration:none;border-radius:6px;letter-spacing:-0.2px;">
                                  {rotuloAcao}
                                </a>
                              </td>
                            </tr>
                          </table>
            """;

        var corpo = ModeloEmail.Montar(ModeloEmail.MarcaFinance, card, ModeloEmail.Rodape(rodape1, rodape2));

        return enviador.Enviar(usuario.TxEmail, $"{titulo} — lawllit.finance", corpo, cancellationToken);
    }
}

#region Interfaces

public interface IEmailService
{
    Task EnviarConfirmacao(UsuarioModel usuario, string urlConfirmacao, CancellationToken cancellationToken);
    Task EnviarRedefinicaoSenha(UsuarioModel usuario, string urlRedefinicao, CancellationToken cancellationToken);
}

#endregion

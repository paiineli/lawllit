namespace Lawllit.Api.Common;

// A moldura dos e-mails do projeto, para o do financeiro e o do contato não saírem
// diferentes quando um dos dois for mexido.
public static class ModeloEmail
{
    // Cliente de e-mail não carrega fonte da web, então o visual novo vai em Helvetica e Arial.
    public const string Fonte = "Helvetica,Arial,sans-serif";
    public const string CorTexto = "#181512";
    public const string CorSuave = "#5f574e";
    public const string CorDestaque = "#ff5a1f";
    public const string CorSobreDestaque = "#170a04";

    public const string MarcaFinance = $"lawllit<span style=\"color:{CorDestaque};\">.</span><span style=\"color:{CorSuave};font-weight:700;\">finance</span>";
    public const string MarcaLawllit = $"lawllit<span style=\"color:{CorDestaque};\">.</span>";

    public static string Montar(string marca, string card, string rodape = "")
        => $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width"></head>
            <body style="margin:0;padding:0;background-color:#f4f1ec;font-family:{Fonte};">
              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f1ec;padding:48px 20px;">
                <tr>
                  <td align="center">
                    <table width="100%" cellpadding="0" cellspacing="0" style="max-width:500px;">

                      <tr>
                        <td align="center" style="padding-bottom:28px;">
                          <span style="font-size:26px;font-weight:800;color:{CorTexto};letter-spacing:-1px;">{marca}</span>
                        </td>
                      </tr>

                      <tr>
                        <td style="background-color:#ffffff;border:1px solid rgba(30,20,10,0.09);border-radius:20px;padding:36px 32px;">
                          {card}
                        </td>
                      </tr>
                      {rodape}
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

    public static string Rodape(string linha1, string linha2)
        => $"""
            <tr>
                        <td style="padding:24px 0 0 0;text-align:center;">
                          <p style="margin:0 0 4px 0;font-size:12px;color:{CorSuave};">{linha1}</p>
                          <p style="margin:0;font-size:12px;color:{CorSuave};">{linha2}</p>
                        </td>
                      </tr>
            """;
}

namespace Lawllit.Api.Common;

// A moldura dos e-mails do projeto, para o do financeiro e o do contato não saírem
// diferentes quando um dos dois for mexido.
public static class ModeloEmail
{
    public const string MarcaFinance = "lawllit<span style=\"color:#4ade80;\">.finance</span>";
    public const string MarcaLawllit = "lawllit<span style=\"color:#6b7280;\">.</span>";

    public static string Montar(string marca, string card, string rodape = "")
        => $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width"></head>
            <body style="margin:0;padding:0;background-color:#0a0a0a;font-family:Menlo,Monaco,Consolas,'Courier New',monospace;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#0a0a0a;padding:48px 20px;">
                <tr>
                  <td align="center">
                    <table width="100%" cellpadding="0" cellspacing="0" style="max-width:500px;">

                      <tr>
                        <td align="center" style="padding-bottom:28px;">
                          <span style="font-size:20px;font-weight:700;color:#e5e7eb;letter-spacing:-0.3px;">{marca}</span>
                        </td>
                      </tr>

                      <tr>
                        <td style="background-color:#111111;border:1px solid rgba(255,255,255,0.08);border-radius:12px;padding:36px 32px;">
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
                          <p style="margin:0 0 4px 0;font-size:12px;color:#6b7280;">{linha1}</p>
                          <p style="margin:0;font-size:12px;color:#6b7280;">{linha2}</p>
                        </td>
                      </tr>
            """;
}

using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lawllit.Api.Common;

public sealed class GeradorToken(IConfiguration configuration)
{
    public TokenJwtModel Gerar(UsuarioModel usuario)
    {
        var expiraEm = DateTime.UtcNow.Add(ConfiguracaoJwt.Validade);

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfiguracaoJwt.LerChaveSecreta(configuration))),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: ConfiguracaoJwt.Emissor,
            audience: ConfiguracaoJwt.Audiencia,
            claims: [new Claim(ConfiguracaoJwt.ClaimAssunto, usuario.CdUsuario.ToString())],
            expires: expiraEm,
            signingCredentials: credenciais);

        return new TokenJwtModel
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraEm = expiraEm,
        };
    }
}

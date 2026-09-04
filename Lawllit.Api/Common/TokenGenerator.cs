using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lawllit.Api.Common;

public sealed class TokenGenerator(IConfiguration configuration) : ITokenGenerator
{
    public JwtTokenMOD Generate(UserMOD user)
    {
        var expiresAt = DateTime.UtcNow.Add(JwtSettings.Lifetime);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.ReadSecretKey(configuration))),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtSettings.Issuer,
            audience: JwtSettings.Audience,
            claims: [new Claim(JwtRegisteredClaimNames.Subject, user.Id.ToString())],
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtTokenMOD
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
        };
    }
}

#region Interfaces

public interface ITokenGenerator
{
    JwtTokenMOD Generate(UserMOD user);
}

#endregion

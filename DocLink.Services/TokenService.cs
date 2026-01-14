using System.Security.Claims;
using DocLink.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using DocLink.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DocLink.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<Account> _userManager;

    public TokenService(IConfiguration configuration, UserManager<Account> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    public async Task<string> GenerateJwtToken(Account user)
    {
        if (string.IsNullOrEmpty(user.Email)) 
            throw new ArgumentException("User email cannot be null or empty");
        var roles = await _userManager.GetRolesAsync(user);
        var mainRole = roles.FirstOrDefault() ?? "Patient";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, mainRole),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
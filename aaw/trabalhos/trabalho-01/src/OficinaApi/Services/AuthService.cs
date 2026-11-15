using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OficinaApi.Data;
using OficinaApi.Models;

namespace OficinaApi.Services;

public class AuthService
{
    private static readonly string[] RolesPermitidas = ["admin", "mecanico"];

    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public AuthService(IUsuarioRepository usuarioRepo, IConfiguration config)
    {
        _usuarioRepo = usuarioRepo;
        _config = config;
    }

    public async Task<Usuario> RegistrarAsync(RegistrarRequest request)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var roleNormalizada = request.Role.Trim().ToLowerInvariant();

        if (!RolesPermitidas.Contains(roleNormalizada))
            throw new ArgumentException("Perfil inválido. Use 'admin' ou 'mecanico'.");

        var existente = await _usuarioRepo.ObterPorEmailAsync(emailNormalizado);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe um usuário cadastrado com o e-mail '{emailNormalizado}'.");

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = emailNormalizado,
            Role = roleNormalizada
        };

        usuario.SenhaHash = _passwordHasher.HashPassword(usuario, request.Senha);

        return await _usuarioRepo.CriarAsync(usuario);
    }

    public async Task<(string Token, DateTime ExpiraEm, Usuario Usuario)?> LoginAsync(LoginRequest request)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarioRepo.ObterPorEmailAsync(emailNormalizado);

        if (usuario is null)
            return null;

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Senha);
        if (resultado == PasswordVerificationResult.Failed)
            return null;

        var (token, expiraEm) = GerarToken(usuario);
        return (token, expiraEm, usuario);
    }

    private (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Chave JWT (JWT_KEY) não configurada.");

        var issuer = _config["Jwt:Issuer"] ?? "OficinaApi";
        var audience = _config["Jwt:Audience"] ?? "OficinaApiClientes";
        var minutos = int.TryParse(_config["Jwt:ExpiresInMinutes"], out var parsed) ? parsed : 60;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id ?? string.Empty),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var agora = DateTime.UtcNow;
        var expiraEm = agora.AddMinutes(minutos);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: agora,
            expires: expiraEm,
            signingCredentials: credenciais);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiraEm);
    }
}
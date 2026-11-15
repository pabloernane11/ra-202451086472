using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Models;
using OficinaApi.Services;

namespace OficinaApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Cadastra um novo usuário com perfil 'admin' ou 'mecanico'.</summary>
    [AllowAnonymous]
    [HttpPost("registrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarRequest request)
    {
        try
        {
            var usuario = await _authService.RegistrarAsync(request);
            return StatusCode(StatusCodes.Status201Created, new
            {
                id = usuario.Id,
                nome = usuario.Nome,
                email = usuario.Email,
                role = usuario.Role
            });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Autentica um usuário e retorna o token JWT.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var resultado = await _authService.LoginAsync(request);
        if (resultado is null)
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });

        var (token, expiraEm, usuario) = resultado.Value;
        return Ok(new
        {
            token,
            expiraEm,
            nome = usuario.Nome,
            email = usuario.Email,
            role = usuario.Role
        });
    }
}
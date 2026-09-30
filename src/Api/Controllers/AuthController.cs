using System.Security.Claims;
using AutoEscola.Api.Application.DTOs;
using AutoEscola.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoEscola.Api.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Autentica Aluno, Instrutor ou Admin via E-mail ou CPF e retorna o JWT + Perfil.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Altera a senha do usuário autenticado (usado também no fluxo de Primeiro Acesso).
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await _authService.ChangePasswordAsync(userId, request);
        return Ok(new { message = "Senha atualizada com sucesso." });
    }
}

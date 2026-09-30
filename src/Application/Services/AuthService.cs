using AutoEscola.Api.Application.DTOs;
using AutoEscola.Api.Domain.Exceptions;
using AutoEscola.Api.Infrastructure.Data;
using AutoEscola.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace AutoEscola.Api.Application.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly JwtTokenService _jwtService;

    public AuthService(AppDbContext context, JwtTokenService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var normalizedInput = request.EmailOrCpf.Trim().ToLower();
        var cleanCpf = new string(request.EmailOrCpf.Where(char.IsDigit).ToArray());

        var user = await _context.Users
            .Include(u => u.StudentProfile)
            .Include(u => u.InstructorProfile)
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == normalizedInput ||
                (!string.IsNullOrEmpty(cleanCpf) && u.Cpf == cleanCpf));

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Credenciais inválidas. Verifique seu e-mail/CPF e senha.", 401);

        if (!user.IsActive)
            throw new DomainException("Conta inativa. Entre em contato com a secretaria da Auto Escola.", 403);

        Guid? profileId = user.StudentProfile?.Id ?? user.InstructorProfile?.Id;

        var (accessToken, expiresIn) = _jwtService.GenerateAccessToken(user, profileId);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresIn: expiresIn,
            User: new UserSummaryDto(
                user.Id,
                profileId,
                user.Name,
                user.Email,
                user.Role.ToString(),
                user.MustChangePassword
            )
        );
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new DomainException("Usuário não encontrado.", 404);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new DomainException("A senha atual informada está incorreta.", 400);

        if (request.NewPassword.Length < 6)
            throw new DomainException("A nova senha deve ter no mínimo 6 caracteres.", 400);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.MustChangePassword = false;

        await _context.SaveChangesAsync();
    }
}

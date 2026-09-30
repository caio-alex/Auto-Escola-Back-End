namespace AutoEscola.Api.Application.DTOs;

public record LoginRequestDto(string EmailOrCpf, string Password);

public record RefreshTokenRequestDto(string AccessToken, string RefreshToken);

public record ChangePasswordRequestDto(string CurrentPassword, string NewPassword);

public record UserSummaryDto(
    Guid Id,
    Guid? ProfileId,
    string Name,
    string Email,
    string Role,
    bool MustChangePassword
);

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserSummaryDto User
);

namespace AuthService.Application.Features.Authentication.RotateRefreshToken;

public class RotateRefreshTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

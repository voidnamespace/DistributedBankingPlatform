namespace AuthService.Application.Commands.RotateRefreshToken;

public class RotateRefreshTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
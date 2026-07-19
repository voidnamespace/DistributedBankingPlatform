namespace AuthService.Application.Abstractions.Authentication;

public interface IRefreshTokenHasher
{
    string Hash(string refreshToken);
}

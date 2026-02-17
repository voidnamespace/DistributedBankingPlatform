using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
namespace AuthService.Application.Commands.LoginUser;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LoginUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LoginUserHandler(IUserRepository userRepository,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<LoginUserHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }


    public async Task<LoginResponse> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting login for {Email}", command.Email);

        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        var lowerEmail = command.Email.ToLower().Trim();

        var user = await _userRepository.GetByEmailAsync(lowerEmail, cancellationToken);

        if (user == null)
            throw new UnauthorizedAccessException("Incorrect email or password");

        if (!BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash!.Hash))
            throw new UnauthorizedAccessException("Incorrect email or password");


        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} successfully logged in", user.Id);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UserId = user.Id,
            Email = user.Email.Value,
            Role = user.Role.ToString(),
        };
    }
}

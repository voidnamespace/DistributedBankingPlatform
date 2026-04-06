using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.RegisterUser;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegisterUserHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "RegisterUserCommand started {Email}",
            command.Email);

        if (command.Password != command.ConfirmPassword)
        {
            _logger.LogWarning(
                "Password mismatch during registration {Email}",
                command.Email);

            throw new ArgumentException("The passwords don't match");
        }

        var user = new User(
            new EmailVO(command.Email.Trim()),
            new PasswordVO(command.Password));

        _logger.LogInformation(
            "User entity created {Email}",
            command.Email);

        await _userRepository.CreateAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
             "User saved to database {UserId}",
             user.Id);

        _logger.LogInformation(
            "RegisterUserCommand completed {UserId} {Email}",
            user.Id,
            user.Email.Value);

        return new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email.Value,
            Message = "Registration was successful"
        };
    }
}

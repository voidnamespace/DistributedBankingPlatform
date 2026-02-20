using AuthService.Application.Commands.RegisterUser;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using MediatR;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<RegisterResponse> Handle(
    RegisterUserCommand command,
    CancellationToken cancellationToken)
    {
        if (command.Password != command.ConfirmPassword)
            throw new ArgumentException("The passwords don't match");

        var user = new User(
            new EmailVO(command.Email.Trim()),
            new PasswordVO(command.Password));

        await _userRepository.CreateAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email.Value,
            Message = "Registration was successful"
        };
    }
}

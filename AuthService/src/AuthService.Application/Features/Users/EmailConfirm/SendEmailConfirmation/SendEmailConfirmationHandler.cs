using AuthService.Application.Abstractions.EmailConfirmation;
using AuthService.Application.Abstractions.Persistence;
using MediatR;

namespace AuthService.Application.Features.Users.EmailConfirm.SendEmailConfirmation;

public class SendEmailConfirmationHandler : IRequestHandler<SendEmailConfirmationCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;

    public SendEmailConfirmationHandler(
        IUserRepository userRepository, IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
    }

    public async Task Handle(SendEmailConfirmationCommand command, CancellationToken ct) 
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);

        if (user == null)
            throw new KeyNotFoundException("User not found"); 

        var email = user.Email.Value;

        if (user.EmailConfirmed)
        {
            return;
        }



        await _emailSender.SendAsync(
            email,
            ct);
    }
}

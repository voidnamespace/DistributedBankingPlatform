using AuthService.Application.Abstractions.EmailConfirmation;
using AuthService.Application.Abstractions.Persistence;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Application.Features.Users.EmailConfirm.SendEmailConfirmation;

public sealed class SendEmailConfirmationHandler : IRequestHandler<SendEmailConfirmationCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailConfirmationTokenStore _emailConfirmationTokenStore;
    private readonly IEmailSender _emailSender;

    public SendEmailConfirmationHandler(
        IUserRepository userRepository,
        IEmailConfirmationTokenStore emailConfirmationTokenStore,
        IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _emailConfirmationTokenStore = emailConfirmationTokenStore;
        _emailSender = emailSender;
    }

    public async Task Handle(SendEmailConfirmationCommand command, CancellationToken ct) 
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);

        if (user is null)
            throw new KeyNotFoundException("User not found");

        var email = user.Email.Value;

        if (user.EmailConfirmed)
            return;

        var token = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));

        var tokenHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token)));

        var tokenLifetime = TimeSpan.FromMinutes(15);

        await _emailConfirmationTokenStore.StoreAsync(
            tokenHash,
            user.Id,
            tokenLifetime,
            ct);

        var confirmationLink =
            "https://localhost:5001/api/auth/emailConfirm" +
            $"?token={Uri.EscapeDataString(token)}";

        var message =
            $"For mail confirmation follow the link: {confirmationLink}";

        await _emailSender.SendAsync(
            email,
            "Confirm your email",
            message,
            ct);
    }
}

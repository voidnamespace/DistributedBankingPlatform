using AuthService.Application.Abstractions.EmailConfirmation;
using AuthService.Application.Abstractions.Persistence;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Application.Features.Users.EmailConfirm.ConfirmEmail;

public sealed class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailConfirmationTokenStore _emailConfirmationTokenStore;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmEmailHandler(
        IEmailConfirmationTokenStore emailConfirmationTokenStore,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _emailConfirmationTokenStore = emailConfirmationTokenStore;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ConfirmEmailCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new KeyNotFoundException(
                "Email confirmation token is invalid or expired.");

        var tokenHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(command.Token)));

        var userId = await _emailConfirmationTokenStore.GetUserIdAsync(
            tokenHash,
            cancellationToken);

        if (userId is null)
            throw new KeyNotFoundException(
                "Email confirmation token is invalid or expired.");

        var user = await _userRepository.GetByIdAsync(
            userId.Value,
            cancellationToken);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        user.ConfirmEmail();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailConfirmationTokenStore.DeleteAsync(
            tokenHash,
            cancellationToken);
    }
}

using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Commands.Accounts;

public class CreateUserAccountProjectionHandler
    : IRequestHandler<CreateUserAccountProjectionCommand>
{
    private readonly IUserAccountRepository _userAccountRepository;


    public CreateUserAccountProjectionHandler(
        IUserAccountRepository userAccountRepository)
    {
        _userAccountRepository = userAccountRepository;
    }

    public async Task Handle(
        CreateUserAccountProjectionCommand command,
        CancellationToken cancellationToken)
    {
        var alreadyExists = await _userAccountRepository.GetByAccountNumberAsync(
            command.AccountNumber,
            cancellationToken);

        if (alreadyExists is not null)
            return;

        _userAccountRepository.Add(
            new UserAccount(
                command.AccountId,
                command.UserId,
                command.AccountNumber));

    }
}

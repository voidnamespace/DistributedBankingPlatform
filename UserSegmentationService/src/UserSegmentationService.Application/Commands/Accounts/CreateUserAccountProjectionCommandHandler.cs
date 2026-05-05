using MediatR;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Application.Commands.Accounts;

public class CreateUserAccountProjectionCommandHandler
    : IRequestHandler<CreateUserAccountProjectionCommand>
{
    private readonly IUserAccountRepository _userAccountRepository;

    public CreateUserAccountProjectionCommandHandler(
        IUserAccountRepository userAccountRepository)
    {
        _userAccountRepository = userAccountRepository;
    }

    public async Task Handle(
        CreateUserAccountProjectionCommand request,
        CancellationToken cancellationToken)
    {
        var alreadyExists = await _userAccountRepository.GetByAccountNumberAsync(
            request.AccountNumber,
            cancellationToken);

        if (alreadyExists is not null)
            return;

        _userAccountRepository.Add(
            new UserAccount(
                request.AccountId,
                request.UserId,
                request.AccountNumber));
    }
}

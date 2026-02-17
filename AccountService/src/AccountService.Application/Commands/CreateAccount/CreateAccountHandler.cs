using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using AccountService.Domain.Entity;
using AccountService.Domain.ValueObjects;
using MediatR;

namespace AccountService.Application.Commands.CreateAccount;

public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, ReadAccountDTO>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;


    public CreateAccountHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task<ReadAccountDTO> Handle(CreateAccountCommand command, CancellationToken ct)
    {
        AccountNumberVO accountNumberVO;
        do
        {
            accountNumberVO = AccountNumberVO.Generate();
        }
        while (await _accountRepository
            .ExistsByAccountNumberAsync(accountNumberVO, ct));
        var acc = new Account(command.request.UserId, accountNumberVO, command.request.Currency);
        await _accountRepository.AddAsync(acc, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return new ReadAccountDTO
        {
            Id = acc.Id,
            UserId = acc.UserId,
            AccountNumber = acc.AccountNumber.Value,
            BalanceAmount = acc.Balance.Amount,
            BalanceCurrency = acc.Balance.Currency,
            CreatedAt = acc.CreatedAt,
            UpdatedAt = acc.UpdatedAt,
            IsActive = acc.IsActive
        };
    }
}

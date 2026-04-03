using MediatR;

namespace AccountService.Application.Commands.WithdrawMoney;

public record WithdrawMoneyCommand(Guid TransactionId,
    string FromAccountNumber,
    decimal Amount,
    int Currency
) : IRequest;

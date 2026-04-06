using MediatR;

namespace AccountService.Application.Commands.DepositMoney;

public record DepositMoneyCommand (
    Guid TransactionId,
    string ToAccountNumber,
    decimal Amount, 
    int Currency) : IRequest;


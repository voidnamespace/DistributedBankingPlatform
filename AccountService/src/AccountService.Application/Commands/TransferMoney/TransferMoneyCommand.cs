using AccountService.Domain.Enums;
using MediatR;

namespace AccountService.Application.Commands.TransferMoney;

public record TransferMoneyCommand (Guid TransactionId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency Currency) : IRequest;


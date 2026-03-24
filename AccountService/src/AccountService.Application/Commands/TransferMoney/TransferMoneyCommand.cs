using AccountService.Domain.Enums;
using MediatR;

namespace AccountService.Application.Commands.TransferMoney;

public record TransferMoneyCommand (Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    Currency Currency) : IRequest;


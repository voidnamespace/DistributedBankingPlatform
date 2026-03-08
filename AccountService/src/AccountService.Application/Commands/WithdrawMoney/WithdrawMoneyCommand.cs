using AccountService.Application.DTOs;
using AccountService.Domain.Enums;
using MediatR;
namespace AccountService.Application.Commands.WithdrawMoney;

public record WithdrawMoneyCommand(
    decimal Amount,
    Currency Currency,
    string AccountNumber,
    Guid UserId
) : IRequest;

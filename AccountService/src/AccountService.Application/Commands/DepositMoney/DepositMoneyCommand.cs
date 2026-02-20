using AccountService.Domain.Enums;
using MediatR;
namespace AccountService.Application.Commands.DepositMoney;

public record DepositMoneyCommand (decimal Amount, Currency Currency, string AccountNumber) : IRequest;


using AccountService.Application.DTOs;
using MediatR;
namespace AccountService.Application.Commands.WithdrawMoney;

public record WithdrawMoneyCommand (WithdrawRequest request, string AccountNumber) : IRequest;

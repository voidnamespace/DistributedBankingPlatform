using AccountService.Application.DTOs;
using MediatR;
namespace AccountService.Application.Commands.DepositMoney;

public record DepositMoneyCommand (DepositRequest request, string AccountNumber) : IRequest;


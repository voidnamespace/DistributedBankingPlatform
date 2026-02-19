using AccountService.Domain.Enums;
using MediatR;
namespace AccountService.Application.Commands.CreateAccount;

public record CreateAccountCommand(
    Guid UserId,
    Currency Currency
) : IRequest;



using AccountService.Application.DTOs;
using MediatR;
namespace AccountService.Application.Commands.CreateAccount;

public record CreateAccountCommand (CreateAccountRequest request) : IRequest<ReadAccountDTO>;


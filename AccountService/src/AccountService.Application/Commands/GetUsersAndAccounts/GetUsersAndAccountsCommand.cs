using MediatR;

namespace AccountService.Application.Commands.GetUsersAndAccounts;

public sealed record GetUsersAndAccountsCommand(
    Guid RequestId,
    DateTime RequestedAt) : IRequest;


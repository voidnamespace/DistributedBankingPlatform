using AccountService.Application.DTOs;
using MediatR;
namespace AccountService.Application.Queries.GetAllAccounts;

public record GetAllAccountsQuery : IRequest<IReadOnlyList<ReadAccountDTO>>;


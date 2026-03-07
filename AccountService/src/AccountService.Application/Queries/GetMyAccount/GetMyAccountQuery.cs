using AccountService.Application.DTOs;
using MediatR;
namespace AccountService.Application.Queries.GetMyAccount;

public record GetMyAccountQuery(Guid UserId) : IRequest<IReadOnlyList<ReadAccountDTO>>;

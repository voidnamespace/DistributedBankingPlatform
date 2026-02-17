using AccountService.Application.DTOs;
using MediatR;
namespace AccountService.Application.Queries.GetByIdAccount;

public record GetByIdAccountQuery(Guid UserId) : IRequest<ReadAccountDTO>;

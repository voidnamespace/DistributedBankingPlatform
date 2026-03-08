using AccountService.Application.DTOs;
using MediatR;
namespace AccountService.Application.Queries.GetByAccountNumberAccount;

public record GetByAccountNumberAccountQuery(string AccountNumber, Guid UserId) : IRequest<ReadAccountDTO>;

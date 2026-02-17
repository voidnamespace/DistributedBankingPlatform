using AccountService.Application.DTOs;
using MediatR;
namespace AccountService.Application.Queries.GetByAccountNumberAccount;

public record GetByAccountNumberAccountQuery(string AccountId) : IRequest<ReadAccountDTO>;

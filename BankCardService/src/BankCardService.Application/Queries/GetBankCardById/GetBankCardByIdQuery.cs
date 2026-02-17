using BankCardService.Application.DTOs;
using MediatR;

namespace BankCardService.Application.Queries.GetBankCardById;

public record GetBankCardByIdQuery(Guid CardId)
    : IRequest<BankCardDTO>;

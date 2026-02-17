using BankCardService.Application.DTOs;
using MediatR;
namespace BankCardService.Application.Queries.GetAllBankCards;

public record GetAllBankCardsQuery 
    : IRequest<IEnumerable<BankCardDTO>>;


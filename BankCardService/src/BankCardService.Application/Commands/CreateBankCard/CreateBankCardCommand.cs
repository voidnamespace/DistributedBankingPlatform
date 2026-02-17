using BankCardService.Application.DTOs;
using MediatR;
namespace BankCardService.Application.Commands.CreateBankCard;

public record CreateBankCardCommand(string CardNumber, string CardHolder) : IRequest<BankCardDTO>;

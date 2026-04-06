using MediatR;

namespace TransactionService.Application.Commands.MarkDepositSuccess;

public sealed record MarkDepositSuccessCommand(Guid TransactionId) : IRequest;

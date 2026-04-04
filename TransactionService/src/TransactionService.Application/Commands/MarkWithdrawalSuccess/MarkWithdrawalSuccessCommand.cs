using MediatR;

namespace TransactionService.Application.Commands.MarkWithdrawalSuccess;

public sealed record MarkWithdrawalSuccessCommand(Guid TransactionId) : IRequest;


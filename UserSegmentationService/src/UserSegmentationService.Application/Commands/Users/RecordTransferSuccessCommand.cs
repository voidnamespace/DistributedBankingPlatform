using MediatR;

namespace UserSegmentationService.Application.Commands.Users;

public sealed record RecordTransferSuccessCommand(
    Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency,
    DateTime RecordedAt) : IRequest;

using MediatR;

namespace UserSegmentationService.Application.Commands.Accounts;

public sealed record CreateUserAccountProjectionCommand(
    Guid UserId,
    Guid AccountId,
    string AccountNumber) : IRequest;

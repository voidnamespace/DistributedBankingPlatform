using MediatR;

namespace UserSegmentationService.Application.Commands.Accounts.CreateProjection;

public sealed record CreateUserAccountProjectionCommand(
    Guid UserId,
    Guid AccountId,
    string AccountNumber) : IRequest;

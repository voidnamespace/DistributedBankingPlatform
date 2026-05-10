using MediatR;

namespace UserSegmentationService.Application.Commands.Simulation.LastTransactionAt;

public sealed record SimulateLastTransactionAtCommand(
    Guid UserId) : IRequest;

using MediatR;

namespace UserSegmentationService.Application.Commands.Simulation.SimulateBulkUserMetricUpdates;

public sealed record SimulateBulkUserMetricUpdatesCommand(
    int Count = 500) : IRequest;

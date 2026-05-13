using MediatR;

namespace UserSegmentationService.Application.Commands.Simulation.SimulateBulkUserMetricCreation;

public sealed record SimulateBulkUserMetricCreationCommand() : IRequest;

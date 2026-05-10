using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserSegmentationService.Application.Commands.Simulation.LastTransactionAt;
using UserSegmentationService.Application.Commands.Simulation.SimulateBulkUserMetricCreation;
using UserSegmentationService.Application.Commands.Simulation.SimulateBulkUserMetricUpdates;

namespace UserSegmentationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulationController : ControllerBase
{
    private readonly IMediator _mediator;

    public SimulationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("user-metrics/{userId:guid}/last-transaction-now")]
    public async Task<IActionResult> SimulateLastTransactionAt(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new SimulateLastTransactionAtCommand(userId),
            cancellationToken);

        return Ok();
    }

    [HttpPost("user-metrics/bulk-create")]
    public async Task<IActionResult> SimulateBulkUserMetricCreation(
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new SimulateBulkUserMetricCreationCommand(),
            cancellationToken);

        return Ok();
    }

    [HttpPost("user-metrics/bulk-update")]
    public async Task<IActionResult> SimulateBulkUserMetricUpdates(
        [FromQuery] int count = 500,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new SimulateBulkUserMetricUpdatesCommand(count),
            cancellationToken);

        return Ok();
    }
}

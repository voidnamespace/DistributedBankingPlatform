using Microsoft.AspNetCore.Mvc;
using MediatR;
using UserSegmentationService.Application.Commands.Simulation.LastTransactionAt;

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

}

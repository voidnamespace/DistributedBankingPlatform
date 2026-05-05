using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserSegmentationService.Application.Commands.Segments.ActiveUsers;

namespace UserSegmentationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SegmentationController : ControllerBase
{
    private readonly IMediator _mediator;

    public SegmentationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("active-users/evaluate")]
    public async Task<IActionResult> EvaluateActiveUsers(
        CancellationToken cancellationToken)
    {
        var activeSince = DateTime.UtcNow.AddDays(-30);

        await _mediator.Send(
            new EvaluateActiveUserSegmentCommand(activeSince),
            cancellationToken);

        return Ok();
    }
}

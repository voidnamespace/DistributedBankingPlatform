using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserSegmentationService.Application.Commands.Segments.ActiveUsers;
using UserSegmentationService.Application.Commands.Segments.RiskUsers;
using UserSegmentationService.Application.Commands.Segments.VipAtRiskUsers;
using UserSegmentationService.Application.Commands.Segments.VipUsers;

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

    [HttpPost("vip-users/evaluate")]
    public async Task<IActionResult> EvaluateVipUsers(
        [FromQuery] decimal minimumSpend = 5_000m,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new EvaluateVipUserSegmentCommand(minimumSpend),
            cancellationToken);

        return Ok();
    }

    [HttpPost("risk-users/evaluate")]
    public async Task<IActionResult> EvaluateRiskUsers(
        [FromQuery] int inactiveDays = 90,
        CancellationToken cancellationToken = default)
    {
        var inactiveSince = DateTime.UtcNow.AddDays(-inactiveDays);

        await _mediator.Send(
            new EvaluateRiskUserSegmentCommand(inactiveSince),
            cancellationToken);

        return Ok();
    }

    [HttpPost("vip-at-risk-users/evaluate")]
    public async Task <IActionResult> EvaluateVipAtRiskUsers(
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new EvaluateVipAtRiskUserSegmentCommand(),
            cancellationToken);

        return Ok();

    }


}

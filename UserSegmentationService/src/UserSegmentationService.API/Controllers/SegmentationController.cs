using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserSegmentationService.Application.Commands.Accounts.RequestBackfill;
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
    private readonly ILogger<SegmentationController> _logger;

    public SegmentationController(
        IMediator mediator, ILogger<SegmentationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("active-users/evaluate")]
    public async Task<IActionResult> EvaluateActiveUsers(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Evaluate active users started");

        var activeSince = DateTime.UtcNow.AddDays(-30);

        await _mediator.Send(
            new EvaluateActiveUserSegmentCommand(activeSince),
            cancellationToken);

        _logger.LogInformation(
            "Evaluate active users completed");

        return Ok();
    }

    [HttpPost("vip-users/evaluate")]
    public async Task<IActionResult> EvaluateVipUsers(
        CancellationToken cancellationToken,
        [FromQuery] decimal minimumSpend = 5_000m
        )
    {
        await _mediator.Send(
            new EvaluateVipUserSegmentCommand(minimumSpend),
            cancellationToken);

        return Ok();
    }

    [HttpPost("risk-users/evaluate")]
    public async Task<IActionResult> EvaluateRiskUsers(
        CancellationToken cancellationToken,
        [FromQuery] int inactiveDays = 90
        )
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

    [HttpPost("123")]
    public async Task <IActionResult> RequestBackFillUsers(
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new RequestUserAccountsBackfillCommand(),
            cancellationToken);
            
        return Ok();
    }



}

using AccountService.Application.Commands.ActivateAccount;
using AccountService.Application.Commands.CreateAccount;
using AccountService.Application.Commands.DeleteAccount;
using AccountService.Application.DTOs;
using AccountService.Application.Queries.GetAllAccounts;
using AccountService.Application.Queries.GetByAccountNumberAccount;
using AccountService.Application.Queries.GetByIdAccount;
using AccountService.Application.Queries.GetMyAccount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccountService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{

    private readonly IMediator _mediator;
    private readonly ILogger<AccountController> _logger;

    public AccountController (
        IMediator mediator, ILogger<AccountController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAccount(
        CreateAccountRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "CreateAccount request started for {userId}",
            userId);

        await _mediator.Send(
            new CreateAccountCommand(
                userId,
                request.Currency),
            ct);

        _logger.LogInformation(
            "CreateAccount request complete for {userId}",
            userId);

        return Accepted();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "GetAll request started for admin {userId}",
            userId);

        var result = await _mediator.Send(new GetAllAccountsQuery(), ct);

        _logger.LogInformation(
            "GetAll request complete for admin {userId}",
            userId);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("{accountId:guid}")]
    public async Task<IActionResult> GetById(Guid accountId, CancellationToken ct)
    {
        var tokenUserId = Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "GetById request started for {tokenUserId}",
            tokenUserId);

        var result = await _mediator.Send(new GetByIdAccountQuery(accountId, tokenUserId), ct);

        _logger.LogInformation(
            "GetById request completed for {tokenUserId}",
            tokenUserId);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("by-number/{accountNumber}")]
    public async Task<IActionResult> GetByAccountNumber(
        string accountNumber,
        CancellationToken ct)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "GetByAccountNumber request started for {userId}",
            userId);

        var result = await _mediator.Send(
            new GetByAccountNumberAccountQuery(accountNumber, userId),
            ct
        );

        _logger.LogInformation(
            "GetByAccountNumber request completed for {userId}",
            userId);

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{accountId:guid}")]
    public async Task <IActionResult> DeleteAccount(Guid accountId, CancellationToken ct)
    {
        var userId = Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "DeleteAccount request started for {userId}",
            userId);

        var command = new DeleteAccountCommand(accountId, userId);
        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "DeleteAccount request completed for {userId}",
            userId);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyAccount(CancellationToken ct)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "GetMyAccount request started for {userId}",
            userId);

        var result = await _mediator.Send(
            new GetMyAccountQuery(userId),
            ct);

        _logger.LogInformation(
            "GetMyAccount request completed for {userId}",
            userId);

        return Ok(result);
    }

    [Authorize]
    [HttpPatch("{accountId:guid}/activate")]
    public async Task<IActionResult> ActivateAccount(
    Guid accountId,
    CancellationToken ct)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "ActivateAccount request started for {userId}",
            userId);

        var command = new ActivateAccountCommand(accountId, userId);

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "ActivateAccount request completed for {userId}",
            userId);

        return NoContent();
    }

}

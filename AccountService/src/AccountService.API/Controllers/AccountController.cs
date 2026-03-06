using AccountService.Application.Commands.CreateAccount;
using AccountService.Application.Commands.DeleteAccount;
using AccountService.Application.Commands.DepositMoney;
using AccountService.Application.Commands.WithdrawMoney;
using AccountService.Application.DTOs;
using AccountService.Application.Queries.GetAllAccounts;
using AccountService.Application.Queries.GetByAccountNumberAccount;
using AccountService.Application.Queries.GetByIdAccount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.JsonWebTokens;
namespace AccountService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{

    private readonly IMediator _mediator;

    public AccountController (IMediator mediator)
    {
        _mediator = mediator; 
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAccount(
        CreateAccountRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);

        await _mediator.Send(
            new CreateAccountCommand(
                userId,
                request.Currency),
            ct);

        return Accepted();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllAccountsQuery(), ct);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{accountId:guid}")]
    public async Task<IActionResult> GetById(Guid accountId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetByIdAccountQuery(accountId), ct);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("by-number/{accountNumber}")]
    public async Task<IActionResult> GetByAccountNumber(string accountNumber, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetByAccountNumberAccountQuery(accountNumber),
            ct
        );

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{accountNumber}/deposit")]
    public async Task <IActionResult> Deposit(string accountNumber,
    [FromBody] DepositRequest request, CancellationToken ct)
    {
        var command = new DepositMoneyCommand(
            request.Amount,
            request.Currency,
            accountNumber);
        await _mediator.Send(command,ct);
        return Ok();
    }

    [Authorize]
    [HttpPatch("{accountNumber}/withdraw")]
    public async Task<IActionResult> Withdraw(
    string accountNumber,
    [FromBody] WithdrawRequest request,
    CancellationToken ct)
    {
        var command = new WithdrawMoneyCommand(
            request.Amount,
            request.Currency,
            accountNumber
        );

        await _mediator.Send(command, ct);

        return Ok();
    }

    [Authorize]
    [HttpDelete("{accountId:guid}")]
    public async Task <IActionResult> DeleteAccount(Guid accountId, CancellationToken ct)
    {
        var command = new DeleteAccountCommand(accountId);
        await _mediator.Send(command);
        return NoContent();
    }

}

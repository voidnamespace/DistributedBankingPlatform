using AccountService.Application.Commands.CreateAccount;
using AccountService.Application.Commands.DeleteAccount;
using AccountService.Application.Commands.DepositMoney;
using AccountService.Application.Commands.WithdrawMoney;
using AccountService.Application.DTOs;
using AccountService.Application.Queries.GetAllAccounts;
using AccountService.Application.Queries.GetByAccountNumberAccount;
using AccountService.Application.Queries.GetByIdAccount;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost]
    public async Task<IActionResult> CreateAccount(
    CreateAccountRequest request,
    CancellationToken ct)
    {
        await _mediator.Send(new CreateAccountCommand(request), ct);
        return Accepted();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllAccountsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{accountId:guid}")]
    public async Task<IActionResult> GetById(Guid accountId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetByIdAccountQuery(accountId), ct);
        return Ok(result);
    }

    [HttpGet("by-number/{accountNumber}")]
    public async Task<IActionResult> GetByAccountNumber(string accountNumber, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetByAccountNumberAccountQuery(accountNumber),
            ct
        );

        return Ok(result);
    }

    [HttpPost("{accountNumber}/deposit")]
    public async Task <IActionResult> Deposit(string accountNumber,
    [FromBody] DepositRequest request, CancellationToken ct)
    {
        await _mediator.Send(new DepositMoneyCommand(request, accountNumber),ct);
        return Ok();
    }

    [HttpPatch("{accountNumber}/withdraw")]
    public async Task <IActionResult> Withdraw (string accountNumber,
    [FromBody] WithdrawRequest request, CancellationToken ct)
    {
        await _mediator.Send(new WithdrawMoneyCommand(request, accountNumber), ct);
        return Ok();
    }

    [HttpDelete("{accountId:guid}")]
    public async Task <IActionResult> DeleteAccount(Guid accountId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteAccountCommand(accountId));
        return NoContent();
    }

}

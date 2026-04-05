using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TransactionService.Application.Commands.CreateDeposit;
using TransactionService.Application.Commands.CreateTransfer;
using TransactionService.Application.Commands.CreateWithdrawal;
using TransactionService.Application.DTOs;
using TransactionService.Application.Queries.CheckTransferStatus;

namespace TransactionService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
    [FromBody] CreateTransferRequest request,
    CancellationToken ct)
    {
        var initiatorId = Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (request.FromAccountNumber == request.ToAccountNumber)
            return BadRequest("Cannot transfer to the same account");
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than zero");

        var command = new CreateTransferCommand(
            initiatorId,
            request.FromAccountNumber,
            request.ToAccountNumber,
            request.Amount,
            request.Currency);

        var transactionId = await _mediator.Send(command, ct);

        return Accepted(new { transactionId });
    }

    [HttpGet]
    public async Task<IActionResult> CheckStatus(Guid transactionId, CancellationToken ct)  
    {
        var query = new CheckTransferStatusQuery(transactionId);
        var status = await _mediator.Send(query, ct);
        return Ok(status);
    }

    [HttpPost("withdrawal")]
    public async Task<IActionResult> Withdrawal(
        [FromBody] CreateWithdrawalRequest request,
        CancellationToken ct)
    {
        var command = new CreateWithdrawalCommand(
            request.AccountNumber,
            request.Amount,
            request.Currency);

        var transactionId = await _mediator.Send(command, ct);

        return Accepted(new { transactionId });
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(
        [FromBody] CreateDepositRequest request,
        CancellationToken ct)
    {
        var command = new CreateDepositCommand(
            request.ToAccountNumber,
            request.Amount,
            request.Currency);

        var transactionId = await _mediator.Send(command, ct);

        return Accepted(new { transactionId });

    }

}

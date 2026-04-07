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
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(
        IMediator mediator, 
        ILogger<TransactionController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
    [FromBody] CreateTransferRequest request,
    CancellationToken ct)
    {
        var initiatorId = Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "Transfer request started {initiatorId}",
            initiatorId);


        var command = new CreateTransferCommand(
            initiatorId,
            request.FromAccountNumber,
            request.ToAccountNumber,
            request.Amount,
            request.Currency);

        var transactionId = await _mediator.Send(command, ct);

        _logger.LogInformation(
            "Transfer request completed {initiatorId}, {transactionId}",
            initiatorId,
            transactionId);

        return Accepted(new { transactionId });
    }

    [HttpGet("{transactionId}")]
    public async Task<IActionResult> CheckStatus(Guid transactionId, CancellationToken ct)  
    {
        var query = new CheckTransferStatusQuery(transactionId);
        var status = await _mediator.Send(query, ct);
        return Ok(status);
    }

    [Authorize]
    [HttpPost("withdrawal")]
    public async Task<IActionResult> Withdrawal(
        [FromBody] CreateWithdrawalRequest request,
        CancellationToken ct)
    {
        var initiatorId = Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "Withdrawal request started {initiatorId}",
            initiatorId);

        var command = new CreateWithdrawalCommand(
            initiatorId,
            request.AccountNumber,
            request.Amount,
            request.Currency);

        var transactionId = await _mediator.Send(command, ct);

        _logger.LogInformation(
            "Withdrawal request completed {initiatorId}, {transactionId}",
             initiatorId,
             transactionId);

        return Accepted(new { transactionId });
    }


    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(
        [FromBody] CreateDepositRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Deposit request started for {ToAccountNumber}",
            request.ToAccountNumber);
        
        var command = new CreateDepositCommand(
            request.ToAccountNumber,
            request.Amount,
            request.Currency);

        var transactionId = await _mediator.Send(command, ct);

        _logger.LogInformation(
            "Deposit request completed for {ToAccountNumber}",
            request.ToAccountNumber);

        return Accepted(new { transactionId });

    }

}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.Commands.CreateTransfer;
using TransactionService.Application.DTOs;
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
        if (request.FromAccountId == request.ToAccountId)
            return BadRequest("Cannot transfer to the same account");
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than zero");
        var command = new CreateTransferCommand(
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            request.Currency);

        var transactionId = await _mediator.Send(command, ct);

        return Accepted(new { transactionId });
    }

}

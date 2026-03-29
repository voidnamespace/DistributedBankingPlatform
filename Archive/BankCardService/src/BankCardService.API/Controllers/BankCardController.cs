using BankCardService.Application.Commands.ActivateBankCard;
using BankCardService.Application.Commands.ChangeCardHolder;
using BankCardService.Application.Commands.CreateBankCard;
using BankCardService.Application.Commands.DeactivateBankCard;
using BankCardService.Application.DTOs;
using BankCardService.Application.Interfaces;
using BankCardService.Application.Queries.GetAllBankCards;
using BankCardService.Application.Queries.GetBankCardById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankCardService.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class BankCardController : ControllerBase
{
    private readonly IMediator _mediator;

    public BankCardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBankCardDTO createBankCardDTO)
    {
        var command = new CreateBankCardCommand(
            createBankCardDTO.CardNumber,
            createBankCardDTO.CardHolder
        );

        var response = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetById),
            new { cardId = response.Id },
            response
        );
    }


    [HttpGet("{cardId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid cardId)
    {
        var query = new GetBankCardByIdQuery(cardId);
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllBankCardsQuery();
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    [HttpPut("{cardId}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> Activate(Guid cardId)
    {
        var command = new ActivateBankCardCommand(cardId);
        await _mediator.Send(command);
        return Ok(new { Message = "Activate successful" });
    }

    [HttpPut("{cardId}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> Deactivate(Guid cardId)
    {
        var command = new DeactivateBankCardCommand(cardId);
        await _mediator.Send(command);
        return Ok(new { Message = "Deactivate successful" });
    }

    [HttpPut("{cardId}/changeHolder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> ChangeCardHolder(Guid cardId, string newHolder)
    {
        var command = new ChangeCardHolderCommand(cardId, newHolder);
        await _mediator.Send(command);
        return Ok(new { Message = "Card holder change successful" });
    }

    [HttpPut("{cardId}/changeNumber")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> ChangeCardNumber( Guid cardId, string newNumber)
    {
        var command = new ChangeCardHolderCommand(cardId, newNumber);
        await _mediator.Send(command);
        return Ok(new { Message = "Card number change successfull" });
    }
}

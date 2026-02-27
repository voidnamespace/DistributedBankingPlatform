using AuthService.Application.Commands.DeleteUser;
using AuthService.Application.Commands.LoginUser;
using AuthService.Application.Commands.LogoutUser;
using AuthService.Application.Commands.MakeRefreshToken;
using AuthService.Application.Commands.RegisterUser;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Application.Queries.GetAllUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.ConfirmPassword
            );

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginUserCommand(
            request.Email,
            request.Password
            );
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { message = "Invalid token" });

        await _mediator.Send(new LogoutUserCommand(userId));
        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            userId,
            email,
            role,
            message = "You have been successfully authenticated."
        });
    }
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _mediator.Send(new GetAllUsersQuery());
        return Ok(users);
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid userId)
    {
        await _mediator.Send(new DeleteUserCommand(userId));
        return NoContent();
    }

    [HttpPatch("{userId}/deactivate")]
    [Authorize]
    public async Task<IActionResult> Deactivate(Guid userId)
    {
        await _mediator.Send(new DeactivateUserCommand(userId));
        return Ok();
    }
    [HttpPatch("{userId}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid userId)
    {
        await _mediator.Send(new ActivateUserCommand(userId));
        return Ok();
    }

    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromServices] IEventPublisher publisher, CancellationToken ct)
        {
            await publisher.PublishAsync(new { msg = "hello", at = DateTime.UtcNow }, "auth.test", ct);
            return Ok("published");
        }
    }
}

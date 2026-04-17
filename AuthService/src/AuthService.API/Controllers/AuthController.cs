using AuthService.API.Contracts.Requests;
using AuthService.API.Contracts.Responses;
using AuthService.API.Extensions;
using AuthService.Application.Commands.ActivateUser;
using AuthService.Application.Commands.DeactivateUser;
using AuthService.Application.Commands.DeleteUser;
using AuthService.Application.Commands.LoginUser;
using AuthService.Application.Commands.LogoutUser;
using AuthService.Application.Commands.RotateRefreshToken;
using AuthService.Application.Commands.RegisterUser;
using AuthService.Application.Queries.GetAllUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator,
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation(
            "Register request started for {Email}",
            request.Email);

        var command = new RegisterUserCommand(
            request.Email,
            request.Password
        );

        var result = await _mediator.Send(command);

        var response = new RegisterResponse
        {
            UserId = result.UserId,
            Email = result.Email,
            Message = result.Message
        };

        _logger.LogInformation(
            "Register request completed for {Email}",
            request.Email);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation(
            "Login request started for {Email}",
            request.Email);

        var command = new LoginUserCommand(
            request.Email,
            request.Password
        );

        var result = await _mediator.Send(command);

        var response = new LoginResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresAt = result.ExpiresAt,
            UserId = result.UserId,
            Email = result.Email,
            Role = result.Role,
        };
            

        _logger.LogInformation(
            "Login request completed for {Email}",
            request.Email);

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Refresh token request started");

        var command = new RotateRefreshTokenCommand(request.RefreshToken);

        var result = await _mediator.Send(command);

        var response = new RefreshTokenResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresAt = result.ExpiresAt
        };

        _logger.LogInformation("Refresh token request completed");

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.GetUserId();

        _logger.LogInformation(
            "Logout request started {UserId}",
             userId);

        var command = new LogoutUserCommand(userId);

        await _mediator.Send(command);

        _logger.LogInformation(
          "Logout request completed {UserId}",
          userId);

        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.GetUserId();
        var email = User.GetEmail();
        var role = User.GetRole();

        _logger.LogInformation("User requested /me endpoint {UserId}", userId);

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
        _logger.LogInformation("Admin request all users");

        var users = await _mediator.Send(new GetAllUsersQuery());

        return Ok(users);
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid userId)
    {
        _logger.LogInformation(
            "Admin deleting user {UserId}",
            userId);

        await _mediator.Send(new DeleteUserCommand(userId));

        return NoContent();
    }

    [HttpPatch("me/deactivate")]
    [Authorize]
    public async Task<IActionResult> Deactivate()
    {
        var userId = User.GetUserId();

        _logger.LogInformation(
            "User deactivation request for {UserId}",
            userId);

        await _mediator.Send(
            new DeactivateUserCommand(userId));

        _logger.LogInformation(
            "User deactivation request completed for {UserId}",
            userId);

        return Ok(new { message = "User deactivated successfully" });
    }

    [HttpPatch("{userId}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid userId)
    {
        _logger.LogInformation(
            "Admin activating user {UserId}",
            userId);

        await _mediator.Send(new ActivateUserCommand(userId));

        _logger.LogInformation(
            "Admin activated user {UserId}",
            userId);

        return Ok();
    }
}

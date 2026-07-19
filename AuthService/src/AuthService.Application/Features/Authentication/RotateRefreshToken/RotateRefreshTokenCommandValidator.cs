using FluentValidation;

namespace AuthService.Application.Features.Authentication.RotateRefreshToken;

public class RotateRefreshTokenCommandValidator
    : AbstractValidator<RotateRefreshTokenCommand>
{
    public RotateRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token must be provided.");
    }
}

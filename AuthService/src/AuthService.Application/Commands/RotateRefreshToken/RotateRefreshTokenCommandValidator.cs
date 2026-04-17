using FluentValidation;

namespace AuthService.Application.Commands.RotateRefreshToken;

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

using FluentValidation;

namespace AuthService.API.Contracts.Requests.Validators;

public class RefreshTokenRequestValidator
    : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.")
            .Must(token => !string.IsNullOrWhiteSpace(token))
            .WithMessage("Refresh token must not be whitespace.");
    }
}

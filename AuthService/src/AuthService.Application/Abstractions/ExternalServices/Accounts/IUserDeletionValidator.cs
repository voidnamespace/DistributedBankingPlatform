using AuthService.Application.Abstractions.ExternalServices.Accounts.Contracts;

namespace AuthService.Application.Abstractions.ExternalServices.Accounts;

public interface IUserDeletionValidator
{
    Task<UserDeletionValidationResponse> ValidateUserDeletion(
        UserDeletionValidationRequest request);
}

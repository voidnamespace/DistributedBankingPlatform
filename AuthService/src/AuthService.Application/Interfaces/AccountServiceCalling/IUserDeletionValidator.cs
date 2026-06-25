using AuthService.Application.Interfaces.AccountServiceCalling.Contracts;

namespace AuthService.Application.Interfaces.AccountServiceCalling;

public interface IUserDeletionValidator
{
    Task<UserDeletionValidationResponse> ValidateUserDeletion(UserDeletionValidationRequest request);
}

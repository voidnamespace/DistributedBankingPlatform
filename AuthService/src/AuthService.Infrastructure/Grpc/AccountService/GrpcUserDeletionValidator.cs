using AuthService.Application.Interfaces.AccountServiceCalling;
using AuthService.Application.Interfaces.AccountServiceCalling.Contracts;
using AuthService.Infrastructure.GrpcCall.AccountService;

namespace AuthService.Infrastructure.Grpc.AccountService;

public sealed class GrpcUserDeletionValidator : IUserDeletionValidator
{
    private readonly UserLifecycleValidation.UserLifecycleValidationClient _client;

    public GrpcUserDeletionValidator(UserLifecycleValidation.UserLifecycleValidationClient client)
    {
        _client = client;
    }

    public async Task<UserDeletionValidationResponse> ValidateUserDeletion(
        UserDeletionValidationRequest request)
    {
        var response = await _client.ValidateUserDeletionAsync(
            new ValidateUserDeletionGrpcRequest
            {
                UserId = request.UserId.ToString()
            });

        return new UserDeletionValidationResponse(response.IsAllowed);
    }




}

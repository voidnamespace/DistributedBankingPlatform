using AccountService.Api.Grpc;

namespace AccountService.API.Grpc;

public sealed class UserLifecycleGrpcService : UserLifecycleValidation.UserLifecycleValidationBase
{

    public UserLifecycleGrpcService (AccountService.Application.GrpcValidation.IUserDeletionValidatorService userDeletionValidatorService)
    {

    }



}

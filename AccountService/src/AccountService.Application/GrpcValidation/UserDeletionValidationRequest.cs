namespace AccountService.Application.GrpcValidation;

public sealed record UserDeletionValidationRequest(Guid UserId);

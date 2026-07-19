namespace AuthService.Application.Common.Exceptions;

public sealed class UserDeletionRejectedException : Exception
{
    public UserDeletionRejectedException(string message)
        : base(message)
    {
    }
}

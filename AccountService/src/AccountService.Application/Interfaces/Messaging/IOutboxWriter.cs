namespace AccountService.Application.Interfaces.Messaging;

public interface IOutboxWriter
{
    Task SaveAsync();
}

namespace FeeService.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

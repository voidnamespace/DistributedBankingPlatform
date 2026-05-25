using MediatR;
using UserSegmentationService.Domain.Events;

namespace UserSegmentationService.Application.Common.DomainEvents;

public sealed record DomainEventNotification<TDomainEvent>(
    TDomainEvent DomainEvent)
    : INotification
    where TDomainEvent : IDomainEvent;

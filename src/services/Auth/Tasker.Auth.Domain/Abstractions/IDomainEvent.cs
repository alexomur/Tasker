namespace Tasker.Auth.Domain.Abstractions;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
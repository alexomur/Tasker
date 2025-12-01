namespace Tasker.Shared.Kernel.Abstractions;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
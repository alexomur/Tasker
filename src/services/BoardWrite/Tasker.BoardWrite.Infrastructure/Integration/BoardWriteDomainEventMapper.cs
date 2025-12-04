using Tasker.BoardWrite.Domain.Events.CardEvents;
using Tasker.Contracts;
using Tasker.Contracts.Boards.V1;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Infrastructure.Integration;

public sealed record IntegrationMessage(
    string Topic,
    string Key,
    object Payload);

public interface IDomainEventToIntegrationEventMapper
{
    IEnumerable<IntegrationMessage> Map(IDomainEvent domainEvent);
}

public sealed class BoardWriteDomainEventMapper : IDomainEventToIntegrationEventMapper
{
    public IEnumerable<IntegrationMessage> Map(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case CardAssigneesChanged e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.CardAssigneesChangedV1,
                    Key: e.CardId.ToString("D"),
                    Payload: new BoardCardAssigneesChangedV1(
                        BoardId: e.BoardId,
                        CardId: e.CardId,
                        AssigneeUserIds: e.AssigneeUserIds.ToArray(),
                        OccurredAt: e.OccurredAt));
                break;

            case CardDueDateChanged e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.CardDueDateChangedV1,
                    Key: e.CardId.ToString("D"),
                    Payload: new BoardCardDueDateChangedV1(
                        BoardId: e.BoardId,
                        CardId: e.CardId,
                        DueDate: e.NewDueDate,
                        OccurredAt: e.OccurredAt));
                break;

            default:
                yield break;
        }
    }
}
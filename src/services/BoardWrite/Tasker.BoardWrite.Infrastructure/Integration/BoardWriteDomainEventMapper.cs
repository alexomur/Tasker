using Tasker.BoardWrite.Domain.Events.BoardEvents;
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
            case BoardCreated e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.BoardCreatedV1,
                    Key: e.BoardId.ToString("D"),
                    Payload: new BoardCreatedV1(
                        BoardId: e.BoardId,
                        OwnerUserId: e.OwnerUserId,
                        Title: e.Title,
                        Description: e.Description,
                        OccurredAt: e.OccurredAt));
                break;

            case BoardDeleted e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.BoardDeletedV1,
                    Key: e.BoardId.ToString("D"),
                    Payload: new BoardDeletedV1(
                        BoardId: e.BoardId,
                        DeletedByUserId: e.DeletedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case BoardMemberAdded e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.BoardMemberAddedV1,
                    Key: $"{e.BoardId:D}:{e.UserId:D}",
                    Payload: new BoardMemberAddedV1(
                        BoardId: e.BoardId,
                        UserId: e.UserId,
                        Role: (int)e.Role,
                        AddedByUserId: e.AddedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case BoardMemberRemoved e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.BoardMemberRemovedV1,
                    Key: $"{e.BoardId:D}:{e.UserId:D}",
                    Payload: new BoardMemberRemovedV1(
                        BoardId: e.BoardId,
                        UserId: e.UserId,
                        RemovedByUserId: e.RemovedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case ColumnCreated e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.ColumnCreatedV1,
                    Key: e.ColumnId.ToString("D"),
                    Payload: new BoardColumnCreatedV1(
                        BoardId: e.BoardId,
                        ColumnId: e.ColumnId,
                        Title: e.Title,
                        Description: e.Description,
                        Order: e.Order,
                        CreatedByUserId: e.CreatedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case ColumnDeleted e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.ColumnDeletedV1,
                    Key: e.ColumnId.ToString("D"),
                    Payload: new BoardColumnDeletedV1(
                        BoardId: e.BoardId,
                        ColumnId: e.ColumnId,
                        DeletedByUserId: e.DeletedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case LabelCreated e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.LabelCreatedV1,
                    Key: e.LabelId.ToString("D"),
                    Payload: new BoardLabelCreatedV1(
                        BoardId: e.BoardId,
                        LabelId: e.LabelId,
                        Title: e.Title,
                        Description: e.Description,
                        Color: e.Color,
                        CreatedByUserId: e.CreatedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case LabelDeleted e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.LabelDeletedV1,
                    Key: e.LabelId.ToString("D"),
                    Payload: new BoardLabelDeletedV1(
                        BoardId: e.BoardId,
                        LabelId: e.LabelId,
                        DeletedByUserId: e.DeletedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case CardCreated e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.CardCreatedV1,
                    Key: e.CardId.ToString("D"),
                    Payload: new BoardCardCreatedV1(
                        BoardId: e.BoardId,
                        CardId: e.CardId,
                        ColumnId: e.ColumnId,
                        Title: e.Title,
                        Description: e.Description,
                        Order: e.Order,
                        CreatedByUserId: e.CreatedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case CardUpdated e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.CardUpdatedV1,
                    Key: e.CardId.ToString("D"),
                    Payload: new BoardCardUpdatedV1(
                        BoardId: e.BoardId,
                        CardId: e.CardId,
                        Title: e.Title,
                        Description: e.Description,
                        UpdatedByUserId: e.UpdatedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case CardMoved e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.CardMovedV1,
                    Key: e.CardId.ToString("D"),
                    Payload: new BoardCardMovedV1(
                        BoardId: e.BoardId,
                        CardId: e.CardId,
                        FromColumnId: e.FromColumnId,
                        ToColumnId: e.ToColumnId,
                        Order: e.Order,
                        MovedByUserId: e.MovedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case CardDeleted e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.CardDeletedV1,
                    Key: e.CardId.ToString("D"),
                    Payload: new BoardCardDeletedV1(
                        BoardId: e.BoardId,
                        CardId: e.CardId,
                        DeletedByUserId: e.DeletedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case CardLabelAttached e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.CardLabelAttachedV1,
                    Key: $"{e.CardId:D}:{e.LabelId:D}",
                    Payload: new BoardCardLabelAttachedV1(
                        BoardId: e.BoardId,
                        CardId: e.CardId,
                        LabelId: e.LabelId,
                        AttachedByUserId: e.AttachedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case CardLabelDetached e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.CardLabelDetachedV1,
                    Key: $"{e.CardId:D}:{e.LabelId:D}",
                    Payload: new BoardCardLabelDetachedV1(
                        BoardId: e.BoardId,
                        CardId: e.CardId,
                        LabelId: e.LabelId,
                        DetachedByUserId: e.DetachedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            case CardAssigneesChanged e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.BoardWrite.CardAssigneesChangedV1,
                    Key: e.CardId.ToString("D"),
                    Payload: new BoardCardAssigneesChangedV1(
                        BoardId: e.BoardId,
                        CardId: e.CardId,
                        AssigneeUserIds: e.AssigneeUserIds.ToArray(),
                        ChangedByUserId: e.ChangedByUserId,
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
                        ChangedByUserId: e.ChangedByUserId,
                        OccurredAt: e.OccurredAt));
                break;

            default:
                yield break;
        }
    }
}


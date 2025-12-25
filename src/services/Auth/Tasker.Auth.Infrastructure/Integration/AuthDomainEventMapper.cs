using Tasker.Auth.Domain.Events;
using Tasker.Contracts;
using Tasker.Contracts.Auth.V1;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.Auth.Infrastructure.Integration;

public sealed record IntegrationMessage(
    string Topic,
    string Key,
    object Payload);

public interface IAuthDomainEventMapper
{
    IEnumerable<IntegrationMessage> Map(IDomainEvent domainEvent);
}

public sealed class AuthDomainEventMapper : IAuthDomainEventMapper
{
    public IEnumerable<IntegrationMessage> Map(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case UserRegistered e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.Auth.UserRegisteredV1,
                    Key: e.UserId.ToString("D"),
                    Payload: new UserRegisteredV1(
                        UserId: e.UserId,
                        Email: e.Email,
                        DisplayName: e.DisplayName,
                        OccurredAt: e.OccurredAt));
                break;

            case UserEmailConfirmed e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.Auth.UserEmailConfirmedV1,
                    Key: e.UserId.ToString("D"),
                    Payload: new UserEmailConfirmedV1(
                        UserId: e.UserId,
                        OccurredAt: e.OccurredAt));
                break;

            case UserEmailChanged e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.Auth.UserEmailChangedV1,
                    Key: e.UserId.ToString("D"),
                    Payload: new UserEmailChangedV1(
                        UserId: e.UserId,
                        NewEmail: e.NewEmail,
                        OccurredAt: e.OccurredAt));
                break;

            case UserDisplayNameChanged e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.Auth.UserDisplayNameChangedV1,
                    Key: e.UserId.ToString("D"),
                    Payload: new UserDisplayNameChangedV1(
                        UserId: e.UserId,
                        NewDisplayName: e.NewDisplayName,
                        OccurredAt: e.OccurredAt));
                break;

            case UserPasswordChanged e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.Auth.UserPasswordChangedV1,
                    Key: e.UserId.ToString("D"),
                    Payload: new UserPasswordChangedV1(
                        UserId: e.UserId,
                        OccurredAt: e.OccurredAt));
                break;

            case UserLocked e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.Auth.UserLockedV1,
                    Key: e.UserId.ToString("D"),
                    Payload: new UserLockedV1(
                        UserId: e.UserId,
                        Reason: e.Reason,
                        OccurredAt: e.OccurredAt));
                break;

            case UserUnlocked e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.Auth.UserUnlockedV1,
                    Key: e.UserId.ToString("D"),
                    Payload: new UserUnlockedV1(
                        UserId: e.UserId,
                        OccurredAt: e.OccurredAt));
                break;

            case UserLoginSucceeded e:
                yield return new IntegrationMessage(
                    Topic: KafkaTopics.Auth.UserLoginSucceededV1,
                    Key: e.UserId.ToString("D"),
                    Payload: new UserLoginSucceededV1(
                        UserId: e.UserId,
                        Email: e.Email,
                        OccurredAt: e.OccurredAt));
                break;

            default:
                yield break;
        }
    }
}

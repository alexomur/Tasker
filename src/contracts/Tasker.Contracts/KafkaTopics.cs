namespace Tasker.Contracts;

public static class KafkaTopics
{
    public static class Auth
    {
        public const string UserRegisteredV1 = "auth.user-registered-v1";
        public const string UserEmailConfirmedV1 = "auth.user-email-confirmed-v1";
        public const string UserEmailChangedV1 = "auth.user-email-changed-v1";
        public const string UserDisplayNameChangedV1 = "auth.user-display-name-changed-v1";
        public const string UserPasswordChangedV1 = "auth.user-password-changed-v1";
        public const string UserLockedV1 = "auth.user-locked-v1";
        public const string UserUnlockedV1 = "auth.user-unlocked-v1";
        public const string UserLoginSucceededV1 = "auth.user-login-succeeded-v1";
    }

    public static class BoardWrite
    {
        public const string BoardCreatedV1 = "boardwrite.board-created-v1";
        public const string BoardDeletedV1 = "boardwrite.board-deleted-v1";
        public const string BoardMemberAddedV1 = "boardwrite.board-member-added-v1";
        public const string BoardMemberRemovedV1 = "boardwrite.board-member-removed-v1";
        public const string ColumnCreatedV1 = "boardwrite.column-created-v1";
        public const string ColumnDeletedV1 = "boardwrite.column-deleted-v1";
        public const string LabelCreatedV1 = "boardwrite.label-created-v1";
        public const string LabelDeletedV1 = "boardwrite.label-deleted-v1";
        public const string CardCreatedV1 = "boardwrite.card-created-v1";
        public const string CardUpdatedV1 = "boardwrite.card-updated-v1";
        public const string CardMovedV1 = "boardwrite.card-moved-v1";
        public const string CardDeletedV1 = "boardwrite.card-deleted-v1";
        public const string CardLabelAttachedV1 = "boardwrite.card-label-attached-v1";
        public const string CardLabelDetachedV1 = "boardwrite.card-label-detached-v1";
        public const string CardAssigneesChangedV1 = "boardwrite.card-assignees-changed-v1";
        public const string CardDueDateChangedV1 = "boardwrite.card-due-date-changed-v1";
    }
}

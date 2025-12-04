namespace Tasker.Contracts;

public static class KafkaTopics
{
    public static class BoardWrite
    {
        public const string CardAssigneesChangedV1 = "boardwrite.card-assignees-changed-v1";
        public const string CardDueDateChangedV1 = "boardwrite.card-due-date-changed-v1";
    }
}
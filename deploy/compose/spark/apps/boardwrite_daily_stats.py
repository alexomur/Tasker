import os
from functools import reduce
from typing import Iterable, Optional

from pyspark.sql import SparkSession, DataFrame, functions as F


HDFS_NAMENODE = os.getenv("HDFS_NAMENODE", "namenode:8020")
RAW_BASE = os.getenv("HDFS_RAW_BASE", "/raw/events")
OUT_BASE = os.getenv("HDFS_ANALYTICS_BASE", "/analytics")


BOARD_EVENT_SPECS = [
    ("boardwrite.board-created-v1", "boardId", "ownerUserId", "board_created"),
    ("boardwrite.board-deleted-v1", "boardId", "deletedByUserId", "board_deleted"),
    ("boardwrite.board-member-added-v1", "boardId", "addedByUserId", "board_member_added"),
    ("boardwrite.board-member-removed-v1", "boardId", "removedByUserId", "board_member_removed"),
    ("boardwrite.column-created-v1", "boardId", "createdByUserId", "column_created"),
    ("boardwrite.column-deleted-v1", "boardId", "deletedByUserId", "column_deleted"),
    ("boardwrite.label-created-v1", "boardId", "createdByUserId", "label_created"),
    ("boardwrite.label-deleted-v1", "boardId", "deletedByUserId", "label_deleted"),
    ("boardwrite.card-created-v1", "boardId", "createdByUserId", "card_created"),
    ("boardwrite.card-updated-v1", "boardId", "updatedByUserId", "card_updated"),
    ("boardwrite.card-moved-v1", "boardId", "movedByUserId", "card_moved"),
    ("boardwrite.card-deleted-v1", "boardId", "deletedByUserId", "card_deleted"),
    ("boardwrite.card-label-attached-v1", "boardId", "attachedByUserId", "card_label_attached"),
    ("boardwrite.card-label-detached-v1", "boardId", "detachedByUserId", "card_label_detached"),
    ("boardwrite.card-assignees-changed-v1", "boardId", "changedByUserId", "card_assignees_changed"),
    ("boardwrite.card-due-date-changed-v1", "boardId", "changedByUserId", "card_due_date_changed"),
]

AUTH_EVENT_SPECS = [
    ("auth.user-registered-v1", "userId", "user_registered"),
    ("auth.user-login-succeeded-v1", "userId", "user_login_succeeded"),
    ("auth.user-email-confirmed-v1", "userId", "user_email_confirmed"),
    ("auth.user-email-changed-v1", "userId", "user_email_changed"),
    ("auth.user-display-name-changed-v1", "userId", "user_display_name_changed"),
    ("auth.user-password-changed-v1", "userId", "user_password_changed"),
    ("auth.user-locked-v1", "userId", "user_locked"),
    ("auth.user-unlocked-v1", "userId", "user_unlocked"),
]


def hdfs_glob_exists(spark: SparkSession, path: str) -> bool:
    jvm = spark._jvm
    fs = jvm.org.apache.hadoop.fs.FileSystem.get(spark._jsc.hadoopConfiguration())
    statuses = fs.globStatus(jvm.org.apache.hadoop.fs.Path(path))
    return statuses is not None and len(statuses) > 0


def read_topic(
    spark: SparkSession, topic: str, board_field: Optional[str], user_field: str, event_type: str
) -> Optional[DataFrame]:
    path = f"hdfs://{HDFS_NAMENODE}{RAW_BASE}/{topic}/dt=*/hour=*/part-*.json"
    if not hdfs_glob_exists(spark, path):
        return None

    df = spark.read.json(path)

    base = df.select(
        F.lit(event_type).alias("event_type"),
        F.col("payload.occurredAt").alias("occurred_at"),
        F.col(f"payload.{user_field}").alias("user_id"),
    )

    if board_field:
        base = base.withColumn("board_id", F.col(f"payload.{board_field}"))
    else:
        base = base.withColumn("board_id", F.lit(None).cast("string"))

    return (
        base.withColumn("event_date", F.to_date("occurred_at"))
        .where(F.col("event_date").isNotNull())
        .select("event_date", "event_type", "board_id", "user_id")
    )


def union_or_empty(frames: Iterable[DataFrame]) -> Optional[DataFrame]:
    frames = [frame for frame in frames if frame is not None]
    if not frames:
        return None
    return reduce(lambda left, right: left.unionByName(right), frames)


def main() -> None:
    spark = SparkSession.builder.appName("boardwrite-daily-stats").getOrCreate()
    spark.sparkContext.setLogLevel(os.getenv("SPARK_LOG_LEVEL", "WARN"))
    spark.conf.set("spark.sql.session.timeZone", "UTC")
    spark.conf.set("spark.sql.files.ignoreMissingFiles", "true")

    board_events = [
        read_topic(spark, topic, board_field, user_field, event_type)
        for topic, board_field, user_field, event_type in BOARD_EVENT_SPECS
    ]
    auth_events = [
        read_topic(spark, topic, None, user_field, event_type)
        for topic, user_field, event_type in AUTH_EVENT_SPECS
    ]

    board_df = union_or_empty(board_events)
    auth_df = union_or_empty(auth_events)

    if board_df is not None:
        board_daily = (
            board_df.groupBy("event_date", "board_id", "event_type")
            .count()
            .orderBy("event_date", "board_id", "event_type")
        )
        user_daily = (
            board_df.groupBy("event_date", "user_id", "event_type")
            .count()
            .orderBy("event_date", "user_id", "event_type")
        )
        topic_daily = (
            board_df.groupBy("event_date", "event_type")
            .count()
            .orderBy("event_date", "event_type")
        )

        board_daily.write.mode("overwrite").partitionBy("event_date").parquet(
            f"hdfs://{HDFS_NAMENODE}{OUT_BASE}/boardwrite/board_daily"
        )
        user_daily.write.mode("overwrite").partitionBy("event_date").parquet(
            f"hdfs://{HDFS_NAMENODE}{OUT_BASE}/boardwrite/user_daily"
        )
        topic_daily.write.mode("overwrite").partitionBy("event_date").parquet(
            f"hdfs://{HDFS_NAMENODE}{OUT_BASE}/boardwrite/topic_daily"
        )

    if auth_df is not None:
        auth_daily = (
            auth_df.groupBy("event_date", "event_type")
            .count()
            .orderBy("event_date", "event_type")
        )

        auth_daily.write.mode("overwrite").partitionBy("event_date").parquet(
            f"hdfs://{HDFS_NAMENODE}{OUT_BASE}/auth/daily"
        )


if __name__ == "__main__":
    main()

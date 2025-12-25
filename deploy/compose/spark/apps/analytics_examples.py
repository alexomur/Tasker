import os
from functools import reduce
from typing import Iterable, Optional

from pyspark.sql import DataFrame, SparkSession, functions as F


HDFS_NAMENODE = os.getenv("HDFS_NAMENODE", "namenode:8020")
RAW_BASE = os.getenv("HDFS_RAW_BASE", "/raw/events")
OUT_BASE = os.getenv("HDFS_ANALYTICS_BASE", "/analytics")
EXAMPLES_BASE = f"{OUT_BASE}/examples"


def hdfs_glob_has_data(spark: SparkSession, path: str) -> bool:
    jvm = spark._jvm
    fs = jvm.org.apache.hadoop.fs.FileSystem.get(spark._jsc.hadoopConfiguration())
    statuses = fs.globStatus(jvm.org.apache.hadoop.fs.Path(path))
    if statuses is None:
        return False
    for status in statuses:
        if status.isFile() and status.getLen() > 0:
            return True
    return False


def read_topic(spark: SparkSession, topic: str) -> Optional[DataFrame]:
    path = f"hdfs://{HDFS_NAMENODE}{RAW_BASE}/{topic}/dt=*/hour=*/part-*.json"
    if not hdfs_glob_has_data(spark, path):
        print(f"No data found for topic: {topic}")
        return None

    df = spark.read.json(path)
    if df.rdd.isEmpty():
        print(f"No rows parsed for topic: {topic}")
        return None

    return df


def union_or_empty(frames: Iterable[DataFrame]) -> Optional[DataFrame]:
    frames = [frame for frame in frames if frame is not None]
    if not frames:
        return None
    return reduce(lambda left, right: left.unionByName(right), frames)


def write_if_has_rows(df: Optional[DataFrame], path: str, label: str) -> None:
    if df is None:
        print(f"Skip {label}: no data")
        return
    if df.rdd.isEmpty():
        print(f"Skip {label}: empty dataset")
        return
    df.write.mode("overwrite").parquet(path)
    print(f"Wrote {label} to {path}")


def main() -> None:
    spark = SparkSession.builder.appName("analytics-examples").getOrCreate()
    spark.sparkContext.setLogLevel(os.getenv("SPARK_LOG_LEVEL", "WARN"))
    spark.conf.set("spark.sql.session.timeZone", "UTC")
    spark.conf.set("spark.sql.files.ignoreMissingFiles", "true")

    all_path = f"hdfs://{HDFS_NAMENODE}{RAW_BASE}/*/dt=*/hour=*/part-*.json"
    if not hdfs_glob_has_data(spark, all_path):
        print("No HDFS data found under /raw/events")
        return

    all_events = spark.read.json(all_path)
    if "topic" in all_events.columns:
        topic_counts = (
            all_events.groupBy("topic")
            .count()
            .orderBy(F.desc("count"), F.asc("topic"))
        )
        write_if_has_rows(
            topic_counts,
            f"hdfs://{HDFS_NAMENODE}{EXAMPLES_BASE}/topics",
            "topics.counts",
        )
    else:
        print("Missing 'topic' column in raw events payloads")

    login_df = read_topic(spark, "auth.user-login-succeeded-v1")
    login_daily = None
    if login_df is not None:
        login_daily = (
            login_df.select(
                F.to_date("payload.occurredAt").alias("event_date"),
                F.col("payload.email").alias("email"),
                F.col("payload.userId").alias("user_id"),
            )
            .where(F.col("event_date").isNotNull())
            .groupBy("event_date")
            .count()
            .orderBy("event_date")
        )
    write_if_has_rows(
        login_daily,
        f"hdfs://{HDFS_NAMENODE}{EXAMPLES_BASE}/auth/login_daily",
        "auth.login_daily",
    )

    board_created = read_topic(spark, "boardwrite.board-created-v1")
    board_deleted = read_topic(spark, "boardwrite.board-deleted-v1")
    board_events = union_or_empty(
        [
            board_created.select(
                F.to_date("payload.occurredAt").alias("event_date"),
                F.lit("board_created").alias("event_type"),
                F.col("payload.boardId").alias("board_id"),
            )
            if board_created is not None
            else None,
            board_deleted.select(
                F.to_date("payload.occurredAt").alias("event_date"),
                F.lit("board_deleted").alias("event_type"),
                F.col("payload.boardId").alias("board_id"),
            )
            if board_deleted is not None
            else None,
        ]
    )
    if board_events is not None:
        board_events = board_events.where(F.col("event_date").isNotNull())
        board_daily = (
            board_events.groupBy("event_date", "event_type")
            .count()
            .orderBy("event_date", "event_type")
        )
    else:
        board_daily = None
    write_if_has_rows(
        board_daily,
        f"hdfs://{HDFS_NAMENODE}{EXAMPLES_BASE}/boardwrite/board_events_daily",
        "boardwrite.board_events_daily",
    )

    card_created = read_topic(spark, "boardwrite.card-created-v1")
    card_daily = None
    if card_created is not None:
        card_daily = (
            card_created.select(
                F.to_date("payload.occurredAt").alias("event_date"),
                F.col("payload.boardId").alias("board_id"),
                F.col("payload.cardId").alias("card_id"),
            )
            .where(F.col("event_date").isNotNull())
            .groupBy("event_date", "board_id")
            .count()
            .orderBy("event_date", F.desc("count"))
        )
    write_if_has_rows(
        card_daily,
        f"hdfs://{HDFS_NAMENODE}{EXAMPLES_BASE}/boardwrite/card_created_daily",
        "boardwrite.card_created_daily",
    )


if __name__ == "__main__":
    main()

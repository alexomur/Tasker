import os

from pyspark.sql import SparkSession, functions as F, types as T


def main() -> None:
    kafka_bootstrap = os.getenv("KAFKA_BOOTSTRAP_SERVERS", "redpanda:9092")
    hdfs_raw_base = os.getenv("HDFS_RAW_BASE", "/raw/events")
    target_topic = os.getenv("KAFKA_TARGET_TOPIC", "analytics.hdfs-events-v1")
    checkpoint_base = os.getenv("SPARK_CHECKPOINT_BASE", "/analytics/checkpoints/hdfs_to_kafka")
    max_files = os.getenv("MAX_FILES_PER_TRIGGER", "50")

    spark = SparkSession.builder.appName("hdfs-to-kafka-stream").getOrCreate()
    spark.sparkContext.setLogLevel(os.getenv("SPARK_LOG_LEVEL", "WARN"))

    input_path = f"hdfs://namenode:8020{hdfs_raw_base}/*/dt=*/hour=*/part-*.json"

    raw = (
        spark.readStream.format("text")
        .option("maxFilesPerTrigger", max_files)
        .load(input_path)
    )

    schema = T.StructType(
        [
            T.StructField("topic", T.StringType()),
            T.StructField("key", T.StringType()),
        ]
    )

    parsed = raw.select(
        F.col("value").cast("string").alias("raw_value"),
        F.from_json(F.col("value").cast("string"), schema).alias("json"),
    ).where(F.col("raw_value").isNotNull())

    outgoing = parsed.select(
        F.col("json.key").cast("string").alias("key"),
        F.col("raw_value").alias("value"),
    )

    (
        outgoing.writeStream.format("kafka")
        .option("kafka.bootstrap.servers", kafka_bootstrap)
        .option("topic", target_topic)
        .option(
            "checkpointLocation",
            f"hdfs://namenode:8020{checkpoint_base}",
        )
        .outputMode("append")
        .start()
        .awaitTermination()
    )


if __name__ == "__main__":
    main()

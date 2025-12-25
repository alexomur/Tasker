import os
from pathlib import Path
from typing import List, Optional

from pyspark.sql import SparkSession, functions as F


HDFS_NAMENODE = os.getenv("HDFS_NAMENODE", "namenode:8020")
RAW_BASE = os.getenv("HDFS_RAW_BASE", "/raw/events")
OUTPUT_DIR = os.getenv("SPARK_OUTPUT_DIR", "/opt/spark-output")
MAX_TOPICS = int(os.getenv("MAX_TOPICS", "20"))


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


def render_plot(
    labels: List[str],
    values: List[int],
    title: str,
    output_path: Path,
    empty_message: str,
) -> bool:
    try:
        import matplotlib
        matplotlib.use("Agg")
        import matplotlib.pyplot as plt
    except Exception as exc:
        print(f"Matplotlib is not available: {exc}")
        return False

    output_path.parent.mkdir(parents=True, exist_ok=True)
    fig, ax = plt.subplots(figsize=(10, 6))
    if not labels:
        ax.axis("off")
        ax.text(0.5, 0.5, empty_message, ha="center", va="center", fontsize=14)
    else:
        ax.barh(labels, values, color="#1f77b4")
        ax.invert_yaxis()
        ax.set_xlabel("Events")
        ax.set_title(title)
    fig.tight_layout()
    fig.savefig(output_path)
    print(f"Wrote plot to {output_path}")
    return True


def main() -> None:
    spark = SparkSession.builder.appName("analytics-examples-plot").getOrCreate()
    spark.sparkContext.setLogLevel(os.getenv("SPARK_LOG_LEVEL", "WARN"))
    spark.conf.set("spark.sql.session.timeZone", "UTC")
    spark.conf.set("spark.sql.files.ignoreMissingFiles", "true")

    path = f"hdfs://{HDFS_NAMENODE}{RAW_BASE}/*/dt=*/hour=*/part-*.json"
    output_path = Path(OUTPUT_DIR) / "topic_counts.png"

    if not hdfs_glob_has_data(spark, path):
        render_plot(
            [],
            [],
            "Topic counts",
            output_path,
            "No data found under /raw/events",
        )
        return

    events = spark.read.json(path)
    if events.rdd.isEmpty():
        render_plot(
            [],
            [],
            "Topic counts",
            output_path,
            "No rows parsed from raw events",
        )
        return

    if "topic" not in events.columns:
        render_plot(
            [],
            [],
            "Topic counts",
            output_path,
            "Missing 'topic' field in events",
        )
        return

    counts = (
        events.where(F.col("topic").isNotNull())
        .groupBy("topic")
        .count()
        .orderBy(F.desc("count"), F.asc("topic"))
        .limit(MAX_TOPICS)
    )
    rows = counts.collect()
    labels = [row["topic"] for row in rows]
    values = [int(row["count"]) for row in rows]

    if not labels:
        render_plot(
            [],
            [],
            "Topic counts",
            output_path,
            "No events found to plot",
        )
        return

    render_plot(labels, values, "Top topics", output_path, "")


if __name__ == "__main__":
    main()

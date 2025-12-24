# Spark analytics

This folder contains PySpark jobs used for analytics.

Jobs:
- hdfs_to_kafka_stream.py: reads raw JSONL envelopes from HDFS and publishes them to Kafka topic `analytics.hdfs-events-v1`.
- boardwrite_daily_stats.py: computes daily aggregates from raw events and writes parquet datasets under `/analytics`.
- analytics_examples.py: sample PySpark analytics (topic counts, logins per day, board and card activity).
- analytics_examples_plot.py: renders a PNG chart of top topics into `/opt/spark-output/topic_counts.png`.

Run batch job manually:
```
docker compose -f deploy/compose/docker-compose.yml --profile analytics run --rm analytics-batch
```

Run example analytics (PowerShell):
```
.\scripts\analytics\run_spark_examples.ps1
```

Render a chart (PowerShell):
```
.\scripts\analytics\run_spark_plot.ps1 -InstallMatplotlib
```

Chart output:
```
deploy/compose/spark/output/topic_counts.png
```

Watch streaming job logs:
```
docker compose -f deploy/compose/docker-compose.yml logs -f analytics-hdfs-kafka
```

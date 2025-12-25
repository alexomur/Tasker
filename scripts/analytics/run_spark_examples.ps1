param(
  [string]$Network = "appnet"
)

$root = Resolve-Path (Join-Path $PSScriptRoot "..\\..")
$apps = Join-Path $root "deploy\\compose\\spark\\apps"

$dockerArgs = @(
  "run",
  "--rm",
  "--network", $Network,
  "-e", "HDFS_NAMENODE=namenode:8020",
  "-e", "HDFS_RAW_BASE=/raw/events",
  "-e", "HDFS_ANALYTICS_BASE=/analytics",
  "-e", "SPARK_LOG_LEVEL=WARN",
  "-v", "$($apps):/opt/spark-apps:ro",
  "apache/spark:3.5.1",
  "/opt/spark/bin/spark-submit",
  "--master", "spark://spark-master:7077",
  "--conf", "spark.jars.ivy=/tmp/ivy",
  "--conf", "spark.hadoop.fs.defaultFS=hdfs://namenode:8020",
  "--conf", "spark.sql.session.timeZone=UTC",
  "/opt/spark-apps/analytics_examples.py"
)

& docker @dockerArgs

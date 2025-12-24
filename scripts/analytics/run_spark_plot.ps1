param(
  [string]$Network = "appnet",
  [switch]$InstallMatplotlib
)

$root = Resolve-Path (Join-Path $PSScriptRoot "..\\..")
$apps = Join-Path $root "deploy\\compose\\spark\\apps"
$output = Join-Path $root "deploy\\compose\\spark\\output"

New-Item -ItemType Directory -Force -Path $output | Out-Null

$install = ""
if ($InstallMatplotlib) {
  $install = "python3 -m ensurepip --upgrade >/dev/null 2>&1 || true; " +
    "python3 -m pip install -q matplotlib || " +
    "(apt-get update -y >/dev/null 2>&1 && apt-get install -y python3-pip >/dev/null 2>&1 && python3 -m pip install -q matplotlib); "
}

$command = $install + "/opt/spark/bin/spark-submit " +
  "--master spark://spark-master:7077 " +
  "--conf spark.jars.ivy=/tmp/ivy " +
  "--conf spark.hadoop.fs.defaultFS=hdfs://namenode:8020 " +
  "--conf spark.sql.session.timeZone=UTC " +
  "/opt/spark-apps/analytics_examples_plot.py"

$dockerArgs = @(
  "run",
  "--rm",
  "--user", "0",
  "--network", $Network,
  "-e", "HDFS_NAMENODE=namenode:8020",
  "-e", "HDFS_RAW_BASE=/raw/events",
  "-e", "SPARK_OUTPUT_DIR=/opt/spark-output",
  "-e", "SPARK_LOG_LEVEL=WARN",
  "-v", "$($apps):/opt/spark-apps:ro",
  "-v", "$($output):/opt/spark-output",
  "apache/spark:3.5.1",
  "bash", "-lc", $command
)

& docker @dockerArgs

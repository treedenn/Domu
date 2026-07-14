#!/usr/bin/env bash
set -euo pipefail

adb reverse tcp:8080 tcp:8080
adb reverse tcp:5070 tcp:5070
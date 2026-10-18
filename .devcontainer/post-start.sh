#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

pkill -f "FlyVip.Web" 2>/dev/null || true

ASPNETCORE_URLS=http://0.0.0.0:8080 setsid nohup dotnet run --no-launch-profile --project src/FlyVip.Web </dev/null >/tmp/serve.log 2>&1 &
disown || true

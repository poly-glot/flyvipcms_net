#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

git config --global --add safe.directory "$PWD"
sudo chown vscode:vscode node_modules

dotnet tool restore
dotnet restore FlyVip.sln
dotnet build src/FlyVip.Web
npm ci --no-audit --no-fund

migrate() {
  ConnectionStrings__Default="$1" dotnet run --no-build --no-launch-profile --project src/FlyVip.Web -- migrate
}

for attempt in $(seq 1 30); do
  if migrate "$ConnectionStrings__Default" && migrate "$ConnectionStrings__Test"; then
    exit 0
  fi
  echo "Database not ready (attempt $attempt/30), retrying in 5s"
  sleep 5
done

exit 1

#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")"

DOTNET="${DOTNET:-$HOME/.dotnet/dotnet}"
[ -x "$DOTNET" ] || DOTNET="dotnet"

"$DOTNET" format FlyVip.sln --verify-no-changes
"$DOTNET" build FlyVip.sln -warnaserror

rm -rf test-results
"$DOTNET" test FlyVip.sln --no-build --collect "XPlat Code Coverage" --results-directory test-results -- \
  'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByFile=**/Data/Migrations/*.cs,**/obj/**/*.cs'
node tools/coverage-gate.mjs "${QUALITY_COVERAGE_FLOOR:-55}" test-results

npm run lint

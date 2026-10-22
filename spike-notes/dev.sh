#!/usr/bin/env bash
# Kill (only the dev instance this script started), rebuild, relaunch Antipad against throwaway test data.
# Never touches an Antipad the user launched: the dev instance's PID is kept in build/dev.pid.
# Usage: spike-notes/dev.sh [--fresh]   (--fresh wipes build/testdata first)
# HEADLESS RUNS: do not use this script. Build to build/headless and verify with --selftest only.
cd "$(dirname "$0")/.."
if [ -f build/dev.pid ]; then taskkill //PID "$(cat build/dev.pid)" //F >/dev/null 2>&1; rm -f build/dev.pid; sleep 0.5; fi
if tasklist //FI "IMAGENAME eq Antipad.exe" | grep -qi antipad; then
  echo "Another Antipad.exe is running (probably the user's). Not rebuilding build/antipad over it."; exit 2
fi
[ "$1" = "--fresh" ] && rm -rf build/testdata
out=$(dotnet build product/Antipad -c Debug -o build/antipad 2>&1)
echo "$out" | grep -oE "[A-Za-z.]+\.cs\([0-9,]+\): (error|warning) [^[]*" | sort -u
echo "$out" | grep -q "0 Error(s)" || { echo BUILD FAILED; exit 1; }
mkdir -p build/testdata build/shots
ANTIPAD_DATA="$(cygpath -w "$PWD/build/testdata")" ANTIPAD_SHOTS="$(cygpath -w "$PWD/build/shots")" ./build/antipad/Antipad.exe > build/run.log 2>&1 &
sleep 3
pid=$(tasklist //FI "IMAGENAME eq Antipad.exe" //FO CSV //NH | head -1 | cut -d, -f2 | tr -d '"')
[ -n "$pid" ] && echo "$pid" > build/dev.pid && echo "RUNNING pid=$pid" || { echo NOT RUNNING; cat build/run.log; }

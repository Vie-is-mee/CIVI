#!/usr/bin/env bash
# Runs a Unity batch method on the CIVI_7 project and prints only what matters from the log.
# usage: unity.sh <Namespace.Class.Method> [--graphics] [extra Unity args]
method="$1"; shift
gfx="-nographics"
if [ "$1" = "--graphics" ]; then gfx=""; shift; fi
root="$(cd "$(dirname "$0")/.." && pwd)"
log="$root/Game/unity-batch.log"
"/c/Program Files/Unity/Hub/Editor/6000.4.0f1/Editor/Unity.exe" -batchmode $gfx -quit -projectPath "$(cygpath -w "$root/Game")" \
  -executeMethod "$method" -logFile "$(cygpath -w "$log")" "$@"
rc=$?
grep -E '^CIVI|error CS|Shader error|Compilation failed|executeMethod method|Exception:|NullReference|^light ' "$log" | sort -u | cut -c1-220 | head -40
echo "unity rc=$rc"
exit $rc

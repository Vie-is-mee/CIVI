#!/usr/bin/env bash
# Run a Blender script headlessly and wait for its .done marker.
# Usage: run.sh <script.py> <marker-file> [arguments passed after --]
set -euo pipefail

script="$1"; marker="$2"; shift 2
root="$(cd "$(dirname "$0")" && pwd)"
rm -f "$marker"

py -3.12 -c 'import ast,sys; ast.parse(open(sys.argv[1],encoding="utf-8").read())' "$root/$script"

if ! command -v cygpath >/dev/null 2>&1; then
  echo "CIVI: run.sh requires Git Bash for Windows (cygpath)." >&2
  exit 1
fi

if [ -n "${BLENDER_BIN:-}" ]; then
  blender="$BLENDER_BIN"
elif command -v blender.exe >/dev/null 2>&1; then
  blender="$(command -v blender.exe)"
elif command -v blender >/dev/null 2>&1; then
  blender="$(command -v blender)"
else
  blender=""
  for candidate in "/c/Program Files/Blender Foundation"/Blender*/blender.exe; do
    if [ -f "$candidate" ]; then blender="$candidate"; break; fi
  done
  if [ -z "$blender" ]; then
    echo "CIVI: Blender not found. Install Blender or set BLENDER_BIN." >&2
    exit 1
  fi
fi

"$blender" --background --factory-startup --python "$(cygpath -w "$root/$script")" -- "$@" > /dev/null 2>&1 &
pid=$!
for ((i=0; i<300; i++)); do
  if [ -f "$marker" ]; then exit 0; fi
  if ! kill -0 "$pid" 2>/dev/null; then
    wait "$pid" || true
    if [ -f "$marker" ]; then exit 0; fi
    echo "CIVI: Blender exited before writing $marker" >&2
    exit 1
  fi
  sleep 1
done
echo "CIVI: Blender timed out waiting for $marker" >&2
exit 1

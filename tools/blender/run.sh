#!/usr/bin/env bash
# Runs a Blender script headless and waits for its .done marker: the Store launcher returns before Blender finishes.
# usage: run.sh <script.py> <marker-file> [args passed after --]
script="$1"; marker="$2"; shift 2
rm -f "$marker"
py -3.12 -c "import ast,sys; ast.parse(open(sys.argv[1],encoding='utf8').read())" "$(dirname "$0")/$script" || exit 1
"/c/Users/ACER/AppData/Local/Microsoft/WindowsApps/blender-launcher.exe" --background --factory-startup \
  --python "$(cygpath -w "$(dirname "$0")")\\$script" -- "$@" > /dev/null 2>&1
for _ in $(seq 1 300); do [ -f "$marker" ] && exit 0; sleep 1; done
echo "timed out waiting for $marker"; exit 1

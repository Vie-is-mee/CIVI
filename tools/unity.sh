#!/usr/bin/env bash
# Run a Unity Editor batch method for the version pinned by this project.
# Usage: unity.sh Namespace.Class.Method [--graphics] [extra Unity args]
set -euo pipefail
if [ "$#" -lt 1 ]; then echo "Usage: unity.sh Namespace.Class.Method ..." >&2; exit 2; fi
method="$1"; shift
gfx="-nographics"
if [ "$#" -gt 0 ] && [ "$1" = "--graphics" ]; then gfx=""; shift; fi
root="$(cd "$(dirname "$0")/.." && pwd)"
log="$root/Game/unity-batch.log"

if ! command -v cygpath >/dev/null 2>&1; then
  echo "CIVI: Unity shell wrapper requires Git Bash for Windows (cygpath)." >&2
  exit 1
fi
version="$(sed -n 's/^m_EditorVersion: //p' "$root/Game/ProjectSettings/ProjectVersion.txt" | head -1)"
default_editor="/c/Program Files/Unity/Hub/Editor/$version/Editor/Unity.exe"
unity="$default_editor"
if [ -n "${UNITY_EDITOR:-}" ]; then unity="$UNITY_EDITOR"; fi
if [ ! -f "$unity" ] && ! command -v "$unity" >/dev/null 2>&1; then
  echo "CIVI: Unity $version not found at $unity. Install the pinned editor or set UNITY_EDITOR." >&2
  exit 1
fi

rc=0
"$unity" -batchmode $gfx -quit -projectPath "$(cygpath -w "$root/Game")" \
  -executeMethod "$method" -logFile "$(cygpath -w "$log")" "$@" || rc=$?
if [ -f "$log" ]; then
  grep -E '^CIVI|error CS|Shader error|Compilation failed|executeMethod method|Exception:|NullReference|^light ' "$log" \
    | sort -u | cut -c1-220 | head -40 || true
fi
echo "unity rc=$rc"
exit "$rc"

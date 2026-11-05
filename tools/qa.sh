#!/usr/bin/env bash
# Runs the automated playthrough in the real player. usage: qa.sh <out-dir> [player args]
root="$(cd "$(dirname "$0")/.." && pwd)"
out="$1"; shift
mkdir -p "$out"
timeout 560 "$root/release/CIVI/CIVI.exe" -civi-qa "$(cygpath -w "$(realpath "$out")")" -logFile "$(cygpath -w "$(realpath "$out")/player.log")" -screen-fullscreen 0 -screen-width 1600 -screen-height 900 "$@"
echo "player rc=$?"
tail -4 "$out/qa.txt" | cut -c1-240
cat "$out/fps.txt" 2>/dev/null
grep -n -E 'Exception|NullReference|Shader.*not|Missing' "$out/player.log" | head -12 | cut -c1-220

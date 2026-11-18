#!/usr/bin/env bash
# Regenerates generated content, then builds the player. usage: rebuild.sh [world] [chars] [story] (default: all)
root="$(cd "$(dirname "$0")/.." && pwd)"
models="$root/Game/Assets/_Civi/Models"
tmp="$root/artifacts/.build"; mkdir -p "$tmp"
what="${*:-world chars story}"
win() { cygpath -w "$1"; }
for w in $what; do
  case "$w" in
    world)
      rm -f "$tmp/world.done.error"
      bash "$root/tools/blender/run.sh" build_world.py "$tmp/world.done" "$(win "$tmp/world.done")" "$(win "$models/World")" || exit 1
      [ -f "$tmp/world.done.error" ] && { tail -6 "$tmp/world.done.error"; exit 1; }
      mv -f "$models/World/floor-holes.txt" "$tmp/"
      [ "$(head -1 "$tmp/floor-holes.txt")" = 0 ] || { echo "floor has holes, see $tmp/floor-holes.txt"; exit 1; } ;;
    chars)
      rm -f "$tmp/chars.done.error"
      bash "$root/tools/blender/run.sh" build_characters.py "$tmp/chars.done" "$(win "$tmp/chars.done")" "$(win "$models/Characters")" || exit 1
      [ -f "$tmp/chars.done.error" ] && { tail -6 "$tmp/chars.done.error"; exit 1; } ;;
    story)
      (cd "$root/tools" && py -3.12 compile_chapters.py && py -3.12 import_audio.py) || exit 1 ;;
  esac
done
bash "$root/tools/unity.sh" Civi.EditorTools.CiviBuild.Build | tail -5

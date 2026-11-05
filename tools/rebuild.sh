#!/usr/bin/env bash
# Regenerates models from the Blender scripts, then builds the player. usage: rebuild.sh [world] [chars] (default both)
root="$(cd "$(dirname "$0")/.." && pwd)"
models="$(cygpath -w "$root/Game/Assets/_Civi/Models")"
tmp="$root/artifacts/.build"; mkdir -p "$tmp"
what="${*:-world chars}"
for w in $what; do
  [ "$w" = world ] && s=build_world.py d=World
  [ "$w" = chars ] && s=build_characters.py d=Characters
  rm -f "$tmp/$w.done.error"
  bash "$root/tools/blender/run.sh" "$s" "$tmp/$w.done" "$(cygpath -w "$tmp/$w.done")" "$models/$d" || exit 1
  [ -f "$tmp/$w.done.error" ] && { tail -6 "$tmp/$w.done.error"; exit 1; }
done
bash "$root/tools/unity.sh" Civi.EditorTools.CiviBuild.Build | tail -5

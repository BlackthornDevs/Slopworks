#!/usr/bin/env bash
# Run this after cloning to configure the autocontext union merge driver.
# Without this, merges of lessons.json fall back to default (conflict-prone) behavior.

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$REPO_ROOT"

git config merge.autocontext-union.name "Autocontext lessons.json union merge"

# Try to find the merge driver script. It ships with the autocontext plugin.
MERGE_SCRIPT=""
for candidate in \
  "$HOME/.claude/plugins/cache/claude-skills-journalism/autocontext/1.0.0/scripts/merge-driver.py" \
  "$HOME/.claude/plugins/autocontext/scripts/merge-driver.py"; do
  if [ -f "$candidate" ]; then
    MERGE_SCRIPT="$candidate"
    break
  fi
done

if [ -n "$MERGE_SCRIPT" ]; then
  git config merge.autocontext-union.driver "python3 $MERGE_SCRIPT %O %A %B"
  echo "Merge driver configured: $MERGE_SCRIPT"
else
  # Fallback: union merge (built-in git strategy, appends both sides)
  git config merge.autocontext-union.driver "git merge-file --union %A %O %B"
  echo "Merge driver configured with built-in union fallback (merge script not found)"
fi

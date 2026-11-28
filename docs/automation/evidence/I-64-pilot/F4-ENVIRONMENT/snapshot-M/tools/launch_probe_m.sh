#!/usr/bin/env bash
# Sonda neutra con el binario nuevo. Mismo lanzador que los controles (env -C, sin cd al worktree). Sin GIT_DIR ni GIT_WORK_TREE.
wt="<worktree-de-A-14>"
test -n "$1" || exit 91
run_dir="$(cd "$(dirname "$0")/../../a14r/F4-ENVIRONMENT" && pwd)/probe-$1"  # carpeta de la sesion: nada se escribe en el worktree
test -d "$run_dir" || exit 92
codex_bin="<datos-locales-del-usuario>/OpenAI/Codex/bin/5449c1f0f26f1f98/codex.exe"
date -u +%Y-%m-%dT%H:%M:%SZ > "$run_dir/cession-start.txt"
PATH="<runtime-de-codex>/codex-primary-runtime/dependencies/native/powershell:$PATH" \
timeout --kill-after=10 600 env -C "$wt" "$codex_bin" exec -C "$wt" -s read-only -m gpt-6-luna -c 'model_reasoning_effort="high"' \
  --output-schema "$(cygpath -w "$run_dir/probe.schema.json")" \
  -o "$(cygpath -w "$run_dir/output.json")" --json "$(cat "$run_dir/prompt.md")" < /dev/null > "$run_dir/events.jsonl" 2> "$run_dir/stderr.txt" &
lpid=$!
echo "$lpid" > "$run_dir/launcher-pid.txt"
cat /proc/$lpid/winpid > "$run_dir/launcher-winpid.txt" 2>/dev/null
wait $lpid
ec=$?
echo "$ec" > "$run_dir/exit-code.txt"
date -u +%Y-%m-%dT%H:%M:%SZ > "$run_dir/cession-end.txt"
echo "exit=$ec"

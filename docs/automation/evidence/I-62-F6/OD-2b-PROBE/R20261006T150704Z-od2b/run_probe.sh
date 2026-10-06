#!/usr/bin/env bash
# OD-2b-PROBE runner (decisiones §47). Usage: run_probe.sh <n> <winDir> <model> <expectedFingerprint>
# Read-only sandbox. Fingerprint revalidated BEFORE the invocation: any mismatch => P-01 STOP, no invocation.
set -u
N="$1"; DIR="$2"; MODEL="$3"; EXPECT="$4"
SP="<scratchpad>/i62/f6/od2b"
SPW="$(cygpath -w "$SP")"
CODEX="$LOCALAPPDATA/OpenAI/Codex/bin/8aaf1547b825b104/codex.exe"
RTPS="$USERPROFILE/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell"
SESS="$USERPROFILE/.codex/sessions/2026/10/06"
UDIR="$(cygpath -u "$DIR")"

FP=$(pwsh -NoProfile -File "$SP/../cfg-fp.ps1" -Out "$SP/probe$N-cfg-before.json" | tr -d '\r')
python "$SP/cfg_projects.py" 091540ED2DE6CFAC5C12110337305FFCABCEEEE0D8CB696F7E04EA057F9F3D66 "$EXPECT" > "$SP/probe$N-cfgproj-before.json"
echo "fingerprint-before=$FP"
if [ "$FP" != "$EXPECT" ]; then echo "P-01 STOP: fingerprint $FP != expected $EXPECT; no invocation"; exit 3; fi
sha256sum "$CODEX" | cut -d' ' -f1 > "$SP/probe$N-binary-sha.txt"
git -C "$UDIR" rev-parse HEAD > "$SP/probe$N-head-before.txt"
git -C "$UDIR" status --porcelain --ignored > "$SP/probe$N-status-before.txt"
ls -1 "$SESS" > "$SP/probe$N-sessions-before.txt" 2>/dev/null
date -u +%Y-%m-%dT%H:%M:%SZ > "$SP/probe$N-start.txt"
PROMPT="$(cat "$SP/probe$N-prompt.txt")"
PATH="$RTPS:$PATH" "$CODEX" exec -C "$DIR" -s read-only -m "$MODEL" -c 'model_reasoning_effort="high"' \
  --output-schema "$SPW\\probe-schema.json" -o "$SPW\\probe$N-last.json" --json "$PROMPT" \
  < /dev/null > "$SP/probe$N-events.jsonl" 2> "$SP/probe$N-stderr.txt"
echo "exit=$?" | tee "$SP/probe$N-exit.txt"
date -u +%Y-%m-%dT%H:%M:%SZ > "$SP/probe$N-end.txt"
FPA=$(pwsh -NoProfile -File "$SP/../cfg-fp.ps1" -Out "$SP/probe$N-cfg-after.json" | tr -d '\r')
python "$SP/cfg_projects.py" 091540ED2DE6CFAC5C12110337305FFCABCEEEE0D8CB696F7E04EA057F9F3D66 "$EXPECT" > "$SP/probe$N-cfgproj-after.json"
echo "fingerprint-after=$FPA"
git -C "$UDIR" rev-parse HEAD > "$SP/probe$N-head-after.txt"
git -C "$UDIR" status --porcelain --ignored > "$SP/probe$N-status-after.txt"
ls -1 "$SESS" > "$SP/probe$N-sessions-after.txt" 2>/dev/null
diff "$SP/probe$N-sessions-before.txt" "$SP/probe$N-sessions-after.txt" | grep '^>' | sed 's/^> //' > "$SP/probe$N-new-sessions.txt"
echo "head: $(cat "$SP/probe$N-head-before.txt") -> $(cat "$SP/probe$N-head-after.txt")"
echo "status-before-lines=$(wc -l < "$SP/probe$N-status-before.txt") status-after-lines=$(wc -l < "$SP/probe$N-status-after.txt")"
echo "new-sessions: $(cat "$SP/probe$N-new-sessions.txt")"

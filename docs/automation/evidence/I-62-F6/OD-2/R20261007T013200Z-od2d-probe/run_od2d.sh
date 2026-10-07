#!/usr/bin/env bash
# OD-2d-PROBE runner (decisiones §51). Usage:
#   run_od2d.sh measure <tag>                         -> fingerprint, binary, app version, key structure, per-key digest diff
#   run_od2d.sh version                               -> codex --version and codex login status, measured before/after
#   run_od2d.sh probe <n> <winDir> <model>            -> one read-only model probe, measured before/after
# Any fingerprint change versus 723A6898... => P-01 STOP (exit 3), nothing further.
set -u
SP="<scratchpad>/i62/f6"
OUT="$SP/od2d"; OUTW="$(cygpath -w "$OUT")"
EXPECT=723A68985165BAE40689172F4E573C47FC45D1BA0D19F9192E3120DDD28B18C8
CODEX="$LOCALAPPDATA/OpenAI/Codex/bin/5ea220ae823df3d7/codex.exe"
RTPS="$USERPROFILE/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell"
SESS="$USERPROFILE/.codex/sessions/2026/10/07"

measure() {
  local tag="$1"
  local fp; fp=$(pwsh -NoProfile -File "$SP/cfg-fp.ps1" -Out "$OUT/$tag-cfg.json" | tr -d '\r')
  python "$SP/od2b/cfg_keyed_digest.py" diff "$(cygpath -w "$SP")\\od2b\\cfg-keyed-723A6898.json" > "$OUT/$tag-keyed.txt"
  local bins; bins=$(ls "$LOCALAPPDATA"/OpenAI/Codex/bin/*/codex.exe 2>/dev/null | wc -l)
  local bh; bh=$(sha256sum "$CODEX" 2>/dev/null | cut -c1-64 | tr -d '\\')
  local app; app=$(powershell.exe -NoProfile -Command "(Get-AppxPackage *OpenAI.Codex*).Version" 2>/dev/null | tr -d '\r')
  echo "$tag fp=$fp bin=$bh bins=$bins app=$app keyed=[$(sed -n '2,4p' "$OUT/$tag-keyed.txt" | tr '\n' ' ')]" | tee -a "$OUT/measurements.log"
  if [ "$fp" != "$EXPECT" ]; then echo "P-01 STOP at $tag: fingerprint $fp != $EXPECT" | tee -a "$OUT/measurements.log"; exit 3; fi
}

case "$1" in
  measure) measure "$2" ;;
  version)
    measure pre-version
    date -u +%Y-%m-%dT%H:%M:%SZ > "$OUT/version-utc.txt"
    "$CODEX" --version < /dev/null > "$OUT/version.txt" 2>&1
    "$CODEX" login status < /dev/null > "$OUT/login-status.txt" 2>&1
    cat "$OUT/version.txt" "$OUT/login-status.txt"
    measure post-version ;;
  probe)
    N="$2"; DIR="$3"; MODEL="$4"; UDIR="$(cygpath -u "$DIR")"
    measure "pre-probe$N"
    git -C "$UDIR" rev-parse HEAD > "$OUT/probe$N-head-before.txt"
    git -C "$UDIR" status --porcelain --ignored > "$OUT/probe$N-status-before.txt"
    ls -1 "$SESS" > "$OUT/probe$N-sessions-before.txt" 2>/dev/null
    date -u +%Y-%m-%dT%H:%M:%SZ > "$OUT/probe$N-start.txt"
    PROMPT="$(cat "$SP/od2b/probe$N-prompt.txt")"
    PATH="$RTPS:$PATH" "$CODEX" exec -C "$DIR" -s read-only -m "$MODEL" -c 'model_reasoning_effort="high"' \
      --output-schema "$(cygpath -w "$SP")\\od2b\\probe-schema.json" -o "$OUTW\\probe$N-last.json" --json "$PROMPT" \
      < /dev/null > "$OUT/probe$N-events.jsonl" 2> "$OUT/probe$N-stderr.txt"
    echo "exit=$?" | tee "$OUT/probe$N-exit.txt"
    date -u +%Y-%m-%dT%H:%M:%SZ > "$OUT/probe$N-end.txt"
    git -C "$UDIR" rev-parse HEAD > "$OUT/probe$N-head-after.txt"
    git -C "$UDIR" status --porcelain --ignored > "$OUT/probe$N-status-after.txt"
    ls -1 "$SESS" > "$OUT/probe$N-sessions-after.txt" 2>/dev/null
    diff "$OUT/probe$N-sessions-before.txt" "$OUT/probe$N-sessions-after.txt" | grep '^>' | sed 's/^> //' > "$OUT/probe$N-new-sessions.txt"
    echo "head $(cat "$OUT/probe$N-head-before.txt") -> $(cat "$OUT/probe$N-head-after.txt"); status lines $(wc -l < "$OUT/probe$N-status-before.txt") -> $(wc -l < "$OUT/probe$N-status-after.txt"); session $(cat "$OUT/probe$N-new-sessions.txt")"
    measure "post-probe$N" ;;
esac

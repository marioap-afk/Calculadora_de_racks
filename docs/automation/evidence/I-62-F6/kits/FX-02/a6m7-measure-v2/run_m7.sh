#!/usr/bin/env bash
# run_m7.sh - the single A6-7 measurement of I-62 (one read-only codex-cli invocation of the FX-02 EXECUTION_CONTROLLER with
# the B-min instrument), with its gates, the point-of-use check, the P-07 marker, the launch per controller-contracts.md §4
# and the post-launch pipeline. Prepared by the supervision; NOT LAUNCHED by whoever built it.
#
# Exactly one of (no other flag, no other form):
#   bash run_m7.sh launch              gates -> point of use -> P-07 marker -> ONE launch -> post (never a retry)
#   bash run_m7.sh --dry-run <dir>     every gate against the fixture <dir> (temporary decisions file, Owner lines, identity
#                                      fixtures, scratch -C); stops before the marker and the launch
#   bash run_m7.sh --selftest-launch <dir>   self-tests only: the same sequence with the fixture's STUB binary (a temporary
#                                      directory, never the real Codex path: gates_m7.py refuses it), fixture marker and -C
# Before "launch": the Coordinator's measurement disposition and both Owner lines in the decisions file, prep_write.py done
# (A6-3 rule 1), the Codex desktop app closed (no codex.exe may run), I-64 without critical work.
set -u
K="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
KW="$(cygpath -w "$K")"
PY="$LOCALAPPDATA/Microsoft/WindowsApps/python.exe"          # the declared interpreter also runs the supervision scripts
[ -x "$PY" ] || [ -e "$PY" ] || { echo "REFUSED: declared interpreter absent" >&2; exit 2; }

MODE="${1:-}"
FIXARGS=()
POSTARGS=()
case "$MODE" in
  launch) [ "$#" -eq 1 ] || { echo "uso: bash run_m7.sh launch   (sin otros argumentos)" >&2; exit 64; } ;;
  --dry-run) [ "$#" -eq 2 ] && [ -f "$2/fixture.json" ] || { echo "uso: bash run_m7.sh --dry-run <dir con fixture.json>" >&2; exit 64; }
             FIXARGS=(--mode dry-run --fixture "$(cygpath -w "$2")") ;;
  --selftest-launch) [ "$#" -eq 2 ] && [ -f "$2/fixture.json" ] || { echo "uso: bash run_m7.sh --selftest-launch <dir>" >&2; exit 64; }
             FIXARGS=(--mode selftest-launch --fixture "$(cygpath -w "$2")"); POSTARGS=(--fixture "$(cygpath -w "$2")") ;;
  *) echo "uso: bash run_m7.sh launch | bash run_m7.sh --dry-run <dir>" >&2; exit 64 ;;
esac

eval "$("$PY" -I "$KW\\gates_m7.py" env-sh "${FIXARGS[@]}")" || { echo "REFUSED: parameters unreadable" >&2; exit 2; }
mkdir -p "$OUT" "$RAW"
refuse() { echo "REFUSED: $*" | tee -a "$OUT/refusals.log" >&2; exit 2; }

# ---------------------------------------------------------------- launch environment of the child (controller-contracts §4)
# PATH = the runtime pwsh directory first, then the Windows PATH (Machine;User from the registry), never the PATH of this Git
# Bash (whose /mingw64/bin would make "git" resolve to another git.exe); every GIT_* variable removed.
WINPATH="$(powershell.exe -NoProfile -Command "[Environment]::GetEnvironmentVariable('Path','Machine') + ';' + [Environment]::GetEnvironmentVariable('Path','User')" | tr -d '\r')"
[ -n "$WINPATH" ] || refuse "Windows PATH unreadable"
CHILD_PATH="$(cygpath -u "$RTPS_DIR_WIN"):$(cygpath -u -p "$WINPATH")"
UNSET=()
for v in $(compgen -e); do case "${v^^}" in GIT_*) UNSET+=(-u "$v") ;; esac; done
# GNU timeout by absolute path: with the child PATH, a bare "timeout" would be C:\Windows\System32\timeout.exe (another program)
TIMEOUT_BIN="$(command -v timeout)"
"$TIMEOUT_BIN" --version 2>/dev/null | head -1 | grep -q 'coreutils' || refuse "GNU timeout not found ($TIMEOUT_BIN)"
run_child() { env "${UNSET[@]}" PATH="$CHILD_PATH" MSYS2_ARG_CONV_EXCL='*' MSYS_NO_PATHCONV=1 "$@"; }
# the environment exactly as a native child receives it (the point-of-use check (vi) and the acceptor use this one)
run_child "$PY" -I -c "import json,os,sys; sys.stdout.write(json.dumps(dict(os.environ), ensure_ascii=True, sort_keys=True))" > "$RAW/launch-env.json" \
  || refuse "launch environment not captured"

# ---------------------------------------------------------------- gates (refuse before launching)
"$PY" -I "$KW\\gates_m7.py" gates "${FIXARGS[@]}" || refuse "gates (see $OUT/gates.json)"
"$PY" -I "$KW\\gates_m7.py" decl "${FIXARGS[@]}" || refuse "declaration for the point of use not written"
"$PY" -I "$KW\\pou_m7.py" --repo "$REPO" --decl "$(cygpath -w "$RAW/decl.json")" --registered-config "$(cygpath -w "$OUT/registered-config.json")" \
  --launch-env-json "$(cygpath -w "$RAW/launch-env.json")" --out "$(cygpath -w "$RAW/pou.json")" || refuse "point-of-use check (i)-(viii) (see $RAW/pou.json)"
"$PY" -I "$KW\\gates_m7.py" procs after-pou "${FIXARGS[@]}" || refuse "P-02 after the point-of-use check"
"$PY" -I "$KW\\gates_m7.py" snap "$(cygpath -w "$RAW/snap-before.json")" "${FIXARGS[@]}" || refuse "snapshot before the launch"
echo "ALL GATES PASSED $(date -u +%Y-%m-%dT%H:%M:%SZ)" | tee "$OUT/gates-passed.txt"

if [ "$MODE" = "--dry-run" ]; then
  echo "DRY-RUN: stopping before the P-07 marker and the launch (nothing launched)"
  exit 0
fi

# ---------------------------------------------------------------- P-07 marker (tope 1), created before the launch, never deleted
( set -o noclobber; date -u +%Y-%m-%dT%H:%M:%SZ > "$MARKER" ) 2>/dev/null || refuse "P-07: the launch marker exists or cannot be created"

# ---------------------------------------------------------------- the single launch (controller-contracts.md §4)
PROMPT="$(cat "$RAW/prompt.txt")"
OUTPUT_WIN="$REPO_WIN\\$(printf '%s' "$IGN" | tr '/' '\\')"
START="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
run_child "$TIMEOUT_BIN" --kill-after="$KILL_AFTER" "$TIMEOUT_S" "$CODEX" exec -C "$REPO_WIN" -s read-only -m "$MODEL" \
  -c "model_reasoning_effort=\"$EFFORT\"" --output-schema "$KW\\schema.m7.strict-projection.json" -o "$OUTPUT_WIN" --json "$PROMPT" \
  < /dev/null > "$RAW/events.jsonl" 2> "$RAW/stderr.txt" &
CPID=$!
T=0; WATCHDOG=no
while kill -0 "$CPID" 2>/dev/null; do
  if [ "$T" -ge $(( TIMEOUT_S - 5 )) ]; then
    WATCHDOG=yes
    "$PY" -I "$KW\\gates_m7.py" killtree "${FIXARGS[@]}" > "$RAW/killtree.txt" 2>&1   # taskkill /T /F of the codex.exe tree while it lives
    break
  fi
  sleep 5; T=$((T + 5))
done
wait "$CPID"; RC=$?
END="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
DEAD=no
for i in $(seq 1 12); do
  if "$PY" -I "$KW\\gates_m7.py" procs after-launch "${FIXARGS[@]}" > /dev/null 2>&1; then DEAD=yes; break; fi
  "$PY" -I "$KW\\gates_m7.py" killtree "${FIXARGS[@]}" > /dev/null 2>&1; sleep 5
done
TO=false; { [ "$RC" = "124" ] || [ "$RC" = "137" ] || [ "$WATCHDOG" = "yes" ]; } && TO=true
DC=false; [ "$DEAD" = "yes" ] && DC=true
printf '{"Exit": %s, "TimedOut": %s, "Watchdog": "%s", "ElapsedS": %s, "DeathConfirmed": %s, "StartUtc": "%s", "EndUtc": "%s", "CapS": %s, "KillAfterS": %s}\n' \
  "$RC" "$TO" "$WATCHDOG" "$T" "$DC" "$START" "$END" "$TIMEOUT_S" "$KILL_AFTER" > "$OUT/launch.json"
echo "launch: exit=$RC timed_out=$TO death_confirmed=$DC elapsed<=${T}s; no retry, whatever the result"

# ---------------------------------------------------------------- after: identity, Entry, events, session record, Runs, request,
# acceptor (same -C, after Entry, read-only), table, comparison, custody
"$PY" -I "$KW\\tools_m7.py" post "${POSTARGS[@]}"
echo "Fin. Sin reintento. Siguiente: disposicion de elegibilidad del Coordinator sobre $OUT/comparison.json y la custodia."

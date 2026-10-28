#!/usr/bin/env bash
# selftest_m7.sh - runs the self-tests of the A6-7 measurement kit (I-62) without launching anything: bash -n of the two
# shell scripts, then selftest_m7.py (static checks, reconstruction rule v2, preparation write and point of use on scratch
# copies, synthetic session through the declared form in the PATH pwsh 7, real acceptor, comparator positive and negatives, gate
# refusal matrix, dry-run and stub launch through run_m7.sh). Codex and the runtime pwsh are never executed.
#   bash selftest_m7.sh [--keep]
set -u
K="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PY="$LOCALAPPDATA/Microsoft/WindowsApps/python.exe"
bash -n "$K/run_m7.sh" && bash -n "$K/selftest_m7.sh" || { echo "bash -n failed" >&2; exit 1; }
exec "$PY" -I "$(cygpath -w "$K")\\selftest_m7.py" "$@"

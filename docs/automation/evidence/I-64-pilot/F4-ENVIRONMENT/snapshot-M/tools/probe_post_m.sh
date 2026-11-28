#!/usr/bin/env bash
S="$(cd "$(dirname "$0")" && pwd)"; F="$S/../f2t1"; RID=$(cat "$S/probe-runid-m.txt")
R="$(cd "$S/../../a14r" && pwd)"  # carpeta de la sesion; nada se escribe en el worktree de A-14
D="$R/F4-ENVIRONMENT/probe-$RID"
echo "exit $(cat "$D/exit-code.txt") cession $(cat "$D/cession-start.txt") .. $(cat "$D/cession-end.txt")"
pwsh -NoProfile -File "$(cygpath -w "$S/proc_check_m.ps1")" -OutPath "$(cygpath -w "$D/processes-entry.json")" > /dev/null; echo "proc_ec=$?"
python -I "$S/cfg_check_m.py" "$D/config-entry.json" > /dev/null; echo "cfg_ec=$?"
python -I "$S/probe_state_m.py" "$(cygpath -w "$D/state-entry.json")"; echo "state_ec=$?"
python -I "$S/../f4/show_procs.py" "$D/processes-entry.json"
python -I "$S/eval_probe_m.py" "$RID"; echo "eval_ec=$?"

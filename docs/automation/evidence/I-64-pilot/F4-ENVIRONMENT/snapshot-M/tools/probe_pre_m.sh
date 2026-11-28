#!/usr/bin/env bash
S="$(cd "$(dirname "$0")" && pwd)"; F="$S/../f2t1"
R="$(cd "$S/../../a14r" && pwd)"  # carpeta de la sesion; nada se escribe en el worktree de A-14
test ! -e "$S/probe-runid-m.txt" || { echo "ya preparada"; exit 93; }
RID="R$(date -u +%Y%m%dT%H%M%SZ)-$(od -An -N2 -tx1 /dev/urandom | tr -d ' \n')"
echo "$RID" > "$S/probe-runid-m.txt"; echo "RUNID $RID"
python -I "$S/make_probe_m.py" "$RID" || exit 92
D="$R/F4-ENVIRONMENT/probe-$RID"
python -I "$S/probe_state_m.py" "$(cygpath -w "$D/state-exit.json")"; echo "state_ec=$?"
python -I "$S/cfg_check_m.py" "$D/config-exit.json" > /dev/null; echo "cfg_ec=$?"
pwsh -NoProfile -File "$(cygpath -w "$S/proc_check_m.ps1")" -OutPath "$(cygpath -w "$D/processes-exit.json")" > /dev/null; echo "proc_ec=$?"
python -I "$S/../f4/show_procs.py" "$D/processes-exit.json"

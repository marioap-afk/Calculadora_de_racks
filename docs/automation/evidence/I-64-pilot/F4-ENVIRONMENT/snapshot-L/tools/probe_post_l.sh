#!/usr/bin/env bash
S="$(cd "$(dirname "$0")" && pwd)"; F="$S/../f2t1"; RID=$(cat "$S/probe-runid-l.txt")
wt=<worktree-de-I-64>
D="$wt/artifacts/orchestration/I-64/F4-ENVIRONMENT/probe-$RID"
echo "exit $(cat "$D/exit-code.txt") cession $(cat "$D/cession-start.txt") .. $(cat "$D/cession-end.txt")"
pwsh -NoProfile -File "$(cygpath -w "$F/proc_check4.ps1")" -OutPath "$(cygpath -w "$D/processes-entry.json")" > /dev/null; echo "proc_ec=$?"
python "$S/cfg_check_l.py" "$D/config-entry.json" > /dev/null; echo "cfg_ec=$?"
python "$S/probe_state_l.py" "$(cygpath -w "$D/state-entry.json")"; echo "state_ec=$?"
python "$S/../f4/show_procs.py" "$D/processes-entry.json"
python "$S/eval_probe_l.py" "$RID"; echo "eval_ec=$?"

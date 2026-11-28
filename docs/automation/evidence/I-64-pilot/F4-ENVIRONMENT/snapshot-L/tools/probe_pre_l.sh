#!/usr/bin/env bash
S="$(cd "$(dirname "$0")" && pwd)"; F="$S/../f2t1"
wt=<worktree-de-I-64>
test ! -e "$S/probe-runid-l.txt" || { echo "ya preparada"; exit 93; }
RID="R$(date -u +%Y%m%dT%H%M%SZ)-$(python -c 'import secrets;print(secrets.token_hex(2))')"
echo "$RID" > "$S/probe-runid-l.txt"; echo "RUNID $RID"
python "$S/make_probe_l.py" "$RID" || exit 92
D="$wt/artifacts/orchestration/I-64/F4-ENVIRONMENT/probe-$RID"
python "$S/probe_state_l.py" "$(cygpath -w "$D/state-exit.json")"; echo "state_ec=$?"
python "$S/cfg_check_l.py" "$D/config-exit.json" > /dev/null; echo "cfg_ec=$?"
pwsh -NoProfile -File "$(cygpath -w "$F/proc_check4.ps1")" -OutPath "$(cygpath -w "$D/processes-exit.json")" > /dev/null; echo "proc_ec=$?"
python "$S/../f4/show_procs.py" "$D/processes-exit.json"

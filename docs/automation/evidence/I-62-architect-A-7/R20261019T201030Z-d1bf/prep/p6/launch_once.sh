# Decisiones §95, puntos 4 y 5 (sesión principal): precondiciones finales y UNA invocación de la orden congelada. Se detiene antes de lanzar si falla
# cualquier precondición. Registro en launch_once.log; la retención de suspensión (hold_awake.py, con pantalla) termina cuando el registro dice DONE o STOP.
K=/d/r62-a7/kit-review2
P=$K/prep/p6
L=$P/launch_once.log
ts() { date -u +%Y-%m-%dT%H:%M:%SZ; }
stop() { echo "$(ts) STOP $1" >> $L; exit 1; }
: > $L
# 1. host (solo lectura): checkout principal, I-64, worktree de I-62, clones, procesos, carga
python -I -B $K/prep/p3/host_checks_r2.py > $P/host-checks-prelaunch.json 2> $P/host-checks-prelaunch.err || stop host-checks
python -I -B - $P/host-checks-prelaunch.json <<'EOF' >> $L 2>&1 || stop host-conditions
import json, sys
d = json.load(open(sys.argv[1], encoding="utf-8"))
ok = (d["MainCheckout"]["Branch"] == "main" and d["MainCheckout"]["Head"].startswith("95690c28") and d["MainCheckout"]["StatusLines"] == 0 and
      d["I64"]["Head"] == "09f2f1c3" and d["I64"]["StatusLines"] == 0 and d["W"]["StatusLines"] == 0 and
      d["R2Clone"]["Head"].startswith("b432c40f") and d["R2Clone"]["StatusLinesWithIgnored"] == 0 and
      not any("claude.exe" == p["Name"].lower() for p in d["Processes"]))
print("host", ok, d["UtcTime"], [p["Name"] for p in d["Processes"]])
sys.exit(0 if ok else 1)
EOF
# 2. alimentación
pl=$(powershell.exe -NoProfile -Command "Add-Type -AssemblyName System.Windows.Forms; [System.Windows.Forms.SystemInformation]::PowerStatus.PowerLineStatus" 2>/dev/null | tr -d '\r ')
echo "$(ts) PowerLineStatus $pl" >> $L
[ "$pl" = "Online" ] || stop power
# 3. sonda y prueba en seco finales (orden congelada exacta)
(cd $K && python -B kit/launch.py --record-probe > $P/record-probe-final.raw 2> $P/record-probe-final.err); rc=$?
echo "$(ts) record-probe rc=$rc" >> $L; [ $rc -eq 0 ] || stop record-probe
(cd $K && python -B kit/launch.py --dry-run > $P/dry-run-final.raw 2> $P/dry-run-final.err); rc=$?
echo "$(ts) dry-run rc=$rc" >> $L; [ $rc -eq 2 ] || stop dry-run
(cd $K && python -I -B prep/p5/check_dry_zero.py kit prep/p6/dry-run-final.raw prep/p6/launch-dry-run-final.json >> $L 2>&1) || stop dry-run-not-zero
# 4. UNA invocación
echo "$(ts) LAUNCH START" >> $L
date -u +%Y-%m-%dT%H:%M:%SZ > $P/launch-start.txt
(cd $K && python -B kit/launch.py > $P/launch-run.log 2>&1); rc=$?
date -u +%Y-%m-%dT%H:%M:%SZ > $P/launch-end.txt
echo "$(ts) LAUNCH END rc=$rc" >> $L
echo "$(ts) DONE" >> $L

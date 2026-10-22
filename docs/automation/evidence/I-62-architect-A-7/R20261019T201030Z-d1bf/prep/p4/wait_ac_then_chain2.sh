# Espera a que el equipo tenga corriente (Win32_Battery.BatteryStatus distinto de 1 = descargando) y entonces lanza chain2.sh con la retención de
# suspensión por inactividad (hold_awake.py, ligada a chain2.log). Mientras espera no retiene nada.
P=/d/r62-a7/kit-review2/prep/p4
H='%TEMP%/claude/<proyecto>/<sesión>/scratchpad/i62/day5/a7r2/hold_awake.py'   # ruta del scratchpad de la sesión, redactada; copia en prep/p4/hold_awake.py
echo "$(date -u +%Y-%m-%dT%H:%M:%SZ) esperando corriente" > $P/wait-ac.log
while true; do
  b=$(powershell.exe -NoProfile -Command "(Get-CimInstance Win32_Battery).BatteryStatus" 2>/dev/null | tr -d '\r ')
  if [ -n "$b" ] && [ "$b" != "1" ]; then break; fi
  sleep 60
done
echo "$(date -u +%Y-%m-%dT%H:%M:%SZ) corriente (BatteryStatus $b): chain2" >> $P/wait-ac.log
: > $P/chain2.log
python -I -B "$H" 'D:\r62-a7\kit-review2\prep\p4\chain2.log' 5 >> $P/wait-ac.log 2>&1 &
bash $P/chain2.sh
echo "$(date -u +%Y-%m-%dT%H:%M:%SZ) chain2 exit=$?" >> $P/wait-ac.log
wait
cat $P/wait-ac.log $P/chain2.log

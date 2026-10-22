# Continuación de chain.sh (sesión principal, 2026-10-20) si la batería obligó a detenerla antes de la mutación: paso 12 de finish_kit.py (batería
# COMPLETA) y después 9-11 otra vez, con las mismas órdenes. Se registra en chain2.log; se detiene en el primer código inesperado.
K=/d/r62-a7/kit-review2
P=$K/prep/p4
L=$P/chain2.log
ts() { date -u +%Y-%m-%dT%H:%M:%SZ; }
run() {  # nombre, código esperado, directorio, orden...
  local name=$1 want=$2 dir=$3; shift 3
  echo "$(ts) START $name" >> $L
  (cd $dir && "$@" > $P/$name.raw 2> $P/$name.err); local rc=$?
  echo "$(ts) END $name rc=$rc" >> $L
  if [ $rc -ne $want ]; then echo "$(ts) STOP $name (esperado $want)" >> $L; exit 1; fi
}
: > $L
run s12-mutation 0 $K/kit python -X utf8 -B mutation-post-review.py 'D:\r62-a7\kit-review2\kit\mutation-result.json'
run s9b-manifest 0 $K/kit python -X utf8 -B make_manifest.py
run s10b-record-probe 0 $K python -B kit/launch.py --record-probe
run s11b-dry-run 2 $K python -B kit/launch.py --dry-run
run s11b-check 0 $K python -I -B prep/p4/check_dry.py kit prep/p4/s11b-dry-run.raw launch-dry-run.json
echo "$(ts) DONE" >> $L

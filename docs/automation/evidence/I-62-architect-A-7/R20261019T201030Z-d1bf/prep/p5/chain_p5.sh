# Decisiones §95, punto 4 (sesión principal): controles repetidos tras fijar §95 en owner_gate.py, nueva congelación, sonda y prueba en seco con cero
# bloqueos. Se registra en chain_p5.log; se detiene en el primer código inesperado. Sonda y prueba en seco con la orden congelada exacta.
K=/d/r62-a7/kit-review2
P=$K/prep/p5
L=$P/chain_p5.log
ts() { date -u +%Y-%m-%dT%H:%M:%SZ; }
run() {  # nombre, código esperado, directorio, orden...
  local name=$1 want=$2 dir=$3; shift 3
  echo "$(ts) START $name" >> $L
  (cd $dir && "$@" > $P/$name.raw 2> $P/$name.err); local rc=$?
  echo "$(ts) END $name rc=$rc" >> $L
  if [ $rc -ne $want ]; then echo "$(ts) STOP $name (esperado $want)" >> $L; exit 1; fi
}
: > $L
run s8-selftest-pinned 0 $K/kit python -X utf8 -B selftest-post-review.py 'D:\r62-a7\kit-review2\kit\selftest-result.json'
run m-owner-gate-pinned 0 $K/kit python -X utf8 -B ../prep/p5/mutation_owner_gate.py 'D:\r62-a7\kit-review2\kit' 'D:\r62-a7\kit-review2\prep\p5\mutation-owner-gate-pinned.json'
run s9-manifest-pinned 0 $K/kit python -X utf8 -B make_manifest.py
run s10-record-probe-pinned 0 $K python -B kit/launch.py --record-probe
run s11-dry-run-pinned 2 $K python -B kit/launch.py --dry-run
run s11-check-pinned 0 $K python -I -B prep/p5/check_dry_zero.py kit prep/p5/s11-dry-run-pinned.raw prep/p5/launch-dry-run-pinned.json
echo "$(ts) DONE" >> $L

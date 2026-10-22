# Pasos 8-12 de finish_kit.py para el kit de la ronda 2 (sesión principal, 2026-10-20), uno a uno con sus mismas órdenes, y el registro de tiempos.
# Se detiene en el primer código inesperado. La sonda y la prueba en seco con la orden congelada exacta, desde la carpeta que contiene kit/.
K=/d/r62-a7/kit-review2
P=$K/prep/p4
L=$P/chain.log
ts() { date -u +%Y-%m-%dT%H:%M:%SZ; }
run() {  # nombre, código esperado, directorio, orden...
  local name=$1 want=$2 dir=$3; shift 3
  echo "$(ts) START $name" >> $L
  (cd $dir && "$@" > $P/$name.raw 2> $P/$name.err); local rc=$?
  echo "$(ts) END $name rc=$rc" >> $L
  if [ $rc -ne $want ]; then echo "$(ts) STOP $name (esperado $want)" >> $L; exit 1; fi
}
: > $L
run s8-selftest 0 $K/kit python -X utf8 -B selftest-post-review.py 'D:\r62-a7\kit-review2\kit\selftest-result.json'
run s9-manifest 0 $K/kit python -X utf8 -B make_manifest.py
run s10-record-probe 0 $K python -B kit/launch.py --record-probe
run s11-dry-run 2 $K python -B kit/launch.py --dry-run
run s11-check 0 $K python -I -B prep/p4/check_dry.py kit prep/p4/s11-dry-run.raw launch-dry-run-before-full-mutation.json
cp $P/s10-record-probe.raw $P/s10-record-probe.keep
run s12-mutation 0 $K/kit python -X utf8 -B mutation-post-review.py 'D:\r62-a7\kit-review2\kit\mutation-result.json'
run s9b-manifest 0 $K/kit python -X utf8 -B make_manifest.py
run s10b-record-probe 0 $K python -B kit/launch.py --record-probe
run s11b-dry-run 2 $K python -B kit/launch.py --dry-run
run s11b-check 0 $K python -I -B prep/p4/check_dry.py kit prep/p4/s11b-dry-run.raw launch-dry-run.json
echo "$(ts) DONE" >> $L

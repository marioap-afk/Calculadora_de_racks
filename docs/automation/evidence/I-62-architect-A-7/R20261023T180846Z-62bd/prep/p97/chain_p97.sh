# Decisiones §97, puntos 2 y 4 (sesión principal): tras fijar §97 en owner_gate.py y la validación incremental (prep/p97/targeted-validation.json),
# nueva congelación, sonda de registro y prueba en seco con cero bloqueos, con la orden congelada exacta desde la carpeta que contiene kit/. Se registra
# en chain_p97.log; se detiene en el primer código inesperado.
K=/d/r62-a7/kit-review3
P=$K/prep/p97
L=$P/chain_p97.log
ts() { date -u +%Y-%m-%dT%H:%M:%SZ; }
run() {  # nombre, código esperado, directorio, orden...
  local name=$1 want=$2 dir=$3; shift 3
  echo "$(ts) START $name" >> $L
  (cd $dir && "$@" > $P/$name.raw 2> $P/$name.err); local rc=$?
  echo "$(ts) END $name rc=$rc" >> $L
  if [ $rc -ne $want ]; then echo "$(ts) STOP $name (esperado $want)" >> $L; exit 1; fi
}
: > $L
run s9-manifest-pinned 0 $K/kit python -X utf8 -B make_manifest.py
run s10-record-probe-pinned 0 $K python -B kit/launch.py --record-probe
run s11-dry-run-pinned 2 $K python -B kit/launch.py --dry-run
run s11-check-pinned 0 $K python -I -B prep/p97/check_dry_zero.py kit prep/p97/s11-dry-run-pinned.raw prep/p97/launch-dry-run-pinned.json
echo "$(ts) DONE" >> $L

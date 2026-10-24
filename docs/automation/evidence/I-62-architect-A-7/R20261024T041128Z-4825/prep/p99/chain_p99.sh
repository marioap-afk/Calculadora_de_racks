# Decisiones §99, puntos 2-4 (sesión principal): tras fijar §99 en owner_gate.py y la validación incremental (prep/p99/targeted-validation.json),
# nueva congelación, sonda de registro y prueba en seco con cero bloqueos, con la orden congelada exacta desde la carpeta que contiene kit/. Se registra
# en chain_p97.log; se detiene en el primer código inesperado.
K=/d/r62-a7/kit-review4
P=$K/prep/p99
L=$P/chain_p99.log
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
run s11-check-pinned 0 $K python -I -B prep/p99/check_dry_zero.py kit prep/p99/s11-dry-run-pinned.raw prep/p99/launch-dry-run-pinned.json
run s10-convert-pinned 0 $K python -I -B prep/convert_probe.py prep/p99/s10-record-probe-pinned.raw prep/p99/record-probe-pinned.json
run s13-regen-pinned 0 $K python -I -B prep/regen_faithful_r4.py kit 'D:\r62-arch-a7r4' prep/p99/faithful-regeneration-pinned.json
echo "$(ts) DONE" >> $L

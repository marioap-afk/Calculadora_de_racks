# Pasos 8b-12 de finish_kit.py para el kit de la ronda 4 (sesión principal), uno a uno con sus mismas órdenes, y el registro de tiempos, después
# del paso 8 (selftest-post-review.py, ejecutado aparte sobre los módulos finales: kit/selftest-result.json). Añade la negativa de verify_clone.py
# (decisiones §98, punto 6: validación incremental; verify_clone.py cambió). El paso 12 es la mutación DIRIGIDA (kit/targeted-mutation.py; §98,
# punto 6). Se detiene en el primer código inesperado. La sonda y la prueba en seco con la orden congelada exacta, desde la carpeta que contiene
# kit/. Al terminar escribe DONE (o STOP) en chain.log.
K=/d/r62-a7/kit-review4
P=$K/prep
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
run s8b-selftest-fidelity 0 $K/kit python -X utf8 -B selftest-fidelity.py 'D:\r62-a7\kit-review4\kit\selftest-fidelity-result.json'
run s8c-neg-verify-clone 0 $K python -I -B prep/neg_verify_clone_r4.py kit 'D:\r62-arch-a7r4-run' prep 'D:\r62-a7\kit-review4\prep\neg-verify-result-r4.json'
run s9-manifest 0 $K/kit python -X utf8 -B make_manifest.py
run s10-record-probe 0 $K python -B kit/launch.py --record-probe
run s11-dry-run 2 $K python -B kit/launch.py --dry-run
run s11-check 0 $K python -I -B prep/check_dry.py kit prep/s11-dry-run.raw launch-dry-run-before-mutation.json
cp $P/s10-record-probe.raw $P/s10-record-probe.keep
run s12-mutation 0 $K/kit python -X utf8 -B targeted-mutation.py 'D:\r62-a7\kit-review4\kit\targeted-mutation-result.json'
run s9b-manifest 0 $K/kit python -X utf8 -B make_manifest.py
run s10b-record-probe 0 $K python -B kit/launch.py --record-probe
run s10b-convert 0 $K python -I -B prep/convert_probe.py prep/s10b-record-probe.raw record-probe.json
run s11b-dry-run 2 $K python -B kit/launch.py --dry-run
run s11b-check 0 $K python -I -B prep/check_dry.py kit prep/s11b-dry-run.raw launch-dry-run.json
run s13-regen 0 $K python -I -B prep/regen_faithful_r4.py kit 'D:\r62-arch-a7r4' prep/faithful-regeneration.json
echo "$(ts) DONE" >> $L

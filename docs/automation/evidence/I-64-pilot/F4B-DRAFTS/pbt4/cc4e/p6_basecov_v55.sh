#!/usr/bin/env bash
# PB-T4e (paquete v5.5), precondicion 6, la REFERENCIA de la cobertura y de los MinSelected: la base sin cambios (160e019d), en una copia
# EXPORTADA nueva, sola (comprueba que no hay otro dotnet ni testhost):
#   1. git archive de 160e019d; la compilacion de Core y UI (Debug), sin warning CS;
#   2. la suite COMPLETA de Core con coverlet (JSON y Cobertura; cov_v55.runsettings: Domain, Application y UI); su TRX es la base de Core;
#   3. la union de los 19 filtros de UI del contrato (ui_union_v55.txt) tres veces, con cobertura;
#   4. cada uno de los 19 filtros en su proceso, con cobertura (el de la clase nueva no selecciona nada en la base).
# Uso: bash p6_basecov_v55.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=160e019d
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
UIF="$S/ui_filters_pbt4e_v55.txt"
UNION="$(cat "$S/ui_union_v55.txt")"
CHK="$S/check_trx_v55.py"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-bc"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
procs() { tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost'; }
echo "precondicion 6 de PB-T4e (v5.5), la base con cobertura; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
echo "procesos al empezar: $(procs) ($(date '+%H:%M:%S'))"
test "$(procs)" -eq 0 || { echo "hay otro dotnet o testhost: la base tiene que correr sola"; exit 4; }
for f in "$0" "$CHK" "$S/cov_v55.runsettings" "$UIF" "$S/ui_union_v55.txt"; do echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"; done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
echo "copia exportada: $(find "$C" -type f | wc -l) archivos"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
build() {
  ( cd "$C" && "$DN" build "$2" -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  echo "$1 rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log") avisos-CS=$(grep -c 'warning CS' "$OUT/$1.log")"
}
cov() {
  local j x; j=$(find "$C/_trx/$1" -name 'coverage.json' | head -1); x=$(find "$C/_trx/$1" -name 'coverage.cobertura.xml' | head -1)
  test -n "$j" && cp "$j" "$OUT/$1.coverage.json"; test -n "$x" && cp "$x" "$OUT/$1.cobertura.xml"
  echo "  cobertura json=$(test -n "$j" && sha256sum "$OUT/$1.coverage.json" | cut -c1-16)"
}
run() { # $1 etiqueta, $2 proyecto, $3 filtro (vacio: la suite completa)
  local t0=$(date +%s)
  ( cd "$C" && "$DN" test "$2" -c Debug --no-build ${3:+--filter "$3"} --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" \
      --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v55.runsettings")" ) > "$OUT/$1.log" 2>&1
  local rc=$? t1=$(date +%s); lf "$OUT/$1.log"
  if test -f "$C/_trx/$1/$1.trx"; then
    cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
    python -I "$CHK" "$OUT/$1.trx" "$OUT/$1-res.txt" > /dev/null
    echo "$1 rc=$rc segundos=$((t1 - t0)) :: $(head -1 "$OUT/$1-res.txt")"; sed -n '2,10p' "$OUT/$1-res.txt" | grep -v '^no pasan: 0' | cut -c1-260
    cov "$1"
  else
    echo "$1 rc=$rc :: sin TRX (ningun resultado)"
  fi
}
build b-core tests/RackCad.Tests/RackCad.Tests.csproj
build b-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
run base-core-cov tests/RackCad.Tests/RackCad.Tests.csproj ''
run base-ui-cov tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "$UNION"
run base-ui-cov-r-1 tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "$UNION"
run base-ui-cov-r-2 tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "$UNION"
n=0
while IFS= read -r f; do
  test -z "$f" && continue
  n=$((n + 1)); run "base-uif$(printf '%02d' $n)" tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "$f"
done < "$UIF"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "procesos al acabar: $(procs) ($(date '+%H:%M:%S'))"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

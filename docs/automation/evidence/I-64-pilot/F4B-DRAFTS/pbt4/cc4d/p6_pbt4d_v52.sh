#!/usr/bin/env bash
# PB-T4d (paquete v5.2), la precondicion 6, pasos 1 a 7, en una copia EXPORTADA nueva de 6c2820ba, fuera de todo worktree (nada en el checkout
# principal salvo git archive):
#   1. git archive de 6c2820ba;
#   2. el RED literal: apply_red_pbt4d_v52.py con pbt4d-red-v52.txt (todo lo de tests/, src/ intacto: D-S11);
#   3. la compilacion del RED (Core y UI, Debug), sin ningun warning CS;
#   4. el RED exacto: la suite COMPLETA de Core (solo cae CT-PBT4D-01, por asercion) y los 18 filtros de UI heredados (0 Failed);
#   5. sobre la misma copia, el GREEN literal: apply_green_pbt4d_v52.py; la compilacion de Core y UI (Debug) y del Plugin (Release, -o), sin warning CS;
#   6. el GREEN: la suite COMPLETA de Core (0 Failed) y los 18 filtros de UI (0 Failed), con la duracion de la suite y la de la prueba lenta;
#   7. la cobertura del GREEN, rama por rama: la suite COMPLETA de Core otra vez, con coverlet en JSON y Cobertura (cov_v52b.runsettings).
# Las tablas por clase (classes_v51.py de PB-T4c, sin cambio). El SDK de usuario, -nodeReuse:false -p:UseSharedCompilation=false; build-server
# shutdown al final. UTF-8 con LF; nombres cortos (<copia>-p6/).
# Uso: bash p6_pbt4d_v52.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
UIF="$S/../../pbt4ccc/compile/ui_filters_pbt4c_v50b.txt"
CLS="$S/../../pbt4ccc/compile/classes_v51.py"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-p6"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "precondicion 6 de PB-T4d; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PB4/pbt4d-red-v52.txt" "$PB4/red_pbt4d_v52.py" "$PB4/apply_red_pbt4d_v52.py" "$PB4/pbt4d-green-v52.txt" "$PB4/green_pbt4d_v52.py" \
         "$PB4/apply_green_pbt4d_v52.py" "$0" "$S/p6d_check_v52.py" "$S/cov_v52b.runsettings" "$UIF" "$CLS"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
echo "copia exportada: $(find "$C" -type f | wc -l) archivos"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
build() { # $1 etiqueta, $2 proyecto, $3 configuracion, $4 opcional: -o
  ( cd "$C" && "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  echo "$1 rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log") avisos-CS=$(grep -c 'warning CS' "$OUT/$1.log")"
  grep -E ': error [A-Z]+[0-9]+|warning CS' "$OUT/$1.log" | sed 's/^/    /' | sort -u | head -20
}
core() { # $1 etiqueta, $2 opcional: cov
  local t0=$(date +%s)
  if test "${2:-}" = cov; then
    ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" \
        --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v52b.runsettings")" ) > "$OUT/$1.log" 2>&1
  else
    ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" ) > "$OUT/$1.log" 2>&1
  fi
  local rc=$? t1=$(date +%s); lf "$OUT/$1.log"
  cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
  python -I "$S/p6d_check_v52.py" "$OUT/$1.trx" "$OUT/$1-res.txt" > /dev/null
  echo "core $1 rc=$rc segundos=$((t1 - t0)) trx=$(sha256sum "$OUT/$1.trx" | cut -c1-16) :: $(head -1 "$OUT/$1-res.txt")"
  sed -n '2,30p' "$OUT/$1-res.txt" | cut -c1-260
  if test "${2:-}" = cov; then
    local j x; j=$(find "$C/_trx/$1" -name 'coverage.json' | head -1); x=$(find "$C/_trx/$1" -name 'coverage.cobertura.xml' | head -1)
    test -n "$j" && cp "$j" "$OUT/$1.coverage.json"; test -n "$x" && cp "$x" "$OUT/$1.cobertura.xml"
    echo "  cobertura json=$(test -n "$j" && sha256sum "$OUT/$1.coverage.json" | cut -c1-16) cobertura=$(test -n "$x" && sha256sum "$OUT/$1.cobertura.xml" | cut -c1-16)"
  fi
}
ui() { # $1 rama
  local n=0 f
  while IFS= read -r f; do
    test -z "$f" && continue
    n=$((n + 1)); local tag="$1-ui$(printf '%02d' $n)"
    ( cd "$C" && "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-build --filter "$f" --logger "trx;LogFileName=$tag.trx" \
        --results-directory "$C/_trx/$tag" ) > "$OUT/$tag.log" 2>&1
    local rc=$?; lf "$OUT/$tag.log"
    if test -f "$C/_trx/$tag/$tag.trx"; then
      cp "$C/_trx/$tag/$tag.trx" "$OUT/$tag.trx"
      python -I "$S/p6d_check_v52.py" "$OUT/$tag.trx" "$OUT/$tag-res.txt" > /dev/null
      echo "  ui $tag rc=$rc $f :: $(head -1 "$OUT/$tag-res.txt")"; sed -n '2,10p' "$OUT/$tag-res.txt" | grep -v '^no pasan: 0' | cut -c1-260
    else
      echo "  ui $tag rc=$rc $f :: sin TRX (ningun resultado)"
    fi
  done < "$UIF"
}
python -I "$PB4/apply_red_pbt4d_v52.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; tail -5 "$OUT/apply-red.txt"; exit 5; }
tail -1 "$OUT/apply-red.txt"
build r-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
build r-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
core core-red
ui red
python -I "$PB4/apply_green_pbt4d_v52.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; tail -5 "$OUT/apply-green.txt"; exit 6; }
tail -1 "$OUT/apply-green.txt"
build g-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
build g-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
build g-plugin src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out"
core core-green
ui green
core core-green-cov cov
python -I "$CLS" "$OUT/clases-red.txt" "$OUT/core-red.trx" "$OUT"/red-ui*.trx > /dev/null
python -I "$CLS" "$OUT/clases-green.txt" "$OUT/core-green.trx" "$OUT"/green-ui*.trx > /dev/null
echo "clases red=$(sha256sum "$OUT/clases-red.txt" | cut -c1-16) green=$(sha256sum "$OUT/clases-green.txt" | cut -c1-16)"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

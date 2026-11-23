#!/usr/bin/env bash
# PB-T4e (paquete v5.6), la precondicion 6, la copia PRINCIPAL, en una copia EXPORTADA nueva de 160e019d, fuera de todo worktree (nada en el
# checkout principal salvo git archive y lecturas). Corre SOLA: comprueba al empezar y al acabar que no hay otro dotnet ni testhost.
#   1. git archive de 160e019d;
#   2. el RED literal: apply_red_pbt4e_v56.py con pbt4e-red-v56.txt (solo los dos archivos nuevos de tests/; src/ intacto: sin esqueleto);
#   3. la compilacion del RED (Core y UI, Debug), sin ningun warning CS;
#   4. el RED exacto: la suite COMPLETA de Core (solo caen las tres CT de Core, por asercion) y los 19 filtros de UI del contrato
#      (ui_filters_pbt4e_v55.txt, la misma lista que la v5.5: los 18 heredados de PB-T4d y el de la clase nueva), CADA UNO EN SU PROCESO; solo cae CT-PBT4E-03;
#   5. sobre la misma copia, el GREEN literal: apply_green_pbt4e_v56.py; la compilacion de Core y UI (Debug) y del Plugin (Release, -o), sin
#      warning CS;
#   6. el GREEN: la suite COMPLETA de Core (0 Failed) y los 19 filtros de UI (0 Failed; cada uno en su proceso, con cobertura), con la duracion;
#   7. la cobertura del GREEN: la suite COMPLETA de Core y la union de los 19 filtros (ui_union_v55.txt) tres veces, con coverlet en JSON y
#      Cobertura (cov_v55.runsettings: Domain, Application y UI). La base, en otra copia (p6_basecov_v55.sh).
# Las tablas por clase (classes_v55.py, copia de classes_v51.py de PB-T4c). El SDK de usuario, -nodeReuse:false -p:UseSharedCompilation=false;
# build-server shutdown al final. UTF-8 con LF; nombres cortos (<copia>-p6/).
# Uso: bash p6_pbt4e_v56.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=160e019d
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
UIF="$S/ui_filters_pbt4e_v55.txt"
UNION="$(cat "$S/ui_union_v55.txt")"
CLS="$S/classes_v55.py"
CHK="$S/check_trx_v55.py"
test "$(grep -c . "$UIF")" -eq 19 || { echo "la lista de filtros de UI ($UIF) no tiene 19 lineas"; exit 2; }
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-p6"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
procs() { tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost'; }
echo "precondicion 6 de PB-T4e (v5.6), copia principal; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
echo "procesos al empezar: $(procs) ($(date '+%H:%M:%S'))"
test "$(procs)" -eq 0 || { echo "hay otro dotnet o testhost: la copia principal tiene que correr sola"; exit 4; }
for f in "$PB4/pbt4e-red-v56.txt" "$PB4/red_pbt4e_v56.py" "$PB4/apply_red_pbt4e_v56.py" "$PB4/pbt4e-green-v56.txt" "$PB4/green_pbt4e_v56.py" \
         "$PB4/apply_green_pbt4e_v56.py" "$0" "$CHK" "$S/cov_v55.runsettings" "$UIF" "$S/ui_union_v55.txt" "$S/ui_union18_v56.txt" "$CLS"; do
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
cov() { # $1 etiqueta: copia el JSON y el Cobertura de la corrida
  local j x; j=$(find "$C/_trx/$1" -name 'coverage.json' | head -1); x=$(find "$C/_trx/$1" -name 'coverage.cobertura.xml' | head -1)
  test -n "$j" && cp "$j" "$OUT/$1.coverage.json"; test -n "$x" && cp "$x" "$OUT/$1.cobertura.xml"
  echo "  cobertura json=$(test -n "$j" && sha256sum "$OUT/$1.coverage.json" | cut -c1-16) cobertura=$(test -n "$x" && sha256sum "$OUT/$1.cobertura.xml" | cut -c1-16)"
}
core() { # $1 etiqueta, $2 opcional: cov, $3 opcional: un filtro (solo con cov)
  local t0=$(date +%s)
  if test "${2:-}" = cov; then
    ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build ${3:+--filter "$3"} --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" \
        --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v55.runsettings")" ) > "$OUT/$1.log" 2>&1
  else
    ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" ) > "$OUT/$1.log" 2>&1
  fi
  local rc=$? t1=$(date +%s); lf "$OUT/$1.log"
  cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
  python -I "$CHK" "$OUT/$1.trx" "$OUT/$1-res.txt" > /dev/null
  echo "core $1 rc=$rc segundos=$((t1 - t0)) trx=$(sha256sum "$OUT/$1.trx" | cut -c1-16) :: $(head -1 "$OUT/$1-res.txt")"
  sed -n '2,30p' "$OUT/$1-res.txt" | cut -c1-260
  test "${2:-}" = cov && cov "$1"
}
uicov() { # $1 etiqueta, $2 opcional: el filtro (por defecto, la union de los 19); con cobertura
  local t0=$(date +%s)
  ( cd "$C" && "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-build --filter "${2:-$UNION}" --logger "trx;LogFileName=$1.trx" \
      --results-directory "$C/_trx/$1" --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v55.runsettings")" ) > "$OUT/$1.log" 2>&1
  local rc=$? t1=$(date +%s); lf "$OUT/$1.log"
  cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
  python -I "$CHK" "$OUT/$1.trx" "$OUT/$1-res.txt" > /dev/null
  echo "ui $1 rc=$rc segundos=$((t1 - t0)) :: $(head -1 "$OUT/$1-res.txt")"; sed -n '2,10p' "$OUT/$1-res.txt" | grep -v '^no pasan: 0' | cut -c1-260
  cov "$1"
}
ui() { # $1 rama, $2 opcional: cov (cada filtro, en su proceso)
  local n=0 f
  while IFS= read -r f; do
    test -z "$f" && continue
    n=$((n + 1)); local tag="$1-ui$(printf '%02d' $n)"
    local t0=$(date +%s)
    if test "${2:-}" = cov; then
      ( cd "$C" && "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-build --filter "$f" --logger "trx;LogFileName=$tag.trx" \
          --results-directory "$C/_trx/$tag" --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v55.runsettings")" ) > "$OUT/$tag.log" 2>&1
    else
      ( cd "$C" && "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-build --filter "$f" --logger "trx;LogFileName=$tag.trx" \
          --results-directory "$C/_trx/$tag" ) > "$OUT/$tag.log" 2>&1
    fi
    local rc=$? t1=$(date +%s); lf "$OUT/$tag.log"
    if test "${2:-}" = cov; then local j; j=$(find "$C/_trx/$tag" -name 'coverage.json' | head -1); test -n "$j" && cp "$j" "$OUT/$tag.coverage.json"; fi
    if test -f "$C/_trx/$tag/$tag.trx"; then
      cp "$C/_trx/$tag/$tag.trx" "$OUT/$tag.trx"
      python -I "$CHK" "$OUT/$tag.trx" "$OUT/$tag-res.txt" > /dev/null
      echo "  ui $tag rc=$rc segundos=$((t1 - t0)) $f :: $(head -1 "$OUT/$tag-res.txt")"; sed -n '2,10p' "$OUT/$tag-res.txt" | grep -v '^no pasan: 0' | cut -c1-260
    else
      echo "  ui $tag rc=$rc $f :: sin TRX (ningun resultado)"
    fi
  done < "$UIF"
}
python -I "$PB4/apply_red_pbt4e_v56.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; tail -5 "$OUT/apply-red.txt"; exit 5; }
cat "$OUT/apply-red.txt"
build r-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
build r-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
core core-red
ui red
python -I "$PB4/apply_green_pbt4e_v56.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; tail -5 "$OUT/apply-green.txt"; exit 6; }
cat "$OUT/apply-green.txt"
build g-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
build g-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
build g-plugin src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out"
core core-green
ui green cov
core core-green-cov cov
uicov ui-green-cov
uicov ui-green-cov-r-1
uicov ui-green-cov-r-2
# 7b (CA-T4e M-02, la corrida opcional): la cobertura del GREEN SIN las dos clases nuevas, para la parte de las pruebas existentes: Core
# entera con FullyQualifiedName!~PushBackEditAllReload y la union de los 18 filtros heredados (ui_union18_v56.txt) tres veces
core core-green-cov-sin cov 'FullyQualifiedName!~PushBackEditAllReload'
U18="$(cat "$S/ui_union18_v56.txt")"
uicov ui-green-cov18 "$U18"
uicov ui-green-cov18-r-1 "$U18"
uicov ui-green-cov18-r-2 "$U18"
python -I "$CLS" "$OUT/clases-red.txt" "$OUT/core-red.trx" "$OUT"/red-ui*.trx > /dev/null
python -I "$CLS" "$OUT/clases-green.txt" "$OUT/core-green.trx" "$OUT"/green-ui*.trx > /dev/null
echo "clases red=$(sha256sum "$OUT/clases-red.txt" | cut -c1-16) green=$(sha256sum "$OUT/clases-green.txt" | cut -c1-16)"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos al acabar: $(procs) ($(date '+%H:%M:%S'))"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

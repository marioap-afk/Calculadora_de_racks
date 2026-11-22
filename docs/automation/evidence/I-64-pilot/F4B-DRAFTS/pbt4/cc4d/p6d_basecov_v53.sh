#!/usr/bin/env bash
# PB-T4d (paquete v5.3), la precondicion 6, la referencia de la cobertura ampliada (CA-T4d I-02): la base SIN CAMBIOS (6c2820ba), en una copia
# EXPORTADA nueva, fuera de todo worktree, y sola (comprueba que no hay otro dotnet ni testhost):
#   1. git archive de 6c2820ba; la compilacion de Core y UI (Debug), sin warning CS;
#   2. la suite COMPLETA de Core con coverlet en JSON y Cobertura (cov_v53.runsettings: Domain, Application y UI); su TRX es la base de Core;
#   3. la union de los filtros de UI de PB-T4c y de PB-T4d (ui_union_v53.txt), con coverlet; su TRX es la base de UI de cada filtro.
# El SDK de usuario, -nodeReuse:false -p:UseSharedCompilation=false; build-server shutdown al final. UTF-8 con LF; nombres cortos (<copia>-bc/).
# Uso: bash p6d_basecov_v53.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
UIF="$S/ui_filters_pbt4d_v53.txt"
UNION="$(cat "$S/ui_union_v53.txt")"
CLS="$S/../../pbt4ccc/compile/classes_v51.py"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-bc"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
procs() { tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost'; }
echo "precondicion 6 de PB-T4d (v5.3), la base con cobertura; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
echo "procesos al empezar: $(procs) ($(date '+%H:%M:%S'))"
test "$(procs)" -eq 0 || { echo "hay otro dotnet o testhost: la base de la cobertura tiene que correr sola"; exit 4; }
for f in "$0" "$S/p6d_check_v52.py" "$S/cov_v53.runsettings" "$S/ui_union_v53.txt"; do
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
core() { # $1 etiqueta, $2 opcional: cov
  local t0=$(date +%s)
  if test "${2:-}" = cov; then
    ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" \
        --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v53.runsettings")" ) > "$OUT/$1.log" 2>&1
  else
    ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" ) > "$OUT/$1.log" 2>&1
  fi
  local rc=$? t1=$(date +%s); lf "$OUT/$1.log"
  cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
  python -I "$S/p6d_check_v52.py" "$OUT/$1.trx" "$OUT/$1-res.txt" > /dev/null
  echo "core $1 rc=$rc segundos=$((t1 - t0)) trx=$(sha256sum "$OUT/$1.trx" | cut -c1-16) :: $(head -1 "$OUT/$1-res.txt")"
  sed -n '2,30p' "$OUT/$1-res.txt" | cut -c1-260
  test "${2:-}" = cov && cov "$1"
}
uicov() { # $1 etiqueta: la union de los filtros, con cobertura
  local t0=$(date +%s)
  ( cd "$C" && "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-build --filter "$UNION" --logger "trx;LogFileName=$1.trx" \
      --results-directory "$C/_trx/$1" --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v53.runsettings")" ) > "$OUT/$1.log" 2>&1
  local rc=$? t1=$(date +%s); lf "$OUT/$1.log"
  cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
  python -I "$S/p6d_check_v52.py" "$OUT/$1.trx" "$OUT/$1-res.txt" > /dev/null
  echo "ui $1 rc=$rc segundos=$((t1 - t0)) :: $(head -1 "$OUT/$1-res.txt")"; sed -n '2,10p' "$OUT/$1-res.txt" | grep -v '^no pasan: 0' | cut -c1-260
  cov "$1"
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
build b-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
build b-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
core base-core-cov cov
uicov base-ui-cov
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos al acabar: $(procs) ($(date '+%H:%M:%S'))"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

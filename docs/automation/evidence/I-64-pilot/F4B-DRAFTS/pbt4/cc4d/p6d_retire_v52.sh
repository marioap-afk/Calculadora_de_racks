#!/usr/bin/env bash
# PB-T4d (paquete v5.2, fase 2): la medida de la retirada del piloto en una copia EXPORTADA nueva del cierre de PB-T4c (6c2820ba), fuera de todo
# worktree; nada en el checkout principal salvo git archive. Una variante de la retirada (A: D-PBT4-10 y lo que hace falta para compilar; B: la A y lo
# que queda sin uso). Pasos: (1) el bucle de compilacion de measure_retire_v52.py (las bajas de la lista, la Theory de SUS-01 y lo que la compilacion
# anade); (2) la compilacion de Core y de UI en Debug y del Plugin en Release con -o, contando los "warning CS"; (3) la suite COMPLETA de Core y los 18
# filtros de UI del contrato de PB-T4c (nunca la suite completa de UI); (4) la sonda del equivalente de CT-PBT4B-20 (sus lineas, esperadas y reales);
# (5) las tablas de resultados por clase. El SDK de usuario, con --no-incremental -nodeReuse:false -p:UseSharedCompilation=false; build-server
# shutdown al final. Las salidas, en UTF-8 con LF y con nombres cortos (<copia>-<variante>/).
# Uso: bash p6d_retire_v52.sh <A|B> <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
CC4="$(cd "$S/../../pbt4ccc/compile" && pwd)"
VAR="$1"; C="$2"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
UIF="$CC4/ui_filters_pbt4c_v50b.txt"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-$VAR"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "retirada, variante $VAR; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PB4/v52/retire_pbt4d_v52.py" "$PB4/v52/measure_retire_v52.py" "$PB4/lists_core.py" "$PB4/pbt4d-retire-list-138.tsv" "$PB4/v52/pbt4d-probe-fullcopy.cs" \
         "$S/p6d_check_v52.py" "$CC4/run_classes_v51.py" "$UIF" "$0"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
echo "copia exportada: $(find "$C" -type f | wc -l) archivos"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
python -I "$PB4/v52/measure_retire_v52.py" "$C" "$VAR" "$OUT/bucle" --repo "$MAINCO" > "$OUT/bucle.txt" 2>&1; rc=$?
lf "$OUT/bucle.txt"; cat "$OUT/bucle.txt"
if test $rc != 0; then echo "PARA: el bucle de compilacion no cerro (rc=$rc)"; "$DN" build-server shutdown > /dev/null 2>&1; echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"; exit 4; fi
WARN_TOTAL=0; ERR_TOTAL=0
build() { # $1 etiqueta, $2 proyecto, $3 configuracion, $4 opcional: carpeta de salida (-o)
  ( cd "$C" && "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} ) > "$OUT/$1.log" 2>&1
  local rc=$? e w
  lf "$OUT/$1.log"
  e=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log"); w=$(grep -c 'warning CS' "$OUT/$1.log")
  WARN_TOTAL=$((WARN_TOTAL + w)); ERR_TOTAL=$((ERR_TOTAL + e))
  echo "$1 rc=$rc errores=$e avisos-CS=$w"
  test "$w" = 0 || grep -o 'warning CS[0-9]*' "$OUT/$1.log" | sort | uniq -c
  test "$w" = 0 || grep 'warning CS' "$OUT/$1.log" | sed 's/^/    /' | sort -u | head -20
}
build b-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
build b-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
build b-plugin src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out"
echo "compilacion: errores $ERR_TOTAL, avisos CS $WARN_TOTAL"
test "$ERR_TOTAL" = 0 || { echo "PARA: la compilacion final tiene errores"; "$DN" build-server shutdown > /dev/null 2>&1; exit 4; }
test_run() { # $1 proyecto, $2 nombre del trx, $3 opcional: filtro
  local t0=$(date +%s)
  ( cd "$C" && "$DN" test "$1" -c Debug --no-build ${3:+--filter "$3"} --logger "trx;LogFileName=$2" --results-directory "$C/_trx" ) > "$OUT/${2%.trx}.log" 2>&1
  local rc=$? t1=$(date +%s)
  lf "$OUT/${2%.trx}.log"
  python -I "$S/p6d_check_v52.py" "$C/_trx/$2" "$OUT/${2%.trx}-res.txt" > /dev/null
  echo "test $2 rc=$rc segundos=$((t1 - t0)) trx-sha256=$(sha256sum "$C/_trx/$2" | cut -d' ' -f1) :: $(head -1 "$OUT/${2%.trx}-res.txt") | $(tail -1 "$OUT/${2%.trx}-res.txt")"
}
test_run tests/RackCad.Tests/RackCad.Tests.csproj core.trx
n=0; UI=()
while read -r flt; do
  flt="${flt%$'\r'}"; test -n "$flt" || continue
  n=$((n + 1)); nn=$(printf '%02d' $n); echo "$nn $flt" >> "$OUT/ui-filtros.txt"
  test_run tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "ui-$nn.trx" "$flt"
  UI+=("RUN=$C/_trx/ui-$nn.trx")
done < "$UIF"
# la sonda del equivalente de CT-PBT4B-20, sola, sobre la misma copia
cp "$PB4/v52/pbt4d-probe-fullcopy.cs" "$C/tests/RackCad.Tests/TmpFullcopy.cs"
build b-sonda tests/RackCad.Tests/RackCad.Tests.csproj Debug
test_run tests/RackCad.Tests/RackCad.Tests.csproj sonda.trx "FullyQualifiedName~RackCad.Tests.TmpPbt4dFullCopyProbe"
python -I "$CC4/trx_message_v50.py" "$C/_trx/sonda.trx" "$OUT/sonda-lineas.txt"
python -I "$CC4/run_classes_v51.py" "$OUT/clases.txt" "RUN=$C/_trx/core.trx" "${UI[@]}"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

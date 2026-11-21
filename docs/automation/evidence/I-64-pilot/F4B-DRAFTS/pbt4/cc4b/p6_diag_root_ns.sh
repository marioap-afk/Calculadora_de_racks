#!/usr/bin/env bash
# DIAGNOSTICO para el analisis de lo que cae fuera de las 477 (no es la precondicion 6 ni cambia el paquete): una copia exportada NUEVA del cierre de
# PB-T4a con el RED de la v4.7 con su particion y, ademas, las 20 copias con el espacio de nombres raiz de su ensamblado (diag_root_ns.py rewrite).
# Compila Core-tests y UI-tests en Debug (errores y avisos CS); si compila, corre la suite completa de Core y los 15 filtros de UI de §6.2.1 (el de
# las copias de UI pasa a FullRedTests|FullGreenTests, porque ya no estan en ...FullPanelCopies), sobre el RED y sobre el GREEN literal.
# Uso: bash p6_diag_root_ns.sh <copia nueva> <particion>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"; PART="$2"
MAINCO='<checkout-principal>'
REV=78a7cef3
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
PKG="$PB4/I-64-f4b-pbt4-gate-contract-package-v47.md"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/out-diag-rootns-$(basename "$C")"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "diagnostico; copia $C; revision $(git -C "$MAINCO" rev-parse $REV); paquete $(sha256sum "$PKG" | cut -d' ' -f1); particion $(sha256sum "$PART" | cut -d' ' -f1)"
for f in "$0" "$S/diag_root_ns.py" "$S/p6_check_v47.py" "$PB4/apply_red_pbt4b_v47.py" "$PB4/pbt4b_hand_edits_v47.py" "$PB4/make_copies_pbt4b_v47.py"; do echo "  guion $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"; done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
python -I "$PB4/apply_red_pbt4b_v47.py" "$C" --repo "$MAINCO" --rev $REV --partition "$PART" > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; exit 6; }
tail -1 "$OUT/apply-red.txt"
python -I "$S/diag_root_ns.py" rewrite "$C" "$OUT/rewrite.txt" | tail -1 || exit 7
ERR=0; WARN=0
build() { # $1 etiqueta, $2 proyecto
  ( cd "$C" && "$DN" build "$2" -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/$1.log" 2>&1
  local rc=$? e w
  e=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log"); w=$(grep -c 'warning CS' "$OUT/$1.log"); ERR=$((ERR + e)); WARN=$((WARN + w))
  echo "$1 rc=$rc errores=$e avisos-CS=$w"
  test "$w" = 0 || grep -o 'warning CS[0-9]*' "$OUT/$1.log" | sort | uniq -c
  test "$e" = 0 || grep -o -E '[A-Za-z0-9_.]+\.cs\([0-9]+,[0-9]+\): error [A-Z]+[0-9]+: [^[]*' "$OUT/$1.log" | sort -u | head -40
}
test_run() { # $1 proyecto, $2 trx, $3 opcional: filtro
  local t0=$(date +%s)
  ( cd "$C" && "$DN" test "$1" -c Debug --no-build ${3:+--filter "$3"} --logger "trx;LogFileName=$2" --results-directory "$C/_trx" ) > "$OUT/${2%.trx}.log" 2>&1
  local rc=$?
  python -I "$S/p6_check_v47.py" summary "$C/_trx/$2" "$OUT/${2%.trx}-outcomes.txt" > /dev/null
  echo "test $2 rc=$rc segundos=$(( $(date +%s) - t0 )) trx-sha256=$(sha256sum "$C/_trx/$2" | cut -d' ' -f1) :: $(head -1 "$OUT/${2%.trx}-outcomes.txt")"
}
ui_runs() { # $1 prefijo
  python -I "$S/ui_filters_p6.py" "$PKG" "$OUT/ui-filters.txt" > /dev/null
  local i=0
  while read -r flt; do
    flt="${flt%$'\r'}"; test -n "$flt" || continue
    test "$flt" = "FullyQualifiedName~RackCad.UI.Tests.FullPanelCopies" && flt="FullyQualifiedName~FullRedTests|FullyQualifiedName~FullGreenTests"
    i=$((i + 1)); test_run tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "$1-ui-$i.trx" "$flt"
    echo "    filtro $i: $flt"
  done < "$OUT/ui-filters.txt"
}
phase() { # $1 red|green
  build "$1-core-tests" tests/RackCad.Tests/RackCad.Tests.csproj
  build "$1-ui-tests" tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
  if test "$ERR" != 0; then echo "$1: NO COMPILA (errores $ERR, avisos CS $WARN)"; return 1; fi
  test_run tests/RackCad.Tests/RackCad.Tests.csproj "core-$1.trx"
  python -I "$S/diag_root_ns.py" check "$PART" "$OUT/check-core-$1.txt" "$C/_trx/core-$1.trx" > /dev/null; sed -n 1,3p "$OUT/check-core-$1.txt"; grep -A20 '^Failed fuera' "$OUT/check-core-$1.txt" | cut -c1-260
  ui_runs "$1"
}
phase red; RED_RC=$?
if test "$RED_RC" = 0; then
  python -I "$PB4/apply_green_pbt4b_v45.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; exit 8; }
  tail -1 "$OUT/apply-green.txt"
  ERR=0; WARN=0
  phase green
fi
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

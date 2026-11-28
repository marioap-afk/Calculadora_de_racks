#!/usr/bin/env bash
# PB-T4d (paquete v5.2), precondicion 6: los filtros de UI de los RequiredTests heredados de PB-T4c, que son 19 (los 18 de ui_filters_pbt4c_v50b.txt
# y PushBackEditAllWindowTests, n.o 82 de PB-T4c, que no estaba en ellos), en una copia EXPORTADA nueva de 6c2820ba: los 19 en la base sin cambios
# (la base de los MinSelected de UI); y PushBackEditAllWindowTests con el RED literal y con el GREEN literal (los otros 18 ya los mide la corrida
# principal). Compila UI en Debug cada vez, sin warning CS. Nada en el checkout principal salvo git archive. El SDK de usuario; build-server
# shutdown al final. UTF-8 con LF; nombres cortos (<copia>-u9/).
# Uso: bash p6d_ui19_v52.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
UIF="$S/../../pbt4ccc/compile/ui_filters_pbt4c_v50b.txt"
EXTRA='FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllWindowTests'
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-u9"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "filtros de UI de los RequiredTests; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PB4/pbt4d-red-v52.txt" "$PB4/pbt4d-green-v52.txt" "$0" "$UIF" "$S/p6d_check_v52.py"; do echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"; done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
build() {
  ( cd "$C" && "$DN" build tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/b-$1.log" 2>&1
  local rc=$?; lf "$OUT/b-$1.log"
  echo "b-$1 rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/b-$1.log") avisos-CS=$(grep -c 'warning CS' "$OUT/b-$1.log")"
}
one() { # $1 etiqueta, $2 filtro
  ( cd "$C" && "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-build --filter "$2" --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  if test -f "$C/_trx/$1/$1.trx"; then
    cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"; python -I "$S/p6d_check_v52.py" "$OUT/$1.trx" "$OUT/$1-res.txt" > /dev/null
    echo "  ui $1 rc=$rc $2 :: $(head -1 "$OUT/$1-res.txt")"
  else
    echo "  ui $1 rc=$rc $2 :: sin TRX"
  fi
}
build base
n=0
while IFS= read -r f; do test -z "$f" && continue; n=$((n + 1)); one "base-ui$(printf '%02d' $n)" "$f"; done < "$UIF"
one base-ui19 "$EXTRA"
python -I "$PB4/apply_red_pbt4d_v52.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; exit 5; }
tail -1 "$OUT/apply-red.txt"
build red
one red-ui19 "$EXTRA"
python -I "$PB4/apply_green_pbt4d_v52.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; exit 6; }
tail -1 "$OUT/apply-green.txt"
build green
one green-ui19 "$EXTRA"
"$DN" build-server shutdown > /dev/null 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

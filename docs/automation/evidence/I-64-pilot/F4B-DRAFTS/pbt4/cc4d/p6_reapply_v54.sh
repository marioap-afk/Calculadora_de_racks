#!/usr/bin/env bash
# PB-T4d (paquete v5.4, solo de texto): las piezas de la v5.4 en una copia EXPORTADA nueva de 6c2820ba (desde /d/r70): el RED (pbt4d-red-v54.txt)
# y el GREEN (pbt4d-green-v54.txt) con los aplicadores de la v5.3 (--pieza); la compilacion de Core y UI (Debug) tras cada uno y del Plugin
# (Release) tras el GREEN, sin warning CS; y la comparacion de tests/ y src/ con la copia principal de la v5.3 (/d/r6y): el codigo sin
# comentarios igual (codeonly_v54.py), src/ igual. No corre pruebas. El SDK de usuario; build-server shutdown al final. UTF-8 con LF.
# Uso: bash p6_reapply_v54.sh <copia> <copia-v53>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"; C53="$2"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-ra"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "las piezas de la v5.4; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PB4/pbt4d-red-v54.txt" "$PB4/pbt4d-green-v54.txt" "$PB4/red_pbt4d_v53.py" "$PB4/apply_red_pbt4d_v53.py" "$PB4/green_pbt4d_v53.py" "$PB4/apply_green_pbt4d_v53.py" "$0"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
build() {
  ( cd "$C" && "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  echo "$1 rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log") avisos-CS=$(grep -c 'warning CS' "$OUT/$1.log")"
}
python -I "$PB4/apply_red_pbt4d_v53.py" "$C" --repo "$MAINCO" --rev $REV --pieza "$PB4/pbt4d-red-v54.txt" > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; exit 5; }
tail -1 "$OUT/apply-red.txt"
build r-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
build r-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
python -I "$PB4/apply_green_pbt4d_v53.py" "$C" --repo "$MAINCO" --rev $REV --pieza "$PB4/pbt4d-green-v54.txt" > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; exit 6; }
tail -1 "$OUT/apply-green.txt"
build g-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
build g-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
build g-plugin src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out"
echo "tests/ frente a la v5.3 ($C53):"; python -I "$PB4/v54/codeonly_v54.py" "$C53" "$C" | sed 's/^/  /'
echo "src/ frente a la v5.3: $(diff -rq --exclude=bin --exclude=obj "$C53/src" "$C/src" | wc -l) diferencias"
"$DN" build-server shutdown > /dev/null 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

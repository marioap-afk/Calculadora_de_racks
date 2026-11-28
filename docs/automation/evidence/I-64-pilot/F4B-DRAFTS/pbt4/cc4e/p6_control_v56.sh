#!/usr/bin/env bash
# PB-T4e (paquete v5.6), precondicion 6, el CONTROL de las CT, en una copia EXPORTADA nueva de 160e019d (nada en el checkout principal salvo git
# archive y lecturas):
#   1. git archive; el RED literal; la sonda del detalle de Actualizar (SondaPushBackEditAllReloadUpdate.cs, temporal, nunca en un commit);
#   2. la compilacion (Core y UI, Debug) y, sobre src/ de la base, la sonda (lo de hoy);
#   3. UNA pieza parcial del GREEN (control_pieces_v55.py): `campo` (solo src/RackCad.UI) o `anfitrion` (solo src/RackCad.Application);
#   4. la compilacion y las dos clases de las CT y la sonda: con `campo` caen las tres CT de Core y pasa CT-PBT4E-03; con `anfitrion`, al reves.
# Uso: bash p6_control_v56.sh <copia> campo|anfitrion
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"; MODE="$2"
MAINCO='<checkout-principal>'
REV=160e019d
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
case "$MODE" in campo|anfitrion) ;; *) echo "modo $MODE"; exit 2;; esac
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-ct"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "control de las CT de PB-T4e (v5.6), modo $MODE; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
python -I "$S/control_pieces_v55.py" "$PB4/pbt4e-green-v56.txt" "$OUT" || exit 5
for f in "$PB4/pbt4e-red-v56.txt" "$PB4/apply_red_pbt4e_v56.py" "$PB4/pbt4e-green-v56.txt" "$PB4/apply_green_pbt4e_v56.py" "$0" "$S/control_pieces_v55.py" \
         "$OUT/control-$MODE.txt" "$PB4/v56/probe/SondaPushBackEditAllReloadUpdate.cs" "$S/trx_lines_v55.py"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
build() {
  ( cd "$C" && "$DN" build "$2" -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  echo "$1 rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log") avisos-CS=$(grep -c 'warning CS' "$OUT/$1.log")"
}
run() {
  ( cd "$C" && "$DN" test "$2" -c Debug --no-build --filter "$3" --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  if test -f "$C/_trx/$1/$1.trx"; then
    cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
    python -I "$S/trx_lines_v55.py" "$OUT/$1.trx" "$OUT/$1-lineas.txt" > /dev/null
    echo "$1 rc=$rc :: $(grep '^== ' "$OUT/$1-lineas.txt" | sed 's/\.CT_PBT4E_\(0[0-9]\)_.*/ CT-\1/' | tr '\n' ';')"
  else
    echo "$1 rc=$rc sin TRX"
  fi
}
CF='FullyQualifiedName~RackCad.Tests.PushBackEditAllReloadTests'
UF='FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllReloadFieldTests'
PF='FullyQualifiedName~RackCad.Tests.SondaPushBackEditAllReloadUpdateTests'
python -I "$PB4/apply_red_pbt4e_v56.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; exit 5; }
cp "$PB4/v56/probe/SondaPushBackEditAllReloadUpdate.cs" "$C/tests/RackCad.Tests/SondaPushBackEditAllReloadUpdate.cs"
build r-core tests/RackCad.Tests/RackCad.Tests.csproj
run sonda-base tests/RackCad.Tests/RackCad.Tests.csproj "$PF"
python -I "$PB4/apply_green_pbt4e_v56.py" "$C" --repo "$MAINCO" --rev $REV --pieza "$OUT/control-$MODE.txt" > "$OUT/apply-control.txt" 2>&1 || { echo "apply control fallo"; tail -3 "$OUT/apply-control.txt"; exit 6; }
tail -1 "$OUT/apply-control.txt"
build c-core tests/RackCad.Tests/RackCad.Tests.csproj
build c-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
run control-core tests/RackCad.Tests/RackCad.Tests.csproj "$CF"
run control-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "$UF"
run sonda-control tests/RackCad.Tests/RackCad.Tests.csproj "$PF"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

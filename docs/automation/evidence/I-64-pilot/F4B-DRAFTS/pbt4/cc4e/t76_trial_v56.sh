#!/usr/bin/env bash
# PB-T4e (paquete v5.6), precondicion 6, el ENSAYO y la SONDA de las lineas de las CT, en una copia EXPORTADA nueva de 160e019d, fuera de todo
# worktree (nada en el checkout principal salvo git archive y lecturas):
#   1. git archive de 160e019d;
#   2. el RED literal (apply_red_pbt4e_v56.py) y las sondas (write_probe_v56.py: copias temporales de las dos clases de las CT, con el prefijo
#      Sonda, que acaban en Assert.Fail con sus lineas; nunca entran en un commit);
#   3. la compilacion de Core y UI (Debug), con los avisos CS contados;
#   4. en el RED: las dos clases de las CT y sus sondas, cada proyecto en su proceso;
#   5. sobre la misma copia, el GREEN literal (apply_green_pbt4e_v56.py), la compilacion y lo mismo.
# Uso: bash t76_trial_v56.sh <copia> <pieza-green>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"; GP="${2:?falta la pieza del GREEN}"
MAINCO='<checkout-principal>'
REV=160e019d
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-tr"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "ensayo de PB-T4e (v5.6); copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV); GREEN $(sha256sum "$GP" | cut -c1-64) ${GP##*/}"
for f in "$PB4/pbt4e-red-v56.txt" "$PB4/red_pbt4e_v56.py" "$PB4/apply_red_pbt4e_v56.py" "$PB4/pbt4e-green-v56.txt" "$PB4/green_pbt4e_v56.py" \
         "$PB4/apply_green_pbt4e_v56.py" "$PB4/v56/probe/write_probe_v56.py" "$0" "$S/trx_lines_v55.py"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
echo "copia exportada: $(find "$C" -type f | wc -l) archivos"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
build() { # $1 etiqueta, $2 proyecto
  ( cd "$C" && "$DN" build "$2" -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  echo "$1 rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log") avisos-CS=$(grep -c 'warning CS' "$OUT/$1.log")"
  grep -E ': error [A-Z]+[0-9]+|warning CS' "$OUT/$1.log" | sed 's/^/    /' | sort -u | head -20
}
run() { # $1 etiqueta, $2 proyecto, $3 filtro
  ( cd "$C" && "$DN" test "$2" -c Debug --no-build --filter "$3" --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  if test -f "$C/_trx/$1/$1.trx"; then
    cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
    python -I "$S/trx_lines_v55.py" "$OUT/$1.trx" "$OUT/$1-lineas.txt" > /dev/null
    echo "$1 rc=$rc trx=$(sha256sum "$OUT/$1.trx" | cut -c1-16) :: $(grep '^== ' "$OUT/$1-lineas.txt" | tr '\n' ';' | cut -c1-600)"
  else
    echo "$1 rc=$rc sin TRX"
  fi
}
CF='FullyQualifiedName~RackCad.Tests.PushBackEditAllReloadTests|FullyQualifiedName~RackCad.Tests.SondaPushBackEditAllReloadTests'
UF='FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllReloadFieldTests|FullyQualifiedName~RackCad.UI.Tests.SondaPushBackEditAllReloadFieldTests'
python -I "$PB4/apply_red_pbt4e_v56.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; tail -5 "$OUT/apply-red.txt"; exit 5; }
tail -1 "$OUT/apply-red.txt"
python -I "$PB4/v56/probe/write_probe_v56.py" "$C" > "$OUT/probe.txt" 2>&1 || { echo "sonda fallo"; tail -5 "$OUT/probe.txt"; exit 5; }
build r-core tests/RackCad.Tests/RackCad.Tests.csproj
build r-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
run red-core tests/RackCad.Tests/RackCad.Tests.csproj "$CF"
run red-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "$UF"
python -I "$PB4/apply_green_pbt4e_v56.py" "$C" --repo "$MAINCO" --rev $REV --pieza "$GP" > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; tail -5 "$OUT/apply-green.txt"; exit 6; }
tail -1 "$OUT/apply-green.txt"
build g-core tests/RackCad.Tests/RackCad.Tests.csproj
build g-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
run green-core tests/RackCad.Tests/RackCad.Tests.csproj "$CF"
run green-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "$UF"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

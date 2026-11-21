#!/usr/bin/env bash
# DIAGNOSTICO para el analisis de lo que cae (no es la precondicion 6 ni cambia el paquete): una copia exportada NUEVA del cierre de PB-T4a con el
# RED de la v4.7 con su particion (el kit entra) y la sonda temporal TmpPhantomProbe.cs, que responde, para los casos de las dos copias de WPP que no
# pasan en el GREEN, que da el PREPARE del piloto y el del modelo completo (PrepareFull no cambia en el GREEN). Compila Core-tests en Debug y corre
# solo el filtro de la sonda.
# Uso: bash p6_diag_phantom.sh <copia nueva> <particion>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"; PART="$2"
MAINCO='<checkout-principal>'
REV=78a7cef3
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/out-diag-phantom-$(basename "$C")"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "diagnostico; copia $C; revision $(git -C "$MAINCO" rev-parse $REV); sonda $(sha256sum "$S/TmpPhantomProbe.cs" | cut -d' ' -f1)"
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
python -I "$PB4/apply_red_pbt4b_v47.py" "$C" --repo "$MAINCO" --rev $REV --partition "$PART" > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; exit 6; }
tail -1 "$OUT/apply-red.txt"
cp "$S/TmpPhantomProbe.cs" "$C/tests/RackCad.Tests/TmpPhantomProbe.cs"
( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/core-tests.log" 2>&1
echo "core-tests rc=$? errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/core-tests.log") avisos-CS=$(grep -c 'warning CS' "$OUT/core-tests.log")"
grep -o -E '[A-Za-z0-9_.]+\.cs\([0-9]+,[0-9]+\): error [A-Z]+[0-9]+: [^[]*' "$OUT/core-tests.log" | sort -u | head -20
if ! grep -q -E ': error [A-Z]+[0-9]+' "$OUT/core-tests.log"; then
  ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~TmpPhantomProbe" --logger "trx;LogFileName=phantom.trx" --results-directory "$C/_trx" ) > "$OUT/phantom.log" 2>&1
  echo "test phantom rc=$? trx-sha256=$(sha256sum "$C/_trx/phantom.trx" | cut -d' ' -f1)"
  python -I "$S/p6_check_v47.py" message "$C/_trx/phantom.trx" "$OUT/phantom-probe.txt" > /dev/null; cat "$OUT/phantom-probe.txt"
fi
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

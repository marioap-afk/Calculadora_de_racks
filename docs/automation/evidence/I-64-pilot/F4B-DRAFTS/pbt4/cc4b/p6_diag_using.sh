#!/usr/bin/env bash
# DIAGNOSTICO tras el fallo de la fase probe (no es la precondicion 6 ni corrige el paquete): una copia exportada NUEVA del cierre de PB-T4a con el
# RED de la v4.5 en modo de medida y, ademas, la directiva using de Draft/ en las copias que la necesitan (diag_add_using.py). Compila Core-tests en
# Debug, cuenta errores y avisos CS y, si compila, corre el filtro de las copias de Core y la particion, para ver si queda otro defecto.
# Uso: bash p6_diag_using.sh <copia nueva>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=78a7cef3
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
OUT="$S/out-diag-using"; mkdir -p "$OUT"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
python -I "$PB4/apply_red_pbt4b_v45.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red-probe.txt" 2>&1 || { echo "apply_red fallo"; exit 6; }
tail -1 "$OUT/apply-red-probe.txt"
python -I "$S/diag_add_using.py" "$C" | tee "$OUT/using.txt"
( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/core-tests.log" 2>&1
echo "core-tests rc=$? errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/core-tests.log") avisos-CS=$(grep -c 'warning CS' "$OUT/core-tests.log")"
grep -E ': error [A-Z]+[0-9]+' "$OUT/core-tests.log" | sed -E 's# \[.*$##' | sort -u | head -30
grep -o 'warning CS[0-9]*' "$OUT/core-tests.log" | sort | uniq -c
if test -f "$C/tests/RackCad.Tests/bin/Debug/net8.0/RackCad.Tests.dll" && ! grep -q -E ': error [A-Z]+[0-9]+' "$OUT/core-tests.log"; then
  t0=$(date +%s)
  ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~RackCad.Tests.FullPanelCopies" --logger "trx;LogFileName=core-probe.trx" --results-directory "$C/_trx" ) > "$OUT/core-probe.log" 2>&1
  echo "core-probe rc=$? segundos=$(( $(date +%s) - t0 ))"
  python -I "$S/p6_check.py" summary "$C/_trx/core-probe.trx" "$OUT/core-probe-outcomes.txt" > /dev/null; head -1 "$OUT/core-probe-outcomes.txt"
  # la UI no cambia con el diagnostico: su TRX es la de la fase probe (/d/r4g)
  python -I "$PB4/partition_pbt4b_v45.py" red "$C/_trx/core-probe.trx" /d/r4g/_trx/ui-probe.trx "$OUT/partition-diag.tsv" > "$OUT/partition-diag.txt" 2>&1
  echo "particion (diagnostico) rc=$?"; cat "$OUT/partition-diag.txt" | head -40
fi
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

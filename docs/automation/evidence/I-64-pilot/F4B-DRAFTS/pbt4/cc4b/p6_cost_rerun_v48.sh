#!/usr/bin/env bash
# PB-T4b, precondicion 6 de la v4.8, paso 7: la TERCERA corrida de la sonda del coste de la lectura (pbt4b/v48/session-cost-rerun-rule.md), sin cambiar
# nada de la sonda (ni n, ni los escenarios, ni el orden), sobre una copia exportada NUEVA del mismo GREEN literal de la v4.8: el RED con la particion
# de /d/r4q, el GREEN de apply_green_pbt4b_v45.py y la sonda pbt4b-read-cost-probe.cs (TmpReadCostProbe), compilada como en /d/r4s (Debug y Release)
# y corrida solo con su filtro, en Release. Antes y despues, los procesos pesados de la maquina (tasklist filtrado). Al acabar, build-server shutdown.
# Uso: bash p6_cost_rerun_v48.sh <copia nueva> <particion>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"; PART="$2"
MAINCO='<checkout-principal>'
REV=78a7cef3
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
PKG="$PB4/I-64-f4b-pbt4-gate-contract-package-v48.md"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/out-costrerun-v48-$(basename "$C")"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
procs() { # $1 etiqueta
  local l; l=$(tasklist 2>/dev/null | grep -i -E '^(dotnet|testhost|codex|msbuild|vbcscompiler|csc|node|devenv|acad)' )
  echo "procesos ($1): $(echo "$l" | grep -c . ) | $(echo "$l" | awk 'NF{print $1}' | sort | uniq -c | awk '{printf "%s x%s; ", $2, $1}')"
  echo "$l" | sed 's/^/    /' | grep -v '^    $'
}
echo "tercera corrida del coste; copia $C; salida $OUT; revision $(git -C "$MAINCO" rev-parse $REV); paquete $(sha256sum "$PKG" | cut -d' ' -f1); particion $(sha256sum "$PART" | cut -d' ' -f1)"
for f in "$0" "$PB4/pbt4b-read-cost-probe.cs" "$PB4/apply_red_pbt4b_v48.py" "$PB4/apply_green_pbt4b_v45.py" "$S/p6_check_v48.py" "$PB4/v48/session-cost-rerun-rule.md"; do
  echo "  guion $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
procs "antes"
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
python -I "$PB4/apply_red_pbt4b_v48.py" "$C" --repo "$MAINCO" --rev $REV --partition "$PART" > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; exit 6; }
python -I "$PB4/apply_green_pbt4b_v45.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; exit 7; }
tail -1 "$OUT/apply-red.txt"; tail -1 "$OUT/apply-green.txt"
cp "$PB4/pbt4b-read-cost-probe.cs" "$C/tests/RackCad.Tests/TmpReadCostProbe.cs"
echo "sonda $(sha256sum "$C/tests/RackCad.Tests/TmpReadCostProbe.cs" | cut -d' ' -f1) TmpReadCostProbe.cs"
ERR=0; WARN=0
for cfg in Debug Release; do
  ( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c $cfg --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/cost-core-tests-$cfg.log" 2>&1
  rc=$?; e=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/cost-core-tests-$cfg.log"); w=$(grep -c 'warning CS' "$OUT/cost-core-tests-$cfg.log"); ERR=$((ERR + e)); WARN=$((WARN + w))
  echo "cost-core-tests-$cfg rc=$rc errores=$e avisos-CS=$w"
done
if test "$ERR" = 0 && test "$WARN" = 0; then
  procs "antes de la sonda"
  t0=$(date +%s)
  ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~TmpReadCostProbe" --logger "trx;LogFileName=cost-run3.trx" --results-directory "$C/_trx" ) > "$OUT/cost-run3.log" 2>&1
  echo "test cost-run3.trx rc=$? segundos=$(( $(date +%s) - t0 )) trx-sha256=$(sha256sum "$C/_trx/cost-run3.trx" | cut -d' ' -f1)"
  python -I "$S/p6_check_v48.py" message "$C/_trx/cost-run3.trx" "$OUT/cost-run3.txt" > /dev/null
  python -I "$S/p6_check_v48.py" cost "$OUT/check-cost-run3.txt" "$OUT/cost-run3.txt" > /dev/null; echo "check cost (tercera) rc=$? :: $(tail -1 "$OUT/check-cost-run3.txt")"
else
  echo "PARA: la compilacion da errores $ERR y avisos CS $WARN"
fi
procs "despues"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "procesos de dotnet/testhost tras el shutdown: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

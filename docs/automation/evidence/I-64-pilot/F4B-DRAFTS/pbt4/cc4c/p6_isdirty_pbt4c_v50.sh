#!/usr/bin/env bash
# PB-T4c, precondicion 6 (paquete v5.0), paso 7: el coste de IsDirty sobre el GREEN literal de PB-T4c, con la sonda de PB-T4b
# (pbt4bcc/compile/TmpIsDirtyCostProbe.cs, que no se toca: se lee y se copia a la copia), en Release y solo con su filtro. En una copia EXPORTADA
# nueva de 6adff90d, fuera de todo worktree; nada en el checkout principal salvo git archive. Al acabar, build-server shutdown.
# Uso: bash p6_isdirty_pbt4c_v50.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
PROBE="$(cd "$S/../../pbt4bcc/compile" && pwd)/TmpIsDirtyCostProbe.cs"
C="$1"
MAINCO='<checkout-principal>'
REV=6adff90d
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/out-isdirty-v50-$(basename "$C")"; test -e "$OUT" && { echo "la salida ya existe"; exit 2; }; mkdir -p "$OUT"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
echo "coste de IsDirty; copia $C; salida out-isdirty-v50-$(basename "$C"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PB4/apply_red_pbt4c_v50.py" "$PB4/apply_green_pbt4c_v50.py" "$PB4/green_pbt4c_v50.py" "$PB4/pbt4c-green-v50.txt" "$PROBE" "$S/trx_message_v50.py" "$0"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
python -I "$PB4/apply_red_pbt4c_v50.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; exit 6; }
python -I "$PB4/apply_green_pbt4c_v50.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; exit 7; }
tail -1 "$OUT/apply-red.txt"; tail -1 "$OUT/apply-green.txt"
cp "$PROBE" "$C/tests/RackCad.Tests/TmpIsDirtyCostProbe.cs"
( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Release --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/build-release.log" 2>&1
rc=$?; lf "$OUT/build-release.log"
echo "build Release rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/build-release.log") avisos-CS=$(grep -c 'warning CS' "$OUT/build-release.log")"
t0=$(date +%s)
( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~TmpIsDirtyCostProbe" --logger "trx;LogFileName=isdirty.trx" --results-directory "$C/_trx" ) > "$OUT/isdirty.log" 2>&1
rc=$?; t1=$(date +%s); lf "$OUT/isdirty.log"
python -I "$S/trx_message_v50.py" "$C/_trx/isdirty.trx" "$OUT/isdirty-probe.txt"
echo "test isdirty.trx rc=$rc segundos=$((t1 - t0)) trx-sha256=$(sha256sum "$C/_trx/isdirty.trx" | cut -d' ' -f1)"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

#!/usr/bin/env bash
# Control negativo corregido de PB-T4a (el primero, compile_check_ctrl.sh, partia de una premisa falsa: sobre el esqueleto inerte CT-PBT3A-12
# PASA sin el Skip, como ya preveia el paquete, «sin el Skip, 19 123 + 6»; el Skip hace falta para el GREEN). En la MISMA copia:
#  1. el archivo de la guarda vuelve al de la base (sin el Skip) y se recompila solo tests/RackCad.Tests;
#  2. se anade a RIA (src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs), en la copia, una linea que nombra PushBackFullDraftReader, como hara
#     el GREEN, y se ejecuta solo CT-PBT3A-12: lo esperado es que FALLE (la guarda lee el texto de la copia y ve el nombre fuera de Draft/);
#  3. se restaura RIA byte a byte y la misma prueba PASA;
#  4. se restaura el archivo de la guarda del parche (con el Skip), se recompila y la prueba sale omitida.
# Uso: bash compile_check_ctrl2.sh <sha-base> <dir-copia de compile_check.sh>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
BASE="$1"; C="$2"
MAINCO='<checkout-principal>'
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
OUT="$S/out-ctrl2"; mkdir -p "$OUT"
G="tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs"
R="src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs"
FILTER='FullyQualifiedName~RackCad.Tests.PushBackDraftWriteGuardTests.CT_PBT3A_12'
cd "$C" || exit 3
cp "$G" "$OUT/guard.patched.cs"; cp "$R" "$OUT/ria.orig.cs"
echo "guarda con el parche sha256 $(sha256sum "$G" | cut -d' ' -f1); RIA sha256 $(sha256sum "$R" | cut -d' ' -f1)"
git -C "$MAINCO" show "$BASE:$G" > "$G"
"$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug -nodeReuse:false -p:UseSharedCompilation=false -v:minimal > "$OUT/build-noskip.log" 2>&1
echo "1. guarda de la base (sin Skip): build rc=$? errores=$(grep -c ' error ' "$OUT/build-noskip.log")"
printf '\n// control negativo de la sesion: PushBackFullDraftReader nombrado fuera de Draft/, como hara el GREEN\n' >> "$R"
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$FILTER" --logger "trx;LogFileName=ctrl2-injected.trx" --results-directory "$C/_trx" > "$OUT/ctrl2-injected.log" 2>&1
echo "2. con el nombre en RIA: rc=$?"
python -I "$S/trx_outcomes.py" "$C/_trx/ctrl2-injected.trx" "$OUT/ctrl2-injected-outcomes.txt"; head -1 "$OUT/ctrl2-injected-outcomes.txt"
grep -o 'coincidencias=\[[^]]*\]' "$OUT/ctrl2-injected.log" | sort -u | head -3
cp "$OUT/ria.orig.cs" "$R"
echo "3. RIA restaurado sha256 $(sha256sum "$R" | cut -d' ' -f1)"
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$FILTER" --logger "trx;LogFileName=ctrl2-ria-restored.trx" --results-directory "$C/_trx" > "$OUT/ctrl2-ria-restored.log" 2>&1
echo "   sin el nombre: rc=$?"
python -I "$S/trx_outcomes.py" "$C/_trx/ctrl2-ria-restored.trx" "$OUT/ctrl2-ria-restored-outcomes.txt"; head -1 "$OUT/ctrl2-ria-restored-outcomes.txt"
cp "$OUT/guard.patched.cs" "$G"
"$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug -nodeReuse:false -p:UseSharedCompilation=false -v:minimal > "$OUT/build-restored.log" 2>&1
echo "4. guarda del parche (con Skip): build rc=$? errores=$(grep -c ' error ' "$OUT/build-restored.log")"
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$FILTER" --logger "trx;LogFileName=ctrl2-skip.trx" --results-directory "$C/_trx" > "$OUT/ctrl2-skip.log" 2>&1
echo "   con el Skip: rc=$?"
python -I "$S/trx_outcomes.py" "$C/_trx/ctrl2-skip.trx" "$OUT/ctrl2-skip-outcomes.txt"; head -1 "$OUT/ctrl2-skip-outcomes.txt"
rm -f "$OUT/guard.patched.cs" "$OUT/ria.orig.cs"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

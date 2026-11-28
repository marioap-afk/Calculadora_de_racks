#!/usr/bin/env bash
# Control negativo de la compilacion de comprobacion de PB-T4a: en la MISMA copia de compile_check.sh, se quita el Skip de CT-PBT3A-12 (el archivo
# de la guarda vuelve al de la base, byte a byte), se recompila solo tests/RackCad.Tests y se ejecuta solo CT-PBT3A-12. Lo esperado: FALLA, porque
# el esqueleto nombra los tipos del modelo fuera de su carpeta: el Skip del parche es necesario. Despues se restaura el archivo del parche, se
# recompila y la misma prueba sale omitida (Skipped).
# Uso: bash compile_check_ctrl.sh <sha-base> <dir-copia de compile_check.sh>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
BASE="$1"; C="$2"
MAINCO='<checkout-principal>'
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
OUT="$S/out-ctrl"; mkdir -p "$OUT"
F="tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs"
cd "$C" || exit 3
test -f "$F" || exit 4
cp "$F" "$OUT/guard.patched.cs"
echo "con el parche: sha256 $(sha256sum "$F" | cut -d' ' -f1); Skip en el archivo: $(grep -c 'Skip' "$F")"
git -C "$MAINCO" show "$BASE:$F" > "$F"
echo "sin el Skip (el de la base): sha256 $(sha256sum "$F" | cut -d' ' -f1); Skip en el archivo: $(grep -c 'Skip' "$F")"
FILTER='FullyQualifiedName~RackCad.Tests.PushBackDraftWriteGuardTests.CT_PBT3A_12'
"$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug -nodeReuse:false -p:UseSharedCompilation=false -v:minimal > "$OUT/build-noskip.log" 2>&1
echo "build sin Skip rc=$? errores=$(grep -c ' error ' "$OUT/build-noskip.log")"
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$FILTER" --logger "trx;LogFileName=ctrl-noskip.trx" --results-directory "$C/_trx" > "$OUT/ctrl-noskip.log" 2>&1
echo "sin Skip: rc=$?"
python -I "$S/trx_outcomes.py" "$C/_trx/ctrl-noskip.trx" "$OUT/ctrl-noskip-outcomes.txt"; head -1 "$OUT/ctrl-noskip-outcomes.txt"
cp "$OUT/guard.patched.cs" "$F"
echo "restaurado: sha256 $(sha256sum "$F" | cut -d' ' -f1)"
"$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug -nodeReuse:false -p:UseSharedCompilation=false -v:minimal > "$OUT/build-restored.log" 2>&1
echo "build restaurado rc=$? errores=$(grep -c ' error ' "$OUT/build-restored.log")"
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$FILTER" --logger "trx;LogFileName=ctrl-restored.trx" --results-directory "$C/_trx" > "$OUT/ctrl-restored.log" 2>&1
echo "restaurado: rc=$?"
python -I "$S/trx_outcomes.py" "$C/_trx/ctrl-restored.trx" "$OUT/ctrl-restored-outcomes.txt"; head -1 "$OUT/ctrl-restored-outcomes.txt"
rm -f "$OUT/guard.patched.cs"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

#!/usr/bin/env bash
# Control negativo opcional de la compilacion de comprobacion de PB-T3c (CA-T3c §, la falta de control negativo): en la MISMA copia de
# compile_check.sh, se mete la cadena PushBack en el CODIGO de la fachada (un identificador, que Clean no quita) y se ejecuta solo
# CT-PBT1B-15, que lee el texto de src/RackCad.Plugin al ejecutar (RepoRoot sube hasta el RackCad.sln de la copia), sin recompilar.
# Lo esperado: falla, con archivos=15 en vez de 14. Despues se restaura el archivo del esqueleto byte a byte y la misma prueba pasa.
# Uso: bash compile_check_ctrl.sh <dir-copia de compile_check.sh>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
OUT="$S/out-ctrl"; mkdir -p "$OUT"
F="$C/src/RackCad.Plugin/Views/RackViewBlockEraser.cs"
test -f "$F" || { echo "sin el archivo del esqueleto en la copia"; exit 2; }
cd "$C" || exit 3
echo "esqueleto antes: sha256 $(sha256sum "$F" | cut -d' ' -f1)"
cp "$F" "$OUT/RackViewBlockEraser.cs.orig"
grep -n 'internal static class RackViewBlockEraser' "$F" > /dev/null || { echo "sin la clase"; exit 4; }
sed -i '/internal static class RackViewBlockEraser/{n;s/$/\n        private const int PushBackNegativeControl = 0;/}' "$F"
echo "inyectado: $(grep -c 'PushBackNegativeControl' "$F") linea(s)"
FILTER='FullyQualifiedName~RackCad.Tests.PushBackSharedReferenceGuardTests.CT_PBT1B_15'
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$FILTER" --logger "trx;LogFileName=ctrl-injected.trx" --results-directory "$C/_trx" > "$OUT/ctrl-injected.log" 2>&1
echo "con la inyeccion: rc=$?"
grep -o 'archivos=1[0-9]' "$OUT/ctrl-injected.log" | sort | uniq -c
python -I "$S/trx_outcomes.py" "$C/_trx/ctrl-injected.trx" "$OUT/ctrl-injected-outcomes.txt"; head -1 "$OUT/ctrl-injected-outcomes.txt"
cp "$OUT/RackViewBlockEraser.cs.orig" "$F"
echo "esqueleto restaurado: sha256 $(sha256sum "$F" | cut -d' ' -f1)"
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$FILTER" --logger "trx;LogFileName=ctrl-restored.trx" --results-directory "$C/_trx" > "$OUT/ctrl-restored.log" 2>&1
echo "restaurado: rc=$?"
python -I "$S/trx_outcomes.py" "$C/_trx/ctrl-restored.trx" "$OUT/ctrl-restored-outcomes.txt"; head -1 "$OUT/ctrl-restored-outcomes.txt"
rm -f "$OUT/RackViewBlockEraser.cs.orig"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

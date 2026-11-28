#!/usr/bin/env bash
# Control negativo del tercer choque (por reflexion) sobre la copia de compile_check3.sh: devuelve PrepareFull a Prepare en PP,
# recompila las pruebas de Core y ejecuta solo WorkspacePilotPrepareTests. Se espera que falle la prueba de :2155 (Assert.Single "Prepare").
# Uso: bash compile_check3_ctrl.sh <dir-copia de compile_check3.sh>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"; C="$1"
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
OUT="$S/out3ctrl"; mkdir -p "$OUT"
F="$C/src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs"
n=$(grep -c 'public static PushBackPilotPrepareResult PrepareFull(PushBackFullPrepareInput input' "$F"); test "$n" = 1 || { echo "PrepareFull no esta una vez ($n)"; exit 2; }
sed -i 's/public static PushBackPilotPrepareResult PrepareFull(PushBackFullPrepareInput input/public static PushBackPilotPrepareResult Prepare(PushBackFullPrepareInput input/' "$F"
cd "$C" || exit 3
"$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug -nodeReuse:false -p:UseSharedCompilation=false -v:minimal > "$OUT/core-tests.log" 2>&1
echo "core-tests rc=$? errores=$(grep -c ' error ' "$OUT/core-tests.log")"
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~WorkspacePilotPrepareTests" --logger "trx;LogFileName=ctrl.trx" --results-directory "$C/_trx_ctrl" > "$OUT/core-test.log" 2>&1
echo "core-test rc=$?"
cp "$C/_trx_ctrl/ctrl.trx" "$OUT/ctrl.trx" 2>/dev/null
python -I "$S/trx_failed.py" "$OUT/ctrl.trx"
sed -i 's/public static PushBackPilotPrepareResult Prepare(PushBackFullPrepareInput input/public static PushBackPilotPrepareResult PrepareFull(PushBackFullPrepareInput input/' "$F"
echo "restaurado PrepareFull: $(grep -c 'PrepareFull(PushBackFullPrepareInput input' "$F")"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"

#!/usr/bin/env bash
# PB-T4d (paquete v5.2, fase 2; OWNER-05): la prueba lenta de la copia de WPP, sola y sin tocarla, sobre una copia EXPORTADA nueva de 6c2820ba tal
# cual (fuera de todo worktree; nada en el checkout principal salvo git archive), en Debug como la CI, sola en la maquina; y la sonda de su bucle
# (pbt4d-probe-slow.cs, clase temporal), en la misma copia. El SDK de usuario, con --no-incremental -nodeReuse:false -p:UseSharedCompilation=false;
# build-server shutdown al final. Salida corta: <copia>-sl/.
# Uso: bash p6d_slow_v52.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
CC4="$(cd "$S/../../pbt4ccc/compile" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
TEST='FullyQualifiedName=RackCad.Tests.FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten'
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-sl"; test -e "$OUT" && { echo "la salida ya existe"; exit 2; }; mkdir -p "$OUT"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
echo "la prueba lenta; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PB4/v52/pbt4d-probe-slow.cs" "$S/p6d_check_v52.py" "$CC4/trx_message_v50.py" "$0"; do echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"; done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
b() { ( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/$1.log" 2>&1
      local rc=$?; lf "$OUT/$1.log"; echo "$1 rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log") avisos-CS=$(grep -c 'warning CS' "$OUT/$1.log")"; }
t() { local t0=$(date +%s)
      ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$2" --logger "trx;LogFileName=$1" --results-directory "$C/_trx" ) > "$OUT/${1%.trx}.log" 2>&1
      local rc=$? t1=$(date +%s); lf "$OUT/${1%.trx}.log"
      python -I "$S/p6d_check_v52.py" "$C/_trx/$1" "$OUT/${1%.trx}-res.txt" > /dev/null
      echo "test $1 rc=$rc segundos=$((t1 - t0)) trx-sha256=$(sha256sum "$C/_trx/$1" | cut -d' ' -f1) :: $(head -1 "$OUT/${1%.trx}-res.txt")"
      grep -o 'duration="[^"]*"' "$C/_trx/$1" | head -3 | sed 's/^/  /'; }
b base
t lenta-1.trx "$TEST"
t lenta-2.trx "$TEST"
cp "$PB4/v52/pbt4d-probe-slow.cs" "$C/tests/RackCad.Tests/TmpSlow.cs"
b sonda
t sonda.trx "FullyQualifiedName~RackCad.Tests.TmpPbt4dSlowProbe"
python -I "$CC4/trx_message_v50.py" "$C/_trx/sonda.trx" "$OUT/sonda-lineas.txt"
cat "$OUT/sonda-lineas.txt"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

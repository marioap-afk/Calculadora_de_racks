#!/usr/bin/env bash
# PB-T4d (paquete v5.2): la cobertura de la corrida generica que ejercitan las 48 de WorkspacePilotUpdateRunTests (WPUR) y si la cubre lo que se
# queda (owner-pbt4d-substitutes-decision.md, la nota de la sesion). En una copia EXPORTADA nueva de 6c2820ba, sin cambios (la base), fuera de todo
# worktree; nada en el checkout principal salvo git archive. Se compila Core en Debug y se corren, con coverlet (cobertura de lineas y ramas, solo
# RackCad.Application), estos filtros, cada uno con su TRX y su cobertura:
#   wpur  WPUR sola;
#   resto la suite COMPLETA de Core menos WPUR (lo que se queda, salvo las otras bajas de PB-T4d, que no corren la corrida: ver cov_v52.py);
#   y, para las citas, por clase: PushBackFullWriteRunTests, las copias de WorkspacePilotUpdateHost (Red y Green), las de
#   WorkspacePilotUpdateRequest, PushBackFullPanelTests, PushBackEditAllTests y PushBackFullWritePrepareTests.
# El SDK de usuario, -nodeReuse:false -p:UseSharedCompilation=false; build-server shutdown al final. UTF-8 con LF; nombres cortos (<copia>-cv/).
# Uso: bash p6d_cov_v52.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-cv"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "cobertura de la corrida generica; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
echo "  pieza $(sha256sum "$0" | cut -d' ' -f1) ${0##*/}"
echo "  pieza $(sha256sum "$S/cov_v52.runsettings" | cut -d' ' -f1) cov_v52.runsettings"
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
echo "copia exportada: $(find "$C" -type f | wc -l) archivos"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/b-core.log" 2>&1
echo "b-core rc=$? errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/b-core.log") avisos-CS=$(grep -c 'warning CS' "$OUT/b-core.log")"; lf "$OUT/b-core.log"
cov() { # $1 etiqueta, $2 filtro
  local t0=$(date +%s)
  ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$2" --logger "trx;LogFileName=$1.trx" \
      --results-directory "$C/_cov/$1" --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v52.runsettings")" ) > "$OUT/$1.log" 2>&1
  local rc=$? t1=$(date +%s)
  lf "$OUT/$1.log"
  local x; x=$(find "$C/_cov/$1" -name 'coverage.cobertura.xml' | head -1)
  test -n "$x" && cp "$x" "$OUT/$1.cobertura.xml"
  python -I "$S/p6d_check_v52.py" "$C/_cov/$1/$1.trx" "$OUT/$1-res.txt" > /dev/null
  echo "cov $1 rc=$rc segundos=$((t1 - t0)) cobertura=$(test -n "$x" && sha256sum "$OUT/$1.cobertura.xml" | cut -c1-16) :: $(head -1 "$OUT/$1-res.txt")"
}
cov wpur 'FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateRunTests'
cov fwr 'FullyQualifiedName~RackCad.Tests.PushBackFullWriteRunTests'
cov wpuh 'FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHost'
cov wpuq 'FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequest'
cov fpt 'FullyQualifiedName~RackCad.Tests.PushBackFullPanelTests|FullyQualifiedName~RackCad.Tests.PushBackEditAllTests'
cov fwp 'FullyQualifiedName~RackCad.Tests.PushBackFullWritePrepareTests'
cov resto 'FullyQualifiedName!~RackCad.Tests.WorkspacePilotUpdateRunTests'
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

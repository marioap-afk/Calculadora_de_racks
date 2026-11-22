#!/usr/bin/env bash
# PB-T4d (paquete v5.2): la cobertura PRUEBA A PRUEBA de WorkspacePilotUpdateRunTests (WPUR), para saber que pruebas son las unicas que
# ejercitan cada camino de la corrida generica que el resto de la suite no ejercita (cov_v52.py). En una copia EXPORTADA nueva de 6c2820ba, sin
# cambios, fuera de todo worktree; nada en el checkout principal salvo git archive. Se compila Core en Debug y se corre cada metodo de WPUR
# solo, con coverlet (cov_v52.runsettings). El SDK de usuario; build-server shutdown al final. UTF-8 con LF; nombres cortos (<copia>-ct/).
# Uso: bash p6d_covtest_v52.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-ct"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "cobertura prueba a prueba de WPUR; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
echo "  pieza $(sha256sum "$0" | cut -d' ' -f1) ${0##*/}"
echo "  pieza $(sha256sum "$S/cov_v52.runsettings" | cut -d' ' -f1) cov_v52.runsettings"
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
echo "copia exportada: $(find "$C" -type f | wc -l) archivos"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/b-core.log" 2>&1
echo "b-core rc=$? errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/b-core.log") avisos-CS=$(grep -c 'warning CS' "$OUT/b-core.log")"; lf "$OUT/b-core.log"
names=$(git -C "$MAINCO" show "$REV:tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs" | tr -d '\r' | grep -E '^\s*public void ' | sed -E 's/^\s*public void ([A-Za-z0-9_]+).*/\1/')
n=0
for name in $names; do
  n=$((n + 1)); nn=$(printf '%02d' $n)
  ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateRunTests.$name" \
      --results-directory "$C/_cov/$nn" --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v52.runsettings")" ) > "$OUT/$nn.log" 2>&1
  rc=$?; lf "$OUT/$nn.log"
  x=$(find "$C/_cov/$nn" -name 'coverage.cobertura.xml' | head -1)
  test -n "$x" && cp "$x" "$OUT/$nn.cobertura.xml"
  echo "$nn $name rc=$rc $(grep -o -E '(Superado|Correctas|Passed)[^,]*' "$OUT/$nn.log" | head -1)" | tee -a "$OUT/pruebas.txt"
done
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

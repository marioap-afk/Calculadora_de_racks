#!/usr/bin/env bash
# PB-T4d (paquete v5.2), precondicion 6: la cobertura de REFERENCIA, rama por rama. En una copia EXPORTADA nueva de 6c2820ba sin cambios (la base,
# con el piloto y con WPUR), la suite COMPLETA de Core con coverlet en JSON y Cobertura (cov_v52b.runsettings: solo RackCad.Application). El JSON
# dice que rama de cada linea se cubre: es la referencia con la que cov_p6_v52.py compara la cobertura del GREEN (owner-pbt4d-deadcode-27-
# decisions.md, pregunta 2). Nada en el checkout principal salvo git archive. El SDK de usuario, -nodeReuse:false -p:UseSharedCompilation=false;
# build-server shutdown al final. UTF-8 con LF; nombres cortos (<copia>-bc/).
# Uso: bash p6d_basecov_v52.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-bc"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "cobertura de referencia de la base; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$0" "$S/cov_v52b.runsettings" "$S/p6d_check_v52.py"; do echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"; done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
echo "copia exportada: $(find "$C" -type f | wc -l) archivos"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/b-core.log" 2>&1
echo "b-core rc=$? errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/b-core.log") avisos-CS=$(grep -c 'warning CS' "$OUT/b-core.log")"; lf "$OUT/b-core.log"
t0=$(date +%s)
( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=base.trx" \
    --results-directory "$C/_cov" --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v52b.runsettings")" ) > "$OUT/base.log" 2>&1
rc=$?; t1=$(date +%s); lf "$OUT/base.log"
j=$(find "$C/_cov" -name 'coverage.json' | head -1); x=$(find "$C/_cov" -name 'coverage.cobertura.xml' | head -1)
test -n "$j" && cp "$j" "$OUT/base.coverage.json"; test -n "$x" && cp "$x" "$OUT/base.cobertura.xml"
python -I "$S/p6d_check_v52.py" "$C/_cov/base.trx" "$OUT/base-res.txt" > /dev/null
echo "cov base rc=$rc segundos=$((t1 - t0)) json=$(test -n "$j" && sha256sum "$OUT/base.coverage.json" | cut -c1-16) cobertura=$(test -n "$x" && sha256sum "$OUT/base.cobertura.xml" | cut -c1-16) :: $(head -1 "$OUT/base-res.txt")"
sed -n '2,20p' "$OUT/base-res.txt" | cut -c1-260
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

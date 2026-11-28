#!/usr/bin/env bash
# PB-T4d (paquete v5.3): ¿es ruido de la medida? Sobre copias YA compiladas (la base, /d/r6w, y el GREEN del ensayo 2, /d/r6x), corre N veces
# un filtro de UI con cobertura (cov_v53.runsettings) y da los golpes de unas lineas en cada corrida (json_lines_v53.py). No cambia nada de las
# copias salvo sus carpetas de resultados (_noise/). El SDK de usuario; build-server shutdown al final.
# Uso: bash noise_v53.sh <filtro> <ruta-que-acaba-en> <lineas> <N> <etiqueta> <copia>...
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
F="$1"; END="$2"; LINES="$3"; N="$4"; TAG="$5"; shift 5
OUT="$S/noise-$TAG"; mkdir -p "$OUT"
for C in "$@"; do
  for i in $(seq 1 "$N"); do
    R="$C/_noise/$TAG-$i"
    ( cd "$C" && "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-build --filter "$F" --results-directory "$R" \
        --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v53.runsettings")" ) > "$OUT/$(basename "$C")-$i.log" 2>&1
    j=$(find "$R" -name 'coverage.json' | head -1)
    echo "$(basename "$C") corrida $i rc=$? $(grep -E 'Superado|Passed!|Correctas|Failed!|Con error' "$OUT/$(basename "$C")-$i.log" | tail -1 | tr -d '\r' | cut -c1-120)"
    test -n "$j" && python -I "$S/json_lines_v53.py" "$END" "$LINES" "$j" | sed 's/^.*coverage.json/  golpes/'
  done
done
"$DN" build-server shutdown > /dev/null 2>&1; echo "build-server shutdown rc=$?"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"

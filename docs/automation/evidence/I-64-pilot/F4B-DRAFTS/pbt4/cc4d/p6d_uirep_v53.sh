#!/usr/bin/env bash
# PB-T4d (paquete v5.3), precondicion 6: repeticiones de la union de los filtros de UI con cobertura (cov_v53.runsettings) sobre una copia YA
# compilada, para que la cobertura de UI no dependa de una sola corrida: los golpes de un binding de WPF dependen del despachador (medido en
# noise-batch/: la base da 1, 0 y 1 en tres corridas de la misma prueba). Copia la TRX y el JSON de cada corrida a <salida>/<etiqueta>-<i>.*.
# Sola: comprueba que no hay otro dotnet ni testhost al empezar. El SDK de usuario; build-server shutdown al final. UTF-8 con LF.
# Uso: bash p6d_uirep_v53.sh <copia> <dir-salida> <etiqueta> <N>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
C="$1"; OUT="$2"; TAG="$3"; N="$4"
UNION="$(cat "$S/ui_union_v53.txt")"
procs() { tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost'; }
echo "repeticiones de la union de UI con cobertura; copia $C; salida $(basename "$OUT"); procesos al empezar: $(procs) ($(date '+%H:%M:%S'))"
test "$(procs)" -eq 0 || { echo "hay otro dotnet o testhost"; exit 4; }
mkdir -p "$OUT"
for i in $(seq 1 "$N"); do
  R="$C/_trx/$TAG-$i"; test -e "$R" && { echo "ya existe $R"; exit 2; }
  t0=$(date +%s)
  ( cd "$C" && "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-build --filter "$UNION" --logger "trx;LogFileName=$TAG-$i.trx" \
      --results-directory "$R" --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v53.runsettings")" ) > "$OUT/$TAG-$i.log" 2>&1
  rc=$?; t1=$(date +%s); tr -d '\r' < "$OUT/$TAG-$i.log" > "$OUT/$TAG-$i.log.lf" && mv "$OUT/$TAG-$i.log.lf" "$OUT/$TAG-$i.log"
  cp "$R/$TAG-$i.trx" "$OUT/$TAG-$i.trx"
  python -I "$S/p6d_check_v52.py" "$OUT/$TAG-$i.trx" "$OUT/$TAG-$i-res.txt" > /dev/null
  j=$(find "$R" -name 'coverage.json' | head -1); test -n "$j" && cp "$j" "$OUT/$TAG-$i.coverage.json"
  echo "ui $TAG-$i rc=$rc segundos=$((t1 - t0)) :: $(head -1 "$OUT/$TAG-$i-res.txt") json=$(sha256sum "$OUT/$TAG-$i.coverage.json" | cut -c1-16)"
done
"$DN" build-server shutdown > /dev/null 2>&1; echo "build-server shutdown rc=$?"
echo "procesos al acabar: $(procs) ($(date '+%H:%M:%S'))"

#!/usr/bin/env bash
# PB-T4d (paquete v5.3), precondicion 6: cada filtro de UI por separado, con cobertura (cov_v53.runsettings), sobre una copia YA compilada: como
# los corre el contrato, cada uno en su proceso. La cobertura de UI de una rama es la union de estas corridas y de las de la union de los filtros
# (p6d_uirep_v53.sh): un binding de WPF da golpes que dependen de lo que corrio antes en el mismo proceso (noise-batch/; R-T4-38). Copia la TRX y el
# JSON de cada corrida a <salida>/<etiqueta>-NN.*. Sola: comprueba que no hay otro dotnet ni testhost al empezar. El SDK de usuario; build-server
# shutdown al final. UTF-8 con LF.
# Uso: bash p6d_uifilt_v53.sh <copia> <dir-salida> <etiqueta> <archivo-de-filtros>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
C="$1"; OUT="$2"; TAG="$3"; UIF="$4"
procs() { tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost'; }
echo "filtros de UI por separado con cobertura; copia $C; salida $(basename "$OUT"); filtros $(basename "$UIF") $(sha256sum "$UIF" | cut -c1-16); procesos al empezar: $(procs) ($(date '+%H:%M:%S'))"
test "$(procs)" -eq 0 || { echo "hay otro dotnet o testhost"; exit 4; }
mkdir -p "$OUT"
n=0
while IFS= read -r f; do
  test -z "$f" && continue
  n=$((n + 1)); tag="$TAG-$(printf '%02d' $n)"; R="$C/_trx/$tag"; test -e "$R" && { echo "ya existe $R"; exit 2; }
  ( cd "$C" && "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-build --filter "$f" --logger "trx;LogFileName=$tag.trx" \
      --results-directory "$R" --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v53.runsettings")" ) > "$OUT/$tag.log" 2>&1
  rc=$?; tr -d '\r' < "$OUT/$tag.log" > "$OUT/$tag.log.lf" && mv "$OUT/$tag.log.lf" "$OUT/$tag.log"
  if test -f "$R/$tag.trx"; then
    cp "$R/$tag.trx" "$OUT/$tag.trx"
    python -I "$S/p6d_check_v52.py" "$OUT/$tag.trx" "$OUT/$tag-res.txt" > /dev/null
    j=$(find "$R" -name 'coverage.json' | head -1); test -n "$j" && cp "$j" "$OUT/$tag.coverage.json"
    echo "  ui $tag rc=$rc $f :: $(head -1 "$OUT/$tag-res.txt") json=$(test -n "$j" && sha256sum "$OUT/$tag.coverage.json" | cut -c1-16)"
  else
    echo "  ui $tag rc=$rc $f :: sin TRX (ningun resultado)"
  fi
done < "$UIF"
"$DN" build-server shutdown > /dev/null 2>&1; echo "build-server shutdown rc=$?"
echo "procesos al acabar: $(procs) ($(date '+%H:%M:%S'))"

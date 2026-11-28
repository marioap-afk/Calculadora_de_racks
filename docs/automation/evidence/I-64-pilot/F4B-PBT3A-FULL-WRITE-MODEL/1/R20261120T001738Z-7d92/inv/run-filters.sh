#!/bin/bash
# Uso: DOTNET=<ruta-del-dotnet> run-filters.sh <carpeta-de-resultados> <fase> <subconjunto> [<desde> <hasta>]
# subconjunto: all | red (solo los dos filtros con ExpectRed) | green-red (igual) | others (todos menos los dos con ExpectRed)
# Corre cada filtro del contrato uno a uno (sin copiar arboles), guarda TRX y log, y deja una fila por filtro en results.tsv.
# Solo lectura sobre el producto: no escribe fuera de la carpeta de resultados.
OUT="$1"
PHASE="$2"
SUBSET="$3"
START="${4:-1}"
END="${5:-9999}"
LIST="$(dirname "$0")/filters.txt"
mkdir -p "$OUT"
TSV="$OUT/results-$PHASE-$SUBSET-$START.tsv"
: > "$TSV"
INDEX=0
while IFS='|' read -r KIND FILTER MIN EXPECTRED; do
  [ -z "$KIND" ] && continue
  if [ "$SUBSET" = "red" ] && [ "$EXPECTRED" != "1" ]; then continue; fi
  if [ "$SUBSET" = "others" ] && [ "$EXPECTRED" = "1" ]; then continue; fi
  INDEX=$((INDEX + 1))
  if [ "$INDEX" -lt "$START" ] || [ "$INDEX" -gt "$END" ]; then continue; fi
  NAME="$PHASE-$SUBSET-$INDEX"
  if [ "$KIND" = "core" ]; then PROJECT="tests/RackCad.Tests/RackCad.Tests.csproj"; else PROJECT="tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj"; fi
  CMD="dotnet test $PROJECT --no-build --filter \"FullyQualifiedName~$FILTER\" --logger \"trx;LogFileName=$NAME.trx\" --results-directory $OUT --blame-hang-timeout 10m --blame-hang-dump-type none"
  "$DOTNET" test "$PROJECT" --no-build --filter "FullyQualifiedName~$FILTER" --logger "trx;LogFileName=$NAME.trx" --results-directory "$OUT" --blame-hang-timeout 10m --blame-hang-dump-type none > "$OUT/$NAME.log" 2>&1
  EXITCODE=$?
  LINE=$(grep -E "Con error: *[0-9]+, Superado: *[0-9]+, Omitido: *[0-9]+, Total: *[0-9]+" "$OUT/$NAME.log" | tail -1)
  FAILED=$(echo "$LINE" | sed -E 's/.*Con error: *([0-9]+),.*/\1/')
  PASSED=$(echo "$LINE" | sed -E 's/.*Superado: *([0-9]+),.*/\1/')
  SKIPPED=$(echo "$LINE" | sed -E 's/.*Omitido: *([0-9]+),.*/\1/')
  TOTAL=$(echo "$LINE" | sed -E 's/.*Total: *([0-9]+),.*/\1/')
  if [ -z "$LINE" ]; then FAILED="?"; PASSED="?"; SKIPPED="?"; TOTAL="0"; fi
  printf '%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n' "$PHASE" "$KIND" "$FILTER" "$MIN" "$EXPECTRED" "$TOTAL" "$PASSED" "$FAILED" "$SKIPPED" "$EXITCODE" "$CMD" "$NAME.trx" >> "$TSV"
done < "$LIST"
cat "$TSV" | cut -f1-10

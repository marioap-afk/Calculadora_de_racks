#!/bin/bash
# Uso (cwd = worktree, DOTNET = dotnet.exe del usuario): bash run_filters.sh <RunRef> <proyecto: core|ui> <archivo-de-filtros> [--no-build]
# Corre cada filtro, uno a uno, en Debug, con TRX y log propios, y escribe una linea de resumen por filtro.
REL=artifacts/orchestration/I-64/F4B-PBT4-RETIRE-PILOT/1/R20261122T173325Z-c9e1
RUN=$1; PROJ=$2; LIST=$3; NB=$4
if [ "$PROJ" = core ]; then CSPROJ=tests/RackCad.Tests/RackCad.Tests.csproj; else CSPROJ=tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj; fi
OUT=$REL/tests/$RUN
mkdir -p $OUT
i=0
while IFS= read -r F; do
  [ -z "$F" ] && continue
  i=$((i+1))
  N=$(printf "%s%02d" $PROJ $i)
  "$DOTNET" test $CSPROJ $NB -c Debug --filter "$F" --logger "trx;LogFileName=$N.trx" --results-directory $OUT --blame-hang-timeout 10m --blame-hang-dump-type none > $OUT/$N.log 2>&1
  echo "$N | $F | exit=$? | $(grep -E 'Correctas!|Erróneas!|Errores!|Passed!|Failed!|Con error' $OUT/$N.log | tail -1)" >> $OUT/summary.txt
done < $LIST
echo DONE >> $OUT/summary.txt

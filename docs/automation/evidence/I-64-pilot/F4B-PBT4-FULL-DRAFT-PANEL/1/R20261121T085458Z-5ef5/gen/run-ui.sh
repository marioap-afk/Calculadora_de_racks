#!/bin/bash
# Corre, uno a uno, los filtros de UI del contrato (nunca la suite completa de UI). Uso: run-ui.sh <RunRef> <config> <filtro-corto>...
# Cada filtro es la parte tras "RackCad.UI.Tests." (por ejemplo "Workspace"). TRX y log en tests/<RunRef>/.
W=<worktree-de-I-64>
R=artifacts/orchestration/I-64/F4B-PBT4-FULL-DRAFT-PANEL/1/R20261121T085458Z-5ef5
DN=<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe
cd "$W" || exit 9
ref=$1; cfg=$2; shift 2
mkdir -p "$R/tests/$ref"
for f in "$@"; do
  name=$(echo "$f" | tr -c 'A-Za-z0-9\n' '_')
  "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c "$cfg" --filter "FullyQualifiedName~RackCad.UI.Tests.$f" --logger "trx;LogFileName=ui-$name.trx" --results-directory "$R/tests/$ref" --blame-hang-timeout 10m --blame-hang-dump-type none > "$R/tests/$ref/ui-$name.log" 2>&1
  echo "$f exit=$? :: $(grep -E '^(Correctas|Erróneas|Error)! -|Superado|Con error' "$R/tests/$ref/ui-$name.log" | tail -1)"
done

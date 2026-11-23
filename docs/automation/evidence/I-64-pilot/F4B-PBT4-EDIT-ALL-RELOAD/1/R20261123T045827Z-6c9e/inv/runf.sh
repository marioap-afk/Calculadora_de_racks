#!/bin/bash
# Uso: runf.sh <RunRef> <core|ui> <filtro> <nombre-corto>
# Corre un filtro del contrato en Debug (con TRX y log bajo tests/<RunRef>/), desde la raiz del worktree. Requiere $DOTNET.
A=artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-RELOAD/1/R20261123T045827Z-6c9e
R=$1; K=$2; F=$3; N=$4
if [ "$K" = "core" ]; then P=tests/RackCad.Tests/RackCad.Tests.csproj; else P=tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj; fi
mkdir -p $A/tests/$R
"$DOTNET" test $P --filter "FullyQualifiedName~$F" --logger "trx;LogFileName=$N.trx" --results-directory $A/tests/$R --blame-hang-timeout 10m --blame-hang-dump-type none > $A/tests/$R/log-$N.txt 2>&1
echo "rc=$?" >> $A/tests/$R/log-$N.txt
python -I $A/inv/trxsum.py $A/tests/$R/$N.trx

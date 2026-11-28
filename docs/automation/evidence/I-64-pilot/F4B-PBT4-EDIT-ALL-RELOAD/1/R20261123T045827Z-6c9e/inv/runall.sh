#!/bin/bash
# Uso: runall.sh <RunRef> <incluir-red: 0|1>
# Corre, uno a uno, los filtros del RequiredTests (inv/filters.tsv); con 0 omite los dos con ExpectRed. Resumen en tests/<RunRef>/all-summary.txt
A=artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-RELOAD/1/R20261123T045827Z-6c9e
R=$1; INC=$2
mkdir -p $A/tests/$R
: > $A/tests/$R/all-summary.txt
i=0
while IFS=$'\t' read -r k f min red; do
  if [ "$red" = red ] && [ "$INC" != 1 ]; then continue; fi
  i=$((i+1)); n=$(echo "$f" | sed 's/.*\.//')
  echo "== $k $f min=$min red=$red" >> $A/tests/$R/all-summary.txt
  bash $A/inv/runf.sh $R $k "$f" "$k$i-$n" >> $A/tests/$R/all-summary.txt 2>&1
done < $A/inv/filters.tsv
echo "ALLDONE" >> $A/tests/$R/all-summary.txt

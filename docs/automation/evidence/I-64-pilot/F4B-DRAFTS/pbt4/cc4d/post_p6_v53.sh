#!/usr/bin/env bash
# PB-T4d (paquete v5.3), precondicion 6: las lecturas tras las corridas (no corre dotnet): la cobertura ampliada (cov_p6_v53.py, con la UI de cada
# lado como union de corridas, D-S16), los RequiredTests (required_pbt4d_v53.py), los totales de UI (ui_totals_v52.py) y el resumen
# (p6_json_pbt4d_v53.py). Todo con python -I <archivo>. UTF-8 con LF.
# Uso: bash post_p6_v53.sh <p6> <bc> <ct> <lista.tsv>
set -euo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
cd "$S"
P6="$1"; BC="$2"; CT="$3"; LIST="$4"
REPO='<checkout-principal>'
BU=(BASE_UI="$BC/base-ui-cov.coverage.json" BASE_UI="$BC/base-ui-cov-r-1.coverage.json" BASE_UI="$BC/base-ui-cov-r-2.coverage.json")
for i in $(seq -w 1 19); do BU+=(BASE_UI="$BC/base-uif-$i.coverage.json"); done
GU=(GREEN_UI="$P6/ui-green-cov.coverage.json" GREEN_UI="$P6/ui-green-cov-r-1.coverage.json" GREEN_UI="$P6/ui-green-cov-r-2.coverage.json")
for i in $(seq -w 1 18); do GU+=(GREEN_UI="$P6/green-ui$i.coverage.json"); done
python -I cov_p6_v53.py "$REPO" ../../pbt4/pbt4d-green-v53.txt r6j-cv cov-p6-v53.txt BASE_CORE="$BC/base-core-cov.coverage.json" "${BU[@]}" \
    GREEN_CORE="$P6/core-green-cov.coverage.json" "${GU[@]}"
RU=(); GR=()
for i in $(seq -w 1 18); do RU+=(RED_UI="$P6/red-ui$i.trx"); GR+=(GREEN_UI="$P6/green-ui$i.trx"); done
python -I required_pbt4d_v53.py ../../pbt4/v52/pbt4c-required-6c2820ba.tsv required-pbt4d.tsv required-pbt4d.txt BASE_CORE="$BC/base-core-cov.trx" \
    BASE_UI="$BC/base-ui-cov.trx" RED_CORE="$P6/core-red.trx" "${RU[@]}" GREEN_CORE="$P6/core-green.trx" "${GR[@]}" | tail -6
python -I ui_totals_v52.py ui-totals-red.txt red BASE="$BC/base-ui-cov.trx" $(for i in $(seq -w 1 18); do printf 'RAMA=%s ' "$P6/red-ui$i.trx"; done)
python -I ui_totals_v52.py ui-totals-green.txt green BASE="$BC/base-ui-cov.trx" $(for i in $(seq -w 1 18); do printf 'RAMA=%s ' "$P6/green-ui$i.trx"; done)
python -I p6_json_pbt4d_v53.py . "$P6" "$BC" "$CT" "$LIST"
sha256sum cov-p6-v53.txt required-pbt4d.tsv required-pbt4d.txt ui-totals-red.txt ui-totals-green.txt pbt4d-precondition6.json compile-check.md

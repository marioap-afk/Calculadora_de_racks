#!/usr/bin/env bash
# PB-T4e (paquete v5.5), precondicion 6, lo que sigue a la copia principal (r72) y a la base (r73): el control de las CT (r74 y r75, nuevas), los
# RequiredTests heredados del contrato emitido de PB-T4d, la cobertura, los RequiredTests y los totales, el resumen y la v5.5 del paquete.
# Uso (desde pbt4ecc/compile/): bash post_p6_v55.sh
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
REPO='<checkout-principal>'
cd "$S"
bash p6_control_v55.sh /d/r74 campo 2>&1 | tr -d '\r' > driver-r74-ct.txt; echo "r74 $(tail -2 driver-r74-ct.txt | head -1)"
bash p6_control_v55.sh /d/r75 anfitrion 2>&1 | tr -d '\r' > driver-r75-ct.txt; echo "r75 $(tail -2 driver-r75-ct.txt | head -1)"
python -I req_pbt4d_v55.py "$REPO" 160e019d required-pbt4d-emitted.tsv
BUI=(r73-bc/base-ui-cov.coverage.json r73-bc/base-ui-cov-r-1.coverage.json r73-bc/base-ui-cov-r-2.coverage.json)
for f in r73-bc/base-uif*.coverage.json; do BUI+=("$f"); done
GUI=(r72-p6/ui-green-cov.coverage.json r72-p6/ui-green-cov-r-1.coverage.json r72-p6/ui-green-cov-r-2.coverage.json)
for f in r72-p6/green-ui*.coverage.json; do GUI+=("$f"); done
args=(BASE_CORE=r73-bc/base-core-cov.coverage.json GREEN_CORE=r72-p6/core-green-cov.coverage.json)
for f in "${BUI[@]}"; do args+=("BASE_UI=$f"); done
for f in "${GUI[@]}"; do args+=("GREEN_UI=$f"); done
echo "cobertura: base UI ${#BUI[@]} corridas, GREEN UI ${#GUI[@]} corridas"
python -I cov_p6_pbt4e_v55.py "$REPO" ../../pbt4/pbt4e-green-v55.txt cov-p6-v55.txt "${args[@]}"
rargs=(BASE_CORE=r73-bc/base-core-cov.trx RED_CORE=r72-p6/core-red.trx GREEN_CORE=r72-p6/core-green.trx)
for f in r73-bc/base-uif*.trx; do rargs+=("BASE_UI=$f"); done
for f in r72-p6/red-ui*.trx; do rargs+=("RED_UI=$f"); done
for f in r72-p6/green-ui*.trx; do rargs+=("GREEN_UI=$f"); done
python -I required_pbt4e_v55.py required-pbt4d-emitted.tsv required-pbt4e.tsv required-pbt4e.txt "${rargs[@]}"
python -I p6_json_pbt4e_v55.py "$REPO"
( cd ../../pbt4/v55 && python -I make_v55.py "$REPO" make_v55-out.txt )
echo "MAINCO [$(git -C "$REPO" status --porcelain)]"

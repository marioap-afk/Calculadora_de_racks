#!/usr/bin/env bash
# PB-T4e (paquete v5.6), precondicion 6, lo que sigue a la copia principal (r83; la r78, historia: su conductor no leia la lista de filtros de UI) y a la base (r79): el control de las CT (r80 y r81, nuevas; si su registro ya tiene las dos lineas del control, no se repiten: una copia no se reutiliza), los
# RequiredTests heredados del contrato emitido de PB-T4d, la cobertura (la principal y la opcional de CA-T4e M-02), los RequiredTests y los
# totales, el resumen y, como ULTIMO paso y una sola vez (CA-T4e M-04), el armado de la v5.6: este registro nombra la version que se emite.
# Antes guarda, sin cambio, el resumen y el registro de la v5.5 (pbt4e-precondition6-v55.json y compile-check-v55.md).
# Uso (desde pbt4ecc/compile/): bash post_p6_v56.sh
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
REPO='<checkout-principal>'
cd "$S"
test -e pbt4e-precondition6-v55.json || cp -p pbt4e-precondition6.json pbt4e-precondition6-v55.json
test -e compile-check-v55.md || cp -p compile-check.md compile-check-v55.md
echo "v5.5 guardada: $(sha256sum pbt4e-precondition6-v55.json | cut -c1-64) pbt4e-precondition6-v55.json; $(sha256sum compile-check-v55.md | cut -c1-64) compile-check-v55.md"
control() { # $1 copia (r80 o r81), $2 modo; una copia no se reutiliza: si el registro ya tiene las dos lineas del control, no se repite
  if test "$(grep -c -E '^control-(core|ui) rc=' "driver-$1-ct.txt" 2>/dev/null)" = 2; then
    echo "$1 ya corrido, no se repite: $(sha256sum "driver-$1-ct.txt" | cut -c1-64) driver-$1-ct.txt"
  else
    bash p6_control_v56.sh "/d/$1" "$2" 2>&1 | tr -d '\r' > "driver-$1-ct.txt"
  fi
  echo "$1 $(grep -E '^control-(core|ui)' "driver-$1-ct.txt" | tr '\n' ' ')"
}
control r80 campo
control r81 anfitrion
python -I req_pbt4d_v55.py "$REPO" 160e019d required-pbt4d-emitted.tsv
BUI=(r79-bc/base-ui-cov.coverage.json r79-bc/base-ui-cov-r-1.coverage.json r79-bc/base-ui-cov-r-2.coverage.json)
for f in r79-bc/base-uif*.coverage.json; do BUI+=("$f"); done
GUI=(r83-p6/ui-green-cov.coverage.json r83-p6/ui-green-cov-r-1.coverage.json r83-p6/ui-green-cov-r-2.coverage.json)
for f in r83-p6/green-ui*.coverage.json; do GUI+=("$f"); done
args=(BASE_CORE=r79-bc/base-core-cov.coverage.json GREEN_CORE=r83-p6/core-green-cov.coverage.json)
for f in "${BUI[@]}"; do args+=("BASE_UI=$f"); done
for f in "${GUI[@]}"; do args+=("GREEN_UI=$f"); done
echo "cobertura: base UI ${#BUI[@]} corridas, GREEN UI ${#GUI[@]} corridas"
python -I cov_p6_pbt4e_v56.py "$REPO" ../../pbt4/pbt4e-green-v56.txt cov-p6-v56.txt "${args[@]}"
# la opcional (CA-T4e M-02): el GREEN sin las dos clases nuevas (Core con FullyQualifiedName!~PushBackEditAllReload; la union de los 18 filtros
# heredados, tres veces, y cada uno de esos 18 en su proceso) frente a la base (Core y los 18 filtros heredados)
xargs_=(BASE_CORE=r79-bc/base-core-cov.coverage.json GREEN_CORE=r83-p6/core-green-cov-sin.coverage.json MODO=existentes)
for f in "${BUI[@]}"; do case "$f" in *base-uif19*) ;; *) xargs_+=("BASE_UI=$f");; esac; done
for f in r83-p6/ui-green-cov18.coverage.json r83-p6/ui-green-cov18-r-1.coverage.json r83-p6/ui-green-cov18-r-2.coverage.json; do xargs_+=("GREEN_UI=$f"); done
for f in r83-p6/green-ui*.coverage.json; do case "$f" in *green-ui19*) ;; *) xargs_+=("GREEN_UI=$f");; esac; done
python -I cov_p6_pbt4e_v56.py "$REPO" ../../pbt4/pbt4e-green-v56.txt cov-p6-existentes-v56.txt "${xargs_[@]}"
rargs=(BASE_CORE=r79-bc/base-core-cov.trx RED_CORE=r83-p6/core-red.trx GREEN_CORE=r83-p6/core-green.trx)
for f in r79-bc/base-uif*.trx; do rargs+=("BASE_UI=$f"); done
for f in r83-p6/red-ui*.trx; do rargs+=("RED_UI=$f"); done
for f in r83-p6/green-ui*.trx; do rargs+=("GREEN_UI=$f"); done
python -I required_pbt4e_v56.py required-pbt4d-emitted.tsv required-pbt4e-v56.tsv required-pbt4e-v56.txt "${rargs[@]}"
python -I p6_json_pbt4e_v56.py "$REPO"
( cd ../../pbt4/v56 && python -I make_v56.py "$REPO" make_v56-out.txt )
echo "la version que se emite: $(sha256sum ../../pbt4/I-64-f4b-pbt4-gate-contract-package-v56.md | cut -c1-64) I-64-f4b-pbt4-gate-contract-package-v56.md"
echo "MAINCO [$(git -C "$REPO" status --porcelain)]"

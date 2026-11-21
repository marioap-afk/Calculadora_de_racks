#!/usr/bin/env bash
# PB-T4c (paquete v5.1), el control negativo del paso 8: el ensayo de las CT de texto (pbt4c-rehearsal-textct.cs, el mismo de la v5.1) sobre las
# piezas de la v5.0 (apply_red_pbt4c_v50.py y apply_green_pbt4c_v50.py, con #nullable enable en los dos archivos de Draft/). Si el paso nuevo ve
# CA-T4c I-01, CT-15 cae por su Assert.Equal en las dos ramas, con la pureza nombrando #nullable; las otras cuatro dan lo mismo que con la v5.1.
# En una copia EXPORTADA nueva de 6adff90d, fuera de todo worktree; nada en el checkout principal salvo git archive; solo el proyecto de Core en
# Debug y solo el filtro del ensayo. Salida corta: <copia>-cc/.
# Uso: bash p6_control50_v51.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=6adff90d
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-cc"; test -e "$OUT" && { echo "la salida ya existe"; exit 2; }; mkdir -p "$OUT"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
echo "control del ensayo sobre la v5.0; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PB4/apply_red_pbt4c_v50.py" "$PB4/apply_green_pbt4c_v50.py" "$PB4/green_pbt4c_v50.py" "$PB4/pbt4c-green-v50.txt" "$PB4/pbt4c-skeleton-v50.txt" \
         "$PB4/pbt4c-rehearsal-textct.cs" "$S/control_check_v51.py" "$S/p6_check_pbt4c_v50b.py" "$0"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
run() { # $1 rama
  ( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/cc-$1-core.log" 2>&1
  local rc=$?; lf "$OUT/cc-$1-core.log"
  echo "cc-$1-core rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/cc-$1-core.log") avisos-CS=$(grep -c 'warning CS' "$OUT/cc-$1-core.log")"
  ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~RackCad.Tests.TmpPbt4cTextCt" --logger "trx;LogFileName=cc-$1.trx" --results-directory "$C/_trx" ) > "$OUT/cc-$1.log" 2>&1
  lf "$OUT/cc-$1.log"
  python -I "$S/p6_check_pbt4c_v50b.py" summary "$C/_trx/cc-$1.trx" "$OUT/cc-$1-res.txt"
  echo "test cc-$1.trx trx-sha256=$(sha256sum "$C/_trx/cc-$1.trx" | cut -d' ' -f1) :: $(head -1 "$OUT/cc-$1-res.txt")"
  python -I "$S/control_check_v51.py" "$1" "$C/_trx/cc-$1.trx" "$OUT/control-$1.txt"
}
python -I "$PB4/apply_red_pbt4c_v50.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; exit 6; }
tail -1 "$OUT/apply-red.txt"
cp "$PB4/pbt4c-rehearsal-textct.cs" "$C/tests/RackCad.Tests/TmpPbt4cTextCt.cs"
run red
python -I "$PB4/apply_green_pbt4c_v50.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; exit 7; }
tail -1 "$OUT/apply-green.txt"
run green
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

#!/usr/bin/env bash
# Compilacion de comprobacion de PB-T4a, segunda version (tras CA-T4a I-01 y M-03), sobre una copia EXPORTADA nueva del cierre de PB-T3c.
# Fase "skel": aplica el parche del esqueleto. Fase "green": aplica el parche y ADEMAS escribe el src/ del GREEN literal que prescribe el paquete
# (apply_green_pbt4a.py; recomendacion de M-01 de CA-T4a). En cada fase: compila Core-tests y UI-tests en Debug y el Plugin en Release con -o,
# cuenta los errores y los avisos CS (lo que rompe build-plugin en la CI: cualquier "warning CS"), control positivo de los tipos nuevos, la suite
# completa de Core con TRX y los filtros de UI del contrato, uno a uno (INV-10: nunca la suite completa de UI en local).
# Uso: bash compile_check_v2.sh <skel|green> <sha-base> <dir-copia-corta, que no exista> <patch> <contrato.json> [<apply_green_pbt4a.py>]
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PHASE="$1"; BASE="$2"; C="$3"; PATCH="$4"; CONTRACT="$5"; GREENPY="${6:-}"
MAINCO='<checkout-principal>'
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
OUT="$S/out-$PHASE"; mkdir -p "$OUT"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
test -f "$PATCH" && test -f "$CONTRACT" || { echo "sin el parche o sin el contrato"; exit 2; }
test "$PHASE" = skel || test -f "$GREENPY" || { echo "la fase green necesita apply_green_pbt4a.py"; exit 2; }
echo "fase $PHASE; patch sha256 $(sha256sum "$PATCH" | cut -d' ' -f1); contrato sha256 $(sha256sum "$CONTRACT" | cut -d' ' -f1)"
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$BASE" | tar -x -C "$C" || exit 3
cd "$C" || exit 4
if test "$PHASE" = skel; then
  git apply --check "$PATCH" && git apply "$PATCH" || { echo "el patch no aplica"; exit 5; }
  echo "esqueleto aplicado: $(grep -c '^+++ b/' "$PATCH") archivos, $(grep -c '^new file mode' "$PATCH") nuevos"
else
  # en la fase green solo la parte de tests/ del parche (el Skip de CT-PBT3A-12): apply_green_pbt4a.py exige el src/ de la revision y escribe el GREEN
  git apply --check --include='tests/*' "$PATCH" && git apply --include='tests/*' "$PATCH" || { echo "la parte de tests/ del patch no aplica"; exit 5; }
  echo "de la parte de tests/ del parche: $(grep '^+++ b/tests/' "$PATCH" | wc -l) archivo(s)"
fi
if test "$PHASE" = green; then
  echo "apply_green sha256 $(sha256sum "$GREENPY" | cut -d' ' -f1)"
  python -I "$GREENPY" "$C" --repo "$MAINCO" --rev "$BASE" > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; cat "$OUT/apply-green.txt"; exit 6; }
  cat "$OUT/apply-green.txt"
fi
WARN_TOTAL=0
build() { # $1 etiqueta, $2 proyecto, $3 configuracion, $4 opcional: carpeta de salida (-o)
  "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} > "$OUT/$1.log" 2>&1
  local rc=$? e w
  e=$(grep -c ' error ' "$OUT/$1.log"); w=$(grep -c 'warning CS' "$OUT/$1.log")
  WARN_TOTAL=$((WARN_TOTAL + w))
  echo "$1 rc=$rc errores=$e avisos-CS=$w"
  test "$w" = 0 || grep -o 'warning CS[0-9]*' "$OUT/$1.log" | sort | uniq -c
}
build core-tests tests/RackCad.Tests/RackCad.Tests.csproj Debug
build ui-tests tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
build plugin src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out"
echo "avisos CS en total: $WARN_TOTAL (tienen que ser 0: build-plugin de la CI falla con cualquier warning CS)"
DLL="$C/tests/RackCad.Tests/bin/Debug/net8.0/RackCad.Application.dll"
for f in $(grep -A3 '^new file mode' "$PATCH" | sed -n 's#^+++ b/src/##p'); do
  n=$(basename "$f" .cs); echo "control positivo: $n=$(grep -c -a "$n" "$DLL" 2>/dev/null) en RackCad.Application.dll"
done
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=core.trx" --results-directory "$C/_trx" > "$OUT/core-test.log" 2>&1
echo "core-test rc=$?"
cp "$C/_trx/core.trx" "$OUT/core.trx" 2>/dev/null
python -I "$S/trx_outcomes.py" "$OUT/core.trx" "$OUT/trx-outcomes.txt"; head -1 "$OUT/trx-outcomes.txt"
i=0
python -I "$S/ui_filters.py" "$CONTRACT" > "$OUT/ui-filters.txt"
while read -r flt; do
  flt="${flt%$'\r'}"  # la salida de Python en Windows lleva CRLF: sin esto el filtro y el registro llevan un CR suelto
  test -n "$flt" || continue
  i=$((i + 1))
  "$DN" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --no-build --filter "$flt" --logger "trx;LogFileName=ui-$i.trx" --results-directory "$C/_trx" > "$OUT/ui-$i.log" 2>&1
  rc=$?
  python -I "$S/trx_outcomes.py" "$C/_trx/ui-$i.trx" "$OUT/ui-$i-outcomes.txt"
  echo "ui $i rc=$rc $flt :: $(head -1 "$OUT/ui-$i-outcomes.txt")"
done < "$OUT/ui-filters.txt"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

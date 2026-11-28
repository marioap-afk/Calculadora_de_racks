#!/usr/bin/env bash
# Compilacion de comprobacion del esqueleto de PB-T4a (precondicion 6 del paquete de PB-T4; F4B-E45) sobre una copia EXPORTADA nueva del
# cierre de PB-T3c: aplica pbt4a-skeleton.patch (el esqueleto inerte de Application y el Skip de CT-PBT3A-12); compila Core-tests y UI-tests en
# Debug y el Plugin en Release con -o; control positivo: los tipos de los archivos nuevos del parche estan en RackCad.Application.dll; y EJECUTA
# la suite completa de Core con TRX. Lo esperado (v4.2): Core 19 129 resultados = 19 122 pasadas + 7 omitidas (las 6 de D-1 y CT-PBT3A-12).
# Uso: bash compile_check.sh <sha-base> <dir-copia-corta, que no exista> <patch>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
BASE="$1"; C="$2"; PATCH="$3"
MAINCO='<checkout-principal>'
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
OUT="$S/out"; mkdir -p "$OUT"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
test -f "$PATCH" || { echo "sin el parche"; exit 2; }
echo "patch sha256 $(sha256sum "$PATCH" | cut -d' ' -f1)"
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$BASE" | tar -x -C "$C" || exit 3
cd "$C" || exit 4
for f in $(grep -A3 '^new file mode' "$PATCH" | sed -n 's#^+++ b/##p'); do
  test -e "$f" && { echo "el archivo nuevo $f ya existe en la base"; exit 5; }
done
git apply --check "$PATCH" && git apply "$PATCH" || { echo "el patch no aplica"; exit 5; }
echo "esqueleto aplicado: $(grep -c '^+++ b/' "$PATCH") archivos, $(grep -c '^new file mode' "$PATCH") nuevos"
build() { # $1 etiqueta, $2 proyecto, $3 configuracion, $4 opcional: carpeta de salida (-o)
  "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} > "$OUT/$1.log" 2>&1
  echo "$1 rc=$? errores=$(grep -c ' error ' "$OUT/$1.log")"
}
build core-tests tests/RackCad.Tests/RackCad.Tests.csproj Debug
build ui-tests tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
build plugin src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out"
DLL="$C/tests/RackCad.Tests/bin/Debug/net8.0/RackCad.Application.dll"
if test -f "$DLL"; then
  for f in $(grep -A3 '^new file mode' "$PATCH" | sed -n 's#^+++ b/src/##p'); do
    n=$(basename "$f" .cs); echo "control positivo: $n=$(grep -c -a "$n" "$DLL") en RackCad.Application.dll"
  done
else
  echo "control positivo: sin $DLL"
fi
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=core.trx" --results-directory "$C/_trx" > "$OUT/core-test.log" 2>&1
echo "core-test rc=$?"
cp "$C/_trx/core.trx" "$OUT/core.trx" 2>/dev/null
python -I "$S/trx_outcomes.py" "$OUT/core.trx" "$S/trx-outcomes.txt"; head -1 "$S/trx-outcomes.txt"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

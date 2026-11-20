#!/usr/bin/env bash
# Segunda comprobacion del esqueleto de PB-T3b (F4B-E45, barrido ampliado de la v4.6) sobre una copia EXPORTADA nueva de la base:
# esqueleto del intento 1 + los TRES nombres (PrepareFull, ExecuteFull, ComparePostFull); compila Core-tests, UI-tests (solo compilar)
# y Plugin; y EJECUTA la suite completa de Core con TRX. Sin los Skip de D-1 (no estan en el parche), se espera que solo fallen
# los metodos S-1..S-6 de D-1; cualquier otro fallo de una prueba existente es un choque que el paquete no ha resuelto.
# Uso: bash compile_check3.sh <sha-base> <dir-copia-corta, que no exista>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
BASE="$1"; C="$2"
MAINCO='<checkout-principal>'
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
OUT="$S/out3"; mkdir -p "$OUT"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$BASE" | tar -x -C "$C" || exit 3
cd "$C" || exit 4
git apply --check "$S/../attempt1/blocked-skeleton.patch" && git apply "$S/../attempt1/blocked-skeleton.patch" || { echo "el patch no aplica"; exit 5; }
python -I "$S/rename_overloads3.py" "$C" || exit 6
build() { # $1 etiqueta, $2 proyecto, $3 configuracion, $4 opcional: carpeta de salida (-o)
  "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} > "$OUT/$1.log" 2>&1
  echo "$1 rc=$? errores=$(grep -c ' error ' "$OUT/$1.log")"
}
build core-tests tests/RackCad.Tests/RackCad.Tests.csproj Debug
build ui-tests tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
build plugin src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out"
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=core.trx" --results-directory "$C/_trx" > "$OUT/core-test.log" 2>&1
echo "core-test rc=$?"
cp "$C/_trx/core.trx" "$OUT/core.trx" 2>/dev/null
python -I "$S/trx_failed.py" "$OUT/core.trx"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

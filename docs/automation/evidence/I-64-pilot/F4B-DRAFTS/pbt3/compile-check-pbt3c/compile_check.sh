#!/usr/bin/env bash
# Compilacion de comprobacion del esqueleto de PB-T3c (precondicion 13 de la v4.7; V47-02) sobre una copia EXPORTADA nueva del cierre de PB-T3b:
# aplica pbt3c-skeleton.patch (la fachada del Plugin, inerte); compila Core-tests y UI-tests en Debug y el Plugin en Release con -o; comprueba que
# el tipo y el metodo del esqueleto estan en el ensamblado del Plugin (control positivo: el archivo nuevo se compilo); y EJECUTA la suite completa
# de Core con TRX. Lo esperado (v4.7, precondicion 13): 19 124 resultados = 19 118 pasadas + 6 omitidas, 0 fallidas.
# Uso: bash compile_check.sh <sha-base> <dir-copia-corta, que no exista>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
BASE="$1"; C="$2"
MAINCO='<checkout-principal>'
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
PATCH="$S/../../pbt3/pbt3c-skeleton.patch"
OUT="$S/out"; mkdir -p "$OUT"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
echo "patch sha256 $(sha256sum "$PATCH" | cut -d' ' -f1)"
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$BASE" | tar -x -C "$C" || exit 3
cd "$C" || exit 4
test -e src/RackCad.Plugin/Views/RackViewBlockEraser.cs && { echo "el archivo del esqueleto ya existe en la base"; exit 5; }
git apply --check "$PATCH" && git apply "$PATCH" || { echo "el patch no aplica"; exit 5; }
echo "esqueleto aplicado: $(wc -l < src/RackCad.Plugin/Views/RackViewBlockEraser.cs) lineas"
build() { # $1 etiqueta, $2 proyecto, $3 configuracion, $4 opcional: carpeta de salida (-o)
  "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} > "$OUT/$1.log" 2>&1
  echo "$1 rc=$? errores=$(grep -c ' error ' "$OUT/$1.log") avisos-del-esqueleto=$(grep -c 'RackViewBlockEraser' "$OUT/$1.log")"
}
build core-tests tests/RackCad.Tests/RackCad.Tests.csproj Debug
build ui-tests tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
build plugin src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out"
DLL="$C/_plugin-out/RackCad.Plugin.dll"
if test -f "$DLL"; then
  echo "control positivo: RackViewBlockEraser=$(grep -c -a 'RackViewBlockEraser' "$DLL") EraseInTransaction=$(grep -c -a 'EraseInTransaction' "$DLL") en RackCad.Plugin.dll"
else
  echo "control positivo: sin RackCad.Plugin.dll"
fi
"$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=core.trx" --results-directory "$C/_trx" > "$OUT/core-test.log" 2>&1
echo "core-test rc=$?"
cp "$C/_trx/core.trx" "$OUT/core.trx" 2>/dev/null
python -I "$S/trx_failed.py" "$OUT/core.trx"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

#!/usr/bin/env bash
# Compilacion de comprobacion del esqueleto de PB-T3b (F4B-E45) sobre una copia EXPORTADA de la base (git archive), fuera del worktree.
# Paso A (control negativo): el esqueleto del intento 1 tal cual -> se esperan los 2 CS0121 en WorkspacePilotUpdateRunTests.cs.
# Paso B: el mismo esqueleto con ExecuteFull y ComparePostFull -> se esperan 0 errores en Core, pruebas de Core, UI, pruebas de UI y Plugin.
# Uso: bash compile_check.sh <sha-base> <dir-copia-corta, que no exista>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
BASE="$1"; C="$2"
MAINCO='<checkout-principal>'
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
OUT="$S/out"; mkdir -p "$OUT"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$BASE" | tar -x -C "$C" || exit 3
cd "$C" || exit 4
git apply --check "$S/../attempt1/blocked-skeleton.patch" && git apply "$S/../attempt1/blocked-skeleton.patch" || { echo "el patch no aplica"; exit 5; }
build() { # $1 etiqueta, $2 proyecto, $3 configuracion, $4 opcional: carpeta de salida (-o)
  "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} > "$OUT/$1.log" 2>&1
  local rc=$?
  local errs; errs=$(grep -c ' error ' "$OUT/$1.log")
  echo "$1 rc=$rc errores=$errs"
  grep ' error ' "$OUT/$1.log" | sed -E 's#^.*[\\/](tests|src)[\\/]#\1/#' | sort -u | head -6
}
echo "== A: esqueleto tal cual"
build A-core-tests tests/RackCad.Tests/RackCad.Tests.csproj Debug
echo "== B: con ExecuteFull y ComparePostFull"
python -I "$S/rename_overloads.py" "$C" || exit 6
build B-core-tests tests/RackCad.Tests/RackCad.Tests.csproj Debug
build B-ui-tests tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
build B-plugin src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

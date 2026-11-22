#!/usr/bin/env bash
# PB-T4d (paquete v5.3): un ENSAYO 2 de las piezas (no es la precondicion 6), en una copia EXPORTADA nueva de 6c2820ba, fuera de todo worktree.
# Como t53_trial_v53.sh, sin la compilacion de la base (los IDE de la base se copian de r6v-tr), y al final, en el GREEN, la suite COMPLETA de
# Core y la union de los filtros de UI con cobertura (cov_v53.runsettings), para cov_p6_v53.py frente a la base de r6w-bc:
#   1. git archive de 6c2820ba; solo en la copia, el .editorconfig suma IDE0051 e IDE0052 (miembros privados sin uso o sin leer) como aviso;
#   2. la base: Core y UI compilados con EnforceCodeStyleInBuild (los IDE0051/IDE0052 de la base);
#   3. el RED (apply_red_pbt4d_v53.py con la pieza que se da): lo mismo, y las clases de Core que se dan en el filtro;
#   4. el GREEN (apply_green_pbt4d_v53.py con la pieza que se da): lo mismo.
# Los avisos IDE no son del contrato: solo sirven para ver los ayudantes que la retirada deja sin uso (CA-T4d M-03). Nada en el checkout
# principal salvo git archive. El SDK de usuario, -nodeReuse:false -p:UseSharedCompilation=false; build-server shutdown al final. UTF-8 con LF.
# Uso: bash t53_trial_v53.sh <copia> <pieza-red> <pieza-green> <filtro-core>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"; RED="$2"; GREEN="$3"; FILTER="$4"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-t2"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "ensayo v5.3; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$RED" "$GREEN" "$PB4/red_pbt4d_v53.py" "$PB4/apply_red_pbt4d_v53.py" "$PB4/green_pbt4d_v53.py" "$PB4/apply_green_pbt4d_v53.py" "$0"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
printf '\n[*.cs]\ndotnet_diagnostic.IDE0051.severity = warning\ndotnet_diagnostic.IDE0052.severity = warning\n' >> "$C/.editorconfig"
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
build() { # $1 etiqueta, $2 proyecto
  ( cd "$C" && "$DN" build "$2" -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -p:EnforceCodeStyleInBuild=true -v:minimal ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  grep -E 'warning IDE005[12]' "$OUT/$1.log" | sed -E 's/^.*[\\/](src|tests)[\\/]/\1\//; s/ \[[^]]*\]$//; s/\([0-9]+,[0-9]+\)//' | tr '\\' '/' | sort -u > "$OUT/$1-ide.txt"
  echo "$1 rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log") avisos-CS=$(grep -c 'warning CS' "$OUT/$1.log") IDE0051/52=$(wc -l < "$OUT/$1-ide.txt")"
  grep -E ': error [A-Z]+[0-9]+|warning CS' "$OUT/$1.log" | sed 's/^/    /' | sort -u | head -20
}
core() { # $1 etiqueta
  ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$FILTER" --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  if test -f "$C/_trx/$1/$1.trx"; then
    cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
    python -I "$S/p6d_check_v52.py" "$OUT/$1.trx" "$OUT/$1-res.txt" > /dev/null
    echo "core $1 rc=$rc :: $(head -1 "$OUT/$1-res.txt")"; sed -n '2,30p' "$OUT/$1-res.txt" | cut -c1-300
  else
    echo "core $1 rc=$rc sin TRX"; tail -5 "$OUT/$1.log"
  fi
}
cp "$S/r6v-tr/b-core.log" "$S/r6v-tr/b-ui.log" "$S/r6v-tr/b-core-ide.txt" "$S/r6v-tr/b-ui-ide.txt" "$OUT/"
python -I "$PB4/apply_red_pbt4d_v53.py" "$C" --repo "$MAINCO" --rev $REV --pieza "$RED" > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; tail -5 "$OUT/apply-red.txt"; exit 5; }
tail -1 "$OUT/apply-red.txt"
build r-core tests/RackCad.Tests/RackCad.Tests.csproj
build r-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
core core-red
python -I "$PB4/apply_green_pbt4d_v53.py" "$C" --repo "$MAINCO" --rev $REV --pieza "$GREEN" > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; tail -5 "$OUT/apply-green.txt"; exit 6; }
tail -1 "$OUT/apply-green.txt"
build g-core tests/RackCad.Tests/RackCad.Tests.csproj
build g-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
core core-green
UNION="$(cat "$S/ui_union_v53.txt")"
covrun() { # $1 etiqueta, $2 proyecto, $3 filtro o vacio
  local t0=$(date +%s)
  ( cd "$C" && "$DN" test "$2" -c Debug --no-build ${3:+--filter "$3"} --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" \
      --collect:"XPlat Code Coverage" --settings "$(cygpath -w "$S/cov_v53.runsettings")" ) > "$OUT/$1.log" 2>&1
  local rc=$? t1=$(date +%s); lf "$OUT/$1.log"
  cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
  python -I "$S/p6d_check_v52.py" "$OUT/$1.trx" "$OUT/$1-res.txt" > /dev/null
  echo "cov $1 rc=$rc segundos=$((t1 - t0)) :: $(head -1 "$OUT/$1-res.txt")"; sed -n '2,12p' "$OUT/$1-res.txt" | grep -v '^no pasan: 0' | cut -c1-260
  local j; j=$(find "$C/_trx/$1" -name 'coverage.json' | head -1); test -n "$j" && cp "$j" "$OUT/$1.coverage.json"
  echo "  json=$(test -n "$j" && sha256sum "$OUT/$1.coverage.json" | cut -c1-16)"
}
covrun g-core-cov tests/RackCad.Tests/RackCad.Tests.csproj ""
covrun g-ui-cov tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "$UNION"
for k in core ui; do
  echo "IDE nuevos en el RED ($k):"; comm -13 "$OUT/b-$k-ide.txt" "$OUT/r-$k-ide.txt" | sed 's/^/    /'
  echo "IDE nuevos en el GREEN frente al RED ($k):"; comm -13 "$OUT/r-$k-ide.txt" "$OUT/g-$k-ide.txt" | sed 's/^/    /'
done
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

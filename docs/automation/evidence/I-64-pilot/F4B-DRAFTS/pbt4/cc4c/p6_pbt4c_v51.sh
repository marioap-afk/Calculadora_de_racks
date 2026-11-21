#!/usr/bin/env bash
# PB-T4c, precondicion 6 del paquete de PB-T4 v5.1 (§6.3.1): con las piezas de la v5.1 (CA-T4c I-01 y M-03) y el paso nuevo, el ensayo de las CT
# de texto. Cada fase en una copia EXPORTADA nueva del cierre de PB-T4b (6adff90d), en una carpeta nueva de ruta corta, fuera de todo worktree;
# nada en el checkout principal salvo git archive. El SDK de usuario, con --no-incremental -nodeReuse:false -p:UseSharedCompilation=false; las
# pruebas en Debug y el Plugin en Release con -o; se cuentan los "warning CS" (tienen que ser 0); la suite COMPLETA de Core, y en UI solo los
# filtros del contrato (nunca la suite completa de UI). Al acabar, build-server shutdown. Las salidas, en UTF-8 con LF y con nombres cortos
# (<copia>-rg/ y <copia>-en/; los filtros de UI por su numero, NN, en ui-filtros.txt), para que ninguna ruta pase de 150 caracteres en cc4c/.
# Uso:
#   bash p6_pbt4c_v51.sh redgreen <copia>   pasos 2 a 6: el RED exacto y el GREEN literal, con la suite de Core y los filtros de UI; los Skip y los filtros
#   bash p6_pbt4c_v51.sh ensayo <copia>     pasos 7 y 8: el ensayo de las CT de texto sobre el RED y sobre el GREEN, y las sondas sobre el GREEN
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
PHASE="$1"; C="$2"
MAINCO='<checkout-principal>'
REV=6adff90d
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
CONTRACT_DUMP="$PB4/v50/pbt4b-contract-dump-v50.txt"
UIF="$S/ui_filters_pbt4c_v50b.txt"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
case "$PHASE" in redgreen) SUF=rg;; ensayo) SUF=en;; *) echo "fase desconocida"; exit 2;; esac
OUT="$S/$(basename "$C")-$SUF"; test -e "$OUT" && { echo "la salida $OUT ya existe: no se reutiliza"; exit 2; }; mkdir -p "$OUT"
echo "fase $PHASE; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PB4/green_pbt4c_v51.py" "$PB4/pbt4c-green-v51.txt" "$PB4/pbt4c-skeleton-v51.txt" "$PB4/pbt4c_skips_v50.py" "$PB4/make_equivalents_pbt4c_v50.py" \
         "$PB4/apply_red_pbt4c_v51.py" "$PB4/apply_green_pbt4c_v51.py" "$PB4/check_skips_pbt4c_v50.py" "$PB4/pbt4c-substitution-list-4.tsv" \
         "$PB4/pbt4c-rehearsal-textct.cs" "$PB4/pbt4c-probe-editall.cs" "$PB4/pbt4c-probe-close-ui.cs" "$S/p6_check_pbt4c_v50b.py" "$S/rehearsal_check_v51.py" \
         "$S/filter_overlap_pbt4c_v50b.py" "$S/classes_v51.py" "$S/trx_message_v50.py" "$UIF" "$CONTRACT_DUMP" "$0"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
echo "copia exportada: $(find "$C" -type f | wc -l) archivos"
WARN_TOTAL=0; ERR_TOTAL=0
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
build() { # $1 etiqueta, $2 proyecto, $3 configuracion, $4 opcional: carpeta de salida (-o)
  ( cd "$C" && "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} ) > "$OUT/$1.log" 2>&1
  local rc=$? e w
  lf "$OUT/$1.log"
  e=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log"); w=$(grep -c 'warning CS' "$OUT/$1.log")
  WARN_TOTAL=$((WARN_TOTAL + w)); ERR_TOTAL=$((ERR_TOTAL + e))
  echo "$1 rc=$rc errores=$e avisos-CS=$w"
  test "$w" = 0 || grep -o 'warning CS[0-9]*' "$OUT/$1.log" | sort | uniq -c
  test "$e" = 0 || grep -E ': error [A-Z]+[0-9]+' "$OUT/$1.log" | sed 's/^/    /' | sort -u | head -20
}
build3() { # $1 prefijo
  build "$1-core" tests/RackCad.Tests/RackCad.Tests.csproj Debug
  build "$1-ui" tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
  build "$1-plugin" src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out-$1"
  echo "$1: errores $ERR_TOTAL, avisos CS $WARN_TOTAL (en total, hasta aqui)"
  for d in "$C/tests/RackCad.Tests/bin/Debug/net8.0/RackCad.Application.dll" "$C/tests/RackCad.Tests/bin/Debug/net8.0/RackCad.Tests.dll" \
           "$C/tests/RackCad.UI.Tests/bin/Debug/net8.0-windows/RackCad.UI.Tests.dll" "$C/_plugin-out-$1/RackCad.Plugin.dll"; do
    test -f "$d" && echo "  binario $(sha256sum "$d" | cut -d' ' -f1) ${d#$C/}"
  done
}
halt_on_build() { # $1 etiqueta
  if test "$ERR_TOTAL" != 0 || test "$WARN_TOTAL" != 0; then
    echo "PARA: la compilacion ($1) da errores $ERR_TOTAL y avisos CS $WARN_TOTAL"
    "$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
    echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"
    exit 4
  fi
}
test_run() { # $1 proyecto, $2 configuracion, $3 nombre del trx, $4 opcional: filtro
  local t0=$(date +%s)
  ( cd "$C" && "$DN" test "$1" -c "$2" --no-build ${4:+--filter "$4"} --logger "trx;LogFileName=$3" --results-directory "$C/_trx" ) > "$OUT/${3%.trx}.log" 2>&1
  local rc=$? t1=$(date +%s)
  lf "$OUT/${3%.trx}.log"
  python -I "$S/p6_check_pbt4c_v50b.py" summary "$C/_trx/$3" "$OUT/${3%.trx}-res.txt"
  echo "test $3 rc=$rc segundos=$((t1 - t0)) trx-sha256=$(sha256sum "$C/_trx/$3" | cut -d' ' -f1) :: $(head -1 "$OUT/${3%.trx}-res.txt")"
}
ui_runs() { # $1 prefijo de los trx (r o g)
  UI_TRX=(); local n=0
  while read -r flt; do
    flt="${flt%$'\r'}"; test -n "$flt" || continue
    n=$((n + 1)); local nn=$(printf '%02d' $n)
    echo "$nn $flt" >> "$OUT/ui-filtros-$1.txt"
    test_run tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug "$1-ui-$nn.trx" "$flt"
    UI_TRX+=("$C/_trx/$1-ui-$nn.trx")
  done < "$UIF"
}
red() {
  python -I "$PB4/apply_red_pbt4c_v51.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; tail -20 "$OUT/apply-red.txt"; exit 6; }
  tail -1 "$OUT/apply-red.txt"
}
green() {
  python -I "$PB4/apply_green_pbt4c_v51.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; tail -20 "$OUT/apply-green.txt"; exit 7; }
  tail -1 "$OUT/apply-green.txt"
}
case "$PHASE" in
  redgreen)
    red
    python -I "$PB4/check_skips_pbt4c_v50.py" "$MAINCO" $REV "$C" "$OUT/check-skips.txt"; echo "check_skips rc=$? :: $(tail -1 "$OUT/check-skips.txt")"
    build3 red; halt_on_build red
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug core-red.trx
    python -I "$S/p6_check_pbt4c_v50b.py" core red "$C/_trx/core-red.trx" "$CONTRACT_DUMP" "$OUT/check-core-red.txt"; echo "check core red rc=$? :: $(tail -1 "$OUT/check-core-red.txt")"
    ui_runs r
    python -I "$S/p6_check_pbt4c_v50b.py" ui red "$OUT/check-ui-red.txt" "${UI_TRX[@]}"; echo "check ui red :: $(tail -1 "$OUT/check-ui-red.txt")"
    python -I "$S/classes_v51.py" "$OUT/clases-ui-red.txt" "${UI_TRX[@]}"
    python -I "$S/classes_v51.py" --solo-no-verdes "$OUT/clases-core-red.txt" "$C/_trx/core-red.trx"
    green
    WARN_TOTAL=0; ERR_TOTAL=0
    build3 green; halt_on_build green
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug core-green.trx
    python -I "$S/p6_check_pbt4c_v50b.py" core green "$C/_trx/core-green.trx" "$CONTRACT_DUMP" "$OUT/check-core-green.txt"; echo "check core green rc=$? :: $(tail -1 "$OUT/check-core-green.txt")"
    ui_runs g
    python -I "$S/p6_check_pbt4c_v50b.py" ui green "$OUT/check-ui-green.txt" "${UI_TRX[@]}"; echo "check ui green :: $(tail -1 "$OUT/check-ui-green.txt")"
    python -I "$S/classes_v51.py" "$OUT/clases-ui-green.txt" "${UI_TRX[@]}"
    python -I "$S/classes_v51.py" --solo-no-verdes "$OUT/clases-core-green.txt" "$C/_trx/core-green.trx"
    python -I "$S/classes_v51.py" "$OUT/clases-core-green-todas.txt" "$C/_trx/core-green.trx"
    python -I "$S/filter_overlap_pbt4c_v50b.py" "$CONTRACT_DUMP" "$OUT/filtros.txt" "$C/_trx/core-green.trx" "${UI_TRX[@]}"; echo "filtros :: $(tail -1 "$OUT/filtros.txt")"
    ;;
  ensayo)
    red
    cp "$PB4/pbt4c-rehearsal-textct.cs" "$C/tests/RackCad.Tests/TmpPbt4cTextCt.cs"; echo "ensayo $(sha256sum "$PB4/pbt4c-rehearsal-textct.cs" | cut -d' ' -f1) -> tests/RackCad.Tests"
    build en-red-core tests/RackCad.Tests/RackCad.Tests.csproj Debug; halt_on_build en-red
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug en-red.trx "FullyQualifiedName~RackCad.Tests.TmpPbt4cTextCt"
    python -I "$S/rehearsal_check_v51.py" red "$C/_trx/en-red.trx" "$OUT/ensayo-red.txt"
    green
    cp "$PB4/pbt4c-probe-editall.cs" "$C/tests/RackCad.Tests/TmpEditall.cs"; cp "$PB4/pbt4c-probe-close-ui.cs" "$C/tests/RackCad.UI.Tests/TmpCloseui.cs"
    echo "sondas $(sha256sum "$PB4/pbt4c-probe-editall.cs" | cut -d' ' -f1) y $(sha256sum "$PB4/pbt4c-probe-close-ui.cs" | cut -d' ' -f1)"
    WARN_TOTAL=0; ERR_TOTAL=0
    build en-green-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
    build en-green-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
    halt_on_build en-green
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug en-green.trx "FullyQualifiedName~RackCad.Tests.TmpPbt4cTextCt"
    python -I "$S/rehearsal_check_v51.py" green "$C/_trx/en-green.trx" "$OUT/ensayo-green.txt"
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug sonda-core.trx "FullyQualifiedName~RackCad.Tests.TmpPbt4cEditAllProbe"
    test_run tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug sonda-ui.trx "FullyQualifiedName~RackCad.UI.Tests.TmpPbt4c"
    python -I "$S/trx_message_v50.py" "$C/_trx/sonda-core.trx" "$OUT/sonda-core-lineas.txt"
    python -I "$S/trx_message_v50.py" "$C/_trx/sonda-ui.trx" "$OUT/sonda-ui-lineas.txt"
    python -I "$S/classes_v51.py" "$OUT/clases-ensayo.txt" "$C/_trx/en-red.trx" "$C/_trx/en-green.trx" "$C/_trx/sonda-core.trx" "$C/_trx/sonda-ui.trx"
    ;;
esac
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

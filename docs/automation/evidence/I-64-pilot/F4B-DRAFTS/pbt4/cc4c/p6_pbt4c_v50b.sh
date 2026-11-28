#!/usr/bin/env bash
# PB-T4c, precondicion 6 del paquete de PB-T4 v5.0 (§6.3.1), con la lista de 4 (owner-pbt4c-list-4-decision.md) y D-S9: el de la primera vuelta (p6_pbt4c_v50.sh, que no se toca) con p6_check_pbt4c_v50b.py, ui_filters_pbt4c_v50b.txt y las sondas de Core y de UI. Cada fase en una copia EXPORTADA nueva del cierre de PB-T4b (6adff90d), en una
# carpeta nueva de ruta corta, fuera de todo worktree; nada en el checkout principal salvo git archive. El SDK de usuario, con --no-incremental
# -nodeReuse:false -p:UseSharedCompilation=false; las pruebas en Debug y el Plugin en Release con -o; se cuentan los "warning CS" (tienen que ser
# 0); la suite COMPLETA de Core, y en UI solo los filtros del contrato (nunca la suite completa de UI). Al acabar, build-server shutdown. Las
# salidas, en UTF-8 con LF.
# Uso:
#   bash p6_pbt4c_v50.sh compile <copia>    el RED y el GREEN del paquete, solo compilados (diagnostico)
#   bash p6_pbt4c_v50.sh redgreen <copia>   pasos 2 a 6: el RED exacto y el GREEN literal, con la suite de Core y los filtros de UI
#   bash p6_pbt4c_v50.sh probes <copia>     paso 7: las sondas de las lineas de las CT sobre el GREEN, solo con su filtro
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
PHASE="$1"; C="$2"
MAINCO='<checkout-principal>'
REV=6adff90d
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
CONTRACT_DUMP="$PB4/v50/pbt4b-contract-dump-v50.txt"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/out-$PHASE-v50b-$(basename "$C")"; test -e "$OUT" && { echo "la salida $OUT ya existe: no se reutiliza"; exit 2; }; mkdir -p "$OUT"
echo "fase $PHASE; copia $C; salida out-$PHASE-v50b-$(basename "$C"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PB4/green_pbt4c_v50.py" "$PB4/pbt4c-green-v50.txt" "$PB4/pbt4c-skeleton-v50.txt" "$PB4/pbt4c_skips_v50.py" "$PB4/make_equivalents_pbt4c_v50.py" \
         "$PB4/apply_red_pbt4c_v50.py" "$PB4/apply_green_pbt4c_v50.py" "$PB4/pbt4c-substitution-list-4.tsv" "$PB4/pbt4c-probe-editall.cs" "$PB4/pbt4c-probe-close-ui.cs" "$S/p6_check_pbt4c_v50b.py" "$S/ui_filters_pbt4c_v50b.txt" "$CONTRACT_DUMP" "$0"; do
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
  build "$1-core-tests" tests/RackCad.Tests/RackCad.Tests.csproj Debug
  build "$1-ui-tests" tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
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
  python -I "$S/p6_check_pbt4c_v50b.py" summary "$C/_trx/$3" "$OUT/${3%.trx}-outcomes.txt"
  echo "test $3 rc=$rc segundos=$((t1 - t0)) trx-sha256=$(sha256sum "$C/_trx/$3" | cut -d' ' -f1) :: $(head -1 "$OUT/${3%.trx}-outcomes.txt")"
}
slug() { echo "$1" | sed 's/^FullyQualifiedName~//; s/[^A-Za-z0-9]/_/g'; }
ui_runs() { # $1 prefijo de los trx
  UI_TRX=()
  while read -r flt; do
    flt="${flt%$'\r'}"; test -n "$flt" || continue
    local n="$1-ui-$(slug "$flt").trx"
    test_run tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug "$n" "$flt"
    UI_TRX+=("$C/_trx/$n")
  done < "$S/ui_filters_pbt4c_v50b.txt"
}
red() {
  python -I "$PB4/apply_red_pbt4c_v50.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; tail -20 "$OUT/apply-red.txt"; exit 6; }
  tail -1 "$OUT/apply-red.txt"
}
green() {
  python -I "$PB4/apply_green_pbt4c_v50.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; tail -20 "$OUT/apply-green.txt"; exit 7; }
  tail -1 "$OUT/apply-green.txt"
}
case "$PHASE" in
  compile)
    red; build3 red
    WARN_TOTAL=0; ERR_TOTAL=0
    green; build3 green
    ;;
  redgreen)
    red; build3 red; halt_on_build red
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug core-red.trx
    python -I "$S/p6_check_pbt4c_v50b.py" core red "$C/_trx/core-red.trx" "$CONTRACT_DUMP" "$OUT/check-core-red.txt"; echo "check core red rc=$? :: $(tail -1 "$OUT/check-core-red.txt")"
    ui_runs red
    python -I "$S/p6_check_pbt4c_v50b.py" ui red "$OUT/check-ui-red.txt" "${UI_TRX[@]}"; echo "check ui red :: $(tail -1 "$OUT/check-ui-red.txt")"
    green
    WARN_TOTAL=0; ERR_TOTAL=0
    build3 green; halt_on_build green
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug core-green.trx
    python -I "$S/p6_check_pbt4c_v50b.py" core green "$C/_trx/core-green.trx" "$CONTRACT_DUMP" "$OUT/check-core-green.txt"; echo "check core green rc=$? :: $(tail -1 "$OUT/check-core-green.txt")"
    ui_runs green
    python -I "$S/p6_check_pbt4c_v50b.py" ui green "$OUT/check-ui-green.txt" "${UI_TRX[@]}"; echo "check ui green :: $(tail -1 "$OUT/check-ui-green.txt")"
    ;;
  probes)
    red; green
    # Las sondas de Core en tests/RackCad.Tests/ y las de UI (*-ui.cs) en tests/RackCad.UI.Tests/; cada una solo con su filtro.
    for f in "$PB4"/pbt4c-probe-*.cs; do
      case "$f" in *-ui.cs) d=tests/RackCad.UI.Tests;; *) d=tests/RackCad.Tests;; esac
      cp "$f" "$C/$d/Tmp$(basename "$f" .cs | sed 's/^pbt4c-probe-//; s/-//g').cs"; echo "sonda $(sha256sum "$f" | cut -d' ' -f1) ${f##*/} -> $d"
    done
    build probes-core-tests tests/RackCad.Tests/RackCad.Tests.csproj Debug
    build probes-ui-tests tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
    echo "probes: errores $ERR_TOTAL, avisos CS $WARN_TOTAL"; halt_on_build probes
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug probes-core.trx "FullyQualifiedName~RackCad.Tests.TmpPbt4c"
    test_run tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug probes-ui.trx "FullyQualifiedName~RackCad.UI.Tests.TmpPbt4c"
    python -I "$S/trx_message_v50.py" "$C/_trx/probes-core.trx" "$OUT/probe-core-lines.txt"
    python -I "$S/trx_message_v50.py" "$C/_trx/probes-ui.trx" "$OUT/probe-ui-lines.txt"
    ;;
  *) echo "fase desconocida"; exit 2;;
esac
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"; lf "$OUT/build-server-shutdown.txt"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

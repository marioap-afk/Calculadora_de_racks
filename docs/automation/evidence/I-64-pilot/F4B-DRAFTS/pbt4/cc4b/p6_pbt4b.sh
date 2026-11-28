#!/usr/bin/env bash
# PB-T4b, precondicion 6 del paquete de PB-T4 v4.5 (§6.2.1, los ocho pasos), en nombre de la sesion. Cada fase en una copia EXPORTADA nueva del
# cierre de PB-T4a (78a7cef3), en una carpeta nueva de ruta corta, fuera de todo worktree; nada en el checkout principal salvo git archive.
# El SDK de usuario, con --no-incremental -nodeReuse:false -p:UseSharedCompilation=false; las pruebas en Debug y el Plugin en Release con -o; se
# cuentan los "warning CS" (tienen que ser 0); la suite completa de Core, y en UI solo los filtros del contrato (nunca la suite completa de UI).
# Al acabar cada fase, build-server shutdown.
# Uso:
#   bash p6_pbt4b.sh probe <copia>                 pasos 1 a 4: el RED en modo de medida, las TRX de las copias y la particion
#   bash p6_pbt4b.sh redgreen <copia> <particion>   pasos 5 y 6: el RED exacto y el GREEN literal, con la suite de Core y los filtros de UI
#   bash p6_pbt4b.sh probes <copia> <particion>     paso 7: las dos sondas (y la de IsDirty) sobre el GREEN, en Release, solo con su filtro
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
PHASE="$1"; C="$2"; PART="${3:-}"
MAINCO='<checkout-principal>'
REV=78a7cef3
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
PKG="$PB4/I-64-f4b-pbt4-gate-contract-package-v45.md"
OUT="$S/out-$PHASE"; mkdir -p "$OUT"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
test "$PHASE" = probe || test -f "$PART" || { echo "la fase $PHASE necesita la particion"; exit 2; }
echo "fase $PHASE; copia $C; revision $(git -C "$MAINCO" rev-parse $REV); paquete sha256 $(sha256sum "$PKG" | cut -d' ' -f1)"
test -n "$PART" && echo "particion sha256 $(sha256sum "$PART" | cut -d' ' -f1)"
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
echo "copia exportada: $(find "$C" -type f | wc -l) archivos"
WARN_TOTAL=0; ERR_TOTAL=0
build() { # $1 etiqueta, $2 proyecto, $3 configuracion, $4 opcional: carpeta de salida (-o)
  ( cd "$C" && "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} ) > "$OUT/$1.log" 2>&1
  local rc=$? e w
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
test_run() { # $1 proyecto, $2 configuracion, $3 nombre del trx, $4 opcional: filtro
  local t0=$(date +%s)
  ( cd "$C" && "$DN" test "$1" -c "$2" --no-build ${4:+--filter "$4"} --logger "trx;LogFileName=$3" --results-directory "$C/_trx" ) > "$OUT/${3%.trx}.log" 2>&1
  local rc=$? t1=$(date +%s)
  python -I "$S/p6_check.py" summary "$C/_trx/$3" "$OUT/${3%.trx}-outcomes.txt" > /dev/null
  echo "test $3 rc=$rc segundos=$((t1 - t0)) trx-sha256=$(sha256sum "$C/_trx/$3" | cut -d' ' -f1) :: $(head -1 "$OUT/${3%.trx}-outcomes.txt")"
}
slug() { echo "$1" | sed 's/^FullyQualifiedName~//; s/[^A-Za-z0-9]/_/g'; }
ui_runs() { # $1 prefijo de los trx
  python -I "$S/ui_filters_p6.py" "$PKG" "$OUT/ui-filters.txt" > /dev/null
  UI_TRX=()
  while read -r flt; do
    flt="${flt%$'\r'}"; test -n "$flt" || continue
    local n="$1-ui-$(slug "$flt").trx"
    test_run tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug "$n" "$flt"
    UI_TRX+=("$C/_trx/$n")
  done < "$OUT/ui-filters.txt"
}
case "$PHASE" in
  probe)
    python -I "$PB4/apply_red_pbt4b_v45.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red-probe.txt" 2>&1 || { echo "apply_red fallo"; tail -20 "$OUT/apply-red-probe.txt"; exit 6; }
    tail -1 "$OUT/apply-red-probe.txt"
    build3 probe
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug core-probe.trx "FullyQualifiedName~RackCad.Tests.FullPanelCopies"
    test_run tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug ui-probe.trx "FullyQualifiedName~RackCad.UI.Tests.FullPanelCopies"
    python -I "$PB4/partition_pbt4b_v45.py" red "$C/_trx/core-probe.trx" "$C/_trx/ui-probe.trx" "$OUT/pbt4b-copy-partition.tsv" > "$OUT/partition-red.txt" 2>&1
    echo "partition red rc=$?"; cat "$OUT/partition-red.txt"
    ;;
  redgreen)
    python -I "$PB4/apply_red_pbt4b_v45.py" "$C" --repo "$MAINCO" --rev $REV --partition "$PART" > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; tail -20 "$OUT/apply-red.txt"; exit 6; }
    tail -1 "$OUT/apply-red.txt"
    python -I "$S/check_skips_copy_v45.py" "$MAINCO" $REV "$C" "$OUT/check-skips-red.txt" > /dev/null; echo "check_skips (RED) rc=$? :: $(tail -1 "$OUT/check-skips-red.txt")"
    build3 red
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug core-red.trx
    ui_runs red
    python -I "$S/p6_check.py" red "$C/_trx/core-red.trx" "$PART" "$OUT/check-red.txt" "${UI_TRX[@]}" > /dev/null; echo "check red rc=$? :: $(tail -1 "$OUT/check-red.txt")"
    python -I "$PB4/apply_green_pbt4b_v45.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; tail -20 "$OUT/apply-green.txt"; exit 7; }
    tail -1 "$OUT/apply-green.txt"
    python -I "$S/check_skips_copy_v45.py" "$MAINCO" $REV "$C" "$OUT/check-skips-green.txt" > /dev/null; echo "check_skips (GREEN) rc=$? :: $(tail -1 "$OUT/check-skips-green.txt")"
    WARN_TOTAL=0; ERR_TOTAL=0
    build3 green
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Debug core-green.trx
    ui_runs green
    python -I "$S/p6_check.py" green "$C/_trx/core-green.trx" "$OUT/check-green.txt" "${UI_TRX[@]}" > /dev/null; echo "check green rc=$? :: $(tail -1 "$OUT/check-green.txt")"
    python -I "$PB4/partition_pbt4b_v45.py" green "$C/_trx/core-green.trx" "$C/_trx/green-ui-RackCad_UI_Tests_FullPanelCopies.trx" > "$OUT/partition-green.txt" 2>&1
    echo "partition green rc=$? :: $(head -1 "$OUT/partition-green.txt")"
    ;;
  probes)
    python -I "$PB4/apply_red_pbt4b_v45.py" "$C" --repo "$MAINCO" --rev $REV --partition "$PART" > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; exit 6; }
    python -I "$PB4/apply_green_pbt4b_v45.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; exit 7; }
    tail -1 "$OUT/apply-red.txt"; tail -1 "$OUT/apply-green.txt"
    cp "$PB4/pbt4b-read-cost-probe.cs" "$C/tests/RackCad.Tests/TmpReadCostProbe.cs"
    cp "$PB4/pbt4b-ct06-presentar-probe.cs" "$C/tests/RackCad.Tests/TmpCt06PresentarProbe.cs"
    cp "$S/TmpIsDirtyCostProbe.cs" "$C/tests/RackCad.Tests/TmpIsDirtyCostProbe.cs"
    for f in TmpReadCostProbe.cs TmpCt06PresentarProbe.cs TmpIsDirtyCostProbe.cs; do echo "sonda $(sha256sum "$C/tests/RackCad.Tests/$f" | cut -d' ' -f1) $f"; done
    build probes-core-tests-debug tests/RackCad.Tests/RackCad.Tests.csproj Debug
    build probes-core-tests-release tests/RackCad.Tests/RackCad.Tests.csproj Release
    echo "probes: errores $ERR_TOTAL, avisos CS $WARN_TOTAL"
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Release ct06.trx "FullyQualifiedName~TmpCt06PresentarProbe"
    python -I "$S/p6_check.py" message "$C/_trx/ct06.trx" "$OUT/ct06-probe.txt" > /dev/null
    python -I "$S/p6_check.py" ct06 "$OUT/check-ct06.txt" "$OUT/ct06-probe.txt" > /dev/null; echo "check ct06 rc=$? :: $(tail -1 "$OUT/check-ct06.txt")"
    for run in 1 2; do
      test_run tests/RackCad.Tests/RackCad.Tests.csproj Release "cost-run$run.trx" "FullyQualifiedName~TmpReadCostProbe"
      python -I "$S/p6_check.py" message "$C/_trx/cost-run$run.trx" "$OUT/cost-run$run.txt" > /dev/null
    done
    python -I "$S/p6_check.py" cost "$OUT/check-cost.txt" "$OUT/cost-run1.txt" "$OUT/cost-run2.txt" > /dev/null; echo "check cost rc=$? :: $(tail -1 "$OUT/check-cost.txt")"
    test_run tests/RackCad.Tests/RackCad.Tests.csproj Release isdirty.trx "FullyQualifiedName~TmpIsDirtyCostProbe"
    python -I "$S/p6_check.py" message "$C/_trx/isdirty.trx" "$OUT/isdirty-probe.txt" > /dev/null
    ;;
  *) echo "fase desconocida"; exit 2;;
esac
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

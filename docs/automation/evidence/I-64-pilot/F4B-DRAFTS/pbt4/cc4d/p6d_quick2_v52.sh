#!/usr/bin/env bash
# PB-T4d (paquete v5.2): un ENSAYO rapido de las piezas (no es la precondicion 6): en una copia EXPORTADA nueva de 6c2820ba, el RED de tests/
# (apply_red_pbt4d_v52.py con la pieza que se da) y el GREEN de src/; en cada rama, la compilacion de Core y UI (y del Plugin en el GREEN) con sus
# "warning CS", y solo las clases de Core que el RED edita o crea, filtradas. Nada en el checkout principal salvo git archive. El SDK de usuario;
# build-server shutdown al final. UTF-8 con LF; nombres cortos (<copia>-qk/).
# Version 2 (relevo, tras owner-pbt4d-deadcode-27-decisions.md): el filtro suma las guardas que leen el texto del plan (la rama muerta de
# BuildCore cambia ese archivo): PushBackDraftWriteGuardTests, PushBackFullPanelCensusGuardTests, PushBackFullReadGuard*,
# PushBackWritePathGuardTests y su copia, y CustomPropertiesEnvelopeGuardTests.
# Uso: bash p6d_quick2_v52.sh <copia> <pieza-red>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"; PIECE="$2"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-qk"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "ensayo rapido; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PIECE" "$PB4/red_pbt4d_v52.py" "$PB4/apply_red_pbt4d_v52.py" "$PB4/green_pbt4d_v52.py" "$PB4/pbt4d-green-v52.txt" "$PB4/apply_green_pbt4d_v52.py" "$0"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
build() { # $1 etiqueta, $2 proyecto, $3 configuracion, $4 opcional: -o
  ( cd "$C" && "$DN" build "$2" -c "$3" --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ${4:+-o "$4"} ) > "$OUT/$1.log" 2>&1
  local rc=$?; lf "$OUT/$1.log"
  echo "$1 rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/$1.log") avisos-CS=$(grep -c 'warning CS' "$OUT/$1.log")"
  grep -E ': error [A-Z]+[0-9]+|warning CS' "$OUT/$1.log" | sed 's/^/    /' | sort -u | head -20
}
F='FullyQualifiedName~RackCad.Tests.PushBackFullWritePrepareTests|FullyQualifiedName~RackCad.Tests.PushBackFullWriteRunTests|FullyQualifiedName~RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests|FullyQualifiedName~RackCad.Tests.WorkspaceDraftPilot|FullyQualifiedName~RackCad.Tests.WorkspacePilotEditGuardTests|FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotEditGuard|FullyQualifiedName~RackCad.Tests.WorkspacePilotPrepareGuardTests|FullyQualifiedName~RackCad.Tests.FullPanelCopyWorkspacePilotPrepare|FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateGuardTests|FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests|FullyQualifiedName~RackCad.Tests.PushBackEditAllFullCopyGuardTests|FullyQualifiedName~RackCad.Tests.PushBackRetirePilot|FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateRunTests|FullyQualifiedName~RackCad.Tests.PushBackDraftWriteGuardTests|FullyQualifiedName~RackCad.Tests.PushBackFullPanelCensusGuardTests|FullyQualifiedName~RackCad.Tests.PushBackFullReadGuard|FullyQualifiedName~RackCad.Tests.PushBackWritePathGuardTests|FullyQualifiedName~RackCad.Tests.FullPanelCopyPushBackWritePathGuard|FullyQualifiedName~RackCad.Tests.CustomPropertiesEnvelopeGuardTests'
run() { # $1 etiqueta
  ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$F" --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx" ) > "$OUT/$1.log" 2>&1
  lf "$OUT/$1.log"
  python -I "$S/p6d_check_v52.py" "$C/_trx/$1.trx" "$OUT/$1-res.txt" > /dev/null
  echo "test $1 :: $(head -1 "$OUT/$1-res.txt")"; sed -n '2,40p' "$OUT/$1-res.txt" | cut -c1-260
}
python -I "$PB4/apply_red_pbt4d_v52.py" "$C" --repo "$MAINCO" --rev $REV --pieza "$PIECE" > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; tail -5 "$OUT/apply-red.txt"; exit 5; }
tail -1 "$OUT/apply-red.txt"
build r-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
build r-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
run red
python -I "$PB4/apply_green_pbt4d_v52.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-green.txt" 2>&1 || { echo "apply_green fallo"; tail -5 "$OUT/apply-green.txt"; exit 6; }
tail -1 "$OUT/apply-green.txt"
build g-core tests/RackCad.Tests/RackCad.Tests.csproj Debug
build g-ui tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj Debug
build g-plugin src/RackCad.Plugin/RackCad.Plugin.csproj Release "$C/_plugin-out"
run green
"$DN" build-server shutdown > /dev/null 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

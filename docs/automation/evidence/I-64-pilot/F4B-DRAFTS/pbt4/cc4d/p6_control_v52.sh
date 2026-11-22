#!/usr/bin/env bash
# PB-T4d (paquete v5.2), precondicion 6, el control del ensayo de las CT de texto, en una copia EXPORTADA nueva de 6c2820ba:
#   (a) la base sin el RED, con solo las dos clases de las CT (write_cts_v52.py): CT-PBT4D-01 cae (src/ tiene el piloto) y CT-PBT4D-02 cae
#       (tests/ nombra lo retirado);
#   (b) se quitan las dos clases; el RED literal (apply_red_pbt4d_v52.py) y, en src/, la variante A de la medida (retire_pbt4d_v52.py A con un
#       archivo de bajas vacio: solo src/), una retirada incompleta: CT-PBT4D-01 cae y CT-PBT4D-02 pasa.
# Cada vez, la compilacion de Core (Debug, sin warning CS) y las dos clases por clase exacta. Nada en el checkout principal salvo git archive.
# El SDK de usuario; build-server shutdown al final. UTF-8 con LF; nombres cortos (<copia>-ct/).
# Uso: bash p6_control_v52.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
PB4="$(cd "$S/../../pbt4" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=6c2820ba
DN="<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/$(basename "$C")-ct"; test -e "$OUT" && { echo "la salida $OUT ya existe"; exit 2; }; mkdir -p "$OUT"
echo "control de las CT de PB-T4d; copia $C; salida $(basename "$OUT"); revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$PB4/pbt4d-red-v52.txt" "$PB4/red_pbt4d_v52.py" "$PB4/apply_red_pbt4d_v52.py" "$PB4/v52/retire_pbt4d_v52.py" "$0" "$S/write_cts_v52.py" "$S/p6d_check_v52.py"; do
  echo "  pieza $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"
done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
lf() { tr -d '\r' < "$1" > "$1.lf" && mv "$1.lf" "$1"; }
F='FullyQualifiedName=RackCad.Tests.PushBackRetirePilotGuardTests.CT_PBT4D_01_TheCodeOfThePilotThatThePanelNoLongerUsesIsRetiredAndWhatTheFullModelUsesStays|FullyQualifiedName=RackCad.Tests.PushBackRetirePilotCensusGuardTests.CT_PBT4D_02_NoFileOfTheTestsNamesWhatTheRetirementOfThePilotRemoves'
step() { # $1 etiqueta
  ( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/b-$1.log" 2>&1
  local rc=$?; lf "$OUT/b-$1.log"
  echo "b-$1 rc=$rc errores=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/b-$1.log") avisos-CS=$(grep -c 'warning CS' "$OUT/b-$1.log")"
  ( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --filter "$F" --logger "trx;LogFileName=$1.trx" --results-directory "$C/_trx/$1" ) > "$OUT/$1.log" 2>&1
  lf "$OUT/$1.log"
  cp "$C/_trx/$1/$1.trx" "$OUT/$1.trx"
  python -I "$S/p6d_check_v52.py" "$OUT/$1.trx" "$OUT/$1-res.txt" > /dev/null
  echo "test $1 trx=$(sha256sum "$OUT/$1.trx" | cut -c1-16) :: $(head -1 "$OUT/$1-res.txt")"; sed -n '2,10p' "$OUT/$1-res.txt" | cut -c1-200
}
python -I "$S/write_cts_v52.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/write-cts.txt" 2>&1 || { echo "write_cts fallo"; cat "$OUT/write-cts.txt"; exit 5; }
cat "$OUT/write-cts.txt"
step base-cts
python -I "$S/write_cts_v52.py" "$C" --repo "$MAINCO" --rev $REV --quitar > "$OUT/quitar-cts.txt" 2>&1 || { echo "quitar fallo"; exit 6; }
python -I "$PB4/apply_red_pbt4d_v52.py" "$C" --repo "$MAINCO" --rev $REV > "$OUT/apply-red.txt" 2>&1 || { echo "apply_red fallo"; tail -5 "$OUT/apply-red.txt"; exit 7; }
tail -1 "$OUT/apply-red.txt"
printf 'origen\truta\tmiembro\n' > "$OUT/bajas-vacio.tsv"
python -I "$PB4/v52/retire_pbt4d_v52.py" "$C" A "$OUT/bajas-vacio.tsv" "$OUT/retire-A.txt" --repo "$MAINCO" > "$OUT/retire-A-out.txt" 2>&1 || { echo "retire A fallo"; tail -5 "$OUT/retire-A-out.txt"; exit 8; }
cat "$OUT/retire-A-out.txt"
step red-A
"$DN" build-server shutdown > /dev/null 2>&1; echo "build-server shutdown rc=$?"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

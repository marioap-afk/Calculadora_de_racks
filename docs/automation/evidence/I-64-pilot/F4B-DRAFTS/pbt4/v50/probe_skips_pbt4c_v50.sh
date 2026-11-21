> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

#!/usr/bin/env bash
# PB-T4c (v5.0), fase 2: la sonda de la lista sobre el cierre de PB-T4b (6adff90d), en una copia EXPORTADA nueva, fuera de todo worktree; nada en
# el checkout principal salvo git archive. Solo los dos Skip de la lista (probe_skips_pbt4c_v50.py), sin GREEN ni CT. El SDK de usuario, con
# --no-incremental -nodeReuse:false -p:UseSharedCompilation=false; Core en Debug; se cuentan los "warning CS" (tienen que ser 0); la suite
# COMPLETA de Core (la de UI no se corre: la sonda no toca la UI). Al acabar, build-server shutdown.
# Uso: bash probe_skips_pbt4c_v50.sh <copia>
set -uo pipefail
S="$(cd "$(dirname "$0")" && pwd)"
CC="$(cd "$S/../../pbt4bcc/compile" && pwd)"
C="$1"
MAINCO='<checkout-principal>'
REV=6adff90d
DN="$<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
test -e "$C" && { echo "la copia $C ya existe: no se reutiliza"; exit 2; }
OUT="$S/out-probe-skips-$(basename "$C")"; test -e "$OUT" && { echo "la salida $OUT ya existe: no se reutiliza"; exit 2; }; mkdir -p "$OUT"
echo "sonda de los Skip; copia $C; salida $OUT; revision $(git -C "$MAINCO" rev-parse $REV)"
for f in "$S/probe_skips_pbt4c_v50.py" "$CC/p6_check_v48.py" "$0"; do echo "  guion $(sha256sum "$f" | cut -d' ' -f1) ${f##*/}"; done
mkdir -p "$C"
git -C "$MAINCO" archive --format=tar "$REV" | tar -x -C "$C" || exit 3
echo "copia exportada: $(find "$C" -type f | wc -l) archivos"
python -I "$S/probe_skips_pbt4c_v50.py" "$C" > "$OUT/apply-skips.txt" 2>&1 || { echo "probe_skips fallo"; cat "$OUT/apply-skips.txt"; exit 6; }
cat "$OUT/apply-skips.txt"
( cd "$C" && "$DN" build tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal ) > "$OUT/core-tests.log" 2>&1
rc=$?; e=$(grep -c -E ': error [A-Z]+[0-9]+' "$OUT/core-tests.log"); w=$(grep -c 'warning CS' "$OUT/core-tests.log")
echo "core-tests rc=$rc errores=$e avisos-CS=$w"
if test "$e" != 0 || test "$w" != 0; then
  grep -E ': error [A-Z]+[0-9]+|warning CS' "$OUT/core-tests.log" | sort -u | head -20
  "$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "PARA: la compilacion no es limpia"; exit 4
fi
t0=$(date +%s)
( cd "$C" && "$DN" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-build --logger "trx;LogFileName=core-skips.trx" --results-directory "$C/_trx" ) > "$OUT/core-skips.log" 2>&1
rc=$?; t1=$(date +%s)
python -I "$CC/p6_check_v48.py" summary "$C/_trx/core-skips.trx" "$OUT/core-skips-outcomes.txt" > /dev/null
echo "test core-skips.trx rc=$rc segundos=$((t1 - t0)) trx-sha256=$(sha256sum "$C/_trx/core-skips.trx" | cut -d' ' -f1) :: $(head -1 "$OUT/core-skips-outcomes.txt")"
"$DN" build-server shutdown > "$OUT/build-server-shutdown.txt" 2>&1; echo "build-server shutdown rc=$?"
echo "procesos: $(tasklist 2>/dev/null | grep -i -c -E 'dotnet|VBCSCompiler|testhost')"
echo "MAINCO [$(git -C "$MAINCO" status --porcelain)]"

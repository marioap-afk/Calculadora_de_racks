#!/usr/bin/env bash
# Sonda medida de A4-1 — BORRADOR, NO EJECUTADO (preparación de la supervisión, plano a; clasificación de FX-02, §3 fila P7).
# Decisiones §61.4: «La sonda adicional del Controller no se gasta antes del acuerdo de A-4 y de A4-SONDA-CONSUMO = A».
# Adaptado del ejecutor del bloque A2-P2 (f6/block-a2p2/run_block.sh, decisiones §58.2): las mismas comprobaciones de identidad antes y después
# (huella exacta, estructura saneada, binario, número de codex.exe y versión de la app), nunca valores ni digests por clave.
# UNA sonda: celda del Controller gpt-6-luna/high, shell declarada cmd.exe, -C = el directorio que fije la disposición de aplicación.
# Puertas (README): A-4 AGREED con A4-1; disposición de aplicación (celda, shell, trío y directorio); A4-SONDA-CONSUMO = A; P-07 tope 1; sin reintento.
# Uso:
#   run_probe.sh measure <tag>   -> solo identidad (sin modelo)
#   run_probe.sh probe           -> la única sonda, con las puertas de gates.env
#   run_probe.sh compare         -> después de la sonda: referencia con git y hashlib, y comparación (sin modelo)
# Cualquier diferencia de huella, estructura, binario o versión frente a la identidad autorizada => STOP (exit 3): la medición no vale.
set -u
B="$(cd "$(dirname "$0")" && pwd)"
OUT="$B/out"; mkdir -p "$OUT"; OUTW="$(cygpath -w "$OUT")"; OUTM="$(cygpath -m "$OUT")"; BM="$(cygpath -m "$B")"
EXPECT_FP=6518EFAB0BC0C0C2C2C5DCCD3A3D3646DFDC9F15B222FD6B744CBC84B857DB32
EXPECT_BIN=3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68
EXPECT_APP=26.1002.7124.0
EXPECT_TRIO="$EXPECT_BIN/$EXPECT_APP/$EXPECT_FP"
EXPECT_CELL='gpt-6-luna/high'; MODEL='gpt-6-luna'
EXPECT_SHELL='cmd.exe'
FIXTURE_ORIGIN_URL='D:/r62-fixture/fixture-origin.git'
CODEX="$LOCALAPPDATA/OpenAI/Codex/bin/9691020b546a15b2/codex.exe"
RTPS="$USERPROFILE/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell"
TIMEOUT_S=600                       # AP 16.4: tope de 600 s; A4-1, regla 1: el tope de tiempo de la receta no cambia
LAUNCH_MARKER="$OUT/PROBE_LAUNCHED" # P-07, tope 1: se crea antes de lanzar y nunca se borra (un lanzamiento incierto cuenta como lanzado)

refuse() { echo "REFUSED: $*" | tee -a "$OUT/refusals.log"; exit 2; }

measure() {
  local tag="$1"
  local fp; fp=$(pwsh -NoProfile -File "$BM/cfg-fp.ps1" -Out "$OUTM/$tag-cfg.json" | tr -d '\r')
  local struct; struct=$(python -c "
import json,sys
a=json.load(open(r'$OUTM/$tag-cfg.json',encoding='utf-8-sig'))['KeyNames']; b=json.load(open(r'$BM/baseline-keynames-6518EFAB.json',encoding='utf-8'))
print('EQUAL' if a==b else 'DIFF')")
  local bins; bins=$(ls "$LOCALAPPDATA"/OpenAI/Codex/bin/*/codex.exe 2>/dev/null | wc -l)
  local bh; bh=$(sha256sum "$CODEX" 2>/dev/null | sed 's/^\\//' | cut -c1-64)
  local app; app=$(powershell.exe -NoProfile -Command "(Get-AppxPackage *OpenAI.Codex*).Version" 2>/dev/null | tr -d '\r')
  echo "$tag utc=$(date -u +%Y-%m-%dT%H:%M:%SZ) fp=$fp structure=$struct bin=$bh bins=$bins app=$app" | tee -a "$OUT/measurements.log"
  if [ "$fp" != "$EXPECT_FP" ] || [ "$struct" != "EQUAL" ] || [ "$bh" != "$EXPECT_BIN" ] || [ "$bins" != "1" ] || [ "$app" != "$EXPECT_APP" ]; then
    echo "STOP at $tag: identity or fingerprint differs from the authorized trio (fp/structure/binary/app); P-01, OD-2 nueva; medición inválida" | tee -a "$OUT/measurements.log"; exit 3
  fi
}

load_gates() {
  [ -f "$B/gates.env" ] || refuse "falta gates.env (copia de gates.env.template rellenada tras las tres autorizaciones)"
  set -a; . "$B/gates.env"; set +a
  local v val
  for v in DECISIONS_FILE A4_AGREED_MARKER DISPOSITION_MARKER CELL SHELL_NAME TRIO PROBE_DIR BASE_SHA HEAD_SHA HASH_FILES JSON_INPUT \
           JSON_FIELDS JSON_ARRAYS CLAUSE_MAP_MODE CLAUSE_MAP_PATH CLAUSE_PAIRS DECLARATION_FILE DECLARATION_TEXT; do
    val="${!v-}"
    [ -n "$val" ] || refuse "$v vacío"
    case "$val" in *'<'*|*'>'*) refuse "$v conserva un marcador de la plantilla";; esac
  done
}

check_gates() {
  load_gates
  # 1) A-4 AGREED con A4-1, 2) disposición de aplicación y 3) A4-SONDA-CONSUMO = A, registrados en el archivo de decisiones
  [ -f "$DECISIONS_FILE" ] || refuse "no existe DECISIONS_FILE"
  [ "${#A4_AGREED_MARKER}" -ge 20 ] && [ "${#DISPOSITION_MARKER}" -ge 20 ] || refuse "marcadores de registro demasiado cortos para identificar un registro"
  tr -d '\r' < "$DECISIONS_FILE" | grep -Fq -- "$A4_AGREED_MARKER" || refuse "el acuerdo de A-4 (A4-1) no consta en el archivo de decisiones"
  tr -d '\r' < "$DECISIONS_FILE" | grep -Fq -- "$DISPOSITION_MARKER" || refuse "la disposición de aplicación de A4-1 no consta"
  [ "$(grep -c '' "$B/consumo-line.txt")" = "1" ] || refuse "consumo-line.txt debe tener una sola línea"
  tr -d '\r' < "$DECISIONS_FILE" | grep -Fxq -- "$(tr -d '\r\n' < "$B/consumo-line.txt")" || refuse "A4-SONDA-CONSUMO = A no consta literal en el archivo de decisiones"
  # lo que nombra la disposición = lo que mide este kit
  [ "$CELL" = "$EXPECT_CELL" ] || refuse "la disposición nombra otra celda ($CELL): rehacer el kit"
  [ "$SHELL_NAME" = "$EXPECT_SHELL" ] || refuse "la disposición nombra otra shell ($SHELL_NAME): rehacer el kit"
  [ "$TRIO" = "$EXPECT_TRIO" ] || refuse "la disposición nombra otro trío: rehacer el kit (y OD-2 nueva si cambió la huella)"
  case "$CLAUSE_MAP_MODE" in RESOLVE|DECLARED_UNAVAILABLE) ;; *) refuse "CLAUSE_MAP_MODE inválido";; esac
  echo "$BASE_SHA" | grep -Eq '^[0-9a-f]{40}$' || refuse "BASE_SHA no es un SHA completo"
  echo "$HEAD_SHA" | grep -Eq '^[0-9a-f]{40}$' || refuse "HEAD_SHA no es un SHA completo"
  # directorio
  local UDIR LDIR CPD f
  UDIR="$(cygpath -u "$PROBE_DIR")"; LDIR="$(cygpath -m "$PROBE_DIR" | tr 'A-Z' 'a-z')"
  [ -d "$UDIR/.git" ] || refuse "PROBE_DIR no es un clon con historia"
  case "$LDIR" in
    d:/r62-fixture/b|d:/r62-fixture/b2|d:/r62-fixture/b3|d:/r62-fixture/r|d:/r62-fixture/a6|d:/r62-fixture/arch|d:/r62-fixture/arch02|d:/r62-fixture/evidence-out*|*.git)
      refuse "directorio no admitido para la sonda del Controller de FX-02 ($LDIR)";;
  esac
  [ "$(git -C "$UDIR" config --get remote.origin.url)" = "$FIXTURE_ORIGIN_URL" ] || refuse "el origin del clon no es el origen del fixture"
  [ "$(git -C "$UDIR" config --get core.autocrlf)" = "false" ] || refuse "core.autocrlf != false: el SHA-256 del archivo no sería el del blob"
  git -C "$UDIR" cat-file -e "$BASE_SHA^{commit}" 2>/dev/null || refuse "BASE_SHA no está en la historia del clon"
  git -C "$UDIR" cat-file -e "$HEAD_SHA^{commit}" 2>/dev/null || refuse "HEAD_SHA no está en la historia del clon"
  [ -z "$(git -C "$UDIR" status --porcelain)" ] || refuse "árbol sucio en PROBE_DIR"
  for f in $HASH_FILES "$JSON_INPUT"; do git -C "$UDIR" cat-file -e "HEAD:$f" 2>/dev/null || refuse "$f no está en HEAD del clon"; done
  if [ "$CLAUSE_MAP_MODE" = "RESOLVE" ]; then git -C "$UDIR" cat-file -e "HEAD:$CLAUSE_MAP_PATH" 2>/dev/null || refuse "el mapa de cláusulas no está en HEAD"; fi
  CPD="$USERPROFILE/.claude/projects/$(cygpath -w "$PROBE_DIR" | sed 's/[^A-Za-z0-9]/-/g')"
  if [ "$LDIR" = "d:/r62-fixture/a2" ] && [ -e "$CPD" ]; then
    refuse "A2 ya tiene directorio de proyecto de Claude: nunca -C A2 con A2 abierta (la sonda en A2 va tras S03 y antes de S04)"
  fi
  # P-07: tope 1, sin reintento
  [ ! -e "$LAUNCH_MARKER" ] || refuse "la sonda ya se lanzó: tope 1 y sin reintento (A4-1, reglas 3 y 4)"
  ls "$OUT"/probe-* >/dev/null 2>&1 && refuse "hay salidas de una sonda anterior en out/"
  return 0
}

case "${1:-}" in
  measure) measure "$2" ;;
  probe)
    check_gates
    UDIR="$(cygpath -u "$PROBE_DIR")"
    SESS="$USERPROFILE/.codex/sessions/$(date +%Y/%m/%d)"
    python "$BM/probe_tools.py" build-prompt "$BM" "$OUTM/probe-prompt.txt" > "$OUT/probe-prompt-build.txt" || refuse "no se pudo construir el texto de la sonda"
    sha256sum "$OUT/probe-prompt.txt" | sed 's/^\\//' | cut -c1-64 > "$OUT/probe-prompt.sha256"
    measure pre-probe
    ( set -o noclobber; date -u +%Y-%m-%dT%H:%M:%SZ > "$LAUNCH_MARKER" ) 2>/dev/null || refuse "no se pudo crear el marcador de lanzamiento (¿ya lanzada?)"
    git -C "$UDIR" rev-parse HEAD > "$OUT/probe-head-before.txt"
    git -C "$UDIR" status --porcelain --ignored > "$OUT/probe-status-before.txt"
    ls -1 "$SESS" > "$OUT/probe-sessions-before.txt" 2>/dev/null
    date -u +%Y-%m-%dT%H:%M:%SZ > "$OUT/probe-start.txt"
    PROMPT="$(cat "$OUT/probe-prompt.txt")"
    PATH="$RTPS:$PATH" "$CODEX" exec -C "$PROBE_DIR" -s read-only -m "$MODEL" -c 'model_reasoning_effort="high"' \
      --output-schema "$(cygpath -w "$B")\\probe-schema.json" -o "$OUTW\\probe-last.json" --json "$PROMPT" \
      < /dev/null > "$OUT/probe-events.jsonl" 2> "$OUT/probe-stderr.txt" &
    CPID=$!
    WINPID="$(cat "/proc/$CPID/winpid" 2>/dev/null || true)"
    T=0; TIMED_OUT=no
    while kill -0 "$CPID" 2>/dev/null; do
      if [ "$T" -ge "$TIMEOUT_S" ]; then
        TIMED_OUT=yes
        [ -n "$WINPID" ] && taskkill //PID "$WINPID" //T //F > "$OUT/probe-taskkill.txt" 2>&1
        break
      fi
      sleep 5; T=$((T+5))
    done
    wait "$CPID"; RC=$?
    echo "exit=$RC timed_out=$TIMED_OUT" | tee "$OUT/probe-exit.txt"
    date -u +%Y-%m-%dT%H:%M:%SZ > "$OUT/probe-end.txt"
    git -C "$UDIR" rev-parse HEAD > "$OUT/probe-head-after.txt"
    git -C "$UDIR" status --porcelain --ignored > "$OUT/probe-status-after.txt"
    SESS2="$USERPROFILE/.codex/sessions/$(date +%Y/%m/%d)"
    { ls -1 "$SESS" 2>/dev/null; [ "$SESS2" != "$SESS" ] && ls -1 "$SESS2" 2>/dev/null; } > "$OUT/probe-sessions-after.txt"
    diff "$OUT/probe-sessions-before.txt" "$OUT/probe-sessions-after.txt" | grep '^>' | sed 's/^> //' > "$OUT/probe-new-sessions.txt"
    echo "head $(cat "$OUT/probe-head-before.txt") -> $(cat "$OUT/probe-head-after.txt"); status lines $(wc -l < "$OUT/probe-status-before.txt") -> $(wc -l < "$OUT/probe-status-after.txt"); session $(cat "$OUT/probe-new-sessions.txt")"
    measure post-probe
    echo "Sin reintento, sea cual sea el resultado (A4-1, regla 4). Siguiente: run_probe.sh compare" ;;
  compare)
    [ -e "$LAUNCH_MARKER" ] || refuse "no hay ninguna sonda lanzada"
    load_gates
    python "$BM/probe_tools.py" reference "$OUTM" && python "$BM/probe_tools.py" compare "$OUTM" ;;
  *) echo "uso: run_probe.sh measure <tag> | probe | compare"; exit 64 ;;
esac

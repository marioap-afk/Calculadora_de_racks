#!/usr/bin/env bash
# Sonda medida de A4-1, kit a41-v3 — NO EJECUTADO (preparación de la supervisión, plano a).
# Una sola invocación read-only de codex-cli: celda del Controller de FX-02 gpt-6-luna/high, shell declarada cmd.exe, -C = el clon que fija
# la disposición (§64, punto 2: D:\r62-fixture\A2), trío BinaryHash 3553cd6e…/AppVersion 26.1002.7124.0/huella 73890CA3… (§65, punto 5;
# OD-2f = A). Mide en un solo lanzamiento las operaciones obligatorias de PLAN y VERIFY (§65, punto 6, D-4; §64, punto 2).
# Puertas (README §3): A-4 AGREED; disposición de aplicación (celda, shell, trío y directorio); A4-SONDA-CONSUMO = A del trío nuevo (línea
# literal de §65); P-07 tope uno en total (también contra EVIDENCE_ROOT); sin reintento; nunca -C A2 con A2 abierta; directorios no admitidos.
# Uso:
#   run_probe.sh measure <tag>   -> solo identidad (sin modelo)
#   run_probe.sh probe           -> la única sonda, con las puertas de gates.env (prepara y retira el escenario S alrededor del lanzamiento)
#   run_probe.sh compare         -> después de la sonda: referencia con git y Python, y comparación (sin modelo)
# Cualquier diferencia de huella, estructura, binario o versión frente al trío autorizado => STOP (exit 3): la medición no vale.
set -u
B="$(cd "$(dirname "$0")" && pwd)"
OUT="$B/out"; mkdir -p "$OUT"; OUTW="$(cygpath -w "$OUT")"; OUTM="$(cygpath -m "$OUT")"; BM="$(cygpath -m "$B")"; BW="$(cygpath -w "$B")"
EXPECT_FP=73890CA3319206B85B68A42E23E8D323C45361CFC29E06B3E116EFFF5DAD0FBF
EXPECT_BIN=3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68
EXPECT_APP=26.1002.7124.0
EXPECT_TRIO="$EXPECT_BIN/$EXPECT_APP/$EXPECT_FP"
BASELINE="$BM/baseline-keynames-73890CA3.json"
EXPECT_CELL='gpt-6-luna/high'; MODEL='gpt-6-luna'
EXPECT_SHELL='cmd.exe'
FIXTURE_ORIGIN_URL='D:/r62-fixture/fixture-origin.git'
CODEX="$LOCALAPPDATA/OpenAI/Codex/bin/9691020b546a15b2/codex.exe"
RTPS="$USERPROFILE/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell"
TIMEOUT_S=600                       # AP 16.4: tope de 600 s; A4-1, regla 1: el tope de la receta no cambia
LAUNCH_MARKER="$OUT/PROBE_LAUNCHED" # P-07, tope 1: se crea antes de lanzar y nunca se borra (un lanzamiento incierto cuenta como lanzado)
PY="python -I"

refuse() { echo "REFUSED: $*" | tee -a "$OUT/refusals.log"; exit 2; }
sha() { sha256sum "$1" | sed 's/^\\//' | cut -c1-64; }

measure() {
  local tag="$1"
  local fp; fp=$(pwsh -NoProfile -File "$BM/cfg-fp.ps1" -Out "$OUTM/$tag-cfg.json" | tr -d '\r')
  local struct; struct=$(python -I -c "
import json
a=json.load(open(r'$OUTM/$tag-cfg.json',encoding='utf-8-sig'))['KeyNames']; b=json.load(open(r'$BASELINE',encoding='utf-8'))
print('EQUAL' if a==b else 'DIFF')")
  local bins; bins=$(ls "$LOCALAPPDATA"/OpenAI/Codex/bin/*/codex.exe 2>/dev/null | wc -l)
  local bh; bh=$(sha "$CODEX" 2>/dev/null)
  local app; app=$(powershell.exe -NoProfile -Command "(Get-AppxPackage *OpenAI.Codex*).Version" 2>/dev/null | tr -d '\r')
  echo "$tag utc=$(date -u +%Y-%m-%dT%H:%M:%SZ) fp=$fp structure=$struct bin=$bh bins=$bins app=$app" | tee -a "$OUT/measurements.log"
  if [ "$fp" != "$EXPECT_FP" ] || [ "$struct" != "EQUAL" ] || [ "$bh" != "$EXPECT_BIN" ] || [ "$bins" != "1" ] || [ "$app" != "$EXPECT_APP" ]; then
    echo "STOP at $tag: identity or fingerprint differs from the authorized trio (fp/structure/binary/app); P-01, OD-2 nueva; medición inválida" | tee -a "$OUT/measurements.log"; exit 3
  fi
}

load_gates() {
  [ -f "$B/gates.env" ] || refuse "falta gates.env (copia de gates.env.template rellenada con los literales del registro)"
  set -a; . "$B/gates.env"; set +a
  local v val
  for v in DECISIONS_FILE A4_AGREED_MARKER DISPOSITION_MARKER DISPOSITION_DIR_MARKER DISPOSITION_TRIO_MARKER CELL SHELL_NAME TRIO PROBE_DIR EVIDENCE_ROOT BRANCH \
           BASE_SHA HEAD_SHA RED_SHA BAD_SHA IGNORE_PATH HASH_FILES CONTRACT_PATH STATE_PATH DECISIONS_MD MD_LINE CLAUSE_MAP_PATH \
           CLAUSE_SCHEMA_PATH TRX_SOURCE TRX_SOURCE_SHA256 TRX_RUN_ID TRX_RUN_HEAD_SHA CANONICAL_SCHEMA_MODE; do
    val="${!v-}"
    [ -n "$val" ] || refuse "$v vacío"
    case "$val" in *'<'*|*'>'*) refuse "$v conserva un marcador de la plantilla";; esac
  done
  DECISIONS_U="$(cygpath -u "$DECISIONS_FILE")"; EVIDENCE_U="$(cygpath -u "$EVIDENCE_ROOT")"; TRX_U="$(cygpath -u "$TRX_SOURCE")"
  case "$CANONICAL_SCHEMA_MODE" in VERBATIM) SCHEMA_FILE="probe-schema.json";; STRICT_PROJECTION) SCHEMA_FILE="probe-schema.strict-projection.json";;
    *) refuse "CANONICAL_SCHEMA_MODE inválido";; esac
}

check_gates() {
  load_gates
  # 1) A-4 AGREED, 2) disposición de aplicación (celda, shell, trío y directorio) y 3) A4-SONDA-CONSUMO = A, en el archivo de decisiones
  [ -f "$DECISIONS_U" ] || refuse "no existe DECISIONS_FILE"
  [ "${#A4_AGREED_MARKER}" -ge 20 ] && [ "${#DISPOSITION_MARKER}" -ge 20 ] && [ "${#DISPOSITION_DIR_MARKER}" -ge 20 ] && [ "${#DISPOSITION_TRIO_MARKER}" -ge 20 ] \
    || refuse "marcadores de registro demasiado cortos para identificar un registro"
  tr -d '\r' < "$DECISIONS_U" | grep -Fq -- "$A4_AGREED_MARKER" || refuse "el acuerdo de A-4 no consta en el archivo de decisiones"
  tr -d '\r' < "$DECISIONS_U" | grep -Fq -- "$DISPOSITION_MARKER" || refuse "la disposición de aplicación de A4-1 no consta"
  tr -d '\r' < "$DECISIONS_U" | grep -Fq -- "$DISPOSITION_DIR_MARKER" || refuse "el texto de la disposición que fija el directorio -C no consta"
  tr -d '\r' < "$DECISIONS_U" | grep -Fq -- "$DISPOSITION_TRIO_MARKER" || refuse "el texto de la disposición que nombra el trío no consta"
  case "$DISPOSITION_TRIO_MARKER" in *"$EXPECT_BIN"*"$EXPECT_APP"*"$EXPECT_FP"*) ;; *) refuse "la disposición del trío no nombra BinaryHash, AppVersion y la huella aceptada, en ese orden";; esac
  local NDIR NMARK
  NDIR="$(printf '%s' "$PROBE_DIR" | tr '\134' '/' | tr 'A-Z' 'a-z')"; NMARK="$(printf '%s' "$DISPOSITION_DIR_MARKER" | tr '\134' '/' | tr 'A-Z' 'a-z')"
  case "$NMARK" in *"$NDIR"*) ;; *) refuse "la disposición no fija este PROBE_DIR (A4-1, regla 3)";; esac
  [ "$(grep -c '' "$B/consumo-line.txt")" = "1" ] || refuse "consumo-line.txt debe tener una sola línea"
  tr -d '\r' < "$DECISIONS_U" | grep -Fxq -- "$(tr -d '\r\n' < "$B/consumo-line.txt")" || refuse "A4-SONDA-CONSUMO = A (trío nuevo) no consta literal, como línea completa"
  grep -Fq "$EXPECT_FP" "$B/consumo-line.txt" || refuse "consumo-line.txt no nombra la huella aceptada (OD-2f): no se usa la línea de la huella anterior"
  # lo que nombra la disposición = lo que mide este kit
  [ "$CELL" = "$EXPECT_CELL" ] || refuse "la disposición nombra otra celda ($CELL): rehacer el kit"
  [ "$SHELL_NAME" = "$EXPECT_SHELL" ] || refuse "la disposición nombra otra shell ($SHELL_NAME): rehacer el kit"
  [ "$TRIO" = "$EXPECT_TRIO" ] || refuse "la disposición nombra otro trío: rehacer el kit (y OD-2 nueva si cambió la huella)"
  # A4-1, regla 3: tope de uno EN TOTAL para FX-02 en F6, también frente a la evidencia real
  [ -d "$EVIDENCE_U" ] || refuse "no existe EVIDENCE_ROOT"
  ls -d "$EVIDENCE_U"/*a4-1-probe* >/dev/null 2>&1 && refuse "ya hay una medición de A4-1 custodiada: tope uno en total para FX-02 en F6"
  [ ! -e "$LAUNCH_MARKER" ] || refuse "la sonda ya se lanzó: tope 1 y sin reintento (A4-1, reglas 3 y 4)"
  ls "$OUT"/probe-* >/dev/null 2>&1 && refuse "hay salidas de una sonda anterior en out/"
  [ -e "$OUT/stage-manifest.json" ] && refuse "hay un escenario S preparado en out/ (stage-manifest.json): ejecutar unstage y revisar"
  # parámetros
  local s
  for s in BASE_SHA HEAD_SHA RED_SHA BAD_SHA TRX_RUN_HEAD_SHA; do echo "${!s}" | grep -Eq '^[0-9a-f]{40}$' || refuse "$s no es un SHA completo"; done
  echo "$TRX_SOURCE_SHA256" | grep -Eq '^[0-9a-f]{64}$' || refuse "TRX_SOURCE_SHA256 inválido"
  echo "$MD_LINE" | grep -Eq '^[0-9]+$' || refuse "MD_LINE inválido"
  case "$IGNORE_PATH" in artifacts/orchestration/*) ;; *) refuse "IGNORE_PATH debe estar en artifacts/orchestration/";; esac
  case "$IGNORE_PATH" in *..*|*\\*) refuse "IGNORE_PATH con '..' o '\\'";; esac
  [ -f "$TRX_U" ] && [ "$(sha "$TRX_U")" = "$TRX_SOURCE_SHA256" ] || refuse "el TRX custodiado falta o no tiene el SHA-256 esperado"
  # directorio -C
  local UDIR LDIR CPD f
  UDIR="$(cygpath -u "$PROBE_DIR")"; LDIR="$(cygpath -m "$PROBE_DIR" | tr 'A-Z' 'a-z')"
  [ -d "$UDIR/.git" ] || refuse "PROBE_DIR no es un clon con historia (el clon preparado aún no existe)"
  case "$LDIR" in
    d:/r62-fixture/a|d:/r62-fixture/b|d:/r62-fixture/b2|d:/r62-fixture/b3|d:/r62-fixture/r|d:/r62-fixture/a6|d:/r62-fixture/arch|d:/r62-fixture/arch02|d:/r62-fixture/evidence-out*|d:/r62-fixture/supervisor*|*.git)
      refuse "directorio no admitido para la sonda del Controller de FX-02 ($LDIR)";;
  esac
  [ "$(git -C "$UDIR" config --get remote.origin.url)" = "$FIXTURE_ORIGIN_URL" ] || refuse "el origin del clon no es el origen del fixture"
  [ "$(git -C "$UDIR" config --get core.autocrlf)" = "false" ] || refuse "core.autocrlf != false: el SHA-256 del archivo no sería el del blob"
  for s in "$BASE_SHA" "$HEAD_SHA" "$RED_SHA" "$TRX_RUN_HEAD_SHA"; do git -C "$UDIR" cat-file -e "$s^{commit}" 2>/dev/null || refuse "$s no está en la historia del clon"; done
  git -C "$UDIR" cat-file -e "$BAD_SHA" 2>/dev/null && refuse "BAD_SHA existe en el clon: el camino de error no se ejercería"
  git -C "$UDIR" merge-base --is-ancestor "$BASE_SHA" "$RED_SHA" && git -C "$UDIR" merge-base --is-ancestor "$RED_SHA" HEAD \
    || refuse "RED_SHA no está entre BASE_SHA y HEAD"
  git -C "$UDIR" rev-parse --verify -q "refs/remotes/origin/$BRANCH" >/dev/null || refuse "el clon no tiene refs/remotes/origin/$BRANCH (Identity)"
  git -C "$UDIR" rev-parse --verify -q refs/remotes/origin/main >/dev/null || refuse "el clon no tiene refs/remotes/origin/main (Remote)"
  [ "$(git -C "$UDIR" rev-parse --abbrev-ref HEAD)" = "$BRANCH" ] || refuse "el clon no está en la rama $BRANCH"
  [ "$(git -C "$UDIR" rev-parse HEAD)" = "$(git -C "$UDIR" ls-remote origin "refs/heads/$BRANCH" | cut -f1)" ] \
    || refuse "HEAD del clon != ls-remote origin $BRANCH: el clon preparado no está en la punta de la rama (rehacer el clon)"
  [ -z "$(git -C "$UDIR" status --porcelain --ignored)" ] || refuse "PROBE_DIR no está limpio (incluidos los ignorados): clon fresco requerido"
  for f in $HASH_FILES "$CONTRACT_PATH" "$STATE_PATH" "$DECISIONS_MD" "$CLAUSE_MAP_PATH" "$CLAUSE_SCHEMA_PATH"; do
    git -C "$UDIR" cat-file -e "HEAD:$f" 2>/dev/null || refuse "$f no está en HEAD del clon"
  done
  git -C "$UDIR" check-ignore -q --no-index "$IGNORE_PATH" || refuse "IGNORE_PATH no está ignorado en el clon (falta el .gitignore de U-08)"
  CPD="$USERPROFILE/.claude/projects/$(cygpath -w "$PROBE_DIR" | sed 's/[^A-Za-z0-9]/-/g')"
  if [ "$LDIR" = "d:/r62-fixture/a2" ] && [ -e "$CPD" ]; then
    refuse "A2 ya tiene directorio de proyecto de Claude: nunca -C A2 con A2 abierta (§64, punto 2: sin sesión A2 activa)"
  fi
  # el esquema de salida incrusta los esquemas canónicos de HEAD del clon
  $PY "$BW\\probe_tools.py" check-schema "$BW" > "$OUT/schema-check.txt" 2>&1 || refuse "el esquema de salida no incrusta los canónicos de HEAD ($(cat "$OUT/schema-check.txt"))"
  return 0
}

case "${1:-}" in
  measure) measure "$2" ;;
  probe)
    check_gates
    UDIR="$(cygpath -u "$PROBE_DIR")"
    SESS="$USERPROFILE/.codex/sessions/$(date +%Y/%m/%d)"
    $PY "$BW\\probe_tools.py" build-prompt "$BW" "$OUTW\\probe-prompt.txt" > "$OUT/probe-prompt-build.txt" || refuse "no se pudo construir el texto de la sonda"
    sha "$OUT/probe-prompt.txt" > "$OUT/probe-prompt.sha256"
    cp "$B/$SCHEMA_FILE" "$OUT/probe-schema.used.json"; sha "$OUT/probe-schema.used.json" > "$OUT/probe-schema.used.sha256"
    measure pre-probe
    $PY "$BW\\probe_tools.py" stage "$OUTW" > "$OUT/stage.txt" 2>&1 || refuse "no se pudo preparar el escenario S ($(tail -1 "$OUT/stage.txt")); revisar y ejecutar unstage si escribió algo"
    ( set -o noclobber; date -u +%Y-%m-%dT%H:%M:%SZ > "$LAUNCH_MARKER" ) 2>/dev/null \
      || { $PY "$BW\\probe_tools.py" unstage "$OUTW" > "$OUT/unstage.txt" 2>&1; refuse "no se pudo crear el marcador de lanzamiento (¿ya lanzada?); escenario retirado"; }
    git -C "$UDIR" rev-parse HEAD > "$OUT/probe-head-before.txt"
    git -C "$UDIR" status --porcelain > "$OUT/probe-status-before.txt"
    git -C "$UDIR" status --porcelain --ignored > "$OUT/probe-status-ignored-before-run.txt"
    git -C "$UDIR" ls-remote origin > "$OUT/probe-lsremote-before.txt" 2>/dev/null
    ls -1 "$SESS" > "$OUT/probe-sessions-before.txt" 2>/dev/null
    date -u +%Y-%m-%dT%H:%M:%SZ > "$OUT/probe-start.txt"
    PROMPT="$(cat "$OUT/probe-prompt.txt")"
    PATH="$RTPS:$PATH" "$CODEX" exec -C "$PROBE_DIR" -s read-only -m "$MODEL" -c 'model_reasoning_effort="high"' \
      --output-schema "$BW\\$SCHEMA_FILE" -o "$OUTW\\probe-last.json" --json "$PROMPT" \
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
    echo "exit=$RC timed_out=$TIMED_OUT elapsed_s<=$T" | tee "$OUT/probe-exit.txt"
    date -u +%Y-%m-%dT%H:%M:%SZ > "$OUT/probe-end.txt"
    git -C "$UDIR" rev-parse HEAD > "$OUT/probe-head-after.txt"
    git -C "$UDIR" status --porcelain > "$OUT/probe-status-after.txt"
    git -C "$UDIR" status --porcelain --ignored > "$OUT/probe-status-ignored-after-run.txt"
    git -C "$UDIR" ls-remote origin > "$OUT/probe-lsremote-after.txt" 2>/dev/null
    SESS2="$USERPROFILE/.codex/sessions/$(date +%Y/%m/%d)"
    { ls -1 "$SESS" 2>/dev/null; [ "$SESS2" != "$SESS" ] && ls -1 "$SESS2" 2>/dev/null; } > "$OUT/probe-sessions-after.txt"
    diff "$OUT/probe-sessions-before.txt" "$OUT/probe-sessions-after.txt" | grep '^>' | sed 's/^> //' > "$OUT/probe-new-sessions.txt"
    # retirada del escenario S (comprueba que lo preparado no cambió y que status --ignored vuelve al de antes del stage)
    $PY "$BW\\probe_tools.py" unstage "$OUTW" > "$OUT/unstage.txt" 2>&1 || echo "AVISO: unstage con diferencias; ver out/unstage-report.json" | tee -a "$OUT/unstage.txt"
    echo "head $(cat "$OUT/probe-head-before.txt") -> $(cat "$OUT/probe-head-after.txt"); session $(cat "$OUT/probe-new-sessions.txt")"
    measure post-probe
    echo "Sin reintento, sea cual sea el resultado (A4-1, regla 4). Siguiente: run_probe.sh compare" ;;
  compare)
    [ -e "$LAUNCH_MARKER" ] || refuse "no hay ninguna sonda lanzada"
    load_gates
    $PY "$BW\\probe_tools.py" reference "$OUTW" && $PY "$BW\\probe_tools.py" compare "$OUTW" ;;
  *) echo "uso: run_probe.sh measure <tag> | probe | compare"; exit 64 ;;
esac

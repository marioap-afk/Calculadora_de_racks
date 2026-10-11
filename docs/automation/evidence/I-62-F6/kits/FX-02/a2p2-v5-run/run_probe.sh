#!/usr/bin/env bash
# Kit a2p2-v5 — NO EJECUTADO (preparación de la supervisión, plano a). Deriva del kit a2p2-v4 (misma receta, medición de identidad, escenario S,
# instantáneas y auditor de eventos con la corrección de git_write).
# Sonda 2 (y última) del bloque A2-P2 del par d13914ce…/26.1007.2314.0 (huella F909F480…), reasignada al Controller (decisiones §73, opción B):
#   CONTROLLER_V5  Controller gpt-6-luna/high, receta 16.4 literal (sin shell declarada), -C = D:\r62-fixture\A2 (U-03, §55.10); invocación
#                  conjunta de PLAN y VERIFY con un diseño distinto del de la sonda 1 (paso 0 de UTF-8, sin presión de tiempo, sin negativos
#                  agregados, sin ls-remote, respuesta acotada). UN solo lanzamiento (marcador que nunca se borra), sin reintento.
# Los modos de a2p2-v4 (NATIVE_CONTROLLER, NATIVE_ARCHITECT, CMD_A41) están retirados: este kit no los lanza.
# Uso:
#   run_probe.sh measure <tag>           -> solo identidad (sin modelo), en out/measure/
#   run_probe.sh probe CONTROLLER_V5     -> la sonda, con las puertas de gates.env
#   run_probe.sh compare CONTROLLER_V5   -> después de la sonda: referencia con git y Python, y comparación (sin modelo)
# Cualquier diferencia de huella, estructura, binario o versión frente a la identidad autorizada => STOP (exit 3): la medición no vale.
set -u
B="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUT="$B/out"; mkdir -p "$OUT"; BM="$(cygpath -m "$B")"; BW="$(cygpath -w "$B")"
EXPECT_FP=F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113
EXPECT_BIN=d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec
EXPECT_APP=26.1007.2314.0
EXPECT_LABEL=2e5e00daee91c61d
EXPECT_PAIR="$EXPECT_BIN/$EXPECT_APP"
BASELINE="$BM/baseline-keynames-F909F480.json"
FIXTURE_ORIGIN_URL='D:/r62-fixture/fixture-origin.git'
CODEX="$LOCALAPPDATA/OpenAI/Codex/bin/$EXPECT_LABEL/codex.exe"
RTPS="$USERPROFILE/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell"
TIMEOUT_S=600                      # AP 16.4: tope de 600 s
EFFORT='high'                      # Balanced con nivel Eficiente (Controller) -> high (catálogo)
TEMPLATE='probe-prompt.controller-v5.template.txt'
STEP0_MARK='0. Fidelidad UTF-8 de la shell'
PY="python -I"

refuse() { echo "REFUSED: $*" | tee -a "$OUT/refusals.log" >&2; exit 2; }
sha() { sha256sum "$1" | sed 's/^\\//' | cut -c1-64; }
norm() { printf '%s' "$1" | tr '\134' '/' | tr 'A-Z' 'a-z' | tr -d ' `'; }
in_decisions() { tr -d '\r' < "$DECISIONS_U" | grep -Fq -- "$1"; }
line_in_decisions() {  # $1 = archivo del kit con UNA línea literal del Owner; tiene que ser una línea completa del archivo de decisiones
  [ -f "$B/$1" ] || refuse "falta $1"
  [ "$(grep -c '' "$B/$1")" = "1" ] || refuse "$1 debe tener una sola línea"
  tr -d '\r' < "$DECISIONS_U" | grep -Fxq -- "$(tr -d '\r\n' < "$B/$1")" || refuse "$1 no consta literal, como línea completa, en el archivo de decisiones"
}
names_in_order() {  # $1 = literal; cierto si contiene BinaryHash, AppVersion y huella, en ese orden
  case "$1" in *"$EXPECT_BIN"*"$EXPECT_APP"*"$EXPECT_FP"*) return 0;; *) return 1;; esac
}

mode_setup() {
  MODE="${1:-}"
  case "$MODE" in
    CONTROLLER_V5) SLUG=controller-v5; CELL='gpt-6-luna/high'; MODEL='gpt-6-luna'; EVID_PAT='*a2p2-v5-controller*' ;;
    NATIVE_CONTROLLER|NATIVE_ARCHITECT|CMD_A41) echo "modo retirado en a2p2-v5: '$MODE' (solo CONTROLLER_V5)"; exit 64 ;;
    *) echo "modo inválido: '${MODE}' (CONTROLLER_V5)"; exit 64 ;;
  esac
  OUTD="$OUT/$SLUG"; mkdir -p "$OUTD"; OUTDW="$(cygpath -w "$OUTD")"; OUTDM="$(cygpath -m "$OUTD")"
  LAUNCH_MARKER="$OUTD/PROBE_LAUNCHED"   # P-07: se crea antes de lanzar y nunca se borra (un lanzamiento incierto cuenta como lanzado)
  export PROBE_MODE="$MODE"
}

measure() {
  local tag="$1" dir="$2"
  local fp; fp=$(pwsh -NoProfile -File "$BM/cfg-fp.ps1" -Out "$(cygpath -m "$dir")/$tag-cfg.json" | tr -d '\r')
  local struct; struct=$(python -I -c "
import json
a=json.load(open(r'$(cygpath -m "$dir")/$tag-cfg.json',encoding='utf-8-sig'))['KeyNames']; b=json.load(open(r'$BASELINE',encoding='utf-8'))
print('EQUAL' if a==b else 'DIFF')")
  local bins; bins=$(ls "$LOCALAPPDATA"/OpenAI/Codex/bin/*/codex.exe 2>/dev/null | wc -l)
  local bh; bh=$(sha "$CODEX" 2>/dev/null)
  local app; app=$(powershell.exe -NoProfile -Command "(Get-AppxPackage *OpenAI.Codex*).Version" 2>/dev/null | tr -d '\r')
  # ejecutables acompañantes del binario: huella conjunta; entra en la comparación antes/después (un cambio a mitad es una actualización)
  local comp; comp=$( (cd "$LOCALAPPDATA/OpenAI/Codex/bin/$EXPECT_LABEL" 2>/dev/null && for f in *.exe; do printf '%s %s\n' "$f" "$(sha256sum "$f" | cut -c1-64)"; done) \
    | tee "$dir/$tag-companions.txt" | sha256sum | cut -c1-16)
  echo "$tag utc=$(date -u +%Y-%m-%dT%H:%M:%SZ) fp=$fp structure=$struct bin=$bh bins=$bins app=$app comp=$comp" | tee -a "$dir/measurements.log"
  if [ "$fp" != "$EXPECT_FP" ] || [ "$struct" != "EQUAL" ] || [ "$bh" != "$EXPECT_BIN" ] || [ "$bins" != "1" ] || [ "$app" != "$EXPECT_APP" ]; then
    echo "STOP at $tag: identity or fingerprint differs from the authorized identity (fp/structure/binary/app); P-01, OD-2 nueva; medición inválida" | tee -a "$dir/measurements.log"; exit 3
  fi
}

need_vars() {
  local v val
  for v in "$@"; do
    val="${!v-}"
    [ -n "$val" ] || refuse "$v vacío"
    case "$val" in *'<'*|*'>'*) refuse "$v conserva un marcador de la plantilla";; esac
  done
}

load_gates() {
  [ -f "$B/gates.env" ] || refuse "falta gates.env (copia de gates.env.template rellenada con los literales del registro)"
  set -a; . "$B/gates.env"; set +a
  need_vars DECISIONS_FILE EVIDENCE_FILE EVIDENCE_ROOT CONTROLLER_PROBE_DIR EXPECTED_TIP BRANCH BAD_SHA CONTRACT_PATH CANONICAL_SCHEMA_MODE \
            BASE_SHA HEAD_SHA RED_SHA IGNORE_PATH HASH_FILES STATE_PATH DECISIONS_MD MD_LINE U0_LINE CLAUSE_MAP_PATH CLAUSE_SCHEMA_PATH \
            TRX_SOURCE TRX_SOURCE_SHA256 TRX_RUN_ID TRX_RUN_HEAD_SHA PROBE1_RUN PROBE1_PROMPT_SHA \
            A2P2_PAIR_MARKER A2P2_STABILITY_MARKER DISP_V5_REASSIGN_MARKER DISP_V5_CELL_MARKER DISP_V5_DESIGN_MARKER DIR_U03_MARKER
  export PROBE_DIR="$CONTROLLER_PROBE_DIR"
  DECISIONS_U="$(cygpath -u "$DECISIONS_FILE")"; EVIDENCE_FILE_U="$(cygpath -u "$EVIDENCE_FILE")"; EVIDENCE_U="$(cygpath -u "$EVIDENCE_ROOT")"
  case "$CANONICAL_SCHEMA_MODE" in VERBATIM) SCHEMA_FILE="probe-schema.$SLUG.json";; STRICT_PROJECTION) SCHEMA_FILE="probe-schema.$SLUG.strict-projection.json";;
    *) refuse "CANONICAL_SCHEMA_MODE inválido";; esac
  return 0
}

count_dirs() { ls -d "$EVIDENCE_U"/$1 2>/dev/null | wc -l | tr -d ' '; }

check_budget() {
  # P-07 del bloque A2-P2 (A-2, §3.3): ≤ 2 sondas para el par; la 1 (NATIVE_CONTROLLER) está consumida y custodiada; queda 1/2 (§73.4)
  local nv4c nv4a nv5 nloc
  [ -d "$EVIDENCE_U" ] || refuse "no existe EVIDENCE_ROOT"
  nv4c=$(count_dirs '*a2p2-v4-controller*'); nv4a=$(count_dirs '*a2p2-v4-architect*'); nv5=$(count_dirs "$EVID_PAT")
  nloc=0; [ -e "$LAUNCH_MARKER" ] && nloc=1
  [ "$nv5" = "0" ] || refuse "ya hay una medición custodiada de $MODE ($EVID_PAT): un lanzamiento, sin reintento"
  [ "$nloc" = "0" ] || refuse "la sonda $MODE ya se lanzó (marcador local): tope 1 y sin reintento"
  [ "$nv4a" = "0" ] || refuse "el bloque A2-P2 ya tiene una sonda del Architect custodiada: con la sonda 1, el bloque consumió sus dos sondas"
  [ "$nv4c" = "1" ] && [ -d "$EVIDENCE_U/$PROBE1_RUN" ] \
    || refuse "presupuesto del bloque no verificable: la evidencia no tiene exactamente la sonda 1 consumida ($PROBE1_RUN); se exige 1/2 restante (§73.6)"
  [ $(( nv4c + nv4a + nv5 + nloc )) -lt 2 ] || refuse "el bloque A2-P2 de este par ya consumió sus dos sondas"
  [ -f "$EVIDENCE_U/$PROBE1_RUN/comparison.json" ] && [ -f "$EVIDENCE_U/$PROBE1_RUN/probe-prompt.sha256" ] \
    || refuse "la evidencia de la sonda 1 está incompleta (comparison.json y probe-prompt.sha256)"
  return 0
}

check_not_repetition() {
  # §69.5, §70.2, §73.4: la sonda 1 no se repite. Diseño distinto (plantilla v5) y texto armado distinto del de la sonda 1.
  local tpl="$B/$TEMPLATE" h p1
  [ -f "$tpl" ] || refuse "falta $TEMPLATE"
  grep -Fq "$STEP0_MARK" "$tpl" || refuse "la plantilla no tiene el paso 0 de fidelidad UTF-8 (§73.5): no es el diseño v5"
  grep -Eiq 'tiempo[[:space:]]+limitado|pocas[[:space:]]+invocaciones|no[[:space:]]+repitas[[:space:]]+lecturas' "$tpl" \
    && refuse "la plantilla conserva la presión de tiempo de la sonda 1 (§73.5)"
  $PY "$BW\\probe_tools.py" build-prompt "$BW" "$OUTDW\\gate-prompt.txt" > "$OUTD/gate-prompt-build.txt" 2>&1 \
    || refuse "no se pudo armar el texto de la sonda ($(tail -1 "$OUTD/gate-prompt-build.txt"))"
  h="$(sha "$OUTD/gate-prompt.txt")"
  p1="$(tr -d '\r\n ' < "$EVIDENCE_U/$PROBE1_RUN/probe-prompt.sha256" | cut -c1-64)"
  [ "$h" != "$PROBE1_PROMPT_SHA" ] && [ "$h" != "$p1" ] || refuse "el texto armado es el de la sonda 1 ($h): repetición prohibida"
  echo "$h" > "$OUTD/gate-prompt.sha256"
  return 0
}

check_controller_dir() {
  local dir="$CONTROLLER_PROBE_DIR" UDIR LDIR CPD s f lsm
  LDIR="$(cygpath -m "$dir" | tr 'A-Z' 'a-z')"
  case "$LDIR" in
    d:/r62-fixture/a|d:/r62-fixture/b|d:/r62-fixture/b2|d:/r62-fixture/b3|d:/r62-fixture/r|d:/r62-fixture/a6|d:/r62-fixture/arch|d:/r62-fixture/arch02|\
    d:/r62-fixture/arch-a2p2v4|d:/r62-fixture/evidence-out*|d:/r62-fixture/supervisor*|d:/r62-fixture/fx05*|d:/r62-fixture/probe-od4-1|*.git)
      refuse "directorio no admitido para esta sonda ($LDIR)";;
  esac
  UDIR="$(cygpath -u "$dir")"
  [ -d "$UDIR/.git" ] || refuse "PROBE_DIR no es un clon con historia"
  [ "$(git -C "$UDIR" config --get remote.origin.url)" = "$FIXTURE_ORIGIN_URL" ] || refuse "el origin del clon no es el origen del fixture"
  [ "$(git -C "$UDIR" config --get core.autocrlf)" = "false" ] || refuse "core.autocrlf != false: el SHA-256 del archivo no sería el del blob"
  [ "$(git -C "$UDIR" rev-parse --abbrev-ref HEAD)" = "$BRANCH" ] || refuse "el clon no está en la rama $BRANCH"
  [ "$(git -C "$UDIR" rev-parse HEAD)" = "$EXPECTED_TIP" ] || refuse "HEAD del clon != EXPECTED_TIP ($EXPECTED_TIP)"
  [ "$(git -C "$UDIR" rev-parse HEAD)" = "$(git -C "$UDIR" ls-remote origin "refs/heads/$BRANCH" | cut -f1)" ] \
    || refuse "HEAD del clon != ls-remote origin $BRANCH: el clon no está en la punta de la rama"
  [ "$(git -C "$UDIR" rev-parse "refs/remotes/origin/$BRANCH")" = "$EXPECTED_TIP" ] || refuse "refs/remotes/origin/$BRANCH != EXPECTED_TIP (Identity)"
  lsm="$(git -C "$UDIR" ls-remote origin refs/heads/main | cut -f1)"
  [ -n "$lsm" ] && [ "$(git -C "$UDIR" rev-parse refs/remotes/origin/main 2>/dev/null)" = "$lsm" ] \
    || refuse "refs/remotes/origin/main del clon != ls-remote origin main: el MainSha de PLAN no sería «origin/main recién obtenido» (AP 16.5, A6)"
  [ -z "$(git -C "$UDIR" status --porcelain --ignored)" ] || refuse "PROBE_DIR no está limpio (incluidos los ignorados): clon fresco requerido"
  git -C "$UDIR" cat-file -e "$BAD_SHA" 2>/dev/null && refuse "BAD_SHA existe en el clon: el camino de error no se ejercería"
  CPD="$USERPROFILE/.claude/projects/$(cygpath -w "$dir" | sed 's/[^A-Za-z0-9]/-/g')"
  [ ! -e "$CPD" ] || refuse "el directorio -C tiene directorio de proyecto de Claude: nunca -C con una sesión A2 abierta o previa en él (§64, punto 2; §73.6)"
  for s in BASE_SHA HEAD_SHA RED_SHA BAD_SHA TRX_RUN_HEAD_SHA EXPECTED_TIP; do echo "${!s}" | grep -Eq '^[0-9a-f]{40}$' || refuse "$s no es un SHA completo"; done
  echo "$TRX_SOURCE_SHA256" | grep -Eq '^[0-9a-f]{64}$' || refuse "TRX_SOURCE_SHA256 inválido"
  echo "$MD_LINE" | grep -Eq '^[0-9]+$' && echo "$U0_LINE" | grep -Eq '^[0-9]+$' || refuse "MD_LINE o U0_LINE inválido"
  case "$IGNORE_PATH" in artifacts/orchestration/*) ;; *) refuse "IGNORE_PATH debe estar en artifacts/orchestration/";; esac
  case "$IGNORE_PATH" in *..*|*\\*) refuse "IGNORE_PATH con '..' o '\\'";; esac
  [ -f "$(cygpath -u "$TRX_SOURCE")" ] && [ "$(sha "$(cygpath -u "$TRX_SOURCE")")" = "$TRX_SOURCE_SHA256" ] || refuse "el TRX custodiado falta o no tiene el SHA-256 esperado"
  for s in "$BASE_SHA" "$HEAD_SHA" "$RED_SHA" "$TRX_RUN_HEAD_SHA"; do git -C "$UDIR" cat-file -e "$s^{commit}" 2>/dev/null || refuse "$s no está en la historia del clon"; done
  git -C "$UDIR" merge-base --is-ancestor "$BASE_SHA" "$RED_SHA" && git -C "$UDIR" merge-base --is-ancestor "$RED_SHA" HEAD \
    || refuse "RED_SHA no está entre BASE_SHA y HEAD"
  for f in $HASH_FILES "$CONTRACT_PATH" "$STATE_PATH" "$DECISIONS_MD" "$CLAUSE_MAP_PATH" "$CLAUSE_SCHEMA_PATH"; do
    git -C "$UDIR" cat-file -e "HEAD:$f" 2>/dev/null || refuse "$f no está en HEAD del clon"
  done
  git -C "$UDIR" check-ignore -q --no-index "$IGNORE_PATH" || refuse "IGNORE_PATH no está ignorado en el clon (falta el .gitignore de U-08)"
  return 0
}

check_gates() {
  mode_setup "$1"
  load_gates
  local NDIR NCELL
  [ -f "$DECISIONS_U" ] || refuse "no existe DECISIONS_FILE"
  [ -x "$RTPS/pwsh.exe" ] || [ -f "$RTPS/pwsh.exe" ] || refuse "no existe el pwsh del runtime que la receta 16.4 pone primero en el PATH"
  # P-01 desde la actualización (evidencia §125): OD-2 del Owner sobre la huella vigente (A2-P2, regla 4)
  line_in_decisions od2-line.txt
  grep -Fq "$EXPECT_FP" "$B/od2-line.txt" && grep -Fq "$EXPECT_BIN" "$B/od2-line.txt" || refuse "od2-line.txt no nombra la huella y el binario vigentes"
  # A2-P2, regla 3: disposición del bloque que nombra el par exacto con su huella estable, observación estable y consumo del Owner
  for v in A2P2_PAIR_MARKER A2P2_STABILITY_MARKER DISP_V5_REASSIGN_MARKER DISP_V5_CELL_MARKER DISP_V5_DESIGN_MARKER DIR_U03_MARKER; do
    [ "$(printf '%s' "${!v}" | wc -c)" -ge 20 ] || refuse "$v demasiado corto para identificar un registro"
  done
  in_decisions "$A2P2_PAIR_MARKER" || refuse "la disposición del bloque A2-P2 (par exacto) no consta en el archivo de decisiones"
  names_in_order "$A2P2_PAIR_MARKER" || refuse "la disposición del bloque no nombra BinaryHash, AppVersion y la huella, en ese orden"
  [ -f "$EVIDENCE_FILE_U" ] && tr -d '\r' < "$EVIDENCE_FILE_U" | grep -Fq -- "$A2P2_STABILITY_MARKER" || refuse "la observación estable del par no consta en el archivo de evidencia"
  case "$A2P2_STABILITY_MARKER" in *"$EXPECT_FP"*) ;; *) refuse "el registro de estabilidad no nombra la huella F909F480…";; esac
  line_in_decisions bloque-consumo-line.txt
  grep -Fq "$EXPECT_PAIR" "$B/bloque-consumo-line.txt" || refuse "bloque-consumo-line.txt no nombra el par exacto (BinaryHash/AppVersion)"
  # §73.4: reasignación de la segunda sonda al Controller, celda e identidad; §73.5: diseño
  in_decisions "$DISP_V5_REASSIGN_MARKER" || refuse "la reasignación de la sonda 2 al Controller no consta en el archivo de decisiones"
  case "$DISP_V5_REASSIGN_MARKER" in *Controller*) ;; *) refuse "la reasignación no nombra al Controller";; esac
  case "$DISP_V5_REASSIGN_MARKER" in *A2-P2*) ;; *) refuse "la reasignación no nombra el bloque A2-P2";; esac
  in_decisions "$DISP_V5_CELL_MARKER" || refuse "la celda de la sonda reasignada no consta en el archivo de decisiones"
  NCELL="$(norm "$DISP_V5_CELL_MARKER")"
  case "$NCELL" in *"codex-cli/$(norm "$CELL")"*) ;; *) refuse "la disposición no nombra la celda codex-cli / $CELL";; esac
  names_in_order "$DISP_V5_CELL_MARKER" || refuse "la disposición de la celda no nombra la identidad autorizada (BinaryHash, AppVersion, huella)"
  in_decisions "$DISP_V5_DESIGN_MARKER" || refuse "la disposición del diseño de la sonda no consta en el archivo de decisiones"
  case "$DISP_V5_DESIGN_MARKER" in *PLAN*VERIFY*) ;; *) refuse "la disposición del diseño no nombra PLAN y VERIFY";; esac
  # U-03 (§55.10): -C del Controller de FX-02
  in_decisions "$DIR_U03_MARKER" || refuse "el literal de U-03 (-C del Controller) no consta en el archivo de decisiones"
  case "$DIR_U03_MARKER" in *Controller*) ;; *) refuse "el literal de U-03 no nombra al Controller";; esac
  NDIR="$(norm "$CONTROLLER_PROBE_DIR")"
  case "$(norm "$DIR_U03_MARKER")" in *"$NDIR"|*"$NDIR"[!a-z0-9_-]*) ;; *) refuse "U-03 no fija este directorio -C ($CONTROLLER_PROBE_DIR)";; esac
  # presupuesto, no repetición, salidas previas, clon y esquema
  check_budget
  ls "$OUTD"/probe-* >/dev/null 2>&1 && refuse "hay salidas de una sonda anterior en out/$SLUG/"
  [ -e "$OUTD/stage-manifest.json" ] && refuse "hay un escenario S preparado en out/$SLUG/ (stage-manifest.json): ejecutar unstage y revisar"
  check_controller_dir
  check_not_repetition
  $PY "$BW\\probe_tools.py" check-schema "$BW" > "$OUTD/schema-check.txt" 2>&1 || refuse "el esquema de salida no incrusta los canónicos de HEAD ($(cat "$OUTD/schema-check.txt"))"
  return 0
}

snapshot() {  # $1 = etiqueta (before|after)
  local UDIR; UDIR="$(cygpath -u "$PROBE_DIR")"
  git -C "$UDIR" rev-parse HEAD > "$OUTD/probe-head-$1.txt"
  git -C "$UDIR" status --porcelain > "$OUTD/probe-status-$1.txt"
  git -C "$UDIR" status --porcelain --ignored > "$OUTD/probe-status-ignored-$1-run.txt"
  git -C "$UDIR" ls-remote origin > "$OUTD/probe-lsremote-$1.txt" 2>/dev/null
  pwsh -NoProfile -File "$BM/procs-snapshot.ps1" -Out "$OUTDM/procs-$1.json" -Dir "$PROBE_DIR" > /dev/null 2>&1 || echo "[]" > "$OUTD/procs-$1.json"
}

# biblioteca para el arnés de puertas del autoensayo: al cargarse con «.» solo define funciones
if [ "${BASH_SOURCE[0]}" != "$0" ]; then return 0; fi

case "${1:-}" in
  measure)
    mkdir -p "$OUT/measure"; measure "${2:?falta la etiqueta}" "$OUT/measure" ;;
  probe)
    check_gates "${2:-}"
    UDIR="$(cygpath -u "$PROBE_DIR")"
    SESS="$USERPROFILE/.codex/sessions/$(date +%Y/%m/%d)"
    $PY "$BW\\probe_tools.py" build-prompt "$BW" "$OUTDW\\probe-prompt.txt" > "$OUTD/probe-prompt-build.txt" || refuse "no se pudo construir el texto de la sonda"
    sha "$OUTD/probe-prompt.txt" > "$OUTD/probe-prompt.sha256"
    [ "$(cat "$OUTD/probe-prompt.sha256")" = "$(cat "$OUTD/gate-prompt.sha256")" ] || refuse "el texto armado al lanzar difiere del comprobado por las puertas"
    cp "$B/$SCHEMA_FILE" "$OUTD/probe-schema.used.json"; sha "$OUTD/probe-schema.used.json" > "$OUTD/probe-schema.used.sha256"
    [ -x "$RTPS/pwsh.exe" ] && sha "$RTPS/pwsh.exe" > "$OUTD/runtime-pwsh.sha256"
    measure pre-probe "$OUTD"
    $PY "$BW\\probe_tools.py" stage "$OUTDW" > "$OUTD/stage.txt" 2>&1 || refuse "no se pudo preparar el escenario S ($(tail -1 "$OUTD/stage.txt")); revisar y ejecutar unstage si escribió algo"
    ( set -o noclobber; date -u +%Y-%m-%dT%H:%M:%SZ > "$LAUNCH_MARKER" ) 2>/dev/null || {
      $PY "$BW\\probe_tools.py" unstage "$OUTDW" > "$OUTD/unstage.txt" 2>&1
      refuse "no se pudo crear el marcador de lanzamiento (¿ya lanzada?); escenario retirado"; }
    snapshot before
    ls -1 "$SESS" > "$OUTD/probe-sessions-before.txt" 2>/dev/null
    date -u +%Y-%m-%dT%H:%M:%SZ > "$OUTD/probe-start.txt"
    PROMPT="$(cat "$OUTD/probe-prompt.txt")"
    # Receta 16.4 literal (igual que en v4): binario por ruta verificada, -C, -s read-only, -m, -c effort, --output-schema, -o, --json, sin
    # --ephemeral, stdin cerrado y el pwsh del runtime primero en el PATH del proceso hijo. Solo cambia el texto (diseño v5) y el esquema.
    PATH="$RTPS:$PATH" "$CODEX" exec -C "$PROBE_DIR" -s read-only -m "$MODEL" -c "model_reasoning_effort=\"$EFFORT\"" \
      --output-schema "$BW\\$SCHEMA_FILE" -o "$OUTDW\\probe-last.json" --json "$PROMPT" \
      < /dev/null > "$OUTD/probe-events.jsonl" 2> "$OUTD/probe-stderr.txt" &
    CPID=$!
    WINPID="$(cat "/proc/$CPID/winpid" 2>/dev/null || true)"
    T=0; TIMED_OUT=no
    while kill -0 "$CPID" 2>/dev/null; do
      if [ "$T" -ge "$TIMEOUT_S" ]; then
        TIMED_OUT=yes
        [ -n "$WINPID" ] && taskkill //PID "$WINPID" //T //F > "$OUTD/probe-taskkill.txt" 2>&1
        break
      fi
      sleep 5; T=$((T+5))
    done
    wait "$CPID"; RC=$?
    echo "exit=$RC timed_out=$TIMED_OUT elapsed_s<=$T" | tee "$OUTD/probe-exit.txt"
    date -u +%Y-%m-%dT%H:%M:%SZ > "$OUTD/probe-end.txt"
    snapshot after
    SESS2="$USERPROFILE/.codex/sessions/$(date +%Y/%m/%d)"
    { ls -1 "$SESS" 2>/dev/null; [ "$SESS2" != "$SESS" ] && ls -1 "$SESS2" 2>/dev/null; } > "$OUTD/probe-sessions-after.txt"
    diff "$OUTD/probe-sessions-before.txt" "$OUTD/probe-sessions-after.txt" | grep '^>' | sed 's/^> //' > "$OUTD/probe-new-sessions.txt"
    $PY "$BW\\probe_tools.py" unstage "$OUTDW" > "$OUTD/unstage.txt" 2>&1 || echo "AVISO: unstage con diferencias; ver out/$SLUG/unstage-report.json" | tee -a "$OUTD/unstage.txt"
    echo "head $(cat "$OUTD/probe-head-before.txt") -> $(cat "$OUTD/probe-head-after.txt"); session $(cat "$OUTD/probe-new-sessions.txt")"
    measure post-probe "$OUTD"
    echo "Sin reintento, sea cual sea el resultado. Siguiente: run_probe.sh compare $MODE (y la supervisión: modelo/effort efectivos del registro de sesión)" ;;
  compare)
    mode_setup "${2:-}"
    [ -e "$LAUNCH_MARKER" ] || refuse "no hay ninguna sonda $MODE lanzada"
    load_gates
    $PY "$BW\\probe_tools.py" reference "$OUTDW" && $PY "$BW\\probe_tools.py" compare "$OUTDW" ;;
  *) echo "uso: run_probe.sh measure <tag> | probe CONTROLLER_V5 | compare CONTROLLER_V5"; exit 64 ;;
esac

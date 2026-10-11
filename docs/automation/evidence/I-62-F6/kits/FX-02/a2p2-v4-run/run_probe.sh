#!/usr/bin/env bash
# Kit a2p2-v4 — NO EJECUTADO (preparación de la supervisión, plano a). Reutiliza la maquinaria del kit a41-v3.
# Identidad nueva de codex-cli (decisiones §68; evidencia §125): BinaryHash d13914ce…/AppVersion 26.1007.2314.0/huella F909F480…
# Tres modos, cada uno de UN solo lanzamiento (marcador propio que nunca se borra) y con sus propias puertas:
#   NATIVE_CONTROLLER  sonda 1 del bloque A2-P2: Controller gpt-6-luna/high, receta 16.4 literal (sin shell declarada), -C CONTROLLER_PROBE_DIR
#   NATIVE_ARCHITECT   sonda 2 del bloque A2-P2: Architect gpt-6.1-sol/high, receta 16.4 literal, -C ARCHITECT_PROBE_DIR (clon de lectura propio)
#   CMD_A41            sonda A4-1 (fuera del bloque; A4-1, reglas 3 y 4): el v3 con el trío nuevo; solo si NATIVE_CONTROLLER muestra F6-OBS-03
# Uso:
#   run_probe.sh measure <tag>          -> solo identidad (sin modelo), en out/measure/
#   run_probe.sh probe <MODO>           -> la sonda del modo, con las puertas de gates.env
#   run_probe.sh compare <MODO>         -> después de la sonda: referencia con git y Python, y comparación (sin modelo)
# Cualquier diferencia de huella, estructura, binario o versión frente a la identidad autorizada => STOP (exit 3): la medición no vale.
set -u
B="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUT="$B/out"; mkdir -p "$OUT"; BM="$(cygpath -m "$B")"; BW="$(cygpath -w "$B")"
EXPECT_FP=F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113
EXPECT_BIN=d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec
EXPECT_APP=26.1007.2314.0
EXPECT_LABEL=2e5e00daee91c61d
EXPECT_PAIR="$EXPECT_BIN/$EXPECT_APP"
EXPECT_TRIO="$EXPECT_BIN/$EXPECT_APP/$EXPECT_FP"
BASELINE="$BM/baseline-keynames-F909F480.json"
FIXTURE_ORIGIN_URL='D:/r62-fixture/fixture-origin.git'
CODEX="$LOCALAPPDATA/OpenAI/Codex/bin/$EXPECT_LABEL/codex.exe"
RTPS="$USERPROFILE/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell"
TIMEOUT_S=600                      # AP 16.4: tope de 600 s (igual en los tres modos)
EFFORT='high'                      # Deep (Architect) y Balanced con nivel Eficiente (Controller) -> high (catálogo)
A41_PRIOR_RUN='R20261010T000542Z-a4-1-probe'   # la única A4-1 consumida (decisiones §66, punto 1)
PY="python -I"

refuse() { echo "REFUSED: $*" | tee -a "$OUT/refusals.log" >&2; exit 2; }
sha() { sha256sum "$1" | sed 's/^\\//' | cut -c1-64; }
norm() { printf '%s' "$1" | tr '\134' '/' | tr 'A-Z' 'a-z' | tr -d ' '; }
in_decisions() { tr -d '\r' < "$DECISIONS_U" | grep -Fq -- "$1"; }
line_in_decisions() {  # $1 = archivo del kit con UNA línea literal del Owner; tiene que ser una línea completa del archivo de decisiones
  [ -f "$B/$1" ] || refuse "falta $1"
  [ "$(grep -c '' "$B/$1")" = "1" ] || refuse "$1 debe tener una sola línea"
  tr -d '\r' < "$DECISIONS_U" | grep -Fxq -- "$(tr -d '\r\n' < "$B/$1")" || refuse "$1 no consta literal, como línea completa, en el archivo de decisiones"
}

mode_setup() {
  MODE="${1:-}"
  case "$MODE" in
    NATIVE_CONTROLLER) SLUG=native-controller; ROLE=controller; KIND=block; CELL='gpt-6-luna/high'; MODEL='gpt-6-luna'; EVID_PAT='*a2p2-v4-controller*' ;;
    NATIVE_ARCHITECT)  SLUG=native-architect;  ROLE=architect;  KIND=block; CELL='gpt-6.1-sol/high'; MODEL='gpt-6.1-sol'; EVID_PAT='*a2p2-v4-architect*' ;;
    CMD_A41)           SLUG=cmd-a41;           ROLE=controller; KIND=a41;   CELL='gpt-6-luna/high'; MODEL='gpt-6-luna'; EVID_PAT='*a4-1-probe-2*' ;;
    *) echo "modo inválido: '${MODE}' (NATIVE_CONTROLLER | NATIVE_ARCHITECT | CMD_A41)"; exit 64 ;;
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
  # ejecutables acompañantes del binario (codex-command-runner.exe, codex-windows-sandbox-setup.exe, codex-code-mode-host.exe): huella conjunta
  # informativa; entra en la comparación antes/después (un cambio a mitad del bloque es una actualización: A2-P2, regla 6)
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
  need_vars DECISIONS_FILE EVIDENCE_FILE EVIDENCE_ROOT CONTROLLER_PROBE_DIR ARCHITECT_PROBE_DIR BRANCH BAD_SHA CONTRACT_PATH CANONICAL_SCHEMA_MODE
  if [ "$ROLE" = controller ]; then
    need_vars BASE_SHA HEAD_SHA RED_SHA IGNORE_PATH HASH_FILES STATE_PATH DECISIONS_MD MD_LINE CLAUSE_MAP_PATH CLAUSE_SCHEMA_PATH \
              TRX_SOURCE TRX_SOURCE_SHA256 TRX_RUN_ID TRX_RUN_HEAD_SHA
    export PROBE_DIR="$CONTROLLER_PROBE_DIR"
  else
    need_vars TARGET_COMMIT TARGET_BLOB ARQ_FID_PATH ARQ_DECOY_BLOB ARQ_MISSING_PATH
    export PROBE_DIR="$ARCHITECT_PROBE_DIR"
  fi
  if [ "$KIND" = block ]; then need_vars A2P2_PAIR_MARKER A2P2_STABILITY_MARKER; fi
  [ "$MODE" = NATIVE_CONTROLLER ] && need_vars DISP_NATIVE_CONTROLLER_MARKER
  [ "$MODE" = NATIVE_ARCHITECT ] && need_vars DISP_NATIVE_ARCHITECT_MARKER
  [ "$MODE" = CMD_A41 ] && need_vars A4_AGREED_MARKER DISP_CMD_A41_MARKER
  DECISIONS_U="$(cygpath -u "$DECISIONS_FILE")"; EVIDENCE_FILE_U="$(cygpath -u "$EVIDENCE_FILE")"; EVIDENCE_U="$(cygpath -u "$EVIDENCE_ROOT")"
  case "$CANONICAL_SCHEMA_MODE" in VERBATIM) SCHEMA_FILE="probe-schema.$SLUG.json";; STRICT_PROJECTION) SCHEMA_FILE="probe-schema.$SLUG.strict-projection.json";;
    *) refuse "CANONICAL_SCHEMA_MODE inválido";; esac
  return 0
}

mode_used() {  # $1 = SLUG, $2 = patrón de la evidencia custodiada: 1 si la sonda del modo ya se lanzó (marcador local o carpeta custodiada)
  if [ -e "$OUT/$1/PROBE_LAUNCHED" ] || ls -d "$EVIDENCE_U"/$2 >/dev/null 2>&1; then echo 1; else echo 0; fi
}

check_dir_common() {  # $1 = directorio -C; $2 = el otro directorio del kit (nunca intercambiables)
  local dir="$1" other="$2" UDIR LDIR CPD
  LDIR="$(cygpath -m "$dir" | tr 'A-Z' 'a-z')"
  case "$LDIR" in
    d:/r62-fixture/a|d:/r62-fixture/b|d:/r62-fixture/b2|d:/r62-fixture/b3|d:/r62-fixture/r|d:/r62-fixture/a6|d:/r62-fixture/arch|d:/r62-fixture/arch02|\
    d:/r62-fixture/evidence-out*|d:/r62-fixture/supervisor*|d:/r62-fixture/fx05*|d:/r62-fixture/probe-od4-1|*.git)
      refuse "directorio no admitido para esta sonda ($LDIR)";;
  esac
  [ "$LDIR" != "$(cygpath -m "$other" | tr 'A-Z' 'a-z')" ] || refuse "el -C del Controller y el del Architect no pueden ser el mismo directorio"
  if [ "$ROLE" = architect ] && [ "$LDIR" = "d:/r62-fixture/a2" ]; then refuse "el Architect no usa A2 (es el -C del Controller y del titular A2 de FX-02)"; fi
  UDIR="$(cygpath -u "$dir")"
  [ -d "$UDIR/.git" ] || refuse "PROBE_DIR no es un clon con historia (el clon preparado aún no existe)"
  [ "$(git -C "$UDIR" config --get remote.origin.url)" = "$FIXTURE_ORIGIN_URL" ] || refuse "el origin del clon no es el origen del fixture"
  [ "$(git -C "$UDIR" config --get core.autocrlf)" = "false" ] || refuse "core.autocrlf != false: el SHA-256 del archivo no sería el del blob"
  [ "$(git -C "$UDIR" rev-parse --abbrev-ref HEAD)" = "$BRANCH" ] || refuse "el clon no está en la rama $BRANCH"
  [ "$(git -C "$UDIR" rev-parse HEAD)" = "$(git -C "$UDIR" ls-remote origin "refs/heads/$BRANCH" | cut -f1)" ] \
    || refuse "HEAD del clon != ls-remote origin $BRANCH: el clon preparado no está en la punta de la rama (rehacer el clon)"
  [ -z "$(git -C "$UDIR" status --porcelain --ignored)" ] || refuse "PROBE_DIR no está limpio (incluidos los ignorados): clon fresco requerido"
  git -C "$UDIR" cat-file -e "$BAD_SHA" 2>/dev/null && refuse "BAD_SHA existe en el clon: el camino de error no se ejercería"
  CPD="$USERPROFILE/.claude/projects/$(cygpath -w "$dir" | sed 's/[^A-Za-z0-9]/-/g')"
  [ ! -e "$CPD" ] || refuse "el directorio -C tiene directorio de proyecto de Claude: nunca -C con una sesión abierta o previa en él (§64, punto 2)"
  return 0
}

check_controller_dir() {
  local UDIR s f
  check_dir_common "$CONTROLLER_PROBE_DIR" "$ARCHITECT_PROBE_DIR"
  UDIR="$(cygpath -u "$CONTROLLER_PROBE_DIR")"
  for s in BASE_SHA HEAD_SHA RED_SHA BAD_SHA TRX_RUN_HEAD_SHA; do echo "${!s}" | grep -Eq '^[0-9a-f]{40}$' || refuse "$s no es un SHA completo"; done
  echo "$TRX_SOURCE_SHA256" | grep -Eq '^[0-9a-f]{64}$' || refuse "TRX_SOURCE_SHA256 inválido"
  echo "$MD_LINE" | grep -Eq '^[0-9]+$' || refuse "MD_LINE inválido"
  case "$IGNORE_PATH" in artifacts/orchestration/*) ;; *) refuse "IGNORE_PATH debe estar en artifacts/orchestration/";; esac
  case "$IGNORE_PATH" in *..*|*\\*) refuse "IGNORE_PATH con '..' o '\\'";; esac
  [ -f "$(cygpath -u "$TRX_SOURCE")" ] && [ "$(sha "$(cygpath -u "$TRX_SOURCE")")" = "$TRX_SOURCE_SHA256" ] || refuse "el TRX custodiado falta o no tiene el SHA-256 esperado"
  for s in "$BASE_SHA" "$HEAD_SHA" "$RED_SHA" "$TRX_RUN_HEAD_SHA"; do git -C "$UDIR" cat-file -e "$s^{commit}" 2>/dev/null || refuse "$s no está en la historia del clon"; done
  git -C "$UDIR" merge-base --is-ancestor "$BASE_SHA" "$RED_SHA" && git -C "$UDIR" merge-base --is-ancestor "$RED_SHA" HEAD \
    || refuse "RED_SHA no está entre BASE_SHA y HEAD"
  git -C "$UDIR" rev-parse --verify -q "refs/remotes/origin/$BRANCH" >/dev/null || refuse "el clon no tiene refs/remotes/origin/$BRANCH (Identity)"
  git -C "$UDIR" rev-parse --verify -q refs/remotes/origin/main >/dev/null || refuse "el clon no tiene refs/remotes/origin/main (Remote)"
  for f in $HASH_FILES "$CONTRACT_PATH" "$STATE_PATH" "$DECISIONS_MD" "$CLAUSE_MAP_PATH" "$CLAUSE_SCHEMA_PATH"; do
    git -C "$UDIR" cat-file -e "HEAD:$f" 2>/dev/null || refuse "$f no está en HEAD del clon"
  done
  git -C "$UDIR" check-ignore -q --no-index "$IGNORE_PATH" || refuse "IGNORE_PATH no está ignorado en el clon (falta el .gitignore de U-08)"
  return 0
}

check_architect_dir() {
  local UDIR K AR MS
  check_dir_common "$ARCHITECT_PROBE_DIR" "$CONTROLLER_PROBE_DIR"
  UDIR="$(cygpath -u "$ARCHITECT_PROBE_DIR")"
  echo "$TARGET_COMMIT" | grep -Eq '^[0-9a-f]{40}$' && echo "$TARGET_BLOB" | grep -Eq '^[0-9a-f]{40}$' && echo "$ARQ_DECOY_BLOB" | grep -Eq '^[0-9a-f]{40}$' \
    || refuse "TARGET_COMMIT, TARGET_BLOB o ARQ_DECOY_BLOB no son SHA completos"
  git -C "$UDIR" cat-file -e "$TARGET_COMMIT^{commit}" 2>/dev/null || refuse "TARGET_COMMIT no está en la historia del clon"
  git -C "$UDIR" merge-base --is-ancestor "$TARGET_COMMIT" HEAD || refuse "TARGET_COMMIT no es ancestro de HEAD"
  [ "$(git -C "$UDIR" rev-parse "$TARGET_COMMIT:$CONTRACT_PATH" 2>/dev/null)" = "$TARGET_BLOB" ] || refuse "el blob del contrato en TARGET_COMMIT no es TARGET_BLOB"
  [ "$ARQ_DECOY_BLOB" != "$TARGET_BLOB" ] || refuse "ARQ_DECOY_BLOB no puede ser el blob del objeto revisado"
  K="$(git -C "$UDIR" show "$TARGET_COMMIT:$CONTRACT_PATH")"
  AR="$(printf '%s' "$K" | $PY -c "import json,sys; print(json.loads(sys.stdin.buffer.read().decode('utf-8'))['AuthorityRevision'])")"
  MS="$(printf '%s' "$K" | $PY -c "import json,sys; print(json.loads(sys.stdin.buffer.read().decode('utf-8'))['MainSha'])")"
  git -C "$UDIR" cat-file -e "$AR^{commit}" 2>/dev/null && git -C "$UDIR" cat-file -e "$MS^{commit}" 2>/dev/null || refuse "AuthorityRevision o MainSha de K no están en la historia"
  git -C "$UDIR" cat-file -e "$AR:$ARQ_FID_PATH" 2>/dev/null || refuse "ARQ_FID_PATH no existe en AR"
  git -C "$UDIR" cat-file -e "$AR:$ARQ_MISSING_PATH" 2>/dev/null && refuse "ARQ_MISSING_PATH existe en AR: el camino de error no se ejercería"
  git -C "$UDIR" cat-file -e "HEAD:docs/automation/agent-execution/schemas/architect-review-result.v1.schema.json" 2>/dev/null || refuse "falta el esquema canónico del Architect en HEAD"
  return 0
}

check_gates() {
  mode_setup "$1"
  load_gates
  local NCELL NMARK NDIR n sv
  [ -f "$DECISIONS_U" ] || refuse "no existe DECISIONS_FILE"
  [ -x "$RTPS/pwsh.exe" ] || [ -f "$RTPS/pwsh.exe" ] || refuse "no existe el pwsh del runtime que la receta 16.4 pone primero en el PATH"
  # P-01 en vigor desde la actualización (evidencia §125): ninguna invocación con una huella sin OD-2 (A2-P2, regla 4; A4-1, regla 6)
  line_in_decisions od2-line.txt
  grep -Fq "$EXPECT_FP" "$B/od2-line.txt" && grep -Fq "$EXPECT_BIN" "$B/od2-line.txt" || refuse "od2-line.txt no nombra la huella y el binario de la identidad nueva"
  if [ "$KIND" = block ]; then
    # A2-P2, regla 3: disposición del Coordinator que nombra el par exacto ya observado con su huella resultante estable, y consumo del Owner
    [ "${#A2P2_PAIR_MARKER}" -ge 20 ] && [ "${#A2P2_STABILITY_MARKER}" -ge 20 ] || refuse "marcadores del bloque demasiado cortos para identificar un registro"
    in_decisions "$A2P2_PAIR_MARKER" || refuse "la disposición del bloque A2-P2 (par exacto) no consta en el archivo de decisiones"
    case "$A2P2_PAIR_MARKER" in *"$EXPECT_BIN"*"$EXPECT_APP"*"$EXPECT_FP"*) ;; *) refuse "la disposición del bloque no nombra BinaryHash, AppVersion y la huella estable, en ese orden";; esac
    [ -f "$EVIDENCE_FILE_U" ] && tr -d '\r' < "$EVIDENCE_FILE_U" | grep -Fq -- "$A2P2_STABILITY_MARKER" || refuse "la observación estable del par no consta en el archivo de evidencia"
    case "$A2P2_STABILITY_MARKER" in *"$EXPECT_FP"*) ;; *) refuse "el registro de estabilidad no nombra la huella F909F480…";; esac
    line_in_decisions bloque-consumo-line.txt
    grep -Fq "$EXPECT_PAIR" "$B/bloque-consumo-line.txt" || refuse "bloque-consumo-line.txt no nombra el par exacto (BinaryHash/AppVersion)"
    if [ "$MODE" = NATIVE_CONTROLLER ]; then NMARK="$DISP_NATIVE_CONTROLLER_MARKER"; sv="$CONTROLLER_PROBE_DIR"; else NMARK="$DISP_NATIVE_ARCHITECT_MARKER"; sv="$ARCHITECT_PROBE_DIR"; fi
    # P-07: como máximo dos sondas en el bloque y una por modo
    n=$(( $(mode_used native-controller '*a2p2-v4-controller*') + $(mode_used native-architect '*a2p2-v4-architect*') ))
    [ "$n" -lt 2 ] || refuse "el bloque A2-P2 de este par ya consumió sus dos sondas"
  else
    # A4-1, reglas 3 y 4: A-4 AGREED, disposición (celda, shell, trío exacto, -C), consumo del Owner del trío nuevo; solo tras NATIVE_CONTROLLER con F6-OBS-03
    in_decisions "$A4_AGREED_MARKER" || refuse "el acuerdo de A-4 no consta en el archivo de decisiones"
    NMARK="$DISP_CMD_A41_MARKER"; sv="$CONTROLLER_PROBE_DIR"
    case "$NMARK" in *cmd.exe*) ;; *) refuse "la disposición de CMD_A41 no nombra la shell cmd.exe";; esac
    case "$NMARK" in *"$EXPECT_BIN"*"$EXPECT_APP"*"$EXPECT_FP"*) ;; *) refuse "la disposición de CMD_A41 no nombra el trío nuevo (BinaryHash, AppVersion, huella)";; esac
    line_in_decisions a41-consumo-line.txt
    grep -Fq "$EXPECT_TRIO" "$B/a41-consumo-line.txt" || refuse "a41-consumo-line.txt no nombra el trío nuevo: no se usa la línea de un trío anterior"
    [ -d "$EVIDENCE_U" ] || refuse "no existe EVIDENCE_ROOT"
    n=$(ls -d "$EVIDENCE_U"/*a4-1-probe* 2>/dev/null | wc -l)
    [ "$n" = "1" ] && [ -d "$EVIDENCE_U/$A41_PRIOR_RUN" ] || refuse "A4-1: la evidencia no tiene exactamente la medición consumida $A41_PRIOR_RUN (regla 3: una más solo con un bloque nuevo)"
    local NC="$OUT/native-controller/comparison.json"
    [ -f "$NC" ] || NC="$(ls -d "$EVIDENCE_U"/*a2p2-v4-controller*/comparison.json 2>/dev/null | head -1)"
    [ -n "$NC" ] && [ -f "$NC" ] || refuse "CMD_A41 solo tras la sonda NATIVE_CONTROLLER comparada (comparison.json local o custodiado)"
    [ "$($PY -c "import json,sys; c=json.load(open(sys.argv[1],encoding='utf-8')); print('yes' if c.get('Mode')=='NATIVE_CONTROLLER' and c.get('NativeRoute',{}).get('ShellLimitationPersists') is True else 'no')" "$(cygpath -w "$NC")")" = yes ] \
      || refuse "la sonda NATIVE_CONTROLLER no muestra la limitación de shell (F6-OBS-03): CMD_A41 no se abre"
  fi
  # la disposición del modo nombra el modo, la celda y el directorio -C
  [ "${#NMARK}" -ge 20 ] || refuse "marcador de la disposición del modo demasiado corto"
  in_decisions "$NMARK" || refuse "la disposición del modo $MODE no consta en el archivo de decisiones"
  case "$NMARK" in *"$MODE"*) ;; *) refuse "la disposición no nombra el modo $MODE";; esac
  NCELL="$(norm "$NMARK")"
  case "$NCELL" in *"$(norm "$CELL")"*) ;; *) refuse "la disposición no nombra la celda $CELL";; esac
  NDIR="$(norm "$sv")"
  case "$NCELL" in *"$NDIR"|*"$NDIR"[!a-z0-9_-]*) ;; *) refuse "la disposición no fija este directorio -C ($sv)";; esac
  # un lanzamiento por modo, sin reintento, también frente a la evidencia custodiada
  [ -d "$EVIDENCE_U" ] || refuse "no existe EVIDENCE_ROOT"
  ls -d "$EVIDENCE_U"/$EVID_PAT >/dev/null 2>&1 && refuse "ya hay una medición custodiada de $MODE ($EVID_PAT): un lanzamiento por modo"
  [ ! -e "$LAUNCH_MARKER" ] || refuse "la sonda $MODE ya se lanzó: tope 1 y sin reintento"
  ls "$OUTD"/probe-* >/dev/null 2>&1 && refuse "hay salidas de una sonda anterior en out/$SLUG/"
  [ -e "$OUTD/stage-manifest.json" ] && refuse "hay un escenario S preparado en out/$SLUG/ (stage-manifest.json): ejecutar unstage y revisar"
  if [ "$ROLE" = controller ]; then check_controller_dir; else check_architect_dir; fi
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
    cp "$B/$SCHEMA_FILE" "$OUTD/probe-schema.used.json"; sha "$OUTD/probe-schema.used.json" > "$OUTD/probe-schema.used.sha256"
    [ -x "$RTPS/pwsh.exe" ] && sha "$RTPS/pwsh.exe" > "$OUTD/runtime-pwsh.sha256"
    measure pre-probe "$OUTD"
    if [ "$ROLE" = controller ]; then
      $PY "$BW\\probe_tools.py" stage "$OUTDW" > "$OUTD/stage.txt" 2>&1 || refuse "no se pudo preparar el escenario S ($(tail -1 "$OUTD/stage.txt")); revisar y ejecutar unstage si escribió algo"
    fi
    ( set -o noclobber; date -u +%Y-%m-%dT%H:%M:%SZ > "$LAUNCH_MARKER" ) 2>/dev/null || {
      [ "$ROLE" = controller ] && $PY "$BW\\probe_tools.py" unstage "$OUTDW" > "$OUTD/unstage.txt" 2>&1
      refuse "no se pudo crear el marcador de lanzamiento (¿ya lanzada?); escenario retirado"; }
    snapshot before
    ls -1 "$SESS" > "$OUTD/probe-sessions-before.txt" 2>/dev/null
    date -u +%Y-%m-%dT%H:%M:%SZ > "$OUTD/probe-start.txt"
    PROMPT="$(cat "$OUTD/probe-prompt.txt")"
    # Receta 16.4 literal: binario por ruta verificada, -C, -s read-only, -m, -c effort, --output-schema, -o, --json, sin --ephemeral,
    # stdin cerrado y el pwsh del runtime primero en el PATH del proceso hijo. En CMD_A41 cambia solo el texto (shell declarada: A4-1, regla 1).
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
    if [ "$ROLE" = controller ]; then
      $PY "$BW\\probe_tools.py" unstage "$OUTDW" > "$OUTD/unstage.txt" 2>&1 || echo "AVISO: unstage con diferencias; ver out/$SLUG/unstage-report.json" | tee -a "$OUTD/unstage.txt"
    fi
    echo "head $(cat "$OUTD/probe-head-before.txt") -> $(cat "$OUTD/probe-head-after.txt"); session $(cat "$OUTD/probe-new-sessions.txt")"
    measure post-probe "$OUTD"
    echo "Sin reintento, sea cual sea el resultado. Siguiente: run_probe.sh compare $MODE (y la supervisión: modelo/effort efectivos del registro de sesión)" ;;
  compare)
    mode_setup "${2:-}"
    [ -e "$LAUNCH_MARKER" ] || refuse "no hay ninguna sonda $MODE lanzada"
    load_gates
    $PY "$BW\\probe_tools.py" reference "$OUTDW" && $PY "$BW\\probe_tools.py" compare "$OUTDW" ;;
  *) echo "uso: run_probe.sh measure <tag> | probe <MODO> | compare <MODO>   (MODO = NATIVE_CONTROLLER | NATIVE_ARCHITECT | CMD_A41)"; exit 64 ;;
esac

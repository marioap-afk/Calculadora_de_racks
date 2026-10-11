#!/usr/bin/env bash
# Arnés de las puertas del kit a2p2-v4 (solo autoensayo). Copia el kit a WORK/k, escribe un registro SINTÉTICO (decisiones, evidencia y
# EVIDENCE_ROOT bajo WORK) y ejecuta check_gates de run_probe.sh, cargado como biblioteca, para cada caso. Nunca llama a measure ni a codex,
# nunca lee ~/.codex y solo escribe en WORK y en los dos clones temporales que recibe (un archivo ignorado que crea y borra en el caso G12).
# Uso: selftest-gates.sh <kitdir> <workdir> <clon-controller> <clon-architect> <informe.json>
set -u
KIT="$1"; WORK="$2"; CTRL="$3"; ARCH="$4"; REPORT="$5"
BIN=d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec; APP=26.1007.2314.0; FP=F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113
OLDBIN=3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68; OLDAPP=26.1002.7124.0; OLDFP=73890CA3319206B85B68A42E23E8D323C45361CFC29E06B3E116EFFF5DAD0FBF
rm -rf "$WORK"; mkdir -p "$WORK/k" "$WORK/reg/ev"
for f in run_probe.sh probe_tools.py cfg-fp.ps1 procs-snapshot.ps1 baseline-keynames-F909F480.json shell-declaration.txt od2-line.txt \
         bloque-consumo-line.txt a41-consumo-line.txt probe-prompt.native-controller.template.txt probe-prompt.native-architect.template.txt \
         probe-prompt.cmd-a41.template.txt probe-schema.native-controller.json probe-schema.native-controller.strict-projection.json \
         probe-schema.native-architect.json probe-schema.native-architect.strict-projection.json probe-schema.cmd-a41.json \
         probe-schema.cmd-a41.strict-projection.json; do cp "$KIT/$f" "$WORK/k/$f"; done
K="$WORK/k"; REG="$WORK/reg"
PAIR="BLOQUE-CODEX = AUTORIZADO bajo A2-P2 para el par exacto BinaryHash $BIN, AppVersion $APP, observado con su huella resultante estable $FP"
OLDPAIR="BLOQUE-CODEX = AUTORIZADO bajo A2-P2 para el par exacto BinaryHash $OLDBIN, AppVersion $OLDAPP, observado con su huella resultante estable $OLDFP"
STAB="Observación 2 (SINTÉTICA) del par $BIN/$APP: huella $FP sin cambios, estable"
DNC="Sonda 1 (NATIVE_CONTROLLER): celda codex-cli / gpt-6-luna / high, receta 16.4 literal sin shell declarada, directorio de medición $CTRL, escenario S sintético en su área ignorada"
DNA="Sonda 2 (NATIVE_ARCHITECT): celda codex-cli / gpt-6.1-sol / high, receta 16.4 literal sin shell declarada, directorio de medición $ARCH, clon de lectura nuevo"
DA41="CMD_A41: aplicación de A4-1 con el trío nuevo; celda codex-cli / gpt-6-luna / high; shell declarada cmd.exe; trío BinaryHash $BIN, AppVersion $APP, Fingerprint $FP; directorio de medición $CTRL"
DA41OLD="CMD_A41: aplicación de A4-1 con el trío nuevo; celda codex-cli / gpt-6-luna / high; shell declarada cmd.exe; trío BinaryHash $OLDBIN, AppVersion $OLDAPP, Fingerprint $OLDFP; directorio de medición $CTRL"
A4AG='A4-1..A4-6 = AGREED, incluidas A4-5 y A4-6.'

write_registry() {  # $1.. = nombres de líneas que se OMITEN del archivo de decisiones sintético
  local skip=" $* "
  {
    echo "# Decisiones SINTÉTICAS del arnés de puertas del kit a2p2-v4 (no es el registro real)"
    [[ "$skip" == *" od2 "* ]] || { cat "$K/od2-line.txt"; echo; }
    [[ "$skip" == *" blk "* ]] || { cat "$K/bloque-consumo-line.txt"; echo; }
    [[ "$skip" == *" a41 "* ]] || { cat "$K/a41-consumo-line.txt"; echo; }
    echo "| 2 | $PAIR |"; echo "| 2b | $OLDPAIR |"
    echo "| 3 | $DNC |"; echo "| 4 | $DNA |"; echo "| 5 | $A4AG |"; echo "| 6 | $DA41 |"; echo "| 6b | $DA41OLD |"
    echo "| 7 | Sonda 1: celda codex-cli / gpt-6-luna / high, directorio de medición $CTRL (sin modo) |"
    echo "| 8 | Sonda 1 (NATIVE_CONTROLLER): celda codex-cli / gpt-6-luna / high, directorio de medición D:\\r62-fixture\\A |"
    echo "| 9 | Sonda 2 (NATIVE_ARCHITECT): celda codex-cli / gpt-6.1-sol / high, directorio de medición D:\\r62-fixture\\A2 |"
    echo "| 10 | CMD_A41: celda codex-cli / gpt-6-luna / high; shell declarada pwsh; trío BinaryHash $BIN, AppVersion $APP, Fingerprint $FP; directorio de medición $CTRL |"
  } > "$REG/decisions.md"
  { echo "# Evidencia SINTÉTICA"; [[ "$skip" == *" stab "* ]] || echo "- $STAB"; } > "$REG/evidence.md"
}

write_gates() {  # sustituciones por caso: VAR=valor ...
  {
    echo "DECISIONS_FILE='$(cygpath -m "$REG/decisions.md")'"
    echo "EVIDENCE_FILE='$(cygpath -m "$REG/evidence.md")'"
    echo "EVIDENCE_ROOT='$(cygpath -m "$REG/ev")'"
    grep -E '^(BRANCH|BASE_SHA|HEAD_SHA|RED_SHA|BAD_SHA|IGNORE_PATH|HASH_FILES|CONTRACT_PATH|STATE_PATH|DECISIONS_MD|MD_LINE|CLAUSE_MAP_PATH|CLAUSE_SCHEMA_PATH|TRX_SOURCE|TRX_SOURCE_SHA256|TRX_RUN_ID|TRX_RUN_HEAD_SHA|TARGET_COMMIT|TARGET_BLOB|ARQ_FID_PATH|ARQ_DECOY_BLOB|ARQ_MISSING_PATH|CANONICAL_SCHEMA_MODE|A4_AGREED_MARKER)=' "$KIT/gates.env.template"
    printf "A2P2_PAIR_MARKER='%s'\nA2P2_STABILITY_MARKER='%s'\nDISP_NATIVE_CONTROLLER_MARKER='%s'\nDISP_NATIVE_ARCHITECT_MARKER='%s'\nDISP_CMD_A41_MARKER='%s'\n" \
      "$PAIR" "$STAB" "$DNC" "$DNA" "$DA41"
    printf "CONTROLLER_PROBE_DIR='%s'\nARCHITECT_PROBE_DIR='%s'\n" "$CTRL" "$ARCH"
    local kv; for kv in "$@"; do printf "%s='%s'\n" "${kv%%=*}" "${kv#*=}"; done
  } > "$K/gates.env"
}

native_cmp() {  # $1 = true|false
  mkdir -p "$K/out/native-controller"
  printf '{"Mode": "NATIVE_CONTROLLER", "NativeRoute": {"ShellLimitationPersists": %s}}\n' "$1" > "$K/out/native-controller/comparison.json"
}

RESULTS="$WORK/results.tsv"; : > "$RESULTS"
run_case() {  # $1 = nombre, $2 = modo, $3 = esperado (PASS | texto de la negativa)
  local log="$WORK/$1.log" rc
  ( . "$K/run_probe.sh"; check_gates "$2" ) > "$log" 2>&1; rc=$?
  local got; got="$(grep -m1 'REFUSED' "$log" | sed 's/^REFUSED: //')"
  local ok=no
  if [ "$3" = PASS ]; then [ "$rc" = 0 ] && ok=yes; else [ "$rc" = 2 ] && [[ "$got" == *"$3"* ]] && ok=yes; fi
  printf '%s\t%s\t%s\t%s\t%s\n' "$1" "$2" "$rc" "$ok" "${got:-PASS}" >> "$RESULTS"
}
reset() { rm -rf "$K/out" "$REG/ev"; mkdir -p "$REG/ev/R20261010T000542Z-a4-1-probe"; write_registry; write_gates "$@"; }

# ---------------------------------------------------------------- NATIVE_CONTROLLER
reset; run_case G01_pasa NATIVE_CONTROLLER PASS
reset; write_registry od2; run_case G02_sin_OD2 NATIVE_CONTROLLER "od2-line.txt no consta"
reset; write_registry blk; run_case G03_sin_consumo_bloque NATIVE_CONTROLLER "bloque-consumo-line.txt no consta"
reset "A2P2_PAIR_MARKER=$OLDPAIR"; run_case G04_par_anterior NATIVE_CONTROLLER "no nombra BinaryHash, AppVersion y la huella"
reset "DISP_NATIVE_CONTROLLER_MARKER=<literal de la disposición>"; run_case G05_marcador_plantilla NATIVE_CONTROLLER "conserva un marcador"
reset "DISP_NATIVE_CONTROLLER_MARKER=Sonda 1 (NATIVE_CONTROLLER): celda codex-cli / gpt-6-luna / high, directorio de medición D:\\r62-fixture\\A"
run_case G06_directorio_no_fijado NATIVE_CONTROLLER "no fija este directorio"
reset "DISP_NATIVE_CONTROLLER_MARKER=Sonda 1: celda codex-cli / gpt-6-luna / high, directorio de medición $CTRL (sin modo)"
run_case G07_sin_modo NATIVE_CONTROLLER "no nombra el modo"
reset; write_registry stab; run_case G08_sin_estabilidad NATIVE_CONTROLLER "observación estable"
reset; mkdir -p "$K/out/native-controller"; date > "$K/out/native-controller/PROBE_LAUNCHED"; run_case G09_ya_lanzada NATIVE_CONTROLLER "ya se lanzó"
reset; mkdir -p "$REG/ev/R20261012T000000Z-a2p2-v4-controller"; run_case G10_ya_custodiada NATIVE_CONTROLLER "medición custodiada"
reset; mkdir -p "$K/out/native-architect" "$REG/ev/R20261012T000000Z-a2p2-v4-controller"; date > "$K/out/native-architect/PROBE_LAUNCHED"
run_case G11_tope_del_bloque NATIVE_CONTROLLER "dos sondas"
reset; mkdir -p "$(cygpath -u "$CTRL")/artifacts/orchestration"; echo x > "$(cygpath -u "$CTRL")/artifacts/orchestration/residuo.tmp"
run_case G12_clon_con_ignorados NATIVE_CONTROLLER "no está limpio"
rm -rf "$(cygpath -u "$CTRL")/artifacts"
reset "ARCHITECT_PROBE_DIR=$CTRL"; run_case G13_mismo_directorio NATIVE_CONTROLLER "mismo directorio"
reset "CONTROLLER_PROBE_DIR=D:\\r62-fixture\\A" "DISP_NATIVE_CONTROLLER_MARKER=Sonda 1 (NATIVE_CONTROLLER): celda codex-cli / gpt-6-luna / high, directorio de medición D:\\r62-fixture\\A"
run_case G14_directorio_no_admitido NATIVE_CONTROLLER "no admitido"
reset; mkdir -p "$K/out/native-controller"; echo x > "$K/out/native-controller/probe-events.jsonl"; run_case G15_salidas_previas NATIVE_CONTROLLER "salidas de una sonda anterior"
reset "BAD_SHA=95bdc29d428b35d2188a10e1c5462d6caf2be4df"; run_case G16_BAD_SHA_existe NATIVE_CONTROLLER "BAD_SHA existe"
reset "CANONICAL_SCHEMA_MODE=STRICT_PROJECTION"; run_case G17_proyeccion_estricta NATIVE_CONTROLLER PASS
rm -f "$K/gates.env"; run_case G18_sin_gates NATIVE_CONTROLLER "falta gates.env"
# ---------------------------------------------------------------- NATIVE_ARCHITECT
reset; run_case A01_pasa NATIVE_ARCHITECT PASS
reset "ARCHITECT_PROBE_DIR=D:\\r62-fixture\\A2" "DISP_NATIVE_ARCHITECT_MARKER=Sonda 2 (NATIVE_ARCHITECT): celda codex-cli / gpt-6.1-sol / high, directorio de medición D:\\r62-fixture\\A2"
run_case A02_A2_no_admitido NATIVE_ARCHITECT "no usa A2"
reset "TARGET_BLOB=52a3f7d77a4df1a2e70e5d443eece3384cebd670"; run_case A03_blob_objeto NATIVE_ARCHITECT "no es TARGET_BLOB"
reset; mkdir -p "$REG/ev/R20261012T000000Z-a2p2-v4-controller" "$REG/ev/R20261012T010000Z-a2p2-v4-architect"; run_case A04_tope_del_bloque NATIVE_ARCHITECT "dos sondas"
reset "ARQ_MISSING_PATH=AGENTS.md"; run_case A05_ruta_ausente_existe NATIVE_ARCHITECT "ARQ_MISSING_PATH existe"
reset "DISP_NATIVE_ARCHITECT_MARKER=$DNC"; run_case A06_disposicion_de_otro_modo NATIVE_ARCHITECT "no nombra el modo"
reset; write_registry od2; run_case A07_sin_OD2 NATIVE_ARCHITECT "od2-line.txt no consta"
# ---------------------------------------------------------------- CMD_A41
reset; run_case C01_sin_sonda_nativa CMD_A41 "solo tras la sonda NATIVE_CONTROLLER"
reset; native_cmp false; run_case C02_sin_limitacion_de_shell CMD_A41 "no muestra la limitación"
reset; native_cmp true; run_case C03_pasa CMD_A41 PASS
reset; native_cmp true; write_registry a41; run_case C04_sin_consumo_A41 CMD_A41 "a41-consumo-line.txt no consta"
reset "DISP_CMD_A41_MARKER=CMD_A41: celda codex-cli / gpt-6-luna / high; shell declarada pwsh; trío BinaryHash $BIN, AppVersion $APP, Fingerprint $FP; directorio de medición $CTRL"
native_cmp true; run_case C05_disposicion_sin_cmd CMD_A41 "no nombra la shell cmd.exe"
reset; native_cmp true; mkdir -p "$REG/ev/R20261012T020000Z-a4-1-probe-2"; run_case C06_segunda_A41_custodiada CMD_A41 "exactamente la medición consumida"
reset "DISP_CMD_A41_MARKER=$DA41OLD"; native_cmp true; run_case C07_trio_anterior CMD_A41 "no nombra el trío nuevo"
reset; native_cmp true; write_registry od2; run_case C08_sin_OD2 CMD_A41 "od2-line.txt no consta"
reset; native_cmp true; mkdir -p "$K/out/cmd-a41"; date > "$K/out/cmd-a41/PROBE_LAUNCHED"; run_case C09_ya_lanzada CMD_A41 "ya se lanzó"

python -I - "$RESULTS" "$REPORT" "$(git -C "$(cygpath -u "$CTRL")" status --porcelain --ignored | wc -l)" "$(git -C "$(cygpath -u "$ARCH")" status --porcelain --ignored | wc -l)" <<'EOF'
import json, sys
rows = [l.rstrip('\n').split('\t') for l in open(sys.argv[1], encoding='utf-8') if l.strip()]
cases = [{'Case': r[0], 'Mode': r[1], 'ExitCode': int(r[2]), 'Pass': r[3] == 'yes', 'Result': r[4]} for r in rows]
rep = {'Harness': 'selftest-gates.sh (kit a2p2-v4)', 'NoModelInvoked': True, 'MeasureCalled': False, 'CodexHomeRead': False,
       'Cases': cases, 'AllCasesPass': all(c['Pass'] for c in cases),
       'ClonesCleanAfter': {'controller': sys.argv[3].strip() == '0', 'architect': sys.argv[4].strip() == '0'}}
json.dump(rep, open(sys.argv[2], 'w', encoding='utf-8', newline='\n'), ensure_ascii=False, indent=1)
print('gates', 'PASS' if rep['AllCasesPass'] and all(rep['ClonesCleanAfter'].values()) else 'FAIL', len(cases), 'casos')
EOF

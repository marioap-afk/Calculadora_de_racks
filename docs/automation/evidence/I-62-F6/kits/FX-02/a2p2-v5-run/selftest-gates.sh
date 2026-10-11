#!/usr/bin/env bash
# Arnés de las puertas del kit a2p2-v5 (solo autoensayo). Copia el kit a WORK/k, escribe un registro SINTÉTICO (decisiones, evidencia y
# EVIDENCE_ROOT bajo WORK, con una sonda 1 sintética) y ejecuta check_gates de run_probe.sh, cargado como biblioteca, para cada caso. Nunca llama
# a measure ni a codex, nunca lee ~/.codex y solo escribe en WORK y en el clon temporal que recibe (un archivo ignorado en G19 y una ref de
# seguimiento en G21, ambos restaurados). En G22, USERPROFILE apunta a un perfil falso bajo WORK.
# Uso: selftest-gates.sh <kitdir> <workdir> <clon-temporal> <informe.json> [<plantilla de la sonda 1 (kit a2p2-v4, native-controller)>]
set -u
KIT="$1"; WORK="$2"; CTRL="$3"; REPORT="$4"; V4T="${5:-}"
BIN=d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec; APP=26.1007.2314.0; FP=F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113
OLDBIN=3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68; OLDAPP=26.1002.7124.0; OLDFP=73890CA3319206B85B68A42E23E8D323C45361CFC29E06B3E116EFFF5DAD0FBF
P1RUN='R20261011T050156Z-a2p2-v4-controller'; P1SHA='f88d3313d57f993d3bbe72f1f7e6582982afa07b1e5a27bd1fe0bef4f89662b7'
UCTRL="$(cygpath -u "$CTRL")"
rm -rf "$WORK"; mkdir -p "$WORK/k" "$WORK/reg/ev"
for f in run_probe.sh probe_tools.py cfg-fp.ps1 procs-snapshot.ps1 baseline-keynames-F909F480.json od2-line.txt bloque-consumo-line.txt \
         probe-prompt.controller-v5.template.txt probe-schema.controller-v5.json probe-schema.controller-v5.strict-projection.json gates.env.template; do
  cp "$KIT/$f" "$WORK/k/$f"
done
K="$WORK/k"; REG="$WORK/reg"
cp "$K/probe-prompt.controller-v5.template.txt" "$WORK/template.orig"
PAIR="BLOQUE-CODEX = AUTORIZADO bajo A2-P2 para el par exacto: BinaryHash $BIN, AppVersion $APP, Fingerprint $FP"
OLDPAIR="BLOQUE-CODEX = AUTORIZADO bajo A2-P2 para el par exacto: BinaryHash $OLDBIN, AppVersion $OLDAPP, Fingerprint $OLDFP"
STAB="Par estable (SINTÉTICO): huella $FP sin cambios en las observaciones 1 y 2"
REAS='La segunda sonda disponible del bloque A2-P2 se reasigna al Controller en lugar del Architect.'
CELL="Celda: codex-cli / gpt-6-luna / high. Identidad autorizada: BinaryHash $BIN, AppVersion $APP, Fingerprint $FP"
CELLARQ="Celda: codex-cli / gpt-6.1-sol / high. Identidad autorizada: BinaryHash $BIN, AppVersion $APP, Fingerprint $FP"
CELLOLD="Celda: codex-cli / gpt-6-luna / high. Identidad autorizada: BinaryHash $OLDBIN, AppVersion $OLDAPP, Fingerprint $OLDFP"
DESIGN='Una invocación conjunta de PLAN y VERIFY, distinta de la anterior.'
U03="\`-C\` del Controller de FX-02: \`$CTRL\`"
U03OTHER='`-C` del Controller de FX-02: `D:\r62-fixture\A2`'

write_registry() {  # $1.. = nombres de líneas que se OMITEN del archivo de decisiones sintético
  local skip=" $* "
  {
    echo "# Decisiones SINTÉTICAS del arnés de puertas del kit a2p2-v5 (no es el registro real)"
    [[ "$skip" == *" od2 "* ]] || { cat "$K/od2-line.txt"; echo; }
    [[ "$skip" == *" blk "* ]] || { cat "$K/bloque-consumo-line.txt"; echo; }
    echo "| 1 | $PAIR |"; echo "| 1b | $OLDPAIR |"
    [[ "$skip" == *" reas "* ]] || echo "| 4 | Opción B. $REAS La autorización del Owner cubre hasta dos invocaciones. $CELL |"
    echo "| 4b | $CELLARQ |"; echo "| 4c | $CELLOLD |"
    [[ "$skip" == *" design "* ]] || echo "| 5 | $DESIGN |"
    echo "| 10 | $U03; $U03OTHER |"
    echo "| 11 | \`-C\` del Controller de FX-02: \`D:\\r62-fixture\\A\` |"
  } > "$REG/decisions.md"
  { echo "# Evidencia SINTÉTICA"; [[ "$skip" == *" stab "* ]] || echo "- $STAB"; } > "$REG/evidence.md"
}

write_gates() {  # sustituciones por caso: VAR=valor ...
  {
    echo "DECISIONS_FILE='$(cygpath -m "$REG/decisions.md")'"
    echo "EVIDENCE_FILE='$(cygpath -m "$REG/evidence.md")'"
    echo "EVIDENCE_ROOT='$(cygpath -m "$REG/ev")'"
    grep -E '^(BRANCH|BASE_SHA|HEAD_SHA|RED_SHA|BAD_SHA|IGNORE_PATH|HASH_FILES|CONTRACT_PATH|STATE_PATH|DECISIONS_MD|MD_LINE|U0_LINE|CLAUSE_MAP_PATH|CLAUSE_SCHEMA_PATH|TRX_SOURCE|TRX_SOURCE_SHA256|TRX_RUN_ID|TRX_RUN_HEAD_SHA|CANONICAL_SCHEMA_MODE|PROBE1_RUN|PROBE1_PROMPT_SHA|EXPECTED_TIP)=' "$KIT/gates.env.template"
    printf "A2P2_PAIR_MARKER='%s'\nA2P2_STABILITY_MARKER='%s'\nDISP_V5_REASSIGN_MARKER='%s'\nDISP_V5_CELL_MARKER='%s'\nDISP_V5_DESIGN_MARKER='%s'\nDIR_U03_MARKER='%s'\n" \
      "$PAIR" "$STAB" "$REAS" "$CELL" "$DESIGN" "$U03"
    printf "CONTROLLER_PROBE_DIR='%s'\n" "$CTRL"
    local kv; for kv in "$@"; do printf "%s='%s'\n" "${kv%%=*}" "${kv#*=}"; done
  } > "$K/gates.env"
}

probe1() {  # sonda 1 custodiada sintética: comparison.json y probe-prompt.sha256 ($1 = SHA del texto)
  mkdir -p "$REG/ev/$P1RUN"
  echo '{"Mode": "NATIVE_CONTROLLER", "Verdict": "NOT_DEMONSTRATED"}' > "$REG/ev/$P1RUN/comparison.json"
  echo "${1:-$P1SHA}" > "$REG/ev/$P1RUN/probe-prompt.sha256"
}

RESULTS="$WORK/results.tsv"; : > "$RESULTS"
run_case() {  # $1 = nombre, $2 = modo, $3 = esperado (PASS | RC64 | texto de la negativa)
  local log="$WORK/$1.log" rc
  ( . "$K/run_probe.sh"; check_gates "$2" ) > "$log" 2>&1; rc=$?
  local got; got="$(grep -m1 'REFUSED' "$log" | sed 's/^REFUSED: //')"
  local ok=no
  if [ "$3" = PASS ]; then [ "$rc" = 0 ] && ok=yes
  elif [ "$3" = RC64 ]; then [ "$rc" = 64 ] && ok=yes; got="$(head -1 "$log")"
  else [ "$rc" = 2 ] && [[ "$got" == *"$3"* ]] && ok=yes; fi
  printf '%s\t%s\t%s\t%s\t%s\n' "$1" "$2" "$rc" "$ok" "${got:-PASS}" >> "$RESULTS"
}
reset() { rm -rf "$K/out" "$REG/ev"; mkdir -p "$REG/ev"; probe1; cp "$WORK/template.orig" "$K/probe-prompt.controller-v5.template.txt"; write_registry; write_gates "$@"; }
M=CONTROLLER_V5

reset; run_case G01_pasa $M PASS
reset; write_registry od2; run_case G02_sin_OD2 $M "od2-line.txt no consta"
reset; write_registry blk; run_case G03_sin_consumo_bloque $M "bloque-consumo-line.txt no consta"
reset "A2P2_PAIR_MARKER=$OLDPAIR"; run_case G04_par_anterior $M "no nombra BinaryHash, AppVersion y la huella"
reset "DISP_V5_CELL_MARKER=<literal de la disposición>"; run_case G05_marcador_plantilla $M "conserva un marcador"
reset; write_registry stab; run_case G06_sin_estabilidad $M "observación estable"
reset; write_registry reas; run_case G07_sin_reasignacion $M "reasignación de la sonda 2"
reset "DISP_V5_CELL_MARKER=$CELLARQ"; run_case G08_celda_del_Architect $M "no nombra la celda"
reset "DISP_V5_CELL_MARKER=$CELLOLD"; run_case G09_identidad_anterior $M "identidad autorizada"
reset "DIR_U03_MARKER=$U03OTHER"; run_case G10_U03_fija_otro_directorio $M "U-03 no fija este directorio"
reset; write_registry design; run_case G11_sin_disposicion_de_diseno $M "diseño de la sonda no consta"
reset; mkdir -p "$REG/ev/R20261012T000000Z-a2p2-v4-architect"; run_case G12_bloque_agotado_por_Architect $M "consumió sus dos sondas"
reset; rm -rf "$REG/ev/$P1RUN"; run_case G13_sin_la_sonda_1_custodiada $M "presupuesto del bloque no verificable"
reset; mkdir -p "$REG/ev/R20261012T000000Z-a2p2-v4-controller-bis"; run_case G14_dos_sondas_1 $M "presupuesto del bloque no verificable"
reset; mkdir -p "$REG/ev/R20261012T010000Z-a2p2-v5-controller"; run_case G15_v5_ya_custodiada $M "ya hay una medición custodiada"
reset; mkdir -p "$K/out/controller-v5"; date > "$K/out/controller-v5/PROBE_LAUNCHED"; run_case G16_ya_lanzada $M "ya se lanzó"
reset; mkdir -p "$K/out/controller-v5"; echo x > "$K/out/controller-v5/probe-events.jsonl"; run_case G17_salidas_previas $M "salidas de una sonda anterior"
# repetición de la sonda 1: (a) el SHA custodiado de la sonda 1 coincide con el del texto armado; (b) plantilla de v4; (c) «Tiempo limitado» añadido
reset; H="$( ( . "$K/run_probe.sh"; mode_setup $M; load_gates; export PROBE_DIR; $PY "$(cygpath -w "$K")\\probe_tools.py" build-prompt "$(cygpath -w "$K")" "$(cygpath -w "$WORK")\\h.txt" ) | awk '{print $NF}')"
probe1 "$H"; run_case G18_repeticion_SHA_custodiado $M "repetición prohibida"
if [ -f "$V4T" ]; then reset; cp "$V4T" "$K/probe-prompt.controller-v5.template.txt"; run_case G19_plantilla_de_la_sonda_1 $M "no es el diseño v5"
else printf 'G19_plantilla_de_la_sonda_1	%s	0	no	SKIPPED (sin plantilla v4)
' $M >> "$RESULTS"; fi
reset; { printf 'Tiempo limitado. '; cat "$WORK/template.orig"; } > "$K/probe-prompt.controller-v5.template.txt"; run_case G20_tiempo_limitado $M "presión de tiempo"
reset; mkdir -p "$UCTRL/artifacts/orchestration"; echo x > "$UCTRL/artifacts/orchestration/residuo.tmp"
run_case G21_clon_con_ignorados $M "no está limpio"
rm -rf "$UCTRL/artifacts"
reset "EXPECTED_TIP=ae25b596a11fe3508ac60a3ef7de5dc23df971ce"; run_case G22_clon_no_en_la_punta_esperada $M "EXPECTED_TIP"
reset; OM="$(git -C "$UCTRL" rev-parse refs/remotes/origin/main)"; git -C "$UCTRL" update-ref refs/remotes/origin/main "$(git -C "$UCTRL" rev-parse "$OM^")"
run_case G23_origin_main_desfasado $M "origin/main recién obtenido"
git -C "$UCTRL" update-ref refs/remotes/origin/main "$OM"
reset "BAD_SHA=95bdc29d428b35d2188a10e1c5462d6caf2be4df"; run_case G24_BAD_SHA_existe $M "BAD_SHA existe"
reset "CONTROLLER_PROBE_DIR=D:\\r62-fixture\\A" "DIR_U03_MARKER=\`-C\` del Controller de FX-02: \`D:\\r62-fixture\\A\`"; run_case G25_directorio_no_admitido $M "no admitido"
reset "CANONICAL_SCHEMA_MODE=STRICT_PROJECTION"; run_case G26_proyeccion_estricta $M PASS
reset; run_case G27_modo_retirado NATIVE_ARCHITECT RC64
reset; rm -f "$K/gates.env"; run_case G28_sin_gates $M "falta gates.env"
# proyecto de Claude en el -C (sesión A2 activa o previa): perfil falso bajo WORK con el pwsh del runtime simulado
reset; FH="$WORK/fakehome"; mkdir -p "$FH/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell" \
  "$FH/.claude/projects/$(cygpath -w "$CTRL" | sed 's/[^A-Za-z0-9]/-/g')"
: > "$FH/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell/pwsh.exe"
( export USERPROFILE="$(cygpath -w "$FH")"; run_case G29_proyecto_de_Claude_en_el_directorio $M "proyecto de Claude" )

python -I - "$RESULTS" "$REPORT" "$(git -C "$UCTRL" status --porcelain --ignored | wc -l)" "$(git -C "$UCTRL" rev-parse refs/remotes/origin/main)" <<'EOF'
import json, sys
rows = [l.rstrip('\n').split('\t') for l in open(sys.argv[1], encoding='utf-8') if l.strip()]
cases = [{'Case': r[0], 'Mode': r[1], 'ExitCode': int(r[2]), 'Pass': r[3] == 'yes', 'Result': r[4]} for r in rows]
rep = {'Harness': 'selftest-gates.sh (kit a2p2-v5)', 'NoModelInvoked': True, 'MeasureCalled': False, 'CodexHomeRead': False,
       'Cases': cases, 'AllCasesPass': all(c['Pass'] for c in cases),
       'CloneCleanAfter': sys.argv[3].strip() == '0', 'CloneOriginMainAfter': sys.argv[4].strip()}
json.dump(rep, open(sys.argv[2], 'w', encoding='utf-8', newline='\n'), ensure_ascii=False, indent=1)
print('gates', 'PASS' if rep['AllCasesPass'] and rep['CloneCleanAfter'] else 'FAIL', sum(c['Pass'] for c in cases), '/', len(cases), 'casos')
EOF

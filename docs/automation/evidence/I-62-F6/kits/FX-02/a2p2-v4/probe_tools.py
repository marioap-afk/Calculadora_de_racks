#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# Herramientas del kit a2p2-v4 (NO EJECUTADO): bloque A2-P2 de codex-cli para el par nuevo y, si procede, la sonda A4-1 del trío nuevo.
# Reutiliza la maquinaria del kit a41-v3 (escenario S, referencia, comparación, validador y autoensayo). Sin red, sin modelo y sin leer ~/.codex.
# Solo biblioteca estándar de Python (jsonschema NO es necesario: lleva un validador propio del subconjunto de JSON Schema que usan
# los esquemas canónicos; si jsonschema está instalado, `selftest` lo usa además como contraste).
#
# Modo (variable PROBE_MODE, la fija run_probe.sh):
#   NATIVE_CONTROLLER  sonda 1 del bloque: Controller gpt-6-luna/high, receta 16.4 literal (sin shell declarada), 32 pasos del v3
#   NATIVE_ARCHITECT   sonda 2 del bloque: Architect gpt-6.1-sol/high, receta 16.4 literal, REVIEW_DESIGN del contrato de T1
#   CMD_A41            sonda A4-1 (fuera del bloque): el v3 con el trío nuevo, sin otro cambio que la corrección de git_write
#
#   build-schema <kitdir>                 genera los esquemas de salida de los tres modos (VERBATIM y STRICT_PROJECTION) desde los esquemas
#                                         canónicos del árbol HEAD de PROBE_DIR (git show; nunca del árbol de trabajo)
#   check-schema <kitdir>                 comprueba que el esquema de salida del modo incrusta los canónicos de HEAD (sin `$schema`)
#   strict-audit <schema.json>            auditoría frente al modo estricto de Structured Outputs (lo que exige y lo que documenta)
#   validate <schema.json> <inst.json> [<propiedad>]   validación local (subconjunto de JSON Schema 2020-12)
#   build-prompt <kitdir> <outfile>       arma el texto de la sonda del modo (plantilla [+ declaración de shell en CMD_A41] + gates.env)
#   stage <outdir>                        Controller: escribe el escenario S sintético en el área transitoria ignorada de PROBE_DIR
#   unstage <outdir>                      Controller: borra lo que escribió stage, comprueba que no cambió y que status --ignored se restaura
#   reference <outdir>                    DESPUÉS de la sonda: valores de referencia con git y Python (PROBE_DIR, solo lectura)
#   compare <outdir>                      compara probe-last.json con reference.json; veredicto por operación y global (y clasificación
#                                         de la celda del Architect en NATIVE_ARCHITECT)
#   audit-events <events.jsonl>           auditoría de los comandos de un registro de eventos (shell usada, firma F6-OBS-03, escrituras)
#   synth <outdir> <outfile>              salida sintética PERFECTA construida desde reference.json (solo para el autoensayo)
#   selftest <kitdir> <outdir> <report>   autoensayo del modo (stage → reference → casos → unstage); nunca invoca ningún modelo
#
# compare no decide nada: propone a la supervisión si cada operación quedó demostrada. La elegibilidad la declara el Coordinator.
import copy, datetime, hashlib, json, os, re, shutil, subprocess, sys, uuid
import xml.etree.ElementTree as ET

KIT = 'a2p2-v4'
MODES = ('NATIVE_CONTROLLER', 'NATIVE_ARCHITECT', 'CMD_A41')
MODE = os.environ.get('PROBE_MODE', '')
# rótulo de la sonda en los textos sintéticos del escenario S (en v3: «sonda A4-1»)
LABEL = {'NATIVE_CONTROLLER': 'sonda 1 del bloque A2-P2', 'NATIVE_ARCHITECT': 'sonda 2 del bloque A2-P2'}.get(MODE, 'sonda A4-1')
CONTROLLER_MODES = ('NATIVE_CONTROLLER', 'CMD_A41')
NATIVE_MODES = ('NATIVE_CONTROLLER', 'NATIVE_ARCHITECT')


def need_mode(allowed=MODES):
    if MODE not in allowed:
        sys.exit(f'PROBE_MODE inválido o ausente ({MODE!r}); admitidos aquí: {", ".join(allowed)}')
    return MODE
# ------------------------------------------------------------------ constantes del escenario S (sintético; congeladas con el kit)
UNIT, TASK = 'FX-U1', 'T1'
PLAN_RUN_ID = 'R20261009T210000Z-a41a'
WORK_RUN_ID = 'R20261009T211000Z-a41b'
VERIFY_RUN_ID = 'R20261009T212000Z-a41c'
NC_RUN_IDS = {'nc1': 'R20261009T213100Z-a4c1', 'nc2': 'R20261009T213200Z-a4c2',
              'nc3': 'R20261009T213300Z-a4c3', 'nc4': 'R20261009T213400Z-a4c4'}
ORCH = 'artifacts/orchestration/FX-U1'


def run_dir(task, rid):
    return f'{ORCH}/{task}/0/{rid}'


DELEGATION_REL = run_dir('T1', PLAN_RUN_ID) + '/delegation.json'
HANDOFF_REL = run_dir('T1', WORK_RUN_ID) + '/worker-handoff.json'
RELAY_REL = run_dir('T1', WORK_RUN_ID) + '/relay-facts.json'
TRX_CURRENT_REL = run_dir('T1', VERIFY_RUN_ID) + '/ci-current/fixture-tests.trx'
TRX_RED_REL = run_dir('T1', VERIFY_RUN_ID) + '/ci-red/fixture-tests.trx'
NC_REL = {'nc1': run_dir('T1-nc1', NC_RUN_IDS['nc1']) + '/worker-handoff.json',
          'nc2': run_dir('T1-nc2', NC_RUN_IDS['nc2']) + '/delegation.json',
          'nc3': run_dir('T1-nc3', NC_RUN_IDS['nc3']) + '/worker-handoff.json',
          'nc4': run_dir('T1-nc4', NC_RUN_IDS['nc4']) + '/delegation.json'}
NC_BASE = {'nc1': HANDOFF_REL, 'nc2': DELEGATION_REL, 'nc3': HANDOFF_REL, 'nc4': DELEGATION_REL}
# controles con invocación de la orden FX-U1-O4, punto 15 (N4, N5, N7a, N7b), con sus mutaciones exactas; N6 = NOT_APPLICABLE
N_RUN_IDS = {'N4': 'R20261009T213500Z-a4c5', 'N5': 'R20261009T213600Z-a4c6', 'N7a': 'R20261009T213700Z-a4c7', 'N7b': 'R20261009T213800Z-a4c8'}
N4_DIR = run_dir('T1-N4', N_RUN_IDS['N4'])
N_REL = {'N4': N4_DIR + '/relay-facts.json', 'N5': run_dir('T1-N5', N_RUN_IDS['N5']) + '/relay-facts.json',
         'N7a': run_dir('T1-N7a', N_RUN_IDS['N7a']) + '/worker-handoff.json',
         'N7b': run_dir('T1-N7b', N_RUN_IDS['N7b']) + '/worker-handoff.json'}
NC4_EXTRA = 'docs/automation/state/'
NC3_SUFFIX = ' GATE PASS.'
CESSION_START, OUTPUT_WRITTEN, CESSION_END = '2026-10-09T10:00:00Z', '2026-10-09T10:20:00Z', '2026-10-09T10:25:00Z'
WORKER_MODEL, WORKER_EFFORT = 'claude-sonnet-5-5', 'medium'
WORKER_TRAILER = 'Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>'
ISSUED_BY = f'EXECUTION_CONTROLLER ({LABEL}, kit {KIT})'
RED_TEST = 'Fixture.Tests.CalculatorTests.SubtractReturnsDifference'
SYNTH_NOTE = f'SINTÉTICO: preparado por el kit {KIT} para la {LABEL}; no describe ninguna invocación, entrega ni corrida real'


def sha_text(t):
    return hashlib.sha256(t.encode('utf-8')).hexdigest()


def binding_ref(role, bid, rid, name):
    return {'UnitId': UNIT, 'Scope': 'TASK', 'TaskId': TASK, 'Role': role, 'BindingId': bid,
            'Sha256': sha_text(f'{KIT}: binding sintético {role} ({LABEL})'),
            'Location': {'Kind': 'TRANSIENT', 'Path': run_dir('T1', rid) + '/' + name, 'Commit': None, 'Blob': None}}


EXECUTOR = {'Provider': 'anthropic', 'Transport': 'claude-subagent', 'Role': 'WORKER',
            'Cell': 'claude-subagent:claude-sonnet-5-5:<Effort.Semantic>',
            'BindingRef': binding_ref('WORKER', 'B20261009T205000Z-a41d', PLAN_RUN_ID, 'binding-worker.json')}
VERIFIER = {'Role': 'EXECUTION_CONTROLLER',
            'BindingRef': binding_ref('EXECUTION_CONTROLLER', 'B20261009T205500Z-a41e', VERIFY_RUN_ID, 'binding-controller.json'),
            'Actor': {'AdapterId': 'codex-cli', 'InstanceId': 'NOT_OBSERVED',
                      'InstanceIdSource': f'declaración de la {LABEL} (sin observación)', 'Assurance': 'NONE'}}

# ------------------------------------------------------------------ constantes de la sonda del Architect (NATIVE_ARCHITECT; sintéticas, congeladas)
ARQ_RUN = 'R20261011T115500Z-a2b5'
ARQ_RESULT_ID = 'AR-I20261011T120000Z-a2b4'
ARQ_LRR_ID = 'L20261011T120000Z-a2b4'
ARQ_INVOCATION_ID = 'I20261011T120000Z-a2b4'
ARQ_BINDING = {'UnitId': UNIT, 'Scope': 'TASK', 'TaskId': TASK, 'Role': 'ARCHITECT', 'BindingId': 'B20261011T115500Z-a2b5',
               'Sha256': sha_text(f'{KIT}: binding sintético ARCHITECT (sonda 2 del bloque A2-P2)'),
               'Location': {'Kind': 'TRANSIENT', 'Path': run_dir('T1', ARQ_RUN) + '/binding-architect.json', 'Commit': None, 'Blob': None}}
ARQ_IFE = {'path': run_dir('T1', ARQ_RUN) + '/input-fidelity.preflight.json',
           'blob': hashlib.sha1(f'{KIT}: input-fidelity sintético (sonda 2 del bloque A2-P2)'.encode('utf-8')).hexdigest()}
ARQ_SCHEMA_PATH = 'docs/automation/agent-execution/schemas/architect-review-result.v1.schema.json'
ARQ_DIMS = ['Actor', 'Session', 'Context', 'Provider']
# effort y nivel que exige ARCHITECTURE_REVIEW (routing §1 y §3: Deep, Equilibrado o Frontera) y la traducción del catálogo para la celda
ARQ_CELL_MODEL, ARQ_CELL_EFFORT, ARQ_CELL_LEVEL = 'gpt-6.1-sol', 'high', 'Equilibrado'
CTRL_CELL_MODEL, CTRL_CELL_EFFORT = 'gpt-6-luna', 'high'

# ------------------------------------------------------------------ constantes normativas (texto de AP en el fixture; se comprueban al usarlas)
ENTRY_HEADING = '## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)'
R_HEADING = '### 16.13 Compatibilidad de protocolos de ejecución delegada'
POINTER = 'Antes de aplicar esta sección, toda unidad aplica §16.13.'
TRAILER_LINE = 'Agent-Protocol-Normative: I-62'
ORDER = ['Termination', 'Handoff', 'Authority', 'Contract', 'Identity', 'Remote', 'Scope', 'CleanTree', 'Ci', 'Tests', 'Trailer',
         'Routing', 'FreeText', 'Denials']
RANK = {'REWORK': 1, 'BLOCKED': 2, 'STOP': 3}
GATE_TERMS = ['GATE PASS', 'GATE_PASS', 'Candidato', 'Candidate', 'FINAL_CANDIDATE_SHA', 'gate cerrado', 'gate closed', 'cierre del gate',
              'rama integrada', 'integrada en main', 'integrated into main', 'merged into main', 'lista para integrar', 'ready to merge',
              'Owner Validation APROBADA', 'Owner Validation APPROVED']
FREE_TEXT_FIELDS = ['WorkCompleted', 'Evidence', 'UnexpectedFindings', 'KnownLimitations', 'Deviations', 'OpenQuestions', 'RecommendedNextAction']
EFFORTS = ['Routine', 'Balanced', 'Deep', 'Long-horizon', 'Maximum']
# routing.md §1 (fixture, blob bba08fc4): Clase -> (perfiles admitidos, effort de partida)
ROUTING_CLASSES = {
    'Implementación mecánica': (['ROUTINE_IMPLEMENTATION'], 'Routine'),
    'Propagación repetitiva / cableado': (['ROUTINE_IMPLEMENTATION'], 'Routine'),
    'Implementación de pruebas': (['ROUTINE_IMPLEMENTATION'], 'Balanced'),
    'Documentación': (['DOCUMENTATION'], 'Routine'),
    'Caracterización': (['CHARACTERIZATION'], 'Balanced'),
    'Depuración': (['DEBUGGING'], 'Balanced'),
    'Causa raíz incierta': (['DEBUGGING'], 'Deep'),
    'Implementación transversal a capas': (['ROUTINE_IMPLEMENTATION', 'LONG_HORIZON_IMPLEMENTATION'], 'Deep'),
    'Revisión de arquitectura / conformidad adversarial': (['ARCHITECTURE_REVIEW'], 'Deep'),
    'Implementación agéntica larga': (['LONG_HORIZON_IMPLEMENTATION'], 'Long-horizon'),
    'Controller: planificación / verificación': (['CONTROLLER_PLANNING', 'CONTROLLER_VERIFICATION'], 'Balanced'),
}
CANON = {'delegation_v2': 'docs/automation/agent-execution/schemas/delegation.v2.schema.json',
         'controller_verification_v2': 'docs/automation/agent-execution/schemas/controller-verification.v2.schema.json'}
CANON_ALL = dict(CANON, architect_review_result_v1='docs/automation/agent-execution/schemas/architect-review-result.v1.schema.json')
HANDOFF_SCHEMA = 'docs/automation/agent-execution/schemas/worker-handoff.schema.json'
CLOSURE_FILES = ['AGENTS.md', 'docs/WORKFLOW.md', 'docs/AUTOMATION_PLAN.md', 'docs/automation/agent-execution/README.md',
                 'docs/automation/agent-execution/routing.md']
TRX_NS = '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'


# ------------------------------------------------------------------ utilidades
def env(k, default=None):
    v = os.environ.get(k)
    if v is None or v == '':
        if default is not None:
            return default
        sys.exit(f'falta la variable {k} (gates.env)')
    return v


def read_text(p):
    with open(p, encoding='utf-8') as f:
        return f.read()


def read_bytes(p):
    with open(p, 'rb') as f:
        return f.read()


def write_json(p, obj):
    os.makedirs(os.path.dirname(os.path.abspath(p)), exist_ok=True)
    with open(p, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(obj, f, ensure_ascii=False, indent=1)
        f.write('\n')


def json_bytes(obj):
    return (json.dumps(obj, ensure_ascii=False, indent=1) + '\n').encode('utf-8')


def sha256b(b):
    return hashlib.sha256(b).hexdigest()


def git_raw(d, *a):
    r = subprocess.run(['git', '-c', 'core.quotepath=off', '-C', d, *a], capture_output=True)
    return r.returncode, r.stdout, r.stderr


def git(d, *a):
    rc, out, err = git_raw(d, *a)
    if rc != 0:
        raise RuntimeError(f'git {" ".join(a)} -> {rc}: {err.decode("utf-8", "replace").strip()}')
    return out.decode('utf-8')


def git_rc(d, *a):
    rc, out, _ = git_raw(d, *a)
    return rc, out.decode('utf-8', 'replace')


def git_lines(d, *a):
    return [x for x in git(d, *a).splitlines() if x.strip()]


def show(d, rev, path):
    rc, out, _ = git_raw(d, 'show', f'{rev}:{path}')
    return out if rc == 0 else None


def blob_at(d, rev, path):
    rc, out = git_rc(d, 'rev-parse', '--verify', '-q', f'{rev}:{path}')
    return out.strip() if rc == 0 and out.strip() else None


def ls_tree(d, rev):
    res = {}
    for ln in git(d, 'ls-tree', '-r', '--full-tree', rev).splitlines():
        meta, path = ln.split('\t', 1)
        res[path] = meta.split()[2]
    return res


def is_anc(d, a, b):
    return git_rc(d, 'merge-base', '--is-ancestor', a, b)[0]


def yn(b):
    return 'yes' if b else 'no'


def nl(lst):
    return '\n'.join(lst)


def norm_ws(s):
    return re.sub(r'\s+', ' ', (s or '').replace('\r\n', '\n')).strip()


def nonascii(s):
    return [c for c in s if ord(c) > 127]


# ------------------------------------------------------------------ YAML mínimo (subconjunto en bloque del estado rackcad-automation-state/v2)
def _yscalar(s):
    s = s.strip()
    if s.startswith('"'):
        return json.loads(s)
    if s.startswith("'"):
        return s[1:-1].replace("''", "'")
    if ' #' in s:
        s = s.split(' #', 1)[0].rstrip()
    if s in ('null', '~', ''):
        return None
    if s == 'true':
        return True
    if s == 'false':
        return False
    if s == '[]':
        return []
    if s == '{}':
        return {}
    if re.fullmatch(r'-?[0-9]+', s):
        return int(s)
    return s


def parse_yaml(text):
    lines = []
    for raw in text.replace('\r\n', '\n').split('\n'):
        if not raw.strip() or raw.lstrip().startswith('#'):
            continue
        lines.append((len(raw) - len(raw.lstrip(' ')), raw.strip()))

    def kv(content):
        m = re.match(r'^([^:]+?):(?:\s+(.*))?$', content)
        if not m:
            raise ValueError(f'YAML no admitido: {content!r}')
        return m.group(1).strip(), (m.group(2) if m.group(2) is not None else '')

    def block(i, ind):
        if i < len(lines) and lines[i][1].startswith('- ') or (i < len(lines) and lines[i][1] == '-'):
            return seq(i, ind)
        return mapping(i, ind)

    def value_after(i, ind, rest):
        if rest.strip() != '':
            return _yscalar(rest), i + 1
        j = i + 1
        if j < len(lines) and (lines[j][0] > ind or (lines[j][0] == ind and lines[j][1].startswith('- '))):
            return block(j, lines[j][0])
        return None, j

    def mapping(i, ind, first=None):
        out = {}
        if first is not None:
            k, rest = kv(first)
            out[k], i = value_after(i, ind, rest)
        while i < len(lines) and lines[i][0] == ind and not lines[i][1].startswith('- '):
            k, rest = kv(lines[i][1])
            out[k], i = value_after(i, ind, rest)
        return out, i

    def seq(i, ind):
        out = []
        while i < len(lines) and lines[i][0] == ind and (lines[i][1].startswith('- ') or lines[i][1] == '-'):
            content = lines[i][1][2:].strip() if lines[i][1] != '-' else ''
            if content == '':
                v, i = block(i + 1, lines[i + 1][0])
                out.append(v)
            elif re.match(r'^[^"\'\[{][^:]*:(\s|$)', content):
                v, i = mapping(i, ind + 2, first=content)
                out.append(v)
            else:
                out.append(_yscalar(content))
                i += 1
        return out, i

    v, i = block(0, lines[0][0] if lines else 0)
    if i != len(lines):
        raise ValueError(f'YAML no consumido desde la línea lógica {i}')
    return v


def yget(obj, dotted):
    cur = obj
    for p in dotted.split('.'):
        if not isinstance(cur, dict) or p not in cur:
            return None
        cur = cur[p]
    return cur


def has_key_anywhere(obj, key):
    if isinstance(obj, dict):
        return key in obj or any(has_key_anywhere(v, key) for v in obj.values())
    if isinstance(obj, list):
        return any(has_key_anywhere(v, key) for v in obj)
    return False


# ------------------------------------------------------------------ validador JSON Schema (subconjunto 2020-12)
def _type_ok(inst, t):
    return {'object': isinstance(inst, dict), 'array': isinstance(inst, list), 'string': isinstance(inst, str),
            'boolean': isinstance(inst, bool), 'null': inst is None,
            'integer': isinstance(inst, int) and not isinstance(inst, bool),
            'number': isinstance(inst, (int, float)) and not isinstance(inst, bool)}.get(t, False)


def _eq(a, b):
    return type(a) == type(b) and a == b if not (isinstance(a, (int, float)) and isinstance(b, (int, float))
                                                  and not isinstance(a, bool) and not isinstance(b, bool)) else a == b


def validate(inst, schema, root=None, path='', errors=None):
    root = schema if root is None else root
    errors = [] if errors is None else errors
    if schema is True or schema == {}:
        return errors
    if schema is False:
        errors.append(f'{path or "/"}: false')
        return errors
    if '$ref' in schema:
        ref = schema['$ref']
        if not ref.startswith('#'):
            errors.append(f'{path}: $ref externo no admitido {ref}')
        else:
            tgt = root
            for part in [p for p in ref[1:].split('/') if p]:
                tgt = tgt[part.replace('~1', '/').replace('~0', '~')]
            validate(inst, tgt, root, path, errors)
    t = schema.get('type')
    if t is not None:
        ts = t if isinstance(t, list) else [t]
        if not any(_type_ok(inst, x) for x in ts):
            errors.append(f'{path or "/"}: tipo {type(inst).__name__} no es {ts}')
            return errors
    if 'enum' in schema and not any(_eq(inst, e) for e in schema['enum']):
        errors.append(f'{path or "/"}: {inst!r} fuera del enum')
    if 'const' in schema and not _eq(inst, schema['const']):
        errors.append(f'{path or "/"}: distinto de const')
    if isinstance(inst, str):
        if 'minLength' in schema and len(inst) < schema['minLength']:
            errors.append(f'{path}: minLength {schema["minLength"]}')
        if 'maxLength' in schema and len(inst) > schema['maxLength']:
            errors.append(f'{path}: maxLength {schema["maxLength"]}')
        if 'pattern' in schema and not re.search(schema['pattern'], inst):
            errors.append(f'{path}: no casa {schema["pattern"]}')
    if isinstance(inst, (int, float)) and not isinstance(inst, bool):
        if 'minimum' in schema and inst < schema['minimum']:
            errors.append(f'{path}: minimum {schema["minimum"]}')
        if 'maximum' in schema and inst > schema['maximum']:
            errors.append(f'{path}: maximum {schema["maximum"]}')
        if 'exclusiveMinimum' in schema and inst <= schema['exclusiveMinimum']:
            errors.append(f'{path}: exclusiveMinimum')
        if 'exclusiveMaximum' in schema and inst >= schema['exclusiveMaximum']:
            errors.append(f'{path}: exclusiveMaximum')
    if isinstance(inst, list):
        if 'minItems' in schema and len(inst) < schema['minItems']:
            errors.append(f'{path}: minItems {schema["minItems"]}')
        if 'maxItems' in schema and len(inst) > schema['maxItems']:
            errors.append(f'{path}: maxItems {schema["maxItems"]}')
        if schema.get('uniqueItems'):
            seen = [json.dumps(x, sort_keys=True) for x in inst]
            if len(seen) != len(set(seen)):
                errors.append(f'{path}: uniqueItems')
        if isinstance(schema.get('items'), dict):
            for i, x in enumerate(inst):
                validate(x, schema['items'], root, f'{path}/{i}', errors)
    if isinstance(inst, dict):
        for r in schema.get('required', []):
            if r not in inst:
                errors.append(f'{path or "/"}: falta {r}')
        props = schema.get('properties', {})
        for k, v in inst.items():
            if k in props:
                validate(v, props[k], root, f'{path}/{k}', errors)
            elif schema.get('additionalProperties') is False:
                errors.append(f'{path or "/"}: propiedad no admitida {k}')
            elif isinstance(schema.get('additionalProperties'), dict):
                validate(v, schema['additionalProperties'], root, f'{path}/{k}', errors)
    for kw in ('allOf', 'anyOf', 'oneOf'):
        if kw in schema:
            res = [validate(inst, s, root, path, []) for s in schema[kw]]
            ok = [r for r in res if not r]
            if kw == 'allOf' and len(ok) != len(res):
                errors.extend(e for r in res for e in r)
            if kw == 'anyOf' and not ok:
                errors.append(f'{path}: ninguna rama de anyOf')
            if kw == 'oneOf' and len(ok) != 1:
                errors.append(f'{path}: oneOf con {len(ok)} ramas válidas')
    return errors


# ------------------------------------------------------------------ esquemas: construcción y auditoría estricta
# Guía de Structured Outputs de OpenAI (consultada el 2026-10-09): tipos string, number, boolean, integer, object, array, enum, anyOf;
# string: pattern, format; number: multipleOf, maximum, exclusiveMaximum, minimum, exclusiveMinimum; array: minItems, maxItems; toda
# propiedad en required y additionalProperties:false en todo objeto; raíz objeto sin anyOf; no admitidos: allOf, not, dependentRequired,
# dependentSchemas, if, then, else; «solo para modelos ajustados» no admitidos: minLength, maxLength, pattern, format, minimum, maximum,
# multipleOf, patternProperties, minItems, maxItems. Límites: 5000 propiedades de objeto, 10 niveles, 1000 valores de enum, 120000 caracteres.
DOC_SUPPORTED = {'type', 'properties', 'required', 'additionalProperties', 'items', 'enum', 'const', 'anyOf', '$ref', '$defs', 'definitions',
                 'description', 'title', 'pattern', 'format', 'minimum', 'maximum', 'exclusiveMinimum', 'exclusiveMaximum', 'multipleOf',
                 'minItems', 'maxItems'}
DOC_UNSUPPORTED = {'allOf', 'not', 'dependentRequired', 'dependentSchemas', 'if', 'then', 'else', 'patternProperties'}
EMPIRICAL_ACCEPTED = {'type', 'properties', 'required', 'additionalProperties', 'items', 'enum', 'pattern', 'description', 'title', '$ref',
                      '$schema',  # esquemas v1 aceptados por codex exec --output-schema en el piloto de I-61 y esquemas del Architect v9-v14
                      'minLength', 'minimum'}  # A4-1 (R20261010T000542Z-a4-1-probe): delegation.v2 y controller-verification.v2 aceptados en
                                               # modo estricto con la CLI anterior (binario 3553cd6e…); con el binario nuevo, sin medir


def strict_audit(schema):
    issues, kw_count, depth_max, n_props, n_enum, str_len = [], {}, [0], [0], [0], [0]

    def walk(n, p, depth, is_root=False):
        if not isinstance(n, dict):
            return
        for k in n:
            if k in ('properties', '$defs', 'definitions'):
                continue
            if k == '$schema' and not is_root:
                issues.append(f'{p}: $schema anidado')
            kw_count[k] = kw_count.get(k, 0) + 1
        t = n.get('type')
        ts = t if isinstance(t, list) else [t]
        if 'object' in ts or 'properties' in n:
            depth_max[0] = max(depth_max[0], depth)
            if n.get('additionalProperties') is not False:
                issues.append(f'{p}: additionalProperties no es false')
            props = list((n.get('properties') or {}).keys())
            n_props[0] += len(props)
            str_len[0] += sum(len(x) for x in props)
            miss = [x for x in props if x not in (n.get('required') or [])]
            if miss:
                issues.append(f'{p}: propiedades fuera de required: {miss}')
        if 'enum' in n:
            n_enum[0] += len(n['enum'])
            str_len[0] += sum(len(str(x)) for x in n['enum'])
        for k, v in n.items():
            if k in ('properties', '$defs', 'definitions'):
                for kk, vv in v.items():
                    walk(vv, f'{p}/{k}/{kk}', depth + (1 if k == 'properties' else 0))
            elif isinstance(v, dict) and k not in ('enum', 'const'):
                walk(v, f'{p}/{k}', depth)
            elif isinstance(v, list) and k in ('anyOf', 'oneOf', 'allOf', 'prefixItems'):
                for i, x in enumerate(v):
                    walk(x, f'{p}/{k}/{i}', depth)

    root_ok = schema.get('type') == 'object' and 'anyOf' not in schema
    walk(schema, '#', 1, True)
    used = set(kw_count)
    return {
        'RootIsObjectWithoutAnyOf': root_ok,
        'StructuralIssues': issues,
        'Keywords': dict(sorted(kw_count.items())),
        'NotDocumentedAsSupported': sorted(k for k in used if k not in DOC_SUPPORTED and k != '$schema'),
        'DocumentedUnsupported': sorted(used & DOC_UNSUPPORTED),
        'NotEmpiricallyAccepted': sorted(k for k in used if k not in EMPIRICAL_ACCEPTED),
        'ObjectNestingDepth': depth_max[0], 'ObjectProperties': n_props[0], 'EnumValues': n_enum[0],
        'NamesAndEnumChars': str_len[0],
        'WithinDocumentedLimits': depth_max[0] <= 10 and n_props[0] <= 5000 and n_enum[0] <= 1000 and str_len[0] <= 120000,
    }


def canonical_from_head(d, keys=None):
    out = {}
    for k, p in CANON_ALL.items():
        if keys is not None and k not in keys:
            continue
        b = show(d, 'HEAD', p)
        if b is None:
            sys.exit(f'falta {p} en HEAD de {d}')
        out[k] = (json.loads(b.decode('utf-8')), blob_at(d, 'HEAD', p))
    return out


def embed(canon, projection):
    s = copy.deepcopy(canon)
    s.pop('$schema', None)
    removed = [0]

    def strip(n):
        if isinstance(n, dict):
            if 'minLength' in n:
                del n['minLength']
                removed[0] += 1
            for v in n.values():
                strip(v)
        elif isinstance(n, list):
            for v in n:
                strip(v)
    if projection:
        strip(s)
    return s, removed[0]


STEP_SCHEMA = {
    'type': 'array',
    'items': {'type': 'object', 'additionalProperties': False, 'required': ['id', 'commands', 'ok', 'values', 'error'],
              'properties': {'id': {'type': 'integer', 'description': 'número del paso del texto, de 1 a 32'},
                             'commands': {'type': 'array', 'items': {'type': 'string'}},
                             'ok': {'type': 'boolean'},
                             'values': {'type': 'array', 'items': {'type': 'object', 'additionalProperties': False, 'required': ['key', 'value'],
                                                                   'properties': {'key': {'type': 'string'}, 'value': {'type': 'string'}}}},
                             'error': {'type': ['string', 'null']}}}}


MODE_KEYS = {'NATIVE_CONTROLLER': ('delegation_v2', 'controller_verification_v2'),
             'CMD_A41': ('delegation_v2', 'controller_verification_v2'),
             'NATIVE_ARCHITECT': ('architect_review_result_v1',)}
MODE_STEPS = {'NATIVE_CONTROLLER': 32, 'CMD_A41': 32, 'NATIVE_ARCHITECT': 6}
MODE_SLUG = {'NATIVE_CONTROLLER': 'native-controller', 'NATIVE_ARCHITECT': 'native-architect', 'CMD_A41': 'cmd-a41'}


def probe_schema(canon, projection, mode):
    # Igual que en v3: los canónicos se incrustan como propiedades obligatorias, textuales salvo la palabra `$schema` (VERBATIM), o además sin
    # minLength (STRICT_PROJECTION). El paso lleva la descripción del número de pasos del modo.
    steps = copy.deepcopy(STEP_SCHEMA)
    steps['items']['properties']['id']['description'] = f'número del paso del texto, de 1 a {MODE_STEPS[mode]}'
    props = {'steps': steps, 'head_sha': {'type': ['string', 'null']}}
    removed = 0
    for k in MODE_KEYS[mode]:
        props[k], r = embed(canon[k][0], projection)
        removed += r
    tail = '(proyección estricta: sin minLength)' if projection else '(textuales salvo la palabra $schema)'
    desc = {'CMD_A41': f'Salida de la sonda medida de A4-1 (kit {KIT}): steps 1-32 y los dos objetos canónicos incrustados {tail}.',
            'NATIVE_CONTROLLER': f'Salida de la sonda 1 del bloque A2-P2 (kit {KIT}, Controller, receta literal): steps 1-32 y los dos objetos '
                                 f'canónicos incrustados {tail}.',
            'NATIVE_ARCHITECT': f'Salida de la sonda 2 del bloque A2-P2 (kit {KIT}, Architect, receta literal): steps 1-6 y el resultado canónico '
                                f'de la revisión incrustado {tail}.'}[mode]
    return {'type': 'object', 'description': desc, 'additionalProperties': False,
            'required': ['steps', 'head_sha'] + list(MODE_KEYS[mode]), 'properties': props}, removed


def schema_name(mode, projection):
    return f'probe-schema.{MODE_SLUG[mode]}' + ('.strict-projection' if projection else '') + '.json'


def build_schema(kit):
    d = env('PROBE_DIR')
    canon = canonical_from_head(d)
    for mode in MODES:
        for proj in (False, True):
            s, removed = probe_schema(canon, proj, mode)
            name = schema_name(mode, proj)
            write_json(os.path.join(kit, name), s)
            print(name, 'ok', 'minLength quitados:', removed, 'blobs:', {k: canon[k][1] for k in MODE_KEYS[mode]})


def schema_file_for_mode(kit):
    mode = need_mode()
    sm = env('CANONICAL_SCHEMA_MODE')
    if sm not in ('VERBATIM', 'STRICT_PROJECTION'):
        sys.exit('CANONICAL_SCHEMA_MODE inválido')
    return os.path.join(kit, schema_name(mode, sm == 'STRICT_PROJECTION'))


def check_schema(kit):
    mode = need_mode()
    d = env('PROBE_DIR')
    canon = canonical_from_head(d, MODE_KEYS[mode])
    path = schema_file_for_mode(kit)
    s = json.loads(read_text(path))
    proj = env('CANONICAL_SCHEMA_MODE') == 'STRICT_PROJECTION'
    expected, _ = probe_schema(canon, proj, mode)
    ok = s == expected
    print('schema', os.path.basename(path), 'OK' if ok else 'DIFF', {k: v[1] for k, v in canon.items()})
    sys.exit(0 if ok else 1)


# ------------------------------------------------------------------ build-prompt
def render_subs():
    if MODE == 'NATIVE_ARCHITECT':
        return {
            '{{TARGET_COMMIT}}': env('TARGET_COMMIT'), '{{TARGET_PATH}}': env('CONTRACT_PATH'), '{{TARGET_BLOB}}': env('TARGET_BLOB'),
            '{{FID_PATH}}': env('ARQ_FID_PATH'), '{{BAD_SHA}}': env('BAD_SHA'), '{{DECOY_BLOB}}': env('ARQ_DECOY_BLOB'),
            '{{MISSING_PATH}}': env('ARQ_MISSING_PATH'), '{{ARCH_SCHEMA_PATH}}': ARQ_SCHEMA_PATH, '{{RESULT_ID}}': ARQ_RESULT_ID,
            '{{LRR_ID}}': ARQ_LRR_ID, '{{INVOCATION_ID}}': ARQ_INVOCATION_ID,
            '{{BINDING_JSON}}': json.dumps(ARQ_BINDING, ensure_ascii=False), '{{IFE_JSON}}': json.dumps(ARQ_IFE, ensure_ascii=False)}
    hf = env('HASH_FILES').split()
    return {
        '{{BRANCH}}': env('BRANCH'), '{{BASE_SHA}}': env('BASE_SHA'), '{{HEAD_SHA}}': env('HEAD_SHA'), '{{RED_SHA}}': env('RED_SHA'),
        '{{BAD_SHA}}': env('BAD_SHA'), '{{CONTRACT_PATH}}': env('CONTRACT_PATH'), '{{STATE_PATH}}': env('STATE_PATH'),
        '{{DECISIONS_MD}}': env('DECISIONS_MD'), '{{MD_LINE}}': env('MD_LINE'), '{{CLAUSE_MAP_PATH}}': env('CLAUSE_MAP_PATH'),
        '{{CLAUSE_SCHEMA_PATH}}': env('CLAUSE_SCHEMA_PATH'), '{{HASH_FILES}}': ', '.join(f'`{f}`' for f in hf),
        '{{IGNORE_PATH}}': env('IGNORE_PATH'), '{{DELEGATION_PATH}}': DELEGATION_REL, '{{HANDOFF_PATH}}': HANDOFF_REL,
        '{{RELAY_PATH}}': RELAY_REL, '{{TRX_CURRENT}}': TRX_CURRENT_REL, '{{TRX_RED}}': TRX_RED_REL, '{{WORK_RUN_ID}}': WORK_RUN_ID,
        '{{PLAN_RUN_ID}}': PLAN_RUN_ID, '{{VERIFY_RUN_ID}}': VERIFY_RUN_ID, '{{ENTRY_HEADING}}': ENTRY_HEADING,
        '{{R_HEADING}}': R_HEADING, '{{POINTER}}': POINTER, '{{EXECUTOR_JSON}}': json.dumps(EXECUTOR, ensure_ascii=False),
        '{{VERIFIER_JSON}}': json.dumps(VERIFIER, ensure_ascii=False), '{{ISSUED_BY}}': ISSUED_BY,
        '{{NC1_PATH}}': NC_REL['nc1'], '{{NC2_PATH}}': NC_REL['nc2'], '{{NC3_PATH}}': NC_REL['nc3'], '{{NC4_PATH}}': NC_REL['nc4'],
        '{{N4_DIR}}': N4_DIR, '{{N5_PATH}}': N_REL['N5'], '{{N7A_PATH}}': N_REL['N7a'], '{{N7B_PATH}}': N_REL['N7b'],
    }


def build_prompt(kit, out):
    mode = need_mode()
    text = read_text(os.path.join(kit, f'probe-prompt.{MODE_SLUG[mode]}.template.txt'))
    # Solo CMD_A41 declara una shell (A4-1, regla 1). Los modos NATIVE_* usan la receta 16.4 literal: ninguna declaración ni pista de shell.
    if mode == 'CMD_A41':
        text = text.replace('{{SHELL_DECLARATION}}', read_text(os.path.join(kit, 'shell-declaration.txt')).strip())
    elif '{{SHELL_DECLARATION}}' in text or 'cmd.exe' in text:
        sys.exit('la plantilla de un modo NATIVE_* no puede declarar ni nombrar una shell')
    for k, v in render_subs().items():
        text = text.replace(k, v)
    if '{{' in text:
        sys.exit('el texto de la sonda conserva un marcador sin rellenar')
    if len(text) > 30000:
        sys.exit(f'el texto de la sonda tiene {len(text)} caracteres: excede el margen de la línea de órdenes de Windows (32767)')
    with open(out, 'w', encoding='utf-8', newline='\n') as f:
        f.write(text)
    print('prompt ok', len(text), 'caracteres', hashlib.sha256(text.encode('utf-8')).hexdigest())


# ------------------------------------------------------------------ alcance (AP 16.5 + U-19)
def norm_entry(e):
    return e[:-2] if e.endswith('/**') else e


def covers(entry, path):
    e = norm_entry(entry)
    return path == e or (e.endswith('/') and path.startswith(e))


def covered_by(path, entries):
    for e in entries:
        if covers(e, path):
            return e
    return None


def entry_subset(a_entries, b_entries):
    # A ⊆ B: cada entrada de A cubierta por alguna de B; un prefijo de A solo lo cubre un prefijo de B que lo contenga
    for a in a_entries:
        an = norm_entry(a)
        ok = False
        for b in b_entries:
            bn = norm_entry(b)
            if an.endswith('/'):
                ok = bn.endswith('/') and an.startswith(bn)
            else:
                ok = covers(b, an)
            if ok:
                break
        if not ok:
            return False, a
    return True, None


# ------------------------------------------------------------------ TRX
def trx_facts(b):
    root = ET.fromstring(b.decode('utf-8-sig'))
    c = root.find(f'{TRX_NS}ResultSummary/{TRX_NS}Counters')
    tests = [(r.get('testName'), r.get('outcome')) for r in root.findall(f'{TRX_NS}Results/{TRX_NS}UnitTestResult')]
    return {'total': c.get('total'), 'executed': c.get('executed'), 'passed': c.get('passed'), 'failed': c.get('failed'),
            'notExecuted': c.get('notExecuted'), 'tests': [t for t, _ in tests], 'failed_tests': [t for t, o in tests if o == 'Failed']}


def make_red_trx(src):
    bom = b'\xef\xbb\xbf' if src.startswith(b'\xef\xbb\xbf') else b''
    s = src[len(bom):].decode('utf-8')
    tid = str(uuid.uuid5(uuid.NAMESPACE_URL, 'a41-v3/red/test'))
    eid = str(uuid.uuid5(uuid.NAMESPACE_URL, 'a41-v3/red/exec'))
    lst = re.search(r'testListId="([0-9a-f-]+)"', s).group(1)
    reps = [
        ('?>\n', '?>\n<!-- ' + SYNTH_NOTE + '. Derivado del TRX de la corrida 37949808974 con una prueba Subtract fallida añadida. -->\n'),
        ('  </Results>', f'    <UnitTestResult executionId="{eid}" testId="{tid}" testName="{RED_TEST}" computerName="sintetico" '
                         f'duration="00:00:00.0010000" outcome="Failed" testListId="{lst}" />\n  </Results>'),
        ('  </TestDefinitions>', f'    <UnitTest name="{RED_TEST}" storage="sintetico" id="{tid}">\n      <Execution id="{eid}" />\n'
                                 f'      <TestMethod codeBase="sintetico" adapterTypeName="executor://xunit/VsTestRunner2/netcoreapp" '
                                 f'className="Fixture.Tests.CalculatorTests" name="SubtractReturnsDifference" />\n    </UnitTest>\n  </TestDefinitions>'),
        ('  </TestEntries>', f'    <TestEntry testId="{tid}" executionId="{eid}" testListId="{lst}" />\n  </TestEntries>'),
        ('<Counters total="1" executed="1" passed="1" failed="0"', '<Counters total="2" executed="2" passed="1" failed="1"'),
    ]
    for a, b in reps:
        if s.count(a) != 1:
            sys.exit(f'TRX de origen inesperado: ancla {a!r} aparece {s.count(a)} veces')
        s = s.replace(a, b)
    return bom + s.encode('utf-8')


# ------------------------------------------------------------------ stage / unstage
def clone_facts(d):
    return {'HEAD': git(d, 'rev-parse', 'HEAD').strip(), 'toplevel': git(d, 'rev-parse', '--show-toplevel').strip(),
            'origin_main': git(d, 'rev-parse', 'refs/remotes/origin/main').strip(),
            'lsremote_branch': (git(d, 'ls-remote', 'origin', f'refs/heads/{env("BRANCH")}').split() or [''])[0]}


def nc2_mutation(allowed, diff_paths):
    for p in sorted(diff_paths):
        e = covered_by(p, allowed)
        if e is None:
            continue
        if not norm_entry(e).endswith('/'):
            return [x for x in allowed if x != e], p
        others = sorted(q for q in diff_paths if q != p and covers(e, q))
        new = []
        for x in allowed:
            new.extend(others if x == e else [x])
        return new, p
    sys.exit('nc2: ninguna ruta del diff está cubierta por AllowedWriteScope; no hay mutación posible')


def build_scenario(d):
    br, base, red = env('BRANCH'), env('BASE_SHA'), env('RED_SHA')
    f = clone_facts(d)
    K = json.loads(show(d, 'HEAD', env('CONTRACT_PATH')).decode('utf-8'))
    st = parse_yaml(show(d, 'HEAD', env('STATE_PATH')).decode('utf-8'))
    diff_paths = git_lines(d, 'diff', '--name-only', f'{base}..{f["HEAD"]}')
    executor = copy.deepcopy(EXECUTOR)
    executor['Cell'] = 'claude-subagent:claude-sonnet-5-5:Balanced'
    deleg = {
        'Schema': 'rackcad-delegation/v2', 'TaskId': K['TaskId'], 'RunId': PLAN_RUN_ID, 'Initiative': yget(st, 'automation_state.initiative'),
        'Unit': K['Unit'], 'Gate': K['Gate'], 'Attempt': yget(st, 'automation_state.attempts'), 'AuthorityRevision': K['AuthorityRevision'],
        'MainSha': f['origin_main'], 'BaseSha': base, 'ExpectedBranch': br, 'ExpectedWorktree': f['toplevel'],
        'Owner': {'Kind': 'session-internal', 'Id': 'session-internal:' + PLAN_RUN_ID}, 'TaskClass': 'Implementación de pruebas',
        'Dimensions': {'Ambiguity': 'Low', 'ArchitecturalSensitivity': 'Low', 'Breadth': 'Low', 'ToolUse': 'Medium', 'FailureCost': 'Low',
                       'MechanicalRepetition': 'Low', 'Horizon': 'Low'},
        'Executor': executor, 'Model': WORKER_MODEL, 'Effort': {'Semantic': 'Balanced', 'Provider': WORKER_EFFORT},
        'PromptProfile': 'ROUTINE_IMPLEMENTATION', 'RoutingReason': f'Escenario S sintético de la {LABEL}.', 'ModelEscalationReason': None,
        'RoutingEnforcement': K['RoutingEnforcement'], 'Authorities': K['Authorities'], 'Objective': K['Objective'],
        'AllowedWriteScope': K['AllowedWriteScope'], 'ForbiddenWriteScope': K['ForbiddenWriteScope'], 'Invariants': K['Invariants'],
        'AcceptanceCriteria': ['Calculator.Subtract con su prueba: primero RED y después GREEN (escenario S).'],
        'RequiredTests': K['RequiredTests'], 'ExpectedEvidence': K['ExpectedEvidence'], 'StopConditions': K['StopConditions'],
        'MaxReworkLoops': 3, 'AttemptsRemaining': 3, 'ChainBaseSha': base, 'ChainRedSha': None, 'ChainRedFiles': [], 'CorrectionOf': None,
        'ExpectedHandoffPath': f'{ORCH}/T1/0/{{WorkRunId}}/worker-handoff.json',
        'IssuedBy': f'supervisión (escenario S sintético de la {LABEL})', 'RoleRequirements': K['RoleRequirements'],
        'SupersededCommits': K['SupersededCommits']}
    handoff = {
        'Schema': 'rackcad-worker-handoff/v1', 'TaskId': 'T1', 'RunId': WORK_RUN_ID, 'DelegationRunId': PLAN_RUN_ID, 'Initiative': UNIT,
        'Gate': K['Gate'], 'Attempt': deleg['Attempt'], 'BaseSha': base, 'RedSha': red, 'CurrentSha': f['HEAD'], 'Branch': br,
        'Worktree': f['toplevel'], 'Pushed': True, 'FilesChanged': diff_paths,
        'WorkCompleted': 'Añadida la prueba de Subtract (RED) y su implementación (GREEN) según la delegación; queda la verificación del Controller.',
        'TestsExecuted': [{'RunRef': 'local-red', 'Command': 'dotnet test tests/Fixture.Tests/Fixture.Tests.csproj --filter FullyQualifiedName~Subtract',
                           'Phase': 'RED', 'TreeSha': None, 'ResultsFile': None}],
        'TestResults': [{'RunRef': 'local-red', 'Selected': 1, 'Passed': 0, 'Failed': 1, 'Skipped': 0, 'FailedTests': [RED_TEST]}],
        'Evidence': ['Commits RED y GREEN publicados en fx/u1 (escenario S sintético).'], 'UnexpectedFindings': [],
        'KnownLimitations': [f'Escenario sintético de la {LABEL}: no describe ningún trabajo real.'], 'Deviations': [], 'OpenQuestions': [],
        'WorkerStatus': 'IMPLEMENTATION_COMPLETE', 'Disposition': 'NONE', 'TriggeredStopConditions': [],
        'RecommendedNextAction': 'Verificación del Controller.',
        'Worker': {'Provider': 'anthropic', 'ModelRequested': WORKER_MODEL, 'EffortRequested': WORKER_EFFORT, 'Trailer': WORKER_TRAILER}}
    src = read_bytes(env('TRX_SOURCE'))
    if sha256b(src) != env('TRX_SOURCE_SHA256'):
        sys.exit('el TRX custodiado no tiene el SHA-256 esperado (TRX_SOURCE_SHA256)')
    red_trx = make_red_trx(src)
    cur, redf = trx_facts(src), trx_facts(red_trx)
    hb = json_bytes(handoff)
    relay = {
        'Synthetic': SYNTH_NOTE, 'RunId': WORK_RUN_ID, 'Phase': 'WORK',
        'Cession': {'StartUtc': CESSION_START, 'EndUtc': CESSION_END, 'SessionOperated': False},
        'Outcome': {'Kind': 'COMPLETED', 'LaunchedPid': None, 'ExitCode': 0, 'TerminalEvent': 'notification', 'TreeKilled': False,
                    'DeathConfirmed': True, 'OutputPath': HANDOFF_REL, 'OutputSha256': sha256b(hb), 'OutputWrittenUtc': OUTPUT_WRITTEN,
                    'Acceptance': None},
        'Termination': {'Operation7': 'ACCREDITED'},
        'Participant': {'Observation': {'ModelRequested': WORKER_MODEL, 'ModelEffective': WORKER_MODEL, 'EffortRequested': WORKER_EFFORT,
                                        'EffortEffective': WORKER_EFFORT}},
        'Denials': [],
        'RemoteFacts': {
            'CurrentRun': {'GhRunId': int(env('TRX_RUN_ID')), 'Event': 'push', 'Ref': f'refs/heads/{br}', 'HeadSha': env('TRX_RUN_HEAD_SHA'),
                           'Status': 'completed', 'Conclusion': 'success',
                           'Jobs': [{'Name': 'fixture-build', 'Conclusion': 'success'}, {'Name': 'fixture-tests', 'Conclusion': 'success'}]},
            'RedRun': {'GhRunId': 0, 'Event': 'push', 'Ref': f'refs/heads/{br}', 'HeadSha': red, 'Status': 'completed', 'Conclusion': 'failure',
                       'Jobs': [{'Name': 'fixture-build', 'Conclusion': 'success'}, {'Name': 'fixture-tests', 'Conclusion': 'failure'}]},
            'TestArtifacts': [
                {'RunRef': f'gh-run:{env("TRX_RUN_ID")}', 'Name': 'fixture-tests.trx', 'Sha256': sha256b(src), 'Selected': int(cur['total']),
                 'Passed': int(cur['passed']), 'Failed': int(cur['failed']), 'Skipped': 0, 'FailedTests': cur['failed_tests']},
                {'RunRef': 'gh-run:0-sintetico-red', 'Name': 'fixture-tests.trx (RED sintético)', 'Sha256': sha256b(red_trx),
                 'Selected': int(redf['total']), 'Passed': int(redf['passed']), 'Failed': int(redf['failed']), 'Skipped': 0,
                 'FailedTests': redf['failed_tests']}],
            'OriginMainSha': f['origin_main'], 'LsRemoteSha': f['lsremote_branch'], 'Commands': ['(sintético)'],
            'ObtainedUtc': '2026-10-09T10:30:00Z', 'Diagnosis': None}}
    nc1 = dict(handoff, CurrentSha=env('BAD_SHA'))
    new_allowed, _ = nc2_mutation(K['AllowedWriteScope'], diff_paths)
    nc2 = dict(deleg, AllowedWriteScope=new_allowed)
    nc3 = dict(handoff, WorkCompleted=handoff['WorkCompleted'] + NC3_SUFFIX)
    nc4 = dict(deleg, AllowedWriteScope=K['AllowedWriteScope'] + [NC4_EXTRA])
    n5 = copy.deepcopy(relay)
    for a in n5['RemoteFacts']['TestArtifacts']:
        if a['RunRef'] == f'gh-run:{env("TRX_RUN_ID")}':
            a.update({'Selected': 0, 'Passed': 0, 'Failed': 0, 'Skipped': 0, 'FailedTests': []})
    n7a = dict(handoff, RunId=PLAN_RUN_ID)
    n7b = dict(handoff, RunId=PLAN_RUN_ID, CurrentSha=red)
    files = {DELEGATION_REL: json_bytes(deleg), HANDOFF_REL: hb, RELAY_REL: json_bytes(relay), TRX_CURRENT_REL: src,
             TRX_RED_REL: red_trx, NC_REL['nc1']: json_bytes(nc1), NC_REL['nc2']: json_bytes(nc2), NC_REL['nc3']: json_bytes(nc3),
             NC_REL['nc4']: json_bytes(nc4), N_REL['N4']: json_bytes(relay), N_REL['N5']: json_bytes(n5), N_REL['N7a']: json_bytes(n7a),
             N_REL['N7b']: json_bytes(n7b)}
    # los insumos del escenario tienen que ser válidos contra sus esquemas del fixture
    for rel, sp in ((DELEGATION_REL, CANON['delegation_v2']), (HANDOFF_REL, HANDOFF_SCHEMA)):
        errs = validate(json.loads(files[rel].decode('utf-8')), json.loads(show(d, 'HEAD', sp).decode('utf-8')))
        if errs:
            sys.exit(f'escenario S inválido ({rel}): {errs[:5]}')
    return files, f


def stage(outdir):
    need_mode(CONTROLLER_MODES)
    d = env('PROBE_DIR')
    os.makedirs(outdir, exist_ok=True)
    if git(d, 'status', '--porcelain').strip():
        sys.exit('stage: árbol sucio en PROBE_DIR')
    if git_rc(d, 'check-ignore', '-q', '--no-index', f'{ORCH}/x')[0] != 0:
        sys.exit('stage: artifacts/orchestration/ no está ignorado en PROBE_DIR')
    if os.path.exists(os.path.join(d, *ORCH.split('/'))):
        sys.exit(f'stage: {ORCH} ya existe en PROBE_DIR; no se escribe encima')
    if os.path.exists(os.path.join(outdir, 'stage-manifest.json')):
        sys.exit('stage: ya hay un stage-manifest.json en outdir')
    before = git(d, 'status', '--porcelain', '--ignored')
    with open(os.path.join(outdir, 'stage-status-ignored-before.txt'), 'w', encoding='utf-8', newline='\n') as fh:
        fh.write(before)
    files, facts = build_scenario(d)
    created = []
    for rel in files:
        parts = rel.split('/')[:-1]
        for i in range(1, len(parts) + 1):
            p = os.path.join(d, *parts[:i])
            if not os.path.isdir(p):
                os.makedirs(p)
                created.append('/'.join(parts[:i]))
    manifest = {'Kit': KIT, 'StageHead': facts['HEAD'], 'Files': [], 'CreatedDirs': created}
    os.makedirs(os.path.join(outdir, 'stage'), exist_ok=True)
    for i, (rel, b) in enumerate(files.items(), 1):
        with open(os.path.join(d, *rel.split('/')), 'wb') as fh:
            fh.write(b)
        # copia plana (sin la jerarquía del área transitoria): evita MAX_PATH bajo la ruta de custodia del kit
        name = f'{i:02d}-{rel.split("/")[-1]}'
        with open(os.path.join(outdir, 'stage', name), 'wb') as fh:
            fh.write(b)
        manifest['Files'].append({'Path': rel, 'Copy': name, 'Sha256': sha256b(b), 'Bytes': len(b)})
    write_json(os.path.join(outdir, 'stage-manifest.json'), manifest)
    after = git(d, 'status', '--porcelain', '--ignored')
    with open(os.path.join(outdir, 'stage-status-ignored-after.txt'), 'w', encoding='utf-8', newline='\n') as fh:
        fh.write(after)
    if git(d, 'status', '--porcelain').strip():
        sys.exit('stage: lo escrito no quedó ignorado (status --porcelain no vacío)')
    print('stage ok', len(files), 'archivos;', len(created), 'directorios creados')


def unstage(outdir):
    need_mode(CONTROLLER_MODES)
    d = env('PROBE_DIR')
    m = json.loads(read_text(os.path.join(outdir, 'stage-manifest.json')))
    changed, missing = [], []
    for fobj in m['Files']:
        p = os.path.join(d, *fobj['Path'].split('/'))
        if not os.path.exists(p):
            missing.append(fobj['Path'])
            continue
        if sha256b(read_bytes(p)) != fobj['Sha256']:
            changed.append(fobj['Path'])
        os.remove(p)
    leftovers = []
    for rel in reversed(m['CreatedDirs']):
        p = os.path.join(d, *rel.split('/'))
        if os.path.isdir(p):
            if os.listdir(p):
                leftovers.append(rel)
            else:
                os.rmdir(p)
    after = git(d, 'status', '--porcelain', '--ignored')
    with open(os.path.join(outdir, 'stage-status-ignored-after-unstage.txt'), 'w', encoding='utf-8', newline='\n') as fh:
        fh.write(after)
    restored = after == read_text(os.path.join(outdir, 'stage-status-ignored-before.txt'))
    rep = {'FilesChangedDuringRun': changed, 'FilesMissing': missing, 'NonEmptyDirsLeft': leftovers, 'StatusIgnoredRestored': restored}
    write_json(os.path.join(outdir, 'unstage-report.json'), rep)
    ok = not changed and not missing and not leftovers and restored
    print('unstage', 'ok' if ok else 'DIFF', json.dumps(rep, ensure_ascii=False))
    sys.exit(0 if ok else 1)


def staged_files(outdir):
    m = json.loads(read_text(os.path.join(outdir, 'stage-manifest.json')))
    out = {}
    for fobj in m['Files']:
        b = read_bytes(os.path.join(outdir, 'stage', fobj['Copy']))
        if sha256b(b) != fobj['Sha256']:
            sys.exit(f'la copia de {fobj["Path"]} en outdir/stage no es la del manifiesto')
        out[fobj['Path']] = b
    return out, m


# ------------------------------------------------------------------ Markdown: encabezados y secciones (AP 16.3 / 16.13)
def md_lines(text):
    return text.replace('\r\n', '\n').split('\n')


def md_headings(text):
    heads, fence = [], False
    for i, l in enumerate(md_lines(text)):
        if l.startswith('```'):
            fence = not fence
            continue
        m = re.match(r'^(#{1,6}) ', l)
        if m and not fence:
            heads.append((i, len(m.group(1)), norm_ws(l)))
    return heads


def md_sections(text):
    ls = md_lines(text)
    heads = md_headings(text)
    secs, dup = {}, []
    for idx, (i, lvl, h) in enumerate(heads):
        end = len(ls)
        for j, lvl2, _ in heads[idx + 1:]:
            if lvl2 <= lvl:
                end = j
                break
        if h in secs:
            dup.append(h)
        secs[h] = norm_ws('\n'.join(ls[i:end]))
    first2 = next((i for i, lvl, _ in heads if lvl == 2), len(ls))
    secs['preámbulo'] = norm_ws('\n'.join(ls[:first2]))
    return secs, dup


def section_body(text, heading):
    ls = md_lines(text)
    heads = md_headings(text)
    for idx, (i, lvl, h) in enumerate(heads):
        if h == norm_ws(heading):
            end, sub = len(ls), None
            for j, lvl2, _ in heads[idx + 1:]:
                if sub is None:
                    sub = j
                if lvl2 <= lvl:
                    end = j
                    break
            return ls[i + 1:end], ls[i + 1:(sub if sub is not None else end)]
    return None, None


# ------------------------------------------------------------------ reference
def evaluate_i62(d, K, base, st_base):
    main, ar = K['MainSha'], K['AuthorityRevision']
    r = {}
    tcs = [c for c in git_lines(d, 'rev-list', main)
           if TRAILER_LINE in [x.strip() for x in git(d, 'log', '-1', '--format=%B', c).splitlines()]]
    fpm = git_lines(d, 'rev-list', '--first-parent', '--merges', main)
    eff = None
    if len(tcs) == 1:
        for mrg in reversed(fpm):
            p1, p2 = git(d, 'rev-parse', f'{mrg}^1').strip(), git(d, 'rev-parse', f'{mrg}^2').strip()
            if is_anc(d, tcs[0], p2) == 0 and is_anc(d, tcs[0], p1) != 0:
                eff = mrg
                break
    if eff is None:
        sys.exit('referencia: EFF no derivable en el fixture (inesperado)')
    e1, e2 = git(d, 'rev-parse', f'{eff}^1').strip(), git(d, 'rev-parse', f'{eff}^2').strip()
    body = git(d, 'show', '-s', '--format=%B', eff).splitlines()
    hi = next((i for i, x in enumerate(body) if x.startswith('Initiative |')), None)
    rows = [x for x in body[hi + 1:] if '|' in x] if hi is not None else []
    cid = yget(st_base, 'automation_state.claim_id')
    r.update({'trailer-commits': tcs, 'fp-merges': fpm, 'EFF': eff, 'EFF^1': e1, 'EFF^2': e2, 'pre-rows': len(rows),
              'cid-in-pre': yn(any(cid in x for x in rows))})
    dblob = yget(st_base, 'protocol.g0_acceptance.decision.blob')
    rc, typ = git_rc(d, 'cat-file', '-t', dblob)
    dtext = git(d, 'cat-file', '-p', dblob) if rc == 0 else ''
    markers = all(x in dtext for x in ('I62-CLASSIFICATION: I62', 'I62-DELEGATED-EXECUTION: I62_DELEGATED', cid))
    if any(cid in x for x in rows):
        P = 'I61'
    elif (st_base.get('schema') == 'rackcad-automation-state/v2' and yget(st_base, 'protocol.set') == 'rackcad-protocol/I62'
          and yget(st_base, 'protocol.effective_sha') == eff and yget(st_base, 'protocol.basis.claim_id') == cid):
        g0 = yget(st_base, 'protocol.g0_acceptance.state')
        P = 'PENDING_G0' if g0 == 'PENDING' else ('I62' if g0 == 'ACCEPTED' and rc == 0 and markers else 'UNKNOWN')
    else:
        P = 'UNKNOWN'
    r.update({'classify:decision-blob-type': typ.strip() if rc == 0 else f'rc={rc}', 'classify:markers': yn(markers), 'classify:P': P})
    wf = show(d, main, 'docs/WORKFLOW.md').decode('utf-8')
    ap = show(d, main, 'docs/AUTOMATION_PLAN.md').decode('utf-8')
    eh = [i for i, _, h in md_headings(wf) if h == norm_ws(ENTRY_HEADING)]
    rh = [i for i, _, h in md_headings(ap) if h == norm_ws(R_HEADING)]
    entry_lines, _ = section_body(wf, ENTRY_HEADING)
    r.update({'workflow-blob': blob_at(d, main, 'docs/WORKFLOW.md'), 'ap-blob': blob_at(d, main, 'docs/AUTOMATION_PLAN.md'),
              'entry-count': len(eh), 'entry-line': eh[0] + 1 if eh else 0, 'r-count': len(rh), 'r-line': rh[0] + 1 if rh else 0,
              'entry-names-r': yn(entry_lines is not None and R_HEADING in '\n'.join(entry_lines))})
    # Validate(M) en EFF
    mp, sp = env('CLAUSE_MAP_PATH'), env('CLAUSE_SCHEMA_PATH')
    mb_ = show(d, eff, mp)
    sb_ = show(d, eff, sp)
    M = None
    mv = {}
    try:
        M = json.loads(mb_.decode('utf-8'))
        mv['mv1'] = 'pass' if sb_ is not None and not validate(M, json.loads(sb_.decode('utf-8'))) else 'fail'
    except Exception:
        mv['mv1'] = 'fail'
    m = re.search(r'\*\*Superficies\*\* \(lista cerrada\):(.*?)\n\n', ap, re.S)
    closed = re.findall(r'`([^`]+)`', m.group(1)) if m else []
    mv['mv2'] = 'pass' if M and M.get('Surfaces') == closed and closed else 'fail'
    t1, t2 = ls_tree(d, e1), ls_tree(d, eff)
    surf = (M or {}).get('Surfaces', closed)
    under = lambda p: any(p == s or (s.endswith('/') and p.startswith(s)) for s in surf)
    allp = sorted(set(t1) | set(t2))
    changed = [p for p in allp if under(p) and t1.get(p) != t2.get(p) and p != mp]
    deleted = [p for p in changed if p in t1 and p not in t2]
    fpaths = [x['Path'] for x in (M or {}).get('Files', [])]
    mv['mv3-changed'] = changed
    mv['mv3'] = 'pass' if M and not deleted and len(fpaths) == len(set(fpaths)) and set(changed) == set(fpaths) else 'fail'
    mism = 0
    for x in (M or {}).get('Files', []):
        p, k = x['Path'], x['FileKind']
        if k == 'MODIFIED':
            ok = x['BaseBlob'] == t1.get(p) and x['EffBlob'] == t2.get(p) and t1.get(p) is not None and t2.get(p) is not None
        else:
            ok = x['BaseBlob'] is None and x['EffBlob'] == t2.get(p) and t2.get(p) is not None and (k != 'ENTRY' or p == sp)
        mism += 0 if ok else 1
    mv['mv4-mismatches'] = mism
    mv['mv4'] = 'pass' if M and mism == 0 else 'fail'
    modified = [x['Path'] for x in (M or {}).get('Files', []) if x['FileKind'] == 'MODIFIED']
    ents = (M or {}).get('Entries', [])
    keys = [(e['Path'], norm_ws(e['Section'])) for e in ents]
    mv['mv5'] = 'pass' if M and len(keys) == len(set(keys)) and all(e['Path'] in modified for e in ents) else 'fail'
    mv6_ok = bool(M)
    entry_set = set()
    for p in modified:
        b1, b2 = show(d, e1, p), show(d, eff, p)
        mv[f'mv6-identical:{p}'] = 'missing' if b1 is None or b2 is None else yn(t1.get(p) == t2.get(p))
        s1, d1 = md_sections(b1.decode('utf-8')) if b1 is not None else ({}, [])
        s2, d2 = md_sections(b2.decode('utf-8')) if b2 is not None else ({}, [])
        if b1 is None:
            s1 = {}
        if b2 is None:
            s2 = {}
        if d1 or d2:
            mv6_ok = False
        der_mod = {h for h in set(s1) & set(s2) if s1[h] != s2[h]}
        der_rem = set(s1) - set(s2)
        der_add = set(s2) - set(s1)
        mine = [e for e in ents if e['Path'] == p]
        mm = {norm_ws(e['Section']) for e in mine if e['Kind'] == 'MODIFIED'}
        mr = {norm_ws(e['Section']) for e in mine if e['Kind'] == 'REMOVED'}
        ma = {norm_ws(e['Section']) for e in mine if e['Kind'] in ('ADDED', 'ENTRY')}
        entry_set |= {(p, norm_ws(e['Section'])) for e in mine if e['Kind'] == 'ENTRY'}
        if mm != der_mod or mr != der_rem or ma != der_add:
            mv6_ok = False
    if entry_set != {('docs/AUTOMATION_PLAN.md', norm_ws(R_HEADING)), ('docs/WORKFLOW.md', norm_ws(ENTRY_HEADING))}:
        mv6_ok = False
    mv['mv6'] = 'pass' if mv6_ok else 'fail'
    wf_e = show(d, eff, 'docs/WORKFLOW.md').decode('utf-8')
    ap_e, ap_1 = show(d, eff, 'docs/AUTOMATION_PLAN.md').decode('utf-8'), show(d, e1, 'docs/AUTOMATION_PLAN.md')
    ap_1 = ap_1.decode('utf-8') if ap_1 is not None else ''
    e_once = len([1 for _, _, h in md_headings(wf_e) if h == norm_ws(ENTRY_HEADING)]) == 1
    eb, _ = section_body(wf_e, ENTRY_HEADING)
    names = eb is not None and R_HEADING in '\n'.join(eb)
    _, s16_intro = section_body(ap_e, '## 16. Ejecución delegada bajo orden del Coordinator')
    ptr16 = s16_intro is not None and norm_ws('\n'.join(s16_intro)).startswith(POINTER)
    s163_e, _ = section_body(ap_e, '### 16.3 Lectura de autoridades')
    s163_1, _ = section_body(ap_1, '### 16.3 Lectura de autoridades')
    t163e = norm_ws('\n'.join(s163_e or []))
    ptr163 = t163e.endswith(POINTER)
    stripped = t163e[:-len(POINTER)].strip() if ptr163 else t163e
    eq163 = s163_1 is not None and stripped == norm_ws('\n'.join(s163_1))
    mv.update({'mv7-pointer-16': yn(ptr16), 'mv7-pointer-163': yn(ptr163), 'mv7-163-equal': yn(eq163),
               'mv7': 'pass' if e_once and names and ptr16 and ptr163 and eq163 else 'fail'})
    failed = [f'MV-{i}' for i in range(1, 8) if mv[f'mv{i}'] != 'pass']
    mv.update({'map-blob': blob_at(d, eff, mp), 'schema-blob': blob_at(d, eff, sp), 'map-verdict': 'MAP_VALID' if not failed else 'MAP_INVALID',
               'mv-failed': ','.join(failed)})
    r.update(mv)
    # Resolve, pasos 3-5 (forma I62)
    mbs = git(d, 'merge-base', main, ar).strip()
    ext = [a['Path'] for a in K['Authorities'] if a['Class'] == 'EXTERNAL']
    uch = [a['Path'] for a in K['Authorities'] if a['Class'] == 'UNIT_CHANGE']
    r['res:mb'] = mbs
    for p in ext:
        r[f'res:ext-diff:{p}'] = git_lines(d, 'diff', '--name-only', mbs, ar, '--', p)
    abd = git_lines(d, 'diff', '--name-only', ar, base)
    r['res:ar-base-diff'] = abd
    r['res:ar-base-clean'] = yn(not any(p in ext or p in uch for p in abd))
    authority_ok = (r['map-verdict'] == 'MAP_VALID' and P == 'I62' and len(eh) == 1 and len(rh) == 1 and r['entry-names-r'] == 'yes'
                    and all(not r[f'res:ext-diff:{p}'] for p in ext) and r['res:ar-base-clean'] == 'yes' and is_anc(d, ar, base) == 0)
    resolution = [{'Path': a['Path'], 'Section': a['Section'], 'Class': a['Class'], 'Mode': 'NORMAL',
                   'RevisionSha': ar if a['Class'] in ('UNIT_DOC', 'UNIT_CHANGE') else main, 'Parts': [], 'EffectiveSha': eff,
                   'ClauseMapBlob': r['map-blob'], 'UnitProtocol': 'rackcad-protocol/I62'} for a in K['Authorities']]
    return r, authority_ok, resolution


def routing_effort(task_class, dims):
    if task_class not in ROUTING_CLASSES:
        return None, None
    profiles, start = ROUTING_CLASSES[task_class]
    i = EFFORTS.index(start)
    up = any(dims.get(k) == 'High' for k in ('Ambiguity', 'ArchitecturalSensitivity', 'FailureCost'))
    down = dims.get('MechanicalRepetition') == 'High'
    if up and not down and i < EFFORTS.index('Deep'):
        i += 1
    elif down and not up and i > 0:
        i -= 1
    if dims.get('Horizon') == 'High':
        i = max(i, EFFORTS.index('Long-horizon'))
    return profiles, EFFORTS[i]


def compute_checks(d, K, deleg, handoff, relay, files, authority_ok, live):
    rf = relay['RemoteFacts']
    base, cur, red, br = handoff['BaseSha'], handoff['CurrentSha'], handoff['RedSha'], deleg['ExpectedBranch']
    res, disp = {}, {}
    res['Termination'] = 'pass' if (relay['Outcome']['Kind'] == 'COMPLETED' and relay['Outcome']['DeathConfirmed'] is True
                                    and relay.get('Termination', {}).get('Operation7') == 'ACCREDITED') else 'fail'
    disp['Termination'] = 'BLOCKED'
    hpath = deleg['ExpectedHandoffPath'].replace('{WorkRunId}', WORK_RUN_ID)
    hschema = json.loads(show(d, 'HEAD', HANDOFF_SCHEMA).decode('utf-8'))
    hb = files.get(hpath)
    fields_ok = (handoff['TaskId'] == deleg['TaskId'] and handoff['Attempt'] == deleg['Attempt'] and handoff['DelegationRunId'] == deleg['RunId']
                 and handoff['RunId'] == WORK_RUN_ID)
    present = hb is not None
    h_ok = (present and all(k in handoff for k in hschema['required']) and fields_ok and sha256b(hb) == relay['Outcome']['OutputSha256']
            and relay['Outcome']['OutputWrittenUtc'] > relay['Cession']['StartUtc'])
    res['Handoff'] = 'pass' if h_ok else 'fail'
    res['Authority'] = 'pass' if authority_ok else 'fail'
    disp['Authority'] = 'STOP'
    a3, _ = entry_subset(deleg['AllowedWriteScope'], K['AllowedWriteScope'])
    a4, _ = entry_subset(K['ForbiddenWriteScope'], deleg['ForbiddenWriteScope'])
    a5 = (all(x in deleg['Invariants'] for x in K['Invariants'])
          and all(any(s['Id'] == k['Id'] for s in deleg['StopConditions']) for k in K['StopConditions'])
          and all(a in deleg['Authorities'] for a in K['Authorities'])
          and all(any(t['Project'] == k['Project'] and t['Filter'] == k['Filter'] and t['MinSelected'] >= k['MinSelected']
                      and t['ExpectRed'] == k['ExpectRed'] for t in deleg['RequiredTests']) for k in K['RequiredTests']))
    order_ind = {'NOT_REQUIRED': 0, 'PREFERRED': 1, 'REQUIRED': 2}
    g4 = True
    for kr in K['RoleRequirements']:
        dr = [x for x in deleg['RoleRequirements'] if x['Role'] == kr['Role']]
        if len(dr) != 1 or not set(kr['Mandatory']) <= set(dr[0]['Mandatory']):
            g4 = False
            continue
        for ki in kr['Independence']:
            di = [x for x in dr[0]['Independence'] if x['ReferenceRole'] == ki['ReferenceRole']]
            if not di or any(order_ind[di[0][dim]] < order_ind[ki[dim]] for dim in ('Actor', 'Session', 'Context', 'Provider')):
                g4 = False
    g5 = deleg['SupersededCommits'] == K['SupersededCommits']
    res['Contract'] = 'pass' if a3 and a4 and a5 and g4 and g5 else 'fail'
    disp['Contract'] = 'STOP'
    cur_exists = git_rc(d, 'cat-file', '-e', f'{cur}^{{commit}}')[0] == 0
    ident = (cur_exists and handoff['Branch'] == br == live['branch'] and norm_path(handoff['Worktree']) == norm_path(live['toplevel'])
             and is_anc(d, base, cur) == 0 and red is not None and is_anc(d, base, red) == 0 and is_anc(d, red, cur) == 0
             and live['HEAD'] == live['origin-branch'] == cur)
    res['Identity'] = 'pass' if ident else 'fail'
    disp['Identity'] = 'STOP'
    res['Remote'] = 'pass' if rf['LsRemoteSha'] == cur and rf['OriginMainSha'] == deleg['MainSha'] else 'fail'
    disp['Remote'] = 'STOP' if rf.get('OriginMainSha') else 'BLOCKED'
    diff = git_lines(d, 'diff', '--name-only', f'{base}..{cur}') if cur_exists else []
    scope_ok = cur_exists and all(covered_by(p, deleg['AllowedWriteScope']) and not covered_by(p, deleg['ForbiddenWriteScope']) for p in diff)
    res['Scope'] = 'pass' if scope_ok else 'fail'
    disp['Scope'] = 'STOP'
    res['CleanTree'] = 'pass' if live['status'] == '' and live['check-ignore-rc'] == 0 else 'fail'
    disp['CleanTree'] = 'REWORK'
    exige_red = deleg['ChainRedSha'] is None
    rr = rf.get('RedRun') or {}
    jobs = lambda run: {j['Name']: j['Conclusion'] for j in run.get('Jobs', [])}
    ci_red = 'pass' if (exige_red and red is not None and rr.get('HeadSha') == red and jobs(rr).get('fixture-tests') == 'failure') else (
        'fail' if exige_red else 'not_applicable')
    cr = rf.get('CurrentRun') or {}
    run_ok = cr.get('HeadSha') == cur and cr.get('Event') == 'push' and cr.get('Ref') == f'refs/heads/{br}'
    green = jobs(cr).get('fixture-build') == 'success' and jobs(cr).get('fixture-tests') == 'success'
    res['Ci'] = 'pass' if run_ok and green and ci_red != 'fail' else 'fail'
    res['Ci.RedPart'] = ci_red
    disp['Ci'] = 'BLOCKED' if not run_ok else 'REWORK'
    flt = K['RequiredTests'][0]['Filter'].split('~', 1)[1]
    red_trx = trx_facts(files[TRX_RED_REL])
    cur_trx_b = files[TRX_CURRENT_REL]
    cur_trx = trx_facts(cur_trx_b)
    rt = git_lines(d, 'diff', '--name-only', deleg['ChainBaseSha'], red, '--', 'tests/')
    touches = any(p in set(rt) | set(deleg['ChainRedFiles']) for p in git_lines(d, 'diff', '--name-only', red, cur)) if cur_exists else True
    t_red = 'pass' if (exige_red and any(flt in t for t in red_trx['failed_tests']) and not touches) else ('fail' if exige_red else 'not_applicable')
    art = [a for a in rf['TestArtifacts'] if a['RunRef'] == f'gh-run:{cr.get("GhRunId")}']
    counts_ok = bool(art) and (art[0]['Selected'], art[0]['Passed'], art[0]['Failed']) == (
        int(cur_trx['total']), int(cur_trx['passed']), int(cur_trx['failed']))
    tests_ok = (run_ok and art and art[0]['Sha256'] == sha256b(cur_trx_b) and counts_ok
                and sum(1 for t in cur_trx['tests'] if flt in t) >= K['RequiredTests'][0]['MinSelected'] and t_red != 'fail')
    res['Tests'] = 'pass' if tests_ok else 'fail'
    res['Tests.RedPart'] = t_red
    disp['Tests'] = 'REWORK'
    tval = handoff['Worker']['Trailer'].split(':', 1)[1].strip()
    commits = git_lines(d, 'rev-list', f'{base}..{cur}') if cur_exists else []
    tr_ok = cur_exists and all(tval in [x.strip() for x in git(d, 'log', '-1', '--format=%(trailers:key=Co-Authored-By,valueonly)', c).splitlines()]
                               for c in commits)
    res['Trailer'] = 'pass' if tr_ok else 'fail'
    disp['Trailer'] = 'REWORK'
    ob = relay['Participant']['Observation']
    res['Routing'] = 'pass' if ob['ModelEffective'] == ob['ModelRequested'] and ob['EffortEffective'] == ob['EffortRequested'] else 'fail'
    disp['Routing'] = 'BLOCKED'
    res['FreeText'] = 'pass' if not gate_terms_in(handoff) else 'fail'
    disp['FreeText'] = 'REWORK'
    res['Denials'] = 'pass' if not relay.get('Denials') else 'fail'
    disp['Denials'] = 'BLOCKED'
    disp['Handoff'] = 'BLOCKED' if not present else ('REWORK' if res['Identity'] == 'pass' else 'STOP')
    fails = [c for c in ORDER if res[c] != 'pass']
    if not fails:
        cls, dsp, fc = 'EXECUTION_VERIFIED', 'NONE', 'NONE'
    else:
        top = max((disp[c] for c in fails), key=lambda x: RANK[x])
        cls = 'EXECUTION_REWORK_REQUIRED' if top == 'REWORK' else 'EXECUTION_BLOCKED'
        dsp, fc = top, fails[0]
    return res, cls, dsp, fc, diff


def gate_terms_in(handoff):
    found = []
    for k in FREE_TEXT_FIELDS:
        v = handoff.get(k)
        texts = v if isinstance(v, list) else [v or '']
        for t in texts:
            for term in GATE_TERMS:
                if re.search(r'(?<![A-Za-z0-9_])' + re.escape(term) + r'(?![A-Za-z0-9_])', t, re.I):
                    m = re.search(re.escape(term), t, re.I)
                    found.append(m.group(0))
    return found


def norm_path(v):
    return (v or '').strip().strip('"').replace('\\', '/').rstrip('/').lower()


def mutated_fields(a, b):
    return [k for k in sorted(set(a) | set(b)) if a.get(k) != b.get(k)]


def reference(outdir):
    if need_mode() == 'NATIVE_ARCHITECT':
        return reference_architect(outdir)
    d = env('PROBE_DIR')
    br, base, headx, red, bad = env('BRANCH'), env('BASE_SHA'), env('HEAD_SHA'), env('RED_SHA'), env('BAD_SHA')
    files, manifest = staged_files(outdir)
    head = git(d, 'rev-parse', 'HEAD').strip()
    if head != manifest['StageHead']:
        sys.exit('HEAD de PROBE_DIR distinto del de stage: la referencia no sería la del escenario')
    S = {}

    def put(step, key, v, cmp='exact'):
        S.setdefault(str(step), {})[key] = {'v': v, 'cmp': cmp}

    K = json.loads(show(d, 'HEAD', env('CONTRACT_PATH')).decode('utf-8'))
    st_head = parse_yaml(show(d, 'HEAD', env('STATE_PATH')).decode('utf-8'))
    st_base = parse_yaml(show(d, base, env('STATE_PATH')).decode('utf-8'))
    main, ar = K['MainSha'], K['AuthorityRevision']
    ap_main = show(d, main, 'docs/AUTOMATION_PLAN.md').decode('utf-8')
    t1610 = ap_main.split('### 16.10 Términos de gate', 1)[1].split('\n### ', 1)[0]
    missing_terms = [t for t in GATE_TERMS if f'`{t}`' not in t1610]
    if missing_terms:
        sys.exit(f'AP 16.10 en MainSha_eval no contiene los términos esperados: {missing_terms}')
    live = {'HEAD': head, 'branch': git(d, 'rev-parse', '--abbrev-ref', 'HEAD').strip(),
            'toplevel': git(d, 'rev-parse', '--show-toplevel').strip(), 'origin-branch': git(d, 'rev-parse', f'refs/remotes/origin/{br}').strip(),
            'origin-main': git(d, 'rev-parse', 'refs/remotes/origin/main').strip()}
    # A (NATIVE_CONTROLLER: la shell no se presupone; el paso 1 pide la que el modelo observa y la shell real se lee de los eventos)
    if MODE == 'NATIVE_CONTROLLER':
        put(1, 'shell', None, 'present')
    else:
        put(1, 'ver', None, 'present')
        put(1, 'ComSpec', 'cmd.exe', 'contains')
    put(1, 'cwd', live['toplevel'], 'path')
    # B
    for k in ('HEAD', 'branch', 'toplevel', 'origin-branch', 'origin-main'):
        put(2, k, live[k], 'path' if k == 'toplevel' else ('hex' if k not in ('branch',) else 'exact'))
    paths = git_lines(d, 'diff', '--name-only', f'{base}..{headx}')
    put(3, 'paths', paths, 'list')
    put(3, 'paths-2', paths, 'list')
    put(3, 'identical', 'yes', 'yesno')
    commits = git_lines(d, 'rev-list', '--reverse', f'{base}..{headx}')
    put(4, 'commits', commits, 'hexlist')
    for c in commits:
        tl = [x.strip() for x in git(d, 'log', '-1', '--format=%(trailers:key=Co-Authored-By,valueonly)', c).splitlines() if x.strip()]
        put(4, f'trailer:{c}', ' | '.join(tl), 'trailers')
    put(5, 'diff-red-head', git_lines(d, 'diff', '--name-only', red, head), 'list')
    put(5, 'rt', git_lines(d, 'diff', '--name-only', base, red, '--', 'tests/'), 'list')
    put(6, 'mb', git(d, 'merge-base', main, ar).strip(), 'hex')
    for k, (x, y) in {'anc:base-red': (base, red), 'anc:red-head': (red, head), 'anc:head-base': (head, base), 'anc:ar-base': (ar, base)}.items():
        rc = is_anc(d, x, y)
        put(6, k, 'yes' if rc == 0 else ('no' if rc == 1 else f'rc={rc}'), 'yesno')
    for fpath in env('HASH_FILES').split():
        put(7, f'hash-object:{fpath}', git(d, 'hash-object', fpath).strip(), 'hex')
        put(7, f'rev-parse:{fpath}', git(d, 'rev-parse', f'HEAD:{fpath}').strip(), 'hex')
        put(7, f'sha256:{fpath}', sha256b(read_bytes(os.path.join(d, *fpath.split('/')))), 'hex')
    for p in (env('STATE_PATH'), env('CONTRACT_PATH')):
        rc, out = git_rc(d, 'rev-parse', f'{ar}:{p}')
        put(7, f'blob-at-ar:{p}', out.strip() if rc == 0 else f'rc={rc}', 'exact_ci')
    put(8, 'bad-rc', git_rc(d, 'cat-file', '-t', bad)[0], 'int')
    lsr = lambda r: (git(d, 'ls-remote', 'origin', r).split() or [''])[0]
    put(9, 'ls-remote:branch', lsr(f'refs/heads/{br}'), 'hex')
    put(9, 'ls-remote:main', lsr('refs/heads/main'), 'hex')
    sfile = os.path.join(outdir, 'probe-status-before.txt')
    status = read_text(sfile) if os.path.exists(sfile) else git(d, 'status', '--porcelain')
    ifile = os.path.join(outdir, 'probe-status-ignored-before-run.txt')
    if not os.path.exists(ifile):
        sys.exit('falta probe-status-ignored-before-run.txt (lo que vio la sonda con el escenario preparado)')
    put(10, 'status', status, 'block')
    put(10, 'status-ignored', read_text(ifile), 'block')
    rc, out = git_rc(d, 'check-ignore', '-v', '--no-index', env('IGNORE_PATH'))
    put(10, 'check-ignore', out, 'block')
    put(10, 'check-ignore-rc', rc, 'int')
    live['status'] = status.strip()
    live['check-ignore-rc'] = rc
    # C
    md = show(d, 'HEAD', env('DECISIONS_MD')).decode('utf-8').split('\n')
    line = md[int(env('MD_LINE')) - 1].rstrip('\r')
    na = nonascii(line)
    put(11, 'md-line', line, 'verbatim')
    put(11, 'md-nonascii-count', len(na), 'int')
    put(11, 'md-nonascii-chars', ';'.join(f'{c}={na.count(c)}' for c in sorted(set(na), key=ord)), 'verbatim')
    ag = show(d, 'HEAD', 'AGENTS.md').decode('utf-8')
    sec = ag.split('## Leer primero', 1)[1].split('\n## ', 1)[0]
    put(11, 'read-first', re.findall(r'\]\(([^)]+)\)', sec), 'list')
    cmds = ag.split('## Comandos canonicos', 1)[1].split('\n## ', 1)[0]
    put(11, 'action-incompatible', [x.strip() for x in cmds.splitlines() if x.strip().startswith('dotnet test')][0], 'cmdline')
    for k in ('automation_state.initiative', 'automation_state.attempts', 'automation_state.branch', 'protocol.set', 'protocol.effective_sha',
              'custody.task_intent.kind', 'custody.task_intent.attempt'):
        v = yget(st_head, k)
        put(12, f'yaml:{k}', '' if v is None else str(v), 'exact')
    put(12, 'yaml:custody.chains.count', len(yget(st_head, 'custody.chains') or []), 'int')
    put(12, 'yaml:max_attempts', 'ABSENT' if not has_key_anywhere(st_head, 'max_attempts') else str(st_head.get('max_attempts')), 'exact')
    put(12, 'yaml:automation_state.next_action', str(yget(st_head, 'automation_state.next_action')), 'verbatim')
    for k in ('Schema', 'MainSha', 'AuthorityRevision', 'RoutingEnforcement', 'CorrectionsAuthorized', 'Objective'):
        put(13, f'json:{k}', json.dumps(K[k], ensure_ascii=False), 'json')
    put(13, 'json:RoleRequirements[1].Independence[0].ReferenceRole', json.dumps(K['RoleRequirements'][1]['Independence'][0]['ReferenceRole']), 'json')
    put(13, 'json:RequiredTests[0].Filter', json.dumps(K['RequiredTests'][0]['Filter']), 'json')
    put(13, 'json:Invariants', json.dumps(K['Invariants'], ensure_ascii=False), 'json')
    put(13, 'json:Objective.nonascii-count', len(nonascii(K['Objective'])), 'int')
    for k in ('AllowedWriteScope', 'ForbiddenWriteScope', 'RoleRequirements'):
        put(13, f'len:{k}', len(K[k]), 'int')
    for p in CLOSURE_FILES + [env('CONTRACT_PATH'), env('STATE_PATH'), env('DECISIONS_MD')]:
        put(13, f'closure:{p}', git(d, 'rev-parse', f'HEAD:{p}').strip(), 'hex')
    # D
    deleg = json.loads(files[DELEGATION_REL].decode('utf-8'))
    handoff = json.loads(files[HANDOFF_REL].decode('utf-8'))
    relay = json.loads(files[RELAY_REL].decode('utf-8'))
    rf = relay['RemoteFacts']
    for step, rel, pre in ((14, TRX_CURRENT_REL, 'trx'), (15, TRX_RED_REL, 'trxred')):
        b = files[rel]
        t = trx_facts(b)
        put(step, f'{pre}:sha256', sha256b(b), 'hex')
        for k in ('total', 'executed', 'passed', 'failed', 'notExecuted'):
            put(step, f'{pre}:{k}', int(t[k]), 'int')
        put(step, f'{pre}:tests', t['tests'], 'list')
        put(step, f'{pre}:failed-tests', t['failed_tests'], 'list')
        put(step, f'{pre}:filter-selected', sum(1 for x in t['tests'] if 'Subtract' in x), 'int')
        put(step, f'{pre}:sha256-match', yn(any(a['Sha256'] == sha256b(b) for a in rf['TestArtifacts'])), 'yesno')
        if pre == 'trxred':
            put(step, 'trxred:filter-failed', sum(1 for x in t['failed_tests'] if 'Subtract' in x), 'int')
    fm = (handoff['TaskId'] == deleg['TaskId'] and handoff['Attempt'] == deleg['Attempt'] and handoff['DelegationRunId'] == deleg['RunId']
          and handoff['RunId'] == WORK_RUN_ID)
    put(16, 'handoff:present', 'yes', 'yesno')
    put(16, 'handoff:valid-json', 'yes', 'yesno')
    put(16, 'handoff:fields-match', yn(fm), 'yesno')
    put(16, 'handoff:CurrentSha', handoff['CurrentSha'], 'hex')
    put(16, 'handoff:sha256', sha256b(files[HANDOFF_REL]), 'hex')
    put(16, 'handoff:sha256-match', yn(sha256b(files[HANDOFF_REL]) == relay['Outcome']['OutputSha256']), 'yesno')
    put(16, 'handoff:after-start', yn(relay['Outcome']['OutputWrittenUtc'] > relay['Cession']['StartUtc']), 'yesno')
    jobs = lambda run: ';'.join(f'{j["Name"]}={j["Conclusion"]}' for j in run['Jobs'])
    put(17, 'rf:current-head', rf['CurrentRun']['HeadSha'], 'hex')
    put(17, 'rf:red-head', rf['RedRun']['HeadSha'], 'hex')
    put(17, 'rf:current-jobs', jobs(rf['CurrentRun']), 'exact')
    put(17, 'rf:red-jobs', jobs(rf['RedRun']), 'exact')
    put(17, 'rf:current-is-currentsha', yn(rf['CurrentRun']['HeadSha'] == handoff['CurrentSha']), 'yesno')
    put(17, 'rf:lsremote-is-currentsha', yn(rf['LsRemoteSha'] == handoff['CurrentSha']), 'yesno')
    put(17, 'rf:originmain-is-mainsha', yn(rf['OriginMainSha'] == deleg['MainSha']), 'yesno')
    rt = set(git_lines(d, 'diff', '--name-only', base, red, '--', 'tests/')) | set(deleg['ChainRedFiles'])
    put(18, 'redpart:touches', yn(any(p in rt for p in git_lines(d, 'diff', '--name-only', red, head))), 'yesno')
    # E
    ev, authority_ok, resolution = evaluate_i62(d, K, deleg['BaseSha'], st_base)
    kinds19 = {'trailer-commits': 'hexset', 'fp-merges': 'hexset', 'EFF': 'hex', 'EFF^1': 'hex', 'EFF^2': 'hex', 'pre-rows': 'int', 'cid-in-pre': 'yesno'}
    for k, c in kinds19.items():
        put(19, k, ev[k], c)
    for k in ('classify:decision-blob-type', 'classify:markers', 'classify:P'):
        put(20, k, ev[k], 'yesno' if k.endswith('markers') else 'exact_ci')
    for k, c in (('workflow-blob', 'hex'), ('ap-blob', 'hex'), ('entry-count', 'int'), ('entry-line', 'int'), ('r-count', 'int'),
                 ('r-line', 'int'), ('entry-names-r', 'yesno')):
        put(21, k, ev[k], c)
    for k in ('map-blob', 'schema-blob'):
        put(22, k, ev[k], 'hex')
    for i in range(1, 8):
        put(22, f'mv{i}', ev[f'mv{i}'], 'passfail')
    put(22, 'mv3-changed', ev['mv3-changed'], 'list')
    put(22, 'mv4-mismatches', ev['mv4-mismatches'], 'int')
    for k in [x for x in ev if x.startswith('mv6-identical:')]:
        put(22, k, ev[k], 'exact_ci')
    for k in ('mv7-pointer-16', 'mv7-pointer-163', 'mv7-163-equal'):
        put(22, k, ev[k], 'yesno')
    put(22, 'map-verdict', ev['map-verdict'], 'exact_ci')
    put(22, 'mv-failed', ev['mv-failed'], 'idlist')
    put(23, 'res:mb', ev['res:mb'], 'hex')
    for k in [x for x in ev if x.startswith('res:ext-diff:')]:
        put(23, k, ev[k], 'list')
    put(23, 'res:ar-base-diff', ev['res:ar-base-diff'], 'list')
    put(23, 'res:ar-base-clean', ev['res:ar-base-clean'], 'yesno')
    # F
    for k, p in (('schema:delegation.v2', CANON['delegation_v2']), ('schema:controller-verification.v2', CANON['controller_verification_v2'])):
        put(24, k, blob_at(d, 'HEAD', p), 'hex')
    # G
    origs = {DELEGATION_REL: deleg, HANDOFF_REL: handoff}
    copies = {n: json.loads(files[NC_REL[n]].decode('utf-8')) for n in NC_REL}
    fields = {n: mutated_fields(origs[NC_BASE[n]], copies[n]) for n in NC_REL}
    for n, fl in fields.items():
        if len(fl) != 1:
            sys.exit(f'{n}: la copia cambia {fl}, no un solo campo')
    put(25, 'nc1-field', fields['nc1'][0], 'exact_ci')
    put(25, 'nc1-rc', git_rc(d, 'cat-file', '-t', copies['nc1']['CurrentSha'])[0], 'int')
    r_nc1, *_ = compute_checks(d, K, deleg, copies['nc1'], relay, files, authority_ok, live)
    put(25, 'nc1-Identity', r_nc1['Identity'], 'passfail')
    put(26, 'nc2-field', fields['nc2'][0], 'exact_ci')
    diff_s = git_lines(d, 'diff', '--name-only', f'{handoff["BaseSha"]}..{handoff["CurrentSha"]}')
    excl = [p for p in diff_s if covered_by(p, deleg['AllowedWriteScope']) and not covered_by(p, copies['nc2']['AllowedWriteScope'])]
    put(26, 'nc2-excluded', nl(excl), 'list')
    r_nc2, *_ = compute_checks(d, K, copies['nc2'], handoff, relay, files, authority_ok, live)
    put(26, 'nc2-Scope', r_nc2['Scope'], 'passfail')
    put(27, 'nc3-field', fields['nc3'][0], 'exact_ci')
    put(27, 'nc3-term', ';'.join(gate_terms_in(copies['nc3'])), 'exact_ci')
    r_nc3, *_ = compute_checks(d, K, deleg, copies['nc3'], relay, files, authority_ok, live)
    put(27, 'nc3-FreeText', r_nc3['FreeText'], 'passfail')
    put(28, 'nc4-field', fields['nc4'][0], 'exact_ci')
    extra = [e for e in copies['nc4']['AllowedWriteScope'] if not entry_subset([e], K['AllowedWriteScope'])[0]]
    put(28, 'nc4-extra', nl(extra), 'list')
    put(28, 'nc4-A3', 'pass' if entry_subset(copies['nc4']['AllowedWriteScope'], K['AllowedWriteScope'])[0] else 'fail', 'passfail')
    # orden FX-U1-O4, punto 15: N4 (sin entrega), N5 (conteos de TestArtifacts de la corrida actual a 0), N7a (RunId de otra corrida),
    # N7b (N7a + CurrentSha = RedSha)
    if json.loads(files[N_REL['N4']].decode('utf-8')) != relay:
        sys.exit('N4: la copia de los hechos del relevo no es idéntica')
    put(29, 'n4-handoff-present', 'no', 'yesno')
    no_handoff = {k: v for k, v in files.items() if k != HANDOFF_REL}
    r_n4, *_ = compute_checks(d, K, deleg, handoff, relay, no_handoff, authority_ok, live)
    put(29, 'n4-Handoff', r_n4['Handoff'], 'passfail')
    n5 = json.loads(files[N_REL['N5']].decode('utf-8'))
    f5 = mutated_fields(relay, n5)
    if len(f5) != 1:
        sys.exit(f'N5: la copia cambia {f5}')
    put(30, 'n5-field', f5[0], 'exact_ci')
    put(30, 'n5-selected', [a for a in n5['RemoteFacts']['TestArtifacts'] if a['RunRef'] == f'gh-run:{env("TRX_RUN_ID")}'][0]['Selected'], 'int')
    r_n5, *_ = compute_checks(d, K, deleg, handoff, n5, files, authority_ok, live)
    put(30, 'n5-Tests', r_n5['Tests'], 'passfail')
    n7a = json.loads(files[N_REL['N7a']].decode('utf-8'))
    n7b = json.loads(files[N_REL['N7b']].decode('utf-8'))
    f7a, f7b = mutated_fields(handoff, n7a), mutated_fields(handoff, n7b)
    if len(f7a) != 1 or len(f7b) != 2:
        sys.exit(f'N7a/N7b: campos {f7a} / {f7b}')
    put(31, 'n7a-field', f7a[0], 'exact_ci')
    r_7a, *_ = compute_checks(d, K, deleg, n7a, relay, files, authority_ok, live)
    put(31, 'n7a-Handoff', r_7a['Handoff'], 'passfail')
    put(32, 'n7b-fields', ','.join(f7b), 'idlist')
    r_7b, *_ = compute_checks(d, K, deleg, n7b, relay, files, authority_ok, live)
    put(32, 'n7b-Identity', r_7b['Identity'], 'passfail')
    put(32, 'n7b-Handoff', r_7b['Handoff'], 'passfail')
    # objetos canónicos esperados
    checks, cls, dsp, fc, scope_diff = compute_checks(d, K, deleg, handoff, relay, files, authority_ok, live)
    lsmain = lsr('refs/heads/main')
    att = yget(st_head, 'automation_state.attempts')
    max_att = st_head.get('max_attempts') if has_key_anywhere(st_head, 'max_attempts') else 3
    dexp = {'Schema': 'rackcad-delegation/v2', 'TaskId': K['TaskId'], 'RunId': PLAN_RUN_ID, 'Initiative': yget(st_head, 'automation_state.initiative'),
            'Unit': K['Unit'], 'Gate': K['Gate'], 'Attempt': att, 'AuthorityRevision': ar, 'MainSha': lsmain, 'BaseSha': head,
            'ExpectedBranch': br, 'Owner': {'Kind': 'session-internal', 'Id': 'session-internal:' + PLAN_RUN_ID}, 'Model': WORKER_MODEL,
            'RoutingEnforcement': K['RoutingEnforcement'], 'Authorities': K['Authorities'], 'Objective': K['Objective'],
            'AllowedWriteScope': K['AllowedWriteScope'], 'ForbiddenWriteScope': K['ForbiddenWriteScope'], 'Invariants': K['Invariants'],
            'RequiredTests': K['RequiredTests'], 'ExpectedEvidence': K['ExpectedEvidence'], 'MaxReworkLoops': 3,
            'AttemptsRemaining': max_att - att, 'ChainBaseSha': head, 'ChainRedSha': None, 'ChainRedFiles': [], 'CorrectionOf': None,
            'ExpectedHandoffPath': f'artifacts/orchestration/{K["Unit"]}/{K["TaskId"]}/{att}/{{WorkRunId}}/worker-handoff.json',
            'IssuedBy': ISSUED_BY, 'RoleRequirements': K['RoleRequirements'], 'SupersededCommits': K['SupersededCommits']}
    cexp = {'Schema': 'rackcad-controller-verification/v2', 'TaskId': handoff['TaskId'], 'RunId': VERIFY_RUN_ID,
            'DelegationRunId': handoff['DelegationRunId'], 'WorkRunId': handoff['RunId'], 'Attempt': handoff['Attempt'],
            'VerifiedSha': handoff['CurrentSha'], 'Verifier': VERIFIER, 'AuthorityResolution': resolution,
            'Classification': cls, 'Disposition': dsp, 'FailureClass': fc}
    ref = {'Kit': KIT, 'Mode': MODE, 'steps': S,
           'delegation': {'exact': dexp, 'ExpectedWorktree': live['toplevel'], 'StopConditions': K['StopConditions'],
                          'ExecutorBase': {k: v for k, v in EXECUTOR.items() if k != 'Cell'}},
           'cv': {'exact': cexp, 'checks': checks, 'scope_diff': scope_diff},
           'meta': {'lsremote_live': git(d, 'ls-remote', 'origin'), 'stage_head': manifest['StageHead'],
                    'remote_keys': ['9:ls-remote:branch', '9:ls-remote:main', '17:rf:lsremote-is-currentsha', 'delegation:MainSha']}}
    write_json(os.path.join(outdir, 'reference.json'), ref)
    print('reference ok')


# ------------------------------------------------------------------ compare
def cmp_value(kind, got, exp):
    if got is None:
        return False
    g = got if isinstance(got, str) else str(got)
    hexn = lambda v: re.sub(r'\s+', '', v or '').lower()
    lst = lambda v: [x.strip() for x in v.replace('\r\n', '\n').replace('\r', '\n').split('\n') if x.strip()]
    if kind == 'present':
        return True
    if kind == 'contains':
        return exp.lower() in g.lower()
    if kind == 'path':
        return norm_path(g) == norm_path(exp)
    if kind == 'hex':
        return hexn(g) == hexn(exp or '')
    if kind == 'hexlist':
        return [hexn(x) for x in lst(g)] == [hexn(x) for x in exp]
    if kind == 'hexset':
        return sorted(hexn(x) for x in lst(g)) == sorted(hexn(x) for x in exp)
    if kind == 'list':
        e = exp if isinstance(exp, list) else lst(exp)
        return lst(g) == [x.strip() for x in e]
    if kind == 'block':
        nb = lambda v: '\n'.join(x.rstrip().replace('\t', ' ') for x in v.replace('\r\n', '\n').replace('\r', '\n').strip('\n').split('\n')).strip()
        return nb(g) == nb(exp)
    if kind == 'int':
        try:
            return int(g.strip()) == int(exp)
        except Exception:
            return False
    if kind in ('yesno', 'passfail', 'exact_ci'):
        return g.strip().lower() == str(exp).strip().lower()
    if kind == 'verbatim':
        return g.rstrip('\r\n') == exp
    if kind == 'json':
        try:
            return json.loads(g) == json.loads(exp)
        except Exception:
            return False
    if kind == 'trailers':
        tn = lambda v: ' | '.join(x.strip() for x in (v or '').split('|') if x.strip())
        return tn(g) == tn(exp)
    if kind == 'cmdline':
        return g.strip().strip('`').strip() == exp
    if kind == 'idlist':
        return [x.strip().upper() for x in g.split(',') if x.strip()] == [x.strip().upper() for x in exp.split(',') if x.strip()]
    return g.strip() == str(exp).strip()


FORBIDDEN = {'powershell': r'(?i)pwsh|powershell',
             'interpreter': r'(?:^|[\s"\'|&(\\/])(?<!-)(?:py|python|python3|node|perl|ruby)(?:\.exe)?(?=$|[\s"\'|&)])',
             'unix_tool': r'(?:^|[\s"\'|&(\\/])(?<!-)(?:grep|egrep|sed|awk|find|wc|sort|head|tail|cut|xargs|sha256sum|bash|sh)(?:\.exe)?(?=$|[\s"\'|&)])'}
# git que escribe. Corrección del defecto de v3 (R20261010T000542Z-a4-1-probe, result.json ToolDefect): la expresión de v3 contaba
# `git merge-base` como `merge` porque `\b` casa entre «merge» y «-». Ahora el subcomando es el primer argumento que no es una opción y
# tiene que terminar en espacio, comillas, separador o fin (nunca en «-»): `merge-base`, `merge-tree`, `stash list|show`, `tag -l`,
# `branch --list|-a|-r|--show-current` y `hash-object` sin `-w` no cuentan. En v4 (GIT_WRITE, NETWORK, FILE_WRITE) la orden tiene además que
# estar en posición de orden (inicio, tras un separador `& | ; ( { =` o tras `/c` y `-Command` del envoltorio de la shell), para no contar el
# texto de una búsqueda (p. ej. `Select-String "git push"` o `findstr /c:"Coordinador del fixture"`).
_CMDPOS = r'(?:^|[&|;(\n{=]|/c\s+["\']?|-Command\s+["\']?|-c\s+["\']?)\s*(?:&\s*)?'
_GIT_PRE = (_CMDPOS + r'git(?:\.exe)?["\']?\s+(?:(?:-C|-c|--git-dir|--work-tree|--namespace)\s+(?:"[^"]*"|\'[^\']*\'|\S+)\s+'
            r'|--?[A-Za-z][\w-]*(?:=\S+)?\s+)*')
_END = r'(?=$|[\s"\'&|;)])'
GIT_WRITE = re.compile(
    _GIT_PRE + r'(?:'
    r'(?:fetch|pull|push|clone|commit|checkout|switch|restore|reset|add|rm|mv|merge|rebase|cherry-pick|revert|am|apply|clean|gc|prune|repack'
    r'|update-ref|update-index|read-tree|write-tree|commit-tree|init|filter-branch|fast-import|maintenance|notes|replace|submodule)' + _END +
    r'|stash' + _END + r'(?!\s*(?:list|show)' + _END + r')'
    r'|tag\s+(?:-[adfsu]\b|(?!-)[^\s"\'&|;]+)'
    r'|branch\s+(?:-[dDmMcCf]\b|--(?:delete|move|copy|force|set-upstream-to|unset-upstream|edit-description)\b|(?!-)[^\s"\'&|;]+)'
    r'|worktree\s+(?:add|remove|move|prune|lock|unlock|repair)\b'
    r'|remote\s+(?:add|remove|rm|rename|set-url|set-head|set-branches|prune|update)\b'
    r'|config\s+(?:[^&|;\n]*\s)?(?:--global|--system|--add|--unset|--unset-all|--replace-all|--rename-section|--remove-section|--edit|-e)\b'
    r'|hash-object\s+(?:[^&|;\n]*\s)?-w\b'
    r')', re.I)
# red: la única lectura del origen admitida es `git ls-remote origin` (transporte local del origen del fixture)
NETWORK = re.compile(
    r'(?i)(?:' + _CMDPOS + r'(?:curl|wget|Invoke-WebRequest|iwr|Invoke-RestMethod|irm|Start-BitsTransfer|bitsadmin)(?:\.exe)?(?=$|[\s"\'&|;)])'
    r'|' + _CMDPOS + r'certutil(?:\.exe)?\s+(?:-f\s+)?-urlcache\b'
    r'|' + _GIT_PRE + r'(?:fetch|pull|push|clone)' + _END +
    r'|' + _GIT_PRE + r'ls-remote\s+(?:--?[\w-]+(?:=\S+)?\s+)*(?!origin' + _END + r')[^\s"\'&|;-][^\s"\'&|;]*)')
# escrituras de archivos fuera de Git (PowerShell, cmd.exe, herramientas de Unix) y redirecciones a un archivo (no a NUL/$null/&n)
FILE_WRITE = re.compile(
    r'(?i)(?:' + _CMDPOS + r'(?:Set-Content|Add-Content|Out-File|New-Item|Remove-Item|Copy-Item|Move-Item|Rename-Item|Clear-Content|Set-ItemProperty'
    r'|New-ItemProperty|Export-Csv|Export-Clixml|Tee-Object|Expand-Archive|Compress-Archive'
    r'|del|erase|copy|xcopy|robocopy|move|ren|rename|mkdir|md|rmdir|rd|mklink|attrib|icacls|takeown|touch|rm|cp|mv|tee|truncate)(?:\.exe)?(?=$|\s)'
    r'|(?:^|[\s*\d])>>?\s*(?!&|nul\b|\$null\b|/dev/null\b)["\']?[\w.\/:%$~-])')
# firma de F6-OBS-03 (evidencia §93): el runtime no puede lanzar la shell que selecciona bajo el sandbox de solo lectura
F6_SIG = re.compile(r'CreateProcessAsUserW failed|Failed to create unified exec process')
USAGE_LIMIT = re.compile(r'(?i)usage limit|rate[ _-]?limit(?:ed|_reached| reached)|insufficient_quota|quota exceeded|out of credits|credit balance')
SYNTH_SHELL = '%USERPROFILE%\\.cache\\codex-runtimes\\codex-primary-runtime\\dependencies\\native\\powershell\\pwsh.exe'


def shell_of(cmd):
    # La shell con la que el runtime lanzó el comando: el primer elemento de la línea de órdenes que registra el evento
    # (p. ej. `"C:\\WINDOWS\\system32\\cmd.exe" /c '…'` o `"…\\WindowsApps\\pwsh.exe" -Command '…'`).
    s = (cmd if isinstance(cmd, str) else ' '.join(map(str, cmd or []))).strip().replace('\\\\', '\\')
    m = re.match(r'^"([^"]+)"|^(\S+)', s)
    tok = (m.group(1) or m.group(2) or '') if m else ''
    low = tok.replace('/', '\\').lower()
    base = low.rsplit('\\', 1)[-1]
    if base in ('pwsh.exe', 'pwsh'):
        return 'pwsh-windowsapps' if '\\windowsapps\\' in low else ('pwsh-runtime' if 'codex-runtimes' in low else 'pwsh-other')
    if base in ('powershell.exe', 'powershell'):
        return 'powershell'
    if base in ('cmd.exe', 'cmd'):
        return 'cmd'
    if base in ('bash.exe', 'bash', 'sh.exe', 'sh'):
        return 'bash'
    return 'direct:' + (base or '?')


def _sanitize(t):
    t = re.sub(r'(?i)C:\\+Users\\+[^\\"\s|]+', '%USERPROFILE%', t or '')
    return t[:200]


def audit_events(path, stderr_path=None):
    stats = {'commands': 0, 'cmd': 0, 'failed': 0, 'powershell': 0, 'interpreter': 0, 'unix_tool': 0, 'forbidden_completed': 0,
             'git_write': 0, 'list': [],
             # ampliación de v4 (informativa en CMD_A41; criterio en los modos NATIVE_*)
             'completed_ok': 0, 'git_write_completed': 0, 'network': 0, 'network_completed': 0, 'file_write': 0, 'file_write_completed': 0,
             'shells': {}, 'default_shell': None, 'model_fallback': False,
             'f6_obs03': {'present': False, 'count': 0, 'shells': [], 'sample': None, 'in_stderr': False}, 'usage_limit_signal': False}
    order = []

    def walk(o, etype=None):
        if isinstance(o, dict):
            it = o.get('item') if isinstance(o.get('item'), dict) else None
            if it and it.get('type') == 'command_execution' and o.get('type') == 'item.completed':
                c = it.get('command')
                s = c if isinstance(c, str) else ' '.join(map(str, c or []))
                cats = [k for k, rx in FORBIDDEN.items() if re.search(rx, s)]
                ok = it.get('exit_code') == 0 and it.get('status') == 'completed'
                sh = shell_of(c)
                out = it.get('aggregated_output') or ''
                gw, nw, fw = bool(GIT_WRITE.search(s)), bool(NETWORK.search(s)), bool(FILE_WRITE.search(s))
                stats['commands'] += 1
                stats['cmd'] += 1 if 'cmd.exe' in s.lower() else 0
                stats['failed'] += 0 if ok else 1
                stats['completed_ok'] += 1 if ok else 0
                for k in cats:
                    stats[k] += 1
                if cats and ok:
                    stats['forbidden_completed'] += 1
                if gw:
                    stats['git_write'] += 1
                    stats['git_write_completed'] += 1 if ok else 0
                if nw:
                    stats['network'] += 1
                    stats['network_completed'] += 1 if ok else 0
                if fw:
                    stats['file_write'] += 1
                    stats['file_write_completed'] += 1 if ok else 0
                e = stats['shells'].setdefault(sh, {'total': 0, 'ok': 0})
                e['total'] += 1
                e['ok'] += 1 if ok else 0
                sig = (not ok) and bool(F6_SIG.search(out))
                if sig:
                    f = stats['f6_obs03']
                    f['present'] = True
                    f['count'] += 1
                    if sh not in f['shells']:
                        f['shells'].append(sh)
                    f['sample'] = f['sample'] or _sanitize(out)
                order.append((sh, ok))
                extra = [x for x, flag in (('git_write', gw), ('network', nw), ('file_write', fw), ('f6_obs03', sig)) if flag]
                stats['list'].append({'categories': cats + extra, 'exit_code': it.get('exit_code'), 'status': it.get('status'), 'chars': len(s),
                                      'shell': sh})
            else:
                if o.get('type') in ('error', 'turn.failed') and USAGE_LIMIT.search(json.dumps(o, ensure_ascii=False)):
                    stats['usage_limit_signal'] = True
                for v in o.values():
                    walk(v)
        elif isinstance(o, list):
            for v in o:
                walk(v)
    if os.path.exists(path):
        with open(path, encoding='utf-8', errors='replace') as f:
            for ln in f:
                try:
                    walk(json.loads(ln))
                except Exception:
                    pass
    if stderr_path and os.path.exists(stderr_path):
        err = read_text_lenient(stderr_path)
        if F6_SIG.search(err):
            stats['f6_obs03']['present'] = True
            stats['f6_obs03']['in_stderr'] = True
        if USAGE_LIMIT.search(err):
            stats['usage_limit_signal'] = True
    if order:
        stats['default_shell'] = order[0][0]
        first_fail = next((i for i, (_, ok) in enumerate(order) if not ok), None)
        stats['model_fallback'] = first_fail is not None and any(sh != order[0][0] for sh, _ in order[first_fail + 1:])
    return stats


def read_text_lenient(p):
    with open(p, encoding='utf-8', errors='replace') as f:
        return f.read()


def native_route(ev):
    # Lectura de la ruta de la shell para el Coordinator (A4-1, condición de apertura: «el runtime no usa el shell que la receta pone primero en
    # el PATH … y bajo el sandbox de solo lectura rechaza el shell que él mismo selecciona»).
    f = ev['f6_obs03']
    persists = bool(f['present'])
    return {'DefaultShell': ev['default_shell'], 'ShellsUsed': ev['shells'], 'RuntimePwshUsed': 'pwsh-runtime' in ev['shells'],
            'F6Obs03Signature': f, 'ModelFallback': ev['model_fallback'], 'ShellLimitationPersists': persists,
            'CmdA41Opens': persists,
            'Note': ('ShellLimitationPersists = firma de F6-OBS-03 en algún comando o en stderr (CreateProcessAsUserW / «Failed to create unified '
                     'exec process»). Con ella, A4-1 abre la sonda CMD_A41 (reglas 3 y 4) y la vía normal del Controller no es elegible aunque el '
                     'modelo haya recurrido por su cuenta a otra shell.')}


def process_residue(outdir):
    # informativo (operación 7 del descriptor): procesos codex*.exe vivos tras la sonda cuya línea de órdenes contiene el directorio -C
    p = os.path.join(outdir, 'procs-after.json')
    if not os.path.exists(p):
        return {'Observed': False}
    try:
        lst = json.loads(read_text_lenient(p).lstrip('\ufeff'))
    except Exception:
        return {'Observed': False, 'Error': 'procs-after.json ilegible'}
    lst = lst if isinstance(lst, list) else [lst]
    alive = [x for x in lst if isinstance(x, dict) and x.get('CmdHasProbeDir')]
    return {'Observed': True, 'CodexProcessesAfter': len(lst), 'WithProbeDirAfter': len(alive),
            'Names': sorted({x.get('Name') for x in alive if isinstance(x, dict)})}


GROUPS = {'OP_A_shell': [1], 'OP_B_git_y_hashes': list(range(2, 11)), 'OP_C_fidelidad_de_entradas': [11, 12, 13],
          'OP_D_trx_y_ci': list(range(14, 19)), 'OP_E_mapa_I62': list(range(19, 24)), 'OP_F_esquemas_canonicos': [24],
          'OP_G_controles_negativos': list(range(25, 33))}


def compare_obj(outdir, probe, ref):
    reasons, details = [], {}
    steps = {}
    for s in probe.get('steps', []) if isinstance(probe, dict) else []:
        if isinstance(s, dict) and isinstance(s.get('id'), int):
            steps.setdefault(s['id'], s)
    key_ok = {}
    for sid, keys in ref['steps'].items():
        vals = {}
        for p in steps.get(int(sid), {}).get('values', []) or []:
            if isinstance(p, dict) and 'key' in p and p['key'] not in vals:
                vals[p['key']] = p.get('value')
        for k, spec in keys.items():
            ok = k in vals and cmp_value(spec['cmp'], vals[k], spec['v'])
            key_ok[f'{sid}:{k}'] = ok
            if not ok:
                details[f'{sid}:{k}'] = {'expected': spec['v'] if not isinstance(spec['v'], str) else spec['v'][:300],
                                         'got': (vals.get(k) or '')[:300] if k in vals else 'FALTA'}
    canon = {}
    d = env('PROBE_DIR')
    for k, p in CANON.items():
        canon[k] = json.loads(show(d, 'HEAD', p).decode('utf-8'))
    # PLAN
    dl = probe.get('delegation_v2') if isinstance(probe, dict) else None
    dchk = {}
    derr = validate(dl, canon['delegation_v2']) if isinstance(dl, dict) else ['ausente']
    dchk['schema_valid'] = not derr
    dl = dl if isinstance(dl, dict) else {}
    dexp = ref['delegation']['exact']
    for k, v in dexp.items():
        dchk[f'field:{k}'] = k in dl and (norm_hex_eq(dl[k], v) if k in ('MainSha', 'BaseSha', 'ChainBaseSha', 'AuthorityRevision') else dl[k] == v)
    dchk['field:ExpectedWorktree'] = norm_path(dl.get('ExpectedWorktree')) == norm_path(ref['delegation']['ExpectedWorktree'])
    sc = dl.get('StopConditions') or []
    dchk['field:StopConditions_contains_K'] = all(x in sc for x in ref['delegation']['StopConditions'])
    ex = dl.get('Executor') or {}
    eff = (dl.get('Effort') or {}).get('Semantic')
    dchk['field:Executor'] = (all(ex.get(k) == v for k, v in ref['delegation']['ExecutorBase'].items())
                              and ex.get('Cell') == f'claude-subagent:claude-sonnet-5-5:{eff}')
    profiles, exp_eff = routing_effort(dl.get('TaskClass'), dl.get('Dimensions') or {})
    dchk['routing:TaskClass_in_routing_1'] = profiles is not None
    dchk['routing:PromptProfile'] = profiles is not None and dl.get('PromptProfile') in profiles
    dchk['routing:Effort_semantic'] = exp_eff is not None and eff == exp_eff
    # VERIFY
    cv = probe.get('controller_verification_v2') if isinstance(probe, dict) else None
    cchk = {}
    cerr = validate(cv, canon['controller_verification_v2']) if isinstance(cv, dict) else ['ausente']
    cchk['schema_valid'] = not cerr
    cv = cv if isinstance(cv, dict) else {}
    cexp = ref['cv']['exact']
    for k, v in cexp.items():
        if k == 'AuthorityResolution':
            continue
        cchk[f'field:{k}'] = k in cv and (norm_hex_eq(cv[k], v) if k == 'VerifiedSha' else cv[k] == v)
    got_checks = cv.get('Checks') or {}
    for c in ORDER:
        cchk[f'check:{c}'] = (got_checks.get(c) or {}).get('Result') == ref['cv']['checks'][c]
    for c in ('Ci', 'Tests'):
        cchk[f'check:{c}.RedPart'] = (got_checks.get(c) or {}).get('RedPart') == ref['cv']['checks'][f'{c}.RedPart']
    sev = (got_checks.get('Scope') or {}).get('Evidence') or ''
    cchk['check:Scope.Evidence_lists_diff'] = all(p in sev for p in ref['cv']['scope_diff'])
    cchk['coherence_RAE_8'] = coherence_ok(cv)
    ar_ok = cv.get('AuthorityResolution') == cexp['AuthorityResolution'] if 'AuthorityResolution' in cv else False
    if 'AuthorityResolution' in cv and not ar_ok and isinstance(cv.get('AuthorityResolution'), list):
        ar_ok = len(cv['AuthorityResolution']) == len(cexp['AuthorityResolution']) and all(
            isinstance(g, dict) and all(norm_hex_eq(g.get(k), v) if k in ('RevisionSha', 'EffectiveSha', 'ClauseMapBlob') else g.get(k) == v
                                        for k, v in e.items()) for g, e in zip(cv['AuthorityResolution'], cexp['AuthorityResolution']))
    groups = {}
    for g, sids in GROUPS.items():
        groups[g] = all(v for k, v in key_ok.items() if int(k.split(':', 1)[0]) in sids)
    groups['OP_E_mapa_I62'] = groups['OP_E_mapa_I62'] and ar_ok
    groups['OP_F_esquemas_canonicos'] = groups['OP_F_esquemas_canonicos'] and all(dchk.values()) and all(cchk.values())
    return groups, key_ok, details, dchk, cchk, derr[:10], cerr[:10], ar_ok


def norm_hex_eq(a, b):
    if a is None or b is None:
        return a is None and b is None
    return isinstance(a, str) and isinstance(b, str) and a.strip().lower() == b.strip().lower()


def coherence_ok(v):
    try:
        pairs = {'EXECUTION_VERIFIED': 'NONE', 'EXECUTION_REWORK_REQUIRED': 'REWORK'}
        ok_pair = pairs.get(v['Classification']) == v['Disposition'] or (v['Classification'] == 'EXECUTION_BLOCKED' and v['Disposition'] in ('BLOCKED', 'STOP'))
        not_pass = [c for c in ORDER if v['Checks'][c]['Result'] != 'pass']
        ok_ver = (v['Classification'] == 'EXECUTION_VERIFIED') == (not not_pass)
        ok_cls = v['FailureClass'] == 'NONE' if not not_pass else v['FailureClass'] in (not_pass[0], 'StopCondition')
        ok_red = not any(v['Checks'][c]['Result'] == 'pass' and v['Checks'][c].get('RedPart') == 'fail' for c in ('Ci', 'Tests'))
        return ok_pair and ok_ver and ok_cls and ok_red
    except Exception:
        return False


def read_opt(outdir, name):
    p = os.path.join(outdir, name)
    return read_text(p) if os.path.exists(p) else None


def process_facts(outdir):
    # hechos del proceso que run_probe.sh registra alrededor de la sonda (iguales en los tres modos)
    exitinfo = (read_opt(outdir, 'probe-exit.txt') or 'missing').strip()
    log = read_opt(outdir, 'measurements.log') or ''
    pre = re.search(r'^pre-probe .*$', log, re.M)
    post = re.search(r'^post-probe .*$', log, re.M)
    strip_utc = lambda m: re.sub(r'utc=\S+ ', '', m.group(0).split(' ', 1)[1]) if m else None
    identity_stable = bool(pre and post and strip_utc(pre) == strip_utc(post)) and 'STOP' not in log
    same = lambda a, b: read_opt(outdir, a) is not None and read_opt(outdir, a) == read_opt(outdir, b)
    head_ok = same('probe-head-before.txt', 'probe-head-after.txt')
    sb = read_opt(outdir, 'probe-status-before.txt')
    tracked_ok = same('probe-status-before.txt', 'probe-status-after.txt') and sb is not None and sb.strip() == ''
    ignored_ok = same('probe-status-ignored-before-run.txt', 'probe-status-ignored-after-run.txt')
    exit_ok = exitinfo.startswith('exit=0 ') and 'timed_out=no' in exitinfo
    return {'exitinfo': exitinfo, 'exit_ok': exit_ok, 'identity_stable': identity_stable, 'head_ok': head_ok, 'tracked_ok': tracked_ok,
            'ignored_ok': ignored_ok}


def compare(outdir):
    mode = need_mode()
    if mode == 'NATIVE_ARCHITECT':
        return compare_architect(outdir)
    ref = json.loads(read_text(os.path.join(outdir, 'reference.json')))
    reasons = []
    try:
        probe = json.loads(read_text(os.path.join(outdir, 'probe-last.json')))
        schema_errs = validate(probe, json.loads(read_text(os.path.join(outdir, 'probe-schema.used.json')))) \
            if os.path.exists(os.path.join(outdir, 'probe-schema.used.json')) else []
    except Exception as e:
        probe, schema_errs = {}, [f'sin salida estructurada válida: {e.__class__.__name__}']
    if schema_errs:
        reasons.append('salida no válida contra el esquema de salida usado: ' + '; '.join(schema_errs[:3]))
    groups, key_ok, details, dchk, cchk, derr, cerr, ar_ok = compare_obj(outdir, probe, ref)
    ev = audit_events(os.path.join(outdir, 'probe-events.jsonl'), os.path.join(outdir, 'probe-stderr.txt'))
    route = native_route(ev)
    if mode == 'CMD_A41':
        # v3 sin cambios (la shell declarada prohíbe PowerShell, intérpretes y herramientas de Unix); git_write ya no cuenta merge-base
        groups['OP_A_shell'] = groups['OP_A_shell'] and ev['forbidden_completed'] == 0 and ev['git_write'] == 0
    else:
        # NATIVE_CONTROLLER: la receta literal no prohíbe ninguna shell ni herramienta; OP_A exige que el runtime ejecute comandos y que
        # ningún comando completado escriba (Git o archivos) o use la red (README §4)
        groups['OP_A_shell'] = (groups['OP_A_shell'] and ev['completed_ok'] >= 1 and ev['git_write_completed'] == 0
                                and ev['network_completed'] == 0 and ev['file_write_completed'] == 0)
    pf = process_facts(outdir)
    exitinfo, identity_stable, head_ok, tracked_ok, ignored_ok = (pf['exitinfo'], pf['identity_stable'], pf['head_ok'], pf['tracked_ok'],
                                                                   pf['ignored_ok'])
    un = read_opt(outdir, 'unstage-report.json')
    unstage_ok = bool(un) and (lambda r: not r['FilesChangedDuringRun'] and not r['FilesMissing'] and r['StatusIgnoredRestored']
                               and not r['NonEmptyDirsLeft'])(json.loads(un))
    b, a = read_opt(outdir, 'probe-lsremote-before.txt'), read_opt(outdir, 'probe-lsremote-after.txt')
    remote_stable = b is not None and b == a == ref['meta']['lsremote_live']
    if not pf['exit_ok']:
        reasons.append(f'salida del proceso: {exitinfo}')
    if not identity_stable:
        reasons.append('identidad no estable antes/después (A2-P2, regla 6; A4-1, regla 5: medición anulada)')
    if not head_ok or not tracked_ok:
        reasons.append('HEAD o árbol versionado del clon cambiaron, o el árbol no estaba limpio')
    if not ignored_ok:
        reasons.append('status --ignored cambió durante la sonda (escritura en el área ignorada)')
    if not unstage_ok:
        reasons.append('unstage: archivos preparados cambiados o ausentes, o status --ignored no restaurado')
    failed_keys = [k for k, v in key_ok.items() if not v]
    remote_keys = set(ref['meta']['remote_keys'])
    for g, ok in groups.items():
        if not ok:
            reasons.append(f'{g}: no demostrado')
    if mode == 'NATIVE_CONTROLLER' and route['ShellLimitationPersists']:
        reasons.append('F6-OBS-03 persiste: el runtime no puede lanzar la shell que selecciona (CreateProcessAsUserW); por A4-1 la vía normal no '
                       'es elegible aunque el modelo haya improvisado otra shell')
    if not remote_stable:
        reasons.append('referencia de Remote no establecida: ls-remote del origen distinto antes, después o en la comparación')
    only_remote = (all(k in remote_keys for k in failed_keys)
                   and all(v or k == 'field:MainSha' for k, v in dchk.items()) and all(cchk.values()) and ar_ok
                   and (ev['forbidden_completed'] == 0 if mode == 'CMD_A41' else groups['OP_A_shell'])
                   and not schema_errs and pf['exit_ok'] and identity_stable
                   and head_ok and tracked_ok and ignored_ok and unstage_ok
                   and not (mode == 'NATIVE_CONTROLLER' and route['ShellLimitationPersists']))
    if not reasons:
        verdict = 'ALL_OPERATIONS_DEMONSTRATED'
    elif not remote_stable and only_remote:
        verdict = 'REFERENCE_UNSTABLE'
    else:
        verdict = 'NOT_DEMONSTRATED'
    note = {'CMD_A41': ('NOT_DEMONSTRATED: la celda no es elegible y la invocación no se repite (A4-1, regla 4). ALL_OPERATIONS_DEMONSTRATED: la '
                        'elegibilidad la declara el Coordinator; la línea del Owner no acepta el resultado. REFERENCE_UNSTABLE: el origen cambió '
                        'durante la medición; lo dispone el Coordinator, sin reintento automático.'),
            'NATIVE_CONTROLLER': ('ALL_OPERATIONS_DEMONSTRATED (sin firma de F6-OBS-03): la celda del Controller sería elegible por la vía normal de '
                                  'la receta 16.4, sin A4-1; lo declara el Coordinator. NOT_DEMONSTRATED con ShellLimitationPersists: abre CMD_A41 '
                                  '(A4-1, reglas 3 y 4), con su disposición y su consumo. NOT_DEMONSTRATED sin la firma: no elegible; A4-1 no se abre '
                                  'por esta causa. REFERENCE_UNSTABLE: lo dispone el Coordinator, sin reintento automático.')}[mode]
    out = {
        'Schema': f'{KIT}-probe-comparison (propuesta de la supervisión; no es decisión)', 'Mode': mode, 'Verdict': verdict, 'Operations': groups,
        'FailedKeys': details, 'PlanCanonicalChecks': dchk, 'PlanSchemaErrors': derr, 'VerifyCanonicalChecks': cchk,
        'VerifySchemaErrors': cerr, 'AuthorityResolutionMatch': ar_ok, 'Process': exitinfo, 'IdentityStable': identity_stable,
        'HeadUnchanged': head_ok, 'TrackedTreeUnchangedAndClean': tracked_ok, 'IgnoredStatusUnchangedDuringRun': ignored_ok,
        'UnstageOk': unstage_ok, 'RemoteReferenceStable': remote_stable,
        'EventsCommandAudit': {k: v for k, v in ev.items() if k != 'list'}, 'EventsCommands': ev['list'],
        'NativeRoute': route, 'UsageLimitSignal': ev['usage_limit_signal'], 'ProcessResidue': process_residue(outdir),
        'Reasons': reasons, 'Note': note}
    write_json(os.path.join(outdir, 'comparison.json'), out)
    print(verdict, '; '.join(reasons))
    return verdict


# ------------------------------------------------------------------ NATIVE_ARCHITECT: referencia, comparación y clasificación de la celda
ARQ_GROUPS = {'ARQ_A_entorno': [1], 'ARQ_B_objeto_revisado': [2, 3], 'ARQ_C_autoridades_fieles': [4], 'ARQ_D_controles_negativos': [5],
              'ARQ_E_salida_canonica': [6]}
PROVIDER_EFFORTS = ['minimal', 'low', 'medium', 'high', 'xhigh', 'max']


def reference_architect(outdir):
    d = env('PROBE_DIR')
    tc, tp, tb, bad = env('TARGET_COMMIT'), env('CONTRACT_PATH'), env('TARGET_BLOB'), env('BAD_SHA')
    S = {}

    def put(step, key, v, cmp='exact'):
        S.setdefault(str(step), {})[key] = {'v': v, 'cmp': cmp}

    head = git(d, 'rev-parse', 'HEAD').strip()
    top = git(d, 'rev-parse', '--show-toplevel').strip()
    put(1, 'shell', None, 'present')
    put(1, 'cwd', top, 'path')
    put(1, 'HEAD', head, 'hex')
    put(1, 'branch', git(d, 'rev-parse', '--abbrev-ref', 'HEAD').strip(), 'exact')
    rc, typ = git_rc(d, 'cat-file', '-t', tc)
    put(2, 'target-type', typ.strip() if rc == 0 else f'rc={rc}', 'exact_ci')
    blob = blob_at(d, tc, tp)
    if blob is None:
        sys.exit('referencia: el objeto revisado no existe en el clon')
    put(2, 'target-blob', blob, 'hex')
    put(2, 'target-blob-match', yn(blob == tb), 'yesno')
    put(2, 'target-in-head', yn(is_anc(d, tc, head) == 0), 'yesno')
    put(2, 'target-blob-at-head', blob_at(d, 'HEAD', tp) or 'missing', 'exact_ci')
    K = json.loads(show(d, tc, tp).decode('utf-8'))
    for k in ('Schema', 'Unit', 'Gate', 'TaskId', 'AuthorityRevision', 'MainSha', 'ProtocolSet', 'Objective', 'Invariants'):
        put(3, f'json:{k}', json.dumps(K[k], ensure_ascii=False), 'json')
    put(3, 'json:StopConditions[1].Text', json.dumps(K['StopConditions'][1]['Text'], ensure_ascii=False), 'json')
    arq = [r for r in K['RoleRequirements'] if r['Role'] == 'ARCHITECT']
    if len(arq) != 1:
        sys.exit('referencia: K no tiene exactamente un RoleRequirements de ARCHITECT')
    put(3, 'json:ARCHITECT.Mandatory', json.dumps(arq[0]['Mandatory'], ensure_ascii=False), 'json')
    put(3, 'json:ARCHITECT.Independence', json.dumps(arq[0]['Independence'], ensure_ascii=False), 'json')
    put(3, 'json:Objective.nonascii-count', len(nonascii(K['Objective'])), 'int')
    for k in ('Authorities', 'AllowedWriteScope', 'ForbiddenWriteScope', 'RoleRequirements'):
        put(3, f'len:{k}', len(K[k]), 'int')
    ar, main = K['AuthorityRevision'], K['MainSha']
    auths = []
    for a in K['Authorities']:
        p = a['Path']
        rev = ar if a['Class'] in ('UNIT_DOC', 'UNIT_CHANGE') else main
        b = blob_at(d, rev, p)
        put(4, f'auth:{p}:rev', rev, 'hex')
        put(4, f'auth:{p}:blob', b if b else 'missing', 'hex' if b else 'exact_ci')
        raw = show(d, rev, p)
        if raw is None:
            sys.exit(f'referencia: la autoridad {p} no existe en {rev} (el kit no la prevé ausente)')
        t = raw.decode('utf-8')
        ls = md_lines(t)
        if a['Section'] == 'documento completo':
            put(4, f'auth:{p}:first-line', ls[0].rstrip('\r'), 'verbatim')
        else:
            hl = [i for i, _, h in md_headings(t) if h == norm_ws(a['Section'])]
            if len(hl) != 1:
                sys.exit(f'referencia: el encabezado {a["Section"]!r} aparece {len(hl)} veces en {p}@{rev}')
            put(4, f'auth:{p}:heading-line', hl[0] + 1, 'int')
            put(4, f'auth:{p}:first-line', next(x for x in ls[hl[0] + 1:] if x.strip()).rstrip('\r'), 'verbatim')
        auths.append({'Path': p, 'Rev': rev, 'Blob': b})
    fid = md_lines(show(d, ar, env('ARQ_FID_PATH')).decode('utf-8'))[0].rstrip('\r')
    na = nonascii(fid)
    put(4, 'fid:nonascii-count', len(na), 'int')
    put(4, 'fid:nonascii-chars', ';'.join(f'{c}={na.count(c)}' for c in sorted(set(na), key=ord)), 'verbatim')
    put(5, 'bad-rc', git_rc(d, 'cat-file', '-t', bad)[0], 'int')
    put(5, 'decoy-blob-match', yn(env('ARQ_DECOY_BLOB') == blob), 'yesno')
    put(5, 'missing-rc', git_rc(d, 'cat-file', '-t', f'{ar}:{env("ARQ_MISSING_PATH")}')[0], 'int')
    put(6, 'schema:architect-review-result.v1', blob_at(d, 'HEAD', ARQ_SCHEMA_PATH), 'hex')
    exact = {'Schema': 'rackcad-architect-review-result/v1', 'ResultId': ARQ_RESULT_ID, 'LogicalReviewRequestId': ARQ_LRR_ID,
             'InvocationId': ARQ_INVOCATION_ID, 'AttemptSeq': 1, 'RequestedRole': 'ARCHITECT', 'Action': 'REVIEW_DESIGN',
             'ReviewedUnit': K['Unit'], 'ReviewedCommit': tc, 'ReviewedPath': tp, 'ReviewedBlob': blob, 'ReviewerBinding': ARQ_BINDING,
             'ReviewerMode': 'SEPARATE SESSION', 'InputFidelityEvidenceRef': ARQ_IFE}
    ref = {'Kit': KIT, 'Mode': MODE, 'steps': S,
           'arq': {'exact': exact, 'RequiredInputs': [tp] + [x['Path'] for x in auths if x['Blob']],
                   'PremiseRevs': sorted({tc, ar, main, head}), 'Authorities': auths},
           'meta': {'head': head}}
    write_json(os.path.join(outdir, 'reference.json'), ref)
    print('reference ok')


def path_listed(p, entries):
    rx = re.compile(r'(?<![\w./\\-])' + re.escape(p) + r'(?![\w./\\-])')
    return any(isinstance(e, str) and rx.search(e.replace('\\', '/')) for e in entries or [])


def premise_ok(d, pr, revs):
    try:
        p, a, b, q = pr['Path'], int(pr['LineStart']), int(pr['LineEnd']), norm_ws(pr['Quote'])
    except Exception:
        return False
    if b < a or a < 1 or not q:
        return False
    for rev in revs:
        raw = show(d, rev, p)
        if raw is None:
            continue
        seg = norm_ws('\n'.join(md_lines(raw.decode('utf-8'))[a - 1:b]))
        if q in seg:
            return True
    return False


def step_keys(probe, ref):
    steps, key_ok, details = {}, {}, {}
    for s in probe.get('steps', []) if isinstance(probe, dict) else []:
        if isinstance(s, dict) and isinstance(s.get('id'), int):
            steps.setdefault(s['id'], s)
    for sid, keys in ref['steps'].items():
        vals = {}
        for p in steps.get(int(sid), {}).get('values', []) or []:
            if isinstance(p, dict) and 'key' in p and p['key'] not in vals:
                vals[p['key']] = p.get('value')
        for k, spec in keys.items():
            ok = k in vals and cmp_value(spec['cmp'], vals[k], spec['v'])
            key_ok[f'{sid}:{k}'] = ok
            if not ok:
                details[f'{sid}:{k}'] = {'expected': spec['v'] if not isinstance(spec['v'], str) else spec['v'][:300],
                                         'got': (vals.get(k) or '')[:300] if k in vals else 'FALTA'}
    return key_ok, details


def compare_architect(outdir):
    ref = json.loads(read_text(os.path.join(outdir, 'reference.json')))
    d = env('PROBE_DIR')
    reasons = []
    try:
        probe = json.loads(read_text(os.path.join(outdir, 'probe-last.json')))
        schema_errs = validate(probe, json.loads(read_text(os.path.join(outdir, 'probe-schema.used.json')))) \
            if os.path.exists(os.path.join(outdir, 'probe-schema.used.json')) else []
    except Exception as e:
        probe, schema_errs = {}, [f'sin salida estructurada válida: {e.__class__.__name__}']
    if schema_errs:
        reasons.append('salida no válida contra el esquema de salida usado: ' + '; '.join(schema_errs[:3]))
    key_ok, details = step_keys(probe, ref)
    canon = json.loads(show(d, 'HEAD', ARQ_SCHEMA_PATH).decode('utf-8'))
    a = probe.get('architect_review_result_v1') if isinstance(probe, dict) else None
    aerr = validate(a, canon) if isinstance(a, dict) else ['ausente']
    a = a if isinstance(a, dict) else {}
    achk = {'schema_valid': not aerr}
    for k, v in ref['arq']['exact'].items():
        achk[f'field:{k}'] = k in a and (norm_hex_eq(a[k], v) if k in ('ReviewedCommit', 'ReviewedBlob') else a[k] == v)
    ie = a.get('IndependenceEvidence') if isinstance(a.get('IndependenceEvidence'), dict) else {}
    dims = sorted((x.get('ReferenceRole'), x.get('Dimension')) for x in ie.get('Dimensions') or [] if isinstance(x, dict))
    achk['independence:AUTHOR_cuatro_dimensiones'] = dims == sorted(('AUTHOR', x) for x in ARQ_DIMS)
    achk['independence:ReviewSubject_null'] = 'ReviewSubject' in ie and ie['ReviewSubject'] is None
    missing_inputs = [p for p in ref['arq']['RequiredInputs'] if not path_listed(p, a.get('InputsRead'))]
    achk['InputsRead_cubre_objeto_y_autoridades'] = not missing_inputs
    prs = []
    for fk in ('RequiredFindings', 'OptionalFindings', 'FindingDispositions'):
        for f in a.get(fk) or []:
            if isinstance(f, dict):
                prs.extend(x for x in f.get('PremiseRefs') or [] if isinstance(x, dict))
    bad_prs = [{'Path': x.get('Path'), 'LineStart': x.get('LineStart'), 'LineEnd': x.get('LineEnd')} for x in prs
               if not premise_ok(d, x, ref['arq']['PremiseRevs'])]
    achk['PremiseRefs_textuales'] = not bad_prs
    groups = {g: all(v for k, v in key_ok.items() if int(k.split(':', 1)[0]) in sids) for g, sids in ARQ_GROUPS.items()}
    ev = audit_events(os.path.join(outdir, 'probe-events.jsonl'), os.path.join(outdir, 'probe-stderr.txt'))
    route = native_route(ev)
    policy_ok = ev['git_write_completed'] == 0 and ev['network_completed'] == 0 and ev['file_write_completed'] == 0
    executes = ev['completed_ok'] >= 1
    groups['ARQ_A_entorno'] = groups['ARQ_A_entorno'] and executes and policy_ok
    groups['ARQ_E_salida_canonica'] = groups['ARQ_E_salida_canonica'] and all(achk.values()) and not schema_errs
    pf = process_facts(outdir)
    # modelo y effort efectivos: los lee la supervisión del registro de sesión de esta invocación (effective-model-effort.json en outdir)
    eme = read_opt(outdir, 'effective-model-effort.json')
    obs = json.loads(eme) if eme else None
    if obs is None:
        routing = {'State': 'PENDING', 'Note': 'falta effective-model-effort.json (procedimiento, paso 6): la clasificación es provisional'}
    else:
        m, e = obs.get('Model'), (obs.get('Effort') or '').lower()
        ei = PROVIDER_EFFORTS.index(e) if e in PROVIDER_EFFORTS else -1
        req = PROVIDER_EFFORTS.index(ARQ_CELL_EFFORT)
        routing = {'State': ('MODEL_MISMATCH' if m != ARQ_CELL_MODEL else ('MATCH' if ei == req else ('ABOVE_REQUIRED' if ei > req else 'BELOW_REQUIRED'))),
                   'Model': m, 'Effort': e}
    for g, ok in groups.items():
        if not ok:
            reasons.append(f'{g}: no demostrado')
    if not pf['exit_ok']:
        reasons.append(f'salida del proceso: {pf["exitinfo"]}')
    if not pf['identity_stable']:
        reasons.append('identidad no estable antes/después (A2-P2, regla 6: medición anulada)')
    if not (pf['head_ok'] and pf['tracked_ok'] and pf['ignored_ok']):
        reasons.append('HEAD, árbol versionado o status --ignored del clon cambiaron durante la sonda, o el clon no estaba limpio')
    causes = []
    if not executes:
        causes.append('EXECUTION_REJECTED' if route['F6Obs03Signature']['present'] else 'NO_COMMAND_EXECUTED')
    if not policy_ok:
        causes.append('WRITE_OR_NETWORK')
    if not all(groups[g] for g in ('ARQ_B_objeto_revisado', 'ARQ_C_autoridades_fieles', 'ARQ_D_controles_negativos')) or \
            not all(v for k, v in key_ok.items() if k.startswith('1:')):
        causes.append('READ_NOT_DEMONSTRATED')
    if not groups['ARQ_E_salida_canonica']:
        causes.append('OUTPUT_CONTRACT')
    if not pf['exit_ok']:
        causes.append('PROCESS_EXIT_OR_TIMEOUT')
    if not (pf['head_ok'] and pf['tracked_ok'] and pf['ignored_ok']):
        causes.append('WORKTREE_CHANGED')
    if routing['State'] == 'BELOW_REQUIRED':
        causes.append('EFFORT_BELOW_REQUIRED')
    if not pf['identity_stable'] or routing['State'] == 'MODEL_MISMATCH':
        cls = 'MEASUREMENT_VOID'
    elif causes:
        cls = 'BELOW_REQUIRED'
    else:
        cls = 'MATCH_OR_ABOVE_REQUIRED'
    read_state = 'MATCH' if 'READ_NOT_DEMONSTRATED' not in causes and executes and 'EXECUTION_REJECTED' not in causes else 'BELOW_REQUIRED'
    requirements = {
        'ARCHITECTURE_REVIEW.read': {'State': read_state if cls != 'MEASUREMENT_VOID' else 'UNKNOWN',
                                     'Basis': 'pasos 1-5 (lectura fiel del objeto y de las autoridades en sus revisiones) y ejecución de comandos'},
        'ARCHITECTURE_REVIEW.effort': {'State': routing['State'] if routing['State'] != 'MODEL_MISMATCH' else 'UNKNOWN',
                                       'Required': 'Deep → high (routing §1; catálogo gpt-6.1-sol)', 'Observed': routing.get('Effort')},
        'ARCHITECTURE_REVIEW.level': {'State': ('MATCH' if routing['State'] not in ('PENDING', 'MODEL_MISMATCH') else
                                                ('PENDING' if routing['State'] == 'PENDING' else 'UNKNOWN')),
                                      'Required': 'Equilibrado o Frontera (routing §3, Deep)', 'Catalog': f'{ARQ_CELL_MODEL} = {ARQ_CELL_LEVEL}'},
        'adapter.op5_resultado_canonico': {'State': 'DEMONSTRATED' if groups['ARQ_E_salida_canonica'] else 'NOT_DEMONSTRATED',
                                           'Schema': 'rackcad-architect-review-result/v1 (incrustado sin $schema)'}}
    verdict = 'ALL_OPERATIONS_DEMONSTRATED' if not reasons else 'NOT_DEMONSTRATED'
    out = {
        'Schema': f'{KIT}-architect-comparison (propuesta de la supervisión; no es decisión)', 'Mode': 'NATIVE_ARCHITECT', 'Verdict': verdict,
        'ArchitectCellClassification': cls, 'ClassificationFinal': cls != 'MATCH_OR_ABOVE_REQUIRED' or routing['State'] in ('MATCH', 'ABOVE_REQUIRED'),
        'BelowCauses': causes if cls == 'BELOW_REQUIRED' else [], 'Requirements': requirements, 'Routing': routing,
        'Operations': groups, 'FailedKeys': details, 'CanonicalChecks': achk, 'CanonicalSchemaErrors': aerr[:10],
        'MissingInputsRead': missing_inputs, 'PremiseRefsChecked': len(prs), 'PremiseRefsNotVerbatim': bad_prs[:10],
        'DeclaredVerdict': a.get('Verdict'), 'DeclaredIdentity': a.get('ReviewerDeclaredIdentity'),
        'Process': pf['exitinfo'], 'IdentityStable': pf['identity_stable'], 'HeadUnchanged': pf['head_ok'],
        'TrackedTreeUnchangedAndClean': pf['tracked_ok'], 'IgnoredStatusUnchangedDuringRun': pf['ignored_ok'],
        'EventsCommandAudit': {k: v for k, v in ev.items() if k != 'list'}, 'EventsCommands': ev['list'], 'NativeRoute': route,
        'UsageLimitSignal': ev['usage_limit_signal'], 'ProcessResidue': process_residue(outdir), 'Reasons': reasons,
        'Note': ('MATCH_OR_ABOVE_REQUIRED: la celda Codex del Architect cumple con la identidad vigente los requisitos medidos; A4-2 no se activa. '
                 'BELOW_REQUIRED: requisito obligatorio por debajo con la identidad vigente; con disposición del Coordinator, A4-2 permite otro '
                 'adapter medido elegible (claude-cli, con su propia medición). MEASUREMENT_VOID: identidad o modelo distintos; no clasifica. '
                 'Si NativeRoute.ModelFallback es true, la lectura se obtuvo con una shell que el modelo eligió tras el rechazo del runtime: '
                 'lo valora el Coordinator (README §8, Q-ARQ-FALLBACK).')}
    write_json(os.path.join(outdir, 'comparison.json'), out)
    print(verdict, cls, '; '.join(reasons))
    return verdict


# ------------------------------------------------------------------ synth (autoensayo)
def synth(outdir, outfile):
    if need_mode() == 'NATIVE_ARCHITECT':
        return synth_architect(outdir, outfile)
    ref = json.loads(read_text(os.path.join(outdir, 'reference.json')))
    steps = []
    for sid in sorted(ref['steps'], key=int):
        vals = []
        for k, spec in ref['steps'][sid].items():
            v, c = spec['v'], spec['cmp']
            if c == 'present':
                s = SYNTH_SHELL if k == 'shell' else 'Microsoft Windows [Versión 10.0.26300]'
            elif c == 'contains':
                s = 'C:\\WINDOWS\\system32\\cmd.exe'
            elif c == 'path':
                s = v.replace('/', '\\')
            elif isinstance(v, list):
                s = '\n'.join(v)
            else:
                s = str(v)
            vals.append({'key': k, 'value': s})
        cmd = 'cmd.exe /d /s /c "(sintético)"' if MODE == 'CMD_A41' else '(sintético)'
        steps.append({'id': int(sid), 'commands': [cmd], 'ok': True, 'values': vals, 'error': None})
    dexp = copy.deepcopy(ref['delegation']['exact'])
    dl = dict(dexp)
    dl.update({'ExpectedWorktree': ref['delegation']['ExpectedWorktree'].replace('/', '\\'), 'TaskClass': 'Implementación de pruebas',
               'Dimensions': {'Ambiguity': 'Low', 'ArchitecturalSensitivity': 'Low', 'Breadth': 'Low', 'ToolUse': 'Medium', 'FailureCost': 'Low',
                              'MechanicalRepetition': 'Low', 'Horizon': 'Low'},
               'Executor': dict(ref['delegation']['ExecutorBase'], Cell='claude-subagent:claude-sonnet-5-5:Balanced'),
               'Effort': {'Semantic': 'Balanced', 'Provider': 'medium'}, 'PromptProfile': 'ROUTINE_IMPLEMENTATION',
               'RoutingReason': 'Clase de §1 «Implementación de pruebas» (sintético).', 'ModelEscalationReason': None,
               'AcceptanceCriteria': ['Subtract con su prueba (sintético).'], 'StopConditions': ref['delegation']['StopConditions']})
    dl = {k: dl[k] for k in CANON_ORDER_DELEGATION}
    cexp = ref['cv']['exact']
    checks = {}
    for c in ORDER:
        checks[c] = {'Result': ref['cv']['checks'][c], 'Evidence': ('Diff: ' + ' '.join(ref['cv']['scope_diff'])) if c == 'Scope' else 'sintético'}
        if c in ('Ci', 'Tests'):
            checks[c] = {'Result': ref['cv']['checks'][c], 'RedPart': ref['cv']['checks'][f'{c}.RedPart'], 'Evidence': 'sintético'}
    cv = {'Schema': cexp['Schema'], 'TaskId': cexp['TaskId'], 'RunId': cexp['RunId'], 'DelegationRunId': cexp['DelegationRunId'],
          'WorkRunId': cexp['WorkRunId'], 'Attempt': cexp['Attempt'], 'VerifiedSha': cexp['VerifiedSha'], 'Verifier': cexp['Verifier'],
          'Checks': checks, 'AuthorityResolution': cexp['AuthorityResolution'], 'Classification': cexp['Classification'],
          'Disposition': cexp['Disposition'], 'FailureClass': cexp['FailureClass'], 'TriggeredStopConditions': [], 'Findings': [],
          'RecommendedNextAction': 'sintético'}
    head = next(x['value'] for x in steps[1]['values'] if x['key'] == 'HEAD')
    write_json(outfile, {'steps': steps, 'head_sha': head, 'delegation_v2': dl, 'controller_verification_v2': cv})
    print('synth ok')


def synth_architect(outdir, outfile):
    ref = json.loads(read_text(os.path.join(outdir, 'reference.json')))
    d = env('PROBE_DIR')
    steps = []
    for sid in sorted(ref['steps'], key=int):
        vals = []
        for k, spec in ref['steps'][sid].items():
            v, c = spec['v'], spec['cmp']
            s = SYNTH_SHELL if c == 'present' else (v.replace('/', '\\') if c == 'path' else ('\n'.join(v) if isinstance(v, list) else str(v)))
            vals.append({'key': k, 'value': s})
        steps.append({'id': int(sid), 'commands': ['(sintético)'], 'ok': True, 'values': vals, 'error': None})
    # una cita textual real: la primera línea no vacía tras «## Jobs requeridos» de AGENTS.md en MainSha (EXTERNAL)
    ag = [x for x in ref['arq']['Authorities'] if x['Path'] == 'AGENTS.md'][0]
    ls = md_lines(show(d, ag['Rev'], 'AGENTS.md').decode('utf-8'))
    h = [i for i, _, t in md_headings('\n'.join(ls)) if t == '## Jobs requeridos'][0]
    j = next(i for i in range(h + 1, len(ls)) if ls[i].strip())
    pr = {'Path': 'AGENTS.md', 'Section': '## Jobs requeridos', 'LineStart': j + 1, 'LineEnd': j + 1, 'Quote': ls[j]}
    ex = ref['arq']['exact']
    arr = dict(ex)
    arr.update({
        'ReviewerDeclaredIdentity': {'Runtime': 'codex-cli', 'Model': 'UNKNOWN', 'Effort': 'UNKNOWN', 'SessionOrThread': 'UNKNOWN'},
        'InjectedContextDeclaration': {'State': 'DECLARED', 'Items': [{'Kind': 'AUTOMATIC_INSTRUCTIONS', 'Source': 'AGENTS.md',
                                                                      'SizeOrSha256': str(len(show(d, 'HEAD', 'AGENTS.md')))}]},
        'InputsRead': list(ref['arq']['RequiredInputs']),
        'IndependenceEvidence': {'Dimensions': [{'ReferenceRole': 'AUTHOR', 'Dimension': x, 'Result': 'UNKNOWN', 'Evidence': 'sintético'}
                                                for x in ARQ_DIMS], 'ReviewSubject': None},
        'KnownLimitations': ['sintético'], 'RecommendedNextAction': 'sintético', 'Verdict': 'AGREED', 'RequiredFindings': [],
        'OptionalFindings': [{'FindingId': 'OF-1', 'LineageId': None, 'Source': 'ARCHITECT', 'AffectedSection': 'RequiredTests',
                              'Evidence': 'sintético', 'PremiseRefs': [pr], 'WhyItMatters': 'sintético', 'CorrectionRequired': 'ninguna (sintético)'}],
        'FindingDispositions': [], 'Downgrades': [], 'OwnerDecisions': []})
    head = next(x['value'] for x in steps[0]['values'] if x['key'] == 'HEAD')
    write_json(outfile, {'steps': steps, 'head_sha': head, 'architect_review_result_v1': arr})
    print('synth ok')


CANON_ORDER_DELEGATION = ['Schema', 'TaskId', 'RunId', 'Initiative', 'Unit', 'Gate', 'Attempt', 'AuthorityRevision', 'MainSha', 'BaseSha',
                          'ExpectedBranch', 'ExpectedWorktree', 'Owner', 'TaskClass', 'Dimensions', 'Executor', 'Model', 'Effort', 'PromptProfile',
                          'RoutingReason', 'ModelEscalationReason', 'RoutingEnforcement', 'Authorities', 'Objective', 'AllowedWriteScope',
                          'ForbiddenWriteScope', 'Invariants', 'AcceptanceCriteria', 'RequiredTests', 'ExpectedEvidence', 'StopConditions',
                          'MaxReworkLoops', 'AttemptsRemaining', 'ChainBaseSha', 'ChainRedSha', 'ChainRedFiles', 'CorrectionOf',
                          'ExpectedHandoffPath', 'IssuedBy', 'RoleRequirements', 'SupersededCommits']


# ------------------------------------------------------------------ selftest
def _write(p, text):
    with open(p, 'w', encoding='utf-8', newline='\n') as f:
        f.write(text)


def _cmd_event(command, ok=True, out='', code=None):
    return {'type': 'item.completed', 'item': {'type': 'command_execution', 'command': command, 'aggregated_output': out,
                                               'exit_code': (0 if ok else 1) if code is None else code, 'status': 'completed' if ok else 'failed'}}


_RT = '"C:\\\\Users\\\\<usuario>\\\\.cache\\\\codex-runtimes\\\\codex-primary-runtime\\\\dependencies\\\\native\\\\powershell\\\\pwsh.exe" -Command '
_WA = '"C:\\\\Users\\\\<usuario>\\\\AppData\\\\Local\\\\Microsoft\\\\WindowsApps\\\\pwsh.exe" -Command '
_CMD = '"C:\\\\WINDOWS\\\\system32\\\\cmd.exe" /c '
_F6_OUT = ('Failed to create unified exec process: CreateProcessAsUserW failed: 5 (Acceso denegado.) | cwd=D:\\r62-fixture\\x | '
           'cmd=C:\\Users\\<usuario>\\AppData\\Local\\Microsoft\\WindowsApps\\pwsh.exe -Command "git rev-parse HEAD"')
EVENTS = {
    'cmd': [_cmd_event(_CMD + "'git rev-parse HEAD'")],
    'pwsh_rt': [_cmd_event(_RT + "'git rev-parse HEAD'")],
    'f6_fallback': [_cmd_event(_WA + "'git rev-parse HEAD'", False, _F6_OUT, -1), _cmd_event(_WA + "'git status --porcelain'", False, _F6_OUT, -1),
                    _cmd_event(_CMD + "'git rev-parse HEAD'")],
    'f6_nofallback': [_cmd_event(_WA + "'git rev-parse HEAD'", False, _F6_OUT, -1), _cmd_event(_WA + "'git log -1'", False, _F6_OUT, -1),
                      _cmd_event(_WA + "'git status --porcelain'", False, _F6_OUT, -1)],
    'merge_base_cmd': [_cmd_event(_CMD + "'git merge-base --is-ancestor a b & git merge-base x y'")],
    'merge_base_rt': [_cmd_event(_RT + "'git merge-base --is-ancestor a b; git stash list; git tag -l; git branch --show-current'")],
    'python_rt': [_cmd_event(_RT + "'git rev-parse HEAD'"), _cmd_event(_RT + "'python -c \"print(1)\"'")],
    'python_cmd': [_cmd_event(_CMD + "'git rev-parse HEAD'"), _cmd_event(_CMD + "'python -c \"print(1)\"'")],
    'fetch_ok': [_cmd_event(_RT + "'git rev-parse HEAD'"), _cmd_event(_RT + "'git fetch origin'")],
    'fetch_failed': [_cmd_event(_RT + "'git rev-parse HEAD'"), _cmd_event(_RT + "'git fetch origin'", False, 'fatal: sandbox', 128)],
    'fetch_cmd': [_cmd_event(_CMD + "'git rev-parse HEAD'"), _cmd_event(_CMD + "'git fetch origin'")],
    'write_ok': [_cmd_event(_RT + "'git rev-parse HEAD'"), _cmd_event(_RT + "'Set-Content -Path x.txt -Value 1'")],
    'redirect_ok': [_cmd_event(_RT + "'git rev-parse HEAD > head.txt'")],
    'net_ok': [_cmd_event(_RT + "'git rev-parse HEAD'"), _cmd_event(_RT + "'Invoke-WebRequest https://example.invalid'")],
    'lsremote_github': [_cmd_event(_RT + "'git ls-remote github refs/heads/main'")],
    'benign_rt': [_cmd_event(_RT + "'git ls-remote origin refs/heads/main 2>$null; git cat-file -t x 2>&1; Get-Content -Raw a.md | Out-Null'"),
                  _cmd_event(_RT + "'Select-String -Pattern \"git push\", \"Coordinador del fixture\", \"https://x\" -Path docs/WORKFLOW.md'"),
                  _cmd_event(_CMD + "'findstr /n /c:\"copia del contrato\" /c:\"git fetch\" docs\\AUTOMATION_PLAN.md'")],
}


def _get(obj, dotted):
    # ruta con puntos; admite claves que contienen puntos (p. ej. «ARCHITECTURE_REVIEW.read»): en cada nivel, la clave más larga que exista
    parts, cur, i = dotted.split('.'), obj, 0
    while i < len(parts):
        if not isinstance(cur, dict):
            return '<ausente>'
        for j in range(len(parts), i, -1):
            key = '.'.join(parts[i:j])
            if key in cur:
                cur, i = cur[key], j
                break
        else:
            return '<ausente>'
    return cur


def selftest(kit, outdir, report):
    mode = need_mode()
    d = env('PROBE_DIR')
    controller = mode in CONTROLLER_MODES
    os.makedirs(outdir, exist_ok=True)
    rep = {'Kit': KIT, 'Mode': mode, 'UtcStart': datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ'),
           'NoModelInvoked': True, 'CodexHomeRead': False, 'ProbeDirBasename': os.path.basename(d.rstrip('\\/')), 'Steps': {}, 'Cases': []}
    hist = {'0-before-stage': git(d, 'status', '--porcelain', '--ignored')}
    if controller:
        rc = subprocess.run([sys.executable, '-I', os.path.abspath(__file__), 'stage', outdir]).returncode
        rep['Steps']['stage'] = rc
        if rc:
            write_json(report, rep)
            sys.exit('selftest: stage falló')
    hist['1-after-stage'] = git(d, 'status', '--porcelain', '--ignored')
    head = git(d, 'rev-parse', 'HEAD')
    lsr = git(d, 'ls-remote', 'origin')
    _write(os.path.join(outdir, 'probe-exit.txt'), 'exit=0 timed_out=no\n')
    _write(os.path.join(outdir, 'measurements.log'),
           'pre-probe utc=2026-10-11T00:00:00Z fp=SINTETICO structure=EQUAL bin=SINTETICO bins=1 app=26.1007.2314.0\n'
           'post-probe utc=2026-10-11T00:05:00Z fp=SINTETICO structure=EQUAL bin=SINTETICO bins=1 app=26.1007.2314.0\n')
    for n in ('probe-head-before.txt', 'probe-head-after.txt'):
        _write(os.path.join(outdir, n), head)
    st = git(d, 'status', '--porcelain')
    for n in ('probe-status-before.txt', 'probe-status-after.txt'):
        _write(os.path.join(outdir, n), st)
    for n in ('probe-status-ignored-before-run.txt', 'probe-status-ignored-after-run.txt'):
        _write(os.path.join(outdir, n), hist['1-after-stage'])
    for n in ('probe-lsremote-before.txt', 'probe-lsremote-after.txt'):
        _write(os.path.join(outdir, n), lsr)
    shutil.copyfile(schema_file_for_mode(kit), os.path.join(outdir, 'probe-schema.used.json'))
    reference(outdir)
    hist['2-after-run'] = git(d, 'status', '--porcelain', '--ignored')
    if controller:
        rep['Steps']['unstage'] = subprocess.run([sys.executable, '-I', os.path.abspath(__file__), 'unstage', outdir]).returncode
    hist['3-after-unstage'] = git(d, 'status', '--porcelain', '--ignored')
    rep['StatusIgnored'] = {k: v.splitlines() for k, v in hist.items()}
    rep['StatusIgnoredRestored'] = hist['3-after-unstage'] == hist['0-before-stage']
    perfect_path = os.path.join(outdir, 'synthetic-perfect.json')
    synth(outdir, perfect_path)
    perfect = json.loads(read_text(perfect_path))

    def mutate_value(p, sid, key, fn):
        for s in p['steps']:
            if s['id'] == sid:
                for kv in s['values']:
                    if kv['key'] == key:
                        kv['value'] = fn(kv['value'])
                        return p
        raise KeyError(f'{sid}:{key}')

    def drop_key(p, sid, key):
        for s in p['steps']:
            if s['id'] == sid:
                s['values'] = [kv for kv in s['values'] if kv['key'] != key]
        return p

    def setp(path, value):
        def f(p):
            cur = p
            parts = path.split('.')
            for x in parts[:-1]:
                cur = cur[int(x)] if isinstance(cur, list) else cur[x]
            last = parts[-1]
            if isinstance(cur, list):
                cur[int(last)] = value
            else:
                cur[last] = value
            return p
        return f

    flip = lambda v: ('0' if v[0] != '0' else '1') + v[1:]
    ND, ALL, RU = 'NOT_DEMONSTRATED', 'ALL_OPERATIONS_DEMONSTRATED', 'REFERENCE_UNSTABLE'
    # (nombre, veredicto esperado, comprobaciones adicionales {ruta en comparison.json: valor}, mutación, proceso, eventos, effective-model-effort)
    if mode == 'CMD_A41':
        hf0 = env('HASH_FILES').split()[0]
        cases = [
            ('P0_perfecta', ALL, {}, lambda p: p, None, 'cmd', None),
            ('N1_falta_clave_status', ND, {}, lambda p: drop_key(p, 10, 'status'), None, 'cmd', None),
            ('N2_hash_erroneo', ND, {}, lambda p: mutate_value(p, 7, f'sha256:{hf0}', flip), None, 'cmd', None),
            ('N3_conteo_trx_erroneo', ND, {}, lambda p: mutate_value(p, 14, 'trx:passed', lambda v: str(int(v) + 1)), None, 'cmd', None),
            ('N4_mapa_veredicto_erroneo', ND, {}, lambda p: mutate_value(p, 22, 'map-verdict', lambda v: 'MAP_VALID' if v == 'MAP_INVALID' else 'MAP_INVALID'), None, 'cmd', None),
            ('N4b_ClauseMapBlob_erroneo', ND, {}, setp('controller_verification_v2.AuthorityResolution.0.ClauseMapBlob', '0' * 40), None, 'cmd', None),
            ('N5_copia_no_ascii_no_textual', ND, {}, lambda p: mutate_value(p, 11, 'md-line', lambda v: v.replace('→', '->').replace('«', '"')), None, 'cmd', None),
            ('N5b_Objective_sin_tilde', ND, {}, lambda p: (p['delegation_v2'].__setitem__('Objective', p['delegation_v2']['Objective'].replace('ñ', 'n')), p)[1], None, 'cmd', None),
            ('N6_ls_remote_alterado_en_salida', ND, {}, lambda p: mutate_value(p, 9, 'ls-remote:branch', flip), None, 'cmd', None),
            ('N6b_ls_remote_del_origen_cambio', RU, {}, lambda p: p, 'lsremote', 'cmd', None),
            ('N7_delegacion_invalida_esquema', ND, {}, setp('delegation_v2.MaxReworkLoops', 4), None, 'cmd', None),
            ('N8_verificacion_incoherente', ND, {}, setp('controller_verification_v2.Classification', 'EXECUTION_VERIFIED'), None, 'cmd', None),
            ('N9_interprete_completado', ND, {'Operations.OP_A_shell': False}, lambda p: p, None, 'python_cmd', None),
            ('N10_tope_vencido', ND, {}, lambda p: p, 'timeout', 'cmd', None),
            ('N11_escritura_en_area_ignorada', ND, {}, lambda p: p, 'ignored', 'cmd', None),
            ('N12_routing_incoherente', ND, {}, setp('delegation_v2.Effort.Semantic', 'Deep'), None, 'cmd', None),
            ('N13_falta_clave_vacia_rt', ND, {}, lambda p: drop_key(p, 5, 'rt'), None, 'cmd', None),
            ('N14_negativo_nc2_erroneo', ND, {}, lambda p: mutate_value(p, 26, 'nc2-Scope', lambda v: 'pass'), None, 'cmd', None),
            ('N15_N7b_campos_erroneos', ND, {}, lambda p: mutate_value(p, 32, 'n7b-fields', lambda v: 'RunId'), None, 'cmd', None),
            ('N16_verificacion_check_erroneo', ND, {}, setp('controller_verification_v2.Checks.Trailer.Result', 'pass'), None, 'cmd', None),
            ('C1_merge_base_no_es_escritura', ALL, {'EventsCommandAudit.git_write': 0}, lambda p: p, None, 'merge_base_cmd', None),
            ('C2_fetch_es_escritura', ND, {'Operations.OP_A_shell': False}, lambda p: p, None, 'fetch_cmd', None),
        ]
    elif mode == 'NATIVE_CONTROLLER':
        hf0 = env('HASH_FILES').split()[0]
        cases = [
            ('P0_pwsh_runtime', ALL, {'NativeRoute.DefaultShell': 'pwsh-runtime', 'NativeRoute.ShellLimitationPersists': False}, lambda p: p, None, 'pwsh_rt', None),
            ('P1_cmd_sin_firma', ALL, {'NativeRoute.DefaultShell': 'cmd', 'NativeRoute.CmdA41Opens': False}, lambda p: p, None, 'cmd', None),
            ('P2_interprete_libre', ALL, {'EventsCommandAudit.interpreter': 1}, lambda p: p, None, 'python_rt', None),
            ('P3_mergebase_no_escribe', ALL, {'EventsCommandAudit.git_write': 0}, lambda p: p, None, 'merge_base_rt', None),
            ('P4_fetch_fallido', ALL, {'EventsCommandAudit.git_write': 1, 'EventsCommandAudit.git_write_completed': 0}, lambda p: p, None, 'fetch_failed', None),
            ('P5_redir_null', ALL, {'EventsCommandAudit.file_write': 0, 'EventsCommandAudit.network': 0}, lambda p: p, None, 'benign_rt', None),
            ('F1_F6_fallback_cmd', ND, {'NativeRoute.ShellLimitationPersists': True, 'NativeRoute.ModelFallback': True,
                                                   'NativeRoute.CmdA41Opens': True, 'NativeRoute.DefaultShell': 'pwsh-windowsapps'}, lambda p: p, None, 'f6_fallback', None),
            ('F2_F6_sin_fallback', ND, {'NativeRoute.ShellLimitationPersists': True, 'Operations.OP_A_shell': False}, lambda p: {}, None, 'f6_nofallback', None),
            ('F3_F6_en_stderr', ND, {'NativeRoute.ShellLimitationPersists': True, 'NativeRoute.F6Obs03Signature.in_stderr': True}, lambda p: p, 'stderr_f6', 'pwsh_rt', None),
            ('N_fetch_ok', ND, {'Operations.OP_A_shell': False}, lambda p: p, None, 'fetch_ok', None),
            ('N_escritura_ok', ND, {'Operations.OP_A_shell': False}, lambda p: p, None, 'write_ok', None),
            ('N_redir_archivo', ND, {'Operations.OP_A_shell': False}, lambda p: p, None, 'redirect_ok', None),
            ('N_red_ok', ND, {'Operations.OP_A_shell': False}, lambda p: p, None, 'net_ok', None),
            ('N_lsremote_github', ND, {'Operations.OP_A_shell': False}, lambda p: p, None, 'lsremote_github', None),
            ('N_falta_shell', ND, {'Operations.OP_A_shell': False}, lambda p: drop_key(p, 1, 'shell'), None, 'pwsh_rt', None),
            ('N1_falta_status', ND, {}, lambda p: drop_key(p, 10, 'status'), None, 'pwsh_rt', None),
            ('N2_hash', ND, {}, lambda p: mutate_value(p, 7, f'sha256:{hf0}', flip), None, 'pwsh_rt', None),
            ('N3_trx', ND, {}, lambda p: mutate_value(p, 14, 'trx:passed', lambda v: str(int(v) + 1)), None, 'pwsh_rt', None),
            ('N4_mapa', ND, {}, lambda p: mutate_value(p, 22, 'map-verdict', lambda v: 'MAP_VALID'), None, 'pwsh_rt', None),
            ('N5_no_ascii', ND, {}, lambda p: mutate_value(p, 11, 'md-line', lambda v: v.replace('→', '->')), None, 'pwsh_rt', None),
            ('N6b_origen_cambio', RU, {}, lambda p: p, 'lsremote', 'pwsh_rt', None),
            ('N7_canonico_invalido', ND, {}, setp('delegation_v2.MaxReworkLoops', 4), None, 'pwsh_rt', None),
            ('N10_tope', ND, {}, lambda p: p, 'timeout', 'pwsh_rt', None),
            ('N11_area_ignorada', ND, {}, lambda p: p, 'ignored', 'pwsh_rt', None),
            ('N14_nc2', ND, {}, lambda p: mutate_value(p, 26, 'nc2-Scope', lambda v: 'pass'), None, 'pwsh_rt', None),
            ('N16_check', ND, {}, setp('controller_verification_v2.Checks.Trailer.Result', 'pass'), None, 'pwsh_rt', None),
            ('N17_identidad', ND, {'IdentityStable': False}, lambda p: p, 'identity', 'pwsh_rt', None),
        ]
    else:
        fid_key = f'auth:{env("ARQ_FID_PATH")}:first-line'
        OK = {'Model': ARQ_CELL_MODEL, 'Effort': ARQ_CELL_EFFORT}
        CL = 'ArchitectCellClassification'
        cases = [
            ('P0_con_observacion', ALL, {CL: 'MATCH_OR_ABOVE_REQUIRED', 'ClassificationFinal': True, 'Routing.State': 'MATCH'}, lambda p: p, None, 'pwsh_rt', OK),
            ('P0b_sin_observacion', ALL, {CL: 'MATCH_OR_ABOVE_REQUIRED', 'ClassificationFinal': False, 'Routing.State': 'PENDING'}, lambda p: p, None, 'pwsh_rt', None),
            ('P0c_effort_superior', ALL, {CL: 'MATCH_OR_ABOVE_REQUIRED', 'Routing.State': 'ABOVE_REQUIRED'}, lambda p: p, None, 'pwsh_rt', {'Model': ARQ_CELL_MODEL, 'Effort': 'xhigh'}),
            ('F1_F6_como_A2P2_s2', ND, {CL: 'BELOW_REQUIRED', 'Requirements.ARCHITECTURE_REVIEW.read.State': 'BELOW_REQUIRED'},
             lambda p: {}, None, 'f6_nofallback', OK),
            ('F2_F6_fallback_cmd', ALL, {CL: 'MATCH_OR_ABOVE_REQUIRED', 'NativeRoute.ModelFallback': True, 'NativeRoute.ShellLimitationPersists': True},
             lambda p: p, None, 'f6_fallback', OK),
            ('N_raya_no_textual', ND, {CL: 'BELOW_REQUIRED'}, lambda p: mutate_value(p, 4, fid_key, lambda v: v.replace('—', '-')), None, 'pwsh_rt', OK),
            ('N_blob_objeto', ND, {CL: 'BELOW_REQUIRED'}, lambda p: mutate_value(p, 2, 'target-blob', flip), None, 'pwsh_rt', OK),
            ('N_falta_heading', ND, {CL: 'BELOW_REQUIRED'}, lambda p: drop_key(p, 4, 'auth:AGENTS.md:heading-line'), None, 'pwsh_rt', OK),
            ('N_veredicto_enum', ND, {CL: 'BELOW_REQUIRED', 'CanonicalChecks.schema_valid': False}, setp('architect_review_result_v1.Verdict', 'PASS'), None, 'pwsh_rt', OK),
            ('N_ReviewedBlob', ND, {CL: 'BELOW_REQUIRED', 'CanonicalChecks.field:ReviewedBlob': False}, setp('architect_review_result_v1.ReviewedBlob', '0' * 40), None, 'pwsh_rt', OK),
            ('N_cita_no_textual', ND, {CL: 'BELOW_REQUIRED', 'CanonicalChecks.PremiseRefs_textuales': False},
             lambda p: (p['architect_review_result_v1']['OptionalFindings'][0]['PremiseRefs'][0].__setitem__('Quote', p['architect_review_result_v1']['OptionalFindings'][0]['PremiseRefs'][0]['Quote'].replace('`', '"')), p)[1],
             None, 'pwsh_rt', OK),
            ('N_InputsRead', ND, {CL: 'BELOW_REQUIRED'},
             lambda p: (p['architect_review_result_v1'].__setitem__('InputsRead', p['architect_review_result_v1']['InputsRead'][1:]), p)[1], None, 'pwsh_rt', OK),
            ('N_independencia', ND, {CL: 'BELOW_REQUIRED'},
             lambda p: (p['architect_review_result_v1']['IndependenceEvidence'].__setitem__('Dimensions', p['architect_review_result_v1']['IndependenceEvidence']['Dimensions'][:3]), p)[1],
             None, 'pwsh_rt', OK),
            ('N_tope', ND, {CL: 'BELOW_REQUIRED'}, lambda p: p, 'timeout', 'pwsh_rt', OK),
            ('N_identidad', ND, {CL: 'MEASUREMENT_VOID'}, lambda p: p, 'identity', 'pwsh_rt', OK),
            ('N_effort_inferior', ALL, {CL: 'BELOW_REQUIRED', 'Routing.State': 'BELOW_REQUIRED'}, lambda p: p, None, 'pwsh_rt', {'Model': ARQ_CELL_MODEL, 'Effort': 'medium'}),
            ('N_modelo_distinto', ALL, {CL: 'MEASUREMENT_VOID'}, lambda p: p, None, 'pwsh_rt', {'Model': 'gpt-6-luna', 'Effort': 'high'}),
            ('N_fetch_ok', ND, {CL: 'BELOW_REQUIRED'}, lambda p: p, None, 'fetch_ok', OK),
            ('N_area_ignorada', ND, {CL: 'BELOW_REQUIRED'}, lambda p: p, 'ignored', 'pwsh_rt', OK),
        ]
    probe_schema_obj = json.loads(read_text(schema_file_for_mode(kit)))
    canon = {k: json.loads(show(d, 'HEAD', CANON_ALL[k]).decode('utf-8')) for k in MODE_KEYS[mode]}
    try:
        import jsonschema  # noqa
        have_js = True
    except Exception:
        have_js = False
    rep['JsonschemaAvailable'] = have_js
    for name, expect, extra, fn, proc, evk, eme in cases:
        cd = os.path.join(outdir, 'cases', name)
        if os.path.exists(cd):
            shutil.rmtree(cd)
        shutil.copytree(outdir, cd, ignore=shutil.ignore_patterns('cases'))
        p = fn(copy.deepcopy(perfect))
        write_json(os.path.join(cd, 'probe-last.json'), p)
        with open(os.path.join(cd, 'probe-events.jsonl'), 'w', encoding='utf-8', newline='\n') as f:
            for e in EVENTS[evk]:
                f.write(json.dumps(e) + '\n')
        if eme is not None:
            write_json(os.path.join(cd, 'effective-model-effort.json'), eme)
        if proc == 'lsremote':
            _write(os.path.join(cd, 'probe-lsremote-after.txt'), read_text(os.path.join(cd, 'probe-lsremote-after.txt')) + '0' * 40 + '\trefs/heads/otra\n')
        if proc == 'timeout':
            _write(os.path.join(cd, 'probe-exit.txt'), 'exit=143 timed_out=yes\n')
        if proc == 'ignored':
            _write(os.path.join(cd, 'probe-status-ignored-after-run.txt'), read_text(os.path.join(cd, 'probe-status-ignored-after-run.txt')) + '!! nuevo.tmp\n')
        if proc == 'identity':
            _write(os.path.join(cd, 'measurements.log'), read_text(os.path.join(cd, 'measurements.log')).replace(
                'post-probe utc=2026-10-11T00:05:00Z fp=SINTETICO structure=EQUAL bin=SINTETICO bins=1 app=26.1007.2314.0',
                'post-probe utc=2026-10-11T00:05:00Z fp=OTRA structure=EQUAL bin=OTRO bins=1 app=26.1009.0000.0'))
        if proc == 'stderr_f6':
            _write(os.path.join(cd, 'probe-stderr.txt'), 'Reading additional input from stdin...\nERROR codex_core::tools::router: error=exec_command failed: '
                   'CreateProcess { message: "Rejected(\\"Failed to create unified exec process: CreateProcessAsUserW failed: 5 (Acceso denegado.)\\")" }\n')
        verdict = compare(cd)
        cmpj = json.loads(read_text(os.path.join(cd, 'comparison.json')))
        extra_ok = {k: (_get(cmpj, k) == v) for k, v in extra.items()}
        v_probe = validate(p, probe_schema_obj)
        v_canon = {k: validate(p.get(k), canon[k]) for k in MODE_KEYS[mode]}
        js = None
        if have_js:
            import jsonschema
            js = {'probe': not list(jsonschema.Draft202012Validator(probe_schema_obj).iter_errors(p))}
            for k in MODE_KEYS[mode]:
                js[k] = not list(jsonschema.Draft202012Validator(canon[k]).iter_errors(p.get(k)))
        rep['Cases'].append({'Case': name, 'Expected': expect, 'Verdict': verdict, 'ExtraChecks': extra_ok,
                             'Pass': verdict == expect and all(extra_ok.values()),
                             'Classification': cmpj.get('ArchitectCellClassification'), 'BelowCauses': cmpj.get('BelowCauses'),
                             'NativeRoute': {k: cmpj['NativeRoute'][k] for k in ('DefaultShell', 'ModelFallback', 'ShellLimitationPersists')},
                             'FailedOperations': [k for k, v in cmpj['Operations'].items() if not v], 'Reasons': cmpj['Reasons'][:6],
                             'LocalSchemaValid': dict({'probe-schema': not v_probe}, **{k: not v for k, v in v_canon.items()}),
                             'LocalSchemaErrors': (v_probe + [e for v in v_canon.values() for e in v])[:4], 'JsonschemaValid': js})
    rep['AllCasesPass'] = all(c['Pass'] for c in rep['Cases'])
    rep['StrictAudit'] = {os.path.basename(schema_file_for_mode(kit)): strict_audit(probe_schema_obj)}
    ref = json.loads(read_text(os.path.join(outdir, 'reference.json')))
    if controller:
        rep['ReferenceHighlights'] = {
            'Checks': ref['cv']['checks'], 'Classification': ref['cv']['exact']['Classification'], 'Disposition': ref['cv']['exact']['Disposition'],
            'FailureClass': ref['cv']['exact']['FailureClass'],
            'MapVerdict': ref['steps']['22']['map-verdict']['v'], 'MvFailed': ref['steps']['22']['mv-failed']['v'],
            'Mv4Mismatches': ref['steps']['22']['mv4-mismatches']['v'], 'EFF': ref['steps']['19']['EFF']['v'],
            'ClassifyP': ref['steps']['20']['classify:P']['v'], 'StageHead': ref['meta']['stage_head'],
            'ScopeDiffCount': len(ref['cv']['scope_diff']), 'KeysRequested': sum(len(v) for v in ref['steps'].values())}
    else:
        rep['ReferenceHighlights'] = {'KeysRequested': sum(len(v) for v in ref['steps'].values()), 'RequiredInputs': ref['arq']['RequiredInputs'],
                                      'TargetBlob': ref['arq']['exact']['ReviewedBlob'], 'Head': ref['meta']['head'],
                                      'Authorities': ref['arq']['Authorities']}
    rep['UtcEnd'] = datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ')
    write_json(report, rep)
    ok = rep['AllCasesPass'] and rep['StatusIgnoredRestored'] and (not controller or rep['Steps'].get('unstage') == 0)
    print('selftest', mode, 'PASS' if ok else 'FAIL')
    sys.exit(0 if ok else 1)


if __name__ == '__main__':
    a = sys.argv[1:]
    cmd = a[0] if a else ''
    if cmd == 'build-schema':
        build_schema(a[1])
    elif cmd == 'check-schema':
        check_schema(a[1])
    elif cmd == 'strict-audit':
        print(json.dumps(strict_audit(json.loads(read_text(a[1]))), ensure_ascii=False, indent=1))
    elif cmd == 'validate':
        s = json.loads(read_text(a[1]))
        inst = json.loads(read_text(a[2]))
        if len(a) > 3:
            s = s['properties'][a[3]]
            inst = inst[a[3]]
        errs = validate(inst, s)
        print('VALID' if not errs else 'INVALID', errs[:10])
        sys.exit(0 if not errs else 1)
    elif cmd == 'build-prompt':
        build_prompt(a[1], a[2])
    elif cmd == 'stage':
        stage(a[1])
    elif cmd == 'unstage':
        unstage(a[1])
    elif cmd == 'reference':
        reference(a[1])
    elif cmd == 'compare':
        v = compare(a[1])
        sys.exit(0 if v == 'ALL_OPERATIONS_DEMONSTRATED' else 1)
    elif cmd == 'audit-events':
        sys.stdout.reconfigure(encoding='utf-8')
        ev = audit_events(a[1], a[2] if len(a) > 2 else None)
        print(json.dumps(dict({k: v for k, v in ev.items() if k != 'list'}, NativeRoute=native_route(ev)), ensure_ascii=False, indent=1))
    elif cmd == 'synth':
        synth(a[1], a[2])
    elif cmd == 'selftest':
        selftest(a[1], a[2], a[3])
    else:
        sys.exit('uso: probe_tools.py build-schema|check-schema <kitdir> | strict-audit <schema> | validate <schema> <inst> [prop] | '
                 'build-prompt <kitdir> <out> | stage|unstage|reference|compare <outdir> | audit-events <events.jsonl> [<stderr>] | '
                 'synth <outdir> <out> | selftest <kitdir> <outdir> <report>   (PROBE_MODE = NATIVE_CONTROLLER | NATIVE_ARCHITECT | CMD_A41)')

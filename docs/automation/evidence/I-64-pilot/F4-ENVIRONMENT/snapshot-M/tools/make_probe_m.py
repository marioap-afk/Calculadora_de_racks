"""Sonda neutra de re-acreditacion de la celda del Controller con el binario nuevo. Mismo prompt, esquema y oraculo que las sondas 515d y de F2-T1."""
import json, os, sys, hashlib
R = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'a14r')  # carpeta de la sesion, fuera del repositorio: nada se escribe en el worktree de A-14
os.chdir(R)
RID = sys.argv[1]
D = f'F4-ENVIRONMENT/probe-{RID}'
os.makedirs(D, exist_ok=False)
SHA = '^[0-9a-f]{40}$'
schema = {'$schema': 'https://json-schema.org/draft/2020-12/schema', 'title': 'rackcad-i64-cell-probe/v1', 'type': 'object',
          'additionalProperties': False,
          'properties': {'Schema': {'type': 'string', 'enum': ['rackcad-i64-cell-probe/v1']},
                         'RunId': {'type': 'string', 'enum': [RID]},
                         'HeadSha': {'type': 'string', 'pattern': SHA},
                         'OriginMainSha': {'type': 'string', 'pattern': SHA},
                         'StateBlob': {'type': 'string', 'pattern': SHA},
                         'CommandsRun': {'type': 'array', 'items': {'type': 'string'}}},
          'required': ['Schema', 'RunId', 'HeadSha', 'OriginMainSha', 'StateBlob', 'CommandsRun']}
open(f'{D}/probe.schema.json', 'w', encoding='ascii', newline='\n').write(json.dumps(schema, indent=2) + '\n')
P = [
    'I-64 - SONDA MINIMA DE CELDA (solo lectura). RunId ' + RID + '.',
    'Objetivo: medir la elegibilidad de esta celda (Codex CLI, modelo gpt-6-luna, effort high, solo lectura) para la verificacion. No es una tarea de producto.',
    'Ejecuta en el directorio de trabajo actual exactamente estas tres ordenes y devuelve sus salidas, sin interpretarlas:',
    '  1. git rev-parse HEAD',
    '  2. git rev-parse origin/main',
    '  3. git rev-parse HEAD:docs/automation/state/I-64.yml',
    'Campos: HeadSha = salida de 1; OriginMainSha = salida de 2; StateBlob = salida de 3; CommandsRun = las ordenes ejecutadas.',
    'Reglas: solo lectura; no escribas archivos, no hagas commit ni push, no ejecutes otras ordenes. Salida solo en ASCII.',
    'Informe esperado: la salida exacta del esquema rackcad-i64-cell-probe/v1 (la escribe el CLI por -o).',
]
b = ('\n'.join(P) + '\n').encode('ascii')
open(f'{D}/prompt.md', 'wb').write(b)
print(D, hashlib.sha256(b).hexdigest().upper())

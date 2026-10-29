"""Preflight final del kit v2 de la medición A6-7 (I-62; decisiones §101, punto 7; derivado de s100/preflight_static.py: disposición §101 y líneas del Owner en §102): ejecuta, sin lanzar nada, las puertas del kit (gates_m7.py,
las mismas funciones que usa `bash run_m7.sh launch`) con su salida en una carpeta FUERA del kit (no crea out/ ni el marcador P-07), y añade las
comprobaciones de autoridad, de piezas fijadas, del cierre, de la cobertura, del autotest y del host. Cada condición queda como
  OK          comprobable ahora y conforme;
  FAIL        comprobable ahora y no conforme (bloquea la preparación);
  PENDIENTE   depende de una autorización todavía inexistente o de un paso posterior a ella (disposición, líneas del Owner, escritura de
              preparación, cierre de la app de Codex): la puerta la rechaza hoy por ese motivo, que se nombra;
  SESION      solo se puede verificar en la sesión real (README del kit, §10); nunca se da por cumplida aquí.
Nunca ejecuta Codex ni el pwsh del runtime; no lee valores de config.toml (solo su SHA-256 y los nombres de clave, como las puertas) ni auth.json.
Uso: python -I -B preflight_static.py <kit> <salida.json>
"""
import hashlib, importlib.util, json, os, subprocess, sys

KIT, OUTP = sys.argv[1], os.path.abspath(sys.argv[2])
sys.path.insert(0, KIT)


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(KIT, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


G = _load('gates_m7')
L = G.L
cfg = G.Config()
cfg.out = os.path.join(os.path.dirname(OUTP), 'preflight-out').replace('\\', '/')
os.makedirs(os.path.join(cfg.out, 'raw'), exist_ok=True)
W = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
MAIN = r'D:\Documentos\Codex\Calculadora de racks'
I64 = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-workspace-persistente-rackcad')
ENV = dict(os.environ, GIT_OPTIONAL_LOCKS='0')
rows = []


def row(group, name, status, detail=None, reason=None):
    rows.append({'Group': group, 'Condition': name, 'Status': status, 'Reason': reason, 'Detail': detail})


def g(repo, *a):
    return subprocess.run(['git', '-C', repo] + list(a), capture_output=True, text=True, encoding='utf-8', errors='replace', env=ENV).stdout.strip()


def gate(name, fn, pending_if=None):
    try:
        r = fn()
        r = r[0] if isinstance(r, tuple) else r
    except Exception as e:
        row('Puertas del kit', name, 'FAIL', None, 'excepción %s: %s' % (type(e).__name__, str(e)[:300]))
        return None
    probs = r.get('Problems') or []
    if r.get('Ok'):
        row('Puertas del kit', name, 'OK', {k: r[k] for k in list(r)[:6] if k not in ('Problems', 'Ok')} if isinstance(r, dict) else None)
    elif pending_if and all(pending_if(p) for p in probs):
        row('Puertas del kit', name, 'PENDIENTE', probs[:6], 'rechazo previsto hasta la autorización o el paso que la sigue')
    else:
        row('Puertas del kit', name, 'FAIL', probs[:6])
    return r


# ---- autoridad y piezas fijadas (W, solo lectura)
head = g(W, 'rev-parse', 'HEAD')
for path, blob, what in (('docs/initiatives/I-62-A-6.md', '4bc919dd4768ab81abc74b70d3a4a3821c07feb5', 'A-6 AGREED (§90)'),
                         ('docs/initiatives/I-62-A-7.md', 'ebb3a87b0b33ddf1d6418f4e9f4fb4195d1555d3', 'A-7 AGREED (§100)'),
                         ('docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py', L.B, 'instrumento B-min'),
                         ('docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py', L.ACC, 'aceptante B-min')):
    got = g(W, 'rev-parse', 'HEAD:' + path)
    row('Autoridad y piezas', '%s en la HEAD de W' % what, 'OK' if got == blob else 'FAIL', {'Head': head, 'Path': path, 'Blob': got})
dec = g(W, 'show', 'HEAD:docs/automation/decisions/I-62.md').split('\n')
nums = [int(l[3:].split('.')[0]) for l in dec if l.startswith('## ') and l[3:].split('.')[0].isdigit()]
row('Autoridad y piezas', 'HEAD de W: disposición §101 y líneas del Owner en §102, sin secciones posteriores', 'OK' if nums and max(nums) == 102 and 101 in nums else 'FAIL', {'Last': max(nums) if nums else None})
for blob, sha, what in ((L.B, L.B_SHA256, 'instrumento'), (L.ACC, L.ACC_SHA256, 'aceptante')):
    b = subprocess.run(['git', '-C', W, 'cat-file', 'blob', blob], capture_output=True, env=ENV).stdout
    row('Autoridad y piezas', 'SHA-256 del %s (objeto Git)' % what, 'OK' if hashlib.sha256(b).hexdigest() == sha else 'FAIL', {'Blob': blob})

# ---- puertas del kit (las de run_m7.sh launch, en orden)
dec_pending = lambda p: ('no single Coordinator disposition' in p or 'M7-LECTURAS' in p or 'not a complete line' in p or 'after the disposition' in p)
gate('Kit (cada archivo = FREEZE.json, ninguno nuevo)', lambda: G.check_kit(cfg))
gate('Decisiones (disposición única tras §100 con los marcadores; M7-LECTURAS; líneas del Owner)', lambda: G.check_decisions(cfg))
ident = G.measure(cfg, 'preflight', cfg.out)
row('Puertas del kit', 'Identidad antes (huella, nombres de claves, un único codex.exe, binario, AppVersion, acompañantes, pwsh del runtime)',
    'OK' if ident['EqualsTrio'] else ('PENDIENTE' if set(ident['Differs']) <= {'SingleCodexExe', 'CodexExeCount'} else 'FAIL'),
    {'Differs': ident['Differs']})
gate('P-07 (ningún lanzamiento previo; marcador ausente)', lambda: G.check_p07(cfg))
gate('Estado del -C (instantánea congelada + exactamente el objeto del instrumento; registro de P-W)', lambda: G.check_state(cfg),
     lambda p: ('object' in p.lower() or 'prep' in p.lower() or 'P-W' in p))
gate('Pwsh del runtime (presente, SHA-256)', lambda: G.check_runtime_pwsh(cfg))
gate('Git estático (sin git.* en los directorios vigilados)', lambda: G.check_static_git(cfg))
cap = subprocess.run(['bash', 'D:/r62-a6m7/s100/capture_launch_env.sh', KIT.replace('\\', '/'),
                      os.path.join(cfg.out, 'raw', 'launch-env.json').replace('\\', '/')], capture_output=True, text=True)
gate('Entorno del hijo (sin GIT_*; pwsh del runtime primero; capturado como run_m7.sh, capture_launch_env.sh)',
     lambda: G.check_launch_env(cfg, os.path.join(cfg.out, 'raw', 'launch-env.json')))
gate('P-02 procesos (ningún codex.exe vivo; ninguno con el -C)', lambda: G.check_processes(cfg), lambda p: 'codex.exe' in p.lower())
gate('Proyecto de Claude del -C (ninguno)', lambda: G.check_claude_project(cfg))
gate('Texto armado (SHA-256 = FREEZE)', lambda: G.check_prompt(cfg))

# ---- cierre, cobertura, regla, autotest
fr = L.rjson(KIT + '/FREEZE.json')
der = L.rjson(KIT + '/frozen/derivation.json')
row('Cierre y kit', 'cierre A7-1 aceptado por el validador (check_a7_closure.py)', 'OK' if der.get('Accepted') is True else 'FAIL',
    {'Omitted': [(o['Term'], o['Cause']) for o in der['Omitted']], 'BuilderAbsences': [a.get('Role') for a in der.get('BuilderAbsences') or []]})
cov = subprocess.run([sys.executable, '-I', '-B', KIT + '/check_citation_blobs.py', KIT + '/frozen/designation-M.json', '--e=' + KIT + '/frozen/E.json',
                      '--mode=coverage'], capture_output=True, text=True)
covd = json.loads(cov.stdout) if cov.stdout.strip().startswith('{') else {}
row('Cierre y kit', 'cobertura de A-7 §3.5 (clausemap en EFF^1, EFF y MainSha_eval = K.MainSha) — mitigación de O2 en la medición',
    'OK' if cov.returncode == 0 and covd.get('Result') == 'LAUNCH_MAY_PROCEED' else 'FAIL', {'Rc': cov.returncode, 'Result': covd.get('Result'),
                                                                                          'Blocking': covd.get('Blocking')})
st = L.rjson(KIT + '/selftest-report.json')
row('Cierre y kit', 'autotest del kit v2', 'OK' if st['Holding'] == st['Total'] else 'FAIL', {'Holding': st['Holding'], 'Total': st['Total'], 'ByGroup': st['ByGroup']})
txt = L.rbytes(KIT + '/frozen/prompt.m7.armed.sanitized.txt').decode('utf-8')
row('Cierre y kit', 'texto armado sin la orden contract; 13 ejecuciones del instrumento', 'OK' if ' -I - contract' not in txt and txt.count(L.B) == 13 else 'FAIL',
    {'BlobCount': txt.count(L.B)})
rr = _load('reconstruct_runs_m7')
row('Cierre y kit', 'regla de reconstrucción v3: contract fuera del conjunto declarado', 'OK' if 'contract' not in rr.TOOL_COMMANDS and rr.RULE.endswith('/3') else 'FAIL',
    {'Rule': rr.RULE, 'Orders': list(rr.TOOL_COMMANDS)})
v1 = 'D:/r62-a6m7/kit/FREEZE.json'
row('Cierre y kit', 'kit v1 sin cambios (FREEZE 459a3440) y su -C intacto', 'OK' if hashlib.sha256(open(v1, 'rb').read()).hexdigest().startswith('459a3440') else 'FAIL')

# ---- host
row('Host', 'checkout principal sin cambios', 'OK' if g(MAIN, 'status', '--porcelain') == '' else 'FAIL', {'Branch': g(MAIN, 'rev-parse', '--abbrev-ref', 'HEAD'),
                                                                                                       'Head': g(MAIN, 'rev-parse', 'HEAD')})
row('Host', 'worktree de I-64 sin cambios', 'OK' if g(I64, 'status', '--porcelain') == '' else 'FAIL', {'Head': g(I64, 'rev-parse', '--short', 'HEAD')})

# ---- pasos posteriores a la autorización y condiciones de la sesión real (README del kit, §1 y §10)
for name in ('python -I prep_write.py: escritura del objeto del instrumento en el -C (A6-3, regla 1), solo tras la disposición',
             'comprobación del punto de uso (pou_m7.py, (i)-(viii) de A6-3, regla 3) con el pwsh del runtime, después de prep_write (en run_m7.sh launch)'):
    row('Pendiente de autorización o de un paso posterior', name, 'PENDIENTE')
for name in ('identidad real antes y después (huella, binario, versión, acompañantes) y estado de autenticación',
             'forma exacta con que 26.1023 registra las órdenes; shell real (pwsh del runtime, -NoProfile o cmd) y su modo de lenguaje',
             'sobres sin alterar, separables de stderr y sin truncar (clausemap ≈ 44 KB)',
             'el alias de Python arranca dentro del sandbox; git por nombre en el entorno real = Git declarado',
             'tiempo dentro del tope de 600 s; el modelo copia literalmente las 18 ejecuciones y rellena el esquema completo',
             'modelo y effort efectivos (turn_context) = gpt-6-luna / high; escritura de -o'):
    row('Solo en la sesión real', name, 'SESION')
summary = {}
for r in rows:
    summary[r['Status']] = summary.get(r['Status'], 0) + 1
doc = {'Schema': 'i62-a6m7-v2-static-preflight/1', 'UtcTime': L.utc(), 'Kit': KIT.replace('\\', '/'), 'FreezeSha256': L.sha256_file(KIT + '/FREEZE.json'),
       'WorktreeHead': head, 'Summary': summary, 'NoFail': summary.get('FAIL', 0) == 0, 'Rows': rows}
text = L.sanitize_str(json.dumps(doc, ensure_ascii=False, indent=1, default=str)) if hasattr(L, 'sanitize_str') else json.dumps(doc, ensure_ascii=False, indent=1)
with open(OUTP, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text + '\n')
for r in rows:
    print('%-9s %s — %s' % (r['Status'], r['Group'], r['Condition'][:110]))
print(json.dumps(summary))
sys.exit(0 if doc['NoFail'] else 1)

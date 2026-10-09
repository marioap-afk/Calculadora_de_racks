#!/usr/bin/env python3
# Herramientas de la sonda medida de A4-1 (BORRADOR; NO EJECUTADO). Sin red, sin modelo y sin leer ~/.codex/config.toml.
# Actualizado a la corrección 2 de A-4 (blob 0d954376): la regla 2 exige además las lecturas de Git de Identity, Remote, CleanTree (estado e
# ignorados) y Trailer (historia) (A62-A4-O1), que son los pasos 9-12 del texto.
#   build-prompt <kitdir> <outfile>  arma el texto de la sonda desde la plantilla, la declaración de shell y gates.env (variables de entorno)
#   reference <outdir>               DESPUÉS de la sonda: valores de referencia calculados con git y hashlib en PROBE_DIR (solo lectura)
#   compare <outdir>                 compara probe-last.json con reference.json, audita los comandos de los eventos y emite comparison.json
# La referencia se calcula después de la sonda para que ningún valor esperado exista en disco mientras corre. Para Remote, la referencia solo vale
# si el `git ls-remote origin` que registra run_probe.sh antes y después de la sonda es igual al de la comparación (si no: REFERENCE_UNSTABLE).
# compare no decide nada: propone a la supervisión si cada operación de A4-1, regla 2, quedó demostrada; la elegibilidad la decide el Coordinator.
import hashlib, json, os, re, subprocess, sys


def env(k):
    v = os.environ.get(k)
    if v is None or v == '':
        sys.exit(f'falta la variable {k} (gates.env)')
    return v


def read(p):
    with open(p, encoding='utf-8') as f:
        return f.read()


def write_json(p, obj):
    with open(p, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(obj, f, ensure_ascii=False, indent=1)
        f.write('\n')


def pairs():
    return [p.split('|', 1) for p in env('CLAUSE_PAIRS').split(';') if p.strip()]


# ------------------------------------------------------------------ build-prompt
def build_prompt(kit, out):
    mode = env('CLAUSE_MAP_MODE')
    step8 = read(os.path.join(kit, f'clause-map-step.{mode}.txt')).strip()
    step8 = (step8.replace('{{CLAUSE_MAP_PATH}}', env('CLAUSE_MAP_PATH'))
                  .replace('{{CLAUSE_PAIRS}}', '; '.join(f'`{p}|{s}`' for p, s in pairs()) if mode == 'RESOLVE' else '')
                  .replace('{{DECLARATION_FILE}}', env('DECLARATION_FILE'))
                  .replace('{{DECLARATION_TEXT}}', env('DECLARATION_TEXT')))
    subs = {
        '{{SHELL_DECLARATION}}': read(os.path.join(kit, 'shell-declaration.txt')).strip(),
        '{{BASE_SHA}}': env('BASE_SHA'),
        '{{HEAD_SHA}}': env('HEAD_SHA'),
        '{{HASH_FILES}}': ', '.join(f'`{f}`' for f in env('HASH_FILES').split()),
        '{{JSON_INPUT}}': env('JSON_INPUT'),
        '{{JSON_FIELDS}}': ', '.join(f'"{x}"' for x in env('JSON_FIELDS').split()),
        '{{JSON_ARRAYS}}': ', '.join(f'"{x}"' for x in env('JSON_ARRAYS').split()),
        '{{CLAUSE_MAP_STEP}}': step8,
        '{{BRANCH}}': env('BRANCH'),
        '{{IGNORE_PATH}}': env('IGNORE_PATH'),
    }
    text = read(os.path.join(kit, 'probe-prompt.template.txt'))
    for k, v in subs.items():
        text = text.replace(k, v)
    if '{{' in text or '<' in text.replace('<archivo>', '').replace('<campo>', '').replace('<array>', '').replace('<comando>', '') \
            .replace('<Path>', '').replace('<Section>', '').replace('<Kind>', '').replace('<Level>', '').replace('<commit>', ''):
        sys.exit('el texto de la sonda conserva un marcador sin rellenar')
    with open(out, 'w', encoding='utf-8', newline='\n') as f:
        f.write(text)
    print('prompt ok', hashlib.sha256(text.encode('utf-8')).hexdigest())


# ------------------------------------------------------------------ reference
def git(d, *a):
    return subprocess.run(['git', '-C', d, *a], capture_output=True, text=True, encoding='utf-8', check=True).stdout


def git_rc(d, *a):
    r = subprocess.run(['git', '-C', d, *a], capture_output=True, text=True, encoding='utf-8')
    return r.returncode, r.stdout


def yes_no(rc):
    return 'yes' if rc == 0 else ('no' if rc == 1 else f'error:{rc}')


def reference(outdir):
    d = env('PROBE_DIR')
    files = env('HASH_FILES').split()
    ref = {'1': {'ComSpec': os.environ.get('ComSpec', '')}}
    ref['2'] = {'HEAD': git(d, 'rev-parse', 'HEAD').strip()}
    paths = [x for x in git(d, 'diff', '--name-only', f"{env('BASE_SHA')}..{env('HEAD_SHA')}").splitlines() if x]
    ref['3'] = {'path': paths}
    ref['4'] = {'path': paths, 'identical': 'yes'}
    ref['5'], ref['6'] = {}, {}
    for f in files:
        ref['5'][f'hash-object:{f}'] = git(d, 'hash-object', f).strip()
        ref['5'][f'rev-parse:{f}'] = git(d, 'rev-parse', f'HEAD:{f}').strip()
        with open(os.path.join(d, f), 'rb') as fh:
            ref['6'][f'sha256:{f}'] = hashlib.sha256(fh.read()).hexdigest()
    j = json.loads(read(os.path.join(d, env('JSON_INPUT'))))
    ref['7'] = {}
    for k in env('JSON_FIELDS').split():
        ref['7'][f'field:{k}'] = json.dumps(j.get(k), ensure_ascii=False)
    for k in env('JSON_ARRAYS').split():
        ref['7'][f'length:{k}'] = str(len(j.get(k) or []))
    cm = env('CLAUSE_MAP_PATH')
    if env('CLAUSE_MAP_MODE') == 'RESOLVE':
        m = json.loads(read(os.path.join(d, cm)))
        r8 = {'rev-parse': git(d, 'rev-parse', f'HEAD:{cm}').strip(), 'hash-object': git(d, 'hash-object', cm).strip(),
              'Schema': str(m.get('Schema')), 'Protocol': str(m.get('Protocol'))}
        for p, s in pairs():
            hit = [e for e in m.get('Entries', []) if e.get('Path') == p and e.get('Section') == s]
            r8[f'entry:{p}|{s}'] = f"{hit[0].get('Kind')}|{hit[0].get('Level')}" if hit else 'NO_ENTRY'
    else:
        text = read(os.path.join(d, env('DECLARATION_FILE')))
        lines = [i + 1 for i, ln in enumerate(text.splitlines()) if env('DECLARATION_TEXT') in ln]
        r8 = {'contains': 'yes' if env('DECLARATION_TEXT') in text else 'no', 'line': str(lines[0]) if lines else '',
              'ls-tree': git(d, 'ls-tree', 'HEAD', '--', cm).strip()}
    ref['8'] = r8
    # A4-1, regla 2 (A-4 0d954376; A62-A4-O1): lecturas de Git de Identity, Remote, CleanTree y Trailer
    br, base, head = env('BRANCH'), env('BASE_SHA'), env('HEAD_SHA')
    ref['9'] = {'branch': git(d, 'rev-parse', '--abbrev-ref', 'HEAD').strip(),
                'toplevel': git(d, 'rev-parse', '--show-toplevel').strip(),
                'origin-branch': git(d, 'rev-parse', f'refs/remotes/origin/{br}').strip(),
                'is-ancestor': yes_no(git_rc(d, 'merge-base', '--is-ancestor', base, head)[0])}

    def lsr(r):
        out = git(d, 'ls-remote', 'origin', r).split()
        return out[0] if out else ''
    ref['10'] = {'ls-remote:branch': lsr(f'refs/heads/{br}'), 'ls-remote:main': lsr('refs/heads/main'),
                 'origin-main': git(d, 'rev-parse', 'refs/remotes/origin/main').strip()}
    rc_ci, out_ci = git_rc(d, 'check-ignore', '-v', '--no-index', env('IGNORE_PATH'))
    ref['11'] = {'status': git(d, 'status', '--porcelain'), 'status-ignored': git(d, 'status', '--porcelain', '--ignored'),
                 'check-ignore': out_ci, 'check-ignore-matched': yes_no(rc_ci)}
    commits = [x for x in git(d, 'rev-list', '--reverse', f'{base}..{head}').splitlines() if x]
    r12 = {'commit': commits}
    for c in commits:
        lines = [ln.strip() for ln in git(d, 'log', '-1', '--format=%(trailers:key=Co-Authored-By,valueonly)', c).splitlines() if ln.strip()]
        r12[f'trailer:{c}'] = ' | '.join(lines)
    ref['12'] = r12
    # estabilidad de la referencia de Remote: ls-remote antes y después de la sonda (run_probe.sh) = ls-remote de ahora
    snaps = {}
    for tag in ('before', 'after'):
        p = os.path.join(outdir, f'probe-lsremote-{tag}.txt')
        snaps[tag] = read(p) if os.path.exists(p) else None
    live = git(d, 'ls-remote', 'origin')
    ref['meta'] = {'lsremote_stable': snaps['before'] is not None and snaps['before'] == snaps['after'] == live}
    write_json(os.path.join(outdir, 'reference.json'), ref)
    print('reference ok')


# ------------------------------------------------------------------ compare
def norm_hex(v):
    return re.sub(r'\s+', '', v or '').lower()


def same_json(a, b):
    try:
        return json.loads(a) == json.loads(b)
    except Exception:
        return (a or '').strip() == (b or '').strip()


def audit_events(path):
    stats = {'cmd': 0, 'powershell_or_pwsh': 0, 'other': 0, 'interpreters': 0, 'commands': []}
    def walk(o):
        if isinstance(o, dict):
            c = o.get('command')
            if isinstance(c, (str, list)):
                s = c if isinstance(c, str) else ' '.join(map(str, c))
                low = s.lower()
                kind = 'powershell_or_pwsh' if ('pwsh' in low or 'powershell' in low) else ('cmd' if 'cmd.exe' in low or low.startswith('cmd ') else 'other')
                stats[kind] += 1
                if re.search(r'\b(py|python|python3|node)(\.exe)?\b', low):
                    stats['interpreters'] += 1
                stats['commands'].append({'kind': kind, 'exit_code': o.get('exit_code'), 'status': o.get('status'), 'chars': len(s)})
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
    return stats


def compare(outdir):
    ref = json.loads(read(os.path.join(outdir, 'reference.json')))
    reasons = []
    try:
        probe = json.loads(read(os.path.join(outdir, 'probe-last.json')))
    except Exception as e:
        probe = {'steps': []}
        reasons.append(f'sin salida estructurada válida: {e.__class__.__name__}')
    steps = {s.get('id'): s for s in probe.get('steps', [])}

    def values(i, key):
        return [p.get('value', '') for p in steps.get(i, {}).get('values', []) if p.get('key') == key]

    def first(i, key):
        v = values(i, key)
        return v[0] if v else None

    def has(i, key):
        return bool(values(i, key))

    def norm_block(v):
        lines = (v or '').replace('\r\n', '\n').replace('\r', '\n').strip('\n').split('\n')
        return '\n'.join(ln.rstrip().replace('\t', ' ') for ln in lines).strip()

    def norm_path(v):
        return (v or '').strip().replace('\\', '/').rstrip('/').lower()

    def norm_trailers(v):
        return ' | '.join(x.strip() for x in (v or '').split('|') if x.strip())

    res = {}
    p3 = [v.strip() for v in values(3, 'path')]
    p4 = [v.strip() for v in values(4, 'path')]
    res['OP1_git_diff_reproducible'] = (p3 == ref['3']['path'] and p4 == ref['4']['path'] and (first(4, 'identical') or '').strip() == 'yes')
    res['OP2_blobs_y_hashes'] = all(norm_hex(first(5, k)) == norm_hex(v) for k, v in ref['5'].items()) and \
                               all(norm_hex(first(6, k)) == norm_hex(v) for k, v in ref['6'].items())
    res['OP3_lectura_json'] = all(same_json(first(7, k), v) if k.startswith('field:') else (first(7, k) or '').strip() == v
                                  for k, v in ref['7'].items())
    def eq8(k, v):
        got = first(8, k)
        if not has(8, k):  # una clave ausente nunca casa con una referencia vacía
            return False
        return norm_hex(got) == norm_hex(v) if k in ('rev-parse', 'hash-object') else (got or '').strip() == v.strip()
    res['OP4_mapa_de_clausulas'] = all(eq8(k, v) for k, v in ref['8'].items())
    # A4-1, regla 2 (A-4 0d954376): lecturas de Git de Identity, Remote, CleanTree y Trailer; toda clave pedida tiene que estar presente
    r9, r10, r11, r12 = ref['9'], ref['10'], ref['11'], ref['12']
    res['OP5_identity'] = (all(has(9, k) for k in r9) and (first(9, 'branch') or '').strip() == r9['branch']
                           and norm_path(first(9, 'toplevel')) == norm_path(r9['toplevel'])
                           and norm_hex(first(9, 'origin-branch')) == norm_hex(r9['origin-branch'])
                           and (first(9, 'is-ancestor') or '').strip() == r9['is-ancestor'])
    res['OP6_remote'] = all(has(10, k) and norm_hex(first(10, k)) == norm_hex(v) for k, v in r10.items())
    res['OP7_cleantree_estado_e_ignorados'] = (all(has(11, k) for k in r11)
                                               and all(norm_block(first(11, k)) == norm_block(r11[k]) for k in ('status', 'status-ignored', 'check-ignore'))
                                               and (first(11, 'check-ignore-matched') or '').strip() == r11['check-ignore-matched'])
    res['OP8_trailer_historia'] = ([v.strip() for v in values(12, 'commit')] == r12['commit']
                                   and all(has(12, k) and norm_trailers(first(12, k)) == norm_trailers(v)
                                           for k, v in r12.items() if k.startswith('trailer:')))
    comspec = (first(1, 'ComSpec') or '').lower()
    res['shell_cmd_observada'] = 'cmd.exe' in comspec
    ref_stable = bool(ref.get('meta', {}).get('lsremote_stable'))

    exitinfo = read(os.path.join(outdir, 'probe-exit.txt')).strip() if os.path.exists(os.path.join(outdir, 'probe-exit.txt')) else 'missing'
    log = read(os.path.join(outdir, 'measurements.log')) if os.path.exists(os.path.join(outdir, 'measurements.log')) else ''
    pre = re.search(r'^pre-probe .*$', log, re.M)
    post = re.search(r'^post-probe .*$', log, re.M)
    strip_utc = lambda m: re.sub(r'utc=\S+ ', '', m.group(0).split(' ', 1)[1]) if m else None
    identity_stable = bool(pre and post and strip_utc(pre) == strip_utc(post)) and 'STOP' not in log
    def same_file(a, b):
        pa, pb = os.path.join(outdir, a), os.path.join(outdir, b)
        return os.path.exists(pa) and os.path.exists(pb) and read(pa) == read(pb)
    tree_unchanged = same_file('probe-head-before.txt', 'probe-head-after.txt') and same_file('probe-status-before.txt', 'probe-status-after.txt')
    events = audit_events(os.path.join(outdir, 'probe-events.jsonl'))

    if not exitinfo.startswith('exit=0 '):
        reasons.append(f'salida del proceso: {exitinfo}')
    if not identity_stable:
        reasons.append('identidad no estable antes/después (A4-1, regla 5: la medición queda anulada)')
    if not tree_unchanged:
        reasons.append('HEAD o árbol del clon cambiaron')
    for k, ok in res.items():
        if not ok:
            reasons.append(f'{k}: no demostrado')
    if not ref_stable:
        reasons.append('referencia de Remote no establecida: el ls-remote del origen cambió entre antes de la sonda, después y la comparación')
    if not reasons:
        verdict = 'ALL_OPERATIONS_DEMONSTRATED'
    elif not ref_stable and all(r.startswith(('OP6_remote', 'referencia de Remote')) for r in reasons):
        verdict = 'REFERENCE_UNSTABLE'  # defecto de la referencia, no de la celda: lo dispone el Coordinator; sin reintento automático
    else:
        verdict = 'NOT_DEMONSTRATED'
    out = {
        'Schema': 'a4-1-probe-comparison (borrador; propuesta de la supervisión, no decisión; A-4 0d954376)',
        'Verdict': verdict,
        'Operations': res,
        'RemoteReferenceStable': ref_stable,
        'Process': exitinfo,
        'IdentityStable': identity_stable,
        'TreeUnchanged': tree_unchanged,
        'EventsCommandAudit': {k: v for k, v in events.items() if k != 'commands'},
        'EventsCommands': events['commands'],
        'Reasons': reasons,
        'Note': ('Con NOT_DEMONSTRATED la celda no es elegible y la invocación no se repite bajo A4-1 (regla 4). Con ALL_OPERATIONS_DEMONSTRATED, '
                 'la elegibilidad para la planificación y la verificación la declara el Coordinator; la línea del Owner no acepta el resultado. '
                 'REFERENCE_UNSTABLE: la referencia de Remote no se pudo fijar porque el origen cambió durante la medición; lo dispone el Coordinator, '
                 'sin reintento automático (el tope es uno en total para FX-02 en F6). '
                 'Los intentos con PowerShell o con intérpretes quedan registrados en EventsCommandAudit para la disposición.'),
    }
    write_json(os.path.join(outdir, 'comparison.json'), out)
    print(verdict, '; '.join(reasons))


if __name__ == '__main__':
    cmd = sys.argv[1] if len(sys.argv) > 1 else ''
    if cmd == 'build-prompt':
        build_prompt(sys.argv[2], sys.argv[3])
    elif cmd == 'reference':
        reference(sys.argv[2])
    elif cmd == 'compare':
        compare(sys.argv[2])
    else:
        sys.exit('uso: probe_tools.py build-prompt <kitdir> <outfile> | reference <outdir> | compare <outdir>')

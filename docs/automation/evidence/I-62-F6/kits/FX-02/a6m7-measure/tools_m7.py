# tools_m7.py - oracle, prompt, output schema, comparator, post-launch pipeline and custody of the A6-7 measurement (I-62).
# Stdlib only. Never executes Codex or any model CLI.
#   python -I tools_m7.py post [--fixture <dir>]     after the single launch: identity after, Entry, events, session record,
#                                                    reconstruction, request, acceptor, table, compare, custody
#   python -I tools_m7.py compare [--fixture <dir>]  re-runs the comparison on the artifacts of out/ (no acceptor run)
# The oracle is computed by the supervision from Git primitives and the custodied bytes only (never with the instrument, the
# acceptor or the SPEC), sealed by SHA-256 in FREEZE.json before the launch, and never given to the Controller (it lives in
# the kit, outside the -C). The comparison is a proposal for the Coordinator, never a declaration of ELIGIBLE.
import copy, glob, hashlib, importlib.util, json, os, re, subprocess, sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


L = _load('lib_kit')
P = L.params()
ORACLE = L.FROZEN + '/oracle.json'
TEMPLATE = L.KIT + '/prompt.m7.template.txt'
SCHEMA_FULL = L.KIT + '/schema.m7.json'
SCHEMA_STRICT = L.KIT + '/schema.m7.strict-projection.json'
TRAILER_LINE = 'Agent-Protocol-Normative: I-62'
H1610 = '### 16.10 Términos de gate'
FREE_TEXT_FIELDS = ('WorkCompleted', 'Evidence', 'UnexpectedFindings', 'KnownLimitations', 'Deviations', 'OpenQuestions',
                    'RecommendedNextAction')
VERDICT_OK = 'ALL_OPERATIONS_DEMONSTRATED'
VERDICT_NO = 'NOT_DEMONSTRATED'


# ------------------------------------------------------------------ declared executables and the orders of M
def declared():
    return {'Git': L.expand(P['Git']['Path']), 'Py': L.expand(P['Interpreter']['Path']), 'Pwsh': L.expand(P['RuntimePwsh']['Path'])}


def abs_in(T, rel):
    return T + '/' + rel


def input_shas(T=L.CDIR):
    """SHA-256 of the bytes each --file names: the custodied copies (= the copies in the -C) and the contract file of the tree."""
    s = dict((r, L.sha256_file(L.CUSTODY[r])) for r in ('DELEG', 'H', 'TRX'))
    kb = L.git(T, 'cat-file', 'blob', L.K_BLOB).stdout
    if L.blob_sha1(kb) != L.K_BLOB:
        raise SystemExit('contract blob not readable in ' + T)
    s['CONTRACT'] = L.sha256(kb)
    return s


def order_argvs(T=L.CDIR, shas=None):
    shas = shas or input_shas(T)
    k, d, h, t = abs_in(T, L.K), abs_in(T, L.DELEG_E), abs_in(T, L.H_E), abs_in(T, L.TRX_PLACE)

    def fa(name, p):
        return ['--file', name + '=' + p, '--sha256', name + '=' + shas[name]]
    o = {
        'identity': ['identity', '--repo', T, '--branch', L.BR, '--base', L.TIP, '--current', L.G, '--red', L.RED],
        'diff': ['diff', '--repo', T, '--from', L.TIP, '--to', L.G],
        'scope': ['scope', '--repo', T, '--from', L.TIP, '--to', L.G] + fa('DELEG', d),
        'contract': ['contract', '--repo', T] + fa('CONTRACT', k) + fa('DELEG', d),
        'hashes': ['hashes', '--repo', T, '--rev', L.G, '--paths'] + sorted(L.DELIV_FILES),
        'cleantree': ['cleantree', '--repo', T, '--ignore-path', L.IGNORE_PATH],
        'handoff': ['handoff', '--repo', T] + fa('H', h) + ['--schema-rev', L.G],
        'freetext': ['freetext', '--repo', T] + fa('H', h) + ['--terms-rev', L.MAIN],
        'clausemap': ['clausemap', '--repo', T, '--main', L.MAIN, '--state-rev', L.G, '--state-path', L.STATE] + fa('CONTRACT', k) + ['--base', L.TIP],
        'trx': ['trx', '--repo', T] + fa('TRX', t) + ['--filter-substring', 'Subtract'],
        'read-text': ['read-text', '--repo', T, '--rev', L.G, '--path', L.DEC, '--lines', '1-1'],
        'read-json': ['read-json', '--repo', T, '--rev', L.G, '--path', L.K] + sum([['--pointer', x] for x in L.JSON_POINTERS], []),
    }
    return [(n, o[n]) for n in L.ORDERS]


def pw_sq(s):
    return "'" + s.replace("'", "''") + "'"


def inner_cmd(argv, git, py):
    return '& ' + pw_sq(git) + ' --no-replace-objects cat-file blob ' + L.B + ' | & ' + pw_sq(py) + ' -I - ' + ' '.join(argv)


# ------------------------------------------------------------------ primary contrasts (fixed list; markers M-Pnn)
PRIMARY = [
    ('P01', 'A', 'identity', 'git rev-parse HEAD'),
    ('P02', 'A', 'identity', 'git rev-parse --abbrev-ref HEAD'),
    ('P03', 'A', 'identity', 'git rev-parse --show-toplevel'),
    ('P04', 'A', 'identity', 'git rev-parse refs/remotes/origin/' + L.BR),
    ('P05', 'A', 'identity', 'git rev-parse refs/remotes/origin/main'),
    ('P06', 'A', 'identity', 'git merge-base --is-ancestor %s %s' % (L.TIP, L.G)),
    ('P07', 'A', 'identity', 'git merge-base --is-ancestor %s %s' % (L.TIP, L.RED)),
    ('P08', 'A', 'identity', 'git merge-base --is-ancestor %s %s' % (L.RED, L.G)),
    ('P09', 'A', 'diff', 'git diff --name-only --no-renames %s..%s' % (L.TIP, L.G)),
    ('P10', 'A', 'diff', 'git rev-list --reverse %s..%s' % (L.TIP, L.G)),
    ('P11', 'A', 'diff', "git log --no-show-signature '--format=%%H%%x20%%(trailers:key=Co-Authored-By,valueonly,separator=%%x7C)' %s..%s" % (L.TIP, L.G)),
    ('P12', 'B', 'cleantree', 'git --no-optional-locks status --porcelain'),
    ('P13', 'B', 'cleantree', 'git --no-optional-locks status --porcelain --ignored'),
    ('P14', 'B', 'cleantree', 'git check-ignore -v --no-index ' + L.IGNORE_PATH),
    ('P15', 'B', 'hashes', 'git rev-parse ' + ' '.join('%s:%s' % (L.G, p) for p in sorted(L.DELIV_FILES))),
    ('P16', 'B', 'hashes', 'git hash-object --no-filters -- ' + ' '.join(sorted(L.DELIV_FILES))),
    ('P17', 'B', 'read-json', 'git rev-parse %s:%s' % (L.G, L.K)),
    ('P18', 'B', 'contract', 'git hash-object --no-filters -- <REPO>/' + ' <REPO>/'.join((L.K, L.DELEG_E, L.H_E, L.TRX_PLACE))),
    ('P19', 'B', 'read-text', 'git rev-parse %s:%s' % (L.G, L.DEC)),
    ('P20', 'B', 'handoff', 'git rev-parse %s:%s' % (L.G, L.HANDOFF_SCHEMA)),
    ('P21', 'B', 'freetext', 'git rev-parse %s:%s' % (L.MAIN, L.AP)),
    ('P22', 'B', 'clausemap', 'git rev-list --first-parent --merges ' + L.MAIN),
    ('P23', 'B', 'clausemap', 'git rev-parse <Eff>:' + L.MAP),
    ('P24', 'B', 'clausemap', 'git rev-parse %s:%s' % (L.G, L.STATE)),
]
PIDS = [x[0] for x in PRIMARY]
LASTEXIT = ('P06', 'P07', 'P08', 'P14')
PRECOND_COMMITS = [L.TIP, L.RED, L.G, L.MAIN]
def precond_files(T=L.CDIR):
    return [abs_in(T, x) for x in (L.DELEG_E, L.H_E, L.TRX_PLACE, L.K)] + [abs_in(T, x) for x in sorted(L.DELIV_FILES)]


def primary_cmd(pid, eff='<Eff>', T=L.CDIR):
    c = dict((x[0], x[3]) for x in PRIMARY)[pid]
    return c.replace('<Eff>', eff).replace('<REPO>', T)


def primary_script(block, T=L.CDIR):
    parts = []
    for pid, b, _, cmd in PRIMARY:
        if b != block:
            continue
        parts.append('echo M-' + pid)
        parts.append(cmd.replace('<REPO>', T))
        if pid in LASTEXIT:
            parts.append('echo $LASTEXITCODE')
    return '; '.join(parts)


def precond_script(T=L.CDIR):
    return '; '.join(['git cat-file -t ' + c for c in PRECOND_COMMITS] + ['Test-Path -LiteralPath ' + f for f in precond_files(T)])


def arm_prompt(git, py, template=None, T=L.CDIR):
    t = L.rbytes(template or TEMPLATE).decode('ascii')
    orders = ['C4 a C15 (instrumento de hechos; una orden por ejecucion, copiada literalmente):', '']
    for k, (name, argv) in enumerate(order_argvs(T)):
        orders.append('C%d (%s):' % (k + 4, name))
        orders.append(inner_cmd(argv, git, py))
        orders.append('')
    rep = {'{RUN_ID}': L.RUN_ID, '{CDIR_WIN}': T.replace('/', '\\'), '{GIT}': git, '{PY}': py, '{B12}': L.B12,
           '{PRECONDITIONS}': precond_script(T), '{ORDERS}': '\n'.join(orders).rstrip('\n'),
           '{PRIMARY_A}': primary_script('A', T), '{PRIMARY_B}': primary_script('B', T)}
    for k, v in rep.items():
        t = t.replace(k, v)
    t = t.rstrip('\n')                     # the runner passes "$(cat prompt.txt)", which drops trailing newlines
    if re.search(r'\{[A-Z_0-9]+\}', t):
        raise SystemExit('unfilled placeholder in the prompt')
    if '"' in t:
        raise SystemExit('the armed prompt must not contain double quotes')
    if t.count(L.B) != 12:
        raise SystemExit('the full blob id must appear exactly in the twelve orders')
    return t


# ------------------------------------------------------------------ output schema (full and strict projection)
def _obj(props, desc=None):
    o = {'type': 'object', 'additionalProperties': False, 'required': sorted(props), 'properties': props}
    if desc:
        o['description'] = desc
    return o


def _arr(items, desc=None):
    o = {'type': 'array', 'items': items}
    if desc:
        o['description'] = desc
    return o


def build_schema(strict=False):
    S = {'type': 'string'}
    NE = {'type': 'string', 'minLength': 1}
    H40 = {'type': 'string', 'pattern': '^[0-9a-f]{40}$'}
    H64 = {'type': 'string', 'pattern': '^[0-9a-f]{64}$'}
    N40 = {'type': ['string', 'null'], 'pattern': '^[0-9a-f]{40}$'}
    N64 = {'type': ['string', 'null'], 'pattern': '^[0-9a-f]{64}$'}
    C = {'type': 'string', 'pattern': '^C[1-9][0-9]*$', 'description': 'Ejecucion de la shell C<n>, contadas desde 1 incluidas las fallidas.'}
    B = {'type': 'boolean'}
    I = {'type': 'integer'}
    J = {'type': 'string', 'minLength': 1, 'description': 'Texto JSON en ASCII (escapes \\uXXXX) copiado del sobre.'}
    ar_item = _obj({
        'Path': NE, 'Section': NE, 'Class': {'type': 'string', 'enum': ['UNIT_DOC', 'UNIT_CHANGE', 'EXTERNAL']},
        'Mode': {'type': 'string', 'enum': ['NORMAL', 'COMPAT', 'COMPUESTA', 'ENTRY']},
        'RevisionSha': N40,
        'Parts': _arr(_obj({'Section': NE, 'RevisionSha': H40})),
        'EffectiveSha': H40, 'ClauseMapBlob': H40,
        'UnitProtocol': {'type': 'string', 'enum': ['rackcad-protocol/I61', 'rackcad-protocol/I62']}})
    props = {
        'Schema': {'type': 'string', 'enum': ['rackcad-a6m7-measurement/1']},
        'Mode': {'type': 'string', 'enum': ['A6-7_MEDICION']},
        'RunId': {'type': 'string', 'enum': [L.RUN_ID]},
        'ShellInvocations': {'type': 'integer', 'minimum': 1},
        'GitByName': _obj({'Invocation': C, 'WhereLines': _arr(S), 'ExecPath': S, 'Version': S}),
        'Delivery': _obj({'Invocation': C, 'ObjectType': S, 'RehashedId': S}),
        'Preconditions': _obj({'Invocation': C, 'Commits': _arr(_obj({'Sha': H40, 'Type': S})),
                               'Files': _arr(_obj({'Path': NE, 'Exists': B}))}),
        'Runs': _arr(_obj({'Order': {'type': 'string', 'enum': list(L.ORDERS)}, 'Invocation': C, 'ExitCode': I,
                           'FactsSha256': N64, 'ErrorCode': {'type': ['string', 'null']}})),
        'Primary': _arr(_obj({'Id': {'type': 'string', 'enum': PIDS}, 'Invocation': C, 'GitCommand': NE, 'OutputLines': _arr(S),
                              'InstrumentValue': S, 'Equal': B})),
        'Identity': _obj({'Head': H40, 'AbbrevRef': S, 'Toplevel': S, 'OriginBranch': N40, 'OriginMain': N40,
                          'BaseIsAncestorOfCurrent': B, 'BaseIsAncestorOfRed': B, 'RedIsAncestorOfCurrent': B}),
        'Diff': _obj({'Paths': _arr(S), 'Commits': _arr(H40)}),
        'Scope': _obj({'PerPath': _arr(_obj({'Path': NE, 'AllowedCoveredBy': _arr(S), 'ForbiddenCoveredBy': _arr(S)}))}),
        'Trailer': _obj({'PerCommit': _arr(_obj({'Commit': H40, 'CoAuthoredBy': _arr(S)}))}),
        'CleanTree': _obj({'StatusPorcelain': _arr(S), 'StatusIgnored': _arr(S), 'CheckIgnoreRc': I}),
        'Hashes': _obj({'PerPath': _arr(_obj({'Path': NE, 'RevBlob': N40, 'RevBlobSha256': N64, 'WorktreeRawBlobSha1': N40,
                                              'WorktreeSha256': N64}))}),
        'Reads': _obj({'ContractPointersJson': _obj({'Objective': J, 'Schema': J, 'MainSha': J, 'AuthorityRevision': J}),
                       'DecisionsFirstLineJson': J, 'DecisionsFirstLineNonAsciiCount': I, 'HandoffFieldsJson': J,
                       'TrxSha256': H64, 'TrxCountersJson': J, 'TrxFilterSelected': _arr(S), 'FreeTextTermsJson': J,
                       'FreeTextMatchCount': I, 'ContractA5MissingJson': J, 'ContractSupersededEqual': B}),
        'ClauseMap': _obj({'Eff': N40, 'EffParent1': N40, 'MapBlob': N40}),
        'AuthorityResolution': _arr(ar_item, 'Forma de AuthorityResolution (paso 5 de Resolve, AP 16.13), una fila por cita del contrato.'),
        'Findings': _arr(S),
    }
    sch = _obj(props, 'Salida de la medicion A6-7 (I-62) del EXECUTION_CONTROLLER de FX-02: hechos, contrastes primarios y '
                      'resolucion por el mapa de clausulas; no es una verificacion.')
    sch['title'] = 'rackcad-a6m7-measurement/1'
    if strict:
        def strip(x):
            if isinstance(x, dict):
                return dict((k, strip(v)) for k, v in x.items() if k != 'minLength')
            if isinstance(x, list):
                return [strip(v) for v in x]
            return x
        sch = strip(sch)
        sch['description'] = sch['description'] + ' Proyeccion estricta (sin minLength).'
    return sch


def _type_ok(t, v):
    if t == 'string':
        return isinstance(v, str)
    if t == 'integer':
        return isinstance(v, int) and not isinstance(v, bool)
    if t == 'boolean':
        return isinstance(v, bool)
    if t == 'null':
        return v is None
    if t == 'object':
        return isinstance(v, dict)
    if t == 'array':
        return isinstance(v, list)
    if t == 'number':
        return isinstance(v, (int, float)) and not isinstance(v, bool)
    return False


def validate(s, v, ptr='#', out=None):
    """Validator of the keyword subset used by the kit schema (type, enum, required, properties, additionalProperties, items,
    minLength, pattern, minimum)."""
    out = [] if out is None else out
    ts = s.get('type')
    if ts is not None:
        tl = ts if isinstance(ts, list) else [ts]
        if not any(_type_ok(t, v) for t in tl):
            out.append('%s: type %s expected' % (ptr, ts))
            return out
    if 'enum' in s and v not in s['enum']:
        out.append('%s: not in enum' % ptr)
    if isinstance(v, str):
        if 'minLength' in s and len(v) < s['minLength']:
            out.append('%s: shorter than minLength' % ptr)
        if 'pattern' in s and re.search(s['pattern'], v) is None:
            out.append('%s: does not match %s' % (ptr, s['pattern']))
    if isinstance(v, int) and not isinstance(v, bool) and 'minimum' in s and v < s['minimum']:
        out.append('%s: below minimum' % ptr)
    if isinstance(v, dict):
        for k in s.get('required', []):
            if k not in v:
                out.append('%s: missing required %s' % (ptr, k))
        props = s.get('properties', {})
        for k, x in v.items():
            if k in props:
                validate(props[k], x, ptr + '/' + k, out)
            elif s.get('additionalProperties') is False:
                out.append('%s: additional property %s' % (ptr, k))
    if isinstance(v, list) and 'items' in s:
        for i, x in enumerate(v):
            validate(s['items'], x, '%s/%d' % (ptr, i), out)
    return out


def strict_json(data):
    def hook(pairs):
        d = {}
        for k, val in pairs:
            if k in d:
                raise ValueError('duplicate key ' + k)
            d[k] = val
        return d
    return json.loads(data.decode('utf-8'), object_pairs_hook=hook)


# ------------------------------------------------------------------ oracle (Git primitives + custodied bytes; no instrument)
def u19(e):
    return e[:-2] if e.endswith('/**') else e


def covers(entry, path):
    return path == entry or (entry.endswith('/') and path.startswith(entry))


def norm_ws(s):
    return re.sub(r'\s+', ' ', s).strip()


def word_char(c):
    return c != '' and (c.isalnum() or c == '_')


def count_whole(text, term):
    hay, needle = norm_ws(text).casefold(), norm_ws(term).casefold()
    n, i = 0, 0
    while needle:
        j = hay.find(needle, i)
        if j < 0:
            break
        b = hay[j - 1] if j > 0 else ''
        e = j + len(needle)
        a = hay[e] if e < len(hay) else ''
        if not word_char(b) and not word_char(a):
            n += 1
            i = e
        else:
            i = j + 1
    return n


def gate_terms_oracle(ap_text):
    lines = ap_text.replace('\r\n', '\n').split('\n')
    idx = [i for i, l in enumerate(lines) if norm_ws(l) == H1610]
    if len(idx) != 1:
        return None
    k = idx[0] + 1
    while k < len(lines) and lines[k].strip() == '':
        k += 1
    para = []
    while k < len(lines) and lines[k].strip() != '':
        para.append(lines[k])
        k += 1
    joined = '\n'.join(para)
    cut = joined.find('No se aplica')
    if cut < 0:
        return None
    terms = []
    for span in re.findall(r'`([^`]+)`', joined[:cut]):
        t = norm_ws(span)
        if t and t not in FREE_TEXT_FIELDS and t not in terms:
            terms.append(t)
    return terms


def trx_oracle(data):
    root = ET.fromstring(data)
    local = lambda t: t.rsplit('}', 1)[-1] if isinstance(t, str) else ''
    summ = [e for e in list(root) if local(e.tag) == 'ResultSummary']
    cnt = [e for e in list(summ[0]) if local(e.tag) == 'Counters']
    counters = dict((local(k), v) for k, v in cnt[0].attrib.items())
    tests = [{'TestName': e.attrib['testName'], 'Outcome': e.attrib['outcome']} for e in root.iter() if local(e.tag) == 'UnitTestResult']
    return {'Sha256': L.sha256(data), 'Counters': counters, 'Tests': tests,
            'FailedTests': [t['TestName'] for t in tests if t['Outcome'] == 'Failed'],
            'FilterSelected': [t['TestName'] for t in tests if 'Subtract' in t['TestName']]}


def oracle(T=L.CDIR):
    g = lambda *a: L.gout(T, *a)
    gl = lambda *a: L.glines(T, *a)
    anc = lambda a, b: L.git(T, 'merge-base', '--is-ancestor', a, b).returncode == 0
    o = {'Schema': 'i62-a6m7-oracle/1', 'Repo': T, 'Note': 'computed by the supervision from Git primitives (declared git by full path, '
         'GIT_* removed, --no-replace-objects) and the custodied bytes only; never with the instrument, the acceptor or the SPEC'}
    o['Identity'] = {'Head': g('rev-parse', 'HEAD'), 'AbbrevRef': g('rev-parse', '--abbrev-ref', 'HEAD'),
                     'Toplevel': g('rev-parse', '--show-toplevel').replace('\\', '/'),
                     'OriginBranch': g('rev-parse', '--verify', '--quiet', 'refs/remotes/origin/' + L.BR) or None,
                     'OriginMain': g('rev-parse', '--verify', '--quiet', 'refs/remotes/origin/main') or None,
                     'Ancestry': {'base->current': anc(L.TIP, L.G), 'base->red': anc(L.TIP, L.RED), 'red->current': anc(L.RED, L.G)}}
    paths = [x.decode('utf-8') for x in L.git(T, 'diff', '--name-only', '--no-renames', '-z', L.TIP + '..' + L.G).stdout.split(b'\0') if x]
    commits = gl('rev-list', '--reverse', L.TIP + '..' + L.G)
    tr = []
    for c in commits:
        vals = [x for x in gl('log', '--no-show-signature', '-1', '--format=%(trailers:key=Co-Authored-By,valueonly)', c) if x.strip()]
        tr.append({'Commit': c, 'Count': len(vals), 'ValuesSha256': L.sha256(L.canon(vals).encode('ascii')),
                   'ValueSha256': [L.sha256(v.encode('utf-8')) for v in vals]})
    o['Diff'] = {'Paths': paths, 'Commits': commits}
    o['Trailer'] = tr
    deleg = json.loads(L.rbytes(L.CUSTODY['DELEG']).decode('utf-8'))
    o['Scope'] = [{'Path': p, 'AllowedCoveredBy': [e for e in deleg['AllowedWriteScope'] if covers(u19(e), p)],
                   'ForbiddenCoveredBy': [e for e in deleg['ForbiddenWriteScope'] if covers(u19(e), p)]} for p in paths]
    ci = L.git(T, 'check-ignore', '-v', '--no-index', L.IGNORE_PATH)
    o['CleanTree'] = {'StatusPorcelain': gl('status', '--porcelain'), 'StatusIgnored': gl('status', '--porcelain', '--ignored'),
                      'CheckIgnoreOutput': [x for x in ci.stdout.decode('utf-8').replace('\r\n', '\n').split('\n') if x],
                      'CheckIgnoreRc': ci.returncode}
    hp = []
    for p in sorted(L.DELIV_FILES):
        rb = g('rev-parse', '--verify', '--quiet', L.G + ':' + p) or None
        content = L.git(T, 'cat-file', 'blob', rb).stdout if rb else None
        f = os.path.join(T, *p.split('/'))
        wb = L.rbytes(f) if os.path.isfile(f) else None
        hp.append({'Path': p, 'RevBlob': rb, 'RevBlobSha256': L.sha256(content) if content is not None else None,
                   'WorktreeRawBlobSha1': L.blob_sha1(wb) if wb is not None else None, 'WorktreeSha256': L.sha256(wb) if wb is not None else None})
    o['Hashes'] = hp
    files = {}
    for role, rel in (('CONTRACT', L.K), ('DELEG', L.DELEG_E), ('H', L.H_E), ('TRX', L.TRX_PLACE)):
        b = L.rbytes(os.path.join(T, *rel.split('/')))
        files[role] = {'Path': abs_in(T, rel), 'Blob': L.blob_sha1(b), 'Sha256': L.sha256(b)}
    o['Files'] = files
    kbytes = L.git(T, 'cat-file', 'blob', g('rev-parse', L.G + ':' + L.K)).stdout
    Kd = json.loads(kbytes.decode('utf-8'))
    o['ReadJson'] = dict((ptr, Kd[ptr[1:]]) for ptr in L.JSON_POINTERS)
    db = g('rev-parse', L.G + ':' + L.DEC)
    dtext = L.git(T, 'cat-file', 'blob', db).stdout.decode('utf-8').replace('\r\n', '\n').split('\n')
    o['ReadText'] = {'Blob': db, 'Line1': dtext[0], 'NonAsciiCount': sum(1 for ch in dtext[0] if ord(ch) > 127)}
    hand = json.loads(L.rbytes(L.CUSTODY['H']).decode('utf-8'))
    o['HandoffFields'] = dict((f, hand[f] if f in hand else {'Absent': True}) for f in L.HANDOFF_FIELDS)
    o['HandoffSchemaBlob'] = g('rev-parse', L.G + ':' + L.HANDOFF_SCHEMA)
    o['Trx'] = trx_oracle(L.rbytes(L.CUSTODY['TRX']))
    apb = g('rev-parse', L.MAIN + ':' + L.AP)
    terms = gate_terms_oracle(L.git(T, 'cat-file', 'blob', apb).stdout.decode('utf-8'))
    matches = 0
    for f in FREE_TEXT_FIELDS:
        v = hand.get(f)
        texts = [v] if isinstance(v, str) else (v if isinstance(v, list) else [])
        for t in (terms or []):
            if sum(count_whole(x, t) for x in texts if isinstance(x, str)) > 0:
                matches += 1
    o['FreeText'] = {'ApBlob': apb, 'Terms': terms, 'MatchCount': matches}
    sid = lambda x, key: x.get(key)
    a5 = {'Invariants': [x for x in Kd['Invariants'] if x not in set(deleg['Invariants'])],
          'StopConditions': [x['Id'] for x in Kd['StopConditions'] if x['Id'] not in set(y['Id'] for y in deleg['StopConditions'])],
          'Authorities': [t for t in ('|'.join([x['Path'], x['Section'], x['Class']]) for x in Kd['Authorities'])
                          if t not in set('|'.join([y['Path'], y['Section'], y['Class']]) for y in deleg['Authorities'])],
          'RequiredTests': [x['Filter'] for x in Kd['RequiredTests'] if x['Filter'] not in [y['Filter'] for y in deleg['RequiredTests']]],
          'RoleRequirements': [x['Role'] for x in Kd['RoleRequirements'] if x['Role'] not in set(y['Role'] for y in deleg['RoleRequirements'])]}
    o['Contract'] = {'A5Missing': a5, 'SupersededEqual': L.canon(Kd['SupersededCommits']) == L.canon(deleg['SupersededCommits'])}
    # clause map: EFF by the 16.14 derivation on Git primitives (first-parent merges of MAIN and the trailer line)
    logz = L.git(T, 'log', '--no-show-signature', '-z', '--format=%H%n%B', L.MAIN).stdout
    tcs = []
    for rec in logz.split(b'\0'):
        if not rec.strip():
            continue
        sha, _, body = rec.decode('utf-8').replace('\r\n', '\n').lstrip('\n').partition('\n')
        if TRAILER_LINE in body.split('\n'):
            tcs.append(sha)
    fpm = gl('rev-list', '--first-parent', '--merges', L.MAIN)
    cands = []
    if len(tcs) == 1:
        for m in reversed(fpm):
            p1, p2 = g('rev-parse', m + '^1'), g('rev-parse', m + '^2')
            if anc(tcs[0], p2) and not anc(tcs[0], p1):
                cands.append(m)
    eff = cands[0] if len(cands) == 1 else None
    e1 = g('rev-parse', eff + '^1') if eff else None
    mapb = g('rev-parse', eff + ':' + L.MAP) if eff else None
    o['ClauseMap'] = {'TrailerCommits': tcs, 'FirstParentMerges': fpm, 'Candidates': cands, 'Eff': eff, 'EffParent1': e1, 'MapBlob': mapb,
                      'StateBlob': g('rev-parse', L.G + ':' + L.STATE)}
    rows = lambda rev_of: [{'Path': a['Path'], 'Section': a['Section'], 'Class': a['Class'], 'Mode': 'NORMAL', 'RevisionSha': rev_of(a),
                            'Parts': [], 'EffectiveSha': eff, 'ClauseMapBlob': mapb, 'UnitProtocol': 'rackcad-protocol/I62'}
                           for a in Kd['Authorities']]
    o['AuthorityResolution'] = {
        'Fixed': ['Path', 'Section', 'Class', 'EffectiveSha', 'ClauseMapBlob', 'UnitProtocol'],
        'Readings': {
            'V5': {'Source': 'kit a2p2-v5 probe_tools.evaluate_i62 (RevisionSha = K.AuthorityRevision for UNIT_DOC/UNIT_CHANGE, K.MainSha for EXTERNAL)',
                   'Rows': rows(lambda a: Kd['AuthorityRevision'] if a['Class'] in ('UNIT_DOC', 'UNIT_CHANGE') else Kd['MainSha'])},
            'R3': {'Source': 'banco r3 lib_r3.authority_from (RevisionSha = --main for every row)',
                   'Rows': rows(lambda a: L.MAIN)}}}
    # git by name (declared git), primaries
    dg = declared()['Git']
    e = L.genv()
    o['GitByName'] = {'DeclaredPath': dg, 'Version': subprocess.run([dg, '--version'], capture_output=True, env=e).stdout.decode().strip(),
                      'ExecPath': subprocess.run([dg, '--exec-path'], capture_output=True, env=e).stdout.decode().strip()}
    o['Delivery'] = {'ObjectType': 'blob', 'RehashedId': L.B}
    o['Preconditions'] = {'Commits': dict((c, 'commit') for c in PRECOND_COMMITS), 'Files': dict((f, True) for f in precond_files(T))}
    exp = {'P01': [o['Identity']['Head']], 'P02': [o['Identity']['AbbrevRef']], 'P03': [o['Identity']['Toplevel']],
           'P04': [o['Identity']['OriginBranch']], 'P05': [o['Identity']['OriginMain']], 'P06': ['0'], 'P07': ['0'], 'P08': ['0'],
           'P09': paths, 'P10': commits, 'P11': None, 'P12': o['CleanTree']['StatusPorcelain'], 'P13': o['CleanTree']['StatusIgnored'],
           'P14': o['CleanTree']['CheckIgnoreOutput'] + [str(ci.returncode)], 'P15': [x['RevBlob'] for x in hp],
           'P16': [x['WorktreeRawBlobSha1'] for x in hp], 'P17': [files['CONTRACT']['Blob']],
           'P18': [files[r]['Blob'] for r in ('CONTRACT', 'DELEG', 'H', 'TRX')], 'P19': [db], 'P20': [o['HandoffSchemaBlob']],
           'P21': [apb], 'P22': fpm, 'P23': [mapb], 'P24': [o['ClauseMap']['StateBlob']]}
    o['ExpectedPrimary'] = exp
    o['PrimaryCommands'] = dict((pid, primary_cmd(pid, eff or '<Eff>', T)) for pid in PIDS)
    return o


# ------------------------------------------------------------------ events helpers
def parse_events(data):
    ev = []
    for line in data.split(b'\n'):
        if line.strip():
            try:
                ev.append(json.loads(line.decode('utf-8')))
            except ValueError:
                ev.append({'type': '<unparsable>'})
    return ev


def command_items(events):
    return [e['item'] for e in events if e.get('type') == 'item.completed' and isinstance(e.get('item'), dict)
            and e['item'].get('type') == 'command_execution']


def inner_of(command, R):
    try:
        t = R.split_outer(command)
    except Exception:
        return None, None
    if len(t) == 3 and t[1] == '-Command':
        return t[2], t[0]
    if len(t) == 4 and t[1] == '-NoProfile' and t[2] == '-Command':
        return t[3], t[0]
    if len(t) == 3 and t[1] == '/c':
        return t[2], t[0]
    return None, (t[0] if t else None)


def out_lines(agg):
    return [x.rstrip() for x in (agg or '').replace('\r\n', '\n').split('\n')]


def segments(agg):
    segs, cur = {}, None
    for x in out_lines(agg):
        m = re.fullmatch(r'M-(P\d\d)', x.strip())
        if m:
            cur = m.group(1)
            segs.setdefault(cur, [])
            continue
        if cur and x.strip() != '':
            segs[cur].append(x)
    return segs


def same_path(a, b):
    if not isinstance(a, str) or not isinstance(b, str):
        return False
    n = lambda s: os.path.normcase(os.path.normpath(s.replace('/', '\\')))
    return n(a) == n(b)


ABS_RE = re.compile(r'[A-Za-z]:[\\/][^\s\'"|;,]*')


def p22_scan(items, R, allowed, T=L.CDIR):
    """Every absolute path in the recorded commands must be under the -C or be a declared executable / the shell; no '..',
    no git -C, --git-dir, --work-tree, GIT_DIR, no I-62 worktree or .codex path."""
    sus = []
    for k, it in enumerate(items):
        cmd = it.get('command') or ''
        inner, shell = inner_of(cmd, R)
        text = inner if inner is not None else cmd
        low = text.lower()
        for bad in ('worktrees', '.codex', '--git-dir', '--work-tree', 'git_dir', '..\\', '../'):
            if bad in low:
                sus.append({'Invocation': 'C%d' % (k + 1), 'Token': bad})
        if re.search(r'(?:^|[\s;&|\'])git(?:\.exe)?[\'"]?\s+(?:--no-replace-objects\s+)?-C\s', text):
            sus.append({'Invocation': 'C%d' % (k + 1), 'Token': 'git -C'})
        for m in ABS_RE.findall(text):
            p = m.rstrip("'").rstrip(')')
            if same_path(p, T) or os.path.normcase(p.replace('/', '\\')).startswith(os.path.normcase(T.replace('/', '\\') + '\\')):
                continue
            if any(same_path(p, a) for a in allowed):
                continue
            sus.append({'Invocation': 'C%d' % (k + 1), 'Path': L.sanitize_str(p)})
    return sus


# ------------------------------------------------------------------ session record (only the invocation's own; model/effort)
def session_extract(root, thread_id):
    res = {'ThreadId': thread_id}
    if not thread_id or not re.fullmatch(r'[0-9a-fA-F-]{8,}', thread_id):
        res.update({'Ok': False, 'Problem': 'no thread_id in thread.started'})
        return res
    files = sorted(glob.glob(os.path.join(root, '*', '*', '*', 'rollout-*%s.jsonl' % thread_id)))
    res['Files'] = len(files)
    if len(files) != 1:
        res.update({'Ok': False, 'Problem': 'session record of the thread not found exactly once (%d)' % len(files)})
        return res
    data = L.rbytes(files[0])
    res['Sha256'] = L.sha256(data)
    res['Name'] = os.path.basename(files[0])
    tc, sm = [], []
    for line in data.split(b'\n'):
        if not line.strip():
            continue
        try:
            e = json.loads(line.decode('utf-8'))
        except ValueError:
            continue
        pl = e.get('payload') if isinstance(e.get('payload'), dict) else {}
        if e.get('type') == 'turn_context':
            tc.append({'Model': pl.get('model'), 'Effort': pl.get('effort')})
        elif e.get('type') == 'session_meta':
            sm.append(pl.get('model'))
    res['TurnContexts'] = tc
    res['SessionMetaModel'] = sm[0] if sm else None
    pairs = set((x['Model'], x['Effort']) for x in tc)
    res['Consistent'] = len(pairs) == 1
    res['Model'], res['Effort'] = (list(pairs)[0] if len(pairs) == 1 else (None, None))
    res['Ok'] = bool(tc) and res['Consistent'] and res['Model'] == P['Cell']['Model'] and res['Effort'] == P['Cell']['Effort']
    if not res['Ok']:
        res['Problem'] = 'turn_context model/effort absent, inconsistent or not the cell %s/%s' % (P['Cell']['Model'], P['Cell']['Effort'])
    return res


USAGE_RE = re.compile(r'usage limit|rate limit|credits|quota', re.I)


def usage_signal(events):
    hits = []
    for e in events:
        t = e.get('type')
        if t in ('error', 'turn.failed'):
            hits.append(t)
        it = e.get('item') if isinstance(e.get('item'), dict) else {}
        if it.get('type') in ('agent_message', 'error') and USAGE_RE.search(str(it.get('text', '')) + str(it.get('message', ''))):
            hits.append(it.get('type'))
    return hits


# ------------------------------------------------------------------ the comparison
def _jsonish(text):
    try:
        return True, json.loads(text)
    except (TypeError, ValueError):
        return False, None


def _ascii(text):
    return isinstance(text, str) and all(ord(c) < 128 for c in text)


def compare(ctx):
    """ctx: Output (bytes|None), Events (bytes|None), Reconstruction (dict|None), Table (dict|None), Acceptor (dict|None),
    IdentityPre/IdentityPost (dict), Session (dict), Entry (dict), Launch (dict), Gates (dict), Pou (dict), Oracle (dict),
    RuleSha256, RuleNamed, Repo."""
    O = ctx['Oracle']
    T = ctx.get('Repo') or L.CDIR
    R = _load('reconstruct_runs_m7')
    dec = declared()
    ops, rec_items = {}, {}

    def op(name, checks):
        fails = [c for c in checks if not c[1]]
        ops[name] = {'Status': 'DEMONSTRATED' if not fails else 'NOT_DEMONSTRATED',
                     'Checks': [{'Check': c[0], 'Ok': bool(c[1]), 'Detail': c[2] if len(c) > 2 else None} for c in checks],
                     'Causes': [c[0] + (': ' + str(c[2]) if len(c) > 2 and c[2] is not None else '') for c in fails]}

    def item(name, ok, detail=None):
        rec_items[name] = {'Ok': bool(ok), 'Detail': detail}

    # ---- output
    out, oprobs = None, []
    if ctx.get('Output') is None:
        oprobs.append('no output.json')
    else:
        try:
            out = strict_json(ctx['Output'])
        except Exception as e:
            oprobs.append('output.json is not strict JSON: %s' % type(e).__name__)
    if out is not None:
        oprobs += validate(L.rjson(SCHEMA_FULL), out)
    o = out if isinstance(out, dict) else {}
    get = lambda *ks: _dig(o, *ks)
    # ---- events
    events = parse_events(ctx['Events']) if ctx.get('Events') else []
    items = command_items(events)
    cidx = lambda c: (int(c[1:]) - 1) if isinstance(c, str) and re.fullmatch(r'C[1-9][0-9]*', c) and int(c[1:]) <= len(items) else None
    citem = lambda c: items[cidx(c)] if cidx(c) is not None else None
    inner = lambda it: (inner_of(it.get('command') or '', R)[0] or it.get('command') or '') if it else ''
    rc = ctx.get('Reconstruction') or {}
    runs = rc.get('Runs') or []
    envs = {}
    for r in runs:
        try:
            envs.setdefault(r['Argv'][0], json.loads(L.rbytes(r['EnvelopePath']).decode('ascii')))
        except Exception:
            pass
    F = lambda order: (envs.get(order) or {}).get('Facts') or {}
    acc = ctx.get('Acceptor') or {}
    st = acc.get('Statuses') or {}
    inst_idx = [k for k, it in enumerate(items) if L.B in (it.get('command') or '')]
    shells = set()
    for r in runs:
        shells.add(os.path.normcase(r.get('Shell') or ''))

    # OP1 exact delivery of the object bytes by the declared order in the invocation's shell
    d_it = citem(get('Delivery', 'Invocation'))
    d_in = inner(d_it)
    d_lines = out_lines(d_it.get('aggregated_output')) if d_it else []
    shell_of = (inner_of(d_it.get('command') or '', R)[1] if d_it else None)
    op('OP1_entrega_exacta_bytes', [
        ('delivery invocation cited and recorded', d_it is not None, get('Delivery', 'Invocation')),
        ('declared git by full path with the abbreviated object name, cat-file -t, cat-file blob | hash-object --no-filters --stdin',
         d_it is not None and ('cat-file -t ' + L.B12) in d_in and ('cat-file blob ' + L.B12) in d_in and 'hash-object --no-filters --stdin' in d_in
         and dec['Git'].lower() in d_in.lower()),
        ('record shows type blob and the rehash equal to the pinned blob', 'blob' in [x.strip() for x in d_lines] and L.B in [x.strip() for x in d_lines]),
        ('exit code 0', d_it is not None and d_it.get('exit_code') == 0),
        ('output ObjectType blob and RehashedId = pinned blob', get('Delivery', 'ObjectType') == 'blob' and get('Delivery', 'RehashedId') == L.B),
        ('delivery shell = the single shell of every instrument execution (the shell of the invocation)', shell_of is not None
         and len(shells) == 1 and same_path(list(shells)[0], shell_of),
         {'Shell': L.sanitize_str(str(shell_of)), 'IsRuntimePwsh': bool(shell_of) and same_path(shell_of, dec['Pwsh'])}),
    ])
    ops['OP1_entrega_exacta_bytes']['ShellIsRuntimePwsh'] = bool(shell_of) and same_path(shell_of, dec['Pwsh'])

    # OP2 execution from the object, isolated (with the primary checks before, A6-1 rule 7)
    per_order = dict((n, [r for r in runs if (r.get('Argv') or [None])[0] == n]) for n in L.ORDERS)
    env_ok = []
    for r in runs:
        try:
            e = json.loads(L.rbytes(r['EnvelopePath']).decode('ascii'))
            env_ok.append(e.get('Runtime', {}).get('Isolated') is True and e.get('Runtime', {}).get('Python') == P['Interpreter']['PythonVersion']
                          and e.get('Error') is None and L.sha256(L.rbytes(r['EnvelopePath'])) == r['EnvelopeSha256'])
        except Exception:
            env_ok.append(False)
    pre_it = citem(get('Preconditions', 'Invocation'))
    pre_in = inner(pre_it)
    pre_lines = [x.strip() for x in out_lines(pre_it.get('aggregated_output'))] if pre_it else []
    out_runs = get('Runs') if isinstance(get('Runs'), list) else []
    runs_link = len(out_runs) == 12 and [r.get('Order') for r in out_runs] == list(L.ORDERS)
    for r in out_runs if runs_link else []:
        it = citem(r.get('Invocation'))
        rr = [x for x in runs if it is not None and x.get('ItemId') == it.get('id')]
        e = None
        if rr:
            try:
                e = json.loads(L.rbytes(rr[0]['EnvelopePath']).decode('ascii'))
            except Exception:
                e = None
        if not (it is not None and rr and rr[0]['Argv'][0] == r.get('Order') and it.get('exit_code') == r.get('ExitCode')
                and e is not None and e.get('FactsSha256') == r.get('FactsSha256')):
            runs_link = False
    op('OP2_ejecucion_desde_objeto_aislada', [
        ('session log reconstructible with the named rule', rc.get('Reconstructible') is True, rc.get('Problems')),
        ('each of the 12 orders executed at least once', all(per_order[n] for n in L.ORDERS), [n for n in L.ORDERS if not per_order[n]]),
        ('every execution: declared git and declared interpreter by full path, exit 0', bool(runs) and all(
            same_path(r.get('Git'), dec['Git']) and same_path(r.get('Interpreter'), dec['Py']) and r.get('ExitCode') == 0 for r in runs)),
        ('every envelope: Runtime.Isolated true, declared Python version, Error null, bytes = reconstructed SHA-256', bool(env_ok) and all(env_ok)),
        ('primary checks of revisions and files before the first instrument execution (A6-1 rule 7)',
         pre_it is not None and inst_idx and cidx(get('Preconditions', 'Invocation')) < inst_idx[0]
         and all(('cat-file -t ' + c) in pre_in for c in PRECOND_COMMITS) and all(('Test-Path -LiteralPath ' + f) in pre_in for f in precond_files(T))
         and pre_lines.count('commit') >= len(PRECOND_COMMITS) and pre_lines.count('True') >= len(precond_files(T))),
        ('output Runs: 12 rows in order, linked to the recorded executions (exit code, FactsSha256)', runs_link),
        ('acceptor R1 R2 R3 MATCH', all(st.get(x) == 'MATCH' for x in ('R1', 'R2', 'R3')), dict((x, st.get(x)) for x in ('R1', 'R2', 'R3'))),
    ])

    # primary contrasts (record)
    prim_rows = dict((r.get('Id'), r) for r in (get('Primary') if isinstance(get('Primary'), list) else []) if isinstance(r, dict))
    eff = O['ClauseMap']['Eff']
    prim_res = {}
    for pid in PIDS:
        r = prim_rows.get(pid)
        cmd = primary_cmd(pid, eff, T)
        res = {'Present': r is not None}
        if r is None:
            prim_res[pid] = dict(res, Ok=False, Cause='row absent')
            continue
        it = citem(r.get('Invocation'))
        txt = inner(it)
        gc = norm_ws(re.sub(r';\s*echo \$LASTEXITCODE\s*$', '', r.get('GitCommand') or ''))
        seg = segments(it.get('aggregated_output')) .get(pid) if it else None
        exp = O['ExpectedPrimary'][pid]
        if pid == 'P11':
            okv = seg is not None and _p11_ok(seg, O)
        else:
            okv = seg is not None and [x.strip() for x in seg] == [str(x) for x in exp]
        checks = {'Recorded': it is not None and norm_ws(cmd) in norm_ws(txt) and ('M-' + pid) in txt,
                  'GitCommandExact': gc == norm_ws(cmd), 'OutputEqualsOracle': okv,
                  'OutputLinesEqualRecord': seg is not None and [x.strip() for x in (r.get('OutputLines') or [])] == [x.strip() for x in seg],
                  'EqualTrue': r.get('Equal') is True}
        prim_res[pid] = dict(res, Ok=all(checks.values()), Checks=checks)
    pok = lambda *ids: all(prim_res[i]['Ok'] for i in ids)
    pbad = lambda *ids: [i for i in ids if not prim_res[i]['Ok']]

    # OP3 git diff --name-only <base>..<head> reproducible and Scope membership by path
    sp = sorted(F('scope').get('PerPath') or [], key=lambda x: x.get('Path', ''))
    osp = sorted(O['Scope'], key=lambda x: x['Path'])
    outsp = sorted([dict((k, x.get(k)) for k in ('Path', 'AllowedCoveredBy', 'ForbiddenCoveredBy')) for x in (get('Scope', 'PerPath') or []) if isinstance(x, dict)],
                   key=lambda x: x['Path'] or '')
    op('OP3_diff_y_pertenencia_Scope', [
        ('diff envelope Paths = oracle', F('diff').get('Paths') == O['Diff']['Paths']),
        ('scope envelope PerPath = oracle', [dict((k, x.get(k)) for k in ('Path', 'AllowedCoveredBy', 'ForbiddenCoveredBy')) for x in sp] == osp),
        ('output Diff.Paths = oracle', get('Diff', 'Paths') == O['Diff']['Paths']),
        ('output Scope.PerPath = oracle', outsp == osp),
        ('primary P09 (git diff --name-only) recorded and equal', pok('P09'), pbad('P09')),
        ('acceptor R5 and R7 MATCH', st.get('R5') == 'MATCH' and st.get('R7') == 'MATCH', [st.get('R5'), st.get('R7')]),
    ])

    # OP4 Git reads of Identity, Remote, CleanTree and Trailer
    idf = F('identity')
    oi = O['Identity']
    outid = get('Identity') or {}
    tr_out = get('Trailer', 'PerCommit') if isinstance(get('Trailer', 'PerCommit'), list) else []
    tr_env = F('diff').get('CoAuthoredBy') or {}
    otr = dict((x['Commit'], x['ValuesSha256']) for x in O['Trailer'])
    ct = F('cleantree')
    op('OP4_lecturas_Identity_Remote_CleanTree_Trailer', [
        ('identity envelope = oracle', dict((k, idf.get(k)) for k in ('Head', 'AbbrevRef', 'Toplevel', 'OriginBranch', 'OriginMain', 'Ancestry'))
         == dict((k, oi[k]) for k in ('Head', 'AbbrevRef', 'Toplevel', 'OriginBranch', 'OriginMain', 'Ancestry'))),
        ('output Identity = oracle', [outid.get(k) for k in ('Head', 'AbbrevRef', 'Toplevel', 'OriginBranch', 'OriginMain')]
         == [oi[k] for k in ('Head', 'AbbrevRef', 'Toplevel', 'OriginBranch', 'OriginMain')]
         and [outid.get('BaseIsAncestorOfCurrent'), outid.get('BaseIsAncestorOfRed'), outid.get('RedIsAncestorOfCurrent')]
         == [oi['Ancestry']['base->current'], oi['Ancestry']['base->red'], oi['Ancestry']['red->current']]),
        ('Remote: OriginBranch and OriginMain (refs/remotes) = oracle', outid.get('OriginBranch') == oi['OriginBranch'] and outid.get('OriginMain') == oi['OriginMain']),
        ('cleantree envelope = oracle', ct.get('StatusPorcelain') == O['CleanTree']['StatusPorcelain'] and ct.get('StatusIgnored') == O['CleanTree']['StatusIgnored']
         and ct.get('CheckIgnoreRc') == O['CleanTree']['CheckIgnoreRc'] and ct.get('CheckIgnoreOutput') == O['CleanTree']['CheckIgnoreOutput']),
        ('output CleanTree = oracle', get('CleanTree', 'StatusPorcelain') == O['CleanTree']['StatusPorcelain']
         and get('CleanTree', 'StatusIgnored') == O['CleanTree']['StatusIgnored'] and get('CleanTree', 'CheckIgnoreRc') == O['CleanTree']['CheckIgnoreRc']),
        ('Trailer: diff envelope CoAuthoredBy = oracle (by SHA-256 of the values)',
         sorted(tr_env) == sorted(otr) and all(L.sha256(L.canon(tr_env[c]).encode('ascii')) == otr[c] for c in otr)),
        ('Trailer: output PerCommit = oracle (by SHA-256 of the values)',
         [x.get('Commit') for x in tr_out if isinstance(x, dict)] == O['Diff']['Commits']
         and all(isinstance(x, dict) and L.sha256(L.canon(x.get('CoAuthoredBy')).encode('ascii')) == otr.get(x.get('Commit')) for x in tr_out)),
        ('primaries P01-P08, P10-P14 recorded and equal', pok('P01', 'P02', 'P03', 'P04', 'P05', 'P06', 'P07', 'P08', 'P10', 'P11', 'P12', 'P13', 'P14'),
         pbad('P01', 'P02', 'P03', 'P04', 'P05', 'P06', 'P07', 'P08', 'P10', 'P11', 'P12', 'P13', 'P14')),
    ])

    # OP5 blobs and hashes
    hk = ('Path', 'RevBlob', 'RevBlobSha256', 'WorktreeRawBlobSha1', 'WorktreeSha256')
    norm_h = lambda lst: sorted([dict((k, x.get(k)) for k in hk) for x in lst if isinstance(x, dict)], key=lambda x: x['Path'] or '')
    op('OP5_blobs_y_hashes', [
        ('hashes envelope PerPath = oracle', norm_h(F('hashes').get('PerPath') or []) == norm_h(O['Hashes'])),
        ('output Hashes.PerPath = oracle', norm_h(get('Hashes', 'PerPath') or []) == norm_h(O['Hashes'])),
        ('primaries P15, P16 recorded and equal', pok('P15', 'P16'), pbad('P15', 'P16')),
        ('acceptor R5 MATCH', st.get('R5') == 'MATCH'),
    ])

    # OP6 faithful reading of JSON and text inputs
    rd = get('Reads') or {}
    cpj = rd.get('ContractPointersJson') or {}
    jok = lambda text, want: _ascii(text) and _jsonish(text) == (True, want)
    rt = F('read-text')
    tx = F('trx')
    op('OP6_lectura_fiel_JSON_y_texto', [
        ('read-json envelope Values = oracle', (F('read-json').get('Values') or {}) == O['ReadJson']),
        ('output ContractPointersJson: ASCII JSON text equal to the oracle values', all(jok(cpj.get(p[1:]), O['ReadJson'][p]) for p in L.JSON_POINTERS)),
        ('read-text envelope line 1 and NonAsciiCount = oracle', (rt.get('Lines') or [{}])[0].get('Text') == O['ReadText']['Line1']
         and rt.get('NonAsciiCount') == O['ReadText']['NonAsciiCount'] and rt.get('Blob') == O['ReadText']['Blob']),
        ('output DecisionsFirstLineJson (ASCII JSON) and count = oracle', jok(rd.get('DecisionsFirstLineJson'), O['ReadText']['Line1'])
         and rd.get('DecisionsFirstLineNonAsciiCount') == O['ReadText']['NonAsciiCount']),
        ('handoff envelope Fields = oracle', F('handoff').get('Fields') == O['HandoffFields']),
        ('output HandoffFieldsJson (ASCII JSON) = oracle', jok(rd.get('HandoffFieldsJson'), O['HandoffFields'])),
        ('trx envelope Sha256, Counters, FilterSelected = oracle', tx.get('Sha256') == O['Trx']['Sha256'] and tx.get('Counters') == O['Trx']['Counters']
         and tx.get('FilterSelected') == O['Trx']['FilterSelected']),
        ('output Trx fields = oracle', rd.get('TrxSha256') == O['Trx']['Sha256'] and jok(rd.get('TrxCountersJson'), O['Trx']['Counters'])
         and rd.get('TrxFilterSelected') == O['Trx']['FilterSelected']),
        ('freetext envelope Terms and match count = oracle', F('freetext').get('Terms') == O['FreeText']['Terms']
         and len(F('freetext').get('Matches') or []) == O['FreeText']['MatchCount']),
        ('output FreeTextTermsJson (ASCII JSON) and FreeTextMatchCount = oracle', jok(rd.get('FreeTextTermsJson'), O['FreeText']['Terms'])
         and rd.get('FreeTextMatchCount') == O['FreeText']['MatchCount']),
        ('contract envelope A5Missing and SupersededEqual = oracle', F('contract').get('A5Missing') == O['Contract']['A5Missing']
         and F('contract').get('SupersededEqual') == O['Contract']['SupersededEqual']),
        ('output ContractA5MissingJson (ASCII JSON) and ContractSupersededEqual = oracle', jok(rd.get('ContractA5MissingJson'), O['Contract']['A5Missing'])
         and rd.get('ContractSupersededEqual') == O['Contract']['SupersededEqual']),
        ('primaries P17-P21, P24 recorded and equal', pok('P17', 'P18', 'P19', 'P20', 'P21', 'P24'), pbad('P17', 'P18', 'P19', 'P20', 'P21', 'P24')),
    ])

    # OP7 clause-map resolution in the AuthorityResolution form
    cm = F('clausemap')
    der = cm.get('Derive') or {}
    ar = get('AuthorityResolution') if isinstance(get('AuthorityResolution'), list) else []
    arO = O['AuthorityResolution']
    fixed = arO['Fixed']
    item_schema = build_schema(False)['properties']['AuthorityResolution']['items']
    form_ok = bool(ar) and all(not validate(item_schema, x) for x in ar) and len(ar) == len(arO['Readings']['V5']['Rows'])
    fixed_ok = form_ok and all(dict((k, x.get(k)) for k in fixed) == dict((k, y[k]) for k in fixed) for x, y in zip(ar, arO['Readings']['V5']['Rows']))
    matched = [name for name, rd_ in sorted(arO['Readings'].items()) if form_ok and
               all(dict((k, x.get(k)) for k in ('Mode', 'RevisionSha', 'Parts')) == dict((k, y[k]) for k in ('Mode', 'RevisionSha', 'Parts'))
                   for x, y in zip(ar, rd_['Rows']))]
    op('OP7_mapa_de_clausulas_AuthorityResolution', [
        ('clausemap envelope Derive Eff / EffParent1 = oracle', der.get('Eff') == O['ClauseMap']['Eff'] and der.get('EffParent1') == O['ClauseMap']['EffParent1']),
        ('output ClauseMap = oracle', get('ClauseMap', 'Eff') == O['ClauseMap']['Eff'] and get('ClauseMap', 'EffParent1') == O['ClauseMap']['EffParent1']
         and get('ClauseMap', 'MapBlob') == O['ClauseMap']['MapBlob']),
        ('AuthorityResolution: one row per citation, in the AuthorityResolution form', form_ok, len(ar)),
        ('AuthorityResolution: Path, Section, Class, EffectiveSha, ClauseMapBlob, UnitProtocol = oracle', fixed_ok),
        ('AuthorityResolution: Mode, RevisionSha, Parts = one documented reading (V5 or R3; the Coordinator confirms which)', bool(matched), matched),
        ('primaries P22, P23 recorded and equal', pok('P22', 'P23'), pbad('P22', 'P23')),
        ('acceptor R6 MATCH', st.get('R6') == 'MATCH', st.get('R6')),
    ])
    ops['OP7_mapa_de_clausulas_AuthorityResolution']['MatchedReading'] = matched

    # OP8 contrast of every fact used with the primary Git order
    op('OP8_contraste_con_orden_Git_primaria', [
        ('every primary row P01-P24 present once, recorded, exact command, output = oracle and = record, Equal true',
         all(prim_res[p]['Ok'] for p in PIDS) and len(prim_rows) == len(PIDS), [p for p in PIDS if not prim_res[p]['Ok']]),
    ])
    ops['OP8_contraste_con_orden_Git_primaria']['Primary'] = prim_res

    # OP9 complete structured output
    cited = [c for c in _cited(o) if c is not None]
    op('OP9_salida_estructurada_completa', [
        ('output.json present, strict JSON, valid against schema.m7.json (no mandatory field omitted)', out is not None and not oprobs, oprobs[:10]),
        ('ShellInvocations = number of recorded shell executions', o.get('ShellInvocations') == len(items), [o.get('ShellInvocations'), len(items)]),
        ('every cited C<n> is a recorded execution', all(cidx(c) is not None for c in cited)),
        ('Runs 12 rows, Primary 24 rows, AuthorityResolution non-empty', len(out_runs) == 12 and len(prim_rows) == 24 and bool(ar)),
    ])

    # ---- record items
    g_it = citem(get('GitByName', 'Invocation'))
    g_in = inner(g_it)
    g_lines = [x.strip() for x in out_lines(g_it.get('aggregated_output'))] if g_it else []
    where = [x for x in g_lines if re.match(r'(?i)^[a-z]:\\.*\\git(\.[a-z0-9]+)?$', x)]
    item('RA_git_por_nombre_es_el_declarado',
         g_it is not None and 'where.exe git' in g_in and 'git --exec-path' in g_in and 'git --version' in g_in
         and bool(where) and same_path(where[0], dec['Git']) and any(same_path(x, L.expand(O['GitByName']['ExecPath'])) for x in g_lines)
         and O['GitByName']['Version'] in g_lines and (get('GitByName', 'WhereLines') or [None])[0] is not None
         and same_path((get('GitByName', 'WhereLines') or [''])[0], where[0])
         and (ctx.get('Gates') or {}).get('StaticGit', {}).get('Ok') is True,
         {'WhereFirst': L.sanitize_str(where[0]) if where else None, 'StaticScanOk': (ctx.get('Gates') or {}).get('StaticGit', {}).get('Ok')})
    item('RB_reconstruccion_Runs_regla_nombrada', rc.get('Reconstructible') is True and rc.get('InstrumentExecutions', 0) >= 1
         and len(runs) == rc.get('InstrumentExecutions') and ctx.get('RuleSha256') == ctx.get('RuleNamed') and bool(env_ok) and all(env_ok),
         {'Rule': rc.get('Rule'), 'RuleSha256': ctx.get('RuleSha256'), 'Named': ctx.get('RuleNamed'), 'Executions': rc.get('InstrumentExecutions'),
          'Renderings': sorted(set(r.get('Rendering') for r in runs if r.get('Rendering')))})
    tb = ctx.get('Table') or {}
    want = {'R1': 'MATCH', 'R2': 'MATCH', 'R3': 'MATCH', 'R9': 'MATCH', 'R4': 'NOT_APPLICABLE', 'R10': 'NOT_APPLICABLE', 'R8': 'MANUAL'}
    item('RC_aceptante_ACCEPT_FACTS_tabla', tb.get('Accepted') is True and acc.get('Rc') == 0 and acc.get('Decision') == 'ACCEPT_FACTS'
         and all(st.get(k) == v for k, v in want.items()) and acc.get('Provenance', {}).get('Ok') is True,
         {'Statuses': st, 'Table': dict((k, tb.get(k)) for k in ('Accepted', 'ConstructionViolations', 'AcceptanceReasons', 'ClosureViolations',
                                                                  'DesignationViolations')), 'Provenance': acc.get('Provenance')})
    ip, io = ctx.get('IdentityPre') or {}, ctx.get('IdentityPost') or {}
    keys = ('BinaryHash', 'AppVersion', 'Fingerprint', 'Companions', 'Bins', 'KeyNamesEqualBaseline', 'RuntimePwshSha256')
    item('RD_identidad_antes_y_despues', ip.get('EqualsTrio') is True and io.get('EqualsTrio') is True and all(ip.get(k) == io.get(k) for k in keys),
         {'Before': dict((k, ip.get(k)) for k in keys), 'After': dict((k, io.get(k)) for k in keys)})
    se = ctx.get('Session') or {}
    item('RE_modelo_effort_efectivos', se.get('Ok') is True, dict((k, se.get(k)) for k in ('Model', 'Effort', 'Consistent', 'Sha256', 'Problem')))
    sus = p22_scan(items, R, [dec['Git'], dec['Py'], dec['Pwsh']], T)
    item('RF_sin_lectura_P22', not sus, sus[:10])
    en = ctx.get('Entry') or {}
    item('RG_sin_escritura_en_el_clon', en.get('StateOk') is True, en.get('StateDetail'))
    la = ctx.get('Launch') or {}
    us = usage_signal(events)
    comp_ok = rc.get('Complete') is True
    item('RH_lanzamiento_terminacion_entrada', la.get('Exit') == 0 and la.get('TimedOut') is False and la.get('DeathConfirmed') is True and comp_ok
         and not us and en.get('DeclaredOk') is True,
         {'Exit': la.get('Exit'), 'TimedOut': la.get('TimedOut'), 'DeathConfirmed': la.get('DeathConfirmed'), 'EventsComplete': comp_ok,
          'UsageLimitSignals': us, 'EntryDeclaredInterpreterGit': en.get('DeclaredOk')})
    allok = all(v['Status'] == 'DEMONSTRATED' for v in ops.values()) and all(v['Ok'] for v in rec_items.values())
    causes = [k for k, v in ops.items() if v['Status'] != 'DEMONSTRATED'] + [k for k, v in rec_items.items() if not v['Ok']]
    return L.sanitize({'Schema': 'i62-a6m7-comparison/1', 'Operations': ops, 'RecordItems': rec_items,
                       'Verdict': VERDICT_OK if allok else VERDICT_NO, 'Causes': causes,
                       'Note': 'propuesta para el Coordinator; nunca una declaracion de ELIGIBLE'})


def _dig(o, *ks):
    cur = o
    for k in ks:
        if not isinstance(cur, dict) or k not in cur:
            return None
        cur = cur[k]
    return cur


def _cited(o):
    out = []
    for k in ('GitByName', 'Delivery', 'Preconditions'):
        out.append(_dig(o, k, 'Invocation'))
    for k in ('Runs', 'Primary'):
        for r in (o.get(k) if isinstance(o.get(k), list) else []):
            if isinstance(r, dict):
                out.append(r.get('Invocation'))
    return out


def _p11_ok(seg, O):
    got = {}
    for line in seg:
        sha, _, rest = line.strip().partition(' ')
        vals = [x for x in rest.split('|')] if rest.strip() else []
        got[sha] = L.sha256(L.canon(vals).encode('ascii'))
    want = dict((x['Commit'], x['ValuesSha256']) for x in O['Trailer'])
    return got == want


# ------------------------------------------------------------------ post-launch pipeline
def run_acceptor(cfg, req_path, env):
    """Fixed acceptor (434ef1fc, bytes by blob from the I-62 repository), declared interpreter, same -C, read-only; the -C
    snapshot before/after must be equal and no process of the run may survive it."""
    G = _load('gates_m7')
    prov = {'AcceptorBlob': L.ACC, 'Interpreter': L.sanitize_str(cfg.py), 'Repo': cfg.repo}
    accb = L.pinned_bytes(L.ACC, L.ACC_SHA256)
    prov['AcceptorBytesBlob'] = L.blob_sha1(accb)
    before = L.snapshot(cfg.repo, exe=cfg.git)
    t0 = L.utc()
    p = subprocess.Popen([cfg.py, '-I', '-', '--request', req_path], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                         env=env, cwd=cfg.repo)
    out, err = p.communicate(accb, timeout=3600)
    after = L.snapshot(cfg.repo, exe=cfg.git)
    rows = [r for r in (G.procs([req_path, cfg.repo], procname='') or []) if r.get('MatchesNeedle')] if cfg.check_processes else []
    prov.update({'StartUtc': t0, 'EndUtc': L.utc(), 'Rc': p.returncode, 'ZeroWrite': L.snapdiff(before, after) == {},
                 'SurvivingProcesses': rows, 'ProcessTreeEnded': rows == []})
    prov['Ok'] = prov['AcceptorBytesBlob'] == L.ACC and prov['ZeroWrite'] and prov['ProcessTreeEnded']
    try:
        res = json.loads(out.decode('ascii'))
    except Exception:
        res = {}
    statuses = dict((c.get('Id'), c.get('Status')) for c in res.get('Checks', []) if isinstance(c, dict))
    return {'Rc': p.returncode, 'Decision': res.get('Decision'), 'Statuses': statuses, 'Provenance': prov, 'Stdout': out,
            'StderrHead': err.decode('utf-8', 'replace')[:300]}


def post(cfg):
    G = _load('gates_m7')
    TM = _load('table_m7')
    out = cfg.out
    raw = out + '/raw'
    os.makedirs(raw, exist_ok=True)
    fr = L.rjson(cfg.kit + '/FREEZE.json')
    rule = P['ReconstructionRule']
    rule_sha = L.sha256_file(os.path.join(cfg.kit, rule))
    oracle_path = getattr(cfg, 'oracle_path', None) or (cfg.kit + '/frozen/oracle.json')
    ctx = {'Oracle': L.rjson(oracle_path), 'RuleSha256': rule_sha, 'RuleNamed': fr['ReconstructionRule']['Sha256'], 'Repo': cfg.repo}
    if cfg.mode == 'launch' and L.sha256_file(oracle_path) != fr['Oracle']['Sha256']:
        raise SystemExit('the oracle is not the sealed one')       # fixture runs (self-tests) use the oracle of their scratch -C
    # identity after
    ctx['IdentityPre'] = L.rjson(out + '/pre-launch-identity.json')
    ctx['IdentityPost'] = G.measure(cfg, 'post-launch', out)
    # Entry: state of the -C, declared interpreter and git again
    snap_before = L.rjson(raw + '/snap-before.json')
    snap_after = L.snapshot(cfg.repo, exe=cfg.git)
    d = L.snapdiff(snap_before, snap_after)
    allowed = (set(d) <= {'WorktreeFiles'} and d.get('WorktreeFiles', {}).get('Removed', []) == [] and d.get('WorktreeFiles', {}).get('Modified', []) == []
               and d.get('WorktreeFiles', {}).get('Added', []) in ([], [L.IGNORE_PATH]))
    pc = _load('pou_check')
    decl_ok = {}
    for role, path, want_sha, want_v in (('Git', cfg.git, P['Git']['Sha256'], P['Git']['Version']),
                                        ('Interpreter', cfg.py, P['Interpreter']['Sha256'], P['Interpreter']['Version'])):
        try:
            h, _ = pc.resolve_executable(path)
            v = subprocess.run([path, '--version'], capture_output=True, env=L.genv(), timeout=120).stdout.decode().strip()
            decl_ok[role] = L.sha256_file(h) == want_sha and v == want_v
        except Exception:
            decl_ok[role] = False
    ctx['Entry'] = {'StateOk': allowed, 'StateDetail': {'Diff': L.sanitize(d)}, 'DeclaredOk': all(decl_ok.values()), 'Declared': decl_ok}
    L.wjson(out + '/entry.json', ctx['Entry'])
    ctx['Launch'] = L.rjson(out + '/launch.json') if os.path.isfile(out + '/launch.json') else {}
    ev_path = raw + '/events.jsonl'
    ctx['Events'] = L.rbytes(ev_path) if os.path.isfile(ev_path) else None
    o_path = os.path.join(cfg.repo, *L.IGNORE_PATH.split('/'))
    ctx['Output'] = L.rbytes(o_path) if os.path.isfile(o_path) else None
    if ctx['Output'] is not None:
        L.wbytes(raw + '/output.json', ctx['Output'])
    events = parse_events(ctx['Events']) if ctx['Events'] else []
    tid = next((e.get('thread_id') for e in events if e.get('type') == 'thread.started'), None)
    ctx['Session'] = session_extract(cfg.sessions_root, tid)
    # reconstruction with the named rule
    R = _load(os.path.splitext(rule)[0])
    env_dir = out + '/env'
    rc = R.reconstruct(ctx['Events'] or b'', L.B, env_dir=env_dir) if ctx['Events'] else {'Reconstructible': False, 'Problems': ['no events']}
    ctx['Reconstruction'] = rc
    L.wjson(out + '/reconstruction.json', rc)
    # request, acceptor and table (only with a reconstructible log with executions, A6-3 rule 6)
    ctx['Acceptor'], ctx['Table'] = None, None
    if rc.get('Reconstructible') and rc.get('Runs'):
        runs = [{'Name': 'r%02d-%s' % (r['Seq'], r['Argv'][0]), 'Envelope': r['EnvelopePath'], 'Argv': r['Argv']} for r in rc['Runs']]
        consts = {'Repo': cfg.repo, 'Tool': {'PinnedBlob': L.B, 'RunFrom': 'object', 'GitObject': {'GitDir': cfg.repo, 'Blob': L.B}},
                  'Interpreter': {'Python': P['Interpreter']['PythonVersion'], 'ExecutableSha256': P['Interpreter']['Sha256']},
                  'Closure': {'Path': cfg.kit + '/frozen/closure.json', 'Sha256': L.sha256_file(cfg.kit + '/frozen/closure.json')}}
        req, cond = TM.construct_m(runs, consts)
        req_path = out + '/request.json'
        with open(req_path, 'w', encoding='utf-8', newline='\n') as fh:
            fh.write(json.dumps(req, ensure_ascii=False, indent=1) + '\n')
        env = json.loads(L.rbytes(raw + '/launch-env.json').decode('utf-8'))
        acc = run_acceptor(cfg, req_path, env)
        L.wbytes(out + '/acceptor-stdout.json', acc['Stdout'])
        recon_runs = [{'Argv': r['Argv'], 'EnvelopeSha256': r['EnvelopeSha256']} for r in rc['Runs']]
        tb = TM.evaluate_m(req_path, acc['Rc'], acc['Stdout'], cfg.kit + '/frozen/vector.json', cfg.kit + '/frozen/derivation.json',
                           cfg.kit + '/frozen/designation-M.json', executions=rc.get('InstrumentExecutions'), reconstructed=recon_runs)
        acc.pop('Stdout')
        ctx['Acceptor'], ctx['Table'] = acc, tb
        L.wjson(out + '/acceptor.json', acc)
        L.wjson(out + '/table.json', tb)
    ctx['Gates'] = L.rjson(out + '/gates.json') if os.path.isfile(out + '/gates.json') else {}
    ctx['Pou'] = L.rjson(out + '/pou.json') if os.path.isfile(out + '/pou.json') else {}
    cmp_ = compare(ctx)
    L.wjson(out + '/comparison.json', cmp_)
    custody(cfg, ctx, cmp_)
    return cmp_


def custody(cfg, ctx, cmp_):
    out = cfg.out
    cdir = out + '/custody'
    os.makedirs(cdir, exist_ok=True)
    events = parse_events(ctx['Events']) if ctx.get('Events') else []
    R = _load('reconstruct_runs_m7')
    items = command_items(events)
    ex = []
    for k, it in enumerate(items):
        inner, shell = inner_of(it.get('command') or '', R)
        ex.append({'Invocation': 'C%d' % (k + 1), 'ItemId': it.get('id'), 'ExitCode': it.get('exit_code'), 'Status': it.get('status'),
                   'Shell': L.sanitize_str(shell or ''), 'InstrumentExecution': L.B in (it.get('command') or ''),
                   'CommandSha256': L.sha256((it.get('command') or '').encode('utf-8')),
                   'OutputSha256': L.sha256((it.get('aggregated_output') or '').encode('utf-8')),
                   'OutputBytes': len((it.get('aggregated_output') or '').encode('utf-8'))})
    usage = [e.get('usage') for e in events if e.get('type') == 'turn.completed']
    L.wjson(cdir + '/events-extract.json', {'EventsSha256': L.sha256(ctx['Events']) if ctx.get('Events') else None,
                                            'Events': len(events), 'CommandExecutions': ex, 'Usage': usage})
    se = dict(ctx.get('Session') or {})
    se.pop('Name', None)
    L.wjson(cdir + '/session-extract.json', se)
    if ctx.get('Output') is not None:
        try:
            L.wjson(cdir + '/output.sanitized.json', strict_json(ctx['Output']))
        except Exception:
            pass
    man = {}
    for r, ds, fs in os.walk(out):
        for f in sorted(fs):
            q = os.path.join(r, f)
            rel = os.path.relpath(q, out).replace(os.sep, '/')
            if rel == 'custody/manifest.json':
                continue
            man[rel] = {'Bytes': os.path.getsize(q), 'Sha256': L.sha256_file(q),
                        'Custody': 'hash only (local, unsanitized)' if rel.startswith(('raw/', 'env/')) else 'file'}
    L.wjson(cdir + '/manifest.json', {'Schema': 'i62-a6m7-custody/1', 'UtcTime': L.utc(), 'Verdict': cmp_.get('Verdict'), 'Files': man,
                                      'Note': 'events.jsonl, the session record, output.json raw, the envelopes and the armed prompt are '
                                              'custodied by SHA-256 only; their extracts and sanitized copies are in custody/'})


# ------------------------------------------------------------------ build steps (called by build_m7.py)
def build_step(cmd):
    if cmd == 'oracle':
        o = oracle(L.CDIR)
        data = (json.dumps(L.sanitize(o), ensure_ascii=True, indent=1, sort_keys=True) + '\n').encode('ascii')
        if not L.hygienic(data.decode('ascii')):
            raise SystemExit('oracle hygiene')
        L.wbytes(ORACLE, data)
        print(json.dumps({'Oracle': ORACLE, 'Sha256': L.sha256(data), 'Eff': o['ClauseMap']['Eff'], 'Candidates': len(o['ClauseMap']['Candidates']),
                          'Terms': o['FreeText']['Terms'], 'DiffPaths': o['Diff']['Paths'], 'Commits': o['Diff']['Commits']}, indent=1))
        return 0
    if cmd == 'schema':
        for p, strict in ((SCHEMA_FULL, False), (SCHEMA_STRICT, True)):
            L.wbytes(p, (json.dumps(build_schema(strict), ensure_ascii=True, indent=1) + '\n').encode('ascii'))
            print(p, L.sha256_file(p))
        return 0
    if cmd == 'prompt':
        d = declared()
        t = arm_prompt(d['Git'], d['Py'])
        data = t.encode('utf-8')
        L.wbytes(L.FROZEN + '/prompt.m7.armed.sanitized.txt', L.sanitize_str(t).encode('utf-8'))
        print(json.dumps({'ArmedPromptSha256': L.sha256(data), 'Chars': len(t), 'Bytes': len(data)}))
        return 0
    if cmd == 'markers':
        return write_markers()
    if cmd == 'freeze':
        return freeze()
    return 64


def markers_values():
    p = P
    t = p['Trio']
    return [
        'M7-CELDA: codex-cli / gpt-6-luna / high; EXECUTION_CONTROLLER de FX-02 en F6',
        'M7-TRIO: BinaryHash %s, AppVersion %s, huella %s' % (t['BinaryHash'], t['AppVersion'], t['Fingerprint']),
        'M7-INSTRUMENTO: blob %s; orden declarada <git> --no-replace-objects cat-file blob %s | <interprete> -I - <orden> ...' % (L.B, L.B),
        'M7-INTERPRETE: %s; %s; SHA-256 %s' % (p['Interpreter']['Path'], p['Interpreter']['Version'], p['Interpreter']['Sha256']),
        'M7-GIT: %s; %s; SHA-256 %s' % (p['Git']['Path'], p['Git']['Version'], p['Git']['Sha256']),
        'M7-C: %s; instantanea SHA-256 %s' % (p['RepoWin'], L.sha256_file(L.FROZEN + '/m7-run.snapshot.json')),
        'M7-CIERRE: R7-A; E SHA-256 %s; cierre derivado SHA-256 %s; registro de derivacion SHA-256 %s' % (
            L.sha256_file(L.FROZEN + '/E.json'), L.sha256_file(L.FROZEN + '/closure.json'), L.sha256_file(L.FROZEN + '/derivation.json')),
        'M7-RECONSTRUCCION: %s SHA-256 %s' % (p['ReconstructionRule'], L.sha256_file(L.KIT + '/' + p['ReconstructionRule'])),
        'M7-ESCRITURA: autorizada la escritura de preparacion de A6-3, regla 1, en %s; DecisionRef %s' % (p['RepoWin'], L.DECISION_REF),
    ]


def write_markers():
    m = markers_values()
    L.wbytes(L.KIT + '/disposition-markers.json', (json.dumps({'Schema': 'i62-a6m7-disposition-markers/1', 'Markers': m,
                                                               'Computed': ['M7-KIT: FREEZE.json SHA-256 <SHA-256 de FREEZE.json>'],
                                                               'Variable': ['M7-LECTURAS: disposicion de A6-5, regla 6: §<N>']},
                                                              ensure_ascii=True, indent=1) + '\n').encode('ascii'))
    print('\n'.join(m))
    return 0


def freeze():
    G = _load('gates_m7')
    files = G.kit_files(L.KIT)
    fr = {'Schema': 'i62-a6m7-freeze/1', 'FrozenUtc': L.utc(),
          'Authority': 'I-62, A-6 AGREED (blob 4bc919dd), decisiones §90 punto 3: preparacion de la medicion A6-7; no lanza, no autoriza consumo',
          'Files': dict((f, {'Bytes': os.path.getsize(os.path.join(L.KIT, f)), 'Sha256': L.sha256_file(os.path.join(L.KIT, f))}) for f in files)}
    d = declared()
    t = arm_prompt(d['Git'], d['Py'])
    fr['ArmedPrompt'] = {'Sha256': L.sha256(t.encode('utf-8')), 'Chars': len(t), 'SanitizedCopy': 'frozen/prompt.m7.armed.sanitized.txt'}
    fr['Snapshot'] = {'Path': 'frozen/m7-run.snapshot.json', 'Sha256': L.sha256_file(L.FROZEN + '/m7-run.snapshot.json')}
    fr['Oracle'] = {'Path': 'frozen/oracle.json', 'Sha256': L.sha256_file(ORACLE)}
    fr['E'] = {'Path': 'frozen/E.json', 'Sha256': L.sha256_file(L.FROZEN + '/E.json')}
    fr['Closure'] = {'Path': 'frozen/closure.json', 'Sha256': L.sha256_file(L.FROZEN + '/closure.json')}
    fr['DerivationRecord'] = {'Path': 'frozen/derivation.json', 'Sha256': L.sha256_file(L.FROZEN + '/derivation.json')}
    fr['ReconstructionRule'] = {'Path': P['ReconstructionRule'], 'Sha256': L.sha256_file(L.KIT + '/' + P['ReconstructionRule'])}
    fr['ReferenceReconstructionRule'] = {'Path': P['ReferenceReconstructionRule'], 'Sha256': L.sha256_file(L.KIT + '/' + P['ReferenceReconstructionRule'])}
    L.wbytes(L.KIT + '/FREEZE.json', (json.dumps(fr, ensure_ascii=True, indent=1, sort_keys=True) + '\n').encode('ascii'))
    print(json.dumps({'Files': len(files), 'FreezeSha256': L.sha256_file(L.KIT + '/FREEZE.json')}))
    return 0


def main(argv):
    G = _load('gates_m7')
    fixture = argv[argv.index('--fixture') + 1] if '--fixture' in argv else None
    cfg = G.Config.fixture(fixture, 'dry-run') if fixture else G.Config()
    if len(argv) > 1 and argv[1] == 'post':
        r = post(cfg)
        print('POST verdict: %s; causes: %s' % (r['Verdict'], ', '.join(r['Causes'])))
        return 0
    sys.stderr.write('usage: python -I tools_m7.py post [--fixture DIR]\n')
    return 64


if __name__ == '__main__':
    sys.exit(main(sys.argv))

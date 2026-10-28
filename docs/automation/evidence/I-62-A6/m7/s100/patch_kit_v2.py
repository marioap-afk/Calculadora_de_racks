"""Kit de la medición A6-7, versión 2 (I-62; decisiones §100, puntos 3 y 4): ediciones exactas sobre los componentes del kit congelado v1
(D:\\r62-a6m7\\kit = FREEZE 459a3440; cada SHA-256 de partida se comprueba contra ese FREEZE) y escritura en D:\\r62-a6m7\\kit2. Cada texto
antiguo debe aparecer exactamente las veces declaradas. Cambios: orden declarada sin `contract` (D-1) y dos lecturas read-json --file de
CONTRACT y DELEG con los ocho punteros de Contract (13 ejecuciones del instrumento, 11 órdenes); RunId con la forma de AP 16.6; regla de
reconstrucción v3 (conjunto declarado; cabecera con la sentencia retirada, F8 del preflight de A-7); cierre A7-1 en lugar de R7-A
(build_m7.py v2 aparte, s100/build_m7.v2.py; table_m7 con designate_a7); trío vigente de Codex y su línea base de nombres; disposición
posterior a §100; esquema rackcad-a6m7-measurement/2. Uso: python -I -B patch_kit_v2.py
"""
import hashlib, json, os, shutil

V1, V2, S = r'D:\r62-a6m7\kit', r'D:\r62-a6m7\kit2', r'D:\r62-a6m7\s100'
RUN_ID = 'R20261028T024440Z-27a9'
TRIO = {'BinaryHash': '5c1b55966bf9f757dcbf00065c9a6cb862ead55d34b015f1be1939ff18e43943', 'AppVersion': '26.1023.6123.0',
        'Fingerprint': '0A9F6A344C965BE17650A2559601DB8768AE8DCFAC6BEB7DEEFC615AE4E7F550'}
LABEL, COMPANIONS = '08d187884f2330d1', '4c99510126c2ea89'
BASELINE = 'baseline-keynames-0A9F6A34.json'
F1 = json.load(open(os.path.join(V1, 'FREEZE.json'), encoding='utf-8'))['Files']
sha = lambda b: hashlib.sha256(b).hexdigest()
P = {}


def add(f, old, new, count=1):
    P.setdefault(f, []).append((old, new, count))


# ---------------------------------------------------------------- lib_kit.py
add('lib_kit.py', '# lib_kit.py - shared helpers of the A6-7 measurement kit (I-62, A-6 AGREED, decisiones §90). Stdlib only.\n'
    '# Constants of the measurement M (designation a6m7-designation/1 of the bench, I.TaskId = T1-m7), sanitizing, canonical JSON,',
    '# lib_kit.py - shared helpers of the A6-7 measurement kit v2 (I-62, A-6 and A-7 AGREED, decisiones §90 and §100). Stdlib only.\n'
    '# Constants of the measurement M (designation a7-designation/4, Kind MEASUREMENT, I.TaskId = T1-m7), sanitizing, canonical JSON,')
add('lib_kit.py', r'# Never writes outside D:\r62-a6m7\kit and D:\r62-a6m7\m7-run (', r'# Never writes outside the kit directory and its -C D:\r62-a6m7\m7-run2 (')
# kit v2 has its own -C (the TRX copy lives under the RunId directory, and the v1 -C, snapshot 43a8caf6, stays untouched)
add('lib_kit.py', "CDIR = 'D:/r62-a6m7/m7-run'", "CDIR = 'D:/r62-a6m7/m7-run2'                       # kit v2: own -C (v1: D:/r62-a6m7/m7-run, untouched)")
add('lib_kit.py', r"CDIR_WIN = 'D:\\r62-a6m7\\m7-run'", r"CDIR_WIN = 'D:\\r62-a6m7\\m7-run2'")
add('prep_clone.py', r"# prep_clone.py - creates the -C of the A6-7 measurement (I-62), D:\r62-a6m7\m7-run, deterministically",
    r"# prep_clone.py - creates the -C of the A6-7 measurement (I-62), lib_kit.CDIR (kit v2: D:\r62-a6m7\m7-run2), deterministically")
add('selftest_m7.py', r"# measurement -C (D:\r62-a6m7\m7-run) is only read", r"# measurement -C (lib_kit.CDIR, kit v2: D:\r62-a6m7\m7-run2) is only read")
add('lib_kit.py', "RUN_ID = 'R20261016T000000Z-m7m0'                    # RunId of M (bench designation M, unchanged)",
    "RUN_ID = '%s'                    # RunId of M (kit v2: the AP 16.6 form R<UTC>Z-<4 hex>; decisiones §92, punto 6)" % RUN_ID)
add('lib_kit.py', "ORDERS = ['identity', 'diff', 'scope', 'contract', 'hashes', 'cleantree', 'handoff', 'freetext', 'clausemap', 'trx',\n"
    "          'read-text', 'read-json']",
    "ORDERS = ['identity', 'diff', 'scope', 'hashes', 'cleantree', 'handoff', 'freetext', 'clausemap', 'trx', 'read-text', 'read-json']\n"
    "# kit v2: ORDERS is the DECLARED order set, the B-min orders without `contract` (D-1; decisiones §100, punto 3). A run whose Argv[0] is\n"
    "# outside it is not reconstructible (reconstruct_runs_m7.py v3). The instrument executions of the measurement are the 11 orders plus two\n"
    "# read-json --file reads of CONTRACT and DELEG with the eight pointers of Contract (how the Controller keeps verifying Contract, with U-19,\n"
    "# without `contract`).\n"
    "CONTRACT_POINTERS = ['/AllowedWriteScope', '/ForbiddenWriteScope', '/Invariants', '/StopConditions', '/Authorities', '/RequiredTests',\n"
    "                     '/RoleRequirements', '/SupersededCommits']\n"
    "EXEC_LABELS = ORDERS + ['read-json-contract', 'read-json-deleg']\n"
    "EXEC_ORDER = dict([(n, n) for n in ORDERS] + [('read-json-contract', 'read-json'), ('read-json-deleg', 'read-json')])")
add('lib_kit.py', "def deliv_path(run, name):",
    "def exec_label(argv):\n"
    "    \"\"\"Execution label of an instrument argv: its order, or read-json-contract / read-json-deleg for the two read-json --file reads.\"\"\"\n"
    "    a = list(argv or [])\n"
    "    if a[:1] == ['read-json'] and '--file' in a and a.index('--file') + 1 < len(a):\n"
    "        return 'read-json-' + a[a.index('--file') + 1].split('=', 1)[0].lower()\n"
    "    return a[0] if a else None\n\n\n"
    "def deliv_path(run, name):")

# ---------------------------------------------------------------- tools_m7.py
add('tools_m7.py', "        'contract': ['contract', '--repo', T] + fa('CONTRACT', k) + fa('DELEG', d),\n", '')
add('tools_m7.py', "        'read-json': ['read-json', '--repo', T, '--rev', L.G, '--path', L.K] + sum([['--pointer', x] for x in L.JSON_POINTERS], []),\n"
    "    }\n    return [(n, o[n]) for n in L.ORDERS]",
    "        'read-json': ['read-json', '--repo', T, '--rev', L.G, '--path', L.K] + sum([['--pointer', x] for x in L.JSON_POINTERS], []),\n"
    "        'read-json-contract': ['read-json', '--repo', T] + fa('CONTRACT', k) + sum([['--pointer', x] for x in L.CONTRACT_POINTERS], []),\n"
    "        'read-json-deleg': ['read-json', '--repo', T] + fa('DELEG', d) + sum([['--pointer', x] for x in L.CONTRACT_POINTERS], []),\n"
    "    }\n    return [(n, o[n]) for n in L.EXEC_LABELS]")
add('tools_m7.py', "    ('P18', 'B', 'contract', ", "    ('P18', 'B', 'read-json-contract', ")
add('tools_m7.py', "    orders = ['C4 a C15 (instrumento de hechos; una orden por ejecucion, copiada literalmente):', '']",
    "    orders = ['C4 a C16 (instrumento de hechos; una orden por ejecucion, copiada literalmente):', '']")
add('tools_m7.py', "    if t.count(L.B) != 12:\n        raise SystemExit('the full blob id must appear exactly in the twelve orders')",
    "    if t.count(L.B) != len(L.EXEC_LABELS):\n        raise SystemExit('the full blob id must appear exactly in the thirteen instrument executions')")
add('tools_m7.py', "'rackcad-a6m7-measurement/1'", "'rackcad-a6m7-measurement/2'", 2)
add('tools_m7.py', "                       'FreeTextMatchCount': I, 'ContractA5MissingJson': J, 'ContractSupersededEqual': B}),",
    "                       'FreeTextMatchCount': I,\n"
    "                       'ContractScopeJson': _obj(dict((p[1:], J) for p in L.CONTRACT_POINTERS)),\n"
    "                       'DelegScopeJson': _obj(dict((p[1:], J) for p in L.CONTRACT_POINTERS))}),")
add('tools_m7.py', "            envs.setdefault(r['Argv'][0], json.loads(", "            envs.setdefault(L.exec_label(r['Argv']), json.loads(")
add('tools_m7.py', "    runs_link = len(out_runs) == 12 and [r.get('Order') for r in out_runs] == list(L.ORDERS)",
    "    runs_link = len(out_runs) == len(L.EXEC_LABELS) and [r.get('Order') for r in out_runs] == [L.EXEC_ORDER[x] for x in L.EXEC_LABELS]")
add('tools_m7.py', "        ('each of the 12 orders executed at least once', all(per_order[n] for n in L.ORDERS), [n for n in L.ORDERS if not per_order[n]]),",
    "        ('each of the 11 declared orders executed at least once, and the two read-json --file reads (CONTRACT, DELEG)',\n"
    "         all(per_order[n] for n in L.ORDERS) and all(x in envs for x in ('read-json-contract', 'read-json-deleg')),\n"
    "         [n for n in L.ORDERS if not per_order[n]] + [x for x in ('read-json-contract', 'read-json-deleg') if x not in envs]),")
add('tools_m7.py', "        ('output Runs: 12 rows in order, linked to the recorded executions (exit code, FactsSha256)', runs_link),",
    "        ('output Runs: 13 rows in order (the 13 instrument executions), linked to the recorded executions (exit code, FactsSha256)', runs_link),")
add('tools_m7.py', "        ('contract envelope A5Missing and SupersededEqual = oracle', F('contract').get('A5Missing') == O['Contract']['A5Missing']\n"
    "         and F('contract').get('SupersededEqual') == O['Contract']['SupersededEqual']),\n"
    "        ('output ContractA5MissingJson (ASCII JSON) and ContractSupersededEqual = oracle', jok(rd.get('ContractA5MissingJson'), O['Contract']['A5Missing'])\n"
    "         and rd.get('ContractSupersededEqual') == O['Contract']['SupersededEqual']),",
    "        ('read-json --file CONTRACT and DELEG envelopes: Values of the eight Contract pointers = the custodied bytes',\n"
    "         (F('read-json-contract').get('Values') or {}) == O['ContractScope'] and (F('read-json-deleg').get('Values') or {}) == O['DelegScope']),\n"
    "        ('output ContractScopeJson and DelegScopeJson: ASCII JSON text equal to the custodied values',\n"
    "         all(jok((rd.get('ContractScopeJson') or {}).get(p[1:]), O['ContractScope'][p]) and jok((rd.get('DelegScopeJson') or {}).get(p[1:]), O['DelegScope'][p])\n"
    "             for p in L.CONTRACT_POINTERS)),")
add('tools_m7.py', "        'M7-INSTRUMENTO: blob %s; orden declarada <git> --no-replace-objects cat-file blob %s | <interprete> -I - <orden> ...' % (L.B, L.B),",
    "        'M7-INSTRUMENTO: blob %s; orden declarada <git> --no-replace-objects cat-file blob %s | <interprete> -I - <orden> ...; <orden> una de: %s; sin contract' % (\n"
    "            L.B, L.B, ', '.join(L.ORDERS)),")
add('tools_m7.py', "        'M7-CIERRE: R7-A; E SHA-256 %s; cierre derivado SHA-256 %s; registro de derivacion SHA-256 %s' % (\n"
    "            L.sha256_file(L.FROZEN + '/E.json'), L.sha256_file(L.FROZEN + '/closure.json'), L.sha256_file(L.FROZEN + '/derivation.json')),",
    "        'M7-CIERRE: A7-1 (a7-designation/4, Kind MEASUREMENT); designacion SHA-256 %s; E SHA-256 %s; cierre derivado SHA-256 %s; registro de derivacion SHA-256 %s' % (\n"
    "            L.sha256_file(L.FROZEN + '/designation-M.json'), L.sha256_file(L.FROZEN + '/E.json'), L.sha256_file(L.FROZEN + '/closure.json'),\n"
    "            L.sha256_file(L.FROZEN + '/derivation.json')),")
add('tools_m7.py', "          'Authority': 'I-62, A-6 AGREED (blob 4bc919dd), decisiones §90 punto 3: preparacion de la medicion A6-7; no lanza, no autoriza consumo',",
    "          'Authority': 'I-62, A-6 AGREED (blob 4bc919dd) y A-7 AGREED (blob ebb3a87b), decisiones §100, puntos 3 y 4: kit v2 de la medicion A6-7 '\n"
    "                       '(actualizacion incremental del kit 459a3440); no lanza, no autoriza consumo',")
add('tools_m7.py', "    fr['E'] = {'Path': 'frozen/E.json', 'Sha256': L.sha256_file(L.FROZEN + '/E.json')}",
    "    fr['Designation'] = {'Path': 'frozen/designation-M.json', 'Sha256': L.sha256_file(L.FROZEN + '/designation-M.json')}\n"
    "    fr['ClosureValidation'] = {'Path': 'frozen/closure-validation.json', 'Sha256': L.sha256_file(L.FROZEN + '/closure-validation.json')}\n"
    "    fr['E'] = {'Path': 'frozen/E.json', 'Sha256': L.sha256_file(L.FROZEN + '/E.json')}")

SRC_TOOLS = open(os.path.join(V1, 'tools_m7.py'), encoding='utf-8').read()
_a = SRC_TOOLS.index("    sid = lambda x, key: x.get(key)\n")
_b = SRC_TOOLS.index("    # clause map: EFF by the 16.14 derivation")
assert SRC_TOOLS.count("    sid = lambda x, key: x.get(key)\n") == 1 and "o['Contract'] = {'A5Missing': a5" in SRC_TOOLS[_a:_b]
add('tools_m7.py', SRC_TOOLS[_a:_b],
    "    # kit v2 (D-1): no `contract` order; the oracle of the two read-json --file reads is the values of the eight Contract pointers in\n"
    "    # the bytes that each --file names (the contract file of the -C tree and the custodied delegation)\n"
    "    kfile = json.loads(L.rbytes(os.path.join(T, *L.K.split('/'))).decode('utf-8'))\n"
    "    o['ContractScope'] = dict((ptr, kfile[ptr[1:]]) for ptr in L.CONTRACT_POINTERS)\n"
    "    o['DelegScope'] = dict((ptr, deleg[ptr[1:]]) for ptr in L.CONTRACT_POINTERS)\n")

add('tools_m7.py', "        ('Runs 12 rows, Primary 24 rows, AuthorityResolution non-empty', len(out_runs) == 12 and len(prim_rows) == 24 and bool(ar)),",
    "        ('Runs 13 rows, Primary 24 rows, AuthorityResolution non-empty', len(out_runs) == len(L.EXEC_LABELS) and len(prim_rows) == 24 and bool(ar)),")

# ---------------------------------------------------------------- reconstruct_runs_m7.py (v3)
add('reconstruct_runs_m7.py', "# including every completeness, envelope and exit rule, is the reference unchanged.\n",
    "# including every completeness, envelope and exit rule, is the reference unchanged.\n"
    "# v3 (kit v2 of the measurement, decisiones §100, punto 3; D-1): the order of every instrument execution must be one of the DECLARED\n"
    "# set (identity, diff, scope, hashes, cleantree, read-text, read-json, trx, handoff, freetext, clausemap): `contract` is no longer in\n"
    "# TOOL_COMMANDS, so a recorded execution of `contract` makes the log NOT reconstructible (no request). Also declared (preflight of\n"
    "# A-7, F8, case RECON-HEADER): v2 removed from the reference one statement without effect, `if env.get('Command') != p['Argv'][0]:\n"
    "# pass`; nothing else differs from what this header declares.\n")
add('reconstruct_runs_m7.py', "RULE = 'reconstruct_runs_m7/2'", "RULE = 'reconstruct_runs_m7/3'")
add('reconstruct_runs_m7.py', "TOOL_COMMANDS = ('identity', 'diff', 'scope', 'contract', 'hashes', 'cleantree', 'read-text', 'read-json', 'trx',\n"
    "                 'handoff', 'freetext', 'clausemap')",
    "TOOL_COMMANDS = ('identity', 'diff', 'scope', 'hashes', 'cleantree', 'read-text', 'read-json', 'trx',\n"
    "                 'handoff', 'freetext', 'clausemap')   # v3: the declared set, without `contract`")

# ---------------------------------------------------------------- table_m7.py (designation check under A7-1)
add('table_m7.py', "# table_m7.py - request construction and acceptance table of the A6-7 measurement M (I-62, A-6, decisiones §90), and the\n"
    "# closure cotejo with the designation check (designate_m7.py) for every request of this bench. Stdlib only; never writes.",
    "# table_m7.py - request construction and acceptance table of the A6-7 measurement M (I-62, A-6, decisiones §90), and the\n"
    "# closure cotejo with the designation check for every request of this bench. Stdlib only; never writes. Kit v2 (decisiones §100):\n"
    "# the designation check is designate_a7.py (A7-1, a7-designation/4) instead of designate_m7.py; nothing else changes.")
add('table_m7.py', "DS = _load('designate_m7')", "DS = _load('designate_a7')")

# ---------------------------------------------------------------- gates_m7.py (line base of key names from the parameters)
add('gates_m7.py', "        self.baseline = L.KIT + '/baseline-keynames-F909F480.json'", "        self.baseline = L.KIT + '/' + p['Codex']['KeyNamesBaseline']")

# ---------------------------------------------------------------- selftest_m7.py
add('selftest_m7.py', "             'designate_m7.py': '491dcbd672518befa3cf74a64c7a9971992c66eee5bd122b3ae537a184f5a5c6',\n", '')
add('selftest_m7.py', "             'table_m7.py': 'a0765733be3e140f01afe1818d3a1ff630e83cb1970acba22e593f322574c1e3',\n",
    "             'make_eic_a7.py': 'addf51b2838dfaaa4a33c77f64fc6e5686207a81ee30485800e468e9954cf95c',\n"
    "             'designate_a7.py': '7ea39a5401f4d7345da55b728785c1a4b9cab0748b952c43ae3357f4321167d3',\n"
    "             'check_a7_closure.py': '2d6a891b1321b29cdef6fd6caac2fc42f39699408864456c6fef3f18c7b22080',\n"
    "             'a7-designation-4.schema.json': '0d8effb2caf9e64992ed2b025cbb6db593b3dd383600ce18d976119bfdb79dcc',\n"
    "             'check_citation_blobs.py': 'e99ea7105d82a4262e6998ff767f08341bdae92c24fb31560393f8283e69f737',\n")
add('selftest_m7.py', "             'baseline-keynames-F909F480.json': '076637c550ff0b3109447ef00263ff087749f0aaa71f299d524e43b8cf82b70b',\n"
    "             'line-od2.txt': '3b33dd56cbae5fddab9a9431e7ad9b83170e6782e0161385bc324789634b7f25',\n"
    "             'line-b.txt': 'dda4b3f9c1db0183a38e5400777a2a21d524aa4b92ad684d5c5cae1b2fc4cf27'}", "             }")
add('selftest_m7.py', "    case('A', 'PROMPT', 'armed prompt: ASCII, no double quote, the full blob only in the 12 orders,",
    "    case('A', 'PROMPT', 'armed prompt: ASCII, no double quote, the full blob only in the 13 instrument executions,")
add('selftest_m7.py', "text.count(L.B) == 12 and L.B12 in text", "text.count(L.B) == len(L.EXEC_LABELS) and L.B12 in text")
add('selftest_m7.py', "(the comparator then reports the 12 orders as not executed)", "(the comparator then reports the declared orders as not executed)")
add('selftest_m7.py', "def fixture_decisions(path, drop=(), alter=None, owner=('line-od2.txt', 'line-b.txt'), owner_before=False, lect='§92', lect_section=True,",
    "def fixture_decisions(path, drop=(), alter=None, owner=('line-od2.txt', 'line-b.txt'), owner_before=False, lect='§102', lect_section=True,")
add('selftest_m7.py', "        lines += ['## 90. Disposicion del Coordinator: A-6 AGREED (fixture)', '', 'texto', '']",
    "        lines += ['## 100. Disposicion del Coordinator: A-7 AGREED (fixture)', '', 'texto', '']")
add('selftest_m7.py', "    body = ['## 91. Disposicion del Coordinator: medicion A6-7 del Controller de FX-02 (fixture)', '']",
    "    body = ['## 101. Disposicion del Coordinator: medicion A6-7 del Controller de FX-02 (fixture)', '']")
add('selftest_m7.py', "        lines += [body[0].replace('## 91.', '## 93.')] + body[1:] + ['']", "        lines += [body[0].replace('## 101.', '## 103.')] + body[1:] + ['']")
add('selftest_m7.py', "        lines += ['## 92. Disposicion del Coordinator: lecturas normativas del aceptante (A6-5, regla 6) (fixture)', '', 'texto', '']",
    "        lines += ['## 102. Disposicion del Coordinator: lecturas normativas del aceptante (A6-5, regla 6) (fixture)', '', 'texto', '']")
add('selftest_m7.py', "    dec_case('lecturas-section-absent', lect='§97')", "    dec_case('lecturas-section-absent', lect='§107')")
add('selftest_m7.py', "    dec_case('od2-altered', owner_mut=lambda n, t: t.replace('6EFA5B0E', '6EFA5B0F') if n == 'line-od2.txt' else t)",
    "    dec_case('od2-altered', owner_mut=lambda n, t: t.replace('0A9F6A34', '0A9F6A35') if n == 'line-od2.txt' else t)")
add('selftest_m7.py', "# ------------------------------------------------------------------ S. synthetic session: the 17 executions really run in pwsh 7",
    "# ------------------------------------------------------------------ S. synthetic session: the 18 executions really run in pwsh 7")
add('selftest_m7.py', '    """C1..C17 exactly as the prompt orders them,', '    """C1..C18 exactly as the prompt orders them,')
add('selftest_m7.py', "    scripts.append(('C16', T7.primary_script('A', T)))", "    scripts.append(('C17', T7.primary_script('A', T)))")
add('selftest_m7.py', "    execs.append({'Label': 'C17', 'Script': sB, 'Rc': p.returncode, 'Stdout': p.stdout, 'Stderr': p.stderr})",
    "    execs.append({'Label': 'C18', 'Script': sB, 'Rc': p.returncode, 'Stdout': p.stdout, 'Stderr': p.stderr})")
add('selftest_m7.py', "    segA = T7.segments(by['C16']['Stdout'].decode('utf-8', 'replace'))\n    segB = T7.segments(by['C17']['Stdout'].decode('utf-8', 'replace'))",
    "    segA = T7.segments(by['C17']['Stdout'].decode('utf-8', 'replace'))\n    segB = T7.segments(by['C18']['Stdout'].decode('utf-8', 'replace'))")
add('selftest_m7.py', "        prim.append({'Id': pid, 'Invocation': 'C16' if blk == 'A' else 'C17',", "        prim.append({'Id': pid, 'Invocation': 'C17' if blk == 'A' else 'C18',")
add('selftest_m7.py', "    o = {'Schema': 'rackcad-a6m7-measurement/1',", "    o = {'Schema': 'rackcad-a6m7-measurement/2',")
add('selftest_m7.py', "         'Runs': [{'Order': n, 'Invocation': c('I:' + n), 'ExitCode': by['I:' + n]['Rc'], 'FactsSha256': env[n]['FactsSha256'],\n"
    "                   'ErrorCode': None} for n in L.ORDERS],",
    "         'Runs': [{'Order': L.EXEC_ORDER[n], 'Invocation': c('I:' + n), 'ExitCode': by['I:' + n]['Rc'], 'FactsSha256': env[n]['FactsSha256'],\n"
    "                   'ErrorCode': None} for n in L.EXEC_LABELS],")
add('selftest_m7.py', "                   'FreeTextMatchCount': len(F('freetext')['Matches']), 'ContractA5MissingJson': jd(F('contract')['A5Missing']),\n"
    "                   'ContractSupersededEqual': F('contract')['SupersededEqual']},",
    "                   'FreeTextMatchCount': len(F('freetext')['Matches']),\n"
    "                   'ContractScopeJson': dict((p[1:], jd(F('read-json-contract')['Values'][p])) for p in L.CONTRACT_POINTERS),\n"
    "                   'DelegScopeJson': dict((p[1:], jd(F('read-json-deleg')['Values'][p])) for p in L.CONTRACT_POINTERS)},")
add('selftest_m7.py', "    case('S', 'SYNTH-RECON', 'the synthetic Codex-format log of the 17 executions reconstructs with v2: 12 instrument executions, rendering R1, '",
    "    case('S', 'SYNTH-RECON', 'the synthetic Codex-format log of the 18 executions reconstructs with v3: 13 instrument executions, rendering R1, '")
add('selftest_m7.py', "rcn['InstrumentExecutions'] == 12 and", "rcn['InstrumentExecutions'] == len(L.EXEC_LABELS) and")
add('selftest_m7.py', "    stubdir = ST + '/stubbin/e78bad9887fa50cf'", "    stubdir = ST + '/stubbin/' + P['Codex']['Label']")
# group B under v3: the historical logs carry executions of `contract` (12 orders of kit v1), which v3 rejects by design; v3 is compared
# with the reference on each log without those executions, and a new case checks that every log with them is not reconstructible
add('selftest_m7.py', "def group_recon():\n",
    "def nocontract(b):\n"
    "    \"\"\"kit v2: the log without its executions of `contract` (both events of each such item; lines kept byte for byte), so that the\n"
    "    reconstruction rule v3 (declared order set) is compared with the reference on the rest of the log.\"\"\"\n"
    "    lines = b.split(b'\\n')\n"
    "    drop = set()\n"
    "    for line in lines:\n"
    "        try:\n"
    "            it = json.loads(line.decode('utf-8')).get('item')\n"
    "        except Exception:\n"
    "            continue\n"
    "        if isinstance(it, dict) and it.get('type') == 'command_execution' and L.B in (it.get('command') or '') \\\n"
    "                and ' -I - contract ' in (it.get('command') or ''):\n"
    "            drop.add(it.get('id'))\n"
    "    if not drop:\n"
    "        return b\n"
    "    keep = []\n"
    "    for line in lines:\n"
    "        try:\n"
    "            it = json.loads(line.decode('utf-8')).get('item')\n"
    "        except Exception:\n"
    "            it = None\n"
    "        if isinstance(it, dict) and it.get('id') in drop:\n"
    "            continue\n"
    "        keep.append(line)\n"
    "    return b'\\n'.join(keep)\n\n\n"
    "def group_recon():\n")
add('selftest_m7.py', "    for f in files:\n        b = L.rbytes(f)\n        a, c = rbrief(R1.reconstruct(b, L.B)), rbrief(R2.reconstruct(b, L.B))",
    "    rejected = []\n"
    "    for f in files:\n        b0 = L.rbytes(f)\n        b = nocontract(b0)\n"
    "        if b != b0:\n"
    "            rejected.append((os.path.basename(f), rbrief(R2.reconstruct(b0, L.B))['R'], rbrief(R2.reconstruct(b0, L.B))['P'][:1]))\n"
    "        a, c = rbrief(R1.reconstruct(b, L.B)), rbrief(R2.reconstruct(b, L.B))")
add('selftest_m7.py', "    case('B', 'V2-EQ-REFERENCE', 'every r3/r4/bench/custodied log: same reconstruction as the reference (Reconstructible, Runs, Problems), except '",
    "    case('B', 'V2-EQ-REFERENCE', 'every r3/r4/bench/custodied log, without its executions of contract (kit v2): same reconstruction as the reference (Reconstructible, Runs, Problems), except '")
add('selftest_m7.py', "    # F-outer-flags under v2 = the OK log runs (argv and envelopes)\n    b = L.rbytes('D:/r62-a6r3/r1/s-recon/F-outer-flags-events.jsonl')",
    "    case('B', 'V3-CONTRACT-NOT-RECONSTRUCTIBLE', 'kit v2 (D-1): every log with an execution of contract is NOT reconstructible under v3 (declared '\n"
    "         'order set: inner: unknown tool command), whatever the reference gives; without those executions v3 = the reference (V2-EQ-REFERENCE)',\n"
    "         {'LogsWithContract': len(rejected), 'V3Reconstructible': [x[0] for x in rejected if x[1]], 'Problems': sorted(set(str(x[2]) for x in rejected))[:4]},\n"
    "         bool(rejected) and not any(x[1] for x in rejected))\n"
    "    # F-outer-flags under v2 = the OK log runs (argv and envelopes)\n    b = nocontract(L.rbytes('D:/r62-a6r3/r1/s-recon/F-outer-flags-events.jsonl'))")
add('selftest_m7.py', "    okb = L.rbytes('D:/r62-a6r3/r1/s-recon/OK-events.jsonl')", "    okb = nocontract(L.rbytes('D:/r62-a6r3/r1/s-recon/OK-events.jsonl'))")

# ---------------------------------------------------------------- apply
os.makedirs(V2, exist_ok=True)
rep = {}
for f, edits in P.items():
    b = open(os.path.join(V1, f), 'rb').read()
    assert sha(b) == F1[f]['Sha256'], ('v1 changed', f)
    t = b.decode('utf-8')
    for i, (old, new, count) in enumerate(edits):
        n = t.count(old)
        if n != count:
            raise SystemExit('%s, edit %d: %d occurrences (expected %d): %r' % (f, i, n, count, old[:100]))
        t = t.replace(old, new)
    nb = t.encode('utf-8')
    assert b'\r' not in nb
    open(os.path.join(V2, f), 'wb').write(nb)
    rep[f] = {'Edits': len(edits), 'Sha256': sha(nb)}
# whole-file replacements
for dst, src in (('prompt.m7.template.txt', 'prompt.m7.template.v2.txt'), ('build_m7.py', 'build_m7.v2.py')):
    b = open(os.path.join(S, src), 'rb').read()
    assert b'\r' not in b
    open(os.path.join(V2, dst), 'wb').write(b)
    rep[dst] = {'From': 's100/' + src, 'Sha256': sha(b)}
# parameters
p = json.load(open(os.path.join(V1, 'm7-params.json'), encoding='utf-8'))
p['Note'] = p['Note'] + ' Kit v2 (decisiones §100): trio vigente, RunId AP 16.6, cierre A7-1, disposicion posterior a §100.'
p['Trio'] = TRIO
p['Codex'] = dict(p['Codex'], Label=LABEL, Path=p['Codex']['Path'].replace('e78bad9887fa50cf', LABEL), CompanionsSha256First16=COMPANIONS,
                  KeyNamesBaseline=BASELINE)
p['Repo'], p['RepoWin'] = 'D:/r62-a6m7/m7-run2', 'D:\\r62-a6m7\\m7-run2'
p['RunId'] = RUN_ID
p['IgnorePath'] = 'artifacts/orchestration/FX-U1/T1-m7/0/%s/output.json' % RUN_ID
p['ClosureVariant'] = 'A7-1'
p['LastSectionBeforeDisposition'] = 100
pb = (json.dumps(p, ensure_ascii=False, indent=1) + '\n').encode('utf-8')
open(os.path.join(V2, 'm7-params.json'), 'wb').write(pb)
rep['m7-params.json'] = {'Sha256': sha(pb)}
# Owner lines for the current trio (drafts; never authorizations)
od2 = 'OD-2 = A (línea base %s; binario %s)' % (TRIO['Fingerprint'], TRIO['BinaryHash'])
lb = open(os.path.join(V1, 'line-b.txt'), encoding='utf-8').read()
old_trio = '9ecb9df2ecafcc00542ef0966c6f1737a81397dc9531b0a4b178238e2bf4a46d/26.1013.3656.0/6EFA5B0E1E971F55CC925DB17550325A090FB9155398087C4089444E33DDBE1F'
assert lb.count(old_trio) == 1
lb = lb.replace(old_trio, '%s/%s/%s' % (TRIO['BinaryHash'], TRIO['AppVersion'], TRIO['Fingerprint']))
for n, t in (('line-od2.txt', od2), ('line-b.txt', lb)):
    open(os.path.join(V2, n), 'wb').write(t.encode('utf-8'))
    rep[n] = {'Sha256': sha(t.encode('utf-8'))}
# removed from v2: the v1 designation check (replaced by designate_a7.py), the v1 line base, the v1 freeze and self-test report
for n in ('designate_m7.py', 'baseline-keynames-F909F480.json', 'FREEZE.json', 'selftest-report.json'):
    if os.path.exists(os.path.join(V2, n)):
        os.remove(os.path.join(V2, n))
        rep[n] = 'REMOVED'
shutil.rmtree(os.path.join(V2, '__pycache__'), ignore_errors=True)
print(json.dumps(rep, ensure_ascii=False, indent=1))

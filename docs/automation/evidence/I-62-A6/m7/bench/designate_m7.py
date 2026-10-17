# designate_m7.py - designation of D, H, K (and TRX) of an FX-02 VERIFY invocation by the kit's input paths (A62-A6R4-O3)
# and fail-closed checks of the inputs the A6-9 table reads outside D/H/K or with a type it cannot use (A62-A6R4-O2), before
# the derivation of derive_r4.py (A6-9 table + A6-3 rule 6 (b), unchanged). I-62, A-6, decisiones §90. Stdlib only.
#
# Designation (a6m7-designation/1): Repo (the -C), AuthorityRevision, Invocation {UnitId, TaskId, Attempt, RunId, Target},
# RunPath (directory of the invocation's own output), Inputs [{Role DELEG|H|CONTRACT|TRX, Path, Custody (local file or
# null), Blob (CONTRACT only: blob of the custodied contract)}], Instrument {Path, Blob} | null, MapPath, Adapter.
#
# Rule (i) with O3: D, H and K are the entries of E at the designated paths, and "E pins its blob" means exactly one entry of
# E.CanonicalInputs at that path. Kit path rule (KPR, proposed): KPR-1 RunPath = the directory of IgnorePath(I); KPR-2 every
# designated DELEG / H path is artifacts/orchestration/<I.UnitId>/<dir>/<I.Attempt>/<RunId of the bytes placed there>/...,
# so that Name(p) of the A6-9 table (C6) names it with its own role; KPR-3 <dir> is I.TaskId or I.TaskId-<control>.
#   python -I designate_m7.py <designation.json> <E.json>      -> prints the check result (exit 0 valid, 1 fail closed)
import importlib.util, json, os, re, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
SCHEMA = 'a6m7-designation/1'
ROLES = ('DELEG', 'H', 'CONTRACT', 'TRX')
DV_ROLE = {'DELEG': 'D', 'H': 'H', 'CONTRACT': 'K'}
REL_PATH = re.compile(r'[A-Za-z0-9_.@+-]+(/[A-Za-z0-9_.@+-]+)*\Z')


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


DV = _load('derive_r4')


def rfile(p):
    with open(p, 'rb') as fh:
        return fh.read()


def ignore_path(inv):
    return 'artifacts/orchestration/%s/%s/%s/%s/output.json' % (inv['UnitId'], inv['TaskId'], inv['Attempt'], inv['RunId'])


def name_of(p, contract_path, d_run, h_run):
    """Name(p) of the A6-9 table (C6), exactly as derive_r4.py applies it (contract path first, then the RunId patterns)."""
    if p == contract_path:
        return 'CONTRACT'
    if isinstance(d_run, str) and d_run and re.fullmatch(r'artifacts/orchestration/[^/]+/[^/]+/[0-9]+/%s/output\.json' % re.escape(d_run), p):
        return 'DELEG'
    if isinstance(h_run, str) and h_run and re.fullmatch(r'artifacts/orchestration/[^/]+/[^/]+/[0-9]+/%s/worker-handoff\.json' % re.escape(h_run), p):
        return 'H'
    if p.endswith('.trx'):
        return 'TRX'
    return None


def _entries(E):
    out = []
    for k in ('CanonicalInputs', 'AllowedTransitiveInputs', 'AutomaticInstructions'):
        for e in (E.get(k) or []):
            out.append((k, e))
    return out


def check(des, E, bypass=()):
    """Fail-closed checks. Returns {'Ok', 'Problems', 'Bypassed', 'Inputs', 'Names', 'Vector'}: Vector is the derive_r4
    vector, built only when Ok. A problem whose code is in bypass is recorded as bypassed (used only to show what the
    derivation would do without the check)."""
    probs, byp = [], []

    def P(code, detail):
        (byp if code in bypass else probs).append({'Code': code, 'Detail': detail})

    res = {'Inputs': {}, 'Names': {}}
    # D1 designation shape
    if not isinstance(des, dict) or des.get('Schema') != SCHEMA:
        return dict(res, Ok=False, Problems=[{'Code': 'DESIGNATION_SCHEMA', 'Detail': 'not an a6m7-designation/1 object'}], Bypassed=[], Vector=None)
    inv = des.get('Invocation')
    if not isinstance(inv, dict):
        return dict(res, Ok=False, Problems=[{'Code': 'WRONG_TYPE', 'Detail': 'Invocation is not an object'}], Bypassed=[], Vector=None)
    for k in ('UnitId', 'TaskId', 'RunId'):
        if not isinstance(inv.get(k), str) or not inv.get(k):
            P('WRONG_TYPE', 'I.%s is not a non-empty string' % k)
    if not isinstance(inv.get('Attempt'), int) or isinstance(inv.get('Attempt'), bool):
        P('WRONG_TYPE', 'I.Attempt is not an integer')
    if not isinstance(inv.get('Target'), dict):
        P('WRONG_TYPE', 'I.Target is not an object')
    if not isinstance(des.get('Repo'), str) or not isinstance(des.get('MapPath'), str) or not isinstance(des.get('RunPath'), str):
        P('DESIGNATION_SCHEMA', 'Repo, MapPath and RunPath must be strings')
    ins = des.get('Inputs')
    if not isinstance(ins, list):
        return dict(res, Ok=False, Problems=probs + [{'Code': 'DESIGNATION_SCHEMA', 'Detail': 'Inputs is not a list'}], Bypassed=byp, Vector=None)
    ro = {}
    for i in ins:
        if not isinstance(i, dict) or i.get('Role') not in ROLES:
            P('ROLE_OUTSIDE_DHK', 'input with a role outside DELEG/H/CONTRACT/TRX: %r' % (i.get('Role') if isinstance(i, dict) else i))
            continue
        if i['Role'] in ro:
            P('ROLE_REPEATED', 'role designated twice: ' + i['Role'])
            continue
        if not isinstance(i.get('Path'), str) or not REL_PATH.match(i['Path']) or '..' in i['Path'].split('/'):
            P('PATH', 'designated path is not a relative POSIX path: %r' % i.get('Path'))
            continue
        ro[i['Role']] = i
    for r in ROLES:
        if r not in ro:
            P('ROLE_MISSING', 'role not designated: ' + r)
    paths = [i['Path'] for i in ro.values()]
    if len(set(paths)) != len(paths):
        P('PATH', 'two roles designate the same path')
    if probs:
        return dict(res, Ok=False, Problems=probs, Bypassed=byp, Vector=None)

    # E pins: exactly one CanonicalInputs entry per designated path (O3), never in another list (O2)
    allent = _entries(E)
    pinned = {}
    for r, i in ro.items():
        hits = [(k, e) for k, e in allent if e.get('Path') == i['Path']]
        canon = [e for k, e in hits if k == 'CanonicalInputs']
        if len(hits) != len(canon):
            P('WRONG_CLASS', '%s path pinned outside CanonicalInputs' % r)
        if len(canon) > 1:
            P('MULTIPLE_PINS', '%s path pinned %d times in E' % (r, len(canon)))
        pinned[r] = canon[0] if len(canon) == 1 else None
    # inputs outside D/H/K: every off-Git entry (artifacts/) and every *.trx entry of E must be designated (O2, O3)
    desig = set(paths)
    for k, e in allent:
        p = e.get('Path')
        if p in desig:
            continue
        if isinstance(p, str) and (p.startswith('artifacts/') or p.endswith('.trx')):
            P('UNDESIGNATED_INPUT', 'E pins an off-Git or TRX entry the kit did not designate: ' + p)

    # bytes and rule (i) status of D, H, K (absent inputs are rule (i), not a failure)
    objs = {}
    for r in ('DELEG', 'H', 'CONTRACT'):
        e = pinned.get(r)
        st = {'Path': ro[r]['Path'], 'Pinned': e is not None}
        o = None
        if e is not None:
            cust = ro[r].get('Custody')
            b = None
            if cust:
                b = rfile(cust) if os.path.isfile(cust) else None
            else:
                b = _git_blob(des['Repo'], e.get('Blob'))
            if b is None:
                st['Why'] = 'custodied bytes do not exist'
            elif DV.blob_id(b) != e.get('Blob'):
                st['Why'] = 'bytes do not have the pinned blob'
            else:
                o, why = DV.strict_object(b)
                st['Why'] = why
        st['Present'] = o is not None
        objs[r] = o
        res['Inputs'][r] = st
    # TRX: designated, pinned once, bytes verify (O2: otherwise no derivation)
    te = pinned.get('TRX')
    if te is None:
        P('TRX_NOT_PINNED', 'the designated TRX is not pinned by E')
    else:
        cust = ro['TRX'].get('Custody')
        b = rfile(cust) if cust and os.path.isfile(cust) else None
        if b is None or DV.blob_id(b) != te.get('Blob'):
            P('TRX_UNVERIFIED', 'TRX custodied bytes absent or with another blob')
    # map M read by C4 (O2: otherwise no derivation)
    mhits = [e for k, e in allent if e.get('Path') == des.get('MapPath')]
    if len(mhits) != 1:
        P('MAP_PINS', 'E pins the clause map %d times (exactly 1 required)' % len(mhits))
    else:
        mb = _git_blob(des['Repo'], mhits[0].get('Blob'))
        mo, why = DV.strict_object(mb) if mb is not None and DV.blob_id(mb) == mhits[0].get('Blob') else (None, 'unreadable or another blob')
        if mo is None:
            P('MAP_UNUSABLE', 'clause map bytes: ' + str(why))
        else:
            sf, fl = mo.get('Surfaces'), mo.get('Files')
            if not (isinstance(sf, list) and all(isinstance(x, str) for x in sf)):
                P('MAP_UNUSABLE', 'Surfaces is not a list of strings')
            if not (isinstance(fl, list) and all(isinstance(x, dict) and isinstance(x.get('Path'), str) for x in fl)):
                P('MAP_UNUSABLE', 'Files is not a list of objects with Path')
    # types of the non-commit fields the table reads (commit fields are rule (ii): a wrong type is NOT_A_COMMIT)
    D, H, K = objs['DELEG'], objs['H'], objs['CONTRACT']

    def str_or_absent(o, f, who):
        if o is None or f not in o or o[f] is None or o[f] == '':
            return
        if not isinstance(o[f], str):
            P('WRONG_TYPE', '%s.%s has JSON type %s' % (who, f, type(o[f]).__name__))

    str_or_absent(D, 'ExpectedBranch', 'D')
    str_or_absent(D, 'RunId', 'D')
    str_or_absent(H, 'RunId', 'H')
    if K is not None and K.get('Authorities') not in (None, []) and 'Authorities' in K:
        au = K['Authorities']
        if not (isinstance(au, list) and all(isinstance(a, dict) and isinstance(a.get('Path'), str) and a['Path'] for a in au)):
            P('WRONG_TYPE', 'K.Authorities is not a list of objects with a non-empty Path')
    # kit path rule
    if isinstance(inv.get('UnitId'), str) and isinstance(inv.get('RunId'), str):
        want = ignore_path(inv)[:-len('output.json')]
        if des.get('RunPath') != want:
            P('KPR-1', 'RunPath %r is not the directory of IgnorePath(I) %r' % (des.get('RunPath'), want))
    d_run = D.get('RunId') if D is not None else None
    h_run = H.get('RunId') if H is not None else None
    for r, run, fname in (('DELEG', d_run, 'output.json'), ('H', h_run, 'worker-handoff.json')):
        p = ro[r]['Path']
        m = re.fullmatch(r'artifacts/orchestration/([^/]+)/([^/]+)/([0-9]+)/([^/]+)/' + re.escape(fname), p)
        if not m:
            P('KPR-2', '%s path is not artifacts/orchestration/<u>/<dir>/<a>/<RunId>/%s' % (r, fname))
            continue
        if m.group(1) != inv.get('UnitId') or m.group(3) != str(inv.get('Attempt')):
            P('KPR-3', '%s path unit/attempt differ from I' % r)
        if not (m.group(2) == inv.get('TaskId') or m.group(2).startswith(str(inv.get('TaskId')) + '-')):
            P('KPR-3', '%s path directory %r is neither I.TaskId nor I.TaskId-<control>' % (r, m.group(2)))
        if res['Inputs'][r]['Present'] and isinstance(run, str) and run and m.group(4) != run:
            P('KPR-2', '%s path names RunId %s but the bytes carry RunId %s: Name(p) undefined in C6' % (r, m.group(4), run))
    kp = ro['CONTRACT']['Path']
    for k, e in allent:
        if k != 'CanonicalInputs':
            continue
        n = name_of(e.get('Path'), kp, d_run, h_run)
        if n is not None:
            res['Names'].setdefault(n, []).append(e.get('Path'))
    for n, r, run in (('DELEG', 'DELEG', d_run), ('H', 'H', h_run)):
        # a RunId absent, null or empty is rule (ii) (C6 term omitted); a non-string RunId is WRONG_TYPE above
        if res['Inputs'][r]['Present'] and isinstance(run, str) and run and res['Names'].get(n) != [ro[r]['Path']]:
            P('KPR-2', 'Name(p) = %s is %r, not exactly the designated path' % (n, res['Names'].get(n)))
    if res['Names'].get('TRX') and res['Names']['TRX'] != [ro['TRX']['Path']]:
        P('UNDESIGNATED_INPUT', 'Name(p) = TRX for %r' % res['Names']['TRX'])
    vec = None
    if not probs:
        cust = dict((ro[r]['Path'], ro[r]['Custody']) for r in ('DELEG', 'H', 'TRX') if ro[r].get('Custody'))
        if ro['CONTRACT'].get('Custody'):
            cust[ro['CONTRACT']['Path']] = ro['CONTRACT']['Custody']
        vec = {'Repo': des['Repo'], 'I': inv, 'Designation': dict((DV_ROLE[r], ro[r]['Path']) for r in ('DELEG', 'H', 'CONTRACT')),
               'Custody': cust, 'MapPath': des['MapPath']}
    return dict(res, Ok=not probs and not byp, Problems=probs, Bypassed=byp, Vector=vec)


def _git_blob(repo, blob):
    if not isinstance(blob, str):
        return None
    rc, out = DV.git(repo, 'cat-file', 'blob', blob)
    return out if rc == 0 else None


def main(argv):
    if len(argv) != 3:
        sys.stderr.write('usage: python -I designate_m7.py <designation.json> <E.json>\n')
        return 64
    des = json.loads(rfile(argv[1]).decode('utf-8'))
    E = json.loads(rfile(argv[2]).decode('utf-8'))
    r = check(des, E)
    sys.stdout.write(json.dumps({'Ok': r['Ok'], 'Problems': r['Problems'], 'Inputs': r['Inputs'], 'Names': r['Names']},
                                ensure_ascii=True, sort_keys=True) + '\n')
    return 0 if r['Ok'] else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv))

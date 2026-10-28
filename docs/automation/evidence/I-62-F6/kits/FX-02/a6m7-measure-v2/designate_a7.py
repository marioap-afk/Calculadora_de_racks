# designate_a7.py - designation check and derivation vector of an FX-02 VERIFY invocation for the acceptor-closure derivation
# (derive_r4.py, unchanged: the A6-9 table with A6-3 rule 6 (b)) under A7-1 round 2 (I-62 A-7): the designation a7-designation/2
# names six roles (CONTRACT, DELEG, H, TRX_CURRENT, TRX_RED, RELAY); D, H and K are identified only by their designated paths
# (A7-1 point 6). Round-2 counterpart of designate_m7.py (a6m7-designation/1, four roles), which is not changed. Stdlib only.
#
#   python -I designate_a7.py <designation.json> <E.json>      -> prints the check result (exit 0 valid, 1 fail closed)
#
# Fail-closed checks (before any derivation): every designated artifact that E pins is pinned exactly once and only in
# CanonicalInputs; E pins no off-Git (artifacts/) or *.trx entry that the designation does not name; every designated TRX that
# E pins has custodied bytes with that blob (a BOM is part of the bytes); the clause map is pinned once and usable; KPR-1
# (RunPath = the directory of IgnorePath(I)); KPR-2/3 (DELEG and H under artifacts/orchestration/<UnitId>/<dir>/<Attempt>/<RunId
# of their bytes>/, <dir> = I.TaskId or I.TaskId-<control>), so that Name(p) of C6 is DELEG / H exactly at the designated path;
# Name(p) = TRX exactly for the designated TRX paths; the relay facts get no Name in C6 (the instrument does not read them).
import importlib.util, json, os, re, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
SCHEMA = 'a7-designation/4'   # round 4 (I-62 A-7, decisiones §98): REBASE also as the copy in a control on the inputs of a reverification
REBASE_KINDS = ('REVERIFICATION', 'REVERIFICATION_CONTROL')
ROLES = ('CONTRACT', 'DELEG', 'H', 'TRX_CURRENT', 'TRX_RED', 'RELAY', 'REBASE')


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
    """Name(p) of the A6-9 table (C6), exactly as derive_r4.py applies it."""
    if p == contract_path:
        return 'CONTRACT'
    if isinstance(d_run, str) and d_run and re.fullmatch(r'artifacts/orchestration/[^/]+/[^/]+/[0-9]+/%s/output\.json' % re.escape(d_run), p):
        return 'DELEG'
    if isinstance(h_run, str) and h_run and re.fullmatch(r'artifacts/orchestration/[^/]+/[^/]+/[0-9]+/%s/worker-handoff\.json' % re.escape(h_run), p):
        return 'H'
    if p.endswith('.trx'):
        return 'TRX'
    return None


def _bytes(des, item, blob):
    if item.get('Custody'):
        return rfile(item['Custody']) if os.path.isfile(item['Custody']) else None
    rc, out = DV.git(des['Repo'], 'cat-file', 'blob', blob)
    return out if rc == 0 else None


def check(des, E):
    probs = []
    res = {'Inputs': {}, 'Names': {}}
    if not isinstance(des, dict) or des.get('Schema') != SCHEMA:
        return dict(res, Ok=False, Problems=[{'Code': 'DESIGNATION_SCHEMA', 'Detail': 'not a7-designation/4'}], Vector=None)
    inv = des.get('Invocation') or {}
    ro = {}
    for i in des.get('Artifacts') or []:
        r = i.get('Role') if isinstance(i, dict) else None
        if r not in ROLES or r in ro:
            probs.append({'Code': 'ROLE', 'Detail': 'role outside the seven or repeated: %r' % (r,)})
            continue
        ro[r] = i
    for r in ('CONTRACT', 'DELEG'):
        if r not in ro:
            probs.append({'Code': 'ROLE_MISSING', 'Detail': r})
    if probs:
        return dict(res, Ok=False, Problems=probs, Vector=None)
    canon = E.get('CanonicalInputs') or []
    allent = [(k, e) for k in ('CanonicalInputs', 'AllowedTransitiveInputs', 'AutomaticInstructions') for e in (E.get(k) or [])]
    pinned = {}
    for r, i in ro.items():
        hits = [(k, e) for k, e in allent if e.get('Path') == i['Path']]
        if any(k != 'CanonicalInputs' for k, e in hits):
            probs.append({'Code': 'WRONG_CLASS', 'Detail': '%s pinned outside CanonicalInputs' % r})
        if len(hits) > 1:
            probs.append({'Code': 'MULTIPLE_PINS', 'Detail': '%s pinned %d times' % (r, len(hits))})
        pinned[r] = hits[0][1] if len(hits) == 1 else None
    desig = set(i['Path'] for i in ro.values())
    for k, e in allent:
        p = e.get('Path')
        if p not in desig and isinstance(p, str) and (p.startswith('artifacts/') or p.endswith('.trx')):
            probs.append({'Code': 'UNDESIGNATED_INPUT', 'Detail': 'E pins an off-Git or TRX entry the designation does not name: ' + p})
    objs = {}
    for r in ('DELEG', 'H', 'CONTRACT'):
        e = pinned.get(r)
        o = None
        if e is not None and r in ro:
            b = _bytes(des, ro[r], e.get('Blob'))
            if b is not None and DV.blob_id(b) == e.get('Blob'):
                o, _why = DV.strict_object(b)
        objs[r] = o
        res['Inputs'][r] = {'Pinned': e is not None, 'Present': o is not None}
    for r in ('TRX_CURRENT', 'TRX_RED'):
        e = pinned.get(r)
        if e is not None:
            b = _bytes(des, ro[r], e.get('Blob'))
            if b is None or DV.blob_id(b) != e.get('Blob'):
                probs.append({'Code': 'TRX_UNVERIFIED', 'Detail': '%s custodied bytes absent or with another blob' % r})
    mhits = [e for k, e in allent if e.get('Path') == des.get('MapPath')]
    if len(mhits) != 1:
        probs.append({'Code': 'MAP_PINS', 'Detail': 'E pins the clause map %d times' % len(mhits)})
    else:
        rc, mb = DV.git(des['Repo'], 'cat-file', 'blob', mhits[0].get('Blob'))
        mo, why = DV.strict_object(mb) if rc == 0 else (None, 'unreadable')
        if mo is None or not isinstance(mo.get('Files'), list) or not isinstance(mo.get('Surfaces'), list):
            probs.append({'Code': 'MAP_UNUSABLE', 'Detail': str(why)})
    if des.get('RunPath') != ignore_path(inv)[:-len('output.json')]:
        probs.append({'Code': 'KPR-1', 'Detail': 'RunPath is not the directory of IgnorePath(I)'})
    D, H = objs['DELEG'], objs['H']
    d_run = D.get('RunId') if D else None
    h_run = H.get('RunId') if H else None
    for r, run, fname in (('DELEG', d_run, 'output.json'), ('H', h_run, 'worker-handoff.json')):
        if r not in ro:
            continue
        m = re.fullmatch(r'artifacts/orchestration/([^/]+)/([^/]+)/([0-9]+)/([^/]+)/' + re.escape(fname), ro[r]['Path'])
        if not m:
            probs.append({'Code': 'KPR-2', 'Detail': '%s path shape' % r})
            continue
        if m.group(1) != inv.get('UnitId') or m.group(3) != str(inv.get('Attempt')) or not (
                m.group(2) == inv.get('TaskId') or m.group(2).startswith(str(inv.get('TaskId')) + '-')):
            probs.append({'Code': 'KPR-3', 'Detail': '%s path unit/dir/attempt' % r})
        if isinstance(run, str) and run and m.group(4) != run:
            probs.append({'Code': 'KPR-2', 'Detail': '%s path names RunId %s, bytes carry %s' % (r, m.group(4), run)})
    kp = ro['CONTRACT']['Path']
    for e in canon:
        n = name_of(e.get('Path'), kp, d_run, h_run)
        if n:
            res['Names'].setdefault(n, []).append(e.get('Path'))
    for n, r, run in (('DELEG', 'DELEG', d_run), ('H', 'H', h_run)):
        if objs.get(r) is not None and isinstance(run, str) and run and res['Names'].get(n) != [ro[r]['Path']]:
            probs.append({'Code': 'KPR-2', 'Detail': 'Name(p) = %s is %r' % (n, res['Names'].get(n))})
    want_trx = sorted(ro[r]['Path'] for r in ('TRX_CURRENT', 'TRX_RED') if r in ro and pinned.get(r) is not None)
    if sorted(res['Names'].get('TRX', [])) != want_trx:
        probs.append({'Code': 'UNDESIGNATED_INPUT', 'Detail': 'Name(p) = TRX for %r, designated %r' % (res['Names'].get('TRX'), want_trx)})
    if 'RELAY' in ro and name_of(ro['RELAY']['Path'], kp, d_run, h_run) is not None:
        probs.append({'Code': 'RELAY_NAMED', 'Detail': 'the relay facts path gets a Name in C6'})
    if 'REBASE' in ro:
        if name_of(ro['REBASE']['Path'], kp, d_run, h_run) is not None:
            probs.append({'Code': 'REBASE_NAMED', 'Detail': 'the rebase record path gets a Name in C6'})
        if inv.get('Kind', 'VERIFY') not in REBASE_KINDS and pinned.get('REBASE') is not None:
            probs.append({'Code': 'REBASE_NOT_ADMITTED', 'Detail': 'E pins a rebase record in an invocation that is neither the 16.7 reverification nor a '
                                                                   'control on its inputs'})
        # the reverification: the attempt directory of the task; a control on its inputs: the attempt directory of the control path
        tdir = inv.get('TaskId') if inv.get('Kind') != 'REVERIFICATION_CONTROL' else '%s-%s' % (inv.get('TaskId'), (des.get('Source') or {}).get('Control'))
        m = re.fullmatch(r'artifacts/orchestration/([^/]+)/([^/]+)/([0-9]+)/([^/]+)/[^/]+', ro['REBASE']['Path'])
        if not m or m.group(1) != inv.get('UnitId') or m.group(2) != tdir or m.group(3) != str(inv.get('Attempt')) \
                or m.group(4) != ro['REBASE'].get('RunId'):
            probs.append({'Code': 'KPR-4', 'Detail': 'REBASE path is not in the attempt directory (of the task, or of the control path) under its registered RunId'})
    elif inv.get('Kind') in REBASE_KINDS:
        probs.append({'Code': 'ROLE_MISSING', 'Detail': 'REBASE (16.7 reverification or a control on its inputs)'})
    vec = None
    if not probs:
        cust = dict((ro[r]['Path'], ro[r]['Custody']) for r in ro if ro[r].get('Custody'))
        vec = {'Repo': des['Repo'], 'I': inv, 'Designation': {'D': ro['DELEG']['Path'], 'H': ro['H']['Path'] if 'H' in ro else None,
                                                             'K': ro['CONTRACT']['Path']},
               'Custody': cust, 'MapPath': des['MapPath']}
    return dict(res, Ok=not probs, Problems=probs, Vector=vec)


def main(argv):
    if len(argv) != 3:
        sys.stderr.write('usage: python -I designate_a7.py <designation.json> <E.json>\n')
        return 64
    des = json.loads(rfile(argv[1]).decode('utf-8'))
    E = json.loads(rfile(argv[2]).decode('utf-8'))
    r = check(des, E)
    sys.stdout.write(json.dumps({'Ok': r['Ok'], 'Problems': r['Problems'], 'Names': r['Names']}, ensure_ascii=True, sort_keys=True) + '\n')
    return 0 if r['Ok'] else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv))

"""Recomprobacion v2: los literales de F4B_EXACT_EVIDENCES frente a los contratos (copias y git show, solo lectura), y donde aparecen.
Uso (cwd = checkout del repo): python -I verify_literals_own.py <dir-v2> <dir-v1-pbt2acc2>"""
import hashlib, importlib.util, json, os, subprocess, sys

V2, V1 = sys.argv[1], sys.argv[2]
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
spec = importlib.util.spec_from_file_location('sub_v2', os.path.join(V2, 'nc2_exact_whitelist_substitute_f4b_v2.py'))
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)
lits = m.F4B_EXACT_EVIDENCES


def git_show(rev, path):
    return subprocess.run(['git', 'show', '%s:%s' % (rev, path)], capture_output=True, check=True).stdout


def rb(*p):
    with open(os.path.join(*p), 'rb') as f:
        return f.read()


EVD = 'docs/automation/evidence/I-64-pilot'
sources = {
    'PB-T2a copia v2': rb(V2, 'data', 'pbt2a', 'real', 'inputs', 'gate-contract.json'),
    'PB-T2a copia v1 (mi primera revision)': rb(V1, 'data', 'real', 'inputs', 'gate-contract.json'),
    'PB-T2a git c40eea4a coordinator': git_show('c40eea4a', EVD + '/F4B-PBT2A-DRAFT-STATE-PORT/coordinator/gate-contract.json'),
    'PB-T2b git c40eea4a coordinator': git_show('c40eea4a', EVD + '/F4B-PBT2B-WORKSPACE-MODE-WINDOW/coordinator/gate-contract.json'),
    'PB-T2c git c40eea4a coordinator': git_show('c40eea4a', EVD + '/F4B-PBT2C-WORKSPACE-CARRYOVER/coordinator/gate-contract.json'),
    'PB-T3a copia v2': rb(V2, 'data', 'pbt3a', 'real', 'inputs', 'gate-contract.json'),
    'PB-T3a git 485b5651 coordinator': git_show('485b5651', EVD + '/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json'),
    'PB-T3a git c40eea4a coordinator': git_show('c40eea4a', EVD + '/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json'),
}
out = {'TupleLen': len(lits), 'Types': [type(x).__name__ for x in lits], 'Ascii': all(ord(c) < 128 for l in lits for c in l),
       'Distinct': len(set(lits)) == len(lits), 'LiteralSha256': [sha(l.encode('utf-8')) for l in lits], 'Sources': {}}
for name, b in sources.items():
    ct = json.loads(b.decode('utf-8'))
    ee = ct.get('ExpectedEvidence', [])
    hits = {i: [j for j, l in enumerate(lits) if e == l] for i, e in enumerate(ee) if any(e == l for l in lits)}
    byte_hits = [j for j, l in enumerate(lits) if json.dumps(l, ensure_ascii=False).encode('utf-8') in b]
    bab = [i for i, e in enumerate(ee) if isinstance(e, str) and 'byte a byte' in e]
    out['Sources'][name] = {'Sha256': sha(b), 'TaskId': ct.get('TaskId'), 'EECount': len(ee), 'ExactHits(index->literal)': hits,
                            'LiteralBytesPresent': byte_hits, 'EntriesWithByteAByte': bab,
                            'EE0EqualsLiteral': [j for j, l in enumerate(lits) if ee and ee[0] == l]}
# dato: las entradas de ExpectedEvidence de PB-T3a que mencionan byte a byte (para ver la segunda aparicion)
ct3 = json.loads(sources['PB-T3a git 485b5651 coordinator'].decode('utf-8'))
out['PBT3a_EE_byte_entries'] = {i: e[:260] for i, e in enumerate(ct3['ExpectedEvidence']) if 'byte a byte' in e}
out['PBT3a_other_fields_with_byte_a_byte'] = [k for k, v in ct3.items() if k != 'ExpectedEvidence' and 'byte a byte' in json.dumps(v, ensure_ascii=False)]
# los datos de PB-T2a de la v2 frente a los de mi primera revision
same = {}
for sub in ('real', 'nc2', 'planning'):
    for r, _, fs in os.walk(os.path.join(V1, 'data', sub)):
        for f in fs:
            rel = os.path.relpath(os.path.join(r, f), os.path.join(V1, 'data'))
            p2 = os.path.join(V2, 'data', 'pbt2a', rel)
            same[rel.replace(os.sep, '/')] = os.path.exists(p2) and rb(p2) == rb(r, f)
out['PBT2aDataSameAsFirstReview'] = all(same.values())
out['PBT2aDataDiffs'] = [k for k, v in same.items() if not v]
print(json.dumps(out, ensure_ascii=False, indent=1))

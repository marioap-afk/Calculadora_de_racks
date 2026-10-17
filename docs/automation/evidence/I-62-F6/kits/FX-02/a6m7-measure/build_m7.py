# build_m7.py - build-time steps of the A6-7 measurement kit (I-62): custodied inputs, the designation of M against the -C,
# the VERIFY closure of M (E with make_eic_verify.py R7-A, designation check with designate_m7.py, derived closure with
# derive_r4.py and its omission record), the sealed oracle (tools_m7.py), the armed prompt, the schemas and the freeze.
# Stdlib only. Never executes Codex or any model CLI; every git call is read-only.
#   python -I build_m7.py inputs | designation | closure | oracle | schema | prompt | markers | freeze | all-after-clone
import importlib.util, json, os, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


L = _load('lib_kit')
for _k in [k for k in os.environ if k.upper().startswith('GIT_')]:
    del os.environ[_k]                                    # bench modules call git by name with the inherited environment
MK = _load('make_eic_verify')
DS = _load('designate_m7')
DV = _load('derive_r4')

BENCH = 'D:/r62-a6m7/bench'
BENCH_M = BENCH + '/designations/M.json'
BENCH_M_SHA = '8c3e9ecd11117bd46cd7a59adcf3b9ea339c5604529f3f875090e3a6bd2c5f79'
DESIG = L.FROZEN + '/designation-M.json'
E_PATH = L.FROZEN + '/E.json'
VEC = L.FROZEN + '/vector.json'
CLOSURE = L.FROZEN + '/closure.json'
RECORD = L.FROZEN + '/derivation.json'


def wjson_local(p, obj):
    """Work JSON as the benches write the tool / acceptor inputs: UTF-8, indent 1, ensure_ascii False, LF."""
    os.makedirs(os.path.dirname(p), exist_ok=True)
    with open(p, 'w', encoding='utf-8', newline='\n') as fh:
        fh.write(json.dumps(obj, ensure_ascii=False, indent=1) + '\n')
    return p


def input_objects(worktree):
    deleg = json.loads(L.rbytes(L.R2_INP + '/deleg-deliv.json').decode('utf-8'))
    deleg['ExpectedWorktree'] = worktree
    hand = json.loads(L.rbytes(L.R2_INP + '/handoff-deliv.json').decode('utf-8'))
    hand['Worktree'] = worktree
    return deleg, hand


def cmd_inputs():
    """Custodied inputs of M (RAE §10 copies of the real verification's inputs; only the worktree field names the -C).
    Method check: the same transformation with the bench copy reproduces the bench bytes of M (work/in-m)."""
    import tempfile
    rep = {}
    tmp = L.KIT + '/.build-tmp'
    L.rmtree(tmp)
    d, h = input_objects('D:/r62-a6m7/bench/c-m')
    wjson_local(tmp + '/deleg.json', d)
    wjson_local(tmp + '/handoff.json', h)
    for n in ('deleg.json', 'handoff.json'):
        a, b = L.sha256_file(tmp + '/' + n), L.sha256_file(BENCH + '/work/in-m/' + n)
        rep['MethodReproducesBench:' + n] = {'Built': a, 'Bench': b, 'Equal': a == b}
    L.rmtree(tmp)
    d, h = input_objects(L.CDIR)
    wjson_local(L.CUSTODY['DELEG'], d)
    wjson_local(L.CUSTODY['H'], h)
    L.wbytes(L.CUSTODY['TRX'], L.rbytes(L.R2_INP + '/fixture-tests.trx'))
    for r, p in sorted(L.CUSTODY.items()):
        rep[r] = {'Path': p, 'Sha256': L.sha256_file(p), 'Blob': L.blob_sha1(L.rbytes(p))}
    ok = all(v['Equal'] for k, v in rep.items() if k.startswith('MethodReproducesBench'))
    print(json.dumps(rep, indent=1, sort_keys=True))
    return 0 if ok else 1


def cmd_designation():
    if L.sha256_file(BENCH_M) != BENCH_M_SHA:
        raise SystemExit('bench designation M changed')
    des = L.rjson(BENCH_M)
    des['Repo'] = L.CDIR
    for i in des['Inputs']:
        if i['Role'] in L.CUSTODY:
            i['Custody'] = L.CUSTODY[i['Role']]
    des['DecisionRef'] = L.DECISION_REF
    if des['Invocation'] != L.invocation() or des['RunPath'] != L.RUN_PATH:
        raise SystemExit('bench designation M is not the measurement invocation of this kit')
    wjson_local(DESIG, des)
    base = L.rjson(BENCH_M)
    diff = sorted(k for k in set(base) | set(des) if base.get(k) != des.get(k))
    print(json.dumps({'Designation': DESIG, 'Sha256': L.sha256_file(DESIG), 'FieldsDifferingFromBenchM': diff}, indent=1))
    return 0


def counts(c):
    return dict((k, len(c[k])) for k in ('Commits', 'RevPaths', 'Refs', 'Paths', 'Objects', 'Files'))


def cmd_closure():
    des = L.rjson(DESIG)
    E, rep = MK.build(des, 'R7-A', instrument=True)
    L.wbytes(E_PATH, MK.serialize(E))
    chk = DS.check(des, json.loads(L.rbytes(E_PATH).decode('utf-8')))
    if not chk['Ok'] or chk['Vector'] is None:
        print(json.dumps({'DesignationCheck': chk['Problems']}, indent=1))
        return 1
    vec = dict(chk['Vector'], E=E_PATH)
    wjson_local(VEC, vec)
    res = DV.derive_vector(vec)
    wjson_local(CLOSURE, res['Closure'])
    wjson_local(RECORD, {'Schema': 'a6m7-closure-derivation/1', 'Designation': DESIG, 'DesignationSha256': L.sha256_file(DESIG),
                         'Vector': VEC, 'VectorSha256': L.sha256_file(VEC), 'E': E_PATH, 'ESha256': L.sha256_file(E_PATH),
                         'Closure': CLOSURE, 'ClosureSha256': L.sha256_file(CLOSURE),
                         'DesignationCheck': {'Ok': chk['Ok'], 'Problems': chk['Problems'], 'Bypassed': chk['Bypassed'],
                                              'Inputs': chk['Inputs'], 'Names': chk['Names']},
                         'Omitted': res['Omitted'], 'Inputs': res['Inputs']})
    # comparison with the bench derivation of M (D-M-R7-A): the same table, another -C path in the custodied inputs
    bench_c = L.rjson(BENCH + '/work/build/M-R7-A.closure.json')
    mine = res['Closure']
    diff = {}
    for k in ('Commits', 'Refs', 'Paths', 'Objects'):
        a, b = sorted(mine[k]), sorted(bench_c[k])
        if a != b:
            diff[k] = {'Extra': [x for x in a if x not in b], 'Missing': [x for x in b if x not in a]}
    rp = lambda c: sorted((x['Rev'], x['Path']) for x in c['RevPaths'])
    if rp(mine) != rp(bench_c):
        diff['RevPaths'] = 'differ'
    fa = sorted((x['Name'], x['Sha256']) for x in mine['Files'])
    fb = sorted((x['Name'], x['Sha256']) for x in bench_c['Files'])
    if fa != fb:
        diff['Files'] = {'Extra': [x for x in fa if x not in fb], 'Missing': [x for x in fb if x not in fa]}
    out = {'E': {'Path': E_PATH, 'Sha256': L.sha256_file(E_PATH), 'Excluded': rep['Excluded'],
                 'I6Violations': [(r['Path'], r['Status']) for r in rep['I6']['Violations']],
                 'TransitiveAtEFFAllEqual': rep['TransitiveAtEFF']['AllEqual'], 'Entries': len(E['CanonicalInputs'])},
           'Closure': {'Path': CLOSURE, 'Sha256': L.sha256_file(CLOSURE), 'Counts': counts(mine),
                       'Files': sorted(f['Name'] for f in mine['Files'])},
           'Omitted': sorted((o['Term'], o['Cause']) for o in res['Omitted']),
           'Record': {'Path': RECORD, 'Sha256': L.sha256_file(RECORD)}, 'Vector': {'Path': VEC, 'Sha256': L.sha256_file(VEC)},
           'DiffFromBenchM': diff}
    print(json.dumps(out, indent=1, sort_keys=True))
    ok = (out['Omitted'] == [['C1.D.ChainRedSha', 'FIELD_NULL']] or out['Omitted'] == [('C1.D.ChainRedSha', 'FIELD_NULL')]) \
        and sorted(out['Closure']['Files']) == ['CONTRACT', 'DELEG', 'H', 'TRX'] \
        and set(diff) <= {'Files'} and sorted(n for n, _ in diff.get('Files', {}).get('Extra', [])) in ([], ['DELEG', 'H'])
    return 0 if ok else 1


def main(argv):
    cmd = argv[1] if len(argv) > 1 else ''
    if cmd == 'inputs':
        return cmd_inputs()
    if cmd == 'designation':
        return cmd_designation()
    if cmd == 'closure':
        return cmd_closure()
    if cmd in ('oracle', 'schema', 'prompt', 'markers', 'freeze'):
        T = _load('tools_m7')
        return T.build_step(cmd)
    sys.stderr.write('usage: python -I build_m7.py inputs|designation|closure|oracle|schema|prompt|markers|freeze\n')
    return 64


if __name__ == '__main__':
    sys.exit(main(sys.argv))

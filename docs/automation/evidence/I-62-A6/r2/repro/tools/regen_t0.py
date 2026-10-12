# regen_t0.py - verification helper (not part of the reproduction). Re-runs the pinned tool file copy, read-only, on the
# reproduced T0 with the same 13 argv of run_r1.py, because r1-p1-p2-p3.json keeps only the digests of those envelopes.
# Writes the envelopes to D:/r62-a6r2/repro/verify-r1r4/regen-t0/ (outside the work root) and checks that each one is
# byte-identical to what the reproduction recorded (FileCopyStdoutSha256) and that T0 is unchanged.
# python -I regen_t0.py <repro r1r4 scripts dir>
import hashlib, importlib.util, json, os, sys

sys.dont_write_bytecode = True
here = sys.argv[1]
_s = importlib.util.spec_from_file_location('lib_r1r4', os.path.join(here, 'lib_r1r4.py'))
L = importlib.util.module_from_spec(_s)
_s.loader.exec_module(L)
assert L.WORK == 'D:/r62-a6r2/repro/r1r4'
T0 = L.WORK + '/r1/t0'
inp0 = L.WORK + '/r1/in-t0'
outd = 'D:/r62-a6r2/repro/verify-r1r4/regen-t0'
os.makedirs(outd, exist_ok=True)
H = dict((n, L.sha256(L.rbytes(inp0 + '/' + n))) for n in ('K.json', 'deleg.json', 'handoff.json', 'fixture-tests.trx'))
rec = json.load(open(os.path.join(here, 'r1-p1-p2-p3.json'), encoding='ascii'))
want = dict((r['Name'], r['FileCopyStdoutSha256']) for r in rec['P3']['Rows'])
s0 = L.snapshot(T0)
res = {}
for name, argv in L.runs_for(T0, inp0, H):
    rc, out = L.run_tool_file(argv)
    L.wbytes(outd + '/' + name + '.json', out)
    res[name] = {'Exit': rc, 'Sha256': L.sha256(out), 'EqualsRecorded': L.sha256(out) == want.get(name)}
s1 = L.snapshot(T0)
print(json.dumps({'Runs': res, 'T0Unchanged': L.snapdiff(s0, s1) == {}, 'AllEqualRecorded': all(v['EqualsRecorded'] for v in res.values())}, sort_keys=True))

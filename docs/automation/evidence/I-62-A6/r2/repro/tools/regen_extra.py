# regen_extra.py - verification helper (not part of the reproduction) for three digests that run_r1.py / run_r4.py record
# without keeping the underlying bytes:
#   (1) r1-attacks.json /Attacks/d2/BeforeP1/P3Identity  - P3 identity through pwsh on td2b (executes the bench's own
#       harmless FAKE_TOOL served by the alternate pack, exactly as run_r1.py did);
#   (2) r1-attacks.json /Attacks/g/Timeline/2/Envelope   - P3 identity through pwsh on tg;
#   (3) r4-t2.json /Pipeline/P1/Snapshot*/WorktreeFilesSha256 - the working-tree listing of the linked worktree T2, whose
#       '.git' file holds the absolute gitdir path.
# Each regenerated value is first checked against the digest the reproduction recorded (same bytes as at run time), then
# the repro work root is written as the original root to predict the original digest. Writes only under
# D:/r62-a6r2/repro/verify-r1r4/regen-extra/ and the <pairs json> given.
# python -I regen_extra.py <repro r1r4 scripts dir> <pairs json out>
import importlib.util, json, os, re, sys

sys.dont_write_bytecode = True
here, pairs_out = sys.argv[1], sys.argv[2]
_s = importlib.util.spec_from_file_location('lib_r1r4', os.path.join(here, 'lib_r1r4.py'))
L = importlib.util.module_from_spec(_s)
_s.loader.exec_module(L)
assert L.WORK == 'D:/r62-a6r2/repro/r1r4'
outd = 'D:/r62-a6r2/repro/verify-r1r4/regen-extra'
os.makedirs(outd, exist_ok=True)
att = json.load(open(os.path.join(here, 'r1-attacks.json'), encoding='ascii'))['Attacks']
t2 = json.load(open(os.path.join(here, 'r4-t2.json'), encoding='ascii'))
res = {}


def H_of(inp):
    return dict((n, L.sha256(L.rbytes(inp + '/' + n))) for n in ('K.json', 'deleg.json', 'handoff.json', 'fixture-tests.trx'))


def to_orig(b):
    return re.sub(rb'([Dd]:)(/|\\\\|\\)r62-a6r2\2repro\2r1r4(?![A-Za-z0-9])', lambda m: m.group(1) + m.group(2) + b'r62-a6r2' + m.group(2) + b'r1r4', b)


for label, T, inp, want in (('d2b-identity', L.WORK + '/r1/td2b', L.WORK + '/r1/in-td2b', att['d2']['BeforeP1']['P3Identity']['StdoutSha256']),
                            ('tg-identity', L.WORK + '/r1/tg', L.WORK + '/r1/in-tg', att['g']['Timeline'][2]['Envelope']['StdoutSha256'])):
    s0 = L.snapshot(T)
    argv = dict(L.runs_for(T, inp, H_of(inp)))['identity']
    rc, out, cl, err = L.run_tool_shell(T, argv, 'pwsh')
    L.wbytes(outd + '/' + label + '.json', out)
    s1 = L.snapshot(T)
    res[label] = {'Exit': rc, 'Sha256': L.sha256(out), 'EqualsRecorded': L.sha256(out) == want, 'RepoUnchanged': L.snapdiff(s0, s1) == {}}

# (3) T2 working-tree listing
T2 = L.WORK + '/r4/t2wt'
snap = L.snapshot(T2)
wf = snap['WorktreeFiles']
mine = L.sha256(L.canon(wf).encode())
rec_b = t2['Pipeline']['P1']['SnapshotBefore']['WorktreeFilesSha256']
rec_a = t2['Pipeline']['P1']['SnapshotAfter']['WorktreeFilesSha256']
norm = {}
changed = []
for rel, v in wf.items():
    b = L.rbytes(os.path.join(T2, rel))
    nb = to_orig(b)
    if nb != b:
        changed.append(rel)
    norm[rel] = [len(nb), L.sha256(nb)]
pred = L.sha256(L.canon(norm).encode())
res['t2-worktree-files'] = {'ReproNow': mine, 'EqualsRecordedBefore': mine == rec_b, 'EqualsRecordedAfter': mine == rec_a,
                            'EntriesWithRootPath': changed, 'Predicted': pred}
pairs = {}
if mine == rec_b == rec_a and changed == ['.git']:
    pairs[mine] = [pred, 'V5 recomputed listing: the T2 working-tree listing recomputed now equals the recorded one; '
                         'writing the repro root as the original root in its only path-bearing entry (.git, the gitdir pointer) predicts this value']
with open(pairs_out, 'w', encoding='ascii', newline='\n') as fh:
    fh.write(json.dumps(pairs, sort_keys=True, indent=1) + '\n')
print(json.dumps(res, sort_keys=True))

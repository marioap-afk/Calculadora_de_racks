# run_r1_extra.py - extra attack (i) for candidate R1: a tampered objects/info/commit-graph (git's parent cache).
# python -I run_r1_extra.py   (after run_r1.py; work area D:\r62-a6r2\r1r4\r1x; result r1-attacks-extra.json)
# P2 step (2) only tests grafts / alternates / shallow; this measures whether steps (1)-(4) see a lying commit-graph,
# what the tool reports and what the acceptor (own raw parent walk) does.
import hashlib, importlib.util, os, struct, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_s = importlib.util.spec_from_file_location('lib_r1r4', os.path.join(HERE, 'lib_r1r4.py'))
L = importlib.util.module_from_spec(_s)
_s.loader.exec_module(L)
_s2 = importlib.util.spec_from_file_location('run_r1', os.path.join(HERE, 'run_r1.py'))
R = importlib.util.module_from_spec(_s2)
_s2.loader.exec_module(R)
R.R1 = L.WORK + '/r1x'


def patch_commit_graph(path, child, new_parent):
    data = bytearray(L.rbytes(path))
    assert data[:4] == b'CGPH', 'not a commit-graph'
    nchunks = data[6]
    chunks = {}
    for i in range(nchunks):
        cid = bytes(data[8 + 12 * i:12 + 12 * i])
        chunks[cid] = struct.unpack('>Q', bytes(data[12 + 12 * i:20 + 12 * i]))[0]
    n = struct.unpack('>I', bytes(data[chunks[b'OIDF'] + 255 * 4:chunks[b'OIDF'] + 256 * 4]))[0]
    oids = [bytes(data[chunks[b'OIDL'] + 20 * i:chunks[b'OIDL'] + 20 * (i + 1)]).hex() for i in range(n)]
    pos = dict((o, i) for i, o in enumerate(oids))
    e = chunks[b'CDAT'] + 36 * pos[child]
    old = struct.unpack('>I', bytes(data[e + 20:e + 24]))[0]
    data[e + 20:e + 24] = struct.pack('>I', pos[new_parent])
    data[-20:] = hashlib.sha1(bytes(data[:-20])).digest()
    os.chmod(path, 0o666)
    L.wbytes(path, bytes(data))
    return {'Chunks': sorted(c.decode('ascii') for c in chunks), 'Commits': n, 'OldParent1Pos': old, 'NewParent1Pos': pos[new_parent],
            'OldParent1': oids[old] if old < n else None, 'NewParent1': new_parent}


def main():
    L.rmtree(R.R1)
    L.ensure_dir(R.R1)
    vers = L.versions()
    A = R.Attack('ti')
    w = L.git(A.T, 'commit-graph', 'write', '--reachable')
    cd = L.abspath_dir(A.T, '--git-common-dir')
    cg = os.path.join(cd, 'objects', 'info', 'commit-graph')
    rec = {'Attack': 'objects/info/commit-graph rewritten so that HEADX has BASE as first parent (RED dropped); trailer checksum recomputed',
           'Repo': A.T, 'P1': A.p1, 'P2Before': A.p2_before, 'CommitGraphWriteExit': w.returncode, 'CommitGraphExists': os.path.isfile(cg)}
    if os.path.isfile(cg):
        rec['Patch'] = patch_commit_graph(cg, L.HEADX, L.BASE)
    rec['GitViews'] = {'PlainRevList': L.gout(A.T, 'rev-list', L.BASE + '..' + L.HEADX).splitlines(),
                       'NoReplaceRevList': L.gout(A.T, '--no-replace-objects', 'rev-list', L.BASE + '..' + L.HEADX).splitlines(),
                       'NoCommitGraphRevList': L.gout(A.T, '-c', 'core.commitGraph=false', 'rev-list', L.BASE + '..' + L.HEADX).splitlines(),
                       'CatFileParent': [x for x in L.gout(A.T, 'cat-file', '-p', L.HEADX).splitlines() if x.startswith('parent ')],
                       'LogParents': L.gout(A.T, 'log', '-1', '--format=%P', L.HEADX),
                       'NoCommitGraphLogParents': L.gout(A.T, '-c', 'core.commitGraph=false', 'log', '-1', '--format=%P', L.HEADX),
                       'MergeBaseIsAncestorRedHeadxExit': L.git(A.T, 'merge-base', '--is-ancestor', L.RED, L.HEADX).returncode,
                       'NoCommitGraphMergeBaseIsAncestorRedHeadxExit': L.git(A.T, '-c', 'core.commitGraph=false', 'merge-base', '--is-ancestor', L.RED, L.HEADX).returncode,
                       'NoReplaceMergeBaseIsAncestorRedHeadxExit': L.git(A.T, '--no-replace-objects', 'merge-base', '--is-ancestor', L.RED, L.HEADX).returncode,
                       'CommitGraphVerifyExit': L.git(A.T, 'commit-graph', 'verify').returncode}
    p3 = A.run('p3', 'pwsh')
    rec['ToolFactsVsBaseline'] = A.compare(p3)
    rec['P2After'] = L.p2(A.T, 'pwsh')
    acc = A.accept('p3', p3, {'R4': 'ataque (i): sin salida del Controller', 'R6': 'sin corrida clausemap en el ataque'})
    rec['AcceptorOnP3'] = R.summary_acc(acc)
    rec['Held'] = {'tool facts change': any(v.get('FactsKeysChanged') for v in rec['ToolFactsVsBaseline'].values()),
                   'P2 steps 1-3 pass (not designed for this)': [k for k in rec['P2After']['FailedSteps'] if k in ('1', '2', '3')] == [],
                   'P2 step 4 (fsck) detects it': '4' in rec['P2After']['FailedSteps'],
                   'acceptor rejects (R5 MISMATCH)': acc['Decision'] == 'REJECT_FACTS' and acc['Checks'].get('R5') == 'MISMATCH'}
    print('i', rec['Held'], rec['GitViews'], rec['P2After']['FailedSteps'])
    L.write_result('r1-attacks-extra.json', {'Schema': 'r1r4-bench/r1-attacks-extra', 'Versions': vers, 'Attacks': {'i': rec}})
    print('done')


if __name__ == '__main__':
    main()

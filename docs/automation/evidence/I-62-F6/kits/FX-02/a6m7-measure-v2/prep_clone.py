# prep_clone.py - creates the -C of the A6-7 measurement (I-62), lib_kit.CDIR (kit v2: D:\r62-a6m7\m7-run2), deterministically from the r2 part (d)
# delivery repository D:\r62-a6r2\r3\deliv (read only), keeping its refs and history (git diff TIP..G needs both commits), places
# the input copies of the designation of M under the kit paths (KPR-2: delegation and delivery at their E paths; the TRX under
# the ignored run directory), refreshes the index stat data once, and records the state snapshot that the gates freeze
# (refs incl. refs/replace/, HEAD, index SHA-256, status with ignored, file list, git-dir files, local/worktree config).
# It never writes anything but D:\r62-a6m7\m7-run and the snapshot files under the kit. It does not write the instrument
# object (that is prep_write.py, A6-3 rule 1, only after the authorizing disposition).
#   python -I prep_clone.py [--dest D:/r62-a6m7/m7-run] [--snapshot-out <kit>/frozen/m7-run.snapshot.json] [--recreate]
import importlib.util, json, os, shutil, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


L = _load('lib_kit')


def fail(msg):
    sys.stderr.write('prep_clone: REFUSED: %s\n' % msg)
    sys.exit(2)


def create(dest, snap_out, recreate=False):
    destn = os.path.normcase(os.path.normpath(dest))
    if os.path.lexists(dest):
        if not recreate:
            fail('%s already exists (use --recreate only before any preparation write)' % dest)
        if os.path.exists(L.MARKER) or os.path.exists(L.OUT + '/prep-write.json'):
            fail('a preparation write or a launch is recorded: the -C is never recreated')
        if L.git(dest, 'cat-file', '-e', L.B).returncode == 0:
            fail('the -C already holds the instrument object: never recreated')
        L.rmtree(dest)
    for r, p in L.CUSTODY.items():
        if not os.path.isfile(p):
            fail('custodied input missing: ' + p)
    shutil.copytree(L.SRC_DELIV, dest)
    checks = {}
    T = dest
    checks['Head'] = L.gout(T, 'rev-parse', 'HEAD') == L.G
    checks['OriginBranch'] = L.gout(T, 'rev-parse', 'refs/remotes/origin/' + L.BR) == L.G
    checks['OriginMain'] = L.gout(T, 'rev-parse', 'refs/remotes/origin/main') == L.MAIN
    checks['AbbrevRef'] = L.gout(T, 'rev-parse', '--abbrev-ref', 'HEAD') == L.BR
    checks['AutocrlfFalse'] = L.gout(T, 'config', '--get', 'core.autocrlf') == 'false'
    checks['NotShallow'] = L.gout(T, 'rev-parse', '--is-shallow-repository') == 'false'
    checks['NoReplaceRefs'] = L.gout(T, 'for-each-ref', 'refs/replace/') == ''
    cd = os.path.normpath(L.gout(T, 'rev-parse', '--path-format=absolute', '--git-common-dir'))
    checks['NoGraftsNoAlternates'] = not os.path.lexists(os.path.join(cd, 'info', 'grafts')) and \
        not os.path.lexists(os.path.join(cd, 'objects', 'info', 'alternates'))
    checks['InstrumentObjectAbsent'] = L.git(T, 'cat-file', '-e', L.B).returncode != 0
    for c in (L.TIP, L.RED, L.G, L.MAIN, L.AR):
        checks['Commit:' + c[:8]] = L.gout(T, 'cat-file', '-t', c) == 'commit'
    checks['ContractBlobAtTarget'] = L.gout(T, 'rev-parse', L.G + ':' + L.K) == L.K_BLOB
    if not all(checks.values()):
        fail('unexpected copy of the delivery repository: ' + json.dumps(checks))
    # one refresh of the index stat data of the new copy (the copy changes creation times), before any snapshot
    r = L.git(T, 'update-index', '-q', '--refresh')
    if L.git(T, 'status', '--porcelain').stdout != b'':
        fail('the copy is not clean after refreshing the index')
    # input copies (byte-identical to the custodied ones)
    placed = {}
    for role, rel in sorted(L.PLACED.items()):
        dst = os.path.join(T, *rel.split('/'))
        L.wbytes(dst, L.rbytes(L.CUSTODY[role]))
        placed[role] = {'Path': rel, 'Sha256': L.sha256_file(dst), 'CustodySha256': L.sha256_file(L.CUSTODY[role])}
        if placed[role]['Sha256'] != placed[role]['CustodySha256']:
            fail('copy of %s differs from its custody' % role)
        if L.git(T, 'check-ignore', '-q', '--no-index', rel).returncode != 0:
            fail('%s copy is not in an ignored path: %s' % (role, rel))
    checks['IgnorePathIgnored'] = L.git(T, 'check-ignore', '-q', '--no-index', L.IGNORE_PATH).returncode == 0
    checks['StatusClean'] = L.git(T, 'status', '--porcelain').stdout == b''
    checks['StatusIgnored'] = L.glines(T, 'status', '--porcelain', '--ignored') == ['!! artifacts/']
    if not (checks['IgnorePathIgnored'] and checks['StatusClean'] and checks['StatusIgnored']):
        fail('unexpected status after placing the inputs: ' + json.dumps(checks))
    s1 = L.snapshot(T)
    s2 = L.snapshot(T)                      # stability: a second snapshot (with its status runs) must be identical
    if L.snapdiff(s1, s2):
        fail('the snapshot is not stable: ' + json.dumps(L.snapdiff(s1, s2))[:500])
    data = (L.canon(s1) + '\n').encode('ascii')
    if not L.hygienic(data.decode('ascii')):
        fail('snapshot would contain a user name or an address')
    L.wbytes(snap_out, data)
    rec = {'Schema': 'i62-a6m7-prep-clone/1', 'UtcTime': L.utc(), 'Source': L.SRC_DELIV, 'Dest': dest.replace('\\', '/'),
           'Method': 'shutil.copytree of the source (refs, packed-refs, objects, index, worktree), then git update-index -q --refresh once',
           'SourceHead': L.gout(L.SRC_DELIV, 'rev-parse', 'HEAD'), 'Checks': checks, 'Placed': placed,
           'Snapshot': {'Path': snap_out.replace('\\', '/'), 'Sha256': L.sha256(data), 'Refs': len(s1['Refs']),
                        'WorktreeFiles': len(s1['WorktreeFiles']), 'GitFiles': len(s1['GitFiles']), 'IndexSha256': s1['IndexSha256'],
                        'StatusZ': s1['StatusZ'], 'Config': s1['Config']},
           'SourceUnchanged': L.gout(L.SRC_DELIV, 'status', '--porcelain', '--ignored') == ''}
    L.wjson(os.path.splitext(snap_out)[0] + '.record.json', rec)
    print(json.dumps(rec, indent=1, sort_keys=True))
    return 0


def main(argv):
    dest, snap, recreate = L.CDIR, L.FROZEN + '/m7-run.snapshot.json', False
    i = 1
    while i < len(argv):
        if argv[i] == '--dest':
            dest = argv[i + 1]; i += 2
        elif argv[i] == '--snapshot-out':
            snap = argv[i + 1]; i += 2
        elif argv[i] == '--recreate':
            recreate = True; i += 1
        else:
            sys.stderr.write('usage: python -I prep_clone.py [--dest D] [--snapshot-out F] [--recreate]\n')
            return 64
    return create(dest, snap, recreate)


if __name__ == '__main__':
    sys.exit(main(sys.argv))

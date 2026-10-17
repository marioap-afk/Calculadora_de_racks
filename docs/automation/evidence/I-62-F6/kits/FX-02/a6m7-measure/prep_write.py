# prep_write.py - preparation write of A6-3 rule 1 for the A6-7 measurement (I-62): the supervision obtains the instrument
# bytes by its blob from the I-62 repository (never from a working tree) and writes them in the repository of the -C only as a
# loose Git object (git hash-object -w --no-filters --stdin): no file in the tree, no index, no commit, no ref, no push.
# Only after the authorizing disposition: the Coordinator's measurement disposition (all markers, incl. M7-ESCRITURA with the
# DecisionRef) must be in the decisions file. Before writing it checks (i), (ii) and (vii) of A6-3 rule 3 and that the -C is
# still the frozen snapshot; it records the DecisionRef, the repository, the UTC time, the returned id and a snapshot before
# and after, which may differ in exactly one new loose object. Any failure: no write (or, after it, a record with Held false).
#   python -I prep_write.py [--repo D:/r62-a6m7/m7-run] [--decisions <file>] [--out-dir <kit>/out] [--snapshot <frozen>] [--kit <kit>]
# --decisions/--repo/--out-dir/--snapshot other than the defaults are for the self-test on a scratch copy only.
import importlib.util, json, os, subprocess, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


L = _load('lib_kit')
G = _load('gates_m7')
pc = _load('pou_check')


def refuse(msg, rec, out_dir):
    rec['Refused'] = msg
    rec['Held'] = False
    os.makedirs(out_dir, exist_ok=True)
    L.wjson(os.path.join(out_dir, 'prep-write.refused.json'), rec)
    sys.stderr.write('prep_write: REFUSED (nothing written): %s\n' % msg)
    return 2


def main(argv):
    a = dict(zip(argv[1::2], argv[2::2]))
    p = L.params()
    repo = a.get('--repo', p['Repo'])
    out_dir = a.get('--out-dir', L.OUT)
    snap_path = a.get('--snapshot', L.FROZEN + '/m7-run.snapshot.json')
    cfg = G.Config()
    if '--decisions' in a:
        cfg.decisions = a['--decisions']
    if '--kit' in a:
        cfg.kit = a['--kit']
    git = L.expand(p['Git']['Path'])
    rec = {'Schema': 'i62-a6m7-prep-write/1', 'Repo': repo, 'Blob': L.B}
    if os.path.isfile(os.path.join(out_dir, 'prep-write.json')):
        return refuse('a preparation write is already recorded (it is done once)', rec, out_dir)
    # the authorizing disposition (all markers; M7-ESCRITURA carries the DecisionRef)
    dec = G.check_decisions(cfg)
    rec['Decisions'] = {'Ok': dec['Ok'], 'Problems': dec['Problems'], 'Disposition': dec.get('Disposition')}
    if not dec['Ok']:
        return refuse('no authorizing disposition: ' + '; '.join(dec['Problems']), rec, out_dir)
    rec['DecisionRef'] = L.DECISION_REF
    # (i) no refs/replace/, (ii) no grafts / alternates / shallow, (vii) config = the frozen one, no include/includeIf
    pre = {}
    r = L.git(repo, 'for-each-ref', 'refs/replace/', exe=git)
    pre['i'] = r.returncode == 0 and r.stdout.strip() == b''
    cd = os.path.normpath(L.gout(repo, 'rev-parse', '--path-format=absolute', '--git-common-dir', exe=git))
    pre['ii'] = (not os.path.lexists(os.path.join(cd, 'info', 'grafts')) and not os.path.lexists(os.path.join(cd, 'objects', 'info', 'alternates'))
                 and L.gout(repo, 'rev-parse', '--is-shallow-repository', exe=git) == 'false')
    frozen = L.rjson(snap_path)
    cfgnow = pc.snapshot_config(git, os.path.normpath(repo))
    inc = [role for role, f in cfgnow['Files'].items() if f.get('IncludeKeys') or f.get('RawIncludeSection')]
    same_cfg = all(cfgnow['Files'][role]['Exists'] == frozen['Config'][role]['Exists'] and
                   cfgnow['Files'][role].get('Sha256') == frozen['Config'][role]['Sha256'] for role in ('Local', 'Worktree'))
    pre['vii'] = same_cfg and not inc
    before = L.snapshot(repo, exe=git)
    pre['FrozenState'] = L.snapdiff(frozen, before) == {}
    pre['ObjectAbsent'] = L.git(repo, 'cat-file', '-e', L.B, exe=git).returncode != 0
    rec['PreChecks'] = pre
    if not all(pre.values()):
        return refuse('pre-checks failed: ' + ','.join(k for k, v in pre.items() if not v), rec, out_dir)
    data = L.pinned_bytes(L.B, L.B_SHA256)                # by blob, from the I-62 repository
    rec['Source'] = {'Command': 'git --no-replace-objects -C %USERPROFILE%/' + L.W_REL + ' cat-file blob ' + L.B,
                     'BlobSha1': L.blob_sha1(data), 'Sha256': L.sha256(data), 'Size': len(data)}
    rec['UtcTime'] = L.utc()
    w = subprocess.run([git, '-C', repo, 'hash-object', '-w', '--no-filters', '--stdin'], input=data, capture_output=True, env=L.genv())
    oid = w.stdout.decode('ascii', 'replace').strip()
    after = L.snapshot(repo, exe=git)
    d = L.snapdiff(before, after)
    rec.update({'WriteCommand': 'git -C <repo> hash-object -w --no-filters --stdin', 'WriteExit': w.returncode, 'ReturnedId': oid,
                'IdEqualsPinned': oid == L.B, 'SnapshotBeforeSha256': L.snap_sha(before), 'AfterSnapshotSha256': L.snap_sha(after),
                'SnapshotDiff': d, 'OnlyNewLooseObject': L.only_loose_object(d, L.B)})
    rec['Held'] = bool(rec['IdEqualsPinned'] and rec['OnlyNewLooseObject'] and w.returncode == 0)
    os.makedirs(out_dir, exist_ok=True)
    L.wjson(os.path.join(out_dir, 'prep-write.json'), rec)
    # registered configuration of the preparation write (A6-3 rule 3 (vii) compares against it at the point of use)
    reg = pc.snapshot_config(git, os.path.normpath(repo))
    L.wbytes(os.path.join(out_dir, 'registered-config.json'), (json.dumps(L.sanitize(reg), ensure_ascii=True, sort_keys=True) + '\n').encode('ascii'))
    print(json.dumps({'Held': rec['Held'], 'ReturnedId': oid, 'OnlyNewLooseObject': rec['OnlyNewLooseObject'], 'DecisionRef': rec['DecisionRef']}))
    return 0 if rec['Held'] else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv))

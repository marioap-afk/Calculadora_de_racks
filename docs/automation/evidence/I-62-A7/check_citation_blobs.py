# check_citation_blobs.py - the fail-closed precondition that the candidate I-62 A-7 (§3.5, «Residuo declarado») asks the kit
# to check before every FX-02 VERIFY launch: the pinned instrument's `clausemap` also compares every citation of the contract at
# EFF^1, EFF and MainSha_eval; «si alguna lectura de contenido que `clausemap` hace de una cita del contrato cae en una revisión
# en que el blob de esa ruta no es el de una entrada del cierre, no hay lanzamiento y el caso vuelve al Coordinator (S-03); una
# revisión en que la ruta no existe no aporta lectura». Stdlib only; read-only git; independent of the builder and the validator.
#
#   python -I check_citation_blobs.py <designation.json> --e=<E.json> [--mode=coverage|text|unit]
#   exit 0 = launch may proceed, 1 = no launch (S-03), 2 = cannot be evaluated (no launch), 64 = usage.
#
# coverage  (default, the only normative mode): every content read clausemap makes of a citation at EFF^1, EFF or MainSha_eval
#           (where the path exists) is of a blob that E pins for that path. Needs the closure E (--e).
# text      negative evidence only (the round-1 wording before the second correction): every citation with one and the same
#           blob at the AuthorityRevision, EFF, EFF^1 and MainSha_eval (revisions where the path does not exist not compared).
# unit      negative evidence only: the same, only for UNIT_DOC / UNIT_CHANGE citations.
# The contract comes from the designation (custodied bytes, or the Git object of its registered blob); MainSha_eval =
# K.MainSha (or the designation's MainShaEval); EFF = Derive(MainSha_eval) (AP 16.13 / 16.14), EFF^1 its first parent.
import hashlib, json, os, re, subprocess, sys

sys.dont_write_bytecode = True
TRAILER = 'Agent-Protocol-Normative: I-62'
HEX40 = re.compile(r'[0-9a-f]{40}\Z')


def git(repo, *args):
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    env['GIT_OPTIONAL_LOCKS'] = '0'
    p = subprocess.run(['git', '--no-replace-objects', '-C', repo] + list(args), capture_output=True, stdin=subprocess.DEVNULL, env=env)
    return p.returncode, p.stdout


def blob_at(repo, rev, path):
    rc, out = git(repo, 'ls-tree', '-z', '--full-tree', rev, '--', ':(literal)' + path)
    for rec in out.split(b'\0'):
        if rec:
            meta, name = rec.split(b'\t', 1)
            if name.decode('utf-8', 'surrogateescape') == path and meta.split()[1] == b'blob':
                return meta.split()[2].decode('ascii')
    return None


def is_commit(repo, v):
    if not (isinstance(v, str) and HEX40.match(v)):
        return False
    rc, out = git(repo, 'cat-file', '-t', v)
    return rc == 0 and out.strip() == b'commit'


def derive(repo, main):
    rc, out = git(repo, 'log', '--no-show-signature', '-z', '--format=%H%n%B', main)
    tr = []
    for rec in out.split(b'\0'):
        t = rec.decode('utf-8', 'replace').replace('\r\n', '\n').lstrip('\n')
        if t.strip():
            sha, _, body = t.partition('\n')
            if TRAILER in body.split('\n'):
                tr.append(sha.strip())
    if len(tr) != 1:
        return (None, None) if not tr else ('AMBIGUOUS', None)
    rc, out = git(repo, 'rev-list', '--parents', main)
    par = dict((ln.split()[0], ln.split()[1:]) for ln in out.decode('ascii').split('\n') if ln.strip())
    kids = {}
    for c, ps in par.items():
        for p in ps:
            kids.setdefault(p, []).append(c)
    seen, todo = {tr[0]}, [tr[0]]
    while todo:
        for k in kids.get(todo.pop(), []):
            if k not in seen:
                seen.add(k)
                todo.append(k)
    rc, out = git(repo, 'rev-list', '--first-parent', '--merges', main)
    c = [m for m in out.decode('ascii').split() if len(par.get(m, [])) >= 2 and par[m][1] in seen and par[m][0] not in seen]
    return (c[0], par[c[0]][0]) if len(c) == 1 else ('AMBIGUOUS', None)


def contract_bytes(des):
    k = next((i for i in des.get('Inputs') or [] if isinstance(i, dict) and i.get('Role') == 'CONTRACT'), None)
    if k is None:
        return None
    if k.get('Custody'):
        try:
            with open(k['Custody'], 'rb') as fh:
                b = fh.read()
        except OSError:
            return None
    else:
        rc, b = git(des['Repo'], 'cat-file', 'blob', str(k.get('Blob')))
        if rc != 0:
            return None
    if k.get('Blob') and hashlib.sha1(b'blob %d\0' % len(b) + b).hexdigest() != k['Blob']:
        return None
    return b


def check(des, mode='coverage', E=None):
    repo = des['Repo']
    out = {'Mode': mode, 'Citations': [], 'Blocking': [], 'Result': None}
    kb = contract_bytes(des)
    try:
        K = json.loads(kb.decode('utf-8')) if kb is not None else None
    except (UnicodeDecodeError, ValueError):
        K = None
    if not isinstance(K, dict) or not isinstance(K.get('Authorities'), list):
        out['Result'] = 'CANNOT_EVALUATE'
        out['Why'] = 'contract absent, with another blob, unreadable or without Authorities'
        return out
    ar = K.get('AuthorityRevision') or des.get('AuthorityRevision')
    mse = des.get('MainShaEval') or K.get('MainSha')
    if not (is_commit(repo, ar) and is_commit(repo, mse)):
        out['Result'] = 'CANNOT_EVALUATE'
        out['Why'] = 'AuthorityRevision or MainSha_eval is not a commit'
        return out
    eff, eff1 = derive(repo, mse)
    if eff in (None, 'AMBIGUOUS'):
        out['Result'] = 'CANNOT_EVALUATE'
        out['Why'] = 'EFF not derivable from MainSha_eval'
        return out
    revs = {'AR': ar, 'EFF': eff, 'EFF^1': eff1, 'MainSha_eval': mse}
    out['Revisions'] = revs
    pinned = {}
    if E is not None:
        for k in ('CanonicalInputs', 'AllowedTransitiveInputs', 'AutomaticInstructions'):
            for e in E.get(k) or []:
                pinned.setdefault(e.get('Path'), set()).add(e.get('Blob'))
    for a in K['Authorities']:
        if not isinstance(a, dict) or not isinstance(a.get('Path'), str):
            continue
        p, cls = a['Path'], a.get('Class')
        blobs = dict((r, blob_at(repo, c, p)) for r, c in revs.items())
        present = sorted(set(b for b in blobs.values() if b))
        row = {'Path': p, 'Class': cls, 'Blobs': blobs, 'Distinct': len(present)}
        if mode == 'coverage':
            if E is None:
                out['Result'] = 'CANNOT_EVALUATE'
                out['Why'] = 'coverage needs E'
                return out
            outside = [r for r in ('EFF^1', 'EFF', 'MainSha_eval') if blobs[r] and blobs[r] not in pinned.get(p, set())]
            row['ReadsOutsideE'] = outside
            if outside:
                out['Blocking'].append({'Path': p, 'Class': cls, 'Why': 'clausemap reads %s at %s, not pinned by E' % (p, ', '.join(outside))})
        elif mode == 'unit' and cls not in ('UNIT_DOC', 'UNIT_CHANGE'):
            row['Compared'] = False
        elif len(present) > 1:
            out['Blocking'].append({'Path': p, 'Class': cls, 'Why': 'different blobs at %s' % ', '.join('%s %s' % (r, (b or '-')[:8]) for r, b in blobs.items())})
        out['Citations'].append(row)
    out['Result'] = 'LAUNCH_MAY_PROCEED' if not out['Blocking'] else 'NO_LAUNCH_S-03'
    return out


def main(argv):
    opts = dict(a[2:].split('=', 1) for a in argv[1:] if a.startswith('--') and '=' in a)
    args = [a for a in argv[1:] if not a.startswith('--')]
    if len(args) != 1 or opts.get('mode', 'coverage') not in ('text', 'unit', 'coverage'):
        sys.stderr.write('usage: python -I check_citation_blobs.py <designation.json> --e=<E.json> [--mode=coverage|text|unit]\n')
        return 64
    with open(args[0], 'rb') as fh:
        des = json.loads(fh.read().decode('utf-8'))
    E = None
    if opts.get('e'):
        with open(opts['e'], 'rb') as fh:
            E = json.loads(fh.read().decode('utf-8'))
    r = check(des, opts.get('mode', 'coverage'), E)
    sys.stdout.write(json.dumps(r, ensure_ascii=True, sort_keys=True) + '\n')
    return {'LAUNCH_MAY_PROCEED': 0, 'NO_LAUNCH_S-03': 1}.get(r['Result'], 2)


if __name__ == '__main__':
    sys.exit(main(sys.argv))

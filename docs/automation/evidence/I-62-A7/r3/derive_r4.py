# derive_r4.py - acceptor-closure derivation of A-6 round 4 (I-62): the A6-9 table (rule 4, unchanged) applied with
# A6-3 rule 6 (b) (i)-(v) when an input gives no value. Stdlib only; every git call read-only with --no-replace-objects.
#   python -I derive_r4.py <vector.json> <out.json>
# Vector and output follow the interface of SPEC.md (shared with the independent implementation, which was written apart).
import hashlib, json, os, re, subprocess, sys

HEX40 = re.compile(r'[0-9a-f]{40}\Z')
DEFAULT_MAP = 'docs/automation/agent-execution/compatibility/I62-clause-map.json'
ROLES = ('D', 'H', 'K')


def git(repo, *args):
    p = subprocess.run(['git', '--no-replace-objects', '-C', repo] + list(args), capture_output=True, stdin=subprocess.DEVNULL)
    return p.returncode, p.stdout


def gtext(repo, *args):
    rc, out = git(repo, *args)
    return rc, out.decode('ascii', 'replace').strip()


def blob_id(b):
    return hashlib.sha1(b'blob ' + str(len(b)).encode('ascii') + b'\0' + b).hexdigest()


def sha256(b):
    return hashlib.sha256(b).hexdigest()


class Dup(ValueError):
    pass


def _pairs(pairs):
    d = {}
    for k, v in pairs:
        if k in d:
            raise Dup(k)
        d[k] = v
    return d


def strict_object(b):
    """(object, None) or (None, why): one JSON object, strict UTF-8 without BOM, no repeated key at any depth."""
    try:
        s = b.decode('utf-8', 'strict')
    except UnicodeDecodeError:
        return None, 'not UTF-8'
    if s.startswith('﻿'):
        return None, 'BOM'
    try:
        o = json.loads(s, object_pairs_hook=_pairs)
    except Dup as e:
        return None, 'repeated key ' + str(e)
    except ValueError as e:
        return None, 'not JSON: ' + str(e)[:80]
    if not isinstance(o, dict):
        return None, 'not an object'
    return o, None


class Derivation(object):
    def __init__(self, vec):
        self.repo = vec['Repo']
        with open(vec['E'], 'rb') as fh:
            self.E = json.loads(fh.read().decode('utf-8'))
        self.I = vec['I']
        self.des = vec['Designation']
        self.cust = vec.get('Custody') or {}
        self.map_path = vec.get('MapPath') or DEFAULT_MAP
        self.entries = [e for k in ('CanonicalInputs', 'AllowedTransitiveInputs', 'AutomaticInstructions') for e in (self.E.get(k) or [])]
        self.omitted = []
        self.inputs = {}
        self.input_notes = {}
        self._commit_cache = {}
        self._trees = {}

    # -------------------------------------------------------------- inputs (rule (i))
    def entry(self, path):
        for e in self.entries:
            if e.get('Path') == path:
                return e
        return None

    def raw_bytes(self, path, blob):
        if path in self.cust:
            f = self.cust[path]
            if not os.path.isfile(f):
                return None
            with open(f, 'rb') as fh:
                return fh.read()
        rc, out = git(self.repo, 'cat-file', 'blob', blob)
        return out if rc == 0 else None

    def load_inputs(self):
        for r in ROLES:
            path = self.des.get(r)
            e = self.entry(path) if path else None
            if e is None:
                self.inputs[r], self.input_notes[r] = None, 'E does not pin the designated path'
                continue
            b = self.raw_bytes(path, e['Blob'])
            if b is None:
                self.inputs[r], self.input_notes[r] = None, 'custodied bytes do not exist'
                continue
            if blob_id(b) != e['Blob']:
                self.inputs[r], self.input_notes[r] = None, 'bytes do not have the pinned blob'
                continue
            o, why = strict_object(b)
            self.inputs[r], self.input_notes[r] = o, why

    def absent(self, *roles):
        return [r for r in roles if self.inputs.get(r) is None]

    # -------------------------------------------------------------- values (rule (ii))
    def omit(self, term, cause, detail=''):
        self.omitted.append({'Term': term, 'Cause': cause, 'Detail': detail})

    @staticmethod
    def field(obj, name):
        if name not in obj:
            return None, 'FIELD_MISSING'
        v = obj[name]
        if v is None:
            return None, 'FIELD_NULL'
        if v == '' or v == []:
            return None, 'FIELD_EMPTY'
        return v, None

    def is_commit(self, v):
        if not (isinstance(v, str) and HEX40.match(v)):
            return False
        if v not in self._commit_cache:
            rc, t = gtext(self.repo, 'cat-file', '-t', v)
            self._commit_cache[v] = rc == 0 and t == 'commit'
        return self._commit_cache[v]

    def commit_value(self, role, name):
        """(value, cause) for a commit field of E, D, H, K or I; cause None means the value counts."""
        if role in ROLES:
            obj = self.inputs.get(role)
            if obj is None:
                return None, 'INPUT_ABSENT'
        elif role == 'E':
            obj = self.E
        else:
            obj = self.I
        if role == 'I' and name == 'Target.Commit':
            t, c = self.field(obj, 'Target')
            if c:
                return None, c
            if not isinstance(t, dict):
                return None, 'FIELD_EMPTY'
            obj, name = t, 'Commit'
        v, c = self.field(obj, name)
        if c:
            return None, c
        if not self.is_commit(v):
            return None, 'NOT_A_COMMIT'
        return v, None

    # -------------------------------------------------------------- graph primitives (rule (iii))
    def revlist(self, a, b):
        rc, t = gtext(self.repo, 'rev-list', a + '..' + b)
        if rc != 0:
            raise RuntimeError('rev-list failed for existing commits')
        return t.split()

    def first_parent_chain(self, x):
        rc, t = gtext(self.repo, 'rev-list', '--first-parent', x)
        if rc != 0:
            raise RuntimeError('rev-list --first-parent failed')
        chain = t.split()
        extra = []
        for m in chain:
            rc, t2 = gtext(self.repo, 'rev-list', '--parents', '-n', '1', m)
            ps = t2.split()[1:]
            if len(ps) >= 2:
                extra += ps
        return chain + extra

    def merge_base(self, x, y):
        rc, t = gtext(self.repo, 'merge-base', x, y)
        return t if rc == 0 and t else None

    def blob_at(self, c, p):
        """Blob(c:p): the blob id at path p of the tree of commit c (None if absent or not a blob); one recursive listing per commit."""
        if c not in self._trees:
            rc, out = git(self.repo, 'ls-tree', '-r', '-z', '--full-tree', c)
            if rc != 0:
                raise RuntimeError('ls-tree failed for an existing commit')
            t = {}
            for rec in out.split(b'\0'):
                if not rec:
                    continue
                meta, name = rec.split(b'\t', 1)
                mode, typ, oid = meta.decode('ascii').split()
                t[name.decode('utf-8', 'surrogateescape')] = (typ, oid)
            self._trees[c] = t
        typ, oid = self._trees[c].get(p, (None, None))
        return oid if typ == 'blob' else None

    # -------------------------------------------------------------- the table
    def derive(self):
        self.load_inputs()
        commits = set()

        def term(label, role, name):
            v, c = self.commit_value(role, name)
            if c:
                self.omit(label, c)
            else:
                commits.add(v)
            return v

        term('C1.E.AuthorityRevision', 'E', 'AuthorityRevision')
        d_base = term('C1.D.BaseSha', 'D', 'BaseSha')
        d_main = term('C1.D.MainSha', 'D', 'MainSha')
        term('C1.D.ChainBaseSha', 'D', 'ChainBaseSha')
        term('C1.D.ChainRedSha', 'D', 'ChainRedSha')
        term('C1.H.BaseSha', 'H', 'BaseSha')
        h_cur = term('C1.H.CurrentSha', 'H', 'CurrentSha')
        term('C1.H.RedSha', 'H', 'RedSha')
        term('C1.I.Target.Commit', 'I', 'Target.Commit')
        k_ar = term('C1.K.AuthorityRevision', 'K', 'AuthorityRevision')
        term('C1.K.MainSha', 'K', 'MainSha')
        # primitives: INPUT_ABSENT wins; then an omitted end or no result = PRIMITIVE_EMPTY
        lab = 'C1.RevList(D.BaseSha..H.CurrentSha)'
        if self.absent('D', 'H'):
            self.omit(lab, 'INPUT_ABSENT')
        elif d_base is None or h_cur is None:
            self.omit(lab, 'PRIMITIVE_EMPTY', 'omitted end')
        else:
            commits.update(self.revlist(d_base, h_cur))
        lab = 'C1.FirstParentChain(D.MainSha)'
        if self.absent('D'):
            self.omit(lab, 'INPUT_ABSENT')
        elif d_main is None:
            self.omit(lab, 'PRIMITIVE_EMPTY', 'omitted end')
        else:
            commits.update(self.first_parent_chain(d_main))
        lab = 'C1.MergeBase(D.MainSha,K.AuthorityRevision)'
        if self.absent('D', 'K'):
            self.omit(lab, 'INPUT_ABSENT')
        elif d_main is None or k_ar is None:
            self.omit(lab, 'PRIMITIVE_EMPTY', 'omitted end')
        else:
            mb = self.merge_base(d_main, k_ar)
            if mb is None:
                self.omit(lab, 'PRIMITIVE_EMPTY', 'no common ancestor')
            else:
                commits.add(mb)
        # C2
        refs = ['HEAD']
        if self.absent('D'):
            self.omit('C2.origin/D.ExpectedBranch', 'INPUT_ABSENT')
        else:
            br, c = self.field(self.inputs['D'], 'ExpectedBranch')
            if c:
                self.omit('C2.origin/D.ExpectedBranch', c)
            else:
                refs.append('refs/remotes/origin/' + br)
        refs.append('refs/remotes/origin/main')
        # C3
        want = {}
        for e in self.entries:
            want.setdefault(e['Path'], set()).add(e['Blob'])
        revpaths = []
        for c in sorted(commits):
            for p in sorted(want):
                b = self.blob_at(c, p)
                if b is not None and b in want[p]:
                    revpaths.append({'Rev': c, 'Path': p})
        # C4
        paths = set(e['Path'] for e in self.entries)
        paths.update(o['Target'] for o in (self.E.get('Obligations') or []) if o.get('Class') == 'READ' and o.get('Target'))
        me = self.entry(self.map_path)
        m = {}
        if me is not None:
            mb_ = self.raw_bytes(self.map_path, me['Blob'])
            if mb_ is not None and blob_id(mb_) == me['Blob']:
                m = json.loads(mb_.decode('utf-8'))
        paths.update(m.get('Surfaces') or [])
        paths.update(f['Path'] for f in (m.get('Files') or []))
        if self.absent('K'):
            self.omit('C4.K.Authorities', 'INPUT_ABSENT')
        else:
            au = self.inputs['K'].get('Authorities', None)
            if 'Authorities' not in self.inputs['K']:
                self.omit('C4.K.Authorities', 'FIELD_MISSING')
            elif au is None:
                self.omit('C4.K.Authorities', 'FIELD_NULL')
            else:
                paths.update(a['Path'] for a in au)
        ign = []
        for n in ('UnitId', 'TaskId', 'Attempt', 'RunId'):
            v, c = self.field(self.I, n)
            if c:
                self.omit('C4.IgnorePath(I)', c, n)
                ign = None
                break
            ign.append(str(v))
        if ign is not None:
            paths.add('artifacts/orchestration/%s/%s/%s/%s/output.json' % tuple(ign))
        # C6
        files = []
        rid = {}
        for nm, r in (('DELEG', 'D'), ('H', 'H')):
            if self.absent(r):
                self.omit('C6.' + nm, 'INPUT_ABSENT')
                continue
            v, c = self.field(self.inputs[r], 'RunId')
            if c:
                self.omit('C6.' + nm, c)
                continue
            rid[nm] = v
        if self.absent('K'):
            self.omit('C6.CONTRACT', 'INPUT_ABSENT')
        for e in (self.E.get('CanonicalInputs') or []):
            p = e['Path']
            if p == self.des.get('K'):
                if self.absent('K'):
                    continue
                n = 'CONTRACT'
            elif 'DELEG' in rid and re.fullmatch(r'artifacts/orchestration/[^/]+/[^/]+/[0-9]+/%s/output\.json' % re.escape(rid['DELEG']), p):
                n = 'DELEG'
            elif 'H' in rid and re.fullmatch(r'artifacts/orchestration/[^/]+/[^/]+/[0-9]+/%s/worker-handoff\.json' % re.escape(rid['H']), p):
                n = 'H'
            elif p.endswith('.trx'):
                n = 'TRX'
            else:
                continue
            b = self.raw_bytes(p, e['Blob'])
            if b is None or blob_id(b) != e['Blob']:
                if n == 'TRX':
                    raise RuntimeError('TRX bytes do not verify: ' + p)
                continue
            files.append({'Name': n, 'Sha256': sha256(b)})
        closure = {'Schema': 'rackcad-bmin-closure/0-experimental', 'Commits': sorted(commits), 'RevPaths': revpaths, 'Refs': refs,
                   'Paths': sorted(paths), 'Objects': [], 'Files': files}
        return closure, self.omitted


def derive_vector(vec):
    d = Derivation(vec)
    closure, omitted = d.derive()
    return {'Closure': closure, 'Omitted': omitted,
            'Inputs': dict((r, {'Present': d.inputs.get(r) is not None, 'Why': d.input_notes.get(r)}) for r in ROLES)}


def main(argv):
    if len(argv) != 3:
        print('usage: derive_r4.py <vector.json> <out.json>', file=sys.stderr)
        return 64
    with open(argv[1], 'rb') as fh:
        vec = json.loads(fh.read().decode('utf-8'))
    out = derive_vector(vec)
    with open(argv[2], 'wb') as fh:
        fh.write((json.dumps(out, ensure_ascii=True, indent=1, sort_keys=True) + '\n').encode('ascii'))
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))

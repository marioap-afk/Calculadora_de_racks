# compare.py - field-level comparison of reproduced result JSON files against the custodied ones (I-62 A-6 round 2).
# python -I compare.py <pkg: r1r4|r3> <custodied results dir> <repro results dir> <out json> <file> [<file> ...]
#
# Read-only on everything except <out json>. The original work root is READ (never written) only to explain digest
# differences: a differing SHA-256 is accepted as "expected" only when it is shown to be the digest of content that
# differs solely by work-root paths, timestamps, PIDs or other already-explained digests.
import base64, fnmatch, hashlib, json, os, re, subprocess, sys

sys.dont_write_bytecode = True
SEP = r'(?:/|\\\\|\\)'
HEX = re.compile(r'^(?:[0-9a-f]{40}|[0-9a-f]{64})$')
HEXTOK = re.compile(r'(?<![0-9a-f])([0-9a-f]{64}|[0-9a-f]{40})(?![0-9a-f])')
TS = re.compile(r'\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?')
PID_KEYS = {'Pid', 'ParentPid', 'CheckerPid', 'LaunchedPid'}
DURATION_KEYS = {'Seconds'}
GIT_STATE_KEYS = {'IndexSha256', 'GitFilesSha256'}
MISSING = '<missing>'
EXPECTED = {'DURATION', 'PATH', 'TIMESTAMP', 'PATH+TIMESTAMP', 'PID', 'HOST_PROCESS_TABLE', 'PATH_TRUNCATION', 'DIGEST_VERIFIED',
            'STRING_WITH_VERIFIED_DIGESTS', 'DERIVED_FROM_COMPARED_FILE'}


def sha256(b):
    return hashlib.sha256(b).hexdigest()


def canon(o):
    return json.dumps(o, ensure_ascii=True, sort_keys=True, separators=(',', ':'))


def rbytes(p):
    with open(p, 'rb') as fh:
        return fh.read()


def genv():
    e = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    e['GIT_OPTIONAL_LOCKS'] = '0'
    e['GIT_TERMINAL_PROMPT'] = '0'
    e['GIT_NO_REPLACE_OBJECTS'] = '1'
    return e


def git(*args):
    p = subprocess.run(['git'] + list(args), capture_output=True, env=genv(), stdin=subprocess.DEVNULL)
    return p.returncode, p.stdout


# ------------------------------------------------------------------ hygiene of what this script writes
_EMAIL = re.compile(r'[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}')


def _user_forms():
    prof = os.environ.get('USERPROFILE') or ''
    name = os.path.basename(prof.rstrip('\\/'))
    core = ''.join(c for c in name if c not in ' .').upper()
    forms = []
    if len(prof) > 3:
        forms += [(prof, '%USERPROFILE%'), (prof.replace('\\', '/'), '%USERPROFILE%')]
    if len(name) > 2:
        forms += [(name, '<usuario>'), (core[:6] + '~1', '<usuario>')]
    return forms


_FORMS = _user_forms()


def sanitize_str(s):
    for f, r in _FORMS:
        s = re.sub(re.escape(f), r, s, flags=re.I)
    return _EMAIL.sub('<email>', s)


def sanitize(o):
    if isinstance(o, dict):
        return dict((sanitize_str(k), sanitize(v)) for k, v in o.items())
    if isinstance(o, list):
        return [sanitize(x) for x in o]
    if isinstance(o, str):
        return sanitize_str(o)
    return o


# ------------------------------------------------------------------ configuration
class Cfg(object):
    def __init__(self, pkg):
        self.pkg = pkg
        self.orig_root = 'D:/r62-a6r2/' + pkg
        self.repro_root = 'D:/r62-a6r2/repro/' + pkg
        n = re.escape(pkg)
        self.root_re = re.compile(r'D:' + SEP + r'r62-a6r2' + SEP + r'(?:repro' + SEP + r')?' + n + r'(?![A-Za-z0-9])', re.I)
        self.here_re = re.compile(r'a6r2' + SEP + r'ev' + SEP + r'(?:repro' + SEP + r')?' + n + r'(?![A-Za-z0-9])', re.I)
        nb = pkg.encode('ascii')
        self.sub_root = re.compile(rb'([Dd]:)(/|\\\\|\\)r62-a6r2\2repro\2' + re.escape(nb) + rb'(?![A-Za-z0-9])')
        self.sub_here = re.compile(rb'(a6r2)(/|\\\\|\\)ev\2repro\2' + re.escape(nb) + rb'(?![A-Za-z0-9])')

    def norm_paths(self, s):
        s = self.root_re.sub('<WORKROOT>', s)
        return self.here_re.sub('a6r2/ev/<RESULTS>', s)

    def to_orig_bytes(self, b):
        """Repro bytes with the repro roots written as the original roots (separator form preserved)."""
        b = self.sub_root.sub(lambda m: m.group(1) + m.group(2) + b'r62-a6r2' + m.group(2) + self.pkg.encode('ascii'), b)
        return self.sub_here.sub(lambda m: m.group(1) + m.group(2) + b'ev' + m.group(2) + self.pkg.encode('ascii'), b)


def walk_files(root, prefix=''):
    out = {}
    for r, ds, fs in os.walk(root):
        ds[:] = [d for d in ds if d != '__pycache__']
        for f in fs:
            p = os.path.join(r, f)
            out[prefix + os.path.relpath(p, root).replace(os.sep, '/')] = p
    return out


# ------------------------------------------------------------------ engine
class Engine(object):
    def __init__(self, cfg, cust_dir, repro_dir, extra_repro_files=()):
        self.cfg = cfg
        self.cust_dir, self.repro_dir = cust_dir, repro_dir
        self.pairs = {}          # verified: repro digest -> custodied digest
        self.pair_how = {}
        self.failed_pairs = {}
        self.equiv_memo = {}
        self.depth = 0
        # file inventories (orig side READ only)
        self.orig_files = walk_files(cfg.orig_root) if os.path.isdir(cfg.orig_root) else {}
        self.repro_files = walk_files(cfg.repro_root)
        for k, v in walk_files(cust_dir, 'RESULTS:').items():
            self.orig_files[k] = v
        for k, v in walk_files(repro_dir, 'RESULTS:').items():
            self.repro_files[k] = v
        self.orig_sha, self.repro_sha = {}, {}
        self.orig_bytes_sha = {}
        for side, files, idx in (('o', self.orig_files, self.orig_sha), ('r', self.repro_files, self.repro_sha)):
            for rel, p in files.items():
                try:
                    h = sha256(rbytes(p))
                except OSError:
                    continue
                idx.setdefault(h, []).append(rel)
                if side == 'o':
                    self.orig_bytes_sha[rel] = h
        self.extra_repro_files = list(extra_repro_files)
        self.subst = self.build_subst()
        self.ext_pairs = {}

    # ---------------------------------------------------------- V1: substitution map over the repro files
    def build_subst(self):
        cand = []
        for rel, p in list(self.repro_files.items()) + [('EXTRA:' + os.path.basename(x), x) for x in self.extra_repro_files]:
            if '/.git/' in '/' + rel or rel.startswith('pin/') or '/objects/' in rel:
                continue
            try:
                b = rbytes(p)
            except OSError:
                continue
            if len(b) > 8 * 1024 * 1024 or b'\0' in b[:4096]:
                continue
            cand.append(b)
        subst = {}
        for _ in range(8):
            changed = False
            for b in cand:
                nb = self.to_orig(b, subst)
                k, v = sha256(b), sha256(nb)
                if k != v and subst.get(k) != v:
                    subst[k] = v
                    changed = True
                # FactsSha256 of tool envelopes
                try:
                    e = json.loads(b.decode('utf-8'))
                except Exception:
                    continue
                if isinstance(e, dict) and 'Facts' in e and isinstance(e.get('FactsSha256'), str):
                    mine = sha256(canon(e['Facts']).encode('ascii'))
                    if mine == e['FactsSha256']:
                        nf = json.loads(self.to_orig(canon(e['Facts']).encode('ascii'), subst).decode('ascii'))
                        v2 = sha256(canon(nf).encode('ascii'))
                        if v2 != mine and subst.get(mine) != v2:
                            subst[mine] = v2
                            changed = True
            if not changed:
                break
        return subst

    def to_orig(self, b, subst):
        b = self.cfg.to_orig_bytes(b)
        return re.sub(rb'(?<![0-9a-f])([0-9a-f]{64})(?![0-9a-f])', lambda m: subst.get(m.group(1).decode(), m.group(1).decode()).encode(), b)

    # ---------------------------------------------------------- digest verification
    def verify(self, a, b, key, ctx):
        """a = custodied digest, b = repro digest. Returns (ok, how)."""
        if self.pairs.get(b) == a:
            return True, self.pair_how.get(b)
        if (b, a) in self.failed_pairs:
            return False, self.failed_pairs[(b, a)]
        how = None
        if self.subst.get(b) == a:
            how = 'V1 substitution: repro content with the repro work root written as the original root hashes to the custodied value'
        if how is None and b in self.ext_pairs and self.ext_pairs[b][0] == a:
            how = self.ext_pairs[b][1]
        if how is None and self.depth < 10:
            common = sorted(set(self.orig_sha.get(a, [])) & set(self.repro_sha.get(b, [])))
            for rel in common:
                self.depth += 1
                try:
                    ok, det = self.equiv(rel)
                finally:
                    self.depth -= 1
                if ok:
                    how = 'V2 counterpart file %s: original and repro bytes differ only by %s' % (rel, det)
                    break
            if how is None and not common:
                ra, rb_ = self.orig_sha.get(a, []), self.repro_sha.get(b, [])
                if ra and rb_:
                    how_fail = 'digests name files at different relative paths: %s vs %s' % (ra[:3], rb_[:3])
                    self.failed_pairs[(b, a)] = how_fail
        if how is None and key in GIT_STATE_KEYS and getattr(self, 'git_state_ok', False):
            how = 'V4 git state: digest of the git dir / index at snapshot time; the final git dirs of every repository pair differ only in index stat data, reflog timestamps or worktree path files (see GitDirEquivalence)'
        if how is None and len(a) == 40 and len(b) == 40:
            ok, det = self.commit_equiv(a, b)
            if ok:
                how = 'V3 commit object: ' + det
        if how is None:
            self.failed_pairs.setdefault((b, a), 'not explained')
            return False, self.failed_pairs[(b, a)]
        self.pairs[b] = a
        self.pair_how[b] = how
        return True, how

    def commit_equiv(self, a, b):
        def find(root, oid):
            for r, ds, fs in os.walk(root):
                if '.git' in ds:
                    rc, out = git('-C', r, 'cat-file', 'commit', oid)
                    if rc == 0:
                        return out
                ds[:] = [d for d in ds if d != '.git']
            return None
        ca, cb = find(self.cfg.orig_root, a), find(self.cfg.repro_root, b)
        if ca is None or cb is None:
            return False, 'commit object not found'
        norm = lambda c: re.sub(rb'^((?:author|committer) .*?>) \d+ [+-]\d{4}$', rb'\1 <date>', c, flags=re.M)
        if norm(ca) == norm(cb):
            return True, 'same tree, parents, identities and message; only the author/committer dates differ'
        return False, 'commit objects differ beyond dates'

    # ---------------------------------------------------------- V2: equivalence of a counterpart file
    def equiv(self, rel):
        if rel in self.equiv_memo:
            return self.equiv_memo[rel]
        self.equiv_memo[rel] = (False, 'cycle')
        po, pr = self.orig_files.get(rel), self.repro_files.get(rel)
        if po is None or pr is None:
            res = (False, 'missing on one side')
        else:
            bo, br = rbytes(po), rbytes(pr)
            res = self.equiv_bytes(bo, br, rel)
        self.equiv_memo[rel] = res
        return res

    def equiv_bytes(self, bo, br, rel):
        if bo == br:
            return True, 'nothing (byte-identical)'
        jo, jr = self.try_json(bo), self.try_json(br)
        if jo is not None and jr is not None:
            sub = Comparer(self)
            sub.cmp(jo, jr, '')
            sub.resolve()
            bad = [d for d in sub.diffs if d['Class'] not in EXPECTED]
            if bad:
                return False, 'unexplained: ' + json.dumps(sanitize(bad[:3]))[:600]
            return True, ', '.join(sorted(set(d['Class'] for d in sub.diffs))) or 'nothing (JSON-equal)'
        lo, lr = self.try_jsonl(bo), self.try_jsonl(br)
        if lo is not None and lr is not None:
            if len(lo) != len(lr):
                return False, 'JSONL line count'
            sub = Comparer(self)
            sub.cmp(lo, lr, '')
            sub.resolve()
            bad = [d for d in sub.diffs if d['Class'] not in EXPECTED]
            if bad:
                return False, 'unexplained: ' + json.dumps(sanitize(bad[:3]))[:600]
            return True, ', '.join(sorted(set(d['Class'] for d in sub.diffs)))
        try:
            to, tr = bo.decode('utf-8'), br.decode('utf-8')
        except UnicodeDecodeError:
            return False, 'binary content differs'
        return self.text_equiv(TS.sub('<TS>', self.cfg.norm_paths(to)), TS.sub('<TS>', self.cfg.norm_paths(tr)), rel, 'PATH/TIMESTAMP (text)')

    def text_equiv(self, no, nr, rel, label):
        if no == nr:
            return True, label
        if HEXTOK.sub('<HEX>', no) != HEXTOK.sub('<HEX>', nr):
            return False, 'text differs beyond paths/timestamps/digests'
        hows = []
        for a, b in zip(HEXTOK.findall(no), HEXTOK.findall(nr)):
            if a == b:
                continue
            ok, how = self.verify(a, b, None, rel)
            if not ok:
                return False, 'text: unverified digest pair (%s)' % how
            hows.append(how.split(':')[0])
        return True, label + ' + verified ids (' + ', '.join(sorted(set(hows))) + ')'

    @staticmethod
    def try_json(b):
        try:
            return json.loads(b.decode('utf-8'))
        except Exception:
            return None

    @staticmethod
    def try_jsonl(b):
        try:
            lines = [ln for ln in b.decode('utf-8').split('\n') if ln.strip()]
            return [json.loads(ln) for ln in lines] if lines else None
        except Exception:
            return None


class Comparer(object):
    def __init__(self, eng):
        self.eng = eng
        self.cfg = eng.cfg
        self.diffs = []
        self.pending = []

    def add(self, path, cls, a, b, note=None, pairs=None):
        d = {'Path': path or '/', 'Class': cls, 'Custodied': a, 'Repro': b}
        if note:
            d['Note'] = note
        self.diffs.append(d)
        if pairs:
            self.pending.append((d, pairs))
        return d

    def cmp(self, a, b, path, vol=False):
        if isinstance(a, dict) and isinstance(b, dict):
            proc = ('CheckerPid' in a) or ('CheckerPid' in b)
            for k in sorted(set(a) | set(b)):
                p = path + '/' + k
                if k not in a or k not in b:
                    self.add(p, 'HOST_PROCESS_TABLE' if vol else 'STRUCTURE', a.get(k, MISSING), b.get(k, MISSING))
                    continue
                self.cmp(a[k], b[k], p, vol or (proc and k in ('UnreadableClosedList', 'Total')))
            return
        if isinstance(a, list) and isinstance(b, list):
            if len(a) != len(b):
                if vol:
                    self.add(path, 'HOST_PROCESS_TABLE', 'len %d' % len(a), 'len %d' % len(b))
                else:
                    self.add(path, 'LIST_LENGTH', a, b)
                return
            for i, (x, y) in enumerate(zip(a, b)):
                self.cmp(x, y, path + '/' + str(i), vol)
            return
        if type(a) is type(b) and a == b:
            return
        self.leaf(a, b, path, vol)

    def leaf(self, a, b, path, vol):
        key = path.rsplit('/', 1)[-1]
        if vol:
            return self.add(path, 'HOST_PROCESS_TABLE', a, b)
        if key in PID_KEYS and isinstance(a, int) and isinstance(b, int) and not isinstance(a, bool) and not isinstance(b, bool):
            return self.add(path, 'PID', a, b)
        if key in DURATION_KEYS and isinstance(a, (int, float)) and isinstance(b, (int, float)) and not isinstance(a, bool) and not isinstance(b, bool):
            return self.add(path, 'DURATION', a, b)
        if not (isinstance(a, str) and isinstance(b, str)):
            return self.add(path, 'VALUE' if type(a) is type(b) else 'TYPE', a, b)
        if key == 'StdoutB64':
            try:
                ja = json.loads(base64.b64decode(a).decode('utf-8'))
                jb = json.loads(base64.b64decode(b).decode('utf-8'))
            except Exception:
                ja = jb = None
            if ja is not None:
                n0 = len(self.diffs)
                self.cmp(ja, jb, path + '(base64-decoded)')
                return None
        if HEX.match(a) and HEX.match(b) and len(a) == len(b):
            return self.add(path, 'DIGEST', a, b, pairs=[(a, b, key)])
        pa, pb = self.cfg.norm_paths(a), self.cfg.norm_paths(b)
        if pa == pb:
            return self.add(path, 'PATH', a, b)
        ta, tb = TS.sub('<TS>', pa), TS.sub('<TS>', pb)
        if ta == tb:
            return self.add(path, 'TIMESTAMP' if (pa == a and pb == b) else 'PATH+TIMESTAMP', a, b)
        ha, hb = HEXTOK.findall(ta), HEXTOK.findall(tb)
        xa, xb = HEXTOK.sub('<HEX>', ta), HEXTOK.sub('<HEX>', tb)
        trunc = len(a) == len(b) and xa != xb and (xa.startswith(xb) or xb.startswith(xa))
        if xa == xb or trunc:
            prs = [(p, q, key) for p, q in zip(ha, hb) if p != q]
            if not prs:
                return self.add(path, 'PATH_TRUNCATION', a, b, note='capped string; the longer repro root shifts the cut')
            return self.add(path, 'STRING_WITH_DIGESTS', a, b, pairs=prs, note='truncated' if trunc else None)
        return self.add(path, 'VALUE', a, b)

    def resolve(self):
        for d, prs in self.pending:
            hows, bad = [], []
            for a, b, key in prs:
                ok, how = self.eng.verify(a, b, key, d['Path'])
                (hows if ok else bad).append(how)
            if bad:
                d['Class'] = 'UNVERIFIED_DIGEST'
                d['Note'] = '; '.join(sorted(set(str(x) for x in bad)))[:400]
            else:
                if d['Class'] == 'DIGEST':
                    d['Class'] = 'DIGEST_VERIFIED'
                else:
                    d['Class'] = 'STRING_WITH_VERIFIED_DIGESTS' if d.get('Note') != 'truncated' else 'PATH_TRUNCATION'
                d['Verified'] = sorted(set(hows))[:4]
        self.pending = []


# ------------------------------------------------------------------ git dir / work-root equivalence (final states)
def reflog_norm(b, cfg):
    t = cfg.norm_paths(b.decode('utf-8', 'replace'))
    return re.sub(r'(<[^>]*>) \d+ [+-]\d{4}(?=\t|$)', r'\1 <date>', t, flags=re.M)


def workroot_equivalence(eng):
    cfg = eng.cfg
    o = dict((k, v) for k, v in eng.orig_files.items() if not k.startswith('RESULTS:'))
    r = dict((k, v) for k, v in eng.repro_files.items() if not k.startswith('RESULTS:'))
    tops_r = set(k.split('/')[0] for k in r)
    not_produced = sorted(k for k in set(o) - set(r) if k.split('/')[0] not in tops_r)
    only_o = sorted(k for k in set(o) - set(r) if k.split('/')[0] in tops_r)
    only_r = sorted(set(r) - set(o))
    classes, unexplained, same = {}, [], 0
    for rel in sorted(set(o) & set(r)):
        bo, br = rbytes(o[rel]), rbytes(r[rel])
        if bo == br:
            same += 1
            continue
        base = rel.rsplit('/', 1)[-1]
        cls = None
        if base == 'index' and ('/.git/' in '/' + rel or '/worktrees/' in rel):
            gd_o, gd_r = os.path.dirname(o[rel]), os.path.dirname(r[rel])
            ro, lo = git('--git-dir=' + gd_o, 'ls-files', '-s', '-z')
            rr, lr = git('--git-dir=' + gd_r, 'ls-files', '-s', '-z')
            cls = 'GIT_INDEX_STAT_DATA_ONLY' if (ro == rr == 0 and lo == lr) else None
        elif '/logs/' in '/' + rel:
            ok, det = eng.text_equiv(reflog_norm(bo, cfg), reflog_norm(br, cfg), rel, 'REFLOG_DATES_PATHS_ONLY')
            cls = det if ok else None
        if cls is None:
            ok, det = eng.equiv(rel)
            cls = ('CONTENT: ' + det) if ok else None
            if not ok:
                unexplained.append({'File': rel, 'Why': det})
        if cls:
            classes.setdefault(cls[:120], []).append(rel)
    summ = dict((k, {'Count': len(v), 'Examples': v[:4]}) for k, v in sorted(classes.items()))
    tops = {}
    for k in not_produced:
        tops[k.split('/')[0]] = tops.get(k.split('/')[0], 0) + 1
    return {'FilesCompared': len(set(o) & set(r)), 'ByteIdentical': same, 'DifferingExplained': summ,
            'Unexplained': unexplained, 'OnlyInOriginal': only_o, 'OnlyInRepro': only_r,
            'OriginalRootEntriesNotProducedByThePackageScripts': tops}


# ------------------------------------------------------------------ main
def compare_file(eng, name):
    pa, pb = os.path.join(eng.cust_dir, name), os.path.join(eng.repro_dir, name)
    ba, bb = rbytes(pa), rbytes(pb)
    rec = {'File': name, 'CustodiedSha256': sha256(ba), 'ReproSha256': sha256(bb), 'ByteIdentical': ba == bb}
    if ba == bb:
        rec.update({'Status': 'IDENTICAL', 'Differences': [], 'ClassCounts': {}})
        return rec
    c = Comparer(eng)
    c.cmp(json.loads(ba.decode('utf-8')), json.loads(bb.decode('utf-8')), '')
    c.resolve()
    counts = {}
    for d in c.diffs:
        counts[d['Class']] = counts.get(d['Class'], 0) + 1
    fails = [d for d in c.diffs if d['Class'] not in EXPECTED]
    rec.update({'Status': 'REPRODUCTION_FAILURE' if fails else ('EXPECTED_ONLY_DIFFERENCES' if c.diffs else 'IDENTICAL_CONTENT'),
                'ClassCounts': counts, 'Failures': fails, 'Differences': c.diffs})
    return rec


def main():
    pkg, cust, repro, out = sys.argv[1:5]
    names = sys.argv[5:]
    extra, pairs_file = [], None
    if '--extra' in names:
        i = names.index('--extra')
        extra = names[i + 1:]
        names = names[:i]
    if '--pairs' in names:
        i = names.index('--pairs')
        pairs_file = names[i + 1]
        names = names[:i] + names[i + 2:]
    cfg = Cfg(pkg)
    eng = Engine(cfg, cust, repro, extra)
    if pairs_file:
        eng.ext_pairs = json.loads(rbytes(pairs_file).decode('ascii'))
    gde = workroot_equivalence(eng)
    eng.git_state_ok = not gde['Unexplained'] and not [x for x in gde['OnlyInOriginal'] + gde['OnlyInRepro'] if '/objects/' not in x]
    files = []
    for n in names:
        if n == 'manifest.json':
            continue
        files.append(compare_file(eng, n))
    if 'manifest.json' in names:
        okfiles = dict((f['File'], f) for f in files)
        m = compare_file(eng, 'manifest.json')
        for d in m.get('Differences', []):
            if d['Class'] in EXPECTED:
                continue
            mm = re.match(r'^/Files/(\d+)/(Sha256|Bytes)$', d['Path'])
            if mm:
                row = json.loads(rbytes(os.path.join(repro, 'manifest.json')).decode('ascii'))['Files'][int(mm.group(1))]
                fn = row['File'].split('/', 1)[-1]
                f = okfiles.get(fn)
                if f and f['Status'] in ('EXPECTED_ONLY_DIFFERENCES', 'IDENTICAL_CONTENT'):
                    d['Class'] = 'DERIVED_FROM_COMPARED_FILE'
                    d['Note'] = 'digest/size of results/%s, whose own comparison has only expected differences' % fn
        fails = [d for d in m.get('Differences', []) if d['Class'] not in EXPECTED]
        m['Failures'] = fails
        m['ClassCounts'] = {}
        for d in m.get('Differences', []):
            m['ClassCounts'][d['Class']] = m['ClassCounts'].get(d['Class'], 0) + 1
        if m['Status'] != 'IDENTICAL':
            m['Status'] = 'REPRODUCTION_FAILURE' if fails else 'EXPECTED_ONLY_DIFFERENCES'
        files.append(m)
    verdict = 'REPRODUCED' if all(f['Status'] != 'REPRODUCTION_FAILURE' for f in files) else 'NOT REPRODUCED'
    res = {'Schema': 'r62-a6r2-repro/compare-1', 'Package': pkg, 'Verdict': verdict,
           'OriginalWorkRoot': cfg.orig_root, 'ReproWorkRoot': cfg.repro_root,
           'ClassesExpected': sorted(EXPECTED),
           'Files': files, 'WorkRootEquivalence': gde,
           'VerifiedDigestPairs': len(eng.pairs)}
    data = (json.dumps(sanitize(res), ensure_ascii=True, sort_keys=True, indent=1) + '\n').encode('ascii')
    with open(out, 'wb') as fh:
        fh.write(data)
    print(verdict)
    for f in files:
        print(' ', f['File'], f['Status'], json.dumps(f.get('ClassCounts')))
    print('  workroot: compared', gde['FilesCompared'], 'identical', gde['ByteIdentical'], 'unexplained', len(gde['Unexplained']),
          'onlyOrig', len(gde['OnlyInOriginal']), 'onlyRepro', len(gde['OnlyInRepro']))


if __name__ == '__main__':
    main()

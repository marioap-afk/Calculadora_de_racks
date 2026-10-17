# check_a7_closure.py - validator of an effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY invocation
# (or of the A6-7 measurement) against the CANDIDATE rule A7-1 (I-62 A-7 §3.2 and the table of §3.3, round 1 as corrected
# after its preflight), written from the A-7 text and independent of the builder: it imports neither make_eic_a7.py nor
# make_eic_verify.py nor derive_r4.py, and computes its own references from the designation (a6m7-designation/1) and the -C.
# Stdlib only; read-only git.
#
#   python -I check_a7_closure.py <designation.json> <E.json> [--absent-a=no-entry|no-closure] [--path-rule=text|strict]
#                                 [--i62-repo=<path> [--i62-commit=<sha>]]
#   exit 0 = accepted, 1 = rejected, 64 = usage. Prints the structured verdict (JSON) on stdout.
#
# What it checks (A7-1 as corrected):
#   rule 1   every entry {Path, Blob} of the three lists belongs to at least one category (a)-(d) (an entry with none is
#            FOREIGN); an entry is in a category when its blob is that category's reference for its path, so an entry in more
#            than one category matches each reference (recorded as OVERLAP, admitted); (a) is narrowed to the contract's
#            authorities, the automatic instruction, the transitive inputs of the obligations and the files whose content
#            16.13 reads: map and schema, MODIFIED map files, WORKFLOW.md and AUTOMATION_PLAN.md in Evaluate and Validate
#            (ADDED / ENTRY map files are not admitted); (b), (c), (d) only in CanonicalInputs, the instruction only in
#            AutomaticInstructions; no record repeated within its list (DUPLICATE); the same entry in more than one list, or
#            one path with one blob per reading revision, is admitted (rule 2 (a));
#   rule 2   the reference of each category: (a) the reading revision (AR for UNIT_DOC / UNIT_CHANGE, instructions and the
#            transitive reads of the obligations; MainSha_eval for EXTERNAL and WORKFLOW.md / AUTOMATION_PLAN.md in Evaluate;
#            EFF for map, schema, MODIFIED files and WORKFLOW.md / AUTOMATION_PLAN.md in Validate; EFF^1 for MODIFIED files and
#            AUTOMATION_PLAN.md in Validate); (b) Target.commit; (c) the Git blob of the custodied bytes at the designated
#            paths, delegation and handoff under artifacts/orchestration/ in a directory named with the RunId of their own
#            bytes; (d) the blob fixed by A-6;
#   rule 3   the comparison is made against that reference (the I6 check of the category);
#   rule 5   (a) an (a) input absent at its revision is not required (recorded absence); (b) the Target object absent means no
#            closure may exist (NO_CLOSURE_EXPECTED), a diff path or unit record absent is not required; (c) without a present,
#            blob-verified, readable contract and delegation no closure may exist; an absent handoff or TRX is not required;
#   rule 2 (c) registration: a blob the designation registers for an artifact must be the blob of its custodied bytes; an
#            artifact without a registered blob is reported (UnregisteredBlobs), not rejected (the bench format registers only K);
#   mandatory entries: every input the categories name and that exists.
# Switches kept only as negative evidence of the round-1 text before its correction: --absent-a=no-closure (rule 5 (a): an
# absent (a) input left the closure uncomputed) and --path-rule=strict (rule 2 (c): artifacts/orchestration/<UnitId>/
# <I.TaskId>/<Attempt>/<RunId>/). AGENTS.md at the AuthorityRevision is the source of the kit's 16.24 fixed point: without
# it no closure may exist in either reading.
import hashlib, json, os, re, subprocess, sys

sys.dont_write_bytecode = True
SCHEMA = 'rackcad-input-closure/v1'
DES_SCHEMA = 'a6m7-designation/1'
HEX40 = re.compile(r'[0-9a-f]{40}\Z')
A6_INSTRUMENT_BLOB = '9a829b0eeb1210238052b1feac06cfa8f7964aed'
TRAILER = 'Agent-Protocol-Normative: I-62'
AGENTS = 'AGENTS.md'
AP = 'docs/AUTOMATION_PLAN.md'
WF = 'docs/WORKFLOW.md'
MAP_SCHEMA = 'docs/automation/agent-execution/compatibility/clause-map.schema.json'
S1613 = '### 16.13 Compatibilidad de protocolos de ejecución delegada'
LISTS = ('CanonicalInputs', 'AllowedTransitiveInputs', 'AutomaticInstructions')


def git(repo, *args):
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    env['GIT_OPTIONAL_LOCKS'] = '0'
    env['GIT_TERMINAL_PROMPT'] = '0'
    p = subprocess.run(['git', '--no-replace-objects', '-C', repo] + list(args), capture_output=True, stdin=subprocess.DEVNULL, env=env)
    return p.returncode, p.stdout


class Repo(object):
    def __init__(self, path):
        self.path = path
        self.trees = {}
        self.kinds = {}

    def is_commit(self, v):
        if not (isinstance(v, str) and HEX40.match(v)):
            return False
        if v not in self.kinds:
            rc, out = git(self.path, 'cat-file', '-t', v)
            self.kinds[v] = out.decode('ascii', 'replace').strip() if rc == 0 else None
        return self.kinds[v] == 'commit'

    def blob(self, commit, path):
        if commit is None or not isinstance(path, str):
            return None
        if commit not in self.trees:
            rc, out = git(self.path, 'ls-tree', '-r', '-z', '--full-tree', commit)
            t = {}
            for rec in (out.split(b'\0') if rc == 0 else []):
                if rec:
                    meta, name = rec.split(b'\t', 1)
                    _mode, typ, oid = meta.decode('ascii').split()
                    t[name.decode('utf-8', 'surrogateescape')] = (typ, oid)
            self.trees[commit] = t
        typ, oid = self.trees[commit].get(path, (None, None))
        return oid if typ == 'blob' else None

    def object_bytes(self, oid):
        rc, out = git(self.path, 'cat-file', 'blob', oid)
        return out if rc == 0 and git_blob_id(out) == oid else None


def git_blob_id(b):
    """git hash-object --no-filters of the bytes."""
    return hashlib.sha1(b'blob %d\0' % len(b) + b).hexdigest()


def json_object(b):
    """One JSON object, strict UTF-8 without BOM, no repeated key at any depth; None otherwise."""
    def hook(pairs):
        d = {}
        for k, v in pairs:
            if k in d:
                raise ValueError('repeated key')
            d[k] = v
        return d
    try:
        s = b.decode('utf-8', 'strict')
        if s.startswith('﻿'):
            return None
        o = json.loads(s, object_pairs_hook=hook)
    except (UnicodeDecodeError, ValueError):
        return None
    return o if isinstance(o, dict) else None


def derive(repo, main):
    """EFF of AP 16.13 / 16.14 from MainSha_eval: (eff, eff1), (None, None) without trailer, or raises ValueError."""
    rc, out = git(repo.path, 'log', '--no-show-signature', '-z', '--format=%H%n%B', main)
    if rc != 0:
        raise ValueError('git log failed')
    with_trailer = []
    for rec in out.split(b'\0'):
        text = rec.decode('utf-8', 'replace').replace('\r\n', '\n').lstrip('\n')
        if text.strip():
            sha, _, body = text.partition('\n')
            if TRAILER in body.split('\n'):
                with_trailer.append(sha.strip())
    if len(with_trailer) > 1:
        raise ValueError('trailer in %d commits' % len(with_trailer))
    if not with_trailer:
        return None, None
    rc, out = git(repo.path, 'rev-list', '--parents', main)
    par = dict((ln.split()[0], ln.split()[1:]) for ln in out.decode('ascii').split('\n') if ln.strip())
    kids = {}
    for c, ps in par.items():
        for p in ps:
            kids.setdefault(p, set()).add(c)
    seen, todo = set([with_trailer[0]]), [with_trailer[0]]
    while todo:
        for k in kids.get(todo.pop(), ()):
            if k not in seen:
                seen.add(k)
                todo.append(k)
    rc, out = git(repo.path, 'rev-list', '--first-parent', '--merges', main)
    found = [m for m in out.decode('ascii').split() if len(par.get(m, [])) >= 2 and par[m][1] in seen and par[m][0] not in seen]
    if len(found) != 1:
        raise ValueError('%d qualifying merges' % len(found))
    return found[0], par[found[0]][0]


def read_custody(repo, role, item):
    cust, reg = item.get('Custody'), item.get('Blob')
    if cust:
        if not os.path.exists(cust):
            return None, 'ABSENT'
        try:
            with open(cust, 'rb') as fh:
                b = fh.read()
        except OSError:
            return None, 'UNREADABLE'
    elif role == 'CONTRACT' and isinstance(reg, str) and HEX40.match(reg):
        b = repo.object_bytes(reg)
        if b is None:
            return None, 'ABSENT'
    else:
        return None, 'ABSENT'
    if isinstance(reg, str) and git_blob_id(b) != reg:
        return None, 'BLOB_MISMATCH'
    return b, None


def check(des, E, absent_a='no-entry', path_rule='text', i62_repo=None, i62_commit='HEAD'):
    AEFF = 'no-closure' if absent_a == 'no-closure' else 'record'
    v = {'Accepted': False, 'Readings': {'AbsentA': absent_a, 'PathRule': path_rule}, 'NoClosureExpected': [], 'Foreign': [],
         'Duplicates': [], 'Missing': [], 'Overlaps': [], 'ListViolations': [], 'Problems': [], 'Absences': [], 'RepeatedPairs': [], 'UnregisteredBlobs': [],
         'Classified': []}
    nc, prob = v['NoClosureExpected'], v['Problems']
    if not isinstance(des, dict) or des.get('Schema') != DES_SCHEMA:
        prob.append('DESIGNATION: not a6m7-designation/1')
        return v
    if not isinstance(E, dict) or E.get('Schema') != SCHEMA or not all(isinstance(E.get(k), list) for k in LISTS):
        prob.append('SHAPE: not a rackcad-input-closure/v1 object with the three lists')
        return v
    for k in LISTS:
        for e in E[k]:
            if not (isinstance(e, dict) and isinstance(e.get('Path'), str) and isinstance(e.get('Blob'), str) and HEX40.match(e['Blob'])):
                prob.append('SHAPE: %s entry without Path / 40-hex Blob' % k)
                return v
    repo = Repo(des['Repo'])
    ar = des.get('AuthorityRevision')
    inv = des.get('Invocation') or {}
    tg = inv.get('Target') if isinstance(inv.get('Target'), dict) else {}
    if E.get('AuthorityRevision') != ar:
        prob.append('AUTHORITY_REVISION: E.AuthorityRevision is not the designated AuthorityRevision')
    if not repo.is_commit(ar):
        nc.append('the AuthorityRevision is not a commit of the -C')
    tcommit = tg.get('Commit')
    tblob = repo.blob(tcommit, tg.get('Path')) if repo.is_commit(tcommit) else None
    if tblob is None or (tg.get('Blob') is not None and tblob != tg.get('Blob')):
        nc.append('rule 5 (b): the exact object of the Target is absent at Target.commit')
    # (c) designated artifacts
    roles = {}
    for item in des.get('Inputs') or []:
        if isinstance(item, dict) and item.get('Role') in ('CONTRACT', 'DELEG', 'H', 'TRX'):
            if item['Role'] in roles:
                prob.append('DESIGNATION: role %s designated twice' % item['Role'])
            roles[item['Role']] = item
    for r in ('CONTRACT', 'DELEG', 'H', 'TRX'):
        if r not in roles:
            prob.append('DESIGNATION: role %s not designated (rule 2 (c))' % r)
    if prob:
        return v
    # rule 2 (c): the designation registers the path and the blob of each artifact; a registered blob is checked against the
    # custodied bytes (read_custody), an unregistered one is reported (the a6m7-designation/1 format registers only the contract's)
    v['UnregisteredBlobs'] = [r for r in ('CONTRACT', 'DELEG', 'H', 'TRX') if not (isinstance(roles[r].get('Blob'), str) and HEX40.match(roles[r]['Blob']))]
    C, cbytes = {}, {}
    for r in ('CONTRACT', 'DELEG', 'H', 'TRX'):
        b, cause = read_custody(repo, r, roles[r])
        if cause:
            v['Absences'].append({'Category': 'c', 'Role': r, 'Path': roles[r]['Path'], 'Cause': cause})
        else:
            C[(roles[r]['Path'], git_blob_id(b))] = r
            cbytes[r] = b
    K = json_object(cbytes['CONTRACT']) if 'CONTRACT' in cbytes else None
    D = json_object(cbytes['DELEG']) if 'DELEG' in cbytes else None
    H = json_object(cbytes['H']) if 'H' in cbytes else None
    if K is None or not isinstance(K.get('Authorities'), list):
        nc.append('the contract is absent or unusable: (a) has no resolution input (Resolve takes K)')
    if D is None or not repo.is_commit(D.get('BaseSha')):
        nc.append('the delegation is absent or has no BaseSha commit: (b) has no BaseSha')
    if K is not None and K.get('AuthorityRevision') not in (None, ar):
        prob.append('AUTHORITY_REVISION: K.AuthorityRevision differs from the designated AuthorityRevision')
    mse = des.get('MainShaEval') or (K.get('MainSha') if K else None)
    eff = eff1 = None
    if K is not None:
        if not repo.is_commit(mse):
            nc.append('MainSha_eval is not a commit')
        else:
            try:
                eff, eff1 = derive(repo, mse)
            except ValueError as e:
                nc.append('Derive(MainSha_eval) fails: %s' % e)
    rev = {'AR': ar, 'MainSha_eval': mse, 'EFF': eff, 'EFF^1': eff1, 'Target.commit': tcommit}
    v['Revisions'] = dict(rev)
    A, B, Dd = {}, {}, {}             # (path, blob) -> labels
    must = []                          # (label, path, blob, allowed lists)

    def ref(table, path, rv, label, mandatory, lists, absent_effect):
        b = repo.blob(rev[rv], path) if rev.get(rv) else None
        if b is None:
            if absent_effect == 'no-closure':
                nc.append('rule 5 (a): %s absent at %s (%s)' % (path, rv, label))
            elif absent_effect == 'record':
                v['Absences'].append({'Category': label.split(':')[0], 'Role': label, 'Path': path, 'Revision': rv, 'Cause': 'absent'})
            return
        table.setdefault((path, b), set()).add(label)
        if mandatory:
            must.append((label, path, b, lists))

    canon_only = ('CanonicalInputs',)
    a_lists = ('CanonicalInputs', 'AllowedTransitiveInputs')
    # (a) authorities cited by the contract
    if K is not None and isinstance(K.get('Authorities'), list):
        for a in K['Authorities']:
            if not (isinstance(a, dict) and isinstance(a.get('Path'), str)):
                nc.append('a contract authority without Path')
                continue
            cls = a.get('Class')
            rv = 'AR' if cls in ('UNIT_DOC', 'UNIT_CHANGE') else ('MainSha_eval' if cls == 'EXTERNAL' else None)
            if rv is None:
                nc.append('authority %s with Class %r outside the 16.13 step 3 classes' % (a['Path'], cls))
                continue
            ref(A, a['Path'], rv, 'a:AUTHORITY:%s@%s' % (cls, rv), True, canon_only, AEFF)
    # (a) automatic instruction and the transitive reads its obligations and the authorities' obligations declare
    ref(A, AGENTS, 'AR', 'a:INSTRUCTION@AR', True, ('AutomaticInstructions',), 'no-closure')   # source of the 16.24 fixed point
    for o in E.get('Obligations') or []:
        if not isinstance(o, dict):
            continue
        if o.get('SourceBlob') != repo.blob(ar, o.get('SourcePath')):
            prob.append('OBLIGATION: SourceBlob of %s#%s is not its blob at the AuthorityRevision' % (o.get('SourcePath'), o.get('SourceSection')))
        if o.get('Class') == 'READ' and isinstance(o.get('Target'), str):
            rv = 'EFF' if o.get('SourceSection') == S1613 else 'AR'
            ref(A, o['Target'], rv, 'a:OBLIGATION-READ@%s' % rv, True, ('CanonicalInputs', 'AllowedTransitiveInputs'), AEFF)
    # (a) the inputs that the 16.13 resolution reads
    mapdoc = None
    map_path = des.get('MapPath')
    if eff is not None:
        ref(A, map_path, 'EFF', 'a:16.13-MAP@EFF', True, a_lists, AEFF)
        ref(A, MAP_SCHEMA, 'EFF', 'a:16.13-MAP_SCHEMA@EFF', True, a_lists, AEFF)
        mb = repo.blob(eff, map_path)
        mapdoc = json_object(repo.object_bytes(mb) or b'') if mb else None
        if mapdoc is None or not isinstance(mapdoc.get('Files'), list):
            if AEFF == 'no-closure':
                nc.append('the clause map at EFF is absent or unusable')
            else:
                v['Absences'].append({'Category': 'a', 'Role': 'a:16.13-MV6', 'Path': map_path, 'Revision': 'EFF',
                                      'Cause': 'map absent or unusable: MODIFIED files not enumerable (Validate: MAP_INVALID)'})
            mapdoc = {'Files': []}
        effect = AEFF
        for f in mapdoc['Files']:
            if not (isinstance(f, dict) and isinstance(f.get('Path'), str)):
                continue
            if f.get('FileKind') == 'MODIFIED':
                ref(A, f['Path'], 'EFF', 'a:16.13-MV6@EFF', True, a_lists, effect)
                ref(A, f['Path'], 'EFF^1', 'a:16.13-MV6@EFF^1', True, a_lists, effect)
        ref(A, WF, 'EFF', 'a:16.13-VALIDATE@EFF', True, a_lists, AEFF)
        ref(A, AP, 'EFF', 'a:16.13-VALIDATE@EFF', True, a_lists, AEFF)
        ref(A, AP, 'EFF^1', 'a:16.13-VALIDATE@EFF^1', True, a_lists, AEFF)
        ref(A, WF, 'MainSha_eval', 'a:16.13-EVALUATE@MainSha_eval', True, a_lists, AEFF)
        ref(A, AP, 'MainSha_eval', 'a:16.13-EVALUATE@MainSha_eval', True, a_lists, AEFF)
    # (b) objects of the Target
    if tblob is not None:
        B.setdefault((tg['Path'], tblob), set()).add('b:TARGET')
        must.append(('b:TARGET', tg['Path'], tblob, canon_only))
    if D is not None and repo.is_commit(D.get('BaseSha')) and repo.is_commit(tcommit):
        rc, out = git(repo.path, 'diff', '--name-only', '--no-renames', '-z', D['BaseSha'] + '..' + tcommit)
        for p in sorted(x.decode('utf-8') for x in out.split(b'\0') if x):
            ref(B, p, 'Target.commit', 'b:DIFF', True, canon_only, 'record')
    unit = inv.get('UnitId')
    for p in ('docs/automation/decisions/%s.md' % unit, 'docs/automation/state/%s.yml' % unit):
        ref(B, p, 'Target.commit', 'b:RECORD', True, canon_only, 'record')
    # (c) artifacts: mandatory when their custodied bytes verify; path rule of rule 2 (c)
    for (p, b), r in C.items():
        must.append(('c:' + r, p, b, canon_only))
    for r, obj in (('DELEG', D), ('H', H)):
        if obj is None or not isinstance(obj.get('RunId'), str):
            continue
        path = roles[r]['Path']
        if path_rule == 'strict':
            # superseded round-1 text (negative evidence only): artifacts/orchestration/<UnitId>/<I.TaskId>/<Attempt>/<RunId>/
            m = re.fullmatch(r'artifacts/orchestration/([^/]+)/([^/]+)/([0-9]+)/([^/]+)/[^/]+', path)
            ok = bool(m) and m.group(1) == inv.get('UnitId') and m.group(2) == str(inv.get('TaskId')) \
                and m.group(3) == str(inv.get('Attempt')) and m.group(4) == obj['RunId']
        else:
            # rule 2 (c) as corrected: under artifacts/orchestration/, in the route of the task or of the control (RAE 10), in a
            # directory named with the RunId of its own bytes; only that segment is constrained here
            parts = path.split('/')
            ok = path.startswith('artifacts/orchestration/') and len(parts) >= 4 and parts[-2] == obj['RunId']
        if not ok:
            prob.append('PATH_RULE: %s path is not under artifacts/orchestration/ in a directory named with the RunId of its bytes '
                        '(path rule %s)' % (r, path_rule))
    # (d) instrument
    ins = des.get('Instrument')
    if ins:
        if ins.get('Blob') != A6_INSTRUMENT_BLOB:
            prob.append('INSTRUMENT: the designated blob is not the one fixed by A-6')
        Dd[(ins.get('Path'), A6_INSTRUMENT_BLOB)] = {'d:INSTRUMENT'}
        must.append(('d:INSTRUMENT', ins.get('Path'), A6_INSTRUMENT_BLOB, canon_only))
        if i62_repo:
            rc, out = git(i62_repo, 'rev-parse', '--verify', '--quiet', '%s:%s' % (i62_commit, ins.get('Path')))
            if rc != 0 or out.decode('ascii').strip() != A6_INSTRUMENT_BLOB:
                prob.append('INSTRUMENT: the provenance path does not hold the A-6 blob in the I-62 commit')
    # classification
    seen = {}
    for lst in LISTS:
        for e in E[lst]:
            pb = (e['Path'], e['Blob'])
            key = (lst, e['Path'], e['Blob'], json.dumps(e.get('RequiredBy'), sort_keys=True) if lst == 'AllowedTransitiveInputs' else None)
            if key in seen:
                v['Duplicates'].append({'List': lst, 'Path': e['Path'], 'Blob': e['Blob']})
            seen[key] = True
            labels = set()
            if lst == 'AutomaticInstructions':
                labels |= set(x for x in A.get(pb, ()) if x.startswith('a:INSTRUCTION'))
            else:
                labels |= set(x for x in A.get(pb, ()) if not x.startswith('a:INSTRUCTION') or lst == 'CanonicalInputs')
                if lst == 'CanonicalInputs':
                    labels |= B.get(pb, set()) | Dd.get(pb, set())
                    if pb in C:
                        labels.add('c:' + C[pb])
                else:
                    for t in (B, Dd):
                        if pb in t:
                            v['ListViolations'].append({'List': lst, 'Path': e['Path'], 'Blob': e['Blob'], 'Why': 'category (b)/(d) entry outside CanonicalInputs'})
                    if pb in C:
                        v['ListViolations'].append({'List': lst, 'Path': e['Path'], 'Blob': e['Blob'], 'Why': 'category (c) entry outside CanonicalInputs'})
            cats = sorted(set(x[0] for x in labels))
            v['Classified'].append({'List': lst, 'Path': e['Path'], 'Blob': e['Blob'], 'Categories': cats, 'Labels': sorted(labels)})
            if not cats:
                refs = sorted(set(b for (p, b) in list(A) + list(B) + list(C) + list(Dd) if p == e['Path']))
                why = ('the path has no reference in any category (outside the diff, the records, the designation and the authorities)'
                       if not refs else 'the blob is not the reference of its category (%s)' % ', '.join(x[:8] for x in refs))
                if e['Path'].startswith('artifacts/') or e['Path'].endswith('.trx'):
                    why = 'off-Git or TRX path not designated, or with another blob than the custodied bytes'
                if 'Blob' in e and e['Blob'] == A6_INSTRUMENT_BLOB and not ins:
                    why = 'instrument entry in an invocation that does not admit it'
                v['Foreign'].append({'List': lst, 'Path': e['Path'], 'Blob': e['Blob'], 'Why': why})
            if len(cats) > 1:
                v['Overlaps'].append({'List': lst, 'Path': e['Path'], 'Blob': e['Blob'], 'Categories': cats})
            if lst == 'AllowedTransitiveInputs':
                rb = e.get('RequiredBy')
                if not (isinstance(rb, dict) and rb.get('Blob') == repo.blob(ar, rb.get('Path'))):
                    prob.append('REQUIRED_BY: %s: RequiredBy is not a source pinned at the AuthorityRevision' % e['Path'])
    # mandatory entries
    have = dict((lst, set((e['Path'], e['Blob']) for e in E[lst])) for lst in LISTS)
    for label, p, b, lists in must:
        if not any((p, b) in have[l] for l in lists):
            v['Missing'].append({'Label': label, 'Path': p, 'Blob': b})
    # one pair in several records (several lists, or several RequiredBy): admitted by rule 2 (a) as corrected; reported
    count = {}
    for lst in LISTS:
        for e in E[lst]:
            count.setdefault((e['Path'], e['Blob']), []).append(lst)
    v['RepeatedPairs'] = [{'Path': p, 'Blob': b, 'Lists': ls} for (p, b), ls in sorted(count.items()) if len(ls) > 1]
    # actions: the instrument action only with the admitted instrument entry (A6-2 rule 2)
    acts = [a for a in (E.get('AllowedActions') or []) if isinstance(a, dict) and A6_INSTRUMENT_BLOB in str(a.get('Action'))]
    has_ins = any(e['Blob'] == A6_INSTRUMENT_BLOB for e in E['CanonicalInputs'])
    if acts and not ins:
        prob.append('ACTION: instrument action in an invocation that does not admit the instrument')
    if ins and has_ins and len(acts) != 1:
        prob.append('ACTION: %d instrument actions with the instrument entry (exactly 1 required)' % len(acts))
    v['Accepted'] = not (nc or v['Foreign'] or v['Duplicates'] or v['Missing'] or v['ListViolations'] or prob)
    return v


def main(argv):
    opts = dict(a[2:].split('=', 1) for a in argv[1:] if a.startswith('--') and '=' in a)
    args = [a for a in argv[1:] if not a.startswith('--')]
    if len(args) != 2 or opts.get('absent-a', 'no-entry') not in ('no-entry', 'no-closure') or opts.get('path-rule', 'text') not in ('text', 'strict'):
        sys.stderr.write('usage: python -I check_a7_closure.py <designation.json> <E.json> [--absent-a=no-entry|no-closure] '
                         '[--path-rule=text|strict] [--i62-repo=<path> [--i62-commit=<sha>]]\n')
        return 64
    with open(args[0], 'rb') as fh:
        des = json.loads(fh.read().decode('utf-8'))
    with open(args[1], 'rb') as fh:
        E = json.loads(fh.read().decode('utf-8'))
    v = check(des, E, opts.get('absent-a', 'no-entry'), opts.get('path-rule', 'text'), opts.get('i62-repo'), opts.get('i62-commit', 'HEAD'))
    sys.stdout.write(json.dumps(v, ensure_ascii=True, sort_keys=True) + '\n')
    return 0 if v['Accepted'] else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv))

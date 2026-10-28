# make_eic_verify.py - mechanical builder of the effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY
# invocation (and of the A6-7 measurement), from a per-invocation designation (a6m7-designation/1). I-62, A-6, decisiones
# §90 point 3 (A6-7 rule 7). Stdlib only; every git call is read-only and uses --no-replace-objects.
#
#   python -I make_eic_verify.py <designation.json> <R7-A|R7-B|R7-B-K> <out.json> [--no-instrument]
#
# Variants (what each one pins; the obligation table and the fixed point are the kit profile of r2/independence part (d)):
#   R7-A  (cover): authority class (the contract, its Authorities, the automatic instructions and the transitive inputs of
#         the AP 16.24 fixed point) by their blob at the AuthorityRevision; the contract, absent from the AuthorityRevision
#         tree, by the blob of its custodied bytes (the designation repeats it); the object under verification
#         (git diff --name-only D.BaseSha..I.Target.Commit) and the window records (the contract Authorities of Class
#         UNIT_DOC) by their blob at I.Target.Commit; the off-Git artifacts (DELEG, H, TRX) by the Git blob id of their
#         custodied bytes; and the instrument entry {Path, Blob} of A6-2 rule 1.
#   R7-B  (limit): RAE §15.2 I6 read literally for every Git entry (blob = its blob at the AuthorityRevision; an entry
#         without that blob is excluded, the contract included); off-Git artifacts and the instrument entry (A6-2 rule 3)
#         kept.
#   R7-B-K: R7-B keeping the contract by its custodied blob (the "I6-strict" variant of r2 part (d)); comparison only.
# --no-instrument builds E without the A6-2 entry and action (regression against r2 / r4, which predate A-6 round 2).
import hashlib, importlib.util, json, os, re, subprocess, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
SCHEMA = 'rackcad-input-closure/v1'
HEX40 = re.compile(r'[0-9a-f]{40}\Z')
VARIANTS = ('R7-A', 'R7-B', 'R7-B-K')

# kit profile (the r2 part (d) obligation table; the blobs are computed, never written here)
AP = 'docs/AUTOMATION_PLAN.md'
WF = 'docs/WORKFLOW.md'
RAE = 'docs/automation/agent-execution/README.md'
AGENTS = 'AGENTS.md'
MAP_SCHEMA = 'docs/automation/agent-execution/compatibility/clause-map.schema.json'
HANDOFF_SCHEMA = 'docs/automation/agent-execution/schemas/worker-handoff.schema.json'
S1613 = '### 16.13 Compatibilidad de protocolos de ejecución delegada'
S169 = '### 16.9 Verificación: comprobaciones obligatorias y modos de fallo'
LEER = '## Leer primero'
COMANDOS = '## Comandos canonicos'
FORBIDDEN = ['transcripción y memoria del autor', 'sesiones ajenas', 'worktrees reales de otras unidades',
             'artefactos transitorios no enumerados']
INSTRUMENT_ACTION = ('<git declarado> --no-replace-objects cat-file blob %s | <interprete declarado> -I - <orden> ...')


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


DV = _load('derive_r4')          # strict_object (rule (i) JSON criteria), blob_id


class BuildError(Exception):
    def __init__(self, code, detail):
        Exception.__init__(self, code + ': ' + detail)
        self.code = code
        self.detail = detail


def genv():
    e = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith('GIT_'))
    e['GIT_OPTIONAL_LOCKS'] = '0'
    e['GIT_TERMINAL_PROMPT'] = '0'
    return e


def git(repo, *args):
    p = subprocess.run(['git', '--no-replace-objects', '-C', repo] + list(args), capture_output=True,
                       stdin=subprocess.DEVNULL, env=genv())
    return p.returncode, p.stdout


def gtext(repo, *args):
    rc, out = git(repo, *args)
    return rc, out.decode('utf-8', 'replace').strip()


def is_commit(repo, v):
    if not (isinstance(v, str) and HEX40.match(v)):
        return False
    rc, t = gtext(repo, 'cat-file', '-t', v)
    return rc == 0 and t == 'commit'


_TREES = {}


def blob_at(repo, c, p):
    """Blob id at path p of commit c, or None (absent, or not a blob): one recursive ls-tree per (repo, commit)."""
    if not c:
        return None
    key = (os.path.normcase(os.path.abspath(repo)), c)
    if key not in _TREES:
        rc, out = git(repo, 'ls-tree', '-r', '-z', '--full-tree', c)
        t = {}
        if rc == 0:
            for rec in out.split(b'\0'):
                if not rec:
                    continue
                meta, name = rec.split(b'\t', 1)
                mode, typ, oid = meta.decode('ascii').split()
                t[name.decode('utf-8', 'surrogateescape')] = (typ, oid)
        _TREES[key] = t
    typ, oid = _TREES[key].get(p, (None, None))
    return oid if typ == 'blob' else None


def git_blob_bytes(repo, blob):
    rc, out = git(repo, 'cat-file', 'blob', blob)
    if rc != 0 or DV.blob_id(out) != blob:
        return None
    return out


def rfile(p):
    with open(p, 'rb') as fh:
        return fh.read()


def roles_of(des):
    out = {}
    for i in des.get('Inputs') or []:
        out[i['Role']] = i
    return out


# ------------------------------------------------------------------ the builder
def build(des, variant, instrument=True):
    """Returns (E, report). Raises BuildError when an input the builder itself needs (contract, delegation, Target,
    AuthorityRevision, map) cannot be used: then there is no E (fail closed)."""
    if variant not in VARIANTS:
        raise BuildError('USAGE', 'variant ' + repr(variant))
    repo = des['Repo']
    ar = des['AuthorityRevision']
    if not is_commit(repo, ar):
        raise BuildError('AUTHORITY_REVISION', 'not a commit of the -C')
    inv = des['Invocation']
    tgt = (inv.get('Target') or {}).get('Commit')
    if not is_commit(repo, tgt):
        raise BuildError('TARGET', 'I.Target.Commit is not a commit of the -C')
    ro = roles_of(des)
    for r in ('DELEG', 'H', 'CONTRACT', 'TRX'):
        if r not in ro:
            raise BuildError('DESIGNATION', 'role %s not designated' % r)
    # contract: bytes from the custodied blob (Git) or the custodied file
    kin = ro['CONTRACT']
    if kin.get('Custody'):
        kbytes = rfile(kin['Custody']) if os.path.isfile(kin['Custody']) else None
        kblob = DV.blob_id(kbytes) if kbytes is not None else None
        if kin.get('Blob') and kblob != kin['Blob']:
            raise BuildError('CONTRACT', 'custodied bytes do not have the designated blob')
    else:
        kblob = kin.get('Blob')
        kbytes = git_blob_bytes(repo, kblob) if isinstance(kblob, str) else None
    if kbytes is None:
        raise BuildError('CONTRACT', 'custodied contract bytes not available')
    K, why = DV.strict_object(kbytes)
    if K is None:
        raise BuildError('CONTRACT', 'not a single JSON object: ' + why)
    auth = K.get('Authorities')
    if not isinstance(auth, list) or not all(isinstance(a, dict) and isinstance(a.get('Path'), str) and a['Path'] for a in auth):
        raise BuildError('CONTRACT', 'Authorities is not a list of objects with Path')
    # delegation: D.BaseSha bounds the object under verification
    dfile = ro['DELEG'].get('Custody')
    if not dfile or not os.path.isfile(dfile):
        raise BuildError('DELEG', 'custodied delegation absent')
    D, why = DV.strict_object(rfile(dfile))
    if D is None:
        raise BuildError('DELEG', 'not a single JSON object: ' + why)
    base = D.get('BaseSha')
    if not is_commit(repo, base):
        raise BuildError('DELEG', 'D.BaseSha is not a commit of the -C')
    d_main = D.get('MainSha')

    rep = {'Variant': variant, 'Instrument': bool(instrument and des.get('Instrument')), 'Entries': [], 'Excluded': [],
           'Notes': []}
    canon = []

    def add(ent, cls, origin):
        pair = (ent['Path'], ent['Blob'])
        if pair in [(e['Path'], e['Blob']) for e in canon]:
            rep['Notes'].append('duplicate pair not repeated: %s %s (%s)' % (ent['Path'], ent['Blob'], cls))
            return
        canon.append(ent)
        rep['Entries'].append({'Path': ent['Path'], 'Blob': ent['Blob'], 'Class': cls, 'PinnedBy': origin,
                               'BlobAtAuthorityRevision': blob_at(repo, ar, ent['Path']) if cls not in ('OFFGIT', 'INSTRUMENT') else None})

    add({'Path': kin['Path'], 'Blob': kblob}, 'CONTRACT', 'blob of the custodied contract (designation)')
    rep['ContractBlobAtAuthorityRevision'] = blob_at(repo, ar, kin['Path'])
    for a in auth:
        b = blob_at(repo, ar, a['Path'])
        if b:
            add({'Path': a['Path'], 'Blob': b}, 'AUTHORITY', 'AuthorityRevision')
        else:
            rep['Notes'].append('authority absent at the AuthorityRevision: ' + a['Path'])
    rc, out = git(repo, 'diff', '--name-only', '--no-renames', '-z', base + '..' + tgt)
    if rc != 0:
        raise BuildError('OBJECT', 'git diff D.BaseSha..I.Target.Commit failed')
    obj_paths = sorted(x.decode('utf-8') for x in out.split(b'\0') if x)
    rep['ObjectPaths'] = obj_paths
    for p in obj_paths:
        b = blob_at(repo, tgt, p)
        if b:
            add({'Path': p, 'Blob': b}, 'OBJECT', 'I.Target.Commit')
        else:
            rep['Notes'].append('object path deleted at the Target (no blob): ' + p)
    rec_paths = [a['Path'] for a in auth if a.get('Class') == 'UNIT_DOC']
    rep['WindowRecordPaths'] = rec_paths
    for p in rec_paths:
        b = blob_at(repo, tgt, p)
        if b:
            add({'Path': p, 'Blob': b}, 'RECORD', 'I.Target.Commit')
    for r in ('DELEG', 'H', 'TRX'):
        f = ro[r].get('Custody')
        if f and os.path.isfile(f):
            add({'Path': ro[r]['Path'], 'Blob': DV.blob_id(rfile(f))}, 'OFFGIT', 'blob of the custodied bytes (%s)' % r)
        else:
            rep['Notes'].append('off-Git input %s has no custodied bytes: no entry (a missing file has no blob)' % r)
    ins = des.get('Instrument') if instrument else None
    if ins:
        add({'Path': ins['Path'], 'Blob': ins['Blob']}, 'INSTRUMENT', 'A6-2 rule 1 (blob fixed by A-6)')

    # R7-B / R7-B-K: I6 literal on every Git entry; A6-2 rule 3 for the instrument entry
    if variant in ('R7-B', 'R7-B-K'):
        keep = []
        kept_rows = []
        for ent, row in zip(canon, rep['Entries']):
            cls = row['Class']
            if cls in ('OFFGIT', 'INSTRUMENT'):
                keep.append(ent)
                kept_rows.append(row)
                continue
            if cls == 'CONTRACT' and variant == 'R7-B-K':
                keep.append(ent)
                kept_rows.append(row)
                continue
            if row['BlobAtAuthorityRevision'] == ent['Blob']:
                keep.append(ent)
                kept_rows.append(row)
            else:
                rep['Excluded'].append({'Path': ent['Path'], 'Blob': ent['Blob'], 'Class': cls,
                                        'BlobAtAuthorityRevision': row['BlobAtAuthorityRevision'],
                                        'Cause': 'absent at the AuthorityRevision' if row['BlobAtAuthorityRevision'] is None
                                        else 'blob differs from the one at the AuthorityRevision'})
        canon = keep
        rep['Entries'] = kept_rows

    agents_b = blob_at(repo, ar, AGENTS)
    ap_b = blob_at(repo, ar, AP)
    if not agents_b or not ap_b:
        raise BuildError('PROFILE', 'AGENTS.md or AUTOMATION_PLAN.md absent at the AuthorityRevision')
    rb_agents = {'Path': AGENTS, 'Section': LEER, 'Blob': agents_b}
    rb_ap = {'Path': AP, 'Section': S1613, 'Blob': ap_b}
    rb_169 = {'Path': AP, 'Section': S169, 'Blob': ap_b}
    map_path = des['MapPath']
    map_b = blob_at(repo, ar, map_path)
    if not map_b:
        raise BuildError('MAP', 'clause map absent at the AuthorityRevision')
    mb = git_blob_bytes(repo, map_b)
    mapdoc, why = DV.strict_object(mb) if mb is not None else (None, 'unreadable')
    if mapdoc is None or not isinstance(mapdoc.get('Files'), list):
        raise BuildError('MAP', 'clause map unusable: ' + str(why))
    trans = []
    for p, rb in ((WF, rb_agents), (RAE, rb_agents), (map_path, rb_ap), (MAP_SCHEMA, rb_ap), (HANDOFF_SCHEMA, rb_169)):
        b = blob_at(repo, ar, p)
        if not b:
            raise BuildError('PROFILE', 'transitive input absent at the AuthorityRevision: ' + p)
        trans.append({'Path': p, 'Blob': b, 'RequiredBy': rb})
    for f in mapdoc['Files']:
        if not isinstance(f, dict) or f.get('FileKind') != 'MODIFIED':
            continue
        b = blob_at(repo, ar, f['Path'])
        ent = {'Path': f['Path'], 'Blob': b, 'RequiredBy': rb_ap}
        if b and ent not in trans and (f['Path'], b) not in [(e['Path'], e['Blob']) for e in canon]:
            trans.append(ent)
    # where the instrument reads the transitive inputs (clausemap: EFF = D.MainSha and EFF^1), the AR pin covers them
    # only if the blobs there are the same; recorded, never used to change E
    eff_rows = []
    if is_commit(repo, d_main):
        rc, eff1 = gtext(repo, 'rev-parse', '--verify', '--quiet', d_main + '^1')
        for e in trans:
            eff_rows.append({'Path': e['Path'], 'AtAR': e['Blob'], 'AtEFF': blob_at(repo, d_main, e['Path']),
                             'AtEFF1': blob_at(repo, eff1, e['Path']) if rc == 0 and eff1 else None})
    rep['TransitiveAtEFF'] = {'Rows': eff_rows, 'AllEqual': bool(eff_rows) and all(r['AtAR'] == r['AtEFF'] == r['AtEFF1'] for r in eff_rows)}

    obl = []
    for t in (WF, AP, RAE):
        obl.append({'SourcePath': AGENTS, 'SourceSection': LEER, 'SourceBlob': agents_b, 'Text': 'Leer primero: ' + t,
                    'Class': 'READ', 'Resolution': 'INCLUDED', 'Target': t, 'Authority': None, 'Reason': None})
    obl.append({'SourcePath': AGENTS, 'SourceSection': COMANDOS, 'SourceBlob': agents_b, 'Text': 'git status; git log --oneline -10',
                'Class': 'ACTION_COMPATIBLE', 'Resolution': 'ALLOWED', 'Target': 'git status', 'Authority': None, 'Reason': None})
    obl.append({'SourcePath': AGENTS, 'SourceSection': COMANDOS, 'SourceBlob': agents_b,
                'Text': 'dotnet test tests/Fixture.Tests/Fixture.Tests.csproj', 'Class': 'ACTION_INCOMPATIBLE', 'Resolution': 'EXEMPTED',
                'Target': 'dotnet test', 'Authority': 'CD-07 / OQ-17 (assumed exemption of the R3 evidence)', 'Reason': None})
    for t in (map_path, MAP_SCHEMA):
        obl.append({'SourcePath': AP, 'SourceSection': S1613, 'SourceBlob': ap_b,
                    'Text': 'M := mapa en EFF (ruta fija); valido contra el esquema en EFF; Validate(M)', 'Class': 'READ',
                    'Resolution': 'INCLUDED', 'Target': t, 'Authority': None, 'Reason': None})
    obl.append({'SourcePath': AP, 'SourceSection': S169, 'SourceBlob': ap_b, 'Text': 'Handoff: entrega presente y valida',
                'Class': 'READ', 'Resolution': 'INCLUDED', 'Target': HANDOFF_SCHEMA, 'Authority': None, 'Reason': None})
    actions = [{'Action': 'git status', 'RequiredBy': 'AGENTS.md#Comandos canonicos', 'Class': 'ACTION_COMPATIBLE',
                'ExemptionRef': None, 'ExemptionScope': None},
               {'Action': 'dotnet test tests/Fixture.Tests/Fixture.Tests.csproj', 'RequiredBy': 'AGENTS.md#Comandos canonicos',
                'Class': 'EXEMPTED', 'ExemptionRef': 'CD-07 / OQ-17 (assumed)', 'ExemptionScope': 'this VERIFY invocation'}]
    if ins:
        actions.append({'Action': INSTRUMENT_ACTION % ins['Blob'],
                        'RequiredBy': 'A6-2, regla 2 (I-62-A-6, blob 4bc919dd); DecisionRef de la disposicion que autorice la invocacion: '
                                      + (des.get('DecisionRef') or 'BANCO-m7 (sin disposicion)'),
                        'Class': 'ACTION_COMPATIBLE', 'ExemptionRef': None, 'ExemptionScope': None})
    E = {'Schema': SCHEMA, 'AuthorityRevision': ar, 'CanonicalInputs': canon,
         'AutomaticInstructions': [{'Path': AGENTS, 'Blob': agents_b, 'Adapter': des.get('Adapter') or 'codex-cli'}],
         'Obligations': obl, 'AllowedTransitiveInputs': trans, 'AllowedActions': actions,
         'DeclaredRuntimeContext': [{'Kind': 'ADAPTER_SYSTEM_INSTRUCTIONS', 'Source': 'codex-cli descriptor 155f3469 (not exposed)',
                                     'SizeOrSha256': '0'}],
         'ForbiddenInputs': list(FORBIDDEN), 'FixedPointIterations': 2}
    offgit = set(ro[r]['Path'] for r in ('DELEG', 'H', 'TRX'))
    rep['I6'] = i6_report(repo, E, ar, offgit, ins['Path'] if ins else None)
    return E, rep


def i6_report(repo, E, ar, offgit=(), instrument_path=None):
    """RAE §15.2 I6 per entry: a Git entry holds iff its blob is the one at the AuthorityRevision; off-Git artifacts (no
    path in the AuthorityRevision tree) and the instrument entry (A6-2 rule 3) are classified apart, never as holding."""
    rows = []
    for k in ('CanonicalInputs', 'AllowedTransitiveInputs', 'AutomaticInstructions'):
        for e in E[k]:
            at = blob_at(repo, ar, e['Path'])
            if e['Path'] == instrument_path:
                st = 'A6-2_RULE_3'
            elif e['Path'] in offgit:
                st = 'OFF_GIT'
            else:
                st = 'HOLDS' if at == e['Blob'] else ('ABSENT_AT_AR' if at is None else 'OTHER_BLOB')
            rows.append({'List': k, 'Path': e['Path'], 'Blob': e['Blob'], 'AtAR': at, 'Status': st})
    return {'Rows': rows, 'Violations': [r for r in rows if r['Status'] in ('ABSENT_AT_AR', 'OTHER_BLOB')],
            'OffGit': [r['Path'] for r in rows if r['Status'] == 'OFF_GIT'],
            'Instrument': [r['Path'] for r in rows if r['Status'] == 'A6-2_RULE_3']}


def serialize(E):
    """Same bytes as the custodied EIC of r2 part (d) and the E files of r4: UTF-8, indent 1, LF, final newline."""
    return (json.dumps(E, ensure_ascii=False, indent=1) + '\n').encode('utf-8')


def main(argv):
    args = [a for a in argv[1:] if a != '--no-instrument']
    if len(args) != 3:
        sys.stderr.write('usage: python -I make_eic_verify.py <designation.json> <R7-A|R7-B|R7-B-K> <out.json> [--no-instrument]\n')
        return 64
    des = json.loads(rfile(args[0]).decode('utf-8'))
    try:
        E, rep = build(des, args[1], instrument='--no-instrument' not in argv)
    except BuildError as e:
        sys.stderr.write('no E (fail closed): %s\n' % e)
        return 1
    with open(args[2], 'wb') as fh:
        fh.write(serialize(E))
    sys.stdout.write(json.dumps({'Excluded': rep['Excluded'], 'I6Violations': len(rep['I6']['Violations'])}, ensure_ascii=True) + '\n')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))

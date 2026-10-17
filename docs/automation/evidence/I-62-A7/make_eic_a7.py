# make_eic_a7.py - builder of the effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY invocation (and
# of the A6-7 measurement) under the CANDIDATE rule A7-1 (I-62 A-7, PROPUESTA ronda 1 as corrected after its preflight; not
# agreed, not applied), from a per-invocation designation (a6m7-designation/1) and the -C. Preflight of A-7 (decisiones
# §92, puntos 2 y 7). Stdlib only; every git call is read-only and uses --no-replace-objects.
#
#   python -I make_eic_a7.py <designation.json> <out.json> [--absent-a=no-entry|no-closure] [--no-instrument]
#
# "R7-A plus": the obligation table, the fixed point, the lists and their order are those of make_eic_verify.py R7-A
# (imported unchanged, SHA-256 58f13c6a...). What changes is the revision at which each entry is pinned (A7-1 rule 2):
#   (a) UNIT_DOC / UNIT_CHANGE citations of the contract, the automatic instruction and the transitive inputs of the fixed
#       point at the AuthorityRevision; EXTERNAL citations and WORKFLOW.md / AUTOMATION_PLAN.md in Evaluate at MainSha_eval
#       (= K.MainSha, or the designation's MainShaEval in a 16.7 reverification); the clause map, its schema and every
#       MODIFIED file of the map at EFF (= Derive(MainSha_eval), AP 16.13 / 16.14), and WORKFLOW.md and AUTOMATION_PLAN.md
#       in Validate at EFF; every MODIFIED file and AUTOMATION_PLAN.md in Validate at EFF^1. One blob per reading revision in
#       which it differs; ADDED / ENTRY map files (read by Validate only by blob id) are not entries.
#   (b) the exact object of the Target, every path of git diff --name-only <D.BaseSha>..<Target.commit> and the unit's
#       decisions and state files (docs/automation/decisions/<UnitId>.md, docs/automation/state/<UnitId>.yml) at Target.commit.
#   (c) contract, delegation, handoff and TRX by the Git blob of their custodied bytes at the designated paths.
#   (d) the instrument entry of A6-2 rule 1 (and its action, rule 2) only when the designation admits it.
# Absence (A7-1 rule 5 as corrected): (a) an (a) input absent at its revision gives no entry, recorded (the rule that requires
# it treats it: 16.3, 16.13 MAP_INVALID, 16.24); (b) the Target object absent -> no closure, a diff path or a unit record
# absent at Target.commit -> no entry; (c) without a present, blob-verified, readable contract and delegation there is no
# closure; an absent / other-blob / unreadable handoff or TRX gives no entry; every absence is recorded with its cause and
# nothing substitutes it. AGENTS.md and AUTOMATION_PLAN.md at the AuthorityRevision are the sources of the kit's 16.24 fixed
# point: without them the fixed point (16.24) cannot be computed and there is no closure.
# --absent-a=no-closure: the SUPERSEDED reading of rule 5 (a) (round-1 text before its correction), kept only as negative
# evidence of preflight finding F1.
import importlib.util, json, os, re, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
TRAILER = 'Agent-Protocol-Normative: I-62'
READINGS = ('no-entry', 'no-closure')


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


MEV = _load('make_eic_verify')   # R7-A builder, unchanged: constants, git helpers, serialization
DV = MEV.DV                      # derive_r4: strict_object, blob_id


class BuildError(Exception):
    def __init__(self, code, detail, rep=None):
        Exception.__init__(self, code + ': ' + detail)
        self.code = code
        self.detail = detail
        self.rep = rep


def decisions_path(unit):
    return 'docs/automation/decisions/%s.md' % unit


def state_path(unit):
    return 'docs/automation/state/%s.yml' % unit


def derive_eff(repo, main):
    """EFF := Derive(MainSha_eval) (AP 16.13 Classify step 1 / 16.14): the first-parent merge of main whose second parent
    reaches the unique commit with the trailer line and whose first parent does not. Returns (eff, eff1) or (None, None)
    (PRE_ACTIVATION); raises BuildError on a duplicated trailer or an ambiguous merge (ACTIVATION_INVALID)."""
    rc, out = MEV.git(repo, 'log', '--no-show-signature', '-z', '--format=%H%n%B', main)
    if rc != 0:
        raise BuildError('MAINSHA_EVAL', 'git log failed for MainSha_eval')
    trailer = []
    for rec in out.split(b'\0'):
        t = rec.decode('utf-8', 'replace').replace('\r\n', '\n').lstrip('\n')
        if not t.strip():
            continue
        sha, _, body = t.partition('\n')
        if TRAILER in body.split('\n'):
            trailer.append(sha.strip())
    if len(trailer) > 1:
        raise BuildError('ACTIVATION_INVALID', 'trailer line in %d commits' % len(trailer))
    if not trailer:
        return None, None
    rc, out = MEV.gtext(repo, 'rev-list', '--parents', main)
    parents = {}
    for line in out.split('\n'):
        p = line.split()
        if p:
            parents[p[0]] = p[1:]
    children = {}
    for c, ps in parents.items():
        for p in ps:
            children.setdefault(p, []).append(c)
    reach, stack = {trailer[0]}, [trailer[0]]
    while stack:
        cur = stack.pop()
        for ch in children.get(cur, []):
            if ch not in reach:
                reach.add(ch)
                stack.append(ch)
    rc, out = MEV.gtext(repo, 'rev-list', '--first-parent', '--merges', main)
    cands = [m for m in out.split() if len(parents.get(m, [])) >= 2 and parents[m][1] in reach and parents[m][0] not in reach]
    if len(cands) != 1:
        raise BuildError('ACTIVATION_INVALID', 'unique trailer commit with %d qualifying first-parent merges' % len(cands))
    return cands[0], parents[cands[0]][0]


def custodied(repo, role, i):
    """(bytes, blob, None) or (None, None, cause): category (c) by the Git blob of the custodied bytes (A7-1 rule 2 (c));
    a registered blob (designation field Blob) must match (rule 5 (c))."""
    cust, reg = i.get('Custody'), i.get('Blob')
    if cust:
        if not os.path.exists(cust):
            return None, None, 'ABSENT: the custodied file does not exist'
        try:
            b = MEV.rfile(cust)
        except OSError:
            return None, None, 'UNREADABLE: the custodied bytes cannot be read'
    else:
        if role != 'CONTRACT' or not isinstance(reg, str):
            return None, None, 'ABSENT: no custodied bytes designated'
        b = MEV.git_blob_bytes(repo, reg)
        if b is None:
            return None, None, 'ABSENT: the Git object of the registered blob is not available in the -C'
    bid = DV.blob_id(b)
    if isinstance(reg, str) and bid != reg:
        return None, None, 'BLOB_MISMATCH: the custodied bytes do not have the registered blob'
    return b, bid, None


def build(des, reading='no-entry', instrument=True):
    """Returns (E, report). Raises BuildError when there is no closure (fail closed); the error carries the report."""
    if reading not in READINGS:
        raise BuildError('USAGE', 'reading ' + repr(reading))
    rep = {'Rule': 'A7-1 (I-62 A-7, PROPUESTA ronda 1, corregida)', 'Reading': reading, 'Instrument': bool(instrument and des.get('Instrument')),
           'Entries': [], 'Absences': [], 'Notes': [], 'Revisions': {}}

    def fail(code, detail):
        raise BuildError(code, detail, rep)

    repo = des['Repo']
    ar = des['AuthorityRevision']
    if not MEV.is_commit(repo, ar):
        fail('AUTHORITY_REVISION', 'not a commit of the -C')
    inv = des['Invocation']
    T = inv.get('Target') if isinstance(inv.get('Target'), dict) else {}
    tgt = T.get('Commit')
    if not MEV.is_commit(repo, tgt):
        fail('TARGET', 'Target.commit is not a commit of the -C (rule 5 (b): no launch)')
    tblob = MEV.blob_at(repo, tgt, T.get('Path')) if isinstance(T.get('Path'), str) else None
    if tblob is None or (T.get('Blob') is not None and tblob != T.get('Blob')):
        fail('TARGET', 'the exact object of the Target is absent at Target.commit (rule 5 (b): no launch)')
    ro = MEV.roles_of(des)
    for r in ('DELEG', 'H', 'CONTRACT', 'TRX'):
        if r not in ro:
            fail('DESIGNATION', 'role %s not designated (rule 2 (c): the designation names it)' % r)
    rep['Revisions'] = {'AuthorityRevision': ar, 'TargetCommit': tgt}
    # (c) custodied artifacts: record every absence with its cause before anything else (rule 5 (c))
    rep['UnregisteredBlobs'] = [r for r in ('CONTRACT', 'DELEG', 'H', 'TRX') if not isinstance(ro[r].get('Blob'), str)]
    art = {}
    for r in ('CONTRACT', 'DELEG', 'H', 'TRX'):
        b, bid, cause = custodied(repo, r, ro[r])
        if cause:
            rep['Absences'].append({'Category': 'c', 'Role': r, 'Path': ro[r]['Path'], 'Cause': cause, 'Substituted': False})
        else:
            art[r] = (b, bid)
    if 'CONTRACT' not in art:
        fail('NO_CLOSURE', 'contract absent, with another blob or unreadable: (a) cannot be computed (rule 5 (c): no launch); '
                           'nothing substitutes it')
    K, why = DV.strict_object(art['CONTRACT'][0])
    if K is None:
        fail('NO_CLOSURE', 'contract bytes are not a single JSON object (%s): (a) cannot be resolved' % why)
    auth = K.get('Authorities')
    if not isinstance(auth, list) or not all(isinstance(a, dict) and isinstance(a.get('Path'), str) and a['Path'] for a in auth):
        fail('NO_CLOSURE', 'K.Authorities is not a list of objects with Path')
    mse = des.get('MainShaEval') or K.get('MainSha')
    if not MEV.is_commit(repo, mse):
        fail('MAINSHA_EVAL', 'MainSha_eval (K.MainSha) is not a commit of the -C')
    if 'DELEG' not in art:
        fail('NO_CLOSURE', 'delegation absent, with another blob or unreadable: (b) cannot be computed (rule 5 (c): no launch); nothing substitutes it')
    D, why = DV.strict_object(art['DELEG'][0])
    if D is None or not MEV.is_commit(repo, D.get('BaseSha')):
        fail('NO_CLOSURE', 'D.BaseSha unavailable (delegation not a JSON object or BaseSha not a commit): (b) undefined')
    base = D['BaseSha']
    eff, eff1 = derive_eff(repo, mse)
    if eff is None:
        fail('PROFILE', 'PRE_ACTIVATION at MainSha_eval: the kit profile (16.13 obligations) presupposes EFF')
    rep['Revisions'].update({'MainShaEval': mse, 'EFF': eff, 'EFF^1': eff1, 'BaseSha': base})
    rev_of = {'AR': ar, 'MainSha_eval': mse, 'EFF': eff, 'EFF^1': eff1, 'Target.commit': tgt}

    def absent_a(path, rv, kind):
        """Rule 5 (a): an (a) input absent at its revision gives no entry (the rule that requires it treats the absence);
        with the superseded reading ('no-closure', negative evidence only) there is no closure."""
        if reading == 'no-closure':
            fail('AUTHORITY_ABSENT', '%s absent at %s, read by %s (superseded reading of rule 5 (a): no closure)' % (path, rv, kind))
        rep['Absences'].append({'Category': 'a', 'Role': kind, 'Path': path, 'Revision': rv,
                                'Cause': 'absent at %s: no entry; treated by the rule that requires it (16.3, 16.13, 16.24)' % rv,
                                'Substituted': False})

    canon, trans = [], []

    def pairs():
        return set((e['Path'], e['Blob']) for e in canon)

    def add(ent, cat, kind, rev, lst='CanonicalInputs'):
        rep['Entries'].append({'List': lst, 'Path': ent['Path'], 'Blob': ent['Blob'], 'Category': cat, 'Kind': kind, 'Revision': rev,
                               'BlobAtAuthorityRevision': MEV.blob_at(repo, ar, ent['Path']) if cat in ('a', 'b') else None})

    def add_canon(path, blob, cat, kind, rev):
        if (path, blob) in pairs():
            rep['Notes'].append('pair already pinned, not repeated: %s %s (%s)' % (path, blob, kind))
            return
        canon.append({'Path': path, 'Blob': blob})
        add(canon[-1], cat, kind, rev)

    # CanonicalInputs, in the R7-A order
    add_canon(ro['CONTRACT']['Path'], art['CONTRACT'][1], 'c', 'CONTRACT', 'custody')
    for a in auth:
        cls = a.get('Class')
        if cls in ('UNIT_DOC', 'UNIT_CHANGE'):
            rv = 'AR'
        elif cls == 'EXTERNAL':
            rv = 'MainSha_eval'
        else:
            fail('NO_CLOSURE', 'authority with a Class outside UNIT_DOC/UNIT_CHANGE/EXTERNAL: %r' % (cls,))
        b = MEV.blob_at(repo, rev_of[rv], a['Path'])
        if b is None:
            absent_a(a['Path'], rv, 'AUTHORITY:' + cls)
            continue
        add_canon(a['Path'], b, 'a', 'AUTHORITY:' + cls, rv)
    rc, out = MEV.git(repo, 'diff', '--name-only', '--no-renames', '-z', base + '..' + tgt)
    if rc != 0:
        fail('OBJECT', 'git diff BaseSha..Target.commit failed')
    obj_paths = sorted(x.decode('utf-8') for x in out.split(b'\0') if x)
    rep['ObjectPaths'] = obj_paths
    for p in obj_paths:
        b = MEV.blob_at(repo, tgt, p)
        if b:
            add_canon(p, b, 'b', 'OBJECT', 'Target.commit')
        else:
            rep['Absences'].append({'Category': 'b', 'Role': 'DIFF_PATH', 'Path': p, 'Cause': 'deleted at Target.commit: no entry (rule 5 (b))',
                                    'Substituted': False})
    if (T['Path'], tblob) not in pairs():
        add_canon(T['Path'], tblob, 'b', 'TARGET', 'Target.commit')
    unit = inv.get('UnitId')
    rec_paths = [decisions_path(unit), state_path(unit)]
    rep['RecordPaths'] = rec_paths
    for p in rec_paths:
        b = MEV.blob_at(repo, tgt, p)
        if b:
            add_canon(p, b, 'b', 'RECORD', 'Target.commit')
        else:
            rep['Absences'].append({'Category': 'b', 'Role': 'RECORD', 'Path': p, 'Cause': 'absent at Target.commit: no entry (rule 5 (b))',
                                    'Substituted': False})
    for r in ('DELEG', 'H', 'TRX'):
        if r in art:
            add_canon(ro[r]['Path'], art[r][1], 'c', r, 'custody')
    ins = des.get('Instrument') if instrument else None
    if ins:
        add_canon(ins['Path'], ins['Blob'], 'd', 'INSTRUMENT', 'A-6 (A6-2 rule 1)')

    # profile (R7-A obligation table); instructions and transitives at the AuthorityRevision
    agents_b = MEV.blob_at(repo, ar, MEV.AGENTS)
    ap_b = MEV.blob_at(repo, ar, MEV.AP)
    if not agents_b or not ap_b:
        fail('PROFILE_16.24', 'AGENTS.md or AUTOMATION_PLAN.md absent at the AuthorityRevision: the 16.24 fixed point of the kit profile cannot be computed')
    rb_agents = {'Path': MEV.AGENTS, 'Section': MEV.LEER, 'Blob': agents_b}
    rb_ap = {'Path': MEV.AP, 'Section': MEV.S1613, 'Blob': ap_b}
    rb_169 = {'Path': MEV.AP, 'Section': MEV.S169, 'Blob': ap_b}
    map_path = des['MapPath']

    def add_trans(path, rv, rb, kind, always=False):
        b = MEV.blob_at(repo, rev_of[rv], path)
        if b is None:
            absent_a(path, rv, kind)
            return
        ent = {'Path': path, 'Blob': b, 'RequiredBy': rb}
        if not always and (ent in trans or (path, b) in pairs()):
            return
        trans.append(ent)
        add(ent, 'a', kind, rv, 'AllowedTransitiveInputs')

    # the five fixed transitive inputs of R7-A, in its order (map and schema at EFF: Resolve step 2)
    for p, rv, rb, kind in ((MEV.WF, 'AR', rb_agents, 'TRANSITIVE'), (MEV.RAE, 'AR', rb_agents, 'TRANSITIVE'),
                            (map_path, 'EFF', rb_ap, '16.13:MAP'), (MEV.MAP_SCHEMA, 'EFF', rb_ap, '16.13:MAP_SCHEMA'),
                            (MEV.HANDOFF_SCHEMA, 'AR', rb_169, 'TRANSITIVE')):
        add_trans(p, rv, rb, kind, always=True)
    # "Leer primero: AUTOMATION_PLAN" is a transitive read at the AuthorityRevision: present unless already pinned
    add_trans(MEV.AP, 'AR', rb_agents, 'TRANSITIVE')
    mblob = MEV.blob_at(repo, eff, map_path)
    mb = MEV.git_blob_bytes(repo, mblob) if mblob else None
    mapdoc, why = DV.strict_object(mb) if mb is not None else (None, 'absent')
    if mapdoc is None or not isinstance(mapdoc.get('Files'), list):
        if reading == 'no-closure':
            fail('MAP', 'clause map at EFF unusable: ' + str(why))
        rep['Absences'].append({'Category': 'a', 'Role': '16.13:MV-6', 'Path': map_path, 'Revision': 'EFF',
                                'Cause': 'clause map absent or unusable (%s): the MODIFIED files cannot be enumerated, no entry; '
                                         'Validate gives MAP_INVALID' % why, 'Substituted': False})
        mapdoc = {'Files': []}
    wide = []
    for f in mapdoc['Files']:
        if not isinstance(f, dict) or not isinstance(f.get('Path'), str):
            continue
        if f.get('FileKind') == 'MODIFIED':
            add_trans(f['Path'], 'EFF', rb_ap, '16.13:MV-6')
            add_trans(f['Path'], 'EFF^1', rb_ap, '16.13:MV-6')
        else:
            b = MEV.blob_at(repo, eff, f['Path'])
            if b and (f['Path'], b) not in pairs() and not any(e['Path'] == f['Path'] and e['Blob'] == b for e in trans):
                wide.append(f['Path'])
    rep['NotAdmittedMapFiles'] = wide     # ADDED / ENTRY map files (read only by blob id, MV-4): outside rule 1 (a) as corrected
    # MV-7 and the entry point (E1-E3), unless already pinned
    add_trans(MEV.WF, 'EFF', rb_ap, '16.13:MV-7')
    add_trans(MEV.AP, 'EFF', rb_ap, '16.13:MV-7')
    add_trans(MEV.AP, 'EFF^1', rb_ap, '16.13:MV-7')
    add_trans(MEV.WF, 'MainSha_eval', rb_ap, '16.13:E1-E2 entry point')
    add_trans(MEV.AP, 'MainSha_eval', rb_ap, '16.13:E3')

    obl = []
    for t in (MEV.WF, MEV.AP, MEV.RAE):
        obl.append({'SourcePath': MEV.AGENTS, 'SourceSection': MEV.LEER, 'SourceBlob': agents_b, 'Text': 'Leer primero: ' + t,
                    'Class': 'READ', 'Resolution': 'INCLUDED', 'Target': t, 'Authority': None, 'Reason': None})
    obl.append({'SourcePath': MEV.AGENTS, 'SourceSection': MEV.COMANDOS, 'SourceBlob': agents_b, 'Text': 'git status; git log --oneline -10',
                'Class': 'ACTION_COMPATIBLE', 'Resolution': 'ALLOWED', 'Target': 'git status', 'Authority': None, 'Reason': None})
    obl.append({'SourcePath': MEV.AGENTS, 'SourceSection': MEV.COMANDOS, 'SourceBlob': agents_b,
                'Text': 'dotnet test tests/Fixture.Tests/Fixture.Tests.csproj', 'Class': 'ACTION_INCOMPATIBLE', 'Resolution': 'EXEMPTED',
                'Target': 'dotnet test', 'Authority': 'CD-07 / OQ-17 (assumed exemption of the R3 evidence)', 'Reason': None})
    for t in (map_path, MEV.MAP_SCHEMA):
        obl.append({'SourcePath': MEV.AP, 'SourceSection': MEV.S1613, 'SourceBlob': ap_b,
                    'Text': 'M := mapa en EFF (ruta fija); valido contra el esquema en EFF; Validate(M)', 'Class': 'READ',
                    'Resolution': 'INCLUDED', 'Target': t, 'Authority': None, 'Reason': None})
    obl.append({'SourcePath': MEV.AP, 'SourceSection': MEV.S169, 'SourceBlob': ap_b, 'Text': 'Handoff: entrega presente y valida',
                'Class': 'READ', 'Resolution': 'INCLUDED', 'Target': MEV.HANDOFF_SCHEMA, 'Authority': None, 'Reason': None})
    actions = [{'Action': 'git status', 'RequiredBy': 'AGENTS.md#Comandos canonicos', 'Class': 'ACTION_COMPATIBLE',
                'ExemptionRef': None, 'ExemptionScope': None},
               {'Action': 'dotnet test tests/Fixture.Tests/Fixture.Tests.csproj', 'RequiredBy': 'AGENTS.md#Comandos canonicos',
                'Class': 'EXEMPTED', 'ExemptionRef': 'CD-07 / OQ-17 (assumed)', 'ExemptionScope': 'this VERIFY invocation'}]
    if ins:
        actions.append({'Action': MEV.INSTRUMENT_ACTION % ins['Blob'],
                        'RequiredBy': 'A6-2, regla 2 (I-62-A-6, blob 4bc919dd); DecisionRef de la disposicion que autorice la invocacion: '
                                      + (des.get('DecisionRef') or 'BANCO-m7 (sin disposicion)'),
                        'Class': 'ACTION_COMPATIBLE', 'ExemptionRef': None, 'ExemptionScope': None})
    E = {'Schema': MEV.SCHEMA, 'AuthorityRevision': ar, 'CanonicalInputs': canon,
         'AutomaticInstructions': [{'Path': MEV.AGENTS, 'Blob': agents_b, 'Adapter': des.get('Adapter') or 'codex-cli'}],
         'Obligations': obl, 'AllowedTransitiveInputs': trans, 'AllowedActions': actions,
         'DeclaredRuntimeContext': [{'Kind': 'ADAPTER_SYSTEM_INSTRUCTIONS', 'Source': 'codex-cli descriptor 155f3469 (not exposed)',
                                     'SizeOrSha256': '0'}],
         'ForbiddenInputs': list(MEV.FORBIDDEN), 'FixedPointIterations': 2}
    rep['Entries'].append({'List': 'AutomaticInstructions', 'Path': MEV.AGENTS, 'Blob': agents_b, 'Category': 'a', 'Kind': 'INSTRUCTION',
                           'Revision': 'AR', 'BlobAtAuthorityRevision': agents_b})
    rep['DifferFromAuthorityRevision'] = [{'Path': e['Path'], 'Blob': e['Blob'], 'Revision': e['Revision'], 'AtAR': e['BlobAtAuthorityRevision']}
                                          for e in rep['Entries'] if e['Category'] == 'a' and e['Blob'] != e['BlobAtAuthorityRevision']]
    return E, rep


def main(argv):
    args = [a for a in argv[1:] if not a.startswith('--')]
    reading = 'no-entry'
    for a in argv[1:]:
        if a.startswith('--absent-a='):
            reading = a.split('=', 1)[1]
    if len(args) != 2:
        sys.stderr.write('usage: python -I make_eic_a7.py <designation.json> <out.json> [--absent-a=no-entry|no-closure] [--no-instrument]\n')
        return 64
    des = json.loads(MEV.rfile(args[0]).decode('utf-8'))
    try:
        E, rep = build(des, reading, instrument='--no-instrument' not in argv)
    except BuildError as e:
        sys.stderr.write('no closure (fail closed): %s\n' % e)
        if e.rep is not None:
            with open(args[1] + '.report.json', 'wb') as fh:
                fh.write((json.dumps(dict(e.rep, NoClosure={'Code': e.code, 'Detail': e.detail}), ensure_ascii=True, indent=1) + '\n').encode('ascii'))
        return 1
    with open(args[1], 'wb') as fh:
        fh.write(MEV.serialize(E))
    with open(args[1] + '.report.json', 'wb') as fh:
        fh.write((json.dumps(rep, ensure_ascii=True, indent=1) + '\n').encode('ascii'))
    sys.stdout.write(json.dumps({'Entries': len(rep['Entries']), 'Absences': len(rep['Absences'])}) + '\n')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))

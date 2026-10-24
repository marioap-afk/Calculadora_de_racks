# patch_tools_r4.py - derives the pieces of the FOURTH candidate of I-62 A-7 (round 4; decisiones §98, punto 4) from the round-3 pieces
# (copied byte for byte from docs/automation/evidence/I-62-A7/r3/) by exact replacements: the designation format a7-designation/4, the
# builder make_eic_a7.py, the validator check_a7_closure.py (written from the A-7 text, independent of the builder), designate_a7.py and the
# round-2 library r2lib.py (only the format, its schema file, the base and the work subdirectory). Each old text must occur exactly once
# (or the declared number of times). derive_r4.py, make_eic_verify.py, designate_m7.py, lib_r1r4.py and check_citation_blobs.py are not
# touched.   python -I patch_tools_r4.py
import hashlib, json, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
R3_SHA = {'make_eic_a7.py': '8ebe617d87e19d0b703e3225184ec26090d8eb70181e65f272d0848ee1269f09',
          'check_a7_closure.py': '2afa162156ebb17aaeac0f5c3a5919efe23f87e486f2ca71b8475ef7565a0f50',
          'a7-designation-3.schema.json': 'a897f617a9ea19bdc4f6b35c6bcb44d36d864b186457e0d56752f1b1e0368e1b',
          'designate_a7.py': 'b0d4bf136e620ee5fa162f966f0ab28155ff8951ec36f1ce5bc2ae0142942222'}
BASE4 = '2405a919b8ce42c8b0b971d97e5f20294059b1e9'


def rd(name):
    with open(os.path.join(HERE, name), 'rb') as fh:
        return fh.read()


def wr(name, data):
    assert b'\r' not in data
    with open(os.path.join(HERE, name), 'wb') as fh:
        fh.write(data)


def patch(name, pairs, out=None):
    t = rd(name).decode('utf-8')
    for i, (old, new, count) in enumerate(pairs):
        n = t.count(old)
        if n != count:
            raise SystemExit('%s, replacement %d: %d occurrences (want %d): %r' % (name, i, n, count, old[:90]))
        t = t.replace(old, new)
    wr(out or name, t.encode('utf-8'))
    return out or name


for n, s in R3_SHA.items():
    assert hashlib.sha256(rd(n)).hexdigest() == s, 'not the round-3 piece: ' + n

# ------------------------------------------------------------------ a7-designation/4
d = json.loads(rd('a7-designation-3.schema.json').decode('utf-8'))
assert (json.dumps(d, indent=1) + '\n').encode('utf-8') == rd('a7-designation-3.schema.json')
d['$comment'] = ('a7-designation/4: per-invocation designation of an FX-02 VERIFY invocation (the verification, its 16.7 reverification, a negative '
                 'control) or of the A6-7 measurement under the candidate A7-1 round 4 (I-62 A-7, point 2 (c)): it names the role of every custodied '
                 'artifact of category (c) (at most one artifact per role) and registers its path and its blob; the rebase record (role REBASE, with its '
                 'RunId registered) only in the reverification (Invocation.Kind REVERIFICATION) and, as its copy, in a negative control with invocation '
                 'on the inputs of that reverification (Invocation.Kind REVERIFICATION_CONTROL, RAE §10), whose designation identifies the '
                 'reverification (Source). MainSha_eval is derived (K.MainSha, or the MainAfterSha of the rebase record of the reverification), never '
                 'designated. Part of the construction and validation procedure that A7-1 point 7 names as invalidator (identified by the SHA-256 of this '
                 'file).')
d['title'] = 'a7-designation/4'
d['properties']['Schema']['const'] = 'a7-designation/4'
k = d['properties']['Invocation']['properties']['Kind']
k['enum'] = ['VERIFY', 'REVERIFICATION', 'REVERIFICATION_CONTROL', 'MEASUREMENT']
k['$comment'] = ('absent = VERIFY (the verification and the negative controls on its inputs); REVERIFICATION = the 16.7 reverification without '
                 'conflicts; REVERIFICATION_CONTROL = a negative control with invocation on the inputs of that reverification (RAE §10), with Source; '
                 'only these two kinds have a REBASE role; MEASUREMENT = the A6-7 measurement invocation, which produces no verification and has no '
                 'relay facts (no RELAY role)')
d['properties']['Artifacts']['items']['properties']['RunId']['$comment'] = (
    'only for the REBASE role, where it is required (checked by the builder and the validator: the JSON-schema subset of the bench has no '
    'conditional keywords): the RunId of the rebase record (16.6), the name of its directory; in a REVERIFICATION_CONTROL, the RunId of the '
    'record of the reverification, whose copy it is')
d['properties']['Source'] = {
    'type': 'object', 'additionalProperties': False, 'required': ['Control', 'RunId', 'Designation', 'DesignationSha256'],
    'properties': {'Control': {'enum': ['nc1', 'nc2', 'nc3', 'n4', 'n5', 'n6', 'n7a', 'n7b'],
                               '$comment': 'the control (RAE §10 nc1-nc3; V14 D.3 N4-N7b), as in its path artifacts/orchestration/<UnitId>/<TaskId>-<control>/'},
                   'RunId': {'type': 'string', 'pattern': '^R[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$', '$comment': 'the RunId of the reverification'},
                   'Designation': {'type': 'string', 'minLength': 1, '$comment': 'the custodied a7-designation/4 of the reverification'},
                   'DesignationSha256': {'type': 'string', 'pattern': '^[0-9a-f]{64}$'}},
    '$comment': 'only (and always) for Invocation.Kind REVERIFICATION_CONTROL (checked by the builder and the validator): the reverification whose '
                'inputs the control copies'}
wr('a7-designation-4.schema.json', (json.dumps(d, indent=1) + '\n').encode('utf-8'))
os.remove(os.path.join(HERE, 'a7-designation-3.schema.json'))

# ------------------------------------------------------------------ builder
B_CTRL = '''

CONTROLS = ('nc1', 'nc2', 'nc3', 'n4', 'n5', 'n6', 'n7a', 'n7b')
# A7-1 round 4, point 2 (c): the artifact of (c) that each control mutates and the fields in which its copy may differ from the
# reverification's. nc1-nc3: RAE §10 (CurrentSha inexistente; AllowedWriteScope; a gate term in WorkCompleted). N4-N7b: V14 D.3 and G.2 name
# the effect, not the field (handoff ausente; 0 pruebas; trabajo sin commit, a state of the -C and of no input; otra corrida): the fields
# below are those of the A6-7 bench (evidence §181); any other realization has no closure (fail closed, never a false admission).
MUTATIONS = {'nc1': ('H', ('CurrentSha',)), 'nc2': ('DELEG', ('AllowedWriteScope',)), 'nc3': ('H', ('WorkCompleted',)), 'n4': ('H', None),
             'n5': ('H', ('TestResults',)), 'n6': (None, ()), 'n7a': ('H', ('DelegationRunId', 'RunId')),
             'n7b': ('H', ('DelegationRunId', 'RunId', 'CurrentSha'))}
RUN_FIELDS = ('DelegationRunId', 'RunId')


def control_main_sha_eval(repo, des, ro, art, inv, ar):
    """A7-1 round 4, points 2 (c) and 5 (c): (MainSha_eval, source report, None) for a negative control with invocation on the inputs of a
    16.7 reverification (Invocation.Kind REVERIFICATION_CONTROL), or (None, source report, cause). The coherence of the rebase record is the
    reverification's, checked with ITS inputs (round-3 rule), never with the mutated or absent delivery of the control."""
    src = des.get('Source') if isinstance(des.get('Source'), dict) else None
    srep = {'Source': src}
    if src is None or src.get('Control') not in CONTROLS or not RUN_ID.match(str(src.get('RunId'))) or not isinstance(src.get('Designation'), str) \\
            or not isinstance(src.get('DesignationSha256'), str):
        return None, srep, 'rule 2 (c): the control does not identify its reverification (Source: Control, RunId, Designation, DesignationSha256)'
    try:
        sb = MEV.rfile(src['Designation'])
    except OSError:
        return None, srep, 'rule 5 (c): the designation of the identified reverification is absent or unreadable'
    if hashlib.sha256(sb).hexdigest() != src['DesignationSha256']:
        return None, srep, 'rule 2 (c): the designation of the reverification does not have the identified SHA-256'
    S, why = DV.strict_object(sb)
    sinv = S.get('Invocation') if isinstance(S, dict) and isinstance(S.get('Invocation'), dict) else {}
    if S is None or S.get('Schema') != DES2 or sinv.get('Kind') != 'REVERIFICATION' or sinv.get('RunId') != src['RunId']:
        return None, srep, 'rule 2 (c): the identified designation is not that of a 16.7 reverification (a7-designation/4) with that RunId'
    for key in ('UnitId', 'TaskId', 'Attempt', 'Target'):
        if inv.get(key) != sinv.get(key):
            return None, srep, 'rule 2 (c): the control does not have the %s of the reverification' % key
    if S.get('AuthorityRevision') != ar:
        return None, srep, 'rule 2 (c): the control does not have the AuthorityRevision of the reverification'
    try:
        sro = designated_roles(S)
    except BuildError as e:
        return None, srep, 'rule 2 (c): the designation of the reverification is not usable (%s)' % e.detail
    sart = {}
    for r in ('CONTRACT', 'DELEG', 'H', 'REBASE', 'TRX_CURRENT', 'TRX_RED'):
        if r in sro:
            b, bid, cause = custodied(repo, r, sro[r])
            if cause is None:
                sart[r] = (b, bid)
    miss = [r for r in ('CONTRACT', 'DELEG', 'H', 'REBASE') if r not in sart]
    if miss:
        return None, srep, 'rule 5 (c): the reverification has no present, blob-verified %s' % ', '.join(miss)
    sK, sD, sH = (DV.strict_object(sart[r][0])[0] for r in ('CONTRACT', 'DELEG', 'H'))
    if sK is None or sD is None:
        return None, srep, 'rule 5 (c): the contract or the delegation of the reverification is not a JSON object'
    smse, why = rebase_main_sha_eval(repo, sro, sart, sinv, sK, sD, sH, ar)
    if smse is None:
        return None, srep, 'rule 2 (c): the identified reverification does not satisfy point 2 (c) (%s)' % why
    rid = sro['REBASE']['RunId']
    srep.update({'ReverificationRunId': src['RunId'], 'RebaseRunId': rid, 'RebaseBlob': sro['REBASE']['Blob']})
    if inv.get('RunId') in (src['RunId'], rid):
        return None, srep, 'rule 2 (c): the control has no RunId of its own (it is that of the reverification or of the rebase record)'
    ctl = src['Control']
    if 'REBASE' not in ro:
        return None, srep, 'rule 5 (c): the control designates no copy of the rebase record of the reverification: MainSha_eval cannot be fixed'
    i = ro['REBASE']
    parts = i['Path'].split('/')
    if i.get('RunId') != rid or i.get('Blob') != sro['REBASE']['Blob'] or not (
            len(parts) == 7 and parts[:2] == ['artifacts', 'orchestration'] and parts[2] == inv.get('UnitId')
            and parts[3] == '%s-%s' % (inv.get('TaskId'), ctl) and parts[4] == str(inv.get('Attempt')) and parts[5] == rid):
        return None, srep, ('rule 2 (c): the rebase record of the control is not the copy of the reverification\\'s (same blob and RunId) in '
                            'artifacts/orchestration/<UnitId>/<TaskId>-<control>/<Attempt>/<RunId>/')
    if 'REBASE' not in art:
        return None, srep, 'rule 5 (c): the copy of the rebase record is absent, unregistered, with another blob or unreadable'
    mrole, mfields = MUTATIONS[ctl]
    for r in ('CONTRACT', 'DELEG', 'H', 'TRX_CURRENT', 'TRX_RED'):
        if r in ro and r in ('DELEG', 'H'):
            pp = ro[r]['Path'].split('/')
            if not (len(pp) == 7 and pp[:2] == ['artifacts', 'orchestration'] and pp[2] == inv.get('UnitId') and pp[3] == '%s-%s' % (inv.get('TaskId'), ctl)
                    and pp[4] == str(inv.get('Attempt'))):
                return None, srep, 'rule 2 (c): the %s of the control is not in the control path <TaskId>-<control>/<Attempt>/' % r
        if r == mrole:
            continue
        if (r in ro) != (r in sro) or (r in ro and (ro[r].get('Blob') != sro[r].get('Blob') or (
                r not in ('DELEG', 'H') and ro[r].get('Path') != sro[r].get('Path')))):
            return None, srep, ('rule 2 (c): the %s of the control is not the copy of the reverification\\'s (same blob and the same path, or the control '
                                'path for the delegation and the delivery): an unauthorized substitution' % r)
    if mrole is not None and mfields is None:
        if mrole in art:
            return None, srep, 'rule 2 (c): %s leaves the delivery absent and the control has one' % ctl
    elif mrole is not None:
        mine = DV.strict_object(art[mrole][0])[0] if mrole in art else None
        theirs = DV.strict_object(sart[mrole][0])[0] if mrole in sart else None
        if mine is None or theirs is None:
            return None, srep, 'rule 2 (c): the %s that %s mutates is absent or not a JSON object' % (mrole, ctl)
        gone = object()
        diff = sorted(k for k in set(mine) | set(theirs) if mine.get(k, gone) != theirs.get(k, gone))
        srep['MutatedFields'] = diff
        if not diff:
            return None, srep, 'rule 2 (c): %s does not apply its mutation (the %s is the reverification\\'s)' % (ctl, mrole)
        if any(k not in mfields for k in diff):
            return None, srep, 'rule 2 (c): the %s of %s differs outside its mutated fields (%s)' % (mrole, ctl, ', '.join(k for k in diff if k not in mfields))
        if ctl == 'nc1' and MEV.is_commit(repo, mine.get('CurrentSha')):
            return None, srep, 'rule 2 (c): the CurrentSha of nc1 is a commit of the -C (RAE §10: inexistente)'
        if ctl in ('n7a', 'n7b') and not any(k in diff for k in RUN_FIELDS):
            return None, srep, 'rule 2 (c): the delivery of %s is not of another run' % ctl
    return smse, srep, None
'''
patch('make_eic_a7.py', [
    ('''# make_eic_a7.py (round 3) - builder of the effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY invocation
# (the verification, its 16.7 reverification, a negative control) and of the A6-7 measurement under the CANDIDATE rule A7-1 of
# I-62 A-7 round 3 (PROPUESTA; not agreed, not applied), from a per-invocation designation a7-designation/3
# (a7-designation-3.schema.json) and the -C. Preflight of A-7 round 3 (decisiones §96, puntos 2 y 3). Stdlib only; every git call
# is read-only and uses --no-replace-objects.
''', '''# make_eic_a7.py (round 4) - builder of the effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY invocation
# (the verification, its 16.7 reverification, a negative control) and of the A6-7 measurement under the CANDIDATE rule A7-1 of
# I-62 A-7 round 4 (PROPUESTA; not agreed, not applied), from a per-invocation designation a7-designation/4
# (a7-designation-4.schema.json) and the -C. Preflight of A-7 round 4 (decisiones §98, punto 4). Stdlib only; every git call
# is read-only and uses --no-replace-objects.
# Round 4 (A62-A7R3-R1): Invocation.Kind REVERIFICATION_CONTROL, a negative control with invocation on the inputs of a 16.7
# reverification (RAE §10). Its designation carries Source {Control, RunId, Designation, DesignationSha256}: the a7-designation/4 of that
# reverification, by its SHA-256, with Kind REVERIFICATION and that RunId, whose rebase record is present, blob-verified and coherent with
# ITS OWN delivery, Target and contract (the round-3 coherence, unchanged). The control has the UnitId, TaskId, Attempt, AuthorityRevision and
# Target of the reverification and a RunId of its own; its REBASE is the copy of the reverification's (same blob, same registered RunId) in
# artifacts/orchestration/<UnitId>/<TaskId>-<control>/<Attempt>/<RunId>/; every other designated (c) artifact is the copy of the
# reverification's (same blob), except RELAY (the control's own record) and the artifact that carries the fields the control mutates
# (MUTATIONS), which differs only in them, or the delivery that N4 leaves absent. MainSha_eval of the control = the MainAfterSha of that
# record, whatever its own (mutated or absent) delivery. Otherwise no closure, nothing substitutes it. REBASE stays not admitted in VERIFY
# (the verification and the controls on its inputs) and in MEASUREMENT. Round 3 text follows.
''', 1),
    ("import importlib.util, json, os, re, sys\n", "import hashlib, importlib.util, json, os, re, sys\n", 1),
    ("DES2 = 'a7-designation/3'      # round 3 (the name of the constant is kept)", "DES2 = 'a7-designation/4'      # round 4 (the name of the constant is kept)", 1),
    ("raise BuildError('DESIGNATION_FORMAT', 'not an a7-designation/3 designation (the round-1 format is read only as negative evidence)')",
     "raise BuildError('DESIGNATION_FORMAT', 'not an a7-designation/4 designation (the round-1 format is read only as negative evidence)')", 1),
    ("INVOCATION_KINDS = ('VERIFY', 'REVERIFICATION', 'MEASUREMENT')   # a7-designation/3 Invocation.Kind (absent = VERIFY)",
     "INVOCATION_KINDS = ('VERIFY', 'REVERIFICATION', 'REVERIFICATION_CONTROL', 'MEASUREMENT')   # a7-designation/4 Invocation.Kind (absent = VERIFY)\n"
     "REBASE_KINDS = ('REVERIFICATION', 'REVERIFICATION_CONTROL')   # round 4: the only kinds with a rebase record (the copy, in the control)", 1),
    ("\n\ndef red_required(repo, D, hbytes, rule='16.8'):", B_CTRL + "\n\ndef red_required(repo, D, hbytes, rule='16.8'):", 1),
    ("""        fail('DESIGNATION', 'Invocation.Kind %r is not VERIFY, REVERIFICATION or MEASUREMENT' % (kind,))
    rep['InvocationKind'] = kind""",
     """        fail('DESIGNATION', 'Invocation.Kind %r is not VERIFY, REVERIFICATION, REVERIFICATION_CONTROL or MEASUREMENT' % (kind,))
    if (kind == 'REVERIFICATION_CONTROL') != ('Source' in des):
        fail('DESIGNATION', 'Source is designated only (and always) for a control on the inputs of a reverification (A7-1 round 4, point 2 (c))')
    rep['InvocationKind'] = kind""", 1),
    ("""    if kind != 'REVERIFICATION' and 'REBASE' in ro:
        # A7-1 round 3, points 1 (c) and 5 (c): only the 16.7 reverification has a rebase record
        rep['Absences'] = [a for a in rep['Absences'] if a['Role'] != 'REBASE']
        rep['Absences'].append({'Category': 'c', 'Role': 'REBASE', 'Path': ro['REBASE']['Path'],
                                'Cause': 'NOT_ADMITTED: only the 16.7 reverification has a rebase record (A7-1, point 1 (c)); a %s invocation has none' % kind,""",
     """    if kind not in REBASE_KINDS and 'REBASE' in ro:
        # A7-1 round 4, points 1 (c) and 5 (c): only the 16.7 reverification and the controls on its inputs have a rebase record
        rep['Absences'] = [a for a in rep['Absences'] if a['Role'] != 'REBASE']
        rep['Absences'].append({'Category': 'c', 'Role': 'REBASE', 'Path': ro['REBASE']['Path'],
                                'Cause': 'NOT_ADMITTED: only the 16.7 reverification and the controls on its inputs have a rebase record (A7-1, point 1 '
                                         '(c)); a %s invocation has none' % kind,""", 1),
    ("    if kind != 'REVERIFICATION' and not MEV.is_commit(repo, mse):", "    if kind not in REBASE_KINDS and not MEV.is_commit(repo, mse):", 1),
    ("""        rep['MainShaEvalSource'] = 'MainAfterSha of the RebaseMap of the rebase record %s (16.13: 16.7 reverification)' % ro['REBASE']['RunId']
    else:""",
     """        rep['MainShaEvalSource'] = 'MainAfterSha of the RebaseMap of the rebase record %s (16.13: 16.7 reverification)' % ro['REBASE']['RunId']
    elif kind == 'REVERIFICATION_CONTROL':
        mse, srep, why = control_main_sha_eval(repo, des, ro, art, inv, ar)
        rep['Source'] = srep
        if mse is None:
            fail('NO_CLOSURE', why + '; nothing substitutes it (no other record, no other run, not K.MainSha)')
        rep['MainShaEvalSource'] = ('MainAfterSha of the RebaseMap of the rebase record %s of the reverification %s (copy in the control %s; 16.13)'
                                    % (srep['RebaseRunId'], srep['ReverificationRunId'], des['Source']['Control']))
    else:""", 1),
    ("rep = {'Rule': 'A7-1 (I-62 A-7, PROPUESTA ronda 3)'", "rep = {'Rule': 'A7-1 (I-62 A-7, PROPUESTA ronda 4)'", 1),
])

# ------------------------------------------------------------------ validator (independent of the builder)
V_CTRL = '''CONTROLS = ('nc1', 'nc2', 'nc3', 'n4', 'n5', 'n6', 'n7a', 'n7b')
# the artifact each control mutates and the fields in which it may differ (RAE §10 for nc1-nc3; V14 D.3 / G.2 name only the effect of
# N4-N7b, so their fields are those of the A6-7 bench, evidence §181; another realization is not coherent)
MUTATIONS = {'nc1': ('H', ('CurrentSha',)), 'nc2': ('DELEG', ('AllowedWriteScope',)), 'nc3': ('H', ('WorkCompleted',)), 'n4': ('H', None),
             'n5': ('H', ('TestResults',)), 'n6': (None, ()), 'n7a': ('H', ('DelegationRunId', 'RunId')),
             'n7b': ('H', ('DelegationRunId', 'RunId', 'CurrentSha'))}


def control_coherence(repo, des, roles, cbytes, inv, ar):
    """A7-1 round 4, points 2 (c) and 5 (c), written from the A-7 text: (MainSha_eval, problems) of a negative control on the inputs of a 16.7
    reverification; MainSha_eval is None when there is any problem."""
    src = des.get('Source')
    if not isinstance(src, dict) or src.get('Control') not in CONTROLS or not RUN_ID.match(str(src.get('RunId'))) \\
            or not isinstance(src.get('Designation'), str) or not isinstance(src.get('DesignationSha256'), str):
        return None, ['the control does not identify its reverification (Source)']
    try:
        with open(src['Designation'], 'rb') as fh:
            sb = fh.read()
    except OSError:
        return None, ['the designation of the reverification is absent or unreadable']
    if hashlib.sha256(sb).hexdigest() != src['DesignationSha256']:
        return None, ['the designation of the reverification does not have the identified SHA-256']
    S = json_object(sb) or {}
    sinv = S.get('Invocation') if isinstance(S.get('Invocation'), dict) else {}
    if S.get('Schema') != DES_SCHEMA or sinv.get('Kind') != 'REVERIFICATION' or sinv.get('RunId') != src['RunId']:
        return None, ['the identified designation is not that of a 16.7 reverification with that RunId']
    out = ['the control has another %s than the reverification' % k for k in ('UnitId', 'TaskId', 'Attempt', 'Target') if inv.get(k) != sinv.get(k)]
    if S.get('AuthorityRevision') != ar:
        out.append('the control has another AuthorityRevision than the reverification')
    sroles = {}
    for item in S.get('Artifacts') or []:
        r = item.get('Role') if isinstance(item, dict) else None
        if r not in ROLES2 or r in sroles:
            return None, out + ['the designation of the reverification has a role outside the seven or a repeated one']
        sroles[r] = item
    sbytes = {}
    for r in ('CONTRACT', 'DELEG', 'H', 'REBASE', 'TRX_CURRENT', 'TRX_RED'):
        if r in sroles:
            b, cause = read_custody(repo, r, sroles[r])
            if cause is None:
                sbytes[r] = b
    miss = [r for r in ('CONTRACT', 'DELEG', 'H', 'REBASE') if r not in sbytes]
    if miss:
        return None, out + ['the reverification has no present, blob-verified %s' % ', '.join(miss)]
    rp = rebase_coherence(repo, sroles, sbytes['REBASE'], sinv, json_object(sbytes['CONTRACT']), json_object(sbytes['DELEG']),
                          json_object(sbytes['H']), ar)
    if rp:
        return None, out + ['the reverification does not satisfy point 2 (c): %s' % '; '.join(rp)]
    rid = sroles['REBASE'].get('RunId')
    if inv.get('RunId') in (src['RunId'], rid):
        out.append('the control has no RunId of its own')
    ctl = src['Control']
    i = roles.get('REBASE')
    if i is None:
        return None, out + ['the control designates no copy of the rebase record of the reverification']
    m = re.fullmatch(r'artifacts/orchestration/([^/]+)/([^/]+)/([0-9]+)/([^/]+)/[^/]+', i.get('Path') or '')
    if i.get('RunId') != rid or i.get('Blob') != sroles['REBASE'].get('Blob') or not (
            m and m.group(1) == inv.get('UnitId') and m.group(2) == '%s-%s' % (inv.get('TaskId'), ctl) and m.group(3) == str(inv.get('Attempt'))
            and m.group(4) == rid):
        out.append('the rebase record of the control is not the copy of the reverification\\'s (same blob and RunId) under <TaskId>-<control>/<Attempt>/<RunId>/')
    if 'REBASE' not in cbytes:
        out.append('the copy of the rebase record is absent, unregistered, with another blob or unreadable')
    mrole, mfields = MUTATIONS[ctl]
    for r in ('CONTRACT', 'DELEG', 'H', 'TRX_CURRENT', 'TRX_RED'):
        if r in roles and r in ('DELEG', 'H'):
            m = re.fullmatch(r'artifacts/orchestration/([^/]+)/([^/]+)/([0-9]+)/[^/]+/[^/]+', roles[r].get('Path') or '')
            if not (m and m.group(1) == inv.get('UnitId') and m.group(2) == '%s-%s' % (inv.get('TaskId'), ctl) and m.group(3) == str(inv.get('Attempt'))):
                out.append('the %s of the control is not in the control path' % r)
        if r == mrole:
            continue
        if (r in roles) != (r in sroles):
            out.append('the %s of the control is not the copy of the reverification\\'s (designated in only one of them)' % r)
        elif r in roles and roles[r].get('Blob') != sroles[r].get('Blob'):
            out.append('the %s of the control is not the copy of the reverification\\'s (another blob)' % r)
        elif r in roles and r not in ('DELEG', 'H') and roles[r].get('Path') != sroles[r].get('Path'):
            out.append('the %s of the control is not the copy of the reverification\\'s (another path: another run)' % r)
    if mrole is not None and mfields is None:
        if mrole in cbytes:
            out.append('%s leaves the delivery absent and the control has one' % ctl)
    elif mrole is not None:
        a = json_object(cbytes[mrole]) if mrole in cbytes else None
        b = json_object(sbytes[mrole]) if mrole in sbytes else None
        if a is None or b is None:
            out.append('the %s mutated by %s, or the reverification\\'s, is absent or not a JSON object' % (mrole, ctl))
        else:
            diff = sorted(k for k in set(a) | set(b) if (k in a, a.get(k)) != (k in b, b.get(k)))
            if not diff:
                out.append('%s: the mutation is not applied' % ctl)
            elif [k for k in diff if k not in mfields]:
                out.append('%s: the %s differs outside the mutated fields (%s)' % (ctl, mrole, ', '.join(k for k in diff if k not in mfields)))
            elif ctl == 'nc1' and repo.is_commit(a.get('CurrentSha')):
                out.append('nc1: the CurrentSha of the copy is a commit of the -C (RAE §10: inexistente)')
            elif ctl in ('n7a', 'n7b') and not set(diff) & set(('DelegationRunId', 'RunId')):
                out.append('%s: the delivery is not of another run' % ctl)
    if out:
        return None, out
    return json_object(sbytes['REBASE'])['RebaseMap']['MainAfterSha'], []


'''
patch('check_a7_closure.py', [
    ('# check_a7_closure.py (round 3) - validator of an effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY\n',
     '''# check_a7_closure.py (round 4) - A7-1 of I-62 A-7 round 4 (decisiones §98; A62-A7R3-R1), written from the A-7 text and independent of
# the builder: Invocation.Kind REVERIFICATION_CONTROL, a negative control with invocation on the inputs of a 16.7 reverification (RAE §10).
# Its designation names the reverification (Source: Control, RunId, Designation, DesignationSha256); the validator reads that designation,
# checks its SHA-256, its kind and RunId, re-reads its CONTRACT, DELEG, H and REBASE from their custody and applies to it the round-3
# coherence (with the reverification's own delivery, Target and contract); then the control: the same UnitId, TaskId, Attempt,
# AuthorityRevision and Target, a RunId of its own, REBASE = the copy of the reverification's (same blob and registered RunId) under
# <TaskId>-<control>/<Attempt>/<RunId>/, every other designated (c) artifact a copy (same blob) except RELAY and the mutated artifact, which
# differs only in the fields of its control (MUTATIONS; N4: the delivery absent; N6: nothing). MainSha_eval of the control = the
# MainAfterSha of the reverification's record. REBASE stays FOREIGN in VERIFY and MEASUREMENT. Round 3 text follows:
# check_a7_closure.py (round 3) - validator of an effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY
''', 1),
    ("DES_SCHEMA = 'a7-designation/3'", "DES_SCHEMA = 'a7-designation/4'", 1),
    ("\nA6_INSTRUMENT_BLOB = ", "\n" + V_CTRL + "A6_INSTRUMENT_BLOB = ", 1),
    ("""    if kind not in ('VERIFY', 'REVERIFICATION', 'MEASUREMENT'):
        prob.append('DESIGNATION: Invocation.Kind %r is not VERIFY, REVERIFICATION or MEASUREMENT' % (kind,))""",
     """    if kind not in ('VERIFY', 'REVERIFICATION', 'REVERIFICATION_CONTROL', 'MEASUREMENT'):
        prob.append('DESIGNATION: Invocation.Kind %r is not VERIFY, REVERIFICATION, REVERIFICATION_CONTROL or MEASUREMENT' % (kind,))
    if ('Source' in des) != (kind == 'REVERIFICATION_CONTROL'):
        prob.append('DESIGNATION: Source only (and always) in a control on the inputs of a reverification')""", 1),
    ("""    if 'REBASE' in roles and kind != 'REVERIFICATION':
        # rule 1 (c) round 3: only the 16.7 reverification has a rebase record""",
     """    if 'REBASE' in roles and kind not in ('REVERIFICATION', 'REVERIFICATION_CONTROL'):
        # rule 1 (c) round 4: only the 16.7 reverification and the controls on its inputs have a rebase record""", 1),
    ("""                v['MainShaEvalSource'] = 'MainAfterSha of the rebase record %s' % roles['REBASE']['RunId']
    eff = eff1 = None""",
     """                v['MainShaEvalSource'] = 'MainAfterSha of the rebase record %s' % roles['REBASE']['RunId']
    elif kind == 'REVERIFICATION_CONTROL':
        mse2, cp = control_coherence(repo, des, roles, cbytes, inv, ar)
        v['ControlCoherence'] = cp
        if cp:
            nc.append('rule 2 (c) / 5 (c): the control on the inputs of a reverification is not coherent (%s)' % '; '.join(cp))
            mse = None
        else:
            mse = mse2
            v['MainShaEvalSource'] = 'MainAfterSha of the rebase record %s of the reverification %s (copy in the control)' % (
                roles['REBASE']['RunId'], des['Source']['RunId'])
    eff = eff1 = None""", 1),
])

# ------------------------------------------------------------------ designate_a7.py
patch('designate_a7.py', [
    ("SCHEMA = 'a7-designation/3'   # round 3 (I-62 A-7, decisiones §96): REBASE (the rebase record of the 16.7 reverification), no Name in C6",
     "SCHEMA = 'a7-designation/4'   # round 4 (I-62 A-7, decisiones §98): REBASE also as the copy in a control on the inputs of a reverification\n"
     "REBASE_KINDS = ('REVERIFICATION', 'REVERIFICATION_CONTROL')", 1),
    ("Problems=[{'Code': 'DESIGNATION_SCHEMA', 'Detail': 'not a7-designation/3'}]", "Problems=[{'Code': 'DESIGNATION_SCHEMA', 'Detail': 'not a7-designation/4'}]", 1),
    ("""        if inv.get('Kind', 'VERIFY') != 'REVERIFICATION' and pinned.get('REBASE') is not None:
            probs.append({'Code': 'REBASE_NOT_ADMITTED', 'Detail': 'E pins a rebase record in an invocation that is not the 16.7 reverification'})
        m = re.fullmatch(r'artifacts/orchestration/([^/]+)/([^/]+)/([0-9]+)/([^/]+)/[^/]+', ro['REBASE']['Path'])
        if not m or m.group(1) != inv.get('UnitId') or m.group(2) != inv.get('TaskId') or m.group(3) != str(inv.get('Attempt')) \\
                or m.group(4) != ro['REBASE'].get('RunId'):
            probs.append({'Code': 'KPR-4', 'Detail': 'REBASE path is not in the attempt directory under its registered RunId'})
    elif inv.get('Kind') == 'REVERIFICATION':
        probs.append({'Code': 'ROLE_MISSING', 'Detail': 'REBASE (16.7 reverification)'})""",
     """        if inv.get('Kind', 'VERIFY') not in REBASE_KINDS and pinned.get('REBASE') is not None:
            probs.append({'Code': 'REBASE_NOT_ADMITTED', 'Detail': 'E pins a rebase record in an invocation that is neither the 16.7 reverification nor a '
                                                                   'control on its inputs'})
        # the reverification: the attempt directory of the task; a control on its inputs: the attempt directory of the control path
        tdir = inv.get('TaskId') if inv.get('Kind') != 'REVERIFICATION_CONTROL' else '%s-%s' % (inv.get('TaskId'), (des.get('Source') or {}).get('Control'))
        m = re.fullmatch(r'artifacts/orchestration/([^/]+)/([^/]+)/([0-9]+)/([^/]+)/[^/]+', ro['REBASE']['Path'])
        if not m or m.group(1) != inv.get('UnitId') or m.group(2) != tdir or m.group(3) != str(inv.get('Attempt')) \\
                or m.group(4) != ro['REBASE'].get('RunId'):
            probs.append({'Code': 'KPR-4', 'Detail': 'REBASE path is not in the attempt directory (of the task, or of the control path) under its registered RunId'})
    elif inv.get('Kind') in REBASE_KINDS:
        probs.append({'Code': 'ROLE_MISSING', 'Detail': 'REBASE (16.7 reverification or a control on its inputs)'})""", 1),
])

# ------------------------------------------------------------------ r2lib.py (format, schema file, base, work subdirectory)
patch('r2lib.py', [
    ('''# r2lib.py - the round-2 preflight runner (run_a7r2.py) as a library for the round-3 preflight (run_a7r3.py; decisiones §96): phases q1-q6
# reused with the round-3 pieces; only the designation format (a7-designation/3), its schema file, the base and the work subdirectory change.''',
     '''# r2lib.py - the round-2 preflight runner (run_a7r2.py) as a library for the round-4 preflight (run_a7r4.py; decisiones §98), as in round 3:
# phases q1-q6 reused with the round-4 pieces; only the designation format (a7-designation/4), its schema file, the base and the work
# subdirectory change (round-3 text: «for the round-3 preflight (run_a7r3.py; decisiones §96) ... (a7-designation/3)»).''', 1),
    ("BASE = '5da2007f45160ced37b958a3a772c46ac64874ee'   # round 3 (r2lib)", "BASE = '%s'   # round 4 (r2lib)" % BASE4, 1),
    ("d['Schema'] = 'a7-designation/3'", "d['Schema'] = 'a7-designation/4'", 1),
    ("des = {'Schema': 'a7-designation/3', 'Case': 'B1-LIKE-'", "des = {'Schema': 'a7-designation/4', 'Case': 'B1-LIKE-'", 1),
    ("a7-designation-3.schema.json", "a7-designation-4.schema.json", 4),
    ("d = os.path.join(WORK, 'r3', name)", "d = os.path.join(WORK, 'r4', name)", 1),
])
for n in ('a7-designation-4.schema.json', 'make_eic_a7.py', 'check_a7_closure.py', 'designate_a7.py', 'r2lib.py'):
    sys.stdout.write('%s %s\n' % (hashlib.sha256(rd(n)).hexdigest(), n))

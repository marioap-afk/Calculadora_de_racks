# patch_tools_r3.py - derives the round-3 pieces of the A7-1 procedure (I-62 A-7 round 3; decisiones §96, puntos 2 y 3) from the round-2 copies by
# exact replacements: the builder make_eic_a7.py, the validator check_a7_closure.py and the designation check designate_a7.py (the derivation
# derive_r4.py is not changed). Round 3 adds the seventh role REBASE (the rebase record of the 16.7 reverification, relay-record/v2 with
# Phase REBASE and its RebaseMap) and the invocation kind REVERIFICATION; MainSha_eval is derived (K.MainSha, or the MainAfterSha of the coherent
# rebase record in the reverification) and never designated. The builder and the validator check the coherence independently.
import hashlib, os, sys
HERE = os.path.dirname(os.path.abspath(__file__))
SRC = {'make_eic_a7.py': 'c0cd1d1b', 'check_a7_closure.py': None, 'designate_a7.py': None}


def patch(name, reps):
    p = os.path.join(HERE, name)
    t = open(p, encoding='utf-8', newline='').read()
    for a, b in reps:
        assert t.count(a) == 1, (name, 'not unique', a[:120])
        t = t.replace(a, b)
    open(p, 'w', encoding='utf-8', newline='').write(t)
    print(name, hashlib.sha256(t.encode('utf-8')).hexdigest()[:12])


RB_DOC = '''RunId registered by the designation, the name of its directory and distinct from the invocation, the delegation and the delivery; TaskId and
Attempt of the invocation; a RebaseMap without conflict paths, unmapped entries or commits without image; MainBeforeSha = K.MainSha;
AuthorityRevision = the contract's, with UnitDocBlobsEqual and UnitChangeSectionsEqual true (16.7); BaseSha = D.BaseSha; and one commit whose
OriginalSha is the CurrentSha of the delivery, with ImageSha = Target.commit and PatchIdEqual true. MainSha_eval of the reverification = MainAfterSha.'''

# ------------------------------------------------------------------ builder
B = []
B.append(('''# make_eic_a7.py (round 2) - builder of the effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY invocation
# (and of the A6-7 measurement) under the CANDIDATE rule A7-1 of I-62 A-7 round 2 (PROPUESTA; not agreed, not applied), from a
# per-invocation designation a7-designation/2 (a7-designation-2.schema.json) and the -C. Preflight of A-7 round 2 (decisiones
# §94, puntos 2 y 3). Stdlib only; every git call is read-only and uses --no-replace-objects.''',
          '''# make_eic_a7.py (round 3) - builder of the effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY invocation
# (the verification, its 16.7 reverification, a negative control) and of the A6-7 measurement under the CANDIDATE rule A7-1 of
# I-62 A-7 round 3 (PROPUESTA; not agreed, not applied), from a per-invocation designation a7-designation/3
# (a7-designation-3.schema.json) and the -C. Preflight of A-7 round 3 (decisiones §96, puntos 2 y 3). Stdlib only; every git call
# is read-only and uses --no-replace-objects.
# Round 3 (A62-A7R2-R1): a seventh role REBASE, only in the 16.7 reverification (Invocation.Kind REVERIFICATION): the rebase record
# of the session (relay-record/v2, Phase REBASE, with its RebaseMap), by the Git blob of its custodied bytes, in the attempt directory
# artifacts/orchestration/<UnitId>/<TaskId>/<Attempt>/<RunId>/ with its own RunId registered. It is coherent with: '''
          + RB_DOC.replace('\n', '\n# ') + '''
# Without a present, blob-verified, readable and coherent rebase record (or without the delivery that the coherence needs) the
# reverification has no MainSha_eval: no closure, no launch, nothing substitutes it. In any other invocation a designated REBASE is
# not admitted (recorded). MainSha_eval is never designated: a MainShaEval field in the designation is a format error.
# Round 2 text follows.'''))
B.append(('''DES2 = 'a7-designation/2'
ROLES2 = ('CONTRACT', 'DELEG', 'H', 'TRX_CURRENT', 'TRX_RED', 'RELAY')''',
          '''DES2 = 'a7-designation/3'      # round 3 (the name of the constant is kept)
ROLES2 = ('CONTRACT', 'DELEG', 'H', 'TRX_CURRENT', 'TRX_RED', 'RELAY', 'REBASE')
RUN_ID = re.compile(r'R[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}\\Z')
HEX40 = re.compile(r'[0-9a-f]{40}\\Z')'''))
B.append(('''        raise BuildError('DESIGNATION_FORMAT', 'not an a7-designation/2 designation (the round-1 format is read only as negative evidence)')''',
          '''        raise BuildError('DESIGNATION_FORMAT', 'not an a7-designation/3 designation (the round-1 format is read only as negative evidence)')
    if 'MainShaEval' in des:
        raise BuildError('DESIGNATION_FORMAT', 'MainShaEval in the designation: MainSha_eval is derived (K.MainSha, or the MainAfterSha of the rebase '
                                               'record in the 16.7 reverification), never designated')'''))
B.append(('''            raise BuildError('DESIGNATION', 'artifact with a role outside the six of rule 1 (c): %r' % (r,))''',
          '''            raise BuildError('DESIGNATION', 'artifact with a role outside the seven of rule 1 (c): %r' % (r,))
        if (r == 'REBASE') != ('RunId' in i):
            raise BuildError('DESIGNATION', 'the RunId is registered for the REBASE role and only for it (rule 2 (c))')
        if r == 'REBASE' and not (isinstance(i.get('RunId'), str) and RUN_ID.match(i['RunId'])):
            raise BuildError('DESIGNATION', 'REBASE with a RunId that is not R<yyyyMMddTHHmmssZ>-<4 hex> (16.6)')'''))
B.append(('''INVOCATION_KINDS = ('VERIFY', 'MEASUREMENT')   # a7-designation/2 Invocation.Kind (absent = VERIFY)''',
          '''INVOCATION_KINDS = ('VERIFY', 'REVERIFICATION', 'MEASUREMENT')   # a7-designation/3 Invocation.Kind (absent = VERIFY)


def rebase_main_sha_eval(repo, ro, art, inv, K, D, H, ar):
    """A7-1 round 3, points 2 (c) and 5 (c): (MainSha_eval, None) from the coherent rebase record of the 16.7 reverification, or (None, cause)."""
    if 'REBASE' not in ro:
        return None, 'rule 5 (c): the reverification designates no rebase record: MainSha_eval cannot be fixed (no closure, no launch)'
    if 'REBASE' not in art:
        return None, 'rule 5 (c): the rebase record is absent, unregistered, with another blob or unreadable: MainSha_eval cannot be fixed'
    if H is None:
        return None, 'rule 5 (c): without the delivery the coherence of the rebase record cannot be checked: no closure'
    i = ro['REBASE']
    rid = i.get('RunId')
    parts = i['Path'].split('/')
    if not (len(parts) == 7 and parts[:2] == ['artifacts', 'orchestration'] and parts[2] == inv.get('UnitId') and parts[3] == inv.get('TaskId')
            and parts[4] == str(inv.get('Attempt')) and parts[5] == rid):
        return None, ('rule 2 (c): the rebase record is not in the attempt directory artifacts/orchestration/<UnitId>/<TaskId>/<Attempt>/<RunId>/ '
                      'named with its registered RunId')
    rec, why = DV.strict_object(art['REBASE'][0])
    if rec is None:
        return None, 'rule 2 (c): the rebase record is not a single JSON object (%s)' % why
    if rec.get('Schema') != 'rackcad-relay-record/v2' or rec.get('Phase') != 'REBASE':
        return None, 'rule 2 (c): the rebase record is not a relay-record/v2 with Phase REBASE'
    if rec.get('RunId') != rid:
        return None, 'rule 2 (c): the RunId of the rebase record is not the registered one (another run)'
    if rid in (inv.get('RunId'), D.get('RunId'), H.get('RunId')):
        return None, 'rule 2 (c): the RunId of the rebase record is that of the reverification, the delegation or the delivery (16.6: its own RunId)'
    if rec.get('TaskId') != inv.get('TaskId') or rec.get('Attempt') != inv.get('Attempt'):
        return None, 'rule 2 (c): the rebase record belongs to another task or attempt'
    m = rec.get('RebaseMap')
    if not isinstance(m, dict):
        return None, 'rule 2 (c): the rebase record has no RebaseMap'
    if m.get('ConflictPaths') != [] or m.get('Unmapped') != []:
        return None, 'rule 2 (c): the RebaseMap records conflict paths or unmapped entries (no 16.7 reverification without conflicts)'
    commits = m.get('Commits') if isinstance(m.get('Commits'), list) else None
    if not commits or any(not isinstance(c, dict) or not (isinstance(c.get('ImageSha'), str) and HEX40.match(c['ImageSha'])) for c in commits):
        return None, 'rule 2 (c): the RebaseMap has no commits or a commit without image'
    if m.get('MainBeforeSha') != K.get('MainSha'):
        return None, 'rule 2 (c): MainBeforeSha of the RebaseMap is not K.MainSha (another rebase)'
    if m.get('AuthorityRevision') != ar or K.get('AuthorityRevision') not in (None, ar) or m.get('UnitDocBlobsEqual') is not True \\
            or m.get('UnitChangeSectionsEqual') is not True:
        return None, 'rule 2 (c): the AuthorityRevision of the RebaseMap is not the contract\\'s or its 16.7 equalities are not recorded'
    if m.get('BaseSha') != D.get('BaseSha'):
        return None, 'rule 2 (c): BaseSha of the RebaseMap is not the BaseSha of the delegation'
    mine = [c for c in commits if c.get('OriginalSha') == H.get('CurrentSha')]
    T = inv.get('Target') or {}
    if len(mine) != 1 or mine[0].get('ImageSha') != T.get('Commit') or mine[0].get('PatchIdEqual') is not True:
        return None, ('rule 2 (c): the RebaseMap does not map the CurrentSha of the delivery to Target.commit with equal patch-id '
                      '(a rebase of another delivery or run)')
    mse = m.get('MainAfterSha')
    if not MEV.is_commit(repo, mse):
        return None, 'rule 2 (c): MainAfterSha of the RebaseMap is not a commit of the -C'
    return mse, None'''))
B.append(('''    rep = {'Rule': 'A7-1 (I-62 A-7, PROPUESTA ronda 2)', ''', '''    rep = {'Rule': 'A7-1 (I-62 A-7, PROPUESTA ronda 3)', '''))
B.append(('''        fail('DESIGNATION', 'Invocation.Kind %r is neither VERIFY nor MEASUREMENT' % (kind,))''',
          '''        fail('DESIGNATION', 'Invocation.Kind %r is not VERIFY, REVERIFICATION or MEASUREMENT' % (kind,))'''))
B.append(('''        art.pop('RELAY', None)
    for r in ('CONTRACT', 'DELEG'):''',
          '''        art.pop('RELAY', None)
    if kind != 'REVERIFICATION' and 'REBASE' in ro:
        # A7-1 round 3, points 1 (c) and 5 (c): only the 16.7 reverification has a rebase record
        rep['Absences'] = [a for a in rep['Absences'] if a['Role'] != 'REBASE']
        rep['Absences'].append({'Category': 'c', 'Role': 'REBASE', 'Path': ro['REBASE']['Path'],
                                'Cause': 'NOT_ADMITTED: only the 16.7 reverification has a rebase record (A7-1, point 1 (c)); a %s invocation has none' % kind,
                                'Substituted': False})
        art.pop('REBASE', None)
    for r in ('CONTRACT', 'DELEG'):'''))
B.append(('''    mse = des.get('MainShaEval') or K.get('MainSha')
    if not MEV.is_commit(repo, mse):
        fail('MAINSHA_EVAL', 'MainSha_eval (K.MainSha) is not a commit of the -C')
    if 'DELEG' not in art:''',
          '''    mse = K.get('MainSha')
    if kind != 'REVERIFICATION' and not MEV.is_commit(repo, mse):
        fail('MAINSHA_EVAL', 'MainSha_eval (K.MainSha) is not a commit of the -C')
    if 'DELEG' not in art:'''))
B.append(('''    base = D['BaseSha']
    red, red_basis = red_required(''',
          '''    base = D['BaseSha']
    if kind == 'REVERIFICATION':
        Hrb = DV.strict_object(art['H'][0])[0] if 'H' in art else None
        mse, why = rebase_main_sha_eval(repo, ro, art, inv, K, D, Hrb, ar)
        if mse is None:
            fail('NO_CLOSURE', why + '; nothing substitutes it (no other record, no other run, not K.MainSha)')
        rep['MainShaEvalSource'] = 'MainAfterSha of the RebaseMap of the rebase record %s (16.13: 16.7 reverification)' % ro['REBASE']['RunId']
    else:
        rep['MainShaEvalSource'] = 'K.MainSha (16.13)'
    red, red_basis = red_required('''))
B.append(('''    for r in ('DELEG', 'H', 'TRX_CURRENT', 'TRX_RED', 'RELAY'):
        if r in art:
            add_canon(ro[r]['Path'], art[r][1], 'c', r, 'custody')''',
          '''    for r in ('DELEG', 'H', 'TRX_CURRENT', 'TRX_RED', 'RELAY', 'REBASE'):
        if r in art:
            add_canon(ro[r]['Path'], art[r][1], 'c', r, 'custody')'''))
patch('make_eic_a7.py', B)

# ------------------------------------------------------------------ validator (independent of the builder)
V = []
V.append(('''# check_a7_closure.py (round 2) - validator of an effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY
# invocation (or of the A6-7 measurement) against the CANDIDATE rule A7-1 of I-62 A-7 round 2 (§3.2 and the table of §3.3),''',
          '''# check_a7_closure.py (round 3) - validator of an effective input closure E (rackcad-input-closure/v1) of an FX-02 VERIFY
# invocation (the verification, its 16.7 reverification, a negative control) or of the A6-7 measurement against the CANDIDATE rule
# A7-1 of I-62 A-7 round 3 (§3.2 and the table of §3.3). Round 3 (A62-A7R2-R1): a seventh role REBASE, only for Invocation.Kind
# REVERIFICATION (in any other invocation it is not applicable and its entry is FOREIGN): the rebase record (relay-record/v2, Phase
# REBASE) is mandatory there, and it must be coherent: '''
          + RB_DOC.replace('\n', '\n# ') + '''
# Otherwise no closure may exist. MainSha_eval of every other invocation is K.MainSha; a MainShaEval field in the designation is a problem.
# Round 2 text follows (with a7-designation/3 for a7-designation/2):'''))
V.append(('''DES_SCHEMA = 'a7-designation/2'
ROLES2 = ('CONTRACT', 'DELEG', 'H', 'TRX_CURRENT', 'TRX_RED', 'RELAY')''',
          '''DES_SCHEMA = 'a7-designation/3'
ROLES2 = ('CONTRACT', 'DELEG', 'H', 'TRX_CURRENT', 'TRX_RED', 'RELAY', 'REBASE')
RUN_ID = re.compile(r'R[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}\\Z')


def rebase_coherence(repo, roles, rb, inv, K, D, H, ar):
    """A7-1 round 3, point 2 (c), written from the A-7 text: the problems of the rebase record of the reverification ([] = coherent)."""
    i = roles['REBASE']
    rid = i.get('RunId')
    out = []
    m = re.fullmatch(r'artifacts/orchestration/([^/]+)/([^/]+)/([0-9]+)/([^/]+)/[^/]+', i.get('Path') or '')
    if not (m and m.group(1) == inv.get('UnitId') and m.group(2) == inv.get('TaskId') and m.group(3) == str(inv.get('Attempt')) and m.group(4) == rid):
        out.append('path not in the attempt directory, in a directory named with its registered RunId')
    rec = json_object(rb)
    if rec is None:
        return out + ['not a single JSON object']
    if rec.get('Schema') != 'rackcad-relay-record/v2' or rec.get('Phase') != 'REBASE':
        out.append('not a relay-record/v2 with Phase REBASE')
    if rec.get('RunId') != rid:
        out.append('RunId is not the registered one')
    if rid in (inv.get('RunId'), (D or {}).get('RunId'), (H or {}).get('RunId')):
        out.append('RunId is that of the reverification, the delegation or the delivery')
    if rec.get('TaskId') != inv.get('TaskId') or rec.get('Attempt') != inv.get('Attempt'):
        out.append('another task or attempt')
    mp = rec.get('RebaseMap') if isinstance(rec.get('RebaseMap'), dict) else None
    if mp is None:
        return out + ['no RebaseMap']
    if mp.get('ConflictPaths') != [] or mp.get('Unmapped') != []:
        out.append('conflict paths or unmapped entries')
    cs = mp.get('Commits') if isinstance(mp.get('Commits'), list) else []
    if not cs or not all(isinstance(c, dict) and isinstance(c.get('ImageSha'), str) and HEX40.match(c['ImageSha']) for c in cs):
        out.append('no commits, or a commit without image')
    if K is None or mp.get('MainBeforeSha') != K.get('MainSha'):
        out.append('MainBeforeSha is not K.MainSha')
    if mp.get('AuthorityRevision') != ar or mp.get('UnitDocBlobsEqual') is not True or mp.get('UnitChangeSectionsEqual') is not True:
        out.append('AuthorityRevision is not the contract\\'s or the 16.7 equalities are not recorded')
    if D is None or mp.get('BaseSha') != D.get('BaseSha'):
        out.append('BaseSha is not the delegation\\'s')
    if H is None:
        out.append('no delivery to check the commit mapping')
    else:
        mine = [c for c in cs if isinstance(c, dict) and c.get('OriginalSha') == H.get('CurrentSha')]
        if len(mine) != 1 or mine[0].get('ImageSha') != (inv.get('Target') or {}).get('Commit') or mine[0].get('PatchIdEqual') is not True:
            out.append('the CurrentSha of the delivery is not mapped to Target.commit with equal patch-id')
    if not repo.is_commit(mp.get('MainAfterSha')):
        out.append('MainAfterSha is not a commit')
    return out'''))
V.append(('''            prob.append('DESIGNATION: artifact with a role outside the six of rule 1 (c): %r' % (r,))
            continue''',
          '''            prob.append('DESIGNATION: artifact with a role outside the seven of rule 1 (c): %r' % (r,))
            continue
        if not legacy and ((r == 'REBASE') != ('RunId' in item) or (r == 'REBASE' and not (isinstance(item.get('RunId'), str) and RUN_ID.match(item['RunId'])))):
            prob.append('DESIGNATION: the REBASE role needs its RunId (R<yyyyMMddTHHmmssZ>-<4 hex>) and no other role has one')
            continue'''))
V.append(('''    for r in ('CONTRACT', 'DELEG'):
        if r not in roles:
            nc.append('rule 5 (c): role %s not designated' % r)
    if prob:
        return v''',
          '''    for r in ('CONTRACT', 'DELEG'):
        if r not in roles:
            nc.append('rule 5 (c): role %s not designated' % r)
    if 'MainShaEval' in des:
        prob.append('DESIGNATION: MainShaEval is designated (MainSha_eval is derived, never designated)')
    if prob:
        return v'''))
V.append(('''    if kind not in ('VERIFY', 'MEASUREMENT'):
        prob.append('DESIGNATION: Invocation.Kind %r is neither VERIFY nor MEASUREMENT' % (kind,))''',
          '''    if kind not in ('VERIFY', 'REVERIFICATION', 'MEASUREMENT'):
        prob.append('DESIGNATION: Invocation.Kind %r is not VERIFY, REVERIFICATION or MEASUREMENT' % (kind,))'''))
V.append(('''        na.append('RELAY')
        v['Absences'] = [a for a in v['Absences'] if a['Role'] != 'RELAY']
    v['NotApplicableRoles'] = na''',
          '''        na.append('RELAY')
        v['Absences'] = [a for a in v['Absences'] if a['Role'] != 'RELAY']
    if 'REBASE' in roles and kind != 'REVERIFICATION':
        # rule 1 (c) round 3: only the 16.7 reverification has a rebase record
        na.append('REBASE')
        v['Absences'] = [a for a in v['Absences'] if a['Role'] != 'REBASE']
    v['NotApplicableRoles'] = na'''))
V.append(('''    mse = des.get('MainShaEval') or (K.get('MainSha') if K else None)
    eff = eff1 = None''',
          '''    mse = K.get('MainSha') if K else None
    v['MainShaEvalSource'] = 'K.MainSha'
    if kind == 'REVERIFICATION':
        if 'REBASE' not in roles:
            nc.append('rule 5 (c): the reverification has no designated rebase record (MainSha_eval cannot be fixed)')
            mse = None
        elif 'REBASE' not in cbytes:
            nc.append('rule 5 (c): the rebase record is absent, unregistered, with another blob or unreadable')
            mse = None
        else:
            rp = rebase_coherence(repo, roles, cbytes['REBASE'], inv, K, D, H, ar)
            v['RebaseCoherence'] = rp
            if rp:
                nc.append('rule 2 (c) / 5 (c): the rebase record is not coherent (%s)' % '; '.join(rp))
                mse = None
            else:
                mse = json_object(cbytes['REBASE'])['RebaseMap']['MainAfterSha']
                v['MainShaEvalSource'] = 'MainAfterSha of the rebase record %s' % roles['REBASE']['RunId']
    eff = eff1 = None'''))
V.append(('''    if K is not None:
        if not repo.is_commit(mse):''', '''    if K is not None and mse is not None:
        if not repo.is_commit(mse):'''))
patch('check_a7_closure.py', V)

# ------------------------------------------------------------------ designation check of the derivation vector
G = []
G.append(('''SCHEMA = 'a7-designation/2'
ROLES = ('CONTRACT', 'DELEG', 'H', 'TRX_CURRENT', 'TRX_RED', 'RELAY')''',
          '''SCHEMA = 'a7-designation/3'   # round 3 (I-62 A-7, decisiones §96): REBASE (the rebase record of the 16.7 reverification), no Name in C6
ROLES = ('CONTRACT', 'DELEG', 'H', 'TRX_CURRENT', 'TRX_RED', 'RELAY', 'REBASE')'''))
G.append(('''        return dict(res, Ok=False, Problems=[{'Code': 'DESIGNATION_SCHEMA', 'Detail': 'not a7-designation/2'}], Vector=None)''',
          '''        return dict(res, Ok=False, Problems=[{'Code': 'DESIGNATION_SCHEMA', 'Detail': 'not a7-designation/3'}], Vector=None)'''))
G.append(('''            probs.append({'Code': 'ROLE', 'Detail': 'role outside the six or repeated: %r' % (r,)})''',
          '''            probs.append({'Code': 'ROLE', 'Detail': 'role outside the seven or repeated: %r' % (r,)})'''))
G.append(('''    if 'RELAY' in ro and name_of(ro['RELAY']['Path'], kp, d_run, h_run) is not None:
        probs.append({'Code': 'RELAY_NAMED', 'Detail': 'the relay facts path gets a Name in C6'})''',
          '''    if 'RELAY' in ro and name_of(ro['RELAY']['Path'], kp, d_run, h_run) is not None:
        probs.append({'Code': 'RELAY_NAMED', 'Detail': 'the relay facts path gets a Name in C6'})
    if 'REBASE' in ro:
        if name_of(ro['REBASE']['Path'], kp, d_run, h_run) is not None:
            probs.append({'Code': 'REBASE_NAMED', 'Detail': 'the rebase record path gets a Name in C6'})
        if inv.get('Kind', 'VERIFY') != 'REVERIFICATION' and pinned.get('REBASE') is not None:
            probs.append({'Code': 'REBASE_NOT_ADMITTED', 'Detail': 'E pins a rebase record in an invocation that is not the 16.7 reverification'})
        m = re.fullmatch(r'artifacts/orchestration/([^/]+)/([^/]+)/([0-9]+)/([^/]+)/[^/]+', ro['REBASE']['Path'])
        if not m or m.group(1) != inv.get('UnitId') or m.group(2) != inv.get('TaskId') or m.group(3) != str(inv.get('Attempt')) \\
                or m.group(4) != ro['REBASE'].get('RunId'):
            probs.append({'Code': 'KPR-4', 'Detail': 'REBASE path is not in the attempt directory under its registered RunId'})
    elif inv.get('Kind') == 'REVERIFICATION':
        probs.append({'Code': 'ROLE_MISSING', 'Detail': 'REBASE (16.7 reverification)'})'''))
patch('designate_a7.py', G)
print('done')

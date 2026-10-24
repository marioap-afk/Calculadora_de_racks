# r4phases.py - the new phases of the round-4 preflight of I-62 A-7 (decisiones §98, puntos 3-5), used by run_a7r4.py:
#   q9   the eight negative controls with invocation of V14 D.3 on the inputs of the 16.7 reverification RV of q8 (RAE §10; A62-A7R3-R1):
#        positive closure of each, the comparison of its inputs with RV's (the static counterpart of the relative oracle), the substitution
#        and manipulation of the copy of the rebase record, of the identity of RV, of the link and of the copies, the mutation absent, extra
#        or in another role, the format, the misuse of a control of V with a rebase record, and the acceptor (A-6 unchanged);
#   q10  the other modalities of the coverage matrix: with and without the instrument, the ordinary nc3, a transport re-run of a control,
#        A62-A7R3-O1 (C1 of the acceptor-closure derivation, with the pinned instrument and acceptor) and A62-A7R3-O2 (a real second rebase);
#   modalities.md / modalities.json: the matrix of decisiones §98, punto 3, with the cases that exercise each modality.
# Stdlib only. Never runs Codex, claude or any model CLI; no network; read-only on the I-62 worktree; writes next to this file and in the work
# area (D:/r62-a7/work/r4).
import json, os, shutil, subprocess

M = None   # the runner module (run_a7r4.py), set by init()


def init(runner):
    global M
    M = runner


CONTROLS = ('nc1', 'nc2', 'nc3', 'n4', 'n5', 'n6', 'n7a', 'n7b')
CTRL_RUN = dict((c, 'R20261024T1100%02dZ-0e%02d' % (i, i)) for i, c in enumerate(CONTROLS, 1))
RERUN = 'R20261024T113000Z-0e21'
RB2_RUN, RV2_RUN = 'R20261024T120000Z-0f01', 'R20261024T121000Z-0f02'
OTHER_RUN = 'R20261011T080000Z-d000'                      # «otra corrida» of N7a and N7b in the A6-7 bench (evidence §181)
N7B_SHA = '13654f96be6dd1effc167d883a8e18fa718dd8fe'      # N7b of the bench: the head of origin/fx/u1-fx05
NC1_SHA = 'deadbeef' * 5                                  # nc1 of the bench: a CurrentSha that is not a commit
RED_SHA = '53436cb20adf1fe87dc91b0382dd3123a1fac44f'
OTHER_MAIN = '930288c51ab1b5dbd370c8ebf9206d78670b89a0'   # a commit of main from which the image does not descend (as in q8)
BENCH_WORK = 'D:/r62-a6m7/bench/work'
TITLE = {'nc1': 'nc1 (Identity)', 'nc2': 'nc2 (Scope)', 'nc3': 'nc3 (FreeText)', 'n4': 'N4 (handoff ausente)', 'n5': 'N5 (0 pruebas)',
         'n6': 'N6 (trabajo sin commit)', 'n7a': 'N7a (otra corrida, Identity pass)', 'n7b': 'N7b (otra corrida, Identity fail)'}


def mut_values(c, H, D):
    """The fields that control c mutates, with the values of the A6-7 bench (evidence §181): nc1 CurrentSha inexistente; nc2 AllowedWriteScope
    without the first file of the diff (its prefix entry replaced by the other files of the diff, RAE §10); nc3 a gate term in WorkCompleted; N5
    0 tests; N7a another run; N7b another run and its CurrentSha. N4 has no copy of the delivery and N6 changes no input."""
    return {'nc1': {'CurrentSha': NC1_SHA}, 'nc2': {'AllowedWriteScope': [p for p in D['AllowedWriteScope'] if not p.endswith('/**')]},
            'nc3': {'WorkCompleted': H['WorkCompleted'] + ' GATE PASS'},
            'n5': {'TestResults': [dict(t, Selected=0, Passed=0) for t in H['TestResults']]},
            'n7a': {'DelegationRunId': OTHER_RUN}, 'n7b': {'DelegationRunId': OTHER_RUN, 'CurrentSha': N7B_SHA}}.get(c, {})


def j(b):
    return json.loads(b.decode('utf-8-sig'))


def art(d, r):
    return next(a for a in d['Artifacts'] if a['Role'] == r)


def rvc_designations(C, rvp, out):
    """The eight controls of D.3 on the inputs of RV (RAE §10): copies of its inputs (the delegation, the delivery and the rebase record in
    artifacts/orchestration/FX-U1/T1-<control>/0/<RunId of their bytes>/; the contract and the TRX at the same paths), the mutation of each
    control, its own relay facts and RunId, the Target of RV and Source = RV."""
    rv, fx = C['rv'], C['fx']
    ro = dict((a['Role'], a) for a in rv['Artifacts'])
    src = dict((r, M.rb(ro[r]['Custody'])) for r in ('DELEG', 'H', 'REBASE'))
    D, H = j(src['DELEG']), j(src['H'])
    rvb = M.rb(rvp)
    shutil.rmtree(out, ignore_errors=True)
    des = {}
    for c in CONTROLS:
        mrole, mfields = M.R2.A7B.MUTATIONS[c]
        run = CTRL_RUN[c]
        d = json.loads(json.dumps(rv))
        d['Case'] = 'RVC-' + c.upper()
        d['Invocation'].update({'RunId': run, 'Kind': 'REVERIFICATION_CONTROL'})
        d['RunPath'] = 'artifacts/orchestration/FX-U1/T1/0/%s/' % run
        d['Source'] = {'Control': c, 'RunId': M.RV_RUN, 'Designation': M.fwd(rvp), 'DesignationSha256': M.sha256(rvb)}
        arts = []
        for a in d['Artifacts']:
            r = a['Role']
            if r in ('DELEG', 'H', 'REBASE'):
                b = src[r]
                if r == mrole and mfields:
                    o = j(b)
                    o.update(mut_values(c, H, D))
                    b = M.R1M.jbytes(o)
                name = a['Path'].split('/')[-1]
                dirn = M.RB_RUN if r == 'REBASE' else j(b)['RunId']
                path = 'artifacts/orchestration/FX-U1/T1-%s/0/%s/%s' % (c, dirn, name)
                if r == mrole and mfields is None:
                    a = {'Role': r, 'Path': path, 'Blob': 'f' * 40, 'Custody': M.fwd(os.path.join(out, c, 'absent-' + name))}
                else:
                    cp = M.wb(os.path.join(out, c, '%s-%s' % (r.lower(), name)), b)
                    a = dict(a, Path=path, Blob=M.blob_id(b), Custody=M.fwd(cp))
            elif r == 'RELAY':
                rel = M.R2.relay_facts('RVC-' + c, run, fx['BranchAfter'], RED_SHA, fx['MainAfter'], 'controller-output.json', M.sha256(M.rb(ro['TRX_RED']['Custody'])))
                relp = M.wb(os.path.join(out, c, 'relay-facts.json'), M.R1M.jbytes(rel))
                a = {'Role': 'RELAY', 'Path': 'artifacts/orchestration/FX-U1/T1/0/%s/relay-facts.json' % run, 'Blob': M.blob_id(M.rb(relp)), 'Custody': M.fwd(relp)}
            arts.append(a)
        d['Artifacts'] = arts
        des[c] = d
        M.jdump(os.path.join(M.HERE, 'designations-r4', 'RVC-%s.json' % c.upper()), d)
    return des, D, H


def rel_inputs(c, rc, rr):
    """Static counterpart of the relative oracle (RAE §10): the inputs of the control against RV's. (a), (b) and (d) and the revisions
    identical; in (c) every role a copy (same blob; the delegation, the delivery and the rebase record in the control path), except RELAY (its
    own record) and the mutated role (another blob; N4: absent)."""
    mrole, mfields = M.R2.A7B.MUTATIONS[c]
    ab = lambda rep: sorted((e['List'], e['Path'], e['Blob']) for e in rep['Entries'] if e['Category'] in ('a', 'b', 'd'))
    cc = lambda rep: dict((e['Kind'], (e['Path'], e['Blob'])) for e in rep['Entries'] if e['Category'] == 'c')
    probs = []
    if ab(rc) != ab(rr):
        probs.append('(a)/(b)/(d) differ')
    for k in ('MainShaEval', 'EFF', 'EFF^1', 'TargetCommit', 'BaseSha'):
        if rc['Revisions'].get(k) != rr['Revisions'].get(k):
            probs.append('revision %s differs' % k)
    xc, xr = cc(rc), cc(rr)
    seen = []
    for role in sorted(set(xc) | set(xr)):
        if role == 'RELAY':
            ok = role in xc and xc[role] != xr.get(role)
            seen.append('RELAY propio' if ok else 'RELAY ajeno')
        elif role == mrole:
            ok = (role not in xc) if mfields is None else (role in xc and role in xr and xc[role][1] != xr[role][1])
            seen.append('%s %s' % (role, 'ausente' if mfields is None else 'mutado') if ok else '%s sin la mutación' % role)
        else:
            ok = role in xc and role in xr and xc[role][1] == xr[role][1] and (
                (xc[role][0] != xr[role][0] and '/T1-%s/' % c in xc[role][0]) if role in ('DELEG', 'H', 'REBASE') else xc[role][0] == xr[role][0])
            seen.append('%s copia' % role if ok else '%s no es copia' % role)
        if not ok:
            probs.append(seen[-1])
    return probs, seen


def q9():
    case, build, CHK, DSA, MEV, R2, R1M = M.case, M.build, M.CHK, M.DSA, M.MEV, M.R2, M.R1M
    rb, wb, fwd, sha256, blob_id, W, BASE = M.rb, M.wb, M.fwd, M.sha256, M.blob_id, M.W, M.BASE
    C = M.Q8CTX
    out = R2.out_dir('q9')
    rvp = os.path.join(M.HERE, 'designations-r4', 'RV.json')
    des, D, H = rvc_designations(C, rvp, os.path.join(M.HERE, 'inputs-rvc'))
    fx, rv = C['fx'], C['rv']
    cases = []
    # ---- the mutation values are those of the A6-7 bench
    bench = {}
    for c, f, keys in (('nc2', 'deleg.json', ('AllowedWriteScope',)), ('nc3', 'handoff.json', ('WorkCompleted',)), ('n5', 'handoff.json', ('TestResults',)),
                       ('n7a', 'handoff.json', ('DelegationRunId',)), ('n7b', 'handoff.json', ('DelegationRunId', 'CurrentSha'))):
        p = '%s/in-%s/%s' % (BENCH_WORK, c, f)
        o = j(rb(p)) if os.path.isfile(p) else {}
        mv = mut_values(c, H, D)
        bench[c] = all(o.get(k) == mv.get(k) for k in keys)
    hn1 = j(rb('%s/in-nc1/handoff.json' % BENCH_WORK)) if os.path.isfile('%s/in-nc1/handoff.json' % BENCH_WORK) else {}
    bench['nc1'] = hn1.get('CurrentSha') == NC1_SHA
    cases.append(case('RVC-MUTVALUES', 'valores de las mutaciones de los controles (RAE §10 para nc1-nc3; V14 D.3 y G.2 para N4-N7b, que solo nombran el efecto)',
                      'los del banco de A6-7 (evidencia §181), campo a campo', ', '.join('%s %s' % (c, 'igual' if v else 'distinto') for c, v in sorted(bench.items())),
                      all(bench.values()) and len(bench) == 6))
    dschema = json.loads(rb(os.path.join(M.HERE, 'a7-designation-4.schema.json')).decode('utf-8'))
    sv = dict((c, R2.schema_ok(des[c], dschema)) for c in CONTROLS)
    bad = json.loads(json.dumps(des['nc1']))
    bad['Source']['Control'] = 'nc9'
    cases.append(case('RVC-SCHEMA', 'designaciones a7-designation/4 de los ocho controles sobre RV (Kind REVERIFICATION_CONTROL, Source)',
                      'las ocho válidas contra el esquema; Source.Control fuera de la lista, inválido', 'violaciones: %s; nc9: %s' % (
                          ', '.join('%s %s' % (c, v) for c, v in sv.items() if v) or 'ninguna', R2.schema_ok(bad, dschema) or 'válida'),
                      not any(sv.values()) and R2.schema_ok(bad, dschema)))
    # ---- positive: the closure of each control, and its inputs against RV's
    pos = {}
    for c in CONTROLS:
        d = des[c]
        E, rep, err = build(d)
        if E is None:
            cases.append(case('RVC-%s' % c.upper(), TITLE[c] + ' sobre las entradas de RV', 'cierre construido', 'constructor: %s' % err, False))
            continue
        Ep = wb(os.path.join(out, 'E-rvc-%s.json' % c), MEV.serialize(E))
        v = CHK.check(d, E, i62_repo=W, i62_commit=BASE)
        dchk = DSA.check(d, E)
        cl = R2.derive(d, Ep, out, 'rvc-' + c)
        names = [f['Name'] for f in cl['Closure']['Files']] if 'Closure' in cl else None
        rbk = [e for e in E['CanonicalInputs'] if e['Path'] == art(d, 'REBASE')['Path']]
        ok = (rep['Revisions']['MainShaEval'] == fx['MainAfter'] and len(rbk) == 1 and rbk[0]['Blob'] == art(rv, 'REBASE')['Blob'] and v['Accepted']
              and dchk['Ok'] and names is not None and 'REBASE' not in names)
        cases.append(case('RVC-%s' % c.upper(), '%s sobre las entradas de RV: copias en T1-%s, mutación del control, hechos del relevo propios, Target y Source = RV' % (TITLE[c], c),
                          'cierre construido; MainSha_eval = MainAfterSha de la copia (≠ K.MainSha); la copia del registro en CanonicalInputs con el blob del de RV; '
                          'validador acepta; designate_a7 válida; derivación del aceptante sin nombre en C6 para el registro',
                          'MainShaEval %s (%s); copia del registro: %d (%s); validador: %s; designate_a7: %s; C6: %s; omisiones: %s' % (
                              rep['Revisions']['MainShaEval'][:8], (rep.get('Source') or {}).get('ReverificationRunId'), len(rbk), rbk[0]['Blob'][:8] if rbk else '-',
                              'acepta' if v['Accepted'] else 'rechaza %s' % M.vb(v), 'válida' if dchk['Ok'] else dchk['Problems'], ', '.join(sorted(set(names or []))),
                              ', '.join('%s %s' % o for o in cl.get('Omitted', [])) or 'ninguna'), ok, Closure=cl.get('Sha256'),
                          MutatedFields=(rep.get('Source') or {}).get('MutatedFields')))
        probs, seen = rel_inputs(c, rep, C['rep'])
        cases.append(case('REL-%s' % c.upper(), 'entradas de %s frente a las de RV (contrapartida estática del oráculo relativo de RAE §10)' % TITLE[c],
                          '(a), (b), (d), MainSha_eval, EFF, EFF^1, Target y BaseSha idénticos; en (c), copias salvo los hechos del relevo (propios) y el papel mutado',
                          '%s; %s' % ('; '.join(seen), ('discrepancias: ' + ', '.join(probs)) if probs else 'sin discrepancias'), not probs))
        pos[c] = (E, rep, Ep, cl)
    E1 = pos['nc1'][0]

    def var(c, f):
        d = json.loads(json.dumps(des[c]))
        f(d)
        return d

    def no_closure(cid, what, d, why, c='nc1', want_code='NO_CLOSURE'):
        Ex, repx, errx = build(d)
        vx = CHK.check(d, pos[c][0], i62_repo=W, i62_commit=BASE)
        detail = (errx or {}).get('Detail') or ''
        ok = Ex is None and (errx or {}).get('Code') == want_code and (why is None or why in detail) and not vx['Accepted']
        cases.append(case(cid, what, 'sin cierre (%s%s) y el validador rechaza el cierre de RVC-%s con esa designación' % (want_code, ': «' + why + '»' if why else '', c.upper()),
                          'constructor: %s %s; validador: %s' % ((errx or {}).get('Code'), detail[:170], 'acepta' if vx['Accepted'] else 'rechaza (%s)' % (
                              '; '.join((vx['NoClosureExpected'] + vx['Problems'])[:2])[:190])), ok))

    def put(name, b):
        return fwd(wb(os.path.join(out, name), b))
    rec, rid = C['rec'], M.RB_RUN
    rvrb = rb(art(rv, 'REBASE')['Custody'])
    # ---- the copy of the rebase record: substitution and manipulation
    alt = rvrb + b' '
    no_closure('RVC-REBASE-BYTES', 'nc1: copia del registro con un byte más (blob registrado el de esos bytes)',
               var('nc1', lambda d: art(d, 'REBASE').update({'Custody': put('rb-byte.json', alt), 'Blob': blob_id(alt)})), 'not the copy of the reverification')
    other = R1M.jbytes(dict(rec, RebaseMap=dict(rec['RebaseMap'], MainAfterSha=OTHER_MAIN)))
    no_closure('RVC-REBASE-OTHER', 'nc1: la copia sustituida por otro registro de rebase con el mismo RunId (otro MainAfterSha), blob registrado',
               var('nc1', lambda d: art(d, 'REBASE').update({'Custody': put('rb-other.json', other), 'Blob': blob_id(other)})), 'not the copy of the reverification')
    rid2 = 'R20261023T100000Z-0b02'
    rb_rid2 = R1M.jbytes(dict(rec, RunId=rid2))
    no_closure('RVC-REBASE-RUNID', 'nc1: copia con otro RunId (registrado y en su directorio)', var('nc1', lambda d: art(d, 'REBASE').update({
        'RunId': rid2, 'Path': 'artifacts/orchestration/FX-U1/T1-nc1/0/%s/rebase-record.json' % rid2, 'Custody': put('rb-rid2.json', rb_rid2),
        'Blob': blob_id(rb_rid2)})), 'not the copy of the reverification')
    no_closure('RVC-REBASE-PATH-TASK', 'nc1: la copia en el directorio del intento de la tarea (el original de RV), no en el del control',
               var('nc1', lambda d: art(d, 'REBASE').update({'Path': 'artifacts/orchestration/FX-U1/T1/0/%s/rebase-record.json' % rid})), 'not the copy of the reverification')
    no_closure('RVC-REBASE-PATH-OTHERCTL', 'nc1: la copia en la ruta de otro control (T1-nc2)',
               var('nc1', lambda d: art(d, 'REBASE').update({'Path': 'artifacts/orchestration/FX-U1/T1-nc2/0/%s/rebase-record.json' % rid})), 'not the copy of the reverification')
    no_closure('RVC-REBASE-ABSENT', 'nc1 sin copia del registro designada', var('nc1', lambda d: d['Artifacts'].remove(art(d, 'REBASE'))), 'designates no copy of the rebase record')
    no_closure('RVC-REBASE-NOBYTES', 'nc1: copia designada sin bytes custodiados',
               var('nc1', lambda d: art(d, 'REBASE').update({'Custody': fwd(os.path.join(out, 'no-existe.json'))})), 'copy of the rebase record is absent')
    no_closure('RVC-N4-REBASE-ABSENT', 'N4 (entrega ausente) sin copia del registro', var('n4', lambda d: d['Artifacts'].remove(art(d, 'REBASE'))),
               'designates no copy of the rebase record', c='n4')
    # ---- the identity of the reverification (Source)
    no_closure('RVC-SOURCE-SHA', 'nc1: Source con otro SHA-256 de la designación de RV', var('nc1', lambda d: d['Source'].update({'DesignationSha256': '0' * 64})),
               'does not have the identified SHA-256')
    no_closure('RVC-SOURCE-ABSENT', 'nc1: la designación identificada no existe', var('nc1', lambda d: d['Source'].update({'Designation': fwd(os.path.join(out, 'no-RV.json'))})),
               'absent or unreadable')
    vp = os.path.join(M.HERE, 'designations', 'V.json')
    no_closure('RVC-SOURCE-KIND-V', 'nc1: Source = la designación de la verificación ordinaria V (Kind VERIFY), con su SHA-256',
               var('nc1', lambda d: d['Source'].update({'Designation': fwd(vp), 'DesignationSha256': sha256(rb(vp)), 'RunId': json.loads(rb(vp).decode('utf-8'))['Invocation']['RunId']})),
               'is not that of a 16.7 reverification')
    no_closure('RVC-SOURCE-RUNID', 'nc1: Source con otro RunId de reverificación', var('nc1', lambda d: d['Source'].update({'RunId': 'R20261023T101000Z-0c03'})),
               'is not that of a 16.7 reverification')
    bad_rec = R1M.jbytes(dict(rec, RebaseMap=dict(rec['RebaseMap'], MainBeforeSha=OTHER_MAIN)))
    rvbad = json.loads(json.dumps(rv))
    art(rvbad, 'REBASE').update({'Custody': put('rb-badsource.json', bad_rec), 'Blob': blob_id(bad_rec)})
    rvbadp = M.jdump(os.path.join(out, 'RV-badsource.json'), rvbad)
    no_closure('RVC-SOURCE-INCOHERENT', 'nc1 sobre una reverificación cuyo registro no es coherente (MainBeforeSha ≠ K.MainSha); la copia es la de ese registro',
               var('nc1', lambda d: (d['Source'].update({'Designation': fwd(rvbadp), 'DesignationSha256': sha256(rb(rvbadp))}),
                                     art(d, 'REBASE').update({'Custody': put('rb-badsource-copy.json', bad_rec), 'Blob': blob_id(bad_rec)}))),
               'does not satisfy point 2 (c)')
    rvnoh = json.loads(json.dumps(rv))
    rvnoh['Artifacts'].remove(art(rvnoh, 'H'))
    rvnohp = M.jdump(os.path.join(out, 'RV-noH.json'), rvnoh)
    no_closure('RVC-SOURCE-NO-H', 'nc1 sobre una reverificación sin entrega (no se puede comprobar su coherencia)',
               var('nc1', lambda d: d['Source'].update({'Designation': fwd(rvnohp), 'DesignationSha256': sha256(rb(rvnohp))})), 'has no present, blob-verified H')
    # ---- the link: task, attempt, Target, AuthorityRevision, RunId
    vdes = json.loads(rb(vp).decode('utf-8'))
    no_closure('RVC-TARGET', 'nc1 con el Target de V (el commit original), no el de RV', var('nc1', lambda d: d['Invocation'].update({'Target': vdes['Invocation']['Target']})),
               'does not have the Target of the reverification')
    no_closure('RVC-TASK', 'nc1 con otra tarea (TaskId T2)', var('nc1', lambda d: d['Invocation'].update({'TaskId': 'T2'})), 'does not have the TaskId')
    no_closure('RVC-ATTEMPT', 'nc1 con otro intento (Attempt 1)', var('nc1', lambda d: d['Invocation'].update({'Attempt': 1})), 'does not have the Attempt')
    no_closure('RVC-AR', 'nc1 con otra AuthorityRevision', var('nc1', lambda d: d.update({'AuthorityRevision': fx['MainBefore']})), 'AuthorityRevision of the reverification')
    no_closure('RVC-RUNID-SELF', 'nc1 con el RunId de RV', var('nc1', lambda d: (d['Invocation'].update({'RunId': M.RV_RUN}),
                                                                            d.update({'RunPath': 'artifacts/orchestration/FX-U1/T1/0/%s/' % M.RV_RUN}))), 'no RunId of its own')
    no_closure('RVC-RUNID-REBASE', 'nc1 con el RunId del registro de rebase', var('nc1', lambda d: (d['Invocation'].update({'RunId': rid}),
                                                                                           d.update({'RunPath': 'artifacts/orchestration/FX-U1/T1/0/%s/' % rid}))), 'no RunId of its own')
    # ---- the copies: unauthorized substitutions
    dn1 = '%s/in-nc1/deleg.json' % BENCH_WORK
    if os.path.isfile(dn1):
        no_closure('RVC-SUBST-DELEG', 'nc1: la delegación sustituida por otra (la copia del banco para nc1: otro ExpectedWorktree), en la ruta del control',
                   var('nc1', lambda d: art(d, 'DELEG').update({'Custody': put('deleg-other.json', rb(dn1)), 'Blob': blob_id(rb(dn1))})), 'is not the copy of the reverification')
    no_closure('RVC-SUBST-TRX', 'nc1: el TRX de CurrentSha de la corrida original (V), con los mismos bytes que el de la imagen pero de otra corrida',
               var('nc1', lambda d: art(d, 'TRX_CURRENT').update({'Path': art(vdes, 'TRX_CURRENT')['Path']})), 'is not the copy of the reverification')
    no_closure('RVC-SUBST-TRXRED-ABSENT', 'nc1 sin el TRX de RedSha que RV designa', var('nc1', lambda d: d['Artifacts'].remove(art(d, 'TRX_RED'))),
               'is not the copy of the reverification')
    no_closure('RVC-SUBST-H-PATH', 'nc2: la copia de la entrega en la ruta de la tarea, no en la del control',
               var('nc2', lambda d: art(d, 'H').update({'Path': art(rv, 'H')['Path']})), 'not in the control path', c='nc2')
    # ---- the mutation: absent, extra, in another role, not of the control
    hb = rb(art(rv, 'H')['Custody'])
    no_closure('RVC-MUT-NONE', 'nc1 con la entrega idéntica a la de RV (mutación no aplicada)',
               var('nc1', lambda d: art(d, 'H').update({'Custody': put('h-same.json', hb), 'Blob': blob_id(hb)})), 'does not apply its mutation')
    hx = R1M.jbytes(dict(H, CurrentSha=NC1_SHA, WorkCompleted=H['WorkCompleted'] + ' GATE PASS'))
    no_closure('RVC-MUT-EXTRA', 'nc1 con CurrentSha inexistente y, además, otro WorkCompleted (un campo fuera de su mutación)',
               var('nc1', lambda d: art(d, 'H').update({'Custody': put('h-extra.json', hx), 'Blob': blob_id(hx)})), 'differs outside its mutated fields')
    hw = R1M.jbytes(dict(H, WorkCompleted=H['WorkCompleted'] + ' GATE PASS'))
    db = rb(art(rv, 'DELEG')['Custody'])
    no_closure('RVC-MUT-ROLE', 'nc2 con la delegación de RV sin mutar y la entrega mutada (la mutación en otro papel)',
               var('nc2', lambda d: (art(d, 'DELEG').update({'Custody': put('deleg-same.json', db), 'Blob': blob_id(db)}),
                                     art(d, 'H').update({'Custody': put('h-nc3like.json', hw), 'Blob': blob_id(hw)}))), 'is not the copy of the reverification', c='nc2')
    he = R1M.jbytes(dict(H, CurrentSha=fx['MainBefore']))
    no_closure('RVC-NC1-EXISTS', 'nc1 con un CurrentSha que sí es un commit del -C (K.MainSha)',
               var('nc1', lambda d: art(d, 'H').update({'Custody': put('h-exists.json', he), 'Blob': blob_id(he)})), 'is a commit of the -C')
    no_closure('RVC-N4-PRESENT', 'N4 con la entrega presente (copia de la de RV)',
               var('n4', lambda d: art(d, 'H').update({'Custody': put('h-n4present.json', hb), 'Blob': blob_id(hb)})), 'leaves the delivery absent', c='n4')
    no_closure('RVC-N6-MUTATED', 'N6 (sin mutación en las entradas) con la entrega mutada',
               var('n6', lambda d: art(d, 'H').update({'Custody': put('h-n6mut.json', hw), 'Blob': blob_id(hw)})), 'is not the copy of the reverification', c='n6')
    hc = R1M.jbytes(dict(H, DelegationRunId=OTHER_RUN, CurrentSha=N7B_SHA))
    no_closure('RVC-N7A-CURRENT', 'N7a (Identity pass) con otro CurrentSha además de otra corrida',
               var('n7a', lambda d: art(d, 'H').update({'Custody': put('h-n7acur.json', hc), 'Blob': blob_id(hc)})), 'differs outside its mutated fields', c='n7a')
    hr = R1M.jbytes(dict(H, CurrentSha=N7B_SHA))
    no_closure('RVC-N7B-NORUN', 'N7b con otro CurrentSha pero de la misma corrida', var('n7b', lambda d: art(d, 'H').update({'Custody': put('h-n7bnorun.json', hr), 'Blob': blob_id(hr)})),
               'is not of another run', c='n7b')
    # ---- format
    no_closure('RVC-NO-SOURCE', 'designación REVERIFICATION_CONTROL sin Source', var('nc1', lambda d: d.pop('Source')), 'Source is designated only', want_code='DESIGNATION')
    vsrc = json.loads(json.dumps(vdes))
    vsrc['Source'] = des['nc1']['Source']
    Ev, repv, errv = build(vsrc)
    cases.append(case('RVC-SOURCE-ON-VERIFY', 'designación VERIFY (V) con un Source', 'sin cierre (DESIGNATION)', 'constructor: %s' % ((errv or {}).get('Code'),),
                      Ev is None and errv['Code'] == 'DESIGNATION'))
    no_closure('RVC-MSE-DESIGNATED', 'nc1 con MainShaEval designado', var('nc1', lambda d: d.update({'MainShaEval': fx['MainAfter']})), None, want_code='DESIGNATION_FORMAT')
    no_closure('RVC-SOURCE-SHAPE', 'nc1 con Source.Control fuera de la lista (nc9)', var('nc1', lambda d: d['Source'].update({'Control': 'nc9'})), 'does not identify its reverification')
    no_closure('RVC-EXTRA-ROLE', 'nc1 con dos registros de rebase designados (la copia y el original de RV)',
               var('nc1', lambda d: d['Artifacts'].append(dict(art(rv, 'REBASE')))), None, want_code='DESIGNATION')
    # ---- injected entries
    Ex = json.loads(json.dumps(E1))
    Ex['CanonicalInputs'].append({'Path': art(rv, 'REBASE')['Path'], 'Blob': art(rv, 'REBASE')['Blob']})
    vx, dx = CHK.check(des['nc1'], Ex, i62_repo=W, i62_commit=BASE), DSA.check(des['nc1'], Ex)
    cases.append(case('RVC-EXTRA-RECORD', 'cierre de nc1 sobre RV con el registro original de RV (ruta de la tarea) añadido a mano',
                      'validador: ajeno; designate_a7: UNDESIGNATED_INPUT', 'validador %s (ajenas %d); designate_a7 %s' % (
                          'acepta' if vx['Accepted'] else 'rechaza', len(vx['Foreign']), [p['Code'] for p in dx['Problems']]),
                      not vx['Accepted'] and vx['Foreign'] and any(p['Code'] == 'UNDESIGNATED_INPUT' for p in dx['Problems'])))
    # ---- misuse: a control of V (Kind VERIFY) with a designated rebase record, and a control of V presented as a control of RV
    n1 = json.loads(rb(os.path.join(M.HERE, 'designations', 'NC1.json')).decode('utf-8'))
    Eb, repb, errb = build(n1)
    n1r = json.loads(json.dumps(n1))
    n1r['Artifacts'].append(dict(art(rv, 'REBASE'), Path='artifacts/orchestration/FX-U1/T1-nc1/0/%s/rebase-record.json' % rid))
    Em, repm, errm = build(n1r)
    same = Em is not None and Eb is not None and MEV.serialize(Em) == MEV.serialize(Eb)
    ab = [a for a in (repm or {}).get('Absences', []) if a['Role'] == 'REBASE']
    Einj = json.loads(json.dumps(Em)) if Em else {'CanonicalInputs': []}
    Einj['CanonicalInputs'].append({'Path': n1r['Artifacts'][-1]['Path'], 'Blob': n1r['Artifacts'][-1]['Blob']})
    vinj = CHK.check(n1r, Einj, i62_repo=W, i62_commit=BASE) if Em else {'Accepted': True, 'Foreign': []}
    dinj = DSA.check(n1r, Einj) if Em else {'Problems': []}
    cases.append(case('RVC-MISUSE-V-NC1', 'nc1 de la verificación ordinaria V (Kind VERIFY) con un registro de rebase designado (uso indebido)',
                      'no se admite: E idéntico al de nc1 sin él, ausencia NOT_ADMITTED, MainSha_eval = K.MainSha; inyectado a mano, ajeno para el validador y '
                      'REBASE_NOT_ADMITTED para designate_a7', 'E igual: %s; ausencia: %s; MainSha_eval %s; inyectado: %s (ajenas %d), designate_a7 %s' % (
                          same, ab[0]['Cause'][:70] if ab else '-', ((repm or {}).get('Revisions') or {}).get('MainShaEval', '-')[:8],
                          'acepta' if vinj['Accepted'] else 'rechaza', len(vinj.get('Foreign', [])), [p['Code'] for p in dinj['Problems']]),
                      same and bool(ab) and not vinj['Accepted'] and bool(vinj.get('Foreign')) and any(p['Code'] == 'REBASE_NOT_ADMITTED' for p in dinj['Problems'])))
    n1c = json.loads(json.dumps(n1))
    n1c['Invocation']['Kind'] = 'REVERIFICATION_CONTROL'
    n1c['Source'] = des['nc1']['Source']
    n1c['Artifacts'].append(dict(art(des['nc1'], 'REBASE')))
    Ec, repc, errc = build(n1c)
    cases.append(case('RVC-MISUSE-V-AS-RVC', 'nc1 de V (copias de V, Target de V) presentado como control de RV (Kind REVERIFICATION_CONTROL, Source = RV, copia del registro)',
                      'sin cierre: no tiene el Target de RV ni sus copias', 'constructor: %s %s' % ((errc or {}).get('Code'), ((errc or {}).get('Detail') or '')[:120]),
                      Ec is None and (errc or {}).get('Code') == 'NO_CLOSURE'))
    # ---- acceptor (A-6 unchanged) on a control on the inputs of RV
    T8 = C['T8']
    ro2 = dict((a['Role'], a) for a in des['nc2']['Artifacts'])
    runs = []
    for name, a in (('trx-current', ro2['TRX_CURRENT']), ('trx-red', ro2['TRX_RED'])):
        r, rcx, env = R2.instr_run(T8, out, 'rvc-nc2-' + name, ['trx', '--repo', fwd(T8), '--file', 'TRX=' + a['Custody'], '--sha256', 'TRX=' + sha256(rb(a['Custody'])),
                                                               '--filter-substring', 'Subtract'])
        runs.append(r)
    acc = R2.accept_req(R2.minimal_request(T8, runs, pos['nc2'][3], (ro2['RELAY']['Custody'], sha256(rb(ro2['RELAY']['Custody'])))), out, 'rvc-nc2', [T8])
    cases.append(case('RVC-ACCEPTOR', 'aceptante fijado (A-6 sin cambio) sobre las dos corridas trx con el cierre derivado de nc2 sobre RV',
                      'ACCEPT_FACTS: R9 MATCH (C6 fija los dos TRX), R10 MATCH (hechos del relevo propios del control); la copia del registro no entra en C6',
                      '%s rc %d; R9 %s; R10 %s; cero escrituras: %s' % (acc['Decision'], acc['Rc'], acc['Checks'].get('R9'), acc['Checks'].get('R10'), acc['ZeroWrite']),
                      acc['Decision'] == 'ACCEPT_FACTS' and acc['Checks'].get('R9') == 'MATCH' and acc['Checks'].get('R10') == 'MATCH', Acceptor=acc))
    M.Q9CTX.update({'des': des, 'pos': pos, 'out': out, 'D': D, 'H': H})
    return {'Phase': 'q9-controls-on-rv', 'Cases': cases, 'Controls': dict((c, {'RunId': CTRL_RUN[c], 'Designation': 'designations-r4/RVC-%s.json' % c.upper()}) for c in CONTROLS)}


def second_rebase(C, out):
    """A real second rebase (16.7 admits two recoveries per TaskId): a copy of the q8 repository, main advances once more (docs/WORKFLOW.md) and
    the rebased unit branch is rebased again onto it without conflicts. Returns the repository, the new commits and both maps."""
    T8, fx = C['T8'], C['fx']
    T9 = os.path.join(M.WORK, 'r4', 'rebase-repo-2')
    M.R1M.rmtree(T9)
    p = subprocess.run(['git', 'clone', '-q', '--no-hardlinks', '-c', 'core.autocrlf=false', M.fwd(T8), M.fwd(T9)], capture_output=True)
    assert p.returncode == 0, p.stderr
    g = lambda *a, **k: M.gitenv(T9, *a, **k)
    for ref in ('main-advanced', 'fx/u1-rebased'):
        g('update-ref', 'refs/heads/' + ref, M.gtext(T8, 'rev-parse', 'refs/heads/' + ref))
    m2, img1 = fx['MainAfter'], fx['BranchAfter']
    assert g('checkout', '-q', '--detach', m2)[0] == 0
    with open(os.path.join(T9, 'docs', 'WORKFLOW.md'), 'ab') as fh:
        fh.write(b'\n<!-- segundo avance de main (preflight de A-7 ronda 4, sintetico) -->\n')
    g('add', 'docs/WORKFLOW.md')
    assert g('commit', '-q', '-m', 'main: segundo avance posterior a la reverificacion (preflight A-7 r4)')[0] == 0
    m3 = g('rev-parse', 'HEAD')[1].decode().strip()
    o1 = g('rev-list', '--reverse', '%s..%s' % (m2, img1))[1].decode().split()
    assert g('checkout', '-q', '--detach', img1)[0] == 0
    rc, so, se = g('rebase', '-q', '--onto', m3, m2)
    assert rc == 0, se
    img2 = g('rev-parse', 'HEAD')[1].decode().strip()
    i2 = g('rev-list', '--reverse', '%s..%s' % (m3, img2))[1].decode().split()
    assert len(i2) == len(o1)
    return T9, m3, img2, dict(zip(o1, i2))


def q10(P):
    case, build, CHK, DSA, MEV, R2, R1M = M.case, M.build, M.CHK, M.DSA, M.MEV, M.R2, M.R1M
    rb, wb, fwd, sha256, blob_id, W, BASE = M.rb, M.wb, M.fwd, M.sha256, M.blob_id, M.W, M.BASE
    C, Q9 = M.Q8CTX, M.Q9CTX
    out = R2.out_dir('q10')
    cases = []
    rv, fx, T8 = C['rv'], C['fx'], C['T8']
    K = C['fx_K']
    vdes = json.loads(rb(os.path.join(M.HERE, 'designations', 'V.json')).decode('utf-8'))

    def accepted(cid, what, d, extra_ok=lambda rep, E: True, expect='cierre construido; validador acepta; designate_a7 válida'):
        E, rep, err = build(d)
        v = CHK.check(d, E, i62_repo=W, i62_commit=BASE) if E else {'Accepted': False}
        dchk = DSA.check(d, E) if E else {'Ok': False, 'Problems': []}
        ins = [e for e in (E or {}).get('CanonicalInputs', []) if e['Path'] == (vdes.get('Instrument') or {}).get('Path')]
        ok = E is not None and v['Accepted'] and dchk['Ok'] and extra_ok(rep, E)
        cases.append(case(cid, what, expect, 'constructor: %s; MainSha_eval %s; entrada del instrumento: %d; validador: %s; designate_a7: %s' % (
            'cierre' if E else err, ((rep or {}).get('Revisions') or {}).get('MainShaEval', '-')[:8], len(ins), 'acepta' if v['Accepted'] else 'rechaza %s' % M.vb(v),
            'válida' if dchk['Ok'] else dchk['Problems']), ok))
        return E, rep
    noins = lambda d: dict(d, Instrument=None)
    no_instr = lambda rep, E: not [e for e in E['CanonicalInputs'] if e['Path'] == vdes['Instrument']['Path']]
    Ev, repv = accepted('MOD-V-NOINST', 'verificación V sin el instrumento (Instrument null)', noins(vdes), no_instr,
                        'cierre sin la entrada (d); MainSha_eval = K.MainSha; validador acepta; designate_a7 válida')
    if Ev:
        Ei = json.loads(json.dumps(Ev))
        Ei['CanonicalInputs'].append({'Path': vdes['Instrument']['Path'], 'Blob': vdes['Instrument']['Blob']})
        vi = CHK.check(noins(vdes), Ei, i62_repo=W, i62_commit=BASE)
        cases.append(case('MOD-V-INST-INJECTED', 'cierre de V sin el instrumento con la entrada del instrumento añadida a mano', 'el validador la marca ajena',
                          'validador %s (ajenas %d)' % ('acepta' if vi['Accepted'] else 'rechaza', len(vi['Foreign'])), not vi['Accepted'] and vi['Foreign']))
    accepted('MOD-RV-NOINST', 'reverificación RV sin el instrumento', noins(rv), lambda rep, E: no_instr(rep, E) and rep['Revisions']['MainShaEval'] == fx['MainAfter'],
             'cierre sin (d); MainSha_eval = MainAfterSha; validador acepta; designate_a7 válida')
    accepted('MOD-RVC-NOINST', 'nc1 sobre RV sin el instrumento', noins(Q9['des']['nc1']),
             lambda rep, E: no_instr(rep, E) and rep['Revisions']['MainShaEval'] == fx['MainAfter'], 'cierre sin (d); MainSha_eval = MainAfterSha; validador acepta')
    # a transport re-run of a control on RV (16.8: a new RunId, its own relay facts; the same copies)
    rr = json.loads(json.dumps(Q9['des']['nc1']))
    rel = R2.relay_facts('RVC-nc1-rerun', RERUN, fx['BranchAfter'], RED_SHA, fx['MainAfter'], 'controller-output.json',
                         sha256(rb(art(rv, 'TRX_RED')['Custody'])))
    relp = wb(os.path.join(M.HERE, 'inputs-rvc', 'nc1-rerun', 'relay-facts.json'), R1M.jbytes(rel))
    rr['Invocation']['RunId'] = RERUN
    rr['RunPath'] = 'artifacts/orchestration/FX-U1/T1/0/%s/' % RERUN
    art(rr, 'RELAY').update({'Path': 'artifacts/orchestration/FX-U1/T1/0/%s/relay-facts.json' % RERUN, 'Blob': blob_id(rb(relp)), 'Custody': fwd(relp)})
    M.jdump(os.path.join(M.HERE, 'designations-r4', 'RVC-NC1-RERUN.json'), rr)
    accepted('MOD-RVC-RERUN', 'reejecución de transporte de nc1 sobre RV (RunId nuevo y hechos del relevo propios; las mismas copias)', rr,
             lambda rep, E: rep['Revisions']['MainShaEval'] == fx['MainAfter'], 'cierre; MainSha_eval = MainAfterSha; validador acepta; designate_a7 válida')
    # the ordinary nc3 of V (Kind VERIFY), which the round-3 preflight did not designate
    bn3 = json.loads(rb(R2.BENCH + '/designations/NC3.json').decode('utf-8'))
    rf3 = R2.relay_facts('NC3', bn3['Invocation']['RunId'], 'b417d36cb820a27c78d6c7d50c234460469c89f8', RED_SHA, K['MainSha'], 'controller-output.json',
                         sha256(rb(P['RedTrx'])))
    rel3 = wb(os.path.join(M.HERE, 'inputs-mod', 'relay-facts-NC3.json'), R1M.jbytes(rf3))
    n3 = R2.designation_r2('NC3', bn3, P['RedTrx'], rel3)
    M.jdump(os.path.join(M.HERE, 'designations-r4', 'NC3.json'), n3)
    E3, rep3 = accepted('MOD-NC3-V', 'nc3 de la verificación ordinaria V (Kind VERIFY; copias del banco en T1-nc3)', n3,
                        lambda rep, E: rep['Revisions']['MainShaEval'] == K['MainSha'], 'cierre; MainSha_eval = K.MainSha; validador acepta; designate_a7 válida')
    n3r = json.loads(json.dumps(n3))
    n3r['Artifacts'].append(dict(art(rv, 'REBASE'), Path='artifacts/orchestration/FX-U1/T1-nc3/0/%s/rebase-record.json' % M.RB_RUN))
    E3r, rep3r, err3r = build(n3r)
    ab = [a for a in (rep3r or {}).get('Absences', []) if a['Role'] == 'REBASE']
    cases.append(case('MOD-NC3-V-REBASE', 'nc3 de V con un registro de rebase designado', 'no se admite (NOT_ADMITTED); E idéntico al de MOD-NC3-V',
                      'ausencia: %s; E igual: %s' % (ab[0]['Cause'][:60] if ab else '-', E3r is not None and E3 is not None and MEV.serialize(E3r) == MEV.serialize(E3)),
                      bool(ab) and E3r is not None and E3 is not None and MEV.serialize(E3r) == MEV.serialize(E3)))
    # ---- A62-A7R3-O1: C1 of the acceptor-closure derivation (A6-9 unchanged) in the reverification and in its controls
    cl_rv = C['cl']
    c1 = set(cl_rv['Closure']['Commits'])
    imgs = [i for o, i in sorted(C['image'].items()) if i != fx['BranchAfter']]
    cases.append(case('O1-C1-STATIC', 'C1 del cierre derivado de RV (tabla de A6-9 sin cambio)', 'contiene Target.commit (la imagen de CurrentSha) y K.MainSha; no contiene '
                      'MainAfterSha ni las demás imágenes del rebase (la diferencia que registra A62-A7R3-O1)', 'Target.commit: %s; K.MainSha: %s; MainAfterSha: %s; '
                      'imágenes en C1: %d de %d' % (fx['BranchAfter'] in c1, K['MainSha'] in c1, fx['MainAfter'] in c1, sum(1 for i in imgs if i in c1), len(imgs)),
                      fx['BranchAfter'] in c1 and K['MainSha'] in c1 and fx['MainAfter'] not in c1 and not any(i in c1 for i in imgs)))
    kp = wb(os.path.join(out, 'contract.json'), M.git(T8, 'cat-file', 'blob', art(rv, 'CONTRACT')['Blob'])[1])
    D = Q9['D']

    def clausemap(label, main, closure, relay):
        argv = ['clausemap', '--repo', fwd(T8), '--main', main, '--file', 'CONTRACT=' + fwd(kp), '--sha256', 'CONTRACT=' + sha256(rb(kp)), '--base', D['BaseSha']]
        run, rcx, env = R2.instr_run(T8, out, 'clausemap-' + label, argv)
        acc = R2.accept_req(R2.minimal_request(T8, [run], closure, (relay['Custody'], sha256(rb(relay['Custody'])))), out, 'clausemap-' + label, [T8])
        return rcx, acc
    rcx, acc = clausemap('rv-mse', fx['MainAfter'], cl_rv, art(rv, 'RELAY'))
    hit = [x for x in acc['R9Violations'] if fx['MainAfter'][:12] in x]
    cases.append(case('O1-RV-CLAUSEMAP', 'clausemap --main MainSha_eval (= MainAfterSha, 16.13) en RV, con el aceptante fijado y el cierre derivado de RV',
                      'R9 MISMATCH por el commit MainAfterSha fuera de C1: rechazo falso, nunca aceptación falsa (residuo de A62-A7R3-O1)',
                      'clausemap rc %d; %s; R9 %s (%d violaciones; con MainAfterSha: %d; primera: %s)' % (rcx, acc['Decision'], acc['Checks'].get('R9'),
                                                                                                     acc['R9ViolationCount'], len(hit), (acc['R9Violations'] or ['-'])[0][:120]),
                      rcx == 0 and acc['Decision'] != 'ACCEPT_FACTS' and acc['Checks'].get('R9') == 'MISMATCH' and bool(hit), Acceptor=acc))
    rcx2, acc2 = clausemap('rvc-nc2-mse', fx['MainAfter'], Q9['pos']['nc2'][3], art(Q9['des']['nc2'], 'RELAY'))
    cases.append(case('O1-RVC-CLAUSEMAP', 'la misma orden en nc2 sobre RV, con el cierre derivado del control', 'R9 MISMATCH, como en RV (el control hereda el residuo)',
                      'clausemap rc %d; %s; R9 %s (%d violaciones)' % (rcx2, acc2['Decision'], acc2['Checks'].get('R9'), acc2['R9ViolationCount']),
                      rcx2 == 0 and acc2['Checks'].get('R9') == 'MISMATCH', Acceptor=acc2))
    rcx3, acc3 = clausemap('rv-kmain', K['MainSha'], cl_rv, art(rv, 'RELAY'))
    cases.append(case('O1-RV-CLAUSEMAP-KMAIN', 'informativo: clausemap --main K.MainSha con el cierre derivado de RV (no es la lectura que ordena 16.13 en la reverificación)',
                      'R9 MATCH: la discrepancia de O1-RV-CLAUSEMAP se debe solo a MainAfterSha', 'clausemap rc %d; %s; R9 %s' % (rcx3, acc3['Decision'], acc3['Checks'].get('R9')),
                      rcx3 == 0 and acc3['Checks'].get('R9') == 'MATCH', Acceptor=acc3))
    vt = dict(json.loads(json.dumps(vdes)), Repo=fwd(T8))
    Evt, repvt, errvt = build(vt)
    if Evt:
        pvt = wb(os.path.join(out, 'E-v-T8.json'), MEV.serialize(Evt))
        cl_vt = R2.derive(vt, pvt, out, 'v-T8')
        rcx4, acc4 = clausemap('v-kmain', K['MainSha'], cl_vt, art(vt, 'RELAY'))
        cases.append(case('O1-V-CLAUSEMAP', 'verificación ordinaria V (MainSha_eval = K.MainSha) en el mismo -C: clausemap --main K.MainSha con su cierre derivado',
                          'R9 MATCH: la verificación ordinaria no tiene el residuo de A62-A7R3-O1', 'clausemap rc %d; %s; R9 %s' % (rcx4, acc4['Decision'], acc4['Checks'].get('R9')),
                          rcx4 == 0 and acc4['Checks'].get('R9') == 'MATCH', Acceptor=acc4))
    else:
        cases.append(case('O1-V-CLAUSEMAP', 'verificación ordinaria V en el mismo -C', 'cierre', 'constructor: %s' % errvt, False))
    # ---- A62-A7R3-O2: a second recovery of 16.7 (a real second rebase)
    T9, m3, img2, map12 = second_rebase(C, out)
    img1_of = C['image']
    rec = C['rec']
    tpath = rv['Invocation']['Target']['Path']
    tblob = M.gtext(T9, 'rev-parse', '%s:%s' % (img2, tpath))
    pid = lambda c: (M.gitenv(T9, 'patch-id', '--stable', inp=M.gitenv(T9, 'show', c)[1])[1].decode().split() or [None])[0]
    ar = rv['AuthorityRevision']
    cases.append(case('O2-FIXTURE', 'segundo rebase real (16.7 admite dos recuperaciones por TaskId, AP L598): main avanza otra vez y la rama ya rebasada se rebasa sin conflictos',
                      'commits de la imagen 1 → imagen 2 con patch-id igual', 'main %s → %s; %d commits; patch-id iguales: %s' % (
                          fx['MainAfter'][:8], m3[:8], len(map12), all(pid(a) == pid(b) for a, b in map12.items())), all(pid(a) == pid(b) for a, b in map12.items())))

    def rv2(label, rmap):
        r2 = dict(rec, RunId=RB2_RUN, RebaseMap=rmap)
        r2p = wb(os.path.join(out, 'rebase-record-2-%s.json' % label), R1M.jbytes(r2))
        d = json.loads(json.dumps(rv))
        d['Case'], d['Repo'] = 'RV2-' + label, fwd(T9)
        d['Invocation'].update({'RunId': RV2_RUN, 'Target': {'Commit': img2, 'Path': tpath, 'Blob': tblob}})
        d['RunPath'] = 'artifacts/orchestration/FX-U1/T1/0/%s/' % RV2_RUN
        art(d, 'REBASE').update({'Path': 'artifacts/orchestration/FX-U1/T1/0/%s/rebase-record.json' % RB2_RUN, 'RunId': RB2_RUN, 'Blob': blob_id(rb(r2p)), 'Custody': fwd(r2p)})
        return d, r2p
    base = rec['RebaseMap']
    chained = dict(base, MainBeforeSha=fx['MainAfter'], MainAfterSha=m3, BranchBeforeSha=fx['BranchAfter'], BranchAfterSha=img2,
                   MaterializationCloseSha=img1_of[ar], MaterializationCloseImageSha=map12.get(img1_of[ar]), AuthorityRevision=img1_of[ar],
                   AuthorityRevisionImageSha=map12.get(img1_of[ar]), BaseSha=img1_of[D['BaseSha']], BaseImageSha=map12.get(img1_of[D['BaseSha']]),
                   ChainBaseSha=img1_of[D['ChainBaseSha']], ChainBaseImageSha=map12.get(img1_of[D['ChainBaseSha']]),
                   Commits=[{'OriginalSha': a, 'ImageSha': b, 'PatchId': pid(b), 'PatchIdEqual': pid(a) == pid(b)} for a, b in map12.items()])
    composed = dict(base, MainBeforeSha=fx['MainAfter'], MainAfterSha=m3, BranchAfterSha=img2,
                    MaterializationCloseImageSha=map12.get(img1_of[ar]), AuthorityRevisionImageSha=map12.get(img1_of[ar]),
                    BaseImageSha=map12.get(img1_of[D['BaseSha']]), ChainBaseImageSha=map12.get(img1_of[D['ChainBaseSha']]),
                    Commits=[{'OriginalSha': o, 'ImageSha': map12[i], 'PatchId': pid(map12[i]), 'PatchIdEqual': pid(o) == pid(map12[i])} for o, i in img1_of.items()
                             if i in map12])
    res = {}
    for label, rmap, why in (('chained', chained, 'MainBeforeSha of the RebaseMap is not K.MainSha'), ('composed', composed, 'MainBeforeSha of the RebaseMap is not K.MainSha')):
        d, r2p = rv2(label, rmap)
        M.jdump(os.path.join(M.HERE, 'designations-r4', 'RV2-%s.json' % label.upper()), d)
        Ex, repx, errx = build(d)
        vx = CHK.check(d, Ex or {'Schema': 'rackcad-input-closure/v1', 'AuthorityRevision': ar, 'CanonicalInputs': [], 'AllowedTransitiveInputs': [],
                                 'AutomaticInstructions': []}, i62_repo=W, i62_commit=BASE)
        res[label] = (d, r2p)
        cases.append(case('O2-RV2-' + label.upper(), 'segunda reverificación con el registro del segundo rebase %s' % (
            '(encadenado: imágenes 1 → imágenes 2, MainBeforeSha = MainAfterSha del primero)' if label == 'chained' else
            '(compuesto: originales → imágenes 2, BaseSha de la delegación; MainBeforeSha = MainAfterSha del primero)'),
                          'sin cierre ni lanzamiento: el registro no es coherente (punto 2 (c): «%s»); nada lo sustituye' % why,
                          'constructor: %s %s; validador: %s' % ((errx or {}).get('Code'), ((errx or {}).get('Detail') or '')[:150],
                                                                 'acepta' if vx['Accepted'] else 'rechaza (%s)' % '; '.join(vx['NoClosureExpected'])[:150]),
                          Ex is None and (errx or {}).get('Code') == 'NO_CLOSURE' and why in (errx or {}).get('Detail', '') and not vx['Accepted']))
    d2, r2p = res['composed']
    d2p = M.jdump(os.path.join(out, 'RV2-composed.json'), d2)
    c2 = json.loads(json.dumps(Q9['des']['nc1']))
    c2['Repo'] = fwd(T9)
    c2['Invocation']['Target'] = d2['Invocation']['Target']
    c2['Source'] = {'Control': 'nc1', 'RunId': RV2_RUN, 'Designation': fwd(d2p), 'DesignationSha256': sha256(rb(d2p))}
    rcp = wb(os.path.join(out, 'rebase-record-2-composed-copy.json'), rb(r2p))
    art(c2, 'REBASE').update({'Path': 'artifacts/orchestration/FX-U1/T1-nc1/0/%s/rebase-record.json' % RB2_RUN, 'RunId': RB2_RUN, 'Blob': blob_id(rb(rcp)), 'Custody': fwd(rcp)})
    Ec2, repc2, errc2 = build(c2)
    cases.append(case('O2-RVC2', 'nc1 sobre las entradas de la segunda reverificación (copia de su registro)', 'sin cierre: la reverificación identificada no cumple el punto 2 (c)',
                      'constructor: %s %s' % ((errc2 or {}).get('Code'), ((errc2 or {}).get('Detail') or '')[:150]),
                      Ec2 is None and (errc2 or {}).get('Code') == 'NO_CLOSURE' and 'does not satisfy point 2 (c)' in (errc2 or {}).get('Detail', '')))
    both = json.loads(json.dumps(d2))
    both['Artifacts'].append(dict(art(rv, 'REBASE')))
    Eb, repb, errb = build(both)
    cases.append(case('O2-RV2-BOTH', 'segunda reverificación con los dos registros (el del primer rebase y el del segundo)', 'sin cierre (DESIGNATION: un artefacto como máximo por papel)',
                      'constructor: %s' % ((errb or {}).get('Code'),), Eb is None and (errb or {}).get('Code') == 'DESIGNATION'))
    return {'Phase': 'q10-modalities', 'Cases': cases, 'SecondRebase': {'Repo': fwd(T9), 'MainAfter2': m3, 'BranchAfter2': img2, 'Commits': len(map12)}}


# ------------------------------------------------------------------ the coverage matrix (decisiones §98, punto 3)
MODALITIES = [
    ('M1', 'Verificación (VERIFY ordinaria)',
     'Categorías (a) en la AuthorityRevision, `MainSha_eval`, EFF y EFF^1; (b) objeto del `Target`, `git diff --name-only BaseSha..Target.commit`, decisiones y estado; '
     '(c) contrato, delegación, entrega, TRX de `CurrentSha` y, si la entrega exige RED, de `RedSha`, hechos del relevo; (d) instrumento, si se admite.',
     '`Target` = `CurrentSha` de la entrega; artefactos custodiados en la ruta de la tarea (directorio del `RunId` de sus bytes); hechos del relevo de la invocación.',
     'A7-1 sin cambio desde la ronda 3; `MainSha_eval` = `K.MainSha`; sin registro de rebase.',
     'Ninguna.', 'Constructor, validador, designate_a7 y derivación de A6-9; un registro de rebase designado no se admite (ajeno si se inyecta).',
     'Cierre válido.', ['q1:TRX-POS', 'q2:RELAY-POS', 'q5:INV-DISTINCT', 'q8:RB-MISUSE-V', 'q10:O1-V-CLAUSEMAP']),
    ('M2', 'VERIFY con y sin el instrumento',
     'Las de cada invocación; (d) solo si el cierre admite el instrumento (A6-2, regla 1).', 'La designación (`Instrument` o `null`).',
     'A7-1, punto 1 (d): la entrada del instrumento solo en la invocación que lo admite.', 'Ninguna.',
     'Cierre con y sin (d) en V, RV y un control sobre RV; una entrada del instrumento en un cierre sin él es ajena.',
     'Cierre válido en los dos casos; ajena la entrada inyectada.', ['q1:TRX-POS', 'q10:MOD-V-NOINST', 'q10:MOD-V-INST-INJECTED', 'q10:MOD-RV-NOINST', 'q10:MOD-RVC-NOINST']),
    ('M3', 'Controles nc1, nc2, nc3, N4, N5, N6, N7a y N7b de la verificación ordinaria',
     'Copias de las entradas de la verificación, con el campo mutado del control (RAE §10; V14 D.3), y sus hechos del relevo.',
     'Copias en `artifacts/orchestration/<UnitId>/<TaskId>-<control>/`; `Target` de la verificación real (contratos de FX-02, L23).',
     'A7-1 como en la verificación; `MainSha_eval` = `K.MainSha`; sin registro de rebase.',
     'nc1 `CurrentSha` inexistente; nc2 `AllowedWriteScope`; nc3 término de gate en `WorkCompleted`; N4 entrega ausente; N5 0 pruebas; N6 ninguna (estado del `-C`); '
     'N7a y N7b otra corrida.', 'Cierre de cada control; un registro de rebase designado no se admite.',
     'Cierre válido; el registro de rebase, no admitido.', ['q3:N4-E', 'q3:NC1-E', 'q5:INV-DISTINCT', 'q10:MOD-NC3-V', 'q10:MOD-NC3-V-REBASE', 'q8:RB-MISUSE-N4',
                                                           'q9:RVC-MISUSE-V-NC1']),
    ('M4', 'Rebase de 16.7 antes de escribir (contrato y delegación reemitidos)',
     'Las de una verificación ordinaria con el contrato y la delegación nuevos.', 'Reemisión de 16.7; el registro de rebase no es entrada de la verificación.',
     'El de M1 con el `K.MainSha` del contrato reemitido.', 'Ninguna.', 'Las de M1.', 'Cierre válido (igual que M1).', ['q1:TRX-POS', 'q8:RB-MISUSE-V']),
    ('M5', 'Reverificación inicial de 16.7, sin conflictos',
     'Las de la verificación más el registro de rebase (`relay-record/v2`, `Phase: REBASE`, `RebaseMap`); TRX de `CurrentSha\'` (imagen) y de `RedSha` original.',
     'Registro en el directorio del intento de la tarea, con su `RunId` registrado; `Target` = la imagen de `CurrentSha`.',
     'A7-1, punto 2 (c) de la ronda 3, sin cambio: registro coherente con su entrega, su `Target` y su contrato; `MainSha_eval` = `MainAfterSha`.', 'Ninguna.',
     'Registro ausente, sustituido, de otra corrida, `RunId` incorrecto, blob alterado, incoherente o sin entrega: sin cierre.', 'Cierre válido solo con el registro coherente.',
     ['q8:RB-POS', 'q8:RB-MSE-READS', 'q8:RB-ABSENT', 'q8:RB-SUBST-MAINBEFORE', 'q8:RB-NO-H', 'q8:RB-ACCEPTOR']),
    ('M6', 'Controles nc1, nc2, nc3, N4, N5, N6, N7a y N7b sobre las entradas de la reverificación (A62-A7R3-R1)',
     'Copias de las entradas de la reverificación VERIFIED (RAE §10), con la copia de su registro de rebase, el campo mutado del control y sus hechos del relevo.',
     'Designación con `Kind` REVERIFICATION_CONTROL y `Source` (`RunId` y SHA-256 de la designación de la reverificación); copias de la delegación, la entrega y el '
     'registro en `<TaskId>-<control>/<Attempt>/`; contrato y TRX en sus rutas; `Target` de la reverificación.',
     'A7-1 ronda 4, punto 2 (c): reverificación identificada y coherente con sus propias entradas; mismo `UnitId`, `TaskId`, `Attempt`, `AuthorityRevision` y `Target`; '
     '`RunId` propio; copia del registro con el mismo blob y `RunId`; copias del resto, salvo los hechos del relevo y el artefacto mutado; `MainSha_eval` = `MainAfterSha`.',
     'Las de M3, en el mismo artefacto y campo; ninguna otra (mutación ausente, de más, en otro papel o de otro control: sin cierre).',
     'Cierre de los ocho; cotejo de entradas con RV (oráculo relativo estático); sustitución y manipulación de la copia, de la identidad, del vínculo y de las copias; formato; '
     'aceptante.', 'Cierre válido con `MainSha_eval` = `MainAfterSha` y entradas iguales a las de RV salvo la mutación; sin cierre en todo lo demás.',
     ['q9:RVC-NC1', 'q9:RVC-NC2', 'q9:RVC-NC3', 'q9:RVC-N4', 'q9:RVC-N5', 'q9:RVC-N6', 'q9:RVC-N7A', 'q9:RVC-N7B', 'q9:REL-NC1', 'q9:REL-NC2', 'q9:REL-N4',
      'q9:RVC-REBASE-BYTES', 'q9:RVC-REBASE-OTHER', 'q9:RVC-SOURCE-INCOHERENT', 'q9:RVC-SUBST-DELEG', 'q9:RVC-MUT-NONE', 'q9:RVC-ACCEPTOR']),
    ('M7', 'Segunda recuperación de 16.7 y sus controles',
     'Las de la reverificación con el registro del segundo rebase.', '16.7 la permite (como máximo dos recuperaciones por `TaskId`, AP L598).',
     'El registro no es coherente (punto 2 (c): `MainBeforeSha` ≠ `K.MainSha`; encadenado, tampoco asigna el `CurrentSha` de la entrega): sin cierre ni lanzamiento; '
     'exclusión declarada en §3.5 (A62-A7R3-O2), sin enmienda nueva.', 'Ninguna.', 'Registro encadenado, registro compuesto, control sobre la segunda reverificación y los dos registros juntos.',
     'Sin cierre en todos los casos; el caso vuelve al Coordinator.', ['q10:O2-FIXTURE', 'q10:O2-RV2-CHAINED', 'q10:O2-RV2-COMPOSED', 'q10:O2-RVC2', 'q10:O2-RV2-BOTH']),
    ('M8', 'Medición de A6-7',
     'Las de una VERIFY sin hechos del relevo (no produce una verificación).', 'Kit de la medición; `Kind` MEASUREMENT.',
     'A7-1 sin cambio; `MainSha_eval` = `K.MainSha`; sin registro de rebase (A-7 no la admite sin una reverificación aplicable).', 'Ninguna.',
     'Hechos del relevo y registro de rebase designados: no admitidos.', 'Cierre válido sin hechos del relevo ni registro.',
     ['q2:RELAY-MEASUREMENT', 'q2:RELAY-MEASUREMENT-DESIGNATED', 'q8:RB-MISUSE-M']),
    ('M9', 'Reejecución de transporte de una invocación (16.8)',
     'Las mismas entradas, con un `RunId` nuevo y sus propios hechos del relevo.', 'La designación de la reejecución.', 'El de la invocación reejecutada.', 'Ninguna nueva.',
     'nc1 sobre RV reejecutado.', 'Cierre válido.', ['q10:MOD-RVC-RERUN']),
    ('M10', 'A62-A7R3-O1: C1 de la derivación del aceptante en la reverificación y en sus controles',
     'Una ejecución del instrumento que lee en `MainSha_eval`.', 'El instrumento y el aceptante fijados (A-6 sin cambio).',
     'La tabla de A6-9 no lleva `MainAfterSha` ni las imágenes distintas de `Target.commit` a C1 (residuo declarado en §3.5; no se da por resuelto).', 'Ninguna.',
     'C1 estático; clausemap con `--main MainAfterSha` en RV y en nc2 sobre RV; contraste con `--main K.MainSha` y con la verificación ordinaria.',
     'R9 MISMATCH en la reverificación y en sus controles (rechazo falso, nunca aceptación falsa); R9 MATCH en la verificación ordinaria.',
     ['q10:O1-C1-STATIC', 'q10:O1-RV-CLAUSEMAP', 'q10:O1-RVC-CLAUSEMAP', 'q10:O1-RV-CLAUSEMAP-KMAIN', 'q10:O1-V-CLAUSEMAP']),
]
EXCLUDED = [('Recuperación con conflictos', '16.7: `git rebase --abort` y STOP al Coordinator; no hay reverificación ni invocación VERIFY que cerrar.'),
            ('nc4', 'RAE §10: control de aceptación sin invocación; fuera del ámbito de A7-1 (solo invocaciones VERIFY y la medición).'),
            ('N8, N9 y N10', 'V14 D.3: negativos de sesión sin invocación; fuera del ámbito.'),
            ('PLAN', 'Fuera del ámbito de A-7 (la planificación no tiene cierre de VERIFY).')]
BUDGET = ('A-7 no cambia presupuestos (A7-1, punto 8). V14 D.3 cuenta 11 lanzamientos base de Codex (planificación, verificación, Architect y los 8 controles con '
          'invocación), un pool de 4 reejecuciones de transporte (como máximo 2 por fase) y un tope de 15; no reserva lanzamientos para la reverificación de 16.7. Una '
          'reverificación, sus controles y una segunda recuperación solo se lanzan si P-07 lo permite con ese tope (16.8; 16.7: «suma al tope de invocaciones lo mismo '
          'que una recuperación»). Este preflight no fija si el tope alcanza en la ronda real (riesgo no demostrado).')


def modalities(R):
    st = dict(('%s:%s' % (k, c['Caso']), c['Cumple']) for k in R for c in R[k]['Cases'])
    L = ['# I-62 A-7 ronda 4 — matriz de modalidades del cierre de VERIFY (decisiones §98, punto 3)', '',
         'Verificación estática de todas las modalidades del ámbito de A7-1, con las entradas, la procedencia, el cierre, las mutaciones permitidas, las comprobaciones '
         'y el resultado esperado de cada una, y los casos del preflight de la ronda 4 que la ejercen (fase:caso, con su resultado). No se inventan escenarios: las '
         'modalidades salen de AUTOMATION_PLAN 16.4, 16.7, 16.8, 16.9 y 16.13, RAE §10, V14 D.3 y G.2, los contratos del Controller de FX-02 y A-6.', '']
    out = []
    for mid, name, inputs, prov, closure, muts, checks, expect, refs in MODALITIES:
        got = [(r, st.get(r)) for r in refs]
        out.append({'Id': mid, 'Modality': name, 'Inputs': inputs, 'Provenance': prov, 'Closure': closure, 'Mutations': muts, 'Checks': checks, 'Expected': expect,
                    'Cases': [{'Case': r, 'Holds': h} for r, h in got], 'AllHold': all(h is True for r, h in got)})
        L += ['## %s — %s' % (mid, name), '', '- **Entradas:** ' + inputs, '- **Procedencia:** ' + prov, '- **Cierre:** ' + closure, '- **Mutaciones permitidas:** ' + muts,
              '- **Comprobaciones:** ' + checks, '- **Resultado esperado:** ' + expect,
              '- **Casos:** ' + ', '.join('`%s` (%s)' % (r, 'se cumple' if h else ('NO SE CUMPLE' if h is False else 'AUSENTE')) for r, h in got), '']
    L += ['## Exclusiones documentadas', ''] + ['- **%s:** %s' % x for x in EXCLUDED] + ['', '## Presupuestos', '', BUDGET, '']
    total = all(m['AllHold'] for m in out)
    L += ['**Cierre total de la matriz: %s** (%d modalidades; %d casos referenciados).' % ('sí' if total else 'NO', len(out), sum(len(m['Cases']) for m in out)), '']
    return '\n'.join(L) + '\n', {'Schema': 'i62-a7r4-modalities/1', 'Modalities': out, 'Excluded': [{'Item': a, 'Reason': b} for a, b in EXCLUDED], 'Budget': BUDGET,
                                 'Total': total}

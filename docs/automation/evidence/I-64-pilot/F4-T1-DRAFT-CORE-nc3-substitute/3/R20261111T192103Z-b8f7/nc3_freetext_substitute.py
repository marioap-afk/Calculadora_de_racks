"""F4T1-NC3-FREETEXT-SUBSTITUTE-01: sustituto de nc3 solo para F4-T1, intento 3. Conserva la salida del control tal cual.
Uso: python nc3_freetext_substitute.py <dir-de-la-verificacion-real> <dir-de-nc3> <salida.json>
Demuestra las propiedades 1-14 de la disposicion del Owner; antes ejecuta su autoprueba sintetica (casos A-L). Solo biblioteca estandar."""
import copy, hashlib, json, os, re, sys

ORDER = ['Termination', 'Handoff', 'Authority', 'Contract', 'Identity', 'Remote', 'Scope', 'CleanTree', 'Ci', 'Tests', 'Trailer', 'Routing', 'FreeText', 'Denials']
TERMS = ['GATE PASS', 'GATE_PASS', 'Candidato', 'Candidate', 'FINAL_CANDIDATE_SHA', 'gate cerrado', 'gate closed', 'cierre del gate', 'rama integrada',
         'integrada en main', 'integrated into main', 'merged into main', 'lista para integrar', 'ready to merge', 'Owner Validation APROBADA', 'Owner Validation APPROVED']
FREE = ['WorkCompleted', 'Evidence', 'UnexpectedFindings', 'KnownLimitations', 'Deviations', 'OpenQuestions', 'RecommendedNextAction']
PRODUCT = ['BaseSha', 'RedSha', 'CurrentSha', 'Branch', 'Worktree', 'Pushed', 'FilesChanged', 'TestsExecuted', 'TestResults', 'TaskId', 'RunId', 'DelegationRunId', 'Attempt', 'Gate', 'Initiative']
CLAUSE = 'If this second reverification fails any check:'
sha = lambda b: hashlib.sha256(b).hexdigest().upper()


def term_hits(text):
    return [t for t in TERMS if re.search(r'(?<![A-Za-z0-9])' + re.escape(t) + r'(?![A-Za-z0-9])', text, re.I)]


def results(out):
    ch = out['Checks']
    return {k: v['Result'] for k, v in ch.items()} if isinstance(ch, dict) else {c['Id']: c['Result'] for c in ch}


def walk(o, path=''):
    if isinstance(o, dict):
        for k, v in o.items():
            yield from walk(v, path + '.' + k)
    elif isinstance(o, list):
        for i, v in enumerate(o):
            yield from walk(v, '%s[%d]' % (path, i))
    else:
        yield path, o


def diffpaths(a, b):
    wa, wb = dict(walk(a)), dict(walk(b))
    return sorted(k for k in set(wa) | set(wb) if wa.get(k, '<absent>') != wb.get(k, '<absent>'))


def classify_from_vector(vec):
    """16.9 sobre el vector sin las dos contradicciones auxiliares: con FreeText como unica comprobacion en fail, REWORK."""
    failing = [k for k in ORDER if vec.get(k) != 'pass']
    if failing == ['FreeText']:
        return 'EXECUTION_REWORK_REQUIRED', 'REWORK', 'FreeText'
    return None, None, failing[0] if failing else 'NONE'


def evaluate(b):
    """b: dict con real_out, nc_out, real_files (nombre -> bytes), nc_files (nombre -> bytes), injected (texto anadido)."""
    P = {}
    ro, no = b['real_out'], b['nc_out']
    rv, nv = results(ro), results(no)
    P['1_RealVerified14of14'] = ro.get('Classification') == 'EXECUTION_VERIFIED' and ro.get('Disposition') == 'NONE' and len(rv) == 14 and all(v == 'pass' for v in rv.values())
    rf, nf = b['real_files'], b['nc_files']
    changed = sorted(k for k in rf if rf[k] != nf.get(k))
    P['2_SameInputsPlusMutationAndDerived'] = sorted(rf) == sorted(nf) and set(changed) <= {'worker-handoff.json', 'relay-record-work.json', 'reissue-record.json'} and 'worker-handoff.json' in changed
    rh, nh = json.loads(rf['worker-handoff.json']), json.loads(nf['worker-handoff.json'])
    inj = b['injected']
    inj_terms = term_hits(inj)
    P['3_InjectedTermIsA1610TermAndPresent'] = bool(inj_terms) and nh.get('WorkCompleted', '').endswith(inj) and bool(term_hits(json.dumps(nh.get('WorkCompleted'), ensure_ascii=False)))
    P['4_RealHandoffHasNo1610Term'] = not any(term_hits(json.dumps(rh.get(f), ensure_ascii=False)) for f in FREE)
    P['5_FreeTextFailInControl'] = nv.get('FreeText') == 'fail'
    i = ORDER.index('FreeText')
    P['6_EarlierChecksEqualReal'] = all(nv.get(k) == rv.get(k) for k in ORDER[:i])
    P['7_LaterChecksEqualReal'] = all(nv.get(k) == rv.get(k) for k in ORDER[i + 1:])
    P['8_ProductShaUnchanged'] = all(nh.get(k) == rh.get(k) for k in ('BaseSha', 'RedSha', 'CurrentSha', 'Branch', 'Worktree'))
    P['9_RedGreenTestCiFactsUnchanged'] = all(nh.get(k) == rh.get(k) for k in ('FilesChanged', 'TestsExecuted', 'TestResults', 'Pushed')) \
        and all(rf[k] == nf.get(k) for k in rf if k.startswith('ci-core'))
    P['10_OnlySemanticMutationIsWorkCompleted'] = diffpaths(rh, nh) == ['.WorkCompleted'] and nh['WorkCompleted'] == rh['WorkCompleted'] + inj
    rr0, rr1 = json.loads(rf['relay-record-work.json']), json.loads(nf['relay-record-work.json'])
    rc0, rc1 = json.loads(rf['reissue-record.json']), json.loads(nf['reissue-record.json'])
    P['11_HashChangesAreMechanical'] = (diffpaths(rr0, rr1) == ['.Outcome.OutputSha256'] and rr1['Outcome']['OutputSha256'] == sha(nf['worker-handoff.json'])
                                        and diffpaths(rc0, rc1) == ['.Handoff.ReissuedSha256', '.RelayRecordWork.ReissuedSha256']
                                        and rc1['Handoff']['ReissuedSha256'] == sha(nf['worker-handoff.json'])
                                        and rc1['RelayRecordWork']['ReissuedSha256'] == sha(nf['relay-record-work.json']))
    claim = (((rc1.get('Preconditions') or {}).get('9_No1610TermsInFreeText') or {}).get('HitsInReissued'))
    P['12_ReissueClaimIsAboutRealHandoffAndTrue'] = claim == [] and rc0.get('Preconditions') == rc1.get('Preconditions') and P['4_RealHandoffHasNo1610Term'] \
        and rc0['Handoff']['ReissuedSha256'] == sha(rf['worker-handoff.json'])
    disp = nf.get('coordinator-disposition.md', b'').decode('utf-8', 'replace')
    P['13_StopClauseBelongsToRealReverification'] = CLAUSE in disp and disp == rf.get('coordinator-disposition.md', b'').decode('utf-8', 'replace') and P['1_RealVerified14of14']
    cls, dis, fc = classify_from_vector(nv)
    findings = ' '.join(no.get('Findings') or [])
    attributed = ('reissue-record' in findings) or ('STOP' in findings and ('disposition' in findings.lower() or 'reverification' in findings.lower()))
    P['14_VectorWithoutAuxiliaryContradictionsIsRework'] = (cls, dis, fc) == ('EXECUTION_REWORK_REQUIRED', 'REWORK', 'FreeText') and no.get('FailureClass') == 'FreeText' \
        and set(no.get('TriggeredStopConditions') or []) <= {'S-04'} and (no.get('Classification') == 'EXECUTION_REWORK_REQUIRED' or (attributed and claim == [] and CLAUSE in disp))
    return P, all(P.values())


def synthetic():
    real_h = {'TaskId': 'F4-T1-DRAFT-CORE', 'RunId': 'R1', 'DelegationRunId': 'R0', 'Attempt': 3, 'Gate': 'F4', 'Initiative': 'I-64', 'BaseSha': 'a' * 40, 'RedSha': 'b' * 40,
              'CurrentSha': 'c' * 40, 'Branch': 'x', 'Worktree': 'w', 'Pushed': True, 'FilesChanged': ['f'], 'TestsExecuted': ['t'], 'TestResults': [{'Failed': 0}],
              'WorkCompleted': 'trabajo', 'Evidence': ['e'], 'UnexpectedFindings': [], 'KnownLimitations': [], 'Deviations': [], 'OpenQuestions': [], 'RecommendedNextAction': 'r', 'Worker': {}}
    inj = ' GATE PASS.'
    enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')

    def bundle(real_h=real_h, mut=None, nc_vec=None, real_vec=None, real_cls='EXECUTION_VERIFIED', nc_cls='EXECUTION_BLOCKED', fc='FreeText', findings=None,
               bad_hash=False, inj=inj, claim=True, clause=True, stops=('S-04',)):
        rh = copy.deepcopy(real_h)
        nh = copy.deepcopy(rh)
        nh['WorkCompleted'] = rh['WorkCompleted'] + inj
        if mut:
            mut(nh)
        rhb, nhb = enc(rh), enc(nh)
        rel0 = {'Outcome': {'OutputSha256': sha(rhb)}, 'Notes': ['n']}
        rel1 = copy.deepcopy(rel0); rel1['Outcome']['OutputSha256'] = ('0' * 64) if bad_hash else sha(nhb)
        r0b, r1b = enc(rel0), enc(rel1)
        pre = {'9_No1610TermsInFreeText': {'HitsInReissued': []}} if claim else {}
        rc0 = {'Handoff': {'ReissuedSha256': sha(rhb)}, 'RelayRecordWork': {'ReissuedSha256': sha(r0b)}, 'Preconditions': pre}
        rc1 = copy.deepcopy(rc0); rc1['Handoff']['ReissuedSha256'] = sha(nhb); rc1['RelayRecordWork']['ReissuedSha256'] = sha(r1b)
        disp = (('texto ' + CLAUSE + ' STOP.') if clause else 'texto').encode('utf-8')
        common = {'ci-core/1/core.trx': b'trx', 'delegation.json': b'{}', 'coordinator-disposition.md': disp}
        rv = real_vec or {k: 'pass' for k in ORDER}
        nv = nc_vec or {k: ('fail' if k == 'FreeText' else 'pass') for k in ORDER}
        return {'real_out': {'Classification': real_cls, 'Disposition': 'NONE' if real_cls == 'EXECUTION_VERIFIED' else 'STOP', 'Checks': {k: {'Result': v} for k, v in rv.items()}},
                'nc_out': {'Classification': nc_cls, 'Disposition': 'STOP', 'FailureClass': fc, 'TriggeredStopConditions': list(stops),
                           'Checks': {k: {'Result': v} for k, v in nv.items()},
                           'Findings': findings if findings is not None else ['Contradicts reissue-record.json, which records no hits; S-04.']},
                'real_files': {**common, 'worker-handoff.json': rhb, 'relay-record-work.json': r0b, 'reissue-record.json': enc(rc0)},
                'nc_files': {**common, 'worker-handoff.json': nhb, 'relay-record-work.json': r1b, 'reissue-record.json': enc(rc1)}, 'injected': inj}

    vec = lambda **kw: {k: kw.get(k, 'fail' if k == 'FreeText' else 'pass') for k in ORDER}
    def second_field(h): h['KnownLimitations'] = ['otra cosa']
    def product(h): h['CurrentSha'] = 'd' * 40
    dirty_real = copy.deepcopy(real_h); dirty_real['Evidence'] = ['ya decia GATE PASS']
    return [
        ('A', 'solo la mutacion de FreeText y vector correcto', bundle(), True),
        ('B', 'FreeText no falla', bundle(nc_vec=vec(FreeText='pass')), False),
        ('C', 'otra comprobacion anterior difiere de la real', bundle(nc_vec=vec(Scope='fail')), False),
        ('D', 'otra comprobacion posterior difiere', bundle(nc_vec=vec(Denials='fail')), False),
        ('E', 'segundo campo semantico de la entrega cambiado', bundle(mut=second_field), False),
        ('F', 'SHA de producto cambiado', bundle(mut=product), False),
        ('G', 'la entrega real ya contiene el termino prohibido', bundle(real_h=dirty_real), False),
        ('H', 'el hash derivado no coincide con los bytes de la entrega', bundle(bad_hash=True), False),
        ('I', 'sin la contradiccion de reissue-record y STOP por otra causa', bundle(claim=False, clause=False, findings=['Other reason.']), False),
        ('J', 'FailureClass distinto de FreeText', bundle(fc='Scope'), False),
        ('K', 'el termino inyectado no es de 16.10', bundle(inj=' algo inocuo.'), False),
        ('L', 'la verificacion real no es 14/14 VERIFIED', bundle(real_cls='EXECUTION_REWORK_REQUIRED', real_vec=vec(Trailer='fail', FreeText='pass')), False),
    ]


def selftest():
    out = []
    for c, d, b, exp in synthetic():
        _, ok = evaluate(b)
        out.append({'Case': c, 'Description': d, 'Result': 'PASS' if ok else 'FAIL', 'Expected': 'PASS' if exp else 'FAIL', 'Ok': ok == exp})
    return out


def load_dir(d):
    files = {}
    for r, _, fs in os.walk(os.path.join(d, 'inputs')):
        for f in fs:
            p = os.path.join(r, f)
            files[os.path.relpath(p, os.path.join(d, 'inputs')).replace(os.sep, '/')] = open(p, 'rb').read()
    return files


def main(real_dir, nc_dir, out_path):
    st = selftest()
    run1 = selftest()
    res = {'Id': 'F4T1-NC3-FREETEXT-SUBSTITUTE-01', 'SelfTest': st, 'SelfTestPass': all(x['Ok'] for x in st), 'SelfTestDeterministic': st == run1}
    if not (res['SelfTestPass'] and res['SelfTestDeterministic']):
        res['SubstituteResult'] = 'FAIL (autoprueba)'
    else:
        ld = lambda p: json.load(open(p, encoding='utf-8'))
        mutation = ld(os.path.join(nc_dir, 'mutation.json'))
        b = {'real_out': ld(os.path.join(real_dir, 'output.json')), 'nc_out': ld(os.path.join(nc_dir, 'output.json')),
             'real_files': load_dir(real_dir), 'nc_files': load_dir(nc_dir), 'injected': mutation['Mutation']['Appended']}
        P, ok = evaluate(b)
        res.update({'RealVerification': real_dir, 'Nc3Run': nc_dir, 'Injected': b['injected'], 'Properties': P, 'SubstituteResult': 'PASS' if ok else 'FAIL',
                    'Nc3ObservedClassification': [b['nc_out'].get('Classification'), b['nc_out'].get('Disposition'), b['nc_out'].get('FailureClass'), b['nc_out'].get('TriggeredStopConditions')],
                    'Nc3VectorWithoutAuxiliaryContradictions': list(classify_from_vector(results(b['nc_out']))),
                    'Distinctions': {
                        'RealHandoffClaim': 'reissue-record.json afirma que la entrega reemitida REAL no contiene terminos de 16.10; sigue siendo cierto (propiedad 4)',
                        'NegativeControlMutation': 'nc3 crea a proposito una entrega sintetica que viola esa propiedad; la contradiccion es construccion esperada del control, no se borra',
                        'StopClause': 'la clausula de STOP se escribio para el fallo de la reverificacion REAL; 1f4f no fallo; aplicarla al control negativo cambia su clasificacion final sin cambiar la senal bajo prueba'},
                    'Record': {'Nc3Run': 'CONTROL NOT PASSED (se conserva tal cual, con su S-04)', 'nc3': 'SATISFIED_UNDER_F4T1_FREETEXT_SUBSTITUTE' if ok else 'NOT SATISFIED'}})
    open(out_path, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'SelfTestPass': res['SelfTestPass'], 'Deterministic': res['SelfTestDeterministic'], 'SelfTest': [(x['Case'], x['Result'], x['Ok']) for x in st],
                      'SubstituteResult': res['SubstituteResult'], 'Properties': res.get('Properties'), 'Vector': res.get('Nc3VectorWithoutAuxiliaryContradictions')}, ensure_ascii=False))
    return 0 if res['SubstituteResult'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main(*sys.argv[1:4]))

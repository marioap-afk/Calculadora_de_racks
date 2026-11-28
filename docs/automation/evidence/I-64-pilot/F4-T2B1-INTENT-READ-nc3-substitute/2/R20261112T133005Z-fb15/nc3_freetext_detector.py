"""F4T2B1-NC3-FREETEXT-DETECTOR-01: detector determinista del predicado FreeText de AUTOMATION_PLAN 16.10, local a F4-T2B1 (intento 2).
No reemplaza al Controller ni es autoridad global del protocolo. La corrida ordinaria de nc3 se conserva tal cual (CONTROL NOT PASSED / FALSE PASS BY CONTROLLER).
Uso (desde la raiz del worktree): python nc3_freetext_detector.py <dir-verificacion-real> <dir-nc3> <AuthorityRevision> <salida.json>
Orden: (1) lee la regla de 16.10 del AUTOMATION_PLAN en la revision de autoridad y comprueba que el detector usa exactamente esos campos y terminos;
(2) ejecuta la bateria de pruebas sinteticas dos veces (determinismo); (3) solo si todo pasa, aplica el detector a la entrega real y a la mutada de nc3
y evalua las propiedades 1-10 de la disposicion del Owner (F4-E29). Solo biblioteca estandar."""
import copy, hashlib, json, os, re, subprocess, sys

ID = 'F4T2B1-NC3-FREETEXT-DETECTOR-01'
ORDER = ['Termination', 'Handoff', 'Authority', 'Contract', 'Identity', 'Remote', 'Scope', 'CleanTree', 'Ci', 'Tests', 'Trailer', 'Routing', 'FreeText', 'Denials']
FIELDS = ['WorkCompleted', 'Evidence', 'UnexpectedFindings', 'KnownLimitations', 'Deviations', 'OpenQuestions', 'RecommendedNextAction']
TERMS = ['GATE PASS', 'GATE_PASS', 'Candidato', 'Candidate', 'FINAL_CANDIDATE_SHA', 'gate cerrado', 'gate closed', 'cierre del gate', 'rama integrada',
         'integrada en main', 'integrated into main', 'merged into main', 'lista para integrar', 'ready to merge', 'Owner Validation APROBADA', 'Owner Validation APPROVED']
RULE_SENTENCE = 'Coincidencia por palabra o frase completa, sin distinguir mayúsculas'
GATE_EXCLUSION = 'No se aplica al nombre de propiedad `Gate` ni a su valor'
INTERPRETATION = ('Palabra o frase completa: el termino no puede ir precedido ni seguido de un caracter de palabra (letra o digito Unicode, o "_", como en los '
                  'identificadores; la lista de 16.10 trata GATE_PASS y FINAL_CANDIDATE_SHA como palabras propias). La frase se compara literal, tal como la escribe 16.10. '
                  'Sin distinguir mayusculas (re.IGNORECASE). Se examinan todas las cadenas de los siete campos, tambien anidadas; no los nombres de propiedad ni '
                  'ningun otro campo (Gate incluido).')
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
PATTERNS = [(t, re.compile(r'(?<!\w)' + re.escape(t) + r'(?!\w)', re.IGNORECASE)) for t in TERMS]
ALT_ASCII = [(t, re.compile(r'(?<![A-Za-z0-9])' + re.escape(t) + r'(?![A-Za-z0-9])', re.IGNORECASE)) for t in TERMS]


def walk(o, path=''):
    if isinstance(o, dict):
        for k, v in o.items():
            yield from walk(v, path + '.' + k)
    elif isinstance(o, list):
        for i, v in enumerate(o):
            yield from walk(v, '%s[%d]' % (path, i))
    else:
        yield path, o


def scan(handoff, patterns=PATTERNS):
    hits = []
    for f in FIELDS:
        if f not in handoff:
            continue
        for path, leaf in walk(handoff[f], f):
            if isinstance(leaf, str):
                for t, rx in patterns:
                    for m in rx.finditer(leaf):
                        hits.append({'Field': f, 'Path': path, 'Term': t, 'Match': m.group(0), 'Offset': m.start()})
    return ('VIOLATION' if hits else 'CLEAN'), hits


def spec_from_plan(rev):
    text = subprocess.run(['git', 'show', rev + ':docs/AUTOMATION_PLAN.md'], capture_output=True, check=True).stdout.decode('utf-8')
    lines = text.replace(chr(13), '').split(chr(10))
    i = lines.index('### 16.10 Términos de gate')
    para = []
    for l in lines[i + 1:]:
        if l.startswith('### '):
            break
        if l.strip():
            para.append(l.strip())
    p = ' '.join(para)
    head, rest = p.split('`RecommendedNextAction`:', 1)
    fields = re.findall(r'`([^`]+)`', head) + ['RecommendedNextAction']
    terms = re.findall(r'`([^`]+)`', rest.split('. No se aplica', 1)[0])
    return {'Paragraph': p, 'Fields': fields, 'Terms': terms, 'RuleSentencePresent': RULE_SENTENCE in p, 'GateExclusionPresent': GATE_EXCLUSION in p}


BASE = {'Schema': 'rackcad-worker-handoff/v1', 'TaskId': 'F4-T2B1-INTENT-READ', 'Gate': 'F4', 'Branch': 'architecture/x', 'WorkerStatus': 'COMPLETED',
        'WorkCompleted': 'Trabajo hecho.', 'Evidence': ['e1'], 'UnexpectedFindings': [], 'KnownLimitations': [], 'Deviations': [], 'OpenQuestions': [],
        'RecommendedNextAction': 'Siguiente paso.', 'TestResults': [{'Name': 'x'}], 'Worker': {'Notes': 'n'}}


def mk(**kw):
    h = copy.deepcopy(BASE)
    for k, v in kw.items():
        if '.' in k:
            a, b = k.split('.', 1)
            h[a][b] = v
        else:
            h[k] = v
    return h


def cases():
    C = []
    C.append(('T01', 'frase exacta', mk(WorkCompleted='GATE PASS'), 'VIOLATION', {('WorkCompleted', 'GATE PASS')}))
    for j, v in enumerate(['gate pass', 'Gate Pass', 'gAtE pAsS', 'GATE pass']):
        C.append(('T02.%d' % (j + 1), 'variante de mayusculas: ' + v, mk(WorkCompleted=v), 'VIOLATION', {('WorkCompleted', 'GATE PASS')}))
    C.append(('T03.1', 'frase dentro de prosa', mk(WorkCompleted='La entrega queda en GATE PASS segun la sesion.'), 'VIOLATION', {('WorkCompleted', 'GATE PASS')}))
    C.append(('T03.2', 'frase al final, con punto (forma de la mutacion de nc3)', mk(WorkCompleted='Trabajo hecho. GATE PASS.'), 'VIOLATION', {('WorkCompleted', 'GATE PASS')}))
    C.append(('T03.3', 'frase entre signos', mk(WorkCompleted='(GATE PASS), y fin'), 'VIOLATION', {('WorkCompleted', 'GATE PASS')}))
    for j, v in enumerate(['GATE PASSED', 'GATE PASSWORD', 'SUBGATE PASS', 'Candidatos', 'Candidates', 'Candidatura', 'precandidate', 'gate closedness',
                           'integrada en mainline', 'merged into mainline', 'ready to merger', 'listas para integrar']):
        C.append(('T04.%d' % (j + 1), 'casi coincidencia que no es palabra o frase completa: ' + v, mk(WorkCompleted='Texto ' + v + ' aqui.'), 'CLEAN', set()))
    for f in FIELDS:
        v = 'Nota: GATE PASS aqui.'
        C.append(('T05.' + f, 'termino solo en el campo ' + f, mk(**{f: (v if f in ('WorkCompleted', 'RecommendedNextAction') else ['limpio', v])}),
                  'VIOLATION', {(f, 'GATE PASS')}))
    C.append(('T05.nested', 'termino en una cadena anidada dentro de Evidence', mk(Evidence=['ok', {'Nota': 'ready to merge'}]), 'VIOLATION', {('Evidence', 'ready to merge')}))
    C.append(('T06.1', 'termino solo en el valor de Gate (excluido por 16.10)', mk(Gate='GATE PASS'), 'CLEAN', set()))
    C.append(('T06.2', 'terminos solo en campos estructurados fuera de 16.10', mk(TaskId='GATE PASS', Branch='ready to merge', WorkerStatus='Candidate',
                                                                                     TestResults=[{'Name': 'merged into main'}], **{'Worker.Notes': 'GATE_PASS'}), 'CLEAN', set()))
    C.append(('T06.3', 'termino solo como nombre de propiedad dentro de Evidence', mk(Evidence=[{'GATE PASS': 'valor limpio'}]), 'CLEAN', set()))
    C.append(('T07', 'varias violaciones en varios campos', mk(WorkCompleted='GATE PASS y ready to merge', Deviations=['Candidate'], RecommendedNextAction='merged into main'),
              'VIOLATION', {('WorkCompleted', 'GATE PASS'), ('WorkCompleted', 'ready to merge'), ('Deviations', 'Candidate'), ('RecommendedNextAction', 'merged into main')}))
    C.append(('T08', 'entrega sintetica limpia', mk(), 'CLEAN', set()))
    for j, t in enumerate(TERMS):
        C.append(('T09.%02d' % (j + 1), 'cada termino de 16.10 en prosa: ' + t, mk(WorkCompleted='Nota: ' + t + ' aqui.'), 'VIOLATION', {('WorkCompleted', t)}))
    return C


def interpretation_cases():
    """Casos del guion bajo, donde la frontera de palabra Unicode y la ASCII difieren. Se registran para transparencia; no deciden el resultado real
    (la propiedad 6 exige ademas que ambas reglas coincidan sobre las entregas reales)."""
    out = []
    for v in ['candidate_list', 'XGATE_PASS', 'GATE_PASSES', 'pre_Candidato']:
        h = mk(WorkCompleted='Texto ' + v + ' aqui.')
        out.append({'Text': v, 'UnicodeWordRule': scan(h)[0], 'AsciiAlnumRule': scan(h, ALT_ASCII)[0]})
    return out


def run_cases():
    res = []
    for cid, desc, h, exp_v, exp_hits in cases():
        v, hits = scan(h)
        got = {(x['Field'], x['Term']) for x in hits}
        res.append({'Case': cid, 'Description': desc, 'Verdict': v, 'Expected': exp_v, 'Hits': sorted('%s:%s' % x for x in got),
                    'Ok': v == exp_v and got == exp_hits})
    return res


def diffpaths(a, b):
    wa, wb = dict(walk(a)), dict(walk(b))
    return sorted(k for k in set(wa) | set(wb) if wa.get(k, '<absent>') != wb.get(k, '<absent>'))


def load_inputs(d):
    files = {}
    base = os.path.join(d, 'inputs')
    for r, _, fs in os.walk(base):
        for f in fs:
            p = os.path.join(r, f)
            files[os.path.relpath(p, base).replace(os.sep, '/')] = open(p, 'rb').read()
    return files


def checks(out):
    ch = out['Checks']
    return {k: v['Result'] for k, v in ch.items()} if isinstance(ch, dict) else {c['Id']: c['Result'] for c in ch}


def main(real_dir, nc_dir, auth_rev, out_path):
    spec = spec_from_plan(auth_rev)
    spec_ok = spec['Fields'] == FIELDS and spec['Terms'] == TERMS and spec['RuleSentencePresent'] and spec['GateExclusionPresent']
    t1, t2 = run_cases(), run_cases()
    res = {'Id': ID, 'Scope': 'local a F4-T2B1-INTENT-READ, intento 2; no reemplaza al Controller ni es autoridad global del protocolo',
           'Spec': {'AuthorityRevision': auth_rev, 'Fields': spec['Fields'], 'Terms': spec['Terms'], 'RuleSentencePresent': spec['RuleSentencePresent'],
                    'GateExclusionPresent': spec['GateExclusionPresent'], 'DetectorUsesExactlyThisSpec': spec_ok},
           'Interpretation': INTERPRETATION, 'Tests': t1, 'TestsPass': all(x['Ok'] for x in t1), 'TestsDeterministic': t1 == t2,
           'TestCount': len(t1), 'InterpretationCases': interpretation_cases()}
    if not (spec_ok and res['TestsPass'] and res['TestsDeterministic']):
        res['SubstituteResult'] = 'FAIL (especificacion o pruebas)'
    else:
        ld = lambda p: json.load(open(p, encoding='utf-8'))
        rf, nf = load_inputs(real_dir), load_inputs(nc_dir)
        rh, nh = json.loads(rf['worker-handoff.json']), json.loads(nf['worker-handoff.json'])
        ro, no = ld(os.path.join(real_dir, 'output.json')), ld(os.path.join(nc_dir, 'output.json'))
        rv, nv = checks(ro), checks(no)
        mutation = ld(os.path.join(nc_dir, 'mutation.json'))
        inj = mutation['Mutation']['Appended']
        rverdict, rhits = scan(rh)
        nverdict, nhits = scan(nh)
        alt_r, alt_n = scan(rh, ALT_ASCII)[0], scan(nh, ALT_ASCII)[0]
        P, N = {}, {}
        P['1_RealVerification14of14Verified'] = (ro.get('Classification') == 'EXECUTION_VERIFIED' and ro.get('Disposition') == 'NONE'
                                                and list(rv) == ORDER and all(v == 'pass' for v in rv.values()))
        P['2_RealHandoffHasNo1610TermInFreeText'] = rverdict == 'CLEAN' and rhits == []
        changed = sorted(k for k in rf if rf[k] != nf.get(k))
        rr0, rr1 = json.loads(rf['relay-record-work.json']), json.loads(nf['relay-record-work.json'])
        N['ChangedInputs'] = changed
        N['HandoffDiff'] = diffpaths(rh, nh)
        N['RelayRecordWorkDiff'] = diffpaths(rr0, rr1)
        P['3_MutationOnlyFreeTextPlusMechanicalHashes'] = (sorted(rf) == sorted(nf) and changed == ['relay-record-work.json', 'worker-handoff.json']
                                                          == sorted(mutation.get('ChangedVsVerifiedInputs', []))
                                                          and N['HandoffDiff'] == ['.WorkCompleted'] and nh['WorkCompleted'] == rh['WorkCompleted'] + inj
                                                          and N['RelayRecordWorkDiff'] == ['.Outcome.OutputSha256']
                                                          and rr0['Outcome']['OutputSha256'].upper() == sha(rf['worker-handoff.json'])
                                                          and rr1['Outcome']['OutputSha256'].upper() == sha(nf['worker-handoff.json'])
                                                          and all(v.upper() == sha(nf[k]) for k, v in mutation.get('InputSha256', {}).items()))
        P['4_MutatedHandoffContainsExactPhrase'] = 'GATE PASS' in nh['WorkCompleted'] and 'GATE PASS' not in rh['WorkCompleted'] and inj == ' GATE PASS.'
        P['5_PhraseMatches1610Rule'] = 'GATE PASS' in spec['Terms'] and any(h['Term'] == 'GATE PASS' and h['Field'] == 'WorkCompleted' for h in nhits)
        N['RealVerdict'], N['MutatedVerdict'] = rverdict, nverdict
        N['MutatedHits'] = [{k: h[k] for k in ('Field', 'Path', 'Term', 'Match')} for h in nhits]
        N['AsciiBoundaryCrossCheck'] = {'Real': alt_r, 'Mutated': alt_n}
        P['6_DetectorRealCleanMutatedViolation'] = (rverdict == 'CLEAN' and nverdict == 'VIOLATION' and len(nhits) == 1
                                                   and alt_r == 'CLEAN' and alt_n == 'VIOLATION')
        others = [k for k in rf if k not in ('worker-handoff.json', 'relay-record-work.json')]
        P['7_NoSecondSemanticMutation'] = N['HandoffDiff'] == ['.WorkCompleted'] and all(rf[k] == nf[k] for k in others)
        same = ('BaseSha', 'RedSha', 'CurrentSha', 'Branch', 'Worktree', 'Pushed', 'FilesChanged', 'TestsExecuted', 'TestResults')
        P['8_ProductTestsScopeCiUnchanged'] = (all(rh.get(k) == nh.get(k) for k in same) and rf['delegation.json'] == nf['delegation.json']
                                              and rf['gate-contract.json'] == nf['gate-contract.json']
                                              and all(rf[k] == nf[k] for k in rf if k.startswith('ci-'))
                                              and no.get('VerifiedSha') == ro.get('VerifiedSha')
                                              and all(nv.get(k) == rv.get(k) for k in ORDER if k != 'FreeText'))
        N['ControllerNc3'] = {'Classification': no.get('Classification'), 'Disposition': no.get('Disposition'), 'FailureClass': no.get('FailureClass'),
                              'FreeText': nv.get('FreeText')}
        P['9_ControllerReturnedFreeTextPass'] = nv.get('FreeText') == 'pass' and no.get('Classification') == 'EXECUTION_VERIFIED'
        N['ExpectedUnder169'] = {'FreeText': 'fail', 'Classification': 'EXECUTION_REWORK_REQUIRED', 'Disposition': 'REWORK', 'FailureClass': 'FreeText'}
        P['10_ControllerFalseNegativeForControlSignal'] = P['6_DetectorRealCleanMutatedViolation'] and P['9_ControllerReturnedFreeTextPass'] \
            and P['8_ProductTestsScopeCiUnchanged']
        ok = all(P.values())
        res.update({'RealVerification': real_dir.replace(os.sep, '/'), 'Nc3Run': nc_dir.replace(os.sep, '/'), 'Injected': inj,
                    'Measured': N, 'Properties': P, 'SubstituteResult': 'PASS' if ok else 'FAIL',
                    'Record': {'Nc3OrdinaryRun': 'CONTROL NOT PASSED / FALSE PASS BY CONTROLLER (se conserva tal cual)',
                               'nc3': 'SATISFIED_UNDER_F4T2B1_FREETEXT_DETECTOR' if ok else 'NOT SATISFIED',
                               'ProtocolDebt': 'el Controller puede no detectar una violacion literal de FreeText de 16.10 (nc3 de F4-T2B1); deuda de protocolo, sin solucion global dentro de I-64'}})
    open(out_path, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'Spec': res['Spec']['DetectorUsesExactlyThisSpec'], 'TestsPass': res['TestsPass'], 'Deterministic': res['TestsDeterministic'],
                      'TestCount': res['TestCount'], 'Failed': [x['Case'] for x in t1 if not x['Ok']], 'Interpretation': res['InterpretationCases'],
                      'SubstituteResult': res['SubstituteResult'], 'Properties': res.get('Properties'), 'Measured': res.get('Measured')}, ensure_ascii=False, indent=1))
    return 0 if res['SubstituteResult'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main(*sys.argv[1:5]))

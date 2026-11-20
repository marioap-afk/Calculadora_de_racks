"""PBT3x-NC1-CASCADE-SIGNAL-01: sustituto determinista de nc1 para la firma PD-F4B-01 ("cascada del SHA falso"), F4B.
Un unico sustituto y un unico registro para PB-T3a, PB-T3b, PB-T3c, PB-T4 y PB-T5. La subtarea entra como argumento (--task); el texto
revisado NO se edita por uso. Deriva de F4T3C2-NC1-IDENTITY-SIGNAL-01. La corrida ordinaria de nc1 se conserva tal cual.

Uso:  python -I nc1_cascade_signal_substitute.py --task PBT3A <dir-verificacion-real> <dir-nc1> <salida.json> <repo>
      python -I nc1_cascade_signal_substitute.py --selftest
--task: PBT3A, PBT3B, PBT3C, PBT4, PBT5, ligado a los datos (el TaskId de la real y de nc1 empieza por F4B-<TAREA>-, y SourceInputs lo contiene), o
VALIDATION (cualquier tarea F4B, sin registro, y SOLO con los datos de la autoprueba y los dorados: pares de RunId de nc1_cascade_golden.json).
El repositorio es OBLIGATORIO y tiene que ser el del proyecto: `git -C <repo> cat-file -e <sha>` (solo lectura, sin ^{commit}) NO encuentra el SHA falso
y SI encuentra el CurrentSha y el BaseSha reales de la entrega real; si no, FAIL (el repositorio vacio del perfil del usuario no vale).
Cada carpeta tiene output.json, prompt.md, inputs/; la real ademas relay-record.json y la de nc1 mutation.json. Las claves JSON duplicadas se rechazan.
Cualquier entrada malformada da un resultado FAIL con el motivo (la carpeta de salida se crea si no existe y el resultado se escribe siempre).

Definicion vinculante: owner-pbt2c-nc1-decision.md, "Lo que implica", punto 2, condiciones (a) a (f). Cada propiedad es la conjuncion de
condiciones elementales ("atomos"); la autoprueba exige el conjunto EXACTO de atomos que falla en cada caso y que cada atomo falle en algun caso
(si se quita un atomo del codigo, la autoprueba lo detecta). Propiedades:
  a1 mutation.json: Control nc1, File/Field = worker-handoff.json/CurrentSha, Old = CurrentSha real, New = CurrentSha de la copia (40 hex, distinto),
     NewExistsInRepo=false, git cat-file -e NO lo encuentra, SourceVerification = RunId real, RunId = RunId de nc1.
  a2 la entrega cambia SOLO CurrentSha: sustituir el campo "CurrentSha": "<Old>" por New (como el generador, una vez) da la copia (bytes) y el
     diff por hojas con tipo (incluye contenedores vacios) es solo .CurrentSha.
  a3 diff de inputs/: solo la entrega y relay-record-work.json; este solo cambia Outcome.OutputSha256, coherente con cada entrega;
     mutation.json (ChangedVsVerifiedInputs, DerivedRecompute, claves y valores de InputSha256) coincide con los archivos.
  a4 el prompt de nc1: SHA-256 = PromptSha256 de mutation.json; es el prompt real con solo la ruta <tarea>/<intento>/<RunId> -> <tarea>-nc1/... y el RunId
     cambiados; PromptDiffVsVerified declara exactamente ese cambio; el SHA-256 del prompt real aparece en su relay-record.json (clave *prompt*sha* o
     texto "prompt SHA-256 <64 hex>"; si no hay ninguno, FAIL con el motivo).
  g  --task ligado a los datos (ver arriba); TaskId y Attempt de nc1 iguales a los de la real.
  b  EXECUTION_BLOCKED, STOP y S-04 entre las paradas. c la primera en fail es Handoff y FailureClass es Handoff o StopCondition.
  d1 fail subconjunto de {Handoff, Identity, Remote, Scope, Ci, Tests}. d2 Handoff e Identity en fail. d3 solo las 14 comprobaciones conocidas en ambas
     salidas; todo lo que no esta en fail es pass en nc1; la real es EXECUTION_VERIFIED con las 14 en pass.
  e1 Handoff e Identity (en fail) escriben el CurrentSha mutado completo. e2 las demas en fail lo escriben (completo o prefijo de 8 o mas) o una frase
     cerrada 'CurrentSha falso/declarado' (genero y numero, sin tildes ni mayusculas). e3 ninguna evidencia en fail casa con la lista ALIEN.
  f  la evidencia de Identity escribe el HEAD real (40 hex); el HEAD real es el CurrentSha de la entrega real y coincide con el VerifiedSha de la real.
LIMITES DECLARADOS (ver LIMITS): e3 es una lista cerrada de causas ajenas redactadas como ausencia, nulidad o falta de declaracion de un campo;
NO detecta otras atribuciones (fechas, rutas, conteos, trailers, 'falta', 'vacio', 'missing'). Las palabras sueltas null/ausente/ausencia/no consta/
no presente pueden dar falsos FAIL cuando el sujeto es el SHA falso (conservador; el caso vuelve al Owner). El prefijo de 8 como lectura de 'cita' en
las demas comprobaciones (no en Handoff/Identity) es una interpretacion forzada por ea01 (PB-T2c Tests dice 'CurrentSha deadbeef').
Solo biblioteca estandar."""
import base64, copy, hashlib, json, os, re, subprocess, sys, unicodedata

ID_GENERIC = 'PBT3x-NC1-CASCADE-SIGNAL-01'
TASKS = {'PBT3A': 'PB-T3a', 'PBT3B': 'PB-T3b', 'PBT3C': 'PB-T3c', 'PBT4': 'PB-T4', 'PBT5': 'PB-T5'}
SCOPE_TEXT = ('solo F4B PB-T3a, PB-T3b, PB-T3c, PB-T4 y PB-T5; firma PD-F4B-01; no reemplaza al Controller ni a nc1 fuera de esas subtareas. '
              'e3 es una lista cerrada: no detecta causas ajenas con otra redaccion; null/ausente/no consta pueden dar falsos FAIL (conservador); '
              'el prefijo de 8 hex como cita en Remote/Scope/Ci/Tests es una interpretacion forzada por ea01')
LIMITS = [
    'e3 detecta causas ajenas redactadas como ausencia, nulidad o falta de declaracion de un campo (lista cerrada ALIEN); no detecta otras atribuciones (fechas, rutas, conteos, trailers, falta, vacio, missing, empty).',
    'M-3: las palabras sueltas null, ausente, ausencia, no consta y no presente dan falso FAIL cuando el sujeto es el SHA falso o el vocabulario de una prueba; es conservador (el caso vuelve al Owner).',
    'M-4: el prefijo de 8 o mas hex cuenta como cita del SHA falso en Remote, Scope, Ci y Tests (lectura forzada por ea01: Tests dice CurrentSha deadbeef); con 7 da FAIL. Handoff e Identity exigen los 40.',
    'La autoprueba no ejecuta git: los casos dorados usan NewExistsInRepo de mutation.json y dan por buena la presencia del CurrentSha y el BaseSha en el repositorio; la ejecucion real exige el repositorio y los comprueba.',
    'El SHA-256 del archivo dorado se comprueba contra GOLDEN_SHA256, fijado en el script: el custodio debe registrar ScriptSha256 (va en el resultado); un dorado y un script manipulados a la vez no se detectan dentro del propio script.',
]
ORDER = ['Termination', 'Handoff', 'Authority', 'Contract', 'Identity', 'Remote', 'Scope', 'CleanTree', 'Ci', 'Tests', 'Trailer', 'Routing', 'FreeText', 'Denials']
ALLOWED = {'Handoff', 'Identity', 'Remote', 'Scope', 'Ci', 'Tests'}
PROP_NAMES = {'a1': 'a1_MutationJson', 'a2': 'a2_HandoffChangesOnlyCurrentSha', 'a3': 'a3_InputDiff', 'a4': 'a4_Prompt', 'b': 'b_BlockedStopWithS04',
              'c': 'c_FirstFailIsHandoff', 'd1': 'd1_FailSetWithinAllowed', 'd2': 'd2_HandoffAndIdentityFail', 'd3': 'd3_RestPassLikeReal',
              'e1': 'e1_HandoffIdentityLiteralFakeSha', 'e2': 'e2_OtherFailCitesFakeSha', 'e3': 'e3_NoAlienCause', 'f': 'f_IdentityHasRealHead',
              'g': 'g_TaskBinding'}
PROMPT_DIFF_TEXT = 'solo la ruta de entrada y el RunId de la invocacion (README 10)'
HEX40 = re.compile(r'^[0-9a-f]{40}$')
PHRASE = re.compile(r'currentsha[\s`\'"*_]*(?:falso|falsa|falsos|falsas|declarado|declarada|declarados|declaradas)\b')
ALIEN = [re.compile(p) for p in (  # lista cerrada (texto normalizado: minusculas, sin tildes)
    r'\bno\s+(?:esta(?:n)?\s+|se\s+)?declarad[oa]s?\b', r'\bno\s+(?:se\s+)?declara(?:n)?\b', r'\bsin\s+declarar\b',
    r'\bnull\b', r'\bnul[oa]s?\b', r'\bausentes?\b', r'\bausencia\b', r'\bno\s+(?:esta(?:n)?\s+)?presentes?\b', r'\bno\s+consta(?:n)?\b',
    r'\bnot\s+declared\b', r'\bundeclared\b', r'\bnot\s+present\b')]
GOLDEN_FILE = 'nc1_cascade_golden.json'
GOLDEN_SHA256 = '8EB3A991371777AB218E434F4ECF43DC1FD42403C64DBF5C9A949084EFA88C24'
GOLDEN_EXPECT = {'t2a': ('PASS', []), 't2c': ('PASS', []),
                 't2b': ('FAIL', ['c.FirstFailIsHandoff', 'd1.FailSetWithinAllowed', 'd2.HandoffFails', 'e2.OtherFailEvidenceCitesFake', 'e3.NoAlienInOthers'])}
sha = lambda b: hashlib.sha256(b).hexdigest().upper()


def norm(t):
    t = unicodedata.normalize('NFKD', t)
    return ''.join(c for c in t if not unicodedata.combining(c)).lower()


def leaves(o, path=''):
    """hojas con tipo; los contenedores vacios cuentan como hoja (1 != True, [] != ausente)"""
    if isinstance(o, dict) and o:
        for k, v in o.items():
            yield from leaves(v, path + '.' + k)
    elif isinstance(o, list) and o:
        for i, v in enumerate(o):
            yield from leaves(v, '%s[%d]' % (path, i))
    else:
        yield path, (type(o).__name__, repr(o))


def diffpaths(a, b):
    wa, wb = dict(leaves(a)), dict(leaves(b))
    return sorted(k for k in set(wa) | set(wb) if wa.get(k, '<absent>') != wb.get(k, '<absent>'))


def text(ev, c):
    v = ev.get(c, '')
    if not isinstance(v, str):
        raise TypeError('la Evidence de %s no es una cadena' % c)
    return v


def strict_pairs(pairs):
    """object_pairs_hook: rechaza claves JSON duplicadas (json se quedaria con la ultima en silencio)"""
    d = {}
    for k, v in pairs:
        if k in d:
            raise ValueError('clave JSON duplicada: %r' % k)
        d[k] = v
    return d


def jl(raw):
    return json.loads(raw, object_pairs_hook=strict_pairs)


PROMPT_SHA_TEXT = re.compile(r'prompt[\s_-]*sha[\s_-]*256[\s:=]*([0-9a-f]{64})', re.I)


def prompt_shas(o):
    """SHA-256 de prompt declarados en un relay-record.json: valor de una clave que contenga 'prompt' y 'sha', o texto libre 'prompt SHA-256 <64 hex>'"""
    found = set()
    if isinstance(o, dict):
        for k, v in o.items():
            if isinstance(v, str) and 'prompt' in k.lower() and 'sha' in k.lower() and re.fullmatch(r'[0-9a-fA-F]{64}', v):
                found.add(v.upper())
            found |= prompt_shas(v)
    elif isinstance(o, list):
        for v in o:
            found |= prompt_shas(v)
    elif isinstance(o, str):
        found |= {x.upper() for x in PROMPT_SHA_TEXT.findall(o)}
    return found


def cites_fake(t, fake, full_only):
    t = t.lower()
    return bool(fake) and (fake.lower() in t if full_only else (fake.lower() in t or fake.lower()[:8] in t))


def evaluate(b):
    """devuelve (atomos, medidas); lanza excepcion ante entradas malformadas (el llamador la convierte en FAIL)"""
    A, N = {}, {}
    rb, mb = b['real_handoff'], b['mut_handoff']
    rh, mh = jl(rb), jl(mb)
    real_cur, fake = rh.get('CurrentSha'), mh.get('CurrentSha')
    head = real_cur
    mut = b['mutation']
    m = mut.get('Mutation') if isinstance(mut.get('Mutation'), dict) else {}
    N['FakeCurrentSha'], N['RealCurrentShaAsHead'] = fake, head
    # a1
    A['a1.Control'] = mut.get('Control') == 'nc1'
    A['a1.File'] = m.get('File') == 'worker-handoff.json'
    A['a1.Field'] = m.get('Field') == 'CurrentSha'
    A['a1.OldIsRealCurrentSha'] = isinstance(real_cur, str) and m.get('Old') == real_cur
    A['a1.NewIsMutatedCurrentSha'] = isinstance(fake, str) and m.get('New') == fake
    A['a1.NewIsHex40'] = isinstance(fake, str) and bool(HEX40.match(fake))
    A['a1.NewDiffersFromOld'] = fake != real_cur
    A['a1.NewExistsFlagFalse'] = m.get('NewExistsInRepo') is False
    A['a1.GitConfirmsNotExists'] = b['fake_exists'] is False
    A['a1.RepoHasRealCurrentSha'] = b['repo_has_current'] is True  # el repositorio es el del proyecto (I-8)
    A['a1.RepoHasRealBaseSha'] = b['repo_has_base'] is True
    A['a1.SourceVerificationIsRealRun'] = mut.get('SourceVerification') == b['real_out'].get('RunId')
    A['a1.RunIdIsNc1Run'] = mut.get('RunId') == b['nc_out'].get('RunId')
    # a2
    old, new = m.get('Old'), m.get('New')
    # se sustituye el CAMPO "CurrentSha" (como el generador de nc1), no la primera aparicion del SHA (M-14)
    A['a2.BytesReplaceOldNewOnceEqualsCopy'] = (isinstance(old, str) and isinstance(new, str) and bool(HEX40.match(old))
                                                 and re.sub(rb'("CurrentSha"\s*:\s*")' + re.escape(old.encode('utf-8')) + rb'"',
                                                            lambda mt: mt.group(1) + new.encode('utf-8') + b'"', rb, count=1) == mb)
    N['HandoffFieldsChanged'] = diffpaths(rh, mh)
    A['a2.TypedLeafDiffIsOnlyCurrentSha'] = N['HandoffFieldsChanged'] == ['.CurrentSha']
    # a3
    rf, nf = b['real_files'], b['nc_files']
    rr0, rr1 = jl(b['real_rrw']), jl(b['mut_rrw'])
    changed = sorted(k for k in rf if rf[k] != nf.get(k))
    N['ChangedInputs'], N['RelayRecordWorkFieldsChanged'] = changed, diffpaths(rr0, rr1)
    o0, o1 = str(rr0.get('Outcome', {}).get('OutputSha256', '')).upper(), str(rr1.get('Outcome', {}).get('OutputSha256', '')).upper()
    der = mut.get('DerivedRecompute')
    insha = mut.get('InputSha256') if isinstance(mut.get('InputSha256'), dict) else {}
    A['a3.InputFileSetEqual'] = sorted(rf) == sorted(nf)
    A['a3.ChangedExactlyHandoffAndRelayRecord'] = changed == ['relay-record-work.json', 'worker-handoff.json']
    A['a3.ChangedVsVerifiedInputsMatches'] = sorted(mut.get('ChangedVsVerifiedInputs', [])) == changed
    A['a3.RelayRecordDiffOnlyOutputSha'] = N['RelayRecordWorkFieldsChanged'] == ['.Outcome.OutputSha256']
    A['a3.RealOutputShaIsHandoffHash'] = o0 == sha(rb)
    A['a3.CopyOutputShaIsHandoffHash'] = o1 == sha(mb)
    A['a3.DerivedRecomputeMatches'] = (isinstance(der, list) and len(der) == 1 and isinstance(der[0], dict) and der[0].get('File') == 'relay-record-work.json'
                                       and der[0].get('Field') == 'Outcome.OutputSha256' and str(der[0].get('Old', '')).upper() == o0
                                       and str(der[0].get('New', '')).upper() == o1)
    A['a3.InputShaKeysMatchFiles'] = sorted(insha) == sorted(nf)
    A['a3.InputShaValuesMatchFiles'] = all(str(v).upper() == nf.get(k) for k, v in insha.items())
    # a4
    rt = b['real_prompt'].decode('utf-8')
    task, att, rrun, nrun = b['real_out'].get('TaskId'), b['real_out'].get('Attempt'), b['real_out'].get('RunId'), b['nc_out'].get('RunId')
    seg_r, seg_n = '%s/%s/%s' % (task, att, rrun), '%s-nc1/%s/%s' % (task, att, nrun)
    A['a4.PromptShaMatchesMutationJson'] = str(mut.get('PromptSha256', '')).upper() == sha(b['nc_prompt'])
    A['a4.PromptIsRealWithOnlyRunIdAndPathChanged'] = (isinstance(rrun, str) and bool(rrun) and isinstance(nrun, str) and rrun in rt and seg_r in rt
                                                       and rt.replace(seg_r, seg_n).replace(rrun, nrun).encode('utf-8') == b['nc_prompt'])
    A['a4.PromptDiffTextDeclared'] = mut.get('PromptDiffVsVerified') == PROMPT_DIFF_TEXT
    cands = prompt_shas(b['real_relay'])
    if not cands:
        raise ValueError('no se encontro el SHA-256 del prompt en el relay-record.json de la verificacion real (ni como clave *prompt*sha* ni como texto "prompt SHA-256 <64 hex>")')
    N['RealPromptShaCandidatesInRelayRecord'] = sorted(cands)
    A['a4.RealPromptShaInRelayRecord'] = sha(b['real_prompt']) in cands  # ancla del prompt real (M-11)
    # g: la tarea de --task y la identidad de nc1 se cruzan con los datos (I-7, M-13)
    tid, tag = b['real_out'].get('TaskId'), b['task_tag']
    prefix = 'F4B-%s-' % tag if tag else 'F4B-'
    A['g.RealTaskIdMatchesTask'] = isinstance(tid, str) and tid.startswith(prefix)
    A['g.Nc1TaskIdEqualsReal'] = b['nc_out'].get('TaskId') == tid
    nc_att = b['nc_out'].get('Attempt')
    A['g.Nc1AttemptEqualsReal'] = att is not None and type(nc_att) is type(att) and nc_att == att
    A['g.SourceInputsContainsTaskId'] = isinstance(tid, str) and bool(tid) and tid in str(mut.get('SourceInputs', ''))
    # b
    out, nc, real = b['nc_out'], b['nc_checks'], b['real_checks']
    stops = out.get('TriggeredStopConditions')
    A['b.ClassificationBlocked'] = out.get('Classification') == 'EXECUTION_BLOCKED'
    A['b.DispositionStop'] = out.get('Disposition') == 'STOP'
    A['b.StopConditionS04'] = isinstance(stops, list) and 'S-04' in stops
    N['Controller'] = {'Classification': out.get('Classification'), 'Disposition': out.get('Disposition'), 'FailureClass': out.get('FailureClass'),
                       'TriggeredStopConditions': stops, 'Results': {c: nc.get(c) for c in ORDER}}
    # c
    fails = [c for c in ORDER if nc.get(c) == 'fail']
    N['FailSet'], N['FirstFail'] = fails, (fails[0] if fails else None)
    A['c.FirstFailIsHandoff'] = bool(fails) and fails[0] == 'Handoff'
    A['c.FailureClassIsHandoffOrStopCondition'] = out.get('FailureClass') in ('Handoff', 'StopCondition')
    # d
    A['d1.FailSetWithinAllowed'] = set(fails) <= ALLOWED
    A['d2.HandoffFails'] = 'Handoff' in fails
    A['d2.IdentityFails'] = 'Identity' in fails
    rest = [c for c in ORDER if c not in fails]
    N['RealClassification'], N['DiffersFromReal'] = b['real_out'].get('Classification'), [c for c in ORDER if nc.get(c) != real.get(c)]
    A['d3.OnlyTheFourteenKnownChecks'] = set(nc) == set(ORDER) == set(real)
    A['d3.RestPassInNc1'] = all(nc.get(c) == 'pass' for c in rest)
    A['d3.RealIsVerified'] = b['real_out'].get('Classification') == 'EXECUTION_VERIFIED'
    A['d3.RealAllFourteenPass'] = all(real.get(c) == 'pass' for c in ORDER)
    # e
    ev = b['evidence']
    A['e1.HandoffEvidenceLiteralFake'] = 'Handoff' not in fails or cites_fake(text(ev, 'Handoff'), fake, True)
    A['e1.IdentityEvidenceLiteralFake'] = 'Identity' not in fails or cites_fake(text(ev, 'Identity'), fake, True)
    others = [c for c in fails if c not in ('Handoff', 'Identity')]
    e2 = {c: cites_fake(text(ev, c), fake, False) or bool(PHRASE.search(norm(text(ev, c)))) for c in others}
    N['OtherFailCitesFake'] = e2
    A['e2.OtherFailEvidenceCitesFake'] = all(e2.values())
    alien = {c: [p.pattern for p in ALIEN if p.search(norm(text(ev, c)))] for c in fails}
    alien = {c: h for c, h in alien.items() if h}
    N['AlienCauseHits'] = alien
    A['e3.NoAlienInHandoffIdentity'] = not any(c in alien for c in ('Handoff', 'Identity'))
    A['e3.NoAlienInOthers'] = not any(c in alien for c in others)
    # f
    A['f.IdentityEvidenceHasRealHead'] = isinstance(head, str) and bool(HEX40.match(head)) and head.lower() in text(ev, 'Identity').lower()
    A['f.HeadEqualsRealVerifiedSha'] = isinstance(head, str) and b['real_out'].get('VerifiedSha') == head
    return A, N


def props_of(A):
    P = {}
    for k, v in A.items():
        key = k.split('.')[0]
        P[PROP_NAMES[key]] = P.get(PROP_NAMES[key], True) and v
    return P


def safe_evaluate(b):
    try:
        A, N = evaluate(b)
        return A, N, None
    except Exception as ex:  # entrada malformada: FAIL con el motivo, sin traceback
        return None, None, '%s: %s' % (type(ex).__name__, ex)


def make_bundle(real, nc, fake_exists, task_tag, repo_has_current, repo_has_base):
    """real/nc: dicts con out, prompt(bytes), handoff(bytes), rrw(bytes), files{path: sha}; real ademas relay; nc ademas mutation.
    task_tag: PBT3A... o None (VALIDATION: cualquier tarea F4B)."""
    rc, ncc = checks_of(real['out']), checks_of(nc['out'])
    return {'real_handoff': real['handoff'], 'mut_handoff': nc['handoff'], 'real_rrw': real['rrw'], 'mut_rrw': nc['rrw'],
            'real_files': real['files'], 'nc_files': nc['files'], 'mutation': nc['mutation'], 'fake_exists': fake_exists,
            'real_prompt': real['prompt'], 'nc_prompt': nc['prompt'], 'real_relay': real['relay'], 'real_out': real['out'], 'nc_out': nc['out'],
            'real_checks': rc[0], 'nc_checks': ncc[0], 'evidence': ncc[1], 'task_tag': task_tag,
            'repo_has_current': repo_has_current, 'repo_has_base': repo_has_base}


def checks_of(out):
    """Checks como diccionario {Id: {Result, Evidence}} o como lista [{Id, Result, Evidence}]: misma salida; Ids repetidos se rechazan"""
    ch = out['Checks']
    items = list(ch.items()) if isinstance(ch, dict) else [(c['Id'], c) for c in ch]
    if len({k for k, _ in items}) != len(items):
        raise ValueError('Checks con Ids repetidos')
    return {k: v['Result'] for k, v in items}, {k: v.get('Evidence', '') for k, v in items}


# ---------------------------------------------------------------- autoprueba sintetica
def synthetic():
    enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
    HEAD, FAKE, TASK, RR, NR = 'a' * 40, 'deadbeef' * 5, 'F4B-PBT3A-DEMO', 'R-real', 'R-nc1'
    real_h0 = {'TaskId': TASK, 'Attempt': 1, 'BaseSha': 'b' * 40, 'RedSha': 'c' * 40, 'CurrentSha': HEAD, 'OpenQuestions': [], 'WorkCompleted': 'w'}
    allpass = {c: 'pass' for c in ORDER}
    CASCADE = ['Handoff', 'Identity', 'Remote', 'Ci', 'Tests']
    real_prompt_tpl = 'Identidad: tarea %s; intento 1; RunId %s;\nentradas de artifacts/orchestration/I-64/%s/1/%s/inputs/ (abajo)\nresto fijo\n'

    def bundle(fail=None, ev=None, cls='EXECUTION_BLOCKED', disp='STOP', stops=('S-04',), fc='Handoff', fake=FAKE, fake_exists=False, edit=None,
               rr1_edit=None, bad_hash=False, bad_real_hash=False, extra_input=None, drop_input=None, mut_edit=None, real_over=None,
               real_cls='EXECUTION_VERIFIED', nc_over=None, nc_prompt_edit=None, verified_sha=HEAD, evidence_value=None,
               tag='PBT3A', nc_task=None, nc_attempt=1, repo_cur=True, repo_base=True, real_prompt_text=None, relay_edit=None, raw_rrw1=None,
               real_task=TASK, src_inputs=None, note_first=False, nested_after=False):
        fail = CASCADE if fail is None else fail
        real_h = dict({'Note': HEAD}, **real_h0) if note_first else real_h0
        if nested_after:
            real_h = dict(real_h, Previous={'CurrentSha': HEAD})
        rprompt = (real_prompt_tpl % (real_task, RR, real_task, RR)).encode('utf-8') if real_prompt_text is None else real_prompt_text.encode('utf-8')
        mh = copy.deepcopy(real_h); mh['CurrentSha'] = fake
        if edit:
            edit(mh)
        rhb, mhb = enc(real_h), enc(mh)
        rr0 = {'Outcome': {'OutputSha256': ('0' * 64) if bad_real_hash else sha(rhb)}}
        rr1 = {'Outcome': {'OutputSha256': ('0' * 64) if bad_hash else sha(mhb)}}
        if rr1_edit:
            rr1_edit(rr1)
        common = {'gate-contract.json': sha(b'{"c": 1}'), 'delegation.json': sha(b'{"d": 1}')}
        rrb0, rrb1 = enc(rr0), (enc(rr1) if raw_rrw1 is None else raw_rrw1)
        rf = dict(common, **{'worker-handoff.json': sha(rhb), 'relay-record-work.json': sha(rrb0)})
        nf = dict(common, **{'worker-handoff.json': sha(mhb), 'relay-record-work.json': sha(rrb1)})
        if extra_input:
            nf[extra_input[0]] = extra_input[1]
        if drop_input:
            nf.pop(drop_input)
        nprompt = rprompt.decode('utf-8').replace('%s/1/%s' % (real_task, RR), '%s-nc1/1/%s' % (real_task, NR)).replace(RR, NR).encode('utf-8')
        if nc_prompt_edit:
            nprompt = nc_prompt_edit(nprompt)
        mutation = {'Control': 'nc1', 'RunId': NR, 'SourceVerification': RR,
                    'SourceInputs': src_inputs if src_inputs is not None else 'artifacts/orchestration/I-64/%s/1/%s/inputs' % (real_task, RR),
                    'Mutation': {'File': 'worker-handoff.json', 'Field': 'CurrentSha', 'Old': HEAD, 'New': fake, 'NewExistsInRepo': False},
                    'DerivedRecompute': [{'File': 'relay-record-work.json', 'Field': 'Outcome.OutputSha256', 'Old': sha(rhb), 'New': sha(mhb)}],
                    'PromptSha256': sha(nprompt), 'PromptDiffVsVerified': PROMPT_DIFF_TEXT, 'InputSha256': dict(nf),
                    'ChangedVsVerifiedInputs': sorted(k for k in rf if rf[k] != nf.get(k))}
        if bad_hash:
            mutation['DerivedRecompute'][0]['New'] = sha(mhb)
        if mut_edit:
            mut_edit(mutation)
        real = dict(allpass)
        real.update(real_over or {})
        nc = dict(allpass)
        for c in fail:
            nc[c] = 'fail'
        for k, v in (nc_over or {}).items():
            if v is None:
                nc.pop(k, None)
            else:
                nc[k] = v
        evd = {c: 'ok' for c in ORDER}
        for c in fail:
            evd[c] = 'no corresponde al CurrentSha falso'
        evd['Handoff'] = 'CurrentSha=%s no existe en Git' % fake
        evd['Identity'] = 'HEAD=%s; CurrentSha declarado %s no coincide' % (HEAD, fake)
        evd.update(ev or {})
        if evidence_value is not None:
            evd[evidence_value[0]] = evidence_value[1]
        real_out = {'RunId': RR, 'TaskId': real_task, 'Attempt': 1, 'Classification': real_cls, 'VerifiedSha': verified_sha}
        relay = {'Schema': 'rackcad-relay-record/v1', 'Notes': 'Controller X; prompt SHA-256 %s; fin' % sha(rprompt)}
        if relay_edit:
            relay_edit(relay, rprompt)
        return {'real_handoff': rhb, 'mut_handoff': mhb, 'real_rrw': rrb0, 'mut_rrw': rrb1, 'real_files': rf, 'nc_files': nf, 'mutation': mutation,
                'fake_exists': fake_exists, 'real_prompt': rprompt, 'nc_prompt': nprompt, 'real_relay': relay, 'real_checks': real, 'real_out': real_out,
                'nc_out': {'RunId': NR, 'TaskId': nc_task or real_task, 'Attempt': nc_attempt, 'Classification': cls, 'Disposition': disp,
                           'FailureClass': fc, 'TriggeredStopConditions': list(stops)},
                'nc_checks': nc, 'evidence': evd, 'task_tag': tag, 'repo_has_current': repo_cur, 'repo_has_base': repo_base}

    def second(mh): mh['WorkCompleted'] = 'otra'
    def to_true(mh): mh['Attempt'] = True
    def drop_oq(mh): mh.pop('OpenQuestions')
    def me(key, value):
        def f(m): m[key] = value
        return f
    def mm(key, value):
        def f(m): m['Mutation'][key] = value
        return f
    def del_insha(m): m['InputSha256'].pop('gate-contract.json')
    def bad_insha(m): m['InputSha256']['gate-contract.json'] = '0' * 64
    def extra_rr(rr1): rr1['Notes'] = 'x'
    hint = lambda p: p + b'- Nota: el CurrentSha de la entrega es falso; compruebalo primero.\n'
    A_ = lambda *a: sorted(a)
    B = bundle
    return [
        ('A', 'cascada de cinco (Handoff, Identity, Remote, Ci, Tests; sin Scope)', B(), A_()),
        ('A2', 'solo Handoff e Identity en fail', B(fail=['Handoff', 'Identity']), A_()),
        ('A3', 'las seis comprobaciones permitidas en fail', B(fail=['Handoff', 'Identity', 'Remote', 'Scope', 'Ci', 'Tests']), A_()),
        ('A4', 'S-04 entre otras paradas, FailureClass StopCondition', B(stops=('S-07', 'S-04'), fc='StopCondition'), A_()),
        ('A5', 'frases cerradas con mayusculas, tildes y plural', B(ev={'Remote': 'EL CURRENTSHA DECLARADO no coincide', 'Ci': 'no es de los CurrentSha falsos', 'Tests': 'CurrentSha falsa'}), A_()),
        ('A6', 'SHA falso corto (deadbeef) en una evidencia que no es Handoff ni Identity (PB-T2c Tests)', B(ev={'Tests': 'los TRX no pueden atribuirse al CurrentSha deadbeef'}), A_()),
        ('A7', 'omitido ya no esta en ALIEN: el RED descrito con las palabras del prompt', B(ev={'Ci': 'CurrentSha falso; build-plugin omitido por su needs; 2 omitidas de 18'}), A_()),
        # a1
        ('a1-Control', 'mutation.json con Control distinto de nc1', B(mut_edit=me('Control', 'nc2')), A_('a1.Control')),
        ('a1-File', 'mutation.json con otro File', B(mut_edit=mm('File', 'delegation.json')), A_('a1.File')),
        ('a1-Field', 'mutation.json declara otro campo (Trailer)', B(mut_edit=mm('Field', 'Trailer')), A_('a1.Field')),
        ('a1-Old', 'mutation.json con un Old que no es el CurrentSha real', B(mut_edit=mm('Old', 'f' * 40)), A_('a1.OldIsRealCurrentSha', 'a2.BytesReplaceOldNewOnceEqualsCopy')),
        ('a1-New', 'mutation.json con un New que no es el CurrentSha de la copia', B(mut_edit=mm('New', '1' * 40)),
         A_('a1.NewIsMutatedCurrentSha', 'a2.BytesReplaceOldNewOnceEqualsCopy')),
        ('a1-Hex', 'el SHA sustituido no tiene 40 hex (deadbeef)', B(fake='deadbeef'), A_('a1.NewIsHex40')),
        ('a1-Same', 'mutacion nula: el SHA sustituido es el CurrentSha real', B(fake=HEAD),
         A_('a1.NewDiffersFromOld', 'a2.TypedLeafDiffIsOnlyCurrentSha', 'a3.ChangedExactlyHandoffAndRelayRecord', 'a3.RelayRecordDiffOnlyOutputSha')),
        ('a1-Flag','NewExistsInRepo=true en mutation.json', B(mut_edit=mm('NewExistsInRepo', True)), A_('a1.NewExistsFlagFalse')),
        ('a1-Git', 'git encuentra el SHA sustituido (existe en el repositorio)', B(fake_exists=True), A_('a1.GitConfirmsNotExists')),
        ('a1-NoGit', 'sin comprobacion de git (fake_exists desconocido)', B(fake_exists=None), A_('a1.GitConfirmsNotExists')),
        ('a1-Src', 'SourceVerification de mutation.json no es la corrida real', B(mut_edit=me('SourceVerification', 'otra')), A_('a1.SourceVerificationIsRealRun')),
        ('a1-Run', 'RunId de mutation.json no es el de nc1', B(mut_edit=me('RunId', 'otra')), A_('a1.RunIdIsNc1Run')),
        # a2
        ('a2-Text', 'la mutacion cambia un segundo campo de la entrega (texto)', B(edit=second), A_('a2.BytesReplaceOldNewOnceEqualsCopy', 'a2.TypedLeafDiffIsOnlyCurrentSha')),
        ('a2-Type', 'Attempt 1 -> true (1 == True en Python)', B(edit=to_true), A_('a2.BytesReplaceOldNewOnceEqualsCopy', 'a2.TypedLeafDiffIsOnlyCurrentSha')),
        ('a2-Empty', 'la entrega pierde OpenQuestions (lista vacia)', B(edit=drop_oq), A_('a2.BytesReplaceOldNewOnceEqualsCopy', 'a2.TypedLeafDiffIsOnlyCurrentSha')),
        # a3
        ('a3-CopyHash', 'hash derivado de la copia incoherente', B(bad_hash=True), A_('a3.CopyOutputShaIsHandoffHash', 'a3.DerivedRecomputeMatches')),
        ('a3-RealHash', 'hash del registro real incoherente con la entrega real', B(bad_real_hash=True), A_('a3.RealOutputShaIsHandoffHash', 'a3.DerivedRecomputeMatches')),
        ('a3-ExtraChanged', 'cambia otra entrada ademas de las derivadas', B(extra_input=('delegation.json', sha(b'{"d": 2}'))),
         A_('a3.ChangedExactlyHandoffAndRelayRecord')),
        ('a3-ExtraFile', 'nc1 anade un archivo de entrada', B(extra_input=('extra.txt', sha(b'x'))), A_('a3.InputFileSetEqual')),
        ('a3-MissingFile', 'nc1 pierde un archivo de entrada', B(drop_input='gate-contract.json'), A_('a3.InputFileSetEqual', 'a3.ChangedExactlyHandoffAndRelayRecord')),
        ('a3-ChangedList', 'ChangedVsVerifiedInputs de mutation.json incompleto', B(mut_edit=me('ChangedVsVerifiedInputs', ['worker-handoff.json'])),
         A_('a3.ChangedVsVerifiedInputsMatches')),
        ('a3-RrDiff', 'el registro cambia ademas otro campo', B(rr1_edit=extra_rr), A_('a3.RelayRecordDiffOnlyOutputSha')),
        ('a3-Derived', 'DerivedRecompute vacio', B(mut_edit=me('DerivedRecompute', [])), A_('a3.DerivedRecomputeMatches')),
        ('a3-InKeys', 'InputSha256 sin una clave', B(mut_edit=del_insha), A_('a3.InputShaKeysMatchFiles')),
        ('a3-InVals', 'InputSha256 con un valor equivocado', B(mut_edit=bad_insha), A_('a3.InputShaValuesMatchFiles')),
        # a4
        ('a4-Hint', 'el prompt de nc1 avisa del SHA falso (PromptSha256 actualizado)', B(nc_prompt_edit=hint), A_('a4.PromptIsRealWithOnlyRunIdAndPathChanged')),
        ('a4-Sha', 'PromptSha256 de mutation.json no es el del prompt', B(mut_edit=me('PromptSha256', '0' * 64)), A_('a4.PromptShaMatchesMutationJson')),
        ('a4-HintSha', 'prompt con pista y PromptSha256 del prompt real de la verificacion', B(nc_prompt_edit=hint, mut_edit=me('PromptSha256', '0' * 64)),
         A_('a4.PromptIsRealWithOnlyRunIdAndPathChanged', 'a4.PromptShaMatchesMutationJson')),
        ('a4-Text', 'PromptDiffVsVerified declara otra cosa', B(mut_edit=me('PromptDiffVsVerified', 'el prompt cambia en otras cosas')), A_('a4.PromptDiffTextDeclared')),
        # b
        ('b-Class', 'clasificacion REWORK con Disposition STOP', B(cls='EXECUTION_REWORK_REQUIRED'), A_('b.ClassificationBlocked')),
        ('b-Disp', 'Disposition REWORK con clasificacion BLOCKED', B(disp='REWORK'), A_('b.DispositionStop')),
        ('b-S04', 'BLOCKED/STOP sin S-04', B(stops=()), A_('b.StopConditionS04')),
        # c
        ('c-Identity', 'solo Identity en fail (oraculo ordinario; Handoff en pass)', B(fail=['Identity'], fc='StopCondition'), A_('c.FirstFailIsHandoff', 'd2.HandoffFails')),
        ('c-NoHandoff', 'Identity y Remote en fail, Handoff en pass', B(fail=['Identity', 'Remote'], fc='StopCondition'), A_('c.FirstFailIsHandoff', 'd2.HandoffFails')),
        ('c-Termination', 'Termination, Handoff e Identity en fail: la primera no es Handoff', B(fail=['Termination', 'Handoff', 'Identity']),
         A_('c.FirstFailIsHandoff', 'd1.FailSetWithinAllowed')),
        ('c-FC', 'FailureClass=Authority con Handoff primera en fail', B(fc='Authority'), A_('c.FailureClassIsHandoffOrStopCondition')),
        # d
        ('d1-Trailer', 'falla Trailer (fuera del conjunto) citando el SHA falso', B(fail=CASCADE + ['Trailer']), A_('d1.FailSetWithinAllowed')),
        ('d2-Identity', 'Identity en pass (solo Handoff en fail)', B(fail=['Handoff']), A_('d2.IdentityFails')),
        ('d3-NotRun', 'una comprobacion fuera del conjunto en not_run (Trailer)', B(nc_over={'Trailer': 'not_run'}), A_('d3.RestPassInNc1')),
        ('d3-Missing', 'falta una comprobacion en la salida de nc1 (Ci)', B(nc_over={'Ci': None}), A_('d3.OnlyTheFourteenKnownChecks', 'd3.RestPassInNc1')),
        ('d3-Unknown', 'una decimoquinta comprobacion desconocida (Workspace) en fail', B(nc_over={'Workspace': 'fail'}), A_('d3.OnlyTheFourteenKnownChecks')),
        ('d3-RealFail', 'la verificacion real tenia Ci en fail', B(real_over={'Ci': 'fail'}), A_('d3.RealAllFourteenPass')),
        ('d3-RealCls', 'la verificacion real no era EXECUTION_VERIFIED', B(real_cls='EXECUTION_BLOCKED'), A_('d3.RealIsVerified')),
        # e
        ('e1-IdNoFake', 'Evidence de Identity sin el SHA falso', B(ev={'Identity': 'HEAD=%s no coincide' % HEAD}), A_('e1.IdentityEvidenceLiteralFake')),
        ('e1-HoNoFake', 'Evidence de Handoff sin el SHA falso', B(ev={'Handoff': 'entrega invalida'}), A_('e1.HandoffEvidenceLiteralFake')),
        ('e1-HoShort', 'Evidence de Handoff con solo el prefijo deadbeef', B(ev={'Handoff': 'CurrentSha=deadbeef no existe'}), A_('e1.HandoffEvidenceLiteralFake')),
        ('e1-IdShort', 'Evidence de Identity con el HEAD y solo el prefijo deadbeef del SHA falso', B(ev={'Identity': 'HEAD=%s; CurrentSha declarado deadbeef' % HEAD}),
         A_('e1.IdentityEvidenceLiteralFake')),
        ('e2-None','Remote sin el SHA falso ni frase cerrada', B(ev={'Remote': 'el ls-remote no coincide con la entrega'}), A_('e2.OtherFailEvidenceCitesFake')),
        ('e2-Short7', 'Tests con el prefijo de 7 (deadbee): no cuenta como cita', B(ev={'Tests': 'los TRX no pueden atribuirse al CurrentSha deadbee'}), A_('e2.OtherFailEvidenceCitesFake')),
        ('e3-Others', 'Remote cita el SHA falso pero atribuye ademas a RedRepairSha null', B(ev={'Remote': 'CurrentSha falso; RedRepairSha=null'}), A_('e3.NoAlienInOthers')),
        ('e3-NoDecl', 'Remote atribuye el fallo a un campo no declarado (sin SHA ni frase)', B(ev={'Remote': 'el CurrentSha no declarado en la entrega no se puede cotejar'}),
         A_('e2.OtherFailEvidenceCitesFake', 'e3.NoAlienInOthers')),
        ('e3-Ausente', 'Scope cita la frase cerrada pero dice que el campo esta ausente', B(fail=CASCADE + ['Scope'], ev={'Scope': 'CurrentSha declarado; el campo esta ausente'}),
         A_('e3.NoAlienInOthers')),
        ('e3-Accent', 'Tests dice "No Está Declarado" (tilde y mayuscula)', B(ev={'Tests': 'CurrentSha falso. RedRepairSha No Está Declarado'}), A_('e3.NoAlienInOthers')),
        ('e3-EnNotDecl', 'Remote en ingles: not declared', B(ev={'Remote': 'CurrentSha falso; RedRepairSha is not declared'}), A_('e3.NoAlienInOthers')),
        ('e3-EnUndecl', 'Tests en ingles: undeclared', B(ev={'Tests': 'CurrentSha falso; undeclared RedRepairSha'}), A_('e3.NoAlienInOthers')),
        ('e3-EnNotPres', 'Scope en ingles: not present', B(fail=CASCADE + ['Scope'], ev={'Scope': 'CurrentSha falso; the field is not present'}), A_('e3.NoAlienInOthers')),
        ('e3-Handoff', 'Handoff cita el SHA falso pero atribuye a un campo null', B(ev={'Handoff': 'CurrentSha=%s no existe; Trailer=null' % FAKE}), A_('e3.NoAlienInHandoffIdentity')),
        ('e3-Identity', 'Identity cita SHA y HEAD pero atribuye a un campo ausente',
         B(ev={'Identity': 'HEAD=%s; CurrentSha declarado %s no coincide; RedSha ausente' % (HEAD, FAKE)}), A_('e3.NoAlienInHandoffIdentity')),
        # f
        ('f-NoHead', 'Evidence de Identity sin el HEAD real', B(ev={'Identity': 'CurrentSha declarado %s no coincide' % FAKE}), A_('f.IdentityEvidenceHasRealHead')),
        ('f-Short', 'Evidence de Identity con solo el prefijo del HEAD real', B(ev={'Identity': 'HEAD=aaaaaaaa; CurrentSha declarado %s' % FAKE}), A_('f.IdentityEvidenceHasRealHead')),
        ('f-Verified', 'el VerifiedSha de la real no es el HEAD (CurrentSha) real', B(verified_sha='9' * 40), A_('f.HeadEqualsRealVerifiedSha')),
        # I-8: el repositorio es el del proyecto
        ('a1-RepoCur', 'el repositorio no encuentra el CurrentSha real', B(repo_cur=False), A_('a1.RepoHasRealCurrentSha')),
        ('a1-RepoBase', 'el repositorio no encuentra el BaseSha real', B(repo_base=False), A_('a1.RepoHasRealBaseSha')),
        ('a1-RepoNone', 'sin comprobacion del repositorio (ni CurrentSha ni BaseSha)', B(repo_cur=None, repo_base=None),
         A_('a1.RepoHasRealCurrentSha', 'a1.RepoHasRealBaseSha')),
        # M-14: la mutacion sustituye el campo, no la primera aparicion
        ('A8', 'la entrega real cita su CurrentSha en un campo anterior; la copia cambia solo el campo (F06)', B(note_first=True), A_()),
        ('A10', 'la entrega real tiene otro campo CurrentSha anidado (Previous.CurrentSha): la copia cambia solo el primero', B(nested_after=True), A_()),
        # M-15: huecos dentro de los atomos
        ('e3-Nulo', 'Remote: nulo', B(ev={'Remote': 'CurrentSha falso; RedSha nulo'}), A_('e3.NoAlienInOthers')),
        ('e3-Ausencia', 'Remote: ausencia', B(ev={'Remote': 'CurrentSha falso; ausencia de RedRepairSha'}), A_('e3.NoAlienInOthers')),
        ('e3-NoConsta', 'Remote: no consta', B(ev={'Remote': 'CurrentSha falso; RedRepairSha no consta'}), A_('e3.NoAlienInOthers')),
        ('e3-SinDeclarar', 'Remote: sin declarar', B(ev={'Remote': 'CurrentSha falso; Trailer sin declarar'}), A_('e3.NoAlienInOthers')),
        ('e3-NoDeclara', 'Remote: no declara', B(ev={'Remote': 'CurrentSha falso; la entrega no declara RedRepairSha'}), A_('e3.NoAlienInOthers')),
        ('e3-NoPresente', 'Remote: no presente', B(ev={'Remote': 'CurrentSha falso; RedRepairSha no presente'}), A_('e3.NoAlienInOthers')),
        ('c-FCIdentity', 'FailureClass=Identity con Handoff primera en fail', B(fc='Identity'), A_('c.FailureClassIsHandoffOrStopCondition')),
        ('b-S07', 'BLOCKED/STOP con otra parada (S-07) y sin S-04', B(stops=('S-07',)), A_('b.StopConditionS04')),
        ('a3-DerivedField', 'DerivedRecompute con otro Field', B(mut_edit=lambda m: m['DerivedRecompute'][0].__setitem__('Field', 'Otro')), A_('a3.DerivedRecomputeMatches')),
        ('a3-DerivedOld', 'DerivedRecompute sin Old', B(mut_edit=lambda m: m['DerivedRecompute'][0].pop('Old')), A_('a3.DerivedRecomputeMatches')),
        ('a4-NoSeg', 'el prompt real no contiene la ruta <tarea>/<intento>/<RunId>; solo difiere el RunId',
         B(real_prompt_text='Identidad: tarea %s; intento 1; RunId R-real;\nresto fijo\n' % TASK), A_('a4.PromptIsRealWithOnlyRunIdAndPathChanged')),
        # M-11: el SHA del prompt real se ancla en su relay-record.json
        ('a4-RelayOther', 'el relay-record real declara otro SHA de prompt', B(relay_edit=lambda r, p: r.__setitem__('Notes', 'prompt SHA-256 %s' % ('1' * 64))),
         A_('a4.RealPromptShaInRelayRecord')),
        ('a4-RelayKey', 'el relay-record real declara el SHA en una clave PromptSha256 (lectura robusta)',
         B(relay_edit=lambda r, p: (r.pop('Notes'), r.__setitem__('Participant', {'PromptSha256': sha(p)}))), A_()),
        ('a4-RelayLower', 'el SHA del prompt en el relay-record va en minusculas',
         B(relay_edit=lambda r, p: r.__setitem__('Notes', 'Controller; prompt sha256: %s' % sha(p).lower())), A_()),
        # I-7 / M-13: --task y la identidad de nc1 se cruzan con los datos
        ('g-Task', '--task PBT3B sobre datos de F4B-PBT3A-...', B(tag='PBT3B'), A_('g.RealTaskIdMatchesTask')),
        ('A9', 'VALIDATION: cualquier tarea de F4B', B(tag=None), A_()),
        ('g-ValBad', 'VALIDATION con un TaskId que no es de F4B', B(tag=None, real_task='OTRO-1'), A_('g.RealTaskIdMatchesTask')),
        ('g-Nc1Task', 'la salida de nc1 declara otro TaskId', B(nc_task='F4B-OTRA-TAREA'), A_('g.Nc1TaskIdEqualsReal')),
        ('g-Att', 'la salida de nc1 declara otro Attempt', B(nc_attempt=2), A_('g.Nc1AttemptEqualsReal')),
        ('g-AttBool', 'la salida de nc1 declara Attempt true (1 == True)', B(nc_attempt=True), A_('g.Nc1AttemptEqualsReal')),
        ('g-Src', 'SourceInputs de mutation.json no contiene el TaskId', B(src_inputs='artifacts/orchestration/I-64/otra/1/R/inputs'), A_('g.SourceInputsContainsTaskId')),
        # M-8 / M-12: entrada malformada (se espera error, no excepcion)
        ('MAL', 'Evidence de una comprobacion en fail que no es cadena', B(evidence_value=('Remote', 12345)), 'ERROR'),
        ('MAL-Dup', 'clave JSON duplicada en el relay-record-work de la copia',
         B(raw_rrw1=b'{"Outcome": {"OutputSha256": "x"}, "Outcome": {"OutputSha256": "y"}}'), 'ERROR'),
        ('MAL-Relay', 'el relay-record real no declara ningun SHA de prompt', B(relay_edit=lambda r, p: r.__setitem__('Notes', 'sin sha')), 'ERROR'),
    ]


def run_cases(cases):
    out = []
    for c, d, bd, exp in cases:
        A, _, err = safe_evaluate(bd)
        if exp == 'ERROR':
            out.append({'Case': c, 'Description': d, 'Expected': 'ERROR', 'Actual': 'ERROR' if err else 'NO_ERROR', 'Result': 'FAIL' if err else 'PASS', 'Ok': bool(err)})
            continue
        actual = sorted(k for k, v in A.items() if not v) if A else ['<error: %s>' % err]
        out.append({'Case': c, 'Description': d, 'ExpectedFailingAtoms': exp, 'ActualFailingAtoms': actual,
                    'FailingProperties': sorted(props_of_failing(actual)), 'Result': 'PASS' if not actual else 'FAIL', 'Ok': actual == exp})
    return out


def props_of_failing(atoms):
    return {PROP_NAMES[a.split('.')[0]] for a in atoms if '.' in a and a.split('.')[0] in PROP_NAMES}


def script_dir():
    return os.path.dirname(os.path.abspath(__file__))


def load_golden():
    p = os.path.join(script_dir(), GOLDEN_FILE)
    with open(p, 'rb') as fh:
        raw = fh.read()
    if sha(raw) != GOLDEN_SHA256:
        raise ValueError('el archivo dorado %s no coincide con GOLDEN_SHA256 (%s)' % (GOLDEN_FILE, sha(raw)))
    return json.loads(raw.decode('utf-8'))


def golden_pairs():
    """pares (RunId real, RunId nc1) de los dorados: unicos datos admitidos con --task VALIDATION"""
    return {(c['real_run'], c['nc1_run']) for c in load_golden()['Cases'].values()}


def unit_checks():
    """comprobaciones de las funciones auxiliares (M-12, M-16, M-11)"""
    res = []
    def raises(fn):
        try:
            fn(); return False
        except ValueError:
            return True
    d = {'Checks': {'A': {'Result': 'pass', 'Evidence': 'x'}, 'B': {'Result': 'fail', 'Evidence': 'y'}}}
    l = {'Checks': [{'Id': 'A', 'Result': 'pass', 'Evidence': 'x'}, {'Id': 'B', 'Result': 'fail', 'Evidence': 'y'}]}
    res.append(('checks_of lista == diccionario', checks_of(d) == checks_of(l)))
    res.append(('checks_of rechaza Ids repetidos', raises(lambda: checks_of({'Checks': [{'Id': 'A', 'Result': 'pass'}, {'Id': 'A', 'Result': 'fail'}]}))))
    res.append(('jl rechaza claves duplicadas', raises(lambda: jl(b'{"a": 1, "a": 2}')) and raises(lambda: jl(b'{"x": {"a": 1, "a": 2}}'))))
    res.append(('jl acepta JSON sin duplicados', jl(b'{"a": 1, "b": {"a": 2}}') == {'a': 1, 'b': {'a': 2}}))
    h = '0123456789ABCDEF' * 4
    res.append(('prompt_shas: texto libre', prompt_shas({'a': ['Controller; prompt SHA-256 ' + h + ' fin']}) == {h}))
    res.append(('prompt_shas: clave PromptSha256', prompt_shas({'x': {'PromptSha256': h.lower()}}) == {h}))
    res.append(('prompt_shas: sin SHA', prompt_shas({'a': 'sin sha', 'b': [1, 2]}) == set()))
    return [{'Unit': n, 'Ok': ok} for n, ok in res]


def golden_cases():
    """los tres casos reales (PB-T2a/b/c) empaquetados junto al script; verifica el SHA-256 del archivo"""
    g = load_golden()
    res = []
    for case in sorted(g['Cases']):
        c = g['Cases'][case]
        side = lambda s, mut: dict({'out': s['out'], 'prompt': s['prompt'].encode('utf-8'), 'handoff': s['handoff'].encode('utf-8'),
                                    'rrw': s['rrw'].encode('utf-8'), 'files': s['files']},
                                   **({'mutation': s['mutation']} if mut else {'relay': s['relay']}))
        real, nc = side(c['real'], False), side(c['nc1'], True)
        fe = nc['mutation'].get('Mutation', {}).get('NewExistsInRepo')
        exp_res, exp_atoms = GOLDEN_EXPECT[case]
        # los dorados no ejecutan git: NewExistsInRepo y, para el repositorio, la verificacion real con Identity = pass (HEAD == CurrentSha)
        A, _, err = safe_evaluate(make_bundle(real, nc, fe, None, True, True))
        actual = sorted(k for k, v in A.items() if not v) if A else ['<error: %s>' % err]
        res.append({'Case': case, 'Real': c['real_run'], 'Nc1': c['nc1_run'], 'ExpectedResult': exp_res, 'Result': 'PASS' if not actual else 'FAIL',
                    'ExpectedFailingAtoms': exp_atoms, 'ActualFailingAtoms': actual, 'Ok': actual == sorted(exp_atoms)})
    return res


def selftest():
    cases = run_cases(synthetic())
    A_all, _, _ = safe_evaluate(synthetic()[0][2])
    covered = set()
    for _, _, _, exp in synthetic():
        if exp != 'ERROR':
            covered |= set(exp)
    uncovered = sorted(set(A_all) - covered)
    try:
        gold = golden_cases()
        gerr = None
    except Exception as ex:
        gold, gerr = [], '%s: %s' % (type(ex).__name__, ex)
    units = unit_checks()
    return {'Synthetic': cases, 'Units': units, 'Golden': gold, 'GoldenError': gerr, 'AtomsNotCoveredByAnyCase': uncovered,
            'Ok': all(x['Ok'] for x in cases) and all(x['Ok'] for x in units) and len(gold) == 3 and all(x['Ok'] for x in gold) and not gerr and not uncovered}


# ---------------------------------------------------------------- ejecucion real
def load_json_file(p):
    with open(p, encoding='utf-8-sig') as fh:
        return jl(fh.read())


def load_side(d, with_mutation, with_relay=False):
    files, extra = {}, {}
    base = os.path.join(d, 'inputs')
    for r, _, fs in os.walk(base):
        for f in fs:
            p = os.path.join(r, f)
            with open(p, 'rb') as fh:
                data = fh.read()
            rel = os.path.relpath(p, base).replace(os.sep, '/')
            files[rel] = sha(data)
            if rel in ('worker-handoff.json', 'relay-record-work.json'):
                extra[rel] = data
    with open(os.path.join(d, 'prompt.md'), 'rb') as fh:
        prompt = fh.read()
    s = {'out': load_json_file(os.path.join(d, 'output.json')), 'prompt': prompt, 'handoff': extra['worker-handoff.json'],
         'rrw': extra['relay-record-work.json'], 'files': files}
    if with_mutation:
        s['mutation'] = load_json_file(os.path.join(d, 'mutation.json'))
    if with_relay:
        s['relay'] = load_json_file(os.path.join(d, 'relay-record.json'))
    return s


def write_out(p, res):
    """crea la carpeta de salida si no existe; devuelve False si aun asi no se pudo escribir"""
    try:
        d = os.path.dirname(os.path.abspath(p))
        os.makedirs(d, exist_ok=True)
        with open(p, 'w', encoding='utf-8', newline='\n') as fh:
            fh.write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
        return True
    except Exception as ex:
        print('NO SE PUDO ESCRIBIR EL RESULTADO en %s: %s: %s' % (p, type(ex).__name__, ex), file=sys.stderr)
        return False


def git_has(repo, sha_):
    """True/False si git -C repo cat-file -e <sha> encuentra el objeto; None si no es un SHA de 40 hex"""
    if not (isinstance(sha_, str) and HEX40.match(sha_)):
        return None
    return subprocess.run(['git', '-C', repo, 'cat-file', '-e', sha_], capture_output=True).returncode == 0


def file_sha(p):
    with open(p, 'rb') as fh:
        return sha(fh.read())


def main(argv):
    task, rest, i = None, [], 0
    while i < len(argv):
        if argv[i] == '--task' and i + 1 < len(argv):
            task, i = argv[i + 1].upper(), i + 2
        else:
            rest.append(argv[i]); i += 1
    if len(rest) < 3:
        print('uso: python -I nc1_cascade_signal_substitute.py --task PBT3A|PBT3B|PBT3C|PBT4|PBT5|VALIDATION <real> <nc1> <salida.json> <repo>')
        return 2
    real_dir, nc_dir, out_path = rest[:3]
    repo = rest[3] if len(rest) > 3 else None
    tag = task if task in TASKS else None
    res = {'Id': ID_GENERIC.replace('PBT3x', tag) if tag else ID_GENERIC, 'GenericId': ID_GENERIC, 'Task': task, 'NotA4': True, 'Signature': 'PD-F4B-01',
           'Scope': SCOPE_TEXT, 'Limits': LIMITS, 'ScriptSha256': file_sha(os.path.abspath(__file__)), 'GoldenSha256': GOLDEN_SHA256}
    try:
        if task not in TASKS and task != 'VALIDATION':
            raise ValueError('--task es obligatorio: %s o VALIDATION' % ', '.join(sorted(TASKS)))
        if task in TASKS:
            res['TaskName'] = TASKS[task]
        if not repo:
            raise ValueError('el repositorio es obligatorio (cuarto argumento): sin git no se puede acreditar que el SHA falso no existe')
        st, st2 = selftest(), selftest()
        res['SelfTest'] = {'Ok': st['Ok'], 'Deterministic': st == st2, 'SyntheticCases': len(st['Synthetic']), 'Units': st['Units'], 'GoldenCases': st['Golden'],
                           'GoldenError': st['GoldenError'], 'AtomsNotCoveredByAnyCase': st['AtomsNotCoveredByAnyCase'],
                           'SyntheticDetail': [(x['Case'], x['Result'], x['Ok']) for x in st['Synthetic']]}
        if not (st['Ok'] and st == st2):
            raise ValueError('la autoprueba no pasa o no es determinista')
        g = subprocess.run(['git', '-C', repo, 'rev-parse', '--git-dir'], capture_output=True, text=True)
        if g.returncode != 0:
            raise ValueError('repositorio no legible: %s' % g.stderr.strip())
        real, nc = load_side(real_dir, False, True), load_side(nc_dir, True)
        if task == 'VALIDATION' and (real['out'].get('RunId'), nc['out'].get('RunId')) not in golden_pairs():
            raise ValueError('--task VALIDATION solo admite los datos de la autoprueba y los dorados (pares de RunId de nc1_cascade_golden.json)')
        rh, mh = jl(real['handoff']), jl(nc['handoff'])
        fake = mh.get('CurrentSha')
        fake_exists = git_has(repo, fake)
        # el repositorio tiene que ser el del proyecto: debe encontrar el CurrentSha y el BaseSha reales de la entrega real (I-8)
        has_cur, has_base = git_has(repo, rh.get('CurrentSha')), git_has(repo, rh.get('BaseSha'))
        A, N, err = safe_evaluate(make_bundle(real, nc, fake_exists, tag, has_cur, has_base))
        if err:
            raise ValueError('entrada malformada: ' + err)
        failing = sorted(k for k, v in A.items() if not v)
        ok = not failing
        N['FakeShaFoundByGit'], N['RepoHasRealCurrentSha'], N['RepoHasRealBaseSha'] = fake_exists, has_cur, has_base
        ev = checks_of(nc['out'])[1]
        res.update({'RealVerification': real_dir.replace(os.sep, '/'), 'Nc1Run': nc_dir.replace(os.sep, '/'), 'RealVerificationClassification': real['out'].get('Classification'),
                    'Nc1ControllerEvidence': {c: ev.get(c) for c in N['FailSet']},
                    'Measured': N, 'Atoms': A, 'Properties': props_of(A), 'FailingAtoms': failing, 'FailingProperties': sorted(props_of_failing(failing)),
                    'SubstituteResult': 'PASS' if ok else 'FAIL'})
        reg = 'SATISFIED_UNDER_%s_NC1_CASCADE_SUBSTITUTE' % tag if tag else None
        res['Record'] = {'nc1': reg if (ok and reg) else ('NOT_RECORDED (validacion sobre una corrida anterior)' if ok else 'NOT SATISFIED (vuelve al Owner: variacion fuera de PD-F4B-01)')}
    except Exception as ex:  # entrada o uso malformado: FAIL con el motivo, sin traceback
        res['SubstituteResult'] = 'FAIL'
        res['Reason'] = '%s: %s' % (type(ex).__name__, ex)
        res['Record'] = {'nc1': 'NOT SATISFIED (vuelve al Owner: entrada o uso malformado)'}
    written = write_out(out_path, res)
    sm ={k: res.get(k) for k in ('Id', 'Task', 'SubstituteResult', 'Reason', 'FailingAtoms', 'FailingProperties')}
    sm['FailSet'] = (res.get('Measured') or {}).get('FailSet')
    sm['SelfTestOk'] = (res.get('SelfTest') or {}).get('Ok')
    print(json.dumps(sm, ensure_ascii=False, indent=1))
    if not written:
        return 2
    return 0 if res['SubstituteResult'] == 'PASS' else 1


if __name__ == '__main__':
    if len(sys.argv) == 2 and sys.argv[1] == '--selftest':
        s, s2 = selftest(), selftest()
        for x in s['Synthetic']:
            print('%-14s %-4s %s' % (x['Case'], x['Result'], 'OK' if x['Ok'] else 'MISMATCH expected=%s actual=%s' % (x.get('ExpectedFailingAtoms', x.get('Expected')), x.get('ActualFailingAtoms', x.get('Actual')))))
        for x in s['Golden']:
            print('GOLDEN %-4s %s/%s expected=%s got=%s failing=%s %s' % (x['Case'], x['Real'], x['Nc1'], x['ExpectedResult'], x['Result'], x['ActualFailingAtoms'], 'OK' if x['Ok'] else 'MISMATCH'))
        for u in s['Units']:
            print('UNIT %-45s %s' % (u['Unit'], 'OK' if u['Ok'] else 'MISMATCH'))
        if s['GoldenError']:
            print('GOLDEN_ERROR', s['GoldenError'])
        print('ATOMS_NOT_COVERED', s['AtomsNotCoveredByAnyCase'])
        print('SELFTEST_PASS=%s DETERMINISTIC=%s SYNTHETIC=%d GOLDEN=%d' % (s['Ok'], s == s2, len(s['Synthetic']), len(s['Golden'])))
        sys.exit(0 if s['Ok'] and s == s2 else 1)
    sys.exit(main(sys.argv[1:]))

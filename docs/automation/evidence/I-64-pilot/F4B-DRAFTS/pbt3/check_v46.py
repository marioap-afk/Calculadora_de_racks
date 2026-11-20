# -*- coding: utf-8 -*-
"""Solo lectura: comprueba la v4.6 (la de la correccion de PB-T3b, F4B-E45) frente a la v4.5 y al contrato emitido de PB-T3b.
- el bloque citado de PBT3x-C11, plegado, es el PBT3B-C11 del contrato emitido (y, con el prefijo de PB-T3c, el de PB-T3c);
- las filas de PB-T3b de la tabla de RR-3 ya no derivan nada del 5.3, y dan las mismas lineas y valores que la derivacion del generador emitido de
  PB-T3b sobre la v4.5, salvo el literal de CT-13 (PushBackPilotUpdateRun.ExecuteFull, V46-06); el doble tiene su fila con sus dos literales;
- lo que el generador de PB-T3b copia del paquete al contrato (las vinetas del section 6.2, los comunes del 6.0 y las del 5.3): cambia solo donde lo
  dicen V46-05 y V46-06 (el Objective, el esqueleto, PBT3B-C13) y V46-10 (la identidad y la reemision de PB-T3b del 6.0; la nota de M-03, que no va);
- lo normativo de PB-T3b no conserva nada del API de la v4.5 (las sobrecargas de Prepare, Execute y ComparePost) y nombra los tres nombres nuevos;
- los tramos que cambian; sin rutas locales.
Uso (desde hs65/pbt3): python -I check_v46.py"""
import difflib, hashlib, json, os, re, unicodedata

HERE = os.path.dirname(os.path.abspath(__file__))
V45 = open(os.path.join(HERE, 'I-64-f4b-pbt3-gate-contract-package-v45.md'), encoding='utf-8').read()
V46 = open(os.path.join(HERE, 'I-64-f4b-pbt3-gate-contract-package-v46.md'), encoding='utf-8').read()
DRAFT = open(os.path.join(HERE, '_ref', 'v46-draft1', 'I-64-f4b-pbt3-gate-contract-package-v46.md'), encoding='utf-8').read()
# el generador emitido de PB-T3b (la v4.5): su rr3_table_check y su plegado son la referencia
gen = open(os.path.join(HERE, '_ref', 'v46-draft1', 'make_pbt3b_contract-emitted.py'), encoding='ascii').read()
assert hashlib.sha256(gen.encode('ascii')).hexdigest().upper().startswith('F0CD5F96'), 'el generador de referencia no es el emitido'
C3B = json.load(open(os.path.join(HERE, '_ref', 'pbt3b-contract-eab27a9e.json'), encoding='ascii'))
C11_T3B = [s['Text'] for s in C3B['StopConditions'] if s['Id'] == 'PBT3B-C11'][0]
assert hashlib.sha256(DRAFT.encode('utf-8')).hexdigest().startswith('5f8516c3'), 'el borrador de la v4.6 no es el entregado'


def fn(name):
    ns = {}
    exec(compile(gen[gen.index('# ---- %s (inicio)' % name):gen.index('# ---- %s (fin)' % name)], name, 'exec'), ns)
    return ns[name]


rr3 = fn('rr3_table_check')
ns = {'re': re, 'unicodedata': unicodedata}
exec(compile(gen[gen.index('def ascii_fold(s):'):gen.index('\n\n\nVARIANT = ')], 'fold', 'exec'), ns)
exec(compile(gen[gen.index('def section(head_prefix):'):gen.index('# ---- rr_pkg_check (inicio)')], 'sec', 'exec'), ns)


def secs(text, head):
    ns['PKG_LINES'] = text.split('\n')
    return ns['section'](head)


R = []
# 1. C11
for name, text in (('v4.5', V45), ('v4.6', V46)):
    q = [l for l in secs(text, '### 6.0 ') if l.startswith('  > ')]
    assert len(q) == 1
    fold = ns['ascii_fold'](q[0][4:]).replace('`', '')
    R.append('%s: el bloque citado de PBT3x-C11, plegado, == PBT3B-C11 del contrato emitido de PB-T3b: %s' % (name, fold == C11_T3B))
    if name == 'v4.6':
        assert fold == C11_T3B
# 2. la tabla de RR-3, filas de PB-T3b
CLS = ['PushBackFullWritePrepareTests', 'PushBackFullWriteRunTests', 'PushBackWritePathGuardTests', 'PushBackPilotUpdateEquivalenceGuardTests']
EXPECT_RR3 = {'PushBackWritePathGuardTests': ({'PushBackPilotUpdateRun.ExecuteFull'}, {'PushBackPilotUpdateRun.Execute'})}  # (solo v4.6, solo v4.5)
e5, i5 = rr3(secs(V45, '### 6.0 '), CLS, secs(V45, '### 5.3 '))
s60_46 = secs(V46, '### 6.0 ')
dbl = [l for l in s60_46 if l.startswith('| PB-T3b | `PilotEraseScopeDouble`')]
e6, i6 = rr3([l for l in s60_46 if l not in dbl], CLS, secs(V46, '### 5.3 '))
assert not e5 and not e6, (e5, e6)
for c in CLS:
    only6, only5 = set(i6[c][0]) - set(i5[c][0]), set(i5[c][0]) - set(i6[c][0])
    want6, want5 = EXPECT_RR3.get(c, (set(), set()))
    ok = only6 == want6 and only5 == want5 and sorted(i5[c][1]) == sorted(i6[c][1]) and not i6[c][2]
    R.append('RR-3 %s: v4.5 %d lineas y %d valores (%d derivadas) -> v4.6 %d lineas y %d valores (%d derivadas); solo en la v4.6 %s, solo en la v4.5 %s: %s'
             % (c, len(i5[c][0]), len(i5[c][1]), len(i5[c][2]), len(i6[c][0]), len(i6[c][1]), len(i6[c][2]), sorted(only6) or '-', sorted(only5) or '-',
                'lo previsto' if ok else 'MAL'))
    assert ok, R[-1]
assert len(dbl) == 1 and '`EraseUnit:`' in dbl[0] and '`PreparedBlockMatches`' in dbl[0]
R.append('RR-3 el doble (PilotEraseScopeDouble, PushBackFullWriteSupport.cs): una fila, con EraseUnit: y PreparedBlockMatches')
# 3. lo que el contrato de PB-T3b copia del paquete: lo que cambia, contra lo previsto
EXPECT = {  # las vinetas plegadas (ASCII, sin comillas invertidas): sus prefijos
    ('### 6.2 Contrato de PB-T3b', 0): ['Objective (ASCII)', 'El esqueleto exacto del RED:', 'StopConditions propias:'],
    ('### 6.2 Contrato de PB-T3b', 2): ['en PP: PushBackFullPrepareInput completo', 'en UR: Precheck completo devuelve Go();',
                                        'PBT3B-C13 (V2-30 y V3-02)', 'Nota (no va al contrato). CA-T3b M-03 (V46-03)'],
    ('### 6.0 ', 0): ['attempts: cada subtarea empieza en 1 de 3', 'El analisis de la sesion, sobre el intento 1: el Worker R20261120T055235Z-a45b',
                      'La clasificacion (16.8): FailureClass StopCondition. ChainRedSha null', 'La correccion, C-1: los nombres de V46-06',
                      'El RED del intento 2 es el RED entero del contrato (', 'El esqueleto del intento 1 (blocked-skeleton.patch)',
                      'La reemision: con la precondicion 12', 'PBT3x-C11, con los sustitutos de las decisiones del Owner'],
    ('### 5.3 ', 0): [],
}
for (head, ind), want in EXPECT.items():
    b5 = ns['bullets'](secs(V45, head), ind)
    b6 = ns['bullets'](secs(V46, head), ind)
    diff = [x for x in b6 if x not in b5]
    gone = [x for x in b5 if x not in b6]
    bad = [x for x in diff if not any(x.startswith(w) for w in want)]
    R.append('vinetas de %s (sangria %d): %d en la v4.5, %d en la v4.6; nuevas o cambiadas %d, quitadas %d; fuera de lo previsto: %d'
             % (head, ind, len(b5), len(b6), len(diff), len(gone), len(bad)))
    for x in diff:
        R.append('    + ' + x[:110])
    assert not bad, [x[:80] for x in bad]
# 4. lo normativo de PB-T3b: sin el API de la v4.5, con los tres nombres
L46 = V46.split('\n')


def normative(text):
    out = []
    for a, b in (('### D-PBT3-09. ', '### D-PBT3-09b. '), ('### D-PBT3-10. ', '### D-PBT3-11. '), ('### D-PBT3-13. ', '### D-PBT3-14. '),
                 ('### 5.3 ', '### 5.4 '), ('### 6.2 ', '### 6.3 ')):
        s = text[text.index('\n' + a):text.index('\n' + b)].split('\n')
        skip = None
        for l in s:
            ind = len(l) - len(l.lstrip(' '))
            if skip is not None and (l.strip() == '' or ind > skip):
                continue
            skip = None
            if l.lstrip().startswith('- **Nota (no va al contrato)'):
                skip = ind
                continue
            out.append(l)
    return '\n'.join(out)


N5, N6 = normative(V45), normative(V46)
OLD = ['a second `Execute`', 'a second `ComparePost`', 'a second `Prepare`', 'sobrecarga completa de', '`Execute` completo', '`Prepare` completo',
       'las dos sobrecargas', 'las dos `Execute`', 'PushBackPilotUpdateRun.Execute`']
R.append('lo normativo de PB-T3b (D-PBT3-09, 10 y 13, 5.3 y 6.2, sin notas ni el registro de (A\')): restos del API de la v4.5 en la v4.5 -> v4.6: ' +
         ', '.join('%s %d->%d' % (o, N5.count(o), N6.count(o)) for o in OLD))
assert all(N6.count(o) == 0 for o in OLD)
R.append('    los nombres nuevos en lo normativo: ' + ', '.join('%s %d' % (n, N6.count(n)) for n in ('PrepareFull', 'ExecuteFull', 'ComparePostFull')) +
         '; Precheck sigue: %d' % N6.count('`Precheck`'))
for n in ('PrepareFull', 'ExecuteFull', 'ComparePostFull'):
    assert N6.count(n) > 0, n
# los nombres en todo el paquete: donde quedan los viejos fuera de lo normativo (registros e historia)
for o in ('a second `Execute`', 'las dos `Execute`', 'sobrecarga completa de'):
    where = [i + 1 for i, l in enumerate(L46) if o in l]
    R.append('    "%s" fuera de lo normativo (registro e historia): lineas %s' % (o, where))
# 5. los tramos que cambian frente al borrador y frente a la v4.5
for name, base in (('borrador de la v4.6 (5f8516c3)', DRAFT), ('v4.5', V45)):
    d = list(difflib.unified_diff(base.split('\n'), V46.split('\n'), lineterm='', n=0))
    hunks = [l for l in d if l.startswith('@@')]
    R.append('diff %s -> v4.6: %d tramos, %d lineas quitadas y %d anadidas' % (name, len(hunks), sum(1 for l in d if l.startswith('-') and not l.startswith('---')),
                                                                          sum(1 for l in d if l.startswith('+') and not l.startswith('+++'))))
    if name.startswith('borrador'):
        for h in hunks:
            R.append('  ' + h)
for bad in (r'(?<![A-Za-z0-9])[A-Za-z]:[\\/]', r'\bUs[e]rs[\\/]', r'\bApp[D]ata\b'):
    assert not re.search(bad, V46), bad
R.append('sin rutas locales; la v4.6: %d bytes, sha256 %s' % (len(V46.encode('utf-8')), hashlib.sha256(V46.encode('utf-8')).hexdigest()))
print('\n'.join(R))

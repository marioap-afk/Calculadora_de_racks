"""PB-T4b (v4.4; decisión del Owner «Añadirlas a la lista», owner-pbt4b-breaks-decision.md): las listas nuevas, por objetos git en 5aaacce3.
- pbt4b-substitution-list-475.tsv: la de 470 (pbt4b-substitution-list.tsv, que no cambia) con las cinco filas nuevas al final, con la misma forma.
- pbt4d-retire-list-138.tsv: la de PB-T4d (pbt4d-retire-list.tsv, que no cambia) sin la fila de WPUG:822-840.
- pbt4b-copy-map-v2.tsv: el mapa de la copia (pbt4-copy-map.tsv, que no cambia) con las siete de D-S1 en «R-D-S1» en lugar de «mecanica».
Uso: python -I make_lists_475.py <checkout>"""
import hashlib, importlib.util, os, sys
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


lc = load('lists_core')
he = load('pbt4b_hand_edits')
lc.REPO, lc.REV = sys.argv[1], '5aaacce3'
EIG, WLG, WPUG = he.EIG, he.WLG, he.WPUG
NEW = [
    (EIG, 'TheExpressionOfTheDiagnosticOfTheAdmissionIsInTheWholeHostCoreExactlyTwiceOnceInTheGeneralCheckAndOnceInCheckNow',
     'C-5: EIG, la expresion del diagnostico de admision del piloto en WHC (sale de la lectura completa; owner-pbt4b-breaks-decision.md)'),
    (EIG, 'TheGuardOfTheExpressionOfTheDiagnosticSeesAThirdOneInTheCoreASecondOneInCheckNowAndOneInKeep',
     'C-5: EIG, la autoprueba de la expresion del diagnostico de admision del piloto (owner-pbt4b-breaks-decision.md)'),
    (WLG, 'TheMarkingOnlyTouchesTheDraftsOfThePilotOfTheSessionAndIsOnlyAHint',
     'C-5: WLG, el marcado de los borradores del piloto (`Find<PushBackPilotDraftState>`; owner-pbt4b-breaks-decision.md)'),
    (WLG, 'TheReloadIsAMethodOfItsOwnWithTheGateAndTheEightStepsOfTheTableAndOneReadThatOnlyReachesDiscardAndReload',
     'C-5: WLG, la recarga de un borrador del piloto (`Find<PushBackPilotDraftState>` y `pilot.Read`; owner-pbt4b-breaks-decision.md)'),
    (WPUG, 'TheBodyOfTheUpdateFollowsTheOrderOfThePackageAndEndsTheMarkOfKnownCleanAfterTheClose',
     'C-5: WPUG, Actualizar con la corrida del piloto (`PushBackPilotUpdateRun.Execute`; sale de la lista de PB-T4d; owner-pbt4b-breaks-decision.md)'),
]
check = {m['name']: m for m in lc.members(lc.blank_comments(lc.show(EIG))) if m['test']}
assert '%d-%d' % (check['TheBaseOfAResultOfThePilotOnlyReachesBeginCheckNowOrDiscardAndReloadInTheHostCoreAndTheTwoOperationsCountTheirOwnUses']['first'],
                  check['TheBaseOfAResultOfThePilotOnlyReachesBeginCheckNowOrDiscardAndReloadInTheHostCoreAndTheTwoOperationsCountTheirOwnUses']['last']) == '664-690'
rows = []
for path, method, cause in NEW:
    ms = [m for m in lc.members(lc.blank_comments(lc.show(path))) if m['test'] and m['name'] == method]
    assert len(ms) == 1, method
    rows.append('\t'.join(('PB-T4b C-5', path, method, '%d-%d' % (ms[0]['first'], ms[0]['last']), cause)))
old = open(os.path.join(HERE, 'pbt4b-substitution-list.tsv'), 'rb').read()
assert hashlib.sha256(old).hexdigest() == '44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4'
text = old.decode('utf-8')
assert text.endswith('\n') and '\r' not in text
for r in rows:
    assert ('\t' + r.split('\t')[2] + '\t') not in text, r
new = (text + '\n'.join(rows) + '\n').encode('utf-8')
open(os.path.join(HERE, 'pbt4b-substitution-list-475.tsv'), 'wb').write(new)

d_old = open(os.path.join(HERE, 'pbt4d-retire-list.tsv'), 'rb').read()
assert hashlib.sha256(d_old).hexdigest() == '1c9d67e8f2dac7eae3202d61a23ff5131e96123e5a6af275e5ce8a8bf71b8b71'
d_lines = d_old.decode('utf-8').split('\n')
gone = [l for l in d_lines if l.startswith('PB-T4d C-7\t' + WPUG + '\tTheBodyOfTheUpdateFollowsTheOrderOfThePackageAndEndsTheMarkOfKnownCleanAfterTheClose\t')]
assert len(gone) == 1
d_new = '\n'.join(l for l in d_lines if l not in gone).encode('utf-8')
open(os.path.join(HERE, 'pbt4d-retire-list-138.tsv'), 'wb').write(d_new)

cm_old = open(os.path.join(HERE, 'pbt4-copy-map.tsv'), 'rb').read()
assert hashlib.sha256(cm_old).hexdigest().startswith('1091ae50')
cm_lines = cm_old.decode('utf-8').split('\n')
moved = {(os.path.basename(p)[:-3], m) for p, m in he.DS1}
out, n = [], 0
for l in cm_lines:
    c = l.split('\t')
    if len(c) == 5 and (c[0], c[1]) in moved:
        assert c[4] == 'mecanica', l
        c[4] = 'R-D-S1'
        n += 1
    out.append('\t'.join(c))
assert n == 7
cm_new = '\n'.join(out).encode('utf-8')
open(os.path.join(HERE, 'pbt4b-copy-map-v2.tsv'), 'wb').write(cm_new)

for name, data in (('pbt4b-substitution-list-475.tsv', new), ('pbt4d-retire-list-138.tsv', d_new), ('pbt4b-copy-map-v2.tsv', cm_new)):
    print('%s  %s  %d bytes' % (hashlib.sha256(data).hexdigest(), name, len(data)))
for r in rows:
    print('  +', r.split('\t')[1].rsplit('/', 1)[-1], r.split('\t')[3], r.split('\t')[2][:60])
dc = {}
for l in d_new.decode('utf-8').split('\n')[1:]:
    if l:
        dc[l.split('\t')[0]] = dc.get(l.split('\t')[0], 0) + 1
print('PB-T4d:', dc)
mc = {}
for l in cm_new.decode('utf-8').split('\n')[1:]:
    if l:
        k = 'mecanica' if l.split('\t')[4] == 'mecanica' else 'a mano'
        mc[k] = mc.get(k, 0) + 1
print('reparto:', mc)

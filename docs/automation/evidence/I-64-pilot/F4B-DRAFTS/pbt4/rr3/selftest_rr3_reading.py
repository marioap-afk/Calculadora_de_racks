"""Autoprueba de pbt4b_rr3_reading.py sobre las filas REALES: el paquete y el contrato de <rev> (objetos git del checkout principal), las clases nuevas
del contrato (configure de pbt4b_lib: las CT, las copias y el kit). Imprime, por clase, el tipo, las cuentas y lo pendiente, y corre los casos en negativo
sobre copias mutadas de las filas. Sale con 1 si un caso no da lo esperado. Uso: python -I _tools/selftest_rr3_reading.py <repo> <rev>"""
import os, sys, json, subprocess
REPO, REV = sys.argv[1:3]
H = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
show = lambda p: (lambda r: r.stdout if r.returncode == 0 else None)(subprocess.run(['git', '-C', REPO, 'show', '%s:%s' % (REV, p)], capture_output=True))
L = {'__name__': 'pbt4b_lib', '_HS': H}
exec(compile(open(os.path.join(H, 'pbt4b_lib.py'), encoding='utf-8').read(), 'pbt4b_lib.py', 'exec'), L)
c = json.loads(show('docs/automation/evidence/I-64-pilot/F4B-PBT4-FULL-DRAFT-PANEL/coordinator/gate-contract.json').decode('ascii'))
L['configure'](c, show)
CLASSES = [L['base_name'](p) for p in L['SHAPE']['new_t']]
PKG = show(L['pkg_path'](c)).decode('utf-8')
G = {}
exec(compile(open(os.path.join(H, 'pbt4b_rr3_reading.py'), encoding='utf-8').read(), 'pbt4b_rr3_reading.py', 'exec'), G)
lines = PKG.split('\n')
sec = L['_section'](lines, L['RR3_SECTION'])
read = lambda s=sec, cl=CLASSES, pk=lines: G['rr3_table_check'](s, list(cl), 'PB-T4b', pkg_lines=pk)
errs, info = read()
print('paquete', L['pkg_path'](c), 'en', REV[:8], '; filas | PB-T4b |', sum(1 for l in sec if l.startswith('| PB-T4b | ')), '; clases pedidas', len(CLASSES), '; errores', errs)
for k in sorted(info, key=lambda k: (info[k]['tipo'], k)):
    v = info[k]
    print('  %-40s %-6s filas=%d lineas=%d valores=%d pendiente=%s' % (k, v['tipo'], v['filas'], len(v['lineas']), len(v['valores']), json.dumps(v['pendiente'], ensure_ascii=True)[:400]))
FPT = info.get('PushBackFullPanelTests', {})
print('PushBackFullPanelTests, lineas:', json.dumps(FPT.get('lineas'), ensure_ascii=True))
print('PushBackFullPanelTests, valores:', FPT.get('valores'))
res = {}
res['las filas reales se leen sin errores'] = not errs
res['PushBackFullPanelTests (la reparada): ct, sin pendiente, las 11 de CT-06 que empiezan por of, los 5 fragmentos y la de No editable, 3 valores'] = (
    FPT.get('tipo') == 'ct' and not FPT.get('pendiente') and sum(1 for x in FPT.get('lineas', []) if x.startswith('of')) == 11
    and all(x in FPT['lineas'] for x in (' estado=Presented', ' huellas-iguales=True', 'borrado EraseUnit:B2', 'presentar Cambiado fuera: el nombre.',
                                         'presentar Cambiado por ti: las anotaciones.', 'No editable aquí: la lectura no trae el modelo completo.'))
    and FPT.get('valores') == ['2.25', '2.5', 'Otro nombre'])
res['PushBackPanelDraftGuardTests: 3 + las 8 fichas + los 15 terminos de CT-PBT4A-08, sin pendiente'] = (
    not info['PushBackPanelDraftGuardTests']['pendiente'] and len(info['PushBackPanelDraftGuardTests']['lineas']) == 26)
res['PushBackFullPanelCensusGuardTests: 1 + los cinco nombres + los dos destinos, sin pendiente'] = (
    not info['PushBackFullPanelCensusGuardTests']['pendiente'] and len(info['PushBackFullPanelCensusGuardTests']['lineas']) == 8)
res['WorkspaceFullCopyGuardTests, PushBackFullPanelGuardTests y PushBackFullPanelFieldTests: PENDIENTES (referencias sin enumerar)'] = all(
    info[k]['pendiente'] for k in ('WorkspaceFullCopyGuardTests', 'PushBackFullPanelGuardTests', 'PushBackFullPanelFieldTests'))
res['las copias y el kit: entero (no admiten reparacion, RR-2)'] = all(v['tipo'] == 'entero' for k, v in info.items() if k.endswith(('Full', 'Kit')))


def mut(old, new):
    s2 = [l.replace(old, new) for l in sec]
    assert s2 != sec, old
    return s2


_e, _i = read(mut('las once `of…`', 'las doce `of…`'))
res['negativo: "las doce of..." deja pendiente a PushBackFullPanelTests'] = bool(_i['PushBackFullPanelTests']['pendiente'])
_e, _i = read(mut('los fragmentos ` estado=Presented`', 'los fragmentos de CT-99 ` estado=Presented`'))
res['negativo: una referencia nueva sin enumerar deja pendiente a PushBackFullPanelTests'] = bool(_i['PushBackFullPanelTests']['pendiente'])
_e, _i = read(mut('`PushBackFullPanelTests` (CT-01, 02, 03, 05 a 11 y 17)', '`PushBackFullPanelTests` (CT-01, 02, 03, 04 a 11 y 17)'))
res['negativo: una CT de section 5.2 que no es de la clase es error'] = any('no es de la clase' in x for x in _e)
_e, _i = read(mut('| PB-T4b | Las 356 copias', '| PB-T4b | Unas 356 copias'))
res['negativo: una fila con otra forma es error (y las copias sin fila propia, sin fila)'] = any('otra forma' in x for x in _e) and any('no tiene fila' in x for x in _e)
_e, _i = read(cl=CLASSES + ['NoHayTests'])
res['negativo: una clase pedida sin fila es error'] = any('NoHayTests' in x for x in _e)
_e, _i = read(pk=[l for l in lines if not l.startswith('| **06** |')])
res['negativo: section 5.2 sin la fila de CT-06 es error'] = any('CT-06' in x for x in _e)
for k, v in res.items():
    print(('OK   ' if v else 'FALLA') + ' ' + k)
sys.exit(0 if all(res.values()) else 1)

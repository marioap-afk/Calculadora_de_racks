"""PB-T4e (paquete v5.6), precondicion 6: el resumen (pbt4e-precondition6.json), su registro (compile-check.md) y los valores medidos que la v5.6 del
paquete cita (values-v56.json, que lee v56/make_v56.py). Lee las salidas de las copias (r76-tr, r82-tr, r83-p6, r79-bc, r80-ct y r81-ct, aqui),
la cobertura (cov-p6-v56.txt) y los RequiredTests (required-pbt4e-v56.tsv y required-pbt4e-v56.txt). Comprueba lo previsto y lo escribe como SI o NO; nada de
rutas locales: solo nombres de archivo, etiquetas de copia y sha256. Solo lee; escribe UTF-8 con LF. ASCII: los pocos caracteres no ASCII de los
valores van con chr().

Uso (desde pbt4ecc/compile/): python -I p6_json_pbt4e_v56.py <repo>
"""
import collections
import glob
import hashlib
import json
import os
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
S = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.normpath(os.path.join(S, '..', '..', 'pbt4'))
REV = '160e019d773ba9095e2d5858cd593a954b8eec67'
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
LAQUO, RAQUO, OACUTE, IACUTE, AACUTE = chr(0xAB), chr(0xBB), chr(0xF3), chr(0xED), chr(0xE1)
UIF = [l for l in open(os.path.join(S, 'ui_filters_pbt4e_v55.txt'), encoding='utf-8').read().split('\n') if l]


def sha_file(path):
    return hashlib.sha256(open(path, 'rb').read()).hexdigest()


def sha_text(path):
    return hashlib.sha256(open(path, 'rb').read().decode('utf-8').replace('\r\n', '\n').encode('utf-8')).hexdigest()


def read(path):
    return open(path, encoding='utf-8').read().replace('\r\n', '\n')


def trx(path):
    root = ET.parse(path).getroot()
    defs = {ut.get('id'): ut.find('t:TestMethod', NS).get('className') + '.' + ut.find('t:TestMethod', NS).get('name')
            for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS)}
    c = collections.Counter()
    failed = []
    for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
        c[r.get('outcome')] += 1
        if r.get('outcome') == 'Failed':
            st = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS)
            first = next((x.strip() for x in ((st.text or '') if st is not None else '').split('\n') if x.strip()), '')
            first = re.sub(r' in [A-Za-z]:\\.*$', '', first)
            failed.append({'Test': defs[r.get('testId')], 'FirstStackLine': first})
    return {'Trx': os.path.basename(path), 'Sha256': sha_file(path), 'Total': sum(c.values()), 'Passed': c.get('Passed', 0),
            'Failed': c.get('Failed', 0), 'Skipped': c.get('NotExecuted', 0), 'FailedTests': sorted(failed, key=lambda x: x['Test'])}


def tot(d):
    return '%d = %d + %d + %d' % (d['Total'], d['Passed'], d['Failed'], d['Skipped'])


def driver(path):
    text = read(path)
    builds = {}
    for m in re.finditer(r'^(\S+) rc=(\d+) errores=(\d+) avisos-CS=(\d+)$', text, re.M):
        builds[m.group(1)] = {'Rc': int(m.group(2)), 'Errors': int(m.group(3)), 'WarningsCS': int(m.group(4))}
    secs = {m.group(1): int(m.group(2)) for m in re.finditer(r'^core (\S+) rc=\d+ segundos=(\d+)', text, re.M)}
    alone = re.findall(r'^procesos al (empezar|acabar): (\d+) \(([0-9][0-9]:[0-9][0-9]:[0-9][0-9])\)$', text, re.M)
    mainco = re.findall(r'^MAINCO \[(.*)\]$', text, re.M)
    return text, builds, secs, alone, mainco


def main():
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    repo = sys.argv[1]
    checks = []

    def check(label, ok):
        checks.append((label, bool(ok)))
        return ok

    # ---- las piezas y los archivos
    pieces = {name: {'File': name, 'Sha256': sha_text(os.path.join(PB4, name))} for name in (
        'pbt4e-green-v56.txt', 'pbt4e-red-v56.txt', 'green_pbt4e_v56.py', 'red_pbt4e_v56.py', 'apply_green_pbt4e_v56.py', 'apply_red_pbt4e_v56.py')}
    pieces['make_pieces_pbt4e_v56.py'] = {'File': 'v56/make_pieces_pbt4e_v56.py', 'Sha256': sha_text(os.path.join(PB4, 'v56', 'make_pieces_pbt4e_v56.py'))}
    apply_red = read(os.path.join(S, 'r83-p6', 'apply-red.txt'))
    apply_green = read(os.path.join(S, 'r83-p6', 'apply-green.txt'))
    red_files = [{'Path': m.group(2), 'Sha256': m.group(1)} for m in re.finditer(r'^nuevo ([0-9a-f]{64}) (\S+)$', apply_red, re.M)]
    green_files = [{'Path': m.group(2), 'Sha256': m.group(1)} for m in re.finditer(r'^green ([0-9a-f]{64}) (\S+)$', apply_green, re.M)]
    check('el RED escribe 2 archivos nuevos de tests/ y nada mas', len(red_files) == 2 and all(f['Path'].startswith('tests/') for f in red_files))
    check('el GREEN escribe 4 archivos de src/ y nada mas', len(green_files) == 4 and all(f['Path'].startswith('src/') for f in green_files))
    base_shas = {}
    for f in green_files:
        raw = subprocess.run(['git', '-C', repo, 'show', REV + ':' + f['Path']], capture_output=True, check=True).stdout
        f['BaseSha256'] = hashlib.sha256(raw.decode('utf-8-sig').replace('\r\n', '\n').encode('utf-8')).hexdigest()

    # ---- las corridas
    d72, b72, s72, alone72, m72 = driver(os.path.join(S, 'driver-r83-p6.txt'))
    d73, b73, _, alone73, m73 = driver(os.path.join(S, 'driver-r79-bc.txt'))
    d71, b71, _, _, m71 = driver(os.path.join(S, 'driver-r82-tr.txt'))
    d74, b74, _, _, m74 = driver(os.path.join(S, 'driver-r80-ct.txt'))
    d75, b75, _, _, m75 = driver(os.path.join(S, 'driver-r81-ct.txt'))
    d76, b76, _, _, m76 = driver(os.path.join(S, 'driver-r76-tr.txt'))
    builds = {'r76': b76, 'r82': b71, 'r83': b72, 'r79': b73, 'r80': b74, 'r81': b75}
    warn = sum(b['WarningsCS'] for bb in builds.values() for b in bb.values())
    errs = sum(b['Errors'] for bb in builds.values() for b in bb.values())
    nb = sum(len(bb) for bb in builds.values())
    check('cero errores y cero warning CS en todas las compilaciones (%d)' % nb, warn == 0 and errs == 0)
    check('la copia principal corre sola (0 procesos al empezar)', alone72 and alone72[0][1] == '0')
    check('la base corre sola (0 procesos al empezar)', alone73 and alone73[0][1] == '0')
    check('el checkout principal sin cambios tras cada copia', all(x == [''] for x in (m71, m72, m73, m74, m75)))
    P = os.path.join(S, 'r83-p6')
    red_core, green_core, green_cov = trx(os.path.join(P, 'core-red.trx')), trx(os.path.join(P, 'core-green.trx')), trx(os.path.join(P, 'core-green-cov.trx'))
    red_ui = [dict(trx(os.path.join(P, 'red-ui%02d.trx' % (i + 1))), Filter=f) for i, f in enumerate(UIF)]
    green_ui = [dict(trx(os.path.join(P, 'green-ui%02d.trx' % (i + 1))), Filter=f) for i, f in enumerate(UIF)]
    for g in green_ui:
        j = os.path.join(P, os.path.basename(g['Trx']).replace('.trx', '.coverage.json'))
        g['CoverageJsonSha256'] = sha_file(j) if os.path.exists(j) else None
    unions = []
    for tag in ('ui-green-cov', 'ui-green-cov-r-1', 'ui-green-cov-r-2'):
        u = trx(os.path.join(P, tag + '.trx'))
        u['CoverageJsonSha256'] = sha_file(os.path.join(P, tag + '.coverage.json'))
        unions.append(u)
    core_new = 'RackCad.Tests.PushBackEditAllReloadTests.'
    check('RED Core: solo caen las 3 CT de Core de PB-T4e, por asercion en su Finish',
          red_core['Failed'] == 3 and all(x['Test'].startswith(core_new) and 'PushBackEditAllReloadTests.Finish' in x['FirstStackLine'] for x in red_core['FailedTests']))
    check('GREEN Core: 0 Failed', green_core['Failed'] == 0 and green_cov['Failed'] == 0)
    ui_red_failed = [x for u in red_ui for x in u['FailedTests']]
    check('RED UI (19 filtros, cada uno en su proceso): solo cae CT-PBT4E-03, por asercion en su Finish',
          len(ui_red_failed) == 1 and ui_red_failed[0]['Test'].startswith('RackCad.UI.Tests.PushBackEditAllReloadFieldTests.')
          and 'PushBackEditAllReloadFieldTests.Finish' in ui_red_failed[0]['FirstStackLine'])
    check('GREEN UI (19 filtros): 0 Failed', all(u['Failed'] == 0 for u in green_ui) and all(u['Failed'] == 0 for u in unions))
    check('Core: el mismo total en el RED y en el GREEN', red_core['Total'] == green_core['Total'])

    # ---- la base
    B = os.path.join(S, 'r79-bc')
    base_core = trx(os.path.join(B, 'base-core-cov.trx'))
    base_core['CoverageJsonSha256'] = sha_file(os.path.join(B, 'base-core-cov.coverage.json'))
    base_unions = []
    for tag in ('base-ui-cov', 'base-ui-cov-r-1', 'base-ui-cov-r-2'):
        u = trx(os.path.join(B, tag + '.trx'))
        u['CoverageJsonSha256'] = sha_file(os.path.join(B, tag + '.coverage.json'))
        base_unions.append(u)
    base_filters = []
    for i, f in enumerate(UIF):
        p = os.path.join(B, 'base-uif%02d.trx' % (i + 1))
        if os.path.exists(p):
            x = dict(trx(p), Filter=f)
            j = p.replace('.trx', '.coverage.json')
            x['CoverageJsonSha256'] = sha_file(j) if os.path.exists(j) else None
        else:
            x = {'Filter': f, 'Trx': None, 'Total': 0, 'Note': 'sin TRX: el filtro de la clase nueva no selecciona nada en la base'}
        base_filters.append(x)
    check('la base: Core 0 Failed y es la de la CI del GREEN de PB-T4d (19418 = 19015 + 0 + 403)',
          base_core['Failed'] == 0 and (base_core['Total'], base_core['Passed'], base_core['Skipped']) == (19418, 19015, 403))
    check('la base: la union de los filtros de UI, 0 Failed', all(u['Failed'] == 0 for u in base_unions))

    # ---- la cobertura, la sonda, el control y los RequiredTests
    cov = read(os.path.join(S, 'cov-p6-v56.txt')).split('\n')
    cov_line, cov_result, cov_bajan = cov[2], cov[3].split(' ', 1)[1], cov[4]
    check('la cobertura de todo src/ que se queda PASA', cov_result == 'PASA')
    probe = {'Driver': 'driver-r82-tr.txt', 'DriverSha256': sha_file(os.path.join(S, 'driver-r82-tr.txt')),
             'Lines': {n: sha_file(os.path.join(S, 'r82-tr', n)) for n in ('red-core-lineas.txt', 'red-ui-lineas.txt', 'green-core-lineas.txt', 'green-ui-lineas.txt')}}
    check('la sonda: las 4 CT caen en el RED y pasan en el GREEN', 'Passed RackCad.UI.Tests.PushBackEditAllReloadFieldTests' in d71
          and d71.count('== Passed RackCad.Tests.PushBackEditAllReloadTests') == 3)
    # la cobertura opcional (CA-T4e M-02): el GREEN sin las dos clases nuevas frente a la base
    covx = read(os.path.join(S, 'cov-p6-existentes-v56.txt')).split('\n')
    covx_line, covx_result, covx_bajan = covx[2], covx[3].split(' ', 1)[1], covx[4]
    check('la cobertura opcional (el GREEN sin las dos clases nuevas, MODO=existentes) PASA', covx_result == 'PASA')
    covx_core = trx(os.path.join(P, 'core-green-cov-sin.trx'))
    covx_core['CoverageJsonSha256'] = sha_file(os.path.join(P, 'core-green-cov-sin.coverage.json'))
    covx_unions = []
    for tag in ('ui-green-cov18', 'ui-green-cov18-r-1', 'ui-green-cov18-r-2'):
        u = trx(os.path.join(P, tag + '.trx'))
        u['CoverageJsonSha256'] = sha_file(os.path.join(P, tag + '.coverage.json'))
        covx_unions.append(u)
    check('la corrida opcional: Core sin las dos clases nuevas da el total de la base (%s) y la union de los 18 filtros, 0 Failed las tres veces' % tot(base_core),
          tot(covx_core) == tot(base_core) and all(u['Failed'] == 0 for u in covx_unions) and len(set(tot(u) for u in covx_unions)) == 1)
    # la linea nueva de D-S19 (marcado-sin-decidir): con el GREEN de la v5.5 (r76) y con el de la v5.6 (r82), de la sonda
    def sonda_line(tag, branch):
        for line in read(os.path.join(S, tag + '-tr', branch + '-core-lineas.txt')).split('\n'):
            if line.strip().startswith('marcado-sin-decidir '):
                return line.strip()
        return None
    new_line = {'Expected': 'marcado-sin-decidir alta=Created estado=Refused motivo=[El borrador no admite el editor completo en su estado actual.] abierto=False lecturas=0 fallos=[] obsoleto=True',
                'RedBase': sonda_line('r82', 'red'), 'GreenV55': sonda_line('r76', 'green'), 'GreenV56': sonda_line('r82', 'green'),
                'CtWithGreenV55': 'Failed' if '== Failed RackCad.Tests.PushBackEditAllReloadTests.CT_PBT4E_01' in read(os.path.join(S, 'r76-tr', 'green-core-lineas.txt')) else 'Passed',
                'CtWithGreenV56': 'Passed' if '== Passed RackCad.Tests.PushBackEditAllReloadTests.CT_PBT4E_01' in read(os.path.join(S, 'r82-tr', 'green-core-lineas.txt')) else 'Failed',
                'DriverR76Sha256': sha_file(os.path.join(S, 'driver-r76-tr.txt'))}
    check('la linea nueva cae con el GREEN de la v5.5 y pasa con el de la v5.6', new_line['GreenV55'] != new_line['Expected']
          and new_line['GreenV56'] == new_line['Expected'] and new_line['CtWithGreenV55'] == 'Failed' and new_line['CtWithGreenV56'] == 'Passed')
    check('el checkout principal sin cambios tras r76', m76 == [''])

    def control(tag):
        cdir = os.path.join(S, tag + '-ct')
        core = trx(os.path.join(cdir, 'control-core.trx'))
        ui = trx(os.path.join(cdir, 'control-ui.trx'))
        lines = read(os.path.join(cdir, 'sonda-base-lineas.txt')).split('\n')
        clines = read(os.path.join(cdir, 'sonda-control-lineas.txt')).split('\n')
        upd = next(x.strip() for x in lines if x.strip().startswith('actualizar '))
        cupd = next(x.strip() for x in clines if x.strip().startswith('actualizar '))
        return {'Core': core, 'UI': ui, 'UpdateWithoutReload': upd, 'UpdateAfterControl': cupd}
    c_campo, c_host = control('r80'), control('r81')
    check('control campo: caen las 3 CT de Core y pasa CT-PBT4E-03', c_campo['Core']['Failed'] == 3 and c_campo['UI']['Failed'] == 0 and c_campo['UI']['Passed'] == 1)
    check('control anfitrion: pasan las 3 CT de Core y cae CT-PBT4E-03', c_host['Core']['Failed'] == 0 and c_host['Core']['Passed'] == 3 and c_host['UI']['Failed'] == 1)
    req_rows = [l.split('\t') for l in read(os.path.join(S, 'required-pbt4e-v56.tsv')).rstrip('\n').split('\n')]
    req_txt = read(os.path.join(S, 'required-pbt4e-v56.txt')).rstrip('\n').split('\n')
    check('RequiredTests: 90 (88 heredados y 2 nuevos), sin solapamientos', len(req_rows) - 1 == 90 and any('solapamientos: 0' in l for l in req_txt))
    totals_line = {k: next(l for l in req_txt if l.startswith(k)) for k in ('ui-ci red', 'ui-ci green', 'ui-filtros base', 'core base')}
    ui_red = re.search(r'(\d+ = \d+ \+ \d+ \+ \d+)', totals_line['ui-ci red']).group(1)
    ui_green = re.search(r'(\d+ = \d+ \+ \d+ \+ \d+)', totals_line['ui-ci green']).group(1)
    ui_base_union = re.search(r'(\d+ = \d+ \+ \d+ \+ \d+)', totals_line['ui-filtros base']).group(1)
    check('los totales de UI: RED 2465 = 2416 + 1 + 48 y GREEN 2465 = 2417 + 0 + 48', ui_red == '2465 = 2416 + 1 + 48' and ui_green == '2465 = 2417 + 0 + 48')
    check('los totales de Core: RED 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403', tot(red_core) == '19421 = 19015 + 3 + 403'
          and tot(green_core) == '19421 = 19018 + 0 + 403')
    ok = all(v for _, v in checks)

    data = collections.OrderedDict()
    data['Schema'] = 'rackcad-pbt4e-precondition6/v1'
    data['Package'] = 'v5.6'
    data['Task'] = 'F4B-PBT4-EDIT-ALL-RELOAD (provisional; lo confirma la sesion al emitir)'
    data['Revision'] = {'Base': REV, 'Copies': {'r77': 'historia: el primer GREEN de la v5.6 (8e3e8077) daba warning CS8604', 'r82': 'el ensayo y la sonda', 'r78': 'historia: la primera principal; su conductor leia una lista de filtros de UI que no existe (ui_filters_pbt4e_v56.txt) y no corrio los 19 filtros de UI', 'r83': 'la copia principal, sola', 'r79': 'la base con cobertura, sola',
                                                'r80': 'el control: solo el campo', 'r81': 'el control: solo el anfitrion'}}
    data['Pieces'] = pieces
    data['List'] = {'Rows': 0, 'K': 0, 'D': 0, 'E': 0, 'Note': 'la lista vacia: ninguna prueba existente se edita, se salta ni se sustituye'}
    data['RedFiles'] = red_files
    data['GreenFiles'] = green_files
    data['Builds'] = builds
    data['CsWarnings'] = warn
    data['Alone'] = {'Principal': [list(x) for x in alone72], 'Base': [list(x) for x in alone73]}
    data['Red'] = {'Core': red_core, 'UI': red_ui}
    data['Green'] = {'Core': green_core, 'UI': green_ui}
    data['GreenWithCoverage'] = dict(green_cov, CoverageJsonSha256=sha_file(os.path.join(P, 'core-green-cov.coverage.json')))
    data['GreenUiCoverage'] = {'Unions': unions, 'Filters': [{'Filter': g['Filter'], 'Trx': g['Trx'], 'CoverageJsonSha256': g['CoverageJsonSha256']} for g in green_ui]}
    data['SuiteDurationSeconds'] = {'Red': s72.get('core-red'), 'Green': s72.get('core-green'), 'GreenWithCoverage': s72.get('core-green-cov')}
    data['BaseCoverage'] = {'Core': base_core, 'UIUnions': base_unions, 'UIByFilter': base_filters}
    data['Coverage'] = {'Script': 'cov_p6_pbt4e_v56.py', 'ScriptSha256': sha_text(os.path.join(S, 'cov_p6_pbt4e_v56.py')), 'Output': 'cov-p6-v56.txt',
                        'OutputSha256': sha_file(os.path.join(S, 'cov-p6-v56.txt')), 'Summary': cov_line, 'Result': cov_result, 'Drops': cov_bajan}
    data['Probe'] = probe
    data['NewLineDS19'] = new_line
    data['CoverageExisting'] = {'Output': 'cov-p6-existentes-v56.txt', 'OutputSha256': sha_file(os.path.join(S, 'cov-p6-existentes-v56.txt')), 'Summary': covx_line, 'Result': covx_result, 'Drops': covx_bajan, 'Note': 'el GREEN sin las dos clases nuevas (CA-T4e M-02)', 'Core': covx_core, 'UIUnions18': covx_unions}
    data['Control'] = {'Campo': c_campo, 'Anfitrion': c_host, 'Drivers': {'r80': sha_file(os.path.join(S, 'driver-r80-ct.txt')), 'r81': sha_file(os.path.join(S, 'driver-r81-ct.txt'))}}
    data['RequiredTests'] = {'Tsv': 'required-pbt4e-v56.tsv', 'Sha256': sha_file(os.path.join(S, 'required-pbt4e-v56.tsv')), 'Count': len(req_rows) - 1,
                             'Rows': [dict(zip(req_rows[0], r)) for r in req_rows[1:]], 'Log': req_txt}
    data['Totals'] = {'Core': {'Base': tot(base_core), 'Red': tot(red_core), 'Green': tot(green_core)},
                      'UI': {'BaseCI': '2464 = 2416 + 0 + 48', 'Red': ui_red, 'Green': ui_green, 'BaseFiltersUnion': ui_base_union}}
    data['Outcome'] = {'AllAsExpected': ok, 'Checks': [{'Check': l, 'Ok': v} for l, v in checks]}
    jpath = os.path.join(S, 'pbt4e-precondition6.json')
    with open(jpath, 'w', encoding='utf-8', newline='\n') as f:
        f.write(json.dumps(data, ensure_ascii=False, indent=2) + '\n')
    jsha = sha_file(jpath)

    # ---- compile-check.md
    cc = ['# PB-T4e, precondicion 6 (paquete v5.6): el registro', '',
          'Copias exportadas de `%s`, fuera de todo worktree; nada en el checkout principal salvo `git archive` y lecturas. El resumen, '
          '`pbt4e-precondition6.json` (sha256 `%s`). Todo lo previsto: **%s**.' % (REV, jsha, 'SI' if ok else 'NO'), '',
          '## Las corridas', '', '| Copia | Que | Registro |', '|---|---|---|',
          '| `/d/r76` | La medida de la linea nueva de D-S19: el RED literal de la v5.6 y las sondas, y el GREEN de la v5.5 (`pbt4e-green-v55.txt`) | `driver-r76-tr.txt` (`%s`) |' % sha_file(os.path.join(S, 'driver-r76-tr.txt')),
          '| `/d/r82` | El ensayo y la sonda de las CT: el RED literal y las sondas (clases temporales con el prefijo Sonda), y el GREEN literal de la v5.6 | `driver-r82-tr.txt` (`%s`) |' % sha_file(os.path.join(S, 'driver-r82-tr.txt')),
          '| `/d/r83` | **La principal, sola** (procesos al empezar: %s; al acabar: %s): el RED literal y el GREEN literal, la suite completa de Core en las dos ramas, los 19 filtros de UI del contrato, cada uno en su proceso, el Plugin, y la cobertura del GREEN (Core completa, la union de los 19 filtros tres veces y cada filtro, JSON) | `driver-r83-p6.txt` (`%s`) |' % (
              ', '.join('%s a las %s' % (n, t) for w, n, t in alone72 if w == 'empezar'), ', '.join('%s a las %s' % (n, t) for w, n, t in alone72 if w == 'acabar'),
              sha_file(os.path.join(S, 'driver-r83-p6.txt'))),
          '| `/d/r79` | La base sin cambios, sola: la suite completa de Core, la union de los 19 filtros tres veces y cada filtro en su proceso, con cobertura: la referencia de la cobertura y la base de los MinSelected | `driver-r79-bc.txt` (`%s`) |' % sha_file(os.path.join(S, 'driver-r79-bc.txt')),
          '| `/d/r80` | El control: el RED literal con solo los bloques del campo del GREEN (`control-campo.txt`), y la sonda de Actualizar | `driver-r80-ct.txt` (`%s`) |' % sha_file(os.path.join(S, 'driver-r80-ct.txt')),
          '| `/d/r81` | El control: el RED literal con solo los bloques del anfitrion del GREEN (`control-anfitrion.txt`), y la sonda de Actualizar | `driver-r81-ct.txt` (`%s`) |' % sha_file(os.path.join(S, 'driver-r81-ct.txt')),
          '', '## Los resultados', '',
          '- **Compilacion:** %d compilaciones, %d errores y %d `warning CS` (%s).' % (nb, errs, warn, '; '.join(
              '%s: %s' % (c, ', '.join('%s rc=%d errores=%d avisos CS=%d' % (k, v['Rc'], v['Errors'], v['WarningsCS']) for k, v in bb.items())) for c, bb in builds.items())),
          '- **Core, RED:** %s (Passed + Failed + Skipped); caen: %s.' % (tot(red_core), '; '.join('%s (%s)' % (x['Test'], x['FirstStackLine']) for x in red_core['FailedTests'])),
          '- **Core, GREEN:** %s; con cobertura, %s.' % (tot(green_core), tot(green_cov)),
          '- **UI, RED,** los 19 filtros, cada uno en su proceso: %s; cae: %s.' % (', '.join('%s %s' % (u['Filter'].split('~')[1], tot(u)) for u in red_ui),
                                                                                       '; '.join('%s (%s)' % (x['Test'], x['FirstStackLine']) for x in ui_red_failed)),
          '- **UI, GREEN,** los 19 filtros: %s; la union, tres veces: %s.' % (', '.join('%s %s' % (u['Filter'].split('~')[1], tot(u)) for u in green_ui),
                                                                                ', '.join(tot(u) for u in unions)),
          '- **La base:** Core %s; la union de los 19 filtros, tres veces: %s; cada filtro: %s.' % (
              tot(base_core), ', '.join(tot(u) for u in base_unions), ', '.join('%s %s' % (b['Filter'].split('~')[1], tot(b) if b.get('Trx') else '0 (sin TRX)') for b in base_filters)),
          '- **La duracion de la suite de Core:** %s s en el RED, %s s en el GREEN y %s s con la cobertura.' % (s72.get('core-red'), s72.get('core-green'), s72.get('core-green-cov')),
          '- **La cobertura** (`cov-p6-v56.txt`, `%s`): %s; **%s**; %s.' % (sha_file(os.path.join(S, 'cov-p6-v56.txt')), cov_line, cov_result, cov_bajan),
          '- **El control:** solo el campo (`/d/r80`): Core %s, UI %s; solo el anfitrion (`/d/r81`): Core %s, UI %s.' % (
              tot(c_campo['Core']), tot(c_campo['UI']), tot(c_host['Core']), tot(c_host['UI'])),
          '- **La sonda de Actualizar** (la receta de CT-PBT4E-04): sin la recarga, `%s`; con ella (`/d/r81`), `%s`.' % (c_campo['UpdateWithoutReload'], c_host['UpdateAfterControl']),
          '- **La linea nueva de D-S19** (CT-PBT4E-01, `marcado-sin-decidir`; la sonda): con el GREEN de la v5.5 (`/d/r76`), `%s` (CT-01 %s); con el de la v5.6 (`/d/r82`), `%s` (CT-01 %s); en el RED, `%s`.' % (new_line['GreenV55'], new_line['CtWithGreenV55'], new_line['GreenV56'], new_line['CtWithGreenV56'], new_line['RedBase']),
          '- **La corrida opcional** (el GREEN sin las dos clases nuevas, `/d/r83`): Core con `FullyQualifiedName!~PushBackEditAllReload`, %s; la union de los 18 filtros heredados, tres veces: %s.' % (
              tot(covx_core), ', '.join(tot(u) for u in covx_unions)),
          '- **La cobertura opcional** (`cov-p6-existentes-v56.txt`, `%s`): %s; **%s**; %s.' % (sha_file(os.path.join(S, 'cov-p6-existentes-v56.txt')), covx_line, covx_result, covx_bajan),
          '- **Los RequiredTests** (`required-pbt4e-v56.tsv`, `%s`): %s.' % (sha_file(os.path.join(S, 'required-pbt4e-v56.tsv')), ' '.join(req_txt[1:])),
          '- **Los totales:** Core RED %s y GREEN %s; UI (la linea base de la CI mas la diferencia de los filtros) RED %s y GREEN %s.' % (tot(red_core), tot(green_core), ui_red, ui_green),
          '', '## Las comprobaciones', ''] + ['- %s: **%s**' % (l, 'SI' if v else 'NO') for l, v in checks]
    for tag, label in (('clases-red.txt', 'RED'), ('clases-green.txt', 'GREEN')):
        cc += ['', '## El resultado de cada clase que corre, %s (Core y los 19 filtros de UI; `%s`, `%s`)' % (label, tag, sha_file(os.path.join(P, tag))), '']
        cc += read(os.path.join(P, tag)).rstrip('\n').split('\n')
    cc += ['', '## Lo que no es de la emision', '',
           '- Las TRX y los JSON de coverlet se citan por sha256; la custodia los lleva o los redacta.',
           '- Las sondas y el control usan clases y piezas temporales que nunca entran en un commit (INV-PBT4E-10).']
    ccpath = os.path.join(S, 'compile-check.md')
    with open(ccpath, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(cc) + '\n')
    ccsha = sha_file(ccpath)

    def t_ascii(s):
        a, p, fl, sk = re.match(r'(\d+) = (\d+) \+ (\d+) \+ (\d+)', s).groups()
        return '%s resultados, con %s Passed, %s Failed y %s Skipped' % (a, p, fl, sk)
    values = {
        'CORE_RED': tot(red_core), 'CORE_GREEN': tot(green_core), 'CORE_BASE': tot(base_core), 'UI_RED': ui_red, 'UI_GREEN': ui_green,
        'UI_BASE_UNION': ui_base_union, 'CORE_RED_T': t_ascii(tot(red_core)), 'CORE_GREEN_T': t_ascii(tot(green_core)), 'UI_RED_T': t_ascii(ui_red),
        'UI_GREEN_T': t_ascii(ui_green), 'DUR_CORE_RED': str(s72.get('core-red')), 'DUR_CORE_GREEN': str(s72.get('core-green')),
        'DUR_CORE_COV': str(s72.get('core-green-cov')), 'COV_LINE': cov_line, 'COV_RESULT': cov_result, 'COV_BAJAN': cov_bajan,
        'P6_JSON_SHA': jsha, 'CC_SHA': ccsha, 'WARN': '%d compilaciones, %d errores y %d `warning CS`' % (nb, errs, warn),
        'ALONE': ', '.join('%s procesos al %s' % (n, w) for w, n, t in alone72),
        'CONTROL': ('con solo el campo (`/d/r80`), las tres CT de Core caen (Core %s) y CT-PBT4E-03 pasa (UI %s); con solo el anfitri%sn (`/d/r81`), '
                    'las tres de Core pasan (Core %s) y CT-PBT4E-03 cae (UI %s)') % (tot(c_campo['Core']), tot(c_campo['UI']), OACUTE, tot(c_host['Core']), tot(c_host['UI'])),
        'NEWLINE_V55': new_line['GreenV55'], 'NEWLINE_V56': new_line['GreenV56'], 'NEWLINE_RED': new_line['RedBase'], 'COVX_LINE': covx_line, 'COVX_RESULT': covx_result, 'COVX_BAJAN': covx_bajan, 'COVX_CORE': tot(covx_core), 'COVX_U18': tot(covx_unions[0]),
        'SONDA_RED_UPDATE': '`%s` (la sonda de %sActualizar%s sobre la base, `/d/r80`; con la recarga, `%s`, `/d/r81`)' % (
            c_campo['UpdateWithoutReload'], LAQUO, RAQUO, c_host['UpdateAfterControl']),
    }
    with open(os.path.join(S, 'values-v56.json'), 'w', encoding='utf-8', newline='\n') as f:
        f.write(json.dumps(values, ensure_ascii=False, indent=2) + '\n')
    print('json %s; compile-check %s; todo lo previsto: %s' % (jsha, ccsha, 'SI' if ok else 'NO'))
    for l, v in checks:
        if not v:
            print('  NO: ' + l)


if __name__ == '__main__':
    main()

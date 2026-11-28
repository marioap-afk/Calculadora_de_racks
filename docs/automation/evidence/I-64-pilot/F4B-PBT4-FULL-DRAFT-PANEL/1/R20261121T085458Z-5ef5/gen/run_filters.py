"""Corre, uno a uno, los filtros del RequiredTests (gen/filters.tsv) con `dotnet test` y resume cada uno contra su minimo y su RED esperado.
Los de UI son solo los del contrato (nunca la suite completa). Resultados (TRX y log) en tests/<RunRef>/f<N>.*; resumen en tests/<RunRef>/filters-summary.txt y .json.
Uso: python -I run_filters.py <RunRef> <Debug|Release> <core|ui|all> [--no-build] [--expect-red-as-green] [--red-only] [--only <subcadena>]
- sin --expect-red-as-green: un filtro con RED esperado "si" tiene que dar Failed == Selected, todo por asercion (la primera linea del StackTrace bajo tests/), sin
  Skipped ni NotExecuted; uno con "no" tiene que pasar entero, con Passed >= minimo.
- con --expect-red-as-green (el GREEN): todos tienen que pasar entero, con Passed >= minimo."""
import json
import os
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

HERE = os.path.dirname(os.path.abspath(__file__))
RUN = os.path.dirname(HERE)
W = RUN
for _ in range(6):
    W = os.path.dirname(W)
DN = r'<datos-locales-del-usuario>\Microsoft\dotnet\dotnet.exe'
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
REL = os.path.relpath(RUN, W).replace(os.sep, '/')


def summarize(trx):
    root = ET.parse(trx).getroot()
    c = {'Passed': 0, 'Failed': 0, 'NotExecuted': 0, 'Other': 0}
    by_assertion = 0
    failed = []
    for r in root.iter('{%s}UnitTestResult' % NS['t']):
        o = r.get('outcome')
        if o == 'Passed':
            c['Passed'] += 1
        elif o == 'Failed':
            c['Failed'] += 1
            st = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS)
            first = ''
            if st is not None and st.text:
                for line in st.text.split('\n'):
                    if line.strip().startswith('at '):
                        first = line.strip()
                        break
            if re.search(r'in .*[\\/]tests[\\/]', first):
                by_assertion += 1
            failed.append(r.get('testName'))
        elif o == 'NotExecuted':
            c['NotExecuted'] += 1
        else:
            c['Other'] += 1
    return c, by_assertion, failed


def main():
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    ref, cfg, which = args[0], args[1], args[2]
    no_build = '--no-build' in sys.argv
    green = '--expect-red-as-green' in sys.argv
    only = sys.argv[sys.argv.index('--only') + 1] if '--only' in sys.argv else None
    out_dir = os.path.join(RUN, 'tests', ref)
    os.makedirs(out_dir, exist_ok=True)
    rows = [l.split('\t') for l in open(os.path.join(HERE, 'filters.tsv'), encoding='utf-8').read().split('\n') if l]
    results, bad = [], 0
    for n, (project, flt, minimum, red) in enumerate(rows):
        is_ui = 'UI.Tests' in project
        if (which == 'core' and is_ui) or (which == 'ui' and not is_ui):
            continue
        if only and only not in flt:
            continue
        if '--red-only' in sys.argv and red != 'si':
            continue
        expect_red = (red == 'si') and not green
        tag = 'f%02d' % n
        cmd = [DN, 'test', project, '-c', cfg, '--filter', flt, '--logger', 'trx;LogFileName=%s.trx' % tag, '--results-directory', os.path.join(out_dir),
               '--blame-hang-timeout', '10m', '--blame-hang-dump-type', 'none']
        if no_build:
            cmd.append('--no-build')
        with open(os.path.join(out_dir, tag + '.log'), 'wb') as log:
            p = subprocess.run(cmd, cwd=W, stdout=log, stderr=subprocess.STDOUT)
        trx = os.path.join(out_dir, tag + '.trx')
        if not os.path.exists(trx):
            line = 'SIN-TRX exit=%d %s' % (p.returncode, flt)
            results.append({'Filter': flt, 'Project': project, 'Tag': tag, 'Error': 'sin TRX'})
            print(line)
            bad += 1
            continue
        c, by_assertion, failed = summarize(trx)
        selected = c['Passed'] + c['Failed'] + c['NotExecuted'] + c['Other']
        skipped = 0
        # los Skipped de xUnit salen como NotExecuted en el TRX
        skipped = c['NotExecuted']
        if expect_red:
            ok = selected >= int(minimum) and c['Failed'] == selected and by_assertion == c['Failed']
        else:
            ok = c['Failed'] == 0 and c['Passed'] >= int(minimum) and c['Other'] == 0
        bad += 0 if ok else 1
        line = '%s %s sel=%d passed=%d failed=%d skipped=%d por-asercion=%d min=%s %s' % (
            'OK  ' if ok else 'MAL ', flt.replace('FullyQualifiedName~', ''), selected, c['Passed'], c['Failed'], skipped, by_assertion, minimum,
            'RED' if expect_red else 'verde')
        print(line)
        results.append({'Project': project, 'Filter': flt, 'Tag': tag, 'Selected': selected, 'Passed': c['Passed'], 'Failed': c['Failed'], 'Skipped': skipped,
                        'FailedByAssertion': by_assertion, 'MinSelected': int(minimum), 'ExpectRed': expect_red, 'Ok': ok,
                        'ResultsFile': '%s/tests/%s/%s.trx' % (REL, ref, tag)})
    suffix = '' if not only else '-' + re.sub(r'\W+', '_', only)
    with open(os.path.join(out_dir, 'filters-summary%s-%s.json' % (suffix, which)), 'w', encoding='utf-8') as f:
        json.dump(results, f, indent=1)
    print('filtros=%d mal=%d' % (len(results), bad))
    sys.exit(0 if bad == 0 else 1)


if __name__ == '__main__':
    main()

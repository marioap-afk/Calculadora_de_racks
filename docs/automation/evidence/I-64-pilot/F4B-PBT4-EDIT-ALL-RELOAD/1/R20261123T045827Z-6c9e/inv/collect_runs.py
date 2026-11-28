"""Reune, solo leyendo, las corridas de prueba (TRX) de tests/<RunRef>/ y escribe inv/runs.json con TestsExecuted y TestResults listos para la entrega.
Uso: python -I collect_runs.py"""
import json
import os
import re
import sys
import xml.etree.ElementTree as ET

A = 'artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-RELOAD/1/R20261123T045827Z-6c9e'
NS = '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
DOTNET = os.environ['DOTNET']
PROJ = {'core': 'tests/RackCad.Tests/RackCad.Tests.csproj', 'ui': 'tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj'}
PHASE = {'b0': 'RELEVANT', 'r1': 'RED', 'r2': 'RED', 'r3': 'RED', 'gf': 'GREEN', 'ga': 'GREEN', 'g1': 'GREEN', 'g2': 'GREEN', 'g3': 'GREEN'}
ORDER = ['b0', 'r1', 'r2', 'r3', 'gf', 'ga', 'g1', 'g2', 'g3']


def trx(path):
    counts = {}
    failed = []
    for r in ET.parse(path).getroot().iter(NS + 'UnitTestResult'):
        o = r.get('outcome')
        counts[o] = counts.get(o, 0) + 1
        if o == 'Failed':
            failed.append(r.get('testName'))
    total = sum(counts.values())
    skipped = counts.get('NotExecuted', 0) + counts.get('Skipped', 0)
    return total, counts.get('Passed', 0), counts.get('Failed', 0), skipped, sorted(failed)


def filter_map(run):
    """nombre-del-trx -> (kind, filtro) a partir del resumen de la corrida."""
    m = {}
    for fn in ('all-summary.txt', 'ui-filters-summary.txt'):
        p = '%s/tests/%s/%s' % (A, run, fn)
        if not os.path.exists(p):
            continue
        cur = None
        for line in open(p, encoding='utf-8'):
            line = line.rstrip('\n')
            mm = re.match(r'== (?:(core|ui) )?(\S+) min=', line)
            if mm:
                kind = mm.group(1) or 'ui'
                cur = (kind, mm.group(2))
                continue
            mm = re.search(r'/([^/]+\.trx) total=', line)
            if mm and cur:
                m[mm.group(1)] = cur
                cur = None
    return m


def main():
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    executed, results = [], []
    for run in ORDER:
        d = '%s/tests/%s' % (A, run)
        fm = filter_map(run)
        for fn in sorted(os.listdir(d)):
            if not fn.endswith('.trx'):
                continue
            name = fn[:-4]
            runref = '%s/%s' % (run, name)
            rel = '%s/%s' % (d, fn)
            if name == 'core-full':
                cmd = '%s test %s --logger "trx;LogFileName=core-full.trx" --results-directory %s --blame-hang-timeout 10m --blame-hang-dump-type none' % (DOTNET, PROJ['core'], d)
            else:
                if fn in fm:
                    kind, flt = fm[fn]
                elif name.startswith('core-'):
                    kind, flt = 'core', 'RackCad.Tests.' + name[5:]
                elif name.startswith('ui-'):
                    kind, flt = 'ui', 'RackCad.UI.Tests.' + name[3:]
                else:
                    raise SystemExit('sin filtro para ' + rel)
                cmd = '%s test %s --filter "FullyQualifiedName~%s" --logger "trx;LogFileName=%s.trx" --results-directory %s --blame-hang-timeout 10m --blame-hang-dump-type none' % (DOTNET, PROJ[kind], flt, name, d)
            total, passed, failed, skipped, names = trx(rel)
            executed.append({'RunRef': runref, 'Command': cmd, 'Phase': PHASE[run], 'TreeSha': None, 'ResultsFile': rel})
            results.append({'RunRef': runref, 'Selected': total, 'Passed': passed, 'Failed': failed, 'Skipped': skipped, 'FailedTests': names})
    json.dump({'TestsExecuted': executed, 'TestResults': results}, open(A + '/inv/runs.json', 'w', encoding='utf-8', newline='\n'), indent=1, ensure_ascii=False)
    print('corridas:', len(executed))
    # resumen por fase
    for run in ORDER:
        rs = [r for r in results if r['RunRef'].startswith(run + '/')]
        print(run, 'corridas=%d' % len(rs), 'Selected=%d Passed=%d Failed=%d Skipped=%d' % tuple(sum(r[k] for r in rs) for k in ('Selected', 'Passed', 'Failed', 'Skipped')))


if __name__ == '__main__':
    main()

"""Cuenta filas de la TRX por clase y por metodo (solo lectura). Uso: python -I count_rows.py <trx> [<trx> ...]"""
import sys, re, collections
import xml.etree.ElementTree as ET
NS = '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
CLASSES = ('PushBackFullWritePrepareTests', 'PushBackFullWriteRunTests', 'PushBackPilotUpdateEquivalenceGuardTests', 'PushBackWritePathGuardTests',
           'WorkspacePilotPrepareGuardTests', 'WorkspacePilotUpdateGuardTests', 'WorkspacePilotUpdateIntegrationGuardTests')
for p in sys.argv[1:]:
    root = ET.parse(p).getroot()
    tot = collections.Counter()
    per = collections.Counter()
    skipped = []
    for r in root.iter(NS + 'UnitTestResult'):
        n, o = r.get('testName'), r.get('outcome')
        tot[o] += 1
        for c in CLASSES:
            if '.' + c + '.' in '.' + n:
                per[(c, o)] += 1
        if o == 'NotExecuted':
            skipped.append(n)
    print(p.replace('\\', '/').split('/rv/')[-1], dict(tot), sum(tot.values()))
    for k in sorted(per):
        print('   ', k, per[k])
    for s in skipped:
        if 'Workspace' in s or 'PushBack' in s:
            print('    skip', s[:200])

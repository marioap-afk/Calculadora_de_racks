"""Sonda: para cada ancla (archivo, linea) da el metodo que la contiene y sus lineas, y los metodos vecinos.

Uso: python -I probe_anchors.py <checkout> <rev>
"""
import sys
import importlib.util

spec = importlib.util.spec_from_file_location('lc', __file__.replace('probe_anchors.py', 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
lc.REPO, lc.REV = sys.argv[1:3]
TW = 'tests/RackCad.Tests/Workspace/'
ANCHORS = [
    ('tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs', [61]),
    ('tests/RackCad.Tests/PushBackWritePathGuardTests.cs', [235, 252, 296]),
    (TW + 'WorkspacePilotUpdateIntegrationGuardTests.cs', [497, 498]),
    (TW + 'WorkspacePilotEditGuardTests.cs', [156, 163, 280, 380, 386]),
    (TW + 'WorkspacePilotReconciliationGuardTests.cs', [109, 123]),
    ('tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs', [307]),
    ('tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs', [137, 221, 231]),
    (TW + 'WorkspacePilotPrepareTests.cs', [1350, 1355]),
]
for path, lines in ANCHORS:
    ms = lc.members(lc.blank_comments(lc.show(path)))
    print('==', path)
    for line in lines:
        hit = [m for m in ms if m['first'] <= line <= m['last']]
        print('  :%d -> %s' % (line, ', '.join('%s %s %d-%d' % ('T' if m['test'] else 'h', m['name'], m['first'], m['last']) for m in hit) or '-'))

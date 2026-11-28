import json, subprocess, sys, xml.etree.ElementTree as ET

ART = 'artifacts/orchestration/I-64/F4-T4C2-PANEL/1/R20261115T190629Z-9b55'
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
RED_TREE = subprocess.run(['git', 'rev-parse', 'HEAD^{tree}'], capture_output=True, text=True).stdout.strip()
BASE = '1cce48c00575f48e15106e4007c887c08746b228'
RED = subprocess.run(['git', 'rev-parse', 'HEAD'], capture_output=True, text=True).stdout.strip()


def counters(trx):
    root = ET.parse(trx).getroot()
    c = root.find('.//t:ResultSummary/t:Counters', NS)
    total = int(c.get('total'))
    passed = int(c.get('passed'))
    failed = int(c.get('failed'))
    skipped = total - passed - failed
    names = []
    for u in root.iter('{%s}UnitTestResult' % NS['t']):
        if u.get('outcome') == 'Failed':
            names.append(u.get('testName'))
    return total, passed, failed, skipped, sorted(set(names))


CORE = 'tests/RackCad.Tests/RackCad.Tests.csproj'
UI = 'tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj'
classes = [l.split()[0] for l in open(ART + '/classes.txt', encoding='utf-8').read().splitlines() if l.strip()]

runs = []
runs.append(('r1-core-ws', 'RED', RED_TREE, CORE, 'FullyQualifiedName~RackCad.Tests.Workspace', 'tests/r1/core-ws.trx', 'r1'))
runs.append(('r1-ui-ws', 'RED', RED_TREE, UI, 'FullyQualifiedName~RackCad.UI.Tests.Workspace', 'tests/r1/ui-ws.trx', 'r1'))
for cls in classes:
    runs.append(('r2-' + cls, 'RED', RED_TREE, CORE, 'FullyQualifiedName~RackCad.Tests.' + cls, 'tests/r2/' + cls + '.trx', 'r2'))
runs.append(('r2-ui-pbc', 'RED', RED_TREE, UI, 'FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization', 'tests/r2/ui-pbc.trx', 'r2'))
runs.append(('g1-core-ws', 'RELEVANT', None, CORE, 'FullyQualifiedName~RackCad.Tests.Workspace', 'tests/g1/core-ws.trx', 'g1'))
runs.append(('g1-ui-ws', 'RELEVANT', None, UI, 'FullyQualifiedName~RackCad.UI.Tests.Workspace', 'tests/g1/ui-ws.trx', 'g1'))

executed = []
results = []
for ref, phase, tree, proj, flt, trx, folder in runs:
    path = ART + '/' + trx
    fname = trx.split('/')[-1]
    cmd = 'dotnet test ' + proj + ' -c Debug' + (' --no-build' if folder == 'r2' else '') + ' --filter "' + flt + '" --logger "trx;LogFileName=' + fname + '" --results-directory ' + ART + '/tests/' + folder
    executed.append({'RunRef': ref, 'Command': cmd, 'Phase': phase, 'TreeSha': tree, 'ResultsFile': path})
    total, passed, failed, skipped, names = counters(path)
    results.append({'RunRef': ref, 'Selected': total, 'Passed': passed, 'Failed': failed, 'Skipped': skipped, 'FailedTests': names})

executed.append({
    'RunRef': 'b0-build-release-base',
    'Command': 'dotnet build RackCad.sln -c Release --no-incremental -o ' + ART + '/tests/b0/out',
    'Phase': 'RELEVANT', 'TreeSha': BASE if False else None, 'ResultsFile': ART + '/tests/b0/build.log'})
executed.append({
    'RunRef': 'g2-build-release-wip',
    'Command': 'dotnet build RackCad.sln -c Release --no-incremental -o ' + ART + '/tests/g2/out',
    'Phase': 'RELEVANT', 'TreeSha': None, 'ResultsFile': ART + '/tests/g2/build.log'})
results.append({'RunRef': 'b0-build-release-base', 'Selected': 0, 'Passed': 0, 'Failed': 0, 'Skipped': 0, 'FailedTests': []})
results.append({'RunRef': 'g2-build-release-wip', 'Selected': 0, 'Passed': 0, 'Failed': 0, 'Skipped': 0, 'FailedTests': []})

files = subprocess.run(['git', 'diff', '--name-only', BASE + '..' + RED], capture_output=True, text=True).stdout.split()

ev = json.load(open(ART + '/evidence.json', encoding='utf-8'))

handoff = {
    'Schema': 'rackcad-worker-handoff/v1',
    'TaskId': 'F4-T4C2-PANEL',
    'RunId': 'R20261115T190629Z-9b55',
    'DelegationRunId': 'R20261115T190148Z-4797',
    'Initiative': 'I-64',
    'Gate': 'F4',
    'Attempt': 1,
    'BaseSha': BASE,
    'RedSha': RED,
    'CurrentSha': RED,
    'Branch': 'architecture/workspace-persistente-rackcad',
    'Worktree': '<worktree-de-I-64>',
    'Pushed': True,
    'FilesChanged': files,
    'WorkCompleted': ev['WorkCompleted'],
    'TestsExecuted': executed,
    'TestResults': results,
    'Evidence': ev['Evidence'],
    'UnexpectedFindings': ev['UnexpectedFindings'],
    'KnownLimitations': ev['KnownLimitations'],
    'Deviations': ev['Deviations'],
    'OpenQuestions': [],
    'WorkerStatus': 'BLOCKED',
    'Disposition': 'BLOCKED',
    'TriggeredStopConditions': ['S-04'],
    'RecommendedNextAction': ev['RecommendedNextAction'],
    'Worker': {
        'Provider': 'Anthropic',
        'ModelRequested': 'claude-sonnet-5-5',
        'EffortRequested': 'high',
        'Trailer': 'Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>',
    },
}
json.dump(handoff, open(ART + '/worker-handoff.json', 'w', encoding='utf-8', newline='\n'), ensure_ascii=False, indent=2)
print('written', len(executed), 'runs', len(files), 'files')

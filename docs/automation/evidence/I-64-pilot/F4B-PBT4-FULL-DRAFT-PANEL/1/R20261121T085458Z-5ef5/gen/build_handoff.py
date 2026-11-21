"""Compone worker-handoff.json con los datos medidos (TRX, resumenes, SHA, CI). Solo lee; escribe el JSON de la entrega.
Uso: python -I build_handoff.py <BaseSha> <RedSha> <RedRepairSha> <GreenSha> <ciRed> <ciRr> <ciGreen> <ciGreenSummaryFile>
Las rutas que escribe son relativas al worktree y se comprueban con os.path.exists."""
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
REL = os.path.relpath(RUN, W).replace(os.sep, '/')
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
TERMS = ['GATE PASS', 'GATE_PASS', 'Candidato', 'Candidate', 'FINAL_CANDIDATE_SHA', 'gate cerrado', 'gate closed', 'cierre del gate', 'rama integrada',
         'integrada en main', 'integrated into main', 'merged into main', 'lista para integrar', 'ready to merge', 'Owner Validation APROBADA',
         'Owner Validation APPROVED']

DOTNET = 'dotnet'
SETS = [
    # (carpeta, fase, descripcion)
    ('b0', 'RELEVANT', 'BaseSha, linea base (sin commit)'),
    ('r0', 'RED', 'arbol del RED antes del commit (clases de las CT)'),
    ('r1', 'RED', 'arbol del RED antes del commit (suite completa de Core)'),
    ('r2', 'RED', 'arbol del RED antes del commit (filtros del contrato)'),
    ('rr1', 'RED', 'arbol de la reparacion del RED antes del commit (suite completa de Core)'),
    ('rr2', 'RED', 'arbol de la reparacion del RED antes del commit (filtros del contrato)'),
    ('g0', 'GREEN', 'arbol del GREEN de trabajo, antes de la reparacion (clases de las CT)'),
    ('g1', 'GREEN', 'arbol del GREEN de trabajo, antes de la reparacion (suite completa de Core)'),
    ('g2', 'GREEN', 'arbol del GREEN de trabajo, antes de la reparacion (filtros de UI)'),
    ('tmp', 'RELEVANT', 'corrida temporal de diagnostico de CT-PBT4B-08 (no entra en ningun commit)'),
    ('gz', 'GREEN', 'arbol del GREEN final antes del commit (suite completa de Core)'),
    ('g3', 'GREEN', 'arbol del GREEN final antes del commit (filtros del contrato)'),
    ('gA', 'GREEN', 'primera de las tres corridas en verde de los filtros con RED esperado'),
    ('gB', 'GREEN', 'segunda de las tres corridas en verde de los filtros con RED esperado'),
    ('gC', 'GREEN', 'tercera de las tres corridas en verde de los filtros con RED esperado'),
]
NO_BUILD = {('r2', 'core'), ('r2', 'ui'), ('rr2', 'core'), ('g3', 'core'), ('gA', 'core'), ('gA', 'ui'), ('gB', 'core'), ('gB', 'ui'), ('gC', 'core'), ('gC', 'ui')}
UI_PROJECT = 'tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj'
CORE_PROJECT = 'tests/RackCad.Tests/RackCad.Tests.csproj'


def counts(trx):
    root = ET.parse(trx).getroot()
    c = {'Passed': 0, 'Failed': 0, 'Skipped': 0, 'Other': 0}
    failed = []
    for r in root.iter('{%s}UnitTestResult' % NS['t']):
        o = r.get('outcome')
        if o == 'Passed':
            c['Passed'] += 1
        elif o == 'Failed':
            c['Failed'] += 1
            failed.append(r.get('testName'))
        elif o == 'NotExecuted':
            c['Skipped'] += 1
        else:
            c['Other'] += 1
    return c, failed


def exists(rel):
    assert os.path.exists(os.path.join(W, *rel.split('/'))), rel
    return rel


def git(*args):
    return subprocess.run(['git', '-C', W] + list(args), capture_output=True, check=True).stdout.decode('utf-8', 'replace')


def main():
    base, red, rr, green, ci_red, ci_rr, ci_green, ci_green_file = sys.argv[1:9]
    executed, results = [], []
    for ref, phase, what in SETS:
        d = os.path.join(RUN, 'tests', ref)
        if not os.path.isdir(d):
            continue
        summaries = {}
        for which in ('core', 'ui'):
            p = os.path.join(d, 'filters-summary-%s.json' % which)
            if os.path.exists(p):
                for row in json.load(open(p, encoding='utf-8')):
                    summaries[row['Tag']] = (which, row)
        for f in sorted(os.listdir(d)):
            if not f.endswith('.trx'):
                continue
            stem = f[:-4]
            rel = '%s/tests/%s/%s' % (REL, ref, f)
            exists(rel)
            outdir = '%s/tests/%s' % (REL, ref)
            if stem == 'core-full':
                cmd = '%s test %s --logger "trx;LogFileName=core-full.trx" --results-directory %s --blame-hang-timeout 10m --blame-hang-dump-type none' % (DOTNET, CORE_PROJECT, outdir)
            elif stem in summaries:
                which, row = summaries[stem]
                cmd = '%s test %s -c Debug --filter "%s" --logger "trx;LogFileName=%s.trx" --results-directory %s --blame-hang-timeout 10m --blame-hang-dump-type none%s' % (
                    DOTNET, row['Project'], row['Filter'], stem, outdir, ' --no-build' if (ref, which) in NO_BUILD else '')
            elif stem.startswith('core-') or stem.startswith('ui-'):
                kind, name = stem.split('-', 1)
                project = CORE_PROJECT if kind == 'core' else UI_PROJECT
                ns = 'RackCad.Tests' if kind == 'core' else 'RackCad.UI.Tests'
                cmd = '%s test %s -c Debug --filter "FullyQualifiedName~%s.%s" --logger "trx;LogFileName=%s.trx" --results-directory %s --blame-hang-timeout 10m --blame-hang-dump-type none' % (
                    DOTNET, project, ns, name, stem, outdir)
            else:
                continue
            c, failed = counts(os.path.join(d, f))
            run_ref = '%s-%s' % (ref, stem)
            executed.append({'RunRef': run_ref, 'Command': cmd, 'Phase': phase, 'TreeSha': None, 'ResultsFile': rel})
            results.append({'RunRef': run_ref, 'Selected': sum(c.values()), 'Passed': c['Passed'], 'Failed': c['Failed'], 'Skipped': c['Skipped'], 'FailedTests': failed})
    # CI: los TRX descargados
    for ref, phase, folder in (('redci', 'RED', 'tests/redci'), ('rrci', 'RED', 'tests/rrci')):
        for kind, project in (('core', CORE_PROJECT), ('ui', UI_PROJECT)):
            rel = '%s/%s/%s/%s.trx' % (REL, folder, kind, kind)
            exists(rel)
            c, failed = counts(os.path.join(W, *rel.split('/')))
            run_ref = '%s-%s' % (ref, kind)
            executed.append({'RunRef': run_ref, 'Command': 'gh run download %s --repo marioap-afk/Calculadora_de_racks -n rackcad-%s-test-diagnostics (TRX de la CI push)' % (ci_red if ref == 'redci' else ci_rr, kind), 'Phase': phase, 'TreeSha': None, 'ResultsFile': rel})
            results.append({'RunRef': run_ref, 'Selected': sum(c.values()), 'Passed': c['Passed'], 'Failed': c['Failed'], 'Skipped': c['Skipped'], 'FailedTests': failed})
    for kind, project in (('core', CORE_PROJECT), ('ui', UI_PROJECT)):
        rel = '%s/tests/gci/%s/%s.trx' % (REL, kind, kind)
        if os.path.exists(os.path.join(W, *rel.split('/'))):
            c, failed = counts(os.path.join(W, *rel.split('/')))
            run_ref = 'gci-%s' % kind
            executed.append({'RunRef': run_ref, 'Command': 'gh run download %s --repo marioap-afk/Calculadora_de_racks -n rackcad-%s-test-diagnostics (TRX de la CI push)' % (ci_green, kind), 'Phase': 'GREEN', 'TreeSha': None, 'ResultsFile': rel})
            results.append({'RunRef': run_ref, 'Selected': sum(c.values()), 'Passed': c['Passed'], 'Failed': c['Failed'], 'Skipped': c['Skipped'], 'FailedTests': failed})

    files = [l for l in git('diff', '--name-only', base, green).split('\n') if l]
    extra = json.load(open(os.path.join(HERE, 'handoff-text.json'), encoding='utf-8'))
    for key in ('WorkCompleted',):
        extra[key] = extra[key].replace('{BASE}', base).replace('{RED}', red).replace('{RR}', rr).replace('{GREEN}', green)
    for key in ('Evidence', 'UnexpectedFindings', 'KnownLimitations', 'Deviations', 'OpenQuestions', 'TriggeredStopConditions'):
        extra[key] = [s.replace('{BASE}', base).replace('{RED}', red).replace('{RR}', rr).replace('{GREEN}', green).replace('{CIRED}', ci_red).replace('{CIRR}', ci_rr).replace('{CIGREEN}', ci_green) for s in extra[key]]
    ci_text = open(ci_green_file, encoding='utf-8').read().strip()
    extra['Evidence'] = [s.replace('{CIGREENSUMMARY}', ci_text) for s in extra['Evidence']]
    extra['RecommendedNextAction'] = extra['RecommendedNextAction'].replace('{GREEN}', green)
    doc = {
        'Schema': 'rackcad-worker-handoff/v1',
        'TaskId': 'F4B-PBT4-FULL-DRAFT-PANEL',
        'RunId': 'R20261121T085458Z-5ef5',
        'DelegationRunId': 'R20261121T084940Z-a2b4',
        'Initiative': 'I-64',
        'Gate': 'F4B',
        'Attempt': 1,
        'BaseSha': base,
        'RedSha': red,
        'CurrentSha': green,
        'Branch': 'architecture/workspace-persistente-rackcad',
        'Worktree': r'<worktree-de-I-64>',
        'Pushed': True,
        'FilesChanged': files,
        'WorkCompleted': extra['WorkCompleted'],
        'TestsExecuted': executed,
        'TestResults': results,
        'Evidence': extra['Evidence'],
        'UnexpectedFindings': extra['UnexpectedFindings'],
        'KnownLimitations': extra['KnownLimitations'],
        'Deviations': extra['Deviations'],
        'OpenQuestions': extra['OpenQuestions'],
        'WorkerStatus': 'IMPLEMENTATION_COMPLETE',
        'Disposition': 'NONE',
        'TriggeredStopConditions': extra['TriggeredStopConditions'],
        'RecommendedNextAction': extra['RecommendedNextAction'],
        'Worker': {'Provider': 'Anthropic', 'ModelRequested': 'claude-sonnet-5-5', 'EffortRequested': 'high',
                   'Trailer': 'Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>'},
    }
    text = json.dumps(doc, indent=1, ensure_ascii=False)
    low = text.lower()
    for term in TERMS:
        assert term.lower() not in low, 'termino prohibido en la entrega: ' + term
    # las rutas de artefacto citadas en Evidence: relativas y existentes
    for s in doc['Evidence']:
        for m in re.finditer(r'artifacts/orchestration/I-64/F4B-PBT4-FULL-DRAFT-PANEL/1/R20261121T085458Z-5ef5/[\w./@-]+', s):
            exists(m.group(0).rstrip('.,;)'))
    with open(os.path.join(RUN, 'worker-handoff.json'), 'w', encoding='utf-8', newline='\n') as f:
        f.write(text + '\n')
    print('worker-handoff.json: %d corridas, %d archivos cambiados, %d bytes' % (len(executed), len(files), len(text)))


if __name__ == '__main__':
    main()

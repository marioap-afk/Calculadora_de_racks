"""Recomprobacion v2: resumen de las corridas en review/runs (y de las sondas en review/probes si existen). Uso: python -I -B summarize_runs.py <dir> [<dir-firma-real> ...]"""
import glob, json, os, sys

D = sys.argv[1]
for p in sorted(glob.glob(os.path.join(D, '*.json'))):
    if p.endswith('summary.json'):
        continue
    r = json.load(open(p, encoding='utf-8'))
    pr = r.get('Properties') or {}
    m = r.get('Measured') or {}
    print('%-34s %-6s id=%s task=%s mode=%s false=%s matched=%s by=%s bind=%s record=%s st=%s/%s det=%s sha=%s scope=%s' % (
        os.path.basename(p), str(r.get('SubstituteResult'))[:70], r.get('Id'), r.get('Task'), r.get('Mode'),
        sorted(k for k, v in pr.items() if not v), m.get('ExactEvidenceMatched'), m.get('ExactWhitelistBy'),
        (r.get('TaskBinding') or {}).get('Ok'), (r.get('Record') or {}).get('nc2'),
        sum(1 for x in r.get('SelfTest', []) if x.get('Ok')), len(r.get('SelfTest', [])), r.get('SelfTestDeterministic'),
        (r.get('ScriptSha256') or '')[:8], r.get('Scope')))
    if 'Nc2ControllerChecks' in r:
        print('    nc2 checks no-pass:', {k: v for k, v in r['Nc2ControllerChecks'].items() if v != 'pass'}, r.get('Nc2ControllerClassification'), r.get('Nc2ControllerDisposition'), r.get('Nc2ControllerFailureClass'),
              '| ChangedPath:', m.get('ChangedPath'), '| bytesEq:', m.get('RawPlusContractCollectionsEqualsDelegationBytes'))
for real_dir in sys.argv[2:]:
    ro = json.load(open(os.path.join(real_dir, 'real', 'output.json'), encoding='utf-8'))
    no = json.load(open(os.path.join(real_dir, 'nc2', 'output.json'), encoding='utf-8'))
    rc = {k: v['Result'] for k, v in ro['Checks'].items()}
    nc = {k: v['Result'] for k, v in no['Checks'].items()}
    print('FIRMA', real_dir[-12:], '| real:', ro.get('Classification'), ro.get('Disposition'), '| nc2:', no.get('Classification'), no.get('Disposition'), no.get('FailureClass'),
          no.get('TriggeredStopConditions'), '| difieren:', {k: (rc.get(k), nc.get(k)) for k in sorted(set(rc) | set(nc)) if rc.get(k) != nc.get(k)})

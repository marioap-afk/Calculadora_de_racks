"""Revision v3 (solo lectura): tamano de los alcances, diff, ruta quitada y firma de nc2 en cada caso real.
No usa codigo de los sustitutos. Uso: python -I -B inspect_cases.py <worktree> <cases.json>"""
import json, os, subprocess, sys

wt, cases = sys.argv[1], json.load(open(sys.argv[2], encoding='utf-8'))


def rd(p):
    return json.load(open(p if os.path.isabs(p) else os.path.join(wt, p), encoding='utf-8'))


def checks(o):
    ch = o['Checks']
    return {k: v['Result'] for k, v in ch.items()} if isinstance(ch, dict) else {c['Id']: c['Result'] for c in ch}


for name, c in cases.items():
    if name.startswith('_'):
        continue
    real, nc2 = c['real'], c['nc2']
    rdel, mdel = rd(os.path.join(real, 'inputs', 'delegation.json')), rd(os.path.join(nc2, 'inputs', 'delegation.json'))
    ct = rd(os.path.join(real, 'inputs', 'gate-contract.json'))
    changed = subprocess.run(['git', 'diff', '--name-only', c['base'] + '..' + c['cur']], cwd=wt, capture_output=True, text=True, check=True).stdout.split()
    ra, ma, ca = rdel['AllowedWriteScope'], mdel['AllowedWriteScope'], ct['AllowedWriteScope']
    ro, no = rd(os.path.join(real, 'output.json')), rd(os.path.join(nc2, 'output.json'))
    rc, nc = checks(ro), checks(no)
    print('==', name, '| TaskId', rdel.get('TaskId'), '| Attempt', rdel.get('Attempt'))
    print('  len AWS contrato/real/mutado:', len(ca), len(ra), len(ma), '| distintos en contrato:', len(set(ca)), '| prefijos en contrato:', [e for e in ca if e.endswith('/')])
    print('  len Forbidden contrato:', len(ct.get('ForbiddenWriteScope', [])), '| diff:', len(changed), '| quitadas (real - mutado):', sorted(set(ra) - set(ma)), '| anadidas:', sorted(set(ma) - set(ra)))
    print('  sorted(diff)[0]:', sorted(changed)[0] if changed else None)
    print('  real:', ro.get('Classification'), ro.get('Disposition'), '| nc2:', no.get('Classification'), no.get('Disposition'), no.get('FailureClass'), no.get('TriggeredStopConditions'))
    print('  checks que difieren real->nc2:', {k: (rc.get(k), nc.get(k)) for k in sorted(set(rc) | set(nc)) if rc.get(k) != nc.get(k)})
    sc = (no['Checks'].get('Scope') if isinstance(no['Checks'], dict) else None) or {}
    print('  evidencia Scope nc2:', json.dumps(sc, ensure_ascii=False)[:400])

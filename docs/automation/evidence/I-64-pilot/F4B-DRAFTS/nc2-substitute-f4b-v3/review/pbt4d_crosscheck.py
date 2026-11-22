"""Revision v3 (solo lectura): comprobaciones propias de la cadena de PB-T4d, sin codigo de los sustitutos.
Uso: python -I -B pbt4d_crosscheck.py <worktree> <cases.json>"""
import hashlib, json, os, subprocess, sys

wt, c = sys.argv[1], json.load(open(sys.argv[2], encoding='utf-8'))['PBT4D']
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
rb = lambda p: open(p if os.path.isabs(p) else os.path.join(wt, p), 'rb').read()
real_i, nc_i = os.path.join(c['real'], 'inputs'), os.path.join(c['nc2'], 'inputs')
rdb, mdb, ctb = rb(os.path.join(real_i, 'delegation.json')), rb(os.path.join(nc_i, 'delegation.json')), rb(os.path.join(real_i, 'gate-contract.json'))
rd, md, ct = json.loads(rdb), json.loads(mdb), json.loads(ctb)
rawb, noteb = rb(c['raw']), rb(c['note'])
raw, note = json.loads(rawb), json.loads(noteb)
rr0, rr1 = json.loads(rb(os.path.join(real_i, 'relay-record-planning.json'))), json.loads(rb(os.path.join(nc_i, 'relay-record-planning.json')))
bfc = json.loads(rb(os.path.join(real_i, 'byte-fidelity-check.json')))
changed = subprocess.run(['git', 'diff', '--name-only', c['base'] + '..' + c['cur']], cwd=wt, capture_output=True, text=True, check=True).stdout.split()
INJ = ['AllowedWriteScope', 'Objective', 'Authorities', 'ForbiddenWriteScope', 'Invariants', 'RequiredTests', 'ExpectedEvidence', 'StopConditions']
inj = dict(raw)
for k in INJ:
    inj[k] = ct.get(k)
p = sorted(changed)[0]
F = ct.get('ForbiddenWriteScope', [])
cov = lambda x, s: any(x == e or (e.endswith('/') and x.startswith(e)) for e in s)
checks = {
    'sha(cruda) = nota.OutputSha256 = relevo real': sha(rawb) == note['OutputSha256'].upper() == rr0['Outcome']['OutputSha256'].upper(),
    'cruda con las 8 colecciones vacias': all(raw[k] == ('' if k == 'Objective' else []) for k in INJ),
    'cruda + 8 del contrato = delegation.json (bytes)': (json.dumps(inj, ensure_ascii=False, indent=2) + '\n').encode('utf-8') == rdb,
    'relevo nc2 = sha(delegacion mutada)': rr1['Outcome']['OutputSha256'].upper() == sha(mdb),
    'bfc.OutputSha256 = sha(delegacion real); bfc.ContractSha256 = sha(contrato)': bfc['OutputSha256'].upper() == sha(rdb) and bfc['ContractSha256'].upper() == sha(ctb),
    'nota.DelegationSha256 y ContractSha256': note['DelegationSha256'].upper() == sha(rdb) and note['ContractSha256'].upper() == sha(ctb),
    'RunId igual en nota, bfc, relevo y delegacion': note['RunId'] == bfc.get('PlanningRunId') == rr0.get('RunId') == rd.get('RunId') != '',
    'AWS real = AWS contrato (en orden)': rd['AllowedWriteScope'] == ct['AllowedWriteScope'],
    'solo cambia AllowedWriteScope entre real y nc2': sorted(k for k in set(rd) | set(md) if rd.get(k) != md.get(k)) == ['AllowedWriteScope'],
    'mutado = real sin sorted(diff)[0], en orden': md['AllowedWriteScope'] == [e for e in rd['AllowedWriteScope'] if e != p],
    'diff dentro del alcance real y fuera del prohibido': all(cov(x, rd['AllowedWriteScope']) for x in changed) and not any(cov(x, F) for x in changed),
    'p fuera del alcance mutado': not cov(p, md['AllowedWriteScope']),
    'p no prohibido (forma cerrada de la 10)': cov(p, ct['AllowedWriteScope']) and not cov(p, F),
}
print('p =', p, '| len AWS', len(ct['AllowedWriteScope']), '| len diff', len(changed), '| set(diff) == set(AWS):', set(changed) == set(ct['AllowedWriteScope']))
print('sha cruda', sha(rawb), '| sha nota', sha(noteb))
for k, v in checks.items():
    print('%-5s %s' % (v, k))
print('TODO', all(checks.values()))

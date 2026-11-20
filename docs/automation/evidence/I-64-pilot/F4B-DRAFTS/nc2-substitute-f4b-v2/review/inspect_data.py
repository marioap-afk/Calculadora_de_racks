"""Inspeccion de los datos reales para la revision independiente (solo lectura)."""
import hashlib, json, os, sys, unicodedata

ROOT = sys.argv[1]
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
rb = lambda p: open(os.path.join(ROOT, p), 'rb').read()
lj = lambda p: json.loads(rb(p))


def norm(s):
    return ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c)).lower()


ct = lj('data/real/inputs/gate-contract.json')
rd = lj('data/real/inputs/delegation.json')
md = lj('data/nc2/inputs/delegation.json')
raw = lj('data/planning/output.json')
note = lj('data/planning/session-inject-note.json')
bfc = lj('data/real/inputs/byte-fidelity-check.json')

print('== ExpectedEvidence del contrato (%d entradas)' % len(ct.get('ExpectedEvidence', [])))
for i, e in enumerate(ct.get('ExpectedEvidence', [])):
    n = norm(e) if isinstance(e, str) else ''
    flags = []
    if 'comparacion byte a byte' in n:
        flags.append('BYTE_COMPARISON')
    if 'byte' in n:
        flags.append('byte')
    if 'exact' in n:
        flags.append('exact')
    if 'allowedwritescope' in n:
        flags.append('AllowedWriteScope')
    if flags:
        print(i, flags, repr(e)[:600])

print('== Campos de nivel superior del contrato:', sorted(ct.keys()))
print('== Campos de nivel superior de la delegacion real:', sorted(rd.keys()))
print('== Campos de la salida cruda:', sorted(raw.keys()))
opaque = ['AllowedWriteScope', 'Objective', 'Authorities', 'ForbiddenWriteScope', 'Invariants', 'RequiredTests', 'ExpectedEvidence', 'StopConditions']
print('== Colecciones opacas en la salida cruda:', {k: (raw.get(k) if not raw.get(k) else 'NO VACIA len=%d' % len(raw.get(k))) for k in opaque})
diff_keys = sorted(k for k in set(raw) | set(rd) if raw.get(k, '<absent>') != rd.get(k, '<absent>'))
print('== Campos que difieren entre salida cruda y delegacion real:', diff_keys)
print('== Campos fuera de las 8 que difieren:', [k for k in diff_keys if k not in opaque])
inj = dict(raw)
for k in opaque:
    inj[k] = ct.get(k)
print('== cruda + 8 del contrato == delegacion real (decodificado):', inj == rd)
print('== orden de claves cruda vs real igual:', list(raw.keys()) == list(rd.keys()))
# reconstruccion de bytes con el mismo formato
for indent in (2, 4):
    for ea in (False, True):
        b = (json.dumps(inj, ensure_ascii=ea, indent=indent) + '\n').encode('utf-8')
        if sha(b) == sha(rb('data/real/inputs/delegation.json')):
            print('== reconstruccion byte a byte coincide con indent=%d ensure_ascii=%s' % (indent, ea))
print('== SHA cruda', sha(rb('data/planning/output.json')), 'nota.OutputSha256', note.get('OutputSha256'))
print('== SHA delegacion real', sha(rb('data/real/inputs/delegation.json')), 'nota.DelegationSha256', note.get('DelegationSha256'), 'bfc.OutputSha256', bfc.get('OutputSha256'))
print('== SHA contrato', sha(rb('data/real/inputs/gate-contract.json')), 'nota.ContractSha256', note.get('ContractSha256'), 'bfc.ContractSha256', bfc.get('ContractSha256'))
print('== RunId nota', note.get('RunId'), 'bfc.PlanningRunId', bfc.get('PlanningRunId'), 'relevo.RunId', lj('data/real/inputs/relay-record-planning.json').get('RunId'), 'delegacion.RunId', rd.get('RunId'))
print('== RunId nc2-relevo', lj('data/nc2/inputs/relay-record-planning.json').get('RunId'))
# hash del AllowedWriteScope que declara el bfc: que serializacion es
f = {x['Field']: x for x in bfc['Fields']}
aws = ct['AllowedWriteScope']
for indent in (None, 2):
    for sep in ((',', ':'), (', ', ': ')):
        for ea in (False, True):
            b = json.dumps(aws, ensure_ascii=ea, indent=indent, separators=sep).encode('utf-8')
            if sha(b).lower() == f['AllowedWriteScope']['SourceSha256'].lower():
                print('== bfc AllowedWriteScope SourceSha256 = json.dumps(indent=%s, sep=%r, ensure_ascii=%s)' % (indent, sep, ea))
            if sha(json.dumps(md['AllowedWriteScope'], ensure_ascii=ea, indent=indent, separators=sep).encode('utf-8')).lower() == f['AllowedWriteScope']['SourceSha256'].lower():
                print('!! la lista MUTADA tambien coincide con el SourceSha256 del bfc')
print('== AllowedWriteScope contrato', aws)
print('== AllowedWriteScope real    ', rd['AllowedWriteScope'])
print('== AllowedWriteScope mutado  ', md['AllowedWriteScope'])
print('== real == contrato (orden):', rd['AllowedWriteScope'] == aws)

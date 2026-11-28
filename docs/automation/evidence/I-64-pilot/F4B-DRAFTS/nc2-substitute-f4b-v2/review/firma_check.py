"""Revision independiente: comprueba a mano los puntos de la firma PD-F4-01 que el sustituto no afirma (solo registra)."""
import json, os, sys

ROOT = sys.argv[1]
lj = lambda *p: json.load(open(os.path.join(ROOT, *p), encoding='utf-8'))
real, nc2, mut = lj('data', 'real', 'output.json'), lj('data', 'nc2', 'output.json'), lj('data', 'nc2', 'mutation.json')
res = lj('review', 'runs', 'R1-real-with-note.json')
rc = {k: v['Result'] for k, v in real['Checks'].items()}
nc = {k: v['Result'] for k, v in nc2['Checks'].items()}
print('Real Classification/Disposition/FailureClass:', real['Classification'], real['Disposition'], real['FailureClass'])
print('nc2  Classification/Disposition/FailureClass:', nc2['Classification'], nc2['Disposition'], nc2['FailureClass'])
print('Mismas claves de comprobacion:', sorted(rc) == sorted(nc))
print('Comprobaciones que difieren real -> nc2:', {k: (rc.get(k), nc.get(k)) for k in sorted(set(rc) | set(nc)) if rc.get(k) != nc.get(k)})
print('RedPart real/nc2:', {k: (real['Checks'][k].get('RedPart'), nc2['Checks'][k].get('RedPart')) for k in rc if 'RedPart' in real['Checks'][k] or 'RedPart' in nc2['Checks'][k]})
print('TriggeredStopConditions nc2:', nc2.get('TriggeredStopConditions'))
print('git diff (sustituto) == mutation.DiffBaseToCurrent:', sorted(res['ChangedPaths']) == sorted(mut['Mutation']['DiffBaseToCurrent']))
print('mutation.New == MutatedAllowedWriteScope:', mut['Mutation']['New'] == res['MutatedAllowedWriteScope'])

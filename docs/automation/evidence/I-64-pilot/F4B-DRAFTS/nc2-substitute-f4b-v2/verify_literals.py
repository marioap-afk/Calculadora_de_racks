"""Comprueba que los dos literales de F4B_EXACT_EVIDENCES son, byte a byte, ExpectedEvidence[0] de los contratos de PB-T2a y PB-T3a (copias en data/) y del contrato de 485b5651.
Uso: python -I verify_literals.py <dir-v2>"""
import importlib.util, json, os, sys

D = sys.argv[1]
spec = importlib.util.spec_from_file_location('sub', os.path.join(D, 'nc2_exact_whitelist_substitute_f4b_v2.py'))
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)
ld = lambda p: json.loads(open(os.path.join(D, p), 'rb').read().decode('utf-8'))
t2 = ld('data/pbt2a/real/inputs/gate-contract.json')['ExpectedEvidence']
t3 = ld('data/pbt3a/real/inputs/gate-contract.json')['ExpectedEvidence']
t3git = ld('data/pbt3a-gate-contract-485b5651.json')['ExpectedEvidence']
lits = m.F4B_EXACT_EVIDENCES
print('tupla de', len(lits), 'literales; tipos', [type(x).__name__ for x in lits])
print('PB-T2 == ExpectedEvidence[0] del contrato real de PB-T2a:', lits[0].encode('utf-8') == t2[0].encode('utf-8'))
print('PB-T3 == ExpectedEvidence[0] del contrato real de PB-T3a (copia):', lits[1].encode('utf-8') == t3[0].encode('utf-8'))
print('PB-T3 == ExpectedEvidence[0] del contrato de 485b5651 (git show):', lits[1].encode('utf-8') == t3git[0].encode('utf-8'))
print('contratos de PB-T3a (copia y git) iguales:', t3 == t3git)
print('ASCII:', all(ord(c) < 128 for l in lits for c in l))
print('literales distintos:', lits[0] != lits[1])
ok = lits[0] == t2[0] and lits[1] == t3[0] == t3git[0]
print('OK' if ok else 'FALLA')
sys.exit(0 if ok else 1)

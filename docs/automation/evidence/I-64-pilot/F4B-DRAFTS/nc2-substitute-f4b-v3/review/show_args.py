"""Revision v3 (solo lectura): imprime los argumentos de cada corrida registrada del sustituto (substitute-result.json).
Uso: python -I -B show_args.py <substitute-result.json> [...]"""
import json, sys

for p in sys.argv[1:]:
    r = json.load(open(p, encoding='utf-8'))
    print('==', p)
    for k in ('Id', 'Task', 'Mode', 'ScriptSha256', 'RealVerification', 'Nc2Run', 'BaseSha', 'CurrentSha', 'SubstituteResult'):
        print('  %-18s %s' % (k, r.get(k)))
    for k in ('InjectNote', 'RawControllerOutput'):
        print('  %-18s %s' % (k, r.get(k)))
    m = r.get('Measured') or {}
    print('  Alt...            ', m.get('AlternativeCandidatesExcludingPath'), '| ChangedPath', m.get('ChangedPath'))
    print('  len(RealAWS)      ', len(r.get('RealAllowedWriteScope') or []), '| ChangedPaths', len(r.get('ChangedPaths') or []))
    print('  Properties        ', r.get('Properties'))

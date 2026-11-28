"""Recomprobacion: busca claves duplicadas (a cualquier profundidad) en los JSON reales de la cadena F4B-E21."""
import json, os, sys

ROOT = sys.argv[1]
dups = {}


def hook(pairs):
    seen = set()
    for k, _ in pairs:
        if k in seen:
            dups.setdefault(CUR, []).append(k)
        seen.add(k)
    return dict(pairs)


for rel in ['data/planning/output.json', 'data/planning/session-inject-note.json', 'data/real/inputs/delegation.json', 'data/nc2/inputs/delegation.json',
            'data/real/inputs/gate-contract.json', 'data/real/inputs/byte-fidelity-check.json', 'data/real/inputs/relay-record-planning.json',
            'data/nc2/inputs/relay-record-planning.json']:
    CUR = rel
    with open(os.path.join(ROOT, rel), 'rb') as f:
        json.loads(f.read().decode('utf-8'), object_pairs_hook=hook)
print(json.dumps({'DuplicateKeys': dups, 'AnyDuplicate': bool(dups)}, ensure_ascii=False))

# Negative controls of the shadow QH (record_version 3): literal replacements on the committed point, each caught by its invariants.
import json, os
NL = chr(10)
S = os.path.dirname(os.path.abspath(__file__))
neg = [
    {"Case": "NEG-QH-1 titular HELD en un QH", "Scope": "File", "Expect": ["I-S04"],
     "Find": '  principal:' + NL + '    state: RELEASED' + NL, "Replace": '  principal:' + NL + '    state: HELD' + NL},
    {"Case": "NEG-QH-2 binding cambiado fuera de QR o del QU de su aceptación", "Scope": "Pair", "Expect": ["I-P07"],
     "Find": 'docs/automation/evidence/I-66-R1/qu/B20261029T141351Z-af6a.json' + NL + '      blob: 88a734a26e60edc91bb0e9705c63e0d266ad95bf',
     "Replace": 'docs/automation/evidence/I-66-R1/bootstrap/B20261029T141351Z-af6a.json' + NL + '      blob: 0d74802326123bcf9ae4f321f874fdcc5acaa3ca'},
    {"Case": "NEG-QH-3 attempts que crece en un QH", "Scope": "Pair", "Expect": ["I-P05"],
     "Find": '  attempts: 0' + NL, "Replace": '  attempts: 1' + NL},
    {"Case": "NEG-QH-4 g0_acceptance decidida fuera de un QU", "Scope": "Pair", "Expect": ["I-P09"],
     "Find": '  g0_acceptance:' + NL + '    state: PENDING' + NL + '    decision: null' + NL,
     "Replace": '  g0_acceptance:' + NL + '    state: REJECTED' + NL + '    decision:' + NL + '      path: docs/automation/decisions/I-66.md' + NL
                + '      blob: 8c155609a31475d4b8bdd2068de7531446ca8c96' + NL},
    {"Case": "NEG-QH-5 handoff citado con un blob que no está en el árbol", "Scope": "File", "Expect": ["I-S13"],
     "Find": 'blob: 49bdde414a8a92c9f1b4e7bb69fca4016f07ede9', "Replace": 'blob: 49bdde414a8a92c9f1b4e7bb69fca4016f07ede0'},
]
json.dump(neg, open(os.path.join(S, 'negatives-qh.json'), 'w', encoding='utf-8', newline=NL), ensure_ascii=False, indent=1)
print('ok', len(neg))

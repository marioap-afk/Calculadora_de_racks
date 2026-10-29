# Negative controls of the shadow QU (record_version 2): literal replacements on the committed point, each caught by its invariants.
import json, os
NL = chr(10)
S = os.path.dirname(os.path.abspath(__file__))
DEC_BLOB = 'b9b3488248cdb8ca4c99205bb82ae5578ed15c63'
g0_pending = '  g0_acceptance:' + NL + '    state: PENDING' + NL + '    decision: null' + NL
g0_accepted = ('  g0_acceptance:' + NL + '    state: ACCEPTED' + NL + '    decision:' + NL + '      path: docs/automation/decisions/I-66.md' + NL
               + '      blob: ' + DEC_BLOB + NL)
neg = [
    {"Case": "NEG-QU-1 g0_acceptance ACCEPTED sin I62-CLASSIFICATION en el G0", "Scope": "File", "Expect": ["I-S16"], "Find": g0_pending, "Replace": g0_accepted},
    {"Case": "NEG-QU-2 el mismo G0 aceptado como transición (marcadores I62 de I-P09)", "Scope": "Pair", "Expect": ["I-P09"], "Find": g0_pending, "Replace": g0_accepted},
    {"Case": "NEG-QU-3 record_version que salta (3 en vez de 2)", "Scope": "Pair", "Expect": ["I-P01"],
     "Find": '  record_version: 2' + NL, "Replace": '  record_version: 3' + NL},
    {"Case": "NEG-QU-4 aceptación del Principal sin su marcador (decisiones del BOOTSTRAP)", "Scope": "Pair", "Expect": ["I-P09"],
     "Find": '        blob: ' + DEC_BLOB + NL, "Replace": '        blob: 5c6a57e82305ee95fcaa0b77f95d165032b4c29b' + NL},
    {"Case": "NEG-QU-5 Q0 con g0_acceptance PENDING", "Scope": "File", "Expect": ["I-S15"],
     "Find": '  point: QU' + NL + '  point_kind: ORDINARY' + NL, "Replace": '  point: Q0' + NL + '  point_kind: null' + NL},
]
json.dump(neg, open(os.path.join(S, 'negatives-qu.json'), 'w', encoding='utf-8', newline=NL), ensure_ascii=False, indent=1)
print('ok', len(neg))

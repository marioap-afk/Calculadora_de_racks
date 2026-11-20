"""Renombra en la copia exportada las dos sobrecargas nuevas del esqueleto de PB-T3b (decision del Owner F4B-E45):
Execute(DraftSession<PushBackFullDraftState>, ...) -> ExecuteFull y ComparePost(plan, PushBackFullIntentResult?) -> ComparePostFull.
Cada sustitucion debe aplicar exactamente una vez. Uso: python -I rename_overloads.py <raiz-de-la-copia>"""
import os, sys
root = sys.argv[1]
p = os.path.join(root, 'src', 'RackCad.Application', 'Systems', 'PushBack', 'Pilot', 'PushBackPilotUpdateRun.cs')
b = open(p, 'rb').read()
t = b.decode('utf-8')
subs = [
    ('public static DraftApplyOutcome<PushBackFullDraftState> Execute(', 'public static DraftApplyOutcome<PushBackFullDraftState> ExecuteFull('),
    ('public static string? ComparePost(PushBackPilotUpdatePlan plan, PushBackFullIntentResult? postRead)', 'public static string? ComparePostFull(PushBackPilotUpdatePlan plan, PushBackFullIntentResult? postRead)'),
]
for old, new in subs:
    n = t.count(old)
    assert n == 1, (old, n)
    t = t.replace(old, new)
open(p, 'wb').write(t.encode('utf-8'))
print('ok', p.replace(root, '<copia>'), len(b), '->', len(t.encode('utf-8')))

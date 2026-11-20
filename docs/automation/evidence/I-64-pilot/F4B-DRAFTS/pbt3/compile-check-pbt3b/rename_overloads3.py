"""Renombra en la copia exportada las TRES sobrecargas nuevas del esqueleto de PB-T3b (F4B-E45 con el barrido ampliado de la v4.6):
Prepare(PushBackFullPrepareInput, ...) -> PrepareFull (PP), Execute(DraftSession<PushBackFullDraftState>, ...) -> ExecuteFull y
ComparePost(plan, PushBackFullIntentResult?) -> ComparePostFull (UR). Cada sustitucion aplica exactamente una vez.
Uso: python -I rename_overloads3.py <raiz-de-la-copia>"""
import os, sys
root = sys.argv[1]
pilot = os.path.join(root, 'src', 'RackCad.Application', 'Systems', 'PushBack', 'Pilot')
plan = [
    ('PushBackPilotUpdatePlan.cs', 'public static PushBackPilotPrepareResult Prepare(PushBackFullPrepareInput input', 'public static PushBackPilotPrepareResult PrepareFull(PushBackFullPrepareInput input'),
    ('PushBackPilotUpdateRun.cs', 'public static DraftApplyOutcome<PushBackFullDraftState> Execute(', 'public static DraftApplyOutcome<PushBackFullDraftState> ExecuteFull('),
    ('PushBackPilotUpdateRun.cs', 'public static string? ComparePost(PushBackPilotUpdatePlan plan, PushBackFullIntentResult? postRead)', 'public static string? ComparePostFull(PushBackPilotUpdatePlan plan, PushBackFullIntentResult? postRead)'),
]
for name, old, new in plan:
    p = os.path.join(pilot, name)
    t = open(p, 'rb').read().decode('utf-8')
    n = t.count(old)
    assert n == 1, (name, old, n)
    open(p, 'wb').write(t.replace(old, new).encode('utf-8'))
    print('ok', name, old.split('(')[0].split()[-1], '->', new.split('(')[0].split()[-1])

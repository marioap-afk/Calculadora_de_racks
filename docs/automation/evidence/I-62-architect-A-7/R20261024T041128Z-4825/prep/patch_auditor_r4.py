"""Paso B del auditor post-review.py v7.0-a7r4 (decisiones §98, punto 7) sobre la salida del paso A: la cabecera de la ronda 4 (con el párrafo de
v7.0-a7r3 tomado SIN desplazar de base3/, como historia), las claves del Paso 0 de la ronda 4 (lectura directa de RAE §10 L219-L226) y las citas de
la orden: el dictamen de compatibilidad sobre decisiones §98, punto 4 («Conservar A-5, A-6, EFF/EFF^1, oráculos y líneas Owner intactos»); el
mecanismo de referencias conserva su origen (decisiones §96, punto 6). Las reglas no cambian. Uso: python -I -B patch_auditor_r4.py
"""
import hashlib, os

PREP = os.path.dirname(os.path.abspath(__file__))
SH, BASE3, KIT = os.path.join(PREP, 'shifted'), os.path.join(os.path.dirname(PREP), 'base3'), os.path.join(os.path.dirname(PREP), 'kit')
t = open(os.path.join(SH, 'post-review.py'), encoding='utf-8', newline='').read()
b3 = open(os.path.join(BASE3, 'post-review.py'), encoding='utf-8', newline='').read()
# el párrafo de v7.0-a7r3 tal como lo escribió la ronda 3 (base3, sin desplazar)
s3 = b3.index('v7.0-a7r3 = the v6.0-a7r2 auditor (text below) with:')
e3 = b3.index('Text of v6.0-a7r2 (with round 2\'s numbers):')
hist = b3[s3:e3]
s = t.index('v7.0-a7r4 = the v6.0-a7r3 auditor (text below) with:')
e = t.index('Text of v6.0-a7r3 (with round 3\'s numbers):')
NEW = '''v7.0-a7r4 = the v7.0-a7r3 auditor (its text below, shifted one round by prep/shift_r4.py) with: the round-4 schema
rackcad-a7r4-architect-review/0 (PriorFindingResolution of A62-A7R3-R1; the eleven changes of A-7 §3.8 in DeltaPurity; RegressionCheck of the twelve
topics that round 3 found corrected or conforming; CompatibilityVerdicts A5 and A6, now on decisions §98, point 4: «Conservar A-5, A-6, EFF/EFF^1,
oráculos y líneas Owner intactos»; Q-A7R4-01..03); the closure's DirectRead is RAE §10 L219-L226 (the negative controls run on copies of every input
of the VERIFIED verification of the last WorkRunId, with the relative oracle); the round-4 Step 0 (STEP0_KEYS); gate 11 after §98; and the premise
a7-r3.md (the round-3 text). The reference mechanism of decisions §96, point 6 (schema families, exactness of the written path, presence in the
closure, faithful delivery) and every other rule are unchanged.

Paragraph of v7.0-a7r3, as round 3 wrote it (round-3 numbers):
''' + hist
# el resto del docstring (la historia de v6.0-a7r2 y anteriores), literal de la ronda 3, sin desplazar
d3 = b3.index('"""', e3)
dt = t.index('"""', e)
t = t[:s] + NEW + b3[e3:d3] + t[dt:]
R = [
    ('consequences (decisions §98, points 2 and 6), by an independent ARCHITECT.', 'consequences (decisions §98, points 2 and 7), by an independent ARCHITECT.', 1),
    ('# v7.0-a7r4: the reference as written and why (decisions §98, point 6); the rule itself is unchanged',
     '# v7.0-a7r3: the reference as written and why (decisions §96, point 6); the rule itself is unchanged', 1),
    ('''STEP0_KEYS = ("AP167DirectReadL593L595", "OrderReadWhole", "Decisions98DeclaresOrderBytesAndSha256",
              "Decisions98AcceptsR3R1AndAuthorizesThirdCandidate", "GuardsRunModeBaseHeadResult", "PreflightSummaryCandidateAndTotals",
              "Round2OutputIdentifiesRunAndFindings")''',
     '''STEP0_KEYS = ("RAE10DirectReadL219L226", "OrderReadWhole", "Decisions98DeclaresOrderBytesAndSha256",
              "Decisions98AcceptsR3R1AndAuthorizesFourthCandidate", "GuardsRunModeBaseHeadResult", "PreflightSummaryCandidateAndTotals",
              "Round3OutputIdentifiesRunAndFindings")''', 1),
    ('# v7.0-a7r4: the path as written = the closure path or the premise name', '# v7.0-a7r3: the path as written = the closure path or the premise name', 1),
    ('INCOMPATIBLE sin un REQUIRED vinculado (decisiones §98, punto 6)', 'INCOMPATIBLE sin un REQUIRED vinculado (decisiones §98, punto 4)', 1),
    ('AGREED exige A-5 y A-6 COMPATIBLE (CompatibilityVerdicts, decisiones §98, punto 6)', 'AGREED exige A-5 y A-6 COMPATIBLE (CompatibilityVerdicts, decisiones §98, punto 4)', 1),
]
for i, (old, new, count) in enumerate(R):
    n = t.count(old)
    if n != count:
        raise SystemExit('edición %d: %d apariciones (se esperaban %d): %r' % (i, n, count, old[:90]))
    t = t.replace(old, new)
b = t.encode('utf-8')
assert b'\r' not in b
open(os.path.join(KIT, 'post-review.py'), 'wb').write(b)
print('post-review.py', hashlib.sha256(b).hexdigest())

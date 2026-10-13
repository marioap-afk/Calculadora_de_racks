# Derives make_candidate_commit_a6r3.py and neg_controls_a6r3.py (A-6 round 3) from the round-2 files by exact substitutions.
import os, subprocess
D = os.path.dirname(os.path.abspath(__file__))
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')


def src(name):
    return subprocess.run(['git', '-C', WT, 'show', 'HEAD:docs/automation/evidence/I-62-A6/r2/' + name], capture_output=True).stdout.replace(
        b'\r\n', b'\n').decode('utf-8')


def apply(t, pairs):
    for old, new in pairs:
        c = t.count(old)
        assert c == 1, (old[:70], c)
        t = t.replace(old, new)
    return t


mk = apply(src('make_candidate_commit_a6r2.py'), [
    ('"""I-62 A-6 ronda 2, preflight: commit candidato HIPOTÉTICO de la ronda 2, construido solo en un clon temporal compartido bajo D:/r62-a6r2/guards.\n\nAdaptado de docs/automation/evidence/I-62-A6/make_candidate_commit_a6.py (ronda 1, sin cambios).',
     '"""I-62 A-6 ronda 3, preflight: commit candidato HIPOTÉTICO de la ronda 3, construido solo en un clon temporal compartido bajo D:/r62-a6r3/guards.\n\nAdaptado de docs/automation/evidence/I-62-A6/r2/make_candidate_commit_a6r2.py (ronda 2, sin cambios) con adapt_neg_r3.py.'),
    ('commit-tree). El commit MODIFICA docs/initiatives/I-62-A-6.md (de la ronda 1 a la ronda 2) y AÑADE bajo docs/automation/evidence/I-62-A6/r2/ las',
     'commit-tree). El commit MODIFICA docs/initiatives/I-62-A-6.md (de la ronda 2 a la ronda 3) y AÑADE bajo docs/automation/evidence/I-62-A6/r3/ las'),
    ('Uso: python -I -B make_candidate_commit_a6r2.py --dest D:/r62-a6r2/guards/<dir> --out <json> --a6 <texto> --selftest <json>\n                                                [--guards <a6r2-guards.py>]',
     'Uso: python -I -B make_candidate_commit_a6r3.py --dest D:/r62-a6r3/guards/<dir> --out <json> --a6 <texto> --selftest <json>\n                                                [--guards <a6r3-guards.py>]'),
    ('TMP_ROOT = "D:/r62-a6r2/guards"', 'TMP_ROOT = "D:/r62-a6r3/guards"'),
    ('EVID_R2 = EVID + "r2/"\nR1_BLOB = "eac473c0d2e58cf1c4594f48a8816b3833af6509"',
     'EVID_R3 = EVID + "r3/"\nPREV_BLOB = "86da0ae770e683a647f2724860a4bd890061154d"   # A-6 ronda 2'),
    ('"docs/automation/evidence/I-62-architect-A-6"]', '"docs/automation/evidence/I-62-architect-A-6", "docs/automation/evidence/I-62-architect-A-6r2",\n             "docs/automation/evidence/I-62-A6/r2"]'),
    ('    ap.add_argument("--guards", default=os.path.join(HERE, "a6r2-guards.py"))', '    ap.add_argument("--guards", default=os.path.join(HERE, "a6r3-guards.py"))'),
    ('        raise SystemExit("destino fuera de D:/r62-a6r2/guards")', '        raise SystemExit("destino fuera de D:/r62-a6r3/guards")'),
    ('    assert git("rev-parse", base + ":" + A6).strip() == R1_BLOB, "la base no contiene la ronda 1 de A-6"\n    files = {A6: a.a6, EVID_R2 + "a6r2-guards.py": a.guards, EVID_R2 + "a6r2-selftest.json": a.selftest}',
     '    assert git("rev-parse", base + ":" + A6).strip() == PREV_BLOB, "la base no contiene la ronda 2 de A-6"\n    files = {A6: a.a6, EVID_R3 + "a6r3-guards.py": a.guards, EVID_R3 + "a6r3-selftest.json": a.selftest}'),
    ('    env = {**os.environ, "GIT_INDEX_FILE": idx, "GIT_AUTHOR_NAME": "a6r2-preflight", "GIT_AUTHOR_EMAIL": "fixture@example.invalid",\n           "GIT_COMMITTER_NAME": "a6r2-preflight", "GIT_COMMITTER_EMAIL": "fixture@example.invalid",\n           "GIT_AUTHOR_DATE": "2026-10-12T09:00:00+00:00", "GIT_COMMITTER_DATE": "2026-10-12T09:00:00+00:00"}',
     '    env = {**os.environ, "GIT_INDEX_FILE": idx, "GIT_AUTHOR_NAME": "a6r3-preflight", "GIT_AUTHOR_EMAIL": "fixture@example.invalid",\n           "GIT_COMMITTER_NAME": "a6r3-preflight", "GIT_COMMITTER_EMAIL": "fixture@example.invalid",\n           "GIT_AUTHOR_DATE": "2026-10-13T09:00:00+00:00", "GIT_COMMITTER_DATE": "2026-10-13T09:00:00+00:00"}'),
    ('data="I-62 A-6 ronda 2 (HIPOTÉTICO, solo preflight): candidata y guardas\\n"', 'data="I-62 A-6 ronda 3 (HIPOTÉTICO, solo preflight): candidata y guardas\\n"'),
    ('    r1ev = [s for s in status if s[-1].startswith(EVID) and not s[-1].startswith(EVID_R2)]', '    r1ev = [s for s in status if s[-1].startswith(EVID) and not s[-1].startswith(EVID_R3)]'),
    ('           "OnlyA6ModifiedAndR2Added": all((s[-1] == A6 and s[0] == "M") or (s[-1].startswith(EVID_R2) and s[0] == "A") for s in status),\n           "ForbiddenDiff": prod.splitlines(), "ForbiddenDiffEmpty": prod == "", "Round1EvidenceTouched": [s[-1] for s in r1ev],\n           "Note": "clon --bare --shared temporal bajo D:/r62-a6r2/guards; sin push; el worktree de I-62 no se escribe"}',
     '           "OnlyA6ModifiedAndR3Added": all((s[-1] == A6 and s[0] == "M") or (s[-1].startswith(EVID_R3) and s[0] == "A") for s in status),\n           "ForbiddenDiff": prod.splitlines(), "ForbiddenDiffEmpty": prod == "", "OtherRoundsEvidenceTouched": [s[-1] for s in r1ev],\n           "Note": "clon --bare --shared temporal bajo D:/r62-a6r3/guards; sin push; el worktree de I-62 no se escribe"}'),
    ('    print(json.dumps({k: out[k] for k in ("Candidate", "Files", "OnlyA6ModifiedAndR2Added", "ForbiddenDiffEmpty")}))\n    sys.exit(0 if out["ForbiddenDiffEmpty"] and out["OnlyA6ModifiedAndR2Added"] else 1)',
     '    print(json.dumps({k: out[k] for k in ("Candidate", "Files", "OnlyA6ModifiedAndR3Added", "ForbiddenDiffEmpty")}))\n    sys.exit(0 if out["ForbiddenDiffEmpty"] and out["OnlyA6ModifiedAndR3Added"] else 1)'),
])
open(os.path.join(D, 'make_candidate_commit_a6r3.py'), 'w', encoding='utf-8', newline='\n').write(mk)

nc = apply(src('neg_controls_a6r2.py'), [
    ('"""I-62 A-6 ronda 2, preflight: controles de extremo a extremo de a6r2-guards.py (por la línea de órdenes, no en memoria).\n\nAdaptado de docs/automation/evidence/I-62-A6/neg_controls_a6.py (ronda 1, sin cambios).',
     '"""I-62 A-6 ronda 3, preflight: controles de extremo a extremo de a6r3-guards.py (por la línea de órdenes, no en memoria).\n\nAdaptado de docs/automation/evidence/I-62-A6/r2/neg_controls_a6r2.py (ronda 2, sin cambios) con adapt_neg_r3.py.'),
    ('D:/r62-a6r2/guards/neg, que se borra al terminar. Sin red, sin modelos.\nUso: python -I -B neg_controls_a6r2.py',
     'D:/r62-a6r3/guards/neg, que se borra al terminar. Sin red, sin modelos.\nUso: python -I -B neg_controls_a6r3.py'),
    ('GUARDS = os.path.join(HERE, "a6r2-guards.py")\nMAKE = os.path.join(HERE, "make_candidate_commit_a6r2.py")\nTMP = "D:/r62-a6r2/guards/neg"',
     'GUARDS = os.path.join(HERE, "a6r3-guards.py")\nMAKE = os.path.join(HERE, "make_candidate_commit_a6r3.py")\nTMP = "D:/r62-a6r3/guards/neg"'),
    ('     lambda t: sub1(t, "`docs/automation/evidence/I-62-architect-A-6/` (la revisión de la ronda 1) y ", "")),',
     '     lambda t: sub1(t, "`docs/automation/evidence/I-62-architect-A-6/` (la revisión de la ronda 1), ", "")),'),
    ('    ("N-05", "A6-2 cambia respecto de la ronda 1", ["G5"],', '    ("N-05", "A6-2 cambia respecto de la ronda 2", ["G5"],'),
    ('    ("N-12", "tabla de A6-5 sin R4 en MATCH", ["G10"], lambda t: sub1(t, "R1, R2, R3, R4, R9 y R10 están en `MATCH`", "R1, R2, R3, R9 y R10 están en `MATCH`")),',
     '    ("N-12", "tabla de A6-5 sin la condición de R4 sobre `Verification`", ["G10"],\n     lambda t: sub1(t, "R4 está en `MATCH` si la solicitud lleva `Verification`, y en `NOT_APPLICABLE` solo si la regla de A-6 en D.6 la dejó fuera; y R5, R6 y R7 están",\n                    "y R4, R5, R6 y R7 están")),'),
    ('    ("N-15", "cabecera con la ronda 2 lanzada", ["G1", "G8"], lambda t: sub1(t, "Ronda 2: NOT LAUNCHED.", "Ronda 2: LAUNCHED.")),\n]',
     '    ("N-15", "cabecera con la ronda 3 lanzada", ["G1", "G8"], lambda t: sub1(t, "Ronda 3:\\nNOT LAUNCHED.", "Ronda 3:\\nLAUNCHED.")),\n'
     '    ("N-16", "§3.17 sin la fila R2 (a)", ["G4"], lambda t: resub1(t, r"^\\| R2 \\(a\\): [^\\n]*\\n", "")),\n'
     '    ("N-17", "A6-3, regla 6 (c): condición de R4 con Authority en lugar de FreeText", ["G10"],\n'
     '     lambda t: sub1(t, "`Trailer` y `FreeText` está en `not_run`", "`Trailer` y `Authority` está en `not_run`")),\n'
     '    ("N-18", "A6-5, regla 7: el instrumento obligatorio en toda invocación VERIFY", ["G5", "G10"],\n'
     '     lambda t: sub1(t, "Ninguna está obligada a ejecutar el instrumento", "Toda invocación ejecuta el instrumento")),\n'
     '    ("N-19", "A6-1 cambia respecto de la ronda 2", ["G5"], lambda t: sub1(t, "no es un lanzamiento.", "no es un lanzamiento nuevo.")),\n'
     '    ("N-20", "línea «Cubre» de A6-5 sin O5 de la ronda 2", ["G4"], lambda t: sub1(t, "R1, R2 y O5 de la ronda 2.", "R1 y R2 de la ronda 2.")),\n]'),
    ('    ("N-26", "commit candidato que además añade un archivo a I-62-A6/ fuera de r2/", ["G2", "G3"], "docs/automation/evidence/I-62-A6/nuevo.md"),\n]',
     '    ("N-26", "commit candidato que además añade un archivo a I-62-A6/ fuera de r3/", ["G2", "G3"], "docs/automation/evidence/I-62-A6/nuevo.md"),\n'
     '    ("N-27", "commit candidato que además modifica un archivo de r2/ (evidencia de la ronda 2)", ["G2", "G3"], "docs/automation/evidence/I-62-A6/r2/preflight.md"),\n'
     '    ("N-28", "commit candidato que además modifica la revisión de la ronda 2", ["G2", "G3"],\n'
     '     "docs/automation/evidence/I-62-architect-A-6r2/R20261012T195511Z-e5c3/output.json"),\n]'),
    ('R2_PLACEHOLDERS = ["docs/automation/evidence/I-62-A6/r2/r1r4/README.md", "docs/automation/evidence/I-62-A6/r2/independence/README.md"]',
     'R2_PLACEHOLDERS = ["docs/automation/evidence/I-62-A6/r3/r1probes/README.md", "docs/automation/evidence/I-62-A6/r3/r2static/README.md",\n                   "docs/automation/evidence/I-62-A6/r3/launchfix/README.md"]'),
    ('    """Commit candidato hipotético con A-6, las guardas, la autoprueba y un marcador por cada carpeta de evidencia r2/ que A-6 nombra\n    (r1r4/ e independence/; la evidencia real la aporta el paquete r2/, no este preflight)."""',
     '    """Commit candidato hipotético con A-6, las guardas, la autoprueba y un marcador por cada carpeta de evidencia r3/ que A-6 nombra\n    (r1probes/, r2static/ y launchfix/; la evidencia real la aporta el paquete r3/, no este preflight)."""'),
    ('            fh.write("# Marcador del preflight de las guardas de la ronda 2 (no es evidencia)\\n")', '            fh.write("# Marcador del preflight de las guardas de la ronda 3 (no es evidencia)\\n")'),
    ('    rows.append({"Id": "P-1", "Case": "texto conforme en un commit candidato hipotético (A-6 modificada y r2/ añadido)", "Expected": "PASS",',
     '    rows.append({"Id": "P-1", "Case": "texto conforme en un commit candidato hipotético (A-6 modificada y r3/ añadido)", "Expected": "PASS",'),
    ('    rows.append({"Id": "D-1", "Case": "el borrador tal como está, en un commit candidato y en modo run", "Expected": ["G4", "G8", "G10", "G5B"],\n                 "Result": res["Result"], "FailingGuards": fl, "FirstFinding": first(res, fl),\n                 "AsExpected": rc == 1 and res["Result"] == "FAIL" and {"G4", "G8", "G10", "G5B"} <= set(fl)})',
     '    rows.append({"Id": "D-1", "Case": "el borrador con marcadores sin rellenar, en un commit candidato y en modo run", "Expected": ["G8"],\n                 "Result": res["Result"], "FailingGuards": fl, "FirstFinding": first(res, fl),\n                 "AsExpected": rc == 1 and res["Result"] == "FAIL" and {"G8"} <= set(fl)})'),
    ('            fh.write(cur + b"\\nlinea negativa del preflight de la ronda 2 de A-6\\n")', '            fh.write(cur + b"\\nlinea negativa del preflight de la ronda 3 de A-6\\n")'),
    ('    out = {"Tool": "neg_controls_a6r2.py",', '    out = {"Tool": "neg_controls_a6r3.py",'),
])
open(os.path.join(D, 'neg_controls_a6r3.py'), 'w', encoding='utf-8', newline='\n').write(nc)
for n in ('make_candidate_commit_a6r3.py', 'neg_controls_a6r3.py'):
    compile(open(os.path.join(D, n), encoding='utf-8').read(), n, 'exec')
print('ok')

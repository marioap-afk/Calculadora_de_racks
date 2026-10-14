# Derives make_candidate_commit_a6r4.py and neg_controls_a6r4.py (A-6 round 4, decisiones §85) from the round-3 files
# (r3/make_candidate_commit_a6r3.py, blob aa32fd3f; r3/neg_controls_a6r3.py, blob ba3d27de) by exact substitutions, each matching once.
# Read-only on the I-62 worktree (git show of its HEAD); writes only the two derived files next to this script.
import hashlib, os, subprocess
D = os.path.dirname(os.path.abspath(__file__))
WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-portabilidad-coordinador-principal')
ENV = dict(os.environ, GIT_NO_REPLACE_OBJECTS='1', GIT_OPTIONAL_LOCKS='0')
BLOBS = {'make_candidate_commit_a6r3.py': 'aa32fd3f9bd16323c7a72010b27a198dcade87ff', 'neg_controls_a6r3.py': 'ba3d27de597ce65d92ee60edc2dce42f3c9803cc'}


def src(name):
    raw = subprocess.run(['git', '-C', WT, 'show', 'HEAD:docs/automation/evidence/I-62-A6/r3/' + name], capture_output=True, env=ENV,
                         check=True).stdout.replace(b'\r\n', b'\n')
    assert hashlib.sha1(b'blob %d\x00' % len(raw) + raw).hexdigest() == BLOBS[name], name
    return raw.decode('utf-8')


def apply(t, pairs):
    for old, new in pairs:
        c = t.count(old)
        assert c == 1, (old[:70], c)
        t = t.replace(old, new)
    return t


mk = apply(src('make_candidate_commit_a6r3.py'), [
    ('"""I-62 A-6 ronda 3, preflight: commit candidato HIPOTÉTICO de la ronda 3, construido solo en un clon temporal compartido bajo D:/r62-a6r3/guards.\n\nAdaptado de docs/automation/evidence/I-62-A6/r2/make_candidate_commit_a6r2.py (ronda 2, sin cambios) con adapt_neg_r3.py.',
     '"""I-62 A-6 ronda 4, preflight: commit candidato HIPOTÉTICO de la ronda 4, construido solo en un clon temporal compartido bajo D:/r62-a6r4/guards.\n\nAdaptado de docs/automation/evidence/I-62-A6/r3/make_candidate_commit_a6r3.py (ronda 3, sin cambios) con adapt_neg_r4.py.'),
    ('commit-tree). El commit MODIFICA docs/initiatives/I-62-A-6.md (de la ronda 2 a la ronda 3) y AÑADE bajo docs/automation/evidence/I-62-A6/r3/ las',
     'commit-tree). El commit MODIFICA docs/initiatives/I-62-A-6.md (de la ronda 3 a la ronda 4) y AÑADE bajo docs/automation/evidence/I-62-A6/r4/ las'),
    ('Uso: python -I -B make_candidate_commit_a6r3.py --dest D:/r62-a6r3/guards/<dir> --out <json> --a6 <texto> --selftest <json>\n                                                [--guards <a6r3-guards.py>]',
     'Uso: python -I -B make_candidate_commit_a6r4.py --dest D:/r62-a6r4/guards/<dir> --out <json> --a6 <texto> --selftest <json>\n                                                [--guards <a6r4-guards.py>]'),
    ('TMP_ROOT = "D:/r62-a6r3/guards"', 'TMP_ROOT = "D:/r62-a6r4/guards"'),
    ('EVID_R3 = EVID + "r3/"\nPREV_BLOB = "86da0ae770e683a647f2724860a4bd890061154d"   # A-6 ronda 2',
     'EVID_R4 = EVID + "r4/"\nPREV_BLOB = "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1"   # A-6 ronda 3'),
    ('"docs/automation/evidence/I-62-architect-A-6", "docs/automation/evidence/I-62-architect-A-6r2",\n             "docs/automation/evidence/I-62-A6/r2"]',
     '"docs/automation/evidence/I-62-architect-A-6", "docs/automation/evidence/I-62-architect-A-6r2",\n             "docs/automation/evidence/I-62-architect-A-6r3", "docs/automation/evidence/I-62-A6/r2", "docs/automation/evidence/I-62-A6/r3"]'),
    ('    ap.add_argument("--guards", default=os.path.join(HERE, "a6r3-guards.py"))', '    ap.add_argument("--guards", default=os.path.join(HERE, "a6r4-guards.py"))'),
    ('        raise SystemExit("destino fuera de D:/r62-a6r3/guards")', '        raise SystemExit("destino fuera de D:/r62-a6r4/guards")'),
    ('    assert git("rev-parse", base + ":" + A6).strip() == PREV_BLOB, "la base no contiene la ronda 2 de A-6"\n    files = {A6: a.a6, EVID_R3 + "a6r3-guards.py": a.guards, EVID_R3 + "a6r3-selftest.json": a.selftest}',
     '    assert git("rev-parse", base + ":" + A6).strip() == PREV_BLOB, "la base no contiene la ronda 3 de A-6"\n    files = {A6: a.a6, EVID_R4 + "a6r4-guards.py": a.guards, EVID_R4 + "a6r4-selftest.json": a.selftest}'),
    ('    env = {**os.environ, "GIT_INDEX_FILE": idx, "GIT_AUTHOR_NAME": "a6r3-preflight", "GIT_AUTHOR_EMAIL": "fixture@example.invalid",\n           "GIT_COMMITTER_NAME": "a6r3-preflight", "GIT_COMMITTER_EMAIL": "fixture@example.invalid",\n           "GIT_AUTHOR_DATE": "2026-10-13T09:00:00+00:00", "GIT_COMMITTER_DATE": "2026-10-13T09:00:00+00:00"}',
     '    env = {**os.environ, "GIT_INDEX_FILE": idx, "GIT_AUTHOR_NAME": "a6r4-preflight", "GIT_AUTHOR_EMAIL": "fixture@example.invalid",\n           "GIT_COMMITTER_NAME": "a6r4-preflight", "GIT_COMMITTER_EMAIL": "fixture@example.invalid",\n           "GIT_AUTHOR_DATE": "2026-10-14T09:00:00+00:00", "GIT_COMMITTER_DATE": "2026-10-14T09:00:00+00:00"}'),
    ('data="I-62 A-6 ronda 3 (HIPOTÉTICO, solo preflight): candidata y guardas\\n"', 'data="I-62 A-6 ronda 4 (HIPOTÉTICO, solo preflight): candidata y guardas\\n"'),
    ('    r1ev = [s for s in status if s[-1].startswith(EVID) and not s[-1].startswith(EVID_R3)]', '    r1ev = [s for s in status if s[-1].startswith(EVID) and not s[-1].startswith(EVID_R4)]'),
    ('           "OnlyA6ModifiedAndR3Added": all((s[-1] == A6 and s[0] == "M") or (s[-1].startswith(EVID_R3) and s[0] == "A") for s in status),',
     '           "OnlyA6ModifiedAndR4Added": all((s[-1] == A6 and s[0] == "M") or (s[-1].startswith(EVID_R4) and s[0] == "A") for s in status),'),
    ('           "Note": "clon --bare --shared temporal bajo D:/r62-a6r3/guards; sin push; el worktree de I-62 no se escribe"}',
     '           "Note": "clon --bare --shared temporal bajo D:/r62-a6r4/guards; sin push; el worktree de I-62 no se escribe"}'),
    ('    print(json.dumps({k: out[k] for k in ("Candidate", "Files", "OnlyA6ModifiedAndR3Added", "ForbiddenDiffEmpty")}))\n    sys.exit(0 if out["ForbiddenDiffEmpty"] and out["OnlyA6ModifiedAndR3Added"] else 1)',
     '    print(json.dumps({k: out[k] for k in ("Candidate", "Files", "OnlyA6ModifiedAndR4Added", "ForbiddenDiffEmpty")}))\n    sys.exit(0 if out["ForbiddenDiffEmpty"] and out["OnlyA6ModifiedAndR4Added"] else 1)'),
])
open(os.path.join(D, 'make_candidate_commit_a6r4.py'), 'w', encoding='utf-8', newline='\n').write(mk)

OLD_B = ('>    (b) `Repo` = el `-C`; `Tool` = el blob fijado, ejecutado desde el objeto del `-C`; `Interpreter` = el intérprete declarado; `Closure` = el\\n'
         '>    cierre derivado por la regla de A-6 sobre la independencia; `RelayFacts` = los hechos del relevo que cita la verificación, con el SHA-256\\n'
         '>    citado;\\n')

nc = apply(src('neg_controls_a6r3.py'), [
    ('"""I-62 A-6 ronda 3, preflight: controles de extremo a extremo de a6r3-guards.py (por la línea de órdenes, no en memoria).\n\nAdaptado de docs/automation/evidence/I-62-A6/r2/neg_controls_a6r2.py (ronda 2, sin cambios) con adapt_neg_r3.py.',
     '"""I-62 A-6 ronda 4, preflight: controles de extremo a extremo de a6r4-guards.py (por la línea de órdenes, no en memoria).\n\nAdaptado de docs/automation/evidence/I-62-A6/r3/neg_controls_a6r3.py (ronda 3, sin cambios) con adapt_neg_r4.py.'),
    ('  - el borrador tal como está, en un commit candidato y en modo `run`: debe fallar (hallazgos reales y marcadores pendientes).',
     '  - el borrador tal como está, en un commit candidato y en modo `run`: debe fallar (hallazgos reales y marcadores pendientes);\n'
     '  - un marcador pendiente en `preview --final`: debe fallar G8.'),
    ('D:/r62-a6r3/guards/neg, que se borra al terminar. Sin red, sin modelos.\nUso: python -I -B neg_controls_a6r3.py',
     'D:/r62-a6r4/guards/neg, que se borra al terminar. Sin red, sin modelos.\nUso: python -I -B neg_controls_a6r4.py'),
    ('GUARDS = os.path.join(HERE, "a6r3-guards.py")\nMAKE = os.path.join(HERE, "make_candidate_commit_a6r3.py")\nTMP = "D:/r62-a6r3/guards/neg"',
     'GUARDS = os.path.join(HERE, "a6r4-guards.py")\nMAKE = os.path.join(HERE, "make_candidate_commit_a6r4.py")\nTMP = "D:/r62-a6r4/guards/neg"'),
    ('def ins_ev(text, line):',
     'OLD_B = (' + repr(OLD_B.replace('\\n', '\n')) + ')   # apartado (b) de la regla 6 de A6-3 en la ronda 3\n\n\n'
     'def resub1s(text, pat, new):\n'
     '    t, n = re.subn(pat, lambda m: new, text, count=1, flags=re.M | re.S)\n'
     '    assert n == 1, "caso no aplicable: %s" % pat[:50]\n'
     '    return t\n\n\n'
     'def ins_ev(text, line):'),
    ('    ("N-05", "A6-2 cambia respecto de la ronda 2", ["G5"],', '    ("N-05", "A6-2 cambia respecto de la ronda 3", ["G5"],'),
    ('    ("N-15", "cabecera con la ronda 3 lanzada", ["G1", "G8"], lambda t: sub1(t, "Ronda 3:\\nNOT LAUNCHED.", "Ronda 3:\\nLAUNCHED.")),',
     '    ("N-15", "cabecera con la ronda 4 lanzada", ["G1", "G8"], lambda t: sub1(t, "Ronda 4: NOT LAUNCHED.", "Ronda 4: LAUNCHED.")),'),
    ('    ("N-19", "A6-1 cambia respecto de la ronda 2", ["G5"], lambda t: sub1(t, "no es un lanzamiento.", "no es un lanzamiento nuevo.")),\n'
     '    ("N-20", "línea «Cubre» de A6-5 sin O5 de la ronda 2", ["G4"], lambda t: sub1(t, "R1, R2 y O5 de la ronda 2.", "R1 y R2 de la ronda 2.")),\n]',
     '    ("N-19", "A6-1 cambia respecto de la ronda 3", ["G5"], lambda t: sub1(t, "no es un lanzamiento.", "no es un lanzamiento nuevo.")),\n'
     '    ("N-20", "línea «Cubre» de A6-5 sin O5 de la ronda 2", ["G4", "G5"], lambda t: sub1(t, "R1, R2 y O5 de la ronda 2.", "R1 y R2 de la ronda 2.")),\n'
     '    ("N-29", "A6-9, regla 4, alterada (admite otra fuente del cierre)", ["G5"],\n'
     '     lambda t: sub1(t, "con la tabla de A-6 y con operaciones de grafo de Git fijadas,", "con la tabla de A-6, o con la regla 6 (b) de A6-3, y con operaciones de grafo de Git fijadas,")),\n'
     '    ("N-30", "tabla de derivación de A6-9 alterada (§3.12)", ["G5"],\n'
     '     lambda t: sub1(t, "U {MergeBase(D.MainSha, K.AuthorityRevision)}", "U {MergeBase(D.MainSha, H.CurrentSha)}")),\n'
     '    ("N-31", "A6-5 alterado en un byte (espacio doble, mismo texto normalizado)", ["G5"],\n'
     '     lambda t: sub1(t, "R1, R2, R3, R9 y R10 están en `MATCH`", "R1, R2, R3, R9 y R10  están en `MATCH`")),\n'
     '    ("N-32", "A6-7 alterado", ["G5"], lambda t: sub1(t, "Sin esa disposición no hay medición", "Sin esa disposición no hay medición alguna")),\n'
     '    ("N-33", "A6-3, regla 6 (b), sin el apartado (iv)", ["G5"],\n'
     '     lambda t: resub1(t, r"^>    \\(iv\\) un término omitido .*\\n(?:>    (?!\\(|La prueba previa ).*\\n)*", "")),\n'
     '    ("N-34", "A6-3, regla 6 (b), de la ronda 3 reintroducida", ["G5"], lambda t: resub1s(t, r"^>    \\(b\\) `Repo` = .*?(?=^>    \\(c\\) `Verification` = )", OLD_B)),\n'
     '    ("N-35", "A6-3, regla 6 (b): un término omitido se sustituye", ["G5"],\n'
     '     lambda t: sub1(t, "no se sustituye por ningún otro valor", "se sustituye por otro valor")),\n'
     '    ("N-36", "§3.18 sin la fila R1", ["G4"], lambda t: resub1(t, r"(^### 3\\.18 (?:.*\\n)*?)\\| R1: .*\\n", r"\\1")),\n'
     '    ("N-37", "línea «Cubre» de A6-3 sin el segmento de la ronda 3", ["G4", "G5"],\n'
     '     lambda t: resub1(t, r"(; R1 y R2 de la ronda 2); [^;\\n]* de la ronda 3\\.$", r"\\1.")),\n'
     '    ("N-38", "revisión de la ronda 3 retirada de las superficies prohibidas", ["G3"],\n'
     '     lambda t: sub1(t, "`docs/automation/evidence/I-62-architect-A-6r3/` (la revisión de la ronda 3 y su kit) y ", "y ")),\n'
     '    ("N-39", "cabecera: la ronda 3 vuelve a NOT LAUNCHED", ["G1"],\n'
     '     lambda t: sub1(t, "Ronda 3:\\nCHANGES REQUIRED, ACCREDITED (R20261013T183701Z-9f80).", "Ronda 3:\\nNOT LAUNCHED.")),\n'
     '    ("N-41", "§3.17: celda «Prueba» de R1 (b) reescrita (no por añadido)", ["G4"],\n'
     '     lambda t: sub1(t, "con una ejecución con error, rechazo (con el cierre", "con una ejecución con error, aceptación (con el cierre")),\n'
     '    ("N-42", "sin la fuente de la ronda 4 docs/automation/agent-execution/README.md:219-220", ["G11"],\n'
     '     lambda t: sub1(t, "(docs/automation/agent-execution/README.md:219-220)", "(docs/automation/agent-execution/README.md)")),\n'
     '    ("N-43", "estado PROPUESTA (ronda 3)", ["G1", "G8"],\n'
     '     lambda t: sub1(t, "State:\\nPROPUESTA (ronda 4). No aplicada", "State:\\nPROPUESTA (ronda 3). No aplicada")),\n]'),
    ('    ("N-28", "commit candidato que además modifica la revisión de la ronda 2", ["G2", "G3"],\n     "docs/automation/evidence/I-62-architect-A-6r2/R20261012T195511Z-e5c3/output.json"),\n]',
     '    ("N-28", "commit candidato que además modifica la revisión de la ronda 2", ["G2", "G3"],\n     "docs/automation/evidence/I-62-architect-A-6r2/R20261012T195511Z-e5c3/output.json"),\n'
     '    ("N-44", "commit candidato que además añade un archivo a r3/ (fuera de r4/)", ["G2", "G3"], "docs/automation/evidence/I-62-A6/r3/nuevo.md"),\n'
     '    ("N-45", "commit candidato que además modifica un archivo de r3/ (evidencia de la ronda 3)", ["G2", "G3"], "docs/automation/evidence/I-62-A6/r3/preflight.md"),\n'
     '    ("N-46", "commit candidato que además modifica la revisión de la ronda 3", ["G2", "G3"],\n'
     '     "docs/automation/evidence/I-62-architect-A-6r3/R20261013T183701Z-9f80/output.json"),\n'
     '    ("N-47", "commit candidato que además modifica el kit de la revisión de la ronda 3", ["G2", "G3"],\n'
     '     "docs/automation/evidence/I-62-architect-A-6r3/R20261013T183701Z-9f80/kit/launch.py"),\n'
     '    ("N-48", "commit candidato que además modifica el aceptante fijado", ["G2", "G3"], "docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py"),\n]'),
    ('R2_PLACEHOLDERS = ["docs/automation/evidence/I-62-A6/r3/r1probes/README.md", "docs/automation/evidence/I-62-A6/r3/r2static/README.md",\n                   "docs/automation/evidence/I-62-A6/r3/launchfix/README.md"]',
     'R2_PLACEHOLDERS = ["docs/automation/evidence/I-62-A6/r4/r1probes/README.md", "docs/automation/evidence/I-62-A6/r4/r1probes/table_r4.py"]'),
    ('    """Commit candidato hipotético con A-6, las guardas, la autoprueba y un marcador por cada carpeta de evidencia r3/ que A-6 nombra\n    (r1probes/, r2static/ y launchfix/; la evidencia real la aporta el paquete r3/, no este preflight)."""',
     '    """Commit candidato hipotético con A-6, las guardas, la autoprueba y un marcador por cada evidencia r4/ que A-6 nombra\n    (r1probes/ y r1probes/table_r4.py; la evidencia real la aporta el paquete r4/, no este preflight)."""'),
    ('            fh.write("# Marcador del preflight de las guardas de la ronda 3 (no es evidencia)\\n")', '            fh.write("# Marcador del preflight de las guardas de la ronda 4 (no es evidencia)\\n")'),
    ('    rows.append({"Id": "P-1", "Case": "texto conforme en un commit candidato hipotético (A-6 modificada y r3/ añadido)", "Expected": "PASS",',
     '    rows.append({"Id": "P-1", "Case": "texto conforme en un commit candidato hipotético (A-6 modificada y r4/ añadido)", "Expected": "PASS",'),
    ('    # negativos de commit\n',
     '    # marcador pendiente en preview --final\n'
     '    path = "%s/N-40.md" % TMP\n'
     '    with open(path, "w", encoding="utf-8", newline="\\n") as fh:\n'
     '        fh.write(ins_ev(text, "Casos: @@R4.CASES@@."))\n'
     '    rc, res = guards("preview", "%s/N-40.json" % TMP, *common, "--a6", path, "--selftest", a.selftest, "--final")\n'
     '    fl = fails(res)\n'
     '    rows.append({"Id": "N-40", "Case": "marcador pendiente en preview --final", "Expected": ["G8"], "Result": res["Result"], "FailingGuards": fl,\n'
     '                 "FirstFinding": first(res, ["G8"]), "AsExpected": rc == 1 and res["Result"] == "FAIL" and "G8" in fl})\n'
     '    # negativos de commit\n'),
    ('            fh.write(cur + b"\\nlinea negativa del preflight de la ronda 3 de A-6\\n")', '            fh.write(cur + b"\\nlinea negativa del preflight de la ronda 4 de A-6\\n")'),
    ('    out = {"Tool": "neg_controls_a6r3.py",', '    out = {"Tool": "neg_controls_a6r4.py",'),
])
open(os.path.join(D, 'neg_controls_a6r4.py'), 'w', encoding='utf-8', newline='\n').write(nc)
for n in ('make_candidate_commit_a6r4.py', 'neg_controls_a6r4.py'):
    compile(open(os.path.join(D, n), encoding='utf-8').read(), n, 'exec')
print('ok')

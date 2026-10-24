"""Deriva kit/verify_clone.py de la ronda 3 (decisiones §96) del de la ronda 2: sustituye el docstring y el bloque de constantes y de comprobaciones
de la historia (de «KIT = » a «reparse, files = 0, 0»… según los marcadores) por los de la ronda 3, y ajusta la reproducción de las guardas
(r3/a7-guards.py: base y cabeza completas; autoprueba con el conjunto de --evid que existía cuando run_a7r3.py la ejecutó, incluido
preflight_text_r2.py, borrado antes de la publicación) y el resultado AllChecks. Las comprobaciones del clon, del run, de las copias fieles, del
registro de caracterización y de la sesión nueva no cambian. Uso: python -I -B patch_verify_clone_r3.py <kit> <prefijo del SHA-256 de partida>
"""
import hashlib
import os
import sys

p = os.path.join(sys.argv[1], "verify_clone.py")
t = open(p, encoding="utf-8", newline="").read()
assert hashlib.sha256(t.encode("utf-8")).hexdigest().startswith(sys.argv[2]), "verify_clone.py de partida (el de la ronda 2)"

DOC = '''"""Verificación mecánica del clon limpio y del run de la ÚNICA revisión formal de la RONDA 3 de la enmienda A-7 (decisiones §96, puntos 6 y 7)
por claude-cli, antes del lanzamiento (la identidad la verifica el invocador, porque el revisor no tiene herramientas de ejecución), con la
reproducción de las comprobaciones estáticas de la ronda 3 (guardas r3/a7-guards.py). Derivado del verify_clone.py del kit de A-7 ronda 2
(R20261019T201030Z-d1bf, a su vez del de la ronda 1 con la corrección de fidelidad de la fase 1): las mismas comprobaciones del clon, del run, del
registro de caracterización y de la sesión nueva; cambian el objeto (8bcccb2f), los commits, la premisa de la ronda 2 (a7-r2.md) y las guardas
reproducidas (las de r3/).

Comprueba: kit-params.json no provisional; HEAD = CloneCommit, separado (sin ninguna rama local); sin remotos; árbol limpio (también sin
ignorados); ningún objeto fuera de la historia de HEAD y ninguna etiqueta fuera de ella; los blobs designados (los de CanonicalInputs del cierre); la
historia del objeto (la ronda 2, 8a528133, en el padre de la publicación y en la publicación de la ronda 2, e90b444d; el blob de la ronda 3 en la
publicación, en la custodia de las guardas y en el commit del clon; la A-6 acordada en los cuatro; r3/a7-guards.py, q7-guards-selftest.json,
summary.json y preflight.md de r3/ sin cambio desde la publicación; el resultado `run` sin cambio desde su custodia; I-62-A7/r3/ ausente en el padre
de la publicación; la evidencia de la ronda 2, I-62-A7/r2/, sin cambio del padre de la publicación al commit del clon; la revisión de la ronda 2
(output.json, audit.json, README.md) igual a su custodia 3f912c84); commits presentes (los de la ronda 3, §96, la custodia de la revisión de la
ronda 2, el kit de la ronda 2 fijado en §95, §95, la ronda 2 y su registro, §94, §93, la ronda 1 y su registro, §92, §91, la preparación de la
medición, el acuerdo de A-6, la caracterización de 2.1.299, el paquete B-min y la base del mapa) y ausentes (los del Freeze y de V14, anteriores al
rebase); sin enlaces simbólicos ni uniones en el árbol de trabajo ni entradas 120000 en el índice; core.autocrlf; el run (exactamente order.txt,
prompt.md, a7-r2.md y las copias fieles de FaithfulCopies, fidelity.run_files_of; order.txt con el SHA-256 y los bytes que declara decisiones §96 en
el commit; a7-r2.md = el blob de la ronda 2 en el padre de la publicación; cada archivo idéntico a la copia del kit y a RunFileHashes del cierre;
prompt.md sin salto de línea final ni CR y en UTF-8; cada copia fiel, decisiones §94, punto 5: fidelity.verify contra los bytes exactos del original
en el commit, sin problemas, blob del original y SHA-256 de la copia = los del cierre); el registro de caracterización que fija la compuerta
(decisiones §87: copia en kit/; su SHA-256 = el fijado = el de transport-gate.json, y su blob = el del registro en el commit 1c960c0a, leído del
worktree con git de solo lectura y del clon); que no exista ningún directorio de proyecto de claude-cli para el clon o el run (patrón
D--r62-arch-a7r3*) ni una transcripción con el session id fijado; y una preflight de las guardas de la ronda 3 (evidencia del invocador: el revisor
no la ejecuta), con la salida y los temporales en <kit-review>/.tmp/ex1, que se borra. Como en los kits de las rondas 1 y 2, la preflight ejecuta el
r3/a7-guards.py del clon con --repo = el worktree de la rama, solo lectura (las guardas leen con GIT_OPTIONAL_LOCKS=0 y --no-replace-objects;
ninguna escritura en el worktree):
  * `a7-guards.py run --repo <worktree> --base 5da2007f… --head 37ac6ca4…` (SHA completos, como los registra el resultado custodiado): su salida,
    entera, igual al resultado `run` custodiado (PASS);
  * `a7-guards.py selftest --repo <worktree> --cand <A-7 del clon> --evid <copia de I-62-A7/r3/ tal como estaba cuando se ejecutó la autoprueba>
    --base <padre de la publicación>`: Result PASS, todos los mutantes muertos, e igual entera a la autoprueba custodiada (q7-guards-selftest.json).
    La copia de --evid se escribe desde el commit de publicación con los archivos que existían cuando run_a7r3.py ejecutó la autoprueba (fase q7,
    primera orden; antes de escribir q7-guards-selftest.json, q7-guards-preview.json, q7-guards.json, summary.json, preflight.md y MANIFEST.sha256)
    MÁS preflight_text_r2.py: una copia byte a byte de r2/preflight_text_r2.py (blob a773a873) que estaba en el área del preflight durante esa
    ejecución y se borró antes de regenerar MANIFEST.sha256 y publicar (orden de las 18:00:03 UTC del 2026-10-23); el conjunto se comprueba contra el
    número de archivos añadidos que registra la autoprueba custodiada (53 = 52 archivos + la enmienda) y contra el commit hipotético entero.
Las guardas escriben solo --out y su --tmp; se ejecutan con el intérprete aislado (-I -B) y el cwd en el clon, que se comprueba limpio al terminar;
el worktree, sin cambio de HEAD ni de estado. No ejecuta el constructor ni el validador del cierre de A7-1, el paquete B-min ni el kit de la medición.
Escribe clone-verification.json junto a este script. No escribe en el clon ni en el worktree.
Uso: python -B verify_clone.py
"""
'''
a = t.index('"""Verificación mecánica')
b = t.index('import glob\n')
t = DOC + t[b:]

R = [
    ('CLONE, RUN = r"D:\\r62-arch-a7r2", r"D:\\r62-arch-a7r2-run"\n', 'CLONE, RUN = r"D:\\r62-arch-a7r3", r"D:\\r62-arch-a7r3-run"\n'),
    ('OBJECT_BLOB = "8a528133c7cccd83356e31f8fd79cd2ebb0cec15"   # A-7 ronda 2\n'
     'PREV_BLOB = "6c29747d61c5f689a195095ad3712e6d9dba62aa"     # A-7 ronda 1 (a7-r1.md)\n',
     'OBJECT_BLOB = "8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff"   # A-7 ronda 3\n'
     'PREV_BLOB = "8a528133c7cccd83356e31f8fd79cd2ebb0cec15"     # A-7 ronda 2 (a7-r2.md)\n'),
    ('SESSION_ID = "3dd5ac31-ddf5-4e4a-b732-b913933a9ae3"\n'
     'R1EV = "docs/automation/evidence/I-62-A7/"\n'
     'EV = R1EV + "r2/"\n'
     'R1EV_EXCLUDE = (EV, R1EV + "r2-dispositions/")\n',
     'SESSION_ID = "a720544e-1ac0-43d9-96bd-ae39e1090dfb"\n'
     'A7EV = "docs/automation/evidence/I-62-A7/"\n'
     'EV = A7EV + "r3/"\n'
     'R2EV = A7EV + "r2/"\n'),
    ('DISPOSITIONS = R1EV + "r2-dispositions/optional-dispositions.md"\n'
     'R1_PUBLICATION, R1_GUARDS_RUN = "6fee8fd6a96ce4ae45d116e34affee4ed1b1fd40", "5e3141a2f10bbfc5e4fe1dfe5823382da841efd1"\n'
     'R1_REVIEW_CUSTODY = "e71400a2c7e842b4c8662e51675932623429bedf"\n'
     'R1_REVIEW = "docs/automation/evidence/I-62-architect-A-7/R20261019T010200Z-4a72/"\n'
     'EVID_NOT_YET_AT_SELFTEST = ["q7-guards-selftest.json", "q7-guards-preview.json", "q7-guards.json", "summary.json", "preflight.md", "MANIFEST.sha256"]\n'
     'ORDER_TEXTS = {"order.txt": ("94", "515ccfa37205aa1d9d1fccbd3eb3c2ed0c2b56b8bb67c56102d1a8b0bea3db0e", 3522)}\n'
     'RUN_FILES = ["order.txt", "prompt.md", "a7-r1.md"]\n',
     'R2_PUBLICATION, R2_GUARDS_RUN = "e90b444d5d23288e0b5a59881820377bee0d7d09", "b432c40f8ca7b1e915dff531cb9a8cad6466bb0c"\n'
     'R1_PUBLICATION, R1_GUARDS_RUN = "6fee8fd6a96ce4ae45d116e34affee4ed1b1fd40", "5e3141a2f10bbfc5e4fe1dfe5823382da841efd1"\n'
     'R2_REVIEW_CUSTODY = "3f912c84465468dda7625470c7fe8d605ae60e06"\n'
     'R2_REVIEW = "docs/automation/evidence/I-62-architect-A-7/R20261019T201030Z-d1bf/"\n'
     'EVID_NOT_YET_AT_SELFTEST = ["q7-guards-selftest.json", "q7-guards-preview.json", "q7-guards.json", "summary.json", "preflight.md", "MANIFEST.sha256"]\n'
     'EVID_REMOVED_BEFORE_PUBLICATION = {"preflight_text_r2.py": (R2EV + "preflight_text_r2.py", "a773a8733574182eafd768b0a74d70ecc41179bc")}\n'
     'ORDER_TEXTS = {"order.txt": ("96", "adb11535f24cac2874ddef13af712cc56eebece8905fe948db2f0077b3dd87b2", 3198)}\n'
     'RUN_FILES = ["order.txt", "prompt.md", "a7-r2.md"]\n'),
    ('GUARDS_RUN_BASE, GUARDS_RUN_HEAD = PUB_PARENT[:8], PUBLICATION[:8]   # abreviadas, tal como las registra el resultado `run` custodiado\n',
     'GUARDS_RUN_BASE, GUARDS_RUN_HEAD = PUB_PARENT, PUBLICATION   # completas, tal como las registra el resultado `run` custodiado de la ronda 3\n'),
    ('             (PUB_PARENT, OBJECT): PREV_BLOB, (R1_PUBLICATION, OBJECT): PREV_BLOB,\n',
     '             (PUB_PARENT, OBJECT): PREV_BLOB, (R2_PUBLICATION, OBJECT): PREV_BLOB,\n'),
    ('for _n in ("output.json", "audit.json", "README.md"):\n'
     '    AT_COMMIT[(REV, R1_REVIEW + _n)] = at(R1_REVIEW_CUSTODY, R1_REVIEW + _n)   # la revisión de la ronda 1, tal como se custodió\n'
     'PRESENT = {REV: "commit del clon (registro de la ronda 2: resultado `run` de sus guardas y disposiciones propuestas de O1-O8; decisiones §94)",\n'
     '           PUBLICATION: "publicación de la ronda 2 de A-7 (8a528133)", PUB_PARENT: "padre de la publicación (decisiones §94; base de las guardas)",\n'
     '           R1_REVIEW_CUSTODY: "custodia de la revisión de A-7 ronda 1 (R20261019T010200Z-4a72; decisiones §93)",\n',
     'for _n in ("output.json", "audit.json", "README.md"):\n'
     '    AT_COMMIT[(REV, R2_REVIEW + _n)] = at(R2_REVIEW_CUSTODY, R2_REVIEW + _n)   # la revisión de la ronda 2, tal como se custodió\n'
     'PRESENT = {REV: "commit del clon (registro de la ronda 3: resultado `run` de sus guardas; decisiones §96)",\n'
     '           PUBLICATION: "publicación de la ronda 3 de A-7 (8bcccb2f)", PUB_PARENT: "padre de la publicación (decisiones §96; base de las guardas)",\n'
     '           R2_REVIEW_CUSTODY: "custodia de la revisión de A-7 ronda 2 (R20261019T201030Z-d1bf; decisiones §95)",\n'
     '           "e42d23d12845972912c2b46703c306f8b88a24f1": "kit de la revisión de A-7 ronda 2 fijado en §95 y vuelto a congelar",\n'
     '           "6b00b2bf5759e109e3b7ff21a8327a42d0a1949d": "decisiones §95",\n'
     '           "b23d71b8fd6aef934666d29b0de48c437d20ead5": "custodia del kit de la revisión de A-7 ronda 2 con los bytes exactos",\n'
     '           R2_GUARDS_RUN: "registro de A-7 ronda 2 (resultado `run` de sus guardas; disposiciones propuestas de O1-O8)",\n'
     '           R2_PUBLICATION: "publicación de A-7 ronda 2 (8a528133)",\n'
     '           "b9334f22204c95872e7defe41bbdaaf849097585": "decisiones §94",\n'
     '           "e71400a2c7e842b4c8662e51675932623429bedf": "custodia de la revisión de A-7 ronda 1 (R20261019T010200Z-4a72; decisiones §93)",\n'),
    ('project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(projects, "D--r62-arch-a7r2*")))\n',
     'project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(projects, "D--r62-arch-a7r3*")))\n'),
    ('r1ev_parent = sorted(n for n in git("ls-tree", "-r", "--name-only", PUB_PARENT, "--", R1EV).stdout.split("\\n") if n)\n'
     'r1ev_now = sorted(n for n in git("ls-tree", "-r", "--name-only", REV, "--", R1EV).stdout.split("\\n") if n and not n.startswith(R1EV_EXCLUDE))\n'
     'r1ev_changed = sorted(p for p in r1ev_now if at(PUB_PARENT, p) != at(REV, p))\n',
     'ev_parent = set(n for n in git("ls-tree", "-r", "--name-only", PUB_PARENT, "--", EV).stdout.split("\\n") if n)\n'
     'r2ev_parent = sorted(n for n in git("ls-tree", "-r", "--name-only", PUB_PARENT, "--", R2EV).stdout.split("\\n") if n)\n'
     'r2ev_now = sorted(n for n in git("ls-tree", "-r", "--name-only", REV, "--", R2EV).stdout.split("\\n") if n)\n'
     'r2ev_changed = sorted(p for p in r2ev_now if at(PUB_PARENT, p) != at(REV, p))\n'),
    ('    "Round1AtPublicationParent": at(PUB_PARENT, OBJECT) == PREV_BLOB,\n'
     '    "A7EvidenceAddedAfterPublication": sorted(ev_now - ev_pub), "A7EvidenceRemovedAfterPublication": sorted(ev_pub - ev_now),\n'
     '    "Round1Evidence": {"FilesAtParent": len(r1ev_parent), "FilesAtClone": len(r1ev_now), "SameList": r1ev_parent == r1ev_now,\n'
     '                       "ChangedBlobs": r1ev_changed},\n'
     '    "DispositionsAtClone": {"Path": DISPOSITIONS, "Present": ok("cat-file", "-e", REV + ":" + DISPOSITIONS),\n'
     '                            "AbsentAtPublication": not ok("cat-file", "-e", PUBLICATION + ":" + DISPOSITIONS)},\n',
     '    "Round2AtPublicationParent": at(PUB_PARENT, OBJECT) == PREV_BLOB,\n'
     '    "A7R3EvidenceAtPublicationParent": sorted(ev_parent),\n'
     '    "A7EvidenceAddedAfterPublication": sorted(ev_now - ev_pub), "A7EvidenceRemovedAfterPublication": sorted(ev_pub - ev_now),\n'
     '    "Round2Evidence": {"FilesAtParent": len(r2ev_parent), "FilesAtClone": len(r2ev_now), "SameList": r2ev_parent == r2ev_now,\n'
     '                       "ChangedBlobs": r2ev_changed},\n'),
    ('    "ClaudeProjectDirsForCloneOrRun": {"Pattern": "%USERPROFILE%\\\\.claude\\\\projects\\\\D--r62-arch-a7r2*", "Found": project_dirs},\n',
     '    "ClaudeProjectDirsForCloneOrRun": {"Pattern": "%USERPROFILE%\\\\.claude\\\\projects\\\\D--r62-arch-a7r3*", "Found": project_dirs},\n'),
    ('r1b = open(os.path.join(RUN, "a7-r1.md"), "rb").read() if os.path.isfile(os.path.join(RUN, "a7-r1.md")) else b""\n'
     'run["a7-r1.md"].update({"GitBlob": git_blob(r1b), "IsRound1Blob": git_blob(r1b) == PREV_BLOB,\n'
     '                        "EqualsObjectAtPublicationParent": r1b == git("show", PUB_PARENT + ":" + OBJECT, binary=True).stdout})\n',
     'r2b = open(os.path.join(RUN, "a7-r2.md"), "rb").read() if os.path.isfile(os.path.join(RUN, "a7-r2.md")) else b""\n'
     'run["a7-r2.md"].update({"GitBlob": git_blob(r2b), "IsRound2Blob": git_blob(r2b) == PREV_BLOB,\n'
     '                        "EqualsObjectAtPublicationParent": r2b == git("show", PUB_PARENT + ":" + OBJECT, binary=True).stdout})\n'),
    ('    open(dst, "wb").write(git("show", PUBLICATION + ":" + n, binary=True).stdout)\n',
     '    open(dst, "wb").write(git("show", PUBLICATION + ":" + n, binary=True).stdout)\n'
     'removed_ok = {}\n'
     'for n, (src, blob) in EVID_REMOVED_BEFORE_PUBLICATION.items():   # en el área del preflight durante la autoprueba; borrado antes de publicar\n'
     '    rb_ = git("show", PUBLICATION + ":" + src, binary=True).stdout\n'
     '    removed_ok[n] = git_blob(rb_) == blob and not ok("cat-file", "-e", PUBLICATION + ":" + EV + n)\n'
     '    open(os.path.join(evid_dir, n), "wb").write(rb_)\n'
     'evid_count = len(evid_names) + len(EVID_REMOVED_BEFORE_PUBLICATION)\n'),
    ('pre["SelfTest"] = {"Command": "python -I -B -X utf8 %s selftest --repo <worktree> --cand <A-7 del clon> --evid <copia de I-62-A7/r2/ en %s, %d "\n'
     '                              "archivos> --base %s --out <temporal> --tmp <temporal> (script del clon; cwd = el clon; solo lectura de git)"\n'
     '                              % (GUARDS, PUBLICATION[:8], len(evid_names), PUB_PARENT[:8]),\n'
     '                   "EvidFiles": len(evid_names), "EvidExcluded": EVID_NOT_YET_AT_SELFTEST,\n',
     'pre["SelfTest"] = {"Command": "python -I -B -X utf8 %s selftest --repo <worktree> --cand <A-7 del clon> --evid <copia de I-62-A7/r3/ en %s, %d "\n'
     '                              "archivos, más %s> --base %s --out <temporal> --tmp <temporal> (script del clon; cwd = el clon; solo lectura de git)"\n'
     '                              % (GUARDS, PUBLICATION[:8], len(evid_names), ", ".join(sorted(EVID_REMOVED_BEFORE_PUBLICATION)), PUB_PARENT[:8]),\n'
     '                   "EvidFiles": evid_count, "EvidExcluded": EVID_NOT_YET_AT_SELFTEST,\n'
     '                   "EvidRemovedBeforePublication": {n: {"Source": s, "Blob": b, "Ok": removed_ok[n]}\n'
     '                                                    for n, (s, b) in EVID_REMOVED_BEFORE_PUBLICATION.items()},\n'),
    ('                    all(v["Expected"] == v["Got"] and v["Got"] for v in at_commit.values()) and doc["Round1AtPublicationParent"] and\n',
     '                    all(v["Expected"] == v["Got"] and v["Got"] for v in at_commit.values()) and doc["Round2AtPublicationParent"] and\n'
     '                    doc["A7R3EvidenceAtPublicationParent"] == [] and\n'),
    ('                    doc["Round1Evidence"]["SameList"] and not r1ev_changed and bool(r1ev_now) and\n'
     '                    doc["DispositionsAtClone"]["Present"] and doc["DispositionsAtClone"]["AbsentAtPublication"] and\n'
     '                    run["a7-r1.md"]["IsRound1Blob"] and run["a7-r1.md"]["EqualsObjectAtPublicationParent"] and\n',
     '                    doc["Round2Evidence"]["SameList"] and not r2ev_changed and bool(r2ev_now) and\n'
     '                    run["a7-r2.md"]["IsRound2Blob"] and run["a7-r2.md"]["EqualsObjectAtPublicationParent"] and\n'),
    ('                    gs.get("Mutants") == gs.get("MutantsKilled") and bool(gs.get("Mutants")) and gs.get("CandidateAdded") == len(evid_names) + 1 and\n',
     '                    gs.get("Mutants") == gs.get("MutantsKilled") and bool(gs.get("Mutants")) and gs.get("CandidateAdded") == evid_count + 1 and\n'
     '                    all(removed_ok.values()) and\n'),
]
for a_, b_ in R:
    assert t.count(a_) == 1, ("no único", t.count(a_), a_[:110])
    t = t.replace(a_, b_)
for bad in ("a7-r1", "R1EV", "r1ev", "DISPOSITIONS", "R1_REVIEW", "§94, punto 6", "3dd5ac31", 'D--r62-arch-a7r2', 'r"D:\\r62-arch-a7r2'):
    assert bad not in t, bad
open(p, "w", encoding="utf-8", newline="").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest())

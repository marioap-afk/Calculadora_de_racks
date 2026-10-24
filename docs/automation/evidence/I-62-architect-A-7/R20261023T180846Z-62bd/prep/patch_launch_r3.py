"""Deriva kit/launch.py de la ronda 3 (decisiones §96) del de la ronda 2 por sustituciones exactas: clon y run (D:\\r62-arch-a7r3*), session id del
kit de la ronda 3, premisa de la ronda anterior (a7-r2.md, blob 8a528133 de la ronda 2), patrón de proyecto y textos de §96. La lógica (G, P, R, D,
preflight 1-11, lanzamiento y registro) no cambia. Uso: python -I -B patch_launch_r3.py <kit> <prefijo del SHA-256 de partida>
"""
import hashlib
import os
import sys

p = os.path.join(sys.argv[1], "launch.py")
t = open(p, encoding="utf-8", newline="").read()
assert hashlib.sha256(t.encode("utf-8")).hexdigest().startswith(sys.argv[2]), "launch.py de partida (el de la ronda 2)"
R = [
    ('"""I-62, decisiones §94: lanzador de la ÚNICA revisión formal independiente del Architect de la RONDA 2 de la enmienda A-7 (blob 8a528133…\n'
     'fijado en kit-params.json, en el cierre y en owner_gate.py), acotada a A62-A7-R1..R3 y a sus consecuencias, por una celda claude-cli medida y\n',
     '"""I-62, decisiones §96: lanzador de la ÚNICA revisión formal independiente del Architect de la RONDA 3 de la enmienda A-7 (blob 8bcccb2f…\n'
     'fijado en kit-params.json, en el cierre y en owner_gate.py), acotada a A62-A7R2-R1 y a sus consecuencias, por una celda claude-cli medida y\n'),
    ('con una disposición del Coordinator POSTERIOR a §94 que nombre esta revisión fijada en owner_gate.py (§94, punto 6: «Sin revisión formal sin una\n'
     'disposición nueva del Coordinator»); en el kit entregado esa autorización está PENDIENTE y la compuerta 11 bloquea. Quien prepara el kit no lo lanza\n'
     '(solo sus modos --dry-run y --record-probe, que no ejecutan el binario). Derivado del launch.py del kit de A-7 ronda 1 (R20261019T010200Z-4a72, con la\n'
     'corrección de fidelidad de la fase 1): la misma lógica; cambian las constantes (clon, run, session id, patrón de proyecto, archivos del run) y\n'
     'vuelve la comprobación de la premisa de la ronda anterior (a7-r1.md con el blob 6c29747d de la ronda 1, como a6-r3.md en la ronda 4 de A-6).\n'
     'Corrección de fidelidad (decisiones §94, punto 5): la comprobación 7 toma los archivos del run del cierre (fidelity.run_files_of: order.txt,\n'
     'prompt.md, a7-r1.md y las copias fieles de los archivos que Read no entrega fielmente).\n',
     'con una disposición del Coordinator POSTERIOR a §96 que nombre esta revisión fijada en owner_gate.py (§96, punto 7: «Preparar el kit acotado, pero\n'
     'no lanzar una nueva revisión formal sin disposición posterior»); en el kit entregado esa autorización está PENDIENTE y la compuerta 11 bloquea.\n'
     'Quien prepara el kit no lo lanza (solo sus modos --dry-run y --record-probe, que no ejecutan el binario). Derivado del launch.py del kit de A-7\n'
     'ronda 2 (R20261019T201030Z-d1bf, a su vez del de la ronda 1 con la corrección de fidelidad de la fase 1): la misma lógica; cambian las constantes\n'
     '(clon, run, session id, patrón de proyecto, archivos del run) y la premisa de la ronda anterior (a7-r2.md con el blob 8a528133 de la ronda 2).\n'
     'Corrección de fidelidad (decisiones §94, punto 5; reutilizada por §96, punto 6): la comprobación 7 toma los archivos del run del cierre\n'
     '(fidelity.run_files_of: order.txt, prompt.md, a7-r2.md y las copias fieles de los archivos que Read no entrega fielmente).\n'),
    ('  7. run: exactamente order.txt, prompt.md, a7-r1.md y las copias fieles de FaithfulCopies (decisiones §94, punto 5: fidelity.run_files_of exige\n'
     '     que RunFiles del cierre termine en esas copias; sin launch/), cada uno = copia del kit = RunFileHashes del cierre = kit-manifest.json, y\n'
     '     a7-r1.md con el blob Git de la ronda 1 (6c29747d);\n'
     '  8. sesión nueva: sin transcripción con el session id fijado en ningún proyecto y sin directorio de proyecto para el clon ni para el run (sin\n'
     '     memoria previa; patrón D--r62-arch-a7r2*, que no casa con los de las revisiones anteriores, tampoco con D--r62-arch-a7 de la ronda 1);\n',
     '  7. run: exactamente order.txt, prompt.md, a7-r2.md y las copias fieles de FaithfulCopies (decisiones §94, punto 5: fidelity.run_files_of exige\n'
     '     que RunFiles del cierre termine en esas copias; sin launch/), cada uno = copia del kit = RunFileHashes del cierre = kit-manifest.json, y\n'
     '     a7-r2.md con el blob Git de la ronda 2 (8a528133);\n'
     '  8. sesión nueva: sin transcripción con el session id fijado en ningún proyecto y sin directorio de proyecto para el clon ni para el run (sin\n'
     '     memoria previa; patrón D--r62-arch-a7r3*, que no casa con los de las revisiones anteriores: D--r62-arch-a7r2* de la ronda 2 y D--r62-arch-a7\n'
     '     de la ronda 1);\n'),
    (' 11. compuerta del Owner y de la autorización (owner_gate.py; decisiones §78, §56 y §94, punto 6), BLOQUEANTE: en la HEAD del worktree de\n',
     ' 11. compuerta del Owner y de la autorización (owner_gate.py; decisiones §78, §56 y §96, punto 7), BLOQUEANTE: en la HEAD del worktree de\n'),
    ('     fijada en owner_gate.py (una disposición del Coordinator posterior a §94, con su número, su encabezado y el SHA-256 de su texto; PENDIENTE en el\n',
     '     fijada en owner_gate.py (una disposición del Coordinator posterior a §96, con su número, su encabezado y el SHA-256 de su texto; PENDIENTE en el\n'),
    ('CLONE, RUN = r"D:\\r62-arch-a7r2", r"D:\\r62-arch-a7r2-run"\n', 'CLONE, RUN = r"D:\\r62-arch-a7r3", r"D:\\r62-arch-a7r3-run"\n'),
    ('REV = PARAMS["CloneCommit"]   # commit del clon (A-7 ronda 2 publicada y registrada, el resultado `run` de sus guardas custodiado y decisiones §94)\n',
     'REV = PARAMS["CloneCommit"]   # commit del clon (A-7 ronda 3 publicada y registrada, el resultado `run` de sus guardas custodiado y decisiones §96)\n'),
    ('SESSION_ID = "3dd5ac31-ddf5-4e4a-b732-b913933a9ae3"   # uuid4 del único lanzamiento (generado una vez; prep/p2/ids.txt)\n'
     'A7R1_BLOB = "6c29747d61c5f689a195095ad3712e6d9dba62aa"   # a7-r1.md: el texto de A-7 de la ronda 1 (commit 6fee8fd6), premisa de la ronda 2\n',
     'SESSION_ID = "a720544e-1ac0-43d9-96bd-ae39e1090dfb"   # uuid4 del único lanzamiento (generado una vez; prep/ids.txt)\n'
     'A7R2_BLOB = "8a528133c7cccd83356e31f8fd79cd2ebb0cec15"   # a7-r2.md: el texto de A-7 de la ronda 2 (commit e90b444d), premisa de la ronda 3\n'),
    ('RUN_FILES = ["order.txt", "prompt.md", "a7-r1.md"]\n', 'RUN_FILES = ["order.txt", "prompt.md", "a7-r2.md"]\n'),
    ('PROJECT_DIR = os.path.join(PROJECTS, "D--r62-arch-a7r2")\n', 'PROJECT_DIR = os.path.join(PROJECTS, "D--r62-arch-a7r3")\n'),
    ('    """Comprobación 7: el run tiene exactamente order.txt, prompt.md, a7-r1.md y las copias fieles de FaithfulCopies (decisiones §94, punto 5;\n',
     '    """Comprobación 7: el run tiene exactamente order.txt, prompt.md, a7-r2.md y las copias fieles de FaithfulCopies (decisiones §94, punto 5;\n'),
    ('    r1p = os.path.join(RUN, "a7-r1.md")\n'
     '    c["7-RunFiles"]["A7R1GitBlob"] = git_blob(open(r1p, "rb").read()) if os.path.isfile(r1p) else None\n'
     '    if c["7-RunFiles"]["A7R1GitBlob"] != A7R1_BLOB:\n'
     '        c["7-RunFiles"]["Ok"] = False\n'
     '        c["7-RunFiles"]["Mismatches"].append("a7-r1.md sin el blob de la ronda 1 (6c29747d)")\n',
     '    r2p = os.path.join(RUN, "a7-r2.md")\n'
     '    c["7-RunFiles"]["A7R2GitBlob"] = git_blob(open(r2p, "rb").read()) if os.path.isfile(r2p) else None\n'
     '    if c["7-RunFiles"]["A7R2GitBlob"] != A7R2_BLOB:\n'
     '        c["7-RunFiles"]["Ok"] = False\n'
     '        c["7-RunFiles"]["Mismatches"].append("a7-r2.md sin el blob de la ronda 2 (8a528133)")\n'),
    ('    project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(PROJECTS, "D--r62-arch-a7r2*")))\n',
     '    project_dirs = sorted(os.path.basename(p) for p in glob.glob(os.path.join(PROJECTS, "D--r62-arch-a7r3*")))\n'),
    ('"%USERPROFILE%\\\\.claude\\\\projects\\\\D--r62-arch-a7r2\\\\" + SESSION_ID + ".jsonl"',
     '"%USERPROFILE%\\\\.claude\\\\projects\\\\D--r62-arch-a7r3\\\\" + SESSION_ID + ".jsonl"'),
]
for a, b in R:
    assert t.count(a) == 1, ("no único", t.count(a), a[:100])
    t = t.replace(a, b)
for bad in ("a7-r1", "A7R1", '"D--r62-arch-a7r2', "D--r62-arch-a7r2\\\\", 'r"D:\\r62-arch-a7r2', "§94, punto 6", "3dd5ac31", "6c29747d"):
    assert bad not in t, bad                                        # (D--r62-arch-a7r2* solo en la mención del patrón de la ronda 2)
open(p, "w", encoding="utf-8", newline="").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest())

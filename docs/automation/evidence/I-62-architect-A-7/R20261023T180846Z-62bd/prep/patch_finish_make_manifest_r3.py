"""Deriva kit/finish_kit.py y kit/make_manifest.py de la ronda 3 (decisiones §96) de los de la ronda 2 por sustituciones exactas: run
D:\\r62-arch-a7r3-run, premisa a7-r2.md, §96 y la revisión de la ronda 2 como ronda anterior. Las funciones step, frozen, check_pending,
probe_and_dry_run y la congelación no cambian. Uso: python -I -B patch_finish_make_manifest_r3.py <kit> <sha finish r2> <sha make_manifest r2>
"""
import hashlib
import os
import sys

kit = sys.argv[1]


def patch(name, want, R, bad):
    p = os.path.join(kit, name)
    t = open(p, encoding="utf-8", newline="").read()
    assert hashlib.sha256(t.encode("utf-8")).hexdigest().startswith(want), "%s de partida (el de la ronda 2)" % name
    for a, b in R:
        assert t.count(a) == 1, (name, "no único", t.count(a), a[:110])
        t = t.replace(a, b)
    for x in bad:
        assert x not in t, (name, x)
    open(p, "w", encoding="utf-8", newline="").write(t)
    print("ok", name, hashlib.sha256(t.encode("utf-8")).hexdigest())


if sys.argv[2] != "-":
  patch("finish_kit.py", sys.argv[2], [
    ('"""Termina el kit de la ÚNICA revisión formal de la RONDA 2 de la enmienda A-7 (decisiones §94, punto 6). Lo ejecuta quien prepara el kit, una vez,\n',
     '"""Termina el kit de la ÚNICA revisión formal de la RONDA 3 de la enmienda A-7 (decisiones §96, puntos 6 y 7). Lo ejecuta quien prepara el kit, una vez,\n'),
    ('Derivado del finish_kit.py del kit de A-7 ronda 1 (R20261019T010200Z-4a72): las mismas funciones step y frozen; cambian el clon y el run, la\n'
     'autorización (PENDIENTE: no hay paso de fijación; el paso 5 comprueba que owner_gate.py la entrega pendiente), la mutación (la batería COMPLETA de\n'
     'mutation-post-review.py, decisiones §88, punto 4, porque cambiaron reglas del auditor y la comprobación 7 del lanzador; una sola vez, al final) y la\n'
     'condición de la prueba en seco (BlockingChecks = solo la compuerta 11, con el único motivo de la autorización pendiente).\n',
     'Derivado del finish_kit.py del kit de A-7 ronda 2 (R20261019T201030Z-d1bf, a su vez del de la ronda 1): las mismas funciones step y frozen y la\n'
     'misma secuencia; cambian el clon y el run (D:\\\\r62-arch-a7r3*), la premisa (a7-r2.md) y la sección base (§96). La autorización se entrega\n'
     'PENDIENTE (el paso 5 lo comprueba), la mutación es la batería COMPLETA de mutation-post-review.py (decisiones §88, punto 4: cambiaron el auditor,\n'
     'v7.0-a7r3, y el esquema del resultado; una sola vez, al final) y la prueba en seco exige BlockingChecks = solo la compuerta 11, con el único\n'
     'motivo de la autorización pendiente.\n'),
    ('  1. prepare_clone.py <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT> → D:\\\\r62-arch-a7r2, D:\\\\r62-arch-a7r2-run\\\\{order.txt,a7-r1.md} y\n',
     '  1. prepare_clone.py <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT> → D:\\\\r62-arch-a7r3, D:\\\\r62-arch-a7r3-run\\\\{order.txt,a7-r2.md} y\n'),
    ('  3. copia byte a byte de kit/prompt.md y de kit/fiel__*.txt → D:\\\\r62-arch-a7r2-run\\\\\n',
     '  3. copia byte a byte de kit/prompt.md y de kit/fiel__*.txt → D:\\\\r62-arch-a7r3-run\\\\\n'),
    ('     disposición del Coordinator posterior a §94 con pin_authorization.py, y el kit se volverá a congelar)\n',
     '     disposición del Coordinator posterior a §96 con pin_authorization.py, y el kit se volverá a congelar)\n'),
    ('  7. verify_clone.py                                   → kit/clone-verification.json (AllChecks, con la reproducción de las guardas de la ronda 2)\n',
     '  7. verify_clone.py                                   → kit/clone-verification.json (AllChecks, con la reproducción de las guardas de la ronda 3)\n'),
    ('RUN = r"D:\\r62-arch-a7r2-run"\n', 'RUN = r"D:\\r62-arch-a7r3-run"\n'),
    ('    """Paso 5: la autorización se entrega PENDIENTE (decisiones §94, punto 6: la revisión exige una disposición del Coordinator posterior)."""\n',
     '    """Paso 5: la autorización se entrega PENDIENTE (decisiones §96, punto 7: la revisión exige una disposición del Coordinator posterior)."""\n'),
    ('"Next": "custodiar el kit; la revisión solo puede lanzarse después de una disposición del Coordinator posterior a §94 que la "',
     '"Next": "custodiar el kit; la revisión solo puede lanzarse después de una disposición del Coordinator posterior a §96 que la "'),
], ("a7r2", "a7-r1", "§94, punto 6", "posterior a §94"))

patch("make_manifest.py", sys.argv[3], [
    ('punto 5). launch.py lo exige entero (preflight 1: ningún archivo distinto y ninguno sin listar). Derivado del make_manifest.py del kit de A-7\n'
     'ronda 1 (R20261019T010200Z-4a72). Es la congelación del kit (decisiones §94, punto 6). La cabecera registra la autorización fijada o pendiente\n',
     'punto 5). launch.py lo exige entero (preflight 1: ningún archivo distinto y ninguno sin listar). Derivado del make_manifest.py del kit de A-7\n'
     'ronda 2 (R20261019T201030Z-d1bf, a su vez del de la ronda 1). Es la congelación del kit (decisiones §96, punto 7). La cabecera registra la\n'
     'autorización fijada o pendiente\n'),
    ('"""Manifiesto del kit de la ÚNICA revisión formal de la RONDA 2 de la enmienda A-7 (kit-manifest.json): bytes y SHA-256 de cada archivo de kit/\n'
     '(salvo el propio manifiesto) y de los archivos del run (order.txt, prompt.md, a7-r1.md y las copias fieles de FaithfulCopies, decisiones §94,\n',
     '"""Manifiesto del kit de la ÚNICA revisión formal de la RONDA 3 de la enmienda A-7 (kit-manifest.json): bytes y SHA-256 de cada archivo de kit/\n'
     '(salvo el propio manifiesto) y de los archivos del run (order.txt, prompt.md, a7-r2.md y las copias fieles de FaithfulCopies, decisiones §94,\n'),
    ('RUN = r"D:\\r62-arch-a7r2-run"\nRUN_FILES = ["order.txt", "prompt.md", "a7-r1.md"]\n',
     'RUN = r"D:\\r62-arch-a7r3-run"\nRUN_FILES = ["order.txt", "prompt.md", "a7-r2.md"]\n'),
    ('        "Kit": "I-62: kit de la ÚNICA revisión formal independiente del Architect de la RONDA 2 de la enmienda A-7 por claude-cli, acotada a "\n'
     '               "A62-A7-R1..R3 y a sus consecuencias (decisiones §94, punto 6; Owner CLAUDE-CLI-I62 = A; lanzamiento con la orden congelada exacta y "\n'
     '               "condicionado a la sonda de registro y a la compuerta 11: línea del Owner de §78, blobs de A-7 ronda 2 y de la A-6 acordada en la HEAD "\n'
     '               "y una disposición del Coordinator posterior a §94 que nombre esta revisión, fijada en owner_gate.py, sin ninguna sección posterior)",\n',
     '        "Kit": "I-62: kit de la ÚNICA revisión formal independiente del Architect de la RONDA 3 de la enmienda A-7 por claude-cli, acotada a "\n'
     '               "A62-A7R2-R1 y a sus consecuencias (decisiones §96, puntos 2, 3, 6 y 7; Owner CLAUDE-CLI-I62 = A; lanzamiento con la orden congelada "\n'
     '               "exacta y condicionado a la sonda de registro y a la compuerta 11: línea del Owner de §78, blobs de A-7 ronda 3 y de la A-6 acordada en "\n'
     '               "la HEAD y una disposición del Coordinator posterior a §96 que nombre esta revisión, fijada en owner_gate.py, sin ninguna sección "\n'
     '               "posterior)",\n'),
    ('        "PreviousRound": ("A-7 ronda 1: R20261019T010200Z-4a72 (blob 6c29747d; auditor NOT_ACCREDITED por una lectura DEGRADED de un TRX con BOM; "\n'
     '                          "Architect CHANGES REQUIRED con A62-A7-R1..R3; custodia e71400a2; decisiones §93 y §94)"),\n',
     '        "PreviousRound": ("A-7 ronda 2: R20261019T201030Z-d1bf (blob 8a528133; auditor NOT_ACCREDITED por una referencia no canónica del revisor; "\n'
     '                          "Architect CHANGES REQUIRED con A62-A7R2-R1; custodia 3f912c84; decisiones §95 y §96)"),\n'),
    ('        "Clone": "D:/r62-arch-a7r2 (git clone --no-local -c core.autocrlf=false --single-branch desde el directorio común de git; HEAD separado en "\n'
     '                 "el commit, sin ramas locales; sin remoto; clone-verification.json AllChecks, con la reproducción de las guardas de la ronda 2)",\n'
     '        "Run": "D:/r62-arch-a7r2-run (order.txt, prompt.md, a7-r1.md y %d copias fieles de los archivos que Read no entrega fielmente, decisiones "\n',
     '        "Clone": "D:/r62-arch-a7r3 (git clone --no-local -c core.autocrlf=false --single-branch desde el directorio común de git; HEAD separado en "\n'
     '                 "el commit, sin ramas locales; sin remoto; clone-verification.json AllChecks, con la reproducción de las guardas de la ronda 3)",\n'
     '        "Run": "D:/r62-arch-a7r3-run (order.txt, prompt.md, a7-r2.md y %d copias fieles de los archivos que Read no entrega fielmente, decisiones "\n'),
    ('        "Auditor": "v6.0-a7r2 (declarado y autoprobado antes del lanzamiento: selftest-result.json AllPass %s, %d/%d; selftest-fidelity-result.json "\n',
     '        "Auditor": "v7.0-a7r3 (declarado y autoprobado antes del lanzamiento: selftest-result.json AllPass %s, %d/%d; selftest-fidelity-result.json "\n'),
    ('                                     "posterior a §94 fijada en owner_gate.py (número, encabezado y SHA-256 de su texto, con el blob completo) sin "\n',
     '                                     "posterior a §96 fijada en owner_gate.py (número, encabezado y SHA-256 de su texto, con el blob completo) sin "\n'),
    ('                               "fijar una disposición del Coordinator posterior a §94 que nombre esta revisión (pin_authorization.py) y volver a "\n',
     '                               "fijar una disposición del Coordinator posterior a §96 que nombre esta revisión (pin_authorization.py) y volver a "\n'),
], ("a7r2", "a7-r1", "§94, punto 6", "posterior a §94"))

"""Deriva kit/make_manifest.py del make_manifest.py del kit acreditado de A-6 ronda 4 (SHA-256 f160f911…) por sustituciones exactas (cada una, una
sola coincidencia): run de A-7 (dos archivos), textos de la cabecera para A-7, el auditor v5.5-a7 y la mutación dirigida (mutation-targeted-result.json)
en lugar de la batería completa. La lógica del manifiesto (archivos de kit/, run, --gate) no cambia.
Uso: python -I -B adapt_make_manifest.py <kit dir>
"""
import hashlib
import sys

BASE_SHA = "f160f9119ab57f6ca0fca672945b30a7245622ab92fcb14005a6237da323ddd8"
p = sys.argv[1] + "/make_manifest.py"
b = open(p, "rb").read()
assert hashlib.sha256(b).hexdigest() == BASE_SHA, "make_manifest.py no es la copia del kit acreditado de A-6 ronda 4"
s = b.decode("utf-8")
SUBS = [
    ('''"""Manifiesto del kit de la ÚNICA revisión formal de la ronda 4 de A-6 (kit-manifest.json): bytes y SHA-256 de cada archivo de kit/ (salvo el propio
manifiesto) y de los tres archivos del run. launch.py lo exige entero (preflight 1: ningún archivo distinto y ninguno sin listar). Derivado del
make_manifest.py del kit acreditado de la ronda 3. Es la congelación del kit (decisiones §85, punto 6). Después de fijar la autorización en
owner_gate.py (pin_authorization.py), se vuelve a ejecutar sin argumentos: la cabecera registra la autorización fijada (o pendiente) y si
owner_gate.py cambió desde la autoprueba y la mutación (solo esas tres constantes deben cambiar).''',
     '''"""Manifiesto del kit de la ÚNICA revisión formal de la enmienda A-7 (kit-manifest.json): bytes y SHA-256 de cada archivo de kit/ (salvo el propio
manifiesto) y de los dos archivos del run. launch.py lo exige entero (preflight 1: ningún archivo distinto y ninguno sin listar). Derivado del
make_manifest.py del kit acreditado de A-6 ronda 4. Es la congelación del kit (decisiones §93, punto 6). La cabecera registra la autorización fijada
(§93) o pendiente y si owner_gate.py cambió desde la autoprueba y la mutación dirigida (mutation-targeted-result.json).'''),
    ('RUN = r"D:\\r62-arch-a6r4-run"\nRUN_FILES = ["order.txt", "prompt.md", "a6-r3.md"]', 'RUN = r"D:\\r62-arch-a7-run"\nRUN_FILES = ["order.txt", "prompt.md"]'),
    ('''    mp = os.path.join(KIT, "mutation-result.json")
    if os.path.isfile(mp):
        mu = json.load(open(mp, encoding="utf-8"))
        mutation = "mutation-result.json AllKilled %s, %d/%d" % (mu["AllKilled"], mu["Totals"]["Killed"], mu["Totals"]["Mutants"])
        mutated_og = (mu.get("Modules") or {}).get("owner_gate.py", {}).get("Sha256")
    else:
        mutation = "sin prueba de mutación completa en este kit (README, desviaciones)"
        mutated_og = None''',
     '''    mp = os.path.join(KIT, "mutation-targeted-result.json")
    if os.path.isfile(mp):
        mu = json.load(open(mp, encoding="utf-8"))
        mutation = ("mutation-targeted-result.json (mutación DIRIGIDA a los sitios cambiados) AllKilled %s, %d/%d; los sitios no cambiados reutilizan "
                    "mutation-result.json 424c983e del kit de A-6 ronda 4 (191/191)" % (mu["AllKilled"], mu["Totals"]["Killed"], mu["Totals"]["Mutants"]))
        mutated_og = (mu.get("Modules") or {}).get("owner_gate.py", {}).get("Sha256")
    else:
        mutation = "sin prueba de mutación en este kit (README, desviaciones)"
        mutated_og = None'''),
    ('''        "Kit": "I-62: kit de la ÚNICA revisión formal independiente del Architect de la ronda 4 de la enmienda A-6 por claude-cli, acotada a "
               "A62-A6R3-R1 y a sus consecuencias directas (decisiones §85, punto 6; Owner CLAUDE-CLI-I62 = A; lanzamiento con la orden congelada "
               "exacta y condicionado a la sonda de registro y a la compuerta 11: línea del Owner de §78, blob de la ronda 4 en la HEAD y una "
               "disposición del Coordinator posterior a §85 fijada en owner_gate.py, sin ninguna sección posterior)",
        "Ids": closure["Ids"], "PreviousAttempts": [],
        "PreviousRound": "R20261013T183701Z-9f80 (ronda 3: CHANGES REQUIRED; auditor ACCREDITED)",''',
     '''        "Kit": "I-62: kit de la ÚNICA revisión formal independiente del Architect de la enmienda A-7 por claude-cli, acotada a A7-1 y a sus "
               "consecuencias sobre el cierre de VERIFY de FX-02/F6 y la medición de A6-7 (decisiones §93; Owner CLAUDE-CLI-I62 = A; lanzamiento con "
               "la orden congelada exacta y condicionado a la sonda de registro y a la compuerta 11: línea del Owner de §78, blobs de A-7 y de la "
               "A-6 acordada en la HEAD y la disposición del Coordinator §93, posterior a §92, fijada en owner_gate.py, sin ninguna sección posterior)",
        "Ids": closure["Ids"], "PreviousAttempts": [],
        "PreviousRound": "ninguna: A-7 ronda 1 (kit derivado del de A-6 ronda 4, R20261014T180556Z-5e02, ACCREDITED)",'''),
    ('''        "Auditor": "v5.4-a6r4 (declarado y autoprobado antes del lanzamiento: selftest-result.json AllPass %s, %d/%d; %s)"''',
     '''        "Auditor": "v5.5-a7 (declarado y autoprobado antes del lanzamiento: selftest-result.json AllPass %s, %d/%d; %s)"'''),
    ('''        "Clone": "D:/r62-arch-a6r4 (git clone --no-local -c core.autocrlf=false --single-branch desde el directorio común de git; HEAD separado en "
                 "el commit, sin ramas locales; sin remoto; clone-verification.json AllChecks, con la reproducción de las guardas de la ronda 4)",
        "Run": "D:/r62-arch-a6r4-run (order.txt, prompt.md y a6-r3.md, idénticos byte a byte a las copias del kit)",''',
     '''        "Clone": "D:/r62-arch-a7 (git clone --no-local -c core.autocrlf=false --single-branch desde el directorio común de git; HEAD separado en "
                 "el commit, sin ramas locales; sin remoto; clone-verification.json AllChecks, con la reproducción de las guardas de A-7)",
        "Run": "D:/r62-arch-a7-run (order.txt y prompt.md, idénticos byte a byte a las copias del kit)",'''),
    ('''        "OwnerAndAuthorizationGate": "%s (owner_gate.py): launch.py solo lanza con la línea del Owner A6-ADR0046-9-FX02 (SHA-256 %s…) después de §78, "
                                     "CLAUDE-CLI-I62 = A vigente, A-6 en la HEAD con el blob %s y la disposición del Coordinator posterior a §85 "
                                     "fijada en owner_gate.py (número, encabezado y SHA-256 de su texto, con el blob completo) sin ninguna sección "
                                     "posterior; se evalúa en el momento del lanzamiento"''',
     '''        "OwnerAndAuthorizationGate": "%s (owner_gate.py): launch.py solo lanza con la línea del Owner A6-ADR0046-9-FX02 (SHA-256 %s…) después de §78, "
                                     "CLAUDE-CLI-I62 = A vigente, A-7 en la HEAD con el blob %s, la A-6 acordada y la disposición del Coordinator §93 "
                                     "(posterior a §92) fijada en owner_gate.py (número, encabezado y SHA-256 de su texto, con el blob completo) sin "
                                     "ninguna sección posterior; se evalúa en el momento del lanzamiento"'''),
    ('''        "AuthorizationState": ("PENDIENTE: owner_gate.py entrega AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA en None; la compuerta 11 bloquea hasta "
                               "fijar una disposición del Coordinator posterior a §85 (pin_authorization.py) y volver a congelar el kit"''',
     '''        "AuthorizationState": ("PENDIENTE: owner_gate.py entrega AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA en None; la compuerta 11 bloquea hasta "
                               "fijar una disposición del Coordinator posterior a §92 (pin_authorization.py) y volver a congelar el kit"'''),
]
for old, new in SUBS:
    assert s.count(old) == 1, ("sustitución", old[:80], s.count(old))
    s = s.replace(old, new)
open(p, "w", encoding="utf-8", newline="\n").write(s)
print("ok")

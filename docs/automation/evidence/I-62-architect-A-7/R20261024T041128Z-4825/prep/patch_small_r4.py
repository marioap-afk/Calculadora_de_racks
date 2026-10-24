"""Paso B (componentes pequeños) del kit de la RONDA 4 de A-7: ediciones semánticas exactas sobre la salida del paso A (shifted/) y escritura en
kit/. Cada texto antiguo debe aparecer exactamente las veces declaradas. owner_gate.py no necesita ediciones (el paso A basta).
Uso: python -I -B patch_small_r4.py
"""
import hashlib, os, shutil

PREP = os.path.dirname(os.path.abspath(__file__))
SH = os.path.join(PREP, 'shifted')
KIT = os.path.join(os.path.dirname(PREP), 'kit')
P = {}


def add(f, old, new, count=1):
    P.setdefault(f, []).append((old, new, count))


# ---- launch.py: las menciones de A-6 que el desplazamiento cambió; la cita de §98, punto 7 partida en dos líneas
add('launch.py', '''(§98, punto 7: «Preparar el kit acotado, pero
no lanzar una nueva revisión formal sin disposición posterior»)''', '''(§98, punto 7: «Preparar el kit acotado, pero
no ejecutar revisión formal sin nueva disposición»)''')
add('launch.py', '     y del kit; la de la ronda 4, §84, punto 3) funciona:', '     y del kit; la de la ronda 3, §84, punto 3) funciona:')
add('launch.py', 'OutcomeCategories (los de la ronda 4, §84, punto 4):', 'OutcomeCategories (los de la ronda 3, §84, punto 4):')
add('launch.py', 'Correcciones heredadas de la ronda 4 (tras la ronda 3, RunId R20261012T195511Z-e5c3:',
    'Correcciones heredadas de la ronda 3 (tras la ronda 2, RunId R20261012T195511Z-e5c3:')
add('launch.py', 'byte a byte las del lanzador de A-6 ronda 4 (y de la ronda 4).', 'byte a byte las del lanzador de A-6 ronda 4 (y de las rondas 2 y 3 de A-7).')
# ---- mutation-post-review.py
add('mutation-post-review.py', '(unchanged from A-6 round 4 and round 4: the D-01 gate', '(unchanged from A-6 round 4 and round 3: the D-01 gate')
# ---- make_manifest.py
add('make_manifest.py', '''        "PreviousRound": ("A-7 ronda 3: R20261023T180846Z-62bd (blob 8bcccb2f; auditor NOT_ACCREDITED por una referencia no canónica del revisor; "
                          "Architect CHANGES REQUIRED con A62-A7R3-R1; custodia 3f912c84; decisiones §97 y §98)"),''',
    '''        "PreviousRound": ("A-7 ronda 3: R20261023T180846Z-62bd (blob 8bcccb2f; auditor ACCREDITED; Architect CHANGES REQUIRED con A62-A7R3-R1 y "
                          "A62-A7R3-O1..O3; custodia 068d3368; decisiones §97 y §98)"),''')
add('make_manifest.py', '(decisiones §98, puntos 2, 3, 6 y 7;', '(decisiones §98, puntos 2, 6 y 7;')
# ---- verify_clone.py
add('verify_clone.py', 'reproducción de las comprobaciones estáticas de la ronda 4 (guardas r3/a7-guards.py).',
    'reproducción de las comprobaciones estáticas de la ronda 4 (guardas r4/a7-guards.py).')
add('verify_clone.py', 'reproducidas (las de r3/).', 'reproducidas (las de r4/).')
add('verify_clone.py', 'publicación, en la custodia de las guardas y en el commit del clon; la A-6 acordada en los cuatro; r3/a7-guards.py, q7-guards-selftest.json,\nsummary.json y preflight.md de r3/ sin cambio',
    'publicación, en la custodia de las guardas y en el commit del clon; la A-6 acordada en los cuatro; r4/a7-guards.py, q7-guards-selftest.json,\nsummary.json y preflight.md de r4/ sin cambio')
add('verify_clone.py', '(output.json, audit.json, README.md) igual a su custodia 3f912c84)', '(output.json, audit.json, README.md) igual a su custodia 068d3368)')
add('verify_clone.py', '''commits presentes (los de la ronda 4, §98, la custodia de la revisión de la
ronda 3, el kit de la ronda 3 fijado en §97, §97, la ronda 3 y su registro, §94, §93, la ronda 1 y su registro,''',
    '''commits presentes (los de la ronda 4, §98, la custodia de la revisión de la
ronda 3, el kit de la ronda 3 fijado en §97 y su custodia, §97, la ronda 3 y su registro, §96, la custodia de la revisión de la ronda 2 y su kit, §95, la
ronda 2 y su registro, §94, §93, la ronda 1 y su registro,''')
add('verify_clone.py', 'r3/a7-guards.py del clon con --repo = el worktree de la rama', 'r4/a7-guards.py del clon con --repo = el worktree de la rama')
add('verify_clone.py', '''    primera orden; antes de escribir q7-guards-selftest.json, q7-guards-preview.json, q7-guards.json, summary.json, preflight.md y MANIFEST.sha256)
    MÁS preflight_text_r2.py: una copia byte a byte de r2/preflight_text_r2.py (blob a773a873) que estaba en el área del preflight durante esa
    ejecución y se borró antes de regenerar MANIFEST.sha256 y publicar (orden de las 18:00:03 UTC del 2026-10-23); el conjunto se comprueba contra el
    número de archivos añadidos que registra la autoprueba custodiada (53 = 52 archivos + la enmienda) y contra el commit hipotético entero.''',
    '''    primera orden; antes de escribir q7-guards-selftest.json, q7-guards-preview.json, q7-guards.json, summary.json, modalities.md, modalities.json,
    preflight.md y MANIFEST.sha256); en la ronda 4 ningún archivo se borró del área del preflight antes de publicar. El conjunto se comprueba contra
    el número de archivos añadidos que registra la autoprueba custodiada y contra el commit hipotético entero.''')
add('verify_clone.py', 'R2_PUBLICATION, R2_GUARDS_RUN = "37ac6ca4a6f0a04268ac357764bb74f843eb3a08", "b432c40f8ca7b1e915dff531cb9a8cad6466bb0c"',
    'R2_PUBLICATION, R2_GUARDS_RUN = "37ac6ca4a6f0a04268ac357764bb74f843eb3a08", "b9bfae50a82ba59c834658e54d7699662ee298bb"   # la ronda 3')
add('verify_clone.py', 'R2_REVIEW_CUSTODY = "3f912c84465468dda7625470c7fe8d605ae60e06"', 'R2_REVIEW_CUSTODY = "068d3368a8034277404bdb0aac28891907b2d156"   # la revisión de la ronda 3')
add('verify_clone.py', '''EVID_NOT_YET_AT_SELFTEST = ["q7-guards-selftest.json", "q7-guards-preview.json", "q7-guards.json", "summary.json", "preflight.md", "MANIFEST.sha256"]
EVID_REMOVED_BEFORE_PUBLICATION = {"preflight_text_r2.py": (R2EV + "preflight_text_r2.py", "a773a8733574182eafd768b0a74d70ecc41179bc")}''',
    '''EVID_NOT_YET_AT_SELFTEST = ["q7-guards-selftest.json", "q7-guards-preview.json", "q7-guards.json", "summary.json", "modalities.md", "modalities.json",
                            "preflight.md", "MANIFEST.sha256"]
EVID_REMOVED_BEFORE_PUBLICATION = {}   # ronda 4: nada se borró del área del preflight antes de publicar''')
add('verify_clone.py', '''           "e42d23d12845972912c2b46703c306f8b88a24f1": "kit de la revisión de A-7 ronda 3 fijado en §97 y vuelto a congelar",
           "6b00b2bf5759e109e3b7ff21a8327a42d0a1949d": "decisiones §97",
           "b23d71b8fd6aef934666d29b0de48c437d20ead5": "custodia del kit de la revisión de A-7 ronda 3 con los bytes exactos",
           R2_GUARDS_RUN: "registro de A-7 ronda 3 (resultado `run` de sus guardas; disposiciones propuestas de O1-O8)",
           R2_PUBLICATION: "publicación de A-7 ronda 3 (8bcccb2f)",''',
    '''           "850dd7dc2352715a6f7ea2893114d16aa0eeb739": "kit de la revisión de A-7 ronda 3 fijado en §97 y vuelto a congelar",
           "92aa4d0ce27d37696ff016100d5c29de28bbb6f1": "decisiones §97",
           "000fd6eb745f3db5f68b27e73563fb529d6ca3e2": "custodia del kit de la revisión de A-7 ronda 3 (pendiente)",
           R2_GUARDS_RUN: "registro de A-7 ronda 3 (resultado `run` de sus guardas)",
           R2_PUBLICATION: "publicación de A-7 ronda 3 (8bcccb2f)",
           "5da2007f45160ced37b958a3a772c46ac64874ee": "decisiones §96",
           "3f912c84465468dda7625470c7fe8d605ae60e06": "custodia de la revisión de A-7 ronda 2 (R20261019T201030Z-d1bf; decisiones §95)",
           "e42d23d12845972912c2b46703c306f8b88a24f1": "kit de la revisión de A-7 ronda 2 fijado en §95 y vuelto a congelar",
           "6b00b2bf5759e109e3b7ff21a8327a42d0a1949d": "decisiones §95",
           "b23d71b8fd6aef934666d29b0de48c437d20ead5": "custodia del kit de la revisión de A-7 ronda 2 con los bytes exactos",
           "b432c40f8ca7b1e915dff531cb9a8cad6466bb0c": "registro de A-7 ronda 2 (resultado `run` de sus guardas; disposiciones propuestas de O1-O8)",
           "e90b444d5d23288e0b5a59881820377bee0d7d09": "publicación de A-7 ronda 2 (8a528133)",''')

# ---- make_manifest.py y finish_kit.py: la mutación de la ronda 4 es la DIRIGIDA de targeted-mutation.py (decisiones §98, punto 6)
add('make_manifest.py', '(pendiente en el kit entregado) y si owner_gate.py cambió desde la autoprueba y la batería completa de mutación (mutation-result.json).',
    '(pendiente en el kit entregado) y si owner_gate.py cambió desde la autoprueba y la mutación (en la ronda 4, la DIRIGIDA de\n'
    'targeted-mutation-result.json, decisiones §98, punto 6; la batería completa, mutation-result.json, si se ejecutó con --full-mutation).')
add('make_manifest.py', '''    else:
        mutation = "batería completa de mutación PENDIENTE (se ejecuta una sola vez, al final; el manifiesto se regenera después)"''',
    '''    elif os.path.isfile(os.path.join(KIT, "targeted-mutation-result.json")):
        tm = json.load(open(os.path.join(KIT, "targeted-mutation-result.json"), encoding="utf-8"))
        mutated = {n: r.get("Sha256") for n, r in (tm.get("Modules") or {}).items()}
        stale = sorted(n for n, s in mutated.items() if not os.path.isfile(os.path.join(KIT, n)) or rec(os.path.join(KIT, n))["Sha256"] != s)
        o = tm["OwnerGateTargeted"]
        mutation = ("mutación DIRIGIDA (decisiones §98, punto 6: componentes congelados idénticos y validación incremental) "
                    "targeted-mutation-result.json AllPass %s: reglas de post-review.py, transport_gate.py, owner_gate.py y fidelity.py con los "
                    "mismos sitios que en la ronda 3 %s (su batería completa, 209/209, sigue siendo la evidencia aplicable) y owner_gate.py %d/%d "
                    "sobre los casos de la compuerta 11%s" % (tm["AllPass"], tm["RuleIdentityOk"], o["Killed"], o["Sites"],
                                                              "; MÓDULOS CAMBIADOS DESDE LA MUTACIÓN: %s" % stale if stale else ""))
    else:
        mutation = ("mutación PENDIENTE (la dirigida de targeted-mutation.py, decisiones §98, punto 6; se ejecuta una sola vez, al final; el "
                    "manifiesto se regenera después)")''')
add('finish_kit.py', '''PENDIENTE (el paso 5 lo comprueba), la mutación es la batería COMPLETA de mutation-post-review.py (decisiones §88, punto 4: cambiaron el auditor,
v7.0-a7r4, y el esquema del resultado; una sola vez, al final) y la prueba en seco exige BlockingChecks = solo la compuerta 11, con el único
motivo de la autorización pendiente.''', '''PENDIENTE (el paso 5 lo comprueba), la mutación es la DIRIGIDA de targeted-mutation.py (decisiones §98, punto 6: reutilizar componentes
congelados idénticos y sus evidencias; validación incremental: las reglas del auditor v7.0-a7r4, de la compuerta D-01 y de la fidelidad tienen los
mismos sitios que en la ronda 3, cuya batería completa sigue siendo la evidencia aplicable, y owner_gate.py se muta sobre los casos de la compuerta
11; una sola vez, al final; con --full-mutation, la batería completa de mutation-post-review.py) y la prueba en seco exige BlockingChecks = solo la
compuerta 11, con el único motivo de la autorización pendiente.''')
add('finish_kit.py', '(congelación previa a la batería)', '(congelación previa a la mutación)')
add('finish_kit.py', '→ ../launch-dry-run-before-full-mutation.json: código 2 siempre en seco; se exige',
    '→ ../launch-dry-run-before-mutation.json: código 2 siempre en seco; se exige')
add('finish_kit.py', ''' 12. mutation-post-review.py kit/mutation-result.json  → AllKilled (batería COMPLETA de los cuatro módulos; con --no-mutation se omite y el
                                                         manifiesto lo declara), y después 9-11 otra vez (manifiesto final, ../record-probe.json y
                                                         ../launch-dry-run.json con la misma condición)''',
    ''' 12. targeted-mutation.py kit/targeted-mutation-result.json → AllPass (mutación DIRIGIDA, decisiones §98, punto 6; con --full-mutation,
                                                         mutation-post-review.py kit/mutation-result.json, la batería COMPLETA de los cuatro módulos;
                                                         con --no-mutation se omite y el manifiesto lo declara), y después 9-11 otra vez (manifiesto
                                                         final, ../record-probe.json y ../launch-dry-run.json con la misma condición)''')
add('finish_kit.py', 'Uso: python -B finish_kit.py <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT> [--clone-ready] [--no-mutation]',
    'Uso: python -B finish_kit.py <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT> [--clone-ready] [--no-mutation | --full-mutation]')
add('finish_kit.py', '''    probe_and_dry_run("launch-dry-run-before-full-mutation.json")
    if "--no-mutation" not in argv:
        step("12 mutation (batería COMPLETA)", ["mutation-post-review.py", os.path.join(KIT, "mutation-result.json")])''',
    '''    probe_and_dry_run("launch-dry-run-before-mutation.json")
    if "--no-mutation" not in argv:
        if "--full-mutation" in argv:
            step("12 mutation (batería COMPLETA)", ["mutation-post-review.py", os.path.join(KIT, "mutation-result.json")])
        else:
            step("12 mutation (DIRIGIDA, decisiones §98, punto 6)", ["targeted-mutation.py", os.path.join(KIT, "targeted-mutation-result.json")])''')

os.makedirs(KIT, exist_ok=True)
for f in ('owner_gate.py', 'launch.py', 'prepare_clone.py', 'finish_kit.py', 'make_manifest.py', 'mutation-post-review.py', 'verify_clone.py'):
    t = open(os.path.join(SH, f), encoding='utf-8', newline='').read()
    for i, (old, new, count) in enumerate(P.get(f, [])):
        n = t.count(old)
        if n != count:
            raise SystemExit('%s, edición %d: %d apariciones (se esperaban %d): %r' % (f, i, n, count, old[:90]))
        t = t.replace(old, new)
    b = t.encode('utf-8')
    assert b'\r' not in b
    dst = os.path.join(KIT, f)
    same = os.path.isfile(dst) and open(dst, 'rb').read() == b
    if not same:   # sin reescribir los idénticos: la autoprueba puede estar leyéndolos
        open(dst, 'wb').write(b)
    print('%-26s %s (%d ediciones)%s' % (f, hashlib.sha256(b).hexdigest()[:16], len(P.get(f, [])), '' if same else ' ESCRITO'))

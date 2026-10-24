"""Deriva kit/owner_gate.py de la ronda 3 (decisiones §96) del de la ronda 2, por sustituciones exactas: objeto A-7 ronda 3 (blob 8bcccb2f…),
sección base §96, RunId del kit de la ronda 3 y autorización PENDIENTE (AUTH_* en None; la copia de partida era la fijada en §95). Las funciones de
sección, token, autorización y secciones posteriores no cambian. Uso: python -I -B patch_owner_gate_r3.py <kit>
"""
import hashlib
import os
import sys

p = os.path.join(sys.argv[1], "owner_gate.py")
t = open(p, encoding="utf-8", newline="").read()
assert hashlib.sha256(t.encode("utf-8")).hexdigest().startswith("0064b35e"), "owner_gate.py de partida (fijado en §95)"
R = [
    ('"""I-62, decisiones §94: compuerta del Owner y de la autorización de la ÚNICA revisión formal de la RONDA 2 de la enmienda A-7 (preflight 11,',
     '"""I-62, decisiones §96: compuerta del Owner y de la autorización de la ÚNICA revisión formal de la RONDA 3 de la enmienda A-7 (preflight 11,'),
    ('«11-OwnerAndAuthorizationGate», de launch.py). BLOQUEANTE. Adaptada de owner_gate.py del kit de A-7 ronda 1 (R20261019T010200Z-4a72, a su vez\n'
     'del de A-6 ronda 4): las funciones de sección, token, autorización y secciones posteriores, byte a byte; cambian el objeto (A-7 ronda 2, blob\n'
     '8a528133…), la sección base (§94) y la autorización, que queda PENDIENTE en el kit entregado (AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA en None).',
     '«11-OwnerAndAuthorizationGate», de launch.py). BLOQUEANTE. Adaptada de owner_gate.py del kit de A-7 ronda 2 (R20261019T201030Z-d1bf, a su vez\n'
     'del de la ronda 1 y del de A-6 ronda 4): las funciones de sección, token, autorización y secciones posteriores, byte a byte; cambian el objeto\n'
     '(A-7 ronda 3, blob 8bcccb2f…), la sección base (§96) y la autorización, que queda PENDIENTE en el kit entregado (AUTH_SECTION, AUTH_HEADING y\n'
     'AUTH_TEXT_SHA en None).'),
    ('§94, punto 6: «Preparar y custodiar un kit de revisión limitado a R1-R3 y sus consecuencias. Sin revisión formal sin una disposición nueva del\n'
     'Coordinator». La compuerta bloquea, con el único motivo PENDING_REASON, hasta que una disposición del Coordinator POSTERIOR a §94 que nombre esta',
     '§96, punto 7: «Preparar el kit acotado, pero no lanzar una nueva revisión formal sin disposición posterior». La compuerta bloquea, con el único\n'
     'motivo PENDING_REASON, hasta que una disposición del Coordinator POSTERIOR a §96 que nombre esta'),
    ('(= closure.ObjectBlob; la ronda 2 de §94) y\n      docs/initiatives/I-62-A-6.md tiene el blob acordado A6_AGREED_BLOB (decisiones §90; §94, punto 2: «Sin modificar A-6»);',
     '(= closure.ObjectBlob; la ronda 3 de §96) y\n      docs/initiatives/I-62-A-6.md tiene el blob acordado A6_AGREED_BLOB (decisiones §90; §96, punto 3: «No modificar A-6»);'),
    ('una disposición del Coordinator POSTERIOR a §94 (BASE_SECTION: la disposición que pidió la segunda candidata y prohibió la',
     'una disposición del Coordinator POSTERIOR a §96 (BASE_SECTION: la disposición que pidió la tercera candidata y prohibió la'),
    ('Con las tres fijadas: AUTH_SECTION posterior a §94;', 'Con las tres fijadas: AUTH_SECTION posterior a §96;'),
    ('En este kit, §94 es la última sección del commit del clon', 'En este kit, §96 es la última sección del commit del clon'),
    ('las secciones posteriores a §94;', 'las secciones posteriores a §96;'),
    ('(solo añadido: §78, §92, §93, §94 y lo anterior, intactos).', '(solo añadido: §78, §92-§96 y lo anterior, intactos).'),
    ('OBJECT_BLOB = "8a528133c7cccd83356e31f8fd79cd2ebb0cec15"         # A-7, ronda 2 (publicada en e90b444d; registrada en b432c40f; decisiones §94)',
     'OBJECT_BLOB = "8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff"         # A-7, ronda 3 (publicada en 37ac6ca4; registrada en b9bfae50; decisiones §96)'),
    ('BASE_SECTION = 94                                                # decisiones §94: segunda candidata; «Sin revisión formal sin una disposición nueva del Coordinator»\n'
     'AUTH_SECTION = 95   # decisiones §95: disposición del Coordinator que autoriza el lanzamiento (pin_authorization.py)\n'
     'AUTH_HEADING = "## 95. Disposición del Coordinator: revisión formal acotada de A-7 ronda 2 (2026-10-22 UTC)"\n'
     'AUTH_TEXT_SHA = "9ba8fd5b647fa08ca769c9472889c72f33a26a79360313d481ab2423810dda88"   # SHA-256 del texto de §95 (pin_authorization.py)\n'
     'PENDING_REASON = ("authorization pending: a Coordinator disposition after §94 must be pinned (section number, heading, text SHA-256) and the kit "',
     'BASE_SECTION = 96                                                # decisiones §96: tercera candidata; «no lanzar una nueva revisión formal sin disposición posterior»\n'
     'AUTH_SECTION = None   # PENDIENTE: la disposición del Coordinator posterior a §96 que autorice esta revisión (pin_authorization.py)\n'
     'AUTH_HEADING = None\n'
     'AUTH_TEXT_SHA = None\n'
     'PENDING_REASON = ("authorization pending: a Coordinator disposition after §96 must be pinned (section number, heading, text SHA-256) and the kit "'),
    ('RUN_ID = "R20261019T201030Z-d1bf"                                # RunId del kit (generado una vez; prep/p2/ids.txt)',
     'RUN_ID = "R20261023T180846Z-62bd"                                # RunId del kit (generado una vez; prep/ids.txt)'),
    ('(decisiones §90; §94, punto 2)', '(decisiones §90; §96, punto 3)'),
    ('no es una sección posterior a §%d (decisiones §94, punto 6)', 'no es una sección posterior a §%d (decisiones §96, punto 7)'),
    ('fijado en el kit (%s…) (decisiones §94, punto 6)', 'fijado en el kit (%s…) (decisiones §96, punto 7)'),
    ('(§94 o lo anterior cambiaron)', '(§96 o lo anterior cambiaron)'),
]
for a, b in R:
    assert t.count(a) == 1, ("no único", t.count(a), a[:90])
    t = t.replace(a, b)
assert "§94" not in t and "8a528133" not in t and 'RUN_ID = "R20261019T201030Z-d1bf"' not in t   # (d1bf solo en la mención del kit de origen)
open(p, "w", encoding="utf-8", newline="").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest())

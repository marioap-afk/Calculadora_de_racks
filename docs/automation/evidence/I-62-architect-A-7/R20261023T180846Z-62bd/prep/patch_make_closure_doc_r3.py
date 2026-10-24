"""Docstring de kit/make_closure.py de la ronda 3 (decisiones §96): sustituye el tramo inicial del docstring de la ronda 2 (hasta «Fidelidad de
lectura») por el de la ronda 3 y ajusta el nombre del archivo premisa del run (a7-r1.md → a7-r2.md) en el resto del docstring. El código se adapta
después con sustituciones exactas (prep/diffs/make_closure.py.diff frente al de la ronda 2). Uso: python -I -B patch_make_closure_doc_r3.py <kit>
"""
import hashlib
import os
import sys

p = os.path.join(sys.argv[1], "make_closure.py")
t = open(p, encoding="utf-8", newline="").read()
assert hashlib.sha256(t.encode("utf-8")).hexdigest().startswith(sys.argv[2]), "make_closure.py de partida (el de la ronda 2)"
assert t.startswith('"""I-62, decisiones §94: EffectiveInputClosure de la ÚNICA REVISIÓN FORMAL de la RONDA 2')
cut = "Fidelidad de lectura (decisiones §94, punto 5; fidelity.py):"
assert t.count(cut) == 1
NEW = '''"""I-62, decisiones §96: EffectiveInputClosure de la ÚNICA REVISIÓN FORMAL de la RONDA 3 de la enmienda A-7 (docs/initiatives/I-62-A-7.md, blob
8bcccb2f… de kit-params.json, PROPUESTA ronda 3) por un ARCHITECT independiente, acotada a A62-A7R2-R1 y a sus consecuencias (decisiones §96,
puntos 2, 3 y 6), en una celda `claude-cli` medida y elegible (autorización del Owner CLAUDE-CLI-I62 = A: revisiones de ARCHITECT de I-62, solo lectura).
La revisión solo se lanza con la compuerta 11 de launch.py en Ok: una disposición del Coordinator POSTERIOR a §96 que nombre esta revisión, fijada en
owner_gate.py (número, encabezado y SHA-256 de su texto, con el blob completo), y ninguna sección posterior. En el kit entregado la autorización
está PENDIENTE (owner_gate.py con AUTH_* en None): la compuerta 11 bloquea (§96, punto 7: «Preparar el kit acotado, pero no lanzar una nueva
revisión formal sin disposición posterior»).

Adaptado del make_closure.py del kit de A-7 ronda 2 (R20261019T201030Z-d1bf, a su vez del de la ronda 1): las mismas funciones de rangos
mecánicos, de registro por blob, de copias legibles y fieles, de transporte, de run y de política de lectura; cambian el objeto (la ronda 3), el
alcance del cierre, las comprobaciones de identidad, la lectura directa (AUTOMATION_PLAN 16.7, L593-L595) y el renderizado del prompt, que añade
el índice de las rutas del cierre y la autocomprobación de las referencias (decisiones §96, punto 6: «Exigir referencias relativas válidas,
presentes en el cierre y comprobadas contra el archivo real»). El run tiene, además de las copias fieles, TRES archivos: order.txt (= cuerpo pegado
de §96, 3 198 bytes; premisa), a7-r2.md (el texto de la ronda 2, blob 8a528133…, byte a byte; premisa: el hallazgo de la ronda 2 lo cita y la pureza
del delta se juzga frente a él) y prompt.md (renderizado desde prompt.template.md; nunca premisa).

Cierre canónico, en el commit del clon (CloneCommit de kit-params.json: b9bfae50, con la publicación de la ronda 3, la custodia del resultado `run`
de sus guardas y decisiones §96), cada archivo fijado por su blob (los directorios por su árbol):
  * A-7 ronda 3, entera (objeto): cabecera, mapa de secciones, filas L1-L22 de §2, el texto de A7-1 con sus ocho puntos y las categorías (a)-(d), la
    tabla de §3.3, §3.6 y §3.7 (correcciones de las rondas 2 y 3, fila por fila), las viñetas de §3.1, §3.4 y §6, los párrafos de §3.5, las tablas
    de §4, §5, §8 (Q-A7-01..06, Q-A7R2-01..03 y Q-A7R3-01..03) y §11;
  * el preflight de la ronda 3 (docs/automation/evidence/I-62-A7/r3/ entero, con su MANIFEST.sha256 comprobado: q0-q8, el RebaseMap en q8, las
    guardas de la ronda 3, su autoprueba y su resultado `run`);
  * el preflight de la ronda 2 (docs/automation/evidence/I-62-A7/r2/ entero, sin cambio desde la ronda 2: la evidencia de R1-R3 corregidos);
  * la revisión de la ronda 2 (output.json con A62-A7R2-R1 y A62-A7R2-O1..O4, audit.json y README.md de R20261019T201030Z-d1bf);
  * las fuentes literales de A-7 §2 con sus rangos (V14 §20.3.1, B.8.7 y B.9; AUTOMATION_PLAN 16.3, 16.6, 16.7, 16.8, 16.9, 16.13 y 16.24; RAE §10 y
    §15.2; A-6, reglas citadas; el esquema relay-record/v2), la A-5 (A5-1, A5-2) y el fin de A6-8 (A-6 L815-L816);
  * decisiones §90-§96 (con §62, U-19; §73, A-5 AGREED; §78, la línea del Owner), entera como archivo y con sus rangos;
  * la determinación de §91 y el resto de s91/, los borradores de §92 (s92/), del banco README, designaciones, p1-build.json, p3-n4.json y
    p4-nc1.json, y readings/readings.md (orientación: el origen de A-7 y la referencia R7-A);
  * el kit a6m7-measure entero (A-7 §10) y el paquete B-min entero (árbol 3caa0dfa: instrumento y aceptante fijados);
  * la base normativa: LIFECYCLE, ADR-0046, ADR-0048, el registro del Freeze, A-1..A-4 (cabecera como orientación), controller-contracts.md, el mapa
    de cláusulas y su esquema, los esquemas rackcad-input-closure/v1 y rackcad-role-invocation/v1;
  * la evidencia de I-62 (I-62-evidence.md, §192-§197 como orientación; evidencia y no autoridad).
  Fuera: docs/automation/evidence/I-62-C1C3/ y todo material de I-65; el preflight de la ronda 1 (I-62-A7/ fuera de r2/ y r3/) y las disposiciones
  propuestas de O1-O8 (r2-dispositions/); la revisión de la ronda 1; el resto de I-62-A6/ (r4/ incluido: solo lo citaba el preflight de la ronda
  1); los kits de las revisiones anteriores (salvo los tres archivos de la revisión de la ronda 2); transcripciones, memoria, fixture y el
  repositorio de la entrega.
'''
t = NEW + t[t.index(cut):]
t = t.replace(cut, "Fidelidad de lectura (decisiones §94, punto 5; §96, punto 6: componentes congelados reutilizados; fidelity.py):", 1)
R = [("bytes exactos o no fuera legible fielmente, el script se detiene (STOP). Los archivos premisa del run (order.txt, a7-r1.md) no pueden estar",
      "bytes exactos o no fuera legible fielmente, el script se detiene (STOP). Los archivos premisa del run (order.txt, a7-r2.md) no pueden estar"),
     ("exactamente order.txt, prompt.md, a7-r1.md y las copias fieles, iguales a las",
      "exactamente order.txt, prompt.md, a7-r2.md y las copias fieles, iguales a las")]
for a, b in R:
    assert t.count(a) == 1, a[:80]
    t = t.replace(a, b)
open(p, "w", encoding="utf-8", newline="").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest())

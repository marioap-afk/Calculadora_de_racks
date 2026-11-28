"""Genera I-64-f4b-pbt5-package-v2.3.md desde la v2.2 con las siete decisiones del Owner.

- La cabecera hasta «## 4. La lista del Smoke-PB» y la cola desde «## 5. Cómo se ejecuta» son nuevas (HEAD y TAIL).
- §4 se conserva, con filas enteras sustituidas por su comienzo («| SPB-07 |», ...) y unos pocos textos exactos.
Cada marca o texto viejo tiene que aparecer una sola vez; si no, para sin escribir. No sobrescribe.
"""
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
base = os.path.dirname(here)
SRC, DST = "I-64-f4b-pbt5-package-v2.2.md", "I-64-f4b-pbt5-package-v2.3.md"

HEAD = open(os.path.join(here, "pkg_v23_head.md"), encoding="utf-8").read()
TAIL = open(os.path.join(here, "pkg_v23_tail.md"), encoding="utf-8").read()

ROWS = {
    "| P-02 |": "| P-02 | [G] §2; AGENTS; [S2] F4-E59; **Q-PBT5-1: «Extender la dispensa»** | Core Full local y los 19 filtros de UI de los `RequiredTests` del contrato de PB-T4e, cada uno en su proceso; el SHA-256 y la fecha de `RackFrameConfigurator.layout`, antes y después. **Sin la UI Full local:** la dispensa de Smoke-2 (DV-05 / O-08) extendida a F4B por el Owner | Core y los filtros con los totales esperados. El cambio del archivo de disposición, si lo hay, se registra |",
    "| P-08 |": None,  # se conserva y se le añade P-09 detrás (abajo)
    "| SPB-07 |": "| SPB-07 | D-PB-06 (b); OD-PBTC-2; F4B-E04, M-7; (b): A2-11, la mitad del estilo (M-15); **Q-PBT5-2: «Contar antes en otros»** | **Antes, P-09:** el recuento de los sobres con `DimensionStyle` no nulo en las copias de D1 y en las de los demás dibujos reales de la selección de Smoke-2, en solo lectura. (a) **Si alguno lo guarda:** en una copia nueva de ese dibujo, RACKEDITAR → «Actualizar» → el estilo de sus cotas.<br>(b) En otra copia nueva del mismo dibujo, lo mismo por «Editar todo» → «Actualizar» | El recuento por dibujo; el estilo de las cotas | **Si alguno lo guarda:** PASS si las cotas salen con el estilo guardado, en (a) y en (b); FAIL si no. **Si ninguno lo guarda:** el paso queda **PENDIENTE DEL OWNER**, que decide si acepta el criterio 3 de §3.3 cumplido en parte (es lo que dice su respuesta: «si ninguno, te vuelve»). No se fabrica | S |",
    "| SPB-45 |": "| SPB-45 | OWNER-1 de PB-T3c (F4B-E48); **Q-PBT5-4: «Construirlos y rotularlos»** | **CONSTRUCTED REPRESENTATIVE TEST CASES.** Copia y gemela, las dos construidas igual: el lateral del último poste insertado con RACKEDITAR («Insertar lateral») y una segunda referencia de esa definición en la primera presentación, con `-LAYOUT` y `-INSERT` tecleados → panel: un frente menos → «Actualizar» → `RCDUMP`. En la gemela, la misma edición con RACKEDITAR → `RCDUMP` | Las dos referencias, la de modelo y la de papel, borradas con la definición; si hay aviso; igual que la gemela (MA H-063) | PASS si el panel hace lo mismo que el clásico. La falta de aviso, OBSERVADO (riesgo ya informado). FAIL si el panel deja una referencia sin definición o difiere del clásico sin declaración | S |",
    "| SPB-46 |": "| SPB-46 | OWNER-2 de PB-T3c; [T3] §5.5 (1), «con una referencia anidada»; [BM] §3; **Q-PBT5-4** | **CONSTRUCTED REPRESENTATIVE TEST CASES.** Copia y gemela: el lateral insertado, metido en un bloque del usuario con `-BLOCK` tecleado (nombre `ZZ-CONSTRUCTED-SPB46`) e insertado otra vez con `-INSERT` → panel: un frente menos → «Actualizar» → `RCDUMP` (`rcblk` y `rcnest`). En la gemela, RACKEDITAR | Qué queda del bloque del usuario y de las definiciones anidadas (la purga de un nivel) | Como SPB-45 | S |",
    "| SPB-57 |": "| SPB-57 | [T3] §5.5 (4); `PushBackPilotUpdateRun.LockedLayerPrefix`; **Q-PBT5-4** | **CONSTRUCTED REPRESENTATIVE TEST CASES.** Copia y gemela: el lateral insertado y la capa de su referencia bloqueada con `-LAYER` → panel: un frente menos → «Actualizar» → `RCDUMP`. En la gemela, la misma edición con RACKEDITAR → `RCDUMP` | En el panel, un texto que empieza por «LOCKED_LAYER: » y nada escrito; en la gemela, lo que haga el clásico | PASS si el panel rechaza sin escribir nada. Lo del clásico se registra como hecho | S |",
    "| SPB-58 |": "| SPB-58 | [T3] §5.5 (5), el último vínculo; [BM] M-2b; `PushBackPilotUpdatePlan.AllViewsObsoleteReason`; **Q-PBT5-4** | **CONSTRUCTED REPRESENTATIVE TEST CASES.** Copia y gemela, sobre un rack sin referencias anidadas: el lateral insertado; `ERASE` de las demás referencias del rack y `-PURGE` de sus definiciones, tecleados (el método de [BM] M-2b) → seleccionar el lateral → panel: un frente menos → «Actualizar» → `RCDUMP`. En la gemela, RACKEDITAR | En el panel, «Todas las vistas del rack quedarían obsoletas: no se elimina el último vínculo del rack.» y nada escrito; en la gemela, lo medido en [BM] M-2b | PASS si el panel rechaza sin escribir nada | S |",
    "| SPB-59 |": "| SPB-59 | [T3] §5.5 (6); [T3] `:915` (la cuenta incluye `EraseUnit`); [S2], pasos 78 a 80; R-PB-03; **Q-PBT5-10: «Hacerlas como en Smoke-2»** | Un AutoCAD propio, lanzado con `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT=2` **solo en el entorno de ese proceso**, con una copia con el lateral insertado → panel: un frente menos (la segunda unidad puede ser el borrado) → «Actualizar» → `RCDUMP` → «Descartar» → `QUIT` → «No». Antes y después, ninguna variable en el usuario, la máquina ni el shell | «No se aplicó: La escritura falló antes de confirmarse: …»; ninguna vista cambia ni se borra; una entrada de Deshacer | PASS si el aborto no deja nada escrito ni borrado | S |",
    "| SPB-60 |": "| SPB-60 | [T3] §5.5 (6), el sellado; [S2], paso 82; **Q-PBT5-10** | Lo mismo que SPB-59, en otro AutoCAD propio, con `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL=before` | El estado desconocido, «Comprobar ahora» y que nada cambió | Como el paso 82 de [S2] | S |",
    "| SPB-61 |": "| SPB-61 | [T3] §5.5 (6); [S2], paso 84; **Q-PBT5-10** | Lo mismo, en otro AutoCAD propio, con `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL=after` | El estado desconocido; la escritura confirmada, con el borrado incluido | Como el paso 84 de [S2] | S |",
}
P09 = "| P-09 | **Q-PBT5-2: «Contar antes en otros»**; [S2] `prep/selection.md` | **El recuento del estilo de cotas, antes de SPB-07:** de cada original de la selección de Smoke-2 con racks Push Back, además de D1 y D2, un respaldo y una copia, con la regla de P-06; cada copia abierta en la instancia de la sesión, solo para leer (`RCDUMP estilo`), y cerrada sin guardar; el recuento, con `dimkeys.py` de [BM] | Respaldo = original; copia = original; el original, igual antes y después. El recuento, por dibujo y sin nombres. **Si hay alguno con estilo:** dos copias más de ese dibujo para SPB-07 (a) y (b) |"

TEXTS = [
    ("**Resultados.** Son los de Smoke-2. Adoptarlos en F4B lo decide el Owner (Q-PBT5-9); hasta entonces, son la forma de registrar.",
     "**Resultados.** Son los de Smoke-2, adoptados en F4B por el Owner (Q-PBT5-9: «Definición y reglas»), con el protocolo OBSERVADO (traer la evidencia antes de decidir) y una copia por escenario."),
    ("- **Construir:** poner el dibujo, con órdenes nativas de AutoCAD, en un estado que el producto no crea. Necesita la respuesta A de Q-PBT5-4 (M-13).",
     "- **Construir:** poner el dibujo, con órdenes nativas de AutoCAD, en un estado que el producto no crea. Lo autorizó el Owner (Q-PBT5-4: «Construirlos y rotularlos»): los cuatro contextos de SPB-45, 46, 57 y 58, en copias, rotulados **CONSTRUCTED REPRESENTATIVE TEST CASES** y cada uno comparado con RACKEDITAR en una gemela."),
    ("""**Dependen de una respuesta del Owner:**
- SPB-45, 46, 57 y 58, de Q-PBT5-4;
- SPB-59 a 61, de Q-PBT5-10;
- la parte de SPB-07 en los demás dibujos, de Q-PBT5-2.
""",
     """**Las decisiones del Owner que fijan pasos** (`owner-pbt5-decisions.md`):
- SPB-45, 46, 57 y 58 se construyen y se rotulan (Q-PBT5-4);
- SPB-59 a 61 se hacen como en Smoke-2 (Q-PBT5-10);
- SPB-07 va detrás del recuento de P-09, y si ningún dibujo guarda un estilo, vuelve al Owner (Q-PBT5-2).
"""),
    ("**LIMITATION ya previstos:** SPB-22, SPB-43 y SPB-65. SPB-07, SPB-47, SPB-48 y SPB-56 lo serán si no hay casos reales.",
     "**LIMITATION ya previstos:** SPB-22, SPB-43 y SPB-65. SPB-47, SPB-48 y SPB-56 lo serán si no hay casos reales. SPB-07 sin casos reales no es LIMITATION: vuelve al Owner."),
    ("Son SPB-59 a SPB-61, y dependen de Q-PBT5-10.",
     "Son SPB-59 a SPB-61, y el Owner las autorizó (Q-PBT5-10)."),
]


def main():
    src = os.path.join(base, SRC)
    dst = os.path.join(base, DST)
    if os.path.exists(dst):
        sys.exit(f"STOP: {DST} ya existe")
    text = open(src, encoding="utf-8").read()
    for marker in ("## 4. La lista del Smoke-PB\n", "## 5. Cómo se ejecuta\n"):
        if text.count(marker) != 1:
            sys.exit(f"STOP: la marca {marker!r} no aparece una vez")
    body = text[text.index("## 4. La lista del Smoke-PB\n"):text.index("## 5. Cómo se ejecuta\n")]
    lines = body.split("\n")
    for start, new in ROWS.items():
        idx = [i for i, ln in enumerate(lines) if ln.startswith(start)]
        if len(idx) != 1:
            sys.exit(f"STOP: la fila {start!r} aparece {len(idx)} veces")
        if new is None:
            lines.insert(idx[0] + 1, P09)
        else:
            lines[idx[0]] = new
    body = "\n".join(lines)
    for old, new in TEXTS:
        if body.count(old) != 1:
            sys.exit(f"STOP: el texto no aparece una vez: {old[:70]!r}")
        body = body.replace(old, new)
    out = HEAD + body + TAIL
    with open(dst, "x", encoding="utf-8", newline="\n") as f:
        f.write(out)
    print(f"{DST}: {len(ROWS)} filas, {len(TEXTS)} textos")


main()

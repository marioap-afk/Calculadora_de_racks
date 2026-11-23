"""Genera la v2.2 del paquete y de las preguntas desde la v2.1, con reemplazos exactos.

Cada texto viejo tiene que aparecer exactamente una vez; si no, para sin escribir nada.
No sobrescribe: si el destino existe, para.
"""
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
base = os.path.dirname(here)


def apply(src_name, dst_name, pairs):
    src = os.path.join(base, src_name)
    dst = os.path.join(base, dst_name)
    if os.path.exists(dst):
        sys.exit(f"STOP: {dst_name} ya existe")
    text = open(src, encoding="utf-8").read()
    for i, (old, new) in enumerate(pairs, 1):
        n = text.count(old)
        if n != 1:
            sys.exit(f"STOP: {src_name}, reemplazo {i}: aparece {n} veces: {old[:80]!r}")
        text = text.replace(old, new)
    with open(dst, "x", encoding="utf-8", newline="\n") as f:
        f.write(text)
    print(f"{dst_name}: {len(pairs)} reemplazos")


PKG = [
    (r"""# I-64 F4B — Paquete de PB-T5: CI exacta, Smoke-PB y gate-close de F4B (v2.1)""",
     r"""# I-64 F4B — Paquete de PB-T5: CI exacta, Smoke-PB y gate-close de F4B (v2.2)"""),
    (r"""**Estado:** v2.1. Aplica los menores de la recomprobación de la v2""",
     r"""**Estado:** v2.2. Aplica lo de la recomprobación 2 que toca al paquete (I-08, M-26 a M-29); la corrida de esta noche va en `pre-run-tonight-v2.2.md`. La v2.1 aplicaba los menores de la recomprobación de la v2"""),
    (r"""- `owner-questions-v2.1.md`: ocho preguntas en dos vueltas, sin valores por defecto, y dos informaciones;
- `pre-run-tonight-v2.1.md`: la corrida previa a la emisión, bajo la orden directa del Owner, para esta noche, con el paso de copia corregido (I-07) y las reglas de las capturas;
- `handoff-pbt5-v2.1.md` y `handoff-pbt5-v2.md`: lo leído y las discrepancias con la revisión.""",
     r"""- `owner-questions-v2.2.md`: ocho preguntas en dos vueltas, sin valores por defecto, y dos informaciones;
- `pre-run-tonight-v2.2.md`: la corrida previa a la emisión, bajo la orden directa del Owner, para esta noche, con cada bloque de PowerShell independiente (I-08) y la carpeta de trabajo en `<raiz-de-trabajo>\` (M-26);
- `handoff-pbt5-v2.2.md`, `handoff-pbt5-v2.1.md` y `handoff-pbt5-v2.md`: lo leído y las discrepancias con la revisión."""),
    (r"""## Registro de cambios v2 → v2.1
""",
     r"""## Registro de cambios v2.1 → v2.2

La «Recomprobación 2» está al final de [IR1].

| Hallazgo | Qué cambia en la v2.2 |
|---|---|
| **I-08** | Cada bloque de PowerShell de la guía funciona solo, en un proceso nuevo: `Set-StrictMode`, `$ErrorActionPreference = 'Stop'`, sus variables, sus comprobaciones y `$LASTEXITCODE`. Nunca escribe en la raíz de una unidad. §4.3 y §4.4 de la guía son un solo bloque (INV-PBT5-16 y R-14) |
| M-26 | La carpeta de trabajo de esta noche es `<raiz-de-trabajo>\`, nueva, fuera del perfil del usuario (que está dentro de un repositorio git), del worktree y del checkout principal. Se comprueba con `git rev-parse`, y si ya existe, se para. `RCDUMP` no sobrescribe una etiqueta (P-06 y §5.1) |
| M-27 | Antes de `CloseMainWindow`, el PID se comprueba por su ruta, su hora de inicio registrada y su título. Nunca `Stop-Process` (PBT5-C08) |
| M-28 | `rcdynchk` compara `rcdyn` con `IsDynamicBlock` de ActiveX, en positivo y en negativo (§4.0 y SPB-47) |
| M-29 | El bloque opcional J solo empieza a las 12:15 UTC o antes: 30 min de J y 15 del bloque Z (§0 y PBT5-C11) |
| M-30 | Las dos frases de las preguntas, corregidas |

## Registro de cambios v2 → v2.1
"""),
    (r"""más 4 opcionales (SPB-62 a SPB-65, todos S) si queda tiempo antes de las 13:00 UTC. En total, hasta 58 pasos: 49 S y 9 S+O. Está descrita en `pre-run-tonight-v2.1.md`.""",
     r"""más 4 opcionales (SPB-62 a SPB-65, todos S) si al terminar lo obligatorio son las 12:15 UTC o antes (30 min del bloque J y 15 del bloque Z). En total, hasta 58 pasos: 49 S y 9 S+O. Está descrita en `pre-run-tonight-v2.2.md`."""),
    (r"""| La orden directa del Owner de esta noche | Solo la corrida previa a la emisión (`pre-run-tonight-v2.1.md`) |""",
     r"""| La orden directa del Owner de esta noche | Solo la corrida previa a la emisión (`pre-run-tonight-v2.2.md`) |"""),
    (r"""| P-06 | [F] OB-09.2 y §6.3; M-18 | Las carpetas, fuera del worktree y de todo repositorio.""",
     r"""| P-06 | [F] OB-09.2 y §6.3; M-18; M-26 | Las carpetas, nuevas (si ya existen, se para), fuera del perfil del usuario, del worktree, del checkout principal y de todo repositorio, comprobado con `git rev-parse`; esta noche, `<raiz-de-trabajo>\`."""),
    (r"""  - `rcdyn`: si una definición es dinámica (`ACAD_ENHANCEDBLOCK` en el diccionario de extensión de su registro).""",
     r"""  - `rcdyn`: si una definición es dinámica (`ACAD_ENHANCEDBLOCK` en el diccionario de extensión de su registro). **Su control** (M-28): `rcdynchk` lo compara en c01 con la propiedad `IsDynamicBlock` de ActiveX, un oráculo independiente. El control negativo son las vistas de rack, que dan «0»; el positivo, las definiciones dinámicas, que dan «1»; y no puede haber ninguna discrepancia. Si falla, SPB-47 queda LIMITATION.
  - `c:RCDUMP` no sobrescribe: si la etiqueta ya existe en esa copia, no escribe nada (M-26)."""),
    (r"""| SPB-47 | OWNER-3 de PB-T3c; M-21 | Recuento con `rcnest` en las copias.""",
     r"""| SPB-47 | OWNER-3 de PB-T3c; M-21; M-28 | Recuento con `rcnest` en las copias, si pasaron los controles de `rcnest` y de `rcdyn`; si no, LIMITATION."""),
    (r"""- **`<carpeta-de-respaldo>` y `<carpeta-del-Smoke-PB>` quedan fuera del worktree y de todo repositorio** (M-18).""",
     r"""- **`<carpeta-de-respaldo>` y `<carpeta-del-Smoke-PB>` quedan fuera del worktree, del checkout principal, del perfil del usuario (que está dentro de un repositorio git) y de todo repositorio** (M-18 y M-26). Se crean nuevas, y si ya existen, se para. Esta noche, `<carpeta-de-respaldo>` y `<carpeta-del-Smoke-PB>`, que al custodiar pasan a sus marcadores."""),
    (r"""Su texto exacto está en `pre-run-tonight-v2.1.md`, §4.6. Antes de usarlas se comprueban una vez en la primera copia, con los controles de M-21 (R-5).""",
     r"""Su texto exacto está en `pre-run-tonight-v2.2.md`, §4.6. Antes de usarlas se comprueban una vez en la primera copia, con los controles de M-21 y el de `rcdyn` contra `IsDynamicBlock` (M-28; R-5)."""),
    (r"""Si la instancia se queda sin documentos, se cierra por su PID y se relanza. Las órdenes exactas, en `pre-run-tonight-v2.1.md` §4.7 y §4.8.""",
     r"""Si la instancia se queda sin documentos, se cierra por su PID y se relanza. Antes de cerrarla se comprueba que el PID sigue siendo el suyo: su ruta, su hora de inicio registrada y un título sin documento (M-27). Nunca se usa `Stop-Process`. Las órdenes exactas, en `pre-run-tonight-v2.2.md` §4.7 y §4.8.
- **Cada bloque de PowerShell funciona solo** (I-08): `Set-StrictMode -Version Latest`, `$ErrorActionPreference = 'Stop'`, sus variables y sus comprobaciones. Para si un valor está vacío o sin rellenar, o si una ruta no existe, y nunca escribe en la raíz de una unidad. El PID de AutoCAD pasa de un bloque a otro por un archivo de registro, no por una variable."""),
    (r"""con la lectura que fijó la sesión principal (`pre-run-tonight-v2.1.md`).""",
     r"""con la lectura que fijó la sesión principal (`pre-run-tonight-v2.2.md`)."""),
    (r"""  - las reglas completas están en `pre-run-tonight-v2.1.md` §8, y valen también para la corrida contratada.""",
     r"""  - las reglas completas están en `pre-run-tonight-v2.2.md` §8, y valen también para la corrida contratada. Lo que se tapa incluye todo el historial visible de la línea de órdenes que lleve una ruta."""),
    (r"""Nunca se usa COM para abrir o cerrar documentos (I-07) |""",
     r"""Nunca se usa COM para abrir o cerrar documentos (I-07), ni `Stop-Process`. Antes de `CloseMainWindow`, el PID se comprueba por su ruta, su hora de inicio y su título (M-27) |"""),
    (r"""| PBT5-C11 | La frontera horaria | A las 13:00 UTC, se para en el límite de paso y se espera |""",
     r"""| PBT5-C11 | La frontera horaria | A las 13:00 UTC, se para en el límite de paso y se espera; después solo SPB-51 y SPB-52, que leen. El bloque J, solo si lo obligatorio termina a las 12:15 UTC o antes (M-29) |"""),
    (r"""| INV-PBT5-15 | Ninguna suite completa de UI en local sin la respuesta de Q-PBT5-1 (F4-E59) |""",
     r"""| INV-PBT5-15 | Ninguna suite completa de UI en local sin la respuesta de Q-PBT5-1 (F4-E59) |
| INV-PBT5-16 | Cada bloque de PowerShell funciona solo, en un proceso nuevo: para si un valor está vacío o sin rellenar o si una ruta no existe, y nunca escribe en la raíz de una unidad (I-08) |"""),
    (r"""  - `_tools/control_patterns.py` lee sus muestras de `_tools/control-samples.local.txt`, que tampoco se custodia, y `check_pbt5.py` lo analiza (M-19).""",
     r"""  - `_tools/control_patterns.py` lee sus muestras de `_tools/control-samples.local.txt`, que tampoco se custodia, y `check_pbt5.py` lo analiza (M-19).
  - `check_pbt5.py` solo admite la carpeta de trabajo de esta noche (`<raiz-de-trabajo>`), desde `_tools/allow.local.txt`, que no se custodia. Al custodiar, esa carpeta pasa a sus marcadores."""),
    (r"""| R-13 | Un `Documents.Open` por COM se engancha a cualquier AutoCAD abierto |""",
     r"""| R-14 | La herramienta de PowerShell no conserva variables entre órdenes: un bloque que dependa de otro puede escribir en la raíz de una unidad o saltarse sus comprobaciones (I-08) | Cada bloque, independiente; `Set-StrictMode`, `Stop` y sus comprobaciones; §4.3 y §4.4 de la guía, unidos |
| R-13 | Un `Documents.Open` por COM se engancha a cualquier AutoCAD abierto |"""),
    (r"""4. **Esta noche, si se cumple su frontera:** la corrida previa (`pre-run-tonight-v2.1.md`) y su custodia, en un commit documental. No emite PB-T5.""",
     r"""4. **Esta noche, si se cumple su frontera:** la corrida previa (`pre-run-tonight-v2.2.md`) y su custodia, en un commit documental. No emite PB-T5."""),
    (r"""En `owner-questions-v2.1.md`, sin valores por defecto:""",
     r"""En `owner-questions-v2.2.md`, sin valores por defecto:"""),
    (r"""se comprueban en la corrida (`pre-run-tonight-v2.1.md` §4.1 y §4.5).""",
     r"""se comprueban en la corrida (`pre-run-tonight-v2.2.md` §4.1 y §4.5, bloques 1, 3, 5 y 6)."""),
]

QST = [
    (r"""# I-64 F4B PB-T5 — Preguntas al Owner (v2.1)""",
     r"""# I-64 F4B PB-T5 — Preguntas al Owner (v2.2)"""),
    (r"""**Qué cambia en la v2.1** (la recomprobación de la v2):""",
     r"""**Qué cambia en la v2.2** (la recomprobación 2): dos frases que decían de más, en Q-PBT5-8 y en Q-PBT5-1 (M-30). Nada más.

**Qué cambió en la v2.1** (la recomprobación de la v2):"""),
    (r"""en Smoke-2 viste en directo la mitad de la corrida, y esta vez puede que no veas ninguna parte.""",
     r"""en Smoke-2 viste en directo los pasos 0 a 29 de 88, y esta vez puede que no veas ninguno."""),
    (r"""  - Esos 19 ya corrieron en local en la precondición 6 de PB-T4e, y ninguno nombra las clases de prueba que reescriben el archivo de tu perfil.""",
     r"""  - Esos 19 ya corrieron en local en la precondición 6 de PB-T4e, y ninguno nombra las clases conocidas que reescriben el archivo de tu perfil. Esa lista no es completa, así que el archivo se mide antes y después."""),
]

apply("I-64-f4b-pbt5-package-v2.1.md", "I-64-f4b-pbt5-package-v2.2.md", PKG)
apply("owner-questions-v2.1.md", "owner-questions-v2.2.md", QST)

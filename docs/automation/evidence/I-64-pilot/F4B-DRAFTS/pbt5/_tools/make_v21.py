"""Genera la v2.1 del paquete y de las preguntas desde la v2, con reemplazos exactos.

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
    (r"""# I-64 F4B — Paquete de PB-T5: CI exacta, Smoke-PB y gate-close de F4B (v2)""",
     r"""# I-64 F4B — Paquete de PB-T5: CI exacta, Smoke-PB y gate-close de F4B (v2.1)"""),
    (r"""**Estado:** v2, para la comprobación acotada. **No está emitido:** la emisión es del Owner (Q-PBT5-7).""",
     r"""**Estado:** v2.1. Aplica los menores de la recomprobación de la v2 (APTO TRAS CORRECCIONES, B 0 / I 1 / M 8; su importante, I-07, es de la corrida de esta noche y se corrige en `pre-run-tonight-v2.1.md`). **No está emitido:** la emisión es del Owner (Q-PBT5-7)."""),
    (r"""- `owner-questions-v2.md`: ocho preguntas en dos vueltas, sin valores por defecto, y dos informaciones;
- `pre-run-tonight-v2.md`: la corrida previa a la emisión, bajo la orden directa del Owner, para esta noche;
- `handoff-pbt5-v2.md`: lo leído y las discrepancias con la revisión.""",
     r"""- `owner-questions-v2.1.md`: ocho preguntas en dos vueltas, sin valores por defecto, y dos informaciones;
- `pre-run-tonight-v2.1.md`: la corrida previa a la emisión, bajo la orden directa del Owner, para esta noche, con el paso de copia corregido (I-07) y las reglas de las capturas;
- `handoff-pbt5-v2.1.md` y `handoff-pbt5-v2.md`: lo leído y las discrepancias con la revisión."""),
    (r"""## Registro de cambios v1 → v2
""",
     r"""## Registro de cambios v2 → v2.1

La recomprobación de la v2 está al final de [IR1], en «Recomprobación 1 (v2)».

| Hallazgo | Qué cambia en la v2.1 |
|---|---|
| **I-07** | El paso de una copia a la siguiente: se abre la siguiente con `vla-open`, se cierra la paleta de referencias externas y la anterior se cierra desde la nueva con `vla-close … :vlax-false`. Sin COM. Va en `pre-run-tonight-v2.1.md` §4.7 y §4.8, en §5.3, y en PBT5-C08, R-12 y R-13 |
| M-18 | Ninguna escritura sobrescribe; el SHA-256 de D1 se comprueba antes del respaldo; las carpetas, fuera de todo repositorio (P-06, §5.1 y §6.2) |
| M-19 | `control_patterns.py` lee sus muestras de un archivo que no se custodia, y `check_pbt5.py` lo analiza (§6.5) |
| M-20 | La línea de base de `DBMOD` y de Deshacer se toma después de seleccionar y de la vista previa (SPB-04, SPB-15 y SPB-35). La selección por expresión es equivalente a la de Smoke-2 |
| M-21 | El control de `rcblk`; `rcnest` con el indicador del contenedor y `rcdyn`; SPB-47 excluye el control positivo (§4.0 y SPB-47) |
| M-22 | La cabecera del registro lleva el sha256 del documento de la corrida, del paquete y de la guía (§5.6) |
| M-23 | Q-PBT5-8 lleva la regla de reejecutar la CI y el porqué de la recomendada (PBT5-C15) |
| M-24 | Q-PBT5-1 nombra los 19 filtros de PB-T4e |
| M-25 | SPB-62 a SPB-65, como bloque opcional al final de la corrida de esta noche (§0) |
| Las capturas | Dónde van y cómo se redactan antes de custodiarlas (§5.6; `pre-run-tonight-v2.1.md` §8) |

## Registro de cambios v1 → v2
"""),
    (r"""- **Esta noche:** con la orden directa del Owner, una corrida previa a la emisión, de 54 pasos (45 S y 9 S+O), descrita en `pre-run-tonight-v2.md`. No es el Smoke-PB contratado.""",
     r"""- **Esta noche:** con la orden directa del Owner, una corrida previa a la emisión, de 54 pasos obligatorios (45 S y 9 S+O), más 4 opcionales (SPB-62 a SPB-65, todos S) si queda tiempo antes de las 13:00 UTC. En total, hasta 58 pasos: 49 S y 9 S+O. Está descrita en `pre-run-tonight-v2.1.md`. No es el Smoke-PB contratado."""),
    (r"""| La orden directa del Owner de esta noche | Solo la corrida previa a la emisión (`pre-run-tonight-v2.md`) |""",
     r"""| La orden directa del Owner de esta noche | Solo la corrida previa a la emisión (`pre-run-tonight-v2.1.md`) |"""),
    (r"""| P-06 | [F] OB-09.2 y §6.3 | Respaldo de cada original en `<carpeta-de-respaldo>`; después, las copias con `copy` desde el original y el manifiesto `copias.json` | Respaldo = original; copia = original; el original, igual antes y después. **Los originales nunca se abren** |""",
     r"""| P-06 | [F] OB-09.2 y §6.3; M-18 | Las carpetas, fuera del worktree y de todo repositorio. El SHA-256 del original, frente al registrado (para D1, el de `F4-SMOKE-2/run/integrity.md`), **antes** del respaldo. Después, el respaldo en `<carpeta-de-respaldo>`, las copias con `copy` desde el original y el manifiesto `copias.json`. **Ninguna escritura sobrescribe:** se para si el destino ya existe | Respaldo = original; copia = original; el original, igual antes y después. **Los originales nunca se abren** |"""),
    (r"""| SPB-04 | [S2], pasos 0.6 y 0.7 | `RACKPANEL`; las funciones de inventario; `RCDUMP pre`; la lista de Deshacer; `DBMOD` y `PICKFIRST`, leídos y sin cambiarlos | «RackCad: panel Workspace mostrado.», sin errores; `DBMOD` = 0, o explicado como en [S2] 0.7 | PASS si la línea de base queda registrada | S |""",
     r"""| SPB-04 | [S2], pasos 0.6, 0.7 y 75; M-20 | `RACKPANEL`; las funciones de inventario y sus controles; `RCDUMP pre`; `PICKFIRST`, leído y sin cambiarlo. **La línea de base de `DBMOD` y de Deshacer se toma después de seleccionar el rack y de la vista previa del panel:** en [S2] 75, esa selección puso `DBMOD` = 1 | «RackCad: panel Workspace mostrado.», sin errores; la línea de base, con su `DBMOD` explicado como en [S2] 0.7 y 75 | PASS si la línea de base queda registrada | S |"""),
    (r"""| SPB-15 | [T4] §5.5; paso 7 de PB-T4e; F4B-E55 («los eventos del puente durante el modal»); M-09 | `RCDUMP` → «Editar todo» → «Cerrar» sin cambios → `RCDUMP`; `DBMOD`; la lista de Deshacer; el estado a los 5 s |""",
     r"""| SPB-15 | [T4] §5.5; paso 7 de PB-T4e; F4B-E55 («los eventos del puente durante el modal»); M-09; M-20 | Selección y vista previa → la línea de base de `DBMOD` y de Deshacer → `RCDUMP` → «Editar todo» → «Cerrar» sin cambios → `RCDUMP`; `DBMOD`; la lista de Deshacer; el estado a los 5 s |"""),
    (r"""| SPB-35 | 7 | «Editar todo» → cerrar sin «Actualizar»; `DBMOD` y Deshacer, antes y después | Ningún cambio | PASS / FAIL | S |""",
     r"""| SPB-35 | 7; M-20 | «Editar todo» → cerrar sin «Actualizar»; `DBMOD` y Deshacer antes (después de la selección y de la vista previa) y después | Ningún cambio | PASS / FAIL | S |"""),
    (r"""- **Inventario** (`RCDUMP <etiqueta>`): las funciones de solo lectura de [BM] (`lisp_src.txt`), tecleadas en cada documento, más dos de solo lectura nuevas (§5.3):
  - `rcblk`: todas las definiciones de bloque, para ver la purga;
  - `rcnest`: las referencias de vistas de rack dentro de otras definiciones, también anónimas y dinámicas.""",
     r"""- **Inventario** (`RCDUMP <etiqueta>`): las funciones de solo lectura de [BM] (`lisp_src.txt`), tecleadas en cada documento, más tres de solo lectura nuevas (§5.3):
  - `rcblk`: todas las definiciones de bloque, para ver la purga. **Su control** (M-21): en c01, más de 0 líneas `BLK`, y entre ellas todas las definiciones de rack de las líneas `DEF`;
  - `rcnest`: las referencias de vistas de rack dentro de otras definiciones, con el indicador del contenedor y si es dinámico;
  - `rcdyn`: si una definición es dinámica (`ACAD_ENHANCEDBLOCK` en el diccionario de extensión de su registro).

  **El control de `rcnest`:** la planta anidada de D1, dentro de un bloque pegado «A$C…» ([BM] §3). Ese contenedor no es dinámico ni anónimo, y no cuenta como caso de SPB-47."""),
    (r"""| SPB-47 | OWNER-3 de PB-T3c | Recuento con `rcnest` en las copias: vistas de rack dentro de una definición dinámica o anónima. Si hay alguna, lo mismo que SPB-46, sin construir nada |""",
     r"""| SPB-47 | OWNER-3 de PB-T3c; M-21 | Recuento con `rcnest` en las copias. Solo cuentan los contenedores dinámicos (`rcdyn` = 1) o anónimos `*U…`; el control positivo de D1, en un bloque pegado, no cuenta. Si hay alguno, lo mismo que SPB-46, sin construir nada |"""),
    (r"""- Las salidas del producto, solo en `<carpeta-de-salidas>`.
""",
     r"""- Las salidas del producto, solo en `<carpeta-de-salidas>`.
- **`<carpeta-de-respaldo>` y `<carpeta-del-Smoke-PB>` quedan fuera del worktree y de todo repositorio** (M-18). Si no, P-01 y SPB-51 fallarían, y habría copias de dibujos de clientes al alcance de un commit.
- **Ninguna escritura sobrescribe un archivo existente:** se para si el destino existe.
"""),
    (r"""- **El inventario:** las funciones de [BM], tecleadas en cada documento porque SECURELOAD impide `load`, más `rcblk` y `rcnest`, de solo lectura. Su texto exacto está en `pre-run-tonight-v2.md`, §4. Antes de usarlas se comprueban una vez en la primera copia, con la referencia anidada conocida de D1 como control positivo (R-5).""",
     r"""- **El inventario:** las funciones de [BM], tecleadas en cada documento porque SECURELOAD impide `load`, más `rcblk`, `rcdyn` y `rcnest`, de solo lectura. Su texto exacto está en `pre-run-tonight-v2.1.md`, §4.6. Antes de usarlas se comprueban una vez en la primera copia, con los controles de M-21 (R-5).
- **El paso de una copia a la siguiente** (I-07):
  1. desde la copia activa, se abre la siguiente con `vla-open`;
  2. en ella se cierra la paleta de referencias externas (`EXTERNALREFERENCESCLOSE`), que D1 abre siempre;
  3. se teclea el inventario;
  4. la anterior se cierra desde la nueva con `vla-close … :vlax-false`.

  AutoCAD nunca se queda sin un documento con el LISP cargado, y **nunca se usa COM**, que podría engancharse a un AutoCAD que no abrió la sesión. Si la instancia se queda sin documentos, se cierra por su PID y se relanza. Las órdenes exactas, en `pre-run-tonight-v2.1.md` §4.7 y §4.8."""),
    (r"""- **La corrida previa de esta noche:** la sesión, por la orden directa del Owner y con la lectura que fijó la sesión principal (`pre-run-tonight-v2.md`).""",
     r"""- **La corrida previa de esta noche:** la sesión, por la orden directa del Owner y con la lectura que fijó la sesión principal (`pre-run-tonight-v2.1.md`)."""),
    (r"""  - `owner-summary.md`: una fila por paso, la sección «Para tu revisión» con los 9 S+O y sus capturas, los LIMITATION con su motivo, los FAIL y los OBSERVADO con toda su evidencia, y la información de R-T4-39.
""",
     r"""  - `owner-summary.md`: una fila por paso, la sección «Para tu revisión» con los 9 S+O y sus capturas, los LIMITATION con su motivo, los FAIL y los OBSERVADO con toda su evidencia, y la información de R-T4-39.
- **La cabecera de `run-log.md`** (M-22) lleva el sha256 del documento de la corrida, el del paquete y el de la guía redactada, que son los textos con que se opera cada paso.
- **Las capturas:**
  - las crudas, solo en `<carpeta-del-Smoke-PB>\screens\raw\`, y nunca se custodian;
  - las custodiadas, recortadas al área del paso o tapadas con un rectángulo opaco, con la lista de antes de custodiar y `derivation-record.txt`;
  - nunca se custodian la paleta de referencias externas, el aviso de NETLOAD, los diálogos de archivos ni una captura con otra aplicación;
  - las reglas completas están en `pre-run-tonight-v2.1.md` §8, y valen también para la corrida contratada.
"""),
    (r"""- **Fuera:** `<carpeta-del-Smoke-PB>` (copias, respaldos, salidas, inventarios y capturas) y el `bin/` y el `obj/` del worktree.""",
     r"""- **Fuera:** `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>` (copias, respaldos, salidas, inventarios y capturas), fuera de todo repositorio (M-18), y el `bin/` y el `obj/` del worktree."""),
    (r"""| PBT5-C08 | Un AutoCAD del Owner abierto al compilar, o que haría falta tocar | No se toca; STOP del build |""",
     r"""| PBT5-C08 | Un AutoCAD del Owner abierto al compilar, o que haría falta tocar | No se toca; STOP del build. Nunca se usa COM para abrir o cerrar documentos (I-07) |"""),
    (r"""La regla la confirma el Owner con la forma del contrato (Q-PBT5-8) |""",
     r"""La regla va escrita en Q-PBT5-8, y el Owner la confirma al responderla (M-23) |"""),
    (r"""  - `_tools/check_pbt5.py` lee sus patrones de `_tools/patterns.local.txt`, que **no se custodia**.""",
     r"""  - `_tools/check_pbt5.py` lee sus patrones de `_tools/patterns.local.txt`, que **no se custodia**.
  - `_tools/control_patterns.py` lee sus muestras de `_tools/control-samples.local.txt`, que tampoco se custodia, y `check_pbt5.py` lo analiza (M-19)."""),
    (r"""| R-11 | La corrida previa no se acepta como parte del Smoke-PB | Q-PBT5-9; se repite lo que haga falta en la contratada |""",
     r"""| R-11 | La corrida previa no se acepta como parte del Smoke-PB | Q-PBT5-9; se repite lo que haga falta en la contratada |
| R-12 | D1 abre la paleta de referencias externas en cada apertura: quita el foco y muestra rutas | `EXTERNALREFERENCESCLOSE` antes de cualquier captura que se vaya a custodiar, y nunca se custodia (I-07) |
| R-13 | Un `Documents.Open` por COM se engancha a cualquier AutoCAD abierto | Sin COM: `vla-open` desde la copia activa, y si no queda documento, cerrar y relanzar la instancia propia por su PID (I-07) |"""),
    (r"""4. **Esta noche, si se cumple su frontera:** la corrida previa (`pre-run-tonight-v2.md`) y su custodia, en un commit documental. No emite PB-T5.""",
     r"""4. **Esta noche, si se cumple su frontera:** la corrida previa (`pre-run-tonight-v2.1.md`) y su custodia, en un commit documental. No emite PB-T5."""),
    (r"""En `owner-questions-v2.md`, sin valores por defecto:""",
     r"""En `owner-questions-v2.1.md`, sin valores por defecto:"""),
    (r"""- **`rcblk` y `rcnest`** no se han ejecutado.""",
     r"""- **`rcblk`, `rcdyn` y `rcnest`** no se han ejecutado.
- **`gh`, `PICKFIRST` y si la instancia lanzada con `Process.Start` sobrevive a la orden que la lanza:** se comprueban en la corrida (`pre-run-tonight-v2.1.md` §4.1 y §4.5)."""),
]

QST = [
    (r"""# I-64 F4B PB-T5 — Preguntas al Owner (v2)""",
     r"""# I-64 F4B PB-T5 — Preguntas al Owner (v2.1)"""),
    (r"""**Qué cambia respecto de la v1** (la revisión independiente de la v1, NO APTO, B 2 / I 6 / M 17):""",
     r"""**Qué cambia en la v2.1** (la recomprobación de la v2): Q-PBT5-8 lleva la regla de reejecutar la CI y el porqué de la recomendada (M-23), y Q-PBT5-1 nombra los filtros exactos (M-24). Nada más.

**Qué cambió en la v2 respecto de la v1** (la revisión independiente de la v1, NO APTO, B 2 / I 6 / M 17):"""),
    (r"""- El cierre de F4B exige tu firma en cualquier caso.
""",
     r"""- El cierre de F4B exige tu firma en cualquier caso.
- **La regla de reejecutar la CI va con esta respuesta.** Si la CI del Candidato sale en rojo **solo** por el detector de cuelgue (`blame-hang`), en una prueba ya conocida como lenta, se reejecutan una vez los trabajos fallidos sobre el mismo SHA. Otra causa, o un segundo rojo, vuelve a ti. Con A, B o C la confirmas; con D, la escribes tú.
"""),
    (r"""- **B. Adaptación, con una revisión independiente de la evidencia antes de tu firma (Recomendado).** Igual que A, pero un revisor aparte comprueba, en solo lectura, que el registro, las capturas y los inventarios dicen lo mismo. De la cadena de F4 se conserva lo que sirve sin Worker: la revisión del paquete, la CI exacta y una revisión independiente. Añade una o dos horas.""",
     r"""- **B. Adaptación, con una revisión independiente de la evidencia antes de tu firma (Recomendado).** Igual que A, pero un revisor aparte comprueba, en solo lectura, que el registro, las capturas y los inventarios dicen lo mismo. De la cadena de F4 se conserva lo que sirve sin Worker: la revisión del paquete, la CI exacta y una revisión independiente. Añade una o dos horas.
  - **Por qué B y no A,** que es el precedente: en Smoke-2 viste en directo la mitad de la corrida, y esta vez puede que no veas ninguna parte. Además, en Smoke-2 el paso 50 fue un falso FAIL de la sesión, por un clic fuera del botón, que solo se aclaró al revisar las capturas. Una revisión aparte busca ese tipo de error antes de que firmes."""),
    (r"""- **Lo que se hará esta noche, en cualquier caso:** la suite completa de Core en local, los filtros de UI de los contratos de F4B y la CI exacta. El Candidato queda «pendiente de Q-PBT5-1». La UI completa puede correr después sobre el mismo SHA.""",
     r"""- **Lo que se hará esta noche, en cualquier caso:** la suite completa de Core en local, los 19 filtros de UI de los `RequiredTests` del contrato de PB-T4e (cada uno en su proceso) y la CI exacta.
  - Esos 19 ya corrieron en local en la precondición 6 de PB-T4e, y ninguno nombra las clases de prueba que reescriben el archivo de tu perfil.
  - El Candidato queda «pendiente de Q-PBT5-1». La UI completa puede correr después sobre el mismo SHA."""),
]

apply("I-64-f4b-pbt5-package-v2.md", "I-64-f4b-pbt5-package-v2.1.md", PKG)
apply("owner-questions-v2.md", "owner-questions-v2.1.md", QST)

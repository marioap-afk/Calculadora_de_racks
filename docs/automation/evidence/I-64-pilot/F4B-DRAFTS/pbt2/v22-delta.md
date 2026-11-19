# I-64 F4B — PB-T2: de la v2.1 a la v2.2 del paquete, y lo que se aplica en el generador de PB-T2a (hallazgo por hallazgo)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Entrada:**
- la comprobación acotada de PB-T2a, `independent-review-pbt2a-v21-check.md` (citada **CA-T2a**): **EMITIBLE TRAS CORRECCIONES, 0/0/6**, sin decisiones nuevas para el Owner;
- el mensaje de la sesión: aplicar sus menores y sus respuestas a D-1 a D-14; `BOUNDED_RESULT` = «EMITIBLE TRAS CORRECCIONES 0/0/6, aplicadas»; preparar las comprobaciones de su §3.

**Salida:**
- `I-64-f4b-pbt2-gate-contract-package-v22.md`, con el registro v2.1 → v2.2 (V22-01 a V22-04) delante del de v2 → v2.1;
- `make_pbt2a_contract.py`, puesto al día;
- este archivo.

**Resultado:**
- **El paquete solo cambia por M-4,** que toca la regla de las paradas de [DP] del §6.0: el texto de C11 ya no es «solo el prefijo». Con él cambian la custodia de este archivo, la cabecera y el registro.
- **Lo demás va al generador:** M-1 a M-3, M-5 y las respuestas a D-1 a D-14.
- **M-6** va al paquete antes de emitir PB-T2b, como pide CA-T2a.
- **El diseño no cambia,** y PB-T2a no cambia fuera de PBT2A-C11.

## 1. Los seis menores

| Hallazgo | Qué decía CA-T2a | Dónde se aplica | Cómo |
|---|---|---|---|
| **M-1** | La línea base de la CI del cierre de PB-T1c no se ata al cierre ni se compara con la prevista | Generador | `ci_base_check`: el SHA de la corrida tiene el `src` y los `tests` de `PT1C_CLOSE` (`git diff --quiet <sha> PT1C_CLOSE -- src tests`), y las cifras valen (19 088, 2 439, 21). Si no, error, salvo que la sesión ponga la explicación por el diff de PB-T1c en `CI_T1C_EXPLAINED`, que entonces va a la `ExpectedEvidence`. Sin `PT1C_CLOSE`, error. Probado con commits reales (8 casos) |
| **M-2** | V2-05 compara la declaración de `editable` byte a byte | Generador | La regla de D-13, con los espacios colapsados. **Vale:** un solo campo `private readonly PushBackEditableState editable` con inicializador, en W. **Solo forma** (por ejemplo, `= new();`): se anota como hecho no material y no para. **Si no:** STOP y v3 (error al escribir) |
| **M-3** | `window_checks` no mira las capturas del estado ni los choques de nombres con PBT2B-C13 | Generador | Dos condiciones nuevas: ningún campo ni asignación de constructor de W o WC guarda una cadena de acceso que sale de `editable`, `composite`, `state` o `moduleOwner`; y ningún miembro de W o WC se llama como uno de los 26 nombres de PBT2B-C13. Las dos se cumplen ya en `d8ccbcb8` y son `assert`. Llevan autopruebas sintéticas: 20 en total, 4 que pasan y 16 que fallan |
| **M-4** | C11 dice «section 6.0 of the package», y en un contrato de PB-T2 ese §6.0 no lista las diferencias admitidas | **Paquete (V22-01)** y generador | El §6.0 dice ahora: «cambiando solo el prefijo y, en PBT2x-C11, la referencia «section 6.0 of the package» por «section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md)»». Vale para PB-T2a, PB-T2b y PB-T2c. La regla no cambia |
| **M-5** | La sección de F4B-E25 repite frases; `ExpectedEvidence[1]` dice «en los tres RED» | Generador | La sección de F4B-E25 lleva la celda del §1 y dos frases, sin repetir el §6.0. **La parte opcional no se aplica:** `ExpectedEvidence[1]` sigue literal del §6.0, para no apartarse del paquete |
| **M-6** | Dos hechos de F4B-E30 para PB-T2b y PB-T2c: la escritura de `RackFrameConfigurator.layout` en el perfil y el diseño una pasada por detrás | Ninguno en la v2.2 (V22-04) | Va al paquete antes de emitir PB-T2b |

## 2. Las respuestas a las DUDAS

| Duda | CA-T2a | Qué hace el generador |
|---|---|---|
| D-1 | Aceptar 25 en Core y anotarlo | Lo imprime en el seco: «lectura D-1 (CA-T2a, aceptada)». El seco se custodia como `make_pbt2a_contract-dry.txt` y la salida de los anclajes la recoge |
| D-2, D-3, D-4, D-6, D-7, D-9, D-10, D-11 | Correctos o dejarlos como están | Sin cambio |
| D-5 | F4B-E28 es obligatoria, y la entrada de emisión tiene que decir que se cumplen las condiciones de la pregunta 4 | Comprobación nueva: la última entrada de `f4b-dispositions.md` es «Emisión de F4B-PBT2A-DRAFT-STATE-PORT» y nombra F4B-E28 y la pregunta 4. Es AVISO en seco y error al escribir |
| D-8 | Corregir | M-4 |
| D-12 | Suficiente, con M-3 | M-3 |
| D-13 | La regla de los espacios colapsados | M-2 |
| D-14 | Correcto | `BOUNDED_RESULT` = «EMITIBLE TRAS CORRECCIONES 0/0/6, aplicadas», literal de la sesión |

## 3. El §3 de CA-T2a, preparado para escribir

El generador imprime el estado de cada paso, en seco y al escribir:

| Paso | Qué comprueba el generador | Qué queda para la sesión |
|---|---|---|
| 1. El cierre | `PT1C_CLOSE`: desciende de `833ea6bf`, la revisión desciende de él, `src` es igual, y tiene su entrada de cierre y su `task-close-package.md` | Rellenar `PT1C_CLOSE` |
| 2. V2-05 | La regla de D-13, `composite`, la costura, `state` y `moduleOwner`, las asignaciones, las suscripciones, las capturas y los nombres. También las listas y los literales de las tres clases de PB-T1c, con la CT-02 corregida | — |
| 3. La CI | `ci_base_check` (M-1) | Rellenar `CI_BASE_T1C` y, si hace falta, `CI_T1C_EXPLAINED`. Que sea la corrida `push` 4/4 lo mira la sesión |
| 4. El producto | Application, Domain y Plugin iguales que en `a18ba345`; G-L1c igual que en `95373af6`; los recuentos iguales a los de `d8ccbcb8` (`COUNTS_D8`). Si cambian, AVISO en seco y error al escribir | Explicar y poner al día `COUNTS_D8` si cambian |
| 5. La custodia | Las 15 rutas, el paquete v2.2 y el registro con Q-7 en HEAD; la comprobación custodiada con su veredicto; la entrada de emisión (D-5) | El commit de la `AuthorityRevision` |
| 6. El entorno | `MainSha` = `origin/main`; las celdas hasta el 2026-12-29; `Test-Json` | `config.toml` = Snapshot J, el binario y una sola delegación abierta |
| 7. La autorización | `BOUNDED_RESULT` | Que la remedición no traiga ningún cambio material y que PB-T1c no llegue a su intento 3 |

## 4. Observación, sin cambio

En la v2.1, al final de la fila V21-07 del registro v2 → v2.1, queda pegada la viñeta «**Marca [T2b]:** …», que debía ir en «Base de la medición». Solo afecta a cómo se ve la tabla. La v2.2 no la mueve, para que el cambio sea mínimo.

## 5. Reglas

- **Lecturas:** el repositorio se leyó solo por objetos git desde el checkout principal (`d8ccbcb8`), sin tocar el worktree.
- **Escrituras:** solo en `hs65\pbt2\`. Python, siempre con un guion escrito con la herramienta de escritura (`_work/a/make_v22.py`), sin heredoc.
- **Una incidencia:** una orden de Bash incluyó `python -I - < /dev/null` (un programa vacío por la entrada estándar). No leyó ni escribió nada, se paró y no se repitió.

# Paquete de escalamiento de I-65 a I-62: fallos del mecanismo de ejecución delegada (I-61) observados en I-65

- **Estado:** ESCALAMIENTO. Preparado por el Coordinator de I-65 por orden del Owner («Option C and Checkout Incident Containment», punto 4).
  **No modifica** la infraestructura de I-61/I-62 (esquemas, scripts, prompts, protocolo); describe problemas y propone controles para que I-62
  los evalúe con su propio proceso.
- **Sustituye** a la propuesta previa [i62-worker-environment-controls-proposal.md](i62-worker-environment-controls-proposal.md), que se conserva
  como antecedente.
- **Criterio:** controles **mecánicos y verificables** antes que nuevas prohibiciones en los prompts. En I-65 la prohibición textual se endureció
  tres veces sin eliminar el fallo.

## 1. Resumen

En I-65 hubo 12 invocaciones de Worker (subagente `claude-sonnet-5-5` de la sesión de escritorio). En 7 hubo invocaciones de un intérprete
Python prohibido, dos de ellas en F1 sin registrar hasta un barrido posterior. Además hubo tres escrituras fuera del worktree, una de ellas en el
checkout principal del Owner, con consecuencias reales; no la declaró el Worker y no la vio ningún control del relevo ni de la verificación.
Los controles negativos y la aceptación dieron falsos negativos y falsas alarmas. Todo se detectó tarde y con trabajo manual de la sesión.

## 2. Hechos

### 2.1 Invocaciones de herramientas prohibidas (intérprete Python)

| Worker (RunId) | Tarea | Orden | Momento | Registro |
|---|---|---|---|---|
| `R20261011T032758Z-3556` | F1-T1 | `python3 --version` | durante el trabajo | **no registrada** (barrido del 2026-10-12, DEV-I65-F4-17) |
| `R20261011T044815Z-7eae` | F1-T2 | `python3 -c "print(1)"` | durante el trabajo | **no registrada** (DEV-I65-F4-17) |
| `R20261011T062529Z-8e09` | F3-T1 | `python3 -` para leer `p1-report.json` | durante el trabajo | CD-I65-F3-12 |
| `R20261011T155212Z-11d5` | F4-T1 | `python3 -c "print(1)"` | durante el trabajo | DEV-I65-F4-01 |
| `R20261011T173811Z-0167` | F4-T2 corrección | `python3 --version` | durante el trabajo | DEV-I65-F4-05 |
| `R20261011T183618Z-05a9` | F4-T3 | `python --version 2>$null \| Out-Null` | tras los commits | DEV-I65-F4-06 (STOP del Worker; disposición del Owner) |
| `R20261011T215838Z-09a6` | F4-T5 | `python3 --version 2>/dev/null` | antes de cualquier commit | DEV-I65-F4-14 (STOP; S-11) |

Ninguna ejecutó scripts del producto ni el programa de análisis. Patrón: sondas de disponibilidad por hábito del modelo, presentes aunque el
prompt las prohíba expresamente y cite los casos anteriores.

### 2.2 Escrituras fuera del worktree

| Caso | Qué | Detección | Efecto |
|---|---|---|---|
| F4-T4 intento 2 (`R20261011T202738Z-6356`, llamada 25) | Reescritura de `D:\…\RackCad.sln` del **checkout principal** (proyecto añadido, BOM eliminado) | ninguna en la cesión ni en la verificación; hallado una hora después por la sesión | **real**: cambio no confirmado en la copia de trabajo del Owner durante unas 11 h; restaurado el 2026-10-12 con autorización del Owner ([incidente](incident-main-checkout/README.md)) |
| F4-T5 (`R20261011T215838Z-09a6`, llamada 37) | `placeholder.txt` en el checkout principal | declarado por el Worker (C-05) | transitorio: borrado a los 3 s |
| F4-T5 (llamada 34) | Borrador de `RackRamElementsCommands.cs` en el área temporal de la sesión | declarado a medias («draft in the session scratchpad») | ninguno en el repositorio |
| F4-T1 corrección | Edición por la ruta `D:\…\..\..\..\Users\…\worktree\…` | barrido posterior | destino dentro del worktree; señal de rutas construidas desde el checkout principal |

### 2.3 Resolución inesperada de rutas relativas

- El directorio de proceso de las herramientas de la sesión y de sus subagentes es el **checkout principal**; cada orden de shell vuelve a él.
- En PowerShell, `Set-Location` cambia la ubicación de PowerShell pero **no** `Environment.CurrentDirectory`: `[IO.File]::ReadAllText("RackCad.sln")`
  tras `cd <worktree>` lee y escribe el archivo del checkout principal (reproducido byte a byte en la evidencia del incidente).
- Lo mismo vale para cualquier API .NET con rutas relativas y para órdenes de Bash lanzadas sin `cd`.

### 2.4 Controles de alcance incompletos

- Los hechos del relevo (árbol limpio, `HEAD`, `ls-remote`, procesos) solo miran el **worktree**; el checkout principal y los demás worktrees del
  mismo repositorio quedan fuera.
- `Scope` (AUTOMATION_PLAN 16.9 fila 7) evalúa `git diff BaseSha..CurrentSha` de la rama: por construcción no ve escrituras fuera del worktree
  ni archivos ignorados.
- La lectura de `Scope` por el Controller no es fiable: en F4-T3 nc2 afirmó que un prefijo retirado cubría un archivo y dio VERIFIED
  (DEV-I65-F4-08), el mismo modo que I-63 DEV-G2-01, pese a la instrucción expresa del prompt.
- La aceptación A5 compara `StopConditions` por id: una delegación con el texto de C-07 recortado («o al reves» omitido) pasó (DEV-I65-F4-11).

### 2.5 Controles negativos y verificaciones con falsos resultados

- **Falso negativo:** nc2 de F4-T3 (arriba).
- **Falsas alarmas sistemáticas:** nc1 de F1-T2, F3-T1, F4-T1 y F4-T4 detecta la mutación en `Handoff` antes que en `Identity` (mutar `CurrentSha`
  cambia la entrega frente a `OutputSha256` del registro del trabajo); la disposición es la correcta, pero el oráculo estricto falla siempre
  (DEV-I65-F1-02, DEV-I65-F3-01, DEV-I65-F4-03, DEV-I65-F4-13).
- **Salida incoherente:** en F4-T3 el Controller dio `EXECUTION_BLOCKED/STOP` con las 14 comprobaciones en `pass` (README §8): el protocolo no
  tiene una vía coherente para una parada de contrato resuelta sin cambio del trabajo; hizo falta una disposición del Owner.
- **Corrupción de transcripción:** símbolo de sección convertido en U+0016 y cinco caracteres de un SHA-256 cambiados en una planificación
  (CD-I65-F4-19); tildes en F3 (CD-I65-F3-11).

### 2.6 Verificación del checkout principal

No existe. El incidente de F4-T4 la habría detectado en la misma cesión. La sesión de I-65 la hace a mano desde el incidente
(`git --no-optional-locks status` y barrido de fechas), pero no es parte del relevo.

## 3. Controles propuestos (priorizados; todos mecánicos y verificables)

| # | Control | Qué detecta o impide | Cómo se verifica |
|---|---|---|---|
| C1 | **Fotografía de todos los checkouts del repositorio** (checkout principal y demás worktrees) en los hechos de salida y de entrada de cada cesión: `git --no-optional-locks status --porcelain --untracked-files=all`, hash de cada archivo modificado y barrido de fechas del árbol de trabajo | Cualquier escritura fuera del worktree durante la cesión (F4-T4, F4-T5) | Diferencia salida/entrada = STOP con la lista de rutas; prueba con una escritura canario en un control negativo nuevo |
| C2 | **Directorio de proceso del Worker = su worktree** (o un directorio vacío), fijado por el transporte, no por el prompt | Resolución de rutas relativas contra el checkout principal | El primer hecho de cada orden del Worker registra `Environment.CurrentDirectory`; distinto del worktree = STOP |
| C3 | **Bloqueo del intérprete en el entorno del Worker**: `PATH` sin el intérprete y alias de ejecución de aplicaciones desactivados, o un gancho previo a cada orden que la rechace con una clasificación propia (no `Denials`) | Las sondas `--version` y cualquier uso del intérprete | Canario en cada cesión (la orden bloqueada aparece en el registro con su clasificación); cero invocaciones en las transcripciones |
| C4 | **Barrido mecánico de la transcripción** en el registro del trabajo: invocaciones de intérpretes, `Write`/`Edit` fuera del worktree, APIs de archivos con rutas relativas | Lo que C1-C3 no cubran y la clasificación A/B (§4) con hechos | Señal «sesión → Controller» con la lista y su clase |
| C5 | **`Scope` mecánico** (diff ⊆ `AllowedWriteScope`, intersección vacía con `ForbiddenWriteScope`) como señal de la sesión que el Controller no reinterpreta | Falsos negativos del Controller (DEV-I65-F4-08, I-63 DEV-G2-01) | nc2 debe discriminar siempre; el resultado mecánico de nc2 es `fail` |
| C6 | **Aceptación por igualdad de texto** de `Invariants` y `StopConditions` (no solo por id), rechazo de caracteres de control y campos con formato de esquema entregados ya normalizados | Recortes o corrupciones de transcripción (DEV-I65-F4-11, CD-I65-F4-19) | A5 en `fail` ante cualquier diferencia de bytes |
| C7 | **Clasificación predefinida de desviaciones** (§4) con vía coherente en 16.9: una infracción de clase A no es parada; una de clase B lo es | Salidas incoherentes y paradas resueltas a mano (F4-T3) | El Controller tiene un `FailureClass` y una disposición definidos para cada clase |
| C8 | **Oráculo de nc1 corregido**: aceptar `Handoff` o `Identity` como detección cuando se muta `CurrentSha`, o mutar sin cambiar la huella de la entrega | Falsas alarmas sistemáticas de nc1 | nc1 cumple su oráculo en todas las cadenas |
| C9 | Prohibiciones en el prompt (secundario) | Refuerzo, no control | — |

## 4. Clasificación propuesta de desviaciones

- **Clase A — infracción de procedimiento sin efectos observados:** sin ejecución de código del repositorio o del programa, sin escrituras
  fuera del alcance, árbol y commits intactos, salida no usada como evidencia; demostrada por C1-C4. Registro obligatorio; sin STOP.
- **Clase B — violación con consecuencias reales o no demostrables:** escritura fuera del alcance (con o sin residuo), ejecución de scripts o del
  programa, dependencia del producto o de las pruebas, uso como evidencia, instalación o red, o cualquier caso que C1-C4 no puedan descartar. STOP.

Casos de I-65: las siete invocaciones del intérprete son de clase A (las dos de F1, además, sin registrar); F4-T5 C-05 es de clase B transitoria;
F4-T4 `RackCad.sln` es de clase B con consecuencias.

## 5. Límites

- I-65 no implementa nada de esto y no cambia I-61/I-62. C-08 sigue vigente en I-65; la aceptación del Owner de F4-T3 se limita a esa ejecución.
- La restauración del checkout principal se hizo el 2026-10-12 con autorización del Owner (evidencia del incidente, §6).

## 6. Evidencia

`docs/automation/decisions/I-65.md` §27-§38; `docs/automation/evidence/I-65-evidence.md` §36-§45; custodia en `docs/automation/evidence/I-65-pilot/`;
[incidente del checkout principal](incident-main-checkout/README.md) con sus barridos (`main-checkout-mtime-scan*.json`,
`ram-execution-scan.json`).

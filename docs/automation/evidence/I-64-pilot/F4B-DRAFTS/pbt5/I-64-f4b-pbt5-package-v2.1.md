# I-64 F4B — Paquete de PB-T5: CI exacta, Smoke-PB y gate-close de F4B (v2.1)

> **Redacción para el repo público:** las rutas locales van como marcadores (`<worktree>`, `<carpeta-del-Smoke-PB>`, `<carpeta-de-respaldo>`, `<carpeta-de-salidas>`, `<original-D1>`, `<dotnet-del-usuario>`). No hay nombres personales ni de clientes. La hora del Owner va en UTC; su zona horaria, como `<zona-horaria-del-Owner>`.

**Estado:** v2.1. Aplica los menores de la recomprobación de la v2 (APTO TRAS CORRECCIONES, B 0 / I 1 / M 8; su importante, I-07, es de la corrida de esta noche y se corrige en `pre-run-tonight-v2.1.md`). **No está emitido:** la emisión es del Owner (Q-PBT5-7).

**Base y método:**
- Todo se lee por objetos git en `7da361eb`, la emisión de PB-T4e y la punta de la rama `architecture/workspace-persistente-rackcad`.
- No se leyó el worktree. No se compiló, no se ejecutó ninguna prueba y no se abrió AutoCAD.
- Lo que depende del cierre de PB-T4e lleva **[T4e]**, y lo rellena la sesión principal.

**Documentos que lo acompañan:**
- `owner-questions-v2.1.md`: ocho preguntas en dos vueltas, sin valores por defecto, y dos informaciones;
- `pre-run-tonight-v2.1.md`: la corrida previa a la emisión, bajo la orden directa del Owner, para esta noche, con el paso de copia corregido (I-07) y las reglas de las capturas;
- `handoff-pbt5-v2.1.md` y `handoff-pbt5-v2.md`: lo leído y las discrepancias con la revisión.

**Abreviaturas:**
- **[F]:** el paquete de la fase, `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1.
- **[DISP]:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md`.
- **[T3]:** el paquete de PB-T3, `docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md`.
- **[T4]:** el paquete de PB-T4, `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` v5.6.
- **[G]:** la guía `docs/guias/validacion-manual-autocad.md`. **No se modifica.**
- **[S2]:** el precedente de F4: `I-64-pilot/F4-SMOKE-2/`, `F4-DRAFTS/f4-dispositions.md` (F4-E59 y F4-E71 a F4-E81), `F4-DRAFTS/coordinator/` y `F4-DRAFTS/gate-close.md`.
- **[BM]:** el bloque M de PB-T0: `I-64-pilot/F4B-PBT0-CHARACTERIZATION/block-m/` y `coordinator/pbt0-block-m-procedure.md`.
- **[IR1]:** la revisión independiente de la v1, `pbt5/review-v1/independent-review-pbt5-v1.md` (sha256 `656980ca…696c`).

---

## Registro de cambios v2 → v2.1

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

| Hallazgo de [IR1] | Qué cambia en la v2 |
|---|---|
| **B-01** | [T3] entra en las autoridades. Nuevo bloque I, «La lista de [T3] §5.5» (SPB-53 a SPB-61), y la purga de las anidadas en SPB-18 (f). Se corrigen las dos afirmaciones falsas de la v1, en §4.4 y §5.7. PBT5-C04 e INV-PBT5-05 dicen ahora «en el usuario, la máquina o el shell» |
| **B-02** | «Listo para empezar» ya no incluye la emisión. La emisión es la pregunta Q-PBT5-7, sin valor por defecto. Esta noche, solo la corrida previa a la emisión (`pre-run-tonight-v2.md`) |
| **I-01** | La forma del contrato (§6.1) y la definición de intento (§6.6) pasan a ser propuestas ligadas a Q-PBT5-8 y Q-PBT5-9. §6.1 nombra la descripción con que el Owner eligió la v3 de nc2 «para PB-T5» |
| **I-02** | Q-PBT5-1 va sin valor por defecto y nombra la regla de F4-E59. P-02: esta noche solo Core Full, los filtros de UI y la CI exacta. El Candidato queda «pendiente de Q-PBT5-1» |
| **I-03** | Q-PBT5-5 es ahora una confirmación de lectura. Esta noche, a las 13:00Z, la corrida para y espera (orden de la sesión principal) |
| **I-04** | SPB-26 y SPB-28 comparan los campos del sobre que no son del editor (D-PB-05, pasos 5 a 7). SPB-48 queda para el rechazo de AUTH-13 |
| **I-05** | SPB-09 es obligatorio, con la geometría, y pasa a S+O. La antigua Q-PBT5-3 pasa a información |
| **I-06** | SPB-09, SPB-16 y SPB-20 pasan a S+O. SPB-19 y SPB-40 guardan sus capturas de antes y después para el Owner |
| M-01 | Los literales salen de `src/`: «Restaurar estandar», «Insertar vista actual» e «Insertar lateral». S-10 está corregido |
| M-02 | SPB-01: no se marca «Always ignore», y la casilla de «Proxy Information» no se toca |
| M-03 | P-05 comprueba que la carpeta de la biblioteca de diseños existe. Si no, la segunda pasada de SPB-17 para |
| M-04 | P-01 exige `event = push` y `ref = refs/heads/<rama>` |
| M-05 | `gate-close.md` lleva la tabla de los INV de [F] §1 |
| M-06 | Son 18 tareas: 17 más PB-T5 |
| M-07 | PBT5-C01 se separa en C01 (entorno) y C15 (una prueba que falla). Se propone una regla para reejecutar la CI |
| M-08 | `check_pbt5.py` lee sus patrones de un archivo que no se custodia, se analiza a sí mismo y sale con 1 si hay coincidencias |
| M-09 | El reanclaje de POST y los eventos del puente se declaran inferidos, y se nombra el efecto que se mide |
| M-10 | Nuevo bloque J (SPB-62 a SPB-65), con los subcasos de OWNER-03 de PB-T4a y M-23 |
| M-11 | El coste de `Equals` va a SPB-23, y el de «Conservar», a SPB-26 |
| M-12 | §3.3, condición 3, y Q-PBT5-2 |
| M-13 | Q-PBT5-4 suma `LOCKED_LAYER` y el último vínculo, y fija la línea entre preparar y construir. INV-PBT5-06 rotula también los contextos construidos |
| M-14 | §5.5: las coordenadas de cada clic y una captura de después en cada resultado negativo |
| M-15 | A2-11 se traza a SPB-06 (b), y la mitad del estilo, a SPB-07 (b) |
| M-16 | SPB-50: el camino de «Descartar borradores de este documento» |
| M-17 | P-05 y SPB-52 se limitan a `Applications`, `Loaded` y `AssemblyMap`. Las `Dialogs\RackCad.*` son una escritura esperada del host |

---

## 0. En una página

- **Qué es PB-T5:** la última tarea de F4B. Tabla del §5 de [F]: «CI exacta, Smoke-PB (D-PB-09) y gate-close de F4B», que depende de PB-T4. Empieza cuando PB-T4e cierre.
- **No tiene Worker de código.** La forma de su contrato la decide el Owner (Q-PBT5-8), y su emisión, también (Q-PBT5-7).
- **La CI exacta:** el Candidato es el SHA exacto desde el que se compila el DLL. Lleva la CI de push 4/4 sobre ese SHA, con los totales del GREEN de PB-T4e, más la evidencia local que fije el Owner (Q-PBT5-1).
- **El Smoke-PB:** 65 pasos en 12 bloques, más 8 precondiciones.
  - 56 los mide la sesión;
  - 9 necesitan, además, un juicio visual del Owner (S+O);
  - los A/B y los contextos construidos van rotulados **CONSTRUCTED REPRESENTATIVE TEST CASES** (OB-09.1), y lo demás se hace sobre racks reales, en copias (OB-09.2).
- **Esta noche:** con la orden directa del Owner, una corrida previa a la emisión, de 54 pasos obligatorios (45 S y 9 S+O), más 4 opcionales (SPB-62 a SPB-65, todos S) si queda tiempo antes de las 13:00 UTC. En total, hasta 58 pasos: 49 S y 9 S+O. Está descrita en `pre-run-tonight-v2.1.md`. No es el Smoke-PB contratado.
- **El gate-close:** lo autoriza la firma del Owner, como en F4-E81. No cambian `main`, HANDOFF, ROADMAP ni [G].

---

## 1. Autoridades

| Autoridad | Qué fija para PB-T5 |
|---|---|
| [F] §5, fila PB-T5 | El alcance |
| [F] §5, «Cada tarea pasa por la cadena de F4…», y §6.5 | La cadena de cada tarea y la única adaptación prevista, la de PB-T0 |
| [F] D-PB-09 (OB-09.1 y OB-09.2) | Los A/B fabricados y rotulados; racks reales donde existen; copias; originales sin abrir |
| [F] D-PB-10 (OB-10.1 y OB-10.2) | 1/3 por tarea; el intento 2 lo decide la sesión; el 3 vuelve al Owner |
| [F] §6.3 | Las reglas de operación por control de pantalla, NETLOAD con «Load Once», los inventarios de solo lectura y «No se finge una condición interna» |
| [F] §7.2 | Las reglas de F4 no se heredan sin orden del Owner |
| [F] §8 | R-PB-03, R-PB-04, R-PB-06 y R-PB-11 remiten al Smoke-PB |
| [F] §9 | Los criterios de cierre de F4B |
| **[T3] §5.5** («Para el Smoke-PB de PB-T5»), §5.6 y A-T3-5 (`:1865`) | Lo físico del escritor completo: el borrado con la purga, un `U` por «Actualizar» (también con renombrado y con A/B), la colisión de nombres, `LOCKED_LAYER`, los rechazos del frontal B y del último vínculo, el lateral de origen obsoleto, las inyecciones, el tiempo, los A/B y la barrera |
| [T4] §5.5 y §10 | La lista del Smoke-PB de PB-T4 y lo que queda para PB-T5 |
| [DISP] F4B-E06, E08, E09, E12, E23, E48, E51, E53, E55, E58, E59 y E60 | Lo que cada tarea dejó al Smoke-PB (§4.5) |
| Los paquetes de cierre de PB-TC (`:65`), PB-T3c (`:53` y `:56`) y PB-T4b (`:59`); la revisión de PB-T4a (`:207-215`); el paquete de PB-T2 (`:1329`) | Lo que se dejó al Smoke-PB fuera de [DISP]: la geometría de D-PB-11, la purga, los costes y los subcasos de OWNER-03 |
| [S2] F4-E59 | La regla del Owner: «en tareas con contención de perfil, nunca suites completas de UI en local; solo en la CI» |
| [S2] F4-E71 a F4-E81 | El precedente de un smoke de fase: cómo se contrató, se operó, se firmó y se cerró |
| [BM] | Cómo operó la sesión en F4B: NETLOAD por expresión con «Load Once», funciones LISP tecleadas, N-I1 y una segunda instancia si el Owner tiene AutoCAD abierto |
| [G] §1 a §3, §6, §7, §7.1 y §7.2; WORKFLOW §4 y §6; AGENTS, «Pruebas — definición de terminado» y «Reutilización de evidencia» | El entorno, el DLL del worktree, la carga, los criterios, el Candidato y su evidencia, y la reutilización solo por SHA exacto |
| Las respuestas del Owner a `owner-questions-v2.md` | La emisión, la forma del contrato, el intento, la evidencia local, las inyecciones, los contextos construidos y el estilo de cotas |
| La orden directa del Owner de esta noche | Solo la corrida previa a la emisión (`pre-run-tonight-v2.1.md`) |

---

## 2. Hechos de partida (medidos en `7da361eb`)

- **Las tareas:**
  - 16 tareas de F4B están cerradas, CLOSED / PASS WITH LIMITATIONS (`I-64.yml`, `f4b_status`);
  - PB-T4e está emitida (F4B-E60) y su Worker está en curso;
  - PB-T5 sigue en WAITING ([F] §0; [DISP] F4B-E01).
- **Los totales esperados en la CI del GREEN de PB-T4e:** Core 19 421 = 19 018 + 0 + 403; UI 2 465 = 2 417 + 0 + 48 (F4B-E60). **[T4e]**: si el cierre registra otros, mandan los suyos.
- **El estado en `I-64.yml`:** `current_phase: F4B`, `state: implementing`, `gate: none`, `attempts: 1`.
- **Los dibujos reales de F4B:**
  - **D1** (R006): racks Push Back de un sentido, con 14 sobres sin `DimensionStyle`. Tiene una referencia anidada de una planta dentro de un bloque huérfano ([BM] §3). Según la selección de Smoke-2, un rack tiene una planta y dos laterales enlazadas, una de ellas con el sufijo `_1`;
  - **D2** (R053): sus definiciones no tienen referencias de nivel superior, y no sirve para el panel;
  - **ningún A/B real tiene referencias** (R-PB-04).
- **El estilo de cotas:** 0 de 22 sobres reales lo guardan, y ningún camino de la interfaz lo escribe (F4B-E04, M-7). Solo se contaron D1 y D2.
- **La contención de perfil sigue abierta:** las pruebas de UI existentes reescriben `RackFrameConfigurator.layout` (F4B-E30; `src/RackCad.UI/RackFrames/RackFrameConfiguratorLayoutStore.cs:29`).
- **El precedente de carga:**
  - NETLOAD, también por expresión, con «Load Once» y nunca «Always Load»;
  - los avisos de arranque, sin «Always ignore» ([S2], run-log-2: el relanzamiento de las 04:56Z);
  - las inyecciones de Smoke-2, con la variable solo en el entorno del proceso de AutoCAD ([S2], los pasos 78 a 84).
- **El perfil de AutoCAD** tiene ya 20 entradas `Dialogs\RackCad.*` ([IR1] §4, medido a las 05:48Z), que AutoCAD escribe al mostrar ventanas del producto.
- **«Guardar en biblioteca»** (`src/RackCad.UI/UiSupport.cs`, `PromptSaveToLibrary`) usa un `SaveFileDialog` con la carpeta de la biblioteca de diseños como inicial. Antes de abrirlo, crea esa carpeta si no existe.
- **Los literales que cita el §4** están en el `src/` de `7da361eb`, comprobados por `git grep` ([IR1], `check_literals-out.txt`, más los de [T3] que añade la v2). PB-T4e no añade ningún texto ([T4] D-PBT4-19).

---

## 3. El alcance de PB-T5

### 3.1 La CI exacta del Candidato

**Base:** [F] §9; [T4] §10; [G] §2, §7.1 y §7.2; AGENTS, «Evidencia CI de rama y Candidato» y «definición de terminado».

- **El Candidato** es el SHA exacto de la punta desde la que se compila el DLL.
  - **Esta noche,** es la punta del cierre de PB-T4e **[T4e]**.
  - **Si el Owner emite PB-T5,** es esa punta o un commit documental encima, según la respuesta a Q-PBT5-9 y la regla de reutilización.
- **El producto** es el último commit que toca `src/` o `tests/` **[T4e]**. `git diff <producto> <Candidato> -- src tests assets` tiene que salir vacío.
- **Por qué el Candidato es la punta:** [G] §7.2 exige que la `InformationalVersion` del DLL identifique el Candidato. Se registran el Candidato y el producto, como en `dll-c2.md`.
- **La CI exacta:** una corrida con `event = push`, `ref = refs/heads/architecture/workspace-persistente-rackcad` y `head_sha` = Candidato, con los cuatro trabajos en `success`:
  - `Tests (Domain + Application)`;
  - `UI Tests (WPF controls, net8.0-windows)`;
  - `Build UI (WPF, valida API de Application)`;
  - `Build Plugin without AutoCAD`.

  Y los totales de §2. Si el Candidato es un commit documental, se registra además la CI del producto.
- **La evidencia local:**
  - **siempre:** Core Full local sobre el Candidato, tras el commit y con el árbol limpio; los filtros de UI de los `RequiredTests` del contrato de PB-T4e, cada uno en su proceso; y el build Debug de UI y del Plugin;
  - **la UI Full local:** [G] y AGENTS la piden para un Candidato, y F4-E59 la prohíbe mientras siga la contención. **Lo decide el Owner (Q-PBT5-1).** Hasta entonces, el Candidato queda «pendiente de Q-PBT5-1», y PB-T5 no cierra. La UI Full puede correr después sobre el mismo SHA ([G] §7.1, «La evidencia local se ejecuta DESPUÉS de crear el commit»).

### 3.2 El Smoke-PB

**Base:** [F] D-PB-09, §8 y §9; [T3] §5.5; [T4] §5.5; [DISP].

- **Lo que cubre:** lo que solo se ve en AutoCAD:
  - lo que PB-T3, PB-T4 y sus subtareas dejaron «sin AutoCAD»;
  - las correcciones declaradas de RACKEDITAR;
  - los casos A/B fabricados.
- **Lo que no es:** ni Owner Validation, ni la validación total de [G] §4. No repite la evidencia automatizada de las tareas.

### 3.3 El gate-close de F4B

**Condiciones** ([F] §9):
1. **Todas las tareas cerradas:** 18, las 16 de §2, PB-T4e **[T4e]** y PB-T5, que cierra con el gate.
2. **T1B en verde,** con sustitución solo registrada: todas las secciones de `superseding-characterization.md` en EJECUTADA, también «## PB-T4e» **[T4e]**.
3. **El Smoke-PB verifica en RACKEDITAR D-PB-06 y D-PB-08** (SPB-06 a SPB-08).
   - **Atención (M-12):** si la mitad del estilo de D-PB-06 (b) queda NOT AVAILABLE (SPB-07), el criterio se cumple solo en parte. Lo tiene que aceptar el Owner (Q-PBT5-2); no basta con registrarlo como limitación.
4. **IC-06.**
5. **Los INV del §1 de [F]** (INV-09, 10, 15, 18, 19, 25, 28 y 29), cubiertos para Push Back completo por la evidencia automatizada de las tareas. `gate-close.md` los enumera en una tabla INV → tarea y prueba, que se rellena desde los paquetes de cierre, como hizo F4 con los suyos.
6. **La CI exacta del Candidato,** y la evidencia local que fije Q-PBT5-1.
7. **Smoke-PB PASS,** o PASS WITH LIMITATIONS con las limitaciones aceptadas por el Owner, con los casos fabricados rotulados.
8. **La firma del Owner.**
9. **Sin integración parcial en `main`.**

**Lo que cambia** (siguiendo el commit de cierre de F4, `5ccaa770`):

| Archivo | Cambio |
|---|---|
| `docs/automation/state/I-64.yml` | `next_action`; `f4b_status` («CLOSED / PASS [WITH LIMITATIONS] (F4B-E(n); F4B-DRAFTS/gate-close.md): 18 tareas cerradas; Candidato F4B `<sha>` (CI `<run>` 4/4); Smoke-PB …; limitaciones …; no es Owner Validation; no integra en main»); en `persistent_next_action`, `NEXT_ROLE`, `NEXT_ACTION`, `GATE` y `TASK`, y `REMAINING_BUDGET` mientras dure PB-T5. **No cambian**, como en F4: `current_phase`, `state`, `gate`, `attempts` ni `last_evidence_commit` |
| [DISP] | Entradas nuevas F4B-E(n) **[T4e]**: (1) la corrida previa a la emisión, si la hay, y su custodia; (2) las respuestas del Owner y la emisión; (3) la preparación y la CI exacta; (4) la corrida del Smoke-PB y su custodia; (5) la disposición y la firma del Owner; (6) el cierre del gate |
| `docs/automation/evidence/I-64-evidence.md` | Una sección por entrada de [DISP], con la forma de §230 |
| `I-64-pilot/F4B-DRAFTS/gate-close.md` (nuevo) | Con la forma de `F4-DRAFTS/gate-close.md`: el resultado, la autoridad, la secuencia, la tabla de las 18 tareas, la tabla de los INV (condición 5), el Smoke-PB por bloque, las limitaciones y lo siguiente |
| `I-64-pilot/F4B-SMOKE-PB/` (nombre provisional) | `prep/`, `prerun/` (la corrida previa) y `run/` |
| `I-64-pilot/F4B-DRAFTS/coordinator/` | Los registros literales del Owner |
| `I-64-pilot/F4B-DRAFTS/pbt5/` | El paquete y sus versiones, las revisiones, los traspasos y la guía redactada. `_tools/check_pbt5.py`, solo por su sha256 si casa algo prohibido, como [DISP] F4B-E60 |
| `docs/ideas-futuras.md` | Solo lo que mande el Owner |

**No cambian:**
- `src/`, `tests/` y `assets/`;
- [G];
- HANDOFF y ROADMAP, que se tocan al integrar la iniciativa;
- `superseding-characterization.md`;
- `main`.

**La redacción del cierre:**
- «F4B CLOSED / PASS WITH LIMITATIONS» o «CLOSED / PASS», según la firma;
- «No es Owner Validation y no integra en `main`»;
- desde ese momento se puede decir «editor de Push Back migrado» (IC-06 e IC-11), con sus limitaciones;
- lo siguiente: F5-A deja de esperar, con su paquete y su contrato.

---

## 4. La lista del Smoke-PB

### 4.0 Convenciones

**Resultados.** Son los de Smoke-2. Adoptarlos en F4B lo decide el Owner (Q-PBT5-9); hasta entonces, son la forma de registrar.

| Resultado | Cuándo |
|---|---|
| **PASS** | Se cumple el criterio |
| **FAIL** | No se cumple. Se registra y no se corrige en PB-T5 (PBT5-C05) |
| **LIMITATION**, en una de tres formas | **NOT AVAILABLE FOR PHYSICAL SMOKE:** el caso no existe en los dibujos reales y no está autorizado fabricarlo.<br>**NO VERIFICABLE FÍSICAMENTE / CUBIERTO POR PRUEBA AUTOMATIZADA:** exige una condición interna que AutoCAD no muestra.<br>**OBSERVADO:** no coincide con lo previsto y no está caracterizado; se trae la evidencia antes de decidir |
| **INFORMATIVO** | Una medida sin umbral (R-PB-06). Solo es FAIL si AutoCAD deja de responder |

**Quién observa:**
- **S:** lo mide la sesión.
- **S+O:** lo mide la sesión, y hace falta además un juicio visual del Owner. Si opera la sesión, queda **PENDIENTE DE REVISIÓN DEL OWNER** con sus capturas.

**Términos:**
- **Rack real:** un rack Push Back de un sentido de D1, en su copia.
- **Copia:** una copia nueva del original para cada escenario contaminable.
- **Gemela:** otra copia, para hacer la misma edición con RACKEDITAR y comparar (la igualdad con el clásico, [F] §6.4, fila 1).
- **Preparar:** un gesto del producto, como insertar una vista con RACKEDITAR («Insertar lateral» o «Insertar vista actual»).
- **Construir:** poner el dibujo, con órdenes nativas de AutoCAD, en un estado que el producto no crea. Necesita la respuesta A de Q-PBT5-4 (M-13).
- **Inventario** (`RCDUMP <etiqueta>`): las funciones de solo lectura de [BM] (`lisp_src.txt`), tecleadas en cada documento, más tres de solo lectura nuevas (§5.3):
  - `rcblk`: todas las definiciones de bloque, para ver la purga. **Su control** (M-21): en c01, más de 0 líneas `BLK`, y entre ellas todas las definiciones de rack de las líneas `DEF`;
  - `rcnest`: las referencias de vistas de rack dentro de otras definiciones, con el indicador del contenedor y si es dinámico;
  - `rcdyn`: si una definición es dinámica (`ACAD_ENHANCEDBLOCK` en el diccionario de extensión de su registro).

  **El control de `rcnest`:** la planta anidada de D1, dentro de un bloque pegado «A$C…» ([BM] §3). Ese contenedor no es dinámico ni anónimo, y no cuenta como caso de SPB-47.

  El sobre JSON completo de cada vista ya va en `RCDUMP`. El análisis lee de él `SchemaVersion`, el GUID, los metadatos de sesión y la `ExtensionData` (I-04).
- **N-I1:** entre la orden medida y sus `U` no se teclea nada.
- **Los literales** entre comillas latinas están en `src/` de `7da361eb`. La guía los copia del `src/` del Candidato.

### 4.1 Precondiciones (la sesión, con AutoCAD cerrado)

| Id | Origen | Qué se hace | Criterio (si no se cumple, STOP) |
|---|---|---|---|
| P-01 | [F] §9; §3.1; M-04 | `git -C <worktree> status` y `rev-parse HEAD`; el diff `<producto>..<Candidato> -- src tests assets`; la CI exacta | Árbol limpio; HEAD = Candidato; diff vacío; la corrida con `event = push`, `ref = refs/heads/<rama>`, `head_sha` = Candidato, los cuatro trabajos en `success` y los totales de §2. Si falla una prueba: PBT5-C15 |
| P-02 | [G] §2; AGENTS; [S2] F4-E59; Q-PBT5-1 | Core Full local; los filtros de UI de los `RequiredTests` de PB-T4e, cada uno en su proceso; el SHA-256 y la fecha de `RackFrameConfigurator.layout`, antes y después. La UI Full local, solo con la respuesta de Q-PBT5-1 | Core y los filtros con los totales esperados. El cambio del archivo de disposición, si lo hay, se registra |
| P-03 | [G] §1, §2 y §7.2; WORKFLOW §6 | Sin `acad.exe` ni `accoreconsole.exe`, los builds Debug de UI y del Plugin de [G] §2. El DLL es `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` | 0 errores y ningún `warning CS`; la `InformationalVersion` identifica el Candidato; los SHA-256 de los cuatro DLL y la hora. Si un AutoCAD del Owner está abierto: PBT5-C08 |
| P-04 | [BM] P-2 | `catalogs` junto al DLL, frente a `assets/catalogs` del Candidato | Iguales byte a byte |
| P-05 | [S2], «Comprobaciones del día»; [G] §7.1; M-03; M-17 | `RACKCAD_DEBUG_*` en el usuario, la máquina y el shell de la sesión; bundles de RackCad; `Applications`, `Loaded` y `AssemblyMap` del perfil de AutoCAD (lectura); el número de `Dialogs\RackCad.*`, solo como registro; la biblioteca de bloques (ruta y SHA-256); **si existe la carpeta de la biblioteca de diseños**; el registro de RackCad del día; keep-awake | Ninguna variable; ningún bundle; ninguna entrada de RackCad en `Applications`, `Loaded` ni `AssemblyMap`. Las `Dialogs` no son criterio. Si la carpeta de diseños no existe, se anota para SPB-17 |
| P-06 | [F] OB-09.2 y §6.3; M-18 | Las carpetas, fuera del worktree y de todo repositorio. El SHA-256 del original, frente al registrado (para D1, el de `F4-SMOKE-2/run/integrity.md`), **antes** del respaldo. Después, el respaldo en `<carpeta-de-respaldo>`, las copias con `copy` desde el original y el manifiesto `copias.json`. **Ninguna escritura sobrescribe:** se para si el destino ya existe | Respaldo = original; copia = original; el original, igual antes y después. **Los originales nunca se abren** |
| P-07 | [S2] F4-E71, precondición 7 | Ningún Worker ni otra sesión en el worktree | Sin delegaciones abiertas |
| P-08 | OB-09.1; M-01 | La guía del operador, con los literales copiados del `src/` del Candidato, en local, y su versión redactada | Los rótulos de los A/B y de los contextos construidos, en la guía |

**«Listo para empezar»** = P-01 a P-08 cumplidas. **La emisión no forma parte de esto** (B-02).

### 4.2 Los pasos

#### Bloque 0. Apertura y carga

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-01 | [G] §3, paso 1; [BM] P-6; [S2], el relanzamiento; M-02 | Abrir AutoCAD 2025 con una copia, en una instancia nueva, sin `RACKCAD_DEBUG_*` en su entorno. Un AutoCAD del Owner no se toca. «Proxy Information» → «OK», sin cambiar su opción. Referencias no encontradas → «Ignore unresolved reference files», **sin marcar «Always ignore»** | El título; el PID; el `.dwl` junto a la copia | PASS si se abre la copia sin tocar otra instancia. FAIL si se abre un original | S |
| SPB-02 | [G] §3, pasos 2 y 3; [F] §6.3 | `NETLOAD` del DLL de P-03, como en [S2] y [BM]: por expresión, o con el diálogo si `FILEDIA` = 1 (la variable no se cambia). En el aviso de seguridad, «Load Once» | Los módulos del proceso, desde la carpeta de P-03 y con sus SHA-256 | PASS si coinciden. FAIL si se carga otro DLL. PBT5-C03 si hiciera falta otra cosa | S |
| SPB-03 | [G] §3, pasos 4 y 5 | `RACKCAD` | El menú, sin excepciones, con la biblioteca de P-05 | PASS / FAIL | S |
| SPB-04 | [S2], pasos 0.6, 0.7 y 75; M-20 | `RACKPANEL`; las funciones de inventario y sus controles; `RCDUMP pre`; `PICKFIRST`, leído y sin cambiarlo. **La línea de base de `DBMOD` y de Deshacer se toma después de seleccionar el rack y de la vista previa del panel:** en [S2] 75, esa selección puso `DBMOD` = 1 | «RackCad: panel Workspace mostrado.», sin errores; la línea de base, con su `DBMOD` explicado como en [S2] 0.7 y 75 | PASS si la línea de base queda registrada | S |

#### Bloque A. RACKEDITAR (modo CLASSIC) y sus correcciones declaradas (racks reales)

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-05 | OB-01.1b, OB-01.6 y OB-RE.1; M-01 | RACKEDITAR sobre un rack real | En CLASSIC: «Actualizar», «Insertar vista actual», «Insertar lateral», los botones por vista e «Insertar varias…», con los literales del xaml; ningún «Aplicar al borrador» | PASS / FAIL | S |
| SPB-06 | D-PB-06 (a); OD-PBTC-1; F4B-E08; (b): A2-11 (el paso 4 de D-PB-06 hasta el DWG, [T3] §5.6) y F4B-E06; M-15 | (a) Copia nueva. RACKEDITAR → el fondo de la tarima con un valor de prueba de 4 decimales (por ejemplo, 48.0004) → «Actualizar» → `RCDUMP a1` → RACKEDITAR **sin tocar nada** → «Actualizar» → `RCDUMP a2`.<br>(b) En la misma copia: «Editar» → «Editar todo» → cambiar la escala → «Aplicar al borrador» → «Actualizar» del panel → `RCDUMP a3` | El campo en `a1`, `a2` y `a3` | PASS si `a2` y `a3` conservan el exacto. FAIL si sale redondeado. El valor es de prueba sobre un rack real, y se declara | S |
| SPB-07 | D-PB-06 (b); OD-PBTC-2; F4B-E04, M-7; (b): A2-11, la mitad del estilo (M-15); Q-PBT5-2 | (a) Recuento, con el inventario, de los sobres con `DimensionStyle` no nulo en las copias. Si Q-PBT5-2 = C, también en los demás dibujos reales de la selección de Smoke-2, en copias y en solo lectura. Si hay alguno: RACKEDITAR → «Actualizar» → el estilo de sus cotas.<br>(b) Si hay alguno: lo mismo por «Editar todo» → «Actualizar» | El recuento; el estilo | Si hay alguno: PASS si las cotas salen con el estilo guardado. Si no hay ninguno: NOT AVAILABLE FOR PHYSICAL SMOKE, en (a) y en (b). **El criterio 3 de §3.3 queda en parte, y lo acepta o no el Owner (Q-PBT5-2).** No se fabrica sin su respuesta B | S |
| SPB-08 | D-PB-08 (OB-08.1 a OB-08.3); OD-PBTC-3; el cierre de PB-TC (`:65`, «el aspecto del aviso») | RACKEDITAR sobre un rack real de un sentido → marcar «Rack de dos sentidos (lado B)» sin activar ningún frente de B → «Actualizar» → `RCDUMP` → RACKEDITAR otra vez | (a) En la franja de estado, el literal: «Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.»; (b) no bloquea; (c) el lado B no se guarda; (d) al reabrir, la casilla puede salir desmarcada | PASS si (a) es igual byte a byte y se cumplen (b) y (c). FAIL si no. **O:** si el aviso se entiende | S+O |
| SPB-09 | **D-PB-11** (H-PB-28); OD-PBTC-4; F4B-E08; **el cierre de PB-TC (`:65`), «la geometría del reinicio de D-PB-11»**; I-05; I-06 | Copia nueva. RACKEDITAR → una altura de cabecera distinta de la estándar → «Actualizar» → `RCDUMP r1` y captura de cada vista → RACKEDITAR → «Restaurar estandar» → «Actualizar» → `RCDUMP r2` y captura de cada vista | Los valores de `r2` (altura de cabecera, poste derivado y separadores); el número de entidades de cada definición en `r1` y `r2`; las vistas, antes y después | PASS si `r2` tiene los valores estándar y las vistas se redibujan con ellos. FAIL si vuelven los de `r1`. **O:** si la geometría restaurada se ve correcta | S+O |
| SPB-10 | H-31 (OD-PBTC-5, PB-TC2); F4B-E09 y F4B-E12. Cobertura añadida por la sesión (información de `owner-questions-v2.md`) | Copia nueva. RACKEDITAR → la longitud de un módulo con 4 decimales → «Actualizar» → RACKEDITAR → pasar por la caja sin escribir → «Actualizar» → `RCDUMP` | El valor | PASS si es exacto. FAIL si sale redondeado | S |

#### Bloque B. «Editar todo»: la ventana, el modal y cero escrituras (rack real)

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-11 | [T4] §5.5 («el botón abre la ventana modal real»; «el abridor cableado»); OB-01.6; F4B-E23 | Seleccionar el rack → «Editar» → «Listo para editar» → «Editar todo» | La ventana en WORKSPACE, desde la paleta: «Aplicar al borrador», con su tooltip; sin las inserciones; con «BOM» y «Guardar en biblioteca». Que no tenga dueño es NO OBSERVABLE, y lo cubre PB-T4c | PASS / FAIL | S |
| SPB-12 | [T4] §5.5 (K = 1); F4B-E55; M-14 | Con la ventana abierta, un clic en la paleta, con sus coordenadas registradas y capturas de antes y después | La paleta no responde; al cerrar la ventana, sí | PASS / FAIL | S |
| SPB-13 | OB-01.4; M-14 | Con la ventana abierta, teclear en AutoCAD y hacer clic en el dibujo, con las coordenadas y capturas de antes y después | Nada llega | PASS / FAIL | S |
| SPB-14 | F4B-E55 («el foco y el teclado») | Escribir en una caja de la ventana; Tab, Retroceso y Supr | Nada en la línea de órdenes; sin PASTECLIP | PASS / FAIL | S |
| SPB-15 | [T4] §5.5; paso 7 de PB-T4e; F4B-E55 («los eventos del puente durante el modal»); M-09; M-20 | Selección y vista previa → la línea de base de `DBMOD` y de Deshacer → `RCDUMP` → «Editar todo» → «Cerrar» sin cambios → `RCDUMP`; `DBMOD`; la lista de Deshacer; el estado a los 5 s | «Editor cerrado sin cambios en el borrador.»; «Limpio»; los inventarios iguales; sin entrada nueva de Deshacer; a los 5 s, sin «(posiblemente obsoleto)». **Los eventos del puente son internos:** se mide su efecto, que ninguna lectura propia marque el borrador | PASS / FAIL | S |
| SPB-16 | D-PB-08 en WORKSPACE (F4B-E06; OB-08.2, «va en los dos modos»); I-06 | En la ventana de «Editar todo», marcar el lado B sin frentes, con captura | El literal de SPB-08, sin bloquear | PASS / FAIL. **O:** el juicio de SPB-08, sobre esta captura | S+O |
| SPB-17 | D-PB-07 (F4B-E23); OWNER-03 de PB-T4c; M-03 | Copia nueva. Un cambio sin aplicar → «BOM» → exportar a `<carpeta-de-salidas>` → «Guardar en biblioteca» en `<carpeta-de-salidas>`, con un nombre nuevo → «Cerrar» → `RCDUMP`. Segunda pasada: «Guardar en biblioteca» → «Aplicar al borrador» → «Actualizar» → `RCDUMP` | Los archivos, solo en `<carpeta-de-salidas>`; el DWG sin cambios en la primera pasada; el GUID igual; si el `SetName` llega al borrador | PASS si nada sale de `<carpeta-de-salidas>`, el DWG no cambia sin «Actualizar» y el GUID no cambia. **Si P-05 vio que la carpeta de diseños no existe, «Guardar en biblioteca» la crearía:** STOP de esa parte, y LIMITATION | S |

#### Bloque C. «Editar todo» → «Actualizar» y los caminos de `BuildCore` (racks reales)

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-18 | [T4] §5.5 («el borrado físico»); la receta de CT-PBT4C-10 y CT-PBT4E-04; F4B-E48; F4B-E53; F4B-E55; F4B-E59, OWNER-02, camino 1; R-PB-03; [T3] §5.5 (1), la purga; el cierre de PB-T3c (`:53`) | Copia y gemela. **Preparación** en las dos: RACKEDITAR → «Insertar lateral» del último poste ([BM] M-6a) → `RCDUMP ins`. **Panel:** «Editar» → «Editar todo» → un frente menos y la escala a 2.5 → «Aplicar al borrador» → «Cerrar» → «Actualizar» → `RCDUMP post`. **Gemela:** lo mismo con RACKEDITAR | (a) Tras «Cerrar», el resumen y «Modificado». (b) «Actualizado: el cambio se aplicó al dibujo.» y «Limpio». (c) El lateral obsoleto, borrado (definición y referencia); lo demás, redibujado con un frente menos y la escala 2.5; ninguna vista nueva. (d) Igual que la gemela, salvo lo declarado. (e) El nombre, los niveles y las cotas en las vistas. (f) **La purga:** las líneas `BLK` de `ins` y `post`, es decir, qué definiciones anidadas del lateral borrado desaparecen (un nivel, OWNER-2 de PB-T3c) | PASS si se cumplen (a) a (d) y (f) coincide con la gemela o queda explicado. FAIL si no se borra, si se crea una vista o si difiere del clásico sin declaración. **O:** (e) | S+O |
| SPB-19 | [BM] M-5 y N-I1; [S2] S2-03; [T3] §5.5 (2), con borrado; I-06 | Copia nueva. Lo de SPB-18 hasta «Actualizar» → `U` sin nada en medio → `RCDUMP u1` → `REDO`. Capturas de antes y después, guardadas para el Owner | Un `U` deshace todo, también el borrado; la vista se refresca sola; `REDO` lo rehace | PASS si `u1` = `ins` y `REDO` rehace. FAIL si no | S |
| SPB-20 | F4B-E59, OWNER-02, camino 2; I-06 | Copia nueva. «Editar todo» → vaciar el nombre → «Aplicar al borrador» → «Actualizar» → `RCDUMP` y **captura de cada vista** | El rótulo del nombre en cada vista y `Name` en cada sobre | PASS si todas llevan el nombre del sobre de origen. **O:** el rótulo en las capturas, porque `RCDUMP` no lee los textos de la geometría | S+O |
| SPB-21 | A-T2b-OWNER-01; D-PBT4-17; OWNER-04 de PB-T4c | Escenificar un módulo en la ventana y dejar otro cambio sin aplicar → «Cerrar» | Una sola pregunta, con el aviso real | PASS / FAIL. Si el gesto no se alcanza: NOT AVAILABLE | S |
| SPB-22 | M-04 de la revisión de PB-T4c | Ninguno: «No se abrió el editor: <mensaje>.» solo sale con una excepción del editor después de aplicar | — | NO VERIFICABLE FÍSICAMENTE | S |
| SPB-23 | R-PB-06; [T3] §5.5 («el tiempo de un "Actualizar" grande»); [T4] §5.5 («el coste»); el paquete de PB-T2 (`:1329`, el coste de `Equals`); M-11 | En el rack real más grande: la duración del «Actualizar» del panel y la de RACKEDITAR en la gemela; y la del clic en «Aplicar al borrador» y en «Cerrar» | Los segundos, con su resolución | INFORMATIVO. **O:** si la espera es aceptable | S+O |

#### Bloque D. El conflicto y «Conservar mi borrador» (racks reales)

**Origen común:** D-PB-05, pasos 1 a 7 (OB-05.3 a OB-05.5); D-PBT4-08 y D-PBT4-09; [T4] §5.5; F4B-E53; OWNER-2 de PB-T3b (I-04).

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-24 | El común; R-T4-39 | Copia nueva, con el lateral del último poste insertado. «Editar todo» → un frente menos → «Aplicar al borrador» → «Cerrar» → un cambio de fuera (la guía fija el gesto: RACKEDITAR, o RACKPROYECTAR como en el paso 34 de Smoke-2) → «Actualizar» | La respuesta, literal. «No se aplicó: el dibujo cambió fuera del panel. Diferencias: …» y «Conservar mi borrador», o `NotAppliedPreflight` | PASS si no se escribe nada sin avisar. `NotAppliedPreflight` está previsto (R-T4-39). Si nada lleva a «Conservar mi borrador», SPB-25 a SPB-28 quedan NOT AVAILABLE | S |
| SPB-25 | El común (paso 2) | «Conservar mi borrador» → primer «Sí» | Las líneas «Por grupos» encima del Sí/No: «Cambiado fuera: …», «Cambiado por ti: …» y, si tocan, «Cambiado en los dos: …», «Cambios estructurales: …» y «Vistas: se borrará 1 lateral obsoleto.» | PASS si salen las que tocan. FAIL si conserva sin presentar. **O:** si basta para elegir | S+O |
| SPB-26 | OWNER-04 de PB-T4b; **D-PB-05, pasos 5 a 7 (I-04)**; el cierre de PB-T4b (`:59`, «Conservar lee el dibujo dos veces»; M-11) | Segundo «Sí», sin cambios en el dibujo → «Actualizar» → `RCDUMP`; el tiempo de cada «Sí» | Conserva a la primera. Tras «Actualizar», los campos del sobre que no son del editor (`SchemaVersion`, GUID, metadatos de sesión y `ExtensionData`, si la hay) son los del rack actual releído, no los del borrador viejo | PASS si no hay bucle y los campos que no son del editor no vuelven atrás. FAIL si no. El tiempo, INFORMATIVO | S |
| SPB-27 | El común (el paso 3 con un cambio real) | Otra copia, hasta la presentación → un cambio de fuera en otro campo → segundo «Sí» | Vuelve a presentar, con las líneas nuevas, y no conserva | PASS / FAIL | S |
| SPB-28 | OWNER-01 de PB-T4b («Es lo esperado»); D-PB-05, pasos 4 a 7 (I-04) | Copia nueva. Un cambio propio → renombrado de fuera con RACKEDITAR → «Conservar» → los dos «Sí» → «Actualizar» → `RCDUMP` | «Cambiado fuera: el nombre.»; el dibujo lleva el nombre del borrador; los campos que no son del editor, como en SPB-26 | PASS si se cumple lo decidido y los campos no vuelven atrás | S |

#### Bloque E. La recarga de «Editar todo» (PB-T4e; [T4] §5.5, «Desde la v5.5»; F4B-E60)

| Id | Paso de [T4] | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-29 | 1 | Fijar la pestaña y «Editar»; no cambiar nada | «Listo para editar» | PASS / FAIL | S |
| SPB-30 | 2 | Cambiar fuera, con RACKEDITAR, un valor que la ventana completa enseña. Captura | «Limpio (posiblemente obsoleto)» y la caja en solo lectura | PASS / FAIL | S |
| SPB-31 | 3 | «Editar todo» → cerrar sin cambios | Abre con el valor nuevo, sin aviso; al cerrar, «Editor cerrado sin cambios en el borrador.», «Limpio» y la caja editable | PASS / FAIL | S |
| SPB-32 | 4 | 1 y 2 otra vez → «Editar todo» → un cambio → «Aplicar al borrador» → cerrar → «Actualizar» → `RCDUMP` | Sin «No se aplicó: el dibujo cambió fuera del panel» ni «Conservar mi borrador»; los dos cambios en el dibujo | PASS / FAIL | S |
| SPB-33 | 5 | Borrador Clean → borrar fuera todas las vistas del rack → «Editar todo» → Deshacer | No abre; «No editable aquí: el origen ya no está en el dibujo» (`OriginMissingDiagnostic`), la pista de RACKEDITAR y «No se recargó: el borrador se conserva tal como estaba.»; Deshacer restaura | PASS / FAIL | S |
| SPB-34 | 6; R-T4-39 | Copia y gemela. Un cambio propio → un cambio de fuera → «Editar todo» → «Actualizar». En la gemela, lo mismo con «Editar» | Abre sobre la base de antes; la respuesta, igual en las dos | PASS si coinciden. `NotAppliedPreflight` se cuenta al Owner como información | S |
| SPB-35 | 7; M-20 | «Editar todo» → cerrar sin «Actualizar»; `DBMOD` y Deshacer antes (después de la selección y de la vista previa) y después | Ningún cambio | PASS / FAIL | S |
| SPB-36 | 8; F4B-E51 y F4B-E53 (el coste de la lectura) | Con la ventana abierta, la paleta (K = 1); al cerrar, responde. El tiempo del clic a la ventana, con la marca y sin ella | La paleta deshabilitada; los tiempos | PASS en el modal; INFORMATIVO en el tiempo. **O:** la espera | S+O |

#### Bloque F. A/B: **CONSTRUCTED REPRESENTATIVE TEST CASES**

**Origen común:** D-PB-09 (OB-09.1); R-PB-04; [T3] §5.5 («los racks A/B fabricados»). Todo lo de este bloque va rotulado, en la guía, el registro, las capturas y los inventarios.

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-37 | OB-09.1; [BM] M-4 | **Fabricación,** con el clásico, en una copia nueva de D1: RACKEDITAR → marcar «Rack de dos sentidos (lado B)» → «Lado activo: Lado B» → quitar «En blanco» a un frente de B → «Actualizar» → `RCDUMP ab1` → RACKEDITAR → insertar el frontal entrada/salida de B → `RCDUMP ab2`. Cada copia fabricada, desde el original | En `ab1`, el lado B; en `ab2`, el frontal B (frontal, sección 2) | PASS si queda fabricado y rotulado. Si no se puede, STOP del bloque | S |
| SPB-38 | OB-03.2; D-PBT4-05; F4B-E53 | Seleccionarlo → «Editar» | «Listo para editar», sin «fuera del piloto (F5)» | PASS / FAIL | S |
| SPB-39 | OB-03.2; [T4] §5.5; OB-04.2 | Copia y gemela fabricadas. «Editar todo» → un segundo frente de B → «Aplicar al borrador» → «Actualizar» → `RCDUMP`. En la gemela, RACKEDITAR | Dos frentes de B; las vistas redibujadas; ninguna vista nueva; igual que la gemela | PASS / FAIL. **O:** las vistas A/B | S+O |
| SPB-40 | F4B-E59, OWNER-02, camino 3; R-PB-11; [F] §6.4, fila 3; [T3] §5.5 (5), el rechazo del frontal B; I-06 | Copia fabricada con el frontal B, y gemela. «Editar todo» → quitar el lado B → «Aplicar al borrador» → «Actualizar» → `RCDUMP`. En la gemela, RACKEDITAR → `RCDUMP`. Capturas de antes y después en las dos, guardadas para el Owner | Panel: «La vista <nombre> es un frontal del lado B en un rack sin lado B: el panel no la borra ni la redibuja; usa RACKEDITAR.» y el dibujo sin cambios. Clásico: el frontal redibujado de un sentido, renombrado y con `Section` 2 | PASS si el panel no escribe nada y el clásico hace lo medido en [BM]. FAIL si el panel escribe | S |
| SPB-41 | OWNER-01 de PB-T1c; [T4] §5.5 («"Restaurar"»); M-01 | Copia y gemela fabricadas. El mismo gesto (el hueco entre lados) → «Restaurar», el botón con el tooltip «Vuelve a los valores calculados del sistema.» → aplicar y «Actualizar»; en la gemela, RACKEDITAR | Los dos inventarios | PASS si coinciden. OBSERVADO si se ve el desfase de una pasada, que es una limitación conocida e igual en la base. FAIL solo si difieren | S |
| SPB-42 | H-31 (el hueco y las cajas avanzadas) y OWNER-1 de PB-TC2; F4B-E09 y F4B-E12. Cobertura añadida por la sesión | Copia fabricada. (a) El hueco con 4 decimales → «Actualizar» → pasar por la caja → «Actualizar» → `RCDUMP`. (b) Un campo de celda distinto en A y B → «Ambos lados» (la caja sale mixta) → escribir el número que se ve → «Actualizar» → `RCDUMP` | (a) El exacto; (b) los dos lados iguales | PASS / FAIL | S |
| SPB-43 | F4B-E09, respuesta 3; D-PBT4-05 | Ninguno: no hay ningún rack real de un sentido con cola, y D-PB-09 solo autoriza fabricar A/B | — | NOT AVAILABLE FOR PHYSICAL SMOKE | S |

#### Bloque G. El borrado real y la relectura con el origen

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-44 | [T4] §5.5; [T3] §5.5 («el rack abierto desde un lateral que la edición deja obsoleto», V2-01); F4B-E51 (OWNER-01; OWNER-03 (b)); M-09; M-10 | Copia con el lateral del último poste insertado. Seleccionar **ese lateral** → «Editar» → «Editar todo» → un frente menos → «Aplicar al borrador» → «Actualizar» → «Comprobar ahora» → «Editar». El tiempo de cada uno | Se aplica y borra el lateral de origen. **El reanclaje de POST es interno:** se mide su efecto, que la relectura siguiente funcione sobre otra vista. Se registra a qué vista va | PASS si se aplica y la relectura sigue. OBSERVADO si el origen cae en el `Primary` o sale «Ningún sobre editable de la relectura coincide con el origen del borrador.». El tiempo, INFORMATIVO | S |
| SPB-45 | OWNER-1 de PB-T3c (F4B-E48); **Q-PBT5-4** | Solo con la respuesta A, que es construir: copia y gemela; el lateral insertado y otra referencia en una presentación (`-INSERT`), rotulado → panel: un frente menos → «Actualizar» → `RCDUMP`. En la gemela, RACKEDITAR | Las dos referencias borradas; si hay aviso; igual que la gemela | PASS si coincide con el clásico. La falta de aviso, OBSERVADO. **Sin la respuesta A no se hace** | S |
| SPB-46 | OWNER-2 de PB-T3c; [T3] §5.5 (1), «con una referencia anidada»; [BM] §3; **Q-PBT5-4** | Solo con la respuesta A: copia y gemela; el lateral dentro de un bloque del usuario (`-BLOCK`), rotulado → panel → «Actualizar» → `RCDUMP` (`rcnest` y `rcblk`). En la gemela, RACKEDITAR | Qué queda del bloque y de las anidadas | Como SPB-45 | S |
| SPB-47 | OWNER-3 de PB-T3c; M-21 | Recuento con `rcnest` en las copias. Solo cuentan los contenedores dinámicos (`rcdyn` = 1) o anónimos `*U…`; el control positivo de D1, en un bloque pegado, no cuenta. Si hay alguno, lo mismo que SPB-46, sin construir nada | — | Si no hay: NOT AVAILABLE FOR PHYSICAL SMOKE | S |
| SPB-48 | OWNER-2 de PB-T3b, la parte del rechazo (AUTH-13) | Recuento, desde el JSON de `RCDUMP`, de los sobres con `SchemaVersion` 1.1 y de los racks con sobres de nombres distintos. Si hay alguno: «Editar» | El rechazo, con su diagnóstico | Si no hay: NOT AVAILABLE FOR PHYSICAL SMOKE. Los pasos 5 a 7 de D-PB-05 se miran en SPB-26 y SPB-28 | S |

#### Bloque H. La regla única cableada

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-49 | [T4] §5.5; D-PBT4-06 | Los mismos valores no válidos de la escala («abc», «0» y «-1») en el campo del panel y en la caja de la ventana | Los mensajes, por captura ampliada | PASS si son iguales letra a letra | S |

#### Bloque Z. Cierre de la corrida

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-50 | [F] §6.3; [S2] R7; M-16 | En cada copia: si hay borradores en riesgo, el producto veta el cierre (INV-20), así que primero «Descartar borradores de este documento» → «Descartar» ([S2], pasos 80 y 82); después `CLOSE` → «No». Al final, cerrar la instancia de la sesión. La del Owner no se toca | Ninguna copia guardada; ningún `acad.exe` de la sesión | PASS / FAIL | S |
| SPB-51 | [S2] `integrity.md`; [BM], «Al cerrar» | SHA-256 de originales, respaldos y copias frente a `copias.json`; autoguardados; `git -C <worktree> status` y HEAD | Originales y respaldos iguales; copias = original; el árbol limpio; HEAD = Candidato | PASS / FAIL | S |
| SPB-52 | [S2] F4-E79; M-17 | `RACKCAD_DEBUG_*`; líneas `ERROR` del registro de RackCad; eventos de error de acad; `Applications`, `Loaded` y `AssemblyMap`; el número de `Dialogs\RackCad.*`; la disposición del configurador y `settings.json` | Lo mismo que en P-05 en `Applications`, `Loaded` y `AssemblyMap`. Las `Dialogs`, registradas como escritura esperada del host | PASS si no queda configuración de RackCad ni errores sin explicar | S |

#### Bloque I. La lista de [T3] §5.5 (B-01)

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-53 | [T3] §5.5 (3); `PushBackPilotUpdateRun.NameDiffersPrefix`; [BM] M-3 | Copia nueva. «Editar todo» → el nombre de otro rack que ya existe en D1 → «Aplicar al borrador» → «Actualizar» → `RCDUMP` | El aviso, que empieza por «El nombre del bloque de la vista »; las definiciones, que no cambian de nombre; `Name` en los sobres | PASS si es como el clásico ([BM] M-3: cambia el sobre y no la definición) y sale el aviso. FAIL si no | S |
| SPB-54 | [T3] §5.5 (2), con renombrado; N-I1 | Copia nueva. «Editar todo» → un nombre nuevo, sin colisión → «Aplicar al borrador» → «Actualizar» → `U` sin nada en medio → `RCDUMP u1` → `REDO` | Un `U` deshace el renombrado entero | PASS si `u1` = el estado previo | S |
| SPB-55 | [T3] §5.5 (2), con la conversión A/B; OB-09.1 | **CONSTRUCTED REPRESENTATIVE TEST CASES.** Copia nueva. «Editar todo» sobre un rack real de un sentido → marcar el lado B y activar un frente → «Aplicar al borrador» → «Actualizar» → `U` sin nada en medio → `RCDUMP u1` → `REDO` | Un `U` deshace la conversión entera | PASS si `u1` = el estado previo | S |
| SPB-56 | [T3] §5.5 («la barrera del piloto», V2-09); `PushBackPilotUpdatePlan.BlockedPrefix` | Ninguno propio: se registra si, en algún paso con un rack real, «Actualizar» dio un texto que empieza por «El diseño tiene un error que impide escribirlo: » | — | Si aparece en un rack real: PASS si no se escribe nada. Si no: NOT AVAILABLE FOR PHYSICAL SMOKE, cubierto por CT-PBT3B-06 | S |
| SPB-57 | [T3] §5.5 (4); `PushBackPilotUpdateRun.LockedLayerPrefix`; **Q-PBT5-4** | Solo con la respuesta A: copia con el lateral insertado; su capa bloqueada con `-LAYER`, rotulado → panel: un frente menos → «Actualizar» → `RCDUMP` | Un texto que empieza por «LOCKED_LAYER: »; nada escrito | PASS si rechaza sin escribir. **Sin la respuesta A no se hace** | S |
| SPB-58 | [T3] §5.5 (5), el último vínculo; [BM] M-2b; `PushBackPilotUpdatePlan.AllViewsObsoleteReason`; **Q-PBT5-4** | Solo con la respuesta A: el método de [BM] M-2b (`ERASE` de las demás referencias y `PURGE` de sus definiciones), rotulado → panel: un frente menos → «Actualizar» → `RCDUMP` | «Todas las vistas del rack quedarían obsoletas: no se elimina el último vínculo del rack.»; nada escrito | PASS si rechaza sin escribir. **Sin la respuesta A no se hace** | S |
| SPB-59 | [T3] §5.5 (6); [T3] `:915` (la cuenta incluye `EraseUnit`); [S2], pasos 78 a 80; R-PB-03; **Q-PBT5-10** | Solo con la respuesta A: un AutoCAD propio con `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT=2` **solo en el entorno de ese proceso**, con una copia con el lateral insertado → panel: un frente menos → «Actualizar» → `RCDUMP` → descartar y cerrar. Antes y después, ninguna variable en el usuario, la máquina ni el shell | «No se aplicó: La escritura falló antes de confirmarse: …»; ninguna vista cambia; una entrada de Deshacer | PASS si el aborto no deja nada escrito. **Sin la respuesta A no se hace** | S |
| SPB-60 | [T3] §5.5 (6), el sellado; [S2], paso 82; **Q-PBT5-10** | Lo mismo, con `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL=before` | El estado desconocido, «Comprobar ahora» y que nada cambió | Como el paso 82 de [S2] | S |
| SPB-61 | [T3] §5.5 (6); [S2], paso 84; **Q-PBT5-10** | Lo mismo, con `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL=after` | El estado desconocido; la escritura confirmada, con el borrado incluido | Como el paso 84 de [S2] | S |

#### Bloque J. La relectura con el origen: los subcasos de PB-T4a (M-10)

| Id | Origen | Qué se hace | Qué se observa | Criterio | Observa |
|---|---|---|---|---|---|
| SPB-62 | OWNER-03 de PB-T4a, el primer caso declarado («un origen borrado fuera, que cae al `Primary`») | Copia nueva. «Editar» desde un lateral → `ERASE` fuera de la referencia de ese lateral (un gesto del usuario, como el paso 5 de PB-T4e) → «Comprobar ahora» | La relectura, que cae al `Primary`, o su mensaje | PASS si se comporta como declara PB-T4a. OBSERVADO si no | S |
| SPB-63 | OWNER-03 de PB-T4a, el segundo caso declarado (dos vistas con el mismo texto) | El rack real de D1 con dos laterales enlazadas (una con el sufijo `_1`): «Editar» desde una → «Comprobar ahora» | «Varios», con la caída al `Primary` | PASS / OBSERVADO. Si la copia enlazada ya no tiene el mismo texto, NOT AVAILABLE | S |
| SPB-64 | M-23 de [T4] (`:977`, «las relecturas pasan del `Primary` al origen del borrador») | Copia nueva. «Editar» desde un lateral → «Comprobar ahora» y «Volver a leer» | Sin «falta el origen» y sin cambiar de vista de origen. Que la lectura va al origen es interno: se mide ese efecto | PASS / OBSERVADO | S |
| SPB-65 | OWNER-03 (a) de PB-T4a (sin destinos frescos de navegación) | Ninguno: depende del estado interno de la navegación, que AutoCAD no muestra | — | NO VERIFICABLE FÍSICAMENTE | S |

### 4.3 Recuento

| Bloque | Pasos | S | S+O |
|---|---|---|---|
| 0 | 4 | 4 | 0 |
| A | 6 | 4 | 2 (SPB-08 y 09) |
| B | 7 | 6 | 1 (SPB-16) |
| C | 6 | 3 | 3 (SPB-18, 20 y 23) |
| D | 5 | 4 | 1 (SPB-25) |
| E | 8 | 7 | 1 (SPB-36) |
| F | 7 | 6 | 1 (SPB-39) |
| G | 5 | 5 | 0 |
| H | 1 | 1 | 0 |
| Z | 3 | 3 | 0 |
| I | 9 | 9 | 0 |
| J | 4 | 4 | 0 |
| **Total** | **65** | **56** | **9** |

**Dependen de una respuesta del Owner:**
- SPB-45, 46, 57 y 58, de Q-PBT5-4;
- SPB-59 a 61, de Q-PBT5-10;
- la parte de SPB-07 en los demás dibujos, de Q-PBT5-2.

**LIMITATION ya previstos:** SPB-22, SPB-43 y SPB-65. SPB-07, SPB-47, SPB-48 y SPB-56 lo serán si no hay casos reales.

### 4.4 Lo que el Smoke-PB no cubre

- **Guardar la copia y reabrirla:** ningún registro de F4B lo pide.
- **La carrera de cierre a mitad de transacción:** la de F4, no provocada. F4B no toca el cierre.
- **El nombre del grupo de Deshacer y el número de transacciones:** NO OBSERVABLE ([BM] §2).
- **Corrección de la v1:** las inyecciones UNIT y SEAL **sí** entran (R-PB-03; [T3] §5.5, (6)). Son SPB-59 a SPB-61, y dependen de Q-PBT5-10.

### 4.5 Trazabilidad

| Lo que se dejó al Smoke-PB | Dónde | Paso |
|---|---|---|
| A/B fabricados y rotulados; racks reales en lo demás | D-PB-09 | Bloque F y SPB-55; los demás bloques |
| D-PB-06 y D-PB-08 en RACKEDITAR | [F] §9 | SPB-06 a SPB-08 |
| A2-11: el paso 4 de D-PB-06 hasta el DWG (el exacto y el estilo) | [T3] §5.6; F4B-E06 | SPB-06 (b) y SPB-07 (b) |
| El aviso de D-PB-08 en WORKSPACE | F4B-E06 | SPB-16 |
| La geometría del reinicio de D-PB-11 y el aspecto del aviso | El cierre de PB-TC (`:65`) | SPB-09 y SPB-08 |
| H-31 y OWNER-1 de PB-TC2 | F4B-E12 (cobertura añadida) | SPB-10 y SPB-42 |
| R-PB-03 | [F] §8 | SPB-18, 19, 45 a 47 y 59 a 61 |
| R-PB-04 | [F] §8 | Bloque F |
| R-PB-06 | [F] §8; [T3] §5.5 | SPB-23 |
| R-PB-11 | [F] §8 | SPB-40 |
| El borrado real con sus referencias, una anidada y la purga | [T3] §5.5 (1); el cierre de PB-T3c (`:53`) | SPB-18 (f) y SPB-46 |
| Un `U` con borrado, con renombrado y con A/B | [T3] §5.5 (2) | SPB-19, 54 y 55 |
| El renombrado con colisión y `NameDiffersPrefix` | [T3] §5.5 (3) | SPB-53 |
| `LOCKED_LAYER` | [T3] §5.5 (4) | SPB-57 |
| Los rechazos del frontal B y del último vínculo | [T3] §5.5 (5) | SPB-40 y 58 |
| El lateral de origen que la edición deja obsoleto | [T3] §5.5; [T4] §5.5 | SPB-44 |
| Las inyecciones de borrado y de sellado | [T3] §5.5 (6) | SPB-59 a 61 |
| La barrera del piloto | [T3] §5.5 | SPB-56 |
| El botón, el modal, el abridor y K = 1 | [T4] §5.5; F4B-E55 | SPB-11, 12, 15 y 36 |
| OB-01.4 | [T4] §5.5 | SPB-13 |
| El coste | [T4] §5.5 | SPB-23, 26, 36 y 44 |
| «Restaurar» | [T4] §5.5 | SPB-41 |
| OWNER-2 de PB-T3b: los pasos 5 a 7 de D-PB-05 y el rechazo de AUTH-13 | [T4] §5.5 y `:1530`; I-04 | SPB-26, 28 y 48 |
| La lectura completa: un A/B se abre en el panel | [T4] §5.5; F4B-E53 | SPB-38 y 39 |
| La regla única | [T4] §5.5 | SPB-49 |
| El paso 3 con un cambio real | [T4] §5.5 | SPB-27 |
| OWNER-1 a OWNER-3 de PB-T3c | F4B-E48 | SPB-45 a 47 |
| La recarga, pasos 1 a 8 | [T4] §5.5; F4B-E60 | SPB-29 a 36 |
| R-T4-39 | F4B-E60 | SPB-24 y 34 |
| OWNER-01 y OWNER-03 de PB-T4a; M-23 | F4B-E51; [T4] `:977` | SPB-44 y 62 a 65 |
| OWNER-01 y OWNER-04 de PB-T4b; el coste de «Conservar» | F4B-E53; el cierre de PB-T4b (`:59`) | SPB-28 y 26 |
| OWNER-02, OWNER-03 y OWNER-04 de PB-T4c | F4B-E55 | SPB-29 a 36, 17, 21, 18 (a) y 22 |
| El foco, el teclado, «Actualizar» tras editar y los eventos del puente | F4B-E55 | SPB-14, 18 y 15 |
| Los tres caminos de `BuildCore` | F4B-E59 | SPB-18, 20 y 40 |
| El coste de `Equals` | El paquete de PB-T2 (`:1329`) | SPB-23 |
| `pb-21-cola` | F4B-E09 | SPB-43 |
| La CI exacta | [F] §9 | P-01 |
| Los rótulos de CLASSIC | OB-01.6; OB-RE.1 | SPB-05 |

---

## 5. Cómo se ejecuta

### 5.1 Los dibujos

- Solo copias de trabajo. Los originales nunca se abren, ni con AutoCAD ni con `accoreconsole`.
- Antes de usar un original, una copia de seguridad, con su SHA-256 comprobado.
- Cada copia sale del original. Nada se guarda sobre un original, y las copias se cierran sin guardar.
- Las salidas del producto, solo en `<carpeta-de-salidas>`.
- **`<carpeta-de-respaldo>` y `<carpeta-del-Smoke-PB>` quedan fuera del worktree y de todo repositorio** (M-18). Si no, P-01 y SPB-51 fallarían, y habría copias de dibujos de clientes al alcance de un commit.
- **Ninguna escritura sobrescribe un archivo existente:** se para si el destino existe.
- **Privacidad:** sin nombres ni rutas de clientes. Los JSON crudos se quedan en local, con su SHA-256.

### 5.2 El DLL

- **El DLL Debug del worktree,** compilado desde el Candidato, en la ruta de [G] §2 y WORKFLOW §6, con AutoCAD cerrado y el `dotnet` del usuario, desde la raíz del worktree para que se resuelva `global.json`.
- **Antes de NETLOAD:** la `InformationalVersion` y el SHA-256 ([G] §7.2).
- **La diferencia con Smoke-2, declarada:** Smoke-2 compiló con `-p:UseAutoCADNuGetReferences=true -o <carpeta>`.
  - Si la versión no identifica el Candidato, se vuelve a compilar con `--no-incremental`.
  - Si sigue sin identificarlo: PBT5-C01.

### 5.3 La configuración de AutoCAD, la carga y el inventario

- **No se toca nada:** ni Trusted Locations, ni SECURELOAD, ni «Always Load», ni el registro, ni ninguna variable de sistema.
- **La carga, como en [G] §3:** abrir una copia → `NETLOAD` del DLL exacto → `RACKCAD` → la biblioteca.
  - El aviso de seguridad, «Load Once».
  - Los avisos de arranque, sin «Always ignore».
- **El inventario:** las funciones de [BM], tecleadas en cada documento porque SECURELOAD impide `load`, más `rcblk`, `rcdyn` y `rcnest`, de solo lectura. Su texto exacto está en `pre-run-tonight-v2.1.md`, §4.6. Antes de usarlas se comprueban una vez en la primera copia, con los controles de M-21 (R-5).
- **El paso de una copia a la siguiente** (I-07):
  1. desde la copia activa, se abre la siguiente con `vla-open`;
  2. en ella se cierra la paleta de referencias externas (`EXTERNALREFERENCESCLOSE`), que D1 abre siempre;
  3. se teclea el inventario;
  4. la anterior se cierra desde la nueva con `vla-close … :vlax-false`.

  AutoCAD nunca se queda sin un documento con el LISP cargado, y **nunca se usa COM**, que podría engancharse a un AutoCAD que no abrió la sesión. Si la instancia se queda sin documentos, se cierra por su PID y se relanza. Las órdenes exactas, en `pre-run-tonight-v2.1.md` §4.7 y §4.8.

### 5.4 Quién opera

- **La corrida previa de esta noche:** la sesión, por la orden directa del Owner y con la lectura que fijó la sesión principal (`pre-run-tonight-v2.1.md`).
  - Empieza solo si está lista antes de las 13:00 UTC.
  - A las 13:00 UTC, para en el siguiente límite de paso y espera.
  - Q-PBT5-5 pide al Owner que confirme esa lectura.
- **El Smoke-PB contratado** (tras Q-PBT5-7 y Q-PBT5-8):
  - **con el Owner:** él opera los gestos de los pasos que quiera y juzga los S+O. La sesión mide lo de máquina, avisa antes de teclear un inventario y devuelve el control después;
  - **o la sesión, si el Owner lo ordena,** como en la corrida 2 de Smoke-2. Sus resultados son propuestas, y los S+O quedan PENDIENTE DE REVISIÓN DEL OWNER.
- **En cualquier caso, la firma es del Owner.**

### 5.5 Las reglas de operación por control de pantalla

- Textos en trozos de 16 caracteres como máximo; líneas de 1024 como máximo; 3 s entre `defun`.
- Una captura antes de cada clic, abierta y revisada al tomarla.
- **En cada resultado negativo** («no responde», «nada llega»): las coordenadas del clic, en el registro, y una captura de después (M-14).
- Nunca Ctrl+V en la línea de órdenes.
- El primer clic tras dar el foco a la paleta solo la activa.
- Una ventana inesperada con el foco para la acción. Nunca se abre otra instancia por error.
- N-I1.
- **El error del operador:** como máximo dos repeticiones por paso; a la tercera, o con dudas, vuelve al Owner (PBT5-C07).

### 5.6 La evidencia para firmar sin haber estado delante

En `I-64-pilot/F4B-SMOKE-PB/`:
- **`prep/`:** `candidate.md` (el bloque de [G] §7.1, con «UI Full local: pendiente de Q-PBT5-1» mientras no haya respuesta), `dll.md`, `selection.md`, `copias.md`, `environment-pre.md` y la guía redactada.
- **`prerun/` o `run/`:**
  - `run-log.md`, con la hora UTC de cada acción, el resultado propuesto, las desviaciones y los incidentes;
  - `screens/`, recortadas, sin rutas ni nombres, con `derivation-record.txt`, y las de antes y después de SPB-09, 19, 20 y 40;
  - `inventories/`, con los resúmenes y el SHA-256 de cada crudo, `diffs.txt` y el análisis de los campos del sobre de I-04;
  - `integrity.md` y `environment-post.md`;
  - `owner-summary.md`: una fila por paso, la sección «Para tu revisión» con los 9 S+O y sus capturas, los LIMITATION con su motivo, los FAIL y los OBSERVADO con toda su evidencia, y la información de R-T4-39.
- **La cabecera de `run-log.md`** (M-22) lleva el sha256 del documento de la corrida, el del paquete y el de la guía redactada, que son los textos con que se opera cada paso.
- **Las capturas:**
  - las crudas, solo en `<carpeta-del-Smoke-PB>\screens\raw\`, y nunca se custodian;
  - las custodiadas, recortadas al área del paso o tapadas con un rectángulo opaco, con la lista de antes de custodiar y `derivation-record.txt`;
  - nunca se custodian la paleta de referencias externas, el aviso de NETLOAD, los diálogos de archivos ni una captura con otra aplicación;
  - las reglas completas están en `pre-run-tonight-v2.1.md` §8, y valen también para la corrida contratada.

### 5.7 Lo que no se finge

- Ninguna condición interna que AutoCAD no muestre: SPB-22 y SPB-65; el nombre del grupo de Deshacer.
- Ningún arnés, ningún código de producto para probar, y ninguna escritura en un Xrecord fuera del producto (SPB-07, salvo la respuesta B de Q-PBT5-2).
- **Las variables `RACKCAD_DEBUG_*`,** nunca en el usuario, la máquina ni el shell de la sesión. Solo en el entorno de un proceso de AutoCAD propio, para SPB-59 a SPB-61 y con la respuesta A de Q-PBT5-10, como en Smoke-2.
  - **Corrección de la v1:** la decisión 1 de Smoke-2 solo prohíbe **añadir** una inyección al escritor; Smoke-2 corrió tres.
- **Ningún caso fabricado:** solo los A/B de D-PB-09, y los contextos de Q-PBT5-4 con su respuesta A.

---

## 6. El contrato (propuesto; su forma la decide el Owner)

### 6.1 La forma

- **PB-T5 no cambia `src/`, `tests/` ni `assets/`,** y no hay entrega de un Worker.
- **[F] §5 pide a cada tarea la cadena de F4,** que acaba en D2. Solo adapta la de PB-T0 (§6.5), y §7.2 no deja heredar reglas de F4.
- **Smoke-2 no era una tarea, sino una actividad del gate,** regida por decisiones del Owner y cerrada por su firma (F4-E81).
- **El Owner eligió la v3 del sustituto de nc2 con una opción que decía «la aplico a este nc2 y a PB-T5»** (`owner-pbt4d-nc2-decision.md`, opción 1; F4B-E58).
- **Por eso la forma es una pregunta, Q-PBT5-8,** con cuatro opciones:
  - A: adaptación, y firma sobre la evidencia;
  - B: adaptación, con revisión independiente de la evidencia antes de la firma;
  - C: la cadena de F4 completa, con nc2 por la v3;
  - D: otra.
- **Lo que sigue en §6.2 a §6.6 vale para A y B.** Con C, el contrato se escribe en el formato de los contratos de F4B y lleva estos mismos invariantes y paradas como texto.
- **El cierre es por la firma** ([F] §9; F4-E81), en cualquiera de las opciones.

### 6.2 La identidad y el alcance de escritura

- **`TaskId` (provisional):** `F4B-PBT5-SMOKE-PB-GATE-CLOSE`.
- **En el repositorio:** `F4B-SMOKE-PB/`, `F4B-DRAFTS/pbt5/`, `F4B-DRAFTS/coordinator/`, `F4B-DRAFTS/gate-close.md`, [DISP], `I-64-evidence.md` y `I-64.yml`; `docs/ideas-futuras.md`, solo por orden del Owner.
- **Fuera:** `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>` (copias, respaldos, salidas, inventarios y capturas), fuera de todo repositorio (M-18), y el `bin/` y el `obj/` del worktree.
- **Prohibido:**
  - `src/`, `tests/`, `assets/` y los archivos de proyecto;
  - [G], HANDOFF, ROADMAP, WORKFLOW y AGENTS;
  - `main` y otras ramas;
  - los originales;
  - la configuración de AutoCAD y el registro;
  - las bibliotecas y `settings.json` del usuario.

### 6.3 Invariantes

| Id | Invariante |
|---|---|
| INV-PBT5-01 | `git diff <Candidato> <punta del cierre> -- src tests assets` vacío |
| INV-PBT5-02 | El DLL sale del Candidato: su `InformationalVersion` lo identifica, y su SHA-256 se registra antes de NETLOAD y se comprueba en el proceso |
| INV-PBT5-03 | Solo copias; los originales, con respaldo y sin abrir |
| INV-PBT5-04 | Ningún cambio de configuración: «Load Once» y ningún «Always …» |
| INV-PBT5-05 | Nada se finge. Ninguna `RACKCAD_DEBUG_*` en el usuario, la máquina ni el shell; en el entorno de un proceso propio, solo para SPB-59 a SPB-61 con Q-PBT5-10 A |
| INV-PBT5-06 | Los A/B y los contextos construidos llevan el rótulo **CONSTRUCTED REPRESENTATIVE TEST CASES** |
| INV-PBT5-07 | Los resultados de la sesión son propuestas; F4B se cierra solo con la firma del Owner |
| INV-PBT5-08 | La evidencia custodiada no lleva rutas, nombres de clientes ni datos personales |
| INV-PBT5-09 | [G] no cambia |
| INV-PBT5-10 | Sin integración en `main`; HANDOFF y ROADMAP sin cambios |
| INV-PBT5-11 | Una captura antes de cada clic, abierta y revisada; coordenadas y captura de después en cada resultado negativo |
| INV-PBT5-12 | N-I1 |
| INV-PBT5-13 | Las salidas del producto, solo en `<carpeta-de-salidas>` |
| INV-PBT5-14 | Ningún FAIL se corrige en PB-T5 |
| INV-PBT5-15 | Ninguna suite completa de UI en local sin la respuesta de Q-PBT5-1 (F4-E59) |

### 6.4 Paradas

| Id | Cuándo | Qué se hace |
|---|---|---|
| PBT5-C01 | **El entorno:** el árbol sucio, HEAD distinto, el diff no vacío, la CI sin la corrida de push exacta, el DLL que no identifica el Candidato o un SHA-256 del DLL que cambia | STOP antes de abrir AutoCAD, o en el momento |
| PBT5-C02 | Una copia distinta de su original, o un original cambiado | STOP; si cambió un original, aviso inmediato al Owner |
| PBT5-C03 | Haría falta cambiar la configuración | STOP |
| PBT5-C04 | Una `RACKCAD_DEBUG_*` en el usuario, la máquina o el shell | STOP |
| PBT5-C05 | Un FAIL de producto | Se registra con toda su evidencia, se siguen solo los pasos independientes e inocuos, y vuelve al Owner |
| PBT5-C06 | Una escritura posible fuera de las copias o de `<carpeta-de-salidas>` | STOP del paso; LIMITATION |
| PBT5-C07 | La tercera repetición por error del operador, o la duda | Al Owner |
| PBT5-C08 | Un AutoCAD del Owner abierto al compilar, o que haría falta tocar | No se toca; STOP del build. Nunca se usa COM para abrir o cerrar documentos (I-07) |
| PBT5-C09 | Un Worker u otra sesión en el worktree | STOP |
| PBT5-C10 | Solo observable fingiendo | NO VERIFICABLE FÍSICAMENTE |
| PBT5-C11 | La frontera horaria | A las 13:00 UTC, se para en el límite de paso y se espera |
| PBT5-C12 | AutoCAD se cuelga o se cierra | Se registra y se cierra de forma ordinaria. El bloque a medias se repite en una copia nueva; el segundo caso, al Owner |
| PBT5-C13 | Un comportamiento no caracterizado | OBSERVADO, con toda la evidencia |
| PBT5-C14 | Haría falta el intento 3 | Al Owner |
| PBT5-C15 | **Una prueba falla en la CI o en local sobre el Candidato** (M-07) | Es un fallo de producto y un modo nuevo: vuelve al Owner, no es una parada de entorno. **Propuesta para reejecutar:** una CI en rojo solo por el detector de cuelgue (`blame-hang`) en una prueba ya conocida admite una sola reejecución de los trabajos fallidos sobre el mismo SHA; otra causa, o un segundo rojo, vuelve al Owner. La regla va escrita en Q-PBT5-8, y el Owner la confirma al responderla (M-23) |

### 6.5 La evidencia esperada

- `F4B-SMOKE-PB/prep/`, `prerun/` y `run/` (§5.6).
- `F4B-DRAFTS/pbt5/`: el paquete y sus versiones, las revisiones, la guía redactada, los traspasos y las herramientas.
  - `_tools/check_pbt5.py` lee sus patrones de `_tools/patterns.local.txt`, que **no se custodia**.
  - `_tools/control_patterns.py` lee sus muestras de `_tools/control-samples.local.txt`, que tampoco se custodia, y `check_pbt5.py` lo analiza (M-19).
  - Si el guion casara algo prohibido, va solo por su sha256 ([DISP] F4B-E60).
- `F4B-DRAFTS/coordinator/`: las respuestas del Owner, su disposición y su firma.
- `F4B-DRAFTS/gate-close.md`, con la tabla de los INV.
- Lo del §3.3 en [DISP], `I-64-evidence.md` e `I-64.yml`.

### 6.6 El presupuesto

- **1 de 3** (OB-10.1); el intento 3, al Owner (OB-10.2).
- **Qué es un intento, y si cuenta la corrida previa,** lo decide el Owner (Q-PBT5-9). La propuesta de su opción A:
  - un intento es una corrida sobre un Candidato;
  - no cuentan las repeticiones por error del operador (dos como máximo), las paradas del entorno antes de SPB-01 ni las adendas que pida el Owner;
  - la corrida previa cuenta como parte del intento 1, si cumple la reutilización y el Owner la acepta paso a paso.
- **Una tarea de corrección** que nazca de un FAIL tiene su propio contrato y su 1 de 3.

### 6.7 La cadena

1. **Esta v2** y su comprobación acotada (B-01, B-02 e I-01 a I-06).
2. **La primera vuelta de preguntas:** Q-PBT5-7, 8, 9 y 5.
3. **El cierre de PB-T4e [T4e].**
4. **Esta noche, si se cumple su frontera:** la corrida previa (`pre-run-tonight-v2.1.md`) y su custodia, en un commit documental. No emite PB-T5.
5. **La segunda vuelta:** Q-PBT5-1, 10, 4 y 2.
6. **La emisión,** por Q-PBT5-7, con la forma de Q-PBT5-8.
7. **La preparación,** con el Candidato de §3.1.
8. **El Smoke-PB contratado:** lo que la corrida previa no cubrió, o todo, según Q-PBT5-9.
9. **La custodia;** la revisión independiente de la evidencia, si Q-PBT5-8 = B; la disposición del Owner.
10. **La firma y el gate-close.**

---

## 7. Riesgos

| Id | Riesgo | Tratamiento |
|---|---|---|
| R-1 | La corrida previa no acaba antes de las 13:00 UTC | Para en el límite de paso; el registro dice dónde; los bloques son independientes |
| R-2 | Ningún cambio de fuera lleva a «Conservar» (R-T4-39) | SPB-24 lo registra; SPB-25 a 28 quedan NOT AVAILABLE |
| R-3 | Un AutoCAD del Owner abierto impide compilar | PBT5-C08 |
| R-4 | La contención de perfil (F4B-E30) | Sin UI Full local hasta Q-PBT5-1; el archivo de disposición se mide antes y después de los filtros |
| R-5 | `rcblk` y `rcnest` son nuevas | Solo lectura; control positivo con la anidada conocida de D1 antes de usarlas |
| R-6 | El error del operador | Capturas, coordenadas, dos repeticiones como máximo |
| R-7 | Las capturas sacan rutas o nombres | Recortes y `derivation-record.txt` |
| R-8 | Lo construido se toma por real | El rótulo; el resumen los separa |
| R-9 | El aviso de NETLOAD ofrece «Always Load» | Captura antes del clic; solo «Load Once» |
| R-10 | Las salidas van a carpetas del usuario | `<carpeta-de-salidas>` en el diálogo, comprobada en la captura; P-05 para la carpeta de diseños |
| R-11 | La corrida previa no se acepta como parte del Smoke-PB | Q-PBT5-9; se repite lo que haga falta en la contratada |
| R-12 | D1 abre la paleta de referencias externas en cada apertura: quita el foco y muestra rutas | `EXTERNALREFERENCESCLOSE` antes de cualquier captura que se vaya a custodiar, y nunca se custodia (I-07) |
| R-13 | Un `Documents.Open` por COM se engancha a cualquier AutoCAD abierto | Sin COM: `vla-open` desde la copia activa, y si no queda documento, cerrar y relanzar la instancia propia por su PID (I-07) |

---

## 8. Supuestos declarados

- **S-1:** la lectura de la orden de esta noche que fijó la sesión principal (§5.4). Tiene a la vista el literal: «se complete antes de las 6 am», que son las 12:00 UTC. Q-PBT5-5 la confirma o la corrige.
- **S-3:** el Candidato es la punta del DLL (§3.1).
- **S-4:** los dibujos reales son D1 y, si un paso lo necesita, los de la selección de Smoke-2.
- **S-5:** los A/B se fabrican con el método de [BM] M-4.
- **S-6:** la igualdad con el clásico, en una gemela, es el oráculo de SPB-18, 39, 40, 41, 45 y 46.
- **S-7:** una copia por escenario contaminable (Q-PBT5-9 lo adopta o no).
- **S-10 (corregido, M-01):** los literales del §4 están en el `src/` de `7da361eb`. PB-T4e no añade ningún texto ([T4] D-PBT4-19). La guía los copia del `src/` del Candidato, por objetos git.
- **S-11:** la parte de cobertura de la antigua Q-PBT5-3 (H-31 y OWNER-1 de PB-TC2) la decide la sesión, como dice [IR1] (I-05), y se informa al Owner.

(S-2, S-8 y S-9 de la v1 pasan a Q-PBT5-5, Q-PBT5-9 y Q-PBT5-8.)

---

## 9. Preguntas al Owner

En `owner-questions-v2.1.md`, sin valores por defecto:
- **Vuelta 1:**
  - **Q-PBT5-7:** la emisión;
  - **Q-PBT5-8:** la forma del contrato y el cierre;
  - **Q-PBT5-9:** el intento, las reglas de Smoke-2 y la corrida previa;
  - **Q-PBT5-5:** la confirmación de la lectura de esta noche.
- **Vuelta 2:**
  - **Q-PBT5-1:** la UI Full local y F4-E59;
  - **Q-PBT5-10:** las inyecciones;
  - **Q-PBT5-4:** los contextos construidos;
  - **Q-PBT5-2:** el estilo de cotas y el criterio de cierre en parte.
- **Información:** D-PB-11 y la cobertura de RACKEDITAR (la antigua Q-PBT5-3); R-T4-39.

---

## 10. Lo no verificado

- **PB-T4e:** su cierre, su producto, su CI, si tendrá reparación del RED y los números F4B-E **[T4e]**.
- **No se leyó el worktree,** ni se compiló, ni se ejecutó nada, ni se abrió AutoCAD.
- **Sin comprobar:**
  - si se llega a «Conservar» (R-2);
  - si se alcanza escenificar un módulo (SPB-21);
  - si el hueco entre lados se edita como suponen SPB-41 y SPB-42;
  - si hay en D1 un rack con un diagnóstico bloqueante (SPB-56);
  - si la copia enlazada de D1 sigue teniendo el mismo texto (SPB-63).
- **Los dibujos:** dónde está hoy el original D1, y su SHA-256.
- **`rcblk`, `rcdyn` y `rcnest`** no se han ejecutado.
- **`gh`, `PICKFIRST` y si la instancia lanzada con `Process.Start` sobrevive a la orden que la lanza:** se comprueban en la corrida (`pre-run-tonight-v2.1.md` §4.1 y §4.5).
- **Si NETLOAD mostrará el aviso,** y **la duración real:** la corrida 2 de Smoke-2 duró 4 h 40 min para 88 pasos.
- **Los nombres** de `TaskId`, carpetas y archivos son provisionales.

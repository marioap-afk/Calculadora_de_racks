# I-64 F4B — Paquete de PB-T5: CI exacta, Smoke-PB y gate-close de F4B (v1)

> **Redacción para el repo público:** las rutas locales van como marcadores (`<worktree>`, `<carpeta-del-Smoke-PB>`, `<carpeta-de-respaldo>`, `<carpeta-de-salidas>`, `<original-D1>`, `<dotnet-del-usuario>`). No hay nombres personales ni de clientes. La hora del Owner se da en UTC; el nombre de su zona horaria, que trae su mensaje literal, va como `<zona-horaria-del-Owner>`.

**Estado:** v1, borrador para la revisión independiente. **No está emitido.** Lo escribe el autor del paquete por encargo de la sesión principal, mientras PB-T4e está en su Worker.

**Base y método:**
- Todo se lee por objetos git en `7da361eb`, la emisión de PB-T4e y la punta de la rama `architecture/workspace-persistente-rackcad`. No se leyó el worktree ni los archivos en disco del checkout principal.
- No se compiló, no se ejecutó ninguna prueba y no se abrió AutoCAD.
- Lo que depende del cierre de PB-T4e lleva la marca **[T4e]**: su producto, su CI y su número F4B-E. Se completa al cerrar PB-T4e, sin cambiar nada más.

**Abreviaturas:**
- **[F]:** el paquete de la fase, `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1.
- **[DISP]:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md`.
- **[T4]:** el paquete de PB-T4, `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` v5.6.
- **[G]:** la guía `docs/guias/validacion-manual-autocad.md`. **Este paquete no la modifica.**
- **[S2]:** el precedente de F4: `I-64-pilot/F4-SMOKE-2/`, `F4-DRAFTS/f4-dispositions.md` (F4-E71 a F4-E81) y `F4-DRAFTS/gate-close.md`.
- **[BM]:** el bloque M de PB-T0: `I-64-pilot/F4B-PBT0-CHARACTERIZATION/block-m/` y `coordinator/pbt0-block-m-procedure.md`.

---

## 0. En una página

- **Qué es PB-T5:** la última tarea de F4B. Tabla del §5 de [F]: «CI exacta, Smoke-PB (D-PB-09) y gate-close de F4B», que depende de PB-T4. Empieza cuando PB-T4e cierre.
- **No tiene Worker de código.** No cambia `src/`, `tests/` ni `assets/`. Su cadena se adapta como en el Smoke-2 de F4 y como el bloque M de PB-T0 (§6.1).
- **La CI exacta:** el Candidato de F4B es el SHA exacto desde el que se compila el DLL del Smoke-PB. Lleva la CI 4/4 sobre ese SHA, con los totales del GREEN de PB-T4e, y la evidencia local que fije el Owner (Q-PBT5-1).
- **El Smoke-PB:** 52 pasos en 10 bloques, más 8 precondiciones.
  - 46 los puede medir la sesión.
  - 6 necesitan, además, un juicio visual del Owner: en directo, o sobre las capturas si opera la sesión.
  - Los casos A/B son fabricados y van rotulados **CONSTRUCTED REPRESENTATIVE TEST CASES** (OB-09.1). Lo demás se hace sobre racks Push Back reales, en copias de trabajo (OB-09.2).
- **Quién opera:** la sesión, si el Smoke-PB está listo para empezar antes de las 13:00 UTC del 2026-11-23; si no, con el Owner. **La firma es siempre del Owner.**
- **El gate-close:** lo autoriza la firma del Owner, como en F4-E81. Cambian `I-64.yml`, [DISP], `I-64-evidence.md` y la evidencia nueva. No cambian `main`, HANDOFF, ROADMAP ni [G].
- **Preguntas al Owner:** seis y una información, en `owner-questions-v1.md`. Ninguna impide preparar. Cada una lleva el valor que se aplica si no hay respuesta.

---

## 1. Autoridades

| Autoridad | Qué fija para PB-T5 |
|---|---|
| [F] §5, tabla, fila PB-T5 | El alcance: «CI exacta, Smoke-PB (D-PB-09) y gate-close de F4B». Depende de PB-T4 |
| [F] §5, «Cada tarea pasa por la cadena de F4…» y §6.5 | La cadena y su adaptación cuando no hay Worker (la de PB-T0 para el bloque M) |
| [F] D-PB-09 (OB-09.1 y OB-09.2) | Los casos A/B fabricados y rotulados; los racks reales donde existen; las copias; los originales nunca se abren |
| [F] D-PB-10 (OB-10.1 y OB-10.2) | PB-T5 empieza en 1/3; el intento 2 lo decide la sesión; antes del 3 se vuelve al Owner |
| [F] §6.3 | Las reglas de operación por control de pantalla, la carga con NETLOAD y «Load Once», los inventarios de solo lectura, «No se finge una condición interna» |
| [F] §7.2 | Las reglas de F4 no se heredan en F4B sin orden expresa |
| [F] §8 | R-PB-03, R-PB-04, R-PB-06 y R-PB-11 remiten al Smoke-PB |
| [F] §9 | Los criterios de cierre de F4B |
| [DISP] F4B-E48, E51, E53, E55, E59 y E60 | Lo que cada tarea dejó al Smoke-PB (§4.5) |
| [DISP] F4B-E06, E08, E12, E09 y E23 | Las correcciones declaradas de RACKEDITAR; `pb-21-cola` fuera de la garantía; BOM y biblioteca en WORKSPACE |
| [DISP] F4B-E58 | La v3 del sustituto de nc2 vale de aquí en adelante, «PB-T5 incluida». PB-T5 no tiene controles de entrega, así que no la usa (§6.1) |
| [T4] §5.5 y §10 | La lista del Smoke-PB y lo que queda para PB-T5 |
| [S2] | El precedente: cómo se contrató, se operó, se firmó y se cerró un smoke de fase |
| [BM] | Cómo operó la sesión en F4B: NETLOAD con «Load Once», funciones LISP tecleadas, la regla N-I1 de Deshacer, una segunda instancia si el Owner tiene AutoCAD abierto |
| [G] §1 a §3, §6, §7, §7.1 y §7.2 | El entorno, el DLL, la carga, los criterios y el formato de evidencia. **Se sigue sin cambiarla** |
| `docs/WORKFLOW.md` §4 y §6; `AGENTS.md`, «Pruebas — definición de terminado» y «Reutilización de evidencia» | El DLL Debug del worktree; el Candidato y su evidencia; la reutilización solo por SHA exacto |
| Mensaje del Owner de esta noche, transmitido por la sesión principal (§5.4) | Quién opera el Smoke-PB esta noche |

---

## 2. Hechos de partida (medidos en `7da361eb`)

- **Las tareas de F4B:**
  - cerradas: PB-T0, PB-TC, PB-TC2, PB-T1a, PB-T1b, PB-T1c, PB-T2a, PB-T2b, PB-T2c, PB-T3a, PB-T3b, PB-T3c, PB-T4a, PB-T4b, PB-T4c y PB-T4d, todas CLOSED / PASS WITH LIMITATIONS (`I-64.yml`, `f4b_status`);
  - PB-T4e: emitida (F4B-E60), con el Worker en curso.
- **Los totales que se esperan en la CI del GREEN de PB-T4e** (F4B-E60): Core 19 421 = 19 018 + 0 + 403; UI 2 465 = 2 417 + 0 + 48. **[T4e]:** si el cierre de PB-T4e registra otros, mandan los del cierre.
- **El estado de la fase en `I-64.yml`:** `current_phase: F4B`, `state: implementing`, `gate: none`, `attempts: 1`.
- **Los dibujos reales usados en F4B:**
  - D1 (rol R006 de la selección de Smoke-2): un proyecto real con racks Push Back de un sentido. El bloque M encontró 14 sobres Push Back, todos sin `DimensionStyle`, y una referencia anidada de una planta dentro de un bloque huérfano;
  - D2 (R053): 8 definiciones Push Back sin referencias de nivel superior. No sirve para el panel, que necesita una vista insertada;
  - ningún dibujo real trae un rack A/B con referencias (R-PB-04; F4-E79).
- **El estilo de cotas:** 0 de 22 sobres Push Back reales guardan un `DimensionStyle`, y ningún camino de la interfaz lo escribe (F4B-E04, M-7).
- **El precedente de la carga:** NETLOAD con «Load Once», sin tocar SECURELOAD ni Trusted Locations. Smoke-2 lo ratificó: «"Load Once" fue la elección correcta» (F4-E79).
- **El diálogo de «Guardar en biblioteca»** (`src/RackCad.UI/UiSupport.cs`, `PromptSaveToLibrary`) es un `SaveFileDialog` estándar:
  - su carpeta inicial es la de la biblioteca de diseños;
  - se puede guardar en otra carpeta;
  - si la carpeta de la biblioteca no existe, la crea (best-effort).

---

## 3. El alcance de PB-T5

### 3.1 La CI exacta del Candidato

**Base:** [F] §9, «CI exacta del Candidato»; [T4] §10; [G] §2, §7.1 y §7.2; AGENTS, «definición de terminado».

- **El Candidato de F4B** es el SHA exacto de la punta desde la que se compila el DLL del Smoke-PB:
  - la punta tras el cierre de PB-T4e;
  - o el commit documental de emisión de PB-T5 encima de ella (§6.7).
- **El producto** es el último commit que toca `src/` o `tests/`: el GREEN de PB-T4e, o su reparación del RED si la hay **[T4e]**. `git diff <producto> <Candidato> -- src tests assets` tiene que salir vacío.
- **Por qué el Candidato es la punta y no el producto:** [G] §7.2 exige que la `InformationalVersion` del DLL identifique el Candidato, y AGENTS estampa el SHA del commit en el ensamblado.
  - El cierre de F4 citó como Candidato el producto (`9abc1320`) y compiló el DLL desde un commit documental encima (`9f894587`, `dll-c2.md`).
  - Aquí se registran los dos, y el Candidato es el del DLL.
- **La CI exacta:**
  - la corrida de CI sobre el SHA del Candidato, con `head_sha` igual a él;
  - los cuatro trabajos en `success`, incluido `UI Tests (WPF controls, net8.0-windows)` ([G] §7.1);
  - los totales iguales a los del GREEN de PB-T4e (§2).
  - Si el Candidato es un commit documental, se registra además la CI del producto, como en `dll-c2.md`.
- **La evidencia local:** [G] §7.1 pide Core Full y UI Full en local sobre el SHA exacto, después del commit y con el árbol limpio. En F4 hubo una dispensa para la UI (DV-05 / O-08, F4-E71), que [F] §7.2 no deja heredar. **Lo decide el Owner (Q-PBT5-1).** Sin respuesta: las dos suites en local.

### 3.2 El Smoke-PB

**Base:** [F] D-PB-09, §8 y §9; [T4] §5.5; [DISP] (§4.5).

La lista completa está en el §4, y cómo se ejecuta, en el §5.
- **Lo que cubre:** lo que solo se ve en AutoCAD:
  - lo que las tareas de PB-T3c a PB-T4e dejaron «sin AutoCAD»;
  - las correcciones declaradas de RACKEDITAR (§9 de [F]);
  - los casos A/B fabricados.
- **Lo que no es:**
  - no es Owner Validation, ni la validación total de [G] §4 («No se declara validación total si solo se ejecutó un smoke test»);
  - no vuelve a medir lo que ya fijó la evidencia automatizada de las tareas.

### 3.3 El gate-close de F4B

**Base:** [F] §9; el precedente F4-E81 (`F4-DRAFTS/gate-close.md` y el commit `5ccaa770`).

**Condiciones** ([F] §9, una a una):
1. todas las tareas cerradas: las 16 de §2 más PB-T4e **[T4e]**;
2. T1B en verde, y sustituida con registro solo para los cambios autorizados: todas las secciones de `F4B-DRAFTS/superseding-characterization.md` en EJECUTADA;
3. el Smoke-PB verifica en RACKEDITAR D-PB-06 y D-PB-08 (SPB-06 a SPB-08);
4. IC-06: hasta el cierre no se dice «editor de Push Back migrado»;
5. los INV del §1 de [F], cubiertos para Push Back completo, por la evidencia automatizada de las tareas (como en F4, «este cierre no la vuelve a ejecutar»);
6. la CI exacta del Candidato (§3.1);
7. el Smoke-PB PASS, con los casos fabricados rotulados. Con limitaciones aceptadas por el Owner, PASS WITH LIMITATIONS, como Smoke-2;
8. la firma del Owner;
9. sin integración parcial en `main`.

**Lo que cambia** (siguiendo `5ccaa770`):

| Archivo | Cambio |
|---|---|
| `docs/automation/state/I-64.yml` | `next_action`; `f4b_status` («CLOSED / PASS [WITH LIMITATIONS] (F4B-E(n); F4B-DRAFTS/gate-close.md): 17 tareas cerradas; Candidato F4B `<sha>` (CI `<run>` 4/4); Smoke-PB …; limitaciones …; no es Owner Validation; no integra en main»); en `persistent_next_action`, `NEXT_ROLE`, `NEXT_ACTION`, `GATE` y `TASK`, y `REMAINING_BUDGET` mientras dure PB-T5. **No cambian**, como en F4: `current_phase` (sigue `F4B` hasta que empiece F5-A), `state`, `gate`, `attempts` ni `last_evidence_commit` |
| [DISP] | Entradas nuevas, con numeración provisional F4B-E(n) en adelante, porque el cierre de PB-T4e tomará la siguiente **[T4e]**: (1) la emisión de PB-T5 y las respuestas del Owner; (2) la preparación y la CI exacta; (3) la corrida del Smoke-PB y su custodia; (4) la disposición y la firma del Owner; (5) el cierre del gate F4B |
| `docs/automation/evidence/I-64-evidence.md` | Secciones nuevas detrás de la de PB-T4e, una por entrada de [DISP], con la forma de §230 (el cierre de F4) |
| `I-64-pilot/F4B-DRAFTS/gate-close.md` (nuevo) | Con la forma de `F4-DRAFTS/gate-close.md`: el resultado; la autoridad (la firma); la secuencia de cierre; la tabla de las 17 tareas (producto, intentos y CI, tomados de cada `task-close-package.md`); la tabla del Smoke-PB por bloque; las limitaciones; lo siguiente |
| `I-64-pilot/F4B-SMOKE-PB/` (nuevo; nombre provisional) | `prep/` y `run/`, como `F4-SMOKE-2/` (§6.5) |
| `I-64-pilot/F4B-DRAFTS/coordinator/` | Los registros literales del Owner: las respuestas a `owner-questions-v1.md`, su disposición sobre el Smoke-PB y su firma |
| `I-64-pilot/F4B-DRAFTS/pbt5/` (nuevo) | Este paquete y sus versiones, su revisión independiente, los traspasos y la guía del operador redactada |
| `docs/ideas-futuras.md` | Solo lo que el Owner mande anotar en su disposición, como en F4-E80 |

**Lo que no cambia:**
- `src/`, `tests/` y `assets/`;
- [G];
- `docs/HANDOFF.md` y `docs/ROADMAP.md`: se tocan al integrar la iniciativa, no al cerrar una fase (WORKFLOW §4; F4-E81 no los tocó);
- `superseding-characterization.md`: PB-T5 no sustituye pruebas;
- `main`.

**La redacción del cierre:**
- el resultado: «F4B CLOSED / PASS WITH LIMITATIONS» o «CLOSED / PASS», según la firma;
- «No es Owner Validation y no integra en `main`»;
- desde ese momento ya se puede decir «editor de Push Back migrado» (IC-06 e IC-11), con sus limitaciones;
- lo siguiente: F5-A, que deja de esperar (la disposición del Owner de F4B, citada en [F] §0: «F5-A waits until F4B closes»), con su paquete y su contrato.

---

## 4. La lista del Smoke-PB

### 4.0 Convenciones

**Resultados** (los de Smoke-2: [S2] F4-E71, decisión D, y F4-E79):

| Resultado | Cuándo |
|---|---|
| **PASS** | Se cumple el criterio |
| **FAIL** | No se cumple. Se registra con su evidencia y no se corrige en PB-T5 (PBT5-C05) |
| **LIMITATION**, en una de tres formas | **NOT AVAILABLE FOR PHYSICAL SMOKE:** el caso no existe en los dibujos reales y no está autorizado fabricarlo.<br>**NO VERIFICABLE FÍSICAMENTE / CUBIERTO POR PRUEBA AUTOMATIZADA:** exige una condición interna que AutoCAD no expone.<br>**OBSERVADO:** el resultado no coincide con lo previsto y no está caracterizado. Se trae la evidencia antes de decidir, sin reinterpretarlo como FAIL ([S2], decisión 2) |
| **INFORMATIVO** | Una medida sin umbral (el coste: R-PB-06, «sin cambio en F4B»). Solo es FAIL si AutoCAD deja de responder o la orden no termina |

**Quién observa:**
- **S:** lo mide la sesión, con inventario, captura, hash u hora.
- **S+O:** lo mide la sesión, y además hace falta un juicio visual del Owner: en directo, o sobre las capturas si opera la sesión.

**Términos:**
- **Rack real:** un rack Push Back de un sentido de D1, en su copia de trabajo (por ejemplo, el que el bloque M llamó PB E, con planta, lateral y 72 frentes).
- **Copia:** una copia de trabajo nueva del original. Hay una por escenario cuya historia de Deshacer, de edición o de conflicto pueda contaminar otro escenario (la regla de [S2], decisión 3, adoptada aquí como regla de la sesión).
- **Gemela:** otra copia del mismo original, donde se hace la misma edición con RACKEDITAR para comparar con el clásico (la igualdad de PB-T3 con el clásico, [F] §6.4, fila 1).
- **Inventario** (`RCDUMP <etiqueta>`): las funciones de solo lectura de [BM] (`block-m/tools/lisp_src.txt`), tecleadas en cada documento.
  - Se amplían, también en solo lectura, para contar **todas** las referencias de una definición: de espacio modelo, de espacio papel y anidadas.
  - La ampliación va en la custodia y se revisa antes de usarla (§7, R-5).
- **N-I1** ([BM] §1): entre la orden medida y sus `U` no se teclea nada.
- **Los textos** entre comillas latinas son los literales que citan los registros. La guía del operador los copia de allí, nunca de memoria.

### 4.1 Precondiciones (la sesión, con AutoCAD cerrado)

| Id | Origen | Qué se hace | Criterio (si no se cumple, STOP) |
|---|---|---|---|
| P-01 | [F] §9; §3.1 | `git -C <worktree> status` y `rev-parse HEAD`; `git diff <producto> <Candidato> -- src tests assets`; la CI exacta del Candidato | Árbol limpio; HEAD = Candidato; diff vacío; la CI 4/4 con `ui-tests` en `success` y los totales de §2 |
| P-02 | [G] §7.1; Q-PBT5-1 | La evidencia local que fije el Owner. Por defecto, Core Full y UI Full con el `dotnet` que resuelve `global.json`. Antes de la UI se copia `RackCad/RackFrameConfigurator.layout` de los datos locales del usuario, y después se restaura byte a byte, porque las pruebas de UI lo sobrescriben (F4B-E30) | Las dos suites en verde, con los mismos totales que la CI; la disposición del configurador restaurada, con el mismo SHA-256 |
| P-03 | [G] §1, §2 y §7.2; WORKFLOW §6 | Sin `acad.exe` ni `accoreconsole.exe`, los comandos de compilación de [G] §2. El DLL que se carga es `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` | 0 errores y ningún `warning CS` (los `MSB3277` conocidos se aceptan); la `InformationalVersion` identifica el Candidato; se registran los SHA-256 de Plugin, Application, Domain y UI y la hora de escritura (la forma de `dll-c2.md`). Si un AutoCAD del Owner está abierto, **no se cierra**: STOP (PBT5-C08) |
| P-04 | [BM] P-2 | La carpeta `catalogs` junto al DLL frente a `assets/catalogs` del Candidato | Iguales byte a byte |
| P-05 | [S2] `run-log-2.md`, «Comprobaciones del día»; [G] §7.1 | Variables `RACKCAD_DEBUG_*` del usuario, de la máquina y del proceso; bundles de RackCad y entradas en `Applications`, `Loaded` y `AssemblyMap` del perfil de AutoCAD (lectura del registro, sin escribir); la ruta resuelta de la biblioteca de bloques y su SHA-256; el registro de RackCad del día (si existe y su tamaño); keep-awake activo | Ninguna variable; ningún bundle ni entrada de RackCad; la biblioteca registrada |
| P-06 | [F] OB-09.2 y §6.3, «Dibujos»; condición de la sesión principal | Una **copia de seguridad** de cada original que se vaya a usar, en `<carpeta-de-respaldo>`; después, las copias de trabajo con `copy` desde el original (nunca desde otra copia), con el manifiesto `copias.json` | Respaldo = original, por SHA-256; cada copia = original; el original, igual antes y después. **Los originales nunca se abren** |
| P-07 | [S2] F4-E71, precondición 7; P-08 de la sesión | Ningún Worker ni otra sesión trabajando en el worktree; la cadena de PB-T4e cerrada | Sin delegaciones abiertas |
| P-08 | OB-09.1 | La guía del operador, generada desde este §4, con los nombres reales en local y su versión redactada para la custodia. Los casos A/B, rotulados | La guía cita los textos de los registros; los A/B llevan el rótulo en la guía |

**«Listo para empezar»** = P-01 a P-08 cumplidas y PB-T5 emitido (§6.7).

### 4.2 Los pasos

#### Bloque 0. Apertura y carga

| Id | Origen | Qué se hace | Qué se observa | PASS / FAIL / LIMITATION | Observa |
|---|---|---|---|---|---|
| SPB-01 | [G] §3, paso 1; [BM] P-6 | Abrir AutoCAD 2025 con una copia, en una instancia nueva. Si hay un AutoCAD del Owner abierto, no se toca. Diálogos de arranque como en [BM]: «Proxy Information» → OK; referencias no encontradas → ignorar | El título con la copia; el PID; el `.dwl` junto a la copia | PASS si se abre la copia y no hay otra instancia tocada. FAIL si se abre un original | S |
| SPB-02 | [G] §3, pasos 2 y 3; [F] §6.3, «Carga» | `NETLOAD` del DLL exacto de P-03. Si sale el aviso de seguridad, «Load Once». Si `FILEDIA` es 0, la ruta se teclea en la línea de órdenes; la variable no se cambia | Los módulos de RackCad del proceso, cargados desde la carpeta de P-03 y con sus SHA-256 | PASS si coinciden. FAIL si se carga otro DLL. STOP si hiciera falta «Always Load» u otro cambio de configuración (PBT5-C03) | S |
| SPB-03 | [G] §3, pasos 4 y 5 | `RACKCAD` | El menú abre sin excepciones, con la ruta de la biblioteca de P-05 | PASS / FAIL | S |
| SPB-04 | [S2] pasos 0.6 y 0.7 | `RACKPANEL`; funciones de inventario; `RCDUMP pre`; captura de la lista de Deshacer; `DBMOD` y `PICKFIRST` leídos, no cambiados | «RackCad: panel Workspace mostrado.», sin errores; `DBMOD` = 0 en la copia recién abierta, o explicado como en [S2] 0.7 | PASS si el panel se muestra y la línea de base queda registrada | S |

#### Bloque A. RACKEDITAR, el modo CLASSIC y sus correcciones declaradas (racks reales)

**Origen común:** [F] §9, «Smoke-PB verifica en RACKEDITAR las correcciones D-PB-06 y D-PB-08»; OB-RE.1.

| Id | Origen | Qué se hace | Qué se observa | PASS / FAIL / LIMITATION | Observa |
|---|---|---|---|---|---|
| SPB-05 | OB-01.1b, OB-01.6 y OB-RE.1 | RACKEDITAR sobre un rack real | La ventana en CLASSIC: «Actualizar», «Insertar», los cuatro botones por vista e «Insertar varias…»; ningún «Aplicar al borrador» | PASS si está todo y sin rótulos de WORKSPACE. FAIL si falta algo o aparece «Aplicar al borrador» | S |
| SPB-06 | D-PB-06 (a); OD-PBTC-1; F4B-E06 y F4B-E08; para (b), la obligación de PB-T2 en WORKSPACE (F4B-E06) | (a) Copia nueva. RACKEDITAR → el fondo de la tarima con un valor de prueba de 4 decimales que no se conserva con 3 (por ejemplo, 48.0004) → «Actualizar» → `RCDUMP a1` → RACKEDITAR otra vez, **sin tocar nada** → «Actualizar» → `RCDUMP a2`.<br>(b) En la misma copia: «Editar» → «Editar todo» → cambiar otro campo (la escala) → «Aplicar al borrador» → «Actualizar» del panel → `RCDUMP a3` | El campo en el sobre, en `a1`, `a2` y `a3` | PASS si `a2` y `a3` conservan el valor exacto. FAIL si sale redondeado a 3 decimales. El valor es de prueba sobre un rack real, y se declara así | S |
| SPB-07 | D-PB-06 (b); OD-PBTC-2; F4B-E04, M-7; Q-PBT5-2 | Recuento, con el inventario, de los sobres Push Back con `DimensionStyle` no nulo en las copias de los dibujos reales. Si hubiera alguno: RACKEDITAR → «Actualizar» → el estilo de sus cotas y una captura | El recuento | Si hay alguno: PASS si las cotas salen con el estilo guardado. Si no hay ninguno (lo medido: 0 de 22): **NOT AVAILABLE FOR PHYSICAL SMOKE**, cubierto por las pruebas de PB-TC. No se fabrica sin respuesta del Owner (Q-PBT5-2) | S |
| SPB-08 | D-PB-08 (OB-08.1 a OB-08.3); OD-PBTC-3 | RACKEDITAR sobre un rack real de un sentido → marcar «Rack de dos sentidos (lado B)» sin activar ningún frente de B → «Actualizar» → `RCDUMP` → abrir RACKEDITAR otra vez | (a) En la franja de estado, el literal: «Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.»; (b) el aviso no bloquea: «Actualizar» sigue disponible; (c) el lado B no se guarda en el sobre; (d) al reabrir, la casilla puede salir desmarcada (comportamiento caracterizado) | PASS si (a) es igual byte a byte, (b) y (c) se cumplen. FAIL si falta el texto, si bloquea o si se guarda un lado B sin frentes. **O:** si el aviso hace entender la consecuencia («the UI should make this consequence understandable») | S+O |
| SPB-09 | D-PB-11 (H-PB-28); OD-PBTC-4; F4B-E08; Q-PBT5-3 | Copia nueva. RACKEDITAR → altura de cabecera distinta de la estándar → «Actualizar» → `RCDUMP r1` → RACKEDITAR → «Restaurar estándar» → «Actualizar» → `RCDUMP r2` | La altura de cabecera, el poste derivado y los separadores en `r2` | PASS si `r2` tiene los valores estándar. FAIL si vuelven los de `r1` | S |
| SPB-10 | H-31 (OD-PBTC-5, PB-TC2); F4B-E09 y F4B-E12; Q-PBT5-3 | Copia nueva. RACKEDITAR → la longitud de un módulo con un valor de prueba de 4 decimales → «Actualizar» → RACKEDITAR → pasar por la caja (foco dentro y fuera) sin escribir → «Actualizar» → `RCDUMP` | El valor del módulo en el sobre | PASS si se conserva exacto. FAIL si sale redondeado | S |

#### Bloque B. «Editar todo»: la ventana, el modal y cero escrituras (rack real)

| Id | Origen | Qué se hace | Qué se observa | PASS / FAIL / LIMITATION | Observa |
|---|---|---|---|---|---|
| SPB-11 | [T4] §5.5 («el botón abre la ventana modal real»; «el abridor cableado»); OB-01.6; F4B-E23 | Seleccionar el rack → «Editar» → «Listo para editar» → «Editar todo» | La ventana de Push Back, en WORKSPACE, abierta desde la paleta: «Aplicar al borrador», con su tooltip; sin «Insertar», sin los cuatro botones por vista y sin «Insertar varias…»; con «BOM» y «Guardar en biblioteca». Que no tenga dueño no se ve: se registra NO OBSERVABLE, cubierto por las pruebas de PB-T4c | PASS / FAIL | S |
| SPB-12 | [T4] §5.5 («K = 1 con el modal real»); F4B-E55 | Con la ventana abierta, un clic en otra pestaña o botón de la paleta. Captura antes y después | La paleta, deshabilitada, no responde. Al cerrar la ventana, responde | PASS / FAIL | S |
| SPB-13 | OB-01.4; [T4] §5.5 | Con la ventana abierta, intentar teclear una orden en AutoCAD y hacer clic en el dibujo | Nada llega al dibujo ni a la línea de órdenes | PASS / FAIL | S |
| SPB-14 | F4B-E55 («el foco y el teclado») | Con la ventana abierta: escribir en una caja, Tab, Retroceso y Supr | Las teclas van a la ventana; nada aparece en la línea de órdenes; sin PASTECLIP | PASS / FAIL | S |
| SPB-15 | [T4] §5.5 («nada se escribe tras cerrarla»; paso 7 de PB-T4e); F4B-E55 («los eventos del puente durante el modal») | `RCDUMP` antes → «Editar todo» → «Cerrar» sin cambiar nada → `RCDUMP` después; `DBMOD`; la lista de Deshacer; el estado del campo a los 5 s | «Editor cerrado sin cambios en el borrador.»; «Limpio»; los dos inventarios iguales; `DBMOD` y la lista de Deshacer sin entrada nueva de RackCad; a los 5 s, sin «(posiblemente obsoleto)» | PASS / FAIL | S |
| SPB-16 | D-PB-08 en WORKSPACE (F4B-E06: la obligación de PB-T2) | En la ventana de «Editar todo», marcar el lado B sin activar ningún frente | El mismo literal de SPB-08 en la franja de estado, sin bloquear | PASS / FAIL | S |
| SPB-17 | D-PB-07 (F4B-E23); OWNER-03 de PB-T4c (F4B-E55) | Copia nueva. En la ventana de «Editar todo», cambiar un valor **sin aplicar** → «BOM» → exportar a `<carpeta-de-salidas>` → «Guardar en biblioteca» en `<carpeta-de-salidas>`, con un nombre nuevo → «Cerrar» sin aplicar → `RCDUMP`. Después, en otra pasada: «Guardar en biblioteca» → «Aplicar al borrador» → «Actualizar» → `RCDUMP` | Los dos archivos, solo en `<carpeta-de-salidas>`; el dibujo sin cambios tras la primera pasada; el GUID del borrador, igual en las dos; si el `SetName` de la biblioteca llega al borrador con «Aplicar al borrador» | PASS si nada sale de `<carpeta-de-salidas>`, el DWG no cambia sin «Actualizar» y el GUID no cambia. Que el BOM o el archivo reflejen valores sin aplicar está aceptado (F4B-E23). Lo del `SetName` se registra como hecho. Si el diálogo no deja elegir la carpeta: STOP de este paso (PBT5-C06) y LIMITATION | S |

#### Bloque C. «Editar todo» → «Actualizar» y los caminos de `BuildCore` (racks reales)

| Id | Origen | Qué se hace | Qué se observa | PASS / FAIL / LIMITATION | Observa |
|---|---|---|---|---|---|
| SPB-18 | [T4] §5.5 («A2-11 hasta el DWG»; «el borrado físico»); F4B-E48; F4B-E53 («Actualizar»; el borrado de vistas reales); F4B-E55 («Actualizar» tras editar; «Modificado» tras cerrar); F4B-E59, OWNER-02, camino 1 («un plan que borra un lateral obsoleto»); R-PB-03 | Copia nueva y gemela. **Preparación** en las dos, con el clásico: RACKEDITAR → insertar el corte lateral del último poste ([BM] M-6a). **Panel:** «Editar» → «Editar todo» → un frente menos y la escala a 2.5 (la receta de CT-PBT4E-04) → «Aplicar al borrador» → «Cerrar» → «Actualizar» → `RCDUMP post`. **Gemela:** la misma edición con RACKEDITAR → «Actualizar» → `RCDUMP post` | (a) Tras «Cerrar», el resumen y «Modificado». (b) Tras «Actualizar»: «Actualizado: el cambio se aplicó al dibujo.», «Limpio». (c) La definición y la referencia del lateral obsoleto, borradas; la planta y el lateral que queda, redibujados con un frente menos y la escala 2.5; ninguna vista nueva (H-PB-16). (d) Igual que la gemela: el mismo conjunto de definiciones y los mismos sobres, salvo los campos que estén declarados. (e) El nombre, los niveles y las cotas en las vistas | PASS si (a) a (d) se cumplen. FAIL si no se borra el lateral, si se crea una vista o si el resultado difiere del clásico sin declaración. **O:** (e), si el redibujo se ve correcto, como el paso 14 de Smoke-2 | S+O |
| SPB-19 | [BM] M-5 y N-I1; [S2] S2-03 | Copia nueva. Lo mismo que el panel de SPB-18 hasta «Actualizar» → `U`, sin nada en medio (eco literal) → `RCDUMP u1` → `REDO` | Un solo `U` deshace el «Actualizar» entero, también el borrado; la vista se refresca sola; `REDO` lo rehace | PASS si `u1` = el estado previo y `REDO` lo rehace. FAIL si hace falta más de un `U` o la vista no se refresca | S |
| SPB-20 | F4B-E59, OWNER-02, camino 2 («un borrador con el nombre en blanco») | Copia nueva. «Editar todo» → vaciar el nombre → «Aplicar al borrador» → «Actualizar» → `RCDUMP` | El rótulo del nombre en las vistas y `Name` en cada sobre | PASS si todas las vistas llevan el nombre del sobre de origen. FAIL si alguna queda sin nombre o con otro | S |
| SPB-21 | A-T2b-OWNER-01; D-PBT4-17; OWNER-04 de PB-T4c (F4B-E55) | En la ventana de «Editar todo»: escenificar un módulo (abrir su configurador desde la ventana y aceptar) **y** dejar otro cambio sin aplicar → «Cerrar» | Una sola pregunta, con el aviso real; lo que deja cada respuesta | PASS si es una sola pregunta y la respuesta hace lo que dice. Si el gesto de escenificar no se alcanza desde la interfaz: NOT AVAILABLE FOR PHYSICAL SMOKE | S |
| SPB-22 | M-04 de la revisión independiente de PB-T4c; OWNER-04 de PB-T4c | Ninguno: el aviso «No se abrió el editor: <mensaje>.» solo sale si el editor lanza una excepción después de aplicar | — | **NO VERIFICABLE FÍSICAMENTE** (no se provoca una excepción: [F] §6.3). El defecto del texto ya está en `docs/ideas-futuras.md` | S |
| SPB-23 | R-PB-06; [T4] §5.5 («el coste») | En el rack real más grande disponible: la duración del «Actualizar» del panel (SPB-18) y la de RACKEDITAR en la gemela, del clic al «Limpio» o al final de la orden, por la hora de las capturas | Los segundos, con la resolución de la medida | INFORMATIVO. **O:** si la espera se percibe aceptable | S+O |

#### Bloque D. El conflicto y «Conservar mi borrador» (racks reales)

**Origen común:** D-PB-05, pasos 1 a 4 (OB-05.3 a OB-05.5); D-PBT4-08 y D-PBT4-09; [T4] §5.5 («el paso 3 con un cambio real del dibujo entre la presentación y el "Sí"»); F4B-E53.

| Id | Origen | Qué se hace | Qué se observa | PASS / FAIL / LIMITATION | Observa |
|---|---|---|---|---|---|
| SPB-24 | El común; R-T4-39 (F4B-E60, información) | Copia nueva, con el lateral del último poste insertado. «Editar todo» → un frente menos (el del lateral) → «Aplicar al borrador» → «Cerrar» (Dirty) → un cambio de fuera del rack. La guía fija el gesto: RACKEDITAR, o RACKPROYECTAR como en el paso 34 de Smoke-2 → «Actualizar» del panel | El texto literal de la respuesta; si se ofrece «Conservar mi borrador» | PASS si no se escribe nada sin avisar y el texto es el de la rama que toca. Que salga `NotAppliedPreflight` y no «Conservar mi borrador» está previsto (R-T4-39): se registra. Si ningún gesto disponible lleva a «Conservar mi borrador», SPB-25 a SPB-28 quedan NOT AVAILABLE y se informa | S |
| SPB-25 | El común (paso 2); OB-05.3 | «Conservar mi borrador» → el primer «Sí» | Encima del Sí/No, las líneas «Por grupos», con los literales de D-PBT4-09: «Cambiado fuera: …», «Cambiado por ti: …» y, si tocan, «Cambiado en los dos: …», «Cambios estructurales: …» y «Vistas: se borrará 1 lateral obsoleto.» | PASS si salen las líneas que corresponden, con su literal. FAIL si conserva sin presentar. **O:** si basta para una elección informada (D-PB-05) | S+O |
| SPB-26 | OWNER-04 de PB-T4b (F4B-E53) | El segundo «Sí», sin cambiar nada en el dibujo → «Actualizar» → `RCDUMP` | «Conservar» conserva a la primera y no vuelve a presentar; «Actualizar» aplica | PASS / FAIL (FAIL si entra en un bucle de presentaciones) | S |
| SPB-27 | El común, el paso 3 con un cambio real | Otra copia, hasta la presentación de SPB-25 → un cambio de fuera en otro campo → el segundo «Sí» | Vuelve a presentar, con las líneas nuevas, y no conserva | PASS / FAIL | S |
| SPB-28 | OWNER-01 de PB-T4b (F4B-E53: «Es lo esperado») | Copia nueva. Borrador con un cambio propio → renombrado de fuera con RACKEDITAR → «Conservar mi borrador» → los dos «Sí» → «Actualizar» → `RCDUMP` | La presentación dice «Cambiado fuera: el nombre.»; tras «Actualizar», el dibujo lleva el nombre del borrador | PASS si se cumple lo decidido por el Owner. FAIL si el nombre es otro | S |

#### Bloque E. La recarga de «Editar todo» (PB-T4e)

**Origen común:** [T4] §5.5, «Desde la v5.5», pasos 1 a 8; F4B-E60. Es una copia de un dibujo con un rack Push Back real (OB-09.2). Los criterios son los de [T4], literales.

| Id | Paso de [T4] | Qué se hace | Qué se observa | PASS / FAIL / LIMITATION | Observa |
|---|---|---|---|---|---|
| SPB-29 | 1 | Fijar la pestaña del rack y «Editar»; no cambiar nada | «Listo para editar» | PASS / FAIL | S |
| SPB-30 | 2 | Con el panel visible, cambiar fuera un valor que la ventana completa enseña (RACKEDITAR) y aceptar. Captura | «Limpio (posiblemente obsoleto)» y la caja en solo lectura | PASS / FAIL | S |
| SPB-31 | 3 | «Editar todo» → cerrar sin cambios | La ventana abre con el valor nuevo del paso 2, sin aviso; al cerrar, «Editor cerrado sin cambios en el borrador.», «Limpio» sin «(posiblemente obsoleto)» y la caja editable | PASS / FAIL (sin PB-T4e abría con el valor viejo) | S |
| SPB-32 | 4 | Repetir 1 y 2 → «Editar todo» → cambiar algo → «Aplicar al borrador» → cerrar («Modificado») → «Actualizar» → `RCDUMP` | Se aplica sin «No se aplicó: el dibujo cambió fuera del panel» ni «Conservar mi borrador»; el dibujo conserva el cambio del paso 2 y el del editor | PASS / FAIL | S |
| SPB-33 | 5 | Borrador Clean → borrar fuera todas las vistas del rack → «Editar todo» → después, Deshacer | No abre; el campo muestra «No editable aquí: el origen ya no está en el dibujo», la pista de RACKEDITAR y «No se recargó: el borrador se conserva tal como estaba.»; Deshacer restaura el dibujo | PASS / FAIL | S |
| SPB-34 | 6 (y R-T4-39) | Copia nueva y gemela. Borrador con un cambio propio → un cambio de fuera → «Editar todo» → «Actualizar». En la gemela, la misma receta con «Editar» | Abre sobre la base de antes, como «Editar»; el resultado de «Actualizar», literal, igual en las dos copias | PASS si los dos caminos coinciden. Si sale `NotAppliedPreflight` y no «Conservar mi borrador», es lo que F4B-E60 manda contar al Owner, como información | S |
| SPB-35 | 7 | «Editar todo» → cerrar sin «Actualizar»; `(getvar "DBMOD")` y la lista de Deshacer, antes y después | Ningún cambio en el dibujo | PASS / FAIL | S |
| SPB-36 | 8; F4B-E51 y F4B-E53 (el coste real de la lectura) | Con la ventana abierta, la paleta (K = 1); al cerrar, el panel responde. En el dibujo más grande: el tiempo del clic a la ventana, con la marca de obsoleto y sin ella | La paleta deshabilitada; los dos tiempos | PASS en la parte del modal; INFORMATIVO en el tiempo. **O:** si la espera se percibe aceptable | S+O |

#### Bloque F. A/B: **CONSTRUCTED REPRESENTATIVE TEST CASES**

**Origen común:** D-PB-09 (OB-09.1) y R-PB-04. Cada copia, captura, inventario y línea del registro de este bloque lleva el rótulo **CONSTRUCTED REPRESENTATIVE TEST CASES**, no «real production examples».

| Id | Origen | Qué se hace | Qué se observa | PASS / FAIL / LIMITATION | Observa |
|---|---|---|---|---|---|
| SPB-37 | OB-09.1; el método de [BM] M-4 | **Fabricación**, en una copia nueva de D1 y con el clásico: RACKEDITAR sobre un rack real de un sentido → marcar «Rack de dos sentidos (lado B)» → «Lado activo: Lado B» → quitar «En blanco» a un frente de B → «Actualizar» → `RCDUMP ab1` → RACKEDITAR → insertar el frontal entrada/salida del lado B → `RCDUMP ab2`. Se hacen tantas copias fabricadas como pidan SPB-38 a SPB-42, cada una desde el original | En `ab1`, el lado B presente; en `ab2`, el frontal B (vista frontal, sección 2) | PASS si el caso queda fabricado y rotulado. Si no se puede fabricar: STOP del bloque y se informa | S |
| SPB-38 | OB-03.2; D-PBT4-05; F4B-E53 | Seleccionar el rack fabricado → «Editar» | Abre su borrador («Listo para editar»), sin «fuera del piloto (F5)» | PASS / FAIL | S |
| SPB-39 | OB-03.2; [T4] §5.5 («la lectura completa en AutoCAD»); OB-04.2 | Copia fabricada y gemela fabricada. «Editar todo» → activar un segundo frente de B → «Aplicar al borrador» → «Actualizar» → `RCDUMP`. En la gemela, lo mismo con RACKEDITAR | El lado B con dos frentes activos; las vistas redibujadas; ninguna vista nueva; igual que la gemela salvo lo declarado | PASS / FAIL. **O:** si las vistas A/B se ven correctas | S+O |
| SPB-40 | F4B-E59, OWNER-02, camino 3 («el rechazo de la frontal B de un rack sin lado B»); R-PB-11; [F] §6.4, fila 3; H-PB-18 y H-PB-22 | Copia fabricada con el frontal B y gemela. «Editar todo» → quitar el lado B → «Aplicar al borrador» → «Actualizar» → `RCDUMP`. En la gemela, lo mismo con RACKEDITAR → `RCDUMP` | Panel: el rechazo, que remite a RACKEDITAR, y el dibujo sin cambios. Clásico: el frontal B redibujado como frontal de un sentido, renombrado y con `Section` 2 (lo medido en [BM]) | PASS si el panel rechaza sin escribir nada (inventario igual) y el clásico hace lo medido. FAIL si el panel escribe algo | S |
| SPB-41 | OWNER-01 de PB-T1c («Restaurar» no recalcula antes de leer el diseño; L-PBT2-13); [T4] §5.5 («"Restaurar"») | Copia fabricada y gemela. En las dos, el mismo gesto (por ejemplo, el hueco entre lados) → «Restaurar» → aplicar y «Actualizar» en el panel; «Actualizar» en RACKEDITAR | Los dos inventarios | PASS si los dos modos dan lo mismo. Si se ve el desfase de una pasada, OBSERVADO: es una limitación conocida e igual en la base, y CLASSIC no cambia (D-16). FAIL solo si los modos difieren | S |
| SPB-42 | H-31 (el hueco entre lados y las cuatro cajas avanzadas anulables) y OWNER-1 de PB-TC2 (la caja mixta de «Ambos lados»); F4B-E09 y F4B-E12; Q-PBT5-3 | Copia fabricada. (a) RACKEDITAR → el hueco entre lados con un valor de prueba de 4 decimales → «Actualizar» → RACKEDITAR → pasar por la caja sin escribir → «Actualizar» → `RCDUMP`. (b) Un campo de celda con valores exactos distintos en A y en B → «Ambos lados» (la caja sale mixta) → escribir el número que se ve → «Actualizar» → `RCDUMP` | (a) El valor exacto conservado. (b) Los dos lados con el mismo valor | PASS / FAIL | S |
| SPB-43 | F4B-E09, respuesta 3 (`pb-21-cola` fuera de la garantía); D-PBT4-05 | Ninguno: no hay rack real de un sentido con cola compuesta guardada (F4B-E09), y D-PB-09 solo autoriza fabricar racks A/B | — | **NOT AVAILABLE FOR PHYSICAL SMOKE**. El rechazo lo cubre la prueba de D-PBT4-05 | S |

#### Bloque G. El borrado real y la relectura con el origen

| Id | Origen | Qué se hace | Qué se observa | PASS / FAIL / LIMITATION | Observa |
|---|---|---|---|---|---|
| SPB-44 | [T4] §5.5 («la relectura con el origen tras borrar la vista de origen», A-T3-8 con POST reanclado, y su coste); F4B-E51 (OWNER-01: el origen deriva al `Primary`; OWNER-03: dos subcasos sin CT) | Copia nueva, con el lateral del último poste insertado. Seleccionar **ese lateral** → «Editar» (el lateral queda como origen) → «Editar todo» → un frente menos → «Aplicar al borrador» → «Actualizar» → «Comprobar ahora» → «Editar» | Se aplica y borra el lateral de origen; POST se reancla a una vista que queda; la relectura posterior funciona; a qué vista va el origen después; el tiempo | PASS si se aplica y la relectura sigue. OBSERVADO si el origen cae en el `Primary` (OWNER-01, conocido), o si sale «No se aplicó: Ningún sobre editable de la relectura coincide con el origen del borrador.»: en F4 es el diseño declarado tras perder el origen ([S2], disposición B), pero con el POST reanclado de A-T3-8 no se espera. El tiempo, INFORMATIVO | S |
| SPB-45 | OWNER-1 de PB-T3c (F4B-E48: «las referencias de la vista en espacio papel o dentro de bloques del usuario se borran sin aviso»); Q-PBT5-4 | Con la respuesta A de Q-PBT5-4: copia nueva y gemela, con el lateral del último poste insertado y una segunda referencia de esa definición en una presentación, con `-INSERT` tecleado → panel: un frente menos → «Actualizar» → `RCDUMP`. En la gemela, RACKEDITAR | Las dos referencias, la de modelo y la de papel, borradas con la definición; si hay aviso; igual en la gemela (MA H-063) | PASS si el panel hace lo mismo que el clásico. La falta de aviso se registra OBSERVADO, como riesgo ya informado. Sin respuesta A: NOT AVAILABLE FOR PHYSICAL SMOKE, y el paso queda como adenda | S |
| SPB-46 | OWNER-2 de PB-T3c («la purga de las anidadas es de un nivel»); [BM] §3, hecho 2; Q-PBT5-4 | Con la respuesta A: copia nueva y gemela, con el lateral insertado y metido en un bloque del usuario (`-BLOCK` tecleado) → panel: un frente menos → «Actualizar» → `RCDUMP`. En la gemela, RACKEDITAR | Qué queda del bloque del usuario y de las definiciones anidadas | Como SPB-45 | S |
| SPB-47 | OWNER-3 de PB-T3c («los bloques dinámicos») | Solo si un dibujo real trae una vista de rack dentro de un bloque dinámico del usuario: lo mismo que SPB-46 | — | Si no lo trae: NOT AVAILABLE FOR PHYSICAL SMOKE. Fabricar un bloque dinámico no está autorizado | S |
| SPB-48 | OWNER-2 de PB-T3b (AUTH-13: un sobre con `SchemaVersion` 1.1 o un rack con sobres de nombres distintos no se admiten); [T4] §5.5 («OWNER-2») | Solo si un dibujo real trae alguno de los dos casos: seleccionarlo → «Editar» | El rechazo, con su diagnóstico y la pista del editor clásico | Si no hay ninguno: NOT AVAILABLE FOR PHYSICAL SMOKE | S |

#### Bloque H. La regla única cableada

| Id | Origen | Qué se hace | Qué se observa | PASS / FAIL / LIMITATION | Observa |
|---|---|---|---|---|---|
| SPB-49 | [T4] §5.5 («los mensajes del campo del panel y de la caja son iguales en AutoCAD»); D-PBT4-06 | Los mismos valores no válidos de la escala (por ejemplo, «abc», «0» y «-1») en el campo del panel y en la caja de la ventana de «Editar todo» | Los mensajes, por captura ampliada | PASS si son iguales letra a letra. FAIL si difieren | S |

#### Bloque Z. Cierre de la corrida

| Id | Origen | Qué se hace | Qué se observa | PASS / FAIL / LIMITATION | Observa |
|---|---|---|---|---|---|
| SPB-50 | [F] §6.3, «Dibujos»; [S2] R7 | Cerrar cada copia sin guardar («No»). Cerrar la instancia que abrió la sesión. La del Owner, si la hay, no se toca | Ninguna copia guardada; ni `acad.exe` ni `accoreconsole.exe` de la sesión | PASS / FAIL | S |
| SPB-51 | [S2] `integrity.md`; [BM], «Al cerrar» | SHA-256 de los originales, de los respaldos y de las copias frente a `copias.json`; autoguardados de las copias; `git -C <worktree> status` y HEAD | Originales y respaldos iguales; copias iguales a su original; los autoguardados, registrados; el árbol limpio y HEAD = Candidato | PASS / FAIL (FAIL si cambió un original) | S |
| SPB-52 | [S2] F4-E79, «Seguridad, ratificada» | `RACKCAD_DEBUG_*`; líneas `ERROR` del registro de RackCad del día; eventos «.NET Runtime» y «Application Error» de acad durante la corrida; `Applications`, `Loaded` y `AssemblyMap` del perfil; la disposición del configurador y `settings.json` | Todo igual que en P-05, salvo lo explicado | PASS si no queda rastro de configuración ni errores sin explicar | S |

### 4.3 Recuento

| Bloque | Pasos | Solo S | S+O |
|---|---|---|---|
| 0. Apertura y carga | 4 | 4 | 0 |
| A. RACKEDITAR (CLASSIC) | 6 | 5 | 1 (SPB-08) |
| B. «Editar todo», la ventana | 7 | 7 | 0 |
| C. «Actualizar» y `BuildCore` | 6 | 4 | 2 (SPB-18 y SPB-23) |
| D. Conflicto y «Conservar» | 5 | 4 | 1 (SPB-25) |
| E. La recarga (PB-T4e) | 8 | 7 | 1 (SPB-36) |
| F. A/B fabricados | 7 | 6 | 1 (SPB-39) |
| G. Borrado real y relectura | 5 | 5 | 0 |
| H. La regla única | 1 | 1 | 0 |
| Z. Cierre | 3 | 3 | 0 |
| **Total** | **52** | **46** | **6** |

**Además:**
- 8 precondiciones, P-01 a P-08, todas de la sesión.
- Los resultados ya previstos como LIMITATION son tres o más, según los dibujos y las respuestas del Owner: SPB-22 y SPB-43; SPB-07 si no hay estilo guardado (lo medido); SPB-47 y SPB-48 si no hay caso real; SPB-45 y SPB-46 sin la respuesta A de Q-PBT5-4.
- Dependen de una respuesta: SPB-09, SPB-10 y SPB-42 (Q-PBT5-3); SPB-45 y SPB-46 (Q-PBT5-4); SPB-07 si el Owner pidiera fabricar (Q-PBT5-2).

**Los seis S+O:** la sesión mide en los seis, y el Owner juzga lo visual:
- SPB-08: si el aviso del lado B se entiende;
- SPB-18: si el redibujo se ve correcto (nombre, niveles y cotas);
- SPB-23 y SPB-36: si la espera es aceptable;
- SPB-25: si la presentación basta para elegir;
- SPB-39: si las vistas A/B se ven correctas.

### 4.4 Lo que el Smoke-PB no cubre, y por qué

- **Guardar la copia y reabrirla** (el «round-trip tras reabrir DWG» de [G] §7): ningún registro de F4B lo pide para el Smoke-PB, y PB-T0 cerró sin guardar. Si el Owner lo quiere, es un paso nuevo.
- **La carrera de cierre a mitad de transacción y las inyecciones UNIT y SEAL:** son de F4 y están en su evidencia. F4B no las cambia.
- **El nombre interno del grupo de Deshacer y el número de transacciones:** NO OBSERVABLE ([BM] §2).

### 4.5 Trazabilidad: cada registro y su paso

| Lo que se dejó al Smoke-PB | Dónde | Paso |
|---|---|---|
| A/B fabricados y rotulados; racks reales en lo demás | D-PB-09 (OB-09.1 y OB-09.2) | Bloque F; bloques A a E, G y H |
| D-PB-06 y D-PB-08 en RACKEDITAR | [F] §9 | SPB-06 a SPB-08 |
| D-PB-06, paso 4, y el aviso de D-PB-08, en WORKSPACE | F4B-E06 (obligaciones de PB-T2) | SPB-06 (b) y SPB-16 |
| D-PB-11, H-31 y OWNER-1 de PB-TC2 | F4B-E08 y F4B-E12 | SPB-09, SPB-10 y SPB-42 (Q-PBT5-3) |
| R-PB-03 (el borrado en la transacción única) | [F] §8 | SPB-18, SPB-19 y SPB-45 a SPB-47 |
| R-PB-04 (no hay A/B reales) | [F] §8 | Bloque F |
| R-PB-06 (el coste del Actualizar) | [F] §8 | SPB-23 |
| R-PB-11 (la paridad del frontal B) | [F] §8 | SPB-40 |
| El botón y la ventana modal real; el abridor cableado | [T4] §5.5 | SPB-11 y SPB-15 |
| El dibujo no se edita con la ventana abierta (OB-01.4) | [T4] §5.5 | SPB-13 |
| El borrado físico de PB-T3c | [T4] §5.5; F4B-E48 | SPB-18 y SPB-19 |
| A2-11 hasta el DWG | [T4] §5.5 | SPB-18 |
| El coste | [T4] §5.5 | SPB-23, SPB-36 y SPB-44 |
| «Restaurar» | [T4] §5.5 | SPB-41 |
| OWNER-2 de PB-T3b | [T4] §5.5 | SPB-48 |
| La lectura completa en AutoCAD: un A/B se abre en el panel (OB-03.2) | [T4] §5.5; F4B-E53 | SPB-38 y SPB-39 |
| La relectura con el origen tras borrar la vista de origen, y su coste | [T4] §5.5; F4B-E51 | SPB-44 |
| La regla única cableada | [T4] §5.5 | SPB-49 |
| El paso 3 con un cambio real del dibujo | [T4] §5.5 | SPB-27 |
| K = 1 con el modal real | [T4] §5.5; F4B-E55 | SPB-12 y SPB-36 |
| OWNER-1 a OWNER-3 de PB-T3c | [T4] §5.5; F4B-E48 | SPB-45 a SPB-47 |
| La recarga de «Editar todo», pasos 1 a 8 | [T4] §5.5; F4B-E60 | SPB-29 a SPB-36 |
| R-T4-39, como información junto al paso 6 | F4B-E60 | SPB-24 y SPB-34; la información de `owner-questions-v1.md` |
| El origen que deriva al `Primary` (OWNER-01) y los subcasos sin CT (OWNER-03) de PB-T4a | F4B-E51 | SPB-44 |
| El nombre tras «Conservar» y «Actualizar» (OWNER-01 de PB-T4b) | F4B-E53 | SPB-28 |
| El segundo «Sí» sin cambios (OWNER-04 de PB-T4b) | F4B-E53 | SPB-26 |
| «Actualizar», «Conservar» y el borrado de vistas reales | F4B-E53 | SPB-18 y SPB-24 a SPB-28 |
| «Editar todo» sobre un borrador Clean posiblemente obsoleto (OWNER-02 de PB-T4c) | F4B-E55; sustituido por PB-T4e ([T4] §5.5) | SPB-29 a SPB-36 |
| BOM y «Guardar en biblioteca» desde «Editar todo» (OWNER-03 de PB-T4c) | F4B-E55; F4B-E23 | SPB-17 |
| La pregunta combinada de «Cerrar», «Modificado» y el aviso de M-04 (OWNER-04 de PB-T4c) | F4B-E55 | SPB-21, SPB-18 (a) y SPB-22 |
| El modal con la paleta deshabilitada, el foco y el teclado, «Actualizar» tras editar y los eventos del puente | F4B-E55 | SPB-12, SPB-14, SPB-18 y SPB-15 |
| Los tres caminos de `BuildCore`, junto a la pasada de «Editar todo» → «Actualizar» | F4B-E59, OWNER-02 | SPB-18, SPB-20 y SPB-40 |
| `pb-21-cola` fuera de la garantía | F4B-E09 | SPB-43 |
| La CI exacta del Candidato | [F] §9; [T4] §10 | P-01 |
| Los rótulos del modo CLASSIC sin cambios | OB-01.6; OB-RE.1 | SPB-05 |

---

## 5. Cómo se ejecuta

### 5.1 Las condiciones de los dibujos

- **Solo copias de trabajo.** Los originales nunca se abren, ni en AutoCAD ni con `accoreconsole` ([F] OB-09.2 y §6.3).
- **Antes de usar un original importante** (D1 es un proyecto real de un cliente), se hace una copia de seguridad en `<carpeta-de-respaldo>`, y se comprueba que su SHA-256 es igual (P-06).
- Cada copia sale de su original, nunca de otra copia.
- Nada se guarda sobre un original. Las copias se cierran sin guardar (SPB-50).
- **Las salidas a disco** (BOM y biblioteca, SPB-17) van solo a `<carpeta-de-salidas>`. Nunca se sobrescribe un archivo que ya existía.
- **La privacidad:** los dibujos son de clientes del Owner. Sus nombres, sus rutas y los JSON crudos de los sobres no se custodian: van sus resúmenes y su SHA-256, como en [BM].

### 5.2 El DLL

- **El DLL Debug del worktree,** compilado desde el Candidato (§3.1), en la ruta de [G] §2 y WORKFLOW §6: `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`.
- **Con AutoCAD cerrado** ([G] §1). Si hay un AutoCAD del Owner abierto, la sesión no lo cierra (PBT5-C08).
- **Con los comandos de [G] §2,** y el `dotnet` del usuario, el que resuelve `global.json`.
- **Antes de NETLOAD** ([G] §7.2):
  - la `InformationalVersion`, que identifica el Candidato;
  - el SHA-256 del DLL.
- **La diferencia con Smoke-2, declarada:** Smoke-2 compiló con `-p:UseAutoCADNuGetReferences=true -o <carpeta>` (`dll.md` y `dll-c2.md`). PB-T5 sigue la ruta de la guía y de WORKFLOW, que es lo que pide la sesión principal.
  - Si la `InformationalVersion` no identifica el Candidato, se vuelve a compilar con `--no-incremental`, como en Smoke-2, y se mide otra vez.
  - Si sigue sin identificarlo: STOP (PBT5-C01).

### 5.3 La configuración de AutoCAD y la carga

- **No se toca nada:** ni Trusted Locations, ni SECURELOAD, ni «Always Load», ni el registro, ni ninguna variable de sistema (`FILEDIA` y `PICKFIRST` incluidas).
- **La carga sigue [G] §3 tal cual:**
  1. abrir AutoCAD 2025 con un dibujo de prueba recuperable (una copia);
  2. `NETLOAD`;
  3. seleccionar el DLL exacto del worktree;
  4. `RACKCAD` y el menú sin excepciones;
  5. la ruta de la biblioteca de bloques.
- **El aviso de seguridad:** si AutoCAD lo muestra al cargar un DLL fuera de una ubicación de confianza, se responde «Load Once». Es lo que hicieron Smoke-2 y el bloque M, y no cambia la configuración. SPB-52 comprueba que no queda ninguna entrada en el perfil.
- **Las funciones de inventario** se teclean en cada documento, porque SECURELOAD impide `load` de un `.lsp` ([BM] §1). No se cambia SECURELOAD para cargarlas.

### 5.4 Quién lo opera

**El mensaje del Owner de esta noche,** literal, transmitido por la sesión principal y con su zona horaria redactada:

> «En caso de que se complete antes de las 6 am y quieras ejecutar el smoke, ejecutalo tú ya que estaré dormido, a menos que sean después de las 7 am de `<zona-horaria-del-Owner>`»

**La lectura de la sesión principal:**
- si el Smoke-PB está **listo para empezar** (§4.1) antes de las 7:00 de la hora del Owner, que son las **13:00 UTC del 2026-11-23**, lo opera la sesión, con control de pantalla;
- **la firma sigue siendo del Owner,** que revisa la evidencia al despertar;
- después de esa hora, se hace con el Owner.

**Dos supuestos de este paquete,** declarados, que el Owner puede corregir (Q-PBT5-5):
- **S-1:** entre las 6:00 y las 7:00 de su hora vale lo mismo que antes de las 6:00. La frontera es la de las 7:00, como lee la sesión principal.
- **S-2:** si a las 7:00 la sesión ya está operando, sigue hasta que el Owner escriba. Entonces para en el siguiente límite de paso y le pregunta si sigue ella o sigue él.

**Modo A, la sesión opera** (antes de la frontera):
- todos los pasos, con control de pantalla y las reglas del §5.5;
- los resultados PASS, FAIL u OBSERVADO son **propuestas de la sesión**, con su evidencia, no la firma del Owner. Es la lectura de `owner-smoke2-session-operator.md` (F4-E79);
- en los seis S+O, la sesión mide y deja el juicio visual del Owner como **PENDIENTE DE REVISIÓN DEL OWNER**, con las capturas que lo permiten;
- si el Owner, al revisar, juzga una captura insuficiente, ese paso se repite con él delante, en una adenda dirigida, como el mini-smoke del paso 50 (F4-E80).

**Modo B, con el Owner** (después de la frontera, o si el Owner aparece):
- el Owner opera los gestos del producto de los pasos que quiera y responde PASS, FAIL u OBSERVADO en los S+O;
- la sesión mide lo de máquina: los inventarios, los hashes, las horas y los módulos. Al Owner solo se le piden observaciones y la firma;
- cuando la sesión tiene que teclear un inventario, avisa antes («ahora tecleo yo; no toques AutoCAD») y devuelve el control después;
- la guía del operador va pegada en el chat, paso a paso;
- el Owner puede pasar el control a la sesión en cualquier momento, como en la corrida 2 de Smoke-2.

### 5.5 Las reglas de operación por control de pantalla

Son las de [F] §6.3 y las aprendidas en [S2] y [BM]:
- textos en trozos de 16 caracteres como máximo; líneas de 1024 como máximo; 3 s entre `defun`;
- **una captura antes de cada clic** en una ventana de RackCad o de AutoCAD. **Cada captura se abre y se revisa al tomarla,** y el registro lo dice. No se afirma revisada una captura que no se abrió;
- **nunca Ctrl+V en la línea de órdenes** (PASTECLIP). Se escribe con el teclado;
- tras dar el foco a la paleta, el primer clic solo la activa; se comprueba con la captura;
- **una ventana inesperada que se queda con el foco** (el incidente de [BM]) para la acción. Nunca se abre otra instancia de AutoCAD por error: si pasa, se cierra la vacía de forma ordinaria y se registra;
- **N-I1:** entre una orden medida y sus `U` no se teclea nada;
- **el error del operador** ([F] §6.5): una acción que, según la captura o la transcripción, no llegó al producto como se quería; por ejemplo, un clic perdido o un texto truncado.
  - Se puede repetir **como máximo dos veces** por paso, en una copia nueva si el paso lo necesita.
  - A la tercera, o ante cualquier duda de si fue error del operador, se registra y se vuelve al Owner (PBT5-C07).
  - Una repetición no gasta intento.

### 5.6 La evidencia para que el Owner firme sin haber estado delante

Va en `I-64-pilot/F4B-SMOKE-PB/`, con la forma de `F4-SMOKE-2/`:
- **`prep/candidate.md`:** el bloque del Candidato de [G] §7.1, con el SHA, el árbol limpio, el SDK resuelto, las suites locales, el build Debug de UI y del Plugin, la CI exacta (run, trabajos y totales), la biblioteca (ruta redactada y SHA-256) y la versión de AutoCAD;
- **`prep/dll.md`:** la forma de `dll-c2.md`, con la orden, el resultado, la `InformationalVersion`, los SHA-256 de los cuatro DLL y la hora;
- **`prep/selection.md` y `prep/copias.md`:** los dibujos, por su rol y su SHA-256, sin nombres ni rutas; los respaldos; las copias por paso;
- **`prep/environment-pre.md`:** P-05 y P-07, con su hora;
- **`prep/guia-smoke-pb-v1.md`:** la guía del operador, redactada, con su SHA-256;
- **`run/run-log.md`:** cada acción con su hora UTC y el resultado propuesto de cada paso. En el modo A, al empezar, la hora frente a las 13:00 UTC. Las desviaciones y los errores del operador, declarados; los incidentes, con su evidencia;
- **`run/screens/`:** las capturas, recortadas al panel, la ventana, la línea de órdenes o la vista. Sin rutas, sin nombres de cliente y sin otras ventanas del escritorio. Llevan su `derivation-record.txt`, como en [S2], y el nombre del paso. Los A/B, rotulados;
- **`run/inventories/`:** los resúmenes de los inventarios, con el SHA-256 de cada crudo; los crudos se quedan en local. Las comparaciones (`diffs.txt`), con las herramientas de [BM];
- **`run/integrity.md`:** originales, respaldos y copias, como `integrity.md` de [S2];
- **`run/environment-post.md`:** SPB-52;
- **`run/owner-summary.md`:**
  - una fila por paso, con el resultado propuesto y los punteros a su evidencia;
  - una sección **«Para tu revisión»** con los seis S+O y sus capturas;
  - los LIMITATION y su motivo;
  - los FAIL y los OBSERVADO, cada uno con su evidencia completa: dibujo, órdenes, mensajes, estado antes y después y Deshacer si aplica ([S2], disposición C);
  - la información de R-T4-39.

### 5.7 Lo que no se finge

- **Ninguna condición interna** que AutoCAD no exponga: excepciones del editor (SPB-22), fallos de transacción, inyecciones, el nombre de un grupo de Deshacer. Se registran NO VERIFICABLE FÍSICAMENTE o NO OBSERVABLE.
- **Ningún arnés,** ninguna variable `RACKCAD_DEBUG_*` y ningún código de producto para probar ([S2], decisión 1).
- **Ninguna escritura en el Xrecord de un sobre** fuera del producto, para fabricar un caso (SPB-07; Q-PBT5-2).
- **Ningún caso fabricado** salvo los A/B que autoriza D-PB-09 y, si el Owner responde A en Q-PBT5-4, las referencias de espacio papel y de bloque del usuario hechas con órdenes nativas.

---

## 6. El contrato (propuesto)

### 6.1 La forma: PB-T5 no tiene Worker de código

- **PB-T5 no cambia `src/`, `tests/` ni `assets/`.** No hay RED ni GREEN. No hay entrega de un Worker que planificar, verificar ni controlar.
- **La cadena de F4B** ([F] §5) es paquete con revisión independiente, contrato, planificación, Worker, CI exacta, verificación, revisiones, nc1 a nc3 y cierre por D2. Sus piezas de Controller (la planificación, la verificación y nc1 a nc4) existen para comprobar la entrega de un Worker. Aquí no hay entrega.
- **El precedente de F4** para un smoke de fase sin Worker ([S2]):
  - lo rigieron las decisiones del Owner (`owner-f4-smoke2-decisions.md`), la guía y las precondiciones de «PREPARAR SMOKE-2» (F4-E71);
  - no hubo `gate-contract.json` ni cadena del Controller;
  - lo cerró la disposición del Owner (F4-E79 y F4-E80);
  - y el gate-close, su firma: «Este cierre no tiene una revisión del Coordinator aparte: lo autoriza esa firma» (F4-E81).
- **El precedente dentro de F4B:** [F] §6.5 adapta la cadena al bloque M de PB-T0. No consume intentos del bloque C, y fija el error del operador y sus dos repeticiones.
- **La propuesta:** el contrato de PB-T5 es este §6, en Markdown, custodiado con la emisión.
  - Lleva lo que llevan los contratos de F4B que sí aplica: `TaskId`, autoridades, alcance de escritura, invariantes, paradas, evidencia esperada y presupuesto.
  - No lleva lo que solo aplica a un Worker: la planificación del Controller, A1-A8, nc1 a nc4, `RequiredTests` ni la regla RR.
  - Por eso la v3 del sustituto de nc2 (F4B-E58), que vale «PB-T5 incluida», no tiene aquí ningún control que sustituir.
  - **Es una adaptación, no una forma nueva.** La revisión independiente del paquete lo comprueba; si la juzga insuficiente, vuelve al Owner.
- **El cierre no es por D2.** [F] §9 exige la firma del Owner, y con ella se cierra a la vez PB-T5 y F4B, como F4-E81 cerró F4.

### 6.2 La identidad, las autoridades y el alcance de escritura

- **`TaskId` (provisional):** `F4B-PBT5-SMOKE-PB-GATE-CLOSE`. Se confirma en la emisión.
- **Autoridades:** las del §1, con el registro literal de las respuestas del Owner a `owner-questions-v1.md`.
- **El alcance de escritura de la sesión:**
  - **en el repositorio,** solo `docs/automation/evidence/I-64-pilot/F4B-SMOKE-PB/`, `.../F4B-DRAFTS/pbt5/`, `.../F4B-DRAFTS/coordinator/` (los registros del Owner), `.../F4B-DRAFTS/gate-close.md`, [DISP], `docs/automation/evidence/I-64-evidence.md` y `docs/automation/state/I-64.yml`; y `docs/ideas-futuras.md`, solo si el Owner lo manda;
  - **fuera del repositorio,** solo `<carpeta-del-Smoke-PB>` (copias, respaldos, salidas, inventarios crudos y capturas sin recortar), el build en `bin/` y `obj/` del worktree, y la restauración de la disposición del configurador de P-02.
- **Prohibido:**
  - `src/`, `tests/`, `assets/` y los archivos de proyecto;
  - [G]; `docs/HANDOFF.md`; `docs/ROADMAP.md`; `docs/WORKFLOW.md`; `AGENTS.md`;
  - `main` y cualquier otra rama;
  - los originales de los dibujos;
  - la configuración de AutoCAD y el registro;
  - la biblioteca de bloques, la de diseños y `settings.json` del usuario.

### 6.3 Invariantes

| Id | Invariante |
|---|---|
| INV-PBT5-01 | `git diff <Candidato> <punta del cierre> -- src tests assets` vacío. PB-T5 solo añade documentación y evidencia |
| INV-PBT5-02 | El DLL del Smoke-PB sale del Candidato: su `InformationalVersion` lo identifica, y su SHA-256 se registra antes de NETLOAD y se comprueba en el proceso (SPB-02) |
| INV-PBT5-03 | Solo copias de trabajo. Los originales nunca se abren, y tienen copia de seguridad antes de usarse |
| INV-PBT5-04 | Ningún cambio de la configuración de AutoCAD: NETLOAD de la guía y, si sale el aviso, «Load Once» |
| INV-PBT5-05 | Nada se finge (§5.7): ni condiciones internas, ni arnés, ni variables de depuración, ni escrituras fuera del producto |
| INV-PBT5-06 | Todo A/B fabricado lleva el rótulo **CONSTRUCTED REPRESENTATIVE TEST CASES** en la guía, el registro y la evidencia |
| INV-PBT5-07 | Los resultados de la sesión son propuestas. F4B se cierra solo con la firma del Owner |
| INV-PBT5-08 | La evidencia custodiada no lleva rutas locales, nombres de clientes ni datos personales. Los crudos se quedan en local, con su SHA-256 |
| INV-PBT5-09 | [G] no cambia |
| INV-PBT5-10 | Sin integración en `main`. HANDOFF y ROADMAP no cambian |
| INV-PBT5-11 | Una captura antes de cada clic, y cada captura abierta y revisada al tomarla |
| INV-PBT5-12 | N-I1 en toda medición de Deshacer |
| INV-PBT5-13 | Las salidas a disco del producto, solo en `<carpeta-de-salidas>` |
| INV-PBT5-14 | Ningún FAIL se corrige dentro de PB-T5. Una corrección es una tarea propia, con su contrato y su presupuesto |

### 6.4 Paradas

| Id | Cuándo | Qué se hace |
|---|---|---|
| PBT5-C01 | El Candidato no cumple P-01 o P-02, o el DLL no lo identifica (P-03), o cambia un SHA-256 del DLL durante la corrida | STOP antes de abrir AutoCAD, o en el momento. No se carga otro DLL |
| PBT5-C02 | Una copia no es igual a su original antes de usarla, o un original cambia | STOP. Si cambió un original, se avisa al Owner en seguida |
| PBT5-C03 | Haría falta cambiar algo de la configuración («Always Load», Trusted Locations, SECURELOAD, una variable o el registro) | STOP. No se cambia |
| PBT5-C04 | Hay alguna `RACKCAD_DEBUG_*` | STOP |
| PBT5-C05 | Un FAIL de producto | Se registra con su evidencia completa y no se corrige. Se siguen solo los pasos independientes que no puedan dañar nada ([S2] R10). Vuelve al Owner con la corrida |
| PBT5-C06 | Una acción podría escribir fuera de las copias o de `<carpeta-de-salidas>` (un diálogo de guardado que apunta a un original o a una carpeta del usuario) | STOP del paso, con LIMITATION |
| PBT5-C07 | La tercera repetición de un paso por error del operador, o una duda de si lo fue | Se registra y vuelve al Owner |
| PBT5-C08 | Hay un AutoCAD del Owner abierto al compilar, o la corrida tendría que tocarlo | No se cierra ni se toca. STOP del build; la corrida usa otra instancia, y si no puede, STOP |
| PBT5-C09 | Hay un Worker u otra sesión trabajando en el worktree | STOP |
| PBT5-C10 | Un paso solo se puede observar fingiendo una condición interna | No se hace: NO VERIFICABLE FÍSICAMENTE |
| PBT5-C11 | La frontera horaria del §5.4 | Lo que diga S-2, o la respuesta del Owner a Q-PBT5-5 |
| PBT5-C12 | AutoCAD se cuelga o se cierra solo | Se registra. Si se cuelga, se espera y se cierra de forma ordinaria, sin guardar. La corrida sigue en una instancia y una copia nuevas solo si el bloque no quedó a medias; si quedó a medias, el bloque se repite entero. Un segundo caso vuelve al Owner |
| PBT5-C13 | Un comportamiento no caracterizado | OBSERVADO, con toda la evidencia. No se reinterpreta como FAIL ni como PASS ([S2], decisión 2) |
| PBT5-C14 | Haría falta el intento 3 | Vuelve al Owner (OB-10.2) |

### 6.5 La evidencia esperada

- `I-64-pilot/F4B-SMOKE-PB/prep/` y `run/`, con lo del §5.6.
- `I-64-pilot/F4B-DRAFTS/pbt5/`:
  - este paquete y sus versiones, su revisión independiente y su encargo;
  - la guía del operador redactada;
  - los traspasos;
  - las herramientas de inventario y de análisis que se amplíen, con su revisión.
- `I-64-pilot/F4B-DRAFTS/coordinator/`, literales:
  - las respuestas del Owner a `owner-questions-v1.md`;
  - su disposición sobre la corrida;
  - su firma del cierre.
- `I-64-pilot/F4B-DRAFTS/gate-close.md`.
- Las entradas de [DISP], las secciones de `I-64-evidence.md` y los cambios de `I-64.yml` del §3.3.

### 6.6 El presupuesto

- **PB-T5 empieza en el intento 1 de 3** (OB-10.1). Sin bolsa compartida.
- **Qué es un intento** (propuesta de la sesión, declarada): una corrida del Smoke-PB sobre un Candidato.
  - **No gastan intento:**
    - la repetición de un paso por error del operador, como máximo dos (§5.5);
    - una parada antes de SPB-01 por el entorno (PBT5-C01 a C04, C08 y C09), como los lanzamientos no productivos de PB-T4a y PB-T4b;
    - una adenda dirigida que pida el Owner al revisar (§5.4).
  - **Pasa al intento 2:** una corrida nueva sobre un Candidato nuevo, tras una tarea de corrección cerrada. Lo decide la sesión (OB-10.2). Qué pasos se repiten lo fija el Owner en su disposición. La propuesta por defecto es el bloque 0, los bloques que toca la corrección y el bloque Z.
  - **El intento 3** vuelve siempre al Owner.
- **Una tarea de corrección** que nazca de un FAIL tiene su propio contrato y empieza en 1/3, como F4-T4-HOST-C1 en F4 (F4-E74).

### 6.7 La cadena, paso a paso

1. **Este paquete,** con su revisión independiente, en solo lectura, por objetos git ([F] §5). Las correcciones, en una v1.x.
2. **Las preguntas al Owner** (`owner-questions-v1.md`), con su respuesta o con el valor por defecto que declara cada una. Ninguna impide preparar.
3. **El cierre de PB-T4e** **[T4e]**: se completan el producto, la CI y los números F4B-E.
4. **La emisión de PB-T5:**
   - **autoridad:** la orden de esta noche del Owner, leída como autorización para operar el Smoke-PB; para emitir, además, la condición de las emisiones condicionales de F4B (F4B-E09, F4B-E49 y la 15.ª tanda): la revisión del paquete da 0 bloqueantes y 0 importantes sin resolver, y no aparece ninguna decisión material nueva. Si no se cumple, vuelve al Owner (Q-PBT5-5 lo confirma);
   - **lo que se custodia:** un commit documental con este paquete, su revisión, la guía redactada y la entrada F4B-E(n);
   - **el Candidato:** ese commit, si el DLL se compila después (§3.1).
5. **La preparación:** P-01 a P-08 y el registro «PREPARADO», como F4-E71.
6. **La corrida,** en el modo A o en el B (§5.4).
7. **La custodia** de la corrida, redactada, en un commit documental de la sesión: la entrada de [DISP] y la sección de la evidencia. No cambia el Candidato.
8. **La revisión del Owner y su disposición:** ratifica o reclasifica cada paso y decide los OBSERVADO y los LIMITATION. Si hay un FAIL, PBT5-C05 y §6.6.
9. **La firma del Owner** y el gate-close (§3.3), en un commit documental.

---

## 7. Riesgos

| Id | Riesgo | Tratamiento |
|---|---|---|
| R-1 | La corrida por control de pantalla es larga (52 pasos y unas 30 copias) y puede cruzar la frontera de las 13:00 UTC | S-2 y Q-PBT5-5; el registro deja el punto exacto en que se para; los bloques son independientes |
| R-2 | Ningún gesto de fuera disponible lleva a «Conservar mi borrador» (R-T4-39) | SPB-24 lo registra; SPB-25 a SPB-28 quedan NOT AVAILABLE y se informa. No se fuerza |
| R-3 | Un AutoCAD del Owner abierto impide compilar | PBT5-C08: no se toca. El build espera al Owner |
| R-4 | Las pruebas de UI locales sobrescriben la disposición del configurador del usuario (F4B-E30) | Copia antes y restauración byte a byte después (P-02) |
| R-5 | La ampliación del inventario (contar referencias anidadas y de espacio papel) es nueva | Solo lectura; se revisa antes de usarla; un control positivo sobre la referencia anidada que ya midió [BM] en D1 |
| R-6 | El error del operador hace pasar por defecto lo que es un fallo, o al revés (el paso 50 de Smoke-2) | Una captura antes de cada clic; las capturas abiertas; dos repeticiones como máximo; ante la duda, al Owner |
| R-7 | Las capturas sacan rutas, nombres de clientes u otras ventanas (la herramienta copia la pantalla real) | Recortes al panel, la ventana o la línea de órdenes; `derivation-record.txt`; revisión antes de custodiar |
| R-8 | Los casos construidos (A/B y, con Q-PBT5-4 A, espacio papel y bloques) se toman por ejemplos reales | El rótulo; OB-09.1; el resumen los separa |
| R-9 | El aviso de seguridad de NETLOAD ofrece «Always Load» por defecto | Captura antes del clic; solo «Load Once»; SPB-52 comprueba el perfil |
| R-10 | Las salidas del BOM o de la biblioteca acaban en una carpeta del usuario | SPB-17 con `<carpeta-de-salidas>` escrita entera en el diálogo y comprobada en la captura; PBT5-C06 |

---

## 8. Supuestos declarados

- **S-1 y S-2:** la frontera horaria del §5.4.
- **S-3:** el Candidato es la punta desde la que se compila el DLL, y se registra también el producto (§3.1).
- **S-4:** los dibujos reales son los de F4B: D1 y, si un paso lo necesita, otros de la selección de Smoke-2 (`prep/selection.md`), con las mismas reglas. La sesión los elige, como en F4-E71.
- **S-5:** los A/B se fabrican con el método de [BM] M-4, con el clásico y sobre una copia de un rack real de D1. D-PB-09 autoriza fabricarlos, pero no fija cómo.
- **S-6:** la igualdad con el clásico, en una copia gemela, es el oráculo de SPB-18, SPB-39, SPB-40, SPB-41, SPB-45 y SPB-46. Sale de [F] §6.4, fila 1, y de la «igualdad de PB-T3».
- **S-7:** una copia por escenario contaminable. Es la regla de Smoke-2, adoptada aquí sin que [F] §7.2 la herede.
- **S-8:** qué es un intento (§6.6).
- **S-9:** la forma del contrato y de la cadena (§6.1 y §6.7).
- **S-10:** los textos literales que cita el §4 salen de los registros, no del `src/` del GREEN de PB-T4e, que no existe aún **[T4e]**. La guía los vuelve a comprobar en el `src/` del Candidato, por objetos git, antes de la corrida.

---

## 9. Preguntas al Owner

Están en `owner-questions-v1.md`, cada una con sus opciones, la recomendada y el valor que se aplica si no hay respuesta:
- **Q-PBT5-1:** la evidencia local del Candidato (UI Full local o la dispensa de F4).
- **Q-PBT5-2:** el estilo de cotas (D-PB-06 (b)) en RACKEDITAR, sin casos reales.
- **Q-PBT5-3:** sumar al Smoke-PB las otras correcciones declaradas de RACKEDITAR (D-PB-11, H-31 y OWNER-1 de PB-TC2).
- **Q-PBT5-4:** los riesgos del borrado real (OWNER-1 a OWNER-3 de PB-T3c), con casos construidos o solo con los reales.
- **Q-PBT5-5:** confirmar la lectura de la orden de esta noche y la frontera de las 7:00.
- **Q-PBT5-6:** una revisión independiente de la evidencia antes de firmar, o firmar sobre ella como en Smoke-2.

Y una **información, no una decisión:** R-T4-39, que F4B-E60 manda contar en PB-T5 junto al paso 6.

---

## 10. Lo no verificado

- **PB-T4e no ha cerrado.** Su producto, su CI, sus totales finales y su número F4B-E van marcados **[T4e]**. Si su cierre cambia algo material (por ejemplo, una reparación del RED que toque un texto del §4), este paquete se vuelve a medir.
- **No se leyó el worktree,** ni se compiló, ni se ejecutó ninguna prueba, ni se abrió AutoCAD.
- **Los literales de la interfaz** del §4 salen de los registros ([T4] §5.5, D-PBT4-09, F4B-E06, la corrida 2 de Smoke-2 y la revisión de PB-T4c), no de una lectura completa del `src/` (S-10).
- **Sin comprobar:**
  - que un cambio de fuera disponible lleve a «Conservar mi borrador» con un borrador Dirty (R-2);
  - que escenificar un módulo se alcance desde la ventana (SPB-21);
  - que el hueco entre lados (SPB-41 y SPB-42) se edite como supone el paso en un rack fabricado con un solo frente de B.
- **Los dibujos:**
  - no se comprobó que el original D1 siga donde estaba ni con el SHA-256 de Smoke-2 y de [BM];
  - no se sabe si hay, en dibujos reales, vistas de rack en espacio papel, dentro de bloques dinámicos, con `SchemaVersion` 1.1 o con nombres distintos (SPB-45 a SPB-48).
- **La ampliación del inventario** para las referencias anidadas no está escrita ni probada (R-5).
- **Si NETLOAD mostrará el aviso de seguridad** depende de SECURELOAD y de la ubicación del worktree. El paquete cubre los dos casos.
- **La duración de la corrida** no se ha estimado con medida. Smoke-2 tardó unas 4,5 h en su corrida 2 (00:40Z a 05:20Z) con 88 pasos, operados en parte por el Owner.
- **La forma del contrato** (§6.1) es una adaptación propuesta. La comprueba la revisión independiente.
- **Los nombres** de `TaskId`, de la carpeta `F4B-SMOKE-PB/` y de los archivos de la evidencia son provisionales.

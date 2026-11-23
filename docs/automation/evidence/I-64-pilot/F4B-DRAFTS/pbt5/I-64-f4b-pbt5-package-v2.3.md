# I-64 F4B — Paquete de PB-T5: CI exacta, Smoke-PB y gate-close de F4B (v2.3)

> **Redacción para el repo público:** las rutas locales van como marcadores, salvo la carpeta de trabajo del Smoke-PB, `<raiz-de-trabajo>\`, que fijó la sesión principal. Al custodiar, `<carpeta-del-Smoke-PB>` pasa a `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>` a `<carpeta-de-respaldo>`, como las copias exportadas `r71…` pasaron a `<copia>` (F4B-E60). No hay nombres personales ni de clientes. La hora del Owner, en UTC; su zona horaria, como `<zona-horaria-del-Owner>`.

**Estado:** v2.3. **Aplica las siete decisiones del Owner** de `owner-pbt5-decisions.md` ([OD5]), literal. Es la versión que pasa la comprobación acotada que exige la emisión condicional (Q-PBT5-7). **No está emitido.**

**Base y método:**
- Todo se lee por objetos git en `7da361eb`. No se leyó el worktree. No se compiló, no se ejecutó ninguna prueba y no se abrió AutoCAD.
- **El cierre de PB-T4e,** leído por objetos git: F4B-E61 y la §291, en `ebd304a3773495fa9071de6611936c4c01a7b06c`, que es el Candidato. El producto es el GREEN `0eac7911a025a59628b446db8b768035175f96de`: `src/` y `tests/` del Candidato son los suyos. La CI del GREEN es `45123886125`, 4/4; la de push del Candidato, `45147867500`, estaba en curso al escribir esto. Ya no queda nada marcado [T4e].

**Los documentos de PB-T5:**
- **este paquete;**
- **`smoke-run-v1.md`:** la guía operativa del Smoke-PB contratado, con las órdenes exactas;
- **`_tools/emit_pbt5.py`:** el guion de la emisión, con `--dry` y `--commit`, que corre la sesión;
- **`owner-pbt5-decisions.md`:** el registro literal de las decisiones. Al custodiar, va a `F4B-DRAFTS/coordinator/`;
- **la historia:** las versiones v1 a v2.2 del paquete y de las preguntas, la revisión independiente y sus cuatro recomprobaciones (`review-v1/`), las guías de la corrida previa (`pre-run-tonight-v2.md` a `v2.4.md`), que no llegó a hacerse, y los traspasos.

**Abreviaturas:**
- **[F]:** el paquete de la fase, `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1;
- **[DISP]:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md`;
- **[T3] y [T4]:** los paquetes de PB-T3 y de PB-T4 (v5.6);
- **[G]:** la guía `docs/guias/validacion-manual-autocad.md`, que **no se modifica**;
- **[S2]:** Smoke-2 de F4 (`I-64-pilot/F4-SMOKE-2/`, `F4-DRAFTS/f4-dispositions.md` F4-E59 y F4-E71 a F4-E81, `F4-DRAFTS/coordinator/`, `F4-DRAFTS/gate-close.md`);
- **[BM]:** el bloque M de PB-T0;
- **[IR1]:** la revisión independiente del paquete, con sus recomprobaciones;
- **[OD5]:** `owner-pbt5-decisions.md`;
- **[SR]:** `smoke-run-v1.md`.

---

## Registro de cambios v2.2 → v2.3

| Decisión del Owner ([OD5]) | Qué cambia en la v2.3 |
|---|---|
| **Q-PBT5-7, «Sí, con condiciones»** | La emisión, en §6.7: sin volver al Owner si se cumplen tres condiciones (la comprobación acotada de esta v2.3 da 0 bloqueantes y 0 importantes sin resolver; no aparece ninguna decisión material nueva; el Candidato tiene la CI exacta en verde). Vuelve por una decisión nueva, una prueba que falle o un intento 3 |
| **Q-PBT5-8, «Adaptación con revisión»** | El contrato de §6 en su forma B: propio (invariantes, paradas, evidencia y presupuesto), sin Controller, sin nc1 a nc4 y sin D2. **Antes de la firma, una revisión independiente de solo lectura** comprueba que el registro, las capturas y los inventarios coinciden. La regla de reejecutar la CI queda confirmada (PBT5-C15) |
| **Q-PBT5-9, «Definición y reglas»** | La definición de intento (§6.6) y las tres reglas de Smoke-2, adoptadas: las etiquetas, el protocolo OBSERVADO y una copia por escenario (§4.0) |
| **Q-PBT5-1, «Extender la dispensa»** | La CI exacta y los filtros locales, sin la UI completa en local (§3.1, P-02 e INV-PBT5-15) |
| **Q-PBT5-10, «Hacerlas como en Smoke-2»** | SPB-59 a SPB-61 entran: un AutoCAD por inyección, con la variable solo en el entorno de ese proceso (§5.5; [SR] §4.11) |
| **Q-PBT5-4, «Construirlos y rotularlos»** | SPB-45, 46, 57 y 58 entran: en copias, con órdenes nativas tecleadas, rotulados y comparados con RACKEDITAR en una gemela (§4.0; [SR] §4.12) |
| **Q-PBT5-2, «Contar antes en otros»** | La precondición P-09 nueva: el recuento en los demás dibujos de la selección de Smoke-2 antes de SPB-07. Si ninguno guarda un estilo, SPB-07 vuelve al Owner (§3.3, condición 3) |
| **Q-PBT5-5, no se preguntó** | La corrida previa no llegó a hacerse. Sus guías quedan como historia. La frontera de esta noche, para el Smoke-PB contratado, es la lectura de la sesión principal (§5.4) |

**Lo demás:**
- de §4 no cambia nada fuera de lo de la tabla;
- §5 y §6 se reescriben con las decisiones;
- las correcciones de la guía de la corrida previa (I-07 a I-09 y M-18 a M-35) pasan a [SR].

---

## 0. En una página

- **Qué es PB-T5:** la última tarea de F4B: «CI exacta, Smoke-PB (D-PB-09) y gate-close de F4B» ([F] §5). Empieza con el cierre de PB-T4e (F4B-E61, `ebd304a3`).
- **No tiene Worker de código.** El contrato es la forma B que eligió el Owner (Q-PBT5-8): sin Controller, sin nc1 a nc4 y sin D2, con una revisión independiente de la evidencia antes de su firma.
- **La emisión** es condicional (Q-PBT5-7), por el guion `_tools/emit_pbt5.py`. Es la entrada F4B-E62 y la sección 292 de la evidencia.
- **La CI exacta:** el Candidato es `ebd304a3`, la punta del cierre de PB-T4e. Lleva la CI de push 4/4 sobre ese SHA y los totales del GREEN de PB-T4e, la suite de Core en local y los 19 filtros de UI, sin la UI completa en local (Q-PBT5-1).
- **El Smoke-PB:** 65 pasos en 12 bloques, más 9 precondiciones.
  - 56 los mide la sesión; 9 necesitan además el juicio visual del Owner (S+O);
  - los A/B y los cuatro contextos construidos van rotulados **CONSTRUCTED REPRESENTATIVE TEST CASES**;
  - lo demás, sobre racks reales, en copias.
- **Quién opera:** la sesión, si puede empezar antes de las 13:00 UTC; a las 13:00 UTC para en el siguiente límite de paso y espera al Owner. Después, con el Owner. Los juicios S+O, la revisión de la evidencia y la firma no los hace la sesión.
- **El gate-close:** tras la revisión independiente de la evidencia y la firma del Owner, como F4-E81. No cambian `main`, HANDOFF, ROADMAP ni [G].

---

## 1. Autoridades

| Autoridad | Qué fija para PB-T5 |
|---|---|
| **[OD5]** | La emisión (Q-PBT5-7), la forma del contrato y la regla de la CI (Q-PBT5-8), el intento y las reglas de Smoke-2 (Q-PBT5-9), la dispensa de la UI (Q-PBT5-1), las inyecciones (Q-PBT5-10), los contextos construidos (Q-PBT5-4) y el estilo de cotas (Q-PBT5-2) |
| [F] §5, fila PB-T5 | El alcance |
| [F] §5, «Cada tarea pasa por la cadena de F4…», y §6.5 | La cadena, adaptada en la forma B que eligió el Owner |
| [F] D-PB-09 (OB-09.1 y OB-09.2) | Los A/B fabricados y rotulados; racks reales donde existen; copias; originales sin abrir |
| [F] D-PB-10 | 1/3 por tarea; el intento 2 lo decide la sesión; el 3 vuelve al Owner |
| [F] §6.3 | Las reglas de operación, NETLOAD con «Load Once», los inventarios de solo lectura y «No se finge una condición interna» |
| [F] §8 y §9 | Los riesgos que remiten al Smoke-PB y los criterios de cierre |
| [T3] §5.5, §5.6 y A-T3-5 | Lo físico del escritor completo |
| [T4] §5.5 y §10 | La lista del Smoke-PB de PB-T4 y lo que queda para PB-T5 |
| [DISP] F4B-E06, E08, E09, E12, E23, E48, E51, E53, E55, E58, E59 y E60 | Lo que cada tarea dejó al Smoke-PB (§4.5) |
| Los cierres de PB-TC (`:65`), PB-T3c (`:53` y `:56`) y PB-T4b (`:59`); la revisión de PB-T4a (`:207-215`); el paquete de PB-T2 (`:1329`) | Lo dejado al Smoke-PB fuera de [DISP] |
| [S2] F4-E59 | La regla de la UI en local con contención de perfil, que la dispensa extendida respeta |
| [S2] F4-E71 a F4-E81 | El precedente de un smoke de fase y su cierre por la firma |
| [BM] | El precedente de operación de la sesión en F4B |
| [G]; WORKFLOW §4 y §6; AGENTS | El entorno, el DLL del worktree, la carga, el Candidato y la reutilización por SHA exacto |

---

## 2. Hechos de partida

**Medidos en `7da361eb`:**
- 17 tareas de F4B están cerradas: las 16 de antes y PB-T4e, CLOSED / PASS WITH LIMITATIONS sobre `0eac7911`, intento 1 de 3 (F4B-E61, en `ebd304a3`);
- los totales esperados en la CI del GREEN de PB-T4e: Core 19 421 = 19 018 + 0 + 403; UI 2 465 = 2 417 + 0 + 48 (F4B-E60), confirmados en el cierre: la CI del GREEN `45123886125`, 4/4 (F4B-E61);
- `I-64.yml`: `current_phase: F4B`, `state: implementing`, `gate: none`, `attempts: 1`;
- **la numeración:** tras el cierre de PB-T4e, [DISP] llega a F4B-E61 y la evidencia a la §291. La emisión de PB-T5 es **F4B-E62** con la **§292**. El guion lo comprueba (§6.7).

**Los dibujos:**
- D1 (R006), con 14 sobres Push Back de un sentido, sin `DimensionStyle`, y una referencia anidada de una planta en un bloque pegado;
- D2 (R053), sin referencias de nivel superior;
- ningún A/B real con referencias;
- los demás dibujos reales con racks Push Back de la selección de Smoke-2, donde nadie ha contado el estilo de cotas (Q-PBT5-2).

**Lo de la carga, la contención de perfil, el diálogo de la biblioteca y los literales:** como en la v2.2 (§2), sin cambios.

**La corrida previa a la emisión no se hizo:** PB-T4e no cerró a tiempo ([OD5], «El origen»).

---

## 3. El alcance de PB-T5

### 3.1 La CI exacta del Candidato

- **El Candidato** es `ebd304a3773495fa9071de6611936c4c01a7b06c`, la punta del cierre de PB-T4e: el SHA exacto desde el que se compila el DLL.
- **El orden** que exige [G] §7.2 (la `InformationalVersion` del DLL identifica el Candidato):
  1. la comprobación del Candidato y el build, con el worktree en el Candidato;
  2. después, el commit documental de la emisión (F4B-E62), encima.

  El DLL compilado en el Candidato no se vuelve a compilar tras ese commit: si se recompilara, la versión cambiaría, y los bloques de [SR] pararían.
- **El producto** es `0eac7911a025a59628b446db8b768035175f96de`, el GREEN de PB-T4e, el último commit que toca `src/` o `tests/`. `git diff <producto> <Candidato> -- src tests assets` vacío.
- **La CI exacta:** `event = push`, `ref = refs/heads/architecture/workspace-persistente-rackcad` y `head_sha` = Candidato, con los cuatro trabajos en `success` y los totales de §2.
- **La regla de reejecutar, confirmada por el Owner (Q-PBT5-8):** si la CI sale en rojo solo por el detector de cuelgue en una prueba lenta conocida, se reejecutan una vez los trabajos fallidos sobre el mismo SHA; otra causa, o un segundo rojo, vuelve al Owner.
- **La evidencia local:** la suite completa de Core en local; los 19 filtros de UI de los `RequiredTests` del contrato de PB-T4e, cada uno en su proceso; y los builds Debug de UI y del Plugin.
- **La dispensa (Q-PBT5-1, «Extender la dispensa»):** sin la UI completa en local, como en Smoke-2 (DV-05 / O-08). No se levanta la regla de F4-E59. La validación final de la iniciativa sí la exigirá. El bloque del Candidato de [G] §7.1 lo escribe así: «UI Full local: dispensada por el Owner para el Smoke-PB de F4B (Q-PBT5-1)».

### 3.2 El Smoke-PB

- **Cubre** lo que solo se ve en AutoCAD:
  - lo que PB-T3, PB-T4 y sus subtareas dejaron «sin AutoCAD»;
  - las correcciones declaradas de RACKEDITAR;
  - los A/B fabricados;
  - y, por las decisiones del Owner, las inyecciones y los cuatro contextos construidos.
- **No es** Owner Validation ni la validación total de [G] §4.

### 3.3 El gate-close de F4B

**Condiciones** ([F] §9):
1. **18 tareas cerradas:** las 17 de §2, PB-T4e incluida, y PB-T5, que cierra con el gate.
2. **`superseding-characterization.md`** con todas sus secciones en EJECUTADA, también «## PB-T4e», que el cierre de PB-T4e pasó a EJECUTADA (F4B-E61).
3. **El Smoke-PB verifica en RACKEDITAR D-PB-06 y D-PB-08** (SPB-06 a SPB-08).
   - Para el estilo de D-PB-06 (b), primero el recuento de P-09 (Q-PBT5-2).
   - Si ningún dibujo real guarda un estilo, **vuelve al Owner,** que decide si acepta el criterio cumplido en parte.
4. **IC-06.**
5. **Los INV del §1 de [F],** con su tabla INV → tarea y prueba en `gate-close.md`.
6. **La CI exacta del Candidato** y la evidencia local de §3.1, con la dispensa de la UI.
7. **Smoke-PB PASS,** o PASS WITH LIMITATIONS con las limitaciones aceptadas por el Owner.
8. **La revisión independiente de la evidencia** (Q-PBT5-8) y, después, **la firma del Owner.**
9. **Sin integración parcial en `main`.**

**Lo que cambia y lo que no:** como en la v2.2 (§3.3), con la revisión de la evidencia en `F4B-DRAFTS/pbt5/` y su resultado en la entrada de [DISP] de la disposición del Owner.

**La redacción del cierre:**
- «F4B CLOSED / PASS WITH LIMITATIONS» o «CLOSED / PASS»;
- «No es Owner Validation y no integra en `main`»;
- desde ese momento, «editor de Push Back migrado» (IC-06 e IC-11);
- F5-A deja de esperar.

---

## 4. La lista del Smoke-PB

### 4.0 Convenciones

**Resultados.** Son los de Smoke-2, adoptados en F4B por el Owner (Q-PBT5-9: «Definición y reglas»), con el protocolo OBSERVADO (traer la evidencia antes de decidir) y una copia por escenario.

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
- **Construir:** poner el dibujo, con órdenes nativas de AutoCAD, en un estado que el producto no crea. Lo autorizó el Owner (Q-PBT5-4: «Construirlos y rotularlos»): los cuatro contextos de SPB-45, 46, 57 y 58, en copias, rotulados **CONSTRUCTED REPRESENTATIVE TEST CASES** y cada uno comparado con RACKEDITAR en una gemela.
- **Inventario** (`RCDUMP <etiqueta>`): las funciones de solo lectura de [BM] (`lisp_src.txt`), tecleadas en cada documento, más tres de solo lectura nuevas (§5.3):
  - `rcblk`: todas las definiciones de bloque, para ver la purga. **Su control** (M-21): en c01, más de 0 líneas `BLK`, y entre ellas todas las definiciones de rack de las líneas `DEF`;
  - `rcnest`: las referencias de vistas de rack dentro de otras definiciones, con el indicador del contenedor y si es dinámico;
  - `rcdyn`: si una definición es dinámica (`ACAD_ENHANCEDBLOCK` en el diccionario de extensión de su registro). **Su control** (M-28): `rcdynchk` lo compara en c01 con la propiedad `IsDynamicBlock` de ActiveX, un oráculo independiente. El control negativo son las vistas de rack, que dan «0»; el positivo, las definiciones dinámicas, que dan «1»; y no puede haber ninguna discrepancia. Si falla, SPB-47 queda LIMITATION.
  - `c:RCDUMP` no sobrescribe: si la etiqueta ya existe en esa copia, no escribe nada (M-26).

  **El control de `rcnest`:** la planta anidada de D1, dentro de un bloque pegado «A$C…» ([BM] §3). Ese contenedor no es dinámico ni anónimo, y no cuenta como caso de SPB-47.

  El sobre JSON completo de cada vista ya va en `RCDUMP`. El análisis lee de él `SchemaVersion`, el GUID, los metadatos de sesión y la `ExtensionData` (I-04).
- **N-I1:** entre la orden medida y sus `U` no se teclea nada.
- **Los literales** entre comillas latinas están en `src/` de `7da361eb`. La guía los copia del `src/` del Candidato.

### 4.1 Precondiciones (la sesión, con AutoCAD cerrado)

| Id | Origen | Qué se hace | Criterio (si no se cumple, STOP) |
|---|---|---|---|
| P-01 | [F] §9; §3.1; M-04 | `git -C <worktree> status` y `rev-parse HEAD`; el diff `<producto>..<Candidato> -- src tests assets`; la CI exacta | Árbol limpio; HEAD = Candidato; diff vacío; la corrida con `event = push`, `ref = refs/heads/<rama>`, `head_sha` = Candidato, los cuatro trabajos en `success` y los totales de §2. Si falla una prueba: PBT5-C15 |
| P-02 | [G] §2; AGENTS; [S2] F4-E59; **Q-PBT5-1: «Extender la dispensa»** | Core Full local y los 19 filtros de UI de los `RequiredTests` del contrato de PB-T4e, cada uno en su proceso; el SHA-256 y la fecha de `RackFrameConfigurator.layout`, antes y después. **Sin la UI Full local:** la dispensa de Smoke-2 (DV-05 / O-08) extendida a F4B por el Owner | Core y los filtros con los totales esperados. El cambio del archivo de disposición, si lo hay, se registra |
| P-03 | [G] §1, §2 y §7.2; WORKFLOW §6 | Sin `acad.exe` ni `accoreconsole.exe`, los builds Debug de UI y del Plugin de [G] §2. El DLL es `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` | 0 errores y ningún `warning CS`; la `InformationalVersion` identifica el Candidato; los SHA-256 de los cuatro DLL y la hora. Si un AutoCAD del Owner está abierto: PBT5-C08 |
| P-04 | [BM] P-2 | `catalogs` junto al DLL, frente a `assets/catalogs` del Candidato | Iguales byte a byte |
| P-05 | [S2], «Comprobaciones del día»; [G] §7.1; M-03; M-17 | `RACKCAD_DEBUG_*` en el usuario, la máquina y el shell de la sesión; bundles de RackCad; `Applications`, `Loaded` y `AssemblyMap` del perfil de AutoCAD (lectura); el número de `Dialogs\RackCad.*`, solo como registro; la biblioteca de bloques (ruta y SHA-256); **si existe la carpeta de la biblioteca de diseños**; el registro de RackCad del día; keep-awake | Ninguna variable; ningún bundle; ninguna entrada de RackCad en `Applications`, `Loaded` ni `AssemblyMap`. Las `Dialogs` no son criterio. Si la carpeta de diseños no existe, se anota para SPB-17 |
| P-06 | [F] OB-09.2 y §6.3; M-18; M-26 | Las carpetas, nuevas (si ya existen, se para), fuera del perfil del usuario, del worktree, del checkout principal y de todo repositorio, comprobado con `git rev-parse`; esta noche, `<raiz-de-trabajo>\`. El SHA-256 del original, frente al registrado (para D1, el de `F4-SMOKE-2/run/integrity.md`), **antes** del respaldo. Después, el respaldo en `<carpeta-de-respaldo>`, las copias con `copy` desde el original y el manifiesto `copias.json`. **Ninguna escritura sobrescribe:** se para si el destino ya existe | Respaldo = original; copia = original; el original, igual antes y después. **Los originales nunca se abren** |
| P-07 | [S2] F4-E71, precondición 7 | Ningún Worker ni otra sesión en el worktree | Sin delegaciones abiertas |
| P-08 | OB-09.1; M-01 | La guía del operador, con los literales copiados del `src/` del Candidato, en local, y su versión redactada | Los rótulos de los A/B y de los contextos construidos, en la guía |
| P-09 | **Q-PBT5-2: «Contar antes en otros»**; [S2] `prep/selection.md` | **El recuento del estilo de cotas, antes de SPB-07:** de cada original de la selección de Smoke-2 con racks Push Back, además de D1 y D2, un respaldo y una copia, con la regla de P-06; cada copia abierta en la instancia de la sesión, solo para leer (`RCDUMP estilo`), y cerrada sin guardar; el recuento, con `dimkeys.py` de [BM] | Respaldo = original; copia = original; el original, igual antes y después. El recuento, por dibujo y sin nombres. **Si hay alguno con estilo:** dos copias más de ese dibujo para SPB-07 (a) y (b) |

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
| SPB-07 | D-PB-06 (b); OD-PBTC-2; F4B-E04, M-7; (b): A2-11, la mitad del estilo (M-15); **Q-PBT5-2: «Contar antes en otros»** | **Antes, P-09:** el recuento de los sobres con `DimensionStyle` no nulo en las copias de D1 y en las de los demás dibujos reales de la selección de Smoke-2, en solo lectura. (a) **Si alguno lo guarda:** en una copia nueva de ese dibujo, RACKEDITAR → «Actualizar» → el estilo de sus cotas.<br>(b) En otra copia nueva del mismo dibujo, lo mismo por «Editar todo» → «Actualizar» | El recuento por dibujo; el estilo de las cotas | **Si alguno lo guarda:** PASS si las cotas salen con el estilo guardado, en (a) y en (b); FAIL si no. **Si ninguno lo guarda:** el paso queda **PENDIENTE DEL OWNER**, que decide si acepta el criterio 3 de §3.3 cumplido en parte (es lo que dice su respuesta: «si ninguno, te vuelve»). No se fabrica | S |
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
| SPB-31 | 3 | «Editar todo» → cerrar sin cambios | Abre con el valor nuevo, sin aviso; al cerrar, «Editor cerrado sin cambios en el borrador.», «Limpio» y la caja editable | PASS / FAIL. Se anota, como información, OWNER-01 de PB-T4e (F4B-E61): con la ventana abierta, la paleta no se repinta y sigue mostrando «Limpio (posiblemente obsoleto)» y el valor de antes, hasta cerrarla | S |
| SPB-32 | 4 | 1 y 2 otra vez → «Editar todo» → un cambio → «Aplicar al borrador» → cerrar → «Actualizar» → `RCDUMP` | Sin «No se aplicó: el dibujo cambió fuera del panel» ni «Conservar mi borrador»; los dos cambios en el dibujo | PASS / FAIL | S |
| SPB-33 | 5 | Borrador Clean → borrar fuera todas las vistas del rack → «Editar todo» → Deshacer | No abre; «No editable aquí: el origen ya no está en el dibujo» (`OriginMissingDiagnostic`), la pista de RACKEDITAR y «No se recargó: el borrador se conserva tal como estaba.»; Deshacer restaura | PASS / FAIL | S |
| SPB-34 | 6; R-T4-39 | Copia y gemela. Un cambio propio → un cambio de fuera → «Editar todo» → «Actualizar». En la gemela, lo mismo con «Editar» | Abre sobre la base de antes; la respuesta, igual en las dos | PASS si coinciden. `NotAppliedPreflight` se cuenta al Owner como información | S |
| SPB-35 | 7; M-20 | «Editar todo» → cerrar sin «Actualizar»; `DBMOD` y Deshacer antes (después de la selección y de la vista previa) y después | Ningún cambio | PASS / FAIL | S |
| SPB-36 | 8; F4B-E51 y F4B-E53 (el coste de la lectura) | Con la ventana abierta, la paleta (K = 1); al cerrar, responde. El tiempo del clic a la ventana, con la marca y sin ella | La paleta deshabilitada; los tiempos | PASS en el modal; INFORMATIVO en el tiempo. **O:** la espera. Se anota, como información, OWNER-01 de PB-T4e (F4B-E61): con la ventana abierta, la paleta no se repinta y sigue mostrando «Limpio (posiblemente obsoleto)» y el valor de antes, hasta cerrarla | S+O |

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
| SPB-45 | OWNER-1 de PB-T3c (F4B-E48); **Q-PBT5-4: «Construirlos y rotularlos»** | **CONSTRUCTED REPRESENTATIVE TEST CASES.** Copia y gemela, las dos construidas igual: el lateral del último poste insertado con RACKEDITAR («Insertar lateral») y una segunda referencia de esa definición en la primera presentación, con `-LAYOUT` y `-INSERT` tecleados → panel: un frente menos → «Actualizar» → `RCDUMP`. En la gemela, la misma edición con RACKEDITAR → `RCDUMP` | Las dos referencias, la de modelo y la de papel, borradas con la definición; si hay aviso; igual que la gemela (MA H-063) | PASS si el panel hace lo mismo que el clásico. La falta de aviso, OBSERVADO (riesgo ya informado). FAIL si el panel deja una referencia sin definición o difiere del clásico sin declaración | S |
| SPB-46 | OWNER-2 de PB-T3c; [T3] §5.5 (1), «con una referencia anidada»; [BM] §3; **Q-PBT5-4** | **CONSTRUCTED REPRESENTATIVE TEST CASES.** Copia y gemela: el lateral insertado, metido en un bloque del usuario con `-BLOCK` tecleado (nombre `ZZ-CONSTRUCTED-SPB46`) e insertado otra vez con `-INSERT` → panel: un frente menos → «Actualizar» → `RCDUMP` (`rcblk` y `rcnest`). En la gemela, RACKEDITAR | Qué queda del bloque del usuario y de las definiciones anidadas (la purga de un nivel) | Como SPB-45 | S |
| SPB-47 | OWNER-3 de PB-T3c; M-21; M-28 | Recuento con `rcnest` en las copias, si pasaron los controles de `rcnest` y de `rcdyn`; si no, LIMITATION. Solo cuentan los contenedores dinámicos (`rcdyn` = 1) o anónimos `*U…`; el control positivo de D1, en un bloque pegado, no cuenta. Si hay alguno, lo mismo que SPB-46, sin construir nada | — | Si no hay: NOT AVAILABLE FOR PHYSICAL SMOKE | S |
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
| SPB-57 | [T3] §5.5 (4); `PushBackPilotUpdateRun.LockedLayerPrefix`; **Q-PBT5-4** | **CONSTRUCTED REPRESENTATIVE TEST CASES.** Copia y gemela: el lateral insertado y la capa de su referencia bloqueada con `-LAYER` → panel: un frente menos → «Actualizar» → `RCDUMP`. En la gemela, la misma edición con RACKEDITAR → `RCDUMP` | En el panel, un texto que empieza por «LOCKED_LAYER: » y nada escrito; en la gemela, lo que haga el clásico | PASS si el panel rechaza sin escribir nada. Lo del clásico se registra como hecho | S |
| SPB-58 | [T3] §5.5 (5), el último vínculo; [BM] M-2b; `PushBackPilotUpdatePlan.AllViewsObsoleteReason`; **Q-PBT5-4** | **CONSTRUCTED REPRESENTATIVE TEST CASES.** Copia y gemela, sobre un rack sin referencias anidadas: el lateral insertado; `ERASE` de las demás referencias del rack y `-PURGE` de sus definiciones, tecleados (el método de [BM] M-2b) → seleccionar el lateral → panel: un frente menos → «Actualizar» → `RCDUMP`. En la gemela, RACKEDITAR | En el panel, «Todas las vistas del rack quedarían obsoletas: no se elimina el último vínculo del rack.» y nada escrito; en la gemela, lo medido en [BM] M-2b | PASS si el panel rechaza sin escribir nada | S |
| SPB-59 | [T3] §5.5 (6); [T3] `:915` (la cuenta incluye `EraseUnit`); [S2], pasos 78 a 80; R-PB-03; **Q-PBT5-10: «Hacerlas como en Smoke-2»** | Un AutoCAD propio, lanzado con `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT=2` **solo en el entorno de ese proceso**, con una copia con el lateral insertado → panel: un frente menos (la segunda unidad puede ser el borrado) → «Actualizar» → `RCDUMP` → «Descartar» → `QUIT` → «No». Antes y después, ninguna variable en el usuario, la máquina ni el shell | «No se aplicó: La escritura falló antes de confirmarse: …»; ninguna vista cambia ni se borra; una entrada de Deshacer | PASS si el aborto no deja nada escrito ni borrado | S |
| SPB-60 | [T3] §5.5 (6), el sellado; [S2], paso 82; **Q-PBT5-10** | Lo mismo que SPB-59, en otro AutoCAD propio, con `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL=before` | El estado desconocido, «Comprobar ahora» y que nada cambió | Como el paso 82 de [S2] | S |
| SPB-61 | [T3] §5.5 (6); [S2], paso 84; **Q-PBT5-10** | Lo mismo, en otro AutoCAD propio, con `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL=after` | El estado desconocido; la escritura confirmada, con el borrado incluido | Como el paso 84 de [S2] | S |

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

**Las decisiones del Owner que fijan pasos** (`owner-pbt5-decisions.md`):
- SPB-45, 46, 57 y 58 se construyen y se rotulan (Q-PBT5-4);
- SPB-59 a 61 se hacen como en Smoke-2 (Q-PBT5-10);
- SPB-07 va detrás del recuento de P-09, y si ningún dibujo guarda un estilo, vuelve al Owner (Q-PBT5-2).

**LIMITATION ya previstos:** SPB-22, SPB-43 y SPB-65. SPB-47, SPB-48 y SPB-56 lo serán si no hay casos reales. SPB-07 sin casos reales no es LIMITATION: vuelve al Owner.

### 4.4 Lo que el Smoke-PB no cubre

- **Guardar la copia y reabrirla:** ningún registro de F4B lo pide.
- **La carrera de cierre a mitad de transacción:** la de F4, no provocada. F4B no toca el cierre.
- **El nombre del grupo de Deshacer y el número de transacciones:** NO OBSERVABLE ([BM] §2).
- **Corrección de la v1:** las inyecciones UNIT y SEAL **sí** entran (R-PB-03; [T3] §5.5, (6)). Son SPB-59 a SPB-61, y el Owner las autorizó (Q-PBT5-10).

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
| OWNER-01 de PB-T4e: la paleta no se repinta con la ventana abierta (informativo) | F4B-E61 | SPB-31 y SPB-36 |
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

Las órdenes exactas están en [SR]. Aquí van las reglas.

### 5.1 Los dibujos

- **Solo copias de trabajo.** Los originales nunca se abren, ni con AutoCAD ni con `accoreconsole`: solo se leen para copiarlos.
- **Antes de usar un original,** un respaldo, comprobado por su SHA-256. D1 se compara además con el SHA-256 de `F4-SMOKE-2/run/integrity.md` antes de escribir nada.
- **Cada copia sale del original.** Nada se guarda sobre un original, y las copias se cierran sin guardar.
- **Las carpetas,** `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>`, son nuevas (si `<raiz-de-trabajo>\` ya existe, se para) y quedan fuera del perfil del usuario, del worktree, del checkout principal y de todo repositorio, comprobado con `git rev-parse`.
- **Ninguna escritura sobrescribe** un archivo existente.
- **Las salidas del producto,** solo en `<carpeta-del-Smoke-PB>\salidas`.
- **Privacidad:** sin nombres ni rutas de clientes. Los JSON crudos se quedan en local, con su SHA-256.

### 5.2 El DLL

- **El DLL Debug del worktree,** compilado en el Candidato con AutoCAD cerrado, con los comandos de [G] §2 y el `dotnet` del usuario, desde la raíz del worktree.
- **Antes de NETLOAD,** la `ProductVersion` termina en `+<Candidato>`, y se registra el SHA-256 de los cuatro DLL.
- **El build va antes del commit de la emisión** (§3.1).

### 5.3 La configuración de AutoCAD, la carga y el inventario

- **No se toca nada:** ni Trusted Locations, ni SECURELOAD, ni «Always Load», ni el registro, ni ninguna variable de sistema. Tampoco «Always ignore» ni la casilla de «Proxy Information».
- **La carga,** como en [G] §3: NETLOAD del DLL exacto, por expresión, y «Load Once» en el aviso.
- **El inventario,** con las funciones de [BM], tecleadas en cada documento, más `rcblk`, `rcdyn`, `rcnest` y `rcdynchk`, de solo lectura, con sus controles. `RCDUMP` no sobrescribe.
- **El paso de una copia a la siguiente,** sin COM (I-07 de [IR1]):
  1. `vla-open` de la siguiente desde la activa;
  2. `EXTERNALREFERENCESCLOSE`;
  3. el inventario;
  4. `vla-close … :vlax-false` de la anterior.
- **La instancia de AutoCAD de la sesión** se registra al lanzarla: el PID, la ruta y la hora de inicio en `Ticks`.
  - Cada bloque que la toca comprueba esos tres datos.
  - Si se queda sin documentos, se cierra con `CloseMainWindow`, nunca con `Stop-Process`.
- **Cada bloque de PowerShell funciona solo** (I-08): `Set-StrictMode`, `Stop`, sus variables y sus comprobaciones, y `exit 0` al final.

### 5.4 Quién opera

- **La frontera de esta noche** (la lectura de la sesión principal del mensaje del Owner; Q-PBT5-5 no se le preguntó):
  - la sesión opera el Smoke-PB contratado **solo si puede empezar antes de las 13:00 UTC** del 2026-11-23;
  - **a las 13:00 UTC para** en el siguiente límite de paso y espera al Owner. AutoCAD y la copia abierta se quedan como estén, sin guardar, y solo se hacen SPB-51 y SPB-52, que leen.
- **Después de la frontera,** o si el Owner aparece:
  - **el Owner** opera los gestos del producto de los pasos que quiera;
  - **la sesión** mide lo de máquina, avisa antes de teclear y devuelve el control después.
- **Lo que se le deja al Owner, siempre:**
  - **los juicios visuales de los 9 S+O,** con su captura (SPB-08, 09, 16, 18, 20, 23, 25, 36 y 39), como PENDIENTE DE REVISIÓN DEL OWNER;
  - **SPB-07,** si ningún dibujo guarda un estilo;
  - **los OBSERVADO,** con su evidencia;
  - **la disposición,** después de la revisión independiente de la evidencia;
  - **la firma.**
- **Los resultados que registra la sesión son propuestas.**

### 5.5 Las inyecciones (Q-PBT5-10, «Hacerlas como en Smoke-2»)

- **Tres corridas,** SPB-59 a SPB-61, cada una con su copia y **un AutoCAD propio por inyección**, lanzado por la sesión.
- **La variable,** `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT=2`, `…_SEAL=before` o `…_SEAL=after`, **solo en el entorno de ese proceso** (`ProcessStartInfo.Environment`), nunca en el usuario, la máquina ni el shell.
  - **Antes y después de cada corrida,** se comprueba que no está en ninguno de los tres.
  - Ninguna otra instancia de la sesión puede estar viva al lanzarla.
- **Cada corrida termina** con «Descartar borradores de este documento» → «Descartar» → `QUIT` → «No», y la comprobación de que la instancia salió.
- **Nada cambia el producto.** Las órdenes exactas, en [SR] §4.11.

### 5.6 Los contextos construidos (Q-PBT5-4, «Construirlos y rotularlos»)

- **Los cuatro de SPB-45, 46, 57 y 58,** en copias, con órdenes nativas tecleadas: `-LAYOUT` y `-INSERT`, `-BLOCK`, `-LAYER`, y `ERASE` con `-PURGE`.
- **Cada uno se compara con RACKEDITAR** en una gemela construida igual.
- **Cada uno va rotulado CONSTRUCTED REPRESENTATIVE TEST CASES** en la guía, el registro, las etiquetas de `RCDUMP` y el nombre de las capturas.
- **La línea entre preparar y construir:**
  - insertar una vista con RACKEDITAR es un gesto del producto, y se usa como preparación;
  - poner el dibujo con órdenes nativas en un estado que el producto no crea es construir.
- **Las órdenes exactas,** en [SR] §4.12.

### 5.7 El recuento del estilo de cotas (Q-PBT5-2, «Contar antes en otros»)

- **Antes de SPB-07,** en la precondición P-09: de cada original de la selección de Smoke-2 con racks Push Back, un respaldo y una copia.
- **Cada copia se abre** en la instancia de la sesión, solo para `RCDUMP estilo`, y se cierra sin guardar.
- **El recuento** se hace fuera de AutoCAD, con `dimkeys.py` de [BM].
- **Si alguno guarda un estilo,** SPB-07 se hace con dos copias nuevas de ese dibujo. **Si ninguno,** SPB-07 vuelve al Owner.
- **Las órdenes,** en [SR] §4.13.

### 5.8 Las reglas de operación, la evidencia y las capturas

- **Las reglas de operación por control de pantalla,** como en la v2.2 (§5.5):
  - trozos de 16 caracteres como máximo;
  - una captura antes de cada clic;
  - coordenadas y captura de después en cada resultado negativo;
  - nunca Ctrl+V;
  - N-I1;
  - dos repeticiones como máximo por error del operador.
- **La evidencia,** en `I-64-pilot/F4B-SMOKE-PB/` (`prep/` y `run/`), como en la v2.2 (§5.6).
  - La cabecera del registro lleva el sha256 de [SR], de este paquete y de la guía redactada.
  - La sección «Para tu revisión» lleva los 9 S+O.
- **Las capturas,** como en [SR] §8:
  - las crudas, nunca custodiadas;
  - las custodiadas, recortadas o tapadas con un rectángulo opaco, con la lista de antes de custodiar y `derivation-record.txt`;
  - nunca la paleta de referencias externas, el aviso de NETLOAD ni los diálogos de archivos.

### 5.9 Lo que no se finge

- **Ninguna condición interna que AutoCAD no muestre** (SPB-22 y SPB-65), ningún arnés y ningún código de producto para probar.
- **Ninguna escritura en el sobre de un rack fuera del producto:** el estilo de cotas no se fabrica.
- **Las variables `RACKCAD_DEBUG_*`,** solo en el entorno de los tres AutoCAD de SPB-59 a SPB-61.
- **Ningún caso fabricado** salvo los A/B de D-PB-09 y los cuatro contextos de Q-PBT5-4, rotulados.

---

## 6. El contrato (forma B, la que eligió el Owner)

### 6.1 La forma

- **El Owner eligió «Adaptación con revisión»** (Q-PBT5-8):
  - contrato propio, con invariantes, paradas, evidencia y presupuesto;
  - sin Controller, sin nc1 a nc4 y sin D2;
  - **antes de su firma, un revisor aparte comprueba en solo lectura que el registro, las capturas y los inventarios coinciden.**
- **El contrato es este §6.** El guion de la emisión lo saca tal cual a `F4B-PBT5-SMOKE-PB-GATE-CLOSE/coordinator/contract.md` (§6.7).
- **La v3 del sustituto de nc2** (F4B-E58) se eligió con una opción que decía «la aplico a este nc2 y a PB-T5». Con la forma B no hay nc2 en PB-T5, así que no se usa. La forma B la eligió el Owner después, sabiendo que no tiene nc1 a nc4.
- **El cierre es por la firma** ([F] §9; F4-E81), tras la revisión de la evidencia.

### 6.2 La identidad y el alcance de escritura

- **`TaskId`:** `F4B-PBT5-SMOKE-PB-GATE-CLOSE`. La emisión lo fija.
- **En el repositorio,** solo:
  - `I-64-pilot/F4B-SMOKE-PB/`;
  - `I-64-pilot/F4B-PBT5-SMOKE-PB-GATE-CLOSE/` (el contrato, la revisión de la evidencia y el paquete de cierre);
  - `F4B-DRAFTS/pbt5/`, `F4B-DRAFTS/coordinator/` y `F4B-DRAFTS/gate-close.md`;
  - [DISP], `I-64-evidence.md` e `I-64.yml`;
  - `docs/initiatives/I-64-f4b-pbt5-package.md`, este paquete;
  - `docs/ideas-futuras.md`, solo por orden del Owner.
- **Fuera:** `<raiz-de-trabajo>\` y el `bin/` y el `obj/` del worktree.
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
| INV-PBT5-01 | `git diff <Candidato> <punta del cierre de F4B> -- src tests assets` vacío |
| INV-PBT5-02 | El DLL sale del Candidato: su `ProductVersion` lo identifica, y su SHA-256 se registra antes de NETLOAD y se comprueba en el proceso |
| INV-PBT5-03 | Solo copias; los originales, con respaldo y sin abrir |
| INV-PBT5-04 | Ningún cambio de configuración: «Load Once» y ningún «Always …» |
| INV-PBT5-05 | Nada se finge. Ninguna `RACKCAD_DEBUG_*` en el usuario, la máquina ni el shell; solo en el entorno de los AutoCAD de SPB-59 a SPB-61 (Q-PBT5-10) |
| INV-PBT5-06 | Los A/B y los cuatro contextos construidos llevan el rótulo **CONSTRUCTED REPRESENTATIVE TEST CASES**, y cada contexto construido tiene su gemela clásica (Q-PBT5-4) |
| INV-PBT5-07 | Los resultados de la sesión son propuestas. F4B se cierra solo con la firma del Owner, después de la revisión independiente de la evidencia (Q-PBT5-8) |
| INV-PBT5-08 | La evidencia custodiada no lleva rutas locales, nombres de clientes ni datos personales |
| INV-PBT5-09 | [G] no cambia |
| INV-PBT5-10 | Sin integración en `main`; HANDOFF y ROADMAP sin cambios |
| INV-PBT5-11 | Una captura antes de cada clic, abierta y revisada; coordenadas y captura de después en cada resultado negativo |
| INV-PBT5-12 | N-I1 |
| INV-PBT5-13 | Las salidas del producto, solo en `<carpeta-del-Smoke-PB>\salidas` |
| INV-PBT5-14 | Ningún FAIL se corrige en PB-T5 |
| INV-PBT5-15 | La UI completa no corre en local: la dispensa de Smoke-2, extendida por el Owner (Q-PBT5-1). La regla de F4-E59 no se levanta |
| INV-PBT5-16 | Cada bloque de PowerShell funciona solo, para si un valor está vacío o una ruta no existe, nunca escribe en la raíz de una unidad y termina con `exit 0` |
| INV-PBT5-17 | El estilo de cotas no se fabrica. Si ningún dibujo real lo guarda, SPB-07 vuelve al Owner (Q-PBT5-2) |

### 6.4 Paradas

Son las de la v2.2 (§6.4: PBT5-C01 a PBT5-C15), con estos cambios:
- **PBT5-C04:** una `RACKCAD_DEBUG_*` en el usuario, la máquina o el shell; o en el entorno de un AutoCAD que no sea el de su inyección.
- **PBT5-C11:** a las 13:00 UTC, se para en el límite de paso y se espera al Owner; después, solo SPB-51 y SPB-52, que leen.
- **PBT5-C15** (Q-PBT5-8, confirmada): una prueba que falla en la CI o en local sobre el Candidato vuelve al Owner. **La CI en rojo solo por el detector de cuelgue en una prueba lenta conocida** admite una reejecución de los trabajos fallidos sobre el mismo SHA; otra causa, o un segundo rojo, vuelve al Owner.
- **PBT5-C16, nueva:** la revisión independiente de la evidencia encuentra que el registro, las capturas o los inventarios no coinciden. Vuelve a la sesión, que corrige el registro o repite el paso, sin cambiar un resultado, y otra vez a la revisión. Si hace falta repetir un paso en AutoCAD, es una adenda, que no gasta intento (§6.6).
- **PBT5-C17, nueva:** una de las tres condiciones de la emisión condicional (Q-PBT5-7) no se cumple. No se emite, y vuelve al Owner.

### 6.5 La evidencia esperada

- `I-64-pilot/F4B-SMOKE-PB/prep/` y `run/`;
- `I-64-pilot/F4B-PBT5-SMOKE-PB-GATE-CLOSE/coordinator/contract.md`, la revisión independiente de la evidencia y su encargo, y el paquete de cierre;
- `F4B-DRAFTS/pbt5/`: las versiones del paquete y de las preguntas, las revisiones, [SR] redactada, las guías de la corrida previa como historia, los traspasos y las herramientas.
  - Las `_tools/*.local.txt` no se custodian.
  - Lo que case un patrón prohibido tras la redacción va solo por su sha256 ([DISP] F4B-E60).
- `F4B-DRAFTS/coordinator/owner-pbt5-decisions.md` y, después, la disposición y la firma del Owner;
- `F4B-DRAFTS/gate-close.md`, con la tabla de los INV;
- F4B-E62 (la emisión), la §292 y las entradas y secciones siguientes; los cambios de `I-64.yml`.

### 6.6 El presupuesto (Q-PBT5-9, «Definición y reglas»)

- **1 de 3** (OB-10.1); el intento 3, siempre al Owner (OB-10.2).
- **Un intento es una corrida del Smoke-PB sobre un Candidato.**
- **No cuentan:**
  - repetir un paso por error del operador (dos veces como máximo);
  - una parada del entorno antes de abrir AutoCAD;
  - una adenda que pida el Owner al revisar.
- **Se adoptan las tres reglas de Smoke-2:**
  - las etiquetas: NOT AVAILABLE FOR PHYSICAL SMOKE; NO VERIFICABLE FÍSICAMENTE / CUBIERTO POR PRUEBA AUTOMATIZADA; OBSERVADO;
  - traer la evidencia antes de decidir un OBSERVADO;
  - una copia de trabajo por escenario.
- **Una tarea de corrección** que nazca de un FAIL tiene su propio contrato y su 1 de 3.

### 6.7 La cadena

1. **Esta v2.3 y su comprobación acotada,** que tiene que dar 0 bloqueantes y 0 importantes sin resolver. Su resultado va en una línea `BOUNDED_RESULT:`, que lee el guion.
2. **El cierre de PB-T4e,** hecho: F4B-E61 y §291, en `ebd304a3`, que es el Candidato. La CI de push del Candidato tiene que terminar 4/4 (`45147867500`).
3. **`emit_pbt5.py --dry`,** que corre la sesión. Comprueba:
   - la numeración: existen F4B-E61 y la §291, y no existen F4B-E62 ni la §292;
   - el Candidato, su árbol y la CI exacta en verde;
   - la línea `BOUNDED_RESULT:`;
   - los sha256 de lo que se custodia y la redacción.

   Escribe su listado.
4. **La preparación y el build en el Candidato** ([SR] §3 y §4.1 a §4.3).
5. **`emit_pbt5.py --commit`,** que corre la sesión:
   - compara con el listado del `--dry`;
   - copia con la redacción;
   - saca el contrato (§6) a `contract.md`;
   - añade F4B-E62 y la §292;
   - cambia `I-64.yml`;
   - y hace el commit documental y lo sube, sin forzar.

   **Es la emisión,** por la autorización condicional del Owner (Q-PBT5-7).
6. **El Smoke-PB contratado** ([SR]), con la frontera de §5.4.
7. **La custodia** de la corrida, redactada, en un commit documental de la sesión.
8. **La revisión independiente de la evidencia** (Q-PBT5-8), en solo lectura.
9. **La disposición del Owner** (los S+O, los OBSERVADO, SPB-07 si toca) **y su firma.**
10. **El gate-close** (§3.3).

---

## 7. Riesgos

Son los de la v2.2 (§7), con estos cambios:
- **R-1:** la corrida es larga (65 pasos y 44 copias, más las de P-09). La frontera de las 13:00 UTC la corta, y se sigue con el Owner. Los bloques son independientes, y el orden de [SR] deja para el final las inyecciones, que necesitan sus propios AutoCAD.
- **R-15, nuevo: una variable de depuración que se escapa del proceso de su inyección.** Se lanza con `ProcessStartInfo.Environment`, y antes y después se comprueban el usuario, la máquina y el shell. Ninguna otra instancia de la sesión puede estar viva al lanzarla.
- **R-16, nuevo: un contexto construido que se toma por real.** El rótulo, la gemela clásica y la línea entre preparar y construir (§5.6).
- **R-17, nuevo: recompilar el DLL después del commit de la emisión.** Cambiaría la `ProductVersion`, y el bloque 7 de [SR] lo detectaría. El build va antes del commit (§3.1).

---

## 8. Supuestos declarados

- **S-1:** la frontera de esta noche es la lectura de la sesión principal del mensaje del Owner (§5.4). El Owner no la confirmó, porque Q-PBT5-5 no se le preguntó.
- **S-3:** el Candidato es la punta del cierre de PB-T4e, desde la que se compila el DLL, y el commit de la emisión va encima (§3.1).
- **S-4 a S-7, S-10 y S-11:** como en la v2.2 (§8). S-7, una copia por escenario, deja de ser un supuesto: el Owner la adoptó (Q-PBT5-9).
- **S-12, nuevo:** la emisión de PB-T5 es un solo commit documental, F4B-E62 y la §292. Lleva el paquete, el registro del Owner, el contrato como documento y la historia. Las tareas de F4B con `gate-contract.json` emitían en dos commits (X y BASE), pero sin contrato JSON no hay BASE que generar.

---

## 9. Las decisiones del Owner, y lo que le queda

- **Decididas** ([OD5]): Q-PBT5-7, Q-PBT5-8, Q-PBT5-9, Q-PBT5-1, Q-PBT5-10, Q-PBT5-4 y Q-PBT5-2.
- **No preguntada:** Q-PBT5-5. La corrida previa no se hizo.
- **Lo que le queda al Owner en PB-T5:**
  - los 9 juicios S+O;
  - SPB-07 si ningún dibujo guarda un estilo;
  - los OBSERVADO;
  - una decisión nueva, una prueba que falle o un intento 3;
  - la disposición, después de la revisión de la evidencia;
  - la firma.
- **Información, como en la v2.2:**
  - D-PB-11 es obligatorio;
  - H-31 y OWNER-1 de PB-TC2 los añade la sesión;
  - R-T4-39.

---

## 10. Lo no verificado

- **La CI de push del Candidato** (`45147867500`): en curso al escribir esto.
- **Sin AutoCAD, sin verificar:**
  - `vla-item` por nombre;
  - `rcdyn` y `rcdynchk`;
  - `WaitForInputIdle`;
  - que `StartTime` esté disponible justo después de `Start`;
  - las órdenes nativas de los contextos construidos (`-LAYOUT`, `-INSERT`, `-BLOCK`, `-LAYER` y `-PURGE`) con sus respuestas;
  - que `QUIT` cierre limpio la instancia de cada inyección.
- **Si se llega a «Conservar»** (R-2), si se alcanza escenificar un módulo (SPB-21), si hay un rack real con un diagnóstico bloqueante (SPB-56) y si la copia enlazada de D1 sigue teniendo el mismo texto (SPB-63).
- **Los originales** de D1 y de los demás dibujos de la selección de Smoke-2: dónde están hoy y sus SHA-256.
- **Que la herramienta deje correr en segundo plano** las órdenes de más de 10 minutos.
- **La duración real** de la corrida.
- **El guion de la emisión** no se ha ejecutado, ni en `--dry`.

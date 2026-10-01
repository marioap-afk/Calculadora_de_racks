# I-64 — Paquete de re-revisión del Architect (Proposal V2)

```text
PROPOSAL V2 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED
Architect      = RE-REVIEW REQUIRED por el MISMO Architect que emitió A64-PV1-01..22 (sesión separada «I-64 Architect Review Proposal V1»)
Consensus      = NOT REACHED
Freeze         = NONE
Implementation = NOT AUTHORIZED

Objeto de la re-revisión = docs/initiatives/I-64-proposal-v2.md, en el commit que publica este paquete (rama
                           architecture/workspace-persistente-rackcad; el SHA no se escribe aquí para no autorreferenciarse:
                           se toma de `git log -1 -- docs/initiatives/I-64-proposal-v2.md`)
Versión anterior         = docs/initiatives/I-64-proposal-v1.md, commit dcc16bed371207c8c68dd6bc6a7dd7e5a7acee6d,
                           blob 962fc1958f3b6885d43eff3a94190325edaf850f (sin cambios en este commit)
Veredicto previo         = CHANGES REQUIRED: A64-PV1-01..22 REQUIRED; O-01..O-17 OPTIONAL (evidencia §14)
Orden del Coordinator    = F0 / Proposal V2, con las precisiones vinculantes PV2-01..PV2-22 y la materialidad aceptada (evidencia §14)
Discovery base           = docs/initiatives/I-64-discovery.md, versión D1-R1, blob 3565015c21a44b08df564b3277b5546a8a6ce013 (cerrado)
Base de main             = 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Qué es este paquete.** El material para la re-revisión: la versión completa (Proposal V2, autocontenida), el **delta explícito**
> respecto de V1 y la **disposición por ID** de los hallazgos previos (LIFECYCLE §5). El delta es el foco mínimo, no un límite: un hallazgo
> material en una sección intacta es válido. La sesión autora no emite ni simula el veredicto, no declara consenso, Freeze ni GATE PASS, y no
> afirma la independencia de nadie.

## 1. Veredicto que se solicita

```text
Architect: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Por cada A64-PV1-nn: CLOSED | STILL OPEN (con razón) — solo el emisor puede cerrarlo o rebajarlo
Hallazgos nuevos: A64-PV2-nn REQUIRED | OPTIONAL, con sección, evidencia y cambio exigido
Modo: SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN; si revisor y autor son la misma persona
Versión: el veredicto vale solo para el SHA y el blob exactos revisados
```

## 2. Lectura mínima

1. [Proposal V2](I-64-proposal-v2.md), completa, en especial §18 (disposición por ID) y los anexos A y B.
2. Este paquete: delta (§3), disposición (§4) y preguntas (§5).
3. [Evidencia](../automation/evidence/I-64-evidence.md) §14: orden V2 del Coordinator, procedencia del veredicto V1, CI de V1, aviso de I-63
   sobre P-14 y puntas de las ramas.
4. Código de la base citado por la V2: `ISiblingRedrawPort.cs`, `RackSiblingRedrawRun.cs`, `SiblingRedrawTransaction.cs`,
   `SiblingRedrawDebugFaultInjection.cs`, `CallerOwnedFacadeGuardTests`, `RackSiblingMembership.cs`, `RackVariablesCommands.cs:47-76`,
   `RackCantileverCommands.cs:158-168`, `RackSelectivoCommands.cs` y `RackSelectivoInsertIntegration.cs:81-85`.
5. I-55 V5 §4.4-§4.10 y la guía de OV de I-55 (OV-RED-06).

## 3. Delta explícito V1 → V2

| Sección | V1 | V2 |
|---|---|---|
| Cabecera y §14 | M-02 y M-03 UNKNOWN tratados como activados | M-02 y M-03 NOT ACTIVATED con condiciones, aceptado por el Coordinator; el resto ACTIVATED |
| §0 | glosario de tres niveles | añade StaleBase, InitialDraftState, CurrentDraftState, Dirty, sesión, modal RackCad activo y eco |
| D-01 | `KeepFocus = false` congelado | obligación de foco congelada sin mecánica; API interna solo en el adaptador; acuse de I-52 y diff de solo lectura del perfil antes de Smoke-1 |
| D-02 | operaciones acotadas | + `TopTransaction == null` Debug; panel oculto sin lecturas; estado sin documento; subdiálogos con la API modal |
| D-03 | identidad por puntero no gestionado | identidad de instancia abierta; dos aperturas = dos sesiones; SAVEAS conserva; sin reemparejar; peticiones y ejecuciones ligadas a la sesión |
| D-04 | «durante un modal no hay eventos»; manejadores filtran y anotan | hecho/inferencia/obligación; ninguna corrección depende de la ausencia de eventos; manejadores solo encolan; drenaje en reposo y sin modal RackCad, con operaciones enumeradas; ciclo de vida de suscripciones; ámbito de mutación propia por (sesión, miembros); modo degradado retira suscripciones |
| D-05 | F2 consume el snapshot; resecuenciar por A-n | solo **F6** consume el snapshot; STOP al Master al agotar F1..F5; sin integración parcial sin el Owner; conflicto con P-14 declarado |
| D-06, D-07 | INV-03 «el contexto no cambia»; eco sin especificar | ningún evento escribe selección o vista; eco por contenido sin cambios de pestaña, preview ni lista; `UNKNOWN` de la selección tras el comando interno |
| D-08 | guardia que actualizaba la preview «en segundo plano»; una superficie | la primera pulsación fija la preview; K superficies (K = 1); estado Error |
| D-10 | dirty = borrador ≠ base | StaleBase / InitialDraftState / CurrentDraftState / dirty separados; base e inicial de una sola lectura en la intención de edición; la preview no captura AUTH-06; fronteras bloqueables y no bloqueables |
| D-11 | admisión «al abrir» | admisión en la lectura de la intención de edición; coste AUTH-06 situado; medición de admisión |
| D-12 | comparaba clase Redraw/Erase y «entradas de escaneo» | compara atribución MUTABLE/READ_ONLY/BLOCKING/NOT_MEMBER; Redraw/Erase al aplicar; registro de Selectivo completo, sin entradas de escaneo; comprobación previa antes de importar; H-LOCK reformulada |
| D-13 | importación dentro de la transacción y primitivas anidadas; renombrado en MUTATE; excepción de `Commit()` = «ninguna escritura» | resultado todo o nada congelado sin mecánica; costura de I-55 como vía implementable; importación en PREPARE (residuo OM-5); renombrado en POST; clasificación por frontera de `Commit()`; Cantilever y Cama con variante caller-owned; un Actualizar = un paso de UNDO |
| D-15 | diferimiento por A-n del Coordinator; INV-19 parcial | diferimiento `OWNER-RESERVED`; caracterización D13 y D9 completa; escritor caller-owned como obligación del adaptador |
| D-17 | veto del primer intento y descarte en el segundo; sin contención de excepciones | veto en todo intento hasta el descarte explícito (panel y comando); pérdida visible si no se respeta; contención de toda entrada del panel |
| D-18 | P-01 «cerrado» ambiguo; nueve escenarios | P-01 nunca abierto y P-01b oculto; escenario 10 masivo (≥ 10⁴ objetos) en tres estados |
| D-20 | residual de ID3 = lotes | residual = lotes + sistemas no integrados |
| §10 | INV-01..21 | INV-01..21 corregidos e INV-22..33 nuevos |
| §11 | F2 = inventario + navegación (con snapshot); Smoke-1 y Smoke-2 | **F2 = navegación sin snapshot → Smoke-NAV**; F3 incluye Selección (N); **F6 = inventario y navegador (snapshot)**; F7 incluye la evaluación de ID3; secuencia de cierre de F4 con Smoke-2 |
| §12 | Smoke-1 y Smoke-2 básicos; OV-01..23 | Smoke-1 con oráculos de foco y diff de perfil; Smoke-NAV; Smoke-2 con rollback, Desconocido, avisos, admisión, H-LOCK y undo; inyecciones Debug por variable de entorno del proceso; OV-24..26 |
| §15 | tips de V1 | puntas actuales y conflicto MASTER-I63-I64-01 / P-14 |
| §18 | — | disposición de A64-PV1-01..22 y O-01..O-17 |
| Anexos | ADR con veto de un intento; nota a ADR-0032 que llamaba fronteras D6 a las no bloqueables | ADR actualizado; nota a ADR-0032 reescrita |

## 4. Disposición por ID

La tabla completa, con sección por hallazgo, está en la Proposal V2 §18. Resumen:

| ID | Estado propuesto | Dónde |
|---|---|---|
| A64-PV1-01 | atendido | D-12, D-13, D-15; INV-12, INV-30 |
| A64-PV1-02 | atendido | D-17; INV-20; S2-06 |
| A64-PV1-03 | atendido | D-03; INV-22; SN-08 |
| A64-PV1-04 | atendido | D-04; INV-23 |
| A64-PV1-05 | atendido | D-10; INV-27 |
| A64-PV1-06 | atendido | D-12; INV-28 |
| A64-PV1-07 | atendido | D-12; INV-29 |
| A64-PV1-08 | atendido | D-10; INV-26 |
| A64-PV1-09 | atendido | D-06, D-07; INV-03; SN-06 |
| A64-PV1-10 | atendido | D-01; INV-24; S1-06a..h |
| A64-PV1-11 | atendido | D-08, D-17; INV-25 |
| A64-PV1-12 | atendido | D-08, D-10; INV-04, INV-18; anexo B |
| A64-PV1-13 | atendido | D-18; INV-33; S1-10 |
| A64-PV1-14 | atendido | D-04; INV-02 |
| A64-PV1-15 | atendido | D-02; INV-14 |
| A64-PV1-16 | atendido | D-15; INV-19 |
| A64-PV1-17 | atendido | §11 (secuencia de F4); §12.4 |
| A64-PV1-18 | atendido | D-07; §11 (F2); §12.3 |
| A64-PV1-19 | atendido | D-01; S1-11 |
| A64-PV1-20 | atendido | D-15, D-20 |
| A64-PV1-21 | atendido | D-05; §11; §15 |
| A64-PV1-22 | atendido | D-13; INV-31; OV-24..26 |
| C64-PV1-01..04 | dispuestos por el Architect en A64-PV1-02, 03, 04 y 01; atendidos por ellos | §18 |
| O-01..O-17 | quince adoptadas; O-02 no adoptada; O-09 adoptada solo en su medición | Proposal §18 |

## 5. Preguntas para la re-revisión

1. **Reestructuración de gates (§11).** La navegación pasa a F2 sin snapshot y el navegador del documento a F6, para que Smoke-NAV no
   dependa de I-63 y el STOP de PV2-21 llegue solo tras F1..F5. ¿Es aceptable este cambio respecto del plan del brief?
2. **Eco (D-07).** ¿Basta reconocer el eco mientras la selección de AutoCAD siga siendo igual al conjunto escrito por la acción, con la
   primera selección distinta reanudando la sincronización?
3. **Intención de edición (D-10).** ¿Es correcto que la pulsación que dispara la lectura no se aplique si el sobre cambió desde la preview,
   o debe exigirse además una confirmación?
4. **Clasificación por frontera (D-13, INV-30).** ¿Es aceptable que el panel distinga la excepción de `Commit()` con un cambio aditivo que
   no altere la clasificación de la costura para Insertar?
5. **Comprobaciones de host (§12.4).** ¿Son suficientes S2-09..S2-13, con inyecciones Debug por variable de entorno del proceso? ¿Es
   aceptable dejar `UNKNOWN` el caso de H-LOCK con otros complementos?
6. **O-02 no adoptada.** ¿Exige el Architect quitar los reactores de objeto con el panel oculto antes del Freeze?
7. **Conflicto con P-14 (§15).** ¿Debe resolverse antes del Freeze, dado que I-63 ya no será autor del snapshot y F6 depende de él?

## 6. Lo que este paquete no hace

No realiza la re-revisión. No declara AGREED, consenso, Freeze, GATE PASS ni independencia. No autoriza implementación, delegaciones,
sondas, pilotos ni ningún uso de AutoCAD. `IMPLEMENTATION AUTHORIZATION = NO`.

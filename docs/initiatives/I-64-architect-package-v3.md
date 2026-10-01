# I-64 — Paquete de re-revisión del Architect (Proposal V3)

```text
PROPOSAL V3 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED
Architect      = RE-REVIEW REQUIRED por el MISMO Architect que emitió A64-PV1-01..22
Consensus      = NOT REACHED
Freeze         = NONE
Implementation = NOT AUTHORIZED

Objeto de la re-revisión = docs/initiatives/I-64-proposal-v3.md, en el commit que publica este paquete (rama
                           architecture/workspace-persistente-rackcad; el SHA no se escribe aquí para no autorreferenciarse:
                           se toma de `git log -1 -- docs/initiatives/I-64-proposal-v3.md`)
Versiones anteriores     = V1: commit dcc16bed371207c8c68dd6bc6a7dd7e5a7acee6d, blob 962fc1958f3b6885d43eff3a94190325edaf850f
                           V2: commit ab4efe865638cbe121c0ecd232a689563ba7ee6a, blob 7bf98cca583c4b5c0ea2c090f2b239cdfd7f8239
                           (ambas sin cambios en este commit; V2 no llegó a re-revisarse)
Veredicto previo         = CHANGES REQUIRED sobre V1: A64-PV1-01..22 REQUIRED; O-01..O-17 OPTIONAL (evidencia §14)
Órdenes del Coordinator  = Proposal V2 (PV2-01..PV2-22; evidencia §14) y Proposal V3 (MASTER-I63-I64-02 y plan de gates; evidencia §15)
Discovery base           = docs/initiatives/I-64-discovery.md, versión D1-R1, blob 3565015c21a44b08df564b3277b5546a8a6ce013 (cerrado)
Base de main             = 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

**Identidad del revisor** (la misma del veredicto sobre V1, según lo declaró el propio Architect):

| Campo | Valor |
|---|---|
| Rol | Architect |
| Modo | SEPARATE SESSION; revisor = autor: NO |
| Sesión | «I-64 Architect Review Proposal V1» (`local_172c5d51-ea91-4f95-8c17-23a1563e61b7`) |
| Perfil | ARCHITECTURE_REVIEW / Deep |
| Límite de independencia declarado | mismo modelo (Claude) que la sesión autora |
| Autoridad sobre sus REQUIRED | solo este revisor puede cerrar o rebajar A64-PV1-01..22, con ID, razón y evidencia |

> **Qué es este paquete.** El material para la re-revisión: la versión completa (Proposal V3, autocontenida), los **deltas explícitos**
> V1→V2 y V2→V3, la **disposición por ID** de A64-PV1-01..22 y la decisión **MASTER-I63-I64-02** (LIFECYCLE §5). El delta es el foco
> mínimo, no un límite: un hallazgo material en una sección intacta es válido. La sesión autora no emite ni simula el veredicto, no declara
> consenso, Freeze ni GATE PASS, y no afirma la independencia de nadie.

## 1. Veredicto que se solicita

```text
Architect: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Por cada A64-PV1-nn: CLOSED | STILL OPEN (con razón) — solo el emisor puede cerrarlo o rebajarlo
Hallazgos nuevos: A64-PV3-nn REQUIRED | OPTIONAL, con sección, evidencia y cambio exigido
Modo: SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN; si revisor y autor son la misma persona
Versión: el veredicto vale solo para el SHA y el blob exactos revisados
```

## 2. Lectura mínima

1. [Proposal V3](I-64-proposal-v3.md), completa: en especial D-05, §11, §15, §18, §19 y los anexos A y B.
2. Este paquete: deltas (§3), disposición (§4), MASTER-I63-I64-02 (§5) y preguntas (§6).
3. [Evidencia](../automation/evidence/I-64-evidence.md) §14 y §15: órdenes del Coordinator, procedencia del veredicto V1, CI de V1 y V2,
   puntas de las ramas y hechos de código verificados.
4. Código de la base citado: `RackBlockFinder.ScanEnvelopes`, `RackListBuilder`, `RackEnvelopeIdProbe`, `ISiblingRedrawPort.cs`,
   `RackSiblingRedrawRun.cs`, `SiblingRedrawTransaction.cs`, `SiblingRedrawDebugFaultInjection.cs`, `CallerOwnedFacadeGuardTests`,
   `RackSiblingMembership.cs`, `RackVariablesCommands.cs:47-76`, `RackCantileverCommands.cs:158-168`, `RackSelectivoCommands.cs` y
   `RackSelectivoInsertIntegration.cs:81-85`.
5. I-55 V5 §4.4-§4.10 y la guía de OV de I-55 (OV-RED-06).

## 3. Deltas explícitos

### 3.1 V1 → V2 (sin cambios respecto del paquete V2)

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

### 3.2 V2 → V3

| Sección | V2 | V3 |
|---|---|---|
| Cabecera y fuentes | Master: MASTER-I63-I64-01, conflicto abierto con P-14 | Master: MASTER-I63-I64-02, que sustituye parcialmente a -01; orden V3 como fuente |
| §0 | «snapshot común» de I-63 | «hechos neutrales internos» de I-64, no fundación ni contrato para I-63; el índice runtime incluye el inventario del documento |
| §1 | criterio 3 en F1/F3; criterio 12 en F7 | criterio 3 en F1/F6 (con OV-14); criterio 12 en F6 y READY |
| §2 | enumeración neutral = snapshot de I-63 | identidad de rack y vista = autoridades integradas; inventario e índice = I-64 sobre ellas; métricas = I-63 |
| D-05 | snapshot de I-63 consumido en F6 cuando se integre; STOP al Master al agotar F1..F5 | inventario runtime propio sobre `ScanEnvelopes`, agrupación por RackId como la regla integrada, sondeo existente para ilegibles; sin `Design` retenido ni resolución; separación explícita respecto de las métricas; sin dependencia de I-63 ni STOP; `ProjectSummary` como superficie futura |
| D-18 | escenario 8 en F3 | escenarios 7 y 8 en F6 |
| D-20 | evaluación de ID3 en F7 | evaluación de ID3 en F6 |
| §10 | INV-01..33 | + **INV-34** (Selección (N) sin pestañas ni borradores) e **INV-35** (captura sin diseños, resolución, `Design` retenido ni RackId inventado) |
| §11 | F2 «Navegación»; F3 con Selección (N); F6 dependiente del contrato de I-63; F7 con ID3; STOP al Master | plan confirmado por el Coordinator: F2 **Context Navigation**; F3 **Browser Session**; F6 **Document-wide Inventory + Selección (N) + ID3**, entra con F2 y F3; F7 rendimiento y regresión; ningún gate depende de I-63 |
| §12.1 | reglas «fijadas por MASTER-I63-I64-01» | las mismas, señaladas como cláusulas que -02 no modifica |
| §13 | F6 «depende de I-63» | F6 sin dependencia; F3 sin Selección (N) |
| §14 | M-07 por panel, puente y sesiones | M-07 incluye el inventario propio; la representación interna no se declara fundación; el resto sin cambio |
| §15 | puntas de V2; conflicto abierto | puntas actuales (I-62 `acd88eaf`, I-63 `a0654ae2`, I-52 sin cambio); conflicto resuelto por MASTER-I63-I64-02 |
| §16 | riesgo «contrato de I-63 no disponible» | retirado; nuevos: coste de la captura completa (UNKNOWN) y duplicación aceptada con I-63 |
| §17 | — | MASTER-I63-I64-01 sustituida parcialmente; MASTER-I63-I64-02 y plan confirmado |
| §18 | disposición de V2 | conservada; cambia la forma de A64-PV1-18 y A64-PV1-21 |
| §19 | — | **nueva**: cláusula de MASTER-I63-I64-01 → disposición bajo MASTER-I63-I64-02 |
| Anexo A | inventario desde el snapshot común | inventario propio sobre autoridades integradas, sin fundación compartida; alternativa «snapshot común con I-63» retirada |

## 4. Disposición de A64-PV1-01..22

La tabla completa está en la Proposal V3 §18. La disposición de V2 se conserva íntegra; V3 solo cambia la forma de 18 y 21.

| ID | Estado propuesto | Dónde (V3) | Cambio en V3 |
|---|---|---|---|
| A64-PV1-01 | atendido | D-12, D-13, D-15; INV-12, INV-30 | — |
| A64-PV1-02 | atendido | D-17; INV-20; S2-06 | — |
| A64-PV1-03 | atendido | D-03; INV-22; SN-08 | — |
| A64-PV1-04 | atendido | D-04; INV-23 | — |
| A64-PV1-05 | atendido | D-10; INV-27 | — |
| A64-PV1-06 | atendido | D-12; INV-28 | — |
| A64-PV1-07 | atendido | D-12; INV-29 | — |
| A64-PV1-08 | atendido | D-10; INV-26 | — |
| A64-PV1-09 | atendido | D-06, D-07; INV-03; SN-06 | — |
| A64-PV1-10 | atendido | D-01; INV-24; S1-06a..h | — |
| A64-PV1-11 | atendido | D-08, D-17; INV-25 | — |
| A64-PV1-12 | atendido | D-08, D-10; INV-04, INV-18; anexo B | — |
| A64-PV1-13 | atendido | D-18; INV-33; S1-10 | — |
| A64-PV1-14 | atendido | D-04; INV-02 | — |
| A64-PV1-15 | atendido | D-02; INV-14 | — |
| A64-PV1-16 | atendido | D-15; INV-19 | — |
| A64-PV1-17 | atendido | §11 (secuencia de F4); §12.4 | — |
| A64-PV1-18 | atendido | D-07; §11 (F2 Context Navigation); §12.3 | nombre del gate confirmado por el Coordinator |
| A64-PV1-19 | atendido | D-01; S1-11 | — |
| A64-PV1-20 | atendido | D-15, D-20 | — |
| A64-PV1-21 | atendido | D-05; §11; §15; §19 | la causa desaparece: MASTER-I63-I64-02 §6 retira la dependencia de I-63 y el STOP; se conserva «ninguna integración parcial sin el Owner» |
| A64-PV1-22 | atendido | D-13; INV-31; OV-24..26 | — |
| C64-PV1-01..04 | dispuestos por el Architect en A64-PV1-02, 03, 04 y 01 | §18 | — |
| O-01..O-17 | quince adoptadas; O-02 no adoptada; O-09 solo en su medición | §18 | — |

## 5. MASTER-I63-I64-02

Decisión transmitida por el Coordinator en la orden de V3 (evidencia §15). Sustituye parcialmente a MASTER-I63-I64-01:

1. Se **retira** la obligación de crear una fundación o snapshot compartido entre I-63 e I-64.
2. Se **retira** la autoría inicial asignada a I-63.
3. Se conserva la frontera: I-63 = métricas, providers, población y agregación; I-64 = captura e índice runtime para navegación, selección,
   UI y sesión; RackId e identidad de vista = autoridades integradas vigentes; pertenencia para mutar = autoridades integradas
   correspondientes; DWG authored = única autoridad persistida.
4. I-64 puede usar una representación pura interna de hechos neutrales para su índice, pero **no** la declara fundación reutilizable ni
   contrato consumido por I-63.
5. Convertirla después en fundación común requerirá una decisión explícita nueva del Master.
6. I-64 no depende de la integración de I-63 para cerrar ningún gate.
7. I-63 no consume código no integrado de I-64.

Disposición cláusula por cláusula de MASTER-I63-I64-01: Proposal V3 §19.

## 6. Preguntas para la re-revisión

1. **Inventario propio (D-05, INV-35).** ¿Es suficiente capturar con `ScanEnvelopes`, agrupar con la igualdad de la regla integrada y
   atribuir los ilegibles solo por el sondeo existente, sin un enumerador nuevo con semántica propia?
2. **Frontera con las métricas (D-05, §19).** ¿Basta declarar que el índice no decide métricas ni se ofrece a I-63, o hace falta una guarda
   que impida su uso como autoridad de métricas?
3. **Plan de gates (§11).** ¿Es coherente que F6 entre con F2 y F3, sin esperar a F4 y F5?
4. Las preguntas 1-6 del paquete V2 siguen abiertas: eco (D-07), intención de edición (D-10), clasificación por frontera de `Commit()`
   (D-13, INV-30), comprobaciones de host (§12.4) y O-02 no adoptada. La pregunta 7 de V2 (conflicto con P-14) queda resuelta por
   MASTER-I63-I64-02.

## 7. Lo que este paquete no hace

No realiza la re-revisión. No declara AGREED, consenso, Freeze, GATE PASS ni independencia. No autoriza implementación, delegaciones,
sondas, pilotos ni ningún uso de AutoCAD. `IMPLEMENTATION AUTHORIZATION = NO`.

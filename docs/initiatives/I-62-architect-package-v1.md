# I-62 — Paquete de revisión del Architect (Proposal V1)

```text
PROPOSAL V1 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED
Architect      = REVIEW REQUIRED (revisión NO realizada; no hay revisor asignado en este paquete)
Consensus      = NOT REACHED
Implementation = NOT AUTHORIZED

Objeto de la revisión = docs/initiatives/I-62-proposal-v1.md, en el commit que publica este paquete (rama
                        architecture/portabilidad-coordinador-principal; el SHA no se escribe aquí para no autorreferenciarse:
                        se toma de `git log -1 -- docs/initiatives/I-62-proposal-v1.md`)
Discovery base        = docs/initiatives/I-62-discovery.md, ronda R1, blob 86f24e657998a126ef6fc719ff20f1de938c83e7 (commit 2b7976b0)
Resolución            = decisiones C62-F0-08..12 del Coordinator (docs/automation/decisions/I-62.md §10)
Base de main          = 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Qué es este paquete.** El material para revisar la Proposal V1 sin reconstruir la sesión. La sesión autora **no** emite ni simula el veredicto del
> Architect, **no** declara consenso y **no** afirma independencia de nadie. La forma de la revisión (SAME-SESSION ROLE, SEPARATE SESSION o EXTERNAL
> HUMAN) la decide quien la asigne.

## 1. Veredicto que se solicita

```text
Architect: AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
Hallazgos: REQUIRED | RECOMMENDED | NOTE — sección, evidencia (archivo:línea o hecho medido), cambio exigido
Modo de la revisión: SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN, con la identidad de la versión revisada
Versión: el veredicto vale solo para el SHA exacto revisado
```

## 2. Lectura mínima

1. [Proposal V1](I-62-proposal-v1.md), completa, incluido el anexo A (borrador del ADR sucesor parcial).
2. [Discovery R1](I-62-discovery.md): §§1-3 (estado actual y acoplamientos), §9 (semántica de M), §10.2-10.6 (EXP-02/04/05/06/08) y §§11-13 (capacidades,
   triage, `config.toml`).
3. [Decisiones](../automation/decisions/I-62.md): §7 (C62-G0-02, C62-F0-01), §9 (F0-R1) y §10 (C62-F0-08..12).
4. Fuentes integradas que la Proposal modifica o cita:
   - AUTOMATION_PLAN §16;
   - ADR-0046;
   - Freeze de I-61 ([Proposal V9](I-61-proposal-v9.md) §§3, 8, 13 y 15);
   - `routing.md`, `model-catalog.md` y README de `docs/automation/agent-execution/`;
   - LIFECYCLE §§2-5;
   - WORKFLOW §§3-4 y 10.

## 3. Delta respecto de I-61 (resumen; detalle en la Proposal §0)

- **Roles:** cinco, sin proveedor (D-01).
- **Binding:** por capacidad acreditada (D-05).
- **Principal:** perfil, `CONFIGURATION_STATUS` y autoverificación (D-03, D-04).
- **Preflight y adapters:** preflight de núcleo + adapters (D-06, D-07).
- **Independencia:** por riesgo (D-12).
- **Custodia:** por unidad, con estados del escritor (D-08, D-09).
- **«Cierre de G2»:** pasa a MaterializationClose (D-15).
- **Esquemas:** conjunto `/v2` donde cambia el contrato, con `/v1` intacto (D-14).
- **Nivel:** A, con F5 no planificado (D-16).
- **ADR:** sucesor parcial de ADR-0046 (anexo A) con ampliación del dominio de §16, que requiere al Owner.

## 4. Los 12 retos del mandato («ARCHITECT»)

| # | Reto | Dónde lo trata la Proposal | Riesgo residual que se pide juzgar |
|---|---|---|---|
| 1 | Identidad del proveedor filtrada en los roles | D-01 (§2); INV-I62-01; anexo A | que la tabla de acumulación reintroduzca supuestos de proveedor (p. ej., «Worker subagente») |
| 2 | Supuestos no verificables sobre el modelo vigente | D-04 (agregación), D-05 (filtro), D-11 (niveles de garantía); catálogo con fecha | que RUNTIME_OBSERVED baste como mínimo (§10) |
| 3 | Autointrospección falsa | D-11: autodeclaración excluida; observación ligada a sesión, turno o invocación; SP-3 solo como candidata | que `get_session` o `turn_context` reflejen el cliente y no el backend |
| 4 | Dependencias ocultas de memoria privada | D-08 (custodia durable por unidad); prueba de portabilidad con oráculo fuera del host (§12); Discovery §13.2 | el canal de acuses entre runtimes sigue UNVERIFIED |
| 5 | Router de roles demasiado complejo | D-05: un algoritmo de ocho pasos sobre el routing existente; sin motor | el número de contratos nuevos (`preflight`, `binding`, hechos por adapter) |
| 6 | Requisitos excesivos entre proveedores | D-12: proveedor distinto solo con justificación y mecanismo viable; SAME_SESSION_ALLOWED para lo rutinario | las clases asignadas a «cambio sensible a la seguridad» |
| 7 | Filtración de seguridad o autenticación | D-06: estados de autenticación sin credenciales; INV-I62-08; huella de configuración solo como hash + nombres de claves | la lectura de metadatos de sesiones del Owner (SP-3) en futuros preflights |
| 8 | Custodia ambigua | D-08: registro por unidad con declarado/observado/autorizado; sin registro global | la convivencia con WORKFLOW §3 y los hechos de Git (M-01) |
| 9 | Recuperación que pueda sobrescribir trabajo | D-09: TERMINATION_UNACCREDITED y ORPHAN_CONFIRMED no permiten tomar posesión; sin reset ni borrado | el criterio de «terminación confirmada» en otra máquina |
| 10 | Level B sin necesidad demostrada | D-16: F5 no planificado; F4 prueba la viabilidad del procedimiento versionado | si los fragmentos de procedimiento del README son de hecho tooling |
| 11 | Esquemas específicos del proveedor | D-07/D-14: `AdapterId` con patrón, `AdapterFacts` validados por su esquema, sin enum de marcas | el coste de un esquema de hechos por adapter |
| 12 | Imposibilidad de reanudar en otra máquina | D-09 (P-13), D-08; prueba A→B; Host en el relevo | la acreditación de terminación entre máquinas sigue sin mecanismo medido |

## 5. Disposición de hallazgos por ID

| ID | Origen | Disposición | Dónde |
|---|---|---|---|
| R62-F0-01..08 | revisión F0-R1 | atendidos en Discovery R1; dispuestos por el Coordinator en C62-F0-08 | Discovery §20; decisiones §§9-10 |
| R62-F0-03 | revisión F0-R1 | **rectificado por su emisor** (C62-F0-08): el bloque de PROMPT_TEMPLATES §2 existe; no es un error del Executor | decisiones §10 |
| C62-F0-09 §1 | decisión | arquetipo NEW ARCHITECTURE; M-01 creador, M-02 modificación y creador, M-04..M-08 activados; M-03 no | Proposal, cabecera y §3 |
| C62-F0-09 §2 | decisión | una unidad; F6 dentro de la misma entrega; custodia por unidad sin registro global | Proposal §§8, 12, 17 |
| C62-F0-09 §3 | decisión | mapa de autoridad como asignación de diseño; ampliación de §16 identificada | Proposal §3; anexo A |
| C62-F0-10 A-E | decisión | desarrollados | Proposal §§4.2, 10, 12, 15, 17 y 14 |

## 6. Preguntas que se piden expresamente al Architect

1. ¿Es suficiente **RUNTIME_OBSERVED** como nivel mínimo de introspección para los requisitos obligatorios (§10), o el Freeze debe fijar otro criterio por
   rol?
2. ¿La custodia en el estado canónico por unidad (§8.1) evita una segunda autoridad frente a WORKFLOW §3? ¿Basta la distinción
   declarado/observado/autorizado?
3. ¿Los nuevos contratos `preflight/v1`, `binding/v1` y los hechos por adapter son el mínimo necesario, o pueden fusionarse sin perder validación?
4. ¿Es correcta la asignación de predicados de independencia entre LIFECYCLE y §16 (§11), incluida la regla de que el router no rebaja la exigencia?
5. ¿El aislamiento del oráculo fuera del host (§12) es suficiente y verificable?
6. ¿La decisión sobre F5 (§17) está justificada con lo que F4 debe probar?
7. ¿La ampliación del dominio de §16 (§3; anexo A) está bien delimitada y puede aceptarla el Owner sin desplazar a WORKFLOW §§3-4?

## 7. Lo que este paquete no hace

No asigna revisor; no realiza la revisión; no declara AGREED, Freeze ni independencia; no autoriza implementación, delegaciones, sondas ni pilotos.

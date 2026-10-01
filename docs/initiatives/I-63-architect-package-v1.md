# I-63 — Paquete para el Architect, ronda 1 (Proposal V1)

```text
Iniciativa: I-63 / ID20 — Computed Parameters & Project Summary Foundation
Arquetipo: NEW ARCHITECTURE (rondas Coordinator ↔ Architect hasta acuerdo; INITIATIVE_LIFECYCLE §5)
Objeto de la revisión: docs/initiatives/I-63-proposal-v1.md, blob 48a68307be3f046c20c2405252dc8af3ad050342,
                       en el commit que publica este paquete (SHA y CI en el informe al Coordinator)
Base de código: origin/main 819955d61a6da4c811a11fbd11b5dca13f634b7c (fuera de docs/, idéntica a la rama de I-63)
Estado: revisión pedida, NO realizada. Freeze: NO. IMPLEMENTATION AUTHORIZATION = NO
```

## 1. Qué se pide

Una **revisión adversarial** de la Proposal V1 completa, con el formato de
[PROMPT_TEMPLATES](PROMPT_TEMPLATES.md) §C y el perfil ARCHITECTURE_REVIEW: sin escribir en el repositorio y contra las autoridades
citadas.

- **Modo:** la primera línea de la revisión declara `SAME-SESSION ROLE`, `SEPARATE SESSION` o `EXTERNAL HUMAN` y si revisor y autor
  son la misma persona (LIFECYCLE §5).
- **Severidad:** un hallazgo es `REQUIRED` si activa M-01..08 sobre un elemento congelable, deja un invariante sin prueba, produce un
  comportamiento observable distinto, revela algo congelable incorrecto, no ejecutable o no verificable, o un oráculo ciego o un plan
  de gates imposible. Una precisión sin cambio de significado es `OPTIONAL`.
- **Veredicto:** `AGREED` (cero REQUIRED abiertos sobre la versión exacta), `CHANGES REQUIRED` o `BLOCKED — OWNER DECISION`, ligado a
  commit, ruta y blob.
- **IDs de hallazgo:** `A63-PV1-nn`.

## 2. Material de entrada

| Material | Referencia exacta |
|---|---|
| **Proposal V1** | `docs/initiatives/I-63-proposal-v1.md`, blob `48a68307be3f046c20c2405252dc8af3ad050342` |
| Discovery R2 **aceptado** | `docs/initiatives/I-63-discovery.md` en `23eeefc5f5ae0c50c1addd041865ceb6f69139fc`, blob `df74e7fe8f86302493e6cd3312206eaf304b1891` (CD-I63-F0-R2-04) |
| Discovery con las decisiones posteriores del Owner | Mismo archivo en `a0654ae29f21532c1a97bf12d0fa3abc4ba1211f`, blob `eaffc67d700863cae98a49a25426197a98e424b1`, §§24-26 (P-14, P-01..P-05, cierre de P-05) |
| Revisiones del Coordinator | Decisiones de I-63 §2: CD-I63-F0-R1-01..03, CD-I63-F0-R2-01..06 y la orden PV1 |
| Decisiones del Owner | Decisiones de I-63 §6; texto literal en la evidencia §§13-15 |
| Mandato | `docs/automation/decisions/I-63-owner-mandate.original.txt`, blob `876bdd50f10f76bb73eed7c8e686e5cb9b8972c6` |

**Autoridad exacta de I-49** (blobs en `origin/main`):

| Documento | Blob |
|---|---|
| `I-49-proposal-v6.md` | `ef4db3aa400483ff25a8f39b2beb93708fa43d1a` |
| A1 `I-49-proposal-v6-amendment-a1-text-guard.md` | `d62019088b9e7a140d5066799afe6ace6db303ba` |
| A2 `I-49-proposal-v6-amendment-a2-exact-key-qualifier.md` | `49a925336dd3775929a35b0f40b73cb7f8c487f7` |
| A3-R2 `I-49-proposal-v6-amendment-a3-multi-cause-dependency-failures.md` | `4da6ef3caa21dcf31140983c6db7e23f02aa3e18` |
| ADR-0043 | `dd88bf06fe5b59a5b7683cb513a4a42460d71438` |
| Freeze correctivo `I-49-consensus-freeze-v6-a1-a2-a3-r1.md` | `440ae61abe93458261944d9d3df3dfa5fe04d94a` |

Cláusulas pertinentes: V6 P25 (ID20), P26 (ID23), P27 (ID28/29); ADR-0043 D5, D6, D9, D24 y D25.

**Custom Properties:** ADR-0039 §13, blob `539065e645aa73105c6a0b6d71d4431f9eef19e2`, y la D-16 de I-54 en
`I-54-proposal-v5.md`, blob `a75444702ad354b35b67bfbbf5bb955913a02c81`.

## 3. Resumen de lo que hay que juzgar

| Tema | Dónde | Decisión de V1 |
|---|---|---|
| Matriz sistema × métrica | Proposal §6; Discovery §14 | Solo el Selectivo tiene métricas numéricas en V1. Los demás kinds dan `NotSupported` o `NotApplicable` explícitos |
| Materialidad M-01..08 | Proposal §3; Discovery §11 | M-01, M-04..M-08 activados; M-02 y M-03 **no** activados (sin persistencia nueva ni cambio observable) |
| Expansiones ejecutadas | Discovery §12 (EXP-01 negativa; EXP-02, 04, 05, 06, 08 y 09 ejecutadas; EXP-03 no activada; EXP-07 cerrada por P-14) | — |
| Decisiones del Owner | Proposal §2; Discovery §§24-26 | P-01 cotizables; P-02a excluir; P-03 `Unavailable`; P-04 referencia directa; P-05 fondo 0 con vacíos y métrica aparte; P-14 sin contrato común |
| Riesgos de ciclo | Proposal §12; Discovery §16.2 | R1..R6; ningún símbolo calculado antes de Φ3 ni en fórmulas de propiedad o variables |
| Población y cotizabilidad | Proposal §10 | E1..E6 con autoridades existentes; clasificación pertenencia / cobertura / otra métrica |
| Coordinación con I-64 | Proposal §16; Discovery §§9.4-9.5 | P-14 (Owner). I-64 V3 (`1f1530be`) registra `MASTER-I63-I64-02`, que retira el *snapshot* compartido y la dependencia de I-63: **alineado**, sin contrato común |
| Gates e I-61 | Proposal §21 | G1..G4 conductuales; perfiles iniciales de `routing.md`; RED→GREEN real; INV-12..14 de no regresión, escritos antes del cambio |
| ADR | Proposal §23, anexo A | Sucesor parcial de ADR-0043 D5, D9, D24 y D25; persistencia cerrada |

## 4. Desafíos que la orden PV1 pide expresamente

El Architect debe desafiar, como mínimo:

1. una segunda autoridad de conteo o de cotización (Proposal §10, R-01);
2. una definición circular de «cotizable» (§10, R-02);
3. el uso accidental del BOM como autoridad de métricas (§§7 y 10, R-03);
4. `Rack.Frentes` y `FrentesVacios`: fuente, fase, predicado y equivalencia con el diseño (§7; INV-04, INV-05);
5. la semántica por sistema (§6);
6. los símbolos *built-in* y la compatibilidad con documentos anteriores (§13, D-19);
7. la persistencia de los namespaces nuevos (D-18, INV-15);
8. los ciclos entre la evaluación de propiedades, el efectivo y los agregados de proyecto (§12);
9. la resolución completa y ansiosa del proyecto (§§11 y 18; R6);
10. un *framework* sobregeneralizado (D-08);
11. la separación ComputedParameter / ProjectVariable / CustomProperty (§4, D-23);
12. la duplicación deliberada con I-64 (§16);
13. el plan RED→GREEN y su ejecutabilidad real bajo I-61 (§21);
14. la aplicabilidad de la Owner Validation (§22).

Y los del mandato («ARCHITECT»):
- autoridades de métricas duplicadas;
- geometría o BOM usados como fuente accidental;
- vistas de rack contadas como racks;
- ciclos ocultos;
- UI filtrada en los *providers*;
- resolución ansiosa;
- semántica ambigua entre sistemas;
- nombres de símbolo que actúan como identidad;
- un *framework* universal de métricas.

## 5. Código que conviene leer (base `819955d6`)

| Área | Archivos |
|---|---|
| Núcleo de I-49 | `Application/Expressions/{SymbolId,SymbolTable,ExpressionContext,ExpressionBinder,ExpressionEvaluator,ExpressionFormatter,RegistryEvaluation,DependencyGraph,FunctionRegistry}.cs` |
| Persistencia de expresiones | `Application/Persistence/PersistedBoundExpressionJson.cs` (`:95`, `:170`) |
| Consumidores de expresiones | `Application/ProjectVariables/{LinkedPropertyExpressionAuthoring,LinkedPropertyAuthoringContext,ProjectVariablesExpressionAdapter,BindingInspection}.cs` |
| Selectivo | `Application/Systems/Selective/{SelectiveEffectiveDesignResolver,SelectiveGeometryResolver,SelectiveDepthLayout,SelectivePlantaBuilder,SelectiveDesviadorPlan}.cs`; `Domain/Systems/Selective/{SelectivePalletDesign,SelectiveRackSystem}.cs` |
| Población y BOM | `Application/Bom/{BomAuthoredAuthority,RackBomOutputGate,ConsolidatedBom}.cs`; `Application/ProjectVariables/ProjectVariableScanProjection.cs`; `Application/Persistence/{RackListBuilder,RackEmbedDocument,KindDispatch}.cs` |
| Captura (Plugin) | `Plugin/RackBlockFinder.cs` (`ScanEnvelopes`); `Plugin/RackInventarioCommands.cs` y `.BomTotal.cs`; `Plugin/KindHandlers/*`; `Plugin/Views/RackSiblingScan.cs` |
| Pruebas de referencia | `G8PersistenceIntegrationContractTests`, `ExpressionRoundTripTests`, `ExpressionFormatterTests`, `ExpressionBinderTests`, `RackListBuilderTests`, `SharedPhysicalFactsTests`, `RackSiblingRedrawTests` |

## 6. Encargo listo para usar

```text
I-63 — ARCHITECT REVIEW R1 (Proposal V1)
Declara en la primera línea: Review mode: <SAME-SESSION ROLE|SEPARATE SESSION|EXTERNAL HUMAN>; revisor=autor: <sí/no>.
Repositorio marioap-afk/Calculadora_de_racks, rama architecture/parametros-calculados-resumen-proyecto.
Revisa la versión EXACTA de docs/initiatives/I-63-proposal-v1.md, blob 48a68307be3f046c20c2405252dc8af3ad050342
(compruébalo con git rev-parse <commit>:docs/initiatives/I-63-proposal-v1.md), contra este paquete (docs/initiatives/I-63-architect-package-v1.md) y el código de
origin/main 819955d6. No escribas en el repositorio. No implementes. No congeles.
Aplica INITIATIVE_LIFECYCLE §§3-6 y PROMPT_TEMPLATES §C y el perfil ARCHITECTURE_REVIEW.
Desafía como mínimo los 14 puntos de la orden PV1 y los del mandato (§4 del paquete).
Cada hallazgo: A63-PV1-nn; elemento; razón; evidencia (archivo:línea o cláusula); REQUIRED|OPTIONAL; corrección mínima.
Termina con AGREED POINTS, DISAGREEMENTS, MATERIAL RISKS, REQUIRED CHANGES, OPTIONAL IMPROVEMENTS y
CONSENSUS STATUS = AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION, ligado a commit/ruta/blob.
```

## 7. Después de la revisión

- `CHANGES REQUIRED` → al Coordinator, con la Proposal V1, el veredicto completo y los REQUIRED por ID.
- `AGREED` → al Coordinator, con la identidad exacta del commit y el blob revisados, para decidir el Consensus Freeze.
- En ningún caso se congela ni se implementa sin una orden nueva.

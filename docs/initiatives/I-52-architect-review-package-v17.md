# I-52 — Paquete autonomo para Architect Review de Proposal V17

## 1. Encargo y objeto exacto

Revisar Proposal V17 de RACKMIRROR y ADR-0036 actualizado, sin implementar, congelar ni abrir G3.
Codex es **ejecutor/redactor** de V17. Este paquete no es una revision del Arquitecto ni afirma independencia
de una revision aun no realizada. La revision de V16 se recibió mediante el relevo del usuario, con veredicto
**CHANGES REQUIRED — PROPOSAL V17**, y queda registrada como entrada en decisions/I-52 §98.

El objeto de revision es el commit que introduce este paquete junto con V17, el ADR y el registro.
`PROPOSAL_V17_SHA` es el SHA completo entregado en el informe de publicacion; para resolverlo desde Git:

```powershell
git log --diff-filter=A --format=%H -- docs/initiatives/I-52-proposal-v17.md
git show <PROPOSAL_V17_SHA>:docs/initiatives/I-52-proposal-v17.md
git show <PROPOSAL_V17_SHA>:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md
git show <PROPOSAL_V17_SHA>:docs/automation/decisions/I-52.md
git diff --name-status <PROPOSAL_V17_SHA>^ <PROPOSAL_V17_SHA>
```

No usar el tip mutable de la rama como sustituto. El informe adjunta CI de push sobre ese SHA; verificar
head_sha, branch, event y conclusion de cada job. Publicar este paquete no es Technical Consensus.

## 2. Fuentes minimas y orden de lectura

1. [AGENTS](../../AGENTS.md), [WORKFLOW](../WORKFLOW.md), [AUTOMATION_PLAN](../AUTOMATION_PLAN.md).
2. [Contrato de I-52](I-52-rackmirror-espejo-semantico.md) como apertura historica;
   [Discovery](I-52-discovery.md) como base; estado actual en [decisions/I-52](../automation/decisions/I-52.md) §§98–103.
3. [Proposal V17](I-52-proposal-v17.md), primero §0 (matriz, reglas y preflight), despues §§1–21 consolidadas.
4. [ADR-0036](../adr/0036-rackmirror-espejo-semantico-por-copia.md), decisiones 7, 11 y 13, contexto y consecuencias.
5. [V16](I-52-proposal-v16.md), solo para comparar los puntos sustituidos y las marcas heredadas. No se exige
   leer V1–V15 enteras; consultar un antecedente solo cuando la decision evaluada lo requiera.
6. Paralelas en sus refs actuales tras fetch, comprobando materialidad frente a los refs del informe:
   I-49 decisions §16 (A3-R3 sobre A3-R2), I-55 Proposal V4/decisions G2E y paquetes, I-56 review V2.
   No traer esas ramas ni asumir que su contenido intermedio es autoridad final de I-52.

V17 mantiene el contrato funcional: copia semantica editable/persistente, un diseño por RackId, seleccion
multi-rack todo-o-nada, sin busqueda automatica de hermanas, variables intactas, autoridad pura de planes,
colocacion de determinante positivo y equivalencia por todas las vistas admisibles. Conserva las limitaciones
L-01..L-37 y la evidencia indicativa de biblioteca como indicativa, sin convertirla en resultado G3.

## 3. Hallazgos de entrada y cierre que se solicita revisar

| Entrada de V16 | Cambio V17 | Ataque minimo del revisor |
|---|---|---|
| AR16-01 HIGH: politica de terceros no observable/proporcionada | D01–D03, D11; §0.3/§0.7 | ¿Distingue efectos de overrule y escrituras de reactor? ¿Clasifica familias y aplicabilidad sin caer en Overruling ⇒ EVERYTHING UNKNOWN? |
| AR16-02 MEDIUM: discovery cerrado sobre señales | D04–D06; §0.4 | ¿Encuentra modos persistentes antes de seleccionar señales y demuestra el mapping? ¿Una autoridad generica prueba realmente todos los modos que se le asignan? |
| AR16-03 MEDIUM: familias de API sin cierre mecanico | D07–D09; §0.5 | ¿Una invocacion nueva de cualquier familia, indirecta o desde un helper, queda censada o RED? ¿Setters/getters y handlers diferidos quedan alcanzados? |
| LOW: nested/cyclic fields | D10; §0.6 | ¿Hojas EDIT fijas y STATE variables recursivamente? ¿Ciclo sin cota finita es UNKNOWN? ¿Attribute/Dimension conservan causalidad? |
| LOW: fuente fiable de host identity | D06; §0.4; R-57 | ¿Ausencia en CT-49 obliga STOP y se distingue de lectura fallida/host fuera del conjunto en runtime? |
| LOW: casos G3 faltantes | D12; §0.8 | ¿Incluye todos los casos prescritos sin afirmar que se ejecutaron ni abrir G3? |

## 4. Checklist tecnico autonomo

- **Overrules:** Drawable/Geometry/Transform y equivalentes; Grip/Osnap/Highlight/Properties solo irrelevantes
  con demostracion. UNKNOWN no se transforma en irrelevante. HasOverrule consulta el sujeto y la clase de
  overrule; validar filtros y granularidad real en 2025. No inspeccionar plugins. Fallback global solo con
  autoridad limitada demostrada, expuesta en O-1/L-34/M-35/R-58. No penalizar un overrule distinguible no aplicable.
- **Modos:** censo desde comandos, estados persistentes, contextual editing environments/tabs, open/close,
  long transactions, fuentes asociativas y block authoring/testing. Minimos REFEDIT/REFCLOSE, BEDIT/BCLOSE,
  ARRAYEDIT Source/ARRAYCLOSE, BTESTBLOCK lifecycle. Active-command state no cubre los intervalos persistentes.
  Modo sin detector dedicado/generico fiable ⇒ CT-49 STOP.
- **Genericas:** LongTransactionManager prueba existencia de long transaction por document/database; working
  set prueba pertenencia al conjunto activo, no ausencia global. Predicado y cobertura demostrados o STOP.
- **Identidad:** product, vertical/host, major/API con fuente fiable caracterizada; ausencia ⇒ CT-49 STOP;
  lectura fallida o host no caracterizado en runtime ⇒ E12 en P0 (E10 en R6, E11/rollback en MUTATE).
- **Censo API:** todas las invocaciones alcanzables de produccion Plugin/materializacion/helpers/factories;
  READ_ONLY/DECLARED_MUTATOR/EXCLUDED_WITH_REASON; UNCLASSIFIED ⇒ G-M24 RED. Cierre mecanico independiente de
  fixtures, con llamadas indirectas no resueltas visibles. CT-50 captura el censo inicial.
- **Fixtures:** conservar todas las de V16 y añadir EvaluateFields, Field.Evaluate, DataLinkManager.Update*,
  Table.UpdateDataLink, UnloadXrefs/DetachXref/AttachXref/OverlayXref, LayoutManager.*, UpdateExt, Application y
  DocumentCollection. T-M75 incluye invocacion ARBITRARIA sin clasificar ⇒ RED.
- **Eventos:** Database/Document/Editor/Application/DocumentCollection con handlers mutadores o que programen
  escrituras diferidas; cada registro RackCad DECLARED o EXCLUDED_WITH_REASON; no confundirlo con terceros runtime.
- **Fields/XREF:** nested, cyclic, Attribute+STATE; Dimension geometry EDIT frente a estado/style/sysvar
  caracterizado; DataLink excepcion STATE. XREF complete/incomplete/unresolved, archivo sustituido sin reload
  y reload con contenido cambiado, siempre desde contenido cargado actual y completitud.
- **Matrices/ADR:** V16-D01/D02/D05/D06/D07/D11/D12/D13 reabiertas/cerradas; D03/D08 precisadas;
  V17-D01..D14 trazables; OPEN MATERIAL = NONE solo como estado de redaccion. ADR PROPOSED.
- **Paralelas:** A3-R2 ya tiene consenso exacto en I-49, pero sucesor/Owner/freeze pendientes; RS-3 PENDING
  e I-52 FREEZE BLOCKED. I-55 V4 formalmente pendiente, diferencias X-* no adoptadas. I-56 NOT EFFECTIVE,
  I-52 grandfathered. Autoridades compartidas siguen provisionales hasta reconciliacion.

## 5. Alcance de evidencia y proceso

Gate docs-only: cuatro archivos de docs. No produccion, tests nuevos productivos, rebase, freeze ni O-1.
Core de entrada comprueba la base V16; no se hereda a V17. La evidencia propia de V17 es la revision documental,
diff mecanico y CI de push exacta del informe, no caracterizacion de AutoCAD. CT-49/CT-50 siguen pendientes.

```text
acuerdo Coordinator + Architect sobre V17 exacta
→ consenso tecnico → rebase CURRENT main → reconciliacion I-52/I-55/I-49
→ resolver I-55/X-* formal → autoridad FINAL I-49 → freeze exact SHA → re-check
→ O-1 → G3
  contradiccion material: Proposal V18
  sin contradiccion material ni G3_PENDING: Owner acepta ADR-0036 → G4
```

La revision debe entregar: SHA revisado, hallazgos con severidad y archivo/seccion, agreed points, materialidad,
veredicto y cambios concretos requeridos. Un desacuerdo material se eleva; el ejecutor no decide por los roles.

```text
SUBSTANTIVE IMPLEMENTATION: BLOCKED
Coordinator: REVIEW REQUIRED
Architect: REVIEW REQUIRED
Consensus: NOT REACHED
G3: NOT OPEN
Implementation: NOT READY
```

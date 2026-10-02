# I-62 — Paquete de revisión del Architect (Proposal V14)

```text
PROPOSAL V14 — NOT REVIEWED
Architect      = REVIEW REQUIRED: UNA revisión formal FINAL, con la fidelidad probada por el mismo camino, la evidencia de lo visible por el revisor y lecturas
                 acotadas; sin expandir los documentos alcanzables por la prosa
Coordinator    = A62-V11-01 ACCEPTED REQUIRED, residuo final; todo lo demás CLOSED o SUPERSEDED sigue igual; la revisión de V13 sigue siendo válida
                 (registro I-62-architect-review-v13-disposition.md)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V14 §11.4); sin decisión no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v14.md                        blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
  docs/initiatives/I-62-architect-review-v13-disposition.md    blob 37c1752c914d01605b36937192b677d2a4fc281e
Versión anterior:
  V13: commit b337a59fa6879893229096f6e423d441b3c97bc6, blob 949c6403a04570dfa7b5dcd1a3509271819dc696 (Architect: CHANGES REQUIRED; custodia de la
       revisión en f51eda39d56a498a4ec56b4604acbe3a9be25746)
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (R62-V1-09, preservado). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v14.md` = `34ad80ea…`. Si no coincide, revisa la versión designada o rechaza la
> discordancia. Este paquete no lleva su propio blob.

## 0. Condiciones de la revisión final (disposición del Owner y del Coordinator, «NEXT REVIEW»)

V14 tendrá **una** revisión formal final del Architect, con:
- el `EffectiveInputClosure` recalculado sobre el commit exacto (§20.3.1);
- el preflight de fidelidad por el **mismo** camino de lectura (§20.3.3), sobre todos los caracteres no ASCII del corpus del cierre;
- la evidencia de fidelidad sobre **lo entregado al revisor**, con lecturas acotadas para los archivos grandes y un presupuesto de salida por llamada;
- la evidencia de identidad del runtime, con las compactaciones (§20.3.2);
- la misma exención acotada de `dotnet test` si hace falta (§20.3.1).

La revisión **no** debe exigir que se expandan mecánicamente todos los documentos alcanzables por la prosa.

**Formas de lectura acreditadas** (evidencia `I-62-architect-v13/…/input-fidelity-preflight.json`). Se vuelven a probar sobre el commit exacto:
- **A, archivo completo** (`cmd /c type <ruta con barras invertidas>`): solo para los archivos pequeños;
- **B, rango** (un `pwsh` anidado tras `chcp 65001`, con `Get-Content … | Select-Object -Skip N -First M`): para los archivos grandes;
- **C, búsqueda** con salida en cadenas planas `número:línea`;
- **Git**, directamente.

**Cierre efectivo previsto.** El invocador lo recalcula sobre el commit exacto:
- **`CanonicalInputs`:** los de §2;
- **`AllowedTransitiveInputs`:** los de la revisión de V13, que en esas rutas siguen iguales a `main`:
  - `docs/HANDOFF.md`, `README.md` y `docs/ARCHITECTURE.md`;
  - `docs/context-packs/README.md` y `documentation-governance.md`;
  - `docs/ROADMAP.md` y `docs/FOUNDATIONS.md`;
  - `docs/initiatives/README.md` y `docs/initiatives/PROMPT_TEMPLATES.md`.

**Forma del resultado:** en lo posible, `rackcad-architect-review-result/v1` (Proposal V14 B.10.1), como representación experimental. Cada `PremiseRef` lleva
`LineStart`, `LineEnd` y la **proposición normativa completa**. `OpenFindings`:
- **REQUIRED aceptado:** A62-V11-01 (residuo final);
- **OPTIONAL abiertos, no bloquean:** O-V7-01, O-V7-02, O-05 y O-07.

Hay que reevaluar R62-FIDELITY-02, -03 y -06, que la revisión de V13 dejó en PARTIALLY_SATISFIED. Hay que confirmar que todos los cierres anteriores siguen
válidos, salvo que V14 introduzca una contradicción material directa:
- A62-V10-03, A62-V10-01, A62-V10-04 y A62-V9-04;
- A62-V9-02, -03, -05 y -06;
- A62-V7-01..03 y A62-V6-01..03;
- los cierres previos del Coordinator;
- SUPERSEDED: A62-V9-01 → A62-V10-03.

## 1. Veredicto que se solicita (LIFECYCLE §5)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, premisa canónica completa (PremiseRefs con líneas), autoridad o contraejemplo, por qué importa, corrección.
Modo:      SEPARATE SESSION (declarado); si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

Con cero REQUIRED: el Owner decide OD-6, el Coordinator da AGREED sobre la V14 exacta, y llega el Consensus Freeze.

## 2. Lectura (insumos canónicos)

1. [Proposal V14](I-62-proposal-v14.md) completa; [Proposal V13](I-62-proposal-v13.md) y su [paquete](I-62-architect-package-v13.md), para verificar el delta.
2. [Disposición del Owner y del Coordinator sobre la revisión de V13](I-62-architect-review-v13-disposition.md); [registro de la revisión de
   V13](I-62-architect-review-v13.md) y su evidencia en `docs/automation/evidence/I-62-architect-v13/R20261002T165850Z-648a/`:
   - [`output.json`](../automation/evidence/I-62-architect-v13/R20261002T165850Z-648a/output.json);
   - [README](../automation/evidence/I-62-architect-v13/R20261002T165850Z-648a/README.md);
   - [`input-fidelity-postrun.json`](../automation/evidence/I-62-architect-v13/R20261002T165850Z-648a/input-fidelity-postrun.json).
3. Las disposiciones y los registros anteriores: [V12](I-62-architect-review-v12-disposition.md) y su [registro](I-62-architect-review-v12.md),
   [V11](I-62-architect-review-v11-disposition.md), [V10](I-62-architect-review-v10-disposition.md), [V9](I-62-architect-review-v9-disposition.md),
   [requisito R62-AUTO](I-62-coordinator-requirement-auto.md), y los registros del Architect [V6](I-62-architect-review-v6.md) y
   [V7](I-62-architect-review-v7.md).
4. [Discovery R1](I-62-discovery.md); [mandato](../automation/decisions/I-62-owner-mandate.txt); [contrato](I-62-portabilidad-coordinador-principal.md);
   [decisiones](../automation/decisions/I-62.md); [evidencia](../automation/evidence/I-62-evidence.md); [estado](../automation/state/I-62.yml).
5. Autoridades: LIFECYCLE; WORKFLOW; AGENTS; AUTOMATION_PLAN completo; ADR-0046; el Freeze de I-61 ([Proposal V9 de I-61](I-61-proposal-v9.md)); los
   documentos y esquemas de `docs/automation/agent-execution/`.

## 3. Delta V13 → V14

| Área | V13 | V14 |
|---|---|---|
| Unidades de la clausura | unidades estructurales identificadas por `{Path, LineStart, LineEnd}` | **`NormativeUnitRef {Document, UnitId, Anchor, Revision}`** (§20.3.3, punto 1): identidad canónica, no por subcadena; las líneas son una proyección |
| Resolución de referencias | regla general, con referencias inexistentes o ambiguas en UNKNOWN | **orden explícito** de seis reglas (punto 2): calificada; mismo documento; destino propuesto mapeado; identificador definido; AMBIGUOUS_REFERENCE; UNRESOLVED_REFERENCE. Sin búsqueda en otros documentos para una referencia sin calificador |
| Secciones propuestas | sin tratamiento («§16.13» quedaba sin destino) | **§3.1**: mapa de materialización con `ProposedNormativeTarget {FutureDocument, FutureAnchor, DesignSource, State}` para las secciones que I-62 propone; resuelven a su unidad de diseño |
| Documentos enteros | fuente terminal incondicional | **clasificados** (punto 3): BOUNDED_ENTRY_SET, COMPOSITE_RULE o WHOLE_DOCUMENT_UNBOUNDED → UNKNOWN; ni terminales automáticos ni expansión por defecto |
| Aristas | toda referencia, conector o identificador | **solo aristas de control** (punto 4); fuera los enlaces informativos, las citas históricas, los ejemplos, la procedencia y las referencias sin relación |
| Grafo | inferido del texto | **manifiesto de dependencias** `rackcad-normative-dependency-manifest/v1` (punto 5, **B.11**), versionado y acotado a las superficies de I-62; INCOMPLETE_METADATA → UNKNOWN; validador determinista; entregable de F3 |
| Terminalidad y acotación | terminal por documento entero | terminal solo con cero dependencias de control pendientes (punto 7); **principio de acotación** con su guarda |
| Fidelidad | tres condiciones | cuatro (punto 3 del envoltorio): incluye que toda unidad de la clausura fue **vista** fielmente y que toda dependencia se resolvió de forma determinista |
| Evidencia y contratos | `premise_independence` con la clausura | con el manifiesto, los resultados de resolución y la visibilidad por unidad: B.8.8 (campo e I-S18), B.10.1, B.1, P-25 |
| Casos | C-42 (h)-(p) | más C-42 (q)-(z): los diez casos de la disposición; G.1 y G.2 |
| Huecos | GAP-10 y GAP-11 | más **GAP-12**: el diagnóstico del runtime antepuesto a varias salidas deja cada revisión medida en DEGRADED_BOUNDED (§20.11) |
| Caso de V13 | — | registrado en §20.3.3: clausura literal UNKNOWN bajo la regla sustituida; no se fabrica una clausura retroactiva |

El resto de V13 queda sin cambios.

## 4. Disposición de la sesión por hallazgo (la sesión declara; el cierre lo decide quien revisa)

| Hallazgo | Corrección en V14 | Obligación de verificación |
|---|---|---|
| A62-V11-01 (residuo final) | §20.3.3, «Clausura de dependencias normativas sobre un grafo canónico y acotado» (puntos 1-8, principio de acotación y guarda); punto 2 del envoltorio; regla de fidelidad en cuatro condiciones; §3.1; B.11; B.8.8 (`premise_independence`, I-S18); B.10.1; B.1; P-25; G.1 y G.2 | C-42 (q) documento entero B → B1 → C degradada; (r) conjunto de entrada {B1} con B2 degradada y sin relación; (s) documento sin conjunto de entrada ni regla compuesta; (t) §16.13 propuesta; (u) «§16.3» ambiguo; (v) «§16.3» del mismo documento; (w) ciclo A → B → C; (x) cita histórica o enlace sin relación; (y) arista del manifiesto a una C inexistente; (z) todos los nodos fieles |

## 5. Riesgos y preguntas para la revisión

1. **Cuándo existe el manifiesto.** V14 define el manifiesto y lo hace entregable de F3. Hasta entonces, con DEGRADED_BOUNDED, una premisa con dependencias
   queda en INCOMPLETE_METADATA y no se acredita. ¿Basta, o debe acompañar a la Proposal, antes del Freeze, un manifiesto de sus propias superficies?
2. **GAP-12.** Todas las revisiones medidas con `codex-cli` (V11 a V13) quedaron en DEGRADED_BOUNDED solo por el diagnóstico que el runtime antepone a
   algunas salidas. Junto con la pregunta 1, esto significa que, mientras no haya manifiesto, una revisión así no acredita los hallazgos con dependencias.
   V14 no cambia la normalización permitida de R62-FIDELITY-02. ¿Es la consecuencia buscada?
3. **Referencia sin calificador con un solo candidato externo.** La regla 5 la trata como AMBIGUOUS_REFERENCE (falta el espacio de nombres), más estricta que
   «más de un destino». ¿Es la regla correcta?
4. **`UnitId` sin identificador propio.** Para una unidad sin identificador de cláusula, `UnitId` = ancla de sección + tipo + ordinal, ligado a la
   `Revision`. Una edición posterior puede cambiar el ordinal. ¿Basta con ligarlo a la revisión?

## 6. Decisiones del Owner y su frontera real

Sin cambios respecto de V13: OD-6 (acuerdo y Freeze), OD-1 (READY-03 y vigencia), OD-2, OD-3, OD-4, OD-5 y OD-7 (§18 de la Proposal). La exención de
`dotnet test` para la invocación de esta revisión no es una OD del diseño: es un parámetro de la autorización de esa invocación.

## 7. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni sesiones nuevas.
IMPLEMENTATION AUTHORIZATION = NO.

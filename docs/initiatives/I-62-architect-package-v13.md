# I-62 — Paquete de revisión del Architect (Proposal V13)

```text
PROPOSAL V13 — NOT REVIEWED
Architect      = REVIEW REQUIRED: UNA revisión formal FINAL, con la fidelidad probada por el mismo camino, la auditoría de lo visible por el revisor, lecturas
                 acotadas para los archivos grandes y la verificación de la NormativeDependencyClosure
Coordinator    = A62-V10-03 CLOSED; A62-V11-01 ACCEPTED REQUIRED, solo el residuo; todo lo demás CLOSED o SUPERSEDED sigue igual; la revisión de V11 sigue
                 acreditada, con su registro de evidencia corregido (registro I-62-architect-review-v12-disposition.md)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V13 §11.4); sin decisión no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v13.md                        blob 949c6403a04570dfa7b5dcd1a3509271819dc696
  docs/initiatives/I-62-architect-review-v12-disposition.md    blob 3751ca998a908a37022126d23a519ab6c6aec77c
Versión anterior:
  V12: commit d0947c5d58280f8b0bfe2808dd8b7ff79e9f76bb, blob 320cecc9bd1b68112509e510b97c768b6d505ba2 (Architect: CHANGES REQUIRED; custodia de la
       revisión en 4a4ceae712cecd3a0667b3ed3600a042b014e8d4)
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (R62-V1-09, preservado). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v13.md` = `949c6403…`. Si no coincide, revisa la versión designada o rechaza la
> discordancia. Este paquete no lleva su propio blob.

## 0. Condiciones de la revisión final (disposición del Owner y del Coordinator, «NEXT REVIEW»)

V13 tendrá **una** revisión formal final del Architect. Usa el mecanismo de fidelidad ya probado, con:
- la **auditoría de la salida visible para el revisor**: la comparación tras la corrida se hace sobre la representación que recibió el modelo, con sus
  truncamientos, y no solo sobre el registro de eventos (§20.3.3, «Representación entregada al revisor»);
- **lecturas acotadas o por rangos** para los archivos grandes. Es una mitigación operativa, no un número de líneas normativo. Lección de V12: la herramienta
  del revisor puede truncar la salida conjunta de una llamada que agrupa varias lecturas, así que el invocador puede limitar también ese tamaño;
- la **verificación de la `NormativeDependencyClosure`** de cada premisa, que calcula y custodia el invocador tras la corrida (§20.3.3);
- la misma **exención acotada de `dotnet test`** si hace falta (§20.3.1), con la CI de publicación solo como señal separada.

Antes de lanzar: el `EffectiveInputClosure` recalculado sobre el commit exacto (§20.3.1), el preflight de fidelidad por el **mismo** camino de lectura
(§20.3.3) sobre todos los caracteres no ASCII del corpus del cierre, y la evidencia de identidad del runtime (§20.3.2), con las compactaciones como evidencia
de runtime.

**Formas de lectura acreditadas** (evidencia `I-62-architect-v12/…/input-fidelity-preflight.json`). Se vuelven a probar sobre el commit exacto:
- **A, archivo completo** (`cmd /c type <ruta con barras invertidas>`): solo para los archivos pequeños;
- **B, rango** (un `pwsh` anidado tras `chcp 65001`, con `Get-Content … | Select-Object -Skip N -First M`): la forma por defecto para los archivos grandes;
- **C, búsqueda** con salida en cadenas planas `número:línea`;
- **Git**, directamente.

No se fija ningún número de líneas: el requisito observable es que el envoltorio y la clausura de dependencias de cada premisa usada lleguen fielmente al
revisor. Tras la corrida, la comprobación de fidelidad sobre lo entregado al revisor, la de los envoltorios y la de las clausuras (§20.3.3) son las
decisivas.

**Cierre efectivo previsto.** El invocador lo recalcula sobre el commit exacto:
- **`CanonicalInputs`:** los de §2;
- **`AllowedTransitiveInputs`:** los de la revisión de V12, que en esas rutas siguen iguales a `main`:
  - `docs/HANDOFF.md`, `README.md` y `docs/ARCHITECTURE.md`;
  - `docs/context-packs/README.md` y `documentation-governance.md`;
  - `docs/ROADMAP.md` y `docs/FOUNDATIONS.md`;
  - `docs/initiatives/README.md` y `docs/initiatives/PROMPT_TEMPLATES.md`.

**Forma del resultado:** en lo posible, `rackcad-architect-review-result/v1` (Proposal V13 B.10.1), como representación experimental. Cada `PremiseRef` lleva
`LineStart`, `LineEnd` y la **proposición normativa completa**. `OpenFindings`:
- **REQUIRED aceptado:** A62-V11-01 (residuo);
- **OPTIONAL abiertos, no bloquean:** O-V7-01, O-V7-02, O-05 y O-07.

Hay que reevaluar R62-FIDELITY-02, -03 y -06, que la revisión de V12 dejó en PARTIALLY_SATISFIED por el residuo de A62-V11-01. Hay que confirmar que todos
los cierres anteriores siguen válidos, salvo que V13 introduzca una contradicción material directa:
- A62-V10-03, cerrado en la revisión de V12;
- A62-V10-01, A62-V10-04 y A62-V9-04;
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

Con cero REQUIRED: el Owner decide OD-6, el Coordinator confirma el acuerdo sobre la V13 exacta, y llega el Consensus Freeze.

## 2. Lectura (insumos canónicos)

1. [Proposal V13](I-62-proposal-v13.md) completa; [Proposal V12](I-62-proposal-v12.md) y su [paquete](I-62-architect-package-v12.md), para verificar el delta.
2. [Disposición del Owner y del Coordinator sobre la revisión de V12](I-62-architect-review-v12-disposition.md); [registro de la revisión de
   V12](I-62-architect-review-v12.md) y su evidencia en `docs/automation/evidence/I-62-architect-v12/R20261002T151854Z-603d/`:
   - [`output.json`](../automation/evidence/I-62-architect-v12/R20261002T151854Z-603d/output.json);
   - [README](../automation/evidence/I-62-architect-v12/R20261002T151854Z-603d/README.md);
   - [`input-fidelity-postrun.json`](../automation/evidence/I-62-architect-v12/R20261002T151854Z-603d/input-fidelity-postrun.json);
   - [`v11-model-visible-reaudit.json`](../automation/evidence/I-62-architect-v12/R20261002T151854Z-603d/v11-model-visible-reaudit.json).
3. Las disposiciones y los registros anteriores: [V11](I-62-architect-review-v11-disposition.md) y su [registro](I-62-architect-review-v11.md),
   [V10](I-62-architect-review-v10-disposition.md), [V9](I-62-architect-review-v9-disposition.md), [requisito R62-AUTO](I-62-coordinator-requirement-auto.md),
   y los registros del Architect [V6](I-62-architect-review-v6.md) y [V7](I-62-architect-review-v7.md).
4. [Discovery R1](I-62-discovery.md); [mandato](../automation/decisions/I-62-owner-mandate.txt); [contrato](I-62-portabilidad-coordinador-principal.md);
   [decisiones](../automation/decisions/I-62.md); [evidencia](../automation/evidence/I-62-evidence.md); [estado](../automation/state/I-62.yml).
5. Autoridades: LIFECYCLE; WORKFLOW; AGENTS; AUTOMATION_PLAN completo; ADR-0046; el Freeze de I-61 ([Proposal V9 de I-61](I-61-proposal-v9.md)); los
   documentos y esquemas de `docs/automation/agent-execution/`.

## 3. Delta V12 → V13

| Área | V12 | V13 |
|---|---|---|
| Dependencias de una premisa (A62-V11-01, residuo) | §20.3.3 punto 2: un destino de referencia cruzada entraba en el envoltorio solo si la evidencia del hallazgo también lo citaba, y solo por su título o su fila | **§20.3.3, «Clausura de dependencias normativas»:** la fidelidad sigue las dependencias normativas, no las citas del revisor. El invocador calcula, sobre el texto canónico, una `NormativeDependencyClosure` transitiva: referencias explícitas, conectores de dependencia («según», «sujeto a», «salvo», «solo si», «tal como se define en»…), identificadores definidos fuera, tablas y enumeraciones, invariantes, algoritmos y transiciones, y cláusulas de autoridad o precedencia. De cada dependencia entra la unidad de control completa, nunca solo el título. Lista de trabajo con visitados hasta el punto fijo o una fuente terminal explícita; un ciclo no es error. Una referencia sin resolver deja la independencia en UNKNOWN; una aplicabilidad que no se puede determinar invalida el hallazgo. Regla de fidelidad en tres condiciones (envoltorios directos, toda la clausura y el contexto de control). El Architect no la certifica. Además: §13 (P-25), B.1, B.8.8 (`premise_independence`, I-S18), B.10.1, C-42 (h)-(n), G.1 y G.2 |
| Visibilidad de la salida de herramientas (GAP-10) | la comparación tras la corrida usaba los segmentos capturados de las lecturas; GAP-10 solo documentaba el truncamiento de la captura | **§20.3.3, «Representación entregada al revisor»:** la evidencia de fidelidad describe lo que recibió de verdad el modelo, con sus truncamientos; una línea alterada en cualquier lectura cuenta como degradada; sin una fuente de lo entregado al modelo, UNVERIFIED; sin implementación de proveedor congelada. Las lecturas acotadas son una mitigación operativa. §20.11 (GAP-10 con la medición de V12), B.8.8, C-42 (o) y G.2 |
| Compactación del contexto | no tratada | **§20.3.2:** evidencia de runtime (GAP-11); no invalida por sí sola; las premisas usadas después cumplen los mismos requisitos con la evidencia visible; un resumen cifrado nunca es evidencia. §20.11 (GAP-11), B.8.8 (`runtime_evidence`), C-42 (p) y G.2 |
| Cabecera y navegación | mapa V11 → V12 | mapa V12 → V13, revisión de V12 en `Reviews`, A62-V10-03 entre los cierres preservados, fuentes §§7-26 |

El resto de V12 queda sin cambios, incluida la corrección de A62-V10-03, que la revisión de V12 cerró.

## 4. Disposición de la sesión por hallazgo (la sesión declara; el cierre lo decide quien revisa)

| Hallazgo | Corrección en V13 | Obligación de verificación |
|---|---|---|
| A62-V11-01 (residuo) | §20.3.3: punto 2 del envoltorio sin la condición de cita; «Clausura de dependencias normativas» (quién y sobre qué, dependencias que se siguen, unidad de control completa, algoritmo con visitados y punto fijo, referencias sin resolver, aplicabilidad, evidencia); regla de fidelidad en tres condiciones (punto 3); «Representación entregada al revisor»; B.8.8 (`premise_independence` con la clausura; I-S18); B.10.1 (el Architect no declara la clausura); P-25 | C-42 (h) cuerpo del destino degradado con título y proposición fieles; (i) párrafo de control fiel; (j) cadena A → B → C con C degradada; (k) ciclo A ↔ B fiel, que termina; (l) referencia sin resolver; (m) degradación ajena a la clausura; (n) enumeración o definición referida desde una tabla, degradada; además (o) truncamiento visible con captura completa y (p) compactación |

## 5. Riesgos y preguntas para la revisión

1. **Amplitud de la clausura.** Seguir identificadores definidos fuera de la proposición puede traer secciones extensas, sobre todo en el Anexo B. Es
   conservador: más premisas pueden quedar sin acreditar bajo DEGRADED_BOUNDED, pero ninguna se acredita sin fidelidad. ¿Es aceptable ese coste?
2. **Fuente terminal.** Un documento referido como un todo entra entero y es terminal: no se siguen sus referencias internas, pero tiene que ser fiel entero.
   ¿Basta, o deben seguirse también sus referencias internas cuando controlan la proposición?
3. **Línea alterada en una sola lectura.** Una línea recibida alterada en cualquier lectura cuenta como degradada, aunque otra lectura la entregara fiel. Es
   conservador; ¿es la regla correcta?
4. **Aplicabilidad indeterminada.** Invalidar el hallazgo cuando no se puede determinar si una dependencia aplica es conservador. ¿Hay casos en que esto
   impediría acreditar casi cualquier hallazgo?

## 6. Decisiones del Owner y su frontera real

Sin cambios respecto de V12: OD-6 (acuerdo y Freeze), OD-1 (READY-03 y vigencia), OD-2, OD-3, OD-4, OD-5 y OD-7 (§18 de la Proposal). La exención de
`dotnet test` para la invocación de esta revisión no es una OD del diseño: es un parámetro de la autorización de esa invocación.

## 7. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni sesiones nuevas.
IMPLEMENTATION AUTHORIZATION = NO.

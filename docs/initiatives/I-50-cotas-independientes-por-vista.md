---
schema: rackcad-initiative/v1
id: I-50
title: "Cotas independientes por vista"
type: feature
status: claimed
branch: feature/cotas-independientes-por-vista
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [ui-editors, persistence, system-selective, architecture-kernel]
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-50 — Cotas independientes por vista

> Fase actual: **G0 CLOSED · G1 CLOSED · G2 IN PROGRESS**. G2A prepara la Proposal V1 y el ADR
> `propuesto`, **solo documentación**. No hay una sola línea de producción en la rama.
>
> ```text
> BASE_SHA      = a4d88f18a1f42263d366c44dc05dd18a6786f152
> CLAIM_SHA     = 97cc0ef66bf865175a8976e4e2452aa6a4e0d5e6
> Claim-Id      = 4d9fb22f-3c68-4fa9-b4bf-52ff9d9b8252
> Branch        = feature/cotas-independientes-por-vista
> BOOTSTRAP_SHA = a2fba4f280027f36f32d04298a48f975c999a3ee
> Discovery     = docs/initiatives/I-50-discovery.md
> ```

## 1. Objetivo

Permitir decidir de forma independiente **en qué vistas de un rack se muestran las cotas**, reutilizando el motor de cotas existente y **sin reconstruirlo**.

ID1 no pide crear cotas automáticas. El pendiente es seleccionar la visibilidad de las cotas por vista real soportada por cada sistema.

## 2. Pregunta de contrato que Discovery debe resolver

La hipótesis inicial es una política persistida por `rack + view-kind`, pero **no queda aceptada por este contrato**. Discovery debe demostrar si la autoridad correcta pertenece:

- al **tipo de vista** del rack, de modo que dos instancias `Front` compartan política; o
- a **cada instancia dibujada**, de modo que dos vistas del mismo kind puedan diferir.

Alternativas a comparar: flags explícitos, set de `ViewKind`, policy object y metadata por view instance. Si el producto no queda determinado por el comportamiento y contratos actuales, se escala al Owner antes de G2. Si Discovery exige una decisión arquitectónica transversal material, se pide revisión de Arquitecto antes de implementación.

> **Resuelta en G1.** La autoridad es **rack × tipo de vista**; la metadata por instancia queda
> rechazada. Evidencia en [I-50-discovery.md](I-50-discovery.md) §9; decisiones `CD-01` y `CD-08` en la
> sección 12.

## 3. Compatibilidad obligatoria

Los DWG existentes deben conservar **exactamente** el comportamiento visual actual. Debe existir un default legacy equivalente a la configuración vigente. Abrir o actualizar un dibujo viejo no puede hacer desaparecer cotas.

La política acordada debe sobrevivir el flujo completo:

`crear -> insertar -> RACKEDITAR -> Actualizar -> save -> reopen -> linked view insertion`.

Un redraw no puede recuperar silenciosamente una política global histórica y perder la selección por vista.

## 4. Alcance de Discovery

Auditar **todos los sistemas/productos con cotas existentes** y concretar por sistema:

- tipos de vista;
- dónde se generan cotas;
- propiedad global vigente;
- quién decide `DrawDimensions` o equivalente;
- persistencia actual;
- metadata/identidad de vista;
- insert/update/redraw;
- save/reopen;
- `RACKEDITAR`;
- library/export cuando aplique.

Entregable mínimo de G1: matriz `Sistema × Vista × cotas existentes × autoridad actual × persistencia × cambio necesario`, inventario completo de call sites de drawing y trazado create/update/reopen.

> **Entregado**: [I-50-discovery.md](I-50-discovery.md).

## 5. Alcance de producto

La fila original dice **Todos**. I-50 no puede declararse completa si solo cubre Selectivo. Discovery debe identificar qué sistemas poseen cotas y qué vistas soportan; una solución shared + adaptadores es válida si cae naturalmente del diseño existente.

No se crean controles para vistas o cotas que un sistema no posee hoy.

> **Resultado de G1**: poseen cotas **Selectivo, Dinámico y Push Back**, en sus vistas frontal, lateral y
> planta. Cantilever, Cama, Larguero y Cabecera no dibujan cotas, así que no reciben controles
> ([I-50-discovery.md](I-50-discovery.md) §3).

## 6. UI

Buscar el punto mínimo y coherente de configuración: creación, editor o una única sección `Cotas` con controles por vista según la taxonomía real. Evitar duplicar configuración en varias ventanas salvo necesidad demostrada.

**No tocar `LinkedPropertyEditor`, Expression Engine ni Project Variables salvo necesidad real demostrada.** I-49 corre en paralelo **sin conflicto declarado**; la coordinación obligatoria por archivos compartidos está en la sección 11.

## 7. Fuera de alcance

- rediseñar el motor completo de cotas;
- ID17 first-view freedom;
- ID18 multi-view queue;
- ID19 multi-rack projection;
- ID2 pallet visibility;
- ID13 blank fronts;
- Expression Engine;
- cotas nuevas no pedidas;
- refactor general de UI;
- los hallazgos H1–H7 del Discovery (`CD-09`, sección 13).

## 8. Tests mínimos de aceptación

- default legacy;
- Front ON / Side OFF / Plan OFF;
- Front OFF / Side ON;
- combinaciones relevantes por sistema;
- insert;
- update;
- RACKEDITAR;
- save/reload;
- linked sibling views;
- nueva vista creada después del cambio;
- cada sistema incluido;
- no cross-write entre vistas;
- no pérdida de geometría ni BOM.

## 9. Owner Validation

AutoCAD 2025, por cada sistema acordado: crear rack, elegir cotas por vista, insertar, verificar solo vistas seleccionadas, `RACKEDITAR`/Actualizar, save/reopen, insertar vista adicional y smoke legacy.

## 10. Gates

| Gate | Entregable | Estado |
|---|---|---|
| G0 | Preflight, claim, worktree, bootstrap | **CLOSED** — reclamo `97cc0ef`, contrato `4e22d91`, fila de ROADMAP `a2fba4f`, worktree creado |
| G1 | Characterization / matriz completa | **CLOSED** — [I-50-discovery.md](I-50-discovery.md); decisiones del Coordinador en la sección 12 |
| G2 | Contrato de autoridad + persistencia + default legacy | **IN PROGRESS** |
| G2A | Proposal V1 + ADR `propuesto` (solo documentación) | en preparación |
| G3 | UI mínima | pendiente |
| G4 | Builders / draw path | pendiente |
| G5 | Persistence / update | pendiente |
| G6 | Cobertura de sistemas | pendiente |
| G7 | Candidate + Owner AutoCAD 2025 | pendiente |
| G8 | Docs + integration + cleanup | pendiente |

**No implementation before G1/G2.** G3+ queda bloqueado hasta cerrar el contrato y, si aparece una decisión arquitectónica transversal material, hasta la revisión de Arquitecto correspondiente. **Cerrar G1 no autoriza implementar**: G3+ sigue bloqueado hasta que el Coordinador y el Arquitecto revisen la Proposal.

## 11. Coordinación con I-49 e I-51

**Sin conflicto declarado** (`CD-10`, `CD-11`). `conflicts_with` queda vacío y la fila propia de I-50 en
`docs/ROADMAP.md` ya no lista I-49 en «Se estorba con».

**I-49** (`architecture/motor-expresiones-parametricas`) corre en paralelo bajo
`OWNER_OVERRIDE_I49_I50_PARALLEL`, registrado en `docs/automation/decisions/I-49.md` de **su** rama. Ese
registro asigna a I-50 la corrección documental que hace este contrato. Reglas vigentes para I-50:

- **Coordinación obligatoria por archivos compartidos**:
  `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml` y `RackSelectiveWindow.xaml.cs` —cruce
  material: los controles de cotas y los `LinkedPropertyEditor` conviven en la misma sección y en
  `BuildDesign`/`LoadDesign` ([I-50-discovery.md](I-50-discovery.md) §11.2)— y, posiblemente,
  `src/RackCad.Application/Persistence/SelectivePalletDesignDocument.cs`.
- **Protocolo antes de editar uno de esos archivos**, mientras I-49 tenga rama remota no integrada:
  1. `git fetch --all --prune`;
  2. `git diff --name-only origin/main...origin/architecture/motor-expresiones-parametricas` y, si el
     archivo aparece, su diff concreto;
  3. si I-49 lo modifica **materialmente**: **detenerse antes de editar** y reportar el archivo, el SHA
     observado de I-49 y la naturaleza del cruce.
- No refactorizar `LinkedPropertyEditor`; no tocar Project Variables ni Expression Engine salvo necesidad
  real demostrada: son alcance funcional de I-49.
- Commits pequeños.
- Integración **serializada** con I-49: la segunda en integrar se reconcilia con `main` en su rebase final.

**I-51** (`feature/rackduplicar-multiples-origenes`) solo tiene un conflicto **documental**: su fila de
ROADMAP se inserta en el mismo punto que la de I-50. **No es `conflicts_with`**. I-50 no prevé tocar
`RackDuplicarCommands.cs` ni `RackEnvelopeRestamp.cs`, y la política de I-50 vive en el **diseño**, no en
la referencia ni en el sobre, que es lo que la decisión AM-4 de I-51 necesitaba saber.

`docs/ROADMAP.md` y `docs/ideas-futuras.md` pueden chocar textualmente con I-49 e I-51 al integrar: quien
integre después conserva todas las filas y secciones.

## 12. Decisiones vinculantes del Coordinador (2026-09-12, cierre de G1)

| ID | Decisión |
|---|---|
| `CD-01` | Autoridad = **rack × tipo de vista**, NO instancia. |
| `CD-02` | Tipos de vista de I-50 = **Frontal / Lateral / Planta**. En el Dinámico, salida y entrada comparten Frontal. En Push Back, todos sus cortes frontales comparten Frontal. `Section` **no** crea un tipo de vista. |
| `CD-03` | `DimensionDetail` sigue siendo global del rack. |
| `CD-04` | `DimensionStyle` sigue siendo global del rack. |
| `CD-05` | La visibilidad es **solo ON/OFF** por tipo de vista. |
| `CD-06` | `Dimensions = None` **siempre gana** y produce cero cotas. |
| `CD-07` | Política nueva nula o ausente = **LEGACY exacto**: todas las vistas históricamente elegibles usan `Dimensions` como hoy. No se materializa «todas» al guardar un rack legacy sin tocar las cotas. |
| `CD-08` | Metadata por instancia **RECHAZADA**. |
| `CD-09` | H1 (estilo de cota de Push Back) y H2–H7 quedan **fuera de alcance**. |
| `CD-10` | I-49 **debe** poder correr en paralelo: se quita `conflicts_with: [I-49]` de este contrato y «I-49» de «Se estorba con» en la fila propia de I-50. Se mantiene la coordinación obligatoria por `RackSelectiveWindow.xaml/.cs` y el posible `SelectivePalletDesignDocument`. |
| `CD-11` | I-51 solo tiene conflicto documental de ROADMAP; no es `conflicts_with`. |

Lo que queda para G2: la **representación** del dato. La Proposal V1 compara solo dos formas —una lista
nula de tokens de vista y un `[Flags] DimensionViewVisibility` nulo— y recomienda una.

## 13. Hallazgos fuera de alcance (`CD-09`)

Registrados **sin corregir** en [ideas-futuras.md](../ideas-futuras.md), sección «I-50»; el detalle y la
evidencia están en [I-50-discovery.md](I-50-discovery.md) §13.

| ID | Hallazgo |
|---|---|
| H1 | Push Back no tiene control de estilo de cota y cada recálculo escribe `DimensionStyle = null` |
| H2 | El lateral de Push Back envía la posición en la lista de cortes y el Plugin la lee como índice de poste |
| H3 | Los bloques anónimos `*D` de las cotas no se encolan para purga al cancelar una inserción ni en `EraseViewBlocks` |
| H4 | `HeaderRunPlan.PlacedClone` no copia los campos de texto ni de cota |
| H5 | El Dinámico descarta un estilo de cota guardado que el DWG no tiene; el Selectivo lo conserva |
| H6 | El BOM del Selectivo calcula las cotas laterales y las descarta |
| H7 | Tres comentarios desfasados (`RackBlockData.cs:8`, `RackInsertionRequest.cs:152`, `PushBackSystemLateralBuilder.cs:147-151`) |

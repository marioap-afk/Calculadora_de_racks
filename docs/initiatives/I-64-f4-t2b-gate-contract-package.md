# I-64 — Paquete del contrato de F4-T2B (lectura de la intención y ciclo de vida en el host)

**Estado:** v3, emitible. Es la v2 con las correcciones menores de su re-revisión independiente (§11). La prepara la sesión principal bajo la delegación nocturna del Owner (`F4-DRAFTS/coordinator/owner-night-delegation.md`), que registra las decisiones en F4-E26.
**Base:** rama `architecture/workspace-persistente-rackcad`, en `5c9cc24e` (F4-T2A CLOSED / PASS sobre `cf9f9441`, F4-E25). Todos los hechos de este paquete se midieron en `5c9cc24e`.
**Historia:** la revisión adversarial de la v1 dio NO EMITIBLE (1 bloqueante, 8 importantes, 8 menores; disposición en el §9). La re-revisión de la v2 dio EMITIBLE TRAS CORRECCIONES (12 menores; disposición en el §11).

## 1. Autoridades

- **Freeze** (`docs/initiatives/I-64-proposal-v5.md`):
  - D-02 (`:149-176`) y O3-01 (`:165-168`): puerto sin tipos de AutoCAD; lecturas iniciadas por el usuario en contexto de aplicación, con bloqueo, en reposo y sin modal; `TopTransaction == null` en Debug;
  - D-05 (`:250-326`): «sin Id» = `IsNullOrWhiteSpace` (O-08), un solo diagnóstico «ilegible o de versión futura» (O3-03), Kind divergente entre miembros;
  - D-10 (`:408-442`): nacimiento de base e inicial en una lectura, preview sin AUTH-06, INV-27;
  - D-11 (`:445-462`): pertenencia, origen de apertura y admisión;
  - D-12 (`:463-499`): StaleBase, «Comprobar ahora»; la relectura caller-owned es de T4A;
  - D-17 (`:595-619`): error de host en una lectura, veto de cierre y aviso de pérdida;
  - INV-14, INV-16, INV-17, INV-20, INV-22, INV-27 e INV-31 (`:752-769`).
- **Paquete de F4 v2** (`docs/initiatives/I-64-f4-gate-contract-package.md`): fila de T2 (`:45`), obligaciones 3, 4, 10, 11, 13, 14, 15 y 18 (`:65-86`), reglas de la StaleBase (`:96-102`).
- **Paquete de F4-T2** (`docs/initiatives/I-64-f4-t2-gate-contract-package.md`): §3 (fila de T2B, `:53`; presupuesto, `:68-71`), §5 (obligaciones, `:236-277`), §6 (precedente de contrato, `:279-324`) y §7 (`:326-335`).
- **Disposiciones:** F4-E09 (presupuesto por tarea), F4-E16 (M-6, M-9, una entrada por clave), F4-E18 (división de T2), F4-E21 (`ForbiddenWriteScope` por prefijos seguros), F4-E25 (arrastres).
- **Aclaraciones:** IC-04, IC-06 e IC-07.
- **Producto cerrado:** núcleo de borradores (`Workspace/Drafts/`) y piloto (`Systems/PushBack/Pilot/`).

## 2. Hallazgos del repositorio (medidos en `5c9cc24e`)

| Id | Hallazgo | Consecuencia |
|---|---|---|
| B-H01 | `RackSiblingScan.CaptureDrawing(document)` (`src/RackCad.Plugin/Views/RackSiblingScan.cs:86-87`) recorre la BlockTable con su propio `LockDocument` y su propia transacción con `Commit` (`:102-103`, `:152`). Lee el texto crudo con `RackBlockData.Read` (`:111`) y no lo conserva. La clave es `definitionId.Handle.ToString()` (`:115`). Sin fuente elegida: `IsSelectedSource` es false y `EraseByKind` es false en todos los hechos (`:87`, `:135`, `:142`). `Section` está en el sobre (`RackEmbedDocument.Section`, int, -1 por defecto), en `BrowserRackFact` y en la lectura de AUTH-13 (`AuthoredRawReader.cs:41`); el hecho del barrido no la lleva | La proyección solo necesita el texto crudo: `View` sale del hecho y `Section` del mismo texto crudo |
| B-H02 | `RackSiblingMembership.Classify` (`src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs:93-155`) da Redraw, Erase, ReadOnly, NotMember o BlockingUnreadable. Lanza si el RackId pedido está en blanco (`:100`), si falta un hecho o su clave, si una clave se repite o si un recuento es negativo (`:112-125`). Un sobre interpretable con el Id vacío es NotMember (`:138-141`). La atribución usa `OrdinalIgnoreCase` (`:102`). `MutableMembers` = Redraw ∪ Erase = `AuthoredGateMembers` (`:66-89`). `MembershipFor(rackId)` llama `Classify(Facts, rackId, null, false)` (`RackSiblingScan.cs:63-64`) | Las cuatro precondiciones estructurales las garantiza el barrido; el RackId en blanco y el Id vacío del origen se comprueban antes, sin lanzar |
| B-H03 | AUTH-13: `RackAuthoredComparatorPorts.PushBack()` (`src/RackCad.Application/Systems/Shared/RackAuthoredComparator.FourKinds.cs:18-20`, `:49-82`) da Single, Divergent o Unreadable. Exige `IsComplete`, una evidencia de completitud no vacía y al menos un hermano (`:51-53`); convierte cada `JsonException` del lector en Unreadable «AUTH-13 raw: …» (`:68-70`); compara el `Name` y el valor canónico (`:74-77`). El lector exige el sobre crudo y el `Design` crudo, y comprueba que el `Design` del sobre crudo es ordinalmente igual al `RawDesign` dado (`AuthoredRawReader.cs:19-52`). Compara el Id con el RackId de forma ordinal (`:47`) y el Kind sin mayúsculas (`:23-24`, `:48`); rechaza todo miembro desconocido del sobre (`:44`). Solo admite el esquema `1.0` (o null) en el sobre y `1.0` o `2.0` en el proyecto (`:133-141`). Los llamadores actuales le pasan el sobre re-serializado (`RackUnsupportedSiblingInsert.cs:64`, `RackProjectionKindSessions.cs:231`) | El piloto le pasa el texto crudo real (más estricto; ver D-T2B-06) |
| B-H04 | `RackProjectionKindSessions.SiblingInput()` (`src/RackCad.Plugin/Views/RackProjectionKindSessions.cs:223-237`): un hermano por miembro de la compuerta authored con sobre; `complete` = ningún miembro BlockingUnreadable **y** tantos hermanos como miembros de la compuerta (`:234-235`); evidencia `"RackSiblingScan/CaptureDrawing"` (`:236`) | Regla de `IsComplete` y evidencia del piloto |
| B-H05 | `DraftStaleBase.Create(entries, variableRegistryText)` (`Drafts/DraftStaleBase.cs:80`): una entrada por clave; NotMember se descarta; el texto solo se guarda para Mutable y ReadOnly; las ambiguas nunca coinciden. `DraftIntentRead<TState>.Admitted(base, state)` y `.NotAdmitted(diag)`; NotAdmitted deja `StaleBase = null` (`Drafts/DraftSession.cs:25-52`) | El resultado del piloto lleva su propia base (B-1) |
| B-H06 | `DraftSession.CheckNow(DraftStaleBase reread)` (`Drafts/DraftSession.cs:571-589`) solo sirve a Desconocido con origen `CommitThrew`: MATCH → Recovered; diferencia → Conflict con las diferencias | «Comprobar ahora» necesita una base también sin admisión |
| B-H07 | `PushBackPilotMaterializer` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs`): `UnreadableOrFutureDiagnostic` (`:100`) para el texto en blanco, el sobre ilegible o futuro y el diseño interior ilegible o futuro (`:126`, `:131`, `:137-138`); `KindDivergentDiagnostic` con comparación ordinal (`:132-133`); `NoDesignDiagnostic` (`:141`); `OutOfPilot` para A/B y cola (`:144`, `:165-166`); `CatalogRequiredDiagnostic` si el catálogo es null (`:127`); nunca lanza (`:173-175`) | Las reglas 12-14 de la admisión reutilizan sus resultados |
| B-H08 | `NavigationTarget.DefinitionHandle` es un `long` (`src/RackCad.Application/Workspace/NavigationTargetSet.cs:17-19`); `NavigationTargetSet.Primary` es el primer objetivo del orden determinista (`:52-53`). La clave del barrido es `Handle.ToString()`. Los efectos de navegación convierten el `long` con `new Handle(handle)` (`src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs:41`, `:132`) | La conversión de la clave del origen se hace en el Plugin con el mismo `Handle` (T2B2) |
| B-H09 | El catálogo del Plugin sale de `LateralHeaderDrawService.LoadCatalog()` (`src/RackCad.Plugin/Drawing/LateralHeaderDrawService.cs:299`, que llama `RackCatalogLoader.Load()`); lo usan los comandos de Push Back (`RackPushBackCommands.cs:268`) | Fuente del catálogo en T2B2, cargado fuera del bloqueo |
| B-H10 | Push Back no usa el registro de variables: ninguna ocurrencia en `src/RackCad.Application/Systems/PushBack`, en el dominio de Push Back ni en `RackPushBackCommands.cs`. D-10 y D-12 lo limitan a Selectivo (`:416`, `:475-477`) | `VariableRegistryText = null` en el piloto |
| B-H11 | La guarda de preview (`tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs:547-567`) recorre 4 archivos y solo veta 4 nombres de tipo (`:555`) | Una guarda nueva, aditiva, cubre los tipos nuevos (T2B1) |
| B-H12 | Guardas del host: `Plugin/Workspace` tiene exactamente 9 archivos (`WorkspaceBrowserHostGuardTests.cs:38`); 16 manejadores del puente (`WorkspaceBridgeFacadeSourceGuardTests`); fuera de `Plugin/Workspace` ningún archivo del Plugin nombra `RackCad.Application.Workspace` (`WorkspaceBridgeSourceGuardTests.cs:353-358`). `OnBeginDocumentClose` siempre devuelve Allow y el puente descarta el valor; `DocumentToBeDestroyed` no lleva el `SessionId`; `MarkDraftsPossiblyStale`, `DraftClosePolicy` y `RetireSession` no tienen llamador fuera de `Drafts/` (`EventBridgeCore.cs:242`, `:321-333`) | T2B2 |
| B-H13 | Líneas base de la CI de `5c9cc24e` (corrida `43171988530`, success): `FullyQualifiedName~RackCad.Tests.Workspace` = 1468 y `FullyQualifiedName~RackCad.UI.Tests.Workspace` = 248, todas Passed | Mínimos del contrato de T2B1 |

## 3. Descomposición y presupuesto

| Tarea | Capa | Alcance | Mutación |
|---|---|---|---|
| **F4-T2B1-INTENT-READ** | Application pura (piloto) y pruebas Core | Entrada pura con los hechos del barrido y el texto crudo por clave; StaleBase total; admisión ordenada sin lanzar, con diagnósticos unificados; resultado envoltorio con la base siempre, la lectura de la intención, la clave de origen y el estado del testigo de la preview; guardas nuevas | Ninguna |
| **F4-T2B2-HOST-LIFECYCLE** | Plugin (`Views/RackSiblingScan.cs`, adaptador nuevo y `Plugin/Workspace`), Application (`EventBridgeCore`, `WorkspaceHostCore`) y pruebas | Proyección aditiva del texto crudo; adaptador de lectura y lector del testigo; elección del origen desde `NavigationTargets` revalidados; registro de borradores en el host; punto de entrada de la intención con INV-22; «Comprobar ahora»; recarga perezosa por acción del usuario; marcado de obsolescencia (C-F4-04); veto real; sesión destruida, `RetireSession` y aviso de pérdida (§6) | Ninguna |

- **Por qué la proyección y el adaptador pasan a T2B2 (M-6):** un adaptador sin cablear solo se verifica con guardas de texto. En T2B2 se prueba con su llamador real.
- **Presupuesto (M-3):** cada subtarea empieza en 1 de 3 (F4-E09). T2B1 recibe el arranque de F4-E25. La división duplica la capacidad de intentos de T2B (de 3 a 6). Se registra en F4-E26 y se avisa al Owner en el informe de la mañana; no detiene la cadena (delegación nocturna). El agotamiento de cualquiera de las dos es S-11 y STOP de F4-T2.

## 4. Decisiones de T2B1

### D-T2B-01. Entrada pura: `PushBackPilotIntentScan`

Tipo inmutable del espacio `RackCad.Application.Systems.PushBack.Pilot`. Su constructor **copia** las colecciones (como `RackAuthoredInput.cs:19`): los hechos a una lista de solo lectura y el texto crudo a un `Dictionary` nuevo con `StringComparer.Ordinal` (N-7). Lleva:
- `RackId` (string): el RackId de la pestaña;
- `OriginKey` (string): la clave de la definición de origen de D-11, en el formato de la clave del barrido; la elige T2B2;
- `Facts` (`IReadOnlyList<RackSiblingScanFact>`): los hechos **sin clasificar**, tal como los da el barrido;
- `RawEnvelopes` (`IReadOnlyDictionary<string, string>`, ordinal): el texto crudo de cada definición por clave; un valor null equivale a una clave ausente;
- `CompletenessEvidence` (string): la evidencia que se pasa a AUTH-13; T2B2 pone `"RackSiblingScan/CaptureDrawing"`;
- `PreviewWitness` (`PushBackPilotPreviewWitness` o null): el registro `PushBackPilotPreviewWitness(string Key, string? RawEnvelopeText)`, la clave y el texto crudo del sobre que mostró la preview (D-T2B-05), en el mismo archivo.

**Precondiciones estructurales** (las garantiza el barrido por construcción): colecciones no nulas; ningún hecho nulo; claves no vacías y únicas; recuentos no negativos; ningún hecho con `IsSelectedSource` (el barrido de la intención no selecciona fuente, y un hecho seleccionado sale Redraw sin mirar el sobre, `RackSiblingMembership.cs:128-131`); `CompletenessEvidence` no vacía; y, si hay testigo, su clave no vacía. Si se violan, el constructor o el lector lanzan `ArgumentException` antes de hacer nada más: es un error de programación que contiene la frontera de T2B2 (D-17). **Ningún estado del dibujo lanza**, incluidos el RackId en blanco, el Id vacío, los sobres ilegibles y los textos con UTF-16 inválido (I-2, N-5).

### D-T2B-02. StaleBase total: `PushBackPilotIntentReader.BuildStaleBase(scan)`

Función pura que no depende de la admisión (B-1):
- **RackId en blanco** (`IsNullOrWhiteSpace`): base vacía, sin entradas y sin lanzar (no se llama a `Classify`).
- **Clasificación:** `RackSiblingMembership.Classify(Facts, RackId, null, false)`, los mismos argumentos que `MembershipFor` (consumo sin cambios, D-11).
- **Pertenencia:**

  | De `RackSiblingMembershipKind` | A `DraftMembership` | `View` | `Section` | Texto |
  |---|---|---|---|---|
  | Redraw, Erase | Mutable | el del hecho | la del sobre, en texto invariante | el crudo de `RawEnvelopes` |
  | ReadOnly | ReadOnly | ídem | ídem | ídem |
  | BlockingUnreadable | Blocking | null | null | null |
  | NotMember | no se pasa (desaparece de la base) | — | — | — |

- **Deserialización local:** `Section`, `Kind` y `Design` salen de `RackEmbedStore.Deserialize` del **mismo** texto crudo, envuelta en un `try` que trata **cualquier** excepción como «no se deserializa» (`Deserialize` solo captura `JsonException`, `RackEmbedDocument.cs:105-112`; N-5). `Section` se escribe con `ToString(CultureInfo.InvariantCulture)`.
- Si el texto de un miembro interpretable falta o no se deserializa (entrada incoherente), la entrada queda con `Section` null y el texto tal cual (o null): la base nunca coincide con una relectura coherente y la admisión lo rechaza (reglas 3 y 7).
- `VariableRegistryText` = null: Push Back no usa variables y el piloto no lee el registro (I-4, B-H10). Si F5 lo necesita, tendrá que codificar el estado del payload (Absent, PresentButUnreadable, texto).
- **Una entrada por clave**, sin normalización (CRLF incluido).
- **Recorte (M-8):** la base solo retiene texto de los miembros. El resultado no conserva `RawEnvelopes` ni la entrada.
- T4A reutilizará esta misma función en la relectura caller-owned y en la comprobación previa O-04 (arrastre).

### D-T2B-03. Admisión: `PushBackPilotIntentReader.Read(scan, catalog)`

La base se construye **siempre** con D-T2B-02. Después se evalúan las reglas en este orden; la primera que falla da NotAdmitted con su diagnóstico. Las reglas 3 a 6 leen el **hecho** de origen y su clasificación, nunca el sobre deserializado (N-4):

| N.º | Regla | Diagnóstico |
|---|---|---|
| 1 | `RackId` en blanco | `NoIdentityDiagnostic` (INV-16) |
| 2 | `OriginKey` en blanco o sin hecho en `Facts` | `OriginMissingDiagnostic` |
| 3 | El hecho de origen no es interpretable (`EnvelopeInterpretable` false), o su texto falta o no se deserializa | `UnreadableDiagnostic` (INV-17) |
| 4 | El Id del hecho de origen está en blanco (`IsNullOrWhiteSpace`, O-08) | `NoIdentityDiagnostic` (INV-16) |
| 5 | El Id del hecho de origen no es el `RackId` (`OrdinalIgnoreCase`, la igualdad de `Classify`), o el origen no quedó clasificado Redraw ni Erase | `OriginNotMemberDiagnostic` |
| 6 | El hecho de origen es de una referencia externa (`IsXrefDependent`, ReadOnly) | `OriginReadOnlyDiagnostic` |
| 7 | Hay algún miembro Blocking, o el texto de algún miembro Mutable falta o no se deserializa | `UnreadableDiagnostic` (INV-17) |
| 8 | Los miembros Mutable tienen Kinds distintos (`OrdinalIgnoreCase`, la igualdad del lector de AUTH-13) | `KindDivergentDiagnostic` (INV-17) |
| 9 | El Kind del origen no es `RackEmbedDocument.KindPushBack` (ordinal, como el camino de RACKEDITAR que sigue `Materialize`) | `OutOfPilotDiagnostic`, con el Kind como detalle. Cama cae aquí: es residual de F5 (M-2; D-11 la admite con un solo miembro mutable) |
| 10 | AUTH-13 `PushBack()` sobre los miembros Mutable ordenados por clave (ordinal), con el `RackId` de la entrada, el texto crudo real, `IsComplete` (D-T2B-06) y `CompletenessEvidence`: Divergent | `DivergentDiagnostic`, con el mensaje del comparador como detalle |
| 11 | AUTH-13 Unreadable | `UnreadableDiagnostic`, con el mensaje del comparador como detalle (INV-17) |
| 12 | `Materialize(texto de origen, catalog)` = OutOfPilot | `OutOfPilotDiagnostic`, con el motivo del materializador como detalle |
| 13 | `Materialize` = Failed con `UnreadableOrFutureDiagnostic` | `UnreadableDiagnostic` (INV-17) |
| 14 | `Materialize` = Failed con cualquier otro motivo (sin diseño, catálogo null, excepción interna) | `MaterializeDiagnostic`, con el motivo como detalle |
| — | `Materialize` = Admitted | `DraftIntentRead.Admitted(base, estado)` |

**Excepciones inesperadas (N-6):** una excepción que lance el comparador de AUTH-13 (pasos 10-11) se trata igual que una del materializador: `MaterializeDiagnostic`, con el mensaje como detalle. Nunca da una admisión.

**Textos exactos** (constantes públicas de `PushBackPilotIntentReader`; todas empiezan por «No editable aquí: » y siguen en minúscula, M-1 y N-1):
- `NoIdentityDiagnostic` = `"No editable aquí: el sobre no tiene identidad"`
- `OriginMissingDiagnostic` = `"No editable aquí: el origen ya no está en el dibujo"`
- `UnreadableDiagnostic` = `"No editable aquí: ilegible o de versión futura"`. Es el **único** texto de lo ilegible o futuro (O3-03, I-1); el `UnreadableOrFutureDiagnostic` del materializador queda como su motivo interno.
- `OriginNotMemberDiagnostic` = `"No editable aquí: el origen pertenece a otro rack"`
- `OriginReadOnlyDiagnostic` = `"No editable aquí: el origen es una referencia externa"`
- `KindDivergentDiagnostic` = `"No editable aquí: los miembros del rack son de sistemas distintos"`
- `OutOfPilotDiagnostic` = `"No editable aquí: fuera del piloto (F5)"`, el texto de IC-07 (`I-64-implementation-clarifications.md:119`) y de D-T2-02.
- `DivergentDiagnostic` = `"No editable aquí: las definiciones del rack no coinciden"`
- `MaterializeDiagnostic` = `"No editable aquí: no se pudo preparar el borrador"`
- `ClassicEditorHint` = `"Usa RACKEDITAR para editarlo con el editor clásico."`

**Reglas de los diagnósticos:**
- `DraftAdmission.Diagnostic` lleva el texto principal. El detalle técnico va aparte, en el resultado, nunca en el texto principal.
- Todo NotAdmitted lleva `ClassicEditorHint`; ningún archivo lanza un comando (C-F4-07).
- Kind divergente (regla 8) va antes que «fuera del piloto» (regla 9): un rack con Kinds mezclados nunca se muestra como «fuera del piloto» (INV-17, M-1).
- La regla 3 tiene definición operativa: el hecho no interpretable es exactamente lo que el lector integrado devuelve como sobre nulo: texto vacío, JSON inválido o versión mayor futura (`RackEmbedDocument.cs:99-102`, `:105-112` y `:119-126`). No se inventa una distinción (O3-03).
- Ninguna regla lanza.

### D-T2B-04. Resultado: `PushBackPilotIntentResult`

- `StaleBase` (`DraftStaleBase`, **nunca null**): la de D-T2B-02, también sin admisión (B-1).
- `Read` (`DraftIntentRead<PushBackPilotDraftState>`): Admitted con esa misma base, o NotAdmitted con el diagnóstico.
- `OriginKey` (string): la clave de origen de la entrada.
- `Detail` (string o null): el detalle técnico del diagnóstico.
- `ClassicEditorHint` (string o null): presente si y solo si no hay admisión.
- `PreviewWitness` (enum `PushBackPilotPreviewWitnessStatus`): `NotSupplied`, `Unchanged` o `Changed` (D-T2B-05).

### D-T2B-05. Una sola lectura (R4, M-9) e INV-27

- **Una lectura:** el texto de origen que recibe `Materialize` es la **misma cadena** de `RawEnvelopes` que queda en la entrada de origen de la base; se prueba con `ReferenceEquals(InitialState.Origin, RawEnvelopes[OriginKey])` y con la igualdad ordinal del texto de la entrada de origen de la base. El estado nunca se materializa desde la base: los tres archivos de producto no leen `Entries`, `EnvelopeText` ni `MembershipOf` (M-9).
- **Testigo de la preview (I-6, N-8):** el par (clave, texto crudo leído con `RackBlockData.Read`) que T3 captura al mostrar la preview, con la misma regla de origen que T2B2.
  - Sin testigo → `NotSupplied`. T3 no puede tratarlo como «sin cambios».
  - Clave igual a `OriginKey` y texto no nulo y ordinalmente igual al de `RawEnvelopes[OriginKey]` → `Unchanged`.
  - Cualquier otra cosa (otra clave, texto null, otro texto, un solo carácter o un CRLF distinto, origen ausente) → `Changed`. T3 re-renderiza desde esta lectura y no aplica la pulsación; la base y el inicial ya son los de la lectura nueva.

### D-T2B-06. `IsComplete`, Kind y `RawDesign` de cada hermano (M-4, M-5, M-7)

- Para cada miembro Mutable, en orden ordinal de clave, el hermano de AUTH-13 es `RackAuthoredSibling(clave, Kind, texto crudo, Design)`. `Kind` y `Design` salen de la deserialización local del mismo texto crudo (D-T2B-02). El orden por clave hace determinista el detalle, que el comparador toma del primer error (`FourKinds.cs:68`; N-6).
- `IsComplete` sigue la regla integrada (B-H04): ningún miembro Blocking y un hermano por miembro Mutable. Al llegar a la regla 10 siempre es true, porque la regla 7 ya rechazó lo demás; se calcula igual y no se supone.
- **Más estricto que Insertar y RACKPROYECTAR (M-5):** el piloto pasa el texto crudo real, que es lo que declara el lector de AUTH-13 («original raw»), y no un sobre re-serializado. Un sobre que el deserializador tolerante acepta y el lector estricto rechaza (por ejemplo, miembros repetidos que solo difieren en mayúsculas) da Unreadable solo en el panel. Solo puede dar rechazos falsos, nunca admisiones falsas. Smoke-2 mide la distribución de la admisión (O-09).
- **Mayúsculas del Id (M-7):** `Classify` atribuye sin mayúsculas y AUTH-13 compara el Id con el RackId de forma ordinal. Un miembro con un Id que solo difiere en mayúsculas queda Mutable y AUTH-13 da Unreadable: `UnreadableDiagnostic` con el detalle del comparador. Se prueba. El texto principal dice «ilegible» de un sobre legible: es un residual aceptado (§7).

### D-T2B-07. Catálogo (I-8)

`Read` recibe el `RackCatalog` como parámetro. Si es null, la regla 14 da `MaterializeDiagnostic` con `CatalogRequiredDiagnostic` como detalle. T2B2 lo carga con `LateralHeaderDrawService.LoadCatalog()` **fuera** del bloqueo del documento.

### D-T2B-08. Pruebas de T2B1

**StaleBase** (`WorkspaceDraftPilotIntentStaleBaseTests.cs`):
- la tabla de pertenencia completa, incluido Erase → Mutable;
- una entrada por clave, texto ordinalmente exacto (con CRLF), `View` tal cual y `Section` invariante («-1», «3»);
- `VariableRegistryText` null;
- RackId en blanco (`""`, `" "`): base vacía, sin lanzar;
- independencia de la admisión: la base de un resultado NotAdmitted y la de uno Admitted coinciden (`CompareWith` sin diferencias) con `BuildStaleBase` de la misma entrada;
- recorte: ninguna entrada ni texto de definiciones de otros racks;
- independencia del orden de los hechos;
- cada precondición estructural violada → `ArgumentException`;
- un texto con UTF-16 inválido (un surrogate suelto, escrito en C# como `(char)0xD800`) no lanza (N-5);
- la entrada copia sus colecciones: mutar la lista y el diccionario del llamador después de construirla no cambia el resultado (N-7);
- **«Comprobar ahora» (B-1):** un borrador en Desconocido por `CommitThrew` recibe la base de una relectura NotAdmitted que difiere (un miembro pasó a Blocking; o AUTH-13 Divergent) → Conflict con sus diferencias; con una relectura igual → Recovered.

**Lectura y admisión** (`WorkspaceDraftPilotIntentReadTests.cs`):
- admitido con un miembro (`pb-01-base`) y con dos vistas del mismo rack con el mismo diseño;
- cada regla de D-T2B-03 con su constante exacta, su detalle cuando lo hay y el `ClassicEditorHint`;
- **orden (N-3):** estos pares, en que fallan las dos reglas, dan el diagnóstico de la primera: (1 sobre 2), (2 sobre 7), (4 sobre 7), (5 sobre 7), (6 sobre 7), (7 sobre 8), (8 sobre 9, con origen Selectivo y otro miembro Push Back), (9 sobre 10, con un origen Selectivo solo), (10 sobre 12, un A/B con dos miembros de diseño distinto) y (11 sobre 12, un A/B con un Id solo distinto en mayúsculas). Los pares (2,3), (10,11), (12,13), (12,14) y (13,14) son mutuamente excluyentes y no se exigen;
- INV-16: Id `""` y `" "` en el origen; ningún caso lanza;
- INV-17: miembro ilegible o futuro, Kinds mezclados con origen Selectivo (da Kind divergente, no «fuera del piloto»), esquema interior futuro, AUTH-13 Unreadable por `Design` crudo distinto y por miembro desconocido;
- M-7 (Id solo distinto en mayúsculas) y M-5 (miembros repetidos solo distintos en mayúsculas);
- A/B (`pb-20-ab`) y cola (`pb-21-cola`) → «fuera del piloto» con su motivo; Cama y Selectivo → «fuera del piloto» con el Kind;
- AUTH-13 Divergent con dos sobres de diseño o `Name` distintos; el detalle no depende del orden de los hechos;
- catálogo null;
- una lectura: `ReferenceEquals(InitialState.Origin, RawEnvelopes[OriginKey])` y el texto de la entrada de origen de la base ordinalmente igual;
- el testigo: `NotSupplied`, `Unchanged`, `Changed` por texto (un carácter, CRLF frente a LF), por texto null, por clave y por origen ausente; una clave de testigo vacía → `ArgumentException`.

**Guardas nuevas** (`WorkspaceDraftPilotIntentGuardTests.cs`; ninguna guarda existente se toca):
- los **tres** archivos de producto no nombran `Autodesk`, `RackCad.Plugin`, `RackCad.UI`, `System.Windows`, `System.IO`, `Transaction`, `LockDocument`, `Dispatcher`, `Task`, `async` ni `await`;
- M-9: los **tres** archivos de producto no contienen `.Entries`, `.EnvelopeText` ni `MembershipOf(` (N-2);
- una sola llamada textual a `RackSiblingMembership.Classify(`, a `RackAuthoredComparatorPorts.PushBack()` y a `PushBackPilotMaterializer.Materialize(` en `PushBackPilotIntentReader.cs`;
- los 4 archivos de preview de `WorkspaceDraftBoundaryTests` no nombran `PushBackPilotIntent` ni el espacio `RackCad.Application.Systems.PushBack.Pilot` (obligación 3, B-H11), con su prueba de detección por inyección en el texto real de cada archivo.

## 5. Contrato propuesto de F4-T2B1-INTENT-READ

**Objetivo:** implementar en Application pura la lectura de la intención del piloto (D-T2B-01 a D-T2B-07) con sus pruebas y guardas (D-T2B-08): StaleBase total, admisión ordenada con diagnósticos unificados y resultado envoltorio, sin AutoCAD y sin tocar el núcleo ni el piloto cerrado.

**Régimen** (como F4-T2 §6):
- RED y GREEN exactos. El RED compila y falla por aserción: es un esqueleto inerte de los tres tipos nuevos que devuelve valores **no nulos** e inertes (una base vacía, `NotAdmitted` con un texto provisional y `NotSupplied`) y nunca lanza, para que las pruebas fallen por aserción y no por `NullReferenceException` (N-9), porque el GREEN no puede tocar ningún archivo de `tests/` del RED (16.8).
- CI del SHA exacto; verificación del Controller (14 comprobaciones); revisión directa de la sesión; revisión adversarial independiente.
- **nc4** antes de aceptar la primera delegación, porque T2B1 abre una cadena nueva; nc1..nc3 tras la verificación.
- El prompt del Worker prohíbe los términos de 16.10 también en nombres de archivo, rutas y artefactos.

**`AllowedWriteScope`** (la lista blanca exacta):
- `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentScan.cs`
- `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs`
- `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentResult.cs`
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs`
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentReadTests.cs`
- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs`
- `tests/RackCad.Tests/Fixtures/I64/intent/` (fixtures nuevos; hoy no existe nada bajo ese prefijo, y queda fuera del censo `pb-*.json` de las pruebas de UI)

`PushBackPilotPreviewWitness` vive en `PushBackPilotIntentScan.cs` y `PushBackPilotPreviewWitnessStatus` en `PushBackPilotIntentResult.cs`.

**`ForbiddenWriteScope`:** solo prefijos seguros (ninguna ruta permitida debajo), por la regla de F4-E21, más los archivos de guarda y de proyecto. Lo genera un script desde el árbol y tiene 58 entradas. Incluye `src/RackCad.UI/`, `src/RackCad.Plugin/`, `src/RackCad.Domain/`, `src/RackCad.Application/Workspace/`, `src/RackCad.Application/Views/`, `src/RackCad.Application/Systems/Shared/`, `src/RackCad.Application/Persistence/` y `tests/RackCad.UI.Tests/`. Los archivos existentes de los directorios mixtos (los cuatro del piloto, las pruebas no guarda de `tests/RackCad.Tests/Workspace/` y los fixtures `pb-*.json`) quedan fuera por `AllowedWriteScope`, que es la lista blanca.

**Invariantes** (el contrato los lleva en ASCII):
- **INV-F4T2B1-01, pureza:** los tres tipos son puros e inmutables, viven en `RackCad.Application.Systems.PushBack.Pilot` y no referencian Plugin, UI, `Autodesk`, IO, `Transaction`, tareas ni el dispatcher.
- **INV-F4T2B1-02, entrada:** D-T2B-01, con la copia de las colecciones y las precondiciones estructurales.
- **INV-F4T2B1-03, StaleBase total:** D-T2B-02.
- **INV-F4T2B1-04, resultado:** D-T2B-04; la base de `Read` admitido es la misma instancia que la del resultado.
- **INV-F4T2B1-05, admisión:** el orden y los textos exactos de D-T2B-03, con el trato de las excepciones inesperadas.
- **INV-F4T2B1-06, INV-16:** RackId en blanco u origen sin Id dan `NoIdentityDiagnostic`, sin lanzar y sin inventar RackId.
- **INV-F4T2B1-07, INV-17:** lo ilegible o futuro da siempre `UnreadableDiagnostic`; el Kind divergente nunca se oculta tras «fuera del piloto».
- **INV-F4T2B1-08, una lectura (R4, M-9):** D-T2B-05, primer punto.
- **INV-F4T2B1-09, AUTH-13:** D-T2B-06.
- **INV-F4T2B1-10, testigo (INV-27):** D-T2B-05, segundo punto.
- **INV-F4T2B1-11, «Comprobar ahora» (B-1):** la base de una relectura NotAdmitted sirve a `CheckNow`.
- **INV-F4T2B1-12, catálogo:** D-T2B-07.
- **INV-F4T2B1-13, guardas nuevas (N-2):** las cuatro guardas de D-T2B-08, con la prueba de inyección de la guarda de preview; ninguna guarda existente cambia.
- **INV-F4T2B1-14, cobertura:** el RED cubre como mínimo INV-02 a INV-12 y las pruebas de D-T2B-08; el GREEN no toca `tests/`.

**Pruebas:**
- `FullyQualifiedName~RackCad.Tests.Workspace`: mínimo 1468 (B-H13) más las nuevas; `ExpectRed` true.
- `FullyQualifiedName~RackCad.UI.Tests.Workspace`: mínimo 248, sin pruebas nuevas; `ExpectRed` false (verde en el RED y en el GREEN; precedente `AUTOMATION_PLAN.md:620`).
- Las suites completas de los dos proyectos, en verde en la CI del SHA exacto del GREEN.

**Paradas:**
- el lector exige modificar un archivo existente (núcleo, piloto, `Classify`, AUTH-13 o `RackEmbedStore`);
- una regla exige una semántica de producto que D-11 no fija (ambigüedad real: STOP; un detalle de implementación se resuelve con la opción conservadora, la que nunca admite de más);
- `Classify`, AUTH-13 o `Materialize` se comportan distinto de lo medido en §2;
- una prueba necesita AutoCAD.

## 6. Esbozo de T2B2 (se contrata tras cerrar T2B1)

Su paquete fijará, con archivos exactos y excepciones de guarda por archivo exacto, sin debilitar ninguna regla:
1. **Proyección aditiva** en `RackSiblingDrawingScan`: un diccionario ordinal de solo lectura con el texto crudo de cada definición leída por `RackBlockData.Read`. Ninguna firma ni consumidor existente cambia; solo `Traverse` lo construye. `Section` no se proyecta aparte: sale del mismo texto crudo en `BuildStaleBase` (desviación de la letra de F4 v2 `:45` y de F4-T2 `:53`, equivalente, registrada en F4-E26; N-11).
2. **Adaptador de lectura** (en `Plugin/Views`, fuera de `Plugin/Workspace`, sin nombrar `RackCad.Application.Workspace` ni construir `RackEmbedDocument`):
   - comprueba el documento esperado **antes** del bloqueo;
   - carga el catálogo **fuera** del bloqueo (B-H09);
   - un solo `using (document.LockDocument())`; `RackSiblingScan.CaptureDrawing` en el nivel superior, sin transacción exterior (la del barrido no queda anidada; I-5); una sola llamada textual a `CaptureDrawing(`;
   - comprobación Debug de `TopTransaction == null` antes y después (obligación 18, INV-14);
   - no lee el registro de variables (I-4);
   - captura `LockViolation` (`Autodesk.AutoCAD.Runtime.Exception`) y `System.Exception` y devuelve un fallo tipado (D-17; precedente `WorkspaceSelectionReader.cs:73-74`, `:109-115`);
   - arma `PushBackPilotIntentScan` y llama a `PushBackPilotIntentReader.Read`;
   - ofrece también la lectura ligera del **testigo de la preview** (`RackBlockData.Read` del origen) para T3, en el mismo archivo fuera de `Plugin/Workspace`, para que T3 no necesite una excepción a las guardas de `RackBlockData` del host (`WorkspaceHostSourceGuardTests.cs:293-297`, `WorkspaceBridgeFacadeSourceGuardTests.cs:125`; N-12 d).
3. **Origen (I-3, N-12 e):** desde los `NavigationTargets` de la sesión **revalidados en la misma lectura**, nunca desde una caché caducada: la definición de la referencia elegida si el usuario viene de una referencia; si no, `Primary` (orden de D-07). La clave se convierte con `new Handle(target.DefinitionHandle).ToString()`, el mismo método que produce la clave del barrido. Prueba: la conversión y la elección; un origen fuera de los objetivos se rechaza.
4. **Host:** propiedad del `DraftRegistry` por sesión; el host no conserva lo materializado fuera del borrador (R2): guarda de que ningún campo del host es de los tipos del piloto.
5. **Punto de entrada de la intención** para T3: petición ligada a la sesión y al documento exactos (INV-22: la petición de A con B activo se rechaza sin efectos); lectura en contexto de aplicación, en reposo, sin modal y con la espera de D-10 (O3-01); nunca dentro de la pila del drenaje (IC-04) ni del Idle mientras no exista T2C. Acepta el testigo de la preview de T3.
6. **«Comprobar ahora»:** relectura con el mismo adaptador y `CheckNow(result.StaleBase)` (B-1).
7. **Recarga perezosa** de un borrador Limpio y posiblemente obsoleto, solo por acción del usuario (O-01): `DiscardAndReload(result.Read)`.
8. **Obsolescencia y C-F4-04, parte de marcado (N-12 a):** consumo de `MarkDraftsPossiblyStale`. Las pistas no llevan rack (`WorkspaceSession.cs:55`), así que el paquete de T2B2 fija qué borradores se marcan y lo concilia con INV-28 (un Actualizar sobre A no marca el borrador de B); C-F4-04 se registra como IC o A-n según F4 v2 `:60`.
9. **Veto:** el manejador de `BeginDocumentClose` llama a `e.Veto()` cuando `DraftClosePolicy` decide Veto (M-6: una excepción da Veto).
10. **Sesión destruida:** una señal con su `SessionId` hacia `RetireSession` y el `DraftLossNotice`, sin texto de UI (el texto es de T3), **también cuando la sesión destruida no es la activa** (hoy solo se avisa si lo era; T2H-09, N-12 c).
11. **Pruebas:** obligación 4 (lector simulado: una intención = una lectura); INV-27 con lector simulado (sobre cambiado entre la preview y la intención → `Changed`, re-render, pulsación no aplicada); INV-22; obligación 3 extendida al punto de entrada del host; R6 y obligaciones 5 y 10 (ninguna escritura; la StaleBase solo detecta cambios; N-12 b); obligaciones 13, 14, 15 y 18.

## 7. Arrastres y residuales que fija este paquete

- **A T4A:** reutilizar `BuildStaleBase` en la relectura caller-owned y en O-04; los arrastres de F4-E25 siguen igual.
- **A T3:** capturar el testigo de la preview con el lector de T2B2 y la regla de origen de T2B2; mostrar `ClassicEditorHint` y `Detail` como información secundaria (INV-31).
- **A F5:** Cama con un solo miembro mutable (D-11); el registro de variables con su estado de payload para Selectivo.
- **Residuales aceptados (N-6), conservadores, sin admisiones falsas:**
  - un sobre legible que el lector estricto de AUTH-13 rechaza (Id solo distinto en mayúsculas, miembros repetidos) se muestra con el texto de lo ilegible, con el motivo real en `Detail`;
  - un Kind `PushBack` con otras mayúsculas sale «fuera del piloto» con el Kind como detalle.
  Smoke-2 mide si aparecen en los dibujos del Owner.

## 8. Paradas del paquete

- La proyección aditiva exige cambiar a un consumidor o una guarda que cuenta llamadas (T2B2).
- El lector exige tocar `Drafts/` o el piloto cerrado.
- La admisión exige una semántica que D-11 no fija.

## 9. Disposición de la revisión adversarial de la v1

| Hallazgo | Disposición |
|---|---|
| B-1 «Comprobar ahora» sin base | D-T2B-02 (base total), D-T2B-04 (`StaleBase` nunca null), INV-F4T2B1-11 y la prueba de Conflict y Recovered; el cableado, en T2B2 punto 6 |
| I-1 diagnóstico único de INV-17 | `UnreadableDiagnostic` único (reglas 3, 7, 11 y 13) con el detalle aparte; la regla 3 con definición operativa |
| I-2 INV-16 inalcanzable y `Classify` que lanza | entrada con hechos sin clasificar; RackId en blanco antes de `Classify`; «sin identidad» del origen antes de la pertenencia (regla 4); precondiciones estructurales separadas de los estados del dibujo |
| I-3 origen sin fijar | T2B2 punto 3: `NavigationTargets` revalidados, `Primary` y la conversión con `Handle` |
| I-4 registro de variables | `VariableRegistryText` null; el adaptador no lo lee |
| I-5 transacción, INV-14 y `LockViolation` | T2B2 punto 2 |
| I-6 INV-27 sin fuente | D-T2B-05: testigo (clave, texto crudo), estado de tres valores, lector en T2B2, captura en T3, prueba con lector simulado en T2B2 |
| I-7 obligaciones de T2B2 | §6, puntos 4, 5 y 11 |
| I-8 falta el contrato y el catálogo | §5 completo; D-T2B-07 |
| M-1 diagnósticos de Kind | regla 8 antes que la 9; prefijo común «No editable aquí: » |
| M-2 Cama | regla 9 y §7 (residual de F5) |
| M-3 presupuesto | §3: se registra en F4-E26 y se avisa al Owner |
| M-4 entradas sin definir | D-T2B-06 |
| M-5 citas | §2 corregido y la declaración de D-T2B-06 |
| M-6 adaptador sin cablear | el adaptador pasa a T2B2 |
| M-7 mayúsculas del Id | D-T2B-06, su prueba y el residual del §7 |
| M-8 textos de otros racks | recorte de D-T2B-02 y su prueba |

## 10. Puntos que se pidieron a la re-revisión de la v2

1. ¿Cierra la v2 el bloqueante y los 8 importantes, sin abrir uno nuevo?
2. ¿Es correcto y completo el orden de las reglas frente a D-11, INV-16 e INV-17?
3. ¿Hay algún estado del dibujo que todavía lance o que llegue a una admisión falsa?
4. ¿Es emitible el contrato del §5 tal cual?

## 11. Re-revisión de la v2 y disposición

**Veredicto:** EMITIBLE TRAS CORRECCIONES. Cierra B-1 y los ocho importantes (I-8 parcial por N-2); no abre ningún bloqueante ni importante; abre 12 menores sin cambio de semántica. Comprobó B-H01 a B-H12 línea a línea; B-H13 no se puede verificar sin red. Respuestas al §10: (1) sí; (2) el orden es correcto y las reglas 4-6 antes de la 7 se sostienen; (3) con hechos del barrido ningún estado da una admisión falsa, y solo `Deserialize` podía lanzar (N-5); (4) emitible tras N-1, N-2 y N-3.

| Hallazgo | Disposición en esta versión |
|---|---|
| N-1 texto de «fuera del piloto» | el texto de IC-07; todas las constantes siguen en minúscula tras los dos puntos |
| N-2 pruebas y guardas fuera del contrato | D-T2B-08 en el objetivo; INV-F4T2B1-13 con las cuatro guardas, la de M-9 sobre los tres archivos y la prueba de inyección |
| N-3 prueba de orden imposible | lista cerrada de pares exigidos y de pares excluyentes |
| N-4 fuente de las reglas 3-6 | las reglas leen el hecho y su clasificación; origen Redraw o Erase; precondiciones `IsSelectedSource` false y evidencia no vacía; AUTH-13 con el RackId de la entrada |
| N-5 lectura no total | deserialización local que trata cualquier excepción como «no se deserializa», con prueba de surrogate suelto |
| N-6 diagnósticos engañosos | excepciones del comparador y del materializador unificadas; hermanos ordenados por clave; residuales en el §7 |
| N-7 inmutabilidad | el constructor copia las colecciones con un diccionario ordinal |
| N-8 testigo | nombre del tipo; texto null → `Changed`; clave vacía, precondición; prueba con `ReferenceEquals` |
| N-9 RED por excepción | esqueleto con valores inertes no nulos |
| N-10 referencias | corregidas en D-T2B-02, D-T2B-03 y D-T2B-06, y en la consecuencia de B-H07 |
| N-11 desviación de la letra | T2B2 punto 1 y F4-E26 |
| N-12 obligaciones de T2B2 | §6, puntos 2, 3, 8, 10 y 11 |

No se pide una tercera ronda: las correcciones son menores, no cambian la semántica y la re-revisión las declaró suficientes para emitir. Lo decide la sesión bajo la delegación nocturna (F4-E26).

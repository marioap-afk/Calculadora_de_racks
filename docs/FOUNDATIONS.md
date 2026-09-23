# FOUNDATIONS — registro descriptivo de fundaciones

## Estado normativo

Este registro es metadata **descriptiva** materializada por I-56. No constituye politica efectiva de
Workflow V2 mientras no exista `WORKFLOW_V2_EFFECTIVE_SHA`. Nunca prevalece sobre los hechos del codigo
ni sobre un ADR aceptado o un Freeze integrado y sus enmiendas aplicables.

Quien consuma o extienda una entrada debe volver a verificarla en su base actual mediante
[DC-08](INITIATIVE_LIFECYCLE.md#4-discovery-core-y-expansiones). Todo desacuerdo activa
[EXP-01](INITIATIVE_LIFECYCLE.md#4-discovery-core-y-expansiones); el tratamiento completo de consumo,
discrepancias y cambios materiales vive en
[INITIATIVE_LIFECYCLE](INITIATIVE_LIFECYCLE.md#41-interaccion-con-foundations).

## Esquema de entrada

Cada entrada usa: `Name`, `Status`, `Authority`, `Persistence`, `Mutation contract`, `Extension point`,
`Decision source`, `Protecting tests`, `Known limitations` y `Last changed by`. `Status` es `STABLE` o
`SUPERSEDED -> <entry>`.

Una entrada solo puede ser `STABLE` cuando su fuente es un ADR aceptado o un Freeze integrado; el codigo
relevante existe; hay pruebas o guardas identificables; autoridad, persistencia, mutacion y extension
coinciden con el codigo actual; no queda una discrepancia de clase A abierta; y existe un punto de
extension declarado o un consumidor ajeno a la iniciativa que la introdujo.

## Entradas

Name: Rack Identity
Status: STABLE
Authority: `RackEmbedDocument.Id` es el GUID del rack logico; `Name`, nombre de bloque y `ObjectId` no lo sustituyen.
Persistence: `RackEmbedStore` serializa el sobre y `RackBlockData` lo guarda en el Xrecord de cada definicion; las vistas hermanas comparten `Id`.
Mutation contract: actualizar y agregar vistas conserva `Id`; crear o duplicar un rack independiente asigna uno nuevo.
Extension point: toda operacion que cree o relacione representaciones usa `RackEmbedComposer` y conserva o renueva `Id` segun la identidad resultante.
Decision source: [ADR-0009](adr/0009-identidad-guid-embebida-en-dwg.md).
Protecting tests: `RackEmbedDocumentTests`, `RackListBuilderTests`, `RackDuplicationPlanTests`, `SelectiveDuplicationFailClosedTests`.
Known limitations: un sobre legacy sin `Id` no permite inferir hermanas por nombre, geometria ni contenido.
Last changed by: I-51

Name: View Identity
Status: STABLE
Authority: una representacion se distingue por la definicion de bloque y su sobre `(Id, View, Section)`; la referencia solo la coloca.
Persistence: `RackEmbedDocument.View` y `Section` viajan con cada definicion junto al `Id` compartido y el diseño requerido para reabrirla.
Mutation contract: Actualizar redefine representaciones existentes; Insertar agrega una representacion ligada con el mismo `Id` y una vista/seccion propia.
Extension point: cada editor declara sus vistas admitidas y materializa una vista nueva desde un rack existente mediante el flujo de composicion y colocacion.
Decision source: [ADR-0010](adr/0010-actualizar-redibuja-insertar-liga-vistas.md), con identidad de [ADR-0009](adr/0009-identidad-guid-embebida-en-dwg.md).
Protecting tests: `RackEmbedDocumentTests`, `PersistenceReopenPreservationTests`, `SelectiveAuthoredBindingTests`, `DimensionViewsRestampTests`.
Known limitations: las capacidades multivista y el significado de `Section` dependen del sistema; varias referencias pueden compartir una definicion.
Last changed by: I-07

Name: RACKDUPLICAR / Restamp Identity
Status: STABLE
Authority: `RackDuplicationPlan` decide grupos e identidades nuevas; `RackEnvelopeRestamp.RestampEnvelope` aplica el mismo GUID al sobre y al diseño interior.
Persistence: el restamp conserva `View`, `Section`, `SchemaVersion`, `ExtensionData`, authored y bindings de cada sobre y cambia su `Id` y nombre logico.
Mutation contract: preflight y restamps se completan antes de escribir; cada destino usa una transaccion y falla cerrado sin identidad parcial.
Extension point: un nuevo kind implementa su restamp en `IRackKindHandler`; los comandos consumen el registro y el plan sin ramas paralelas por kind.
Decision source: Freeze integrado [I-51](initiatives/I-51-rackduplicar-multiples-origenes.md), apoyado por [ADR-0009](adr/0009-identidad-guid-embebida-en-dwg.md).
Protecting tests: `RackDuplicationPlanTests`, `SelectiveDuplicationFailClosedTests`, `DimensionViewsRestampTests`, `CustomPropertiesEnvelopeTests`.
Known limitations: selecciona solo referencias fisicas de Model Space; la atomicidad entre destinos es por destino, no por el comando completo.
Last changed by: I-51

Name: Project Variables
Status: STABLE
Authority: un `ProjectVariablesDocument` por DWG es la autoridad; `VariableId` identifica y `Name` solo presenta.
Persistence: `ProjectVariablesData` guarda `RACKCAD_PROJECT` como Xrecord del NOD; `ProjectVariablesStore` gobierna schema, lectura y JSON con extension data.
Mutation contract: `ProjectVariablesWorkspace` y el preflight acreditan registro y consumidores antes de una escritura confirmada; estados desconocidos o ambiguos fallan cerrado.
Extension point: nuevos tipos extienden `VariableTypes` y nuevas propiedades usan `PropertyId` y el descriptor vinculable, sin resolver referencias fuera de Application.
Decision source: [ADR-0034](adr/0034-project-variables-autoridad-drawing-level.md), enmendado por Freeze integrado [I-48](initiatives/I-48-generic-linked-property-editing.md).
Protecting tests: `ProjectVariablesDocumentTests`, `ProjectVariablesWorkspaceTests`, `ProjectVariablesConformanceTests`, `RegistryCommitAccreditationTests`.
Known limitations: el registro no ofrece control general de concurrencia entre preflight y commit; una relectura debe acreditarse por si misma.
Last changed by: I-48

Name: Shared View Foundation — AUTH-13 Authored Comparators
Status: STABLE
Authority: `RackAuthoredComparatorPorts` conserva AUTH-13 neutral. Selective delega a `SelectiveAuthoredAuthority.Resolve`; Dynamic, PushBack, Cantilever y Cabecera acreditan todas las hermanas raw antes de comparar y materializar. Los outcomes son `Single`, `Divergent` y `Unreadable`; Cama y factories genericas arbitrarias permanecen unsupported/fail-closed.
Persistence: no cambia formatos, DTOs, serializers ni stores. `RackAuthoredInput` transporta pertenencia exterior, snapshot completo, source y raw original de envelope/Design; el reader local cierra forma, schema y unknowns antes de cualquier mapper. Selective conserva su contrato historico; los otros Singles publican `DynamicRackDesign`, `PushBackDesign`, `CantileverLineDesign` y `RackFrameConfiguration`.
Mutation contract: la igualdad es tipada y no llama resolver ni catalogo; cada Single nuevo es profundo e independiente. AUTH-09 opera sobre una copia de trabajo y llama una vez a la autoridad vigente; AUTH-10 prepara el effective tipado sin re-resolver, y el effective nunca sustituye authored.
Extension point: consumidores usan el puerto concreto por kind y las seams AUTH-09/10 existentes, acreditan captura completa y conservan metadata del envelope y sus gates propios. No existe registry o conversor universal.
Decision source: Consensus Freeze V3 [I-58](initiatives/I-58-freeze-draft.md), Characterization V3 y [ADR-0044](adr/0044-hechos-neutrales-de-vistas-compartidas.md), sobre AUTH-13 de I-57.
Protecting tests: `I58F1CharacterizationTests`, `I58F1OracleChecks`, `I58F2ReaderTests`, `I58F2BoundaryTests`, `I58F2MaterializationTests`, `I58F3SeamTests`, `I58Conf58SchemaWhitespaceTests`, `SharedViewFoundationF5Tests`, `SharedViewFoundationF6Tests` y `SelectiveAuthored*`.
Known limitations: con PostPeralte global 0 conserva y compara cada Header persistible completo por orden, modulo y provenance, lo que puede producir Divergent conservador aunque el peralte efectivo coincida. CustomProperties mantiene autoridad separada. En Cantilever, membership exterior y `Line.Id` interior son distintos y no se corrige D58-CANT-ID/I-37. Cama, AUTH-15, captura AutoCAD, remedios de divergence y consumo I-55 G12 quedan fuera; G12 sigue bloqueado hasta integracion completa y tag `integration/I-58` valido.
Last changed by: I-58

Name: Authored vs Effective
Status: STABLE
Authority: `SelectiveAuthoredAuthority` obtiene un authored logico por RackId; `SelectiveEffectiveDesignResolver` es el punto unico de authored + variables a effective.
Persistence: `SelectivePalletDesignDocument` conserva literales authored y `PropertyValues`; el diseño effective no reemplaza esos valores persistidos.
Mutation contract: hermanas ilegibles o divergentes y bindings rotos fallan cerrado; dibujo, preview y BOM reciben el effective resuelto.
Extension point: una propiedad nueva se registra mediante un descriptor y `PropertyId`; ningun consumidor calcula su effective por su cuenta.
Decision source: [ADR-0034](adr/0034-project-variables-autoridad-drawing-level.md), enmendado por Freeze integrado [I-48](initiatives/I-48-generic-linked-property-editing.md).
Protecting tests: `SelectiveAuthoredBindingTests`, `SelectiveEffectiveDesignResolverTests`, `SelectiveBomAuthorityTests`, `LinkedPropertyKernelTests`.
Known limitations: la autoridad authored estricta entre vistas esta implementada para Selectivo; no es una garantia general de todos los sistemas.
Last changed by: I-48

Name: Custom Properties
Status: STABLE
Authority: `CustomPropertiesDocument` identifica entradas por GUID; `RackCustomPropertiesAuthority` exige una coleccion coherente entre vistas hermanas.
Persistence: proyecto usa el Xrecord NOD `RACKCAD_CUSTOM_PROPERTIES`; rack usa JSON crudo en `RackEmbedDocument.CustomProperties`; ambos documentos versionan y preservan extension data.
Mutation contract: `CustomPropertiesMutations` produce documentos nuevos y `CustomPropertiesExecutor` preflights autoridad/estado fisico antes de confirmar escrituras.
Extension point: agregar metadata consume `CustomPropertiesWorkspace` y las mutaciones por `CustomPropertyId`; el sobre sigue sin interpretar el contenido.
Decision source: [ADR-0039](adr/0039-custom-properties-persistencia-autoridad.md).
Protecting tests: `CustomPropertiesStoreTests`, `CustomPropertiesMutationTests`, `CustomPropertiesAuthorityTests`, `CustomPropertiesCommitTests`, `CustomPropertiesEnvelopeTests`.
Known limitations: V1 admite solo valores string literales; identidad y nombres son locales a cada coleccion y no enlazan proyecto con rack.
Last changed by: I-54

Name: DimensionViews
Status: STABLE
Authority: `DimensionViewPolicy.EffectiveDetail` decide en Application si el detalle global aplica a Frontal, Lateral o Planta.
Persistence: `int? DimensionViews` vive en el diseño Selectivo, Dinamico y Push Back; ausencia/null conserva el comportamiento legacy y cualquier `int` presente vuelve exacto.
Mutation contract: editores, copias y restamps trasladan la politica; `Dimensions = None` siempre produce cero cotas y bits ajenos se preservan.
Extension point: un emisor de cotas consulta la regla pura con su `DimensionViewKind`; un tipo de vista nuevo debe definir su fallback legacy.
Decision source: [ADR-0035](adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md).
Protecting tests: `DimensionViewPolicyTests`, `DimensionViewsLegacyJsonTests`, `DimensionViewsCopySitesTests`, `DimensionViewsPhysicalAndBomInvarianceTests`, `DimensionViewsRestampTests`.
Known limitations: no hay control por copia o seccion; builds anteriores pueden volver a mostrar cotas y algunos pierden el campo al re-guardar.
Last changed by: I-50

Name: Header Mutation / Reconciliation
Status: STABLE
Authority: `HeaderConfigurationSnapshot` captura authored; los planners Selectivo/Dinamico aplican taxonomias propias sobre `HeaderBatchPlan` y `HeaderBatchOutcome`.
Persistence: la configuracion copiada queda en los diseños de cada sistema; `RackModuleReconciliation` reconcilia overrides dinamicos por `ModuleId + Kind` tras reconstruir.
Mutation contract: preparar resuelve, normaliza y valida sin escribir; una firma stale rechaza antes de la primera asignacion y solo `Committed` permite un recompute.
Extension point: cada sistema aporta direcciones, planner, normalizacion y mutador propios; comparte snapshot, plan, outcome y codigos sin crear un ejecutor universal.
Decision source: [ADR-0037](adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md).
Protecting tests: `HeaderConfigurationSnapshotTests`, `HeaderBatchContractTests`, `SelectiveHeaderBatchPrepareTests`, `SelectiveHeaderBatchMutateTests`, `DynamicHeaderBatchPrepareTests`, `DynamicHeaderBatchReconciliationTests`.
Known limitations: Push Back conserva el contrato anterior de I-40 y no consume el lote comun; la reconciliacion dinamica depende de identificadores posicionales.
Last changed by: I-53

Name: Unknown-field Preservation in Persisted Envelopes
Status: STABLE
Authority: cada DTO participante conserva sus miembros JSON desconocidos mediante `[JsonExtensionData]`; el store o composer propietario decide su acarreo.
Persistence: `ExtensionData` existe en `RackEmbedDocument`, `RackProjectDocument`, `FlowBedDocument` y `LargueroDocument`, y en documentos posteriores que declaran el mismo contrato.
Mutation contract: rutas de re-guardado y `RackEmbedComposer` parten del documento fuente, conservan campos desconocidos y rechazan colisiones con miembros declarados.
Extension point: un DTO persistido nuevo declara schema, fallback legacy, extension data y pruebas de round-trip; toda reconstruccion acepta el documento fuente.
Decision source: Freeze integrado [I-11](initiatives/I-11-persistencia-uniforme.md), con reglas del miembro tipado de [ADR-0039](adr/0039-custom-properties-persistencia-autoridad.md).
Protecting tests: `PersistenceUniformityTests`, `PersistenceReopenPreservationTests`, `RackEmbedDocumentTests`, `CustomPropertiesEnvelopeTests`.
Known limitations: solo se preservan limites que declaran y acarrean extension data; un miembro conocido conserva su validacion tipada y no se degrada a metadata desconocida.
Last changed by: I-54

Name: Linked Properties
Status: STABLE
Authority: `SelectiveLinkedProperties` registra descriptores por `PropertyId`; `SelectiveLinkedPropertyKernel` comparte inspeccion, lookup y resolucion.
Persistence: `SelectivePalletDesignDocument.PropertyValues` conserva por propiedad el literal authored y la referencia a `VariableId`, incluida extension data por valor.
Mutation contract: `LinkedPropertyEditSession` escenifica la edicion y `LinkedPropertyReconciler` aplica todos los estados sobre una copia y devuelve authored + effective o fallo sin resultado parcial.
Extension point: una propiedad Selectiva nueva agrega un `PropertyId` y un descriptor con lectura/escritura authored; UI y mutaciones consumen el mecanismo generico.
Decision source: Freeze integrado [I-48](initiatives/I-48-generic-linked-property-editing.md), sobre [ADR-0034](adr/0034-project-variables-autoridad-drawing-level.md).
Protecting tests: `LinkedPropertyFoundationTests`, `LinkedPropertyKernelTests`, `LinkedPropertyEditSessionTests`, `LinkedPropertyReconcilerTests`, `MultiPropertyRealProofTests`.
Known limitations: el catalogo actual contiene propiedades Selectivas; la concurrencia general entre preflight y commit queda fuera del contrato.
Last changed by: I-48

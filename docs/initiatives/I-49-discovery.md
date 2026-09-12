# I-49 — Discovery (G1): estado real para un motor de expresiones paramétricas

> **Fase G1 — DISCOVERY read-only.** Este documento es el único artefacto de G1. No contiene ni autoriza
> producción, parser, AST, evaluador, grafo de dependencias, schema nuevo ni UI productiva. **No es una
> Proposal**: rige la compuerta del contrato §12 (**NO IMPLEMENTATION BEFORE CONSENSUS**).

## Convención de clases

Cada afirmación lleva una clase. Las rutas son relativas a la raíz del repositorio y las líneas se refieren
al código auditado (§1).

| Clase | Significado |
|---|---|
| **FACT** | Demostrado por código, tests o documentación vigentes en el SHA auditado, con `ruta:línea` |
| **INFERENCE** | Deducción razonable a partir de FACTs. Puede ser falsa: se verifica antes de apoyarse en ella |
| **PROPOSAL INPUT** | Cuestión que el Discovery **no** decide. La deciden Coordinator + Architect, y el Owner cuando corresponda |
| **OWNER INPUT** | Aclaración o requisito recibido del Owner. **No** es un hecho descubierto en el código |

Un FACT marcado «por código» describe lo que el código hace sin que haya un test que lo fije.

---

## 1. Identificación / SHA auditado

```text
Iniciativa    = I-49 — Motor de expresiones paramétricas (Expression Engine, ID22B)
Rama          = architecture/motor-expresiones-parametricas
Worktree      = ~/.codex/worktrees/architecture-motor-expresiones-parametricas
HEAD auditado = f2d28a2097cd2cff47aa9878908be792833abd36   (bootstrap G0; = origin/<rama>)
Código        = a4d88f18a1f42263d366c44dc05dd18a6786f152   (origin/main; el bootstrap solo añadió docs/)
origin/main   = a4d88f18a1f42263d366c44dc05dd18a6786f152   (no avanzó desde G0)
I-50          = origin/feature/cotas-independientes-por-vista     @ 0d7670c   (solo docs/; G2A, ADR-0035 propuesto)
I-51          = origin/feature/rackduplicar-multiples-origenes    @ c4cc2e4   (solo docs/; G2)
Fecha         = 2026-09-12
```

- **FACT** — Preflight de sesión: rama `architecture/motor-expresiones-parametricas`, upstream
  `origin/architecture/motor-expresiones-parametricas`, `+0/-0`, árbol limpio, sin stash ni operaciones Git a
  medias; `HEAD...origin/main` = `2/0`, así que no hace falta rebase. Bootstrap presente:
  `docs/initiatives/I-49-motor-expresiones-parametricas.md`, `docs/automation/decisions/I-49.md` y la fila
  de I-49 en `docs/ROADMAP.md`.
- **FACT** — `git diff --name-status origin/main...<rama>` de I-50 e I-51 solo lista archivos de `docs/`, al
  empezar la sesión y otra vez justo antes del commit de G1: **no hay colisión productiva visible** (§19).
- **FACT** — Se aplica `OWNER_OVERRIDE_I49_I50_PARALLEL` (`docs/automation/decisions/I-49.md`); este G1 no
  toca ningún archivo productivo, así que su condición de colisión no se activa.

**Método.** Lectura directa, completa, de los archivos núcleo de Project Variables, persistencia, mutación,
Plugin, editor vinculable y RACKVARIABLES; tres barridos de exploración de solo lectura (parsing y unidades,
inventario de tests, búsquedas negativas y nombres), con sus afirmaciones clave verificadas por muestreo
sobre el código. **No se ejecutaron suites**: el Discovery no es un gate de Candidato.

**Entradas vinculantes del Owner recibidas para G1 (OWNER INPUT, no descubiertas):**

- **ID20 futuro — Computed / Built-in Parameters** (`Rack.Frentes`, `Rack.Niveles`, `Rack.Altura`,
  `Project.TotalRacks`, `Project.TotalFrentes`). I-49 **no** los implementa; solo debe **preparar**
  `SymbolId`, scopes/namespaces, `ExpressionContext`, resolución de símbolos y los datos que necesita el
  evaluador.
- **ID23 futuro — Custom / Calculated BOM** (ejemplo conceptual `Guías.Quantity = Rack.Frentes * 2`). I-49
  **no** implementa BOM virtual; debe permitir que ID23 reutilice parser/evaluador/contexto **sin que estos
  dependan de ProjectVariables**.
- **ID28 — Explain / Trace** e **ID29 — Impact Preview**: I-49 no implementa sus superficies, pero este
  Discovery debe auditar qué información habría que preservar para no rehacer el motor.

---

## 2. Executive summary factual

### 2.1 Hechos principales

1. **FACT** — La autoridad semántica de una variable es su **`VariableId`** (GUID), dentro de **un único**
   `ProjectVariablesDocument` por DWG, guardado en el NOD bajo `RACKCAD_PROJECT`
   (`src/RackCad.Plugin/ProjectVariablesData.cs:36`). La **legibilidad** la decide `ProjectVariablesStore`
   (`src/RackCad.Application/Persistence/ProjectVariablesStore.cs:101-208`); la **unicidad de identidad** la
   acredita `UsableProjectVariablesRegistry.Accredit`
   (`src/RackCad.Application/ProjectVariables/UsableProjectVariablesRegistry.cs:120-182`). `Name` no participa
   en ninguna resolución.
2. **FACT** — `VariableDefinition` tiene **un solo caso** (`VariableDefinitionKind.Literal = 1`,
   `VariableDefinition.cs:6-10`), cuyo payload es un `double` privado (`:28`); `LiteralValue` **lanza** fuera de
   `Literal` (`:39-51`). En disco la definición es `{ "Kind": "literal", "Value": <double> }` y el store
   **rechaza** cualquier otro `Kind` como `PresentButUnreadable` (`ProjectVariablesStore.cs:193-197`), algo que un
   test fija **con el valor `"expression"`** (`tests/RackCad.Tests/ProjectVariablesDocumentTests.cs:152-160`).
3. **FACT** — El valor efectivo lo produce **un único** resolver
   (`SelectiveEffectiveDesignResolver.ResolveWith`, `src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs:110-164`),
   que escribe `inspection.Target.LiteralValue` (`:160`). El target acreditado
   (`VariableTargetSnapshot`, `VariableTargetSnapshot.cs:29-49`) es **literal por construcción**: no existe
   ningún paso de evaluación entre el registro y la geometría.
4. **FACT** — Hay **al menos 15 ubicaciones de producción** que asumen que toda definición es literal (§5.5,
   filas 1-15), empezando por la acreditación (`UsableProjectVariablesRegistry.cs:150,162`) y la escritura
   del registro (`MutationPlan.cs:96-106,119-131`).
5. **FACT** — La propagación es de **profundidad 1**: `ChangeValue` descubre los consumidores
   **propiedad→variable** de UNA variable, resuelve cada rack contra el registro «después», y emite **una**
   `RegistryMutation` + N `RackMutation` (`ProjectVariableMutationPreflight.cs:121-167`). El ejecutor escribe
   todo en **una** transacción, un `Commit` y **un** `Regen`
   (`src/RackCad.Plugin/ProjectVariableMutationExecutor.cs:250-293`).
6. **FACT** — El descubrimiento de consumidores es un **barrido del dibujo** (Plugin) proyectado a datos
   puros; considera **solo racks Selectivos** y **aborta** ante cualquier cosa indeterminada
   (`ProjectVariableConsumerDiscovery.cs:100-155,228-259`). No existe índice inverso persistido ni **ninguna**
   relación variable→variable.
7. **FACT** — En el editor vinculable, **todo texto que empieza por `=`** es una *consulta de referencia* que
   solo filtra por nombre (`LinkedPropertyEditSession.cs:129-131,183-203,431-432`); una referencia solo se
   compromete por **selección explícita de un `VariableId`** (`:307-329`), y el protocolo C4 **bloquea** un
   borrador que cambie la fuente (`:335-357`).
8. **FACT** — Los **nombres duplicados son legales** y **ninguna ruta de producción** resuelve nombre→id
   (§13). El desambiguador visible son los **8 primeros caracteres del `VariableId`**, y solo aparece en la
   lista de candidatos (`LinkedPropertyOptions.cs:52-65`).
9. **FACT** — Hay **dos gramáticas numéricas** en superficies de Project Variables: el editor vinculable
   parsea en cultura **invariante** (`LinkedPropertyEditSession.cs:439-443`) y RACKVARIABLES usa
   `UiSupport.TryNum` → `LocalizedNumberParser.TryDouble`, que acepta cultura actual, invariante o una sola coma
   decimal (`src/RackCad.UI/UiSupport.cs:97-98`; `src/RackCad.Application/Formatting/LocalizedNumberParser.cs:19-54`).
   La presentación redondea con `"0.###"` (§11).
10. **FACT** — Hay **guardas de fuente vigentes** que un motor de expresiones rompería tal como están:
    `ProjectVariablesConformanceTests.cs:125-137` prohíbe los tokens `ExpressionParser`, `FormulaParser` y
    `DependencyGraph` en Application/Plugin/UI, y `:141-149` exige que `VariableDefinition.cs` **no** contenga
    `"Expression = "`.
11. **FACT** — Todo el camino de vínculo y resolución está **acoplado al Selectivo**: descriptores sobre
    `SelectivePalletDesignDocument`/`SelectivePalletDesign` con accesores `double`
    (`SelectiveLinkedProperties.cs:23-69`), consumidores solo Selectivos, y `RackMutation` con tipos Selectivos
    (`MutationPlan.cs:148-175`).
12. **FACT** — `PropertyValue<T>` **no tiene ningún consumidor de producción** (solo su propio archivo): la
    fuente real de un vínculo viaja como `SelectivePropertyValueDocument` (persistencia) y como
    `LinkedPropertySource` (editor). Coexisten tres representaciones de la fuente de un valor de propiedad (§4.1).
13. **FACT** — Las búsquedas negativas no encuentran parser, AST, evaluador, grafo de dependencias, detección
    de ciclos genérica, registro de funciones, `SymbolId`, `ExpressionContext` ni `SymbolResolver` (§15).
14. **FACT** — Con I-50 e I-51 no hay colisión productiva real: ambas ramas solo tocan `docs/`. Hay conflictos
    **documentales** previstos en `docs/ROADMAP.md` y `docs/ideas-futuras.md`, y en la numeración de ADR (I-50 ya
    ocupa ADR-0035). El ADR-0035 propuesto de I-50 anticipa cambios en `SelectivePalletDesignDocument` y en los
    editores (§19).

### 2.2 Respuestas a las 20 preguntas obligatorias

| # | Pregunta | Respuesta con evidencia | Clase | Detalle |
|---|---|---|---|---|
| 1 | ¿Autoridad semántica de una variable? | Su `VariableId`, dentro del único `ProjectVariablesDocument` del DWG. La legibilidad la decide `ProjectVariablesStore`; la unicidad de identidad, `UsableProjectVariablesRegistry.Accredit`. `Name` no participa en ninguna resolución (`VariableId.cs:5-14`; `ProjectVariablesStore.cs:101-208`; `UsableProjectVariablesRegistry.cs:120-182`) | FACT | §4.1 |
| 2 | ¿Qué extensión dejó `VariableDefinition`? | Un discriminador (`VariableDefinitionKind`, un solo miembro) y una clase con igualdad por valor, pero **sin payload** para otra cosa que un `double`: constructor privado `(kind, double)` y `LiteralValue` que lanza fuera de `Literal`. Una guarda prohíbe añadir `Expression = ` al archivo (`VariableDefinition.cs:6-75`; `ProjectVariablesConformanceTests.cs:141-149`) | FACT | §4.3 |
| 3 | ¿Qué extensión dejó la persistencia? | `Kind` string, `Value double?` y `ExtensionData` en raíz, entrada y definición, con minor preservado al re-guardar. Pero el store rechaza todo `Kind` ≠ `literal` —test con `"expression"`— y exige `Value` finito (`ProjectVariablesDocument.cs:57-58,131-148`; `ProjectVariablesStore.cs:193-204`; `ProjectVariablesDocumentTests.cs:152-160,209-233`) | FACT | §5.3–§5.5 |
| 4 | ¿Algún consumidor de `VariableDefinition` asume `Literal`? | **Sí**, al menos 15 ubicaciones de producción (§5.5, filas 1-15). Las decisivas: la acreditación exige `Definition.Value` (`UsableProjectVariablesRegistry.cs:150,162`), el resolver escribe `Target.LiteralValue` (`SelectiveEffectiveDesignResolver.cs:160`), `ApplyTo` escribe `"literal"` (`MutationPlan.cs:96-106,119-131`) y `Rename` valida reconstruyendo un literal (`ProjectVariableMutationPreflight.cs:97-98`) | FACT | §5.5 |
| 5 | ¿Rename puede seguir siendo registry-only con expresiones ligadas por identidad? | Hoy lo es: plan sin racks ni redibujo (`ProjectVariableMutationPreflight.cs:76-107`). Seguiría siéndolo si las expresiones persisten `VariableId`; dejaría de serlo si persisten nombres | FACT (hoy) / INFERENCE / PROPOSAL INPUT | §8.2 |
| 6 | ¿Qué necesita Delete para ver dependientes variable→variable? | Hoy solo bloquea por consumidores propiedad→variable (`ProjectVariableMutationPreflight.cs:188-202`). Necesitaría ver qué definiciones del **registro** refieren el id, sin barrer el dibujo, y decidir si bloquea o hace cascada | FACT / INFERENCE / PROPOSAL INPUT | §6.3, §8.2 |
| 7 | ¿Cómo se descubre hoy un consumidor propiedad→variable? | Barrido del Plugin → `ProjectVariableScanProjection.Project` → agrupación de Selectivos por rack → sonda tri-estado sobre `PropertyValues` → autoridad authored única → consumidor (`ProjectVariableConsumerDiscovery.cs:100-155`; `ProjectVariableConsumerProbe.cs:52-93`) | FACT | §7 |
| 8 | ¿Frontera correcta para dependencias variable→variable? | ADR-0034 §15 separa `Definition` (variable) de `PropertyValue<T>` (propiedad). La relación propiedad→variable vive en el authored del rack; una relación variable→variable viviría en el registro. Mantenerlas separadas y unirlas solo al componer el plan es una deducción, no una decisión | FACT (doc + código) / INFERENCE / PROPOSAL INPUT | §6.3 |
| 9 | ¿Unidad numérica interna real? | **Pulgadas**, como `double` sin unidad: ADR-0005 aceptado (`docs/adr/0005-estrategia-de-unidades.md:31-33`), `VariableType.Length` «in inches … introducing no conversion» (`VariableType.cs:4-5,21`) y la etiqueta «Valor (in)» (`RackProjectVariablesWindow.xaml:68`). El registro no persiste unidad | FACT | §12.2 |
| 10 | ¿Qué parsing/formatting debe preservarse? | ADR-0015 (aceptado): regla localizada única para la entrada humana, fijada por tests. Editor vinculable: invariante + finito, sin rango, con los pines de comportamiento de `=`. Presentación `"0.###"` invariante. JSON por defecto. Igualdad exacta. **Divergencia vigente sin decisión documentada**: el editor vinculable no usa la regla de ADR-0015 | FACT | §11.8 |
| 11 | ¿Qué parte de `LinkedPropertyEditSession` asume que todo `=` es consulta de referencia? | `IsQuery`, `Draft`, `Candidates`, `DraftChangesSource`, `TryCommitByEnter`, `TryStage` y `DisplayOf`, más `RefreshCandidates` del control y `Describe` de la ventana (tabla §9.6) | FACT | §9.6 |
| 12 | ¿Seam mínimo para una futura fuente `Expression`? | `LinkedPropertySourceKind`/`LinkedPropertySource` + `LinkedPropertyEditState` + la clasificación `Draft`/`IsQuery` + `LinkedPropertyReconciler.Apply` + `SelectivePropertyValueDocument.Kind` + inspección, sonda y resolver. Cuál se usa depende de dónde viva la expresión | FACT (ubicaciones) / INFERENCE / PROPOSAL INPUT | §9.7 |
| 13 | ¿Cómo se representan y presentan hoy los nombres duplicados? | Son legales. La lista de candidatos añade los 8 primeros caracteres del id; el campo (`=Nombre`), el estado bajo el campo y la lista de RACKVARIABLES **no** muestran id | FACT | §13 |
| 14 | ¿Alguna ruta productiva resuelve nombre→id? | **No.** El editor selecciona por `VariableId` (`LinkedPropertyEditor.cs:357-368`; `LinkedPropertyEditSession.cs:307-329`), Enter sobre una consulta se rechaza (`:269-271`), RACKVARIABLES y reparación direccionan por id y rack, y el reconciliador escribe `Source.VariableId`. Una guarda lo protege en el control (`SelectiveBindingIntentTests.cs:393-409`); en el resto lo establece el barrido de §13.3 | FACT | §13.3 |
| 15 | ¿Qué parte del `MutationPlan` impide o facilita la propagación transitiva? | **Facilita**: N `RackMutation` con efectivo completo, un commit y un regen, todo o nada, re-acreditación en el commit. **Limita**: una sola `RegistryMutation`, descubrimiento de UNA variable, targets literales, y `MutationDestinationBinding` aborta si una definición se nombra dos veces (`MutationPlan.cs:148-212`; `MutationDestinationBinding.cs:85-91`) | FACT / INFERENCE | §6.2–§6.3 |
| 16 | ¿Cuántas veces podría regenerarse un rack en una cadena hipotética? | Hoy una ejecución hace **un** `Regen` y un rack aparece una vez por plan (`ProjectVariableMutationExecutor.cs:293`; `ProjectVariableMutationPreflight.cs:151-166`). Una cadena planificada como un único plan deduplicado daría un regen; ejecutada como K planes sucesivos, K regens, y un rack común a varios eslabones se redibujaría K veces. Cada ejecución barre además el dibujo una vez por rack (`RackCommandSupport.cs:109-134`) | FACT (hoy) / INFERENCE (cadena) | §6.1 |
| 17 | ¿Qué información actual serviría a ID28 Explain? | `BindingInspection`, `SelectiveEffectiveResolution`, `VariableConsumerSummary` y el texto de estado del editor. La procedencia se pierde en cuanto el resolver escribe el número en Domain | FACT | §16.4 |
| 18 | ¿Qué información inversa existe para ID29 Impact? | `DiscoverConsumers`, `Summarize`, `PropertiesBoundTo`, el `MutationPlan` previo a ejecutar y `MutationExecutionResult`. Nada persistido, y ninguna arista variable→variable | FACT | §7, §16.4 |
| 19 | ¿Qué acoplamiento impediría registrar `Rack.*` / `Project.*`? | (a) El registro acreditado se indexa solo por `VariableId`, sin namespace (`UsableProjectVariablesRegistry.cs:97`). (b) Resolución, descriptores y consumidores atados al Selectivo. (c) Target con forma de literal. (d) Los datos por rack requieren diseño + catálogo (`SelectiveGeometryResolver.cs:33`) y difieren por sistema; los de proyecto solo existen tras un barrido del **Plugin**, y hoy no hay ningún agregado de frentes, niveles ni altura | FACT (a–d) / INFERENCE (que impidan) | §16.2 |
| 20 | ¿Qué decisiones no resuelve el Discovery? | Dónde vive la expresión, su representación persistida, la versión, dónde y cuándo se evalúa, grafo y ciclos, tipos y unidades, nombres, significado de `=`, fronteras de ID20/ID23, trazas de ID28/ID29, evolución de las guardas y ADR (23 preguntas en §18) | PROPOSAL INPUT | §18 |

---

## 3. Mapa de archivos y símbolos

Las visibilidades son las del código: `public`, `internal` (visible para `RackCad.Tests` por
`InternalsVisibleTo`) o `private`.

### 3.1 Application — `src/RackCad.Application/ProjectVariables/`

| Archivo | Símbolos principales | Visibilidad | Rol |
|---|---|---|---|
| `VariableId.cs` | `VariableId` (`New`, `TryParse`, `Parse`) | public | Identidad GUID de la variable |
| `VariableType.cs` | `VariableType { Length = 1 }`, `VariableTypes.IsSupported`, `VariableTypes.TryParseToken` | public | Tipo y mapping único del token persistido |
| `VariableDefinition.cs` | `VariableDefinitionKind { Literal = 1 }`, `VariableDefinition` (`Literal`, `LiteralValue`) | public | Definición del valor |
| `ProjectVariable.cs` | `ProjectVariable` (`Create`, `WithName`, `WithDefinition`) | public | Entidad pura |
| `PropertyId.cs` | `PropertyId` | public | Identidad persistida de una propiedad |
| `PropertyValue.cs` | `PropertyValueKind`, `PropertyValue<T>` | public | Modelo puro de valor de propiedad (sin consumidor de producción) |
| `ProjectPropertyIds.cs` | `SelectiveVerticalClearance(Token)`, `SelectivePalletTolerance(Token)`, `IsKnown` | public | Tokens de las propiedades vinculables |
| `SelectiveLinkedProperties.cs` | `SelectiveLinkedPropertyDescriptor`, `LinkedPropertyDescriptorSet`, `SelectiveLinkedProperties.All/IsKnown` | internal | Catálogo cerrado de propiedades vinculables |
| `SelectiveLinkedPropertyKernel.cs` | `RackRepairability`, `RackRepairabilityAssessment`, `SelectiveLinkedPropertyKernel.InspectBindings/Assess/PropertiesBoundTo` | internal | Kernel genérico por rack |
| `BindingInspection.cs` | `BindingInspectionOutcome`, `MalformedReferenceReason`, `BindingInspection`, `LinkedPropertyInspection.InspectBinding` | internal | Autoridad semántica de UN binding |
| `VariableTargetSnapshot.cs` | `VariableTargetSnapshot.TryCreate` | internal | Proyección acreditada de un target |
| `UsableProjectVariablesRegistry.cs` | `ProjectVariablesAccreditationOutcome`, `ProjectVariablesAccreditation`, `UsableProjectVariablesRegistry.Accredit/FromTargets/TryGetTarget/Targets` | internal | Acreditación de identidad |
| `RegistryCommit.cs` | `RegistryCommitOutcome`, `RegistryCommitPreparation`, `RegistryCommit.Prepare` | public | Re-lectura acreditada del commit |
| `MutationPlan.cs` | `RegistryMutationKind`, `RegistryMutation` (`ApplyTo` internal), `RackMutation`, `MutationPlan`, `VariableConsumerSummary`, `ProjectVariableCloning` | public / internal | Plan semántico |
| `VariableMutationPreflightResult.cs` | `VariableMutationOutcome`, `VariableMutationPreflightResult` | public | Resultado del preflight |
| `ProjectVariableMutationPreflight.cs` | `Create`, `Rename`, `ChangeValue`, `Delete`, `UnlinkAllAndDelete`, `Link`, `Unlink`, `RepairBrokenRack`, `Summarize` | public | Preflight semántico puro |
| `ProjectVariableIntent.cs` | `ProjectVariableIntentKind`, `ProjectVariableIntent`, `ProjectVariableIntentPreflight.Run` | public | Intents de RACKVARIABLES |
| `SelectiveBindingIntent.cs` | `SelectiveBindingIntent`, `ProjectVariableOption`, `SelectiveBindingOptions.ForLength`, `SelectiveBindingIntentPreflight.Run` | public | Intents de vínculo (camino heredado de I-47 G17) |
| `MutationDestinationBinding.cs` | `DestinationBindingOutcome`, `DestinationBindingResult`, `MutationDestinationBinding.Bind` | public | Vistas planeadas = vistas presentes |
| `ProjectVariableConsumerDiscovery.cs` | `ProjectVariableConsumer`, `ConsumerDiscoveryResult`, `DiscoverConsumers`, `ResolveTargetRack`, `GroupSelectiveByRack` | public / internal | Descubrimiento de consumidores |
| `ProjectVariableConsumerProbe.cs` | `ConsumerProbeOutcome`, `ProjectVariableConsumerProbe.Probe` | public | Sonda tri-estado por vista |
| `ProjectVariableScanEntry.cs` | `ProjectVariableScanEntry` | public | Vista barrida, proyectada |
| `ProjectVariableScanProjection.cs` | `ProjectVariableScanProjection.Project` | public | Proyección pura del barrido |
| `SelectiveAuthoredAuthority.cs` | `AuthoredAuthorityOutcome`, `AuthoredAuthorityResult`, `SelectiveAuthoredAuthority.IsSameAuthority/Resolve` | public | Autoridad authored multi-vista |
| `ProjectVariablesWorkspace.cs` | `ProjectVariableRow`, `RepairBinding`, `RackRepairBatch`, `RackWithoutAuthority`, `BrokenBindingRow`, `ProjectVariablesWorkspace.Build` | public | Proyección de RACKVARIABLES |
| `LinkedPropertySource.cs` | `LinkedPropertySourceKind`, `LinkedPropertySource` | public | Fuente declarada por el editor |
| `LinkedPropertyEditState.cs` | `LinkedPropertyEditState` | public | Estado final comprometido |
| `LinkedPropertyOptions.cs` | `LinkedPropertyOption`, `LinkedPropertyOptions.For/ForProperty`, `LinkedPropertyOptionsResult` | public / internal | Opciones acreditadas y compatibles |
| `LinkedPropertyEditSession.cs` | `LinkedPropertyDraftKind`, `LinkedPropertyStageOutcome`, `LinkedPropertyStageResult`, `LinkedPropertyEditSession` | public | Semántica de edición, pura |
| `LinkedPropertyReconciler.cs` | `LinkedPropertyReconcileOutcome`, `LinkedPropertyReconciliation`, `LinkedPropertyReconciler.Reconcile` | public | Estado final → authored + efectivo |

### 3.2 Application — persistencia, Selectivo y BOM

| Archivo | Símbolos principales | Rol |
|---|---|---|
| `Persistence/ProjectVariablesDocument.cs` | `ProjectVariablesDocument` (`CurrentSchemaVersion`, `SupportedMajor`, `CreateNew`, `ToProjectVariables`), `ProjectVariableDocument`, `ProjectVariableDefinitionDocument` | DTO del registro |
| `Persistence/ProjectVariablesStore.cs` | `Serialize`, `Read`, `Deserialize`, `FirstInvalidEntry`, `TryParseVersion` | Autoridad de legibilidad |
| `Persistence/ProjectVariablesPayload.cs` | `ProjectVariablesPayloadState`, `ProjectVariablesPayload` | Mitad física de la lectura |
| `Persistence/ProjectVariablesReadResult.cs` | `ProjectVariablesReadOutcome`, `ProjectVariablesReadResult` (`CanWrite`) | Resultado discriminado |
| `Persistence/ProjectVariablesWriteGuard.cs` | `ProjectVariablesWriteGuard.CanOverwrite` | Guarda de escritura |
| `Persistence/SelectivePropertyValueDocument.cs` | `ProjectVariableKind = "projectVariable"`, `ToProjectVariable` | Binding persistido |
| `Persistence/SelectivePalletDesignDocument.cs` | `PropertyValues`, `HasPropertyValues`, `HasBindingEntry`, `IsBound`, `TryGetBinding`, `WithDesign`, `ToDomain`, constantes de schema | Portador authored del rack |
| `Persistence/SelectiveDesignSchema.cs` | `SelectiveDesignSchema.ResolveWriteVersion` | Promoción *sticky* 1.x → 2.0 |
| `Persistence/SchemaVersionPolicy.cs` | `IsReadable`, `ResolveWriteVersion`, `MajorOf` | Política general de versión |
| `Persistence/RestampResult.cs` | `RestampResult`, `SelectiveAuthoredRestamp.Restamp` | Re-estampado de copias |
| `Systems/Selective/SelectiveEffectiveDesignResolver.cs` | `Resolve`, `ResolveAccredited`, `ResolveAgainst`, `ResolveWith` | EL resolver efectivo |
| `Systems/Selective/SelectiveEffectiveResolution.cs` | `SelectiveEffectiveOutcome`, `SelectiveEffectiveResolution` | Resultado tipado |
| `Systems/Selective/SelectiveEditorOpen.cs` | `SelectiveEditorOpen.Resolve`, `SelectiveEditorOpenResult`, `VerticalClearanceBindingState` | Compuerta de apertura de RACKEDITAR |
| `Systems/Selective/SelectiveLibraryExport.cs` | `Materialize`, `FromEffective` | Exportación materializada |
| `Bom/BomAuthoredAuthority.cs` | `BomAuthorityOutcome`, `BomAuthorityResult`, `BomAuthoredAuthority.Resolve` | Representante de cotización |

### 3.3 Plugin — `src/RackCad.Plugin/`

| Archivo | Símbolos principales | Rol |
|---|---|---|
| `ProjectVariablesData.cs` | `DictKey = "RACKCAD_PROJECT"`, `Read`, `Write` | Única escritura/lectura física del NOD |
| `ProjectVariablesRegistry.cs` | `Read`, `TryWrite` | Composición física + guardas puras |
| `ProjectVariableMutationExecutor.cs` | `MutationExecutionOutcome`, `MutationExecutionResult`, `Execute` | PREPARE / MUTATE / POST |
| `RackVariablesCommands.cs` | `RACKVARIABLES`, alias `RVA`, `Read` | Superficie central |
| `RackSelectivoCommands.cs` | `EditSelective` (`:47`; tramo de variables y reconciliación `:60-269`), `ReadProjectVariables`, `ApplyBinding`, `SerializeSelectiveDesign`, `WrapSelectivePayload` | RACKEDITAR del Selectivo |
| `KindHandlers/SelectiveKindHandler.cs` | `BuildBom`, `RestampDesign` | BOM y duplicación del Selectivo |
| `RackInventarioCommands.BomTotal.cs` | lectura única del registro para el total | RACKBOMTOTAL |
| `RackCommandSupport.cs` | `FindRackBlocks` | Vistas de un rack por GUID |
| `Systems/Shared/SystemBlockWriter.cs` | `ApplyRegen`, `PurgeAfterCommit` | Regen único |
| `RackEnvelopeRestamp.cs` | despacho de `RestampDesign` por Kind | Duplicación |

### 3.4 UI — `src/RackCad.UI/`

| Archivo | Símbolos principales | Rol |
|---|---|---|
| `Controls/LinkedPropertyEditor.cs` | `LinkedPropertyEditor` | Control compuesto literal / `=Nombre` |
| `Controls/PendingTextField.cs` | `IPendingTextField`, `PendingTextField<T>`, `PendingParse<T>` | Protocolo C4 de dos fases |
| `Systems/Selective/RackSelectiveWindow.xaml(.cs)` | `pendingAll`, `linkedEditors`, `CommitPendingEditors`, `SetProjectVariables`, `LinkedPropertyFinalStates`, `AttachLinkedEditors`, `TryEffective`, `Describe`, `BuildDesign`, `BindingIntent` | Integración del editor vinculable |
| `RackProjectVariablesWindow.xaml(.cs)` | ventana de RACKVARIABLES | UX central |
| `ProjectVariableRepairText.cs` | `Describe`, `DescribeUnresolvable` | Texto de reparación |
| `UiSupport.cs` | `TryNum`, `TryOptionalNum` | Parsing numérico de UI |

**Cifras de la auditoría.** Leídos directamente **64 archivos de producción**: los 30 de
`src/RackCad.Application/ProjectVariables/` completos, más 34 de persistencia, Selectivo, BOM, formato, unidades,
Plugin y UI, algunos por las secciones pertinentes. **≈249 identificadores distintos** (tipos, miembros, archivos y namespaces) citados en este mapa.
Tests: 5 archivos leídos directamente y **≈33 inventariados**, con ≈667 métodos y 20 guardas o pines (§14).
Además, barridos de parsing y unidades, búsquedas negativas y fuentes de parámetros computados sobre `src/`,
`tests/` y `docs/adr/`.

---

## 4. Modelo actual

### 4.1 Tipos, autoridades e invariantes

| Tipo | Hechos | Clase |
|---|---|---|
| `VariableId` | Struct; GUID generado una vez (`VariableId.cs:46`). `TryParse` rechaza lo que no es GUID y guarda `value.Trim()` (`:57-63`), aunque el comentario diga que el texto «nunca se normaliza» (`:23-24`). Igualdad y hash `OrdinalIgnoreCase` (`:79,83`) | FACT |
| `VariableType` | `enum { Length = 1 }`, sin miembro cero (`VariableType.cs:19-23`). Tabla cerrada `Supported` (`:32`) de la que derivan `IsSupported` (`:35-46`) y `TryParseToken`, que compara el **nombre** `OrdinalIgnoreCase` y no usa `Enum.TryParse` (`:64-97`) | FACT |
| `VariableDefinition` | Un caso (`:6-10`); campo `private readonly double _literal` (`:28`); constructor privado `(kind, double)` (`:30-34`); `LiteralValue` lanza fuera de `Literal` (`:39-51`); `Literal(double)` rechaza NaN/∞ (`:54-64`); igualdad por `Kind` + `_literal` (`:66-67`). **`VariableDefinitionKind` solo se usa dentro de este archivo** (grep sobre `src/`) | FACT |
| `ProjectVariable` | Entidad sin igualdad por valor (`ProjectVariable.cs:10-13`). `Create` exige id no vacío, tipo soportado, definición no nula y nombre no en blanco; **no** recorta el nombre ni comprueba unicidad (`:48-77`). En producción solo se construye en `ProjectVariableMutationPreflight.Create` (`:39`) y `Rename` (`:97`), y en `ToProjectVariables` (`ProjectVariablesDocument.cs:108`), que **no tiene consumidor de producción** | FACT |
| `PropertyId` | Struct; `TryParse` sin recorte (`PropertyId.cs:42-53`); igualdad `Ordinal` (`:67`) | FACT |
| `PropertyValue<T>` | `Literal = 1`, `ProjectVariableReference = 2` (`PropertyValue.cs:7-14`); accesos tipados que lanzan (`:54-81`). **Ningún archivo de `src/` fuera del suyo lo usa** | FACT |
| `LinkedPropertySource` | `Literal = 1`, `ProjectVariableReference = 2` (`LinkedPropertySource.cs:18-25`); enum y no `bool` para que una fórmula o una referencia rack→rack sean decisión en compilación (`:9-13`) | FACT |
| `SelectivePropertyValueDocument` | `Kind` string (solo `"projectVariable"`), `VariableId` string y `ExtensionData` (`SelectivePropertyValueDocument.cs:25-38`) | FACT |
| `ProjectPropertyIds` | `selective.verticalClearance` (`ProjectPropertyIds.cs:20`), `selective.palletTolerance` (`:35`); `IsKnown` delega en el catálogo (`:50`) | FACT |
| `SelectiveLinkedPropertyDescriptor` | `PropertyId` + `VariableType` + cuatro accesores **`double`** sobre `SelectivePalletDesignDocument` (authored) y `SelectivePalletDesign` de Domain (efectivo) (`SelectiveLinkedProperties.cs:23-69`). Catálogo estático cerrado de **2** entradas, ambas `Length` (`:186-225`) | FACT |
| `VariableTargetSnapshot` | `{VariableId, VariableType, double LiteralValue, Name}` (`VariableTargetSnapshot.cs:29-49`); `TryCreate` exige id y valor finito y **no** llama a `IsSupported` a propósito (`:21-25,55-80`) | FACT |
| `UsableProjectVariablesRegistry` | `Accredit(read)` solo admite `Absent`/`Readable` con documento; por entrada exige `VariableId` parseable, token de tipo soportado y `Definition.Value` no nulo; un id duplicado da `AmbiguousIdentity` (`UsableProjectVariablesRegistry.cs:120-182`). `FromTargets` es la costura pura de tests (`:195-226`). `Targets()` ordena por id `OrdinalIgnoreCase` (`:237-243`) | FACT |
| `LinkedPropertyInspection.InspectBinding` | Única autoridad sobre un binding: token→descriptor, `Kind == "projectVariable"` Ordinal, `VariableId`, existencia del target y tipo (`BindingInspection.cs:167-241`); cinco resultados (`:19-35`) | FACT |
| `SelectiveLinkedPropertyKernel` | `InspectBindings` en orden Ordinal (`SelectiveLinkedPropertyKernel.cs:121-149`); `Assess` → `Healthy/Repairable/Blocked` (`:8-18,72-96,152-156`); `PropertiesBoundTo(authored, id)` da el conjunto P de un rack (`:171-202`) | FACT |

**Autoridades vigentes, en una línea cada una (FACT):**

| Pregunta | Autoridad | Evidencia |
|---|---|---|
| ¿Qué registro es legible? | `ProjectVariablesStore.Deserialize` + `FirstInvalidEntry` | `ProjectVariablesStore.cs:101-208` |
| ¿Existe la entrada física? | `ProjectVariablesData.Read` (Absent / Present / PresentButUnreadable) | `ProjectVariablesData.cs:45-98` |
| ¿Es usable como autoridad de identidad? | `UsableProjectVariablesRegistry.Accredit` | `UsableProjectVariablesRegistry.cs:120-182` |
| ¿Qué es un binding? | `LinkedPropertyInspection.InspectBinding` | `BindingInspection.cs:167-241` |
| ¿Cuál es el valor efectivo? | `SelectiveEffectiveDesignResolver.ResolveWith` | `SelectiveEffectiveDesignResolver.cs:110-164` |
| ¿Qué authored tiene un rack? | `SelectiveAuthoredAuthority.Resolve` | `SelectiveAuthoredAuthority.cs:128-157` |
| ¿Quién consume una variable? | `ProjectVariableConsumerDiscovery.DiscoverConsumers` | `ProjectVariableConsumerDiscovery.cs:100-155` |
| ¿Qué puede hacer una operación? | `ProjectVariableMutationPreflight.*` | `ProjectVariableMutationPreflight.cs:30-530` |
| ¿Se puede escribir el registro? | `RegistryCommit.Prepare` + `ProjectVariablesWriteGuard.CanOverwrite` | `RegistryCommit.cs:89-107`, `ProjectVariablesWriteGuard.cs:24-41` |
| ¿Qué muestra y edita el campo? | `LinkedPropertyEditSession` | `LinkedPropertyEditSession.cs:92-444` |

### 4.2 Invariantes verificados en el código

- **FACT** — Identidad por `VariableId`: intents (`ProjectVariableIntent.cs:35-96`), vínculo
  (`SelectiveBindingIntent.cs:27-52`), selección del editor (`LinkedPropertyEditSession.cs:307-329`) y
  acreditación (`UsableProjectVariablesRegistry.cs:229-230`) direccionan por id.
- **FACT** — Registro presente e ilegible ≠ vacío: `ProjectVariablesPayload`/`ReadResult` separan los estados
  (`ProjectVariablesPayload.cs:4-17`, `ProjectVariablesReadResult.cs:4-23`); `Accredit` no convierte `null` en
  registro vacío (`UsableProjectVariablesRegistry.cs:129-142`); RACKEDITAR, RACKVARIABLES y RACKBOMTOTAL
  bloquean (`SelectiveEditorOpen.cs:172-182`, `ProjectVariablesWorkspace.cs:260-266`,
  `RackInventarioCommands.BomTotal.cs:60-67`).
- **FACT** — Plan vacío ante cualquier fallo: `VariableMutationPreflightResult.Failed/Blocked` devuelven
  `MutationPlan.Empty` (`VariableMutationPreflightResult.cs:62-72`); cada operación retorna en el primer
  fallo sin emitir racks parciales (`ProjectVariableMutationPreflight.cs:153-164,252-295`).
- **FACT** — Resolución sin escritura parcial: el resolver valida **todos** los bindings antes de escribir un
  solo campo (`SelectiveEffectiveDesignResolver.cs:133-161`).
- **FACT** — Identidad ambigua fail-closed: `Accredit` falla con `AmbiguousIdentity` sin elegir ni reparar
  (`UsableProjectVariablesRegistry.cs:168-175`), y el commit re-acredita la re-lectura antes de `ApplyTo`
  (`RegistryCommit.cs:97-106`).
- **FACT** — `ApplyTo` es `internal` y el Plugin no puede invocarlo (`MutationPlan.cs:76-87`).
- **FACT** — Una sola función efectiva en todo el repo: guarda `SIGUE_HABIENDO_UN_SOLO_RESOLVEDOR_EFECTIVO`
  (`tests/RackCad.Tests/ProjectVariablesConformanceTests.cs:226-234`).

### 4.3 Puntos de extensión reales

| Punto | Qué existe hoy | Límite verificado | Clase |
|---|---|---|---|
| `VariableDefinitionKind` | Enum de un miembro (`VariableDefinition.cs:6-10`) | La clase solo lleva un `double`; `LiteralValue` lanza fuera de `Literal`; la guarda `ProjectVariablesConformanceTests.cs:141-149` prohíbe `"Expression = "` en el archivo | FACT |
| `ProjectVariableDefinitionDocument.Kind` | Discriminador string + `Value double?` + `ExtensionData` (`ProjectVariablesDocument.cs:139-148`) | El store solo acepta `"literal"` y exige `Value` finito (`ProjectVariablesStore.cs:193-204`) | FACT |
| `VariableType` | Tabla cerrada única (`VariableType.cs:32`) | Un segundo tipo exige tocar la tabla; `ProjectVariable.Create` lanza con tipos no soportados (`ProjectVariable.cs:57-62`) | FACT |
| `LinkedPropertySourceKind` | Enum de dos miembros (`LinkedPropertySource.cs:18-25`) | Añadir un miembro obliga a revisar cada `IsReference`/`switch` en compilación | FACT |
| `SelectivePropertyValueDocument.Kind` | String discriminado (`SelectivePropertyValueDocument.cs:25-31`) | Un kind desconocido es fatal en inspección y sonda (`BindingInspection.cs:193-206`, `ProjectVariableConsumerProbe.cs:79-84`) | FACT |
| `LinkedPropertyDescriptorSet` | Parámetro de funciones puras, catálogo cerrado en producción (`SelectiveLinkedProperties.cs:86-158,176-184`) | Descriptores `double` y Selectivos | FACT |
| `UsableProjectVariablesRegistry.FromTargets` | Costura pura para tests (`UsableProjectVariablesRegistry.cs:195-226`) | Solo targets literales | FACT |
| ADR-0034 §15 | «ID22B (fórmulas, que extenderá `ProjectVariable.Definition`)» (`docs/adr/0034-project-variables-autoridad-drawing-level.md:167-169`) | Dirección aceptada; no dice cómo | FACT (doc) |

### 4.4 Acoplamientos que I-49 tendría que romper o preservar

| Acoplamiento | Evidencia | ¿Romper o preservar? | Clase |
|---|---|---|---|
| Definición = número | `VariableDefinition.cs:28-51`; `ProjectVariableDefinitionDocument.Value` (`ProjectVariablesDocument.cs:144`) | Romper: una expresión no es un `double` | FACT / INFERENCE |
| Target acreditado = literal | `VariableTargetSnapshot.cs:29-49`; `UsableProjectVariablesRegistry.cs:150,161-162` | Romper o envolver: el efectivo de una expresión no existe en disco | FACT / INFERENCE |
| Efectivo = `Target.LiteralValue` | `SelectiveEffectiveDesignResolver.cs:160` | Romper: hace falta un valor evaluado | FACT / INFERENCE |
| Opciones del editor = `LiteralValue` | `LinkedPropertyOptions.cs:25-46,115-116`; `LinkedPropertyEditSession.cs:218-239` | Romper: el editor muestra y usa ese número | FACT / INFERENCE |
| Escritura del registro = `"literal"` | `MutationPlan.cs:96-106,119-131` | Romper | FACT / INFERENCE |
| Intents numéricos | `ProjectVariableIntent.cs:62,73-80,127-135` | Romper para crear o cambiar una expresión | FACT / INFERENCE |
| Identidad por `VariableId` | §4.2 | **Preservar**: una expresión debe referir ids, no nombres | FACT / PROPOSAL INPUT |
| Resolución única en Application | `ProjectVariablesConformanceTests.cs:226-234` | **Preservar** | FACT |
| Propiedades y consumidores solo Selectivos | `SelectiveLinkedProperties.cs:23-69`; `ProjectVariableConsumerDiscovery.cs:241` | Preservar en I-49; es el eje de ID20/ID23 más adelante | FACT / PROPOSAL INPUT |
| Evaluación futura sin depender de ProjectVariables | OWNER INPUT (ID23); hoy no hay evaluación | Decidir la frontera | PROPOSAL INPUT |

---

## 5. Persistencia y schema

### 5.1 Forma JSON actual

**FACT** — Opciones del store: sin `PropertyNamingPolicy`, así que las propiedades salen en PascalCase;
`WriteIndented = false`, `PropertyNameCaseInsensitive = true`, comentarios omitidos, comas finales admitidas,
`WhenWritingNull` y `JsonStringEnumConverter` (`ProjectVariablesStore.cs:249-262`). Forma, tal como la fijan
los fixtures de test (`tests/RackCad.Tests/ProjectVariablesDocumentTests.cs:36-41`;
`ProjectVariablesRegistryAccessTests.cs:28`):

```json
{"SchemaVersion":"1.0","Variables":[{"VariableId":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44","Name":"Holgura","Type":"Length","Definition":{"Kind":"literal","Value":6.0}}]}
```

- **FACT** — Almacenamiento físico: `Xrecord` en el NOD bajo `RACKCAD_PROJECT`, con el JSON troceado en
  cadenas `DxfCode.Text` de hasta 255 caracteres (`ProjectVariablesData.cs:36-38,104-134`).
- **FACT** — `Type` es un string en el DTO (`ProjectVariablesDocument.cs:127`); lo escribe `Type.ToString()`
  (`MutationPlan.cs:100`). `Kind` lo escribe `"literal"` literal (`MutationPlan.cs:103,126`).
- **INFERENCE** — El texto exacto de un número al re-serializar lo decide `System.Text.Json` (forma más corta
  que hace *round-trip*); ningún test fija bytes numéricos del registro, así que `6.0` puede reescribirse
  como `6`.

**Lado rack (FACT).** Mapa opcional `PropertyValues` en el portador authored
(`SelectivePalletDesignDocument.cs:128-129`), omitido cuando es nulo:

```json
"PropertyValues":{"selective.verticalClearance":{"Kind":"projectVariable","VariableId":"<guid>"}}
```

Un documento con bindings pasa a la línea **2.0**, *sticky*, y nunca vuelve a 1.x
(`SelectivePalletDesignDocument.cs:24-39`; `SelectiveDesignSchema.cs:43-66`).

### 5.2 Autoridad de legibilidad

| Paso | Qué decide | Evidencia | Clase |
|---|---|---|---|
| Físico | Absent / Present / PresentButUnreadable (nunca lanza) | `ProjectVariablesData.cs:45-98` | FACT |
| Payload nulo | PresentButUnreadable, no ausente | `ProjectVariablesStore.cs:77-81` | FACT |
| Versión | Ausente, en blanco o no `MAJOR.MINOR` estricto → PresentButUnreadable; major > 1 → IncompatibleMajor | `ProjectVariablesStore.cs:128-147,223-247` | FACT |
| Entradas | Entrada nula, id no GUID, nombre en blanco, tipo desconocido, sin definición, `Kind` ≠ `literal` (OrdinalIgnoreCase), valor ausente/NaN/∞ → PresentButUnreadable | `ProjectVariablesStore.cs:157-208` | FACT |
| Unicidad | **No** la comprueba el store; la exige `Accredit` | `ProjectVariablesStore.cs:157-208`; `UsableProjectVariablesRegistry.cs:168-175` | FACT |
| Escritura | Solo tras Absent o Readable | `ProjectVariablesReadResult.cs:60-61`; `ProjectVariablesWriteGuard.cs:24-41` | FACT |

### 5.3 Fail-closed ante kinds y tipos desconocidos

- **FACT** — `Kind` desconocido (test con `"expression"`), tipo desconocido (test con `"Volumen"`) y major
  superior (test con `"2.0"`) son errores duros (`ProjectVariablesDocumentTests.cs:132-160`).
- **FACT** — Consecuencia **en todo el dibujo**, no solo en los racks vinculados: RACKEDITAR de **cualquier**
  Selectivo se bloquea (`SelectiveEditorOpen.cs:172-182`), RACKVARIABLES queda bloqueado
  (`ProjectVariablesWorkspace.cs:260-266`), RACKBOMTOTAL no genera listado
  (`RackInventarioCommands.BomTotal.cs:60-67`), no hay opciones de vínculo (`LinkedPropertyOptions.cs:136-140`)
  y ningún commit de registro se aplica (`RegistryCommit.cs:97-104`).

### 5.4 Versión y ExtensionData

- **FACT** — `ExtensionData` en tres niveles: raíz, entrada y definición (`ProjectVariablesDocument.cs:57-58,131-132,146-147`).
- **FACT** — Re-guardar conserva un minor mayor del mismo major (`ProjectVariablesStore.cs:54-56`;
  `SchemaVersionPolicy.cs:40-51`), probado con `"1.7"` y con `"1.1"` + campo desconocido en la raíz
  (`ProjectVariablesDocumentTests.cs:209-233`).
- **FACT** — Crear o escribir estampa explícitamente `"1.0"`; leer nunca inventa versión
  (`ProjectVariablesDocument.cs:15-32,47,61-62`; tests `:187-207`).
- **FACT** — La decisión de I-47 que gobierna esta evolución: «`VariableType` desconocido y `Definition.kind`
  desconocido = ERROR … ID22B decidirá su evolución» (`docs/automation/decisions/I-47.md:194`, C4-8).

### 5.5 Qué tocaría, como mínimo, un `Definition.Kind = expression`

No es un diseño: es la lista de lugares que **hoy** rechazan o asumen literal. Cada ubicación es **FACT**; que
«habría que cambiarla» es **INFERENCE**.

| # | Ubicación | Comportamiento actual |
|---|---|---|
| 1 | `ProjectVariablesStore.cs:29,193-197` | Rechaza cualquier `Kind` ≠ `"literal"` |
| 2 | `ProjectVariablesStore.cs:199-204` | Exige `Value` finito en toda entrada |
| 3 | `ProjectVariablesDocument.cs:139-148` | `ProjectVariableDefinitionDocument` solo tiene `Value double?`: no hay campo para la expresión |
| 4 | `UsableProjectVariablesRegistry.cs:150,161-162` | Exige `Definition.Value` y construye el target con él |
| 5 | `VariableTargetSnapshot.cs:29-49` | El target es `LiteralValue` |
| 6 | `SelectiveEffectiveDesignResolver.cs:160` | Escribe `Target.LiteralValue` |
| 7 | `ProjectVariablesDocument.cs:108-112` | `ToProjectVariables` construye `VariableDefinition.Literal` |
| 8 | `VariableDefinition.cs:28-51` | Payload `double`; `LiteralValue` lanza fuera de `Literal` |
| 9 | `MutationPlan.cs:96-106,119-131` | `Add` y `ChangeValue` escriben `{Kind:"literal", Value}` |
| 10 | `ProjectVariableMutationPreflight.cs:97-98` | `Rename` valida reconstruyendo un literal |
| 11 | `ProjectVariableIntent.cs:62,73-80,127-135` | Intents con `double Value` y `Literal(value)` |
| 12 | `LinkedPropertyOptions.cs:25-46,115-116` | Opción con `LiteralValue` |
| 13 | `LinkedPropertyEditSession.cs:218-239` | Efectivo de una referencia = `option.LiteralValue` |
| 14 | `ProjectVariablesWorkspace.cs:22,310-315` | Fila con `LiteralValue` |
| 15 | `RackProjectVariablesWindow.xaml:55,68-69`; `.xaml.cs:73,215-224` | Muestra y edita un número `> 0` |
| 16 | `SelectiveBindingIntent.cs:111,120` | `ForLength` exige `Definition.Value` (sin consumidor de producción) |
| 17 | `tests/RackCad.Tests/ProjectVariablesDocumentTests.cs:152-160` | Test que exige **rechazar** `"expression"` |
| 18 | `tests/RackCad.Tests/ProjectVariablesConformanceTests.cs:141-149` | Guarda que prohíbe `"Expression = "` en `VariableDefinition.cs` |

### 5.6 ¿Es viable una evolución minor? Hechos, sin decidir

- **FACT** — Un build de la línea I-47/I-48 lee **cualquier** `1.x` (`ProjectVariablesDocument.cs:40`;
  `ProjectVariablesStore.cs:135-147`) y conserva su minor y sus campos desconocidos al re-guardar (§5.4).
- **FACT** — Ese mismo build rechaza el registro **entero** si **una** entrada tiene `Kind` ≠ `literal`, con el
  mensaje «declara una definición de clase desconocida» (`ProjectVariablesStore.cs:193-197`), y con un major
  superior dice «fueron creadas con una versión más nueva de RackCad» (`:142-147`).
- **INFERENCE** — Un registro `1.x` que contuviera una expresión seguiría fallando **cerrado** en builds
  anteriores (sin lectura silenciosa y sin escritura), lo que cumple C4-8; pero el usuario vería un mensaje de
  «clase desconocida» y no de «versión más nueva», y todo lo Selectivo de ese DWG quedaría bloqueado en ese build.
- **INFERENCE** — Un registro sin expresiones escrito como `1.0` por un build nuevo seguiría siendo legible,
  sin cambio, por los builds anteriores.
- **PROPOSAL INPUT** — Minor o major; si la versión cambia cuando no existe ninguna expresión; qué mensaje ve un
  build anterior; si hace falta señalizar por documento o por entrada.

### 5.7 Cómo preservar los DWG de I-47/I-48

- **FACT** — Leer nunca escribe: el NOD solo lo escribe `ProjectVariablesRegistry.TryWrite`
  (`ProjectVariablesRegistry.cs:30-51`), y solo lo llama el ejecutor cuando el plan trae una `RegistryMutation`
  distinta de `None` (`ProjectVariableMutationExecutor.cs:252-275`). Un DWG de I-47/I-48 abierto por cualquier
  build conserva los bytes del registro hasta el primer commit de registro.
- **FACT** — Re-escribir **no** conserva bytes: re-serializa con las opciones del store (§5.1). Lo que sí
  conserva es versión (sin degradar el minor), `ExtensionData` y contenido (§5.4).
- **FACT** — La comparación multi-vista es **estructural** (objetos sin orden, números por valor), así que una
  diferencia de formato no fabrica divergencia (`SelectiveAuthoredAuthority.cs:162-235`).
- **PROPOSAL INPUT** — Qué equivalencia se exige a un DWG I-47/I-48 re-guardado por un build con motor: bytes,
  árbol JSON o semántica; y con qué fixtures dorados se prueba.

### 5.8 Persistencia de property values del Selectivo

- **FACT** — Presencia frente a interpretación: `HasBindingEntry` solo mira la clave
  (`SelectivePalletDesignDocument.cs:170-171`); `TryGetBinding` exige `Kind` Ordinal e id parseable (`:186-199`).
- **FACT** — Promoción: `Link` usa `SelectiveDesignSchema.ResolveWriteVersion(stored, true)`
  (`ProjectVariableMutationPreflight.cs:356`); el reconciliador fija `"2.0"` sin condición
  (`LinkedPropertyReconciler.cs:218`); el store vuelve a resolver al serializar
  (`SelectivePalletDesignStore.cs:29-31`).
- **INFERENCE (hallazgo lateral L2)** — Por esa asignación incondicional, un documento guardado como `2.x`
  (x > 0) por un build futuro podría re-escribirse como `2.0` al pasar por RACKEDITAR. Fuera del alcance de
  I-49; no hay test que lo cubra.
- **FACT** — `WithDesign` comparte la **misma instancia** del diccionario `PropertyValues` en vez de clonarla
  (`SelectivePalletDesignDocument.cs:321`) y solo congela el literal de la holgura (`:324-327`); para la
  tolerancia lo congela el reconciliador, que es el único llamador de `WithDesign` en `src/`
  (`LinkedPropertyReconciler.cs:146,204`).
- **FACT** — Duplicar re-estampa el documento conservando bindings, literal congelado, versión y campos
  desconocidos (`RestampResult.cs:42-83`); exportar a biblioteca **materializa** y vuelve a `"1.0"`
  (`SelectiveLibraryExport.cs:61-94`).

---

## 6. Mutación y propagación

### 6.1 Traza exacta de `ChangeValue(variable)` desde RACKVARIABLES

| # | Paso | Evidencia | Clase |
|---|---|---|---|
| 1 | El comando lee en **una** transacción el registro y el barrido completo, y proyecta cada definición a una entrada pura | `RackVariablesCommands.cs:101-125`; `ProjectVariableScanProjection.cs:30-65` | FACT |
| 2 | `LastRegistry = registry.Document` (nulo si el registro es ilegible) y `LastEntries` = **todas** las entradas | `RackVariablesCommands.cs:121-122` | FACT |
| 3 | `ProjectVariablesWorkspace.Build` proyecta filas y reparaciones | `ProjectVariablesWorkspace.cs:250-320` | FACT |
| 4 | La ventana copia `LiteralValue` a `ValueBox` con `"0.###"`; «Cambiar valor» parsea con `UiSupport.TryNum` y exige `> 0` | `RackProjectVariablesWindow.xaml.cs:73,158-168,215-224` | FACT |
| 5 | `ProjectVariableIntentPreflight.Run` → `VariableDefinition.Literal(value)` → `Preflight.ChangeValue` | `ProjectVariableIntent.cs:133-135` | FACT |
| 6 | **Preflight — antes**: acredita el documento como `Readable` y comprueba que el target existe | `ProjectVariableMutationPreflight.cs:58-74,127-135` | FACT |
| 7 | **Racks afectados**: `DiscoverConsumers(entries, id)` → consumidores propiedad→variable de ESA variable (§7) | `ProjectVariableMutationPreflight.cs:137-142` | FACT |
| 8 | **Estado después**: `Accredit(mutation.ApplyTo(registry))`; `ApplyTo` clona y escribe `{Kind:"literal", Value}` | `ProjectVariableMutationPreflight.cs:144-149`; `MutationPlan.cs:87-140` | FACT |
| 9 | **Efectivo**: por consumidor, clon del authored **sin cambios** y `ResolveAgainst(authored, after)`; un solo fallo vacía el plan | `ProjectVariableMutationPreflight.cs:151-164`; `SelectiveEffectiveDesignResolver.cs:110-164` | FACT |
| 10 | **Mutaciones**: `MutationPlan.Of(ChangeValue, racks)`: 1 `RegistryMutation` + 1 `RackMutation` por rack | `ProjectVariableMutationPreflight.cs:163-166`; `MutationPlan.cs:148-212` | FACT |
| 11 | **PREPARE** (sin transacción de escritura): por rack `FindRackBlocks` (transacción y barrido propios), `MutationDestinationBinding.Bind`, geometría del efectivo, **una** serialización del authored, sobre por vista y `PrepareRedraw` | `ProjectVariableMutationExecutor.cs:143-244`; `RackCommandSupport.cs:109-134`; `MutationDestinationBinding.cs:62-135` | FACT |
| 12 | **Transacción**: una sola; re-lee el registro, `RegistryCommit.Prepare` acredita esa re-lectura y aplica, `TryWrite`, se escriben todas las vistas y hay un `Commit` | `ProjectVariableMutationExecutor.cs:250-289`; `RegistryCommit.cs:89-107` | FACT |
| 13 | **Redibujo / regen**: purga de definiciones huérfanas y `ApplyRegen(document, prepared.Count > 0)`: **un** `Regen` | `ProjectVariableMutationExecutor.cs:291-296`; `SystemBlockWriter.cs:135-141` | FACT |
| 14 | El bucle vuelve a leer el dibujo y reabre la ventana | `RackVariablesCommands.cs:47-82` | FACT |

### 6.2 Dónde vive hoy la limitación de profundidad 1

| Lugar | Por qué es profundidad 1 | Evidencia | Clase |
|---|---|---|---|
| Descubrimiento | Un solo target por llamada; la sonda solo mira propiedades que referencian ese id | `ProjectVariableMutationPreflight.cs:137,188,235`; `ProjectVariableConsumerProbe.cs:52-93` | FACT |
| Plan | Una `RegistryMutation` por plan | `MutationPlan.cs:191-212` | FACT |
| Resolución | binding → snapshot → `LiteralValue`; ningún valor se calcula a partir de otro | `SelectiveEffectiveDesignResolver.cs:147-161` | FACT |
| Delete | Solo cuenta consumidores propiedad→variable | `ProjectVariableMutationPreflight.cs:188-202` | FACT |
| RACKVARIABLES | Consumidores directos por variable | `ProjectVariablesWorkspace.cs:300-316` | FACT |
| ADR-0034 | «Persistir un grafo de dependencias — ID22A tiene profundidad 1; el grafo pertenece a ID22B/ID21» | `docs/adr/0034-project-variables-autoridad-drawing-level.md:182` | FACT (doc) |

### 6.3 Qué tendría que cambiar conceptualmente para dependencias transitivas

Sin proponer implementación:

- **INFERENCE** — Los racks afectados por un cambio en X dejarían de ser «consumidores de X» y pasarían a ser
  la **unión** de los consumidores de X y de toda variable cuyo valor dependa de X. Hoy esa unión no se puede
  calcular: no hay ninguna relación variable→variable (§6.2).
- **INFERENCE** — La unión tendría que **deduplicarse por rack**: `RackMutation` lleva un efectivo completo y
  `MutationDestinationBinding` aborta si una definición aparece dos veces (`MutationDestinationBinding.cs:85-91`).
- **INFERENCE** — La resolución necesitaría valores **evaluados** del registro «después» antes de construir
  snapshots, porque hoy el snapshot es el literal persistido (`UsableProjectVariablesRegistry.cs:161-162`).
- **INFERENCE** — Un ciclo, una dependencia rota o un tipo incompatible tendrían que detectarse **antes** de
  emitir plan para conservar «plan vacío ante fallo» (`VariableMutationPreflightResult.cs:62-72`).
- **INFERENCE** — Rename seguiría siendo registry-only si las expresiones refieren ids; Delete necesitaría ver
  dependientes variable→variable, que viven **dentro del registro** y no requieren barrer el dibujo.
- **PROPOSAL INPUT** — Dónde se calcula el cierre transitivo y la evaluación (acreditación, resolver o capa
  nueva); si los valores evaluados se persisten o se derivan; vocabulario de fallos; coste en dibujos grandes
  (ADR-0034 lo deja «sin medir», `docs/adr/0034-project-variables-autoridad-drawing-level.md:212-213`).

---

## 7. Descubrimiento de consumidores

| Aspecto | Hechos | Evidencia | Clase |
|---|---|---|---|
| Entrada | Barrido de **definiciones de bloque** del dibujo en el Plugin, proyectado a `ProjectVariableScanEntry` | `RackVariablesCommands.cs:111-116`; `RackSelectivoCommands.cs:287-292`; `RackInventarioCommands.BomTotal.cs:52`; `ProjectVariableScanProjection.cs:30-65` | FACT |
| Clasificación | Sobre ilegible, sin id o sin kind → `UnreadableEnvelope`; kind no Selectivo → `Foreign`; diseño que no deserializa → `SelectiveUnreadableDesign` | `ProjectVariableScanProjection.cs:37-64`; `ProjectVariableScanEntry.cs:63-104` | FACT |
| Precondición global | Cualquier entrada con sobre no interpretable **aborta** ambas familias | `ProjectVariableConsumerDiscovery.cs:104-109,165-170,197-216` | FACT |
| Agrupación | Solo Selectivos con `RackId`, `OrdinalIgnoreCase`, en orden de barrido | `ProjectVariableConsumerDiscovery.cs:228-259` | FACT |
| Familia A (target-variable) | Sonda por vista: `Indeterminate` aborta; 0 positivos ignora el rack; positivos parciales abortan; exige autoridad `Single` | `ProjectVariableConsumerDiscovery.cs:100-155` | FACT |
| Familia B (target-rack) | El rack lo elige el usuario; sin sonda; exige autoridad `Single` | `ProjectVariableConsumerDiscovery.cs:161-189` | FACT |
| Sonda | Recorre el mapa **entero**; propiedad desconocida, kind desconocido o id ilegible → `Indeterminate` | `ProjectVariableConsumerProbe.cs:52-93` | FACT |
| Autoridad multi-vista | Comparación estructural de árboles persistidos; `SchemaVersion` y `ExtensionData` cuentan | `SelectiveAuthoredAuthority.cs:63-157,168-235` | FACT |
| Colocación | La ventana ignora entradas no colocadas (`DirectReferenceCount == 0`) | `ProjectVariablesWorkspace.cs:282,337-355` | FACT |
| Asimetría (hallazgo lateral L1) | El preflight recibe **todas** las entradas (`RackVariablesCommands.cs:59-60,122`) y aborta ante cualquier sobre ilegible, esté colocado o no; y un registro bloqueado por una definición colocada ilegible sigue ofreciendo reparaciones (`ProjectVariablesWorkspace.cs:284-294`) que `ResolveTargetRack` abortaría | código citado; sin test que lo cubra | INFERENCE |
| Coste | RACKVARIABLES ejecuta **un** `DiscoverConsumers` por variable al abrir (`ProjectVariablesWorkspace.cs:300-316`); el ejecutor, un barrido por rack (`ProjectVariableMutationExecutor.cs:147`) | código citado | FACT; coste O(variables × vistas) = INFERENCE |

**Reverse information disponible hoy (FACT):** consumidores de una variable
(`DiscoverConsumers`), propiedades de un rack ligadas a una variable (`PropertiesBoundTo`,
`SelectiveLinkedPropertyKernel.cs:171-202`), resumen rack + propiedades (`Summarize`,
`ProjectVariableMutationPreflight.cs:562-589`) y el plan completo antes de ejecutar (`MutationPlan`). Todo se
**calcula bajo demanda** desde un barrido; nada se persiste.

---

## 8. Operaciones

### 8.1 Tabla por operación

| Operación | Inputs | Identidad | Precondiciones | Fail-closed | Qué muta | Qué NO muta | Discovery | Nombres duplicados | authored / effective |
|---|---|---|---|---|---|---|---|---|---|
| **Create** (`ProjectVariableMutationPreflight.cs:30-48`) | `name`, `VariableType`, `VariableDefinition` (desde RACKVARIABLES: `Length` + `Literal(value)`, `ProjectVariableIntent.cs:126-128`) | id **nuevo** (`VariableId.New()`, `:39`) | Nombre no en blanco, tipo soportado, definición no nula (`ProjectVariable.cs:48-77`); la UI exige valor `> 0` y recorta el nombre (`RackProjectVariablesWindow.xaml.cs:136-144,215-224`) | `ArgumentException` → plan vacío (`:41-44`); en commit, la re-lectura debe acreditarse (`RegistryCommit.cs:97-104`) | Registro: `Add` | Ningún rack; sin redibujo (`ProjectVariableMutationExecutor.cs:293`) | Ninguno | **No** comprueba unicidad; el preflight ni siquiera recibe el registro | No aplica |
| **Rename** (`:80-107`) | registro, `VariableId`, `newName` | `VariableId` | Registro acreditado; target existe; nombre válido | Plan vacío si el registro no se acredita o falta el target | Registro: `Rename` (`List.Find`, primera coincidencia OrdinalIgnoreCase, `MutationPlan.cs:109-117,142-144`) | Racks, bindings, valor | Ninguno | Permitidos; no hay regla de nombres | Nada que resolver: las referencias viajan por id |
| **ChangeValue** (`:121-167`) | registro, `VariableId`, `VariableDefinition`, entradas | `VariableId` | Target existe; discovery con éxito; registro «después» acreditado; todos los consumidores resuelven | Cualquier fallo → plan vacío | Registro: `ChangeValue`; rack: redibujo con el efectivo nuevo | Literal authored del consumidor (clon sin cambios, `:155,163`) | Familia A | Irrelevante | authored igual; efectivo = valor nuevo |
| **Delete** (`:173-206`) | registro, `VariableId`, entradas | `VariableId` | Target existe; discovery con éxito; **cero** consumidores | `BlockedByConsumers` lista racks y propiedades (`:195-202`) | Registro: `Remove` (`RemoveAll`, `MutationPlan.cs:133-136`) | Racks | Familia A | Irrelevante | No aplica |
| **Link** (`:316-366`) | registro, `rackId`, `PropertyId`, `VariableId`, entradas | `VariableId` + `PropertyId` | Propiedad en el catálogo; target existe; **tipo compatible** (`:339-344`); rack con autoridad única; el rack entero resuelve | Plan vacío (incluido otro binding roto del mismo rack) | Rack: `PropertyValues[prop]` + promoción de schema (`:353-356`) | Registro; literal authored (queda **congelado**) | Familia B | Irrelevante (por id) | efectivo = valor de la variable |
| **Unlink** (`:383-434`) | registro, `rackId`, `PropertyId`, entradas | `PropertyId` | Descriptor; rack; entrada presente (`:405-408`); el efectivo actual **resuelve** (`:410-417`) | Vínculo roto → error que remite a reparar | Rack: literal = efectivo actual (**materializa**) y se quita el binding (`:423-424`) | Registro; schema promovido | Familia B | Irrelevante | authored = efectivo previo; efectivo sin cambio geométrico |
| **RepairBroken** (`RepairBrokenRack`, `:458-530`) | registro, `rackId`, entradas, `confirmed` | `rackId` (la unidad es el rack, `ProjectVariableIntent.cs:89-95,150-156`) | Rack con autoridad única; assessment **no** `Blocked`; al menos un `Missing`; confirmación | Cualquier FATAL bloquea el rack entero (`:475-482`); sin confirmar → error con el detalle (`:491-512`) | Rack: quita **todos** los bindings `Missing` (`:514-520`) | Literales almacenados; schema promovido | Familia B | Irrelevante | Efectivo = literales almacenados; puede cambiar la geometría |
| **UnlinkAllAndDelete** (`:220-298`) | registro, `VariableId`, entradas (+ confirmación en el intent, `ProjectVariableIntent.cs:140-148`) | `VariableId` | Target existe; discovery; registro «después» acreditado; cada consumidor resuelve antes y después; P no vacío | Plan vacío | Registro: `Remove`; racks: materializa **todo** P y quita esos bindings (`:252-295`) | Otras propiedades del rack | Familia A | Irrelevante | authored = efectivo previo en todo P |

**Nota (FACT):** `Link` y `Unlink` existen en el preflight, pero el camino heredado que los invocaba
(`SelectiveBindingIntentPreflight.Run`, `SelectiveBindingIntent.cs:136-158`, llamado desde
`RackSelectivoCommands.ApplyBinding`, `:305-326`) **no tiene disparador en producción**:
`RackSelectiveWindow.BindingIntent` (`RackSelectiveWindow.xaml.cs:2398`) no se asigna en ningún archivo de
`src/`. Desde I-48, vincular y desvincular en RACKEDITAR entra por el **reconciliador**
(`RackSelectivoCommands.cs:144-151`; `LinkedPropertyReconciler.cs:113-220`).

### 8.2 Lo que ID22B tendría que considerar

| Situación | Hoy | ID22B | Clase |
|---|---|---|---|
| **Renombrar una variable usada en una fórmula** | Registry-only, sin racks ni redibujo (`ProjectVariableMutationPreflight.cs:76-107`) | Si la fórmula persiste **ids**, sigue siendo registry-only y solo cambia su texto mostrado; si persiste **nombres**, obliga a reescribir fórmulas y rompe la independencia nombre/identidad | FACT (hoy) / INFERENCE / PROPOSAL INPUT |
| **Borrar una variable usada en una fórmula** | Delete solo ve consumidores propiedad→variable (`:188-202`) | Delete tendría que ver dependientes variable→variable, que están **en el registro** | FACT / INFERENCE |
| **Dependencia rota** | Un target ausente es `RepairableMissingTarget` (`BindingInspection.cs:219-227`); se repara por rack con literal almacenado | Una variable cuya fórmula refiere un id ausente no tiene «literal almacenado» equivalente: la reparación rack-scoped no la cubre | FACT / INFERENCE / PROPOSAL INPUT |
| **Dependientes aguas abajo** | No existen | Cambiar X afecta a los consumidores de todo dependiente de X (§6.3) | INFERENCE |
| **UnlinkAllAndDelete** | Materializa los consumidores directos | ¿Qué materializa una variable dependiente cuya fuente desaparece? | PROPOSAL INPUT |
| **Tipo** | Compatibilidad propiedad↔variable en `Link`, `InspectBinding` y opciones | Una expresión necesita tipar operandos y resultado; `VariableType` tiene un solo miembro | FACT / PROPOSAL INPUT |

---

## 9. `LinkedPropertyEditor` y el protocolo C4

### 9.1 Piezas y fronteras

| Pieza | Capa | Responsabilidad | Evidencia | Clase |
|---|---|---|---|---|
| `LinkedPropertyEditSession` | Application, pura | **Toda** la semántica de edición: borradores, Enter, Escape, LostFocus, selección, C4 y congelado 20.13 | `LinkedPropertyEditSession.cs:62-91` | FACT |
| `LinkedPropertyEditor` | UI (WPF) | Pintar texto, abrir la lista, mover la selección y reportar el foco; «no consulta el registro, no resuelve ids, no juzga tipos, no escribe vínculos» | `src/RackCad.UI/Controls/LinkedPropertyEditor.cs:21-26` | FACT |
| `LinkedPropertyOptions.ForProperty` | Application | Opciones **acreditadas** y compatibles por tipo del descriptor | `LinkedPropertyOptions.cs:127-141` | FACT |
| `LinkedPropertyEditState` | Application | Estado final: `CommittedLiteral` + `Source`, sin historia ni borradores | `LinkedPropertyEditState.cs:5-33` | FACT |
| `LinkedPropertyReconciler` | Application | Estado final → authored + efectivo, atómico | `LinkedPropertyReconciler.cs:60-179` | FACT |
| `SelectiveEditorOpen` | Application | Estados iniciales por descriptor del catálogo; bloquea la apertura si algo no resuelve | `SelectiveEditorOpen.cs:157-227` | FACT |
| `RackSelectiveWindow` | UI | Mapa `linkedEditors`, `pendingAll`, commit en dos fases | `RackSelectiveWindow.xaml.cs:449-481,558-612,2410-2473` | FACT |

**FACT** — Las opciones se calculan **una vez**, con el `PropertyId` de la holgura, y se comparten con los dos
editores (`RackSelectivoCommands.cs:80-89`; `RackSelectiveWindow.xaml.cs:2405-2423`). Hoy es inocuo porque las
dos propiedades son `Length`.

### 9.2 Estado y clasificación del texto

- **FACT** — Campos: `_options`, `_committed`, `_text`, `_dirty`, `_stagedLiteral`, `_staged`
  (`LinkedPropertyEditSession.cs:94-99`).
- **FACT** — `Draft` (`:120-138`): sin edición → `Committed`; **si `IsQuery(text)`** → `DraftReferenceQuery`; si
  parsea → `DraftLiteral`; si no → `InvalidDraft`. `IsQuery` = `text.TrimStart().StartsWith("=", Ordinal)`
  (`:431-432`): se evalúa **antes** que el número.
- **FACT** — `Candidates` (`:174-204`): fuera de consulta, todas las opciones; dentro, el filtro es
  `text.Substring(1).Trim()` comparado con `Name` por **contains** `OrdinalIgnoreCase`. Filtra y **nunca**
  resuelve.
- **FACT** — `DisplayOf` (`:376-399`): literal → `ToString("0.###", InvariantCulture)`; referencia → `"=" +` el
  nombre de la **primera** opción con ese id, o `"=" + id` si no está en oferta.

### 9.3 Gestos

| Gesto | Control | Sesión | Efecto | Clase |
|---|---|---|---|---|
| Teclear | `Box_TextChanged` → `session.Type` + `RefreshCandidates` (`LinkedPropertyEditor.cs:269-278`) | `Type` (`:242-247`) | Borrador; no muta nada. La selección de la lista se **limpia** en cada tecla y el popup solo se abre con consulta y candidatos (`LinkedPropertyEditor.cs:386-398`) | FACT |
| Enter | Si hay candidato **navegado** → `Select`; si no, `TryCommitByEnter` (`:338-355`) | `TryCommitByEnter` (`:253-277`) | Literal válido: se compromete (incluido Referencia→Literal); consulta: rechazo «Elige una variable de la lista: el texto solo filtra, no la resuelve.»; inválido: rechazo | FACT |
| Escape | `ResetToCommitted` (`:255-265`) | `Cancel` → `Resolve` (`:332,424-429`) | Vuelve exactamente al estado comprometido | FACT |
| Perder el foco | `Box_LostKeyboardFocus` → `HandleFocusLeaving` (`:169-194,313-323`); moverse a la lista **no** cuenta (`OwnsFocus`, `:140-163`) | `TryCommitByLostFocus` (`:283-300`) | Solo compromete si **no** cambia la fuente; si la cambia, el borrador sobrevive y se emite `Refused` | FACT |
| Clic en candidato | `Candidates_Commit` → `Select` (`:325-332,357-368`) | `TrySelect(id)` (`:307-329`) | Id no ofrecido → rechazo; mismo id → no-op; otro → `Reference(CommittedLiteral, id)` | FACT |
| Arriba/Abajo | Solo con el popup abierto (`:299-309`) | — | Mueve la selección sin confirmar | FACT |

### 9.4 Transiciones actuales

| Transición | Cómo se llega | Qué persiste el reconciliador | Clase |
|---|---|---|---|
| **literal → literal** | Número + Enter, perder el foco o frontera C4 (`TryStage` = `Ready`, `ApplyStaged`) | `WriteAuthored(b)` y se quita el binding si lo hubiera (`LinkedPropertyReconciler.cs:204-210`) | FACT |
| **literal → reference** | `=` + filtro + **selección explícita** (clic, o navegar + Enter) | `WriteAuthored(literal comprometido)`, `PropertyValues[prop] = projectVariable(X)`, `SchemaVersion = "2.0"` (`:204,212-218`) | FACT |
| **reference → reference** | `=` + seleccionar Y; si es el mismo X, no-op (`LinkedPropertyEditSession.cs:319-325`) | Literal congelado conservado; binding a Y | FACT |
| **reference → literal** | **Solo** número + Enter: perder el foco lo rechaza (`:292-297`) y C4 lo bloquea (`:349-352`) | `WriteAuthored(b)` y se quita el binding | FACT |
| **borrador inválido** | Enter lo rechaza; perder el foco emite `Refused`; C4 da `Blocked` («no es un numero valido», `:354-355`) | Nada: la ventana aborta la frontera entera y devuelve el foco al editor (`RackSelectiveWindow.xaml.cs:578-583,632-638`) | FACT |
| **C4 con dos propiedades** | `pendingAll` incluye `ClearanceEditor` y `ToleranceEditor` (`RackSelectiveWindow.xaml.cs:449-452`). Fase 1: `TryStage` de **todos**; un bloqueo aborta todo y enfoca al primer editor culpable. Fase 2: `ApplyStaged` de todos y **un** `Recompute` (`:558-612`) | `LinkedPropertyFinalStates` entrega las dos (`:2434-2450`); el reconciliador aplica ambas y resuelve el rack **completo** una vez (`LinkedPropertyReconciler.cs:148-178`) | FACT |

### 9.5 Parsing y formato del editor

- **FACT** — Literal = `double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture)` + finito; valida
  la **forma**, no el rango (`LinkedPropertyEditSession.cs:82-85,434-443`).
- **FACT** — El rango lo impone la ventana al construir el diseño: `TryEffective` rechaza valores `< 0`
  (`RackSelectiveWindow.xaml.cs:2481-2505`), usado para tolerancia y holgura (`:2307,2312`).
- **FACT** — Presentación: `"0.###"` invariante en `DisplayOf` (`:385`), `LinkedPropertyOption.DisplayText`
  (`LinkedPropertyOptions.cs:62-65`) y `Describe` de la ventana (`RackSelectiveWindow.xaml.cs:2533-2539`).

### 9.6 Qué asume hoy que todo `=` es una consulta de referencia (pregunta 11)

| Ubicación | Qué asume | Clase |
|---|---|---|
| `LinkedPropertyEditSession.IsQuery` (`:431-432`) | Cualquier texto que empieza por `=` (tras recortar espacios iniciales) es consulta | FACT |
| `LinkedPropertyEditSession.Draft` (`:129-131`) | La consulta gana a cualquier otra lectura | FACT |
| `LinkedPropertyEditSession.Candidates` (`:183-203`) | Lo que sigue al `=` es un **filtro de nombre** | FACT |
| `LinkedPropertyEditSession.DraftChangesSource` (`:156-158`) | Una consulta siempre cambia la fuente | FACT |
| `LinkedPropertyEditSession.TryCommitByEnter` (`:269-271`) | Una consulta nunca se compromete por texto | FACT |
| `LinkedPropertyEditSession.TryStage` (`:349-352`) | Una consulta bloquea la frontera | FACT |
| `LinkedPropertyEditSession.DisplayOf` (`:388-398`) | Una referencia se muestra como `=Nombre` | FACT |
| `LinkedPropertyEditor.RefreshCandidates` (`LinkedPropertyEditor.cs:393-397`) | Consulta ⇒ lista de variables | FACT |
| `RackSelectiveWindow.Describe` (`RackSelectiveWindow.xaml.cs:2537`) | `Text.TrimStart('=')` es el nombre de la variable | FACT |

**INFERENCE (por código, sin test que lo fije)** — Hoy `=Holgura General + 2` sería una consulta cuyo filtro
`"Holgura General + 2"` no coincide con ningún nombre: cero candidatos, popup cerrado, Enter rechazado y la
frontera C4 bloqueada. No se pierde nada, pero tampoco hay forma de comprometerla.

### 9.7 El seam real para una futura fuente `Expression`

Inventario de lo que una tercera fuente tendría que atravesar. Las ubicaciones son **FACT**; que sea el seam
mínimo es **INFERENCE**; su forma es **PROPOSAL INPUT**.

| Capa | Seam | Por qué es obligado |
|---|---|---|
| Modelo de fuente | `LinkedPropertySourceKind` + `LinkedPropertySource` (solo lleva `VariableId`, `LinkedPropertySource.cs:36-81`) | Es el enum que el propio código reservó para un tercer caso (`:9-13`) |
| Estado final | `LinkedPropertyEditState` (`CommittedLiteral` + `Source`, `LinkedPropertyEditState.cs:21-33`) | Es lo único que cruza del editor al reconciliador |
| Clasificación del texto | `LinkedPropertyEditSession.Draft` / `IsQuery` (`:120-138,431-432`) | Es donde hoy se decide qué significa `=` |
| Compromiso | `TryCommitByEnter`, `TryCommitByLostFocus`, `TryStage`, `ApplyStaged` (`:253-369`) | Reglas de fuente y C4 |
| Valor en vigor y texto | `TryGetEffectiveValue`, `DisplayOf` (`:218-239,376-399`) | Hoy usan `option.LiteralValue` y el nombre |
| Persistencia del vínculo | `LinkedPropertyReconciler.Apply` (`LinkedPropertyReconciler.cs:193-220`) + `SelectivePropertyValueDocument.Kind` | Única rama que escribe `PropertyValues` |
| Interpretación | `LinkedPropertyInspection.InspectBinding`, `ProjectVariableConsumerProbe.Probe`, resolver (`BindingInspection.cs:193-206`; `ProjectVariableConsumerProbe.cs:79-84`; `SelectiveEffectiveDesignResolver.cs:130-161`) | Un kind nuevo es FATAL/`Indeterminate` hoy |

**PROPOSAL INPUT — tensión documental que el Discovery no resuelve.** HANDOFF §4 registra como dirección no
normativa que «el mismo campo admitiría más adelante `=Holgura General + 2`»
(`docs/HANDOFF.md:1591-1594`), es decir, una expresión **escrita en la propiedad**. ADR-0034 §15, aceptado,
dice que ID22B «extenderá `ProjectVariable.Definition`» y reserva la extensión de `PropertyValue<T>` para ID21
(`docs/adr/0034-project-variables-autoridad-drawing-level.md:167-169`). La Proposal tiene que decidir si una
expresión en un campo es (a) un tipo nuevo de valor de propiedad, (b) una variable implícita del registro,
o (c) algo no admitido en el campo, con expresiones solo en definiciones de variable.

---

## 10. RACKVARIABLES

### 10.1 Comando y bucle

| Paso | Hecho | Evidencia | Clase |
|---|---|---|---|
| Registro | `[CommandMethod("RACKVARIABLES")]` y alias `RVA` | `RackVariablesCommands.cs:31-33` | FACT |
| Lectura | Una transacción: registro + barrido + proyección pura | `RackVariablesCommands.cs:101-125` | FACT |
| Ventana | Modal, construida con el workspace | `RackVariablesCommands.cs:50-52` | FACT |
| Cierre sin intent | Termina el comando | `RackVariablesCommands.cs:54-57` | FACT |
| Preflight | `ProjectVariableIntentPreflight.Run(intent, LastRegistry, LastEntries)` | `RackVariablesCommands.cs:59-60` | FACT |
| Fallo | Mensaje en la línea de comandos, más los consumidores que bloquean; vuelve al bucle | `RackVariablesCommands.cs:62-74` | FACT |
| Éxito | `ProjectVariableMutationExecutor.Execute`; mensaje con vistas redibujadas | `RackVariablesCommands.cs:76-81` | FACT |
| Refresco | **Re-lee el dibujo en cada vuelta**: la ventana nunca es la autoridad | `RackVariablesCommands.cs:24-27,47-50` | FACT |

### 10.2 Workspace

- **FACT** — `Build(read, entries)` (`ProjectVariablesWorkspace.cs:250-320`): bloquea si no hay lectura, si no es
  `Absent`/`Readable` o si la acreditación falla (incluida la identidad ambigua); filtra entradas colocadas;
  calcula **primero** las reparaciones; bloquea ante una definición colocada ilegible; y crea una fila por
  target con `DiscoverConsumers` + `Summarize`.
- **FACT** — Filas: `ProjectVariableRow(Id, Name, Type, LiteralValue, Consumers)` (`:16-47`). Reparación:
  `BrokenBindingRow` con el lote completo del rack (`RackRepairBatch`) o el motivo de bloqueo
  (`:53-194,429-522`).

### 10.3 UX exacta de la ventana

- **FACT** — Título «Variables de proyecto», 800×640 (`RackProjectVariablesWindow.xaml:4-8`). Cabecera: «Valores
  del DIBUJO. Un rack vinculado toma su valor de aquí; el literal que tenía queda congelado.» (`:31`).
- **FACT** — Banner rojo de bloqueo con el error del workspace (`:35-38`; `.xaml.cs:45-49`).
- **FACT** — Lista de variables: nombre en negrita y debajo `Type · valor LiteralValue · racks: ConsumerCount`,
  con bindings **sin** `StringFormat` y **sin** id ni desambiguador (`.xaml:47-62`).
- **FACT** — Campos «Nombre» y «Valor (in)» (`.xaml:65-69`); botones Nueva, Renombrar, Cambiar valor y Eliminar
  (`:71-76`); «Racks que la usan» (`:78-79`); casilla de confirmación y botón «Desvincular todos y eliminar»
  (`:81-84`).
- **FACT** — Al seleccionar: `NameBox` = nombre y `ValueBox` = `LiteralValue.ToString("0.###", Invariant)`
  (`.xaml.cs:66-79`). Renombrar, Cambiar valor y Eliminar requieren workspace editable y selección; Desvincular
  todos exige además la casilla (`:112-119`).
- **FACT** — Panel «Referencias rotas» **fuera** del panel editable, a propósito (`.xaml:88-118`): lista
  `RackName · PropertyId` / `variable VariableId · literal almacenado StoredLiteral`, texto de autoridad, aviso
  rack-scoped (`ProjectVariableRepairText.cs:29-51`), casilla y botón «Reparar (quitar el vínculo)». Reparar
  exige `RackCanRepair` y la casilla, **con independencia** de que el registro sea editable
  (`.xaml.cs:121-126,194-206`).
- **FACT** — Valor: `UiSupport.TryNum(ValueBox.Text)` y `> 0`; si no, «El valor tiene que ser un número mayor que
  cero.» (`.xaml.cs:215-224`).

### 10.4 Intents

| Intent | Payload | Origen | Clase |
|---|---|---|---|
| `Create(name, value)` | nombre **recortado** en la UI, `double` | `.xaml.cs:136-144`; `ProjectVariableIntent.cs:73-74` | FACT |
| `Rename(id, name)` | id + nombre recortado | `.xaml.cs:146-156`; `ProjectVariableIntent.cs:76-77` | FACT |
| `ChangeValue(id, value)` | id + `double` | `.xaml.cs:158-168`; `ProjectVariableIntent.cs:79-80` | FACT |
| `Delete(id)` | id | `.xaml.cs:170-180` | FACT |
| `UnlinkAllAndDelete(id, confirmed: true)` | id + confirmación | `.xaml.cs:182-192`; confirmación exigida también en `ProjectVariableIntent.cs:140-146` | FACT |
| `RepairBroken(rackId, confirmed: true)` | **rack**, sin propiedad | `.xaml.cs:194-206`; `ProjectVariableIntent.cs:89-95` | FACT |

**FACT** — RACKVARIABLES **no** tiene Link ni Unlink: se vincula desde RACKEDITAR (§9).

### 10.5 Qué asume hoy RACKVARIABLES

| Supuesto | Evidencia | Clase |
|---|---|---|
| **Valor literal** | `ProjectVariableRow.LiteralValue` (`ProjectVariablesWorkspace.cs:39`); `ValueBox` (`RackProjectVariablesWindow.xaml.cs:73`); intents `double` (`ProjectVariableIntent.cs:62`); `Create`/`ChangeValue` → `VariableDefinition.Literal` (`ProjectVariableIntent.cs:127-135`) | FACT |
| **`Length`** | «Valor (in)» (`.xaml:68`); `Create` fija `VariableType.Length` (`ProjectVariableIntent.cs:127-128`); valor `> 0` (`.xaml.cs:217`) | FACT |
| **Consumidores directos** | `DiscoverConsumers` por variable (`ProjectVariablesWorkspace.cs:302`); «Racks que la usan» = rack + propiedades (`RackProjectVariablesWindow.xaml.cs:92-110`) | FACT |
| **Referencias directas** | Referencias rotas = bindings de racks (`ProjectVariablesWorkspace.cs:429-486`); no existe noción de variable rota por dependencia | FACT |
| **Identidad visible** | La lista no muestra ids: dos homónimas solo se distinguen por valor y conteo (`.xaml:50-58`) | FACT |

---

## 11. Parsing y formato numérico

### 11.1 Componentes reutilizables

| Componente | Regla | Capa | Consumidores | Clase |
|---|---|---|---|---|
| `LocalizedNumberParser` (`src/RackCad.Application/Formatting/LocalizedNumberParser.cs:11-64`) | Estilos: signo, punto decimal y espacios; **sin** miles ni exponente (`:13-14`). Orden: cultura actual → invariante → una sola coma sin punto como decimal (`:28-50`); siempre finito (`:63`). `TryInteger`: actual y luego invariante (`:56-61`) | Parser de Application para texto humano | `UiSupport.TryNum/TryInt` (`src/RackCad.UI/UiSupport.cs:97-102`), `NumericFieldValidation` (`src/RackCad.UI/Controls/NumericFieldValidation.cs:93-105`), helpers de RackFrames | FACT |
| ADR-0015, **aceptado** | «Parsear la entrada numérica con una regla localizada única»: punto o coma decimal y sin agrupadores (`docs/adr/0015-entrada-numerica-localizada.md:3,27-36`) | Norma | — | FACT (doc) |
| `UiSupport.TryNum` / `TryInt` / `TryOptionalNum` (`UiSupport.cs:96-121`) | Pasarelas; `TryOptionalNum`: vacío → null, `> 0` → valor | UI | ≈84 llamadas en 16 archivos de UI | FACT |
| `NumericField` + `NumericFieldValidation` (`src/RackCad.UI/Controls/NumericField.cs:21-212`) | Parser localizado + rango + mensajes; escribe `"0.###"` con **CurrentCulture** (`NumericField.cs:131-136`; `NumericFieldValidation.cs:136`) | UI (parser y formateador) | Ventanas de Push Back y Cantilever; **no** Selectivo, Dinámico ni RACKVARIABLES | FACT |
| `PendingTextField<T>` (`src/RackCad.UI/Controls/PendingTextField.cs:50-194`) | Sin regla numérica propia: delega en el `parse` que recibe | UI | Ventana Selectiva | FACT |
| `LinkedPropertyEditSession.TryParseLiteral` (`LinkedPropertyEditSession.cs:439-443`) | **Solo invariante**, `NumberStyles.Float` (admite exponente, no miles), finito, sin rango | Parser de Application para texto de UI | Editor vinculable | FACT |
| `StrictCsvTable.Row.OptionalDouble/RequiredDouble` (`src/RackCad.Application/StructuralSections/StrictCsvTable.cs:185-217`) | Invariante `Float`; lanza ante no-número o no finito | Persistencia (CSV de catálogo) | Catálogo neutral de secciones | FACT |
| `CsvCatalogReader.TrySetTyped` (`src/RackCad.Application/Catalogs/CsvCatalogReader.cs:80-125`) | Invariante `Float | AllowThousands`; **traga** excepciones y deja el valor por defecto | Persistencia (CSV heredado) | Catálogo heredado | FACT |
| `PeralteList.Parse` (`src/RackCad.Application/Catalogs/PeralteList.cs:11-40`) | Separa por `;` y `,`; invariante y luego actual, con miles; `> 0`; sin duplicados | Application | Ventanas Selectiva y Larguero | FACT |
| `GeometryTolerance` (`src/RackCad.Application/Geometry/Vector2D.cs:83-131`) | `IsFinite`, `AreClose`, `RequireFinite`, `RequirePositive`; tolerancias **en pulgadas** (`:86`) | Application (geometría) | 37 archivos de geometría y Cantilever | FACT |

### 11.2 Familias de reglas texto→número

| Familia | Regla | Sitios representativos | Clase |
|---|---|---|---|
| R1 | `LocalizedNumberParser` | RACKVARIABLES `ValueBox` (`RackProjectVariablesWindow.xaml.cs:215-224`); casi todos los campos de las ventanas Selectiva, Dinámica, Cama, seguridad, layout y RackFrames | FACT |
| R2 | Solo invariante, `Float` | `LinkedPropertyEditSession.cs:439-443`; `StrictCsvTable.cs:194-202`; `RackFrameConfiguratorLayoutStore.cs:52` | FACT |
| R3 | Toda coma → punto, invariante | `src/RackCad.UI/Systems/Cantilever/CantileverPanelSegmentRow.cs:136-151` | FACT |
| R4 | Actual → invariante, `Float` | `StructuralSectionInspectorState.cs:122-137`; `StructuralSectionInspectorWindow.cs:321-337` | FACT |
| R5 | Admite miles | `PeralteList.cs:34-39`; `CsvCatalogReader.cs:100`; `RackLargueroWindow.xaml.cs:85` | FACT |
| R6 | Enteros | `LocalizedNumberParser.TryInteger`; `SelectiveFondoTargets.cs:67-144`; parsers de versión (`SchemaVersionPolicy.cs:56-78`; `ProjectVariablesStore.cs:223-247`) | FACT |
| R7 | El framework parsea | Bindings WPF TwoWay a `double` (`RackFrameConfiguratorWindow.xaml:712,715,820`); prompts de AutoCAD `GetDistance`/`GetInteger` (p. ej. `RackCabeceraCommands.cs:77-97`) | FACT (INFERENCE: en esos prompts AutoCAD podría aceptar pies-pulgadas según LUNITS) |

**FACT** — La Proposal V1 de I-48, superada, decía que el parseo «vive en `LocalizedNumberParser` y sigue ahí»
(`docs/initiatives/I-48-proposal-v1.md:165,394`); las Proposals V2–V8 no mencionan la regla (búsqueda sin
resultados); y el código integrado usa R2 (`LinkedPropertyEditSession.cs:439-443`). La divergencia **no consta
como decisión** en ningún documento.

### 11.3 Divergencia observable en las dos superficies de Project Variables

Comportamiento deducido de la semántica de .NET y del código citado (**INFERENCE**); ninguna de estas
combinaciones está fijada por un test en esas superficies.

| Entrada | Editor vinculable (R2) | RACKVARIABLES (R1 + `> 0`) |
|---|---|---|
| `96.5` | 96.5 | 96.5 |
| `96,5` | **rechaza** | 96.5 |
| `1,234` | rechaza | 1.234 (coma única = decimal) |
| `1e3` | **1000** | rechaza (sin exponente) |
| `-5` | parsea; la ventana lo rechaza al construir el diseño (`< 0`, `RackSelectiveWindow.xaml.cs:2498`) | rechaza (`> 0`, `RackProjectVariablesWindow.xaml.cs:217`) |
| `0` | admitido (`>= 0`) | **rechaza** |
| `NaN`, `Infinity` | rechaza | rechaza |
| `12 in`, `12"`, `10'6"`, `1 1/8` | rechaza | rechaza |

### 11.4 Validación del valor de una variable

| Paso | Regla | Evidencia | Clase |
|---|---|---|---|
| Ventana RACKVARIABLES | R1 y `> 0` | `RackProjectVariablesWindow.xaml.cs:215-224` (sin test que fije `> 0`) | FACT |
| Intent → definición | Finito (`VariableDefinition.Literal`) | `ProjectVariableIntent.cs:127-135`; `VariableDefinition.cs:54-64` | FACT |
| Store | Presente y finito | `ProjectVariablesStore.cs:199-204` | FACT |
| Acreditación | Finito | `VariableTargetSnapshot.cs:72-76` | FACT |
| Literal vinculado | Finito, sin rango | `LinkedPropertyEditSession.cs:439-443`; `LinkedPropertyEditState.cs:53-57` | FACT |
| Valor en vigor en el Selectivo | `>= 0` | `RackSelectiveWindow.xaml.cs:2498` | FACT |
| Intent con NaN | `Literal()` se evalúa **fuera** del `try` del preflight, así que lanzaría en vez de devolver `Failed`; no es alcanzable desde la ventana | `ProjectVariableIntent.cs:127-135`; `ProjectVariableMutationPreflight.cs:37-44` | INFERENCE |
| Igualdad | Exacta (`double.Equals`) en definición, estado de edición y autoridad multi-vista | `VariableDefinition.cs:66-67`; `LinkedPropertyEditState.cs:62-63`; `SelectiveAuthoredAuthority.cs:228-229` | FACT |

### 11.5 Formato de presentación

- **FACT** — `"0.###"` con `InvariantCulture` en todo lo que muestra el valor de una variable o de una propiedad
  vinculada: `DisplayOf` (`LinkedPropertyEditSession.cs:385`), candidatos (`LinkedPropertyOptions.cs:62-65`),
  estado bajo el campo (`RackSelectiveWindow.xaml.cs:2533-2539`), `ValueBox` (`RackProjectVariablesWindow.xaml.cs:73`)
  y texto de reparación (`ProjectVariableRepairText.cs:99`).
- **FACT** — Sin formato: los `Run` de `LiteralValue` y `StoredLiteral` en la ventana central
  (`RackProjectVariablesWindow.xaml:55,105`). **INFERENCE**: WPF formatea con su `Language` por defecto (en-US) y a
  precisión completa.
- **FACT** — `ToString()` implícito, sensible a la cultura actual, en `VariableTargetSnapshot`,
  `LinkedPropertyEditState` y `PropertyValue<T>` (`VariableTargetSnapshot.cs:82-83`; `LinkedPropertyEditState.cs:69`;
  `PropertyValue.cs:118-119`). Solo se usan en diagnóstico y depuración.
- **FACT** — En todo `src/`: `"0.###"` 58 veces, `"0.####"` 55, `"0.##"` 49 y `"R"` 6; ninguna interpolación con
  formato numérico.
- **INFERENCE (hallazgo lateral L5)** — Campos sembrados con `"0.###"` y releídos pierden los dígitos que no se
  muestran: `ValueBox` de RACKVARIABLES, separadores y editor de celda del Selectivo. **FACT**: las ventanas de
  Cantilever lo evitan con `Keep()` (`RackCantileverWindow.xaml.cs:366-382`).

### 11.6 Representación numérica persistida

- **FACT** — Ningún archivo usa `JsonNumberHandling`, `AllowNamedFloatingPointLiterals` ni un conversor numérico
  propio; las opciones del registro no tocan números (`ProjectVariablesStore.cs:249-262`).
- **FACT** — `Value` es `double?` (`ProjectVariablesDocument.cs:144`); dentro de Persistence, la única comprobación
  de finitud al cargar es la del registro (`ProjectVariablesStore.cs:199-204`).
- **INFERENCE** — `System.Text.Json` escribe números en forma invariante y *round-trip* y no puede escribir NaN ni
  ∞: lanzaría al serializar.
- **FACT** — Otras representaciones: CSV del catálogo neutral con `"R"` invariante
  (`StructuralSectionCsvWriter.cs:142`); claves de BOM con `Round(4)` + `"R"` (`ConsolidatedBom.cs:63`).

### 11.7 Sintaxis de longitudes

- **FACT** — No existe parser de pies-pulgadas (`10'6"`) ni de fracciones (`1/2`).
- **FACT** — Solo dos helpers privados quitan `in` y `"` antes de parsear (`RackFrameConfiguratorViewModel.cs:2248-2266`,
  que exige `> 0`; `HorizontalEditorRow.cs:247-268`, que exige `>= 0`).
- **FACT** — La unidad vive en las etiquetas y no en el texto: «Valor (in)» (`RackProjectVariablesWindow.xaml:68`),
  «Holgura vertical (in)» (`RackSelectiveWindow.xaml:138`).
- **FACT** — Los mismos caracteres ya tienen significados distintos en el repositorio: `,` es decimal en R1 pero
  separador de lista en `PeralteList.cs:22` y `SelectiveFondoTargets.cs:94`; `-` y `+` son operadores de rango y
  lista en `SelectiveFondoTargets.cs:94,96`.

### 11.8 Qué debe preservarse (pregunta 10)

- **FACT** — ADR-0015, aceptado: una única regla localizada para la entrada humana, fijada por
  `LocalizedNumberParserTests` (`TryDouble_AcceptsEitherDecimalSeparator`, `TryDouble_RejectsGroupedOrNonFiniteInput`)
  y `NumericFieldValidationTests`.
- **FACT** — Editor vinculable: forma invariante + finito, sin rango, con los pines de §14.3 (G20): texto inválido no
  muta y bloquea la frontera; un literal se muestra como `"6"`.
- **FACT** — Presentación `"0.###"` invariante en las superficies de Project Variables; persistencia JSON por
  defecto; igualdad exacta.
- **PROPOSAL INPUT** — Qué regla rige los literales numéricos **dentro** de una expresión; si se hace converger el
  editor con RACKVARIABLES y ADR-0015; cómo se resuelve coma decimal frente a separador de argumentos; qué
  precisión se muestra y se persiste.

---

## 12. Unidades

### 12.1 Decisiones vigentes

- **FACT** — ADR-0005, **aceptado**: «La pulgada es la unidad interna canónica de RackCad»
  (`docs/adr/0005-estrategia-de-unidades.md:31-33`); I-05 «NO convierte, reescala ni reinterpreta nada» (`:35-48`);
  toda conversión futura será una frontera explícita DWG↔interno con ADR propio (`:50-54`); la columna `units`
  de los catálogos es decorativa (`:10-13,69-71,125-137`).
- **FACT** — ADR-0021, **aceptado**: la geometría resuelta sigue en pulgadas (`docs/adr/0021-identidad-unidades-y-presentacion-de-secciones.md:102-103`);
  las unidades nativas del catálogo AISC son US customary (`:105-107`); las equivalencias se calculan con
  factores exactos (`:117-119`); formateador puro e invariante (`:130-146`); nada de eso autoriza convertir el DWG
  (`:190-193`).
- **FACT** — ADR-0034 §3: un solo tipo, `Length` (`docs/adr/0034-project-variables-autoridad-drawing-level.md:76-78`);
  `VariableType`: «A length, in inches … introducing no conversion» (`VariableType.cs:4-5,21`).

### 12.2 Unidad interna real (pregunta 9)

- **FACT** — Pulgadas, como `double` sin unidad: ADR-0005 §1, ADR-0021 §7, `VariableType.cs:4-5,21`,
  `StructuralSectionUnits.cs:10-11`, `Vector2D.cs:86` y las etiquetas «(in)» de la UI.
- **FACT** — El registro de variables **no persiste unidad** (`ProjectVariablesDocument.cs:120-148`).
  **INFERENCE**: la unidad la implica el token `"Length"`.
- **FACT** — La masa no tiene unidad interna única: el peso de tarima es número + etiqueta de unidad y el catálogo
  de secciones conserva la unidad nativa de la fuente.
- **FACT** — Las pendientes usan dos notaciones («rise per 12 in» en Cantilever; «7/16 in per foot» en
  `DynamicHeaderHeightCalculator.cs:42-49`) y los ángulos van en radianes en Application (`Vector2D.cs:99-100`).

### 12.3 Helpers existentes

| Helper | API | Neutral o específico | Consumidores | Clase |
|---|---|---|---|---|
| `StructuralSectionUnits` (`src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs:16-118`) | Constantes `25.4`, `645.16` y lb/ft→kg/m exacta; cuatro conversores escalares; cinco métodos de peso que reciben `StructuralSectionDefinition` | **Específico**: su doc lo limita a «DATA of a catalogued section» y dice que «does not convert the drawing» (`:5-15`) | Formateador de etiquetas, `PrismaticSectionInstance`, inspector de UI, `StructuralSectionCommandFlow`, importador de `tools/` | FACT |
| `StructuralSectionUnitSystem(s)` (`StructuralSectionSource.cs:6-41`) | `UsCustomary`/`Metric` + tokens | Específico | Catálogo de secciones | FACT |
| `StructuralSectionLabelFormatter` | `"lb/ft"`, `"kg/m"`, nativo primero | Específico (presentación) | Inspector, comando | FACT |
| `DrawingUnits` + `DrawingUnitsAdvisory` (`src/RackCad.Application/Drawing/DrawingUnitsAdvisory.cs:11-40`) | `Inches`/`Unitless`/`Other`; aviso si no es pulgadas | **Neutral**, sin conversión | `RackUnitsGuard` | FACT |
| `RackUnitsGuard` (`src/RackCad.Plugin/RackUnitsGuard.cs:20-64`) | Lee `INSUNITS` y avisa | Neutral respecto de secciones; ligado a AutoCAD | Comandos de inserción | FACT |
| `GeometryTolerance` | Tolerancias en pulgadas | Neutral (geometría) | Geometría y Cantilever | FACT |
| Constantes de 12 in/ft | `StructuralSectionUnits.cs:27` (privada), `SelectiveGeometryResolver.cs:31` (`FootInches`), `DynamicHeaderHeightCalculator.cs:48-49` (`CommercialFoot`) | Específicas | Reglas de altura y pendiente | FACT |
| Cadenas de unidad en modelos | `CatalogEntries.Units`, `RackFrameConfiguration.Units = "in"`, `PalletSpecification.WeightUnit` | Metadato: solo se copian o muestran | — | FACT |

- **FACT** — No existe ningún tipo general `Length`, `Unit`, `Measure` ni `Quantity`.
- **FACT** — En `src/`, el factor `25.4` solo aparece en `StructuralSectionUnits.cs:19`, y una guarda lo prohíbe en el
  materializador del Plugin (`StructuralSectionPluginSourceGuardTests.NothingIsScaledOnTheWayIntoTheDrawing`); el
  factor lb/ft→kg/m está duplicado como literal en `tools/RackCad.StructuralSections.Import/AiscShapesImporter.cs:108`.

### 12.4 ¿Existe una autoridad neutral reutilizable?

- **FACT** — No hay autoridad neutral de **conversión** de unidades. Las piezas neutrales que existen —`DrawingUnits`,
  `GeometryTolerance` y `LocalizedNumberParser`— no contienen factores de conversión.
- **FACT** — No existe parser neutral de unidades (§15).

### 12.5 ¿Puede reutilizarse `StructuralSectionUnits` sin acoplar mal?

- **FACT** — Solo importa `System`, vive en el mismo ensamblado que ProjectVariables y lo que alcanza está en su
  propio namespace. ProjectVariables hoy no referencia `StructuralSections` y no hay guarda que lo impida; sí la
  hay para Cantilever (`tests/RackCad.Tests/CantileverSourceGuardTests.cs:160-169`).
- **INFERENCE** — Compilaría, pero introduciría una dependencia de catálogo en un concepto neutral (una variable de
  longitud), contra el alcance que el propio helper documenta y contra el «no conversion» de `VariableType`. Solo
  sus cuatro conversores escalares y sus constantes serían usables sin arrastrar tipos de sección.
- **PROPOSAL INPUT** — Si las expresiones llevan unidades; si hace falta una autoridad neutral de unidades; si eso
  exige ADR (ADR-0005 §4 lo exige para convertir el DWG).

---

## 13. Semántica de nombres duplicados

### 13.1 Legalidad y normalización

| Pregunta | Respuesta | Evidencia | Clase |
|---|---|---|---|
| ¿Son legales? | **Sí.** `ProjectVariable.Create` solo exige un nombre no en blanco (`ProjectVariable.cs:69-74`); el store solo exige que no esté en blanco (`ProjectVariablesStore.cs:177-180`); un test lo afirma (`tests/RackCad.Tests/ProjectVariablesModelTests.cs:52`) | código + test | FACT |
| ¿Dónde se permiten? | En todo el registro: `Create` ni siquiera recibe el registro (`ProjectVariableMutationPreflight.cs:30-48`) y `Rename` no mira los demás nombres (`:80-107`) | código | FACT |
| ¿Se normalizan? | Application **no** recorta ni pliega mayúsculas; la ventana central **recorta** al crear y al renombrar (`RackProjectVariablesWindow.xaml.cs:143,155`) | código | FACT |
| ¿Hay política de mayúsculas o unicidad? | No existe ninguna | código + barrido de tests (§14.2) | FACT |

### 13.2 Presentación y desambiguación

| Superficie | Qué muestra | ¿Distingue homónimas? | Evidencia | Clase |
|---|---|---|---|---|
| Lista de candidatos del editor | `Nombre = valor("0.###")  [8 primeros caracteres del id]` | **Sí**, el desambiguador es obligatorio | `LinkedPropertyOptions.cs:48-65`; test `LinkedPropertyEditSessionTests.cs:357` | FACT |
| Texto comprometido del campo | `=Nombre` (nombre de la **primera** opción con ese id) | **No** | `LinkedPropertyEditSession.cs:388-398` | FACT |
| Estado bajo el campo | `variable: Nombre = valor · el literal … queda congelado en …` | **No** | `RackSelectiveWindow.xaml.cs:2533-2539` | FACT |
| Lista de RACKVARIABLES | Nombre, tipo, valor y conteo de racks, **sin id** | **No**, salvo por valor o conteo | `RackProjectVariablesWindow.xaml:50-58` | FACT |
| Nombre ligado al abrir RACKEDITAR | Búsqueda **por id** en el registro acreditado | No aplica | `SelectiveEditorOpen.cs:145-155` | FACT |

### 13.3 Filtrado, comparadores y resolución nombre→id

- **FACT** — El único uso del nombre en una decisión es el **filtro** de candidatos: `Name.IndexOf(query,
  OrdinalIgnoreCase) >= 0` (`LinkedPropertyEditSession.cs:196`). Filtra y no resuelve; comprometer exige
  `TrySelect(VariableId)` (`:307-329`), y ni un nombre exacto ni un candidato único se auto-seleccionan (tests
  `LinkedPropertyEditSessionTests.cs:212-262`).
- **FACT** — Una guarda impide resolver por nombre en el control: prohíbe `Name ==` y
  `FirstOrDefault(o => o.Name` (`tests/RackCad.Tests/SelectiveBindingIntentTests.cs:393-409`).
- **FACT** — El código lo declara: «Nothing here ever supports the reverse lookup name → id»
  (`LinkedPropertyOptions.cs:13-14`).
- **FACT** — Barrido de todos los puntos de producción que tocan el nombre de una variable (modelo, store,
  acreditación, opciones, sesión, control, workspace, ventana, intents, preflight, `ApplyTo`, apertura de
  RACKEDITAR, comandos del Plugin y export): **ninguno resuelve nombre→id**. Las únicas búsquedas van de id a
  nombre, para mostrar (`SelectiveEditorOpen.cs:145-155`; `LinkedPropertyEditSession.cs:388-398`).
- **FACT** — Orden: los candidatos siguen el orden de `Targets()`, **por id** y no alfabético
  (`UsableProjectVariablesRegistry.cs:237-243`; `LinkedPropertyEditSession.cs:171-172`).
- **FACT** — Comparadores vigentes:

  | Valor | Comparador | Evidencia |
  |---|---|---|
  | Nombre (filtro) | OrdinalIgnoreCase, *contains* | `LinkedPropertyEditSession.cs:196` |
  | `VariableId` | OrdinalIgnoreCase | `VariableId.cs:78-83` |
  | `PropertyId` | Ordinal | `PropertyId.cs:67,71` |
  | Token de tipo | OrdinalIgnoreCase | `VariableType.cs:89` |
  | Kind de definición `literal` | OrdinalIgnoreCase | `ProjectVariablesStore.cs:193` |
  | Kind de binding `projectVariable` | Ordinal | `SelectiveLinkedPropertyKernel.cs:189-190` |
  | Agrupación por `RackId` | OrdinalIgnoreCase | `ProjectVariableConsumerDiscovery.cs:174,232` |

- **FACT** — No hay normalización Unicode ni de acentos: los únicos `Trim` del área están en
  `LinkedPropertyEditSession.cs:183,432` y `VariableId.cs:62`.
- **INFERENCE (sin test)** — `IsQuery` recorta espacios **iniciales** antes de buscar `=`, pero `Candidates` toma
  `_text.Substring(1)` sin recortar: `" =Hol"` es consulta con filtro `"=Hol"` y no coincide con nada.
- **INFERENCE** — El desambiguador es un **prefijo** de 8 caracteres del texto del id tal como se guardó
  (`VariableId.cs:62`): dos ids con el mismo primer grupo hexadecimal se verían iguales, y un id persistido en otro
  formato de GUID daría otro fragmento.

### 13.4 Impacto en fórmulas futuras

- **INFERENCE** — Si una fórmula se escribe con nombres, comprometerla exigiría resolver nombre→id: sería la
  **primera** ruta productiva de ese tipo, y con homónimas legales esa resolución puede ser ambigua.
- **INFERENCE** — Un filtro *contains* sin distinguir mayúsculas no sirve como regla de coincidencia exacta de
  un token dentro de una fórmula.
- **INFERENCE** — Si la fórmula persiste ids, un rename solo cambia el texto mostrado (§8.2); si persiste
  nombres, rename deja de ser registry-only.
- **PROPOSAL INPUT** — Política de nombres para expresiones (unicidad, mayúsculas, caracteres admitidos,
  espacios como en «Holgura General»), sintaxis de desambiguación y comportamiento ante homónimas.

---

## 14. Tests existentes de I-47 e I-48

### 14.1 Marco

- **FACT** — xUnit 2.9.2, solo `[Fact]` y `[Theory]`/`[InlineData]`, sin librerías de mocks ni de aserciones
  (`tests/RackCad.Tests/RackCad.Tests.csproj`, `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj`).
- **FACT** — `RackCad.Tests` no referencia el Plugin: el Plugin solo se verifica con **guardas de texto**. El único
  `InternalsVisibleTo` es `RackCad.Tests` (`src/RackCad.Application/RackCad.Application.csproj:11`).
- **FACT** — Alcance medido por conteo de atributos: ≈27 archivos Core con ≈599 métodos y 6 archivos UI con 68
  métodos. No hay builder compartido de documentos: ≈25 clases re-declaran `Registro`/`Doc`/`Diseno` con los
  mismos GUID canónicos.

### 14.2 Inventario por área

| Área | Archivos (≈métodos) | Contrato que protege (FACT) | Reutilizable por I-49 (FACT) | Escenario ID22B que falta (INFERENCE) |
|---|---|---|---|---|
| **Modelo** | `ProjectVariablesModelTests` (26) | Identidad por id y no por nombre (`:39,:52`); `VariableId` OrdinalIgnoreCase y `PropertyId` Ordinal (`:67,:90`); acceso al brazo equivocado lanza (`:163,:171`); estados inválidos rechazados, incluido `(VariableType)0` (`:227`) | `SeisPulgadas()`, `Holgura(id, name)` (`:31-34`) | Definición no literal; extracción de ids referidos; política de nombres |
| **Persistencia / store** | `ProjectVariablesDocumentTests` (18), `ProjectVariablesRegistryAccessTests` (14), `PersistedVariableTypeGrammarTests` (15), `SelectiveAuthoredBinding(Presence)Tests` (21+13), `SelectiveAuthoredCarrierAdoptionTests` (12) | Versión ausente ≠ `"1.0"`; major 2 incompatible; minor preservado; `ExtensionData` de raíz; `Kind` `"expression"` y tipo `"Volumen"` son error duro (`ProjectVariablesDocumentTests.cs:46-233`); troceado de 255 caracteres; gramática de tipo por nombre; promoción *sticky* y presencia ≠ interpretación | `Json(...)` + `UnaVariable(type, kind)` parametrizables (`ProjectVariablesDocumentTests.cs:36-41`); `Entry`/`Document`/`RoundTrip` sobre el store real (`PersistedVariableTypeGrammarTests.cs:39-60`); `Trocear` (`ProjectVariablesRegistryAccessTests.cs:202-212`) | Round-trip de una definición no literal; `ExtensionData` por entrada y por definición (FACT: sin test); qué versión exige un kind nuevo |
| **Acreditación** | `LinkedPropertyFoundationTests` (21), `RegistryCommitAccreditationTests` (12), `LinkedPropertyKernelTests` (31, parte) | `Accredit` Absent/Usable/AmbiguousIdentity/NotReadable (`LinkedPropertyFoundationTests.cs:163-235`); snapshot con tipo no soportado construible (`:267`); cinco resultados de `InspectBinding` (`:281-364`); `RegistryCommit` bloquea re-lecturas no acreditadas y demuestra que `ApplyTo` crudo borraría duplicados (`RegistryCommitAccreditationTests.cs:65-202`) | `Target(...)`, `Registry(...)` vía `FromTargets`, `TipoSintetico = (VariableType)9001`, descriptores sintéticos `Alpha()`/`Beta()` (`LinkedPropertyKernelTests.cs:39-95`) | Resultado de acreditación para ciclo, dependencia colgante o fallo de evaluación; deriva de dependencias entre plan y commit |
| **Preflight de mutación** | `ProjectVariableMutationPreflightTests` (46), `ProjectVariableMutationExecutorTests` (28) | Todo o nada (`AssertPlanVacio`, `:114-120`); registry-only para Create/Rename (`:125-158`); una mutación por rack con todas las vistas (`:190,:631,:653`); `MutationDestinationBinding` exacto; ejecutor con un Commit, una transacción y un Regen (guardas `:226,:313`) | `Doc`, `DocDos`, `Vista`, `Registro(params (Id, Valor)[])` (`ProjectVariableMutationPreflightTests.cs:35-120`); `Bind(destinos, presentes)` | Delete bloqueado por dependientes variable→variable; propagación transitiva; plan con más de una variable |
| **Descubrimiento de consumidores** | `ProjectVariableDiscoveryTests` (39), `ProjectVariableScanProjectionTests` (13) | Sonda tri-estado donde lo desconocido nunca es negativo (`:118-156`); familias A/B (`:305-438`); sobre ininterpretable aborta (`:375,:459`); proyección sin identidad inventada | `Doc(...)`, `A(variableId)`, `Vista` (`ProjectVariableDiscoveryTests.cs:58-93`); `Sobre(kind, id, design)` | Descubrimiento variable→variable; cierre transitivo; clasificación de un kind nuevo de valor de propiedad |
| **Resolver efectivo** | `SelectiveEffectiveDesignResolverTests` (20), `SelectiveEditorOpenTests` (33) + subconjuntos de Kernel y Discovery | Sin binding gana el literal; con binding, la variable; fallos tipados sin fallback (`:109-233`); precedencia de `ClearOverride` con geometría real (`:251`); Domain sin conocer variables por reflexión (`:298-308`); apertura rechazada ante roto, ilegible o duplicado | `Vinculado(...)`, `Registro(...)`, costura `ResolveWith(authored, registry, descriptorSet)` | Resolver a través de una definición evaluada; error de evaluación; determinismo de orden |
| **Operaciones** | `SelectiveBindingIntentTests` (22) + Preflight, Workspace y pruebas reales | Link congela, Unlink materializa, roto no se desvincula, Delete bloqueado, UnlinkAllAndDelete todo o nada, Create acuña id, Rename conserva id, ChangeValue planifica todos los consumidores (tabla de pruebas en `ProjectVariableMutationPreflightTests.cs:125-530`) | `Correr`, `Vincular`, `Variable`, `Registro` (`SelectiveBindingIntentTests.cs:77-104`) | Link a una variable derivada; Delete de una variable usada en fórmulas; transición literal↔expresión |
| **Workspace / RACKVARIABLES** | `ProjectVariablesWorkspaceTests` (46), UI `ProjectVariablesWindowTests` (21) | Proyección editable/bloqueada; reparación rack-scoped; intents por id con `double`; la ventana no muta su modelo; confirmaciones (`ProjectVariablesWorkspaceTests.cs:118-679`; `ProjectVariablesWindowTests.cs:159-561`) | `Doc`, `Dos`, `Vista`, `Registro`, `Abrir`, `HermanasDivergentes`; UI `Vacio()`, `ConUnaVariable`, `ConUnaRota`, `Control<T>` | Texto de definición, error de parseo, valor evaluado frente a fuente, lista de dependientes, diagnóstico de ciclo |
| **Editor vinculable** | `LinkedPropertyEditSessionTests` (28), `LinkedPropertyReconcilerTests` (16), `LinkedPropertyViewPayloadTests` (6), UI `LinkedPropertyEditorControlTests` (8), `SelectiveEditorBindingTests` (7), `SelectiveBindingUiTests` (14) | Regla 20.13 (`LinkedPropertyEditSessionTests.cs:55-89`); LostFocus nunca cambia fuente (`:134`); `=` filtra y **nunca** resuelve, ni con nombre exacto ni con candidato único (`CASO_6` Theory `:212`, `CASO_7` `:233`, `CASO_8` `:250`); transiciones del reconciliador `CASO_17..21` (`LinkedPropertyReconcilerTests.cs:101-195`) | `Option`, `DosOpciones`, `Literal`, `Vinculada` (`LinkedPropertyEditSessionTests.cs:38-50`); UI `Editor(committed)` y costuras `SelectFromList`, `MoveSelectionForTest`, `CommitByEnterForTest` | `LinkedPropertyOptions.ForProperty` sin test Core directo (FACT: solo como preparación en UI); una sintaxis `=expr` choca con los comportamientos fijados |
| **C4 y multipropiedad** | `MultiPropertyRealProofTests` (29), `PalletToleranceRealProofTests` (16), UI `MultiPropertySessionTests` (8), `PalletToleranceEditorTests` (10) | `TryStage`/`ApplyStaged` sin mutar; dos borradores que cambian fuente bloquean y el estado nombra ambos (`MultiPropertySessionTests.cs:303-376`); cada token gobierna su campo; una variable que gobierna dos propiedades da una sola mutación; efecto real en geometría y BOM (`MultiPropertyRealProofTests.cs:888-940`) | `DisenoReal`/`AuthoredReal` sobre catálogo real; `Registro(x, y, z)`; `Firma(bom)`; UI `Con<T>(authored, read, body)` | Valor mostrado que depende de la definición de otra variable; cadenas variable→variable→propiedad hasta geometría y BOM |
| **VariableId duplicado** | ≈12 tests repartidos | El store lo acepta, la identidad lo rechaza y el commit bloquea antes de `ApplyTo` (`PersistedVariableTypeGrammarTests.cs:141`; `LinkedPropertyKernelTests.cs:555`; `RegistryCommitAccreditationTests.cs:79-152`; `MultiPropertyRealProofTests.cs:1052`) | Fixtures `Ambiguo()` | Una definición que referencia un id ambiguo (FACT: tampoco hay test de duplicado en BOM, export ni opciones) |
| **Nombres duplicados** | ≈17 tests repartidos | Etiquetas, homónimas coexisten, desambiguador visible, nada resuelve por nombre (`LinkedPropertyEditSessionTests.cs:357`; `SelectiveBindingIntentTests.cs:272-299,393-409`) | — | Referencias por nombre dentro de definiciones; rename propagado a definiciones (FACT: no hay política de unicidad ni de mayúsculas) |
| **Save / reload** | Registro, documento Selectivo, UI y guardas del Plugin | Round-trips del registro y del portador authored; guardado desde ventana y reconciliador. **FACT**: ningún test lee `ProjectVariablesData.cs` ni `ProjectVariablesRegistry.cs`; NOD y Xrecord solo por `ProjectVariablesPayload` y `Trocear` | Round-trip real del store | Registro con un kind nuevo escrito por un minor posterior |
| **BOM / efectivo** | `SelectiveBomAuthorityTests` (27) + pruebas reales | Autoridad de cotización multi-vista; se cotiza el efectivo (30 frente a 6, `:267`); `BomBuildResult` tipado; RACKBOMTOTAL lee el registro una vez y no resuelve por su cuenta (guardas `:386-480`) | `Diseno(clearance)`, `Doc(...)`, `Bom(authored, registry)`, `Firma(bom)` | BOM de racks ligados a variables derivadas; aborto por fallo de evaluación |
| **Duplicación / export** | `SelectiveDuplicationFailClosedTests` (17), `SelectiveLibraryExportTests` (21) | La copia conserva el mismo `VariableId` (`SelectiveDuplicationFailClosedTests.cs:111`); literal, versión y `ExtensionData` preservados; el export materializa, quita el binding y vuelve a `"1.0"`; sin fallback al literal (`SelectiveLibraryExportTests.cs:92-206`) | `Json`, `Copia`; `Authored`, `Ref`, `Archivo` | Exportar un valor derivado; fallo de evaluación al exportar |

### 14.3 Guardas de fuente y pines de comportamiento que un motor de expresiones tocaría

**FACT** — Mecánica común: `Sources(project)` barre `*.cs` de `src/<proyecto>` salvo `obj/` y `bin/`; `Code(path)`
solo quita líneas que empiezan por `//`, así que comentarios de bloque, comentarios finales y literales de
cadena **sí** cuentan; `AssertAbsentFrom` compara por **substring** Ordinal
(`tests/RackCad.Tests/ProjectVariablesConformanceTests.cs:39-71`). Los `.xaml` no se barren.

| ID | Guarda o pin | Aserción exacta | Qué la haría fallar | Clase |
|---|---|---|---|---|
| G1 | `ProjectVariablesConformanceTests.NO_HAY_FORMULAS_NI_REFERENCIAS_A_PROPIEDADES_DE_OTROS_RACKS` (`:125-137`) | Ausentes `ExpressionParser`, `FormulaParser`, `DependencyGraph`, `RackPropertyReference`, `rackProperty` en Application, Plugin y UI | Cualquier identificador o cadena que los **contenga** (p. ej. `VariableExpressionParser`, `DependencyGraphBuilder`); `ExpressionEvaluator` o `Ast` no coinciden | FACT |
| G2 | `...LA_DEFINICION_DE_UNA_VARIABLE_SIGUE_TENIENDO_UN_SOLO_CASO` (`:141-149`) | `VariableDefinition.cs` contiene `Literal = 1` y no contiene `Expression = ` ni `Formula = ` (archivo entero, con comentarios) | Añadir `Expression = …` o `Formula = …`, renumerar `Literal` o mover el archivo | FACT |
| G3 | `ProjectVariablesDocumentTests.Prueba1_DefinitionKindDesconocido_ES_ERROR_DURO` (`:152-160`) | `"Kind":"expression"` → `PresentButUnreadable` | Que el store acepte `expression` | FACT |
| G4 | Domain sin variables (`ProjectVariablesConformanceTests.cs:112-115`; `SelectiveEditorOpenTests.cs:550-557`; `SelectiveEffectiveDesignResolverTests.cs:298-308`) | Ausentes `ProjectVariables`, `VariableId`, `PropertyValues` en Domain; `SelectivePalletDesign` sin propiedades con «Variable», «PropertyValues» o «SchemaVersion» | Un motor en Domain que nombre esos términos | FACT |
| G5 | `ProjectVariableMutationExecutorTests.LA_CAPA_PURA_SIGUE_SIN_CONOCER_EL_DIBUJO` (`:347-362`) | Ausentes `ObjectId`, `Autodesk.AutoCAD` en `src/RackCad.Application/ProjectVariables/*.cs` (solo nivel superior) | Tipos de AutoCAD en esa carpeta | FACT |
| G6–G10 | `PersistedVariableTypeGrammarTests` (`:74-97,255-316`), `ProjectVariablesDocumentTests` (`:142-150`), `ProjectVariablesModelTests` (`:227-231`) | Rechazo de `"Dimension"`, `"9001"`, `"Volumen"`, `(VariableType)0`, formas numéricas y listas | Un segundo `VariableType` con esos nombres o valores | FACT |
| G11 | `SIGUE_HABIENDO_UN_SOLO_RESOLVEDOR_EFECTIVO` (`ProjectVariablesConformanceTests.cs:226-234`; `SelectiveEditorOpenTests.cs:484-492`) | Una sola declaración de `class SelectiveEffectiveDesignResolver` | Un segundo resolver | FACT |
| G12 | `SelectiveBomAuthorityTests` (`:386-480`) | `new SelectiveEffectiveDesignResolver()` exactamente una vez en `SelectiveKindHandler.cs`; RACKBOMTOTAL sin resolver ni `ToProjectVariables` y con `ProjectVariablesRegistry.Read` una sola vez | Evaluar en el comando o en otro handler | FACT |
| G13 | `ProjectVariableMutationExecutorTests` (`:211-338`); `RegistryCommitAccreditationTests` (`:207-233`) | El ejecutor no contiene `ProjectVariableMutationPreflight`, `ProjectVariableConsumerDiscovery`, `SelectiveEffectiveDesignResolver`, `ApplyTo(`, `lastRead.Document`, `Editor.Regen()`; contiene **una vez** `Commit()`, `StartTransaction()`, `ApplyRegen`; `ApplyTo` sigue `internal` | Evaluar o re-planificar dentro del ejecutor | FACT |
| G14 | `ProjectVariablesWorkspaceTests` (`:700-779`) | `RackVariablesCommands.cs` contiene lectura, barrido, proyección, `ProjectVariableIntentPreflight.Run` y `Execute`, y no contiene `ProjectVariableConsumerDiscovery`, `RegistryMutation.`, `MutationPlan.Of` ni el resolver | Mover semántica al comando | FACT |
| G15 | `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA` (`:540-547`) | Exactamente **33** `[CommandMethod(` en el Plugin | Cualquier comando nuevo | FACT |
| G16 | `WindowCensusGuardTests` (`tests/RackCad.UI.Tests/WindowCensusGuardTests.cs:89-172`) | Censo de **29** ventanas | Cualquier ventana nueva | FACT |
| G17 | Catálogo cerrado (`LinkedPropertyFoundationTests.cs:147-158`; Kernel `:626-634`; Multi `:223-236`; PalletTolerance `:157-166`) | Exactamente `{selective.palletTolerance, selective.verticalClearance}` | Una tercera propiedad vinculable | FACT |
| G18 | Propiedad del mapa de vínculos (`ProjectVariablesConformanceTests.cs:158-164,180-216`; `SelectiveEditorOpenTests.cs:470-532`; `SelectiveBindingIntentTests.cs:377-386`; `SelectiveLibraryExportTests.cs:321-328`) | Plugin y UI sin `PropertyValues` ni `SelectivePropertyValueDocument`; `RackSelectivoCommands.cs` y `RackSelectiveWindow.xaml.cs` sin `VariableId`, `ToProjectVariables` ni resolver | Que el Plugin o la UI escriban o interpreten vínculos o fórmulas | FACT |
| G19 | `SelectiveBindingIntentTests.GUARDA_LA_VENTANA_VINCULA_POR_IDENTIDAD_Y_NO_POR_NOMBRE` (`:393-409`) | `LinkedPropertyEditor.cs` contiene `session.TrySelect(option.VariableId` y no contiene `Name ==` ni `FirstOrDefault(o => o.Name` | Resolver tokens por nombre dentro del control | FACT |
| G20 | Pines de comportamiento del `=` (`LinkedPropertyEditSessionTests.cs:118-129,212-262,452-461`; UI `LinkedPropertyEditorControlTests.cs:59,108,130`; `SelectiveBindingUiTests.cs:376-392`) | `"="`, `"=H"`, `"=Holg"`, `"=Holgura General"` → `DraftReferenceQuery`; `"abc"` → `InvalidDraft`; nunca auto-selección | Dar otro significado a `=` en el editor | FACT |

**Helpers globales (FACT):** `StaTestRunner.Run` (hilo STA compartido que sustituye `EditorDiscardPrompt`,
`tests/RackCad.UI.Tests/StaTestRunner.cs:57,78-109`), `EditorWindowTestSupport` (`ClickNamed`, `SetText`,
`Descendants` con slots del shell), `SelectiveWindowTestSupport.Open` (único constructor permitido de la ventana
Selectiva), `TestCatalogIds` y `JsonRackCatalogProvider.FromBaseDirectory().Load()`.

**Hallazgos del barrido de tests (FACT):** varios comentarios de tests siguen describiendo una sola propiedad
vinculable (p. ej. `ProjectVariablesWorkspaceTests.cs:72-73`), y `SelectiveBindingUiTests.cs:23-24` dice que la
caja vinculada queda deshabilitada mientras `:230` afirma que es editable. Solo documentación de tests.

---

## 15. Búsquedas negativas

Ámbitos: `src/`, `tests/`, `docs/adr/` y listas `PackageReference` de todos los `*.csproj`. Búsquedas con ripgrep; los
aciertos se clasifican como RELACIONADO o NO RELACIONADO.

| # | Qué se buscó | Términos principales | Resultado | Aciertos relacionados | Clase |
|---|---|---|---|---|---|
| 1 | Parser de expresiones | `tokeni[sz]`, `lexer`, `shunting`, `precedence`, `recursive descent`, `\bparser\b`; tipos `*Expression*`, `*Parser*`, `*Tokenizer*`, `*Lexer*` | **0 parsers de expresiones.** El único lexer es de CSV (`src/RackCad.Application/Catalogs/CsvLexer.cs`). `SelectiveFondoTargets` parsea una notación de fondos (`1+3`, `2-4`) y tiene 0 llamadores en `src/` | Solo comentarios que declaran la ausencia (`VariableDefinition.cs:16-19`; `VariableType.cs:9-12`) y la guarda G1 | FACT |
| 2 | AST | `\bAST\b`, `SyntaxNode`, `BinaryExpression`, `UnaryExpression`, `\w+Node\b` | **0** en `src/` salvo el comentario «no AST» (`VariableDefinition.cs:19`); en tests, solo `JsonNode` y nodos WPF | — | FACT |
| 3 | Evaluador | `Evaluat*`, `Eval`, `System.Linq.Expressions`, `DataTable.Compute`, `NCalc`, `Jace`, `Roslyn`, `Microsoft.CodeAnalysis`, `Scripting` | **0** librerías y **0** evaluadores. Los `Evaluate` existentes son diagnósticos de Push Back (`PushBackCompositeDiagnostics.cs:94,142`) | ADR-0034 `:183` («exige ciclos, orden de evaluación y tipado de operandos») | FACT |
| 4 | Grafo de dependencias | `graph`, `grafo`, `dependenc*`, `topological`, `toposort`, `adjacency`, `kahn` | **0** grafos y **0** ordenaciones topológicas. `Dependency*` es WPF; «topología» es topología física de racks | Guarda G1 prohíbe `DependencyGraph`; ADR-0034 `:182` | FACT |
| 5 | Detección genérica de ciclos | `cycl*`, `circular`, `visited`, `DFS`, `Tarjan`, `strongly connected`, `SCC`, `onStack` | **0**. Los `seen` son deduplicaciones; «cycle» es ciclo de recálculo o de vida | — | FACT |
| 6 | Registro de funciones | `FunctionRegistry`, `IFunction`, `BuiltIn`, `"MIN"`, `"MAX"`, `"ROUND"`, `"IF"` | **0**. Los registros existentes son de sistemas, handlers, módulos y variables | — | FACT |
| 7 | `SymbolId` | Literal en todo el worktree | **0** | — | FACT |
| 8 | `ExpressionContext` | Literal en todo el worktree | **0** | — | FACT |
| 9 | `SymbolResolver` | Literal en todo el worktree | **0** (los «Symbol» existentes son `SymbolUtilityServices` de AutoCAD) | — | FACT |
| 10 | Fórmulas persistidas | `"expression"`, `"formula"`, propiedades DTO `*Expression*`/`*Formula*`, `JsonPropertyName`, `assets/` | **0 DTO y 0 datos.** Único kind `"literal"` (`ProjectVariablesStore.cs:29`); `"expression"` solo existe como fixture que debe **fallar** (`ProjectVariablesDocumentTests.cs:152-160`) | Kinds de binding desconocidos (`rackProperty`, `futureKind`) como fixtures fail-closed | FACT |
| 11 | Nombres como identidad semántica | Comparaciones, claves, búsquedas o resoluciones sobre `Name` | **0 rutas de producción nombre→id** (§13.3). Fuera de Project Variables hay búsquedas por nombre no relacionadas (presets de cabecera dinámica, RACKLISTA ordenada por nombre, definiciones de bloque por nombre) | Guarda G19 | FACT |
| 12 | Parser neutral de unidades | `"mm"`, `"in"`, `"ft"`, pies-pulgadas, `25.4`, `UnitConver*`, `ToMillimet*`, sufijos | **0**. Solo los dos `TryParseDimension` privados que quitan `in` y `"` (§11.7) | — | FACT |

**Contexto normativo de las búsquedas (FACT, doc)** — ADR-0012, aceptado: «No agregar otra dependencia NuGet al código
de producto sin acuerdo explícito del propietario y un ADR» (`docs/adr/0012-producto-sin-dependencias-nuget.md:3,44`). El
único `PackageReference` de producto es `AutoCAD.NET`, condicional y solo de compilación
(`src/RackCad.Plugin/RackCad.Plugin.csproj:12`).

---

## 16. Extension seams para I-49

Las ubicaciones son **FACT**. Que sean «el» seam es **INFERENCE**. Cómo se usan es **PROPOSAL INPUT**.

### 16.1 Catálogo de seams

| # | Seam | Ubicación | Qué ofrece hoy | Qué lo limita |
|---|---|---|---|---|
| S1 | Discriminador de definición (modelo) | `VariableDefinition.cs:6-75` | Enum de un caso y clase con igualdad por valor | Payload `double`; guarda G2 |
| S2 | Discriminador de definición (persistencia) | `ProjectVariablesDocument.cs:139-148`; `ProjectVariablesStore.cs:157-208` | `Kind` string + `ExtensionData` + política de versión con minor preservado | El store rechaza todo lo que no sea `literal`; test G3 |
| S3 | Acreditación → lookup | `UsableProjectVariablesRegistry.cs:120-243`; `VariableTargetSnapshot.cs` | Único lugar donde el documento se convierte en targets consultables | Targets con forma de valor literal |
| S4 | Valor → diseño | `SelectiveEffectiveDesignResolver.cs:110-164` | Único punto donde un vínculo se vuelve número; guardas G11/G12 | Escribe `Target.LiteralValue`; solo Selectivo |
| S5 | Composición del plan | `ProjectVariableMutationPreflight.cs:121-298` | Resolución contra el registro «después» y todo o nada | Descubrimiento de UNA variable (profundidad 1) |
| S6 | Descubrimiento | `ProjectVariableConsumerDiscovery.cs:100-259` | Barrido puro, tri-estado y autoridad multi-vista | Solo propiedad→variable y solo Selectivo |
| S7 | Forma del plan | `MutationPlan.cs:148-212` | N `RackMutation`, una por rack, con efectivo completo | Una sola `RegistryMutation` |
| S8 | Commit | `RegistryCommit.cs:89-107`; ejecutor `:250-289` | Re-lectura acreditada antes de aplicar; un commit | La «semantic-usability» solo cubre identidad única |
| S9 | Fuente de edición | `LinkedPropertySource.cs`, `LinkedPropertyEditState.cs`, `LinkedPropertyEditSession.cs:120-138,431-432`, `LinkedPropertyReconciler.cs:193-220` | Enum pensado para un tercer caso; estado final sin historia; reconciliador genérico por descriptor | `=` ya significa consulta (G20) |
| S10 | Superficie central | `ProjectVariablesWorkspace.cs`, `ProjectVariableIntent.cs`, `RackProjectVariablesWindow.xaml(.cs)` | Intents por id, re-lectura en cada vuelta | Valor `double`, `Length`, consumidores directos |
| S11 | Guardas | `ProjectVariablesConformanceTests.cs:125-149` y G11–G20 (§14.3) | Fijan ausencias y conteos | Tienen que evolucionar de forma explícita |

### 16.2 ID20 — preparar `SymbolId`, scopes, `ExpressionContext` y resolución (OWNER INPUT)

**Qué existe hoy (FACT):**

- No existe `SymbolId`, `ExpressionContext`, `SymbolResolver`, scope ni namespace de símbolos (§15).
- Identidades disponibles: `VariableId` (GUID, OrdinalIgnoreCase, `VariableId.cs:30-90`), `PropertyId`
  (token cualificado por sistema, Ordinal, `PropertyId.cs:22-78`; `ProjectPropertyIds.cs:20,35`) y el `RackId` del
  sobre y del documento authored, un string GUID (`ProjectVariableScanEntry.cs:47-48`;
  `SelectivePalletDesignDocument.cs:49-50`).
- El registro acreditado se indexa **solo por `VariableId`**, sin namespace (`UsableProjectVariablesRegistry.cs:97`).
- Los datos de proyecto (todos los racks) solo existen tras un **barrido del Plugin**; su proyección pura es
  `ProjectVariableScanEntry`, que solo tiene documento authored para los Selectivos
  (`ProjectVariableScanEntry.cs:66-104`).

**Dónde viven hoy los datos de los parámetros de ejemplo (FACT; no se implementa ninguno):**

| Parámetro (OWNER INPUT) | Fuente actual | Capa y pureza | Observación |
|---|---|---|---|
| `Rack.Frentes` (Selectivo) | `Bays.Count` **por fondo** en el diseño (`src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs:69,79`); resuelto en `SelectiveRackSystem.Bays`/`FondoBays` (`src/RackCad.Domain/Systems/Selective/SelectiveRackSystem.cs:41,50`) | Domain/Application, puro | Cada fondo tiene su propio conteo; la retícula de postes la fija el fondo más largo (`SelectiveGeometryResolver.cs:120-144`). «Frentes del rack» admite al menos dos lecturas |
| `Rack.Niveles` (Selectivo) | Celdas de diseño por frente (`SelectiveTopology.LevelCount(fondo, frente)`) **o** niveles de larguero resueltos (`SelectiveBay.Levels`) | Application/Domain, puro | Sin larguero de piso, el nivel 0 de diseño es tarima de piso, así que niveles de larguero = celdas − 1 (`SelectiveGeometryResolver.cs:275-319`); varía por frente y por fondo |
| `Rack.Altura` (Selectivo) | `SelectiveRackSystem.Height` = altura de la bahía más alta de **todos** los fondos (`SelectiveRackSystem.cs:16-17`; `SelectiveGeometryResolver.cs:176-182`) | Application, puro pero **requiere `RackCatalog`** (`SelectiveGeometryResolver.cs:33`) | Redondeada al pie (`SelectiveGeometryResolver.cs:31,422`); una cabecera personalizada no entra en esa altura |
| Equivalentes en otros sistemas | Dinámico: `DynamicRackDesign.Fronts`, `DynamicFrontActivation.EffectiveLoadLevels`, `DynamicFrontGeometry.Height`; Push Back: estructura dinámica + lado B; Cama: sin frentes, niveles ni altura; Cantilever: `StationCount`, `LevelCount`, `ColumnHeight` | Application, puros; Cantilever requiere el catálogo de secciones | Semánticas distintas por sistema |
| `Project.TotalRacks` / `Project.TotalFrentes` | **No existe** ningún agregado de frentes, niveles ni altura sobre todo el dibujo. Los únicos números globales son `RackCount` y `TotalCopies` del BOM consolidado (`src/RackCad.Application/Bom/ConsolidatedBom.cs:40-41`) y la columna de copias de RACKLISTA | El barrido es del **Plugin** (`src/RackCad.Plugin/RackBlockFinder.cs:57-91`); lo que sigue es puro | **INFERENCE**: `RackCount` cuenta solo racks que produjeron BOM, no racks colocados (`ConsolidatedBom.cs:50-52`; `RackInventarioCommands.BomTotal.cs:153-157,209-215`) |

**Acoplamientos que dificultarían registrar `Rack.*` / `Project.*` más adelante (INFERENCE):**

- Resolución, descriptores y consumidores atados al Selectivo (S4, S6; §4.4).
- Registro indexado solo por `VariableId`, sin espacio para símbolos de otra naturaleza.
- Datos de proyecto disponibles solo tras un barrido del dibujo, no en Application a demanda.
- `VariableTargetSnapshot` con forma de valor literal (S3), sin lugar para un valor calculado ni para un símbolo
  que no sea variable.

### 16.3 ID23 — reutilizar parser, evaluador y contexto sin depender de ProjectVariables (OWNER INPUT)

**Qué existe hoy (FACT):**

- No hay parser, evaluador ni contexto que reutilizar (§15).
- Todo el vocabulario de valor vive en `RackCad.Application.ProjectVariables` y `…Systems.Selective`
  (§3.1–§3.2).
- El BOM del Selectivo se calcula desde la geometría resuelta: `SelectiveGeometryResolver.Resolve` →
  `SelectiveBomBuilder.Build(system, catalog)` (`src/RackCad.Plugin/KindHandlers/SelectiveKindHandler.cs:68-69`).
- **FACT** — Modelo de BOM vigente: `BomLine` y `BomComponent` con `Quantity` **entero** (`src/RackCad.Application/Bom/BillOfMaterials.cs:8-32`);
  `BillOfMaterials` con componentes y líneas aplanadas; resultado tipado `BomBuildResult` (`src/RackCad.Application/Bom/BomBuildResult.cs:4-89`);
  consolidación × copias (`ConsolidatedBom.cs:48-85`).
- **FACT** — Cada cantidad del Selectivo sale de un constructor o plan de Application (cabeceras, largueros ×2,
  desviadores, separadores, topes y parrillas) sobre la geometría resuelta, no de entidades del DWG
  (`src/RackCad.Application/Systems/Selective/SelectiveBomBuilder.cs:88-534`); los elementos de seguridad sin regla de
  dibujo usan la cantidad manual de la selección (`SelectiveBomBuilder.cs:138-140`).
- **FACT** — No hay línea de BOM virtual, ni cantidad derivada de una fórmula, ni punto de extensión para
  añadirlas.
- **INFERENCE** — Un parser, evaluador o contexto declarado dentro de `RackCad.Application.ProjectVariables`
  arrastraría esa dependencia a ID23, contra la entrada del Owner.
- **PROPOSAL INPUT** — Namespace o proyecto del motor; qué interfaces mínimas ve ProjectVariables y cuáles
  vería ID23.

### 16.4 ID28 / ID29 — información que existe y que habría que preservar

| Necesidad | Existe hoy | Se pierde hoy | Clase |
|---|---|---|---|
| **Explain** de un valor efectivo | `BindingInspection` (token, id crudo, target con id, tipo, valor y nombre, detalle; `BindingInspection.cs:55-141`); `SelectiveEffectiveResolution` (resultado, `PropertyId`, `VariableId`, error; `SelectiveEffectiveResolution.cs:42-74`); texto de estado de la ventana (`RackSelectiveWindow.xaml.cs:2519-2540`) | La **procedencia** del número: el resolver lo escribe en el diseño de Domain y la inspección se descarta (`SelectiveEffectiveDesignResolver.cs:145-163`) | FACT |
| **Explain** de una expresión | Nada: no hay evaluación | — | FACT |
| **Impact** de un cambio | `DiscoverConsumers`, `Summarize` (rack + propiedades), `PropertiesBoundTo`; el `MutationPlan` completo **antes** de ejecutar (racks, vistas, authored y efectivo) (`ProjectVariableMutationPreflight.cs:121-167`; `MutationPlan.cs:148-212`); `MutationExecutionResult` después (`ProjectVariableMutationExecutor.cs:29-63`) | Nada persiste: todo se calcula bajo demanda desde un barrido | FACT |
| **Impact** transitivo | Nada: no hay aristas variable→variable ni índice inverso | — | FACT |
| Qué conservar para no rehacer el motor | Árbol evaluado, símbolos resueltos por id, valores intermedios, dependencias directas e inversas, causa tipada del fallo | — | INFERENCE / PROPOSAL INPUT |

---

## 17. Riesgos

| # | Riesgo | Evidencia | Clase |
|---|---|---|---|
| R1 | **Legibilidad en builds anteriores.** Una sola entrada no literal hace ilegible el registro entero para un build I-47/I-48, y con él quedan bloqueados RACKEDITAR de todo Selectivo, RACKVARIABLES y RACKBOMTOTAL de ese DWG | `ProjectVariablesStore.cs:193-197`; `SelectiveEditorOpen.cs:172-182`; `ProjectVariablesWorkspace.cs:260-266`; `RackInventarioCommands.BomTotal.cs:60-67` | FACT (comportamiento) / INFERENCE (impacto de uso) |
| R2 | **Segunda autoridad de valor.** El target acreditado es literal y el efectivo sale de un único resolver; evaluar fuera de ese camino crearía dos respuestas para el mismo número, justo lo que la guarda de resolver único prohíbe | `VariableTargetSnapshot.cs:29-49`; `SelectiveEffectiveDesignResolver.cs:160`; `ProjectVariablesConformanceTests.cs:226-234` | FACT / INFERENCE |
| R3 | **Guardas de fuente.** `ExpressionParser`, `FormulaParser`, `DependencyGraph` y `"Expression = "` hoy hacen fallar la suite. Esquivarlas renombrando tipos vaciaría su intención; las que protegen ID21 (`RackPropertyReference`, `rackProperty`) siguen siendo alcance prohibido | `ProjectVariablesConformanceTests.cs:125-149` | FACT |
| R4 | **Profundidad 1 embebida** en descubrimiento, plan, Delete y RACKVARIABLES. Una propagación transitiva sin deduplicar por rack chocaría con `MutationDestinationBinding` («nombra dos veces») o redibujaría a medias | §6.2; `MutationDestinationBinding.cs:85-91` | FACT / INFERENCE |
| R5 | **Coste sin medir.** RACKVARIABLES hace un descubrimiento por variable al abrir; el ejecutor, un barrido del dibujo por rack; ADR-0034 deja la propagación en dibujos grandes «sin medir» | `ProjectVariablesWorkspace.cs:300-316`; `ProjectVariableMutationExecutor.cs:147`; `RackCommandSupport.cs:109-134`; ADR-0034 `:212-213` | FACT / INFERENCE |
| R6 | **Nombres en fórmulas.** Nombres duplicados legales, sin resolución nombre→id en producción y homónimas indistinguibles en la lista de RACKVARIABLES y en el texto `=Nombre`. Un parser que resolviera nombres introduciría la **primera** ruta productiva nombre→id | §13 | FACT / INFERENCE |
| R7 | **Gramática numérica divergente.** Invariante en el editor y localizada en RACKVARIABLES; `"0.###"` redondea al mostrar, y «Cambiar valor» sin reescribir envía el valor redondeado (hallazgo lateral L5). Una gramática de expresiones con separador de argumentos colisionaría con la coma decimal | §11; `RackProjectVariablesWindow.xaml.cs:73,158-168` | FACT / INFERENCE |
| R8 | **Unidades.** No hay autoridad neutral de unidades ni tipo de magnitud: la unidad se infiere del token `Length`. Reutilizar `StructuralSectionUnits` metería una dependencia de catálogo en un concepto neutral, y cualquier conversión que toque el DWG exige ADR (ADR-0005 §4) | §12.4–§12.5; `docs/adr/0005-estrategia-de-unidades.md:50-54` | FACT / INFERENCE |
| R9 | **Acoplamiento al Selectivo.** Descriptores `double` sobre tipos Selectivos, consumidores solo Selectivos y `RackMutation` Selectiva. Montar el evaluador ahí ataría ID20/ID23 a Project Variables y al Selectivo, contra la entrada del Owner | §4.4; OWNER INPUT (ID20, ID23) | FACT / INFERENCE |
| R10 | **Semántica actual de `=`.** Ya significa consulta de referencia, con reglas de no auto-selección, LostFocus sin cambio de fuente y bloqueo C4. Sobrecargarla con expresiones cambia contratos que I-48 validó con el Owner | §9.6; HANDOFF §1 (`docs/HANDOFF.md:34-38`) | FACT / PROPOSAL INPUT |
| R11 | **Pérdida de trazabilidad** para ID28/ID29: el resolver escribe el valor en el diseño de Domain sin procedencia y las inspecciones son transitorias; nada se persiste | `SelectiveEffectiveDesignResolver.cs:145-163`; §16.4 | FACT / INFERENCE |
| R12 | **Colisión potencial con I-50** en archivos calientes si ambas implementan a la vez: su ADR-0035 propuesto añade `DimensionViews` al portador authored del Selectivo y toca los tres editores, y el portador (`From`/`WithDesign`) es también el camino de los bindings | §19.2 | FACT (doc de I-50) / INFERENCE |
| R13 | **Defectos laterales heredados** (L1 asimetría de colocación, L2 posible degradación de minor 2.x, L3 diccionario compartido por `WithDesign`) en superficies que ID22B tendría que tocar | §7, §5.8 | INFERENCE |
| R14 | **Extensión muerta.** `ProjectVariable` y `ToProjectVariables` no están en el camino operativo, y `PropertyValue<T>` no tiene consumidor: extender solo el modelo de dominio puede dejar tests verdes con producción intacta | §4.1 | FACT / INFERENCE |
| R15 | **Caminos heredados sin disparador** (`SelectiveBindingOptions.ForLength` con `Enum.TryParse`, `BindingIntent`/`ApplyBinding`, `VerticalClearanceBindingState`). Ampliarlos reabriría una gramática de tipo que I-48 retiró | `SelectiveBindingIntent.cs:92-124`; `RackSelectiveWindow.xaml.cs:2398`; `SelectiveEditorOpen.cs:26-50` | FACT / INFERENCE |
| R16 | **Materializar valores calculados.** `Unlink`, `UnlinkAllAndDelete` y la exportación escriben el efectivo como literal; con expresiones escribirían resultados de coma flotante, con el ruido de representación que eso implica | `ProjectVariableMutationPreflight.cs:283,423`; `SelectiveLibraryExport.cs:61-94` | INFERENCE |

---

## 18. Preguntas abiertas para Proposal V1

Todas son **PROPOSAL INPUT**. Ninguna se resuelve en este Discovery.

1. **Dónde vive una expresión**: en la definición de una variable (ADR-0034 §15), en un campo de propiedad
   (dirección no normativa de HANDOFF §4), o en ambos con reglas distintas (§9.7).
2. **Representación persistida**: texto fuente, forma normalizada con `VariableId`, o ambas; qué texto se
   muestra tras un rename.
3. **Versión del registro**: minor o major; si cambia cuando ninguna entrada es expresión; qué mensaje ve un
   build I-47/I-48 (§5.6).
4. **Token y payload del nuevo `Kind`**, y su relación con `ExtensionData` y con `PropertyNameCaseInsensitive`.
5. **Evaluación**: dónde (acreditación, resolver o capa nueva), cuándo (lectura, preflight, commit) y si el
   valor evaluado se persiste o siempre se deriva (§6.3).
6. **Grafo y ciclos**: detección antes de emitir plan, vocabulario de fallos, orden determinista y límites.
7. **Propagación transitiva**: cierre, deduplicación por rack y un único plan/commit/regen (§6.3, R4).
8. **Ciclo de vida con dependientes**: qué hacen Delete, Rename, UnlinkAllAndDelete y RepairBroken ante
   dependencias variable→variable; qué es una «variable rota» y si se repara, con qué valor (§8.2).
9. **Tipos**: si solo hay aritmética sobre `Length` o también escalares adimensionales; qué significa
   `Length × Length` o una división; si hace falta un segundo `VariableType`.
10. **Unidades** en literales de expresión: pulgadas implícitas, sufijos o nada; qué helper, si alguno, se
    reutiliza (§12).
11. **Gramática numérica**: invariante o localizada; conflicto entre coma decimal y separador de argumentos;
    precisión y formato de presentación (§11).
12. **Nombres dentro de expresiones**: si se escriben nombres y se resuelven a ids al comprometer; cómo se
    desambiguan homónimas; mayúsculas y minúsculas (§13).
13. **Editor**: qué significa `=`; cómo conviven consulta de referencia y expresión; qué reglas de C4,
    LostFocus y no auto-selección aplican; cómo se informa un error de parseo (§9).
14. **RACKVARIABLES**: cómo se crea, edita y muestra una expresión; valor evaluado frente a fuente; si se
    listan consumidores transitivos y dependientes (§10.5).
15. **ID20 (preparar, no implementar)**: forma de `SymbolId`; scopes y namespaces; `ExpressionContext`; qué
    resolución de símbolos y qué datos necesita el evaluador; cómo se impide que I-49 active `Rack.*` o
    `Project.*` en producción (§16.2).
16. **ID23 (independencia)**: frontera de namespace o ensamblado para parser, evaluador y contexto fuera de
    Project Variables, y el contrato mínimo que ID23 reutilizaría (§16.3).
17. **ID28/ID29 (preservar)**: qué traza se conserva (árbol, valores intermedios, símbolos resueltos) y qué
    impacto se calcula (dependientes, racks, vistas), en memoria o persistido (§16.4).
18. **Guardas de fuente**: cómo evolucionan `NO_HAY_FORMULAS_NI_REFERENCIAS_A_PROPIEDADES_DE_OTROS_RACKS` y
    `LA_DEFINICION_DE_UNA_VARIABLE_SIGUE_TENIENDO_UN_SOLO_CASO` sin perder su intención, en particular la
    prohibición de ID21 (R3).
19. **Compatibilidad de DWG I-47/I-48**: equivalencia exigida al re-guardar (bytes, árbol o semántica) y
    fixtures dorados (§5.7).
20. **ADR**: si la Proposal cambia persistencia, schema o una decisión de arquitectura, qué ADR se acepta antes
    de implementar (WORKFLOW §8; contrato §3.1). ADR-0035 ya lo ocupa I-50 (propuesto, §19.1).
21. **Alcance de sistemas y propiedades**: si el motor es neutral respecto de `RackSystemKind` y si I-49 amplía
    el catálogo de propiedades vinculables más allá de las dos actuales.
22. **Semántica numérica del resultado**: división por cero, NaN/∞, redondeo y determinismo, siempre
    fail-closed como exige ADR-0034 §8.
23. **Hallazgos laterales L1–L5**: si se tratan antes, dentro o fuera de I-49. Por contrato se registrarán en
    `docs/ideas-futuras.md` en una fase posterior; este G1 solo puede escribir su propio informe.

---

## 19. Colisiones reales y potenciales con I-50 e I-51

### 19.1 Reales

- **FACT** — I-50 @ `0d7670c` (G1 cerrado, G2A con Proposal V1.1 y ADR-0035 propuesto): `git diff --name-status
  origin/main...origin/feature/cotas-independientes-por-vista` = `docs/ROADMAP.md`,
  `docs/adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md`, `docs/adr/README.md`, `docs/ideas-futuras.md`,
  `docs/initiatives/I-50-cotas-independientes-por-vista.md`, `docs/initiatives/I-50-discovery.md`,
  `docs/initiatives/I-50-proposal-v1.md`, `docs/initiatives/I-50-proposal-v1.1.md`.
- **FACT** — I-51 @ `c4cc2e4` (G2): `git diff --name-status origin/main...origin/feature/rackduplicar-multiples-origenes`
  = `docs/ROADMAP.md`, `docs/automation/decisions/I-51.md`, `docs/ideas-futuras.md`,
  `docs/initiatives/I-51-discovery.md`, `docs/initiatives/I-51-rackduplicar-multiples-origenes.md`.
- **FACT** — Este G1 solo crea `docs/initiatives/I-49-discovery.md`.
- **FACT** — **No hay colisión productiva real**: ninguna de las tres ramas ha tocado un archivo productivo. La
  condición de `OWNER_OVERRIDE_I49_I50_PARALLEL` no se activa y el Discovery no se detiene.
- **FACT** — **Colisiones documentales previstas**: (a) las tres ramas insertan su fila de ROADMAP tras I-48;
  quien integre después conserva todas en orden numérico. (b) I-50 e I-51 modifican `docs/ideas-futuras.md`,
  donde I-49 registrará sus hallazgos laterales en una fase posterior. (c) I-50 ocupa **ADR-0035** y modifica
  `docs/adr/README.md`: un ADR de I-49 no puede reutilizar ese número.

### 19.2 Potenciales, a vigilar en G4+

| Archivo o área | I-49 (si la Proposal lo requiere) | I-50 (según su contrato) | I-51 (según su Discovery) | Clase |
|---|---|---|---|---|
| `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml(.cs)` (caliente, WORKFLOW §7) | Seam del editor vinculable (§9.7) | «UI mínima» de cotas (contrato I-50 §6 y §10, G3) | No previsto | INFERENCE |
| `src/RackCad.Plugin/*Commands*.cs` (grupo caliente) | `RackVariablesCommands.cs`, `RackSelectivoCommands.cs` | Caminos de inserción, actualización y redibujo (G4/G5) | `RackDuplicarCommands.cs` | INFERENCE |
| `SelectivePalletDesignDocument.cs` / `SelectivePalletDesign.cs` (caliente) | Solo si cambia la representación de bindings o el portador `WithDesign`/`From` (`SelectivePalletDesignDocument.cs:201-330`) | **Previsto por su ADR-0035 propuesto**: `int? DimensionViews` en `SelectivePalletDesignDocument` y `DynamicRackSystemDocument`, copiado en todo sitio que hoy copia `Dimensions` (ADR-0035 `:51-53,108-109` en `0d7670c`) | No (lo vigila por restamp) | FACT (doc de I-50) / INFERENCE (cruce con I-49) |
| `RackEmbedDocument.cs` / `RackEmbedComposer.cs` | Improbable | No según ADR-0035 propuesto: la política vive en el diseño y no en el sobre (`:44-45`) | No | FACT (doc de I-50) |
| Los tres editores ricos | Solo el Selectivo, si la Proposal toca el editor vinculable | ADR-0035 declara que los tres editores y `SelectivePalletDesign.cs` son archivos calientes y que los cambios en `RackSelectiveWindow` «se coordinan con I-49» (`:111-112`) | No | FACT (doc de I-50) |
| `RackEnvelopeRestamp.cs` / restamp del Selectivo | Solo si cambia `SelectivePropertyValueDocument` | Posible | Sí (sobrecarga) | INFERENCE |

- **INFERENCE** — Acoplamiento semántico con I-50: el diseño efectivo que produce el resolver es lo que dibuja
  el camino de cotas; si I-50 añade política por vista al portador authored, la comparación estructural
  multi-vista la incluye por defecto (`SelectiveAuthoredAuthority.cs:63-70`). No es una colisión, es un
  invariante compartido.
- **FACT (doc de I-51)** — El Discovery de I-51 no prevé «ningún archivo previsto en común» con I-49; su único
  acoplamiento con I-49 aparece si I-49 introdujera ID21 (fuera de alcance) o cambiara la representación de
  bindings, que el restamp debe preservar (`docs/initiatives/I-51-discovery.md` §13, en
  `origin/feature/rackduplicar-multiples-origenes`).

---

## 20. Confirmación de ausencia de implementación

- **FACT** — El único archivo creado por G1 es `docs/initiatives/I-49-discovery.md`, comprobado con
  `git status --porcelain` y `git diff --cached --name-status` antes del commit: un único cambio, añadido.
- **FACT** — No se modificó `src/`, `tests/`, `assets/`, `eng/`, `deploy/`, `tools/` ni `.github/`.
- **FACT** — No se creó parser, AST, evaluador, grafo de dependencias, schema nuevo ni UI productiva, y no se
  ejecutaron builds ni suites.
- **FACT** — No se editó `docs/HANDOFF.md`, `docs/ROADMAP.md`, `docs/ideas-futuras.md`, el contrato de I-49 ni
  ninguna rama o worktree de I-50 o I-51.

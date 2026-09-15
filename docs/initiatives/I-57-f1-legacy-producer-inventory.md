# I-57 F1 — Inventario cerrado de productores legacy

```text
Gate       = F1 / CHARACTERIZATION ONLY
Baseline   = c641476abca9d6a4370bd222961a0f997fbf5c73
Production = UNCHANGED
AUTH-15    = OUT OF SCOPE
```

Este inventario cierra el conjunto de productores observados que F2-F6 tendran que reutilizar, adaptar o dejar
detras de un port. Un path no implica autorizacion de cambio en F1. `Plugin` conserva toda medicion AutoCAD y
materializacion; `Application` conserva resolvers y builders puros. El retiro solo puede ocurrir en el gate indicado,
cuando el consumidor migre en el mismo commit verde.

| AUTH | Productor observado (path / simbolo) | Autoridad y consumidores actuales | Disposicion futura | Gate de retiro o delegacion |
|---|---|---|---|---|
| 01 | `src/RackCad.Application/Systems/Shared/DimensionViewPolicy.cs` / `DimensionViewKind`; `src/RackCad.Application/Systems/Cantilever/CantileverViewPlanBuilder.cs` / `CantileverViewKind` | taxonomia de cotas y camaras Cantilever | REUSE `DimensionViewKind`; adaptar camaras | F2 |
| 02 | `src/RackCad.Plugin/RackSelectivoCommands.cs`, `RackDinamicoCommands.cs`, `RackPushBackCommands.cs`, `RackCantileverCommands.cs`, `RackCabeceraCommands.cs`, `RackCamaCommands.cs` / interpretacion de `View` + `Section` | direccion persistida para editar/redibujar seis kinds | EXTRACT valor semantico puro; policy permanece en cada consumer | F2 |
| 03 | `src/RackCad.Application/Persistence/RackEmbedDocument.cs` / constantes; seis comandos anteriores; `src/RackCad.Application/Systems/PushBack/PushBackSystemFrontalBuilder.cs` / `EncodeSection`, `DecodeSection` | sintaxis, case, legacy y particiones de variante | EXTRACT codec total despues de CT-04 | F2 |
| 04 | builders Selective/Dynamic/PushBack bajo `src/RackCad.Application/Systems/`; `CantileverViewPlanBuilder`; layouts `RackFrames`; `FlowBedLateralBuilder` | disponibilidad fisica de vistas, fondos, cortes, lados y estaciones | EXTRACT facts por adapters de kind | F2-F3 |
| 05 | builders de vistas anteriores; `src/RackCad.Application/Systems/Selective/SelectiveDepthLayout.cs`; `CantileverViewPlan.Bounds` | ejes, origen, tramo, centro, extremo, offset y bounds | EXTRACT solo desde CT-05; sin correcciones I-52/I-55 | F3 |
| 06 | barridos en Plugin (`RackDuplicarCommands`, `RackVariablesCommands`, `ProjectVariableMutationExecutor`); `src/RackCad.Application/ProjectVariables/ProjectVariableScanProjection.cs` | snapshot fisico, conteo y clasificacion | ADAPT DTO/clasificador; probe AutoCAD queda Plugin | F4 |
| 07 | `src/RackCad.Application/Persistence/RackDuplicationPlan.cs` / `Build`, `RackDuplicationDestinationAssigner` | filtro, grouping, COPY identity, nombre y conteo | ADAPT solo prefiltro neutral; mantener semantica de duplicacion | F4 |
| 08 | `src/RackCad.Application/Geometry/Transform2D.cs` | matriz, composicion, espejo, escala, determinante | REUSE + adapter de facts; tolerancia pertenece al consumer | F4 |
| 09 | resolvers por sistema en `src/RackCad.Application/Systems/`; `src/RackCad.Plugin/KindHandlers/*KindHandler.cs`; `IRackKindHandler` | resolucion efectiva y autoridad del handler; Selective sigue ADR-0034 | KEEP IN PLACE BEHIND PORT; adapter por kind | F5 |
| 10 | builders Selective/Dynamic/PushBack/FlowBed/RackFrames/Cantilever; `HeaderRunPlan`; `CantileverViewPlan` | geometria tipada por kind | ADAPT fachada que delega; no plan geometrico universal | F5 |
| 11 | `*DrawService.BlockName`; `RackCantileverCommands.ViewBlockName`; `CantileverViewMaterializer.SuggestName`; `BlockNaming.SanitizeBlockName`; `UniqueBlockName` en materializadores Plugin | base de nombre y colision de definicion generada | EXTRACT base exacta; colision real permanece Plugin | F5 |
| 12 | `HeaderBlockInstance.BlockName`; `src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs` / `EnsureForPlan`, `EnsureBlocks`; `LateralHeaderDrawer.AppendInstance` | requirement key, import y observacion final | EXTRACT requirement puro; query e importer separados en Plugin | F6 |
| 13 | `src/RackCad.Application/ProjectVariables/SelectiveAuthoredAuthority.cs`; stores/resolvers propios de los otros kinds y `*KindHandler` | equivalencia authored y resultado Single/Divergent/Unreadable | ADAPT comparator por kind; reutilizar Selective | F6 |
| 14 | suites listadas por `tests/RackCad.Tests/Fixtures/I57/f1-characterizations.json` y `SharedViewFoundationF1CharacterizationTests` | golden masters y guardas de seams existentes | CHARACTERIZE ONLY; fixtures reutilizados por F2-F6 | F1, queda permanente |

## Comprobaciones de cierre

- Los seis comandos que interpretan el envelope estan censados en AUTH-02/03.
- Los builders que producen planes siguen siendo la unica autoridad geometrica; ningun adapter futuro puede copiar su
  aritmetica.
- `RackDuplicationPlan` permanece completo: F4 solo podra adaptar hechos neutrales sin transformar COPY identity,
  grouping, nombres ni conteo en selection policy.
- `BlockLibraryImporter` y `LateralHeaderDrawer` separan importacion de observacion final; ni importer ni query reciben
  el nombre de la vista generada.
- Los helpers `UniqueBlockName` pertenecen a colision Plugin y no son requirements de biblioteca.
- No se encontro un productor AUTH-15 dentro del alcance acordado. Crear definiciones caller-owned sigue fuera de I-57.

Resultado F1: inventario cerrado para la base observada. Un productor adicional descubierto en F2-F6 reabre el gate
correspondiente y falla cerrado; no se absorbe por analogia.

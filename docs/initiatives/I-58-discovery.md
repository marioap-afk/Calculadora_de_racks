# I-58 — Discovery DELTA de AUTH-13

Estado: D/F0 entregado para revision; F1 preparado, no cerrado. Toda afirmacion de diseño es
INFERENCE/propuesta hasta el acuerdo exacto; MEASURED identifica lectura de codigo o ejecucion.
Identidades de base, recibos, pruebas y blobs: [evidencia canonica](../automation/evidence/I-58-evidence.md).
Autoridades: WORKFLOW §§10-11; INITIATIVE_LIFECYCLE §§3-6; ADR-0034 §§5,8,9; ADR-0044;
I-57 Proposal V5 y R3. No se reescriben esas fuentes.

## DC-01 — comportamiento actual (MEASURED)

`src/RackCad.Application/Systems/Shared/RackAuthoredComparator.cs` define tres outcomes, resultado tipado
con Authored=default para Divergent/Unreadable, interfaz `IRackAuthoredComparatorPort<TInput,TAuthored>`
y `SelectiveAuthoredComparisonInput(RackId, IReadOnlyList<ProjectVariableScanEntry>)`.
Selective delega a `SelectiveAuthoredAuthority.Resolve`. Dynamic, PushBack, Cantilever, Cabecera y Cama
son `UnsupportedComparator<TInput,TAuthored>`: ignoran input y devuelven Unreadable siempre.
No existe un carrier concreto compartido para los cuatro kinds de I-58. No se puede afirmar que uno
existente transporte schemas/unknowns: los genericos permiten cualquier TInput, incluso Marker.

`SharedViewFoundationF6Tests.AUTH13_UNSUPPORTED_KIND_COMPARATORS_FAIL_CLOSED_AS_UNREADABLE` protege
precisamente esa ausencia. El probe de esta entrega observa el hueco con fixtures validos del store real;
sus PASS de Unreadable son vacuos respecto de lectura/schema y NO validan un reader futuro.

## DC-02 — autoridades verificadas (MEASURED / diseño INFERENCE)

| Kind | Authored authority | Canonical typed form propuesta | Schema gate | Unknown-member gate | Equality method | Expected Single/Divergent/Unreadable |
|---|---|---|---|---|---|---|
| Dynamic | `DynamicRackDesign`; `RackProject.DynamicDesign` | snapshot profundo `DynamicRackSystemDocument` de authored, sin Bfr derivado | Envelope 1.0; wrapper 1.0/2.0; headers 1.0; Dynamic NO tiene schema propio | raw input completo antes del DTO; wrapper/envelope ExtensionData vacia y cierre recursivo | comparacion explicita tipada de escalares/nullables y secuencias; exclusiones listadas en Freeze | todos iguales / cambio authored / cualquier ilegible |
| PushBack | `PushBackDesign`, incluyendo Structure, SideB, Composite | snapshot profundo `PushBackDesignDocument`, Structure bajo regla Dynamic | anteriores + payload 1.0 | ExtensionData de payload vacia; validar Structure, lados y topologias antes de ToDomain | comparacion tipada explicita por subarbol, sin resolver estructura compuesta | iguales / cambio authored incluso minoritario / schema o unknown inseguro |
| Cantilever | `CantileverLineDesign` | copia authored tipada de `CantileverLineDocument.Line` | envelope/wrapper + Cantilever 1.0 | ExtensionData externa vacia; cierre recursivo de Line; excepciones retiradas declaradas | campos authored tipados, identidad estable, secuencias ordenadas | iguales / cambio authored / payload, version, identidad o nested unknown ilegible |
| Cabecera | `RackFrameConfiguration` persistible | snapshot profundo `RackFrameProjectDocument` | envelope/wrapper + Header 1.0; formato legacy plano 1.0 | no ExtensionData en Header; rechazo previo a Deserialize | campos tipados de posts/plates/horizontals/panels; sin Members/Exceptions | iguales / cambio authored / documento inseguro |

Ownership neutral sigue I-57/Shared View Foundation (ADR-0044); I-58 extiende AUTH-13 por entrega propia,
no transfiere authority a I-55 ni crea un segundo comparador. La forma tipada es propuesta, no codigo nuevo.

## DC-03 — persistencia y perdidas reales (MEASURED)

`RackEmbedDocument.Design` es un string con JSON de proyecto. Los cuatro handlers leen
`RackProjectStore.Deserialize(embed.Design)`. Dynamic usa wrapper `RackProjectDocument.DynamicSystem`;
PushBack usa `.PushBack`; Cantilever `.Cantilever.Line`; Cabecera `.Header`, o proyecto plano legacy.
Cabecera tiene Kind externo `cabecera`, pero Kind del wrapper `RackSystemKind.Selective`; no confundirlo
con payload SelectiveRack ni dejar que BuildProject use fallback de kind desconocido.

- `RackEmbedDocument` 1.0 y `RackProjectDocument` 2.0 conservan ExtensionData en el root.
- DynamicRackSystemDocument no tiene SchemaVersion ni ExtensionData. Sus Front/Level/Module/overrides tampoco.
- PushBackDesignDocument y CantileverLineDocument 1.0 tienen ExtensionData SOLO en su root.
- Structure, SideB, Composite, topologias, Line y los objetos de Line no preservan unknowns.
- RackFrameProjectDocument 1.0 y Post/Plate/Horizontal/Panel no preservan unknowns.
- SafetySelectionDocument vive en `SelectivePalletDesignDocument.cs`, no en un archivo propio;
  esta anidado en Dynamic/PushBack. Incluye AuthoredSide, DerivedAisles, botas A/B y colecciones por celda/poste.

`SchemaVersionPolicy.IsReadable` rechaza major superior pero tolera ausente/malformada y minor futuro.
`ResolveWriteVersion` puede reemplazar una version malformada; no sirve como acreditacion del input.
Los stores aceptan nombres case-insensitive, comentarios y trailing comma en proyectos, enums string;
no activan una politica recursiva de rechazo de miembros desconocidos.

Perdidas que impiden usar round-trip a dominio como oraculo:

- Dynamic.ToDesign filtra peraltes <=0, entradas null, defaults, inserta un frente si no hay;
  Front.ToDesign omite Bfr; Module.ToDesign repara UseCalculatedHeaderConfiguration si falta Header.
- PushBack.ToDomain fabrica Structure si falta, normaliza strings, aplica defaults; Composite.ParseTopology /
  ParseDirection toleran nombres desconocidos con fallback. No acreditan equivalencia authored.
- Cantilever.ToDomain/DeepCopy sustituyen objetos null y filtran overrides null; Line.Id inicializa Guid.NewGuid:
  ausencia de Id no puede fabricar identidad distinta al leer cada hermana.
- RackFrameProjectStore.Deserialize refresca modelo fisico; DeepCopy usa ese camino y ademas copia Exceptions
  runtime-only. No usarlo dentro de AUTH-13. ToConfiguration sin builder sigue aplicando defaults.
- RackProjectStore/SystemRegistry.BuildDynamic llama ToDomain y refresca cabeceras; no es reader puro authored.
- `RackProjectStore.DropRetiredCantileverPunchMargins` retira ColumnBottomPlateEndOffset y ColumnTopPunchOffset
  al leer/escribir. Son excepciones legacy conocidas, no unknown generico.
- Cantilever.Line.IntervalCount es getter derivado sin JsonIgnore; las opciones del store no ignoran readonly
  globalmente. La inspeccion por sintaxis debe admitir el getter que el serializer puede emitir, pero no compararlo
  como authored. PanelSegment.Height si tiene JsonIgnore. El probe confirma IntervalCountSerialized=True; excluirlo no se basa solo en el comentario.

Hallazgo adicional MEASURED: `PushBackSafetyAuthority.RestrictToLowEnd` conserva AuthoredSide y colapsa Side;
`SelectiveSafetySelection.ChosenSide` usa AuthoredSide ?? Side. Comparar ambos campos literalmente filtra
estado effective a igualdad. `DynamicRackSystemResolver` ignora Module.Header cuando la procedencia es calculada
y lo reconstruye con catalogo; ese snapshot tampoco debe volverse authored. Freeze F-08 fija esas exclusiones
condicionadas, manteniendo schema/unknown antes de excluir. DerivedAisles no vacio en DTO no se restaura por
ToDomain: propuesta fail-closed, no descarte silencioso. Ambos casos requieren contraprueba conductual en F1.

## Matriz schema/unknown — propuesta cerrada para revision

Todos los gates se aplican a CADA hermana, cada wrapper y cada objeto versionado anidado ANTES de conversion.
No se cambia SchemaVersionPolicy, stores, DTO ni sus campos. `SchemaVersion` reconocido solo donde existe.

| Kind | Ausente / legacy | Malformed | Same-major future minor | Future major | ExtensionData | Unknown nested | Perdida potencial |
|---|---|---|---|---|---|---|---|
| Dynamic | envelope ausente/null=1.0; wrapper ausente/null=legacy 1.0; 1.0 y 2.0 conocidos; payload sin version propia | Unreadable; un SchemaVersion inyectado en Dynamic es unknown | Unreadable en wrapper/envelope/headers | Unreadable en cualquier nivel | root no vacia: Unreadable | Unreadable incluso Safety, Module.Header, Front.Level | SI; nunca From(ToDesign(raw)) para comparar |
| PushBack | igual wrapper; payload ausente/null=1.0; optional nullable se conserva | Unreadable, incluidos enums futuros no comprendidos | Unreadable aun sin unknown visible | Unreadable | root payload no vacia: Unreadable | Unreadable en Structure, SideB, Composite, cells | SI; no fallback de ParseTopology/Direction |
| Cantilever | payload ausente/null=1.0; defaults deterministas de campos ausentes; Id ausente/vacio=Unreadable | Unreadable | Unreadable | Unreadable | root payload no vacia: Unreadable | Unreadable en todo Line salvo dos margenes retirados conocidos en su ruta exacta | SI; impedir Guid nuevo y DeepCopy reparador |
| Cabecera | Header plano ausente/null=1.0 o wrapper conocido; fallbacks nullable conservados | Unreadable | Unreadable | Unreadable | wrapper/envelope no vacia: Unreadable; Header no dispone de ella | Unreadable en posts, plates, horizontals, panels y headers anidados | SI; no refrescar Members ni transportar Exceptions |

Version presente exige MAJOR.MINOR decimal sin signos, segmentos extra ni overflow; whitespace exterior
puede recortarse; string vacio/blanco no es ausencia y falla. Version futura 1.1 dentro de Header falla aun
con wrapper 2.0. Wrapper 1.1 tampoco es formato expresamente comprendido: solo 1.0/2.0. Version numerica,
objeto o array falla. El control es de lectura de autoridad, no cambia politica de guardado existente.

Unknown no se degrada a metadata inocua por estar en ExtensionData. Nombres duplicados (incluida colision
case-insensitive), valores no finitos y estructuras truncadas fallan. Ausente y null solo se equiparan donde
el contrato DTO lo hace; nullable authored null NO se colapsa al valor effective heredado. Cambio entre null
heredado y override explicito igual al valor actual sigue siendo Divergent. Una lista null/ausente y vacia
solo se equiparan cuando el contrato no les atribuye intencion distinta; agujeros posicionales nunca se borran.

## DC-04 — camino actual y frontera (MEASURED)

Editor -> diseño authored -> `RackProject.ForDynamic/ForPushBack/ForCantilever/ForSelective` ->
`RackProjectStore.Serialize` + SystemRegistry.Write* -> RackEmbedDocument.Design -> RackBlockData/Xrecord.
Reopen: KindHandler.Edit -> comando del kind -> RackProjectStore -> diseño del editor.
Dibujo/BOM: DynamicKindHandler/PushBackKindHandler/CantileverKindHandler/CabeceraKindHandler -> store ->
resolver/assembler/builder propio. Los cuatro BuildBom reciben ProjectVariablesDocument y no resuelven bindings
para estos kinds. AUTH-13 futuro se interpone sobre persistido, ANTES de cualquier resolver.
No entran catalogo, variables, geometria, BOM, frame fisico, matriz de colocacion o vista en igualdad.

## DC-05 — consumidores y lectores (MEASURED)

Inventario completo de referencias textuales dentro de src/ y pruebas protectoras:
[I-58-source-inventory.txt](../automation/evidence/I-58-source-inventory.txt). Comandos y alcance en cabecera.
Cubre Application, UI y Plugin, documentos, mappers, library, comandos, handlers y ProjectVariables.
Es inventario de simbolos, no prueba conductual ni afirmacion de ausencia de lectores externos.
Huecos: lectores binarios fuera del repo y DWG historicos no adjuntos UNKNOWN; no se auditan por esta unidad.

AUTH-13 hoy solo tiene consumidores de pruebas en main. En I-55, inspeccion read-only de su punta:
`Views/Preparation/RackProductPrepare.cs:PrepareExisting<TComparisonInput>` recibe el port, valida Kind,
traduce Divergent/Unreadable antes de Resolve y pasa `comparison.Authored` al resolver tipado.
`RackViewBatchPlan` conserva contrato puro G11; G12 driver/UI no existe en main.
Seam propuesto: carrier completo desde scan -> AUTH-13 tipado -> PrepareExisting -> Resolve -> Prepare.
El source envelope que I-55 reusa para compose debe estar acreditado en la misma lectura completa; su
CustomProperties y atomicidad siguen autoridades consumidoras. Esta entrega solo documenta ese precontrato.
No se copia codigo de la rama I-55 ni se implementa driver, UI, policy Insertar/ID18/ID19.

## DC-06 — protecciones y huecos (MEASURED)

I-57: SharedViewFoundationF6Tests protege unsupported, Selective Single/Divergent/Unreadable, resultado null,
no JSON/First/Majority en fachada y no AutoCAD/object/reflection escape. Mantener genericos unsupported como
compatibilidad; agregar overloads concretos tipados evita convertir Marker en exito y no rompe Cama/Selective.
Guardia de fuente no sustituye pruebas de unknown recursivo ni prueba de comportamiento.
Heredadas: DynamicDimensionViewsDocumentTests, DynamicHeaderBatchPersistenceTests, PushBackA1PersistenceTests,
PushBackNoParrillaPersistenceTests, CantileverPersistenceAndViewTests, CantileverRoundTwoCharacterizationTests,
RackFrameProjectStoreTests, PersistenceUniformityTests, PersistenceReopenPreservationTests.
Las mutaciones/authored equivalence para cuatro kinds no existen: [matriz](I-58-characterization.md).

## DC-07 — intersecciones (MEASURED)

Base y tips observados estan en evidencia. Diff de I-55 contra base toca preparation/redraw/batch/policy,
comandos/UI y pruebas; no modifica RackAuthoredComparator.cs ni DTO/stores. I-52 sigue rama propia.
D/F0 escribe solo archivos propios y fila propia ROADMAP. Interseccion documental compartida ROADMAP prevista,
resoluble por fila. Cualquier edicion futura sobre AUTH-13 exige repetir preflight y coordinar consumidores.
No editar su estado ni declarar que G12 ya esta desbloqueado.

## DC-08 — fundaciones (MEASURED)

AUTH-13 existe en main y tiene ADR-0044 aceptada, Proposal V5 integrada y consumidor I-55.
FOUNDATIONS describe Authored vs Effective como Selectivo solamente: coincide con codigo, no garantiza los cuatro
kinds. Unknown-field Preservation declara limites participantes: NO promete preservacion recursiva universal.
Header Mutation/Reconciliation no es un comparador generico; snapshot de header puede inspirar inventario,
no autoriza arrastrar Members ni normalizacion geometrica. No se altera el registro antes de conformidad.
I-58 sigue doctrine ADR-0034 y ownership ADR-0044; no reinterpreta historia de I-57 como soporte ya implementado.

## DC-09 — materialidad, riesgos y arquetipo propuesto

| ID | Estado | Evidencia / riesgo / disposicion |
|---|---|---|
| M-01 | no activado | owner AUTH-13 permanece Foundation/I-57; evitar autoridad paralela en consumidor |
| M-02 | activado | cambia trato de unknown/future en NUEVO lector de autoridad; wire, DTO, store y fallback persistido intactos; revisar cada descarte |
| M-03 | no activado | delta pedido precisamente soporta cuatro kinds; Cama/Selective y rutas actuales no cambian |
| M-04 | activado | Unreadable incondicional pasa a tres outcomes; precedencia y fallo cerrado son invariantes |
| M-05 | activado | I-55 consume interfaz tipada existente con nuevos carriers concretos; no presuponer que TInput actual basta |
| M-06 | activado | nuevos adapters tipados en punto de extension AUTH-13; evitar generico semanticamente incompleto |
| M-07 | no activado | sin kernel/registry/reflection comparer/resolver universal; cuatro readers/equalities tipados acotados |
| M-08 | no activado | ADR-0034/0044 y R3 conservados; no cambia autoridad, ni product policy |

FOUNDATION EVOLUTION propuesto; Coordinator confirma, Architect puede elevar (lifecycle §3).
Riesgos centrales: effective->authored por ToDomain; minor futuro aceptado por store; unknown nested perdido;
comparacion que omite nullable/coleccion; Guid no determinista; first/majority; SchemaVersion mutado al guardar;
comparator generico que ignora input. Todos tienen obligaciones en Freeze/matriz.

## EXP-01..09 — evaluacion y remision (INFERENCE)

| EXP | Evaluacion | Pregunta acotada y salida / autoridad pendiente |
|---|---|---|
| 01 | negativa | discrepancias de encabezados historicos resueltas por merge/recibo y registros; ninguna contradiccion consumida A encontrada |
| 02 | positiva, hallazgo delimitado | DTO authored comparte campos derivados Bfr y getters; decidir exclusiones exactas con Coordinator/Architect; propuesta en Freeze §3 |
| 03 | negativa | wrapper y cabecera plana localizados; fixture historico externo no inventado |
| 04 | positiva | uso futuro PrepareExisting de I-55 mas scan/composer; limite documentado, aprobacion Coordinator pendiente |
| 05 | positiva | igualdad nueva sin tests protectores; matriz y probe definidos; cierre conductual real en F1/F2 despues de acuerdo |
| 06 | positiva | ToDomain y parser de version normalizan en silencio; estrategia raw preflight tipado propuesta; revision pendiente |
| 07 | negativa actual | no interseccion del comparator/DTO con diff observado de I-55; repetir antes de implementar |
| 08 | positiva | nested unknowns se pierden en caminos antiguos; NO repararlos globalmente, rechazar antes en AUTH-13; revisar impacto legacy |
| 09 | negativa | M clasificados, sin UNKNOWN material; pendientes son veredictos, no hechos rellenados |

No se autoautoriza una expansion general. El Owner solicito expresamente estas preguntas de codigo;
se entregan hallazgos acotados y propuestas. Coordinator debe autorizar/cerrar formalmente EXP positivas y
preguntar cual debio activarse y no se activo. Discovery no se declara CLOSED ni F0 AGREED sin esa revision.

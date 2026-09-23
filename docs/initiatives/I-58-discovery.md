# I-58 — Discovery DELTA de AUTH-13 — correccion AR58-V2-01 / V3

Estado: D/F0 corregido para revision V3; EXP-01 CLASS B FOR I-58 ONLY / CONFIRMED; F1 NOT OPEN. Toda afirmacion de diseño es
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

| Kind | Authored authority | Canonical INTERNA propuesta (no salida publica) | Schema gate | Unknown-member gate | Equality method | Expected Single/Divergent/Unreadable |
|---|---|---|---|---|---|---|
| Dynamic | `DynamicRackDesign`; `RackProject.DynamicDesign` | snapshot profundo `DynamicRackSystemDocument` de authored, sin Bfr derivado | Envelope 1.0; wrapper 1.0/2.0; headers 1.0; Dynamic NO tiene schema propio | raw input completo antes del DTO; wrapper/envelope ExtensionData vacia y cierre recursivo | comparacion explicita tipada de escalares/nullables y secuencias; exclusiones listadas en Freeze | todos iguales / cambio authored / cualquier ilegible |
| PushBack | `PushBackDesign`, incluyendo Structure, SideB, Composite | snapshot profundo `PushBackDesignDocument`, Structure bajo regla Dynamic | anteriores + payload 1.0 | ExtensionData de payload vacia; validar Structure, lados y topologias antes de ToDomain | comparacion tipada explicita por subarbol, sin resolver estructura compuesta | iguales / cambio authored incluso minoritario / schema o unknown inseguro |
| Cantilever | `CantileverLineDesign` | copia authored tipada de `CantileverLineDocument.Line` | envelope/wrapper + Cantilever 1.0 | ExtensionData externa vacia; cierre recursivo de Line; excepciones retiradas declaradas | campos authored tipados, identidad estable, secuencias ordenadas | iguales / cambio authored / payload, version, identidad o nested unknown ilegible |
| Cabecera | `RackFrameConfiguration` persistible | snapshot profundo `RackFrameProjectDocument` | envelope/wrapper + Header 1.0; formato legacy plano 1.0 | no ExtensionData en Header; rechazo previo a Deserialize | campos tipados de posts/plates/horizontals/panels; sin Members/Exceptions | iguales / cambio authored / documento inseguro |

Ownership neutral sigue I-57/Shared View Foundation (ADR-0044); I-58 extiende AUTH-13 por entrega propia,
no transfiere authority a I-55 ni crea un segundo comparador. La forma interna es propuesta, no codigo nuevo.
CR58-02: el TAuthored PUBLICO es respectivamente DynamicRackDesign, PushBackDesign,
CantileverLineDesign y RackFrameConfiguration. Los DTO de la tabla NO son la autoridad retornada.
F-06/F-12/F-13 definen materializacion fiel despues del gate raw y seam hacia autoridad existente.

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
estado effective a igualdad. CORRECCION AR58-V2-01: `DynamicRackSystemResolver` SI observa
Module.Header.PostPeralte calculado al elegir fallback global, ANTES de reconstruir la cabecera. V2
confundia reconstruccion del frame con irrelevancia de todo el payload. F-08 V3 distingue global positivo
y global heredado; mantiene schema/unknown antes de cualquier exclusion. DerivedAisles no vacio en DTO no se restaura por
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

## DC-02..06 — expansion C1 de identidad Cantilever (acotada)

MEASURED por fuente: decision vinculante I-37 §12.46 y XML-doc de CantileverLineDesign.Id exigen una
identidad de linea comun a vistas. Constructor inicializa Guid.NewGuid. RackEditorIdentity.EnsureId
usa otro factory; LoadNew/LoadDesignForNew no sincronizan; RequestDraw pasa ctx.Id separado del
lastComputation.Design. BuildCantileverPayload serializa diseño y compone envelope con el argumento id.
LoadExisting adopta envelope.Id y DeepCopy preserva Line.Id. CantileverKindHandler.RestampDesign asigna
el nuevo GUID interior; RackEnvelopeRestamp pasa el mismo texto a exterior e interior. Es contradiccion
real consumida por F-03 V1, no simple encabezado historico: EXP-01 inicialmente CLASS A / STOP.

MEASURED de ejecucion WPF y RECONSTRUCTED de writer/restamp: cuatro escenarios en
[I-58-c1-identity.md](../automation/evidence/I-58-c1-identity.md), que separa cada identidad, pertenencia,
AUTH09/10, impacto y limites. A/B/C outer!=inner; hermanas conservan ambos por separado. D reconstruido
outer==inner nuevo compartido. Biblioteca puede reutilizar inner entre racks exteriores diferentes.
No se ejecutaron comandos/transacciones AutoCAD ni se auditaron DWG historicos no aportados.

MEASURED por fuente I-55: RackSiblingScan llena RackSiblingScanFact con envelope.Id y probe exterior;
RackSiblingMembership.Classify compara RackId/ProbeId con conjunto exterior atribuible, no lee Line.Id.
Su rama se inspecciona solamente. PrepareExisting pasa comparison.Authored a AUTH09 sin conversion de
DTO, y conserva source.Id exterior para componer; no reinterpreta identidad interior. En main AUTH09
acepta delegate tipado; AUTH10 recibe resolved/address/frame/baseName sin parametro exterior Id.
El probe invoca AUTH09<CantileverLineDesign,...> y assembler existente una vez, conserva inner. No prueba
que el wiring completo de I-55 este implementado ni usa una guardia textual como test de comportamiento.

INFERENCE/propuesta B SOLO para alcance I-58: la igualdad authored dentro de un conjunto de pertenencia
exterior coherente necesita inner valido/equivalente ENTRE hermanas, no outer==inner. F-03 V1 hubiera
rechazado documentos producidos por A/B/C; V2 retira esa precondicion. Inner distinto entre hermanas es
Divergent; inner ausente/invalido es Unreadable; exterior mezclado es Unreadable aun con inner comun.
La falta de igualdad exterior/interior no requiere reparacion para comparar el conjunto. Esto no prueba
que la identidad sea correcta para todos los demas consumidores ni cura incumplimiento de I-37.
Deuda D58-CANT-ID registrada en evidencia propia, fuera del alcance de reparacion. Sin restamp/migracion.

Clasificacion vinculante de entrada V3: EXP-01 = CLASS B FOR I-58 ONLY / CONFIRMED por
Coordinator + Architect, comunicada por Owner. CR58-01/02 = RESOLVED. Se conserva deuda fuera de alcance;
no reabrir la decision sin evidencia materialmente nueva. La dependencia de peralte es distinta y no aporta
nueva evidencia sobre identidad Cantilever. EXP-08 incorpora ahora tambien ese fallback preexistente.

CR58-02 (INFERENCE de diseño): raw siblings -> validacion schema/unknown/forma -> canonical INTERNA ->
Single de DOMINIO profundo -> AUTH09 -> autoridad vigente una vez -> AUTH10. F-13 enumera
DynamicRackSystemResolver, PushBackResolver, CantileverLineEditorAssembler y BracingPanelMemberBuilder.
No es una promesa de mappers actuales sin perdida: CT58-26 exige comprobar valor retornado por subarbol,
incluso cuando canonical igual ya dio Single. Si ToDomain/ToDesign pierde intencion, no puede usarse para
materializar ese caso. Foundation no obliga a I-55 a arreglarlo con otro conversor/resolver.

## DC-03/05 delta V3 — dependencia de Header.PostPeralte (MEASURED)

Inventario de fuente acotado y blobs exactos: [I-58-v3-source-identities.txt](../automation/evidence/I-58-v3-source-identities.txt).
Matriz ejecutada y limites: [I-58-v3-fallback.md](../automation/evidence/I-58-v3-fallback.md).
Se siguio SOLO transporte/transformacion de PostPeralte/header, no una auditoria general de geometria o UI.

| Sitio actual | Hecho observado | Consecuencia para AUTH-13 |
|---|---|---|
| DynamicRackDesign / DynamicRackModuleDesign | global double default0, flag calculada defaulttrue, Header opcional; IsHeader por Kind | global y peralte de header son ubicaciones distintas; orden no es metadata |
| DynamicRackSystemResolver.ResolvePostPeralte | global>0; luego primer IsHeader cuyo Header.PostPeralte>0 sin filtrar flag; luego Width del perfil por id y default3 | eliminar calculated Header en global0 altera resultado; conservar global0, no copiar efectivo a global |
| Resolve, construccion de cada header | useCalculated = flag OR Header null; calculated reconstruye con builder; custom clona; luego estampa peralte del rack | global positivo enmascara fallback calculado; custom conserva authored aunque parte se sobrescriba al resolver |
| Validate | global<0 lanza; no es legacy cero | V3 rechaza negativo; ausencia/null de DTO equivale0; nofinito rechazado raw |
| DynamicRackSystemDocument.From/ToDesign | From global positivo o null; ToDesign global nullable??0 | global ausente/null/0 pertenece al mismo legacy, positivo explicitado no se infiere de header |
| DynamicRackModuleDocument.From/ToDesign | copia Header completo y procedencia; ToDesign repara flag a true si Header null | raw custom+null debe rechazarse antes, aunque resolver/store actuales toleren |
| RackFrameProjectDocument.FromConfiguration/ToConfiguration | preserva PostPeralte; nullable??0; arbol persistido de posts/plates/horizontals/panels | normalizar peralte nullable dentro de Header NO equivale a borrar Header |
| DynamicRackSystemResolver.Snapshot | toma peralte efectivo de system/geometry como global; copia associated headers y flag | Snapshot NO sirve para materializar authored acreditado |
| RackModuleEditSession.Begin/CopyIntent/Commit | copia en orden, DeepCopy completo, ResolveProvenance repara ausencia; SetHeader marca custom; Restore borra y marca calc | no introducir stub no acreditado ni usar sesion como canonical reader |
| RackFrameProjectStore.DeepCopy / HeaderConfigurationSnapshot | roundtrip + validacion de alto/fondo/postes + reconstruccion fisica; snapshot exige header usable | cabecera de solo peralte no acredita rutas de copia; mantener payload completo en rama conservadora, copiar puro en comparator |
| RackModuleReconciliation | solo arrastra custom/longitud manual; Adapt estampa global positivo y fondo; calculated queda reconstruida | esta operacion cambia effective por edicion, no define equivalencia authored de persistidos |
| DynamicRackSystemBuilder.BuildHeaderConfiguration/ApplyPostPeralte | construye header; Apply estampa peralte en sistema y associated frames | modifica resolved, no fuente de canonical |
| DynamicEditorDesignAssembler / PushBackEditorDesignAssembler | copian cabeceras y Snapshot; Dynamic restablece global del input; PushBack aplica global antes de snapshot | transporte de UI no sustituye captura raw original; no usar para promover effective |
| PushBackDesignDocument.FromDomain/ToDomain | Structure via Dynamic document, SideB/Composite separados | mismo fallback persiste dentro de Structure; no hay segunda Structure authored en SideB |
| PushBackResolver.ResolveSingleSided | invoca DynamicRackSystemResolver sobre Structure | la matriz PushBack simple prueba consumo real, no inferencia por tipo |
| PushBackCompositeStructure.StoredSideModules/Reconcile/SideStructuralDesign | particiona A/B por identidad; invierte B; copia global; conserva Header/flag si caracter por posicion coincide | conservar cada modulo, incluido fallback no primero del rack; cambios de orden pueden cambiar primer positivo local |
| PushBackCompositeStructure.Compose / CompositeResolver | resuelve locales, compone y vuelve a resolver estructura; adopta cabeceras de los lados | parity exige rack + locales A/B; no basta peralte global compuesto |
| PushBackMirror | copia peralte de sistema y associated frames al reflejar resolved | no da autoridad authored nueva ni legitima usar resultado espejo como canonical |
| SystemRegistry.Default.BuildDynamic/BuildPushBack | leen DTO, refrescan modelos fisicos de cabeceras | store se usa SOLO en diagnostico save/reopen, nunca para comparar |
| DynamicHeaderBatch / UI Dynamic y PushBack | aplican custom, estampa peralte del rack y refresh; UI obtiene/edita global y copia/restaura header | son caminos de mutacion autorizada por editor, no saneamiento silencioso permitido al comparator |
| DynamicFrontGeometry y builders de vista/BOM | consumen sistema/associated header resueltos, algunos tienen fallback efectivo propio | no se modifican ni se integran en igualdad; no se audita su geometria |

Distinguir cuatro fuentes: global authored positivo; peralte persistido calculado; peralte persistido custom;
ancho catalog/default externo. Las dos fuentes de header compiten en UNA secuencia para Dynamic/single-sided.
En compuesto, A/B pueden observar secuencias distintas. Probe: global0, A headers7/B headers9 produce
rack7/A7/B9; global8 produce8/8/8. No es valido conservar solo el primer positivo del conjunto.

INFERENCE V3: forma interna tipada por modulo y Header persistible completo cuando global0; con global>0
Header calculado=null tras validar raw. Comparacion explicita campo a campo, sin serializer/resolver/catalogo.
Se elige conservar MAS que el minimo numerico: copia/edicion exige header completo, y no se ha acreditado
una normalizacion menor de todos los subarboles. Coste declarado: [7,9] vs [7,0] y cambios de metadata pueden
ser Divergent aunque el efectivo actual coincida. No afirmar que todo ese payload determina el peralte;
la regla conservadora necesita revision expresa, no se presenta como minimalidad probada.
Esto satisface seguridad de fallback sin elegir first sibling, inventar dimensiones, promover global o cambiar
procedencia. CT58-29..32 aun deben demostrarlo con el comparator real. No se implementa esa autoridad aqui.

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
| M-01 | no activado | AUTH-13 conserva owner Foundation/I-57; canonical privado no es segunda autoridad publica; F-06/13 lo explicitan |
| M-02 | activado | nuevo gate raw de unknown/schema y compatibilidad F-09; no cambios de wire/DTO/store; CT58-27 antes de mappers |
| M-03 | ACTIVATED | AR58-V2-01 demuestra que V2 podia alterar resultado observable de documentos existentes al materializar Single: 7/9 pasa a3. Aceptado por Coordinator. F-08/12a/13 V3 y CT58-29..32 corrigen la perdida; no se cambia resolver |
| M-04 | activado | unsupported pasa a tres outcomes; precedencia y validacion de inner entre hermanas explicitas |
| M-05 | activado | contrato publico a consumidores usa cuatro diseños de DOMINIO exactos; DTO canonical no escapa; seam AUTH13->AUTH09 sin reconversion I-55 |
| M-06 | activado | adapters concretos amplian AUTH-13 manteniendo genericos unsupported; materializacion por kind dentro del limite existente |
| M-07 | no activado | ningun converter/kernel/registry/equality universal; contrato generico vigente no se sustituye |
| M-08 | activado conservador para revision | V2 admite comparar documentos que incumplen decision integrada I-37 §12.46. Aunque no modifica esa decision ni la creacion, su tratamiento conserva M-08 conservador, sin afirmar inocuidad porque I-37 no esta en el diff; EXP-01 B confirmado SOLO I-58 |

FOUNDATION EVOLUTION propuesto; Coordinator confirma, Architect puede elevar (lifecycle §3).
Riesgos centrales: effective->authored por ToDomain; minor futuro aceptado por store; unknown nested perdido;
comparacion que omite nullable/coleccion; Guid no determinista; first/majority; SchemaVersion mutado al guardar;
comparator generico que ignora input. Todos tienen obligaciones en Freeze/matriz.

## EXP-01..09 — reevaluacion V3 acotada

| EXP | Evaluacion | Evidencia / disposicion |
|---|---|---|
| 01 | CLASS B FOR I-58 ONLY / CONFIRMED | Coordinator + Architect segun entrada vinculante; CR58-01/02 RESOLVED; no reabrir sin evidencia materialmente nueva de identidad |
| 02 | positiva; ampliada | calculated Header no es enteramente effective: peralte persistido influye fallback. F-08 distingue global; F-12a conserva returned domain |
| 03 | negativa acotada | legacy global ausente/null caracterizado explicitamente; no formatos nuevos ni auditoria de DWG externos |
| 04 | positiva; conservada | seam publica tipada AUTH13->AUTH09; fidelity no se delega a conversor de I-55 |
| 05 | positiva; ampliada | matriz Dynamic y consumo real PushBack, orden y locales; CT58-25/26 ampliados y29..32; probe diagnostico no cierra F1 |
| 06 | positiva; ampliada | perdida puede ocurrir DESPUES de igualdad, en canonical/materialization; sabotaje Header=null con igualdad intacta rompe parity; global promovido debe fallar assert authored |
| 07 | negativa actual | preflight no interseccion nueva en autoridad/persistencia; I-52 avanza en rama propia, no se toca; repetir antes de implementacion |
| 08 | positiva; ampliada | dependencia preexistente del resolver respecto de Header.PostPeralte sin filtro provenance; unknowns perdidos e identidad Cantilever siguen registrados con disposiciones previas; no reparar resolvers |
| 09 | positiva para reevaluacion, acotada a AR58-V2-01 | el M-03 omitido en V2 ocultaba cambio observable; ahora ACTIVATED. Riesgo residual de exceso de conservacion requiere respuesta Architect/Coordinator; no abre auditoria general ni autoridad universal |

**¿qué expansión debió activarse y todavía no está activada?** La omission demostrada fue M-03 y la
ampliacion de EXP-02/05/06/08 al fallback; quedan activadas en V3. EXP-09 se eleva a positiva para revisar
la suficiencia de esa correccion y el coste de falsos Divergent conservadores. No hay otra expansion concreta
sin activar identificada en este recorrido acotado. Es conclusion revisable, no garantia universal ni cierre
unilateral de EXP positivas. Coordinator y Architect deben responder expresamente sobre suficiencia y coste.
M-07 no se activa: regla concreta de dos kinds en AUTH-13 existente, sin kernel/registry/converter universal.

## Disposicion individual de findings y entrada vinculante V3

| ID | Estado vinculante / propuesta | Pendiente |
|---|---|---|
| CR58-01 | RESOLVED segun Owner; EXP-01 B confirmado por Coordinator + Architect | no reabrir sin evidencia materialmente nueva |
| CR58-02 | RESOLVED segun Owner | conservar cuatro tipos dominio y seam; AR no revoca resolucion previa |
| AR58-V2-01 | REQUIRED / ACCEPTED BY COORDINATOR; CORRECTION PROPOSED | Architect re-review V3 requerido; no auto-cerrar finding. F-08/12a/13, MM-D09a..e, CT58-29..32 y diagnostico124 |

Revision Architect V2 realizada en SAME-SESSION ROLE; revisor=autor: SI. Ese modo no se convierte en
revision independiente. El Owner comunico CHANGES REQUIRED ON V2 y aceptacion Coordinator del finding.
Paquete V3 solicita nueva revision y no anticipa veredicto. Freeze sigue DRAFT/Frozen NO.

AR58-V2-01 = CORRECTION PROPOSED / ARCHITECT RE-REVIEW REQUIRED
Coordinator = REVIEW REQUIRED ON V3
Architect = CHANGES REQUIRED ON V2 / PENDING V3
Frozen = NO
F1 = NOT OPEN
I-55 G12 = NOT UNBLOCKED
IMPLEMENTATION AUTHORIZATION = NO

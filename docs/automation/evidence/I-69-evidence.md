# I-69 — Evidencia de preflight y Discovery

Unit: I-69. Workflow: V2. Claim-Id: `ee21eac2-5e9c-4761-aa20-71c18fe3ac23`.

## 1. Preflight (MEASURED)

- Repo: https://github.com/marioap-afk/Calculadora_de_racks
- origin/main verificado por API, fetch y ls-remote: `812baf879612320653094a8aaf46aca0bac81383`.
- Claim remoto: `ac0e2826aa7493885840103dd66924583f3cfbc7`; arbol igual a main y padre main.
- Rama: `feature/selectivo-catalogo-componentes`.
- Worktree: `/workspace/scratch/e1f4ec6199fc/feature-selectivo-catalogo-componentes`.
- Transporte: commit GitHub API + creacion atomica de ref nueva sin force; fetch y seguimiento
  local confirmados. CLI git push fallo por falta de credenciales; no se obtuvieron credenciales.
- Claim local no publicado: `ed6df4e014339ee98ca5d03130a12dd66f4c982a`; sustituido solo localmente
  por el claim remoto con reset --soft despues de comprobar arbol limpio y arboles identicos.
- I-69 libre: revisados contratos/decisiones de todas las refs remotas, ROADMAP y trailers
  Initiative-Id de la historia disponible; maximo I-68.
- Workflow V2 efectivo: `8a021fb67c16dfccd6afc18448ea7e6a71a32364`, ancestro de main.
  Trailer normativo unico: `afd1077cba90ace890ac253bbe34a234b6691540`.
  Tag integration/I-56: `41b1835b3d1a78d37c34dd945c2feb981539a287`; fin de pausa
  `2026-09-17T22:30:00Z`. Nueva clasificacion T4.
- I-61: ADR-0046 aceptado e integrado; AUTOMATION_PLAN §16 leido.
- Linux, dotnet no disponible en PATH. Core/UI/build/AutoCAD: NOT RUN.

## 2. Ramas activas (MEASURED)

| Rama | Punta | Interseccion inicial |
|---|---|---|
| architecture/portabilidad-coordinador-principal | a08afecc17ee69ed95c7a85508dad47e788a35b4 | ROADMAP; sin catalogo/Selectivo de producto en diff inicial |
| architecture/workspace-persistente-rackcad | fa59bd82acff7ce276a1605c60d426c45a3e3b8d | ROADMAP y SelectiveEditorOpenTests |
| architecture/workspace-persistente-rackcad-a14 | 5131585fa5b15f20f2d1696d3e33479a4ebe6633 | misma iniciativa I-64, ROADMAP y SelectiveEditorOpenTests |
| feature/rackmirror-espejo-semantico | fb6b56483a421e84c4bc44dd0aa1761813b7e457 | ROADMAP; consumidores por revisar |
| fix/placedclone-copia-anotaciones | 5d52e5d8b2590e0e65b9a9f746b07923e2bb8a78 | sin catalogo/Selectivo de producto en diff inicial |

Diff de cada punta contra su merge-base con main. ROADMAP recibe solo la fila nueva; no se
reescriben filas ajenas. No existe acuse de otros responsables ni se declara exclusividad de
sus superficies. Resolver las intersecciones antes de escribir producto.

Relectura remota previa a publicar Discovery: main y las demas puntas siguen iguales.
La rama I-64 a14 avanzo desde 4efa1a10d719c132f96e018eefede90ebc1ca00e a la punta
de la tabla. Fetch/diff confirmo que ese delta solo cambia evidencia/estado I-64;
las intersecciones actuales siguen siendo ROADMAP y SelectiveEditorOpenTests.

## 3. Resultado, estado y clases de evidencia

Fecha: 2026-11-23. Fuente de producto: main identificado en §1.
**IMPLEMENTATION AUTHORIZATION = NO.** F0 no cerrado; sin Freeze, GATE PASS ni Candidato.

El dropdown es catalog-driven: secciones.csv distribuido en runtime, proyectado por rol
a BeamProfiles. Id y displayName estan separados. Id llega a BeamId en persistencia,
PieceId en geometria y ProfileId en BOM. Renombrar displayName conserva la identidad,
pero cambia etiquetas de otros consumidores y puede reordenar la seleccion inicial.

Seguridad usa seguridad.csv y una coleccion de SelectiveSafetySelection. No existen
las dos piezas solicitadas. El camino generico existente admite UI + persistencia +
BOM manual, equivalente a B. A y C requieren definicion adicional. Una segunda
PARRILLA puede quedar persistida pero no representada/contada: la UI permite varias,
mientras dibujo y BOM toman la primera.

Clases de evidencia:

- MEASURED: Git, bytes/inventarios de archivos, presencia de simbolos, CI identificado
  y ausencia del SDK. No significa ejecucion de RackCad.
- RECONSTRUCTED: comportamiento reconstruido siguiendo codigo y pruebas existentes.
- INFERENCE: consecuencia deducida, sin ejecutar el caso.
- UNKNOWN: intencion Owner, piezas nuevas, biblioteca externa y observacion en AutoCAD.

El [inventario de datos](I-69/catalog-inventory.json) conserva SHA-256, cuatro filas de
larguero con mappings/conexiones y 18 filas de seguridad. Es auditoria Python/CSV,
no ejecucion del parser C#, editor ni AutoCAD. La instalacion y CSV locales del Owner
no fueron observados.

### Evidencia adicional de preflight y bootstrap

MEASURED: arbol de main 9ee20e23049e6bad8daa80ffbda1e357e399ae82.
Bootstrap publicado 41b5e376148a94879ceebe758a20cc395e749991. Su commit local
dbb66b47b4453d5da857df8dddc8917402f60f73 no se publico con ese hash: la API creo
el mismo arbol y se alineo localmente tras verificar limpieza e igualdad de arboles.

El intento local de dotnet test tests/RackCad.Tests/RackCad.Tests.csproj termino con
exit 127, dotnet: command not found. Core/UI/build/AutoCAD locales: NOT RUN.

[CI bootstrap, run 45272269600](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/45272269600):
event push, head_branch feature/selectivo-catalogo-componentes,
head_sha 41b5e376148a94879ceebe758a20cc395e749991, completed/success.
Se consultaron los cuatro jobs: Tests (Domain + Application), UI Tests (WPF controls,
net8.0-windows), Build UI (WPF, valida API de Application) y Build Plugin without
AutoCAD; los cuatro success. No se recuperaron conteos de pruebas.
Esta evidencia solo corresponde al bootstrap, no se hereda a otro commit y no
sustituye Core local/AutoCAD/Owner Validation. Los hechos posteriores al commit de
este informe se reportaran aparte.

I-61/ADR-0046 integrado: AUTOMATION_PLAN §16 exige Controller/Worker, contrato, relevo,
custodia, identidad exacta y cesion exclusiva. Las revisiones auxiliares de Discovery
se hicieron sobre una exportacion inmutable de main, sin escribir el worktree.
No constituyen implementacion delegada formal ni acreditan EXECUTION_VERIFIED.
No se declaran celdas/modelos elegibles sin medirlos. Coordinator conserva GATE PASS.

Coordinacion adicional: I-25 / feature/guardas-traseras es backlog diferido en ROADMAP,
sin rama remota activa observada. No se reclama, cierra ni absorbe. Antes de escribir
producto se repetira el diff de ramas y se coordinara cualquier interseccion. No se
ha escrito SelectiveEditorOpenTests (hotfile I-64) ni HANDOFF.

## 4. Fuente y esquema de largueros

RECONSTRUCTED desde [JsonRackCatalogProvider](../../../src/RackCad.Application/Catalogs/JsonRackCatalogProvider.cs),
[CatalogDirectory](../../../src/RackCad.Application/Catalogs/CatalogDirectory.cs),
[CsvCatalogReader](../../../src/RackCad.Application/Catalogs/CsvCatalogReader.cs) y
[CatalogEntries](../../../src/RackCad.Application/Catalogs/CatalogEntries.cs):

1. RackEditorSession entrega el RackCatalog cargado por JsonRackCatalogProvider.FromBaseDirectory.
2. CatalogDirectory.Resolve prefiere catalogs junto al ensamblado, con fallback a
   AppContext.BaseDirectory.
3. ReadArray solicita secciones.json, pero prueba antes el CSV hermano: CSV con filas
   tiene precedencia; si no aporta filas, prueba JSON.
4. LoadUncached usa la fuente unificada cuando aporta filas. Solo si no aporta ninguna
   usa los archivos legacy beam-profiles/post-profiles/truss-profiles.
5. SeccionRoles.Classify normaliza trim/mayusculas; LARGUERO se proyecta como Beam.
6. ToBeam copia Id, DisplayName, Description y propiedades a BeamProfileCatalogEntry.

La autoridad vigente del dropdown es
[assets/catalogs/secciones.csv](../../../assets/catalogs/secciones.csv).
Excel es herramienta de edicion, no dependencia runtime ni libro de importacion de
esta lista. No se localizo workbook xls/xlsx versionado que alimente el dropdown.
El catalogo neutral structural-section/AISC no es su fuente.

Esquema completo observado:

rol, id, displayName, description, family, peraltes, mensula, width, depth, thickness,
units, gauge, weightPerMeter, material, finish, partNumber, manufacturer, unitCost,
currency, costUnit, Ix, Iy, norma, perforacion.

| Campo | Responsabilidad |
|---|---|
| rol | LARGUERO entra en BeamProfiles; no existe bandera exclusiva Selectivo |
| id | Identidad del tipo; enlace con pieza, bloques, conexiones y persistencia |
| displayName / description | Label elige DisplayName no vacio, despues Description y finalmente Id |
| family | Metadata, no filtro adicional del dropdown Selectivo |
| peraltes | Lista separada por punto y coma; PeralteList conserva positivos validos sin duplicados |
| mensula | FK textual a mensulas.csv para receta BOM |
| width/depth/thickness/units/gauge | Datos del perfil; no sustituyen bloques/conexiones reales |
| peso/comerciales | Metadata; partNumber esta vacio en los cuatro largueros y no es su identidad |

CsvCatalogReader asigna encabezados a propiedades sin distinguir mayusculas; columnas
extra entran en Properties. Trim de strings y conversion tolerante: una celda tipada
invalida deja el default. Acepta comillas, escapes y saltos de linea entrecomillados.
UTF-8 estricto con/sin BOM; fallback efectivo Encoding.Latin1.

La cache usa nombre/tamaño/mtime de CSV/JSON, no hash de contenido ni DWG. Un guardado
que cambie la firma recarga en el proximo Load; conservar nombre, tamaño y mtime puede
impedir invalidacion. La ventana abierta conserva el catalogo de su sesion: no hay
promesa de refresco inmediato de combos ya abiertos.

ADR-0007 esta aceptado. ADR-0008 fue reemplazado conceptualmente por ADR-0020, cuya
decision 4 mantiene expresamente este catalogo legado. No corresponde migrar Selectivo
a StructuralSection ni interpretar esa coexistencia deliberada como contradiccion.

## 5. UI, filtro e inventario completo

[UiSupport.ToOptions](../../../src/RackCad.UI/UiSupport.cs) descarta IDs vacios, agrupa
por Id trimmed con OrdinalIgnoreCase, conserva la primera fila y ordena por Label con
CurrentCultureIgnoreCase. Crea [CatalogOption](../../../src/RackCad.UI/CatalogOption.cs)
con Id y DisplayName.

[RackSelectiveWindow](../../../src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs)
asigna todos los BeamProfiles a CellBeamBox. El
[XAML](../../../src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml) usa
SelectedValuePath=Id y DisplayMemberPath=DisplayName. ReadCellEditor exige Id no vacio
y peralte positivo; escribe SelectiveEditorCell.BeamId/BeamPeralte.
No hay allowlist Selectivo ni filtro adicional de family. No se encontraron otras
filas LARGUERO ocultas que baste habilitar.

MEASURED: cuatro IDs distintos; las cuatro FK de mensula resuelven; cada tipo tiene
mappings FRONTAL/LATERAL/PLANTA. Estos nombres son displayName literales; description
esta vacio en los cuatro.

| Stable ID / Code | Current Description | Source | Persisted As | Geometry Consumer | BOM Consumer |
|---|---|---|---|---|---|
| LARGUERO_ESCALON_CAL14_3_REMACHES | Larguero escalón calibre 14 con ménsula 3 remaches | secciones.csv | BeamId = mismo Id | PieceId en builders F/L/P | ProfileId en LargueroBomBuilder |
| LARGUERO_ESCALON_INFINITO | Larguero escalón de ajuste infinito | secciones.csv | BeamId = mismo Id | PieceId en builders F/L/P | ProfileId en LargueroBomBuilder |
| LARGUERO_ESCALON_TROQUEL_REDONDO | Larguero escalon troquel redondo | secciones.csv | BeamId = mismo Id | PieceId en builders F/L/P | ProfileId en LargueroBomBuilder |
| LARGUERO_IN_OUT_C6 | Larguero in n out de canal de 6" | secciones.csv | BeamId = mismo Id | PieceId en builders F/L/P | ProfileId en LargueroBomBuilder |

El tercer nombre dice literalmente "escalon", sin acento. No se corrigio. El orden de
esta tabla identifica piezas; el visible depende de la cultura y las etiquetas.

| ID abreviado solo en esta tabla | Peraltes | FK mensula |
|---|---|---|
| CAL14_3_REMACHES | 3;3.5;4;4.5;5;5.5;6 | MENSULA_3_REMACHES_CAL_10 |
| INFINITO | 3;3.5;4;4.5;5;5.5;6 | MENSULA_AJUSTE_INFINITO_CAL_10 |
| TROQUEL_REDONDO | 3;3.5;4;4.5;5;5.5;6 | MENSULA_TROQUEL_REDONDO_CAL_10 |
| IN_OUT_C6 | 6 | MENSULA_TROQUEL_REDONDO_CAL_10 |

La deduplicacion UI puede ocultar filas duplicadas. T2 debe comprobar unicidad en
fuente y una opcion visible por Id, no solo el tamaño del dropdown.

## 6. Persistencia y create/save/reopen/edit

RECONSTRUCTED:

CellBeamBox.SelectedValue → SelectiveEditorCell.BeamId →
SelectiveEditorState.BuildBayDesigns → SelectiveCell.BeamId →
SelectiveCellDocument.BeamId → SelectivePalletDesignStore JSON →
RackEmbedDocument.Design → RackBlockData Xrecord.

Campos JSON, con mayusculas reales:

- Bays[].Levels[].BeamId y BeamPeralte.
- ExtraFondoBays[][].Levels[].BeamId y BeamPeralte.
- BeamLengthOverride y ClearOverride en las mismas celdas.
- SafetySelections[].ElementId y Quantity, mas configuraciones flat de seguridad.

No se persiste DisplayName/Description como identidad de celda. El store no aplica
naming policy, lee case-insensitive, omite null y serializa enums como strings.
SelectivePalletDesignDocument mantiene 1.0 y promocion sticky a 2.0 para bindings.
Agregar valores de Id al catalogo no requiere por si mismo schema nuevo.

[RackSelectivoCommands.EditSelective](../../../src/RackCad.Plugin/RackSelectivoCommands.cs)
lee embed.Design, resuelve variables/SelectiveEditorOpen y carga authored/effective en
LoadExisting; la opcion se restaura por BeamId. SerializeSelectiveDesign usa From en
creacion y authored.WithDesign al editar.
[RackBlockData](../../../src/RackCad.Plugin/Systems/Shared/RackBlockData.cs) guarda el
sobre JSON en Xrecord RACKCAD_SELECTIVE de la ExtensionDictionary de la definicion,
fragmentado en strings de hasta 255 caracteres.

Compatibilidad:

1. Renombre puro de displayName: sin alias/migracion de BeamId. El diseño viejo y su
   BOM regenerado muestran la etiqueta vigente, no una copia historica del nombre.
2. Cambiar/eliminar Id seria otro cambio, no autorizado. Conservar tambien casing:
   aunque los lookups suelen ignorarlo, authored compara strings con Ordinal.
3. SelectedIndex=0 inicializa DefaultBeamId tras ordenar. INFERENCE: un renombre
   puede cambiar la pieza inicial de NewCell y de celdas legacy con BeamId null.
4. Carga usa cell.BeamId ?? DefaultBeamId; un Id desconocido no vacio no se reemplaza
   automaticamente. El combo puede quedar sin seleccion y Aplicar exige un larguero.
5. WithDesign preserva ExtensionData en la raiz del documento. SelectiveCellDocument
   y SafetySelectionDocument no tienen JsonExtensionData: no hay promesa de
   preservacion recursiva de propiedades desconocidas.

Lectores adicionales DC-05: SelectiveAuthoredAuthority compara el documento tipado
completo serializado (arrays en orden, strings Ordinal), sin lookup de catalogo.
RackAuthoredComparator.SelectiveComparator y BomAuthoredAuthority consumen esa
autoridad. RackDuplicationPlan acredita acuerdo authored; SelectiveAuthoredRestamp
deserializa, cambia Id/Name del rack y vuelve a serializar, sin traducir IDs de piezas.
SelectiveEditorCell.Clone/CopyFrom y SelectiveSafetySelection.DeepCopy transportan
los IDs. Esto acredita transporte, no soporte geometrico de piezas nuevas.

Si C introduce campos en SafetySelectionDocument, el contrato es compartido:
AuthoredRawReader.Shapes enumera miembros admitidos para Dinamico/PushBack,
AuthoredCanonical.Safety exige ElementId no vacio y AuthoredTypedValues.Same compara
los campos explicitamente. No se amplian automaticamente por añadir una propiedad.

## 7. Geometria de largueros

RECONSTRUCTED: SelectiveGeometryResolver.AddBeam conserva BeamId/BeamPeralte. El
nivel de mayor longitud gobierna GoverningBeamId del frente. SelectivePostGeometry
busca INICIO_PERFIL por Id/vista para posiciones, separacion de postes y superficie
de carga.

| Vista | Consumidor y datos |
|---|---|
| Frontal | SelectiveFrontalBuilder.AddLargueros: PieceId, block lookup, LONGITUD y PERALTE |
| Lateral | SelectiveLateralBuilder.BuildLargueros/MakeLarguero: bloque/PERALTE; CollectLevels deduplica Y + BeamId + Peralte |
| Planta | SelectivePlantaBuilder.AddLargueros: primer nivel como representacion colapsada, frente/fondo y tramos cargados |
| Plugin | DrawService → ViewBlockDraw → SystemBlockWriter → LateralHeaderDrawer |

MEASURED: blocks.csv enumera explicitamente <ID>_FRONTAL, <ID>_LATERAL y <ID>_PLANTA
para los cuatro. Ese patron observado no autoriza inventar bloques de nuevas piezas.

| ID abreviado | INICIO_PERFIL FRONTAL (X,Y) | INICIO_PERFIL PLANTA (X,Y) |
|---|---|---|
| CAL14_3_REMACHES | 1, 4.9375 | 0.125, 1 |
| INFINITO | 1.0514, 6.3437 | 0.1187, 1.0514 |
| TROQUEL_REDONDO | 0.997, 6.8125 | 0.0375, 0.98 |
| IN_OUT_C6 | 0.997, 5 | -0.15, 0.98 |

INFINITO/TROQUEL_REDONDO tambien tienen mates LATERAL izquierdo/derecho; IN_OUT_C6
tiene TROQUEL_CAMA. El inventario conserva las coordenadas completas. Todas sus
pendientes parametricas son cero.

Ancho/forma efectiva dependen del DWG externo y parametros; columnas de perfil no
acreditan por si solas la geometria. No se encontraron lecturas de Width/Thickness
de BeamProfileCatalogEntry en los builders Selectivo revisados. La biblioteca real
no fue inspeccionada; el preview lateral es esquematico.

INFERENCE acotada: la altura superior del resolver usa LocalY directo, mientras
BeamProfileStartY resuelve LocalY + LocalYPorParam × PERALTE. Un offset nuevo con
pendiente no nula exige caracterizacion antes de prometer soporte. Las cuatro
filas actuales no tienen esa discrepancia porque sus pendientes son cero.

## 8. Consumidores compartidos de larguero

| Consumidor | Acoplamiento comprobado |
|---|---|
| RackLargueroWindow | Ofrece todos los BeamProfiles; altas/renombres llegan al editor independiente |
| DynamicRackLevelGeometry | CompatibleInOutBeams usa TROQUEL_CAMA + bloque LATERAL; CompatibleIntermediateBeams exige mates izquierdo/derecho + bloque LATERAL |
| DynamicRackDefaults / PushBackDefaults | IDs constantes actuales; renombrar displayName no los cambia |
| SystemBomBuilder / PushBackBomBuilder | Lookup por Id, descripcion desde Label compartido |
| CatalogBlockParameters | Contrato Beam y excepcion especifica por Id para IN_OUT_C6 |

No se localizo identidad comparada por DisplayName/Label en las rutas inspeccionadas.
Agregar un perfil compatible puede ampliar otras listas automaticamente. Delimitar
ese efecto requiere decision de alcance; no se propone un catalogo paralelo.

## 9. BOM de largueros

[SelectiveKindHandler.BuildBom](../../../src/RackCad.Plugin/KindHandlers/SelectiveKindHandler.cs)
deserializa authored, resuelve effective/geometria y llama a
[SelectiveBomBuilder](../../../src/RackCad.Application/Systems/Selective/SelectiveBomBuilder.cs).

AddLargueroComponents recorre el contenido real de cada fondo y sus instancias
frontales; agrupa PieceId + longitud + peralte (redondeo de numeros a dos decimales)
y suma dos largueros, delantero/trasero, por instancia. Delega en
[LargueroBomBuilder.Component](../../../src/RackCad.Application/Systems/Larguero/LargueroBomBuilder.cs):

- ProfileId = BeamId.
- Receta: una pieza de perfil y dos mensulas mediante el FK del larguero.
- Descripcion = Label vigente + " · P" + peralte, si el peralte es positivo.

BillOfMaterials aplana cantidad de pieza × cantidad de componente. ConsolidatedBom
aplica Math.Max(1, rack.Copies) a cantidades de componentes. Display no es clave.
Nivel/frente/fondo determinan enumeracion fisica, pero BomLine/BomComponent no
conservan esa atribucion. "Nivel" del export significa Componente/Pieza.

INFERENCE previa, sin ejecutar: el agrupamiento inicial incluye peralte, pero el
total plano usa Category/ProfileId/Length y el consolidado categoria/longitud/firma
de receta. Igual Id/longitud/receta con peraltes distintos puede fusionarse y
conservar la primera descripcion. Se registra el limite; no se modifica
consolidacion ni I-68/XlsxWriter.

## 10. Seguridad: inventario, modelo y comportamiento

Fuente: [seguridad.csv](../../../assets/catalogs/seguridad.csv); mismo provider
CSV-first, SafetyElementsFile = seguridad.json. SafetyElementCatalogEntry hereda
Id/DisplayName/Description y agrega Type, Units y WeightEach. Type es string de
familia, no un enum de piezas. El modelo es una coleccion:

SelectivePalletDesign.SafetySelections → SelectiveSafetySelection(ElementId,
Quantity, Side, PostSides y configuraciones por familia). DeepCopy central compone
las copias. SafetySelectionDocument.From/ToDomain y SafetySelectionDocuments
mapean el payload flat con campos nullable y fallback legacy. No hace falta un
campo por cada Id generico nuevo.

### Inventario completo (MEASURED)

| Type | Id | displayName literal | UI normal Selectivo |
|---|---|---|---|
| BOTA | PROTECTOR_BOTA_H_3_16_18 | Protector bota tipo H | Variante |
| BOTA | PROTECTOR_BOTA_C_4 | Protector bota tipo C4 | Variante |
| BOTA | PROTECTOR_BOTA_C_6 | Protector bota tipo C6 | Variante |
| LATERAL | PROTECTOR_LATERAL_BOTA_H_3_16_18 | Protector lateral bota tipo H | Variante |
| LATERAL | PROTECTOR_LATERAL_BOTA_C_4 | Protector lateral bota tipo C4 | Variante |
| LATERAL | PROTECTOR_LATERAL_BOTA_C_6 | Protector lateral bota tipo C6 | Variante |
| TOPE | LARGUERO_ESCALON_TOPE_DE_3 | Larguero tope de 3 | Variante |
| TOPE | POSTE_3_1_5_8_TOPE | Poste tope de 3 x 1 5/8 | Variante |
| PARRILLA | PARRILLA_GENERICA | Parrilla | Entrada |
| DESVIADOR | DESVIADOR_A_3 | Desviador A para poste de 3" | Variante |
| DESVIADOR | DESVIADOR_A_4 | Desviador A para poste de 4" | Variante |
| DESVIADOR | DESVIADOR_L_3 | Desviador L para poste de 3" | Variante |
| DESVIADOR | DESVIADOR_L_3_5 | Desviador L para poste de 3.5" | Variante |
| DESVIADOR | DESVIADOR_L_4 | Desviador L para poste de 4" | Variante |
| DESVIADOR | DESVIADOR_L_4_5 | Desviador L para poste de 4.5" | Variante |
| DESVIADOR | DESVIADOR_L_5 | Desviador L para poste de 5" | Variante |
| GUIA | GUIA_ENTRADA | Guía de entrada | Oculta |
| DEFENSA | DEFENSA_MONTACARGAS | Defensa de montacargas | Oculta |

18 registros, 16 variantes visibles en cinco familias. SelectiveSafetyWindow tiene
includeGuia=false/includeDefensa=false por defecto; Safety_Click los conserva.
SelectiveSafetyFamilies agrupa como exclusivas BOTA/LATERAL/TOPE/DESVIADOR/GUIA/DEFENSA;
PARRILLA queda fuera de esa exclusividad.

### UI → persistencia → geometria → BOM (RECONSTRUCTED)

| Familia | UI / configuracion persistida | Geometria Selectivo | Cantidad BOM |
|---|---|---|---|
| BOTA | Variante, ubicacion/lados y overrides por poste | Frontal/lateral/planta; lateral puede sustituir bota | Conteo fisico en planta; cero si queda reemplazada, sin fallback manual |
| LATERAL | Variante, lados por poste | Frontal/lateral/planta | Conteo fisico; longitud dibujada + allowance vigente de 4 pulgadas |
| TOPE | Variante, plan frente/nivel/fondo, lados y offsets | Tres vistas, proyecciones del plan | Segmentos fisicos habilitados por longitud; ruta actual exige mapping lateral; visual frontal no gobierna cantidad |
| PARRILLA | Plan por celda/tarima, frente y cantidad limitada a capacidad | Frontal/lateral; sin llamada de colocacion en planta aunque exista mapping | Plan fisico por frente, independiente de visibilidad |
| DESVIADOR | Variante, lados y plan poste/nivel | Tres vistas | PhysicalQuantity del plan, no suma de proyecciones |
| GUIA / DEFENSA | No ofrecidas en Selectivo normal; ElementId/config si llegan en payload | Sin rama de colocacion Selectivo | Una seleccion inyectada puede caer en fallback manual generico; no acredita soporte UI Selectivo |

Fuentes: [SelectiveSafetyWindow](../../../src/RackCad.UI/SelectiveSafetyWindow.cs),
[SelectiveSafetyFamilies](../../../src/RackCad.Application/Systems/Selective/SelectiveSafetyFamilies.cs),
[SelectiveSafetyPlacement](../../../src/RackCad.Application/Systems/Selective/SelectiveSafetyPlacement.cs),
SelectiveTopePlan/SelectiveParrillaPlan/SelectiveDesviadorPlan, builders F/L/P y
SelectiveBomBuilder.AddSafetyComponents/AddParrillaComponents/AddDesviadorComponents.

### Las dos piezas pedidas y A/B/C

MEASURED: no hay filas para Guarda trasera ni Entrepaño caída de agua. TRASERA existe
como tipo documentado y titulo de grupo "Guardas traseras", sin regla funcional de
dibujo encontrada. No se encontro modelo, regla ni prueba del entrepaño solicitado.

| Alcance | Encaje actual |
|---|---|
| A — UI + persistencia | No se logra solo con fila generica: cantidad positiva entra en BOM; cero/vacio no persiste seleccion. Exige comportamiento adicional |
| B — UI + persistencia + BOM | Camino generico existente: entero manual positivo; ElementId/Quantity persistidos; componente Seguridad con ProfileId=ElementId, Label, longitud 0 y receta de una pieza |
| C — UI + persistencia + geometria + BOM | Necesita piezas, colocacion, dimensiones, vistas, conteo y bloques reales; reutilizar una familia exige equivalencia funcional comprobada |

En B la cantidad se lee una vez por seleccion del rack logico, no se multiplica
por niveles/tarimas/fondos. Las copias siguen la consolidacion vigente. No se ha
elegido ni inventado una cantidad para el Owner.

Riesgos concretos:

- Segunda PARRILLA: la UI puede persistir ambas; SelectedOfType y
  AddParrillaComponents eligen la primera. INFERENCE estatica, sin reproduccion
  runtime. Definir variante alternativa vs elemento independiente/coexistente.
- PushBack usa denylist GUIA/PARRILLA/TOPE/DEFENSA en el dialogo; una familia nueva
  o TRASERA puede aparecer. Su autoridad tampoco la excluye por ser desconocida.
  Dinamico usa allowlist de familias conocidas. No asumir aislamiento Selectivo.
- Aceptar el dialogo reconstruye selecciones desde filas presentes: una seguridad
  cuyo Id desaparecio puede perderse ahi, aunque DTO/domain/BOM la conserven.
- Agregar campos de configuracion, a diferencia de agregar valores Id, alcanza
  lectores/comparadores compartidos descritos en §6.

## 11. Recurso ausente: politica efectiva

RECONSTRUCTED. No existe un fail-closed general de catalogo.

| Frontera | Comportamiento actual para T14 |
|---|---|
| BeamId desconocido en JSON | Conserva string; UI puede no seleccionar opcion y rechazar Aplicar sin larguero |
| BeamProfile ausente en BOM | Conserva ProfileId/descripcion cruda y no obtiene FK de mensula |
| FK mensula presente, fila destino ausente | Conserva dos piezas con ese Id y descripcion cruda; distinto de no tener FK |
| Conexion ausente | Offset cero/origen en los resolvers revisados |
| Mapping de bloque ausente | Frontal/lateral conservan instancia BlockName null; planta omite la pieza |
| Bloque DWG ausente | Drawer omite/registra faltante; preparacion puede colocar con informe |
| Seguridad desconocida | Store/domain conservan Id; BOM puede usar Id crudo/cantidad manual; aceptar dialogo puede perderla |
| Payload ilegible, authored divergente o binding roto | Mantener las puertas estrictas existentes |

SelectiveKindHandler.OutputBlockedReason devuelve null. RackSingleViewPlacementTests
y RackViewBatchPlanTests documentan PlacedWithReport/advertencias con piezas requeridas
faltantes. Endurecer todos los casos seria M-04, no una descripcion del estado actual.

## 12. Distribucion y biblioteca externa

RECONSTRUCTED: RackCad.Plugin.csproj incluye *.csv/*.json/*.dwg desde assets/catalogs
como Content, CopyToOutputDirectory=Always, hacia catalogs junto al plugin.
AssembleAutoloaderBundle corre AfterTargets=Publish, recrea el bundle y copia esos
patrones directamente de fuente a Contents/catalogs. Un build simple no produce
por si solo ese bundle.

deploy/verify-bundle.ps1 compara inventario plano y SHA-256 frente a los archivos
presentes en assets/catalogs del checkout. No exige por nombre blocks-library.dwg
si la fuente no lo contiene. AGENTS declara esa biblioteca no versionada y una
ruta en settings del usuario; en assets/deploy del snapshot no hay archivos DWG.

deploy/install-bundle.ps1 puede copiar la biblioteca ya instalada sobre staging y
verifica que conserve su hash. Actualizar catalogos/bundle puede conservar
deliberadamente el DWG anterior. Por eso T15 acredita distribucion de catalogos;
el smoke de AutoCAD acredita por separado presencia/parametros de bloques reales.
No se construyo ni entrego DLL en Discovery.

## 13. DC-01..09 y Consumes / Extends / Introduces

| Core | Evidencia y limite |
|---|---|
| DC-01 | §§4–11: codigo, inventarios y pruebas localizadas; sin ejecucion local de producto |
| DC-02 | Datos CSV y reglas Application/UI; ADR-0007, ADR-0020 carveout legacy, ADR-0014 |
| DC-03 | §6/10: BeamId/ElementId, DTO/store/envelope/Xrecord, schema y limites de desconocidos |
| DC-04 | §§4–10: entrada → estado → persistencia → dibujo/BOM → reapertura |
| DC-05 | §§6/8/10: lectores/copia/comparadores y consumidores entre sistemas |
| DC-06 | Proposal §5: T1–15, protecciones existentes y oraculos aun no definidos |
| DC-07 | §2/3: ramas/hotspots; coordinacion de producto pendiente antes de escritura |
| DC-08 | Tabla siguiente: fuentes/simbolos/pruebas; no se inventa una entrada FOUNDATIONS |
| DC-09 | §14: materialidad parcial, EXP-09 abierto y arquetipo superior provisional |

| Relacion | Autoridad / mecanismo | Verificacion y limite |
|---|---|---|
| Consumes | Catalogos CSV, ADR-0007 / compatibilidad ADR-0020 | Provider/parser/entries; CsvCatalogReaderTests, JsonRackCatalogProviderTests, SeccionesCatalogTests |
| Consumes | Rack Identity y View Identity, ADR-0009/0010 | Envelope/composer/store/RackBlockData/command; RackEmbedDocumentTests/PersistenceReopenPreservationTests |
| Consumes | Authored vs Effective, ADR-0034 y decision integrada I-48 | SelectiveEditorOpen/EffectiveDesignResolver/authority/kind handler; SelectiveAuthoredBindingTests/SelectiveBomAuthorityTests. Consumidor: dibujo/BOM reciben effective, persistencia conserva authored |
| Consumes | Shared View Authored Comparators, ADR-0044 y Freeze V3 I-58 | RackAuthoredComparator delega Selectivo a su autoridad vigente; SharedViewFoundationF5Tests y SelectiveAuthoredBindingTests. Se consume el puerto, sin igualdad paralela por catalogo |
| Consumes | Unknown-field Preservation, contrato integrado I-11 y ADR-0039 | I-11 gobierna el envelope y sus cuatro limites declarados; otros DTO segun su contrato. WithDesign/ExtensionData/store/composer; pruebas PersistenceReopen/SelectivePalletDesignDocument |
| Consumes | Copia central de seguridad, ADR-0014 | DeepCopy/documentos por subtipo; SafetySelectionDocumentsTests/SelectiveSafetyConfigTests |
| Consumes de coordinacion | Agent Execution Protocol, ADR-0046 / I-61 | AUTOMATION_PLAN §16 integrado, sin modificacion ni entrega delegada acreditada |
| Extends previsto | Registros BeamProfiles/SafetyElements y mappings necesarios | Datos compatibles; contrato final depende de piezas/A-B-C/consumidores |
| Introduces | Ninguna foundation, plataforma, schema o migracion decidida | Nuevos IDs pendientes; UNKNOWN no autoriza infraestructura nueva |

FOUNDATIONS es registro descriptivo; no tiene una entrada independiente "Beam Catalog"
que esta sesion deba inventar. No se edita el registro por consumir lo integrado.

Fuentes de decision: [ADR-0034](../../adr/0034-project-variables-autoridad-drawing-level.md),
[decision Owner I-48](../decisions/I-48.md) que fija el acuerdo sobre Proposal V8,
[ADR-0044](../../adr/0044-hechos-neutrales-de-vistas-compartidas.md),
[Freeze V3 I-58](../../initiatives/I-58-freeze-draft.md) y su
[evidencia final de integracion](I-58-evidence.md),
[contrato I-11](../../initiatives/I-11-persistencia-uniforme.md) y
[ADR-0039](../../adr/0039-custom-properties-persistencia-autoridad.md).
I-58 tiene cabecera Frozen: YES y receipt integration/I-58 integrado en main;
los textos historicos pendientes de su cuerpo no sustituyen los acuerdos y cierre
versionados. No se reinterpreta su clausula F-02: Selectivo conserva autoridad propia.

Limites DC-05: busqueda productiva de BeamProfiles/BeamId/SafetySelections en src,
seguimiento de DTO/serializacion/comparadores/canonicalizers/copia/restamp y fuentes
assets/tests/deploy/docs. No es prueba de exhaustividad runtime, ni inspeccion de
plugins externos o instalaciones del Owner.

## 14. Materialidad y expansiones: STOP antes del Freeze

EXTENSION fue el arquetipo inicial. Se descarta descripcion como identidad persistida,
pero se confirma acoplamiento compartido. INITIATIVE_LIFECYCLE §3 convierte UNKNOWN
en activado y pide el arquetipo superior ante duda.
**FOUNDATION EVOLUTION provisional**, clasificacion final pendiente de EXP-09,
Owner y revision Architect. No afirma que ya haga falta schema/migracion.
Datos compatibles + B podrian permitir EXTENSION antes del Freeze, si todos los
disparadores quedan inactivos con evidencia y decision de las autoridades.

| ID | Disparador oficial | Estado y condicion |
|---|---|---|
| M-01 | Cambia quien posee una regla o valor, o aparece una segunda autoridad para el mismo dato | NO ACTIVADO: propuesta conserva CSV/Id; no crea catalogo paralelo |
| M-02 | Cambia esquema, DTO o wire, significado persistido, fallback legacy o preservacion de campos desconocidos | UNKNOWN: renombre/B no lo requieren; C podria requerir configuracion nueva |
| M-03 | Cambia comportamiento observable o trato de documentos existentes fuera de lo pedido | UNKNOWN: etiquetas compartidas, default y visibilidad necesitan alcance Owner |
| M-04 | Cambia que falla o como falla: silencioso/visible, abortar/normalizar o fail-closed | NO ACTIVADO: propuesta conserva §11; endurecimiento/sustitucion reabre el disparador |
| M-05 | Cambia un contrato consumido por otros sistemas o kinds | UNKNOWN: filtro/variantes/DTO compartidos dependen del alcance; filas compatibles no prueban cambio de contrato por si solas |
| M-06 | Añade, cambia o retira un punto de extension reutilizable | UNKNOWN: B usa el existente; una familia geometrica nueva podria modificarlo |
| M-07 | Introduce framework, registro, kernel, shell o mecanismo generico transversal | NO ACTIVADO: fuera de alcance, no hay propuesta creadora |
| M-08 | Modifica, reinterpreta o contradice un ADR aceptado o la fuente de decision integrada citada por una fundacion; una discrepancia solo en el registro descriptivo activa EXP-01 y no le concede autoridad | NO ACTIVADO: propuesta respeta catalogo legado, identidad y copia central; reevaluar si cambia |

### Discovery Conditional acotado por Coordinator

| Expansion | Evaluacion, pregunta y criterio de salida |
|---|---|
| EXP-01 | No activada: ADR-0008/0020 explican la convivencia. Sin contradiccion consumida clase A encontrada |
| EXP-02 | No activada: fuente runtime e Id/display localizados; no hay autoridades rivales del dropdown |
| EXP-03 | No activada: payload/store/Xrecord/reopen localizados; smoke pendiente no equivale a camino desconocido |
| EXP-04 | Activada/caracterizada: consumidores §§6/8/10; salir con contrato Owner de visibilidad/etiquetas |
| EXP-05 | No activada como imposibilidad de probar: se conocen pruebas Core/UI/bundle y smoke. El oraculo fisico faltante es decision Owner de alcance |
| EXP-06 | Activada/caracterizada en §11: fallos tolerantes/silenciosos. Fijar politica por frontera en T14; cambio exige decision material |
| EXP-07 | Activada por test/contrato adyacente I-64. No se escribe durante Discovery; salir antes de producto con nuevo diff/coordinacion |
| EXP-08 | Activada: segunda PARRILLA, offset parametrico y consolidacion sin peralte. Determinar si las piezas concretas exponen esos limites; no reparar incidentalmente |
| EXP-09 | ABIERTA: M-02/03/05/06 UNKNOWN. Owner aporta alcance; Coordinator/Architect resuelven clasificacion y suficiencia |

Revision Coordinator: las nueve salidas y expansiones, incluidos negativos, se
revisaron. La pregunta "que expansion debio activarse y no se activo" identifica
EXP-04/06/07/08/09, ahora declaradas. No se da F0 por terminado ni se emite AGREED.

## 15. Decision Owner y camino minimo

[Proposal V1](../../initiatives/I-69-proposal-v1.md) contiene el camino minimo,
invariantes, pruebas, smoke y routing. Frozen: NO. No inventa piezas/dimensiones/
cantidades/codigos.

Faltan: altas de larguero (Id, texto, peraltes, mensula, bloques/mates o reutilizacion
explicita), mapa de renombres por Id, A/B/C para cada seguridad, relacion del
entrepaño con Parrilla y alcance de consumidores compartidos. Para B basta confirmar
cantidad manual total por rack y unidad; datos geometricos se piden solo si se elige C.

## 16. Indice de los 30 requisitos de primera devolucion

| Requisitos | Ubicacion |
|---|---|
| 1–6 preflight/main/Id/claim/hotspots/Workflow-I61 | §§1–3 |
| 7–9 fuente, Excel y schema | §4 |
| 10–12 modelo opcion/Id/display | §§4–5 |
| 13 persistencia | §6 |
| 14 geometria / 15 BOM | §§7/9 |
| 16 filtro / 17 inventario | §5 |
| 18 compatibilidad de renombres | §§6/8 |
| 19–21 modelo/persistencia/geometria-BOM de seguridad | §10 |
| 22 datos Owner | §15 y Proposal §2 |
| 23 camino minimo | Proposal §3 |
| 24 arquetipo | §14 |
| 25 Consumes/Extends/Introduces | §13 |
| 26 Freeze propuesto | Proposal §4 |
| 27 regresion | Proposal §5 |
| 28 Owner Validation | Proposal §6 |
| 29 routing | Proposal §8 |
| 30 autorizacion NO | §3, contrato y Proposal |

IMPLEMENTATION AUTHORIZATION = NO

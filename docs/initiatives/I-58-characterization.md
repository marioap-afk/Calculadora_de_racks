# I-58 — Characterization V2 y preparacion F1

Estado: plan completo para revision. Probe D/F0 ejecutable en evidencia; no sustituye la suite F1 sobre
carriers definitivos. No se implementa produccion. Identidad y resultados observados en
[I-58-evidence.md](../automation/evidence/I-58-evidence.md). Anexo congelable con Freeze draft V2.

## Escenarios por kind

Cada fila aplica separadamente a Dynamic, PushBack, Cantilever y Cabecera. `P` significa que el probe
D/F0 ejecuta la expectativa contra el port actual; `F1` exige fixture/prueba adicional tras consenso.
A=authored valido; A'=copia independiente igual; B=un campo authored cambiado; X=ilegible.
Para todos los resultados Divergent/Unreadable se exige authored null; Single requiere no-null y copia aislada.

| ID | Estimulo / oracle | Expected | Preparacion / RED |
|---|---|---|---|
| CT58-01 | A/A' con View/Section distintos | Single | P view; F1 incluir cada vista del kind y secciones |
| CT58-02 | A/A' con colocacion distinta | Single | P carrier local PlacementX; F1 scan de entrada excluye placement y control negativo |
| CT58-03 | A/A', catalogo/variables/effective distinto | Single | P contexto opaco diferente; F1 demostrar dos effective distintos fuera del comparator y cero llamadas a resolver dentro |
| CT58-04 | A/B un campo authored | Divergent | P: PalletDepth, RearTopeSaque, ColumnCentreSpacing, Height segun kind |
| CT58-05 | A/X JSON malformado; X en toda posicion | Unreadable | P una posicion; F1 todas y excepciones/entrada null |
| CT58-06 | major futuro en cada frontera | Unreadable | P wrapper; F1 envelope/payload/header anidado por separado |
| CT58-07 | minor futuro SIN ningun unknown visible | Unreadable | P wrapper; F1 todas las fronteras versionadas |
| CT58-08 | version malformada, numero, objeto, overflow, signo, segmentos extra | Unreadable | P string bad; F1 resto |
| CT58-09 | todas las permutaciones de A/A', A/B, A/B/X | outcome y authored tipado identicos | P inversa equal y mezcla; F1 propiedad exhaustiva de permutacion |
| CT58-10 | B primero y A/A' despues | Divergent | P; F1 verificar que no se retorna B ni A |
| CT58-11 | mayoria A/A'/A'' y minoria B | Divergent | P A/A'/B; F1 diferentes tamanos sin votacion |
| CT58-12 | A vs save/reopen real equivalente | Single | P RackProjectStore serialize/deserialize/serialize con fixture usable; F1 legacy y overrides |
| CT58-13 | kind externo incorrecto o payload de otro kind bajo kind correcto | Unreadable | P kind externo; F1 wrapper mismatch y payload cruzado |
| CT58-14 | authored nulo en fallos | null | P en todos los resultados; F1 Divergent real |
| CT58-15 | unknown root / ExtensionData en envelope, wrapper y payload | Unreadable | P unknown payload; F1 cada nivel y valores null/object/array |
| CT58-16 | unknown nested en cada tipo alcanzable | Unreadable | P Front/Structure/StationTopology/LeftPost; F1 recorrido manual del inventario completo |
| CT58-17 | A/B/X y X/B/A | Unreadable | P; F1 confirmar inspeccion de todos sin early Divergent |
| CT58-18 | raw duplicado o colision case-insensitive, nombre futuro, enum futuro | Unreadable | F1; exigir falso Single al desactivar guarda |
| CT58-19 | input vacio, incompleto, IDs mezclados, Cantilever Id ausente | Unreadable | F1; no generar GUID o hermana sintetica |
| CT58-20 | legacy ausente/default vs explicitado equivalente | Single | F1 segun tabla F-09; overrides null vs explicitos no se equiparan |
| CT58-21 | mutar resultado; repetir compare sobre input intacto | Single determinista y snapshots independientes | F1; referenciar input debe hacer fallar test |
| CT58-22 | input arbitrary Marker y Cama; Selective fixtures de I-57 | Unreadable para unsupported; Selective mismos outcomes | tests I-57 heredados; no sustituir sus oraculos por expectativa nueva |

Los P con Unreadable hoy pasan porque el port rechaza TODO. No acreditan schema, wrong kind ni
unknown validation. Los P view/placement/context solo demuestran el hueco del baseline; sus carriers
locales no son API propuesta. No afirmar que ya probaron no-leakage del futuro reader.
RED->GREEN funcional exige repetir con API final, asserts de valores authored tipados y mecanismo de
mutacion controlada que desactive cada regla despues de implementada. Cero seleccion es fallo.

## Mutation matrix representativa

Todas las mutaciones usan un par de fixtures inicialmente equivalente, cambian UNA intencion y esperan
Divergent con Authored=null salvo donde se declara Unreadable o Single. Incluir las mutaciones de cada
subtipo en F1; la lista representativa no exime el inventario de F-07. No usar reflection para mutar/comparar.

| ID | Kind | Mutacion | Oracle / riesgo cubierto |
|---|---|---|---|
| MM-D01 | Dynamic | Pallet.Depth/Weight/WeightUnit; BeamDepth; InOutBeamCatalogId | Divergent, identidad authored aunque no cambie frame |
| MM-D02 | Dynamic | Fronts count, orden o IsActive; Levels.ClearHeight | Divergent, colecciones/blank fronts/nested |
| MM-D03 | Dynamic | Modules.Length y IsManualOverride por separado; Notes | Divergent, automatico/manual no colapsados |
| MM-D04 | Dynamic | HeaderLineOverrides.Header.LeftPost.ReinforcementHeight; DerivedPostLineOverrides.Height | Divergent, authored de cabecera por linea |
| MM-D05 | Dynamic | BeamLengthOverride null->valor explicitado; FirstLevelDatum null->valor; DimensionViews null->0 | Divergent, heritage/legacy distinto de override |
| MM-D06 | Dynamic | SafetySelections.AuthoredSide; PostSides; TopeOffCells; BotaBPosts; BotaPieceId | Divergent; evitar effective side leakage y omisiones |
| MM-D07 | Dynamic | Bfr conocido cambia sin authored; Source Members/StartX cambian fuera del input | Single; excluidos derivados, sin geometria |
| MM-D09 | Dynamic/PushBack | Module.Header distinto con procedencia calculada; mismo cambio con custom | Single en calculada; Divergent en custom; unknown nested siempre Unreadable |
| MM-D10 | Dynamic/PushBack | Side effective distinto, AuthoredSide igual; luego cambiar AuthoredSide | Single en primero; Divergent en segundo; DerivedAisles no vacio ilegible |
| MM-D08 | Dynamic | peralte negativo que ToDesign descartaria; unknown nested en Module.Header | Unreadable; no comparar despues de saneamiento |
| MM-P01 | PushBack | Structure.Pallet/Fronts/Modules/Safety mutaciones MM-D | Divergent; estructura no es todo PushBack |
| MM-P02 | PushBack | HighEndBeamPeraltes [null,x] -> [x,null]; PalletsDeepOverrides; DefaultPalletsDeep null->valor | Divergent, posicion/nullable |
| MM-P03 | PushBack | DrawPallets un false->true; lista null->solo null | Divergent en primero, Single en segundo conforme fallback false |
| MM-P04 | PushBack | SideB null->lado; Fronts agujero null cambia de indice; FrontConfigs override | Divergent, presencia de lado y ranuras |
| MM-P05 | PushBack | Composite.Gap/CentralSeparator/StructureOverrideA/B; Topologies.Direction/CorridaDepth | Divergent; no omitir nested interface |
| MM-P06 | PushBack | AbsentSlotsA/B y RearTopeOffCells colecciones; DefensePieceId/RearTopePieceId | Divergent; campos fuera de Structure |
| MM-P07 | PushBack | DefaultTopology='FutureTopology', Direction futuro | Unreadable; mapper tolerant no acredita authored |
| MM-P08 | PushBack | SideB.Structure inventado o nested unknown en topologia | Unreadable; no duplicar estructura ni perder extension |
| MM-C01 | Cantilever | Name, StationCount, ColumnCentreSpacing | Divergent; Line.Id valido distinto ENTRE hermanas -> Divergent; outer!=inner por si solo admisible; Id interior ausente/vacio/malformado -> Unreadable |
| MM-C02 | Cantilever | StationTopology.FaceMode/SingleSide/ColumnHeight.Mode/ManualHeight | Divergent; modo manual y valor incluso inactivo |
| MM-C03 | Cantilever | DefaultArmTemplate.Body.CutLength/SectionId/Arrangement; MountingPlate.VerticalEndOffset null->valor | Divergent; arbol authored completo |
| MM-C04 | Cantilever | ArmCellOverrides agrega/quita/reordena; cambia StationIndex/Side/Arm.EndPlate | Divergent, nested/coleccion |
| MM-C05 | Cantilever | Bracing.AdvancedPanelSegments elevaciones/bracing; ManualPanelCount null->valor | Divergent; no usar geometria resuelta |
| MM-C06 | Cantilever | PlantaVisibility.ShowArms/ShowBraces | Divergent, preferencia authored aunque solo afecte una vista |
| MM-C07 | Cantilever | BaseFollowsColumn; Connection.Punches.Diameter; placa/gusset.Thickness | Divergent, subtemplates no omitidos |
| MM-C08 | Cantilever | solo dos punch margins retirados conocidos; IntervalCount derivado | Single tras validacion de tipo; llave futura vecina -> Unreadable |
| MM-H01 | Cabecera | Height/Depth/Units/Name; StandardBaselineId/Version | Divergent, no solo dimensiones |
| MM-H02 | Cabecera | Left/RightPost id/Description/refuerzo; Plate.ConnectionPointId | Divergent, lados y metadatos persistidos |
| MM-H03 | Cabecera | Plate.PeralteOverride null->valor actual heredado | Divergent, override explicito |
| MM-H04 | Cabecera | Horizontals agrega/elimina/reordena; Elevation/ProfileId/State/Notes | Divergent, lista y edicion manual |
| MM-H05 | Cabecera | Panels Lower/UpperHorizontalId/Arrangement/DiagonalDirection/IsException | Divergent, conectividad authored |
| MM-H06 | Cabecera | parametros troquel/celosia null->default legacy equivalente | Single; null->valor diferente Divergent |
| MM-H07 | Cabecera | Members/Exceptions cambian solo en modelo effective externo | Single; key no declarada en payload -> Unreadable |
| MM-H08 | Cabecera | unknown en Post o Panel; schema Header futuro bajo wrapper actual | Unreadable, cierre recursivo |

## Fixtures y control de oraculos

F1 debe producir fixtures del store actual con custom headers y miembros nested no-default; fixtures legacy
explicitos de cada version admitida; inputs unsafe originales (sin round-trip previo); y expected tipado.
Usar `RackProjectStore` para generar/comprobar save/reopen en TESTS, nunca en comparador productivo.
Para CT58-03 el test puede resolver dos contextos fuera del SUT y asegurar diferencia efectiva; el SUT no
recibe resolver/catalogo y un spy falla si se llama accidentalmente. Para campos cuyo effective no cambia,
mutar el authored inactivo debe seguir dando Divergent. Un guard de texto no prueba estas obligaciones.

Probe de esta entrega no usa equality alternativa; llama ports reales con output tipado y asserts de outcome.
El intento preliminar con fixture Dynamic sin Modules fallo durante setup (no RED admisible), fue corregido
agregando modulo authored. La corrida observada valida cada fixture con el store real antes del primer assert.
No se agregan tests rojos a las suites canonicas ni se marcan omitidos para fingir verde.

## C1 — identidad y seam publica (obligaciones futuras, NO ejecutadas contra AUTH-13)

| ID | Estimulo y oracle obligatorio | RED que debe ser observable en F1/F2 |
|---|---|---|
| CT58-23 | A nueva, B biblioteca, C reabierta+nueva hermana: mismo outer entre hermanas, mismo inner valido, outer!=inner. D restamp: ambos iguales. Con resto authored igual, Single en los cuatro | validar outer==inner indebidamente hace fallar A/B/C; no llamar al comparator actual un reader validado |
| CT58-24 | mismo outer con dos inner validos distintos -> Divergent; inner ausente/vacio/malformado -> Unreadable; dos outer distintos con inner igual de biblioteca -> Unreadable | ignorar inner, acuñar Id o agrupar por inner produce Single falso; permutar todos los casos |
| CT58-25 | cada kind raw original -> validacion -> canonical interna -> Single del tipo publico -> AUTH09 -> autoridad real de F-13 una vez -> AUTH10 sin re-resolve | compilar prueba con DynamicRackDesign/PushBackDesign/CantileverLineDesign/RackFrameConfiguration; devolver DTO incompatible no satisface contrato; spy de authority cuenta 1, fallos cuentan 0 |
| CT58-26 | todos los valores de F-07/09 y MM acreditados reaparecen en el dominio retornado; defaults/exclusiones F-08/09 simetricos | sabotear materializador de salida dejando igualdad intacta debe fallar; dos canonicals iguales no autorizan salida distinta, mapper que pierda override/null/orden falla |
| CT58-27 | unknown/schema futuro/malformed en raw antes de ToDomain/ToDesign; probar miembros que estos mappers borran | desactivar gate produce falso Single y debe fallar; no usar round-trip previo para fabricar fixture ilegible |
| CT58-28 | mutar CADA subarbol del dominio retornado y del snapshot de trabajo del resolver, repetir y permutar input | ningun cambio en hermana, otro resultado ni intencion retornada; salida efectiva o referencias compartidas falla |

CT58-25 se escribe por separado para cada tipo concreto y autoridad F-13, sin usar Marker como sustituto.
Cabecera prueba RefreshPhysicalModel sobre copia de trabajo, fuera del comparator; Cantilever prueba el
assembler y evita llamar despues al resolver otra vez. Los conteos son por invocacion de autoridad externa,
no por los pasos internos ya existentes de cada resolver. Ninguna prueba necesita un conversor universal.
CT58-26 inspecciona el DOMINIO retornado, no solo el DTO canonical ni el outcome. Incluye custom headers,
Safety.AuthoredSide, override null vs explicitado, SideB holes, Composite, templates/overrides Cantilever,
placas/paneles Cabecera. Ante valor no representable, Unreadable/finding; no inventar miembro de DTO/Domain.

## Diagnostico C1 actual y limites

El probe separado I-58-c1-probe ejecuta cuatro escenarios A/B/C/D y AUTH09 con assembler actual;
resultados, SHA y procedencia en evidencia. A/B/C usan WPF real sin AutoCAD; writer y restamp son metodos
puros extraidos con control de drift, despacho de kind limitado en harness. Hermanas A/B sintetizadas;
C usa LoadExisting/InsertPlanta real. Scan/transaccion DWG, comandos RACKEDITAR/RACKDUPLICAR y AUTH10
NO ejecutados. No confundir estas observaciones con CT58-23..28 verdes: AUTH13 sigue unsupported.
El probe V1 queda como historia de la version V1, no evidencia de los tipos publicos V2 ni GREEN funcional.

EXP-01 efectivo CLASS A OPEN / STOP. Propuesta B limitada pendiente Coordinator + Architect.
CR58-01/02 requieren revision de V2; F1 NOT OPEN; IMPLEMENTATION AUTHORIZATION = NO.

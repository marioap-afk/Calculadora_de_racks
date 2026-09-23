# I-58 — Consensus Freeze DRAFT V1

Frozen: NO
Status: DRAFT FOR COORDINATOR + ARCHITECT REVIEW
Coordinator: PENDING
Architect: PENDING
IMPLEMENTATION AUTHORIZATION = NO

Este documento completo es la version a revisar. Su identidad commit/ruta/blob vive en
[la evidencia](../automation/evidence/I-58-evidence.md). No es un Freeze aprobado ni modifica I-57 V5.
Autoridades por referencia: ADR-0034 §§5,8,9; ADR-0044; I-57 Proposal V5/R3;
INITIATIVE_LIFECYCLE §§3-8; WORKFLOW §§4,10,11; AGENTS (pruebas/evidencia).
Solo despues de ambos AGREED sobre la misma identidad procede el commit de Freeze previsto en lifecycle §6.
Lineas de cabecera candidatas a cambio administrativo: `Frozen: NO` y `Status: DRAFT FOR COORDINATOR + ARCHITECT REVIEW`.
Los revisores deben enumerar literalmente lo permitido en el acuerdo. Ninguna clausula cambia en ese commit.

## 1. Resultado, ownership y limites

F-01. AUTH-13 pertenece a Shared View Foundation/I-57 conforme ADR-0044. I-58 es su evolucion V2;
los consumidores no duplican igualdad ni eligen authored. Conservar interfaz/outcomes existentes:
Single=1, Divergent=2, Unreadable=3. Divergent/Unreadable llevan Authored=null, nunca resultado parcial.

F-02. Alcance: Dynamic, PushBack, Cantilever y Cabecera. Selective sigue su autoridad vigente, sin
reescritura; Cama sigue Unreadable. AUTH-15 queda fuera. Mantener genericos unsupported compatibles
para inputs arbitrarios; el soporte nuevo tiene entradas y salidas concretas por kind, sin casts desde object.
No se cambian enum values, persisted schemas, DTO fields, stores, SchemaVersionPolicy, geometria, BOM,
resolvers, UI, comandos, Insertar, ID18, ID19 o RACKMIRROR policy. Sin NuGet ni nueva arquitectura universal.
No tocar main, ramas consumidoras, integration/I-57, R3 ni Proposal V5 historica.

## 2. Entrada y validacion completa

F-03. La entrada nueva expresa RackId y snapshot completo de hermanas persistidas, incluyendo entradas
ilegibles y evidencia de completitud. Cada entrada conserva identidad de origen y texto ORIGINAL del sobre
embebido con Design; no basta un DTO ya deserializado porque habria perdido nested unknowns. Es un carrier
runtime de Application, no formato persistido ni scan AutoCAD nuevo. Nombres de tipos no congelados.
Sin prueba de completitud, entrada null/vacia, hermana null, envelope/payload ausente, mezcla de RackId/kind,
lectura fallida, ambiguedad por duplicados o estado unsafe: Unreadable. No omitir hermanas silenciosamente.
RackId se exige no vacio y coherente; Cantilever.Line.Id requiere GUID no vacio y concordancia con envelope.
La pertenencia fisica la acredita el consumidor; Foundation valida la coherencia de todas las entradas dadas.

F-04. Secuencia: capturar snapshot inmutable -> validar envelope y forma de wrapper -> schema/unknown
recursivo -> leer DTO tipado -> canonicalizar authored sin resolver -> comparar TODOS -> construir resultado.
El uso de JsonDocument para sintaxis/presencia/unknown es lectura, NUNCA igualdad raw JSON. La igualdad no
recorre reflection, JsonElement, diccionarios de propiedades, texto serializado, hash de JSON ni object.
No usar RackProjectStore.Deserialize dentro de comparador: BuildDynamic/BuildHeader reconstruyen fisico.
Se reutilizan DTOs y contratos del serializer, no sus efectos ni fallbacks tolerantes inseguros.

F-05. Primero acreditar legibilidad de todas las hermanas. Precedencia global:
Unreadable > Divergent > Single. Encontrar dos authored distintos no permite retornar Divergent antes de
revisar las restantes. Con todas legibles, cualquier diferencia authored produce Divergent; solo una clase
de equivalencia produce Single. Cero hermanas nunca Single. Orden de entrada, vista frontal, primera,
ultima y mayoria no otorgan autoridad. Duplicados del mismo payload no votan ni ocultan otra hermana.

## 3. Canonical forms TIPADAS y frontera authored/effective

F-06. Tabla de salida (todos snapshots independientes, sin mutar input):

| Kind | Forma authored tipada | Construccion y comparacion |
|---|---|---|
| Dynamic | `DynamicRackSystemDocument` canonical | copia explicita de intencion persistida; scalar/nullables/listas tipadas; excluir derivados enumerados en F-08 |
| PushBack | `PushBackDesignDocument` canonical | Structure bajo F-06 Dynamic; Fronts, SideB, Composite y restantes campos completos; ExtensionData vacia acreditada |
| Cantilever | `CantileverLineDesign` canonical, fuente `CantileverLineDocument.Line` | copia explicita del arbol authored; no defaults aleatorios ni DeepCopy que sanee antes del gate; metadata de schema se valida fuera |
| Cabecera | `RackFrameProjectDocument` canonical | posts, plates, horizontals, panels y parametros completos; sin RefreshPhysicalModel |

Los DTO existentes son carrier tipado, no una promesa de que Equals compare valores. Implementar equality
explicita por kind y subestructura; no `object.Equals`, reference equality, reflection equality ni conversion
a JSON para comparar. Sin geometria, tolerancia de coordenadas ni redondeo de dimensiones. Numeros finitos
se comparan por su valor tipado exacto (0 y -0 son iguales); strings authored Ordinal, sin Trim/upper arbitrario.
Identificadores de schema/kind solo se normalizan conforme a su contrato de lectura; enums conocidos por su
valor. DimensionViews conserva cualquier Int32 (incluidos bits desconocidos) conforme ADR-0035: no es enum
cerrado. El scanner no debe confundir ese contrato conocido con un futuro enum ilegible.

F-07. Inventario obligatorio de igualdad:

- Dynamic: tarima completa y unidad; fondos/niveles/altura/datum; peraltes y tolerancia; ids de perfiles;
  PostPeralte; overrides de separadores; poste derivado y alturas; override altura cabecera; anotaciones y cotas;
  SafetySelections completa; Fronts con IsActive, count, niveles, fondos, inicio, overrides, niveles anidados;
  Modules con ModuleId, Kind, Length, IsCalculated, IsManualOverride, procedencia, Notes y Header;
  HeaderLineOverrides por PostIndex/ModuleId/Header; DerivedPostLineOverrides por PostIndex/Height.
  Load/level nullable, manual/auto e inherit/override son authored, aunque hoy produzcan la misma geometria.
- SafetySelections: TODOS los campos publicos persistidos de SafetySelectionDocument y sus subdocumentos
  (PostSides, AuthoredSide, DerivedAisles, tope/off-cells, desviador, defensa, guia, parrilla, botas A/B,
  placements, piece ids y declaraciones por lado). No reconstruirlo desde seguridad effective: From aplica
  AuthoredPostSides y puede descartar estado. No se cambia SelectiveSafetySelection.DeepCopy.
- PushBack: Structure completa; Fronts/HighEndBeamPeraltes/DefaultPalletsDeep/PalletsDeepOverrides/DrawPallets;
  LegacyHighEndBeamPeralte; RearTopeSaque/PieceId/OffCells; DefensePieceId; SideB completo (incluido IsPresent y
  agujeros null de Fronts/FrontConfigs); Composite Gap/CentralSeparator/StructureOverrideA/B,
  DefaultTopology/Direction, Topologies con Frente/Level/Topology/Direction/CorridaDepth, AbsentSlotsA/B.
- Cantilever: Id, Name, StationCount, ColumnCentreSpacing; StationTopology (face/side, ColumnBaseTemplate,
  LevelCount, FirstLevelPunchIndex, RequestedClearHeight, TopClearFactor, ColumnHeight);
  templates de columna/base/connection/punches/plates/gusset/BaseFollowsColumn;
  DefaultArmTemplate Body/MountingPlate/EndPlate; ArmCellOverrides por StationIndex/LevelIndex/Side/Arm;
  Bracing SeparatorSectionId/PanelCountMode/ManualPanelCount/BracedPanelHeight/CentralEmptySpaceHeight/
  BraceKind/BraceSectionId/ColdRolled.Diameter/PanelLayoutMode/AdvancedPanelSegments;
  segmentos StartElevation/EndElevation/BracingMode; PlantaVisibility.ShowArms/ShowBraces.
- Cabecera: Name/Units/Height/Depth/PostPeralte; todos los parametros troquel/celosia/panel;
  StandardBaselineId/Version; Left/RightPost (catalogo, descripcion, refuerzo, id/altura refuerzo);
  Left/RightBasePlate (id, descripcion, connection point, PeralteOverride);
  Horizontals (Id, Number, Elevation, ProfileId, Quantity, MountingFace, State, Notes, IsStandard);
  Panels (PanelId, Number, extremos horizontales, Arrangement, MountingFace, DiagonalProfileId,
  DiagonalDirection, connection points, IsStandard/IsException).

El inventario fuente adjunto en evidencia permite cotejar omissions. Un campo nuevo en cualquiera de estos
arboles requiere actualizar cobertura antes de admitir Single; la guarda estructural es complementaria,
no reemplaza mutacion conductual. Se comparan tambien valores authored inactivos: no se borran por no dibujarse.

F-08. Exclusiones cerradas:

| Dato | Razon y tratamiento |
|---|---|
| Envelope View/Section y transformacion fisica | contexto de vista/placement, fuera de igualdad |
| Catalogos, ProjectVariables/effective result, builders, Members | inputs/resultados de resolver; no forman parte del carrier authored |
| Dynamic Module.Header con UseCalculatedHeaderConfiguration=true | snapshot recalculado por DynamicRackSystemResolver desde catalogo; validar schema/unknown primero, canonicalizar Header=null sin llamar builder. Con flag legacy ausente y Header presente es personalizada: comparar completa |
| Safety.Side con AuthoredSide presente | PushBackSafetyAuthority.RestrictToLowEnd conserva authored y colapsa Side; comparar lado elegido AuthoredSide ?? Side con fallback legacy conocido. Canonical Side y AuthoredSide reflejan ese unico lado authored; diferencia solo en Side efectivo no diverge |
| Safety.DerivedAisles no vacio en payload | From no lo emite con contenido y ToDomain no lo restaura: estado conocido pero no acreditado; Unreadable antes de mapper, nunca ignorarlo. Ausente/null/vacio sin intencion son admitidos |
| Dynamic Front.Bfr | ToDesign no lo usa; From(design) lo calcula; reconocer key y tipo pero canonicalizar a ausente sin calcular geometria |
| Dynamic AllowsNonNestedDepthRanges | derivado runtime del compositor PushBack, no persistido; key inyectada en DTO es unknown -> Unreadable |
| Cabecera Members, panel members/elevaciones derivadas, Exceptions runtime | no persistidos; no reconstruir ni comparar; keys no declaradas en documento -> Unreadable |
| Cantilever IntervalCount | getter derivado que serializer puede emitir; validar tipo, omitir igualdad; no usar como autoridad |
| Cantilever PanelSegment.Height | JsonIgnore, derivado; no incluir ni emitir en canonical; si aparece como key desconocida, Unreadable |
| Cantilever Connection.Punches.ColumnBottomPlateEndOffset y ColumnTopPunchOffset | retirados explicitamente por store; admitir solo en esa ruta con tipo legacy, omitir y fijar a default retirado; no extrapolar a otros unknowns |
| SchemaVersion | controla legibilidad, no es authored una vez admitida version conocida; salida usa version corriente conocida |

Envelope Id/Kind acreditan identidad, no se usan para elegir hermana. Envelope Name es authored de rack:
nombres distintos -> Divergent. Cantilever.Name y Header.Name interiores tambien se comparan, no se
reparan a partir del exterior. CustomProperties conocida tiene autoridad propia (ADR-0039/I-54): no entra
en esta igualdad ni se devuelve; el consumidor conserva su gate independiente. Unknowns del sobre no son
CustomProperties y fallan cerrado. Esta separacion no altera el comportamiento Selective ya integrado.

## 4. Legacy, nullable y colecciones

F-09. No usar `From(ToDomain(raw))` como canonicalizacion general. Defaults solo cuando representan el mismo
estado legacy documentado, antes de comparar y de forma identica para cada hermana. Nunca invocar resolver.

| Area | Regla congelable propuesta |
|---|---|
| Dynamic nullable con fallback fijo | LoadLevels/FirstLevelHeight/BeamDepth/PalletTolerance toman defaults legacy del mapper; NumberFronts/NumberLevels/DrawRackName false; AnnotationScale ausente=1; PostPeralte null=0; Dimensions ausente=None. Valores presentes invalidos que el mapper borraria: Unreadable |
| Dynamic front | IsActive ausente=true; LoadLevels/PalletsDeep ausentes heredan authored global segun ToDesign; DepthStartPosition ausente=1. No introducir frente cuando falta toda la coleccion: forma insuficiente=Unreadable |
| Overrides Dynamic | FirstLevelDatum, BeamLengthOverride, FirstLevelHeight de frente, Level overrides, Separator overrides, alturas derivadas/manuales y DimensionViews: conservar null/inherit distinto de valor explicito |
| Module.Header | procedencia ausente usa regla legacy Header presente=>personalizada; Header ausente=>calculada. Contradiccion explicita custom sin Header: Unreadable, no reparacion |
| PushBack escalares | LegacyHighEndBeamPeralte/RearTopeSaque ausentes usan defaults del mapper; SideB null sigue ausencia, no fabricar lado; Composite null sigue ausencia, no fabricar interfaz. Enum malformado/futuro presente no cae a default |
| PushBack override cells | DefaultPalletsDeep/PalletsDeepOverrides/HighEndBeamPeraltes null heredan y siguen null; DrawPallets null equivale false por contrato. Mantener indices y agujeros SideB; no compactar por Where |
| Cabecera | Units ausente/null usa in; campos nullable troquel/celosia/panel usan defaults de RackFrameConfiguration; PostPeralte null=0. Plate.PeralteOverride null significa heredar y no se iguala a valor explicitado |
| Cantilever | defaults deterministas de campos ausentes son los del diseño; Id debe existir; null en objeto estructural obligatorio no se sanea; override Arm null no fabrica template |

Listas ordered de front/module/level/horizontal/panel/segmento conservan orden, multiplicidad e indices.
Overrides indexados y OffCells conservan tuplas completas; en esta entrega no se introduce equivalencia por
sort/deduplicacion de colecciones authored: reordenarlas puede ser Divergent conservador. El orden de HERMANAS
siempre es irrelevante. Coleccion opcional ausente/null se representa como vacia cuando significa sin overrides;
entradas null solo se admiten si el contrato declara agujero/heritage, nunca se filtran.
PushBack listas solo-null que el writer omite se canonicalizan como ausencia de overrides, conservando
longitud posicional cuando hay algun override. Trailing null sin semantica fuera de rango no crea nueva intencion;
F1 debe probar ambos extremos contra writer real. Ante caso no comprendido: Unreadable y finding, no fallback.

No normalizar strings authored por conveniencia. F1 debe demostrar con writer real que un valor aceptado
conserva la forma canonical tras guardar/reabrir. Si una conversion pierde intencion, rechazar antes o revisar
Freeze por autoridades competentes; no ampliar silenciosamente equivalencia para obtener verde.

## 5. Schema y unknown fail-closed

F-10. Matriz schema/unknown de Discovery es parte normativa propuesta de este draft (se congela junto con
sus reglas explicitadas aqui): schema del envelope 1.0, proyecto wrapper 1.0/2.0 conocidos, payloads versionados
1.0; Dynamic no versiona por si mismo. Ausente/null adopta legacy conocido. Presente malformado, version
no comprendida, future minor o major => Unreadable incluso si se ven solo campos conocidos.
No se aplica la permisividad IsReadable del store al gate de autoridad. No se cambia esa politica compartida.

F-11. Inspeccionar raw ORIGINAL, duplicados/case collisions, tipo de cada miembro y cierre de cada subarbol.
Cualquier unknown fuera de F-08, a cualquier profundidad, o ExtensionData no vacia => Unreadable.
No basta mirar root ExtensionData despues de Deserialize: los otros DTO pierden sus unknowns.
El parser puede reutilizar System.Text.Json y opciones locales de lectura; equality sigue tipada.
Un reader con `JsonUnmappedMemberHandling.Disallow` por si solo NO basta para tipos con ExtensionData ni
para keys reconocidas que los mappers descartan. Ningun reader futuro, version checker o copier se hace
publico como framework transversal. Limitarlo a estos cuatro adapters y sus subarboles compartidos.
No se serializa ni sobrescribe el source como parte de comparar. Los serializers actuales permanecen intactos.

## 6. Resultado Single determinista

F-12. Tras validar todas las entradas y probar equivalencia, construir un NUEVO snapshot del unico valor
canonical consensuado, sin referencia mutable a una hermana. Se permite un anchor interno para contrastar
valores, nunca seleccionarlo antes de validar todo ni retornarlo como representante. Su uso no otorga autoridad.
Salida reconstruida campo a campo con los valores acreditados y normalizaciones F-08/09; no SourceEnvelope,
no ExtensionData, no catalogo, no Guid.NewGuid, hora o random. Permutar hermanas mantiene outcome y todos
los valores tipados retornados. Mutar resultado no cambia input, otra salida ni otra hermana.
Schema de salida corriente solo en documento runtime; no instruye al consumidor a guardar perdiendo metadata.
Consumidor conserva sobre acreditado y aplica su propia persistencia; AUTH-13 no compone ni escribe sobres.

## 7. Obligaciones invariante -> prueba, RED y gates

La [matriz F1](I-58-characterization.md) asigna CT58-01..22 y MM por kind. Es anexo normativo de diseño,
congelable junto a este draft; hechos observados y hashes solo en evidencia. Antes de Freeze, los revisores
acuerdan tambien la identidad de este anexo y Discovery §matriz schema; no existe clausula mutable indirecta.

| Invariante | Obligacion conductual | RED esperado antes de implementacion |
|---|---|---|
| F-01/02 | Selective delega; Cama y genericos arbitrarios quedan cerrados | regresion si se altera autoridad existente |
| F-03/04 | input completo y raw conservado; wrong kind/id/truncated/null/duplicate reject | reader que salta hermanas o borra unknown falla |
| F-05 | permutations, first, majority y mezcla divergent+unreadable | Divergent/Single mal elegido falla |
| F-06/07 | mutation matrix escalares/listas/nested/nullable de cada kind | omitir cada campo mutado produce Single incorrecto |
| F-08/09 | vista/placement/context independientes; fallbacks y legacy save/reopen | comparar effective o raw serializado produce falso Divergent |
| F-10/11 | cada schema layer, future minor sin unknown, unknown nested y root | reader tolerante produce Single incorrecto |
| F-12 | resultado tipado determinista, null failures, deep isolation | retorno de primera instancia o Guid generado falla |

D/F0 (esta ejecucion): bootstrap + Discovery + draft + review package + probe diagnostico actual. Sin declarar
consenso, gate funcional cerrado o autorizacion productiva. F1: solo despues del acuerdo exacto, materializar
suite de characterization/RED sobre carriers finales, cubrir inventario y registrar conteos >0. F1 no implementa
produccion. F2: adapters tipados por kind, RED->GREEN; puede particionarse solo por comportamiento revisable,
no por archivos. Cierre de gate bajo lifecycle §7 y AGENTS. F3: integracion de seam neutral/compatibilidad,
conformidad completa y READY segun lifecycle §8. Candidate/integracion/evidencia/tag exclusivamente por
WORKFLOW/AGENTS; no se abre ahora. No debe quedar REQUIRED ni EXP A abierta al avanzar.

## 8. Matriz OV y asignacion

| Escenario | Unidad / momento | Evidencia |
|---|---|---|
| OV58-01 authored igual/diferente/ilegible de cuatro kinds | I-58 F1/F2 | automatizada pura, mutacion y lectura original; no requiere AutoCAD |
| OV58-02 independencia view/placement/effective y deep isolation | I-58 F2 | automatizada con fixtures/control negativo; no resolver en comparator |
| OV58-03 guardar/reabrir legacy y unknown/future | I-58 F1/F2 | serializer real + fixtures historicos comprendidos; matriz recursiva |
| OV58-04 inspeccion de regresion dibujo/edicion/save-reopen cuatro kinds | Owner de I-58, Candidate si naturaleza real del diff activa validacion manual | guia manual §7.1; DLL/SHA/AutoCAD/biblioteca exactos; baseline no se atribuye a comparator sin consumer |
| OV-ID18 y comportamiento de G12 | I-55 despues de integrar I-58 | consumer real y Owner Validation propios; NO se acreditan en I-58 |

La metadata del contrato no exime Owner Validation. Antes de Candidate, Coordinator/Architect determinan
aplicabilidad de OV58-04 contra el diff real; no declarar PASS manual ni inventar escenario UI para autoridad
pura aun no consumida. Si se activa, se prepara/ejecuta segun guia sin cambiar alcance reservado al Owner.

## 9. Consumer unlock y consentimiento

I-55 G12 solo consume tras integracion COMPLETA de I-58 en main, verificacion post-merge y tag anotado
`integration/I-58` valido bajo WORKFLOW §11.6. Claim, draft, RED, push, CI verde y AGREED no desbloquean G12.
I-55 conserva batches, transacciones, remedio de divergence, mensajes y policies. I-52 conserva RACKMIRROR
policy y AUTH-15. I-58 no implementa ninguno. No merge en esta ejecucion.

Coordinator = PENDING
Architect = PENDING
IMPLEMENTATION AUTHORIZATION = NO

# I-57 — Discovery de Shared View Foundation

```text
Estado                 = DISCOVERY DOCUMENTAL COMPLETO
Base observada         = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
I-57 observada         = 623aa8042c004cdca44ed519ed2256d932edcc1e
Fecha                  = 2026-09-15
Proposal               = TODAVIA NO PUBLICADA
Implementation         = BLOCKED
F1                     = NOT OPEN
```

Este Discovery audita el arbol real. La especificacion y el mapa publicados por I-55 son entradas de
diseno, no autoridad sobre I-57. No se modifico codigo ni se implementaron caracterizaciones.

## 1. Metodo y limites de evidencia

Se verificaron las fuentes globales y los context packs declarados por el contrato; los contratos y registros
de I-49, I-52, I-55 e I-56; R1 por commit y blob exactos; y los productores, adaptadores y pruebas del arbol de
`main`. El preflight remoto encontro I-52 en `9e6724175c151a1d217c507a16840969ec768980`: su respuesta a R1 ya
es autoridad remota y deja CR-SVF-I52-01..06 abiertas. R1 sigue registrada solo por I-55 y no es efectiva.

Las conclusiones de coordenadas describen convenciones que el codigo expresa. CT-05 debera fijar valores con
fixtures antes de extraer una autoridad. Cuando no existe una prueba que distinga dos interpretaciones, este
documento marca la incertidumbre y exige fallo cerrado; no convierte una lectura de codigo en golden master.

## 2. Separacion comprobada: sintaxis, disponibilidad y politica

Las tres capas son viables, pero hoy aparecen mezcladas en los comandos:

1. **Sintaxis:** `RackEmbedDocument.View` y `Section` son strings/enteros persistidos. Las constantes viven en
   `RackEmbedDocument`; los comandos comparan tokens con reglas distintas y Push Back delega parte del codec a
   `PushBackSystemFrontalBuilder.EncodeSection/DecodeSection`.
2. **Disponibilidad:** conteos de fondos, `Cortes(...).PostIndex`, lados Push Back y estaciones Cantilever se
   obtienen del sistema resuelto y de sus builders. Son hechos puros.
3. **Politica:** `RACKEDITAR` decide coerciones legacy, vistas fantasma, mensajes, insercion y borrado. I-52 e
   I-55 agregaran sus propias reglas de exposicion, espejo, anclaje y materializacion.

Sitios mezclados: `RackSelectivoCommands`, `RackDinamicoCommands`, `RackPushBackCommands`,
`RackCantileverCommands`, `RackCabeceraCommands`, `RackCamaCommands`, `RackCommandSupport` y el barrido de
`ProjectVariableMutationExecutor`. La extraccion debe conservar las decisiones legacy en esos consumidores.

## 3. Auditoria AUTH-01..AUTH-14

| AUTH | Productores y capa actuales | Consumidores actuales / futuros | Duplicacion, pruebas y guardas | Persistencia / AutoCAD | Riesgo y neutralidad | Disposicion |
|---|---|---|---|---|---|---|
| 01 | `DimensionViewKind` en Application/Shared; `CantileverViewKind` es una camara de builder con `AdapterSection` | politica de cotas; ventanas WPF / I-52, I-55 y codec | `DimensionViewPolicyTests`; renombrar alcanzaria ventanas y guardas sin cambiar semantica | no se serializa; ninguna API AutoCAD | un nombre nuevo duplicaria el enum; Cantilever no puede fusionarse | **REUSE** `DimensionViewKind`; mapear camaras |
| 02 | no existe direccion neutral; cada comando interpreta `View/Section` | seis comandos y persistencia / I-52, I-55 | direcciones repetidas de forma implicita; tests de persistencia y vistas por sistema | sobre vigente permanece igual | neutral si expresa solo kind+variant; no es indice de UI | **EXTRACT** valor semantico en Application |
| 03 | constantes del sobre, comparaciones en comandos y codec frontal Push Back dentro de un builder | edicion, barridos / I-52, I-55 | reglas de case, vacio y legacy dispersas; `RackEmbedDocumentTests`, tests de comandos por guardas | lee/escribe `View/Section`; sin AutoCAD si recibe valores | alto riesgo de cambiar coerciones; codec total debe reportar disposicion, no aceptar | **EXTRACT** despues de CT-04; retirar productores solo tras paridad |
| 04 | conteos/layouts y `Cortes` en builders/resolvers de cada sistema | comandos / I-52, I-55 | pruebas de builders ya fijan gran parte; falta matriz neutral | no persiste; puro | neutral como hechos tipados y codigos; mensajes/eleccion fuera | **EXTRACT** mediante adaptadores por kind |
| 05 | coordenadas en builders, layouts, `Bounds` Cantilever y geometria de cabecera/cama | dibujos y BOM indirectamente / I-52 centro, I-55 extremos | muchas pruebas geométricas; no existe descriptor comun | no persiste; puro | riesgo maximo: bounds dibujados no siempre equivalen a ejes fisicos | **EXTRACT** solo tras CT-05 por familia/variante |
| 06 | barridos AutoCAD crean snapshots; Application clasifica en `RackDuplicationPlan`, PV y propiedades | duplicar, PV, propiedades / I-52, I-55 | seleccion y pertenencia repetidas; `RackDuplicationPlanTests` y guardas de fuentes | `ObjectId`, ModelSpace y Xref son Plugin; clasificacion puede ser pura | snapshot fisico debe permanecer en Plugin; ninguna transaccion en el core | **ADAPT**: DTO puro + clasificador, probe Plugin |
| 07 | `RackDuplicationPlan` clasifica y tambien agrupa, asigna identidad/nombre y aplica reglas Selectivas | `RACKDUPLICAR` / I-52 e I-55 | suite extensa de duplicacion; mezclar todo filtraria politica de COPY | sin AutoCAD una vez proyectada seleccion | solo el prefiltro comun es neutral; identidad, copy count, agrupacion y nombres no | **ADAPT** nucleo minimo; mantener fachada de duplicacion |
| 08 | `Transform2D` puro ya gobierna composicion, espejo, escala y determinante | geometria / I-52 e I-55 | `GeometryPrimitivesTests`; futuras CT-GEO | no persiste; AutoCAD convierte hacia/desde el valor | duplicar transformacion seria dos autoridades; falta resultado de descomposicion | **REUSE** `Transform2D` + **ADAPT** hechos de colocacion |
| 09 | resolvers por sistema; `IRackKindHandler` y `KindHandlerRegistry` orquestan en Plugin. Selectivo resuelve efectivo dentro del handler | BOM, edicion/dibujo / I-52, I-55 | `KindHandlerGuardSourceTests`, pruebas de resolvers y BOM | handler usa `Document/ObjectId`; resolucion pura es Application | mover el handler viola capas y ADR-0034; contrato comun puede devolver resultado tipado | **KEEP IN PLACE BEHIND PORT** con adaptadores por kind |
| 10 | builders puros producen `HeaderRunPlan` o `CantileverViewPlan`; DrawServices y comando Cantilever los invocan | materializadores / I-52, I-55 | `DrawServicePlanBaselineTests` y suites por builder | builders puros; materializacion Plugin | un plan universal reescribiria tipos maduros; se necesita orquestacion, no otro builder | **ADAPT** registro/fachadas hacia builders existentes |
| 11 | helpers `BlockName` por DrawService y `SuggestName/Sanitize/UniqueBlockName` Cantilever | materializadores, renombrado / I-52, I-55 | nombres repetidos con diferencias; baselines parciales | nombre base puro; unicidad consulta `BlockTable` | centralizar exacto; sufijo por colision permanece Plugin | **EXTRACT** autoridad de nombre base |
| 12 | nombres en `HeaderBlockInstance`; `BlockLibraryImporter.EnsureForPlan` aplana y consulta/importa; Cantilever son curvas | DrawServices / I-52, I-55 | pruebas de planes; importer no cubierto por Core | requisito puro; existencia e importacion usan `Database`, `BlockTable`, DWG | no toda geometria exige bloque; consulta no debe importar ni normalizar | **EXTRACT** requirement + **KEEP query BEHIND PORT** Plugin |
| 13 | `SelectiveAuthoredAuthority` compara estructuralmente DTO persistido; otros kinds deserializan por handler sin contrato comun | PV/BOM / I-52, I-55 | pruebas Selectivas preservan include-by-default; `BomAuthoredAuthority` cubre resultados | DTO en Application; lectura de siblings en Plugin | un comparador JSON generico perderia schema/kind; cada adapter conserva codec propio | **ADAPT** contrato por kind; reutilizar comparador Selectivo |
| 14 | pruebas existentes dispersas por persistencia, vistas, builders, geometria, duplicacion y handlers | I-57 y consumidores | crear pruebas duplicadas aumentaria mantenimiento | CT puras salvo probes/guardas Plugin | es trabajo de prueba, no autoridad productiva | **CHARACTERIZE ONLY** y reutilizar fixtures existentes |

### Dependencias y cambios observables potenciales

- 02 depende de 01; 03 depende de 01-02; 04 depende de 02 y 09.
- 05 depende de 02, 04 y de CT-05. 08 reutiliza geometria, pero no depende de cambiar 05.
- 06 alimenta 07 y 13. 07 no puede apropiarse de identidad o politica de duplicacion.
- 10 depende de 04-05 y 09; 11-12 se adjuntan al resultado preparado sin rehacer geometria.
- 13 depende de 06 y de adaptadores por kind. 14 precede toda extraccion.
- Los cambios observables que deben quedar en cero son geometria, BOM, nombres, orden de insercion, mensajes,
  coerciones legacy, persistencia, numero de resoluciones efectivas, importacion y transacciones.

## 4. Fuentes de plan: autoridad geometrica y preparacion

La autoridad geometrica ya esta distribuida correctamente por familia: `SelectiveFrontalBuilder`,
`SelectivePlantaBuilder`, `SelectiveLateralBuilder`; `DynamicSystemFrontalBuilder`,
`DynamicSystemPlantaBuilder`, `DynamicSystemLateralBuilder`; equivalentes Push Back; layouts de cabecera;
`FlowBedLateralBuilder`; y `CantileverViewPlanBuilder`. Los primeros devuelven instancias o `HeaderRunPlan`;
Cantilever devuelve curvas y bounds. Esas formas distintas son datos reales, no una inconsistencia a ocultar.

La autoridad que falta es **preparar una vista**: validar una direccion disponible, delegar al builder existente,
adjuntar frame, nombre base y requisitos de biblioteca, y devolver un resultado cerrado. Se recomienda un registro de
adaptadores por kind con un sobre discriminado. El payload del plan conserva su tipo existente. DrawServices siguen
siendo materializadores Plugin; no calculan geometria ni eligen politica de producto.

Los helpers de nombre locales y el `SuggestName` Cantilever son productores duplicados. `UniqueBlockName` no lo es:
resuelve colisiones reales del `BlockTable` y debe permanecer en Plugin. `BlockLibraryImporter` tampoco es autoridad de
plan; importa best-effort después de que el plan declare requisitos.

## 5. Auditoria fisica de frame y span

Convencion fisica propuesta para caracterizar: R = corrida, D = profundidad, H = altura. Cada descriptor registra
`AxisMap`, el origen fisico expresado en coordenadas locales, `[K_min,K_max]` por eje representado, centro exacto
`(min+max)/2`, convencion de extremo y offset de variante. El centro no incorpora correcciones de I-52.

| Familia/vista | Hecho observado | Incertidumbre que CT-05 debe cerrar |
|---|---|---|
| Selectivo frontal por fondo | local +X sigue R, +Y H; el fondo se recorta con `FondoSystemView(k)` | eje de poste vs envolvente de piezas en extremos; legacy `Section=-1` |
| Selectivo planta Whole | local +X sigue D y +Y R; `SelectiveDepthLayout.MasterGrid` alinea fondos | cara exterior vs eje de poste delantero/trasero |
| Selectivo lateral por poste | local +X sigue D desde `anchorOffset`; +Y H; `PostIndex` es indice de rejilla real | signo y origen del offset para postes que no son el primero |
| Dynamic frontal Exit/Entrance | local +X sigue R y +Y H; ambos extremos usan la misma rejilla transversal | tramo fisico aunque una frontera en blanco no dibuje poste |
| Dynamic planta | local +X sigue D/flujo y +Y R; cabeceras se colocan por modulo | extremo Exit/Entrance usado como D=0 y envolvente de grupos |
| Dynamic lateral por poste | local +X sigue D y +Y H; cortes se seleccionan por `PostIndex`, no ordinal | rango del corte cuando la cobertura longitudinal es parcial |
| Push Back frontal | reutiliza frontal dinamico; variante codifica end y side; compuesto expone cuatro cortes | lado B invierte pertenencia, no debe colapsarse a A |
| Push Back planta/lateral | planta deriva del sistema compuesto; lateral se direcciona por PostIndex real | origen bajo por lado y huecos de indices en cortes |
| Cabecera planta/lateral | layouts parten en `(0,0)`; planta +X D, lateral +X D y +Y H | extremos por cara/eje y efectos de mates catalogados |
| Cantilever frontal/planta | `CantileverViewPlan.Bounds`; variante Whole | bounds de curva no sustituyen automaticamente span fisico |
| Cantilever lateral | variante `Station(s)`; builder verifica rango | `AdapterSection` queda fuera de taxonomia; visibilidad de planta no se corrige aqui |
| Cama lateral | `FlowBedLateralBuilder` parte en `(0,0)`, +X longitud y +Y altura | sobre historico puede no llevar `View`; no existen frontal/planta |

Conclusiones obligatorias: Push Back usa el `PostIndex` contenido en cada corte; no se puede inferir del ordinal. Una
planta Cantilever potencialmente vacia es comportamiento a caracterizar, no a corregir. Cama es un special case con
una unica vista soportada y codec legacy propio.

## 6. Caracterizaciones disenadas

| CT | Proposito y codigo observado | Fixture y expected output | Consumidor / gate | Fallo cerrado y reutilizacion |
|---|---|---|---|---|
| CT-04 | fijar codec total sobre `RackEmbedDocument` y comandos | matriz por seis kinds: null/vacio/case/espacios/desconocido y particiones de Section; direccion+disposicion exactas | codec, I-52/I-55 / F1-F2 | celda sin evidencia impide F2; extender `RackEmbedDocumentTests`, no copiar sus round-trips |
| CT-05 | fijar frames contra builders/layouts | un fixture pequeno y no simetrico por familia y variante; ejes, origen, min/max, centro, offset y bounds dibujados | frame y planes / F1-F3 | ambiguedad de extremo bloquea esa familia; reutilizar suites geometricas como fixture builders |
| CT-16 | separar clasificacion neutral de semantica COPY | selecciones con ModelSpace/Xref/no-id/kind desconocido/miembros mixtos/divergencia y orden estable | scan/selection / F1-F4 | desconocido no se vuelve ausencia; extraer casos comunes de `RackDuplicationPlanTests` |
| CT-RES | numero y resultado de resoluciones por kind | un diseño valido y uno bloqueado por kind; Selectivo con variable; resultado, diagnosticos y exactamente una resolucion efectiva | adapters, BOM / F1-F5 | resolver ilegible/bloqueado no produce plan; reutilizar tests de resolver/BOM |
| CT-PLAN | demostrar delegacion y paridad de payload | mismos fixtures de `DrawServicePlanBaselineTests`; tipo de payload, instancias/curvas, frame y requirements | preparacion / F1-F5 | adapter sin builder o payload incongruente falla; no serializar a formato generico |
| CT-NAME | congelar cada nombre base | nombres vacio, espacios, caracteres invalidos, views y variantes; cadena previa exacta antes de colision | naming / F1-F5 | caso no caracterizado conserva productor legacy; `UniqueBlockName` queda fuera |
| CT-SCAN | separar probe fisico de clasificador puro | snapshots de definicion/referencia, xref, espacio, envelope legible/ilegible y varias copias | scan / F1-F4 | unreadable/unknown explicitos; guarda textual solo para frontera AutoCAD |
| CT-GEO | fijar descomposicion sobre `Transform2D` | identidad, traslacion, cuarto/medio giro, espejo, escala uniforme/no uniforme y degenerada; facts exactos | placement / F1-F4 | transformacion no descomponible produce resultado tipado; tolerancia la inyecta consumidor |
| CT-BLK | requisitos puros y consulta separada | planes con bloque valido/vacio/duplicado, curvas Cantilever y anotaciones; claves requeridas exactas, distinct case-insensitive | plan/import / F1-F6 | clave requerida vacia o ausente no se cumple; query no importa |
| CT-AUTH | equivalencia authored por kind | iguales con orden JSON distinto, schema/extensiones distintas, sibling ilegible y kind incorrecto | comparator / F1-F6 | divergent/unreadable nunca eligen sibling; reutilizar Selective authority tests |

T0 ejecutara solo la CT focal con conteo mayor que cero; T1 las suites de sistema impactadas; T2 sobre el commit
Candidato limpio exige Core Full, UI Full, builds Debug UI/Plugin y CI push exacta; T3 valida merge SHA y cobertura;
T4 es Owner Validation en AutoCAD cuando la implementacion cambie rutas de dibujo aunque prometa paridad.

## 7. ADR-0034 y Resolve

ADR-0034 esta aceptado y es compatible. `SelectiveKindHandler.BuildBom` deserializa authored, invoca
`SelectiveEffectiveDesignResolver.Resolve` una sola vez y luego `SelectiveGeometryResolver.Resolve`. La foundation
puede definir `ResolveResult` y un port por kind; el adaptador Selectivo delega a esa secuencia y el handler sigue
siendo la autoridad que decide el momento y el numero de invocaciones. No se mueve `IRackKindHandler` a Application
porque usa AutoCAD. **No existe conflicto material y no se reabre ADR-0034.**

## 8. Reconciliacion R1 y necesidad de R2

R1 es inmutable y describe una foundation aun sin ID ni claim. I-52 ya publico una respuesta que acepta el mecanismo
neutral pero solicita seis cambios propios: ownership neutral de X-2; ownership neutral de X-8/CT-05; marcar como
provisionales las carreras de extraccion anteriores; mantener path/namespace/API pendientes hasta consenso; consumo
solo con Integration SHA; y prohibicion bilateral de duplicar mientras no haya reconciliacion. Las solicitudes
`CR-SVF-01..06` de R1 son otro conjunto: tolerancia, titularidad X-1/X-3/X-7, precondiciones Integrated, ownership de
CT-04/05/16, centro compartido y query de bloques. Ademas, este Discovery
cambia disposiciones materiales frente al borrador: reutiliza AUTH-01 y AUTH-08, reduce AUTH-07 y adapta AUTH-10.
Por ello hacia falta **R2**, publicada por I-57 sin editar R1. La primera R2 atribuyo mal los dos conjuntos de CR y
quedo inmutable; la correccion se publica como **R3**. R3 no sera efectiva hasta que I-52, I-55 e I-57 registren el
mismo commit exacto.

## 9. Revision adversarial

Se atacaron los veinte riesgos pedidos. Correcciones BLOCKER/HIGH incorporadas:

- se retiro el renombre obligatorio de `DimensionViewKind` y el framework de plan uniforme;
- se separaron builders de preparacion, requisitos de consulta/importacion y nombre base de colision;
- se redujo AUTH-07 para no absorber identidad/duplicacion y se reutilizo `Transform2D`;
- se fijo la frontera handler/adaptador de ADR-0034 y la responsabilidad Plugin de scans/AutoCAD;
- se exigio CT-05 por familia para legacy Selectivo/Dynamic, PostIndex Push Back, visibilidad Cantilever y Cama;
- se separaron las CR originales de R1 de `CR-SVF-I52-01..06`, y se mantuvo consumo solo desde `main` integrado.

No queda BLOCKER/HIGH documental conocido. Quedan abiertas las mediciones que deliberadamente pertenecen a F1.

## 10. Archivos calientes, material abierto y conclusion

Hot files futuros: `DimensionViewPolicy.cs`; `RackDuplicationPlan.cs`; `KindHandlers/*`;
`Rack*Commands.cs`; DrawServices; `CantileverViewMaterializer.cs`; pruebas baseline; y ventanas WPF si una migracion
las alcanzara. I-49 mantiene autoridad final sobre archivos Selectivos que toque; I-52/I-55 no deben implementar
estas autoridades hasta integración I-57.

**Open Material para consenso:** forma exacta del sobre discriminado de Plan; inventario cerrado de coerciones CT-04;
convencion de extremos CT-05; contrato de tolerancia inyectada CT-GEO; comparadores disponibles para los cinco kinds
no Selectivos. **Open Minor:** nombres definitivos de namespaces/tipos, orden de codigos diagnosticos y ubicacion de
fixtures compartidos.

Discovery concluye que la foundation es viable y neutral con las disposiciones anteriores. No abre F1 ni autoriza
produccion.

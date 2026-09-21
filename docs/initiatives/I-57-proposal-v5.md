# I-57 — Shared View Foundation — Proposal V5

```text
Estado                    = PROPOSAL V5 — REVIEW REQUIRED
Foundation Coordinator    = REVIEW REQUIRED
Foundation Architect      = REVIEW REQUIRED
Foundation Consensus      = NOT REACHED
Foundation Implementation = BLOCKED
F1                        = COMPLETE
F2                        = BLOCKED
F3                        = NOT OPEN
ADR                       = ADR-0044 · ACCEPTED
Reconciliation            = R3 · EFFECTIVE
Coordinator input         = MATERIAL CONTRADICTION at F2 start · Proposal V5 REQUIRED
Architect input           = AGREED on V4 · exact-SHA review required on V5
```

## 1. Resultado y frontera

I-57 integrara en `main` una autoridad pequena para describir, resolver y preparar vistas existentes como
hechos neutrales. I-52 consumira esos hechos para el espejo e I-55 para colocacion/proyeccion. Ningun contrato
decide que vista exponer, aceptar, omitir, reflejar, superponer o materializar. Dibujo, BOM, nombres,
persistencia y comportamiento legacy permanecen iguales. Los builders y handlers existentes conservan su
autoridad geometrica y de sistema.

## 2. Alcance final

| AUTH | Decision | Entrega minima |
|---|---|---|
| 01 | REUSE | `DimensionViewKind`; mapeos totales para tokens y camaras |
| 02 | EXTRACT | address y variantes por valor, base 0, sin indices de UI |
| 03 | EXTRACT | codec total `View/Section` hacia address+disposition; sin policy |
| 04 | EXTRACT | availability sobre sistema resuelto mediante adapters por kind |
| 05 | EXTRACT | frame fisico, spans, centro y offset medidos por CT-05 |
| 06 | ADAPT | snapshot/clasificador puro; probe AutoCAD permanece Plugin |
| 07 | ADAPT | nucleo minimo; `RackDuplicationPlan` retiene COPY, identidad, grupos y nombres |
| 08 | REUSE+ADAPT | `Transform2D` mas resultado tipado de transform facts |
| 09 | KEEP IN PLACE BEHIND PORT | contrato comun; handler/resolver por kind no se mueve |
| 10 | ADAPT | preparation delega a builders y conserva payload tipado existente |
| 11 | EXTRACT | nombre base puro; colision sigue en Plugin |
| 12 | EXTRACT+PORT | requirements puros; query AutoCAD separado; importer intacto |
| 13 | ADAPT | comparator port por kind; reutiliza autoridad Selectiva |
| 14 | CHARACTERIZE ONLY | diez CT sin duplicar suites existentes |

AUTH-15 permanece fuera: crear definiciones caller-owned sigue propuesto a I-52.

## 3. Arquitectura minima

```text
View/Section -> syntax codec -> semantic address + disposition
resolved design + address -> availability facts
kind adapter -> existing resolver -> resolved result
resolved result + address -> existing builder -> typed plan payload
preparation result = address + frame + payload + base name + block requirements
consumer policy -> Plugin materializer/transaction
```

- **Application/Shared o Application/Views:** values, codec, facts, frames, classifier, ports, registry, names y
  requirements; cero WPF/AutoCAD.
- **Application/Systems/<Kind>:** adapters hacia resolvers, builders y comparators vigentes.
- **Plugin:** probes `Database/ObjectId`, `BlockTable`, importacion, materializacion y transacciones.
- **Consumidores/UI:** exposicion, seleccion, anclaje, espejo, UX y mensajes.

### 3.1 Contrato cerrado del payload preparado

```text
PreparedViewPlan {
    Address
    Frame
    BaseName
    BlockRequirements
    Payload
}
```

`Payload` es un carrier discriminado y tipado por kind. Conserva el tipo de plan existente que produce el adapter
del kind; la foundation no interpreta su geometria genericamente. El consumer/materializer conoce el carrier que
le corresponde. Un kind sin carrier o adapter valido devuelve fallo tipado. Se prohiben JSON, reflection y
`object` opaco sin contrato. `HeaderRunPlan`, `CantileverViewPlan` y los demas planes vigentes no se convierten en
un modelo geometrico universal y siguen siendo la unica autoridad geometrica. La forma C# concreta —union,
interface o records— es una eleccion de F5 que debe satisfacer este contrato, no una decision de arquitectura
pendiente.

### 3.2 Espacios de identidad de bloque

`PreparedViewPlan.BaseName` y `BlockRequirements[*].Key` coexisten en el resultado, pero son ortogonales y
pertenecen a espacios de identidad distintos:

```text
PreparedViewPlan.BaseName       -> GeneratedViewDefinitionNameSpace
BlockRequirements[*].Key        -> LibraryPieceDefinitionNameSpace
```

`LibraryBlockRequirement.Key` identifica una definicion reutilizable de pieza en la biblioteca. Proviene del
plan/builder tipado vigente, conserva exactamente su key semantica —incluidos caracteres validos reales como
`.`— y viaja por el pipeline de requirements. Si el consumer permite importar, `Ensure/import` ocurre antes del
query que produce availability definitiva; si no permite importar, el query directo produce ese hecho final.
No es una definicion generada de vista, un nombre de rack, un nombre sugerido ni un nombre unico final.

`PreparedViewPlan.BaseName` proviene de AUTH-11 y es solo la base para nombrar la definicion generada de la vista.
El Plugin la pasa despues por la resolucion de colisiones. No se consulta en la biblioteca, no produce
requirements y no sirve como fallback cuando falta una requirement key. Una library key tampoco se convierte en
nombre de vista. La implementacion puede introducir wrappers como `LibraryBlockKey` y
`GeneratedViewBaseName`, pero V5 no los exige: si elige strings debe preservar la misma separacion semantica.

## 4. Contratos centrales

`RackViewAddress` combina `DimensionViewKind` con Whole, Fondo, Post, FlowEnd, PushBackCut o Station.
`AdapterSection` Cantilever no es vista de rack. El codec total opera sobre la sintaxis persistida y devuelve
la sintaxis original, una address opcional, disposition y un codigo de coercion opcional. Reporta hechos, nunca
permiso para continuar. Availability devuelve Available, VariantNotPresent, SystemDoesNotSupportKind o
Unavailable con codigo estable.

`RackViewFrame` contiene mapa de ejes locales a R/D/H, origen fisico local, spans, convencion de extremos,
centro exacto `(min+max)/2`, offset de variante y bounds dibujados cuando difieren. I-52 e I-55 consumen el mismo
descriptor y eligen su punto por policy propia.

Plugin proyecta entidades a snapshots sin perder Unknown/Unreadable/Xref/space/identity; Application clasifica.
El nucleo neutral no asigna RackId, nombres ni cantidad de copias. `Transform2D` permanece intacto; la tolerancia
de descomposicion se inyecta desde el consumidor.

`Resolve` es port/result, no algoritmo universal. El adapter Selectivo delega desde el handler y conserva una
resolucion efectiva por rack conforme ADR-0034. `PrepareView` llama al builder existente una vez. AUTH-11 produce
`BaseName`; AUTH-12 produce `LibraryBlockRequirements` y el port de query del Plugin. No existe dependencia
semantica AUTH-11 -> AUTH-12 ni AUTH-12 -> AUTH-11. Requirements puros, query e importacion permanecen separados.

El comparator port devuelve Single, Divergent o Unreadable. Cada kind controla DTO/schema; Selectivo reutiliza su
comparacion estructural include-by-default. Nunca se elige una hermana.

### 4.1 Codec corregido, coercion y canonicalizacion

La contradiccion material de F2 demostro que `Invalid` no puede generalizarse a todos los tokens desconocidos o
Sections no canonicos. La disposition se define asi:

| Disposition | Address | Contrato |
|---|---|---|
| `Canonical` | presente | representacion exacta que emite el writer canonico |
| `Canonicalizable` | presente | diferencia puramente representacional; puede codificarse en forma canonica sin aplicar fallback productivo |
| `Coerced` | presente | un reader vigente transforma deterministamente la entrada a otra address soportada; lleva codigo estable |
| `Invalid` | ausente | no existe interpretacion persistida segura y neutral |

`Coerced` gana sobre `Canonicalizable` cuando ocurren ambas condiciones. El resultado conserva la sintaxis original,
por lo que el casing o token recibido sigue observable. Ninguna disposition autoriza continuar: cada consumer
conserva su policy vigente.

El decode conceptual queda cerrado, sin exigir los nombres C#:

```text
DecodedViewAddress {
    OriginalSyntax { Kind, View, Section }
    SemanticAddress?
    Disposition
    CoercionCode?
}
```

`Invalid` carece de address. Las otras tres dispositions siempre la llevan. El encode recibe una address semantica y
produce su sintaxis canonica. Solo `Canonical` promete round-trip canonico; una entrada `Canonicalizable` o `Coerced`
puede codificarse de forma distinta. Decode/encode no escriben el DWG ni cambian schema.

CT-04 V2 vive en `tests/RackCad.Tests/Fixtures/I57/ct04-v2-coercion-matrix.json`. Sus codigos minimos son:

- Selectivo: default no-lateral/no-planta a frontal; Section frontal negativa a fondo 0; combinacion de ambas.
- Dinamico: default no-frontal/no-planta a lateral; Section lateral negativa a poste 0; Section frontal no Entrance
  a Exit; combinacion default lateral/poste 0.
- Whole: Section ignorada donde el reader vigente la ignora.
- Cabecera: no-planta a lateral.
- Cama: descriptor alterno ignorado; `null/-1` es su encoding historico canonico.

Push Back permanece estricto en persisted edit: sus secciones frontales validas son 0..3 y el fallback interno de
`DecodeSection` no se alcanza despues del preflight. Cantilever permanece estricto y `AdapterSection` queda fuera.

### 4.2 Frontera codec / availability / consumer policy

Availability ocurre solo despues de un decode con address:

```text
persisted syntax
    -> syntax codec
    -> semantic address + disposition
    -> availability(resolved system, address)
    -> consumer policy
```

Una address `Fondo(999)`, `Post(999)` o `Station(999)` puede ser sintacticamente `Canonical`; si el sistema resuelto
no contiene la variante, AUTH-04 devuelve `VariantNotPresent`. Una address valida contra otro kind devuelve
`SystemDoesNotSupportKind`. Los adapters solo observan hechos ya caracterizados; no eligen otra address, no llaman
resolvers y no deciden skip, prompt, erase, abort, warning o continuation.

Las solicitudes interactivas no son sintaxis persistida. Elegir un fondo o corte mediante prompt, o usar el builder
lateral completo cuando una solicitud Dinamica llega con postIndex negativo, permanece policy del comando. En
particular, el redraw Dinamico legacy que resulta en `Post(0)` no convierte el prompt o whole-system insertion en
regla del codec.

### 4.3 Invariantes de identidad de bloques

- **BLK-ID-1:** `LibraryBlockRequirement.Key` y `BaseName` pertenecen a espacios de identidad distintos.
- **BLK-ID-2:** `BaseName` no puede sustituir una `LibraryBlockRequirement.Key`, ni esta sustituir el nombre de vista.
- **BLK-ID-3:** `BaseName` no se deriva del conjunto de requirements.
- **BLK-ID-4:** resolver una colision de `BaseName` no agrega, quita, reordena ni modifica `BlockRequirements`.
- **BLK-ID-5:** el query del Plugin recibe unicamente `LibraryBlockRequirement.Key`.
- **BLK-ID-6:** el importer recibe unicamente requirements derivados del plan vigente, nunca nombres generados de vista.

Los dos pipelines no se cruzan:

```text
consumer permits import:
typed plan -> LibraryBlockRequirements -> Ensure/import -> Plugin Library Query -> final availability facts

consumer does not permit import:
typed plan -> LibraryBlockRequirements -> Plugin Library Query -> final availability facts

AUTH-11 -> BaseName -> Plugin collision resolution -> generated view definition name
```

### 4.4 Query, importer y availability definitiva

El Plugin Library Query es puro: no importa, crea, repara, normaliza, sanitiza ni muta `Database`; solo observa
availability sobre el estado actual. El importer es una operacion separada. Si el consumer permite importar, el
resultado definitivo se consulta despues de `Ensure/import`. Una observacion previa puede servir de diagnostico,
pero no gobierna materializacion ni error final y un `MISSING` preliminar no queda sticky. Si el consumer no
permite importar, el query directo es definitivo. Una importacion intentada tampoco declara exito: el query final
puede seguir produciendo `MISSING`.

## 5. Caracterizacion y estrategia de pruebas

F1 permanece `COMPLETE` como registro historico. Al abrir F2 se detecto que su declaracion global para CT-04 no
describia varias coerciones vigentes. La sesion se detuvo antes de modificar produccion y publico evidencia adicional,
sin reescribir F1: `I-57-ct04-v2-correction-evidence.md`, la matriz ejecutable CT-04 V2 y su suite focal.

Esta correccion puede crear o modificar solo tests, fixtures y documentos de caracterizacion. No extrae autoridades,
mueve enums, crea codec productivo, cambia resolver/builder/reader ni altera comportamiento observable. CT-05,
CT-16, CT-RES, CT-PLAN, CT-NAME, CT-SCAN, CT-GEO, CT-BLK y CT-AUTH permanecen como en V4.

| Nivel | Evidencia |
|---|---|
| T0 focal | pruebas afectadas con conteo esperado mayor que cero |
| T1 impacto | sistemas afectados, Core local y UI segun LC-UI |
| T2 candidato | Core/UI Full locales, Debug UI/Plugin y CI push 4/4 sobre SHA exacto limpio |
| T3 integracion | CI y cobertura del merge SHA conforme WORKFLOW |
| T4 Owner | AutoCAD 2025 sobre SHA exacto cuando cambien rutas observables |

Una CT sin expected demostrable bloquea su AUTH. La contradiccion encontrada ya ejecuto el STOP previsto por V4.
V5 y CT-04 V2 requieren review exacta del Coordinator y Architect antes de reabrir F2; nunca se parchea producto
para hacer coincidir una caracterizacion incorrecta.

### 5.1 Estado de Characterization Blockers

| Blocker | Expected/fail-closed | Owner | Estado / gate |
|---|---|---|---|
| matriz CT-04 completa | cada categoria produce address/disposition/codigo y separa policy/availability | I-57 | CORRECTED IN V5; review requerida antes de F2 |
| convenciones de extremos CT-05 | cada fixture fija eje/cara, span, centro y offset; ambiguedad = frame no disponible | I-57 | CLOSED por F1; requerido por F3 |
| comparators no Selectivos | cada kind demuestra Single/Divergent/Unreadable; falta de comparator = Unreadable | I-57, adapter por kind | CLOSED para caracterizacion; requerido por F6 |
| inventario de productores legacy | lista cerrada por autoridad y consumidor; productor no censado bloquea delegacion | I-57 | CLOSED por F1; AUTH-01..04 reauditadas en CT-04 V2 |

### 5.2 CT-BLK — separacion de identidades

CT-BLK reutiliza fixtures existentes cuando sea posible y cubre, como minimo:

- **BLK-IDENTITY-01:** `BaseName` es un nombre valido de vista generada ausente de la biblioteca y dos o mas
  requirement keys reales estan presentes. Expected: el query recibe solo esas keys, todas quedan `FOUND`, la
  ausencia de `BaseName` no produce error y la operacion de query es `SUCCESS`. Falla si se consulta `BaseName` o
  falta una key requerida real.
- **BLK-IDENTITY-02:** `BaseName` necesita suffix por colision. Expected: se obtiene el nombre generado unico y
  `BlockRequirements` permanece byte/string-equivalent, sin requirement nuevo derivado del nombre final.
- **BLK-IDENTITY-03:** una library key contiene un caracter permitido real como `.`. Expected: la key permanece
  sin sanitizar y el naming sanitizer de vista nunca se aplica sobre ella.

- **BLK-AVAILABILITY-01:** una requirement key existe en la fuente de importacion, falta inicialmente en el
  target y el consumer permite importar. Expected: un pre-query opcional puede producir `MISSING`;
  `Ensure/import` corre; el query final produce `FOUND`; availability final es `FOUND`; el resultado preliminar no
  queda sticky; el query no importa; y la key permanece byte/string-identical.
- **BLK-AVAILABILITY-02:** una requirement falta y el consumer no permite importar. Expected: el query directo y
  availability final producen `MISSING`, sin side effect.
- **BLK-AVAILABILITY-03:** `Ensure/import` se intenta pero la pieza permanece ausente. Expected: el query final
  produce `MISSING`, el fact/error tipado no declara exito y el query no repara ni importa.

CT-BLK conserva ademas sus casos validos, vacios, whitespace y duplicados. Un plan de geometria pura, incluido
Cantilever, puede producir cero requirements; eso no autoriza consultar `BaseName`. Los planes de piezas, incluido
`HeaderRunPlan`, obtienen las keys de sus instancias tipadas. La capa pura de requirements puede aplicar la
normalizacion que su contrato declare antes del port; el query no normaliza. Importer y query permanecen separados,
y la availability final refleja el estado observado despues del intento de importacion cuando este aplica.

## 6. Migracion y rollback

1. Revisar y acordar V5 + CT-04 V2 sobre el mismo SHA exacto; F2 permanece bloqueado hasta entonces.
2. Introducir values/codec/availability y delegar lectores uno por uno con pruebas de paridad por ruta.
3. Crear frames solo desde CT-05.
4. Separar snapshots/clasificacion y transform facts.
5. Introducir Resolve/PrepareView adapters y centralizar nombres.
6. Agregar requirements/query y comparators.
7. Demostrar cero productores paralelos y producir Candidato.
8. Integrar I-57; despues I-52/I-55 rebasan desde `main`.

Cada cambio de consumidor retira su productor legacy en el mismo commit verde. El rollback revierte el gate
completo. No hay schema nuevo ni migracion de DWG; `View/Section` permanece persistido.

## 7. Gates refinados

| Gate | Contenido | Salida |
|---|---|---|
| F1 | solo tests, fixtures y helpers de caracterizacion | COMPLETE; evidencia original historica + CT-04 V2 adicional; cero producto cambiado |
| F2 | AUTH-01..04 | BLOCKED hasta Coordinator `AGREED` + Architect `AGREED` sobre V5 exacta; despues: taxonomia reutilizada, address/codec/availability unicos, paridad por reader, cero policy leak y cero cambio observable |
| F3 | AUTH-05 | frames por seis kinds, CT-05 y OV-FND-01 |
| F4 | AUTH-06..08 | scan/selection/placement neutrales, CT-16/SCAN/GEO |
| F5 | AUTH-09..11 y 10 | Resolve/PrepareView/names, ADR-0034 intacto, CT-RES/PLAN/NAME, OV-FND-02/03 |
| F6 | AUTH-12..13 | requirements/query/comparators, BLK-ID-1..6, BLK-AVAILABILITY-01..03, CT-BLK/AUTH |
| F7 | Candidato | productores unicos, T2 y OV-FND completa |
| F8 | integracion | rebase si aplica, merge autorizado, T3 y recibo con Integration SHA |

Ningun gate se abre al publicar la Proposal. Un rebase crea SHA nuevo y exige evidencia nueva.

## 8. Compatibilidad, ADR y validacion

R3 incorpora las solicitudes autorales CR-SVF-I52-01..06: ownership neutral de X-2 y X-8/CT-05; carreras previas
marcadas provisionales; path/namespace/API sujetos al consenso; consumo solo Integrated; y prohibicion bilateral de
duplicar. Por separado resuelve CR-SVF-01..06 de R1: tolerancia inyectada; I-57 titular neutral de X-1/X-3/X-7;
CT compartidas en I-57; centro comun; y query separado de importer. I-52 conserva reflexion, read-set,
exposicion y AUTH-15. I-55 conserva ID17/18/19, anclaje, cola, UX y materializacion.

R2 queda preservada como la version inicial publicada; R3 es inmutable y `EFFECTIVE`, con blob
`cd42db03becff42f98b047e61c46689c17a69670`. V5 no la edita ni cambia ownership X-1..X-8: solo corrige la
clasificacion interna de AUTH-03. R4 no es necesaria.

ADR-0044 esta `ACCEPTED` y gobierna ownership, hechos neutrales, separacion syntax/availability/policy y consumo
desde main. V5 refuerza esa separacion: no cambia la decision del ADR ni requiere procedimiento de enmienda.
ADR-0034 permanece intacto. Aunque la meta sea paridad, F3/F5/F7 requieren Owner Validation porque cambian rutas
de dibujo. El Candidato se crea limpio antes de T2/T4.

## 9. Estado de decisiones y preguntas

**Open Material arquitectonico dentro del draft: NONE.** La contradiccion CT-04 queda resuelta conceptualmente y de
forma falsable. Queda pendiente la review, no una decision tecnica sin propuesta. **Open Minor: NONE.** Los codigos
de coercion minimos quedan fijados por la matriz; los nombres C# concretos siguen siendo eleccion de F2.

V5 preserva AUTH-01..02 y AUTH-04..14, payload, BLK-ID/AVAILABILITY, ADR-0034, frame, Resolve/Plan, selection,
comparators y gates de V4. El unico cambio material es AUTH-03/CT-04 derivado del STOP. Coordinator y Architect
deben revisar el mismo SHA exacto V5. Su acuerdo previo sobre V4 no se extrapola.

Hasta ambos veredictos `AGREED` sobre V5: Consensus NOT REACHED, Implementation BLOCKED, F2 BLOCKED y F3 NOT OPEN.

## 10. Adversarial check focal de V5

PASS documental y ejecutable contra CT-04 V2:

1. unknown Selective view queda `Coerced` a frontal, no `Invalid`.
2. null Selective view con Section negativa conserva el doble fallback a frontal/fondo 0.
3. Selective Section -1 queda coercionada a fondo 0.
4. Selective fondo imposible decodifica canonicamente y falla despues como `VariantNotPresent`.
5. unknown Dynamic view en redraw queda coercionada a lateral.
6. Dynamic missing view y Section negativa queda coercionada a Post(0).
7. Dynamic Entrance/Exit conservan los valores 1/0 vigentes.
8. Dynamic frontal Section desconocida queda coercionada a Exit.
9. prompt/whole-system lateral de insercion no se incorpora al codec.
10. Cabecera unknown/null/no-planta queda coercionada a lateral.
11. Cama `null/-1` es canonica y cualquier descriptor alterno observado se ignora con codigo.
12. Push Back conserva side/end 0..3 y no expone el fallback inalcanzable de `DecodeSection`.
13. Cantilever Station es base 0; `AdapterSection` queda fuera.
14. sintaxis valida con variante ausente llega a AUTH-04.
15. disposition no obliga a un consumer a aceptar; la policy vigente permanece.
16. codec no conoce prompts, mensajes, erase, abort ni estado resuelto.
17. encode despues de coercion produce forma canonica sin reescribir DWG.
18. case-only puede ser `Canonicalizable`; whitespace no se recorta y sigue la ruta observada por kind.
19. future view usa coercion solo en kinds cuyo reader real la demuestra; Push Back/Cantilever lo dejan `Invalid`.
20. unknown/null/blank/whitespace kind queda `Invalid`; case-only es reconocible pero el dispatch ordinal vigente
    puede seguir rechazandolo por policy de entrada.

PASS de regresion: V5 no cambia `src/`, schema, readers, builders, resolvers, geometria, BOM, naming, importacion ni
transacciones. R3 conserva blob/ownership y sigue efectiva. ADR-0044 sigue aceptado y ADR-0034 intacto. F1 permanece
completo, F2 bloqueado y F3 no abierto.

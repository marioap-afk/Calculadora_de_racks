# I-57 — Shared View Foundation — Proposal V4

```text
Estado                    = PROPOSAL V4 — NOT CONSENSUS
Foundation Coordinator    = REVIEW REQUIRED
Foundation Architect      = REVIEW REQUIRED
Foundation Consensus      = NOT REACHED
Foundation Implementation = BLOCKED
F1                        = NOT OPEN
ADR                       = ADR-0044 · PROPOSED
Reconciliation            = R3 · PROPOSED BY I-57 · NOT EFFECTIVE
Coordinator input         = AGREED WITH ARCHITECT FINDING on V3 · I57-AR3-01 applied here
Architect input           = CHANGES REQUIRED on V3 · exact-SHA review required on V4
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
`GeneratedViewBaseName`, pero V4 no los exige: si elige strings debe preservar la misma separacion semantica.

## 4. Contratos centrales

`RackViewAddress` combina `DimensionViewKind` con Whole, Fondo, Post, FlowEnd, PushBackCut o Station.
`AdapterSection` Cantilever no es vista de rack. El codec es total y devuelve `Canonical`, `Canonicalizable`,
`Coerced` o `Invalid`; reporta hechos, nunca permiso para continuar. Availability devuelve Available,
VariantNotPresent, SystemDoesNotSupportKind o Unavailable con codigo estable.

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

### 4.1 Invariantes de identidad de bloques

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

### 4.2 Query, importer y availability definitiva

El Plugin Library Query es puro: no importa, crea, repara, normaliza, sanitiza ni muta `Database`; solo observa
availability sobre el estado actual. El importer es una operacion separada. Si el consumer permite importar, el
resultado definitivo se consulta despues de `Ensure/import`. Una observacion previa puede servir de diagnostico,
pero no gobierna materializacion ni error final y un `MISSING` preliminar no queda sticky. Si el consumer no
permite importar, el query directo es definitivo. Una importacion intentada tampoco declara exito: el query final
puede seguir produciendo `MISSING`.

## 5. Caracterizacion y estrategia de pruebas

F1 es exclusivamente implementacion de caracterizaciones. Puede crear o modificar tests, fixtures y helpers de
caracterizacion estrictamente necesarios. No puede extraer autoridades productivas, mover enums, reemplazar el
codec productivo, cambiar resolvers/builders ni alterar comportamiento observable. Implementa CT-04, CT-05,
CT-16, CT-RES, CT-PLAN, CT-NAME, CT-SCAN, CT-GEO, CT-BLK y CT-AUTH. Se amplian pruebas existentes cuando ya
expresan el comportamiento; las guardas textuales se reservan para fronteras Plugin que Core no puede cargar.

| Nivel | Evidencia |
|---|---|
| T0 focal | pruebas afectadas con conteo esperado mayor que cero |
| T1 impacto | sistemas afectados, Core local y UI segun LC-UI |
| T2 candidato | Core/UI Full locales, Debug UI/Plugin y CI push 4/4 sobre SHA exacto limpio |
| T3 integracion | CI y cobertura del merge SHA conforme WORKFLOW |
| T4 Owner | AutoCAD 2025 sobre SHA exacto cuando cambien rutas observables |

Una CT sin expected demostrable bloquea su AUTH. Si contradice materialmente la Proposal: STOP, publicar Proposal
V5 y repetir consenso; nunca parchear producto dentro de F1.

### 5.1 Characterization Blockers de F1

| Blocker | Expected/fail-closed | Owner | Gate | Si contradice V4 |
|---|---|---|---|---|
| matriz CT-04 completa | cada celda produce una sola address/disposition; sin evidencia = Invalid/STOP | I-57 | F1, requerido por F2 | STOP → Proposal V5 |
| convenciones de extremos CT-05 | cada fixture fija eje/cara, span, centro y offset; ambiguedad = frame no disponible | I-57 | F1, requerido por F3 | STOP → Proposal V5 |
| comparators no Selectivos | cada kind demuestra Single/Divergent/Unreadable; falta de comparator = Unreadable | I-57, adapter por kind | F1, requerido por F6 | STOP → Proposal V5 |
| inventario de productores legacy | lista cerrada por autoridad y consumidor; productor no censado bloquea delegacion | I-57 | F1, requerido por F2-F6 | STOP → Proposal V5 |

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

1. Caracterizar sin mover productores.
2. Introducir values/codec/availability y delegar lectores uno por uno.
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
| F1 | solo tests, fixtures y helpers de caracterizacion | Proposal exacta AGREED por Coordinator y Architect; ADR-0044 ACCEPTED; latest reconciliation Rn EFFECTIVE, con el mismo SHA exacto registrado por I-52, I-55 e I-57 y ninguna CR material abierta contra ese Rn. Hoy latest Rn = R3, aun NOT EFFECTIVE. Salida: diez CT verdes y cero producto cambiado |
| F2 | AUTH-01..04 | taxonomia reutilizada, address/codec/availability unicos |
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

R2 queda preservada como la version inicial publicada; R3 es la correccion revisable e inmutable, con blob
`cd42db03becff42f98b047e61c46689c17a69670`; V4 no la edita ni la reabre. ADR-0044 se publica **propuesto** y
gobierna solo ownership, hechos neutrales, capas y consumo desde main. Solo el Owner puede aceptarlo. Aunque la meta
sea paridad, F3/F5/F7 requieren Owner Validation porque cambian rutas de dibujo. El Candidato se crea limpio antes
de T2/T4.

## 9. Estado de decisiones y preguntas

**Open Material arquitectonico: NONE.** V4 preserva los cierres de I57-AR-01/02 y cierra I57-AR3-01 con el orden
`requirements -> Ensure/import cuando aplica -> query -> final availability facts`, query puro y los tres casos
BLK-AVAILABILITY. Las disposiciones AUTH-01..14, payload, capas, ADR-0034, frame, Resolve/Plan, selection,
comparators, BLK-ID-1..6 y limite de F1 permanecen como en V3. Minor: nombres de namespaces/tipos, codigos
diagnosticos y ubicacion de fixtures.

**Architect Re-review de V3:** Reviewed SHA `68a56b2a3c64ce261787224753ed43b2b8e328a3`; verdict
`CHANGES REQUIRED`; I57-AR3-01 HIGH/MATERIAL. I57-AR-01 e I57-AR-02 quedaron `CLOSED`. El Coordinator acepta el
nuevo hallazgo. Esa aceptacion no es un veredicto formal sobre V4: Coordinator y Architect deben revisar el mismo
SHA exacto de V4.

Hasta veredictos `AGREED` sobre el mismo SHA: Consensus NOT REACHED, Implementation BLOCKED, F1 NOT OPEN.

## 10. Adversarial check focal de V4

PASS documental:

1. Un query preliminar antes de import puede producir `MISSING`, pero no es availability final.
2. Import seguido por query final produce `FOUND` cuando la pieza queda disponible.
3. El `MISSING` prematuro no queda sticky.
4. Un consumer sin permiso de import usa el query directo como resultado final.
5. Un importer que falla deja que el query final observe el estado real.
6. Import parcial produce facts finales por cada requirement, sin false success global.
7. El query no muta `Database` ni realiza ninguna accion de reparacion.
8. El importer nunca recibe `BaseName`.
9. El query nunca recibe `BaseName`.
10. La requirement key permanece exacta durante import y query.
11. Duplicated keys se tratan antes del query conforme el contrato puro; el query no normaliza.
12. Cero requirements no activa importer ni query de nombres generados.
13. Cantilever permanece como geometria pura sin library blocks.
14. `HeaderRunPlan` conserva sus piezas y permite importarlas antes del query final.
15. Import reportado como ejecutado pero query final `MISSING` produce availability final `MISSING`.

PASS de regresion: V4 no cambia disposiciones AUTH-01..14; no referencia R2 como reconciliacion vigente; R3 conserva
su blob y sigue no efectiva; I-52/I-55 siguen sin registrarla; payload no crea geometria universal; F1 solo
caracteriza; ADR-0034 no se modifica; ADR-0044 sigue propuesto; F1 permanece NOT OPEN.

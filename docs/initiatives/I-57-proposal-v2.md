# I-57 — Shared View Foundation — Proposal V2

```text
Estado                    = PROPOSAL V2 — NOT CONSENSUS
Foundation Coordinator    = REVIEW REQUIRED
Foundation Architect      = REVIEW REQUIRED
Foundation Consensus      = NOT REACHED
Foundation Implementation = BLOCKED
F1                        = NOT OPEN
ADR                       = ADR-0044 · PROPOSED
Reconciliation            = R3 · PROPOSED BY I-57 · NOT EFFECTIVE
Coordinator input         = AGREED WITH CHANGES on V1 · I57-COORD-01 applied here
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
resolucion efectiva por rack conforme ADR-0034. `PrepareView` llama al builder existente una vez. El nombre base
se calcula puro; `UniqueBlockName` consulta AutoCAD. Requirements puros se separan de query e importacion.

El comparator port devuelve Single, Divergent o Unreadable. Cada kind controla DTO/schema; Selectivo reutiliza su
comparacion estructural include-by-default. Nunca se elige una hermana.

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
V3 y repetir consenso; nunca parchear producto dentro de F1.

### 5.1 Characterization Blockers de F1

| Blocker | Expected/fail-closed | Owner | Gate | Si contradice V2 |
|---|---|---|---|---|
| matriz CT-04 completa | cada celda produce una sola address/disposition; sin evidencia = Invalid/STOP | I-57 | F1, requerido por F2 | STOP → Proposal V3 |
| convenciones de extremos CT-05 | cada fixture fija eje/cara, span, centro y offset; ambiguedad = frame no disponible | I-57 | F1, requerido por F3 | STOP → Proposal V3 |
| comparators no Selectivos | cada kind demuestra Single/Divergent/Unreadable; falta de comparator = Unreadable | I-57, adapter por kind | F1, requerido por F6 | STOP → Proposal V3 |
| inventario de productores legacy | lista cerrada por autoridad y consumidor; productor no censado bloquea delegacion | I-57 | F1, requerido por F2-F6 | STOP → Proposal V3 |

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
| F6 | AUTH-12..13 | requirements/query/comparators, CT-BLK/AUTH |
| F7 | Candidato | productores unicos, T2 y OV-FND completa |
| F8 | integracion | rebase si aplica, merge autorizado, T3 y recibo con Integration SHA |

Ningun gate se abre al publicar la Proposal. Un rebase crea SHA nuevo y exige evidencia nueva.

## 8. Compatibilidad, ADR y validacion

R3 incorpora las solicitudes autorales CR-SVF-I52-01..06: ownership neutral de X-2 y X-8/CT-05; carreras previas
marcadas provisionales; path/namespace/API sujetos al consenso; consumo solo Integrated; y prohibicion bilateral de
duplicar. Por separado resuelve CR-SVF-01..06 de R1: tolerancia inyectada; I-57 titular neutral de X-1/X-3/X-7;
CT compartidas en I-57; centro comun; y query separado de importer. I-52 conserva reflexion, read-set,
exposicion y AUTH-15. I-55 conserva ID17/18/19, anclaje, cola, UX y materializacion.

R2 queda preservada como la version inicial publicada; R3 es la correccion revisable. ADR-0044 se publica
**propuesto** y gobierna solo ownership, hechos neutrales, capas y consumo desde main. Solo el
Owner puede aceptarlo. Aunque la meta sea paridad, F3/F5/F7 requieren Owner Validation porque cambian rutas de
dibujo. El Candidato se crea limpio antes de T2/T4.

## 9. Estado de decisiones y preguntas

**Open Material arquitectonico: NONE.** El contrato del payload queda cerrado en §3.1. La forma C# concreta no es
contrato. Matriz CT-04, extremos CT-05, comparators no Selectivos e inventario de productores son
**Characterization Blockers de F1**, con owner, gate, expected/fail-closed y consecuencia en §5.1. Minor: nombres
de namespaces/tipos, codigos diagnosticos y ubicacion de fixtures.

**Coordinator:** confirmar cierre de I57-COORD-01, contrato del payload y precondicion generica latest Rn.

**Architect:** confirmar capas/ADR-0034, ausencia de segunda autoridad de plan y falsabilidad de las CT.

Hasta veredictos `AGREED` sobre el mismo SHA: Consensus NOT REACHED, Implementation BLOCKED, F1 NOT OPEN.

## 10. Adversarial check de V2

PASS: no referencia activa a R2 como reconciliacion vigente; R3 conserva blob
`cd42db03becff42f98b047e61c46689c17a69670`; R3 no se declara efectiva; I-52/I-55 siguen sin registrarla;
payload no crea geometria universal; F1 solo caracteriza; ADR-0034 y ADR-0044 no se modifican; ningun resultado de
F1 queda clasificado como Open Material; F1 permanece NOT OPEN.

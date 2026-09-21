# I-57 F5 — Evidencia AUTH-09..11

```text
Gate                  = F5 / AUTH-09..11 ONLY
F5_START_SHA          = 97af3a5aaabad5e37026c4592fae47616a76e15e
F5_IMPLEMENTATION_SHA = 696a06f482e89bac7d254c27597cd53d49123e5c
F5_VALIDATION_SHA     = 696a06f482e89bac7d254c27597cd53d49123e5c
Proposal              = V5 @ 91a3d1ca779e57a5c47f474d9ae18d5a01e3f8ca
R3                     = EFFECTIVE / UNCHANGED
ADR-0034               = ACCEPTED / INTACT
ADR-0044               = ACCEPTED / UNCHANGED
F5 result              = COMPLETE
F6                     = NOT OPEN
```

## AUTH-09 — Resolve

`IRackResolvePort<TInput,TResolved>` y `RackResolveResult<TResolved>` expresan un resultado tipado sin crear un
resolver universal. `RackResolvePorts` ofrece un punto de composicion explicito para Selective, Dynamic, Push Back,
Cantilever, Cabecera y Cama. Cada adapter invoca exactamente una vez el resolver vigente que recibe; no contiene
geometria, segunda resolucion, fallback, eleccion de hermana ni policy del consumer. Los resultados distinguen
`UnsupportedKind`, `UnreadableInput`, `Blocked` y `ResolutionFailure` con codigo estable y diagnostico.

Las autoridades productivas permanecen donde estaban: `SelectiveEffectiveDesignResolver` seguido por
`SelectiveGeometryResolver`, `DynamicRackSystemResolver`, `PushBackResolver`,
`CantileverLineEditorAssembler`/`CantileverLineResolver`, la configuracion resuelta de Cabecera y la configuracion
resuelta de Cama. Los handlers siguen siendo los dueños de su flujo. Selective conserva una sola llamada a
`SelectiveEffectiveDesignResolver.Resolve` y, por tanto, ADR-0034 permanece intacta.

## AUTH-10 — PrepareView / Plan

`IRackViewPreparationPort<TResolved,TPayload>` delega una vez al builder tipado vigente. El carrier intermedio
`RackPreparedView<TPayload>` conserva `Kind`, `Address`, `Frame`, `BaseName` y la misma instancia de `Payload`.
Foundation no inspecciona ni reconstruye geometria. Los payloads siguen siendo los planes existentes, en particular
`HeaderRunPlan` y `CantileverViewPlan`.

El carrier no contiene `BlockRequirements`: AUTH-12 aun no fue ejecutada y una coleccion vacia habria declarado
falsamente que los requisitos ya se evaluaron. Tampoco contiene `object`, JSON, reflection, diccionarios genericos,
materializacion, transacciones o API AutoCAD. Address no soportada, frame no disponible, entrada ausente y fallo del
builder producen resultados tipados y no activan fallback.

Las autoridades de plan permanecen en los builders Selective, Dynamic, Push Back, Cantilever, RackFrames y FlowBed.
El adapter solo proporciona el seam compartido y conserva el payload exacto que recibe del builder.

## AUTH-11 — BaseName

`RackViewBaseName` es la unica autoridad pura para la cadena base historica de los seis kinds y sus vistas ligadas.
Los DrawServices, comandos y el materializer Cantilever delegan en ella. `BlockNaming.SanitizeBlockName` conserva la
frontera legacy caracterizada por CT-NAME; la resolucion de colisiones, `UniqueBlockName`, `RackBlockRenamer`,
`BlockTable` y la creacion de definiciones permanecen en Plugin.

`BaseName` no recibe ni produce library keys, requirements, RackId o disponibilidad. No existe dependencia hacia
AUTH-12. Los productores duplicados de cadenas quedaron delegados o retirados; materializers y resolucion de
colisiones permanecen intactos.

## RED → GREEN y evidencia automatizada

La primera corrida focal fallo al compilar porque los contratos AUTH-09 y AUTH-10 aun no existian. El GREEN final
agrega veinte casos que cubren los seis kinds, invocacion exacta, fallos tipados, payload preservado, frontera sin
AutoCAD, nombres de insercion y nombres ligados. Las caracterizaciones y suites de paridad vigentes se reutilizaron
sin cambiar sus expected.

El arbol estaba limpio antes de producir la evidencia local sobre el SHA exacto
`696a06f482e89bac7d254c27597cd53d49123e5c`.

| Evidencia | Resultado |
|---|---|
| AUTH-09..11 focal | 20/20 PASS |
| Impacto + CT + paridad | 121/121 PASS |
| CT-RES | GREEN |
| CT-PLAN | GREEN |
| CT-NAME | GREEN |
| Core Full local | 8,218 passed; 0 failed; 0 skipped |
| UI Full local | 1,581 passed; 0 failed; 17 historical skipped |
| Build Debug UI | success; 0 errors; 0 warnings |
| Build Debug Plugin | success; 0 errors; solo los `MSB3277` conocidos |
| CI push exacta | run `35495427248`; event `push`; branch exacta; head exacto; 4/4 jobs success |

Run: <https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/35495427248>

## Owner Validation — OV-FND-02 y porcion F5 de OV-FND-03

El Owner ejecuto la validacion aplicable sobre el SHA exacto
`696a06f482e89bac7d254c27597cd53d49123e5c` y confirmo:

```text
Environment                = AutoCAD 2025
Exact AutoCAD build        = NOT SUPPLIED BY OWNER
DLL ProductVersion         = 1.0.0+696a06f482e89bac7d254c27597cd53d49123e5c
OV-FND-02                   = PASS
OV-FND-03 F5 names/geometry= PASS
OV-FND-03 importation      = DEFERRED TO F6/F7

Selective  = PASS
Dynamic    = PASS
Push Back  = PASS
Cantilever = PASS
Cabecera   = PASS
Cama       = PASS
```

`RACKBOMTOTAL`, `RACKLISTA`, errores/rechazos, nombres, geometria, posicion, RackId y save/reopen conservaron
paridad. No hubo vistas faltantes o duplicadas, renombres inesperados ni regresiones visibles. El Owner confirmo que
el comportamiento observable permanece igual. El build exacto de AutoCAD no fue suministrado y se registra
literalmente, sin inferirlo.

La porcion de importacion de OV-FND-03 no se declara completa: requirements, query e `Ensure/import` pertenecen a
AUTH-12 y quedan deliberadamente diferidos a F6/F7.

## Inventario y scope guard

| Productor | Disposicion F5 | Resultado |
|---|---|---|
| Resolvers y handlers por kind | KEEP / DELEGATED THROUGH PORT | conservan algoritmos, diagnosticos y policy vigente |
| Builders por kind | KEEP / DELEGATED | una llamada; payload tipado intacto |
| Productores duplicados de BaseName | DELEGATED / REMOVED | la cadena base vive en `RackViewBaseName` |
| Collision y materializers Plugin | KEEP | siguen dueños de DB, nombre final y escritura DWG |

F5 no implementa `LibraryBlockRequirement`, library query, secuencia `Ensure/import`, comparator authored ni
creacion caller-owned. No existe `PreparedViewPlan` final con requirements falsos. No cambiaron schema DWG,
persistencia, geometry, BOM, `RACKLISTA`, nombres, RackId, posicion, errores, imports, comandos ni transacciones.

No existe contradiccion material, CR material o minor abierta. F5 queda `COMPLETE`; F6 permanece `NOT OPEN` hasta
revision del Coordinator. Este recibo no declara Candidato, integracion ni apertura de F6.

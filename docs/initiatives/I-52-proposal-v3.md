# I-52 — Proposal V3: RACKMIRROR, espejo semantico de uno o varios racks (ID16)

> # ⚠ PROPOSAL V3 — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```text
> Proposal Version = V3
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Consensus = NOT REACHED
> ADR-0036 = PROPOSED
> Implementation = BLOCKED
> ```
>
> ```text
> Schema            = SIN CAMBIO (ningun DTO, sobre ni Xrecord cambia en I-52)
> BASE auditada     = 46fcac2b071929d2bd5b07aa28373941417f74a8   (base de la rama; V3 se redacta SIN rebase)
> CITAS             = archivo:linea contra origin/main @ f8deb675c6d1ef0e64693b157d69c4cc170d7b24 (I-50 integrada)
> CLAIM_SHA         = 55281769d5e9317ad25500e6d3f5f8a849f37279
> BOOTSTRAP_SHA     = 7ee79756d2b93b4eded660cb73310ef3edde32ff
> Discovery G1      = 339b3abd238a3a7a42d60c9edd45a1500a776f62   (se conserva)
> Proposal V1       = 0fc7032bf15d03e7d478bbd9350f156708621c9d   (historial)
> Proposal V2       = 04457183fc2d7dcda5d6c1b988a4789ed4ea2f8f   (historial; CI 34736457802 4/4)
> Revision Arq. V2  = «Architect: CHANGES REQUIRED — PROPOSAL V3»; sin BLOCKER; HIGH AR2-01..AR2-02,
>                     MEDIUM AR2-03..AR2-08, LOW AR2-09..AR2-21 (registro: docs/automation/decisions/I-52.md §14)
> Coordinador       = V2 «ACCEPTED FOR ARCHITECT REVIEW»; CR-V2-1..CR-V2-3; orden de V3 con V3-01..V3-09
>                     vinculantes y cambio de proceso «ADR despues de G3»
> Paralelas         = §15.1 (preflight de V3)
> Estado de gates   = G0 ACCEPTED · G1 ACCEPTED (Discovery) · G2 V3 EN REVISION · G3 NOT OPEN
> ```
>
> Este documento **no** autoriza produccion, **no** modifica V1, V2 ni el Discovery y **no** es el contrato
> vinculante. El contrato se reescribe en el freeze de G2 (§14.1).
>
> **Nota de transparencia.** V1, V2, sus revisiones de Arquitecto y esta V3 las redacto el mismo agente en roles
> distintos. V3 re-verifica en codigo cada correccion y declara las precisiones que encontro al redactarla
> (`[V3-D30]`, `[V3-D31]`).

---

## 0. Reconciliacion V2 → V3

V1 (`0fc7032`), V2 (`0445718`) y el Discovery (`339b3ab`) **se conservan sin cambios**. **Donde V2 y V3 difieran, manda
V3.**

### 0.1 Estados de reconciliacion

| Estado | Significado |
|---|---|
| **CLOSED BY V3** | V3 fija el contrato; lo que quede es verificacion ordinaria de gate |
| **DEFERRED TO CHARACTERIZATION WITH FAIL-CLOSED** | El dato exacto se obtiene en G3/G5, y mientras tanto existe un fail-closed inequivoco que no condiciona la arquitectura |
| **OPEN MATERIAL** | Impide presentar V3. **Debe ser NONE** |

### 0.2 Matriz `[V3-D01]`..`[V3-D31]`

| Marca | Origen | V2 | Correccion V3 | Estado | Donde |
|---|---|---|---|---|---|
| `[V3-D01]` | AR2-01 · V3-01 | N-S02 escribia el remanente `r` como `Length` explicito y solo exigia involucion, fisica y BOM del SNAPSHOT | **Obligacion 9** de toda normalizacion: conmutar con la **resolucion efectiva** para cualquier estado acreditable futuro de las propiedades vinculadas de las que dependa. **S-02** se parte por **dependencia efectiva** de `BeamLength` (regla del ancho compartido): invariante ⇒ RN; dependiente de una propiedad vinculada ⇒ **MC**. Un `BeamLengthOverride` que haga el ancho realmente independiente mantiene RN. PV-7 exige que `ChangeValue` no deshaga la equivalencia reflejada | CLOSED BY V3 | §6.5, §7.1, §10.2 |
| `[V3-D02]` | AR2-01 · V3-01 | S-14b R-s con `P` de la geometria resuelta | Si el espacio de indices de `DesviadorOffCells` depende de un medio frente cuyo ajuste depende de una propiedad vinculada ⇒ **UNKNOWN → FAIL_CLOSED** (S-14c). Prueba futura con al menos dos estados del registro (CT-29, T-M32) | CLOSED BY V3 | §6.5.1, §7.1 |
| `[V3-D03]` | AR2-02 · V3-02 · CR-V2-3 | P-04, P-05 y PC-08 R-s «con riesgo AR-01» | El codigo **ya demuestra** la no conmutacion: anclaje al TROQUEL_TOPE del poste izquierdo del frente, mano constante `ElevationMirrored = false` en FRONTAL y `LONGITUD = larguero + 0.25"` con la misma fabrica que AR-01. **UNKNOWN → FAIL_CLOSED** cuando el tope posterior esta activo. Sin politica de holgura (O-2B). CR-V2-3 queda superado por evidencia de codigo | CLOSED BY V3 | §2.3, §7.3, §7.4, §17 |
| `[V3-D04]` | AR2-03 · V3-03 | V-VIEW solo sobre las vistas seleccionadas | **V-VIEW-ALL**: por rack logico, sobre el diseño reflejado, se enumeran **todas** las `(View, Section canonica)` admisibles que el rack podria materializar despues con Insertar y se exige conmutacion en todas. Operacion pura: sin buscar hermanas, sin añadirlas a la seleccion y sin crear vistas. Una vista admisible no seleccionada que no conmuta hace fallar cerrado el rack | CLOSED BY V3 | §3.8, §6.6 |
| `[V3-D05]` | AR2-04 · V3-04 | S-17 incluia `DrawPallets` como toggle R | Tarimas separadas: **S-17b** sin desborde = R + `G3_PENDING` + ALLOW; **S-17c** fila de claro completo desbordada = **UNKNOWN → FAIL_CLOSED**. Incluidas en CT-17, CT-20, V-VIEW-ALL y T-M34 | CLOSED BY V3 | §7.1, §12 |
| `[V3-D06]` | AR2-05 · V3-05 | La regla de fidelidad solo miraba perdidas; `ExtensionData` del payload se copiaba sin transformar | **V-META**: un miembro desconocido no vacio dentro del **payload semantico** sometido a `μ_k` ⇒ **UNKNOWN → FAIL_CLOSED**, salvo declaracion tipada de invariancia o transformador (vacia en el primer corte). No bloquean los **portadores**: sobre (`ExtensionData`, `CustomProperties` de I-54), envoltorio `RackProjectDocument` y `PropertyValues`. V-RT-STORE sigue midiendo perdidas, pero no justifica copiar metadata semantica desconocida | CLOSED BY V3 | §6.4 |
| `[V3-D07]` | AR2-06 · V3-06 | La matriz no estaba ligada a los tipos | **Guarda de cobertura estructural** por kind: cada miembro material de `SelectivePalletDesignDocument` y anidados, `DynamicRackDesign`, `PushBackDesign`, `CantileverLineDesign` y `RackFrameConfiguration` se clasifica exactamente una vez como `INVARIANT`, `TRANSFORMED`, `NORMALIZED`, `CARRIER`, `DERIVED` o `UNSUPPORTED`, distinguiendo persistido, derivado y portador. Miembro nuevo sin clasificar ⇒ **RED / STOP**. Omisiones añadidas: escalares globales del Selectivo (S-04b) y `DerivedAisles` (S-22b) | CLOSED BY V3 | §6.7, §7, §12 |
| `[V3-D08]` | AR2-07 · V3-07 | C-2 se probaba en G8, despues del comando | **C-2 pasa a G6**: C2-1..C2-4 GREEN (Core, UI si corresponde, G-M9, nombre logico, estado efectivo, `View`, `Section` canonica) antes de cerrar G6. **G7 no consume la autoridad** sin C-2 verde. G8 queda para cruces, continuidad de G-M9 y reconciliacion final | CLOSED BY V3 | §8.5, §14 |
| `[V3-D09]` | AR2-08 · V3-08 · CR-V2-2 | Centro en «unidades locales» sin marco | Centro en el **marco local de insercion de la pieza despues de `T(−BlockOrigin)`**; no WCS, no marco del rack, no BTR crudo. Forma afin `c = a0 + a1·Parametro` **solo** si CT-06 la justifica y prueba. Se registra la biblioteca y el manifiesto usados como evidencia | CLOSED BY V3 (forma afin: DEFERRED TO CHARACTERIZATION WITH FAIL-CLOSED) | §11.3 |
| `[V3-D10]` | AR2-09 · V3-09 | Selectivo: `View` vacia se leia como frontal con cualquier `Section` | `View` vacia **solo** con `Section = −1` (legado previo a `View`). `View` vacia con `Section ≥ 0` ⇒ **FAIL_CLOSED**: la historia (`6023572` introduce `View` antes de que `4d2ca82` introduzca `Section`) no permite ese legado | CLOSED BY V3 | §3.7 |
| `[V3-D11]` | AR2-10 · V3-09 | Sin tratamiento de referencias dinamicas o anonimas | **ST-17**: referencia a bloque dinamico, BTR anonimo o definicion anotativa **con payload RackCad** (en su BTR o en el `DynamicBlockTableRecord`) ⇒ **FAIL_CLOSED explicito (E4)**; nunca desaparecen como «no rack» | CLOSED BY V3 | §4.1 |
| `[V3-D12]` | AR2-11 · V3-09 | «Round-trip de RACKEDITAR» y «round-trip del store» usados como sinonimos | Dos obligaciones: **V-RT-STORE** (fidelidad del store del kind) y **V-EDIT = C-2** (estabilidad operativa del redibujo). Un defecto historico de `EditX` no autoriza una perdida adicional del espejo | CLOSED BY V3 | §6.3, §8.5 |
| `[V3-D13]` | AR2-12 · V3-09 | Orden de PREFLIGHT incoherente entre §6.3 y §9.1 | **Un unico orden normativo** P1..P10, ajustado por dependencias reales (la identidad precede al ensayo de restamp) | CLOSED BY V3 | §9.1 |
| `[V3-D14]` | AR2-13 · V3-09 | N-S07 rellenaba filas nulas aunque la lista no existiera | N-S07 **no crea** filas artificiales: se preserva la ausencia frente a la fila explicita | CLOSED BY V3 | §6.5.3 |
| `[V3-D15]` | AR2-14 · V3-09 | Lenguaje condicional sobre I-50 y citas contra `46fcac2` | I-50 **integrada**: `DimensionViews` es requisito normativo; T-M18 incondicional; citas re-ancladas contra `main @ f8deb67` | CLOSED BY V3 | §6.4, §10.3, todo el documento |
| `[V3-D16]` | AR2-15 · V3-09 | Huella solo de referencias y payloads | La huella incluye el **read-set** del registro: las entradas realmente leidas (resultado de acreditacion, version y cada `VariableId` referenciado con su definicion), no todo el registro | CLOSED BY V3 | §9.2 |
| `[V3-D17]` | AR2-16 · V3-09 | G-M2 solo cubria `WithDesign(` y `TryWrite(` | G-M2 prohibe toda escritura del registro, fijar literal, vincular, desvincular, reconciliador mutante, preflight y ejecutor de mutaciones y cualquier API equivalente; la lectura efectiva queda autorizada | CLOSED BY V3 | §12.3 |
| `[V3-D18]` | AR2-17 · V3-09 | V-BOM con redaccion ambigua | **Dos comprobaciones** separadas: multiconjunto de lineas del builder del kind con su propia regla de precision, y clave consolidada de `ConsolidatedBom` | CLOSED BY V3 | §11.5 |
| `[V3-D19]` | AR2-18 · V3-09 | CT-16 solo textos | CT-16 fija tambien orden de grupos, orden de referencias, avisos y errores | CLOSED BY V3 | §5.1, §12.1 |
| `[V3-D20]` | AR2-19 · V3-09 | CT-25 creaba un censo propio | CT-25 reutiliza `CatalogBlockParameters` y `CatalogBlockManifest` (I-19) | CLOSED BY V3 | §11.4, §12.1 |
| `[V3-D21]` | AR2-20 · V3-09 | Sin prueba negativa de simetria | T-M40 negativa: ninguna declaracion admitida hace equivalente un fixture con holgura unilateral tipo AR-01; el registro rechaza declaraciones para familias con fila UK por holgura (hoy `Tope`) | CLOSED BY V3 | §11.3, §12.2 |
| `[V3-D22]` | AR2-21 · V3-09 | `R-s`, `C/F`, `LB` y `FC` usados como clases | **Tres dimensiones**: `RepresentabilityClass`, `EvidenceStatus` y `OperationalDisposition`. Los alias de V2 solo quedan como historial (§0.4) | CLOSED BY V3 | §7.0 |
| `[V3-D23]` | V3-09 (revision) | — | Paquete LOW completo en `[V3-D10]`..`[V3-D22]` | CLOSED BY V3 | — |
| `[V3-D24]` | V3-10 (revision) | Secuencia de freeze y aceptacion del Owner sin escribir | Secuencia normativa escrita (§14.1) y tips de paralelas actualizados (§15.1) | CLOSED BY V3 | §14.1, §15.1 |
| `[V3-D25]` | Decision del Coordinador (este gate) | ADR-0036 aceptado como condicion previa a G3 | **ADR despues de G3**: G3 es caracterizacion y puede forzar V4. ADR-0036 **no** es precondicion de G3 y **si** de G4 | CLOSED BY V3 | §14, §16 |
| `[V3-D26]` | Decision del Coordinador | O-1..O-4 con tiempos de V2 | O-1 despues del freeze tecnico y antes de G3; O-2 y **O-2B** sin decision (UK/FAIL_CLOSED); O-3 en G3/G9; O-4 despues de G3 | CLOSED BY V3 | §1.2 |
| `[V3-D27]` | Decision del Coordinador | Rebase «antes de escribir codigo» | V3 **sin** rebase; tras el acuerdo tecnico: rebase/reconciliacion sobre el `main` vigente, conflicto del indice de ADR, freeze por SHA, re-check; un cambio normativo material vuelve a revision | CLOSED BY V3 | §14.1 |
| `[V3-D28]` | Preflight de V3 | Paralelas de V2 | I-53 rebaso sobre `main` y **acepto ADR-0037** en su rama (`5efaf7e`); I-54 publico Proposal V3 (`5d25da8`); I-49 rebaso su V6 sobre `main` sin cambios de contenido (`048a508` → `1ed93a0`). Todo solo docs frente a `main`: sin cambio de contrato productivo | CLOSED BY V3 | §15, §16 |
| `[V3-D29]` | CR-V2-1 | — | Criterio del registro de produccion confirmado (PV-5): el Selectivo exige registro acreditable aunque no haya vinculo; cambiarlo queda fuera de I-52 | CLOSED BY V3 | §10.2 |
| `[V3-D30]` | Precision encontrada al redactar | — | Los miembros **retirados** que el store descarta por diseño (`ColumnBottomPlateEndOffset`, `ColumnTopPunchOffset`, `RackProjectStore.cs:374-400`) no cuentan como metadata desconocida: se clasifican `DERIVED` (K-17) | CLOSED BY V3 | §6.4, §7.5 |
| `[V3-D31]` | Precision encontrada al redactar | — | La dependencia efectiva de V3-01 exige tambien clasificar en la guarda de cobertura los **descriptores vinculables** (`SelectiveLinkedProperties.All`): una propiedad vinculable nueva sin efecto declarado sobre la geometria usada por una regla ⇒ RED / STOP | CLOSED BY V3 | §6.7 |

### 0.3 Resumen

```text
OPEN MATERIAL = NONE
```

Diferido con fail-closed inequivoco (no condiciona la arquitectura):

| Asunto | Fail-closed mientras tanto | Donde se resuelve |
|---|---|---|
| Semantica de `MountingFace` (S-10, D-03b) | sin confirmacion de CT-12 ⇒ FAIL_CLOSED | G3 |
| Filas `R` con `EvidenceStatus = G3_PENDING` (anclajes) | V-VIEW-ALL por rack; contradiccion de CT-17 ⇒ Proposal V4 | G3 |
| Censo de simetrias y forma afin del centro | sin declaracion ⇒ diferencia de mano no equivalente ⇒ FAIL_CLOSED | G3/G9 |
| Predicado exacto de «tope activo» (Selectivo y Push Back) | cualquier tope configurado con celda dibujable ⇒ FAIL_CLOSED | G3 (CT-15) |
| Ids de placa Cantilever en BOM (K-09) | V-BOM distinto ⇒ FAIL_CLOSED | por rack |
| Linea base exacta de cada store | V-RT-STORE ⇒ FAIL_CLOSED | G3 (CT-35) |
| Valor de la tolerancia de escala | sin valor no hay implementacion (G4) | G4 |
| Semantica de `IsStandard` (H-06) | Auto con baseline ⇒ FAIL_CLOSED | G5 |
| Efectos reales de importacion y UNDO | UNKNOWN declarado; nada se promete | G9 (Owner) |
| Censo de parametros con lado (CT-25) | parametro sin regla ⇒ FAIL_CLOSED | G3 |
| Declaraciones de invariancia de metadata | ninguna ⇒ FAIL_CLOSED | futuro |

### 0.4 Alias historicos de V2 (solo reconciliacion)

| Alias V2 | Equivalente V3 |
|---|---|
| `R-s` | `RepresentabilityClass = REPRESENTABLE` + `EvidenceStatus = G3_PENDING` + `OperationalDisposition = ALLOW` (sujeto a V-VIEW-ALL) |
| `C/F` | fila de seccion: `REPRESENTABLE` + `CODE_SUPPORTED` + `CANONICALIZE` (legado inequivoco) o `FAIL_CLOSED` (resto) |
| `LB` | `REPRESENTABLE` + `CODE_SUPPORTED` + `BASELINE_LIMITED` (normalizacion declarada del store); la metadata semantica desconocida ya no es `LB`, es `UNKNOWN` |
| `FC` | precondicion operativa con `FAIL_CLOSED`; no es clase de representabilidad (§7.9) |

### 0.5 Cobertura explicita

| Hallazgo o cambio | Marca |
|---|---|
| AR2-01 | `[V3-D01]`, `[V3-D02]` |
| AR2-02 | `[V3-D03]` |
| AR2-03 | `[V3-D04]` |
| AR2-04 | `[V3-D05]` |
| AR2-05 | `[V3-D06]`, `[V3-D30]` |
| AR2-06 | `[V3-D07]`, `[V3-D31]` |
| AR2-07 | `[V3-D08]` |
| AR2-08 | `[V3-D09]` |
| AR2-09..AR2-21 | `[V3-D10]`..`[V3-D22]` (en ese orden) |
| V3-01..V3-08 de la revision | `[V3-D01]`..`[V3-D09]` |
| V3-09 de la revision | `[V3-D23]` |
| V3-10 de la revision | `[V3-D24]` |
| Decisiones del Coordinador de este gate | `[V3-D25]`..`[V3-D27]`, `[V3-D29]` |

---

## 1. Decisiones de producto

### 1.1 Decisiones del Coordinador vigentes (desde V1)

Registro: `docs/automation/decisions/I-52.md` §4. Vinculan V3; **no** son consenso final.

| # | Decision | Precision V3 |
|---|---|---|
| **PDC-1** | **Solo copia.** Crea copia reflejada, conserva **todos** los originales, nunca borra, nunca refleja en sitio, nunca conserva el `RackId` en la copia. **Sin** pregunta «¿Borrar objetos originales?» | — |
| **PDC-2** | Nombre logico por rack `<base> - espejo`, **sin ordinal**. `Name` ≠ `RackId`. Los nombres de `BlockTableRecord` siguen la politica de unicidad existente de cada familia de plan | La autoridad de plan recibe el nombre logico (§8.1) |
| **PDC-3** | Autoridad = referencias **fisicamente seleccionadas**. Sin buscar hermanas; sin crear vistas no seleccionadas. Vistas seleccionadas del mismo `RackId` = **un** grupo con **un** `NewRackId`. Una referencia no admisible **falla toda la operacion** antes de MUTATE | V-VIEW-ALL verifica todas las vistas admisibles del diseño reflejado **sin** buscar ni crear hermanas (§6.6) |
| **PDC-4** | Sin prompt de eje semantico. El usuario solo da la linea (dos puntos) | — |
| **PDC-5** | Indices, numeros y letras derivados se **regeneran** desde el documento reflejado | — |
| **PDC-6** | Protector lateral Left/Right: **UNKNOWN** donde sea ambiguo; estado explicito cuya reflexion no este demostrada ⇒ fail-closed | — |
| **PDC-7** | Clases `REPRESENTABLE`, `REPRESENTABLE_BY_NORMALIZATION`, `REQUIRES_MODEL_CHANGE`, `UNKNOWN`, evaluadas **por rack concreto** en PREFLIGHT; `REQUIRES_MODEL_CHANGE` y `UNKNOWN` material ⇒ fail-closed | Tres dimensiones separadas (§7.0) |
| **PDC-8** | Unico comando nuevo `RACKMIRROR`, **sin alias**. Censo de `[CommandMethod(` **33 → 34** | — |
| **PDC-9** | Solo Model Space; MINSERT ⇒ fail-closed; linea por dos puntos UCS→WCS; fuente canonica | `Origin = 0` (ST-14), `View`/`Section` decodificables (ST-15), bloques dinamicos, anonimos o anotativos ⇒ fail-closed (ST-17) |
| **PDC-10** | Arquitectura para los seis kinds con bloque; el primer corte solo ejecuta vistas que exponen `μ_k`; la cama falla cerrado; sin ampliar schema | Aceptacion de alcance del Owner (O-1) despues del freeze tecnico y antes de G3 |

### 1.2 Decisiones del Owner

| # | Decision | Contrato vigente | Momento |
|---|---|---|---|
| **O-1** | **Alcance del primer corte.** No se reflejan laterales de racks, la cama ni Dinamicos materializados solo como lateral; se aceptan los fail-closed adicionales de §2.3 | recorte de §2 | **pendiente**; se pide **despues del freeze tecnico** y **antes de G3** |
| **O-2** | **Topes del Selectivo** | `UNKNOWN → FAIL_CLOSED`. **No** necesita decision ahora; una relajacion vuelve al Coordinador y al Owner | — |
| **O-2B** | **Tope posterior de Push Back** | `UNKNOWN → FAIL_CLOSED`. **No** necesita decision ahora; una relajacion vuelve al Coordinador y al Owner | — |
| **O-3** | **Evidencia de simetrias** | ningun bloque se asume simetrico | durante G3 (censo, CT-06) y G9 (confirmacion visual, M-15) |
| **O-4** | **ADR-0036** | `propuesto` | se pide **despues de G3**, si G3 confirma el contrato; **precondicion de G4** |

---

## 2. Alcance del primer corte

### 2.1 Dentro

| Kind (`Kind` del sobre) | `μ_k` canonica | Vistas admitidas | Evidencia de la vista |
|---|---|---|---|
| Selectivo (`selective`) | **RUN**: invierte frentes/postes | frontal (`Section` = fondo), planta | frontal X = run (`SelectivePostGeometry.cs:32-40`); planta X = fondo, Y = frente (`SelectivePlantaBuilder.cs:15-22`) |
| Dinamico (`dynamic`) | **RT**: invierte frentes | frontal (`Section` 0 salida / 1 entrada), planta | `RackDinamicoCommands.cs:392-393`; `DynamicSystemFrontalBuilder`/`DynamicSystemPlantaBuilder` |
| Push Back simple y compuesto (`pushback`) | **RT** | frontal (`Section` 0..3), planta | `PushBackSystemFrontalBuilder.cs:137-155` |
| Cantilever (`cantilever`) | **R_X**: invierte la linea de estaciones | frontal, planta | camaras `CantileverViewPlanBuilder.cs:205-235` |
| Cabecera independiente (`cabecera`) | **R_D**: invierte la profundidad del marco | lateral, planta | `LateralHeaderLayoutBuilder.cs:49-59`; `PlantaHeaderLayoutBuilder.cs:13-21` |

### 2.2 Fuera (fail-closed o no aplica)

- Laterales de Selectivo, Dinamico, Push Back y Cantilever; la cama (`cama`) entera.
- Diseños con alguna fila `REQUIRES_MODEL_CHANGE` o `UNKNOWN` material (§7).
- Espejo en sitio, borrar originales, conservar `RackId`, prompt de eje, alias.
- «Vista observada desde el lado opuesto» (cambio de modelo; futuro, §3.9).
- Paper Space, MINSERT, fuentes no canonicas (§4), definiciones con BASE movida (`Origin ≠ 0`), referencias a bloques
  dinamicos, anonimos o anotativos.
- Cualquier transformacion de la holgura de topes (O-2, O-2B), cualquier normalizacion de `IsStandard` (H-06) y
  cualquier transformador de metadata desconocida.
- Cambios de schema, de `RackEnvelopeRestamp.cs`, de `PushBackMirror.cs`, de `WithDesign`, de los drawers y builders
  existentes y del registro de variables.
- Drive-In (no existe: `RackMainMenuWindow.xaml:139-144`) y Larguero (sin bloque RackCad).
- Arreglar los hallazgos laterales del Discovery §24: se registran en `ideas-futuras.md` al cerrar G2.

### 2.3 Fail-closed adicionales conocidos

Reducen el alcance util del primer corte sin cambiar la arquitectura. Todos se detectan **antes** de pedir la linea.

| Caso | Motivo | Fila o regla |
|---|---|---|
| Selectivo que usa topes | holgura de 0.25" anclada a un lado (AR-01) | S-13, S-13b |
| **Push Back con tope posterior activo** | mismo patron demostrado en codigo (`[V3-D03]`) | P-04, P-05, PC-08 |
| **Medio frente cuyo `BeamLength` efectivo depende de una propiedad vinculada** | la normalizacion congelaria el remanente (`[V3-D01]`) | S-02b |
| **Desviador con off-cells cuyo espacio de indices depende de ese ajuste** | remapeo inestable ante `ChangeValue` | S-14c |
| Parrilla con una fila desbordada (`count·frente > span`) | reparto apilado desde la izquierda | S-15x |
| **Tarimas con una fila de claro completo desbordada** | mismo reparto apilado (`[V3-D05]`) | S-17c |
| Cabecera con `AutoAlternating` en un panel con diagonal y `StandardBaselineId` no vacio | semantica de «estandar» sin caracterizar; las plantillas crean exactamente eso (`RackFrameConfigurationFactory.cs:85`, `:226-243`) | H-06 |
| **Metadata semantica desconocida no vacia en el payload** | no hay transformador (`[V3-D06]`) | V-META |
| **Alguna vista admisible del rack (seleccionada o no) que no conmuta** | V-VIEW-ALL (`[V3-D04]`) | X-15 |
| Definicion fuente con `Origin ≠ 0` | BASE/BEDIT no soportado | ST-14 |
| **Referencia a bloque dinamico, anonimo o anotativo con payload RackCad** | transformacion adicional no capturada | ST-17 |
| `View` o `Section` no canonizable, incluida la `View` vacia del Selectivo con `Section ≥ 0` | §3.7 | ST-15 |
| Bloques de biblioteca ausentes tras PREPARE | la importacion es best-effort (§9.5) | E10, E11 |
| Registro de variables no acreditable con algun grupo Selectivo seleccionado | criterio de la autoridad efectiva (PV-5) | E7 |

---

## 3. Modelo formal normativo

### 3.1 Definiciones

Transformaciones afines del plano XY de WCS: `T(v)` traslacion, `R(α)` rotacion antihoraria, `S(a,b)` escala.

```text
G        = reflexion de la hoja respecto de la linea del usuario ℓ = (q, φ)
         = T(q) · R(2φ) · S(1,−1) · T(−q)                                      det G = −1
P        = colocacion efectiva de la referencia fuente
         = T(p) · R(θ) · S(s,s) · T(−o)        s > 0,  o = BlockTableRecord.Origin de su definicion     det P = +s²
           primer corte: solo o = 0 (ST-14), sin bloque dinamico, anonimo ni anotativo (ST-17)
           ⇒ P = T(p) · R(θ) · S(s,s)
D        = documento authored del rack (en su sustrato real, §6.2)
R        = estado acreditable del registro de variables del dibujo
Eff(D,R) = diseño efectivo que produce la autoridad efectiva del kind (§10.2)
μ_k      = reflexion semantica canonica del kind k: documento → documento
Π_(V,σ)  = plan fisico de la vista V con seccion semantica σ, en coordenadas locales de la definicion
           (piezas fisicas; anotaciones y cotas aparte, §11.6)
F        = reflexion local de la vista que expone μ_k:
           X_c = T(2c,0)·S(−1,1)   (x → 2c − x)        Y_c = T(0,2c)·S(1,−1)   (y → 2c − y)
σ'       = seccion canonica destino (§3.7)
```

Las definiciones nuevas se crean con `Origin = 0` (`LateralHeaderDrawer.cs:243`; `CantileverViewMaterializer.cs:47`), asi
que la colocacion destino `P'` no lleva termino de origen.

### 3.2 Exposicion y colocacion

```text
EXPOSICION    (V,σ) expone μ_k  ⇔  Π_(V,σ')(Eff(μ_k D, R)) ≡ F ∘ Π_(V,σ)(Eff(D, R))
              para TODO estado acreditable R de las propiedades vinculadas de las que dependa D   (≡ de §11)

COLOCACION    P' = G · P · F                    det P' = (−1)(+s²)(−1) = +s²

              F = X_c  ⇒  parte lineal de P' = R(2φ − θ + π) · S(s,s)
              F = Y_c  ⇒  parte lineal de P' = R(2φ − θ)     · S(s,s)

TEOREMA       para (V,σ) admitida:  P' · Π_(V,σ')(Eff(μ_k D, R))  =  G · P · Π_(V,σ)(Eff(D, R))   para todo R
```

**Consecuencias normativas.**

1. La geometria fisica de cada vista admitida coincide con el espejo geometrico de la hoja, **tambien despues** de un
   `ChangeValue` futuro (V3-01).
2. Los textos y las cotas **se regeneran** desde `μ_k D` y quedan legibles (principio de ADR-0031 §8).
3. La colocacion final tiene determinante positivo y la escala uniforme de la fuente. **Nunca** se persiste escala espejo.
4. `μ_k` **no** depende de `ℓ`: el documento reflejado es el mismo para cualquier linea y cualquier subconjunto de
   vistas. Por eso se calcula y verifica **antes** de pedir la linea (§9.1).
5. Una regla de `μ_k` o una normalizacion que escriba en el documento authored un valor calculado a partir de `R`
   rompe el «para todo R»: la fila correspondiente es `REQUIRES_MODEL_CHANGE` o `UNKNOWN` (§6.5).

### 3.3 Comprobacion algebraica y composicion con `Transform2D`

**Algebra (verificada en la revision de V1).**

```text
S(1,−1)·R(−φ) = R(φ)·S(1,−1)                    ⇒  G = T(q)·R(2φ)·S(1,−1)·T(−q)
F = X_c:  R(2φ)S(1,−1) · R(θ)S(s,s) · S(−1,1)  = R(2φ−θ)·S(−1,−1)·S(s,s) = R(2φ−θ+π)·S(s,s)
F = Y_c:  R(2φ)S(1,−1) · R(θ)S(s,s) · S(1,−1)  = R(2φ−θ)·S(s,s)
det = s² en ambos casos;  F∘F = id  ⇒  P'·Π(μD) = G·P·F·F·Π(D) = G·P·Π(D)
S(−s,−s) = R(π)·S(s,s)                          ⇒  la canonizacion ST-8 es exacta
```

**Ejemplos numericos.**

| # | Datos | Resultado | Comprobacion |
|---|---|---|---|
| E-1 frontal, `X_c` | `s = 1`, `θ = 0`, `p = (10,5)`, linea `x = 0` (`φ = 90°`), `c = 24` | `P' = T(−58,5)`, `θ' = 0` | el poste 0 original cae en `(10,5)` y su espejo en `(−10,5)`; en `μD` ese poste esta en `x = 48` y `P'(48,0) = (−10,5)` |
| E-2 planta, `Y_c` | `s = 2`, `θ = 30°`, `p = (1,2)`, linea por `(3,0)` a 45°, `c = 5`, punto `u = (4,1)` | `θ' = 60°`, `p' = G(P(F(0))) = (22.3205081, −12)` | `G(P(u)) = (10.7320508, 3.9282032)` y `P'(F(u)) = P'(4,9) = (10.7320508, 3.9282032)` |
| E-3 canonizacion | `(−2,−2)` con 30° aplicado a `(1,0)` | `(−1.732, −1)` | igual que `(2,2)` con 210° |

**Composicion.** `Transform2D.Then` lee de izquierda a derecha (`Transform2D.cs:86-93`): `A.Then(B)` aplica `A` y luego
`B`.

```text
P  = Transform2D.Scale(s).Then(Transform2D.Rotation(θ)).Then(Transform2D.Translation(p))      (o = 0)
X_c = Transform2D.MirrorAboutY.Then(Transform2D.Translation(2c, 0))
Y_c = Transform2D.MirrorAboutX.Then(Transform2D.Translation(0, 2c))
G  = ReflectionAboutLine(a, b)                     // fabrica nueva, §8.3 y G4
P' = F.Then(P).Then(G)                             // aplica F, luego P, luego G  ≡  G · P · F
```

La descomposicion de `P'` en `(posicion, rotacion, escala)` exige determinante positivo, parte lineal = rotacion ×
escala uniforme (sin cizalla) dentro de las tolerancias de §4.3, y escala igual a la de la fuente. Si falla: error de
programacion, no clasificacion.

### 3.4 Lemas

- **Lema 1 (elevaciones).** La Y de una elevacion es la altura, que ninguna `μ` valida refleja. Como
  `S(1,−1) = R(π)·S(−1,1)`, cualquier `G·P` se escribe con `F = X_c`. Una elevacion solo puede exponer la `μ` que
  refleja **su** eje X local.
- **Lema 2 (planta).** La planta dibuja los dos ejes horizontales locales y puede exponer cualquiera de las dos
  reflexiones horizontales, eligiendo `F = X_c` o `F = Y_c`.
- **Corolario.** Un rack logico tiene **un** documento authored (ADR-0034 §9) y por tanto **una** `μ`. Una frontal y
  una lateral del mismo rack no pueden ser fieles a la vez.

### 3.5 Contraejemplo minimo (Selectivo frontal + lateral)

```text
Rack Selectivo: DepthCount = 1; Bays = [claro 96", claro 48"]; bota con Placement = EntryExit (solo cara cercana)
Seleccion: frontal (Section 0) + lateral (Section 0); linea vertical
Frontal fiel  ⇒ claros [48", 96"]            ⇒ exige invertir Bays (μ_RUN)
Lateral fiel  ⇒ bota en la cara lejana       ⇒ exige Placement = Rear (μ_DEPTH)
μ_RUN   no voltea la lateral: la bota queda cercana (SelectiveLateralBuilder.cs:173-198)
μ_DEPTH no invierte los claros de la frontal
⇒ no existe D' unico con det P' > 0 para ambas vistas.  ∎
```

Fisicamente `μ_DEPTH = Rot_π ∘ μ_RUN`: los dos describen el mismo rack. El conflicto es de **punto de vista**: RackCad
no puede dibujar una elevacion «vista desde el lado opuesto». Bajo la admisibilidad estricta (§3.8), la lateral no
expone `μ_RUN` y **falla cerrado**.

### 3.6 Reflexion canonica por kind

| Kind | `μ_k` | Por que ese eje |
|---|---|---|
| Selectivo | RUN | Es el eje de la frontal, que el editor obliga a insertar primero (`RackSelectiveWindow.xaml.cs:2145-2153`); conserva «fondo 0 = frente» y evita los casos no representables de profundidad (herencias, diagonales) |
| Dinamico | RT | La salida esta fijada en X=0 (`DynamicLoadBeamGeometry.cs:166-190`): la reflexion de profundidad no es almacenable |
| Push Back simple y compuesto | RT | El lado A siempre existe y la fisica asume flujo hacia +X (`PushBackElevations.cs:166-177`); bajo RT **no** se intercambian A y B |
| Cantilever | R_X | No toca los lados ±Y ni el traslape derivado (`CantileverLineResolver.cs:271-277`) |
| Cabecera | R_D | Su elevacion principal es la lateral (eje profundidad); R_F no produciria cambio visible |
| Cama | ninguna | Ningun campo persistido expresa el lado del tope (`FlowBedLateralBuilder.cs:34-66`) |

### 3.7 Vistas, secciones y exposicion (`SectionRaw → semantica → canonica`)

**Regla.** Por kind, la `View` y la `Section` crudas del sobre se decodifican con la **misma lectura que la
produccion**:

1. si la decodificacion es **inequivoca y ya la soporta produccion** —y el estado crudo pudo producirlo alguna version
   real—, `σ'` es la seccion **canonica** que produccion escribe al redibujar (`OperationalDisposition = CANONICALIZE`);
2. si no es inequivoca, si ninguna version pudo producirla o si produccion la rechaza, la referencia **falla cerrado**
   (toda la operacion, PDC-3).

La `View` canonica es la constante de `RackEmbedDocument` (`frontal`, `lateral`, `planta`), comparada sin mayusculas
como en produccion. Una `View` **no vacia y no reconocida** falla cerrado en todos los kinds. **Selectivo:** la `View`
vacia solo es legado inequivoco con `Section = −1`, porque `View` aparecio (`6023572`) antes que `Section` (`4d2ca82`):
ningun build escribio `View` vacia con `Section ≥ 0`. `c` es el centro del **tramo fisico del eje reflejado** segun las
convenciones de cada builder; el valor entre parentesis es el candidato de V1 y lo fija CT-05.

| Kind | View canonica | `SectionRaw` | `SemanticSection` | `TargetSectionCanonical` | `μ_k` | Expone | `F` | `c` (candidato V1) | Autoridad de produccion |
|---|---|---|---|---|---|---|---|---|---|
| selective | frontal (`View = frontal`) | `k`, con `0 ≤ k < fondos` | fondo `k` | `k` | RUN | **si** | `X_c` | centro del tramo del fondo `k` (ultimo poste / 2) | `RackSelectivoCommands.cs:126`, `:168`, `:177`; `ProjectVariableMutationExecutor.cs:186`, `:216` |
| selective | frontal (`View = frontal` o vacia) | `−1` (legado documentado) | fondo 0 | `0` | RUN | **si** | `X_c` | idem `k = 0` | `RackSelectivoCommands.cs:155`, `:168`; `ProjectVariableMutationExecutor.cs:185-186` |
| selective | `View` vacia | `≥ 0` | combinacion que ningun build escribio | — | — | **fail-closed** | — | — | produccion la lee como frontal (`RackSelectivoCommands.cs:126`); historial `6023572` / `4d2ca82` |
| selective | frontal | `< −1` | coercion a 0 sin legado documentado | — | — | **fail-closed** | — | — | produccion coerciona (`:168`) |
| selective | frontal | `≥ fondos` | vista huerfana | — | — | **fail-closed** | — | — | `RACKEDITAR` la borra como fantasma (`RackSelectivoCommands.cs:169-173`); el ejecutor aborta (`ProjectVariableMutationExecutor.cs:202-205`) |
| selective | planta | cualquiera | planta | `−1` | RUN | **si** | `Y_c` | centro del tramo del grid maestro (ultimo poste / 2) | reescribe `−1` (`RackSelectivoCommands.cs:216`; `ProjectVariableMutationExecutor.cs:216`) |
| selective | lateral | poste `p` | poste | — | — | no (**MC**) | — | — | — |
| dynamic | frontal | `0` / `1` | salida / entrada | igual | RT | **si** | `X_c` | centro del tramo de frentes (ultimo poste / 2) | `RackDinamicoCommands.cs:220-224`, `:392-393` |
| dynamic | frontal | fuera de `{0, 1}` | coercion a salida sin legado documentado | — | — | **fail-closed** | — | — | produccion coerciona y reescribe 0 (`:222-224`) |
| dynamic | planta | cualquiera | planta | `−1` | RT | **si** | `Y_c` | centro del tramo de frentes | reescribe `−1` (`:211`) |
| dynamic | lateral o `View` vacia | poste `p` | poste (legado sin `View` = primer corte) | — | — | no (**MC**) | — | — | `:235-239` |
| pushback | frontal | `0..3` (compuesto) o `0..1` (simple) | (corte, lado) | igual | RT | **si** | `X_c` | centro del tramo de frentes (reticula compartida) | `RackPushBackCommands.cs:254-258`; `PushBackSystemFrontalBuilder.cs:137-155` |
| pushback | frontal | `2..3` en rack simple | lado B inexistente | — | — | **fail-closed** | — | — | el rack simple ignora el lado (`PushBackSystemFrontalBuilder.cs:43-45`) |
| pushback | frontal | fuera de `0..3` | invalida | — | — | **fail-closed** | — | — | `RACKEDITAR` aborta (`RackPushBackCommands.cs:218`, `:427-450`) |
| pushback | planta | `−1` | planta | `−1` | RT | **si** | `Y_c` | centro del tramo de frentes | `:441-444` |
| pushback | planta, `View` vacia o no reconocida | `≠ −1` o cualquiera | invalida | — | — | **fail-closed** | — | — | `RACKEDITAR` aborta (`:427-450`) |
| pushback | lateral | poste `p` | poste | — | — | no (**MC**) | — | — | — |
| cantilever | frontal | `−1` | linea | `−1` | R_X | **si** | `X_c` | `−(N−1)·s/2` | `RackCantileverCommands.cs:530-540`; `CantileverViewPlanBuilder.cs:237-239` |
| cantilever | planta | `−1` | linea | `−1` | R_X | **si** | `X_c` | `−(N−1)·s/2` | idem |
| cantilever | frontal o planta, `View` vacia o no reconocida | `≠ −1` o cualquiera | invalida | — | — | **fail-closed** | — | — | `RACKEDITAR` aborta (`RackCantileverCommands.cs:501-540`) |
| cantilever | lateral | estacion `k` | estacion | — | — | no (**MC**) | — | — | `CantileverViewPlanBuilder.cs:213-215` |
| cabecera | lateral (tambien `View` vacia) | cualquiera | marco | `−1` | R_D | **si** | `X_c` | `Depth / 2` | `RackCabeceraCommands.cs:197`, `:239-240` (no-planta = lateral; reescribe `−1`) |
| cabecera | planta | cualquiera | marco | `−1` | R_D | **si** | `X_c` | `Depth / 2` | idem |
| cama | (lateral) | `−1` | riel | — | ninguna | no (**MC**) | — | — | `FlowBedLateralBuilder.cs:34-66` |

**Notas.**

- Eje X de cada vista: Selectivo frontal = run; Selectivo planta X = fondo, Y = run; Dinamico y Push Back frontal =
  frentes, planta X = profundidad, Y = frentes; Cantilever frontal = −X (camara con `right = up × forward`,
  `CantileverViewPlanBuilder.cs:209-211`; `Spatial3D.cs:186-197`), planta = (−X, +Y); cabecera lateral = profundidad con
  el poste izquierdo en 0 y el derecho espejado en `Depth`, planta X = fondo.
- En Push Back los cuatro cortes comparten la reticula de columnas; bajo RT **no** se intercambian A y B.
- Que produccion coercione una seccion **no** la vuelve inequivoca. La planta y la cabecera se canonizan a `−1` porque
  tienen un unico significado posible, no por la coercion.
- La canonizacion se **reporta** en el resumen del comando (vista, seccion cruda y canonica).

### 3.8 Admisibilidad estricta y verificacion de todas las vistas admisibles

```text
Para cada referencia seleccionada r de un grupo con kind k:
  si no existe reflector registrado para k                             ⇒ FALLO (E2)
  si (View(r), Section(r)) no decodifica segun §3.7                     ⇒ FALLO de TODA la operacion (E4)
  si la vista semantica de r no expone μ_k                               ⇒ FALLO de TODA la operacion (E5)

Para cada rack logico reflejado (grupo), con D' = μ_k D:
  A_k(D') = TODAS las (View, Section canonica) admisibles que el rack podria materializar despues con Insertar
  para cada (V, σ) ∈ A_k(D'): exigir la conmutacion de §3.2 (V-VIEW-ALL, §6.6)
  alguna (V, σ) ∈ A_k(D') no conmuta                                    ⇒ FALLO del rack logico (E6), aunque no este seleccionada
No hay interseccion de ejes: μ_k es unica por kind.
```

`A_k` se calcula **solo** desde el diseño (operacion pura): no busca hermanas en el dibujo, no las añade a la seleccion y
no crea vistas.

| Kind | `A_k(D')` |
|---|---|
| Selectivo | frontal de cada fondo `k` con `0 ≤ k < SelectiveDepthLayout.Count(sistema)`; planta |
| Dinamico | frontal salida (`0`); frontal entrada (`1`); planta |
| Push Back | frontales semanticamente validas para ese diseño (simple: `0`, `1`; compuesto: `0..3`); planta |
| Cantilever | frontal; planta |
| Cabecera | lateral; planta |

### 3.9 Alternativas registradas, no adoptadas

- **ALT-F — fidelidad fisica:** admitir las vistas que no exponen `μ_k` con `P' = G·P·X_c`: se refleja la huella y el
  contenido no se voltea. Fisicamente correcto pero graficamente distinto de MIRROR. Requiere decision del Owner.
- **ALT-L — «vista desde el lado opuesto»:** propiedad de vista en el sobre que el builder respete con anotaciones
  legibles. Cambio de modelo con ADR propio. Es la via para laterales, la vista primaria del Dinamico y la cama.
- **ALT-μ2 — segunda reflexion por kind** (intercambio A↔B en la lateral de Push Back compuesto; R_Y en la lateral de
  Cantilever de estacion sencilla): contraria a PDC-4. Registrada como futuro; no se implementa en I-52.
- **ALT-ABS — medio frente que absorbe en el primer tramo:** permitiria reflejar un medio frente cuyo ancho depende de
  una variable. Es cambio de modelo (el modelo actual siempre absorbe en el ultimo tramo); futuro.

---

## 4. Contrato de fuente, linea y tolerancias

### 4.1 Fuente (por referencia seleccionada, en SNAPSHOT/PREFLIGHT)

Fuente admitida = `BlockReference` de Model Space, no MINSERT, no dinamica, BTR no anonimo y no anotativo,
`Normal = +Z`, rotacion finita, escala uniforme, definicion con `Origin = 0` y `View`/`Section` decodificables.

| # | Condicion | Resultado |
|---|---|---|
| ST-1 | Objeto no `BlockReference` | ignorar + aviso (clasificacion de I-51) |
| ST-2 | `BlockReference` sin payload RackCad **ni** en su BTR **ni** en su `DynamicBlockTableRecord` | ignorar + aviso |
| ST-3 | `MInsertBlock` (subclase de `BlockReference`) | **fail-closed** de la operacion |
| ST-4 | Fuera de Model Space (`OwnerId` ≠ Model Space) | ignorar + aviso (PD-7 de I-51) |
| ST-5 | Angulo entre `Normal` y +Z mayor que `GeometryTolerance.Angle` | **fail-closed** |
| ST-6 | `Rotation` no finita | **fail-closed** |
| ST-7 | Valores absolutos de `sx`, `sy` y `sz` distintos segun la tolerancia de escala (§4.3; valor en G4) | **fail-closed** |
| ST-8 | `sx < 0` y `sy < 0` con `sz > 0` | se canoniza a `(s,s,s)` con `θ + π` (exacto, §3.3) |
| ST-9 | Exactamente una de `sx`, `sy` negativa (MIRROR nativo, semilla espejada de RACKLAYOUT o RACKDUPLICAR) | **fail-closed** |
| ST-10 | `sz < 0` | **fail-closed** |
| ST-11 | Escala uniforme positiva `s ≠ 1` | admitida; `P'` conserva `s` |
| ST-12 | Z de `Position` | se conserva (el plano de reflexion es vertical) |
| ST-13 | Capa | la referencia nueva usa la capa de **su** referencia fuente (como I-51) |
| ST-14 | `BlockTableRecord.Origin` de la definicion fuente con alguna componente de valor absoluto mayor que `GeometryTolerance.Length` | **fail-closed** (BASE/BEDIT movido no se soporta; `RackCloner.cs:34` copia el origen de la fuente) |
| ST-15 | `View`/`Section` que no decodifican segun §3.7 | **fail-closed**; las decodificables dan `σ'` canonica |
| ST-16 | Huella de la referencia y read-set del registro (§9.2) | se capturan en SNAPSHOT y se re-verifican en PREPARE y en MUTATE |
| **ST-17** | `reference.IsDynamicBlock`, o `BlockTableRecord.IsAnonymous`, o definicion anotativa (`BlockTableRecord.Annotative`), **cuando** el BTR de la referencia o su `DynamicBlockTableRecord` llevan payload RackCad | **fail-closed explicito (E4)**, con mensaje propio. Nunca cae en ST-2 como «no rack». Sin payload RackCad en ninguno de los dos, es ST-2 |

RackCad nunca crea referencias dinamicas, anonimas ni anotativas para sus sistemas (`LateralHeaderDrawer.cs:243`;
`CantileverViewMaterializer.cs:47`) y el barrido de envolventes ya ignora los BTR anonimos (`RackBlockFinder.cs:66`): un
rack asi solo aparece si el usuario lo transforma con BEDIT o comandos equivalentes, y sus acciones (volteo, estiramiento)
son una transformacion que `P` no captura.

### 4.2 Linea

| # | Condicion | Resultado |
|---|---|---|
| LN-1 | Dos puntos con `GetPoint`, en UCS | se convierten a WCS con `CurrentUserCoordinateSystem` (patron de `RackDuplicarCommands.cs:145`) |
| LN-2 | Eje Z del UCS no paralelo a Z de WCS | fin sin mutacion |
| LN-3 | Puntos coincidentes tras proyectar a XY (distancia ≤ `GeometryTolerance.Length`) | fin sin mutacion (se puede repetir la peticion) |
| LN-4 | Enter/Esc | fin sin mutacion |

La conversion UCS→WCS y el uso de `Matrix3d` viven **solo** en el Plugin. Application recibe dos `Point2D` de WCS.

### 4.3 Tolerancias

La autoridad del repositorio usa tolerancias **absolutas** en pulgadas y rechaza el epsilon relativo por diseño
(`Vector2D.cs:82-89`). V3 no inventa ninguna tolerancia relativa.

| Magnitud | Autoridad existente | Valor | Uso en I-52 |
|---|---|---|---|
| Longitud | `GeometryTolerance.Length` (`Vector2D.cs:93`) | `1e-9` in | posiciones, `Origin` (ST-14), `Insertion` y `ConnectionAnchor` en V-VIEW-ALL, descomposicion de `P'`, LN-3, desbordes de fila |
| Continuidad | `GeometryTolerance.Continuity` (`:97`) | `1e-7` in | valores calculados por normalizacion (N-S02 invariante) y contornos Cantilever |
| Angulo | `GeometryTolerance.Angle` (`:100`) | `1e-9` rad | rotaciones, `Normal` (ST-5) |
| Marco ortonormal | `LocalFrame3D.OrthonormalTolerance` (`Spatial3D.cs:120`) | `1e-9` | referencia; **no** se reutiliza para escalas |
| BOM | reglas propias de cada builder y de `ConsolidatedBom` (`ConsolidatedBom.cs:62-64`, `:110-115`) | segun autoridad | V-BOM (§11.5) |
| Factor de escala (adimensional) | **no existe** | — | ST-7 y ST-8. G4 fija el valor, absoluto y no relativo, con prueba y registro |

---

## 5. Seleccion, agrupacion e identidad (reutilizacion de I-51)

### 5.1 Nucleo neutral + fachada compatible (Application)

Se extrae de `RackDuplicationPlan` (`RackDuplicationPlan.cs:225-582`) **solo** lo neutral:

- clasificacion de fuentes (cuatro clases de I-51);
- clave logica discriminada `RackId` / `Definition(handle)`;
- agrupacion y deduplicacion de definiciones conservando todas las referencias;
- consistencia de `Kind` y nombre del grupo.

| Pieza | Regla |
|---|---|
| Nucleo neutral (Application, puro) | Vocabulario de mensajes **inyectado**. Sin nombre de copia, sin `Guid`, sin tipos de AutoCAD |
| `RackDuplicationPlan` | **Fachada**: misma API publica, mismos textos **byte a byte**, mismo orden observable, misma politica de nombre «- copia», misma autoridad Selectiva. T1–T15 de I-51 sin cambios |
| Semantica observable historica | **CT-16 en G3**, sobre codigo intacto y antes de cualquier extraccion, fija: textos completos de cada mensaje visible, parametrizados (`RackDuplicationPlan.cs:326`, `:407`, `:414`, `:420`, `:425`, `:432-434`, `:450`, `:455`, `:460`, `:518-519`, `:530-532`, `:573-574`, `:580-581`, `:626`, `:632`, `:638` y la politica de `:656`); **orden de grupos**; **orden de referencias** dentro de cada grupo; **orden y contenido de avisos**; **orden y contenido de errores**. No se confia en las pruebas actuales, que usan `Contains` (`RackDuplicationPlanTests.cs:333`, `:344`, `:401`, `:432`, `:509`, `:525`). Las excepciones de programacion (`:258`, `:282`, `:364`, `:379`, `:385`) se fijan por tipo |
| Asignador de identidad | Se reutiliza su validacion (`Guid.Empty`, repeticion, igualdad con una fuente) con **politica de nombre inyectada**. RACKDUPLICAR pasa la historica; RACKMIRROR pasa `<base> - espejo` y un unico destino |
| Clone path de RACKDUPLICAR (`RackCloner`, PREPARE/MUTATE de copia) | **No** se reutiliza |

Editar `RackDuplicationPlan.cs` **es** tocar RACKDUPLICAR: antes de editarlo se avisa a I-49, cuya condicion de parada
cubre sus propios gates (I-49 V6 §12.2) y que ya registra el cruce (I-49 V6 §12.3); rigen las guardas en RED primero,
Core completo, la regresion de I-51 y M-17 en el Candidato. Si G4 no puede demostrar la equivalencia de la fachada:
**STOP → Proposal V4**. No se duplica la agrupacion en un segundo planificador. Las guardas G-R1..G-R6 leen el codigo de
`RackDuplicarCommands.cs` y `RackEnvelopeRestamp.cs`, no el de `RackDuplicationPlan.cs`
(`SelectiveDuplicationFailClosedTests.cs:323-587`).

### 5.2 SNAPSHOT propio de RACKMIRROR (Plugin)

- RACKMIRROR tiene **su propio** SNAPSHOT neutral, en un archivo nuevo del Plugin. Captura por referencia: handle,
  `OwnerId`, tipo (`MInsertBlock`), `IsDynamicBlock`, `DynamicBlockTableRecord`, `BlockTableRecord`, `IsAnonymous`,
  `Annotative`, `Position`, `Rotation`, `ScaleFactors`, `Normal`, capa, `Origin` de la definicion y payload. Lee el
  registro **una** vez por comando y registra su read-set (§9.2).
- Migrar el SNAPSHOT de RACKDUPLICAR a un helper compartido **no es obligatorio**. Solo se hace si aporta un beneficio
  claro y se demuestra la equivalencia; en ese caso: G-R1, G-R2, G-R4, G-R6 y la guarda de cableado de I-51 **vistas
  en RED primero** y reapuntadas con proteccion igual o mayor, Core completo, regresion especifica de I-51 y M-17.
- Si no se migra, `RackDuplicarCommands.cs` y las guardas de I-51 quedan **intactos** (G-M12).

### 5.3 Plan del espejo (Application, puro)

Compone el nucleo neutral y añade, en el orden normativo de §9.1: decodificacion de vista y seccion (§3.7),
transformacion fuente (§4.1), admisibilidad (§3.8), autoridad authored (§5.4), precondiciones del registro (§10.2),
fidelidad del store y metadata semantica (§6.3, §6.4), representabilidad y reflexion en el sustrato real (§6, §7),
cobertura de portadores (§6.4), V-VIEW-ALL y V-BOM (§6.6), nombre e identidad (§5.5) y composicion del sobre con
`RackEmbedComposer.Compose`. Entrada y salida son datos planos: sin `ObjectId`, sin `Matrix3d`, sin transacciones.

El plan de Application **termina en el documento compuesto**. El re-estampado de identidad lo hace
`RackEnvelopeRestamp`, que vive en el Plugin y despacha el restamp interior por `KindHandlerRegistry`
(`RackEnvelopeRestamp.cs:52-111`); su ensayo lo ejecuta el comando en PREFLIGHT, sin transaccion ni mutacion.

### 5.4 Autoridad authored del grupo (todos los kinds)

- Selectivo: `SelectiveAuthoredAuthority.IsSameAuthority` (`SelectiveAuthoredAuthority.cs:96-121`), sin cambios.
- Demas kinds: comparador **por kind** declarado por su reflector (`IsSameAuthority`), estructural sobre el sustrato del
  kind (§6.2) y **excluyendo** los metadatos propios de cada vista que `RACKEDITAR` preserva por hermana
  (`RackCommandSupport.cs:40-67`).
- Divergente o ilegible ⇒ fail-closed (E3). Nunca se elige una vista.
- Solo en el plan del espejo: **RACKDUPLICAR no cambia**.

### 5.5 Identidad y nombre

| Regla | Contenido |
|---|---|
| ID-1 | Un `NewRackId` (`Guid`) por grupo logico, compartido por todas sus vistas seleccionadas |
| ID-2 | `OldRackId → NewRackId` solo en memoria (asignacion del plan); **no** se persiste |
| ID-3 | Legacy sin `RackId`: clave `Definition(handle)` dentro del lote; la copia recibe `RackId` real (precedente I-51) |
| ID-4 | Nombre logico `<base> - espejo` identico en sobre, identidad interior y anotacion de nombre de todas las vistas del grupo |
| ID-5 | Orden obligatorio: **reflejar** en el sustrato → serializar con el store del kind → `RackEmbedComposer.Compose(sobreFuente, kind, idFuente, nombreFuente, ViewCanonica, σ', diseñoReflejado)` → `RackEnvelopeRestamp.RestampEnvelope(payload, nombreEspejo, NewRackId)` (Plugin; NI-1..NI-6 de I-51). Cumple el invariante D-21 de I-54 (mecanismo A y despues B) |
| ID-6 | `RackEnvelopeRestamp.cs` **no** se modifica (G-R5 intacta; T-GRD-03 de I-54 convive) |
| ID-7 | Referencias enlazadas (misma definicion) seleccionadas ⇒ **una** definicion nueva y N referencias |

---

## 6. Reflectores

### 6.1 Contrato conceptual comun (Application, sin tipos de AutoCAD)

Nombres **no** contractuales; las responsabilidades si. El contrato es comun; **el sustrato no**: cada adaptador de
kind trabaja sobre la autoridad real de ese kind (§6.2). No se introducen DTOs para homogeneizar interfaces.

```text
IRackDesignReflector
  Kind                                  // clave del sobre ("selective", "dynamic", ...)
  CanonicalReflection                   // RUN | RT | LINE_X | DEPTH_D | NONE
  Assess(source)        → RepresentabilityReport { por fila: RepresentabilityClass, EvidenceStatus,
                                                   OperationalDisposition, motivo atribuible }
  Reflect(source)       → ReflectionResult { payload interior reflejado, normalizaciones aplicadas,
                                             cambios authored declarados }
  DescribeView(source, view, section) → ViewExposure { decodificacion §3.7, Expone, F (Transform2D), c, σ', motivo }
  AdmissibleViews(reflected)          → A_k (§3.8)
  IsSameAuthority(sources) → bool

source = { payload interior del sobre (JSON), catalogos cargados, lectura del registro de variables (solo lectura) }
```

| Requisito | Regla |
|---|---|
| RF-1 Despacho | `KindDispatch<IRackDesignReflector>` en Application (`KindDispatch.cs:19-104`), busqueda sin mayusculas. El comando **no** contiene constantes `Kind*`, tipos concretos ni ramas por kind |
| RF-2 Sustrato | Cada adaptador lee y escribe con la **autoridad existente** de su kind (§6.2). El JSON es solo la frontera del sobre: **prohibida** la manipulacion de JSON para escribir |
| RF-3 Orden | `Assess` antes de `Reflect`. `Reflect` sobre algo cuya disposicion no sea `ALLOW`, `CANONICALIZE` o `BASELINE_LIMITED` ⇒ error de programacion |
| RF-4 Pureza | `Reflect` es puro y determinista; no lee el reloj, no genera ids y no escribe el registro |
| RF-5 Selectivo | Reflexion sobre una copia en memoria del DTO. **Prohibido `WithDesign`** (`SelectivePalletDesignDocument.cs:326-340`) y prohibido materializar vinculos |
| RF-6 Dominio | Dinamico, Push Back, Cantilever y cabecera: deserializar con la autoridad existente → reflejar el estado del kind → reconstruir con `RackProject.For*` → `WithSourceMetadataFrom(fuente)` → serializar con `RackProjectStore`. Sin DTO nuevo y sin cambio de schema |
| RF-7 Portadores | Se copian **desde el origen** dentro del sustrato (§6.4); nunca se recalculan |
| RF-8 Normalizaciones | Declaradas, reportadas y validas solo con las nueve obligaciones de §6.5 |
| RF-9 Fidelidad del store | V-RT-STORE (§6.3); nunca se justifica con el camino `EditX` |
| RF-10 Metadata semantica | V-META (§6.4): miembro desconocido no vacio en el payload semantico ⇒ `UNKNOWN` |
| RF-11 Cobertura | Cada miembro material del sustrato tiene exactamente una clasificacion (§6.7) |
| RF-12 Dependencias | Ninguna regla ni normalizacion escribe en el documento un valor calculado a partir del registro (§3.2, consecuencia 5; §6.5, obligacion 9) |

### 6.2 Sustrato real por kind y fidelidad del store

| Kind | `Design` del sobre | Lectura | Se refleja | Escritura | Fidelidad del store (lo que conserva, pierde y normaliza el propio store) |
|---|---|---|---|---|---|
| selective | JSON de `SelectivePalletDesignDocument` | `SelectivePalletDesignStore.Deserialize` | copia en memoria del DTO | `SelectivePalletDesignStore.Serialize` | conserva todos los miembros tipados del DTO, la raiz `ExtensionData` (`SelectivePalletDesignDocument.cs:150`), `PropertyValues` con `Kind` desconocido y su `ExtensionData` (`SelectivePropertyValueDocument.cs:25-38`), `SchemaVersion` sticky (`SelectiveDesignSchema.cs:43-66`) y `DimensionViews` (`SelectivePalletDesignDocument.cs:108-116`; nulo no se escribe). Descarta al leer los miembros desconocidos anidados en tipos sin `ExtensionData` |
| dynamic | JSON de `RackProjectDocument { Kind = PalletFlow, DynamicSystem }` | `RackProjectStore.Deserialize` → `RackProject.DynamicDesign` | `DynamicRackDesign` | `RackProject.ForDynamic(diseño).WithSourceMetadataFrom(fuente)` → `RackProjectStore.Serialize` (`RackDinamicoCommands.cs:379`) | conserva `ExtensionData` y `SchemaVersion` del envoltorio (`RackProjectStore.cs:71-83`) y `DimensionViews` (`DynamicRackSystemDocument.cs:63-72`). El payload **no** tiene `ExtensionData`: descarta todo miembro desconocido del payload. Escribe explicitos los defectos de campos ausentes (`DynamicRackSystemDocument.cs:196-211`) |
| pushback | JSON de `RackProjectDocument { Kind = PushBack, PushBack }` | idem → `RackProject.PushBackDesign` | `PushBackDesign` | `RackProject.ForPushBack(diseño).WithSourceMetadataFrom(fuente)` (`RackPushBackCommands.cs:414`) | conserva envoltorio y raiz del payload (`PushBackDesignDocument.cs:75`; `SystemRegistry.Default.cs:168`) y `DimensionViews` de la estructura; descarta anidados desconocidos |
| cantilever | JSON de `RackProjectDocument { Kind = Cantilever, Cantilever }` | idem → `RackProject.CantileverLineDesign` | `CantileverLineDesign` | `RackProject.ForCantilever(diseño).WithSourceMetadataFrom(fuente)` (`RackCantileverCommands.cs:490-491`) | conserva envoltorio y raiz del payload (`CantileverLineDocument.cs:46`; `SystemRegistry.Default.cs:220-221`); descarta anidados desconocidos y los margenes retirados de troquel (`RackProjectStore.cs:374-400`) |
| cabecera | JSON de `RackProjectDocument { Kind = Selective, Header }` o cabecera legada sin `kind` | idem → `RackProject.Header` | `RackFrameConfiguration` (con `RefreshPhysicalModel`, como el store) | `RackProject.ForSelective(config).WithSourceMetadataFrom(fuente)` (`RackCabeceraCommands.cs:194`) | conserva el envoltorio. El payload `RackFrameProjectDocument` **no** tiene `ExtensionData` y escribe `SchemaVersion = "1.0"` (`RackFrameProjectDocument.cs:15-17`; `RackProjectStore.cs:56-57`); una cabecera legada sin envoltorio sale con envoltorio (`RackProjectStore.cs:313-336`) |

`RackProject.SourceDocument` es `internal` (`RackProject.cs:60`); el adaptador solo usa la costura publica
`WithSourceMetadataFrom` (`:75-79`). Si un adaptador necesitara otra costura del store: **STOP** y revision de
Arquitecto.

### 6.3 Dos obligaciones distintas: V-RT-STORE y V-EDIT (C-2)

| Obligacion | Que demuestra | Linea base | Cuando |
|---|---|---|---|
| **V-RT-STORE** — fidelidad del store | que el espejo no pierde nada que el **store del kind** conserva | `LB(D) = S_k.Write(S_k.Read(D))` con `S_k` de §6.2. **No** es el camino de `RACKEDITAR` | PREFLIGHT, por rack, sobre el payload final |
| **V-EDIT = C-2** — estabilidad operativa del redibujo | que espejo → guardar/reabrir → `RACKEDITAR` → Actualizar → mismo dibujo | los cuatro productores de §8.5 | G6 (pruebas en CI) y M-4 (Owner) |

```text
Miembros(X) = conjunto de rutas de esquema (indices de lista sustituidos por []) con valor no vacio en el payload semantico X
M           = payload semantico final: reflejado → Compose → ensayo de restamp
V-RT-STORE pasa ⇔ Miembros(LB(D)) ⊆ Miembros(M) ∪ Vaciados(μ_k, N)
                ∧ S_k.Write(S_k.Read(M)) = M      (idempotencia; CT-35 la caracteriza por kind)
Vaciados(μ_k, N) = miembros que una regla o normalizacion declarada vacia (hoy: ninguno)
```

- Un defecto historico del camino `EditX` (por ejemplo, la re-congelacion de `PalletTolerance` vinculada que CT-07
  caracteriza, o la normalizacion de `WithDesign`, `SelectivePalletDesignDocument.cs:326-340`) **no** puede usarse
  para autorizar una perdida adicional del espejo: V-RT-STORE compara contra el store, no contra `EditX`.
- La metadata **desconocida** no la cubre V-RT-STORE sino V-META (§6.4).

### 6.4 Portadores y metadata semantica (V-META)

**Dos categorias.**

| Categoria | Definicion | Tratamiento |
|---|---|---|
| **Portador** (`carrier metadata`) | Metadata cuyo contrato es **conservarse exactamente**, sin interpretacion por orientacion | se copia desde el origen; nunca bloquea por si misma |
| **Metadata semantica authored** | Todo miembro del **payload semantico** que se somete a `μ_k` | se clasifica (§6.7) y se transforma segun su regla; **desconocida y no vacia ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`** |

**Alcance por kind.**

| Kind | Payload semantico (sometido a `μ_k`) | Portadores |
|---|---|---|
| selective | JSON de `SelectivePalletDesignDocument`, raiz y anidados, **excepto** el subarbol `PropertyValues` | sobre `RackEmbedDocument` (miembros declarados y `ExtensionData`, incluida la propiedad `CustomProperties` de I-54 antes y despues de integrarse); `PropertyValues` completo con su `ExtensionData` (autoridad de I-47/I-49); `SchemaVersion` |
| dynamic | subarbol `DynamicSystem` | sobre; envoltorio `RackProjectDocument` (`SchemaVersion`, `ExtensionData`) |
| pushback | subarbol `PushBack` | sobre; envoltorio |
| cantilever | subarbol `Cantilever`, **excepto** los miembros retirados que el store declara y descarta (`RackProjectStore.cs:374-400`) | sobre; envoltorio |
| cabecera | subarbol `Header` o la cabecera legada completa | sobre; envoltorio si existe |

**Portadores preservados desde el origen.**

| Portador | Kinds | Sustrato |
|---|---|---|
| `SchemaVersion` interior y del sobre | todos | DTO Selectivo (sticky); envoltorio via `WithSourceMetadataFrom`; raiz de Push Back y Cantilever via `FromDomain(…, fuente)`; sobre via `Compose` |
| `ExtensionData` del sobre y del envoltorio | todos | `Compose` (`RackEmbedComposer.cs:21-35`); `RackProjectStore.cs:71-83` |
| `PropertyValues` completo (incluidos `Kind` desconocidos, `expression` de I-49), literales congelados y `VariableId` | Selectivo | DTO (`SelectivePropertyValueDocument.cs:25-38`; `LinkedPropertyReconciler.cs:193-220`; ADR-0034 §12) |
| `DimensionViews` (I-50, **integrada**) | Selectivo, Dinamico, Push Back | DTO en Selectivo; dominio en Dinamico y Push Back (sitios de copia C-01..C-15 de I-50). Requisito normativo: el `int` exacto, bits desconocidos y `null` incluidos |
| Propiedades de alcance Rack (I-54) | todos | miembro `CustomProperties` del sobre (I-54 V3: `JsonElement?`, heredado por `Compose`; antes de integrarse viaja en `ExtensionData` del sobre) — D-21 |
| Cualquier portador integrado en `main` antes del Candidato | todos | §15 |

**Regla V-META.**

```text
Desconocido(X) = miembros del payload semantico X que el modelo tipado del store no liga en esa posicion,
                 que no son retirados declarados por el store, y cuyo valor no es vacio (null, {} o [])
V-META pasa ⇔ Desconocido(D) = ∅  ∨  todo miembro de Desconocido(D) figura en la declaracion tipada de invariancia
                                      o de transformador (Application; vacia en el primer corte)
V-META falla ⇒ RepresentabilityClass = UNKNOWN, OperationalDisposition = FAIL_CLOSED (E6), con la ruta del miembro
```

- La deteccion compara el JSON crudo del payload con el modelo tipado del store (CT-32); vale para miembros anidados
  aunque el store los descarte al leer.
- V-META **no** bloquea por portadores: `ExtensionData` exterior del sobre, envoltorio `RackProjectDocument`,
  `PropertyValues` ni `CustomProperties` de I-54.
- Una declaracion de invariancia o de transformador exige evidencia (CT y prueba) y vive en un unico registro tipado en
  Application; ninguna existe en el primer corte.

### 6.5 Normalizaciones

Una normalizacion `N` solo es valida si **todo** se cumple y se prueba:

1. `Π_(V,σ)(Eff(N D, R)) ≡ Π_(V,σ)(Eff(D, R))` para **cada** vista del kind, no solo las admitidas.
2. `BOM(Eff(N D, R)) = BOM(Eff(D, R))` con las dos comprobaciones de §11.5.
3. `PropertyValues`, literales y `VariableId` identicos.
4. Portadores de §6.4 identicos.
5. V-RT-STORE sobre `N D` (§6.3).
6. El cambio de representacion authored, si existe, se **declara** en el reporte: campo, valor anterior y nuevo.
7. El editor carga `N D` mostrando la misma configuracion efectiva (CT de UI).
8. Involucion: `Reflect(Reflect(D)) = N(D)`, con `N` la forma normal declarada.
9. **Conmutacion con la resolucion efectiva**: para **todo** estado acreditable `R` de las propiedades vinculadas de
   las que dependa el valor que `N` escribe,

   ```text
   N(Eff(D, R))  ≡  Eff(N(D), R)
   ```

   Si el valor que `N` escribe se calcula a partir de una propiedad vinculada, la obligacion **no** se cumple: la fila
   es `REQUIRES_MODEL_CHANGE` (el modelo no puede expresar la dependencia) o `UNKNOWN` (dependencia no demostrable).

#### 6.5.1 N-S02 — medio frente (Selectivo) y dependencia efectiva

```text
Bay i con Segments s_0..s_{n−1}, n ≥ 2, que RESUELVE (SelectiveMedioFrente.Resolve ≠ null, SelectiveMedioFrente.cs:55-78)
overhead = 2·(troquelX + inicioX)                                            (:10-17, :60)
r        = BeamLength_i − Σ_{j<n−1} s_j.Length − (n−1)·overhead                (remanente del ultimo tramo, :70)
Reflejado:  s'_k = s_{n−1−k}   (Loaded incluido), con
            s'_0.Length     = r            (antes derivado; ahora explicito)
            s'_{n−1}.Length = s_0.Length   (queda almacenado e ignorado por Resolve)
Forma normal N: s_{n−1}.Length := r
Involucion: Reflect(Reflect(D)) = N(D); s_0.Length se recalcula dentro de GeometryTolerance.Continuity
```

**Dependencia efectiva de `BeamLength_i`** (regla del codigo, `main @ f8deb67`):

```text
BayBeamLength(bay) = max sobre sus celdas de ( BeamLengthOverride > 0 ? BeamLengthOverride
                                                                     : frente·count + PalletTolerance·(count+1) )
                                                                     (SelectiveGeometryResolver.cs:369-392)
Ancho compartido del indice i = BayBeamLength del claro i del fondo MAESTRO; si ese claro maestro no tiene niveles
                                (columna vacia, ancho 0), el mayor BayBeamLength real del indice i entre los fondos
                                                                     (SelectiveGeometryResolver.cs:138-176)
Conjunto gobernante(i) = celdas del claro i del fondo maestro; si ese claro no tiene niveles, celdas del claro i de todos
                         los fondos que tienen niveles
Dep(i) ⇔ existe un vinculo sano de una propiedad que alimenta BayBeamLength (hoy: PalletTolerance,
         SelectiveLinkedProperties.cs:208-213; efectivo en SelectiveGeometryResolver.cs:96, :258)
         ∧ alguna celda del conjunto gobernante(i) no tiene BeamLengthOverride > 0
```

| Condicion del claro dividido `i` | Clase | Evidencia | Disposicion |
|---|---|---|---|
| `¬Dep(i)`, postes `f` y `f+1` con mismo peralte y troquel efectivos | `REPRESENTABLE_BY_NORMALIZATION` | `G3_PENDING` | `CANONICALIZE` |
| `Dep(i)` | `REQUIRES_MODEL_CHANGE` | `CODE_SUPPORTED` | `FAIL_CLOSED` |
| postes `f` y `f+1` con peralte o troquel distintos | `REQUIRES_MODEL_CHANGE` | `CODE_SUPPORTED` | `FAIL_CLOSED` |
| `Segments ≥ 2` que hoy **no** resuelve | `UNKNOWN` | `CODE_SUPPORTED` | `FAIL_CLOSED` |

- **No** basta con «`PalletTolerance` vinculada ⇒ MC»: si todas las celdas del conjunto gobernante tienen
  `BeamLengthOverride > 0`, el ancho es realmente independiente y la fila sigue RN.
- `VerticalClearance` (la otra propiedad vinculable hoy) no alimenta `BayBeamLength`. Toda propiedad vinculable nueva se
  clasifica en la guarda de cobertura (§6.7); sin clasificar, **RED / STOP**.
- **Cambio authored declarado** (fila RN): el `Length` del nuevo primer tramo pasa de derivado a explicito y el `Length`
  ignorado del ultimo tramo se reescribe.
- **CT-21** (UI, BOM, round-trip, involucion), **CT-27** (dos estados del registro: con `Dep(i)` el espejo deja de
  conmutar; sin `Dep(i)` conmuta en ambos) y **CT-28** (regla del ancho compartido y columna vacia).

**S-14 — desviador asociado.** El indice de `DesviadorOffCells` es el orden de los postes cargados, intermedios
incluidos (`SelectiveDesviadorPlan.cs:114-148`), y los intermedios existen solo si el medio frente resuelve. Con off-cells
y algun claro dividido con `Dep(i)`, el remapeo `c → P−1−c` puede dejar de ser valido tras un `ChangeValue` que cambie
el ajuste ⇒ **S-14c = `UNKNOWN` → `FAIL_CLOSED`**. En la practica S-02b ya hace fallar ese rack; S-14c se declara igual
para que la regla no dependa de esa coincidencia. **CT-29** y **T-M32** con dos estados del registro.

#### 6.5.2 N-S06 — `PostPeraltes` (Selectivo)

```text
K = M + 1   (postes del grid maestro; SelectiveGeometryResolver.cs:81-88)
L = PostPeraltes
1. si L.Count < K: añadir 0.0 hasta K           marcador authored de herencia que escribe el editor (SelectiveEditorState.cs:744)
2. L'[p] = L[K−1−p]  para 0 ≤ p < K               valores tal cual: un −1 almacenado sigue significando «hereda»
3. L'[i] = L[i]      para i ≥ K                   cola que el resolvedor ignora (SelectiveGeometryResolver.cs:89-93), sin mover
Forma normal N(L) = pasos 1 y 3
Involucion: Reflect(Reflect(L)) = N(L)
```

- El store conserva la lista tal cual (`SelectivePalletDesignDocument.cs:270`, `:400-402`). **Nunca** se rellena con el
  `PostPeralte` resuelto. `PostPeralte` no es vinculable, asi que la obligacion 9 se cumple.
- **CT-22** caracteriza: ceros finales, listas cortas, valores `≤ 0` distintos de `0.0`, listas mas largas que `K`
  (el editor las recorta al abrir, `SelectiveEditorState.cs:745`) e involucion.

#### 6.5.3 N-S07 — `PostCabeceras` y `ExtraFondoPostCabeceras` (Selectivo)

```text
Marcador authored de «estandar» = null (SelectiveGeometryResolver.cs:185-205)
PostCabeceras:                rellenar con null hasta K, invertir el prefijo de K, conservar la cola ignorada
ExtraFondoPostCabeceras:
  ausente (null)              ⇒ sigue ausente: NO se crean filas
  fila k presente             ⇒ rellenar con null hasta C_k + 1 (= K, porque S-01 exige C_k = M), invertir, conservar la cola
  fila k ausente o nula       ⇒ sigue ausente o nula (una fila corta o ausente ya significa «estandar»)
Forma normal N: el relleno con null solo dentro de filas que existen
```

La distincion ausencia/fila explicita la escribe el propio store (`SelectivePalletDesignDocument.cs:259-268`) y se
preserva.

#### 6.5.4 N-H02 — `DiagonalDirection` (cabecera)

```text
UpRight (1) ↔ UpLeft (2)                                          explicitas: se intercambian
AutoAlternating (0): d = ResolveDiagonalDirection() (BracingPanel.cs:40-48); se fija swap(d)     Auto → explicita
otro valor numerico ⇒ UNKNOWN
Forma normal N: cada Auto sustituido por su direccion efectiva explicita
Involucion: Reflect(Reflect(D)) = N(D)
```

**Cambio authored declarado:** la intencion «alternar automaticamente» pasa a una direccion fija; un cambio posterior
de `Number` ya no re-alterna la copia. La cabecera no consume el registro, asi que la obligacion 9 se cumple. **CT-23**
caracteriza fisica, BOM, UI, round-trip y la relacion con `IsStandard`, `IsException` y `StandardBaselineId`. Mientras
H-06 sea `UNKNOWN`: `AutoAlternating` en un panel con diagonal **y** `StandardBaselineId` no vacio ⇒ `FAIL_CLOSED`.

### 6.6 Verificacion dinamica por rack (PREFLIGHT)

Ademas de las reglas estaticas de §7, para cada rack logico el plan del espejo **verifica** sobre el rack concreto, en
el orden de §9.1:

```text
V-META      metadata semantica desconocida (§6.4)
V-DEP       reglas de dependencia efectiva de §6.5 (obligacion 9)
V-VIEW-ALL  para cada (V,σ) ∈ A_k(μ_k D):  Π_(V,σ')(Eff(μ_k D, R)) ≡ F ∘ Π_(V,σ)(Eff(D, R))     (§3.8, §11)
V-BOM       las dos comprobaciones de §11.5
V-RT-STORE  §6.3, sobre el payload final (tras el ensayo de restamp)
```

- V-VIEW-ALL cubre **todas** las vistas admisibles del rack, seleccionadas o no, y es operacion **pura**: no busca
  hermanas, no las añade a la seleccion y no crea vistas.
- Cualquier discrepancia ⇒ `UNKNOWN` material ⇒ `FAIL_CLOSED` (E6), con la vista, pieza, linea o miembro que difiere en
  el diagnostico. La verificacion dinamica es la garantia **ejecutable** del fail-closed para toda vista que la copia
  pueda materializar despues con Insertar.
- `R` es el estado del SNAPSHOT; la validez para estados futuros la garantizan las reglas estaticas de dependencia
  (V-DEP), no una prueba de todos los `R` en tiempo de ejecucion.

### 6.7 Guarda de cobertura estructural (por kind)

Evita que un miembro conocido nuevo se copie sin regla de espejo.

| Elemento | Contrato |
|---|---|
| Tipos cubiertos | `SelectivePalletDesignDocument` y sus anidados relevantes (`SelectiveBayDocument`, `SelectiveSegmentDocument`, `SelectiveCellDocument`, `SafetySelectionDocument`, `BootPostDocument`, `GridCellDocument`, `PostSideDocument`, `PostDefenseDocument`, `RackFrameProjectDocument` de las cabeceras por poste); `DynamicRackDesign` y anidados; `PushBackDesign` y anidados; `CantileverLineDesign` y anidados; `RackFrameConfiguration` y anidados; **descriptores vinculables** (`SelectiveLinkedProperties.All`) |
| Clasificacion | cada miembro material exactamente **una** vez: `INVARIANT`, `TRANSFORMED` (con la fila de §7 que lo transforma), `NORMALIZED` (con su N), `CARRIER`, `DERIVED` o `UNSUPPORTED` (con su fila `MC`/`UK`) |
| Naturaleza | ademas, cada miembro declara si es **persistido**, **derivado/no persistido** o **portador** |
| Descriptores vinculables | cada propiedad vinculable declara que geometria usada por una regla de espejo alimenta (hoy: `PalletTolerance` → `BayBeamLength`; `VerticalClearance` → elevaciones, sin efecto en indices ni en normalizaciones) |
| Donde vive | una tabla por kind en Application, junto a su reflector; **sin** `switch` por kind en el Plugin |
| Guarda | prueba Core (T-M36) que enumera los miembros publicos de los tipos cubiertos y exige clasificacion exacta; un miembro o descriptor nuevo sin clasificar ⇒ **RED / STOP** (nueva revision de la regla antes de integrarse) |
| Omisiones cerradas en V3 | escalares globales del Selectivo (`PostId`, `PostPeralte`, `PalletTolerance`, `VerticalClearance`, `FloorBeamRise`, `DepthCount`, fila S-04b); `DerivedAisles` de `SafetySelectionDocument` y del dominio (`SelectivePalletDesignDocument.cs:598`; `SelectivePalletDesign.cs:399-411`), clasificado `DERIVED` (S-22b) |

---

## 7. Matriz de representabilidad V3

### 7.0 Taxonomia: tres dimensiones

```text
RepresentabilityClass        (normativa; PDC-7)
  REPRESENTABLE                   R
  REPRESENTABLE_BY_NORMALIZATION  RN
  REQUIRES_MODEL_CHANGE           MC
  UNKNOWN                         UK

EvidenceStatus               (de donde sale la clasificacion)
  CODE_SUPPORTED    demostrada en codigo a SHA exacto
  G3_PENDING        depende de anclajes de builders, contenido de bloques o semantica que G3 debe caracterizar
  G3_VERIFIED       confirmada por la caracterizacion de G3 (ninguna fila en V3)

OperationalDisposition       (que hace el comando)
  ALLOW             continua, sujeta a V-META, V-DEP, V-VIEW-ALL, V-BOM y V-RT-STORE
  CANONICALIZE      continua tras llevar el estado a su forma canonica o normal declarada
  BASELINE_LIMITED  continua con la fidelidad limitada a la normalizacion declarada del store
  FAIL_CLOSED       termina la operacion antes de pedir la linea
```

- Las dimensiones son independientes: una fila `R` + `G3_PENDING` + `ALLOW` no esta verificada; una fila `UK` +
  `CODE_SUPPORTED` + `FAIL_CLOSED` significa «el codigo demuestra que no hay regla aprobada».
- «Deteccion» = condicion evaluada por rack en PREFLIGHT, antes de pedir la linea. `M` = frentes del grid maestro
  (Selectivo); `N` = frentes (Dinamico/Push Back) o estaciones (Cantilever).
- Un rack admite el espejo solo si **todas** sus filas aplicables tienen disposicion distinta de `FAIL_CLOSED` y pasan
  las verificaciones dinamicas (§6.6).
- Las precondiciones operativas (fuente, diseño invalido, bloques ausentes) no son clases de representabilidad: §7.9.
- Citas contra `main @ f8deb67`.

### 7.1 Selectivo — `μ_RUN`

| # | Propiedad / estado | Regla bajo μ_RUN | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| S-01 | Frentes por fondo con esquina (algun fondo con menos frentes que el maestro) | la alineacion al final no es expresable | MC | CODE_SUPPORTED | FAIL_CLOSED | `∃k: C_k < M` | `SelectiveGeometryResolver.cs:132-176` |
| S-01b | Orden de frentes con reticula completa (`Bays`, `ExtraFondoBays[k-1]`) | invertir `Bays` y cada lista explicita; un fondo que hereda `Bays` queda invertido por herencia | R | G3_PENDING | ALLOW | — | `SelectiveGeometryResolver.cs:211-225`; `SelectivePostGeometry.cs:32-48` |
| S-02a | Medio frente que resuelve, sin dependencia efectiva, postes `f` y `f+1` iguales | N-S02 | RN | G3_PENDING | CANONICALIZE | `¬Dep(i)` (§6.5.1) | `SelectiveMedioFrente.cs:55-78` |
| S-02b | Medio frente cuyo `BeamLength` efectivo depende de una propiedad vinculada | N-S02 congelaria el remanente | MC | CODE_SUPPORTED | FAIL_CLOSED | `Dep(i)` (§6.5.1) | `SelectiveGeometryResolver.cs:96`, `:138-176`, `:258`, `:369-392`; `SelectiveLinkedProperties.cs:208-213`; `SelectiveMedioFrente.cs:70` |
| S-02c | Medio frente con postes `f` y `f+1` de peralte o troquel distintos | overhead asimetrico | MC | CODE_SUPPORTED | FAIL_CLOSED | peralte y troquel efectivos | `SelectiveMedioFrente.cs:60`; `SelectiveDesviadorPlan.cs:214-226` |
| S-02d | `Segments ≥ 2` que hoy no resuelve | sin regla | UK | CODE_SUPPORTED | FAIL_CLOSED | `Resolve == null` | `SelectiveMedioFrente.cs:62-78` |
| S-03 | `PalletDepth`, `ExtraFondoDepths`, `CabeceraFondoOverrides`, `SeparatorLengths` | sin cambio | R | G3_PENDING | ALLOW | — | `SelectiveDepthLayout.cs:119-216` |
| S-04 | Grid maestro y desempate | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `SelectiveGeometryResolver.cs:138-176` |
| S-04b | Escalares globales: `PostId`, `PostPeralte`, `PalletTolerance`, `VerticalClearance`, `FloorBeamRise`, `DepthCount` | sin cambio (valores y vinculos intactos; los vinculos por S-18) | R | CODE_SUPPORTED | ALLOW | — | `SelectivePalletDesignDocument.cs:55-63` |
| S-05 | Celdas: pallet, larguero, `BeamLengthOverride`, `ClearOverride` | se mueven con su frente; valores intactos | R | G3_PENDING | ALLOW | — | `SelectiveCellAddress.cs:21-42`; `SelectiveFrontalBuilder.cs:138` |
| S-05b | Por frente: `FloorBeam`, `HeightOverride`, `FloorBeamRiseOverride` | se mueven con su frente | R | G3_PENDING | ALLOW | — | `SelectivePalletDesign.cs:699-720` |
| S-06 | `PostPeraltes[p]` | N-S06: marcador `0.0`, permutacion `p → M−p`, cola intacta | RN | G3_PENDING | CANONICALIZE | lista corta, larga o con `≤ 0` | `SelectivePalletDesign.cs:104-107`; `SelectiveGeometryResolver.cs:89-93`; `SelectiveEditorState.cs:744-745` |
| S-07 | `PostCabeceras[p]`, `ExtraFondoPostCabeceras[k-1][p]` | N-S07: relleno con `null` solo en filas existentes; ausencia preservada | RN | G3_PENDING | CANONICALIZE | filas cortas o largas | `SelectiveGeometryResolver.cs:185-205`; `SelectivePalletDesignDocument.cs:259-268` |
| S-08 | Cabecera custom: `LeftPost`/`RightPost`, placas | sin cambio (profundidad) | R | CODE_SUPPORTED | ALLOW | — | `LateralHeaderLayoutBuilder.cs:49-59` |
| S-09 | `DiagonalDirection` y `AutoAlternating` | sin cambio: la diagonal esta en el plano que RUN deja fijo | R | G3_PENDING | ALLOW | — | `BracingPanel.cs:40-48`; `PlantaHeaderLayoutBuilder.cs:71-93` |
| S-10 | `MountingFace` de horizontales y paneles | Front ↔ Back si la semantica es «normal al run» | R si CT-12 confirma; UK si no | G3_PENDING | ALLOW si CT-12 confirma; FAIL_CLOSED si no | CT-12 | `BracingPanelMemberBuilder.cs:159-165`, `:303-322` |
| S-11 | Bota `Side`, `Bota.Placement` | sin cambio | R | G3_PENDING | ALLOW | — | `BootPlacement.cs:41-65` |
| S-11b | Bota `PostSides[].PostIndex`, `Bota.Posts[].PostIndex` | `p → M−p`; valores intactos | R | G3_PENDING | ALLOW | — | `SelectivePalletDesign.cs:248-303` |
| S-12 | Protector lateral `PostSides` explicito distinto del patron por defecto | no demostrada | UK | CODE_SUPPORTED | FAIL_CLOSED | explicito ≠ {poste 0 Left, poste M Right} | `SelectiveSafetyEnds.cs:100-116`; `SelectiveSafetyPlacement.cs:221-248` |
| S-12b | Protector lateral con el patron por defecto | sin cambio | R | G3_PENDING | ALLOW | — | `SelectiveSafetyWindow.cs:788-791` |
| S-13 | Familia tope: `Side`, `TopeFondo`, `TopeShared`, `TopeSaque`, `TopeFrontal` | **sin regla**: holgura de 0.25" anclada al troquel izquierdo (AR-01) | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2) | el diseño efectivo genera al menos un tope en frontal (cualquier fondo) o planta; predicado exacto en CT-15 | `SelectiveFrontalBuilder.cs:220-222`; `SelectiveTopePlan.cs:140`, `:148`, `:262`, `:270`; `SelectivePlantaBuilder.cs:499`; `SelectiveSafetyPlacement.cs:55` |
| S-13b | Tope `TopeOffCells[].Frente` | sin regla mientras S-13 sea UK | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2) | igual que S-13 | `SelectiveTopePlan.cs:101-127` |
| S-14 | Desviador `Side`, `DesviadorLongitud`, `DesviadorPrimerNivelAltura` | sin cambio | R | G3_PENDING | ALLOW | — | `SelectiveDesviadorPlan.cs:116-128` |
| S-14b | `DesviadorOffCells` con espacio de indices estable | `c → P−1−c` | R | G3_PENDING | ALLOW | ningun claro dividido con `Dep(i)` | `SelectiveDesviadorPlan.cs:114-148` |
| S-14c | `DesviadorOffCells` con espacio de indices dependiente de una propiedad vinculada | remapeo inestable ante `ChangeValue` | UK | CODE_SUPPORTED | FAIL_CLOSED | off-cells ≠ ∅ ∧ algun claro dividido con `Dep(i)` | `SelectiveDesviadorPlan.cs:138-140`; `SelectiveMedioFrente.cs:70` |
| S-15 | Parrilla `ParrillaFrontal`, `ParrillaLateral`, `ParrillaFrente`, `ParrillaCantidad`, sin desborde | reparto simetrico en el tramo | R | G3_PENDING | ALLOW | toda fila con `count·frente ≤ span + GeometryTolerance.Length` | `SelectiveParrillaPlacement.cs:40-54`; `SelectiveFrontalBuilder.cs:262-311` |
| S-15x | Parrilla con una fila desbordada | apilado desde la izquierda | UK | CODE_SUPPORTED | FAIL_CLOSED | alguna fila con `count·frente > span + GeometryTolerance.Length` | `SelectiveParrillaPlacement.cs:49`; `SelectiveFrontalBuilder.cs:310` |
| S-15b | Parrilla `ParrillaOffCells[].Frente` | `f → M−1−f` | R | G3_PENDING | ALLOW | — | `SelectiveFrontalBuilder.cs:249-255` |
| S-16 | Numeracion y nombres de bloque por indice | se regeneran (PDC-5) | R | CODE_SUPPORTED | ALLOW | — | `SelectiveFrontalBuilder.cs:320-340` |
| S-17 | Toggles: `Dimensions`, `DimensionStyle`, `AnnotationScale`, `DrawRackName`, `NumberFronts`, `NumberLevels`, `DrawBasePlate` y la **bandera** `DrawPallets` | sin cambio; anotaciones regeneradas con el nombre logico destino | R | CODE_SUPPORTED | ALLOW | — | `SelectiveDimensions.cs:76-263`; `SelectiveFrontalBuilder.cs:342` |
| S-17b | **Tarimas dibujadas** (`DrawPallets` activo), filas sin desborde | reparto simetrico en el claro o tramo | R | G3_PENDING | ALLOW | toda fila de claro completo con `count·frente ≤ BeamLength + GeometryTolerance.Length` (los tramos usan `PalletFit` y no desbordan) | `SelectiveTarimaPlacement.cs:43-51`; `SelectiveFrontalBuilder.cs:389-440` |
| S-17c | **Tarimas**: fila de claro completo desbordada | apilado desde la izquierda | UK | CODE_SUPPORTED | FAIL_CLOSED | alguna fila de claro completo con `count·frente > BeamLength + GeometryTolerance.Length` | `SelectiveTarimaPlacement.cs:47`; `SelectiveFrontalBuilder.cs:389-404` |
| S-18 | `PropertyValues` y literales congelados | copia exacta (portador) | R | CODE_SUPPORTED | ALLOW | registro acreditable (§10.2) y vinculos resolubles | `SelectiveLinkedProperties.cs:188-213` |
| S-19 | `View` y `Section` del sobre | §3.7 | R | CODE_SUPPORTED | CANONICALIZE (legado inequivoco) o FAIL_CLOSED (resto) | `View` vacia con `Section ≥ 0`, `< −1`, `≥ fondos`, `View` no reconocida ⇒ FAIL_CLOSED | `RackSelectivoCommands.cs:126`, `:155`, `:168-177`, `:216` |
| S-20 | Vista lateral (`Section` = poste) | no expone RUN | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` | `SelectiveLateralBuilder.cs:173-198` |
| S-21 | Campos de otros sistemas en el DTO (`BotaB*`, `DefensaPosts`, `GuiaEntradaOffCells`) | round-trip intacto | R | CODE_SUPPORTED | ALLOW | — | `SelectivePalletDesignDocument.cs:539-578` |
| S-22 | `AuthoredSide` (persistido por el DTO) | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `SelectivePalletDesignDocument.cs:590` |
| S-22b | `DerivedAisles` (derivado, no autoria) | no se refleja: el resolvedor lo regenera | R | CODE_SUPPORTED | ALLOW (clasificacion `DERIVED`) | — | `SelectivePalletDesignDocument.cs:598`; `SelectivePalletDesign.cs:399-411` |
| S-23 | `DimensionViews` (I-50 integrada) | sin cambio; `int` exacto | R | CODE_SUPPORTED | ALLOW | — | `SelectivePalletDesignDocument.cs:108-116` |
| S-24 | Miembros desconocidos no vacios en el payload semantico | sin transformador | UK | CODE_SUPPORTED | FAIL_CLOSED | V-META (§6.4) | §6.4 |
| S-25 | Normalizaciones propias del store (`SchemaVersion` sticky; `DimensionViews` nulo no se escribe) | declaradas | R | CODE_SUPPORTED | BASELINE_LIMITED | V-RT-STORE | `SelectiveDesignSchema.cs:43-66`; `SelectivePalletDesignDocument.cs:108-116` |

### 7.2 Dinamico — `μ_RT`

| # | Propiedad / estado | Regla bajo μ_RT | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| D-01 | Salida en X=0 / entrada en `TotalLength` | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `DynamicLoadBeamGeometry.cs:166-190` |
| D-02 | `Fronts[]` y sus niveles | invertir la lista | R | G3_PENDING | ALLOW | — | `DynamicFrontGeometry.cs:129-162` |
| D-03 | `Modules[]` (orden, `Kind`, `Length`) | sin cambio | R | G3_PENDING | ALLOW | — | `DynamicRackSystem.cs:146-158`; `DynamicSystemPlantaBuilder.cs:135` |
| D-03b | `Modules[].Header`: `MountingFace` | Front ↔ Back si CT-12 confirma | R si CT-12 confirma; UK si no | G3_PENDING | ALLOW si CT-12 confirma; FAIL_CLOSED si no | CT-12 | `BracingPanelMemberBuilder.cs:159-165` |
| D-03c | `Modules[].Header`: `LeftPost`/`RightPost`, diagonales | sin cambio | R | G3_PENDING | ALLOW | — | `LateralHeaderLayoutBuilder.cs:49-59`; `DynamicSystemPlantaBuilder.cs:237`, `:260` |
| D-04 | `HeaderLineOverrides[]` (`PostIndex`, `ModuleId`, `Header`) | `p → N−p`; `ModuleId` intacto | R | G3_PENDING | ALLOW | — | `DynamicFrontGeometry.cs:374-395` |
| D-05 | `DerivedPostLineOverrides[]` (`PostIndex`, `Height`) | `p → N−p` | R | G3_PENDING | ALLOW | — | `DynamicFrontGeometry.cs:402-424` |
| D-06 | Escalares (alturas, peraltes, pallets, tolerancias, separadores, `DerivedPost*`) | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `DynamicRackSystemDocument.cs:16-45` |
| D-07 | Bota `PostSides`, `BotaPosts` | `p → N−p`; valores intactos | R | G3_PENDING | ALLOW | — | `BootPlacement.cs:44-53` |
| D-08a | Protector lateral con `Side ≠ None` o `PostSides` explicitos | no demostrada | UK | CODE_SUPPORTED | FAIL_CLOSED | explicito presente | `DynamicSafetyMultiViewBuilder.cs:132-140`; `DynamicSafetyDefaults.cs:57-176` |
| D-08b | Protector lateral con la regla adaptativa | sin cambio | R | G3_PENDING | ALLOW | — | idem |
| D-09 | Desviador `Side` | sin cambio | R | G3_PENDING | ALLOW | — | `DynamicSafetyLateralBuilder.cs:308-317` |
| D-10a | `DesviadorOffCells` con `off(poste 0) = off(poste 1)` por nivel | reclave de `q = N−p` | R | G3_PENDING | ALLOW | por nivel | `SelectiveDesviadorPlan.cs:28-31` |
| D-10b | `DesviadorOffCells` con `off(poste 0) ≠ off(poste 1)` | la clave `Math.Min` colapsa | MC | CODE_SUPPORTED | FAIL_CLOSED | por nivel | idem |
| D-11 | Defensa `DefensaPosts[]` | `p → N−p`; extremos intactos | R | G3_PENDING | ALLOW | — | `DynamicForkliftDefensePlan.cs:50-79` |
| D-12 | Guia de entrada `GuiaEntradaOffCells[]` | `f → N−1−f` | R | G3_PENDING | ALLOW | — | `DynamicEntranceGuidePlan.cs:27-64` |
| D-13 | Numeracion de frentes | se regenera | R | CODE_SUPPORTED | ALLOW | — | `DynamicViewDecorations.cs:66-77` |
| D-14 | `View` y `Section` | §3.7 | R | CODE_SUPPORTED | CANONICALIZE (planta) o FAIL_CLOSED (frontal fuera de `{0,1}`, `View` no reconocida) | — | `RackDinamicoCommands.cs:211`, `:220-224`, `:392-393` |
| D-15 | Vista lateral (poste), vista primaria obligatoria | no expone RT | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` o vacia | `RackDynamicSystemWindow.xaml.cs:2947-2955`; `RackDinamicoCommands.cs:235-239` |
| D-16 | Payload sin `DynamicDesign` utilizable | reflector no definido | UK | CODE_SUPPORTED | FAIL_CLOSED | `DynamicDesign == null` | `DynamicKindHandler.cs:38-44` |
| D-16b | Defectos que `ToDesign()` escribe explicitos | normalizacion declarada del store | R | CODE_SUPPORTED | BASELINE_LIMITED | V-RT-STORE | `SystemRegistry.Default.cs:74-88`; `DynamicRackSystemDocument.cs:196-211` |
| D-17 | `DimensionViews` (I-50 integrada) | sin cambio; `int` exacto | R | CODE_SUPPORTED | ALLOW | — | `DynamicRackSystemDocument.cs:63-72` |
| D-18 | Miembros desconocidos no vacios en el payload `DynamicSystem` | sin transformador (el store los descarta al leer) | UK | CODE_SUPPORTED | FAIL_CLOSED | V-META | §6.4 |

### 7.3 Push Back de un sentido — `μ_RT`

| # | Propiedad / estado | Regla bajo μ_RT | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| P-01 | Pasillo (extremo bajo) en X=0 | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `PushBackPlacements.cs:43-61` |
| P-02 | `Structure`: hereda D-02..D-07, D-09..D-11 | como el Dinamico | segun fila | segun fila | segun fila | — | `PushBackDesignDocument.cs:27` |
| P-03 | Configuracion por frente (`DefaultPalletsDeep`, `PalletsDeepOverrides`, `DrawPallets`, `HighEndBeamPeraltes`, `FirstLevelHeight`) | se reordena con los frentes | R | G3_PENDING | ALLOW | — | `PushBackCellDepth.cs:41-173` |
| P-04 | **Tope posterior activo**: `RearTopeSaque`, `RearTopePieceId` | **sin regla**: anclaje al TROQUEL_TOPE del poste izquierdo del frente, mano constante `ElevationMirrored = false` en FRONTAL y `LONGITUD = larguero + 0.25"` (patron de AR-01) | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2B) | tope posterior configurado con alguna celda que lo dibuja en frontal posterior o planta; predicado exacto en CT-15 | `PushBackRearTopeBuilder.cs:73-86`, `:172-209`; `PushBackSystemFrontalBuilder.cs:285-300`; `PushBackSystemPlantaBuilder.cs:134-154`; `DynamicEndBeamIdentity.cs:94-100`; `SelectiveTopePlacement.cs:41-59` |
| P-04b | Tope posterior configurado **sin** ninguna celda que lo dibuje | escalares sin efecto grafico; sin cambio | R | CODE_SUPPORTED | ALLOW | ninguna celda dibujable | `PushBackSystemFrontalBuilder.cs:277` |
| P-05 | `RearTopeOffCells[].Frente` con tope posterior activo | sin regla mientras P-04 sea UK | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2B) | igual que P-04 | `PushBackRearTopeBuilder.cs:268-307` |
| P-06 | `Side` colapsado, `AuthoredSide`, `PostSides` | `p → N−p` | R | G3_PENDING | ALLOW | protector lateral explicito ⇒ D-08a | `PushBackSafetyAuthority.cs:239-256` |
| P-07 | `DesviadorOffCells` por poste (biyectivo) | `p → N−p` | R | G3_PENDING | ALLOW | — | `PushBackSafetyAuthority.cs:271` |
| P-09 | `View` y `Section` | §3.7 | R | CODE_SUPPORTED | FAIL_CLOSED fuera de rango, `2..3` en rack simple, planta `≠ −1` o `View` no reconocida; ALLOW en el resto | — | `RackPushBackCommands.cs:218`, `:254-258`, `:427-450` |
| P-10 | Vista lateral | no expone RT | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` | — |
| P-11 | Miembros desconocidos no vacios en el payload `PushBack` (raiz y anidados) | sin transformador | UK | CODE_SUPPORTED | FAIL_CLOSED | V-META | §6.4 |

### 7.4 Push Back compuesto A/B — `μ_RT` (sin intercambio A/B)

| # | Propiedad / estado | Regla bajo μ_RT | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| PC-01 | Configuraciones de los lados A y B | **sin intercambio**; cada lado reordena sus frentes | R | G3_PENDING | ALLOW | — | ADR-0031 §5-bis |
| PC-02 | `Structure.Modules` (A → `GAP` → B invertidos con `B:`) | sin cambio | R | G3_PENDING | ALLOW | — | `PushBackCompositeStructure.cs:495-531` |
| PC-03 | `Topologies[]`: `Frente`; tipo y direccion; defaults | `f → N−1−f`; tipo y direccion intactos | R | G3_PENDING | ALLOW | — | `PushBackSide.cs:52-59` |
| PC-04 | `StructureOverrideA/B` | sin cambio | R | G3_PENDING | ALLOW | — | `PushBackSideConfiguration.cs:182-216` |
| PC-05a | `AbsentSlotsA/B` sin ranuras ausentes al inicio ni al final | `i → N−1−i` | R | G3_PENDING | ALLOW | por lado | `PushBackCompositeStructure.cs:284-321` |
| PC-05b | Ranuras ausentes al inicio o al final de un lado | las finales se retiran y las demas dibujan borde | MC | CODE_SUPPORTED | FAIL_CLOSED | por lado | idem |
| PC-06 | Bota por lado (`Bota*`, `BotaB*`, `BotaSidesDeclared`) | postes `p → N−p`; lado intacto | R | G3_PENDING | ALLOW | — | `PushBackBootPlan.cs:114-159` |
| PC-07 | Defensa por pasillo y piezas por lado | `p → N−p`; extremos intactos | R | G3_PENDING | ALLOW | — | `PushBackDefenseSides.cs:35-95` |
| PC-08 | **Tope posterior activo por lado** | sin regla (patron de P-04) | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2B) | igual que P-04, por lado | `PushBackSideDesign.cs:48`; `PushBackRearTopeBuilder.cs:73-86`, `:291-306` |
| PC-09 | `HeaderLineOverrides` con `ModuleId` `M…`/`GAP`/`B:…` | `p → N−p`; `ModuleId` intacto | R | G3_PENDING | ALLOW | — | `PushBackEditorDesignAssembler.cs:348-364` |
| PC-10 | `View` y `Section` frontal `0..3` | §3.7 | R | CODE_SUPPORTED | ALLOW en rango; FAIL_CLOSED fuera | — | `PushBackSystemFrontalBuilder.cs:137-155` |
| PC-11 | Letras «A»/«B» | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `PushBackSideAnnotations.cs:31`, `:104` |
| PC-12 | `Composite.Gap`, `CentralSeparator` | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `PushBackCompositeStructure.cs:123-133` |
| PC-13 | Vista lateral | no expone RT | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` | — |

### 7.5 Cantilever — `μ_X`

| # | Propiedad / estado | Regla bajo μ_X | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| K-01 | `StationTopology.FaceMode` | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `CantileverStationDesign.cs:14-21` |
| K-02 | `StationTopology.SingleSide` | sin cambio (±Y) | R | CODE_SUPPORTED | ALLOW | — | `CantileverArmFrameResolver.cs:59-75` |
| K-03 | `ArmCellOverrides[].Side` | sin cambio (±Y) | R | CODE_SUPPORTED | ALLOW | — | `CantileverLineDesign.cs:259` |
| K-04 | `ArmCellOverrides[].StationIndex` | `i → N−1−i` si `0 ≤ i < N`; fuera de rango se conserva; orden de lista intacto | R | G3_PENDING | ALLOW | — | `CantileverLineDesign.cs:255`, `:430-437` |
| K-05 | `StationCount`, `ColumnCentreSpacing` uniforme | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `CantileverLineDesign.cs:339-380` |
| K-06 | Lado de traslape del arriostramiento (derivado) | sin cambio | R | G3_PENDING | ALLOW | — | `CantileverLineResolver.cs:271-277` |
| K-07a | Arriostramiento `ColdRolledRound` | A↔B derivado | R | G3_PENDING | ALLOW | `BraceKind` | `CantileverIntervalResolver.cs:227-235`; `CantileverLineFrameResolver.cs:98-116` |
| K-07b | Arriostramiento con seccion estructural asimetrica | mano no demostrada | UK | CODE_SUPPORTED | FAIL_CLOSED | `BraceKind` + familia | idem |
| K-08 | Brazo `Single` con seccion asimetrica (canal C, angulo L) | la mano no es expresable | MC | CODE_SUPPORTED | FAIL_CLOSED | `Arrangement = Single` ∧ familia asimetrica | `CantileverArmFrameResolver.cs:88-123`; `CantileverArmResolver.cs:229-243` |
| K-08b | Brazo W o canal doble | sin cambio | R | G3_PENDING | ALLOW | — | `CantileverCataloguePolicies.cs:93-131` |
| K-09 | Ids de placa en BOM (patron medido desde `Outline[0]`) | depende del rack | R si V-BOM igual; UK si difiere | G3_PENDING | ALLOW si V-BOM igual; FAIL_CLOSED si difiere | V-BOM | `CantileverStationBomBuilder.cs:292-314` |
| K-10 | `View` y `Section` | §3.7 | R | CODE_SUPPORTED | ALLOW con `−1`; FAIL_CLOSED en el resto | — | `RackCantileverCommands.cs:501-540`; `CantileverViewPlanBuilder.cs:237-239` |
| K-11 | Separador siempre `Mirrored = true` (posible defecto existente) | no afectado por R_X | R | G3_PENDING | ALLOW | — | `CantileverLineFrameResolver.cs:61-85` |
| K-12 | Paneles, niveles, altura, visibilidad de planta, plantillas de base y columna (W) | sin cambio | R | G3_PENDING | ALLOW | — | `CantileverLineDesign.cs:66-309` |
| K-13 | Vista lateral (estacion) | no expone R_X | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` | `CantileverViewPlanBuilder.cs:213-215` |
| K-15 | Miembros desconocidos no vacios en `Cantilever` | sin transformador | UK | CODE_SUPPORTED | FAIL_CLOSED | V-META | §6.4 |
| K-16 | Rol visual de cada curva (capa de rol) | decidido en el plan; se compara en la firma | R | G3_PENDING | ALLOW | V-VIEW-ALL | `CantileverViewMaterializer.cs:169-214` |
| K-17 | Margenes retirados de troquel (`ColumnBottomPlateEndOffset`, `ColumnTopPunchOffset`) | el store los descarta por diseño (clasificacion `DERIVED`) | R | CODE_SUPPORTED | BASELINE_LIMITED | — | `RackProjectStore.cs:374-400` |

### 7.6 Cabecera independiente — `μ_D`

| # | Propiedad / estado | Regla bajo μ_D | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| H-01 | `LeftPost` ↔ `RightPost`; `LeftBasePlate` ↔ `RightBasePlate` | intercambio de objetos (el lado se re-deriva al cargar) | R | G3_PENDING | ALLOW | — | `RackFrameConfiguration.cs:57-60`; `RackFrameProjectDocument.cs:103-106`; `LateralHeaderLayoutBuilder.cs:49-59` |
| H-02a | `Panels[].DiagonalDirection` explicita | UpRight ↔ UpLeft | R | G3_PENDING | ALLOW | valor 1 o 2 | `BracingPanel.cs:40-48` |
| H-02b | `AutoAlternating` sin `StandardBaselineId` o en panel sin diagonal | N-H02 (Auto → explicita, declarado) | RN | G3_PENDING | CANONICALIZE | valor 0 | idem |
| H-02c | Valor numerico fuera de `{0, 1, 2}` | sin regla | UK | CODE_SUPPORTED | FAIL_CLOSED | valor desconocido | `DiagonalDirection.cs:5-7` |
| H-03 | `MountingFace` | sin cambio (R_D no toca las caras) | R | CODE_SUPPORTED | ALLOW | — | — |
| H-04a | Rasgos que el layout lee del lado izquierdo, iguales en ambos lados | intercambio exacto | R | G3_PENDING | ALLOW | igualdad de campos | `LateralHeaderLayoutBuilder.cs:61-67`; `LateralHeaderParametersFactory.cs:31-32`; `PlantaHeaderLayoutBuilder.cs:71-93` |
| H-04b | Esos rasgos distintos entre lados | el intercambio no es exacto | MC | CODE_SUPPORTED | FAIL_CLOSED | desigualdad de campos | idem |
| H-05 | Horizontales, elevaciones, alturas, troqueles, `Arrangement` | sin cambio | R | G3_PENDING | ALLOW | — | `LateralHeaderLayoutBuilder.cs:83-114`, `:262-294` |
| H-06 | `AutoAlternating` en panel con diagonal **y** `StandardBaselineId` no vacio | semantica de «estandar» tras N-H02 sin caracterizar | UK | CODE_SUPPORTED | FAIL_CLOSED hasta G5 | Auto ∧ baseline no vacio | `RackFrameConfigurationFactory.cs:85`, `:226-243`; `HardcodedStandardRackFrameService.cs:34` |
| H-07 | `View` y `Section` | §3.7 (no-planta = lateral; `σ' = −1`) | R | CODE_SUPPORTED | CANONICALIZE; FAIL_CLOSED con `View` no vacia no reconocida | — | `RackCabeceraCommands.cs:197`, `:239-240` |
| H-08 | Normalizaciones del store (version del payload `"1.0"`; legado sin envoltorio sale con envoltorio) | declaradas | R | CODE_SUPPORTED | BASELINE_LIMITED | V-RT-STORE | `RackFrameProjectDocument.cs:15-17`; `RackProjectStore.cs:56-57`, `:313-336` |
| H-09 | Miembros desconocidos no vacios en `Header` | sin transformador | UK | CODE_SUPPORTED | FAIL_CLOSED | V-META | §6.4 |

### 7.7 Cama

| # | Propiedad / estado | Clase | Evidencia | Disposicion | Evidencia en codigo |
|---|---|---|---|---|---|
| F-01 | Lado del tope / sentido del riel: ningun campo persistido | MC | CODE_SUPPORTED | FAIL_CLOSED siempre | `FlowBedLateralBuilder.cs:34-66` |
| F-02 | `BedType`, `LaneDepth`, `PalletDepth`, `RollerId`, `RollerPitchOverride` (el kind no tiene vista admisible) | MC | CODE_SUPPORTED | FAIL_CLOSED (por F-01) | `FlowBedConfiguration.cs:10-22` |

### 7.8 Transversal

| # | Propiedad / estado | Regla | Clase | Evidencia | Disposicion | Evidencia en codigo |
|---|---|---|---|---|---|---|
| X-01 | `Id`/`Name` del sobre e identidad interior | restamp de copia (§5.5) | R | CODE_SUPPORTED | ALLOW | `RackEnvelopeRestamp.cs:52-111` (Plugin) |
| X-02 | Portadores del sobre: `SchemaVersion`, `ExtensionData`, `CustomProperties` de I-54 | `Compose` los hereda | R | CODE_SUPPORTED | ALLOW | `RackEmbedComposer.cs:21-35`; D-21 de I-54 |
| X-03 | Portadores del envoltorio `RackProjectDocument` | `WithSourceMetadataFrom` | R | CODE_SUPPORTED | ALLOW | `RackProjectStore.cs:71-83` |
| X-04 | Colocacion de la referencia | `P' = G·P·F` | R | CODE_SUPPORTED | ALLOW | §3 |
| X-05 | Referencias enlazadas seleccionadas | una definicion nueva + N referencias | R | CODE_SUPPORTED | ALLOW | I-51 PD-2 |
| X-07 | Drive-In | no existe | — | CODE_SUPPORTED | no aplica | `RackMainMenuWindow.xaml:139-144` |
| X-08 | Larguero | sin bloque RackCad | — | CODE_SUPPORTED | no aplica | `KindHandlerRegistry.cs:51-53` |
| X-13 | Holgura u offset grafico asimetrico hallado por la auditoria de anclajes | ninguna regla implicita | UK | G3_PENDING | FAIL_CLOSED hasta regla aprobada (precedente AR-01) | CT-17 |
| X-14 | Parametro dinamico con semantica de lado sin regla | ninguna regla implicita | UK | CODE_SUPPORTED | FAIL_CLOSED | CT-25; §11.4 |
| X-15 | Vista admisible del rack (seleccionada o no) que no conmuta | V-VIEW-ALL | UK | G3_PENDING | FAIL_CLOSED | §3.8, §6.6 |

### 7.9 Precondiciones operativas (no son clases de representabilidad)

| # | Precondicion | Disposicion | Momento | Evidencia en codigo |
|---|---|---|---|---|
| PRE-01 | Fuente no canonica: MINSERT, `Normal`, rotacion, escala (ST-3, ST-5..ST-7, ST-9, ST-10) | FAIL_CLOSED (E4) | P1 | §4.1 |
| PRE-02 | Definicion con `Origin ≠ 0` (ST-14) | FAIL_CLOSED (E4) | P1 | `RackCloner.cs:34`; `LateralHeaderDrawer.cs:243` |
| PRE-03 | `View` no vacia no reconocida o `Section` no decodificable (ST-15) | FAIL_CLOSED (E4) | P1 | §3.7 |
| PRE-04 | Referencia dinamica, anonima o anotativa con payload RackCad (ST-17) | FAIL_CLOSED (E4) | P1 | `RackBlockFinder.cs:66` |
| PRE-05 | Divergencia authored entre vistas del grupo | FAIL_CLOSED (E3) | P2 | §5.4 |
| PRE-06 | Registro no acreditable con algun grupo cuyo kind lo consume, o vinculo roto | FAIL_CLOSED (E7) | P3 | §10.2 |
| PRE-07 | Push Back bloqueado para salida (`RackBomOutputGate.For(system).Reason ≠ null`) | FAIL_CLOSED (E6) | P4 | `PushBackKindHandler.cs:61-74` |
| PRE-08 | Linea Cantilever invalida o catalogo de secciones ilegible | FAIL_CLOSED (E6) | P4 | `CantileverKindHandler.cs:48-67` |
| PRE-09 | Bloques de biblioteca ausentes tras PREPARE; `MissingInstances > 0` en MUTATE | FAIL_CLOSED (E10/E11) | PREPARE/MUTATE | `BlockLibraryImporter.cs:74-118`; `LateralHeaderDrawer.cs:293-303` |
| PRE-10 | Huella o read-set cambiado | FAIL_CLOSED (E10/E11) | PREPARE/MUTATE | §9.2 |

---

## 8. Regeneracion

### 8.1 Application: autoridad pura de planes por vista

Una autoridad por kind, con contrato comun, **compartida** con el redibujo (no especifica del espejo):

```text
RackViewPlanAuthority (por kind; nombre no contractual)
  Build(request) → ViewPlanResult | fallo atribuible

request = { Design (payload interior en su sustrato), View canonica, σ' canonica, catalogos cargados,
            estado efectivo: lectura acreditada del registro si el kind lo consume (§10.2),
            RackName (nombre logico destino), sugerencia de nombre de bloque }

ViewPlanResult = { Familia: HeaderRun | CantileverCurves,
                   HeaderRunPlan | CantileverViewPlan,
                   nombre de bloque sugerido,
                   tramo del eje de la vista (para c) }
```

`RackName` es obligatorio: la anotacion «nombre de rack» la dibuja `system.Name` (`SelectiveFrontalBuilder.cs:342`;
`DynamicViewDecorations.cs:91`, `:156`, `:256`) y hoy la fijan los comandos (`RackSelectivoCommands.cs:121`, `:176`;
`RackDinamicoCommands.cs:177`; `RackPushBackCommands.cs:182`; `ProjectVariableMutationExecutor.cs:166-168`, `:236-238`).

La autoridad **reutiliza** los builders puros existentes; lo que hoy es pegamento del Plugin pasa a ella:

| Kind | Resolucion (Application, existente) | Plan por vista (Application, existente) |
|---|---|---|
| selective | `SelectivePalletDesignStore` → `SelectiveEffectiveDesignResolver.ResolveAccredited(authored, lectura)` → `SelectiveGeometryResolver.Resolve(efectivo, catalogo)` (mismo camino que `SelectiveKindHandler.cs:36-70`) | frontal: `SelectiveFrontalBuilder.BuildPlan(SelectiveDepthLayout.FondoSystemView(system, k), catalogo)`; planta: `SelectivePlantaBuilder.BuildPlan(system, catalogo)` |
| dynamic | `RackProjectStore` → `DynamicRackSystemResolver(catalogo).Resolve(DynamicDesign).System` (`DynamicKindHandler.cs:38-44`) | frontal: `DynamicSystemFrontalBuilder.BuildPlan(system, catalogo, end)`; planta: `DynamicSystemPlantaBuilder.BuildPlan` |
| pushback | `RackProjectStore` → `PushBackResolver(catalogo).Resolve(PushBackDesign)`; bloqueo `RackBomOutputGate.For` (`PushBackKindHandler.cs:40-74`) | frontal: `PushBackSystemFrontalBuilder.BuildPlan(system, catalogo, end, side)`; planta: `PushBackSystemPlantaBuilder.BuildPlan` |
| cantilever | `RackProjectStore` → `CantileverLineEditorAssembler(secciones).Build(CantileverLineDesign)` (`CantileverKindHandler.cs:48-67`) | `CantileverViewPlanBuilder.Build` |
| cabecera | `RackProjectStore` → `Header` | lateral: `HeaderInstanceGrouper.Group(LateralHeaderLayoutBuilder.Build(config, LateralHeaderParametersFactory.FromConfiguration(config), catalogo).Instances, nombre)` (`LateralHeaderDrawService.cs:60-90`); planta: `new HeaderRunPlan(vacio, PlantaHeaderLayoutBuilder.Build(config, catalogo))` (`PlantaHeaderDrawService.cs:29`) |

Restricciones: sin tipos de AutoCAD; sin `ObjectId`; sin I/O de disco dentro de `Build` (los catalogos llegan cargados);
un fallo de resolucion efectiva devuelve un fallo tipado, nunca un plan con literal congelado. Desde I-50, los builders
aplican `DimensionViewPolicy` a las cotas (`SelectiveDimensions.cs:76-263`; `DynamicViewDecorations.cs`), y la
autoridad pasa `DimensionViews` tal cual.

### 8.2 Plugin: materializador generico

```text
PREPARE (dentro del lock, FUERA de la transaccion de escritura)
  familia HeaderRun  → BlockLibraryImporter.EnsureForPlan(db, plan)                (efectos de infraestructura, §9.5)
                     → re-verificacion en lectura: toda instancia que no sea Annotation ni Dimension tiene BlockName
                       no vacio y presente en la BlockTable
                     → faltantes registrados (pieza, bloque, vista); faltantes ≠ ∅ ⇒ E10
  familia Cantilever → sin bloques de biblioteca (curvas); las capas de rol se crean dentro de MUTATE

CreateInTransaction(db, tx, plan, nombreBloque, payload) → definitionId           (transaccion del llamador)
  HeaderRun  → result = new LateralHeaderDrawer().CreateSystemBlock(db, tx, plan, nombre)     (LateralHeaderDrawer.cs:28-81)
               si result.Outcome.MissingInstances.Count > 0 ⇒ throw  ⇒ rollback total          (camino de I-52)
  Cantilever → CantileverViewMaterializer.CreateBlockDefinition(db, tx, plan, nombre, out real)  (:36-54)
  ambos      → RackBlockData.Write(tx, definitionId, payload)

CreateReference(db, tx, definitionId, colocacion) → referenceId                    (Model Space; transaccion del llamador)
  Position = (p'.X, p'.Y, Z fuente); Rotation = θ'; ScaleFactors = (s, s, s); LayerId = capa de la referencia fuente
```

- **Omision convertida en error.** `CreateSystemBlock` omite hoy las piezas sin bloque y las reporta como faltantes
  (`LateralHeaderDrawer.cs:293-303`; `LateralHeaderDrawOutcome.cs:31-33`; `BlockPlacement.cs:99-117`). Para RACKMIRROR
  una pieza faltante es **fallo duro**. El drawer **no** cambia; el control vive en el materializador de I-52 (G-M11).
- Despacho por **familia de plan**, no por kind: el materializador no conoce Left/Right, A/B, estaciones ni reglas de
  cabecera.
- No abre lock ni transaccion, no confirma, no regenera, no purga y no importa dentro de MUTATE (precedente
  `SystemBlockWriter.RedefineInTransaction`, `SystemBlockWriter.cs:82-116`).
- Las capas se crean dentro de la transaccion del llamador (`LateralHeaderDrawer.cs:270`, `:330-331`, `:356`;
  `CantileverViewMaterializer.cs:142`, `:169-195`).
- La politica de unicidad de nombres de bloque es la existente de cada familia; I-52 **no** unifica las dos politicas
  (decision de I-09).

### 8.3 Geometria neutral

- `Transform2D.ReflectionAboutLine(a, b)` en Application/Geometry, compuesta con las primitivas existentes (§3.3).
- Un valor de colocacion `(Position2D, RotationRadians, UniformScale)` con composicion a `Transform2D` y descomposicion
  validada (§3.3, §4.3).
- **Sin** sistema matematico paralelo y **sin** `Matrix3d` fuera del Plugin.

### 8.4 Autoridad de `View`/`Section`

La decodificacion `SectionRaw → semantica → canonica`, la exposicion y el conjunto `A_k` (§3.7, §3.8) viven en
Application, **junto al contrato de cada kind**. El comando no contiene `switch` de vistas. Las lecturas actuales del
Plugin (`RackSelectivoCommands.cs:126-177`; `RackDinamicoCommands.cs:209-239`, `:392-393`;
`RackPushBackCommands.cs:218-258`, `:427-450`; `RackCantileverCommands.cs:295-314`, `:501-540`;
`RackCabeceraCommands.cs:239-240`; `ProjectVariableMutationExecutor.cs:180-218`) **no** se tocan en el primer corte:
CT-04 fija su comportamiento y C-2 (§8.5) comprueba que la autoridad nueva no diverge.

### 8.5 Convergencia con los productores reales: **C-2 por defecto, GREEN en G6**

**Objetivo (V-EDIT).** Espejo → guardar/reabrir → `RACKEDITAR` → Actualizar → **mismo** dibujo. RACKMIRROR y los caminos
productivos no divergen al generar la misma vista.

**C-2.** Pruebas de equivalencia en **CI**, contra los **cuatro** productores reales, por fixture de (kind, vista
admitida) y por su copia espejo:

| # | Productor | Camino que se ejercita |
|---|---|---|
| C2-1 | Autoridad nueva de plan | `RackViewPlanAuthority.Build(designJson, View canonica, σ' canonica, catalogos, estado efectivo, RackName)` |
| C2-2 | `RACKEDITAR` → Actualizar (`EditX`) | payload → apertura del editor → sistema que dibuja el comando → servicio de dibujo → builder. Selectivo: `SelectiveEditorOpen.Resolve` → `LoadExisting` → `SystemToInsert` (`RackSelectivoCommands.cs:66-117`; `RackSelectiveWindow.xaml.cs:185-188`); Dinamico, Push Back y Cantilever: `LoadExisting` → `DesignToInsert`/`SystemToInsert` (`RackDinamicoCommands.cs:162-177`; equivalentes); cabecera: `RackFrameConfiguratorViewModel.Configuration` |
| C2-3 | `ProjectVariableMutationExecutor` (Selectivo) | `EffectiveOutput` → `SelectiveGeometryResolver` → `FondoSystemView`/planta → builder (`ProjectVariableMutationExecutor.cs:166-243`) |
| C2-4 | Estado del editor → sistema/documento | `SelectiveEditorState`, `DynamicEditorDesignAssembler`, `PushBackEditorDesignAssembler`, `CantileverLineEditorAssembler` en Core; lo que solo pasa por la ventana, en `tests/RackCad.UI.Tests` |

```text
firma(C2-1) = firma(C2-2) = firma(C2-3, solo Selectivo) = firma(C2-4)
firma de §11.2, con: nombre logico, estado efectivo (lectura del registro), View y Section canonicas, DimensionViews
```

- **Gate.** C2-1..C2-4 deben estar **GREEN antes de cerrar G6**, que es donde nace la autoridad. **G7 no consume la
  autoridad** si C-2 no esta verde. G8 conserva los cruces entre iniciativas, la continuidad de G-M9 y la reconciliacion
  final con las autoridades integradas.
- **Guarda G-M9.** Fija el mapa exacto de invocaciones de builders y servicios de dibujo de `EditX` y del ejecutor
  para las vistas admitidas (`SelectiveDepthLayout.FondoSystemView`, `SelectiveFrontalDrawService`,
  `SelectivePlantaDrawService`, `DynamicFrontalDrawService`, `DynamicPlantaDrawService`, `PushBackFrontalDrawService`,
  `PushBackPlantaDrawService`, `CantileverViewPlanBuilder.Build`, `LateralHeaderDrawService`,
  `PlantaHeaderDrawService` y las llamadas a builders dentro de esos servicios). Nace en G6 y sigue vigente en G8.
- **Validacion del Owner obligatoria:** M-4.
- **V-EDIT ≠ V-RT-STORE** (§6.3): un defecto historico de `EditX` no autoriza perdidas del espejo.

**C-1 (condicionada).** `RACKEDITAR` construye los planes de las vistas admitidas llamando a la autoridad. Solo se
activa con evidencia registrada y orden del Coordinador si:

1. hay divergencia demostrada entre productores;
2. el camino `EditX` de un kind no puede ejercitarse de forma fiable en Core ni en UI;
3. hay duplicacion sustantiva que dejaria dos autoridades del mismo plan.

### 8.6 Politica post-commit propia de I-52

- **Sin `Regen`** (DA-3), sin purga (crear definiciones no deja anidadas huerfanas); solo mensajes.
- Precedentes: RACKDUPLICAR crea definiciones y referencias sin `Regen` con PASS del Owner (I-51, INV-14); los caminos de
  insercion tampoco regeneran (`SystemBlockWriter.cs:18-40`; `ViewBlockDraw.cs:28-57`).
- Un **unico** `Regen` post-commit solo si el Owner demuestra representacion obsoleta, con evidencia registrada.
- INV-14 de I-51 **no cambia**.

---

## 9. Atomicidad

### 9.1 Orden normativo unico

```text
ACQUIRE    GetSelection sin SelectionFilter de tipo
SNAPSHOT   una transaccion de lectura: referencias y sus banderas de fuente (MINSERT, dinamica, anonima, anotativa),
           definiciones con Origin y payload, UNA lectura del registro de variables, huellas y read-set (§9.2)

PREFLIGHT  (sin mutacion; un solo orden; cualquier fallo ⇒ mensaje atribuible y FIN, cero mutacion)
  P1  canonicidad de fuente y referencia          ST-1..ST-17, incluida la decodificacion de View/Section (§4.1, §3.7)
  P2  agrupacion + autoridad authored              nucleo neutral; Kind y nombre del grupo; IsSameAuthority (§5)
  P3  precondiciones del registro y de la          acreditacion solo para grupos cuyo kind consume el registro;
      resolucion efectiva                          vinculos sanos; read-set (§10.2)
  P4  fidelidad del store y sustrato               lectura con la autoridad del kind; V-META (§6.4); linea base LB(D) (§6.3);
                                                   precondiciones de diseño (PRE-07, PRE-08)
  P5  Assess + Reflect candidato                   clase, evidencia y disposicion por fila; normalizaciones; V-DEP (§6.5)
  P6  cobertura de portadores y miembros           portadores identicos (§6.4); clasificacion de miembros consistente (§6.7)
  P7  V-VIEW-ALL                                   todas las (View, Section canonica) admisibles del rack logico (§3.8)
  P8  V-BOM                                        las dos comprobaciones (§11.5)
  P9  identidad y nombre                           NewRackId por grupo; «<base> - espejo»; validacion del asignador (§5.5)
  P10 Compose + ensayo de restamp + V-RT-STORE     Plugin, sin transaccion; V-RT-STORE sobre el payload re-estampado (§6.3)

LINE       dos puntos UCS→WCS validos (§4.2)
PREPARE    colocaciones P' · planes efectivos de las vistas SELECCIONADAS (§8.1) · payloads definitivos
           · EnsureForPlan (infraestructura, §9.5) · re-verificacion de bloques · re-verificacion de huellas y read-set
MUTATE     un DocumentLock · UNA transaccion de escritura:
           re-verificacion de huellas y read-set · todas las definiciones nuevas (MissingInstances ⇒ excepcion)
           · todos los payloads · todas las referencias
COMMIT     una vez
POST       solo mensajes (§8.6)
```

**Ajuste por dependencias.** La orden del Coordinador situaba el ensayo de identidad despues del restamp; el restamp
necesita `NewRackId` y el nombre, asi que P9 precede a P10. V-RT-STORE se completa en P10 porque el restamp
re-serializa por dominio en cabecera y Cantilever (`CabeceraKindHandler.cs:44-66`; `CantileverKindHandler.cs:80-110`).

### 9.2 Huella de la referencia y read-set del registro

Capturados en SNAPSHOT y comparados en PREPARE y al inicio de MUTATE:

| Campo | Motivo |
|---|---|
| `ObjectId`/handle estable | identidad de la fuente |
| existencia y no borrada | un comando transparente pudo borrarla |
| `BlockTableRecord`, `IsDynamicBlock`, `DynamicBlockTableRecord`, `IsAnonymous`, `Annotative` | la referencia pudo re-apuntarse o transformarse (ST-17) |
| `Position`, `Rotation`, `ScaleFactors`, `Normal` | `P` y el contrato de fuente (§4.1) |
| capa | la referencia nueva usa esa capa (ST-13) |
| `Origin` de la definicion | ST-14 |
| hash del payload RackCad de la definicion | el documento reflejado depende de el |
| **read-set del registro** (solo si algun grupo consume el registro) | resultado de acreditacion y version del registro, y para cada `VariableId` realmente leido por la resolucion efectiva: tipo y definicion. Cambios en variables **no leidas** no abortan |

Cualquier diferencia ⇒ **fail-closed antes de mutar** (E10 en PREPARE; en MUTATE, excepcion y rollback).

### 9.3 Tres garantias distintas

| Garantia | Estado | Contenido |
|---|---|---|
| **Atomicidad semantica del rack** | **garantizada** | Una sola transaccion MUTATE con todas las definiciones RackCad, payloads y `BlockReference`. Cualquier fallo ⇒ rollback de **todas** las copias. Nunca queda una copia parcial ni un payload RackCad nuevo sin su definicion |
| **Atomicidad de infraestructura** | **NO garantizada** | PREPARE puede importar definiciones de biblioteca, bloques anidados, capas, estilos y demas dependencias, y pueden **permanecer** aunque despues falle PREPARE o MUTATE (§9.5) |
| **UNDO** | **UNKNOWN** hasta la validacion del Owner en AutoCAD | No hay marcas de undo en el Plugin (busqueda `UndoMark`: cero); rige el grupo de undo por comando. **No** se promete que un UNDO revierta las importaciones (M-10, M-20) |

La garantia de I-52 es **cero mutacion semantica** en cualquier fallo, no «cero mutacion de la base de datos».

### 9.4 Tabla de fallos

| # | Fallo | Momento | Resultado |
|---|---|---|---|
| E1 | Ninguna fuente valida | P1 | fin, sin linea |
| E2 | Payload RackCad inutilizable, `Kind` sin reflector o sin handler, `Design` vacio | P1 | fin, cero mutacion |
| E3 | Grupo inconsistente (`Kind`, nombre, autoridad authored) | P2 | fin, cero mutacion |
| E4 | Fuente no canonica (ST-3, ST-5..ST-7, ST-9, ST-10, ST-14, ST-15, **ST-17**) | P1 | fin, cero mutacion; ST-17 con mensaje propio («bloque dinamico, anonimo o anotativo») |
| E5 | Vista seleccionada no admisible (lateral de rack, cama) | P1 | fin, cero mutacion; el mensaje sugiere reflejar las vistas admisibles e insertar las demas con `RACKEDITAR` desde la copia |
| E6 | Fila `REQUIRES_MODEL_CHANGE` o `UNKNOWN` material (topes Selectivo, tope posterior de Push Back, medio frente dependiente, desviador inestable, parrilla o tarimas desbordadas, Auto con baseline), V-META, V-DEP, **V-VIEW-ALL** (incluidas vistas admisibles no seleccionadas), V-BOM o V-RT-STORE fallidas; diseño bloqueado o invalido | P4..P10 | fin, cero mutacion; nombra rack, vista, propiedad, pieza o miembro |
| E7 | Registro **no acreditable** (presente e ilegible, version incompatible o identidad ambigua) **y** algun grupo cuyo kind lo consume (hoy: Selectivo, con o sin vinculos); o vinculo roto | P3 | fin, cero mutacion. Si ningun grupo lo consume, el registro no se evalua (§10.2) |
| E8 | Ensayo de `Compose` + restamp + nombre fallido | P9, P10 | fin, cero mutacion |
| E9 | Linea invalida o cancelada | LINE | fin, cero mutacion |
| E10 | Fallo de plan, bloque ausente tras importar, huella o read-set cambiado | PREPARE | fin; **ninguna** definicion, referencia ni payload RackCad nuevo; lo importado por `EnsureForPlan` puede permanecer (§9.5) |
| E11 | Excepcion en MUTATE, incluidas huella o read-set cambiado y `MissingInstances.Count > 0` | MUTATE | sin commit ⇒ rollback de **toda** la operacion semantica |

### 9.5 Contrato de infraestructura: `EnsureForPlan` / `EnsureBlocks`

Hechos del codigo (`BlockLibraryImporter.cs`, sin cambios en `main`):

| # | Hecho | Evidencia |
|---|---|---|
| IF-1 | Debe llamarse dentro del lock y **fuera** de la transaccion del dibujo | `:12-18` |
| IF-2 | Comprueba en una transaccion de lectura que bloques faltan | `:55-67` |
| IF-3 | Biblioteca ausente ⇒ devuelve 0 sin error | `:74-78` |
| IF-4 | Biblioteca ilegible ⇒ se registra y devuelve 0 (cache de sesion) | `:149-161` |
| IF-5 | Clona con `WblockCloneObjects(ids, BlockTableId, mapping, DuplicateRecordCloning.Ignore, deferTranslation: false)`, que arrastra dependencias (capas, estilos, bloques anidados) y conserva las existentes con el mismo nombre | `:109-110` |
| IF-6 | Captura **toda** excepcion, la registra y devuelve 0: importacion **parcial o nula** sin fallo visible | `:113-118` |
| IF-7 | `CreateSystemBlock` omite despues las piezas sin bloque | `LateralHeaderDrawer.cs:293-303` |

Contrato de I-52:

1. PREPARE llama a `EnsureForPlan` por plan de familia `HeaderRun`.
2. PREPARE **re-verifica** en lectura todas las dependencias requeridas por el plan y registra las faltantes.
3. Faltantes ⇒ E10: no se crea nada semantico. Lo ya importado **puede quedar** en el dibujo.
4. MUTATE vuelve a comprobar con `MissingInstances`; si hay faltantes, lanza y revierte (E11).
5. Ninguna afirmacion de atomicidad de infraestructura ni de UNDO de importaciones en mensajes, ADR ni documentacion.
6. Limitacion declarada: `DuplicateRecordCloning.Ignore` conserva un bloque del dibujo con el mismo nombre aunque su
   contenido difiera del de la biblioteca; afecta a toda vista de RackCad, no solo al espejo.

---

## 10. Persistencia, variables de proyecto y portadores

### 10.1 Composicion del payload (por definicion nueva)

```text
1. sobreFuente   = RackEmbedStore.Deserialize(payloadFuente)
2. vista         = §3.7 (View canonica, σ' canonica) — o fallo
3. reflejado     = Reflector(kind).Reflect({ payload interior = sobreFuente.Design, ... })  en el sustrato de §6.2
4. compuesto     = RackEmbedComposer.Compose(sobreFuente, sobreFuente.Kind, sobreFuente.Id, sobreFuente.Name,
                                             ViewCanonica, σ', reflejado.Design)
5. payload       = RackEnvelopeRestamp.RestampEnvelope(RackEmbedStore.Serialize(compuesto), "<base> - espejo", NewRackId)
                   (Plugin; restamp interior por KindHandlerRegistry)
6. postcondicion = V-RT-STORE sobre el payload final; portadores de §6.4 identicos
```

Todas las definiciones del grupo reciben **el mismo** payload interior reflejado salvo los metadatos propios de su
vista; en Selectivo se re-verifica `IsSameAuthority` sobre los documentos reflejados.

### 10.2 Variables de proyecto

| Regla | Contenido |
|---|---|
| PV-1 | Los vinculos authored se preservan **exactos**: mismo `PropertyId`, `VariableId`, `Kind` y literal congelado |
| PV-2 | El espejo **no** crea variables, **no** desvincula ni revincula, **no** fija literales, **no** materializa, **no** escribe el registro y **no** escribe en el documento ningun valor calculado a partir del registro (obligacion 9) |
| PV-3 | El espejo **no** evalua expresiones con logica propia |
| PV-4 | Para generar geometria consume **en solo lectura** la autoridad efectiva existente del kind sobre la lectura unica del registro hecha en SNAPSHOT (`ProjectVariablesRegistry.Read`, `ProjectVariablesRegistry.cs:22`) |
| PV-5 | **Criterio de acreditacion = el de la autoridad efectiva existente, por kind** (CR-V2-1). Selectivo: `SelectiveEffectiveDesignResolver.ResolveAccredited` exige un registro acreditable (ausente o legible) **aunque el rack no tenga vinculos** (`SelectiveEffectiveDesignResolver.cs:52-66`; `UsableProjectVariablesRegistry.cs:120-135`); asi abren el editor (`SelectiveEditorOpen.cs:184`), reconcilia `RACKEDITAR` (`LinkedPropertyReconciler.cs:131-139`) y cotiza `RACKBOMTOTAL` (`RackInventarioCommands.BomTotal.cs:56-67`). Dinamico, Push Back, Cantilever y cabecera no tienen vinculos e **ignoran** el registro (`DynamicKindHandler.cs:23-25`; `PushBackKindHandler.cs:26-28`; `CantileverKindHandler.cs:33-35`; `CabeceraKindHandler.cs:21-23`). I-52 no cambia esa semantica |
| PV-6 | Si I-49 integra antes: RACKMIRROR invoca la autoridad efectiva integrada, preserva `expression` como portador y **no** reimplementa su evaluacion (P24.8 de I-49 V6) |
| PV-7 | La copia es un **consumidor nuevo** del mismo `VariableId` (ADR-0034 §12): bloquea `Delete` y se redibuja en `ChangeValue` desde su authored reflejado. **`ChangeValue` no puede deshacer la equivalencia reflejada**: la exposicion de §3.2 vale para todo estado acreditable `R`, y un rack cuya equivalencia dependa de un valor vinculado no es admisible (S-02b, S-14c) |

### 10.3 `DimensionViews` (I-50 integrada)

`DimensionViews` es **requisito normativo** del primer corte. Cada reflector es un **sitio de copia**: transporta el
`int` exacto, incluidos bits desconocidos y `null`, por DTO en Selectivo (`SelectivePalletDesignDocument.cs:108-116`) y
por dominio en Dinamico (`DynamicRackSystemDocument.cs:63-72`) y Push Back. **T-M18 es incondicional**: centinelas `-8`,
`13` y `null` a traves del espejo (mismo metodo que `DimensionViewsCopySitesTests`). Las firmas de anotaciones de C-2 y
V-VIEW-ALL se evaluan con la politica de cotas de I-50.

### 10.4 Metadata, I-54 y deuda de cabecera

- La fidelidad del store por kind es la de §6.2 y se mide con V-RT-STORE (§6.3); la metadata semantica desconocida la
  bloquea V-META (§6.4).
- **Cabecera**: el payload no tiene `ExtensionData` y su version vuelve a `"1.0"`. Es deuda conocida, no se arregla en
  I-52; V-META bloquea un payload de cabecera con miembros desconocidos.
- **I-54 (Proposal V3, `5d25da8`)**: las propiedades de alcance Rack viven en el miembro `CustomProperties` del sobre;
  `Compose` lo hereda y no hay promocion de major del sobre. Antes de integrarse, un build sin I-54 lo conserva en
  `ExtensionData` del sobre. En ambos casos es **portador** (§6.4) y el espejo lo conserva. D-21: el espejo cumple el
  invariante. Si I-54 integra antes del Candidato: T-M20.

---

## 11. Equivalencia ejecutable bajo reflexion

### 11.1 Definicion

Para una vista admisible `(V, σ)`:

```text
A = firma(Π_(V,σ')(Eff(μ_k D, R)))          plan reflejado semanticamente
B = firma(F ∘ Π_(V,σ)(Eff(D, R)))           plan original transformado por F
A ≡ B  ⇔  multiconjuntos iguales de piezas fisicas bajo las reglas de §11.2..§11.5
```

El espejo es fiel al **plan regenerado** del documento fuente, **no** a las entidades actuales de su definicion, que
pueden estar desactualizadas o incompletas por bloques que faltaban al dibujarla. Es la misma semantica de `RACKEDITAR`
→ Actualizar.

### 11.2 Firma

| Componente | Regla |
|---|---|
| Piezas | Roles fisicos, incluido `Pallet`; `Annotation` y `Dimension` se validan aparte (§11.6) |
| Aplanado | Cada instancia de un `HeaderGroup` se lleva a coordenadas de la definicion componiendo la colocacion con su transformacion completa (rotacion incluida). **No** se usa `HeaderRunPlan.Flatten`/`PlacedClone` (`HeaderRunPlan.cs:53-80`) |
| Identidad de pieza | `PieceId`, `BlockName`, `View` |
| Posicion | `Insertion` y `ConnectionAnchor` con `GeometryTolerance.Length`; `GeometryTolerance.Continuity` solo donde una normalizacion declarada escribe un valor calculado (N-S02) |
| Orientacion | rotacion modulo 2π con `GeometryTolerance.Angle` |
| Escala y mano | `MirroredX`, `MirroredY` y signo del determinante total de la pieza |
| Parametros dinamicos | §11.4 |
| Curvas Cantilever | contornos con la regla de inversion de `Transform2D` (`PathSegment2D.cs:139-160`), comparados como conjuntos de segmentos (independientes del sentido de trazado) |
| Rol de capa Cantilever | `CantileverVisualRole` de cada curva, decidido en el plan (`CantileverViewMaterializer.cs:207-214`) |
| Seccion | `(V, σ')` frente a `(V, σ)` canonicas (§3.7) |
| BOM | §11.5 |
| Anotaciones | contratos semanticos (§11.6) |

Ninguna tolerancia oculta una holgura unilateral sistematica: `1e-9` in frente a `0.25"`.

### 11.3 Mano y declaracion tipada de simetrias

1. **Ningun bloque DWG se asume simetrico.** La biblioteca no esta versionada ni tiene metadato de simetria.
2. Una diferencia de mano entre A y B en una pieza es **no equivalente**, salvo que el bloque figure en la
   **declaracion tipada unica** de simetrias.
3. **Donde vive.** Un unico registro tipado en Application, en la carpeta del espejo (nombre no contractual
   `BlockSymmetryDeclaration`). **Fuera** de `assets/` y de los catalogos. Su unico consumidor es el comparador de
   equivalencia. Ningun literal de simetria en ningun otro archivo (G-M10).
4. **Contrato conceptual** de cada entrada:

   | Campo | Contenido |
   |---|---|
   | `BlockName` | nombre exacto del bloque, presente en `assets/catalogs/blocks.csv` |
   | `ViewOrFamily` | la `View` del plan (`HeaderRun`) o la familia de curvas |
   | `LocalSymmetryAxis` | `X` o `Y` del bloque |
   | `LocalSymmetryCenter` | coordenada en el **marco local de insercion de la pieza**, **despues** de aplicar `T(−BlockOrigin)` (ver 5). Constante por defecto. Forma afin `c = a0 + a1·Parametro` **solo** si CT-06 demuestra que el centro depende de un parametro dinamico, con justificacion y prueba |
   | `Evidence` | fila de CT-06 que muestra la diferencia de mano y el eje y centro |
   | `OwnerEvidence` | registro fechado en `docs/automation/evidence/` con la vista en AutoCAD de la pieza y su espejo |
   | `EvidenceLibrary` | biblioteca y catalogo usados: ruta de `blocks-library.dwg` (`BlockLibraryLocator.ResolvePath`), firma del archivo (ultima escritura y tamaño) y `CatalogBlockManifest.Fingerprint` (`CatalogBlockManifest.cs:57`, `:127-150`) |
   | `Test` | prueba de equivalencia que usa la entrada |

5. **Marco del centro.** El drawer crea cada pieza como `BlockReference(Position = q, Rotation = r,
   ScaleFactors = (mX, mY, 1))` (`LateralHeaderDrawer.cs:305-314`), cuya transformacion es
   `T(q)·R(r)·S(mX,mY)·T(−o_p)`, con `o_p` = `BlockTableRecord.Origin` del bloque de pieza. El centro se mide en
   `u = x_BTR − o_p`. **No** en WCS, **no** en el marco del rack y **no** en coordenadas crudas del BTR cuando su `Origin`
   desplaza el marco efectivo.
6. **Equivalencia.** Para una entrada con eje X y centro `c` (en `u`), la instancia `(q, r, mX, mY, parametros)` equivale
   a

   ```text
   (q + R(r)·(2·c·mX, 0), r, −mX, mY, parametros)          eje X
   (q + R(r)·(0, 2·c·mY), r, mX, −mY, parametros)          eje Y
   ```

   que sale de `T(q)·R(r)·S(mX,mY)·X_c = T(q + R(r)·(2c·mX, 0))·R(r)·S(−mX, mY)`, con `X_c = T(2c,0)·S(−1,1)`. Con forma
   afin, `c` se evalua con el valor del parametro de **esa** instancia.
7. **Prueba negativa y exclusion.** Una declaracion solo re-expresa la mano de una pieza sobre su propia ocupacion; no
   puede trasladarla. Por eso:
   - **T-M40** exige que, con toda declaracion admitida por el registro, un fixture con holgura unilateral tipo AR-01 (y
     el del tope posterior de Push Back) siga **no equivalente**, e incluye un intento con un centro «conveniente»
     (por ejemplo, desplazado media holgura) que debe fallar la validacion del registro o la equivalencia;
   - el registro **rechaza** declaraciones para piezas de familias con fila `UNKNOWN` por holgura unilateral (hoy, rol
     `Tope`), y para bloques que el manifiesto de I-19 declara con parametro de estiramiento en ese eje si el centro no
     esta respaldado por la evidencia de CT-06.
8. **Censo.** G3 produce el censo de piezas con diferencia de mano por kind y vista sobre fixtures asimetricos (CT-06),
   con eje y centro candidatos. El Owner confirma con evidencia (O-3) en G3 y G9.
9. **Sin declaracion:** la vista no es equivalente ⇒ el rack falla cerrado (E6).

### 11.4 Parametros dinamicos

- El censo de claves y bloques **reutiliza las autoridades de I-19**: `CatalogBlockParameters`
  (`CatalogBlockParameters.cs:28`) y `CatalogBlockManifest` (`CatalogBlockManifest.cs`), cuya guarda builder → manifiesto
  ya impide que diverjan. **No** se crea un censo paralelo (CT-25).
- Hoy las claves son magnitudes: `LONGITUD`, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE` y `FONDO`
  (`SelectiveRackDefaults.cs:46-95`), mas los nombres configurables de cabecera, que por defecto son esas magnitudes
  (`LateralHeaderParameters.cs:66-72`).
- Una magnitud se compara numericamente con `GeometryTolerance.Length`. La **direccion** en que estira dentro del
  bloque es contenido del bloque y la cubre la declaracion de simetria (§11.3).
- Un parametro con semantica de lado o mano (visibilidad, volteo, enumerado de lado) exige una regla especifica
  declarada; sin ella ⇒ **FAIL_CLOSED** (X-14). Como los nombres de cabecera son configurables, el censo se evalua **por
  valor en PREFLIGHT**.

### 11.5 V-BOM: dos comprobaciones

| # | Comprobacion | Regla |
|---|---|---|
| V-BOM-1 | **Multiconjunto semantico de lineas del builder del kind** | `SelectiveBomBuilder`, `SystemBomBuilder`, `PushBackBomBuilder`, `CantileverLineEditorAssembler.Bom`, `BomBuilder`: igualdad del multiconjunto de lineas (categoria, pieza, longitud, peralte, cantidad) usando **la precision y el redondeo que ese builder ya aplica** (por ejemplo, `Round(...)` de `PushBackBomBuilder.cs:403`); el orden de lineas no cuenta |
| V-BOM-2 | **Clave consolidada visible al usuario** | la clave de `ConsolidatedBom` (`ConsolidatedBom.cs:62-64`, `:110-115`): componente = categoria + longitud redondeada a 4 decimales + firma de piezas (categoria, perfil, longitud con formato `0.##`, cantidad), sumando cantidades; un BOM no basado en componentes usa la firma de sus lineas (`:89-107`) |

Ambas deben pasar. **No** se confunden: V-BOM-1 no es mas estricta que su builder y V-BOM-2 no es mas estricta que la
consolidacion.

### 11.6 Textos y cotas

No se comparan geometricamente. Se validan con **contratos semanticos**: numeracion regenerada desde el documento
reflejado (PDC-5), letras A/B por posicion, nombre de rack = `<base> - espejo`, cotas segun `Dimensions`, estilo y la
politica de `DimensionViews` de I-50, y legibilidad (rotacion de texto no invertida). La validacion visual final es del
Owner (M-16).

### 11.7 Holguras y offsets graficos

- **Ninguna holgura, offset o separacion puramente grafica se asume simetrica.** AR-01 (topes Selectivo) y el tope
  posterior de Push Back son los precedentes demostrados.
- CT-17 audita en G3 los anclajes de los builders de las vistas admisibles y clasifica cada coordenada a lo largo del
  eje reflejado como **simetrica** (derivada de ambos extremos o del centro) o **anclada** (desde un poste, troquel o
  extremo, con offset de un lado).
- Cada anclaje asimetrico produce una fila `UNKNOWN` con `FAIL_CLOSED` o una regla explicita aprobada por el Owner.
  Nunca un parche silencioso: una contradiccion con V3 abre Proposal V4.

---

## 12. Plan de pruebas (no se implementa en este gate)

### 12.1 Caracterizacion primero (G3; pasan sobre la produccion intacta)

| # | Caracteriza | Suite |
|---|---|---|
| CT-01 | Fixtures asimetricos por kind y vista admitida con firma de plan (§11.2), con `DimensionViews` presentes | Core |
| CT-02 | BOM base de esos fixtures: multiconjunto de cada builder y clave de `ConsolidatedBom` (§11.5) | Core |
| CT-03 | Normalizaciones conocidas del store por kind (defectos de `ToDesign()`, version `"1.0"` de cabecera, legado sin envoltorio, margenes retirados) | Core |
| CT-04 | Decodificacion de `View`/`Section` de produccion por kind (tabla de §3.7), incluidos `−1` legado, secciones fuera de rango, `2..3` en Push Back simple y `View` vacia | Core + guarda de texto de las lecturas del Plugin |
| CT-05 | Tramos y centros `c` por (kind, vista) desde la geometria resuelta | Core |
| CT-06 | Censo de mano: piezas cuya mano difiere entre `F ∘ Π(D)` y `Π(μD)`, con eje y centro candidatos **en el marco local de insercion tras `T(−BlockOrigin)`**; indica si el centro depende de un parametro. Incluye la **caracterizacion negativa del marco**: un fixture AR-01 (y el del tope posterior de Push Back) no se vuelve equivalente con ningun centro candidato | Core |
| CT-07 | `WithDesign` materializa `PalletTolerance` vinculada (hueco actual del camino `EditX`) | Core |
| CT-08 | `SelectiveDesviadorPlan.CellKey` colapsa N−1/N en el Dinamico | Core |
| CT-09 | `BracingPanel.ResolveDiagonalDirection` por paridad | Core |
| CT-10 | Ausencias de Push Back compuesto: inicio, interior y final dibujan distinto | Core |
| CT-11 | Marco del brazo Cantilever por lado y familia de seccion | Core |
| CT-12 | Semantica de `MountingFace` respecto del run (S-10, D-03b) | Core |
| CT-13 | `SelectiveAuthoredAuthority` distingue un documento reflejado de su origen | Core |
| CT-14 | Censo de `[CommandMethod(` = 33 y unicidad de nombres (linea base) | Core |
| CT-15 | **Topes**: holgura del tope Selectivo en frontal y planta (AR-01) y del **tope posterior de Push Back** en frontal posterior y planta (anclaje, `ElevationMirrored`, `LONGITUD`); predicado exacto de «tope activo» | Core |
| CT-16 | **Semantica observable de `RackDuplicationPlan`**: textos completos, orden de grupos, orden de referencias, avisos y errores, sobre codigo intacto (§5.1) | Core |
| CT-17 | **Auditoria de anclajes** de `SelectiveFrontalBuilder`, `SelectivePlantaBuilder`, `SelectiveTopePlan`, `SelectiveParrillaPlacement`, `SelectiveTarimaPlacement`, `SelectiveDesviadorPlan`, `SelectiveSafetyPlacement`, `DynamicSystemFrontalBuilder`, `DynamicSystemPlantaBuilder`, `DynamicViewDecorations`, `DynamicSafetyMultiViewBuilder`, `DynamicForkliftDefensePlan`, `DynamicEntranceGuidePlan`, `PushBackSystemFrontalBuilder`, `PushBackSystemPlantaBuilder`, `PushBackRearTopeBuilder`, `PushBackCompositeFrontal`, `PushBackBootPlan`, `PushBackDefensePlan`, `CantileverViewPlanBuilder`, `LateralHeaderLayoutBuilder` y `PlantaHeaderLayoutBuilder`: cada coordenada a lo largo del eje reflejado, simetrica o anclada | Core |
| CT-18 | **Origin**: los materializadores crean `Origin = 0`, `RackCloner` copia el de la fuente y ningun comando lo comprueba hoy | guarda de texto (sin AutoCAD en CI, ADR-0003) |
| CT-19 | **Efectos de `EnsureForPlan`**: IF-1..IF-7 de §9.5 | guarda de texto; los efectos reales son evidencia del Owner en G9 |
| CT-20 | **Filas de parrilla y de tarimas**: condicion `count·frente > span` en claros completos, reparto apilado, y ausencia de desborde en tramos (`PalletFit`) | Core |
| CT-21 | N-S02: UI, BOM, round-trip e involucion respecto de la forma normal | Core + UI si hace falta |
| CT-22 | N-S06: ceros finales, listas cortas, valores `≤ 0` distintos de `0.0`, listas largas e involucion | Core |
| CT-23 | N-H02: fisica, BOM, UI, round-trip y relacion con `IsStandard`/`IsException`/`StandardBaselineId` | Core + UI si hace falta |
| CT-24 | Criterio de acreditacion del registro por kind (PV-5) | Core |
| CT-25 | Censo de claves de parametros por kind y vista **con `CatalogBlockParameters` y `CatalogBlockManifest` (I-19)** | Core |
| CT-26 | Mapa de invocaciones de builders de `EditX` y del ejecutor en las vistas admitidas (linea base de G-M9) y caminos del estado del editor (C2-4) | Core + `tests/RackCad.UI.Tests` |
| **CT-27** | **N-S02 con dos estados del registro**: `PalletTolerance` vinculada con dos valores acreditables; con `Dep(i)` la copia deja de ser espejo tras el cambio; con todas las celdas gobernantes con `BeamLengthOverride` conmuta en ambos | Core |
| **CT-28** | **Dependencia efectiva de `BeamLength`**: regla del ancho compartido (fondo maestro, columna vacia, fondos con niveles) y predicado `Dep(i)` | Core |
| **CT-29** | **S-14 asociado**: espacio de indices del desviador bajo dos estados del registro cuando el ajuste del medio frente cambia | Core |
| **CT-30** | **Enumeracion `A_k`** de todas las vistas admisibles por kind y fixture, y conmutacion en cada una | Core |
| **CT-31** | **Linea base de la guarda de cobertura**: miembros publicos de los tipos cubiertos y descriptores vinculables, con naturaleza (persistido, derivado, portador) | Core |
| **CT-32** | **Metadata semantica desconocida**: deteccion por kind (raiz y anidados), exencion de portadores (sobre, envoltorio, `PropertyValues`, `CustomProperties`) y de miembros retirados | Core |
| **CT-33** | **Fuentes dinamicas, anonimas o anotativas**: clasificacion ST-17 frente a ST-2 | guarda de texto; efecto real en M-23 |
| **CT-34** | **Historia de `View` vacia del Selectivo**: `View` introducida en `6023572` antes que `Section` en `4d2ca82`; decodificacion de produccion | Core + registro documental |
| **CT-35** | **V-RT-STORE por kind, separado de C-2**: `Miembros(LB)` e idempotencia del store sobre fixtures con y sin miembros opcionales | Core |

### 12.2 Pruebas nuevas (RED antes de implementar)

| # | Afirma | Gate |
|---|---|---|
| T-M01 | `ReflectionAboutLine`: involucion, determinante −1, puntos de la linea fijos | G4 |
| T-M02 | Colocacion: `P' = G·P·F` con determinante positivo; rotacion `2φ−θ+π` con `X_c` y `2φ−θ` con `Y_c`; escala conservada; descomposicion validada; ejemplos E-1..E-3 | G4 |
| T-M03 | Transformacion fuente ST-1..ST-15 (clasificacion, canonizacion `(−s,−s)`, `Origin`, `View`/`Section`) | G4 (parte pura) y G7 (captura) |
| T-M04 | Nucleo neutral y fachada `RackDuplicationPlan` con T1–T15 intactos y **semantica observable identica a CT-16** (textos, orden, avisos, errores) | G4 |
| T-M05 | Asignador con politica de nombre inyectada: «- copia» historico y `<base> - espejo` | G4 |
| T-M06 | `Assess` por kind: cada fila `MC` y `UK` de §7 y cada precondicion de §7.9 detectada con motivo atribuible | G5 |
| T-M07 | `Reflect` involutivo: `Reflect(Reflect(D)) = N(D)` para diseños con disposicion `ALLOW` o `CANONICALIZE` | G5 |
| T-M08 | Normalizaciones N-S02, N-S06, N-S07 y N-H02 con las **nueve** obligaciones de §6.5 | G5 |
| T-M09 | V-BOM-1 y V-BOM-2 por kind sobre fixtures | G5 |
| T-M10 | Portadores de §6.4 identicos, incluidos `PropertyValues` con `Kind` desconocido, `DimensionViews` y `ExtensionData` del sobre | G5 |
| T-M11 | Autoridad authored por kind (divergente, ilegible, igual) | G5 |
| T-M12 | Conmutacion `Π(μD) ≡ F ∘ Π(D)` por (kind, vista admisible) | G6 |
| T-M13 | Autoridad de plan = builders existentes para las mismas entradas | G6 |
| T-M14 | Plan del espejo: E1..E8 sin mutacion, grupos multi-rack con `NewRackId` distintos, sin `WithDesign` | G6/G7 |
| T-M15 | Guardas del Plugin (§12.3) con RED demostrado por violacion temporal no commiteada | G7 |
| T-M16 | Censo 33 → **34** (guarda reapuntada con motivo) | G7 |
| T-M17 | C-2: autoridad frente a `EditX` (C2-1 = C2-2) | **G6** |
| T-M18 | Cruce I-50: `DimensionViews` exacto a traves del espejo (centinelas `-8`, `13`, `null`) — **incondicional** | G5 (portador) y G8 (cruce final) |
| T-M19 | Cruce I-49: entradas `expression` sobreviven y la autoridad efectiva integrada se invoca | G8 (si I-49 integrada) |
| T-M20 | Cruce I-54: propiedades de alcance Rack sobreviven exactas | G8 (si I-54 integrada) |
| T-M21 | Huella de §9.2: cambio de cualquier campo entre SNAPSHOT, PREPARE y MUTATE ⇒ fail-closed | G4 (comparador puro) y G7 |
| T-M22 | Canonizacion de `View`/`Section` por kind (§3.7), incluida la `View` vacia del Selectivo con `Section ≥ 0` ⇒ FAIL_CLOSED | G5 |
| T-M23 | `MissingInstances.Count > 0` ⇒ excepcion en el camino de I-52 y ninguna copia | G6 (guarda G-M11 + build Plugin; efecto real en M-19) |
| T-M24 | Sustrato real por kind (lectura y escritura con la autoridad existente) | G5 |
| T-M25 | Criterio PV-5: Selectivo con registro no acreditable ⇒ E7 con y sin vinculos; kinds sin vinculos ignoran el registro | G5 |
| T-M26 | La autoridad de plan con `RackName` dibuja `<base> - espejo` en la anotacion de nombre | G6 |
| T-M27 | Equivalencia con declaracion de simetria (desplazamiento con centro en el marco local tras `T(−BlockOrigin)` y volteo de mano) e integridad del registro (bloque en `blocks.csv`, evidencia presente, biblioteca registrada) | G6 |
| T-M28 | Deteccion `UK` de topes Selectivo (S-13/S-13b), **tope posterior de Push Back activo (P-04, P-05, PC-08)**, parrilla desbordada (S-15x) y Auto con baseline (H-06) | G5 |
| T-M29 | C-2 con el ejecutor de variables del Selectivo (C2-1 = C2-3) | **G6** |
| T-M30 | C-2 con el estado del editor → sistema (C2-1 = C2-4), en Core y en UI cuando pase por la ventana | **G6** |
| **T-M31** | **Obligacion 9**: con dos estados del registro, S-02b detectada (`MC`) cuando `Dep(i)`; S-02a (`RN`) conmuta en ambos cuando todas las celdas gobernantes tienen `BeamLengthOverride` | G5 |
| **T-M32** | S-14c `UK` cuando el espacio de indices del desviador depende de un medio frente con `Dep(i)`; dos estados del registro | G5 |
| **T-M33** | **V-VIEW-ALL**: se enumeran todas las vistas admisibles sin leer el dibujo; una vista admisible **no seleccionada** que no conmuta hace fallar el rack | G6 |
| **T-M34** | Tarimas: S-17c `UK` con fila de claro completo desbordada; S-17b sin desborde conmuta | G5/G6 |
| **T-M35** | **V-META**: miembro desconocido no vacio en el payload semantico ⇒ `UK`; sobre, envoltorio, `PropertyValues`, `CustomProperties` y miembros retirados no bloquean | G5 |
| **T-M36** | **Guarda de cobertura estructural**: cada miembro de los tipos cubiertos y cada descriptor vinculable clasificado exactamente una vez, con naturaleza; miembro o descriptor nuevo sin clasificar ⇒ RED | G5 |
| **T-M37** | ST-17: referencia dinamica, anonima o anotativa con payload RackCad ⇒ E4 con mensaje propio; sin payload ⇒ ST-2 | G7 |
| **T-M38** | **V-RT-STORE** (`Miembros(LB) ⊆ Miembros(M)` e idempotencia), independiente de C-2 | G5 |
| **T-M39** | V-BOM-1 frente a V-BOM-2: un fixture donde difieren las precisiones demuestra que ninguna sustituye a la otra | G5 |
| **T-M40** | **Negativa de simetria**: ninguna declaracion admitida hace equivalente un fixture AR-01 ni el del tope posterior de Push Back; un centro «conveniente» se rechaza; el registro rechaza piezas de rol `Tope` | G6 |
| **T-M41** | Read-set del registro: un cambio en una variable no leida no aborta; un cambio en una leida aborta en PREPARE y en MUTATE | G4 (comparador) y G7 |
| **T-M42** | `RepresentabilityReport` lleva las tres dimensiones separadas por fila (clase, evidencia, disposicion) | G5 |

### 12.3 Guardas del Plugin y de Application (nuevas)

| # | Propiedad protegida |
|---|---|
| G-M1 | El comando no contiene constantes `Kind*`, tipos `*KindHandler` concretos, comparaciones ni `switch` sobre `.Kind`, ni literales iguales a un kind |
| G-M2 | En los archivos del camino del espejo (comando, plan, reflectores, autoridad de planes, materializador) **no** aparece ninguna API que cambie el estado de vinculo o escriba el registro: `ProjectVariablesRegistry.TryWrite(`, `ProjectVariablesData.Write(`, `RegistryCommit`, `ProjectVariableMutationExecutor`, `ProjectVariableMutationPreflight`, `ProjectVariableIntent`, `SelectiveBindingIntent`, `SelectiveBindingIntentPreflight`, `LinkedPropertyReconciler`, `LinkedPropertyEditSession`, `LinkedPropertyEditState.Literal(`, `LinkedPropertyEditState.Create(`, `WithDesign(` ni equivalentes. La lectura efectiva queda **autorizada** (`ProjectVariablesRegistry.Read(`, `SelectiveEffectiveDesignResolver`) |
| G-M3 | Ningun `Matrix3d` ni `Autodesk.` en los archivos nuevos de Application |
| G-M4 | Exactamente una transaccion de escritura en MUTATE; `EnsureForPlan(` solo en PREPARE; nada tras el commit salvo mensajes (sin `PurgeUnreferenced(`, `SyncName(`); `Regen(` solo si §8.6 lo autoriza con evidencia |
| G-M5 | Sin `CloneDefinition(` en el comando (no es el camino de copia) |
| G-M6 | Sin `FindRackBlocks(`, `ScanEnvelopes(` ni `SelectAll` (sin busqueda de hermanas; V-VIEW-ALL es pura) |
| G-M7 | Una `GetSelection(` sin filtro de tipo; `CurrentUserCoordinateSystem` solo en la conversion de la linea |
| G-M8 | El materializador no contiene nombres de kind ni de propiedades de sistema |
| G-M9 | Mapa de invocaciones de builders y servicios de dibujo de `EditX` y del ejecutor en las vistas admitidas (linea base CT-26); nace en G6 y sigue en G8 |
| G-M10 | Ningun literal de simetria de bloques fuera del registro tipado de §11.3 |
| G-M11 | El materializador de I-52 comprueba `MissingInstances` tras `CreateSystemBlock` y lanza; `LateralHeaderDrawer.cs` sin cambios |
| G-M12 | Si no se extrae un SNAPSHOT compartido: `RackDuplicarCommands.cs` identico al de la base y guardas de I-51 sin reapuntar |
| **G-M13** | Las tablas de clasificacion de miembros y de descriptores viven en Application, una por kind junto a su reflector; el Plugin no contiene clasificacion ni `switch` por kind |

### 12.4 Suites y builds

Cada gate con codigo: Core completo en local (UI segun LC-UI en iteracion y completa si el gate toca
`tests/RackCad.UI.Tests`), CI verde sobre el SHA exacto. Candidato: Core y UI completas en local, builds Debug de UI y
Plugin con AutoCAD cerrado, CI 4/4 sobre el SHA exacto y cobertura del Candidato (WORKFLOW 4.5).

---

## 13. Validacion manual del Owner (AutoCAD 2025)

| # | Escenario |
|---|---|
| M-1 | Selectivo sin topes, frontal + planta, linea vertical: copia reflejada, textos legibles, numeracion regenerada, nombre `<base> - espejo` |
| M-2 | UCS girado 30° y origen desplazado: `P'` correcto en WCS |
| M-3 | Guardar, cerrar y reabrir: la copia sigue reflejada |
| M-4 | `RACKEDITAR` sobre la copia → Actualizar: el dibujo **no** cambia (C-2) |
| M-5 | Desde la copia, Insertar una vista admisible no reflejada (por ejemplo, la planta si solo se reflejo la frontal) y una lateral: salen del diseño reflejado y la admisible es espejo de la del origen |
| M-6 | `RACKBOMTOTAL`: la copia cotiza igual que el origen |
| M-7 | Selectivo con vinculos sin dependencia efectiva: la copia conserva `VariableId`; `ChangeValue` la redibuja **y sigue siendo espejo** del origen |
| M-8 | Dos racks (frontal + planta de cada uno): dos `NewRackId`, nombres `<base> - espejo` |
| M-9 | Fail-closed sin mutacion: lateral seleccionada, cama, MINSERT, escala no uniforme, fuente espejada, esquina, brazo C sencillo, Selectivo con topes, **Push Back con tope posterior activo**, definicion con BASE movida, seccion invalida, cabecera de plantilla con Auto, **medio frente con `PalletTolerance` vinculada sin overrides**, **tarimas desbordadas** |
| M-10 | UNDO: se registra que revierte y que ocurre con lo importado (evidencia; UNKNOWN hasta entonces) |
| M-11 | Dinamico frontal (salida y entrada) + planta |
| M-12 | Push Back compuesto sin tope posterior activo: frontales 0..3 + planta |
| M-13 | Cantilever frontal + planta |
| M-14 | Cabecera lateral + planta con diagonales explicitas y postes asimetricos |
| M-15 | Censo de mano: confirmacion visual de los bloques declarados simetricos, con eje y centro (O-3) |
| M-16 | Anotaciones y cotas legibles, respetando la politica de `DimensionViews` de I-50 |
| M-17 | **Regresion de RACKDUPLICAR** (G4 edita `RackDuplicationPlan.cs`): multiples origenes y destinos, mensajes y fail-closed de I-51 |
| M-18 | Selectivo legado con frontal `Section = −1`: la copia sale con `Section = 0` y `RACKEDITAR` la trata igual |
| M-19 | Bloque de biblioteca ausente (con una biblioteca de prueba, nunca la del Owner): RACKMIRROR falla sin crear copias |
| M-20 | Efectos de infraestructura tras un fallo en PREPARE o MUTATE: que definiciones, capas o estilos importados quedan (evidencia) |
| **M-21** | Rack con una vista admisible **no seleccionada** que no conmuta (fixture de prueba): falla cerrado sin mutacion y el mensaje nombra la vista |
| **M-22** | Payload con miembro desconocido en el diseño (escrito por un build de prueba): falla cerrado; con el mismo miembro solo en el sobre, el espejo lo conserva |
| **M-23** | Rack convertido por el usuario en bloque dinamico o anonimo (BEDIT): falla cerrado con mensaje propio, no se ignora |

---

## 14. Proceso y gates

### 14.1 Secuencia normativa (sustituye a la de V2)

```text
Consenso tecnico sobre V3 (Coordinador + Arquitecto, misma version)
→ rebase / reconciliacion sobre el main vigente (solo docs; resolver el conflicto del indice de ADR)
→ freeze por SHA exacto + re-check del contenido normativo (Coordinador y Arquitecto)
     si el rebase cambia contenido normativo material → vuelve a revision
→ Owner acepta O-1 (alcance)
→ G3 characterization                                     (ADR-0036 NO es precondicion)
     contradiccion con V3 → Proposal V4
     confirmado           → Owner acepta ADR-0036 (O-4)
→ G4 implementation                                       (ADR-0036 aceptado ES precondicion)
```

### 14.2 Tabla de gates

Cada gate declara entrada, archivos permitidos, RED esperado, PASS requerido, parada y evidencia por SHA exacto. Ningun
gate arranca sin la orden explicita del Coordinador y sin el preflight de paralelas (§15.3). **Cualquier contradiccion
de una caracterizacion o de una prueba con el contrato congelado ⇒ STOP → Proposal V4**; nunca un parche silencioso.

| Gate | Entrada | Archivos permitidos | RED esperado | PASS requerido | Parada | Evidencia |
|---|---|---|---|---|---|---|
| **G2** (este) | revision de V2 | `docs/**` | — | consenso de Coordinador y Arquitecto sobre la misma version; despues, rebase/reconciliacion, freeze por SHA, re-check normativo y **O-1** del Owner; contrato reescrito; hallazgos laterales en `ideas-futuras.md` | desacuerdo material ⇒ Proposal V4 | SHA acordado, SHA de freeze + CI |
| **G3** Characterization | freeze de G2 + O-1 aceptada (**sin** ADR aceptado) | `tests/RackCad.Tests/**` (caracterizaciones y fixtures nuevos); `tests/RackCad.UI.Tests/**` si una caracterizacion lo exige | ninguno: CT-01..CT-35 pasan sobre produccion intacta | Core completo (UI si se toco); censos de mano, parametros y cobertura; tabla de `c`; auditoria de anclajes; semantica observable de I-51; decodificacion de secciones e historia de `View`; lineas base de store; topes (Selectivo y Push Back); marco de simetria y su caracterizacion negativa; N-S02 con dos estados del registro; dependencia efectiva; S-14 asociado; enumeracion `A_k`; tarimas; metadata desconocida; fuentes dinamicas/anonimas; V-RT-STORE separado de C-2 | contradiccion con el contrato ⇒ **STOP → Proposal V4** | SHA + conteos + CI. Tras PASS: se pide **O-4** |
| **G4** Geometria y nucleo neutral | G3 PASS **y ADR-0036 aceptado por el Owner** | `src/RackCad.Application/Geometry/*`; nucleo neutral y fachada en `src/RackCad.Application/Persistence/` (incluye `RackDuplicationPlan.cs`, **con aviso previo a I-49**); pruebas | T-M01..T-M05, T-M21 y T-M41 (comparadores puros) en rojo sobre andamiaje | verdes; T1–T15 de I-51 y CT-16 identicos (textos, orden, avisos, errores); `RackDuplicarCommands.cs` sin cambios; tolerancia de escala fijada y registrada; **sin cambio observable de RACKDUPLICAR** | cambio observable de RACKDUPLICAR | SHA + conteos + CI |
| **G5** Reflectores y representabilidad | G4 | contrato, registros y tablas de clasificacion en carpeta nueva del espejo en Application; reflector por kind en `src/RackCad.Application/Systems/<Kind>/` y `RackFrames/`; registro de simetrias vacio; declaracion de invariancia de metadata vacia; pruebas. Divisible en G5a Selectivo, G5b Dinamico y Push Back, G5c Cantilever y cabecera | T-M06..T-M11, T-M18 (portador), T-M22, T-M24, T-M25, T-M28, T-M31, T-M32, T-M34, T-M35, T-M36, T-M38, T-M39, T-M42 en rojo | verdes; sustrato real; topes Selectivo y Push Back `UK`; H-06 `UK`; obligacion 9; V-META; cobertura estructural; `WithDesign` ausente | una fila `R` o `RN` resulta no equivalente ⇒ STOP → V4 | SHA por subgate + CI |
| **G6** Planes, materializador y **C-2** | G5 | autoridades de plan por kind en Application; `SystemBlockWriter.cs` (`CreateInTransaction`); materializador nuevo en `src/RackCad.Plugin/Systems/Shared/`; pruebas Core y `tests/RackCad.UI.Tests/**` para C-2 | T-M12..T-M14, **T-M17**, T-M23, T-M26, T-M27, **T-M29**, **T-M30**, T-M33, T-M40 y G-M9 en rojo | conmutacion y V-VIEW-ALL verdes; `MissingInstances` = fallo duro; **C2-1..C2-4 GREEN en CI** (Core y UI) con G-M9, nombre logico, estado efectivo, `View` y `Section` canonicas; builders y drawers sin cambio de comportamiento. **G6 no cierra sin C-2** | necesitar cambiar un builder o un drawer existente; divergencia de C-2 ⇒ decidir C-1 con Coordinador y Arquitecto antes de cerrar G6 | SHA + CI + build Debug de Plugin |
| **G7** Comando RACKMIRROR | G6 cerrado (**C-2 verde**) | `src/RackCad.Plugin/RackMirrorCommands.cs` (nuevo) y SNAPSHOT propio; archivos de guardas; `SelectiveEditorOpenTests.cs` (censo); `src/RackCad.UI/RackCommandReference.cs`. `RackDuplicarCommands.cs` **solo** si se decide extraer un SNAPSHOT compartido (§5.2) | T-M15, T-M16, T-M37, T-M41 (MUTATE), G-M1..G-M8 y G-M10..G-M13 en rojo por violacion temporal | guardas verdes; censo 34; build Debug de Plugin; sin extraccion, archivos y guardas de RACKDUPLICAR intactos (G-M12) | C-2 no verde ⇒ G7 no arranca; debilitar una guarda de I-51 | SHA + conteos + CI |
| **G8** Cruces y continuidad | G7 | pruebas cruzadas; continuidad de G-M9; produccion de `RACKEDITAR` **solo** si se activo C-1 con orden del Coordinador | T-M18 (cruce final), T-M19 y T-M20 en rojo cuando apliquen | cruces verdes con lo integrado (I-49, I-50, I-53, I-54); reconciliacion final con autoridades integradas | un cruce exige tocar §20.3 | SHA + CI |
| **G9** Conformidad, Candidato y Owner | G8 | ninguno de produccion salvo correcciones | — | rebase final si `main` avanzo; Core y UI locales; builds Debug; CI 4/4 exacto; cobertura; M-1..M-23 (M-17 incluido); O-3 confirmado | cualquier fallo ⇒ vuelta al gate que corresponda | SHA Candidato + corridas + veredicto del Owner |
| **G10** Documentacion e integracion | G9 | `docs/**`, `README.md` | — | WORKFLOW 4.5: cierre documental, merge `--no-ff`, CI del `MERGE_SHA`, cobertura, limpieza | CI post-merge rojo ⇒ correccion en la rama | SHAs de cierre y merge |

---

## 15. Coordinacion con iniciativas paralelas

### 15.1 Tips observados en el preflight de V3

```text
origin/main                                        = f8deb675c6d1ef0e64693b157d69c4cc170d7b24 (Merge I-50; sin avance desde V2)
I-52 feature/rackmirror-espejo-semantico           = 04457183fc2d7dcda5d6c1b988a4789ed4ea2f8f (V2; base 46fcac2; sin rebase)
I-49 architecture/motor-expresiones-parametricas   = 1ed93a0525ec11c5092df89c55cbf498c99f8e2a (Proposal V6; rebasada sobre main f8deb67
                                                     sin cambios de contenido; antes 048a508; solo docs frente a main)
I-53 feature/cabeceras-configurables-multidestino  = 5efaf7e1ecf8b070e06e4bd6fe8aca179afda847 (G2-F: contrato congelado y
                                                     ADR-0037 ACEPTADO en su rama; rebasada sobre main f8deb67; solo docs frente a main)
I-54 architecture/propiedades-personalizadas       = 5d25da89972df2468f1d03243301761f5463e6eb (Proposal V3; solo docs; base 46fcac2)
I-50                                               = integrada en main; rama remota retirada
```

Ninguna paralela tiene cambios productivos frente a `main`. Entre la base `46fcac2` y `main @ f8deb67` no cambian
`RackDuplicationPlan.cs`, `RackDuplicarCommands.cs`, `RackEnvelopeRestamp.cs`, `RackEmbedComposer.cs`,
`SystemBlockWriter.cs`, `LateralHeaderDrawer.cs`, `CantileverViewMaterializer.cs`, `BlockLibraryImporter.cs`,
`RackProjectStore.cs`, `RackProject.cs` ni los builders de §8.1; si cambian, por I-50, los archivos con lineas de
portador `DimensionViews` y los emisores de cotas. Todas las citas de V3 estan re-ancladas contra `main @ f8deb67`.

### 15.2 Cruces

| Paralela | Cruce | Tratamiento |
|---|---|---|
| **I-50** (**integrada** en `main @ f8deb67`) | `DimensionViews` en `SelectivePalletDesignDocument.cs`, `DynamicRackSystemDocument.cs`, dominio y resolvedores (C-01..C-15); politica de cotas en `SelectiveDimensions.cs`, `DynamicViewDecorations.cs`, `DimensionViewPolicy.cs`; ventanas ricas; ADR-0035 aceptado | `DimensionViews` es requisito normativo (§10.3); T-M18 incondicional; firmas de anotaciones de C-2 y V-VIEW-ALL con la politica de I-50; I-52 no toca esos archivos |
| **I-49** (V6, `1ed93a0`) | `expression` en `PropertyValues`; su condicion de parada cubre `RackDuplicationPlan`, restamp, `SelectiveAuthoredAuthority` y store Selectivo (I-49 V6 §12.2); su §12.3 registra que G4 de I-52 toca `RackDuplicationPlan.cs` con aviso previo | **Aviso a I-49 antes de editar `RackDuplicationPlan.cs`** (G4); T-M19; P24.8 de I-49 (el espejo conserva `expression` sin interpretarla); si I-49 integra antes, su autoridad efectiva y su descriptor vinculable se clasifican en la guarda de cobertura (§6.7) |
| **I-53** (G2-F, `5efaf7e`) | Contrato congelado y **ADR-0037 aceptado** en su rama; copias de configuracion de cabecera que caen en campos **ya persistidos** (`PostCabeceras`, `ExtraFondoPostCabeceras`, `Modules[].Header`), **sin DTO ni campo nuevo**; serializacion por archivos calientes con I-49 (`RackSelectiveWindow.xaml.cs`) | Sin campo nuevo indexado que exija regla de espejo; si I-53 integra antes, la guarda de cobertura re-mide los tipos; el indice de ADR conserva 0035, 0036 y 0037; serializar si C-1 llegara a tocar comandos de `RACKEDITAR` |
| **I-54** (V3, `5d25da8`) | Miembro `CustomProperties` del sobre (`JsonElement?`), heredado por `Compose`, sin promocion de major; **D-21**: el espejo cumple (A y despues B); T-GRD-02 7 → 8; T-GRD-03 convive con G-R5 | `CustomProperties` es **portador** (§6.4) y V-META no bloquea por el; ID-5 intacto; quien integre despues re-apunta el censo de T-GRD-02; T-M20 |
| Documental | Filas de ROADMAP, final de `ideas-futuras.md`, `docs/adr/README.md` (0035 en `main`, 0036 de I-52, 0037 de I-53), censo de comandos 33 → 34 | Quien integre despues conserva todas las filas y entradas en orden numerico y re-mide los censos |

### 15.3 Protocolo antes de G3, G5, G7 y del Candidato

`git fetch --all --prune`; `git diff --name-only origin/main...origin/<rama>` de I-49, I-53 e I-54 (y de cualquier
iniciativa nueva); leer sus Proposals y contratos en el SHA exacto; buscar `docs/adr/0036-*` y citas de «ADR-0036» en
**todos** los refs (§16); comprobar los archivos de §19; si `main` avanzo, rebase segun WORKFLOW antes de escribir codigo.

---

## 16. ADR-0036: numeracion y aceptacion

- Archivo: `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`, estado **`propuesto`**, con fila en el indice.
  Actualizado en este gate con las correcciones de V3.
- Congela: espejo semantico por copia; `μ_k` canonica por kind; admisibilidad por vista con seccion canonica y
  verificacion de **todas** las vistas admisibles; fail-closed; reflectores en Application sobre el sustrato real;
  prohibicion de normalizaciones que congelen valores dependientes de vinculos; metadata semantica desconocida ⇒
  fail-closed; autoridad de planes; colocacion canonica sin escala negativa, `Origin = 0` y sin bloques dinamicos,
  anonimos o anotativos; identidad nueva; portadores; atomicidad semantica e importacion best-effort; simetria en el
  marco local tras `T(−BlockOrigin)`; verificacion dinamica por rack; taxonomia en tres dimensiones.

**Protocolo de numeracion (vigente desde V2).**

1. Un numero de ADR queda **reclamado** por la primera publicacion **observable** de un archivo `docs/adr/NNNN-*` en un
   ref remoto.
2. **Registro de I-52:** ADR-0036 se publico por primera vez en `origin/feature/rackmirror-espejo-semantico` con el
   commit `0fc7032bf15d03e7d478bbd9350f156708621c9d` (`2026-09-12T19:59:36-06:00`; CI del push `34731908035`, creada el
   `2026-09-13T01:59:44Z`). En el preflight de V3 ningun otro ref contiene `docs/adr/0036-*`.
3. **Otros numeros observados:** ADR-0035 es de I-50 (aceptado e integrado en `main`). ADR-0037 lo publico I-53 en
   `d7f17ad` (`2026-09-12T21:24:06-06:00`); tras rebasar, su primera aparicion en el ref actual es `d0db698`
   (`2026-09-12T22:25:31-06:00`) y quedo **aceptado** en `5efaf7e`. Ambas publicaciones son posteriores a 0036 y usan otro
   numero: **sin colision**.
4. **Antes de pedir la aceptacion del Owner**, I-52 busca 0036 en todos los refs: archivos `docs/adr/0036-*` y citas de
   «ADR-0036» o «adr/0036». Si existe una publicacion **anterior** de otro ADR-0036, I-52 renumera antes de la
   aceptacion; una publicacion posterior no obliga a I-52 a renumerar.
5. Una vez ADR-0036 sea **`aceptado`**, **no** se renumera (`adr/README.md`: un aceptado es inmutable).
6. Dos ADR **aceptados** con el mismo numero ⇒ **STOP** y escalado al Owner.

**Aceptacion (sustituye a V2).** Solo el Owner (O-4). **No** es precondicion de G3; se pide **despues de G3**, si G3
confirma el contrato, y **es precondicion de G4**. Mientras tanto el ADR sigue `propuesto` y puede corregirse si G3
abre Proposal V4. `docs/adr/README.md` no cambia en este gate: titulo, estado y numero del indice siguen siendo
correctos.

---

## 17. Riesgos abiertos

| # | Riesgo | Mitigacion |
|---|---|---|
| R-1 | Censo de mano: muchas piezas podrian diferir y dejar kinds enteros en fail-closed hasta la declaracion de simetria (eje, centro y marco) | CT-06 en G3; O-3; M-15 |
| R-2 | Un Dinamico dibujado solo en lateral no puede reflejarse | Mensaje E5 explicito; ALT-L como futuro |
| R-3 | V-RT-STORE puede rechazar payloads legitimos si la idempotencia del store no es exacta | CT-35 caracteriza por kind |
| R-4 | C-1, si llegara a activarse, toca comandos calientes que I-49 e I-53 tambien tocan | C-2 por defecto; C-1 solo con evidencia; serializar |
| R-5 | Semantica de «estandar» de cabecera tras normalizar Auto (H-06): la mayoria de las cabeceras de plantilla quedan fuera | G5 con CT-23; fail-closed mientras tanto; declarado en §2.3 y O-1 |
| R-6 | `MountingFace` sin semantica confirmada (S-10, D-03b) | CT-12; si no se confirma ⇒ FAIL_CLOSED |
| R-7 | Comandos transparentes de AutoCAD entre SNAPSHOT y MUTATE | Huella y read-set de §9.2 en PREPARE y MUTATE |
| R-8 | Numeracion de ADR con varias iniciativas que necesitan ADR | Protocolo de §16 |
| R-9 | Cantilever con brazos C/L sencillos o arriostramiento estructural asimetrico queda fuera | Clasificacion MC/UK; futuro con metadato de mano |
| R-10 | Rendimiento de V-VIEW-ALL y V-BOM en selecciones grandes (dos planes por vista admisible, dos BOM por rack) | Medir en G7; builders puros |
| R-11 | Selectivos con topes y Push Back con tope posterior activo inutilizables hasta O-2/O-2B | Fail-closed verificable antes de mutar; decision futura |
| R-12 | La auditoria de anclajes (CT-17) puede degradar muchas filas `R` + `G3_PENDING` a `UK` | Proposal V4 con la tabla corregida; nunca parche silencioso |
| R-13 | G4 toca el planificador de RACKDUPLICAR, validado por el Owner en I-51 | CT-16 primero; fachada con semantica observable identica; M-17; aviso a I-49 |
| R-14 | V-META deja fuera racks escritos por un build mas nuevo con metadata semantica | Fail-closed deliberado: sin transformador no hay espejo fiel; futuro con declaraciones de invariancia |
| R-15 | C-2 sobre ventanas WPF exige pruebas de UI mas costosas, ahora en G6 | `tests/RackCad.UI.Tests` permitido en G3 y G6; LC-UI |
| R-16 | La importacion de biblioteca puede dejar dependencias tras un fallo y su UNDO no esta demostrado | §9.3 y §9.5 lo declaran; M-10 y M-20 lo registran |
| **R-17** | Medios frentes con `PalletTolerance` vinculada y sin overrides quedan fuera | S-02b declarado; ALT-ABS como futuro |
| **R-18** | La guarda de cobertura obliga a clasificar cada miembro nuevo de cinco familias de tipos | Coste asumido: es la unica defensa ejecutable contra copias sin permutar |
| **R-19** | Aceptar el ADR despues de G3 añade una ronda del Owner entre G3 y G4 | Aceptado por el Coordinador: evita un ADR aceptado que G3 podria refutar |

---

## 18. Desacuerdos y decisiones

### 18.1 Decisiones registradas (no son consenso)

| # | Tema | Resolucion vigente | Donde |
|---|---|---|---|
| DA-1 | Convergencia con `RACKEDITAR` | C-2 por defecto con los cuatro productores, CI, G-M9 y M-4; **GREEN en G6**; C-1 solo con evidencia | §8.5 |
| DA-2 | Declaracion de simetrias | Registro tipado unico en Application; `assets/` y catalogos fuera; eje, centro en marco local tras `T(−BlockOrigin)`, evidencias y biblioteca registrada | §11.3 |
| DA-3 | Politica post-commit | Sin `Regen`; un unico `Regen` solo con evidencia del Owner | §8.6 |
| DA-4 | Recorte del primer corte | Se mantiene; O-1 despues del freeze tecnico y antes de G3; fail-closed adicionales de §2.3 | §2 |
| DA-5 | `IsStandard`/`StandardBaselineId` | UNKNOWN → FAIL_CLOSED hasta G5 | §6.5.4 |
| DA-6 | Numeracion de ADR | Precedencia del primer ref remoto publicado | §16 |
| CR-V2-1 | Registro | Criterio de produccion; I-52 no lo relaja | §10.2 |
| CR-V2-2 | Centro de simetria | Aceptado, con marco local tras `T(−BlockOrigin)` | §11.3 |
| CR-V2-3 | Tope posterior Push Back | Superado por evidencia de codigo: `UNKNOWN → FAIL_CLOSED` | §7.3 |
| Proceso | Aceptacion del ADR | Despues de G3; precondicion de G4 | §14.1, §16 |

### 18.2 Desacuerdos abiertos

**Ninguno material.** Quedan diferidos con fail-closed los asuntos de §0.3. Para la atencion del Arquitecto, sin ser
desacuerdo:

- S-10 y D-03b dependen del resultado de CT-12; el contrato ya fija que sin confirmacion fallan cerrado.
- El predicado `Dep(i)` se fija con la regla del codigo; CT-27/CT-28 pueden descubrir dependencias adicionales, lo que
  llevaria a Proposal V4.
- V-VIEW-ALL puede reducir el alcance util de racks multi-fondo si alguna frontal de fondo no conmuta.

---

## 19. Condiciones de parada

- Implementar sin la orden explicita del gate: **prohibido**.
- Una caracterizacion o prueba contradice el contrato congelado, o una fila `R` o `RN` resulta no equivalente:
  **STOP → Proposal V4**.
- Necesitar cambio de schema, de sobre o de Xrecord, o de `RackEnvelopeRestamp.cs`, `PushBackMirror.cs`, `WithDesign`, un
  drawer o builder existente o el registro de variables: **detenerse**; nueva revision de Arquitecto y ADR.
- Un adaptador necesita una costura del store distinta de `WithSourceMetadataFrom`: **detenerse** (§6.2).
- Una paralela modifica materialmente `RackDuplicationPlan.cs`, `RackEmbedComposer.cs`, `SystemBlockWriter.cs`,
  `LateralHeaderDrawer.cs`, `CantileverViewMaterializer.cs`, `RackEnvelopeRestamp.cs`, `BlockLibraryImporter.cs`,
  `DynamicRackSystemResolver.cs`, los builders de §8.1, los stores authored o los descriptores vinculables:
  **detenerse** antes de editar y reportar SHA y diff.
- Aparece un miembro nuevo en un tipo cubierto o un descriptor vinculable nuevo sin clasificar: **RED / STOP** (§6.7).
- Aparece un parametro dinamico con semantica de lado sin regla: fail-closed y registro para V4.
- G7 intenta consumir la autoridad sin C-2 verde: **prohibido**.
- Abrir G4 sin ADR-0036 aceptado: **prohibido**.
- Debilitar una guarda de I-51 para facilitar el cambio: **prohibido**.

---

## 20. Archivos (sin cambiarlos en este gate)

### 20.1 Produccion probable (G4–G8)

| Capa | Archivo o area |
|---|---|
| Application | `Geometry/Transform2D.cs` (`ReflectionAboutLine`) y valor de colocacion junto a el |
| Application | `Persistence/RackDuplicationPlan.cs` (fachada, con aviso a I-49) + nucleo neutral nuevo en `Persistence/` |
| Application | carpeta nueva del espejo: contrato de reflector, registro, plan del espejo, decodificacion de vistas y `A_k`, equivalencia, declaracion tipada de simetrias, declaracion de invariancia de metadata, tablas de cobertura |
| Application | `Systems/Selective/`, `Systems/Dynamic/`, `Systems/PushBack/`, `Systems/Cantilever/`, `RackFrames/`: reflector, tabla de clasificacion y autoridad de plan por kind |
| Plugin | `RackMirrorCommands.cs` (nuevo) y SNAPSHOT propio; materializador en `Systems/Shared/`; `Systems/Shared/SystemBlockWriter.cs` (`CreateInTransaction`) |
| Plugin | `RackDuplicarCommands.cs` **solo** si se extrae un SNAPSHOT compartido; comandos de `RACKEDITAR` **solo** si se activa C-1 |
| UI | `RackCommandReference.cs` (ayuda) |

### 20.2 Pruebas

Caracterizaciones CT-01..CT-35; pruebas T-M01..T-M42; guardas G-M1..G-M13; `SelectiveEditorOpenTests.cs` (censo 34);
guardas de I-51 reapuntadas en `SelectiveDuplicationFailClosedTests.cs` solo si se extrae el SNAPSHOT; pruebas de UI
en `tests/RackCad.UI.Tests` para C-2 (G6) y las caracterizaciones de UI (G3).

### 20.3 NO TOCAR sin nueva revision de Arquitecto

`RackEnvelopeRestamp.cs`, `RackCloner.cs`, `PushBackMirror.cs`, `SelectivePalletDesign.cs`,
`SelectivePalletDesignDocument.cs` (incluido `WithDesign`), DTO y stores de todos los kinds, `RackProject.cs`,
`LateralHeaderDrawer.cs`, `CantileverViewMaterializer.cs`, `BlockLibraryImporter.cs`, builders de plan,
`ProjectVariables/*` salvo consumo de lectura, `Bom/*` salvo consumo, `Catalogs/*` salvo consumo (I-19),
`KindHandlers/*`, editores WPF, `assets/`, catalogos, biblioteca de bloques, `.github/`, ADR aceptados.

---

## 21. Estado

```text
G0 = ACCEPTED        G1 = ACCEPTED (Discovery)
G2 = V3 EN REVISION  (este documento + ADR-0036 propuesto actualizado + docs/automation/decisions/I-52.md)
     V1 (0fc7032) y V2 (0445718) = historial; revision de V2: Architect: CHANGES REQUIRED — PROPOSAL V3
G3 = NOT OPEN        (requiere consenso sobre la misma version, rebase/reconciliacion, freeze, re-check y O-1)
G4+ = NO INICIADO    (G4 requiere ademas G3 PASS y ADR-0036 aceptado)

Proposal Version = V3
Coordinator = REVIEW REQUIRED
Architect = REVIEW REQUIRED
Consensus = NOT REACHED
ADR-0036 = PROPOSED
Implementation = BLOCKED

SUBSTANTIVE IMPLEMENTATION: BLOCKED
```

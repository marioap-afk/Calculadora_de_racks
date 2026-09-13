# I-52 — Proposal V4: RACKMIRROR, espejo semantico de uno o varios racks (ID16)

> # ⚠ PROPOSAL V4 — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```text
> Proposal Version = V4
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Consensus = NOT REACHED
> ADR-0036 = PROPOSED
> G3 = NOT OPEN
> Implementation = BLOCKED
> ```
>
> ```text
> Schema            = SIN CAMBIO (ningun DTO, sobre ni Xrecord cambia en I-52)
> BASE auditada     = 46fcac2b071929d2bd5b07aa28373941417f74a8   (base de la rama; V4 se redacta SIN rebase)
> CITAS             = archivo:linea contra origin/main @ f8deb675c6d1ef0e64693b157d69c4cc170d7b24 (I-50 integrada);
>                     las citas de paralelas llevan su SHA
> CLAIM_SHA         = 55281769d5e9317ad25500e6d3f5f8a849f37279
> BOOTSTRAP_SHA     = 7ee79756d2b93b4eded660cb73310ef3edde32ff
> Discovery G1      = 339b3abd238a3a7a42d60c9edd45a1500a776f62   (se conserva)
> Proposal V1       = 0fc7032bf15d03e7d478bbd9350f156708621c9d   (historial)
> Proposal V2       = 04457183fc2d7dcda5d6c1b988a4789ed4ea2f8f   (historial)
> Proposal V3       = 545c2229de8d7850e03981d85aced0ac63c24040   (historial; CI 34740634542 4/4)
> Revision Arq. V3  = «Architect: CHANGES REQUIRED — PROPOSAL V4»; sin BLOCKER; HIGH AR3-01..AR3-02,
>                     MEDIUM AR3-03..AR3-06, LOW AR3-07..AR3-12; cambios V4-01..V4-13
>                     (registro: docs/automation/decisions/I-52.md §20)
> Coordinador       = orden de V4 con V4-01..V4-13 y precisiones vinculantes (registro §21)
> Paralelas         = §15.1 (preflight de V4)
> Estado de gates   = G0 ACCEPTED · G1 ACCEPTED (Discovery) · G2 V4 EN REVISION · G3 NOT OPEN
> ```
>
> Este documento **no** autoriza produccion, **no** modifica V1, V2, V3 ni el Discovery y **no** es el contrato
> vinculante. El contrato se reescribe en el freeze de G2 (§14.1).
>
> **Nota de transparencia.** V1..V3, sus revisiones de Arquitecto y esta V4 las redacto el mismo agente en roles
> distintos. V4 re-verifica en codigo cada correccion y declara las precisiones que encontro al redactarla
> (`[V4-D24]`..`[V4-D30]`).

---

## 0. Reconciliacion V3 → V4

V1 (`0fc7032`), V2 (`0445718`), V3 (`545c222`) y el Discovery (`339b3ab`) **se conservan sin cambios**. **Donde V3 y V4
difieran, manda V4.** Las marcas `[V3-D01]`..`[V3-D31]` siguen vigentes salvo donde una marca `[V4-Dnn]` las modifica
(§0.4).

### 0.1 Estados de reconciliacion

| Estado | Significado |
|---|---|
| **CLOSED BY V4** | V4 fija el contrato; lo que quede es verificacion ordinaria de gate |
| **DEFERRED TO G3/G5 WITH FAIL-CLOSED** | El dato exacto se obtiene en G3 o G5, y mientras tanto existe un fail-closed inequivoco que no condiciona la arquitectura |
| **OPEN MATERIAL** | Impide presentar V4. **Debe ser NONE** |

### 0.2 Matriz `[V4-D01]`..`[V4-D30]`

| Marca | Origen | V3 | Correccion V4 | Estado | Donde |
|---|---|---|---|---|---|
| `[V4-D01]` | AR3-01 · V4-01 | CT-06 observaba la diferencia de planes, inferia `c` y ese `c` justificaba la equivalencia (evidencia circular) | Se separan **`GeometricEvidence`** (primero; definicion real del bloque, independiente de builders y planes) y **`PlanPlacementEvidence`** (despues; CT-06 solo verifica que los builders usan la simetria aprobada). Queda prohibido derivar un centro de planes, del bounding box, de la diferencia de inserciones o de un minimo de error | CLOSED BY V4 | §11.3 |
| `[V4-D02]` | AR3-01 · V4-01 | Forma afin «si CT-06 la justifica» | Afin `c = a0 + a1·Parametro` solo con (a) igualdad geometrica de la definicion reflejada en cada estado caracterizado y (b) relacion **derivada** de la parametrizacion del bloque; dos puntos que ajustan una recta no bastan. Bloque dinamico: la declaracion solo cubre su `CharacterizedDomain`. Sin (a) ni (b) ⇒ `UNKNOWN` | CLOSED BY V4 | §11.3 puntos 5-6 |
| `[V4-D03]` | AR3-01 · V4-01 | CT-17 clasificaba coordenadas simetricas o ancladas | CT-17 audita por pieza span de anclas, offset unilateral, direccion de estiramiento, origen local y simetria declarada; un `δ` no repartido simetricamente ⇒ `UNKNOWN` **cualquiera sea el rol** | CLOSED BY V4 | §11.7, CT-17 |
| `[V4-D04]` | AR3-01 · V4-01 | T-M40 solo con fixtures de rol `Tope`, que caen por rol | T-M40 generalizada: rol `Tope`; rol distinto con parametro de longitud; pieza de ancho fijo; afin candidata (caso C); centro conveniente incorrecto (caso D). El rechazo de `Tope` queda como defensa adicional | CLOSED BY V4 | §11.3 puntos 10-11, T-M40 |
| `[V4-D05]` | AR3-01 · V4-01 | — | Evidencia concreta por bloque (CT-36) y canonizacion determinista de `DefinitionGeometryHash` | DEFERRED TO G3/G5 WITH FAIL-CLOSED: sin evidencia, fuera de dominio o sin huella determinista la declaracion no aplica ⇒ X-17 `UNKNOWN` | §11.3, CT-36 |
| `[V4-D06]` | AR3-02 · V4-02 | La obligacion 9 solo regia normalizaciones | **Obligacion 9b — DYNAMIC-STABILITY**: toda decision `ALLOW`/`CANONICALIZE` cuyo predicado dependa directa o transitivamente de una propiedad vinculable conserva su verdad para **todo** estado acreditable futuro; si no puede probarse ⇒ `UNKNOWN` o `REQUIRES_MODEL_CHANGE` ⇒ `FAIL_CLOSED`. Aplica a fit, overflow, ancho gobernante, espacios de indices, cantidad o posicion dependiente de geometria efectiva y reglas futuras | CLOSED BY V4 | §6.5.5 |
| `[V4-D07]` | AR3-02 · V4-02 | V-DEP = reglas de §6.5 (S-02b, S-14c) | **V-DEP es autoridad explicita**: cada fila `ALLOW`/`CANONICALIZE` declara inputs authored, inputs efectivos, descriptores vinculables transitivos, predicado y prueba de estabilidad o motivo de fail-closed; cierre efectivo por descriptor; inventario DEP-01..DEP-08; guardas T-M43, CT-37 y CT-38 | CLOSED BY V4 | §6.5.5, §6.7 |
| `[V4-D08]` | AR3-02 · V4-02 | S-15 y S-17b se evaluaban solo con el `R` del SNAPSHOT | **S-15d** y **S-17d**: parrilla con conteo por defecto y tarimas con `Dep(i)` solo `ALLOW` con `DEP-ROW` (`count·frente ≤ W_lb(i)`); `W_lb` usa una cota inferior **solo si es autoritativa**; en `main` no existe, asi que solo cuentan los overrides positivos | CLOSED BY V4 | §6.5.5, §7.1 |
| `[V4-D09]` | AR3-02 · V4-02 | PV-7 listaba S-02b y S-14c | PV-7: `ChangeValue` no puede invalidar **ninguna** decision `ALLOW`/`CANONICALIZE` tomada en PREFLIGHT | CLOSED BY V4 | §10.2 |
| `[V4-D10]` | AR3-02 · V4-02 | — | Caracterizacion de casos A/B, dos o mas estados del registro, cotas autoritativas o su ausencia, fondo maestro y cambio de gobernante | DEFERRED TO G3/G5 WITH FAIL-CLOSED: sin prueba ⇒ S-15d/S-17d/X-18 `UNKNOWN` | CT-27, CT-28, CT-37, CT-38 |
| `[V4-D11]` | AR3-03 · V4-03 | P-03 declaraba `DrawPallets` como R/ALLOW sin condicion | Tarimas frontales de Push Back separadas: **P-03b/PC-14a** sin desborde = `REPRESENTABLE` + `G3_PENDING` + `ALLOW`; **P-03c/PC-14b** desbordada = `UNKNOWN` + `CODE_SUPPORTED` + `FAIL_CLOSED`, simple y compuesto, por lado, con el predicado de `PushBackTarimaPlacement.FrontalRow`; CT-17, CT-20, T-M34, §2.3 y E6 actualizados. La lateral sigue fuera | CLOSED BY V4 | §7.3, §7.4 |
| `[V4-D12]` | AR3-04 · V4-04 | Todo `ExtensionData` del sobre y del envoltorio era portador | **Allowlist cerrada de portadores** (CA-1..CA-6). Clave desconocida no vacia de `ExtensionData` de `RackEmbedDocument` o de `RackProjectDocument` fuera de la allowlist ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`; duplicado ambiguo de `CustomProperties` ⇒ `UNKNOWN` | CLOSED BY V4 | §6.4, §7.8 |
| `[V4-D13]` | AR3-05 · V4-05 | Censo manual de «anidados relevantes» | **Cierre transitivo por reflexion** desde siete raices, con recorrido de propiedades, elementos y diccionarios, lista de corte razonada, instantanea de enums y clave `(kind, ruta)`; la guarda es la autoridad y la tabla humana es ilustrativa | CLOSED BY V4 | §6.7 |
| `[V4-D14]` | AR3-05 · AR3-12 · V4-05 | — | Coexistencia con las guardas C-08 y C-10 de I-53: I-52 **no** crea tipos en `RackCad.Application.Systems.Shared`; sus tipos viven en `RackCad.Application.Mirror` o junto a su kind; todo tipo persistido de cabecera de I-53 queda en RED hasta clasificarse | CLOSED BY V4 | §6.7, G-M14, CT-41 |
| `[V4-D15]` | AR3-06 · V4-06 | «Push Back con tope posterior activo» como fail-closed adicional, sin decir que es el estado por defecto | Declarado: el tope posterior es **ACTIVE BY DEFAULT**; Push Back solo pasa con `(NINGUNO)` o sin ninguna celda con `Draws` en cada lado. §2.1, §2.3, P-04/P-05/PC-08, R-11, O-1, M-12 y consecuencias del ADR | CLOSED BY V4 | §1.2, §2, §7.3, §7.4, §13, §17 |
| `[V4-D16]` | AR3-07 · V4-07 | ST-3 y ST-5..ST-10 sin precedencia: un bloque cualquiera podia abortar la operacion | Precedencia ejecutable C1..C5: primero se reconoce el candidato RackCad; lo claramente no RackCad se ignora con aviso; las restricciones estrictas solo se aplican a candidatos; bloque de usuario con racks anidados ⇒ aviso especifico sin expandir ni abortar | CLOSED BY V4 | §4.1 |
| `[V4-D17]` | AR3-08 · V4-08 | «version del registro» y «VariableId referenciado» | Read-set por **observaciones reales** (acreditacion usada, variables leidas, definicion observable, dependencias transitivas de I-49 si integra, observaciones adicionales); sin huella del registro completo ni «version» sin autoridad | CLOSED BY V4 | §9.2 |
| `[V4-D18]` | AR3-09 · V4-09 | C2-2 solo cubria Actualizar | C-2 cubre **Actualizar (C2-2a) e Insertar (C2-2b)**; con costura compartida, fixture comun y guarda G-M9 que demuestra que ambos la llaman; si no, prueba propia (T-M17b) | CLOSED BY V4 | §8.5 |
| `[V4-D19]` | AR3-10 · V4-10 | Sin regla de cierre de `G3_PENDING` | Al cerrar G3 ninguna fila queda `G3_PENDING`: `G3_VERIFIED`, `UNKNOWN` + `FAIL_CLOSED` o `REQUIRES_MODEL_CHANGE` + `FAIL_CLOSED`; filas condicionales partidas; cambio material de alcance, ADR, regla de reflexion o arquitectura ⇒ **Proposal V5** | CLOSED BY V4 | §7.0, §14, T-M47 |
| `[V4-D20]` | AR3-11 · V4-11 | Gobernante descrito como «si el claro maestro no tiene niveles» | Autoridad real: el gobernante cambia cuando `W_M(i,R) ≤ 0`; `Dep(i)` se mantiene conservador y se demuestra sound; CT-28 actualizada | CLOSED BY V4 | §6.5.1 |
| `[V4-D21]` | AR3-12 · V4-12 | Paralelas de V3 (`1ed93a0`, `5efaf7e`, `5d25da8`) | HEAD real (segundo fetch, antes de publicar): I-49 `364d6c0` (Consensus Freeze; ADR-0038 **aceptado** por el Owner en `edafade`; implementacion no iniciada), I-53 `7de424e` (G4 Selectivo: produccion aditiva, 0 lineas borradas, sin UI ni Plugin), I-54 `26ca923` (Proposal V5: solo C-F3; D-21 sin cambio). Primer fetch: `2beec61`, `4e00a27`, `8bc991c` | CLOSED BY V4 | §15 |
| `[V4-D22]` | V4-13 | ADR-0036 y registro con el estado de V3 | ADR-0036 actualizado (decisiones 6, 7, 8, 10, 13, consecuencias; sigue `propuesto`); registro con Architect Review V3, AR3-01..AR3-12, V4 obligatoria, precisiones del Coordinador, paralelas y estado | CLOSED BY V4 | §16, ADR, registro |
| `[V4-D23]` | Coordinador (este gate) | Secuencia de V3 con «Proposal V4» ante contradiccion | Consenso V4 → rebase → freeze + re-check → O-1 → G3 → (Proposal V5 si G3 contradice materialmente V4; si no, ADR-0036) → G4 | CLOSED BY V4 | §14.1 |
| `[V4-D24]` | Precision al redactar | — | I-54 V4 y V5 declaran residuales preexistentes del store del sobre: **F-14a** (la lectura lanza `ArgumentException`) ⇒ E2; **F-14b** (la reescritura lanza `JsonException`) y su regla F de `Compose` ⇒ E8 en el ensayo de P10, sin mutacion | CLOSED BY V4 | §4.1, §9.4, §10.4 |
| `[V4-D25]` | Precision al redactar | — | Ranuras de payload conocidas e inactivas del envoltorio: el store no las reescribe; normalizacion declarada (`BASELINE_LIMITED`), no metadata desconocida | CLOSED BY V4 | §6.2, X-03c |
| `[V4-D26]` | Precision al redactar | «carpeta nueva del espejo» sin ubicacion | C-10 de I-53 prohibe en **todo** `RackCad.Application.Systems.Shared` nombres con `Coverage`, `Executor`, `Engine`...: la carpeta del espejo es `src/RackCad.Application/Mirror/` | CLOSED BY V4 | §6.7, §20.1 |
| `[V4-D27]` | Precision al redactar | — | En `main` no hay cota autoritativa del valor efectivo de una propiedad de longitud: el store solo exige finito; `> 0` de RACKVARIABLES y `< 0` del editor Selectivo son de UI | CLOSED BY V4 | §6.5.5 |
| `[V4-D28]` | Precision al redactar | «biblioteca registrada» como evidencia | La declaracion se liga a la **definicion real**: `DefinitionGeometryHash` exigido para las definiciones que el espejo dibuja (PREFLIGHT si ya estan en el dibujo; PREPARE tras importar); las vistas no seleccionadas quedan bajo la limitacion general de §9.5 | CLOSED BY V4 (mecanismo); huella: DEFERRED TO G3/G5 WITH FAIL-CLOSED | §11.3 punto 7 |
| `[V4-D29]` | Precision al redactar (paralela) | — | Observada en `2beec61`: el indice de ADR de I-49 listaba 0038 como `aceptado` mientras el ADR decia `propuesto`. **Resuelta** en `edafade`, donde el Owner acepto ADR-0038. Solo registro; no cambia la numeracion | CLOSED BY V4 (registro) | §15.1, §16 |
| `[V4-D30]` | Precision al redactar | P-04b «escalares sin efecto grafico; sin cambio» | Con el tope inactivo por `(NINGUNO)` la mascara `OffCells` sigue persistida y **vuelve a regir** si el usuario elige una pieza en la copia: se remapea `f → N−1−f` por lado (P-05b, PC-08b) | CLOSED BY V4 | §7.3, §7.4 |

### 0.3 Resumen

```text
OPEN MATERIAL = NONE
```

Diferido con fail-closed inequivoco (no condiciona la arquitectura):

| Asunto | Fail-closed mientras tanto | Donde se resuelve |
|---|---|---|
| Semantica de `MountingFace` (S-10, D-03b) | sin confirmacion de CT-12 ⇒ FAIL_CLOSED | G3 |
| Filas `R` con `EvidenceStatus = G3_PENDING` | V-VIEW-ALL por rack; regla de cierre de G3 (§7.0); contradiccion material ⇒ Proposal V5 | G3 |
| `GeometricEvidence` por bloque y huella de definicion | sin evidencia aplicable ⇒ X-17 FAIL_CLOSED | G3 (CT-36) / G9 (M-15) |
| Relacion afin derivada de la parametrizacion (§11.3 punto 6b) | sin ella la declaracion cubre solo estados medidos | G3 (CT-36) |
| Estabilidad 9b de DEP-05..DEP-07 y cierres de descriptores | contraejemplo ⇒ STOP → Proposal V5 | G3 (CT-37, CT-38) |
| Cota autoritativa de propiedades vinculables | sin cota ⇒ solo overrides en `DEP-ROW` | si I-49 integra (§15.3) |
| Predicado exacto de «tope activo» (Selectivo y Push Back por lado) | cualquier celda dibujable ⇒ FAIL_CLOSED | G3 (CT-15) |
| Ids de placa Cantilever en BOM (K-09) | V-BOM distinto ⇒ FAIL_CLOSED | por rack |
| Linea base exacta de cada store | V-RT-STORE ⇒ FAIL_CLOSED | G3 (CT-35) |
| Valor de la tolerancia de escala | sin valor no hay implementacion (G4) | G4 |
| Semantica de `IsStandard` (H-06) | Auto con baseline ⇒ FAIL_CLOSED | G5 |
| Efectos reales de importacion y UNDO | UNKNOWN declarado; nada se promete | G9 (Owner) |
| Censo de parametros con lado (CT-25) | parametro sin regla ⇒ FAIL_CLOSED | G3 |
| Declaraciones de invariancia de metadata | ninguna ⇒ FAIL_CLOSED | futuro |
| Costuras de Insertar por kind (CT-26) | sin C2-2b verde G7 no arranca | G3 / G6 |

### 0.4 Estado de las marcas de V3 bajo V4

| Marca V3 | Estado en V4 |
|---|---|
| `[V3-D01]` | modificada por `[V4-D06]`..`[V4-D10]` (9b, V-DEP, S-15d/S-17d, PV-7) |
| `[V3-D02]` | vigente (S-14c); incluida en V-DEP como DEP-02 |
| `[V3-D03]` | vigente en la clasificacion; alcance modificado por `[V4-D15]` y mascara latente por `[V4-D30]` |
| `[V3-D04]` | vigente; su validez para estados futuros pasa por V-DEP (`[V4-D07]`) |
| `[V3-D05]` | modificada por `[V4-D08]` y `[V4-D11]` |
| `[V3-D06]` | modificada por `[V4-D12]` |
| `[V3-D07]` | modificada por `[V4-D13]` y `[V4-D14]` |
| `[V3-D08]` | modificada por `[V4-D18]` |
| `[V3-D09]` | modificada por `[V4-D01]`..`[V4-D05]` y `[V4-D28]` |
| `[V3-D10]` | vigente |
| `[V3-D11]` | vigente; su precedencia la fija `[V4-D16]` |
| `[V3-D12]` | vigente |
| `[V3-D13]` | vigente (P1..P10), con P1 reescrito por `[V4-D16]` |
| `[V3-D14]`, `[V3-D15]` | vigentes |
| `[V3-D16]` | sustituida por `[V4-D17]` |
| `[V3-D17]`..`[V3-D20]` | vigentes |
| `[V3-D21]` | sustituida por `[V4-D04]` |
| `[V3-D22]` | vigente; cierre de G3 por `[V4-D19]` |
| `[V3-D23]`, `[V3-D24]` | historial |
| `[V3-D25]` | vigente con `[V4-D19]` y `[V4-D23]` (Proposal V5) |
| `[V3-D26]` | vigente en tiempos; contenido de O-1 por `[V4-D15]` |
| `[V3-D27]`, `[V3-D29]` | vigentes |
| `[V3-D28]` | sustituida por `[V4-D21]` |
| `[V3-D30]` | vigente; se añade `[V4-D25]` |
| `[V3-D31]` | modificada por `[V4-D07]` (el cierre del descriptor alimenta V-DEP) |

### 0.5 Alias historicos de V2 (solo reconciliacion)

| Alias V2 | Equivalente (V3 y V4) |
|---|---|
| `R-s` | `RepresentabilityClass = REPRESENTABLE` + `EvidenceStatus = G3_PENDING` + `OperationalDisposition = ALLOW` (sujeto a V-VIEW-ALL y V-DEP) |
| `C/F` | fila de seccion: `REPRESENTABLE` + `CODE_SUPPORTED` + `CANONICALIZE` (legado inequivoco) o `FAIL_CLOSED` (resto) |
| `LB` | `REPRESENTABLE` + `CODE_SUPPORTED` + `BASELINE_LIMITED` (normalizacion declarada del store); la metadata semantica desconocida ya no es `LB`, es `UNKNOWN` |
| `FC` | precondicion operativa con `FAIL_CLOSED`; no es clase de representabilidad (§7.9) |

### 0.6 Cobertura explicita

| Hallazgo, cambio o precision | Marca |
|---|---|
| AR3-01 | `[V4-D01]`..`[V4-D05]`, `[V4-D28]` |
| AR3-02 | `[V4-D06]`..`[V4-D10]`, `[V4-D27]` |
| AR3-03 | `[V4-D11]` |
| AR3-04 | `[V4-D12]`, `[V4-D25]` |
| AR3-05 | `[V4-D13]`, `[V4-D14]` |
| AR3-06 | `[V4-D15]`, `[V4-D30]` |
| AR3-07 | `[V4-D16]`, `[V4-D24]` |
| AR3-08 | `[V4-D17]` |
| AR3-09 | `[V4-D18]` |
| AR3-10 | `[V4-D19]` |
| AR3-11 | `[V4-D20]` |
| AR3-12 | `[V4-D21]`, `[V4-D14]`, `[V4-D26]`, `[V4-D29]` |
| V4-01..V4-12 de la revision | `[V4-D01]`..`[V4-D21]` (en el orden de los hallazgos) |
| V4-13 de la revision | `[V4-D22]` |
| Precisiones vinculantes del Coordinador de este gate | separacion de evidencias y prohibicion de circularidad (`[V4-D01]`, `[V4-D02]`); 9b y V-DEP (`[V4-D06]`, `[V4-D07]`); `W_min` solo con cota autoritativa (`[V4-D08]`, `[V4-D27]`); allowlist de portadores (`[V4-D12]`); cierre transitivo, enums y guarda cruzada de I-53 (`[V4-D13]`, `[V4-D14]`); O-1 con hechos explicitos (`[V4-D15]`); precedencia de fuente y contenedores (`[V4-D16]`); read-set sin «version» (`[V4-D17]`); C-2 con Insertar (`[V4-D18]`); cierre de G3 con V5 (`[V4-D19]`); proceso (`[V4-D23]`) |

---

## 1. Decisiones de producto

### 1.1 Decisiones del Coordinador vigentes (desde V1)

Registro: `docs/automation/decisions/I-52.md` §4. Vinculan V4; **no** son consenso final.

| # | Decision | Precision V4 |
|---|---|---|
| **PDC-1** | **Solo copia.** Crea copia reflejada, conserva **todos** los originales, nunca borra, nunca refleja en sitio, nunca conserva el `RackId` en la copia. **Sin** pregunta «¿Borrar objetos originales?» | — |
| **PDC-2** | Nombre logico por rack `<base> - espejo`, **sin ordinal**. `Name` ≠ `RackId`. Los nombres de `BlockTableRecord` siguen la politica de unicidad existente de cada familia de plan | La autoridad de plan recibe el nombre logico (§8.1) |
| **PDC-3** | Autoridad = referencias **fisicamente seleccionadas**. Sin buscar hermanas; sin crear vistas no seleccionadas. Vistas seleccionadas del mismo `RackId` = **un** grupo con **un** `NewRackId`. Una referencia no admisible **falla toda la operacion** antes de MUTATE | V-VIEW-ALL verifica todas las vistas admisibles del diseño reflejado **sin** buscar ni crear hermanas (§6.6) |
| **PDC-4** | Sin prompt de eje semantico. El usuario solo da la linea (dos puntos) | — |
| **PDC-5** | Indices, numeros y letras derivados se **regeneran** desde el documento reflejado | — |
| **PDC-6** | Protector lateral Left/Right: **UNKNOWN** donde sea ambiguo; estado explicito cuya reflexion no este demostrada ⇒ fail-closed | — |
| **PDC-7** | Clases `REPRESENTABLE`, `REPRESENTABLE_BY_NORMALIZATION`, `REQUIRES_MODEL_CHANGE`, `UNKNOWN`, evaluadas **por rack concreto** en PREFLIGHT; `REQUIRES_MODEL_CHANGE` y `UNKNOWN` material ⇒ fail-closed | Tres dimensiones separadas (§7.0) |
| **PDC-8** | Unico comando nuevo `RACKMIRROR`, **sin alias**. Censo de `[CommandMethod(` **33 → 34** | — |
| **PDC-9** | Solo Model Space; MINSERT ⇒ fail-closed; linea por dos puntos UCS→WCS; fuente canonica | `Origin = 0` (ST-14), `View`/`Section` decodificables (ST-15), bloques dinamicos, anonimos o anotativos ⇒ fail-closed (ST-17); las restricciones estrictas solo se aplican a candidatos RackCad (precedencia C1..C5, §4.1) |
| **PDC-10** | Arquitectura para los seis kinds con bloque; el primer corte solo ejecuta vistas que exponen `μ_k`; la cama falla cerrado; sin ampliar schema | Aceptacion de alcance del Owner (O-1) despues del freeze tecnico y antes de G3, con los hechos explicitos de §1.2 |

### 1.2 Decisiones del Owner

| # | Decision | Contrato vigente | Momento |
|---|---|---|---|
| **O-1** | **Alcance del primer corte**, decidido con estos hechos **explicitos**: (1) laterales de rack fuera; (2) cama fuera; (3) Dinamico materializado solo como lateral fuera; (4) topes del Selectivo fail-closed; (5) el **tope posterior de Push Back esta activo por defecto**, asi que **la mayoria de los Push Back** fallan cerrado salvo `(NINGUNO)` o ninguna celda con `Draws` en cada lado; (6) tarimas o parrillas cuyo ajuste depende de una propiedad vinculada sin prueba para todo `R` fallan cerrado (S-15d, S-17d) y, sin cota autoritativa en `main`, solo las salvan overrides que cubran la fila; (7) tarimas frontales de Push Back desbordadas fallan cerrado; (8) metadata exterior desconocida no vacia falla cerrado; (9) diferencia de mano sin `GeometricEvidence` aplicable falla cerrado (§11.3); (10) cabeceras de plantilla con Auto y baseline (H-06) y demas filas `UNKNOWN`/`REQUIRES_MODEL_CHANGE` de §2.3 y §7 | recorte de §2 | **pendiente**; se pide **despues del freeze tecnico** y **antes de G3**; **no** se pide en este gate |
| **O-2** | **Topes del Selectivo** | `UNKNOWN → FAIL_CLOSED`. **No** necesita decision ahora; una relajacion vuelve al Coordinador y al Owner | — |
| **O-2B** | **Tope posterior de Push Back** (activo por defecto) | `UNKNOWN → FAIL_CLOSED`. **No** necesita decision ahora; una relajacion vuelve al Coordinador y al Owner | — |
| **O-3** | **Evidencia de simetrias** | ningun bloque se asume simetrico; la prueba automatica es `GeometricEvidence` independiente (§11.3); la confirmacion del Owner **nunca** es la unica prueba | G3 (CT-36 y despues CT-06) y G9 (M-15) |
| **O-4** | **ADR-0036** | `propuesto` | se pide **despues de G3** si G3 no contradice materialmente V4; si la contradice ⇒ Proposal V5; **precondicion de G4** |

---

## 2. Alcance del primer corte

### 2.1 Dentro

| Kind (`Kind` del sobre) | `μ_k` canonica | Vistas admitidas | Evidencia de la vista |
|---|---|---|---|
| Selectivo (`selective`) | **RUN**: invierte frentes/postes | frontal (`Section` = fondo), planta | frontal X = run (`SelectivePostGeometry.cs:32-40`); planta X = fondo, Y = frente (`SelectivePlantaBuilder.cs:15-22`) |
| Dinamico (`dynamic`) | **RT**: invierte frentes | frontal (`Section` 0 salida / 1 entrada), planta | `RackDinamicoCommands.cs:392-393`; `DynamicSystemFrontalBuilder`/`DynamicSystemPlantaBuilder` |
| Push Back simple y compuesto (`pushback`) | **RT** | frontal (`Section` 0..3), planta — **solo** con el tope posterior inactivo en cada lado (P-04b); el tope esta **activo por defecto** (§2.3) | `PushBackSystemFrontalBuilder.cs:137-155`; `PushBackRearTope.cs:9`, `:53` |
| Cantilever (`cantilever`) | **R_X**: invierte la linea de estaciones | frontal, planta | camaras `CantileverViewPlanBuilder.cs:205-235` |
| Cabecera independiente (`cabecera`) | **R_D**: invierte la profundidad del marco | lateral, planta | `LateralHeaderLayoutBuilder.cs:49-59`; `PlantaHeaderLayoutBuilder.cs:13-21` |

### 2.2 Fuera (fail-closed o no aplica)

- Laterales de Selectivo, Dinamico, Push Back y Cantilever; la cama (`cama`) entera.
- Diseños con alguna fila `REQUIRES_MODEL_CHANGE` o `UNKNOWN` material (§7).
- Decisiones `ALLOW`/`CANONICALIZE` cuya estabilidad para todo estado acreditable del registro no puede demostrarse
  (obligacion 9b, §6.5.5).
- Metadata exterior (sobre o envoltorio) desconocida y no vacia fuera de la allowlist de portadores (§6.4).
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
| **Push Back con tope posterior activo — ES EL ESTADO POR DEFECTO** | `PushBackRearTopeConfig` esta activo por defecto (`PushBackRearTope.cs:9`): solo se guardan desactivaciones (`OffCells`, `:41`), `Draws = !IsNone ∧ At` (`:53`) y un `PieceId` en blanco o desconocido usa `LARGUERO_ESCALON_TOPE_DE_3` (`PushBackRearTopeBuilder.cs:27`, `:37-48`); el lado B tiene su propia configuracion, tambien activa (`PushBackSideDesign.cs:48`). Mismo patron de AR-01 demostrado en codigo (`[V3-D03]`). **Afecta a la mayoria de los Push Back existentes y nuevos**: solo pasan con `(NINGUNO)` o sin ninguna celda con `Draws` en **cada** lado | P-04, P-05, PC-08 (admitido solo P-04b / PC-08b) |
| **Medio frente cuyo `BeamLength` efectivo depende de una propiedad vinculada** | la normalizacion congelaria el remanente (`[V3-D01]`) | S-02b |
| **Desviador con off-cells cuyo espacio de indices depende de ese ajuste** | remapeo inestable ante `ChangeValue` | S-14c |
| Parrilla con una fila desbordada (`count·frente > span`) | reparto apilado desde la izquierda | S-15x |
| **Parrilla con conteo por defecto cuyo ajuste depende de una propiedad vinculada sin `DEP-ROW`** | un `ChangeValue` posterior puede desbordarla (casos A y B, obligacion 9b) | S-15d |
| **Tarimas con una fila de claro completo desbordada** | mismo reparto apilado (`[V3-D05]`) | S-17c |
| **Tarimas cuyo ajuste depende de una propiedad vinculada sin `DEP-ROW`** | obligacion 9b (`[V4-D08]`) | S-17d |
| **Tarimas frontales de Push Back desbordadas** (simple o compuesto, por lado) | `FrontalRow` recorta el margen a 0 y ancla la fila al extremo izquierdo (`PushBackTarimaPlacement.cs:224`) | P-03c, PC-14b |
| Cabecera con `AutoAlternating` en un panel con diagonal y `StandardBaselineId` no vacio | semantica de «estandar» sin caracterizar; las plantillas crean exactamente eso (`RackFrameConfigurationFactory.cs:85`, `:226-243`) | H-06 |
| **Metadata semantica desconocida no vacia en el payload** | no hay transformador (`[V3-D06]`) | V-META; S-24, D-18, P-11, K-15, H-09 |
| **Metadata exterior desconocida no vacia** (sobre o envoltorio) fuera de la allowlist de portadores | un build futuro puede poner alli semantica de vista, lado u orientacion (`[V4-D12]`) | V-META; X-02b, X-03b |
| **Miembro `UNSUPPORTED` no vacio segun la guarda de cobertura** | sin regla de espejo (`[V4-D13]`) | X-16 |
| **Diferencia de mano sin declaracion de simetria aplicable** | sin `GeometricEvidence`, fuera del dominio caracterizado o definicion distinta (`[V4-D01]`, `[V4-D28]`) | X-17 |
| **Decision `ALLOW`/`CANONICALIZE` sin prueba de estabilidad para todo `R`** | obligacion 9b (`[V4-D06]`) | X-18 |
| **Alguna vista admisible del rack (seleccionada o no) que no conmuta** | V-VIEW-ALL (`[V3-D04]`) | X-15 |
| Definicion fuente con `Origin ≠ 0` | BASE/BEDIT no soportado | ST-14 |
| **Referencia a bloque dinamico, anonimo o anotativo con payload RackCad** | transformacion adicional no capturada | ST-17 |
| `View` o `Section` no canonizable, incluida la `View` vacia del Selectivo con `Section ≥ 0` | §3.7 | ST-15 |
| **Sobre que no se puede leer o reserializar** | residuales preexistentes F-14a y F-14b del store del sobre (I-54 V4 y V5) | ST-2c/E2, E8 |
| Bloques de biblioteca ausentes, o huella de una definicion declarada simetrica distinta, tras PREPARE | la importacion es best-effort (§9.5) y la declaracion se liga a la definicion real (§11.3) | E10, E11 |
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
6. Una decision `ALLOW` o `CANONICALIZE` cuyo **predicado** dependa directa o transitivamente de una propiedad vinculable
   rompe tambien el «para todo R» si `R` puede volverlo falso: obligacion 9b (§6.5.5).

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
  legibles. Cambio de modelo con ADR propio. Es la via para laterales, la vista primaria del Dinamico y la cama. Si se
  adopta, el miembro nuevo del sobre queda bloqueado por V-META exterior (§6.4) en todo build que no lo clasifique.
- **ALT-μ2 — segunda reflexion por kind** (intercambio A↔B en la lateral de Push Back compuesto; R_Y en la lateral de
  Cantilever de estacion sencilla): contraria a PDC-4. Registrada como futuro; no se implementa en I-52.
- **ALT-ABS — medio frente que absorbe en el primer tramo:** permitiria reflejar un medio frente cuyo ancho depende de
  una variable. Es cambio de modelo (el modelo actual siempre absorbe en el ultimo tramo); futuro.

---

## 4. Contrato de fuente, linea y tolerancias

### 4.1 Fuente (por objeto seleccionado, en SNAPSHOT/PREFLIGHT) — precedencia ejecutable

Fuente admitida = **candidato RackCad** (payload RackCad en su BTR o en su `DynamicBlockTableRecord`) que es
`BlockReference` de Model Space, no MINSERT, no dinamica, BTR no anonimo y no anotativo, `Normal = +Z`, rotacion finita,
escala uniforme, definicion con `Origin = 0` y `View`/`Section` decodificables.

**Principio (V4-07).** Primero se reconoce si el objeto es candidato RackCad. Un objeto que claramente no lo es se ignora
con aviso y **nunca** aborta la operacion, cualquiera sea su escala, rotacion, `Normal`, `Origin` o tipo (MINSERT
incluido). Las restricciones estrictas solo se evaluan sobre candidatos RackCad.

```text
Para cada objeto seleccionado o, en este orden (la primera regla que decide, decide):
  C1  ST-1   o no es BlockReference                                                    → IGNORAR + aviso
  C2  clasificacion de payload: RackBlockData en BTR(o) y en DynamicBlockTableRecord(o), solo lectura
        ST-2   ninguno lleva payload RackCad                                           → IGNORAR + aviso
        ST-2b  ademas, BTR(o) contiene referencias DIRECTAS con payload RackCad          → IGNORAR + aviso especifico
               («bloque de usuario con racks anidados: no se refleja»); no se expande, no se trata como seleccion
               fisica y no aborta
        ST-2c  payload presente pero ilegible o inutilizable                           → E2 (toda la operacion)
      ⇒ en otro caso, o es CANDIDATO RackCad
  C3  ST-4   candidato fuera de Model Space                                             → IGNORAR + aviso (PD-7 de I-51)
  C4  restricciones estrictas del candidato, en este orden; la primera que falla ⇒ E4 de TODA la operacion:
        ST-3 MINSERT · ST-17 dinamica, anonima o anotativa · ST-5 Normal · ST-6 rotacion · ST-10 sz < 0 ·
        ST-9 una sola escala negativa · ST-7 escala no uniforme · ST-14 Origin · ST-15 View/Section
  C5  propiedades de la fuente admitida: ST-8 (canonizacion), ST-11, ST-12, ST-13, ST-16
Si todos los objetos quedan ignorados en C1..C3 ⇒ E1 (fin, sin linea).
```

| # | Orden | Condicion | Resultado |
|---|---|---|---|
| ST-1 | C1 | Objeto no `BlockReference` | ignorar + aviso (clasificacion de I-51) |
| ST-2 | C2 | `BlockReference` sin payload RackCad **ni** en su BTR **ni** en su `DynamicBlockTableRecord` | ignorar + aviso; nunca aborta, sea cual sea su escala, rotacion, `Normal`, `Origin` o tipo |
| ST-2b | C2 | Ademas, su BTR contiene referencias **directas** con payload RackCad (deteccion de un nivel, sin recursion y en solo lectura) | ignorar + aviso especifico; no se expande; anidamiento mas profundo o un XREF no resuelto dan el aviso de ST-2 |
| ST-2c | C2 | Payload RackCad presente pero inutilizable: sin `Kind`, sin diseño, JSON ilegible, o `ArgumentException` del store del sobre por UTF-16 crudo invalido (F-14a de I-54) | **E2**, fail-closed de toda la operacion (una fuente RackCad ilegible nunca desaparece; precedente de I-51, `RackDuplicationPlan.cs:394-428`) |
| ST-4 | C3 | Candidato fuera de Model Space (`OwnerId` ≠ Model Space) | ignorar + aviso (PD-7 de I-51) |
| ST-3 | C4 | Candidato `MInsertBlock` (subclase de `BlockReference`) | **fail-closed** de la operacion |
| **ST-17** | C4 | Candidato con `reference.IsDynamicBlock`, `BlockTableRecord.IsAnonymous` o definicion anotativa (`BlockTableRecord.Annotative`) | **fail-closed explicito (E4)**, con mensaje propio; nunca cae en ST-2 como «no rack» |
| ST-5 | C4 | Angulo entre `Normal` y +Z mayor que `GeometryTolerance.Angle` | **fail-closed** |
| ST-6 | C4 | `Rotation` no finita | **fail-closed** |
| ST-10 | C4 | `sz < 0` | **fail-closed** |
| ST-9 | C4 | Exactamente una de `sx`, `sy` negativa (MIRROR nativo, semilla espejada de RACKLAYOUT o RACKDUPLICAR) | **fail-closed** |
| ST-7 | C4 | Valores absolutos de `sx`, `sy` y `sz` distintos segun la tolerancia de escala (§4.3; valor en G4) | **fail-closed** |
| ST-14 | C4 | `BlockTableRecord.Origin` de la definicion fuente con alguna componente de valor absoluto mayor que `GeometryTolerance.Length` | **fail-closed** (BASE/BEDIT movido no se soporta; `RackCloner.cs:34` copia el origen de la fuente) |
| ST-15 | C4 | `View`/`Section` que no decodifican segun §3.7 | **fail-closed**; las decodificables dan `σ'` canonica |
| ST-8 | C5 | `sx < 0` y `sy < 0` con `sz > 0` | se canoniza a `(s,s,s)` con `θ + π` (exacto, §3.3) |
| ST-11 | C5 | Escala uniforme positiva `s ≠ 1` | admitida; `P'` conserva `s` |
| ST-12 | C5 | Z de `Position` | se conserva (el plano de reflexion es vertical) |
| ST-13 | C5 | Capa | la referencia nueva usa la capa de **su** referencia fuente (como I-51) |
| ST-16 | C5 | Huella de la referencia y read-set del registro (§9.2) | se capturan en SNAPSHOT y se re-verifican en PREPARE y en MUTATE |

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
(`Vector2D.cs:82-89`). V4 no inventa ninguna tolerancia relativa.

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

Editar `RackDuplicationPlan.cs` **es** tocar RACKDUPLICAR: antes de editarlo se coordina con I-49 (obligatorio antes de G4), cuya condicion de parada
cubre sus propios gates (I-49 V6 §12.2) y que ya registra el cruce (I-49 V6 §12.3); rigen las guardas en RED primero,
Core completo, la regresion de I-51 y M-17 en el Candidato. Si G4 no puede demostrar la equivalencia de la fachada:
**STOP → Proposal V5**. No se duplica la agrupacion en un segundo planificador. Las guardas G-R1..G-R6 leen el codigo de
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
| RF-8 Normalizaciones | Declaradas, reportadas y validas solo con las obligaciones 1..9 de §6.5; toda decision `ALLOW`/`CANONICALIZE` cumple ademas la obligacion 9b (§6.5.5) |
| RF-9 Fidelidad del store | V-RT-STORE (§6.3); nunca se justifica con el camino `EditX` |
| RF-10 Metadata | V-META (§6.4): miembro desconocido no vacio en el payload semantico, o clave exterior (sobre o envoltorio) desconocida no vacia fuera de la allowlist ⇒ `UNKNOWN` |
| RF-11 Cobertura | Cada clave `(kind, ruta)` alcanzada por el cierre transitivo de §6.7 tiene exactamente una clasificacion |
| RF-12 Dependencias | Ninguna regla ni normalizacion escribe en el documento un valor calculado a partir del registro (§3.2, consecuencia 5; §6.5, obligacion 9), y ninguna decision `ALLOW`/`CANONICALIZE` depende de un predicado que `R` pueda volver falso (§3.2, consecuencia 6; §6.5.5, obligacion 9b) |
| RF-13 Declaracion V-DEP | Cada fila de §7 con `ALLOW` o `CANONICALIZE` declara sus entradas efectivas; si alguna esta en el cierre de un descriptor vinculable, tiene entrada V-DEP (§6.5.5; T-M43) |
| RF-14 Ubicacion | Los tipos nuevos del espejo viven en `RackCad.Application.Mirror` o junto a su kind; **ninguno** en `RackCad.Application.Systems.Shared` (§6.7; G-M14) |

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

**Ranuras de payload inactivas del envoltorio (`[V4-D25]`).** `RackProjectStore.Serialize` escribe solo la ranura del
kind (`RackProjectStore.cs:50-61`) y la preservacion de desconocidos nunca resucita una ranura conocida inactiva
(`:64-82`): una ranura conocida inactiva no vacia se pierde tambien en `RACKEDITAR`. Es normalizacion declarada del
store (X-03c, `BASELINE_LIMITED`), no metadata desconocida.

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

### 6.4 Portadores y metadata semantica (V-META del payload y del exterior)

**Tres categorias.**

| Categoria | Definicion | Tratamiento |
|---|---|---|
| **Portador** (`carrier metadata`) | Miembro o clave **nombrado en la allowlist** de esta seccion, cuyo contrato es conservarse exactamente, sin interpretacion por orientacion | se copia desde el origen; nunca bloquea por si mismo |
| **Metadata semantica authored** | Todo miembro del **payload semantico** que se somete a `μ_k` | se clasifica (§6.7) y se transforma segun su regla; **desconocida y no vacia ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`** |
| **Metadata exterior** | Miembros declarados y claves de `ExtensionData` de `RackEmbedDocument` (sobre) y de `RackProjectDocument` (envoltorio) | cada miembro declarado tiene regla propia; una clave desconocida no vacia que no figure en la allowlist ⇒ **`UNKNOWN` ⇒ `FAIL_CLOSED`** |

**Ningun contenedor es portador por ser contenedor (V4-04).** V3 eximia todo el `ExtensionData` del sobre y del
envoltorio; V4 lo retira. El sobre ya lleva semantica de vista (`View`, `RackEmbedDocument.cs:41`; `Section`, `:47`),
`Compose` copia su `ExtensionData` sin interpretarlo (`RackEmbedComposer.cs:33`) y ALT-L (§3.9) preve precisamente una
propiedad de vista en el sobre. Un build futuro puede introducir alli semantica de vista, lado, orientacion, referencia o
relaciones que un build de I-52 copiaria sin voltear.

**Allowlist de portadores (autoridad; cerrada).**

| # | Miembro o clave | Donde | Contrato | Evidencia |
|---|---|---|---|---|
| CA-1 | `SchemaVersion` del sobre | `RackEmbedDocument` | version de escritura no degradada | `RackEmbedComposer.cs:26`; `RackEmbedDocument.cs:35` |
| CA-2 | `SchemaVersion` del envoltorio | `RackProjectDocument` | no degradada via `WithSourceMetadataFrom` | `RackProjectStore.cs:81`; `RackProjectDocument.cs:18` |
| CA-3 | `PropertyValues` completo (incluidos `Kind` desconocidos, `expression` de I-49, literales congelados y `VariableId`) | `SelectivePalletDesignDocument` | autoridad de I-47/I-49: un vinculo desconocido o mal formado nunca desaparece del escaneo y hace fallar la resolucion efectiva | `SelectiveLinkedPropertyKernel.cs:103-150`; `SelectiveEffectiveDesignResolver.cs:115-160`; `SelectivePropertyValueDocument.cs:25-38` |
| CA-4 | `CustomProperties` (I-54) | miembro declarado del sobre cuando I-54 este integrada; **antes**, clave `CustomProperties` de `ExtensionData` del sobre | portador D-21 de I-54 (`JsonElement?` heredado por `Compose`) | I-54 Proposal V5 @ `26ca923` §D-21 (sin cambio desde V4) |
| CA-5 | `DimensionViews` (I-50) | DTO Selectivo; dominio Dinamico y Push Back | `int` exacto, bits desconocidos y `null` | §10.3 |
| CA-6 | Cualquier portador integrado en `main` antes del Candidato | segun su contrato | **solo** si se añade **por nombre** a esta tabla con evidencia; sin entrada es metadata exterior o semantica desconocida | §15 |

- **CA-4 como clave de `ExtensionData`:** la coincidencia sigue el enlace de miembros del store del sobre, sin distinguir
  mayusculas (I-54 V5 P-17(9)). **Dos o mas claves** que coinciden (por ejemplo `CustomProperties` y
  `customproperties`, I-54 V5 P-33) son ambiguas ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`.
- La allowlist del `ExtensionData` del envoltorio esta **vacia** en el primer corte.

**Miembros declarados del exterior (regla propia; no son portadores).**

| Miembro | Regla |
|---|---|
| `RackEmbedDocument.Kind` | invariante; debe coincidir con el kind del grupo (§5) |
| `RackEmbedDocument.Id`, `Name` | identidad: restamp de copia (§5.5) |
| `RackEmbedDocument.View`, `Section` | transformadas por §3.7 |
| `RackEmbedDocument.Design` | payload del kind (tabla siguiente) |
| `RackProjectDocument.Kind` | invariante del kind |
| Ranura de payload del kind (`SelectiveRack`, `DynamicSystem`, `PushBack`, `Cantilever`, `Header`) | payload semantico |
| Ranuras conocidas **inactivas** (`FlowBed`, `Larguero` y las de otros kinds) | el store no las reescribe: normalizacion declarada (§6.2; X-03c) |

**Payload semantico por kind.**

| Kind | Payload semantico (sometido a `μ_k`) | Portadores aplicables |
|---|---|---|
| selective | JSON de `SelectivePalletDesignDocument`, raiz y anidados, **excepto** el subarbol `PropertyValues` | CA-1, CA-3, CA-4, CA-5 |
| dynamic | subarbol `DynamicSystem` | CA-1, CA-2, CA-4, CA-5 |
| pushback | subarbol `PushBack` | CA-1, CA-2, CA-4, CA-5 |
| cantilever | subarbol `Cantilever`, **excepto** los miembros retirados que el store declara y descarta (`RackProjectStore.cs:374-400`) | CA-1, CA-2, CA-4 |
| cabecera | subarbol `Header` o la cabecera legada completa | CA-1, CA-2 (si hay envoltorio), CA-4 |

**Regla V-META.**

```text
Vacio(v)          ⇔ v es null, {} o []
Desconocido(X)    = miembros del payload semantico X que el modelo tipado del store no liga en esa posicion,
                    que no son retirados declarados por el store y que no son Vacio
DesconocidoExt(E) = claves de ExtensionData del sobre E y, cuando E.Design es un RackProjectDocument, de su
                    ExtensionData, que no son Vacio y no figuran en la allowlist (CA-4 con coincidencia unica)
V-META pasa ⇔ Desconocido(D) = ∅ ∧ DesconocidoExt(E) = ∅
              (salvo miembros o claves de la declaracion tipada de invariancia o de transformador; vacia en el primer corte)
V-META falla ⇒ RepresentabilityClass = UNKNOWN, OperationalDisposition = FAIL_CLOSED (E6), con la ruta del miembro o clave
```

- La deteccion compara el JSON crudo del payload y del exterior con el modelo tipado del store (CT-32); vale para miembros
  anidados aunque el store los descarte al leer.
- Un portador de la allowlist nunca bloquea por su contenido. Su **reescritura** si puede fallar por residuales
  preexistentes del store (F-14b de I-54): eso es E8 en el ensayo de P10, no V-META (§9.4).
- Una declaracion de invariancia o de transformador exige evidencia (CT y prueba) y vive en un unico registro tipado en
  `RackCad.Application.Mirror`; ninguna existe en el primer corte.
- Pruebas: CT-32 y T-M35 con clave exterior desconocida vacia y no vacia, portador de la allowlist, `CustomProperties`
  antes y despues de I-54, duplicado por mayusculas y clave desconocida del envoltorio.

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

La obligacion 9 rige las normalizaciones. Toda decision `ALLOW` o `CANONICALIZE`, sea o no normalizacion, cumple
ademas la **obligacion 9b** (§6.5.5).

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

**Ancho efectivo del claro `i` — autoridad real** (`main @ f8deb67`; V4-11):

```text
L(c,R)   = BeamLengthOverride_c                                        si BeamLengthOverride_c > 0
           Frente_c·max(1,n_c) + PalletTolerance(R)·(max(1,n_c) + 1)   en otro caso     (SelectiveGeometryResolver.cs:388-393)
BBL(b,R) = max(0, max_{c ∈ celdas(b)} L(c,R))                          (BayBeamLength, :369-386; claro sin celdas = 0)
Maestro  = el fondo con mas frentes; empate → el primero                (:140-144)
W_M(i,R) = BBL(claro i del maestro, R)
W(i,R)   = W_M(i,R)                                                    si W_M(i,R) > 0
           max_k BBL(claro i del fondo k, R)                           si W_M(i,R) ≤ 0   (:150-164: el gobernante CAMBIA)
todos los fondos adoptan W(i,R) y la viga gobernante que lo produjo    (:166-173)
```

El gobernante cambia cuando `W_M(i,R) ≤ 0`, **no solo** cuando el claro maestro no tiene niveles: tambien cuando tiene
niveles y ninguna celda aporta longitud positiva (sin override positivo y con `Frente·n + PalletTolerance·(n+1) ≤ 0`, por
ejemplo `Frente = 0` con tolerancia no positiva).

**Dependencia efectiva (analisis conservador).**

```text
Gov_M(i)   = celdas del claro i del fondo maestro
Gov_all(i) = celdas del claro i de todos los fondos
Dep(i) ⇔ existe un vinculo sano de una propiedad que alimenta L (hoy: PalletTolerance,
         SelectiveLinkedProperties.cs:208-213; efectivo en SelectiveGeometryResolver.cs:96, :258)
         ∧ ( (Gov_M(i) ≠ ∅ ∧ ∃ c ∈ Gov_M(i) sin BeamLengthOverride > 0)
           ∨ (Gov_M(i) = ∅ ∧ ∃ c ∈ Gov_all(i) sin BeamLengthOverride > 0) )
```

**Soundness respecto de la autoridad real.**

- `Gov_M(i) ≠ ∅` y todas sus celdas con override positivo: `W_M(i,R)` es el mayor override, constante y positivo; el
  gobernante nunca cambia y `W(i,R)` no depende de `R` ⇒ `¬Dep(i)` es exacto.
- `Gov_M(i) ≠ ∅` con alguna celda sin override: `W_M` puede variar con `R` y el gobernante puede pasar a `Gov_all(i)`
  cuando `W_M ≤ 0`; `Dep(i)` ya es verdadero, asi que el cambio de gobernante queda cubierto.
- `Gov_M(i) = ∅` (columna vacia): `W_M = 0` siempre y gobierna el mayor claro real; depende de `R` si y solo si alguna
  celda de `Gov_all(i)` no tiene override.
- Caso limite: `Gov_M(i) = ∅` con todas las celdas de `Gov_all(i)` con override ⇒ `W(i,R)` constante ⇒ `¬Dep(i)`.

| Condicion del claro dividido `i` | Clase | Evidencia | Disposicion |
|---|---|---|---|
| `¬Dep(i)`, postes `f` y `f+1` con mismo peralte y troquel efectivos | `REPRESENTABLE_BY_NORMALIZATION` | `G3_PENDING` | `CANONICALIZE` |
| `Dep(i)` | `REQUIRES_MODEL_CHANGE` | `CODE_SUPPORTED` | `FAIL_CLOSED` |
| postes `f` y `f+1` con peralte o troquel distintos | `REQUIRES_MODEL_CHANGE` | `CODE_SUPPORTED` | `FAIL_CLOSED` |
| `Segments ≥ 2` que hoy **no** resuelve | `UNKNOWN` | `CODE_SUPPORTED` | `FAIL_CLOSED` |

- **No** basta con «`PalletTolerance` vinculada ⇒ MC»: si todas las celdas del conjunto gobernante tienen
  `BeamLengthOverride > 0`, el ancho es realmente independiente y la fila sigue RN.
- `VerticalClearance` (la otra propiedad vinculable hoy) no alimenta `L`. Su cierre efectivo (§6.5.5) no contiene ningun
  simbolo a lo largo del eje reflejado. Toda propiedad vinculable nueva se clasifica en la guarda de cobertura (§6.7) con
  su cierre; sin clasificar, **RED / STOP**.
- **Cambio authored declarado** (fila RN): el `Length` del nuevo primer tramo pasa de derivado a explicito y el `Length`
  ignorado del ultimo tramo se reescribe.
- **CT-21** (UI, BOM, round-trip, involucion), **CT-27** (dos o mas estados del registro), **CT-28** (autoridad del ancho:
  fondo maestro, columna vacia, `W_M(i,R) ≤ 0` con niveles y cambio de gobernante) y **CT-37** (diferencial 9b).

**S-14 — desviador asociado.** El indice de `DesviadorOffCells` es el orden de los postes cargados, intermedios
incluidos (`SelectiveDesviadorPlan.cs:114-148`), y los intermedios existen solo si el medio frente resuelve. Con off-cells
y algun claro dividido con `Dep(i)`, el remapeo `c → P−1−c` puede dejar de ser valido tras un `ChangeValue` que cambie
el ajuste ⇒ **S-14c = `UNKNOWN` → `FAIL_CLOSED`** (DEP-02). En la practica S-02b ya hace fallar ese rack; S-14c se declara
igual para que la regla no dependa de esa coincidencia. **CT-29** y **T-M32** con dos estados del registro.

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

#### 6.5.5 Obligacion 9b — DYNAMIC-STABILITY y autoridad V-DEP

**Obligacion 9 frente a 9b.**

| Obligacion | Rige | Enunciado |
|---|---|---|
| **9** | normalizaciones | `N(Eff(D,R)) ≡ Eff(N(D),R)` para todo `R` acreditable (§6.5) |
| **9b — DYNAMIC-STABILITY** | toda decision `REPRESENTABLE` o `REPRESENTABLE_BY_NORMALIZATION` con disposicion `ALLOW` o `CANONICALIZE`, sea o no normalizacion | si su predicado depende **directa o transitivamente** de una propiedad vinculable, su verdad —y la conmutacion de §3.2 que autoriza— se conserva para **todo** estado acreditable futuro `R`. Si no puede probarse: `UNKNOWN` o `REQUIRES_MODEL_CHANGE` ⇒ `FAIL_CLOSED` |

```text
Acreditables(D) = estados R del registro acreditables segun PV-5 en los que Eff(D,R) esta definido
Lema de definicion: Eff(μ_k D, R) definido ⇔ Eff(D, R) definido
    (μ_k no toca PropertyValues ni valores vinculados: PV-1; lo caracteriza CT-37)
9b(d): ∀ R ∈ Acreditables(D):
         p_d(Eff(D,R)) = p_d(Eff(D,R_snapshot))
         ∧ Π_(V,σ')(Eff(μ_k D,R)) ≡ F ∘ Π_(V,σ)(Eff(D,R))   para cada (V,σ) ∈ A_k que la decision d autoriza
```

Aplica, **sin limitarse a S-02**, a: fit (cuantas piezas caben), overflow (desborde de fila), ancho gobernante, espacios de
indices (postes cargados, off-cells), cantidad o posicion que dependen de geometria efectiva y cualquier regla futura que
descubra la guarda de cobertura (§6.7) o CT-38.

**Autoridad V-DEP.** V-DEP es la autoridad explicita de estas dependencias. Cada fila de §7 con disposicion `ALLOW` o
`CANONICALIZE` declara:

| Campo | Contenido |
|---|---|
| `AuthoredInputs` | miembros authored que lee |
| `EffectiveInputs` | simbolos efectivos que lee, de un vocabulario cerrado por kind (Selectivo: `L(c)`, `BBL(b)`, `W(i)`, `Gobernante(i)`, `Remanente(i)`, `PostesCargados`, `FitFila`, `ConteoParrilla`, `PostX`, `ElevacionNivel(b,l)`) |
| `BindableClosure` | descriptores vinculables en cuyo cierre transitivo estan esas entradas |
| `Predicate` | el predicado que decide la fila |
| `StabilityProof` o `FailClosedReason` | la prueba de 9b o el motivo del fail-closed |

**Cierre efectivo de cada descriptor (autoridad del kind, derivada del resolvedor).**

| Descriptor (`main @ f8deb67`) | Cierre efectivo declarado |
|---|---|
| `selective.palletTolerance` (`SelectiveLinkedProperties.cs:208-213`) | `L(c)` → `BBL(b)` → `W(i)` y `Gobernante(i)` → largueros, `PostX`, `Remanente(i)`, `PostesCargados`, `FitFila`, `ConteoParrilla`, cotas horizontales |
| `selective.verticalClearance` | `ElevacionNivel(b,l)`, separaciones, alturas de claro y de poste; ningun simbolo a lo largo del eje reflejado |

Dinamico, Push Back, Cantilever y cabecera no tienen descriptores vinculables en `main`: sus decisiones no dependen de `R`.
Un descriptor nuevo en cualquier kind ⇒ **RED / STOP** hasta declarar su cierre (T-M36, T-M43, CT-38).

**Inventario V-DEP del primer corte (Selectivo).**

| # | Fila | Entradas efectivas | Predicado | Estabilidad 9b / disposicion |
|---|---|---|---|---|
| DEP-01 | S-02a / S-02b (N-S02) | `W(i)`, `Remanente(i)` | `Resolve(claro) ≠ null`; forma normal con `r` | `¬Dep(i)` ⇒ `W` constante ⇒ estable (RN); `Dep(i)` ⇒ `MC` `FAIL_CLOSED` (obligacion 9) |
| DEP-02 | S-14b / S-14c | `PostesCargados` | espacio de indices estable | sin claro dividido con `Dep(i)` ⇒ estable; si no ⇒ `UK` `FAIL_CLOSED` |
| DEP-03 | S-17b / S-17c / S-17d: tarimas de claro completo, incluida la fila de piso (`SelectiveGeometryResolver.cs:354`) | `W(i)`, `FitFila` | `count·frente ≤ W(i) + GeometryTolerance.Length` en toda fila | `¬Dep(i)` ⇒ `W` constante ⇒ se evalua en el SNAPSHOT (S-17b o S-17c); `Dep(i)` ⇒ `ALLOW` solo con `DEP-ROW`; si no ⇒ S-17d `UK` `FAIL_CLOSED` |
| DEP-04 | S-15 / S-15x / S-15d: parrilla de claro completo con conteo por defecto (`SelectiveParrillaPlan.cs:107`; `SelectiveFrontalBuilder.cs:310`) | `W(i)`, `FitFila` | igual que DEP-03 | igual que DEP-03 |
| DEP-05 | S-15 con `ParrillaFrente` o `ParrillaCantidad` manual | `ConteoParrilla` | nunca desborda: el conteo se recorta con `PalletFit` (`SelectiveFrontalBuilder.cs:291-311`) | estable: `D` y `μ_k D` calculan el mismo conteo desde el mismo `W(i,R)` y el reparto es simetrico para todo `R`; `G3_PENDING` con CT-37 |
| DEP-06 | S-01b, S-16, cotas horizontales | `W(i)`, `PostX`, `Gobernante(i)` | reparto simetrico entre postes | estable: la formula de postes es simetrica en los dos troqueles (`SelectivePostGeometry.cs:39`) y `μ_RUN` permuta claros con los mismos datos; `G3_PENDING` con CT-37 |
| DEP-07 | filas que leen `ElevacionNivel` | `ElevacionNivel` | ninguno a lo largo del eje reflejado | estable: la Y no se refleja (Lema 1, §3.4); `G3_PENDING` con CT-37 y CT-38 |
| DEP-08 | S-13 / S-13b (topes) | `W(i)` | — | ya `UK` `FAIL_CLOSED` (O-2) |

**`DEP-ROW` — prueba de ajuste para todo `R`.**

```text
b       = cota inferior AUTORITATIVA del valor efectivo de cada propiedad que alimenta L, si existe
          (una autoridad de resolucion o de dominio que haga fallar Eff(D,R) fuera de ella)
L_lb(c) = BeamLengthOverride_c                           si BeamLengthOverride_c > 0
          Frente_c·max(1,n_c) + b·(max(1,n_c) + 1)       si no, y existe b
          −∞                                             si no
W_lb(i) = max_{c ∈ Gov_M(i)} L_lb(c)                     si Gov_M(i) ≠ ∅
          max_{c ∈ Gov_all(i)} L_lb(c)                   si Gov_M(i) = ∅
DEP-ROW(fila en el indice i) ⇔ count·frente ≤ W_lb(i) + GeometryTolerance.Length
```

- **Soundness.** Para todo `R ∈ Acreditables(D)`, `W_M(i,R) ≥ W_lb(i)` si `Gov_M(i) ≠ ∅` (y la cota vale sobre el mayor
  claro real si `Gov_M(i) = ∅`). Toda fila dibujada tiene `count·frente > 0`, asi que `DEP-ROW` implica `W_lb(i) > 0`: el
  gobernante no cambia y la fila no desborda en ningun `R`.
- **Cota autoritativa (V4-02).** `W_lb` usa `b` **solo** si existe una cota autoritativa real. En `main @ f8deb67` **no
  existe**: el store del registro solo exige un valor finito (`ProjectVariablesStore.cs:199-201`;
  `VariableDefinition.cs:53-64`), la resolucion efectiva no aplica dominio (`SelectiveEffectiveDesignResolver.cs:115-160`)
  y los controles `> 0` de RACKVARIABLES (`RackProjectVariablesWindow.xaml.cs:217-222`) y `< 0` del editor Selectivo
  (`RackSelectiveWindow.xaml.cs:2561-2565`) son de UI: no gobiernan un registro escrito por otro camino. Hoy, por tanto,
  `L_lb(c) = −∞` en toda celda sin override: una fila con `Dep(i)` solo es `ALLOW` si la cubren los overrides positivos del
  conjunto gobernante; si no ⇒ `FAIL_CLOSED`.
- **Si I-49 integra antes** (hoy ADR-0038 aceptado por el Owner en su rama, `edafade`, e implementacion no iniciada): el dominio que su descriptor declara **como dato** y aplica al
  valor efectivo al resolver (I-49 V6 P24.5: `>= 0` en las dos propiedades Selectivas; fuera de dominio `OutOfRange`) es la
  cota autoritativa `b`. V-DEP la consume como dato; I-52 no la reimplementa ni la infiere. Se re-mide en la
  reconciliacion (§15.3).
- No se inventa ningun «valor minimo acreditado» cuando el sistema de variables o de expresiones no define dominio.

**Casos del Architect Review (normativos para G3).**

```text
Caso A — un fondo, valores positivos. Claro i sin medio frente.
  N1: PalletCount 2, Frente 40, BeamLengthOverride 70, tarimas activas.  N2: PalletCount 2, Frente 38, sin override.
  PalletTolerance vinculada a V.
  V = 4 ⇒ W = max(70, 76 + 12) = 88 ≥ 80: la fila de N1 no desborda (en V3: S-17b ALLOW).
  V = 1 ⇒ W = max(70, 76 + 3)  = 79 < 80: la fila de N1 se apila desde la izquierda en el original y en la copia.
  V4: Dep(i) verdadero; W_lb(i) = max(70, −∞) = 70 < 80 ⇒ ¬DEP-ROW ⇒ S-17d UNKNOWN ⇒ FAIL_CLOSED.
Caso B — dos fondos, sin overrides.
  Fondo maestro, claro i: PalletCount 2, Frente 40.  Fondo 1, claro i: PalletCount 2, Frente 44, tarimas activas.
  PalletTolerance vinculada a V.
  V = 4 ⇒ W = 92 ≥ 88;  V = 1 ⇒ W = 83 < 88: la frontal del fondo 1 desborda.
  V4: Dep(i) verdadero; W_lb(i) = −∞ ⇒ ¬DEP-ROW ⇒ S-17d UNKNOWN ⇒ FAIL_CLOSED.
Con una parrilla de conteo por defecto en esas filas el resultado es S-15d.
```

**Guardas de consumidores.**

- **T-M43:** toda fila de §7 con `ALLOW`/`CANONICALIZE` declara `EffectiveInputs`; si intersecan el cierre de algun
  descriptor, existe su entrada V-DEP con prueba o motivo; una fila sin declaracion ⇒ RED.
- **CT-38:** por descriptor, se varia su valor en el corpus de fixtures y se compara el sistema resuelto miembro a
  miembro por reflexion: todo simbolo que cambia y no esta en el cierre declarado ⇒ RED / STOP.
- **CT-37:** diferencial de estabilidad con dos o mas estados del registro por descriptor (con cota autoritativa si
  existe; sin cota: cero, negativos y grandes), fondo maestro distinto y cambio de gobernante: toda decision `ALLOW` en
  `R_a` debe conmutar en `R_b`. Un contraejemplo no previsto por V-DEP contradice V4 ⇒ **STOP → Proposal V5**.

### 6.6 Verificacion dinamica por rack (PREFLIGHT)

Ademas de las reglas estaticas de §7, para cada rack logico el plan del espejo **verifica** sobre el rack concreto, en
el orden de §9.1:

```text
V-META      metadata semantica desconocida del payload y metadata exterior fuera de la allowlist (§6.4)
V-DEP       autoridad de dependencias de §6.5.5 (obligaciones 9 y 9b; DEP-01..DEP-08, DEP-ROW)
V-VIEW-ALL  para cada (V,σ) ∈ A_k(μ_k D):  Π_(V,σ')(Eff(μ_k D, R)) ≡ F ∘ Π_(V,σ)(Eff(D, R))     (§3.8, §11)
V-BOM       las dos comprobaciones de §11.5
V-RT-STORE  §6.3, sobre el payload final (tras el ensayo de restamp)
```

- V-VIEW-ALL cubre **todas** las vistas admisibles del rack, seleccionadas o no, y es operacion **pura**: no busca
  hermanas, no las añade a la seleccion y no crea vistas.
- Cualquier discrepancia ⇒ `UNKNOWN` material ⇒ `FAIL_CLOSED` (E6), con la vista, pieza, linea o miembro que difiere en
  el diagnostico. La verificacion dinamica es la garantia **ejecutable** del fail-closed para toda vista que la copia
  pueda materializar despues con Insertar.
- `R` es el estado del SNAPSHOT; la validez para estados futuros la garantiza la autoridad V-DEP (§6.5.5, obligacion
  9b), no una prueba de todos los `R` en tiempo de ejecucion: V-VIEW-ALL solo prueba el `R` del SNAPSHOT.

### 6.7 Guarda de cobertura estructural (cierre transitivo)

Evita que un tipo, un miembro o un valor de enum nuevo se copie sin regla de espejo. **Sustituye** el censo manual de V3
(«anidados relevantes»): la **guarda transitiva es la autoridad**; toda tabla humana de esta Proposal es ilustrativa.

| Elemento | Contrato |
|---|---|
| Raices (minimo) | `RackEmbedDocument`, `RackProjectDocument`, `SelectivePalletDesignDocument`, `DynamicRackDesign`, `PushBackDesign`, `CantileverLineDesign`, `RackFrameConfiguration`. `RackProjectDocument` alcanza por sus ranuras los DTO de cada kind (`RackFrameProjectDocument`, `DynamicRackSystemDocument`, `SelectivePalletDesignDocument`, `PushBackDesignDocument`, `CantileverLineDocument`, ...; `RackProjectDocument.cs:18-52`) |
| Recorrido | por reflexion y transitivo hasta punto fijo: propiedades y campos publicos de instancia (y los serializados por atributo), tipos complejos, tipos de elemento de arrays y listas, clave y valor de diccionarios tipados, `Nullable<T>` desenvuelto y tipos anidados persistidos |
| Lista de corte (cerrada; cada corte con razon normativa) | (1) `SelectivePropertyValueDocument` y el diccionario `PropertyValues`: su contrato completo se delega en la autoridad de I-47/I-49 (portador CA-3); (2) `JsonElement` y los diccionarios marcados `[JsonExtensionData]`: sus claves las clasifica V-META (§6.4), no la guarda de tipos; (3) tipos primitivos y de framework (`string`, numericos, `bool`, `Guid`, `DateTime`): hojas sin miembros materiales. Un corte nuevo exige razon normativa y revision |
| Enums | instantanea (nombre → valor) de **todos** los enums alcanzados y de los enums de decodificacion de secciones aunque no sean miembros persistidos. Como minimo: `FrameSide`, `DiagonalDirection`, `PushBackSide`, `PushBackFrontalEnd`, `DynamicRackEnd`. Candidatos que la guarda confirma por alcanzabilidad: `PostSide`, `SafetySide`, `BootPlacement`, `BracingPattern`, `ExceptionType`, `FrameComponentState`, `FrameMemberType`, `PushBackCellTopology`, `PushBackRunDirection`, `DynamicRackModuleKind`, `CantileverArmSide`, `CantileverStationFaceMode`, `CantileverArmBodyArrangement`, `CantileverBraceBodyKind`, `DimensionDetail`, `DimensionViewVisibility`, `RackSystemKind` y cualquier otro. Valor nuevo, renombrado o retirado ⇒ **RED / STOP** |
| Clave de clasificacion | `(kind, ruta de esquema)`, con indices de lista sustituidos por `[]` y claves de diccionario por `{}` (la notacion de `Miembros(X)`, §6.3). Un tipo alcanzado en dos kinds se clasifica en cada uno (por ejemplo `RackFrameProjectDocument`: invariante bajo `μ_RUN` en el Selectivo, S-08; intercambio bajo `μ_D` en cabecera, H-01). Un ciclo de tipos exige entrada de recursion declarada; un ciclo sin declarar ⇒ RED |
| Clasificacion | cada clave exactamente **una** vez: `INVARIANT`, `TRANSFORMED` (con su fila de §7), `NORMALIZED` (con su N), `CARRIER` (con su entrada CA-n), `DERIVED` (calculado, no persistido, retirado declarado o descartado por el store) o `UNSUPPORTED` (con su fila `MC`/`UK`) |
| Naturaleza | ademas, **persistido**, **derivado/no persistido** o **portador** |
| Descriptores vinculables | cada descriptor se clasifica con su cierre efectivo (§6.5.5); un descriptor nuevo, o un cierre que CT-38 no confirma ⇒ RED |
| Consumo en ejecucion | un miembro clasificado `UNSUPPORTED` con valor no vacio en un rack concreto ⇒ X-16 `UNKNOWN` ⇒ `FAIL_CLOSED` (P6) |
| Donde vive | una tabla por kind en Application, junto a su reflector, y las del exterior en `RackCad.Application.Mirror`; **ninguna** en `RackCad.Application.Systems.Shared` (G-M14); sin `switch` por kind en el Plugin |
| Guardas | T-M36 (cierre y clasificacion exacta), T-M44 (instantanea de enums), T-M43 (V-DEP), G-M13 y G-M14; linea base CT-31 |

**Coexistencia con I-53 (C-08 y C-10).** I-53 G3 (`4e00a27`) añadio cuatro tipos `Header*` en
`RackCad.Application.Systems.Shared` y dos guardas: C-08 exige que todo tipo `Header*` de ese namespace figure en
`SharedFoundationInspection.Roots` (`HeaderBatchContractTests.cs:476-488` @ `4e00a27`), y C-10 prohibe en **todo** ese
namespace tipos cuyo nombre contenga `Coverage`, `StageInert`, `OmitAndReport`, `Executor`, `BatchService`, `Engine` o
`Callback` (`:493-512` @ `4e00a27`; archivo sin cambios en `7de424e`). Su G4 (`7de424e`) añade consumidores Selectivos en
`RackCad.Application.Systems.Selective` sin tocar `Systems/Shared`. Por tanto:

- I-52 **no** crea tipos en `RackCad.Application.Systems.Shared`, ni `Header*` ni de otro nombre: sus tipos viven en
  `RackCad.Application.Mirror` o junto a su kind (G-M14, CT-41);
- I-52 no modifica `SharedFoundationInspection` ni las guardas de I-53;
- todo tipo o miembro persistido de cabecera que I-53 introduzca al integrarse queda alcanzado por el cierre (via
  `RackFrameConfiguration`, `RackFrameProjectDocument` o las ranuras del envoltorio) y en **RED** hasta clasificarse.

---

## 7. Matriz de representabilidad V4

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
  G3_VERIFIED       confirmada por la caracterizacion de G3 (ninguna fila en V4)

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

**Regla de cierre de G3 (V4-10).** Al cerrar G3 **ninguna** fila puede quedar `G3_PENDING`. Cada fila queda en
exactamente uno de estos estados: `G3_VERIFIED` (con la CT que la verifica); reclasificada `UNKNOWN` + `FAIL_CLOSED`;
reclasificada `REQUIRES_MODEL_CHANGE` + `FAIL_CLOSED`. Antes de cerrar G3, cada fila condicional (por ejemplo S-10,
D-03b, K-09, S-19, D-14, P-09, PC-10, K-10 y H-07) se parte en filas o estados inequivocos, con una sola tripleta
(clase, evidencia, disposicion). Si una reclasificacion cambia materialmente el alcance, el ADR, una regla de reflexion
o la arquitectura ⇒ **Proposal V5**. T-M47 lo hace ejecutable desde G5.

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
| S-15 | Parrilla `ParrillaFrontal`, `ParrillaLateral`, `ParrillaFrente`, `ParrillaCantidad`, sin desborde | reparto simetrico en el tramo | R | G3_PENDING | ALLOW | toda fila con `count·frente ≤ span + GeometryTolerance.Length`; con conteo por defecto y `Dep(i)`, ademas `DEP-ROW` (DEP-04); con conteo o frente manual, DEP-05 | `SelectiveParrillaPlacement.cs:40-54`; `SelectiveParrillaPlan.cs:107`; `SelectiveFrontalBuilder.cs:262-311` |
| S-15x | Parrilla con una fila desbordada | apilado desde la izquierda | UK | CODE_SUPPORTED | FAIL_CLOSED | alguna fila con `count·frente > span + GeometryTolerance.Length` | `SelectiveParrillaPlacement.cs:49`; `SelectiveFrontalBuilder.cs:310` |
| S-15d | Parrilla de claro completo con conteo por defecto, con `Dep(i)` y sin `DEP-ROW` | un `ChangeValue` posterior puede desbordarla (casos A y B, §6.5.5) | UK | CODE_SUPPORTED | FAIL_CLOSED | `Dep(i)` ∧ `¬DEP-ROW` en alguna fila | `SelectiveParrillaPlan.cs:107`; `SelectiveFrontalBuilder.cs:291-311`; `SelectiveGeometryResolver.cs:150-173`, `:369-393` |
| S-15b | Parrilla `ParrillaOffCells[].Frente` | `f → M−1−f` | R | G3_PENDING | ALLOW | — | `SelectiveFrontalBuilder.cs:249-255` |
| S-16 | Numeracion y nombres de bloque por indice | se regeneran (PDC-5) | R | CODE_SUPPORTED | ALLOW | — | `SelectiveFrontalBuilder.cs:320-340` |
| S-17 | Toggles: `Dimensions`, `DimensionStyle`, `AnnotationScale`, `DrawRackName`, `NumberFronts`, `NumberLevels`, `DrawBasePlate` y la **bandera** `DrawPallets` | sin cambio; anotaciones regeneradas con el nombre logico destino | R | CODE_SUPPORTED | ALLOW | — | `SelectiveDimensions.cs:76-263`; `SelectiveFrontalBuilder.cs:342` |
| S-17b | **Tarimas dibujadas** (`DrawPallets` activo), filas sin desborde | reparto simetrico en el claro o tramo | R | G3_PENDING | ALLOW | toda fila de claro completo con `count·frente ≤ BeamLength + GeometryTolerance.Length` (los tramos usan `PalletFit` y no desbordan) y, con `Dep(i)`, ademas `DEP-ROW` (DEP-03) | `SelectiveTarimaPlacement.cs:43-51`; `SelectiveFrontalBuilder.cs:388-440` |
| S-17c | **Tarimas**: fila de claro completo desbordada | apilado desde la izquierda | UK | CODE_SUPPORTED | FAIL_CLOSED | alguna fila de claro completo con `count·frente > BeamLength + GeometryTolerance.Length` | `SelectiveTarimaPlacement.cs:47`; `SelectiveFrontalBuilder.cs:389-404` |
| S-17d | **Tarimas** de claro completo (incluida la fila de piso) con `Dep(i)` y sin `DEP-ROW` | un `ChangeValue` posterior puede desbordarlas (casos A y B, §6.5.5) | UK | CODE_SUPPORTED | FAIL_CLOSED | `Dep(i)` ∧ `¬DEP-ROW` en alguna fila | `SelectiveFrontalBuilder.cs:388-404`; `SelectiveGeometryResolver.cs:150-173`, `:354`, `:369-393` |
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
| P-03 | Configuracion por frente (`DefaultPalletsDeep`, `PalletsDeepOverrides`, la **bandera** `DrawPallets`, `HighEndBeamPeraltes`, `FirstLevelHeight`) | se reordena con los frentes | R | G3_PENDING | ALLOW | — (tarimas dibujadas: P-03b/P-03c) | `PushBackCellDepth.cs:41-173` |
| P-03b | **Tarimas frontales dibujadas** (`DrawPalletAt` verdadero), filas sin desborde | una tarima por calle, centrada en su calle; la holgura del larguero se reparte a los dos extremos | R | G3_PENDING | ALLOW | en toda celda dibujada del corte: `max(1, PalletCount)·lane ≤ CellBeamLength + GeometryTolerance.Length`, con `lane = BFR` si `BFR > 0` y si no `Frente` | `PushBackTarimaPlacement.cs:210-233`; `PushBackSystemFrontalBuilder.cs:323-374`; `PushBackLoadBeamGeometry.cs:31-40` |
| P-03c | **Tarimas frontales**: fila desbordada | `FrontalRow` recorta el margen a 0 y ancla la fila al extremo izquierdo del larguero: no conmuta bajo RT | UK | CODE_SUPPORTED | FAIL_CLOSED | alguna celda dibujada con `max(1, PalletCount)·lane > CellBeamLength + GeometryTolerance.Length` (alcanzable con `BeamLengthOverride` por nivel, `PushBackLoadBeamGeometry.cs:29`) | `PushBackTarimaPlacement.cs:224`, `:228`; `PushBackSystemFrontalBuilder.cs:359-367` |
| P-04 | **Tope posterior activo — estado POR DEFECTO**: `RearTopeSaque`, `RearTopePieceId`, rejilla | **sin regla**: anclaje al TROQUEL_TOPE del poste izquierdo del frente, mano constante `ElevationMirrored = false` en FRONTAL y `LONGITUD = larguero + 0.25"` (patron de AR-01) | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2B) | **activo por defecto**: existe un frente `f` y un nivel `l < DynamicFrontActivation.EffectiveLoadLevels(frente)` con `Draws(f, l)`, donde `Draws = !IsNone ∧ At` y `At` solo es falso en `OffCells`; un `PieceId` en blanco o desconocido usa `LARGUERO_ESCALON_TOPE_DE_3`; predicado exacto en CT-15 | `PushBackRearTope.cs:9`, `:24`, `:37-53`; `PushBackDesign.cs:41`; `PushBackResolver.cs:73`, `:121`; `PushBackDesignDocument.cs:117`, `:204`; `PushBackRearTopeBuilder.cs:27`, `:37-48`, `:73-86`, `:172-209`, `:273`; `PushBackSystemFrontalBuilder.cs:236`, `:277-300`; `PushBackSystemPlantaBuilder.cs:77`, `:115-118`, `:134-154`; `DynamicEndBeamIdentity.cs:94-100`; `SelectiveTopePlacement.cs:41-59` |
| P-04b | Tope posterior **inactivo**: `(NINGUNO)` (`IsNone`) o ninguna celda con `Draws` | escalares sin efecto grafico; se copian sin cambio (la mascara: P-05b) | R | CODE_SUPPORTED | ALLOW | ¬P-04 | `PushBackRearTope.cs:37-38`, `:53`; `PushBackSystemFrontalBuilder.cs:277`; `PushBackSystemPlantaBuilder.cs:115-118` |
| P-05 | `RearTopeOffCells[].Frente` con tope posterior activo | sin regla mientras P-04 sea UK | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2B) | igual que P-04 | `PushBackRearTopeBuilder.cs:268-307` |
| P-05b | `RearTopeOffCells[].Frente` con tope inactivo (P-04b) | `f → N−1−f`: la mascara sigue persistida y vuelve a regir si el usuario elige una pieza en la copia (`[V4-D30]`) | R | G3_PENDING | ALLOW | P-04b | `PushBackRearTope.cs:41`, `:46-53`; `PushBackRearTopeBuilder.cs:268-307` |
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
| PC-08 | **Tope posterior activo por lado — estado por defecto en cada lado** | sin regla (patron de P-04) | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2B) | P-04 evaluado por lado: el lado B tiene su propia configuracion, activa por defecto; basta un lado activo | `PushBackSideDesign.cs:48`, `:74`; `PushBackDesignDocument.cs:283-286`; `PushBackRearTopeBuilder.cs:73-86`, `:291-306` |
| PC-08b | Tope posterior inactivo en **ambos** lados; `OffCells` por lado | `f → N−1−f` por lado (mascara latente, como P-05b) | R | G3_PENDING | ALLOW | P-04b en A y en B | idem |
| PC-09 | `HeaderLineOverrides` con `ModuleId` `M…`/`GAP`/`B:…` | `p → N−p`; `ModuleId` intacto | R | G3_PENDING | ALLOW | — | `PushBackEditorDesignAssembler.cs:348-364` |
| PC-10 | `View` y `Section` frontal `0..3` | §3.7 | R | CODE_SUPPORTED | ALLOW en rango; FAIL_CLOSED fuera | — | `PushBackSystemFrontalBuilder.cs:137-155` |
| PC-11 | Letras «A»/«B» | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `PushBackSideAnnotations.cs:31`, `:104` |
| PC-12 | `Composite.Gap`, `CentralSeparator` | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `PushBackCompositeStructure.cs:123-133` |
| PC-13 | Vista lateral | no expone RT | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` | — |
| PC-14a | **Tarimas frontales por lado**, filas resueltas sin desborde | como P-03b, sobre las filas resueltas por plano de corte (cama fisica × corte) | R | G3_PENDING | ALLOW | en toda fila resuelta: `Lanes·lane ≤ BeamLength + GeometryTolerance.Length` | `PushBackPalletProjection.cs:193-197`, `:208-214`; `PushBackCompositeFrontal.cs:93-108` |
| PC-14b | **Tarimas frontales por lado**: fila resuelta desbordada | como P-03c | UK | CODE_SUPPORTED | FAIL_CLOSED | alguna fila resuelta con `Lanes·lane > BeamLength + GeometryTolerance.Length` | `PushBackTarimaPlacement.cs:224`; `PushBackPalletProjection.cs:211` |

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
| X-02 | Portadores del sobre de la allowlist (CA-1, CA-4) | `Compose` los hereda | R | CODE_SUPPORTED | ALLOW | `RackEmbedComposer.cs:21-35`; §6.4; D-21 de I-54 |
| X-02b | Clave de `ExtensionData` del sobre desconocida y no vacia fuera de la allowlist, o duplicado ambiguo de `CustomProperties` | ninguna regla | UK | CODE_SUPPORTED | FAIL_CLOSED | `RackEmbedDocument.cs:35-64`; `RackEmbedComposer.cs:33`; §6.4 |
| X-03 | Portadores del envoltorio de la allowlist (CA-2) | `WithSourceMetadataFrom` | R | CODE_SUPPORTED | ALLOW | `RackProjectStore.cs:71-82` |
| X-03b | Clave de `ExtensionData` del envoltorio desconocida y no vacia | ninguna regla (allowlist vacia) | UK | CODE_SUPPORTED | FAIL_CLOSED | `RackProjectDocument.cs:51-52`; `RackProjectStore.cs:82`; §6.4 |
| X-03c | Ranura de payload conocida e inactiva del envoltorio, no vacia | el store no la reescribe | R | CODE_SUPPORTED | BASELINE_LIMITED | `RackProjectStore.cs:50-61`, `:64-82`; V-RT-STORE |
| X-04 | Colocacion de la referencia | `P' = G·P·F` | R | CODE_SUPPORTED | ALLOW | §3 |
| X-05 | Referencias enlazadas seleccionadas | una definicion nueva + N referencias | R | CODE_SUPPORTED | ALLOW | I-51 PD-2 |
| X-07 | Drive-In | no existe | — | CODE_SUPPORTED | no aplica | `RackMainMenuWindow.xaml:139-144` |
| X-08 | Larguero | sin bloque RackCad | — | CODE_SUPPORTED | no aplica | `KindHandlerRegistry.cs:51-53` |
| X-13 | Holgura u offset grafico asimetrico hallado por la auditoria de anclajes | ninguna regla implicita | UK | G3_PENDING | FAIL_CLOSED hasta regla aprobada (precedente AR-01) | CT-17 |
| X-14 | Parametro dinamico con semantica de lado sin regla | ninguna regla implicita | UK | CODE_SUPPORTED | FAIL_CLOSED | CT-25; §11.4 |
| X-15 | Vista admisible del rack (seleccionada o no) que no conmuta | V-VIEW-ALL | UK | G3_PENDING | FAIL_CLOSED | §3.8, §6.6 |
| X-16 | Miembro clasificado `UNSUPPORTED` por la guarda de cobertura con valor no vacio | sin regla de espejo | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.7 |
| X-17 | Diferencia de mano de una pieza sin declaracion aplicable (sin `GeometricEvidence`, parametro fuera de `CharacterizedDomain` o `DefinitionGeometryHash` distinto) | la simetria no esta demostrada | UK | CODE_SUPPORTED | FAIL_CLOSED | §11.3 |
| X-18 | Decision `ALLOW`/`CANONICALIZE` sin prueba de estabilidad 9b, fuera de las filas especificas | ninguna | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.5.5 |

### 7.9 Precondiciones operativas (no son clases de representabilidad)

| # | Precondicion | Disposicion | Momento | Evidencia en codigo |
|---|---|---|---|---|
| PRE-01 | Candidato RackCad no canonico: MINSERT, `Normal`, rotacion, escala (ST-3, ST-5..ST-7, ST-9, ST-10); los objetos sin payload RackCad no llegan aqui (C2 de §4.1) | FAIL_CLOSED (E4) | P1 | §4.1 |
| PRE-02 | Definicion con `Origin ≠ 0` (ST-14) | FAIL_CLOSED (E4) | P1 | `RackCloner.cs:34`; `LateralHeaderDrawer.cs:243` |
| PRE-03 | `View` no vacia no reconocida o `Section` no decodificable (ST-15) | FAIL_CLOSED (E4) | P1 | §3.7 |
| PRE-04 | Referencia dinamica, anonima o anotativa con payload RackCad (ST-17) | FAIL_CLOSED (E4) | P1 | `RackBlockFinder.cs:66` |
| PRE-05 | Divergencia authored entre vistas del grupo | FAIL_CLOSED (E3) | P2 | §5.4 |
| PRE-06 | Registro no acreditable con algun grupo cuyo kind lo consume, o vinculo roto | FAIL_CLOSED (E7) | P3 | §10.2 |
| PRE-07 | Push Back bloqueado para salida (`RackBomOutputGate.For(system).Reason ≠ null`) | FAIL_CLOSED (E6) | P4 | `PushBackKindHandler.cs:61-74` |
| PRE-08 | Linea Cantilever invalida o catalogo de secciones ilegible | FAIL_CLOSED (E6) | P4 | `CantileverKindHandler.cs:48-67` |
| PRE-09 | Bloques de biblioteca ausentes tras PREPARE; `MissingInstances > 0` en MUTATE | FAIL_CLOSED (E10/E11) | PREPARE/MUTATE | `BlockLibraryImporter.cs:74-118`; `LateralHeaderDrawer.cs:293-303` |
| PRE-10 | Huella o read-set cambiado | FAIL_CLOSED (E10/E11) | PREPARE/MUTATE | §9.2 |
| PRE-11 | Payload RackCad presente pero ilegible o inutilizable, incluido F-14a de I-54 (`ArgumentException` por UTF-16 crudo invalido) | FAIL_CLOSED (E2) | SNAPSHOT/P1 | §4.1 (ST-2c) |
| PRE-12 | Sobre que no se puede reserializar en el ensayo de P10 (F-14b de I-54) o que `Compose` rechaza (regla F de I-54) | FAIL_CLOSED (E8) | P10 | §9.4 |
| PRE-13 | `DefinitionGeometryHash` de una definicion declarada simetrica distinta del registrado, o no calculable de forma determinista | FAIL_CLOSED (E6 en PREFLIGHT; E10 tras importar en PREPARE) | P7 / PREPARE | §11.3 |

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

**C-2.** Pruebas de equivalencia en **CI**, contra los productores reales, por fixture de (kind, vista admitida) y por su
copia espejo:

| # | Productor | Camino que se ejercita |
|---|---|---|
| C2-1 | Autoridad nueva de plan | `RackViewPlanAuthority.Build(designJson, View canonica, σ' canonica, catalogos, estado efectivo, RackName)` |
| C2-2a | `RACKEDITAR` → **Actualizar** (`EditX`, redibujo en sitio) | payload → apertura del editor → sistema que dibuja el comando → servicio de dibujo → builder. Selectivo: `SelectiveEditorOpen.Resolve` → `LoadExisting` → `SystemToInsert` → `SelectiveFrontalDrawService().RedrawInPlace` / `SelectivePlantaDrawService().RedrawInPlace` (`RackSelectivoCommands.cs:66-117`, `:175-217`; `RackSelectiveWindow.xaml.cs:185-188`); Dinamico, Push Back y Cantilever: `LoadExisting` → `DesignToInsert`/`SystemToInsert` (`RackDinamicoCommands.cs:162-177`; equivalentes); cabecera: `RackFrameConfiguratorViewModel.Configuration` |
| C2-2b | `RACKEDITAR` → **Insertar** desde la copia (vista nueva enlazada, mismo GUID) | payload → apertura del editor → sistema que dibuja el comando → camino de insercion → builder. Selectivo: `DrawSelectiveViewFromAuthored` → `InsertSelectiveFrontal` o `SelectivePlantaDrawService().DrawAndPlace` (`RackSelectivoCommands.cs:246-258`, `:426-452`); equivalentes en Dinamico, Push Back, Cantilever y cabecera |
| C2-3 | `ProjectVariableMutationExecutor` (Selectivo) | `EffectiveOutput` → `SelectiveGeometryResolver` → `FondoSystemView`/planta → builder (`ProjectVariableMutationExecutor.cs:166-243`) |
| C2-4 | Estado del editor → sistema/documento | `SelectiveEditorState`, `DynamicEditorDesignAssembler`, `PushBackEditorDesignAssembler`, `CantileverLineEditorAssembler` en Core; lo que solo pasa por la ventana, en `tests/RackCad.UI.Tests` |

```text
firma(C2-1) = firma(C2-2a) = firma(C2-2b) = firma(C2-3, solo Selectivo) = firma(C2-4)
firma de §11.2, con: nombre logico, estado efectivo (lectura del registro), View y Section canonicas, DimensionViews
```

**Insertar es normativo (V4-09).** V-VIEW-ALL (§3.8) se apoya en que la copia pueda materializar despues cualquier vista
admisible con Insertar. Regla:

- si C2-2a y C2-2b llegan **exactamente** a la misma costura de builder con las mismas entradas (sistema efectivo, `Name`
  logico, vista y seccion canonicas, `DimensionViews`), pueden **compartir fixture** de comportamiento, **pero** la guarda
  G-M9 demuestra que ambos caminos siguen llamando esa costura;
- si no, Insertar es un **productor adicional** con prueba propia (T-M17b).

CT-26 mapea en G3 las costuras de Actualizar e Insertar por kind y vista admitida.

- **Gate.** C2-1, C2-2a, C2-2b, C2-3 y C2-4 deben estar **GREEN antes de cerrar G6**, que es donde nace la autoridad.
  **G7 no consume la autoridad** si C-2 no esta verde. G8 conserva los cruces entre iniciativas, la continuidad de G-M9 y
  la reconciliacion final con las autoridades integradas.
- **Guarda G-M9.** Fija el mapa exacto de invocaciones de builders y servicios de dibujo de `EditX` (Actualizar **e
  Insertar**) y del ejecutor para las vistas admitidas (`SelectiveDepthLayout.FondoSystemView`,
  `SelectiveFrontalDrawService`, `SelectivePlantaDrawService`, `DynamicFrontalDrawService`, `DynamicPlantaDrawService`,
  `PushBackFrontalDrawService`, `PushBackPlantaDrawService`, `CantileverViewPlanBuilder.Build`, `LateralHeaderDrawService`,
  `PlantaHeaderDrawService` y las llamadas a builders dentro de esos servicios). Nace en G6 y sigue vigente en G8.
- **Validacion del Owner obligatoria:** M-4 (Actualizar) y M-5 (Insertar).
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
  P1  precedencia de fuente                        C1..C5 de §4.1 (candidato RackCad primero; restricciones estrictas solo sobre
                                                   candidatos), incluida la decodificacion de View/Section (§3.7)
  P2  agrupacion + autoridad authored              nucleo neutral; Kind y nombre del grupo; IsSameAuthority (§5)
  P3  precondiciones del registro y de la          acreditacion solo para grupos cuyo kind consume el registro;
      resolucion efectiva                          vinculos sanos; read-set (§10.2)
  P4  fidelidad del store y sustrato               lectura con la autoridad del kind; V-META del payload y del exterior (§6.4); LB(D) (§6.3);
                                                   precondiciones de diseño (PRE-07, PRE-08)
  P5  Assess + Reflect candidato                   clase, evidencia y disposicion por fila; normalizaciones; V-DEP (§6.5.5; obligaciones 9 y 9b)
  P6  cobertura de portadores y miembros           portadores de la allowlist identicos (§6.4); miembros UNSUPPORTED no vacios ⇒ X-16 (§6.7)
  P7  V-VIEW-ALL                                   todas las (View, Section canonica) admisibles del rack logico (§3.8); una declaracion de
                                                   simetria solo cuenta si aplica (GeometricEvidence y ligadura, §11.3)
  P8  V-BOM                                        las dos comprobaciones (§11.5)
  P9  identidad y nombre                           NewRackId por grupo; «<base> - espejo»; validacion del asignador (§5.5)
  P10 Compose + ensayo de restamp + V-RT-STORE     Plugin, sin transaccion; V-RT-STORE sobre el payload re-estampado (§6.3)

LINE       dos puntos UCS→WCS validos (§4.2)
PREPARE    colocaciones P' · planes efectivos de las vistas SELECCIONADAS (§8.1) · payloads definitivos
           · EnsureForPlan (infraestructura, §9.5) · re-verificacion de bloques · re-verificacion de huellas y read-set
           · DefinitionGeometryHash de las definiciones declaradas simetricas tras importar (§11.3 punto 7)
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
| **read-set del registro** (solo si algun grupo consume el registro) | las **observaciones** que la autoridad efectiva realmente uso para decidir (§9.2.1). **No** incluye ninguna «version del registro» ni una huella del registro completo |

Cualquier diferencia ⇒ **fail-closed antes de mutar** (E10 en PREPARE; en MUTATE, excepcion y rollback).

#### 9.2.1 Read-set por observaciones (V4-08)

| Observacion | Contenido | Autoridad |
|---|---|---|
| RS-1 Acreditacion | resultado de la lectura (`ProjectVariablesReadOutcome`: ausente, legible, ...) y resultado de la acreditacion **realmente usado** (`Usable`, `AmbiguousIdentity`, `NotReadable`, `InvalidTarget`) | `UsableProjectVariablesRegistry.cs:7-23`, `:120-140` |
| RS-2 Variables leidas | cada `VariableId` que la resolucion efectiva **realmente** leyo al inspeccionar los vinculos del rack: presencia, `VariableType` y definicion observable (literal) | `SelectiveLinkedPropertyKernel.InspectBindings` → `LinkedPropertyInspection.InspectBinding` (`SelectiveLinkedPropertyKernel.cs:121-150`) |
| RS-3 Dependencias transitivas | **solo si I-49 integra antes**: las observaciones de su `PlanReadSet` (resultado de cada simbolo realmente usado, incluidas las dependencias de expresiones) | I-49 V6 D19; ADR-0038 (aceptado @ `edafade`) |
| RS-4 Observaciones adicionales | cualquier dato adicional que la autoridad efectiva vigente necesite para repetir la misma decision (por ejemplo, el dominio del consumidor si I-49 lo aplica) | la autoridad efectiva vigente |

- Un cambio **irrelevante** (una variable no observada, una entrada nueva no leida) **no** aborta. Un cambio de algo
  **observado** ⇒ **abort before MUTATE** (E10 en PREPARE; excepcion y rollback en MUTATE).
- I-52 **no** implementa un segundo motor de expresiones ni su propio calculo de dependencias: si I-49 integra, adopta su
  autoridad de lectura y sus observaciones transitivas (PV-6).
- Pruebas: T-M41 y CT-40.

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
| E1 | Ninguna fuente valida (todos los objetos ignorados en C1..C3 de §4.1) | P1 | fin, sin linea |
| E2 | Payload RackCad inutilizable o ilegible (ST-2c, incluido F-14a de I-54), `Kind` sin reflector o sin handler, `Design` vacio | P1 | fin, cero mutacion |
| E3 | Grupo inconsistente (`Kind`, nombre, autoridad authored) | P2 | fin, cero mutacion |
| E4 | Candidato RackCad no canonico (C4 de §4.1: ST-3, **ST-17**, ST-5, ST-6, ST-10, ST-9, ST-7, ST-14, ST-15) | P1 | fin, cero mutacion; ST-17 con mensaje propio («bloque dinamico, anonimo o anotativo») |
| E5 | Vista seleccionada no admisible (lateral de rack, cama) | P1 | fin, cero mutacion; el mensaje sugiere reflejar las vistas admisibles e insertar las demas con `RACKEDITAR` desde la copia |
| E6 | Fila `REQUIRES_MODEL_CHANGE` o `UNKNOWN` material (topes Selectivo, tope posterior de Push Back **activo por defecto**, medio frente dependiente, desviador inestable, parrilla o tarimas desbordadas, **parrilla o tarimas sin `DEP-ROW` (S-15d, S-17d)**, **tarimas frontales de Push Back desbordadas (P-03c, PC-14b)**, Auto con baseline), V-META (payload **y exterior**), V-DEP (obligaciones 9 y 9b), **V-VIEW-ALL** (incluidas vistas admisibles no seleccionadas), X-16, **X-17** (simetria sin declaracion aplicable), X-18, V-BOM o V-RT-STORE fallidas; diseño bloqueado o invalido | P4..P10 | fin, cero mutacion; nombra rack, vista, propiedad, pieza, clave o miembro |
| E7 | Registro **no acreditable** (presente e ilegible, version incompatible o identidad ambigua) **y** algun grupo cuyo kind lo consume (hoy: Selectivo, con o sin vinculos); o vinculo roto | P3 | fin, cero mutacion. Si ningun grupo lo consume, el registro no se evalua (§10.2) |
| E8 | Ensayo de `Compose` + serializacion + restamp + nombre fallido, incluidos F-14b de I-54 (la reserializacion lanza `JsonException`) y la regla F de `Compose` (origen con una clave de `ExtensionData` igual a un miembro declarado) | P9, P10 | fin, cero mutacion; toda excepcion de esos pasos se traduce a E8 con mensaje atribuible |
| E9 | Linea invalida o cancelada | LINE | fin, cero mutacion |
| E10 | Fallo de plan, bloque ausente tras importar, `DefinitionGeometryHash` de una definicion declarada simetrica distinta tras importar, huella o read-set cambiado | PREPARE | fin; **ninguna** definicion, referencia ni payload RackCad nuevo; lo importado por `EnsureForPlan` puede permanecer (§9.5) |
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
| PV-6 | Si I-49 integra antes (hoy: Consensus Freeze `364d6c0` y ADR-0038 aceptado por el Owner en `edafade`; implementacion no iniciada): RACKMIRROR invoca la autoridad efectiva integrada, preserva `expression` como portador, adopta su `PlanReadSet` para el read-set (§9.2.1) y su dominio de consumidor como cota autoritativa de V-DEP (§6.5.5), y **no** reimplementa su evaluacion ni un segundo motor de expresiones (P24.5 y P24.8 de I-49 V6) |
| PV-7 | La copia es un **consumidor nuevo** del mismo `VariableId` (ADR-0034 §12): bloquea `Delete` y se redibuja en `ChangeValue` desde su authored reflejado. **`ChangeValue` no puede invalidar ninguna decision `ALLOW` o `CANONICALIZE` tomada en PREFLIGHT** (obligacion 9b): la exposicion de §3.2 vale para todo estado acreditable `R`, y un rack con alguna decision cuya verdad dependa de un valor vinculado sin prueba para todo `R` no es admisible (S-02b, S-14c, S-15d, S-17d y toda fila que V-DEP marque) |

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
- **I-54 (Proposal V5, `26ca923`, sobre `main @ f8deb67`; sin consenso ni produccion)**: las propiedades de
  alcance Rack viven en el miembro `CustomProperties` del sobre (`JsonElement?`); `Compose` lo hereda y no hay promocion
  de major. Antes de integrarse, un build sin I-54 lo conserva en `ExtensionData` del sobre. En ambos casos es
  **portador por nombre** (CA-4, §6.4) y el espejo lo conserva. **D-21 sin cambio**: el espejo cumple el invariante (A y
  despues B). V4 y V5 de I-54 declaran residuales preexistentes del store del sobre que afectan a todo flujo que lo reescribe,
  tambien al espejo: **F-14a** (UTF-16 crudo invalido: la lectura lanza `ArgumentException`) ⇒ E2 (ST-2c); **F-14b**
  (surrogate suelto escapado: la reescritura lanza `JsonException`) ⇒ E8 en el ensayo de P10; y su **regla F** (`Compose`
  rechaza un origen con una clave de `ExtensionData` igual, sin distinguir mayusculas, a un miembro declarado) ⇒ E8. Si
  I-54 integra antes del Candidato: T-M20 y re-medicion del censo T-GRD-02 (7 → 8). V5 de I-54 (C-F3) precisa que en los
  consumidores existentes el punto de fallo de F-14b depende de su granularidad transaccional (`RACKEDITAR` puede quedar
  parcialmente actualizado entre vistas); en el espejo el ensayo de P10 serializa todos los payloads **antes** de MUTATE,
  asi que F-14b termina en E8 sin ninguna mutacion parcial.

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

### 11.3 Mano y declaracion tipada de simetrias (evidencia geometrica independiente)

1. **Ningun bloque DWG se asume simetrico.** La biblioteca no esta versionada y no tiene metadato de simetria; el
   manifiesto de I-19 registra nombre, piezas, vistas, parametros y huella, **no** geometria
   (`CatalogBlockManifest.cs:12-59`).
2. Una diferencia de mano entre `A = F ∘ Π(D)` y `B = Π(μD)` en una pieza es **no equivalente**, salvo que el bloque figure
   en la **declaracion tipada unica** de simetrias **y** la declaracion aplique (punto 7).
3. **Donde vive.** Un unico registro tipado en `RackCad.Application.Mirror` (nombre no contractual
   `BlockSymmetryDeclaration`). **Fuera** de `assets/`, de los catalogos y de `RackCad.Application.Systems.Shared`. Su unico
   consumidor es el comparador de equivalencia. Ningun literal de simetria en otro archivo (G-M10).
4. **Prohibicion de circularidad (V4-01).** En V3, CT-06 observaba la diferencia entre planes, inferia `c` y ese mismo `c`
   justificaba la equivalencia. **Queda prohibido.** La evidencia se separa en dos, con orden obligatorio:

   | Evidencia | Que demuestra | Fuentes admitidas | Fuentes prohibidas |
   |---|---|---|---|
   | **`GeometricEvidence`** (primero) | que la **definicion real** del bloque, en cada estado de parametros cubierto, es geometricamente igual a su reflexion respecto de `LocalSymmetryAxis` y `LocalSymmetryCenter` | geometria de la definicion en la biblioteca registrada, leida por una sonda de caracterizacion (CT-36); autoridad de I-19 (`CatalogBlockManifest`) para identidad y parametros; analisis especifico de la definicion; confirmacion visual del Owner **solo** como confirmacion | builders; diferencia entre `Π(D)` y `Π(μD)`; diferencia de inserciones; centro del bounding box; centro que minimiza el error entre planes |
   | **`PlanPlacementEvidence`** (despues) | que los builders usan esa simetria correctamente: con la declaracion aprobada, `A ≡ B` | CT-06 y T-M12 sobre planes | nunca ajusta, infiere ni corrige `c` |

   Una `PlanPlacementEvidence` sin `GeometricEvidence` previa no admite ninguna declaracion. Si con la declaracion
   aprobada `A` y `B` difieren, la diferencia es un **desplazamiento**, no una mano: X-13/X-17 `UNKNOWN` ⇒ `FAIL_CLOSED`.

5. **Contrato de cada entrada.**

   | Campo | Contenido |
   |---|---|
   | `BlockName` | nombre exacto, presente en `assets/catalogs/blocks.csv` y en el manifiesto de I-19 |
   | `ViewOrFamily` | la `View` del plan (`HeaderRun`) o la familia de curvas |
   | `LocalSymmetryAxis` | `X` o `Y` del bloque |
   | `LocalSymmetryCenter` | coordenada en el **marco local de insercion tras `T(−BlockOrigin)`** (punto 8): constante, o afin `c = a0 + a1·Parametro` solo con el punto 6 |
   | `CharacterizedDomain` | bloque estatico: todo estado. Bloque dinamico: lista **finita** de estados de parametros caracterizados, o un **intervalo** por parametro solo si el punto 6(b) establece la relacion sobre ese intervalo |
   | `GeometricEvidence` | registro fechado en `docs/automation/evidence/` con: metodo (sonda y version); biblioteca (ruta de `BlockLibraryLocator.ResolvePath`, tamaño, ultima escritura y hash de contenido del archivo); `CatalogBlockManifest.Fingerprint` (`CatalogBlockManifest.cs:57`, `:127-150`); `DefinitionGeometryHash` de la definicion medida; estados medidos; eje; centro; desviacion maxima (≤ `GeometryTolerance.Length`); y, para forma afin, la fuente de la relacion (punto 6b) |
   | `PlanPlacementEvidence` | fila de CT-06 que muestra `A ≡ B` con la declaracion aprobada |
   | `OwnerEvidence` | confirmacion visual fechada (M-15; O-3); **nunca** es la unica prueba |
   | `Test` | prueba de equivalencia que usa la entrada (T-M27) |

6. **Forma afin.** `c = a0 + a1·Parametro` solo se admite si se demuestran **las dos** cosas:
   - (a) en cada estado caracterizado, la geometria de la definicion reflejada respecto de `c(estado)` coincide con la
     original (desviacion ≤ `GeometryTolerance.Length`), con al menos los extremos del dominio y un valor interior;
   - (b) la relacion proviene del **contrato geometrico del bloque** y no de offsets del builder: la sonda lee la
     parametrizacion de la definicion (parametro lineal y accion de estiramiento: punto base, direccion y conjunto
     estirado), o una autoridad de catalogo ya declara los puntos medidos del bloque en funcion del parametro, y de esa
     parametrizacion se **deriva** `a0` y `a1`.

   Dos o mas puntos usados **solo** para ajustar una recta **no bastan**. Sin (b), la declaracion cubre unicamente la
   lista finita de estados medidos; sin (a) ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`.
7. **Aplicacion en ejecucion (ligadura a la definicion real).** En PREFLIGHT una declaracion **aplica** a una instancia
   solo si el bloque y la vista coinciden y el valor del parametro de **esa** instancia esta en `CharacterizedDomain`.
   Para las definiciones que RACKMIRROR **dibuja** (vistas seleccionadas), la definicion usada debe tener el
   `DefinitionGeometryHash` registrado: se comprueba en PREFLIGHT si la definicion ya esta en el dibujo y en PREPARE tras
   `EnsureForPlan` (una definicion existente con el mismo nombre no se reemplaza, §9.5 punto 6). Discrepancia, o huella que
   no pueda calcularse de forma determinista ⇒ la declaracion **no aplica** ⇒ X-17 `UNKNOWN` ⇒ `FAIL_CLOSED` (E6 en
   PREFLIGHT; E10 en PREPARE). Para las vistas admisibles **no seleccionadas** (V-VIEW-ALL), la equivalencia se afirma
   respecto de la definicion caracterizada; la definicion que un Insertar futuro importe queda bajo la limitacion general
   de §9.5 punto 6, igual que para el rack original. La canonizacion exacta de la huella (tipos de entidad, coordenadas en
   el marco local, independencia de handles) la caracteriza CT-36 en G3; si G3 no la logra determinista, las filas
   afectadas quedan `FAIL_CLOSED` y, si eso cambia materialmente el alcance, **Proposal V5**.
8. **Marco del centro.** El drawer crea cada pieza como `BlockReference(Position = q, Rotation = r,
   ScaleFactors = (mX, mY, 1))` (`LateralHeaderDrawer.cs:305-314`), cuya transformacion es `T(q)·R(r)·S(mX,mY)·T(−o_p)`,
   con `o_p` = `BlockTableRecord.Origin` del bloque de pieza. El centro se mide en `u = x_BTR − o_p`. **No** en WCS, **no**
   en el marco del rack y **no** en coordenadas crudas del BTR cuando su `Origin` desplaza el marco efectivo.
9. **Equivalencia.** Para una entrada que aplica, con eje X y centro `c` (en `u`), la instancia `(q, r, mX, mY,
   parametros)` equivale a

   ```text
   (q + R(r)·(2·c·mX, 0), r, −mX, mY, parametros)          eje X
   (q + R(r)·(0, 2·c·mY), r, mX, −mY, parametros)          eje Y
   ```

   que sale de `T(q)·R(r)·S(mX,mY)·X_c = T(q + R(r)·(2c·mX, 0))·R(r)·S(−mX, mY)`, con `X_c = T(2c,0)·S(−1,1)`. Con forma
   afin, `c` se evalua con el valor del parametro de **esa** instancia.
10. **Por que un centro conveniente no puede ocultar un desplazamiento.** Una declaracion re-expresa la mano de una pieza
    sobre **su propia ocupacion**. Con un `c` distinto del centro geometrico, la formula del punto 9 afirmaria que dos
    instancias con ocupaciones distintas son iguales. Casos del Architect Review (normativos para T-M40):

    ```text
    Caso C (afin). Pieza de rol distinto de Tope con LONGITUD en X; bloque en u ∈ [0, L] (centro geometrico L/2);
      el builder inserta en el ancla izquierda a con L = S + δ.
      A = (F(a), mX = −1);  B = (F(b), mX = +1);  F(a) − F(b) = S.
      A ≡ B con la formula del punto 9  ⇔  c = S/2 = L/2 − δ/2   (a0 = −δ/2, a1 = 1/2).
      GeometricEvidence mide c = L/2 (a0 = 0) ⇒ la entrada con a0 = −δ/2 no se admite ⇒ A ≢ B ⇒ UNKNOWN.
    Caso D (constante). Pieza de ancho fijo w anclada en poste + t, en claros de ancho S uniforme.
      A ≡ B  ⇔  c = (S − 2t)/2.  GeometricEvidence mide c = w/2 ⇒ con S − 2t ≠ w no hay equivalencia ⇒ UNKNOWN.
    ```

11. **Defensas adicionales.** El registro rechaza declaraciones para piezas de familias con fila `UNKNOWN` por holgura
    unilateral (hoy, rol `Tope`) —defensa adicional, **no** mecanismo principal— y para bloques con parametro de
    estiramiento en el eje declarado sin la fuente de relacion del punto 6(b). Ningun componente acepta una diferencia de
    planes como evidencia de centro (G-M15).
12. **Censo.** G3 produce la `GeometricEvidence` de cada bloque candidato (CT-36) y, despues, el diagnostico de piezas con
    diferencia de mano por kind y vista (CT-06). El Owner confirma (O-3) en G3 y G9; su confirmacion no sustituye a la
    evidencia geometrica.
13. **Sin declaracion aplicable:** la vista no es equivalente ⇒ el rack falla cerrado (E6).

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
- **CT-17 audita en G3, por pieza** de cada builder de vista admisible: **span de anclas** (las dos referencias que la
  pieza cubre, si las hay), **offset unilateral** (`δ_izq`, `δ_der`), **direccion de estiramiento** del parametro a lo
  largo del eje reflejado, **origen local del bloque** (en que extremo de su geometria cae la insercion) y la **simetria
  declarada** si la hay. Clasifica cada coordenada como **simetrica** (derivada de ambos extremos o del centro) o
  **anclada** (desde un poste, troquel o extremo, con offset de un lado).
- Un `δ` que **no** se reparte simetricamente (`δ_izq ≠ δ_der`; por ejemplo `LONGITUD = span + δ` anclada a un lado) es
  `UNKNOWN` ⇒ `FAIL_CLOSED`, **cualquiera sea el rol** de la pieza. Una declaracion de simetria no lo corrige (§11.3
  punto 10).
- Cada anclaje asimetrico produce una fila `UNKNOWN` con `FAIL_CLOSED` o una regla explicita aprobada por el Owner.
  Nunca un parche silencioso: una contradiccion material con V4 abre **Proposal V5**.

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
| CT-06 | **PlanPlacementEvidence** (diagnostico, despues de CT-36): piezas cuya mano difiere entre `F ∘ Π(D)` y `Π(μD)` por kind y vista, en el marco local tras `T(−BlockOrigin)`. **No** infiere ni justifica centros: con la declaracion aprobada por `GeometricEvidence` verifica `A ≡ B` y, si difieren, registra el desplazamiento. Incluye la caracterizacion negativa: los fixtures de T-M40 siguen no equivalentes | Core |
| CT-07 | `WithDesign` materializa `PalletTolerance` vinculada (hueco actual del camino `EditX`) | Core |
| CT-08 | `SelectiveDesviadorPlan.CellKey` colapsa N−1/N en el Dinamico | Core |
| CT-09 | `BracingPanel.ResolveDiagonalDirection` por paridad | Core |
| CT-10 | Ausencias de Push Back compuesto: inicio, interior y final dibujan distinto | Core |
| CT-11 | Marco del brazo Cantilever por lado y familia de seccion | Core |
| CT-12 | Semantica de `MountingFace` respecto del run (S-10, D-03b) | Core |
| CT-13 | `SelectiveAuthoredAuthority` distingue un documento reflejado de su origen | Core |
| CT-14 | Censo de `[CommandMethod(` = 33 y unicidad de nombres (linea base) | Core |
| CT-15 | **Topes**: holgura del tope Selectivo en frontal y planta (AR-01) y del **tope posterior de Push Back** en frontal posterior y planta (anclaje, `ElevationMirrored`, `LONGITUD`); predicado exacto de «tope activo»; **activo por defecto** (sin `OffCells` y con `PieceId` en blanco o desconocido dibuja en toda celda), por lado en compuesto, y `(NINGUNO)` con mascara latente | Core |
| CT-16 | **Semantica observable de `RackDuplicationPlan`**: textos completos, orden de grupos, orden de referencias, avisos y errores, sobre codigo intacto (§5.1) | Core |
| CT-17 | **Auditoria de anclajes por pieza** de `SelectiveFrontalBuilder`, `SelectivePlantaBuilder`, `SelectiveTopePlan`, `SelectiveParrillaPlacement`, `SelectiveTarimaPlacement`, `SelectiveDesviadorPlan`, `SelectiveSafetyPlacement`, `DynamicSystemFrontalBuilder`, `DynamicSystemPlantaBuilder`, `DynamicViewDecorations`, `DynamicSafetyMultiViewBuilder`, `DynamicForkliftDefensePlan`, `DynamicEntranceGuidePlan`, `PushBackSystemFrontalBuilder`, `PushBackSystemPlantaBuilder`, `PushBackRearTopeBuilder`, `PushBackCompositeFrontal`, `PushBackTarimaPlacement`, `PushBackPalletProjection`, `PushBackBootPlan`, `PushBackDefensePlan`, `CantileverViewPlanBuilder`, `LateralHeaderLayoutBuilder` y `PlantaHeaderLayoutBuilder`: span de anclas, offset unilateral, direccion de estiramiento, origen local del bloque y simetria declarada; cada coordenada a lo largo del eje reflejado, simetrica o anclada; `δ` no repartido ⇒ `UNKNOWN` sin importar el rol (§11.7) | Core |
| CT-18 | **Origin**: los materializadores crean `Origin = 0`, `RackCloner` copia el de la fuente y ningun comando lo comprueba hoy | guarda de texto (sin AutoCAD en CI, ADR-0003) |
| CT-19 | **Efectos de `EnsureForPlan`**: IF-1..IF-7 de §9.5 | guarda de texto; los efectos reales son evidencia del Owner en G9 |
| CT-20 | **Filas de parrilla y de tarimas**: condicion `count·frente > span` en claros completos, reparto apilado y ausencia de desborde en tramos (`PalletFit`); **Push Back**: `FrontalRow` con `calles·lane > larguero` (override por nivel), simple y compuesto por lado (P-03c, PC-14b) | Core |
| CT-21 | N-S02: UI, BOM, round-trip e involucion respecto de la forma normal | Core + UI si hace falta |
| CT-22 | N-S06: ceros finales, listas cortas, valores `≤ 0` distintos de `0.0`, listas largas e involucion | Core |
| CT-23 | N-H02: fisica, BOM, UI, round-trip y relacion con `IsStandard`/`IsException`/`StandardBaselineId` | Core + UI si hace falta |
| CT-24 | Criterio de acreditacion del registro por kind (PV-5) | Core |
| CT-25 | Censo de claves de parametros por kind y vista **con `CatalogBlockParameters` y `CatalogBlockManifest` (I-19)** | Core |
| CT-26 | Mapa de invocaciones de builders de `EditX` —costuras de **Actualizar e Insertar** por kind y vista admitida (C2-2a, C2-2b)— y del ejecutor en las vistas admitidas (linea base de G-M9), y caminos del estado del editor (C2-4) | Core + `tests/RackCad.UI.Tests` |
| **CT-27** | **Estabilidad con dos o mas estados del registro**: N-S02 (con `Dep(i)` la copia deja de ser espejo tras el cambio; con todas las celdas gobernantes con override conmuta en ambos) y **tarimas y parrillas** con los casos A y B de §6.5.5 (S-17d, S-15d), con cota autoritativa si existe y sin ella | Core |
| **CT-28** | **Autoridad del ancho**: fondo maestro, columna vacia, `W_M(i,R) ≤ 0` con niveles, **cambio de gobernante** y predicado `Dep(i)` (§6.5.1) | Core |
| **CT-29** | **S-14 asociado**: espacio de indices del desviador bajo dos estados del registro cuando el ajuste del medio frente cambia | Core |
| **CT-30** | **Enumeracion `A_k`** de todas las vistas admisibles por kind y fixture, y conmutacion en cada una | Core |
| **CT-31** | **Linea base del cierre transitivo**: tipos y claves `(kind, ruta)` alcanzados desde las raices de §6.7, cortes con su razon, instantanea de enums y descriptores con su cierre efectivo | Core |
| **CT-32** | **Metadata desconocida**: payload por kind (raiz y anidados) y **exterior** (sobre y envoltorio): clave desconocida vacia y no vacia, portador de la allowlist, `CustomProperties` antes y despues de I-54, duplicado por mayusculas, clave desconocida del envoltorio y miembros retirados | Core |
| **CT-33** | **Fuentes dinamicas, anonimas o anotativas**: clasificacion ST-17 frente a ST-2 | guarda de texto; efecto real en M-23 |
| **CT-34** | **Historia de `View` vacia del Selectivo**: `View` introducida en `6023572` antes que `Section` en `4d2ca82`; decodificacion de produccion | Core + registro documental |
| **CT-35** | **V-RT-STORE por kind, separado de C-2**: `Miembros(LB)` e idempotencia del store sobre fixtures con y sin miembros opcionales | Core |
| **CT-36** | **`GeometricEvidence`** (antes de CT-06): sonda sobre la biblioteca registrada que mide la geometria de la definicion por estado de parametros, la compara con su reflexion respecto de eje y centro, lee la parametrizacion para la forma afin (§11.3 punto 6b) y canoniza `DefinitionGeometryHash`; casos C y D | sonda local con AutoCAD (evidencia registrada en `docs/automation/evidence/`); Core consume los registros |
| **CT-37** | **Diferencial de estabilidad 9b** sobre el corpus: dos o mas estados del registro por descriptor (con cota autoritativa si existe; sin cota: cero, negativos y grandes), fondo maestro distinto y cambio de gobernante; toda decision `ALLOW` en `R_a` conmuta en `R_b`; lema de definicion | Core |
| **CT-38** | **Cierre efectivo por descriptor**: variar el valor del descriptor y comparar por reflexion el sistema resuelto; todo simbolo que cambia fuera del cierre declarado ⇒ RED | Core |
| **CT-39** | **Precedencia de fuente C1..C5** (§4.1): objetos no RackCad con escala, `Normal`, `Origin` o MINSERT no canonicos se ignoran sin abortar; bloque de usuario con racks anidados (ST-2b); payload ilegible (ST-2c) | Core (clasificacion pura) + guarda de texto de la captura |
| **CT-40** | **Read-set por observaciones** (§9.2.1): acreditacion usada, variables leidas, cambio irrelevante frente a cambio observado | Core |
| **CT-41** | **Coexistencia con I-53**: ningun tipo de I-52 en `RackCad.Application.Systems.Shared`; C-08 y C-10 de I-53 siguen verdes al reconciliar | Core |

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
| T-M08 | Normalizaciones N-S02, N-S06, N-S07 y N-H02 con las obligaciones 1..9 de §6.5 y la 9b de §6.5.5 | G5 |
| T-M09 | V-BOM-1 y V-BOM-2 por kind sobre fixtures | G5 |
| T-M10 | Portadores de la allowlist de §6.4 identicos (CA-1..CA-5), incluidos `PropertyValues` con `Kind` desconocido, `DimensionViews` y `CustomProperties` | G5 |
| T-M11 | Autoridad authored por kind (divergente, ilegible, igual) | G5 |
| T-M12 | Conmutacion `Π(μD) ≡ F ∘ Π(D)` por (kind, vista admisible) | G6 |
| T-M13 | Autoridad de plan = builders existentes para las mismas entradas | G6 |
| T-M14 | Plan del espejo: E1..E8 sin mutacion, grupos multi-rack con `NewRackId` distintos, sin `WithDesign` | G6/G7 |
| T-M15 | Guardas del Plugin (§12.3) con RED demostrado por violacion temporal no commiteada | G7 |
| T-M16 | Censo 33 → **34** (guarda reapuntada con motivo) | G7 |
| T-M17 | C-2: autoridad frente a `EditX` → Actualizar (C2-1 = C2-2a) | **G6** |
| **T-M17b** | C-2 con **Insertar** desde la copia (C2-1 = C2-2b), o fixture compartido con la guarda G-M9 de costura comun | **G6** |
| T-M18 | Cruce I-50: `DimensionViews` exacto a traves del espejo (centinelas `-8`, `13`, `null`) — **incondicional** | G5 (portador) y G8 (cruce final) |
| T-M19 | Cruce I-49: entradas `expression` sobreviven y la autoridad efectiva integrada se invoca | G8 (si I-49 integrada) |
| T-M20 | Cruce I-54: propiedades de alcance Rack sobreviven exactas | G8 (si I-54 integrada) |
| T-M21 | Huella de §9.2: cambio de cualquier campo entre SNAPSHOT, PREPARE y MUTATE ⇒ fail-closed | G4 (comparador puro) y G7 |
| T-M22 | Canonizacion de `View`/`Section` por kind (§3.7), incluida la `View` vacia del Selectivo con `Section ≥ 0` ⇒ FAIL_CLOSED | G5 |
| T-M23 | `MissingInstances.Count > 0` ⇒ excepcion en el camino de I-52 y ninguna copia | G6 (guarda G-M11 + build Plugin; efecto real en M-19) |
| T-M24 | Sustrato real por kind (lectura y escritura con la autoridad existente) | G5 |
| T-M25 | Criterio PV-5: Selectivo con registro no acreditable ⇒ E7 con y sin vinculos; kinds sin vinculos ignoran el registro | G5 |
| T-M26 | La autoridad de plan con `RackName` dibuja `<base> - espejo` en la anotacion de nombre | G6 |
| T-M27 | Equivalencia con una declaracion de simetria que aplica (desplazamiento con centro en el marco local tras `T(−BlockOrigin)` y volteo de mano) e integridad del registro: bloque en `blocks.csv`, `GeometricEvidence` presente y coherente, `CharacterizedDomain` respetado y `DefinitionGeometryHash` exigido | G6 |
| T-M28 | Deteccion `UK` de topes Selectivo (S-13/S-13b), **tope posterior de Push Back activo (P-04, P-05, PC-08)** —incluida la configuracion por defecto sin `OffCells` y con `PieceId` en blanco—, parrilla desbordada (S-15x) y Auto con baseline (H-06); `(NINGUNO)` inactivo con mascara remapeada (P-05b, PC-08b) | G5 |
| T-M29 | C-2 con el ejecutor de variables del Selectivo (C2-1 = C2-3) | **G6** |
| T-M30 | C-2 con el estado del editor → sistema (C2-1 = C2-4), en Core y en UI cuando pase por la ventana | **G6** |
| **T-M31** | **Obligacion 9**: con dos estados del registro, S-02b detectada (`MC`) cuando `Dep(i)`; S-02a (`RN`) conmuta en ambos cuando todas las celdas gobernantes tienen `BeamLengthOverride` | G5 |
| **T-M32** | S-14c `UK` cuando el espacio de indices del desviador depende de un medio frente con `Dep(i)`; dos estados del registro | G5 |
| **T-M33** | **V-VIEW-ALL**: se enumeran todas las vistas admisibles sin leer el dibujo; una vista admisible **no seleccionada** que no conmuta hace fallar el rack | G6 |
| **T-M34** | Tarimas: S-17c `UK` con fila desbordada; S-17b sin desborde conmuta; S-17d `UK` con `Dep(i)` sin `DEP-ROW`; **Push Back**: P-03c y PC-14b `UK` con fila desbordada; P-03b y PC-14a conmutan | G5/G6 |
| **T-M35** | **V-META**: desconocido no vacio en el payload ⇒ `UK`; clave exterior desconocida no vacia (sobre o envoltorio) ⇒ `UK`; clave desconocida vacia, portadores de la allowlist y miembros retirados no bloquean; duplicado ambiguo de `CustomProperties` ⇒ `UK` | G5 |
| **T-M36** | **Cierre transitivo**: desde las raices de §6.7, cada clave `(kind, ruta)` alcanzada y cada descriptor clasificados exactamente una vez; tipo, miembro, corte o descriptor nuevo sin clasificar ⇒ RED; un tipo nuevo agregado a un DTO existente aparece sin tocar ninguna lista de tipos | G5 |
| **T-M37** | ST-17: referencia dinamica, anonima o anotativa con payload RackCad ⇒ E4 con mensaje propio; sin payload ⇒ ST-2 | G7 |
| **T-M38** | **V-RT-STORE** (`Miembros(LB) ⊆ Miembros(M)` e idempotencia), independiente de C-2 | G5 |
| **T-M39** | V-BOM-1 frente a V-BOM-2: un fixture donde difieren las precisiones demuestra que ninguna sustituye a la otra | G5 |
| **T-M40** | **Negativa de simetria generalizada**: con cualquier declaracion que el registro admita siguen **no** equivalentes (1) fixture de rol `Tope` (AR-01 y tope posterior de Push Back); (2) pieza de otro rol con parametro de longitud y `δ` unilateral; (3) pieza de ancho fijo anclada a un lado (caso D); (4) forma afin candidata `a0 = −δ/2` (caso C); (5) centro conveniente deliberadamente incorrecto. El registro rechaza la entrada por falta de `GeometricEvidence` coherente o la equivalencia falla; el rechazo por rol `Tope` es defensa adicional | G6 |
| **T-M41** | Read-set por observaciones (§9.2.1): un cambio irrelevante no aborta; un cambio de una observacion (acreditacion, variable leida, dependencia transitiva si I-49) aborta en PREPARE y en MUTATE | G4 (comparador) y G7 |
| **T-M42** | `RepresentabilityReport` lleva las tres dimensiones separadas por fila (clase, evidencia, disposicion) | G5 |
| **T-M43** | **V-DEP**: toda fila `ALLOW`/`CANONICALIZE` declara `EffectiveInputs`; las que intersecan un cierre de descriptor tienen entrada V-DEP con prueba o motivo; fila sin declaracion ⇒ RED | G5 |
| **T-M44** | Instantanea de enums alcanzados y de decodificacion de secciones; valor nuevo, renombrado o retirado ⇒ RED | G5 |
| **T-M45** | `DEP-ROW` con dos estados del registro: casos A y B ⇒ S-17d y S-15d `UK`; con overrides que cubren la fila ⇒ `ALLOW` y conmuta en ambos estados | G5 |
| **T-M46** | Precedencia de fuente C1..C5: objetos no RackCad con escala, `Normal`, `Origin` o MINSERT no canonicos se ignoran sin abortar; ST-2b con aviso especifico; ST-2c ⇒ E2 | G4 (clasificacion pura) y G7 (captura) |
| **T-M47** | Desde G5, ninguna fila del `RepresentabilityReport` lleva `EvidenceStatus = G3_PENDING` (regla de cierre de G3) | G5 |

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
| G-M9 | Mapa de invocaciones de builders y servicios de dibujo de `EditX` (Actualizar **e Insertar**) y del ejecutor en las vistas admitidas (linea base CT-26); si C2-2a y C2-2b comparten costura, demuestra que ambos caminos la siguen llamando; nace en G6 y sigue en G8 |
| G-M10 | Ningun literal de simetria de bloques fuera del registro tipado de §11.3; toda entrada referencia un registro de `GeometricEvidence` existente |
| G-M11 | El materializador de I-52 comprueba `MissingInstances` tras `CreateSystemBlock` y lanza; `LateralHeaderDrawer.cs` sin cambios |
| G-M12 | Si no se extrae un SNAPSHOT compartido: `RackDuplicarCommands.cs` identico al de la base y guardas de I-51 sin reapuntar |
| **G-M13** | Las tablas de clasificacion de miembros y de descriptores viven en Application, una por kind junto a su reflector; el Plugin no contiene clasificacion ni `switch` por kind |
| **G-M14** | Ningun tipo de I-52 en el namespace `RackCad.Application.Systems.Shared` (coexistencia con C-08 y C-10 de I-53); I-52 no modifica `SharedFoundationInspection` |
| **G-M15** | Ningun componente del espejo acepta una diferencia de planes como evidencia de centro: el comparador y el registro no exponen API que derive `c` de planes |

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
| M-9 | Fail-closed sin mutacion: lateral seleccionada, cama, MINSERT RackCad, escala no uniforme, fuente espejada, esquina, brazo C sencillo, Selectivo con topes, **Push Back con la configuracion por defecto (tope posterior activo)**, definicion con BASE movida, seccion invalida, cabecera de plantilla con Auto, **medio frente con `PalletTolerance` vinculada sin overrides**, **tarimas desbordadas**, **tarimas o parrillas dependientes sin `DEP-ROW` (casos A y B)**, **tarimas frontales de Push Back desbordadas**, **metadata exterior desconocida** |
| M-10 | UNDO: se registra que revierte y que ocurre con lo importado (evidencia; UNKNOWN hasta entonces) |
| M-11 | Dinamico frontal (salida y entrada) + planta |
| M-12 | Push Back compuesto con el tope posterior **inactivo en ambos lados** (`(NINGUNO)` en A y en B, o todas las celdas apagadas; el estado por defecto es activo y falla cerrado): frontales 0..3 + planta |
| M-13 | Cantilever frontal + planta |
| M-14 | Cabecera lateral + planta con diagonales explicitas y postes asimetricos |
| M-15 | Confirmacion visual de los bloques declarados simetricos, con eje y centro, **despues** de su `GeometricEvidence` (O-3); la confirmacion no sustituye a la evidencia |
| M-16 | Anotaciones y cotas legibles, respetando la politica de `DimensionViews` de I-50 |
| M-17 | **Regresion de RACKDUPLICAR** (G4 edita `RackDuplicationPlan.cs`): multiples origenes y destinos, mensajes y fail-closed de I-51 |
| M-18 | Selectivo legado con frontal `Section = −1`: la copia sale con `Section = 0` y `RACKEDITAR` la trata igual |
| M-19 | Bloque de biblioteca ausente (con una biblioteca de prueba, nunca la del Owner): RACKMIRROR falla sin crear copias |
| M-20 | Efectos de infraestructura tras un fallo en PREPARE o MUTATE: que definiciones, capas o estilos importados quedan (evidencia) |
| **M-21** | Rack con una vista admisible **no seleccionada** que no conmuta (fixture de prueba): falla cerrado sin mutacion y el mensaje nombra la vista |
| **M-22** | Payload con miembro desconocido en el diseño (build de prueba): falla cerrado; con una clave desconocida no vacia en el sobre: **tambien** falla cerrado; con `CustomProperties`: el espejo la conserva |
| **M-23** | Rack convertido por el usuario en bloque dinamico o anonimo (BEDIT): falla cerrado con mensaje propio, no se ignora |
| **M-24** | Push Back recien creado con la configuracion por defecto: RACKMIRROR falla cerrado sin mutacion y el mensaje nombra el tope posterior y como desactivarlo |
| **M-25** | Selectivo de los casos A o B (§6.5.5): falla cerrado; con overrides que cubren la fila: la copia sigue siendo espejo tras `ChangeValue` |
| **M-26** | Seleccion con racks validos, un bloque de usuario no RackCad con escala no uniforme y un bloque de usuario que contiene racks: el espejo refleja los racks validos y avisa de los otros sin abortar |

---

## 14. Proceso y gates

### 14.1 Secuencia normativa (sustituye a la de V3)

```text
Coordinador + Arquitecto de acuerdo sobre V4 (misma version, exact-SHA)
→ rebase / reconciliacion sobre el main vigente         (solo docs; conflicto del indice de ADR: 0035, 0036, 0037, 0038)
→ freeze por SHA exacto + re-check exact-SHA            (Coordinador y Arquitecto)
     si el rebase cambia contenido normativo material → vuelve a revision
→ Owner O-1                                              (alcance real, §1.2)
→ G3 characterization                                    (ADR-0036 NO es precondicion)
     si G3 contradice materialmente V4 (alcance, ADR, regla de reflexion o arquitectura) → Proposal V5
     si no                                               → Owner acepta ADR-0036 (O-4)
→ G4 implementation                                      (ADR-0036 aceptado ES precondicion)
```

### 14.2 Tabla de gates

Cada gate declara entrada, archivos permitidos, RED esperado, PASS requerido, parada y evidencia por SHA exacto. Ningun
gate arranca sin la orden explicita del Coordinador y sin el preflight de paralelas (§15.3). **Cualquier contradiccion
de una caracterizacion o de una prueba con el contrato congelado ⇒ STOP → Proposal V5**; nunca un parche silencioso.

| Gate | Entrada | Archivos permitidos | RED esperado | PASS requerido | Parada | Evidencia |
|---|---|---|---|---|---|---|
| **G2** (este) | revision de V3 | `docs/**` | — | consenso de Coordinador y Arquitecto sobre la misma version (V4); despues, rebase/reconciliacion, freeze por SHA, re-check normativo y **O-1** del Owner; contrato reescrito; hallazgos laterales en `ideas-futuras.md` | desacuerdo material ⇒ Proposal V5 | SHA acordado, SHA de freeze + CI |
| **G3** Characterization | freeze de G2 + O-1 aceptada (**sin** ADR aceptado) | `tests/RackCad.Tests/**` (caracterizaciones y fixtures nuevos); `tests/RackCad.UI.Tests/**` si una caracterizacion lo exige; `docs/automation/evidence/**` (registros de `GeometricEvidence`); la sonda de CT-36 vive fuera de `src/` en la ubicacion que fije el Coordinador al abrir G3 (si exige `src/`: STOP y decision) | ninguno: CT-01..CT-41 pasan sobre produccion intacta | Core completo (UI si se toco); **`GeometricEvidence` (CT-36) antes que `PlanPlacementEvidence` (CT-06)**; cierre transitivo y enums (CT-31); tabla de `c`; auditoria de anclajes por pieza (CT-17); semantica observable de I-51; decodificacion de secciones e historia de `View`; lineas base de store; topes, con el tope posterior de Push Back activo por defecto (CT-15); estabilidad 9b con casos A/B, cotas y cambio de gobernante (CT-27, CT-28, CT-37, CT-38); S-14 asociado; enumeracion `A_k`; tarimas del Selectivo y de Push Back (CT-20); metadata desconocida del payload y del exterior (CT-32); precedencia de fuente (CT-39); read-set (CT-40); costuras de Actualizar e Insertar (CT-26); coexistencia con I-53 (CT-41); fuentes dinamicas/anonimas; V-RT-STORE separado de C-2; **regla de cierre: ninguna fila `G3_PENDING`** (§7.0) | contradiccion material con V4 ⇒ **STOP → Proposal V5** | SHA + conteos + CI + registros de evidencia. Tras PASS: se pide **O-4** |
| **G4** Geometria y nucleo neutral | G3 PASS **y ADR-0036 aceptado por el Owner** | `src/RackCad.Application/Geometry/*`; nucleo neutral y fachada en `src/RackCad.Application/Persistence/` (incluye `RackDuplicationPlan.cs`, **con aviso previo a I-49**); pruebas | T-M01..T-M05, T-M21 y T-M41 (comparadores puros) en rojo sobre andamiaje | verdes; T1–T15 de I-51 y CT-16 identicos (textos, orden, avisos, errores); `RackDuplicarCommands.cs` sin cambios; tolerancia de escala fijada y registrada; **sin cambio observable de RACKDUPLICAR** | cambio observable de RACKDUPLICAR | SHA + conteos + CI |
| **G5** Reflectores y representabilidad | G4 | contrato, registros y tablas de clasificacion en `src/RackCad.Application/Mirror/`; reflector por kind en `src/RackCad.Application/Systems/<Kind>/` y `RackFrames/` (nunca en `Systems/Shared`); registro de simetrias con referencias a `GeometricEvidence`; declaracion de invariancia de metadata vacia; allowlist de portadores; autoridad V-DEP; pruebas. Divisible en G5a Selectivo, G5b Dinamico y Push Back, G5c Cantilever y cabecera | T-M06..T-M11, T-M18 (portador), T-M22, T-M24, T-M25, T-M28, T-M31, T-M32, T-M34, T-M35, T-M36, T-M38, T-M39, T-M42, T-M43, T-M44, T-M45 y T-M47 en rojo | verdes; sustrato real; topes Selectivo y Push Back `UK`; H-06 `UK`; obligaciones 9 y 9b; V-DEP; V-META del payload y del exterior; cierre transitivo con enums; sin `G3_PENDING`; `WithDesign` ausente | una fila `R` o `RN` resulta no equivalente ⇒ STOP → Proposal V5 | SHA por subgate + CI |
| **G6** Planes, materializador y **C-2** | G5 | autoridades de plan por kind en Application; `SystemBlockWriter.cs` (`CreateInTransaction`); materializador nuevo en `src/RackCad.Plugin/Systems/Shared/`; pruebas Core y `tests/RackCad.UI.Tests/**` para C-2 | T-M12..T-M14, **T-M17**, **T-M17b**, T-M23, T-M26, T-M27, **T-M29**, **T-M30**, T-M33, T-M40 y G-M9 en rojo | conmutacion y V-VIEW-ALL verdes; `MissingInstances` = fallo duro; **C2-1, C2-2a, C2-2b, C2-3 y C2-4 GREEN en CI** (Core y UI) con G-M9, nombre logico, estado efectivo, `View` y `Section` canonicas; builders y drawers sin cambio de comportamiento. **G6 no cierra sin C-2** | necesitar cambiar un builder o un drawer existente; divergencia de C-2 ⇒ decidir C-1 con Coordinador y Arquitecto antes de cerrar G6 | SHA + CI + build Debug de Plugin |
| **G7** Comando RACKMIRROR | G6 cerrado (**C-2 verde**, incluido C2-2b) | `src/RackCad.Plugin/RackMirrorCommands.cs` (nuevo) y SNAPSHOT propio; archivos de guardas; `SelectiveEditorOpenTests.cs` (censo); `src/RackCad.UI/RackCommandReference.cs`. `RackDuplicarCommands.cs` **solo** si se decide extraer un SNAPSHOT compartido (§5.2) | T-M15, T-M16, T-M37, T-M41 (MUTATE), T-M46 (captura), G-M1..G-M8 y G-M10..G-M15 en rojo por violacion temporal | guardas verdes; censo 34; build Debug de Plugin; sin extraccion, archivos y guardas de RACKDUPLICAR intactos (G-M12) | C-2 no verde ⇒ G7 no arranca; debilitar una guarda de I-51 o de I-53 | SHA + conteos + CI |
| **G8** Cruces y continuidad | G7 | pruebas cruzadas; continuidad de G-M9; produccion de `RACKEDITAR` **solo** si se activo C-1 con orden del Coordinador | T-M18 (cruce final), T-M19 y T-M20 en rojo cuando apliquen | cruces verdes con lo integrado (I-49, I-50, I-53, I-54); reconciliacion final con autoridades integradas | un cruce exige tocar §20.3 | SHA + CI |
| **G9** Conformidad, Candidato y Owner | G8 | ninguno de produccion salvo correcciones | — | rebase final si `main` avanzo; Core y UI locales; builds Debug; CI 4/4 exacto; cobertura; M-1..M-26 (M-17 incluido); O-3 confirmado | cualquier fallo ⇒ vuelta al gate que corresponda | SHA Candidato + corridas + veredicto del Owner |
| **G10** Documentacion e integracion | G9 | `docs/**`, `README.md` | — | WORKFLOW 4.5: cierre documental, merge `--no-ff`, CI del `MERGE_SHA`, cobertura, limpieza | CI post-merge rojo ⇒ correccion en la rama | SHAs de cierre y merge |

---

## 15. Coordinacion con iniciativas paralelas

### 15.1 Tips observados en el preflight de V4

`git fetch --all --prune` el 2026-09-13 antes de redactar V4, y de nuevo antes de publicarla. Tips del **segundo** fetch
(los del primero entre parentesis):

```text
origin/main                                        = f8deb675c6d1ef0e64693b157d69c4cc170d7b24 (Merge I-50; sin avance desde V2)
I-52 feature/rackmirror-espejo-semantico           = 545c2229de8d7850e03981d85aced0ac63c24040 (V3; base 46fcac2; sin rebase)
I-49 architecture/motor-expresiones-parametricas   = 364d6c06e44273a63a7b6f6509daf357611ea77a (antes 2beec61; G3B.2: Owner ACEPTA
                                                     ADR-0038 en edafade y Consensus Freeze en 364d6c0; base f8deb67;
                                                     solo docs; G4 READY / NOT STARTED)
I-53 feature/cabeceras-configurables-multidestino  = 7de424e4f0f69f6e2fb18fd7c2e018479aab3fbe (antes 4e00a27; G4: consumidor Selectivo
                                                     del nucleo compartido; base f8deb67; produccion ADITIVA: 840 lineas
                                                     añadidas y 0 borradas en Application y pruebas, sin UI ni Plugin;
                                                     ADR-0037 aceptado en su rama)
I-54 architecture/propiedades-personalizadas       = 26ca923492576185b753d6dbf2a852969df2accf (antes 8bc991c; Proposal V5 = V4 + C-F3;
                                                     base f8deb67; sin consenso ni produccion; solo docs)
I-50                                               = integrada en main; rama remota retirada
```

**Cambios productivos de paralelas frente a `main`.** Solo I-53, y son aditivos: G3 (`4e00a27`) añade
`src/RackCad.Application/Systems/Shared/HeaderBatchCodes.cs`, `HeaderBatchOutcome.cs`, `HeaderBatchPlan.cs` y
`HeaderConfigurationSnapshot.cs`; G4 (`7de424e`) añade `SelectiveHeaderAddress.cs`, `SelectiveHeaderBatchPlanner.cs`,
`SelectiveHeaderBatchPreparation.cs`, `SelectiveHeaderBatchRequest.cs`, `SelectiveHeaderResolution.cs` y
`SelectivePostTargets.cs`, y **solo añade miembros** a tres archivos existentes: `SelectivePostGeometry.PostPeralteFor`
(`PostPeralteAt` y la formula de postes no cambian), `SelectiveCabeceraHeightReview.OfDestinations` y
`SelectiveEditorState.ApplyHeaderBatch`; mas pruebas nuevas. Cero lineas borradas y ningun llamador de UI o Plugin.
Ninguno cambia el comportamiento de una autoridad de I-52 (§19): se registran **sin bloquear**. Un avance solo
documental de una paralela no obliga a Proposal V5.

Entre la base `46fcac2` y `main @ f8deb67` no cambian `RackDuplicationPlan.cs`, `RackDuplicarCommands.cs`,
`RackEnvelopeRestamp.cs`, `RackEmbedComposer.cs`, `SystemBlockWriter.cs`, `LateralHeaderDrawer.cs`,
`CantileverViewMaterializer.cs`, `BlockLibraryImporter.cs`, `RackProjectStore.cs`, `RackProject.cs` ni los builders de §8.1;
si cambian, por I-50, los archivos con lineas de portador `DimensionViews` y los emisores de cotas. Todas las citas de V4
estan ancladas contra `main @ f8deb67`.

**Observacion documental (resuelta).** En `2beec61`, `docs/adr/README.md` de I-49 listaba 0038 como `aceptado` mientras
el ADR decia `propuesto`; en `edafade` el Owner acepto ADR-0038 y ambos coinciden. Queda solo como registro (§16).

### 15.2 Cruces

| Paralela | Cruce | Tratamiento |
|---|---|---|
| **I-50** (**integrada** en `main @ f8deb67`) | `DimensionViews` en `SelectivePalletDesignDocument.cs`, `DynamicRackSystemDocument.cs`, dominio y resolvedores (C-01..C-15); politica de cotas en `SelectiveDimensions.cs`, `DynamicViewDecorations.cs`, `DimensionViewPolicy.cs`; ventanas ricas; ADR-0035 aceptado | `DimensionViews` es requisito normativo (§10.3; CA-5); T-M18 incondicional; firmas de anotaciones de C-2 y V-VIEW-ALL con la politica de I-50; I-52 no toca esos archivos |
| **I-49** (Consensus Freeze; ADR-0038 **aceptado**; `364d6c0`) | `expression` en `PropertyValues`; `PlanReadSet` con observaciones (D19); dominio del consumidor declarado por el descriptor como dato y aplicado al valor efectivo (P24.5); su condicion de parada cubre `RackDuplicationPlan`, restamp, `SelectiveAuthoredAuthority` y el store Selectivo; registra que G4 de I-52 toca `RackDuplicationPlan.cs` | **Coordinacion obligatoria con I-49 antes de G4** sobre `RackDuplicationPlan.cs`; T-M19; P24.8 (el espejo conserva `expression` sin interpretarla). Si integra antes: su `PlanReadSet` es la fuente de RS-3 (§9.2.1), su dominio es la cota autoritativa `b` de V-DEP (§6.5.5) y su descriptor vinculable se clasifica en la guarda (§6.7). El freeze de I-49 exige que su prueba de supervivencia de `expression` (P24.8) se escriba contra `RackDuplicationPlan` y el restamp vigentes en `main`. I-52 no implementa un segundo motor |
| **I-53** (G4 Selectivo; `7de424e`) | Contrato congelado y ADR-0037 aceptado en su rama; G3 añade `HeaderConfigurationSnapshot`, `HeaderBatchPlan<T>`, `HeaderBatchOutcome<T>` y codigos en `RackCad.Application.Systems.Shared`, sin llamadores productivos, con las guardas C-08 (todo `Header*` de ese namespace en `Roots`) y C-10 (sin tipos `Coverage`, `Executor`, `Engine`, ... en todo ese namespace); copias de configuracion de cabecera sobre campos ya persistidos | I-52 **no** crea tipos en ese namespace y no modifica `SharedFoundationInspection` (G-M14, CT-41); todo tipo o miembro persistido de cabecera que I-53 introduzca queda en RED en el cierre transitivo hasta clasificarse (§6.7); `SelectiveEditorState.ApplyHeaderBatch` es un productor nuevo de `PostCabeceras`, `ExtraFondoPostCabeceras` y `PostPeraltes` sin llamadores productivos: cuando I-53 lo conecte, C2-4 re-mide el estado del editor y N-S06/N-S07 ya cubren lo que escribe (relleno `0.0`, filas explicitas); serializar si C-1 llegara a tocar comandos de `RACKEDITAR` |
| **I-54** (Proposal V5; `26ca923`) | Miembro `CustomProperties` (`JsonElement?`) heredado por `Compose`; **D-21 sin cambio** (el espejo cumple: A y despues B); T-GRD-02 7 → 8; T-GRD-03 convive con G-R5; residuales preexistentes F-14a (lectura) y F-14b (reescritura), precisados en V5 por C-F3, y regla F de `Compose` | `CustomProperties` es portador **por nombre** (CA-4, §6.4); F-14a ⇒ E2 (ST-2c); F-14b y regla F ⇒ E8 en P10 (§9.4); ID-5 intacto; quien integre despues re-apunta T-GRD-02; T-M20 |
| Documental | Filas de ROADMAP, final de `ideas-futuras.md`, `docs/adr/README.md` (0035 en `main`; 0036 de I-52; 0037 de I-53; 0038 de I-49), censo de comandos 33 → 34 | Quien integre despues conserva todas las filas y entradas en orden numerico y re-mide los censos |

### 15.3 Protocolo antes de G3, G5, G7 y del Candidato

`git fetch --all --prune`; `git diff --name-only origin/main...origin/<rama>` de I-49, I-53 e I-54 (y de cualquier
iniciativa nueva); leer sus Proposals y contratos en el SHA exacto; buscar `docs/adr/0036-*` y citas de «ADR-0036» en
**todos** los refs (§16); comprobar los archivos de §19; si `main` avanzo, rebase segun WORKFLOW antes de escribir codigo. Ademas: si I-49 integra, re-medir la cota
autoritativa de V-DEP (§6.5.5) y la autoridad del read-set (§9.2.1); si I-53 integra, re-medir C-08/C-10 y el cierre
transitivo (§6.7); si I-54 integra, re-medir CA-4 y T-GRD-02; buscar tambien 0037 y 0038 en el indice de ADR.

---

## 16. ADR-0036: numeracion y aceptacion

- Archivo: `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`, estado **`propuesto`**, con fila en el indice.
  Actualizado en este gate con las correcciones de V4.
- Congela: espejo semantico por copia; `μ_k` canonica por kind; admisibilidad por vista con seccion canonica y
  verificacion de **todas** las vistas admisibles; fail-closed; reflectores en Application sobre el sustrato real;
  prohibicion de normalizaciones que congelen valores dependientes de vinculos; metadata semantica desconocida ⇒
  fail-closed; autoridad de planes; colocacion canonica sin escala negativa, `Origin = 0` y sin bloques dinamicos,
  anonimos o anotativos; identidad nueva; portadores; atomicidad semantica e importacion best-effort; simetria con evidencia
  geometrica independiente, en el marco local tras `T(−BlockOrigin)` y ligada a la definicion real; obligacion 9b y V-DEP;
  allowlist de portadores y metadata exterior; cierre transitivo de cobertura; C-2 con Insertar; verificacion dinamica
  por rack; taxonomia en tres dimensiones con regla de cierre de G3.

**Protocolo de numeracion (vigente desde V2).**

1. Un numero de ADR queda **reclamado** por la primera publicacion **observable** de un archivo `docs/adr/NNNN-*` en un
   ref remoto.
2. **Registro de I-52:** ADR-0036 se publico por primera vez en `origin/feature/rackmirror-espejo-semantico` con el
   commit `0fc7032bf15d03e7d478bbd9350f156708621c9d` (`2026-09-12T19:59:36-06:00`; CI del push `34731908035`, creada el
   `2026-09-13T01:59:44Z`). En los preflights de V3 y V4 ningun otro ref contiene `docs/adr/0036-*`; las citas de «ADR-0036»
   en I-49, I-53 e I-54 se refieren a este ADR.
3. **Otros numeros observados:** ADR-0035 es de I-50 (aceptado e integrado en `main`). ADR-0037 lo publico I-53 en
   `d7f17ad` (`2026-09-12T21:24:06-06:00`); tras rebasar, su primera aparicion en el ref actual es `d0db698`
   (fecha de autor `21:24:06`, de commit `22:25:31`) y quedo **aceptado** en `5efaf7e`. ADR-0038 lo publico I-49 en
   `a160560` (`2026-09-13T00:43:42-06:00`) y el Owner lo **acepto** en `edafade` (`2026-09-13T01:58:43-06:00`); la
   inconsistencia de su indice observada en `2beec61` quedo resuelta (`[V4-D29]`). Todas son posteriores a 0036 y usan
   otro numero: **sin colision**.
4. **Antes de pedir la aceptacion del Owner**, I-52 busca 0036 en todos los refs: archivos `docs/adr/0036-*` y citas de
   «ADR-0036» o «adr/0036». Si existe una publicacion **anterior** de otro ADR-0036, I-52 renumera antes de la
   aceptacion; una publicacion posterior no obliga a I-52 a renumerar.
5. Una vez ADR-0036 sea **`aceptado`**, **no** se renumera (`adr/README.md`: un aceptado es inmutable).
6. Dos ADR **aceptados** con el mismo numero ⇒ **STOP** y escalado al Owner.

**Aceptacion (vigente desde V3).** Solo el Owner (O-4). **No** es precondicion de G3; se pide **despues de G3**, si G3
no contradice materialmente V4, y **es precondicion de G4**. Mientras tanto el ADR sigue `propuesto` y puede corregirse si G3
abre Proposal V5. `docs/adr/README.md` no cambia en este gate: titulo, estado y numero del indice siguen siendo
correctos.

---

## 17. Riesgos abiertos

| # | Riesgo | Mitigacion |
|---|---|---|
| R-1 | Censo de mano: muchas piezas podrian diferir; sin `GeometricEvidence` independiente (y, para formas afines, sin relacion derivada de la parametrizacion del bloque) kinds enteros pueden quedar en fail-closed | CT-36 antes que CT-06; O-3; M-15; cambio material de alcance ⇒ Proposal V5 |
| R-2 | Un Dinamico dibujado solo en lateral no puede reflejarse | Mensaje E5 explicito; ALT-L como futuro |
| R-3 | V-RT-STORE puede rechazar payloads legitimos si la idempotencia del store no es exacta | CT-35 caracteriza por kind |
| R-4 | C-1, si llegara a activarse, toca comandos calientes que I-49 e I-53 tambien tocan | C-2 por defecto; C-1 solo con evidencia; serializar |
| R-5 | Semantica de «estandar» de cabecera tras normalizar Auto (H-06): la mayoria de las cabeceras de plantilla quedan fuera | G5 con CT-23; fail-closed mientras tanto; declarado en §2.3 y O-1 |
| R-6 | `MountingFace` sin semantica confirmada (S-10, D-03b) | CT-12; si no se confirma ⇒ FAIL_CLOSED |
| R-7 | Comandos transparentes de AutoCAD entre SNAPSHOT y MUTATE | Huella y read-set de §9.2 en PREPARE y MUTATE |
| R-8 | Numeracion de ADR con varias iniciativas que necesitan ADR | Protocolo de §16 |
| R-9 | Cantilever con brazos C/L sencillos o arriostramiento estructural asimetrico queda fuera | Clasificacion MC/UK; futuro con metadato de mano |
| R-10 | Rendimiento de V-VIEW-ALL y V-BOM en selecciones grandes (dos planes por vista admisible, dos BOM por rack) | Medir en G7; builders puros |
| R-11 | Selectivos con topes y **la mayoria de los Push Back** (tope posterior activo por defecto) inutilizables hasta O-2/O-2B | Fail-closed verificable antes de mutar; declarado en O-1; M-12 y M-24 con fixtures explicitos |
| R-12 | La auditoria de anclajes (CT-17) puede degradar muchas filas `R` + `G3_PENDING` a `UK` | Regla de cierre de G3; Proposal V5 si el cambio es material; nunca parche silencioso |
| R-13 | G4 toca el planificador de RACKDUPLICAR, validado por el Owner en I-51 | CT-16 primero; fachada con semantica observable identica; M-17; aviso a I-49 |
| R-14 | V-META deja fuera racks escritos por un build mas nuevo con metadata semantica, ahora tambien en el sobre y el envoltorio | Fail-closed deliberado: sin transformador no hay espejo fiel; futuro con declaraciones de invariancia o entradas CA-6 |
| R-15 | C-2 sobre ventanas WPF exige pruebas de UI mas costosas, ahora en G6 | `tests/RackCad.UI.Tests` permitido en G3 y G6; LC-UI |
| R-16 | La importacion de biblioteca puede dejar dependencias tras un fallo y su UNDO no esta demostrado | §9.3 y §9.5 lo declaran; M-10 y M-20 lo registran |
| **R-17** | Medios frentes con `PalletTolerance` vinculada y sin overrides quedan fuera | S-02b declarado; ALT-ABS como futuro |
| **R-18** | La guarda de cobertura obliga a clasificar cada miembro nuevo de cinco familias de tipos | Coste asumido: es la unica defensa ejecutable contra copias sin permutar |
| **R-19** | Aceptar el ADR despues de G3 añade una ronda del Owner entre G3 y G4 | Aceptado por el Coordinador: evita un ADR aceptado que G3 podria refutar |
| **R-20** | La obligacion 9b deja fuera racks con tarimas o parrillas cuyo ajuste depende de `PalletTolerance` vinculada sin overrides que las cubran (hoy no hay cota autoritativa) | S-15d/S-17d declarados; si I-49 integra, su dominio de consumidor puede ampliar lo admisible; M-25 |
| **R-21** | La huella canonica `DefinitionGeometryHash` puede no ser determinista en AutoCAD | CT-36 en G3; sin huella determinista la declaracion no aplica (fail-closed); cambio material ⇒ Proposal V5 |
| **R-22** | Nombres de tipos del espejo que choquen con las guardas C-08/C-10 de I-53 | carpeta `RackCad.Application.Mirror`; G-M14; CT-41 |
| **R-23** | El cierre transitivo obliga a clasificar tipos y enums que V3 no listaba | Coste asumido: es la unica defensa ejecutable contra copias sin regla; T-M36 y T-M44 |

---

## 18. Desacuerdos y decisiones

### 18.1 Decisiones registradas (no son consenso)

| # | Tema | Resolucion vigente | Donde |
|---|---|---|---|
| DA-1 | Convergencia con `RACKEDITAR` | C-2 por defecto con los productores reales (Actualizar **e Insertar**), CI, G-M9 y M-4/M-5; **GREEN en G6**; C-1 solo con evidencia | §8.5 |
| DA-2 | Declaracion de simetrias | Registro tipado unico en `RackCad.Application.Mirror`; `assets/` y catalogos fuera; eje y centro en marco local tras `T(−BlockOrigin)`; `GeometricEvidence` independiente y ligada a la definicion real; `PlanPlacementEvidence` despues | §11.3 |
| DA-3 | Politica post-commit | Sin `Regen`; un unico `Regen` solo con evidencia del Owner | §8.6 |
| DA-4 | Recorte del primer corte | Se mantiene; O-1 despues del freeze tecnico y antes de G3, con los hechos explicitos de §1.2 | §2 |
| DA-5 | `IsStandard`/`StandardBaselineId` | UNKNOWN → FAIL_CLOSED hasta G5 | §6.5.4 |
| DA-6 | Numeracion de ADR | Precedencia del primer ref remoto publicado | §16 |
| CR-V2-1 | Registro | Criterio de produccion; I-52 no lo relaja | §10.2 |
| CR-V2-2 | Centro de simetria | Marco local tras `T(−BlockOrigin)`; desde V4, con evidencia geometrica independiente | §11.3 |
| CR-V2-3 | Tope posterior Push Back | Superado por evidencia de codigo: `UNKNOWN → FAIL_CLOSED`; activo por defecto | §7.3 |
| Proceso V3 | Aceptacion del ADR | Despues de G3; precondicion de G4 | §14.1, §16 |
| CR-V3-1 | Simetria (V4-01) | `GeometricEvidence` antes que `PlanPlacementEvidence`; CT-06 no infiere centros; afin solo con relacion derivada; `Tope` como defensa adicional | §11.3 |
| CR-V3-2 | Estabilidad (V4-02) | Obligacion 9b; V-DEP como autoridad; `W_lb` solo con cota autoritativa real | §6.5.5 |
| CR-V3-3 | Metadata exterior (V4-04) | Allowlist cerrada de portadores; clave desconocida no vacia ⇒ `UNKNOWN` | §6.4 |
| CR-V3-4 | Cobertura (V4-05) | Cierre transitivo con enums; coexistencia con C-08/C-10 de I-53 | §6.7 |
| CR-V3-5 | Push Back (V4-06) | Alcance real declarado; O-1 con hechos explicitos | §1.2, §2 |
| CR-V3-6 | Proceso (V4-10) | Contradiccion material de G3 ⇒ Proposal V5 | §7.0, §14.1 |

### 18.2 Desacuerdos abiertos

**Ninguno material.** Quedan diferidos con fail-closed los asuntos de §0.3. Para la atencion del Arquitecto, sin ser
desacuerdo:

- La ligadura a la definicion real por `DefinitionGeometryHash` es una precision de V4 (`[V4-D28]`) que endurece
  «definicion REAL»: si G3 no logra una huella determinista, las filas con diferencia de mano quedan fail-closed y el
  alcance podria cambiar materialmente (Proposal V5).
- La exigencia (b) de la forma afin (§11.3 punto 6) puede no ser alcanzable si la sonda no puede leer la parametrizacion
  del bloque dinamico; entonces la declaracion solo cubre los estados medidos. Afecta a largueros y demas piezas estiradas.
- La mascara latente del tope inactivo (P-05b, PC-08b) es precision de V4 (`[V4-D30]`).
- DEP-05..DEP-07 se declaran estables por construccion y quedan `G3_PENDING` con CT-37 y CT-38.
- V-VIEW-ALL puede reducir el alcance util de racks multi-fondo si alguna frontal de fondo no conmuta.

---

## 19. Condiciones de parada

- Implementar sin la orden explicita del gate: **prohibido**.
- Una caracterizacion o prueba contradice **materialmente** el contrato congelado (alcance, ADR, regla de reflexion o
  arquitectura), o una fila `R` o `RN` resulta no equivalente: **STOP → Proposal V5**.
- Necesitar cambio de schema, de sobre o de Xrecord, o de `RackEnvelopeRestamp.cs`, `PushBackMirror.cs`, `WithDesign`, un
  drawer o builder existente o el registro de variables: **detenerse**; nueva revision de Arquitecto y ADR.
- Un adaptador necesita una costura del store distinta de `WithSourceMetadataFrom`: **detenerse** (§6.2).
- Una paralela modifica materialmente `RackDuplicationPlan.cs`, `RackEmbedComposer.cs`, `RackEmbedDocument.cs`,
  `RackProjectDocument.cs`, `RackProjectStore.cs`, `SystemBlockWriter.cs`, `LateralHeaderDrawer.cs`,
  `CantileverViewMaterializer.cs`, `RackEnvelopeRestamp.cs`, `BlockLibraryImporter.cs`, `DynamicRackSystemResolver.cs`,
  `UsableProjectVariablesRegistry.cs`, `SelectiveLinkedPropertyKernel.cs`, los builders de §8.1, los stores authored o los
  descriptores vinculables: **detenerse** antes de editar y reportar SHA y diff. Un avance solo documental o una
  infraestructura aditiva no conectada se registra sin bloquear.
- Aparece un tipo, miembro, valor de enum o descriptor vinculable alcanzado por el cierre transitivo sin clasificar:
  **RED / STOP** (§6.7).
- CT-38 encuentra un simbolo efectivo fuera del cierre declarado de un descriptor: **RED / STOP** (§6.5.5).
- Se intenta admitir una declaracion de simetria sin `GeometricEvidence` independiente, o derivar un centro de planes:
  **prohibido** (G-M15).
- Se usa en `DEP-ROW` una cota inferior que no es autoritativa: **prohibido**.
- Un tipo de I-52 aparece en `RackCad.Application.Systems.Shared`: **RED** (G-M14).
- Aparece un parametro dinamico con semantica de lado sin regla: fail-closed y registro para Proposal V5.
- G7 intenta consumir la autoridad sin C-2 verde (incluido C2-2b): **prohibido**.
- Abrir G4 sin ADR-0036 aceptado o con alguna fila `G3_PENDING`: **prohibido**.
- Debilitar una guarda de I-51 o de I-53 para facilitar el cambio: **prohibido**.

---

## 20. Archivos (sin cambiarlos en este gate)

### 20.1 Produccion probable (G4–G8)

| Capa | Archivo o area |
|---|---|
| Application | `Geometry/Transform2D.cs` (`ReflectionAboutLine`) y valor de colocacion junto a el |
| Application | `Persistence/RackDuplicationPlan.cs` (fachada, con aviso a I-49) + nucleo neutral nuevo en `Persistence/` |
| Application | carpeta nueva del espejo `src/RackCad.Application/Mirror/` (namespace `RackCad.Application.Mirror`; **nunca** `Systems/Shared`): contrato de reflector, registro, plan del espejo, decodificacion de vistas y `A_k`, equivalencia, declaracion tipada de simetrias con referencias a `GeometricEvidence`, declaracion de invariancia de metadata, allowlist de portadores, autoridad V-DEP comun y tablas de cobertura del exterior |
| Application | `Systems/Selective/`, `Systems/Dynamic/`, `Systems/PushBack/`, `Systems/Cantilever/`, `RackFrames/`: reflector, tabla de clasificacion y autoridad de plan por kind |
| Plugin | `RackMirrorCommands.cs` (nuevo) y SNAPSHOT propio; materializador en `Systems/Shared/`; `Systems/Shared/SystemBlockWriter.cs` (`CreateInTransaction`); calculo de `DefinitionGeometryHash` en PREFLIGHT y PREPARE (§11.3) |
| Plugin | `RackDuplicarCommands.cs` **solo** si se extrae un SNAPSHOT compartido; comandos de `RACKEDITAR` **solo** si se activa C-1 |
| UI | `RackCommandReference.cs` (ayuda) |

### 20.2 Pruebas

Caracterizaciones CT-01..CT-41; pruebas T-M01..T-M47 (con T-M17b); guardas G-M1..G-M15; registros de `GeometricEvidence` en
`docs/automation/evidence/`; `SelectiveEditorOpenTests.cs` (censo 34);
guardas de I-51 reapuntadas en `SelectiveDuplicationFailClosedTests.cs` solo si se extrae el SNAPSHOT; pruebas de UI
en `tests/RackCad.UI.Tests` para C-2 (G6) y las caracterizaciones de UI (G3).

### 20.3 NO TOCAR sin nueva revision de Arquitecto

`RackEnvelopeRestamp.cs`, `RackCloner.cs`, `PushBackMirror.cs`, `SelectivePalletDesign.cs`,
`SelectivePalletDesignDocument.cs` (incluido `WithDesign`), DTO y stores de todos los kinds, `RackProject.cs`,
`LateralHeaderDrawer.cs`, `CantileverViewMaterializer.cs`, `BlockLibraryImporter.cs`, builders de plan,
`ProjectVariables/*` salvo consumo de lectura, `Bom/*` salvo consumo, `Catalogs/*` salvo consumo (I-19),
`KindHandlers/*`, editores WPF, `assets/`, catalogos, biblioteca de bloques, `.github/`, ADR aceptados, y los archivos de
I-53 en `src/RackCad.Application/Systems/Shared/Header*` con sus guardas (`HeaderBatchContractTests.cs`,
`HeaderConfigurationFixtures.cs`).

---

## 21. Estado

```text
G0 = ACCEPTED        G1 = ACCEPTED (Discovery)
G2 = V4 EN REVISION  (este documento + ADR-0036 propuesto actualizado + docs/automation/decisions/I-52.md)
     V1 (0fc7032), V2 (0445718) y V3 (545c222) = historial;
     revision de V3: Architect: CHANGES REQUIRED — PROPOSAL V4
G3 = NOT OPEN        (requiere consenso sobre V4, rebase/reconciliacion, freeze, re-check y O-1)
G4+ = NO INICIADO    (G4 requiere ademas G3 sin contradiccion material, ninguna fila G3_PENDING y ADR-0036 aceptado)

Proposal Version = V4
Coordinator = REVIEW REQUIRED
Architect = REVIEW REQUIRED
Consensus = NOT REACHED
ADR-0036 = PROPOSED
G3 = NOT OPEN
Implementation = BLOCKED

SUBSTANTIVE IMPLEMENTATION: BLOCKED
```

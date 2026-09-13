# I-52 — Proposal V5: RACKMIRROR, espejo semantico de uno o varios racks (ID16)

> # ⚠ PROPOSAL V5 — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```text
> Proposal Version = V5
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Consensus = NOT REACHED
> ADR-0036 = PROPOSED
> O-1 = PENDING
> G3 = NOT OPEN
> Implementation = BLOCKED
> ```
>
> ```text
> Schema            = SIN CAMBIO (ningun DTO, sobre ni Xrecord cambia en I-52)
> BASE auditada     = 46fcac2b071929d2bd5b07aa28373941417f74a8   (base de la rama; V5 se redacta SIN rebase)
> CITAS             = archivo:linea contra origin/main @ f8deb675c6d1ef0e64693b157d69c4cc170d7b24 (I-50 integrada);
>                     las citas de paralelas llevan su SHA
> CLAIM_SHA         = 55281769d5e9317ad25500e6d3f5f8a849f37279
> BOOTSTRAP_SHA     = 7ee79756d2b93b4eded660cb73310ef3edde32ff
> Discovery G1      = 339b3abd238a3a7a42d60c9edd45a1500a776f62   (se conserva)
> Proposal V1       = 0fc7032bf15d03e7d478bbd9350f156708621c9d   (historial)
> Proposal V2       = 04457183fc2d7dcda5d6c1b988a4789ed4ea2f8f   (historial)
> Proposal V3       = 545c2229de8d7850e03981d85aced0ac63c24040   (historial)
> Proposal V4       = 8e2ae4fb9606c246bcd5d666a8f10e35f042a901   (historial; CI 34748944360 4/4)
> Revision Arq. V4  = «Architect: CHANGES REQUIRED — PROPOSAL V5»; sin BLOCKER; HIGH AR4-01; MEDIUM AR4-02;
>                     LOW AR4-03..AR4-14; cambios V5-01..V5-14 (registro: docs/automation/decisions/I-52.md §26)
> Coordinador       = orden de V5 con V5-01..V5-20 vinculantes (registro §27)
> Paralelas         = §15.1 (preflight de V5)
> Estado de gates   = G0 ACCEPTED · G1 ACCEPTED (Discovery) · G2 V5 EN REVISION · G3 NOT OPEN
> ```
>
> Este documento **no** autoriza produccion, **no** modifica V1..V4 ni el Discovery y **no** es el contrato vinculante.
> El contrato se reescribe en el freeze de G2 (§14.1).
>
> **Nota de transparencia.** V1..V4, sus revisiones de Arquitecto y esta V5 las redacto el mismo agente en roles
> distintos. V5 re-verifica en codigo cada correccion y declara las precisiones que encontro al redactarla
> (`[V5-D26]`..`[V5-D30]`).

---

## 0. Reconciliacion V4 → V5

V1 (`0fc7032`), V2 (`0445718`), V3 (`545c222`), V4 (`8e2ae4f`) y el Discovery (`339b3ab`) **se conservan sin cambios**.
**Donde V4 y V5 difieran, manda V5.** Las marcas `[V4-D01]`..`[V4-D30]` siguen vigentes salvo donde una marca
`[V5-Dnn]` las modifica o sustituye (§0.4).

### 0.1 Estados de reconciliacion

| Estado | Significado |
|---|---|
| **CLOSED BY V5** | V5 fija el contrato; lo que quede es verificacion ordinaria de gate |
| **DEFERRED TO G3/G5 WITH FAIL-CLOSED** | El dato exacto se obtiene en G3 o G5, y mientras tanto existe un fail-closed inequivoco que no condiciona la arquitectura |
| **OPEN MATERIAL** | Impide presentar V5. **Debe ser NONE** |

### 0.2 Matriz `[V5-D01]`..`[V5-D30]`

En la columna «Origen», `AR4-nn` es el hallazgo de la revision de V4 y `V5-nn` la precision vinculante del Coordinador
en la orden de este gate. La revision numera sus propios cambios de otra forma: V5-01 = AR4-01, V5-02 = AR4-02 y
V5-03..V5-14 = AR4-03..AR4-14 (§0.6).

| Marca | Origen | V4 | Correccion V5 | Estado | Donde |
|---|---|---|---|---|---|
| `[V5-D01]` | AR4-01 · V5-01 | Solo evidencia estatica registrada en G3 (lista finita de estados o intervalo con relacion afin): no operable para bloques dinamicos con estados continuos | **STATE-EVALUATED GEOMETRIC EVIDENCE** como ruta primaria: para cada pieza de cualquier vista admisible cuya equivalencia requiera aceptar un cambio de mano, se evalua la geometria real del **estado concreto** (definicion real, vector de parametros dinamicos, marco local de insercion), se aplana recursivamente y se prueba la simetria de reflexion; salida `GeometrySymmetryResult` | CLOSED BY V5 | §11.3 |
| `[V5-D02]` | AR4-01 · V5-01 | Biblioteca registrada como evidencia | Fuente de la definicion: la del dibujo si el bloque ya existe; si no, la de la biblioteca candidata en PREFLIGHT; tras `EnsureForPlan`, PREPARE re-verifica la definicion REAL que quedo en el dibujo (`DuplicateRecordCloning.Ignore` puede conservar una local) | CLOSED BY V5 | §11.3 punto 3 |
| `[V5-D03]` | AR4-01 · V5-01 | — | Sonda **sin mutacion** del DWG del usuario (solo lectura o `Database` lateral); produce datos planos; Application no referencia `Autodesk.*`; `GeometrySymmetryResult` no es schema persistido | CLOSED BY V5 | §11.3 puntos 4-5; G-M3, G-M16 |
| `[V5-D04]` | AR4-01 · V5-02 | Sin politica de clases de entidad | Politica explicita: soportadas Line, Arc, Circle, Polyline con bulges, Ellipse, Spline, Solid, Trace, Hatch SOLID por contorno y BlockReference anidado recursivo; fail-closed Region, 3DSolid, proxy/custom, raster, OLE, hatch no reducible, Text, MText, Attribute y Dimension; ciclo de anidados, anidado dinamico no evaluable y correspondencia ambigua ⇒ `UNKNOWN` | CLOSED BY V5 | §11.3 punto 6 |
| `[V5-D05]` | AR4-01 · V5-03 | Centro declarado y verificado solo en estados caracterizados | El centro se obtiene como **candidato** geometrico y se verifica comparando el multiconjunto de primitivas original con el reflejado respecto de `(eje, centro)`, con las tolerancias del contrato. La caja envolvente nunca es prueba: una pieza asimetrica con caja simetrica **falla** (T-M40). Casos C y D siguen normativos | CLOSED BY V5 | §11.3 puntos 7-9 |
| `[V5-D06]` | AR4-01 · V5-04 | `DefinitionGeometryHash` como ligadura normativa | Pasa a `TraceFingerprint`: solo trazabilidad, deteccion de obsolescencia y enlace PREFLIGHT↔PREPARE. Si se conserva, incluye estado dinamico evaluado, clases soportadas, geometria aplanada, anidados, transformacion y firma de parametros, en serializacion canonica (nunca orden de iteracion del BTR); bits exactos para trazabilidad y tolerancias para la equivalencia | CLOSED BY V5 | §11.3 punto 10 |
| `[V5-D07]` | AR4-01 · V5-05 | Forma afin con relacion derivada y `CharacterizedDomain`; contradiccion entre los puntos 6 y 11 | La forma afin **sale** del camino normativo del primer corte; solo optimizacion futura con metadata autoritativa; I-52 no interpreta grafos de acciones de bloques dinamicos. Las contradicciones heredadas desaparecen porque ya no hay estados caracterizados ni relacion afin | CLOSED BY V5 | §11.3 punto 11 |
| `[V5-D08]` | AR4-01 · V5-06 | Evidencia de un estado sin relacion con `ChangeValue` | Si un parametro dinamico necesario para la evidencia depende directa o transitivamente de una propiedad vinculable, la evidencia de un solo estado **no** satisface PV-7. ALLOW exige prueba sobre todo el dominio autoritativo, autoridad integrada con propiedad geometrica universal, o parametro demostrado independiente del vinculo; si no ⇒ `UNKNOWN`. Sin universalidad por muestras finitas. DEP-09 (largueros) y DEP-10 (postes); en `main` el comportamiento es conservador | CLOSED BY V5 | §6.5.5, §11.3 punto 12, S-26, S-27 |
| `[V5-D09]` | AR4-01 · V5-07 | La sonda de CT-36 «fuera de `src/`» sin mas | G3 demuestra viabilidad con `tools/`, harness externo, evidencia del Owner en AutoCAD o artefactos en `docs/automation/evidence/`, sin sonda productiva en `src/`. La sonda productiva nace en el Plugin en el gate que la necesite (previsiblemente G6/G7). Riesgo de rendimiento para G7 | CLOSED BY V5 | §11.3 punto 13, §14.2, R-24 |
| `[V5-D10]` | AR4-01 · V5-07 | — | Viabilidad real de la sonda en AutoCAD 2025: evaluacion de bloques dinamicos, `Database` lateral, anidados, clases de entidad, definicion local frente a biblioteca, obsolescencia y rendimiento preliminar | DEFERRED TO G3/G5 WITH FAIL-CLOSED: sin sonda viable toda pieza con cambio de mano es X-17a; si G3 la descarta ⇒ Proposal V6 o reduccion de alcance | CT-36 |
| `[V5-D11]` | AR4-02 · V5-08 | O-1 con diez hechos y «demas filas» | O-1 se reescribe como **envolvente maxima de alcance** (§1.3) con cada limitacion material nombrada; O-1 no promete que G3 verifique todos los candidatos; si G3 reduce materialmente la envolvente ⇒ Proposal V6 y O-1 se reevalua | CLOSED BY V5 | §1.2, §1.3 |
| `[V5-D12]` | AR4-03 · V5-09 | Lema «`DEP-ROW` ⇒ `W_lb > 0`» | Soundness reescrita: `W(i,R) ≥ W_M(i,R) ≥ W_lb(i)` cuando gobierna el maestro, y el fallback solo puede aumentar `W` | CLOSED BY V5 | §6.5.5 |
| `[V5-D13]` | AR4-04 · V5-10 | Estado de evidencia implicito en las filas reclasificadas | Al cerrar G3: verificada = clase + `G3_VERIFIED` + disposicion; fallo demostrado = `UNKNOWN` o `REQUIRES_MODEL_CHANGE` + `CODE_SUPPORTED` o `G3_VERIFIED` segun el origen + `FAIL_CLOSED`; sin estado implicito; cambio material ⇒ Proposal V6 | CLOSED BY V5 | §7.0 |
| `[V5-D14]` | AR4-05 · V5-11 | P-05b/PC-08b sin politica de indices fuera de rango | Indice de `OffCells` fuera del dominio semantico valido actual ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED` (P-05c, PC-08c); no se conserva opacamente | CLOSED BY V5 | §7.3, §7.4 |
| `[V5-D15]` | AR4-06 · V5-12 | Cobertura y V-META por reflexion CLR | La autoridad es el **contrato de serializacion real**: `JsonTypeInfo` de las mismas `JsonSerializerOptions` del store, con modifiers, converters, `PropertyNameCaseInsensitive`, `[JsonIgnore]` y `[JsonExtensionData]`. Nombres de miembro que colisionan bajo el matching real ⇒ `UNKNOWN` | CLOSED BY V5 | §6.4, §6.7 |
| `[V5-D16]` | AR4-07 · V5-12 | Miembros retirados no declarados | Registro de miembros retirados conocidos (RM-1..RM-4 mas los dos de Cantilever declarados por el store); payload legado con uno de ellos ⇒ `FAIL_CLOSED` con mensaje; sin limpieza automatica. Barrido historico hecho sobre los tipos persistidos alcanzables; el corpus exhaustivo por ruta JSON queda para CT-32 | CLOSED BY V5 (registro y politica); corpus: DEFERRED TO G3/G5 WITH FAIL-CLOSED | §6.4, CT-32, X-19 |
| `[V5-D17]` | AR4-08 · AR4-10 · V5-13 | Instantanea de enums sin la semantica del converter | Todo enum alcanzado usa el contrato real del converter (`JsonStringEnumConverter` admite enteros): valor numerico no definido ⇒ `UNKNOWN` salvo invariante forward-compatible declarado y probado (hoy solo `DimensionViewVisibility`, portador CA-5). La instantanea añade `HeaderBlockRole` y `CantileverVisualRole` | CLOSED BY V5 | §6.7, X-20 |
| `[V5-D18]` | AR4-09 · V5-14 | CA-3 sin condicion | `PropertyValues` sigue siendo portador de almacenamiento, pero todo `PropertyId` conocido tiene descriptor con efecto semantico en la cobertura y en V-DEP; descriptor nuevo ⇒ RED; `PropertyId` desconocido sigue la autoridad efectiva vigente y falla cerrado si no puede interpretarse | CLOSED BY V5 | §6.4 CA-3 |
| `[V5-D19]` | AR4-11 · V5-15 | CT-37 sin BOM | CT-37 compara tambien V-BOM-1 y V-BOM-2 entre estados del registro cuando el descriptor pueda afectar el BOM o la geometria que lo alimenta | CLOSED BY V5 | §6.5.5, CT-37 |
| `[V5-D20]` | AR4-12 · V5-16 | Model Space como C3 sin declarar la decision | Declarado: un RackCad fuera de Model Space no entra como fuente, se ignora con aviso y no aborta por las demas restricciones de candidato (continuidad con PD-7 de I-51); un candidato en Model Space si aplica las restricciones estrictas | CLOSED BY V5 | §4.1 |
| `[V5-D21]` | AR4-13 · V5-17 | «`(NINGUNO)`» como literal | Literal real `"(ninguno)"` con comparacion sin distinguir mayusculas (`PushBackDefaults.cs:44`; `PushBackRearTope.cs:37-38`) | CLOSED BY V5 | §2.3, §7.3 |
| `[V5-D22]` | AR4-14 · V5-18 | Paralelas de V4 | HEAD real: I-49 `82aa61b`, I-53 `f271fd5` (G6 Dinamico aditivo), I-54 `3866252` (ADR-0039 aceptado); ADR 0035..0039 | CLOSED BY V5 | §15, §16 |
| `[V5-D23]` | V5-19 | ADR-0036 con `GeometricEvidence` estatica | Decision 13 y consecuencias actualizadas; sigue `propuesto`; aceptacion despues de G3 y antes de G4 | CLOSED BY V5 | ADR-0036 |
| `[V5-D24]` | V5-20 | — | Matriz: X-17a/X-17b/X-17c, S-26, S-27, X-19/X-19b, X-20, X-21, P-05c, PC-08c, PRE-13/PRE-14. G3: CT-36 (viabilidad por estado), CT-37 (con BOM y parametros dinamicos), CT-32 (contrato y corpus de retirados), CT-42..CT-44 | CLOSED BY V5 | §7, §12 |
| `[V5-D25]` | Coordinador (este gate) | Contradiccion material de G3 ⇒ Proposal V5 | Contradiccion material de G3 ⇒ **Proposal V6**; si G3 reduce la envolvente de O-1, O-1 se reevalua | CLOSED BY V5 | §1.2, §7.0, §14.1 |
| `[V5-D26]` | Precision al redactar | Mensaje unico de remedio para retirados | `HighEndBeamPeralte` (RM-4) esta en la raiz de `PushBackDesignDocument`, cuyo `[JsonExtensionData]` lo conserva al reescribir (`PushBackDesignDocument.cs:75-76`): RACKEDITAR → Actualizar **no** lo elimina, asi que el mensaje de remedio seria falso. Dos variantes: R-A (el store lo descarta: remedio) y R-B (el store lo conserva: sin remedio) | CLOSED BY V5 | §6.4, X-19, X-19b |
| `[V5-D27]` | Precision al redactar | DEP-07: «ningun simbolo a lo largo del eje reflejado» | Sigue siendo cierto para posiciones, pero `LONGITUD` de los postes de la frontal es la altura del poste (`SelectiveFrontalBuilder.cs:88-91`, `:489`), que depende de las separaciones calculadas con `VerticalClearance` (`SelectiveGeometryResolver.cs:310`, `:318`, `:327`): el **estado dinamico** depende del vinculo ⇒ DEP-10 | CLOSED BY V5 | §6.5.5 |
| `[V5-D28]` | Precision al redactar | — | `LONGITUD` de los largueros es `W(i)` en frontal (`SelectiveFrontalBuilder.cs:131`, `:466`) y en planta (`SelectivePlantaBuilder.cs:393-394`) ⇒ DEP-09 | CLOSED BY V5 | §6.5.5 |
| `[V5-D29]` | Precision al redactar | Eje declarado en el registro | El eje a probar se deriva de `F` y de la rotacion de la instancia: rotacion multiplo de `π/2` (con `GeometryTolerance.Angle`) da el eje local X o Y; una rotacion oblicua ⇒ `UNKNOWN` | CLOSED BY V5 | §11.3 punto 7 |
| `[V5-D30]` | Precision al redactar · AR4-14 | `[V4-D29]` registro que el indice de ADR de I-49 en `2beec61` listaba 0038 como `aceptado` con el ADR `propuesto`, y AR4-14 cita una inconsistencia transitoria igual en el indice de I-54 en `d84f480` | Re-verificado con `git show`: en `a160560` y `2beec61` la fila 0038 del indice y el encabezado del ADR dicen `propuesto`, y desde `edafade` ambos dicen `aceptado`; en `d84f480` la fila 0039 y el ADR dicen `propuesto` (el registro de I-54: `propuesto; no aceptado`) y en `3866252` ambos dicen `aceptado`. **Ninguna de las dos inconsistencias se reproduce**: se corrige el registro. Sin efecto en la numeracion | CLOSED BY V5 (registro) | §15.1, §16 |

### 0.3 Resumen

```text
OPEN MATERIAL = NONE
```

Diferido con fail-closed inequivoco (no condiciona la arquitectura):

| Asunto | Fail-closed mientras tanto | Donde se resuelve |
|---|---|---|
| Viabilidad y rendimiento de la sonda por estado (CT-36) | sin sonda viable ⇒ X-17a en toda pieza con cambio de mano | G3; si no es viable ⇒ Proposal V6 o reduccion de alcance |
| Semantica de `MountingFace` (S-10, D-03b) | sin confirmacion de CT-12 ⇒ FAIL_CLOSED | G3 |
| Filas `R` con `EvidenceStatus = G3_PENDING` | V-VIEW-ALL por rack; regla de cierre de G3 (§7.0) | G3 |
| Corpus exhaustivo de miembros retirados por ruta JSON (CT-32) | miembro desconocido ⇒ V-META UNKNOWN | G3 |
| Estabilidad 9b de DEP-05..DEP-07 y cierres de descriptores | contraejemplo ⇒ STOP → Proposal V6 | G3 (CT-37, CT-38) |
| Evidencia universal para estados dinamicos dependientes de vinculos | sin ella ⇒ S-26/S-27 FAIL_CLOSED | futuro (autoridad o dominio autoritativo) |
| Cota autoritativa de propiedades vinculables | sin cota ⇒ solo overrides en `DEP-ROW` | si I-49 integra (§15.3) |
| Predicado exacto de «tope activo» (Selectivo y Push Back por lado) | cualquier celda dibujable ⇒ FAIL_CLOSED | G3 (CT-15) |
| Ids de placa Cantilever en BOM (K-09) | V-BOM distinto ⇒ FAIL_CLOSED | por rack |
| Linea base exacta de cada store | V-RT-STORE ⇒ FAIL_CLOSED | G3 (CT-35) |
| Valor de la tolerancia de escala | sin valor no hay implementacion (G4) | G4 |
| Semantica de `IsStandard` (H-06) | Auto con baseline ⇒ FAIL_CLOSED | G5 |
| Efectos reales de importacion y UNDO | UNKNOWN declarado; nada se promete | G9 (Owner) |
| Censo de parametros con lado (CT-25) | parametro sin regla ⇒ FAIL_CLOSED | G3 |
| Declaraciones de invariancia de metadata o de exclusion de entidades | ninguna ⇒ FAIL_CLOSED | futuro |
| Costuras de Insertar por kind (CT-26) | sin C2-2b verde G7 no arranca | G3 / G6 |

### 0.4 Estado de las marcas de V4 bajo V5

| Marca V4 | Estado en V5 |
|---|---|
| `[V4-D01]` | la separacion `GeometricEvidence`/`PlanPlacementEvidence` sigue; la ruta de evidencia la sustituyen `[V5-D01]`..`[V5-D05]` |
| `[V4-D02]` | sustituida por `[V5-D07]` (sin forma afin ni `CharacterizedDomain`) |
| `[V4-D03]` | vigente |
| `[V4-D04]` | vigente y ampliada por `[V5-D05]` (caja simetrica de pieza asimetrica) |
| `[V4-D05]` | sustituida por `[V5-D06]` y `[V5-D10]` |
| `[V4-D06]`, `[V4-D07]` | vigentes y ampliadas por `[V5-D08]` (DEP-09, DEP-10) |
| `[V4-D08]` | vigente; redaccion de soundness por `[V5-D12]` |
| `[V4-D09]` | vigente y ampliada por `[V5-D08]` (estado dinamico) |
| `[V4-D10]` | vigente y ampliada por `[V5-D19]` |
| `[V4-D11]` | vigente |
| `[V4-D12]` | vigente y ampliada por `[V5-D15]`, `[V5-D16]`, `[V5-D18]` |
| `[V4-D13]` | vigente y ampliada por `[V5-D15]`, `[V5-D17]` |
| `[V4-D14]` | vigente; paralelas por `[V5-D22]` |
| `[V4-D15]` | vigente; O-1 por `[V5-D11]` |
| `[V4-D16]` | vigente; Model Space por `[V5-D20]` |
| `[V4-D17]`, `[V4-D18]` | vigentes |
| `[V4-D19]` | sustituida por `[V5-D13]` |
| `[V4-D20]` | vigente |
| `[V4-D21]` | sustituida por `[V5-D22]` |
| `[V4-D22]` | sustituida por `[V5-D23]` |
| `[V4-D23]` | sustituida por `[V5-D25]` |
| `[V4-D24]`..`[V4-D27]` | vigentes |
| `[V4-D28]` | sustituida por `[V5-D02]` y `[V5-D06]` |
| `[V4-D29]` | corregida por `[V5-D30]`: la inconsistencia registrada no se reproduce |
| `[V4-D30]` | vigente para indices validos; fuera de rango por `[V5-D14]` |

### 0.5 Alias historicos de V2 (solo reconciliacion)

| Alias V2 | Equivalente (V3..V5) |
|---|---|
| `R-s` | `RepresentabilityClass = REPRESENTABLE` + `EvidenceStatus = G3_PENDING` + `OperationalDisposition = ALLOW` (sujeto a V-VIEW-ALL y V-DEP) |
| `C/F` | fila de seccion: `REPRESENTABLE` + `CODE_SUPPORTED` + `CANONICALIZE` (legado inequivoco) o `FAIL_CLOSED` (resto) |
| `LB` | `REPRESENTABLE` + `CODE_SUPPORTED` + `BASELINE_LIMITED` (normalizacion declarada del store); la metadata semantica desconocida ya no es `LB`, es `UNKNOWN` |
| `FC` | precondicion operativa con `FAIL_CLOSED`; no es clase de representabilidad (§7.9) |

### 0.6 Cobertura explicita

| Hallazgo o cambio | Marca |
|---|---|
| AR4-01 | `[V5-D01]`..`[V5-D10]`, `[V5-D27]`..`[V5-D29]` |
| AR4-02 | `[V5-D11]` |
| AR4-03 | `[V5-D12]` |
| AR4-04 | `[V5-D13]` |
| AR4-05 | `[V5-D14]` |
| AR4-06 | `[V5-D15]` |
| AR4-07 | `[V5-D16]`, `[V5-D26]` |
| AR4-08, AR4-10 | `[V5-D17]` |
| AR4-09 | `[V5-D18]` |
| AR4-11 | `[V5-D19]` |
| AR4-12 | `[V5-D20]` |
| AR4-13 | `[V5-D21]` |
| AR4-14 | `[V5-D22]`, `[V5-D30]` |
| Cambios V5-01..V5-14 de la revision | V5-01 = AR4-01 → D01..D10, D27..D29; V5-02 = AR4-02 → D11; V5-03..V5-14 = AR4-03..AR4-14 → D12..D22 (con D26 y D30) |
| Precisiones vinculantes del Coordinador (V5-01..V5-20 de este gate) | V5-01 → D01..D03; V5-02 → D04; V5-03 → D05; V5-04 → D06; V5-05 → D07; V5-06 → D08; V5-07 → D09, D10; V5-08 → D11; V5-09 → D12; V5-10 → D13; V5-11 → D14; V5-12 → D15, D16; V5-13 → D17; V5-14 → D18; V5-15 → D19; V5-16 → D20; V5-17 → D21; V5-18 → D22; V5-19 → D23; V5-20 → D24; proceso → D25 |

---

## 1. Decisiones de producto

### 1.1 Decisiones del Coordinador vigentes (desde V1)

Registro: `docs/automation/decisions/I-52.md` §4. Vinculan V5; **no** son consenso final.

| # | Decision | Precision V5 |
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
| **PDC-10** | Arquitectura para los seis kinds con bloque; el primer corte solo ejecuta vistas que exponen `μ_k`; la cama falla cerrado; sin ampliar schema | Aceptacion de alcance del Owner (O-1) despues del freeze tecnico y antes de G3, sobre la envolvente maxima de §1.3 |

### 1.2 Decisiones del Owner

| # | Decision | Contrato vigente | Momento |
|---|---|---|---|
| **O-1** | **Alcance del primer corte.** El Owner acepta la **envolvente maxima de alcance** de §1.3 y su politica fail-closed. O-1 **no** promete que G3 verifique todos los candidatos: acepta la envolvente y que todo lo que no se demuestre falla cerrado. Si G3 reduce materialmente la envolvente ⇒ Proposal V6 y O-1 se reevalua | envolvente de §1.3 | **pendiente**; se pide **despues del freeze tecnico** y **antes de G3**; **no** se pide en este gate |
| **O-2** | **Topes del Selectivo** | `UNKNOWN → FAIL_CLOSED`. **No** necesita decision ahora; una relajacion vuelve al Coordinador y al Owner | — |
| **O-2B** | **Tope posterior de Push Back** (activo por defecto) | `UNKNOWN → FAIL_CLOSED`. **No** necesita decision ahora; una relajacion vuelve al Coordinador y al Owner | — |
| **O-3** | **Evidencia de simetrias** | ningun bloque se asume simetrico; la prueba es `GeometrySymmetryResult` por estado evaluado (§11.3); la confirmacion del Owner **nunca** es la unica prueba | G3 (CT-36) y G9 (M-15, M-27) |
| **O-4** | **ADR-0036** | `propuesto` | se pide **despues de G3** si G3 no contradice materialmente V5; si la contradice ⇒ Proposal V6; **precondicion de G4** |

### 1.3 Envolvente maxima de alcance (para O-1)

La envolvente es el **maximo** que el primer corte puede reflejar. Dentro de ella, cada rack concreto pasa solo si todas
sus filas y verificaciones pasan; lo que no se demuestre falla cerrado **sin mutacion**.

**Dentro, como maximo:** Selectivo (frontal por fondo y planta), Dinamico (frontales de salida y entrada, y planta),
Push Back simple y compuesto (frontales `0..3` y planta), Cantilever (frontal y planta) y cabecera independiente
(lateral y planta), con la reflexion canonica de §3.6.

**Limitaciones materiales que el Owner acepta:**

| # | Limitacion | Resultado | Fila o regla |
|---|---|---|---|
| L-01 | Laterales de Selectivo, Dinamico, Push Back y Cantilever | fuera (E5) | S-20, D-15, P-10, PC-13, K-13 |
| L-02 | Cama | fuera siempre | F-01, F-02 |
| L-03 | Dinamico materializado solo como lateral | no se puede reflejar (E5) | D-15 |
| L-04 | Fuente ya reflejada con MIRROR nativo, o con una sola escala negativa: RACKMIRROR **no** corrige racks espejados antes con MIRROR | fail-closed (E4) | ST-9 |
| L-05 | MINSERT RackCad | fail-closed (E4) | ST-3 |
| L-06 | Escala no uniforme, `sz < 0` o `Normal` distinta de +Z | fail-closed (E4) | ST-7, ST-10, ST-5 |
| L-07 | Definicion con `Origin ≠ 0` (BASE/BEDIT movido) | fail-closed (E4) | ST-14 |
| L-08 | Fuente RackCad dinamica, anonima o anotativa | fail-closed con mensaje propio (E4) | ST-17 |
| L-09 | RackCad en Paper Space | ignorado con aviso | ST-4 |
| L-10 | Payload RackCad ilegible o inutilizable | la operacion entera se aborta (E2) | ST-2c |
| L-11 | Selectivo con frentes por fondo en esquina | fail-closed | S-01 |
| L-12 | Selectivo con medio frente dependiente de un vinculo, con postes distintos o que no resuelve | fail-closed | S-02b, S-02c, S-02d |
| L-13 | Selectivo con topes | fail-closed | S-13, S-13b |
| L-14 | Protector lateral explicito, desviador con off-cells asimetricos o dependientes, parrilla o tarimas desbordadas o dependientes | fail-closed | S-12, D-08a, D-10b, S-14c, S-15x, S-15d, S-17c, S-17d |
| L-15 | Tope posterior de Push Back **ACTIVE BY DEFAULT**: la mayoria de los Push Back fallan cerrado salvo `"(ninguno)"` o ninguna celda con `Draws` en cada lado; `OffCells` fuera de rango tambien | fail-closed | P-04, P-05, P-05c, PC-08, PC-08c |
| L-16 | Tarimas frontales de Push Back desbordadas (simple o compuesto, por lado) | fail-closed | P-03c, PC-14b |
| L-17 | Push Back compuesto con ranuras ausentes al inicio o al final de un lado | fail-closed | PC-05b |
| L-18 | Cantilever con arriostramiento de seccion asimetrica, brazo sencillo de seccion asimetrica o ids de placa que cambian el BOM | fail-closed | K-07b, K-08, K-09 |
| L-19 | Cabecera con rasgos distintos entre lados, Auto con baseline estandar o valor de diagonal desconocido | fail-closed | H-04b, H-06, H-02c |
| L-20 | Metadata semantica desconocida del payload, clave exterior desconocida, miembro `UNSUPPORTED`, enum no definido o nombres que colisionan | fail-closed | V-META, X-02b, X-03b, X-16, X-20, X-21 |
| L-21 | Payload legado con miembros retirados: con los que el store ya no conserva, el mensaje pide RACKEDITAR → Actualizar; con el que el store conserva (`HighEndBeamPeralte`), no hay remedio en I-52 | fail-closed con mensaje | X-19, X-19b |
| L-22 | **Pieza que necesita aceptar un cambio de mano y no puede demostrar su simetria** con la sonda por estado (sonda no viable, entidad no soportada, simetria no verificada): afecta a practicamente **todas** las piezas de frontal y planta (largueros, postes, placas, tarimas) | fail-closed | X-17a, X-17b |
| L-23 | **Vinculos que cambian el estado dinamico** de esas piezas sin evidencia universal: Selectivo con `PalletTolerance` vinculada sin overrides que fijen el ancho (largueros) o con `VerticalClearance` vinculada sin alturas fijadas (postes) | fail-closed | S-26, S-27, X-17c |
| L-24 | Registro de variables no acreditable con algun grupo Selectivo | fail-closed (E7) | PV-5 |
| L-25 | Alguna vista admisible del rack, seleccionada o no, que no conmuta | fail-closed | X-15 |
| L-26 | Bloques ausentes tras importar, o definicion real distinta o cambiada entre PREFLIGHT y PREPARE | fail-closed sin semantica nueva (E10/E11) | PRE-09, PRE-13 |

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

Reducen el alcance util del primer corte sin cambiar la arquitectura. Todos se detectan **antes** de pedir la linea,
salvo la re-verificacion de definiciones de PREPARE (E10). La vista del Owner es la envolvente de §1.3.

| Caso | Motivo | Fila o regla |
|---|---|---|
| Selectivo que usa topes | holgura de 0.25" anclada a un lado (AR-01) | S-13, S-13b |
| **Push Back con tope posterior activo — ES EL ESTADO POR DEFECTO** | `PushBackRearTopeConfig` esta activo por defecto (`PushBackRearTope.cs:9`): solo se guardan desactivaciones (`OffCells`, `:41`), `Draws = !IsNone ∧ At` (`:53`), `IsNone` compara `"(ninguno)"` sin distinguir mayusculas (`:37-38`; `PushBackDefaults.cs:44`) y un `PieceId` en blanco o desconocido usa `LARGUERO_ESCALON_TOPE_DE_3` (`PushBackRearTopeBuilder.cs:27`, `:37-48`); el lado B tiene su propia configuracion, tambien activa (`PushBackSideDesign.cs:48`). **Afecta a la mayoria de los Push Back** | P-04, P-05, PC-08 (admitidos solo P-04b/PC-08b) |
| `OffCells` del tope posterior inactivo con indices fuera del dominio valido | la mascara latente puede volver a regir en el lado incorrecto | P-05c, PC-08c |
| **Medio frente cuyo `BeamLength` efectivo depende de una propiedad vinculada** | la normalizacion congelaria el remanente | S-02b |
| **Desviador con off-cells cuyo espacio de indices depende de ese ajuste** | remapeo inestable ante `ChangeValue` | S-14c |
| Parrilla con una fila desbordada (`count·frente > span`) | reparto apilado desde la izquierda | S-15x |
| **Parrilla con conteo por defecto cuyo ajuste depende de un vinculo sin `DEP-ROW`** | un `ChangeValue` posterior puede desbordarla | S-15d |
| **Tarimas con una fila de claro completo desbordada** | mismo reparto apilado | S-17c |
| **Tarimas cuyo ajuste depende de un vinculo sin `DEP-ROW`** | obligacion 9b | S-17d |
| **Tarimas frontales de Push Back desbordadas** (simple o compuesto, por lado) | `FrontalRow` recorta el margen a 0 y ancla la fila a la izquierda (`PushBackTarimaPlacement.cs:224`) | P-03c, PC-14b |
| Cabecera con `AutoAlternating` en un panel con diagonal y `StandardBaselineId` no vacio | semantica de «estandar» sin caracterizar (`RackFrameConfigurationFactory.cs:85`, `:226-243`) | H-06 |
| **Metadata semantica desconocida no vacia en el payload** | no hay transformador | V-META; S-24, D-18, P-11, K-15, H-09 |
| **Metadata exterior desconocida no vacia** (sobre o envoltorio) fuera de la allowlist | un build futuro puede poner alli semantica de vista, lado u orientacion | V-META; X-02b, X-03b |
| **Nombres de miembro que colisionan bajo el matching real del serializador** | el valor efectivo depende del orden de las claves | X-21 |
| **Payload legado con un miembro retirado** | dato de una version anterior; R-A: el store lo descarta y Actualizar lo limpia; R-B: el store lo conserva | X-19, X-19b |
| **Enum con valor numerico no definido** | `JsonStringEnumConverter` admite enteros sin validar | X-20 |
| **Miembro `UNSUPPORTED` no vacio segun la guarda de cobertura** | sin regla de espejo | X-16 |
| **Pieza con cambio de mano sin simetria demostrada por la sonda** | sonda no viable, entidad no soportada, simetria no verificada o eje oblicuo | X-17a, X-17b |
| **Estado dinamico de una pieza que depende de un vinculo sin evidencia universal** | la evidencia de un estado no cubre los estados que `ChangeValue` puede producir | S-26, S-27, X-17c |
| **Decision `ALLOW`/`CANONICALIZE` sin prueba de estabilidad para todo `R`** | obligacion 9b | X-18 |
| **Alguna vista admisible del rack (seleccionada o no) que no conmuta** | V-VIEW-ALL | X-15 |
| Definicion fuente con `Origin ≠ 0` | BASE/BEDIT no soportado | ST-14 |
| **Referencia a bloque dinamico, anonimo o anotativo con payload RackCad** | transformacion adicional no capturada | ST-17 |
| `View` o `Section` no canonizable, incluida la `View` vacia del Selectivo con `Section ≥ 0` | §3.7 | ST-15 |
| **Sobre que no se puede leer o reserializar** | residuales preexistentes F-14a y F-14b del store del sobre (I-54) | ST-2c/E2, E8 |
| Bloques de biblioteca ausentes, o definicion real distinta de la verificada, tras PREPARE | importacion best-effort (§9.5) y re-verificacion de la definicion real (§11.3) | E10, E11 |
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
  C3  ST-4   candidato fuera de Model Space                                             → IGNORAR + aviso (decision deliberada,
             continuidad con PD-7 de I-51; no aborta por las demas restricciones de candidato)
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
| ST-4 | C3 | Candidato fuera de Model Space (`OwnerId` ≠ Model Space) | ignorar + aviso. **Decision deliberada (V5-16):** un RackCad fuera de Model Space no entra como fuente y no aborta por las demas restricciones de candidato (continuidad con PD-7 de I-51); un candidato **dentro** de Model Space si aplica las restricciones estrictas de C4 |
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
(`Vector2D.cs:82-89`). V5 no inventa ninguna tolerancia relativa.

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
**STOP → Proposal V6**. No se duplica la agrupacion en un segundo planificador. Las guardas G-R1..G-R6 leen el codigo de
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
| RF-10 Metadata | V-META (§6.4) sobre el contrato de serializacion real: miembro desconocido no vacio en el payload semantico, clave exterior desconocida no vacia fuera de la allowlist, miembro retirado (R-A, R-B) o colision de nombres ⇒ `UNKNOWN` |
| RF-11 Cobertura | Cada clave `(kind, ruta)` alcanzada por el cierre transitivo sobre el contrato de serializacion (§6.7) tiene exactamente una clasificacion; los enums alcanzados tienen instantanea y los enteros no definidos fallan cerrado |
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

**Ningun contenedor es portador por ser contenedor.** El sobre ya lleva semantica de vista (`View`,
`RackEmbedDocument.cs:41`; `Section`, `:47`), `Compose` copia su `ExtensionData` sin interpretarlo
(`RackEmbedComposer.cs:33`) y ALT-L (§3.9) preve una propiedad de vista en el sobre.

**Autoridad de deteccion: el contrato de serializacion real (V5-12).** «Conocido», «desconocido», «vacio» y «colision» se
deciden con el `JsonTypeInfo` que producen **las mismas** `JsonSerializerOptions` del store de cada nivel —con sus
modifiers, converters, `PropertyNameCaseInsensitive`, `[JsonIgnore]` y `[JsonExtensionData]`—, nunca solo con reflexion CLR:

| Nivel | Opciones reales |
|---|---|
| Sobre | `RackEmbedDocument.cs:70-73` (`PropertyNameCaseInsensitive = true`) |
| Selectivo | `SelectivePalletDesignStore.cs:67-76` (sin distinguir mayusculas, `WhenWritingNull`, `JsonStringEnumConverter`) |
| Envoltorio y kinds por dominio | `RackProjectStore.cs:404-423` (sin distinguir mayusculas, comentarios, comas finales, `JsonStringEnumConverter` y el modifier `DropRetiredCantileverPunchMargins`, `:386-400`, `:420-422`) |
| Cabecera | `RackFrameProjectStore.cs:137-145` |

**Colisiones (X-21).** Dos o mas claves JSON que el matching real asigna al **mismo** miembro (por ejemplo
`PalletTolerance` y `palletTolerance` con `PropertyNameCaseInsensitive = true`) ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`: el valor
efectivo dependeria del orden de las claves. Aplica a payload, sobre y envoltorio, y a CA-4.

**Allowlist de portadores (autoridad; cerrada).**

| # | Miembro o clave | Donde | Contrato | Evidencia |
|---|---|---|---|---|
| CA-1 | `SchemaVersion` del sobre | `RackEmbedDocument` | version de escritura no degradada | `RackEmbedComposer.cs:26`; `RackEmbedDocument.cs:35` |
| CA-2 | `SchemaVersion` del envoltorio | `RackProjectDocument` | no degradada via `WithSourceMetadataFrom` | `RackProjectStore.cs:81`; `RackProjectDocument.cs:18` |
| CA-3 | `PropertyValues` completo (incluidos `Kind` desconocidos, `expression` de I-49, literales congelados y `VariableId`) | `SelectivePalletDesignDocument` | **portador de almacenamiento**, con la condicion de V5-14 (debajo) | `SelectiveLinkedPropertyKernel.cs:103-150`; `SelectiveEffectiveDesignResolver.cs:115-160`; `SelectivePropertyValueDocument.cs:25-38` |
| CA-4 | `CustomProperties` (I-54) | miembro declarado del sobre cuando I-54 este integrada; **antes**, clave `CustomProperties` de `ExtensionData` del sobre | portador D-21 de I-54 (`JsonElement?` heredado por `Compose`); ADR-0039 aceptado en su rama sin cambiar ese contrato | I-54 Proposal V5 @ `26ca923` §D-21; ADR-0039 @ `3866252` |
| CA-5 | `DimensionViews` (I-50) | DTO Selectivo; dominio Dinamico y Push Back | `int` exacto, bits desconocidos y `null` | §10.3 |
| CA-6 | Cualquier portador integrado en `main` antes del Candidato | segun su contrato | **solo** si se añade **por nombre** a esta tabla con evidencia | §15 |

- **CA-3 no esconde semantica (V5-14).** Todo `PropertyId` **conocido** tiene descriptor vinculable con efecto semantico
  declarado, y los descriptores entran en la guarda de cobertura y en V-DEP (§6.5.5, §6.7); un descriptor nuevo ⇒ RED
  hasta clasificarse. Un `PropertyId` **desconocido** en ejecucion sigue la autoridad efectiva vigente, que nunca lo
  descarta y hace fallar la resolucion (`SelectiveLinkedPropertyKernel.cs:103-150`) ⇒ fail-closed. Un `PropertyId` que
  codifique un indice sensible a la orientacion (frente, poste, celda) deja de ser portador para esa clave y exige regla
  `TRANSFORMED` o `UNSUPPORTED` antes de integrarse.
- **CA-4 como clave de `ExtensionData`:** la coincidencia sigue el matching real del sobre (sin distinguir mayusculas,
  I-54 P-17(9)); dos o mas claves que coinciden son colision (X-21).
- La allowlist del `ExtensionData` del envoltorio esta **vacia**: ningun codigo productivo escribe claves en
  `ExtensionData` del sobre ni del envoltorio (solo se copian).

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
| cantilever | subarbol `Cantilever`, **excepto** los miembros retirados que el store declara y descarta (RM-5, RM-6) | CA-1, CA-2, CA-4 |
| cabecera | subarbol `Header` o la cabecera legada completa | CA-1, CA-2 (si hay envoltorio), CA-4 |

**Registro de miembros retirados (V5-12).** Un miembro que el formato actual ya no liga en esa ruta y que existio en
una version anterior. El registro los clasifica por lo que hace **hoy** el store con ellos:

| # | Ruta JSON | Retirado en | Que hace el store actual | Tratamiento |
|---|---|---|---|---|
| RM-1 | `DynamicSystem.HeaderDepthOverride` | `6f51f7a` (2026-06-20) | lo descarta al leer: `DynamicRackSystemDocument` no tiene `[JsonExtensionData]` | **R-A** |
| RM-2 | `PushBack.Composite.DefenseSideA` | `6495adc` (2026-08-29) | lo descarta: `PushBackCompositeDocument` no tiene `[JsonExtensionData]` | **R-A** |
| RM-3 | `PushBack.Composite.DefenseSideB` | `6495adc` (2026-08-29) | lo descarta | **R-A** |
| RM-4 | `PushBack.HighEndBeamPeralte` (raiz) | `eba5ac8` (2026-07-23; renombrado a `LegacyHighEndBeamPeralte`) | **lo conserva**: la raiz de `PushBackDesignDocument` tiene `[JsonExtensionData]` (`PushBackDesignDocument.cs:75-76`) y lo reescribe | **R-B** `[V5-D26]` |
| RM-5 | `Cantilever…Connection.Punches.ColumnBottomPlateEndOffset` (todo `CantileverPunchParameters`, `CantileverColumnBaseConnectionDesign.cs:38`) | declarado por el store | el modifier lo quita del contrato al leer y al escribir (`RackProjectStore.cs:373-400`) | `DERIVED` (K-17); no bloquea |
| RM-6 | `Cantilever…Connection.Punches.ColumnTopPunchOffset` (idem) | declarado por el store | idem | `DERIVED` (K-17); no bloquea |

- **R-A (X-19):** el payload crudo aun contiene el miembro ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`, **sin limpieza automatica**, con el
  mensaje:

  > El rack contiene datos de una versión anterior que el formato actual ya no conserva. Ábrelo con RACKEDITAR y
  > Actualizar antes de reflejarlo.

- **R-B (X-19b):** ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`, sin limpieza automatica. RACKEDITAR → Actualizar **no** elimina el miembro,
  asi que el mensaje **no** puede prometer remedio (texto propuesto; el definitivo lo fija G7 con T-M50): «El rack contiene
  un dato de una versión anterior que el formato actual conserva sin interpretar; RACKMIRROR no puede reflejarlo.»
- **Metodo y limite del barrido.** Se recorrio la historia de los 24 archivos de tipos persistidos alcanzables desde las
  raices (69 tipos) buscando propiedades publicas retiradas y ausentes hoy. No detecta un miembro **movido** de ruta sin
  cambiar de nombre: CT-32 hace el barrido definitivo por ruta JSON con el contrato de serializacion. Cualquier miembro no
  registrado sigue la regla general de V-META (`UNKNOWN`).

**Regla V-META.**

```text
Vacio(v)          ⇔ v es null, {} o []
Desconocido(X)    = miembros del payload semantico X que el contrato de serializacion real no liga en esa ruta,
                    que no son retirados declarados por el store (RM-5, RM-6) y que no son Vacio
                    (incluye los retirados R-A y R-B, con su mensaje)
DesconocidoExt(E) = claves de ExtensionData del sobre E y, cuando E.Design es un RackProjectDocument, de su
                    ExtensionData, que no son Vacio y no figuran en la allowlist (CA-4 con coincidencia unica)
Colision(X)       = grupos de claves JSON que el matching real asigna al mismo miembro (X-21)
V-META pasa ⇔ Desconocido(D) = ∅ ∧ DesconocidoExt(E) = ∅ ∧ Colision(D, E) = ∅
              (salvo declaracion tipada de invariancia o de transformador; vacia en el primer corte)
V-META falla ⇒ RepresentabilityClass = UNKNOWN, OperationalDisposition = FAIL_CLOSED (E6), con la ruta y el mensaje
```

- Un portador de la allowlist nunca bloquea por su contenido. Su **reescritura** si puede fallar por residuales
  preexistentes del store (F-14b de I-54): eso es E8 en el ensayo de P10, no V-META (§9.4).
- Una declaracion de invariancia o de transformador exige evidencia (CT y prueba) y vive en un unico registro tipado en
  `RackCad.Application.Mirror`; ninguna existe en el primer corte.
- Pruebas: CT-32 y T-M35 (clave desconocida vacia y no vacia, portador, `CustomProperties` antes y despues de I-54,
  colision por mayusculas, clave desconocida del envoltorio, retirados R-A y R-B) y T-M50/T-M51.

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
  simbolo de **posicion** a lo largo del eje reflejado, pero si el estado dinamico de los postes (DEP-10, `[V5-D27]`). Toda
  propiedad vinculable nueva se clasifica en la guarda de cobertura (§6.7) con
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

Aplica, **sin limitarse a S-02**, a: fit, overflow, ancho gobernante, espacios de indices, cantidad o posicion que
dependen de geometria efectiva, **estado dinamico de las piezas cuya simetria se acepta** (§11.3 punto 12) y cualquier
regla futura que descubra la guarda de cobertura (§6.7) o CT-38.

**Autoridad V-DEP.** Cada fila de §7 con disposicion `ALLOW` o `CANONICALIZE` declara:

| Campo | Contenido |
|---|---|
| `AuthoredInputs` | miembros authored que lee |
| `EffectiveInputs` | simbolos efectivos que lee, de un vocabulario cerrado por kind (Selectivo: `L(c)`, `BBL(b)`, `W(i)`, `Gobernante(i)`, `Remanente(i)`, `PostesCargados`, `FitFila`, `ConteoParrilla`, `PostX`, `ElevacionNivel(b,l)`, `AlturaClaro(b)`, `AlturaPoste(p)` y `ParametroDinamico(pieza, clave)`) |
| `BindableClosure` | descriptores vinculables en cuyo cierre transitivo estan esas entradas |
| `Predicate` | el predicado que decide la fila |
| `StabilityProof` o `FailClosedReason` | la prueba de 9b o el motivo del fail-closed |

**Cierre efectivo de cada descriptor (autoridad del kind, derivada del resolvedor).**

| Descriptor (`main @ f8deb67`) | Cierre efectivo declarado |
|---|---|
| `selective.palletTolerance` (`SelectiveLinkedProperties.cs:208-213`) | `L(c)` → `BBL(b)` → `W(i)` y `Gobernante(i)` → largueros, `PostX`, `Remanente(i)`, `PostesCargados`, `FitFila`, `ConteoParrilla`, cotas horizontales y **`ParametroDinamico(larguero, LONGITUD)`** en frontal y planta (`SelectiveFrontalBuilder.cs:131`, `:466`; `SelectivePlantaBuilder.cs:393-394`) |
| `selective.verticalClearance` | `ElevacionNivel(b,l)` (`SelectiveGeometryResolver.cs:310`, `:318`, `:396-412`) → `AlturaClaro(b)` (`:327`) → `AlturaPoste(p)` (`SelectivePostGeometry.cs:54-58`, `:115-121`) → **`ParametroDinamico(poste, LONGITUD)`** en la frontal, tambien postes intermedios (`SelectiveFrontalBuilder.cs:88-91`, `:150`, `:489`). Ningun simbolo de **posicion** a lo largo del eje reflejado |

Dinamico, Push Back, Cantilever y cabecera no tienen descriptores vinculables en `main`: sus decisiones y los estados
dinamicos de sus piezas no dependen de `R`. Un descriptor nuevo en cualquier kind ⇒ **RED / STOP** hasta declarar su
cierre (T-M36, T-M43, CT-38).

**Inventario V-DEP del primer corte (Selectivo).**

| # | Fila | Entradas efectivas | Predicado | Estabilidad 9b / disposicion |
|---|---|---|---|---|
| DEP-01 | S-02a / S-02b (N-S02) | `W(i)`, `Remanente(i)` | `Resolve(claro) ≠ null`; forma normal con `r` | `¬Dep(i)` ⇒ `W` constante ⇒ estable (RN); `Dep(i)` ⇒ `MC` `FAIL_CLOSED` (obligacion 9) |
| DEP-02 | S-14b / S-14c | `PostesCargados` | espacio de indices estable | sin claro dividido con `Dep(i)` ⇒ estable; si no ⇒ `UK` `FAIL_CLOSED` |
| DEP-03 | S-17b / S-17c / S-17d: tarimas de claro completo, incluida la fila de piso (`SelectiveGeometryResolver.cs:354`) | `W(i)`, `FitFila` | `count·frente ≤ W(i) + GeometryTolerance.Length` en toda fila | `¬Dep(i)` ⇒ se evalua en el SNAPSHOT (S-17b o S-17c); `Dep(i)` ⇒ `ALLOW` solo con `DEP-ROW`; si no ⇒ S-17d `UK` `FAIL_CLOSED` |
| DEP-04 | S-15 / S-15x / S-15d: parrilla de claro completo con conteo por defecto (`SelectiveParrillaPlan.cs:107`; `SelectiveFrontalBuilder.cs:310`) | `W(i)`, `FitFila` | igual que DEP-03 | igual que DEP-03 |
| DEP-05 | S-15 con `ParrillaFrente` o `ParrillaCantidad` manual | `ConteoParrilla` | nunca desborda: el conteo se recorta con `PalletFit` (`SelectiveFrontalBuilder.cs:291-311`) | estable: `D` y `μ_k D` calculan el mismo conteo desde el mismo `W(i,R)` y el reparto es simetrico; `G3_PENDING` con CT-37 |
| DEP-06 | S-01b, S-16, cotas horizontales, largueros de planta | `W(i)`, `PostX`, `Gobernante(i)` | reparto simetrico entre postes | estable para posiciones: la formula de postes es simetrica en los dos troqueles (`SelectivePostGeometry.cs:39`) y `μ_RUN` permuta claros con los mismos datos; `G3_PENDING` con CT-37. El estado dinamico de los largueros lo gobierna DEP-09 |
| DEP-07 | filas que leen `ElevacionNivel` para **posiciones** | `ElevacionNivel` | ninguno a lo largo del eje reflejado | estable para posiciones: la Y no se refleja (Lema 1, §3.4); `G3_PENDING` con CT-37 y CT-38. El estado dinamico de los postes lo gobierna DEP-10 `[V5-D27]` |
| DEP-08 | S-13 / S-13b (topes) | `W(i)` | — | ya `UK` `FAIL_CLOSED` (O-2) |
| **DEP-09** | **S-26**: estado dinamico de largueros con cambio de mano, frontal y planta | `W(i)` → `ParametroDinamico(larguero, LONGITUD)` | la simetria verificada por la sonda para el estado usado vale para todo `R` | `¬Dep(i)` ⇒ `LONGITUD` constante ⇒ basta la evidencia del estado (§11.3) ⇒ `ALLOW` si `IsSymmetric`; `Dep(i)` ⇒ sin evidencia universal en `main` ⇒ S-26 `UK` `FAIL_CLOSED` `[V5-D28]` |
| **DEP-10** | **S-27**: estado dinamico de postes con cambio de mano en la frontal (incluidos intermedios) | `ElevacionNivel` → `AlturaClaro` → `AlturaPoste` → `ParametroDinamico(poste, LONGITUD)` | igual que DEP-09 | `IndepAltura(p)` o sin vinculo sano de `selective.verticalClearance` ⇒ basta la evidencia del estado; si no ⇒ S-27 `UK` `FAIL_CLOSED` `[V5-D27]` |

```text
IndepAltura(p) ⇔ cabecera por poste de p (en la vista de su fondo) con Height > 0          (SelectiveFrontalBuilder.cs:88-91)
                 ∨ ( p tiene al menos un claro adyacente y TODOS sus claros adyacentes existentes (uno o dos)
                     tienen HeightOverride > 0 )                                        (SelectivePostGeometry.cs:115-121;
                                                                                        SelectiveGeometryResolver.cs:335-336)
Con esos overrides la altura del poste es positiva y constante, asi que el fallback de PostHeight (mayor claro del
fondo o altura del sistema) nunca se usa. En cualquier otro caso el poste se considera dependiente.
```

**`DEP-ROW` — prueba de ajuste para todo `R` (V5-09).**

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

- **Soundness.** Para todo `R ∈ Acreditables(D)` y toda celda `c`, `L(c,R) ≥ L_lb(c)`. Por tanto:
  - si `Gov_M(i) ≠ ∅`: `W(i,R) ≥ W_M(i,R) ≥ W_lb(i)`, porque `W_M(i,R) = max(0, max_c L(c,R))` y el fallback de
    `SelectiveGeometryResolver.cs:150-164` solo se activa con `W_M(i,R) ≤ 0` y **solo puede aumentar** `W`;
  - si `Gov_M(i) = ∅`: `W(i,R)` es el mayor `BBL` de los fondos en el indice `i` y es `≥ max_{c ∈ Gov_all(i)} L_lb(c)`.

  Con `DEP-ROW`, `count·frente ≤ W_lb(i) + tol ≤ W(i,R) + tol`: la fila no desborda en ningun `R`. La demostracion no
  necesita que `W_lb(i)` sea positivo.
- **Cota autoritativa.** `W_lb` usa `b` **solo** si existe una cota autoritativa real. En `main @ f8deb67` **no existe**:
  el store del registro solo exige un valor finito (`ProjectVariablesStore.cs:199-201`; `VariableDefinition.cs:53-64`),
  la preflight de mutaciones no valida signo (`ProjectVariableMutationPreflight.cs:98`), la resolucion efectiva no aplica
  dominio (`SelectiveEffectiveDesignResolver.cs:115-160`) y los controles `> 0` de RACKVARIABLES
  (`RackProjectVariablesWindow.xaml.cs:217-222`) y `< 0` del editor Selectivo (`RackSelectiveWindow.xaml.cs:2561-2565`)
  son de UI. Hoy `L_lb(c) = −∞` en toda celda sin override.
- **Si I-49 integra antes** (hoy: V6 congelada y ADR-0038 aceptado en su rama; produccion no iniciada): el dominio que su
  descriptor declara **como dato** y aplica al valor efectivo (I-49 V6 P24.5: `>= 0` en las dos propiedades Selectivas)
  es la cota `b`. V-DEP la consume como dato; I-52 no la reimplementa ni la infiere. Se re-mide en la reconciliacion
  (§15.3). Un dominio `>= 0` **no** convierte en universal la evidencia geometrica de DEP-09/DEP-10 (sigue siendo un
  continuo de estados).
- No se inventa ningun «valor minimo acreditado» cuando el sistema de variables o de expresiones no define dominio.

**Casos del Architect Review (normativos para G3).**

```text
Caso A — un fondo, valores positivos. Claro i sin medio frente.
  N1: PalletCount 2, Frente 40, BeamLengthOverride 70, tarimas activas.  N2: PalletCount 2, Frente 38, sin override.
  PalletTolerance vinculada a V.
  V = 4 ⇒ W = max(70, 76 + 12) = 88 ≥ 80: la fila de N1 no desborda.
  V = 1 ⇒ W = max(70, 76 + 3)  = 79 < 80: la fila de N1 se apila desde la izquierda en el original y en la copia.
  V5: Dep(i) verdadero; W_lb(i) = max(70, −∞) = 70 < 80 ⇒ ¬DEP-ROW ⇒ S-17d UNKNOWN ⇒ FAIL_CLOSED;
      ademas LONGITUD de los largueros depende de V ⇒ S-26 UNKNOWN (DEP-09).
Caso B — dos fondos, sin overrides.
  Fondo maestro, claro i: PalletCount 2, Frente 40.  Fondo 1, claro i: PalletCount 2, Frente 44, tarimas activas.
  PalletTolerance vinculada a V.
  V = 4 ⇒ W = 92 ≥ 88;  V = 1 ⇒ W = 83 < 88: la frontal del fondo 1 desborda.
  V5: Dep(i) verdadero; W_lb(i) = −∞ ⇒ ¬DEP-ROW ⇒ S-17d UNKNOWN ⇒ FAIL_CLOSED (y S-26 UNKNOWN).
Con una parrilla de conteo por defecto en esas filas el resultado es S-15d.
```

**Guardas de consumidores.**

- **T-M43:** toda fila de §7 con `ALLOW`/`CANONICALIZE` declara `EffectiveInputs`; si intersecan el cierre de algun
  descriptor, existe su entrada V-DEP con prueba o motivo; una fila sin declaracion ⇒ RED.
- **CT-38:** por descriptor, se varia su valor en el corpus de fixtures y se compara el sistema resuelto miembro a miembro
  **y los parametros dinamicos de cada instancia de plan**: todo simbolo que cambia y no esta en el cierre declarado ⇒
  RED / STOP.
- **CT-37 (V5-15):** diferencial con dos o mas estados del registro por descriptor (con cota autoritativa si existe; sin
  cota: cero, negativos y grandes), fondo maestro distinto y cambio de gobernante: toda decision `ALLOW` en `R_a` debe
  conmutar en `R_b`, **V-BOM-1 y V-BOM-2 deben seguir iguales** entre `D` y `μ_k D` cuando el descriptor pueda afectar el
  BOM o la geometria que lo alimenta, y los parametros dinamicos que cambian deben estar cubiertos por DEP-09/DEP-10. Un
  contraejemplo no previsto por V-DEP contradice V5 ⇒ **STOP → Proposal V6**.

### 6.6 Verificacion dinamica por rack (PREFLIGHT)

Ademas de las reglas estaticas de §7, para cada rack logico el plan del espejo **verifica** sobre el rack concreto, en
el orden de §9.1:

```text
V-META      metadata semantica desconocida del payload y metadata exterior fuera de la allowlist (§6.4)
V-DEP       autoridad de dependencias de §6.5.5 (obligaciones 9 y 9b; DEP-01..DEP-10, DEP-ROW)
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

### 6.7 Guarda de cobertura estructural (cierre transitivo sobre el contrato de serializacion)

Evita que un tipo, un miembro o un valor de enum nuevo se copie sin regla de espejo. La **guarda transitiva es la
autoridad**; toda tabla humana de esta Proposal es ilustrativa.

| Elemento | Contrato |
|---|---|
| Raices (minimo) | `RackEmbedDocument`, `RackProjectDocument`, `SelectivePalletDesignDocument`, `DynamicRackDesign`, `PushBackDesign`, `CantileverLineDesign`, `RackFrameConfiguration`. `RackProjectDocument` alcanza por sus ranuras los DTO de cada kind (`RackProjectDocument.cs:18-52`) |
| Autoridad del recorrido (V5-12) | para los tipos que se serializan (sobre, envoltorio, DTO y tipos de dominio embebidos en ellos): el **`JsonTypeInfo`** que producen las mismas `JsonSerializerOptions` del store (§6.4: `RackEmbedDocument.cs:70-73`, `SelectivePalletDesignStore.cs:67-76`, `RackProjectStore.cs:404-423`, `RackFrameProjectStore.cs:137-145`), con modifiers (`DropRetiredCantileverPunchMargins`), converters, `PropertyNameCaseInsensitive`, `[JsonIgnore]` y `[JsonExtensionData]`. Reflexion CLR **solo** para los miembros de dominio que no se serializan directamente (clasificados `DERIVED` o `TRANSFORMED` via su DTO) |
| Recorrido | transitivo hasta punto fijo: propiedades del contrato, tipos complejos, elementos de arrays y listas, clave y valor de diccionarios tipados, `Nullable<T>` desenvuelto y tipos anidados persistidos |
| Lista de corte (cerrada; cada corte con razon normativa) | (1) `SelectivePropertyValueDocument` y `PropertyValues`: contrato delegado en la autoridad de I-47/I-49 con la condicion de CA-3; (2) `JsonElement` y `[JsonExtensionData]`: sus claves las clasifica V-META (§6.4); (3) tipos primitivos y de framework: hojas. Un corte nuevo exige razon normativa y revision |
| Colisiones | dos propiedades del contrato cuyo nombre colisiona bajo el matching real (por ejemplo, solo difieren en mayusculas con `PropertyNameCaseInsensitive = true`) ⇒ **RED** en la guarda; en datos, X-21 |
| Enums (V5-13) | instantanea (nombre → valor) de **todos** los enums alcanzados y de los de decodificacion de secciones y de politica del espejo. Minimo: `FrameSide`, `DiagonalDirection`, `PushBackSide`, `PushBackFrontalEnd`, `DynamicRackEnd`, **`HeaderBlockRole`** (`Application/Drawing/HeaderBlockInstance.cs:7`) y **`CantileverVisualRole`** (`Application/Systems/Cantilever/CantileverVisualRole.cs:20`), mas los que la guarda encuentre (`PostSide`, `SafetySide`, `BootPlacement`, `BracingPattern`, `ExceptionType`, `FrameComponentState`, `FrameMemberType`, `PushBackCellTopology`, `PushBackRunDirection`, `DynamicRackModuleKind`, `CantileverArmSide`, `CantileverStationFaceMode`, `CantileverArmBodyArrangement`, `CantileverBraceBodyKind`, `DimensionDetail`, `DimensionViewVisibility`, `RackSystemKind`, ...). Valor nuevo, renombrado o retirado ⇒ **RED / STOP** |
| Enums en datos (V5-13) | los stores serializan enums con `JsonStringEnumConverter` (`SelectivePalletDesignStore.cs:76`, `RackProjectStore.cs:412`, `RackFrameProjectStore.cs:145`), que acepta enteros sin validar: un nombre desconocido ya hace fallar la lectura (E2), pero un **entero no definido** llega en silencio ⇒ **X-20 `UNKNOWN` ⇒ `FAIL_CLOSED`**, salvo invariante forward-compatible declarado y probado. Hoy el unico declarado es `DimensionViewVisibility` (flags; portador CA-5 con el entero exacto). Generaliza H-02c |
| Clave de clasificacion | `(kind, ruta de esquema)`, con indices de lista sustituidos por `[]` y claves de diccionario por `{}`. Un tipo alcanzado en dos kinds se clasifica en cada uno. Un ciclo de tipos exige entrada de recursion declarada; un ciclo sin declarar ⇒ RED |
| Clasificacion | cada clave exactamente **una** vez: `INVARIANT`, `TRANSFORMED` (con su fila de §7), `NORMALIZED` (con su N), `CARRIER` (con su entrada CA-n), `DERIVED` (calculado, no persistido, retirado declarado o descartado por el store) o `UNSUPPORTED` (con su fila `MC`/`UK`) |
| Naturaleza | ademas, **persistido**, **derivado/no persistido** o **portador** |
| Descriptores vinculables | cada descriptor se clasifica con su cierre efectivo, incluidos los `ParametroDinamico` que alimenta (§6.5.5); un descriptor nuevo, o un cierre que CT-38 no confirma ⇒ RED |
| Consumo en ejecucion | un miembro `UNSUPPORTED` con valor no vacio ⇒ X-16 `UNKNOWN` ⇒ `FAIL_CLOSED` (P6) |
| Donde vive | una tabla por kind en Application, junto a su reflector, y las del exterior en `RackCad.Application.Mirror`; **ninguna** en `RackCad.Application.Systems.Shared` (G-M14); sin `switch` por kind en el Plugin |
| Guardas | T-M36 (cierre y clasificacion exacta sobre el contrato), T-M44 (instantanea de enums), T-M51 (enteros no definidos y colisiones), T-M43 (V-DEP), G-M13 y G-M14; linea base CT-31 y CT-42 |

**Coexistencia con I-53 (C-08 y C-10).** I-53 añadio en G3 cuatro tipos `Header*` en
`RackCad.Application.Systems.Shared` con dos guardas: C-08 exige que todo tipo `Header*` de ese namespace figure en
`SharedFoundationInspection.Roots` (`HeaderBatchContractTests.cs:476-488` @ `4e00a27`) y C-10 prohibe en **todo** ese
namespace tipos cuyo nombre contenga `Coverage`, `StageInert`, `OmitAndReport`, `Executor`, `BatchService`, `Engine` o
`Callback` (`:493-512` @ `4e00a27`). Su G4 (`7de424e`) añadio consumidores Selectivos en `Systems.Selective` y su G6
(`f271fd5`) consumidores Dinamicos en `Systems.Dynamic`; ninguno toca `Systems.Shared` ni el contrato persistido. Por
tanto:

- I-52 **no** crea tipos en `RackCad.Application.Systems.Shared`, ni `Header*` ni de otro nombre: sus tipos viven en
  `RackCad.Application.Mirror` o junto a su kind (G-M14, CT-41);
- I-52 no modifica `SharedFoundationInspection` ni las guardas de I-53;
- todo tipo o miembro persistido de cabecera o de modulo que I-53 introduzca al integrarse queda alcanzado por el cierre
  y en **RED** hasta clasificarse.

---

## 7. Matriz de representabilidad V5

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
  G3_VERIFIED       confirmada por la caracterizacion de G3 (ninguna fila en V5)

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

**Regla de cierre de G3 (V5-10).** Al cerrar G3 **ninguna** fila puede quedar `G3_PENDING` y ninguna puede quedar con
`EvidenceStatus` implicito. Cada fila termina en exactamente uno de estos estados:

| Resultado | `RepresentabilityClass` | `EvidenceStatus` | `OperationalDisposition` |
|---|---|---|---|
| Verificada | la clase correspondiente (`REPRESENTABLE` o `REPRESENTABLE_BY_NORMALIZATION`) | `G3_VERIFIED` (con la CT que la verifica) | la disposicion correspondiente (`ALLOW`, `CANONICALIZE` o `BASELINE_LIMITED`) |
| Fallo demostrado | `UNKNOWN` o `REQUIRES_MODEL_CHANGE` | `CODE_SUPPORTED` si la evidencia sale del codigo; `G3_VERIFIED` si sale de la caracterizacion | `FAIL_CLOSED` |

Antes de cerrar G3, cada fila condicional (por ejemplo S-10, D-03b, K-09, S-19, D-14, P-09, PC-10, K-10 y H-07) se parte
en filas o estados inequivocos, con una sola tripleta. Si una reclasificacion cambia materialmente el alcance (la
envolvente de O-1), el ADR, una regla de reflexion o la arquitectura ⇒ **Proposal V6**. T-M47 lo hace ejecutable desde G5.

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
| **S-26** | **Estado dinamico de largueros con cambio de mano** (`LONGITUD = W(i)`), frontal y planta | la simetria verificada por la sonda para el estado usado debe valer para todo `R` (DEP-09) | R si `¬Dep(i)` e `IsSymmetric`; UK si `Dep(i)` | `G3_PENDING` (R) / `CODE_SUPPORTED` (UK) | ALLOW si `¬Dep(i)` e `IsSymmetric`; FAIL_CLOSED si `Dep(i)` | `Dep(i)` (§6.5.1) | `SelectiveFrontalBuilder.cs:131`, `:450-470`; `SelectivePlantaBuilder.cs:393-394` |
| **S-27** | **Estado dinamico de postes con cambio de mano** (`LONGITUD = altura`), frontal, incluidos intermedios | igual que S-26 (DEP-10) | R si `IndepAltura(p)` o sin vinculo sano de `VerticalClearance`, e `IsSymmetric`; UK en otro caso | `G3_PENDING` (R) / `CODE_SUPPORTED` (UK) | ALLOW / FAIL_CLOSED | `IndepAltura(p)` (§6.5.5) | `SelectiveFrontalBuilder.cs:88-91`, `:150`, `:475-491`; `SelectivePostGeometry.cs:54-58`, `:115-121`; `SelectiveGeometryResolver.cs:310`, `:318`, `:327` |

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
| P-04b | Tope posterior **inactivo**: `"(ninguno)"` (`IsNone`, sin distinguir mayusculas) o ninguna celda con `Draws` | escalares sin efecto grafico; se copian sin cambio (la mascara: P-05b) | R | CODE_SUPPORTED | ALLOW | ¬P-04 | `PushBackRearTope.cs:37-38`, `:53`; `PushBackSystemFrontalBuilder.cs:277`; `PushBackSystemPlantaBuilder.cs:115-118` |
| P-05 | `RearTopeOffCells[].Frente` con tope posterior activo | sin regla mientras P-04 sea UK | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2B) | igual que P-04 | `PushBackRearTopeBuilder.cs:268-307` |
| P-05b | `RearTopeOffCells[].Frente` con tope inactivo (P-04b) e indices **validos** (frente en `[0, N)` y nivel en `[0, EffectiveLoadLevels(frente))`) | `f → N−1−f`: la mascara sigue persistida y vuelve a regir si el usuario elige una pieza en la copia | R | G3_PENDING | ALLOW | P-04b ∧ todos los indices validos | `PushBackRearTope.cs:41`, `:46-53`; `PushBackRearTopeBuilder.cs:268-307` |
| **P-05c** | `RearTopeOffCells` con algun indice **fuera** del dominio semantico valido actual | no se conserva opacamente: puede volverse activo tras una edicion y reaparecer en el lado incorrecto (V5-11) | UK | CODE_SUPPORTED | FAIL_CLOSED | algun frente `≥ N` o nivel `≥ EffectiveLoadLevels(frente)` | idem |
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
| PC-08b | Tope posterior inactivo en **ambos** lados; `OffCells` por lado con indices validos | `f → N−1−f` por lado (mascara latente, como P-05b) | R | G3_PENDING | ALLOW | P-04b en A y en B ∧ indices validos | idem |
| **PC-08c** | `OffCells` de algun lado con indices fuera del dominio valido | como P-05c | UK | CODE_SUPPORTED | FAIL_CLOSED | por lado | idem |
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
| X-17a | Pieza con cambio de mano sin `GeometrySymmetryResult` verificado: sonda no disponible, simetria no verificada, correspondencia ambigua o eje oblicuo | la simetria no esta demostrada | UK | CODE_SUPPORTED | FAIL_CLOSED | §11.3 |
| X-17b | Geometria evaluada con una clase de entidad no soportada o un ciclo de anidados | fuera de la politica de geometria | UK | CODE_SUPPORTED | FAIL_CLOSED | §11.3 punto 6 |
| X-17c | Estado dinamico de la pieza dependiente de un vinculo sin evidencia universal | la evidencia de un estado no cubre `ChangeValue` | UK | CODE_SUPPORTED | FAIL_CLOSED | §11.3 punto 12; S-26, S-27 |
| X-18 | Decision `ALLOW`/`CANONICALIZE` sin prueba de estabilidad 9b, fuera de las filas especificas | ninguna | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.5.5 |
| X-19 | Payload legado con un miembro retirado que el store actual descarta (RM-1..RM-3) | R-A: mensaje con remedio (RACKEDITAR → Actualizar); sin limpieza automatica | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.4; `DynamicRackSystemDocument.cs`; `PushBackDesignDocument.cs` (`PushBackCompositeDocument`) |
| X-19b | Payload legado con un miembro retirado que el store actual conserva (RM-4 `HighEndBeamPeralte`) | R-B: mensaje sin remedio; sin limpieza automatica | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.4; `PushBackDesignDocument.cs:75-76` |
| X-20 | Enum con valor numerico no definido en un miembro alcanzado | el converter admite enteros sin validar | UK | CODE_SUPPORTED | FAIL_CLOSED (salvo invariante declarado: `DimensionViewVisibility`) | §6.7; `SelectivePalletDesignStore.cs:76`; `RackProjectStore.cs:412`; `RackFrameProjectStore.cs:145` |
| X-21 | Claves JSON que el matching real asigna al mismo miembro | valor efectivo dependiente del orden | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.4; `PropertyNameCaseInsensitive = true` en los stores |

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
| PRE-13 | Definicion real de una pieza con cambio de mano distinta en PREPARE de la evaluada en PREFLIGHT (`TraceFingerprint` distinto) y cuya simetria ya no se verifica | FAIL_CLOSED (E10) | PREPARE | §11.3 puntos 3 y 10 |
| PRE-14 | Sonda geometrica no disponible o fallida para una pieza con cambio de mano | FAIL_CLOSED (E6, X-17a) | P7 | §11.3 |

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
  P7  V-VIEW-ALL                                   todas las (View, Section canonica) admisibles del rack logico (§3.8):
        P7a  Application enumera las piezas con cambio de mano y su estado (definicion, parametros, marco)
        P7b  el Plugin evalua cada estado con la sonda, sin mutar el DWG (§11.3): GeometrySymmetryResult
        P7c  Application completa la equivalencia con los resultados, DEP-09/DEP-10 y PlanPlacementEvidence
  P8  V-BOM                                        las dos comprobaciones (§11.5)
  P9  identidad y nombre                           NewRackId por grupo; «<base> - espejo»; validacion del asignador (§5.5)
  P10 Compose + ensayo de restamp + V-RT-STORE     Plugin, sin transaccion; V-RT-STORE sobre el payload re-estampado (§6.3)

LINE       dos puntos UCS→WCS validos (§4.2)
PREPARE    colocaciones P' · planes efectivos de las vistas SELECCIONADAS (§8.1) · payloads definitivos
           · EnsureForPlan (infraestructura, §9.5) · re-verificacion de bloques · re-verificacion de huellas y read-set
           · re-verificacion de la definicion real tras EnsureForPlan: TraceFingerprint y, si difiere, simetria (§11.3)
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
| E6 | Fila `REQUIRES_MODEL_CHANGE` o `UNKNOWN` material (topes Selectivo, tope posterior de Push Back **activo por defecto** u `OffCells` fuera de rango, medio frente dependiente, desviador inestable, parrilla o tarimas desbordadas, **parrilla o tarimas sin `DEP-ROW` (S-15d, S-17d)**, **tarimas frontales de Push Back desbordadas (P-03c, PC-14b)**, Auto con baseline, **estado dinamico dependiente de vinculos (S-26, S-27)**), V-META (payload, exterior, **retirados, enums no definidos y colisiones**), V-DEP (obligaciones 9 y 9b), **V-VIEW-ALL** (incluidas vistas admisibles no seleccionadas), X-16, **X-17a/X-17b/X-17c**, X-18, V-BOM o V-RT-STORE fallidas; diseño bloqueado o invalido | P4..P10 | fin, cero mutacion; nombra rack, vista, propiedad, pieza, clave o miembro, con el mensaje de R-A o R-B cuando corresponda |
| E7 | Registro **no acreditable** (presente e ilegible, version incompatible o identidad ambigua) **y** algun grupo cuyo kind lo consume (hoy: Selectivo, con o sin vinculos); o vinculo roto | P3 | fin, cero mutacion. Si ningun grupo lo consume, el registro no se evalua (§10.2) |
| E8 | Ensayo de `Compose` + serializacion + restamp + nombre fallido, incluidos F-14b de I-54 (la reserializacion lanza `JsonException`) y la regla F de `Compose` (origen con una clave de `ExtensionData` igual a un miembro declarado) | P9, P10 | fin, cero mutacion; toda excepcion de esos pasos se traduce a E8 con mensaje atribuible |
| E9 | Linea invalida o cancelada | LINE | fin, cero mutacion |
| E10 | Fallo de plan, bloque ausente tras importar, definicion real de una pieza con cambio de mano cambiada y ya no simetrica (PRE-13), huella o read-set cambiado | PREPARE | fin; **ninguna** definicion, referencia ni payload RackCad nuevo; lo importado por `EnsureForPlan` puede permanecer (§9.5) |
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
| PV-6 | Si I-49 integra antes (hoy: V6 congelada, ADR-0038 aceptado en su rama `82aa61b`, G5 y produccion NOT STARTED): RACKMIRROR invoca la autoridad efectiva integrada, preserva `expression` como portador, adopta su `PlanReadSet` para el read-set (§9.2.1) y su dominio de consumidor como cota autoritativa de V-DEP (§6.5.5), y **no** reimplementa su evaluacion ni un segundo motor de expresiones (P24.5 y P24.8 de I-49 V6) |
| PV-7 | La copia es un **consumidor nuevo** del mismo `VariableId` (ADR-0034 §12): bloquea `Delete` y se redibuja en `ChangeValue` desde su authored reflejado. **`ChangeValue` no puede invalidar ninguna decision `ALLOW` o `CANONICALIZE` tomada en PREFLIGHT** (obligacion 9b), **incluida la aceptacion de un cambio de mano cuya evidencia geometrica dependa de un parametro dinamico vinculado** (§11.3 punto 12): un rack con alguna decision cuya verdad dependa de un valor vinculado sin prueba para todo `R` no es admisible (S-02b, S-14c, S-15d, S-17d, S-26, S-27 y toda fila que V-DEP marque) |

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
- **I-54 (Proposal V5 con consenso congelado; ADR-0039 aceptado en `3866252`; sin produccion de la feature)**: las propiedades de
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

### 11.3 Mano y evidencia geometrica evaluada por estado

1. **Ningun bloque DWG se asume simetrico.** La biblioteca no esta versionada, no tiene metadato de simetria y el
   manifiesto de I-19 no registra geometria (`CatalogBlockManifest.cs:12-59`). Casi todas las piezas de frontal y planta
   son bloques dinamicos (`LONGITUD`, `PERALTE`, `ALTURA`, ...) que el builder emite sin `MirroredX`
   (`SelectiveFrontalBuilder.cs:450-470`, `:475-491`): en `A = F ∘ Π(D)` quedan volteadas y en `B = Π(μD)` no, asi que su
   equivalencia exige **aceptar un cambio de mano**.
2. **Dos evidencias, con orden obligatorio.** Un cambio de mano solo se acepta si pasan **las dos**:

   | Evidencia | Que demuestra | Como |
   |---|---|---|
   | **`GeometricEvidence`** (primero) | que la geometria **real** de la pieza, en el **estado concreto** que usa, es igual a su reflexion respecto de `(eje, centro)` | **STATE-EVALUATED GEOMETRIC EVIDENCE** (puntos 3-10); nunca derivada de builders, de la diferencia entre planes, de inserciones ni de un minimo de error |
   | **`PlanPlacementEvidence`** (despues) | que el builder coloca la pieza de forma coherente con esa simetria: `A ≡ B` con el centro verificado | V-VIEW-ALL por rack en PREFLIGHT (§3.8, §6.6); CT-06 y T-M12 como caracterizacion y prueba. Nunca ajusta, infiere ni corrige `c` |

   Una geometria simetrica **no** oculta un offset del builder: con el centro verdadero, un desplazamiento hace
   `A ≢ B` (punto 9) ⇒ X-13/X-17a `UNKNOWN` ⇒ `FAIL_CLOSED`.
3. **Ruta primaria: STATE-EVALUATED GEOMETRIC EVIDENCE (V5-01).** Para cada pieza de **cualquier** vista admisible del
   rack (seleccionada o no) cuya equivalencia requiera aceptar un cambio de mano:

   ```text
   (BlockName, definicion real, vector de parametros dinamicos, marco local de insercion)
           ↓  evaluar la geometria real de ese estado
           ↓  aplanar recursivamente (anidados con su transformacion completa y su propio estado)
           ↓  probar la simetria de reflexion (puntos 7-8)
           ↓
   GeometrySymmetryResult
   ```

   **Fuente de la definicion.** Si el bloque ya existe en el dibujo, la evidencia corresponde a **esa** definicion. Si no
   existe, se inspecciona en PREFLIGHT la definicion de la biblioteca candidata (lectura con la cache de sesion de
   `BlockLibraryImporter.cs:122-161`). Tras `EnsureForPlan`, **PREPARE** vuelve a verificar la definicion **real** que quedo
   en el dibujo para las vistas que se dibujan: `DuplicateRecordCloning.Ignore` conserva una definicion local con el mismo
   nombre (`BlockLibraryImporter.cs:109-110`), asi que la biblioteca sola nunca basta si el DWG ya contiene el bloque. Para
   las vistas admisibles no seleccionadas, la equivalencia se afirma sobre la definicion evaluada en PREFLIGHT; la que un
   Insertar futuro importe queda bajo la limitacion general de §9.5 punto 6, igual que para el rack original.
4. **Sin mutacion del DWG (V5-01).** La evaluacion se hace en solo lectura o clonando y evaluando en una `Database`
   lateral de trabajo; **nunca** inserta artefactos persistentes (referencias temporales, definiciones anonimas, capas) en
   el dibujo del usuario durante la sonda (G-M16).
5. **Salida pura.** La infraestructura AutoCAD del Plugin produce datos planos; Application **no** referencia
   `Autodesk.*` (G-M3). Contrato conceptual (nombres no contractuales; no es schema persistido):

   ```text
   GeometrySymmetryResult {
       BlockName
       EvaluatedStateSignature     // definicion evaluada + vector de parametros + transformacion de la instancia
       Axis                        // eje local X o Y (punto 7)
       Center                      // candidato verificado, en el marco local tras T(−BlockOrigin)
       IsSymmetric
       SupportedEntityKinds        // clases encontradas; cualquier clase no soportada ⇒ IsSymmetric = false
       TraceFingerprint            // punto 10
       Diagnostics                 // motivo atribuible (entidad, ciclo, ambiguedad, eje oblicuo, sonda no disponible)
   }
   ```

6. **Politica de geometria (V5-02).**

   | Clase | Primer corte |
   |---|---|
   | Line, Arc, Circle, Polyline (con bulges), Ellipse, Spline, Solid, Trace | soportada |
   | Hatch con relleno SOLID representable por su contorno | soportada (se compara el contorno) |
   | BlockReference anidado | soportada, recursiva, con transformacion completa; un anidado dinamico se evalua en **su** estado real |
   | Region, 3DSolid, proxy o entidad custom, raster, OLE | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Hatch cuyo contenido no se reduce al contrato (patron, gradiente) | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Text, MText, AttributeDefinition/Attribute, Dimension | `UNKNOWN` ⇒ `FAIL_CLOSED`, salvo futura exclusion o declaracion tipada con evidencia (ninguna en el primer corte) |
   | Ciclo de anidados | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Correspondencia geometrica ambigua | `UNKNOWN` ⇒ `FAIL_CLOSED` |

   Capa, color y tipo de linea no son geometria y no se comparan.
7. **Eje y centro (V5-03, `[V5-D29]`).** El eje a probar se deriva de `F` y de la rotacion de la instancia: una rotacion
   multiplo de `π/2` (dentro de `GeometryTolerance.Angle`) da el eje local X o Y del bloque; una rotacion oblicua ⇒
   `UNKNOWN`. El centro se obtiene como **candidato** geometrico (por ejemplo, el punto medio de las extensiones de la
   geometria aplanada a lo largo del eje) en el marco `u = x_BTR − o_p` (punto 13). **La caja envolvente nunca es prueba.**
8. **Verificacion.** `IsSymmetric` es verdadero si y solo si el multiconjunto de primitivas originales es igual al
   multiconjunto reflejado respecto de `(eje, centro)` dentro de `GeometryTolerance.Length` (y `GeometryTolerance.Angle`
   donde aplique), con una correspondencia uno a uno inequivoca. Una pieza **asimetrica con caja envolvente simetrica**
   **falla** (T-M40).
9. **Equivalencia.** Con `IsSymmetric` y eje X, la instancia `(q, r, mX, mY, parametros)` equivale a

   ```text
   (q + R(r)·(2·c·mX, 0), r, −mX, mY, parametros)          eje X
   (q + R(r)·(0, 2·c·mY), r, mX, −mY, parametros)          eje Y
   ```

   que sale de `T(q)·R(r)·S(mX,mY)·X_c = T(q + R(r)·(2c·mX, 0))·R(r)·S(−mX, mY)`, con `X_c = T(2c,0)·S(−1,1)` y `c` el centro
   **verificado de ese estado**. Casos del Architect Review (normativos para T-M40):

   ```text
   Caso C. Pieza de rol distinto de Tope con LONGITUD en X; geometria en u ∈ [0, L] (centro verificado L/2);
     el builder inserta en el ancla izquierda a con L = S + δ.  A = (F(a), mX = −1);  B = (F(b), mX = +1).
     A ≡ B ⇔ c = S/2 = L/2 − δ/2  ≠ L/2  ⇒  A ≢ B  ⇒  UNKNOWN.
   Caso D. Pieza de ancho fijo w anclada en poste + t, en claros de ancho S.
     A ≡ B ⇔ c = (S − 2t)/2;  centro verificado w/2 ⇒ con S − 2t ≠ w no hay equivalencia ⇒ UNKNOWN.
   Caso E (V5). Pieza asimetrica cuya caja envolvente es simetrica: la verificacion del punto 8 falla ⇒ UNKNOWN.
   ```

10. **`TraceFingerprint` (V5-04).** `DefinitionGeometryHash` deja de ser evidencia normativa de simetria. El
    fingerprint solo sirve para **trazabilidad**, **deteccion de obsolescencia** y **enlace PREFLIGHT ↔ PREPARE**: si en
    PREPARE difiere del de PREFLIGHT, la simetria se vuelve a comprobar sobre la geometria evaluada; si ya no se cumple ⇒
    E10. Contenido: estado dinamico evaluado, clases encontradas, geometria aplanada, definiciones anidadas,
    transformacion y firma de parametros; en **serializacion canonica** (ordenada por contenido, nunca por el orden de
    iteracion del BTR, que DRAWORDER, SORTENTS o las ediciones alteran). Los bits exactos sirven para la trazabilidad; la
    equivalencia geometrica usa tolerancias.
11. **Sin forma afin en el primer corte (V5-05).** `c = a0 + a1·Parametro` **no** forma parte del camino normativo: el
    centro se verifica en cada estado usado. Solo se registra como optimizacion futura si existe metadata autoritativa que
    derive la relacion. I-52 **no** interpreta grafos de acciones de bloques dinamicos (G-M17). Desaparecen los «estados
    caracterizados», el `CharacterizedDomain` y la regla de rechazo de bloques estirados de V4 (§11.3 puntos 5, 6 y 11 de
    V4).
12. **Estados futuros por variables (V5-06).** La verificacion de un estado concreto **no** basta si el mismo rack puede
    cambiar ese estado con `ChangeValue`:

    ```text
    Si cualquier parametro dinamico necesario para GeometricEvidence depende directa o transitivamente de una
    propiedad vinculable, la evidencia de un unico estado NO satisface PV-7. Para ALLOW se exige una de:
      1. prueba geometrica valida sobre TODO el dominio autoritativo del parametro;
      2. autoridad integrada que garantice una propiedad geometrica universal;
      3. parametro demostrado independiente del vinculo.
    Si ninguna existe: UNKNOWN → FAIL_CLOSED.  No se infiere universalidad con muestras finitas.
    ```

    En `main` no hay dominio autoritativo (§6.5.5) ni autoridad de simetria para un continuo de `LONGITUD`: solo cabe la
    via 3, que V-DEP concreta en DEP-09 (largueros: `¬Dep(i)`) y DEP-10 (postes: `IndepAltura`). Si I-49 integra, se
    consume su dominio y su `PlanReadSet`; I-52 no inventa analisis propio.
13. **Marco del centro.** El drawer crea cada pieza como `BlockReference(Position = q, Rotation = r,
    ScaleFactors = (mX, mY, 1))` (`LateralHeaderDrawer.cs:305-314`), cuya transformacion es `T(q)·R(r)·S(mX,mY)·T(−o_p)`,
    con `o_p` = `BlockTableRecord.Origin` del bloque de pieza. El centro se mide en `u = x_BTR − o_p`, **no** en WCS, en el
    marco del rack ni en coordenadas crudas del BTR.
14. **G3 frente a produccion (V5-07).** G3 **no** añade la sonda productiva a `src/`: demuestra su viabilidad con
    `tools/`, un harness externo, evidencia del Owner en AutoCAD o artefactos en `docs/automation/evidence/` (CT-36). Debe
    cubrir evaluacion de bloques dinamicos, `Database` lateral, anidados, clases de entidad, geometria evaluada,
    definicion local frente a biblioteca, obsolescencia y rendimiento preliminar. Si G3 demuestra que la ruta no es viable
    ⇒ **Proposal V6** o reduccion de alcance. La sonda productiva nace en el Plugin en el gate en que la necesite la autoridad
    de planes o de materializacion (previsiblemente G6/G7 segun la division final); Application solo consume
    `GeometrySymmetryResult` (con dobles en las pruebas Core). Riesgo de rendimiento: R-24, medido en G7.
15. **Defensas adicionales.** El rechazo de piezas de rol `Tope` sigue como defensa adicional (hoy su fila es `UNKNOWN`),
    no como mecanismo principal. Ningun componente acepta una diferencia de planes como evidencia de centro (G-M15).
16. **Sin `GeometrySymmetryResult` verificado** (sonda no disponible, entidad no soportada, ciclo, ambiguedad, eje
    oblicuo o simetria no verificada): la vista no es equivalente ⇒ X-17a/X-17b ⇒ el rack falla cerrado (E6). Con estado
    dependiente de un vinculo sin evidencia universal ⇒ X-17c (S-26, S-27).
17. **Confirmacion del Owner.** El Owner confirma en G3 y G9 (O-3, M-15, M-27) sobre casos reales; su confirmacion **no**
    sustituye a la evidencia.

### 11.4 Parametros dinamicos

- El censo de claves y bloques **reutiliza las autoridades de I-19**: `CatalogBlockParameters`
  (`CatalogBlockParameters.cs:28`) y `CatalogBlockManifest` (`CatalogBlockManifest.cs`), cuya guarda builder → manifiesto
  ya impide que diverjan. **No** se crea un censo paralelo (CT-25).
- Hoy las claves son magnitudes: `LONGITUD`, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE` y `FONDO`
  (`SelectiveRackDefaults.cs:46-95`), mas los nombres configurables de cabecera, que por defecto son esas magnitudes
  (`LateralHeaderParameters.cs:66-72`).
- Una magnitud se compara numericamente con `GeometryTolerance.Length`. La **direccion** en que estira dentro del
  bloque es contenido del bloque y la cubre la evidencia geometrica evaluada por estado (§11.3).
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
  `UNKNOWN` ⇒ `FAIL_CLOSED`, **cualquiera sea el rol** de la pieza. La evidencia geometrica no lo corrige (§11.3
  puntos 2 y 9).
- Cada anclaje asimetrico produce una fila `UNKNOWN` con `FAIL_CLOSED` o una regla explicita aprobada por el Owner.
  Nunca un parche silencioso: una contradiccion material con V5 abre **Proposal V6**.

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
| CT-06 | **PlanPlacementEvidence** (diagnostico, despues de CT-36): piezas cuya mano difiere entre `F ∘ Π(D)` y `Π(μD)` por kind y vista, en el marco local tras `T(−BlockOrigin)`. **No** infiere ni justifica centros: con la simetria verificada por estado comprueba `A ≡ B` y, si difieren, registra el desplazamiento. Incluye los fixtures negativos de T-M40 | Core (con resultados geometricos registrados en CT-36) |
| CT-07 | `WithDesign` materializa `PalletTolerance` vinculada (hueco actual del camino `EditX`) | Core |
| CT-08 | `SelectiveDesviadorPlan.CellKey` colapsa N−1/N en el Dinamico | Core |
| CT-09 | `BracingPanel.ResolveDiagonalDirection` por paridad | Core |
| CT-10 | Ausencias de Push Back compuesto: inicio, interior y final dibujan distinto | Core |
| CT-11 | Marco del brazo Cantilever por lado y familia de seccion | Core |
| CT-12 | Semantica de `MountingFace` respecto del run (S-10, D-03b) | Core |
| CT-13 | `SelectiveAuthoredAuthority` distingue un documento reflejado de su origen | Core |
| CT-14 | Censo de `[CommandMethod(` = 33 y unicidad de nombres (linea base) | Core |
| CT-15 | **Topes**: holgura del tope Selectivo en frontal y planta (AR-01) y del **tope posterior de Push Back** en frontal posterior y planta (anclaje, `ElevationMirrored`, `LONGITUD`); predicado exacto de «tope activo»; **activo por defecto** (sin `OffCells` y con `PieceId` en blanco o desconocido dibuja en toda celda), por lado en compuesto, y `"(ninguno)"` (sin distinguir mayusculas) con mascara latente | Core |
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
| **CT-32** | **Metadata desconocida sobre el contrato de serializacion real**: `JsonTypeInfo` de las mismas opciones de cada store (modifiers, converters, `PropertyNameCaseInsensitive`, `[JsonIgnore]`, `[JsonExtensionData]`); payload por kind (raiz y anidados) y exterior (sobre y envoltorio); clave desconocida vacia y no vacia; portador de la allowlist; `CustomProperties` antes y despues de I-54; colisiones de nombres; **corpus de miembros retirados por ruta JSON** (historia de los tipos persistidos: RM-1..RM-6 y cualquier otro, incluidos los movidos de ruta) con clasificacion R-A/R-B segun lo que hace el store | Core |
| **CT-33** | **Fuentes dinamicas, anonimas o anotativas**: clasificacion ST-17 frente a ST-2 | guarda de texto; efecto real en M-23 |
| **CT-34** | **Historia de `View` vacia del Selectivo**: `View` introducida en `6023572` antes que `Section` en `4d2ca82`; decodificacion de produccion | Core + registro documental |
| **CT-35** | **V-RT-STORE por kind, separado de C-2**: `Miembros(LB)` e idempotencia del store sobre fixtures con y sin miembros opcionales | Core |
| **CT-36** | **Viabilidad de la STATE-EVALUATED GEOMETRIC EVIDENCE** en AutoCAD 2025, **fuera de `src/`** (`tools/`, harness externo, evidencia del Owner o artefactos en `docs/automation/evidence/`): representacion temporal o evaluada de bloques dinamicos en una `Database` lateral sin mutar el DWG; anidados recursivos y ciclos; clases de entidad de la politica; geometria evaluada y aplanada; definicion local frente a biblioteca (`DuplicateRecordCloning.Ignore`); re-verificacion de obsolescencia (`TraceFingerprint`); casos C, D y E; rendimiento preliminar por rack. Si la ruta no es viable ⇒ Proposal V6 o reduccion de alcance | evidencia registrada; Core consume los resultados |
| **CT-37** | **Diferencial de estabilidad 9b** sobre el corpus: dos o mas estados del registro por descriptor (con cota autoritativa si existe; sin cota: cero, negativos y grandes), fondo maestro distinto y cambio de gobernante; toda decision `ALLOW` en `R_a` conmuta en `R_b`; **V-BOM-1 y V-BOM-2 iguales entre `D` y `μ_k D` en cada estado** cuando el descriptor pueda afectar el BOM o la geometria que lo alimenta; **parametros dinamicos que varian con el vinculo** cubiertos por DEP-09/DEP-10; lema de definicion | Core |
| **CT-38** | **Cierre efectivo por descriptor**: variar el valor del descriptor y comparar por reflexion el sistema resuelto **y los parametros dinamicos de cada instancia de plan**; todo simbolo que cambia fuera del cierre declarado ⇒ RED | Core |
| **CT-39** | **Precedencia de fuente C1..C5** (§4.1): objetos no RackCad con escala, `Normal`, `Origin` o MINSERT no canonicos se ignoran sin abortar; bloque de usuario con racks anidados (ST-2b); payload ilegible (ST-2c) | Core (clasificacion pura) + guarda de texto de la captura |
| **CT-40** | **Read-set por observaciones** (§9.2.1): acreditacion usada, variables leidas, cambio irrelevante frente a cambio observado | Core |
| **CT-41** | **Coexistencia con I-53**: ningun tipo de I-52 en `RackCad.Application.Systems.Shared`; C-08 y C-10 de I-53 siguen verdes al reconciliar | Core |
| **CT-42** | **Enums con el contrato real del converter**: nombres desconocidos (fallo de lectura, E2), enteros no definidos (X-20), instantanea con `HeaderBlockRole` y `CantileverVisualRole`, invariante forward-compatible de `DimensionViewVisibility` | Core |
| **CT-43** | **`OffCells` latentes del tope posterior**: indices validos remapeados (P-05b, PC-08b) y fuera de rango `UNKNOWN` (P-05c, PC-08c), por lado | Core |
| **CT-44** | **Parametros dinamicos que varian con vinculos**: `LONGITUD` de largueros con y sin `Dep(i)`; `LONGITUD` de postes con y sin `IndepAltura` (cabecera por poste, `HeightOverride` en claros adyacentes, claros vacios); dos estados del registro | Core |

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
| T-M27 | Equivalencia con `GeometrySymmetryResult` verificado (dobles en Core): desplazamiento con el centro verificado en el marco local tras `T(−BlockOrigin)` y volteo de mano; `IsSymmetric = false`, clase no soportada, ciclo o eje oblicuo ⇒ no equivalente | G6 |
| T-M28 | Deteccion `UK` de topes Selectivo (S-13/S-13b), **tope posterior de Push Back activo (P-04, P-05, PC-08)** —incluida la configuracion por defecto sin `OffCells` y con `PieceId` en blanco—, parrilla desbordada (S-15x) y Auto con baseline (H-06); `"(ninguno)"` (sin distinguir mayusculas) inactivo con mascara remapeada (P-05b, PC-08b) | G5 |
| T-M29 | C-2 con el ejecutor de variables del Selectivo (C2-1 = C2-3) | **G6** |
| T-M30 | C-2 con el estado del editor → sistema (C2-1 = C2-4), en Core y en UI cuando pase por la ventana | **G6** |
| **T-M31** | **Obligacion 9**: con dos estados del registro, S-02b detectada (`MC`) cuando `Dep(i)`; S-02a (`RN`) conmuta en ambos cuando todas las celdas gobernantes tienen `BeamLengthOverride` | G5 |
| **T-M32** | S-14c `UK` cuando el espacio de indices del desviador depende de un medio frente con `Dep(i)`; dos estados del registro | G5 |
| **T-M33** | **V-VIEW-ALL**: se enumeran todas las vistas admisibles sin leer el dibujo; una vista admisible **no seleccionada** que no conmuta hace fallar el rack | G6 |
| **T-M34** | Tarimas: S-17c `UK` con fila desbordada; S-17b sin desborde conmuta; S-17d `UK` con `Dep(i)` sin `DEP-ROW`; **Push Back**: P-03c y PC-14b `UK` con fila desbordada; P-03b y PC-14a conmutan | G5/G6 |
| **T-M35** | **V-META**: desconocido no vacio en el payload ⇒ `UK`; clave exterior desconocida no vacia (sobre o envoltorio) ⇒ `UK`; clave desconocida vacia, portadores de la allowlist y retirados **declarados por el store** (RM-5, RM-6) no bloquean; retirados R-A y R-B ⇒ `UK` (T-M50); duplicado ambiguo de `CustomProperties` ⇒ `UK` | G5 |
| **T-M36** | **Cierre transitivo**: desde las raices de §6.7, cada clave `(kind, ruta)` alcanzada y cada descriptor clasificados exactamente una vez; tipo, miembro, corte o descriptor nuevo sin clasificar ⇒ RED; un tipo nuevo agregado a un DTO existente aparece sin tocar ninguna lista de tipos | G5 |
| **T-M37** | ST-17: referencia dinamica, anonima o anotativa con payload RackCad ⇒ E4 con mensaje propio; sin payload ⇒ ST-2 | G7 |
| **T-M38** | **V-RT-STORE** (`Miembros(LB) ⊆ Miembros(M)` e idempotencia), independiente de C-2 | G5 |
| **T-M39** | V-BOM-1 frente a V-BOM-2: un fixture donde difieren las precisiones demuestra que ninguna sustituye a la otra | G5 |
| **T-M40** | **Negativa de simetria generalizada**: siguen **no** equivalentes (1) fixture de rol `Tope` (AR-01 y tope posterior de Push Back); (2) pieza de otro rol con parametro de longitud y `δ` unilateral (caso C); (3) pieza de ancho fijo anclada a un lado (caso D); (4) **pieza asimetrica con caja envolvente simetrica** (caso E): la verificacion por multiconjunto la rechaza; (5) centro candidato deliberadamente incorrecto. Ninguna depende del rechazo por rol `Tope`, que queda como defensa adicional | G6 |
| **T-M41** | Read-set por observaciones (§9.2.1): un cambio irrelevante no aborta; un cambio de una observacion (acreditacion, variable leida, dependencia transitiva si I-49) aborta en PREPARE y en MUTATE | G4 (comparador) y G7 |
| **T-M42** | `RepresentabilityReport` lleva las tres dimensiones separadas por fila (clase, evidencia, disposicion) | G5 |
| **T-M43** | **V-DEP**: toda fila `ALLOW`/`CANONICALIZE` declara `EffectiveInputs`; las que intersecan un cierre de descriptor tienen entrada V-DEP con prueba o motivo; fila sin declaracion ⇒ RED | G5 |
| **T-M44** | Instantanea de enums alcanzados y de decodificacion de secciones; valor nuevo, renombrado o retirado ⇒ RED | G5 |
| **T-M45** | `DEP-ROW` con dos estados del registro: casos A y B ⇒ S-17d y S-15d `UK`; con overrides que cubren la fila ⇒ `ALLOW` y conmuta en ambos estados | G5 |
| **T-M46** | Precedencia de fuente C1..C5: objetos no RackCad con escala, `Normal`, `Origin` o MINSERT no canonicos se ignoran sin abortar; ST-2b con aviso especifico; ST-2c ⇒ E2 | G4 (clasificacion pura) y G7 (captura) |
| **T-M47** | Desde G5, ninguna fila del `RepresentabilityReport` lleva `EvidenceStatus = G3_PENDING` (regla de cierre de G3) | G5 |
| **T-M48** | Consumo de `GeometrySymmetryResult`: sonda no disponible ⇒ X-17a; clase no soportada o ciclo ⇒ X-17b; correspondencia ambigua ⇒ X-17a; derivacion del eje (multiplo de `π/2`) y eje oblicuo ⇒ `UNKNOWN` | G6 |
| **T-M49** | Estado dinamico y vinculos: evidencia de un estado con `Dep(i)` o sin `IndepAltura` ⇒ S-26/S-27 `UK` (X-17c); con independencia demostrada ⇒ `ALLOW` si `IsSymmetric`; nunca universalidad por muestras | G5/G6 |
| **T-M50** | Miembros retirados: R-A con el mensaje de remedio y R-B con el mensaje sin remedio; ninguna limpieza automatica; retirados declarados por el store (RM-5, RM-6) no bloquean | G5 |
| **T-M51** | Contrato de serializacion: colisiones de nombres ⇒ X-21; enteros no definidos ⇒ X-20; la guarda se alimenta del `JsonTypeInfo` real y no de reflexion CLR | G5 |
| **T-M52** | `OffCells` latentes: indices fuera de rango ⇒ P-05c/PC-08c `UK`; indices validos remapeados | G5 |
| **T-M53** | Sonda sin mutacion: la evaluacion no deja definiciones, referencias ni capas nuevas en el dibujo del usuario (guarda G-M16 y prueba del Plugin donde sea ejecutable; efecto real en M-27) | G6/G7 |

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
| G-M10 | Ningun literal de simetria de bloques en el codigo: un cambio de mano solo se acepta con `GeometrySymmetryResult` verificado por estado (§11.3) |
| G-M11 | El materializador de I-52 comprueba `MissingInstances` tras `CreateSystemBlock` y lanza; `LateralHeaderDrawer.cs` sin cambios |
| G-M12 | Si no se extrae un SNAPSHOT compartido: `RackDuplicarCommands.cs` identico al de la base y guardas de I-51 sin reapuntar |
| **G-M13** | Las tablas de clasificacion de miembros y de descriptores viven en Application, una por kind junto a su reflector; el Plugin no contiene clasificacion ni `switch` por kind |
| **G-M14** | Ningun tipo de I-52 en el namespace `RackCad.Application.Systems.Shared` (coexistencia con C-08 y C-10 de I-53); I-52 no modifica `SharedFoundationInspection` |
| **G-M15** | Ningun componente del espejo acepta una diferencia de planes como evidencia de centro: el comparador y el registro no exponen API que derive `c` de planes |
| **G-M16** | La sonda geometrica no abre en escritura la base de datos del dibujo del usuario ni deja artefactos: trabaja en solo lectura o en una `Database` lateral |
| **G-M17** | Ningun archivo del espejo interpreta grafos de acciones de bloques dinamicos ni contiene una forma afin del centro |

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
| M-12 | Push Back compuesto con el tope posterior **inactivo en ambos lados** (`"(ninguno)"` en A y en B, o todas las celdas apagadas; el estado por defecto es activo y falla cerrado): frontales 0..3 + planta |
| M-13 | Cantilever frontal + planta |
| M-14 | Cabecera lateral + planta con diagonales explicitas y postes asimetricos |
| M-15 | Confirmacion visual, sobre piezas reales, de que las simetrias aceptadas por la sonda son correctas (O-3); la confirmacion no sustituye a la evidencia |
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
| **M-27** | Sonda geometrica en AutoCAD: bloques dinamicos con varias longitudes, anidados, una definicion local distinta de la biblioteca con el mismo nombre (se usa la local) y rendimiento en un rack grande; el dibujo no gana artefactos |
| **M-28** | Payload legado con un miembro retirado: mensaje R-A (y tras RACKEDITAR → Actualizar el espejo funciona) y mensaje R-B (sin remedio) |
| **M-29** | Selectivo con `PalletTolerance` o `VerticalClearance` vinculadas: falla cerrado con mensaje; con overrides que fijan el ancho y alturas fijadas, la copia sigue siendo espejo tras `ChangeValue` |

---

## 14. Proceso y gates

### 14.1 Secuencia normativa (sustituye a la de V4)

```text
Coordinador + Arquitecto de acuerdo sobre V5 (misma version, exact-SHA)
→ rebase / reconciliacion sobre el main vigente         (solo docs; conflicto del indice de ADR: 0035..0039)
→ freeze por SHA exacto + re-check exact-SHA            (Coordinador y Arquitecto)
     si el rebase cambia contenido normativo material → vuelve a revision
→ Owner O-1                                              (envolvente maxima de alcance, §1.3)
→ G3 characterization                                    (ADR-0036 NO es precondicion; sonda solo fuera de src/)
     si G3 contradice materialmente V5 o reduce materialmente la envolvente → Proposal V6 (y O-1 se reevalua)
     si no                                               → Owner acepta ADR-0036 (O-4)
→ G4 implementation                                      (ADR-0036 aceptado ES precondicion)
```

### 14.2 Tabla de gates

Cada gate declara entrada, archivos permitidos, RED esperado, PASS requerido, parada y evidencia por SHA exacto. Ningun
gate arranca sin la orden explicita del Coordinador y sin el preflight de paralelas (§15.3). **Cualquier contradiccion
de una caracterizacion o de una prueba con el contrato congelado ⇒ STOP → Proposal V6**; nunca un parche silencioso.

| Gate | Entrada | Archivos permitidos | RED esperado | PASS requerido | Parada | Evidencia |
|---|---|---|---|---|---|---|
| **G2** (este) | revision de V4 | `docs/**` | — | consenso de Coordinador y Arquitecto sobre la misma version (V5); despues, rebase/reconciliacion, freeze por SHA, re-check normativo y **O-1** del Owner sobre la envolvente de §1.3; contrato reescrito; hallazgos laterales en `ideas-futuras.md` | desacuerdo material ⇒ Proposal V6 | SHA acordado, SHA de freeze + CI |
| **G3** Characterization | freeze de G2 + O-1 aceptada (**sin** ADR aceptado) | `tests/RackCad.Tests/**` (caracterizaciones y fixtures nuevos); `tests/RackCad.UI.Tests/**` si una caracterizacion lo exige; `docs/automation/evidence/**`; la viabilidad de la sonda por estado se demuestra **fuera de `src/`** (`tools/`, harness externo o evidencia del Owner) en la ubicacion que fije el Coordinador al abrir G3; **ninguna sonda productiva en `src/`** | ninguno: CT-01..CT-44 pasan sobre produccion intacta | Core completo (UI si se toco); **viabilidad de la sonda (CT-36) antes que `PlanPlacementEvidence` (CT-06)**; cierre transitivo sobre el contrato real, enums y retirados (CT-31, CT-32, CT-42); tabla de `c`; auditoria de anclajes por pieza (CT-17); semantica observable de I-51; decodificacion de secciones e historia de `View`; lineas base de store; topes, con el tope posterior de Push Back activo por defecto y `OffCells` latentes (CT-15, CT-43); estabilidad 9b con casos A/B, cotas, cambio de gobernante, BOM y parametros dinamicos vinculados (CT-27, CT-28, CT-37, CT-38, CT-44); S-14 asociado; enumeracion `A_k`; tarimas del Selectivo y de Push Back (CT-20); precedencia de fuente (CT-39); read-set (CT-40); costuras de Actualizar e Insertar (CT-26); coexistencia con I-53 (CT-41); fuentes dinamicas/anonimas; V-RT-STORE separado de C-2; **regla de cierre: ninguna fila `G3_PENDING` ni `EvidenceStatus` implicito** (§7.0) | contradiccion material con V5, sonda no viable o envolvente de O-1 reducida materialmente ⇒ **STOP → Proposal V6** | SHA + conteos + CI + registros de evidencia. Tras PASS: se pide **O-4** |
| **G4** Geometria y nucleo neutral | G3 PASS **y ADR-0036 aceptado por el Owner** | `src/RackCad.Application/Geometry/*`; nucleo neutral y fachada en `src/RackCad.Application/Persistence/` (incluye `RackDuplicationPlan.cs`, **con aviso previo a I-49**); pruebas | T-M01..T-M05, T-M21 y T-M41 (comparadores puros) en rojo sobre andamiaje | verdes; T1–T15 de I-51 y CT-16 identicos (textos, orden, avisos, errores); `RackDuplicarCommands.cs` sin cambios; tolerancia de escala fijada y registrada; **sin cambio observable de RACKDUPLICAR** | cambio observable de RACKDUPLICAR | SHA + conteos + CI |
| **G5** Reflectores y representabilidad | G4 | contrato, registros y tablas de clasificacion en `src/RackCad.Application/Mirror/`; reflector por kind en `src/RackCad.Application/Systems/<Kind>/` y `RackFrames/` (nunca en `Systems/Shared`); contrato de datos `GeometrySymmetryResult` y su consumo puro; declaracion de invariancia de metadata vacia; allowlist de portadores; registro de retirados; autoridad V-DEP con DEP-09/DEP-10; guarda sobre el contrato de serializacion; pruebas. Divisible en G5a Selectivo, G5b Dinamico y Push Back, G5c Cantilever y cabecera | T-M06..T-M11, T-M18 (portador), T-M22, T-M24, T-M25, T-M28, T-M31, T-M32, T-M34, T-M35, T-M36, T-M38, T-M39, T-M42..T-M45, T-M47, T-M49..T-M52 en rojo | verdes; sustrato real; topes Selectivo y Push Back `UK`; H-06 `UK`; obligaciones 9 y 9b; V-DEP; V-META del payload, exterior, retirados, enums y colisiones; cierre transitivo con enums; sin `G3_PENDING`; `WithDesign` ausente | una fila `R` o `RN` resulta no equivalente ⇒ STOP → Proposal V6 | SHA por subgate + CI |
| **G6** Planes, materializador y **C-2** | G5 | autoridades de plan por kind en Application; `SystemBlockWriter.cs` (`CreateInTransaction`); materializador nuevo en `src/RackCad.Plugin/Systems/Shared/`; **sonda geometrica productiva en el Plugin** si la division final la situa aqui (si no, en G7); pruebas Core y `tests/RackCad.UI.Tests/**` para C-2 | T-M12..T-M14, **T-M17**, **T-M17b**, T-M23, T-M26, T-M27, **T-M29**, **T-M30**, T-M33, T-M40, T-M48, T-M53 (si la sonda nace aqui), G-M9, G-M16 y G-M17 en rojo | conmutacion y V-VIEW-ALL verdes con resultados geometricos; `MissingInstances` = fallo duro; **C2-1, C2-2a, C2-2b, C2-3 y C2-4 GREEN en CI** (Core y UI) con G-M9, nombre logico, estado efectivo, `View` y `Section` canonicas; builders y drawers sin cambio de comportamiento; Application sin `Autodesk.*`. **G6 no cierra sin C-2** | necesitar cambiar un builder o un drawer existente; divergencia de C-2 ⇒ decidir C-1 con Coordinador y Arquitecto antes de cerrar G6 | SHA + CI + build Debug de Plugin |
| **G7** Comando RACKMIRROR | G6 cerrado (**C-2 verde**, incluido C2-2b) | `src/RackCad.Plugin/RackMirrorCommands.cs` (nuevo) y SNAPSHOT propio; sonda geometrica productiva si no nacio en G6; archivos de guardas; `SelectiveEditorOpenTests.cs` (censo); `src/RackCad.UI/RackCommandReference.cs`. `RackDuplicarCommands.cs` **solo** si se decide extraer un SNAPSHOT compartido (§5.2) | T-M15, T-M16, T-M37, T-M41 (MUTATE), T-M46 (captura), T-M53, G-M1..G-M8 y G-M10..G-M17 en rojo por violacion temporal | guardas verdes; censo 34; build Debug de Plugin; **rendimiento de la sonda medido (R-24)**; mensajes R-A/R-B fijados (T-M50); sin extraccion, archivos y guardas de RACKDUPLICAR intactos (G-M12) | C-2 no verde ⇒ G7 no arranca; debilitar una guarda de I-51 o de I-53 | SHA + conteos + CI |
| **G8** Cruces y continuidad | G7 | pruebas cruzadas; continuidad de G-M9; produccion de `RACKEDITAR` **solo** si se activo C-1 con orden del Coordinador | T-M18 (cruce final), T-M19 y T-M20 en rojo cuando apliquen | cruces verdes con lo integrado (I-49, I-50, I-53, I-54); reconciliacion final con autoridades integradas | un cruce exige tocar §20.3 | SHA + CI |
| **G9** Conformidad, Candidato y Owner | G8 | ninguno de produccion salvo correcciones | — | rebase final si `main` avanzo; Core y UI locales; builds Debug; CI 4/4 exacto; cobertura; M-1..M-29 (M-17 incluido); O-3 confirmado | cualquier fallo ⇒ vuelta al gate que corresponda | SHA Candidato + corridas + veredicto del Owner |
| **G10** Documentacion e integracion | G9 | `docs/**`, `README.md` | — | WORKFLOW 4.5: cierre documental, merge `--no-ff`, CI del `MERGE_SHA`, cobertura, limpieza | CI post-merge rojo ⇒ correccion en la rama | SHAs de cierre y merge |

---

## 15. Coordinacion con iniciativas paralelas

### 15.1 Tips observados en el preflight de V5

`git fetch --all --prune` el 2026-09-13, antes de redactar V5:

```text
origin/main                                        = f8deb675c6d1ef0e64693b157d69c4cc170d7b24 (Merge I-50; sin avance desde V2)
I-52 feature/rackmirror-espejo-semantico           = 8e2ae4fb9606c246bcd5d666a8f10e35f042a901 (V4; base 46fcac2; sin rebase)
I-49 architecture/motor-expresiones-parametricas   = 82aa61b20c59f4377b17f57793eac8edcfd9079f (V6 congelada; ADR-0038 ACEPTADO; G4
                                                     documental cerrado; G5 y produccion NOT STARTED; base f8deb67;
                                                     solo docs)
I-53 feature/cabeceras-configurables-multidestino  = f271fd578b3e583f1261d9f30e97d90a838bb7fe (G6: consumidor Dinamico; base f8deb67;
                                                     produccion ADITIVA en Application; sin UI, Plugin, DTO,
                                                     persistencia ni dibujo; sin llamador productivo; ADR-0037
                                                     aceptado en su rama)
I-54 architecture/propiedades-personalizadas       = 386625259922fc90ec735f739d4d5ed8e9510873 (Proposal V5 con consenso congelado;
                                                     ADR-0039 ACEPTADO; base f8deb67; solo docs; sin produccion de
                                                     la feature)
I-50                                               = integrada en main; rama remota retirada
```

**Cambios productivos de paralelas frente a `main`.** Solo I-53, acumulados y aditivos:

| Gate de I-53 | Cambio | Relacion con I-52 |
|---|---|---|
| G3 (`4e00a27`) | `HeaderBatchCodes.cs`, `HeaderBatchOutcome.cs`, `HeaderBatchPlan.cs`, `HeaderConfigurationSnapshot.cs` en `Application/Systems/Shared` con guardas C-08/C-10 | I-52 no crea tipos en ese namespace (G-M14) |
| G4 (`7de424e`) | seis tipos `Selective*` nuevos y miembros aditivos `SelectivePostGeometry.PostPeralteFor`, `SelectiveCabeceraHeightReview.OfDestinations` y `SelectiveEditorState.ApplyHeaderBatch`; 0 lineas borradas | no cambian el resolvedor (Dep(i)), la autoridad authored ni el contrato persistido; lo que escribe `ApplyHeaderBatch` cae en N-S06/N-S07; C2-4 se re-mide si se conecta |
| G6 (`f271fd5`) | ocho tipos nuevos en `Application/Systems/Dynamic` (`DynamicHeaderAddress`, `DynamicHeaderBatch`, `DynamicHeaderBatchPreparation`, `DynamicHeaderBatchRequest`, `DynamicHeaderBatchState`, `DynamicHeaderSource`, `DynamicModuleTargets`, `DynamicRackRebuild`) y pruebas; en `DynamicRackModule.IsManualOverride` solo cambia el comentario XML | sin llamadores en UI, Plugin ni archivos existentes; no toca DTO, persistencia ni dibujo |

Ninguno modifica materialmente una autoridad de I-52 (§19): se registran **sin bloquear**. El re-fetch previo al commit de
V5 observo los mismos tips. La numeracion de ADR y la correccion de `[V4-D29]` estan en §16 y `[V5-D30]`. Un avance solo documental de
una paralela no obliga a Proposal V6.

Entre la base `46fcac2` y `main @ f8deb67` no cambian `RackDuplicationPlan.cs`, `RackDuplicarCommands.cs`,
`RackEnvelopeRestamp.cs`, `RackEmbedComposer.cs`, `SystemBlockWriter.cs`, `LateralHeaderDrawer.cs`,
`CantileverViewMaterializer.cs`, `BlockLibraryImporter.cs`, `RackProjectStore.cs`, `RackProject.cs` ni los builders de §8.1;
si cambian, por I-50, los archivos con lineas de portador `DimensionViews` y los emisores de cotas.

### 15.2 Cruces

| Paralela | Cruce | Tratamiento |
|---|---|---|
| **I-50** (**integrada** en `main @ f8deb67`) | `DimensionViews` en DTO, dominio y resolvedores (C-01..C-15); politica de cotas; ADR-0035 aceptado | `DimensionViews` es requisito normativo (§10.3; CA-5); T-M18 incondicional; firmas de anotaciones de C-2 y V-VIEW-ALL con la politica de I-50 |
| **I-49** (V6 congelada; ADR-0038 **aceptado**; `82aa61b`) | `expression` en `PropertyValues`; `PlanReadSet` (D19); dominio del consumidor declarado por el descriptor (P24.5); su freeze exige que la prueba de supervivencia de `expression` (P24.8) se escriba contra `RackDuplicationPlan` y el restamp vigentes en `main`; produccion no iniciada | **I-52 coordina con I-49 antes de tocar `RackDuplicationPlan.cs`** (G4); T-M19. Si I-49 integra antes: su `PlanReadSet` es RS-3 (§9.2.1), su dominio es la cota `b` de V-DEP y su descriptor se clasifica en la guarda (§6.7). V5 **no** presupone el motor: RS-3 y `b` son condicionales. I-52 no implementa un segundo motor |
| **I-53** (G6; `f271fd5`) | tipos `Header*` en `Systems.Shared` con C-08/C-10; consumidores Selectivo (G4) y Dinamico (G6) aditivos y sin cablear | I-52 no crea tipos en `Systems.Shared` (G-M14, CT-41) ni modifica `SharedFoundationInspection`; si I-53 conecta G5/G7 antes del gate relevante de I-52, **C2-4 se vuelve a medir**; todo tipo persistido nuevo de cabecera o de modulo queda en RED en el cierre transitivo; serializar si C-1 llegara a tocar comandos de `RACKEDITAR` |
| **I-54** (V5 congelada; ADR-0039 **aceptado**; `3866252`) | `CustomProperties` (`JsonElement?`) heredado por `Compose`; **D-21 vigente** (el espejo cumple: A y despues B); T-GRD-02 7 → 8; residuales F-14a/F-14b vigentes; la aceptacion de ADR-0039 solo cambio su encabezado (contenido identico al de `d84f480`) | CA-4 sin cambio (§6.4); F-14a ⇒ E2 (ST-2c); F-14b y regla F ⇒ E8 en P10 (§9.4); ID-5 intacto; quien integre despues re-apunta T-GRD-02; T-M20 |
| Documental | Filas de ROADMAP, final de `ideas-futuras.md`, `docs/adr/README.md` (0035 en `main`; 0036 de I-52; 0037 de I-53; 0038 de I-49; 0039 de I-54), censo de comandos 33 → 34 | Quien integre despues conserva todas las filas en orden numerico y re-mide los censos |

### 15.3 Protocolo antes de G3, G5, G7 y del Candidato

`git fetch --all --prune`; `git diff --name-only origin/main...origin/<rama>` de I-49, I-53 e I-54 (y de cualquier
iniciativa nueva); leer sus Proposals y contratos en el SHA exacto; buscar `docs/adr/0036-*` y citas de «ADR-0036» en
**todos** los refs (§16); comprobar los archivos de §19; si `main` avanzo, rebase segun WORKFLOW antes de escribir
codigo. Ademas: si I-49 integra, re-medir la cota autoritativa de V-DEP (§6.5.5) y la autoridad del read-set (§9.2.1);
si I-53 integra, re-medir C-08/C-10 y el cierre transitivo (§6.7); si I-54 integra, re-medir CA-4 y T-GRD-02; si I-53
conecta G5/G7 antes del gate relevante de I-52, volver a medir C2-4; buscar tambien 0037, 0038 y 0039 en el indice de
ADR.

---

## 16. ADR-0036: numeracion y aceptacion

- Archivo: `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`, estado **`propuesto`**, con fila en el indice.
  Actualizado en este gate con las correcciones de V5.
- Congela: espejo semantico por copia; `μ_k` canonica por kind; admisibilidad por vista con seccion canonica y
  verificacion de **todas** las vistas admisibles; fail-closed; reflectores en Application sobre el sustrato real;
  prohibicion de normalizaciones que congelen valores dependientes de vinculos; metadata semantica desconocida ⇒
  fail-closed; autoridad de planes; colocacion canonica sin escala negativa, `Origin = 0` y sin bloques dinamicos,
  anonimos o anotativos; identidad nueva; portadores; atomicidad semantica e importacion best-effort; simetria con evidencia
  geometrica evaluada por estado sobre la definicion real, con politica de entidades y sin forma afin; obligacion 9b y V-DEP;
  allowlist de portadores y metadata exterior; cierre transitivo de cobertura; C-2 con Insertar; verificacion dinamica
  por rack; taxonomia en tres dimensiones con regla de cierre de G3.

**Protocolo de numeracion (vigente desde V2).**

1. Un numero de ADR queda **reclamado** por la primera publicacion **observable** de un archivo `docs/adr/NNNN-*` en un
   ref remoto.
2. **Registro de I-52:** ADR-0036 se publico por primera vez en `origin/feature/rackmirror-espejo-semantico` con el
   commit `0fc7032bf15d03e7d478bbd9350f156708621c9d` (`2026-09-12T19:59:36-06:00`; CI del push `34731908035`, creada el
   `2026-09-13T01:59:44Z`). En los preflights de V3, V4 y V5 ningun otro ref contiene `docs/adr/0036-*`; las citas de «ADR-0036»
   en I-49, I-53 e I-54 se refieren a este ADR.
3. **Otros numeros observados:** ADR-0035 es de I-50 (aceptado e integrado en `main`). ADR-0037 lo publico I-53 en
   `d7f17ad` (`2026-09-12T21:24:06-06:00`); tras rebasar, su primera aparicion en el ref actual es `d0db698`
   (fecha de autor `21:24:06`, de commit `22:25:31`) y quedo **aceptado** en `5efaf7e`. ADR-0038 lo publico I-49 en
   `a160560` (`2026-09-13T00:43:42-06:00`) y el Owner lo **acepto** en `edafade` (`2026-09-13T01:58:43-06:00`); la
   fila del indice y el encabezado del ADR coinciden en cada commit, asi que la inconsistencia registrada en `[V4-D29]` no
   se reproduce (`[V5-D30]`). ADR-0039 lo publico I-54 en `d84f480` (`2026-09-13T03:07:47-06:00`, `propuesto` en el ADR,
   en el indice y en su registro) y el Owner lo **acepto** en `3866252` (`2026-09-13T03:45:42-06:00`). Todas son
   posteriores a 0036 y usan otro numero: **sin colision**.
4. **Antes de pedir la aceptacion del Owner**, I-52 busca 0036 en todos los refs: archivos `docs/adr/0036-*` y citas de
   «ADR-0036» o «adr/0036». Si existe una publicacion **anterior** de otro ADR-0036, I-52 renumera antes de la
   aceptacion; una publicacion posterior no obliga a I-52 a renumerar.
5. Una vez ADR-0036 sea **`aceptado`**, **no** se renumera (`adr/README.md`: un aceptado es inmutable).
6. Dos ADR **aceptados** con el mismo numero ⇒ **STOP** y escalado al Owner.

**Aceptacion (vigente desde V3).** Solo el Owner (O-4). **No** es precondicion de G3; se pide **despues de G3**, si G3
no contradice materialmente V5, y **es precondicion de G4**. Mientras tanto el ADR sigue `propuesto` y puede corregirse si G3
abre Proposal V6. `docs/adr/README.md` no cambia en este gate: titulo, estado y numero del indice siguen siendo
correctos.

---

## 17. Riesgos abiertos

| # | Riesgo | Mitigacion |
|---|---|---|
| R-1 | Casi todas las piezas de frontal y planta necesitan aceptar un cambio de mano: si la sonda por estado no es viable, o si la biblioteca usa clases de entidad fuera de la politica (textos, patrones, solidos), kinds enteros quedan en fail-closed | CT-36 en G3; O-1 lo acepta (L-22); cambio material de alcance ⇒ Proposal V6 |
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
| R-12 | La auditoria de anclajes (CT-17) puede degradar muchas filas `R` + `G3_PENDING` a `UK` | Regla de cierre de G3; Proposal V6 si el cambio es material; nunca parche silencioso |
| R-13 | G4 toca el planificador de RACKDUPLICAR, validado por el Owner en I-51 | CT-16 primero; fachada con semantica observable identica; M-17; aviso a I-49 |
| R-14 | V-META deja fuera racks escritos por un build mas nuevo con metadata semantica, ahora tambien en el sobre y el envoltorio | Fail-closed deliberado: sin transformador no hay espejo fiel; futuro con declaraciones de invariancia o entradas CA-6 |
| R-15 | C-2 sobre ventanas WPF exige pruebas de UI mas costosas, ahora en G6 | `tests/RackCad.UI.Tests` permitido en G3 y G6; LC-UI |
| R-16 | La importacion de biblioteca puede dejar dependencias tras un fallo y su UNDO no esta demostrado | §9.3 y §9.5 lo declaran; M-10 y M-20 lo registran |
| **R-17** | Medios frentes con `PalletTolerance` vinculada y sin overrides quedan fuera | S-02b declarado; ALT-ABS como futuro |
| **R-18** | La guarda de cobertura obliga a clasificar cada miembro nuevo de cinco familias de tipos | Coste asumido: es la unica defensa ejecutable contra copias sin permutar |
| **R-19** | Aceptar el ADR despues de G3 añade una ronda del Owner entre G3 y G4 | Aceptado por el Coordinador: evita un ADR aceptado que G3 podria refutar |
| **R-20** | La obligacion 9b deja fuera racks con tarimas o parrillas cuyo ajuste depende de `PalletTolerance` vinculada sin overrides que las cubran (hoy no hay cota autoritativa) | S-15d/S-17d declarados; si I-49 integra, su dominio de consumidor puede ampliar lo admisible; M-25 |
| **R-21** | La evaluacion de bloques dinamicos en una `Database` lateral puede no reproducir exactamente la representacion del dibujo | CT-36; re-verificacion en PREPARE con la definicion real (§11.3 punto 3); sin paridad demostrada ⇒ X-17a |
| **R-22** | Nombres de tipos del espejo que choquen con las guardas C-08/C-10 de I-53 | carpeta `RackCad.Application.Mirror`; G-M14; CT-41 |
| **R-23** | El cierre transitivo obliga a clasificar tipos y enums que V3 no listaba | Coste asumido: es la unica defensa ejecutable contra copias sin regla; T-M36 y T-M44 |
| **R-24** | Rendimiento de la sonda por estado en selecciones grandes (muchas piezas y estados distintos) | cache por comando por `(definicion evaluada, parametros)`; medicion preliminar en CT-36 y medicion en G7 |
| **R-25** | Racks legados con `HighEndBeamPeralte` (R-B) no se pueden reflejar en I-52 y Actualizar no los limpia | fail-closed con mensaje sin remedio; limpieza futura fuera de I-52 |
| **R-26** | Los Selectivos con `PalletTolerance` o `VerticalClearance` vinculadas quedan fuera salvo overrides que fijen el ancho y alturas fijadas por override o cabecera por poste (S-26, S-27) | declarado en O-1 (L-23); futuro con una autoridad que demuestre simetria para un dominio |

---

## 18. Desacuerdos y decisiones

### 18.1 Decisiones registradas (no son consenso)

| # | Tema | Resolucion vigente | Donde |
|---|---|---|---|
| DA-1 | Convergencia con `RACKEDITAR` | C-2 por defecto con los productores reales (Actualizar **e Insertar**), CI, G-M9 y M-4/M-5; **GREEN en G6**; C-1 solo con evidencia | §8.5 |
| DA-2 | Simetria de bloques | Sin registro de centros: `GeometrySymmetryResult` por estado evaluado, sobre la definicion real, con politica de entidades, mas `PlanPlacementEvidence` | §11.3 |
| DA-3 | Politica post-commit | Sin `Regen`; un unico `Regen` solo con evidencia del Owner | §8.6 |
| DA-4 | Recorte del primer corte | Envolvente maxima de §1.3; O-1 despues del freeze tecnico y antes de G3 | §1.2, §1.3, §2 |
| DA-5 | `IsStandard`/`StandardBaselineId` | UNKNOWN → FAIL_CLOSED hasta G5 | §6.5.4 |
| DA-6 | Numeracion de ADR | Precedencia del primer ref remoto publicado | §16 |
| CR-V2-1 | Registro | Criterio de produccion; I-52 no lo relaja | §10.2 |
| CR-V2-3 | Tope posterior Push Back | `UNKNOWN → FAIL_CLOSED`; activo por defecto | §7.3 |
| CR-V3-2 | Estabilidad | Obligacion 9b; V-DEP; `W_lb` solo con cota autoritativa | §6.5.5 |
| CR-V3-3 | Metadata exterior | Allowlist cerrada de portadores | §6.4 |
| CR-V3-4 | Cobertura | Cierre transitivo con enums; coexistencia con I-53 | §6.7 |
| CR-V4-1 | Evidencia geometrica (V5-01..V5-05) | STATE-EVALUATED GEOMETRIC EVIDENCE como ruta primaria; politica de entidades; verificacion por multiconjunto; fingerprint solo de trazabilidad; sin forma afin | §11.3 |
| CR-V4-2 | Estados futuros (V5-06) | Evidencia de un estado no satisface PV-7 si el parametro depende de un vinculo; DEP-09/DEP-10 | §6.5.5, §11.3 |
| CR-V4-3 | G3 y sonda (V5-07) | Viabilidad en G3 fuera de `src/`; sonda productiva en el Plugin en G6/G7 | §11.3, §14.2 |
| CR-V4-4 | O-1 (V5-08) | Envolvente maxima con limitaciones nombradas; V6 y nueva O-1 si G3 la reduce | §1.2, §1.3 |
| CR-V4-5 | Contrato de serializacion y retirados (V5-12, V5-13) | `JsonTypeInfo` real; colisiones, enteros no definidos y retirados fallan cerrado; R-A con remedio y R-B sin el | §6.4, §6.7 |
| CR-V4-6 | Proceso | Contradiccion material de G3 ⇒ Proposal V6 | §7.0, §14.1 |

### 18.2 Desacuerdos abiertos

**Ninguno material.** Quedan diferidos con fail-closed los asuntos de §0.3. Para la atencion del Arquitecto, sin ser
desacuerdo:

- R-B (`HighEndBeamPeralte`, `[V5-D26]`) es precision de V5: el mensaje de remedio de la orden no es aplicable a un
  retirado que el store conserva. V5 propone un segundo texto sin remedio; el definitivo lo fija G7.
- DEP-10 (`IndepAltura`, `[V5-D27]`) es precision de V5: con `VerticalClearance` vinculada, los postes solo pasan con
  alturas fijadas por override o cabecera por poste. Reduce el alcance de los Selectivos con esa variable.
- La politica de entidades deja fuera cualquier bloque con textos o atributos; si la biblioteca real los usa en piezas
  estructurales, G3 lo mostrara (CT-36) y podria forzar Proposal V6.
- DEP-05..DEP-07 se declaran estables para posiciones y quedan `G3_PENDING` con CT-37 y CT-38.
- `[V5-D30]` corrige un registro: ni el indice de ADR de I-49 en `2beec61` ni el de I-54 en `d84f480` contradecian a sus
  ADR (lo afirmaban `[V4-D29]` y la nota transitoria de la revision). La numeracion no cambia.

---

## 19. Condiciones de parada

- Implementar sin la orden explicita del gate: **prohibido**.
- Una caracterizacion o prueba contradice **materialmente** el contrato congelado (alcance, ADR, regla de reflexion o
  arquitectura), reduce materialmente la envolvente de O-1, o una fila `R` o `RN` resulta no equivalente: **STOP →
  Proposal V6**.
- G3 demuestra que la sonda por estado no es viable en AutoCAD 2025: **STOP → Proposal V6** o reduccion de alcance.
- Necesitar cambio de schema, de sobre o de Xrecord, o de `RackEnvelopeRestamp.cs`, `PushBackMirror.cs`, `WithDesign`, un
  drawer o builder existente o el registro de variables: **detenerse**; nueva revision de Arquitecto y ADR.
- Un adaptador necesita una costura del store distinta de `WithSourceMetadataFrom`: **detenerse** (§6.2).
- Una paralela modifica materialmente `RackDuplicationPlan.cs`, `RackEmbedComposer.cs`, `RackEmbedDocument.cs`,
  `RackProjectDocument.cs`, `RackProjectStore.cs`, `SystemBlockWriter.cs`, `LateralHeaderDrawer.cs`,
  `CantileverViewMaterializer.cs`, `RackEnvelopeRestamp.cs`, `BlockLibraryImporter.cs`, `DynamicRackSystemResolver.cs`,
  `SelectiveGeometryResolver.cs`, `SelectivePostGeometry.cs` (salvo miembros aditivos sin llamador),
  `UsableProjectVariablesRegistry.cs`, `SelectiveLinkedPropertyKernel.cs`, los builders de §8.1, los stores authored o
  sus `JsonSerializerOptions`, o los descriptores vinculables: **detenerse** antes de editar y reportar SHA y diff. Un
  avance solo documental o una infraestructura aditiva no conectada se registra sin bloquear.
- Aparece un tipo, miembro, valor de enum, colision o descriptor vinculable alcanzado por el cierre transitivo sin
  clasificar: **RED / STOP** (§6.7).
- CT-38 encuentra un simbolo efectivo o un parametro dinamico fuera del cierre declarado de un descriptor: **RED / STOP**.
- Se intenta admitir un cambio de mano sin `GeometrySymmetryResult` verificado, derivar un centro de planes o de la caja
  envolvente sin verificar, inferir universalidad con muestras finitas o interpretar grafos de acciones de bloques
  dinamicos: **prohibido** (G-M15, G-M17).
- La sonda escribe en el dibujo del usuario: **prohibido** (G-M16).
- Se usa en `DEP-ROW` una cota inferior que no es autoritativa: **prohibido**.
- Un tipo de I-52 aparece en `RackCad.Application.Systems.Shared`: **RED** (G-M14).
- Aparece un parametro dinamico con semantica de lado sin regla: fail-closed y registro para Proposal V6.
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
| Application | carpeta nueva del espejo `src/RackCad.Application/Mirror/` (namespace `RackCad.Application.Mirror`; **nunca** `Systems/Shared`): contrato de reflector, registro, plan del espejo, decodificacion de vistas y `A_k`, equivalencia, contrato de datos `GeometrySymmetryResult` y su consumo, declaracion de invariancia de metadata, allowlist de portadores, registro de retirados, autoridad V-DEP comun y tablas de cobertura del exterior |
| Application | `Systems/Selective/`, `Systems/Dynamic/`, `Systems/PushBack/`, `Systems/Cantilever/`, `RackFrames/`: reflector, tabla de clasificacion y autoridad de plan por kind |
| Plugin | `RackMirrorCommands.cs` (nuevo) y SNAPSHOT propio; materializador en `Systems/Shared/`; `Systems/Shared/SystemBlockWriter.cs` (`CreateInTransaction`); **sonda geometrica por estado** (solo lectura o `Database` lateral) y `TraceFingerprint`, en G6 o G7 (§11.3) |
| Plugin | `RackDuplicarCommands.cs` **solo** si se extrae un SNAPSHOT compartido; comandos de `RACKEDITAR` **solo** si se activa C-1 |
| UI | `RackCommandReference.cs` (ayuda) |

### 20.2 Pruebas

Caracterizaciones CT-01..CT-44; pruebas T-M01..T-M53 (con T-M17b); guardas G-M1..G-M17; evidencia de viabilidad de la sonda
en `docs/automation/evidence/` y, si hace falta, un harness en `tools/` (nunca en `src/` durante G3); `SelectiveEditorOpenTests.cs` (censo 34);
guardas de I-51 reapuntadas en `SelectiveDuplicationFailClosedTests.cs` solo si se extrae el SNAPSHOT; pruebas de UI
en `tests/RackCad.UI.Tests` para C-2 (G6) y las caracterizaciones de UI (G3).

### 20.3 NO TOCAR sin nueva revision de Arquitecto

`RackEnvelopeRestamp.cs`, `RackCloner.cs`, `PushBackMirror.cs`, `SelectivePalletDesign.cs`,
`SelectivePalletDesignDocument.cs` (incluido `WithDesign`), DTO y stores de todos los kinds, `RackProject.cs`,
`LateralHeaderDrawer.cs`, `CantileverViewMaterializer.cs`, `BlockLibraryImporter.cs`, builders de plan,
`ProjectVariables/*` salvo consumo de lectura, `Bom/*` salvo consumo, `Catalogs/*` salvo consumo (I-19),
`KindHandlers/*`, editores WPF, `assets/`, catalogos, biblioteca de bloques, `.github/`, ADR aceptados, y los archivos de
I-53 en `src/RackCad.Application/Systems/Shared/Header*` con sus guardas (`HeaderBatchContractTests.cs`,
`HeaderConfigurationFixtures.cs`) y sus consumidores `Systems/Selective/SelectiveHeader*`, `SelectivePostTargets.cs`,
`Systems/Dynamic/DynamicHeader*`, `DynamicModuleTargets.cs` y `DynamicRackRebuild.cs`.

---

## 21. Estado

```text
G0 = ACCEPTED        G1 = ACCEPTED (Discovery)
G2 = V5 EN REVISION  (este documento + ADR-0036 propuesto actualizado + docs/automation/decisions/I-52.md)
     V1 (0fc7032), V2 (0445718), V3 (545c222) y V4 (8e2ae4f) = historial;
     revision de V4: Architect: CHANGES REQUIRED — PROPOSAL V5
O-1 = PENDING        (envolvente de §1.3; se pide despues del freeze tecnico y antes de G3)
G3 = NOT OPEN        (requiere consenso sobre V5, rebase/reconciliacion, freeze, re-check y O-1)
G4+ = NO INICIADO    (G4 requiere ademas G3 sin contradiccion material, ninguna fila G3_PENDING y ADR-0036 aceptado)

Proposal Version = V5
Coordinator = REVIEW REQUIRED
Architect = REVIEW REQUIRED
Consensus = NOT REACHED
ADR-0036 = PROPOSED
O-1 = PENDING
G3 = NOT OPEN
Implementation = BLOCKED

SUBSTANTIVE IMPLEMENTATION: BLOCKED
```

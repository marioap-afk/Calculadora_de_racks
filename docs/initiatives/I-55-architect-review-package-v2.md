# I-55 — Paquete de revision del Arquitecto (Proposal V2)

```text
PROPOSAL V2 — NOT CONSENSUS
Coordinator    = REVIEW REQUIRED
Architect      = REVIEW REQUIRED
Consensus      = NOT REACHED
Implementation = BLOCKED
Open Material  = M-01 (propio) · X-1..X-8 (entre iniciativas, con I-52)

Objeto de la revision  = docs/initiatives/I-55-proposal-v2.md @ f84f303adc132a7d72ebc3e2c5cf3d7bf9faf6e1 (§8)
Misma version del plan = docs/initiatives/I-55-implementation-map-v2.md · docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
Sustituye a            = docs/initiatives/I-55-architect-review-package-v1.md (historico)
Base del codigo citado = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
Paralela cruzada       = I-52 Proposal V11 @ 2275f21 (secciones citadas sin cambio de numero; §3.7 y §8.1-§8.4 identicas a V10)
```

> **Que es este paquete.** Material autonomo para revisar V2. El ejecutor **no** emite ni simula el veredicto del Arquitecto.

## 1. Veredicto que se solicita

```text
Architect: AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
Hallazgos: BLOCKER | HIGH | MEDIUM | LOW — seccion, evidencia archivo:linea, cambio exigido
Version:   solo para el SHA exacto revisado
```

## 2. Que cambio de V1 a V2

| Tema | V1 | V2 |
|---|---|---|
| ID19 | Proyeccion ortografica como unica foundation | **Group Placement**: una transformacion rigida comun por ejecucion + politicas `Rigid` y `Orthographic` (§12) |
| Propiedades de hermanas | Sin gate; copia de la vista elegida | **Gate acotado al `RackId`** con la igualdad canonica de I-54 y sonda del `Id` de payloads no interpretables (§9.2) |
| Codec en ID19 | `Coerced` aceptado para leer el tipo fuente | `Coerced`, `Invalid` y variantes huerfanas fallan (§7.3) |
| PR-2 (H-02) | Regla nueva en Application consumida tambien por el ensamblador | Solo en el Plugin |
| PR-3 (H-13) | Prerrequisito | Retirado de I-55 |
| ADR-0042 | Sucesor de ADR-0010 | Complementa y enmienda reglas acotadas |
| Open Material | NONE | **M-01** |
| Coordinacion con I-52 | X-1..X-6 | X-1..X-8 (valor de colocacion; origen y tramo del eje de una vista) |

## 3. Preguntas para el Arquitecto

### A-6 — Group Placement (D-16, D-18; §12.2-§12.6)

- **Contrato.** Una **seleccion proyectada** = las referencias de una ejecucion; un **grupo `RackId`** = su subconjunto con un `RackId`.
  Un `SourceGroupFrame (B, φ_s)` y un `TargetGroupFrame (T, φ_t)` por ejecucion;
  `T_common = Transform2D.Translation(−B).Then(Transform2D.Rotation(α)).Then(Transform2D.Translation(T))`, rigida
  (`Determinant = +1`, `ScaleFactor = 1`), **una por ejecucion y nunca por grupo `RackId`**; `targetAnchor_r = T_common(sourceAnchor_r)`
  para toda referencia. Sobre `Transform2D` (`A/Geometry/Transform2D.cs:20-116`; `Rotation` `:51-57`, `Determinant` `:77`, `ScaleFactor`
  `:83`, `Then` `:86-93`, `RotationAngle` `:111-115`), sin sistema matematico paralelo. Todo lo que no depende de los puntos (resolucion,
  validacion, gates, planes, anclas fuente) ocurre antes de pedirlos (§12.1).
- **Por referencia.** `φ_r` = angulo de la parte lineal (una escala (−1,−1) cuenta como giro de π); ancla `A_r = M_r · a_s(r)` con la
  transformacion completa (incluido `Origin`); la referencia nueva usa una definicion con `Origin = 0`
  (`P/Drawing/LateralHeaderDrawer.cs:243`; `P/Drawing/Cantilever/CantileverViewMaterializer.cs:47`, `:110`) y
  `Position = targetAnchor_r − R(ρ_r)·a_t(r)`.
- **Invariantes.** INV-GRP-1: para todo par de referencias, de cualquier grupo, las diferencias de anclas destino son la rotacion `α` de
  las diferencias de anclas fuente. INV-GRP-2: `Rigid` → `ρ_r − φ_r = α`; `Orthographic` → `ρ_r` depende solo de `φ_t` y de la vista
  destino.
- **Politica `Rigid`.** `sourceAnchor = A_r`; `α = φ_t − φ_s`; `Z_r = T.Z + (A_r.Z − B.Z)`. Representa misma clase (variante segun
  OD-2.b), Frontal ↔ Lateral como sustitucion en sitio, orientaciones distintas y familias mezcladas.
- **Politica `Orthographic`.** Eje fisico conservado `K`; direccion destino `w` desde `φ_t` y el descriptor de la vista pedida; recta
  comun `e` de las direcciones de `+K` en la fuente (paralelas o fallo), **con el sentido que elige el destino**: el que maximiza las
  referencias con `σ_s = σ_t`, y en empate el de angulo en [−45°, 135°). Asi el orden a lo largo de `+K` se conserva, el resultado no
  salta con la rotacion y la ida y vuelta planta → frontal → planta es estable (una normalizacion fija del angulo invertia el orden de
  una fila de plantas sin girar; §23, hallazgo 26). Anclas en el extremo del tramo en `K` con coordenada minima en la direccion comun
  (con sentidos iguales es el origen fisico); `Z_r = T.Z`; superposicion evaluada antes de los puntos. Sentidos perpendiculares en
  destino fallan (OM-12).
- **Descriptores.** Tabla de §12.3: familia rack planta X = +D, Y = +R (`A/Systems/Selective/SelectivePlantaBuilder.cs:16-19`), frontal
  X = +R, lateral X = +D; Cantilever planta X = −R, Y = +D, frontal X = −R, lateral X = −D (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:209-219`;
  `A/Geometry/Spatial3D.cs:186-197`); desplazamientos y extension fisica desde el sistema resuelto, caracterizados en G3 junto con la CT-05
  de I-52 (X-8).
- **Preguntas.** ¿Es INV-GRP-1 sobre todos los pares la invariante correcta de CD-04? ¿Es correcto que la proyeccion viva en la politica de
  anclas y no en la transformacion? ¿Se aceptan los descriptores como metadatos caracterizados y no como segunda autoridad geometrica,
  compartidos con I-52 (X-8)? ¿Falta algun gesto que la foundation no pueda representar?

### A-7 — Gate de propiedades acotado al `RackId` (D-07; §9.2)

- **Diseno.** Miembros = el sobre elegido o fuente (siempre, tambien al curar un `Id` en blanco) + sobres interpretables, no dependientes
  de xref, con `Id == RackId` (criterio de I-54, `A/CustomProperties/RackCustomPropertiesAuthority.cs:378-391`). Colecciones leidas con
  `CustomPropertiesStore.ReadElement` (`A/Persistence/CustomPropertiesStore.cs:90`) y comparadas con `CustomPropertiesCanonicalForm`
  (`A/CustomProperties/CustomPropertiesCanonicalForm.cs:24-70`). Reglas en el orden de I-54 (`:268-334`): `Kind` en blanco o varios →
  fallo; coleccion no escribible → fallo; formas distintas → fallo; una forma → comun, se hereda. Un payload que el store no interpreta
  (`A/Persistence/RackEmbedDocument.cs:97-129`) pasa por una sonda del `Id` de nivel superior: igual al `RackId` → fallo; sin `Id` legible
  o con el `Id` de otro rack → no bloquea. D-15 cuenta los redibujos fallidos de esos mismos miembros.
- **Por que no la autoridad global.** Su pertenencia indeterminada bloquea cualquier rack ante un payload ilegible en cualquier parte
  (`A/CustomProperties/RackCustomPropertiesAuthority.cs:249-264`).
- **Frontera con I-54.** Solo consumo del store y de la forma canonica; ningun archivo del Plugin nombra la autoridad (`TGrd04`); el
  archivo del gate queda fuera de la clasificacion de `TGrd05` (`T/CustomPropertiesEdgeGuardTests.cs:287-298`), por lo que una guarda nueva
  fija su independencia; el Plugin aporta el texto del payload y la dependencia de xref, que `FindRackBlocks` no expone
  (`P/RackCommandSupport.cs:109-134`).
- **Residuales.** F-14a: un UTF-16 invalido hace fallar el barrido antes de mutar (`docs/adr/0039-custom-properties-persistencia-autoridad.md:222-223`).
  El remedio `RACKPROPIEDADES` puede estar en solo lectura por la autoridad global (`:266-269`) o por una unificacion bloqueada (`:291-295`):
  mensaje condicionado (R-15).
- **Preguntas.** ¿Es aceptable reutilizar la igualdad canonica fuera del borde de I-54? ¿Es aceptable la sonda tolerante del `Id` (lectura
  del `Id` de nivel superior de un JSON que el store rechaza) o debe tratarse de otro modo? ¿Se acepta el riesgo residual R-14?

### A-8 — Relacion entre ADR-0042 y ADR-0010 (D-12)

- **Tension.** El indice declara inmutable un aceptado, admite «Notas posteriores» con fecha y pide un ADR de reemplazo para cambiar una
  decision (`docs/adr/README.md:31-36`). ADR-0042 cambia dos reglas acotadas de ADR-0010 y conserva el resto.
- **Opciones.** (a) Complemento + nota posterior fechada en ADR-0010 que enlace las reglas enmendadas: CR-08 pide complementar y
  enmendar; la nota fechada es la forma propuesta de registrarlo. Precedentes de complemento (ADR-0028 y ADR-0029) y de nota que enlaza
  un ADR posterior sin reemplazo (ADR-0005, nota del 2026-07-27 sobre ADR-0021), aunque en esos casos no cambiaba una regla. (b) Reemplazo acotado con nota fechada que diga que sigue vigente, como ADR-0008 frente a
  ADR-0020 (`docs/adr/0008-secciones-unificadas-por-rol.md:3`, `:75`).
- **Pregunta.** ¿Cual respeta el sistema de ADR sin reescribir decisiones que no cambian?

### A-1, A-3 y A-4

- **A-1 (§7.3).** Codec con disposicion y politica por consumidor: ¿se acepta que la politica viva en el llamador y el codec sea una sola
  autoridad neutral compartida con I-52 (su §8.4)?
- **A-3 (§11).** Transaccion C, gate antes de preparar y de redibujar, D-15.
- **A-4 (§7.1).** Renombre de `DimensionViewKind`.

### D-01, D-05, D-08, D-10, D-13, D-17

- **D-01.** ALT-B: preparacion pura en Application desde el sistema resuelto, colocacion en el Plugin y builders como unica autoridad
  geometrica (§4), frente a seguir por sistema, un framework generico o un registro persistente (§18).
- **D-05.** `RackId` al aceptar la intencion; curacion de `Id` en blanco; el `Id` interior del Cantilever no se toca (H-13 fuera).
- **D-08.** Grupos `RackId` de una definicion (`P/RackLayoutCommands.cs:29-30`); varias definiciones o tipos fuente segun OD-7.c;
  `MInsertBlock`, referencias dinamicas, anonimas o anotativas con datos, escala de valor absoluto distinta de 1, no uniforme o reflejada
  (una celda de `RACKLAYOUT` hereda la de su semilla, `P/RackLayoutCommands.cs:250-254`), normal no Z o SCP no paralelo → fallo;
  `Origin ≠ 0` por transformacion completa; kind con `TryResolve` (`P/RackMenuCommands.cs:141`); `Coerced`, `Invalid` y huerfanas → fallo.
- **D-10.** Limpieza ante excepcion en `PlaceAndReport` (`P/Drawing/BlockPlacement.cs:49-52`) y en la ruta del Cantilever
  (`P/RackCantileverCommands.cs:151-157`, `:174-177`).
- **D-13 / D-17.** Resolucion antes de validar y de pedir puntos; re-verificacion de bloques antes de escribir; diagnosticos bloqueantes
  y paridad sin editor.

### X-1..X-8 (§17.2)

| # | Pregunta |
|---|---|
| X-1 | ¿Una extraccion del nucleo de seleccion, con hechos neutrales en el nucleo y politicas por llamador? |
| X-2 | ¿Resolucion separada + paso comun «sistema resuelto → plan» con todas las direcciones + codec neutral con politica por consumidor? |
| X-3 | ¿Comparador authored declarado por el contrato del kind, con miembros como parametro? |
| X-4 | ¿Primitivo «definicion + sobre + referencia con rotacion en la transaccion del llamador», con politicas de equivalencia visual solo en I-52? |
| X-5 | ¿Reporte previo a I-52 en cada gate que dispara su clausula (§15.2, §19), sabiendo que la Proposal V2 ya la dispara? |
| X-6 | ¿La segunda iniciativa en integrar re-establece C-2 sobre el arbol combinado? |
| X-7 | ¿Un valor de colocacion `(Position2D, RotationRadians, UniformScale)` y una composicion sobre `Transform2D` compartidos (I-52 V11 §3.2, §8.3), con la rigidez como invariante de I-55 y la reflexion como politica de I-52? |
| X-8 | ¿Un solo descriptor de origen y tramo del eje por vista, compartido entre `RackViewFrame` y el «tramo del eje de la vista» de I-52 V11 §8.1, con una sola caracterizacion (CT-05)? |
| [V11-D10] | ¿Se acepta la tabla de la Proposal V2 §17.2 que asigna las seis autoridades compartidas a X-1..X-8, y que I-55 no congele sin esa reconciliacion registrada (STOP simetrico)? |

## 4. Testabilidad (I-45)

| Regla | Donde | Evidencia |
|---|---|---|
| `CommonTransform2D`, marcos, politicas, plan de grupo | Application | `CommonTransform2DTests`, `RackGroupFrameTests`, `RackRigidPlacementPolicyTests`, `RackOrthographicPlacementPolicyTests`, `RackGroupPlacementPlanTests` |
| Gate de propiedades y sonda del `Id` | Application | `RackSiblingCustomPropertiesGateTests`, `RackEnvelopeIdProbeTests` + guardas de cableado e independencia |
| Codec con politica de ID19 | Application | `RackViewEnvelopeCodecTests` |
| Descriptores y extension fisica | Application | `RackViewFrameCharacterizationTests` (G3), `RackViewFrameTests` |
| PR-2 | Plugin (guarda) + Application (builder) | `CantileverPlantaVisibilityGuardTests`, `CantileverPlantaVisibilityBuilderTests` |
| Resto | igual que V1 | mapa V2 §T |

Filtros por nombre de clase con conteo; sin rutas modales en pruebas de UI.

## 5. Censos y guardas

Reapuntadas con motivo: `TGrd02`, censo de comandos de `SelectiveEditorOpenTests` y censo por nombre `TGrd08`, ayuda, censos de ventanas y de
modales, `SelectiveAuthoredCarrierAdoptionTests`, `PushBackPluginSourceGuardTests`, `RackUnitsGuardSourceTests`,
`CantileverPluginSourceGuardTests`, pruebas de peticion, sesion y modulos. **Nuevas:** `RackSiblingGateWiringGuardTests`,
`RackSiblingGateIndependenceGuardTests`, `CantileverPlantaVisibilityGuardTests`, `RackSiblingScanInputCharacterizationTests`. **Sin
cambio:** `CustomPropertiesEdgeGuardTests.TGrd04_*` y `TGrd05_*`.

## 6. Lo que no se pide al Arquitecto

Las elecciones de producto de M-01 y OD-1..OD-8 (Owner via Coordinador), la aceptacion de ADR-0042 (Owner) y la prioridad entre iniciativas
(Coordinador). Si se pide su juicio tecnico sobre la formulacion de M-01.

## 7. Riesgos tecnicos

R-01 regresion al re-enrutar; R-03 autoridades de I-52 incompatibles; R-10 y R-11 descriptores frente a cambios o inconsistencias; R-13
eleccion de M-01; R-14 payload sin `Id` legible perteneciente al rack; R-15 remedio en solo lectura (mapa V2 §R).

## 8. SHA revisado

```text
PROPOSAL_V2_SHA = f84f303adc132a7d72ebc3e2c5cf3d7bf9faf6e1   (CI push 34863498499: success 4/4)
```

# I-55 — Paquete de revision del Arquitecto independiente (Proposal V3)

```text
PROPOSAL V3 — NOT CONSENSUS
Coordinator      = REVIEW REQUIRED
Architect formal = PENDING   ← se solicita a un Arquitecto INDEPENDIENTE de la sesion que redacto V2 y V3
Owner            = PENDING
Consensus        = NOT REACHED
Implementation   = BLOCKED
Open Material    = M-01 (propio) · adopcion de X-1..X-8 por I-52 · CQ-01 (decision del Coordinador)

Objeto de la revision  = docs/initiatives/I-55-proposal-v3.md @ 8e35a51058033c2876c1935afd3f3a94931748ae (§8)
Misma version del plan = docs/initiatives/I-55-implementation-map-v3.md · docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
Sustituye a            = docs/initiatives/I-55-architect-review-package-v2.md (historico)
Base del codigo citado = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093 (la rama no tiene cambios de produccion)
Paralela cruzada       = I-52 Proposal V13 @ dd45b0f (docs/initiatives/I-52-proposal-v13.md en feature/rackmirror-espejo-semantico)
```

> **Que es este paquete.** Material autonomo para que un Arquitecto **independiente** emita el veredicto formal sobre V3. El ejecutor
> **no** emite ni simula ese veredicto.
>
> **Revisiones previas, todas de la misma sesion de redaccion y ninguna formal:**
> - G2C (`d091eeb`, `docs/initiatives/I-55-architect-review-v2.md`): revision tecnica adversarial de V2, reclasificada por el Coordinador
>   en G2D; sus hallazgos AR2-01..AR2-20 estan incorporados (Proposal V3 §0.4).
> - Revision adversarial de V3 antes del commit (Proposal V3 §23): 62 hallazgos AV3-01..AV3-62 y 14 del pase de verificacion (VF-01..VF-14),
>   incorporados.
>
> Tomelas como evidencia que puede refutarse, no como acuerdo.

## 1. Veredicto que se solicita

```text
Architect: AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
Por pregunta (§3): AGREED | AGREED WITH CHANGES | CHANGES REQUIRED, con motivo
Hallazgos: BLOCKER | HIGH | MEDIUM | LOW — seccion, evidencia archivo:linea, cambio exigido
X-1..X-8:  COMPATIBLE | COMPATIBLE WITH CLARIFICATION | MATERIAL CONFLICT, contra I-52 V13
Version:   solo para el SHA exacto revisado
```

## 2. Que cambio de V2 a V3

| Tema | V2 (`f84f303`) | V3 |
|---|---|---|
| Direccion ortografica | `w` de una familia «rack» implicita; rechazaba Cantilever → Planta | `F_t(r)` por referencia (familia + tipo + variante destino); tabla de ocho pares; recta comun por suma alineada; sentido por mayoria con empate y `ε_ang` (§12.5) |
| Flujo de ID19 | Superposicion en VALIDATE con datos de PLANS | Etapa FRAMES sobre toda la seleccion; comprobaciones puras de bloques antes de los puntos (§12.1) |
| Invariantes de grupo | INV-GRP-1/2 | INV-GRP-3 e INV-GRP-4 sobre la colocacion real (§12.2) |
| Tramos | Por rack | `[K_min, K_max]` por (sistema, tipo, variante), cuadricula del sistema resuelto (§12.3) |
| Redibujo previo a Insertar | `REDRAW_FAILED` sin semantica parcial | Variante antes; PREPARE ALL con planes de hermanas; STOP por unidad; confirmadas permanecen; informe; huerfanas borradas con la primera colocacion; sin transaccion global por CD2-03, con CQ-01 abierta (§11) |
| Bloques de biblioteca | Union de nombres | Requisito estructural por pieza, por rol; alcance = instancias del plan; censo de omisiones de builders (§4.6) |
| Resolucion sin editor | «Camino del BOM» | `Resolve` compartido con resultado tipado, politica por consumidor, paridad en G3, extractor unico I-55 G7a con seis handlers (§4.5) |
| Codec | Con disposiciones que consultaban el sistema | Codec sintactico + disponibilidad (con catalogo) + politica por consumidor (§7.3) |
| API compartidas | Sin forma | `Resolve`/`Plan`/`Prepare`; semilla de nombre que reproduce los nombres base (§4.4) |
| X-1..X-8 | «Extrae el primero» en todas | Extractor unico I-55 para X-2 y X-8; primer gate en X-1, X-3, X-4 (requisito) y X-7; tabla de adopcion (§17.2, §17.2.1) |
| ADR-0042 | «Complementa y enmienda» | Complementario; ADR-0010 sigue aceptado; nota fechada solo al aceptarse (D-12) |

## 3. Preguntas

### A-1 — Variante, codec y disponibilidad (D-02, D-03; §7.2, §7.3)

- **Diseno.** `RackViewAddressCodec.Decode(kind, View, Section)` → direccion + disposicion sintactica (`Canonical`, `Canonicalizable`,
  `Coerced`, `Invalid`) sin consultar el sistema. Reglas: vacio = nulo o cadena vacia sin recortar; token con espacios = desconocido; la
  disposicion es la peor entre `View` y `Section`. `RackViewAvailability(sistema, catalogo, direccion)` → `Available`, `Orphaned`,
  `Unsupported`. Politica por consumidor: ID19 solo `Canonical`/`Canonicalizable` disponibles (`Coerced`/`Invalid` fallan en CLASSIFY);
  `RACKEDITAR` conserva su lectura por kind; I-52 la suya.
- **Evidencia a contrastar.** `P/RackSelectivoCommands.cs:126`, `:168`, `:194-198`, `:229-239`; `P/RackDinamicoCommands.cs:236-239`;
  `A/Systems/Dynamic/DynamicSystemLateralBuilder.cs:34-36`, `:67-80`; `P/RackPushBackCommands.cs:216-223`, `:253-269`;
  `A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:44`, `:50-52`; `P/RackCantileverCommands.cs:264-271`, `:304-315`;
  `A/Persistence/RackListBuilder.cs:117-118`.
- **Preguntas.** ¿La separacion en tres responsabilidades es correcta y compartible con I-52 V13 §3.7/§3.8? ¿Clasificar como `Coerced` las
  dos formas del lateral Dinamico sin seccion es correcto? ¿La politica de `RACKEDITAR` por kind esta bien descrita?

### A-3 — Transaccion de ID18 y redibujo previo (D-06, D-15; §11)

- **Diseno.** Transaccion C (PREPARE ALL + commit por colocacion, `P/Drawing/BlockPlacement.cs:174-201`). En un rack existente:
  variante → gate → PREPARE ALL (vistas nuevas, planes y payloads de hermanas) → redibujo legacy por unidad (importacion + redefinicion +
  payload + renombre; confirmada = commit) → STOP en el primer fallo de una hermana propia del dibujo (las dependientes de xref, aviso)
  sin vistas nuevas ni borrado de huerfanas, informe exacto y `Regen` si alguna confirmo → si todo confirma, huerfanas con borrado
  verificado (si eran las unicas vistas, se borran dentro de la transaccion del jig de la primera colocacion) y cola. Esc o Enter en la
  cola → cancelacion; `PromptStatus.Error` → fallo.
- **Evidencia para CQ-01.** `P/Systems/Shared/SystemBlockWriter.cs:82-116`; `P/Systems/Shared/ViewBlockDraw.cs:65`, `:97`;
  `P/Drawing/LateralHeaderDrawService.cs:60`, `:95`; `P/ProjectVariableMutationExecutor.cs:142`, `:246-293`;
  `P/RackCantileverCommands.cs:319-326`.
- **Preguntas.** ¿La maquina de estados de §11.2 es completa (incluidos `VARIANT_CANCELLED`, `CANCELLED_BEFORE_ANY`, `FAILED_BEFORE_ANY`)?
  ¿Es correcto borrar las huerfanas dentro de la transaccion del jig de la primera colocacion (`P/Drawing/BlockPlacement.cs:179-199`),
  sabiendo que la definicion nueva se confirma antes (`P/Systems/Shared/SystemBlockWriter.cs:27-36`)? ¿Es correcto tratar las hermanas
  dependientes de xref como aviso (OM-25)? **Opinion tecnica pedida para CQ-01** (la decision es del Coordinador): ¿mantener el redibujo
  por unidad o pasar a PREPARE → una MUTATE → POST con el primitivo existente?

### A-4 — `RackViewKind` (D-02; §7.1)

- **Diseno.** Renombre puro de `DimensionViewKind` (`A/Systems/Shared/DimensionViewPolicy.cs:11-16`) a `A/Views/RackViewKind.cs`; no se
  serializa; seis archivos de produccion y seis de pruebas. `CantileverViewKind` es un enum de camara (`AdapterSection`, `:36`). Solo I-55
  G6 lo hace; si la guarda de enums de I-52 V13 §6.7 ya lo alcanza, decide el Coordinador de I-52.
- **Pregunta.** ¿Se sostiene como renombre sin cambio de comportamiento?

### A-6 — Group Placement y politicas (D-16, D-18; §12)

- **Foundation.** Una `CommonTransform2D` rigida por ejecucion: `Translation(−B).Then(Rotation(α)).Then(Translation(T))`
  (`A/Geometry/Transform2D.cs:20-116`), valor de colocacion `(T − R(α)·B, α, 1)`; anclas `A_r = M_r · a_s(r)` con la transformacion completa;
  definicion nueva con `Origin = 0`; escala admitida solo `(1, 1, 1)` y `(−1, −1, +1)`.
- **Orthographic.** Por referencia, `F_t(r)` = familia de r + tipo destino + variante destino; `w(r) = R(φ_t + δ_t)·k_t(r)`; recta comun
  `c = normalizar(Σ signo(e_K(r)·e_K(r0))·e_K(r))`; sentido de `e` por mayoria de `σ_s = σ_t` (empate en [−45° − ε_ang, 135° − ε_ang));
  anclas por el extremo del tramo destino segun σ; `s_r = (A_r(κ_s) − B)·e`; `targetAnchor = T + s_r·w`; superposicion por interseccion de
  intervalos con `ε_ov`, calculada en FRAMES. Tabla de ocho pares y ejemplos numericos en §12.5. Ida y vuelta: solo el intervalo sobre K.
- **Descriptores.** §12.3 (`A/Systems/Selective/SelectivePlantaBuilder.cs:16-19`; `A/Systems/Selective/SelectiveFrontalBuilder.cs:50`;
  `A/Systems/Selective/SelectiveLateralBuilder.cs:82-97`; `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:209-219`;
  `A/Geometry/Spatial3D.cs:186-197`; `A/Systems/Cantilever/CantileverColumnBaseDatum.cs:19-24`, `:45`).
- **Preguntas.** ¿INV-GRP-1..4 son las invariantes correctas y verificables? ¿La etapa FRAMES y el orden de §12.1 son correctos? ¿Es
  aceptable el sentido por mayoria, que puede invertir toda la elevacion al girar racks (OM-24), o hay una regla mejor que no reabra el
  hallazgo 26 de V2? ¿Los tramos como cuadricula del sistema resuelto son la eleccion correcta?

### A-7 — Gate de propiedades acotado al `RackId` (D-07; §9.2)

- **Diseno.** Miembros = sobre elegido + sobres interpretables, no dependientes de xref, con `Id == RackId` (incluidas las huerfanas);
  igualdad canonica de I-54 (`A/CustomProperties/CustomPropertiesCanonicalForm.cs:24-70`); sonda del `Id` de nivel superior en payloads
  no interpretables; remedio por motivo (el `Kind` mezclado no tiene remedio automatico, OM-23); claves duplicadas en distinta
  capitalizacion como riesgo residual R-19.
- **Pregunta.** ¿Sigue siendo construible sin romper `TGrd02`, `TGrd04` ni `TGrd05`, y la politica de remedios es correcta?

### A-8 — Relacion entre ADR-0042 y ADR-0010 (D-12)

- **Formulacion final.** Complemento: generaliza el origen valido de una hermana (A: rack materializado; B: intencion de creacion
  aceptada) y anade precondiciones a Insertar (gate de propiedades y D-15). ADR-0010 sigue aceptado; la nota fechada en «Notas
  posteriores» solo al aceptarse ADR-0042 (`docs/adr/README.md:31-37`; `docs/adr/0010-actualizar-redibuja-insertar-liga-vistas.md:36-46`).
- **Pregunta.** ¿La formulacion respeta el sistema de ADR? ¿El nuevo orden del prompt de variante y el borrado de huerfanas en Insertar
  siguen dentro de «precondiciones del flujo de Insertar» o exigen otra forma de registro?

### D-05 — Ciclo de vida del `RackId` (§6)

`RackId` al aceptar la intencion; curacion vigente de un `Id` en blanco (`IsNullOrWhiteSpace` en I-55); ID19 falla ante un sobre sin `Id`;
RID-5 precisado (las definiciones de biblioteca importadas pueden quedar). ¿Correcto?

### D-10 — Limpieza ante excepcion (§22.1 detalle)

Solo definiciones sin referencias, best effort y registrada (`P/Drawing/BlockPlacement.cs:130-169`); purga de anidadas declarada (OM-21);
limpieza del Cantilever dentro de `PlaceDefinition` (`P/RackCantileverCommands.cs:142-161`). ¿Correcto?

### D-13 — Requisito estructural de bloques (§4.6)

Requiere bloque ⇔ rol distinto de `Annotation` y `Dimension` (`A/Drawing/HeaderBlockInstance.cs:7-50`;
`P/Drawing/LateralHeaderDrawer.cs:258-291`); comprobaciones 1-2 puras antes de los puntos y 3 tras importar; fallo antes de escribir
definiciones o referencias de rack en ID19 e I-52; ID17/ID18 colocan y reportan en todo camino; piezas omitidas por builders antes del
plan (`A/Systems/Selective/SelectiveLateralBuilder.cs:287-290`; `A/Systems/PushBack/PushBackLoadBeamGeometry.cs:139-143`) censadas en G3
(OM-22). ¿El alcance y la discriminacion son correctos?

### D-15 — Redibujo fallido (§11.1)

Ver A-3. ¿La unidad de redibujo, la clasificacion «actualizada con aviso», el borrado verificado de huerfanas y el tratamiento de las dependientes de xref son correctos?

### D-17 — `Resolve` compartido (§4.5)

Resultado tipado; contexto con el resultado de la lectura del registro; entrada persistida o en memoria; politica del BOM por kind sin
cambio observable (`P/RackInventarioCommands.BomTotal.cs:175-190`, `:200-206`, `:208-214`); ID19 solo `Resolved` y hermanas aceptables para
`RACKEDITAR`; paridad en G3 con fixture Dinamico solo-sistema (`A/Systems/Shared/SystemRegistry.Default.cs:74-87`;
`A/Persistence/DynamicRackSystemDocument.cs:196`, `:210`); `UnsupportedLegacy` sin miembro conocido; extractor unico I-55 G7a con los
seis handlers. ¿Es una sola autoridad bien delimitada?

### X-1..X-8 (§17.2, §17.2.1)

Contrastar con I-52 V13: [V13-D08] (§3.3, §3.7, §3.8, §5.1, §5.4, §6.1, §8.1..§8.5, §20.1, CT-04, CT-05 y CT-16 provisionales);
§15.4 (filas de G2C como entrada vinculante no adoptada; V3 §17.2 lista las diferencias); archivos permitidos por gate (§14.2: G4, G5, G6);
archivos excluidos (§20.3); protocolo (§15.3); ST-7..ST-10 (§4.1) y tolerancia de escala (§4.3).

| # | Pregunta |
|---|---|
| X-1 | ¿Nucleo neutral de seleccion con custodio I-52 y regla de primer gate (I-52 G4 / I-55 G13), con el asignador de identidad solo en I-52? |
| X-2 | ¿Extractor unico I-55 (G6, G7a, G7b) para taxonomia, codec, disponibilidad, `Resolve`, `Plan` y `Prepare`, con I-52 consumiendo en G4, G5 y G6 y STOP + decision de Coordinadores si llega antes? ¿El acoplamiento de orden es aceptable o es una incompatibilidad material? |
| X-3 | ¿Comparador authored en el contrato del kind, con el `IsSameAuthority` del reflector delegando y §5.4/§6.1 sin cambio? |
| X-4 | ¿Primitivo de materializacion de I-52 G6 consumido en I-55 G15, y requisito estructural puro en `A/Drawing/LibraryBlockRequirement.cs` con regla de primer gate? |
| X-5 | ¿Comprobacion de existencia y reporte antes de cada gate creador o consumidor, con I-52 anadiendo G4 y G6? |
| X-6 | ¿Un juego unico de C-2 re-establecido por la segunda iniciativa en integrar? |
| X-7 | ¿Un valor de colocacion, una descomposicion de la transformacion fuente y una tolerancia de escala en `A/Geometry/`, con politicas por llamador (I-52 admite `s ≠ 1` y falla con `Origin ≠ 0`; I-55 exige `\|s\| = 1` y lleva `Origin` en `M_r`)? |
| X-8 | ¿Un `RackViewFrame` con tramos en coordenadas fisicas, extraido solo por I-55 G6, con la `c` de I-52 derivada por el origen y δ en CT-06? |

## 4. Testabilidad (I-45)

| Regla | Donde | Evidencia |
|---|---|---|
| Codec, disponibilidad, soporte, marcos | Application | `RackViewAddressCodecTests`, `RackViewAvailabilityTests`, `RackViewSupportMatrixTests`, `RackViewFrameTests` (G6) |
| `Resolve` y delegacion | Application + guarda | `RackSystemResolutionTests`, `KindHandlerResolutionDelegationGuardTests`, `BomResolutionCharacterizationTests` (G3/G7a) |
| Semilla, plan, requisito estructural | Application | `RackViewNameSeedTests`, `RackViewPlannerTests`, `LibraryBlockRequirementTests` (G7b) |
| Redibujo previo a Insertar | Application + guarda | `RackSiblingRedrawOutcomeTests`, `RackSiblingGateWiringGuardTests` (G9) |
| Lote | Application | `RackViewBatchPlanTests` (G11) |
| Colocacion de grupo | Application | `CommonTransform2DTests`, `RackGroupFrameTests`, `RackRigidPlacementPolicyTests`, `RackOrthographicPlacementPolicyTests`, `RackGroupPlacementPlanTests` (G14) |
| Caracterizaciones | Core + UI | CT-04, CT-05, CT-16, paridad con fixtures legacy, preflights de `RACKEDITAR`, censo de omisiones, nombres base (G3) |

Filtros por nombre de clase con conteo; sin rutas modales en pruebas de UI. Matriz completa: mapa V3 §T.

## 5. Lo que no se pide al Arquitecto

Las elecciones de producto de M-01 y OD-1..OD-8 (Owner via Coordinador), la decision CQ-01 (Coordinador; si se pide opinion tecnica), la
aceptacion de ADR-0042 (Owner) y la prioridad entre iniciativas (Coordinadores).

## 6. Riesgos tecnicos

Mapa V3 §R: R-01 regresion al re-enrutar; R-03 I-52 no adopta la tabla; R-10 y R-11 descriptores o paridad inconsistentes; R-16 la
delegacion del BOM cambia un resultado; R-17 rack redibujado en parte; R-19 claves duplicadas; R-20 acoplamiento de X-2; R-21 sentido por
mayoria.

## 7. Revisiones previas (no formales)

| Revision | Donde | Uso |
|---|---|---|
| Revision tecnica adversarial de V2 (G2C) | `docs/initiatives/I-55-architect-review-v2.md` @ `d091eeb` | AR2-01..AR2-20 → Proposal V3 §0.4 |
| Revision adversarial de V3 | Proposal V3 §23 | AV3-01..AV3-62; pase de verificacion VF-01..VF-14 (§23.3) |

## 8. SHA revisado

```text
PROPOSAL_V3_SHA = 8e35a51058033c2876c1935afd3f3a94931748ae   (CI push 34881178359: success 4/4)
```

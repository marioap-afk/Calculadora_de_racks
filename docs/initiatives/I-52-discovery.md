# I-52 — ID16 — Discovery (G1): RACKMIRROR, espejo semantico de uno o varios racks

> **Esto es G1: un informe de caracterizacion.** No cambia codigo productivo ni pruebas. Las «estrategias
> candidatas» de las secciones 10 a 13 son **insumo para la Proposal V1**, no un contrato. Ninguna decision de
> producto ni de arquitectura queda cerrada aqui.
>
> ```text
> SUBSTANTIVE IMPLEMENTATION: BLOCKED
> Coordinator: NOT YET AGREED
> Architect: NOT YET AGREED
> ```
>
> Contrato: [I-52-rackmirror-espejo-semantico.md](I-52-rackmirror-espejo-semantico.md).

## 0. Baseline, metodo y notacion

```text
Rama             = feature/rackmirror-espejo-semantico
Worktree         = C:\Users\alejandra-mendoza\.codex\worktrees\feature-rackmirror-espejo-semantico
BASE_SHA         = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main = main, "Merge I-51")
CLAIM_SHA        = 55281769d5e9317ad25500e6d3f5f8a849f37279   (commit vacio)
Claim-Id         = 232b55c5-3d0c-47c7-90ae-a07caab06431
BOOTSTRAP_SHA    = 7ee79756d2b93b4eded660cb73310ef3edde32ff   (contrato + fila ROADMAP, solo docs)
Codigo auditado  = 46fcac2: src/, tests/ y assets/ byte-identicos en la rama (sus commits son solo docs)
I-49 observada   = origin/architecture/motor-expresiones-parametricas @ 4df9480 (solo docs)
I-50 observada   = origin/feature/cotas-independientes-por-vista      @ a4806ff (con produccion)
```

**Metodo.** Lectura de codigo, documentos y ramas remotas; **no** se ejecuto AutoCAD ni ninguna suite. Ocho
auditorias de solo lectura en paralelo (fundacion I-51; transformaciones; seleccion/vistas/RACKEDITAR;
Selectivo; Dinamico/Push Back/Cama; Cantilever/cabecera/registro; BOM/variables; colocacion de RACKLAYOUT) y,
despues, **verificacion directa por el ejecutor** de las afirmaciones que sostienen las conclusiones (lista V-01..V-34
en la seccion 23.4). Toda cita es `archivo:linea` sobre `46fcac2`.

**Notacion.**

- **FACT**: leido en codigo o documento citado. **INFERENCE**: razonamiento o comportamiento de AutoCAD no
  observable en el repositorio. **NO DECIDIDO**: pregunta de producto o de arquitectura.
- **Reflexiones locales** (nunca por el eje X del mundo):

  | Sistema | Reflexion A | Reflexion B |
  |---|---|---|
  | Selectivo | **RUN**: invierte frentes/postes (`f→M−1−f`, `p→M−p`) | **DEPTH**: invierte fondos (`k→N−1−k`, cara cercana ↔ lejana) |
  | Dinamico y Push Back de un sentido | **RT**: invierte frentes | **RD**: invierte profundidad (salida ↔ entrada) |
  | Push Back compuesto A/B | **RT** | **RD** = intercambio A ↔ B |
  | Cantilever | **R_X**: invierte el orden de estaciones | **R_Y**: intercambia lados ±Y de la columna |
  | Cabecera independiente | **R_D**: plano medio de profundidad (poste izquierdo ↔ derecho) | **R_F**: caras del marco (Front ↔ Back) |

- `M` = frentes del grid maestro; `N` = fondos (Selectivo) o frentes (Dinamico/Push Back) segun contexto, que se
  aclara en cada tabla.

---

## 1. Preflight completo

Ejecutado el 2026-09-12, antes del reclamo, y repetido inmediatamente antes de crear la rama:

| Comprobacion | Resultado |
|---|---|
| `git fetch --all --prune` | sin errores |
| `main` vs `origin/main` | ambos `46fcac2b071929d2bd5b07aa28373941417f74a8`, divergencia `0/0` |
| CI de `push` sobre `46fcac2` | corrida `34724848178`, `success` |
| Arbol principal | limpio |
| Worktrees | principal (`main`), I-49 (`4df9480`), I-50 (`a4806ff`); ninguno de I-52 |
| Ramas locales | `main`, `architecture/motor-expresiones-parametricas`, `feature/cotas-independientes-por-vista` |
| `git ls-remote --heads origin` | `main`, I-49, I-50. **Ninguna de I-52** |
| `git stash list` | vacio |
| Operaciones incompletas (los tres worktrees) | sin `MERGE_HEAD`, `CHERRY_PICK_HEAD`, `REVERT_HEAD`, `BISECT_LOG`, `REBASE_HEAD`, `rebase-merge`, `rebase-apply`, `sequencer` ni `AUTO_MERGE` |
| `git worktree prune --dry-run` | nada que podar |
| `docs/ROADMAP.md` | ultima fila I-51 (integrada); **sin fila I-52**; `ID16` no aparece |
| `docs/HANDOFF.md` §4 | I-51 integrada y cerrada; RACKMIRROR (**ID16**) listado como «Lo que I-51 dejo expresamente fuera» (`docs/HANDOFF.md:1558`) |
| `docs/WORKFLOW.md` | leido completo (§2 caso (d), §4.1 reclamo, §8 bootstrap) |
| `docs/ORCHESTRATION.md` | **no existe**; WORKFLOW manda (precedente I-42) |
| Registro de reclamos (`git log --all --grep=Claim-Id`) | I-37C..I-51; **ninguno de I-52** |
| `I-52` en `origin/main`, I-49 e I-50 (todo el arbol) | **cero** archivos |
| Commits con `I-52` en cualquier ref | **cero** |

## 2. Disponibilidad real de I-52

**LIBRE** al reclamar, conforme a WORKFLOW:

- Ninguna rama remota, fila, contrato, commit ni mencion de I-52 (seccion 1).
- Sin fila previa: se abre por **caso (d)** de WORKFLOW §2, autorizacion explicita del Owner transmitida por el
  Coordinador. El caso (d) exige reclamo atomico + bootstrap inmediato (contrato + fila) antes de trabajo
  sustantivo: cumplido (seccion 3).
- Estorbos: I-52 no tenia fila, asi que no habia estorbos declarados. El cruce real con I-49 e I-50 se mide en la
  seccion 18; en G0/G1 I-52 solo toca `docs/`.

## 3. BASE, reclamo y bootstrap

```text
BASE_SHA      = 46fcac2b071929d2bd5b07aa28373941417f74a8
CLAIM_SHA     = 55281769d5e9317ad25500e6d3f5f8a849f37279   commit vacio, "I-52: reclamo de RACKMIRROR, ..."
Claim-Id      = 232b55c5-3d0c-47c7-90ae-a07caab06431
Push reclamo  = git push -u origin feature/rackmirror-espejo-semantico  →  "* [new branch]" (sin force)
BOOTSTRAP_SHA = 7ee79756d2b93b4eded660cb73310ef3edde32ff   docs/ROADMAP.md (+1 fila) y
                docs/initiatives/I-52-rackmirror-espejo-semantico.md (nuevo); 2 archivos, solo docs
```

No se crearon `docs/automation/state/I-52.yml` ni `docs/automation/decisions/I-52.md`: el caso (d) no los exige y
no hay decisiones que registrar (precedente I-51).

## 4. Rama y worktree

```text
Rama      = feature/rackmirror-espejo-semantico   (upstream origin/feature/rackmirror-espejo-semantico)
Worktree  = C:\Users\alejandra-mendoza\.codex\worktrees\feature-rackmirror-espejo-semantico
Origen    = origin/main (--no-track al crear; upstream fijado por el push -u)
Estado    = limpio y sincronizado (0/0) tras el bootstrap
```

---

## 5. Fundacion I-51 reutilizable

### 5.1 Mapa

| Pieza de I-51 | Evidencia | Para RACKMIRROR | Condicion o bloqueo |
|---|---|---|---|
| Clasificacion de fuentes (no `BlockReference`, sin payload, ilegible/MAJOR futuro, `Kind` vacio o sin handler, `Design` vacio) | `src/RackCad.Application/Persistence/RackDuplicationPlan.cs:225-429` | **Reutilizable tal cual** | Los mensajes dicen «duplicar» (`:326`, `:434`, `:519`, `:532`, `:581`) |
| Clave logica discriminada `RackId` (sin mayusculas) / `Definition(handle)` (legacy) | `RackDuplicationPlan.cs:56-112`, `:439-442` | **Reutilizable tal cual** | — |
| Agrupacion por clave y **deduplicacion de definiciones** conservando todas las referencias | `RackDuplicationPlan.cs:498-507` | **Reutilizable tal cual** | — |
| Consistencia de grupo: `Kind` unico, nombre unico, autoridad authored | `RackDuplicationPlan.cs:510-582` | **Parcial** | La autoridad authored se compara **solo para Selectivo** (`:535-538`); los otros cinco kinds pueden llegar divergentes sin deteccion |
| Grupo publico (`Key`, `Kind`, `BaseName`, `Definitions`, `References`) | `RackDuplicationPlan.cs:137-149` | **Parcial** | No expone `View`, `Section` ni el sobre por definicion (quedan en `GroupBuilder.envelopes`, privado); el espejo los necesita para el mapeo vista→eje y el remapeo de seccion |
| Asignador de identidad con `Guid` inyectado y validado (`Guid.Empty`, repetido, igual a una fuente) | `RackDuplicationPlan.cs:355-367`, `:613-651` | **Parcial** | `CopyName` es privado y fija «&lt;base&gt; - copia[ k]» (`:654-657`); la regla «id ≠ fuente» solo vale para copia |
| Restamp con identidad del llamador `RestampEnvelope(payload, name, Guid)` y NI-1..NI-6 | `src/RackCad.Plugin/RackEnvelopeRestamp.cs:52-88` | **Reutilizable para copia**, despues de reflejar | Solo cambia `Id`, `Name` y la identidad interior via `IRackKindHandler.RestampDesign` (`:75-86`, `:97-111`); **no existe hook para transformar el diseño** |
| `RackCloner.CloneDefinition` | `src/RackCad.Plugin/RackCloner.cs:29-87` | **No directa** | Clona las **entidades** de la definicion (geometria sin reflejar) y escribe un payload nuevo; un espejo semantico necesita **regenerar** la geometria desde el diseño reflejado (seccion 15) |
| Fases ACQUIRE → SNAPSHOT → PREFLIGHT → base → PREPARE → MUTATE | `src/RackCad.Plugin/RackDuplicarCommands.cs:53-165`, `:173-313` | **Patron, no codigo** | Todo es `private` al archivo; las guardas G-R1..G-R6 y la de cableado leen **ese** archivo (seccion 5.3) |
| Desplazamiento UCS→WCS | `RackDuplicarCommands.cs:145` (unico `TransformBy` del Plugin) | **Patron** | El espejo necesita dos puntos de linea en UCS llevados a WCS, no una diferencia |
| Transformacion de la referencia nueva: `Position + d`, `Rotation`, `ScaleFactors`, `LayerId` del origen | `RackDuplicarCommands.cs:201-207`, `:303-308` | **No** | Es traslacion pura; el espejo exige posicion reflejada, rotacion `2φ−θ+π` y escala canonica (seccion 13) |
| `InDocumentTransaction` | `src/RackCad.Plugin/InDocumentTransaction.cs` | **Reutilizable** para un cuerpo de una fase | Una regeneracion con purga post-commit o `Regen` es otra politica (contrato I-51 INV-14 las prohibe en RACKDUPLICAR) |
| Nombre de rehearsal duplicado | `RackDuplicarCommands.cs:89` repite `" - copia"` | — | Deuda de I-51: la regla de nombre vive en dos sitios |

### 5.2 Que reutiliza directamente y que exige extraccion

**INFERENCE, a decidir en la Proposal:**

1. **Directo:** clasificacion, clave logica, agrupacion, deduplicacion, consistencia `Kind`/nombre, ensayo de restamp
   con `Guid`, `RackBlockData`, `RackEmbedStore`, `KindHandlerRegistry.TryGetIgnoreCase`.
2. **Exige extraccion o generalizacion:** la politica de nombre y el asignador (hoy «copia» por ordinal de destino);
   la exposicion de `View`/`Section` por definicion; una consistencia authored para los kinds no selectivos; la
   transformacion de la referencia; la **regeneracion** de definiciones desde un diseño transformado.
3. **No existe en I-51 y es nuevo:** la reflexion del diseño authored por kind, el mapeo vista→eje local, el remapeo
   de `Section` y la regla de colocacion reflejada.
4. **Riesgo de duplicar infraestructura:** copiar las fases privadas de `RackDuplicarCommands.cs` en un comando nuevo
   seria duplicar; extraerlas a un helper compartido obliga a **re-apuntar** G-R1, G-R2, G-R4, G-R6 y la guarda de
   cableado con RED demostrado (regla RC-7 de I-51). Es decision de Arquitecto (AM-7).

### 5.3 Pruebas y guardas de I-51 que condicionan

| Prueba / guarda | Evidencia | Por que importa |
|---|---|---|
| T1-T13 del planificador | `tests/RackCad.Tests/RackDuplicationPlanTests.cs:88-597` | Fijan «- copia» (`:106`, `:110`, `:143`, `:168`, `:552-555`): generalizar el nombre no puede cambiar RACKDUPLICAR |
| T14-T15 (restamp conserva bindings; autoridad tras restamp) | `tests/RackCad.Tests/SelectiveDuplicationFailClosedTests.cs:222`, `:255` | Modelo de transformacion pura sobre el documento authored |
| G-R1, G-R2, G-R4, G-R6 y cableado G5 | `SelectiveDuplicationFailClosedTests.cs:343`, `:420`, `:436`, `:453`, `:536` | Leen **solo** `RackDuplicarCommands.cs`; un archivo nuevo no queda observado y necesita guardas propias |
| G-R5 (dos sobrecargas, un `NewGuid(`, sin `catch (`) | `SelectiveDuplicationFailClosedTests.cs:482`, `:321` | Poner codigo de transformacion en `RackEnvelopeRestamp.cs` lo rompe |
| G-R3 (lookup sin mayusculas, sin ramas por kind) | `tests/RackCad.Tests/PushBackRoundTripSourceGuardTests.cs:318` | Un comando sin ramas por kind es el patron esperado |
| Censo de comandos = **33** | `tests/RackCad.Tests/SelectiveEditorOpenTests.cs:540-546` | Un comando nuevo (+ alias) obliga a re-apuntar el censo |
| Unicidad de `[CommandMethod]` | `PushBackRoundTripSourceGuardTests.cs:361` | Alias libre: ya se usan RA, RB, RCB, RCT, RD, RED, RK, RL, RLY, RPB, RR, RS, RSD, RVA |
| Texto de `IRackKindHandler.cs` y handlers | `SelectiveDuplicationFailClosedTests.cs:305`, `:311`; `SelectiveBomAuthorityTests.cs:377-399`; `CantileverPluginSourceGuardTests.cs:234-243` | Un miembro nuevo del handler toca 6 handlers y estas guardas |

---

## 6. Comportamiento actual de las transformaciones

### 6.1 Abstracciones neutrales (sin AutoCAD)

| Tipo | Evidencia | Refleja | Como |
|---|---|---|---|
| `Transform2D` (matriz 2×3; `Identity`, `Translation`, `Rotation`, `MirrorAboutY`, `MirrorAboutX`, `Scale` uniforme, `Then`, `Apply`, `Determinant`, `ReversesOrientation`, `RotationAngle`) | `src/RackCad.Application/Geometry/Transform2D.cs:20-116` | **Si** | Matriz explicita con determinante negativo; arcos y contornos invierten el sentido (`PathSegment2D.cs:139-160`, `ClosedContour2D.cs:132-136`). **Falta**: inversa, reflexion respecto de una linea arbitraria, garantia de rigidez y descomposicion en (posicion, rotacion, espejo) |
| `Placement2D` | `Geometry/Geometry2D.cs:33-47` | No | Solo traslacion |
| `LocalFrame3D` (`Create`, `Camera`, `FromAxes`, `ToWorld`, `ToLocal`) | `Geometry/Spatial3D.cs:117-235` | **No** | Solo diestro; `FromAxes` lanza si X×Y≠Z |
| `PrismaticSectionInstance` (`Frame`, `RotationRadians`, `Mirrored`) | `src/RackCad.Application/StructuralSections/Geometry/PrismaticSectionInstance.cs:21-131` | Si | Bandera `Mirrored` sobre marco diestro |
| `HeaderBlockInstance` (`Insertion`, `RotationRadians`, `MirroredX`, `MirroredY`) | `src/RackCad.Application/Drawing/HeaderBlockInstance.cs:57-101` | Si | Banderas; es lo mas parecido a los parametros de una `BlockReference`, pero sin metodos |
| `HeaderPlacement` (`InsertionX`, `InsertionY`, `Mirrored`, sin rotacion) | `Drawing/HeaderRunPlan.cs:85-101` | Si | Espejo en X respecto de la insercion |
| `PushBackMirror` | `src/RackCad.Application/Systems/PushBack/PushBackMirror.cs:10-306` | Si, **eje vertical** | Regla documentada (`:22-25`): reflejar `T(t)·R(θ)·S(±1,1)` da `T(Mt)·R(−θ)·S(∓1,1)`; texto y cota solo se trasladan (`:37-41`, `:50`, `:59-68`). Todos sus llamadores son Push Back |
| `SelectiveSafetyPlacement.AppendAtPost` (`mirrorAxisX`) | `Systems/Selective/SelectiveSafetyPlacement.cs:204-251` | Si | `2·c − x` y banderas `MirroredX`/`MirroredY` |
| Cantilever `MirrorAboutCentralPlane`, `Reflect(Point3D)`, `Reflect(Vector3D)` | `Systems/Cantilever/CantileverColumnBaseFrameResolver.cs:130-184` | Si, **plano normal a Y** | Mano en la bandera `Mirrored` de la seccion |
| Cantilever `CantileverStationBaseSideResolver.Mirror` | `CantileverStationBaseSideResolver.cs:218-331` | Si, local | Base negativa derivada por espejo (ADR-0026) |
| `RigidClone` (Dinamico y Push Back) | `DynamicFlowBedLateralBuilder.cs:86-123`, `PushBackFlowBedLateralBuilder.cs:164-200` | No | Rotacion alrededor de un pivote |
| `HeaderRunPlan.PlacedClone` | `Drawing/HeaderRunPlan.cs:53-80` | Si, pero **conserva `RotationRadians`** (`:67`) | Contradice la regla de `PushBackMirror`; seccion 24 |
| Dominio | `src/RackCad.Domain` | — | **No hay ningun tipo de transformacion ni de colocacion** |

### 6.2 Como llega el espejo al Plugin

- Pieza: `LateralHeaderDrawer.AppendInstance` fija `Rotation` y `ScaleFactors = Scale3d(MirroredX?-1:1, MirroredY?-1:1, 1)`
  (`src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:308-314`); cabecera anidada con `Scale3d(-1,1,1)` (`:60-63`, `:145-148`).
- Secciones estructurales y Cantilever: el espejo va **horneado en coordenadas** en Application; el Plugin dibuja
  polilineas (`CantileverViewMaterializer.cs:11-17`) y una guarda prohibe `ScaleFactors` en su materializador
  (`tests/RackCad.Tests/StructuralSectionPluginSourceGuardTests.cs:343`).
- Orden de composicion: espejo → rotacion → traslacion (`EditorPreviewSurface.cs:166-179`,
  `PushBackPreviewModel.cs:271-281`, `PrismaticSectionInstance.cs:87-95`).

### 6.3 Colocacion de la referencia de rack

- **FACT:** la insercion solo fija `Position` con el jig (`src/RackCad.Plugin/Drawing/BlockPlacement.cs:174-201`,
  `:240-244`); nunca asigna `Rotation`, `ScaleFactors`, `Normal` ni alinea al UCS.
- **FACT:** `RackInsertionRequest` no transporta posicion, rotacion ni escala (`src/RackCad.UI/Editor/RackInsertionRequest.cs:22-230`).
- **FACT:** los unicos lectores de `ScaleFactors` de una referencia **copian** el valor: RACKDUPLICAR
  (`RackDuplicarCommands.cs:205`, `:306`), RACKLAYOUT (`RackLayoutCommands.cs:199`, `:253`), RACKRELLENAR
  (`RackLayoutCommands.Fill.cs:457`). Nadie normaliza una escala negativa.
- **FACT:** RACKLAYOUT propaga a proposito un semilla rotado o espejado (`RackLayoutCommands.cs:170-171`) y declara que
  la hilera espalda-con-espalda **no** se voltea: «a true mirror (a 180° turn + bbox-shift accounting) is a future
  refinement» (`:246-249`).
- **FACT:** **no hay reactores, overrules ni suscripciones de eventos**: `PluginInitializer.Initialize/Terminate`
  vacios (`src/RackCad.Plugin/PluginInitializer.cs:5-13`); los `+=` del Plugin son concatenaciones.
- **FACT:** ningun tipo de Domain o Application representa la colocacion de un rack completo; vive solo en cada
  `BlockReference` de vista (`RackEmbedDocument` no tiene transformacion: `RackEmbedDocument.cs:35-64`).

### 6.4 ¿Puede Application/Domain representar una reflexion sin `Matrix3d`?

**Si en 2D, no en Domain.** `Transform2D` ya expresa reflexiones con determinante negativo. Lo que falta es un valor
neutral para la **colocacion de una referencia** (posicion, rotacion, signo de escala) y la regla de reflexion
respecto de una linea arbitraria. Para una linea con angulo `φ`, reflejar `T(p)·R(θ)·S(sx,sy)` da
`T(p')·R(2φ−θ+π)·S(−sx,sy)` (INFERENCE derivada; con `φ = 90°` coincide con la regla de `PushBackMirror`).
Domain no tiene ningun tipo geometrico de este nivel y no lo necesita si la regla vive en Application.

---

## 7. Semantica actual de la seleccion de vistas

### 7.1 Reconocimiento y agrupacion

- El payload vive en la **definicion** (`RackBlockData` escribe en el diccionario de extension del BTR, clave
  `RACKCAD_SELECTIVE` para todos los kinds: `src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:16-89`; ADR-0009).
- Sobre: `SchemaVersion`, `Kind`, `View`, `Section`, `Id`, `Name`, `Design`, `ExtensionData` (`RackEmbedDocument.cs:35-64`).
- `FindRackBlocks` devuelve **definiciones** cuyo `Id` coincide sin mayusculas, de todo el dibujo, colocadas o no
  (`src/RackCad.Plugin/RackCommandSupport.cs:109-134`).
- RACKLISTA y RACKBOMTOTAL agrupan por `Id` sin mayusculas; copias = **maximo** de referencias directas por vista
  (`RackInventarioCommands.cs:68-72`; `RackInventarioCommands.BomTotal.cs:109-113`).

### 7.2 Model Space y Paper Space

| Comando | Tratamiento | Evidencia |
|---|---|---|
| RACKEDITAR (seleccion) | cualquier referencia del espacio actual | `RackCommandSupport.cs:79-94` |
| RACKEDITAR (redibujo) | redefine la definicion: afecta referencias de cualquier espacio (INFERENCE) | `LateralHeaderDrawer.cs:92-171` |
| RACKEDITAR (Insertar) | siempre a Model Space | `BlockPlacement.cs:193-196` |
| RACKLISTA / RACKBOMTOTAL | cuentan referencias sin filtrar espacio | `RackInventarioCommands.cs:46`; `BomTotal.cs:52,109-113` |
| RACKDUPLICAR | solo Model Space, el resto con aviso | `RackDuplicarCommands.cs:190-195`; `RackDuplicationPlan.cs:272-276` |
| Variables de proyecto | nivel definicion, sin filtro de espacio | `ProjectVariableMutationExecutor.cs:147` |

Referencias anidadas en otros bloques o xrefs: sin tratamiento especial (`RackBlockFinder.cs:66` solo excluye
`IsFromExternalReference`).

### 7.3 `View` y `Section` por sistema

| Kind | frontal | lateral | planta | Evidencia |
|---|---|---|---|---|
| `selective` | `Section` = fondo `k` (−1 legado = 0) | `Section` = poste `p` del grid maestro | −1 | `RackSelectivoCommands.cs:166-217` |
| `dynamic` | `Section` 0 = salida, 1 = entrada (otro valor → salida) | `Section` = poste `p` | −1 | `RackDinamicoCommands.cs:211-262`, `:393` |
| `pushback` | `Section` = extremo + (lado B ? 2 : 0): 0 A entrada/salida, 1 A posterior, 2 B entrada/salida, 3 B posterior | `Section` = poste `p` ≥ 0 | −1 exacto | `PushBackSystemFrontalBuilder.cs:134-155`; `RackPushBackCommands.cs:245-293` |
| `cantilever` | −1 | `Section` = estacion `k` | −1 | `CantileverViewPlanBuilder.cs:237-239`; `RackCantileverCommands.cs:530-540` |
| `cabecera` | — | lateral (defecto), −1 | −1 | `RackCabeceraCommands.cs:181-198` |
| `cama` | `View` null (se dibuja como lateral), −1 | | | `RackCamaCommands.cs:197-198` |

`View` null significa «frontal» segun el comentario (`RackEmbedDocument.cs:40`), pero dinamico, Push Back y cabecera
escriben lateral y RACKLISTA lo muestra como lateral (`RackListBuilder.cs:117-118`): inconsistencia (seccion 24).

### 7.4 RACKEDITAR y la autoridad de la vista

- **FACT:** la autoridad es **la vista clicada**. `PickRackBlock` lee solo `reference.BlockTableRecord` y su payload;
  la transformacion de la referencia se descarta (`RackCommandSupport.cs:74-99`).
- **FACT:** el editor carga el diseño de esa vista y, al Actualizar, busca **todas** las hermanas por GUID y escribe
  **el mismo** `designJson` en cada frontal, lateral y planta, redibujando por `Section`; no compara hermanas
  (`RackSelectivoCommands.cs:114-223`). Lo mismo por kind: `RackDinamicoCommands.cs:178-266`,
  `RackPushBackCommands.cs:184-303`, `RackCantileverCommands.cs:234-336`, `RackCabeceraCommands.cs:238-287`; la cama
  solo redibuja la definicion clicada (`RackCamaCommands.cs:226-228`).
- **FACT:** vistas cuyo fondo/corte ya no existe se **borran** si sobrevive alguna (`RackSelectivoCommands.cs:225-239`).
- **FACT:** Actualizar redefine la definicion en sitio y marca `RecordGraphicsModified` en sus referencias; **no toca**
  `Position`, `Rotation` ni `ScaleFactors` de ninguna referencia (`LateralHeaderDrawer.cs:88-182`; ADR-0010: «las copias
  de una definicion se actualizan sin recolocarse»).
- **FACT:** Insertar crea una definicion nueva con el mismo GUID y la coloca con el jig, sin rotacion ni escala
  (`BlockPlacement.cs:174-201`).

### 7.5 Que significa fisicamente seleccionar solo algunas vistas

**INFERENCE (a decidir):** una vista es una **proyeccion** del rack, no una parte del rack. Reflejar un rack es una
sola operacion fisica sobre el conjunto. Por eso:

- En una **copia** (identidad nueva), seleccionar solo algunas vistas produce un rack reflejado **materializado solo en
  esas vistas**; las demas pueden insertarse despues con RACKEDITAR → Insertar y saldrian del diseño reflejado. Es el
  analogo de PD-1 de I-51 y **no** exige buscar hermanas.
- **En sitio conservando el `RackId`**, reflejar solo algunas vistas deja hermanas con diseños authored distintos. La
  siguiente RACKEDITAR desde una hermana sin reflejar **reescribe** las reflejadas con el diseño original (7.4); desde
  una reflejada, reescribe las no reflejadas con el diseño reflejado en posiciones no reflejadas. En Selectivo,
  RACKBOMTOTAL salta el rack por divergencia (`BomAuthoredAuthority.cs:112-124`) y las operaciones de variables abortan
  (`ProjectVariableConsumerDiscovery.cs:144-149`); en los otros cinco kinds **nada lo detecta**
  (`BomAuthoredAuthority.cs:104-110`).
- La instruccion de apertura prohibe **buscar hermanas automaticamente**. Verificar la cobertura completa de un
  `RackId` (sin expandir la seleccion) seria una lectura del dibujo, no una expansion: si se admite es decision de
  producto (PDC-3).

---

## 8. Inventario de orientacion por sistema

### 8.1 Sistemas registrados

| Enum `RackSystemKind` | Sobre `Kind` | Handler | Editor | Vistas |
|---|---|---|---|---|
| `Selective` (0) = **cabecera independiente** | `cabecera` | `CabeceraKindHandler` | `RackFrameConfiguratorWindow` | lateral, planta |
| `PalletFlow` (1) | `dynamic` | `DynamicKindHandler` | `RackDynamicSystemWindow` | lateral, frontal salida/entrada, planta |
| `SelectiveRack` (2) | `selective` | `SelectiveKindHandler` | `RackSelectiveWindow` | frontal por fondo, lateral por poste, planta |
| `Cama` (3) | `cama` | `CamaKindHandler` | `RackFlowBedWindow` | una (lateral) |
| `Larguero` (4) | — | **ninguno**, a proposito | `RackLargueroWindow` (no inserta) | sin bloque |
| `PushBack` (5) | `pushback` | `PushBackKindHandler` | `RackPushBackSystemWindow` | lateral, frontal 0..3, planta |
| `Cantilever` (6) | `cantilever` | `CantileverKindHandler` | `RackCantileverWindow` | frontal, lateral por estacion, planta |

Fuentes: `src/RackCad.Domain/Systems/Shared/RackSystemKind.cs:4-31`, `SystemRegistry.Default.cs:23-39`,
`KindHandlerRegistry.cs:55-63`, `EditorModuleRegistry.cs:111-120`, `RackEmbedDocument.cs:16-26`.

**Drive-In: NO EXISTE.** Unica aparicion en codigo: un boton deshabilitado «Diseñar drive-in / Próximamente.»
(`src/RackCad.UI/RackMainMenuWindow.xaml:139-144`); ningun enum, handler, registro, catalogo ni prueba. Las demas
apariciones estan en documentos historicos y de auditoria.

### 8.2 Marcos locales y eje horizontal de cada vista

| Sistema | Marco local (FACT) | frontal: X de la vista | lateral: X de la vista | planta: X, Y |
|---|---|---|---|---|
| Selectivo | Run: poste 0 en X=0 creciente (`SelectivePostGeometry.cs:32-40`); fondo 0 = frente (`SelectiveDepthLayout.cs:205-215`); fondos cortos = prefijo alineado al poste 0 (`SelectiveGeometryResolver.cs:150-173`) | RUN | DEPTH (frente en 0) | X = DEPTH, Y = RUN (`SelectivePlantaBuilder.cs:15-22`) |
| Dinamico | Salida en X=0 (baja, sin espejo), entrada en `TotalLength` (alta, espejada) (`DynamicLoadBeamGeometry.cs:166-190`) | frentes, misma orientacion en ambos cortes | DEPTH (salida en 0) | X = DEPTH, Y = frentes |
| Push Back simple | Extremo bajo (pasillo) en X=0; la fisica asume flujo hacia +X (`PushBackElevations.cs:166-177`) | frentes | DEPTH (pasillo en 0) | X = DEPTH, Y = frentes |
| Push Back compuesto | Marco de A; B = A reflejado respecto de `TotalLength/2` (`PushBackCompositeSystem.cs:62-69`); retícula de frentes **compartida y no reflejada** | frentes, iguales para A y B | A en 0 … hueco … B en `Total` | idem |
| Cama | X a lo largo del riel; tope junto al origen (`FlowBedLateralBuilder.cs:34-66`) | — | riel | — |
| Cantilever | X = linea (`StationOriginX(i) = i·s`), +Y = hacia la base, Z arriba (`CantileverColumnBaseDatum.cs:9-13`) | (−X, +Z) | (−Y, +Z) | (−X, +Y) (`CantileverViewPlanBuilder.cs:205-235`; `Spatial3D.cs:179-198`) |
| Cabecera | Poste izquierdo en X=0 sin espejo, derecho en `Depth` con `MirroredX` (`LateralHeaderLayoutBuilder.cs:49-59`) | — | DEPTH | X = fondo, Y = cara del poste (`PlantaHeaderLayoutBuilder.cs:13-21`) |

### 8.3 Consecuencia central: la misma linea no significa lo mismo en cada vista

**INFERENCE, verificada contra los marcos:**

- Una linea de espejo **vertical en la vista** refleja el eje que esa vista dibuja en X: en la **frontal** es
  RUN/RT/R_X; en la **lateral** es DEPTH/RD/R_Y; en la **planta** depende del angulo de la linea respecto del
  marco de la planta.
- Aplicar la misma linea a la frontal, la lateral y la planta de **un mismo rack** implica reflexiones fisicas
  **distintas** por vista. Como el diseño authored es **uno por `RackId`** (ADR-0034 §9), un grupo con vistas que
  implican ejes distintos no tiene una reflexion unica.
- En 3D, RUN seguido de un giro de 180° es DEPTH. Eso vale para la **planta** (una referencia de planta puede girarse
  180°) pero **no** para las elevaciones: una frontal girada 180° en su plano queda de cabeza, no vista desde el otro
  pasillo.
- Una reflexion fisica RUN **no** voltea las laterales (solo las re-indexa `p→M−p`); una DEPTH **no** voltea las
  frontales (las re-indexa `k→N−1−k`). Un espejo grafico uniforme de todas las vistas es, por tanto, **incorrecto**
  para al menos una de ellas.

### 8.4 Mano de las vistas (unknown)

- **Selectivo:** con la frontal leida desde el pasillo del fondo 0 (mirando hacia +DEPTH, RUN a la derecha), la planta
  `X = DEPTH, Y = RUN` tiene `X×Y = −Z`: seria una vista superior **espejada**. Si la frontal se lee desde detras del
  ultimo fondo, la planta es propia. **Ningun codigo ni documento fija el punto de vista de la frontal**
  (`SelectivePalletDesign.cs:68` solo dice «left to right»).
- **Cantilever:** `Camera` deriva `right = up × forward` (`Spatial3D.cs:186-197`); con la direccion de vista del
  comentario («Looking towards +Y», `CantileverViewPlanBuilder.cs:210`) la imagen pone −X a la derecha, lo contrario
  de un observador real. Si `viewDirection` apunta **hacia** el observador, la imagen es propia.
- No cambia **que** eje refleja una linea (8.3), pero si **que lado** ve el usuario como izquierda o derecha y donde
  espera la copia. Validacion visual del Owner obligatoria (seccion 22).

---

## 9. Matriz inicial de propiedades sensibles al espejo

Columnas: **Contrato actual** = evidencia; **Mirror candidato** = INFERENCE o NO DECIDIDO; **Tipo** = invariante /
swap / indice (reversal o remapeo) / transform / N/A. **NR** = no representable hoy en el modelo authored.

### 9.1 Selectivo

| # | Propiedad / estado | Contrato actual | Mirror candidato | Tipo |
|---|---|---|---|---|
| S-01 | Orden de frentes `Bays[]`, `ExtraFondoBays[][]` | Run desde poste 0; fondo corto = prefijo en poste 0 (`SelectiveGeometryResolver.cs:150-173`; `SelectivePostGeometry.cs:32-40`) | RUN: `f→M−1−f`; DEPTH: invariante por fondo | indice; **NR** en RUN si los fondos tienen distinto numero de frentes (esquina) |
| S-02 | Medio frente `Segments[]` | Tramos desde el poste **izquierdo**; el ultimo se **calcula** (`SelectiveMedioFrente.cs:6-18`, `:70`) | RUN: invertir y volver explicito el remanente (necesita catalogo y `BeamLength`) | transform; **NR exacto** si `PostPeraltes[f]≠PostPeraltes[f+1]` |
| S-03 | Fondos: `PalletDepth`, `ExtraFondoDepths`, `CabeceraFondoOverrides`, `SeparatorLengths` | Offsets del frente hacia atras; herencia hacia el fondo 0 y relleno direccional de separadores (`SelectiveDepthLayout.cs:118-215`) | DEPTH: materializar herencia y luego invertir; RUN: invariante | indice |
| S-04 | Grid maestro (derivado) | Mas frentes; empate → indice menor (`SelectiveDepthLayout.cs:34-55`) | DEPTH puede cambiar el fondo que gobierna los claros | riesgo (NR potencial) |
| S-05 | Celdas (`FondoIndex`, `FrontIndex`, `LevelIndex`): pallet, larguero, overrides | `SelectiveCellAddress.cs:21-42` | valores invariantes; `f` o `k` remapeado segun eje; nivel invariante | indice |
| S-06 | `PostPeraltes[p]` | Por poste del run, no por fondo (`SelectivePalletDesign.cs:107`) | RUN: `p→M−p`; DEPTH: invariante | indice |
| S-07 | `PostCabeceras[p]`, `ExtraFondoPostCabeceras[k-1][p]` | Clave (`FondoIndex`, `PostIndex`) (`SelectiveCabeceraAuthority.cs:120-135`) | RUN: invertir cada fila; DEPTH: mover fila `k→N−1−k` y transformar contenido (S-08, S-09) | indice + transform |
| S-08 | Cabecera custom `LeftPost`/`RightPost`, `LeftBasePlate`/`RightBasePlate` | Left = poste delantero (X=0), Right = trasero (`LateralHeaderLayoutBuilder.cs:49-59`) | DEPTH: intercambiar; RUN: invariante | swap |
| S-09 | `Panels[].DiagonalDirection` | UpRight/UpLeft explicitos; `AutoAlternating` por paridad de `Number` (`BracingPanel.cs:40-48`); las cabeceras estandar **siempre** son Auto (`RackFrameConfigurationFactory.cs:238`) | DEPTH: UpRight↔UpLeft; Auto no puede voltearse | swap; **NR** (Auto) — NO DECIDIDO si las diagonales estandar deben espejarse |
| S-10 | `MountingFace` (Front/Back/Both) | Solo conteo de BOM y offset de preview (`BracingPanelMemberBuilder.cs:159-165`) | NO DECIDIDO (sin efecto visible en vistas del Selectivo) | swap? |
| S-11 | BOTA `Side`, `PostSides`, `Bota.Placement`, `Bota.Posts` | Left = EntryExit (cara cercana), Right = Rear (lejana) (`BootPlacement.cs:41-65`); precedencia `Bota.Posts` > `PostSides` > `Bota.Placement` > `Side` (`SelectivePalletDesign.cs:244-299`) | DEPTH: intercambiar valores en ambos juegos; RUN: `p→M−p` | swap + indice |
| S-12 | PROTECTOR LATERAL `PostSides` | Orientacion de la guia en el run; defecto poste 0 Left, poste M Right (`SelectiveSafetyPlacement.cs:221-248`) | RUN: `p→M−p` **y** Left↔Right; DEPTH: invariante (confirmar) | swap + indice — NO DECIDIDO |
| S-13 | TOPE `Side` + `TopeFondo` + `TopeShared` + `TopeOffCells` | Lado = extremo BAJO/ALTO del tramo de referencia (I-46; `SelectiveTopePlan.cs:89-160`); `TopeOffCells` compartidas por fondos | DEPTH: Left↔Right y `TopeFondo c→N−2−c`; RUN: `f→M−1−f` en off-cells | swap + indice; **NR**: compartido con hueco, Right en ultimo fondo con N>1, auto central con N impar |
| S-14 | DESVIADOR `Side` + `DesviadorOffCells` | Left = cara del pasillo delantero, Right = trasera espejada (`SelectiveDesviadorPlan.cs:9-15`); off-cells por **columna de poste**, incluidos postes intermedios | DEPTH: swap; RUN: invertir columnas (depende de geometria resuelta) | swap + indice derivado |
| S-15 | PARRILLA `ParrillaOffCells` | Por frente y nivel, compartidas | RUN: `f→M−1−f`; DEPTH: invariante | indice |
| S-16 | Numeracion de frentes/niveles y nombre de bloque «frente F{k+1}», «lateral {p+1}» | Etiquetas = indice + 1 (`SelectiveFrontalBuilder.cs:320-340`; `RackSelectivoCommands.cs:181`, `:206`) | Siguen al indice: el «frente 1» pasa a ser el otro extremo | NO DECIDIDO (producto) |
| S-17 | Cotas y anotaciones (`Dimensions`, `DimensionStyle`, `AnnotationScale`) | Ancladas al origen local; se regeneran (`SelectiveDimensions.cs:67-254`) | valores invariantes; posicion regenerada legible | invariante |
| S-18 | `PropertyValues` (`selective.verticalClearance`, `selective.palletTolerance`) | Escalares del rack, no espaciales (`SelectiveLinkedProperties.cs:188-213`) | copiar tal cual | invariante |
| S-19 | Sobre `Section` | frontal = fondo `k`; lateral = poste `p` | DEPTH: frontal `k→N−1−k`; RUN: lateral `p→M−p` | indice |

### 9.2 Dinamico

`N` = frentes. RD **no se puede almacenar**: la salida esta fijada en X=0, asi que RD ≡ RT + giro de 180° en planta.

| # | Propiedad / estado | Contrato actual | Mirror candidato | Tipo |
|---|---|---|---|---|
| D-01 | Salida/entrada y pendiente | Salida X=0 sin espejo; entrada `TotalLength` espejada; entrada = salida + pendiente (`DynamicLoadBeamGeometry.cs:166-190`; `DynamicRackSystemResolver.cs:102-110`) | Marco fijo: RD solo como transformacion de la referencia | N/A (marco) |
| D-02 | Orden `Fronts[]` y sus niveles | Retícula de postes (`DynamicFrontGeometry.cs:129-162`) | RT: invertir lista | indice |
| D-03 | Orden `Modules[]` (profundidad) | Posicion 1 en X=0 (`DynamicRackSystem.cs:141-153`) | Invariante en el marco local | invariante (patron cabecera/separador con paridad: validar) |
| D-04 | `HeaderLineOverrides` (`PostIndex`, `ModuleId`) | `DynamicFrontGeometry.cs:374-395` | RT: `p→N−p` | indice |
| D-05 | `DerivedPostLineOverrides` (`PostIndex`) | `DynamicFrontGeometry.cs:402-424` | RT: `p→N−p` | indice |
| D-06 | BOTA | Left = cara cercana (salida), Right = lejana (entrada) | RT: `p→N−p` | indice |
| D-07 | PROTECTOR LATERAL `PostSides` | **Dos lecturas**: orientacion en planta (`DynamicSafetyMultiViewBuilder.cs:132-140`), extremo en lateral/frontal (`DynamicSafetyDefaults.cs:108-176`) | RT: `p→N−p`; Left↔Right ambiguo | NO DECIDIDO |
| D-08 | DESVIADOR `Side` + `DesviadorOffCells` | Left = cara de salida, Right = entrada espejada; clave `Math.Min(post, N−1)` (`SelectiveDesviadorPlan.cs:28-31`) | RT: invertir; **no biyectivo** (N−1 cubre N−1 y N) | indice; **NR** |
| D-09 | DEFENSA `DefensaPosts{PostIndex, ExitLength, EntranceLength, ExitAuto, EntranceAuto}` | `DynamicForkliftDefensePlan.cs:50-79` | RT: `p→N−p` | indice |
| D-10 | GUIA DE ENTRADA `GuiaEntradaOffCells` | Par f / f+1 espejado solo en entrada (`DynamicEntranceGuidePlan.cs:27-64`) | RT: `f→N−1−f` | indice |
| D-11 | Sobre `Section` frontal (0 salida / 1 entrada) | `RackDinamicoCommands.cs:393` | RT: invariante | invariante |
| D-12 | Sobre `Section` lateral | poste `p` | RT: `p→N−p` | indice |
| D-13 | Cabecera por modulo (`RackFrameConfiguration`) | marco local de cabecera; alternancia dibujada por paridad ordinal (`DynamicSystemLateralBuilder.cs:151-155`) | invariante en el marco local | invariante (validacion visual) |
| D-14 | Numeracion de frentes | desde el indice (`DynamicViewDecorations.cs:58-69`) | sigue al indice | NO DECIDIDO |

### 9.3 Push Back de un sentido

Hereda D-02..D-14 sobre `PushBackDesign.Structure` (`PushBackDesignDocument.cs:27`). RD no se puede almacenar (el
lado A siempre existe).

| # | Propiedad / estado | Contrato actual | Mirror candidato | Tipo |
|---|---|---|---|---|
| P-01 | Extremo bajo = pasillo en X=0 | `PushBackPlacements.cs:43-61`; ADR-0031 §9 | marco fijo | N/A |
| P-02 | Configuracion por celda (`DefaultPalletsDeep`, `PalletsDeepOverrides`, `DrawPallets`) | `PushBackCellDepth.cs:41-173` (ADR-0030) | RT: reordenar por frente | indice |
| P-03 | Tope posterior `RearTopeSaque`, `RearTopePieceId`, `RearTopeOffCells` | Extremo alto de la cama (`PushBackRearTopeBuilder.cs:268-307`) | RT: invertir `Frente` | indice |
| P-04 | `Side` colapsado + `AuthoredSide` | `PushBackSafetyAuthority.cs:239-256` | RT: invertir `PostSides` | indice |
| P-05 | DESVIADOR por poste (biyectivo) | `PushBackSafetyAuthority.cs:271` | RT: `p→N−p` | indice |
| P-06 | Sobre `Section` frontal 0/1 | `PushBackSystemFrontalBuilder.cs:138-139` | RT: invariante | invariante |

### 9.4 Push Back compuesto A/B

RD **si** se puede almacenar: es el intercambio de las configuraciones A ↔ B mas el reetiquetado de modulos.

| # | Propiedad / estado | Contrato actual | Mirror candidato | Tipo |
|---|---|---|---|---|
| PC-01 | Lado A (campos legacy) y lado B (`SideB`) | ADR-0031 §5-bis y §8; `PushBackDesign.cs:24-56` | RD: intercambiar; RT: invertir ranuras en ambos | swap / indice |
| PC-02 | `Structure.Modules`: A → `GAP` → B invertidos con prefijo `B:` | `PushBackCompositeStructure.cs:495-531` | RD: invertir lista e intercambiar prefijos; `GAP` queda | transform |
| PC-03 | `Topologies[]` (`SoloA`/`SoloB`/`Encontradas`/`Corrida`; `AToB`/`BToA`) y defaults | `PushBackSide.cs:52-59`; `PushBackRuns.cs:176-241` | RD: `SoloA↔SoloB`, `AToB↔BToA`; RT: invertir `Frente`; conservar entradas dormantes | swap + indice |
| PC-04 | `StructureOverrideA/B` | `PushBackSideConfiguration.cs:182-216` | RD: intercambiar | swap |
| PC-05 | `AbsentSlotsA/B` y `SideB.Fronts[i] = null` legacy | `PushBackCompositeStructure.cs:315-323` | RT: `i→N−1−i` (el dibujo no queda identico: lineas de borde); RD: intercambiar; los nulos legacy de B no tienen declaracion para pasar a A | indice / swap; **NR** (legacy) |
| PC-06 | BOTA por lado (`Bota*` / `BotaB*`, `BotaSidesDeclared`) | `SelectivePalletDesignDocument.cs:541-567`; `PushBackBootPlan.cs:114-159` | RD: intercambiar; intencion global legacy ligada a A | swap; riesgo |
| PC-07 | DEFENSA `ExitLength`(pasillo A) / `EntranceLength`(pasillo B) + `DefensePieceId` A/B | `PushBackDefenseSides.cs:35-95` | RD: intercambiar longitudes, auto y piezas | swap |
| PC-08 | Tope posterior por lado (`RearTope*` / `SideB.RearTope*`) | `PushBackSideDesign.cs:48` | RD: intercambiar | swap |
| PC-09 | `HeaderLineOverrides` con `ModuleId` `M…`/`GAP`/`B:…` | `PushBackEditorDesignAssembler.cs:348-364` | RD: intercambiar prefijos; RT: `p→N−p` | indice |
| PC-10 | Sobre `Section` frontal 0..3 | `PushBackSystemFrontalBuilder.cs:134-155` | RD: `0↔2`, `1↔3` | indice |
| PC-11 | Etiquetas «A»/«B» (A siempre en X=0) | `PushBackSideAnnotations.cs:31`, `:104` | la letra sigue a la posicion | NO DECIDIDO |
| PC-12 | `Composite.Gap`, `CentralSeparator` | `PushBackCompositeStructure.cs:123-133` | invariante | invariante |

### 9.5 Cama de rodamiento

| # | Propiedad / estado | Contrato actual | Mirror candidato | Tipo |
|---|---|---|---|---|
| F-01 | Lado del tope | **Ningun campo persistido** lo expresa; tope junto al origen (`FlowBedLateralBuilder.cs:34-66`); la cama no emite textos ni cotas | solo la transformacion de la referencia puede expresarlo | N/A en diseño |
| F-02 | `BedType`, `LaneDepth`, `PalletDepth`, `RollerId`, `RollerPitchOverride` | `FlowBedConfiguration.cs:10-22` | invariante | invariante |

### 9.6 Cantilever

`N` = estaciones.

| # | Propiedad / estado | Contrato actual | Mirror candidato | Tipo |
|---|---|---|---|---|
| K-01 | `StationTopology.FaceMode` Single/Double | `CantileverStationDesign.cs:14-21` | invariante | invariante |
| K-02 | `StationTopology.SingleSide` ±Y | eje del brazo `(0, ±cos, +sin)` (`CantileverArmFrameResolver.cs:59-75`) | R_Y: intercambiar (Single); Double: no se lee | swap |
| K-03 | `ArmCellOverrides[].Side` | `CantileverLineDesign.cs:259` | R_Y: intercambiar todas, incluso lado inactivo | swap |
| K-04 | `ArmCellOverrides[].StationIndex` | `CantileverLineDesign.cs:255`, `:430-437` (primer match gana) | R_X: `i→N−1−i`, conservar orden de lista | indice |
| K-05 | Orden de estaciones, `ColumnCentreSpacing` uniforme | `CantileverLineDesign.cs:370-380` | R_X exacto | indice |
| K-06 | Lado de traslape del arriostramiento (derivado) | `outward = −1` solo para Single +Y; **+1 para Single −Y y para Double** (`CantileverLineResolver.cs:271-277`) | Single: sigue a K-02; **Double: −Y no expresable** | **NR** |
| K-07 | Diagonales A/B y manos de adaptador (derivados) | `CantileverIntervalResolver.cs:227-235`; `CantileverBraceAdapterFrameResolver.cs:212-260` | R_X: A↔B, manos L↔R (derivado) | swap derivado |
| K-08 | Seccion de brazo asimetrica (C, L) en Single | el lado −Y es la +Y girada 180° con la misma bandera (`CantileverArmResolver.cs:242-243`) | la mano no es expresable | **NR** |
| K-09 | Id de placa en BOM | patron de agujeros medido desde `Outline[0]` (`CantileverStationBomBuilder.cs:292-314`) | un contorno reflejado podria cambiar el id | riesgo |
| K-10 | Sobre `Section` lateral = estacion | `CantileverViewPlanBuilder.cs:238-239` | R_X: `k→N−1−k` | indice |
| K-11 | Separador `Mirrored = true` siempre | `CantileverLineFrameResolver.cs:61-85` | con `outward = +1` podria solaparse con su pestaña: posible defecto existente que R_Y expondria | riesgo (seccion 24) |

### 9.7 Cabecera independiente

| # | Propiedad / estado | Contrato actual | Mirror candidato | Tipo |
|---|---|---|---|---|
| H-01 | `LeftPost`/`RightPost`, `LeftBasePlate`/`RightBasePlate` | `RackFrameConfiguration.cs:57-60`; `LateralHeaderLayoutBuilder.cs:42-81` | R_D: intercambiar | swap |
| H-02 | `Panels[].DiagonalDirection` | `BracingPanel.cs:40-48` | R_D: UpRight↔UpLeft; Auto no | swap; **NR** (Auto) |
| H-03 | `Horizontals[]`/`Panels[].MountingFace` | `BracingPanelMemberBuilder.cs:159-165`, `:303-322` | R_F: Front↔Back | swap |
| H-04 | Rasgos leidos solo del poste izquierdo (rejilla de troqueles, `PostId`, placa, celosia en planta) | `LateralHeaderLayoutBuilder.cs:61-67`; `LateralHeaderParametersFactory.cs:31-32`; `PlantaHeaderLayoutBuilder.cs:71-93` | con postes distintos, el intercambio no es espejo exacto | riesgo |
| H-05 | Plantillas (`StandardBaselineId`) | sin lado ni direccion (`RackFrameTemplate.cs:21-37`) | una cabecera espejada deja de ser expresable como plantilla | NO DECIDIDO |

### 9.8 Transversal

| # | Propiedad / estado | Contrato actual | Mirror candidato | Tipo |
|---|---|---|---|---|
| X-01 | Sobre `Id`, `Name` (exterior e interior) | ADR-0009; `RackEnvelopeRestamp.cs:52-88` | copia: nuevos; en sitio: NO DECIDIDO | — |
| X-02 | `DimensionViews` (I-50, **en rama**) | politica por **tipo** de vista (`DimensionViewVisibility`: Frontal, Lateral, Planta) | invariante | invariante |
| X-03 | `SchemaVersion`, `ExtensionData` raiz | preservados (`RackEmbedDocument.cs:63-64`; `SelectivePalletDesignDocument.cs:141-142`) | preservar; campos desconocidos **anidados** se pierden en el round-trip del DTO (limitacion I-11) | invariante con riesgo |
| X-04 | Transformacion de la `BlockReference` (`Position`, `Rotation`, `ScaleFactors`, `Normal`, capa) | no persistida en el diseño; RACKEDITAR no la lee (`RackCommandSupport.cs:89-94`) | calculada por la regla de la seccion 13 | transform |
| — | **Drive-In** | no existe (8.1) | — | N/A |
| — | **Larguero** | sin sobre ni handler | — | N/A |

---

## 10. Opciones copia / en sitio

**No existe contrato historico de RACKMIRROR.** `RACKMIRROR`, «espejo de rack» o equivalentes solo aparecen como
exclusion de I-51 (`docs/initiatives/I-51-rackduplicar-multiples-origenes.md:335`, `:355`; `docs/HANDOFF.md:1558`).
Las unicas decisiones previas relacionadas son internas: ADR-0031 §8 (lado B = imagen especular fisica; texto y cota
solo se trasladan), ADR-0026 (base negativa derivada por espejo) y el comentario de RACKLAYOUT que difiere el espejo
de hileras (`RackLayoutCommands.cs:246-249`). **No hay autoridad previa inequivoca: no se elige.**

| Impacto | **A. Siempre copia reflejada; el original queda** | **B. Prompt estilo MIRROR: «¿Borrar objetos originales? [Sí/No]»** | **C. Espejo como edicion del diseño (justificada por ADR-0010)** |
|---|---|---|---|
| Descripcion | Seleccion → dos puntos de linea → racks nuevos reflejados | «No» = A. «Sí»: **B1** identidad nueva + borrar las referencias seleccionadas; **B2** conservar `RackId` y reflejar en sitio | Accion «Reflejar frentes / fondos» sobre un rack abierto (o variante sin linea): refleja el diseño y Actualizar redibuja **todas** sus vistas en sitio |
| `RackId` | Nuevo por grupo logico (AM-1 de I-51) | B1: nuevo; B2: **conservado** | Conservado |
| Nombre | Regla nueva (PDC-2); I-51 usa «- copia» | B1: regla nueva o el mismo nombre; B2: el mismo | El mismo |
| Referencias | Solo las seleccionadas, reflejadas, sobre definiciones regeneradas; enlaces dentro de la copia (analogo PD-2) | B1: vistas no seleccionadas del origen quedan como rack parcial (riesgo de **doble conteo** en BOM); B2: exige cubrir **todas** las definiciones del `RackId` y **todas** sus referencias, si no hay divergencia (7.5) o geometria reflejada en referencias no movidas | No se mueven; el dibujo cambia anclado al origen local de cada vista |
| Transaccion | PREFLIGHT completo (clasificar, reflejar, remapear, restamp) antes de pedir la linea; una transaccion de escritura | B1: crear + borrar en una transaccion; B2: redefinir + mover en una transaccion, con verificacion previa de cobertura | Flujo existente por kind de `EditX` (varias transacciones de redibujo + un `Regen`) |
| UX | Familiar a MIRROR sin la pregunta | Identica a MIRROR de AutoCAD | Sin linea; no «refleja donde el usuario indica» |
| Compatibilidad con I-51 | **Maxima**: planificador, clave, asignador con `Guid`, restamp | «No» igual que A; «Sí» se aparta: la regla «id ≠ fuente» no vale en B2 y aparece borrado/purga | Ninguna reutilizacion del planificador; toca los editores calientes (WORKFLOW §7) |
| Choque con la apertura | ninguno | B2 necesita **leer** hermanas para verificar cobertura (no expandir) | **Contradice** «no buscar hermanas»: RACKEDITAR las busca por GUID (ADR-0010) |

## 11. Estrategia candidata de identidad

**INFERENCE — depende de PDC-1:**

1. **Copia (A, B-«No», B1):** un `NewRackId` por grupo logico, compartido por todas sus vistas; mismo `Name` logico
   en sobre e identidad interior; nombres de BTR uniquificados. Orden obligatorio: **reflejar el diseño authored →
   restamp de identidad (`RestampEnvelope(payload, name, Guid)`, NI-1..NI-6) → escribir**. Nunca restamp sobre el
   payload sin reflejar ni reflejar despues del restamp con otra ruta de serializacion.
2. **En sitio con `RackId` conservado (B2, C):** ADR-0009 permite conservar identidad porque no nace un rack logico
   independiente, pero **solo** si todas las representaciones del `RackId` se reflejan juntas (ADR-0034 §9: una
   autoridad authored por `RackId`).
3. **Legacy sin `RackId`:** clave `Definition(handle)` solo dentro del lote (PD-6 de I-51). En copia recibe `RackId`
   real (precedente I-51). En sitio: NO DECIDIDO si sigue legacy.
4. **Identidad interior por kind:** Selectivo `Id`/`Name`; Cantilever `Id` (GUID) y `Name`; cabecera solo
   `Header.Name`; dinamico, Push Back y cama sin identidad interior (`RackEnvelopeRestamp.cs:97-111` y handlers).

## 12. Estrategia `OldRackId → NewRackId`

**INFERENCE, alineada con AM-3 de I-51:**

- **Sin estructura persistida.** El espejo tiene un solo «destino» (la linea): el asignador se invoca **una vez** y
  produce `{LogicalSourceKey → NewRackId, Name}` por grupo.
- Correspondencias locales a la transaccion: definicion origen → definicion reflejada (con `Section` remapeada) y
  referencia origen → referencia reflejada.
- **Nada reescribe referencias entre racks**: no existen RackIds dentro del contenido authored (S3 de I-51; ID21 fuera).
- Los `ModuleId` con prefijo `B:` y las claves (`PostIndex`, `ModuleId`) de Push Back **no son identidades de rack**:
  se remapean como indices dentro del diseño (PC-02, PC-09).
- En sitio con identidad conservada no hay mapa: `OldRackId = NewRackId`.

## 13. Modelo candidato de transformacion comun

**Candidato, NO DECIDIDO (AM-1 a AM-5).** Tres capas:

### 13.1 Reflexion de dibujo (neutral, Application)

- Plugin: dos puntos de linea en UCS → WCS (unico contacto con `Editor`, patron de `RackDuplicarCommands.cs:145`).
- Application: valor de colocacion `(Position, RotationRadians, signo de escala X/Y)` y regla de reflexion respecto de
  la linea `(q, φ)`:

  ```text
  p'  = q + Ref_φ(p − q)
  θ'  = 2φ − θ + π
  sx' = −sx ,  sy' = sy
  ```

  Generaliza la regla escrita de `PushBackMirror` (`:22-25`, caso `φ = 90°`). Candidato de hogar: extender
  `Transform2D` (Geometry) o un valor nuevo junto a el; **no** mover `PushBackMirror` (I-50 lo modifica, seccion 18).

### 13.2 Canonizacion semantica por vista

Una colocacion con determinante negativo se descompone en **(reflexion local del diseño, colocacion con escala
positiva)**. Para la reflexion de la X de la vista con tramo `[0, W]` y diseño reflejado `D' = T(W,0)·S(−1,1)·D`:

```text
T(p')·R(θ')·S(−1,1)·D  =  T(p' − W·(cos θ', sin θ'))·R(θ')·D'
```

- `W` es el **tramo del eje reflejado en el modelo** (p. ej. `PostXs[M]` del run, `TotalLength` de profundidad),
  no la caja de la vista con anotaciones.
- Solo es exacto si `plan(reflejar(diseño)) = F·plan(diseño)` salvo anotaciones y agrupacion ARRAY: prueba de
  conmutacion por kind y vista (seccion 22).
- En elevaciones solo es fisica la reflexion de la X de la vista; la de la Y (altura) es la misma mas un giro de
  180° del dibujo.
- Las vistas **no volteadas** por la reflexion fisica elegida (laterales bajo RUN, frontales bajo DEPTH) solo se
  re-indexan y necesitan una **regla de colocacion propia** (p. ej. centro reflejado conservando rotacion): el
  «bbox-shift» que RACKLAYOUT dejo pendiente.

### 13.3 Reflexion semantica del diseño (pura, por kind)

```text
Eje local       por kind: Selectivo {RUN, DEPTH}; Dinamico y Push Back simple {RT}; Push Back compuesto {RT, RD};
                Cantilever {R_X, R_Y}; cabecera {R_D, R_F}; cama {ninguno}
Mapa vista→eje  (kind, View, Section, eje de la vista) → eje local
Reflector       documento authored → documento authored reflejado | NoRepresentable(motivos atribuibles)
Remapeo seccion (View, Section, eje, conteos) → Section'
Invariantes     involucion: reflejar(reflejar(d)) = d en diseños representables;
                BOM(reflejar(d)) = BOM(d) como multiconjunto;
                PropertyValues, literales congelados, DimensionViews, SchemaVersion y ExtensionData intactos
Regla de grupo  un rack logico ⇒ UNA reflexion local; vistas seleccionadas que implican ejes distintos ⇒
                fail-closed o pregunta (PDC-4)
Anotaciones     se regeneran desde el diseño reflejado, legibles (principio de ADR-0031 §8)
```

- Precedentes de hogar: `SelectiveAuthoredRestamp` (transformacion pura sobre el documento authored,
  `RestampResult.cs:57-83`), `PushBackMirror` y `Transform2D` (Application).
- **Prohibido por las invariantes de variables:** reflejar pasando por el dominio y `WithDesign`, que solo re-congela
  `VerticalClearance` y materializaria `PalletTolerance` (`SelectivePalletDesignDocument.cs:316-330`).
- Diseños **NR** (seccion 9): fail-closed con diagnostico atribuible, o extension del modelo (cambio de formato ⇒ ADR).
  NO DECIDIDO (AM-5 / PDC-7).

---

## 14. Persistencia

### 14.1 Donde vive el estado

Una `BlockTableRecord` por vista con Xrecord `RACKCAD_SELECTIVE` (`RackBlockData.cs:16-89`); todas las referencias de
una definicion comparten payload (`SystemBlockWriter.cs:13`, `:33`). El diseño interior por kind:
`SelectivePalletDesignDocument` (selective), `RackProjectDocument` (dynamic, pushback, cantilever, cabecera),
`FlowBedDocument` (cama).

### 14.2 Que debe modificarse para que el rack NO se desespeje

1. **El diseño authored** reflejado, **identico** en todas las definiciones del rack resultante.
2. **`Section`** de las vistas seccionadas segun el remapeo (seccion 9).
3. En copia, **`Id`/`Name`** exterior e interior.
4. **La colocacion de cada referencia** con escala canonica (seccion 13.2), para que Insertar, RACKDUPLICAR y
   RACKLAYOUT no propaguen un espejo grafico.

**No se modifica:** registro de variables, `VariableId`, literales congelados, `DimensionViews`, `SchemaVersion` (salvo
promocion por regla ya vigente) ni `ExtensionData`.

### 14.3 Formato

- Si todas las propiedades se expresan con campos existentes: **sin cambio de formato** (criterio de I-51 §4).
- Casos **NR** (seccion 9) solo se resuelven cambiando formato (ADR + Owner) o bloqueando (fail-closed).
- **Riesgo S5:** los campos desconocidos anidados se pierden en el round-trip de los DTO (limitacion
  `JsonExtensionData` no recursiva de I-11); los kinds cuyo restamp es no-op hoy conservan el JSON byte a byte, y
  reflejarlos obligaria a deserializar y reserializar.

### 14.4 Espejo grafico actual (MIRROR nativo) — por que se «desespeja»

**INFERENCE sobre AutoCAD, con cada efecto anclado en codigo:** la referencia obtiene escala negativa y el diseño no
cambia (no hay reactores: 6.3). Tras eso:

| Efecto | Evidencia |
|---|---|
| Textos y cotas dentro del bloque se ven al reves (MIRRTEXT no aplica al contenido de bloques) | `LateralHeaderDrawer.cs:258-291` crea `DBText`/`RotatedDimension` en la definicion |
| RACKEDITAR carga el diseño sin reflejar: Izquierda/Derecha y numeracion contradicen el dibujo | `RackCommandSupport.cs:89-94` |
| Insertar agrega vistas **sin** espejo | `BlockPlacement.cs:174-201` |
| RACKDUPLICAR y RACKLAYOUT propagan la escala negativa | `RackDuplicarCommands.cs:306`; `RackLayoutCommands.cs:253` |
| Con «conservar originales», la referencia nueva comparte definicion: `RACKLISTA`/`RACKBOMTOTAL` cuentan una copia mas del mismo diseño | `BomTotal.cs:109-113` |

## 15. RACKEDITAR y redibujo

### 15.1 Ciclo trazado para un espejo semantico en copia (hipotetico)

| Paso | Que ocurre | Evidencia del camino existente |
|---|---|---|
| Espejo | diseño reflejado + restamp + `Section` remapeada → payload por definicion; geometria regenerada; referencias con colocacion canonica | seccion 13 |
| Payload authored | sobre con `Design` reflejado identico entre vistas | `RackEmbedDocument.cs:35-64` |
| Guardar | el Xrecord viaja en el BTR | `RackBlockData.cs:20-52` |
| Reabrir | `ScanEnvelopes` lee los sobres | `RackBlockFinder.cs:57-91` |
| RACKEDITAR | carga el diseño reflejado de la vista clicada | `RackCommandSupport.cs:74-99`; `RackMenuCommands.cs:115-152` |
| Actualizar | `FindRackBlocks(id)` → cada definicion redibujada por su `View`/`Section` desde el diseño reflejado; referencias sin recolocar | `RackSelectivoCommands.cs:125-223`; ADR-0010 |
| Redibujo | coincide con la geometria del espejo **solo si** el espejo regenero con los **mismos** builders | seccion 22, prueba de conmutacion |

### 15.2 No existe una regeneracion neutral por kind

- `IRackKindHandler` tiene `Kind`, `BomLabel`, `Edit`, `BuildBom`, `OutputBlockedReason`, `RestampDesign`
  (`src/RackCad.Plugin/KindHandlers/IRackKindHandler.cs:23-75`): **ningun** «regenerar esta definicion desde este
  diseño».
- Los puntos de redibujo son servicios por kind orquestados **dentro** de cada `EditX`: `SelectiveFrontalDrawService`,
  `LateralHeaderDrawService`, `SelectivePlantaDrawService`, `DynamicPlanta/Frontal/SystemDrawService`,
  `PushBackPlanta/Frontal/SystemDrawService`, `CantileverViewMaterializer.RedefineBlock`, `FlowBedDrawService`,
  `PlantaHeaderDrawService` (`RackSelectivoCommands.cs:178-217`; `RackDinamicoCommands.cs:212-248`;
  `RackPushBackCommands.cs:246-293`; `RackCantileverCommands.cs:322`; `RackCamaCommands.cs:226`;
  `RackCabeceraCommands.cs:269-280`).
- `RedefineSystemBlock` borra entidades, reconstruye definiciones anidadas, marca referencias y deja **purga
  post-commit** de anidadas huerfanas (`LateralHeaderDrawer.cs:88-182`); los editores terminan con **un** `Regen`.
  Ambas cosas estan **prohibidas** en RACKDUPLICAR por INV-14 de I-51: un espejo que regenera necesita su propia
  politica (AM-4).

### 15.3 Modos de fallo que el diseño debe impedir

| Fallo | Consecuencia | Evidencia |
|---|---|---|
| Espejo parcial en sitio con `RackId` conservado | la siguiente RACKEDITAR reescribe hermanas desde la vista clicada | `RackSelectivoCommands.cs:153-223` |
| `Section` sin remapear | lateral muestra el corte equivocado; vista borrada como fantasma si la seccion excede | `RackSelectivoCommands.cs:168-199`, `:225-239` |
| Diseño reflejado via dominio + `WithDesign` | materializa `PalletTolerance` | `SelectivePalletDesignDocument.cs:316-330` |
| Regenerar con builders distintos de los de RACKEDITAR | el primer Actualizar cambia el dibujo | — |
| Insertar tras un espejo grafico | vista nueva sin espejo | `BlockPlacement.cs:174-201` |

## 16. BOM

- **FACT:** el BOM sale del **diseño authored**, nunca de entidades dibujadas: `handler.BuildBom(embed, catalog,
  projectVariables)` sobre una vista representativa (`RackInventarioCommands.BomTotal.cs:198`, `:251-266`;
  `SelectiveKindHandler.cs:36-70`).
- **FACT:** copias = maximo de referencias directas por vista del `RackId`; cantidades multiplicadas por ese numero
  (`BomTotal.cs:109-113`; `ConsolidatedBom.cs:59`, `:80`).
- **FACT:** representativa = primera hermana en orden de tabla; solo Selectivo compara autoridad
  (`BomAuthoredAuthority.cs:104-110`).
- **FACT:** **ningun numero de parte ni bloque codifica mano** en `assets/catalogs/*.csv`: `DESVIADOR_L_*` es la
  **forma** L; `INICIO_IZQUIERDO`/`INICIO_DERECHO` son **puntos de conexion** de larguero; `LATERAL_IZQ`/`LATERAL_DER`
  son vistas no usadas por el codigo. Las claves de agrupacion son (categoria, pieza, longitud[, peralte]).
- **INFERENCE — cantidades invariantes** si cada eleccion dependiente de lado se remapea: cabeceras, postes, placas,
  celosia, largueros y mensulas, separadores, postes derivados, camas, rieles, rodillos, frenos, topes, seguridad y
  todos los componentes de Cantilever.
- **Casos que exigen tratamiento explicito o fallan en cantidad/identidad:**

  | Caso | Por que | Evidencia |
  |---|---|---|
  | Push Back compuesto: bota, defensa y tope posterior **por lado** | la pieza depende del lado | `PushBackBootPlan.cs:141-158`; `PushBackDefenseSides.cs:35-95`; `PushBackRearTopeBuilder.cs:37-48` |
  | Tope Selectivo | Left usa el fondo `c`, Right el `c+1`: longitudes distintas si difieren sus claros | `SelectiveTopePlan.cs:103-156` |
  | Desviador Selectivo | cara delantera toma niveles del fondo mas delantero; trasera del mas trasero | `SelectiveDesviadorPlan.cs:116-136` |
  | Dinamico/Push Back: defensa salida/entrada; guias solo en entrada | longitudes por extremo | `DynamicForkliftDefensePlan.cs:50-79` |
  | Placas de Cantilever | id con patron medido desde `Outline[0]` | `CantileverStationBomBuilder.cs:292-314` |

- Prueba candidata por kind: `BOM(reflejar(d)) = BOM(d)` como multiconjunto, sobre fixtures asimetricos (seccion 22).

## 17. ProjectVariables

- **FACT:** solo Selectivo tiene vinculos: `PropertyValues` = mapa `PropertyId → {Kind: projectVariable, VariableId,
  ExtensionData}` (`SelectivePalletDesignDocument.cs:128-129`; `SelectivePropertyValueDocument.cs:25-38`); dos
  propiedades vinculables, ambas escalares del rack: `selective.verticalClearance` y `selective.palletTolerance`
  (`SelectiveLinkedProperties.cs:193`, `:208`). **Ninguna depende de lado.**
- **FACT:** I-51 preserva vinculos porque `SelectiveAuthoredRestamp` hace ida y vuelta por el **documento** y solo toca
  `Id`/`Name` (`RestampResult.cs:57-83`; T14 en `SelectiveDuplicationFailClosedTests.cs:222`); decisiones C-4, C2-7 y
  C4.7-4 de I-47 y ADR-0034 §12.
- **Lo que I-52 necesita para `ProjectVariableReference(X) → X`:**
  1. reflejar sobre el **documento authored**, copiando `PropertyValues` tal cual y sin evaluar;
  2. **no** pasar por dominio + `WithDesign` (seccion 13.3);
  3. conservar entradas de `Kind` desconocido: I-49 V3 preve entradas `expression` que deben sobrevivir a la
     duplicacion (P24.8 de su Proposal, en su rama) y, por la misma razon, al espejo;
  4. no leer ni escribir el registro (`ProjectVariablesDocument`).
- **FACT:** una copia con `RackId` nuevo es **otro consumidor** del mismo `VariableId` (bloquea `Delete`, se redibuja en
  `ChangeValue`, se valida contra sus propias vistas) (`ProjectVariableMutationPreflight.cs:137-202`). `ChangeValue`
  redibuja desde el authored almacenado (`ProjectVariableMutationExecutor.cs:145-217`): si el authored esta reflejado,
  el cambio de valor **no** desespeja.
- **FACT:** en sitio con `RackId` conservado, el descubrimiento exige autoridad unica entre vistas o aborta
  (`ProjectVariableConsumerDiscovery.cs:144-151`, `:179-184`).

## 18. Interaccion con I-49 e I-50

### 18.1 Estado observado

```text
I-49  origin/architecture/motor-expresiones-parametricas = 4df9480   solo docs/ (Proposal V3; Architect PENDING RE-REVIEW;
                                                                    Implementation BLOCKED; ADR REQUIRED, sin numero)
I-50  origin/feature/cotas-independientes-por-vista      = a4806ff   17 archivos de src/ y 17 de tests/ (G5 A/B/C)
Ambas bifurcan de a4d88f1: 9 commits por detras de origin/main (no contienen el merge de I-51).
```

### 18.2 Cruces con I-50

| Archivo / contrato | I-50 | Posible I-52 | Tratamiento antes del Candidato |
|---|---|---|---|
| `src/RackCad.Application/Systems/PushBack/PushBackMirror.cs` | anade `DimensionViews = source.DimensionViews` (1 linea) | reutilizar o extraer la regla | **No** mover ni editar `PushBackMirror` sin procedimiento de colision; generalizar la regla en un tipo nuevo |
| `SelectivePalletDesign.cs` (**archivo caliente**, WORKFLOW §7), `SelectivePalletDesignDocument.cs` | propiedad y mapeo `DimensionViews` | solo si hubiera cambio de formato | Evitar tocarlos; si el formato cambia, serializar con I-50 |
| `DynamicRackDesign.cs`, `DynamicRackSystemDocument.cs`, `SelectiveEditorState.cs`, `SelectiveDesignInputs.cs`, `SelectiveDepthLayout.cs`, `SelectiveGeometryResolver.cs`, `PushBackEditorState.Load.cs` | modificados | lectura; quiza materializacion de herencias | Sin edicion prevista; revalidar tras rebase |
| Contrato de «sitios de copia» C-01..C-15 (`DimensionViewsCopySitesTests.cs`) | cada sitio de copia transporta el `int` exacto | **cada reflector es un sitio de copia nuevo** | Prueba cruzada: el espejo conserva `DimensionViews` exacto (centinelas `-8`, `13`, `null`) |
| `DimensionViewsRestampTests` (T-15) | restamp no toca la politica | el espejo restampa tras reflejar | Misma prueba sobre el camino del espejo |
| `docs/ROADMAP.md` (fila), `docs/ideas-futuras.md` (seccion final), numeracion ADR (0035 es de I-50) | modificados | fila ya anadida; posible ADR | Conflicto documental trivial; numero de ADR a coordinar |

### 18.3 Cruces con I-49

| Archivo / contrato | I-49 V3 | Posible I-52 | Tratamiento |
|---|---|---|---|
| `PropertyValues` con entradas `expression` (P24.8) | duplicar conserva `expression` con el mismo `VariableId` | el espejo debe conservar entradas de kind desconocido | Prueba cruzada: Selectivo con `expression` y `DimensionViews` sobrevive al espejo y a RACKEDITAR |
| `RackDuplicationPlan.cs`, `RackEnvelopeRestamp.cs`, `SelectiveAuthoredAuthority.cs`, store del Selectivo | condicion de parada si un gate suyo necesita cambiarlos (§12.2 de V3) | I-52 podria generalizar el planificador | Si I-52 los cambia, **reporta** a I-49 antes de editar; integracion serializada |
| `SelectiveGeometryResolver.cs` | alias en G6 | lectura | sin cruce previsto |
| Numero de ADR | «ADR REQUIRED, not yet written» | posible ADR | coordinar numeracion antes de escribir |

### 18.4 Lo que debe reconciliarse antes del Candidato de I-52

1. `git fetch --all --prune` y `git diff --name-only origin/main...origin/<rama>` de I-49 e I-50.
2. Si I-50 integra antes: rebase y **anadir el reflector a su contrato de sitios de copia**; si integra despues, I-50
   anade el sitio.
3. Si I-49 integra antes: prueba de supervivencia de `expression` en el espejo; si despues, la anade I-49.
4. Numero de ADR y filas de ROADMAP en orden numerico.
5. **No** se mezclan ramas ni se anaden dependencias directas.

---

## 19. Riesgos y unknowns

| # | Riesgo / unknown | Severidad | Donde se decide |
|---|---|---|---|
| U-01 | Una linea implica reflexiones fisicas distintas por vista; conflicto dentro de un grupo | alta | AM-1, PDC-4 |
| U-02 | Punto de vista de la frontal (Selectivo) y mano de las camaras (Cantilever) | media | Owner, validacion visual |
| U-03 | Diseños no representables (S-01, S-02, S-09, S-13, D-08, PC-05, K-06, K-08, H-02) | alta | AM-5, PDC-7 |
| U-04 | No hay regeneracion neutral por kind; efectos laterales (purga, anidadas, `Regen`) frente a INV-14 de I-51 | alta | AM-4 |
| U-05 | Colocacion: tramo `W`, vistas no volteadas, elevaciones de cabeza | media | AM-3, PDC-4 |
| U-06 | Seleccion parcial en sitio ⇒ divergencia silenciosa en 5 kinds | alta | PDC-1, PDC-3 |
| U-07 | Referencias enlazadas parcialmente seleccionadas en sitio: mutar la definicion afecta a las no seleccionadas | alta | PDC-1 |
| U-08 | Divergencia authored no detectada fuera de Selectivo (`BomAuthoredAuthority.cs:104-110`) | media | AM-7 |
| U-09 | Perdida de campos desconocidos anidados al reflejar (S5) | media | AM-2 |
| U-10 | Numeracion de frentes y letras A/B siguen al indice | baja | PDC-5 |
| U-11 | Protector LATERAL con dos lecturas (Dinamico) | media | PDC-6 |
| U-12 | Cantilever: separador con `outward = +1`, secciones asimetricas, id de placas | media | AM-5 |
| U-13 | Cabecera: paridad Auto, rasgos del poste izquierdo, K-bracing ya divergente | media | PDC-7 |
| U-14 | `HeaderRunPlan.PlacedClone` conserva la rotacion al espejar | baja (latente) | seccion 24 |
| U-15 | Nombre del comando (`RACKMIRROR` frente a la convencion española), alias libre y censo de 33 | baja | PDC-8 |
| U-16 | Granularidad de UNDO (M9 de I-51 la observo) | baja | validacion |
| U-17 | Paper Space, MINSERT (L-4 de I-51), `Normal` no WCS, UCS | media | PDC-9 |
| U-18 | Orden de integracion con I-50 (sitios de copia) e I-49 (`expression`) | media | seccion 18 |
| U-19 | Necesidad y numero de ADR | media | Arquitecto |
| U-20 | Espejo en sitio borrando originales: rack parcial y doble conteo en BOM (B1) | alta | PDC-1 |

## 20. Estructura propuesta para la Proposal V1

```text
0.  Estado de consenso (Coordinator / Architect) y SHAs
1.  Decisiones de producto requeridas (PDC-1..PDC-10) con opcion recomendada y alternativa
2.  Modelo semantico: ejes locales por kind, mapa vista→eje, regla de grupo
3.  Modelo de transformacion neutral: regla de linea, canonizacion, colocacion de vistas no volteadas
4.  Reflectores por kind: contrato, matriz de representabilidad, diagnosticos fail-closed
5.  Identidad y nombre (reuso NI-1..NI-6) y mapa OldRackId→NewRackId
6.  Seleccion y plan: que se reutiliza de RackDuplicationPlan y que se extrae (con politica de guardas)
7.  Regeneracion por kind: capacidad nueva, transaccion PREPARE→MUTATE, politica post-commit
8.  Persistencia: sin cambio de formato o ADR; campos desconocidos; camino authored del Selectivo
9.  RACKEDITAR / redibujo / remapeo de Section / etiquetas
10. BOM: invariancia y casos por lado
11. ProjectVariables: invariantes y pruebas cruzadas con I-49
12. Pruebas (Core, UI, guardas del Plugin) con RED demostrado
13. Checklist de validacion del Owner en AutoCAD
14. Reconciliacion con I-49 e I-50
15. Gates G3+ y condiciones de parada
16. Decision de ADR
```

## 21. Archivos y simbolos que probablemente cambiarian (sin cambiarlos)

**Produccion (INFERENCE, depende de las decisiones):**

| Capa | Archivo o area | Motivo |
|---|---|---|
| Plugin | comando nuevo (p. ej. `src/RackCad.Plugin/RackMirrorCommands.cs`) | adquisicion, linea UCS→WCS, transaccion |
| Plugin | `KindHandlers/IRackKindHandler.cs` + 6 handlers, **o** servicios extraidos de `RackSelectivoCommands.cs`, `RackDinamicoCommands.cs`, `RackPushBackCommands.cs`, `RackCantileverCommands.cs`, `RackCabeceraCommands.cs`, `RackCamaCommands.cs` (grupo caliente) | regeneracion por kind |
| Plugin | `RackDuplicarCommands.cs` (solo si se extraen fases compartidas) | reuso de SNAPSHOT/PREPARE |
| Application | `Persistence/RackDuplicationPlan.cs` (nombre y asignador) o planificador nuevo | seleccion e identidad |
| Application | `Geometry/Transform2D.cs` o tipo nuevo junto a el | regla de linea y colocacion |
| Application | reflectores nuevos por kind en `Systems/<Kind>/` | diseño authored reflejado |
| Application | remapeo de `Section` por kind (junto a `PushBackSystemFrontalBuilder.EncodeSection` y equivalentes) | vistas seccionadas |
| UI | `src/RackCad.UI/RackCommandReference.cs` | ayuda |

**Pruebas:** reflectores (Core), regla de linea (Core), planificador (Core), guardas del comando nuevo, censo de comandos
(`SelectiveEditorOpenTests.cs:540-546`), guardas de texto de handlers si cambia la interfaz.

**Documentacion:** `docs/guias/despliegue.md`, `README.md`, `docs/guias/validacion-manual-autocad.md`,
`docs/ideas-futuras.md`, ADR si procede, este contrato y `docs/automation/decisions/I-52.md`.

**NO TOCAR sin revision de Arquitecto (propuesta):** `PushBackMirror.cs` (I-50), `SelectivePalletDesign.cs` y
`SelectivePalletDesignDocument.cs` (caliente / I-50), `RackEnvelopeRestamp.cs` (G-R5), `RackCloner.cs`,
`SelectiveAuthoredAuthority.cs` y el store Selectivo (I-49), `ProjectVariables/*`, `Bom/*`, editores WPF grandes,
catalogos, biblioteca de bloques, `.github/`, ADR aceptados.

## 22. Tests de caracterizacion que haran falta

| # | Caracteriza | Tipo |
|---|---|---|
| CT-01 | Fixtures asimetricos por kind y vista con firmas de plan (vista, rol, pieza, bloque, insercion, rotacion, espejo) antes de cualquier reflector | Core, goldens |
| CT-02 | BOM base de esos fixtures (para comparar `BOM(reflejar(d))`) | Core |
| CT-03 | Round-trip de cada store con campos desconocidos anidados: que se conserva y que se pierde hoy | Core |
| CT-04 | Codificacion de `Section` por kind (Selectivo fondo/poste, Dinamico 0/1, Push Back 0..3, Cantilever estacion) | Core |
| CT-05 | `PushBackMirror.Structure` involutivo y sus huecos (`FirstLevelDatum` no copiado) | Core |
| CT-06 | `Transform2D`: composicion y determinante con reflexiones | Core |
| CT-07 | `WithDesign` materializa `PalletTolerance` vinculada (hueco actual) | Core |
| CT-08 | `SelectiveDesviadorPlan.CellKey` colapsa N−1/N en Dinamico | Core |
| CT-09 | `BracingPanel.ResolveDiagonalDirection` por paridad | Core |
| CT-10 | `HeaderRunPlan.Flatten` con instancia rotada en colocacion espejada | Core |
| CT-11 | Separador Cantilever con `outward = +1` (posible defecto existente) | Core |
| CT-12 | `SelectiveAuthoredAuthority` distingue un documento reflejado de su origen | Core |
| CT-13 | Censo de comandos (33) y unicidad de `[CommandMethod]` como linea base | Core (guardas) |
| CT-14 | Prueba cruzada con I-50: el espejo conserva `DimensionViews` exacto | Core, tras reconciliar |
| CT-15 | Prueba cruzada con I-49: entradas `expression` sobreviven al espejo | Core, tras reconciliar |
| CT-16 | Conmutacion `plan(reflejar(d)) = F·plan(d)` salvo anotaciones, por kind y vista | Core (primer rojo del reflector) |
| M-x | Owner: frontal/lateral/planta reflejadas con UCS girado; guardar/reabrir; RACKEDITAR → Actualizar; Insertar; BOM; variables (`ChangeValue`); UNDO | AutoCAD 2025 |

---

## 23. Paquete autonomo para revision del Arquitecto

### 23.1 Pregunta

¿Es sostenible, sobre el codigo de `46fcac2`, un RACKMIRROR **semantico** para uno o varios racks que no se desespeje en
RACKEDITAR, redibujo, BOM ni variables, y con que decisiones materiales?

### 23.2 Decisiones arquitectonicas materiales candidatas

| AM | Decision | Opciones observadas |
|---|---|---|
| **AM-1** | Modelo semantico: una reflexion local por rack logico, mapa vista→eje, regla de grupo | fail-closed ante vistas incompatibles / eje derivado de la planta / pregunta al usuario |
| **AM-2** | Hogar del reflector del diseño | reflectores puros por kind en Application / un reflector generico sobre JSON / miembro nuevo de `IRackKindHandler` como fachada |
| **AM-3** | Representacion de la transformacion | extender `Transform2D` / valor nuevo de colocacion / generalizar `PushBackMirror` (colisiona con I-50) |
| **AM-4** | Regeneracion por kind | crear definiciones desde el diseño reflejado / clonar y redibujar; politica de purga y `Regen` frente a INV-14 de I-51 |
| **AM-5** | Diseños no representables | fail-closed con diagnostico / extension del modelo (cambio de formato ⇒ ADR) |
| **AM-6** | Identidad y nombre segun producto | seccion 11 |
| **AM-7** | Reuso de I-51 | generalizar `RackDuplicationPlan` (nombre, regla id≠fuente, exposicion de sobres, consistencia no selectiva) / planificador nuevo reutilizando clave y clasificacion; re-apuntado de guardas con RED |
| **AM-8** | Remapeo de `Section` y etiquetas | por kind junto a su codificacion existente |

### 23.3 Decisiones de producto candidatas (Owner / Coordinador)

| PDC | Pregunta |
|---|---|
| **PDC-1** | Copia (A), prompt MIRROR (B, con B1/B2) o edicion del diseño (C) |
| **PDC-2** | Nombre de la copia reflejada |
| **PDC-3** | Seleccion parcial: solo lo seleccionado (analogo PD-1 de I-51); ¿se admite verificar cobertura sin expandir? |
| **PDC-4** | Vistas que implican ejes distintos en un grupo: bloquear, derivar de la planta o preguntar «Frentes / Fondos» |
| **PDC-5** | Numeracion de frentes y letras A/B tras el espejo |
| **PDC-6** | Significado de Izquierda/Derecha del protector LATERAL |
| **PDC-7** | Que hacer con diseños no representables; ¿diagonales estandar `AutoAlternating` sin espejar son aceptables? |
| **PDC-8** | Nombre del comando y alias |
| **PDC-9** | Espacios (solo Model Space como I-51), MINSERT, UCS/`Normal` |
| **PDC-10** | Sistemas en alcance del primer corte (todos, o por fases) |

### 23.4 Evidencia verificada directamente por el ejecutor

| # | Afirmacion | Evidencia |
|---|---|---|
| V-01 | `Transform2D` refleja; sin inversa ni linea arbitraria | `Transform2D.cs:20-116` |
| V-02 | Regla y texto de `PushBackMirror` | `PushBackMirror.cs:10-78` |
| V-03 | RACKEDITAR lee solo el payload de la definicion | `RackCommandSupport.cs:74-99` |
| V-04 | `FindRackBlocks` por `Id` sin mayusculas en todo el dibujo | `RackCommandSupport.cs:109-134` |
| V-05 | Campos del sobre | `RackEmbedDocument.cs:14-65` |
| V-06 | Actualizar escribe un `designJson` a todas las hermanas; borra fantasmas | `RackSelectivoCommands.cs:114-244` |
| V-07 | Restamp solo identidad | `RackEnvelopeRestamp.cs:31-112` |
| V-08 | Sin reactores ni suscripciones | `PluginInitializer.cs:5-13`; busqueda de `+=` |
| V-09 | `CopyName`, autoridad solo Selectivo, `Build`, asignador | `RackDuplicationPlan.cs:225`, `:355`, `:536-538`, `:654-657` |
| V-10 | Nombre repetido, desplazamiento UCS, transformacion copiada | `RackDuplicarCommands.cs:89`, `:145`, `:303-307` |
| V-11 | Censo de comandos = 33 | `SelectiveEditorOpenTests.cs:540-546` |
| V-12 | RACKLAYOUT: semilla espejado propagado; espejo real pendiente | `RackLayoutCommands.cs:170-171`, `:246-249` |
| V-13 | Planta Selectiva `X = fondo`, `Y = frente` | `SelectivePlantaBuilder.cs:15-22` |
| V-14 | Medio frente: ultimo tramo calculado desde el poste izquierdo | `SelectiveMedioFrente.cs:6-18`, `:70` |
| V-15 | `WithDesign` solo re-congela `VerticalClearance` | `SelectivePalletDesignDocument.cs:316-330` |
| V-16 | Autoridad authored incluye todo por defecto | `SelectiveAuthoredAuthority.cs:55-121` |
| V-17 | BOM no compara autoridad fuera de Selectivo | `BomAuthoredAuthority.cs:104-110` |
| V-18 | Signo de traslape Cantilever | `CantileverLineResolver.cs:271-277` |
| V-19 | Paridad `AutoAlternating` | `BracingPanel.cs:35-48` |
| V-20 | Salida sin espejo, entrada espejada | `DynamicLoadBeamGeometry.cs:166-190` |
| V-21 | Modulos compuestos A → GAP → B invertidos | `PushBackCompositeStructure.cs:495-531` |
| V-22 | `EncodeSection`/`DecodeSection` | `PushBackSystemFrontalBuilder.cs:134-155` |
| V-23 | `CellKey` con `Math.Min` | `SelectiveDesviadorPlan.cs:20-31` |
| V-24 | Drive-In: boton deshabilitado | `RackMainMenuWindow.xaml:139-144` |
| V-25 | Camaras Cantilever y `Camera` | `CantileverViewPlanBuilder.cs:205-235`; `Spatial3D.cs:179-198` |
| V-26 | `PlacedClone` conserva rotacion | `HeaderRunPlan.cs:53-80` |
| V-27 | Bota Left→EntryExit, Right→Rear | `BootPlacement.cs:41-65` |
| V-28 | Seccion frontal dinamica; RACKEDITAR con lookup ordinal | `RackDinamicoCommands.cs:393`; `RackMenuCommands.cs:141` |
| V-29 | `IRackKindHandler` sin regeneracion; puntos de redibujo por kind | `IRackKindHandler.cs:23-75`; busqueda de `RedrawInPlace(` |
| V-30 | ADR-0031 §8, ADR-0009, ADR-0010 | `docs/adr/0031-...md:162-168`; `0009-...md`; `0010-...md` |
| V-31 | Dos propiedades vinculables | `SelectiveLinkedProperties.cs:193`, `:208` |
| V-32 | Diff de I-50 (incluye `PushBackMirror.cs`) y contrato de sitios de copia | `git diff origin/main...origin/feature/cotas-independientes-por-vista` |
| V-33 | I-49 solo docs; estado de V3; condicion de parada §12.2 | `git show origin/architecture/motor-expresiones-parametricas:docs/initiatives/I-49-proposal-v3.md` |
| V-34 | ADR: `main` hasta 0034; 0035 en I-50; I-49 sin numero | `docs/adr/`; `adr/README.md` de I-50 |

El resto de citas procede de las auditorias de solo lectura sobre el mismo arbol y se marca FACT o INFERENCE en su
tabla.

## 24. Hallazgos laterales (fuera de alcance; no se arreglan en I-52)

Se registraran en `docs/ideas-futuras.md` en G2 (precedente I-51), no en G1:

1. `HeaderRunPlan.PlacedClone` conserva `RotationRadians` al espejar una colocacion (`HeaderRunPlan.cs:53-80`); con
   `PushBackMirror` la rotacion se niega. Latente: `Flatten` solo lo consumen vistas previas y pruebas.
2. `RackEmbedDocument.View` null documentado como frontal (`:40`), pero dinamico, Push Back y cabecera escriben lateral
   y RACKLISTA lo muestra como lateral (`RackListBuilder.cs:117-118`).
3. RACKLISTA no tiene etiqueta para `pushback` (`RackListBuilder.cs:92-95`).
4. `RackCommandReference` omite `RACKPUSHBACK`, `RACKVARIABLES` y `RACKSECCION`.
5. Comentarios «cinco kinds» desactualizados (`KindHandlerDispatch.cs:12-13`; `SystemRegistry.cs:11`;
   `IRackKindHandler.cs:43`; `RackMenuCommands.cs:139-140`).
6. RACKEDITAR resuelve el kind con lookup **ordinal** (`RackMenuCommands.cs:141`) mientras RACKDUPLICAR y el restamp lo
   hacen sin mayusculas.
7. La vista insertada con Insertar no regenera en selectivo, dinamico, Push Back ni cabecera, contra el comentario de
   `RackPushBackCommands.cs:317-318`.
8. K-bracing: el dibujo lateral traza una diagonal y el constructor de miembros cuatro (`LateralHeaderLayoutBuilder.cs:292-294`
   frente a `BracingPanelMemberBuilder.cs:199-205`).
9. Separador Cantilever siempre `Mirrored = true` (`CantileverLineFrameResolver.cs:61-85`): posible solape con
   `outward = +1` (K-11).
10. `CantileverStationBaseSideResolver.cs:239` pasa radianes a un parametro de grados (inocuo con rotacion 0).
11. `WithDesign` solo re-congela `VerticalClearance` aunque `PalletTolerance` es vinculable (seguro hoy porque el
    guardado pasa por el reconciliador).
12. Comentarios de Push Back que aun anclan el extremo alto, contra ADR-0031 §9 (`PushBackFlowBedGeometry.cs:28-29`,
    `PushBackLoadBeamGeometry.cs:13-21`, `PushBackBedSpan.cs:247-250` y otros); ADR-0031 tiene **dos** secciones «8-bis».
13. `AuthoredSide` documentado como no persistido (`SelectivePalletDesign.cs:171-176`) pero persistido por el DTO.
14. Comentarios de tope previos a I-46 («back post», «TROQUEL_SEPARADOR») en `SelectiveTopePlan.cs:9-11`,
    `SelectiveLateralBuilder.cs:161`, `:322`, `SelectiveBomBuilder.cs:260`, `SelectiveSafetyPlacement.cs:51`, y
    `docs/ideas-futuras.md:97`.
15. `docs/guias/generacion-cabecera-lateral.md:186-202` lista 4 kinds y dice que el dinamico solo dibuja lateral.
16. `assets/catalogs/views.csv:3-4` declara `LATERAL_IZQ`/`LATERAL_DER`, sin uso en codigo.

---

```text
G0 = CLOSED     reclamo 5528176 + bootstrap 7ee7975
G1 = DELIVERED  este informe (solo docs)
G2 = NOT STARTED — Proposal V1 requiere orden explicita del Coordinador

SUBSTANTIVE IMPLEMENTATION: BLOCKED
Coordinator: NOT YET AGREED
Architect: NOT YET AGREED
```

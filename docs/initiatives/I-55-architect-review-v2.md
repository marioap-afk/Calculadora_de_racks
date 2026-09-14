# I-55 — Revision del Arquitecto sobre la Proposal V2

```text
Objeto de la revision  = docs/initiatives/I-55-proposal-v2.md @ f84f303adc132a7d72ebc3e2c5cf3d7bf9faf6e1
                         (con el mapa de implementacion V2 y ADR-0042 del mismo commit)
Paquete usado          = docs/initiatives/I-55-architect-review-package-v2.md @ 1e1241471377ad76e4edbf1a1f6dc8e71db33c32
Base del codigo        = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093 (sin cambios de produccion en la rama)
Paralelas observadas   = I-52 2275f21 (Proposal V11) al abrir G2C; 915a520 (Proposal V12) al cerrar (§4.1)
                         I-49 75f1862 · I-56 12fb509 (G0.1, solo docs y proceso; I-55 grandfathered)
Orden                  = G2C del Coordinador de I-55

Architect              = CHANGES REQUIRED — PROPOSAL V3
BLOCKER / HIGH         = ninguno
Hallazgos              = MEDIUM AR2-01..AR2-06 · LOW AR2-07..AR2-20
X-1..X-8               = AGREED (8 de 8) · ningun MATERIAL CONFLICT · registro bilateral con I-52 pendiente
A-8                    = (a) complemento + nota posterior fechada en ADR-0010
Owner                  = decisiones NO registradas (el Coordinador recomienda todas las opciones A)
Consensus              = NOT REACHED
Implementation         = BLOCKED
```

> **Nota de transparencia.** La orden G2C asigna la revision del Arquitecto a esta sesion, que es la misma que redacto la
> Proposal V2, como en el precedente registrado de I-52 (`docs/automation/decisions/I-52.md` §44 en su rama). Para no
> revisar con el sesgo del autor, tres revisores independientes de solo lectura intentaron refutar V2 contra el codigo de
> `dad4e77`, contra git y contra la Proposal V11 de I-52, con ejemplos numericos propios. Cada hallazgo de este documento se
> verifico despues en el codigo antes de admitirlo. Si el Coordinador o el Owner consideran que el veredicto debe emitirlo
> otro Arquitecto, este documento queda como revision tecnica previa y el veredicto formal pasa a `PENDING`.

## 1. Veredicto por pregunta

| Pregunta | Veredicto | Motivo |
|---|---|---|
| A-1 variante y codec | AGREED WITH CHANGES | Diseno correcto; el codec no puede calcular disposiciones que dependen del sistema resuelto y el mapa mete politicas en el codec (AR2-05, AR2-17) |
| A-3 transaccion de ID18 | AGREED WITH CHANGES | El modelo C se sostiene sin escrituras ocultas; falta especificar el redibujo parcial (AR2-02, AR2-14) |
| A-4 `RackViewKind` | AGREED | Renombre puro: el tipo no se serializa, ADR-0035 no lo nombra, ninguna guarda se rompe y 6 + 6 archivos es exacto (AR2-18, solo redaccion) |
| A-6 Group Placement | AGREED WITH CHANGES | Base correcta (una transformacion rigida por ejecucion, anclas por la transformacion completa, `Origin = 0`, `Rigid`); defecto en la politica ortografica para Cantilever y precisiones (AR2-01, AR2-07..AR2-12) |
| A-7 gate de propiedades | AGREED | Construible con las API publicas de I-54 sin romper `TGrd02`, `TGrd04` ni `TGrd05`; la herencia por `Compose` es real; sin TOCTOU (AR2-16, LOW) |
| A-8 relacion con ADR-0010 | (a) complemento + nota posterior fechada | §3 |
| D-05 ciclo de vida del `RackId` | AGREED WITH CHANGES | AR2-16 |
| D-10 limpieza | AGREED WITH CHANGES | AR2-15 |
| D-13 verificacion antes de materializar | AGREED WITH CHANGES | AR2-03 |
| D-15 redibujo fallido | AGREED WITH CHANGES | AR2-02 |
| D-17 resolucion sin editor | AGREED WITH CHANGES | AR2-04; pasaria a CHANGES REQUIRED si X-2 no fija donde vive la resolucion |
| X-1..X-8 | AGREED | §4 |

**Veredicto global: `CHANGES REQUIRED — PROPOSAL V3`.** La arquitectura (ALT-B, ID19 en dos niveles, gate acotado al
`RackId`, politica de codec, orden de gates y prerrequisitos) queda acordada y los cambios son una lista cerrada, sin
opciones de diseno abiertas. Pero corrigen una formula normativa, anaden un invariante, cambian la maquina de estados de
ID18 y el alcance de gates, y la orden G2C manda crear una Proposal V3 en ese caso y revisarla de nuevo sobre su SHA exacto.

**Confirmado sin hallazgos.**
- La composicion `Translation(−B).Then(Rotation(α)).Then(Translation(T))` lleva `B` a `T` con `Determinant = +1` y `ScaleFactor = 1`
  (`A/Geometry/Transform2D.cs:77`, `:83`, `:86-93`).
- `Rigid`: con `Origin ≠ 0`, escala (−1,−1) y α = 35°, la referencia nueva coincide con `T_common` en todos los puntos.
- `Orthographic` en los casos declarados: fila de plantas → frontales, el mismo layout girado 30°, planta a 2π − ε, rack girado 180°
  e ida y vuelta planta → frontal → planta.
- No hay escrituras antes de PREPARE: la guarda de unidades solo avisa y las importaciones ocurren al crear o redibujar
  (`P/Systems/Shared/SystemBlockWriter.cs:55-66`).
- Durante la entrada del usuario solo esta abierta la transaccion del propio jig (`P/Drawing/BlockPlacement.cs:180-200`).
- `RackEmbedComposer.Compose` hereda `CustomProperties` del sobre fuente (`A/Persistence/RackEmbedComposer.cs:59`) y las cinco ramas de
  Insertar pasan el sobre elegido.
- El descriptor de ejes de §12.3 no lo contradice el codigo y queda honestamente marcado como hipotesis de G3.

## 2. Hallazgos

### MEDIUM

**AR2-01 (A-6) — La politica ortografica rechaza Cantilever → Planta cuando toda la seleccion es Cantilever.**
- **Evidencia:** §12.5 toma `c_t(K)` de la familia rack en plantas con OD-6.d = A (R → (0,1), D → (1,0)). La planta Cantilever es
  X = −R, Y = +D (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:209-219`; `A/Geometry/Spatial3D.cs:186-197`).
  - Frontales Cantilever en (0, 0) y (−400, 0) → Planta: `w(r) = (−1, 0)` es perpendicular a `w = (0, 1)` y la ejecucion falla como OM-12.
  - Laterales → Planta: `(0, 1)` es perpendicular a `(1, 0)`.
  - Tomando `c_t` de la familia destino: `α = 0` y plantas en (0, 1000) y (−400, 1000), con la separacion y INV-GRP-1 intactas.
- **Por que importa:** §5, §12.6 y OD-7.a = A exponen ese par, y las pruebas de G14 congelarian el rechazo.
- **Cambio:** `c_t(K)` = eje local de K en el marco destino de la familia de la seleccion (una sola familia con OD-7.d = A). Las
  familias mezcladas quedan con su propia regla (OD-7.d = B) y OM-12 se reformula.

**AR2-02 (A-3, D-15) — `REDRAW_FAILED` no refleja que cada redibujo confirma por separado.**
- **Evidencia:**
  - Cada redibujo confirma su propia transaccion (`P/Systems/Shared/SystemBlockWriter.cs:61-66`; `P/Drawing/LateralHeaderDrawService.cs:138-143`;
    `P/RackCantileverCommands.cs:319-326`).
  - Los fallos se ignoran hoy (`P/RackSelectivoCommands.cs:178-183`; `P/RackCantileverCommands.cs:328-332`).
  - El payload de cada miembro se compone dentro del bucle, entre escrituras (`P/RackSelectivoCommands.cs:177`), y `Compose` puede
    lanzar por la regla F (`A/Persistence/RackEmbedComposer.cs:103-121`) o por F-14b (`docs/adr/0039-custom-properties-persistencia-autoridad.md:224-228`).
- **Consecuencias:**
  - Tras `REDRAW_FAILED`, el miembro fallido conserva el authored viejo y sus hermanas el nuevo, lo que contradice INV-AUTH-1 tal como
    esta redactado.
  - §11 y §15 no coinciden sobre la regeneracion tras un redibujo parcial.
  - No se dice si el bucle se detiene en el primer fallo.
  - Una excepcion tras confirmaciones parciales no tiene estado.
- **Cambio:**
  - Componer y serializar todos los payloads de miembros en PREPARE ALL (patron `PreparedViewRedraw`), para que esos fallos sean
    `ABORTED_BEFORE_WRITE`.
  - Redibujar todos los miembros como hoy y regenerar si alguno cambio.
  - `REDRAW_FAILED` con mensaje «rack redibujado en parte; ejecuta Actualizar».
  - Acotar INV-AUTH-1 a las vistas que I-55 crea y registrar la divergencia vigente del redibujo parcial (OM-3).

**AR2-03 (D-13) — Verificar la union de nombres de bloque no detecta piezas sin nombre.**
- **Evidencia:** la importacion descarta nombres en blanco (`P/Drawing/BlockLibraryImporter.cs:44-47`). `CreateSystemBlock` omite en
  silencio las instancias con nombre en blanco o ausente y solo las anota como faltantes (`P/Drawing/LateralHeaderDrawer.cs:293-303`).
  I-52 V11 §8.2 verifica por instancia.
- **Cambio:** re-verificar cada instancia que no sea anotacion ni cota (nombre no vacio y presente en la tabla de bloques) y lanzar
  dentro de la transaccion de ID19 si hay faltantes. Es una sola comprobacion compartida bajo X-4. ID17 e ID18 conservan su
  politica vigente (colocar y reportar).

**AR2-04 (D-17, X-2) — La resolucion sin editor no tiene autoridad ni gate asignados.**
- **Evidencia:**
  - Los handlers solo exponen `BuildBom` y `OutputBlockedReason` (`P/KindHandlers/IRackKindHandler.cs:50`, `:62`). La resolucion es
    privada en cada handler (`P/KindHandlers/DynamicKindHandler.cs:37-44`), y `KindHandlers/*` esta en el NO TOCAR de I-52 (V11 §20.3).
  - El camino del BOM acepta payloads Dinamicos legados que solo traen `DynamicSystem` (`P/KindHandlers/DynamicKindHandler.cs:40-42`),
    y `RACKEDITAR` los rechaza (`P/RackDinamicoCommands.cs:156-160`). ID19 proyectaria racks que no se pueden editar.
- **Cambio:**
  - Una sola autoridad en Application, `Resolve(payload, lectura del registro, catalogos) → sistema | fallo tipado` con los
    diagnosticos bloqueantes. La consumen los handlers del BOM, ID19 y el `Build` de I-52 (X-2), con archivo y gate asignados en el mapa.
  - Fallo cerrado si falta `DynamicDesign`.
  - Paridad definida como «resolver el payload persistido da el sistema que se dibujo al insertar».
  - Se registra H-14 fuera de I-55 (§5).

**AR2-05 (A-1, X-2) — El codec mezcla sintaxis y disponibilidad, y el mapa mete politicas en el codec.**
- **Evidencia:**
  - Varias filas de §7.3 dependen del diseno: `Section` en `[0, fondos)`, «corte existente», «estacion existente» y lado B en un
    Push Back de un solo sentido (`A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:43-50`; `P/RackPushBackCommands.cs:255-260`).
  - §12.1 aplica el codec en CLASSIFY, antes de RESOLVE.
  - El mapa G6 declara «politicas por consumidor» dentro de `RackViewEnvelopeCodec.cs` y prueba ahi la politica de ID19 y las huerfanas,
    en contra de §4.2 y de la decision 2 de ADR-0042.
- **Cambio:**
  - `Decode(kind, View, Section) → (direccion, disposicion)` solo sintactico.
  - Toda comprobacion «existe en el sistema resuelto» (fondo, corte, estacion, lado B) pasa a `RackViewSupport` (disponibilidad),
    y el lado B en un rack de un solo sentido se reclasifica como no disponible.
  - Las pruebas de politica de ID19 y de huerfanas pasan a G14.
  - Las filas dependientes del diseno se marcan en la caracterizacion compartida con la CT-04 de I-52.
  - El resultado de fallo cerrado no cambia.

**AR2-06 (X-2) — Falta fijar la forma de las API compartidas.**
- **Evidencia:**
  - El `RackViewPlanAuthority.Build(request)` de I-52 recibe el payload y resuelve dentro (V11 §8.1). Los editores de I-55 no
    re-resuelven (`P/RackSelectivoCommands.cs:421-424`).
  - La agrupacion lateral necesita el nombre de bloque antes del plan (`P/Drawing/LateralHeaderDrawService.cs:83`, `:244`), pero §4.1
    ordena plan → sobre → nombre.
  - V11 §6.1 declara `DescribeView(source, view, section) → ViewExposure{decodificacion §3.7, Expone, F, c, σ'}` y
    `AdmissibleViews → A_k` (V11 §3.8), y X-2 y X-8 de V2 no los nombran.
- **Cambio:**
  - Dos puntos de entrada: `Resolve(...)` (AR2-04) y `Plan(sistema, direccion, catalogos, nombre) → plan` con todas las direcciones.
    El `Build` de I-52 es su composicion, asi que C2-1 se conserva.
  - El nombre se calcula antes del plan.
  - `DescribeView` y `AdmissibleViews` entran en X-2 y X-8: comparten decodificacion, disponibilidad y tramo, y quedan como politica
    de I-52 `Expone`, `F`, la derivacion de `c` y el filtro de μ_k.
  - La salida de G7 del mapa («consumir la autoridad de I-52 y cubrir laterales») se reescribe.

### LOW

| # | Pregunta | Hallazgo | Cambio |
|---|---|---|---|
| AR2-07 | A-6 | El predicado de superposicion («`A_r · e_⊥` con mas de un valor») avisa sin haber superposicion: filas escalonadas o un rack girado 180° en la misma fila. Con OD-7.b = B rechazaria filas validas | Interseccion por pares de los intervalos proyectados sobre el eje comun; sigue sin depender de los puntos y en VALIDATE |
| AR2-08 | A-6, X-8 | El tramo es por variante, no por rack: la frontal de un fondo usa su propia cuadricula, mas corta (`A/Systems/Selective/SelectiveDepthLayout.cs:28-33`), y el origen D del Cantilever es interior a su extension, porque la columna ocupa y ≤ 0 y la base y ≥ 0 (`A/Systems/Cantilever/CantileverColumnBaseDatum.cs:19-24`, `:45`) | `RackViewFrame` expone `[K_min, K_max]` por (sistema, tipo, variante); `K*_s` y `K*_t` salen de su marco; convencion de extremos (eje de poste o cara) fijada en la caracterizacion unica |
| AR2-09 | A-6 | INV-GRP-1 se cumple por construccion aunque se invierta el orden (e = (0, −1) da 410/310/210 y sigue valiendo): es necesario pero no suficiente | Anadir INV-GRP-3: para todo par con `σ_s = σ_t`, `(target_r − target_q) · w(r) = (A_r(K*) − A_q(K*)) · e_K(r)` |
| AR2-10 | A-6 | §12.1 lista «anclas fuente» en PLANS, pero la proyeccion ortografica usa `B`; con OD-6.c = B falta el paso que pide el angulo | Independientes de los puntos: `A_r(K*)`, `e`, `α`, `ρ`, `a_t` y la superposicion; la proyeccion va en PLACE; paso de angulo junto a PICK si OD-6.c = B |
| AR2-11 | A-6, D-08f | El SNAPSHOT no enumera `IsDynamicBlock`, `IsAnonymous`, `Annotative` ni el payload del registro dinamico, que D-08f necesita; tampoco fija el signo de la escala Z: (−1,−1,+1) es giro de π, y con Z negativa es reflexion | Enumerarlos en §12.1 y en G15; (−1,−1,+1) acepta como π; escala Z negativa falla (alineado con ST-7, ST-8 y ST-10 de I-52) |
| AR2-12 | X-7 | El valor de colocacion solo aparece en G15, aunque G14 ya calcula colocaciones en Application | Consumo o creacion del valor de colocacion en G14, en `A/Geometry/` junto a `Transform2D`; la elevacion Z va fuera del valor 2D |
| AR2-13 | X-1 | La caracterizacion condicional de I-55 no se declara la CT-16 de I-52, que debe correr sobre codigo intacto antes de cualquier extraccion | Una sola caracterizacion = CT-16, ejecutada por el primer G3 que llegue |
| AR2-14 | A-3, OD-4 | Enter tambien detiene la cola: el jig acepta respuesta vacia y cancela con cualquier estado distinto de OK (`P/Drawing/BlockPlacement.cs:186-191`, `:219-229`) | Esc o Enter detienen la cola; mensaje y OV |
| AR2-15 | D-10 | La limpieza reutilizable borra solo definiciones sin referencias, en modo best effort y con registro (`P/Drawing/BlockPlacement.cs:130-169`), pero su purga de anidadas puede alcanzar definiciones de biblioteca que ya estaban sin referencias. En Cantilever, `definitionId` vive dentro del `try` (`P/RackCantileverCommands.cs:142-161`) | Especificar «solo sin referencias, best effort, registrado»; declarar el efecto de la purga; limpieza del Cantilever dentro de `PlaceDefinition` |
| AR2-16 | D-05, A-7 | «Id en blanco» se evalua distinto: `IsNullOrEmpty` en Selectivo y Cabecera (`P/RackSelectivoCommands.cs:119`; `P/RackCabeceraCommands.cs:233`) frente a `IsNullOrWhiteSpace` en I-54 (`A/CustomProperties/RackCustomPropertiesAuthority.cs:386`). `FindRackBlocks` solo excluye el `Id` vacio, no el formado por espacios, y casa sin distinguir mayusculas (`P/RackCommandSupport.cs:112`, `:124`); una fuente lateral o planta con `Id` en blanco no entra en el redibujo (`P/RackSelectivoCommands.cs:132-136`; `P/RackCabeceraCommands.cs:243-246`) | En blanco = `IsNullOrWhiteSpace` en I-55; fijar que hacen el gate y D-15 con un miembro que no se redibuja |
| AR2-17 | A-1 | La columna `RACKEDITAR` de §7.3 generaliza: Cantilever conserva el token `View` crudo (`P/RackCantileverCommands.cs:313-315` → `A/Persistence/RackEmbedComposer.cs:56`) y `RACKLISTA` lee `View` vacio como lateral en todos los kinds (`A/Persistence/RackListBuilder.cs:117-118`) | Columna por kind; «significado unico» solo para lectores de geometria |
| AR2-18 | A-4 | ADR-0042 descarta un «segundo enum de vista con mapeo» y conserva `CantileverViewKind`, que incluye `AdapterSection` (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:36`) | Aclarar que es un enum de camara del builder; registrar en la reconciliacion cuando aterriza el renombre frente a la instantanea de enums de I-52 |
| AR2-19 | X-6 | Si I-52 integra primero, re-establecer C-2 tras PR-2 obligaria a tocar su autoridad de plan Cantilever, fuera del «solo Plugin» de G5; sin un fixture con brazos y tensores visibles, H-02 no se ve (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:293-295`) | El `Plan` compartido de la planta Cantilever recibe `PlantaVisibility` del diseno (como la vista previa); fixture de C-2 con brazos y tensores visibles; G-M9 reapuntada tras G8 |
| AR2-20 | X-4 | El primitivo compartido no fija la elevacion (el valor 2D de X-7 no tiene Z), la presentacion opcional ni la politica de faltantes; `SystemBlockWriter.cs` esta en la lista STOP de I-52 | Parametros explicitos de Z y de presentacion (creacion o fuente) y comprobacion de faltantes por instancia (AR2-03); reporte de X-5 en G15 |

## 3. A-8 — Relacion entre ADR-0042 y ADR-0010

**Veredicto: (a) complemento + nota posterior fechada en ADR-0010.**

**Motivo.** El indice pide un ADR de reemplazo para **cambiar una decision** (`docs/adr/README.md:31-36`), pero ADR-0042 no revierte
ninguna decision de ADR-0010:

- ADR-0010 acota su propio alcance: «Esta decisión gobierna el flujo de edición. No cambia la inserción inicial que crea un rack nuevo
  ni promete capacidades multivista para todos los editores.» (`docs/adr/0010-actualizar-redibuja-insertar-liga-vistas.md:45-46`).
  - El contrato B de ADR-0042 (intencion de creacion aceptada) rige el flujo de **creacion**, que ADR-0010 deja fuera.
  - El contrato A (rack ya materializado) **es** la regla de ADR-0010, y conserva su proposito («para que disponga de diseño e
    identidad fuente», `:40-41`).
- ADR-0010 delega la sincronizacion previa de Insertar en «su flujo implementado» (`:36-37`). El gate de propiedades y D-15 son
  precondiciones de ese flujo; no cambian lo que significan Actualizar e Insertar.
- Hay precedente de nota posterior fechada que enlaza un ADR posterior en un aceptado no reemplazado (ADR-0005, 2026-07-27, sobre
  ADR-0021) y de complemento declarado (ADR-0028, ADR-0029).

**Consecuencias:**

- Mientras ADR-0042 no se acepte no se toca ADR-0010. Al aceptarse, se anade una entrada en «Notas posteriores» con fecha que lo
  enlace, y ADR-0010 sigue `aceptado`.
- La Proposal V3 ajusta la redaccion de ADR-0042: «complementa y precisa ADR-0010 sin revertir ninguna de sus decisiones» en lugar de
  «enmienda», con una seccion «Relacion con ADR-0010» que fija la opcion (a) y conserva la lista expresa de lo que sigue vigente.
- La opcion (b), reemplazo acotado, no hace falta. El reemplazo completo queda descartado.

## 4. Reconciliacion X-1..X-8 con I-52 (V11, contrastada con V12)

**Principio aplicado:** una autoridad por responsabilidad, extraida una sola vez, con politicas por llamador.
- «Extrae» = la primera iniciativa que llega a su gate de creacion, siempre sobre la especificacion compartida registrada aqui; la
  otra consume.
- «Custodio» = la iniciativa que mantiene esa especificacion.
- Ningun X presenta un **MATERIAL CONFLICT**: V11 deja expresamente el reparto a esta reconciliacion ([V11-D10]).

| X | Autoridad I-52 (V11) | Autoridad I-55 (V2) | Nucleo compartido | Politica I-52 | Politica I-55 | Custodio / extrae | Gate que la crea | Orden de consumo | Riesgo | VERDICT |
|---|---|---|---|---|---|---|---|---|---|---|
| **X-1** Seleccion | Nucleo neutral + fachada `RackDuplicationPlan` (§5.1); SNAPSHOT propio (§5.2) | D-08a consume el nucleo; SNAPSHOT propio (§12.1); D-08b..h | Clasificacion de fuentes de I-51 con predicado de kind inyectado, clave `RackId`/`Definition(handle)`, agrupacion y deduplicacion conservando referencias, consistencia de `Kind` y nombre, mensajes inyectados. Transformaciones, `Origin`, MINSERT, banderas y dependencia de xref quedan en el SNAPSHOT de cada llamador | ST-* (payload antes del filtro de Model Space; `Origin ≠ 0` falla; `Id` en blanco → clave de definicion) | Orden de I-51; `Id` en blanco falla (RID-4); kind sensible a mayusculas; sin asignador de identidad | Custodio I-52 (§5.1); extrae I-52 G4; I-55 G13 solo si I-52 no lo extrajo, con la misma especificacion | I-52 G4 (CT-16 en G3); I-55 G13 | CT-16 → I-52 G4 → I-52 G7 → I-55 G13, G14, G15 | MEDIO: precedencia y predicado de kind como politica; una sola caracterizacion (AR2-13) | **AGREED** |
| **X-2** Taxonomia, codec, resolucion y plan | `DescribeView`/`AdmissibleViews` (§6.1, §3.8); decodificacion §3.7/§8.4 (tablas en `Mirror/` segun §20.1); `RackViewPlanAuthority.Build(payload…)` (§8.1); CT-04 | `RackViewKind`; `RackViewEnvelopeCodec` + `RackViewSupport`; paso «sistema resuelto → plan» con todas las direcciones (§4.3); D-17 | En `A/Views` (nunca `Mirror/` ni `Systems/Shared`): taxonomia `RackViewKind`; `Decode` sintactico; disponibilidad sobre el sistema resuelto; `Resolve(payload, registro, catalogos) → sistema \| fallo tipado`, tambien para el BOM; `Plan(sistema, direccion, catalogos, nombre)` con laterales y planta Cantilever con `PlantaVisibility` | σ' canonica; `Coerced`/`Invalid`/no disponible falla; `Expone`, `F`, μ_k; `A_k` = disponible ∩ expuesto; `Build` = `Resolve` + `Plan` (C2-1 intacto) | `RACKEDITAR` sin cambio; ID19 acepta `Canonical`/`Canonicalizable` sin reescribir; editores llaman a `Plan` con su sistema | Custodio I-55 (necesita todas las direcciones); extrae la primera: I-55 G6/G7 o I-52 G5/G6 | I-55 G6 (taxonomia, codec, disponibilidad) y G7 (`Resolve`, `Plan`); I-52 G5 (`DescribeView`) y G6 (plan) | Caracterizacion unica (CT-04 = `RackViewEnvelopeReadingCharacterizationTests`) → creador → I-55 G7..G14 / I-52 G5..G7; el renombre se reporta si la instantanea de enums de I-52 ya existe | MEDIO: I-52 debe ajustar §6.1, §8.1, §8.4 y §20.1 antes de su freeze | **AGREED** |
| **X-3** Comparador authored | `IsSameAuthority` por kind declarado por el reflector (§5.4, §6.1); Selectivo `SelectiveAuthoredAuthority.IsSameAuthority` | `RackSiblingAuthoredGate` (§9.3): Selectivo `Resolve`, que usa `IsSameAuthority`; resto con el comparador de X-3 | `IsSameAuthority(miembros)` por kind en el contrato del kind (fuera del reflector del espejo), excluyendo la metadata de `P/RackCommandSupport.cs:40-67` | Miembros = vistas seleccionadas; E3 | Miembros = todas las hermanas; falla antes de PICK | Custodio I-52 (§5.4); extrae la primera: I-52 G5 o I-55 G14 | I-52 G5a/b/c; I-55 G14 | Creador → el otro delega; el reflector solo lo referencia | MEDIO: I-52 mueve el comparador fuera de `IRackDesignReflector` | **AGREED** |
| **X-4** Primitivo de materializacion | `CreateInTransaction` + `CreateReference` y PREPARE con importacion y verificacion por instancia (§8.2); contexto §8.7 | «Definicion + sobre + referencia con rotacion en la transaccion del llamador» (§12.9; mapa G15) | Despacho por familia de plan (`CreateSystemBlock` / `CantileverViewMaterializer`) + `RackBlockData.Write` + referencia en Model Space con Z y presentacion explicitas; sin lock, commit, regeneracion ni purga; faltantes por instancia (AR2-03) | ST-13, ST-19, PRE-17, PRE-18, `MaterializationContextReadSet` y huellas: todo en el llamador | Presentacion de creacion (OD-8 A), `Z_r`, escala 1, puntos antes de importar | Custodio I-52; extrae I-52 G6; I-55 G15 solo si llega antes, con la firma acordada | I-52 G6; I-55 G15 | I-52 G6 → I-52 G7 → I-55 G15 | MEDIO: Z, presentacion opcional y politica de faltantes; `SystemBlockWriter.cs` en STOP de I-52 (X-5); G-M24 con modo de creacion | **AGREED** |
| **X-5** Protocolo de archivos y autoridades | §15.2, §15.3, §19: reporte y STOP simetrico | Reporte previo en G5, G6, G7, G8, G9, G10, G12, G13, G14 y G15 | Sin autoridad: protocolo de reporte antes de fijar archivos | Reevaluar §8.1, §8.7, read-set, PRE-17, PRE-18, T-GRD-02, censo `B`; G4 avisa a I-55 | Reporte con la lista de reevaluacion; nada congela una autoridad incompatible | — | — | Cada gate listado reporta antes de fijar archivos | BAJO | **AGREED** |
| **X-6** Lineas base de C-2 | C2-1 = C2-2a = C2-2b = C2-3 = C2-4 y G-M9, verdes antes de cerrar su G6 (§8.5) | Cambia productores: G5 (PR-2), G8 (re-enrutado de Insertar), G9 (gate y D-15), G12 (orden de edicion) | Un juego de fixtures y un mapa G-M9 sobre el `main` combinado | C-2 por defecto; C-1 solo condicional | Goldens de G7 y fixtures de Insertar de G8; PR-2 solo en el Plugin | Custodio I-52; la segunda en integrar re-establece C-2 | I-52 G6; re-establecimiento en los gates de la segunda | La segunda corre C-2 y G-M9 antes de su Candidato | ALTO: sin el `Plan` compartido con `PlantaVisibility` (AR2-19), PR-2 obligaria a tocar Application; fixture con brazos y tensores visibles | **AGREED** |
| **X-7** Valor de colocacion | `Transform2D.ReflectionAboutLine` + `(Position2D, RotationRadians, UniformScale)` con composicion y descomposicion (§3.3, §8.3) | `CommonTransform2D` sobre `Transform2D`; colocaciones (Position, ρ, 1) | `Transform2D` + un valor de colocacion; `T_common` = (T − R(α)·B, α, 1); `Rigid` como composicion `Then`; `Orthographic` = (ancla − R(ρ)·a_t, ρ, 1) | Reflexion; escala de la fuente; Z de la fuente | Rigidez (`det = +1`, escala 1); Z y `Origin` fuera del valor | Custodio I-52 (`A/Geometry/`); extrae I-52 G4; I-55 G14 solo si llega antes | I-52 G4 (T-M01, T-M02); I-55 G14 (AR2-12) | Esperado I-52 G4 → I-55 G14 (no garantizado) | BAJO-MEDIO: el valor no lleva `Origin` ni Z | **AGREED** |
| **X-8** Origen y tramo del eje | `ViewExposure.c` (§6.1) y «tramo del eje de la vista (para c)» en `ViewPlanResult` (§8.1); CT-05, CT-06 | `RackViewFrame`: ejes con signo, origen fisico, extension (§12.3); `RackViewFrameCharacterizationTests` | Un descriptor por (sistema, tipo, variante) desde el sistema resuelto: mapa de ejes con signo, origen fisico local, `[K_min, K_max]` por eje (AR2-08) y convencion de extremos | `c` = centro del tramo del eje reflejado; desplazamiento δ registrado (CT-06), nunca ajustado; solo vistas admisibles | Anclas en `K_min`/`K_max` segun σ; par inconsistente fuera de ID19 | Custodio I-55 (laterales y signos); extrae la primera | I-55 G6 (consumo en G14); I-52 G5 (`c`) o G6 (`ViewPlanResult`) | Caracterizacion unica (CT-05 = `RackViewFrameCharacterizationTests`) → creador → I-52 G5/G6 e I-55 G14 | BAJO-MEDIO: tramo por vista frente a extension del rack | **AGREED** |

**Requisitos del lado de I-52 (a registrar por su Coordinador y su Arquitecto antes de su freeze).**
- §5.4 y §6.1: el comparador authored vive en el contrato del kind y el reflector lo referencia.
- §6.1 `DescribeView`: consume la decodificacion sintactica, la disponibilidad y el descriptor de X-8.
- §8.1: `Build` = `Resolve` + `Plan`, con `Plan` cubriendo todas las direcciones; la resolucion la comparten los handlers del BOM.
- §8.4 y §20.1: el nucleo de decodificacion va en `A/Views`, y `Mirror/` conserva `A_k` y la exposicion.
- §8.2: primitivo con Z y presentacion explicitas y verificacion de faltantes por instancia.
- C-2: fixture de planta Cantilever con brazos y tensores visibles.
- Caracterizaciones unicas: CT-04, CT-05 y CT-16.

**Estado de la reconciliacion:** registrada y acordada del lado de I-55. La reconciliacion obligatoria de [V11-D10] queda completa
cuando I-52 la adopte en sus propios registros; hasta entonces ninguna de las dos iniciativas congela.

### 4.1 Contraste con la Proposal V12 de I-52 (`915a520`)

I-52 publico su Proposal V12 mientras se cerraba esta revision (solo documentacion). Se contrastaron las secciones de la tabla y
ninguna cambia de numero.

**Que cambia.** [V12-D11] marca **PROVISIONAL UNTIL CROSS-INITIATIVE RECONCILIATION** las partes de fundacion compartida con I-55:
- §3.7 y §3.8 (taxonomia, codec y `A_k`);
- §5.1 (nucleo de seleccion);
- §8.1 (plan desde el sistema resuelto, resolucion previa y origen y tramo del eje);
- §8.2 (primitivo);
- §8.3 (valor de colocacion);
- §8.4 (codec: su ubicacion ya no se fija, y `Mirror/` solo aloja lo propio del espejo);
- §8.5 (comparador authored y lineas base de C-2);
- §20.1.

Segun [V12-D11], I-52 sola no congela propiedad, namespace ni ubicacion, y la semantica que necesita puede acordarse.
[V12-D17] registra la Proposal V2 de I-55, ADR-0042 V2 como complemento de ADR-0010 y los fixtures de C2-2b con las precondiciones
nuevas de Insertar.

**Que no cambia.** §3.2, §3.3, §5.2, §5.4 y §6.1 son identicas a V11.

**Efecto:**
- Ningun veredicto de X-1..X-8 cambia, y V12 confirma que el reparto se decide en esta reconciliacion.
- Los requisitos del lado de I-52 se reducen, porque la contradiccion entre §8.4 y §20.1 y la ubicacion en `Mirror/` ya estan
  resueltas en V12.
- Siguen pendientes de adoptar por I-52:
  - el comparador en el contrato del kind (§5.4 y §6.1 lo siguen declarando en el reflector);
  - `Build` = `Resolve` + `Plan`;
  - el primitivo con Z, presentacion explicita y faltantes por instancia;
  - el fixture de planta Cantilever con visibilidad;
  - las caracterizaciones unicas CT-04, CT-05 y CT-16.

## 5. Hallazgo fuera de I-55

**H-14 — El editor Dinamico no es idempotente al releer la tarima.**
- **Evidencia:**
  - `Num` formatea con `"0.##"` (`U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:3447-3450`).
  - El fondo de la tarima se escribe en la caja de texto (`:3273`) y se relee (`:1409`).
  - `MustRebuild` compara tarimas con tolerancia 1e-6 (`A/Systems/Dynamic/DynamicEditorDesignAssembler.cs:44-61`).
- **Consecuencia:** un fondo persistido con mas de dos decimales puede reconstruir los modulos en un Actualizar sin cambios.
- **Estado:** sin validar en AutoCAD; I-55 no lo corrige. Candidato a registro o iniciativa aparte.

## 6. Resultado

| Punto | Estado |
|---|---|
| V2 (`f84f303`) | Sigue siendo el SHA revisado y el registro historico. **No vale como SHA de freeze**: requiere los cambios AR2-01..AR2-20 |
| Proposal V3 | **Requerida** (formula normativa, invariante nuevo, maquina de estados de ID18, alcance de gates, redaccion de ADR-0042 segun A-8 y contrato de reconciliacion); se revisa de nuevo por Coordinator y Architect sobre su SHA exacto |
| Open Material | M-01 (decision del Owner pendiente; el Coordinador recomienda A); AR2-01..AR2-06; registro de la reconciliacion por I-52 |
| Open Minor | OM-1..OM-10, OM-12..OM-15, OM-17..OM-19 de V2; AR2-07..AR2-20 (se resuelven en V3); H-14 fuera de I-55 |
| Siguiente gate | G2D — Proposal V3, solo documentacion, por orden del Coordinador; conviene abrirlo con la decision del Owner sobre M-01/OD-1..OD-8 para que V3 sea la unica version a consensuar |

```text
Architect      = CHANGES REQUIRED — PROPOSAL V3
Coordinator    = NOT AGREED (M-01 y X-1..X-8 abiertas al emitir G2C)
Owner          = PENDING (M-01, OD-1..OD-8, ADR-0042)
Consensus      = NOT REACHED
Implementation = BLOCKED
```

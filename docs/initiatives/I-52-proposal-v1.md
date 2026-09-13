# I-52 — Proposal V1: RACKMIRROR, espejo semantico de uno o varios racks (ID16)

> # ⚠ PROPOSAL V1 — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```text
> Proposal Version = V1
> Coordinator      = REVIEW REQUIRED
> Architect        = REVIEW REQUIRED
> Implementation   = BLOCKED
> ADR              = ADR-0036 propuesto (docs/adr/0036-rackmirror-espejo-semantico-por-copia.md)
> Schema           = SIN CAMBIO (ningun DTO, sobre ni Xrecord cambia en I-52)
> ```
>
> ```text
> BASE productiva  = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main al abrir G2; no avanzo)
> CLAIM_SHA        = 55281769d5e9317ad25500e6d3f5f8a849f37279
> BOOTSTRAP_SHA    = 7ee79756d2b93b4eded660cb73310ef3edde32ff
> Discovery G1     = 339b3abd238a3a7a42d60c9edd45a1500a776f62   (I-52-discovery.md; CI 34728658236 success)
> Revision Arq. G1 = favorable a redactar Proposal V1 (sin BLOCKER; H-1..H-6 y M-1..M-7 obligatorios),
>                    aceptada por el Coordinador para redactar V1 (veredicto literal en decisions/I-52.md §3.1)
>                    (registro: docs/automation/decisions/I-52.md §3)
> Paralelas        = §15 (tips observados al abrir G2)
> Estado de gates  = G0 ACCEPTED · G1 ACCEPTED (Discovery) · G2 V1 EN REVISION · G3+ NO INICIADO
> ```
>
> Este documento **no** autoriza produccion, **no** modifica el Discovery y **no** es el contrato vinculante: el
> contrato se reescribe en el freeze de G2, cuando Coordinador y Arquitecto acuerden la **misma** version y el Owner
> acepte el ADR.

---

## 0. Reconciliacion Discovery → Proposal V1

El Discovery (`339b3ab`) **se conserva sin cambios** como evidencia de G1. Esta seccion registra que afirmaciones
suyas corrige V1 y donde queda la version vigente. **Donde el Discovery y esta Proposal difieran, manda V1.**

| Marca | Hallazgo | Afirmacion de G1 | Correccion V1 | Donde |
|---|---|---|---|---|
| `[V1-D1]` | H-1 | §8.3 y §9: la misma linea implica reflexiones distintas por vista; la matriz evalua ambos ejes (RUN y DEPTH; RT y RD; R_X y R_Y; R_D y R_F) | Hay **una reflexion semantica canonica `μ_k` por kind**, independiente de la linea y de las vistas elegidas. Cada vista declara si **expone** `μ_k`; las referencias que no la exponen **fallan cerrado**. **Sin prompt**. Las filas que solo existian bajo el eje no canonico dejan de ser riesgo | §3, §7 |
| `[V1-D2]` | H-2 | §17.4: «no leer ni escribir el registro» | El espejo **regenera** geometria y el literal congelado **no** es el valor efectivo (`SelectiveEffectiveDesignResolver.cs:85`; `SelectiveEffectiveResolution.cs:37`; `LinkedPropertyReconciler.cs:85`). El espejo **lee** el registro y consume la autoridad efectiva **en solo lectura**; nunca escribe, crea, desvincula ni materializa | §10 |
| `[V1-D3]` | H-3 | §8.3: el conflicto se trata como pregunta abierta; no se nota que el Dinamico obliga a insertar primero la lateral | Las laterales de Selectivo, Dinamico, Push Back y Cantilever y la vista unica de la cama **no exponen** `μ_k` ⇒ fail-closed en el primer corte. El Dinamico **exige** la lateral como primera vista (`RackDynamicSystemWindow.xaml.cs:2877-2885`), asi que un Dinamico dibujado solo en lateral no puede reflejarse. No se crea «vista desde el lado opuesto» | §2, §3.7 |
| `[V1-D4]` | H-4 | §15.2: «no existe regeneracion neutral»; no cita la costura existente | Existe la costura PREPARE/MUTATE de I-47 G9.1 (`ViewBlockDraw.cs:59-109`; `SystemBlockWriter.cs:104-116`; `PreparedViewRedraw.cs`), primitivas de crear con transaccion del llamador (`LateralHeaderDrawer.CreateSystemBlock`, `LateralHeaderDrawer.cs:28`; `CantileverViewMaterializer.CreateBlockDefinition`, `:36-58`) y un ejecutor multi-rack atomico **solo Selectivo** con ramas `View`/`Section` dentro del Plugin (`ProjectVariableMutationExecutor.cs:142-304`). V1 las generaliza sin anadir ramas por kind al Plugin | §8 |
| `[V1-D5]` | H-5 | §19 U-17: Paper Space, MINSERT, `Normal` y UCS como riesgos sin contrato | Contrato obligatorio de transformacion fuente y de linea | §4 |
| `[V1-D6]` | H-6 | §22 CT-16: conmutacion sin definicion ejecutable ni politica de mano | Equivalencia ejecutable bajo reflexion y politica de mano/simetria de bloques; ningun bloque DWG se asume simetrico | §11 |
| `[V1-D7]` | M-1 | §9.1 S-14: «swap + indice derivado» bajo los dos ejes, con la reversion de columnas marcada como dependiente de la geometria | El indice de `DesviadorOffCells` del Selectivo es **propio y biyectivo** por poste cargado, incluidos los intermedios (`SelectiveDesviadorPlan.cs:132-148`); **no** usa la clave `Math.Min` del Dinamico (`:28-31`). Bajo RUN la fila es **REPRESENTABLE**. *Precision de transparencia:* la revision de Arquitecto (M-1) describio esto como una confusion de G1 con el `Math.Min`; el texto del Discovery no afirmaba esa confusion, solo sobrestimaba el riesgo | §7.1 S-14b, §7.2 D-10 |
| `[V1-D8]` | M-1 | §9.1 S-01: «**NR** en RUN» sin causa verificada | La causa es que la alineacion al final no es expresable. Ademas, el editor **rellena y avisa**: cuenta `paddedEmptyFrentes` para advertir en vez de convertir en silencio (`SelectiveEditorState.cs:308-315`), asi que rellenar con frentes vacios no es normalizacion valida (anade postes). *Precision de transparencia:* la lectura errónea sobre el relleno venia de una auditoria interna de G1, no del texto del Discovery | §7.1 S-01 |
| `[V1-D9]` | H-1 | §9: S-09, S-13, K-06 y PC-01..PC-08 como NR o riesgo | Bajo la `μ_k` canonica son **REPRESENTABLE** (falsos NR) | §7 |
| `[V1-D10]` | H-6 | §9.6 K-08: la mano del brazo solo falla bajo R_Y | Tambien falla bajo **R_X**: el eje X del marco del brazo lo fija el lado (`CantileverArmFrameResolver.cs:88-123`) | §7.5 K-08 |
| `[V1-D11]` | M-2 | §5.1: mensajes con «duplicar» como detalle menor | Los mensajes son parte de la fachada; el nucleo neutral lleva vocabulario inyectado y `RackDuplicationPlan` conserva los suyos | §5.1 |
| `[V1-D12]` | M-3 | §5.1: consistencia authored solo Selectivo | RACKMIRROR exige autoridad authored por kind para **todos** los kinds, sin cambiar RACKDUPLICAR | §5.3 |
| `[V1-D13]` | M-4 | §7.3: codificacion de `Section` descrita sin ubicar su autoridad | La decodificacion vive hoy en Plugin (`RackSelectivoCommands.cs:166-217`; `RackDinamicoCommands.cs:393`; `ProjectVariableMutationExecutor.cs:181-217`) y en Application (`PushBackSystemFrontalBuilder.cs:134-155`; `CantileverViewPlanBuilder.cs:237-239`). V1 la concentra en Application por kind | §8.4 |
| `[V1-D14]` | M-5 | §15: importacion no tratada | `BlockLibraryImporter.EnsureForPlan` abre su propia transaccion y debe llamarse **fuera** de la del dibujo (`BlockLibraryImporter.cs:12-33`). Es la unica excepcion declarada a la atomicidad semantica | §9 |
| `[V1-D15]` | M-6 | §23.3 PDC-2 abierto | Nombre fijo `<base> - espejo`, **sin ordinal** | §1 |
| `[V1-D16]` | M-7 | §14.3: perdida de campos anidados en general | La cabecera re-serializa por dominio sin `ExtensionData` y reinicia version (confirmado por I-54 §8.2): deuda conocida; I-52 no la empeora | §10.4 |
| `[V1-D17]` | L-4 | §24.1: `PlacedClone` conserva rotacion | Ninguna colocacion ni equivalencia de I-52 usa `HeaderRunPlan.Flatten`/`PlacedClone` | §11.2 |
| `[V1-D18]` | — | §18.1: I-50 @ `a4806ff` | I-50 avanzo a `ed50cbd` y ahora toca tambien `PushBackCompositeStructure.cs` y `RackSelectiveWindow.xaml(.cs)`; I-49 @ `ccf21c6` (V4); I-53 e I-54 publicaron Proposal V1 | §15 |

---

## 1. Decisiones de producto V1

Fijadas por el Coordinador en la orden de G2 (registro: `docs/automation/decisions/I-52.md` §4). Vinculan V1;
**no** son consenso final.

| # | Decision |
|---|---|
| **PDC-1** | **Solo copia.** Crea copia reflejada, conserva **todos** los originales, nunca borra, nunca refleja en sitio, nunca conserva el `RackId` en la copia. **Sin** pregunta «¿Borrar objetos originales?» |
| **PDC-2** | Nombre logico por rack `<base> - espejo`, **sin ordinal**. `Name` ≠ `RackId`. Los nombres de `BlockTableRecord` siguen la politica de unicidad existente de cada familia de plan |
| **PDC-3** | Autoridad = referencias **fisicamente seleccionadas**. Sin buscar hermanas para anadirlas; sin crear vistas no seleccionadas. Vistas seleccionadas del mismo `RackId` = **un** grupo con **un** `NewRackId`. Una referencia no admisible **falla toda la operacion** antes de MUTATE |
| **PDC-4** | Sin prompt de eje semantico: ni Frentes/Fondos, ni RUN/DEPTH, ni A/B. El usuario solo da la linea (dos puntos) |
| **PDC-5** | Indices, numeros y letras derivados se **regeneran** desde el documento reflejado; no se conserva una etiqueta por su posicion grafica |
| **PDC-6** | Protector lateral Left/Right: **UNKNOWN** donde sea ambiguo. Estado explicito cuya reflexion no este demostrada ⇒ fail-closed. No se inventa interpretacion |
| **PDC-7** | Clases normativas `REPRESENTABLE`, `REPRESENTABLE_BY_NORMALIZATION`, `REQUIRES_MODEL_CHANGE`, `UNKNOWN`, evaluadas **por rack concreto** en PREFLIGHT. `REQUIRES_MODEL_CHANGE` ⇒ fail-closed; `UNKNOWN` material ⇒ fail-closed |
| **PDC-8** | Unico comando nuevo `RACKMIRROR`, **sin alias**. Censo de `[CommandMethod(` **33 → 34** |
| **PDC-9** | Solo Model Space; MINSERT ⇒ fail-closed; linea por dos puntos UCS→WCS; contrato de fuente de §4 |
| **PDC-10** | Arquitectura registrada para los seis kinds con bloque; el primer corte solo ejecuta vistas que exponen `μ_k`; la cama falla cerrado; sin ampliar schema |

## 2. Alcance del primer corte

### 2.1 Dentro

| Kind (`Kind` del sobre) | `μ_k` canonica | Vistas admitidas | Evidencia de la vista |
|---|---|---|---|
| Selectivo (`selective`) | **RUN**: invierte frentes/postes | frontal (`Section` = fondo), planta | frontal X = run (`SelectivePostGeometry.cs:32-40`); planta X = fondo, Y = frente (`SelectivePlantaBuilder.cs:15-22`) |
| Dinamico (`dynamic`) | **RT**: invierte frentes | frontal (`Section` 0 salida / 1 entrada), planta | `RackDinamicoCommands.cs:393`; `DynamicSystemFrontalBuilder`/`DynamicSystemPlantaBuilder` |
| Push Back simple y compuesto (`pushback`) | **RT** | frontal (`Section` 0..3), planta | `PushBackSystemFrontalBuilder.cs:134-155` |
| Cantilever (`cantilever`) | **R_X**: invierte la linea de estaciones | frontal, planta | camaras `CantileverViewPlanBuilder.cs:205-235` |
| Cabecera independiente (`cabecera`) | **R_D**: invierte la profundidad del marco | lateral, planta | `LateralHeaderLayoutBuilder.cs:49-59`; `PlantaHeaderLayoutBuilder.cs:13-21` |

### 2.2 Fuera (fail-closed o no aplica)

- Laterales de Selectivo, Dinamico, Push Back y Cantilever; la cama (`cama`) entera.
- Diseños clasificados `REQUIRES_MODEL_CHANGE` o `UNKNOWN` material (§7).
- Espejo en sitio, borrar originales, conservar `RackId`, prompt de eje, alias.
- «Vista observada desde el lado opuesto» (cambio de modelo; futuro, §17).
- Paper Space, MINSERT, fuentes no canonicas (§4).
- Cambios de schema, de `RackEnvelopeRestamp.cs`, de `PushBackMirror.cs`, de `WithDesign` y del registro de variables.
- Drive-In (no existe: `RackMainMenuWindow.xaml:139-144`) y Larguero (sin bloque RackCad).
- Arreglar los hallazgos laterales del Discovery §24: se registran en `ideas-futuras.md` al cerrar G2.

---

## 3. Modelo formal normativo

### 3.1 Definiciones

Transformaciones afines del plano XY de WCS: `T(v)` traslacion, `R(α)` rotacion antihoraria, `S(a,b)` escala.

```text
G        = reflexion de la hoja respecto de la linea del usuario ℓ = (q, φ)
         = T(q) · R(2φ) · S(1,−1) · T(−q)                               det G = −1
P        = colocacion canonica de la referencia fuente = T(p) · R(θ) · S(s,s),  s > 0     det P = +s²
D        = documento authored del rack (payload interior del sobre)
μ_k      = reflexion semantica canonica del kind k: documento → documento
Π_(V,σ)  = plan fisico de la vista V con seccion σ, en coordenadas locales de la definicion
           (piezas fisicas; las anotaciones y cotas se tratan aparte, §11)
F        = reflexion local de la vista que expone μ_k:
           X_c = T(2c,0)·S(−1,1)   (x → 2c − x)      Y_c = T(0,2c)·S(1,−1)   (y → 2c − y)
σ'       = τ_(k,V)(σ), seccion destino
```

### 3.2 Exposicion y colocacion

```text
EXPOSICION    (V,σ) expone μ_k  ⇔  Π_(V,σ')(μ_k D) ≡ F ∘ Π_(V,σ)(D)          (≡ de §11)

COLOCACION    P' = G · P · F                    det P' = (−1)(+s²)(−1) = +s²

              F = X_c  ⇒  parte lineal de P' = R(2φ − θ + π) · S(s,s)
              F = Y_c  ⇒  parte lineal de P' = R(2φ − θ)     · S(s,s)

TEOREMA       para (V,σ) admitida:  P' · Π_(V,σ')(μ_k D)  =  G · P · Π_(V,σ)(D)
```

**Consecuencias normativas.**

1. La geometria fisica de cada vista admitida coincide con el espejo geometrico de la hoja.
2. Los textos y las cotas **se regeneran** desde `μ_k D` y quedan legibles (principio de ADR-0031 §8).
3. La colocacion final tiene determinante positivo y la escala uniforme de la fuente. **Nunca** se persiste escala espejo.
4. `μ_k` **no** depende de `ℓ`: el documento reflejado es el mismo para cualquier linea y cualquier subconjunto de
   vistas. Por eso puede calcularse y verificarse **antes** de pedir la linea (§9).

### 3.3 Composicion con `Transform2D`

`Transform2D.Then` lee de izquierda a derecha (`Transform2D.cs:85-93`): `A.Then(B)` aplica `A` y luego `B`.

```text
P  = Transform2D.Scale(s).Then(Transform2D.Rotation(θ)).Then(Transform2D.Translation(p))
X_c = Transform2D.MirrorAboutY.Then(Transform2D.Translation(2c, 0))
Y_c = Transform2D.MirrorAboutX.Then(Transform2D.Translation(0, 2c))
G  = ReflectionAboutLine(a, b)                     // fabrica nueva, §8.3 y G4
P' = F.Then(P).Then(G)                             // aplica F, luego P, luego G  ≡  G · P · F
```

La descomposicion de `P'` en `(posicion, rotacion, escala)` exige: determinante positivo, parte lineal = rotacion ×
escala uniforme (sin cizalla) dentro de `GeometryTolerance.Length`/`Angle` (`Vector2D.cs:90-100`), y escala igual a la
de la fuente. Si falla: error de programacion, no clasificacion.

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
no puede dibujar una elevacion «vista desde el lado opuesto». Bajo la **admisibilidad estricta** de V1 (§3.8), la
lateral no expone `μ_RUN` y **falla cerrado**.

### 3.6 Reflexion canonica por kind

| Kind | `μ_k` | Por que ese eje |
|---|---|---|
| Selectivo | RUN | Es el eje de la frontal, que el editor obliga a insertar primero (`RackSelectiveWindow.xaml.cs:2083-2091`); conserva «fondo 0 = frente» y evita los casos no representables de profundidad (topes, herencias, diagonales) |
| Dinamico | RT | La salida esta fijada en X=0 (`DynamicLoadBeamGeometry.cs:166-190`): la reflexion de profundidad no es almacenable |
| Push Back simple y compuesto | RT | El lado A siempre existe y la fisica asume flujo hacia +X (`PushBackElevations.cs:166-177`); bajo RT **no** se intercambian A y B |
| Cantilever | R_X | No toca los lados ±Y ni el traslape derivado (`CantileverLineResolver.cs:271-277`) |
| Cabecera | R_D | Su elevacion principal es la lateral (eje profundidad); R_F no produciria cambio visible |
| Cama | ninguna | Ningun campo persistido expresa el lado del tope (`FlowBedLateralBuilder.cs:34-66`) |

### 3.7 Exposicion por vista (primer corte)

`c` es el centro del **tramo del eje reflejado en el modelo** —no la caja de la vista con anotaciones— y lo declara
el kind a partir de la geometria resuelta del documento **fuente**. G3 caracteriza los valores exactos.

| Kind | View | `Section` | Eje X de la vista | Expone | `F` | `c` | `σ'` |
|---|---|---|---|---|---|---|---|
| selective | frontal | fondo `k` (−1 ⇒ 0) | RUN | **si** | `X_c` | ultimo poste del fondo / 2 | `k` |
| selective | planta | −1 | X = fondo, Y = RUN | **si** | `Y_c` | ultimo poste del grid maestro / 2 | −1 |
| selective | lateral | poste `p` | profundidad | no | — | — | — |
| dynamic | frontal | 0 / 1 | frentes | **si** | `X_c` | ultimo poste / 2 | igual |
| dynamic | planta | −1 | X = profundidad, Y = frentes | **si** | `Y_c` | ultimo poste / 2 | −1 |
| dynamic | lateral | poste `p` | profundidad | no | — | — | — |
| pushback | frontal | 0..3 | frentes (reticula compartida) | **si** | `X_c` | ultimo poste / 2 | igual |
| pushback | planta | −1 | X = profundidad, Y = frentes | **si** | `Y_c` | ultimo poste / 2 | −1 |
| pushback | lateral | poste `p` | profundidad | no | — | — | — |
| cantilever | frontal | −1 | −X | **si** | `X_c` | −(N−1)·s/2 | −1 |
| cantilever | planta | −1 | (−X, +Y) | **si** | `X_c` | −(N−1)·s/2 | −1 |
| cantilever | lateral | estacion `k` | −Y | no | — | — | — |
| cabecera | lateral | −1 | profundidad (poste izquierdo en 0) | **si** | `X_c` | `Depth`/2 | −1 |
| cabecera | planta | −1 | X = fondo | **si** | `X_c` | `Depth`/2 | −1 |
| cama | (lateral) | −1 | riel | no | — | — | — |

En todas las vistas admitidas `τ` es la identidad: **no hay remapeo de `Section`** en el primer corte.

### 3.8 Regla de admisibilidad estricta

```text
Para cada referencia seleccionada r de un grupo con kind k:
  si no existe reflector registrado para k                      ⇒ FALLO (E2)
  si (View(r), Section(r)) no expone μ_k                        ⇒ FALLO de TODA la operacion (E5)
No hay interseccion de ejes: μ_k es unica por kind.
```

### 3.9 Alternativas registradas, no adoptadas

- **ALT-F — fidelidad fisica:** admitir las vistas que no exponen `μ_k` con `P' = G·P·X_c`: se refleja la huella y el
  contenido no se voltea. Es fisicamente correcto pero graficamente distinto de MIRROR. Requiere decision del Owner.
- **ALT-L — «vista desde el lado opuesto»:** propiedad de vista en el sobre que el builder respete con anotaciones
  legibles. Cambio de modelo con su propio ADR. Es la via para laterales, la vista primaria del Dinamico y la cama.

---

## 4. Contrato de transformacion fuente y de linea

### 4.1 Fuente (por referencia seleccionada, en SNAPSHOT/PREFLIGHT)

| # | Condicion | Resultado |
|---|---|---|
| ST-1 | Objeto no `BlockReference` | ignorar + aviso (clasificacion de I-51) |
| ST-2 | `BlockReference` sin payload RackCad | ignorar + aviso |
| ST-3 | `MInsertBlock` (subclase de `BlockReference`) | **fail-closed** de la operacion |
| ST-4 | Fuera de Model Space (`OwnerId` ≠ Model Space) | ignorar + aviso (PD-7 de I-51) |
| ST-5 | `Normal` fuera de +Z dentro de `GeometryTolerance.Angle` | **fail-closed** |
| ST-6 | `Rotation` no finita | **fail-closed** |
| ST-7 | valores absolutos de `sx`, `sy` y `sz` distintos dentro de tolerancia relativa (escala no uniforme) | **fail-closed** |
| ST-8 | `sx < 0` y `sy < 0` con `sz > 0` | se canoniza a `(s,s,s)` con `θ + π` |
| ST-9 | exactamente una de `sx`, `sy` negativa (MIRROR nativo, semilla espejada de RACKLAYOUT o RACKDUPLICAR) | **fail-closed** |
| ST-10 | `sz < 0` | **fail-closed** |
| ST-11 | escala uniforme positiva `s ≠ 1` | admitida; `P'` conserva `s` |
| ST-12 | Z de `Position` | se conserva (el plano de reflexion es vertical) |
| ST-13 | Capa | la referencia nueva usa la capa de **su** referencia fuente (como I-51) |

### 4.2 Linea

| # | Condicion | Resultado |
|---|---|---|
| LN-1 | Dos puntos con `GetPoint`, en UCS | se convierten a WCS con `CurrentUserCoordinateSystem` (patron de `RackDuplicarCommands.cs:145`) |
| LN-2 | Eje Z del UCS no paralelo a Z de WCS | fin sin mutacion |
| LN-3 | Puntos coincidentes tras proyectar a XY | fin sin mutacion (se puede repetir la peticion) |
| LN-4 | Enter/Esc | fin sin mutacion |

La conversion UCS→WCS y el uso de `Matrix3d` viven **solo** en el Plugin. Application recibe dos `Point2D` de WCS.

---

## 5. Seleccion, agrupacion e identidad (reutilizacion de I-51)

### 5.1 Nucleo neutral + fachada compatible

Se extrae de `RackDuplicationPlan` (`RackDuplicationPlan.cs:225-582`) **solo** lo neutral:

- clasificacion de fuentes (cuatro clases de I-51);
- clave logica discriminada `RackId` / `Definition(handle)`;
- agrupacion y deduplicacion de definiciones conservando todas las referencias;
- consistencia de `Kind` y nombre del grupo.

| Pieza | Regla |
|---|---|
| Nucleo neutral (Application, puro) | Vocabulario de mensajes **inyectado**. Sin nombre de copia, sin `Guid`, sin tipos de AutoCAD |
| `RackDuplicationPlan` | **Fachada**: misma API publica, mismos textos, misma politica de nombre «- copia», misma autoridad Selectiva. T1–T13 **sin cambios**. Una prueba nueva fija la igualdad de mensajes de la fachada con los historicos |
| Asignador de identidad | Se reutiliza su validacion (`Guid.Empty`, repeticion, igualdad con una fuente) con **politica de nombre inyectada**. RACKDUPLICAR pasa la historica; RACKMIRROR pasa `<base> - espejo` y un unico destino |
| Clone path de RACKDUPLICAR (`RackCloner`, PREPARE/MUTATE de copia) | **No** se reutiliza |
| SNAPSHOT del Plugin | Helper compartido que captura tambien `Normal`, escala completa, tipo `MInsertBlock` y capa. RACKMIRROR lo usa desde su primer gate. Migrar RACKDUPLICAR a el va en un paso propio con G-R1, G-R2, G-R4, G-R6 y la guarda de cableado **vistos en RED primero** y reapuntados con proteccion **igual o mayor** |

### 5.2 Plan del espejo (Application, puro)

Compone el nucleo neutral y anade: transformacion fuente (§4.1), admisibilidad (§3.8), autoridad authored (§5.3),
representabilidad (§7), resolucion efectiva (§10.2), nombre e identidad (§5.4). Entrada y salida son datos planos:
sin `ObjectId`, sin `Matrix3d`, sin transacciones.

### 5.3 Autoridad authored del grupo (todos los kinds)

- Selectivo: `SelectiveAuthoredAuthority.IsSameAuthority` (`SelectiveAuthoredAuthority.cs:96-121`), sin cambios.
- Demas kinds: comparador **por kind** declarado por su reflector (`IsSameAuthority`), estructural sobre el documento
  del kind y **excluyendo** los metadatos propios de cada vista que `RACKEDITAR` preserva por hermana
  (`RackCommandSupport.cs:40-67`).
- Divergente o ilegible ⇒ fail-closed (E3). Nunca se elige una vista.
- Solo en el plan del espejo: **RACKDUPLICAR no cambia**.

### 5.4 Identidad y nombre

| Regla | Contenido |
|---|---|
| ID-1 | Un `NewRackId` (`Guid`) por grupo logico, compartido por todas sus vistas seleccionadas |
| ID-2 | `OldRackId → NewRackId` solo en memoria (asignacion del plan); **no** se persiste |
| ID-3 | Legacy sin `RackId`: clave `Definition(handle)` dentro del lote; la copia recibe `RackId` real (precedente I-51) |
| ID-4 | Nombre logico `<base> - espejo` identico en sobre e identidad interior de todas las definiciones del grupo |
| ID-5 | Orden obligatorio: **reflejar** el documento → `RackEmbedComposer.Compose(sobreFuente, kind, idFuente, nombreFuente, View, σ', diseñoReflejado)` → `RackEnvelopeRestamp.RestampEnvelope(payload, nombreEspejo, NewRackId)` (NI-1..NI-6 de I-51, `RackEnvelopeRestamp.cs:52-88`) |
| ID-6 | `RackEnvelopeRestamp.cs` **no** se modifica (G-R5 intacta) |
| ID-7 | Referencias enlazadas (misma definicion) seleccionadas ⇒ **una** definicion nueva y N referencias |

---

## 6. Reflectores

### 6.1 Contrato comun (Application, sin tipos de AutoCAD)

Nombres **no** contractuales; las responsabilidades si.

```text
IRackDesignReflector
  Kind                                   // clave del sobre ("selective", "dynamic", ...)
  CanonicalReflection                    // RUN | RT | LINE_X | DEPTH_D | NONE
  Assess(input)       → RepresentabilityReport { clase PDC-7, motivos atribuibles, normalizaciones previstas }
  Reflect(input)      → ReflectionResult { diseño reflejado (mismo tipo de documento), normalizaciones aplicadas }
  DescribeView(input, view, section) → ViewExposure { Expone, F (Transform2D), c, σ', motivo }
  IsSameAuthority(diseños) → bool

input = { Design (payload interior del sobre), catalogos (RackCatalog, secciones estructurales),
          instantanea de ProjectVariablesDocument (solo lectura) }
```

| Requisito | Regla |
|---|---|
| RF-1 Despacho | `KindDispatch<IRackDesignReflector>` en Application (`KindDispatch.cs:19-104`), busqueda sin mayusculas. El comando **no** contiene constantes `Kind*`, tipos concretos ni ramas por kind |
| RF-2 Documento | Entrada y salida = documento authored del **mismo** kind, deserializado y serializado con **su** store. El string es solo la frontera del sobre; **no** hay manipulacion de JSON |
| RF-3 Orden | `Assess` antes de `Reflect`. `Reflect` sobre algo que no sea `REPRESENTABLE` o `REPRESENTABLE_BY_NORMALIZATION` ⇒ error de programacion |
| RF-4 Pureza | `Reflect` es puro y determinista; no lee el reloj, no genera ids y no toca el registro |
| RF-5 Selectivo | **Prohibido `WithDesign`** (`SelectivePalletDesignDocument.cs:316-330` solo re-congela `VerticalClearance`), salvo que una prueba demuestre que ya no materializa ningun vinculo |
| RF-6 Dominio | Solo como paso **interno**, y solo si G3 demuestra para ese kind que documento → dominio → documento, mas portadores copiados, es identico |
| RF-7 Portadores | Se copian **desde el origen** (§6.2); nunca se recalculan |
| RF-8 Normalizaciones | Declaradas y reportadas; validas solo con las obligaciones de §6.3 |

### 6.2 Portadores preservados desde el origen

| Portador | Kinds | Evidencia |
|---|---|---|
| `SchemaVersion` del documento | todos | `SelectiveDesignSchema.cs:43-66` (sticky) |
| `ExtensionData` raiz del documento | Selectivo (y los kinds que la tengan) | `SelectivePalletDesignDocument.cs:141-142` |
| `PropertyValues` completo, incluidos `Kind` desconocidos (`expression` de I-49) y su `ExtensionData` | Selectivo | `SelectivePropertyValueDocument.cs:25-38` |
| Literales congelados de **toda** propiedad vinculada | Selectivo | `LinkedPropertyReconciler.cs:193-220` |
| `VariableId` | Selectivo | ADR-0034 §12 |
| `DimensionViews` (I-50, al reconciliar) | Selectivo, Dinamico, Push Back | `DimensionViewVisibility` en la rama de I-50 |
| Propiedades de alcance Rack (I-54, al reconciliar) | todos (miembro del sobre) | Proposal V1 de I-54 §3 |
| Cualquier portador integrado en `main` antes del Candidato | todos | §15 |

### 6.3 Obligaciones de una normalizacion

Una normalizacion `N` solo es valida si **todo** se cumple y se prueba:

1. `Π_(V,σ)(N D) ≡ Π_(V,σ)(D)` para cada vista del kind (no solo las admitidas).
2. `BOM(N D) = BOM(D)` como multiconjunto.
3. `PropertyValues`, literales y `VariableId` identicos.
4. Portadores de §6.2 identicos.
5. Round-trip del store equivalente sobre `N D`.

Normalizaciones previstas en V1: **N-S02**, **N-S06**, **N-S07** y **N-H02** (§7).

### 6.4 Verificacion dinamica por rack (PREFLIGHT)

Ademas de las reglas estaticas de §7, para cada grupo el plan del espejo **verifica** sobre el rack concreto:

```text
V-BOM   BOM(efectivo(μ_k D)) = BOM(efectivo(D))                     multiconjunto (§11.3)
V-VIEW  para cada (V,σ) seleccionada:  Π_(V,σ')(μ_k D) ≡ F ∘ Π_(V,σ)(D)     (§11.1)
V-RT    round-trip del store sobre D y sobre μ_k D sin perdida de portadores  (§10.4)
```

Cualquier discrepancia ⇒ `UNKNOWN` material ⇒ fail-closed (E6), con la pieza o linea que difiere en el diagnostico.

---

## 7. Matriz de representabilidad V1

Clases: **R** = `REPRESENTABLE`; **RN** = `REPRESENTABLE_BY_NORMALIZATION`; **MC** = `REQUIRES_MODEL_CHANGE`;
**UK** = `UNKNOWN`. «Deteccion» = condicion evaluada por rack en PREFLIGHT. `M` = frentes del grid maestro (Selectivo);
`N` = frentes (Dinamico/Push Back) o estaciones (Cantilever).

### 7.1 Selectivo — `μ_RUN`

| # | Propiedad / estado | Marco local | Regla bajo μ_RUN | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| S-01 | Frentes por fondo cuando algun fondo tiene menos frentes que el maestro (esquina) | run; fondo corto = prefijo en el poste 0 | la alineacion al final no es expresable | **MC** | `∃k: C_k < M` | `SelectiveGeometryResolver.cs:131-175` |
| S-01b | Orden de frentes con reticula completa (`Bays`, `ExtraFondoBays[k-1]`) | run | invertir `Bays` y cada lista explicita; un fondo que hereda `Bays` queda invertido por herencia | **R** | — | `SelectiveGeometryResolver.cs:210-224` |
| S-02 | Medio frente `Segments` | tramos desde el poste **izquierdo**; el ultimo se calcula | invertir; el remanente calculado pasa a `Length` explicito del nuevo primer tramo; `Loaded` invertido | **RN** (N-S02) si los postes extremos del frente tienen el mismo peralte y troquel; si no **MC** | comparar peralte y troquel efectivos de `f` y `f+1` | `SelectiveMedioFrente.cs:6-18`, `:70`; `SelectiveDesviadorPlan.cs:214-226` |
| S-03 | `PalletDepth`, `ExtraFondoDepths`, `CabeceraFondoOverrides`, `SeparatorLengths` | profundidad | sin cambio | **R** | — | `SelectiveDepthLayout.cs:118-215` |
| S-04 | Grid maestro y desempate | numero de frentes por fondo | sin cambio (el orden de fondos no se toca) | **R** | — | `SelectiveGeometryResolver.cs:137-175` |
| S-05 | Celdas: pallet, larguero, `BeamLengthOverride`, `ClearOverride` | (fondo, frente, nivel) | se mueven con su frente; valores intactos | **R** | — | `SelectiveCellAddress.cs:21-42` |
| S-05b | Por frente: `FloorBeam`, `HeightOverride`, `FloorBeamRiseOverride` | (fondo, frente) | se mueven con su frente | **R** | — | `SelectivePalletDesign.cs:695-716` |
| S-06 | `PostPeraltes[p]` | poste del run | rellenar a `M+1` con `PostPeralte` (≤0 o corta ⇒ defecto), luego `p → M−p` | **RN** (N-S06) | lista corta o con ≤0 | `SelectivePalletDesign.cs:107` |
| S-07 | `PostCabeceras[p]`, `ExtraFondoPostCabeceras[k-1][p]` | (fondo, poste) | rellenar con `null` a `M+1` / `C_k+1`, luego invertir cada fila | **RN** (N-S07) | filas cortas | `SelectiveGeometryResolver.cs:184-206` |
| S-08 | Cabecera custom: `LeftPost`/`RightPost`, placas | Left = poste delantero (profundidad) | sin cambio | **R** | — | `LateralHeaderLayoutBuilder.cs:49-59` |
| S-09 | `DiagonalDirection` y `AutoAlternating` | plano del marco (profundidad × altura) | sin cambio: la diagonal esta en el plano que RUN deja fijo | **R** | — | `BracingPanel.cs:40-48` |
| S-10 | `MountingFace` de horizontales y paneles | caras del marco, normal al run | Front ↔ Back | **R** si G3 confirma la semantica «normal al run»; si no **UK** | caracterizacion G3 | `BracingPanelMemberBuilder.cs:159-165`, `:303-322` |
| S-11 | Bota `Side`, `Bota.Placement` | cara cercana/lejana (profundidad) | sin cambio | **R** | — | `BootPlacement.cs:41-65` |
| S-11b | Bota `PostSides[].PostIndex`, `Bota.Posts[].PostIndex` | poste del run | `p → M−p`; valores intactos | **R** | — | `SelectivePalletDesign.cs:244-299` |
| S-12 | Protector lateral `PostSides` explicito distinto del patron por defecto | doble lectura: orientacion en planta y extremo de profundidad en lateral | no demostrada | **UK** ⇒ fail-closed | explicito ≠ {poste 0 Left, poste M Right} | `SelectiveSafetyEnds.cs:100-116`; `SelectiveSafetyPlacement.cs:221-248` |
| S-12b | Protector lateral con el patron que escribe el editor por defecto (poste 0 Left, ultimo poste Right) | simetrico | sin cambio | **R** | — | `SelectiveSafetyWindow.cs:788-791` |
| S-13 | Tope `Side`, `TopeFondo`, `TopeShared`, `TopeSaque`, `TopeFrontal` | profundidad | sin cambio | **R** | — | `SelectiveTopePlan.cs:89-160` |
| S-13b | Tope `TopeOffCells[].Frente` | frente, compartido por fondos | `f → M−1−f`; nivel intacto | **R** | — | `SelectiveTopePlan.cs:101-127` |
| S-14 | Desviador `Side`, `DesviadorLongitud`, `DesviadorPrimerNivelAltura` | caras de pasillo (profundidad) | sin cambio | **R** | — | `SelectiveDesviadorPlan.cs:116-128` |
| S-14b | Desviador `DesviadorOffCells[]` (clave = indice de poste cargado, incluidos intermedios) | columnas del run | `c → P−1−c` con `P` de la geometria resuelta | **R** (requiere catalogo) | — | `SelectiveDesviadorPlan.cs:132-148` |
| S-15 | Parrilla `ParrillaFrontal`, `ParrillaLateral`, `ParrillaFrente`, `ParrillaCantidad` | distribucion simetrica | sin cambio | **R** | — | `SelectiveParrillaPlacement.cs:40-54` |
| S-15b | Parrilla `ParrillaOffCells[].Frente` | frente | `f → M−1−f` | **R** | — | `SelectiveFrontalBuilder.cs:249-255` |
| S-16 | Numeracion y nombres de bloque por indice | indice | se regeneran (PDC-5) | **R** | — | `SelectiveFrontalBuilder.cs:320-340` |
| S-17 | `Dimensions`, `DimensionStyle`, `AnnotationScale`, `DrawRackName`, `NumberFronts`, `NumberLevels`, `DrawBasePlate`, `DrawPallets` | toggles | sin cambio; anotaciones regeneradas | **R** | — | `SelectiveDimensions.cs:67-254` |
| S-18 | `PropertyValues` y literales congelados | escalares del rack | copia exacta | **R** | registro legible y vinculos resolubles (§10.2) | `SelectiveLinkedProperties.cs:188-213` |
| S-19 | `Section` del sobre (frontal `k`, planta −1) | — | identidad | **R** | — | `RackSelectivoCommands.cs:166-217` |
| S-20 | Vista lateral (`Section` = poste) | profundidad | no expone RUN | **MC** (primer corte) | `View = lateral` | `SelectiveLateralBuilder.cs:173-198` |
| S-21 | Campos de otros sistemas en el DTO (`BotaB*`, `DefensaPosts`, `GuiaEntradaOffCells`) | no aplican | round-trip intacto | **R** | — | `SelectivePalletDesignDocument.cs:528-567` |
| S-22 | `AuthoredSide` (persistido por el DTO) | — | sin cambio | **R** | — | `SelectivePalletDesignDocument.cs:579` |
| S-23 | `DimensionViews` (I-50) | tipo de vista | sin cambio | **R** tras reconciliar I-50 | centinela cruzado | `DimensionViewVisibility` (rama I-50) |
| S-24 | `ExtensionData` anidado | — | — | **UK** por payload | V-RT (§6.4) | §10.4 |

### 7.2 Dinamico — `μ_RT`

| # | Propiedad / estado | Marco local | Regla bajo μ_RT | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| D-01 | Salida en X=0 / entrada en `TotalLength` | profundidad fijada | sin cambio | **R** | — | `DynamicLoadBeamGeometry.cs:166-190` |
| D-02 | `Fronts[]` y sus niveles | frentes | invertir la lista | **R** | — | `DynamicFrontGeometry.cs:129-162` |
| D-03 | `Modules[]` (orden, `Kind`, `Length`) | profundidad | sin cambio | **R** | — | `DynamicRackSystem.cs:141-153` |
| D-03b | `Modules[].Header`: `MountingFace` | caras del marco, normal a frentes | Front ↔ Back | **R** si G3 confirma; si no **UK** | caracterizacion G3 | `BracingPanelMemberBuilder.cs:159-165` |
| D-03c | `Modules[].Header`: `LeftPost`/`RightPost`, diagonales | profundidad | sin cambio | **R** | — | `LateralHeaderLayoutBuilder.cs:49-59` |
| D-04 | `HeaderLineOverrides[]` (`PostIndex`, `ModuleId`, `Header`) | linea × modulo | `p → N−p`; `ModuleId` intacto; contenido como D-03b/c | **R** | — | `DynamicFrontGeometry.cs:374-395` |
| D-05 | `DerivedPostLineOverrides[]` (`PostIndex`, `Height`) | linea | `p → N−p` | **R** | — | `DynamicFrontGeometry.cs:402-424` |
| D-06 | Escalares (alturas, peraltes, pallets, tolerancias, separadores, `DerivedPost*`) | vertical o rack | sin cambio | **R** | — | `DynamicRackSystemDocument.cs:16-45` |
| D-07 | Bota `PostSides`, `BotaPosts` | poste | `p → N−p`; valores intactos | **R** | — | `BootPlacement.cs:44-53` |
| D-08 | Protector lateral explicito | orientacion segun la regla adaptativa validada por el Owner en I-32 (`DynamicSafetyDefaults.cs:57-92`), pero las copias por extremo dependen del lado (`:108-176`) | no demostrada para valores explicitos | **UK** ⇒ fail-closed; sin eleccion (regla adaptativa) **R** | `Side ≠ None` o `PostSides` explicitos | `DynamicSafetyMultiViewBuilder.cs:132-140`; `DynamicSafetyDefaults.cs:57-176` |
| D-09 | Desviador `Side` | caras de salida/entrada | sin cambio | **R** | — | `DynamicSafetyLateralBuilder.cs:308-317` |
| D-10 | Desviador `DesviadorOffCells` (clave `Math.Min(post, N−1)`) | columnas | reclave de `q = N−p` | **R** si, por nivel, `off(poste 0) = off(poste 1)`; si no **MC** | por nivel | `SelectiveDesviadorPlan.cs:28-31` |
| D-11 | Defensa `DefensaPosts[]` (`PostIndex`, `Exit/EntranceLength`, `Auto`) | poste × extremo | `p → N−p`; extremos intactos | **R** | — | `DynamicForkliftDefensePlan.cs:50-79` |
| D-12 | Guia de entrada `GuiaEntradaOffCells[]` (`Frente`, `Level`) | frente | `f → N−1−f` | **R** | — | `DynamicEntranceGuidePlan.cs:27-64` |
| D-13 | Numeracion de frentes | indice | se regenera | **R** | — | `DynamicViewDecorations.cs:58-69` |
| D-14 | `Section` frontal 0/1; planta −1 | extremo | identidad | **R** | — | `RackDinamicoCommands.cs:393` |
| D-15 | Vista lateral (poste) — vista primaria obligatoria | profundidad | no expone RT | **MC** (primer corte) | `View = lateral` | `RackDynamicSystemWindow.xaml.cs:2877-2885` |
| D-16 | Payload legado solo con `DynamicSystem` resuelto (sin `DynamicDesign`) | sistema resuelto | reflector no definido | **UK** ⇒ fail-closed | `DynamicDesign == null` | `DynamicKindHandler.cs:40-44` |
| D-17 | `DimensionViews` (I-50) | tipo de vista | sin cambio | **R** tras reconciliar | centinela | — |
| D-18 | `ExtensionData` anidado | — | — | **UK** por payload | V-RT | §10.4 |

### 7.3 Push Back de un sentido — `μ_RT`

| # | Propiedad / estado | Marco local | Regla bajo μ_RT | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| P-01 | Pasillo (extremo bajo) en X=0 | profundidad fijada | sin cambio | **R** | — | `PushBackPlacements.cs:43-61` |
| P-02 | `Structure`: hereda D-02..D-07, D-09..D-11 | — | como el Dinamico | **R** / segun fila | — | `PushBackDesignDocument.cs:27` |
| P-03 | Configuracion por frente (`DefaultPalletsDeep`, `PalletsDeepOverrides`, `DrawPallets`, `HighEndBeamPeraltes`, `FirstLevelHeight`) | frente | se reordena con los frentes | **R** | — | `PushBackCellDepth.cs:41-173` |
| P-04 | Tope posterior `RearTopeSaque`, `RearTopePieceId` | lado | sin cambio | **R** | — | `PushBackRearTope.cs:41-53` |
| P-05 | `RearTopeOffCells[].Frente` | frente | `f → N−1−f` | **R** | — | `PushBackRearTopeBuilder.cs:268-307` |
| P-06 | `Side` colapsado, `AuthoredSide`, `PostSides` | poste | `p → N−p` | **R**; protector lateral explicito **UK** como D-08 | — | `PushBackSafetyAuthority.cs:239-256` |
| P-07 | `DesviadorOffCells` por poste (biyectivo) | poste | `p → N−p` | **R** | — | `PushBackSafetyAuthority.cs:271` |
| P-08 | Diseño bloqueado para salida | — | — | fail-closed (E6) | `RackBomOutputGate.For(system).Reason ≠ null` | `PushBackKindHandler.cs:61-74` |
| P-09 | `Section` frontal 0/1; planta −1 | — | identidad | **R** | — | `PushBackSystemFrontalBuilder.cs:138-139` |
| P-10 | Vista lateral | profundidad | no expone RT | **MC** (primer corte) | `View = lateral` | — |

### 7.4 Push Back compuesto A/B — `μ_RT` (sin intercambio A/B)

| # | Propiedad / estado | Marco local | Regla bajo μ_RT | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| PC-01 | Configuraciones de los lados A y B | lado | **sin intercambio**; cada lado reordena sus frentes | **R** | — | ADR-0031 §5-bis |
| PC-02 | `Structure.Modules` (A → `GAP` → B invertidos con `B:`) | profundidad | sin cambio | **R** | — | `PushBackCompositeStructure.cs:495-531` |
| PC-03 | `Topologies[]`: `Frente`; tipo y direccion; defaults | frente | `f → N−1−f`; tipo y direccion intactos; entradas dormantes conservadas | **R** | — | `PushBackSide.cs:52-59` |
| PC-04 | `StructureOverrideA/B` | profundidad por lado | sin cambio | **R** | — | `PushBackSideConfiguration.cs:182-216` |
| PC-05 | `AbsentSlotsA/B` y nulos legacy de `SideB.Fronts` | ranura por lado | `i → N−1−i` | **R** si ningun lado tiene ranuras ausentes al **inicio** ni al **final**; si no **MC** (las finales se retiran y las demas dibujan borde) | por lado | `PushBackCompositeStructure.cs:284-321` |
| PC-06 | Bota por lado (`Bota*`, `BotaB*`, `BotaSidesDeclared`) | lado × poste | postes `p → N−p`; lado intacto | **R** | — | `PushBackBootPlan.cs:114-159` |
| PC-07 | Defensa `ExitLength` (pasillo A) / `EntranceLength` (pasillo B) y piezas por lado | poste × lado | `p → N−p`; extremos intactos | **R** | — | `PushBackDefenseSides.cs:35-95` |
| PC-08 | Tope posterior por lado | lado | off-cells `f → N−1−f` | **R** | — | `PushBackSideDesign.cs:48` |
| PC-09 | `HeaderLineOverrides` con `ModuleId` `M…`/`GAP`/`B:…` | linea × modulo | `p → N−p`; `ModuleId` intacto | **R** | — | `PushBackEditorDesignAssembler.cs:348-364` |
| PC-10 | `Section` frontal 0..3 | extremo × lado | identidad | **R** | — | `PushBackSystemFrontalBuilder.cs:134-155` |
| PC-11 | Letras «A»/«B» | posicion en profundidad | sin cambio | **R** | — | `PushBackSideAnnotations.cs:31`, `:104` |
| PC-12 | `Composite.Gap`, `CentralSeparator` | — | sin cambio | **R** | — | `PushBackCompositeStructure.cs:123-133` |
| PC-13 | Vista lateral | profundidad | no expone RT | **MC** (primer corte) | `View = lateral` | — |

### 7.5 Cantilever — `μ_X`

| # | Propiedad / estado | Marco local | Regla bajo μ_X | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| K-01 | `StationTopology.FaceMode` | — | sin cambio | **R** | — | `CantileverStationDesign.cs:14-21` |
| K-02 | `StationTopology.SingleSide` | ±Y | sin cambio | **R** | — | `CantileverArmFrameResolver.cs:59-75` |
| K-03 | `ArmCellOverrides[].Side` | ±Y | sin cambio | **R** | — | `CantileverLineDesign.cs:259` |
| K-04 | `ArmCellOverrides[].StationIndex` | X (estaciones) | `i → N−1−i` si `0 ≤ i < N`; fuera de rango se conserva; orden de lista intacto (gana la primera coincidencia) | **R** | — | `CantileverLineDesign.cs:255`, `:430-437` |
| K-05 | `StationCount`, `ColumnCentreSpacing` uniforme | X | sin cambio | **R** | — | `CantileverLineDesign.cs:339-380` |
| K-06 | Lado de traslape del arriostramiento (derivado) | ±Y | sin cambio | **R** (falso NR de G1) | — | `CantileverLineResolver.cs:271-277` |
| K-07 | Arriostramiento: diagonales A/B y manos de adaptador (derivadas); `BraceKind`, seccion | plano XZ | A↔B derivado | **R** con `ColdRolledRound`; **UK** con seccion estructural asimetrica | `BraceKind` + familia | `CantileverIntervalResolver.cs:227-235`; `CantileverLineFrameResolver.cs:98-116` |
| K-08 | Brazo `Single` con seccion asimetrica (canal C, angulo L) | eje X del marco fijado por el lado | la mano no es expresable | **MC** ⇒ fail-closed | `Arrangement = Single` ∧ familia asimetrica | `CantileverArmFrameResolver.cs:88-123`; `CantileverArmResolver.cs:229-243` |
| K-08b | Brazo W o canal doble | simetrico | sin cambio | **R** | — | `CantileverCataloguePolicies.cs:93-131` |
| K-09 | Ids de placa en BOM | patron medido desde `Outline[0]` | — | **UK** ⇒ V-BOM decide por rack | `BOM(μD) ≠ BOM(D)` | `CantileverStationBomBuilder.cs:292-314` |
| K-10 | `Section` (frontal y planta −1; lateral = estacion) | — | identidad en vistas admitidas | **R** | — | `CantileverViewPlanBuilder.cs:237-239` |
| K-11 | Separador siempre `Mirrored = true` (posible defecto existente) | — | no afectado por R_X | **R** (defecto fuera de alcance) | — | `CantileverLineFrameResolver.cs:61-85` |
| K-12 | Paneles de arriostramiento, niveles, altura, visibilidad de planta, plantillas de base y columna (W) | Z o simetricos en X | sin cambio | **R** | — | `CantileverLineDesign.cs:66-309` |
| K-13 | Vista lateral (estacion) | −Y | no expone R_X | **MC** (primer corte) | `View = lateral` | `CantileverViewPlanBuilder.cs:213-215` |
| K-14 | Linea invalida o catalogo de secciones ilegible | — | — | fail-closed (E6) | `!line.IsValid` / `TryLoad` falso | `CantileverKindHandler.cs:48-67` |
| K-15 | Campos desconocidos dentro de `Line` | — | — | **UK** por payload | V-RT | `CantileverKindHandler.cs:80-110` |

### 7.6 Cabecera independiente — `μ_D`

| # | Propiedad / estado | Marco local | Regla bajo μ_D | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| H-01 | `LeftPost` ↔ `RightPost`; `LeftBasePlate` ↔ `RightBasePlate` | profundidad | intercambio de objetos (el lado se re-deriva al cargar) | **R** | — | `RackFrameConfiguration.cs:57-60`; `RackFrameProjectDocument.cs:103-106` |
| H-02 | `Panels[].DiagonalDirection` | UpRight sube izquierda → derecha | explicitas: UpRight ↔ UpLeft; `AutoAlternating`: fijar la direccion efectiva por panel y luego intercambiar | **R** (explicitas) / **RN** (N-H02, Auto) | `AutoAlternating` presente | `BracingPanel.cs:40-48` |
| H-03 | `MountingFace` | caras del marco | sin cambio (R_D no toca las caras) | **R** | — | — |
| H-04 | Rasgos que el layout lee solo del lado izquierdo (rejilla de troqueles, `PostId`, placa, inset y celosia en planta, `ConnectionPointId`) | izquierda | el intercambio no es exacto si izquierda y derecha difieren | **R** si ambos lados son equivalentes en esos campos; si no **MC** | igualdad de campos + V-VIEW | `LateralHeaderLayoutBuilder.cs:61-67`; `LateralHeaderParametersFactory.cs:31-32`; `PlantaHeaderLayoutBuilder.cs:71-93` |
| H-05 | Horizontales, elevaciones, alturas, troqueles, `Arrangement` | vertical | sin cambio | **R** | — | `LateralHeaderLayoutBuilder.cs:83-114`, `:262-294` |
| H-06 | `StandardBaselineId`, `IsStandard`, `IsException` tras normalizar Auto | plantilla | semantica de «estandar» tras N-H02 | **UK** hasta G5; mientras tanto, Auto con baseline ⇒ fail-closed | Auto ∧ baseline no vacio | `RackFrameConfigurationFactory.cs:79-91`, `:204-244` |
| H-07 | `Section` (lateral y planta −1) | — | identidad | **R** | — | `RackCabeceraCommands.cs:181-198` |
| H-08 | Round-trip del documento (sin `ExtensionData`; version vuelve a 1.0) | — | deuda conocida; no empeorar | **UK** por payload | V-RT | `CabeceraKindHandler.cs:44-66` |

### 7.7 Cama

| # | Propiedad / estado | Clase | Evidencia |
|---|---|---|---|
| F-01 | Lado del tope / sentido del riel: ningun campo persistido | **MC** ⇒ fail-closed siempre | `FlowBedLateralBuilder.cs:34-66` |
| F-02 | `BedType`, `LaneDepth`, `PalletDepth`, `RollerId`, `RollerPitchOverride` | sin vista admisible | `FlowBedConfiguration.cs:10-22` |

### 7.8 Transversal

| # | Propiedad / estado | Regla | Clase | Evidencia |
|---|---|---|---|---|
| X-01 | `Id`/`Name` del sobre e identidad interior | restamp de copia (§5.4) | **R** | `RackEnvelopeRestamp.cs:52-88` |
| X-02 | `SchemaVersion`, `ExtensionData` y miembros del sobre de otras iniciativas (I-54) | `Compose` los hereda | **R** si `Compose` hereda todo miembro distinto de `Kind/Id/Name/View/Section/Design`; reconciliar I-54 | `RackEmbedComposer.cs:21-35` |
| X-03 | `ExtensionData` anidado por kind | V-RT por payload | **UK** por kind hasta G3 | §10.4 |
| X-04 | Colocacion de la referencia | `P' = G·P·F` | **R** con fuente canonica (§4) | §3 |
| X-05 | Referencias enlazadas seleccionadas | una definicion nueva + N referencias | **R** | I-51 PD-2 |
| X-06 | Divergencia authored entre vistas del grupo | fail-closed | — | §5.3 |
| X-07 | Drive-In | no existe | N/A | `RackMainMenuWindow.xaml:139-144` |
| X-08 | Larguero | sin bloque RackCad | N/A | `KindHandlerRegistry.cs:51-53` |

---

## 8. Regeneracion

### 8.1 Application: autoridad pura de planes por vista

Una autoridad por kind, con contrato comun, **compartida** con el redibujo (no especifica del espejo):

```text
RackViewPlanAuthority (por kind; nombre no contractual)
  Build(request) → ViewPlanResult | fallo atribuible

request = { Design (documento authored), View, Section, catalogos, instantanea de ProjectVariablesDocument,
            sugerencia de nombre de bloque }

ViewPlanResult = { Familia: HeaderRun | CantileverCurves,
                   HeaderRunPlan | CantileverViewPlan,
                   nombre de bloque sugerido,
                   tramo del eje de la vista (para c) }
```

La autoridad **reutiliza** los builders puros existentes; lo que hoy es pegamento del Plugin pasa a ella:

| Kind | Resolucion (Application, existente) | Plan por vista (Application, existente) |
|---|---|---|
| selective | `SelectivePalletDesignStore` → `SelectiveEffectiveDesignResolver.Resolve(authored, projectVariables)` → `SelectiveGeometryResolver.Resolve(efectivo, catalogo)` (mismo camino que `SelectiveKindHandler.cs:36-70`) | frontal: `SelectiveFrontalBuilder.BuildPlan(SelectiveDepthLayout.FondoSystemView(system, k), catalogo)`; planta: `SelectivePlantaBuilder.BuildPlan(system, catalogo)` |
| dynamic | `RackProjectStore` → `DynamicRackSystemResolver(catalogo).Resolve(DynamicDesign).System` (`DynamicKindHandler.cs:40-44`) | frontal: `DynamicSystemFrontalBuilder.BuildPlan(system, catalogo, end)`; planta: `DynamicSystemPlantaBuilder.BuildPlan` |
| pushback | `RackProjectStore` → `PushBackResolver(catalogo).Resolve(PushBackDesign)`; bloqueo `RackBomOutputGate.For` (`PushBackKindHandler.cs:40-74`) | frontal: `PushBackSystemFrontalBuilder.BuildPlan(system, catalogo, end, side)`; planta: `PushBackSystemPlantaBuilder.BuildPlan` |
| cantilever | `RackProjectStore` → `CantileverLineEditorAssembler(secciones).Build(CantileverLineDesign)` (`CantileverKindHandler.cs:48-67`) | `CantileverViewPlanBuilder.Build` (`:248`) |
| cabecera | `RackProjectStore` → `Header` | lateral: `HeaderInstanceGrouper.Group(LateralHeaderLayoutBuilder.Build(config, LateralHeaderParametersFactory.FromConfiguration(config), catalogo).Instances, nombre)` (`LateralHeaderDrawService.cs:60-90`); planta: `new HeaderRunPlan(vacio, PlantaHeaderLayoutBuilder.Build(config, catalogo))` (`PlantaHeaderDrawService.cs:29`) |

Restricciones: sin tipos de AutoCAD; sin `ObjectId`; sin I/O de disco dentro de `Build` (los catalogos llegan cargados);
un fallo de resolucion efectiva devuelve un fallo tipado, nunca un plan con literal congelado.

### 8.2 Plugin: materializador generico

```text
PREPARE (dentro del lock, FUERA de la transaccion de escritura)
  familia HeaderRun  → BlockLibraryImporter.EnsureForPlan(db, plan)          (excepcion de infraestructura, §9.4)
  familia Cantilever → nada (polilineas, sin bloques de biblioteca)
  verificar que todos los bloques del plan existen tras importar             ⇒ si falta alguno: E10

CreateInTransaction(db, tx, plan, nombreBloque, payload) → definitionId      (transaccion del llamador)
  HeaderRun  → new LateralHeaderDrawer().CreateSystemBlock(db, tx, plan, nombre)   (LateralHeaderDrawer.cs:28)
  Cantilever → CantileverViewMaterializer.CreateBlockDefinition(db, tx, plan, nombre, out nombreReal) (:36-58)
  ambos      → RackBlockData.Write(tx, definitionId, payload)

CreateReference(db, tx, definitionId, colocacion) → referenceId               (Model Space; transaccion del llamador)
  Position = (p'.X, p'.Y, Z fuente); Rotation = θ'; ScaleFactors = (s, s, s); LayerId = capa de la referencia fuente
```

- Despacho por **familia de plan**, no por kind: el materializador no conoce Left/Right, A/B, estaciones ni reglas
  de cabecera.
- No abre lock ni transaccion, no confirma, no regenera, no purga y no importa dentro de MUTATE (precedente
  `SystemBlockWriter.RedefineInTransaction`, `SystemBlockWriter.cs:82-116`).
- Todas las vistas de familia `HeaderRun` usan hoy `new LateralHeaderDrawer()` sin opciones (verificado en los siete
  servicios de vista); el materializador no necesita opciones por kind.
- La politica de unicidad de nombres de bloque es la existente de cada familia; I-52 **no** unifica las dos
  politicas (decision de I-09).

### 8.3 Geometria neutral

- `Transform2D.ReflectionAboutLine(a, b)` en Application/Geometry, compuesta con las primitivas existentes (§3.3).
- Un valor de colocacion `(Position2D, RotationRadians, UniformScale)` con composicion a `Transform2D` y
  descomposicion validada (§3.3).
- **Sin** sistema matematico paralelo y **sin** `Matrix3d` fuera del Plugin.

### 8.4 Autoridad de `View`/`Section`

La decodificacion de `Section` y la exposicion por vista (§3.7) viven en Application, **junto al contrato de cada
kind**. El comando no contiene `switch` de vistas. Las lecturas duplicadas del Plugin
(`RackSelectivoCommands.cs:166-217`; `RackDinamicoCommands.cs:393`; `ProjectVariableMutationExecutor.cs:181-217`)
convergen en la autoridad en G8 o quedan fijadas por la prueba de equivalencia (§8.5).

### 8.5 Convergencia con RACKEDITAR → Actualizar

Objetivo: RACKMIRROR y RACKEDITAR → Actualizar **no divergen** al generar la misma vista.

| Opcion | Contenido | Estado |
|---|---|---|
| **C-1 (preferida)** | En G8, `RACKEDITAR` construye los planes de las vistas **admitidas** (frontal y planta de Selectivo, Dinamico, Push Back y Cantilever; lateral y planta de cabecera) llamando a la autoridad con el `designJson` que escribe | Requiere caracterizacion previa: la autoridad sobre ese `designJson` produce el mismo plan que el sistema de la ventana |
| **C-2 (reserva)** | Si C-1 amplia demasiado el gate: prueba de equivalencia **obligatoria antes del Candidato** que fija que autoridad y camino de `EditX` producen la misma firma de plan para fixtures por (kind, vista admitida), mas plan de convergencia registrado en `ideas-futuras.md` | Solo con acuerdo de Coordinador y Arquitecto |

El ejecutor de variables (`ProjectVariableMutationExecutor`) converge con la misma autoridad en la misma decision;
hasta entonces mantiene su camino actual.

### 8.6 Politica post-commit propia de I-52

- Preferencia: **solo mensajes**. Sin purga (crear definiciones no deja anidadas huerfanas) y sin `Regen`.
- Si la validacion del Owner demostrara que AutoCAD necesita regenerar para mostrar las definiciones nuevas, se
  admite **un unico** `Regen` post-commit como politica de I-52, con evidencia registrada.
- INV-14 de I-51 **no cambia**: rige RACKDUPLICAR, no RACKMIRROR.

---

## 9. Atomicidad

### 9.1 Fases

```text
ACQUIRE      GetSelection sin SelectionFilter de tipo
SNAPSHOT ALL una transaccion de lectura: referencias (§4.1), definiciones con payload, registro de variables
PREFLIGHT ALL (puro, ANTES de pedir la linea)
             clasificacion y agrupacion (§5.1) · transformacion fuente (§4.1) · admisibilidad (§3.8)
             · autoridad authored (§5.3) · Assess + Reflect por grupo (§6) · resolucion efectiva (§10.2)
             · verificacion dinamica V-BOM / V-VIEW / V-RT (§6.4) · ensayo de Compose + restamp + nombre (§5.4)
             fallo ⇒ mensaje atribuible y FIN — cero mutacion
LINE         dos puntos UCS→WCS validos (§4.2)
PREPARE ALL  colocaciones P' · planes efectivos por vista (§8.1) · payloads definitivos
             · EnsureForPlan (infraestructura, §9.4) · bloques presentes · huella de payloads fuente
MUTATE ALL   un DocumentLock · UNA transaccion de escritura:
             todas las definiciones nuevas + todos los payloads + todas las referencias
COMMIT       una vez
POST         solo mensajes (§8.6)
```

### 9.2 Tabla de fallos

| # | Fallo | Momento | Resultado |
|---|---|---|---|
| E1 | Ninguna fuente valida | PREFLIGHT | fin, sin linea |
| E2 | Payload RackCad inutilizable, `Kind` sin reflector o sin handler, `Design` vacio | PREFLIGHT | fin, cero mutacion |
| E3 | Grupo inconsistente (`Kind`, nombre, autoridad authored) | PREFLIGHT | fin, cero mutacion |
| E4 | Fuente no canonica (ST-3, ST-5..ST-7, ST-9, ST-10) | PREFLIGHT | fin, cero mutacion |
| E5 | Vista no admisible (lateral de rack, cama) | PREFLIGHT | fin, cero mutacion; el mensaje sugiere reflejar las vistas admisibles e insertar las demas con RACKEDITAR desde la copia |
| E6 | `REQUIRES_MODEL_CHANGE`, `UNKNOWN` material, V-BOM, V-VIEW o V-RT fallidas, diseño bloqueado o invalido | PREFLIGHT | fin, cero mutacion; nombra rack, propiedad o pieza |
| E7 | Registro de variables ilegible o incompatible, vinculo roto | PREFLIGHT | fin, cero mutacion |
| E8 | Ensayo de `Compose` + restamp + nombre fallido | PREFLIGHT | fin, cero mutacion |
| E9 | Linea invalida o cancelada | LINE | fin, cero mutacion |
| E10 | Fallo de plan, de importacion, bloque ausente tras importar, o huella de payload cambiada | PREPARE | fin; **ninguna** definicion, referencia ni payload RackCad nuevo; las definiciones auxiliares importadas pueden permanecer (§9.4) |
| E11 | Excepcion de AutoCAD en MUTATE | MUTATE | sin commit ⇒ rollback de **toda** la operacion semantica |

### 9.3 UNDO

- No hay marcas de undo en el Plugin (busqueda `UndoMark`: cero); rige el grupo de undo por comando.
- El Owner valida (§13, M-10) que **un** UNDO revierte la operacion logica completa, y registra **por separado** que
  ocurre con las definiciones auxiliares importadas en PREPARE.

### 9.4 Excepcion declarada: importacion de infraestructura

`BlockLibraryImporter.EnsureForPlan` puede importar definiciones auxiliares de biblioteca **fuera** de la transaccion
principal, segun el precedente de I-47 G9.1 (`ViewBlockDraw.cs:59-89`). Es **mutacion de infraestructura** (bloques de
pieza de la biblioteca), **no** mutacion semantica del rack. **No** se describe como «cero mutacion de la base de
datos»: la garantia de V1 es **cero mutacion semantica** (ninguna definicion RackCad reflejada, ninguna referencia
reflejada, ningun payload RackCad nuevo parcial).

---

## 10. Persistencia, variables de proyecto y portadores

### 10.1 Composicion del payload (por definicion nueva)

```text
1. sobreFuente   = RackEmbedStore.Deserialize(payloadFuente)
2. reflejado     = Reflector(kind).Reflect({ Design = sobreFuente.Design, ... })      (§6)
3. compuesto     = RackEmbedComposer.Compose(sobreFuente, sobreFuente.Kind, sobreFuente.Id, sobreFuente.Name,
                                             sobreFuente.View, σ', reflejado.Design)
4. payload       = RackEnvelopeRestamp.RestampEnvelope(serializar(compuesto), "<base> - espejo", NewRackId)
5. postcondicion = payload.Design ≡ reflejado.Design salvo Id/Name interiores; portadores de §6.2 identicos
```

Todas las definiciones del grupo reciben **el mismo** documento reflejado salvo metadatos propios de su vista; en
Selectivo se re-verifica `IsSameAuthority` sobre los documentos reflejados.

### 10.2 Variables de proyecto

| Regla | Contenido |
|---|---|
| PV-1 | Los vinculos authored se preservan **exactos**: mismo `PropertyId`, mismo `VariableId`, mismo `Kind`, mismo literal congelado |
| PV-2 | El espejo **no** crea variables, **no** desvincula ni revincula, **no** fija literales y **no** escribe el registro |
| PV-3 | El espejo **no** evalua expresiones con logica propia |
| PV-4 | Para generar geometria, consume **en solo lectura** la autoridad efectiva existente (`SelectiveEffectiveDesignResolver`), sobre la instantanea del registro leida en SNAPSHOT (`ProjectVariablesRegistry.Read`, `ProjectVariablesRegistry.cs:22`) |
| PV-5 | Registro ilegible o incompatible, o vinculo roto ⇒ fail-closed (E7), como RACKBOMTOTAL |
| PV-6 | Si I-49 integra antes: RACKMIRROR invoca la autoridad efectiva integrada, preserva `expression` como portador y **no** reimplementa su evaluacion |
| PV-7 | La copia es un **consumidor nuevo** del mismo `VariableId` (ADR-0034 §12): bloquea `Delete` y se redibuja en `ChangeValue` desde su authored reflejado, sin desespejarse |

### 10.3 `DimensionViews` (I-50)

Cada reflector es un **sitio de copia** de `DimensionViews`: debe transportar el `int` exacto, incluidos bits
desconocidos y `null`. Antes del Candidato, si I-50 esta integrada: centinelas `-8`, `13` y `null` a traves del espejo
(mismo metodo que `DimensionViewsCopySitesTests`).

### 10.4 `ExtensionData`, I-54 y deuda de cabecera

- **Caracterizacion por kind en G3**: que conserva y que pierde el round-trip de cada store con campos desconocidos
  anidados.
- **V-RT por payload** (§6.4): si el round-trip del payload concreto pierde algo que no este declarado como defecto
  benigno por la caracterizacion ⇒ `UNKNOWN` ⇒ fail-closed. I-52 **no** empeora ninguna perdida.
- **Cabecera**: re-serializa por dominio, sin `ExtensionData`, y reinicia la version a 1.0. Es deuda conocida, no se
  arregla en I-52 y V-RT la hace visible por payload.
- **I-54**: sus propiedades de alcance Rack viven como miembro del sobre. Antes del Candidato, si I-54 esta integrada,
  toda propiedad Rack debe sobrevivir **exactamente** al espejo, lo que exige que `Compose` herede ese miembro. I-52
  no arregla I-54 antes de que exista contrato integrado.

---

## 11. Equivalencia ejecutable bajo reflexion

### 11.1 Definicion

Para una vista admitida `(V, σ)`:

```text
A = firma(Π_(V,σ')(μ_k D))          plan reflejado semanticamente
B = firma(F ∘ Π_(V,σ)(D))           plan original transformado por F
A ≡ B  ⇔  multiconjuntos iguales de piezas fisicas, dentro de tolerancia
```

| Componente de la firma | Regla |
|---|---|
| Piezas | Roles fisicos; se **excluyen** `Annotation` y `Dimension` |
| Aplanado | Cada instancia de un `HeaderGroup` se lleva a coordenadas de la definicion componiendo la colocacion con su transformacion completa (rotacion incluida). **No** se usa `HeaderRunPlan.Flatten`/`PlacedClone` (`HeaderRunPlan.cs:53-80`) |
| Identidad de pieza | `PieceId`, `BlockName`, `View` |
| Posicion | `Insertion` y `ConnectionAnchor` con `GeometryTolerance.Length`; `GeometryTolerance.Continuity` solo donde una normalizacion declarada escribe un valor calculado (N-S02) |
| Orientacion | rotacion modulo 2π con `GeometryTolerance.Angle` |
| Parametros | `DynamicParameters` como pares ordenados, numericos con tolerancia |
| Mano | signo del determinante total de la pieza (`MirroredX` ⊕ `MirroredY` ⊕ espejo de la colocacion) |
| Curvas Cantilever | contornos con la regla de inversion de `Transform2D` (`PathSegment2D.cs:139-160`) |

### 11.2 Politica de mano y simetria de bloques

1. **Ningun bloque DWG se asume simetrico.** La biblioteca no esta versionada ni tiene metadato de simetria.
2. Una diferencia de mano entre A y B en una pieza es **no equivalente**, salvo que el bloque figure en una
   **declaracion de bloques simetricos** versionada en Application, respaldada por evidencia del Owner (vista en
   AutoCAD de la pieza y su espejo).
3. G3 produce el **censo** de piezas con diferencia de mano por kind y vista sobre fixtures asimetricos; el Owner
   decide cuales son simetricas antes de G9.
4. Sin declaracion: la vista no es equivalente ⇒ el rack falla cerrado (E6). La feature puede quedar inutil para
   algun kind hasta completar el censo; es el comportamiento correcto.
5. Un metadato de simetria en `assets/catalogs` seria cambio de catalogo (archivo caliente, append-only): queda
   **fuera** de I-52 y registrado como alternativa futura.

### 11.3 BOM como multiconjunto

`BOM(efectivo(μ_k D))` y `BOM(efectivo(D))` con el builder del kind (`SelectiveBomBuilder`, `SystemBomBuilder`,
`PushBackBomBuilder`, `CantileverLineEditorAssembler.Bom`, `BomBuilder`), comparados como multiconjunto de lineas
(categoria, pieza, longitud, peralte, cantidad). El orden de lineas no cuenta.

### 11.4 Textos y cotas

No se comparan geometricamente. Se validan con **contratos semanticos**: numeracion regenerada desde el documento
reflejado (PDC-5), letras A/B por posicion, cotas segun `Dimensions`, estilo y `DimensionViews`, y legibilidad
(rotacion de texto no invertida). La validacion visual final es del Owner.

---

## 12. Plan de pruebas (no se implementa en este gate)

### 12.1 Caracterizacion primero (G3; pasan sobre la produccion intacta)

| # | Caracteriza |
|---|---|
| CT-01 | Fixtures asimetricos por kind y vista admitida con firma de plan (§11.1) |
| CT-02 | BOM base de esos fixtures |
| CT-03 | Round-trip de cada store con campos desconocidos anidados: que se conserva y que se pierde |
| CT-04 | Codificacion de `Section` por kind (Selectivo, Dinamico 0/1, Push Back 0..3, Cantilever, cabecera) |
| CT-05 | Tramos y centros `c` por (kind, vista) desde la geometria resuelta |
| CT-06 | Censo de mano: piezas cuya mano difiere entre `F ∘ Π(D)` y la regla del builder |
| CT-07 | `WithDesign` materializa `PalletTolerance` vinculada (hueco actual) |
| CT-08 | `SelectiveDesviadorPlan.CellKey` colapsa N−1/N en el Dinamico |
| CT-09 | `BracingPanel.ResolveDiagonalDirection` por paridad |
| CT-10 | Ausencias de Push Back compuesto: inicio, interior y final dibujan distinto |
| CT-11 | Marco del brazo Cantilever por lado y familia de seccion |
| CT-12 | Semantica de `MountingFace` respecto del run (S-10, D-03b) |
| CT-13 | `SelectiveAuthoredAuthority` distingue un documento reflejado de su origen |
| CT-14 | Censo de `[CommandMethod(` = 33 y unicidad de nombres (linea base) |

### 12.2 Pruebas nuevas (RED antes de implementar)

| # | Afirma | Gate |
|---|---|---|
| T-M01 | `ReflectionAboutLine`: involucion, determinante −1, puntos de la linea fijos | G4 |
| T-M02 | Colocacion: `P' = G·P·F` con determinante positivo; rotacion `2φ−θ+π` con `X_c` y `2φ−θ` con `Y_c`; escala conservada; descomposicion validada | G4 |
| T-M03 | Transformacion fuente ST-1..ST-13 (clasificacion y canonizacion `(−s,−s)`) | G4 |
| T-M04 | Nucleo neutral: agrupacion multi-rack, legacy, enlaces; fachada `RackDuplicationPlan` con T1–T13 intactos y mensajes historicos identicos | G4 |
| T-M05 | Asignador con politica de nombre inyectada: «- copia» historico y `<base> - espejo` | G4 |
| T-M06 | `Assess` por kind: cada fila **MC** y **UK** de §7 detectada con motivo atribuible | G5 |
| T-M07 | `Reflect` involutivo: `Reflect(Reflect(D)) ≡ N(D)` para diseños **R**/**RN** | G5 |
| T-M08 | Normalizaciones N-S02, N-S06, N-S07, N-H02 con las cinco obligaciones de §6.3 | G5 |
| T-M09 | `BOM(μD) = BOM(D)` por kind sobre fixtures | G5 |
| T-M10 | Portadores de §6.2 identicos, incluidos `PropertyValues` con `Kind` desconocido | G5 |
| T-M11 | Autoridad authored por kind (divergente, ilegible, igual) | G5 |
| T-M12 | Conmutacion `Π(μD) ≡ F ∘ Π(D)` por (kind, vista admitida) | G6 |
| T-M13 | Autoridad de plan = builders existentes para las mismas entradas | G6 |
| T-M14 | Plan del espejo: E1..E8 sin mutacion, grupos multi-rack con `NewRackId` distintos, sin `WithDesign` | G6/G7 |
| T-M15 | Guardas del Plugin (§12.3) con RED demostrado por violacion temporal no commiteada | G7 |
| T-M16 | Censo 33 → **34** (guarda reapuntada con motivo) | G7 |
| T-M17 | Convergencia C-1 o equivalencia C-2 con `RACKEDITAR` | G8 |
| T-M18 | Cruce I-50: `DimensionViews` exacto a traves del espejo | G8 (si I-50 integrada) |
| T-M19 | Cruce I-49: entradas `expression` sobreviven y la autoridad efectiva integrada se invoca | G8 (si I-49 integrada) |
| T-M20 | Cruce I-54: propiedades de alcance Rack sobreviven exactas | G8 (si I-54 integrada) |

### 12.3 Guardas del Plugin (nuevas)

| # | Propiedad protegida |
|---|---|
| G-M1 | El comando no contiene constantes `Kind*`, tipos `*KindHandler` concretos, comparaciones ni `switch` sobre `.Kind`, ni literales iguales a un kind |
| G-M2 | Ninguna llamada a `WithDesign(` en reflectores ni en el camino del espejo; ninguna escritura del registro (`ProjectVariablesRegistry.TryWrite(`) |
| G-M3 | Ningun `Matrix3d` ni `Autodesk.` en los archivos nuevos de Application |
| G-M4 | Exactamente una transaccion de escritura en MUTATE; nada tras el commit salvo mensajes (sin `PurgeUnreferenced(`, `SyncName(`, `EnsureForPlan(`); `Regen(` solo si §8.6 lo autoriza con evidencia |
| G-M5 | Sin `CloneDefinition(` en el comando (no es el camino de copia) |
| G-M6 | Sin `FindRackBlocks(`, `ScanEnvelopes(` ni `SelectAll` (sin busqueda de hermanas) |
| G-M7 | Una `GetSelection(` sin filtro de tipo; `CurrentUserCoordinateSystem` solo en la conversion de la linea |
| G-M8 | El materializador no contiene nombres de kind ni de propiedades de sistema |

### 12.4 Suites y builds

Cada gate con codigo: Core completo en local (UI segun LC-UI en iteracion), CI verde sobre el SHA exacto. Candidato:
Core y UI completas en local, builds Debug de UI y Plugin con AutoCAD cerrado, CI 4/4 sobre el SHA exacto y cobertura
del Candidato (WORKFLOW 4.5).

## 13. Validacion manual del Owner (AutoCAD 2025)

| # | Escenario |
|---|---|
| M-1 | Selectivo frontal + planta, linea vertical: copia reflejada, textos legibles, numeracion regenerada |
| M-2 | UCS girado 30° y origen desplazado: `P'` correcto en WCS |
| M-3 | Guardar, cerrar y reabrir: la copia sigue reflejada |
| M-4 | `RACKEDITAR` sobre la copia → Actualizar: el dibujo **no** cambia |
| M-5 | Desde la copia, Insertar una lateral: sale del diseño reflejado |
| M-6 | `RACKBOMTOTAL`: la copia cotiza igual que el origen |
| M-7 | Selectivo con vinculos: la copia conserva `VariableId`; `ChangeValue` la redibuja reflejada |
| M-8 | Dos racks (frontal + planta de cada uno): dos `NewRackId`, nombres `<base> - espejo` |
| M-9 | Fail-closed sin mutacion: lateral seleccionada, cama, MINSERT, escala no uniforme, fuente espejada, esquina, brazo C sencillo |
| M-10 | UNDO: un paso revierte la operacion; se registra que ocurre con las importaciones |
| M-11 | Dinamico frontal (salida y entrada) + planta |
| M-12 | Push Back compuesto: frontales 0..3 + planta |
| M-13 | Cantilever frontal + planta |
| M-14 | Cabecera lateral + planta (diagonales y postes asimetricos) |
| M-15 | Censo de mano: confirmacion visual de los bloques declarados simetricos |
| M-16 | Anotaciones y cotas legibles, respetando `DimensionViews` si I-50 esta integrada |

## 14. Gates propuestos

Cada gate declara entrada, archivos permitidos, RED esperado, PASS requerido, parada y evidencia por SHA exacto. Ningun
gate arranca sin la orden explicita del Coordinador y sin el preflight de paralelas (§15.3).

| Gate | Entrada | Archivos permitidos | RED esperado | PASS requerido | Parada | Evidencia |
|---|---|---|---|---|---|---|
| **G2** (este) | G1 aceptado | `docs/**` | — | consenso Coordinador + Arquitecto sobre la misma version; ADR-0036 aceptado por el Owner; contrato reescrito; hallazgos laterales en `ideas-futuras.md` | cualquier desacuerdo material ⇒ V2 | SHA de la version acordada + CI |
| **G3** Caracterizacion | G2 congelado | `tests/RackCad.Tests/*Mirror*Characterization*.cs`, fixtures de prueba | ninguno: CT-01..CT-14 pasan sobre produccion intacta | Core completo; censo de mano publicado; tabla de `c` confirmada | una caracterizacion contradice V1 ⇒ Proposal V2 | SHA + conteo Core + CI |
| **G4** Transformacion y seleccion neutral | G3 | `src/RackCad.Application/Geometry/*`; nucleo neutral y fachada en `src/RackCad.Application/Persistence/`; pruebas | T-M01..T-M05 en rojo sobre andamiaje | T-M01..T-M05 verdes; T1–T13 y mensajes de I-51 intactos | cualquier cambio observable de RACKDUPLICAR | SHA + conteos + CI |
| **G5** Reflectores y representabilidad | G4 | contrato y registro en `src/RackCad.Application/` (carpeta nueva del espejo); un reflector por kind en `src/RackCad.Application/Systems/<Kind>/`; pruebas. Divisible en G5a Selectivo, G5b Dinamico y Push Back, G5c Cantilever y cabecera | T-M06..T-M11 en rojo | T-M06..T-M11 verdes; `WithDesign` ausente (guarda de texto) | una fila **R** resulta no equivalente ⇒ reclasificar en V2 | SHA por subgate + CI |
| **G6** Autoridad de planes y materializador | G5 | autoridades de plan por kind en Application; `SystemBlockWriter.cs` (`CreateInTransaction`); materializador nuevo en `src/RackCad.Plugin/Systems/Shared/`; pruebas | T-M12..T-M14 en rojo | conmutacion verde por (kind, vista admitida); builders sin cambio de comportamiento | necesitar cambiar un builder | SHA + CI + build Debug de Plugin |
| **G7** Comando RACKMIRROR | G6 | `src/RackCad.Plugin/RackMirrorCommands.cs` (nuevo); helper de SNAPSHOT compartido; `RackDuplicarCommands.cs` **solo** para migrar al helper; archivos de guardas; `SelectiveEditorOpenTests.cs` (censo); `src/RackCad.UI/RackCommandReference.cs` | T-M15, T-M16 y guardas reapuntadas de I-51 en RED | guardas verdes; censo 34; build Debug de Plugin | debilitar una guarda de I-51 | SHA + conteos + CI |
| **G8** Persistencia, redibujo y cruces | G7 | camino de planes de `RACKEDITAR` para vistas admitidas (C-1) o prueba C-2; pruebas cruzadas | T-M17..T-M20 en rojo cuando apliquen | convergencia o equivalencia verde; cruces verdes con lo integrado | un cruce exige tocar un archivo de §20.3 | SHA + CI |
| **G9** Conformidad, Candidato y Owner | G8 | ninguno de produccion salvo correcciones | — | rebase final si `main` avanzo; Core y UI locales; builds Debug; CI 4/4 exacto; cobertura; M-1..M-16 del Owner | cualquier fallo ⇒ vuelta al gate que corresponda | SHA Candidato + corridas + veredicto del Owner |
| **G10** Documentacion e integracion | G9 | `docs/**`, `README.md` | — | WORKFLOW 4.5: cierre documental, merge `--no-ff`, CI del `MERGE_SHA`, cobertura, limpieza | CI post-merge rojo ⇒ correccion en la rama | SHAs de cierre y merge |

## 15. Coordinacion con iniciativas paralelas

### 15.1 Tips observados al abrir G2

```text
origin/main                                        = 46fcac2b071929d2bd5b07aa28373941417f74a8 (sin avance)
I-49 architecture/motor-expresiones-parametricas   = ccf21c69d9aa6e8d9b622e6389fb61f430d53f03  (Proposal V4; solo docs)
I-50 feature/cotas-independientes-por-vista        = ed50cbd3508c205fb8121a87687cd8416bf47086  (con produccion)
I-53 feature/cabeceras-configurables-multidestino  = f38362d32a737f62d69a229854dc5c1812adf063  (Proposal V1; solo docs)
I-54 architecture/propiedades-personalizadas       = 7c197af81b91df88366c873eddf9e11ddc5e87bb  (Proposal V1; solo docs)
```

### 15.2 Cruces

| Paralela | Cruce | Tratamiento |
|---|---|---|
| **I-50** | Modifica `PushBackMirror.cs`, `PushBackCompositeStructure.cs`, `SelectivePalletDesign(Document).cs` (caliente), `DynamicRackDesign.cs`, `DynamicRackSystemDocument.cs`, `SelectiveDepthLayout.cs`, `SelectiveGeometryResolver.cs`, `RackSelectiveWindow.xaml(.cs)`; contrato de sitios de copia C-01..C-15 | I-52 **no** toca `PushBackMirror.cs` ni esos archivos desde esta base; cada reflector es un sitio de copia; T-M18 al reconciliar |
| **I-49** | `expression` en `PropertyValues`; su condicion de parada cubre `RackDuplicationPlan`, restamp, `SelectiveAuthoredAuthority` y store Selectivo | La fachada de G4 toca `RackDuplicationPlan.cs`: **aviso a I-49 antes de editar**; T-M19; RACKMIRROR invoca la autoridad efectiva integrada |
| **I-53** | Archivos calientes de editores y autoridad de cabecera; su V1 **no** anade datos persistidos (Proposal V1 de I-53 §5.9, §6.10) | Serializar integracion si G8 toca los mismos comandos; todo campo nuevo direccionado por poste, fondo, modulo o lado necesita regla de reflexion antes del Candidato |
| **I-54** | Propiedades de alcance Rack como miembro del sobre; `Compose` solo hereda `SchemaVersion` y `ExtensionData` (su Proposal V1 P-04) | T-M20; `Compose` debe heredar el miembro; I-52 no lo implementa por I-54 |
| Documental | Filas de ROADMAP (I-49, I-50, I-52, I-53, I-54), final de `ideas-futuras.md`, `docs/adr/README.md` (0035 de I-50 y 0036 de I-52) | Quien integre despues conserva todas las filas y entradas en orden numerico |

### 15.3 Protocolo antes de G3, G5, G7 y del Candidato

`git fetch --all --prune`; `git diff --name-only origin/main...origin/<rama>` de I-49, I-50, I-53 e I-54; leer sus
Proposals y contratos en el SHA exacto; comprobar `docs/adr/0036-*` en todos los refs; si `main` avanzo, rebase segun
WORKFLOW antes de escribir codigo.

## 16. ADR-0036

- Archivo: `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`, estado **`propuesto`**, con fila en el indice.
- Congela: espejo semantico (no de entidades); solo copia; `μ_k` canonica por kind; admisibilidad por vista;
  fail-closed; reflectores en Application; autoridad de planes; colocacion canonica sin escala negativa; identidad
  nueva; portadores; fuente canonica; atomicidad; futuro «vista desde el lado opuesto».
- **Numeracion.** 0036 es el primer numero libre en `origin/main` y en todas las ramas remotas vivas al redactarlo:
  `main` llega a 0034, I-50 tiene 0035 en su rama, e I-49, I-53 e I-54 declaran ADR sin numero. `adr/README.md` no
  define reservas; la coordinacion observable es la de I-49 V3 §12.3 («0035 es de I-50»).
- **Protocolo de colision.** En cada preflight de §15.3 se busca `docs/adr/0036-*` y citas de «ADR-0036» en todos los
  refs. Si otra rama crea 0036, gana la que **integre primero** y la otra renumera su ADR en el rebase final, lo que
  esta permitido mientras sea `propuesto` (`adr/README.md`: un propuesto puede editarse libremente).
- **Aceptacion.** Solo el Owner; condicion previa a G3.

## 17. Riesgos abiertos

| # | Riesgo | Mitigacion |
|---|---|---|
| R-1 | Censo de mano: muchas piezas podrian diferir y dejar kinds enteros en fail-closed hasta la declaracion de simetria | CT-06 en G3; decision del Owner antes de G9 |
| R-2 | Un Dinamico dibujado solo en lateral no puede reflejarse | Mensaje E5 explicito; vista del lado opuesto como futuro (ALT-L de §3.9) |
| R-3 | V-RT puede rechazar payloads legitimos si el round-trip normaliza defectos | CT-03 declara defectos benignos por kind |
| R-4 | Convergencia C-1 con `RACKEDITAR` toca comandos calientes que I-53 tambien toca | Decision C-1/C-2 en consenso; serializar |
| R-5 | Semantica de «estandar» de cabecera tras normalizar Auto (H-06) | G5 con caracterizacion; fail-closed mientras tanto |
| R-6 | `MountingFace` sin semantica confirmada (S-10, D-03b) | CT-12; si no se confirma ⇒ **UK** |
| R-7 | Transparencias de AutoCAD entre SNAPSHOT y MUTATE | Huella de payloads en PREPARE (E10) |
| R-8 | Numeracion de ADR con cuatro iniciativas que necesitan ADR | Protocolo de §16 |
| R-9 | Cantilever con brazos C/L sencillos o arriostramiento estructural asimetrico queda fuera | Clasificacion **MC**/**UK**; futuro con metadato de mano |
| R-10 | Rendimiento de V-VIEW y V-BOM en selecciones grandes (dos planes por vista, dos BOM por rack) | Medir en G7; los builders ya son puros y rapidos |

## 18. Desacuerdos no cerrados

| # | Tema | Estado |
|---|---|---|
| DA-1 | Convergencia con `RACKEDITAR`: C-1 (migrar vistas admitidas en G8) frente a C-2 (prueba de equivalencia + plan) | Abierto para Coordinador y Arquitecto |
| DA-2 | Mecanismo de la declaracion de bloques simetricos: lista en Application (propuesta V1) frente a columna de catalogo (futuro) | Propuesta V1 = lista en Application; pendiente de acuerdo |
| DA-3 | Politica post-commit: sin `Regen` frente a un `Regen` unico si AutoCAD lo exige | Pendiente de evidencia del Owner (M-1) |
| DA-4 | Aceptacion por el Owner de que el primer corte no refleja laterales ni la cama, ni Dinamicos solo en lateral | Pendiente del Owner |
| DA-5 | `IsStandard`/`StandardBaselineId` de cabecera tras normalizar Auto | Pendiente de G5 |
| DA-6 | Protocolo de numeracion de ADR con renumeracion del que integre despues | Pendiente del Coordinador |

## 19. Condiciones de parada

- Implementar sin la orden explicita del gate: **prohibido**.
- Una caracterizacion contradice V1, o una fila **R** resulta no equivalente: **detenerse** y abrir Proposal V2.
- Necesitar cambio de schema, de sobre o de Xrecord, o de `RackEnvelopeRestamp.cs`, `PushBackMirror.cs`, `WithDesign` o
  el registro de variables: **detenerse**; nueva revision de Arquitecto y ADR.
- Una paralela modifica materialmente `RackDuplicationPlan.cs`, `RackEmbedComposer.cs`, `SystemBlockWriter.cs`,
  `LateralHeaderDrawer.cs`, `CantileverViewMaterializer.cs`, los builders de plan citados en §8.1 o los stores de
  documentos: **detenerse** antes de editar y reportar SHA y diff.
- Aparece un campo nuevo direccionado por poste, fondo, modulo, lado o estacion sin regla de reflexion: **detenerse**.
- Debilitar una guarda de I-51 para facilitar el cambio: **prohibido**.

## 20. Archivos (sin cambiarlos en este gate)

### 20.1 Produccion probable (G4–G8)

| Capa | Archivo o area |
|---|---|
| Application | `Geometry/Transform2D.cs` (`ReflectionAboutLine`) y valor de colocacion junto a el |
| Application | `Persistence/RackDuplicationPlan.cs` (fachada) + nucleo neutral nuevo en `Persistence/` |
| Application | carpeta nueva del espejo: contrato de reflector, registro, plan del espejo, equivalencia |
| Application | `Systems/Selective/`, `Systems/Dynamic/`, `Systems/PushBack/`, `Systems/Cantilever/`, `RackFrames/`: reflector y autoridad de plan por kind |
| Plugin | `RackMirrorCommands.cs` (nuevo); helper de SNAPSHOT compartido; materializador en `Systems/Shared/`; `Systems/Shared/SystemBlockWriter.cs` (`CreateInTransaction`) |
| Plugin | `RackDuplicarCommands.cs` solo para migrar SNAPSHOT (G7); comandos de `RACKEDITAR` solo si C-1 (G8) |
| UI | `RackCommandReference.cs` (ayuda) |

### 20.2 Pruebas

Caracterizaciones CT-01..CT-14; pruebas T-M01..T-M20; guardas G-M1..G-M8; `SelectiveEditorOpenTests.cs` (censo 34);
guardas de I-51 reapuntadas en `SelectiveDuplicationFailClosedTests.cs` solo si se migra SNAPSHOT.

### 20.3 NO TOCAR sin nueva revision de Arquitecto

`RackEnvelopeRestamp.cs`, `RackCloner.cs`, `PushBackMirror.cs`, `SelectivePalletDesign.cs`,
`SelectivePalletDesignDocument.cs` (incluido `WithDesign`), DTO y stores de todos los kinds, `ProjectVariables/*` salvo
consumo de lectura, `Bom/*` salvo consumo, `KindHandlers/*`, editores WPF, `assets/`, catalogos, biblioteca de
bloques, `.github/`, ADR aceptados.

## 21. Estado

```text
G0 = ACCEPTED        G1 = ACCEPTED (Discovery)
G2 = V1 EN REVISION  (este documento + ADR-0036 propuesto + docs/automation/decisions/I-52.md)
G3+ = NO INICIADO

SUBSTANTIVE IMPLEMENTATION: BLOCKED
Coordinator: REVIEW REQUIRED
Architect: REVIEW REQUIRED
Implementation: NOT READY
```

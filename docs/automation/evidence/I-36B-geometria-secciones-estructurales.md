# Evidencia — I-36B · Geometría y representación prismática de secciones estructurales

> Estado al cerrar: **integrada (2026-07-28)**. Gate `owner-validation` **APROBADO** por el Owner.
> ADR-0022 **aceptado**. El detalle de la validación está en §14.c.

---

## 1. Preflight real

Verificado **antes** de escribir nada, y registrado en
[`../state/I-36B.yml`](../state/I-36B.yml):

| Comprobación | Resultado |
|---|---|
| Base remota | `eafb785` — *Merge I-36A: núcleo y catálogo de secciones estructurales* |
| CI de la base | **verde**, run `30364323887`, 4/4, sobre el SHA exacto de `origin/main` |
| `main` == `origin/main` | sí |
| Árbol de trabajo | limpio |
| Operaciones en curso | ninguna (sin merge, rebase, cherry-pick ni bisect) |
| Worktrees previos | uno solo, el principal del dueño |
| Stashes | cero |
| Ramas remotas | solo `origin/main` |
| Rama objetivo | `architecture/geometria-secciones-estructurales`, **libre** (I-36A la dejó reservada) |
| `OWNER-DECISIONS.md` | **no existe** en el repositorio |
| Siguiente ADR libre | 0022 |
| I-36A carga y valida | sí — 29/29 verde, 983 secciones, catálogo == manifiesto |
| Conflicto material | ninguno |

`origin/main` **no avanzó** durante la ejecución, así que no hubo rebase.

## 2. Reclamo

| Dato | Valor |
|---|---|
| Rama | `architecture/geometria-secciones-estructurales` |
| Worktree | `.claude/worktrees/architecture-geometria-secciones-estructurales` |
| Commit de reclamo | `83f3692` (vacío) |
| `Claim-Id` | `ea5d0ad2-b61b-424c-ab1b-96678eca70a3` |
| Primer push | aceptado, sin `--force` |

---

## 3. Primitivas: reutilizadas y añadidas

**Reutilizadas tal cual** — el mapa se hizo *antes* de escribir código, precisamente para no fundar
infraestructura paralela:

- `Application.Geometry.Point2D` (con `ApproxEquals`): **el único** punto 2D del repositorio.
- `UI.Controls.PreviewProjection`: **el único** ajuste mundo→lienzo. I-30 ya tuvo que unificar
  proyecciones duplicadas; no se creó otra.
- `UI.Controls.PreviewPalette` y `PreviewCanvas`.
- Patrón de materialización de I-16 (`BlockPlacement`, `SystemBlockWriter.ApplyRegen`,
  `SymbolUtilityServices.GetBlockModelSpaceId`): se **reutiliza el patrón**, no se copia la
  infraestructura de racks.

**Añadidas, todas aditivas** en `RackCad.Application.Geometry` — nada movido, nada renombrado:
`Vector2D` + `GeometryTolerance`, `Bounds2D`, `Transform2D`, `PathSegment2D` (línea y arco),
`ClosedContour2D`, y `Point3D`/`Vector3D`/`LocalFrame3D`.

Dos decisiones que conviene tener a la vista:

- **`PathSegment2D` es un `struct` con discriminador**, no una jerarquía de clases. La firma del plan
  necesita igualdad por valor, y una jerarquía habría metido asignaciones en el camino caliente del
  teselado.
- **Un arco se construye solo por centro/radio/ángulo inicial/barrido con signo**, y sus extremos se
  derivan. Construirlo por extremos permite estados imposibles (dos puntos y un radio que no los une).

## 4. Contrato de ejes

| Concepto | Convención |
|---|---|
| Plano de la sección | XY local |
| Eje longitudinal | Z local |
| Origen transversal | **centroide** |
| Contorno exterior | antihorario |
| Huecos | horario |

La orientación se **normaliza al construir**, no se confía en que cada constructor acierte. El centrado
de los asimétricos usa los `x`/`y` **tabulados** por AISC, no un centroide recalculado.

## 5. Política de fidelidad

Nivel de detalle (lo que se pide) y fidelidad (lo que se obtuvo) son cosas distintas y viajan por
separado:

| Fidelidad | Significado |
|---|---|
| `Simplified` | Se pidió simplificada |
| `TabulatedComplete` | Tabulada y no falta nada que la fuente publique |
| `TabulatedDerived` | Tabulada, pero la fuente no publica todo el detalle real |
| `DegradedToSimplified` | Se pidió tabulada, faltó un dato, y **se dice** |

**Resultado medido sobre la v16.0 completa:**

| Fidelidad | Secciones |
|---|---|
| `TabulatedComplete` | **289** (todas las W) |
| `TabulatedDerived` | **694** (HSS, C y L) |
| `DegradedToSimplified` | **0** |

`StructuralSectionGeometry.Create` **rechaza** un resultado que se declare degradado sin un
diagnóstico de severidad `Degraded`: la honestidad no es una convención, está comprobada en el tipo.

## 6. Reglas por familia

**Se deriva, con regla documentada:**

| Radio | Regla | Familias |
|---|---|---|
| Filete de raíz | `r = kdes − tf` | W, C, L |
| Esquina exterior HSS | `r = (Ht − h)/2 ≈ (B − b)/2` | HSS |
| Esquina interior HSS | exterior − `tnom` | HSS |

La derivación del filete vive en **un solo sitio** (`SectionGeometrySupport.DeriveRootFillet`), que
además comprueba que el radio **quepa** antes de aplicarlo. Las dos expresiones del radio del HSS deben
coincidir dentro de tolerancia (`CornerRadiusAgreementTolerance = 0.10`); si no, se emite diagnóstico.

**No se deriva, y por eso se declara:**

- **Redondeo de la punta del ala** (W, C, L): AISC no lo publica. Las puntas quedan vivas. Por eso la W
  es la **única** familia que alcanza `TabulatedComplete` —su punta viva es lo único que le falta y la
  fuente tampoco la publica—, y C y L declaran `TabulatedDerived` **siempre**.
- **Conicidad del ala del canal**: el contorno usa el espesor medio tabulado.

**Orientaciones canónicas, acreditadas y no elegidas por gusto:**

- **Canal**: alma al `−X`, alas abriendo hacia `+X`.
- **Ángulo**: **ala larga vertical**, ala corta horizontal, talón en el origen de construcción. Se
  acreditó aritméticamente sobre `L8X6X1`: construido así, el centroide calculado cae en
  `(1.654, 2.654)`, que son exactamente los `x = 1.65` / `y = 2.65` tabulados. Al revés daría
  `(2.654, 1.654)` y **transpondría en silencio todos los ángulos desiguales**.

## 7. Error de área, medido y explicado

Medido contra el `A` publicado, **sin manipular la geometría para mejorarlo**:

| Familia | n | Máximo | Media | Filas > 5 % |
|---|---|---|---|---|
| W | 289 | **0.732 %** | 0.118 % | 0 |
| HSS rect./cuad. | 525 | **10.927 %** | 8.34 % | 525 |
| C | 32 | **5.545 %** | 2.812 % | 3 |
| L | 137 | **3.012 %** | 0.814 % | 0 |

**HSS — es una diferencia de definición, no un defecto.** AISC calcula `A` con el espesor de **diseño**;
la decisión 11 del dueño fija el **nominal** para la geometría, que es lo que se dibuja y lo que mediría
alguien con un calibrador. `Hss_MatchesTheTabulatedAreaWhenTheSameContourUsesTheDesignThickness`
reconstruye **el mismo contorno** con `tdes`:

| Con | Máximo | Media | Filas > 5 % |
|---|---|---|---|
| `tnom` (lo que se dibuja) | 10.927 % | 8.34 % | 525 |
| `tdes` (lo que AISC tabula) | **4.581 %** | **1.068 %** | **0** |

El contorno es correcto; lo que difiere es qué espesor significa cada número.

**Canal — 3 filas de 32**, por el redondeo de punta que la fuente no publica. Modelarlo con un `r/2`
plausible habría bajado el error a 3.895 %. **Se rechazó expresamente**: sería inventar un radio, que es
justo lo que la decisión 13 prohíbe.

## 8. Centroides

| Familias | Residuo |
|---|---|
| W y HSS (simétricas) | **< 1e-9** in |
| Canal (peor caso) | 0.0373 in — 1.244 % del tamaño, en `AISC-C-C3X3_5` |
| Ángulo (peor caso) | 0.0099 in — 0.329 % del tamaño, en `AISC-L-L3X3X3_16` |

El residuo de los asimétricos es la diferencia entre el `x`/`y` que AISC **tabula redondeado a tres
cifras** y el centroide del contorno derivado. Se mide y se reporta; no se corrige moviendo la sección,
porque el valor tabulado es la autoridad.

El área y el centroide se calculan de forma **analítica exacta** para líneas y arcos (polígono de
cuerdas + segmento circular `r²/2·(Δθ − sen Δθ)`, con el centroide del casquete en
`4r·sen³(θ/2)/(3(θ − sen θ))` sobre la bisectriz). Deliberado: así la regresión de área **no depende de
una tolerancia de teselado**.

## 9. Vistas, teselado y plan neutral

Cinco vistas: `CrossSection`, `LongitudinalX`, `LongitudinalY`, `Isometric` y personalizada.
**No** se llaman frontal/lateral/planta: esos nombres ya significan otra cosa en los cuatro sistemas
vigentes.

Las cámaras se construyen con `LocalFrame3D.Camera(viewDirection, upReference)`, que deriva
`right = up × forward` y `trueUp = forward × right` y por tanto es **dextrógira por construcción**. Se
añadió tras un fallo real: elegir los tres ejes a mano producía tripletas levógiras que reventaban en el
inicializador estático.

Lo que se dibuja es un **wireframe sin eliminación de líneas ocultas**, y está dicho en lugar de
arreglado: un paso de HLR es caro, frágil, y cuando falla produce dibujos sutilmente equivocados en vez
de obviamente rotos.

Las curvas llegan **teseladas** con tolerancia de cuerda declarada (`0.001` in por defecto). En la vista
de sección un arco es un arco, pero en cualquier vista oblicua la proyección de un círculo es una
**elipse**, y emitirla como arco sería callar un error.

`StructuralSectionRepresentationPlan` es el **único** artefacto neutral: curvas ya proyectadas con rol
semántico (contorno, hueco, perfil de extremo, generatriz, eje, envolvente), límites, fidelidad,
diagnósticos y **firma determinista**.

La firma es una **huella del plan** —puntos *y* orden de recorrido—, no una clase de equivalencia
geométrica. Dos pruebas costaron aprenderlo y quedaron escritas para que nadie lo vuelva a confundir:
un espejo **sí** cambia la firma aunque la figura se vea igual (invierte el recorrido), y en la vista
que descarta el eje espejado **no** la cambia (esa coordenada no llega a la proyección).

## 10. Preview e inspector

`StructuralSectionInspectorState` concentra todo el estado y **no conoce WPF**: se prueba sin
dispatcher, igual que I-20 e I-21 hicieron con los editores selectivo y dinámico.
`StructuralSectionPreview` extiende `PreviewCanvas` y estiliza **por rol**, nunca por dimensión.

El inspector ofrece **solo secciones habilitadas**. Una deshabilitada sigue resolviendo por id —un
diseño guardado debe seguir dibujando (I-36A, decisión 15)— pero no se vuelve a ofrecer.

**No es un configurador de miembro**: sin rol, material, cargas, niveles, brazos, conexiones,
fabricación, guardado ni round-trip. Una prueba de frontera lo vigila por reflexión y otra fija que
`RackCad.UI` **no referencia ningún ensamblado de Autodesk**.

## 11. Comando y materialización

`RACKSECCION` → carga el catálogo → inspector → punto de inserción → bloque interno.

| Decisión | Qué hace |
|---|---|
| **Falla cerrada** | Un catálogo de secciones inválido **detiene** el comando. Distinto a propósito del catálogo de producto, que degrada a vacío: dibujar una viga con dimensiones no validadas es peor que no dibujarla |
| **Bloque interno** | Definición creada en **este** dibujo. Sin `blocks-library.dwg`, sin filas en `blocks.csv`, sin bloque previo |
| **El nombre es una salida** | Legible (`RACKCAD_SECCION_<id>_<vista>_L<largo>`), y **nada se resuelve por él**. Una colisión toma el siguiente nombre libre en vez de redefinir el bloque de otro |
| **Sin caché global por longitud** | Una longitud libre haría crecer el dibujo sin límite |
| **Punto antes de crear** | Cancelar no deja un bloque fantasma que luego haya que limpiar. Se evita el problema en vez de repararlo |
| **Una transacción** | Definición y referencia se confirman juntas; una excepción no deja nada parcial porque nunca hubo commit |
| **Regen canónico** | `SystemBlockWriter.ApplyRegen`, no un `Editor.Regen()` suelto |
| **BYBLOCK en capa 0** | Fijada explícitamente: una entidad nace en la capa actual, y heredar `CLAYER` habría roto el BYBLOCK en silencio |
| **Anotaciones aparte** | Eje y envolvente en `RACKCAD_ANOTACIONES`, la capa que ya usa el resto de RackCad, para poder congelarlas sin perder la pieza |
| **No es un rack** | Sin payload, sin GUID, sin round-trip. `RACKEDITAR` no lo reconoce, y no debería |

El adaptador **no calcula geometría**: recibe puntos y roles. Las guardas de código comprueban que no
menciona ninguna factoría, constructor, tipo de dimensiones ni `Math.`; si algún día lo hiciera, el
preview y el dibujo habrían pasado a ser dos implementaciones que hoy coinciden.

## 12. Pruebas, builds y bundle

| Suite / gate | Resultado |
|---|---|
| `RackCad.Tests` | **2043 / 2043** (base 1851 → **+192**; ronda 2: 1992 → **+51**) |
| `RackCad.UI.Tests` | **523 / 523** (**+29**; ronda 2: 521 → **+2**) |
| `dotnet build src/RackCad.Application` Debug | 0 errores |
| `dotnet build src/RackCad.UI` Debug | 0 errores |
| `dotnet build src/RackCad.Plugin` Debug | 0 errores propios (2 avisos `MSB3277` preexistentes por la unificación de `System.Drawing` entre los ensamblados de AutoCAD y el ref-pack de net8.0) |
| `deploy/build-bundle.ps1` | **OK: 147 comprobaciones.** DLL idénticos al publish, catálogos idénticos a `assets/catalogs` (SHA-256), solo archivos RackCad, **cero DLL de Autodesk** |
| Validación del catálogo de I-36A | 29/29 verde, 983 secciones, catálogo == manifiesto |
| **CI sobre el SHA publicado** | **verde 4/4** — run [`30377854819`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30377854819) sobre `a1360ec`: *Tests (Domain + Application)*, *UI Tests (WPF controls, net8.0-windows)*, *Build UI* y *Build Plugin without AutoCAD*. Ronda 1: `30374044447` sobre `69c11eb` |

El único commit posterior a `a1360ec` es **solo documentación** —esta tabla y el estado versionado—, sin
una línea de código ni de catálogo; su propia corrida de CI queda en §17.

**Reparto de las 221 pruebas nuevas:**

| Suite | Nuevas | Qué fija |
|---|---|---|
| `GeometryPrimitivesTests` | 34 | Vectores, límites, transformaciones, arcos, teselado, área/centroide analíticos, marcos 3D |
| `StructuralSectionGeometryTests` | 18 | **Las 983** en los dos niveles: validez, cierre, centrado, área por familia, fidelidad, caché, diagnósticos no silenciosos |
| `StructuralSectionGeometrySentinelTests` | 12 | Sentinelas de las cuatro familias, **incluido el ángulo desigual** `L8X6X1` |
| `StructuralSectionProjectionTests` | 38 | Instancia prismática, cinco vistas, rotación/espejo, modos, teselado y firma |
| `StructuralSectionPluginSourceGuardTests` | 43 | El lado AutoCAD leído como texto (§13), incluida la costura de `Closed` |
| `StructuralSectionWireframeIntegrityTests` | 47 | **Ronda 2**: generatrices interiores, espesor medible, proyecciones que colapsan, aristas repetidas, perfiles legibles, y las 983 en las cuatro vistas |
| `StructuralSectionInspectorTests` (UI) | 29 | Catálogo, filtros, longitud inválida, vista, detalle, rotación, espejo, recomputación, peso, fidelidad, fronteras, y que el preview nunca recibe una curva degenerada |

## 13. Por qué el lado AutoCAD se prueba como fuente

`RackCad.Tests` **no puede** cargar el Plugin: referencia AutoCAD (ADR-0003) y la CI no lo tiene. Es el
mismo problema que ya resolvió `PushBackPluginSourceGuardTests`, y se usa el mismo patrón: leer los
`.cs` como **texto** y fijar lo que solo existe ahí —comando, fallo cerrado, orden punto→creación, una
sola transacción, bloque interno, BYBLOCK, capa de anotaciones, y las prohibiciones (sin biblioteca de
bloques, sin sólidos 3D, sin bloque dinámico, sin payload de rack, sin nada de I-37)—.

Las guardas negativas comparan contra el archivo **sin comentarios**: ambos archivos *explican* por qué
no usan `blocks-library.dwg`, y una guarda que prohibiera nombrar aquello que rechaza habría expulsado
el razonamiento del código.

**Lo que un guarda de texto no puede comprobar es cómo se ve el dibujo.** Eso es §14, y no lo sustituye
nada de lo anterior.

## 13.b Ronda 2 — lo que la validación del dueño rechazó, y cómo quedó

El gate `owner-validation` de la primera entrega quedó **rechazado parcialmente**. Cuatro puntos.

### F1 — Un tubo se dibujaba como una barra maciza

`AddWireframe` generaba perfiles de extremo para **todos** los contornos, pero generatrices sólo desde
`contours[0]`. El hueco existía en la sección y desaparecía en cuanto la pieza tenía longitud.

Ahora todos los contornos aportan generatrices, y hay **roles separados** —`Generatrix` /
`InteriorGeneratrix` y `EndProfile` / `EndProfileHole`— en vez de un rol con una bandera, para que un
consumidor los distinga sin volver a deducir de qué contorno salió cada curva.

Medido en `HSS4X4X1/4` a 120 in, `LongitudinalX`: seis rectas longitudinales en **−2, −1.75, −1.65,
1.65, 1.75 y 2**. Entre ±2 y ±1.75 hay **0.25 in = tnom**, y ahora se mide con `DIST`. Seis y no ocho
porque los arcos interior y exterior son **concéntricos**: sus tangencias caen a la misma altura
(`2 − 0.35 = 1.75 − 0.10 = 1.65`) y ahí las dos líneas son la misma tinta. El espesor sobrevive a
rotación de 90° y a espejo.

W, C y L no ganan nada: no tienen hueco, y una prueba lo fija por rol **y** por recuento de rectas en
las tres vistas donde podría colarse.

### F2 — Perfiles proyectados que colapsaban y seguían cerrados

Mirando exactamente a lo largo de X o de Y el contorno se ve de canto y colapsa de figura a recta. Se
seguía emitiendo **cerrado**: en AutoCAD, una polilínea de área cero que recorre cada arista de ida y de
vuelta y que no hay forma de seleccionar.

Ahora se calcula la **dimensionalidad** de la proyección después de proyectar. Si conserva área sigue
cerrada; si colapsa se reduce a sus segmentos abiertos únicos —sin solapes, sin recorridos inversos
duplicados, sin puntos colineales redundantes, sin tramos de longitud cero—, conservando los roles y con
salida determinista.

La invariante vive en el **tipo**: `SectionPlanCurve` rechaza una curva cerrada cuya proyección es
unidimensional. Como el materializador copia `curve.IsClosed` tal cual —y una guarda de fuente fija que
`Closed` no se asigna desde ninguna otra expresión—, **no queda camino** por el que una polilínea
cerrada de área cero llegue al dibujo. Ésa es la costura que pedía la revisión.

Dos efectos del mismo pase, ambos correctos: dos generatrices colineales se funden en una (en isométrica
los ocho vértices del HSS caen sobre sólo cuatro rectas y sus tramos se solapan), y la boca del tubo deja
de dibujarse aparte en vista de canto porque cae **dentro** de la cara de corte. Lo que delata el hueco
ahí son sus generatrices interiores.

También deja de emitirse una **envolvente cerrada de área cero**, alcanzable desde la UI con modo *Eje*
más *mostrar envolvente* en una vista longitudinal.

### F3 — Dos ideas compartiendo el nombre «centroide»

| Antes | Ahora | Qué es |
|---|---|---|
| — | `Origin` | `(0,0)`. La autoridad de colocación |
| — | `OriginBasis` | `Symmetry` (W, HSS) o `TabulatedCentroid` (C, L) |
| `Centroid` | `GeometricContourCentroid` | El centroide del contorno **aproximado**. Diagnóstico |
| `CentroidOffset` | `GeometricCentroidResidual` | Cuánto se separan |
| `SectionReferencePointKind.Centroid` | `…TabulatedCentroid` | El punto que publica la fuente |

**No se movió ninguna geometría.** Los residuos son los mismos que en la primera entrega. El residuo es
una métrica y nunca una autoridad de colocación: anularlo moviendo la pieza sustituiría a AISC por
nuestro propio contorno incompleto. Una prueba de vocabulario impide que vuelva a existir un miembro
público llamado sólo `Centroid`.

### F4 — Decisión del dueño sobre los canales

Registrada en [`../decisions/I-36B.md`](../decisions/I-36B.md): fidelidad `TabulatedDerived` aceptada,
error máximo de 5.545 % conocido (3 de 32 filas), causa acreditada en la conicidad y el radio de punta
que AISC no publica, prohibición expresa de inventar geometría para forzar el área, y constancia de que
la excepción **no** convierte la fidelidad en `TabulatedComplete`.

### Rojos observados antes de arreglar

Las regresiones se escribieron primero y se vieron fallar: **34 de 42** en
`StructuralSectionWireframeIntegrityTests` sobre la base `4444027`. Tres de ellas eran de la propia
prueba, no del producto, y se corrigieron con su razón anotada: la vista isométrica **escorza** el eje
(120 in se dibujan como 97.98), así que una generatriz no se reconoce por su longitud sino por su
dirección; y varios vértices caen sobre la misma recta proyectada, de modo que «un vértice, una línea»
es falso incluso cuando el dibujo es correcto.

Una prueba anterior, `AnHssDrawsItsInteriorInALongitudinalViewToo`, exigía cuatro `EndProfile` y con eso
fijaba **dos** errores a la vez: llamaba perfil exterior al hueco, y contaba como acierto justo las
curvas degeneradas. Reescrita.

### Invariantes que la ronda 2 no tocó

289 W · 525 HSS · 32 C · 137 L · **983** · 289 `TabulatedComplete` · 694 `TabulatedDerived` · **cero**
degradadas · mismos errores de área por familia · mismos residuos de centroide · catálogos y manifiesto
de I-36A sin una línea · sin cambios en sistemas · sin `blocks.csv` · sin `blocks-library.dwg` · sin
sólidos 3D · sin I-37.

## 14. Checklist de AutoCAD para el dueño

### 14.a Smoke focalizado — hacer esto PRIMERO

Cinco comprobaciones sobre lo que la ronda 2 arregló. Si alguna falla, no tiene sentido seguir.

| # | Qué hacer | Qué debe verse |
|---|---|---|
| 1 | `HSS4X4X1/4`, 120", *Longitudinal X* | **Cuatro** caras: dos exteriores y dos interiores. El espesor se mide con `DIST` y da **0.25"**. Los perfiles de extremo son **una línea recta cada uno**, seleccionable, no una polilínea que se pisa a sí misma |
| 2 | El mismo HSS, *Isométrica* | El hueco aporta sus propias líneas longitudinales, distintas de las exteriores. Las bocas de los dos extremos se ven como contornos cerrados con área |
| 3 | `W12X26`, 120", *Longitudinal X* | Perfiles de extremo legibles: **una** recta por extremo, de altura 12.22". **Ninguna** polilínea cerrada de área cero — selecciónala y comprueba que `LIST` no reporta `Closed` |
| 4 | `C10X15.3`, 120", *Longitudinal Y* | Un solo perfil de extremo por lado, abierto, y sin aristas repetidas |
| 5 | `L8X6X1`, 120", *Longitudinal X* | Ídem, y el ala larga sigue siendo la vertical |

### 14.b Checklist completo (12 puntos)

Sólo después de que los cinco anteriores pasen.

Con el DLL **Debug de este worktree**, según
[`../../guias/validacion-manual-autocad.md`](../../guias/validacion-manual-autocad.md). El comando se
escribe: `RACKSECCION` (aún no aparece en `RACKAYUDA`; ver §16).

| # | Qué comprobar | Qué debe pasar |
|---|---|---|
| 1 | **W** — busca `W12X26`, vista *Sección*, detalle *Tabulada*, inserta | Perfil en I con filetes redondeados en las cuatro esquinas interiores y puntas de ala vivas |
| 2 | **HSS** — `HSS4X4X1/4`, misma vista | Cuadrado con esquinas redondeadas por fuera y por dentro, y **hueco** interior |
| 3 | **C** — `C10X15.3` | Canal con el alma a la izquierda y las alas abriendo a la **derecha** |
| 4 | **L** — `L8X6X1` (ángulo **desigual**) | El ala **larga** queda **vertical** y la corta horizontal |
| 5 | **Cuatro vistas** sobre la misma sección | *Sección*, *Longitudinal X*, *Longitudinal Y* e *Isométrica* dan cuatro dibujos distintos y coherentes |
| 6 | **Rotación ortogonal**: 90° en vista *Sección* | La pieza gira un cuarto de vuelta; alto y ancho se intercambian |
| 7 | **Rotación NO ortogonal**: 30° | Gira 30° reales, sin deformarse ni degradarse |
| 8 | **Espejo** sobre el canal | Las alas abren hacia el lado contrario |
| 9 | **Los dos detalles** en la misma W | *Simplificada* tiene esquinas vivas; *Tabulada* tiene filetes. La ficha de fidelidad cambia en consecuencia |
| 10 | **Eje y envolvente** | Aparecen en `RACKCAD_ANOTACIONES`; congelar esa capa los oculta y **deja la pieza** |
| 11 | **Dos longitudes** (p. ej. 48" y 240") en vista longitudinal | El largo cambia, la sección transversal **no**; el peso total escala proporcional |
| 12 | **Escala, selección y limpieza** | Mide una dimensión conocida con `DIST` y sale en **pulgadas**; el bloque se selecciona como una unidad; **`RACKSECCION` + Esc en el punto no deja ninguna entidad**; `blocks.csv` sin filas nuevas y `blocks-library.dwg` sin tocar; abre un dibujo con racks existentes y comprueba que **nada cambió** |

### 14.c Resultado — APROBADO por el Owner

```yaml
owner_validation:
  status: approved
  date: 2026-07-28
  owner: Mario Pérez
```

| Campo | Valor |
|---|---|
| Smoke focalizado (§14.a, 5 puntos) | **aprobado** |
| Checklist completo (§14.b, 12 puntos) | **aprobado** |
| SHA técnico validado | `30ef95c56c9ce6d3120e13c29f971c40dd65fbec` |
| Bloqueos | **ninguno** |
| `requires_owner_validation` | **satisfied** |
| `requires_autocad` | **satisfied** |
| ADR-0022 | **aceptado** — Mario Pérez, Owner, 2026-07-28 |

Aprobados sobre el dibujo real: la geometría de las cuatro familias; las cuatro vistas; las generatrices
exteriores **e interiores** del HSS; la representación de su espesor nominal; los perfiles de extremo
canonicalizados, **sin polilíneas cerradas degeneradas**; rotación y espejo; la materialización por
`RACKSECCION`; los **bloques internos** del dibujo; la ausencia de cambios en `blocks-library.dwg` y
`blocks.csv`; el **plan neutral único** que consumen igual el preview y AutoCAD; el contrato de origen y
centroide; y la fidelidad con sus diagnósticos visibles.

#### Observación visual sobre los canales C

El Owner comparó los canales de RackCad con perfiles comerciales de librerías CAD y constató que **no
reproducen completamente su apariencia**: falta la **conicidad de los patines**, los **redondeos o
chaflanes de punta** y las **transiciones características del laminado**. La comparación **confirma** que
esa diferencia es la que explica el error de área conocido de la familia.

Aceptado expresamente: los canales permanecen **`TabulatedDerived`**; el error máximo medido de
**5.545 %** **no bloquea** I-36B; **no se inventan** radios, chaflanes ni conicidades dentro de esta
iniciativa; la geometría actual es **técnicamente honesta y suficiente como fundación**; y el objetivo
visual mejorado se atenderá en una iniciativa futura.

**Esta evidencia NO afirma que los canales sean geométricamente idénticos a un perfil de librería
comercial.** Su fidelidad es **derivada** y la diferencia es conocida, medida y documentada.

#### Requisito futuro obligatorio — Perfiles IPS/S y geometría visual mejorada

Registrado por decisión del Owner, **no** como hallazgo opcional. Una iniciativa futura y separada
deberá: incorporar perfiles **IPS** verificando su correspondencia con la familia AISC `S` o con el
catálogo comercial de la empresa; importar su fuente y dimensiones; modelar la **inclinación de los
patines**; representar radios, chaflanes y transiciones **cuando exista una regla acreditada**; mejorar
visualmente C y los demás laminados; mantener **separadas** la geometría tabulada y la visual;
**declarar** cuándo una geometría visual es aproximada; **no sustituir ni alterar** la geometría tabulada
de I-36B; y **no mezclarse con Cantilever** salvo que su contrato lo exija.

**No se abrió** rama, contrato ni worktree para ella. Queda registrado en
[`../../ideas-futuras.md`](../../ideas-futuras.md), en
[`../decisions/I-36B.md`](../decisions/I-36B.md), en
[ADR-0022](../../adr/0022-geometria-parametrica-de-secciones-estructurales.md), en
[`../state/I-36B.yml`](../state/I-36B.yml) y en la
[guía](../../guias/geometria-secciones-estructurales.md) §11.b.

## 15. Diff y guardas de alcance

`git diff --name-only origin/main..HEAD` → **34 archivos, +7 144 líneas, 0 eliminadas.**

| Guarda | Resultado verificado |
|---|---|
| `assets/**` (incl. `secciones.csv`, `blocks.csv`) | **cero líneas** |
| `blocks-library.dwg` | **no tocado**, ningún bloque de biblioteca creado |
| `src/RackCad.Domain/` | **cero archivos** |
| Sistemas vigentes de UI y Plugin | **cero cambios** (todo lo nuevo son archivos nuevos) |
| `deploy/`, `.github/` | **cero archivos** |
| `docs/ROADMAP.md`, `docs/HANDOFF.md` | **cero líneas** (§16) |
| `main` | **intacta** |

**Confirmación expresa de lo que NO se hizo:** no hay I-37 ni Cantilever; no hay `StructuralMember`,
postes, brazos, largueros, celosías ni bases; no hay conexiones, troqueles, perforaciones, placas,
soldaduras, cortes de extremo ni reglas de fabricación; no hay cálculo resistente ni cargas; no hay
sólidos 3D, `Region`, `Solid3d`, extrusión ni sweep; no hay bloques dinámicos; no hay migración de
catálogos ni de sistemas; no hay filas nuevas en `blocks.csv` ni cambios en `blocks-library.dwg`.

## 16. Dos correcciones de proceso que hizo esta ejecución

**(a) ROADMAP no se toca desde esta rama.** La versión inicial del contrato listaba `docs/ROADMAP.md`
entre los archivos modificados. [`WORKFLOW.md`](../../WORKFLOW.md) §8 dice lo contrario sin ambigüedad
—«Nunca HANDOFF/ROADMAP desde ramas paralelas»— y WORKFLOW tiene precedencia (§10 de WORKFLOW). Se
corrigió el contrato y **no se tocó ROADMAP**: la fila de I-36B queda `pendiente` y su actualización
pertenece a la sesión de integración. I-36A sí escribió en ROADMAP, pero con autorización expresa del
dueño registrada en su decisión; I-36B no la tiene y no la pide.

**(b) `README.md` sí, `RackCommandReference` no.** WORKFLOW §8 obliga a actualizar `README.md` **en la
misma rama** cuando cambian los comandos de AutoCAD, así que `RACKSECCION` está en su tabla. La
referencia que muestra `RACKAYUDA` (`src/RackCad.UI/RackCommandReference.cs`) **no** se tocó: es UI
vigente y queda fuera del alcance declarado. Al comprobarlo apareció que **`RACKPUSHBACK` también
falta** ahí desde I-32. Ambos quedan anotados en [`../../ideas-futuras.md`](../../ideas-futuras.md), que
es donde el contrato §4 manda los hallazgos fuera de alcance —no se corrigen de paso—.

## 17. Commits

| SHA | Fase |
|---|---|
| `83f3692` | 1 — reclamo atómico |
| `2c65958` | 2 — contrato, decisión versionada y ADR-0022 `propuesto` |
| `d1a01f0` | 3 — primitivas neutrales aditivas |
| `6cb407e` | 4-5 — geometría transversal de las cuatro familias |
| `d241e03` | 6 — instancia prismática, proyección y plan neutral |
| `4b66bf5` | 6 — precisión sobre la firma del plan |
| `2185962` | 7 — preview e inspector |
| `3dce1f9` | 8 — materialización y `RACKSECCION` |
| `67fec4a` | 9 — diagnósticos no silenciosos |
| `69c11eb` | 10 — guía, evidencia, índices y estado — CI verde 4/4, run `30374044447` |
| `4444027` | 10 — registro de esa CI; **HEAD revisado por el dueño** |
| `186bf31` | R2 — F1 generatrices interiores + F2 canonicalización de proyecciones |
| `443d8fc` | R2 — F3 contrato del origen frente al centroide del contorno |
| `6142284` | R2 — F4 decisión del dueño sobre los canales |
| `a1360ec` | R2 — ADR, guía, evidencia y estado — **CI verde 4/4**, run `30377854819` |
| *(este)* | R2 — solo documentación: registra la CI verde de `a1360ec` |

**Nota honesta sobre dos commits.** `d241e03` se hizo con la suite en **rojo**: encadené el `commit` a
un `grep` que devolvió éxito por *encontrar texto*, no por una suite verde. Corregido en `4b66bf5`, y
desde entonces cada commit pasa por una comprobación explícita de `Con error: 0`. Además, el cuerpo de
`6cb407e` dice «Suite completa: 1931 verde» cuando la cifra real era **1910**.

## 18. Estado final

- **`review-ready`**, gate **`owner-validation`**.
- **ADR-0022 sigue `propuesto`.** Solo el dueño lo acepta, y después de la validación en AutoCAD.
- **No integrada, no limpiada**: rama y worktree se conservan. `main` intacta.
- Sin Pull Request: el repositorio integra por `git merge --no-ff` desde una sesión de integración.

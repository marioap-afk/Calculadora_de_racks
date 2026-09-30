# I-55 G16 C16-06 — Orientacion proyectada de la referencia de bloque (diseño correctivo)

Frozen: YES

Initiative: I-55 (View Placement & Projection), gate G16, correctivo C16-06. Workflow: V1 (reclamo I-55).

Autoria: sesion unica con cambio de rol **autorizado por el Owner para esta ejecucion** (`SAME-SESSION ROLE = OWNER AUTHORIZED FOR THIS EXECUTION`);
las transiciones de rol se registran en [`docs/automation/decisions/I-55.md`](../automation/decisions/I-55.md) §G16-C16-06.

Base: `feature/creacion-de-vistas` reconciliada sobre `main` `1304101d` (AUTH-15-C1 integrada), con C16-05 retirada.

## 1. Problema

`RACKPROYECTAR` crea vistas ligadas validas, pero su orientacion no es la de una vista proyectada. La caracterizacion del codigo actual (seccion 3) muestra que
una proyeccion ortografica (Planta ↔ Frontal/Lateral) coloca SIEMPRE la referencia con rotacion 0 (marco destino universal, OD-6.b A) aunque la vista fuente este girada,
y que una proyeccion de la misma clase (Rigid) copia la rotacion de cada fuente. El Owner exige (C16-06) un modo **Proyectada** por defecto y un modo **Predeterminada** explicito.

Restriccion dura del Owner: la definicion de bloque NO cambia. No se gira ni se regenera geometria, texto, cotas ni anotaciones dentro de la definicion. **Solo gira
la `BlockReference` colocada**, entera (geometria, textos, cotas, numeros y simbolos giran con ella). No se intenta mantener los textos derechos.

## 2. Autoridades consumidas (sin cambio)

- **AUTH-05 (marcos):** `RackViewFrame.AxisMap` dice que eje fisico (Run/Depth/Height) va sobre el X y el Y **locales** de la definicion y con que signo.
  Por sistema (adaptadores de `RackViewFrameAdapters`): Selectivo, Dinamico y Push Back — Planta `DepthRun` (X = Depth, Y = Run), Frontal `RunHeight`,
  Lateral `DepthHeight`; Cabecera — solo Planta `DepthRun` y Lateral `DepthHeight` (no expone Frontal); Cantilever — Planta `RunDepth` (X = Run, Y = Depth),
  Frontal `RunHeight`, Lateral `DepthHeight`. Todos los signos son positivos.
- **AUTH-08 V2 (hechos de colocacion de la fuente):** la parte lineal aceptada `L` de cada referencia fuente (`RackSourcePlacementAcceptance.Linear`), que ya incluye la
  media vuelta planar que reporta la Foundation. `RotationRadians = L.RotationAngle()`.
- **G14:** `RackProjectionClassMapping` (eje conservado `K` por par de clases), `SourceGroupFrame` / `TargetGroupFrame` (orientaciones `φs`, `φt`: `φt = 0` por
  OD-6.b A y `φs = 0` en Rigid por OD-6.c A; entre clases la referencia de la ventana es `R(φs)·s`), `RackOrthographicPlacementPolicy` (linea comun `e` por la Ventana de Marco Relativo, direccion destino `w = R(φt)·t`, `α = ang(e→w)`, rotacion
  de toda vista `ρ = φt`), `RackRigidPlacementPolicy` (`ρ_i = θ_i + α`, con `α = 0`), `CommonTransform2D` (una sola transformacion rigida por operacion).
- **G15:** `RackProjectedPlacement.RotationRadians` → `AutoCadProjectionWriteScope.PlaceReference` → `BlockReference.Rotation` (el write scope NO calcula nada).

## 3. Caracterizacion del comportamiento actual (antes de C16-06)

Selectivo, una referencia en (10, 20), puntos base (0,0) y destino (500,300); rotacion de la referencia colocada `ρ` en grados:

| Par | fuente 0° | 90° | 180° | 270° | modo G14 |
|---|---|---|---|---|---|
| Planta → Frontal | 0 | 0 | 0 | 0 | Orthographic (`α` = −90, −180, −90, 180) |
| Planta → Lateral | 0 | 0 | 0 | 0 | Orthographic |
| Frontal → Planta | 0 | 0 | 0 | 0 | Orthographic |
| Lateral → Planta | 0 | 0 | 0 | 0 | Orthographic |
| Planta → Planta, Frontal → Frontal, Lateral → Lateral | 0 | 90 | 180 | −90 | Rigid (copia la rotacion de la fuente) |

Nota: la Ventana de Marco Relativo pliega el sentido de la linea comun (fuente 0° y 180° dan la misma `e`), por eso el G14 actual no distingue 0° de 180°.

## 4. Derivacion (unica, desde las autoridades)

Notacion, para cada vista fuente `i` con marco fuente `Fs_i` y marco destino `Ft_i`, y eje conservado `K`:

- `s_i` = direccion **local** de `+K` en la fuente (`RackViewFrameSemantics.TryAxisDirection(Fs_i, K)`), `t_i` = la del destino.
- `d_i = normalizar(L_i · s_i)` = direccion **en el dibujo** del eje fisico `+K` de la fuente (incluye la media vuelta de AUTH-08).

Una vista proyectada conserva el eje fisico que comparte con su fuente: en el dibujo, su `+K` apunta a donde apunta el `+K` de la fuente. Como la definicion no se
refleja (AUTH-08 rechaza reflexiones y la definicion no cambia), la unica rotacion rigida de la referencia destino que lo cumple es

```
ρ = ang(t → d)        (R(ρ)·t = d)
```

Es unica: el `AxisMap` fija `t` con su signo y `L` fija `d` con su signo, y una rotacion pura que lleva `t` sobre `d` es una sola. Los marcos de AUTH-05 forman
ternas directas en ambas familias (familia rack: Depth × Run = +Height; Cantilever: Run × Depth = +Height), de modo que conservar `+K` con una rotacion pura produce
un abatimiento real de primer o tercer diedro, y el eje NO conservado del destino (Height en una elevacion, el otro eje horizontal en una planta) queda
**determinado** por la derivacion, no elegido. Ejemplo: Selectivo, Planta a 0° → Frontal: `ρ = 90°` y la altura de la frontal apunta a −X. Que el usuario lea
el resultado como primer o tercer diedro solo depende del lado que elija; `ρ` es el mismo.

Se consideraron y **se rechazan**: (a) la «linea sin sentido» (plegar `d` y `−d` con la Ventana de Marco Relativo), que descarta el signo que AUTH-05 y AUTH-08 si
llevan e invierte `K` para la mitad de las orientaciones (seria una vista posterior, que un bloque nunca reflejado no puede mostrar); (b) «altura siempre hacia
arriba», que es exactamente la orientacion Predeterminada entre clases distintas. No hay dos convenciones que las autoridades no distingan.

Resultados por familia (`θ` = rotacion de la fuente; todos los signos positivos):

| Par | Selectivo / Dinamico / Push Back / Cabecera | Cantilever |
|---|---|---|
| Planta → Frontal (K = Run) | `ρ = θ + 90°` (Planta: Run = +Y local; Frontal: Run = +X) | `ρ = θ` |
| Planta → Lateral (K = Depth) | `ρ = θ` | `ρ = θ + 90°` (Planta: Depth = +Y local) |
| Frontal → Planta (K = Run) | `ρ = θ − 90°` | `ρ = θ` |
| Lateral → Planta (K = Depth) | `ρ = θ` | `ρ = θ − 90°` |

(Cabecera no expone Frontal; los pares se aplican solo donde ID19 los expone.)

## 5. Contrato congelable

### A. Proyectada (`Projected`)

- **Clases distintas (Orthographic).** Toda la operacion usa UNA orientacion proyectada comun: `d = d_0` (todas las `d_i` coinciden, regla E), y se fijan los marcos de grupo
  de G14 a partir de los hechos, no de un cero universal: `SourceGroupFrame.Orientation = φs = ang(s_0 → d)` (igual a la rotacion `θ_0` de la primera fuente) y
  `TargetGroupFrame.Orientation = φt = ang(t_0 → d)`. `φs` no se calcula de nuevo: es la `RotationRadians` de la parte lineal aceptada de la primera fuente
  (media vuelta incluida); `φt` se obtiene componiendo con el `Transform2D` compartido (`RotationAngle`), nunca con `Math.Atan2` (guarda existente de
  Application/Views). Con esos marcos, la politica ortografica existente produce `e = w = d`, `α = 0` y `ρ = φt` para **todas** las vistas:
  las vistas destino quedan alineadas sobre la recta que pasa por el punto destino en la direccion `d`, cada una desplazada segun la coordenada de su fuente sobre `d`
  (proyeccion pura), y todas giradas `ρ`. Los intervalos, el anclaje del extremo del tramo y el aviso de solapamiento no cambian.
- **Misma clase (Rigid).** Es la semantica G14 vigente (OD-6.c A, OD-7.a A, OD-2.b A): una sola transformacion rigida comun con `α = 0`; cada vista conserva su
  propia rotacion fuente (`ρ_i = θ_i`). La copia rigida de un grupo es por si misma una
  operacion coherente: no hay regla de divergencia en Rigid Proyectada.
- **Coherencia (definicion).** Una operacion es coherente si se realiza con UNA `CommonTransform2D` (Rigid) o con UNA linea comun orientada y UNA `ρ` (Orthographic).
  La misma definicion rige para Predeterminada.

### B. Predeterminada (`Canonical`)

- **Clases distintas:** exactamente el resultado G14/G15 actual (`φs = φt = 0`, `ρ = 0`, Ventana de Marco Relativo).
- **Misma clase:** presentacion canonica de RackCad mediante los marcos de grupo de G14, sin salir del modelo: `φs = θ_0` (rotacion de la primera fuente, leida de
  la parte lineal aceptada de AUTH-08, media vuelta incluida), `φt = 0` (marco destino universal), `α = φt − φs = −θ_0`, y UNA `CommonTransform2D` con esa `α`
  para todo el grupo: `ρ_i = θ_i + α = 0`. El punto base elegido cae en el punto destino (semantica de COPY) y la disposicion del grupo se conserva, girada `α`
  alrededor del punto base. Es la **correccion aceptada** de C16-06 a la interpretacion `α = 0` de Rigid, solo en Predeterminada (el Owner la anticipo:
  «If this requires altering prior Rigid alpha=0 interpretation, document C16-06 as the accepted correction»).
- **Predicado congelado de «rotaciones distintas»:** con `u_i = L_i · x̂` (el eje X local de cada referencia en el dibujo, media vuelta incluida), las rotaciones
  difieren si `|u_i × u_0| > GeometryTolerance.Angle` o `u_i · u_0 < 0`. Es modulo `2π` por construccion (no compara angulos: `π`, `−π` y `π + 2π` son la misma
  rotacion) y no usa `Math.Atan2`. Orden: las familias mezcladas se rechazan antes en Validate; despues este predicado. `ρ_i = θ_i + α` puede salir cerca de
  `±2π` en el limite de ±180°: es congruente con 0 y AutoCAD lo muestra como 0° (o 360°).
- **Misma clase con rotaciones distintas** (p. ej. racks espalda con espalda a 0° y 180°): no existe una transformacion rigida que deje todas las vistas en la
  presentacion canonica, y girar cada rack por su cuenta destruiria la operacion comun (prohibido por el Owner). La operacion **falla completa antes de los puntos**
  con el fallo tipado `SourceRotationsDiffer` (etapa Validate, un diagnostico por vista), cuyo remedio es usar Proyectada (conserva el giro de cada rack) o proyectar
  cada grupo de igual giro por separado. Este rechazo es propio de Predeterminada; no depende de la divergencia de Proyectada.
- Predeterminada **nunca** se bloquea por una divergencia que solo afectaria a Proyectada.

### C. Por defecto

`Projected` es el valor por defecto del **producto** (la pregunta de `RACKPROYECTAR`: Enter = Proyectada). En la API pura el modo es un parametro explicito de la solicitud;
el Plugin lo pasa siempre (guarda de fuente).

### D. Autoridad pura

Una autoridad pura de Application (sin tipos de AutoCAD) resuelve, para el modo, el modo de proyeccion (Rigid u Orthographic) y las vistas aceptadas, los marcos de
grupo `(φs, φt)` de la operacion o un fallo tipado; la politica existente los consume (`α = φt − φs` en Rigid; `e`, `w`, `α`, `ρ = φt` en Orthographic). El Plugin solo
aporta los hechos capturados y aplica `RackProjectedPlacement.RotationRadians` tal cual. `AutoCadProjectionWriteScope.PlaceReference` sigue siendo mecanico.
Nombres de simbolos: no congelados.

### E. Compatibilidad entre racks (solo Proyectada y clases distintas)

- Orden de comprobacion congelado: `FrameAxisMissing` → paralelismo (`|d_i × d_0| > GeometryTolerance.Angle` → fallo existente `NonParallelSources`) → sentido
  (`d_i · d_0 < 0` → `SourceOrientationDivergent`). La tolerancia se aplica al producto cruz; una vez paralelas, el signo del producto escalar decide. Las familias
  mezcladas ya se rechazan antes en Validate y la consistencia de los ejes destino la sigue comprobando la politica ortografica.
- Si dos o mas fuentes son paralelas pero de sentido opuesto (p. ej. racks espalda con espalda, uno girado 180°), no existe UNA orientacion proyectada comun: la operacion
  **falla completa antes de los puntos** con el nuevo fallo tipado `SourceOrientationDivergent` (etapa Validate), con un diagnostico por cada vista de la operacion, como el
  resto de fallos de Validate. No se gira cada rack por su cuenta.

### F. Fallos y avisos

- `SourceOrientationDivergent` (Proyectada, clases distintas) y `SourceRotationsDiffer` (Predeterminada, misma clase) son **bloqueantes**, antes de puntos,
  importacion y escritura. Remedios (familia `ReviewSelection`): el primero, proyectar por separado cada sentido o usar Predeterminada; el segundo, usar Proyectada o
  proyectar por separado cada grupo de igual giro. El rechazo obliga a repetir el comando (no se replanifica en otro modo: decision explicita, el comando no
  escribe nada antes de los puntos).
- Avisos existentes sin cambio: solapamiento, vista opcional sin bloque, y `DirectionWindowNearLimit` (solo puede aparecer en Predeterminada: en Proyectada la linea comun
  coincide con el marco, angulo relativo 0). No se añade ningun aviso nuevo.

### G. Interfaz (linea de comandos, sin ventana WPF)

Seleccion (sin cambio; durante la seleccion `F` sigue siendo Fence) → `Clase de vista a proyectar [Frontal/Lateral/Planta]` →
`Orientacion [PRoyectada/PREdeterminada] <PRoyectada>` (Enter = Proyectada; atajos distintos `PR` y `PRE`, porque las dos palabras del Owner empiezan por «P»;
el patron es el de `RACKCAMA`: `Keywords.Add` + `Default` + `AllowNone`; `Esc` cancela sin leer ni escribir) → lectura unica y plan (fallos y avisos) →
`Punto base` → `Punto de destino`. El informe final nombra la orientacion usada.

Mecanica congelada de la pregunta (el Plugin no se carga en CI y esta pregunta ya fallo una vez en host):
1. Patron probado de `RACKCAMA`: `PromptKeywordOptions` con un solo argumento de mensaje, `Keywords.Add` dos veces, `Keywords.Default` = la global de Proyectada y
   `AllowNone = true` (AutoCAD añade la lista y el valor por defecto). No se usa el constructor de dos argumentos ni `AppendKeywordsToMessage`.
2. Mapa de estados: `OK` o `None` → Proyectada, salvo que `StringResult` coincida (sin distinguir mayusculas; AutoCAD devuelve la global registrada) con la
   global de Predeterminada; `Cancel` o `Error` → la operacion termina antes
   de leer el dibujo, con el resultado nuevo `Cancelled` de la instantanea («cancelado: no se leyo ni se escribio nada»), no con «no se selecciono nada».
3. El ayudante de mapeo vive en el puerto, fuera de los metodos de punto; guardas de fuente fijan los literales y el orden.

### G.1 Alcance de las decisiones del Owner anteriores (por orientacion y por par de clases)

Registro en [decisions/I-55.md](../automation/decisions/I-55.md): OD-6.b A = `φt = 0` (X universal); OD-6.c A = `α = 0` en Rigid, traslacion como COPY;
OD-6.d A = orientacion «natural» de las plantas proyectadas desde elevaciones; OD-7.e A = Ventana de Marco Relativo (con el aviso de cercania OM-33).

| Decision | Proyectada, misma clase | Proyectada, clases distintas | Predeterminada, misma clase | Predeterminada, clases distintas |
|---|---|---|---|---|
| OD-6.b A (`φt = 0`) | vigente | **sustituida** por `φt = ang(t_0 → d)` | vigente | vigente |
| OD-6.c A (`α = 0` en Rigid, COPY) | vigente | no aplica | **sustituida** por `α = −θ_0` (seccion B, correccion aceptada) | no aplica |
| OD-6.d A (orientacion natural) | no aplica | **sustituida** (la vista sigue a su fuente) | no aplica | vigente |
| OD-7.e A (Ventana de Marco Relativo, OM-33) | no aplica | se ejecuta pero degenera (angulo relativo 0: sin aviso) | no aplica | vigente |

Las filas congeladas de OV-ID19 que describen la geometria de clases distintas se ejecutan en Predeterminada y las de la misma clase en Proyectada
(asignacion por fila en la guia de validacion §6).

### H. Sin cambios de definicion

`HeaderRunPlan`, `CantileverViewPlan`, los constructores de vistas, `RackDefinitionCreator` / AUTH-15, la Foundation y el esquema del sobre NO cambian. Si la implementacion
pareciera exigir girar geometria interna de un plan, se detiene: violaria el requisito del Owner.

### I. Persistencia

Ninguna. El modo es una eleccion por invocacion y no se guarda; la rotacion ya es una propiedad de la `BlockReference`; el sobre no cambia.

### J. Matriz de Validacion del Owner (OV-C16-06)

Filas `P-01..P-23` (Proyectada, incluidas las de Dinamico, Push Back, Cabecera, Cantilever y la misma clase) y `C-01..C-07` (Predeterminada y la pregunta,
incluidas `C-05b` y `C-05c`), con la rotacion esperada calculada con la seccion 4 y expresada en `[0°, 360°)`, en la guia de validacion de G16
([I-55-g16-owner-validation.md](I-55-g16-owner-validation.md) §6.1). Mapa: O-1 → P-01..P-16 y P-19..P-22 (rotacion); O-2 → P-17 (posiciones sobre la recta); O-3 → P-18; O-4 → C-01..C-04; O-5 → C-05, C-05b;
O-5b → C-05c; O-6 → P-23; O-7 → C-06; O-8 → la comprobacion `BEDIT`/`LIST` del preambulo de §6.1; O-9 → C-07. En Proyectada gira TODA la referencia: geometria, textos, cotas y etiquetas.

## 6. Obligaciones invariante → prueba (RED antes de implementar)

| ID | Invariante | Prueba | RED esperado sobre el codigo actual |
|---|---|---|---|
| O-1 | Proyectada ortografica: `R(ρ)·t = d` para toda vista, en los 5 sistemas y las 4 rotaciones | autoridad pura con marcos REALES de los adaptadores | falla (hoy `ρ = 0`) |
| O-2 | Proyectada ortografica: posiciones = proyeccion pura sobre `d` (`α = 0`, `e = w = d`) | plan G14 / comando G15 | falla |
| O-3 | Proyectada, fuentes antiparalelas → `SourceOrientationDivergent` antes de puntos, sin escritura | comando G15 | falla (hoy completa) |
| O-4 | Predeterminada ortografica = G14 actual (rotacion 0, mismas posiciones) | comando G15 | pasa (se conserva) |
| O-5 | Predeterminada misma clase, giro comun: `ρ = 0`, el punto base cae en el destino y la disposicion de varios racks se conserva (una transformacion con `α = −θ_0`) | comando G15 | falla (hoy `ρ = θ`) |
| O-5b | Predeterminada misma clase con giros distintos → `SourceRotationsDiffer` antes de puntos, sin escritura; `π`, `−π` y `π + 2π` NO difieren | comando G15 y autoridad pura | falla (hoy completa) |
| O-6 | Proyectada misma clase = Rigid G14 (`ρ = θ`) | comando G15 | pasa (se conserva) |
| O-7 | Predeterminada no se bloquea por divergencia | comando G15 | pasa |
| O-8 | Sin cambios de definicion: plan preparado, sobre y nombre base identicos entre modos | comando G15 | pasa |
| O-9 | El Plugin pasa el modo y pregunta en el orden G con atajos distintos; el write scope no calcula rotaciones | guardas de fuente | falla (no hay pregunta) |

**Suites existentes re-fijadas a Predeterminada** (sin editar sus asertos): `RackGroupPlacementPlanTests`, `RackViewCountInvariantTests` y los fakes de G15
(`G15.Scenario.Orientation = Canonical` por defecto) construyen la solicitud en Predeterminada; en sus proyecciones entre clases distintas eso es exactamente el
contrato G14; sus escenarios de la misma clase colocan las fuentes a 0°, donde Predeterminada y Proyectada coinciden (son invariantes al modo; un escenario
rigido girado nuevo debe fijar el modo explicitamente). `RackOrthographicPlacementPolicyTests` llama a la politica con marcos universales, que es el contrato de
Predeterminada entre clases; `RackRigidPlacementPolicyTests` sigue fijando que la politica rigida copia la rotacion de la fuente con
`α = 0` (la sobrecarga de dos argumentos no cambia). Se añaden pruebas de comando en Proyectada (intervalos, aviso de solapamiento, cancelacion, un solo
Commit, espejo Proyectada de `G14_V_A_MAJORITY` = `SourceOrientationDivergent`, 0°/90° = `NonParallelSources` en ambos modos) y la pregunta de orientacion con
su mapa de estados queda fijada por guardas de fuente.

Las comparaciones de O-1/O-2 usan tolerancia en vectores y angulos modulo `2π`; O-1 cubre tambien la media vuelta planar, un giro no cardinal (30°) y varias
referencias (la linea comun es una suma normalizada). Las rotaciones esperadas de la matriz OV se expresan como las muestra AutoCAD, en `[0°, 360°)`.

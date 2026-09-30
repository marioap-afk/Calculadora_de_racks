# I-55 G16 C16-06 — Orientacion proyectada de la referencia de bloque (diseño correctivo)

Frozen: NO

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
  Por sistema (adaptadores de `RackViewFrameAdapters`): Selectivo, Dinamico, Push Back y Cabecera — Planta `DepthRun` (X = Depth, Y = Run), Frontal `RunHeight`,
  Lateral `DepthHeight`; Cantilever — Planta `RunDepth` (X = Run, Y = Depth), Frontal `RunHeight`, Lateral `DepthHeight`. Todos los signos son positivos.
- **AUTH-08 V2 (hechos de colocacion de la fuente):** la parte lineal aceptada `L` de cada referencia fuente (`RackSourcePlacementAcceptance.Linear`), que ya incluye la
  media vuelta planar que reporta la Foundation. `RotationRadians = L.RotationAngle()`.
- **G14:** `RackProjectionClassMapping` (eje conservado `K` por par de clases), `SourceGroupFrame` / `TargetGroupFrame` (orientaciones `φs`, `φt`, hoy congeladas a 0 por
  OD-6.c A / OD-6.b A), `RackOrthographicPlacementPolicy` (linea comun `e` por la Ventana de Marco Relativo, direccion destino `w = R(φt)·t`, `α = ang(e→w)`, rotacion
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

Es unica: el `AxisMap` fija `t` con su signo y `L` fija `d` con su signo. Se consideró y **se rechaza** la alternativa «linea sin sentido» (plegar `d` y `−d` con la
Ventana de Marco Relativo): descarta el signo que AUTH-05 y AUTH-08 si llevan, colocaria en el destino el poste 1 del lado opuesto al de la fuente para 180° y 270°
(no es una proyeccion) y haria indistinguibles filas que el Owner valida por separado (P-01/P-03, P-02/P-04). No hay dos convenciones que las autoridades no distingan.

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
  `TargetGroupFrame.Orientation = φt = ang(t_0 → d)`. Con esos marcos, la politica ortografica existente produce `e = w = d`, `α = 0` y `ρ = φt` para **todas** las vistas:
  las vistas destino quedan alineadas sobre la recta que pasa por el punto destino en la direccion `d`, cada una desplazada segun la coordenada de su fuente sobre `d`
  (proyeccion pura), y todas giradas `ρ`. Los intervalos, el anclaje del extremo del tramo y el aviso de solapamiento no cambian.
- **Misma clase (Rigid).** Es la semantica G14 vigente: una sola transformacion rigida comun con `α = 0`; cada vista conserva su propia rotacion fuente (`ρ_i = θ_i`).
  La copia rigida de un grupo es por si misma una operacion coherente: no hay regla de divergencia en Rigid.

### B. Predeterminada (`Canonical`)

- **Clases distintas:** exactamente el resultado G14/G15 actual (`φs = φt = 0`, `ρ = 0`, Ventana de Marco Relativo).
- **Misma clase:** presentacion canonica de RackCad: `ρ = 0` (marco destino universal) para toda vista; la POSICION sigue siendo la de la transformacion rigida comun
  (el ancla del marco destino cae donde la transformacion lleva el ancla de la fuente). Es la **correccion aceptada** de C16-06 a la presentacion de Rigid en este modo
  (antes copiaba la rotacion de la fuente); la geometria de la transformacion comun y `α = 0` no cambian.
- Predeterminada **nunca** se bloquea por una divergencia que solo afectaria a Proyectada.

### C. Por defecto

`Projected` es el valor por defecto del **producto** (la pregunta de `RACKPROYECTAR`: Enter = Proyectada). En la API pura el modo es un parametro explicito de la solicitud;
el Plugin lo pasa siempre (guarda de fuente).

### D. Autoridad pura

Una autoridad pura de Application (sin tipos de AutoCAD) resuelve, para el modo y las vistas aceptadas, los marcos de grupo de la operacion o un fallo tipado. El Plugin solo
aporta los hechos capturados y aplica `RackProjectedPlacement.RotationRadians` tal cual. `AutoCadProjectionWriteScope.PlaceReference` sigue siendo mecanico.
Nombres de simbolos: no congelados.

### E. Compatibilidad entre racks (solo Proyectada y clases distintas)

- Todas las `d_i` deben ser paralelas (fallo existente `NonParallelSources`) **y del mismo sentido**: `d_i · d_0 > 0`, con la tolerancia congelada de paralelismo
  (`GeometryTolerance.Angle`).
- Si dos o mas fuentes son paralelas pero de sentido opuesto (p. ej. racks espalda con espalda, uno girado 180°), no existe UNA orientacion proyectada comun: la operacion
  **falla completa antes de los puntos** con el nuevo fallo tipado `SourceOrientationDivergent` (etapa Validate), con un diagnostico por cada vista de la operacion, como el
  resto de fallos de Validate. No se gira cada rack por su cuenta.

### F. Fallos y avisos

- `SourceOrientationDivergent` es **bloqueante**, antes de puntos, importacion y escritura. Remedio (familia `ReviewSelection`): proyectar por separado cada sentido o usar
  la orientacion Predeterminada.
- Avisos existentes sin cambio: solapamiento, vista opcional sin bloque, y `DirectionWindowNearLimit` (solo puede aparecer en Predeterminada: en Proyectada la linea comun
  coincide con el marco, angulo relativo 0). No se añade ningun aviso nuevo.

### G. Interfaz (linea de comandos, sin ventana WPF)

Seleccion (sin cambio; durante la seleccion `F` sigue siendo Fence) → `Clase de vista a proyectar [Frontal/Lateral/Planta]` →
`Orientacion [Proyectada/Predeterminada] <Proyectada>` (Enter = Proyectada; `Esc` cancela sin leer ni escribir) → lectura unica y plan (fallos y avisos) →
`Punto base` → `Punto de destino`. El informe final nombra el modo usado.

### H. Sin cambios de definicion

`HeaderRunPlan`, `CantileverViewPlan`, los constructores de vistas, `RackDefinitionCreator` / AUTH-15, la Foundation y el esquema del sobre NO cambian. Si la implementacion
pareciera exigir girar geometria interna de un plan, se detiene: violaria el requisito del Owner.

### I. Persistencia

Ninguna. El modo es una eleccion por invocacion y no se guarda; la rotacion ya es una propiedad de la `BlockReference`; el sobre no cambia.

### J. Matriz de Validacion del Owner (OV-C16-06)

Filas `P-01..P-18` (Proyectada) y `C-01..C-04` (Predeterminada), con la rotacion esperada calculada con la seccion 4, en la guia de validacion de G16
([I-55-g16-owner-validation.md](I-55-g16-owner-validation.md) §9). En Proyectada gira TODA la referencia: geometria, textos, cotas y etiquetas.

## 6. Obligaciones invariante → prueba (RED antes de implementar)

| ID | Invariante | Prueba | RED esperado sobre el codigo actual |
|---|---|---|---|
| O-1 | Proyectada ortografica: `R(ρ)·t = d` para toda vista, en los 5 sistemas y las 4 rotaciones | autoridad pura con marcos REALES de los adaptadores | falla (hoy `ρ = 0`) |
| O-2 | Proyectada ortografica: posiciones = proyeccion pura sobre `d` (`α = 0`, `e = w = d`) | plan G14 / comando G15 | falla |
| O-3 | Proyectada, fuentes antiparalelas → `SourceOrientationDivergent` antes de puntos, sin escritura | comando G15 | falla (hoy completa) |
| O-4 | Predeterminada ortografica = G14 actual (rotacion 0, mismas posiciones) | comando G15 | pasa (se conserva) |
| O-5 | Predeterminada misma clase: `ρ = 0`, posicion por la transformacion rigida | comando G15 | falla (hoy `ρ = θ`) |
| O-6 | Proyectada misma clase = Rigid G14 (`ρ = θ`) | comando G15 | pasa (se conserva) |
| O-7 | Predeterminada no se bloquea por divergencia | comando G15 | pasa |
| O-8 | Sin cambios de definicion: plan preparado, sobre y nombre base identicos entre modos | comando G15 | pasa |
| O-9 | El Plugin pasa el modo y pregunta en el orden G; el write scope no calcula rotaciones | guardas de fuente | falla (no hay pregunta) |

# I-65 — Proposal V1 (Structural Analysis Interop Foundation + RAM Elements Adapter)

```text
Estado: PROPUESTA — NO CONGELADA
Frozen: no
Coordinator: PROPONE (autor)        Architect: pendiente de revisión
IMPLEMENTATION AUTHORIZATION = NO
```

Fuentes: [mandato](../automation/decisions/I-65-owner-mandate.original.txt), [Discovery F0](I-65-discovery.md),
[contrato](I-65-interop-analisis-estructural.md). Los nombres de tipos son **hipótesis** (mandato): se congelan responsabilidades,
contratos observables y semántica de fallo, no identificadores ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §6).

## 1. Principio rector

```text
RackCad / DWG                  = autoridad de producto y diseño (sin cambios de persistencia)
Structural Analysis Model      = representación analítica NEUTRAL, DERIVADA y regenerable
RAM Elements                   = consumidor externo de análisis (nunca autoridad de RackCad)
```

Flujo V1: `DWG → diseño authored → resolver puro existente → fuente estructural (nueva) → StructuralModel (nuevo) →
adaptador RAM (nuevo) → paquete en destino controlado → script ejecutado por el ingeniero en la Python Console de RAM Elements`.

## 2. Decisiones propuestas

### D-01 — Capas y frontera de proveedor

- Modelo neutral, unidades, diagnósticos, huellas y comparador de equivalencia: **puros en `RackCad.Application`**, carpeta raíz nueva
  (hipótesis `StructuralAnalysis/`). Domain no cambia.
- Fuente por sistema: en `Application/Systems/Cantilever/` (convención de I-23: lo propio de un sistema vive en su carpeta), consumiendo
  `CantileverLineResolver`/`CantileverStationAssembly` de solo lectura.
- Adaptador RAM: **puro en Application**, subcarpeta de proveedor (hipótesis `StructuralAnalysis/Providers/RamElements/`), con el
  precedente de los exportadores CSV/XLSX del BOM: solo BCL, sin NuGet, sin COM, sin SDK. Devuelve el paquete como contenido; **escribir
  en disco es del llamador** (Plugin o herramienta).
- Guardas de fuente (pruebas): (G1) ningún archivo fuera de la subcarpeta del proveedor nombra a RAM ni sus tipos; (G2) el modelo neutral
  no referencia `Systems/*` ni proveedores; (G3) Domain no referencia el análisis; (G4) sin literales de conversión de unidades fuera de
  la frontera (D-03).
- Alternativa registrada para el Architect: ensamblado propio del adaptador (frontera por compilador) a costa de un proyecto nuevo en la
  solución, la CI y el bundle. V1 propone carpeta + guardas.

### D-02 — StructuralModel V1 (contenido mínimo para la rebanada)

| Parte | Contenido | Identidad |
|---|---|---|
| Metadata | `SourceRef` (RackId, Kind, alcance —p. ej. índice de estación—, huella de la fuente), versión del generador, referencias con hash del perfil de material y del conjunto de supuestos | neutral |
| Nodos | posición (Length×3), supuesto de apoyo opcional | id neutral determinista derivado de la fuente (p. ej. `ST0.COL.J2`) |
| Miembros | nudo I, nudo J, `SectionRef`, `MaterialRef`, **orientación** (vector de referencia del eje de peralte de la sección en coordenadas globales), liberaciones por extremo, rol (Column/Base/Arm) y pieza de origen (procedencia) | id neutral determinista |
| Secciones | `StructuralSectionId` del catálogo neutral + propiedades **requeridas por la rebanada** (A, Ix, Iy, J; Cw si existe) copiadas del catálogo con su dimensión | `StructuralSectionId` |
| Materiales | E, G (o ν) y peso unitario, del perfil explícito (D-06) | `StructuralMaterialId` neutral |
| Apoyos | 6 banderas por nudo + id del supuesto que las justifica | — |
| Liberaciones | por extremo de miembro (M2, M3, V2, V3; torsión y axial a nivel de miembro) + id de supuesto | — |
| Casos de carga | id neutral, categoría neutral, participación de peso propio | neutral |
| Cargas | nodal (fuerza y momento), puntual en miembro (dirección global/local, magnitud, posición como fracción) | — |
| Combinaciones | **ninguna en V1** (pedir una es `Unsupported`) | — |
| Supuestos | lista explícita `AnalysisAssumption(id, enunciado)` referenciada por apoyos, liberaciones, idealización y cargas | id |
| Diagnósticos | §D-08 | — |

**Prohibido en el modelo neutral:** identificadores, nombres de sección o material, códigos de unidad o convenciones de ejes del
proveedor. Los ids de proveedor viven solo en el mapeo del adaptador y en la lectura de vuelta.

### D-03 — Frontera única de unidades

- Cantidades **tipadas por dimensión** en el modelo neutral: longitud, área, momento de inercia, constante de torsión, constante de
  alabeo, fuerza, momento, esfuerzo/módulo, fuerza por longitud, peso unitario o densidad, ángulo. Almacenamiento canónico en **SI base**
  (m, N, Pa, kg, rad), sin constructor público desde `double` sin unidad.
- **Una** frontera de conversión de análisis: longitudes con las constantes exactas existentes (`LengthUnits.MillimetersPerInch`;
  M-01 no se activa si no se duplica in↔mm); define una sola vez las constantes exactas que no existen: lbf = 4.4482216152605 N,
  kgf = 9.80665 N, lb = 0.45359237 kg. `StructuralSectionUnits` (ADR-0021 §10) sigue siendo la autoridad de los datos del catálogo.
- Entrada desde RackCad: pulgadas (ADR-0005) → SI en la fuente. Salida hacia RAM: el adaptador convierte SI → par de unidades declarado
  del proveedor, **en un solo punto**. Unidad desconocida o no medida → `Invalid`.
- Par de RAM provisional: **N-mm**, sujeto a P1 (RC-01/RC-02). Si P1 muestra que algún grupo de magnitud no sigue al par, el adaptador
  excluye esa magnitud (fail closed) hasta una A-n.
- Pruebas obligatorias que detectan: in↔mm (×25.4), lbf↔N y kgf↔N, carga distribuida (1 N/mm = 1 kN/m = 1000 N/m), momento
  (1 kN·m = 10⁶ N·mm), y conversión de ida y vuelta exacta.

### D-04 — Sistema de coordenadas y orientación

- Neutral: dextrógiro, **Z vertical hacia arriba**, mismo origen que el mundo de la fuente (Cantilever ya usa Z arriba).
- RAM: eje vertical y convención de ejes locales **medidos por P1** (RC-03/04/08/09). Hipótesis fuerte (paquete, ejemplos y ayuda del
  puente en el binario; el manual se contradice): **Y arriba**, con `X_RAM = X`, `Y_RAM = Z`, `Z_RAM = −Y` (rotación propia, conserva la
  mano). La transformación vive solo en el adaptador.
- Orientación: el neutral guarda el vector del eje de peralte de la sección; el adaptador calcula el giro de RAM que lo reproduce según la
  convención medida, y el script **verifica** con `get_local_axes()` (tolerancia angular congelada).

### D-05 — Idealización analítica (rebanada Cantilever)

| Regla | Contenido V1 |
|---|---|
| IDL-01 | El eje analítico de cada miembro es su **eje centroidal** (origen del marco `PrismaticSectionInstance` + eje longitudinal), no el punto de inserción del bloque ni la cara dibujada |
| IDL-02 | Nudos analíticos en las **intersecciones de ejes centroidales** proyectadas en el plano de la estación (columna–brazo, columna–base); los miembros van de nudo a nudo |
| IDL-03 | **Sin offsets rígidos en V1** (RC-01 sin medir): la longitud flexible del brazo incluye media columna y la placa; limitación declarada, con offsets como evolución por A-n tras P1 |
| IDL-04 | Excentricidades fuera del plano de la estación: no se modelan; un miembro cuyo eje se aparta del plano más que la tolerancia → `Unsupported` |
| IDL-05 | Tolerancias congeladas: fusión de nudos y conectividad (valor propuesto 0.001 in) y angular (propuesto 0.01°) |
| IDL-06 | Fuera de V1 → `Unsupported`: brazos de canales pareados, arriostramientos y barras redondas, separadores, placas y cartelas como elementos, estaciones bloqueadas por diagnóstico del resolver |

### D-06 — Secciones

```text
StructuralSectionId (catálogo neutral, AISC v16.0) → StructuralSection (propiedades requeridas) → nombre de sección de RAM
```

- La identidad es `StructuralSectionId`, nunca un `displayName` ni una etiqueta de bloque.
- Mapeo a RAM **explícito y versionado** (tabla de datos nueva, solo filas revisadas; sin inferencia de cadenas en tiempo de ejecución).
  Fila ausente → `Unsupported`.
- RAM no recibe propiedades: usa las de **su** base de datos por nombre. Por eso: el manifiesto lista las propiedades de RackCad por
  sección y el script verifica la asignación por lectura de vuelta; la coherencia de propiedades entre ediciones (AISC v16.0 frente a la
  base de RAM) se comprueba en OV con tolerancia congelada.
- Secciones de rack perforadas: fuera de V1 (sin autoridad en RackCad; en RAM requieren sección de usuario).

### D-07 — Materiales

- Autoridad **nueva y explícita**: perfil de material estructural versionado (`StructuralMaterialId`, E, G o ν, peso unitario, fuente
  declarada). **Nunca** se infiere de nombres de producto ni de la familia de sección sin una regla escrita del perfil.
- Asignación V1 por familia de sección mediante una regla del perfil (propuesta para el ingeniero: W → A992 Gr50; HSS rectangular →
  A500 Gr C; C, L, S → A36), sujeta a la decisión Q-O2.
- Mapeo `StructuralMaterialId → nombre de material de RAM` explícito; fila ausente → `Unsupported`. Solo propiedades requeridas por
  la rebanada (Fy/Fu fuera de V1: no las usa el análisis).

### D-08 — Modelo de errores (sin exportación parcial)

`StructuralExportOutcome = Valid(model) | Unsupported(reasons) | Invalid(reasons) | Unavailable(reasons)`. Cualquier diagnóstico
bloqueante impide producir el paquete; el escritor no deja archivos a medias. Códigos iniciales: sistema o alcance no soportado,
fuente bloqueada o ambigua, mapeo de sección ausente, propiedades de sección ausentes, material ausente, unidad inválida, modelo
desconectado, conexión no soportada, carga inválida, combinación pedida, proveedor no disponible o versión no soportada (dentro de RAM),
fallo del adaptador. Los fallos dentro de RAM (capacidad, modelo no vacío, nombre rechazado, ejes distintos) **detienen** el script y se
escriben en la lectura de vuelta. Como la API no puede borrar nudos ni miembros y asigna la sección después de crear el miembro, un fallo
a mitad deja un modelo incompleto: el script lo declara `FAILED — DISCARD MODEL` y el procedimiento exige descartarlo sin guardar.

### D-09 — Transporte: paquete de construcción para RAM Elements

- Elegido: **API Python oficial**, ejecutada por el ingeniero con «Run File…» sobre un **modelo nuevo y vacío**. No es una
  automatización de interfaz: la semántica viaja por la API.
- Paquete en una **carpeta nueva** elegida por el usuario (si existe, se rechaza): `manifest.json`, `rackcad-ram-build.py` (plantilla
  **fija** y revisada + carga útil JSON embebida y codificada) y, tras ejecutarlo, `ram-readback.json`.
- El script: comprueba versión y funciones del puente (fail closed); exige modelo vacío; fija unidades una vez; crea casos, nudos,
  miembros, apoyos, liberaciones, orientación y cargas; **lee de vuelta** y compara contra la carga útil; escribe el informe; **no**
  guarda, **no** analiza (salvo D-14), **no** abre ni toca otros archivos.
- Restricciones del intérprete de RAM (Discovery §3.1, RC-10/RC-11), congeladas como reglas de la plantilla: solo importa
  `ramelements`, `json` y `math` (otras, solo si P1 las acredita); **no crea carpetas** (la crea RackCad al escribir el paquete) y escribe
  únicamente con `open()` dentro de ella; categorías de caso como texto (`"DL"`, `"OTHER"`), nunca como miembro del enum; una sola
  llamada `add_force` por nudo y caso con la suma de sus cargas; como máximo 24 cargas puntuales por miembro y caso (exceso →
  `Unsupported` en RackCad, antes de generar).
- Determinismo: mismo modelo → paquete idéntico byte a byte (sin marcas de tiempo dentro de lo que se firma).
- OpenRE, STAAD, SAP2000, DXF e ISM no se usan en V1 (Discovery §3).

### D-10 — Apoyos, conexiones y cargas (supuestos explícitos de V1; ver Q-O3)

| ID | Supuesto propuesto | Limitación declarada |
|---|---|---|
| SUP-01 | Pie de columna: empotrado (6 GDL) | sin rigidez de anclaje ni losa |
| SUP-02 | Base: no se modela como miembro en V1 (alternativa B: base como miembro con apoyos articulados en sus extremos) | la base real apoya en el piso |
| SUP-03 | Análisis plano: todos los nudos restringidos fuera del plano de la estación (traslación X, giros Y y Z) | sin pandeo fuera del plano ni efecto de arriostramientos |
| CON-01 | Brazo–columna: continuo (rígido) | la conexión real es semirrígida; sin curva momento-giro en RackCad |
| CON-02 | Columna–base (si se modela): continuo | — |
| LOD-01 | Un caso de referencia controlado (`REF-1`), cargas puntuales verticales por brazo en posiciones fraccionales, con valores **entrados explícitamente** para la validación; peso propio excluido | no deriva de tarimas, impacto, sismo ni tributarias |
| LOD-02 | Sin combinaciones | — |

Hechos de producto ≠ modelo de cargas: RackCad no convierte ningún dato de producto en carga en V1. La capa de traducción existe como
punto de extensión (entrada explícita de cargas) sin otra implementación.

### D-11 — Equivalencia

- Referencia: el ingeniero modela a mano en RAM la rebanada congelada con los mismos supuestos.
- Volcado: un script **fijo de solo lectura** vuelca el modelo abierto (manual o generado) a JSON canónico.
- Comparador **puro** (Application, con pruebas) aplicado a los dos volcados, con emparejamiento de nudos por coordenadas, de miembros
  por extremos y tolerancias congeladas → informe por hecho: `PASS | FAIL(delta) | NOT-COMPARED(motivo)`. Herramienta de consola fuera
  del producto (precedente `tools/RackCad.StructuralSections.Import`).

### D-12 — Provenance y resultados obsoletos

- `SourceFingerprint` = SHA-256 del diseño authored canónico + RackId + Kind + alcance. `AnalysisInputFingerprint` = SHA-256 del
  modelo neutral canónico + hashes de perfil de material, supuestos, mapeos y versión del adaptador.
- El manifiesto registra: RackId, nombre visible (solo informativo), Kind, alcance, ambas huellas, versión de RackCad, versión del
  adaptador, SHA de la plantilla, perfiles y mapeos, versión de RAM declarada y par de unidades. La lectura de vuelta repite la huella de
  entrada.
- Regla para resultados futuros: un resultado se presenta como vigente solo si su huella de entrada coincide con la huella regenerada
  desde el DWG actual; si no, `STALE`. **Sin persistencia nueva en el DWG en V1.**

### D-13 — Capacidad de aplicación para ID30 y comando mínimo

- Capacidad neutral en Application (nombres hipótesis): `BuildStructuralModel(fuente, alcance, perfiles) → Outcome`,
  `CreateRamBuildPackage(modelo, opciones) → Outcome<contenido>`, `CompareReadbacks(a, b, contrato) → informe`. `Analyze` y
  `ReadResults` **no existen**: NOT EXPOSED desde un proceso externo.
- Para la validación temprana, un **comando de línea de AutoCAD** mínimo en un archivo nuevo del Plugin: seleccionar un rack Cantilever,
  pedir la estación, pedir una carpeta nueva, escribir el paquete y listar diagnósticos. Sin ventanas WPF, sin PaletteSet, sin eventos
  ni cambios de selección: la UI final es de ID30.

### D-14 — Análisis y resultados (segunda capacidad)

Fuera del Freeze de V1 salvo decisión del Owner (Q-O5). Si se incluye (F6): bandera opcional del script que corre `analyze()` y escribe
desplazamientos, reacciones y fuerzas de extremo con la huella de entrada en la carpeta del paquete. RackCad **no** importa ni presenta
resultados ni veredictos en esta iniciativa.

### D-15 — ADR

Decisión de arquitectura nueva → **ADR-0050** (siguiente número libre) propuesto en F0 y aceptado por el Owner antes de implementar
([WORKFLOW](../WORKFLOW.md) §8): modelo neutral derivado, frontera de unidades, frontera de proveedor, supuestos explícitos, transporte
por API oficial dentro de RAM. Consume ADR-0017 sin reabrirlo.

## 3. Invariantes → pruebas (RED esperado)

| ID | Invariante | Prueba / RED esperado |
|---|---|---|
| INV-01 | El modelo neutral no contiene identidades ni convenciones de proveedor | guarda G1/G2; RED: un tipo neutral que nombre RAM |
| INV-02 | Toda cantidad tiene dimensión; conversiones solo en la frontera | guarda G4 + pruebas de tipos; RED: conversión fuera de la frontera |
| INV-03 | Conversiones exactas y detectoras de error | in↔mm, lbf↔N, kgf↔N, N/mm↔kN/m, N·mm↔kN·m; RED antes de la frontera |
| INV-04 | Determinismo | mismo diseño → mismo JSON canónico y misma huella, sin depender del orden de entrada |
| INV-05 | Fallo cerrado sin exportación parcial | mapeo ausente, material ausente, alcance no soportado, combinación pedida → no hay paquete |
| INV-06 | Geometría analítica = ejes centroidales (IDL-01/02), no geometría de bloque | golden de una estación real; RED: nudos en puntos de inserción |
| INV-07 | Orientación | eje de peralte neutral = sección; giro de RAM reproduce los ejes medidos en P1 |
| INV-08 | Apoyos y liberaciones exactos | mapeo uno a uno, con supuestos referenciados |
| INV-09 | Cargas | solo cargas explícitas; magnitud, dirección y posición exactas; ninguna carga derivada de producto |
| INV-10 | Paquete seguro y determinista | carpeta nueva; plantilla con guardas de capacidad y modelo vacío; huellas en manifiesto |
| INV-11 | ADR-0017 | ningún cálculo de utilización ni veredicto en RackCad |
| INV-12 | Capas | Domain sin análisis; Application sin AutoCAD ni WPF |
| INV-13 | Comparador | volcados idénticos → PASS; perturbaciones (+1 mm, sección cambiada, eje local invertido, carga ×1000) → FAIL |
| INV-14 | Core sin RAM | ninguna prueba Core requiere RAM Elements ni Python |

## 4. Matriz de equivalencia (generado frente a referencia manual)

| Hecho | Criterio | Tolerancia propuesta |
|---|---|---|
| Coordenadas de nudos | emparejamiento por posición | 0.5 mm |
| Conectividad | mismos pares de nudos por miembro | exacta |
| Tipo de miembro / rol | mismo conjunto | exacta |
| Ejes locales | vectores 1, 2, 3 | 0.1° |
| Sección | mismo nombre de RAM | exacta |
| Material | mismo nombre de RAM | exacta |
| Liberaciones | mismas banderas | exacta |
| Apoyos | mismas banderas | exacta |
| Cargas: valor | por caso, miembro y dirección | 0.1 % |
| Cargas: posición | fracción o distancia | 0.5 mm |
| Combinaciones | ninguna en ambos | exacta |
| Comportamiento (F5, si aplica) | reacciones y desplazamientos de ambos modelos | 0.5 % |

## 5. Validación del Owner / ingeniería

| ID | Gate | Escenario |
|---|---|---|
| OV-I65-01 | F4 (temprana) | Exportar desde un DWG una estación Cantilever de referencia, correr el script en RAM sobre modelo vacío, sin errores; volcar generado y manual; informe de equivalencia PASS en geometría, conectividad, secciones, materiales, apoyos, liberaciones y orientación; avisos de RAM registrados |
| OV-I65-02 | F5 | Mismo caso con el caso de carga de referencia; equivalencia de cargas y, si se analiza, de reacciones y desplazamientos |
| OV-I65-03 | F4 | Una estación no soportada (canales pareados o mapeo ausente) no produce paquete y muestra el diagnóstico |
| OV-I65-04 | F4 | El script se niega a correr sobre un modelo no vacío y no modifica nada |
| OV-I65-05 | READY | Lista de la guía manual sobre el Candidato final |

La validación temprana ocurre **antes** de ampliar cobertura; con FAIL, no se avanza a F5.

## 6. Gates funcionales (sin microgates)

| Gate | Resultado verificable |
|---|---|
| F0 | Discovery, sonda P1 (con autorización), estrategia de referencia, Proposal, ADR-0050 propuesto, `AGREED` Coordinator + Architect, Freeze |
| F1 (incluye F2 del mandato) | De un diseño Cantilever real se obtiene un `StructuralModel` válido, determinista y en SI, con diagnósticos de fallo cerrado; golden + pruebas de unidades. Se fusionan F1 y F2 porque un gate solo de modelo neutral sería un gate de capa ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §7) |
| F3 | Del modelo se obtiene un paquete RAM determinista; pruebas de contrato del adaptador; fallo cerrado ante hechos no soportados |
| F4 | Comando mínimo + volcado + comparador; corrida real en RAM; **OV-I65-01, 03, 04** |
| F5 | Caso de carga de referencia; **OV-I65-02** |
| F6 | Solo si Q-O5 lo incluye: análisis y captura de resultados con huella |
| F7 | Conformidad, regresión, entrada de FOUNDATIONS redactada, guía de uso, ADR-0050 en su estado |
| READY | READY-01..09 |

## 7. Fundaciones

```text
Consumes:   Rack Identity (ADR-0009); catálogo neutral de secciones (ADR-0020..0023); colocación Cantilever
            (PrismaticSectionInstance, ADR-0022/0024/0025); LengthUnits / ADR-0005; ADR-0017 (diferimiento)
Extends:    ninguna
Introduces: Structural Analysis Model neutral; frontera de unidades de análisis; frontera de proveedores de análisis;
            perfil de material estructural; supuestos de análisis explícitos; huella de entrada de análisis
```

Computed Parameters & Project Summary (I-63) **no** se consume en V1.

## 8. Propiedad y paralelismo

- ID30 / I-64: Workspace, PaletteSet, UX WPF, selección y eventos, sesión de RACKEDITAR. I-65 no las toca; deja la capacidad D-13 para
  que ID30 la invoque después.
- I-62: infraestructura de agentes. I-65 no la toca ni adopta comportamiento no integrado.
- I-38 (ingeniería estructural de Cantilever, sin reclamar): verificación y dimensionado siguen en I-38; I-65 no calcula resistencia.
- Una autoridad compartida de Producto/Dominio → STOP y Master Orchestrator.

## 9. No-objetivos

Los del mandato y del contrato §4, más: Selectivo, Dinámico, Push Back y mezzanine en V1; secciones de rack perforadas; offsets
rígidos, resortes y conexiones semirrígidas; combinaciones; importar o mostrar resultados en RackCad; guardar automáticamente modelos
de RAM; OpenRE/STAAD/ISM.

## 10. Decisiones pedidas al Owner

| ID | Decisión | Recomendación |
|---|---|---|
| Q-O1 | Rebanada V1: estación Cantilever, o cabecera Selectivo con prerrequisitos (autoridad de sección de poste, autoridad de geometría, regla de excentricidad) | Cantilever |
| Q-O2 | Perfil de material: valores y regla por familia, con fuente de ingeniería | la de D-07, confirmada por el ingeniero |
| Q-O3 | Supuestos de apoyo, conexión, plano y cargas de referencia (D-10) | los de D-10 |
| Q-O4 | Autorizar la sonda P1 en RAM Elements (consume licencia): la corre el Owner, o el Coordinator por Computer Use con su permiso | correrla antes del Freeze |
| Q-O5 | Incluir F6 (análisis y captura de resultados) | fuera de V1 |
| Q-O6 | Aceptar ADR-0050 tras `AGREED` | — |
| Q-O7 | Quién construye el modelo manual de referencia y cuándo | el ingeniero del Owner, tras el Freeze |

## 11. Ejecución con I-61 (tras el Freeze)

| Trabajo | Clase (routing.md §1) | Effort | Celda propuesta |
|---|---|---|---|
| Investigación y caracterización de F0 | Caracterización | Deep (ambigüedad alta) | subagente `claude-opus-5-5` × read/tool-use (hecho) |
| Revisión del Architect y conformidad | Revisión de arquitectura | Deep | `claude-opus-5-5`, contexto separado del autor |
| F1 modelo neutral + fuente Cantilever | Implementación transversal | Deep | Worker `claude-sonnet-5-5`/`high` (medido), escalado a Frontera solo con evidencia |
| Tablas de mapeo, pruebas de unidades | Implementación de pruebas / mecánica | Balanced | `claude-sonnet-5-5`/`medium` |
| Adaptador y plantilla Python | Implementación + depuración | Deep | `claude-sonnet-5-5`/`high`; causa incierta → `claude-opus-5-5` |
| Planificación y verificación de gates | Controller | Balanced | `gpt-6-luna`/`high` (Codex, solo lectura), previa línea base de `config.toml` y elegibilidad del día |
| Integración real en RAM / OV | Owner + Coordinator | — | sin delegación |

`RoutingEnforcement: required`. Sin reinicios por escalado (AUTOMATION_PLAN 16.8). La fricción del protocolo se registra en la
evidencia para I-62; no se adopta comportamiento no integrado de I-62.

## 12. Riesgos

| Riesgo | Mitigación |
|---|---|
| API 0.1.0 cambia entre versiones de RAM | puerta de capacidades en el script; versión declarada en el manifiesto; prueba real por versión |
| Unidades por grupo mal interpretadas | P1 + pruebas de escala + verificación por lectura de vuelta; magnitudes no medidas excluidas |
| Propiedades de la base de RAM ≠ AISC v16.0 | tabla de propiedades en el manifiesto y comparación en OV |
| Idealización sin offsets sobreestima flexibilidad del brazo | limitación declarada; misma regla en la referencia manual; offsets por A-n tras P1 |
| Script ejecutado sobre un modelo del usuario | guarda de modelo vacío; sin guardar ni abrir archivos |
| Selectivo pospuesto contra la preferencia del mandato | decisión explícita Q-O1 con prerrequisitos listados |

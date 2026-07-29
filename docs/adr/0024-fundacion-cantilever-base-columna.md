# ADR-0024: Fundación Cantilever — diseño en Domain, resolución en Application y autoridad compartida base–columna

- **Estado:** **aceptado**
- **Fecha:** 2026-07-29 (redacción y **aceptación**)
- **Decisores:** **Mario Pérez, Owner del repositorio** (acepta); Claude Opus 5 (redacción)
- **Iniciativa relacionada:** I-37A `architecture/cantilever-base-columna`
- **No reemplaza a ninguna ADR.** Extiende [ADR-0020](0020-catalogo-neutral-de-secciones-estructurales.md),
  [ADR-0021](0021-identidad-unidades-y-presentacion-de-secciones.md),
  [ADR-0022](0022-geometria-parametrica-de-secciones-estructurales.md) y
  [ADR-0023](0023-geometria-visual-derivada-perfiles-s.md) hacia el primer **consumidor** del catálogo neutral.

> **Nació `propuesto` y se aceptó sobre el código, no sobre el dibujo.** A diferencia de ADR-0022 y
> ADR-0023, cuyo gate era ver la geometría en AutoCAD, I-37A **no dibuja nada**: no hay vistas, editor,
> persistencia ni Plugin. Lo que se acepta aquí son **contratos**, y lo verificable de un contrato son sus
> invariantes y sus guardas, no una captura. Por eso `requires_autocad: false` y
> `requires_owner_validation: false`, y por eso el gate se resolvió con CI verde, 2 224 pruebas y las once
> regresiones comprobadas en rojo.

## Aceptación del Owner (2026-07-29)

**Decisor:** Mario Pérez, Owner del repositorio. **Gate:** aprobado.
**Veredicto normativo registrado:** `OWNER_APPROVED_ADR_0024`.
**SHA técnico aprobado:** `15523679e655364c146917ece338c7cecbe24023`.

Aceptado expresamente:

| # | Lo aceptado |
|---|---|
| 1 | El **diseño Cantilever vive en Domain** y guarda los ids de sección como **texto** |
| 2 | Una **frontera única de resolución** en Application: un solo sitio parsea el id y consulta el catálogo |
| 3 | El **modelo híbrido** por **naturaleza física** —perfil del catálogo, placa, cartabón, troquel— con el rol como enum |
| 4 | **`PrismaticSectionInstance` como única autoridad de colocación**; sección, longitud, extremos y dirección son derivados sin campo de respaldo |
| 5 | El **patrón compartido base–columna**: una sola autoridad, consumida igual por la placa posterior y por la cara de la columna |
| 6 | **Igualdad exacta del datum** consistente con su hash, y **comparación geométrica tolerante separada** (`ApproxEquals`) |
| 7 | **Autoridad única de marcos** (`CantileverColumnBaseFrameResolver`), que lee la orientación registrada y falla cerrado |
| 8 | **`NominalCutLength` igual a la longitud geométrica y NO liberada para fabricación** |
| 9 | Toda **geometría exterior derivada de I-36** vía `StructuralSectionGeometry.Bounds` |
| 10 | **Elegibilidad por combinaciones exactas** de ids, inyectable, sin lista blanca familia→rol |
| 11 | El **datum base–columna declarado**: `y = 0` plano de conexión, `z = 0` fondo común, `x = 0` centro de la columna |
| 12 | Las **validaciones de ajuste de troqueles**: offsets ≥ radio, pitches ≥ diámetro, separación de filas ≥ diámetro |

**La aceptación NO autoriza**, y ninguna fase posterior puede darlas por concedidas: vistas; UI; AutoCAD;
persistencia de proyecto; brazos; estaciones; separadores; arriostres; BOM; peso; cálculo estructural;
fabricación; ni **cambios a I-36**. Cada una es una decisión de su propia iniciativa.

## Contexto

ADR-0020 separó tres cosas que `secciones.csv` mezclaba: la **sección transversal**, el **miembro** y la
**pieza comercial**. Dejó escrito que el rol de miembro pertenece a los configuradores futuros y que la
sección «no es un poste, una viga, un separador, un brazo de cantilever ni una columna». I-36B añadió la
**instancia prismática** —donde vive la longitud— y declaró igual de expresamente que **no es un miembro**.

I-37A es la primera iniciativa que construye un miembro. Al hacerlo aparecen cuatro preguntas que
condicionan todo lo que Cantilever construya encima, y las cuatro son caras de revertir porque fijan
contratos públicos y la forma de lo que después se persistirá.

### 1. En qué capa vive el diseño, si el id de sección vive en Application

`StructuralSectionId` es un `readonly struct` de `RackCad.Application.StructuralSections`.
`src/RackCad.Domain/RackCad.Domain.csproj` no declara **ninguna** `ProjectReference`: Domain no puede verlo.
Y los cinco sistemas vigentes ponen su diseño en Domain sin excepción.

### 2. Qué forma tiene un miembro resuelto

Un cantilever se compone de piezas de naturaleza distinta: perfiles estructurales del catálogo, placas
planas, un cartabón triangular y perforaciones. Tratarlas todas igual produce una clase con la mitad de
sus campos nulos; tratarlas todas distinto multiplica los `switch` de tipo en cada consumidor.

### 3. Quién es dueño del patrón de conexión base–columna

La placa posterior de la base y la cara de conexión de la columna llevan **los mismos agujeros**. Si cada
resolver los calcula por su cuenta, el sistema tiene dos algoritmos que hoy coinciden y mañana no. El
precedente medido está en el repositorio: el defecto PB-004 de Push Back —una magnitud vertical que
«emergía de dos snaps independientes más un salto entre dos datums»— costó cuatro validaciones rechazadas
del Owner.

### 4. Qué es una longitud en el MVP

El encargo excluye fabricación: preparación de extremos, tolerancias, CNC. Pero el BOM futuro necesita una
longitud por pieza, y llamarla «longitud de corte» sin las reglas que la producen sería prometer
fabricación con un número que no la tiene.

## Decisión

### D1 — El diseño vive en Domain y guarda el id de sección como TEXTO

`CantileverColumnBaseDesign` y sus partes viven en `RackCad.Domain.Systems.Cantilever` y guardan el id de
sección como `string`, exactamente como `DynamicRackDesign.InOutBeamCatalogId` guarda su id de catálogo.

**No se mueve `StructuralSectionId` a Domain** y **no se invierte la dirección de dependencias.**

El texto se convierte en `StructuralSectionId` en **un único límite de Application**
(`CantileverSectionResolution`), que es también el único que consulta el catálogo. Un id que no parsea, uno
que no resuelve y una combinación no elegible producen **diagnósticos**, no excepciones desde Domain.

### D2 — Modelo híbrido: diseño tipado por pieza, resultado neutral por naturaleza

- **Intención (Domain): un tipo por pieza.** Columna, base, cada placa, el cartabón y los troqueles tienen
  su propio contrato editable. Nada de una clase de configuración con campos que solo aplican a veces.
- **Resultado (Application): un tipo por NATURALEZA física, no por rol.**
  - `CantileverStructuralMemberPlan` — todo lo que es un **perfil del catálogo** colocado (columna y base
    hoy; brazo y arriostre mañana), con `MemberId`, rol, `StructuralSectionId`, `PrismaticSectionInstance`,
    propietario, longitudes y diagnósticos.
  - `CantileverPlatePlan` — una placa plana: contorno rectangular, espesor, plano y normal.
  - `CantileverGussetPlan` — el cartabón: triángulo con su plano y su espesor.
  - `CantileverPunchPlan` — una perforación.

  Son cuatro naturalezas, no cinco roles: el rol viaja como enum **dentro** del plan de miembro. Añadir el
  brazo en una iniciativa posterior añade un valor al enum, no un tipo ni un `switch` en cada consumidor.

**La colocación tiene una sola autoridad:** `PrismaticSectionInstance`. `SectionId`, `Length`, `Start` y
`End` son propiedades **derivadas** de ella, sin campo de respaldo. Guardar cualquiera de ellas aparte
sería una segunda verdad que se desincroniza en cuanto alguien llame a `WithLength`.

### D3 — El patrón de conexión es UNA autoridad compartida, no un cálculo repetido

`CantileverColumnBaseConnectionPattern` es un resultado calculado **una vez** por
`CantileverColumnBaseConnection`, y lo consumen **igual** la placa posterior de la base y la cara de
conexión de la columna. No existen dos algoritmos equivalentes.

La coincidencia se comprueba sobre un **datum lógico** —`CantileverPunchDatum`: coordenada transversal,
elevación y eje de perforación— y **nunca** comparando centros 3D. Dos agujeros del mismo tornillo están en
superficies separadas por el espesor de una placa: sus centros 3D son legítimamente distintos y su datum es
el mismo. Comparar centros 3D obligaría a compensar espesores en cada prueba, que es precisamente el error
que este ADR evita.

### D4 — `NominalCutLength` existe, es igual a la longitud geométrica y NO está liberada para fabricación

`CantileverStructuralMemberPlan.NominalCutLength == Placement.Length` en el MVP, por definición y con
prueba. Es el contrato que el BOM futuro consumirá.

**No** incluye despunte, corte a inglete, tolerancia de montaje ni kerf, **no** es una cota de taller y
**no** está liberada para CNC. Existe con nombre propio —y no como alias silencioso de la longitud
geométrica— para que el día que fabricación entre en alcance haya un campo donde ponerla y una prueba que
señale que dejó de ser una identidad.

**El peso queda fuera de I-37A.** `StructuralSectionDefinition.WeightPerLength` se declara a sí mismo la
única autoridad de peso y `PrismaticSectionInstance.Weight(section)` ya la consume; I-37A no lo calcula, no
lo almacena y no lo expone, porque el BOM —su único consumidor— no nace aquí.

### D5 — La geometría exterior se deriva de I-36, siempre por `Bounds`

Todo dato dimensional que I-37A necesita —el ancho de una placa, la altura de la sección de la base, las
dos coordenadas transversales de las filas de agujeros— se obtiene de `StructuralSectionGeometry.Bounds`
del contorno que I-36 resuelve, **nunca** de `d`, `bf`, `tw` o `tf`, nunca de los tipos concretos de
dimensiones y nunca interpretando la designación.

La razón no es purismo: `Bounds` es la envolvente **real** del contorno que se va a dibujar, incluidos
filetes y bulbos de arco, mientras que una dimensión tabulada es un número nominal. Componer contra el
contorno y dibujar el contorno garantiza que la placa y el perfil coincidan en el dibujo. Componer contra
`bf` y dibujar el contorno no lo garantiza.

**Consecuencia obligatoria:** el origen del marco de un miembro cae en el **centroide** de su sección
(`SectionOriginBasis`), no en una cara. Colocar una cara contra un plano exige derivar el desplazamiento de
`Bounds`, y I-37A lo hace en una sola función por miembro.

### D6 — Elegibilidad explícita e inyectable, sin lista blanca de familia por rol

`ICantileverColumnBaseSectionPolicy` decide qué combinación **columna × base** es un diseño válido, y lo
hace con **ids exactos** registrados como variantes. La política es inyectable; las pruebas registran ids
reales o sintéticos explícitos y el producto no adquiere ids arbitrarios en esta iniciativa.

Esto **no** contradice ADR-0020. ADR-0020 prohíbe que el **catálogo** conozca roles de miembro
—`StructuralSectionFamily` declara que una familia es una FORMA, nunca un rol—. La política vive en
`Systems/Cantilever`, es del **sistema**, no del catálogo, y el catálogo sigue sin saber que Cantilever
existe.

I-37A restringe además la familia a **W**. No es una regla permanente: es el alcance del primer diseño
aprobado, declarado en un solo sitio y ampliable registrando variantes.

Regla de secciones deshabilitadas, heredada sin cambio de la decisión 15 del Owner en I-36A: una **selección
nueva** ofrece solo habilitadas; un **diseño existente** que referencia una sección deshabilitada **sigue
resolviendo y dibujando**, con advertencia y sin sustitución automática.

### D7 — Un datum explícito, y un sistema de coordenadas local declarado

`CantileverColumnBaseDatum` fija los tres planos del subensamble:

- **`y = 0`** — el `ColumnBaseConnectionPlane`: el plano de contacto entre la cara de conexión de la columna
  y la cara de apoyo de la placa posterior. La columna ocupa `y ≤ 0`; todo el conjunto de la base ocupa
  `y ≥ 0`. El eje de perforación de la conexión es `+Y`.
- **`z = 0`** — la elevación común del **fondo de la sección de la columna** y del **fondo de la sección de
  la base**.
- **`x = 0`** — el centro transversal de la sección de la columna.

Los tres se derivan de `Bounds`; ninguno es un offset calculado a partir de una tabla AISC.

## Alternativas consideradas

| # | Alternativa | Por qué se descarta |
|---|---|---|
| A1 | `CantileverColumnBaseDesign` en Application, para poder tipar el id | Rompe la simetría con los cinco sistemas; y el DTO tendría que guardar texto igualmente, porque `StructuralSectionId` tiene constructor privado, `Value` sin `set` y ningún `JsonConverter`: serializarlo emitiría un objeto anidado indeserializable |
| A2 | Mover `StructuralSectionId` a Domain | Cambia el namespace público de ~30 archivos de I-36 y contradice dónde lo situó ADR-0021, por una comodidad de tipado. Fuera del alcance de I-37A y caro de revertir |
| A3 | Jerarquía de tipos por rol hasta el resultado (`CantileverColumn`, `CantileverBase`, …) | Cada consumidor gana un `switch` de tipo. El árbol tiene hoy dos `switch` de discriminador y uno ya olvidó un caso; multiplicarlos es reproducir ese modo de fallo |
| A4 | Un `Member` genérico con diccionario de propiedades | `LocalFrame3D` y `StructuralSectionId` no caben en un `<string,double>`, y un diccionario no puede declarar qué campo es autoridad y cuál derivado |
| A5 | Que cada resolver calcule su mitad del patrón de conexión | Es literalmente PB-004. Dos algoritmos que coinciden hoy |
| A6 | Comparar centros 3D para probar la coincidencia | Obliga a compensar el espesor de cada placa en cada prueba; el primer espesor que cambie rompe pruebas que no describen un defecto |
| A7 | Omitir `NominalCutLength` hasta que exista el BOM | El campo es el contrato que la siguiente iniciativa necesita; añadirlo después obliga a revisar cada sitio que construye un miembro |
| A8 | Derivar las coordenadas de las filas de agujeros de `bf` | Es exactamente la dependencia que ADR-0020 y ADR-0022 prohíben, y produce placas que no casan con el contorno dibujado |

## Consecuencias

**Positivas.** El catálogo neutral queda intacto y sin saber de Cantilever. El diseño es serializable como
texto el día que exista persistencia, sin convertidores nuevos. Un consumidor futuro —BOM, vistas, editor—
lee un solo tipo por naturaleza física. La coincidencia de agujeros es demostrable con una prueba que no
depende de espesores. Añadir el brazo añade un valor de enum, no una capa.

**Negativas, y asumidas.** Un `string` en Domain no valida nada: un diseño puede guardar basura y solo
Application lo descubre — mitigado con un único punto de parseo y diagnósticos explícitos. Y el rol como
enum admite, en teoría, un miembro con un ancla que no le corresponde — mitigado validando la coherencia
rol↔propietario al construir el ensamble.

**Lo que este ADR NO decide.** No decide cómo se dibuja un miembro paramétrico dentro del bloque de vista
de un sistema (no hay vistas en I-37A), ni la persistencia, ni el registro del sistema, ni el BOM, ni el
peso, ni la pendiente del brazo. Cada una es una decisión de su propia iniciativa.

## Referencias

- [ADR-0020](0020-catalogo-neutral-de-secciones-estructurales.md) — sección ≠ miembro ≠ pieza comercial.
- [ADR-0021](0021-identidad-unidades-y-presentacion-de-secciones.md) — identidad y normalización del id.
- [ADR-0022](0022-geometria-parametrica-de-secciones-estructurales.md) — ejes locales, origen en el
  centroide y plan neutral único.
- [ADR-0023](0023-geometria-visual-derivada-perfiles-s.md) — autoridad visual derivada y su advertencia.
- [Contrato de I-37A](../initiatives/I-37A-cantilever-base-columna.md).
- [Decisiones del Owner para I-37](../automation/decisions/I-37.md).

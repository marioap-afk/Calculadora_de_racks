# ADR-0034: Variables de proyecto como autoridad persistente de nivel dibujo y referencias tipadas por VariableId

- **Estado:** **propuesto**
- **Fecha:** 2026-09-09 (propuesto)
- **Decisores:** Mario Pérez, Owner del repositorio (**pendiente de aceptación**: solo el dueño acepta
  o rechaza, `adr/README.md`); Coordinador de I-47 y Arquitecto de I-47 (consenso técnico sobre
  Proposal **V4.8** / SHA `a0621abbd22952ad5a62bf7678212a74526a05ce`); Claude (redacción)
- **Iniciativa relacionada:** I-47 — `architecture/project-variables-foundation`
  ([contrato](../initiatives/I-47-project-variables-foundation.md),
  [proposal](../initiatives/I-47-proposal-v4.md))

> **Este ADR está `propuesto`, no aceptado.** El consenso Coordinador ↔ Arquitecto establece que la
> decisión está **lista para implementarse**; **no** equivale a aceptación. Son dos actos de dos
> autoridades distintas, y el repositorio lo fija así: «Estados: `propuesto` → `aceptado` |
> `rechazado`; **Solo el dueño del repo acepta o rechaza**. Los agentes pueden redactar ADRs en estado
> `propuesto`».
>
> **Producción sigue BLOQUEADA** mientras este ADR no esté aceptado.

## Contexto

Hoy, cada valor que gobierna un rack vive **dentro del diseño de ese rack**. Dos racks del mismo dibujo
que deberían compartir una holgura la comparten por coincidencia de configuración, no por decisión del
proyecto, y cambiarla exige editar cada uno. No existe ninguna autoridad de **nivel dibujo**: el
Discovery de I-47 verificó que el repositorio no usa el `NamedObjectsDictionary` en ningún sitio, ni
`SummaryInfo`, ni `XData`, y que lo único embebido en el DWG es un `Xrecord` colgado de **la definición
de cada bloque**.

ID22A necesita cuatro cosas que hoy no existen:

1. **Una autoridad única por dibujo** cuyo valor gobierne a todos los racks que la referencian.
2. Que **cambiarla actualice lo ya dibujado**, no solo lo que se dibuje después.
3. Separar tres conceptos que el árbol tiene fundidos: la **identidad del rack**, su **estado
   authored** (lo que el usuario tecleó y se persiste) y su **estado effective** (lo que gobierna la
   geometría).
4. Comportarse **fail-closed** donde hay autoridad.

El cuarto punto merece su propia explicación, porque es el que más trabajo costó y el que más veces se
rehízo. El árbol contiene **capas históricamente tolerantes, y lo son a propósito**: `Deserialize`
devuelve `null` en vez de lanzar para que un bloque futuro o ajeno no aborte un barrido de nivel
dibujo; los listados hacen `continue` sobre lo que no entienden; hay `catch {}` que evitan que un
archivo corrupto tumbe una ventana entera. Ninguna de esas tolerancias es un error. Pero **una garantía
de autoridad construida encima de ellas se disuelve al atravesarlas**: cuatro revisiones adversariales
del Arquitecto encontraron el mismo patrón cinco veces —un `null`, un `continue`, un `catch`, un valor
por defecto convirtiendo un estado desconocido en un éxito aparente— y las cuatro primeras se
corrigieron **por enumeración**, caso a caso, hasta que quedó claro que hacía falta un principio.

## Decisión

### 1. Autoridad

Un **único `ProjectVariablesDocument` por DWG**, persistido a nivel dibujo. Es la única autoridad de
variables de proyecto; no hay copias por rack ni por instalación.

### 2. Identidad

**`VariableId` es un identificador estable e inmutable, independiente de `Name`.** `Name` es una
etiqueta mutable que **no participa en ninguna resolución**. Renombrar una variable no toca a ningún
consumidor.

### 3. Tipo

Las variables son **tipadas desde el primer día**. ID22A soporta **exactamente un tipo, `Length`**, con
**definición literal**. El discriminador de tipo se persiste desde el inicio aunque hoy tenga un solo
valor, porque añadirlo después rompería el esquema.

### 4. Binding

Una propiedad es **`Literal(T)`** o **`ProjectVariableReference(VariableId)`** —modelado como
`PropertyValue<T>`—, identificada por un **`PropertyId` estable**. La propiedad piloto es
`selective.verticalClearance`. Cuando hay referencia, **la referencia gobierna el valor efectivo**.

### 5. Authored y effective son distintos

La **autoridad persistida** (`SelectivePalletDesignDocument`) y el **diseño efectivo**
(`SelectivePalletDesign`) son conceptos separados. Mientras existe un binding, el **literal authored
queda congelado e inactivo**: un cambio de variable no lo sobrescribe, y guardar el rack tampoco.

### 6. Resolución central

**Un único punto de resolución, en Application**: `authored + ProjectVariables → effective`. Dibujo,
BOM y preview consumen **solo el efectivo** y **no conocen `VariableId`**. Ninguno resuelve bindings
por su cuenta.

### 7. Propagación atómica

Cambiar una variable es **una operación confirmada**: descubrimiento de consumidores, autoridad
multi-vista, **preflight semántico** (puro) y **preflight físico** (AutoCAD, read-only) **antes de
mutar nada**, y después **una** transacción que escribe registro, geometría y payloads con **un**
commit y **un** `Regen`. **Ningún import ni reparación best-effort dentro del lote.** Un preflight
fallido deja un plan de mutación **vacío**, nunca parcial.

### 8. Fail-closed — la doctrina

> **`UNKNOWN` / `UNREADABLE` / `BROKEN`  ≠  `ABSENT` / `NEGATIVE` / `EMPTY` / `SUCCESS`**

Ninguna garantía de autoridad de ID22A puede depender de que un `null`, una excepción, un `continue`,
un `catch {}`, un fallback best-effort o un valor por defecto **atraviese correctamente una capa
tolerante preexistente**. Los estados esperados se expresan con **resultado tipado** o con
**precondición comprobada antes** de entrar en esa capa.

Esto **no obliga a eliminar los `catch` históricos**, que siguen siendo correctos para lo que fueron
escritos. Obliga a que **no decidan accidentalmente la semántica de ID22A**. Una tolerancia se conserva
solo si está **decidida explícitamente**, **no destruye autoridad** y **no produce un resultado que
parezca completo**.

### 9. Multi-vista

**Una autoridad authored lógica por `RackId`.** Las vistas hermanas de un rack pueden divergir —es
alcanzable por commits parciales históricos— y esa divergencia **no se resuelve eligiendo una vista
arbitraria**: ni la frontal, ni la primera, ni la mayoritaria. Divergencia o ilegible ⇒ **fail-closed**.

### 10. BOM

`RACKBOMTOTAL` usa **el mismo snapshot de `ProjectVariables`** durante todo el total; el **effective
resolver corre una sola vez, dentro del handler Selectivo**; y una **referencia rota es un resultado
semántico tipado** que **aborta el total** en vez de degradarse a «payload ilegible».

### 11. Ciclo de vida

**`Delete` se bloquea si existen consumidores**, listándolos. **`Unlink` materializa el efectivo
actual**, de modo que su efecto geométrico es nulo. **`RepairBroken`** es una acción **explícita y
avisada** que puede cambiar la geometría, y es el único camino por el que un literal authored vuelve a
gobernar con un binding escrito.

### 12. Duplicación

Una copia independiente lleva **`RackId` nuevo** y **las mismas referencias a `VariableId`**. El
restamp es **completo o no hay copia**: **nunca identidad parcial**.

### 13. Schema

El `ProjectVariablesDocument` tiene **versionado propio**, independiente de la línea del diseño
Selectivo, que a su vez usa **promoción sticky** según el Proposal. Y la regla de presencia:

```
READ           : NO inventar una SchemaVersion ausente
CREATE / WRITE : estampar EXPLÍCITAMENTE CurrentSchemaVersion

missing ≠ explicit "1.0"
```

### 14. Capas

**La UI permanece AutoCAD-free**: recibe DTO puros y devuelve intents. **Application** mantiene la
semántica pura y testeable. **El Plugin** es el único dueño del NOD, los `ObjectId`, la `Transaction` y
el `DocumentLock` ([ADR-0006](0006-autocad-solo-en-plugin.md)).

### 15. Alcance futuro

**ID22B** (fórmulas, que extenderá `ProjectVariable.Definition`) e **ID21** (referencias a propiedades
de racks vía `RackId` + `PropertyId`, que extenderá `PropertyValue<T>`) quedan **deliberadamente
fuera**. Extienden **tipos distintos**, y esa separación es lo que les permite avanzar sin estorbarse.

## Alternativas consideradas

| Alternativa | Por qué se rechazó |
|---|---|
| **Variables copiadas dentro de cada rack** | Es lo que hay hoy: no hay autoridad, y cambiar un valor exige editar N racks |
| **Referenciar por `Name`** | Renombrar rompería todas las referencias, que es justo lo que el requisito prohíbe |
| **Materializar el efectivo permanentemente como fallback** | Convierte el binding en decorativo: el valor persistido dejaría de derivar de la variable |
| **Fallback silencioso al literal si falta el `VariableId`** | Cambia la geometría sin que nadie se entere. Es el defecto que la doctrina nombra |
| **Borrar una variable materializando automáticamente sus consumidores** | Reescribe en silencio el diseño persistido de N racks como efecto colateral de un borrado |
| **Resolver el efectivo en cada capa** | Duplica la regla en dibujo, BOM y UI, y contradice la convención 2 de AGENTS. Es el defecto B4 |
| **Elegir una sibling arbitraria cuando las vistas divergen** | Inventa autoridad y descarta en silencio el diseño de las demás |
| **Persistir un grafo de dependencias** | ID22A tiene profundidad 1; el grafo pertenece a ID22B/ID21 |
| **Implementar fórmulas en ID22A** | Multiplica el alcance y exige ciclos, orden de evaluación y tipado de operandos |
| **Copia/restamp best-effort** | Produce identidad parcial: una copia cuyo interior afirma pertenecer a otro rack |
| **Tratar un `ProjectVariables` corrupto como registro vacío** | La siguiente escritura destruiría todas las variables del dibujo |

## Consecuencias

### Positivas

- **Una autoridad por DWG**, en vez de N copias que divergen.
- **Renombrar es seguro** para siempre, porque las referencias van por id.
- **Dibujo, BOM y UI coinciden** por construcción: todos consumen el mismo efectivo.
- **Base extensible** para ID22B e ID21 sin romper el esquema.
- **Los errores de autoridad son visibles**, no silenciosos.
- **La lógica es testeable en Application**, fuera de la mitad que no cubre ninguna suite.

### Costes — no se minimizan

- **Un NOD, un store y un guard nuevos**, en la capa que ninguna suite cubre.
- **Cambios en la persistencia del Selectivo**: el portador authored deja de reconstruirse desde
  Domain.
- **Resultado tipado en el BOM**, que cambia una interfaz compartida por **seis** handlers.
- **Redibujo transaction-aware** sobre **dos** writers distintos, incluido el de las vistas laterales.
- **Una superficie de UI central** nueva.
- **Adaptación de los cinco handlers no-Selectivo**, aunque ignoren el argumento.
- **Validación multi-vista** con su comparador y su probe tri-estado.
- **Owner Validation real en AutoCAD 2025** antes de integrar.

### Riesgos

- **Propagación sobre dibujos grandes**: sin medir. Una transacción única mantiene más estado abierto
  durante más tiempo.
- **Datos históricos divergentes** entre vistas hermanas: alcanzables, y hacen fail-closed una
  operación que el usuario esperaba trivial.
- **Payloads de major futuro o corruptos bloqueando operaciones** hasta que se reparen. Es deliberado
  —la alternativa es corromper en silencio— pero es un coste real de uso.
- **Complejidad de la transacción física**: la atomicidad exigida hay que verificarla ejecutando, no
  razonando.

## Referencias

- **Proposal V4.8** — [`I-47-proposal-v4.md`](../initiatives/I-47-proposal-v4.md), SHA
  `a0621abbd22952ad5a62bf7678212a74526a05ce`. Fuente completa y vinculante.
- **Contrato de iniciativa** — [`I-47-project-variables-foundation.md`](../initiatives/I-47-project-variables-foundation.md)
- **Discovery** — [`I-47-discovery.md`](../initiatives/I-47-discovery.md)
- **Registro de decisiones** — [`docs/automation/decisions/I-47.md`](../automation/decisions/I-47.md)
- [**ADR-0006**](0006-autocad-solo-en-plugin.md) — AutoCAD solo en el Plugin. **Se extiende**: el NOD,
  los `ObjectId` y la `Transaction` quedan del lado del Plugin, y todo lo demás en Application.
- [**ADR-0009**](0009-identidad-guid-embebida-en-dwg.md) — identidad GUID embebida en el DWG. **Se
  extiende**: `VariableId` sigue la misma disciplina de identidad estable, y `RackId` conserva su papel.
- [**ADR-0010**](0010-actualizar-redibuja-insertar-liga-vistas.md) — Actualizar redibuja, Insertar liga
  vistas. **Se extiende**: la propagación de una variable redibuja en sitio todas las vistas del
  consumidor, conservando esa semántica.
- [**ADR-0019**](0019-shell-visual-de-editores-por-composicion.md) — composición visual de editores. **Se extiende** si
  la superficie de Project Variables adopta ese precedente.
- [**ADR-0032**](0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md) — pendiente frente a
  comprometido y autoridades por fondo en el Selectivo. **Se extiende**: `ClearOverride` conserva
  exactamente su precedencia, y la autoridad por fondo no se altera.

**Este ADR no reemplaza a ninguno de los anteriores.** Los extiende.

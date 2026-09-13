# ADR-0039: Contrato de persistencia y autoridad de Custom Properties

- **Estado:** propuesto
- **Fecha:** 2026-09-13 (propuesto)
- **Decisores:** Mario Pérez, Owner del repositorio — **aceptación pendiente**; Coordinador de I-54 y Arquitecto de
  I-54 (consenso técnico **AGREED** sobre Proposal **V5** / SHA `26ca923492576185b753d6dbf2a852969df2accf`); Claude
  (redacción)
- **Iniciativa relacionada:** I-54 — `architecture/propiedades-personalizadas`
  ([contrato](../initiatives/I-54-propiedades-personalizadas.md), [Discovery](../initiatives/I-54-discovery.md),
  [Proposal V5](../initiatives/I-54-proposal-v5.md); Proposals [V1](../initiatives/I-54-proposal-v1.md) a
  [V4](../initiatives/I-54-proposal-v4.md) como registro de sus rondas;
  [registro de I-54](../automation/decisions/I-54.md))

> **Registro propuesto para la decisión del Owner.** Este ADR nace `propuesto` y **no** autoriza implementación.
> Solo el dueño del repositorio acepta o rechaza un ADR ([README](README.md)). Hasta esa decisión, la implementación
> de I-54 sigue **bloqueada** y G3 no está autorizado (contrato de I-54 §12; Proposal V5 D-20).
>
> **Autoridad normativa.** El contrato técnico consensuado es la **Proposal V5** tal como existe en
> `26ca923492576185b753d6dbf2a852969df2accf` (blob `a75444702ad354b35b67bfbbf5bb955913a02c81`). El Coordinador y el
> Arquitecto están **AGREED** sobre ese SHA exacto —el Arquitecto en la revisión final G2J, sin cambios requeridos—,
> y el registro durable está en [`decisions/I-54.md`](../automation/decisions/I-54.md). La cabecera de V5 conserva el
> estado con que se publicó y **no** se reescribe. Este ADR congela los dieciséis puntos del alcance acordado en
> V5 §15 **sin rediseñarlos**: ante una duda de detalle manda V5. Las referencias `D-nn`, `INV-nn`, `P-nn`, `R-nn`,
> `RP-nn` y `§n` remiten a V5 en ese SHA. Los nombres de tipos y miembros de código son **ilustrativos** (V5 D-19.1);
> los nombres persistidos —`RACKCAD_CUSTOM_PROPERTIES`, `CustomProperties`, `SchemaVersion`, `Entries`, `Id`, `Name`
> y `Value`— sí son contrato.
>
> **Numeración.** 0039 es el primer número libre tras censar `docs/adr/` en `origin/main`, que llega a 0035, y en
> todas las ramas remotas vivas: 0036 pertenece a I-52, 0037 a I-53 y 0038 a I-49, y ningún ref contiene ni cita un
> ADR-0039. Hasta que este registro llegue a `main`, el número se vuelve a comprobar en cada preflight de I-54.

## Aceptación del Owner

```text
ADR                  = ADR-0039 — propuesto
Proposal             = V5 @ 26ca923492576185b753d6dbf2a852969df2accf
Coordinator          = AGREED
Architect            = AGREED
Technical Consensus  = REACHED
Owner ADR Acceptance = PENDING
Consensus Freeze     = PENDING OWNER ACCEPTANCE
Implementation       = BLOCKED
```

| Campo | Valor |
|---|---|
| Owner | Mario Pérez — **PENDING** |
| Fecha | **PENDING** |
| Redacción literal recibida | **PENDING** |
| Contenido que se somete | Este ADR tal como existe en el commit que lo crea |

La aceptación tiene que ser **explícita** y posterior a la lectura de este ADR. Antes de decidir, el Owner queda
informado —sin que sea una pregunta técnica— del riesgo residual **R-12** y de los residuales **F-14a** y **F-14b**
(ver «Consecuencias»). Solo esa aceptación abre la sesión documental que la registra textualmente, actualiza este
encabezado y este bloque y versiona el Consensus Freeze; Contexto, Decisión, Alternativas consideradas,
Consecuencias y Referencias no cambian por aceptarse. Después se evalúa, en otra orden, la autorización de G3.

## Contexto

RackCad no tiene hoy metadatos descriptivos definidos por el usuario: no existe metadato de proyecto ni un mapa
clave→valor editable, y el único metadato de un rack es su `Name` (P-01). Hace falta registrar metadatos
**administrativos o comerciales** extensibles —cliente, obra, ubicación, revisión, notas— en dos alcances: el
**Proyecto**, entendido como una colección por `Database` de AutoCAD (un archivo DWG), y el **Rack**. En V1 son
**solo literales**: texto, sin tipos, unidades, fórmulas ni expresiones.

Esos metadatos **no son Project Variables**. `ProjectVariablesDocument` es la única autoridad de variables, con
versionado propio ([ADR-0034](0034-project-variables-autoridad-drawing-level.md) §1 y §13; P-03), y una propiedad
personalizada no se lee, escribe ni resuelve como variable, expresión o fórmula. I-54 **no depende** de I-49 en
ningún sentido y no toca las guardas de fórmulas.

El dato de rack tiene que convivir con la forma en que se persiste un rack. Un rack lógico son **varias
definiciones de bloque hermanas** que comparten el `Id` de su sobre `RackEmbedDocument`
([ADR-0009](0009-identidad-guid-embebida-en-dwg.md)), y Actualizar redibuja cada vista con su propio sobre, en una
transacción por vista ([ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md); P-07, P-37). Hechos
verificados en el código: `RACKCAD_PROJECT` es un Xrecord directo del NOD, y cambiar su forma lo dejaría ilegible en
todos los builds (P-02); `Compose` crea un sobre nuevo y del origen solo copia `SchemaVersion` y `ExtensionData`
(P-04); un miembro **tipado** del sobre con contenido inesperado dejaría **todo** el rack ilegible, porque
`RackEmbedStore.Deserialize` devuelve `null` ante `JsonException` (P-05); ninguna autoridad compara campos del sobre
entre hermanas (P-06); y el NOD no viaja con un rack copiado a otro dibujo, mientras que el sobre sí (P-10).

Rige la doctrina de ADR-0034 §8 —**`UNKNOWN` / `UNREADABLE` ≠ `EMPTY`**—: un contenedor presente pero ilegible,
ambiguo o de un schema futuro nunca se trata como vacío. Y habrá builds ya desplegados abriendo dibujos con estos
datos: los anteriores a I-11 no conservan campos desconocidos del sobre, y los posteriores sí (P-18).

## Decisión

### 1. Modelo e identidad

- **Una sola forma para los dos alcances**: un documento `{ SchemaVersion, Entries }` cuyas entradas son registros
  `{ Id, Name, Value }`, con `[JsonExtensionData]` en la raíz y en cada entrada (D-01). `Entries` es un array
  obligatorio que puede estar vacío; su orden es el de presentación, crear añade al final y el orden participa en la
  igualdad (D-06).
- **`Id` es un GUID estable** que Application acuña al crear y nunca deriva del nombre. Su forma persistida es
  exactamente la `D` de 36 caracteres (`8-4-4-4-12`), sin espacios, llaves ni paréntesis: se comprueba la forma antes
  de obtener el valor, se acepta cualquier caja al leer y se escribe siempre en **minúsculas**. `Guid.Empty` es
  ilegible (D-02.1..4, D-02.7).
- **La identidad es el valor del `Guid`**, nunca su texto (D-02.5). Dos entradas de la misma colección con el mismo
  valor son **`AmbiguousIdentity`**, un resultado propio y distinto de `PresentButUnreadable` (D-02.6). La identidad
  es **local al alcance**: el mismo GUID en el proyecto y en varios racks no significa nada en V1 (D-02.8).
- **`Name` es presentación, nunca identidad**, clave de búsqueda ni clave de agregación; ninguna API de Application
  resuelve, busca ni une una propiedad por nombre (D-05.3, INV-02). Al escribir se valida UTF-16 bien formado, se
  rechazan los no-caracteres Unicode, se normaliza a NFC, se recorta y se exige unicidad sin distinguir mayúsculas
  frente a las otras entradas. Al leer, un `Name` vacío o con un no-caracter es ilegible, y dos entradas con el mismo
  nombre se leen con diagnóstico (D-05.1, D-05.2).
- **`Value` es siempre un string JSON literal**, que puede estar vacío; nulo, ausente o de otro tipo JSON es ilegible
  (D-04.1), y no hay campo de tipo en V1 (D-04.2). Se guarda tal como llega, sin normalizar, después de validar que
  es UTF-16 bien formado (D-05.4).
- Los nombres de miembro JSON se escriben en PascalCase y se leen sin distinguir mayúsculas. **Cualquier** objeto
  del documento, a cualquier profundidad, con dos nombres de miembro iguales sin distinguir mayúsculas hace el
  documento `PresentButUnreadable`: ninguna capa aplica «gana el primero» ni «gana el último» (D-01.5, INV-24).

### 2. Evolución del formato

- `SchemaVersion` es del **documento de propiedades**, sin valor por defecto: ausente no equivale a `"1.0"`. Al leer
  se exige la forma exacta `major.minor`, con `major ≥ 1` (D-01.1).
- **Todo escritor 1.x emite siempre `SchemaVersion` y `Entries`**, también con cero entradas (D-01.4, D-04.4).
- **Regla de evolución contractual** (D-04.3):

  | Cambio | Versión |
  |---|---|
  | Cambiar el significado, la validez de lectura o el tipo JSON de `Value` | **MAJOR** |
  | Cambiar la estructura de una entrada o la de la colección (raíz, `Entries`, orden, identidad) | **MAJOR** |
  | Cambiar la cota de profundidad del documento | **MAJOR** |
  | Hacer depender el significado o la autoridad de la colección del `Kind` del rack | **MAJOR** |
  | Añadir un campo **informativo** que un lector de minor menor puede ignorar y conservar sin dejar de ser correcto | MINOR |
  | Ajustar los límites de escritura de conteo y longitud, que no son reglas de lectura | ninguna |

- **La cota de profundidad es 16 y es una constante del formato 1.x**, no tuning ni un límite configurable. El objeto
  raíz cuenta 1 y cada objeto o array anidado suma 1. Todo escritor 1.x produce documentos de profundidad ≤ 16,
  **subirla exige MAJOR**, y un documento legible más profundo es `DepthLimitExceeded`, de solo lectura (D-05.7).
- **Nada que un build posterior pueda interpretar válidamente se considera destructible** (D-04.5).
- **Diferencia declarada con ADR-0034 §3.** ADR-0034 persiste un discriminador de tipo desde el inicio porque su
  consumidor interpreta el valor numéricamente. En V1 ningún consumidor interpreta `Value`, y la protección que §3
  busca la da el **major del documento**: introducir tipos es MAJOR, un build 1.x ve un documento 2.x como
  `IncompatibleMajor` y conserva sus bytes, y el sobre no se promociona. Un `Type` con un único valor posible sería
  una promesa sin comprobación (D-04).

### 3. Contenedor de Proyecto

- Entrada **`RACKCAD_CUSTOM_PROPERTIES`** del diccionario de objetos con nombre (NOD): un **Xrecord directo** cuyo
  texto va en trozos de 255 caracteres. Clave y forma quedan congeladas desde la primera escritura (D-07.1). Es una
  entrada **separada** de `RACKCAD_PROJECT`, que no cambia (D-07.7, INV-16).
- **Lectura física tri-estado que nunca lanza** —`Absent`, `Present` o `PresentButUnreadable`—, con la semántica de
  `ProjectVariablesData`: una entrada que no es Xrecord, un Xrecord sin texto o una excepción de AutoCAD son ilegibles
  (D-07.2).
- **Store en Application** con los resultados `Absent`, `Readable`, `PresentButUnreadable`, `IncompatibleMajor`,
  `AmbiguousIdentity` y `DepthLimitExceeded`; solo `Absent` y `Readable` permiten escribir (D-07.3). La clasificación
  es determinista y **empieza por la versión** (D-07.4):
  1. texto no JSON o no transcodificable, raíz que no es objeto, nombres de miembro repetidos o strings no
     decodificables a cualquier profundidad, o `SchemaVersion` ausente o mal formada → `PresentButUnreadable`;
  2. major mayor que 1 → `IncompatibleMajor`, sin interpretar las entradas;
  3. `Entries` o alguna entrada mal formadas, `Id` sin la forma exacta o `Guid.Empty`, `Name` o `Value` que no son
     string, o `Name` vacío o con un no-caracter → `PresentButUnreadable`;
  4. profundidad mayor que 16 → `DepthLimitExceeded`;
  5. ids repetidos por valor → `AmbiguousIdentity`;
  6. en otro caso → `Readable`, con diagnóstico de nombres repetidos.
- **El store nunca lanza por contenido externo** y nunca usa `catch (Exception)`: captura solo las clases de excepción
  que el contenido externo puede provocar, en la operación que las produce, y las convierte en `PresentButUnreadable`.
  La única excepción es una defensa acotada ante normalizadores NFC de plataforma más estrictos, que no cambia la
  clasificación (D-07.4, INV-23).
- **Fail-closed al escribir.** La escritura relee, acredita esa lectura en Application, aplica el intent por id,
  valida y escribe en **una** transacción, y solo tras `Absent` o `Readable` **de esa misma transacción** (D-07.5).
- **Independencia en ambos sentidos.** Las propiedades de Proyecto no leen `RACKCAD_PROJECT` ni dependen de su
  estado, y Project Variables no lee `RACKCAD_CUSTOM_PROPERTIES`: un registro de variables corrupto no bloquea las
  propiedades, y una colección de propiedades corrupta no cambia `RACKVARIABLES`, `RACKEDITAR` ni `RACKBOMTOTAL`
  (D-07.6, INV-17).

### 4. Contenedor de Rack

- Miembro **`RackEmbedDocument.CustomProperties`**, de tipo **`JsonElement?`** y **omitido cuando es nulo**
  (`JsonIgnoreCondition.WhenWritingNull`). Contiene como objeto JSON el documento del punto 1 (D-08.1). Un sobre sin
  propiedades se serializa byte a byte igual que antes de I-54 (INV-08).
- **Store propio.** Solo el store de propiedades interpreta el miembro, con las reglas del punto 3: nulo, ausente,
  `Undefined` o `Null` → `Absent`; un valor que no es objeto → `PresentButUnreadable`; un objeto → la clasificación
  del punto 3 (D-08.2). `RackEmbedStore` no cambia.
- `RackEmbedComposer.Compose` **hereda** el miembro del sobre origen sin cambiar su firma ni sus llamadores, y una
  función pura del compositor devuelve un sobre igual al origen en todo salvo `CustomProperties`, con la versión
  resuelta sin degradar (D-08.3).
- **El sobre exterior no sube de major**: `RackEmbedDocument.CurrentSchemaVersion` sigue en `"1.0"` (D-08.5; ver el
  punto 11).

### 5. Aislamiento

- **Afirmación acotada, y ninguna otra**: dado un sobre JSON sintácticamente válido y dentro del límite de
  profundidad soportado, el tipo o la forma JSON de `CustomProperties` no vuelve ilegible el sobre (D-08.4).
- **Restricciones normativas A..G**, cada una con su prueba (D-08.4):
  - **A.** Una corrupción sintáctica o un truncado, también dentro del miembro, vuelve ilegible el sobre en todos los
    builds: para ese caso no se afirma ningún aislamiento.
  - **B.** 64 niveles de anidado dentro del miembro vuelven ilegible el sobre en todos los builds. La cota del formato
    es 16, y un miembro legible más profundo es `DepthLimitExceeded`.
  - **C.** Nunca se escribe `default(JsonElement)`: el compositor lo normaliza a `null`.
  - **D.** Un elemento JSON de tipo `Null` se normaliza a `null` y no se emite.
  - **E.** Un `null` literal equivale a ausente y desaparece al reescribir.
  - **F.** `ExtensionData` nunca contiene una clave igual, sin distinguir mayúsculas, a un miembro declarado del
    sobre; el compositor rechaza un origen que la tenga como error de programación.
  - **G.** El store de propiedades nunca lanza por contenido externo.
- **Sin sobreafirmación.** Para los consumidores que reescriben el sobre, la garantía de I-54 es **no introducir un
  modo de fallo nuevo** en los sobres que la base puede **leer y reserializar** (INV-07). Quedan fuera los residuales
  declarados: la clave del miembro repetida en el JSON del sobre, y **F-14a** y **F-14b**, que son **preexistentes**
  en `RackEmbedStore`, no los introduce I-54 y **no se corrigen aquí**:
  - **F-14a — UTF-16 crudo inválido** en el texto del sobre: `RackEmbedStore.Deserialize` lanza **al leer**, y el
    barrido falla antes de cualquier mutación.
  - **F-14b — escape JSON de un surrogate suelto**: el sobre se deserializa, pero reserializar **ese** sobre lanza y
    ese sobre no se escribe. Cada consumidor existente conserva su granularidad transaccional: un flujo que escribe
    por vista o por etapa puede conservar cambios ya confirmados, y `RACKEDITAR`, que confirma una transacción por
    vista, puede quedar **parcialmente actualizado** entre vistas, como ya ocurre en la base (P-37). Solo el ejecutor
    de Rack de I-54 garantiza que no se escribe ningún miembro (punto 9).

  Si el escape está dentro de `CustomProperties`, el store de propiedades clasifica el miembro como
  `PresentButUnreadable` sin lanzar (D-07.4).

### 6. Preservación del sobre

**Invariante arquitectónico** (D-21, INV-09): todo sobre derivado de un rack existente se origina en
`RackEmbedComposer.Compose(source, …)` con el sobre **real** de ese rack como `source`, o en el re-estampado del
**mismo** objeto deserializado. `Compose(null, …)` solo se permite para un **rack nuevo** y para una **importación
desde la biblioteca**.

El invariante rige para el redibujo, la vista nueva, la propagación de Project Variables, el duplicado, el layout, el
espejo y **cualquier camino futuro**. La escritura de propiedades de I-54 lo cumple aplicando su función pura sobre el
sobre fresco de cada miembro. Lo vigilan pruebas de comportamiento y, como defensa secundaria, guardas estructurales
sobre la construcción del sobre, el censo de llamadas a `Compose` y la forma del re-estampado (D-21).

### 7. Kind

- La semántica de una propiedad personalizada **no depende** del `Kind` del rack, y hacerla depender es MAJOR
  (punto 2).
- **Pero la autoridad de I-54 solo escribe si el `Kind` es conocido.** Entre los miembros de un `RackId`, el `Kind` se
  compara sin distinguir mayúsculas (D-09.5):
  - algún miembro con `Kind` en blanco, o más de un kind no vacío distinto → **`MixedKind`**, de solo lectura;
  - un único kind no vacío, común a todos, que el build **no conoce** → **`UnknownKind`**, de solo lectura: no se
    crea, renombra, cambia valor, elimina ni unifica, y no se modifica el sobre;
  - un único kind común y conocido → la autoridad continúa (punto 8).
- «Conocido» lo decide un predicado que el Plugin **inyecta** en Application; Application no depende del registro de
  handlers del Plugin (D-09.5, D-22.2).
- **Por qué.** Todo camino que hoy reescribe un sobre exige un kind conocido (P-28), y un kind nuevo entra sin cambiar
  la versión del sobre (P-29). Así I-54 **nunca** es el primer comando que reescribe un kind futuro (INV-22).

### 8. Autoridad por `RackId`

- **Barrido.** Dentro de la transacción del llamador se recorren todas las definiciones con payload RackCad,
  **también las ilegibles** (D-09.1).
- **Pertenencia.** Una definición es miembro de `R` si su sobre es legible, su `Id` no está vacío y es igual a `R` sin
  distinguir mayúsculas, y no depende de un xref (D-09.2).
- **Pertenencia indeterminada, fail-closed.** Si alguna definición recorrida, **colocada o no** y no dependiente de
  xref, tiene un payload RackCad no interpretable, la pertenencia de **todo** `RackId` es indeterminada (D-09.3,
  INV-12):
  - se bloquean todas las escrituras de alcance Rack del dibujo, unificación incluida;
  - las lecturas siguen en solo lectura, con un diagnóstico por definición;
  - no se bloquean las propiedades de Proyecto ni ninguna función ajena a las propiedades.

  Un sobre legible con `Id` en blanco no es miembro ni bloquea; si es el bloque elegido, el resultado es `NoIdentity`.
- **Xref y dependientes fuera.** Una definición `IsFromExternalReference` o `IsDependent` no es miembro, no bloquea y
  nunca se escribe, y elegirla se rechaza con diagnóstico (D-09.4, D-14).
- **Todas las hermanas canónicamente iguales, y nunca se elige una.** Solo hay escritura con autoridad `Single` o con
  la unificación segura, y ningún resultado presenta los valores de una vista como los del rack (D-09.9, INV-01,
  INV-11).
- **Orden único de resultados** (D-09.8): `XrefRejected` → `NoIdentity` → `IndeterminateMembership` → `MixedKind` →
  `UnknownKind` → `CustomPropertiesReadOnly` → `Divergent` → `Single`. Todo resultado distinto de `Single` es de solo
  lectura, salvo la unificación segura desde `Divergent`.
- **Igualdad canónica** (D-09.7), sobre documentos que el store ya garantiza sin nombres repetidos ni strings no
  decodificables:
  - **ausente ≡ vacío canónico**: `Absent` y un `Readable` con cero entradas y sin `ExtensionData` de raíz son iguales
    entre sí, con cualquier minor;
  - dos `Readable` son iguales si coinciden en **todo** su contenido salvo el minor: el mismo major, el `ExtensionData`
    de raíz igual en profundidad, el mismo número de entradas y, entrada a entrada y en orden, el `Id` por valor de
    `Guid`, `Name` y `Value` ordinales y el `ExtensionData` de la entrada igual en profundidad;
  - **el minor no participa** de la comparación; **los datos que un minor añada sí participan**, a través de
    `ExtensionData`.
- **Unificación segura** (D-09.10, INV-18). Solo existe desde `Divergent` y con todos los miembros `Readable` o
  `Absent`. La vista origen la **elige el usuario**, sin selección por defecto, y la confirmación exige una casilla
  tras mostrar el contenido de cada vista. Nunca sobrescribe una colección con `ExtensionData` de raíz o de entrada, ni
  con un minor mayor que el del origen, y un origen `Absent` es el documento vacío canónico. Dentro de la transacción
  de escritura se revalidan el conjunto de miembros y la forma canónica de **cada** miembro frente a lo mostrado. **El
  sistema nunca elige.**

### 9. Escritura

- **Una transacción del llamador** para todo el ciclo del ejecutor de Rack de I-54: barrido, pertenencia, autoridad,
  mutación, escritura y confirmación (D-22.1).
- **Snapshot fresco.** El preflight sobre la instantánea solo avisa. La decisión se toma sobre la **lectura fresca**
  dentro de la transacción: se reevalúa el orden único y se aborta sin escribir si cambió la identidad, el kind, la
  autoridad, la validez del intent o el estado escribible (D-22.5).
- **Application decide.** El Plugin entrega una proyección plana por definición —handle, nombre de bloque,
  colocación, dependencia de xref, legibilidad y sobre deserializado—, **sin** `ObjectId`, `Database` ni
  `Transaction`. Application decide la pertenencia, el kind, la autoridad, la validación y el **plan de mutación**, y
  el Plugin escribe y confirma **sin tomar decisiones semánticas** (D-22.2..4, INV-13, INV-21).
- **Todos los payloads preparados antes de la primera escritura** (D-22.10). El plan se construye completo, con todos
  los sobres serializados, antes de la primera `RackBlockData.Write`. Si serializar el sobre de **cualquier** miembro
  lanza —por ejemplo, por F-14b en otro campo de ese sobre—, no hay commit, no se escribe ningún miembro, el error es
  visible y los datos previos quedan intactos. Es atomicidad de infraestructura, **propia del ejecutor de I-54**, y no
  se extrapola a `RACKEDITAR`, `RACKDUPLICAR`, `RACKLAYOUT`, Project Variables ni otros consumidores existentes.
- **Atomicidad lógica** (INV-05): tras un commit exitoso, todos los miembros del `RackId` quedan canónicamente iguales
  al estado destino; si no, no cambia ninguno. Físicamente solo se escriben los miembros que difieren del destino.
- Sin redefinir bloques, importar, purgar ni regenerar: solo `RackBlockData.Write` (D-22.7). El ejecutor de Proyecto
  sigue el mismo esquema sobre el NOD (D-22.9).
- Escribir propiedades no toca geometría, diseño, `Kind`, `Id`, `Name`, `View`, `Section` ni `ExtensionData` del sobre
  (INV-04). Renombrar cambia solo `Name` y cambiar valor solo `Value`, y los dos conservan el `ExtensionData` de la
  entrada. Eliminar la última entrada deja `Entries: []`: nunca se borra el miembro del sobre ni la entrada del NOD
  (D-10.1, INV-03).

### 10. Estados no escribibles y recuperación

- **Sin recuperación destructiva en V1**, ni en Proyecto ni en Rack: no hay descarte, reparación ni sobrescritura de
  contenido no escribible. Cualquier recuperación futura exige una decisión explícita del Owner en otra iniciativa
  (D-09.11, INV-06).
- **Solo lectura**, con bytes intactos y diagnóstico: `PresentButUnreadable`, `AmbiguousIdentity`,
  `IncompatibleMajor` y `DepthLimitExceeded`. Sobre ellos no se ejecuta ninguna operación, tampoco eliminar ni vaciar
  (D-13, D-10.1). También son de solo lectura los estados de rack `XrefRejected`, `NoIdentity`,
  `IndeterminateMembership`, `MixedKind` y `UnknownKind` (D-09.8).
- **`UNKNOWN != EMPTY` en los dos contenedores**: un contenedor no escribible nunca se ofrece como colección vacía, ni
  para «crear la primera» (D-13).
- **La unificación segura es la única escritura posible fuera de `Single`**, y solo en las condiciones congeladas del
  punto 8.
- Los límites de escritura de conteo y longitud validan solo lo que la operación introduce, y eliminar siempre se
  permite frente a ellos. Sus **números** son tuning y quedan fuera de este ADR (D-05.6; V5 §15).

### 11. Compatibilidad

**Sobre exterior, sin promoción de major** (D-08.5, matriz verificada con sondas, P-18):

| Build que abre el dibujo | Lee un sobre con el miembro | Al reescribir el sobre | Si el sobre fuera major 2 |
|---|---|---|---|
| Anterior a I-11 | legible | **pierde** el miembro: no tiene `ExtensionData` | lo lee como legible: no tiene guarda de versión |
| I-11 … anterior a I-54 | legible; el miembro va a `ExtensionData` | **lo conserva exacto**, con `Compose(source)` y con el re-estampado | ilegible: el rack sale de `RACKEDITAR`, `RACKLISTA`, BOM y duplicado |
| I-54 | lo interpreta | lo conserva (`Compose` lo hereda) | ilegible |

Promover el major del sobre **no protege** a los builds anteriores a I-11, que ignoran el major y pierden el miembro
de todos modos, y en cambio dejaría ilegible **todo** rack con propiedades para los builds I-11..I-53, que hoy las
conservan. Por eso el sobre sigue en `"1.0"`.

**Colección de propiedades** (V5 §9):

- **Futuro, mismo major 1**, conforme (profundidad ≤ 16): editable; los campos nuevos se conservan, el minor no se
  degrada y la unificación queda bloqueada sobre ellos.
- **Documento 1.x no conforme** (profundidad > 16): `DepthLimitExceeded`, solo lectura.
- **Major futuro de la colección**: `IncompatibleMajor`, solo lectura con los bytes conservados; el sobre sigue en
  1.x, así que el rack sigue legible y dibujable.
- **Rack de un kind que el build no conoce**: `UnknownKind`, solo lectura; el resto de comandos, como hoy.
- **Payload Selectivo anterior al sobre** (`Kind` en blanco): su rack da `MixedKind`, solo lectura.

### 12. Fronteras de V1

Quedan fuera de V1 como **frontera de alcance**, no como clasificación de los datos (D-12.2):

- **Transporte por la biblioteca.** No se escriben propiedades en `.rackcad.json` ni se leen al importar: un rack
  guardado en la biblioteca no las lleva y uno insertado desde ella nace sin propiedades (`Compose(null)`). El punto de
  extensión aditivo existe —un slot en `RackProjectDocument`— y queda sin diseñar (D-12, INV-15).
- **Dibujo y cuadros de título**: atributos, textos, rotulación, campos de AutoCAD o `DWGPROPS`. BOM, CSV, XLSX y
  `RACKLISTA` no cambian (D-12.1, D-15).
- **Expresiones y fórmulas**: parser, operandos y referencias entre racks (D-16, INV-14).
- **Plantillas** (D-17).
- **Drawing-level**: anotaciones de nivel dibujo, hoja o layout, y proyectos de varios DWG. El alcance Proyecto es una
  colección por `Database`, no una autoridad de dibujo ni multi-DWG (D-03.2).
- **View-level**: propiedades por vista, por referencia o por instalación (D-03.1).
- **Agregación entre racks.** V1 no define ninguna clave de agregación: ni el mismo `Name` ni el mismo id en racks
  distintos significan la misma propiedad conceptual, y ninguna API agrupa colecciones de racks distintos (D-03.4,
  INV-19).

Tampoco entran en V1 los tipos, unidades o listas de valores; la herencia o el valor por defecto entre Proyecto y
Rack; la transferencia o la fusión de propiedades de proyecto entre dibujos (D-14); ni la edición desde los editores de
sistema.

### 13. Futuro

- Todo consumidor futuro —dibujo, expresiones o plantillas— **referencia por identidad estable** (`CustomPropertyId`),
  lee **de la autoridad** (`Single` en Rack; `Absent` o `Readable` en Proyecto) y no del sobre crudo, y ante cualquier
  otro estado no presenta ningún valor (D-15, D-16, D-17).
- **Sin fijar la sintaxis de I-49.** Este ADR no fija tokens, namespaces, ámbitos ni semántica de operando, y **no
  reclama `Rack` ni `Project`**, que ADR-0038 (I-49) reserva conceptualmente para ID20. La relación entre ID24 e ID20
  se decide en ID20 (D-16).
- Una plantilla futura decide dónde vive, si reutiliza ids y qué política de ids aplica. I-54 solo garantiza una
  identidad estable local al alcance y la posibilidad de añadir campos informativos por minor (D-17).

## Alternativas consideradas

| Alternativa | Por qué se descarta | Fuente |
|---|---|---|
| **`Dictionary<string,string>`** | La identidad sería el nombre: renombrar cambia la clave y rompe toda referencia futura, y no admite evolución por entrada. Queda como proyección posible, nunca como forma persistida; I-47 ya la había rechazado (A4) | V5 §5 |
| **Propiedades tipadas en V1** | Introducen tipos sin consumidor y un discriminador que habría que hacer cumplir, y un valor que no es string haría fallar a los lectores de texto ya desplegados. La protección se obtiene con el major del documento (punto 2) | V5 §5, D-04 |
| **Dentro de `ProjectVariablesDocument`** | Acopla la lectura, la acreditación y el bloqueo de dos autoridades distintas (ADR-0034 §1), e I-49 extiende ese documento | D-07 |
| **Sub-diccionario bajo `RACKCAD_PROJECT`** | `RACKCAD_PROJECT` es un Xrecord directo: todo build existente leería la entrada como ilegible y bloquearía tres comandos | D-07, P-02 |
| **`Database.SummaryInfo` como autoridad de Proyecto** | Sin id, sin versión ni campos desconocidos, y editable fuera de RackCad sin guarda | D-07 |
| **DTO de diseño por sistema** | Seis formatos, dos de ellos sin `ExtensionData`, cruce con I-50 e I-53 y acoplamiento con la autoridad del Selectivo | D-08 |
| **Miembro tipado en el sobre** | Un contenido inesperado lanza al deserializar el sobre y deja ilegible el rack entero | D-08, P-05 |
| **Miembro `string` con el JSON dentro** | Un cambio de tipo JSON del miembro lanza `JsonException`; `JsonElement?` lo tolera y evita el doble escape | D-08, P-17 |
| **Solo en `ExtensionData`, sin declarar** | Abusa de «campos que este build no conoce» en el build que sí los conoce | D-08 |
| **Xrecord propio en la definición de bloque** | `RackCloner` no lo copia: se perdería en toda copia independiente | D-08 |
| **Mapa `RackId → propiedades` en el NOD** | No viaja con el rack a otro dibujo y obligaría a `RACKDUPLICAR` a escribir el NOD | D-08, P-10 |
| **Autoridad en la `BlockReference`** (XData, atributos) | La referencia solo coloca (ADR-0010) | D-08 |
| **Promover el major del sobre** | No protege a los builds anteriores a I-11 y dejaría ilegible todo rack con propiedades para I-11..I-53 (punto 11) | D-08.5 |
| **Elegir una hermana** (la primera o la frontal) **o escribir solo las legibles** | Divergencia o escritura parcial silenciosas | D-09 |
| **Ignorar los payloads ilegibles**, o bloquear solo con evidencia de pertenencia | Escritura parcial silenciosa si el ilegible era un miembro, o interpretar a medias formatos desconocidos con falsos negativos | D-09.3 |
| **Escribir sobre un kind desconocido** | I-54 sería el primer comando que reescribe un kind futuro, cuya semántica podría ser otra | D-09.5 |
| **Descarte o reparación de lo ilegible en V1** | Destruiría datos que un build posterior podría entender; exige una decisión explícita del Owner | D-09.11 |

## Consecuencias

### Positivas

- Metadatos con **identidad estable desde el primer día**: renombrar no rompe nada, y un consumidor futuro puede
  referenciar por id.
- Las propiedades de rack **viajan con el rack** —redibujo, vista nueva, propagación de variables, `RACKDUPLICAR`,
  `RACKLAYOUT` y copia a otro dibujo— sin cambiar esos flujos, porque viven en el sobre y `Compose` las hereda. Una
  copia independiente conserva ids y valores, y una copia enlazada es el mismo rack (D-11, D-14, INV-10).
- **Fallo cerrado sin pérdida**: lo ilegible, ambiguo, futuro o no conforme se conserva y se muestra en solo lectura.
- **Dos autoridades independientes**: un registro de variables corrupto no bloquea las propiedades, ni al revés.
- **Sin cambio de formato de lo existente**: `RACKCAD_PROJECT` y el major del sobre quedan intactos, y un sobre sin
  propiedades sigue siendo byte a byte igual.

### Costes y tradeoffs aceptados

- **Metadatos de rack replicados** en el sobre de cada vista hermana: más bytes por rack, y la igualdad entre hermanas
  hay que comprobarla (punto 8) en lugar de tenerla por construcción.
- **`UnknownKind` es de solo lectura**: un rack de un kind posterior, abierto en un build anterior, no admite editar
  sus propiedades (RP-09).
- **La pertenencia indeterminada puede bloquear las escrituras de Rack** de todo un dibujo por una sola definición
  ilegible ajena, incluso no colocada; Proyecto y el resto de funciones siguen (RP-01). ADR-0034 ya aceptó ese coste en
  «Riesgos».
- **Cota de profundidad 16**: un documento 1.x no conforme queda en solo lectura, sin salida en V1 (RP-10), y subir la
  cota exigirá un major.
- **La biblioteca no transporta metadatos en V1**: un rack guardado en la biblioteca y vuelto a insertar nace sin
  propiedades (D-12.3).
- **Un rack `Divergent` puede no tener salida en V1** si toda colección que se sobrescribiría lleva campos desconocidos
  o un minor mayor (RP-02). La unificación exige además una elección explícita del usuario.
- **Un `Kind` en blanco** en alguna vista deja el rack en solo lectura, en un estado que ningún build actual produce
  (RP-06).

### Riesgos residuales declarados

- **R-12 — builds anteriores a I-11.** Al reescribir el sobre **pierden** las propiedades de rack, porque no tienen
  `ExtensionData`, y promover el major del sobre no lo evitaría (punto 11). Es un riesgo informado al Owner, no una
  pregunta abierta.
- **F-14a y F-14b no se corrigen aquí.** Son residuales **preexistentes** de `RackEmbedStore` (punto 5), registrados
  en [ideas-futuras.md](../ideas-futuras.md) como candidatos a una iniciativa futura de robustez del sobre. F-14a falla
  al leer, antes de mutar. F-14b falla al reserializar el sobre afectado, con el resultado que dicte la granularidad
  transaccional de cada consumidor existente —`RACKEDITAR` puede quedar parcialmente actualizado entre vistas—,
  mientras que el ejecutor de Rack de I-54 no escribe ningún miembro (punto 9).
- **UNDO.** No se afirma su granularidad para una escritura de Xrecords sin redefinición; la observa la validación del
  Owner (D-22.8).
- **Definiciones dependientes de xref.** El trato de AutoCAD es inferido; el diseño es conservador —nunca se
  escriben— y lo observa la validación del Owner (D-09.4, RP-07).

## Relación con otros ADR

- **Aplica sin reemplazarlos**: [ADR-0006](0006-autocad-solo-en-plugin.md) (AutoCAD solo en el Plugin),
  [ADR-0009](0009-identidad-guid-embebida-en-dwg.md) (identidad de rack por GUID en el sobre, compartida por sus
  vistas), [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md) (Actualizar redibuja con el sobre de cada
  vista; la referencia solo coloca) y [ADR-0034](0034-project-variables-autoridad-drawing-level.md) (autoridad
  distinta de Project Variables, doctrina fail-closed de §8 y una autoridad por `RackId` sin elegir vista de §9). Se
  aparta **de forma declarada** de ADR-0034 §3 en el discriminador de tipo (punto 2).
- **Acota sin reabrirla** la frase de [ADR-0035](0035-visibilidad-de-cotas-por-tipo-de-vista.md) según la cual
  `RackEmbedComposer` descarta campos nuevos del sobre: vale para campos declarados que `Compose` no hereda, y
  `CustomProperties` sí se hereda.
- **ADR de iniciativas paralelas**, en sus ramas y sin integrar (medidos en G2-FREEZE):
  - **ADR-0036** (I-52, RACKMIRROR; `propuesto` en `8e2ae4fb9606c246bcd5d666a8f10e35f042a901`): incluye las
    propiedades de rack de I-54 en su lista cerrada de metadata **portadora**, que viaja intacta desde el origen, y su
    orden reflejar → `Compose(sobre fuente)` → re-estampado cumple el invariante del punto 6.
  - **ADR-0037** (I-53, reutilización de cabecera; en su rama en `7de424e4f0f69f6e2fb18fd7c2e018479aab3fbe`): no
    decide nada sobre el sobre ni sobre Custom Properties.
  - **ADR-0038** (I-49, motor de expresiones; en su rama en `364d6c06e44273a63a7b6f6509daf357611ea77a`): reserva
    `Rack.*`, `Project.*` y el ámbito `Rack` para ID20; este ADR no los reclama (punto 13).

## Referencias

- [Proposal V5 de I-54](../initiatives/I-54-proposal-v5.md) en `26ca923492576185b753d6dbf2a852969df2accf`: contrato
  técnico consensuado; su §15 es el alcance de este ADR, y su §2.2 el mapa de SHAs pre y post rebase.
- [Discovery de I-54](../initiatives/I-54-discovery.md): evidencia sobre `BASE_SHA`
  `46fcac2b071929d2bd5b07aa28373941417f74a8`, riesgos R-01..R-15 y hallazgos F-01..F-13.
- [Contrato de I-54](../initiatives/I-54-propiedades-personalizadas.md) y
  [registro de decisiones de I-54](../automation/decisions/I-54.md).
- [ADR-0006](0006-autocad-solo-en-plugin.md), [ADR-0009](0009-identidad-guid-embebida-en-dwg.md),
  [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md),
  [ADR-0034](0034-project-variables-autoridad-drawing-level.md) y
  [ADR-0035](0035-visibilidad-de-cotas-por-tipo-de-vista.md); como paralelas, ADR-0036 (I-52), ADR-0037 (I-53) y
  ADR-0038 (I-49) en sus ramas.
- [Contrato de I-11](../initiatives/I-11-persistencia-uniforme.md): campos desconocidos y versión no degradada.
- [WORKFLOW](../WORKFLOW.md) y el [índice de ADR](README.md).
- Hallazgos laterales F-01..F-13, F-14a y F-14b: [ideas-futuras.md](../ideas-futuras.md), sección «I-54 — hallazgos
  fuera de alcance».
- Tag `archive/i-54-custom-properties-pre-rebase-5d25da8` → `5d25da89972df2468f1d03243301761f5463e6eb`: historial
  pre-rebase revisado.
- Evidencia ejecutada con sondas de `System.Text.Json` sobre .NET 8.0.29, fuera del repositorio (V5 P-17..P-20 y
  P-30..P-36).

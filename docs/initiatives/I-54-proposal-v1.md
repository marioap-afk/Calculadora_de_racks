# I-54 — Proposal V1: Custom Properties Foundation (ID24)

```text
Documento      = PROPOSAL V1 — NOT CONSENSUS
Executor       = PROPOSED V1
Coordinator    = NOT REVIEWED
Architect      = NOT REVIEWED
Owner          = NOT ASKED
Consensus      = NOT REACHED
Implementation = BLOCKED
ADR            = RECOMENDADO (§11); sin numero y sin texto
Base           = BASE_SHA 46fcac2b071929d2bd5b07aa28373941417f74a8
Discovery      = docs/initiatives/I-54-discovery.md — G1 @ 195964b00006d2be74eefb8b6ae4f5f46de1cfc9,
                 mas un addendum en el mismo commit que esta Proposal (§2.5 re-medicion de paralelas;
                 §11.5 comandos verificados; §15 F-13)
Paquete        = docs/initiatives/I-54-architect-review-package.md
```

> Esta Proposal **no autoriza** nada. Sus decisiones son propuestas hasta que Coordinator y Architect
> declaren `AGREED` sobre el **mismo** SHA de este documento y el Owner lo apruebe. Las citas `[D §n]`
> remiten a secciones del [Discovery](I-54-discovery.md), donde vive la evidencia con archivo y linea.

## 0. Vocabulario

| Termino | Significado en esta Proposal |
|---|---|
| **Propiedad personalizada** | Entrada `{ Id, Name, Value }` definida por el usuario. No es una variable de proyecto, no es geometria y no participa en el BOM |
| **Coleccion** | Lista **ordenada** de entradas de un alcance |
| **Alcance Proyecto** | Una coleccion por DWG |
| **Alcance Rack** | Una coleccion por `RackId`, replicada en el sobre de cada vista hermana |
| **Hermanas** | Definiciones de bloque cuyo sobre tiene el mismo `RackId` [D §6.3] |
| **Sobre** | `RackEmbedDocument`, en el diccionario de extension de la definicion [D §5.2] |
| **Contenedor** | Donde vive el texto de una coleccion: entrada del NOD (Proyecto) o miembro del sobre (Rack) |

## 1. Premisas verificadas

| # | Premisa | Discovery |
|---|---|---|
| P-01 | No existe ningun metadato descriptivo de proyecto ni mapa clave→valor editable; el unico metadato de rack es `Name` | §3 |
| P-02 | `RACKCAD_PROJECT` es un Xrecord directo del NOD; un build existente leeria un `DBDictionary` en esa clave como `PresentButUnreadable` y bloquearia tres comandos | §4.1, §4.6 |
| P-03 | `ProjectVariablesDocument` es «la unica autoridad de variables de proyecto» con versionado propio; un registro ilegible bloquea `RACKVARIABLES`, `RACKEDITAR` del Selectivo y `RACKBOMTOTAL` | §4.3, §4.7 |
| P-04 | El sobre es la unica costura uniforme de los seis kinds; `Compose` solo hereda `SchemaVersion` y `ExtensionData` | §5.3 |
| P-05 | `RackEmbedStore.Deserialize` devuelve `null` ante `JsonException`: un miembro **tipado** con contenido de forma inesperada dejaria el rack entero ilegible | §5.1, R-02 |
| P-06 | Ninguna autoridad compara campos del sobre entre hermanas; `SelectiveAuthoredAuthority` excluye por diseño lo que vive en el sobre | §6 |
| P-07 | `RACKEDITAR` compone cada hermana con **su propio** sobre, en transacciones por vista; el ejecutor de variables, en una sola transaccion | §7.2 |
| P-08 | `RestampEnvelope` muta y serializa el mismo objeto; `RackCloner` crea BTR nuevo y escribe solo el payload; nada de esto tiene prueba | §8 |
| P-09 | Ningun sistema exporta campos del sobre a `.rackcad.json`; importar acuña GUID nuevo y sobre limpio | §9 |
| P-10 | El NOD no viaja con un rack copiado a otro dibujo; el sobre si | §4.8, §9.4 |
| P-11 | `RackEmbedStore` escribe nulos: un miembro nuevo nulo cambiaria los bytes de todo sobre | §10.3 |
| P-12 | Guardas prohiben `PropertyValues` (Domain/Plugin/UI), `rackProperty` y `RackPropertyReference` (Application/Plugin/UI) | §11.6 |
| P-13 | I-49 reserva `Rack`/`Project` y `Rack.X`/`Project.X` para ID20; su motor solo maneja `double` | §2.3 |
| P-14 | Cruce productivo con I-50 = 0 si no se tocan DTO ni Domain de sistema; I-52 regenerara geometria para el espejo y usara `RestampEnvelope` solo para identidad, y ya declara que debe conservar las propiedades de rack; I-53 toca editores y DTO de sistema y califica el cruce con I-54 como bajo | §2.2, §2.4, §2.5 |
| P-15 | Existe un patron CRUD reutilizable (`RACKVARIABLES`): leer → proyectar → ventana → intent por id → preflight → ejecutor → releer | §11.1 |
| P-16 | El texto de ID24 **no esta versionado** en el arbol; esta Proposal lo toma de la instruccion del Coordinador | §2.4 |

## 2. Objetivos y no-objetivos

**Objetivos**

- O-01. Metadatos definidos por el usuario en dos alcances, Proyecto y Rack, persistidos en el DWG.
- O-02. Identidad estable por entrada, independiente del nombre.
- O-03. Mismos metadatos en todas las vistas de un rack, sin divergencia silenciosa.
- O-04. Supervivencia a guardar/reabrir, `RACKEDITAR` de los seis sistemas, vistas nuevas, propagacion de
  variables, `RACKDUPLICAR` y `RACKLAYOUT`.
- O-05. Fallo cerrado ante contenido ilegible o futuro, **sin degradar nada del rack**.
- O-06. Puntos de extension declarados hacia dibujo, expresiones y plantillas, sin implementarlos.
- O-07. Una UI minima y reutilizable para los dos alcances.

**No-objetivos**

- N-01. Tipos (numero, booleano), unidades, formatos, listas de valores.
- N-02. Herencia o valor por defecto entre Proyecto y Rack.
- N-03. Propiedades por vista, por referencia o por instalacion.
- N-04. Dibujar propiedades, columnas en BOM o `RACKLISTA`, campos de AutoCAD.
- N-05. Expresiones, parser, formulas, referencias entre racks (ID20, ID21, ID22B).
- N-06. Plantillas de propiedades.
- N-07. Exportar o importar propiedades por la biblioteca.
- N-08. Transferir o fusionar propiedades de proyecto entre dibujos.
- N-09. Editar propiedades desde los editores de sistema.
- N-10. Cambiar `RACKCAD_PROJECT`, `ProjectVariablesDocument`, `RackEnvelopeRestamp`, `RackCloner` o los flujos
  `Edit*` de los seis sistemas.

## 3. Alternativas de representacion

### 3.1 Las tres opciones

| | **A** — `Dictionary<string,string>` | **B** — registros con id estable | **C** — documentos de propiedad tipados |
|---|---|---|---|
| Forma persistida | `{ "Cliente": "ACME", ... }` | `[ { "Id": guid, "Name": "Cliente", "Value": "ACME" } ]` | `[ { "Id": guid, "Name": "Altura", "Type": "Number", "Value": 12.5 } ]` |
| Identidad | el nombre | GUID por entrada | GUID por entrada |
| Renombrar | borrar + crear: cambia la clave | cambia `Name`; id intacto | id intacto |
| Referencia futura (dibujo, expresion, plantilla) | por nombre: se rompe al renombrar | por id | por id |
| Nombres repetidos | imposibles en el modelo, pero una clave repetida en el JSON **se colapsa sin diagnostico** [I] | politica explicita | politica explicita |
| Orden | implicito en el objeto JSON, no parte del modelo | lista: orden del modelo | lista |
| Evolucion por entrada | ninguna sin cambiar la forma del valor | `ExtensionData` por entrada | idem, mas un discriminador que hay que hacer cumplir |
| Riesgo en builds ya desplegados | bajo | bajo | **alto**: un valor no-string haria fallar a lectores de texto; un discriminador nuevo obliga a decidir que hace un build viejo |
| Comparacion entre hermanas | trivial | estructural por lista | estructural, con igualdad numerica |
| Coste de UI | minimo | minimo | un editor por tipo, cultura decimal, unidades |
| Precedente en el repo | rechazado en I-47 (A4: `SummaryInfo`, «mapa plano editable… sin estructura») | `VariableId` + `Name` de I-47 | `VariableType` con un solo tipo real, y la deuda que dejo (`ToProjectVariables()` ignoraba el tipo persistido) |

### 3.2 Decision propuesta

**B**, con valor **solo texto** en V1. A queda como **proyeccion** posible para UI o exportacion futura, nunca
como forma persistida. C se rechaza para V1: introduce tipos sin consumidor (ningun objetivo los necesita),
arrastra decisiones de cultura y unidades, y su discriminador seria una promesa sin comprobacion, que es la
deuda que I-48 tuvo que cerrar. La ruta de evolucion hacia tipos queda definida en D-04 sin pagarla ahora.

## 4. Evaluacion de la opcion del Coordinador

La orden pidio evaluar esta opcion **sin aceptarla por instruccion**. Veredicto por componente:

| # | Componente | Veredicto | Motivo |
|---|---|---|---|
| 1 | `CustomPropertyId + Name + string Value` | **ACEPTADO CON PRECISIONES** | Es B (§3). Precisiones: GUID en formato `D`, comparacion por valor de `Guid`, `ExtensionData` por entrada y `SchemaVersion` estricta (D-01, D-02) |
| 2 | Proyecto = documento versionado independiente en el NOD, sin migrar `RACKCAD_PROJECT` | **ACEPTADO** | P-02: migrar rompe a los builds existentes; P-03: mezclarlo con variables acopla lectura y bloqueo de dos autoridades distintas (D-07) |
| 3 | Rack = campo aditivo comun en `RackEmbedDocument`, no en DTOs de producto | **ACEPTADO CON DOS CAMBIOS MATERIALES** | (a) el miembro **no** es un DTO tipado sino un `JsonElement?` que solo interpreta el store de propiedades, para que ningun contenido pueda volver ilegible el sobre (P-05); (b) `Compose` debe **heredarlo** de `source` o se pierde en cada redibujo (P-04). Ninguno de los siete llamadores cambia (D-08) |
| 4 | Misma metadata en todas las vistas hermanas del `RackId` | **ACEPTADO CON AUTORIDAD NUEVA** | Hoy no hay autoridad de sobre (P-06). Hace falta una propia, con divergencia, pertenencia indeterminada y unificacion explicita; y escritura a todas las hermanas en **una** transaccion (D-09) |
| 5 | Renombrar conserva el id | **ACEPTADO** | D-10 |
| 6 | Duplicar copia los valores en colecciones independientes | **ACEPTADO CON PRECISION** | La copia conserva **ids y valores**; la independencia la da que cada rack tiene su propio sobre. Funciona sin tocar `RackEnvelopeRestamp` (P-08), pero exige prueba contractual (D-11) |
| 7 | Ni ProjectVariables, ni parser, ni formulas | **ACEPTADO** | Guardas vigentes intactas; D-16 fija el punto de extension sin sintaxis |

## 5. Decisiones

Cada decision indica **Propuesta**, **Por que**, **Descartado** y **Revisar** (lo que el Arquitecto debe
atacar). Las marcadas **MATERIAL** cambian formato persistido, autoridad o superficie.

### D-01 — Modelo persistido (MATERIAL)

**Propuesta.** Una sola forma para los dos alcances:

```json
{
  "SchemaVersion": "1.0",
  "Entries": [
    { "Id": "0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10", "Name": "Cliente", "Value": "ACME" },
    { "Id": "5d1c2a90-7b44-4f0e-8a3b-0c9e6d2f1a77", "Name": "Notas", "Value": "Linea 1\nLinea 2" }
  ]
}
```

- `SchemaVersion` **sin inicializador** (C4.8-1 de I-47): ausente ≠ `"1.0"`.
- `Entries` **obligatorio** (lista, puede estar vacia).
- `[JsonExtensionData]` en la raiz y en cada entrada.
- Nombres de miembro en PascalCase, como el resto de persistencia.

**Por que.** Un solo tipo, un solo store, una sola validacion y una sola proyeccion de UI para los dos
alcances. El documento nace versionado, como `ProjectVariablesDocument`.

**Descartado.** Formas distintas por alcance (duplican store y reglas); definiciones de proyecto con valores
por rack (§5, D-03).

**Revisar.** `Entries` ausente o nulo como **ilegible**, mas estricto que `Variables` en I-47 (que tolera
ausencia). La razon: un documento nuevo no tiene legado que proteger y «ausente» no se distingue de
«truncado».

### D-02 — Identidad (MATERIAL)

**Propuesta.**

- `CustomPropertyId` = GUID; se escribe en formato `D` y se lee **solo** en formato `D`; `Guid.Empty` es
  ilegible.
- Igualdad por **valor de `Guid`**, no por texto: comparando texto, el mismo GUID escrito en dos formatos
  pareceria dos identidades. La lectura estricta en `D` ya lo impide al leer; la igualdad por valor lo
  impide tambien en memoria.
- Unicidad **dentro de una coleccion**. Un id repetido → `AmbiguousIdentity`, fail-closed (precedente V7-R01
  de I-48).
- El id lo acuña Application al crear; nunca se deriva del nombre.
- El mismo id puede aparecer en el proyecto y en muchos racks: la identidad es **local al alcance**.

**Descartado.** Identidad por nombre (A); ids por rack regenerados en cada copia (rompe la correlacion futura
por plantilla, D-17).

**Revisar.** `AmbiguousIdentity` como resultado del **store** y no como capa de acreditacion separada (I-48
los separo porque el store de variables ya existia; aqui el store es nuevo).

### D-03 — Alcances

**Propuesta.** Conjunto cerrado `{ Proyecto, Rack }`. Sin herencia ni fallback entre ellos; sin alcance de
vista, referencia ni instalacion.

**Por que.** Metadata por instancia de vista ya fue rechazada en I-50 (CD-08, ADR-0035) [D §2.2]; la
`BlockReference` solo coloca (ADR-0010) [D §5.6]; el alcance de instalacion es terreno de plantillas (D-17).

**Descartado.** Modelo de **definiciones de proyecto + valores por rack**: da nombres consistentes entre
racks, pero un rack copiado a otro dibujo llevaria valores cuyas definiciones no existen alli (P-10), la
misma clase de referencia rota que en I-47 exigio `RepairBroken`. Con entradas autodescriptivas esa clase de
fallo **no existe**.

**Revisar.** Si la consistencia de nombres entre racks debe resolverse ya en V1 o puede esperar a plantillas.

### D-04 — Solo texto y regla de evolucion (MATERIAL)

**Propuesta.**

- `Value` es **siempre** un string JSON; vacio permitido; nulo o ausente → ilegible.
- No hay campo de tipo en V1.
- **Regla de evolucion**: toda extension que cambie el significado o la validez de `Value` para un lector
  existente (tipos, unidades, listas) **sube el major** del documento; un build anterior la vera como
  `IncompatibleMajor`, bloqueara la edicion y **preservara los bytes**. Solo son minor los campos
  informativos aditivos.

**Por que.** Ningun objetivo necesita tipos (O-01..O-07). La regla usa la doctrina que el repositorio ya
aplica: un major que el build no entiende se rechaza, no se interpreta.

**Descartado.** Campo `Type` con un unico valor (promesa sin comprobacion hoy); valores numericos JSON (un
lector de texto fallaria).

**Revisar.** Si la regla «semantica ⇒ major» basta para que un build viejo nunca escriba texto en una
propiedad que un build nuevo considere numerica.

### D-05 — Nombres y limites

**Propuesta.**

- Al escribir: nombre recortado, 1..80 caracteres, sin caracteres de control (U+0000–U+001F, U+007F),
  **unico** en la coleccion con `OrdinalIgnoreCase`.
- Al leer: nombre vacio → ilegible; nombres repetidos → legible con **diagnostico** (no son identidad).
- Valor: 0..1000 caracteres; admite salto de linea (CR, LF) y tabulador, y ningun otro caracter de control;
  se guarda tal cual lo entrega el intent.
- Coleccion: como maximo 50 entradas.
- Los limites se aplican **solo al escribir**: leer nunca bloquea por tamaño, para que un build posterior
  pueda ampliarlos sin major.

**Por que.** Cada coleccion de rack se replica en todas sus hermanas (P-07); los limites acotan el tamaño del
payload por definicion.

**Revisar.** Los numeros (decision del Owner, OQ-02) y si los nombres repetidos al leer deben bloquear.

### D-06 — Orden

**Propuesta.** El orden persistido es el orden de presentacion; crear **añade al final**; V1 no reordena. El
orden participa en la comparacion entre hermanas.

**Revisar.** Si reordenar debe entrar en V1 (OQ-06).

### D-07 — Persistencia de Proyecto (MATERIAL)

**Propuesta.**

```text
Database.NamedObjectsDictionaryId
 └─ "RACKCAD_CUSTOM_PROPERTIES" ─► Xrecord ─► DxfCode.Text en trozos de 255
```

- Lectura fisica **tri-estado** que nunca lanza (`Absent | Present | PresentButUnreadable`), con el mismo
  patron que `ProjectVariablesData` [D §4.2], en un tipo nuevo del Plugin.
- Store en Application con resultados `Absent | Readable | PresentButUnreadable | IncompatibleMajor |
  AmbiguousIdentity`; `CanWrite` solo con `Absent` o `Readable`.
- **Version primero**: se lee la raiz como JSON, se exige objeto, se valida `SchemaVersion` estricta
  (`major.minor`) y solo con major compatible se enlazan las entradas; asi un major futuro con otra forma da
  `IncompatibleMajor` y no un error de deserializacion.
- Escritura en **una** transaccion: releer → acreditar esa lectura → aplicar el intent sobre el documento
  acreditado → guarda → escribir → commit (patron `RegistryCommit` de I-47/I-48). Sin regen.
- `RACKCAD_PROJECT`, `ProjectVariablesData` y `ProjectVariablesDocument` **no se tocan**; el troceado se
  reimplementa en el tipo nuevo (la extraccion del ayudante duplicado queda fuera: hallazgo F-13 del
  Discovery).

**Por que.** P-02 (forma fisica), P-03 (dos autoridades, dos bloqueos), ADR-0034 §1 y §13.

**Descartado.**

| Opcion | Motivo |
|---|---|
| Dentro de `ProjectVariablesDocument` | acopla lectura, acreditacion y bloqueo de variables y propiedades; ADR-0034 §1; I-49 extiende ese documento |
| Sub-diccionario bajo `RACKCAD_PROJECT` | P-02 |
| Diccionario `RACKCAD` que agrupe claves | incoherente con el Xrecord directo existente; sin necesidad actual; seria su propio ADR |
| `Database.SummaryInfo` (DWGPROPS) como autoridad | sin id, editable fuera de RackCad sin guarda, sin version ni campos desconocidos (precedente A4 de I-47). Queda como **destino de proyeccion** futura (D-15) |

**Revisar.** El nombre de la clave (es un contrato de persistencia congelado desde la primera escritura) y
reimplementar el troceado frente a extraer el ayudante ahora.

### D-08 — Persistencia de Rack (MATERIAL)

**Propuesta.**

- Miembro nuevo del sobre: `CustomProperties` de tipo **`JsonElement?`**, que contiene el documento de D-01
  como **objeto** JSON, con `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`.
- **Solo el store de propiedades lo interpreta**: nulo → `Absent`; valor que no es objeto →
  `PresentButUnreadable`; objeto → mismas reglas que Proyecto.
- `RackEmbedDocument.CurrentSchemaVersion` sigue en `"1.0"`: **sin promocion de major**.
- `RackEmbedComposer.Compose` añade **una** asignacion: `CustomProperties = source?.CustomProperties`. La
  firma no cambia y **ninguno de los siete llamadores cambia** [D §5.3].
- Funcion pura nueva para escribir: `WithCustomProperties(source, customProperties)` devuelve un sobre igual
  a `source` en todos sus miembros —version resuelta sin degradar, `ExtensionData` incluido— salvo
  `CustomProperties`.

**Por que.**

1. **Aislamiento.** `JsonElement?` acepta cualquier valor JSON sin lanzar, asi que ningun contenido de
   propiedades —malformado, de un build futuro o con otra forma— puede hacer que `RackEmbedStore.Deserialize`
   devuelva `null` (P-05). El rack sigue editable, listable, cotizable y duplicable pase lo que pase con sus
   propiedades. [I sobre System.Text.Json; lo fija T-15]
2. **Bytes intactos.** Con `WhenWritingNull`, un sobre sin propiedades se serializa igual que hoy (P-11).
3. **Cero cambios en flujos calientes.** La herencia desde `source` cubre redibujo, vista nueva y propagacion
   de variables [D §5.4]; `RestampEnvelope` ya conserva cualquier miembro (P-08).
4. **Builds anteriores.** Un build con I-11 no conoce el miembro, lo guarda en `ExtensionData` y lo reemite
   en redibujo, vista nueva y restamp [D §5.4]: **preserva sin interpretar**.
5. **Por que no subir el major.** I-47 rechazo F3 porque un build viejo abriria un rack vinculado por su
   literal y **redibujaria geometria incoherente** con la autoridad. Las propiedades personalizadas no tienen
   efecto geometrico ni de BOM: un build viejo no puede malinterpretarlas porque no las interpreta. Subir el
   major haria que **todo** build anterior rechace **cualquier** rack con propiedades, un coste
   desproporcionado para datos inertes.

**Descartado.**

| Opcion | Motivo |
|---|---|
| Miembro tipado (`CustomPropertiesDocument` como propiedad) | un contenido inesperado lanza al deserializar el sobre → rack entero ilegible (R-02 del Discovery) |
| Miembro `string` con el JSON dentro (precedente `Design`) | protege contra contenido malformado, pero no contra un cambio de tipo JSON del miembro; `JsonElement?` protege contra ambos y evita doble escape |
| Solo en `ExtensionData`, sin declarar | abusa de «campos que este build no conoce» en el build que si los conoce |
| DTO de diseño por sistema | seis formatos, dos sin `ExtensionData`, cruce con I-50 e I-53, acoplamiento con `SelectiveAuthoredAuthority` y con la biblioteca de solo algunos sistemas [D R-08] |
| Xrecord propio en el diccionario de extension del BTR | `RackCloner` no lo copia: se pierde en toda copia independiente [D §8.3] |
| Mapa `RackId → propiedades` en el NOD | no viaja entre dibujos, `RACKDUPLICAR` tendria que escribir el NOD (I-51 INV-10 lo excluye), y borrar un rack deja huerfanos |
| `BlockReference` (XData, atributos) | la referencia solo coloca (ADR-0010); `COPY` divergiria de la identidad |

**Revisar.** `JsonElement?` frente a `string`; la ausencia de promocion de major; y el riesgo de builds
anteriores a I-11 (R-12, OQ-05).

### D-09 — Autoridad entre vistas hermanas (MATERIAL)

**Propuesta.**

1. **Pertenencia.** Las hermanas de un rack son todas las definiciones cuyo sobre tiene ese `RackId`
   (`OrdinalIgnoreCase`), obtenidas de un barrido que **tambien** ve los sobres ilegibles y cuenta referencias.
2. **Pertenencia indeterminada.** Si alguna definicion **colocada** del dibujo tiene payload RackCad con sobre
   ilegible, la pertenencia es desconocida: las **escrituras** de alcance Rack se bloquean nombrando el bloque;
   la lectura se muestra con aviso. Las no colocadas se ignoran (mismo criterio que la ventana de
   `RACKVARIABLES` y `RACKBOMTOTAL` [D §7.4]).
3. **Kind.** Hermanas con `Kind` distinto → bloqueado.
4. **Sin identidad.** Un rack con `RackId` en blanco no tiene alcance Rack: se informa, no se inventa id.
5. **Resolucion.** Se lee la coleccion de cada hermana con el store:
   - alguna `IncompatibleMajor` → **bloqueado**, sin reparacion;
   - alguna `PresentButUnreadable` o `AmbiguousIdentity` → bloqueado, con la unificacion del punto 7 disponible;
   - todas legibles o ausentes → comparacion **estructural, incluir por defecto**: `SchemaVersion`, entradas en
     orden, `Id` por valor, `Name` y `Value` ordinales, `ExtensionData` de raiz y de entrada;
   - **ausente ≡ vacio**: una coleccion ausente iguala a una legible con cero entradas, sin `ExtensionData` y
     con la version actual; cualquier otra diferencia es divergencia;
   - resultados: `Single(coleccion)`, `Divergent(vistas)`, `Unreadable(vista)`, `MixedKind`, `NoIdentity`,
     `IndeterminateMembership(bloques)`.
6. **Escritura.** Siempre a **todas** las hermanas en **una** transaccion: un documento calculado una vez,
   `WithCustomProperties` sobre el sobre **propio** de cada hermana y `RackBlockData.Write`. Sin redefinir
   geometria, sin importar bloques, sin purga y **sin regen**. Dentro de la transaccion se vuelve a barrer y a
   resolver; si ya no es `Single`, o el id del intent no existe, se aborta sin escribir.
7. **Unificacion explicita.** Con `Divergent`, o con hermanas `PresentButUnreadable`/`AmbiguousIdentity` y al
   menos una legible, la ventana ofrece «Unificar desde la vista seleccionada»: el **usuario** elige la vista y
   confirma; se escribe esa coleccion en todas las hermanas en una transaccion. **El sistema nunca elige.**
   Precedente: guardar desde la vista picada es lo que reconcilia el Selectivo hoy (`RackSelectivoCommands.cs:358-360`).
8. **Descarte explicito.** Si **ninguna** hermana es legible ni ausente —todas `PresentButUnreadable` o
   `AmbiguousIdentity`—, la unica salida es «Descartar las propiedades ilegibles», confirmada, que escribe una
   coleccion vacia en todas. Nunca si alguna es `IncompatibleMajor`. En alcance Proyecto rige lo mismo para
   su unico contenedor.
9. **Flujos existentes.** `RACKEDITAR` conserva la coleccion **propia** de cada hermana y una vista nueva hereda
   la del sobre picado (D-08); no se añade ninguna comprobacion a esos flujos.

**Por que.** P-06 (no hay autoridad de sobre), P-07 (transacciones por vista), doctrina
`UNKNOWN ≠ ABSENT` (ADR-0034 §8) y la leccion de I-48: un estado sin salida deja el dato permanentemente
ineditable.

**Descartado.** Reutilizar `SelectiveAuthoredAuthority` (solo ve el diseño interior y solo Selectivo);
elegir la primera o la frontal; escribir solo las hermanas legibles; comparar bytes.

**Revisar.** La regla de pertenencia indeterminada (bloquear escrituras de todos los racks por un sobre
colocado ilegible ajeno); `ausente ≡ vacio`; si la unificacion y el descarte entran en V1 o se difieren; y
que la escritura sin redibujo no necesite regen.

### D-10 — Crear, renombrar, cambiar valor, eliminar, vaciar

**Propuesta.**

| Operacion | Efecto | Confirmacion |
|---|---|---|
| Crear | id nuevo, añade al final, valida D-05 | no |
| Renombrar | cambia solo `Name`; id y valor intactos; cambio solo de mayusculas permitido | no |
| Cambiar valor | cambia solo `Value` | no |
| Eliminar | quita la entrada por id | no (UNDO de AutoCAD disponible) |
| Vaciar | eliminar la ultima deja `Entries: []`; **nunca** se borra el miembro del sobre ni la entrada del NOD | — |
| Unificar | D-09.7 | casilla |
| Descartar ilegible | D-09.8 | casilla |

**Por que.** Vaciar sin borrar conserva la version sin degradar y los campos desconocidos que un build
posterior hubiera escrito; `ausente ≡ vacio` evita que eso cree divergencia.

**Revisar.** Eliminar sin confirmacion.

### D-11 — Duplicacion e independencia

**Propuesta.**

- `RACKDUPLICAR` y `RACKLAYOUT` (independiente) copian `CustomProperties` **tal cual** a traves de
  `RestampEnvelope`: mismos ids, mismos valores, y contenido ilegible sigue ilegible. **Sin cambio de codigo.**
- La independencia la da el contenedor: el sobre de la copia es otro; escribir propiedades en un rack nunca
  toca otro `RackId`.
- `COPY` de AutoCAD y `RACKRELLENAR` comparten la definicion: **es el mismo rack**.
- Sin politica de copia por propiedad en V1 (OQ-04).
- **Coordinacion con I-52 (no dependencia).** Su Discovery descarta `RackCloner` para el espejo, regenerara la
  geometria y usara `RestampEnvelope` solo para identidad [D §2.5]. Cualquier camino de escritura del espejo
  conserva la coleccion si el sobre se obtiene por `RestampEnvelope` (mismo objeto) o por `Compose` con el sobre de
  origen como `source`; la **pierde** si compone con `source = null`. I-52 ya registro esa obligacion.
- **Prueba contractual** en cuatro capas, porque ninguna suite carga el Plugin:
  1. Core: `Compose` hereda; `WithCustomProperties` preserva; round-trip del sobre con el miembro.
  2. Core, **caracterizacion etiquetada como tal**: mutar `Id`/`Name`/`Design` sobre el mismo objeto
     deserializado conserva el miembro.
  3. Guarda de fuente sobre `RackEnvelopeRestamp.cs`: deserializa y serializa **el mismo** objeto; ni
     `new RackEmbedDocument` ni `RackEmbedComposer.Compose` en ese archivo.
  4. Validacion del Owner en AutoCAD (OV-05).

**Descartado.** Regenerar ids en la copia: rompe la correlacion futura entre racks (D-17) y no aporta
independencia, que ya da el contenedor; la regla es la misma que ADR-0034 §12 fija para las referencias a
`VariableId` —cambia la identidad del rack, no la del dato—. Extraer ahora la mitad-sobre del restamp a
Application: tocaria `RackEnvelopeRestamp.cs` mientras I-52 lo caracteriza.

**Revisar.** Si las capas 2 y 3 bastan o la extraccion debe hacerse igualmente.

### D-12 — Biblioteca y exportaciones

**Propuesta.** V1 **no** escribe propiedades en `.rackcad.json` ni las lee al importar; BOM, CSV, XLSX y
`RACKLISTA` no cambian. Se documenta en la guia.

**Por que.** P-09: ningun sistema exporta campos del sobre, y el propio exportador del Selectivo razona que lo
que pertenece al dibujo no pertenece al artefacto. Hacerlo exige decidir que propiedades son de **diseño** y
cuales de **instancia** (cliente, area y ubicacion son de instancia), lo que es producto (OQ-01).

**Punto de extension.** Un slot aditivo en `RackProjectDocument`, que ya tiene `ExtensionData`; la cabecera
desnuda seria el caso aparte.

### D-13 — Malformado y schema futuro

**Propuesta.**

| Estado del contenedor | Proyecto | Rack | Efecto en el resto |
|---|---|---|---|
| `Absent` | coleccion vacia, editable | vacia, editable | ninguno |
| `Readable` | editable | editable si la autoridad es `Single` | ninguno |
| `Readable` con nombres repetidos | editable, con aviso | idem | ninguno |
| `PresentButUnreadable` | bloqueado; sin «crea la primera»; descarte explicito | bloqueado; unificar o descartar (D-09) | **ninguno** |
| `AmbiguousIdentity` | como ilegible | como ilegible | **ninguno** |
| `IncompatibleMajor` | bloqueado; **nunca** se sobrescribe ni descarta | bloqueado; sin unificar ni descartar | **ninguno** |

«Ninguno» significa: dibujo, `RACKEDITAR`, BOM, `RACKLISTA`, `RACKDUPLICAR`, `RACKLAYOUT` y `RACKVARIABLES`
se comportan igual que con un contenedor ausente (INV-07).

### D-14 — Entre dibujos

**Propuesta.** Las propiedades de rack **viajan** con la definicion por los mecanismos nativos de AutoCAD; las
de proyecto **no**. V1 no transfiere, fusiona ni sincroniza. Como las entradas son autodescriptivas, un rack
pegado en otro dibujo no queda con referencias rotas. [I; lo observa OV-09]

### D-15 — Punto de extension: dibujo (declarado, no implementado)

**Propuesta.** Un consumidor futuro (texto o atributo en `RACKCAD_ANOTACIONES`, espejo en DWGPROPS para
campos de AutoCAD, tabla-resumen, columnas de BOM o lista) lee **solo** una proyeccion pura desde la
autoridad —`Single` para Rack; `Absent`/`Readable` para Proyecto—, **nunca** del sobre crudo. Con `Divergent`
o ilegible no dibuja un valor supuesto. Es de **un solo sentido**: el dibujo nunca pasa a ser autoridad. V1
no crea codigo de dibujo.

### D-16 — Punto de extension: expresiones (declarado, no implementado)

**Propuesta.**

- La unica clave de referencia posible es `CustomPropertyId`; el nombre es presentacion.
- **No se asume sintaxis**: I-49 reserva `Rack`/`Project` y `Rack.X`/`Project.X` para ID20 (P-13). Si ID24 y
  ID20 comparten espacio de nombres es decision del Owner (OQ-07).
- Los valores son texto y el motor de I-49 solo opera `double`: V1 no define semantica de operando; un tipo
  numerico futuro pasa por la regla de major de D-04.
- Sin parser, sin tokens y sin dependencia en ningun sentido con I-49; las guardas de fórmulas quedan intactas.

### D-17 — Punto de extension: plantillas (declarado, no implementado)

**Propuesta.** Una plantilla futura seria una lista ordenada `{ Name, valor por defecto, id de entrada }`.
Aplicarla crearia entradas con **los ids de la plantilla**, de modo que racks creados desde la misma plantilla
correlacionen por id. Donde vive (instalacion o proyecto) se decide entonces. V1 lo habilita sin codigo: el
store acepta cualquier GUID valido y `ExtensionData` por entrada admite un origen como campo minor.

### D-18 — UI minima reutilizable (MATERIAL por superficie)

**Propuesta.**

- **Comando** `RACKPROPIEDADES` con alias propuesto `RPR` (libre hoy [D §11.5]); pide «Selecciona un rack o
  [Proyecto]».
- **Bucle** del patron `RACKVARIABLES` (P-15): leer en una transaccion → proyectar → ventana modal → **un**
  intent por id → preflight puro → ejecutor → releer.
- **Workspace puro** en Application, **agnostico al alcance**: estado `Editable | SoloLectura | Bloqueado |
  Divergente`, filas `{ Id, Name, Value, NombreRepetido }`, diagnosticos y etiqueta del alcance.
- **Ventana** nueva `RackCustomPropertiesWindow`, arquetipo **C**, que adopta `DialogWindowChrome`,
  `EditorActions` y las reglas D6/D7/D9 de ADR-0029: lista `Nombre — Valor`, detalle Nombre/Valor, botones
  Nueva / Renombrar / Cambiar valor / Eliminar, banner de bloqueo con motivo, panel de unificacion (vistas +
  casilla) y panel de descarte (casilla), estado y Cerrar. Sin `MessageBox` (costuras como las de I-47).
- La ventana **no** ve `Database`, `ObjectId`, NOD ni sobres.
- **Censos**: comandos 33 → 35; ventanas 29 → 30 (C: 11 → 12); ayuda en `RackCommandReference`.
- **Fuera de V1**: seccion en los editores de sistema, boton en el menu principal, columnas en `RACKLISTA`,
  reordenar, importar/exportar.

**Descartado.** Integrarlo en `RACKVARIABLES` (dos autoridades, ADR-0034 §1); en los editores de sistema
(archivos calientes, I-50 G3, I-53).

**Revisar.** Un comando con palabra clave frente a dos comandos; unificacion y descarte en la misma ventana.

### D-19 — Nombres de codigo y guardas

**Propuesta.** Nombres orientativos (no contractuales salvo lo que fijen las guardas): `CustomPropertiesDocument`,
`CustomPropertyEntryDocument`, `CustomPropertyId`, `CustomPropertiesStore`, `CustomPropertiesReadResult`,
`CustomPropertiesWriteGuard`, `RackCustomPropertiesAuthority`, `CustomPropertiesWorkspace`,
`CustomPropertiesIntent`, `CustomPropertiesPreflight`; en el Plugin `CustomPropertiesData` y
`RackPropiedadesCommands`.

- **Prohibido** usar los substrings de P-12 en identificadores (`CustomPropertyValues` o `rackPropertyId`
  romperian la suite Core).
- **Guardas nuevas**: Domain sin propiedades personalizadas; ejecutor de propiedades sin `Regen(`,
  `RedefineSystemBlock`, `EnsureForPlan` ni `PurgeUnreferenced`, con un solo `Commit`; `RackEnvelopeRestamp`
  sobre el mismo objeto (D-11); censos actualizados. Toda guarda nueva se demuestra en rojo con una violacion
  temporal.
- **Criterio de aceptacion por comportamiento**, no por texto: las guardas son defensa secundaria (leccion
  del BLOCKER de I-48).

### D-20 — ADR

**Propuesta.** ADR **obligatorio** antes de producir codigo (§11).

## 6. Invariantes propuestos

| # | Invariante |
|---|---|
| INV-01 | Una coleccion de Proyecto por DWG; una coleccion de Rack por `RackId`, identica en cada hermana |
| INV-02 | La identidad de una entrada es su `CustomPropertyId`; el nombre nunca es identidad |
| INV-03 | Renombrar no cambia id ni valor |
| INV-04 | Escribir propiedades no toca geometria, diseño, `Kind`, `Id`, `Name`, `View`, `Section` ni `ExtensionData` del sobre |
| INV-05 | Una escritura de Rack alcanza a todas las hermanas en una transaccion, o a ninguna |
| INV-06 | Nada se escribe sobre `IncompatibleMajor`; sobre ilegible solo el descarte o la unificacion confirmados |
| INV-07 | **Aislamiento**: el estado de las propiedades no cambia la legibilidad del sobre ni el resultado de `RACKEDITAR`, BOM, `RACKLISTA`, `RACKDUPLICAR`, `RACKLAYOUT` ni `RACKVARIABLES` |
| INV-08 | Un sobre sin propiedades se serializa byte a byte igual que en `BASE_SHA` |
| INV-09 | Redibujo conserva la coleccion de cada hermana; vista nueva hereda la del sobre picado; rack nuevo nace sin coleccion |
| INV-10 | Copia independiente conserva ids y valores; copia enlazada es el mismo rack |
| INV-11 | Ante divergencia o ilegibilidad, el sistema nunca elige hermana |
| INV-12 | Pertenencia indeterminada bloquea las escrituras de Rack |
| INV-13 | Domain no conoce propiedades; UI no conoce AutoCAD; Application no conoce `ObjectId` |
| INV-14 | Ninguna propiedad se lee, escribe ni resuelve como variable, expresion o formula |
| INV-15 | La biblioteca no exporta ni importa propiedades en V1 |
| INV-16 | `RACKCAD_PROJECT`, `ProjectVariablesDocument`, `RackEnvelopeRestamp`, `RackCloner` y los `Edit*` de sistema no cambian |

## 7. Flujos

```text
LEER PROYECTO     tx { NOD → lectura tri-estado → store } → workspace
ESCRIBIR PROYECTO intent → preflight(snapshot) → tx { releer → acreditar → aplicar → guarda → escribir } → commit → releer
LEER RACK         pick → tx { barrido con referencias → proyeccion → pertenencia → autoridad } → workspace
ESCRIBIR RACK     intent → preflight → tx { re-barrer → autoridad Single → aplicar → por hermana: WithCustomProperties + RackBlockData.Write } → commit → releer   (sin regen)
UNIFICAR          intent(vista, confirmado) → tx { re-barrer → vista legible → escribir su coleccion en todas } → commit
RACKEDITAR        sin cambios de codigo → Compose hereda la coleccion propia de cada hermana
VISTA NUEVA       sin cambios → Compose hereda la del sobre picado
RACKDUPLICAR      sin cambios → RestampEnvelope conserva el miembro
BIBLIOTECA        sin cambios → ni exporta ni importa
```

## 8. Compatibilidad

| Build que abre el dibujo | Rack con propiedades: dibujar / editar sistema | Redibujo y duplicado | Editor de propiedades |
|---|---|---|---|
| Anterior a I-11 (sin `ExtensionData` en el sobre) | normal | **pierde las propiedades** al reescribir el sobre [I] | no existe |
| I-11 … antes de I-54 | normal | **preserva** sin interpretar [I, D §5.4] | no existe |
| I-54 | normal | preserva | segun D-13 |
| Futuro, mismo major (1.x) | normal | preserva; minor sin degradar | editable; campos nuevos preservados |
| Futuro, major 2 | normal | preserva los bytes | `IncompatibleMajor`: bloqueado, nunca sobrescrito |

## 9. Riesgos y mitigaciones

| Riesgo del Discovery | Mitigacion en esta Proposal |
|---|---|
| R-01 campo perdido en redibujo | D-08: herencia en `Compose` + T-16/T-17 |
| R-02 sobre ilegible por contenido | D-08: `JsonElement?` + T-15 |
| R-03 divergencia silenciosa | D-09: autoridad + escritura unica + unificacion |
| R-04 pertenencia indeterminada | D-09.2 |
| R-05 migrar `RACKCAD_PROJECT` | D-07: no se migra |
| R-06 acoplar con variables | D-07: contenedor y store propios |
| R-07 fuera del sobre | D-08: dentro del sobre |
| R-08 DTO de sistema | D-08: descartado |
| R-09 restamp sin prueba / I-52 | D-11: cuatro capas; aviso a I-52 |
| R-10 nombres que rompen guardas | D-19 |
| R-11 sintaxis `Rack.X` | D-16: sin sintaxis; OQ-07 |
| R-12 builds anteriores a I-11 | OQ-05; si existen, reconsiderar promocion |
| R-13 nulos en todos los sobres | D-08: `WhenWritingNull` + T-13 |
| R-14 conflictos de censos y docs | Declarados en el contrato; se resuelven al integrar |
| R-15 expectativa entre dibujos | D-14 documentado; OV-09 |

**Riesgos nuevos de la Proposal**

| # | Riesgo | Mitigacion |
|---|---|---|
| RP-01 | `JsonElement?` con `null` literal: que `System.Text.Json` lo convierta en `null` y no en elemento `Null` | T-15 lo fija; si no, el store trata `ValueKind.Null` como ausente |
| RP-02 | Una definicion colocada ilegible ajena bloquea escrituras de Rack en todo el dibujo | Aviso con nombre de bloque; revisable (RD-11) |
| RP-03 | Dos transacciones de `RACKEDITAR` fallan a medias y dejan la coleccion vieja en una hermana | La coleccion **no cambia** en `RACKEDITAR`: cada hermana conserva la suya; no se introduce divergencia nueva |
| RP-04 | I-52 cambia `RackEnvelopeRestamp`, o escribe el sobre del espejo con `Compose(null, …)`, y la coleccion se pierde | Guarda D-11.3; I-52 ya declara que el espejo debe conservarla [D §2.5]; comprobar su camino de escritura en su G2 |
| RP-05 | Censos de comandos y ventanas cambian tambien en I-52/I-53 | Conflicto textual trivial; quien integre despues suma |

## 10. Mapa previsto de archivos (orientativo, no autorizado)

| Capa | Nuevos | Modificados |
|---|---|---|
| Domain | — | — |
| Application | ~10: documento, store, resultado, guarda, id, autoridad, entrada de barrido, intent, preflight, workspace | `Persistence/RackEmbedDocument.cs` (+1 miembro), `Persistence/RackEmbedComposer.cs` (+1 asignacion, +1 funcion) |
| Plugin | `CustomPropertiesData.cs`, `RackPropiedadesCommands.cs` (comando y ejecutores) | **ninguno** |
| UI | `RackCustomPropertiesWindow.xaml(.cs)` | `RackCommandReference.cs` |
| Tests | Core: documento/store, sobre, autoridad, workspace/preflight, guardas; UI: ventana | `WindowCensusGuardTests.cs`, `SelectiveEditorOpenTests.cs` |
| Docs | ADR, guia de uso, `README`, `despliegue.md`, `validacion-manual-autocad.md`, `ideas-futuras.md` | contrato; HANDOFF y ROADMAP solo al integrar |

Cruces: ninguno de estos archivos de produccion lo toca hoy I-49, I-50, I-52 ni I-53 [D §13]. Los censos y la
ayuda son conflicto textual previsible con I-52.

## 11. ADR

**Recomendacion: SI, antes de implementar.** Cumple tres criterios de [adr/README.md](../adr/README.md):
restringe trabajo futuro en varias capas y en formatos de persistencia (clave nueva del NOD, miembro nuevo del
sobre, documento nuevo); es caro de revertir (datos en DWG de clientes); y cierra un debate recurrente
(donde vive un metadato de rack: sobre, diseño, NOD o referencia).

- **Numero**: no se reserva. ADR-0035 es de I-50 en su rama; I-49 tiene uno pendiente sin numero. Se asigna al
  redactarlo.
- **Contenido minimo**: D-01, D-02, D-04, D-07, D-08, D-09, D-11, D-12, D-13 y los limites de D-15..D-17.
- **No reemplaza** ADR-0009, ADR-0010 ni ADR-0034: los aplica. Debe citar la frase de ADR-0035 sobre
  `RackEmbedComposer` y precisar que vale para campos declarados.
- **Momento**: nace `propuesto` en el Consensus Freeze y el Owner lo acepta antes de G3 (precedente CF-2 de I-47).

## 12. Gates propuestos (no autorizados)

| Gate | Contenido | Evidencia |
|---|---|---|
| G2B | Revision de Arquitecto de esta V1 y reconciliacion (V2… si hace falta) | veredictos por SHA |
| G2-FREEZE | Consenso Coordinator + Architect sobre el mismo SHA; contrato actualizado; ADR `propuesto`; F-01..F-13 a `ideas-futuras.md`; aprobacion del Owner | commit documental + CI |
| G3 | Documento, store, resultados, guarda y mutaciones puras | Core: T-01..T-12 en rojo → verde |
| G4 | Miembro del sobre, herencia en `Compose`, `WithCustomProperties`, aislamiento, caracterizacion de restamp y guarda | Core: T-13..T-22 |
| G5 | Autoridad de Rack, proyeccion del barrido, preflight y workspace | Core: T-23..T-36 |
| G6 | Plugin: datos del NOD, ejecutores (Proyecto, Rack, unificar, descartar) y guardas de fuente; build del Plugin | Core: T-37..T-44 |
| G7 | Ventana, comando, censos y ayuda | UI: U-01..U-10 |
| G8 | Candidato: Core y UI completas en local, CI 4/4 sobre el SHA exacto, Debug de UI y Plugin | AGENTS.md |
| G9 | Validacion del Owner en AutoCAD 2025 | OV-01..OV-12 |
| G10 | Integracion (WORKFLOW 4.5) | CI del merge + cobertura |

### 12.1 Pruebas contractuales propuestas

| # | Afirma |
|---|---|
| T-01 | Sin texto → `Absent`, coleccion vacia, `CanWrite` |
| T-02 | Round-trip conserva orden, ids, nombres y valores (unicode, saltos de linea) |
| T-03 | `SchemaVersion` ausente, en blanco o no parseable → `PresentButUnreadable` |
| T-04 | Major 2 con `Value` numerico → `IncompatibleMajor` (version primero), sin excepcion |
| T-05 | Minor `1.7` → `Readable`; reescribir conserva `1.7` |
| T-06 | Campos desconocidos de raiz y entrada sobreviven a reescribir |
| T-07 | JSON invalido, raiz no objeto, `Entries` ausente o nulo → `PresentButUnreadable` |
| T-08 | Entrada nula, id no `D`, `Guid.Empty`, nombre vacio, valor nulo → `PresentButUnreadable` |
| T-09 | Mismo GUID en dos entradas (distinta capitalizacion) → `AmbiguousIdentity` |
| T-10 | Nombres repetidos al leer → `Readable` con diagnostico |
| T-11 | Guarda: escribir solo tras `Absent`/`Readable`; descartar solo tras `PresentButUnreadable` o `AmbiguousIdentity`, nunca tras `IncompatibleMajor` |
| T-12 | Serializar estampa la version actual si falta y nunca degrada |
| T-13 | Sobre sin propiedades: bytes identicos a los de `BASE_SHA` para el mismo contenido |
| T-14 | Sobre con propiedades: round-trip exacto |
| T-15 | El sobre deserializa con `CustomProperties` objeto, string, numero, array, booleano y `null`; el store clasifica cada caso |
| T-16 | `Compose` hereda de `source`; con `source` nulo no hay coleccion |
| T-17 | Dos vistas conservan cada una su coleccion al componer |
| T-18 | `WithCustomProperties` cambia solo ese miembro; conserva `ExtensionData` y version sin degradar |
| T-19 | Caracterizacion de build anterior: un DTO local con la forma de `BASE_SHA` guarda el miembro en `ExtensionData` y lo reemite |
| T-20 | Caracterizacion de restamp: mutar `Id`/`Name`/`Design` sobre el mismo objeto conserva el miembro |
| T-21 | Guarda: `RackEnvelopeRestamp.cs` serializa el mismo objeto que deserializa |
| T-22 | La exportacion a biblioteca no lee ni escribe el miembro |
| T-23..T-30 | Autoridad: `Single`, vacia, ausente ≡ vacia, divergencia por valor, orden, id, `ExtensionData` y version; hermana ilegible nunca omitida; `MixedKind`; `NoIdentity`; pertenencia indeterminada (colocada bloquea, no colocada se ignora) |
| T-31..T-35 | Preflight: nombre recortado, vacio, repetido, limites, control; renombrar a solo mayusculas; id inexistente; unificar exige confirmacion, vista legible y ningun major futuro; descartar exige confirmacion y todas ilegibles |
| T-36 | Workspace: estados y filas por alcance |
| T-37..T-44 | Guardas de fuente: una transaccion y un commit por ejecutor; sin regen ni redefinicion; escritura solo en hermanas de la autoridad; clave del NOD; sin substrings prohibidos; censos 35/30; Domain sin propiedades |
| U-01..U-10 | Ventana: filas; intent por id; banner deshabilita el editor; unificar y descartar exigen casilla; sin `MessageBox`; cierre y Escape segun ADR-0029 |

Toda seleccion filtrada debe demostrar que selecciono al menos una prueba (AGENTS.md).

### 12.2 Validacion del Owner propuesta

| # | Escenario |
|---|---|
| OV-01 | Proyecto: crear, renombrar, cambiar, eliminar; guardar, cerrar y reabrir |
| OV-02 | Selectivo con frontal, laterales y planta: crear propiedades; todas las vistas muestran lo mismo; reabrir |
| OV-03 | `RACKEDITAR` → Actualizar en los seis sistemas conserva las propiedades |
| OV-04 | Insertar vista nueva desde `RACKEDITAR` hereda las propiedades |
| OV-05 | `RACKDUPLICAR` (varios origenes) y `RACKLAYOUT` independiente llevan las propiedades; editar la copia no cambia el original |
| OV-06 | `COPY` nativo comparte las propiedades |
| OV-07 | `RACKVARIABLES` con un Selectivo vinculado: cambiar valor conserva las propiedades |
| OV-08 | UNDO tras una escritura de propiedades |
| OV-09 | Copiar un rack a otro DWG: lleva las del rack, no las del proyecto |
| OV-10 | `PURGE` y `AUDIT` tras escribir: la entrada del NOD sigue |
| OV-11 | Guardar en biblioteca y abrir: sin propiedades, como se documenta |
| OV-12 | `RACKLISTA` y `RACKBOMTOTAL` sin cambios |

Divergencia, ilegibilidad y major futuro **no** se fabrican a mano para el Owner (regla de I-47 y V5-R07 de
I-48): los cubren T-23..T-35.

## 13. Preguntas al Owner

| # | Pregunta |
|---|---|
| OQ-01 | ¿Alguna propiedad de rack debe viajar con la biblioteca (p. ej. de diseño) o ninguna en V1? |
| OQ-02 | Limites: 50 entradas, nombre 80, valor 1000 con saltos de linea. ¿Sirven? |
| OQ-03 | Nombre y alias del comando (`RACKPROPIEDADES` / `RPR`) y si hace falta boton en el menu principal |
| OQ-04 | Al duplicar, ¿se copian todos los valores (V1) o alguno debe limpiarse? |
| OQ-05 | ¿Hay instalaciones con builds anteriores a I-11 que editen los mismos DWG? |
| OQ-06 | ¿Reordenar entradas debe entrar en V1? |
| OQ-07 | ¿ID24 y ID20 deben compartir espacio de nombres en expresiones futuras? |
| OQ-08 | ¿«Custom Properties Foundation» (ID24) es exactamente metadatos de usuario de Proyecto y Rack, como describe el contrato? (el texto de ID24 no esta versionado) |
| OQ-09 | ¿Editar propiedades desde los editores de sistema es necesario en V1? |

## 14. Decisiones que el Arquitecto debe revisar

La lista exacta, con pregunta y evidencia, vive en el
[paquete de revision](I-54-architect-review-package.md) §5: **RD-01..RD-20**.

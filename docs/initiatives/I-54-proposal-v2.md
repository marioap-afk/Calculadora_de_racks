# I-54 — Proposal V2: Custom Properties Foundation (ID24)

```text
Documento      = PROPOSAL V2 — NOT CONSENSUS
Gate           = G2C — reconciliacion de la Architect Review de V1
Executor       = PROPOSED V2
Coordinator    = NOT YET AGREED ON V2
Architect      = NOT YET REVIEWED ON V2
Owner          = NOT ASKED
Consensus      = NOT REACHED
Implementation = BLOCKED
ADR            = REQUERIDO; alcance candidato en §15; sin numero y sin texto
BASE_SHA       = 46fcac2b071929d2bd5b07aa28373941417f74a8
G1_SHA         = 195964b00006d2be74eefb8b6ae4f5f46de1cfc9   (Discovery)
PROPOSAL_V1    = 7c197af81b91df88366c873eddf9e11ddc5e87bb   (historica; no se edita)
REVIEW_V1      = Architect Review — I-54 Proposal V1 @ 7c197af: AGREED WITH CHANGES
                 0 BLOCKER · 5 MATERIAL (AR-54-01..05) · 13 MINOR (AR-54-06..18) · cambios V2-1..V2-22
PROPOSAL_V2    = el commit que introduce este archivo (como obtenerlo: paquete §1)
Discovery      = I-54-discovery.md; addendum G2C §2.6 en el mismo commit (re-medicion de paralelas)
Paquete        = I-54-architect-review-package.md, reescrito para revisar ESTA version
```

> Esta V2 **no autoriza** nada y **sustituye a V1** como propuesta vigente. V1 queda en el arbol como historia
> y no se modifica. Cada decision cita el cambio vinculante que incorpora (**V2-n**) y el hallazgo que cierra
> (**AR-54-nn**). El consenso exige `Coordinator = AGREED` y `Architect = AGREED` sobre el **mismo** SHA de
> este documento, el ADR en estado `propuesto` y la aprobacion del Owner.

**Marcas de evidencia.** **[E]** leido en esta sesion sobre `BASE_SHA`, o sobre el SHA de rama que se cite.
**[L]** lectura de auditoria con cita, no re-verificada linea a linea. **[I]** inferencia no ejecutada.
**[X]** ejecutado en una sonda local de `System.Text.Json` sobre .NET 8.0.29, **fuera del repositorio**, con
copias literales de `RackEmbedDocument.cs`, `RackEmbedComposer.cs` y `SchemaVersionPolicy.cs` de `BASE_SHA`,
del sobre anterior a I-11 (`4b2f4d7^`) y de la variante con el miembro propuesto. Los casos y resultados estan
en el paquete §5, para reproducirlos. `[D §n]` remite al Discovery.

## 0. Que cambia respecto de V1

| # | Tema | V1 | V2 | Cambio | Cierra |
|---|---|---|---|---|---|
| 1 | Recuperacion destructiva | «Descartar las propiedades ilegibles» en Proyecto y Rack | **No existe.** Ilegible, ambigua o de major futuro = solo lectura con bytes intactos | V2-1 | AR-54-01 |
| 2 | Unificar | Con divergencia, o sobre hermanas ilegibles o ambiguas | Solo divergencia entre hermanas **todas** legibles o ausentes, sin sobrescribir contenido desconocido | V2-2 | AR-54-01 |
| 3 | Pertenencia indeterminada | Solo definiciones **colocadas** | **Cualquier** definicion con payload no interpretable, colocada o no | V2-3 | AR-54-02 |
| 4 | Ausente ≡ vacio | «con la version actual» | Con **cualquier** minor; igualdad canonica | V2-4 | AR-54-03 |
| 5 | Aislamiento de `JsonElement?` | «Ningun contenido puede volver ilegible el sobre» | Afirmacion acotada y restricciones A..G | V2-5 | AR-54-04 |
| 6 | Preservacion del sobre | Implicita en `Compose` | Invariante normativo y tres guardas con rojo | V2-6 | AR-54-05 |
| 7 | Prueba del duplicado | «Cuatro capas» que parecian probar el restamp | Propiedad del store de produccion, guarda estructural y OV | V2-7 | AR-54-09 |
| 8 | Sin promocion de major | Por la inercia de los datos | Por la matriz verificada de builds | V2-8 | AR-54-06 |
| 9 | Evolucion | «Cambio semantico ⇒ major» | Regla normativa, confrontada con ADR-0034 §3 | V2-9 | AR-54-07 |
| 10 | Id | «Formato D estricto» | Forma textual exacta de 36 caracteres, sin espacios, escrita en minusculas | V2-10 | AR-54-08 |
| 11 | Nombres | Recorte y `OrdinalIgnoreCase` | NFC antes de recortar, validar y comparar; el nombre solo presenta | V2-11 | AR-54-08 |
| 12 | Agregacion entre racks | Implicita | Ninguna clave de agregacion | V2-12 | AR-54-15 |
| 13 | Proyecto | Contenedor propio | Mas independencia en ambos sentidos y clave congelada | V2-13 | AR-54-16 |
| 14 | Ejecutor de Rack | «Re-barrer y resolver» | Una transaccion con `ScanEnvelopes`; Application decide sobre la lectura fresca; xref rechazadas | V2-14 | AR-54-10 |
| 15 | Mutaciones | Sin precisar `ExtensionData` de entrada | Conservan `ExtensionData` de entrada; el workspace nunca presenta valores de una hermana | V2-15 | AR-54-13 |
| 16 | `Kind` vacio | Sin definir | `MixedKind` (opcion A) | V2-16 | AR-54-14 |
| 17 | Puntos de extension | Plantillas «con los ids de la plantilla» | Solo restricciones | V2-17 | AR-54-11 |
| 18 | Biblioteca | Presentada como «propiedades de instancia» | Frontera de V1 | V2-18 | AR-54-18 |
| 19 | Alcance Proyecto | «Una coleccion por DWG» | Definido frente a Drawing-level | V2-19 | AR-54-12 |
| 20 | UI | Panel de descarte; nombres de comando | Sin descarte; unificar seguro; nombres sin congelar | V2-20 | AR-54-01, -10, -13 |
| 21 | Gates | G4 sin caracterizacion previa; G6 sin disparador fisico | Caracterizacion al abrir G4; guardas en G4; lo fisico solo en G9 | V2-21 | AR-54-17 |
| 22 | Preguntas al Owner | Nueve preguntas de decision | Reclasificadas; ninguna bloquea el consenso | V2-22 | AR-54-06, -12 |

## 1. Vocabulario

| Termino | Significado en V2 |
|---|---|
| **Propiedad personalizada** | Entrada `{ Id, Name, Value }` definida por el usuario. No es variable de proyecto, geometria ni BOM |
| **Coleccion** | Documento de propiedades: `SchemaVersion` y la lista **ordenada** `Entries` |
| **Alcance Proyecto** | Una coleccion por `Database` de AutoCAD (un archivo DWG), con metadatos **administrativos o comerciales** del proyecto que ese archivo representa. No es metadato de dibujo u hoja, rotulacion, layout ni vista, ni autoridad de un proyecto de varios DWG (D-03) |
| **Alcance Rack** | Una coleccion por `RackId`, replicada en el sobre de cada miembro |
| **Miembro de un RackId** | Definicion de bloque **no dependiente de xref** cuyo sobre es legible y cuyo `Id`, no vacio, es igual al `RackId` sin distinguir mayusculas (ADR-0009 `:42-45`) |
| **Payload RackCad no interpretable** | Definicion que `RackBlockFinder.ScanEnvelopes` devuelve con `Embed = null`: el Xrecord `RACKCAD_SELECTIVE` trae texto, pero `RackEmbedStore.Deserialize` no lo lee (JSON invalido, truncado, anidado excesivo o major futuro) |
| **Sobre** | `RackEmbedDocument`, en el diccionario de extension de la definicion [D §5.2] |
| **Contenedor** | Entrada `RACKCAD_CUSTOM_PROPERTIES` del NOD (Proyecto) o miembro `CustomProperties` del sobre (Rack) |
| **Forma canonica** | Documento parseado, comparado segun D-09.7 |
| **Estado no escribible** | `PresentButUnreadable`, `AmbiguousIdentity` o `IncompatibleMajor` |
| **Profundidad** | Niveles de contenedores JSON: el objeto raiz cuenta 1 y cada objeto o array anidado suma 1 |

## 2. Estado verificado en G2C

```text
Fecha               = 2026-09-12, tras git fetch --all --prune
Rama I-54           = architecture/propiedades-personalizadas; HEAD = upstream = 7c197af; 0/0; arbol limpio
Stash / operaciones = ninguno; sin MERGE_HEAD, REBASE_HEAD, CHERRY_PICK_HEAD, REVERT_HEAD, BISECT_LOG ni rebase-*
origin/main         = 46fcac2 (no avanzo desde BASE_SHA: sin rebase)
I-49                = ccf21c6  (avanzo desde 4df9480: Proposal V4, solo docs)
I-50                = 6cd2970  (avanzo desde 8ceb3a7: G3 (A), (B) y (C), ventanas del Selectivo, Dinamico y Push Back)
I-52                = 0fc7032  (avanzo desde 339b3ab: Proposal V1, ADR-0036 propuesto y registro de decisiones; solo docs)
I-53                = f38362d  (avanzo desde c8476cc: Proposal V1, solo docs)
```

Ningun avance invalida una decision de I-54 (§13; Discovery §2.6). Consecuencias:
- la guarda T-GRD-01 se limita a `src/`, porque una prueba de I-50 construye sobres en `tests/`;
- el espejo de I-52 cumple el invariante D-21, pero añade una llamada a `Compose` que T-GRD-02 tendra que
  clasificar;
- aparecen dos conflictos documentales: el censo de comandos (I-52 lo lleva a 34) y la numeracion de ADR
  (I-52 usa 0036).

## 3. Premisas verificadas

| # | Premisa | Fuente |
|---|---|---|
| P-01 | No existe metadato descriptivo de proyecto ni mapa clave→valor editable; el unico metadato de rack es `Name` | [D §3] |
| P-02 | `RACKCAD_PROJECT` es un Xrecord directo del NOD. Una entrada que no sea Xrecord se lee como `PresentButUnreadable` y bloquea tres comandos | `ProjectVariablesData.cs:36,59-68` [E]; [D §4.6] |
| P-03 | `ProjectVariablesDocument` es la unica autoridad de variables, con versionado propio | [D §4.3]; ADR-0034 §1, §13 |
| P-04 | `Compose` crea un sobre nuevo y del origen solo copia `SchemaVersion` y `ExtensionData` | `RackEmbedComposer.cs:21-33` [E] |
| P-05 | `RackEmbedStore.Deserialize` devuelve `null` ante `JsonException` o major futuro: un miembro **tipado** con contenido inesperado dejaria el rack entero ilegible | `RackEmbedDocument.cs:86-119` [E]; P-17 |
| P-06 | Ninguna autoridad compara campos del sobre entre hermanas; `SelectiveAuthoredAuthority` solo ve el diseño interior | [D §6] |
| P-07 | `RACKEDITAR` compone cada hermana con su propio sobre, en una transaccion por vista; el ejecutor de variables usa una sola | [D §7.2] |
| P-08 | `RestampEnvelope` deserializa, muta `Id`, `Name` y `Design` y serializa el **mismo** objeto. `RackCloner` crea un BTR nuevo y escribe solo el payload. Ninguna suite ejecuta ese codigo | `RackEnvelopeRestamp.cs:52-88`; `RackCloner.cs:34-36,66` [E] |
| P-09 | Ningun sistema exporta campos del sobre a `.rackcad.json`; importar acuña GUID nuevo y compone con `source = null` | [D §9] |
| P-10 | El NOD no viaja con un rack copiado a otro dibujo; el sobre si | [D §4.8, §9.4] |
| P-11 | `RackEmbedStore` escribe los nulos: no fija `DefaultIgnoreCondition` | `RackEmbedDocument.cs:70-74` [E]; [X] |
| P-12 | Hay guardas ordinales que prohiben `PropertyValues` (Domain, Plugin, UI) y `rackProperty` y `RackPropertyReference` (Application, Plugin, UI) | `ProjectVariablesConformanceTests.cs:114,129-135,162` [E] |
| P-13 | I-49 reserva para ID20 los nombres `Rack` y `Project`, sus namespaces y el ambito `Rack` | `I-49-proposal-v4.md:628-633,1737-1745` @ `ccf21c6` [E] |
| P-14 | Ningun archivo productivo del mapa de I-54 lo tocan I-49, I-50, I-52 ni I-53. I-52 reutilizara `RestampEnvelope` despues de reflejar y debe conservar las propiedades de rack | §13 |
| P-15 | Existe un patron CRUD reutilizable, el de `RACKVARIABLES`: leer → proyectar → ventana → intent por id → preflight → ejecutor → releer | `RackVariablesCommands.cs:47-82,101-125` [E] |
| P-16 | El texto de ID24 no esta versionado; el alcance lo fijan el contrato y el objetivo del Coordinador | [D §2.4] |
| P-17 | Comportamiento de `JsonElement?` dentro del sobre [X]. (1) Acepta objeto, string, numero, array y booleano, y reemite el texto exacto. (2) Un `null` literal da `HasValue = false` y el miembro se elimina al reescribir. (3) Sin el miembro, los bytes son identicos a BASE, con y sin `ExtensionData` y tambien con `Compose(null)`. (4) Una corrupcion sintactica o un truncado dentro del miembro, o 64 niveles de anidado dentro de el, dejan el sobre en `null` en BASE y en la variante; 63 niveles se leen. (5) `default(JsonElement)` hace lanzar `InvalidOperationException` a `Serialize`. (6) Un elemento de tipo `Null` asignado escribe `"CustomProperties":null`. (7) Con la clave del miembro repetida gana la ultima. (8) Con el miembro y una clave igual en `ExtensionData` se emiten dos claves, y al releer gana la de `ExtensionData`. (9) Una clave con otra capitalizacion se asigna al miembro. (10) Un miembro `string` que recibe un objeto lanza `JsonException` | sonda |
| P-18 | [X] Un build anterior a I-11 lee un sobre major 2 como legible y **pierde** el miembro al reescribir. I-11..I-53 conservan el miembro exacto, con `Compose(source)` y con el restamp del mismo objeto, y tratan un major 2 como ilegible | sonda; `git show 4b2f4d7^:src/RackCad.Application/Persistence/RackEmbedDocument.cs` [E] |
| P-19 | [X] `Guid.TryParseExact(texto, "D")` acepta espacios o tabulador alrededor y acepta `Guid.Empty`; rechaza llaves y el formato `N`. `Guid.ToString("D")` escribe 36 caracteres en minusculas. «Área» en NFC y en NFD no son iguales con `OrdinalIgnoreCase` hasta normalizar a `FormC` | sonda |
| P-20 | [X] `JsonElement` conserva los nombres de miembro repetidos: `EnumerateObject` los devuelve todos y `TryGetProperty` devuelve el ultimo | sonda |
| P-21 | En Project Variables, **toda** definicion con datos RackCad no interpretables aborta la operacion, colocada o no, porque «sin su identidad» no se puede demostrar que no sea hermana. Su proyeccion trata un `Id` o un `Kind` en blanco como sobre ilegible y filtra por Selectivo | `ProjectVariableConsumerDiscovery.cs:87-91,161-216`; `ProjectVariableScanProjection.cs:37-46` [E] |
| P-22 | `ScanEnvelopes` corre en la transaccion del llamador; omite layouts, anonimos y `IsFromExternalReference`; conserva los ilegibles con `Embed = null` y cuenta las referencias desde el registro. `FindRackBlocks` abre su propio bloqueo y su propia transaccion, y descarta los ilegibles | `RackBlockFinder.cs:57-91`; `RackCommandSupport.cs:109-134` [E] |
| P-23 | `Kind` existe en el sobre desde que este nacio (`74d935f`, 2026-07-08), y los siete llamadores de `Compose` pasan un kind constante. Antes, el Selectivo escribia bajo la misma clave `RACKCAD_SELECTIVE` el `SelectivePalletDesignDocument` desnudo (`aad0c5e`..`74d935f^`: `SchemaVersion`, `Id` y `Name`, sin `Kind` ni `Design`). El store de BASE lo lee como sobre **legible** con `Kind` y `Design` nulos [X] | `git show 74d935f`; `74d935f^:src/RackCad.Application/Persistence/SelectivePalletDesignDocument.cs:16-22` [E]; sonda |
| P-24 | `Kind` en blanco hoy. Lo tolera `FindRackBlocks`, que busca por GUID. Lo rechazan la proyeccion de variables, `RackDuplicationPlan` («no declara tipo de rack»), los preflights de Push Back y Cantilever, `RACKLISTA`, el inventario y `RACKEDITAR` sobre el propio bloque | `RackCommandSupport.cs:120-127`; `ProjectVariableScanProjection.cs:37-42`; `RackDuplicationPlan.cs:412-415`; `RackPushBackCommands.cs:194-203`; `RackCantileverCommands.cs:243-252`; `RackListBuilder.cs:56`; `RackInventarioCommands.cs:53`; `RackMenuCommands.cs:132-146` [E] |
| P-25 | En `src/`, el unico `new RackEmbedDocument` esta en `RackEmbedComposer.cs:24`. En `tests/` construyen sobres ocho archivos de `BASE_SHA` y el `DimensionViewsRestampTests.cs` de I-50 | `git grep` [E] |
| P-26 | El ejecutor de variables relee y re-acredita en Application dentro de su transaccion unica, sin replanificar. Una guarda fija que compone con el sobre de cada hermana | `ProjectVariableMutationExecutor.cs:250-288`; `ProjectVariableMutationExecutorTests.cs:288-298` [E] |
| P-27 | G-R5 fija la forma del restamp con los ayudantes `PluginSourceCode`: una sola asignacion `.Id =`, un solo `NewGuid(`, ningun `catch (` y `TryGetIgnoreCase(` | `SelectiveDuplicationFailClosedTests.cs:474-526` [E] |

## 4. Objetivos y no-objetivos

**Objetivos**

- O-01. Metadatos definidos por el usuario en dos alcances, Proyecto y Rack, persistidos en el DWG.
- O-02. Identidad estable por entrada, independiente del nombre.
- O-03. Mismos metadatos en todos los miembros de un rack, sin divergencia silenciosa y sin elegir hermana.
- O-04. Supervivencia a guardar y reabrir, a `RACKEDITAR` de los seis sistemas, a vistas nuevas, a la
  propagacion de variables, a `RACKDUPLICAR` y a `RACKLAYOUT`.
- O-05. Fallo cerrado ante contenido ilegible, ambiguo o futuro: se **conserva** el dato y se **bloquea** la
  escritura. El estado de las propiedades no cambia el resto del rack **dentro del alcance verificado de D-08**.
- O-06. Una identidad estable que un consumidor futuro pueda referenciar, sin fijar nada de ese consumidor.
- O-07. Una UI minima y reutilizable para los dos alcances.

**No-objetivos**

- N-01. Tipos (numero, booleano), unidades, formatos y listas de valores.
- N-02. Herencia o valor por defecto entre Proyecto y Rack.
- N-03. Propiedades por vista, por referencia o por instalacion.
- N-04. Dibujar propiedades, columnas en BOM o en `RACKLISTA`, y campos de AutoCAD.
- N-05. Expresiones, parser, formulas y referencias entre racks (ID20, ID21, ID22B).
- N-06. Plantillas de propiedades.
- N-07. Exportar o importar propiedades por la biblioteca.
- N-08. Transferir o fusionar propiedades de proyecto entre dibujos.
- N-09. Editar propiedades desde los editores de sistema.
- N-10. Cambiar el formato de `RACKCAD_PROJECT`, `ProjectVariablesData`, `ProjectVariablesDocument`,
  `RackBlockData`, `RackBlockFinder`, `RackEnvelopeRestamp`, `RackCloner` o los flujos `Edit*`.
- N-11. **Recuperacion destructiva** de contenedores no escribibles: descartar, reparar o sobrescribir. Queda
  fuera de I-54 y exige una decision explicita del Owner en otra iniciativa (V2-1).
- N-12. Claves de agregacion entre racks, por nombre o por id (V2-12).
- N-13. Metadatos de dibujo, hoja, rotulacion, layout o de proyectos de varios DWG (V2-19).
- N-14. Fijar semantica de plantillas, sintaxis de expresiones o formato de rotulacion (V2-17).

## 5. Representacion

| | **A** — `Dictionary<string,string>` | **B** — registros con id estable | **C** — documentos tipados |
|---|---|---|---|
| Identidad | el nombre | GUID por entrada | GUID por entrada |
| Renombrar | borrar y crear: cambia la clave | cambia `Name`, el id se conserva | el id se conserva |
| Referencia futura | por nombre: se rompe al renombrar | por id | por id |
| Evolucion por entrada | ninguna sin cambiar la forma | `ExtensionData` por entrada | idem, mas un discriminador que hay que hacer cumplir |
| Riesgo en builds desplegados | bajo | bajo | alto: un valor no-string haria fallar a los lectores de texto |
| Precedente | rechazado en I-47 (A4) | `VariableId` + `Name` de I-47 | `VariableType` con un solo tipo real, y la deuda de `ToProjectVariables()` |

**Decision: B, con valor solo texto en V1.** A queda como proyeccion posible, nunca como forma persistida. C
se rechaza para V1 porque introduce tipos sin consumidor. La confrontacion con ADR-0034 §3 esta en D-04.

## 6. Decisiones

Formato: **Decision** · **Cambio desde V1** · **Por que y evidencia** · **Descartado**. Las **MATERIAL** cambian
formato persistido, autoridad o superficie.

### D-01 — Modelo persistido (MATERIAL)

**Decision.** Una sola forma para los dos alcances:

```json
{
  "SchemaVersion": "1.0",
  "Entries": [
    { "Id": "0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10", "Name": "Cliente", "Value": "ACME" },
    { "Id": "5d1c2a90-7b44-4f0e-8a3b-0c9e6d2f1a77", "Name": "Notas", "Value": "Linea 1\nLinea 2" }
  ]
}
```

1. `SchemaVersion` **sin inicializador** en el DTO (C4.8-1 de I-47): ausente no equivale a `"1.0"`. Al leer
   se exige texto con la forma exacta `major.minor` (dos enteros no negativos separados por un punto, sin
   espacios) y `major ≥ 1`.
2. `Entries` **obligatorio**: un array, que puede estar vacio.
3. `[JsonExtensionData]` en la raiz y en cada entrada. Nunca contiene un nombre igual, con `OrdinalIgnoreCase`,
   a un miembro declarado de ese objeto (T-STO-13). La misma regla rige para el sobre (D-08.4, restriccion F).
4. **Todo escritor 1.x emite siempre `SchemaVersion` y `Entries`**, tambien con cero entradas (V2-9).
5. Los nombres de miembro JSON se escriben en PascalCase y se leen sin distinguir mayusculas. Un mismo
   nombre de miembro repetido en un objeto (la raiz o una entrada), sin distinguir mayusculas, deja el
   documento en `PresentButUnreadable`: nunca «gana el ultimo» en silencio [X P-20]. No confundir con dos
   **entradas** que comparten `Name`, que se leen con diagnostico (D-05.2).

**Cambio desde V1.** Puntos 4 y 5, y la forma exacta de `SchemaVersion`.

**Por que.** Un solo tipo, un solo store, una sola validacion y una sola proyeccion de UI. Con el punto 4, un
build posterior 1.x nunca escribe un documento que I-54 lea como ilegible por falta de miembros obligatorios.

### D-02 — Identidad (MATERIAL · V2-10)

**Decision.**

1. **Forma persistida exacta**: 36 caracteres `8-4-4-4-12` de digitos hexadecimales, con guion en las
   posiciones 8, 13, 18 y 23. Sin espacios alrededor ni dentro, y sin llaves ni parentesis. Al leer se acepta
   el hexadecimal en cualquier caja.
2. Primero se comprueba la forma textual y **despues** se obtiene el valor con `Guid.TryParseExact(texto, "D")`.
   `TryParseExact` solo no basta: acepta espacios y tabuladores alrededor [X P-19].
3. Se escribe con `Guid.ToString("D")`: 36 caracteres en minusculas [X P-19], tambien al reescribir una entrada
   que se leyo en mayusculas. La identidad es el valor, asi que eso no cambia la igualdad canonica.
4. `Guid.Empty` → `PresentButUnreadable`.
5. **Igualdad por valor de `Guid`**, nunca por texto.
6. Dos entradas de la misma coleccion con el mismo valor de `Guid` → **`AmbiguousIdentity`**, un resultado
   **distinto** de `PresentButUnreadable`. Se evalua solo si el documento es valido en todo lo demas (D-07.4).
7. Application acuña el id al crear (`Guid.NewGuid()`); nunca se deriva del nombre.
8. La identidad es **local al alcance**: el mismo GUID puede aparecer en el proyecto y en muchos racks (las
   copias lo conservan, D-11), y **eso no significa nada en V1** (D-03.4).

**Cambio desde V1.** Puntos 1 a 3 y 6 (resultado distinto), y el 8 sin la «correlacion futura por plantilla».

### D-03 — Alcances (V2-19 · V2-12)

**Decision.**

1. Conjunto cerrado `{ Proyecto, Rack }`, sin herencia ni fallback entre ellos. Sin alcance de vista,
   referencia ni instalacion.
2. **Alcance Proyecto** = una coleccion por `Database` de AutoCAD (un archivo DWG), con metadatos
   administrativos o comerciales del proyecto que ese archivo representa (cliente, obra, ubicacion, revision…).
   **No significa**:
   - anotaciones de nivel dibujo;
   - metadatos de hoja, rotulacion o cuadro de titulo;
   - layouts;
   - metadatos de vista;
   - autoridad de un proyecto que abarque varios DWG.

   Un **Drawing-level** futuro sigue fuera de I-54, y nada de V2 lo presupone.
3. **Alcance Rack** = una coleccion por `RackId`, identica canonicamente en cada miembro (D-09).
4. **Sin agregacion entre racks.** V1 no define ninguna clave de agregacion. Ningun consumidor futuro puede
   inferir que «mismo `Name` = misma propiedad conceptual», ni lo mismo por un id repetido entre racks, sin un
   contrato posterior que lo decida. Ninguna API de V1 agrupa ni une colecciones de racks distintos.

**Cambio desde V1.** Los puntos 2 y 4 son nuevos; se retira la pregunta de D-03 sobre consistencia de nombres.

**Descartado.** Definiciones de proyecto con valores por rack: un rack copiado a otro dibujo llevaria valores
cuyas definiciones no existen alli (P-10), la clase de referencia rota que en I-47 obligo a `RepairBroken`.

### D-04 — Solo texto y regla de evolucion normativa (MATERIAL · V2-9)

**Decision.**

1. `Value` es **siempre** un string JSON, y puede estar vacio. Nulo, ausente o de otro tipo JSON →
   `PresentButUnreadable`.
2. No hay campo de tipo en V1.
3. **Regla de evolucion, contractual para el formato:**

   | Cambio | Version |
   |---|---|
   | Cambiar el significado de `Value` | **MAJOR** |
   | Cambiar la validez de `Value` en lectura | **MAJOR** |
   | Cambiar el tipo JSON de `Value` | **MAJOR** |
   | Cambiar la estructura de una entrada (miembros obligatorios, su forma o su significado) | **MAJOR** |
   | Cambiar la estructura de la coleccion (raiz, `Entries`, orden, identidad) | **MAJOR** |
   | Añadir un campo **informativo** que un lector de minor menor puede ignorar y conservar sin dejar de ser correcto | MINOR |
   | Ajustar los limites de **escritura** de D-05 (no son reglas de lectura) | ninguna |

4. Todo escritor 1.x emite siempre `SchemaVersion` y `Entries` (D-01.4).
5. **Nada que un build posterior pueda interpretar validamente se considera destructible.** V1 no tiene
   recuperacion destructiva (D-13) y la unificacion no sobrescribe contenido desconocido (D-09.10).

**Confrontacion con ADR-0034 §3.** ADR-0034 §3 (`:74-78`) persiste el discriminador de tipo «desde el inicio»
porque su consumidor interpreta el valor numericamente y añadirlo despues «romperia el esquema». En V1 de I-54
no hay ningun consumidor que interprete `Value`. La proteccion que §3 busca —que un build viejo no escriba un
valor invalido en una propiedad que un build nuevo considera tipada— la da el **major del documento**:

- introducir tipos es MAJOR (punto 3);
- un build 1.x ve un documento 2.x como `IncompatibleMajor`, bloquea la escritura y conserva los bytes (D-13);
- el sobre no se promociona (D-08), asi que el rack sigue legible y solo la coleccion queda en solo lectura.

Un `Type` con un unico valor seria una promesa sin comprobacion: exactamente la deuda de `ToProjectVariables()`,
que ignoraba el tipo persistido. La diferencia con §3 se declara, no se oculta: el ADR de I-54 debe citarla.

**Cambio desde V1.** La regla pasa de principio a tabla contractual, con los puntos 4 y 5 y la confrontacion.

### D-05 — Nombres, valores, limites y profundidad (V2-11 · V2-5)

**Decision.**

1. **Nombre al escribir**, en este orden:
   1. normalizar a Unicode **NFC** (`NormalizationForm.FormC`);
   2. recortar;
   3. validar 1..80 caracteres UTF-16, sin caracteres de control (U+0000–U+001F, U+007F);
   4. comprobar unicidad frente a las **otras** entradas, comparando con `OrdinalIgnoreCase` sobre la forma NFC
      de ambos lados.

   Se persiste la forma NFC recortada [X P-19].
2. **Nombre al leer**: debe ser string; vacio tras recortar → `PresentButUnreadable`. Los nombres que no se
   tocan **no** se normalizan ni se reescriben. Los repetidos (NFC + `OrdinalIgnoreCase`) → `Readable` con
   diagnostico.
3. **El nombre solo presenta.** Ninguna API de Application resuelve, busca ni une una propiedad por `Name`. Los
   intents y las referencias usan el id.
4. **Valor al escribir**: 0..1000 caracteres UTF-16; se admiten CR, LF y tabulador y ningun otro caracter de
   control; se guarda tal como llega, sin normalizar. **Al leer** basta con que sea string.
5. **Coleccion**: como maximo 50 entradas **al crear**.
6. **Los limites de escritura validan lo que la operacion introduce**:
   - crear: nombre, valor y conteo resultante;
   - renombrar: el nombre nuevo;
   - cambiar valor: el valor nuevo;
   - eliminar: siempre se permite.

   Las entradas que la operacion no toca **no** se revalidan contra tamaño ni conteo: pueden venir de un build
   con otros limites, y leer nunca bloquea por tamaño.
7. **Cota de profundidad de escritura = 16**. Ningun escritor 1.x emite un documento de propiedades con mas
   de 16 niveles; en el sobre suman 17 de los 64 que `System.Text.Json` acepta.
   - La lectura tolera cualquier profundidad que `System.Text.Json` acepte.
   - Si el documento resultante de una operacion superase la cota por `ExtensionData` conservado, la escritura
     se rechaza (`DepthLimitExceeded`) sin tocar nada.
   - El documento que escribe V1 tiene profundidad 3 [X P-17].

**Cambio desde V1.** NFC (1), el nombre como solo presentacion (3), la validacion por lo introducido (6) y la
cota de profundidad (7).

**Por que.** Cada coleccion de rack se replica en todos sus miembros (P-07). Los limites acotan el payload sin
convertirse en reglas de formato, y la cota de profundidad aleja cualquier contenido de propiedades del limite
que dejaria el sobre ilegible en **todos** los builds [X P-17].

### D-06 — Orden

**Decision.** El orden persistido es el de presentacion: crear añade al final y V1 no reordena. El orden
participa en la igualdad canonica. *Sin cambios desde V1.*

### D-07 — Persistencia de Proyecto (MATERIAL · V2-13)

**Decision.**

```text
Database.NamedObjectsDictionaryId
 └─ "RACKCAD_CUSTOM_PROPERTIES" ─► Xrecord ─► DxfCode.Text en trozos de 255
```

1. **Clave nueva, Xrecord directo, congelada desde la primera escritura.**
2. **Lectura fisica tri-estado que nunca lanza** (`Absent`, `Present` o `PresentButUnreadable`), en un tipo
   nuevo del Plugin y con **la misma semantica** que `ProjectVariablesData` [D §4.2]:
   - una entrada que no es Xrecord → ilegible;
   - un Xrecord sin `Data` o sin texto → ilegible;
   - una excepcion de AutoCAD → ilegible.

   El troceado se **reimplementa**: esa pequeña duplicacion se mantiene y su extraccion queda como deuda F-13,
   no como trabajo de I-54.
3. **Store en Application**, con resultados `Absent`, `Readable`, `PresentButUnreadable`, `IncompatibleMajor` y
   `AmbiguousIdentity`. Solo `Absent` y `Readable` permiten escribir.
4. **Orden de clasificacion, determinista y primero la version**:
   1. texto que no es JSON, raiz que no es objeto, miembros JSON repetidos en la raiz, o `SchemaVersion`
      ausente o sin la forma exacta → `PresentButUnreadable`;
   2. major mayor que 1 → `IncompatibleMajor`, sin enlazar entradas;
   3. `Entries` ausente, nulo o no array; entrada que no es objeto o con miembros JSON repetidos; `Id` sin la
      forma de D-02 o `Guid.Empty`; `Name` o `Value` que no son string; o `Name` vacio →
      `PresentButUnreadable`;
   4. ids repetidos por valor → `AmbiguousIdentity`;
   5. en otro caso → `Readable`, con diagnostico de nombres repetidos.

   **Ningun texto externo produce una excepcion** (V2-5.G).
5. **Escritura en una transaccion**: releer → acreditar esa lectura en Application → aplicar el intent por id
   → validar (D-05) → comprobar la guarda → escribir → confirmar. Es el patron `RegistryCommit` de I-47 y
   I-48 [P-26], sin regen. La escritura **solo** ocurre tras `Absent` o `Readable` **de esa misma transaccion**:
   nunca sobre una entrada que no sea Xrecord.
6. **Independencia en ambos sentidos**:
   - La lectura, la escritura y la UI de propiedades **no** leen `RACKCAD_PROJECT` ni dependen de su estado. Un
     registro de variables corrupto o incompatible no bloquea las propiedades de proyecto.
   - Project Variables **no** lee `RACKCAD_CUSTOM_PROPERTIES`. Una coleccion corrupta o incompatible no cambia
     `RACKVARIABLES`, `RACKEDITAR` ni `RACKBOMTOTAL`.
7. `RACKCAD_PROJECT`, `ProjectVariablesData`, `ProjectVariablesDocument` y `RackBlockData` **no se tocan**.

**Cambio desde V1.** Puntos 1, 4 (orden), 5 («misma transaccion») y 6.

**Descartado.**

| Opcion | Motivo |
|---|---|
| Dentro de `ProjectVariablesDocument` | Acopla la lectura, la acreditacion y el bloqueo de dos autoridades (ADR-0034 §1), e I-49 extiende ese documento |
| Sub-diccionario bajo `RACKCAD_PROJECT` | Todo build existente lo leeria como ilegible (P-02) |
| Diccionario `RACKCAD` que agrupe claves | Incoherente con el Xrecord directo existente; no hace falta hoy |
| `Database.SummaryInfo` como autoridad | Sin id, sin version ni campos desconocidos, y editable fuera de RackCad sin guarda (I-47 A4) |
| Extraer ya el troceado de `ProjectVariablesData` | Tocaria un archivo de I-47 sin necesidad; queda como F-13 |

### D-08 — Persistencia de Rack y contrato de aislamiento (MATERIAL · V2-5 · V2-8)

**Decision.**

1. **Miembro** `CustomProperties` de tipo **`JsonElement?`** con
   `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`. Contiene el documento de D-01 como
   **objeto** JSON.
2. **Solo el store de propiedades lo interpreta**:
   - `null`, ausente o un elemento `Undefined` o `Null` → `Absent`;
   - un valor que no es objeto → `PresentButUnreadable`;
   - un objeto → las mismas reglas que Proyecto (D-07.4).
3. **Compositor.** `RackEmbedComposer.Compose` añade una asignacion, `CustomProperties = source?.CustomProperties`,
   y **ni la firma ni sus siete llamadores cambian**. La funcion pura nueva `WithCustomProperties(source, documento)`
   vive en `RackEmbedComposer` y devuelve un sobre igual a `source` en `Kind`, `Id`, `Name`, `View`, `Section`,
   `Design` y `ExtensionData`, con `SchemaVersion` resuelta sin degradar. Solo cambia `CustomProperties`.
4. **Contrato de aislamiento. Afirmacion permitida:**

   > Dado un sobre JSON sintacticamente valido y dentro del limite de profundidad soportado, el tipo o la forma
   > JSON de `CustomProperties` no vuelve ilegible el sobre.

   Restricciones normativas, cada una con prueba:

   | # | Restriccion | Hecho [X P-17] | Prueba |
   |---|---|---|---|
   | A | Una corrupcion sintactica o un truncado, tambien dentro del miembro, vuelve ilegible el sobre en **todos** los builds. No se afirma ningun aislamiento para ese caso | BASE y variante → `null` | T-ENV-03 |
   | B | 64 niveles de anidado dentro del miembro vuelven ilegible el sobre en todos los builds. **Cota normativa de escritura: 16** (D-05.7) | 63 se leen; 64 → `null` | T-ENV-04, T-STO-09 |
   | C | Nunca se escribe `default(JsonElement)`: `Compose` y `WithCustomProperties` lo normalizan a `null` | `Serialize` lanza | T-ENV-05 |
   | D | Un elemento de tipo `Null` se normaliza a `null` y no se emite | escribe `"CustomProperties":null` | T-ENV-05 |
   | E | Un `null` literal equivale a ausente y se elimina al reescribir | `HasValue = false` | T-ENV-02 |
   | F | `ExtensionData` nunca contiene una clave igual, con `OrdinalIgnoreCase`, a un miembro declarado del sobre. Leyendo, el store no puede producirla. `Compose` y `WithCustomProperties` **rechazan** un origen que la tenga (`InvalidOperationException`, error de programacion) antes de producir nada | dos claves; al releer gana `ExtensionData` | T-ENV-07 |
   | G | El store de propiedades **nunca lanza** por contenido externo: devuelve siempre un resultado tipado | — | T-STO-08, T-STO-20 |

   **Residual declarado.** Si la clave del miembro aparece repetida en el JSON del sobre, `System.Text.Json`
   se queda con la ultima en todos los builds, antes de que el store la vea. Ningun escritor RackCad emite
   claves repetidas (T-ENV-06).
5. **Sin promocion de major del sobre.** `RackEmbedDocument.CurrentSchemaVersion` sigue en `"1.0"` (V2-8).
   Justificacion por la matriz verificada:

   | Build que abre el dibujo | Lee un sobre con el miembro | Al reescribir | Si el sobre fuera major 2 |
   |---|---|---|---|
   | Anterior a I-11 | legible | **pierde** el miembro: no tiene `ExtensionData` [X P-18] | lo lee como legible: **no tiene guarda de version** [X P-18] |
   | I-11..I-53 | legible; el miembro va a `ExtensionData` | **lo conserva exacto**, con `Compose(source)` y con el restamp [X P-18] | **ilegible**: el rack sale de `RACKEDITAR`, `RACKLISTA`, BOM y duplicado |
   | I-54 | lo interpreta | lo conserva (`Compose` hereda) | ilegible |

   **Conclusion.** Promover no protege a los builds anteriores a I-11, que ignoran el major y pierden el
   miembro de todos modos. En cambio, dejaria ilegible **todo** rack con propiedades para I-11..I-53, que hoy
   las conservan. El riesgo residual de los builds anteriores a I-11 es R-12, y OQ-05 deja de ser pregunta.

**Cambio desde V1.** El punto 4 sustituye la afirmacion absoluta de V1. El punto 5 cambia la justificacion,
aunque la decision se mantiene.

**Descartado.**

| Opcion | Motivo |
|---|---|
| Miembro tipado (`CustomPropertiesDocument`) | Un contenido inesperado lanza al deserializar el sobre y deja el rack entero ilegible (P-05) |
| Miembro `string` con el JSON dentro | Un cambio de tipo JSON del miembro lanza `JsonException` [X P-17]; `JsonElement?` lo tolera y evita doble escape |
| Solo en `ExtensionData`, sin declarar | Abusa de «campos que este build no conoce» en el build que si los conoce |
| DTO de diseño por sistema | Seis formatos, dos sin `ExtensionData`, cruce con I-50 e I-53 y acoplamiento con la autoridad del Selectivo [D R-08] |
| Xrecord propio en el BTR | `RackCloner` no lo copia: se pierde en toda copia independiente [D §8.3] |
| Mapa `RackId → propiedades` en el NOD | No viaja entre dibujos y obligaria a `RACKDUPLICAR` a escribir el NOD |
| `BlockReference` (XData, atributos) | La referencia solo coloca (ADR-0010) |
| Promocion del major del sobre | Matriz del punto 5 |

### D-09 — Autoridad por RackId (MATERIAL · V2-3 · V2-4 · V2-16 · V2-2 · V2-1)

**Decision.** Todo lo que sigue es **Application pura** sobre la proyeccion plana de D-22.

1. **Barrido.** Dentro de la transaccion del llamador, `RackBlockFinder.ScanEnvelopes(transaction, database,
   includeReferenceCount: true)` recorre las definiciones con payload RackCad, **tambien** las ilegibles [P-22].
   **Nunca** se usa `FindRackBlocks`, que descarta los ilegibles y abre su propia transaccion.
2. **Pertenencia.** Una definicion es miembro de `R` si y solo si:
   - su sobre es legible;
   - su `Id` no esta vacio y es igual a `R` con `OrdinalIgnoreCase`;
   - no es dependiente de xref (`IsDependent`).

   `IsFromExternalReference` ya lo omite el barrido.
3. **Pertenencia indeterminada (V2-3).** Si **alguna** definicion recorrida, **colocada o no** y no
   dependiente de xref, tiene payload RackCad no interpretable, la pertenencia de **todo** `RackId` es
   indeterminada:
   - **se bloquean** todas las escrituras de alcance Rack del dibujo, incluida la unificacion;
   - **las lecturas** siguen, en modo solo lectura, con un diagnostico por definicion: nombre de bloque, handle
     y si esta colocada o no;
   - **no se bloquean** las propiedades de Proyecto, la geometria, el BOM ni ninguna funcion ajena a las
     propiedades: I-54 no añade comprobaciones a otros comandos;
   - un **sobre legible con `Id` en blanco** no pertenece a ningun `RackId` ni provoca bloqueo global. Si es el
     bloque elegido, da `NoIdentity`;
   - **Precedente**: `ProjectVariableConsumerDiscovery.cs:87-91,161-216`, donde toda definicion no interpretable
     aborta porque «sin su identidad» no se puede demostrar que no sea hermana [P-21];
   - **no se reutiliza** `ProjectVariableScanProjection`: filtra por Selectivo y trata un `Kind` en blanco como
     ilegible, mientras que I-54 no depende del kind y su pertenencia descansa solo en el `Id`.

   **Alternativas rechazadas.**
   - **B**, bloquear solo con evidencia de pertenencia: exige interpretar a medias formatos desconocidos o buscar
     el id en el texto, con falsos negativos.
   - **C**, ignorar los ilegibles: escritura parcial silenciosa si la ilegible era un miembro (contra INV-05).

   **Proporcionalidad.** Solo se bloquean las escrituras de propiedades de Rack. ADR-0034 ya acepto este coste
   en «Riesgos» (`:216-217`).
4. **Dependientes de xref.** Una definicion `IsDependent` no es miembro, no bloquea y nunca se escribe:
   pertenece a otro DWG (D-14). Elegir una referencia cuya definicion sea `IsFromExternalReference` o
   `IsDependent` se rechaza con diagnostico. El trato de AutoCAD a los registros dependientes es [I] y lo
   observa OV-14.
5. **Kind (V2-16, opcion A).** Entre los miembros de `R`, el `Kind` se compara con `OrdinalIgnoreCase`, como en
   `RackDuplicationPlan.cs:514`, `RackPushBackCommands.cs:197` y `RackCantileverCommands.cs:246`.
   - Dos kinds no vacios distintos → `MixedKind`.
   - **Un miembro con `Kind` en blanco → `MixedKind`**, con un diagnostico que nombra esa definicion (bloque y
     handle) y remite a `RACKEDITAR`.
   - Un kind no vacio, desconocido para este build, pero igual en todos los miembros **no** bloquea: I-54 no
     interpreta el diseño.

   **Evidencia historica.**
   - Desde que existe el sobre, todo escritor pone `Kind` [P-23].
   - Un `Kind` en blanco con `Id` solo aparece en el payload Selectivo anterior al sobre, que se lee como sobre
     legible sin `Kind` ni `Design` [X P-23], o en datos ajenos.
   - La mayoria de los consumidores lo rechaza: variables, duplicado, Push Back, Cantilever, lista, inventario y
     `RACKEDITAR` sobre el propio bloque. Solo `FindRackBlocks` lo tolera [P-24].

   **Por que A y no B.**
   - Con **B** (excluir el kind de la comparacion), I-54 seria el unico comando que trata esa definicion como
     miembro **escribible**, en contra de `RACKDUPLICAR`, `RACKVARIABLES`, `RACKLISTA`, Push Back y Cantilever.
     Ademas mutaria un payload de origen incierto y no podria duplicarlo despues.
   - **Coste de A**: las propiedades de ese rack quedan en solo lectura, en un estado que ningun build de
     RackCad produce hoy.

   **El mensaje no promete reparacion.** Por lectura [L], `RACKEDITAR` → Actualizar desde otra vista reescribe
   el `Kind` en Selectivo, Dinamico y Cabecera, cuyos preflights no comprueban el kind (`FindRackBlocks` +
   `Compose` con kind constante). En Push Back y Cantilever tambien aborta, y sin ningun miembro con kind no
   hay editor que lo abra. I-54 no repara.
6. **Estado de cada coleccion.** Se lee la de cada miembro con el store. Si alguna es no escribible
   (`PresentButUnreadable`, `AmbiguousIdentity` o `IncompatibleMajor`), el rack es `Unreadable`:
   - solo lectura, con el estado de cada vista;
   - sin escritura, sin unificacion y sin descarte;
   - un miembro no escribible **nunca** se omite para seguir con los demas.
7. **Igualdad canonica (V2-4).** Dos colecciones son iguales si y solo si:
   - **las dos son vacias canonicas**: `Absent`, o `Readable` con cero entradas y sin `ExtensionData` de raiz,
     **con cualquier minor**; o
   - **las dos son `Readable` y coinciden en todo lo siguiente**:
     - mismo major (siempre 1 en una coleccion legible);
     - `ExtensionData` de raiz igual en profundidad (objetos por nombre `Ordinal` sin importar el orden de
       miembros, arrays en orden, strings ordinales, numeros por texto crudo);
     - mismo numero de entradas y, entrada a entrada **en orden**: `Id` por valor de `Guid`, `Name` ordinal,
       `Value` ordinal y `ExtensionData` de entrada igual en profundidad.

   **El minor no participa**: sin campos adicionales no transporta dato (V2-9), y si participase romperia la
   transitividad con «ausente ≡ vacio con cualquier minor». Por ejemplo, `{ausente, vacio@1.0, vacio@1.3}`
   daria divergencia falsa. Es la precision PR-01 de §21.
8. **Resultados**, evaluados en este orden. Los dos primeros se deciden sobre el bloque elegido, antes de
   mirar el resto del dibujo:
   1. `XrefRejected`;
   2. `NoIdentity`;
   3. `IndeterminateMembership(definiciones)`, que incluye el caso de un bloque elegido con payload no
      interpretable;
   4. `MixedKind(vistas)`;
   5. `Unreadable(estado por vista)`;
   6. `Divergent(resumen por vista)`;
   7. `Single(coleccion, versionDeEscritura)`.

   La `versionDeEscritura` de `Single` es el minor mayor entre los miembros, y nunca menor que la version
   actual, porque su contenido es identico. El orden del barrido no cambia el resultado.
9. **Escrituras** solo con `Single` (D-10) o con una unificacion segura (punto 10), siempre por el ejecutor de
   D-22. **Nunca se elige hermana**, y el workspace no presenta los valores de una vista como los del rack (D-10.3).
10. **Unificar desde la vista seleccionada (V2-2).** Solo existe si se cumple todo lo siguiente:
    - el resultado es `Divergent`, y por tanto no hay pertenencia indeterminada, `NoIdentity`, xref ni `MixedKind`;
    - **todos** los miembros son `Readable` o `Absent`: ninguno `PresentButUnreadable`, `AmbiguousIdentity` ni
      `IncompatibleMajor`;
    - el **usuario** elige explicitamente la vista origen; no hay seleccion por defecto;
    - toda coleccion que **se sobrescribiria** (la de cada miembro no igual canonicamente al origen):
      - **no** tiene `ExtensionData` de raiz;
      - **no** tiene `ExtensionData` en ninguna entrada;
      - **no** tiene un minor mayor que el del origen;
    - antes de confirmar se muestra el **contenido de cada vista**, y la confirmacion exige una casilla.

    Al escribir:
    - se escribe el documento origen, con su version resuelta sin degradar, **solo** en los miembros que
      difieren de el canonicamente. El origen y los ya iguales no se reescriben, para no degradar el minor de
      ninguno (PR-07);
    - todo va en **una** transaccion;
    - en la lectura fresca se exige ademas que la coleccion origen siga siendo canonicamente igual a la que se
      mostro (D-22).

    **El sistema nunca elige.**
11. **Descarte: no existe (V2-1).** `PresentButUnreadable`, `AmbiguousIdentity` e `IncompatibleMajor` no tienen
    salida destructiva en V1, en Rack ni en Proyecto: se conservan los bytes y se bloquea la escritura. Toda
    recuperacion destructiva futura queda fuera de alcance y exige una decision explicita del Owner (N-11).
12. **Flujos existentes.** En `RACKEDITAR`, cada miembro conserva su coleccion y una vista nueva hereda la del
    sobre elegido (D-21). No se añade ninguna comprobacion a esos flujos.

**Cambio desde V1.**
- Punto 3: sin filtro de colocacion.
- Puntos 4, 5 y 7 (sin minor): nuevos.
- Punto 6: sin unificar ni descartar.
- Punto 10: restringido.
- Punto 11: descarte eliminado.

**Descartado.** Reutilizar `SelectiveAuthoredAuthority`, que solo ve el diseño interior del Selectivo; elegir
la primera vista o la frontal; escribir solo las hermanas legibles; comparar bytes; comparar el minor.

### D-10 — Mutaciones (V2-15 · V2-1)

**Decision.**

1. **Operaciones:**

   | Operacion | Efecto exacto | Se revalida en la lectura fresca | Confirmacion |
   |---|---|---|---|
   | Crear | Id nuevo en minusculas `D`, añadido al final; nombre NFC recortado; valor tal como llega | autoridad `Single` (Rack), o `Absent`/`Readable` (Proyecto); nombre valido y unico; valor valido; conteo resultante ≤ 50; profundidad ≤ 16 | no |
   | Renombrar | Cambia **solo** `Name` de la entrada con ese id. `Id`, `Value` y el `ExtensionData` de la entrada quedan exactos. Se permite cambiar solo mayusculas | el id existe; nombre nuevo valido y sin colision con **otra** entrada | no |
   | Cambiar valor | Cambia **solo** `Value`. `Id`, `Name` y el `ExtensionData` de la entrada quedan exactos | el id existe; valor nuevo valido | no |
   | Eliminar | Quita **solo** esa entrada, con su `ExtensionData` | el id existe | no (UNDO de AutoCAD, observado en OV-08) |
   | Vaciar | Eliminar la ultima deja `Entries: []`. **Nunca** se borra el miembro del sobre ni la entrada del NOD | — | — |
   | Unificar | D-09.10 | D-09.10 | casilla |

2. **En toda mutacion** se conservan el `ExtensionData` de raiz y el de las entradas no tocadas. La version se
   resuelve sin degradar.
3. **Workspace cuando la autoridad no es `Single`.** **Nunca** presenta los valores de una hermana como valores
   del rack. Presenta:
   - el diagnostico y el modo solo lectura;
   - **cuando corresponde**, resumenes por vista: bloque, handle, vista y seccion, estado y, si la coleccion es
     legible, sus entradas `Nombre — Valor` etiquetadas con esa vista.

**Cambio desde V1.** Sin la fila «Descartar ilegible»; conservacion explicita del `ExtensionData` de entrada; el
punto 3 es nuevo.

**Por que.** Vaciar sin borrar conserva la version sin degradar y los campos que un build posterior hubiera
escrito. Como ausente ≡ vacio con cualquier minor, eso no crea divergencia.

### D-11 — Duplicacion e independencia (V2-7)

**Decision.**

1. `RACKDUPLICAR` y `RACKLAYOUT` (copia independiente) copian `CustomProperties` **tal cual** a traves de
   `RestampEnvelope`: mismos ids y mismos valores, y un contenido no escribible sigue no escribible. **Sin
   cambio de codigo.**
2. La independencia la da el contenedor: el sobre de la copia es otro, y escribir las propiedades de un rack
   nunca toca otro `RackId`.
3. `COPY` de AutoCAD y `RACKRELLENAR` comparten la definicion: **es el mismo rack**, con la misma autoridad.
4. Sin politica de copia por propiedad en V1: se copian todas (OQ-04 → DEFER).
5. **Contrato de prueba.** No se afirma que Core «prueba `RackEnvelopeRestamp`», porque ninguna suite carga el
   Plugin (P-08):

   | Capa | Que demuestra | Que **no** demuestra |
   |---|---|---|
   | **T-CPY-01** (Core, comportamiento) | La **propiedad** del `RackEmbedStore` de produccion de la que depende el restamp: deserializar un sobre con miembro, `View`, `Section`, `SchemaVersion` `1.7` y `ExtensionData`, mutar `Id`, `Name` y `Design` sobre ese objeto y serializarlo conserva `CustomProperties` (texto crudo exacto), `View`, `Section`, `SchemaVersion` y `ExtensionData`. Precedentes: `PersistenceUniformityTests.cs:193-217` en BASE, y el `DimensionViewsRestampTests.cs` de I-50 @ `ed50cbd`, que declara que las lineas del Plugin no se ejecutan | Que `RackEnvelopeRestamp.cs` haga eso |
   | **T-GRD-03** (Core, guarda estructural) | Sobre `RackEnvelopeRestamp.cs` enmascarado, en la sobrecarga con `Guid`: un solo `Deserialize(` asignado a un identificador X, un solo `Serialize(` con argumento X, sin `new RackEmbedDocument` y sin `RackEmbedComposer`. Archivo de prueba **nuevo**; **convive con G-R5** y **no la re-apunta** [P-27]; rojo demostrado con una violacion temporal | Comportamiento; es defensa secundaria |
   | **OV-05 y OV-06** (G9) | `RACKDUPLICAR` con varios origenes y vistas; `RACKLAYOUT` independiente; editar las propiedades de la copia no cambia el original, ni al reves; `COPY` nativo comparte | — |

6. **Mejora futura, registrada, no de I-54**: extraer a Application la mitad del restamp que maneja el sobre.
   Disparador: **cualquier cambio futuro al manejo del sobre en `RackEnvelopeRestamp.cs`**.
7. **I-52** tiene la obligacion de D-21.

**Descartado.** Regenerar ids en la copia: no añade independencia, que ya da el contenedor, y la regla es la de
ADR-0034 §12 (cambia la identidad del rack, no la del dato). Extraer ahora el restamp: tocaria
`RackEnvelopeRestamp.cs` y re-apuntaria G-R5 mientras I-52 lo reutiliza.

### D-12 — Biblioteca y exportaciones (V2-18)

**Decision.**

1. **Frontera de V1**: no se escriben propiedades en `.rackcad.json` ni se leen al importar. BOM, CSV, XLSX y
   `RACKLISTA` no cambian.
2. Es una **frontera de alcance**, no una clasificacion. V1 **no** afirma que las propiedades de rack sean de
   instancia: puede haber metadatos de diseño que en el futuro convenga llevar a la biblioteca.
3. **UX actual, documentada en la guia y observada en OV-11:**
   - guardar un rack en la biblioteca → sus propiedades **no viajan**;
   - abrir o insertar desde la biblioteca → el rack **nace sin propiedades** (importacion con `Compose(null)`, D-21).
4. **Punto de extension aditivo**: un slot en `RackProjectDocument`, que ya tiene `ExtensionData`. Queda sin
   diseñar.

**Cambio desde V1.** Punto 2 (se retira «cliente, area y ubicacion son de instancia») y punto 3.

### D-13 — Estados no escribibles y schema futuro (V2-1)

**Decision.**

| Estado del contenedor | Proyecto | Rack | Efecto en el resto |
|---|---|---|---|
| `Absent` | coleccion vacia, editable | vacia, editable si la autoridad es `Single` | ninguno |
| `Readable` | editable | editable si `Single` | ninguno |
| `Readable` con nombres repetidos | editable, con aviso | idem | ninguno |
| `PresentButUnreadable` | **solo lectura**; diagnostico; bytes intactos; sin «crear la primera» y sin descarte | **solo lectura**; bloquea escritura y unificacion de ese rack | ninguno |
| `AmbiguousIdentity` | **solo lectura**, con diagnostico propio | idem | ninguno |
| `IncompatibleMajor` | **solo lectura**; nunca se sobrescribe | idem | ninguno |

«Ninguno» significa que el dibujo, `RACKEDITAR`, el BOM, `RACKLISTA`, `RACKDUPLICAR`, `RACKLAYOUT` y
`RACKVARIABLES` se comportan igual que con un contenedor ausente, **dentro del alcance de aislamiento de D-08.4**
(INV-07). No hay recuperacion destructiva en V1 (N-11).

**Cambio desde V1.** Se eliminan «descarte explicito» y «unificar o descartar» sobre ilegibles.

### D-14 — Entre dibujos

**Decision.**
- Las propiedades de rack viajan con la definicion por los mecanismos nativos de AutoCAD; las de proyecto no.
  V1 no transfiere, fusiona ni sincroniza. Como las entradas son autodescriptivas, un rack pegado en otro
  dibujo no queda con referencias rotas [I; OV-09].
- Una definicion dependiente de un xref pertenece a otro DWG: no es miembro ni se escribe (D-09.4).

### D-15 — Punto de extension: dibujo (solo restriccion · V2-17)

**Decision.** I-54 solo garantiza una identidad estable y local al alcance, y una autoridad de la que leer. Un
consumidor futuro lee **de la autoridad** (`Single` en Rack; `Absent` o `Readable` en Proyecto) y no del sobre
crudo. Ante cualquier otro estado no presenta ningun valor, y el dibujo nunca pasa a ser autoridad.

**No fija** formato de rotulacion, cuadro de titulo, atributos, campos de AutoCAD, `DWGPROPS` ni ningun destino
concreto.

### D-16 — Punto de extension: expresiones (solo restriccion · V2-17)

**Decision.**
- La unica clave de referencia posible es `CustomPropertyId`; el nombre solo presenta.
- **No fija** sintaxis, tokens, namespaces ni ambitos. `Rack` y `Project` quedan reservados por I-49 para ID20
  (P-13) e I-54 no los reclama.
- Los valores son texto y V1 no define semantica de operando.
- No hay dependencia con I-49 en ningun sentido, y las guardas de formulas quedan intactas.
- La relacion ID24 ↔ ID20 se decide en ID20 (OQ-07).

### D-17 — Punto de extension: plantillas (solo restriccion · V2-17)

**Decision.** I-54 solo garantiza:
- una identidad estable y local al alcance;
- que se pueden añadir campos informativos por minor, en la raiz y por entrada (D-04.3);
- que un consumidor futuro puede referenciar por id.

**No fija** donde vive una plantilla, si reutiliza ids, ni la politica de preservar, derivar o crear ids al
aplicarla. Todo eso pertenece a su iniciativa.

**Cambio desde V1.** Se retira «aplicarla crearia entradas con los ids de la plantilla».

### D-18 — UI minima reutilizable (MATERIAL por superficie · V2-20)

**Decision.**

1. **Un comando** con palabra clave: «Selecciona un rack o [Proyecto]». Es arquitectonicamente aceptable.
   `RACKPROPIEDADES` y el alias `RPR` (libre hoy, [D §11.5]) son **nombres de trabajo**, no congelados: el Owner
   los decide antes de G7 (OQ-03).
2. **Bucle** del patron `RACKVARIABLES` (P-15): leer en una transaccion → proyectar → ventana modal → **un**
   intent por id → preflight puro sobre la instantanea → ejecutor con lectura fresca y decision de Application
   (D-22) → releer.
3. **Workspace puro** en Application, **agnostico al alcance**:
   - estados `Editable`, `ReadOnly(motivo)` y `Divergent(unificacion disponible o no)`;
   - filas `{ Id, Name, Value, NombreRepetido }` **solo** con `Editable`, es decir `Single` en Rack o `Absent`/
     `Readable` en Proyecto;
   - en cualquier otro caso, resumenes por vista (D-10.3), diagnosticos y la etiqueta del alcance.
4. **Ventana** nueva `RackCustomPropertiesWindow`, arquetipo **C**, que adopta `DialogWindowChrome`,
   `EditorActions` y las reglas D6, D7 y D9 de ADR-0029. Contiene:
   - la lista `Nombre — Valor`, el detalle y los botones Nueva, Renombrar, Cambiar valor y Eliminar;
   - un banner de solo lectura con el motivo;
   - un **panel de unificacion solo cuando esta disponible** (D-09.10), con vista origen sin seleccion por
     defecto, el contenido de cada vista y una casilla;
   - estado y Cerrar.

   **Sin panel de descarte** en ningun estado y sin `MessageBox`.
5. **Solo lectura** con pertenencia indeterminada, `NoIdentity`, xref, `MixedKind`, `PresentButUnreadable`,
   `AmbiguousIdentity` e `IncompatibleMajor`.
6. La ventana **no** ve `Database`, `ObjectId`, NOD ni sobres. Devuelve **intents por id** y **nunca es
   autoridad**: Application revalida cada intent sobre la lectura fresca (D-22).
7. **Censos previstos**: +2 `[CommandMethod]` (33 → 35 sobre `BASE_SHA`) y +1 ventana C (29 → 30; C: 11 → 12),
   mas la ayuda en `RackCommandReference`. **El censo real se verifica antes de G7**: I-52 V1 preve `RACKMIRROR`
   sin alias (33 → 34, `I-52-proposal-v1.md:76` @ `0fc7032`).
8. **Fuera de V1**: seccion en los editores de sistema, boton en el menu principal (OQ-03), columnas en
   `RACKLISTA`, reordenar e importar/exportar.

**Descartado.** Integrarlo en `RACKVARIABLES`, porque serian dos autoridades en una ventana (ADR-0034 §1). En
los editores de sistema: son archivos calientes (I-50 G3, I-53).

### D-19 — Nombres de codigo y guardas

**Decision.**

1. **Nombres orientativos** (solo son contractuales los que fijen las guardas):
   - Application: `CustomPropertiesDocument`, `CustomPropertyEntryDocument`, `CustomPropertyId`,
     `CustomPropertiesStore`, `CustomPropertiesReadResult`, `CustomPropertiesWriteGuard`,
     `RackCustomPropertiesAuthority`, `CustomPropertiesWorkspace`, `CustomPropertiesIntent`,
     `CustomPropertiesPreflight` y `CustomPropertiesCommit`;
   - Plugin: `CustomPropertiesData` y `RackPropiedadesCommands`.
2. **Prohibido** usar los substrings de P-12 en identificadores: `CustomPropertyValues` o `rackPropertyId`
   romperian la suite Core.
3. **Guardas nuevas**. Todas se demuestran en rojo con una violacion temporal y son defensa **secundaria**;
   el criterio de aceptacion es el comportamiento (leccion del BLOCKER de I-48).

   | Id | Guarda | Gate |
   |---|---|---|
   | T-GRD-01 | Construccion del sobre: en todo `src/`, sobre codigo enmascarado, `new RackEmbedDocument`, y el `new(` con tipo destino declarado `RackEmbedDocument`, solo aparecen en `RackEmbedComposer.cs`. `tests/` queda fuera (P-25) | G4 |
   | T-GRD-02 | Censo de llamadas a `RackEmbedComposer.Compose(` en `src/`, fuera de `RackEmbedComposer.cs`: exactamente las siete de D-21, por archivo. Una llamada nueva falla y obliga a clasificar su `source` en D-21 | G4 |
   | T-GRD-03 | Restamp estructural (D-11.5), sin re-apuntar G-R5 | G4 |
   | T-GRD-04 | Ejecutores de propiedades: una transaccion y un `Commit`; `ScanEnvelopes(` y no `FindRackBlocks(`; ni `Regen(`, `RedefineSystemBlock`, `EnsureForPlan`, `PurgeUnreferenced` ni `PurgeAfterCommit`; sin decisiones propias, es decir, sin referencias a la autoridad o al preflight fuera de la llamada de commit a Application | G6 |
   | T-GRD-05 | Independencia de Proyecto: los archivos de propiedades no referencian `ProjectVariables*` ni `RACKCAD_PROJECT`, y los de Project Variables no referencian `CustomProperties*` ni `RACKCAD_CUSTOM_PROPERTIES` | G6 |
   | T-GRD-06 | Capas: Domain sin propiedades; UI sin `Autodesk`; los tipos de propiedades de Application sin `ObjectId`, `Database` ni `Transaction` | G6 |
   | T-GRD-07 | Constante de clave del NOD exacta; ningun substring de P-12 | G6 |
   | T-GRD-08 | Censos de comandos y ventanas actualizados, sin relajarlos | G7 |

### D-20 — ADR

**Decision.** El ADR es **obligatorio** antes de producir codigo. Su alcance candidato esta en §15; no se
reserva numero ni se redacta. Nace `propuesto` en el Consensus Freeze y **debe existir antes de G3**.

### D-21 — Invariante de preservacion del sobre (MATERIAL · V2-6)

**Decision.** Invariante arquitectonico:

> **Todo sobre derivado de un rack existente se origina en (A) `RackEmbedComposer.Compose(source, …)`, con el
> sobre real de ese rack como `source`, o en (B) el re-estampado del MISMO objeto deserializado.**
> `Compose(null, …)` solo se permite para un **rack nuevo** y para una **importacion desde la biblioteca**.

Se aplica a redibujo, vista nueva, propagacion de Project Variables, duplicado, layout, espejo y **cualquier
camino futuro**.

| Camino hoy (`BASE_SHA`) | Mecanismo | `source` | Evidencia |
|---|---|---|---|
| `RACKEDITAR` → Actualizar, seis sistemas | A | el sobre de cada miembro | llamadas en `RackSelectivoCommands.cs:390`, `RackDinamicoCommands.cs:380`, `RackPushBackCommands.cs:415`, `RackCabeceraCommands.cs:195`, `RackCamaCommands.cs:197` y `RackCantileverCommands.cs:493` [E]; que cada llamador pase el suyo [L, D §5.3] |
| `RACKEDITAR` → vista nueva | A | el sobre elegido | [L, D §5.4] |
| Propagacion de variables | A | `view.Embed` | `ProjectVariableMutationExecutor.cs:208`; guarda `ProjectVariableMutationExecutorTests.cs:288-298` [E] |
| `RACKDUPLICAR` y `RACKLAYOUT` | B | — | `RackEnvelopeRestamp.cs:60,87` [E] |
| Rack nuevo (menu, `Quick*`) | `Compose(null)` | `null` | los constructores de payload declaran `source = null` por defecto [E]; que los caminos de rack nuevo no lo pasen [L, D §5.4] |
| Importacion de biblioteca | `Compose(null)` | `null` | [D §9.3] |
| Espejo de I-52 (futuro) | **A y despues B**: `Compose(sobreFuente, …)` con el diseño reflejado y `RestampEnvelope` sobre ese payload | real | `I-52-proposal-v1.md:316,698-702` @ `0fc7032` [E]: cumple el invariante |
| Escritura de propiedades de I-54 | `WithCustomProperties` sobre el sobre fresco de cada miembro, en el compositor | real | D-22 |

**Como se vigila.**
- **T-ENV-08**: comportamiento de `Compose`.
- **T-GRD-01**: construccion.
- **T-GRD-02**: censo de llamadas a `Compose`.
- **T-GRD-03**: restamp.
- **OV-03, OV-04, OV-07, OV-11 y OV-13**: los caminos del Plugin, que Core no ejecuta.

Las guardas **no** sustituyen las pruebas de comportamiento.

**Limite declarado.** T-GRD-02 no ve a quien llama a los constructores de payload con `source = null`. Esos
caminos los cubren las OV del parrafo anterior.

**Obligacion explicita para I-52.** Todo camino de escritura del espejo cumple el invariante: `Compose` con el
sobre fuente real y/o re-estampado del mismo objeto, nunca `new RackEmbedDocument` ni `Compose(null)` sobre un
rack existente.

Su Proposal V1 (`0fc7032`) ya lo hace (ID-5, §10.1: reflejar → `Compose(sobreFuente, …)` → `RestampEnvelope`).
Tambien prevé la prueba T-M20 («propiedades de alcance Rack sobreviven exactas» si I-54 esta integrada) y declara
que depende de que `Compose` herede el miembro (`:735-737`).

La llamada nueva a `Compose` del espejo cambia el censo de T-GRD-02 de 7 a 8. Quien integre despues re-apunta el
censo y clasifica esa llamada en esta tabla, como A con `source` real. Si I-52 se integra antes que I-54, I-54 lo
re-mide al rebasar y sus guardas lo comprueban; si despues, las guardas de I-54 ya lo vigilan.

### D-22 — Ejecutor de alcance Rack (MATERIAL · V2-14)

**Decision.**

```text
LEER       tx { ScanEnvelopes(tx, db, refs:true) → proyeccion plana } → Application: pertenencia → autoridad → workspace
PREFLIGHT  Application(intent, proyeccion de la instantanea) → aceptado | rechazado   (aviso temprano; no autoritativo)
ESCRIBIR   tx { ScanEnvelopes(tx, db, refs:true) → proyeccion plana FRESCA
                → Application.Commit(intent, proyeccion fresca) → plan | abortar
                → por cada definicion del plan: RackBlockData.Write(tx, definicion, payload)
                → Commit }
RELEER     LEER
```

1. **Una transaccion del llamador** para todo el ciclo: barrido → pertenencia → autoridad → mutacion →
   escritura de todos los miembros → confirmacion. Todo o nada (INV-05).
2. **Proyeccion plana hacia Application**, por definicion:
   - handle como string y nombre de bloque;
   - colocada (referencias directas > 0) y dependiente de xref;
   - si el sobre es legible;
   - el `RackEmbedDocument` deserializado, cuando existe.

   **Sin** `ObjectId`, `Database` ni `Transaction`. El Plugin guarda en la misma transaccion el mapa
   handle → `ObjectId`.
3. **Application decide**: pertenencia, autoridad, validacion del intent y **plan de mutacion**. El plan es la
   lista de pares `(handle, payload)`, y cada payload es `WithCustomProperties(sobre fresco de ese miembro,
   documento resultante)` serializado.
4. **El Plugin** toma la instantanea, llama a Application sobre la lectura **fresca**, escribe y confirma. **No
   toma ninguna decision semantica** (T-GRD-04). Precedente: `RegistryCommit` en
   `ProjectVariableMutationExecutor.cs:250-288` [P-26].
5. **Se revalida sobre la lectura fresca** y se aborta sin escribir si algo falla:
   - **identidad**: el `RackId` sigue teniendo miembros y el id de la entrada existe;
   - **autoridad**: sigue `Single`, o para unificar sigue `Divergent` segura con la misma vista origen y con la
     coleccion origen canonicamente igual a la mostrada;
   - **nombre y limites**: D-05 sobre el documento resultante;
   - **profundidad**: ≤ 16.
6. **Se rechazan las xref**: una definicion `IsFromExternalReference` o `IsDependent` nunca entra en un plan
   (D-09.4).
7. **Sin** redefinir bloques, importar, purgar ni regenerar: solo `RackBlockData.Write`. `InDocumentTransaction`
   basta para un cuerpo de una fase (`InDocumentTransaction.cs:7-15`).
8. **UNDO.** Su granularidad para una escritura de Xrecords sin redefinicion **no se afirma**: OV-08 la observa
   y la registra. I-51 valido `UNDO` tras varios destinos de `RACKDUPLICAR` (`HANDOFF.md:1133-1136`), pero es
   otro camino de escritura.
9. **El ejecutor de Proyecto** sigue el mismo esquema sobre el NOD (D-07.5).

**Cambio desde V1.** Transaccion explicita, `ScanEnvelopes`, reparto Application/Plugin, revalidacion
enumerada, rechazo de xref y UNDO no afirmado.

## 7. Invariantes

| # | Invariante | Cambio |
|---|---|---|
| INV-01 | Una coleccion de Proyecto por `Database`; una coleccion de Rack por `RackId`, canonicamente identica en cada miembro | precisado |
| INV-02 | La identidad de una entrada es su `CustomPropertyId`. El nombre nunca es identidad, clave de busqueda ni clave de agregacion | ampliado |
| INV-03 | Renombrar cambia solo `Name` y cambiar valor solo `Value`; los dos conservan el `ExtensionData` de la entrada | ampliado |
| INV-04 | Escribir propiedades no toca geometria, diseño, `Kind`, `Id`, `Name`, `View`, `Section` ni `ExtensionData` del sobre; su `SchemaVersion` solo se resuelve sin degradar, como en `Compose` | precisado |
| INV-05 | Una escritura de alcance Rack deja la coleccion resultante en todos los miembros en **una** transaccion, o no cambia ninguno | igual |
| INV-06 | **Nada se escribe sobre `PresentButUnreadable`, `AmbiguousIdentity` ni `IncompatibleMajor`, y no hay recuperacion destructiva en V1** | **cambiado** |
| INV-07 | **Aislamiento acotado**: dado un sobre sintacticamente valido y dentro de la profundidad soportada, el estado de las propiedades no cambia su legibilidad ni el resultado de `RACKEDITAR`, BOM, `RACKLISTA`, `RACKDUPLICAR`, `RACKLAYOUT` y `RACKVARIABLES` | **acotado** |
| INV-08 | Un sobre sin propiedades se serializa byte a byte igual que en `BASE_SHA` | igual |
| INV-09 | **Preservacion del sobre** (D-21) | **nuevo** |
| INV-10 | Una copia independiente conserva ids y valores; una copia enlazada es el mismo rack | igual |
| INV-11 | El sistema nunca elige hermana, ni presenta los valores de una como los del rack | ampliado |
| INV-12 | Un payload RackCad no interpretable, colocado o no, bloquea las escrituras de Rack. Un sobre legible con `Id` en blanco y una definicion dependiente de xref no son miembros ni bloquean | **cambiado** |
| INV-13 | Domain no conoce propiedades; la UI no conoce AutoCAD; Application no conoce `ObjectId`, `Database` ni `Transaction`; el Plugin no toma decisiones semanticas | ampliado |
| INV-14 | Ninguna propiedad se lee, escribe ni resuelve como variable, expresion o formula | igual |
| INV-15 | La biblioteca no exporta ni importa propiedades en V1, como **frontera** | precisado |
| INV-16 | No cambian `RACKCAD_PROJECT`, `ProjectVariablesData`, `ProjectVariablesDocument`, `RackBlockData`, `RackBlockFinder`, `RackEnvelopeRestamp`, `RackCloner` ni los `Edit*` | ampliado |
| INV-17 | Propiedades de Proyecto y Project Variables son independientes en ambos sentidos | **nuevo** |
| INV-18 | Unificar nunca sobrescribe contenido desconocido (`ExtensionData` de raiz o de entrada, minor mayor) y nunca esta disponible con miembros no escribibles | **nuevo** |
| INV-19 | V1 no define claves de agregacion entre racks | **nuevo** |
| INV-20 | Ningun escritor emite `default(JsonElement)`, un elemento `Null`, `ExtensionData` que colisione con un miembro declarado ni un documento de propiedades de mas de 16 niveles | **nuevo** |
| INV-21 | La UI nunca es autoridad: los intents por id se revalidan en Application sobre la lectura fresca | **nuevo** |

## 8. Flujos

```text
LEER PROYECTO     tx { NOD → lectura tri-estado → store } → workspace
ESCRIBIR PROYECTO intent → preflight(instantanea) → tx { releer → acreditar → aplicar por id → D-05 → guarda → escribir } → commit → releer
LEER RACK         pick → [xref ⇒ rechazo] → tx { ScanEnvelopes → proyeccion plana } → pertenencia → autoridad → workspace
ESCRIBIR RACK     intent → preflight → tx { ScanEnvelopes → proyeccion fresca → Commit (Application) → por miembro: RackBlockData.Write } → commit → releer   (sin regen)
UNIFICAR          intent(vista origen, confirmado) → tx { ScanEnvelopes → proyeccion fresca → Commit: Divergent segura, origen igual al mostrado → escribir solo los miembros distintos } → commit
NO ESCRIBIBLE     lectura → solo lectura con diagnostico; no existe camino de escritura
RACKEDITAR        sin cambios de codigo → Compose hereda la coleccion de cada miembro
VISTA NUEVA       sin cambios → Compose hereda la del sobre elegido
RACKDUPLICAR      sin cambios → RestampEnvelope conserva el miembro
BIBLIOTECA        sin cambios → ni exporta ni importa; importar compone con null
```

## 9. Compatibilidad

| Build que abre el dibujo | Rack con propiedades: dibujar y editar sistema | Redibujo y duplicado | Editor de propiedades |
|---|---|---|---|
| Anterior a I-11 | normal | **pierde** las propiedades al reescribir el sobre [X P-18] | no existe |
| I-11 … antes de I-54 | normal | **las conserva** sin interpretar [X P-18] | no existe |
| I-54 | normal | las conserva | segun D-13 |
| Futuro, mismo major 1.x | normal | las conserva; minor sin degradar | editable; campos nuevos conservados; unificar bloqueado sobre ellos |
| Futuro, coleccion major 2 | normal (el sobre sigue 1.x) | conserva los bytes | `IncompatibleMajor`: solo lectura |
| Payload Selectivo anterior al sobre (`Kind` en blanco) | como hoy (P-24) | como hoy | su rack da `MixedKind`: solo lectura |

## 10. Riesgos y mitigaciones

| Riesgo del Discovery | Mitigacion en V2 |
|---|---|
| R-01 campo perdido en redibujo | D-08.3 herencia; D-21; T-ENV-08; T-GRD-02 |
| R-02 sobre ilegible por contenido | D-08.4, afirmacion acotada; restricciones A..G; T-ENV-02..07 |
| R-03 divergencia silenciosa | D-09 autoridad; D-22 escritura unica; unificacion segura |
| R-04 pertenencia indeterminada | D-09.3, sin filtro de colocacion |
| R-05 migrar `RACKCAD_PROJECT` | D-07: no se migra |
| R-06 acoplar con variables | D-07.6 independencia; T-GRD-05 |
| R-07 dato fuera del sobre | D-08: dentro del sobre |
| R-08 DTO de sistema | D-08: descartado |
| R-09 restamp sin prueba e I-52 | D-11.5 contrato honesto; D-21, obligacion de I-52 |
| R-10 nombres que rompen guardas | D-19.2; T-GRD-07 |
| R-11 sintaxis `Rack.X` | D-16: sin sintaxis |
| R-12 builds anteriores a I-11 | **Riesgo residual declarado**: al reescribir pierden las propiedades de rack y la promocion no lo evitaria (D-08.5). Se informa al Owner en el freeze |
| R-13 nulos en todos los sobres | `WhenWritingNull`; T-CHR-01 y T-ENV-01 |
| R-14 conflictos de censos y docs | Declarados; se resuelven al integrar; censo verificado antes de G7 |
| R-15 expectativa entre dibujos | D-14; OV-09 |

**Riesgos propios de V2**

| # | Riesgo | Mitigacion |
|---|---|---|
| RP-01 | Una definicion ilegible ajena, **incluso no colocada**, bloquea las escrituras de Rack de todo el dibujo | Diagnostico con bloque, handle y colocacion; Proyecto y el resto de funciones siguen; coste aceptado en ADR-0034 «Riesgos» |
| RP-02 | Un rack `Divergent` no tiene salida en V1 si, elija el usuario la vista origen que elija, alguna coleccion que se sobrescribiria lleva `ExtensionData` o un minor mayor | Fallo cerrado deliberado (V2-2). La salida es un build que entienda esos campos o una recuperacion futura aprobada por el Owner |
| RP-03 | Un fallo a medias de `RACKEDITAR` deja divergentes otros datos del rack | `RACKEDITAR` no cambia la coleccion: cada miembro conserva la suya |
| RP-04 | I-52 escribe el sobre del espejo con `new RackEmbedDocument` o `Compose(null)` | D-21: obligacion y guardas T-GRD-01..03. Su V1 (`0fc7032`) compone con el sobre fuente y re-estampa; el riesgo queda en que su diseño cambie |
| RP-05 | Los censos cambian tambien en I-52: comandos 33 → 34 y una llamada nueva a `Compose` (T-GRD-02: 7 → 8) | Conflicto textual; quien integre despues re-apunta y clasifica; el censo real se verifica antes de G7 |
| RP-06 | Un rack con una vista sin `Kind` queda en solo lectura | D-09.5; un estado que ningun build actual produce |
| RP-07 | Una definicion dependiente de xref con sobre RackCad se comporta de forma distinta a lo inferido | D-09.4 conservador (nunca se escribe); OV-14 |
| RP-08 | La precision PR-01 (el minor no participa en la igualdad) es incorrecta | Se somete a la revision exact-SHA (§21) |

## 11. Mapa previsto de archivos (orientativo, no autorizado)

| Capa | Nuevos | Modificados |
|---|---|---|
| Domain | — | — |
| Application | ~11: documento, entrada, id, store, resultado, guarda, autoridad y pertenencia, intent, preflight, commit y workspace | `Persistence/RackEmbedDocument.cs` (+1 miembro); `Persistence/RackEmbedComposer.cs` (+1 asignacion, `WithCustomProperties`, normalizacion y rechazo de colision) |
| Plugin | `CustomPropertiesData.cs`; `RackPropiedadesCommands.cs` (comando, lectura, ejecutores de Proyecto y Rack) | **ninguno**. No se tocan `RackBlockFinder.cs`, `RackCommandSupport.cs`, `RackEnvelopeRestamp.cs`, `RackCloner.cs`, `RackBlockData.cs` ni `ProjectVariablesData.cs` |
| UI | `RackCustomPropertiesWindow.xaml(.cs)` | `RackCommandReference.cs` |
| Tests Core | Store, sobre y caracterizacion, autoridad, mutaciones, workspace, y guardas T-GRD-01..07 en archivos **nuevos** | `SelectiveEditorOpenTests.cs` (censo de comandos). **No** se toca `SelectiveDuplicationFailClosedTests.cs` (G-R5) |
| Tests UI | Ventana | `WindowCensusGuardTests.cs` |
| Docs | ADR; guia de uso; `README`, `despliegue.md`, `validacion-manual-autocad.md` e `ideas-futuras.md` | Contrato; HANDOFF y ROADMAP solo al integrar |

Hoy ninguno de estos archivos de produccion lo toca I-49, I-50, I-52 ni I-53 (§13).

## 12. Evidencia de pruebas y validacion

**Nada de esto se implementa ahora.** Toda seleccion filtrada debe demostrar que ejecuto al menos una prueba
(AGENTS.md). Las guardas son defensa secundaria.

### 12.1 STORE — Core, G3

| Id | Afirma |
|---|---|
| T-STO-01 | **Ausente** (sin texto; miembro nulo) → `Absent`, coleccion vacia, se puede escribir |
| T-STO-02 | **Vacio** `{"SchemaVersion":"1.0","Entries":[]}` → `Readable` con cero entradas |
| T-STO-03 | **Schema ausente**, en blanco, `"1"`, `"1.0.0"`, `" 1.0"` o no numerico → `PresentButUnreadable` |
| T-STO-04 | **Schema malo**: raiz no objeto; `Entries` ausente, nulo o no array; entrada no objeto; `SchemaVersion` no string → `PresentButUnreadable` |
| T-STO-05 | **Major futuro** (`2.0` con `Value` numerico y otra forma) → `IncompatibleMajor`, sin excepcion y sin enlazar entradas |
| T-STO-06 | **Minor futuro** (`1.7` con campos desconocidos) → `Readable`; reescribir conserva `1.7` |
| T-STO-07 | **Campos desconocidos** de raiz y de entrada sobreviven exactos a reescribir |
| T-STO-08 | **JSON invalido** o truncado → `PresentButUnreadable`; nunca lanza |
| T-STO-09 | **Profundidad**: un documento con `ExtensionData` a 40 niveles se lee `Readable`; una operacion cuyo resultado supere 16 se rechaza (`DepthLimitExceeded`) sin escribir; lo que escribe V1 tiene profundidad 3 |
| T-STO-10 | **Nulos**: miembro `null` o elemento `Null` → `Absent`; `Value: null` o `Name: null` → `PresentButUnreadable` |
| T-STO-11 | **`JsonElement` Undefined** (`default`) → `Absent`, sin excepcion |
| T-STO-12 | **Miembro JSON duplicado** en el documento (`Entries` y `entries`; `Value` dos veces en una entrada) → `PresentButUnreadable` |
| T-STO-13 | **Colision con `ExtensionData`**: al leer, ningun miembro declarado, en ninguna capitalizacion, acaba en `ExtensionData` de raiz ni de entrada; un `entries` solo se lee como `Entries` y se reescribe `Entries` |
| T-STO-14 | **Id duplicado** (mismo GUID en otra capitalizacion) → `AmbiguousIdentity`, distinto de ilegible; con otra invalidez estructural gana `PresentButUnreadable` |
| T-STO-15 | **GUID `D` estricto**: se aceptan 36 caracteres en minusculas y en mayusculas. Se rechazan espacio o tabulador alrededor, llaves, parentesis, formato `N`, 35 o 37 caracteres y `Guid.Empty`. Se escribe en minusculas |
| T-STO-16 | **Nombre Unicode NFC**: una entrada en NFD se normaliza al escribir; «Área» NFC frente a NFD colisiona en unicidad; tambien «Área» frente a «área»; el diagnostico de repetidos usa NFC; los nombres no tocados no se reescriben |
| T-STO-17 | **Nombres repetidos** al leer → `Readable` con diagnostico |
| T-STO-18 | **Limites de escritura**: nombre de 1..80 tras NFC y recorte, sin controles; valor ≤ 1000 con CR, LF y tabulador; ≤ 50 entradas al crear. Leer nunca aplica limites (60 entradas o un valor de 1500 caracteres son `Readable`). Renombrar una entrada con un valor de 1500 caracteres se permite |
| T-STO-19 | **Guarda**: solo se escribe tras `Absent` o `Readable`; nunca tras un estado no escribible; no existe ninguna API de descarte |
| T-STO-20 | **Ida y vuelta**: conserva orden, ids, nombres y valores (Unicode, saltos de linea); siempre emite `SchemaVersion` y `Entries`; ids en minusculas; la version nunca se degrada; una tabla de textos externos arbitrarios nunca produce excepcion |

### 12.2 ENVELOPE — Core, G4

| Id | Afirma |
|---|---|
| T-CHR-01 | **Caracterizacion de BASE, antes de tocar el sobre**: bytes de sobres representativos sin el miembro, con y sin `ExtensionData`, de cada kind |
| T-CHR-02 | Caracterizacion de BASE: `Compose(source)` hereda `SchemaVersion` y `ExtensionData`; bytes de `Compose(null)` |
| T-CHR-03 | Caracterizacion de BASE: ida y vuelta del store (campos desconocidos, minor mayor, major futuro → `null`) |
| T-ENV-01 | **Nulo omitido**: tras el cambio, los sobres sin miembro son byte a byte T-CHR-01 |
| T-ENV-02 | **Formas de `JsonElement`**: objeto, string, numero, array, `true` y `false` → sobre legible y texto crudo exacto; `null` literal → ausente y eliminado; el store clasifica cada forma |
| T-ENV-03 | **Corrupcion sintactica** o truncado dentro del miembro → sobre `null`, igual que en BASE: documenta el alcance del aislamiento |
| T-ENV-04 | **Profundidad**: 63 niveles en el miembro se leen y 64 dan `null`, en BASE y con el miembro; el escritor nunca pasa de 16 |
| T-ENV-05 | **Undefined y `Null`**: `Compose` y `WithCustomProperties` los normalizan a `null`; `Serialize` nunca lanza con un sobre del compositor |
| T-ENV-06 | **Clave del miembro duplicada** en el JSON del sobre: gana la ultima (residual documentado); ningun escritor RackCad la emite |
| T-ENV-07 | **Colision con `ExtensionData`**: el store nunca pone un miembro declarado del sobre en `ExtensionData`, ni exacto ni con otra capitalizacion; `Compose` y `WithCustomProperties` rechazan un origen con colision antes de producir nada |
| T-ENV-08 | **Preservacion en `Compose`**: con `source` conserva el miembro exacto; dos vistas conservan cada una la suya; con `Compose(null)` no hay miembro |
| T-ENV-09 | **`WithCustomProperties`** cambia solo el miembro y conserva los demas y `ExtensionData`; version sin degradar |
| T-ENV-10 | **Caracterizacion de build anterior**: un DTO local con la forma de BASE guarda el miembro en `ExtensionData` y lo reemite exacto, por `Compose` y por restamp del mismo objeto |
| T-ENV-11 | La exportacion a biblioteca no lee ni escribe el miembro |
| T-ENV-12 | **`Compose(null)` solo en los casos esperados**: T-GRD-02 clasifica las siete llamadas; los caminos del Plugin, en OV-03, OV-04, OV-07, OV-11 y OV-13 |
| T-GRD-01 | Guarda de construccion (D-19), con rojo |
| T-GRD-02 | Censo de `Compose` (D-19), con rojo |

### 12.3 AUTHORITY — Core, G5

| Id | Afirma |
|---|---|
| T-AUT-01 | Todos los miembros legibles e iguales → `Single` |
| T-AUT-02 | **Ausente ≡ vacio con cualquier minor**: `{ausente, vacio@1.0, vacio@1.3}` → `Single`, transitivo. Caso entre builds: coleccion vacia escrita `1.0` por I-54 frente a vacia leida a `1.1` |
| T-AUT-03 | **Divergencia legible** en valor, nombre, orden, id o conteo → `Divergent`, con resumen por vista |
| T-AUT-04 | **Divergencia de `ExtensionData`** de raiz o de entrada → `Divergent`; igualdad en profundidad sin importar el orden de miembros; numeros por texto crudo |
| T-AUT-05 | Solo cambia el minor, con contenido identico → `Single`; version de escritura = minor mayor |
| T-AUT-06 | **Payload ilegible** en cualquier parte, colocado y de otro rack → `IndeterminateMembership` para todo rack; el diagnostico lista bloque, handle y colocacion |
| T-AUT-07 | **Payload ilegible no colocado** → tambien `IndeterminateMembership` (V1 lo ignoraba) |
| T-AUT-08 | **`RackId` en blanco** en un sobre legible → no es miembro ni bloquea; elegido → `NoIdentity` |
| T-AUT-09 | **`Kind` en blanco** en un miembro → `MixedKind`, con diagnostico de la definicion; incluye la forma del payload Selectivo anterior al sobre |
| T-AUT-10 | **`MixedKind`** con dos kinds no vacios distintos; una diferencia solo de mayusculas es el mismo kind; un kind desconocido pero comun no bloquea |
| T-AUT-11 | **xref**: una definicion dependiente no es miembro, no bloquea y no entra en un plan; elegir una xref → `XrefRejected` |
| T-AUT-12 | Coleccion de un miembro `PresentButUnreadable`, `AmbiguousIdentity` o `IncompatibleMajor` → `Unreadable`, solo lectura, sin unificar ni escribir; nunca se omite ese miembro |
| T-AUT-13 | **Sin hermana arbitraria**: ningun resultado distinto de `Single` expone filas de una vista como del rack; permutar el orden del barrido no cambia el resultado |
| T-AUT-14 | **Matriz de unificacion**: disponible solo segun D-09.10. La bloquean el `ExtensionData` de raiz o de entrada en una vista que se sobrescribiria, un minor mayor en ella y cualquier miembro no escribible. Se permite como origen una vista con `ExtensionData` si las demas estan limpias |
| T-AUT-15 | El orden de precedencia de resultados (D-09.8) es determinista |
| T-AUT-16 | **Workspace**: estados y filas por alcance; filas solo con `Editable`; en otro caso, resumenes por vista |

### 12.4 MUTATIONS — Core, G3 puras y G5 commit

| Id | Afirma |
|---|---|
| T-MUT-01 | **Crear**: id nuevo en minusculas `D`, al final, nombre NFC recortado, limites |
| T-MUT-02 | **Renombrar**: solo cambia `Name`; `Id`, `Value` y `ExtensionData` de la entrada exactos; se permite cambiar solo mayusculas; colision con otra entrada → rechazo |
| T-MUT-03 | **Cambiar valor**: solo cambia `Value`; `ExtensionData` de la entrada exacto |
| T-MUT-04 | **Eliminar**: quita solo esa entrada y su `ExtensionData`; el resto exacto |
| T-MUT-05 | **Vaciar**: eliminar la ultima deja `Entries: []` y conserva `ExtensionData` de raiz y version; el contenedor permanece |
| T-MUT-06 | **Conservar el `ExtensionData` de entrada** al renombrar y cambiar valor en un documento `1.7` |
| T-MUT-07 | **Instantanea obsoleta**: con el intent calculado sobre la instantanea, la lectura fresca difiere (entrada borrada, nombre ya ocupado, miembro ilegible, payload ilegible nuevo en el dibujo, autoridad `Divergent`, miembro añadido o coleccion origen cambiada) → abortar sin plan |
| T-MUT-08 | **Plan atomico para todos los miembros**: al editar incluye todos; al unificar, solo los distintos del origen; nunca dependientes de xref; cada payload se deserializa al sobre fresco de su miembro, que solo cambia en el miembro de propiedades |
| T-MUT-09 | **Intent por id**: un id inexistente se rechaza; el nombre nunca localiza |
| T-MUT-10 | **Commit de unificacion**: la vista origen sigue siendo miembro legible y su coleccion es la mostrada; condiciones de D-09.10 re-evaluadas |

### 12.5 COPY

| Id | Afirma | Gate |
|---|---|---|
| T-CPY-01 | Propiedad del store de produccion de la que depende el restamp (D-11.5) | G4 |
| T-CPY-02 | = T-GRD-03, guarda estructural del restamp, con rojo y conviviendo con G-R5 | G4 |
| OV-05 | `RACKDUPLICAR` multivista y con varios origenes; `RACKLAYOUT` independiente; original y copia independientes | G9 |
| OV-06 | `COPY` nativo: misma autoridad | G9 |

### 12.6 PROJECT — Core, G6

| Id | Afirma |
|---|---|
| T-PRJ-01 | Lector del NOD: constante de clave, tri-estado y ningun camino que lance (guarda sobre `CustomPropertiesData.cs`); fisico en OV-01 y OV-10 |
| T-PRJ-02 | **Independiente de un `RACKCAD_PROJECT` corrupto**: lectura, escritura y workspace de propiedades no referencian Project Variables (T-GRD-05), y sus entradas son solo el payload de propiedades |
| T-PRJ-03 | **`RACKCAD_PROJECT` independiente de propiedades corruptas**: el diff del candidato no toca `ProjectVariables*` y esos archivos no referencian propiedades (T-GRD-05) |
| T-PRJ-04 | Ejecutor de Proyecto: releer, acreditar, aplicar por id, guarda, escribir; una transaccion; sin regen |
| T-GRD-04..07 | D-19 |

### 12.7 UI — UI tests, G7

| Id | Afirma |
|---|---|
| U-01 | Filas `Nombre — Valor` de una coleccion `Editable` |
| U-02 | **Estados de solo lectura**: pertenencia indeterminada, `NoIdentity`, xref, `MixedKind`, `PresentButUnreadable`, `AmbiguousIdentity` e `IncompatibleMajor` deshabilitan el editor y muestran el motivo |
| U-03 | **Sin descarte destructivo**: ningun estado tiene un control de descarte |
| U-04 | **Unificar seguro**: el panel solo aparece si esta disponible, muestra el contenido por vista, no tiene origen por defecto y exige la casilla |
| U-05 | **Intent por id**: Nueva, Renombrar, Cambiar valor y Eliminar producen intents con id; la ventana nunca muta colecciones |
| U-06 | **La UI no es autoridad**: la ventana solo presenta el workspace y no referencia store, autoridad ni commit |
| U-07 | `Divergent`: resumenes por vista, sin filas de rack |
| U-08 | Aviso de nombres repetidos |
| U-09 | Sin `MessageBox`; Cerrar y Escape segun ADR-0029 |
| U-10 | Arquetipo C declarado; T-GRD-08 |

### 12.8 Validacion del Owner — AutoCAD 2025, G9

| # | Escenario |
|---|---|
| OV-01 | Proyecto: crear, renombrar, cambiar y eliminar; guardar, cerrar y reabrir |
| OV-02 | Selectivo con frontal, laterales y planta: crear propiedades; todas las vistas muestran lo mismo; reabrir |
| OV-03 | `RACKEDITAR` → Actualizar en los seis sistemas conserva las propiedades |
| OV-04 | Insertar vista nueva desde `RACKEDITAR` hereda las propiedades |
| OV-05 | `RACKDUPLICAR` multivista y con varios origenes, y `RACKLAYOUT` independiente, llevan las propiedades; editar las de la copia no cambia el original, ni al reves |
| OV-06 | `COPY` nativo comparte las propiedades: editar en una copia se ve en la otra |
| OV-07 | `RACKVARIABLES` con un Selectivo vinculado: cambiar un valor conserva las propiedades |
| OV-08 | `UNDO` tras escribir propiedades de Proyecto y de Rack: se **observa y registra** la granularidad, sin afirmarla antes |
| OV-09 | Copiar un rack a otro DWG: lleva las del rack, no las del proyecto |
| OV-10 | `PURGE` y `AUDIT` tras escribir: la entrada del NOD sigue |
| OV-11 | Biblioteca: guardar → las propiedades no viajan; abrir o insertar → el rack nace sin propiedades |
| OV-12 | `RACKLISTA` y `RACKBOMTOTAL` sin cambios |
| OV-13 | Un rack nuevo desde el menu nace sin propiedades |
| OV-14 | Un DWG con racks adjunto como xref: sus racks no se editan ni bloquean los del dibujo anfitrion |

**No se fabrican para el Owner** divergencia, ilegibilidad, identidad ambigua, major futuro, `Kind` en blanco ni
payloads no colocados ilegibles (regla de I-47 y V5-R07 de I-48). Los cubren T-STO, T-AUT y T-MUT.

### 12.9 Trazabilidad V1 → V2

| V1 | V2 | Disposicion |
|---|---|---|
| T-01 | T-STO-01 | igual |
| T-02 | T-STO-20 | ampliado |
| T-03 | T-STO-03 | precisado (forma exacta) |
| T-04 | T-STO-05 | igual |
| T-05 | T-STO-06 | igual |
| T-06 | T-STO-07 | igual |
| T-07 | T-STO-04, T-STO-08 | dividido |
| T-08 | T-STO-10, T-STO-15 | precisado |
| T-09 | T-STO-14 | precisado (resultado distinto y precedencia) |
| T-10 | T-STO-17 | igual |
| T-11 | T-STO-19 | **sin descarte** |
| T-12 | T-STO-20 | fusionado |
| T-13 | T-CHR-01, T-ENV-01 | caracterizacion antes |
| T-14 | T-ENV-02 | fusionado |
| T-15 | T-ENV-02..05 | **acotado** |
| T-16 | T-ENV-08 | igual |
| T-17 | T-ENV-08 | fusionado |
| T-18 | T-ENV-09 | igual |
| T-19 | T-ENV-10 | igual |
| T-20 | T-CPY-01 | **reformulado**: store de produccion, no restamp |
| T-21 | T-GRD-03 | **estructural**, convive con G-R5 |
| T-22 | T-ENV-11 | igual |
| T-23..T-30 | T-AUT-01..15 | **pertenencia sin filtro de colocacion**; `Kind` en blanco; xref; minor |
| T-31..T-35 | T-STO-16, T-STO-18, T-MUT-01..10, T-AUT-14 | **sin descarte**; unificacion segura; revalidacion |
| T-36 | T-AUT-16 | ampliado |
| T-37..T-44 | T-GRD-04..08, T-PRJ-01..04 | ampliado |
| U-01..U-10 | U-01..U-10 | **sin descarte**; solo lectura; unificacion segura |
| OV-01..OV-12 | OV-01..OV-12 | OV-05, OV-08 y OV-11 precisados |
| — | T-STO-09, -11, -12, -13, -16; T-ENV-03..07, -12; T-AUT-02, -05..11, -13; T-MUT-06, -07; T-GRD-01, -02, -05, -06; OV-13; OV-14 | **nuevos** |

## 13. Ramas paralelas (medidas en G2C)

Diff medido desde el SHA de la revision de V1; tambien se reviso el nombre de todos los archivos frente a `main`.

| Rama | SHA actual (completo) | En la revision V1 | Diff productivo relevante | Costuras compartidas | Conflictos | Supuestos de I-54 invalidados |
|---|---|---|---|---|---|---|
| I-49 `architecture/motor-expresiones-parametricas` | `ccf21c69d9aa6e8d9b622e6389fb61f430d53f03` | `4df9480` | **Ninguno**: +1 commit con `I-49-proposal-v4.md` (+2691 lineas, solo docs) | Semantica: sigue reservando `Rack`/`Project`, sus namespaces y el ambito `Rack` para ID20 (`V4:628-633,1737-1745`). Profundidad JSON: el diseño viaja como string en el sobre (`V4:470-484`); el miembro de I-54 es **hermano** de `Design`, no ancestro, y los presupuestos no se suman. Censos 33/29 intactos (`V4:1447`) | Documental: fila de ROADMAP y numeracion de ADR | Ninguno |
| I-50 `feature/cotas-independientes-por-vista` | `6cd2970c36a906cb9e784797008bed5e8111e433` | `8ceb3a7` | +3 commits G3 (A), (B) y (C): `RackSelectiveWindow`, `RackDynamicSystemWindow` y `RackPushBackSystemWindow` (`.xaml` y `.xaml.cs`), mas pruebas de UI (ventanas nuevas de cotas, firmas de editores y censos de `x:Name`). Sin sobre, compositor (sigue con 7 llamadas a `Compose` y un solo `new RackEmbedDocument` en `src/`), restamp, cloner, autoridad de sobre, biblioteca ni censos de comandos o ventanas | Ninguna productiva: I-54 no toca editores. Su `DimensionViewsRestampTests.cs` (en la rama desde `94220fb`) construye `new RackEmbedDocument` en `tests/`, de ahi el alcance `src/` de T-GRD-01. ADR-0035 (`:97-102`) sigue diciendo que `RackEmbedComposer` descarta campos nuevos: el ADR de I-54 lo acota | Ninguno mientras I-54 siga fuera de los editores | Ninguno; precision de alcance de T-GRD-01 |
| I-52 `feature/rackmirror-espejo-semantico` | `0fc7032bf15d03e7d478bbd9350f156708621c9d` | `339b3ab` | **Ninguno**: +1 commit G2 con `I-52-proposal-v1.md` (+1011), `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md` (`propuesto`), `docs/adr/README.md` y `docs/automation/decisions/I-52.md` | **V2-6/V2-7**: el espejo compone `Compose(sobreFuente, kind, idFuente, nombreFuente, View, σ', diseñoReflejado)` y re-estampa con `RestampEnvelope` sin modificar `RackEnvelopeRestamp.cs` (ID-5, ID-6, §10.1: `:316-317,698-702`) → **cumple D-21** y T-GRD-03 sigue verde. Exige que `Compose` herede el miembro de I-54 (X-02 `:537`, §10.4 `:735-737`) y prevé T-M20 (`:835`). «Schema sin cambio» (`:13`) | Textual: censo de comandos 33 → 34 (PDC-8 `:76`), llamada nueva a `Compose` (T-GRD-02: 7 → 8), `RackCommandReference.cs`; documental: ADR-0036 en su rama | Ninguno; obligacion de D-21 ya reflejada en su diseño |
| I-53 `feature/cabeceras-configurables-multidestino` | `f38362d32a737f62d69a229854dc5c1812adf063` | `c8476cc` | **Ninguno**: +1 commit con `I-53-proposal-v1.md` (+957) y su contrato | Ninguna: no añade datos persistidos y declara «sin cruce» con I-54 (`I-53-proposal-v1.md:649-650`); su UI va en las ventanas de editores existentes | Documental | Ninguno |

Medido tras un `git fetch --all --prune` inmediatamente anterior a publicar V2. Entre el preflight de G2C y la
publicacion avanzaron I-50 (`ed50cbd` → `6cd2970`) e I-52 (`339b3ab` → `0fc7032`); los dos avances se midieron y
estan en esta tabla.

## 14. Gates propuestos (no autorizados · V2-21)

| Gate | Contenido | Evidencia |
|---|---|---|
| G2B | Architect Review de V1 | **CERRADO**: AGREED WITH CHANGES @ `7c197af` |
| G2C | Esta Proposal V2 | commit documental y CI del SHA exacto |
| G2D | Revision exact-SHA de V2 por Coordinador y Arquitecto | veredictos sobre el mismo SHA |
| G2-FREEZE | Consenso sobre el mismo SHA; contrato actualizado; **ADR `propuesto`** con el alcance de §15; F-01..F-13 y la mejora de D-11.6 en `ideas-futuras.md`; aprobacion del Owner, informado de R-12 | commit documental y CI |
| G3 | Documento, store, resultados, guarda y mutaciones puras | T-STO-01..20, T-MUT-01..06, T-MUT-09: rojo → verde |
| G4 | **Primero la caracterizacion de BASE** (T-CHR-01..03), verde sobre el codigo de `BASE_SHA` y commiteada; **despues**, produccion: miembro del sobre, herencia en `Compose`, `WithCustomProperties` y restricciones A..G; **guardas** T-GRD-01..03 con rojo demostrado | T-CHR, T-ENV, T-CPY-01 |
| G5 | Pertenencia (indeterminada, `Kind` en blanco, xref), autoridad, preflight, commit sobre proyeccion fresca y workspace | T-AUT-01..16, T-MUT-07, -08, -10 |
| G6 | Plugin: datos del NOD, ejecutores de Proyecto, Rack y unificar, y guardas T-GRD-04..07; build del Plugin. **Sin smoke fisico**: el comando aun no existe | T-PRJ-01..04 |
| G7 | Nombre del comando decidido por el Owner; ventana, comando, **censo real verificado** y actualizado, y ayuda | U-01..10, T-GRD-08 |
| G8 | Candidato: Core y UI completas en local, CI 4/4 sobre el SHA exacto, Debug de UI y Plugin | AGENTS.md |
| G9 | **Toda** la validacion fisica, en AutoCAD 2025 | OV-01..14 |
| G10 | Integracion (WORKFLOW 4.5) | CI del merge y cobertura |

## 15. ADR Candidate Scope

> **No es el ADR.** No tiene numero ni texto. Es el material que el Consensus Freeze debe congelar en un ADR
> `propuesto` antes de G3, con este titulo de trabajo: **«Contrato de persistencia y autoridad de Custom Properties»**.
> Cumple los criterios 1, 2 y 3 de `docs/adr/README.md:19-26`.

**Material de freeze.**

1. **Modelo e identidad.**
   - Registros `{ Id, Name, Value }`, con `ExtensionData` en la raiz y por entrada.
   - `Id` GUID en forma `D` exacta y escrita en minusculas; igualdad por valor; ids duplicados =
     `AmbiguousIdentity`.
   - Identidad local al alcance.
   - El nombre solo presenta: nunca identidad, busqueda ni agregacion.
   - Regla de evolucion normativa (D-04.3..5) y su confrontacion con ADR-0034 §3.
2. **Contenedor de Proyecto.** Xrecord directo `RACKCAD_CUSTOM_PROPERTIES` en el NOD, con la clave congelada;
   lectura tri-estado; version primero; independiente de `RACKCAD_PROJECT` en ambos sentidos.
3. **Contenedor de Rack.** Miembro `CustomProperties` del sobre, de tipo `JsonElement?`, omitido cuando es
   nulo; `Compose` lo hereda y `WithCustomProperties` escribe.
4. **Contrato de aislamiento de `JsonElement?`.** La afirmacion acotada de D-08.4 y las restricciones A..G. El
   ADR fija que **existe** una cota de profundidad de escritura muy por debajo del maximo de `System.Text.Json`
   (64), y que un escritor no la supera; su valor numerico queda fuera del ADR.
5. **Sin promocion de major del sobre**, con la matriz verificada de D-08.5.
6. **Invariante de preservacion del sobre** (D-21): `Compose(source)` o restamp del mismo objeto;
   `Compose(null)` solo para rack nuevo e importacion.
7. **Autoridad entre hermanas por `RackId`.** Pertenencia por `Id`; igualdad canonica; ausente ≡ vacio con
   cualquier minor; `Kind` en blanco = `MixedKind`; nunca se elige hermana.
8. **Pertenencia indeterminada.** Todo payload RackCad no interpretable, colocado o no, bloquea las escrituras
   de Rack. Un `Id` en blanco no es miembro; las dependientes de xref quedan excluidas.
9. **Escritura atomica.** Una transaccion del llamador, a todos los miembros o a ninguno. Decide Application
   sobre la lectura fresca. Sin redefinir, importar, purgar ni regenerar.
10. **`UNKNOWN != EMPTY`**, en los dos contenedores.
11. **Sin recuperacion destructiva en V1.** Cualquier recuperacion futura exige una decision explicita del Owner.
12. **Matriz de compatibilidad** (§9).
13. **Fronteras de V1**: biblioteca, dibujo, expresiones, plantillas, Drawing-level, vista y agregacion.
14. **Sin sintaxis de I-49**: no reclama `Rack` ni `Project` de ID20.
15. **Referencias futuras solo por identidad estable.**

**Relacion con otros ADR.** Aplica ADR-0009, ADR-0010 y ADR-0034 sin reemplazarlos. Acota, **sin reabrirla**, la
frase de ADR-0035 sobre `RackEmbedComposer` (aceptado en la rama de I-50, `:97-102`): vale para campos declarados
que `Compose` no hereda, y `CustomProperties` si se hereda.

Es compatible con ADR-0036 de I-52 (`propuesto` en su rama @ `0fc7032`). Ese ADR declara intactos los portadores
de las iniciativas integradas, y su orden reflejar → `Compose` → restamp cumple el invariante de preservacion. El
numero del ADR de I-54 se asigna al redactarlo; hoy 0035 y 0036 estan tomados en ramas.

**Excluido del ADR.** La UI, los nombres de comando, los limites numericos (50, 80, 1000 y el valor de la cota
de profundidad), los gates y el tuning.

## 16. Preguntas al Owner reclasificadas (V2-22)

Ninguna bloquea el consenso.

| # | Pregunta de V1 | Clasificacion V2 | Resolucion o motivo | Gate limite |
|---|---|---|---|---|
| OQ-01 | ¿Alguna propiedad de rack viaja con la biblioteca? | **DEFER** | Biblioteca fuera de V1 como frontera (D-12), con camino aditivo | Ninguno en I-54; la iniciativa futura de biblioteca |
| OQ-02 | Limites 50 / 80 / 1000 | **ARCHITECTURE** + **DEFER** | La **existencia** de limites, solo al escribir y sin ser reglas de formato, es arquitectura resuelta (D-05). Los **numeros** son tuning y no bloquean el schema | Valores por defecto en G3; ajuste del Owner a mas tardar en G9 |
| OQ-03 | Nombre y alias del comando, boton de menu | **DEFER** | Nombres de trabajo sin congelar (D-18.1) | **Antes de G7** |
| OQ-04 | ¿Se copian todos los valores al duplicar? | **DEFER** | V1 copia todo (D-11.4, ADR-0034 §12); una politica por propiedad se difiere | Iniciativa futura |
| OQ-05 | ¿Hay builds anteriores a I-11? | **ARCHITECTURE** | Ya no es pregunta: promover no protege a esos builds (D-08.5). Queda como riesgo residual R-12 | — (informado en el freeze) |
| OQ-06 | ¿Reordenar entra en V1? | **DEFER** | El orden ya se persiste y reordenar es aditivo | Iniciativa futura |
| OQ-07 | ¿ID24 y ID20 comparten espacio de nombres? | **DEFER a ID20** | V2 no reclama sintaxis ni namespace (D-16) | ID20 |
| OQ-08 | ¿ID24 es exactamente metadatos de Proyecto y Rack? | **RESOLVED** | Lo fijan el contrato y el objetivo del Coordinador; D-03.2 precisa Proyecto frente a Drawing-level | — |
| OQ-09 | ¿Editar desde los editores de sistema en V1? | **DEFER** | V1 usa UI separada; el workspace es reutilizable | Iniciativa futura |

## 17. Matriz de reconciliacion V2-1..V2-22

| Cambio del Arquitecto | Decision V1 afectada | Seccion V2 | Disposicion | Notas |
|---|---|---|---|---|
| V2-1 Sin descarte destructivo | D-09.8, D-10, D-13, INV-06, T-11, T-35, U, OV, gates | D-09.11, D-10, D-13, INV-06, N-11, T-STO-19, U-03, G6 | **INCORPORATED** | Proyecto y Rack; no queda ninguna API ni control de descarte |
| V2-2 Limitar unificacion | D-09.7, D-10, D-18 | D-09.10, D-22.5, INV-18, T-AUT-14, T-MUT-10, U-04 | **INCORPORATED** | Precision PR-07: solo se escriben los miembros distintos del origen |
| V2-3 Pertenencia indeterminada | D-09.2, RP-02, T-30 | D-09.3, INV-12, RP-01, T-AUT-06..08 | **INCORPORATED** | Cita FAMILY B; B y C rechazadas; no se reutiliza la proyeccion de variables |
| V2-4 Ausente ≡ vacio | D-09.5 | D-09.7, T-AUT-02, T-AUT-05 | **INCORPORATED** | **PR-01**: el minor no participa en la igualdad (§21) |
| V2-5 Aislamiento real | D-08.1, INV-07, O-05, T-15 | D-08.4, D-05.7, INV-07, INV-20, O-05, T-ENV-02..07, T-STO-08..13 | **INCORPORATED** | A..G con prueba cada una; cota de profundidad = 16 (PR-03) |
| V2-6 Invariante de preservacion | D-08.3, D-11, RP-04 | D-21, INV-09, T-GRD-01, T-GRD-02, RP-04 | **INCORPORATED** | Guarda limitada a `src/` (P-25); censo de `Compose` (PR-08); obligacion de I-52 |
| V2-7 Contrato de prueba del restamp | D-11 capas 1..4, T-20, T-21 | D-11.5, T-CPY-01, T-GRD-03, OV-05 | **INCORPORATED** | G-R5 no se re-apunta; extraccion registrada con disparador |
| V2-8 Sin promocion | D-08.5, OQ-05, R-12 | D-08.5, OQ-05, R-12 | **INCORPORATED** | Matriz con [X] |
| V2-9 Regla de evolucion | D-04 | D-04.3..5, D-01.4, §15.1 | **INCORPORATED** | Confrontacion explicita con ADR-0034 §3 |
| V2-10 Id | D-02 | D-02.1..6, T-STO-14, T-STO-15 | **INCORPORATED** | Forma textual antes de `TryParseExact` [X P-19] |
| V2-11 Nombre NFC | D-05 | D-05.1..3, T-STO-16 | **INCORPORATED** | Nombre = presentacion |
| V2-12 Sin agregacion por nombre | D-03 | D-03.4, INV-19, N-12 | **INCORPORATED** | Tambien por id entre racks |
| V2-13 Independencia de Proyecto | D-07 | D-07.1, D-07.2, D-07.6, D-07.7, INV-17, T-PRJ-02, T-PRJ-03 | **INCORPORATED** | Troceado duplicado mantenido; F-13 como deuda |
| V2-14 Ejecutor de Rack | D-09.6, flujos | D-22, D-09.1, T-GRD-04, T-MUT-07, T-MUT-08, OV-08 | **INCORPORATED** | xref: PR-05; UNDO no afirmado |
| V2-15 Mutaciones que preservan | D-10 | D-10.1..3, INV-03, T-MUT-02..06, U-07 | **INCORPORATED** | — |
| V2-16 `Kind` en blanco | (sin definir) | D-09.5, T-AUT-09, T-AUT-10, RP-06 | **INCORPORATED** | **Opcion A** con evidencia historica (P-23, P-24); un kind desconocido pero comun no bloquea (PR-12) |
| V2-17 Solo hooks | D-15, D-16, D-17 | D-15, D-16, D-17, N-14 | **INCORPORATED** | Se retiran los ids de plantilla y los destinos concretos |
| V2-18 Biblioteca | D-12 | D-12, INV-15, OV-11 | **INCORPORATED** | UX documentada |
| V2-19 Alcance Proyecto | §0 vocabulario, D-03 | §1, D-03.2, N-13 | **INCORPORATED** | — |
| V2-20 UI | D-18, U-01..10 | D-18, U-01..10 | **INCORPORATED** | Nombres de trabajo; censo verificado antes de G7 |
| V2-21 Gates | §12 gates | §14 | **INCORPORATED** | G2D explicito; caracterizacion en G4; fisico solo en G9 |
| V2-22 Preguntas al Owner | §13 | §16 | **INCORPORATED** | Con gate limite |

**Contradicciones entre cambios del Arquitecto: ninguna material.** Se revisaron los pares V2-2/V2-15,
V2-3/V2-2, V2-14/V2-6, V2-5.G/V2-5.F, V2-1/V2-15, V2-16/V2-3 y V2-18/V2-6. La unica tension es **interna de
V2-4**: la lista de elementos comparados incluia `SchemaVersion`, y a la vez se pide ausente ≡ vacio con
cualquier minor. Se resuelve con la unica lectura transitiva (PR-01), declarada en §21 y no elegida en silencio.

## 18. Matriz de hallazgos AR-54-01..18

| Hallazgo | Severidad | RD | Disposicion V2 | Seccion V2 | Estado |
|---|---|---|---|---|---|
| AR-54-01 Descarte y unificacion destructivos | MATERIAL | RD-13, RD-19 | Descarte eliminado; unificacion solo segura | D-09.10, D-09.11, D-10, D-13, INV-06, INV-18 | **CLOSED** |
| AR-54-02 Filtro «solo colocadas» | MATERIAL | RD-11 | Cualquier payload no interpretable bloquea las escrituras de Rack | D-09.3, INV-12 | **CLOSED** |
| AR-54-03 Ausente ≡ vacio con version actual | MATERIAL | RD-10 | Igualdad canonica con cualquier minor | D-09.7 | **CLOSED** |
| AR-54-04 Sobreafirmacion del aislamiento | MATERIAL | RD-07, RD-17 | Afirmacion acotada y A..G | D-08.4, INV-07, INV-20 | **CLOSED** |
| AR-54-05 Sin invariante ni guarda de preservacion | MATERIAL | RD-09, RD-15 | Invariante y tres guardas con rojo; obligacion de I-52 | D-21, INV-09, D-19 | **CLOSED** |
| AR-54-06 Justificacion de no promover | MINOR | RD-08 | Matriz verificada; OQ-05 → riesgo residual | D-08.5, §16 | **CLOSED** |
| AR-54-07 ADR-0034 §3 y regla de evolucion | MINOR | RD-01, RD-02 | Tabla normativa y confrontacion | D-04 | **CLOSED** |
| AR-54-08 `TryParseExact` y NFC | MINOR | RD-03, RD-04 | Forma textual exacta; NFC | D-02, D-05 | **CLOSED** |
| AR-54-09 Capa 2 no prueba el restamp; convivencia con G-R5 | MINOR | RD-15 | Contrato de prueba honesto; guarda estructural nueva | D-11.5 | **CLOSED** |
| AR-54-10 Barrido, capas, xref y revalidacion | MINOR | RD-12, RD-19 | Ejecutor definido | D-22, D-09.1, D-09.4 | **CLOSED** |
| AR-54-11 D-17 congela plantillas | MINOR | RD-18 | Solo restricciones | D-17 | **CLOSED** |
| AR-54-12 Proyecto frente a Drawing-level | MINOR | alcance | Definicion y no-objetivo | §1, D-03.2, N-13 | **CLOSED** |
| AR-54-13 `ExtensionData` de entrada y workspace | MINOR | RD-14, RD-19 | Conservacion y resumenes por vista | D-10, INV-03, INV-11 | **CLOSED** |
| AR-54-14 `Kind` en blanco | MINOR | RD-10 | Opcion A con evidencia | D-09.5 | **CLOSED** |
| AR-54-15 Agregacion implicita por nombre | MINOR | RD-05 | Sin clave de agregacion | D-03.4, INV-19 | **CLOSED** |
| AR-54-16 Independencia de `RACKCAD_PROJECT` | MINOR | RD-06 | Ambos sentidos; clave congelada | D-07.1, D-07.6, INV-17 | **CLOSED** |
| AR-54-17 Caracterizacion y smoke de G6 | MINOR | RD-20 | Caracterizacion al abrir G4; fisico solo en G9 | §14 | **CLOSED** |
| AR-54-18 Biblioteca como clasificacion | MINOR | RD-16 | Frontera de V1 | D-12 | **CLOSED** |

«CLOSED» es la disposicion del ejecutor en V2. La **re-verificacion** corresponde a la revision exact-SHA, y los
cinco MATERIAL deben re-verificarse expresamente (paquete §8).

## 19. RD-01..RD-20 en V2

| RD | V1 (Arquitecto) | V2 | Que cambio desde V1 | Seccion |
|---|---|---|---|---|
| RD-01 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | B con texto; regla de evolucion normativa y confrontacion con ADR-0034 §3 | §5, D-04 |
| RD-02 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Contenedor ausente o miembro nulo = vacio; documento sin `Entries` = ilegible; escritores 1.x emiten siempre `SchemaVersion` y `Entries` | D-01, D-08.2 |
| RD-03 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Forma `D` exacta sin espacios; minusculas; igualdad por `Guid`; `AmbiguousIdentity` distinto | D-02 |
| RD-04 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | NFC antes de recortar, validar y comparar; limites solo de escritura sobre lo introducido; nombre solo presenta | D-05 |
| RD-05 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Entradas autodescriptivas sin clave de agregacion entre racks | D-03 |
| RD-06 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Clave propia congelada; troceado duplicado; independencia en ambos sentidos | D-07 |
| RD-07 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | `JsonElement?` con aislamiento acotado y restricciones A..G | D-08.4 |
| RD-08 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Sin promocion, justificada por la matriz verificada | D-08.5 |
| RD-09 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Herencia en `Compose` e invariante de preservacion con guardas | D-08.3, D-21 |
| RD-10 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Igualdad canonica; ausente ≡ vacio con cualquier minor; `Kind` en blanco = `MixedKind` | D-09.5, D-09.7 |
| RD-11 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Bloqueo por cualquier payload no interpretable, colocado o no; `Id` en blanco y xref no bloquean | D-09.3, D-09.4 |
| RD-12 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Una transaccion con `ScanEnvelopes`; Application decide sobre la lectura fresca; xref rechazadas; UNDO observado | D-22 |
| RD-13 | **DISAGREE** | **ACCEPTED WITH RECONCILIATION** — **reformulada** | **Sin descarte destructivo**; unificacion **solo** con divergencia segura entre miembros legibles o ausentes, sin sobrescribir contenido desconocido | D-09.10, D-09.11, D-13 |
| RD-14 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Vaciar conserva el contenedor; renombrar y cambiar valor conservan el `ExtensionData` de la entrada | D-10 |
| RD-15 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Sin extraccion; propiedad del store, guarda estructural junto a G-R5 y OV; extraccion con disparador | D-11 |
| RD-16 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Frontera de V1, no clasificacion; UX documentada | D-12 |
| RD-17 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Aislamiento de consumidores dentro del alcance de D-08.4 | D-13, INV-07 |
| RD-18 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Puntos de extension reducidos a restricciones | D-15..D-17 |
| RD-19 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | Sin descarte; unificacion segura; solo lectura; intents por id; nombres de comando sin congelar (OQ-03 DEFER antes de G7) | D-18 |
| RD-20 | AGREE WITH CHANGE | **ACCEPTED WITH RECONCILIATION** | ADR obligatorio con alcance candidato; caracterizacion en G4; fisico solo en G9 | D-20, §14, §15 |

**DEFERRED a nivel de RD: ninguna. OPEN DISAGREEMENT: ninguno.**

## 20. Cambios que V2 no hace

- No redacta ni numera el ADR (§15).
- No reclasifica por su cuenta nada que el Arquitecto no pidio, salvo las precisiones de §21.
- No toca produccion, pruebas productivas, `docs/HANDOFF.md` ni `docs/ROADMAP.md`.
- No reescribe el Discovery: solo añade el addendum factual §2.6.

## 21. Precisiones del ejecutor y desacuerdos abiertos

**OPEN DISAGREEMENT = NINGUNO.** Estas precisiones concretan los cambios vinculantes sin contradecirlos. Se
declaran para que la revision exact-SHA las ataque expresamente:

| # | Precision | Cambio que concreta | Por que |
|---|---|---|---|
| PR-01 | En la igualdad canonica, `SchemaVersion` participa **solo por su major**; el minor no participa | V2-4 | Si participase, `{ausente, vacio@1.0, vacio@1.3}` seria intransitivo y reapareceria la divergencia falsa de AR-54-03. Bajo V2-9, un minor sin campos adicionales no transporta dato, y el `ExtensionData` si se compara |
| PR-02 | `MixedKind` para `Kind` en blanco, con un mensaje que remite a `RACKEDITAR` **sin prometer reparacion** | V2-16 | Evidencia P-23 y P-24: la normalizacion por `RACKEDITAR` es [L] y no existe en Push Back ni Cantilever |
| PR-03 | Cota de profundidad de escritura = 16; la lectura es tolerante | V2-5.B | Queda claramente por debajo de 63; V1 escribe profundidad 3; la lectura tolerante no destruye nada |
| PR-04 | Los limites de escritura validan lo que la operacion introduce | V2-14 | Revalidar «limites» sobre entradas no tocadas bloquearia datos de un build con otros limites |
| PR-05 | Las definiciones `IsDependent` no son miembros, no bloquean y nunca se escriben | V2-14 | `ScanEnvelopes` solo omite `IsFromExternalReference` [P-22]; un dependiente es de otro DWG. Su comportamiento en AutoCAD es [I] → OV-14 |
| PR-06 | Nombres de miembro repetidos dentro del documento → `PresentButUnreadable`; en la clave del sobre, residual en el que gana la ultima | V2-5 | [X P-20]: en el documento es detectable; en el sobre lo resuelve `System.Text.Json` antes que el store |
| PR-07 | Unificar escribe solo los miembros canonicamente distintos del origen | V2-2 | Evita degradar el minor de miembros iguales al origen (PR-01) |
| PR-08 | Censo de llamadas a `Compose` (T-GRD-02) | V2-6 | Da un rojo concreto cuando aparece un camino nuevo como el espejo de I-52; no ve a los llamadores de los constructores de payload (limite declarado en D-21) |
| PR-09 | La colision con `ExtensionData` se rechaza en el compositor como error de programacion | V2-5.F | El store nunca la produce al leer [X P-17]; lanzar ahi no viola V2-5.G, que se refiere a contenido externo |
| PR-10 | Version de escritura con `Single` = minor mayor entre miembros | V2-4, V2-9 | Mismo contenido, sin degradar; es la regla de `ResolveWriteVersion` |
| PR-11 | `SchemaVersion` de la coleccion con forma exacta `major.minor` | V2-9 | Precisa la «version estricta» de V1 D-07 |
| PR-12 | Un `Kind` no vacio, desconocido pero comun a todos los miembros, no bloquea | V2-16 | I-54 no interpreta el diseño; un kind nuevo es un sobre valido de un build posterior |
| PR-13 | Unificar exige que la coleccion origen fresca sea la mostrada | V2-2 | La confirmacion del usuario se refiere a ese contenido |

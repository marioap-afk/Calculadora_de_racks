# I-54 — Paquete de revision exact-SHA: Proposal V2

> Paquete **autonomo** para revisar [I-54-proposal-v2.md](I-54-proposal-v2.md) sin la conversacion que la
> produjo. No autoriza implementar ni sustituye a la Proposal: la resume y dice como atacarla. La version de este
> paquete que servia a la revision de V1 vive en el commit `7c197af`.

## 0. Que se pide y que no

**Se pide** una revision **exact-SHA** de la Proposal V2, por el Coordinador y por el Arquitecto, cada uno con su
veredicto, sobre el **mismo** SHA (§1). En particular:

1. **Re-verificar los cinco hallazgos MATERIAL** de la revision de V1 (AR-54-01..05) contra el texto de V2 y contra
   el codigo de `BASE_SHA`. Que V2 los marque `CLOSED` no los cierra: los cierra esta revision.
2. Comprobar que V2 incorpora **todos** los cambios vinculantes V2-1..V2-22 (Proposal §17) y traza AR-54-06..18
   (Proposal §18).
3. Atacar las **precisiones del ejecutor** PR-01..PR-13 (Proposal §21), que concretan cambios vinculantes y
   todavia no tienen veredicto.
4. Confirmar o rechazar la tabla final RD-01..RD-20 (Proposal §19) y el alcance candidato del ADR (Proposal §15).

**No se pide**:
- implementar, estimar, redactar el ADR o editar la Proposal (una V3, si hace falta, la produce el ejecutor);
- tocar `docs/HANDOFF.md` o `docs/ROADMAP.md`;
- revisar el alcance de I-49, I-50, I-52 o I-53;
- **reabrir puntos ya cerrados en la revision de V1 que V2 no cambia materialmente** (§3.2).

## 1. Identificacion exacta

```text
Repositorio    = marioap-afk/Calculadora_de_racks
Rama           = architecture/propiedades-personalizadas
BASE_SHA       = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main al reclamar y al publicar V2)
CLAIM_SHA      = 143490d8ecfb1c5f3011cf752c5cfcd11af13784   (Claim-Id d4b871e9-8a5d-4e67-bc11-a8f3c023788e)
BOOTSTRAP_SHA  = f908b2f4ba508bf365e55adbda78cd4eac505295
G1_SHA         = 195964b00006d2be74eefb8b6ae4f5f46de1cfc9   (Discovery)
PROPOSAL_V1    = 7c197af81b91df88366c873eddf9e11ddc5e87bb   (historica)
REVIEW_V1      = Architect Review — I-54 Proposal V1 @ 7c197af = AGREED WITH CHANGES
                 0 BLOCKER · 5 MATERIAL · 13 MINOR · cambios vinculantes V2-1..V2-22
PROPOSAL_V2    = el commit que introduce docs/initiatives/I-54-proposal-v2.md   ← SHA REVISADO
```

Un documento no puede contener el SHA del commit que lo crea, asi que el SHA revisado se fija **antes** de leer:

```bash
git fetch origin
git log -1 --format=%H origin/architecture/propiedades-personalizadas -- docs/initiatives/I-54-proposal-v2.md
```

El veredicto vale **solo** para ese SHA. Cualquier cambio de la Proposal es otro SHA y otra revision. Este paquete
apunta **exclusivamente** a la V2; la V1 queda como historia.

## 2. Contexto minimo

- **RackCad**: plugin de AutoCAD 2025 (.NET 8), con capas `Domain ← Application ← UI ← Plugin`. Solo el Plugin
  toca AutoCAD y **ninguna suite de pruebas carga el Plugin** (ADR-0003).
- **Un rack** = una o varias **definiciones** de bloque (una por vista) que comparten un `RackId`. Cada una lleva
  un **sobre** JSON (`RackEmbedDocument`) en un Xrecord `RACKCAD_SELECTIVE` de su diccionario de extension
  (ADR-0009, ADR-0010).
- **Nivel dibujo**: desde I-47 existe **un** Xrecord directo en el NOD, `RACKCAD_PROJECT`, con el registro de
  variables (ADR-0034). La doctrina es `UNKNOWN/UNREADABLE ≠ ABSENT` (ADR-0034 §8).
- **ID24**, segun el contrato y el objetivo del Coordinador (su texto **no esta versionado**): propiedades
  personalizadas **literales** de alcance Proyecto y Rack, que admitan evolucion sin tocar clases de producto, y
  sin formulas, expresiones, dibujo, plantillas, Drawing-level ni View-level.
- **La propuesta en una frase**:
  - registros `{ Id GUID, Name, Value texto }` con `ExtensionData`;
  - Proyecto en un Xrecord nuevo `RACKCAD_CUSTOM_PROPERTIES`;
  - Rack en un miembro `CustomProperties: JsonElement?` del sobre, heredado por `Compose`;
  - una autoridad propia por `RackId` que nunca elige hermana;
  - ninguna recuperacion destructiva;
  - una UI separada.

## 3. Que cambio desde la revision de V1

### 3.1 Resumen de la reconciliacion

La tabla completa esta en la Proposal §0. Lo esencial:

| # | V1 | V2 | Cierra |
|---|---|---|---|
| 1 | «Descartar las propiedades ilegibles» | No existe; los estados no escribibles son solo lectura | AR-54-01 |
| 2 | Unificar tambien sobre ilegibles y contenido desconocido | Solo divergencia entre miembros **todos** legibles o ausentes, sin sobrescribir `ExtensionData` ni un minor mayor | AR-54-01 |
| 3 | Pertenencia indeterminada solo por definiciones **colocadas** | Por **cualquier** payload no interpretable, colocado o no | AR-54-02 |
| 4 | Ausente ≡ vacio «con la version actual» | Con **cualquier** minor; el minor no participa en la igualdad (PR-01) | AR-54-03 |
| 5 | «Ningun contenido puede volver ilegible el sobre» | Afirmacion acotada y restricciones A..G, con cota de escritura 16 | AR-54-04 |
| 6 | Preservacion implicita | Invariante D-21 y guardas T-GRD-01..03 con rojo | AR-54-05 |
| 7 | MINOR AR-54-06..18 | Trazados en la Proposal §18 | — |

### 3.2 Lo que no se reabre

Salvo que el revisor encuentre que V2 los cambia **materialmente**, no se piden de nuevo:
- representacion B y solo texto (RD-01);
- contenedor propio de Proyecto sin migrar `RACKCAD_PROJECT` (RD-06);
- `JsonElement?` frente a `string` o a un miembro tipado (RD-07);
- no promover el major del sobre (RD-08);
- escritura sin redibujo (RD-12);
- vaciar sin borrar el contenedor (RD-14);
- sin extraer el restamp en V1 (RD-15);
- biblioteca fuera de V1 (RD-16);
- que haga falta ADR (RD-20).

En todos ellos V2 incorporo el cambio pedido; basta con verificar esa incorporacion.

## 4. Lectura obligatoria, en este orden

1. **Proposal V2**:
   - §0, §17, §18, §19 y §21: que cambio, trazas y precisiones;
   - D-08, D-09, D-21 y D-22: los cinco MATERIAL;
   - D-01, D-02, D-04, D-05, D-07, D-10, D-11 y D-12: los MINOR;
   - §7, §12, §14, §15 y §16.
2. **Discovery**: §2.6 (addendum G2C), §5, §6, §7.4 y §8.
3. **Codigo** en `BASE_SHA` (`git show 46fcac2:<ruta>`):

   | Archivo | Lineas | Para |
   |---|---|---|
   | `src/RackCad.Application/Persistence/RackEmbedDocument.cs` | 33-64, 70-119 | D-08 |
   | `src/RackCad.Application/Persistence/RackEmbedComposer.cs` | 19-36 | D-08.3, D-21 |
   | `src/RackCad.Plugin/RackBlockFinder.cs` | 57-91 | D-09.1, D-09.4, D-22 |
   | `src/RackCad.Plugin/RackCommandSupport.cs` | 109-134 | D-09.1 |
   | `src/RackCad.Application/ProjectVariables/ProjectVariableConsumerDiscovery.cs` | 69-92, 161-216 | D-09.3 |
   | `src/RackCad.Application/ProjectVariables/ProjectVariableScanProjection.cs` | 30-47 | D-09.3, D-09.5 |
   | `src/RackCad.Application/Persistence/RackDuplicationPlan.cs` | 402-425, 510-533 | D-09.5 |
   | `src/RackCad.Plugin/RackPushBackCommands.cs` | 190-203 | D-09.5 |
   | `src/RackCad.Plugin/RackCantileverCommands.cs` | 241-252 | D-09.5 |
   | `src/RackCad.Plugin/RackMenuCommands.cs` | 115-147 | D-09.5 |
   | `src/RackCad.Plugin/RackEnvelopeRestamp.cs` | 35-88 | D-11, D-21 |
   | `src/RackCad.Plugin/ProjectVariableMutationExecutor.cs` | 64-99, 240-288 | D-22 |
   | `src/RackCad.Plugin/ProjectVariablesData.cs` | completo | D-07 |
   | `tests/RackCad.Tests/SelectiveDuplicationFailClosedTests.cs` | 474-526 | T-GRD-03 frente a G-R5 |
   | `tests/RackCad.Tests/ProjectVariableMutationExecutorTests.cs` | 280-330 | D-21, T-GRD-04 |
   | `tests/RackCad.Tests/PersistenceUniformityTests.cs` | 193-217 | T-CPY-01 |

4. **Historia del `Kind`**: `git show --stat 74d935f` y
   `git show 74d935f^:src/RackCad.Application/Persistence/SelectivePalletDesignDocument.cs` (lineas 14-22).
5. **ADR**: [0009](../adr/0009-identidad-guid-embebida-en-dwg.md) `:42-45`;
   [0034](../adr/0034-project-variables-autoridad-drawing-level.md) §3 (`:74-78`), §8, §9, §12 y «Riesgos»
   (`:210-219`); criterios de [adr/README.md](../adr/README.md) `:19-26`.

## 5. Hechos decisivos y como verificarlos

### 5.1 Codigo e historia

| # | Hecho | Ruta : lineas @ `BASE_SHA` | Si fuera falso, cae |
|---|---|---|---|
| F-1 | `Compose` crea un sobre nuevo y del origen solo copia `SchemaVersion` y `ExtensionData` | `RackEmbedComposer.cs:21-33` | D-08.3 |
| F-2 | `RackEmbedStore.Deserialize` devuelve `null` ante `JsonException` o major futuro; las opciones son `PropertyNameCaseInsensitive = true` y sin `DefaultIgnoreCondition` | `RackEmbedDocument.cs:70-119` | D-08 |
| F-3 | `RACKCAD_PROJECT` es Xrecord directo; una entrada que no sea Xrecord se lee como ilegible | `ProjectVariablesData.cs:36,59-68` | D-07 |
| F-4 | `RestampEnvelope` deserializa (`:60`), muta `Id`, `Name` y `Design` y serializa **el mismo** objeto (`:87`) | `RackEnvelopeRestamp.cs:52-88` | D-11, T-GRD-03 |
| F-5 | En `src/`, el unico `new RackEmbedDocument` esta en el compositor; en `tests/` hay ocho archivos que construyen sobres | `git grep -n "new RackEmbedDocument" 46fcac2 -- src tests` | T-GRD-01 |
| F-6 | Las siete llamadas a `RackEmbedComposer.Compose(` de `src/` estan en el Plugin | `git grep -n "RackEmbedComposer.Compose" 46fcac2 -- src` | T-GRD-02 |
| F-7 | `ScanEnvelopes` usa la transaccion del llamador, omite `IsFromExternalReference`, conserva los ilegibles con `Embed = null` y cuenta referencias desde el registro; `FindRackBlocks` abre su propia transaccion y descarta los ilegibles | `RackBlockFinder.cs:57-91`; `RackCommandSupport.cs:109-134` | D-09.1, D-22 |
| F-8 | En Project Variables, toda definicion con datos RackCad no interpretables aborta la operacion, colocada o no | `ProjectVariableConsumerDiscovery.cs:87-91,161-216` | D-09.3 |
| F-9 | La proyeccion de variables trata un `Id` o un `Kind` en blanco como sobre ilegible y filtra por Selectivo | `ProjectVariableScanProjection.cs:37-46` | D-09.3 («no se reutiliza») |
| F-10 | `Kind` en el sobre desde `74d935f` (2026-07-08). Antes, el Selectivo escribia el diseño desnudo bajo la misma clave, sin `Kind` ni `Design` | `git show 74d935f`; `74d935f^:…/SelectivePalletDesignDocument.cs:16-22`; `74d935f^:src/RackCad.Plugin/Systems/RackBlockData.cs:16` | D-09.5 |
| F-11 | `Kind` en blanco: lo tolera `FindRackBlocks` y lo rechazan duplicado, Push Back, Cantilever, lista, inventario y `RACKEDITAR` sobre el propio bloque | `RackCommandSupport.cs:120-127`; `RackDuplicationPlan.cs:412-415`; `RackPushBackCommands.cs:194-203`; `RackCantileverCommands.cs:243-252`; `RackListBuilder.cs:56`; `RackInventarioCommands.cs:53`; `RackMenuCommands.cs:132-146` | D-09.5 |
| F-12 | El ejecutor de variables relee y re-acredita en Application dentro de su transaccion; una guarda fija que compone con el sobre de cada hermana | `ProjectVariableMutationExecutor.cs:250-288`; `ProjectVariableMutationExecutorTests.cs:288-298` | D-22, D-21 |
| F-13 | G-R5 fija la forma del restamp con `PluginSourceCode` | `SelectiveDuplicationFailClosedTests.cs:474-526` | T-GRD-03 |
| F-14 | Ningun proyecto de pruebas referencia el Plugin | `tests/RackCad.Tests/RackCad.Tests.csproj`; `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | D-11.5 |
| F-15 | Censos: 33 `[CommandMethod(` y 29 ventanas (C = 11) | `SelectiveEditorOpenTests.cs:546`; `WindowCensusGuardTests.cs:166-170` | D-18.7 |
| F-16 | ADR-0034 §3 persiste el discriminador de tipo «desde el inicio»; «Riesgos» acepta el bloqueo por payloads futuros o corruptos | `0034-…md:74-78,216-217` | D-04, D-09.3 |

### 5.2 Hechos ejecutados [X] y como reproducirlos

Son de una sonda local de .NET 8.0.29 **fuera del repositorio**. Para reproducirlos hace falta una consola
`net8.0` con:
- una copia literal de `RackEmbedDocument`, `RackEmbedStore`, `SchemaVersionPolicy` y `RackEmbedComposer` de
  `BASE_SHA` («BASE»);
- una copia del sobre de `4b2f4d7^` («pre-I-11»);
- la variante de V2, que es BASE mas este miembro y esta herencia:

```csharp
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public JsonElement? CustomProperties { get; set; }
// Compose: composed.CustomProperties = source?.CustomProperties;
```

El sobre de prueba es
`{"SchemaVersion":"1.0","Kind":"selective","View":"frontal","Section":0,"Id":"…","Name":"Rack A","Design":"{\"d\":1}","CustomProperties":<X>}`.

| # | Caso | Resultado observado | Sostiene |
|---|---|---|---|
| X-1 | `<X>` = objeto, `"texto"`, `12.5`, `[1,2]`, `true` o `false` | sobre legible; al reescribir, el texto crudo es identico | D-08.4 |
| X-2 | `<X>` = `null` | `HasValue = false`; al reescribir el miembro desaparece | D-08.4.E |
| X-3 | Sobre sin el miembro (con `ExtensionData`) y `Compose(null)` | bytes identicos entre BASE y la variante | INV-08 |
| X-4 | `<X>` = `{"SchemaVersion":"1.0","Entries":[}` o texto truncado | `Deserialize` → `null` en BASE y en la variante | D-08.4.A |
| X-5 | `<X>` = 60 o 63 arrays anidados; 64 o 70 | 60 y 63 legibles; 64 y 70 → `null` en ambos | D-08.4.B |
| X-6 | Clave `CustomProperties` dos veces | gana la ultima, en BASE (`ExtensionData`) y en la variante | D-08.4, residual |
| X-7 | Clave `customproperties` | la variante la asigna al miembro y reescribe `CustomProperties`; BASE la guarda con su capitalizacion | D-08.4.F |
| X-8 | BASE lee un sobre con el miembro y hace `Compose(source)` o muta el mismo objeto | el miembro sobrevive exacto | D-08.5 |
| X-9 | pre-I-11 lee y reescribe un sobre con el miembro; lee un sobre `2.0` | pierde el miembro; lo lee como legible | D-08.5 |
| X-10 | BASE lee un sobre `2.0` | `null` | D-08.5 |
| X-11 | Variante sin la asignacion en `Compose` | el miembro se pierde | D-21 |
| X-12 | `CustomProperties = default(JsonElement)`; `Serialize` | `InvalidOperationException` | D-08.4.C |
| X-13 | Elemento de tipo `Null` asignado | escribe `"CustomProperties":null` | D-08.4.D |
| X-14 | Miembro con valor y `ExtensionData["CustomProperties"]` | dos claves emitidas; al releer gana `ExtensionData` | D-08.4.F |
| X-15 | Miembro declarado `string` y JSON con objeto | `JsonException` (el sobre seria `null`) | D-08, descartado |
| X-16 | `Guid.TryParseExact` con `" …guid… "`, `"\t…guid…"`, llaves, formato `N` y `Guid.Empty` | espacios y tabulador → `true`; llaves y `N` → `false`; `Empty` → `true` | D-02 |
| X-17 | «Área» NFC frente a NFD con `OrdinalIgnoreCase`, antes y despues de `Normalize(FormC)` | `false` y `true` | D-05 |
| X-18 | `JsonDocument` con `Entries` y `entries`; entrada con `Value` dos veces | `EnumerateObject` devuelve ambos; `TryGetProperty` devuelve el ultimo | D-01.5 |
| X-19 | BASE lee `{"SchemaVersion":"1.0","Id":"…","Name":"Rack A","PostId":"P1","Bays":[…]}` (Selectivo anterior al sobre) | sobre **legible**: `Kind` y `Design` nulos; `ExtensionData` = `PostId`, `Bays`… | D-09.5 |
| X-20 | `ResolveWriteVersion("1.3","1.0")`; `IsReadable("2.0","1.0")` | `1.3`; `false` | D-09.8, PR-10 |

## 6. Hallazgos de la revision de V1 y su disposicion en V2

| Hallazgo | Sev. | RD | Donde lo cierra V2 | Que re-verificar |
|---|---|---|---|---|
| AR-54-01 | MATERIAL | 13, 19 | D-09.10, D-09.11, D-10, D-13, D-18, INV-06, INV-18 | Que no quede **ningun** camino de escritura sobre un estado no escribible, en ningun alcance, ni en la UI, las pruebas o los gates; y que unificar no pueda sobrescribir contenido desconocido |
| AR-54-02 | MATERIAL | 11 | D-09.3, INV-12 | Que el bloqueo no dependa de la colocacion; que un `Id` en blanco no bloquee; que Proyecto no se afecte; la cita del precedente |
| AR-54-03 | MATERIAL | 10 | D-09.7, T-AUT-02, T-AUT-05 | Que la igualdad sea transitiva y no dependa de la version actual; **PR-01** |
| AR-54-04 | MATERIAL | 7, 17 | D-08.4, INV-07, INV-20 | Que no quede ninguna afirmacion absoluta de aislamiento en V2 y que cada restriccion A..G tenga prueba |
| AR-54-05 | MATERIAL | 9, 15 | D-21, D-19, INV-09 | Que el invariante cubra los caminos futuros, que las guardas tengan rojo y alcance correcto (`src/`), y la obligacion de I-52 |
| AR-54-06..18 | MINOR | varios | Proposal §18 | Traza de cada uno |

## 7. RD-01..RD-20 reconciliadas

Las veinte quedan **ACCEPTED WITH RECONCILIATION** (Proposal §19). RD-13, que en V1 fue **DISAGREE**, queda
reformulada: sin descarte destructivo y con unificacion solo en divergencia segura entre miembros legibles o
ausentes. **DEFERRED: ninguna. OPEN DISAGREEMENT: ninguno.** Preguntas al Owner: ninguna bloquea el consenso
(Proposal §16).

## 8. Preguntas de ataque para V2

Sugerencias, no conclusiones.

### 8.1 Re-verificacion de los cinco MATERIAL

| # | Pregunta | Pista |
|---|---|---|
| A2-01 | ¿Queda en V2 alguna frase, prueba, U, OV o gate que suponga descartar, reparar o sobrescribir un contenedor no escribible? | Buscar `descart`, `repar` y `sobrescrib`, sin distinguir mayusculas, en la V2 |
| A2-02 | ¿Puede la unificacion escribir sobre `ExtensionData`, sobre un minor mayor o con un miembro no escribible, en algun orden de evaluacion? | D-09.8 y D-09.10; T-AUT-14 |
| A2-03 | ¿Hay algun caso en que un payload no interpretable **no** bloquee las escrituras de Rack y la ilegible pudiera ser miembro? | D-09.3 y D-09.4 (dependientes de xref, PR-05) |
| A2-04 | ¿La igualdad canonica sin minor (PR-01) permite alguna igualdad falsa que pierda datos al escribir? | D-09.7, D-09.8 (version de escritura) y D-04.3 |
| A2-05 | ¿Queda alguna afirmacion de aislamiento sin la condicion «sintacticamente valido y dentro de la profundidad»? | D-08.4; INV-07; O-05; §9 |
| A2-06 | ¿Cubren T-GRD-01..03 los caminos que pueden romper el invariante, o hace falta otra defensa? | D-21 («limite declarado»); F-5, F-6 |

### 8.2 Precisiones nuevas

| # | Pregunta | Pista |
|---|---|---|
| A2-07 | ¿`MixedKind` para un `Kind` en blanco es la opcion correcta, dada la evidencia de F-10, F-11 y X-19? ¿El mensaje promete algo que `RACKEDITAR` no hace? | D-09.5; PR-02 |
| A2-08 | ¿Es sensato que un kind desconocido pero comun no bloquee (PR-12)? | D-09.5 |
| A2-09 | ¿Es 16 una cota razonable, y es correcto que la lectura sea tolerante y la escritura rechace (PR-03)? | D-05.7; X-5 |
| A2-10 | ¿Validar los limites solo sobre lo que la operacion introduce (PR-04) cumple V2-14? | D-05.6; D-22.5 |
| A2-11 | ¿Detectar miembros duplicados en el documento (PR-06) puede volver ilegible algo que un build posterior valido escribiria? | D-01.5; X-18 |
| A2-12 | ¿Unificar solo los miembros distintos (PR-07) deja algun miembro con contenido no canonico? | D-09.10 |
| A2-13 | ¿Rechazar en el compositor una colision con `ExtensionData` (PR-09) puede romper algun flujo de `RACKEDITAR`, dado que sus origenes vienen del store? | D-08.4.F; X-7, X-14 |
| A2-14 | ¿El reparto Application/Plugin de D-22 (proyeccion con `RackEmbedDocument` y handles como texto) respeta INV-13, y el Plugin queda sin decisiones? | D-22.2..4; T-GRD-04 |
| A2-15 | ¿La exclusion de dependientes de xref (PR-05) es consistente con que `ScanEnvelopes` solo omita `IsFromExternalReference`? | F-7; OV-14 |

## 9. Ramas paralelas (medidas al publicar V2)

| Rama | SHA | Que cambio desde la revision de V1 | Cruce con I-54 |
|---|---|---|---|
| I-49 `architecture/motor-expresiones-parametricas` | `ccf21c69d9aa6e8d9b622e6389fb61f430d53f03` | Proposal V4, solo docs | Semantico: `Rack`/`Project` reservados para ID20; sin dependencia |
| I-50 `feature/cotas-independientes-por-vista` | `6cd2970c36a906cb9e784797008bed5e8111e433` | G3 (A), (B) y (C): ventanas del Selectivo, Dinamico y Push Back, y pruebas de UI | Ninguno productivo; su prueba `DimensionViewsRestampTests.cs` construye sobres en `tests/` (alcance de T-GRD-01); ADR-0035 `:97-102` a acotar |
| I-52 `feature/rackmirror-espejo-semantico` | `0fc7032bf15d03e7d478bbd9350f156708621c9d` | Proposal V1, ADR-0036 `propuesto` y registro de decisiones; solo docs | El espejo compone con `Compose(sobreFuente, …)` y re-estampa (`I-52-proposal-v1.md:316-317,698-702`), asi que **cumple D-21**. Conflictos: censo de comandos 33 → 34, llamada nueva a `Compose` (T-GRD-02: 7 → 8) y numeracion de ADR |
| I-53 `feature/cabeceras-configurables-multidestino` | `f38362d32a737f62d69a229854dc5c1812adf063` | Proposal V1, solo docs | Ninguno: sin datos persistidos; «sin cruce» |

Si al revisar alguna rama se movio, citar su SHA nuevo y medir solo si invalida decisiones de I-54.

## 10. Formato del veredicto

```text
Architect Review — I-54 Proposal V2 @ <SHA de 40 hex>
Global = AGREED | AGREED WITH CHANGES | NOT AGREED

Re-verificacion MATERIAL
  AR-54-01 = CLOSED | STILL OPEN: <motivo>
  ...
  AR-54-05 = ...

Hallazgos nuevos
  [BLOCKER | MATERIAL | MINOR] <id> — RD-xx / PR-xx — <afirmacion> — <evidencia ruta:linea @ SHA> — <cambio requerido>

Precisiones PR-01..PR-13
  PR-xx = AGREE | AGREE WITH CHANGE: <cambio> | DISAGREE: <motivo>

Decisiones
  RD-01 = AGREE | AGREE WITH CHANGE: <cambio> | DISAGREE: <motivo>
  ...
  RD-20 = ...

ADR Candidate Scope = AGREE | AGREE WITH CHANGE: <cambio>
Required Proposal V3 changes = NONE | <lista numerada vinculante>
```

El Coordinador emite su veredicto con el mismo encabezado («Coordinator Review — I-54 Proposal V2 @ <SHA>»).

| Severidad | Significado |
|---|---|
| **BLOCKER** | Inimplementable tal como esta escrito, o contradice un ADR aceptado o una doctrina vigente |
| **MATERIAL** | Cambia una decision, un invariante o una superficie |
| **MINOR** | Precision de redaccion o de evidencia que no cambia decisiones |

## 11. Reglas de la revision

- La evidencia de codigo se cita sobre `BASE_SHA`. Si `origin/main` avanzo al revisar, se citan ambos SHAs.
- Una afirmacion de la Proposal sin cita verificable es, por si misma, un hallazgo.
- Una guarda de texto no es criterio de aceptacion: el criterio es el comportamiento (leccion de I-48).
- **La revision no desbloquea la implementacion.** Hacen falta `Coordinator = AGREED` y `Architect = AGREED` sobre
  el mismo SHA, el ADR en estado `propuesto` y la aprobacion del Owner.

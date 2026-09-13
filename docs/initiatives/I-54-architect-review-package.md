# I-54 — Paquete de revision de Arquitecto: Proposal V1

> Paquete **autonomo**: contiene lo necesario para revisar sin la conversacion que lo produjo. No autoriza
> implementar ni sustituye a la Proposal: la resume y dice como atacarla.

## 0. Que se pide y que no

**Se pide** una revision **adversarial** de [I-54-proposal-v1.md](I-54-proposal-v1.md) contra el codigo real
en `BASE_SHA`, con veredicto global y veredicto por cada decision **RD-01..RD-20** (§5).

**No se pide**: implementar, estimar, editar la Proposal (el ejecutor produce la V2 si hace falta), tocar
`docs/HANDOFF.md` o `docs/ROADMAP.md`, ni revisar el alcance de I-49, I-50, I-52 o I-53.

## 1. Identificacion exacta

```text
Repositorio    = marioap-afk/Calculadora_de_racks
Rama           = architecture/propiedades-personalizadas
BASE_SHA       = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main al reclamar y al publicar)
CLAIM_SHA      = 143490d8ecfb1c5f3011cf752c5cfcd11af13784   (Claim-Id d4b871e9-8a5d-4e67-bc11-a8f3c023788e)
BOOTSTRAP_SHA  = f908b2f4ba508bf365e55adbda78cd4eac505295
G1_SHA         = 195964b00006d2be74eefb8b6ae4f5f46de1cfc9   (Discovery)
PROPOSAL_V1    = el commit que introduce este archivo y la Proposal
```

El SHA revisado debe fijarse **antes** de leer, porque un documento no puede contener el SHA del commit que
lo crea:

```bash
git fetch origin
git log -1 --format=%H origin/architecture/propiedades-personalizadas -- docs/initiatives/I-54-proposal-v1.md
```

El veredicto vale **solo** para ese SHA. Cualquier cambio de la Proposal es otro SHA y otra revision.

## 2. Contexto minimo

- **RackCad**: plugin de AutoCAD 2025 (.NET 8). Capas `Domain ← Application ← UI ← Plugin`; solo el Plugin
  toca AutoCAD y **ninguna suite de pruebas carga el Plugin** (ADR-0003).
- **Un rack** = una o varias **definiciones** de bloque (una por vista: frontal, lateral por seccion, planta)
  que comparten un `RackId`. Cada definicion lleva un **sobre** JSON (`RackEmbedDocument`: `SchemaVersion`,
  `Kind`, `View`, `Section`, `Id`, `Name`, `Design`, `ExtensionData`) en un Xrecord de su diccionario de
  extension (ADR-0009, ADR-0010).
- **Nivel dibujo**: desde I-47 existe **un** Xrecord en el NOD, `RACKCAD_PROJECT`, con el registro de
  variables de proyecto (ADR-0034). Doctrina: `UNKNOWN/UNREADABLE ≠ ABSENT` (ADR-0034 §8).
- **ID24**, segun la instruccion del Coordinador (su texto **no esta versionado**): «Custom Properties
  Foundation», metadatos definidos por el usuario en alcances Proyecto y Rack, con puntos de extension hacia
  dibujo, expresiones y plantillas, sin implementarlos.
- **Paralelas activas** al publicar: I-49 @ `4df9480` (expresiones, solo docs), I-50 @ `8ceb3a7` (cotas por vista,
  produccion en DTO y Domain de Selectivo/Dinamico y en `PushBackCompositeStructure.cs`), I-52 @ `339b3ab`
  (RACKMIRROR, Discovery publicado, solo docs) e I-53 @ `c8476cc` (cabeceras multidestino, Discovery publicado,
  solo docs). Re-medicion y lo que sus Discovery dicen de I-54: Discovery §2.5.

## 3. Lectura obligatoria, en este orden

1. Proposal V1: §1 (premisas), §3 (alternativas), §4 (opcion del Coordinador), §5 (decisiones), §6 (invariantes).
2. Discovery: §1, §4, §5, §6, §8, §9, §12, §13 y §14.
3. Codigo en `BASE_SHA`:

   | Archivo | Lineas |
   |---|---|
   | `src/RackCad.Application/Persistence/RackEmbedDocument.cs` | completo (120) |
   | `src/RackCad.Application/Persistence/RackEmbedComposer.cs` | completo (37) |
   | `src/RackCad.Plugin/RackEnvelopeRestamp.cs` | 52-88 |
   | `src/RackCad.Plugin/RackCloner.cs` | 29-68 |
   | `src/RackCad.Plugin/ProjectVariablesData.cs` | completo (136) |
   | `src/RackCad.Plugin/RackBlockFinder.cs` | 57-91 |
   | `src/RackCad.Plugin/RackCommandSupport.cs` | 105-134 |
   | `src/RackCad.Application/ProjectVariables/SelectiveAuthoredAuthority.cs` | 53-157 |
   | `src/RackCad.Plugin/ProjectVariableMutationExecutor.cs` | 84-99 y 196-295 |
   | `src/RackCad.Plugin/RackSelectivoCommands.cs` | 112-151 y 340-394 |
   | `src/RackCad.Application/Systems/Selective/SelectiveLibraryExport.cs` | completo (96) |
   | `tests/RackCad.Tests/ProjectVariablesConformanceTests.cs` | 110-164 |
   | `tests/RackCad.Tests/PersistenceReopenPreservationTests.cs` | 101-150 |

4. ADR: [0009](../adr/0009-identidad-guid-embebida-en-dwg.md), [0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md),
   [0034](../adr/0034-project-variables-autoridad-drawing-level.md) §1, §8, §12-§15, y los criterios de
   [adr/README.md](../adr/README.md).

## 4. Hechos decisivos y como verificarlos

Todos sobre `BASE_SHA`. Forma general: `git show 46fcac2:<ruta>` y leer las lineas indicadas.

| # | Hecho que sostiene la Proposal | Ruta : lineas | Si fuera falso, cae |
|---|---|---|---|
| F-1 | `Compose` crea un sobre nuevo y del origen solo copia `SchemaVersion` y `ExtensionData` | `RackEmbedComposer.cs:24-33` | D-08 (herencia) |
| F-2 | `RackEmbedStore.Deserialize` devuelve `null` ante `JsonException` y ante major futuro | `RackEmbedDocument.cs:94-115` | D-08 (`JsonElement?`) |
| F-3 | `RACKCAD_PROJECT` es Xrecord directo; una entrada que no sea Xrecord se lee como ilegible | `ProjectVariablesData.cs:59-68,124-133` | D-07 |
| F-4 | `RestampEnvelope` deserializa, muta `Id`/`Name`/`Design` y serializa **el mismo** objeto | `RackEnvelopeRestamp.cs:59-87` | D-11 |
| F-5 | `RackCloner` crea un BTR nuevo, clona solo entidades y escribe el payload recibido | `RackCloner.cs:34-50,66` | D-08 (descarte del Xrecord propio) |
| F-6 | `SelectiveAuthoredAuthority` solo ve el documento interior y excluye por diseño la vista y seccion del sobre | `SelectiveAuthoredAuthority.cs:79-82,96,128-157` | D-09 (autoridad nueva) |
| F-7 | `FindRackBlocks` descarta en silencio los sobres ilegibles | `RackCommandSupport.cs:120-127` | D-09.2 |
| F-8 | El ejecutor de variables compone cada hermana con **su** sobre en **una** transaccion | `ProjectVariableMutationExecutor.cs:89-93,207-217,250-288` | D-09.6, D-08 |
| F-9 | La exportacion del Selectivo parte del diseño de dominio; el envoltorio de biblioteca nunca contiene el sobre | `SelectiveLibraryExport.cs:85-94`; `RackProjectDocument.cs:16-52` | D-12 |
| F-10 | Guardas ordinales prohiben `PropertyValues`, `rackProperty`, `RackPropertyReference` | `ProjectVariablesConformanceTests.cs:114,129-135,162` | D-19 |
| F-11 | Censos: 33 `[CommandMethod(` y 29 ventanas (6/6/11/6) | `SelectiveEditorOpenTests.cs:546`; `WindowCensusGuardTests.cs:166-170` | D-18 |
| F-12 | I-50 no toca sobre, compositor, restamp, cloner ni autoridad | `git diff --name-status a4d88f1...origin/feature/cotas-independientes-por-vista` | P-14, §10 |
| F-13 | I-49 reserva `Rack`/`Project` y `Rack.X`/`Project.X` para ID20 | `origin/architecture/motor-expresiones-parametricas:docs/initiatives/I-49-proposal-v3.md:364-366,537-542` | D-16 |
| F-14 | Ningun proyecto de pruebas referencia el Plugin | `tests/RackCad.Tests/RackCad.Tests.csproj:20-27`; `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj:24-28` | D-11 (capas de prueba) |

## 5. Decisiones que el Arquitecto debe revisar

Formato: **Decision** (referencia) · **Pregunta** · **Evidencia** · **Alternativa principal descartada**.
Las preguntas estan redactadas para no presuponer la respuesta.

| RD | Decision | Pregunta | Evidencia | Alternativa descartada |
|---|---|---|---|---|
| **RD-01** | Representacion B, valores solo texto, «cambio semantico ⇒ major» (D-01, D-04, §3) | ¿Es B con texto la representacion adecuada para la fundacion, y basta la regla de major para evolucionar hacia tipos sin que un build viejo escriba valores invalidos? | P-01, P-13; I-47 A4; deuda de tipos de I-48 | A (mapa por nombre); C (tipados con discriminador) |
| **RD-02** | `Entries` obligatorio; `SchemaVersion` sin inicializador (D-01) | ¿Debe un `Entries` ausente o nulo leerse como ilegible o como vacio? | C4.8-1 de I-47; ausencia de legado | Tolerar ausencia como en `Variables` |
| **RD-03** | GUID `D`, igualdad por valor, id repetido = `AmbiguousIdentity` en el store (D-02) | ¿Donde debe vivir la unicidad de identidad: resultado del store o capa de acreditacion separada? | V7-R01 de I-48; store nuevo | Capa separada |
| **RD-04** | Nombres unicos al escribir (`OrdinalIgnoreCase`), tolerados al leer; limites solo al escribir (D-05) | ¿Deben bloquear los nombres repetidos al leer? ¿Es seguro aplicar limites solo al escribir? | P-07 (replicacion por hermana) | Bloquear en lectura; limites en lectura |
| **RD-05** | Entradas autodescriptivas por alcance (D-03) | ¿Es preferible al modelo definiciones-de-proyecto + valores-por-rack, dado P-10? | P-10; OV-9 de I-47 | Definiciones centrales |
| **RD-06** | Proyecto: clave NOD nueva `RACKCAD_CUSTOM_PROPERTIES`, Xrecord directo, tri-estado, version primero; sin tocar `RACKCAD_PROJECT` ni `ProjectVariablesData`; troceado reimplementado (D-07) | ¿Es correcta la separacion de contenedor y store? ¿Debe extraerse ya el ayudante de troceado duplicado? | F-3; P-02, P-03; ADR-0034 §1, §13 | Dentro de variables; sub-diccionario; `SummaryInfo` |
| **RD-07** | Rack: miembro `CustomProperties` como `JsonElement?` interpretado solo por el store, `WhenWritingNull` (D-08) | ¿Aisla de verdad el sobre ante cualquier contenido (objeto, string, numero, array, `null` literal)? ¿Es preferible a `string`? | F-2; P-05, P-11 | Miembro tipado; `string`; solo `ExtensionData` |
| **RD-08** | Sin promocion de major del sobre (D-08) | ¿Es sostenible no promover, siendo que I-47 promovio por vinculos? ¿La inercia geometrica de los datos basta? | I-47 F2/F3; §8 de la Proposal | Promocion pegajosa |
| **RD-09** | `Compose` hereda el miembro de `source` con firma intacta; `WithCustomProperties` para escribir (D-08) | ¿Es correcta la semantica «propio en redibujo, picado en vista nueva» para un dato de nivel rack? ¿Existe algun camino que construya o serialice un sobre sin `Compose` ni `RestampEnvelope`? | F-1, F-4, F-8 | Parametro explicito en los 7 llamadores |
| **RD-10** | Autoridad nueva, estructural incluir-por-defecto; `SchemaVersion` y `ExtensionData` participan; ausente ≡ vacio (D-09.5) | ¿Introduce la regla ausente ≡ vacio alguna igualdad falsa? | F-6; doctrina ADR-0034 §8 | Reutilizar `SelectiveAuthoredAuthority`; comparar bytes |
| **RD-11** | Pertenencia indeterminada: una definicion colocada con sobre ilegible bloquea escrituras de Rack en todo el dibujo (D-09.2) | ¿Bloqueo global, ignorar, u otra regla? ¿Es aceptable el coste en dibujos reales? | F-7; criterio de `RACKVARIABLES`/`RACKBOMTOTAL` | Escribir solo hermanas legibles |
| **RD-12** | Escritura a todas las hermanas en una transaccion, sin redefinir bloques ni regen; re-barrido dentro; sin control optimista (D-09.6) | ¿Basta reescribir el Xrecord de cada definicion? ¿Algun efecto en UNDO, referencias anidadas o Paper Space? | F-8; `RackBlockData.Write` | Reusar el ejecutor con redibujo |
| **RD-13** | Unificacion elegida por el usuario y descarte explicito de ilegibles, en V1 (D-09.7, D-09.8) | ¿Deben entrar en V1? ¿Contradice el descarte la regla de I-47 que impide sobrescribir un registro ilegible? | Leccion del BLOCKER de I-48 (estado sin salida) | Diferirlos |
| **RD-14** | Vaciar deja `Entries: []`, nunca borra el contenedor; eliminar sin confirmacion (D-10) | ¿Es correcta la semantica de vaciado y de eliminacion? | I-11 (no destruir campos futuros) | Borrar el miembro o la entrada |
| **RD-15** | Duplicacion sin cambio de codigo; prueba en cuatro capas sin extraer el restamp (D-11) | ¿Bastan caracterizacion + guarda + validacion del Owner, o hay que extraer la mitad-sobre a Application pese al cruce con I-52? | F-4, F-14; INV-09 de I-51 sin prueba | Extraccion ahora |
| **RD-16** | Biblioteca: ni exporta ni importa propiedades en V1 (D-12) | ¿Es correcto excluirlas de la biblioteca en la fundacion? | F-9 | Slot en `RackProjectDocument` |
| **RD-17** | Estados malformado/futuro y aislamiento INV-07 (D-13) | ¿Hay algun consumidor existente cuyo resultado cambiaria por el estado de las propiedades? | §7.4 del Discovery | — |
| **RD-18** | Puntos de extension: dibujo solo desde autoridad y en un sentido; expresiones sin sintaxis; plantillas por ids (D-15..D-17) | ¿Dejan los puntos de extension alguna decision irreversible tomada de forma implicita? | F-13 | Reservar sintaxis ahora |
| **RD-19** | UI: un comando con palabra clave, workspace agnostico, ventana C, censos 35/30, nombres y guardas (D-18, D-19) | ¿Es la superficie minima correcta y respeta ADR-0029? | F-10, F-11; patron `RACKVARIABLES` | Dos comandos; editores de sistema |
| **RD-20** | ADR obligatorio y orden de gates G3..G10 (D-20, §11, §12) | ¿Es necesario el ADR y es correcto el orden de gates? | criterios de `adr/README.md` | Sin ADR |

## 6. Donde es mas probable que la Proposal este equivocada

Sugerencias de ataque, no conclusiones:

| # | Pregunta | Pista para buscar |
|---|---|---|
| A-1 | ¿Algun camino serializa un `RackEmbedDocument` sin `Compose` ni `RestampEnvelope`? | `new RackEmbedStore().Serialize` y `new RackEmbedDocument` en `src/` |
| A-2 | ¿Alguna prueba o firma compara payloads completos del sobre por bytes? | pruebas de UI de I-24 (firma del dibujo); `Assert.Equal` sobre JSON en `tests/` |
| A-3 | ¿`System.Text.Json` (.NET 8) convierte un `null` literal en `JsonElement?` nulo, y `WhenWritingNull` se aplica a `Nullable<JsonElement>`? | comportamiento del conversor de `Nullable<T>` |
| A-4 | ¿Algun consumidor compara el sobre completo entre hermanas? | `MutationDestinationBinding`, `RackDuplicationPlan`, pruebas de I-24 |
| A-5 | ¿Tiene efectos reescribir el Xrecord de una definicion con referencias en Paper Space o anidadas? | `RackBlockData.Write`; `RACKDUPLICAR` PD-7 |
| A-6 | ¿La regla de pertenencia indeterminada bloquearia con frecuencia dibujos reales? | sobres de major futuro, bloques de terceros con la misma clave |
| A-7 | ¿La unificacion o el descarte pueden escribir sobre una hermana `IncompatibleMajor`? | D-09.5, D-09.7, D-09.8 |
| A-8 | ¿El descarte puede destruir datos que un build posterior del mismo major considere validos? | la clasificacion de ilegible no debe depender de limites (D-05) |
| A-9 | ¿Algun flujo previsto de I-52 reconstruiria el sobre en lugar de re-estamparlo? | Discovery de I-52 `:126-127`, `:516`, `:780`; D-11 de la Proposal |
| A-10 | ¿Contradice la exclusion de la biblioteca alguna guia o expectativa escrita? | `docs/guias/`, `ideas-futuras.md` |

## 7. Formato del veredicto

```text
Architect Review — I-54 Proposal V1 @ <SHA de 40 hex>
Veredicto global = AGREED | AGREED WITH CHANGES | NOT AGREED

Hallazgos
  [BLOCKER | MATERIAL | MINOR] <id> — RD-xx — <afirmacion> — <evidencia ruta:linea @ SHA> — <cambio requerido>

Decisiones
  RD-01 = AGREE | AGREE WITH CHANGE: <cambio> | DISAGREE: <motivo>
  ...
  RD-20 = ...
```

| Severidad | Significado |
|---|---|
| **BLOCKER** | Inimplementable tal como esta escrito, o contradice un ADR aceptado o una doctrina vigente |
| **MATERIAL** | Cambia una decision, un invariante o una superficie |
| **MINOR** | Precision de redaccion o de evidencia que no cambia decisiones |

## 8. Reglas de la revision

- Evidencia de codigo sobre `BASE_SHA`; si `origin/main` avanzo al revisar, citar ambos SHAs.
- Una afirmacion de la Proposal sin cita verificable es, por si misma, un hallazgo.
- Una guarda de texto no es criterio de aceptacion: el criterio es comportamiento (leccion de I-48).
- La revision **no** desbloquea implementacion: hace falta `Coordinator = AGREED` y `Architect = AGREED`
  sobre el mismo SHA, ADR `propuesto` y aprobacion del Owner.

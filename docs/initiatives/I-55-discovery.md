# I-55 — Discovery (G1): creacion de vistas de rack en todos los sistemas

> **G1, solo documentacion.** Este informe describe el codigo de `BASE_SHA`; no toma decisiones de producto ni de
> arquitectura, no autoriza implementar y no reabre ningun ADR aceptado. Lo que G1 no puede cerrar con evidencia
> queda como pregunta abierta (seccion 20) para el Owner, el Coordinador y el Arquitecto, antes de la Proposal.

```text
Initiative     = I-55 — Creacion de vistas de rack en todos los sistemas
Branch         = feature/creacion-de-vistas
BASE_SHA       = ba497f14581d81e83a27514852d6ec082ff57635   (origin/main, merge de I-54)
CLAIM_SHA      = 24bb9cad77d945fecc00ec41f46ba0d0a7abb1de   (Claim-Id f96a4b1f-40ff-40d0-b3cf-e2cfad778cc7)
BOOTSTRAP_SHA  = 5f774b459eb8b13732e4ae5c4e93a909874a10c5
Codigo auditado = BASE_SHA (la rama no difiere de el en src/, tests/ ni assets/)
Owner ID       = NO FIJADO por la orden de apertura (seccion 20, OQ-1)
Contrato       = docs/initiatives/I-55-creacion-de-vistas.md
```

## 0. Metodo y clases de evidencia

- **[E]** leido directamente por el ejecutor en `BASE_SHA`, con las lineas citadas comprobadas en esta sesion.
- **[A]** lectura de auditoria: cinco exploradores de solo lectura recorrieron en paralelo ventanas de edicion,
  cotas por vista, BOM/listado/`RACKLAYOUT`/`RACKDUPLICAR`, sobre + propiedades + variables y registro de sistemas.
  Sus citas clave se re-leyeron por muestreo; lo que no se re-leyo conserva la marca [A].
- **[I]** inferencia a partir del codigo, sin comprobacion en ejecucion.
- **Limites.** Sin AutoCAD: jig, prompts, cancelaciones y `UNDO` se deducen del codigo. Ninguna suite ejecuta el
  Plugin (solo guardas de texto de fuente), asi que todo comportamiento de dibujo aqui descrito es de codigo, no de
  ejecucion. G1 no corre pruebas: no hay cambio de codigo que validar.
- Rutas relativas a la raiz del repositorio; lineas en `BASE_SHA`. Prefijos usados en tablas: `P/` =
  `src/RackCad.Plugin/`, `A/` = `src/RackCad.Application/`, `D/` = `src/RackCad.Domain/`, `U/` = `src/RackCad.UI/`.

## 1. Respuestas cortas

| # | Respuesta | Seccion |
|---|---|---|
| R-1 | Una **vista** es una **definicion de bloque** cuyo sobre `RackEmbedDocument` lleva la terna `(Id, View, Section)`; la `BlockReference` solo la coloca. **Nada impide dos vistas con la misma terna**: ninguna ruta de insercion consulta las hermanas antes de crear. | 4 |
| R-2 | La **primera vista de un rack nuevo no es uniforme**: Selectivo solo frontal, Dinamico solo lateral, Cabecera solo lateral; **Push Back y Cantilever admiten cualquiera**; Cama tiene una sola; Larguero no dibuja. | 5, 6.2 |
| R-3 | **Una vista por gesto.** Toda ventana cierra al pedir una insercion; no hay cola ni lote de vistas. Cada vista adicional exige un `RACKEDITAR` propio. | 6.3, 8 |
| R-4 | **Insertar una vista enlazada** = redibujar y **confirmar una a una** todas las vistas existentes, y **despues** crear la nueva con el sobre de la vista elegida. Cancelar la nueva **no deshace** el redibujo. | 7 |
| R-5 | El **indice de seccion** lo fija la ventana en Push Back, Cantilever y los frontales del Dinamico; el **Plugin lo pregunta** (fondo, poste) en Selectivo y en el lateral del Dinamico. En el lateral de **Push Back** la ventana manda una **posicion de lista** que el Plugin usa como **indice de poste** (posible defecto, H-01). | 5, 21 |
| R-6 | `DimensionViewKind` **no existe en el Plugin**: el tipo sale del metodo emisor que llama cada builder. La politica `DimensionViews` es **del rack** (del diseno), no del bloque; una vista nueva la hereda del diseno. | 11 |
| R-7 | `CustomProperties` viajan **solo por el origen de `Compose`**: un rack nuevo nace sin ellas y una vista nueva **copia las de la vista elegida sin comparar hermanas**. | 12 |
| R-8 | `ProjectVariables` solo afectan al **Selectivo**: todas sus vistas llevan el **authored reconciliado** (no el efectivo), incluida la nueva. `RACKVARIABLES` nunca crea ni borra vistas y aborta si falta una. | 10, 13 |
| R-9 | **BOM y listado** consolidan por `Id`. El BOM **no lee `View` ni `Section`**, pero depende del **conjunto** de vistas: copias = maximo de referencias; representante = primera definicion del escaneo. | 14 |
| R-10 | `RACKLAYOUT` y `RACKRELLENAR` **exigen la planta**; las copias independientes son **racks nuevos con solo planta**. **Ninguna** de las cinco rutas de inventario/copia **materializa** una vista desde un diseno. | 15, 16 |
| R-11 | `RACKDUPLICAR` (I-51) copia **solo lo seleccionado**, con **un `RackId` nuevo por rack logico por destino**; cada destino es atomico y los anteriores permanecen. | 16 |
| R-12 | **Cancelar** nunca deja un sobre huerfano: el jig cancelado borra la definicion recien creada. Lo que si queda depende del punto (seccion 17). | 17 |

## 2. Preflight y ramas paralelas

Medido con `git fetch --all --prune` y `git ls-remote --heads origin` al reclamar y repetido al cerrar G1; `main` no se
movio (`ba497f1`) y solo I-49 avanzo, con un commit de documentacion.

| Rama | Iniciativa | Punta | Main…rama (detras/delante) | CI de `push` sobre la punta | Archivos tocados | Cruce con la superficie de creacion de vistas |
|---|---|---|---|---|---|---|
| `main` | — | `ba497f1` | — | success (push + dispatch) | — | — |
| `architecture/motor-expresiones-parametricas` | I-49 | `5a3714f` (Amendment A2, solo docs; se movio durante G1 desde `5f969cc`, su candidato G6 **no cerrado**, G7 bloqueado segun su propio commit) | 24 / 22 | success | 58 (`A/Expressions/*`, `A/Units/LengthUnits.cs`, `StructuralSectionUnits.cs`, pruebas, docs) | **0** |
| `feature/rackmirror-espejo-semantico` | I-52 | `deb08cd` (Proposal V9) | 61 / 13 | success | 15, solo `docs/` | **0** de archivo; cruce **semantico** (abajo) |
| `feature/cabeceras-multidestino-dinamico` | I-53D | `a57bd50` (G7 rebasado sobre `ba497f1`) | 0 / 3 | success | 15 (ventana y ensamblador del Dinamico, pruebas) | **2**: `U/Systems/Dynamic/RackDynamicSystemWindow.xaml` y `.xaml.cs` |

La **superficie** medida son los 44 archivos de produccion que este informe cita como autoridad de creacion,
insercion, materializacion, identidad e inventario de vistas (secciones 4 a 16). La medicion se repitio sobre carpetas
completas —`P/**`, `A/Persistence/**`, `A/Systems/Shared/**`, `U/Editor/**`, `U/Systems/**` y `U/RackFrames/**`— con
el mismo resultado: 0, 0 y los mismos 2 archivos de I-53D.

- **I-53D.** Toca el archivo caliente `RackDynamicSystemWindow.xaml(.cs)` ([WORKFLOW](../WORKFLOW.md) §7), que
  contiene la puerta de primera vista del Dinamico (seccion 6.2). Si la Proposal de I-55 cambia esa puerta, hay
  **cruce material**; hoy, con I-55 en solo documentacion, el unico archivo comun es `docs/ROADMAP.md`.
- **I-49.** Sin cruce en la superficie. Consta en `docs/ROADMAP.md:471` que su G10 **preve** tocar
  `U/Systems/Selective/RackSelectiveWindow.xaml.cs`, que tambien contiene la puerta de primera vista del Selectivo.
- **I-52.** Sin produccion, pero su Proposal V9 define un **`MaterializationContext`** (nombre no contractual, no
  persistido): los valores por defecto de creacion bajo los que se regeneran las piezas internas de un bloque
  (`docs/initiatives/I-52-proposal-v9.md:79`, `:373`, `:1978` en `deb08cd`). Es el mismo territorio que la
  **materializacion** de esta auditoria (seccion 9): I-55 lo describe y **no** fija semantica propia.
- `docs/ORCHESTRATION.md` no existe: manda [WORKFLOW](../WORKFLOW.md).

## 3. Inventario de sistemas

Hay **tres registros** distintos y ninguno declara que vistas soporta un sistema [E]:
`D/Systems/Shared/RackSystemKind.cs:4-31` (siete valores), `A/Systems/Shared/SystemRegistry.Default.cs:23-39`
(siete descriptores de persistencia) y `P/KindHandlers/KindHandlerRegistry.cs:55-63` (seis handlers por la cadena
`Kind` del sobre; Larguero ausente a proposito, `:52-53`). El menu usa un cuarto, `U/Editor/EditorModuleRegistry.cs`
(siete modulos) [A].

| `RackSystemKind` | `Kind` del sobre | Handler (RACKEDITAR/BOM/copia) | Comando(s) que crean | Modulo de menu (`U/Editor/EditorModules.cs`) | Editor |
|---|---|---|---|---|---|
| `SelectiveRack` | `selective` (`A/Persistence/RackEmbedDocument.cs:16`) | `SelectiveKindHandler` | `RACKSELECTIVO`/`RS` (`P/RackSelectivoCommands.cs:22-45`) | `SelectiveEditorModule` `:35-56` | `RackSelectiveWindow` |
| `PalletFlow` | `dynamic` (`:17`) | `DynamicKindHandler` | `RACKSISTEMADINAMICO`/`RSD`, demostracion **sin ventana** (`P/RackDinamicoCommands.cs:24-80`) | `DynamicEditorModule` `:73-95` | `RackDynamicSystemWindow` |
| `PushBack` | `pushback` (`:20`) | `PushBackKindHandler` | `RACKPUSHBACK`/`RPB` (`P/RackPushBackCommands.cs:30-69`) | `PushBackEditorModule` `:114-133` | `RackPushBackSystemWindow` |
| `Cantilever` | `cantilever` (`:26`) | `CantileverKindHandler` | `RACKCANTILEVER`/`RCT` (`P/RackCantileverCommands.cs:38-86`) | `CantileverEditorModule` `:152-173` | `RackCantileverWindow` |
| `Selective` (cabecera) | `cabecera` (`:18`) | `CabeceraKindHandler` | `RACKCABECERA`/`RCB`, `QUICKCABECERA`/`QCB` (`P/RackCabeceraCommands.cs:24-114`) | `HeaderEditorModule` `:191-206` | `RackFrameConfiguratorWindow` |
| `Cama` | `cama` (`:19`) | `CamaKindHandler` | `QUICKCAMA`/`QCM` (`P/RackCamaCommands.cs:21-112`) | `FlowBedEditorModule` `:223-241` | `RackFlowBedWindow` |
| `Larguero` | — | — | — | `LargueroEditorModule`, `CanInsert => false` `:250`, nunca inserta `:259-273` | `RackLargueroWindow` |

- `RACKCAD`/`RK` abre el menu y despacha la peticion por su **tipo en ejecucion**
  (`P/RackMenuCommands.cs:21-112`) [E]; `RACKEDITAR`/`RED` elige un bloque y llama `handler.Edit`
  (`:115-152`) [E]. La biblioteca de disenos entra por el mismo menu (`U/RackMainMenuWindow.xaml.cs:185-219`) [E].
- No son racks: el **componente Cantilever suelto** (sin sobre, `P/RackCantileverCommands.cs:381-466`) y
  `RACKSECCION` (accion tipada del menu, `P/RackMenuCommands.cs:36-40`) [E].
- `RackSystemKind.Cantilever` documenta que la estacion **no** es insertable ni duplicable por separado: la unidad
  persistida es la linea (`D/Systems/Shared/RackSystemKind.cs:22-30`) [E].

## 4. Identidad de una vista

### 4.1 El sobre [E]

`RackEmbedDocument` (`A/Persistence/RackEmbedDocument.cs:14-76`): `SchemaVersion` (`:35`), `Kind` (`:38`), `View`
(`:41`), `Section` con valor por defecto `-1` (`:47`), `Id` (`:50`), `Name` (`:53`), `Design` (`:56`),
`CustomProperties` crudo y omitido si es nulo (`:66-67`) y `ExtensionData` (`:74-75`). Las tres vistas son
constantes de texto: `frontal`, `lateral`, `planta` (`:28-30`). `RackEmbedStore.Deserialize` devuelve `null` ante
texto vacio, JSON invalido o major futuro, sin lanzar (`:97-129`).

Toda vista se construye por `RackEmbedComposer.Compose(source, kind, id, name, view, section, design)`
(`A/Persistence/RackEmbedComposer.cs:43-60`), que hereda de `source` la version no degradada, `ExtensionData` y
`CustomProperties`. La guarda `TGrd02_CensoDeCompose_SonLasSieteLlamadasDeD21_PorArchivo` fija sus **siete**
llamadores (`tests/RackCad.Tests/CustomPropertiesEnvelopeGuardTests.cs:69-75`, lista en `:185-194`): **un llamador
nuevo de `Compose` rompe esa prueba** y obliga a clasificar su origen.

### 4.2 Donde vive: la definicion [E]

`RackBlockData` escribe el sobre troceado en el Xrecord `RACKCAD_SELECTIVE` del diccionario de extension del objeto
que recibe (`P/Systems/Shared/RackBlockData.cs:16`, `:20-52`, `:54-89`). Todos los escritores pasan la
**definicion**: `P/Systems/Shared/SystemBlockWriter.cs:33` y `:114`, `P/Drawing/LateralHeaderDrawService.cs:258`,
`P/RackCantileverCommands.cs:156` y `:324`. `PickRackBlock` lee el sobre de la definicion de la referencia elegida
(`P/RackCommandSupport.cs:89-97`). El comentario de `RackBlockData.cs:8` dice «referencia»: ya registrado (F-01 de
I-54, L-1 de I-51).

### 4.3 Hermanas [E]

`RackCommandSupport.FindRackBlocks` devuelve toda definicion cuyo `Id` coincida sin distinguir mayusculas; no exige
`Kind` (`P/RackCommandSupport.cs:109-134`). Recorre `RackBlockFinder.ScanEnvelopes`, que salta layouts, anonimos y
xrefs y cuenta referencias directas desde el registro (`P/RackBlockFinder.cs:57-91`).

### 4.4 Sin unicidad de la terna [E]

Ninguna ruta de insercion consulta las hermanas antes de crear: `DrawSelectiveViewFromAuthored`
(`P/RackSelectivoCommands.cs:426-453`), `DrawDynamicView` (`P/RackDinamicoCommands.cs:83-135`), `DrawPushBackView`
(`P/RackPushBackCommands.cs:73-130`), `DrawCantileverView` (`P/RackCantileverCommands.cs:93-178`) y la insercion de
`EditCabecera` (`P/RackCabeceraCommands.cs:296-317`). El nombre de la definicion se hace unico
(`P/Drawing/LateralHeaderDrawer.cs:242`, `:395`; `P/Drawing/Cantilever/CantileverViewMaterializer.cs:45`, `:318`), asi
que **insertar dos veces la misma vista produce dos definiciones con la misma terna**, legales y redibujadas
identicas en cada edicion. Coincide con lo que midio I-50 (`docs/initiatives/I-50-discovery.md:305-307`).

### 4.5 `View` nulo [E]

La Cama escribe `view: null` para su lateral (`P/RackCamaCommands.cs:197-198`); la documentacion del sobre dice que
nulo es frontal (`A/Persistence/RackEmbedDocument.cs:40`); `RackListBuilder` lo muestra como lateral
(`A/Persistence/RackListBuilder.cs:117-118`); el zoom no lo trata como frontal (`P/RackBlockFinder.cs:23-24`); el
Dinamico lo redibuja como lateral del poste 0 (`P/RackDinamicoCommands.cs:236-239`). Es **F-04** de
[ideas-futuras](../ideas-futuras.md) y no se duplica.

### 4.6 Quien acuna el `RackId` [E]

- **Editores con sesion**: `RackEditorIdentity.EnsureId` acuna un GUID la primera vez y lo conserva
  (`U/Editor/RackEditorIdentity.cs:45-53`); lo llama `RackEditorSession.Complete` en cada peticion
  (`U/Editor/RackEditorSession.cs:119-134`, `:126`). `Adopt` conserva el id de un rack abierto con `RACKEDITAR` y deja
  sin id una plantilla de biblioteca, que recibe GUID nuevo al insertar (`RackEditorIdentity.cs:61-65`;
  `U/Editor/EditorModules.cs:124-125`, `:162-163`).
- **Comandos sin ventana o sin sesion**: `Guid.NewGuid()` en `RACKSISTEMADINAMICO` (`P/RackDinamicoCommands.cs:73`),
  `QUICKCAMA` (`P/RackCamaCommands.cs:102`) y `RackCabeceraCommands.DrawAndPlace` (`P/RackCabeceraCommands.cs:173`).
- **En una edicion** manda el `Id` del sobre elegido y el de la ventana es respaldo
  (`P/RackSelectivoCommands.cs:119`, `P/RackDinamicoCommands.cs:174`, `P/RackPushBackCommands.cs:179`,
  `P/RackCantileverCommands.cs:230`), salvo la Cabecera, que acuna uno nuevo si el sobre no tiene `Id`
  (`P/RackCabeceraCommands.cs:233`; H-08).

## 5. Matriz sistema × ViewKind × variante

**Lectura.** `ViewKind` es el token `View` del sobre. `DimensionViewKind` es el tipo con que la vista consulta la
politica de cotas de I-50 (seccion 11; `—` = el sistema no dibuja cotas). «Rack nuevo» dice si la ventana permite que
esa vista sea la **primera** de un rack; «Enlazada» si `RACKEDITAR` → Insertar la agrega. Todas las filas son [E] salvo
marca.

| # | Sistema | `View` | Variante → `Section` | `DimensionViewKind` | Rack nuevo | Enlazada | Quien fija `Section` | Actualizar / obsoleta | Evidencia |
|---|---|---|---|---|---|---|---|---|---|
| S1 | Selectivo | `frontal` | un bloque por **fondo**: `Section` = fondo (0..n-1); legado `-1` → fondo 0 | Frontal | **Si, unica** | Si | **Plugin**: pregunta el fondo si hay mas de uno; con uno, 0 | redibuja cada frontal con su fondo; fondo ≥ n ⇒ obsoleta | `P/RackSelectivoCommands.cs:155-184`, `:461-500`; `U/Systems/Selective/RackSelectiveWindow.xaml.cs:2560-2570` |
| S2 | Selectivo | `lateral` | un bloque por **corte**: `Section` = `PostIndex` sobre la reticula del fondo maestro; un corte cubre los fondos que alcanzan ese poste | Lateral | No | Si | **Plugin**: pregunta el poste | se empareja por `PostIndex`; sin corte ⇒ obsoleta | `P/RackSelectivoCommands.cs:186-210`, `:519-576` |
| S3 | Selectivo | `planta` | un bloque con todos los fondos; `-1` | Planta | No | Si | — | redibujo | `P/RackSelectivoCommands.cs:212-223`, `:444-450` |
| D1 | Dinamico | `lateral` | un bloque por **poste**; la ventana manda `-1`; legado sin `View`/`Section` → poste 0 | Lateral | **Si, unica** | Si | **Plugin**: pregunta el poste | por `PostIndex`; sin corte ⇒ obsoleta | `P/RackDinamicoCommands.cs:102-107`, `:234-265`, `:294-358`; `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:2947-2956` |
| D2 | Dinamico | `frontal` | **extremo**: `0` salida, `1` entrada; todo valor ≠ 1 ⇒ salida | Frontal (los dos extremos) | No | Si | Ventana (boton) | reescribe `(int)end` | `P/RackDinamicoCommands.cs:115-123`, `:220-233`, `:392-393`; ventana `:2927-2931` |
| D3 | Dinamico | `planta` | `-1` | Planta | No | Si | — | redibujo | `P/RackDinamicoCommands.cs:111-114`, `:209-219` |
| P1 | Push Back | `lateral` | un bloque por **poste**; lados A y B en el mismo bloque | Lateral | Si | Si | **Ventana** (combo de cortes): manda la **posicion en la lista**; el prompt del Plugin solo corre con `-1`, que la ventana no envia | por `PostIndex`; sin corte ⇒ obsoleta | `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3036-3043`, `:3065`; `P/RackPushBackCommands.cs:92-97`, `:118-122`, `:280-302`; ver H-01 |
| P2 | Push Back | `frontal` | **corte × lado**: `EncodeSection = (int)extremo + (B ? 2 : 0)` ⇒ 0 EntradaSalida-A, 1 Posterior-A, 2 EntradaSalida-B, 3 Posterior-B; 2-3 solo en compuesto | Frontal (los cuatro cortes) | Si | Si | Ventana (vista + selector de lado) | reescribe la seccion tal cual, lado incluido | `A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:18-25`, `:138-155`; ventana `:3054-3067`; `P/RackPushBackCommands.cs:105-117`, `:253-279` |
| P3 | Push Back | `planta` | `-1`; lados A y B dentro | Planta | Si | Si | — | redibujo | `P/RackPushBackCommands.cs:101-104`, `:243-252`, `:434-437` |
| C1 | Cantilever | `frontal` | la **linea** completa; `-1` | — | Si | Si | — | redibujo | `P/RackCantileverCommands.cs:93-178`, `:501-540` |
| C2 | Cantilever | `lateral` | una **estacion**: `Section` = indice ≥ 0 | — | Si | Si | Ventana (estacion, base 1 → base 0) | estacion ≥ N ⇒ obsoleta | `P/RackCantileverCommands.cs:118-124`, `:302-310`; `U/Systems/Cantilever/RackCantileverWindow.xaml.cs:922-927`, `:1305-1312` |
| C3 | Cantilever | `planta` | la **linea** completa; `-1`; la visibilidad de brazos y tensores del diseno **no** llega al dibujo (H-02) | — | Si | Si | — | redibujo | `P/RackCantileverCommands.cs:131`, `:312`; `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:293-295` |
| H1 | Cabecera | `lateral` | `-1`; puede haber varias laterales | — | **Si, unica** | Si | — | redibujo | `U/RackFrames/RackFrameConfiguratorWindow.xaml.cs:260`; `P/RackCabeceraCommands.cs:164-199`, `:267-276`, `:307-314` |
| H2 | Cabecera | `planta` | `-1` | — | No | Si | — | redibujo | ventana `:84-95`, `:265-282`; `P/RackCabeceraCommands.cs:278-287`, `:298-306` |
| B1 | Cama | nulo (dibuja un lateral) | `-1`; tipo dinamica/pushback no cambia bloques [A] | — | Si (unica vista) | **No existe** | — | solo la definicion elegida | `P/RackCamaCommands.cs:183-238` |
| L1 | Larguero | — | sin bloque ni sobre | — | No inserta | — | — | — | `U/Editor/EditorModules.cs:246-274` |
| X1 | Componente Cantilever suelto | — | sin sobre: **no es rack** | — | Si | — | punto primero | — | `P/RackCantileverCommands.cs:381-466` |

**Capacidades por sistema** [E]:

| Sistema | Vistas para rack nuevo | Insertar enlazada | Preflight antes de redibujar | Obsoletas | Transaccion del redibujo |
|---|---|---|---|---|---|
| Selectivo | frontal | frontal, lateral, planta | registro + `SelectiveEditorOpen` antes de abrir; reconciliador al cerrar (`P/RackSelectivoCommands.cs:66-87`, `:144-151`) | se borran solo si sobrevive alguna vista (`:225-239`) | una por vista |
| Dinamico | lateral | lateral, frontal salida/entrada, planta | disenos interiores (`P/RackDinamicoCommands.cs:184-191`) | igual (`:268-271`) | una por vista |
| Push Back | cualquiera | cualquiera | `Kind` + GUID, disenos interiores y descriptor (`P/RackPushBackCommands.cs:190-223`) | igual (`:305-313`) | una por vista |
| Cantilever | cualquiera | cualquiera | `Kind` + GUID, disenos interiores, descriptor y catalogo de secciones (`P/RackCantileverCommands.cs:241-276`) | igual (`:338-347`) | una por vista (`:317-332`) |
| Cabecera | lateral | lateral, planta | disenos interiores (`P/RackCabeceraCommands.cs:248-256`) | no aplica | una por vista |
| Cama | su unica vista | no | lectura del diseno (`P/RackCamaCommands.cs:206-213`) | no aplica | una |

## 6. Creacion: la primera vista

### 6.1 Entradas [E]

- **Menu `RACKCAD`**: el boton «Disenar …» abre `module.OpenForNew`; si vuelve una peticion, el menu la guarda y cierra
  (`U/RackMainMenuWindow.xaml.cs:132-167`). Tras cerrarse, el Plugin avisa unidades si hay peticion y despacha
  (`P/RackMenuCommands.cs:46-106`); todas las ramas componen con `source: null` salvo el componente suelto.
- **Comandos por sistema**: `RACKSELECTIVO` (`P/RackSelectivoCommands.cs:25-45`), `RACKPUSHBACK`
  (`P/RackPushBackCommands.cs:34-69`), `RACKCANTILEVER` (`P/RackCantileverCommands.cs:42-86`), `RACKCABECERA`
  (`P/RackCabeceraCommands.cs:28-48`); los rapidos `QUICKCABECERA` y `QUICKCAMA` preguntan por linea de comandos y
  `RACKSISTEMADINAMICO` dibuja un lateral de demostracion con valores fijos (`P/RackDinamicoCommands.cs:42-74`).
- **Biblioteca**: `OpenFromLibrary` carga la plantilla como rack **nuevo** (`U/RackMainMenuWindow.xaml.cs:185-219`;
  `U/Systems/Selective/RackSelectiveWindow.xaml.cs:3257-3266`), con las mismas puertas de primera vista que un rack
  nuevo.

### 6.2 Puerta de primera vista, por ventana [E]

| Ventana | Condicion | Efecto | Prueba que la fija |
|---|---|---|---|
| Selectivo | `!isEditingExisting && (updateOnly \|\| view == lateral \|\| view == planta)` (`RackSelectiveWindow.xaml.cs:2560`) | aviso «Primero inserta la vista frontal…» y no pide nada (`:2561-2570`); los botones lateral/planta/Actualizar se apagan (`:274-292`); «Insertar frontal» nunca se apaga | `SelectiveWindow_DisabledDrawActions_KeepReasonAndShowOnDisabled` y `SelectiveWindow_DrawActionsEnabled_WhenEditingExistingFromAutoCad_WithTheirRealTooltips` (`tests/RackCad.UI.Tests/SelectiveShellMigrationTests.cs:156`, `:179`) |
| Dinamico | `!isEditingExisting && (updateOnly \|\| view != lateral)` (`RackDynamicSystemWindow.xaml.cs:2947`) | aviso «Primero inserta la vista lateral…» (`:2949-2955`); frontal, planta y Actualizar se apagan (`:190-195`) | `DynamicWindow_InsertLateralEnabled_WhenOpenedFromAutoCad_WithItsRealTooltip` (`tests/RackCad.UI.Tests/DynamicShellMigrationTests.cs:151`); `NewSystem_InsertLateral_ViaButton_MintsGuid_AndBuildsTheRealPayload` (`tests/RackCad.UI.Tests/DynamicEditorWindowTests.cs:175`) |
| Cabecera | planta con `!IsEditingExisting` (`RackFrameConfiguratorWindow.xaml.cs:269-279`) | aviso «Primero inserta la cabecera lateral…»; Actualizar y planta se apagan al cargar (`:84-95`) | [A] no se localizo prueba especifica |
| Push Back | solo `updateOnly && !isEditingExisting` (`RackPushBackSystemWindow.xaml.cs:3272-3276`) | **cualquier vista** puede ser la primera; solo Actualizar exige `RACKEDITAR` (`:3464-3468`) | `LateralPreview_UsesTheSelectedCorte_AndInsertSectionMatches` (`tests/RackCad.UI.Tests/PushBackEditorWindowTests.cs:403-426`) |
| Cantilever | solo `updateOnly && !isEditingExisting` (`RackCantileverWindow.xaml.cs:1291-1295`) | **cualquier vista** puede ser la primera (`:1389-1396`) | [A] `CantileverEditorWindowTests.cs:380-438` pulsa las tres |
| Cama | — (`U/Systems/FlowBed/RackFlowBedWindow.xaml.cs:258-280` [A]) | una sola insercion | — |

Los avisos justifican la puerta por la **vinculacion** («asi quedan ligadas», `RackSelectiveWindow.xaml.cs:2558-2565`;
«quedaria huerfana», `RackFrameConfiguratorWindow.xaml.cs:267-274`): como la ventana cierra tras **una** insercion y la siguiente sesion acuna
otro GUID (4.6, 6.3), una segunda vista pedida desde una ventana nueva seria **otro rack**. Eso explica que las vistas
adicionales vayan por `RACKEDITAR`, pero **no** que la primera deba ser una concreta: todo bloque lleva el diseno
completo, y Push Back y Cantilever empiezan por cualquier vista sin esa puerta [E]. La norma escrita («las vistas
adicionales solo se insertan desde un rack existente», [ARCHITECTURE](../ARCHITECTURE.md) §4.1;
[ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md)) se cumple en los seis sistemas; lo que difiere es
**cual** puede ser la primera, y ADR-0010 declara expresamente que «no cambia la insercion inicial».

### 6.3 Una vista por gesto [E]

Cada `RequestDraw` termina en `Close()` tras pedir **una** vista: Selectivo `RackSelectiveWindow.xaml.cs:2598`,
Dinamico `RackDynamicSystemWindow.xaml.cs:2981`, Push Back `RackPushBackSystemWindow.xaml.cs:3319`, Cantilever
`RackCantileverWindow.xaml.cs:1328`; la Cabecera cierra en su propio `RequestDraw` [A]
(`RackFrameConfiguratorWindow.xaml.cs:338-341`). Cada peticion lleva **una** `View` y **una** `Section`
(`U/Editor/RackInsertionRequest.cs:60-230`), y cada rama del Plugin dibuja **un** bloque con **un** jig. No existe
estructura de cola ni de lote de vistas en `src/`.

### 6.4 Aviso de unidades [E]

`RackUnitsGuard.WarnIfNotInches` escribe un aviso y **nunca** aborta ni convierte (`P/RackUnitsGuard.cs:7-45`). Se
llama antes del primer dibujo en cada entrada y, en `RACKEDITAR`, solo cuando habra vista nueva
(`P/RackSelectivoCommands.cs:109-112`, `P/RackDinamicoCommands.cs:195-198`, `P/RackPushBackCommands.cs:227-230`,
`P/RackCantileverCommands.cs:280-283`, `P/RackCabeceraCommands.cs:260-263`).

## 7. Vistas enlazadas: `RACKEDITAR` → Insertar

### 7.1 Secuencia comun [E]

1. `PickRackBlock` (cancelable) lee el sobre de la definicion (`P/RackCommandSupport.cs:74-99`); `RACKEDITAR` exige
   `Design` y resuelve el handler de forma ordinal (`P/RackMenuCommands.cs:127-146`).
2. El handler deserializa el diseno y abre la ventana con `LoadExisting` (Selectivo `:54`, `:75-91`; Dinamico
   `:145-165`; Push Back `:143-162`; Cantilever `:192-212`; Cabecera `:205-223`).
3. Si la ventana no pidio nada, **retorna sin tocar el dibujo** (`P/RackSelectivoCommands.cs:101-104`,
   `P/RackDinamicoCommands.cs:167-170`, `P/RackPushBackCommands.cs:164-167`, `P/RackCantileverCommands.cs:214-217`,
   `P/RackCabeceraCommands.cs:225-228`).
4. Reune las hermanas por `Id` y corre los **preflights** (tabla de la seccion 5).
5. **Redibuja en sitio cada vista existente** con su propio sobre como origen, con `regen: false`, y borra las
   obsoletas solo si sobrevive alguna.
6. **Un** `Regen`.
7. Si no es `UpdateOnly`, **inserta la vista nueva** por la ruta de creacion con `source: embed` (el sobre elegido) y,
   donde aplica, `innerSource: project` (el diseno interior elegido): `P/RackSelectivoCommands.cs:249-259`,
   `P/RackDinamicoCommands.cs:278-282`, `P/RackPushBackCommands.cs:326-331`, `P/RackCantileverCommands.cs:360-367`,
   `P/RackCabeceraCommands.cs:296-316`.

### 7.2 No hay atomicidad entre vistas [E]

Cada redibujo abre y **confirma su propia transaccion**: `SystemBlockWriter.RedrawInPlace`
(`P/Systems/Shared/SystemBlockWriter.cs:45-80`), `LateralHeaderDrawService.RedrawInPlace`
(`P/Drawing/LateralHeaderDrawService.cs:112-157`) y el bucle del Cantilever (`P/RackCantileverCommands.cs:317-332`).
Un fallo a mitad deja confirmadas las vistas anteriores [I]. La vista nueva se crea **despues** de ese commit y del
`Regen`, en otra transaccion, y su prompt o su jig pueden cancelarse (seccion 17). Solo la propagacion de variables de
proyecto escribe todas las vistas en **una** transaccion (`P/ProjectVariableMutationExecutor.cs:246-289`).

### 7.3 Particularidades [E]

- **Selectivo.** Es el unico que **pregunta el fondo** en el Plugin (`P/RackSelectivoCommands.cs:472-492`); el poste
  del lateral lo preguntan el Selectivo y el Dinamico (`:546-566`; `P/RackDinamicoCommands.cs:318-330`). Es tambien el
  unico que resuelve el registro de variables **antes** de abrir la ventana (`P/RackSelectivoCommands.cs:66-73`).
- **Dinamico.** Un bloque legado sin `View`/`Section` se redibuja como corte del poste 0
  (`P/RackDinamicoCommands.cs:236-239`).
- **Push Back y Cantilever.** Rechazan un descriptor `(View, Section)` invalido **antes** de tocar geometria
  (`P/RackPushBackCommands.cs:214-223`, `:424-450`; `P/RackCantileverCommands.cs:264-271`, `:525-540`).
- **Cama.** Redibuja **solo** la definicion elegida y no ofrece insercion (`P/RackCamaCommands.cs:226-237`).

## 8. `RequestDraw`: contrato UI → Plugin

### 8.1 Sesion compartida y peticiones tipadas [E]

`RackEditorSession<TDesign,TSystem>.Complete` (`U/Editor/RackEditorSession.cs:119-134`): asegura el id (`:126`),
normaliza `InsertView = updateOnly ? null : view` y `InsertSection = updateOnly ? -1 : section` (`:128-129`),
construye la peticion con un `RackInsertionContext` (`:12-40`) y marca `InsertRequested` (`:132`). Las peticiones
viven en la UI (`U/Editor/RackInsertionRequest.cs`): `SelectiveInsertionRequest` **sin** `Section` (`:207-230`),
`DynamicInsertionRequest` (`:60-90`), `PushBackInsertionRequest` (`:124-156`, cuyo comentario de `:152` sigue
describiendo `Section` como `(int)PushBackFrontalEnd`, hoy 0-3), `CantileverInsertionRequest` (`:168-201`),
`HeaderInsertionRequest` y `FlowBedInsertionRequest` **sin** vista (`:36-53`, `:97-116`). El evento
`InsertRequestedRaised` no tiene suscriptores de produccion [A].

### 8.2 Por ventana

| Ventana | Firma | Llamadas | Antes de pedir | Evidencia |
|---|---|---|---|---|
| Selectivo | `RequestDraw(view, updateOnly)` | frontal, lateral, planta, Actualizar | puerta de primera vista; confirmar editores pendientes y celdas sin aplicar; `BuildSystem` | `RackSelectiveWindow.xaml.cs:2537-2599` [E] |
| Dinamico | `RequestDraw(view, section, updateOnly)` | lateral `-1`, frontal `0`/`1`, planta `-1`, Actualizar | puerta; sistema presente; opcionales validos; `Recompose` | `RackDynamicSystemWindow.xaml.cs:2924-2982` [E] |
| Push Back | `RequestDraw(view, section, updateOnly)` | «Insertar vista actual» y cuatro atajos por vista, Actualizar | confirmar la sesion de modulos; recalculo sincrono; modelo valido; puerta de salida bloqueada | `RackPushBackSystemWindow.xaml.cs:3262-3320` [E]; atajos `:3177-3232` [A] |
| Cantilever | `RequestDraw(view, section, updateOnly)` | frontal `-1`, lateral estacion, planta `-1`, Actualizar | recalculo; linea valida; estacion en rango | `RackCantileverWindow.xaml.cs:1283-1329` [E] |
| Cabecera | `RequestDraw(view, updateOnly)` | `"lateral"`, `"planta"` (solo editando), Actualizar | modo rapido: confirmar descarte; modelo inconsistente: confirmar | `RackFrameConfiguratorWindow.xaml.cs:258-282` [E]; `:288-341` [A] |
| Cama | `session.RequestInsert(null, -1, …)` | una | lectura de la configuracion | `RackFlowBedWindow.xaml.cs:258-280` [A] |

`RACKCABECERA` ignora `InsertView` y `UpdateOnly` y siempre dibuja un lateral con GUID nuevo
(`P/RackCabeceraCommands.cs:37-41`, `:173-174`) [E], coherente con que en ese modo solo el lateral esta habilitado.

## 9. Materializacion

### 9.1 Plan → definicion → referencia [E]

- `ViewBlockDraw.DrawAndPlace` (`P/Systems/Shared/ViewBlockDraw.cs:28-57`) → `SystemBlockWriter.CreateBlock`
  (`P/Systems/Shared/SystemBlockWriter.cs:18-40`): importa de la biblioteca los bloques que falten **fuera** de la
  transaccion (`:25`), crea la definicion, escribe el sobre y **confirma antes del jig** (`:27-38`).
- `BlockPlacement.PlaceAndReport` (`P/Drawing/BlockPlacement.cs:26-53`) arrastra una referencia con
  `HeaderInsertionJig` (`:174-245`) y la agrega a Model Space **sin asignar capa ni otras propiedades**
  (`:182-197`). Si el usuario cancela, borra la definicion recien creada si nadie la referencia, y purga sus
  definiciones anidadas privadas (`:37-42`, `:123-170`).
- `LateralHeaderDrawService` tiene flujo propio con agrupacion ARRAY (`P/Drawing/LateralHeaderDrawService.cs:29-54`,
  `:237-272`) y termina en el mismo `PlaceAndReport` (`:234-235`).
- **Cantilever** no usa `ViewBlockDraw`: crea la definicion desde el plan de contornos, escribe el sobre, confirma y
  coloca con `BlockPlacement.PlaceDefinition` (`P/RackCantileverCommands.cs:142-169`; `P/Drawing/BlockPlacement.cs:64-76`),
  y es el unico que regenera tras insertar (`:169`).
- `ViewBlockDraw.DrawAndPlace` y `BlockPlacement.PlaceAndReport` **no** regeneran: confirma **F-12** de
  [ideas-futuras](../ideas-futuras.md) sobre el comentario de `P/RackPushBackCommands.cs:315-318`, que no se duplica.
- Los valores por defecto de creacion bajo los que AutoCAD agrega esas referencias los esta caracterizando **I-52**
  (`MaterializationContext`, seccion 2); este informe no los mide.

### 9.2 Redibujo [E]

`SystemBlockWriter.RedefineInTransaction` redefine la definicion y reescribe el sobre dentro de la transaccion del
llamador (`P/Systems/Shared/SystemBlockWriter.cs:104-116`); `ViewBlockDraw.PrepareRedraw` / `RedrawInTransaction`
separan PREPARE (catalogo, plan, importacion) de MUTATE (`P/Systems/Shared/ViewBlockDraw.cs:59-109`;
`P/Systems/Shared/PreparedViewRedraw.cs:8-50`). Los editores usan el camino con transaccion propia; solo
`RACKVARIABLES` usa PREPARE/MUTATE sobre varias vistas (seccion 13).

### 9.3 Nombres [E]

Cada servicio decide el nombre base (`P/Systems/Selective/SelectiveFrontalDrawService.cs:76-88`,
`SelectivePlantaDrawService.cs:67-75`, `P/Systems/Dynamic/DynamicFrontalDrawService.cs:50-55`,
`DynamicPlantaDrawService.cs:48-52`, `DynamicSystemDrawService.cs:61-73`, `P/Systems/PushBack/PushBackFrontalDrawService.cs:59-74`,
`PushBackPlantaDrawService.cs:50-58`, `PushBackSystemDrawService.cs:62-73`, `P/Systems/FlowBed/FlowBedDrawService.cs:48-60`,
`P/Drawing/PlantaHeaderDrawService.cs:45-46`); el Plugin anade sufijos por fondo, poste o estacion
(`P/RackSelectivoCommands.cs:503-511`, `:568-569`; `P/RackCantileverCommands.cs:543-559`) y `RackBlockRenamer.SyncName`
los sincroniza al editar.

### 9.4 Quien crea definiciones [E]

Solo cuatro sitios crean un `BlockTableRecord` de vista o pieza: `P/Drawing/LateralHeaderDrawer.cs:242` (via
`SystemBlockWriter` y `LateralHeaderDrawService`), `P/Drawing/Cantilever/CantileverViewMaterializer.cs:45` y `:108`,
`P/Drawing/StructuralSections/StructuralSectionMaterializer.cs:53` y `P/RackCloner.cs:34`. `RackCloner` **copia** la
geometria ya dibujada de otra definicion (cotas incluidas) y solo reescribe el sobre [A] (`P/RackCloner.cs:38-66`):
las copias independientes **no** se re-materializan desde el diseno.

## 10. Authored / effective

- **Solo el Selectivo** tiene vinculos: el catalogo de propiedades vinculables contiene dos propiedades del
  Selectivo [A] (`A/ProjectVariables/SelectiveLinkedProperties.cs:188-214`), y los demas handlers ignoran el registro
  (`P/KindHandlers/DynamicKindHandler.cs:22-35`, `PushBackKindHandler.cs:25-38`, `CabeceraKindHandler.cs:20-33`,
  `CamaKindHandler.cs:22-35`, `CantileverKindHandler.cs:32-45`) [E].
- **Rack nuevo**: el documento se construye desde el diseno de dominio, sin vinculos que preservar
  (`P/RackSelectivoCommands.cs:363-376`, `:396-409`) [E].
- **Edicion**: `SelectiveEditorOpen.Resolve` da a la ventana el diseno **efectivo** y el estado authored de cada
  propiedad (`P/RackSelectivoCommands.cs:62-73`, `:89-90`); al cerrar, `LinkedPropertyReconciler.Reconcile` produce el
  authored final y se serializa **una vez** para todas las vistas (`:138-153`) [E].
- **Vista nueva**: porta **el mismo** JSON authored que las hermanas redibujadas; la geometria sale del sistema
  efectivo de la ventana (`P/RackSelectivoCommands.cs:249-259`, `:411-453`) [E]. Lo fijan
  `GUARDA_INSERTAR_DURANTE_UNA_EDICION_PORTA_EL_AUTHORED_RECONCILIADO` y
  `RECONSTRUIR_EL_DOCUMENTO_DESDE_EL_EFECTIVO_PRODUCE_DIVERGENCIA_AUTHORED` [A]
  (`tests/RackCad.Tests/LinkedPropertyViewPayloadTests.cs:220-229`, `:136-159`).
- La ruta de `RACKEDITAR` **no** compara el authored de las hermanas: `SelectiveAuthoredAuthority` la usan el BOM, la
  duplicacion y la propagacion, no el editor [A] (`A/ProjectVariables/SelectiveAuthoredAuthority.cs:96-157`).

## 11. `DimensionViews` y `DimensionViewKind` (I-50)

- `DimensionViewKind { Frontal, Lateral, Planta }` y la regla `DimensionViewPolicy.EffectiveDetail` viven en
  Application (`A/Systems/Shared/DimensionViewPolicy.cs:11-49`) [E]: `None` siempre gana, politica nula = legado
  exacto, valor presente = bit del tipo. `Section` **no crea tipo** (`:7-9`).
- **No hay correspondencia `(View, Section)` → tipo en produccion** [E]. El tipo esta **fijo en cada metodo emisor**:
  `SelectiveDimensions.cs:42`, `:64` (Frontal), `:144` (Lateral), `:210` (Planta); `DynamicViewDecorations.cs:51`
  (Frontal), `:112` (Planta), `:191` (Lateral). Fuera de Application solo lo usan las tres ventanas; el Plugin **no** lo
  nombra. La unica tabla clave → tipo es de pruebas [A] (`tests/RackCad.Tests/DimensionViewScenarios.cs:126-356`).
  **Una variante de vista nueva heredaria el tipo del builder que la dibuje**, sin ninguna guarda de produccion.
- **La politica es del rack**: `int? DimensionViews` en `SelectivePalletDesignDocument` y `DynamicRackSystemDocument`
  (Push Back la hereda por `Structure`) [A]; el sobre no la lleva (`A/Persistence/RackEmbedDocument.cs:35-75`) [E]. Cada
  frontal por fondo la copia en `SelectiveDepthLayout.FondoSystemView` (`A/Systems/Selective/SelectiveDepthLayout.cs:84`)
  [E]. Una vista enlazada nueva la recibe del diseno que porta.
- **Cotas y copias**: las copias independientes clonan las cotas ya dibujadas (seccion 9.4) [A]; el BOM las fuerza a
  `None` al contar [A] (`A/Systems/Selective/SelectiveBomBuilder.cs:68`).
- Sin cotas: Cantilever, Cama, Cabecera y Larguero ([ADR-0035](../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md);
  `docs/initiatives/I-50-discovery.md:71-74`).

## 12. `CustomProperties` (I-54)

- **Rack nuevo**: `source` nulo ⇒ sin miembro (`A/Persistence/RackEmbedComposer.cs:57`; `RackEmbedDocument.cs:66-67`) [E].
- **Redibujo**: cada vista conserva **la suya** (`Compose` con su propio sobre) [E]; lo fija
  `TEnv08_DosVistasHermanas_CadaUnaConservaLaSuya` [A] (`tests/RackCad.Tests/CustomPropertiesEnvelopeTests.cs:460-474`).
- **Vista nueva**: **copia las del sobre elegido**, verbatim, sin comparar hermanas (llamadas de la seccion 7.1, paso
  7) [E]. Un rack `Divergent` sigue divergente y uno `Single` sigue unico [A]
  (`A/CustomProperties/RackCustomPropertiesAuthority.cs:323-347`).
- **Donde se detecta la divergencia**: solo en `RACKPROPIEDADES`, al leer y al escribir [A]
  (`P/CustomPropertiesExecutor.cs:214`, `:251-252`, `:279-280`); ninguno de los seis archivos de comandos por sistema,
  ni `RackDuplicarCommands` ni `RackLayoutCommands`, las nombra [A].
- **Copias**: `RACKDUPLICAR` y el `RACKLAYOUT` independiente las llevan verbatim por el re-estampado [A]
  (`P/RackEnvelopeRestamp.cs:52-111`); no se prueba que una vista nueva de `RACKEDITAR` coincida con sus hermanas [A].
- [ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md) declara que la igualdad entre hermanas «hay que
  comprobarla» y no tenerla por construccion (`:434-435`) [A].

## 13. `ProjectVariables` (I-47 / I-48)

- `RACKEDITAR` Selectivo lee el registro y el barrido de sobres en **una** transaccion corta antes de abrir la ventana
  (`P/RackSelectivoCommands.cs:271-298`) y no lo relee al cerrar (el valor de `:66` se reutiliza en `:145`) [E].
- `RACKVARIABLES` y el gesto de vinculo ejecutan `ProjectVariableMutationExecutor` [E]:
  - por rack, reune las vistas por GUID y exige que coincidan **exactamente** con los destinos del plan
    (`P/ProjectVariableMutationExecutor.cs:145-164`);
  - una lateral sin corte o una frontal de un fondo que ya no existe **abortan** la operacion, en vez de borrarse como
    en el editor (`:189-205`);
  - compone cada sobre con **el suyo** y el `Kind` fijo en `selective` (`:207-217`);
  - escribe registro y vistas en **una** transaccion y regenera una vez (`:246-293`);
  - **nunca crea ni borra vistas**.

## 14. BOM y listado

- **`RACKLISTA`** [E]: salta sobres nulos o sin `Id`/`Kind` (`P/RackInventarioCommands.cs:50-56`); agrupa por `Id`;
  copias = **maximo** de referencias directas entre las vistas del rack (`:68-72`). `RackListBuilder.DescribeViews`
  agrupa por vista normalizada (nulo ⇒ lateral) y cuenta **secciones distintas por vista** (`A/Persistence/RackListBuilder.cs:104-118`),
  asi que un Selectivo de N fondos apareceria como «frontal ×N» [I] (H-12). El zoom prefiere una referencia frontal
  en Model Space (`P/RackBlockFinder.cs:19-40`).
- **`RACKBOMTOTAL`** [E]: agrupa por `RackId` (`P/RackInventarioCommands.BomTotal.cs:73-107`); copias = maximo
  (`:109-113`); solo racks colocados (`:122-129`); resuelve todos los handlers **antes** de construir, con el `Kind` de
  la **primera** vista (`:131-134`); una definicion colocada con sobre ilegible aborta el total (`:84-96`). El
  **representante** es la primera hermana del escaneo (`A/Bom/BomAuthoredAuthority.cs:104-110`); en el Selectivo todas
  las hermanas deben coincidir en authored (`:112`). Los handlers leen solo `embed.Design`, nunca `View` ni `Section`
  (`P/KindHandlers/*KindHandler.cs`, metodos `Build`/`BuildBom`) [E].
- **Consecuencia para vistas** [I]: agregar o quitar una vista no cambia el BOM de un rack, pero puede cambiar sus
  **copias** (si esa vista tiene mas referencias) y, en el Selectivo, excluirlo si una vista diverge o no se lee.

## 15. `RACKLAYOUT` y `RACKRELLENAR`

- **Semilla** [E]: `RACKLAYOUT` resuelve el `Kind` sin distinguir mayusculas (`P/RackLayoutCommands.cs:60-67`), exige
  una **planta** del rack por GUID y aborta sin ella con «Dibujala primero (RACKEDITAR)» (`:69-76`), y mide la primera
  referencia en Model Space de la primera planta (`:176-211`). `RACKRELLENAR` hace lo mismo **sin** comprobar el kind
  [A] (`P/RackLayoutCommands.Fill.cs:53-64`).
- **Enlazadas**: referencias nuevas a la **misma** definicion de planta (`P/RackLayoutCommands.cs:230`, `:250-256`) [E];
  `RACKRELLENAR` siempre enlaza [A] (`Fill.cs:454`).
- **Independientes**: por celda, re-estampa el sobre de la planta con GUID nuevo y **clona solo esa definicion**
  (`P/RackLayoutCommands.cs:231-244`) [E]. Cada celda independiente es por tanto **un rack nuevo cuya unica vista es una
  planta** [I]; para tener otra vista hay que `RACKEDITAR` sobre esa planta.
- **Sin materializacion ni vistas nuevas**; **sin commit parcial**: toda la rejilla va en una transaccion y un fallo
  de re-estampado lanza antes del commit (`:219-266`) [E].

## 16. `RACKDUPLICAR` (I-51)

- **Solo lo seleccionado**: una vista que nadie selecciono nunca se agrega (`P/RackDuplicarCommands.cs:24-39`) [E].
- **Identidad**: un GUID nuevo por grupo (rack logico) por destino, compartido por sus vistas seleccionadas
  (`:131-141`) [E]; el plan puro agrupa por `RackId`, falla cerrado con kinds mezclados, nombres divergentes o authored
  divergente del Selectivo [A] (`A/Persistence/RackDuplicationPlan.cs:494-582`).
- **Atomicidad**: PREPARE de todos los re-estampados antes de abrir la transaccion; cada destino entero o nada; los
  destinos anteriores **permanecen** (`P/RackDuplicarCommands.cs:36-37`, `:137-155`) [E].
- **Materializacion**: clona definiciones existentes (`P/RackCloner.cs:34`) [E]; no reconstruye la geometria desde el
  diseno [A] (`:38-66`).

## 17. Semantica de cancelacion

| Punto donde el usuario cancela o algo falla | Estado resultante | Evidencia |
|---|---|---|
| Cierra la ventana de un rack nuevo sin pedir | Nada en el dibujo | `P/RackSelectivoCommands.cs:34`; `P/RackPushBackCommands.cs:48-51`; `P/RackCantileverCommands.cs:65-68`; `P/RackCabeceraCommands.cs:37`; `P/RackMenuCommands.cs:53` [E] |
| Prompt de fondo (Selectivo) o de poste (Dinamico) de un rack nuevo | Nada; la definicion aun no existe; sin mensaje | `P/RackSelectivoCommands.cs:485-489`; `P/RackDinamicoCommands.cs:326-330` [E] |
| Jig de un rack nuevo (Esc) | La definicion ya confirmada **se borra** con sus anidadas privadas; mensaje «bloque '…' creado, pero la insercion se cancelo» (H-05); Cantilever: «no se dejo nada en el dibujo» | `P/Drawing/BlockPlacement.cs:37-42`, `:123-170`; `P/RackCommandSupport.cs:182-185`; `P/RackCantileverCommands.cs:163-167` [E] |
| Idem, definiciones de biblioteca importadas antes del jig | Importadas en su propia transaccion antes de crear (`SystemBlockWriter.cs:25`); la limpieza solo purga las anidadas directas de la definicion borrada: pueden **quedar** importadas | `P/Systems/Shared/SystemBlockWriter.cs:23-39`; `P/Drawing/BlockLibraryImporter.cs:24-60`; `BlockPlacement.cs:139-162` [I] |
| `RACKEDITAR`: seleccion, ventana cerrada sin pedir, o preflight | Nada modificado | seccion 7.1, pasos 1-4 [E] |
| `RACKEDITAR` → Insertar: prompt de fondo/poste o jig de la vista nueva | **Las vistas existentes ya quedaron redibujadas y confirmadas** (y las obsoletas borradas); la nueva no se crea | `P/RackSelectivoCommands.cs:241-259`; `P/RackDinamicoCommands.cs:273-282`; `P/RackPushBackCommands.cs:319-331`; `P/RackCantileverCommands.cs:355-367`; `P/RackCabeceraCommands.cs:289-316` [E] |
| `RACKEDITAR`: fallo a mitad del redibujo multivista | Vistas anteriores confirmadas; sin reversion | seccion 7.2 [I] |
| Componente Cantilever pedido **durante** `RACKEDITAR` | La ventana cierra con `InsertRequested = false`; el comando retorna **sin dibujar el componente ni redibujar la linea** (H-03) | `U/Systems/Cantilever/RackCantileverWindow.xaml.cs:176-195`; `P/RackCantileverCommands.cs:214-217` [E] |
| `RACKVARIABLES`: vista ausente u obsoleta | Aborta todo; registro y vistas intactos | `P/ProjectVariableMutationExecutor.cs:159-164`, `:189-205`, `:266-272` [E] |
| `RACKLAYOUT` / `RACKRELLENAR`: cualquier prompt, dialogo o fallo | Nada: una sola transaccion de colocacion | `P/RackLayoutCommands.cs:49-52`, `:86-89`, `:107-110`, `:219-266` [E]; `Fill.cs` [A] |
| `RACKDUPLICAR`: seleccion, plan, ensayo o punto base | Nada | `P/RackDuplicarCommands.cs:54-96` [A] |
| `RACKDUPLICAR`: Enter/Esc en el destino k, o fallo en PREPARE/MUTATE de k | Destinos 1..k-1 permanecen; k no se escribe | `P/RackDuplicarCommands.cs:126-155` [E] |

No hay marcas de `UNDO` (`StartUndoMark`/`EndUndoMark`) en `src/` [A]: lo que agrupa un `UNDO` lo decide AutoCAD.

## 18. Pruebas y guardas que fijan el comportamiento actual

- **Puertas de primera vista y peticiones reales**: seccion 6.2; ademas
  `ExistingRack_InsertLateral_ViaButton_KeepsGuidAndName_LinkedView` (`tests/RackCad.UI.Tests/SelectiveEditorWindowTests.cs:101`),
  `ExistingSystem_InsertEntranceFrontal_ViaButton_KeepsGuidName_CarriesSection_AndSourceMetadata`
  (`tests/RackCad.UI.Tests/DynamicEditorWindowTests.cs:223`) y los `SelectiveWindow_Insert*_ViaShellHostedButton_*`
  (`tests/RackCad.UI.Tests/SelectiveShellMigrationTests.cs:326`, `:367`) [E].
- **Censo de `Compose`**: `TGrd02_*` (`tests/RackCad.Tests/CustomPropertiesEnvelopeGuardTests.cs:69-106`) [E]. Toda ruta
  nueva de creacion que componga un sobre lo actualiza.
- **Guardas de texto del Plugin** (el Plugin no se ejecuta en pruebas): `PushBackRoundTripSourceGuardTests`,
  `CantileverPluginSourceGuardTests`, `LinkedPropertyViewPayloadTests`, `RackUnitsGuardSourceTests`,
  `KindHandlerGuardSourceTests` [A].
- **Cotas por vista**: T01-T21 de I-50 (`DimensionViewPolicyTests`, `*DimensionViews*Tests`, `DimensionViewScenarios`) [A].
- **Inventario y copia**: `RackListBuilderTests`, `SelectiveBomAuthorityTests`, `RackDuplicationPlanTests` [A].
- **Huecos** [A salvo marca]: ninguna prueba ejecuta jig, prompts ni cancelaciones; ninguna compara las
  `CustomProperties` de una vista nueva con sus hermanas; ninguna cubre un rack solo-planta en `RACKLISTA` ni
  «frontal ×N»; la prueba del lateral de Push Back usa un diseno **sin** fronteras suprimidas [E] (H-01).

## 19. Superficie y cruces, por archivo

Archivos que una Proposal de creacion de vistas previsiblemente leeria o tocaria, agrupados; **no** es lista de
cambios (la fija G2):

| Grupo | Archivos | Activas que los tocan |
|---|---|---|
| Contrato UI → Plugin | `U/Editor/RackEditorSession.cs`, `RackInsertionRequest.cs`, `RackEditorIdentity.cs`, `EditorModules.cs`; `U/RackMainMenuWindow.xaml.cs` | ninguna |
| Ventanas (tres son archivos calientes) | `U/Systems/Selective/RackSelectiveWindow.xaml(.cs)`, `U/Systems/Dynamic/RackDynamicSystemWindow.xaml(.cs)`, `U/Systems/PushBack/RackPushBackSystemWindow.xaml(.cs)`, `U/Systems/Cantilever/RackCantileverWindow.xaml(.cs)`, `U/RackFrames/RackFrameConfiguratorWindow.xaml(.cs)`, `U/Systems/FlowBed/RackFlowBedWindow.xaml.cs` | **I-53D** (Dinamico, hoy); **I-49** G10 previsto (Selectivo, `docs/ROADMAP.md:471`) |
| Comandos por sistema (calientes) | `P/RackSelectivoCommands.cs`, `RackDinamicoCommands.cs`, `RackPushBackCommands.cs`, `RackCantileverCommands.cs`, `RackCabeceraCommands.cs`, `RackCamaCommands.cs`, `RackMenuCommands.cs`, `RackCommandSupport.cs` | ninguna hoy |
| Materializacion | `P/Systems/Shared/ViewBlockDraw.cs`, `SystemBlockWriter.cs`, `P/Drawing/BlockPlacement.cs`, `LateralHeaderDrawService.cs`, `Cantilever/CantileverViewMaterializer.cs` | ninguna hoy; **I-52** caracteriza su contexto de creacion |
| Identidad e inventario | `A/Persistence/RackEmbedDocument.cs`, `RackEmbedComposer.cs`, `RackListBuilder.cs`, `RackDuplicationPlan.cs`; `A/Bom/BomAuthoredAuthority.cs`; `P/RackBlockFinder.cs`, `RackInventarioCommands*.cs`, `RackLayoutCommands*.cs`, `RackDuplicarCommands.cs`, `RackEnvelopeRestamp.cs`, `RackCloner.cs` | ninguna hoy; **I-52** (RACKMIRROR) reutilizara la copia |
| Variables y propiedades | `P/ProjectVariableMutationExecutor.cs`, `P/CustomPropertiesExecutor.cs` | ninguna hoy |

## 20. Preguntas abiertas

**Para el Owner (producto).** El codigo y los contratos vigentes **no** determinan estas respuestas.

| # | Pregunta | Por que G1 no puede cerrarla |
|---|---|---|
| OQ-1 | ¿Que resultado de producto persigue I-55? ¿Corresponde a **ID17** (*first-view freedom*), **ID18** (*multi-view queue*), **ID19** (*multi-rack projection*), a varios o a otro? | La orden no cita ningun ID; esos tres solo aparecen como exclusiones de I-50 (`docs/initiatives/I-50-cotas-independientes-por-vista.md:154-156`) y su texto no esta versionado |
| OQ-2 | ¿La primera vista de un rack nuevo debe ser **uniforme** entre sistemas, y cual? | Hoy Selectivo, Dinamico y Cabecera la restringen y Push Back y Cantilever no (6.2); ADR-0010 excluye la insercion inicial |
| OQ-3 | ¿Una operacion debe poder crear **varias vistas** de un rack (o de varios racks)? Si si, ¿que queda si el usuario cancela a mitad? | Hoy una vista por gesto (6.3) |
| OQ-4 | ¿Debe impedirse, advertirse o permitirse una segunda vista con la **misma** `(RackId, View, Section)`? | Hoy es legal y silenciosa (4.4) |
| OQ-5 | Al insertar una vista enlazada, ¿debe exigirse que las hermanas coincidan (authored del Selectivo, `CustomProperties`) o basta con copiar las de la vista elegida? | Hoy se copia sin comparar (10, 12) |
| OQ-6 | ¿«Insertar» debe ser atomico con el redibujo de las existentes (cancelar la nueva ⇒ no redibujar)? | Hoy no lo es (7.2, 17) |
| OQ-7 | ¿I-55 debe ofrecer **completar vistas** de un rack que nacio con una sola, como las celdas independientes de `RACKLAYOUT` (solo planta)? | Hoy solo `RACKEDITAR` (15) |

**Candidatas a decision arquitectonica material (para Arquitecto, antes de G2)**, solo si la Proposal toca el area:

- **A-1 Contrato de identidad de vista**: si la terna `(RackId, View, Section)` pasa a ser clave con reglas
  (unicidad, `View` nulo F-04, codificacion de `Section` por sistema) o sigue siendo descriptiva.
- **A-2 Contrato de materializacion compartido**: hoy cinco rutas de insercion con prompts en el Plugin o en la
  ventana y dos formas de colocar (`ViewBlockDraw` y Cantilever); cualquier unificacion cruza el
  `MaterializationContext` de **I-52** y el censo de `Compose` de I-54.
- **A-3 Atomicidad multivista**: redibujo por vista con commit propio frente a PREPARE/MUTATE de una transaccion, que
  ya existe para `RACKVARIABLES` (7.2, 13).
- **A-4 Correspondencia vista → `DimensionViewKind`**: implicita por builder (11); una variante nueva no tiene guarda.

## 21. Hallazgos laterales (registrados; **no** se arreglan en I-55)

| # | Hallazgo | Evidencia | Clase |
|---|---|---|---|
| H-01 | **Push Back, lateral: posicion de lista usada como indice de poste.** La ventana llena el combo con 1..`LateralCortes.Count` y envia `SelectedIndex` como `Section`; la vista previa elige `cortes[section]`. Pero `Cortes` **salta** las fronteras suprimidas por frentes en blanco (I-33) conservando `PostIndex`, y el Plugin dibuja `Build(system, catalog, postIndex: section)` y al editar empareja por `PostIndex`. Con alguna frontera suprimida, el bloque dibujado no es el corte de la vista previa, y una posicion que coincida con un poste suprimido quedaria obsoleta al siguiente `RACKEDITAR` | `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3036-3043`, `:3065`, `:3150-3155`; `A/Systems/PushBack/PushBackSystemLateralBuilder.cs:91-114`; `P/Systems/PushBack/PushBackSystemDrawService.cs:35`; `P/RackPushBackCommands.cs:118-122`, `:284-290`; la prueba `PushBackEditorWindowTests.cs:403-426` usa un diseno sin supresion | [E] codigo; consecuencia [I]; sin verificar en AutoCAD |
| H-02 | **Cantilever, planta: la visibilidad de brazos y tensores no llega al dibujo.** El diseno la persiste y la vista previa la usa, pero el Plugin llama al builder sin ella y este cae al valor por defecto (apagados) | `P/RackCantileverCommands.cs:131`, `:312`; `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:248-253`, `:285-295`; `U/Systems/Cantilever/RackCantileverWindow.xaml.cs:940-943`; `D/Systems/Cantilever/CantileverLineDesign.cs:303-318` | [E] codigo; sin verificar en AutoCAD |
| H-03 | **Cantilever: un componente pedido durante `RACKEDITAR` se pierde en silencio**, junto con lo aceptado en su configurador | `U/Systems/Cantilever/RackCantileverWindow.xaml.cs:176-195`; `P/RackCantileverCommands.cs:59-63` frente a `:214-217` | [E] codigo |
| H-04 | **Selectivo: `BindingIntent` nunca se asigna** en `src/`; la rama de `EditSelective` que lo consume es inalcanzable | `U/Systems/Selective/RackSelectiveWindow.xaml.cs:2882`; `P/RackSelectivoCommands.cs:93-99` | [E] |
| H-05 | **Mensaje de jig cancelado inexacto**: dice «bloque '…' creado» cuando la definicion ya se borro | `P/RackCommandSupport.cs:182-185`; `P/RackCabeceraCommands.cs:331-334`; `P/Drawing/BlockPlacement.cs:37-42` | [E] |
| H-06 | `LateralHeaderDrawService.DrawAt` **no tiene llamadores**; su comentario le atribuye el trazado de los cortes del Selectivo | `P/Drawing/LateralHeaderDrawService.cs:159-194` | [E] |
| H-07 | Comentarios de conteo desfasados: `SystemRegistry.Default` dice seis kinds y registra siete; tambien `SystemRegistry.cs:11` y `EditorModuleRegistry.cs:13` [A]. Amplia **F-12** | `A/Systems/Shared/SystemRegistry.Default.cs:20` frente a `:23-39` | [E] |
| H-08 | **Cabecera: `EditCabecera` acuna un GUID nuevo** si el sobre elegido no tiene `Id`; los demas editores usan el id de la ventana | `P/RackCabeceraCommands.cs:233`; seccion 4.6 | [E] |
| H-09 | **Dinamico: vista desconocida convertida en lateral** (entero si `section < 0`), cuando Push Back y Cantilever la rechazan; latente, ningun llamador actual la alcanza [A] | `P/RackDinamicoCommands.cs:124-132`; `P/RackPushBackCommands.cs:123-127`; `P/RackCantileverCommands.cs:112-116` | [E] |
| H-10 | **Aviso de unidades doble** al insertar un componente Cantilever desde el menu | `P/RackMenuCommands.cs:46-49`; `P/RackCantileverCommands.cs:425` | [E] |
| H-11 | **Guias desalineadas**: `docs/guias/modelo-de-datos.md:190-193` dice que dinamico y cama dibujan solo LATERAL y que lateral/planta solo vienen de `RACKEDITAR`, y describe `Section` solo como corte del Selectivo; `docs/guias/validacion-manual-autocad.md:115-121` omite Push Back y Cantilever | citas | [E] |
| H-12 | `RACKLISTA` mostraria «frontal ×N» para un Selectivo de N fondos | `A/Persistence/RackListBuilder.cs:104-114`; `P/RackSelectivoCommands.cs:177` | [I], sin prueba |

Ya registrados y **no** duplicados: **F-01** (comentario de `RackBlockData`), **F-04** (`View` nulo), **F-05** (kind con y
sin mayusculas; ademas `RACKRELLENAR` no comprueba el kind [A]) y **F-12** (comentario de regeneracion de Push Back,
confirmado en 9.1).

## 22. Cierre de G1

- **Entregable**: este informe, con la matriz sistema × ViewKind × variante (seccion 5) y evidencia `archivo:linea`.
- **Produccion**: intacta. El commit de G1 solo toca `docs/initiatives/I-55-*.md`.
- **Estado**: `SUBSTANTIVE IMPLEMENTATION: BLOCKED`. La Proposal y el consenso Coordinador ↔ Arquitecto requieren
  orden propia.
- **Escalado**: OQ-1..OQ-7 al Owner via Coordinador **antes** de G2; A-1..A-4 al Arquitecto si la Proposal toca esas
  areas; H-01 y H-02 son posibles defectos de dibujo que conviene que el Coordinador valore por separado, sin
  corregirlos en esta iniciativa sin una decision explicita.
- **Cruces**: I-53D material si G2 toca la ventana del Dinamico; I-49 G10 previsto sobre la ventana del Selectivo;
  I-52 semantico en materializacion. Se re-miden al abrir G2.

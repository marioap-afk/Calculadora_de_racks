# AGENTS.md — guia para agentes (Codex, Claude, etc.)

RackCad: plugin de AutoCAD 2025 (.NET 8, C#/WPF) para disenar y dibujar racks industriales con BOM.
Este archivo contiene SOLO convenciones estables. El estado vivo del proyecto esta en
[docs/HANDOFF.md](docs/HANDOFF.md); la vista general en [README.md](README.md).

## Leer primero

1. [docs/HANDOFF.md](docs/HANDOFF.md) — estado actual, bugs conocidos, siguientes tareas.
2. [README.md](README.md) — que es, comandos de AutoCAD, build, pruebas.
3. [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — arquitectura vigente y objetivo; cargar despues
   los [Context Packs](docs/context-packs/README.md) declarados por la iniciativa.
4. Verificar el estado REAL con `git log --oneline -10` y `dotnet test` antes de asumir nada.

## Mapa de carpetas

| Ruta | Responsabilidad |
|---|---|
| `src/RackCad.Domain` | Modelo puro (net8.0). Sin dependencias. |
| `src/RackCad.Application` | Geometria, resolvers, builders de vistas, BOM, persistencia, catalogos. Puro y testeable. |
| `src/RackCad.UI` | Ventanas WPF (net8.0-windows). NO referencia AutoCAD. |
| `src/RackCad.Plugin` | UNICO proyecto que toca la API de AutoCAD (comandos, drawers, jigs, embed en DWG). |
| `assets/catalogs/` | CSV/JSON de datos (fuente de verdad de perfiles, bloques, seguridad). |
| `tests/RackCad.Tests` | xUnit sobre Domain + Application (**suite Core**); corre sin AutoCAD y sin Windows. |
| `tests/RackCad.UI.Tests` | xUnit sobre los controles y las ventanas WPF (**suite UI**, net8.0-windows); exige Windows y un hilo STA. |
| `deploy/` | Bundle del Autoloader (`install-bundle.ps1`). |

## Comandos canonicos

```powershell
dotnet build RackCad.sln -v:minimal                                   # build completo
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj                  # suite Core
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj            # suite UI (solo Windows)
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug        # el DLL que se prueba con NETLOAD
```

**Hay DOS suites automatizadas y ambas son canonicas.** No existe un unico comando de pruebas: cuando
un gate exige la validacion automatizada completa, exige **las dos** (ver «Pruebas — definicion de
terminado»). La duracion de cada suite **se mide, no se declara**: este documento no fija ninguna
constante de tiempo, porque un numero copiado envejece sin que nadie lo note. Esta regla habla de las
**suites automatizadas**; no alcanza a la duracion activa del dueño, que por diseno solo se admite
declarada por el (guia de validacion manual §8).

- No hay lint/formatter configurado; el compilador C# es el type-check. Meta: **0 errores, 0 advertencias**
  propias (los `MSB3277` de las referencias de AutoCAD en el Plugin son conocidos y se ignoran).
  - **La meta no cambia**, y hoy no se cumple. Los dos `CS0105` que I-45 midió contra el log del CI
    —el caso que documenta `I-45-discovery.md` §8.4— **ya están corregidos** (gate G0B). Lo que sigue
    vivo es la deuda de **advertencias de analizadores xUnit en las suites de prueba**, registrada con
    su cifra, su método de medición y el motivo de haberla diferido en
    [`docs/ideas-futuras.md`](docs/ideas-futuras.md). **No es una excepción permitida**: es deuda con
    registro, y corregirla reescribe aserciones —trabajo propio, no higiene de paso—.
- **Trampa**: con AutoCAD abierto y el plugin cargado, los DLL del bin quedan bloqueados y el build falla
  en el paso de copia (MSB3021/MSB3027). Para validar solo codigo: compilar a una carpeta temporal
  (`dotnet build src/RackCad.UI/RackCad.UI.csproj -o <temp>`) y correr las pruebas. El procedimiento
  manual completo vive en [docs/guias/validacion-manual-autocad.md](docs/guias/validacion-manual-autocad.md).

## Convenciones arquitectonicas (obligatorias)

1. **Direccion de dependencias**: Domain <- Application <- UI <- Plugin. Nada de AutoCAD fuera del Plugin;
   nada de WPF fuera de la UI. La geometria y el BOM se implementan PUROS en Application con pruebas.
2. **Regla en un solo sitio**: cuando el dibujo, el BOM y la UI deben coincidir en un numero, la regla vive
   en UNA funcion de Application que todos consumen (ej.: `SelectiveFrontalBuilder.ParrillaRow`). Nunca
   duplicar la aritmetica en la UI ni en el BOM.
3. **Flags de seguridad del selectivo — copia centralizada**: todo campo nuevo en `SelectiveSafetySelection`
   (Domain) DEBE copiarse en `SelectiveSafetySelection.DeepCopy`; resolver, vista por fondo y UI consumen esa copia.
   Ademas debe mapearse explicitamente en `SelectivePalletDesignDocument` From/ToDomain (persistencia), con fallback
   legacy en el DTO nullable y test de round-trip. Omitir cualquiera de esos dos limites rompe en silencio.
4. **Persistencia versionada**: los DTO (`*Document`) llevan campos nullable para compatibilidad con
   documentos viejos; todo campo nuevo define su fallback legacy y se cubre con test de round-trip.
5. **Bloques de AutoCAD**: un bloque por pieza y vista (`blocks.csv`, `blockName` = nombre EXACTO del DWG).
   Parametros dinamicos por nombre case-insensitive. Si un stretch funciona a mano pero no por API (y otros
   bloques si), el problema es el BLOQUE (direccion del grip), no el codigo.
6. **Performance en insercion**: fijar parametros dinamicos por referencia es lento; usar el patron ARRAY
   (definicion anidada compartida). No reconstruir controles WPF por clic; regen UNICO en edits multi-vista.
7. **Catalogos**: renombrar una columna CSV exige renombrar la propiedad C# correspondiente. Encoding con
   fallback Latin-1 (se editan en Excel). La cache es por mtime.

## Convenciones de codigo

- C# idiomatico del repo: clases selladas, structs readonly para geometria, comentarios XML-doc en ingles
  que explican RESTRICCIONES (por que, no que); mensajes de UI en espanol; docs en espanol sin acentos
  (los archivos historicos) — imitar el estilo del archivo que se toca.
- Terminologia del dominio: "frente" (no "bahia"), "fondo" (linea de profundidad), "tramo" (medio frente),
  "claro" (span), "troquel" (rejilla de perforaciones), "peralte".
- Commits: espanol, asunto `Area: que cambio` (ver `git log`); cuerpo explicando el porque. Sin
  Conventional Commits.

## Flujo Git multi-agente

Proceso completo (ramas, worktrees, integracion, archivos calientes, limpieza): [docs/WORKFLOW.md](docs/WORKFLOW.md).
Plan de iniciativas: [docs/ROADMAP.md](docs/ROADMAP.md). Decisiones: [docs/adr/](docs/adr/README.md).
`WORKFLOW.md` es la autoridad de reclamo, ramas/worktrees, sesiones/rebase, ubicacion de evidencia,
cadencia documental, integracion, ruta R, post-merge, transicion, tags y limpieza. No se copian aqui
sus procedimientos. El ciclo de Discovery, arquetipos, Freeze/A-n, gates, READY y conformidad vive en
[docs/INITIATIVE_LIFECYCLE.md](docs/INITIATIVE_LIFECYCLE.md). Todo commit de agente conserva el trailer
de identificacion (`Co-Authored-By`), tambien los de Codex.

## Dependencias

**Politica general: cero paquetes NuGet en el codigo de producto** (el export XLSX es OOXML escrito
a mano por esta razon). La unica excepcion es la referencia condicional compile-only de
`RackCad.Plugin` a `AutoCAD.NET [25.0.1]` y sus versiones transitivas fijadas, gobernada por
[ADR-0003](docs/adr/0003-referencias-autocad-para-ci.md). No cubre runtime, distribucion ni otros
paquetes; cualquier cambio material exige nueva revision. Fuera de esa excepcion, solo el proyecto
de tests usa paquetes (xunit, Test SDK). No agregar dependencias sin acuerdo explicito del usuario.

## Pruebas — definicion de terminado

**Autoridad y vigencia.** Esta seccion gobierna clases de prueba/evidencia, composicion Full,
identidad exact-SHA, reutilizacion e invalidadores. Workflow V1 sigue efectivo hasta que
`WORKFLOW_V2_EFFECTIVE_SHA` exista conforme a [WORKFLOW §11](docs/WORKFLOW.md). Las reglas marcadas
V2 quedan materializadas pero dormidas hasta entonces y solo se aplican a unidades clasificadas V2;
I-56 y los reclamos formales existentes de I-49, I-52 e I-55 conservan V1. Las salvaguardas no
marcadas por workflow —Full del Candidato, clases separadas, exact-SHA, seleccion mayor que cero y
prueba conductual— se aplican siempre.

Un cambio de comportamiento esta terminado cuando:

1. **Validacion automatizada completa** verde: **suite Core + suite UI**, todas las pruebas y no solo
   las nuevas. Eso es lo que significa «Full» cuando un gate lo exige, y es una definicion de QUE se
   ejecuta, no de DONDE. **El reparto entre la corrida local y la del CI es el de la tabla de abajo.**
   ([ADR-0033](docs/adr/0033-validacion-por-clase-de-evidencia-y-sha-exacto.md) es el **origen** de esta
   regla, pero sigue en estado `propuesto`: vale como origen, no como autoridad, y este punto **se
   sostiene solo**. Donde ambos difieran, **manda este punto**, que es el mas estricto de los dos.)

   | Momento | Core local V1 (vigente) | Core local V2 (tras activacion) | UI local V1/V2 |
   |---|---|---|---|
   | **Push interno de implementacion** | Full antes de cada push de comportamiento | Focales/relevantes; Core Full **no obligatorio** antes de cada push interno | UI Full **no obligatoria** antes del push por LC-UI; pruebas UI focales/relevantes siguen segun el comportamiento |
   | **Cierre de gate funcional V2** | No aplica como regla V2 | **Core Full obligatorio**, despues del commit, con arbol limpio y sobre el SHA de cierre | LC-UI se conserva; UI Full solo si el gate exige Full |
   | **`FINAL_CANDIDATE_SHA`** | **Core Full obligatorio** | **Core Full obligatorio** | **UI Full obligatoria** |
   | **Cierre documental de iniciativa** | **Core Full obligatorio** | **Core Full obligatorio** | **UI Full obligatoria** |
   | **Punto que exija Full** | **obligatoria** | **obligatoria** | **obligatoria** |

   **Iteracion interna V2.** Exige pruebas focales del comportamiento en curso con RED→GREEN y conteo,
   pruebas relevantes del sistema/componente y los builds/guardias que aporten evidencia necesaria.
   El CI completo sigue corriendo en cada push. Esta cadencia moderada termina en Core Full local al
   cierre de **cada** gate funcional; no adopta el modelo «Core Full solo en el Candidato».
   El cierre documental conserva el Full que exige `WORKFLOW.md`: una corrida anterior solo se reutiliza
   si el cierre es literalmente el mismo SHA y cumple clase, proposito, alcance e invalidadores; un SHA
   de cierre nuevo repite la evidencia requerida.

   **LC-UI se conserva.** La suite completa de UI no es obligatoria en local antes de cada push. Es un
   **reparto de responsabilidad declarado, no una equivalencia**: UI local y UI del CI son clases
   distintas, y fuera de esta regla ninguna sustituye a la otra. «UI Full local no obligatoria» no
   significa «UI no se valida»: se ejecutan las pruebas focales/relevantes que correspondan y el CI
   ejecuta la suite UI completa sobre cada punta empujada.

   **El nucleo queda fuera de esta regla, y no por olvido.** Core local y Core del CI son clases
   distintas —el CI corre en ubuntu y el bucle local en Windows, y hay pruebas cuyo resultado depende
   de bytes en disco y de la cultura del hilo—. **No existe ninguna regla que permita que el Core del
   CI sustituya al Core local**, y no se deduce ninguna por analogia con LC-UI.

   **La exencion es previa al push; la evidencia es posterior.** No hay que correr la suite de UI en
   local antes de empujar. Lo que decide si ese SHA **tiene** evidencia de UI es lo que ocurra despues:
   si su corrida de CI pasa, la tiene; si esa corrida no existe o el job no queda en verde, **ese SHA
   se queda sin evidencia de UI** y hay que resolverlo antes de que pueda ser Candidato. No se
   posterga la validacion: se traslada de canal.

   **Evidencia CI de rama y Candidato.** Una corrida acredita el SHA solo cuando se verifican juntas:

   ```
   event               = push
   ref                 = refs/heads/<rama exacta>
   head_sha            = el SHA evaluado, exacto
   jobs requeridos     = success
   ```

   La `ref` se consulta en la corrida; no se infiere del `head_sha`. Para evidencia UI, entre esos jobs
   debe estar `ui-tests` (publicado como "UI Tests (WPF controls, net8.0-windows)") en `success`. NO
   basta el workflow en verde si un job requerido no corrio, ni el mismo SHA en otra rama, ni un commit
   anterior o posterior. Una corrida de `refs/tags/*` no acredita rama, Candidato ni post-merge. El
   criterio post-merge de `main` pertenece a `WORKFLOW.md` y solo se referencia desde aqui.
   Los jobs requeridos actuales son `Tests (Domain + Application)`,
   `UI Tests (WPF controls, net8.0-windows)`, `Build UI (WPF, valida API de Application)` y
   `Build Plugin without AutoCAD`; todos deben terminar en `success`.

   **`event = push` no es un detalle.** Es lo unico que ata la ejecucion al `head_sha`. Una corrida de
   **`workflow_dispatch`** lleva como `head_sha` la punta del ref despachado y ejecuta el commit que
   le pasaron por input: su `head_sha` **no es el SHA medido**. Por eso una corrida de despacho **NO
   acredita evidencia de UI a ningun SHA por su `head_sha`** —ni al tip, ni al medido—, no sustituye a
   la corrida de `push` del Candidato y no se propaga. Mide cobertura; eso es todo lo que hace.
   La politica, poblacion, disparadores, cadencia y semantica de dispatch de cobertura quedan **sin
   cambio**.

   **Push agrupado: la evidencia no se propaga.** Si un push lleva `A → B → C` y Actions corre solo
   sobre `C`, solo `C` recibe evidencia de CI de rama; `A` y `B` no reciben ninguna. No se
   hereda hacia atras ni hacia delante. No es formalismo: I-45 midio que los commits sin corrida
   propia son, en su mayoria, fases **rojas** de TDD empujadas junto a su verde — heredarles el verde
   del tip afirmaria que un commit rojo por diseno estaba verde.

   **SHA sin evidencia de UI.** Un SHA sin corrida local de UI **y** sin corrida propia de CI de UI es
   un SHA sin evidencia de UI, y **no puede ser Candidato**. Es condicion **necesaria, no suficiente**:
   tener evidencia de UI del CI no convierte a un SHA en Candidato — para eso hace falta ademas todo
   lo del parrafo siguiente, **incluida la UI Full local**.

   **`FINAL_CANDIDATE_SHA`** es el SHA exacto que se entrega para validar o integrar. Exige, sobre
   **ese** SHA: Core Full local, **UI Full local**, build Debug de UI, build Debug de Plugin, CI de
   push de la rama exacta con todos los jobs requeridos verdes, la cobertura vigente y Owner Validation
   donde aplique. CI no sustituye suites locales; suites locales no sustituyen CI; Owner Validation no
   sustituye evidencia automatizada. LC-UI **no reduce el Candidato** ni el Full final. La forma de
   declararlo esta en
   [docs/guias/validacion-manual-autocad.md](docs/guias/validacion-manual-autocad.md) §7.1.

   **LC-UI no cambia QUE valida el dueño.** «Sobre ese SHA» rige para las cuatro evidencias
   automatizadas y para el CI; cuando la validacion manual puede reutilizarse lo decide la seccion
   siguiente, no LC-UI.

2. Todo bugfix lleva **test de regresion verificado FALLANDO** con el fix desactivado (un test que nunca se
   vio fallar no prueba nada).
3. Build de UI + Plugin en Debug con 0 errores (el usuario prueba via NETLOAD del Debug, no del Release).
4. Documentacion de comportamiento visible actualizada. Su momento, ubicacion, cierre y evidencia se
   rigen exclusivamente por [WORKFLOW §8 y §11.4](docs/WORKFLOW.md).
5. Owner Validation sigue siendo una clase independiente cuando la naturaleza del cambio la activa;
   metadata `false` no exime. Escenarios, ejecucion sobre `FINAL_CANDIDATE_SHA`, reutilizacion manual,
   identidad del DLL y registro pertenecen a la
   [guia de validacion manual](docs/guias/validacion-manual-autocad.md). La secuencia de integracion
   pertenece a `WORKFLOW.md`.

### Reutilizacion de evidencia: **SHA exacto, y nada mas**

Una evidencia ya producida se reutiliza **solo** si recae sobre **el mismo SHA exacto**. No autorizan
reutilizar nada: el mismo arbol, los mismos archivos, el mismo diff, la misma rama, que `main` no
haya avanzado, ni que `tree(merge) == tree(segundo padre)`. No existe atajo por `patch-id`, `tree-id`
o equivalencia de contenido.

**Por que.** `Directory.Build.targets` estampa el SHA del commit en `InformationalVersion` —siempre
que el build vea el repositorio; el fallback documentado para un build fuera de un checkout deja la
version sin sufijo, y ese caso no es el nuestro—. Dos commits con el mismo arbol producen por tanto
**ensamblados distintos**, asi que **igualdad de arbol no es identidad de binario**. Una regla que
reutilizara por arbol estaria afirmando de un binario algo que solo se comprobo de otro.

**La regla.** Si una evidencia requerida (1) ya existe sobre ese mismo SHA exacto, (2) es de la misma
clase y alcance, (3) sigue valiendo para el mismo proposito y (4) no la invalido nada de la lista de abajo,
entonces **cruzar otro gate administrativo no obliga a repetirla por ceremonia**. El objetivo es
**una ejecucion de suite completa por Candidato**, salvo invalidacion explicita.

**Las clases no se sustituyen entre si**, nunca: Core local ≠ Core del CI; UI local ≠ UI del CI;
build Debug ≠ build del CI; validacion del dueño ≠ cualquier evidencia automatizada. LC-UI es una
**excepcion normativa explicita** de reparto durante la iteracion ordinaria, **no** una equivalencia
inferida, y **no se generaliza**.

**Invalidadores, y solo estos.** Ademas de cambiar el SHA:

- **evidencia local** — el **arbol de trabajo estaba sucio** al producirla. No se puede atribuir
  limpiamente a ese SHA.
- **evidencia dependiente del SDK** — cambio el **SDK resuelto**. `8.0.x` o un `global.json` sin
  tocar **no** demuestran que el SDK resuelto sea el mismo: `8.0.x` es un pin **flotante**.
- **evidencia del dueño en AutoCAD** — cambio la **version de AutoCAD** o la **biblioteca externa de
  bloques**.

La lista no se amplia especulativamente.

Cambiar codigo de producto, pruebas, entradas de build o configuracion sensible versionada crea otro
SHA: la evidencia permanece en el commit que midio y no se traslada al nuevo. Cambiar el
`FINAL_CANDIDATE_SHA` invalida por identidad todas las afirmaciones ligadas al Candidato anterior,
aunque el arbol, patch-id o contenido fuente resulten equivalentes. Los cambios externos se tratan
por la clase afectada y los invalidadores enumerados, nunca mediante equivalencia de contenido.

**Validacion del dueño.** Se reutiliza si y solo si: **mismo SHA exacto** + **misma version de
AutoCAD** + **misma biblioteca de bloques**, y para el mismo proposito y alcance. Si cambia el SHA,
**no es reutilizable automaticamente**, aunque el arbol sea identico.

**Rebase.** Un rebase crea SHAs nuevos: la evidencia previa al rebase **no valida** el SHA rebasado.
**Merge `--no-ff`.** El commit de merge `Y` no es el Candidato `X`: **el CI posterior al merge es
obligatorio**, aunque `tree(Y) == tree(X)`.

**Commit documental.** Un commit que solo toca documentacion **sigue siendo un SHA nuevo** y **no
hereda** la evidencia del anterior. La politica puede exigir sobre el **menos clases** de evidencia
—eso es una regla de que se exige, proporcional al cambio—, pero **no se llama reutilizacion** y no
se apoya en que «el binario no cambia», porque cambia.

En particular, un hijo documental o commit de cierre no hace que el Candidato padre adquiera evidencia
nueva: el CI del cierre mide y acredita solo el SHA del cierre, nunca al padre. El cierre cumple las
clases que `WORKFLOW.md` exija para el contenido real; solo reutiliza una ejecucion previa cuando
coinciden literalmente SHA, clase, proposito y alcance y no existe invalidante.

**Orden para la evidencia local de un Candidato**, en este orden y no en otro:

```
1. crear el commit Candidato
2. confirmar que el arbol esta limpio
3. ejecutar la evidencia requerida sobre ESE HEAD exacto
4. registrar el resultado contra ese SHA
```

Importa porque una suite ejecutada **antes** de crear el commit estampa `git rev-parse HEAD`, que en
ese momento es el **padre**: una frase «pruebas verdes» dentro del commit nuevo **no demuestra por si
sola** que validaran ese SHA. Si una correccion posterior crea otro SHA, el Candidato anterior queda
invalidado y el nuevo necesita sus propias evidencias.

**Mismo SHA, misma clase, mismo entorno, resultados incompatibles.** Eso **no refuta** la identidad
por SHA exacto: es una **senal de no determinismo**, y se trata como tal con el diagnostico del CI
—TRX, `--blame-hang`, volcados—. No se convierte en una equivalencia nueva.

**Registro historico, no precedente.** En `docs/HANDOFF.md`, en archivos de evidencia antiguos **y en
contratos de iniciativas ya cerradas** hay entradas que razonan «el commit de cierre no toca `src/`,
luego no cambia el binario, luego la aprobacion sigue vigente», a veces encadenadas como «con el mismo
criterio que I-31, I-35, I-39A…». Esa frontera **queda retirada** y **no es citable desde ninguno de
esos tres contenedores**: son registros de lo que se hizo entonces, no una regla. La premisa era
ademas falsa —el SHA se estampa—, y no se corrigen hacia atras.

**Lo que esta regla NO reclama.** I-45 midio **cero** reconfirmaciones por SHA exacto determinables en
su corpus, y el canal local historico es **UNKNOWN** porque nunca registro contra que SHA corrio. Esta
regla es **preventiva y de bajo coste**; **no** se le atribuye ningun ahorro historico ni ningun
porcentaje.

### Una seleccion de pruebas que no selecciona nada es un FALLO

Toda invocacion **filtrada o seleccionada** de pruebas —focal, de sistema, relevante, `--filter` o
cualquier mecanismo futuro de seleccion— debe **demostrar que selecciono al menos una prueba esperada**.

```
0 pruebas seleccionadas = FALLO
```

Nunca un exito. Un filtro que deja de coincidir con lo que nombraba —porque una clase se renombro o un
namespace se movio— pasa en verde sin ejecutar nada, y ese verde es indistinguible del de una suite que
si corrio. Por eso el resultado de una corrida focal solo vale acompanado del conteo que produjo.

Esta regla **es normativa desde aqui**. Hasta G0A se citaba como si ya viviera en este documento y no
estaba escrita en ninguna parte; varios documentos y una prueba la atribuian a `AGENTS.md`.

**La obligacion es de quien ejecuta, y de nadie mas: no existe mecanismo automatico que la haga
cumplir.** I-45 lo dejo asi a proposito y no construyo uno —el unico sitio donde la guarda es
automatica es `eng/validation/measure-validation.ps1`, y solo sobre sus propias corridas de
medicion—. Queda registrado como deuda en [`docs/ideas-futuras.md`](docs/ideas-futuras.md); un gate
futuro puede tomarlo, pero **ninguno lo tiene asignado**.

### Guardia de fuente y prueba de comportamiento

Una guardia de fuente, busqueda textual, inspeccion de IL o prueba estructural puede proteger una
frontera estatica, pero **no sustituye una prueba de comportamiento** cuando el invariante es
conductual. Una prueba cuyo oraculo no puede fallar al violar el invariante es un **oraculo ciego** y
no satisface la obligacion, aunque quede verde.

Cuando un gate introduce comportamiento nuevo, su RED→GREEN debe mostrar primero una condicion real
fallando antes de implementar la correccion o comportamiento, y despues el verde con la implementacion.
El Freeze/A-n define la obligacion invariante→prueba y el lifecycle define gates, READY y conformidad;
esta seccion define que evidencia de prueba cuenta.

- No hay secretos, tokens ni variables de entorno en este repo. Mantenerlo asi.
- `blocks-library.dwg` (biblioteca de bloques del usuario) NO se versiona; su ruta vive en
  `%APPDATA%\RackCad\settings.json`. No inventar bloques: los nombres reales los define el usuario.
- No editar a mano `bin/`, `obj/` ni los CSV copiados junto a los ensamblados (se copian del
  `assets/catalogs/` fuente en cada build).

## Fuentes de verdad

| Tema | Fuente |
|---|---|
| Estado del proyecto, bugs, backlog | `docs/HANDOFF.md` |
| Datos de perfiles/bloques/seguridad | `assets/catalogs/*.csv` (+ como editarlos: `docs/guias/catalogos-y-plantillas.md`) |
| Reglas de geometria del selectivo | `src/RackCad.Application/Systems/Selective/` (los XML-doc explican cada regla) |
| Modelo de datos / FKs de catalogos | `docs/guias/modelo-de-datos.md` |
| Despliegue / Autoloader | `docs/guias/despliegue.md` |
| Decisiones historicas | `docs/adr/` para decisiones vigentes; `docs/archivo/` solo para historia |

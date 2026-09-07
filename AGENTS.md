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
  - **La meta no cambia**, y hoy no se cumple: I-45 midió advertencias propias vivas contra el log del
    CI del mismo candidato. El hallazgo, con su evidencia, está en
    [`docs/initiatives/I-45-discovery.md`](docs/initiatives/I-45-discovery.md) §8.4 y **no se repite
    aquí**. No es una excepción permitida: su corrección corresponde al gate `G0B` de I-45 si el
    Coordinador confirma que es higiene no funcional.
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
Reglas rapidas: las ramas se nombran por INICIATIVA, nunca por herramienta (ADR-0001; la lista de
prefijos vive en WORKFLOW seccion 1 — no la copies); 1 iniciativa = 1 rama = 1 worktree (el MISMO
worktree toda la vida de la iniciativa, con sesiones secuenciales y relevo entre herramientas —
nunca dos sesiones activas a la vez); abrir = bifurcar de `origin/<trunk>` tras fetch + commit
vacio de reclamo (ID de iniciativa + Claim-Id UUID + Co-Authored-By) + `git push -u` sin force —
el primer push ACEPTADO es el reclamo; si el remoto ya tiene la rama, no forzar: borrar el reclamo
local y elegir otra iniciativa; rebase al abrir sesion si el trunk avanzo (republicar con
`--force-with-lease`); push de la rama al cerrar CADA sesion (push de rama != integrar); la
integracion es serializada (rebase final + CI + validacion + merge --no-ff, WORKFLOW seccion 4.5);
al cerrar, borrado SEGURO: `git branch -d` (nunca `-D` salvo los casos de WORKFLOW seccion 3) y el
remoto solo tras confirmar el merge en `main`; todo commit de agente lleva trailer de identificacion
(Co-Authored-By), tambien los de Codex. No copiar conteos de tests ni hashes de commit fuera de
`docs/HANDOFF.md` (seccion 12): los numeros copiados divergen.

## Dependencias

**Politica general: cero paquetes NuGet en el codigo de producto** (el export XLSX es OOXML escrito
a mano por esta razon). La unica excepcion es la referencia condicional compile-only de
`RackCad.Plugin` a `AutoCAD.NET [25.0.1]` y sus versiones transitivas fijadas, gobernada por
[ADR-0003](docs/adr/0003-referencias-autocad-para-ci.md). No cubre runtime, distribucion ni otros
paquetes; cualquier cambio material exige nueva revision. Fuera de esa excepcion, solo el proyecto
de tests usa paquetes (xunit, Test SDK). No agregar dependencias sin acuerdo explicito del usuario.

## Pruebas — definicion de terminado

Un cambio de comportamiento esta terminado cuando:

1. **Validacion automatizada completa** verde: **suite Core + suite UI**, todas las pruebas y no solo
   las nuevas. Eso es lo que significa «Full» cuando un gate lo exige, y es una definicion de QUE se
   ejecuta, no de DONDE. **El reparto entre la corrida local y la del CI es el de la tabla de abajo.**
   ([ADR-0033](docs/adr/0033-validacion-por-clase-de-evidencia-y-sha-exacto.md) es el **origen** de esta
   regla, pero sigue en estado `propuesto`: vale como origen, no como autoridad, y este punto **se
   sostiene solo**. Donde ambos difieran, **manda este punto**, que es el mas estricto de los dos.)

   | Momento | suite Core en local | suite UI en local |
   |---|---|---|
   | **Iteracion ordinaria** | sin cambio: LC-UI **no toca el nucleo** | **NO obligatoria** antes del push |
   | **Candidato** | **obligatoria** | **obligatoria** |
   | **Cierre, o cualquier gate que exija Full** | **obligatoria** | **obligatoria** |

   **Iteracion ordinaria (LC-UI).** La suite completa de UI deja de ser obligatoria en local en cada
   iteracion: la evidencia intermedia de UI la aporta el CI. Es un **reparto de responsabilidad
   declarado, no una equivalencia**: la corrida local de UI y la del CI **no son la misma clase de
   evidencia**, y fuera de esta regla ninguna sustituye a la otra. «Opcional en local» significa
   **opcional en local**, no «UI opcional»: la suite se ejecuta igual, en el CI.

   **El nucleo queda fuera de esta regla, y no por olvido.** Core local y Core del CI son clases
   distintas —el CI corre en ubuntu y el bucle local en Windows, y hay pruebas cuyo resultado depende
   de bytes en disco y de la cultura del hilo—. **No existe ninguna regla que permita que el Core del
   CI sustituya al Core local**, y no se deduce ninguna por analogia con LC-UI.

   **La exencion es previa al push; la evidencia es posterior.** No hay que correr la suite de UI en
   local antes de empujar. Lo que decide si ese SHA **tiene** evidencia de UI es lo que ocurra despues:
   si su corrida de CI pasa, la tiene; si esa corrida no existe o el job no queda en verde, **ese SHA
   se queda sin evidencia de UI** y hay que resolverlo antes de que pueda ser Candidato. No se
   posterga la validacion: se traslada de canal.

   **Que cuenta como evidencia de UI del CI**, y solo esto: el job **`ui-tests`** —el que el CI publica
   como `UI Tests (WPF controls, net8.0-windows)`— con `conclusion = success` sobre el **`head_sha`
   exacto** de ese commit. NO basta el workflow en verde si ese job no corrio, ni el mismo SHA en otra
   rama, ni un commit anterior, ni uno posterior.

   **Push agrupado: la evidencia no se propaga.** Si un push lleva `A → B → C` y Actions corre solo
   sobre `C`, solo `C` recibe evidencia; `A` y `B` tienen **evidencia de UI del CI = NINGUNA**. No se
   hereda hacia atras ni hacia delante. No es formalismo: I-45 midio que los commits sin corrida
   propia son, en su mayoria, fases **rojas** de TDD empujadas junto a su verde — heredarles el verde
   del tip afirmaria que un commit rojo por diseno estaba verde.

   **SHA sin evidencia de UI.** Un SHA sin corrida local de UI **y** sin corrida propia de CI de UI es
   un SHA sin evidencia de UI, y **no puede ser Candidato**. Es condicion **necesaria, no suficiente**:
   tener evidencia de UI del CI no convierte a un SHA en Candidato — para eso hace falta ademas todo
   lo del parrafo siguiente, **incluida la UI Full local**.

   **Candidato** es el SHA exacto que se entrega para validar o integrar. Exige, sobre **ese** SHA:
   Core Full local, **UI Full local**, build Debug de UI, build Debug de Plugin y CI verde sobre el
   SHA exacto; mas la validacion del dueño en AutoCAD donde aplique. LC-UI **no reduce el Candidato**,
   no reduce el cierre y no reduce el Full final: solo retira la repeticion local intermedia. La forma
   de declararlo esta en
   [docs/guias/validacion-manual-autocad.md](docs/guias/validacion-manual-autocad.md) §7.1.

   **LC-UI no toca el gate del dueño.** «Sobre ese SHA» rige para las cuatro evidencias automatizadas
   y para el CI; la validacion manual en AutoCAD conserva **exactamente** su regla de reutilizacion de
   hoy —si el trunk no avanzo desde una validacion previa, esa validacion sigue valiendo
   (docs/WORKFLOW.md seccion 4.5.3)—. Ni se relaja ni se endurece aqui.
2. Todo bugfix lleva **test de regresion verificado FALLANDO** con el fix desactivado (un test que nunca se
   vio fallar no prueba nada).
3. Build de UI + Plugin en Debug con 0 errores (el usuario prueba via NETLOAD del Debug, no del Release).
4. Documentacion tocada si cambio comportamiento visible (`docs/guias/catalogos-y-plantillas.md` para catalogos
   y elementos). `docs/HANDOFF.md` secciones 8-12 se actualizan **al INTEGRAR la iniciativa** (ultimo
   commit de la rama, en la sesion de integracion — docs/WORKFLOW.md seccion 4.5), nunca desde ramas
   paralelas; el cierre de una sesion intermedia se registra en el cuerpo del commit.
5. **No INTEGRAR features al trunk sin la verificacion manual del usuario en AutoCAD** (el dibujo real es
   el criterio final; los tests no ven los bloques DWG reales). El push de la RAMA de iniciativa es
   respaldo y se hace al cerrar CADA sesion (push de rama != integrado); la integracion a `main` espera
   la confirmacion del usuario (docs/WORKFLOW.md secciones 4 y 6). Cuando el dueño **ejecute** una de
   esas validaciones, preguntale en el mismo turno su duracion activa y registrala junto al veredicto:
   la definicion completa, la pregunta exacta y sus limites viven en
   [docs/guias/validacion-manual-autocad.md](docs/guias/validacion-manual-autocad.md) §8.
   Es una **metrica experimental de I-45 y NO es parte de este punto 5**: que el dato falte —o que
   nadie lo preguntara— no invalida la validacion, no cambia su veredicto, **no impide que el cambio
   este terminado y no bloquea la integracion**. No preguntar no incumple esta lista.

### Una seleccion de pruebas que no selecciona nada es un FALLO

Toda invocacion **filtrada o seleccionada** de pruebas —`--filter`, o cualquier mecanismo futuro de
seleccion— debe **demostrar que selecciono al menos una prueba esperada**.

```
0 pruebas seleccionadas = FALLO
```

Nunca un exito. Un filtro que deja de coincidir con lo que nombraba —porque una clase se renombro o un
namespace se movio— pasa en verde sin ejecutar nada, y ese verde es indistinguible del de una suite que
si corrio. Por eso el resultado de una corrida focal solo vale acompanado del conteo que produjo.

Esta regla **es normativa desde aqui**. Hasta hoy se citaba como si ya viviera en este documento y no
estaba escrita en ninguna parte; varios documentos y una prueba la atribuian a `AGENTS.md`. El
mecanismo que la haga cumplir automaticamente es trabajo de un gate posterior de I-45: por ahora la
obligacion es de quien ejecuta.

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

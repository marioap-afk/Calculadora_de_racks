# AGENTS.md — guia para agentes (Codex, Claude, etc.)

RackCad: plugin de AutoCAD 2025 (.NET 8, C#/WPF) para disenar y dibujar racks industriales con BOM.
Este archivo contiene SOLO convenciones estables. El estado vivo del proyecto esta en
[docs/HANDOFF.md](docs/HANDOFF.md); la vista general en [README.md](README.md).

## Leer primero

1. [docs/HANDOFF.md](docs/HANDOFF.md) — estado actual, bugs conocidos, siguientes tareas.
2. [README.md](README.md) — que es, comandos de AutoCAD, build, pruebas.
3. [docs/00-indice-contexto.md](docs/00-indice-contexto.md) — indice del resto de la documentacion.
4. Verificar el estado REAL con `git log --oneline -10` y `dotnet test` antes de asumir nada.

## Mapa de carpetas

| Ruta | Responsabilidad |
|---|---|
| `src/RackCad.Domain` | Modelo puro (net8.0). Sin dependencias. |
| `src/RackCad.Application` | Geometria, resolvers, builders de vistas, BOM, persistencia, catalogos. Puro y testeable. |
| `src/RackCad.UI` | Ventanas WPF (net8.0-windows). NO referencia AutoCAD. |
| `src/RackCad.Plugin` | UNICO proyecto que toca la API de AutoCAD (comandos, drawers, jigs, embed en DWG). |
| `assets/catalogs/` | CSV/JSON de datos (fuente de verdad de perfiles, bloques, seguridad). |
| `tests/RackCad.Tests` | xUnit sobre Domain + Application; corre sin AutoCAD y sin Windows. |
| `deploy/` | Bundle del Autoloader (`install-bundle.ps1`). |

## Comandos canonicos

```powershell
dotnet build RackCad.sln -v:minimal                              # build completo
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj             # pruebas (rapidas, <2 s)
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug   # el DLL que se prueba con NETLOAD
```

- No hay lint/formatter configurado; el compilador C# es el type-check. Meta: **0 errores, 0 advertencias**
  propias (los `MSB3277` de las referencias de AutoCAD en el Plugin son conocidos y se ignoran).
- **Trampa**: con AutoCAD abierto y el plugin cargado, los DLL del bin quedan bloqueados y el build falla
  en el paso de copia (MSB3021/MSB3027). Para validar solo codigo: compilar a una carpeta temporal
  (`dotnet build src/RackCad.UI/RackCad.UI.csproj -o <temp>`) y correr las pruebas.

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

**Politica: cero paquetes NuGet en el codigo de producto** (el export XLSX es OOXML escrito a mano por esta
razon). Solo el proyecto de tests usa paquetes (xunit, Test SDK). No agregar dependencias sin acuerdo
explicito del usuario.

## Pruebas — definicion de terminado

Un cambio de comportamiento esta terminado cuando:

1. `dotnet test` verde (todas las pruebas, no solo las nuevas).
2. Todo bugfix lleva **test de regresion verificado FALLANDO** con el fix desactivado (un test que nunca se
   vio fallar no prueba nada).
3. Build de UI + Plugin en Debug con 0 errores (el usuario prueba via NETLOAD del Debug, no del Release).
4. Documentacion tocada si cambio comportamiento visible (`docs/catalogos-y-plantillas.md` para catalogos
   y elementos). `docs/HANDOFF.md` secciones 8-12 se actualizan **al INTEGRAR la iniciativa** (ultimo
   commit de la rama, en la sesion de integracion — docs/WORKFLOW.md seccion 4.5), nunca desde ramas
   paralelas; el cierre de una sesion intermedia se registra en el cuerpo del commit.
5. **No INTEGRAR features al trunk sin la verificacion manual del usuario en AutoCAD** (el dibujo real es
   el criterio final; los tests no ven los bloques DWG reales). El push de la RAMA de iniciativa es
   respaldo y se hace al cerrar CADA sesion (push de rama != integrado); la integracion a `main` espera
   la confirmacion del usuario (docs/WORKFLOW.md secciones 4 y 6).

## Seguridad y datos

- No hay secretos, tokens ni variables de entorno en este repo. Mantenerlo asi.
- `blocks-library.dwg` (biblioteca de bloques del usuario) NO se versiona; su ruta vive en
  `%APPDATA%\RackCad\settings.json`. No inventar bloques: los nombres reales los define el usuario.
- No editar a mano `bin/`, `obj/` ni los CSV copiados junto a los ensamblados (se copian del
  `assets/catalogs/` fuente en cada build).

## Fuentes de verdad

| Tema | Fuente |
|---|---|
| Estado del proyecto, bugs, backlog | `docs/HANDOFF.md` |
| Datos de perfiles/bloques/seguridad | `assets/catalogs/*.csv` (+ como editarlos: `docs/catalogos-y-plantillas.md`) |
| Reglas de geometria del selectivo | `src/RackCad.Application/Systems/Selective*.cs` (los XML-doc explican cada regla) |
| Modelo de datos / FKs de catalogos | `docs/modelo-de-datos.md` |
| Despliegue / Autoloader | `docs/despliegue.md` |
| Decisiones historicas | `docs/arquitectura-autocad-racks.md` y demas historicos (marcados en el indice) |

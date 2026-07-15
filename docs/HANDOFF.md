# Project Handoff

> Documento canonico de continuidad entre sesiones (Claude, Codex o un desarrollador nuevo).
> Actualizado: **2026-07-14**, rama `release/claude-review`, commit `95e25f2`.
> Regla de mantenimiento: este archivo describe ESTADO y CONTEXTO; las convenciones estables viven en
> [AGENTS.md](../AGENTS.md) y la vista general en [README.md](../README.md). Al cerrar una sesion de trabajo
> significativa, actualizar las secciones 8-12 de este archivo.

## 1. Resumen ejecutivo

**RackCad** es un plugin de AutoCAD 2025 (.NET 8, C#/WPF) para **disenar y dibujar racks industriales**
y obtener su **BOM** (lista de materiales) para cotizacion. Lo usa el dueno del repo (ingenieria de racks)
como herramienta interna de dibujo tecnico.

Maneja **cuatro tipos de rack** (cabecera/marco, sistema dinamico pallet-flow, cama de rodamiento y
selectivo), cada uno con ventana editora WPF, dibujo por bloques en AutoCAD y **round-trip de edicion**
(seleccionar un rack dibujado y reabrir su editor con `RACKEDITAR`). El selectivo es el modulo mas rico:
matriz frentes x niveles dirigida por tarima, tres vistas (frontal/lateral/planta) ligadas por GUID,
doble profundidad, medio frente, cotas, elementos de seguridad y BOM por componentes con export CSV/XLSX.

**Estado**: activo y funcional. 489 tests verdes. Hay 4 commits locales por encima de `origin` (tope
medio-frente + parrilla) cuya verificacion manual en AutoCAD esta pendiente (ver seccion 9).

## 2. Estado comprobado

| Aspecto | Estado | Evidencia |
|---|---|---|
| Compilacion Debug (Domain/Application/UI/Plugin) | OK, 0 errores, 0 advertencias propias | `dotnet build` 2026-07-14; solo avisos MSB3277 conocidos en Plugin (refs AutoCAD) |
| Pruebas unitarias | **489/489 verdes** | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug`, 2026-07-14, Windows 11 / .NET SDK 10 (TFM net8.0) |
| Carga en AutoCAD (NETLOAD del Debug) | Verificado por el usuario hasta el commit `6b1f715` | Flujo de trabajo habitual del usuario |
| Commits `b1cfce2`..`95e25f2` (tope medio-frente, parrilla) | Implementado y testeado, **pendiente verificacion manual en AutoCAD** | Ver seccion 9 |
| Release build / bundle de despliegue | No reconstruido en la ultima sesion | `deploy/install-bundle.ps1 -Build` requiere AutoCAD cerrado |

## 3. Estado por funcionalidad

| Funcionalidad | Estado | Codigo principal | Pruebas | Observaciones |
|---|---|---|---|---|
| Cabecera (marco): editor + dibujo + round-trip | completo | `src/RackCad.UI/RackFrameConfiguratorWindow*`, `src/RackCad.Plugin/Headers/` | `tests/RackCad.Tests/*Frame*` | Peralte de placa editable por placa |
| Sistema dinamico (pallet flow) | completo | `RackDynamicSystemWindow`, `DynamicSystem*` | `Dynamic*Tests` | Patron ARRAY para perf |
| Cama de rodamiento (flow bed) | completo | `RackFlowBedWindow`, `FlowBed*` | `FlowBed*Tests` | Rodillos por paso minimo; capacidad = fase futura |
| Selectivo: matriz, 3 vistas, doble profundidad, medio frente, cotas | completo | `RackSelectiveWindow`, `src/RackCad.Application/Systems/Selective*` | `Selective*Tests` | El modulo mas activo |
| BOM por componentes + consolidado (`RACKBOMTOTAL`) + export CSV/XLSX | completo | `SelectiveBomBuilder`, `RackBomWindow`, `XlsxWriter` | `SelectiveBomBuilderTests`, `Xlsx*Tests` | XLSX es OOXML escrito a mano, sin dependencias |
| Seguridad: bota, protector lateral, separador, tope de larguero | completo | `SelectiveSafetyPlacement`, `SelectiveSafetyWindow`, `SafetyTopeGridWindow` | `SelectiveSafetyTests` | Catalogo en `assets/catalogs/seguridad.csv` |
| Seguridad: **parrilla / deck** (una por tarima, frente y cantidad manuales, cuenta en vivo) | completo (codigo) / **pendiente verificacion AutoCAD** | `SelectiveFrontalBuilder.ParrillaRow`, `SelectiveParrillaPlan`, `SafetyParrillaGridWindow` | `SelectiveSafetyTests` (Parrilla_*), `SelectiveMedioFrenteTests` | Ver secciones 8-9 |
| Tarima como referencia visual (frontal + lateral, sin BOM) | completo | `SelectiveFrontalBuilder.AddPallets`, bloque `TARIMA_GENERICA` | `SelectiveFrontalBuilderTests` | Planta = futuro |
| Identidad + round-trip (GUID en Xrecord, `RACKEDITAR`, `RACKDUPLICAR`, `RACKLISTA`) | completo | `RackEmbedDocument`, `RackFrameCommands` | round-trip tests por store | Convencion Actualizar/Insertar en las 4 ventanas |
| Layout de almacen v1 (`RACKLAYOUT`, `RACKRELLENAR`) | completo (v1) | `WarehouseGridPlanner`, comandos en Plugin | `Warehouse*Tests` | Solo motor de colocacion; el optimizador IA es meta futura |
| Bibliotecas (disenos + bloques DWG) | completo | `RackDesignLibrary*`, settings en `%APPDATA%\RackCad\settings.json` | — | `blocks-library.dwg` NO esta en el repo (ver seccion 6) |
| Validacion de cargas / capacidad | **diferido a proposito** | — | — | Ira con RAM Elements; NO re-proponer |
| Desviadores, poste tope, guardas traseras (seguridad) | pendiente | — | — | Siguientes elementos naturales |
| Rejilla de seguridad vs niveles resueltos (desfase) | **bug conocido, preexistente** | `RackSelectiveWindow.Safety_Click` | — | Ver seccion 10, item 1 |

## 4. Arquitectura

Cuatro proyectos en `RackCad.sln`, con dependencias en una sola direccion:

```
RackCad.Domain (net8.0, puro)          modelo: disenos, tarimas, selecciones
        ^
RackCad.Application (net8.0, puro)     geometria/calculo/BOM/persistencia; catalogos CSV; TESTEABLE
        ^
RackCad.UI (net8.0-windows, WPF)       ventanas editoras; NO referencia AutoCAD
        ^
RackCad.Plugin (net8.0-windows)        UNICO proyecto que toca la API de AutoCAD (adaptador delgado)
```

- **Flujo del selectivo** (el patron a imitar): `SelectivePalletDesign` (Domain, lo que el usuario tecleo)
  -> `SelectiveGeometryResolver.Resolve` -> `SelectiveRackSystem` (resuelto) -> builders puros
  (`SelectiveFrontalBuilder` / `SelectiveLateralBuilder` / `SelectivePlantaBuilder`) -> lista de
  `HeaderBlockInstance` -> drawers del Plugin que materializan bloques de AutoCAD.
- **Persistencia**: el diseno viaja EN el DWG — sobre `RackEmbedDocument` (JSON + GUID + nombre) en un
  Xrecord del diccionario de extension de la definicion de bloque. Stores por tipo
  (`SelectivePalletDesignStore`, `RackProjectStore`, `FlowBedConfigurationStore`) con version de esquema.
- **Catalogos**: CSV editables en Excel en `assets/catalogs/` (encoding con fallback Latin-1, cache por
  mtime). Se copian junto al ensamblado; `JsonRackCatalogProvider.FromBaseDirectory()` los resuelve en
  runtime. Detalle: [catalogos-y-plantillas.md](catalogos-y-plantillas.md) y [modelo-de-datos.md](modelo-de-datos.md).
- **Bloques**: cada pieza tiene un bloque por vista (`blocks.csv`); los que faltan se importan de una
  biblioteca DWG externa. Los parametros dinamicos se fijan por nombre case-insensitive
  (`ApplyDynamicParameters` + `RecordGraphicsModified`).
- Sin base de datos, sin servicios externos, sin red. Todo local.

## 5. Mapa del repositorio

| Ruta | Que es |
|---|---|
| `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | Modelo del selectivo, incl. `SelectiveSafetySelection` (flags de seguridad) |
| `src/RackCad.Application/Systems/` | Resolver, builders de vistas, BOM, geometria (todo lo interesante del selectivo) |
| `src/RackCad.Application/Persistence/` | Documentos DTO + stores (round-trip con fallbacks legacy) |
| `src/RackCad.Application/Catalogs/` | Carga de catalogos CSV/JSON -> `RackCatalog` |
| `src/RackCad.UI/` | Ventanas WPF (una por tipo de rack + dialogos de seguridad/BOM/biblioteca) |
| `src/RackCad.Plugin/` | Comandos de AutoCAD, drawers, jigs, embed en DWG |
| `assets/catalogs/` | Los CSV/JSON de datos (fuente de verdad de perfiles/bloques/seguridad) |
| `tests/RackCad.Tests/` | xUnit; corre sin AutoCAD ni Windows-only APIs |
| `deploy/` | Bundle del Autoloader + `install-bundle.ps1` |
| `docs/` | Documentacion; empezar por `00-indice-contexto.md` |

## 6. Configuracion y entorno

- **Requisitos**: Windows, .NET 8 SDK (o superior con TFM net8.0), AutoCAD 2025 completo (no LT) solo
  para probar el plugin. Las pruebas y los proyectos Domain/Application corren en cualquier OS.
- **Sin variables de entorno ni secretos.** La unica configuracion runtime es
  `%APPDATA%\RackCad\settings.json` (ruta de la biblioteca de bloques DWG; se edita desde el menu `RACKCAD`).
- **`blocks-library.dwg` NO esta versionado**: es el DWG del usuario con los bloques reales
  (p. ej. `TARIMA_GENERICA`, `PARRILLA_GENERICA_*`). Sin el, el dibujo reporta piezas faltantes y las
  omite (no aborta). Los nombres deben coincidir con `blockName` de `blocks.csv`.
- **Trampa conocida**: con AutoCAD abierto y el plugin cargado, los DLL del bin quedan bloqueados y el
  build falla en el paso de copia (MSB3021/MSB3027). Cerrar AutoCAD para reconstruir; para validar solo
  codigo, compilar a una carpeta temporal.
- Referencias de AutoCAD: apuntan a `C:\Program Files\Autodesk\AutoCAD 2025\` (avisos MSB3277 esperados).

## 7. Decisiones tecnicas

| Decision | Motivo | Estado |
|---|---|---|
| Solo `RackCad.Plugin` toca la API de AutoCAD | Testeabilidad: geometria/BOM puros en Application | vigente |
| Catalogos = CSV Excel-first (no BD) | El usuario los edita en Excel; cache por mtime; fallback Latin-1 | vigente |
| Un solo `secciones.csv` con columna `rol` (POSTE/CELOSIA/LARGUERO/SEPARADOR) | Una hoja para todos los perfiles; el provider los separa (API intacta) | vigente (2026-07-10) |
| Identidad por GUID embebido en el DWG (Xrecord), no por nombre de bloque | El nombre no es estable; el rack viaja con el archivo | vigente |
| Convencion Actualizar (redibuja en sitio) / Insertar (vista nueva enlazada) | Permanente en las 4 ventanas | vigente |
| Parametros dinamicos por patron ARRAY (HeaderGroup) al insertar | Fijar params por referencia era el cuello de botella | vigente |
| Cero dependencias NuGet en el codigo de producto (el XLSX es OOXML a mano) | Despliegue simple del plugin | vigente |
| **Parrilla: una por tarima; la regla de conteo vive en UN sitio** (`SelectiveFrontalBuilder.ParrillaRow`), consumida por frontal, BOM, lateral y editor | Dibujo, BOM y UI concuerdan por construccion, no por coincidencia | vigente (2026-07-14) |
| Cantidad de parrilla forzada: el dialogo la RECHAZA si no cabe; el builder ademas la ACOTA | El dialogo valida contra la matriz de ese momento; angostar despues degrada en vez de dibujar fuera del marco | vigente |
| Validacion de cargas DIFERIDA (ira con RAM Elements) | Decision explicita del usuario; no re-proponer | vigente |
| Optimizador IA de layout = meta futura; `RACKLAYOUT` es solo el motor de colocacion | El BOM ya da el costo; falta el optimizador beneficio/costo | planeado |

## 8. Trabajo realizado recientemente (los 4 commits locales)

1. **`b1cfce2` Tope medio-frente**: los topes de larguero siguen los tramos de un frente partido
   (frontal + planta), un tope por tramo cargado.
2. **`38572c6` Parrilla v1**: elemento de seguridad PARRILLA (deck) dibujable en frontal + lateral,
   rejilla frente x nivel + toggles por vista, BOM por tramo.
3. **`66d03c7` Parrilla por tarima**: correccion de requisito — va UNA parrilla POR TARIMA (no por nivel);
   campo "Frente" manual; regla unica `ParrillaRow`; fix de decks fantasma en la lateral
   (`ParrillaExistsAt` antes del dedup por Y). Revision adversarial multi-agente confirmo y se corrigio.
4. **`95e25f2` Cantidad manual + cuenta en vivo**: campo "Cantidad" (conserva el ancho de la tarima),
   cuenta calculada por celda + total en el dialogo (`SelectiveParrillaPlan`), rechazo de cantidades que
   no caben, y fix del aviso "no cabe ninguna" que se contradecia con la celda (el aviso ahora sale del
   numero que se pinta; `MaxCountIn` ignora filas inherentemente vacias).

Verificacion realizada: 489 tests verdes; los tests de regresion se verificaron **fallando** con el fix
desactivado antes de darlos por buenos; dos revisiones adversariales multi-agente (los hallazgos
confirmados se corrigieron; el resto fue refutado con evidencia empirica).

## 9. Trabajo en progreso

- **Verificacion manual en AutoCAD de la parrilla** (unico paso abierto de la feature):
  - Objetivo: confirmar en pantalla que cada parrilla cae exactamente bajo su tarima (origenes distintos:
    tarima BASE-CENTRO vs parrilla inferior-izquierda; en teoria calzan) y que frente/cantidad manuales
    se comportan como se espera.
  - Como: NETLOAD de `src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`, `RACKSELECTIVO`,
    activar "Mostrar tarimas", agregar PARRILLA en "Elementos de seguridad", probar vacio / frente=60 /
    cantidad=2.
  - Criterio de aceptacion: dibujo correcto en frontal y lateral; BOM coincide con lo dibujado.
  - Al confirmar: hacer push de la rama (politica de la sesion: no publicar features sin verificar en AutoCAD).

## 10. Problemas conocidos y deuda tecnica

1. **Desfase rejilla de seguridad <-> niveles resueltos** (preexistente, detectado 2026-07-14, prioridad alta):
   con "larguero a piso" APAGADO (default), `Safety_Click` construye la rejilla con los niveles de DISENO
   pero los builders indexan los niveles RESUELTOS (el resolver quita la celda de piso). Medido: 3 celdas
   de diseno -> 3 filas de rejilla pero 2 niveles reales; la casilla "Nivel 1" controla otro nivel y la
   fila superior esta muerta. Afecta a tope Y parrilla. Arreglo posible: pasar los niveles resueltos
   (`Safety_Click` ya resuelve el sistema); OJO: cambia el significado de los `OffCells` ya guardados —
   decidir migracion con el usuario. Detalle completo en la tarea pendiente del repositorio de sesion.
2. **`ParrillaFrente`/`ParrillaCantidad` son un valor unico para TODO el rack**: en racks con frentes de
   anchos distintos, un ancho que no cabe en el frente angosto deja ese frente sin parrilla (consistente
   en dibujo y BOM, y el dialogo lo avisa). Si el usuario necesita valores por frente/nivel, mover el
   override a la rejilla (deuda de diseno, no bug).
3. **Cantidad de parrilla es POR TRAMO en medio frente** (cada tramo es su propia posicion de carga):
   documentado en tooltip y docs; puede sorprender.
4. Avisos **MSB3277** al compilar el Plugin (conflictos de version de las refs de AutoCAD): conocidos,
   no bloquean.
5. Documentos historicos amplios en `docs/` (arquitectura, MVP, analisis VBA): utiles como referencia de
   decisiones, pero NO reflejan el estado actual; el indice los marca como historicos.

## 11. Siguientes tareas recomendadas

1. **Verificar la parrilla en AutoCAD y hacer push** — ver seccion 9 (criterio y pasos ahi). Es del usuario;
   un agente puede acompanar corrigiendo lo que aparezca.
2. **Arreglar el desfase rejilla <-> niveles resueltos** (item 1 de la seccion 10). Empezar en
   `src/RackCad.UI/RackSelectiveWindow.xaml.cs` (`Safety_Click`); anadir test de regresion que fije la
   correspondencia con "larguero a piso" apagado y verificarlo fallando sin el fix; consultar al usuario
   la migracion de `OffCells` guardados. Validar: `dotnet test`.
3. **Elementos de seguridad faltantes**: desviadores, poste tope, guardas traseras. Patron a seguir:
   `seguridad.csv` (tipo nuevo) -> `SelectiveSafetyPlacement` -> builders -> los **4 sitios de copia**
   (ver AGENTS.md) -> BOM -> dialogo. Referencia: como se hizo el TOPE y la PARRILLA.
   Validar: tests + AutoCAD.
4. **Tarima y parrilla en la vista de PLANTA** (hoy solo frontal/lateral). Empezar en
   `SelectivePlantaBuilder`. Criterio: mismas reglas de conteo (`ParrillaRow`), BOM sin duplicar.
5. **Overrides de parrilla por frente/nivel** (item 2 de la seccion 10) si el usuario lo pide.
6. **Integracion BOM -> cotizador** (los CSV de seguridad ya llevan costo/moneda/unidad): explorar con el
   usuario que formato de cotizacion necesita.

No tomar sin confirmar con el usuario: validacion de cargas (diferida a RAM Elements) y el optimizador IA
de layout (meta futura, no inmediata).

## 12. Verificacion del proyecto (ultima ejecucion real)

| Verificacion | Comando | Resultado | Fecha/entorno |
|---|---|---|---|
| Build Debug (todo) | `dotnet build RackCad.sln -v:minimal` | OK (0 errores; MSB3277 esperados) | 2026-07-14, Windows 11, SDK 10.0.301 |
| Pruebas | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug` | **489/489 verdes**, ~0.6 s | 2026-07-14, idem. NOTA: la carpeta `sdk` de .NET desaparecio de la maquina A MITAD de la sesion de cierre (evento externo, probablemente un actualizador); la ultima corrida real fue ANTES de los cambios de documentacion (que no tocan codigo). Primer paso en una maquina nueva: reinstalar el .NET 8 SDK y re-ejecutar este comando. |
| Lint / format / type-check | — | no aplica (no hay linters configurados; el compilador C# es el type-check) | — |
| Release / bundle | `pwsh deploy/install-bundle.ps1 -Build` | **no ejecutada** en esta sesion (requiere AutoCAD cerrado y no era necesaria) | — |
| Verificacion manual AutoCAD (parrilla) | NETLOAD + `RACKSELECTIVO` | **pendiente** (seccion 9) | — |

## 13. Preguntas abiertas

1. ¿Migrar los `OffCells` guardados al arreglar el desfase de la rejilla, o aceptarlos como estan? (usuario)
2. ¿La cantidad de parrilla debe poder variar por frente/nivel, o basta el valor global? (usuario, segun uso real)
3. ¿Que formato final necesita el cotizador que consumira el BOM? (futuro)

## 14. Como reanudar el trabajo

1. Clonar `https://github.com/marioap-afk/Calculadora_de_racks.git` y abrir la rama `release/claude-review`.
2. `git log --oneline -5` y comparar con la seccion 8 de este archivo (¿hubo push nuevo?).
3. Leer en orden: este archivo -> [README.md](../README.md) -> [AGENTS.md](../AGENTS.md) ->
   [docs/00-indice-contexto.md](00-indice-contexto.md).
4. `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` (debe dar 489+ verdes).
5. Tomar la primera tarea de la seccion 11 que siga abierta.

**Prompt de reanudacion (copiar en un chat nuevo de Claude o Codex):**

```
Trabajo en RackCad (D:\Documentos\Codex\Calculadora de racks), plugin de AutoCAD 2025 en C#/.NET8,
rama release/claude-review. Lee primero docs/HANDOFF.md, luego README.md y AGENTS.md; verifica el
estado real con git log y dotnet test (489 tests esperados). El contexto de estado, bugs conocidos
y siguientes tareas esta en las secciones 9-11 del HANDOFF. Continua con: [elige la tarea o pega la
seccion 11]. Respeta las convenciones de AGENTS.md (en especial: los 4 sitios de copia de flags de
seguridad, tests de regresion verificados fallando, y no hacer push sin verificacion en AutoCAD).
```

# Project Handoff

> Documento canonico de continuidad entre sesiones (Claude, Codex o un desarrollador nuevo).
> Actualizado: **2026-07-16**. La rama `codex/dinamico-modular`, creada sobre `release/claude-review`, separa ya
> diseno editable y sistema resuelto e incorpora IN/OUT C6, cama completa, largueros intermedios, seguridad multivista,
> matriz frente x nivel, BFR, fondos/posicion inicial por frente, peralte intermedio por frente y nivel,
> cotas/anotaciones y vistas frontal salida/entrada + planta. El editor dinamico ya usa seleccion de celda y alcance
> Celda/Nivel/Frente/Todas; las preliminares respetan altura y profundidad por corte: 627 tests y builds Debug verdes. El frente con menos
> fondos gobierna los dos `+6"` y el patron estructural compartido. El refuerzo de planta continua sobre X despues del poste principal, en su misma
> linea transversal y sin espejo. El BOM cuenta cabeceras/postes/placas, apoyos reforzados, separadores, IN/OUT,
> intermedios, camas con longitud/BFR y toda la seguridad fisica. DEFENSA usa ahora el piso real de la placa base y
> GUIA_ENTRADA se resuelve por frente/nivel, 8" sobre el IN/OUT de entrada, con LONGITUD del tramo. El cierre tecnico
> esta completo: restauracion, build Debug y 627/627 pruebas verdes. El usuario valido el lote progresivamente en
> AutoCAD; solo queda reconfirmar el ultimo ajuste de orientacion del desviador en la frontal de entrada.
> Base documental del worktree `cd20200`; primer commit modular `ee50526`.
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

**Estado**: activo y funcional. El arbol actual tiene **627/627 tests verdes** y build Debug completo con 0 errores;
solo aparecen los `MSB3277` conocidos de las referencias de AutoCAD. La base publicada conserva su validacion manual
de parrilla, larguero tope, rejilla, persistencia, biblioteca y rendimiento. Las variantes nuevas Bota C 4/6, Poste
tope y Lateral C 4/6 tambien estan verificadas por pruebas, compilacion y comprobacion visual del usuario en AutoCAD.
Los siete desviadores A/L estan implementados, cubiertos por pruebas y validados con sus bloques DWG reales.

## 2. Estado comprobado

| Aspecto | Estado | Evidencia |
|---|---|---|
| Compilacion Debug (Domain/Application/UI/Plugin) | **OK, 0 errores** | `dotnet build RackCad.sln -c Debug -v:minimal --no-restore`; 2 familias MSB3277 conocidas, 2026-07-16 |
| Pruebas unitarias | **627/627 verdes** | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj`, 2026-07-16 |
| Validacion estatica del arbol actual | OK | sintaxis 233 C#; semantica Domain/Application (114), Domain/Application/UI (146 fuentes + 11 XAML generados previos) y Domain/Application/tests (173); 12 XML/XAML bien formados |
| Carga en AutoCAD (NETLOAD del Debug) | **Validacion manual parcial** | Lote dinamico confirmado progresivamente; falta reconfirmar el ultimo ajuste del desviador de entrada, 2026-07-16 |
| Commits `b1cfce2`..`95e25f2` (tope medio-frente, parrilla) | Implementado, testeado y verificado en AutoCAD | Ver seccion 9 |
| Commits `aa42986`, `c11a267`, `a9f1c13` (variantes de seguridad y desviadores) | Implementados, testeados, verificados en AutoCAD y publicados | Ver secciones 8-9 |
| Release build / bundle de despliegue | Reconstruido localmente; no instalado | `src/RackCad.Plugin/bin/Release/net8.0-windows/RackCad.bundle`, 2026-07-16 |

## 3. Estado por funcionalidad

| Funcionalidad | Estado | Codigo principal | Pruebas | Observaciones |
|---|---|---|---|---|
| Cabecera (marco): editor + dibujo + round-trip | completo | `src/RackCad.UI/RackFrameConfiguratorWindow*`, `src/RackCad.Plugin/Headers/` | `tests/RackCad.Tests/*Frame*` | Peralte de placa editable por placa |
| Sistema dinamico (pallet flow) | **cierre tecnico completo; validacion de producto parcial** | `RackDynamicSystem*Builder`, `DynamicDepthGeometry`, `DynamicFrontGeometry`, `DynamicViewDecorations`, `DynamicSafety*Builder` | `Dynamic*Tests` | Matriz frente x nivel, fondos/inicio por frente, BFR, seguridad y anotaciones multivista; planta sin camas; falta reconfirmar desviador en frontal de entrada |
| Cama de rodamiento (flow bed) | completo | `RackFlowBedWindow`, `FlowBed*` | `FlowBed*Tests` | Rodillos por paso minimo; capacidad = fase futura |
| Selectivo: matriz, 3 vistas, doble profundidad, medio frente, cotas | completo | `RackSelectiveWindow`, `src/RackCad.Application/Systems/Selective*` | `Selective*Tests` | El modulo mas activo |
| BOM por componentes + consolidado (`RACKBOMTOTAL`) + export CSV/XLSX | completo | `SelectiveBomBuilder`, `RackBomWindow`, `XlsxWriter` | `SelectiveBomBuilderTests`, `Xlsx*Tests` | XLSX es OOXML escrito a mano, sin dependencias |
| Seguridad: bota, protector lateral, separador, tope de larguero | completo | `SelectiveSafetyPlacement`, `SelectiveSafetyWindow`, `SafetyTopeGridWindow` | `SelectiveSafetyTests` | Catalogo en `assets/catalogs/seguridad.csv` |
| Seguridad: **Bota C 4/6 + Poste tope** | completo y verificado en AutoCAD | `SelectiveSafetyFamilies`, `SelectiveSafetyWindow`, builders existentes | `SelectiveSafetyTests`, `SelectivePerFondoTests` | Variantes por `ElementId`; una seleccion exclusiva por familia |
| Seguridad: **Lateral C 4/6** | completo y verificado en AutoCAD | catalogos + regla `LATERAL` existente | `SelectiveSafetyTests` | Reutiliza colocacion, reemplazo de botas, BOM y selector del Lateral H |
| Seguridad: **Desviadores A/L** | completo y verificado en AutoCAD | `SelectiveDesviadorPlan`, `SelectiveDesviadorDrawing`, `SafetyDesviadorGridWindow` | `SelectiveDesviadorTests` | 7 variantes por `ElementId`; una regla/rejilla comun; lado Izquierdo/Derecho/Ambas |
| Seguridad: **parrilla / deck** (una por tarima, frente y cantidad manuales, cuenta en vivo) | completo y verificado en AutoCAD | `SelectiveFrontalBuilder.ParrillaRow`, `SelectiveParrillaPlan`, `SafetyParrillaGridWindow` | `SelectiveSafetyTests` (Parrilla_*), `SelectiveMedioFrenteTests` | Ver secciones 8-9 |
| Tarima como referencia visual (frontal + lateral, sin BOM) | completo | `SelectiveFrontalBuilder.AddPallets`, bloque `TARIMA_GENERICA` | `SelectiveFrontalBuilderTests` | Planta = futuro |
| Identidad + round-trip (GUID en Xrecord, `RACKEDITAR`, `RACKDUPLICAR`, `RACKLISTA`) | completo | `RackEmbedDocument`, `RackFrameCommands` | round-trip tests por store | Convencion Actualizar/Insertar en las 4 ventanas |
| Layout de almacen v1 (`RACKLAYOUT`, `RACKRELLENAR`) | completo (v1) | `WarehouseGridPlanner`, comandos en Plugin | `Warehouse*Tests` | Solo motor de colocacion; el optimizador IA es meta futura |
| Bibliotecas (disenos + bloques DWG) | completo | `RackDesignLibrary*`, settings en `%APPDATA%\RackCad\settings.json` | — | `blocks-library.dwg` NO esta en el repo (ver seccion 6) |
| Validacion de cargas / capacidad | **diferido a proposito** | — | — | Ira con RAM Elements; NO re-proponer |
| Guardas traseras (seguridad) | pendiente, prioridad final | — | — | No retomarlas hasta cerrar las areas funcionales prioritarias |
| Rejilla de seguridad vs niveles resueltos | **corregido, testeado y verificado en AutoCAD** | `SelectiveSafetyGrid`, `RackSelectiveWindow.Safety_Click` | `SelectiveSafetyGridTests` | Ver secciones 8-10 |

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
- **Flujo del dinamico** (base modular vigente): `DynamicRackDesign` (Domain, entradas editables, frentes y modulos
  sin coordenadas) -> `DynamicRackSystemResolver.Resolve` (Application) -> `DynamicRackSystem` resuelto -> builders
  puros lateral/frontal/planta -> `DynamicSystemPlan` -> servicios del Plugin. El DWG y la biblioteca persisten el
  diseno; coordenadas se regeneran. `DynamicFrontGeometry` resuelve BFR, posiciones/niveles por frente y la reticula transversal compartida;
  `DynamicLoadBeamGeometry` centraliza las elevaciones,
  limites y espejo de una pareja completa `LARGUERO_IN_OUT_C6` por nivel. `DynamicFlowBedGeometry` centraliza la
  geometria inclinada: `DynamicFlowBedLateralBuilder` compone la cama por sus troqueles y
  `DynamicIntermediateBeamGeometry` enumera un apoyo por poste interno, que
  `DynamicIntermediateBeamLateralBuilder` ajusta a la linea del origen del riel. `DynamicSafetyLateralBuilder` y
  `DynamicSafetyMultiViewBuilder` agregan botas, laterales y desviadores en las vistas ligadas; dibujo, BOM y
  persistencia consumen `SelectiveSafetySelection` sin duplicar el contrato del selectivo. `DynamicViewDecorations`
  centraliza numeros, nombre y cotas. Las orientaciones repetidas se agrupan en definiciones compartidas.
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
- **RackCad no requiere variables de entorno ni secretos.** La unica configuracion runtime de la aplicacion es
  `%APPDATA%\RackCad\settings.json` (ruta de la biblioteca de bloques DWG; se edita desde el menu `RACKCAD`).
- En esta workstation, el SDK 8.0.423 esta instalado por usuario en `%LOCALAPPDATA%\Microsoft\dotnet`; el perfil
  PowerShell de usuario antepone esa ruta y define `DOTNET_ROOT` porque `C:\Program Files\dotnet` solo tiene runtime.
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
| Copia de `SelectiveSafetySelection` centralizada en `DeepCopy` | Evita agregar un flag en resolver/UI/vista y olvidarlo en otro camino; el DTO sigue explicito por compatibilidad | vigente (2026-07-15) |
| Entrada numerica localizada sin separadores de miles | Acepta punto/coma sin convertir `96,5` en `965` | vigente (2026-07-15) |
| Cantidad de parrilla forzada: el dialogo la RECHAZA si no cabe; el builder ademas la ACOTA | El dialogo valida contra la matriz de ese momento; angostar despues degrada en vez de dibujar fuera del marco | vigente |
| Validacion de cargas DIFERIDA (ira con RAM Elements) | Decision explicita del usuario; no re-proponer | vigente |
| Optimizador IA de layout = meta futura; `RACKLAYOUT` es solo el motor de colocacion | El BOM ya da el costo; falta el optimizador beneficio/costo | planeado |

## 8. Trabajo realizado recientemente

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

**Lote cerrado 2026-07-15 (publicado, build/tests verdes):**

- `SelectiveSafetyGrid.LevelCounts` deriva las filas de niveles RESUELTOS y expone el maximo real entre fondos;
  elimina la fila de tarima de piso cuando no existe larguero.
- `AllCellsOff` ignora duplicados/indices legacy invalidos; evita descartar una seleccion por conteo inflado.
- Builders y BOM convierten `OffCells` una vez a `HashSet`; dejan de recorrer toda la lista por cada bloque/celda.
- `SelectiveSafetySelection.DeepCopy` centraliza la copia usada por resolver, vista por fondo y UI. Persistencia
  permanece explicita y con fallback.
- `LocalizedNumberParser` acepta punto/coma, prohibe agrupadores ambiguos y se usa en seguridad/layout.
- UX: textos de seguridad corregidos, aviso cuando la geometria no permite conteo, tipos/defaults sin literales
  repetidos; docs y manifest de comandos alineados.
- 14 casos nuevos (`LocalizedNumberParserTests`, `SelectiveSafetyGridTests`), total **503/503 verdes**.
- Regresion verificada con el fix temporalmente desactivado: 5/14 casos dirigidos fallaron (multi-fondo,
  duplicados/indices invalidos y parsing decimal/agrupadores); despues de restaurarlo, 503/503 verdes.

**Lote cerrado 2026-07-15 — variantes Bota C / Lateral C / Poste tope (publicado):**

- Se incorporaron los bloques `PROTECTOR_BOTA_C_4`, `PROTECTOR_BOTA_C_6` y `POSTE_3_1_5_8_TOPE` en las tres vistas.
- `seguridad.csv` los tipa como variantes `BOTA` / `TOPE`; reutilizan sin duplicar las reglas existentes.
- `SelectiveSafetyFamilies` centraliza la exclusividad y hace que dibujo, BOM y UI elijan el mismo `ElementId`.
- El dialogo muestra `Ninguno` + variantes en un combo por familia; los ids legacy H/larguero reabren seleccionados.
- Regresion comprobada antes del fix: 6 pruebas fallaron sin catalogacion; tras agregar los CSV pasaron. Luego 2
  pruebas de duplicados fallaron dibujando simultaneamente ambas variantes; con la regla central pasan. Total 511/511.
- UI y Plugin Debug compilan; el usuario confirmo en AutoCAD dibujo, seleccion y funcionamiento de Bota C 4/6 y Poste tope.
- Se agregaron `PROTECTOR_LATERAL_BOTA_C_4` y `_C_6` en las tres vistas. Las filas recibidas como `BOTA` se
  corrigieron a `LATERAL`, porque deben compartir exactamente la regla, reemplazo de botas y BOM del Lateral H.
- Regresion de Lateral C comprobada antes de catalogar: 4 de 8 casos dirigidos fallaron; despues pasan los 8 y la
  suite completa queda en 516/516. Plugin Debug compila y el usuario confirmo ambos laterales en AutoCAD.

**Lote cerrado 2026-07-15 — familia de desviadores A/L (publicado):**

- `seguridad.csv` cataloga `DESVIADOR_A_3`, `DESVIADOR_A_4`, `DESVIADOR_L_3`, `_L_3_5`, `_L_4`, `_L_4_5` y
  `_L_5`; cada id tiene bloques `FRONTAL`, `LATERAL` y `PLANTA`. El usuario confirmo que los siete reutilizan
  exactamente el mismo contrato de colocacion, parametros, espejo y BOM.
- `SelectiveDesviadorPlan` centraliza la rejilla poste x nivel, incluidos postes intermedios de medio frente, las dos
  caras exteriores expuestas al pasillo, el espejo posterior, el primer nivel sobre troquel y los niveles superiores
  6" debajo del larguero. El selector `Izquierdo/Derecho/Ambas` filtra la cara frontal/posterior o conserva ambas;
  dibujo, BOM y UI consumen el mismo plan y legacy cae en `Ambas`.
- `LONGITUD` y la altura del primer nivel son globales, 18" por defecto y aceptan solo enteros pares mayores de 8".
  La rejilla muestra una nota cuando el claro seleccionado es menor que la longitud y sugiere un par menor.
- Dibuja los bloques catalogados en frontal/lateral/planta; en planta el origen del desviador coincide con el origen
  del poste (sin offset `TROQUEL_LARGUERO`). El BOM cuenta las piezas fisicas de ambos frentes aunque frontal/planta
  colapsen proyecciones superpuestas. `ElementId`, dimensiones y celdas apagadas hacen round-trip.
- Regresion comprobada con la resolucion multi-variante desconectada: A4 y los cinco L dieron cantidad 0 en vez de
  12 (6/7 fallos esperados); restaurada la regla por `type`, pasan 30/30 casos dirigidos y 546/546 en la suite.
- Solucion y Plugin Debug compilan con 0 errores (solo las 2 familias MSB3277 conocidas). El usuario valido en
  AutoCAD los siete bloques, selector, rejilla, lados, dibujo, BOM y round-trip el 2026-07-15.

**Lote cerrado 2026-07-15 — base modular del sistema dinamico (validado en AutoCAD):**

- `DynamicRackDesign` separa las entradas editables (`Pallet`, fondos, niveles, alturas, poste y modulos) de
  `DynamicRackSystem`, que conserva exclusivamente el sistema lateral resuelto y sus coordenadas calculadas.
- `DynamicRackSystemResolver` es la frontera pura compartida: valida, calcula altura, deriva el layout estandar,
  regenera solo cabeceras calculadas y preserva por copia las cabeceras personalizadas.
- La UI, biblioteca y payload del DWG persisten el diseno, no las coordenadas. Los nuevos campos del DTO son nullable
  con fallback legacy; una cabecera legacy sin procedencia se conserva como personalizada para no perder ediciones.
- La validacion numerica de niveles/alturas ya no degrada silenciosamente valores invalidos a otros defaults.
- Alcance deliberado de ese lote: no se agregaron bloques, reglas de ancho/frentes, frontal/planta ni cama integrada;
  la cama se abordo en el lote posterior con el contrato aprobado de un componente `Cama` por nivel, sin despiece.
- 7 pruebas nuevas; total **553/553 verdes**. Regresiones observadas con el fix desactivado: cabecera personalizada
  150 -> 324 al recalcular; niveles persistidos 5 -> 3 al reabrir; cabecera legacy se marcaba calculada y se perdia.

**Lote tecnico 2026-07-16 — largueros IN/OUT C6 (validado en AutoCAD):**

- Se copiaron desde la rama principal las altas de `LARGUERO_IN_OUT_C6` y
  `MENSULA_TROQUEL_REDONDO_CAL_10` en `secciones.csv`, `mensulas.csv`, `blocks.csv` y
  `connection-layout.csv`. Los datos comerciales vacios son validos para esta etapa.
- Contrato corregido tras validacion visual: el bloque lateral C6 es la pieza completa y su origen coloca la pieza;
  no se ensambla ni cuenta una mensula separada. Hay una salida en `X=0` y una entrada en
  `X=TotalLength` por nivel, entrada espejeada en X y elevada por la pendiente completa. No se suman ni
  restan 6" en X.
- `DynamicLoadBeamGeometry` es la regla pura compartida; el resolver materializa niveles y peralte catalogado,
  el builder inserta las parejas y el preview consume exactamente las mismas colocaciones.
- Persistencia agrega `InOutBeamCatalogId` nullable y conserva fallback legacy. Un documento viejo con peralte 4
  se resuelve al unico peralte 6 permitido por el perfil C6. Inicialmente el BOM no los contaba; el contrato vigente
  cuenta el bloque completo por frente, nivel y extremo, sin agregar una mensula separada.
- 5 pruebas nuevas; total **558/558 verdes**. Regresion observada con la insercion temporalmente desactivada:
  la prueba esperaba 6 largueros y obtuvo 0; restaurado el fix, vuelve a verde.
- Correccion visual 2026-07-16: se elimino la compensacion interior de 6" en ambos extremos. La nueva regresion
  `Placements_UseTheFullSystemStartAndEndAsOriginMates` se ejecuto primero contra la regla anterior y fallo con
  `X=6` en vez de `X=0`; tras corregir la fuente unica, las 558 pruebas vuelven a verde.

**Lote tecnico 2026-07-16 — cama integrada y sentido de flujo (validado en AutoCAD):**

- Se copiaron de la rama principal `TROQUEL_CAMA` del C6 y `TROQUEL_IN` del riel en
  `connection-points.csv`/`connection-layout.csv`; las pruebas de catalogo fijan sus coordenadas recibidas.
- El sentido se invirtio sin crear una nueva pendiente: salida baja a la izquierda, entrada alta a la derecha y
  el mismo desnivel calculado por `DynamicHeaderHeightCalculator`.
- `DynamicFlowBedLateralBuilder` compone, no duplica, `FlowBedLateralBuilder`: coloca el conjunto rigido por
  `TROQUEL_IN -> TROQUEL_CAMA`; la longitud incluye el retranqueo del mate del riel para que su extremo alcance
  el mate del segundo larguero. Todos los niveles comparten una definicion anidada.
- Preview y dibujo consumen el plan puro. El BOM agrega una unidad `Cama` por nivel sin rieles, rodillos, frenos,
  topes ni largueros como despiece comercial.
- 5 pruebas nuevas; total **563/563 verdes**. La regresion de sentido se observo fallar contra la colocacion anterior
  (salida real a la derecha en vez de la izquierda) antes de invertir la regla compartida.
- Plugin Debug recompilado desde el worktree; la salida reproducible para NETLOAD es
  `src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll`.
- El usuario confirmo en AutoCAD que la pendiente invertida, los mates y la cama completa funcionan correctamente.

**Correccion 2026-07-16 — longitud comercial de la cama:**

- El contrato fisico definitivo es `LONGITUD = tramo longitudinal del frente - 4"`. Para un frente de `204"`, la
  cama mide `200"`.
- `DynamicFlowBedGeometry.ResolveBedLength` es la fuente unica para dibujo y BOM, tambien cuando cada frente tiene
  fondos/inicio distintos. Los mates de catalogo colocan la cama y resuelven su angulo, pero no modifican el corte.
- La regresion `Build_UsesTotalLengthMinusFourForTheCompleteBed` fallo primero con `198.0828"` en vez de `200"` y
  queda verde despues de sustituir el calculo diagonal anterior.

**Correccion 2026-07-16 — centrado del poste derivado reforzado (validado en AutoCAD):**

- Antes, el origen del poste principal coincidia con el limite entre los dos separadores y el refuerzo se agregaba
  completo hacia la derecha. El conjunto quedaba cargado a un solo lado.
- `DynamicDerivedPostGeometry` fija la regla compartida: el limite coincide con `FIN_POSTE`; para el perfil actual
  (`FIN_POSTE.X=3"`) el principal queda en `limite-3"` y el refuerzo comienza en el limite. Sin refuerzo no hay shift.
- Builder y preview consumen la misma regla. La placa base sigue al poste principal.
- Las dos regresiones dirigidas fallaron con la colocacion anterior y pasaron despues del fix; suite **563/563**.
- El usuario confirmo visualmente que el poste reforzado queda centrado en el limite correcto.

**Lote tecnico 2026-07-16 — largueros intermedios (validado en AutoCAD):**

- Se copiaron de la rama principal `LARGUERO_ESCALON_INFINITO` y `MENSULA_AJUSTE_INFINITO_CAL_10` en
  `secciones.csv`, `mensulas.csv`, `blocks.csv` y `connection-layout.csv`. Se registraron tambien los ids logicos
  `INICIO_IZQUIERDO`/`INICIO_DERECHO` en `connection-points.csv`. Los datos comerciales siguen opcionales.
- `DynamicFlowBedGeometry` centraliza los mates extremos, la pendiente y la transformacion del origen del riel.
  La cama usa `TROQUEL_IN -> TROQUEL_CAMA`; los apoyos usan la linea del origen del riel.
- `DynamicIntermediateBeamGeometry` enumera cada limite interno entre modulos y excluye los extremos IN/OUT.
  `DynamicIntermediateBeamLateralBuilder` coloca una pieza por posicion y nivel: normal/`INICIO_IZQUIERDO` en un
  primer poste, espejo/`INICIO_DERECHO` en un segundo. El derivado reforzado central recibe uno solo sobre el
  perfil principal, normal; el refuerzo no invierte la mensula hacia el otro lado. La ranura resuelve la altura sin
  snap a un troquel discreto.
- Las piezas se agrupan en dos definiciones compartidas (una por orientacion) y el preview WPF usa las colocaciones
  puras. En ese lote el BOM aun contaba solo `Cama`; el contrato vigente ya incluye IN/OUT, intermedios,
  separadores, postes y seguridad fisica, ademas de una `Cama` completa por posicion y nivel.
- La primera regresion se observo con **0 vs 6** al agregar el bloque. Tras la validacion visual, la regresion
  corregida fallo con **6 vs 9**: solo habia dos piezas en el centro y faltaban los demas postes. Ahora tambien
  comprueba un solo centro y contacto con la linea del origen del riel. Suite completa **565/565**.
- El `secciones.csv` recibido tenia una fila nueva en ANSI dentro de un archivo UTF-8; se normalizo esa `o` acentuada
  a UTF-8 para evitar que el fallback Latin-1 corrompiera visualmente los nombres anteriores.

**Lote tecnico 2026-07-16 — seguridad lateral del dinamico (validacion manual progresiva):**

- El editor dinamico reutiliza `SelectiveSafetyWindow`, filtrado a `BOTA`, `LATERAL` y `DESVIADOR`. Este modelo
  vigente es un solo corte: izquierda = salida, derecha = entrada y la rejilla de desviador tiene una columna por
  los niveles de carga reales.
- `DynamicSafetyLateralBuilder` coloca botas en los origenes reales de las placas de los extremos. Un protector
  lateral sustituye las botas y recibe `LONGITUD=TotalLength`. El desviador conserva el contrato selectivo: primer
  nivel sobre `TROQUEL_LARGUERO`, superiores 6" debajo del IN/OUT y seleccion izquierda/derecha/ambas.
- `DynamicRackDesign`/`DynamicRackSystem` llevan copias profundas de `SelectiveSafetySelection`. El DTO dinamico
  reutiliza `SafetySelectionDocument`; campo nuevo nulo abre como lista vacia y el round-trip conserva variante,
  lados, dimensiones y celdas apagadas.
- `SystemBomBuilder` cuenta las instancias `Safety` del mismo plan lateral: respeta sustitucion de botas, longitud
  fabricada del lateral (+4") y cantidad fisica de desviadores. `Cama` sigue sin despiece interno.
- El preview WPF representa las tres familias sin asumir la geometria interna de los bloques. Cinco pruebas nuevas
  cubren builder, BOM, deep-copy y persistencia; suite completa **570/570** y UI aislada 0/0.
- El Plugin Debug compilo con 0 errores (solo las 2 familias MSB3277 conocidas); la salida reproducible para
  NETLOAD es `src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll`.
- Regresion del apoyo central verificada antes del fix: el test esperaba normal y recibio espejo en los tres niveles
  (`Expected False, Actual True`). Tras cambiar solo su orientacion/mate, la colocacion conserva origen y contacto.

**Lote tecnico 2026-07-16 — matriz, BFR y multivista del dinamico (cierre tecnico completo):**

- `DynamicRackFrontDesign`/`DynamicRackFront` incluyen `PalletsDeep` y `DepthStartPosition`; la UI los edita dentro de
  `Frente seleccionado` y muestra `Fondos maximos` como envolvente resuelta. `DynamicDepthGeometry` hace que el frente
  con menos fondos gobierne el rango base, los dos `+6"` y el patron cabecera/separador. Los frentes mayores deben
  contener ese rango y pueden extenderlo antes o despues. Un extremo que cae en separador conserva ese tipo y agrega
  un poste limite sencillo en lateral/planta/BOM, sin inventar una cabecera.
- Cada frente guarda `StartX/EndX` y sus propios `LoadBeamLevels`; pendiente, IN/OUT, camas, apoyos y BOM usan esa
  longitud fisica. Cada lateral por poste dibuja la union de sus frentes adyacentes, mientras planta filtra la
  estructura por linea transversal. La seguridad usa el rango real de cada linea. El DTO guarda ambos enteros como
  nullable; documentos anteriores heredan `PalletsDeep` global e inicio 1.
- `DynamicRackDesign` persiste frentes con posiciones, niveles y largo manual opcional. El resolver calcula y almacena
  `BFR = frenteTarima + 2"`; el IN/OUT automatico mide `BFR * posiciones + 6"`. `PalletTolerance` queda solo para
  round-trip legacy. Campos nuevos del DTO son nullable y los documentos anteriores abren con un frente/niveles base.
- `RackDynamicSystemWindow` reemplaza el grid lateral por una cantidad total de frentes escrita como entero, matriz
  frente x nivel, editor de celda con alcance Celda/Nivel/Frente/Todas y preliminares lateral/frontal salida/frontal entrada. Al aumentar
  el entero conserva los frentes existentes y clona la configuracion editable del seleccionado. La preliminar lateral
  permite elegir poste; cada corte lateral y cada poste frontal usa el maximo de los frentes adyacentes que toca.
- `DynamicSystemFrontalBuilder` genera dos cortes ligados: `Section=0` salida y `Section=1` entrada. Comparten reticula,
  placas y anchos; cada frente dibuja solo sus niveles de IN/OUT y cada poste toma la altura comercial del frente
  adyacente mas alto. No dibujan camas ni largueros intermedios.
- `DynamicSystemPlantaBuilder` repite las cabeceras longitudinales, agrega IN/OUT e intermedios transversales y omite
  las camas. En un apoyo reforzado el principal termina en el limite `FIN_POSTE` y el segundo perfil/placa comienza
  ahi: ambos conservan la misma Y y orientacion, consecutivos sobre X.
- El editor separa `Frente seleccionado` de `Celda seleccionada`. Posiciones, niveles, fondos, inicio longitudinal e
  inicio del primer larguero pertenecen exclusivamente al frente actual. Claro libre, tarima (frente/alto/peso),
  largo manual y tipo/peralte de IN/OUT e intermedio pertenecen a la celda frente x nivel. Celda/Nivel/Frente/Todas
  replica solo la celda; `Frente` no propaga datos estructurales a otras columnas. Las opciones salen de catalogo.
  Ctrl + clic mantiene varias celdas; `Seleccionadas` replica la celda y los datos estructurales ofrecen frente actual,
  frentes con alguna celda seleccionada o todos los frentes.
  Dibujo, altura y BOM consumen `DynamicRackLevelGeometry`; el largo fisico del frente es la mayor solicitud de sus
  niveles. El DTO guarda celdas nullable y conserva fallback a listas por frente y campos globales legacy.
- `Peralte de poste` es un valor global junto al tipo de poste. `DynamicRackSystemResolver` lo impone a cabeceras
  calculadas y personalizadas; el DTO nullable hace que un documento legacy herede el ancho catalogado del perfil.
- `DynamicSafetyMultiViewBuilder` proyecta BOTA/LATERAL/DESVIADOR en frontal y planta desde la misma seleccion usada
  por el lateral. `DynamicViewDecorations` centraliza numeros de frente/nivel, nombre, cotas, escala y estilo.
- `DynamicSystemLateralBuilder.Cortes` genera N+1 laterales para N frentes. Al insertar se pide numero de poste y el
  bloque conserva `Section=postIndex`; altura, niveles, camas, apoyos y seguridad se limitan a sus frentes adyacentes.
- `RACKEDITAR` encuentra las vistas por GUID, redibuja laterales/frontales/planta con un solo `Regen` y puede insertar
  otra vista ligada. El round-trip conserva BFR, niveles por frente y anotaciones; payloads sin `View/Section` se
  actualizan como lateral del poste 1.
- `Altura primer nivel` sigue siendo una entrada nominal (6" por defecto), pero salida y entrada se ajustan al
  `TROQUEL_LARGUERO` mas cercano. La cota del larguero arranca en `INICIO_PERFIL`, no en el troquel del poste.
- El BOM conserva `Cama` sin despiece, cuenta `suma(posiciones * niveles)` por frente y muestra longitud/BFR.
  Las cabeceras se repiten por cada una de las N+1 lineas de postes; los apoyos derivados se identifican como
  `Poste reforzado` y aportan dos perfiles/placas; tambien se cuentan separadores, IN/OUT por frente/nivel/extremo e
  intermedios por longitud/peralte. BOTA/LATERAL salen de planta: un lateral de longitud completa sustituye las dos
  botas de su linea de poste. DESVIADOR sale de ambos cortes frontales para conservar todos sus niveles, sin duplicar
  piezas por proyecciones alternativas.
- Regresiones verificadas antes del fix: tres posiciones daban 142" en lugar de 138"; en planta solo tres de seis
  postes reforzados compartian la X principal. Ambas fallaron contra la regla anterior y pasaron tras corregirla.
- `DynamicSystemPreviewGeometry` entrega a WPF el rango y plan lateral del poste seleccionado y las alturas
  adyacentes del frontal. La lateral ya no muestra la profundidad global; los frontales no estiran todos los postes
  al maximo y resaltan solo la celda elegida.
- Las regresiones nuevas cubren ajuste a troquel, inicio fisico de la cota, cortes laterales por poste, fondos
  variables y preliminares por corte, ademas de BFR, refuerzo, seguridad, niveles, alturas, peraltes, persistencia y BOM.
  La independencia de altura por frente fallo 30" esperado vs 6" global con el fix desactivado.
  La defensa de montacargas agrega grid por poste con salida/entrada y longitud independientes, defaults 12" en
  orillas y 36" en intermedios, proyeccion lateral/frontal/planta, round-trip y BOM fisico. Suite completa:
  **627/627 verdes**.
- El cierre se compilo dentro de este worktree. Los ensamblados reproducibles quedan en
  `src/RackCad.Plugin/bin/Debug/net8.0-windows/` y `src/RackCad.Application/bin/Debug/net8.0/`; `bin/` no se
  versiona y no forma parte del respaldo Git.

## 9. Ultima validacion manual

- **AutoCAD 2025, NETLOAD del Debug, 2026-07-16: validacion progresiva confirmada por el usuario.**
- Biblioteca: disponible y reportada correctamente por `RACKCAD`.
- Rejilla sin larguero a piso: filas resueltas correctas, sin fila muerta ni desplazamiento.
- Parrilla: frente/cantidad, posicion en frontal/lateral y BOM correctos.
- Tope: rejilla, posicion y opciones correctas.
- Variantes de seguridad: selectores exclusivos, Bota C 4/6, Poste tope, Lateral C 4/6 y los siete desviadores A/L
  funcionan correctamente en sus vistas y BOM.
- Persistencia/round-trip: configuracion conservada tras `RACKEDITAR` y actualizar.
- Rendimiento: sin degradacion perceptible en el escenario probado.
- Sistema dinamico modular: **OK confirmado por el usuario** tras probar el DLL Debug; no se cambio ningun bloque DWG.
- Largueros IN/OUT C6 en los limites, sin +/-6" en X: **OK confirmado por el usuario**.
- Nuevo sentido salida izquierda/entrada derecha + cama integrada: **OK confirmado por el usuario**.
- Centrado del poste derivado reforzado por `FIN_POSTE`: **OK confirmado por el usuario**.
- Largueros intermedios uno por poste interno y sobre el origen del riel: orientacion central **OK**; el ajuste de
  posicion sobre el poste principal fue confirmado por el usuario.
- Editor de muchos frentes, matriz/niveles, BFR e IN/OUT; alturas independientes, peraltes por celda, fondos variables
  y alcances masivos: **OK tras correcciones sucesivas confirmadas por el usuario**.
- Vistas preliminares, frontales salida/entrada, planta, laterales por poste, cotas, nombre y numeracion: **OK tras
  correcciones sucesivas confirmadas por el usuario**.
- BOM dinamico estructural y de seguridad, incluida la sustitucion de botas y los datos longitud/BFR de cama:
  **OK tras correcciones sucesivas confirmadas por el usuario**.
- DEFENSA con lados/longitudes independientes y GUIA por frente/nivel: **OK confirmado por el usuario**.
- **No validado despues del ultimo cambio:** reconfirmar que el desviador de la frontal de entrada conserva la
  orientacion dibujada del bloque (sin `MirroredX`). La regresion automatizada esta verde, pero el criterio final es
  el bloque de la biblioteca DWG externa.

## 10. Problemas conocidos y deuda tecnica

1. **Desfase rejilla de seguridad <-> niveles resueltos: corregido, testeado y verificado en AutoCAD**. La UI usa
   `SelectiveSafetyGrid.LevelCounts(resolved)`; sin larguero a piso, 3 filas de diseno producen 2 filas reales.
   Compatibilidad: los builders siempre interpretaron `Level` como indice RESUELTO, por lo que se conserva el
   significado efectivo de `OffCells`; referencias a la antigua fila superior muerta se ignoran y desaparecen al
   volver a guardar. Tests y validacion visual de tope/parrilla estan verdes.
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
6. **Ultima comprobacion de producto del dinamico**: el modelo, builders, round-trip y el resto de la multivista ya
   fueron probados progresivamente. Falta solo reconfirmar en AutoCAD que el desviador de la frontal de entrada no se
   desplaza despues de eliminar su espejo. La prueba pura fija `MirroredX=false` en ambos cortes.
7. **Fallback legacy conservador**: una cabecera dinamica de un documento antiguo no declaraba si era calculada;
   se abre como personalizada para evitar perdida de datos. `Restaurar estandar` o `Calculada` vuelve a derivarla.

## 11. Siguientes tareas recomendadas

1. **Comprobacion manual puntual**: cargar
   `src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll`, insertar la frontal de entrada y confirmar que
   el desviador conserva la orientacion correcta sin desplazamiento lateral. No requiere cambiar el modelo ni los CSV.
2. **Overrides de parrilla por frente/nivel** (item 2 de la seccion 10): mantener a mediano plazo; hoy los valores
   globales son suficientes, pero el control por celda puede aportar valor en configuraciones heterogeneas.
3. **Guardas traseras**: mantener pendientes hasta el final; no son prioridad de producto.

Quedan diferidos sin prioridad actual: tarima/parrilla en PLANTA, integracion BOM -> cotizador, distribucion formal
de `blocks-library.dwg` y reconstruccion del bundle Release. El BOM actual es suficiente; el cotizador real es un
Excel delicado y no justifica el riesgo de integracion. No es necesario desplegar la aplicacion todavia.

No tomar sin confirmar con el usuario: validacion de cargas (diferida a RAM Elements) y el optimizador IA
de layout (meta futura, no inmediata).

## 12. Verificacion del proyecto (ultima ejecucion real)

| Verificacion | Comando | Resultado | Fecha/entorno |
|---|---|---|---|
| Restauracion | `dotnet restore RackCad.sln` | **OK; todos los proyectos actualizados** | 2026-07-16, Windows 11, SDK 8.0.423 por usuario |
| Build Debug (todo) | `dotnet build RackCad.sln -c Debug -v:minimal --no-restore` | **OK, 0 errores, 2 advertencias MSB3277 conocidas** | 2026-07-16, Windows 11, SDK 8.0.423 por usuario |
| Build Plugin Debug | incluido en el build de la solucion | **OK; DLL y bundle Debug generados dentro del worktree** | 2026-07-16 |
| Pruebas | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug --no-restore -v:minimal` | **627/627 verdes**, 0 omitidas | 2026-07-16 |
| Build UI aislado | `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug -o %TEMP%/rackcad-ui-dynamic-final` | **OK, 0 errores, 0 advertencias** | 2026-07-16 |
| Regresion dinamico: largueros intermedios | `DynamicSystemLateralBuilderTests.Build_AddsOneIntermediateBeamAtEveryInternalPost_MatedToRailOriginLine` | **1 fallo esperado contra la primera version:** 6 vs 9; verifica todos los postes, centro unico, espejo y origen del riel | 2026-07-16 |
| Regresion dinamico: orientacion del apoyo central | mismo test dirigido, esperando bloque normal en separador-separador | **1 fallo esperado:** 3/3 niveles daban espejo (`True`) en vez de normal (`False`); fix restaurado | 2026-07-16 |
| Regresion dinamico: posicion del apoyo central | mismo test dirigido, esperando `boundary - FIN_POSTE.X` | **1 fallo esperado:** 0 apoyos en el poste principal; estaban 3" a la derecha sobre el refuerzo; fix restaurado | 2026-07-16 |
| Multivista dinamica dirigida | `DynamicFrontGeometryTests`, `DynamicSystemMultiViewBuilderTests`, resolver/store/BOM | **Verde**; BFR, niveles por frente, ambos cortes, planta sin camas, seguridad/decoraciones y BOM | 2026-07-16 |
| Fondos variables por frente | `DynamicDepthGeometryTests` | **6/6 verdes**; rango base del frente corto, +6 interiores, pendiente/cama/BOM por frente, endpoints multivista y poste limite en separador | 2026-07-16 |
| Regresion dinamico: +6 gobernado por frente corto | `Resolve_ShortestFrontOwnsAllowancesAndLongerFrontContinuesThePattern` | **1 fallo esperado con la regla desactivada:** `[54,48,48,...,54]` global en vez de `[48,48,54,54,48,...]`; fix restaurado | 2026-07-16 |
| Regresion dinamico: BFR/IN-OUT | `DynamicFrontGeometryTests` con tres posiciones de 40" | **1 fallo esperado:** 142" con la formula antigua vs 138" requerido; fix restaurado | 2026-07-16 |
| Regresion dinamico: refuerzo consecutivo en planta | `Planta_DerivedReinforcementContinuesAfterThePrimaryOnTheSameFrontLine` | **1 fallo esperado antes del fix:** el segundo perfil estaba en `Y+6` y espejado; ahora comienza en el limite sobre X con la misma Y/orientacion | 2026-07-16 |
| Regresion dinamico: BOM estructural | `Build_ResolvedSystem_CountsHeaderAndDerivedPostsAcrossEveryFrontLine` + `Build_AddsIntermediateBeamsWithTheirLengthPeralteAndPhysicalQuantity` | **2 fallos esperados antes del fix:** 4 vs 18 postes y 0 vs 2 grupos de intermedios; corregidos | 2026-07-16 |
| Regresion dinamico: datos de cama en BOM | `Build_ResolvedDynamicSystem_CountsOneCompleteBedPerLevelWithoutInternalBreakdown` | **1 fallo esperado antes del fix:** longitud 0; ahora informa longitud fisica y BFR 44 | 2026-07-16 |
| Regresion dinamico: IN/OUT y seguridad en BOM | `Build_AddsEveryInOutBeamByFrontAndLevel_WithLengthAndPeralte` + casos `Build_DynamicSafetyCounts*` / `Build_DynamicLateralGuardsReplaceBothEndBootsAtEachProtectedPost` | Fallos esperados verificados: 0 grupos IN/OUT, seguridad ausente fuera del poste 0 y **8 botas vs 6 fisicas**; BOTA/LATERAL usan planta y DESVIADOR ambos cortes | 2026-07-16 |
| Regresion dinamico: separadores en BOM | `Build_CountsEveryDrawnDynamicSeparatorAcrossAllPostSections` | **1 fallo esperado antes del fix:** 0 vs 16 separadores; dibujo y BOM comparten `DynamicSeparatorGeometry` | 2026-07-16 |
| Peralte intermedio por frente y nivel | builders lateral/planta + resolver + round-trip/legacy | **4 pruebas verdes:** listas independientes llegan a `PERALTE`; lista comun anterior se migra y ausencia legacy resuelve 3.5 por nivel | 2026-07-16 |
| Regresion dinamico: peralte intermedio predeterminado | `Resolve_MissingIntermediateBeamPeraltes_DefaultsEveryFrontLevelToThreePointFive` | **1 fallo esperado antes del fix:** `[6,6]` en vez de `[3.5,3.5]`; valor central corregido | 2026-07-16 |
| Regresion dinamico: peralte independiente en planta | `Planta_AppliesTheLargestConfiguredIntermediateBeamPeralteOfEachFront` | **1 fallo esperado con la regla comun restaurada temporalmente:** primer frente daba 6 en vez de 5; fix restaurado | 2026-07-16 |
| Regresion dinamico: altura de poste por frente | `Frontal_PostHeight_IsTheTallestAdjacentFront_NotTheSystemMaximum` | **1 fallo esperado antes del fix:** `[336,336,336,336]` en vez de `[192,336,336,192]`; regla adyacente restaurada | 2026-07-16 |
| Regresion dinamico: preliminares por corte | `DynamicFrontGeometryTests.Preview*` | **2 fallos esperados con el fix desactivado:** poste corto 192" aparecia 336" y lateral 3-4 volvia al rango global 1-4; reglas restauradas, 2/2 y suite verdes | 2026-07-16 |
| Regresion dinamico: ajuste a troquel | `Resolver_SnapsBothInOutBeamMatesToTheNearestPostTroquel` | **1 fallo esperado antes del fix:** 6.000" en vez de 6.6053"; salida y entrada ahora caen en base + N x 2" | 2026-07-16 |
| Regresion dinamico: origen de cota IN/OUT | `Frontal_BeamCutDimensionStartsAtTheProfileSection_NotAtTheHookTroquel` | **1 fallo esperado antes del fix:** X=0.750" (troquel) en vez de X=1.747" (`INICIO_PERFIL`) | 2026-07-16 |
| Regresion dinamico: lateral por poste | `Cortes_UseOnlyTheHeightAndLevelsOfEachPostsAdjacentFronts` | **1 fallo esperado antes del fix:** poste extremo 336"/5 niveles en vez de 192"/3; cuatro cortes `[3,5,5,3]` verdes | 2026-07-16 |
| Peralte global de poste dinamico | `Resolve_RackWidePostPeralteOverridesCalculatedAndCustomHeaders` + round-trip | **Verde**; 4.5 llega a todas las cabeceras y ausencia legacy hereda el ancho catalogado | 2026-07-16 |
| Regresion dinamico: frente/celda independientes | `Resolve_FrontAndLevelInputsRemainIndependent` + round-trip/BOM por celda | **1 fallo esperado con el fix desactivado:** el segundo frente resolvia 6" en vez de su inicio 30"; restaurado. Tarima, claro, ambos largueros y BFR permanecen por celda | 2026-07-16 |
| Alcances masivos del dinamico | `CellScope_FrontTargetsOnlyTheSelectedFrontLevels` + `CellScope_SelectedKeepsOnlyValidDistinctCells` | **2/2 verdes**; Ctrl selecciona coordenadas validas, Frente no cruza columnas y los datos estructurales tienen alcance actual/seleccionados/todos | 2026-07-16 |
| Seguridad dinamica dirigida | `Build_SafetyBootsUseEndpointPlateOrigins_AndLateralReplacesThem`, `Build_DesviadoresUseSelectiveVerticalContractAndOneCutGrid`, BOM/store/resolver | **5 pruebas verdes** | 2026-07-16 |
| Defensa de montacargas dinamica | `DynamicForkliftDefensePlanTests` + casos `ForkliftDefense` en lateral/multivista/BOM + round-trip | **5 regresiones nuevas verdes**; con planta desconectada fallaron 0 vs 6 piezas y 0 vs 3 grupos BOM. Salida/entrada conservan longitudes independientes | 2026-07-16 |
| Datum de defensa + guia de entrada dinamica | casos `ForkliftDefense`/`EntranceGuide` en lateral, multivista, BOM, DeepCopy y round-trip | **5 fallos esperados observados antes del fix:** DEFENSA quedaba en Y=0 del poste en vez del origen de placa y GUIA daba 0 piezas en lateral/frontal/planta/BOM. Ahora GUIA usa grid frente x nivel (todos activos por defecto), pareja espejeada, +8" y LONGITUD del tramo | 2026-07-16 |
| Seguridad predeterminada de un dinamico nuevo | `DynamicSafetyDefaultsTests` + `Build_NewDynamicSafetyDefaultsProduceEveryPhysicalFamily` | **3 pruebas verdes**; BOTA/LATERAL/DESVIADOR/DEFENSA/GUIA nacen seleccionadas desde catalogo. Las orillas de LATERAL siguen siendo primera/ultima aunque cambie la cantidad de frentes | 2026-07-16 |
| Regresion dinamico: orientacion frontal del desviador | `FrontalEntrance_KeepsTheAuthoredDesviadorOrientationWithoutMirroring` | **1 fallo esperado antes del fix:** 9/9 referencias de entrada tenian `MirroredX=true`; ambos cortes frontales conservan ahora la orientacion original del bloque | 2026-07-16 |
| Cama integrada dirigida | filtro `DynamicFlowBedLateralBuilderTests|DynamicSystemLateralBuilderTests|SystemBomBuilderTests|CatalogStandardConsistencyTests` | **21/21 verdes** | 2026-07-16 |
| Regresion dinamico: poste derivado centrado | `DynamicSystemLateralBuilderTests.Build_DerivedPost_*` | **2 fallos esperados antes del fix:** placa/poste seguian en limite y refuerzo en limite+3; corregido a limite-3/limite | 2026-07-16 |
| Regresion dinamico: insercion IN/OUT | `DynamicSystemLateralBuilderTests.Build_AddsCompleteEntranceAndExitBeamAtEveryResolvedLoadLevel` | **1 fallo esperado con insercion desactivada:** 0 vs 6; fix restaurado | 2026-07-16 |
| Regresion dinamico: limites IN/OUT | `DynamicLoadBeamGeometryTests.Placements_UseTheFullSystemStartAndEndAsOriginMates` | **1 fallo esperado antes del fix:** X real 6 vs esperado 0; corregido a 0/TotalLength | 2026-07-16 |
| Regresion dinamico: cabecera personalizada | `DynamicRackSystemResolverTests.Resolve_RecalculatesStandardHeaders_ButPreservesACustomHeader` | **1 fallo esperado:** 150 se regeneraba como 324; fix restaurado | 2026-07-15 |
| Regresion dinamico: entradas persistidas | `RackProjectStoreTests.RoundTrip_DynamicDesign_PreservesHeightInputsAndHeaderProvenance` | **1 fallo esperado:** niveles 5 reabrian como 3; fix restaurado | 2026-07-15 |
| Regresion dinamico: cabecera legacy | `RackProjectStoreTests.DynamicDocument_LegacyHeaderWithoutProvenance_IsPreservedAsCustom` | **1 fallo esperado:** se marcaba calculada; fix restaurado | 2026-07-15 |
| Regresion con fix desactivado | filtro `SelectiveSafetyGridTests|LocalizedNumberParserTests` | **5 fallos esperados de 14**; fix restaurado y suite completa verde | 2026-07-15 |
| Regresion Desviador A3 con integracion desconectada | `SelectiveDesviadorTests.Drawing_ProjectsTheSamePlanInThreeViews_AndBomKeepsPhysicalLevelCount` | **1 fallo esperado:** frontal 0 vs 6; fix restaurado, 17/17 dirigidas y suite verde | 2026-07-15 |
| Regresion origen A3 en planta | mismo caso dirigido, exigiendo insercion igual al origen del poste | **1 fallo esperado:** 4/4 referencias conservaban offset del troquel; fix restaurado y suite verde | 2026-07-15 |
| Regresion lado A3 | `SelectiveDesviadorTests.SideChoice_FiltersAisleFacesForDrawingAndBom` | **2 fallos esperados:** Izquierdo/Derecho daban 12 piezas en vez de 6; fix restaurado, 3/3 y suite verde | 2026-07-15 |
| Regresion variantes A/L | `SelectiveDesviadorTests.EveryVariant_ReusesTheSamePlanDrawingAndBomRule` | **6/7 fallos esperados:** A4/L daban 0 piezas al limitar la resolucion a la primera variante; restaurado, 7/7 verdes | 2026-07-15 |
| Validacion estatica auxiliar | Roslyn de PowerShell + referencias .NET 8/xUnit en cache; parse XML | **OK:** sintaxis 233 C#; semantica Domain/Application, UI y tests; 12 XML/XAML bien formados | 2026-07-15; no sustituye `dotnet build/test` ni la generacion XAML actual |
| Lint / format | — | no aplica (no hay linters configurados) | — |
| Release / bundle | `pwsh deploy/install-bundle.ps1 -Build` | **no ejecutada en el cierre**; no se instalo ni se modifico el equipo | — |
| Verificacion manual AutoCAD | NETLOAD + `RACKCAD`/`RACKEDITAR` | Lote dinamico validado progresivamente; **pendiente solo reconfirmar el desviador de la frontal de entrada tras el ultimo fix** | 2026-07-16 |

## 13. Preguntas abiertas

1. Confirmar en AutoCAD que el bloque DESVIADOR de la frontal de entrada conserva su orientacion original sin espejo.
2. ¿La cantidad de parrilla debe poder variar por frente/nivel, o basta el valor global? (mediano plazo, segun uso real)

## 14. Como reanudar el trabajo

1. Clonar `https://github.com/marioap-afk/Calculadora_de_racks.git` y abrir la rama `codex/dinamico-modular`.
2. `git log --oneline -5` y comparar con la seccion 8 de este archivo (¿hubo push nuevo?).
3. Leer en orden: este archivo -> [README.md](../README.md) -> [AGENTS.md](../AGENTS.md) ->
   [docs/00-indice-contexto.md](00-indice-contexto.md).
4. `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` (debe descubrir 627 y quedar verde).
5. Tomar la primera tarea de la seccion 11 que siga abierta.

**Prompt de reanudacion (copiar en un chat nuevo de Claude o Codex):**

```
Trabajo en RackCad, plugin de AutoCAD 2025 en C#/.NET8, rama codex/dinamico-modular. Lee primero
docs/HANDOFF.md, luego README.md y AGENTS.md; verifica el estado real con git log y dotnet test
(627 tests verdes al cerrar este worktree). El contexto de estado, bugs conocidos
y siguientes tareas esta en las secciones 9-11 del HANDOFF. Continua con: [elige la tarea o pega la
seccion 11]. Respeta las convenciones de AGENTS.md (en especial: DeepCopy + DTO para flags de
seguridad, tests de regresion verificados fallando, y no hacer push sin verificacion en AutoCAD).
```

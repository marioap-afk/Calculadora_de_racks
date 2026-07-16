# Project Handoff

> Documento canonico de continuidad entre sesiones (Claude, Codex o un desarrollador nuevo).
> Actualizado: **2026-07-15**. Sobre la base `release/claude-review`, el worktree del dinamico separa ya
> diseno editable y sistema resuelto: 553 tests y builds Debug verdes. Bota C, Poste tope,
> Lateral C, los siete desviadores A/L y la base modular del dinamico estan confirmados en AutoCAD; base del
> worktree `cd20200`.
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

**Estado**: activo y funcional. El arbol actual tiene **553/553 tests verdes** y build Debug completo con 0 errores;
solo aparecen los `MSB3277` conocidos de las referencias de AutoCAD. La base publicada conserva su validacion manual
de parrilla, larguero tope, rejilla, persistencia, biblioteca y rendimiento. Las variantes nuevas Bota C 4/6, Poste
tope y Lateral C 4/6 tambien estan verificadas por pruebas, compilacion y comprobacion visual del usuario en AutoCAD.
Los siete desviadores A/L estan implementados, cubiertos por pruebas y validados con sus bloques DWG reales.

## 2. Estado comprobado

| Aspecto | Estado | Evidencia |
|---|---|---|
| Compilacion Debug (Domain/Application/UI/Plugin) | **OK, 0 errores** | `dotnet build RackCad.sln -c Debug`; 2 familias MSB3277 conocidas, 2026-07-15 |
| Pruebas unitarias | **553/553 verdes** | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug`, 2026-07-15 |
| Validacion estatica del arbol actual | OK | sintaxis 233 C#; semantica Domain/Application (114), Domain/Application/UI (146 fuentes + 11 XAML generados previos) y Domain/Application/tests (173); 12 XML/XAML bien formados |
| Carga en AutoCAD (NETLOAD del Debug) | **OK en el arbol actual** | Seguridad publicada + base modular del dinamico confirmadas, 2026-07-15 |
| Commits `b1cfce2`..`95e25f2` (tope medio-frente, parrilla) | Implementado, testeado y verificado en AutoCAD | Ver seccion 9 |
| Commits `aa42986`, `c11a267`, `a9f1c13` (variantes de seguridad y desviadores) | Implementados, testeados, verificados en AutoCAD y publicados | Ver secciones 8-9 |
| Release build / bundle de despliegue | No reconstruido en la ultima sesion | `deploy/install-bundle.ps1 -Build` requiere AutoCAD cerrado |

## 3. Estado por funcionalidad

| Funcionalidad | Estado | Codigo principal | Pruebas | Observaciones |
|---|---|---|---|---|
| Cabecera (marco): editor + dibujo + round-trip | completo | `src/RackCad.UI/RackFrameConfiguratorWindow*`, `src/RackCad.Plugin/Headers/` | `tests/RackCad.Tests/*Frame*` | Peralte de placa editable por placa |
| Sistema dinamico (pallet flow) | lateral funcional; cierre multiplavista pendiente | `RackDynamicSystemWindow`, `DynamicRackDesign`, `DynamicRackSystemResolver`, `DynamicSystem*` | `Dynamic*Tests` | Base modular diseno -> resuelto; aun sin frontal/planta, varios frentes/anchos ni cama integrada |
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
- **Flujo del dinamico** (base modular vigente): `DynamicRackDesign` (Domain, entradas editables y modulos sin
  coordenadas) -> `DynamicRackSystemResolver.Resolve` (Application) -> `DynamicRackSystem` (lateral resuelto) ->
  `DynamicSystemLateralBuilder` -> `DynamicSystemPlan` -> `DynamicSystemDrawService`. El DWG y la biblioteca
  persisten el diseno; `StartX`/`EndX` se regeneran. Este limite prepara frontal/planta y varios frentes/anchos,
  pero esos contratos fisicos aun no estan implementados.
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
- Alcance deliberado: no se agregaron bloques, reglas de ancho/frentes, frontal/planta ni cama integrada; el BOM
  vigente sigue listando cabeceras. Cuando se integre la cama, el contrato aprobado es un componente `Cama` por cama,
  sin despiece comercial en esta primera etapa.
- 7 pruebas nuevas; total **553/553 verdes**. Regresiones observadas con el fix desactivado: cabecera personalizada
  150 -> 324 al recalcular; niveles persistidos 5 -> 3 al reabrir; cabecera legacy se marcaba calculada y se perdia.

## 9. Ultima validacion manual

- **AutoCAD 2025, NETLOAD del Debug, 2026-07-15: OK confirmado por el usuario.**
- Biblioteca: disponible y reportada correctamente por `RACKCAD`.
- Rejilla sin larguero a piso: filas resueltas correctas, sin fila muerta ni desplazamiento.
- Parrilla: frente/cantidad, posicion en frontal/lateral y BOM correctos.
- Tope: rejilla, posicion y opciones correctas.
- Variantes de seguridad: selectores exclusivos, Bota C 4/6, Poste tope, Lateral C 4/6 y los siete desviadores A/L
  funcionan correctamente en sus vistas y BOM.
- Persistencia/round-trip: configuracion conservada tras `RACKEDITAR` y actualizar.
- Rendimiento: sin degradacion perceptible en el escenario probado.
- Sistema dinamico modular: **OK confirmado por el usuario** tras probar el DLL Debug; no se cambio ningun bloque DWG.

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
6. **Dinamico aun es solo lateral**: faltan el modelo de varios frentes y anchos, builders frontal/planta y la
   composicion de la cama completa. No definir offsets ni nombres de bloque hasta que el usuario reconfigure y
   confirme los bloques reales.
7. **Fallback legacy conservador**: una cabecera dinamica de un documento antiguo no declaraba si era calculada;
   se abre como personalizada para evitar perdida de datos. `Restaurar estandar` o `Calculada` vuelve a derivarla.

## 11. Siguientes tareas recomendadas

1. **Largueros especiales de entrada y salida del dinamico**: siguiente paso acordado. Esperar a que el usuario
   cree los bloques reales y modifique los CSV; despues implementar primero su colocacion en la vista lateral,
   consumiendo exclusivamente nombres/parametros catalogados.
2. **Largueros intermedios del dinamico**: abordar despues de validar entrada/salida. No anticipar su geometria ni
   reutilizar por suposicion un bloque de larguero selectivo.
3. **Componer la cama dinamica dentro del diseno/resolucion del sistema** reutilizando `FlowBedLateralBuilder`, no
   duplicandolo. Su builder dibujara la cama completa; el BOM inicial solo contara componentes `Cama`.
4. **Varios frentes/anchos y vistas frontal/planta**: continuar sobre `DynamicRackDesign -> DynamicRackSystemResolver`
   cuando esten confirmadas sus referencias fisicas; no acoplar esos ejes a los modulos longitudinales de la lateral.
5. **Overrides de parrilla por frente/nivel** (item 2 de la seccion 10): mantener a mediano plazo; hoy los valores
   globales son suficientes, pero el control por celda puede aportar valor en configuraciones heterogeneas.
6. **Guardas traseras**: mantener pendientes hasta el final; no son prioridad de producto.

Quedan diferidos sin prioridad actual: tarima/parrilla en PLANTA, integracion BOM -> cotizador, distribucion formal
de `blocks-library.dwg` y reconstruccion del bundle Release. El BOM actual es suficiente; el cotizador real es un
Excel delicado y no justifica el riesgo de integracion. No es necesario desplegar la aplicacion todavia.

No tomar sin confirmar con el usuario: validacion de cargas (diferida a RAM Elements) y el optimizador IA
de layout (meta futura, no inmediata).

## 12. Verificacion del proyecto (ultima ejecucion real)

| Verificacion | Comando | Resultado | Fecha/entorno |
|---|---|---|---|
| Build Debug (todo) | `dotnet build RackCad.sln -c Debug -v:minimal` | **OK, 0 errores, 2 advertencias MSB3277 conocidas** | 2026-07-15, Windows 11, SDK 8.0.423 por usuario |
| Build Plugin Debug | `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -v:minimal` | **OK, 0 errores, 2 advertencias MSB3277 conocidas** | 2026-07-15 |
| Pruebas | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug` | **553/553 verdes**, 0 omitidas | 2026-07-15 |
| Build UI aislado | `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug -o %TEMP%/RackCad-ui-dynamic-modular` | **OK, 0 errores, 0 advertencias** | 2026-07-15 |
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
| Release / bundle | `pwsh deploy/install-bundle.ps1 -Build` | **no ejecutada** en esta sesion (requiere AutoCAD cerrado y no era necesaria) | — |
| Verificacion manual AutoCAD | NETLOAD + `RACKCAD`/`RACKEDITAR` | Seguridad publicada + base modular del dinamico **OK, confirmado por el usuario** | 2026-07-15 |

## 13. Preguntas abiertas

1. Cuando existan los bloques reconfigurados: confirmar origen/parametros de frontal, lateral, planta y cama, y la
   relacion geometrica de varios frentes/anchos. No es bloqueante para la base modular actual.
2. ¿La cantidad de parrilla debe poder variar por frente/nivel, o basta el valor global? (mediano plazo, segun uso real)

## 14. Como reanudar el trabajo

1. Clonar `https://github.com/marioap-afk/Calculadora_de_racks.git` y abrir la rama `release/claude-review`.
2. `git log --oneline -5` y comparar con la seccion 8 de este archivo (¿hubo push nuevo?).
3. Leer en orden: este archivo -> [README.md](../README.md) -> [AGENTS.md](../AGENTS.md) ->
   [docs/00-indice-contexto.md](00-indice-contexto.md).
4. `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` (debe descubrir 553+ y quedar verde).
5. Tomar la primera tarea de la seccion 11 que siga abierta.

**Prompt de reanudacion (copiar en un chat nuevo de Claude o Codex):**

```
Trabajo en RackCad (D:\Documentos\Codex\Calculadora de racks), plugin de AutoCAD 2025 en C#/.NET8,
rama release/claude-review. Lee primero docs/HANDOFF.md, luego README.md y AGENTS.md; verifica el
estado real con git log y dotnet test (546 tests verdes en este arbol). El contexto de estado, bugs conocidos
y siguientes tareas esta en las secciones 9-11 del HANDOFF. Continua con: [elige la tarea o pega la
seccion 11]. Respeta las convenciones de AGENTS.md (en especial: DeepCopy + DTO para flags de
seguridad, tests de regresion verificados fallando, y no hacer push sin verificacion en AutoCAD).
```

# I-18 — Validación manual en AutoCAD (Push Back)

> **Estado: `approved-owner` (2026-07-25).** El Owner ejecutó el retest final en AutoCAD 2025 y
> **aprobó** los seis hallazgos (PB-VAL-01…06) y la geometría Push Back. El **preview visual** queda
> **diferido** a una iniciativa transversal futura y **no bloquea** I-18: no ha sido aprobado
> visualmente y no se registra como tal. Ver §11 para el resultado y el `CODE_SHA` vigente.
>
> El cuerpo que sigue (§1-§10) conserva el registro **histórico** del intento de gate de
> `bca2abb`, que el Owner rechazó en su momento; no se reescribe para no perder la trazabilidad.

## 1. Identificación

| Campo | Valor |
|---|---|
| Initiative | I-18 (Push Back) |
| Phase | I-18b — increment 5 (gate manual) |
| Status | `pending-owner` |
| Rama | `feature/push-back` |
| `CODE_SHA` | `bca2abb2a827a6a43c733777ecef00b14c093712` |
| Run de CI | [30048580953](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30048580953) |
| Conclusión de CI | **success** (4/4 jobs: Tests Domain+Application · UI Tests · Build UI · Build Plugin without AutoCAD) |
| .NET SDK | 8.0.423 |
| AutoCAD objetivo | **2025** |
| Worktree | `C:\Users\alejandra-mendoza\.codex\worktrees\feature-push-back` |

La DLL y el inventario de esta evidencia se construyeron **desde `CODE_SHA`**, con el árbol limpio
y después de que la CI de ese commit quedara verde.

## 2. Bundle canónico

Generado con el flujo soportado (`deploy/build-bundle.ps1`, I-12), configuración **Debug**:

```powershell
pwsh deploy/build-bundle.ps1 -Configuration Debug -InventoryOutPath "$env:TEMP\I-18-bundle-first.txt"

pwsh deploy/build-bundle.ps1 -Configuration Debug `
  -InventoryOutPath "docs/automation/evidence/I-18-bundle-inventory.txt" `
  -BaselineInventoryPath "$env:TEMP\I-18-bundle-first.txt"
```

| Artefacto | Ruta |
|---|---|
| Bundle (relativa) | `src\RackCad.Plugin\bin\Debug\net8.0-windows\publish\RackCad.bundle` |
| Bundle (absoluta) | `C:\Users\alejandra-mendoza\.codex\worktrees\feature-push-back\src\RackCad.Plugin\bin\Debug\net8.0-windows\publish\RackCad.bundle` |
| **DLL a cargar** (relativa) | `src\RackCad.Plugin\bin\Debug\net8.0-windows\publish\RackCad.bundle\Contents\RackCad.Plugin.dll` |
| **DLL a cargar** (absoluta) | `C:\Users\alejandra-mendoza\.codex\worktrees\feature-push-back\src\RackCad.Plugin\bin\Debug\net8.0-windows\publish\RackCad.bundle\Contents\RackCad.Plugin.dll` |

### Contenido verificado

- `PackageContents.xml` — presente.
- `Contents\RackCad.Plugin.dll`, `RackCad.Application.dll`, `RackCad.Domain.dll`, `RackCad.UI.dll` — presentes.
- `Contents\catalogs\` — presente (11 archivos), SHA-256 idénticos a `assets/catalogs`.
- **Cero DLL de Autodesk** dentro del bundle (`AcMgd`, `AcDbMgd`, `AcCoreMgd` ausentes) — ADR-0003.
- Verificador fail-closed: **107 comprobaciones OK** en la segunda publicación (105 en la primera,
  las 2 extra son la comparación de reproducibilidad).

### Reproducibilidad

Dos publicaciones independientes desde el mismo `CODE_SHA` produjeron **inventarios y hashes idénticos**:
salida literal del verificador — `Reproducibilidad: el inventario y los hashes coinciden con el base.`
Inventario completo versionado en [`I-18-bundle-inventory.txt`](I-18-bundle-inventory.txt).

### SHA-256

Las cuatro DLL y el manifiesto:

| Archivo | SHA-256 |
|---|---|
| `Contents/RackCad.Plugin.dll` | `5B782853F47FB406ED540111B4E2DE3E14661EFE44ED5D8AE53409EE90E1119D` |
| `Contents/RackCad.Application.dll` | `F8BF666CD575D058A3673A8122F21E67555AE20A3705298868D52888A31BED37` |
| `Contents/RackCad.Domain.dll` | `F4F7FE471DE8C586CCCC8CAE3E1A250CCD3FBB507B4CC5A9BD6998391E0AF185` |
| `Contents/RackCad.UI.dll` | `F290785DAAF10CF9826BCE3F994570AD95A01F0B6064D984D1041DD1C55A41FC` |
| `PackageContents.xml` | `25F184187E21ED5F36FAFF11B1FFE3B4038E8666CAE663D9F34E74F6ACEC6D18` |

Catálogos **modificados por I-18** (PB-0 del Owner; `git diff origin/main -- assets/catalogs`):

| Catálogo | Cambio | SHA-256 (en el bundle) |
|---|---|---|
| `catalogs/blocks.csv` | +3 filas | `7B4B7677A8F307499892C97D74619590877C865617EBCCA313204B2FDA762874` |
| `catalogs/connection-layout.csv` | +4 filas | `C9A27A10F87CA6BB5A615A39FED070BB75949150F43B9DCFBC44C19E6D847929` |
| `catalogs/secciones.csv` | +1 fila | `8E5F2E9C22EEB54210CE49C8BA145EC5A102DE430A533244E37D5D0DA65F5EE1` |

## 3. Verificación automática (toda verde sobre `CODE_SHA`)

| Gate | Resultado |
|---|---|
| `dotnet test` — RackCad.Tests | **1160** superadas, 0 fallidas, 0 omitidas |
| `dotnet test` — RackCad.UI.Tests | **224** superadas, 0 fallidas, 0 omitidas |
| `dotnet build src/RackCad.UI -c Debug` | **0 errores** |
| `dotnet build src/RackCad.Plugin -c Debug` | **0 errores** (2 MSB3277 conocidos de AcMgd/AcDbMgd) |
| Validador de catálogos I-19 | **51** verdes (baseline intacto) |
| Golden Push Back (`PushBackGoldenTests`) | **1** verde (fija las 6 firmas SHA-256 de I-18a) |
| Golden dinámico (`DrawServicePlanBaselineTests`) | **8** verdes |
| Persistencia I-11 (`Persistence*`) | **117** verdes |
| Handlers + guards del Plugin | **66** verdes |
| Editor + módulo + menú (UI) | **56** verdes |

Ningún filtro se aceptó con cero pruebas encontradas.

### Cadena end-to-end cubierta automáticamente

`RACKCAD → PushBackEditorModule → RackPushBackSystemWindow → PushBackInsertionRequest →
RackPushBackCommands.DrawPushBackView → envelope KindPushBack → draw service → bloque ligado →
KindHandlerRegistry → PushBackKindHandler → EditPushBack / BuildBom / RestampDesign`

- `RACKPUSHBACK` y `RPB` no colisionan con ningún otro `[CommandMethod]` (guard que escanea todo el Plugin).
- El menú y el comando directo convergen en `DrawPushBackView` (una sola ruta de dibujo).
- `RACKEDITAR` resuelve por `KindHandlerDispatch.TryResolve`, sin rama Push Back.
- `RACKBOMTOTAL` acepta Push Back por `TryResolveAll`.
- `RACKDUPLICAR` y `RACKLAYOUT` aceptan Push Back por `TryResolveIgnoreCase`, sin rama por kind.
- Restamp: cambia GUID + nombre del envelope y deja el JSON interno Push Back **byte-idéntico**
  (prueba pura); la copia es funcionalmente igual con identidad independiente.
- Envelope → proyecto → resolver → BOM coherente (prueba pura, misma secuencia que `BuildBom`).
- Biblioteca → ventana → insertion request conserva metadata I-11 (UI.Tests).
- Todos los golden de I-18a siguen fijos.

> **Límite de la verificación automática.** `RackCad.Tests` no carga el ensamblado del Plugin
> (referencias AutoCAD, ADR-0003): el cableado del Plugin se congela con *source guards* sobre el
> texto de los `.cs`. Nada de lo anterior sustituye la comprobación **visual** de geometría en
> AutoCAD, que es justamente el objeto de este gate.

---

## 4. Guion manual del Owner

Ejecutar en orden. Registrar el resultado en la columna **Resultado Owner** de la sección 5
(`OK` / `FALLA` + nota). Cualquier `FALLA` detiene el gate y vuelve a implementación.

### A. Carga

1. Cerrar cualquier AutoCAD que se haya usado durante el build.
2. Abrir **AutoCAD 2025**.
3. Abrir un DWG nuevo **en pulgadas**.
4. Ejecutar `NETLOAD`.
5. Cargar exactamente:
   `C:\Users\alejandra-mendoza\.codex\worktrees\feature-push-back\src\RackCad.Plugin\bin\Debug\net8.0-windows\publish\RackCad.bundle\Contents\RackCad.Plugin.dll`
6. Confirmar que `RACKCAD`, `RACKPUSHBACK` y `RPB` son reconocidos.

### B. Sistema de prueba

Crear `PB-I18-E2E` con: tarima 42 × 48 × 60 in; peso 1000 kg; al menos dos frentes;
**frente 1** con 3 niveles, 5+ fondos y `DepthStartPosition = 1`; **frente 2** con 2 niveles, fondo
distinto y `DepthStartPosition` distinto; al menos tres peraltes posteriores diferentes; un override
de longitud por nivel; un tope posterior desactivado; al menos una seguridad permitida en el extremo
bajo; **ninguna guía**. Insertar inicialmente un corte lateral.

### C. Vistas ligadas

Con `RACKEDITAR` → «Insertar vista», agregar progresivamente: segundo corte lateral, frontal
entrada/salida, frontal posterior y planta. Ejecutar `RACKLISTA`.

### D. Geometría visual

Verificar con `DIST`/`MEASUREGEOM` donde aplique y **registrar las medidas**.

### E. Actualización multivista

`RACKEDITAR` desde una vista distinta de la inicial; cambiar nombre a `PB-I18-E2E-EDIT`, un peralte
posterior, el estado de un tope y una dimensión estructural; pulsar **Actualizar**. Después reducir
la estructura para dejar obsoleto al menos un corte lateral.

### F. BOM — `RACKBOMTOTAL`. ### G. Copia — `RACKDUPLICAR` (y `RACKLAYOUT` si aplica).
### H. Biblioteca. ### I. Persistencia del DWG. ### J. Alias y cancelación.

---

## 5. Checklist manual (ninguna fila aprobada)

| ID | Paso | Resultado esperado | Resultado Owner | Notas/evidencia |
|---|---|---|---|---|
| A1 | Cerrar AutoCAD usado en el build | Sin instancias abiertas | | |
| A2 | Abrir AutoCAD 2025 | Arranca correctamente | | |
| A3 | DWG nuevo en pulgadas | `INSUNITS` = pulgadas; sin aviso de unidades | | |
| A4 | `NETLOAD` de la DLL indicada | Carga sin error | | |
| A5 | Ejecutar `RACKCAD` | Abre el menú principal | | |
| A6 | Ejecutar `RACKPUSHBACK` y `RPB` | Ambos reconocidos; abren el mismo editor | | |
| B1 | Tarima 42 × 48 × 60 in, 1000 kg | Se aceptan los valores | | |
| B2 | Dos o más frentes | La matriz refleja los frentes | | |
| B3 | Frente 1: 3 niveles, 5+ fondos, `DepthStartPosition = 1` | Aceptado | | |
| B4 | Frente 2: 2 niveles, fondo distinto, `DepthStartPosition` distinto | Aceptado | | |
| B5 | Tres o más peraltes posteriores diferentes | Cada celda conserva su peralte | | |
| B6 | Un override de longitud por nivel | Aceptado y reflejado | | |
| B7 | Un tope posterior desactivado | La celda queda sin tope | | |
| B8 | Una seguridad permitida en el extremo bajo | Aceptada | | |
| B9 | Ninguna guía disponible ni persistida | La guía no se ofrece | | |
| B10 | Insertar un corte lateral | Se dibuja y queda ligado | | |
| C1 | Insertar segundo corte lateral | Se dibuja ligado | | |
| C2 | Insertar frontal entrada/salida | Se dibuja ligado | | |
| C3 | Insertar frontal posterior | Se dibuja ligado | | |
| C4 | Insertar planta | Se dibuja ligada | | |
| C5 | `RACKLISTA` | Un solo rack `PB-I18-E2E`; todas las vistas bajo la misma identidad; **una** copia física, no una por vista | | |
| D1 | Extremo de entrada/salida | En el extremo **bajo** | | |
| D2 | Extremo posterior | En el extremo **alto** | | |
| D3 | Pendiente | Ascendente hacia el posterior | | |
| D4 | Valor de pendiente | Equivalente a **7/16 in por pie** | | medir |
| D5 | `IN/OUT` | Únicamente en el extremo bajo | | |
| D6 | `LARGUERO_ESCALON_TROQUEL_REDONDO` | Únicamente en el posterior | | |
| D7 | Peraltes posteriores | Diferentes por celda, según lo configurado | | |
| D8 | Larguero posterior | Misma longitud transversal que su `IN/OUT` | | medir |
| D9 | Cama | Longitud física completa, **sin descuento de 4 in** | | medir |
| D10 | Cama | **Sin frenos** | | |
| D11 | Intermediarios | Tangentes al eje de la cama | | |
| D12 | Topes posteriores | Presentes salvo en la celda desactivada | | |
| D13 | Seguridad normal | Solo en el extremo bajo; **ninguna guía** | | |
| D14 | Frontal y planta | Coherentes con los cortes laterales | | |
| E1 | `RACKEDITAR` desde otra vista | Abre el editor con todos los datos | | |
| E2 | Renombrar a `PB-I18-E2E-EDIT` | Aceptado | | |
| E3 | Cambiar un peralte posterior, un tope y una dimensión | Aceptados | | |
| E4 | Pulsar **Actualizar** | Todas las vistas ligadas cambian | | |
| E5 | Nombres de bloque | Sincronizados en todas las vistas | | |
| E6 | Identidad | **No** aparece una identidad nueva | | |
| E7 | Regen | Un solo regen visual coherente | | |
| E8 | `RACKLISTA` tras editar | Sigue mostrando **un** rack | | |
| E9 | Reducir estructura hasta dejar un corte obsoleto | El corte obsoleto se elimina; las vistas supervivientes permanecen; el mensaje informa el corte eliminado; **nunca** se elimina el último vínculo | | |
| F1 | `RACKBOMTOTAL` | Se ejecuta sin error | | |
| F2 | Tipo mostrado | **Push Back** | | |
| F3 | Cantidades | Coherentes con lo dibujado | | |
| F4 | `IN/OUT` y posterior | Uno de cada por celda | | |
| F5 | Longitudes | Reflejan los overrides | | |
| F6 | Topes | Solo los activos | | |
| F7 | Frenos y guías | Ninguno | | |
| F8 | Copia física | Contabilizada **una vez**, no una vez por vista | | |
| G1 | `RACKDUPLICAR` sobre una vista | Coloca una copia | | |
| G2 | `RACKLISTA` | Aparece un rack independiente | | |
| G3 | Identidad de la copia | GUID/identidad **diferente** | | |
| G4 | Nombre de la copia | Nombre de copia aplicado | | |
| G5 | Editar la copia | **No** modifica el original | | |
| G6 | Editar el original | **No** modifica la copia | | |
| G7 | `RACKLAYOUT` (si aplica) | Acepta Push Back; identidad por celda correcta | | |
| H1 | Guardar el sistema en la biblioteca | Se guarda | | |
| H2 | Abrir desde `RACKCAD` → biblioteca | Aparece y abre | | |
| H3 | Insertar como rack nuevo | Se inserta | | |
| H4 | Identidad | **GUID nuevo**; el original no queda ligado al nuevo | | |
| H5 | Datos | Diseño y topes preservados; metadata y versión **no** se degradan | | |
| I1 | Guardar el DWG | Guarda sin error | | |
| I2 | Cerrar y reabrir AutoCAD y el DWG | Abre sin error | | |
| I3 | `RACKLISTA` tras reabrir | Identidad y vistas intactas | | |
| I4 | `RACKEDITAR` tras reabrir | Diseño y topes sobreviven | | |
| I5 | `RACKBOMTOTAL` tras reabrir | BOM sobrevive | | |
| J1 | `RPB` | Abre el mismo editor que `RACKPUSHBACK` | | |
| J2 | Cerrar/cancelar el editor | **No** modifica el DWG | | |
| J3 | Entrada numérica inválida | Bloquea insertar, actualizar, BOM y guardar | | |

## 6. Resolución del gate

- **Todo OK** → el Owner lo declara aprobado; recién entonces se marca `owner_validation.status: approved`
  y se puede declarar `i18b_complete`.
- **Cualquier FALLA** → se registra en la fila correspondiente, el gate queda rechazado y el trabajo
  vuelve a implementación; esta evidencia se actualiza con la corrección y un `CODE_SHA` nuevo.

Mientras esta línea siga aquí, el gate está **pendiente**: nadie más que el Owner puede aprobarlo.

---

## 11. Resultado del gate manual — APROBADO (2026-07-25)

### 11.1 Identificación vigente

| Campo | Valor |
|---|---|
| Status | **`approved-owner`** |
| `CODE_SHA` | `ec00dabab52e9715468998028f7e073572474595` |
| Run de CI | [30137555378](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30137555378) — **success** |
| Bundle | `src/RackCad.Plugin/bin/Debug/net8.0-windows/publish/RackCad.bundle` |
| Inventario | [`I-18-bundle-inventory.txt`](I-18-bundle-inventory.txt) — 16 archivos, **reproducible** |

#### Cómo leer los SHA de esta sección

El sello de versión del plugin (`AssemblyInformationalVersion`) **incorpora el SHA del HEAD**, de modo que
**cualquier** commit —incluso uno de solo documentación— cambia el SHA-256 del DLL sin cambiar una línea de
código. Por eso esta evidencia **no** declara «el HEAD de la rama es X»: declara el **commit en el que se
construyó** el artefacto medido. Ese enunciado es reproducible y no caduca.

| Referencia | Valor | Qué significa |
|---|---|---|
| **`CODE_SHA` validado por el Owner** | `ec00dabab52e9715468998028f7e073572474595` | El commit cuyo DLL cargó el Owner en AutoCAD 2025 para el retest final. **Fijo e inmutable.** |
| **Último commit de CÓDIGO** | `1b37918` — *«el tope mate por su ORIGEN contra el `TROQUEL_TOPE` del poste»* | Último commit que tocó `src/` o `tests/`. **Todo lo posterior es documentación**, así que el código entregable es el mismo que el Owner validó. |
| **`BUILD_SHA`** (artefacto medido abajo) | `512ad90c4e1c9389f04acc59988a9e8fa0e5dfcf` | Commit en el que se construyó y verificó el artefacto de §11.1. |

#### Artefacto entregable, medido sobre `BUILD_SHA` con árbol limpio

| Campo | Valor |
|---|---|
| Ruta del DLL | `src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll` |
| SHA-256 del DLL | `4E1C178D481C9543F8AC00F29749D912F3E947778D9C424285AE0FFD89AABC65` |
| `InformationalVersion` | `1.0.0+512ad90c4e1c9389f04acc59988a9e8fa0e5dfcf` |
| DLL dentro del bundle | **idéntico** al de `bin/Debug` (mismo SHA-256) |
| Comprobaciones canónicas | **105**, fail-closed — DLL idénticos al publish, catálogos idénticos a `assets/catalogs`, solo archivos RackCad y datos permitidos, **cero DLL de Autodesk** |
| Reproducibilidad | dos publicaciones **independientes** produjeron un inventario **idéntico** (16 archivos, mismos SHA-256), comparado archivo por archivo |

Para reproducirlo exactamente:

```powershell
git checkout 512ad90c4e1c9389f04acc59988a9e8fa0e5dfcf
pwsh deploy/build-bundle.ps1 -Configuration Debug
```

Reconstruir en un commit documental posterior produce **el mismo código** con un sello de versión distinto
(y, por tanto, otro SHA-256): eso es esperado y no indica cambio de producto.

### 11.2 Hallazgos

| Hallazgo | Resultado | Nota |
|---|---|---|
| **PB-VAL-01** — interfaz alineada con el Dinámico | **Aprobado** | Con el **preview visual diferido** (§11.4). Composición, panel frente/celda, matriz de tarjetas y barra de acciones aprobadas. |
| **PB-VAL-02** — orientación y anclaje del larguero tope | **Aprobado** | Lateral sobre `TROQUEL_SEPARADOR`; frontal posterior y planta por **mate de origen** contra el `TROQUEL_TOPE` del poste. |
| **PB-VAL-03** — elevación adicional de 4" | **Aprobado** | Dos pasos exactos de troquel: la retícula se preserva por construcción. |
| **PB-VAL-04** — seguridad por defecto en un rack nuevo | **Aprobado** | Sembrada desde la autoridad canónica, solo extremo bajo, sin GUIA ni PARRILLA. |
| **PB-VAL-05** — tangencia de la cama | **Aprobado** | El IN/OUT bajo queda atornillado (`TROQUEL_CAMA` == `TROQUEL_IN`); el larguero **posterior** es el que se hace tangente a la línea del origen de la cama. |
| **PB-VAL-06** — Push Back no admite parrillas | **Aprobado** | Excluidas en UI, resolver, sistema, planes, dibujo y BOM; lectura legacy no destructiva. |

Además: **Seguridad con sección visible de topes: OK** — el tope se configura dentro de «Elementos de
seguridad» (encabezado, estado y botón «Configurar…»), nunca como `SelectiveSafetySelection`.

**La geometría Push Back queda aprobada.**

### 11.3 Verificación automática que acompaña al gate

| Gate | Resultado |
|---|---|
| `RackCad.Tests` | **1201** verde |
| `RackCad.UI.Tests` | **343** verde |
| Build Debug UI | 0 errores, 0 advertencias |
| Build Debug Plugin | 0 errores (2 `MSB3277` conocidos de las referencias AutoCAD) |
| Golden Push Back (5 vistas + BOM) | verde |
| Suites Dinámico / Selectivo / cama | 188 / 323 / 33 verde |
| Persistencia (round-trip, legacy, metadata I-11) | 124 verde |
| Shell (UI) | 63 verde |
| Handlers / registros | 11 / 21 verde |
| Estados de editor | 51 verde |
| Validador I-19 | 22 verde |
| Bundle | 105 comprobaciones fail-closed; inventario **reproducible** |

Ningún filtro descubrió cero pruebas.

### 11.4 Preview visual — diferido, NO aprobado

El Owner considera los previews **todavía insatisfactorios**, pero **no bloqueantes** para I-18. Su
estandarización completa se **difiere a una iniciativa transversal futura**. En este cierre **no** se declara
aprobado visualmente el preview: lo aprobado es la geometría y el resto de la experiencia.

Lo entregado y verificado en la rama es la infraestructura de preview **compartida**, extraída del renderer
Dinámico (paleta, superficie con la transformación y las primitivas, y partes por familia) y consumida por
los dos editores, con la equivalencia del Dinámico **medida**: misma firma de escena sobre 736 primitivas
antes y después de extraer.

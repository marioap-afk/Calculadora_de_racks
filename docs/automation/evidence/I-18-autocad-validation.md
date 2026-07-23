# I-18 — Validación manual en AutoCAD (Push Back)

> **Estado: `pending-owner`.** Ninguna fila del checklist manual está aprobada. La verificación
> automática está completa y verde; falta exclusivamente la validación humana del Owner en
> AutoCAD 2025 sobre la DLL exacta descrita abajo.

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

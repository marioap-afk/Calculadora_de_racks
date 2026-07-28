# Evidencia — I-36C · Acceso desde el menú principal al generador de perfiles estructurales

> Estado: **`review-ready`**, gate **`owner-validation`**.
> **No integrada. No limpiada. `main` intacta.** El checklist de AutoCAD está en §8.

---

## 0. Lo primero, para que no se malinterprete

**El catálogo y el generador paramétrico de perfiles estructurales YA ESTÁN IMPLEMENTADOS.** El botón
del menú principal **solo incorpora un acceso visible a la funcionalidad existente; no crea un segundo
generador.**

Merece decirse en la primera línea porque el riesgo de este cambio no es que el botón falle —eso se ve
al primer clic— sino que alguien lea «generador de perfiles» en el registro y crea que aquí se
implementó uno.

---

## 1. Preflight

| Comprobación | Resultado |
|---|---|
| Base remota | `14317a5` — *Merge I-36B: geometría y representación prismática de secciones estructurales* |
| CI de la base | **verde 4/4**, run `30382411720`, sobre el SHA exacto de `origin/main` |
| `main` == `origin/main` | sí |
| Árbol de trabajo | limpio |
| Stashes | cero |
| Operaciones en curso | ninguna (sin merge, rebase, cherry-pick ni bisect) |
| Worktrees previos | uno solo, el principal del dueño |
| Ramas remotas | solo `origin/main` |
| Pull requests abiertos | cero |
| Rama objetivo | `fix/acceso-menu-secciones-estructurales`, **libre** en local y en remoto |

`origin/main` **no avanzó** durante la ejecución.

## 2. Reclamo

| Dato | Valor |
|---|---|
| Identificador | **I-36C** |
| Rama | `fix/acceso-menu-secciones-estructurales` |
| Worktree | `.claude/worktrees/fix-acceso-menu-secciones-estructurales` |
| Commit de reclamo | `6db84c4` (vacío) |
| `Claim-Id` | `3afd368a-0eb4-44aa-8c8f-ebde72ed256f` |
| Primer push | aceptado, sin `--force` |

**Por qué I-36C y no I-37.** El ROADMAP **reserva I-37 e I-38** para Cantilever y su ingeniería
estructural. Este trabajo pertenece a la misma familia que I-36A e I-36B y sigue su convención de
sufijo. No tiene fila en ROADMAP todavía: se escribe en la sesión de integración, porque
[`WORKFLOW.md`](../../WORKFLOW.md) §8 prohíbe tocar ROADMAP y HANDOFF desde una rama paralela.

---

## 3. RESUMEN DE LO YA IMPLEMENTADO

### 3.a I-36A — Catálogo (integrada y cerrada, 2026-07-28)

- **Catálogo neutral `StructuralSection`**: la sección transversal deja de ser lo mismo que el
  **miembro** (para qué se usa) y que la **pieza comercial** (qué SKU se compra). Una `StructuralSection`
  **no tiene rol**: `POSTE`, `CELOSIA`, `LARGUERO` y `SEPARADOR` no existen en su esquema.
- **Fuente**: AISC Shapes Database **v16.0**, SHA-256 verificado; el libro no se versiona.
- **983 perfiles**: **W 289**, **HSS rectangular/cuadrado 525**, **C 32**, **L 137**. Cero filas
  seleccionadas rechazadas.
- **IDs estables**: `{ID_NAMESPACE}-{FAMILIA}-{EDI_NORMALIZADO}`, con normalización determinista y **sin
  la revisión dentro del id**.
- **Manifiesto y hashes**: conteos y SHA-256 por archivo, sin timestamps; la CI los recalcula en Linux
  sobre un checkout limpio.
- **Importador reproducible fuera del producto** (`tools/`, .NET 8, BCL puro, cero NuGet, cero Office
  Interop): salida **byte-idéntica** entre ejecuciones y **cero descartes silenciosos**.
- **Lector CSV estricto** dedicado: encabezado faltante, duplicado o desconocido, id vacío, número, bool
  o enum inválido, `NaN`/infinito y requerido ausente son **errores** con archivo, fila, columna e id.
  La tolerancia histórica de `CsvCatalogReader` queda intacta.
- **Overlay de habilitación**: deshabilitar una sección la retira de las selecciones nuevas, pero
  `GetById` **la sigue resolviendo**, así que un diseño guardado no deja de abrirse.
- **Carga fail-closed**: `Load()` valida y falla; no hay forma pública de obtener un catálogo sin validar.
- **Separación sección / miembro / pieza comercial**, que es la razón de ser de todo lo anterior.

### 3.b I-36B — Geometría (integrada y cerrada, 2026-07-28)

- **Geometría generada en código** desde las dimensiones. No hay un bloque por designación: 983 bloques
  dibujados a mano contra un DWG que no se versiona habrían perdido justo lo que hace útil un perfil
  normalizado.
- **Dos niveles de detalle**: `Simplified` (esquinas vivas) y `Tabulated` (todo lo que la fuente permite
  derivar).
- **Fidelidad declarada** y viajando con el resultado: 289 `TabulatedComplete`, 694 `TabulatedDerived`,
  **cero** degradadas. Una degradación nunca es silenciosa.
- **Instancia prismática**: **la longitud está fuera de la definición de sección**, junto con la
  rotación y el espejo. Así el catálogo no crece una fila por medida.
- **Vistas**: sección, longitudinal X, longitudinal Y, isométrica y **personalizada** por marco de
  cámara.
- **Rotación y espejo**, que no alteran ni el área ni el peso.
- **Plan neutral único**: `StructuralSectionRepresentationPlan`, con curvas ya proyectadas, roles
  semánticos, límites, fidelidad, diagnósticos y firma determinista.
- **El preview y AutoCAD consumen el mismo plan.** No hay dos generadores geométricos, y hay guardas de
  código que lo comprueban.
- **HSS con hueco y generatrices interiores**: un tubo visto de lado muestra sus cuatro caras, y la
  separación entre exterior e interior es exactamente `tnom`.
- **Perfiles colapsados canonicalizados**: una proyección que se ve de canto deja de emitirse como
  polilínea cerrada de área cero.
- **Comando `RACKSECCION`** y materialización en **bloque interno del dibujo**.
- **Sin dependencia de `blocks-library.dwg`** ni filas nuevas en `blocks.csv`.

### 3.c Lo que I-36C añade

Un botón. Y una autoridad compartida para que ese botón y `RACKSECCION` sean la misma cosa.

---

## 4. Diseño de la acción del menú

### La acción NO es una inserción de rack

El menú ya lleva **un** `RackInsertionRequest` tipado para los seis sistemas que sabe diseñar (I-15).
Meter ahí una sección habría sido **incorrecto**, no sólo inelegante:

| Un rack tiene | Una sección independiente |
|---|---|
| `RackSystemKind` sobre el que despachar | **no tiene** |
| Payload de diseño que embeber en el DWG | **no tiene** |
| Round-trip (`RACKEDITAR` la reabre) | **no tiene**: es geometría plana |

Un request cuyo `Kind` hubiera que inventar empuja esa mentira hasta el `switch` del host, que acaba con
una rama especial para un rack que no existe. Por eso el menú informa una **acción tipada**:
`MainMenuAction.GenerateStructuralSection`.

### El host la lee después de cerrar el modal

Misma regla que la inserción y por la misma razón: el flujo **pide un punto** y el editor de AutoCAD
tiene que estar libre. `RackMenuCommands` la despacha justo después de que `ShowModalWindow` retorne,
**antes** del `switch` de racks y con un `return` que impide caer en él.

### Ubicación y estilo

Entre «Diseñar larguero» y «Abrir de la biblioteca de diseños», con el **mismo** `MenuButton` que el
resto. Texto: **«Generar perfil estructural»**. Descripción: *«Consulta perfiles W, HSS, C y L del
catálogo AISC, visualiza su geometría e insértala en el dibujo.»*

## 5. Autoridad compartida con `RACKSECCION`

`StructuralSectionCommandFlow.Run(document)` contiene el caso de uso completo:

1. carga **fail-closed** del catálogo — un catálogo inválido detiene el flujo **antes** de abrir el
   inspector, porque dibujar una viga con dimensiones que no validaron es peor que no dibujarla;
2. inspector modal;
3. **cancelar sale sin tocar nada**;
4. aviso de unidades (ADR-0005), **después** de que el usuario confirme;
5. inserción transaccional: punto primero, definición y referencia en **una** transacción, regen por el
   helper canónico;
6. mensaje final con peso, fidelidad y diagnósticos.

Con él viven el resultado de inserción y el servicio que la ejecuta, de modo que el caso de uso completo
está en un archivo. `RackSeccionCommands` queda como lo que es: el punto de entrada del comando.

**Por qué importa.** Copiar el flujo en `RackMenuCommands` habría dado dos caminos que hoy coinciden y
que divergirían a la primera corrección: la carga fail-closed, el aviso de unidades, la transacción, el
regen y el mensaje de fidelidad habría que arreglarlos dos veces, y nada avisaría de que sólo se arregló
uno.

El **aviso de unidades no se duplica**: el del menú cubre las inserciones de rack, el del flujo cubre la
sección. Una guarda fija que aparece exactamente una vez y después del inspector.

## 6. Pruebas

| Suite | Nuevas | Qué fija |
|---|---|---|
| `RackMainMenuStructuralSectionTests` (UI) | **11** | El botón existe, está habilitado, texto y descripción exactos, posición entre larguero y biblioteca, mismo estilo, handler propio; al pulsarlo fija la acción tipada y **cierra**; **no** genera `InsertionRequest`; un menú recién abierto no pide nada; la acción **no** es un `RackInsertionRequest`; los siete botones anteriores conservan título, orden y handler; y `RackCad.UI` **no** referencia Autodesk |
| `MainMenuStructuralSectionAccessGuardTests` (Plugin, fuente) | **25** | Menú y comando corren el **mismo** flujo; **exactamente un** archivo del Plugin menciona cada pieza del caso de uso; el menú no duplica **nada** dentro de su rama; el comando es un punto de entrada delgado; catálogo inválido **falla cerrado antes** del inspector; cancelar **no** materializa; el aviso de unidades salta **una** vez y **después** de confirmar; la acción se despacha **fuera** del `switch` de racks, sin `RackSystemKind`; lo insertado sigue sin ser un rack; el flujo **no** construye geometría; y los cinco sistemas conservan su despacho |

**Siete guardas de I-36B reapuntadas.** Leían `RackSeccionCommands.cs`, donde ya no vive el flujo. Lo
que fijan —fallo cerrado, punto antes de crear, una sola transacción, regen canónico— **no cambió**:
cambió dónde vive, y las guardas lo dicen en su documentación.

### Regresión

- **`RACKSECCION` directo sigue funcionando**: la guarda comprueba que el `[CommandMethod]` sigue
  registrado y que llega al mismo flujo.
- **`RACKCAD` → «Generar perfil estructural» produce el mismo resultado**: por construcción, porque es
  literalmente la misma llamada; las guardas impiden que deje de serlo.
- **Selectivo, Dinámico, Push Back, Cabecera, Cama y Larguero intactos**: título, orden y handler fijados
  por prueba, y sus cinco ramas de despacho por guarda de fuente.

## 7. Builds, bundle y CI

| Gate | Resultado |
|---|---|
| `RackCad.Tests` | **2071 / 2071** (base 2043 → **+28**) |
| `RackCad.UI.Tests` | **534 / 534** (base 523 → **+11**) |
| `dotnet build src/RackCad.Application` Debug | 0 errores |
| `dotnet build src/RackCad.UI` Debug | 0 errores |
| `dotnet build src/RackCad.Plugin` Debug | 0 errores propios (2 `MSB3277` preexistentes) |
| `deploy/build-bundle.ps1` | **OK, 147 comprobaciones** |
| CI sobre el SHA publicado | ver §11 |

## 8. Checklist de AutoCAD para el dueño (8 puntos)

Con el DLL **Debug de este worktree**, según
[`../../guias/validacion-manual-autocad.md`](../../guias/validacion-manual-autocad.md).

| # | Qué hacer | Qué debe pasar |
|---|---|---|
| 1 | Ejecutar **`RACKCAD`** | Se abre el menú principal |
| 2 | Mirar la lista | Aparece **«Generar perfil estructural»**, habilitado, entre «Diseñar larguero» y «Abrir de la biblioteca de diseños», con el mismo aspecto de tarjeta que el resto |
| 3 | Pulsarlo | El menú se cierra y se abre el inspector de secciones |
| 4 | **Cancelar** y volver | No se inserta nada; el dibujo queda exactamente igual |
| 5 | Abrir de nuevo (`RACKCAD` → el botón) | El inspector vuelve a abrirse sin rastro del intento anterior |
| 6 | Seleccionar **`W12X26`** | El preview muestra el perfil en I |
| 7 | **Insertar** | Pide punto, inserta el bloque interno y escribe en la línea de comandos el mensaje con peso y fidelidad |
| 8 | Ejecutar **`RACKSECCION`** con la misma selección | El resultado **coincide** con el del paso 7: mismo dibujo, mismo tipo de bloque, mismo mensaje |

## 9. Pendientes registrados (no implementados)

Ninguno de estos **invalida lo ya implementado**. El catálogo y el generador funcionan y están
validados; lo que sigue son ampliaciones futuras.

| # | Pendiente |
|---|---|
| 1 | **Perfiles IPS/S** |
| 2 | **Verificación de correspondencia** de IPS con la familia AISC `S` o con el catálogo comercial de la empresa |
| 3 | **Geometría visual mejorada** para perfiles laminados |
| 4 | **Conicidad de patines** |
| 5 | **Radios, chaflanes y transiciones comerciales** cuando exista una **regla acreditada** |
| 6 | **Separación** entre geometría **tabulada** y geometría **visual aproximada** |
| 7 | **Cantilever I-37** |
| 8 | **Miembros estructurales** |
| 9 | **Materiales, conexiones y fabricación** |
| 10 | **Cálculo resistente y selección estructural** |
| 11 | **Sólidos 3D** |
| 12 | **Round-trip de perfiles independientes** (hoy lo insertado es geometría plana, no reeditable) |
| 13 | **Posible incorporación de familias adicionales** |

Detalle en [`../../ideas-futuras.md`](../../ideas-futuras.md). Los puntos 1 a 6 están registrados como
**requisito futuro obligatorio** por decisión del Owner al cerrar I-36B; el resto son alcance planificado
o diferimientos ya documentados.

## 10. Diff y guardas de alcance

| Guarda | Resultado verificado |
|---|---|
| `assets/**` (incl. los CSV de secciones y `blocks.csv`) | **cero líneas** |
| `blocks-library.dwg` | **no tocado** |
| `src/RackCad.Domain/` | **cero archivos** |
| Geometría de I-36B (`Application/**/Geometry`) | **cero archivos** |
| Sistemas vigentes de UI y Plugin | **cero cambios funcionales** |
| `deploy/`, `.github/` | **cero archivos** |
| `docs/ROADMAP.md`, `docs/HANDOFF.md` | **cero líneas** (WORKFLOW §8) |
| `main` | **intacta** |

**Confirmación expresa:** no se implementaron IPS/S, ni familias nuevas, ni cambios geométricos, ni la
mejora visual de los canales C, ni I-37, ni sólidos 3D, ni persistencia, ni round-trip. No se modificó
ningún sistema existente ni se rediseñó el menú.

## 11. Commits y CI

Se completa al publicar; ver `git log` de la rama.

## 12. Estado final

- **`review-ready`**, gate **`owner-validation`**.
- **No integrada, no limpiada**: rama y worktree se conservan. `main` intacta.
- Sin Pull Request: el repositorio integra por `git merge --no-ff` desde una sesión de integración.

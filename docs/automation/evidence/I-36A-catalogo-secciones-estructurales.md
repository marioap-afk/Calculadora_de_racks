# Evidencia — I-36A, núcleo y catálogo de secciones estructurales

Todo lo de este documento es **reproducible**: cada número sale de un comando o de una celda del libro
oficial, y cada afirmación se puede comprobar sin confiar en este archivo.

| Campo | Valor |
|---|---|
| Iniciativa | I-36A — Núcleo y catálogo de secciones estructurales |
| Rama | `architecture/catalogo-secciones-estructurales` |
| Claim-Id | `c72f4d42-a201-40c1-b9d9-2f03d9556681` |
| Worktree | `.claude/worktrees/architecture-catalogo-secciones-estructurales` |
| Base | `a35374f99dde91077a2e05bfc69358fb3e3b3ad9` (`Merge I-23: namespaces finales por sistema`) |
| CI de la base | run **30307095708**, `completed / success`, sobre el SHA exacto de `origin/main` |

---

## 0. Ronda 2 — rechazo parcial del gate `owner-validation`

El dueño rechazó parcialmente la primera ronda con **cinco defectos funcionales** y un bloque de
correcciones documentales. Lo que sigue describe el estado **posterior** a corregirlos; las secciones
1–16 conservan lo que no cambió, y donde cambió se anota aquí.

### 0.1 Preflight de reanudación

| Comprobación | Resultado |
|---|---|
| Rama | `architecture/catalogo-secciones-estructurales` |
| HEAD al reanudar | `49e68ad82a26464bdea8e6df221061b47cd60e20` — coincide con la punta del encargo |
| Upstream | `origin/architecture/…` en el **mismo SHA**: nada sin publicar |
| `git status` | limpio |
| Stashes | cero |
| Merge / rebase / cherry-pick / bisect | ninguno |
| Divergencia real vs `origin/main` | **0 detrás / 7 delante**; `merge-base = a35374f = origin/main` |
| ¿`origin/main` avanzó? | **No.** WORKFLOW §4.2 no exige rebase, y no se hizo |
| Worktrees | los dos esperados; el principal sigue en `a35374f` |

### 0.2 Los seis rojos, vistos fallar ANTES del fix

Compilan contra la API tal como estaba en `49e68ad` y fallan **por comportamiento**, no por
compilación. Mensajes literales de la corrida:

| Frente | Rojo observado |
|---|---|
| **F1** | `Tras un fallo de publicacion quedaron archivos REEMPLAZADOS: structural-sections-hss-rect.csv, structural-sections-w.csv` |
| **F2** | `Dos fuentes distintas produjeron el MISMO id 'AISC-W-W12X26': el id no admite otra autoridad.` |
| **F3** | `Un libro SIN hoja Readme se importo y se etiqueto como revision '16.0' de 'AISC Shapes Database v16.0'.` |
| **F4a** | `El manifiesto hashea 'structural-section-status.csv', que es un overlay EDITABLE: una edicion legitima invalidaria los datos AISC.` |
| **F4b** | `La reimportacion descarta 1 entrada(s) del overlay sin detenerse; hoy solo son un aviso por stderr.` |
| **F5** | `Load() devolvio un catalogo de 1 seccion(es) cuyo manifiesto miente: la ruta publica de carga no falla cerrada.` |

`Con error: 6, Superado: 0, Total: 6`. Tras corregir, las seis condiciones quedan cubiertas por las
suites definitivas (§0.9) y el archivo temporal de línea base se retiró.

### 0.3 F1 — diseño final de la publicación y su garantía exacta

```text
1. escribir los 6 archivos reproducibles en  .structural-sections-staging
2. por cada archivo, EN ORDEN (el manifiesto el ULTIMO):
     si existe en destino -> copiarlo a .structural-sections-backup   (ANTES de tocarlo)
     mover staging -> destino
3. sembrar el overlay vacio solo si falta
4. exito     -> borrar staging y backup
   excepcion -> ROLLBACK: restaurar los reemplazados desde backup, borrar los creados,
                borrar staging y backup, y RELANZAR la excepcion original
```

**La garantía, dicha con precisión:** ante una excepción durante la publicación, el directorio queda
exactamente como estaba —los archivos reemplazados vuelven byte por byte, los que no existían se
eliminan— y no quedan carpetas de trabajo.

**Lo que NO se afirma:** atomicidad frente a un corte de energía o a un proceso liquidado. Exigiría
escrituras con journal del sistema de archivos, no puede demostrarse con una prueba y por eso ni el
código ni la documentación lo prometen. Contra ese escenario protege otro mecanismo: el manifiesto se
publica **el último**, así que una publicación interrumpida deja datos nuevos junto a un manifiesto
viejo y la carga validada se niega a abrir esa carpeta — lo demuestra
`APartiallyPublishedStateIsRejectedByTheValidatedLoad`.

La costura es `internal static Action<string,int> AfterReplaceForTests`, no una abstracción pública de
sistema de archivos: lo único que una prueba necesita es fallar en un momento elegido, y una interfaz
para eso no la implementaría nadie más.

### 0.4 F2 — diseño final de la autoridad de IDs

```text
{ID_NAMESPACE}-{FAMILIA}-{DESIGNACION_NORMALIZADA}
StructuralSectionId.Create(idNamespace, family, designation)
```

- `ID_NAMESPACE` lo declara la fuente en `StructuralSectionSource.IdNamespace`, y viaja al CSV de
  fuentes (columna `idNamespace`) y al manifiesto.
- **`AISC-SHAPES` declara `AISC`, así que los 983 ids son EXACTAMENTE los mismos.** Verificado: los
  cuatro CSV de familia conservan su SHA-256 byte a byte (§0.7).
- Forma: `A-Z0-9`, no vacío. El guion queda excluido porque es el separador del propio id.
- **Único en el catálogo**: dos fuentes con el mismo namespace son error.
- `ExpectedSectionId(idNamespace)` es un **método con argumento**, no una propiedad: el validador
  resuelve la fuente y le pasa su autoridad, de modo que no puede ignorarla. Una sección cuya fuente no
  exista se reporta como no comprobable.
- Defecto adicional que la prueba nueva destapó y se corrigió: la unicidad de la designación EDI era
  **global**, así que dos fuentes sintéticas con la misma designación colisionaban aunque sus ids
  fueran distintos. Ahora es **por fuente**, y la búsqueda por EDI sin fuente se niega a elegir cuando
  hay ambigüedad.

### 0.5 F3 — evidencia de libro incorrecto rechazado

La ruta de producción exige, todo publicado por el propio documento: hoja `Readme` legible; que declare
«AISC Shapes Database v16.0»; que mencione «16th Edition» y la convención EDI; que exista la hoja
`Database v16.0`; y coherencia entre revisión detectada, hoja de datos y metadata generada. La revisión
sale de lo acreditado, no de una constante.

`--worksheet` **desaparece** de la CLI y del método público: no hay forma de etiquetar otra hoja o
revisión como v16.0, y una prueba por reflexión lo fija.

| Caso | Resultado |
|---|---|
| Readme ausente | **rechazado** — «El libro no tiene la hoja 'Readme'…» |
| Readme vacío | **rechazado** |
| Readme de la v15.0 | **rechazado** — falta el marcador `AISC SHAPES DATABASE V16.0` |
| Readme sin «16th Edition» | **rechazado** |
| Readme sin la convención EDI | **rechazado** |
| Hoja `Database v16.0` ausente | **rechazado** |
| Revisión y nombre de hoja contradictorios (`Database v15.0`) | **rechazado** |
| Libro correcto | **aceptado**, revisión `16.0`, hoja `Database v16.0` |

**El SHA-256 no es la compuerta**: dos libros sintéticos distintos —distinto hash— se aceptan los dos,
porque lo que se verifica es la identidad publicada. El hash se sigue registrando en el manifiesto como
procedencia.

### 0.6 F4 y F5 — política final de hashes, overlay y metadata

| Archivo | ¿En los hashes del manifiesto? | ¿En el bundle? | ¿Editable? |
|---|---|---|---|
| `structural-sections-{w,hss-rect,c,l}.csv` | **sí** | sí | no |
| `structural-section-sources.csv` | **sí** | sí | no |
| `structural-sections-manifest.json` | **no** (sería circular) | sí | no |
| `structural-section-status.csv` | **NO** (overlay mutable) | sí | **sí, el único** |

- Una edición legítima del overlay **no** invalida los datos AISC — lo demuestra
  `ALegitimateOverlayEditDoesNotInvalidateTheImportedData`.
- El overlay se valida aparte: esquema estricto, sin duplicados y con FK contra el catálogo.
- Una reimportación con una entrada huérfana **se detiene con error**; retirarla es del operador, y la
  forma de hacerlo es editar el overlay primero.
- El importador nunca reescribe un overlay existente; sólo lo siembra vacío cuando falta.
- `--check` informa **por separado** los datos reproducibles y la validez del overlay local.

**Metadata adicional validada por la carga pública** — `Load()` valida antes de cachear y no existe vía
pública para saltárselo: `catalogId`, `sourceId`, `sourceRevision`, `sourceWorksheet`, `mapperVersion`,
`idNamespace`, `sourceFileName`, SHA-256 del libro con **exactamente 64 hexadecimales**, conjunto
**exacto** de archivos declarados (ninguno faltante, inesperado ni repetido; nunca el manifiesto ni el
overlay), hash de cada archivo inmutable y correspondencia **fuente ↔ filas ↔ manifiesto**. Falla con
`StructuralSectionCatalogException` y el diagnóstico completo.

### 0.7 Conteos y hashes tras la ronda 2

Los datos AISC **no cambiaron**: `289 + 525 + 32 + 137 = 983`, cero filas seleccionadas rechazadas,
mismos tipos excluidos, mismo contraste métrico (peor caso `C5X6.7`, 4.128 %).

| Archivo | SHA-256 | ¿Cambió? |
|---|---|---|
| `structural-sections-w.csv` | `9259F672CDDC6855E321E0483F819F5875967145C6F218571F3D8E1FDCE78F1E` | **no** |
| `structural-sections-hss-rect.csv` | `FDC8E3E436DFA33421D0ED8A06F8CAC7B82C232F997802B6A95CC25498443F0D` | **no** |
| `structural-sections-c.csv` | `E42871A455AD2F78E9C9550E6B9D65431B678BB6CC867C841EC4E6BBEF66F63E` | **no** |
| `structural-sections-l.csv` | `6B5077003388735502FEBAC99281266B37C3688FAEEA2D2AB1BDB399E44BF2FC` | **no** |
| `structural-section-sources.csv` | `AD2AC2302FC92D5C956FF1FD2F94C2AC91338609166A3FA74ABFAEE03298B385` | **sí** — gana la columna `idNamespace` |
| `structural-sections-manifest.json` | `A6B40B470B311CE27F0E3BFD8D7672B749680361FBA94184E51B3949147616AD` | **sí** — gana `idNamespace` y deja de hashear el overlay |
| `structural-section-status.csv` | `7B4CB158AF88769BD90AB9CE2CE3D21010EDB7F7FB334E591917B6F5342E6D7D` | **no** — y ya no se hashea |

`MapperVersion` pasa de `I-36A.1` a `I-36A.2`. Dos importaciones independientes vuelven a producir los
siete archivos **byte-idénticos**, y `--check` sobre `assets/catalogs` da OK en las dos categorías.

### 0.8 Correcciones documentales (F6) y estado de ADR-0021 (F7)

| # | Corrección | Dónde |
|---|---|---|
| 1 | `W12X28` no es un ejemplo real de la v16.0: los ejemplos reales usan `W12X26` y `W12X28` queda **etiquetado como fixture sintético de formato** | ADR-0020, ADR-0021, contrato, guía, `initiatives/README`, ROADMAP, `StructuralSectionDefinition` |
| 2 | Los cuatro tramos de columnas descritos correctamente: `A`–`D` metadata/identidad US, `E`–`CF` valores US, `CG`–`CH` designaciones métricas, `CI`–`FJ` valores métricos | ADR-0021 §Contexto, guía §11 |
| 3 | El peor contraste real es **`C5X6.7`**, que es lo que vuelve a emitir el importador; se elimina `C6X6.7` | ADR-0021 |
| 4 | «cuatro sistemas vigentes» → «todos los sistemas vigentes» | ADR-0020, ARCHITECTURE, guía, contrato |
| 5 | No se afirma atomicidad frente a un crash: se documenta la garantía real y su límite | guía §7, `ImportOutputWriter` |
| 6 | Separación datos importados ↔ overlay documentada explícitamente | ADR-0020 §4.b, guía §3, ARCHITECTURE |

**F7:** ADR-0021 pasa a **`propuesto`**. Su decisión central —la política exacta de IDs— es uno de los
siete puntos del gate y el dueño la rechazó parcialmente; un `aceptado` mientras sigue bajo gate diría
lo contrario de lo que ocurre. **ADR-0020 permanece `aceptado`**: la separación sección / miembro fue
decidida expresamente.

### 0.9 Suites, builds y bundle de la ronda 2

| Gate | Resultado |
|---|---|
| `RackCad.Tests` | **1837 / 1837** (ronda 1: 1762 → **+75**) |
| `RackCad.UI.Tests` | **494 / 494** |
| Build herramienta Debug | 0 errores, 0 advertencias |
| Build `RackCad.Application` Debug | 0 errores, 0 advertencias |
| Build `RackCad.UI` Debug | 0 errores, 0 advertencias |
| Build `RackCad.Plugin` Debug | 0 errores (2 `MSB3277` conocidos) |
| Bundle build + verify | **OK, 147 comprobaciones** |
| Reimportación completa del libro oficial | 289/525/32/137 = **983**, cero rechazadas |
| CSV AISC reproducibles | dos corridas byte-idénticas |
| Overlay preservado | intacto byte a byte tras reimportar |

Suites nuevas: `StructuralSectionPublishTransactionTests` (F1), `StructuralSectionIdNamespaceTests`
(F2), `AiscWorkbookVerificationTests` (F3), `StructuralSectionLoadValidationTests` (F4/F5), más
ampliaciones de `StructuralSectionValidatorTests` y `StructuralSectionImporterTests`.

Las clases que publican comparten `StructuralSectionPublishCollection` sin paralelismo: el hook de
fallo es estático y xUnit ejecuta clases en paralelo, así que sin eso una prueba armaba el fallo que
otra sufría.

### 0.10 Lo que NO cambió en la ronda 2

`secciones.csv` y los diez catálogos vigentes byte-idénticos; `blocks.csv` y `blocks-library.dwg`
intactos; `src/RackCad.Domain`, `src/RackCad.UI`, `src/RackCad.Plugin`, `deploy/` y `.github/` con
**cero archivos cambiados**; `docs/HANDOFF.md` sin tocar; la columna Estado del ROADMAP sin marcar nada
como integrada; I-36B sin abrir y su rama sin crear; y **`main` intacta**.

---

## 0-bis. Micro-ronda 3 — los dos residuos funcionales y las tres correcciones documentales

La ronda 2 quedó **aprobada en sus cuatro correcciones principales**. Quedaban dos residuos
funcionales y tres correcciones documentales; esta micro-ronda los cierra. **Sin cambios en los cuatro
CSV de familias ni en sus ids.**

### 0-bis.1 Preflight de reanudación

| Comprobación | Resultado |
|---|---|
| HEAD al reanudar | `f8122a8dd69d73cb70f85d23c4a5893d405bb262` — la punta revisada |
| Upstream | `origin/architecture/…` en el **mismo SHA**: nada sin publicar |
| `git status` | limpio |
| Stashes | cero |
| Merge / rebase / cherry-pick / bisect | ninguno |
| Divergencia real vs `origin/main` | **0 detrás / 10 delante**; `merge-base = a35374f = origin/main` |
| ¿`origin/main` avanzó? | **No** → sin rebase |
| Worktrees | los dos esperados; el principal sigue en `a35374f` |

### 0-bis.2 Rojos observados

Nueve, todos por comportamiento contra la API de `f8122a8`:

| # | Caso | Rojo |
|---|---|---|
| 1–4 | `sourceWorksheet` incorrecto pero no vacío: `Database v15.0`, `Datos`, `database v16.0`, `Database v16.0 ` | `Assert.NotEmpty() Failure: Collection was empty` |
| 5–7 | `mapperVersion` incorrecto pero no vacío: `I-36A.1`, `I-36A.3`, `otra-cosa` | `Assert.NotEmpty() Failure: Collection was empty` |
| 8 | Fuente adicional **sin** secciones | `Assert.NotEmpty() Failure: Collection was empty` |
| 9 | Rollback con una restauración imposible | `Expected: typeof(System.InvalidOperationException)` / `Actual: typeof(System.IO.IOException)` |

El rojo 9 es exactamente el defecto: la excepción del rollback **sustituía** a la que causó el fallo.

Dos de los casos exigidos **ya estaban cubiertos** antes del fix y se dejan fijados explícitamente:
**fuente adicional CON secciones** (el bucle de secciones ya lo detectaba) y **catálogo correcto**.

### 0-bis.3 Diseño final de la validación de metadata

`StructuralSectionsManifest` esquema **1.0** exige ahora, además de lo que ya exigía:

| Regla | Cómo |
|---|---|
| `sourceWorksheet` compatible con fuente y revisión | `StructuralSectionSource.TryExpectedWorksheet(sourceId, revision)` — para `AISC-SHAPES` + `16.0` ⇒ **`Database v16.0`**. Comparación ordinal, exacta |
| `mapperVersion` = la que este build soporta | `StructuralSectionsManifest.SupportedMapperVersion` (`I-36A.2`), comparación ordinal |
| Constante de mapeo **compartida** | Vive en **Application** y la consumen el lector y el importador. `AiscRowMapper.MapperVersion` **desaparece**: eran dos copias del mismo número |
| Regla del worksheet **compartida** | `AiscWorkbookVerifier.DataWorksheetFor` **delega** en la autoridad de Application en vez de concatenar por su cuenta |
| **Exactamente una** fuente en el catálogo v1 | `catalog.Sources.Count != 1` ⇒ error, con los ids que sí hay |
| Esa fuente coincide con el manifiesto | `sourceId`, `sourceRevision` e `idNamespace`, uno a uno |
| Todas las secciones pertenecen a ella | Recorrido sobre `catalog.All` |
| Una fuente adicional **no utilizada** | Error igualmente |

**Precisión que evita una lectura errónea de F2:** el **modelo** es deliberadamente multi-fuente —para
eso existe el namespace de id— pero el **formato de distribución 1.0** no lo es. Un catálogo con dos
fuentes es un catálogo que este manifiesto no sabe describir: sus conteos y sus hashes no dirían nada
de la mitad de lo que hay. Por eso la restricción vive en el **validador del manifiesto** y no en
`StructuralSectionCatalog`, que sigue admitiendo N autoridades.

### 0-bis.4 Garantía exacta del rollback

```text
try:
    escribir staging
    sembrar overlay si falta      -> se REGISTRA como creado ANTES de escribirlo
    por cada archivo: respaldar si existe -> mover -> hook
catch (fallo de publicacion):
    failures  = Rollback(...)          # intenta TODO, nunca lanza
    failures += RemoveWorkingFolders() # idem
    si failures: originalException.Data[RollbackFailuresKey] = failures
    throw;                             # la ORIGINAL, no la del cleanup
```

**Lo que se garantiza:** el rollback **intenta** cada restauración y cada eliminación, no se detiene en
la primera que falla y **nunca sustituye** la excepción que lo provocó. Cuando todos los intentos
salen bien, el directorio queda exactamente como estaba.

**Lo que NO se garantiza, y ahora se dice:**

- **La restauración se intenta, no se asegura.** El sistema de archivos puede negarse —archivo
  bloqueado por otro proceso, atributo de solo lectura, disco lleno—. En ese caso el directorio **no**
  vuelve a su estado anterior, y los fallos quedan adjuntos a la excepción original bajo
  `ImportOutputWriter.RollbackFailuresKey`, legibles con `RollbackFailuresOf`.
- **Atomicidad frente a un corte de energía o a un proceso liquidado.** No es demostrable con una
  prueba.

En ambos casos lo que protege al consumidor no es una promesa sino un mecanismo: el **manifiesto se
publica el último**, así que un conjunto parcial o parcialmente restaurado ya no cuadra con sus hashes
y `Load()` se niega a abrirlo.

**El overlay** se siembra ahora **antes** de los reemplazos y se registra como creado, de modo que un
fallo posterior lo retira. Sembrarlo al final dejaba un archivo que el rollback no conocía y volvía
falsa la frase «el directorio queda exactamente como estaba».

La regresión lo demuestra entero: deja reemplazar `sources` y `c`, **bloquea `c`** —ya reemplazado—,
falla en `hss-rect`, y comprueba que (1) sobrevive la excepción original, (2) el fallo de rollback se
informa y nombra el archivo, (3) `sources` y `hss-rect` **sí** se restauraron, (4) el bloqueado no, y
(5) el catálogo resultante **no se puede cargar**.

### 0-bis.5 Correcciones documentales

| # | Corrección | Dónde |
|---|---|---|
| 1 | `C6X6.7` → **`C5X6.7`** | Comentario **activo** de `AiscShapesImporter.WeightTolerance` |
| 2 | La designación EDI es única **por fuente**, no global, y por qué | XML-doc de `StructuralSectionCatalog` |
| 3 | Un namespace identifica **exactamente una** fuente; dos no lo comparten bajo el esquema actual | XML-doc de `StructuralSectionSource.IdNamespace` |

**ADR-0021 sigue en `propuesto`.** Su aceptación expresa se registrará después de esta micro-ronda.

### 0-bis.6 Gates de la micro-ronda 3

| Gate | Resultado |
|---|---|
| Rojos dirigidos, vistos fallar antes del fix | **9** |
| `RackCad.Tests` | **1851 / 1851** (ronda 2: 1837 → **+14**) |
| `RackCad.UI.Tests` | **494 / 494** |
| Build herramienta Debug | 0 errores, 0 advertencias |
| Build `RackCad.Application` Debug | 0 errores, 0 advertencias |
| Build `RackCad.UI` Debug | 0 errores, 0 advertencias |
| Build `RackCad.Plugin` Debug | 0 errores (2 `MSB3277` conocidos) |
| Bundle build + verify | **OK, 147 comprobaciones** |
| `--check` contra el libro oficial | OK en las **dos** categorías |
| Reimportación completa | W **289**, HSS-RECT **525**, C **32**, L **137**, total **983**, rechazadas **0** |
| Hashes de los cuatro CSV de familia | **idénticos**; `git diff` sobre `assets/` vacío tras reimportar |

### 0-bis.7 Lo que no cambió

`secciones.csv` y los diez catálogos vigentes byte-idénticos; `blocks.csv` y `blocks-library.dwg`
intactos; `src/RackCad.Domain`, `src/RackCad.UI`, `src/RackCad.Plugin`, `deploy/` y `.github/` con
**cero archivos cambiados**; `docs/HANDOFF.md` sin tocar; la columna Estado del ROADMAP sin marcar nada
como integrada; **I-36B sin abrir y su rama sin crear**; y **`main` intacta en `a35374f`**.

---

## 1. Preflight real (ronda 1)

Ejecutado **antes** de escribir nada y antes del reclamo.

| Comprobación | Resultado |
|---|---|
| `git fetch origin --prune` | OK |
| `main` == `origin/main` | sí, `a35374f99dde91077a2e05bfc69358fb3e3b3ad9` en ambos |
| `git status` | limpio |
| Merge / rebase / cherry-pick / bisect en curso | ninguno |
| Worktrees | uno solo, el principal del dueño |
| Stashes | cero |
| Ramas remotas | **solo** `origin/main` |
| `architecture/catalogo-secciones-estructurales` | libre |
| `architecture/geometria-secciones-estructurales` | libre y **reservada** para I-36B (no se creó) |
| Otra iniciativa sobre `Catalogs`, `assets/catalogs`, `tests`, `.sln` o los índices documentales | ninguna |

Documentos leídos: `AGENTS.md`, `docs/WORKFLOW.md`, `docs/ROADMAP.md`, `docs/ARCHITECTURE.md`,
`docs/HANDOFF.md`, `docs/adr/README.md`, ADR-0005, ADR-0007, ADR-0008, ADR-0012,
`docs/guias/catalogos-y-plantillas.md`, `docs/guias/modelo-de-datos.md`,
`docs/initiatives/TEMPLATE.md`, `docs/initiatives/README.md`, los Context Packs
`architecture-kernel`, `catalogs-data`, `delivery-validation` y `documentation-governance`, los
contratos de I-19, I-26 e I-23, `src/RackCad.Application/Catalogs/` completo (incluido `Validation/`),
las pruebas de catálogo de `tests/RackCad.Tests` y las reglas de copiado/bundle de `assets/catalogs`
(los cuatro `.csproj` y `deploy/verify-bundle.ps1`).

`OWNER-DECISIONS.md` **no existe** en el repositorio; las decisiones del dueño de esta iniciativa
están versionadas en [`../decisions/I-36A.md`](../decisions/I-36A.md).

---

## 2. La fuente y su procedencia

| Campo | Valor |
|---|---|
| Nombre | AISC Shapes Database v16.0 |
| Publicador | American Institute of Steel Construction |
| Página oficial (la del encargo) | <https://www.aisc.org/aisc/publications/steel-construction-manual/aisc-shapes-database-v160/> — responde **200** |
| URL del encargo | `https://www.aisc.org/globalassets/product-files-not-searched/manuals/aisc-shapes-database-v16.0.xlsx` — **404** con `curl`, **403** con `Invoke-WebRequest` |
| URL utilizada | `https://cloud.aisc.org/biggie_bin/aisc-shapes-database-v160-2.xlsx` |
| Por qué esa | Es el destino del botón **«DOWNLOAD SHAPES DATABASE V16.0»** que publica la propia página oficial. Dominio del propio AISC: **mismo publicador**, no un mirror ni un tercero |
| Descarga | `HTTP 200`, `content-type application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` |
| Tamaño | **2 028 540** bytes |
| **SHA-256** | **`82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496`** |
| Ubicación | scratchpad de la sesión, **fuera** del repositorio. El libro **no se versiona** |
| No utilizado | `https://cloud.aisc.org/biggie_bin/aisc-shapes-database-v160h.xlsx` — es la **Historic Shapes Database v16.0H**, que la misma página publica aparte. La base suplementaria **ASTM A1085** tampoco se usó |

### Verificación del contenido, no solo del nombre

| Comprobación | Resultado |
|---|---|
| Partes OOXML | `xl/workbook.xml`, `xl/_rels/workbook.xml.rels`, `xl/sharedStrings.xml` (211 KB), `xl/worksheets/sheet1.xml` (55 KB) y `xl/worksheets/sheet2.xml` (**14.7 MB**) |
| Hojas declaradas | `Readme` y `Database v16.0` — **dos**, las que el encargo describe |
| Versión declarada | La hoja `Readme` dice literalmente «AISC Shapes Database v16.0», fecha **agosto 2023**, y «consistent with … AISC Steel Construction Manual, 16th Edition, 1st Printing» |
| Readme legible | Sí: se leyó completo, incluido el glosario de las 84 variables y la lista de formas nuevas de la v16.0 |
| Convención EDI | Documentada en el propio Readme: la columna B es «the shape designation according to the AISC Naming Convention for Structural Steel Products for Use in Electronic Data Interchange (EDI), June 25, 2001» |
| Fila de encabezados | La **1**, con **166** columnas |
| Bloque estadounidense | `A`–`CF` (índices 0–83) |
| Espejo métrico | `CG`–`FJ` (índices 84–165), empezando por su propio `EDI_Std_Nomenclature` |
| Marcador de «no aplica» | **EN DASH** `–` (U+2013), 250 455 apariciones; no es celda vacía |

---

## 3. Reclamo

| Campo | Valor |
|---|---|
| Rama creada desde | `origin/main` (`a35374f`) |
| Commit de reclamo | `c9d53d209f71ff2cf019e8cc121dbe0096f84a2b` |
| Primer push | **aceptado**, sin `--force` — el reclamo es válido |
| Trailers | `Initiative-Id: I-36A`, `Claim-Id: c72f4d42-…`, `Co-Authored-By: Claude Opus 5` |

---

## 4. ADRs

| ADR | Título | Estado | Efecto |
|---|---|---|---|
| **0020** | Catálogo neutral de secciones estructurales | aceptado | **Reemplaza a ADR-0008** en **autoridad conceptual**, no en comportamiento |
| **0021** | Identidad, unidades y presentación de secciones estructurales | aceptado | Nuevo. **No** reemplaza a ADR-0005 |
| 0008 | Perfiles estructurales unificados en secciones.csv por rol | **reemplazado por ADR-0020** | Sección *Decisión* **intacta**; solo estado, enlaces y una nota posterior fechada |
| 0005 | Estrategia de unidades | **aceptado, sin cambio** | Solo una nota posterior con el enlace: la pulgada sigue siendo la unidad interna y no se implementa conversión del DWG |

El índice `docs/adr/README.md` refleja los dos estados y explica el alcance exacto del reemplazo.

---

## 5. Esquema final

Namespace **`RackCad.Application.StructuralSections`**, en
`src/RackCad.Application/StructuralSections/` (cumple la regla namespace = carpeta de I-23).

**Modelo**: `StructuralSectionFamily` (+ tokens), `StructuralSectionId`,
`StructuralSectionDesignationNormalizer`, `StructuralSectionIdentity`, `StructuralSectionSource`
(+ `StructuralSectionUnitSystem`), `StructuralSectionProperties`, `IStructuralSectionDimensions` con
`WSectionDimensions` / `HssRectangularSectionDimensions` / `ChannelSectionDimensions` /
`AngleSectionDimensions`, `StructuralSectionDefinition`, `StructuralSectionStatusOverride`.

**Datos**: `StructuralSectionCsvSchema` (única autoridad de nombres y orden de columnas),
`StructuralSectionCsvSerializer`, `StructuralSectionCsvWriter`, `StrictCsvTable`,
`StructuralSectionCsvException`, `StructuralSectionsManifest`.

**Servicios**: `StructuralSectionCatalog`, `IStructuralSectionCatalogProvider`,
`CsvStructuralSectionCatalogProvider`, `StructuralSectionCatalogValidator`, `StructuralSectionUnits`,
`StructuralSectionLabelFormatter`.

**Compartido**: `RackCad.Application.Catalogs.CsvLexer` — el parser léxico RFC-4180 extraído
**verbatim** de `CsvCatalogReader`, que ahora delega. Comportamiento del lector tolerante **sin
cambio**, fijado por `CsvLexerTests`.

Columnas por archivo: identidad (8) + propiedades comunes (10) + específicas de familia —W 27,
HSS-RECT 7, C 31, L 33—. La matriz completa de las 84 columnas de la fuente, con su significado según
el Readme, unidades US y métricas, familias aplicables, campo de RackCad y razón de omisión cuando
aplica, vive en [`docs/guias/secciones-estructurales.md`](../../guias/secciones-estructurales.md) §11.

**Omitidas deliberadamente**: las seis razones de esbeltez (`bf/2tf`, `b/t`, `b/tdes`, `h/tw`,
`h/tdes`, `D/t`) por ser cocientes derivables de valores ya presentes y pertenecer al dominio de diseño
que ADR-0017 difiere; y `OD` e `ID`, que ninguna de las cuatro familias importadas tabula (`OD` sí se
**lee** para clasificar). El resto de columnas del bloque estadounidense se importa.

---

## 6. Política final de IDs

```
AISC-{FAMILIA}-{EDI_NORMALIZADO}     FAMILIA ∈ { W, HSS-RECT, C, L }
```

Normalización: mayúsculas invariantes, sin espacios, `.` `/` `-` → `_`. Resultado ASCII, mayúsculas,
sin espacios, sin barra, sin punto decimal. Sin revisión, sin grado de material, sin magnitudes.
`DisplayName` nunca es clave. Ninguna dimensión ni peso se deriva del texto de una designación.

| Etiqueta del manual | EDI | Id |
|---|---|---|
| `W12X26` | `W12X26` | `AISC-W-W12X26` |
| `C10X15.3` | `C10X15.3` | `AISC-C-C10X15_3` |
| `L4X4X1/4` | `L4X4X1/4` | `AISC-L-L4X4X1_4` |
| `HSS4X4X1/4` | `HSS4X4X.250` | `AISC-HSS-RECT-HSS4X4X_250` |

**Colisiones: 0.** Las 983 designaciones EDI producen 983 ids distintos, todos dentro de `[A-Z0-9_-]`.
También son 983 las designaciones EDI normalizadas distintas y 983 las etiquetas de manual distintas.

### Discrepancia con el ejemplo del encargo, resuelta y sujeta a validación

El encargo pide una política «basada en la designación EDI oficial» y da como ejemplo
`AISC-HSS-RECT-HSS4X4X1_4`. En el libro real el EDI de esa sección es **`HSS4X4X.250`** y
`HSS4X4X1/4` es su **AISC Manual Label**: para las 525 HSS rectangulares el EDI escribe el espesor en
decimal y el manual en fracción, y **513 de las 983 filas difieren** entre ambas designaciones. Los
otros tres ejemplos del encargo (`AISC-W-W12X28`, `AISC-C-C10X15_3`, `AISC-L-L4X4X1_4`) **sí**
coinciden con el EDI.

Se aplicó la **regla**, no el ejemplo: EDI en las cuatro familias sin excepción, porque una regla que
cambia de fuente según la familia deja de ser determinista. El id real es
**`AISC-HSS-RECT-HSS4X4X_250`**; el Manual Label se conserva íntegro, es único y es con el que se busca
y se muestra. Registrado en **ADR-0021 §6** y es el **punto 5** de la validación del dueño.

### Segundo hallazgo: `W12X28` no existe

La serie W12 de la v16.0 va `W12X14, 16, 19, 22, 26, 30, 35, 40, 45, 50, 53, 58, 65, 72, 79, 87, 96,
106, 120, 136, 152, 170, 190, 210, 230, 252, 279, 305, 336` — **no hay `W12X28`**. El ejemplo del dueño
fija el **formato** de la etiqueta de peso, no una sección real. Por eso la prueba que reproduce
literalmente `W12X28 — 28 lb/ft (41.7 kg/m)` usa una sección **sintética**, y las pruebas del catálogo
distribuido usan `W12X26 — 26 lb/ft (38.7 kg/m)`.

---

## 7. Conteos exactos

Salida literal del importador sobre el libro oficial.

| Concepto | Valor |
|---|---|
| Filas de datos de la hoja | **2 299** (más 2 filas finales vacías, ignoradas) |
| Filas **seleccionadas** | **983** |
| Secciones importadas | **983** |
| **Filas seleccionadas rechazadas** | **0** |

| Familia | Secciones |
|---|---|
| W | **289** |
| HSS rectangular y cuadrado | **525** (de ellas **126** cuadradas) |
| C | **32** |
| L | **137** |
| **Total** | **983** |

| Tipo excluido | Filas |
|---|---|
| `2L` | 639 |
| `WT` | 289 |
| `HSS-ROUND` | 189 |
| `PIPE` | 51 |
| `MC` | 40 |
| `S` | 28 |
| `ST` | 28 |
| `HP` | 22 |
| `M` | 16 |
| `MT` | 14 |
| **Total** | **1 316** |

`983 + 1 316 = 2 299`. La partición cierra exactamente y no queda ninguna fila sin clasificar.

**Criterio del HSS, por campos oficiales y no por expresión regular**: con `OD` y sin paredes →
redondo, excluido (189); con `Ht`/`B`/`h`/`b` y sin `OD` → importado (525). `525 + 189 = 714`, el total
de filas `Type = HSS`, sin solape ni sobrantes; ninguna fila resultó ambigua.

---

## 8. Contraste con el bloque métrico oficial

El bloque métrico **no genera filas**: se usa como testigo independiente de que cada magnitud se leyó
de la columna que su encabezado dice. Una desviación fuera de tolerancia detiene la importación.

| Magnitud | Comparadas | Desviación máxima | Tolerancia | Peor caso |
|---|---|---|---|---|
| `W` (peso) | 983 | **4.128 %** | 5 % | `C5X6.7` |
| `A` | 983 | 0.461 % | 1 % | `W24X55` |
| `d` | 458 | 0.435 % | 1 % | `W36X925` |
| `bf` | 321 | 0.412 % | 1 % | `W10X12` |
| `tw` | 321 | 0.402 % | 1 % | `C7X14.75` |
| `tf` | 321 | 0.445 % | 1 % | `W10X33` |
| `Ht` | 525 | 0.392 % | 1 % | `HSS4X4X1/2` |
| `B` | 525 | 0.392 % | 1 % | `HSS20X4X1/2` |
| `tnom` | 525 | 0.366 % | 1 % | `HSS16X6X3/16` |
| `tdes` | 525 | 0.288 % | 1 % | `HSS34X10X5/8` |
| `t` | 137 | 0.357 % | 1 % | `L8X8X1-1/8` |
| `b` | 662 | 0.392 % | 1 % | `L4X4X3/4` |

La tolerancia del peso es amplia **por una razón concreta y no por imprecisión**: la columna métrica de
peso es un **valor nominal de designación redondeado por separado**, no una conversión. `C5X6.7` se
publica como `10.4 kg/m` cuando la conversión exacta da `9.97`. La geometría, que sí es conversión con
redondeo a tres cifras significativas, se queda por debajo del **0.47 %**.

---

## 9. Archivos generados y sus hashes

SHA-256 tal como los declara `structural-sections-manifest.json`. Verificados **tres veces**: en el
archivo del worktree, en el objeto que Git almacena (`git show HEAD:…`) y dentro del bundle canónico.

| Archivo | Bytes | SHA-256 |
|---|---|---|
| `structural-sections-w.csv` | 71 118 | `9259F672CDDC6855E321E0483F819F5875967145C6F218571F3D8E1FDCE78F1E` |
| `structural-sections-hss-rect.csv` | 93 588 | `FDC8E3E436DFA33421D0ED8A06F8CAC7B82C232F997802B6A95CC25498443F0D` |
| `structural-sections-c.csv` | 9 052 | `E42871A455AD2F78E9C9550E6B9D65431B678BB6CC867C841EC4E6BBEF66F63E` |
| `structural-sections-l.csv` | 38 455 | `6B5077003388735502FEBAC99281266B37C3688FAEEA2D2AB1BDB399E44BF2FC` |
| `structural-section-sources.csv` | 284 | `96FB1590D0FB2E904F4ECEC0D8560A6B7F6080CCE7F6E8302FFFA3A83B5FA694` |
| `structural-section-status.csv` | 26 | `7B4CB158AF88769BD90AB9CE2CE3D21010EDB7F7FB334E591917B6F5342E6D7D` |
| `structural-sections-manifest.json` | 1 519 | *(no se hashea a sí mismo: sería circular)* |

**Reproducibilidad demostrada, no afirmada.** Dos importaciones independientes sobre el mismo libro, a
directorios distintos, produjeron los **siete archivos byte-idénticos**. El manifiesto no lleva ninguna
marca de tiempo.

`.gitattributes` marca los siete como `-text`, con alcance limitado a ellos. El repositorio usa
`core.autocrlf=true`, así que sin esa regla el checkout convertiría los saltos de línea y el hash del
archivo **en disco** dejaría de coincidir con el del manifiesto en un clon nuevo —incluido el runner
Linux de CI—. Comprobado: los hashes del objeto almacenado en Git coinciden exactamente con los seis
de la tabla.

---

## 10. Sentinelas

Dos por familia, leídas **a mano** de la hoja `Database v16.0` del libro cuyo SHA-256 está en §2. Cada
una está fijada en `ShippedStructuralSectionCatalogTests` con su número de fila.

### W — fila 2, `W44X408`

`T_F=T` · `W=408` · `A=120` · `d=44.8` · `ddet=44.75` · `bf=16.1` · `bfdet=16.125` · `tw=1.22` ·
`twdet=1.25` · `tf=2.17` · `tfdet=2.1875` · `kdes=2.96` · `kdet=3.375` · `k1=1.8125` · `Ix=38700` ·
`Zx=2000` · `Sx=1730` · `rx=18` · `Iy=1520` · `Cw=691000` · `T=38` · `WGi=5.5` · `WGo=3`.
Métrico de contraste: `W=607`, `A=77400`, `d=1140`. `T_F=T` porque `tf = 2.17 in > 2 in`.

### W — fila 245, `W12X26`

`T_F=F` · `W=26` · `A=7.65` · `d=12.2` · `bf=6.49` · `tw=0.23` · `tf=0.38` · `kdes=0.68` ·
`kdet=1.0625` · `Ix=204` · `Iy=17.3` · `Cw=607` · `T=10.125`. Métrico: `W=38.7`, `A=4940`, `d=310`.

### HSS-RECT — fila 1536, `HSS34X10X1`

`W=277.07` · `A=76.2` · `Ht=34` · `h=31.2` · `B=10` · `b=7.21` · `tnom=1` · `tdes=0.93` · `Ix=9600` ·
`Zx=750` · `J=4040` · `C=555`. Métrico: `W=412`, `A=49200`. No es cuadrado.

### HSS-RECT — fila 1983, `HSS4X4X1/4` (EDI `HSS4X4X.250`)

`W=12.21` · `A=3.37` · `Ht=4` · `h=3.3` · `B=4` · `b=3.3` · `tnom=0.25` · `tdes=0.233` · `Ix=7.8` ·
`J=12.8` · `C=6.56`. Métrico: `W=18.1`, `A=2170`. **Es cuadrado** y se cuenta dentro de `HSS-RECT`.

### C — fila 357, `C15X50`

`W=50` · `A=14.7` · `d=15` · `bf=3.72` · `tw=0.716` · `tf=0.65` · `kdes=1.44` · `x=0.799` ·
`eo=0.583` · `xp=0.49` · `Ix=404` · `Cw=492` · `ro=5.49` · `H=0.937`. Métrico: `W=74`, `A=9480`,
`d=381`.

### C — fila 366, `C10X15.3`

`W=15.3` · `A=4.48` · `d=10` · `bf=2.6` · `tw=0.24` · `tf=0.436` · `x=0.634` · `eo=0.796` ·
`Ix=67.3` · `H=0.884`. Métrico: `W=22.8`, `A=2890`, `d=254`.

### L — fila 429, `L12X12X1-3/8`

`W=105` · `A=31.1` · `d=12` · `b=12` · `t=1.38` · `kdes=2.09` · `x=3.5` · `y=3.5` · `Ix=413` ·
`Iz=165` · `rz=2.3` · `H=0.627` · `tan(α)=1` · `Iw=661`. Métrico: `W=156`, `A=20100`, `d=305`.
Alas iguales.

### L — fila 509, `L4X4X1/4`

`W=6.6` · `A=1.93` · `d=4` · `b=4` · `t=0.25` · `kdes=0.625` · `x=1.08` · `y=1.08` · `Ix=3` ·
`Iz=1.19` · `J=4.38e-2` · `Cw=5.05e-2` · `zB=0`. Métrico: `W=9.8`, `A=1250`, `d=102`.

### Sentinela extra de nomenclatura — `L8X6X1`

AISC etiqueta el ángulo por su ala **larga** primero, mientras que las columnas son `d` = ala **corta**
y `b` = ala **larga**: `d=6`, `b=8`. Se fija con una prueba propia porque invertirlo espejearía en
silencio cada ángulo desigual que dibuje I-36B.

### `zB = 0` es un valor real

`zB` vale exactamente **0** en los **61** ángulos de alas iguales, porque el punto B está sobre el eje
z. Es el único cero legítimo de todo el cuerpo de datos (barrido sobre las 983 filas: ninguna otra
columna contiene un cero). La regla «un cero significa un valor perdido» se restringió por eso a las
**magnitudes**, dejando fuera las **posiciones**; hay regresión para ambos casos.

---

## 11. Pruebas y builds

| Suite / gate | Resultado |
|---|---|
| `RackCad.Tests` | **1762 / 1762**, 0 fallos, 0 omitidas (base `origin/main`: 1619 → **+143**) |
| `RackCad.UI.Tests` | **494 / 494**, 0 fallos, 0 omitidas (sin cambio: I-36A no toca UI) |
| Build `tools/RackCad.StructuralSections.Import` Debug | **0 errores, 0 advertencias** |
| Build `src/RackCad.Application` Debug | **0 errores, 0 advertencias** |
| Build `src/RackCad.UI` Debug | **0 errores, 0 advertencias** |
| Build `src/RackCad.Plugin` Debug | **0 errores** (2 `MSB3277` conocidos de las referencias AutoCAD) |
| `deploy/build-bundle.ps1` + `verify-bundle.ps1` | **OK, 147 comprobaciones**; DLL idénticos al publish, catálogos idénticos a `assets/catalogs`, cero DLL de Autodesk |
| Los 7 archivos nuevos dentro del bundle | **sí**, con hashes idénticos a los del manifiesto |

Reparto de las 143 pruebas nuevas:

- **`CsvLexerTests` (10)** — regresiones del parser léxico extraído: rarezas históricas (CR suelto
  descartado, última fila sin salto emitida, salto final sin fila fantasma, comilla doblada, salto
  dentro de comillas, línea en blanco) **más** el lector tolerante conducido de punta a punta.
- **`StructuralSectionModelTests` (37)** — normalización, política de ids, ausencia de revisión en el
  id, lookups por id/EDI/designación, habilitado/deshabilitado, filtro por familia, ambigüedad
  rechazada, duplicado rechazado, propiedades opcionales, `tnom` ≠ `tdes`, cuadrado como caso de
  rectangular, factor exacto de conversión, peso por longitud (incluida la prueba de que usa
  `WeightPerLength` y no el texto de la designación) y el formateador en sus tres formas, con el
  ejemplo literal del dueño.
- **`StructuralSectionStrictReaderTests` (25)** — encabezado faltante, duplicado, desconocido y vacío;
  fila con número de celdas incorrecto; id vacío; número ilegible con archivo, línea, columna e id;
  `NaN`/infinito; separador de miles rechazado; requerido ausente; opcional vacío que queda `null`;
  familia inválida; familia que contradice su archivo; sistema de unidades inválido; booleano
  inválido; archivo vacío; líneas en blanco ignoradas; y el overlay de estado completo (deshabilita
  sin borrar, id desconocido, id duplicado, overlay vacío, round-trip con coma dentro del texto).
- **`StructuralSectionValidatorTests` (24)** — fuente desconocida, revisión ausente, id que no
  corresponde a su designación, cero donde se espera magnitud, **cero aceptado donde se espera
  posición**, peso y área no positivos, área ausente, dimensión de familia faltante, dimensiones que no
  corresponden a la familia, `tdes > tnom`, pared plana que alcanza la total, alas del ángulo
  invertidas, grado de material como advertencia, designación ambigua, y el manifiesto (conteo,
  hash, filas rechazadas, auto-inclusión, hash del libro ausente, round-trip y **JSON sin `\r`**).
- **`StructuralSectionImporterTests` (19)** — enumeradas en §12.
- **`ShippedStructuralSectionCatalogTests` (28)** — el catálogo realmente distribuido, incluidas las
  sentinelas de §10.

Regresiones vigentes que siguen verdes y no se tocaron: `CsvCatalogReaderTests`,
`SeccionesCatalogTests`, `CatalogValidatorTests` (I-19), `CatalogCanonicalIdsTests` y
`CatalogManifestGuardTests` (I-26), `ShippedCatalogIntegrityTests`, `CatalogBlockManifestTests`,
`NamespaceFolderGuardTests` (I-23) y `UiSystemBoundaryGuardTests`.

---

## 12. Pruebas del importador, sobre un XLSX generado en runtime

No se versiona ningún fixture binario: `SyntheticAiscWorkbook` construye un `.xlsx` **real** —
container, `workbook.xml`, relaciones, `sharedStrings.xml` y celdas con referencia `r`— incluido el
**espejo métrico**, así que las pruebas recorren el mismo camino que el libro de 2 MB y no un stub.

1. Solo entran las **cuatro familias autorizadas** (W, HSS rect., HSS cuadrado, C, L conviviendo con
   HSS redondo, MC y 2L).
2. Los tipos excluidos se **reportan por tipo** y no son error.
3. Redondo y rectangular se separan **por los campos oficiales**, con dos designaciones que empiezan
   igual y que ninguna expresión regular podría separar con fiabilidad.
4. Un HSS que no sea exactamente una de las dos formas es **ambiguo** y detiene la importación.
5. **Ninguna fila seleccionada se descarta**: filas seleccionadas == secciones producidas.
6. Un encabezado **renombrado** detiene la importación nombrándolo.
7. Un **espejo métrico ausente** detiene la importación.
8. Un **número ilegible** detiene la importación.
9. Una **dimensión obligatoria ausente** rechaza la fila con su número y su razón.
10. Un `T_F` inválido rechaza la fila.
11. Un **peso no positivo** rechaza la fila.
12. Una **colisión de ids** detiene la importación nombrando las dos designaciones.
13. Un espejo métrico **contradictorio** detiene la importación.
14. Dos corridas producen archivos **byte-idénticos**.
15. El manifiesto **no lleva marca de tiempo** y **no se hashea a sí mismo**.
16. El manifiesto declara el hash del libro y los conteos que produjo.
17. `Publish` escribe los siete archivos y **no deja staging**.
18. Lo publicado se lee con el **proveedor estricto** y **valida limpio**.
19. Una reimportación **preserva el overlay** de las secciones que siguen existiendo.

---

## 13. Guardas de alcance, verificadas con `git diff`

| Guarda | Resultado |
|---|---|
| `assets/catalogs/secciones.csv` | **byte-idéntico** a `origin/main` (`git diff --exit-code` limpio) |
| Los otros 10 catálogos vigentes | **cero líneas cambiadas** |
| `blocks-library.dwg` | no se tocó; **no se creó ningún bloque** |
| `src/RackCad.Domain` | **cero archivos** |
| `src/RackCad.UI` | **cero archivos** |
| `src/RackCad.Plugin` | **cero archivos** |
| `deploy/` | **cero archivos** |
| `.github/` | **cero archivos** |
| `docs/HANDOFF.md` | **no tocado** (corresponde a la sesión de integración) |
| Columna Estado del ROADMAP | las filas de la Fase 6 nacen en `pendiente`; **nada marcado como integrada** |
| `architecture/geometria-secciones-estructurales` | **no creada**; reservada para I-36B |
| Geometría, contornos, radios, proyecciones, bloques, AutoCAD, WPF | **cero** |

Archivos **modificados** (todos los demás son nuevos): `.gitignore` (ruta ignorada del libro),
`RackCad.sln` (alta del proyecto de herramienta), `tests/RackCad.Tests/RackCad.Tests.csproj`
(referencia a la herramienta) y `src/RackCad.Application/Catalogs/CsvCatalogReader.cs` (**solo** delega
su parser léxico; comportamiento idéntico y fijado por regresiones), más los documentos del §14.

---

## 14. Documentación tocada

`docs/ARCHITECTURE.md` (§4.4.1 nueva y §7.6 ampliada), `docs/guias/catalogos-y-plantillas.md`,
`docs/guias/modelo-de-datos.md`, `docs/guias/secciones-estructurales.md` (**nueva**, con la matriz
completa de columnas), `docs/adr/README.md`, `docs/adr/0005-*.md` (nota posterior),
`docs/adr/0008-*.md` (estado + nota posterior), `docs/adr/0020-*.md` y `docs/adr/0021-*.md` (nuevos),
`docs/ROADMAP.md` (Fase 6), `docs/initiatives/README.md`,
`docs/initiatives/I-36A-catalogo-secciones-estructurales.md` (nuevo),
`docs/automation/decisions/I-36A.md` (nuevo), `docs/automation/state/I-36A.yml` (nuevo) y esta
evidencia.

---

## 15. Validación del dueño — **APROBADA** (2026-07-28)

**AutoCAD: no aplica** — I-36A no cambia dibujo, bloques, comandos ni el comportamiento visible de
ningún sistema. El dueño lo confirmó expresamente al aprobar.

Gate `owner-validation`: **`approved`** sobre el HEAD técnicamente aprobado
**`5cd526cca252ffcd30dc0e598c8e3049632ea4ec`** (CI verde, run 30354958938, 4/4). Los siete puntos:

| # | Punto | Dónde se comprueba | Aprobado |
|---|---|---|---|
| 1 | Fuente oficial AISC Shapes Database v16.0 y su SHA-256 registrado | §2 — incluida la nota de que la URL `globalassets` del encargo ya no existe y de que se usó el enlace que publica la propia página oficial | ✔ |
| 2 | Conteos: W **289**, HSS rect./cuadrado **525**, C **32**, L **137**, total **983**, rechazadas **0** | §7 — excluidos 1 316; `983 + 1 316 = 2 299` cierra exacto | ✔ |
| 3 | Sentinelas documentadas, **dos por familia** | §10 — cada una con su fila del libro | ✔ |
| 4 | Peso con **unidad nativa primero** y equivalente después | §5 — `W12X26 — 26 lb/ft (38.7 kg/m)` | ✔ |
| 5 | Política de IDs `{ID_NAMESPACE}-{FAMILIA}-{EDI_NORMALIZADO}` | §6 | ✔ |
| 6 | Overlay habilitado/deshabilitado **sin** perder la resolución por ID | §0.5 y §6 — overlay de excepciones, hoy vacío; `GetById` sigue resolviendo una sección deshabilitada | ✔ |
| 7 | `secciones.csv`, **miembros** y sistemas existentes no cambiaron | §13 | ✔ |

### Aprobación expresa del caso HSS

Es el punto que había motivado el rechazo parcial de la primera ronda, y el dueño lo aprobó
explícitamente:

- `HSS4X4X1/4` conserva su **Manual Label visible**;
- su designación **EDI es `HSS4X4X.250`**;
- su **ID técnico es `AISC-HSS-RECT-HSS4X4X_250`**;
- `ID_NAMESPACE` es una **autoridad explícita**, declarada por la fuente y no una constante del código;
- **`AISC-SHAPES` declara el namespace `AISC`**, así que los 983 identificadores son los de siempre;
- la **revisión de la fuente no forma parte del ID**.

Con ello **ADR-0021 pasó de `propuesto` a `aceptado`**, con Mario Pérez como decisor Owner y fecha
**2026-07-28**. ADR-0020 ya estaba aceptado desde la ronda 1.

`requires_owner_validation: **satisfied**`. La decisión versionada está en
[`../decisions/I-36A.md`](../decisions/I-36A.md).

---

## 16. Confirmación expresa

- **No** se implementó nada de **I-36B**: cero geometría, contornos, radios, filetes, polilíneas,
  regiones, sólidos, vistas, longitudes prismáticas, orientación, proyección ni definiciones internas
  de AutoCAD. Su rama **no se creó**.
- **No** se tocó **AutoCAD**: ni la API, ni un bloque, ni `blocks-library.dwg`, ni `blocks.csv`.
- **No** se tocó la **UI**: `src/RackCad.UI` con cero archivos cambiados.
- **No** se migró ningún **miembro**: postes, largueros, celosías, separadores y `secciones.csv`
  intactos.
- **Ningún cambio funcional** en los sistemas existentes: Domain, UI y Plugin sin un archivo tocado, y
  el único cambio en Application fuera del namespace nuevo es la delegación del parser léxico.
- **`main` intacta**: todo el trabajo vive en `architecture/catalogo-secciones-estructurales`; no hubo
  merge, ni push a `main`, ni borrado de rama o worktree.

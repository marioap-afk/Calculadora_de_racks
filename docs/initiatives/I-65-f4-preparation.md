# I-65 — Preparación de F4 (Coordinator)

Documento de la unidad (Coordinator, 2026-10-11), por orden del Owner
([I-65-owner-authorization-f4-preparation.original.txt](../automation/decisions/I-65-owner-authorization-f4-preparation.original.txt)):
F3 = ACCEPTED; se autoriza **solo la preparación de F4** (documentación, verificación de contratos, preparación de evidencias y paquete de
validación de ingeniería). **F4 IMPLEMENTATION AUTHORIZATION = NO.** No se modificó código de producción, no se ejecutó el adaptador en
RAM Elements y no se declara completada la validación de ingeniería. Decisiones: [decisiones](../automation/decisions/I-65.md) §23.

Este documento cubre los puntos 1 y 5-9 del entregable; los puntos 2-4 (matriz, especificación manual y procedimiento) están en
[I-65-f4-engineering-validation-package.md](I-65-f4-engineering-validation-package.md).

## 1. Preflight (2026-10-11)

| Comprobación | Resultado | Evidencia |
|---|---|---|
| Rama | `architecture/interop-analisis-estructural`, HEAD `ad5443bd` = `ls-remote`, árbol limpio, 42 commits sobre `origin/main` | `git status`, `git ls-remote` |
| `origin/main` | `bb0d5522`, sin cambios desde el inicio de I-65; base de merge = `bb0d5522`: no hace falta rebase | `git merge-base` |
| Freeze | Proposal V3.1 blob `4e6e1feb`, último cambio en el commit del Freeze `a9d7080b` | `git rev-parse HEAD:…`, `git log -1` |
| ADR-0050 | blob `c18def42`, último cambio en la aceptación `ed8154c3` | ídem |
| CI del cierre de F3 | `1d8c713e`: corrida `push` 38126497464 `success`; registro `ad5443bd`: corrida 38126805055 `success`. En ambas, los 4 trabajos en `success` (Tests Domain + Application; Build UI; UI Tests; Build Plugin without AutoCAD) | `gh run view` |
| Código de producción desde el cierre | `git diff 1d8c713e..HEAD -- src tests assets` vacío | `git diff --stat` |
| Iniciativas paralelas | I-62 (`architecture/portabilidad-coordinador-principal`, `5369b83b`), I-64/ID30 (`architecture/workspace-persistente-rackcad`, `f3a9f40e`), I-52 (`feature/rackmirror-espejo-semantico`, `fb6b5648`): 0 archivos en las áreas de I-65 (`StructuralAnalysis`, `Units`, catálogos, `assets/structural-analysis`, pruebas, documentos de I-65); único archivo común `docs/ROADMAP.md`; `git merge-tree` limpio contra HEAD en los tres. I-64 toca `src/RackCad.Plugin/Workspace` (6 archivos): sin choque hoy; vigilar en F4 (R-12). Las ramas avanzaron durante la preparación: re-comprobación al cerrar en la evidencia §33 | `git diff --name-only`, `git merge-tree` |
| Hallazgos A65-F3-01..12 | **los 12 abiertos**, trasladados a F4 por CD-I65-F3-16; ninguno corregido (sin cambios de producción) | §5.2 |
| Pendientes de F1 (CD-I65-F1-20) | a F4: A65-F1-08, 09 y 11; a la primera tarea que toque la idealización o las familias: A65-F1-03, 10 y 12; A65-F1-06 si otra iniciativa necesita ese vocabulario. CD-I65-F1-19: (a) resuelto en F3 (permutación de D-04), (b) documentado sin cambio | §5.2 |
| Tabla de mapeo | 2 filas `proposed`, sin valores de ingeniería: toda exportación real es `Unsupported` | `section-material-map.json` |
| RAM Elements instalado | `RAMElements.exe` SHA-256 `7dcb4855…7970`, el mismo de P1; RAM no estaba en ejecución | pre-lectura |
| Autoridades vigentes | sin conflicto: el Freeze, ADR-0050, CD-I65-F1-01..21 y CD-I65-F3-01..17 no se contradicen con la orden; la orden no pide nada fuera de ellas | revisión del Coordinator |
| Codex | sin invocaciones en esta preparación; `config.toml` no tocado | — |

## 2-4. Matriz, especificación manual y procedimiento

En [I-65-f4-engineering-validation-package.md](I-65-f4-engineering-validation-package.md) §1, §2 y §3. Hallazgos de la preparación:

- Pre-lectura pasiva de la base instalada de RAM (sin abrir RAM): los cuatro nombres existen **una sola vez** en todas las bases de país;
  A, Ix, Iy y J de RAM coinciden **exactamente** con el catálogo AISC v16.0 en las dos secciones (Δ = 0 %); RAM guarda la HSS con el
  espesor de diseño (0.233 in), igual que el catálogo.
- Materiales: E (29 000 ksi) y ν (0.3) de RAM coinciden con los candidatos; el **peso unitario de RAM (486.94 pcf) queda −0.62 % por
  debajo de 490 pcf**, fuera de la tolerancia de ±0.5 % de la matriz §4 → decisión del Owner (OD-F4-04).
- La configuración de RAM del usuario apunta a una base de secciones de usuario «Montilla» no localizada (DIF-03).
- La columna necesita θ = −90° en el modelo manual; +90° deja el peralte en el plano pero con ejes opuestos, que la matriz rechaza
  (OD-F4-05).

## 5. Riesgos y comprobaciones

### 5.1 Riesgos priorizados (orden del Owner primero)

| # | Riesgo | Origen | Prob. / impacto | Comprobación ya hecha | Tratamiento en F4 | Pruebas |
|---|---|---|---|---|---|---|
| R-01 | **Lectura real de θ = −90°**: RAM podría devolver 270 y la lectura de vuelta exige igualdad literal → `FAILED` en falso | A65-F3-01 | media / alto (Candidato perdido) | P1 leyó igual 0°, 90° y 12.3456°, nunca un giro negativo | comparar giro módulo 360 (o no comparar el giro: los ejes ya lo verifican); registrar el valor leído | T4-01, SIM-01, SIM-02, M-01 |
| R-02 | **Descripción del miembro**: la lectura de vuelta exige descripción = id (15-16 caracteres); un truncado da `FAILED` en falso | A65-F3-02 | baja-media / alto | P1 solo leyó `"N2"` | no exigir igualdad; registrar el valor leído (la identidad del miembro la dan sus nudos) | T4-02, SIM-03, M-02 |
| R-03 | **Apoyos parcialmente restringidos** (`Restraints(dx, ry, rz)`) no ejecutados en P1 | F3 §7; Architect F3 | baja / medio | P1 ejecutó `FIXED` y `PINNED` por el mismo puente; la plantilla lee de vuelta las banderas exactas | lectura de vuelta exacta (ya existe) y fila de apoyos del comparador | T4-03, SIM-04, M-03, OV-I65-01 |
| R-04 | **Integridad del paquete y lectura única de datos**: carpeta embebida y destino como argumentos separados; lectura de vuelta escrita fuera del `try` (archivo vacío); reglas de carpeta; carrera del destino; el sobre del diseño leído dos veces | A65-F3-07, 04, 08, 09; A65-F1-08 | baja / alto | manifiesto con SHA-256 de cada archivo y huella de entrada repetida en la lectura de vuelta (F3) | una sola cadena de carpeta; escritura de la lectura de vuelta dentro del manejo de errores con estado de respaldo; endurecer la regla de carpeta; prueba de la carrera; una lectura del sobre, bytes hasheados intactos | T4-04, T4-06..T4-09, SIM-05 |
| R-05 | **Correspondencia de unidades, ejes y cargas** entre generado, manual y volcados | D-03, D-04, D-11; A65-F3-10 | media / alto | golden de RAM derivado a mano (F3 §8); especificación manual con sentido I → J y θ fijados | volcado con contrato `LB_IN`; comparador para las filas de §4 que leen los volcados (incluidos los nudos de cada miembro y la categoría del caso del generado); las filas de propiedades de sección y material, por la hoja firmada (paquete §3 F) | T4-12..T4-15, OV-I65-01, M-04 |
| R-06 | **Rechazo seguro de modelos incompatibles**: modelo no vacío, caso `REF1` de un intento abortado, modelo analizado, versión de API distinta, carga útil inválida | D-08, D-09; A65-F3-11, 03 | baja / alto | puertas de capacidad y de modelo vacío antes de la primera escritura (F3, simulacro S3, S4, S6) | comprobaciones de la carga útil antes de escribir; la guía explica que «gate:» = el script no escribió, y que todo `FAILED` se descarta sin guardar (D-08) | T4-05, T4-21, SIM-06, SIM-07, OV-I65-04, OV-I65-07 |
| R-07 | Filas sin verificar y diferencias de material (peso unitario −0.62 %; G frente a ν) | D-07; DIF-01, DIF-02 | cierta / bloquea la OV | pre-lectura y matriz | firma del ingeniero; decisión del Owner | T4-18, hoja §1.6 |
| R-08 | Nombre duplicado en una base de usuario («Montilla») | Freeze §12; DIF-03 | baja / medio | sin duplicados en las bases instaladas | revisión del ingeniero (paso B4) | — |
| R-09 | Entorno de RAM: licencia lenta en estado «Desconocido», error gRPC al cerrar, diálogo modal tras analizar | P1 | media / medio | — | procedimiento §3 H; el Owner atiende la licencia | — |
| R-10 | Ruta larga (MAX_PATH) o reintento con el mismo paquete (lectura de vuelta exclusiva) | Architect F3 | media / bajo | simulacro S2 de F3 (no sobrescribe una lectura de vuelta existente) | carpeta corta; un paquete nuevo por intento (guía) | T4-07 (ruta); S2 (reintento) |
| R-11 | Deudas de guardas: literales partidos para esquivar INV-14; prueba de F1 que busca «Writer»; constructor público del perfil de material | A65-F3-05, 06, 12 | — / bajo | invariantes intactas (Architect F3) | mover a archivo eximido o exención por ruta; edición autorizada de la prueba; guarda «el comando usa solo la tabla» | T4-17, T4-19 |
| R-12 | Choque futuro con I-64/ID30 en el Plugin (guardas del comando de D-13) | Freeze §8; preflight | baja / medio | sin archivos comunes hoy | archivo de comando nuevo fuera de `Plugin/Workspace`; nada de código de ID30 | T4-16 |
| R-13 | Costo de tiempo del Owner: Candidato de hito en F4 y repetición sobre el Candidato final | Freeze §5, §12 | cierta / medio | — | planificar la sesión de OV con el ingeniero; decidir cómo se repite el modelo manual (OD-F4-11) | — |
| R-14 | **Lectura por la API de lo creado a mano**: P1 solo leyó entidades creadas por la API (con el enum, la categoría se guardó como `"LoadCategory.DEAD"`); la categoría, el modo de posición de la carga o el giro del modelo manual podrían leerse distinto y dar un FAIL en falso | Architect A65-F4P-02 | media / medio | — | categoría comprobada solo en el generado (`"OTHER"`), informativa en el manual; posición comparada en pulgadas tras normalizar el %; medición M-05 | T4-14, M-05 |
| R-15 | **Distribución de la tabla con el producto**: `RackCad.Plugin.csproj` solo copia `assets/catalogs` (`Content` y `BundleCatalogFiles`) | Architect A65-F4P-03 | cierta / medio | — | la autorización de F4 nombra la ruta exacta del `.csproj`, limitada a distribuir la tabla | T4-16, T4-18 |

### 5.2 Trazabilidad de las observaciones heredadas

| ID | Tratamiento propuesto en F4 | Momento | Pruebas |
|---|---|---|---|
| A65-F3-01 | giro módulo 360 o sin comprobación de giro | antes del Candidato de hito | T4-01, SIM-01, SIM-02 |
| A65-F3-02 | descripción no exigida; valor registrado | antes del Candidato | T4-02, SIM-03 |
| A65-F3-03 | guía: «gate:» = el script no escribió; aun así, todo `FAILED — DISCARD MODEL` se descarta sin guardar (D-08) | guía de F4 | T4-21 |
| A65-F3-04 | escritura de la lectura de vuelta dentro del manejo de errores, con estado de respaldo | antes del Candidato | T4-04, SIM-05 |
| A65-F3-05 | literales a un archivo eximido o exención por ruta exacta | F4 | T4-19 |
| A65-F3-06 | edición autorizada de la prueba de F1: E/S neutral solo en `StructuralPackageWriter.cs` | F4 | T4-19 |
| A65-F3-07 | una sola cadena para carpeta embebida y destino | F4 | T4-06 |
| A65-F3-08 | segmentos terminados en punto o espacio y nombres reservados → `Invalid` | F4 | T4-07 |
| A65-F3-09 | prueba de la carrera del destino con el sistema de archivos falso | F4 | T4-08 |
| A65-F3-10 | comparador: categoría del caso y nudos de cada miembro | F4 | T4-14 |
| A65-F3-11 | comprobaciones de la carga útil antes de la primera escritura | antes del Candidato | T4-05, SIM-06 |
| A65-F3-12 | guarda: el comando usa solo la tabla | F4 | T4-17 |
| A65-F1-08 | una entrada neutral lee el sobre una vez, deserializa esos bytes y los hashea intactos | F4 | T4-09 |
| A65-F1-09 | excepciones de la fuente o del resolver → `Unavailable` en el comando | F4 | T4-10 |
| A65-F1-11 | redacción del diagnóstico de bloqueo de línea | F4 | T4-11 |
| A65-F1-03 | IDL-09 como lista de permiso (W, HSS-RECT, S): una familia nueva → `Unsupported` | F4-T4 | T4-22 |
| A65-F1-10 | acotar la re-ortogonalización del vector de peralte (p. ej. sen 0.01°) y rechazar lo mayor | F4-T4 | T4-23 |
| A65-F1-12 | fallar cerrado ante un miembro estructural desconocido en el plano de la estación | F4-T4 | T4-24 |
| A65-F1-06 | disposición expresa en el contrato de F4: sin cambio (las subcadenas de INV-11 no han dado falsos positivos), salvo que F4 lo necesite | contrato de F4 | — |

CD-I65-F1-20 enviaba A65-F1-03, 10 y 12 a «la primera tarea que toque la idealización o las familias» y A65-F1-06 a «si otra iniciativa
lo necesita»; CD-I65-F3-16, posterior, los incluye entre las entradas obligatorias de F4. Prevalece CD-I65-F3-16 (CD-I65-F4P-06): el
contrato de F4 trae cada uno con corrección o disposición expresa; la propuesta es la de la tabla.

## 6. Plan de pruebas positivas y negativas (para cuando el Owner autorice F4)

Tres niveles: (a) pruebas automáticas en la suite, con RED antes de GREEN (namespace `RackCad.Tests`; clases con prefijo
`StructuralAnalysisRamElements…`; filtros con `FullyQualifiedName~`); (b) simulacro **fuera de la suite** con un módulo `ramelements`
falso, como el de F3 (evidencia, no RAM; INV-14 prohíbe Python en la suite); (c) corridas reales en RAM (OV y mediciones).

### 6.1 Automáticas (suite)

| ID | Qué verifica | Positiva | Negativa / RED esperado |
|---|---|---|---|
| T4-01 | Giro leído (R-01) | la plantilla acepta −90 y 270 como el mismo giro (o no compara el giro y sí los ejes) | la plantilla actual, que compara igualdad literal, hace fallar la prueba |
| T4-02 | Descripción (R-02) | la lectura de vuelta no exige descripción = id y la registra | exigir igualdad hace fallar la prueba |
| T4-03 | Banderas de apoyo (R-03) | contrato de la carga útil: `COL.FOOT` seis banderas, los demás `dx ry rz` | mapeo índice a índice o una bandera perdida → falla (ya cubierto por INV-08; se repite sobre la carga útil final) |
| T4-04 | Lectura de vuelta robusta (A65-F3-04) | `json.dumps` y la escritura dentro del manejo de errores, con estado de respaldo | plantilla con la escritura fuera del `try` → falla |
| T4-05 | Orden de las comprobaciones (A65-F3-11) | todas las comprobaciones de la carga útil antes de `units.set` | una comprobación después de la primera escritura → falla |
| T4-06 | Una sola carpeta (A65-F3-07) | ruta embebida = carpeta escrita | dos cadenas distintas posibles → falla |
| T4-07 | Reglas de carpeta (A65-F3-08) | ruta absoluta de Windows normalizada → `Valid` | segmento que termina en punto o espacio, `CON`, `NUL`, `COM1` → `Invalid` |
| T4-08 | Carrera del destino (A65-F3-09) | sin carrera, publicación atómica | el destino aparece entre comprobación y renombrado → fallo sin tocarlo y sin temporales |
| T4-09 | Lectura única del sobre (A65-F1-08) | mismos bytes → misma `SourceFingerprint` | cambio solo de espacios → huella distinta; diseño re-serializado no admitido como entrada |
| T4-10 | Excepciones (A65-F1-09) | — | excepción de la fuente o del resolver → `Unavailable`, sin paquete |
| T4-11 | Diagnóstico de bloqueo (A65-F1-11) | bloqueo de línea con su propio código | informado como `station-N` → falla |
| T4-12 | Plantilla del volcado (D-11) | imports `ramelements`, `json`, `math`; ruta embebida; archivo exclusivo; `units.set` solo si el par no es `LB_IN`, con restauración en `try/finally` | `analyze`, `save`, `new`, `os.*` o `units.set` incondicional → falla |
| T4-13 | Lector del volcado | volcado en `LB_IN` → aceptado | otro par → `Invalid("unit pair")`; volcado `FAILED` → rechazo; esquema roto → `Invalid` |
| T4-14 | Comparador, INV-13, A65-F3-10 y R-14 | volcados iguales → PASS; casos sin cargas ignorados; categoría del manual distinta → solo informativa; misma posición dada en % y en distancia → PASS | +1 mm; sección cambiada; eje local invertido (columna con +90°); carga ×1000; emparejamiento ambiguo; volcado `FAILED`; huella distinta (`STALE`); par distinto de `LB_IN`; categoría del **generado** distinta de `"OTHER"`; miembro con nudos invertidos → FAIL o rechazo |
| T4-15 | Bordes de tolerancia de §4 | 0.019 in; 0.09°; 0.09 % de carga → PASS | 0.021 in; 0.11°; 0.11 % → FAIL |
| T4-16 | Comando del Plugin (D-13, INV-12, A65-F1-05, R-15) | un solo archivo nuevo en la ruta exacta de la lista de permiso de G1; el `.csproj` solo añade la tabla a `Content` y al paquete | suscripción a eventos de AutoCAD, `SetImpliedSelection`, WPF, `PaletteSet`, edición de menús o código de ID30 → falla |
| T4-17 | Autoridad única de material (A65-F3-12) | el comando lee la tabla con su lector | `new StructuralMaterialProfile(` fuera de la tabla → falla |
| T4-18 | Tabla verificada | dos filas `verified` completas → `Valid`; la tabla distribuida con el producto es la del repositorio | quitar `verifiedBy`, `source` o una unidad → `Invalid`; cambiar un campo de una fila verificada → `AnalysisInputFingerprint` distinta (INV-04) |
| T4-19 | Guardas (A65-F3-05, 06) | literales de INV-14 solo en archivos eximidos por ruta exacta; E/S neutral solo en `StructuralPackageWriter.cs` | un literal partido fuera de un archivo eximido, o E/S en otro archivo neutral → falla |
| T4-20 | Herramienta de inyección de fallos (OV-05, 06, 07) | cada paquete difiere del válido solo en el campo inyectado | el producto no puede emitir esos paquetes (guarda) |
| T4-21 | Estado de los rechazos de puerta (A65-F3-03) | el detalle de todo rechazo previo a escribir empieza por `gate:` (el estado sigue siendo `FAILED — DISCARD MODEL`) | un rechazo de puerta sin `gate:` → falla |
| T4-22 | Familias admitidas (A65-F1-03) | W, HSS-RECT y S → admitidas | una familia fuera de la lista → `Unsupported` |
| T4-23 | Re-ortogonalización acotada (A65-F1-10) | componente eliminada ≤ el límite → igual que hoy | componente mayor → diagnóstico, sin modelo |
| T4-24 | Miembros desconocidos (A65-F1-12) | estación con columna y brazos `PositiveY` → `Valid` | un miembro estructural en el plano que la fuente no conoce → fallo cerrado |

### 6.2 Simulacro fuera de la suite (módulo `ramelements` falso; no es RAM)

Reejecutar S1-S6 del simulacro de F3 sobre la plantilla corregida y añadir:

| ID | Respuesta del módulo falso | Esperado |
|---|---|---|
| SIM-01 | giro de las columnas leído como 270 | `READBACK OK` |
| SIM-02 | giro de las columnas leído como 0 (ejes con el peralte fuera del plano) | `FAILED` por ejes |
| SIM-03 | descripción truncada a 8 caracteres | `READBACK OK`, valor registrado |
| SIM-04 | una bandera de apoyo perdida en la lectura | `FAILED` |
| SIM-05 | `json.dumps` lanza una excepción | `ram-readback.json` no vacío, con estado `FAILED` de respaldo |
| SIM-06 | carga útil con material vacío | rechazo `gate:` sin ninguna escritura |
| SIM-07 | modelo con un caso `REF1` previo | rechazo `gate:` sin escritura |
| SIM-08 | volcado: par inicial `KIP_FT` | `units.set(LB_IN)` y restauración; volcado en `LB_IN` |

### 6.3 Corridas reales en RAM (con el Candidato de hito)

| ID | Escenario | Positiva | Negativa |
|---|---|---|---|
| M-01 | giro leído de las columnas | registrado en la lectura de vuelta (−90 o 270) | — |
| M-02 | descripción leída | registrada | — |
| M-03 | banderas parciales leídas | iguales a las escritas | — |
| M-04 | par de unidades | `units.current` inicial y final registrados; interfaz sin cambio | — |
| M-05 | lectura por la API del modelo manual (R-14) | categoría del caso, modo de posición de la carga, giro de las columnas y banderas registrados | — |
| OV-I65-01 | §3 C-F del paquete de ingeniería | `READBACK OK`; matriz §4 PASS según la tabla de evaluadores (paquete §3 F), salvo informativas | — |
| OV-I65-03 | estación no soportada o sección sin fila en la tabla | — | sin paquete, diagnóstico visible |
| OV-I65-04 | modelo no vacío (desechable, o el manual ya volcado), paquete nuevo | — | rechazo sin modificar el modelo |
| OV-I65-05 | sección inexistente | — | `FAILED — DISCARD MODEL`; el comparador lo rechaza |
| OV-I65-06 | ejes esperados alterados | — | `FAILED` |
| OV-I65-07 | capacidad alterada | — | rechazo antes de escribir; nudos = miembros = 0 |

Una sonda previa en RAM para M-01..M-03 (P2) es opcional (OD-F4-07): con T4-01, T4-02 y la lectura de vuelta exacta, los tres casos
quedan cubiertos de forma segura dentro de OV-I65-01.

## 7. Condiciones de aceptación de F4

| # | Condición |
|---|---|
| AC-F4-01 | Orden del Owner que autorice la implementación de F4, con presupuesto; contrato de gate (I-61) con las entradas de CD-I65-F3-16 y CD-I65-F1-20 |
| AC-F4-02 | Freeze y ADR-0050 sin cambios, o A-n aprobada por el Owner |
| AC-F4-03 | Resultado de la fila F4: comando mínimo, volcado y comparador, con RED→GREEN (selección > 0) y T4-01..T4-24 en verde |
| AC-F4-04 | A65-F3-01, 02, 04 y 11 corregidos **antes** del Candidato de hito; el resto de §5.2 (incluidos A65-F1-03, 06, 10 y 12, CD-I65-F4P-06) corregido o con disposición expresa del Coordinator revisada por el Architect |
| AC-F4-05 | Las dos filas `verified` con la hoja firmada (§1.6) y su evidencia; DIF-01..04 resueltas, con el reconocimiento del Owner registrado literalmente; OD-F4-03 decidida antes del Candidato (fija si la fila de propiedades de sección es informativa o criterio) |
| AC-F4-06 | Candidato de hito completo (LIFECYCLE §8) y DLL según la guía manual |
| AC-F4-07 | OV-I65-01 (matriz §4 PASS según la tabla de evaluadores del paquete §3 F, salvo informativas; deltas registrados) y OV-I65-03..07 PASS, con custodia |
| AC-F4-08 | Fronteras: Domain intacto; sin tipos de RAM en el modelo neutral; ID30 e I-62 intactos; sin interfaz gráfica nueva (solo comando de línea); en el Plugin, solo el archivo de comando y la edición del `.csproj` para distribuir la tabla; sin análisis, importación de resultados, guardado ni sobrescritura de modelos de RAM |
| AC-F4-09 | Revisión del Coordinator del diff contra el Freeze y las A-n, hallazgos y desviaciones, sin EXP-01 A abierta (LIFECYCLE §7 punto 5); revisión independiente del Architect sin REQUIRED abierto |
| AC-F4-10 | LIFECYCLE §7: commit de cierre → árbol limpio → Core Full local → push → CI del SHA exacto en verde → GATE PASS registrado después; todo rebase exigido entre gates produce una punta nueva con su propia evidencia (punto 7) |
| AC-F4-11 | Condición de Q-O1 y Q-O7: si OV-I65-01 muestra que la transferencia no es verificable sin hipótesis incorrectas → STOP y decisión del Owner |

## 8. Decisiones que requieren al Owner

| ID | Decisión | Recomendación del Coordinator |
|---|---|---|
| OD-F4-01 | Autorizar la implementación de F4, su alcance de rutas y su presupuesto | autorizar con **28** invocaciones de Codex (4 tareas × 6 + 4 de reserva para un ciclo de corrección; en F3 el tope de 14 impidió corregir A65-F3-01 y 02); el alcance nombra `src/RackCad.Plugin/RackCad.Plugin.csproj`, limitado a distribuir la tabla (R-15) |
| OD-F4-02 | Designar al ingeniero verificador de las filas (`verifiedBy`), cuándo firma, y **autorizar expresamente abrir RAM** para verificarlas (licencia y perfil de RAM) | antes del Candidato de hito; puede trabajar en paralelo a la implementación |
| OD-F4-03 | Confirmar Q-O2 = (b): la base de RAM como fuente, con comparación ±0.5 % informativa | confirmar (b) **antes del Candidato**: la pre-lectura no muestra deltas |
| OD-F4-04 | Oráculo de material: ν o G; y el peso unitario (−0.62 % frente a 490 pcf) | ν o G según la fuente que cite el ingeniero (si solo hay G, reconocer DIF-02); **reconocer** el delta de peso unitario por escrito (V1 no usa peso propio) en lugar de cambiar el valor esperado |
| OD-F4-05 | Giro de las columnas en la referencia manual | confirmar **θ = −90°** (concreción de §15 dentro del Freeze, coherente con el golden de F3 §8 y con CD-I65-F3-16); una regla de signo en el comparador cambiaría el criterio de §4 y exigiría una A-n |
| OD-F4-06 | Archivo de cargas para OV-I65-01 (las filas de cargas de §4 cuentan en OV-I65-01 por el Freeze; excluirlas exigiría una A-n) | el archivo `REF1` del fixture de F1 (`P1`-`P3`); OV-I65-02 sigue en F5 con su propio archivo |
| OD-F4-07 | Sonda previa P2 en RAM (giro, descripción, banderas parciales) | no hacerla; medir M-01..M-03 dentro de OV-I65-01 |
| OD-F4-08 | Quién opera RAM en las OV (Owner o ingeniero a mano, o Coordinator con Computer Use como en P1) y quién atiende la licencia | Owner o ingeniero a mano para el modelo manual; el Coordinator puede ejecutar los scripts con autorización expresa por corrida |
| OD-F4-09 | Cómo se obtiene `REF-CANT-01` en un DWG (paso C1) | F4 entrega los pasos para dibujarlo con RackCad con los parámetros de F1 §1; un DWG de referencia, opcional |
| OD-F4-10 | Base de usuario «Montilla» | el ingeniero la revisa; si contiene los mismos nombres, se excluye de la corrida |
| OD-F4-11 | Costo del Candidato de hito en F4 más la repetición de OV-I65-01..07 sobre el Candidato final (Freeze §5), y cómo se repite el modelo manual | aceptar el costo (lo fija el Freeze; reducirlo exige al Owner). Para la repetición: reutilizar el volcado manual custodiado si el hash de `RAMElements.exe` y de los archivos de la base no cambiaron y el ingeniero conservó el modelo manual (guardarlo es decisión suya, D-11); si no, reconstruirlo con §2 |

## 9. Recomendación GO / NO-GO

- **Ejecutar F4 en RAM hoy: NO-GO.** Las filas siguen `proposed`, el delta de peso unitario no está decidido y A65-F3-01, 02, 04 y 11
  no están corregidos: una corrida real hoy no tiene paquete válido (`Unsupported`) y, con filas verificadas, se arriesgaría a un `FAILED`
  en falso.
- **Autorizar la implementación de F4: GO condicionado** a OD-F4-01 (con la ruta del `.csproj` en el alcance), OD-F4-05 y OD-F4-06.
  Las tareas de código no dependen de las filas verificadas (las pruebas usan filas de fixture), así que la verificación del ingeniero
  (OD-F4-02..04, 10) puede correr en paralelo. Compuerta interna: **ningún Candidato de hito ni corrida en RAM** hasta cumplir AC-F4-04 y
  AC-F4-05 (incluida OD-F4-03 decidida).

## 10. Propuesta de descomposición (entrada para el contrato; no es autorización)

| Tarea | Alcance | Verificación |
|---|---|---|
| F4-T1 | Endurecer plantilla y paquete: A65-F3-01, 02, 04, 07, 08, 09, 11; guardas A65-F3-05, 06, 12; T4-21 | Core; T4-01..T4-08, T4-17, T4-19, T4-21; SIM-01..07 |
| F4-T2 | Volcado (plantilla fija del proveedor) y lector con el contrato `LB_IN` | Core; T4-12, T4-13; SIM-08 |
| F4-T3 | Comparador puro (Application), herramienta de consola fuera del producto e inyección de fallos | Core; T4-14, T4-15, T4-20 |
| F4-T4 | Comando del Plugin (un archivo nuevo, lista de permiso de G1), A65-F1-08, 09, 11; A65-F1-03, 10, 12 en la fuente; tabla distribuida con el producto (`RackCad.Plugin.csproj`) | Core + Build Plugin; T4-09..T4-11, T4-16, T4-18, T4-22..T4-24 |
| F4-D | Cambio de datos: filas `verified` tras la firma del ingeniero | T4-18 |
| F4-C | Candidato de hito, corridas en RAM y OV | §6.3; AC-F4-05..07 |

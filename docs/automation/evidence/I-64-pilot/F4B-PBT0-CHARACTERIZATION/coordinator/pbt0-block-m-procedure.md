# I-64 F4B PB-T0 — Procedimiento del bloque M (medición física en AutoCAD), v2

**v2:** corrige N-I1 de la comprobación de la v2 del paquete del contrato (`pbt0-independent-review-v2-check.md` §4.1). En la v1, los `RCDUMP` iban entre la orden medida y sus `U`, y absorbían el «Actualizar». Cambian P-5, el §1, M-5a, M-5b, M-5c y el tramo de Deshacer de M-3. Se añaden el control (b) y el §2.1.

**Autoridades:**
- el paquete de la fase v3.1: `docs/initiatives/I-64-f4b-push-back-completo-package.md` en `67e28452`, §6.3 a §6.5 (`:607-672`);
- la decisión del Owner «La sesión (Recommended)»: `F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, pregunta 3, con su respuesta y la descripción de la opción (F4B-E03). Opera la sesión por control de pantalla, con las reglas de Smoke-2. Se cita por párrafo, no por línea;
- el paquete del contrato de PB-T0, bloque C (`docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md`, INV-PBT0-20 e INV-PBT0-24), del que este procedimiento es anexo.

**No cambia producto:** no compila nada, no usa arnés, no pone variables de depuración ni toca configuración de seguridad.

**Marcadores** que se usan en la custodia:
- `<carpeta-de-copias-de-PB-T0>`: la carpeta de trabajo de este bloque, fuera del repositorio;
- `<carpeta-del-DLL-de-C2>`: el DLL del Candidato de F4 (`F4-SMOKE-2/prep/dll-c2.md`);
- `<original-D1>` y `<original-D2>`: los originales de Smoke-2. D1 es el rol R006 y D2 el rol R053, que es el mismo original que D3b.

## 0. Preparación (sin AutoCAD)

| Id | Paso | Criterio |
|---|---|---|
| P-0 | Antes de cada sesión de medición: comprobar que el Worker de PB-T0 (bloque C) no tiene en curso una corrida local de los filtros de UI (OD-6; INV-PBT0-20 del paquete del contrato). Mientras dure la medición, el Worker no los corre | Sin corrida de UI en curso. Si la hay, se espera a que termine; nunca se mide en paralelo |
| P-1 | SHA-256 de los cuatro DLL de `<carpeta-del-DLL-de-C2>` (Plugin, Application, Domain y UI) | Iguales a `dll-c2.md`. Si no, STOP |
| P-2 | La carpeta `catalogs` del DLL frente a `assets/catalogs` en `5ccaa770` | Igual byte a byte. Si no, STOP |
| P-3 | `git diff 9abc1320 <HEAD de la rama> -- src assets` | Vacío. Si no, STOP |
| P-4 | Variables `RACKCAD_DEBUG_*` en el usuario, la máquina y el entorno del proceso que lanza AutoCAD | Ninguna. Si hay alguna, STOP |
| P-5 | **Copias**, con `copy` (lectura del original, sin abrirlo en AutoCAD):<br>• de `<original-D1>`: `M1.dwg`, `M2.dwg`, `M3.dwg`, `M4.dwg` y `M6.dwg`, y **las de Deshacer** `M1u.dwg`, `M1v.dwg`, `M2u.dwg` y `M3u.dwg`;<br>• de `<original-D2>`: `M7-D2.dwg`;<br>• si una medición de Deshacer necesita k ≥ 2 (§2), se hace en ese momento otra copia nueva del mismo original, `<copia>-k<k>.dwg` (por ejemplo, `M1u-k2.dwg`).<br>SHA-256 de cada original antes y después, y de cada copia | Cada copia igual a su original (D1 = `7A868624…F47A5`). Los originales, sin cambios. `copias.json` registra todas las copias, también las `-k<k>` |
| P-6 | AutoCAD sin documentos. Las copias se abren por COM (`Documents.Open`) | **Nunca** se abre un original |

## 1. Carga e inventario (en cada documento)

- **Carga del DLL:** `(command "NETLOAD" "<carpeta-del-DLL-de-C2>/RackCad.Plugin.dll")`, en la línea de comandos («Load Once»). Una vez por proceso de AutoCAD.
- **Funciones de inventario, de solo lectura.** Se teclean en trozos de ≤ 16 caracteres, con 3 s entre `defun` y líneas de ≤ 1024 caracteres. `SECURELOAD` impide `load`, así que se teclean en cada documento, **al abrirlo y antes de cualquier orden medida**.
  - **`rcj`:** el sobre de una definición. Toma de `RACKCAD_SELECTIVE` en el diccionario de extensión del `BLOCK_RECORD` los grupos 1 y los concatena. Es la de Smoke-2.
  - **`rcn`:** el número de entidades de una definición. Recorre `entnext` desde `tblobjname "BLOCK"`.
  - **`c:RCDUMP`:** escribe `<carpeta-de-copias-de-PB-T0>/inv/<dibujo>-<etiqueta>.txt`, con la etiqueta preguntada por `getstring`:
    - (a) por cada `INSERT` de una definición con sobre: el handle, el nombre efectivo, el punto de inserción, la rotación, la escala X, la capa y la caja `GetBoundingBox`;
    - (b) por cada definición con sobre: `DEF`, el nombre, las referencias, las entidades y la longitud del sobre, y en la línea siguiente el **sobre JSON completo**;
    - (c) `CLAYER`.
- **Regla de Deshacer (N-I1).** Toda expresión LISP tecleada (`defun`, `RCDUMP`, `rcj`, `rcn`, la selección con `ssget`) es un grupo de Deshacer propio, y puede absorber la orden anterior. Por eso:
  - **entre la orden medida y sus `U` no se teclea nada:** ni `RCDUMP`, ni `rcj` ni `rcn`, ni la selección, ni ninguna otra orden;
  - la selección y los `RCDUMP` previos van **antes** de la orden medida;
  - después de los `U`, **un solo** `RCDUMP u<k>`;
  - el eco de cada `U` se transcribe **literal** en `run-log-m.md`.

  «La orden medida» es RACKEDITAR con su edición en la ventana, hasta su «Actualizar», o lo que diga cada fila del §2.
- **El análisis** lo hace después la sesión, con un script de Python sobre los `.txt`. Por definición extrae GUID, `Name`, `Kind`, vista, sección, `AnnotationScale`, `DimensionStyle`, el número de frentes, si es compuesto (`SideB` con `IsPresent`), los módulos y las ranuras ausentes. Compara dos estados cualesquiera: definiciones nuevas, borradas o cambiadas (sobre o número de entidades) y referencias.

## 2. Mediciones

**Antes de cada orden medida** se elige el rack con la selección por expresión de Smoke-2 (`ssget` con el nombre del bloque). Se hace una captura antes de cada clic en una ventana de RackCad o de AutoCAD, y cada captura se abre y se revisa. Un error del operador se puede repetir como máximo dos veces (§6.5 de la fase).

**Mediciones de Deshacer.** Cada una va en **su copia nueva** (P-5). El patrón es:
1. funciones tecleadas;
2. `RCDUMP pre`, o el estado previo que diga la fila;
3. selección;
4. la orden medida;
5. `U` × k seguidos, con el eco de cada uno transcrito;
6. **un solo** `RCDUMP u<k>`.

Se empieza con k = 1. Si `u1` no es igual al estado de referencia, se repite **en otra copia nueva** con k = 2, y así sucesivamente, hasta la igualdad o hasta k = 10. Si no se alcanza, se registra NO DISPONIBLE. Nunca se sigue deshaciendo más allá de la igualdad.

| Id | Copia | Rack | Pasos | Qué se registra |
|---|---|---|---|---|
| M-1 | M1 | PB E (planta y lateral, 8 frentes) | `RCDUMP pre` → selección → RACKEDITAR → «+ frente» una vez → «Actualizar» → `RCDUMP post` | El mensaje de la línea de órdenes; qué definiciones cambian; si aparece alguna vista o definición nueva (H-PB-16 dice que no); los nombres |
| M-5a | M1u (y `M1u-k<k>`) | PB E | Funciones → `RCDUMP pre` → selección → RACKEDITAR → «+ frente» → «Actualizar» → `U` (eco literal) → `RCDUMP u1`. Comparación de `u1` con `pre` | Cuántos `U` deshacen el «Actualizar», sin nada en medio; el eco de cada `U` |
| M-5a (b), control | M1v | PB E | Lo mismo que M-5a, pero con **`REGEN`** tecleado entre «Actualizar» y `U`: … → «Actualizar» → `REGEN` → `U` (eco literal) → `RCDUMP u1`. Comparación de `u1` con `pre` | Si una orden nativa posterior absorbe el «Actualizar» (§2.1) |
| M-6a | M2 | PB E | `RCDUMP pre` → selección → RACKEDITAR → elegir en el selector lateral el **último poste** → «Insertar» esa vista → en el jig, escribir un punto libre a la derecha de las vistas existentes → `RCDUMP ins` | **Creación:** el nombre del bloque y el sufijo, el punto, la rotación, la escala, la capa de la referencia frente a `CLAYER`, las capas de los textos y las cotas dentro de la definición, el GUID compartido y el sobre de la vista nueva |
| M-2a | M2 | PB E | Tras M-6a: selección → RACKEDITAR → «- frente» una vez, con lo que el último poste desaparece → «Actualizar» → `RCDUMP post` | Que el corte del último poste se borra (definición y referencias); el mensaje con «cortes obsoletos eliminados» |
| M-5b | M2u (y `M2u-k<k>`) | PB E | Funciones → `RCDUMP pre` → selección → RACKEDITAR → selector lateral, último poste → «Insertar» → punto libre → `RCDUMP ins` (todo **antes** de la orden medida) → selección → RACKEDITAR → «- frente» → «Actualizar» → `U` (eco literal) → `RCDUMP u1`. Comparación de `u1` con `ins` | Cuántos `U` deshacen el «Actualizar» que borra el corte; si el corte borrado vuelve |
| M-2b | M6 | PB F | Regla del último vínculo. Insertar el corte del último poste (como M-6a). Borrar con `ERASE` las demás referencias de PB F y quitar con `PURGE` sus definiciones, para que el corte sea la única vista. Después selección → RACKEDITAR → «- frente» → «Actualizar» → `RCDUMP` | El mensaje de «todas obsoletas» y que no se borra nada (MA H-062). Si el procedimiento no deja una sola vista, se registra NO DISPONIBLE |
| M-3 | M3 | PB F | `RCDUMP pre` → selección → RACKEDITAR → nombre «PB F2» → «Actualizar» → `RCDUMP r1` → selección → RACKEDITAR → nombre «PB E» (ya existe) → «Actualizar» → `RCDUMP r2` | Los nombres de las definiciones y el `Name` de cada sobre en `r1` y `r2`; la colisión (`SyncName` sin sufijo) |
| M-3 (Deshacer) | M3u (y `M3u-k<k>`) | PB F | Funciones → `RCDUMP pre` → selección → RACKEDITAR → «PB F2» → «Actualizar» → selección → RACKEDITAR → «PB E» → «Actualizar» → `U` (eco literal) → `RCDUMP u1`. Comparación de `u1` con el estado tras el primer renombrado, que es `r1` de M-3 en la copia M3: M3 y M3u son copias del mismo original, así que los sobres y las definiciones son comparables | Cuántos `U` deshacen el segundo renombrado |
| M-6b | M3 | PB F | Tras M-3, en la misma sesión de RACKEDITAR: cambiar el nombre a «PB F3» **y**, sin «Actualizar», pulsar «Insertar» la planta → punto libre → `RCDUMP i2` | MA H-077: si el sobre de la vista nueva lleva el nombre o el diseño anteriores a la edición |
| M-4 | M4 | PB F, **CONSTRUCTED REPRESENTATIVE TEST CASES** | `RCDUMP pre` → selección → RACKEDITAR → marcar «Rack de dos sentidos (lado B)» → lado activo B → quitar «En blanco» a un frente de B → «Actualizar» → `RCDUMP ab1` → selección → RACKEDITAR (comprobar que la casilla sale marcada) → «Insertar» el frontal del lado B → punto libre → `RCDUMP ab2` → selección → RACKEDITAR → desmarcar «Rack de dos sentidos (lado B)» → «Actualizar» → **sigue M-5c, sin nada en medio** | Si el rack queda compuesto en `ab1`; el frontal B insertado |
| M-5c | M4, sin copia extra | PB F | Tras el último «Actualizar» de M-4: `U` (eco literal) → `RCDUMP u1`, comparado con `ab2`. Después **se repite la misma edición**: selección → RACKEDITAR → desmarcar «Rack de dos sentidos (lado B)» → «Actualizar» → `RCDUMP ab3` → captura del frontal B. Si `u1` ≠ `ab2`: otra copia nueva `M4-k2.dwg`, que repite M-4 hasta su último «Actualizar» y sigue con `U` × 2 → `RCDUMP u2`, y así sucesivamente | Los `U` del «Actualizar» de la vuelta a un sentido. Con `ab3`: qué se dibuja en el frontal B tras quitar B (H-PB-18) y qué cortes laterales se vuelven obsoletos y se borran (H-PB-17 corregido) |
| M-7 | M1 (D1) y M7-D2 | todos | `RCDUMP` en los dos dibujos y recuento de las definiciones Push Back con `DimensionStyle` no nulo. Si hay alguna con referencia: selección → RACKEDITAR → «Actualizar» → `RCDUMP` y captura de una cota | El recuento. Si no hay ninguna, **NO DISPONIBLE**: decide la prueba Core de OB-06.1 y el recuento (0) es el dato para el Owner antes de PB-TC |

### 2.1 El dato ya tomado y la absorción

- **El dato ya tomado no se descarta.** Se registran como **«MEDIDO con `RCDUMP` intercalado (evidencia de absorción, no respuesta de M-5)»**:
  - M-5a en M1, en la que un solo `U`, con eco «RCDUMP GROUP (Lisp Expression)», devolvió el rack al estado previo;
  - M-5b en M2.

  No cuentan como error del operador ni consumen las dos repeticiones de §6.5, porque el defecto era del procedimiento y se corrige por escrito.
- **Lo que responde M-5** son M-5a en M1u, M-5b en M2u, M-3 (Deshacer) en M3u y M-5c en M4.
- **Si el control (b) confirma la absorción** (en M1v, `REGEN` absorbe el «Actualizar»), es un **hecho para el paquete de PB-T3** («un solo Actualizar lógico» y su Deshacer). Se registra en el análisis y en MA §11 según el último punto del §6.3 de la fase. **No es una decisión del Owner ahora.**

## 3. Cierre y evidencia

- Cada copia se cierra **sin guardar**. A la pregunta de guardar de AutoCAD se contesta «No».
- **Al final:**
  - SHA-256 de los originales, sin cambios;
  - SHA-256 de todas las copias, también las `-k<k>`, iguales a su original porque nada se guardó;
  - sin variables `RACKCAD_DEBUG_*`.
- **El registro con hora** (`run-log-m.md`) lleva:
  - cada paso con su resultado: MEDIDO, MEDIDO con `RCDUMP` intercalado, NO OBSERVABLE o NO DISPONIBLE;
  - el eco literal de cada `U`.
- **Las capturas** van recortadas al panel o la ventana, a la línea de órdenes y a la vista, sin rutas ni nombres de cliente, cada una con su `derivation-record`.
- **El análisis** queda en `block-m-analysis.md`: la tabla de hechos M-1 a M-7, con M-5 respondido por las copias de Deshacer y el control (b). Actualiza MA §11 y resuelve el §6.4 de la fase.
- **Custodia** en `I-64-pilot/F4B-PB-T0/block-m/`, con los inventarios redactados: los sobres JSON de racks reales pueden llevar nombres de proyecto o de cliente, así que se sustituyen o se resumen antes de custodiar.
- **Un resultado que contradiga MA** se registra, y el mapa se corrige. **Un resultado que exija cambiar producto** no se corrige en PB-T0.

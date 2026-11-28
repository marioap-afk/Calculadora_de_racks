
## Recomprobación 1 (v5.6, solo el diff)

**Fecha:** 2026-11-22. **El objeto:** la v5.6 (`pbt4/I-64-f4b-pbt4-gate-contract-package-v56.md`, sha256 `6b8939ae26d39392824108fe9b73bc3741f30a45657810a2ecf534e9a40cf66a`), solo en lo que cambia frente a la v5.5. Con ella van sus piezas, la precondición 6 repetida (las copias r76 a r83) y el traspaso. **Las reglas** son las del primer encargo: objetos git en `160e019d`, nada compilado ni ejecutado, Python solo con `python -I <archivo>` (esta vez sin ningún desliz) y escrituras solo en `S`.

**Las entradas, con su sha256 comprobado:**

| Archivo (relativo a `S`) | sha256 |
|---|---|
| `pbt4/I-64-f4b-pbt4-gate-contract-package-v56.md` | `6b8939ae…f66a` |
| `pbt4/pbt4e-green-v56.txt` | `a745c273…8158` |
| `pbt4/pbt4e-red-v56.txt` | `d052f73d…c9e1` |
| `pbt4/session-pbt4e-design-decisions.md` | `b21f2af4…1e6d` |
| `pbt4ecc/compile/pbt4e-precondition6.json` | `daea5984…c169` |
| `pbt4ecc/compile/compile-check.md` | `549766eb…d426` |
| `pbt4ecc/compile/post-p6-v56-out.txt` | `36f88c52…b485` |
| `pbt4/v56/handoff-pbt4e-v56.md` | `dc7e6775…45e0` |

También casan con lo que dice el paquete:
- **las herramientas:** `red_pbt4e_v56.py` (`8c38e532…`), `apply_red_pbt4e_v56.py` (`59a0fcb3…`), `green_pbt4e_v56.py` (`b3aba680…`), `apply_green_pbt4e_v56.py` (`492aaab1…`) y `v56/make_pieces_pbt4e_v56.py` (`8a00c0d2…`); `green_` y `red_` solo cambian la versión respecto a las de la v5.5;
- **los registros:** `v56/make_v56-out.txt` (`53e884a8…`), `v56/historia/v56-38efafdb.md` (`38efafdb…`), `cov-p6-v56.txt` (`057591b7…`), `cov-p6-existentes-v56.txt` (`3887c2ef…`) y `driver-r76-tr.txt` a `driver-r83-p6.txt`, con los sha256 de `compile-check.md`.

**Reproducido por mí:**
- **el GREEN de la v5.6,** aplicado en memoria sobre `160e019d`: cada OLD está una vez, y WHC da `6eb9bf7a…`; los otros tres archivos dan lo mismo que en la v5.5. Es lo que escriben las copias r82, r83, r80 y r81;
- **los dos `NewFileSha`:** el de Core, `9e849864…`, y el de UI, `a29d7a36…`, sin cambio;
- **el anexo V56-A,** igual byte a byte a las piezas;
- **el diff de secciones** de la v5.5 a la v5.6, y el de tres vías con la v5.4;
- **PBT4E-C11,** frente a PBT4D-C11 del contrato emitido;
- **las 25 líneas,** frente a las sondas de r76 y r82;
- **la cobertura de las dos líneas VARIA,** leída en los JSON de coverlet de r73, r79, r72 y r83.

**Veredicto:** EMITIBLE, B 0 / I 0 / M 2 (M-06 y M-07, uno de texto y otro de custodia, sin decisión del Owner; M-01, M-02, M-03 y M-04, resueltos; M-05, para el registro de la emisión).

### Los hallazgos nuevos

| Id | Gravedad | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| **M-06** | Menor | **El registro de la sesión:** D-S17 (`pbt4/session-pbt4e-design-decisions.md:11-20`), frente a D-S19 (:58-81) y al GREEN de la v5.6 | **D-S17 sigue describiendo el sitio de la v5.5.** Dice «una rama `else if (draft.NeedsLazyReload)`, tras la de «sin borrador» y antes de las comprobaciones del estado, del texto pendiente y de `ShowModal` … Es el GREEN literal del anexo V55-A» (:13), y «la de empezar o la de recargar (`else if`)» (:19).<br>**D-S19 dice que ajusta el sitio** (:58), pero D-S17 no lleva ninguna nota que remita a ella, como sí la lleva la corrección de M-03 (:48). El título sigue siendo «sobre la v5.5» (:1).<br>**El paquete está bien:** D-PBT4-22 dice «D-S17 (confirmada; el sitio exacto, D-S19)». Pero quien lea D-S17 sola en el registro custodiado se queda con el sitio de antes | **Lo más barato:** declararlo en el registro de la emisión (F4B-E60), junto a M-05: «el sitio de D-S17 lo sustituye D-S19 (`!begunHere && draft.NeedsLazyReload`, tras las tres comprobaciones; anexo V56-A)».<br>**La otra opción:** añadir esa nota en D-S17 y en el título. Pero cambia el sha256 del registro (`b21f2af4…`), que el paquete cita en la cabecera, en §1, §8.5 y §11 y en §6.5.2. Haría falta una v5.7 solo de texto |
| **M-07** | Menor | **La v5.6:** la cabecera (:5) y §1, la fila «La comprobación acotada de la v5.5». **§11.10** | **La v5.6 cita la versión de antes de esta recomprobación.** Cita `independent-review-pbt4e-check.md` con `27636ed5…ef72`, que es el informe sin esta sección. Al añadirla, el sha256 del archivo cambia.<br>**Lo comprobé:** los primeros 30 371 bytes del archivo resultante siguen dando `27636ed5…`; lo de antes no se tocó.<br>**§11.10** custodia «la comprobación acotada CA-T4e … y su recomprobación», así que la custodia necesita el sha256 final | **En la custodia** (F4B-E60), registrar el sha256 final del archivo y que `27636ed5…` es el de sus primeros 30 371 bytes, la comprobación de la v5.5.<br>No hace falta cambiar el paquete |

### Lo que comprobé de M-01 a M-05

**M-01 → D-S19: resuelto.**

**Nada evalúa la marca antes de las tres comprobaciones de la vista.** En el GREEN, `EditAllOfPilot` va así:

| Paso | Líneas (GREEN WHC) |
|---|---|
| La compuerta, la actualización en vuelo y el anfitrión del editor | :1185-1191, sin cambio |
| La búsqueda, con `begunHere = false` | :1194-1195 |
| La rama de «sin borrador», con `begunHere = true` | :1196-1203 |
| Las tres comprobaciones, sobre `ViewOfPilotDraft` | :1205-1208 |
| **Solo después,** `if (!begunHere && draft.NeedsLazyReload)` | :1215-1219 |
| `ShowModal` | :1228 |

`NeedsLazyReload` no aparece en ningún otro sitio de `EditAllOfPilot`.

**Los estados distintos de «Clean y marcado» responden exactamente como en `160e019d`:**

| Estado | La respuesta | Dónde (GREEN, y en la base) |
|---|---|---|
| Applying o no registrado | `Waiting` | GREEN :1206, base :1204 |
| «Sin decidir», Unknown o Error | `Refused(EditAllStateReason)` | GREEN :1207, base :1205 |
| Con texto pendiente | `Refused(EditAllPendingTextReason)` | GREEN :1208, base :1206 |
| Conflict | Abre como hoy: `NeedsLazyReload` es falso por el estado explícito, sin llamar al comparador | — |
| Sin la marca | Abre como hoy: corta en `IsPossiblyStale` | — |
| Dirty | Abre como hoy: `NeedsLazyReload` es falso | — |
| Un borrador que el gesto acaba de empezar | No se recarga (`begunHere`) | — |

**Sobre el Dirty sin estado explícito:** el comparador se llama una vez más, después de `ViewOfPilotDraft`. Solo cambiaría algo con un comparador no determinista. El de producción es una igualdad pura (`PushBackFullDraftStateComparer.cs:19-25`), así que no es un hallazgo.

**Sigue habiendo como mucho una lectura por gesto, y ninguna con el editor abierto:**
- la estructura lo garantiza: se empieza el borrador o se recarga, nunca las dos cosas;
- la recarga va dentro de la marca de `TryEditAll` y antes de `ShowModal`;
- las líneas de CT-01 que lo fijan no cambian.

**La línea nueva fija lo que dice.**
- **La receta:** un borrador empezado en el registro con `PilotEditThrowingComparerFull`, que siempre lanza «the comparer threw» (`tests/RackCad.Tests/Workspace/WorkspacePilotEditHostFull.cs:16-27`, la misma que usa `OpenUndecidable` en :115-123). Después, `MarkPossiblyStale` y `TryEditAll`.
- **Lo que fija:** `Refused` con `EditAllStateReason`, sin lecturas, sin fallos y con la marca puesta.
- **Lo medido:**

| Copia | El GREEN | `marcado-sin-decidir` | CT-01 |
|---|---|---|---|
| r76 | El de la v5.5 (`apply-green.txt`: WHC `fa19d28c`) | `estado=Failed motivo=[the comparer threw] … fallos=[WorkspaceHostCore.TryEditAll]` | Cae |
| r82 | El de la v5.6 (WHC `6eb9bf7a`) | `estado=Refused …` | Pasa |
| r83 | El de la v5.6 | — | La suite de Core del GREEN da 0 `Failed` |
| El RED | — | Ya da `Refused` | Cae por `marcado-limpio` |

**Las 24 líneas que comprobé siguen iguales y en su orden:**
- las 15 de Core de la v5.5 están en la v5.6, más la nueva;
- la clase de UI es idéntica byte a byte;
- con el GREEN de la v5.5 (r76), la sonda da exactamente las otras 15 de Core y las 9 de UI; con el de la v5.6 (r82), las 25;
- §5.7 las lleva todas.

**Los textos dicen lo mismo que el código:**
- **D-PBT4-22:** el punto 1, las citas de WHC:1203-1206 y :1209-1215 de `160e019d`, «Lo que no hace» y la nueva sección D-S19;
- **§6.5.1:** INV-PBT4E-03 y 04, y PBT4E-C13, con `begunHere` y la rama como cambios que no son declaraciones;
- **R-T4-41 (§7), §8.5, §0 y §1;**
- **la única excepción es el registro de la sesión** (M-06).

**El control** (r80 y r81) se hizo con el GREEN final: `a745c273`, con las piezas de control `d46c2550` y `c0712516`. Da lo mismo que en la v5.5.

**M-02: resuelto.**

**Los dos límites están declarados:** en §4.5 y en la cabecera de `pbt4ecc/compile/cov_p6_pbt4e_v56.py` (:16-25, sha256 `cbbffa87…`). El resto del guion solo añade `MODO=existentes` (:47 y :209, que no cuenta como CAE una línea nueva sin ejecutar) y el título.

**La corrida opcional está bien hecha:**
- **Core sin las dos clases** (`FullyQualifiedName!~PushBackEditAllReload`): 19418 = 19015 + 0 + 403 (`driver-r83-p6.txt`), que es el total de la base.
- **`ui_union18_v56.txt`** es `ui_union_v55.txt` sin el filtro 19 (por `diff`). La unión da 899 = 869 + 0 + 30 las tres veces.
- **Cada filtro en su proceso:** se usan los `green-ui01` a `18` de la principal, porque ningún filtro heredado selecciona las clases nuevas; del lado de la base, `base-uif01` a `18`.

**Y sostiene que las pruebas existentes conservan sus caminos:**
- PASA, sin líneas ni ramas perdidas;
- los cuerpos vigilados quedan iguales:

| Cuerpo | Líneas (base → GREEN) | Ramas (base → GREEN) |
|---|---|---|
| `EditAllOfPilot` | 30 → 30 | 21 → 21 |
| `TryReloadPilotEdit` | — | 24 → 24 |
| `EditAll` | 34 → 34 | 18 → 18 |
| `BeginEdit` | 33 → 33 | 25 → 25 |

- el texto cambiado, FVM:410, sigue con sus 44 golpes;
- sin las CT nuevas, solo se ejecutan GREEN WHC:1195 y :1202, y :1215 tiene 3 de 4 ramas: solo las CT nuevas llegan a la recarga, como dice §4.5.

**Las 2 líneas VARIA** (`src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs:37-38`, las propiedades `Available` y `Selected`) **son variación entre corridas, sin relación con el cambio.**
- PB-T4e no toca ese archivo ni su carpeta.
- Lo medí en los JSON de coverlet: el mismo código de la base da 0 de 3 uniones en r73 y 2 de 3 en r79.

| Copia | Uniones que la cubren | El filtro 04 |
|---|---|---|
| r73 (la base de la v5.5) | 0 de 3 | La cubre |
| r79 (la base de la v5.6) | 2 de 3 | La cubre |
| r72 (el GREEN de la v5.5) | 0 de 3 | La cubre |
| r83 (el GREEN de la v5.6) | 0 de 3; y 1 de 3 en la unión de 18 | La cubre |

**La regla las trata bien:**
- la línea sigue ejecutada en el GREEN, por el filtro 04 en su proceso, así que el PASA no se toca;
- la base no es estable (2 de 3), así que son VARIA y no PERDIDA (`cov_p6_pbt4e_v56.py:285-287`), como dice D-S16;
- en la v5.5 no salían porque las dos bases daban 0 de 3.

**M-03: resuelto.** La etiqueta está corregida (`session-pbt4e-design-decisions.md:44-48`). Las dos premisas, `list-proposal.md:31` y `owner-pbt4c-code-decisions.md:25`, están nombradas con su conclusión (:50-54), y la información para PB-T5 está en :56.

**M-04: resuelto, y basta como «un solo armado».**
- **`post-p6-v56-out.txt`** nombra la `6b8939ae` (:30-31).
- **Frente al intento 2** solo cambian esas dos líneas. Las del sha256 del JSON (`daea5984`) y de `compile-check.md` (`549766eb`) son iguales.
- **La `38efafdb` y la `6b8939ae`** solo difieren en la línea 9.
- **Los dos `make_v56-out`** solo difieren en la línea de las 10 secciones de más y en la de la versión.
- **La versión que se emite** sale de un solo `make_v56`, el último paso de un `post-p6` cuyo registro la nombra.
- **La anterior** se guarda en `v56/historia/` con su registro, y §11.10 custodia `v56/`.
- **El traspaso** declara la segunda ejecución de `make_v55.py` (`a241bfbe` → `e597790f`) y lo que cambió en `ops-d.md` y `head.md`.

No hace falta nada más.

**M-05:** va al registro de la emisión (F4B-E60), según §1 y el registro de la sesión (:88). Hoy no se puede comprobar.

### Los literales nuevos, «Lo que cambia» y el resto

**PBT4E-C11 es PBT4D-C11 con exactamente los reemplazos declarados.**
- **El origen:** `make_c11_v56.py` lo saca del paquete en `160e019d`. Ese texto es igual al del contrato emitido de PB-T4d (`F4B-PBT4-RETIRE-PILOT/coordinator/gate-contract.json`): lo comparé.
- **El fragmento** `v56/frag/c11.txt` es la línea del paquete.
- **El diff palabra a palabra** solo da:
  - la etiqueta, PBT4D → PBT4E (el cuerpo no tiene ningún `PBT4D`);
  - el «PB-T4d» final → «PB-T4e»;
  - «version 2» → «version 3», dos veces;
  - la ruta, el SHA-256, la decisión, la revisión y la propiedad 10 de la v3;
  - la cláusula de la satisfacción de PB-T4d.
- **En `160e019d`:** la v3 está en su ruta y da `e68b6e59…28c6` (la v2, `c033e96c…`). La revisión dice «Veredicto: APTO» (`independent-review-substitute-v3.md:9`).
- **La decisión del Owner,** literal, es «Sustituto v3 y revisión (Recomendado)», con «la v3 sustituye a la v2 de aquí en adelante, PB-T5 incluida» (`owner-pbt4d-nc2-decision.md:21` y `:27`).
- **La lectura del autor (desviación 8 del traspaso) es correcta.**
  - Lo de PB-T4d no fue una aceptación puntual: el Owner no eligió «Aceptar nc2 solo esta vez» (:18). Fue la satisfacción bajo un sustituto revisado.
  - Ponerla entre lo que no se extiende tiene la misma forma que la disposición de PB-T3a, que PBT4D-C11 ya llevaba.
  - Y es cierto: PB-T4e corre su propio nc2.
- **El texto** es ASCII.

**P-01, con la Snapshot L, solo en lo de PB-T4e.**
- Va literal en §6.5.1: `docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-L/snapshot-l.json`, que en `160e019d` da `69781991…5e8e`, con `ConfigSha256` `B71650A6…EBB3` y 105 nombres.
- La decisión del Owner está en `owner-config-snapshot-l-decision.md:21` y `:25`: la L sustituye a la K.
- **§6.0 sigue con la K** para PB-T4a a PB-T4d (:2189 y :2200), y el P-01 del contrato emitido de PB-T4d es la K.
- **El bloque de §6.5.1 sigue siendo ASCII:** 18 paradas (P-01 y las 17 propias) y 14 invariantes; con las 16 estándar de §6.0, 34.

**§6.5.2** lleva las autoridades nuevas con sus rutas en `160e019d`: la v3, su revisión APTO, la decisión del Owner y F4B-E58; la Snapshot L, su decisión y F4B-E57. Lleva también la v2 y la K entre las que ya no son autoridades. F4B-E57 y F4B-E58 casan con el mensaje del commit `160e019d`.

**«Lo que cambia» sale del diff de secciones.**
- Las 14 cambiadas y el anexo nuevo son exactamente las de `make_v56-out.txt`.
- **Las 109 sin cambio son ciertas** (mi diff de tres vías): 99 sin cambio desde la v5.4, que son PB-T4a a PB-T4d, y las 10 que nombra la cabecera, con el archivo de CT-PBT4E-03 en el anexo. El preámbulo tampoco cambia.
- **No hay ningún cambio sin declarar.**
- **Un detalle que no cuento como hallazgo:** el cambio del nombre de las copias (r71 → r82, r74 → r80 y r75 → r81) también aparece en D-PBT4-22, §5.7 y §7, que la columna «Dónde» de V56-04 no nombra. «Lo que cambia» sí nombra esas secciones.

**Los totales, los `RequiredTests` y los filtros no cambian.**
- La tabla de los 90 es idéntica fila a fila a la de la v5.5.
- `required-pbt4e-v56.tsv` es idéntico byte a byte al de la v5.5 (`a66fd8d4…`).
- Los filtros son `ui_filters_pbt4e_v55.txt` (`d769e5e3…`).
- **Los totales,** los de la v5.5: Core RED 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403; UI RED 2465 = 2416 + 1 + 48 y GREEN 2465 = 2417 + 0 + 48.
- **La base** (r79): Core 19418 = 19015 + 0 + 403, igual que la CI `45057868915`; la unión de los filtros, 899 = 869 + 0 + 30.
- **En r83** corrieron los 38 procesos de UI.
- **`values-v56.json`** solo cambia, frente al de la v5.5, en lo declarado: las duraciones, la cobertura, el JSON y su registro, las 21 compilaciones, el control y las entradas nuevas.

**Las desviaciones del traspaso: ninguna afecta a la medida.** El traspaso tiene 8; el encargo dice «1 a 6» y llama «desviación 6» a la lectura de C11, que en el traspaso es la 8.

| N.º | Qué | Por qué no afecta |
|---|---|---|
| 1 | r77: el GREEN con `CS8604`, sobrescrito | El final se midió en r82 y r83. La cabecera de r76 lista el sha256 de aquel GREEN como pieza, pero r76 aplicó el de la v5.5 (`apply-green.txt`) |
| 2 | r78 sin UI | r83 la repite entera |
| 3 | Las tres ejecuciones de `post-p6` | Comprobado arriba (M-04) |
| 4 | El control no se repite | Se hizo con el GREEN final |
| 5 | Los ensayos en seco | No son versiones |
| 6 | `python -I -m py_compile` y dos archivos en `/tmp` | Están fuera de las reglas, pero se borraron y no tocan nada de lo medido |
| 7 | Los guiones reutilizados | La cabecera de r79 dice «(v5.5)» porque usa `p6_basecov_v55.sh` sin cambio: es cosmético |
| 8 | La lectura de C11 | Correcta (arriba) |

**Decisiones nuevas del Owner: ninguna.**
- D-S19 no cambia nada visible ni la lista; el sitio de la recarga es lo que el Owner dejó a la sesión.
- C11 y P-01 aplican decisiones del Owner ya tomadas: la v3 «de aquí en adelante» y la Snapshot L.
- M-06 y M-07 son de texto y de custodia.

### Lo no verificado

- **No compilé ni ejecuté nada.** Lo de las corridas lo tomo de sus registros, cuyo sha256 comprobé. La cobertura no la volví a calcular: de los JSON solo leí las dos líneas VARIA.
- **Los GREEN que no se conservan:** el primero de la v5.6 (`8e3e8077`, sobrescrito) y la v5.5 `a241bfbe`. Solo cuentan como historia.
- **Lo que aún no existe:** el número de `ForbiddenWriteScope`, que da el generador, y el registro de la emisión (M-05, M-06 y M-07).
- **La suite completa de UI y el Smoke-PB:** como en la comprobación de la v5.5.

> ; recomprobacion v5.6: EMITIBLE; B 0 / I 0 / M 2 (M-06: el D-S17 del registro de la sesion sigue describiendo el sitio de la v5.5, un else if antes de las comprobaciones, sin remitir a D-S19; M-07: la v5.6 cita CA-T4e con el sha256 27636ed5, el de antes de esta recomprobacion; los dos de texto o de custodia, sin decision del Owner); M-01 resuelto por D-S19 (la recarga tras las tres comprobaciones de la vista, con !begunHere && NeedsLazyReload; nada evalua la marca antes; marcado-sin-decidir da Failed con Report con el GREEN de la v5.5 en r76 y Refused sin fallos ni lecturas con el de la v5.6 en r82 y r83; las otras 24 lineas iguales); M-02 resuelto (los dos limites declarados en 4.5 y en el guion; la corrida opcional sin las clases nuevas, Core 19418 = 19015 + 0 + 403 y la union de 18 filtros 899 = 869 + 0 + 30 tres veces, PASA con los cuerpos vigilados intactos; las 2 VARIA de RackViewBatchDialogPresenter.cs:37-38 varian entre corridas del mismo codigo, la base r73 0/3 frente a la r79 2/3, y el filtro 04 las cubre siempre); M-03 corregido en el registro; M-04 resuelto (post-p6 nombra la 6b8939ae; frente a la 38efafdb solo cambia la linea 9, y en post-p6 solo las dos lineas de la version); M-05 al registro de la emision; PBT4E-C11 es PBT4D-C11 con los reemplazos declarados y la v3 e68b6e59 (APTO; decision del Owner Sustituto v3 y revision, de aqui en adelante); P-01 con la Snapshot L 69781991 solo en PB-T4e, y 6.0 sigue con la K; 109 secciones sin cambio = 99 + 10; RequiredTests 90 y su tsv identicos a la v5.5; precondicion 6 sobre 160e019d: base Core 19418 = 19015 + 0 + 403 igual a la CI 45057868915, RED 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403, UI RED 2465 = 2416 + 1 + 48 y GREEN 2465 = 2417 + 0 + 48, 21 compilaciones sin warning CS, control campo y anfitrion correcto; GREEN y RED de la v5.6 reproducidos por objetos git con los mismos sha256

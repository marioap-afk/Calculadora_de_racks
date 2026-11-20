# PB-T4a: compilación de comprobación (precondición 6 del paquete de PB-T4 v4.3)

**Qué es:** la comprobación que piden la precondición 6 del paquete de PB-T4 y la lección de F4B-E45. Antes de emitir, la sesión aplica el esqueleto de PB-T4a sobre una copia exportada del cierre de PB-T3c, fuera del worktree, lo compila, cuenta los avisos y **ejecuta la suite completa de Core** y los filtros de UI del contrato. Además prueba el GREEN literal que prescribe el paquete, por recomendación de CA-T4a (M-01). Las sobrecargas ambiguas, la reflexión por nombre, los escaneos de ensamblado y los avisos solo se ven al compilar y ejecutar.

**Las copias:** cada fase usa una copia nueva, `git archive 7f0d9aa377ca1e3301bfce3716bd29d602d762dd` (el cierre de PB-T3c, F4B-E48) desde el checkout principal, en una carpeta de ruta corta (`<copia>`), fuera del worktree. Ninguna es un worktree de git, y los guiones se niegan a reutilizar una carpeta.

**El compilador:** el SDK de usuario, con `--no-incremental -nodeReuse:false -p:UseSharedCompilation=false`. `tests/RackCad.Tests` y `tests/RackCad.UI.Tests` en Debug, y `src/RackCad.Plugin` en Release con `-o` hacia una carpeta dentro de la copia. Al acabar cada fase, `build-server shutdown`. No queda ningún `dotnet.exe`, `VBCSCompiler.exe` ni `testhost.exe` en marcha, y el checkout principal sigue sin cambios (`git status --porcelain` vacío).

## 1. La comprobación válida, sobre la v4.3 (`compile_check_v2.sh`)

### 1.1 El esqueleto (`out-run-skel.txt`, `out-skel/`)

**El parche:** `pbt4a-skeleton.patch`, sha256 `e99c87c61dec083c9b9efefd7a7cf44e0bae31fd0e477b1347618683e4d1de19`, sobre la v4.3 (`4f482279…`). Son 4 archivos, 2 nuevos que no existían en la base, y lleva el `Skip` de CT-PBT3A-12.

| Paso | Resultado |
|---|---|
| Compilación: Core-tests Debug, UI-tests Debug y Plugin Release (`-o`) | rc 0 en los tres, **0 errores y 0 avisos CS** |
| El control positivo | `IWorkspaceEditIntentOriginReader` y `PushBackOriginByText` están en `RackCad.Application.dll` |
| La suite completa de Core (`out-skel/trx-outcomes.txt`) | **19 129 = 19 122 pasadas + 0 fallidas + 7 omitidas** |
| Los 12 filtros de UI del contrato, uno a uno | Todos en verde, sin ningún fallo. `Workspace`: 680 pasadas y 1 omitida; `PushBackFullEditorCharacterization`: 16 pasadas y 2 omitidas; los demás, todas pasadas |

### 1.2 El GREEN literal (`out-run-green.txt`, `out-green/`)

**Qué parche se usó (CA-T4a, M-05):** esta fase corrió antes de que existiera el parche de la v4.3. Aplicó la parte de `tests/` del parche de la v4.2 (`25771775…`) y tomó los filtros de UI de un contrato en seco intermedio (`bead322a…`), como dice su registro. No tiene efecto: la parte de `tests/` es solo el `Skip` de CT-PBT3A-12, idéntico en los dos parches, y los 12 filtros de UI son los mismos en los dos contratos.

**Qué se aplica:** la parte de `tests/` del parche, que es el `Skip` de CT-PBT3A-12, y el `src/` del GREEN literal que prescribe la v4.3, escrito por `apply_green_pbt4a.py` (sha256 `b53a6b32…`). Son 6 archivos, y su conjunto tiene sha256 `7d29d396d8181e4ae83c5ec2693ed20e6f71b2846938eb07816967d33f4586cd`, el mismo que mide el redactor del paquete. El guion se niega a escribir si la copia no es una exportación exacta de la revisión.

| Paso | Resultado |
|---|---|
| Compilación de los tres proyectos | rc 0 en los tres, **0 errores y 0 avisos CS** |
| El control positivo | Los dos tipos nuevos, en `RackCad.Application.dll` |
| La suite completa de Core (`out-green/trx-outcomes.txt`) | **19 129 = 19 122 pasadas + 0 fallidas + 7 omitidas**. Con el GREEN prescrito no cae ninguna prueba existente fuera de la lista de PB-T4a |
| Los 12 filtros de UI | Todos en verde, con las mismas cuentas que en el esqueleto |

**Las siete omitidas, en las dos fases,** llevan su motivo literal:
- las seis de D-1 (S-1 a S-6, F4B-E29);
- CT-PBT3A-12, con el `Skip` del parche: «I-64 F4B PB-T4a: sustituida por CT-PBT4A-06 de PushBackFullReadGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).»

**Las TRX:** `out-skel/core.trx` (sha256 `7364e3b1…`) y `out-green/core.trx` (sha256 `3283e0c8…`), solo por SHA-256, porque llevan usuario, máquina y rutas. Su resumen está en los `trx-outcomes.txt`.

**Lo que esto cubre de M-01 de CA-T4a:** la suite completa de Core y los filtros de UI corren sobre el `src/` del GREEN literal, así que una guarda que lea esos archivos por un camino que el recorrido por marcas no ve también se ejecuta. El riesgo que queda bajo PBT4A-C16 es solo una diferencia entre el GREEN del Worker y el literal del paquete.

## 2. La primera compilación, sobre el parche de la v4.2 (`compile_check.sh`): sustituida

**Qué dio** (`out-run1.txt`, `out/`): compilación con 0 errores, el control positivo correcto y Core 19 129 = 19 122 + 7.

**Lo que no vio, un fallo de la sesión:** los registros traían **6 avisos CS8632**, producidos por el `string?` de `PushBackOriginByText.cs` en un proyecto con `Nullable=disable`. El guion contaba solo errores. `build-plugin` de la CI falla con cualquier `warning CS`, así que el GREEN no habría dado 4/4. Lo encontró CA-T4a (I-01); la v4.3 corrige el código y añade la regla al §5.0.

**La lección:** la compilación de comprobación cuenta los `warning CS` y exige 0. `compile_check_v2.sh` lo hace.

## 3. El control negativo (`compile_check_ctrl.sh` y `compile_check_ctrl2.sh`, sobre la copia de la §2)

**El primer control partía de una premisa falsa de la sesión, y lo declaro** (`out-run-ctrl.txt`, `out-ctrl/`).
- **La premisa:** sin el `Skip`, CT-PBT3A-12 fallaría sobre el esqueleto.
- **Lo que pasó:** sin el `Skip`, la guarda pasa sobre el esqueleto. Es inerte y no nombra los tipos del modelo fuera de `Draft/`, como ya preveía el paquete («sin el `Skip`, 19 123 + 6»).
- **Lo que significa:** el `Skip` hace falta para el GREEN.

**El control corregido** (`out-run-ctrl2.txt`, `out-ctrl2/`):

| Paso | Resultado |
|---|---|
| 1. La guarda vuelve al archivo de la base, sin el `Skip`, y se recompila | rc 0, 0 errores |
| 2. En RIA de la copia se añade una línea que nombra `PushBackFullDraftReader` fuera de `Draft/`, como hará el GREEN | **CT-PBT3A-12 falla 1 de 1** |
| 3. RIA se restaura byte a byte (el mismo sha256) | Pasa 1 de 1 |
| 4. Se restaura la guarda del parche, con el `Skip` | Omitida 1 de 1 |

El control vale también para la v4.3: el `Skip` y CT-PBT3A-12 no cambian entre las dos versiones del parche.

## 4. Un tropiezo de la sesión, sin efecto

El primer intento de la fase del GREEN (`out-run-green-try1.txt`, `out-green-try1/`) paró sin escribir nada: el guion no le pasaba a `apply_green_pbt4a.py` el checkout principal (`--repo`) del que lee los objetos git. Se corrigió y se repitió sobre una copia nueva. Además, el registro de la fase del GREEN lleva CR sueltos en los nombres de los filtros de UI, porque el guion leía la lista de una salida de Python con CRLF. La custodia los normaliza con nota en su manifiesto, y el guion ya está corregido para la fase del esqueleto.

## 5. Lo que no cubre

- **Las pruebas nuevas de PB-T4a.** Las escribe el Worker en el RED. Aquí solo están el esqueleto, el GREEN literal y las pruebas existentes.
- **El GREEN del Worker.** Si se aparta del literal del paquete, lo cubren la CI y la verificación.
- **La suite completa de UI.** No se ejecuta en local, por la regla vigente; solo corren sus 12 filtros del contrato.

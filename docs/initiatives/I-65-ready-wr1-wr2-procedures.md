# I-65 — READY: procedimientos completos de W-R1 (AutoCAD, OV-08 y HB-01..05) y W-R2 (RAM Elements) sobre el Candidato final

Documento de la unidad (Coordinator, 2026-10-29), por la disposición del Owner «R-D11 Acceptance / READY Final Preparation and
Validation Planning» ([texto literal](../automation/decisions/I-65-owner-disposition-rd11-acceptance-ready-final-preparation.original.txt);
decisiones [§111](../automation/decisions/I-65.md)).

- **Base reutilizada:**
  - W1/W2 de F4 ([W1](I-65-f4-w1-autocad-procedure.md), [W2](I-65-f4-w2-ram-procedure.md));
  - W3/W4 de F5 ([procedimientos](I-65-f5-w3-w4-procedures.md), [anexo operativo](I-65-f5-w3-w4-operational-annex.md));
  - [OV-08](I-65-ov08-procedure.md) (R-D9);
  - prueba del bundle del [contrato de R-D11](I-65-rd11-autoloader-gate.md) §6.
- **Reglas que se aplican:**
  - R-D2: reutilización de los volcados manuales;
  - R-D3: opción 0;
  - R-D4: Computer Use;
  - R-D5: opción 1 en (a), (b) y (c);
  - R-D6: opción 1.
- **Matriz y frontera del Candidato:** [I-65-final-candidate.md](I-65-final-candidate.md).

> **Estado: PREPARADO.**
>
> - **W-R1 EXECUTION = NOT YET AUTHORIZED. W-R2 EXECUTION = NOT YET AUTHORIZED.**
> - Cada ventana exige, en este orden:
>   1. `FINAL_CANDIDATE_SHA` declarado por el Owner;
>   2. la autorización específica de la ventana;
>   3. para HB-01..05, que esa autorización libere **expresamente** la instalación temporal en `ApplicationPlugins`.
> - Ninguna parte de este documento cambia el Freeze, ADR-0050, la tabla, las tolerancias ni los oráculos aprobados.

## 1. Identidad (se fija al declarar el Candidato; una diferencia es parada)

`<SHA>` es `FINAL_CANDIDATE_SHA` (40 hex) y `<s8>` sus 8 primeros caracteres. Los valores concretos (SHA-256, número de archivos,
ProductVersion) los fija la evidencia del Candidato; aquí se fijan **qué** se mide y **dónde**.

### 1.1 Producto y entradas (carpeta nueva `D:\r65-smoke\<s8>\`, creada por la evidencia del Candidato)

| Elemento | Ubicación | Regla |
|---|---|---|
| DLL Debug del Plugin y salida completa | `bin-Debug-net8.0-windows\` y su `output-manifest.txt` | ProductVersion `1.0.0+<SHA>`; SHA-256 de `RackCad.Plugin.dll` y manifiesto de la evidencia |
| Tabla junto al DLL | `bin-Debug-net8.0-windows\structural-analysis\ram-elements\section-material-map.json` | igual (SHA-256) a la fuente del checkout de compilación; esa fuente es el blob `d088e2a9…` salvo el fin de línea (A65-RD11-01 = A): `FC3D9146…B619` con LF o `B22F7D9D…2E7D` con CRLF. Se registra `git ls-files --eol` |
| Herramienta (comparador, `dump-script`, `inject`) | `ramtool\` y su manifiesto | compilada desde `<SHA>` |
| Bundle del Autoloader (HB) | `bundle\publish\RackCad.bundle\` y `bundle\inventory.txt` | publicado por `deploy\build-bundle.ps1 -Configuration Release` desde `<SHA>` con el árbol limpio; verificador y harness OK en la evidencia |
| Archivos de cargas | `inputs\`: `ref-cant-01.loads.json` (`25f48e7f…ae05`), `ov03a-double-face.loads.json` (`8e05faad…`), `ov03b-negative-y.loads.json` (`87e64230…`), `ov03c…`, `ov03d…` y `ov03e…` (los tres con los bytes de REF-CANT-01), `ov-i65-02.loads.json` (`d675d9f6…8ba4c`) | copias LF desde los blobs de `<SHA>` |
| Valores esperados del validador v2 | `expected-inputs.json` (cargas de REF-CANT-01) y `expected-inputs-ov02.json` (cargas de OV-02) | calculados con `smoke2_expected_inputs.ps1` y el `RackCad.Application.dll` de esa salida |
| Comprobación en seco | `w1_dryrun.ps1` (REF-CANT-01 `Valid`; OV-03 (a)-(e) con su código) y `f5_dryrun.ps1` + `f5_dryrun_compare.py` (OV-02, 6/6) con las DLL de esa salida | hecha en la evidencia del Candidato, antes de W-R1; no es evidencia de ninguna OV |

### 1.2 Aplicaciones y datos externos (se miden en cada ventana)

| Elemento | Referencia | Regla |
|---|---|---|
| AutoCAD 2025 | `acad.exe` R25.0.171.0.0 `2a75996f…6422` (última medida); perfil `<<Unnamed Profile>>` | se mide en la BASELINE y en el POST; si cambia la versión, la evidencia del dueño no es comparable (AGENTS.md) |
| Biblioteca de bloques | override de `%APPDATA%\RackCad\settings.json`: `D:\Base_de_datos_AutoCAD_V.0.dwg`, `b4ca2248…60e8` | ruta y SHA-256 en la BASELINE y en el POST (guía §7.1); no se abre ni se modifica |
| `RAMElements.exe` | 26.00.01.15 `7dcb4855…7970` | distinto = STOP |
| API `ramelements` | 0.1.0, árbol de 30 archivos `f71a017f…0a60` | distinto = STOP |
| Bases de RAM (R-D5 (a)) | `W.sec` `dffd9e00…b977`, `HSS_SQR.sec` `992ddac9…3df0`, `Steel.mat` `2a8b4239…9ce1`; árboles `ccdc2346…dc05` y `4cb742a5…a87e` | medidas con `w2_hostsnap.ps1` (método único) en el PRE y el POST de W-R2; distinto = STOP |
| Volcados manuales reutilizables (R-D2) | OV-01: `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\p2-R20261016T2147Z\dump-man\ram-dump.json` `c4a98fb0…ba23b`; OV-02: `D:\r65-smoke\bd1fcf8a\f5-R20261019T1750Z\dump-OV02-man\ram-dump.json` `12c89479…aef2` | SHA-256 medido antes de abrir RAM (§4.1, WR2-PR-04) |

### 1.3 Herramientas del Coordinator (fuera del producto; SHA-256 medido justo antes de usarlas; distinto = STOP sin sustituir)

| Uso | Herramienta | SHA-256 del archivo ejecutado | Copia versionada |
|---|---|---|---|
| BASELINE/POST del host de AutoCAD y de HB (R-D6 opción 1: ACL por `Get-Acl .Sddl`) | `wr_hostsnap.ps1` | `f867318c37330ebe61b658f54ab690959b44553d8d8ee36424d90f85a4832a54` | [`I-65-ready/tools/wr-hostsnap.ps1.txt`](../automation/evidence/I-65-ready/tools/wr-hostsnap.ps1.txt) |
| Nombres de valor del registro (clases del DIFF) | `smoke2_regdiff.ps1` | `353ecc74d023c4e35d2fe9089014b62185b956938d8afeb661ebf89872891aa5` | `I-65-f4/smoke2-51fc82f1/smoke2-regdiff.ps1.txt` |
| Inactividad y proceso en primer plano | `w1_idle.ps1` | `ac61c01bae9975e335b5632bb0ed6066f341f1005cb44af3fd30c06e690fa760` | `I-65-f4/w1-R20261017T0847Z/w1-idle.ps1.txt` |
| Valores esperados del validador | `smoke2_expected_inputs.ps1` | `0c84a4d0c4145b82505bb5c759cf03489c598da79794bfaba164bbfc0333f379` | `I-65-f4/smoke2-expected-inputs.ps1.txt` |
| Validador del paquete (16 comprobaciones) | `validate_smoke2_package_v2.py` | `1f56b8cba1a64f908c46eb5691930891c3dc0ef1d9bf8ed9c45f8e5f8bdd297a` | `I-65-f4/smoke2-51fc82f1/v11-correction/validate-smoke2-package-v2.py.txt` |
| Carga útil de REF-CANT-01 (117) | `p2_payload_vs_manual.py` | `a10d5b46938cc88ce4700e491c00a4beb6a90167c21c684ec2c57d8c45f959fa` | `I-65-f4/p2-prep/p2-payload-vs-manual.py.txt` |
| Carga útil de OV-02 (6/6) | `f5_dryrun_compare.py` | `39ac443a5a20368afbab217309c9e8afa27af77e7e7dcf97cdfd0aa950f85265` | `I-65-f5/candidate-bd1fcf8a/tools/f5-dryrun-compare.py.txt` |
| Comprobaciones en seco | `w1_dryrun.ps1`, `f5_dryrun.ps1` | `f5725f602ce16f051a996f86c36d854f6472556f0dd8de8660f8ad603627bfb1`, `7dc01426047e7bbed619dbd347e81e8df908cc86a14b6877fa8c9f8b8641aa3d` | `I-65-f4/w1-prep/w1-dryrun.ps1.txt`, `I-65-f5-prep/f5-dryrun.ps1.txt` |
| Preparación de RAM (solo lectura) | `w2_readiness.ps1` | `466f7a933042b34e423eb021cf99dd942c57a45d62391db306935d0297bab827` | `I-65-f4/w1-R20261017T0847Z/w2-readiness.ps1.txt` |
| PRE/POST del host de RAM (R-D5 (a)) | `w2_hostsnap.ps1` **sin modificar** | `6904a43965de69b33b78c304cd57d1958b31b2018121906cf22d034724ff5485` | `I-65-f4/w2-prep/w2-hostsnap.ps1.txt` |
| Custodia de sesión (R-D3 opción 0) | `session_custody.ps1` | `5487544aec2685c8554145b46d68bd4fe8fe9545e0b1628deee84d7fc080491b` | `I-65-f5-prep/session-custody.ps1.txt` |
| Contraste lectura de vuelta ↔ volcado | `readback_dump_contrast.py` | `0790930297d221ddefeadaef8e04611858b2e81446717155e754d34155114bb1` | `I-65-f5-prep/readback-dump-contrast.py.txt` |
| Comprobación mecánica de privacidad antes de versionar | `secrets_scan.py` | `bcccaf9123923eca9f998c078ba3850d6d934c133d3d170ce1b969abe4fa9fb3` | `I-65-f5/candidate-bd1fcf8a/tools/secrets-scan.py.txt` |

**Cómo se comprueban las identidades de §1.1 en WR1-01 y WR1-16 (A65-FC-05):**

- `wr_hostsnap.ps1` (B-09) cubre la salida, la tabla, las cargas y `expected-inputs.json`.
- La herramienta: manifiesto de `ramtool\` recalculado con la función `Manifest` de `fc-identity.ps1` (copia versionada en la evidencia
  del Candidato) e igual al custodiado.
- El bundle: `verify-bundle.ps1 -BaselineInventoryPath` (HB-02 (1)).
- `expected-inputs-ov02.json`: `Get-FileHash`, igual al de la evidencia del Candidato.

Con la tabla en `w/lf` (medida en la evidencia del Candidato), la fuente del checkout es igual al blob. Por eso O8-PR-02 («igual al
blob») y esta sección («fuente del checkout de compilación») coinciden. Con un checkout CRLF rige esta sección (A65-RD11-01 = A;
A65-FC-12).

Los scripts del producto que usa HB (`deploy\verify-bundle.ps1` e `install-bundle.ps1`) se ejecutan desde el worktree. Antes de usarlos
se comprueba que `git diff --name-only <SHA>..HEAD -- deploy assets src` está vacío: los commits posteriores al Candidato solo pueden
ser documentales.

## 2. Reglas comunes a las dos ventanas

- **Coordinación de host:**
  - confirmación **expresa** del Coordinator de I-64 en su chat (un acuse de recibo no basta);
  - aviso a I-52;
  - I-62 sin operación de pantalla;
  - avisos de apertura y de cierre.
  - C-5 = A no afecta a esta coordinación: se refiere a notificaciones a terceros.
- **Estado del equipo:**
  - AutoCAD y RAM cerrados al empezar;
  - inactividad del usuario de al menos 120 s justo antes de empezar;
  - la app de Claude en primer plano (`w1_idle.ps1`).
- **Operador:** Claude con Computer Use. El tecleo y la navegación siguen las reglas de W1/W2:
  - las rutas de carpeta nueva se teclean con `\`, en trozos de 10 caracteres como máximo y nunca por el portapapeles;
  - en los diálogos de archivo se navega con doble clic en el icono de cada carpeta.
- **Carpeta de corrida:** `D:\r65-smoke\<s8>\wr1-<RunId>\`, nueva, con `<RunId>` = `R<AAAAMMDD>T<HHMM>Z` de la BASELINE de W-R1. W-R2
  usa la **misma** carpeta (como W2 usó la de W1), así que `w2_hostsnap.ps1` mide sin modificarse los paquetes de W-R1.
- **Privacidad (§90):**
  - capturas recortadas a la zona técnica y redactadas;
  - rutas con `<usuario>`;
  - nunca credenciales, tokens ni datos de terceros;
  - comprobación mecánica (`secrets_scan.py`) y revisión visual antes de versionar;
  - los originales con datos identificables quedan en local con su SHA-256 custodiado.
- **Prohibido:**
  - guardar modelos de RAM;
  - guardar un DWG fuera de la carpeta de corrida (OV-08 es el único guardado autorizado);
  - cambiar `TRUSTEDPATHS`, `SECURELOAD`, perfil o ACL;
  - escribir en `ApplicationPlugins` fuera de HB;
  - editar el registro;
  - analizar en RAM;
  - cambiar código, paquetes, tabla o tolerancias.
- **Intentos:** un intento es una ejecución de `RACKRAMPAQUETE` o de un script en RAM. Corregir datos en la ventana del configurador
  antes de ejecutar no es un intento, y se registra. **Ningún intento se repite sin autorización.**

## 3. W-R1 — AutoCAD (tres sesiones de AutoCAD: A con NETLOAD, B = OV-08 con NETLOAD y C = HB sin NETLOAD)

### 3.1 Condiciones de entrada (si falta una, no se abre AutoCAD)

| # | Condición |
|---|---|
| WR1-PR-01 | `FINAL_CANDIDATE_SHA` declarado por el Owner con su bloque de la guía §7.1 completo (Core y UI Full locales, builds, CI exacta con `ui-tests`) |
| WR1-PR-02 | Autorización específica de W-R1, que libera expresamente HB-01..05 (instalación temporal en `ApplicationPlugins` y desinstalación) y el guardado, cierre y reapertura del DWG desechable de OV-08 |
| WR1-PR-03 | Este procedimiento versionado en `origin` |
| WR1-PR-04 | Coordinación de host (§2) |
| WR1-PR-05 | `acad.exe` = 0 y `RAMElements.exe` = 0; nadie usando el equipo (§2) |
| WR1-PR-06 | BASELINE limpia con `wr_hostsnap.ps1`: perfil `<<Unnamed Profile>>`; `TRUSTEDPATHS` con el número de entradas y la huella de la última medida; `SECURELOAD` 1; `acad.exe`; exportaciones del registro; **las cuatro raíces de `ApplicationPlugins` sin RackCad** (la del usuario, vacía); 0 variables `RackCad`; ACL |
| WR1-PR-07 | Identidades de §1.1 y §1.2 (DLL, manifiesto recalculado, tabla, herramienta, bundle con su inventario, cargas, valores esperados, biblioteca) |
| WR1-PR-08 | Herramientas de §1.3 con su SHA-256 |
| WR1-PR-09 | Ninguna carpeta de paquete o de inyección de §3.2 existe |
| WR1-PR-10 | Instantánea K1 de los checkouts |
| WR1-PR-11 | Comprobaciones en seco de §1.1 hechas y conformes en la evidencia del Candidato |

### 3.2 Carpetas (dentro de `D:\r65-smoke\<s8>\wr1-<RunId>\`)

| Carpeta | Uso | Al final |
|---|---|---|
| `baseline\`, `post\` | `wr_hostsnap.ps1` de la ventana | existen |
| `pkg-OV01`, `pkg-OV02`, `pkg-OV04` | paquetes válidos de la sesión A | 3 archivos cada una |
| `pkg-OV03a` … `pkg-OV03e` | casos de OV-03 | **no existen** |
| `ov08\` | DWG desechable `ov08-ref.dwg`, su copia `hb-ref.dwg` (entrada de HB-04 (a)), `pkg-A` y `pkg-B` de OV-08 | existen; los DWG solo aquí |
| `hb\` | `baseline\`, `post\`, `pkg-HB`, `pkg-HB2` y registros de HB | existen; cada paquete con 3 archivos |
| `inj-OV05`, `inj-OV06`, `inj-OV07a`, `inj-OV07b`, `dump-OV01-gen`, `dump-OV02-gen` | preparación de W-R2 (parte D) | existen; sin `ram-readback.json` ni `ram-dump.json` |
| `captures\` | capturas | originales en local |

### 3.3 Pasos

**Parte A — sesión 1 (NETLOAD): OV-01, OV-02, OV-04 y OV-03**

| Paso | Acción | Oráculo |
|---|---|---|
| WR1-01 | Crear la carpeta de corrida; K1; `wr_hostsnap.ps1 -Phase baseline -Run <corrida> -Base D:\r65-smoke\<s8> -Packages pkg-OV01,pkg-OV02,pkg-OV04,pkg-OV03a,pkg-OV03b,pkg-OV03c,pkg-OV03d,pkg-OV03e` | WR1-PR-06 |
| WR1-02 | Aviso de apertura a I-64, I-52 e I-62 | — |
| WR1-03 | Abrir AutoCAD 2025 (una sola instancia) con un dibujo nuevo desechable | título `Drawing1.dwg` |
| WR1-04 | SHA-256 del DLL justo antes de cargar. `NETLOAD` navegando hasta `D:\r65-smoke\<s8>\bin-Debug-net8.0-windows\RackCad.Plugin.dll`; en el aviso estándar «Security - Unsigned Executable File», **solo «Load Once»**. Después, lista de módulos de `acad.exe` | un único `RackCad.Plugin.dll`, desde esa carpeta y con el SHA-256 declarado |
| WR1-05 | REF-CANT-01 con `RACKCANTILEVER`, exactamente como en [W1 §4.1](I-65-f4-w1-autocad-procedure.md); «Actualizar»; «Insertar frontal» en un punto libre | resumen «2 estaciones · 1 intervalos · altura común 90.5 in …» |
| WR1-06 | **OV-01:** `RACKRAMPAQUETE` → rack REF-CANT-01 → estación `0` → `D:\r65-smoke\<s8>\inputs\ref-cant-01.loads.json` → `<corrida>\pkg-OV01` | `Valid`, la huella y tres «Archivo escrito», sin diagnósticos. Se anota la **huella de entrada H0**: es `analysisInputFingerprint` del `manifest.json` de `pkg-OV01` (A65-FC-11) |
| WR1-07 | Validador v2: `python -I validate_smoke2_package_v2.py <corrida>\pkg-OV01 D:\r65-smoke\<s8>\expected-inputs.json <carpeta tecleada> <informe nuevo>`; contraste de la carga útil: `p2_payload_vs_manual.py` | **16/16**; carga útil **117/117**; huella de la consola = manifiesto = script (como W1-07). Esta igualdad se exige en **cada** paquete válido de W-R1: `pkg-OV02`, `pkg-OV04`, `pkg-A`, `pkg-B`, `pkg-HB` y `pkg-HB2`. H1, H2 y la huella de HB se toman de sus manifiestos |
| WR1-08 | **OV-02:** `RACKRAMPAQUETE` → el mismo rack → estación `0` → `inputs\ov-i65-02.loads.json` → `<corrida>\pkg-OV02`; validador v2 con `expected-inputs-ov02.json`; `f5_dryrun_compare.py` sobre la línea `PAYLOAD` de su script | `Valid`, sin diagnósticos; **16/16**; **6/6** (≤ 1e-9). Q2 al 99.99999999999997 % es diferencia de representación (CD-I65-F5-08) |
| WR1-09 | **OV-04:** `RACKRAMPAQUETE` → el mismo rack → estación `0` → `ref-cant-01.loads.json` → `<corrida>\pkg-OV04`; validador v2 | `Valid`; **16/16**; huella = H0 |
| WR1-10 | **OV-03 (a)-(e)** en este orden, cada uno según [W1 §4.2](I-65-f4-w1-autocad-procedure.md) (REF-CANT-01 con un único cambio, su archivo de cargas y su carpeta `pkg-OV03a`…`e`); **un único intento** cada uno | `Unsupported` con **exactamente** los códigos de W1 §4.2; `Test-Path` de la carpeta = `False`; 0 hermanos `*.tmp-*` |
| WR1-11 | Cerrar AutoCAD **sin guardar** | `acad.exe` = 0 |

El caso «fila sin verificar» de OV-03 no se ejecuta en AutoCAD: lo cubren pruebas de fixture del Candidato (CD-I65-F4-186).

**Parte B — sesión 2 (NETLOAD): OV-I65-08**

| Paso | Acción | Oráculo |
|---|---|---|
| WR1-12 | O8-01..O8-13 del [procedimiento de OV-08](I-65-ov08-procedure.md), aprobado por R-D9 y sin cambios, con estas concreciones: DLL de §1.1; carpeta de corrida `<corrida>\ov08\`; DWG `ov08-ref.dwg` guardado **solo** ahí; cargas `inputs\ref-cant-01.loads.json`; `pkg-A` y `pkg-B` dentro de `ov08\`. En O8-01, `RACKAYUDA` **no** incluye `RACKRAMPAQUETE` (R-D8 = A). En O8-11, si no hay un rack no Cantilever en el dibujo, se inserta uno con `RACKSELECTIVO` y sus valores por omisión. **En O8-07, entre «cerrar» y «reabrir»** (el DWG cerrado y sin bloqueo), fuera de AutoCAD: comprobar que no existen `ov08\ov08-ref.dwl` ni `ov08\ov08-ref.dwl2` (si existen, el DWG sigue abierto: no se copia); `Copy-Item` de `ov08\ov08-ref.dwg` a `ov08\hb-ref.dwg`; SHA-256 de los dos, que deben ser iguales y se anotan. Es la entrada de HB-04 (a). Si OV-08 se detiene antes de O8-07, no hay copia: HB-04 (a) queda NOT_EVALUATED con la razón, y HB-04 (b) se ejecuta igual (A65-FC-04) | los oráculos de O8-01..13. En particular, **H2 = H1** y la huella de fuente es igual en los dos manifiestos (O8-09). H1 no se compara con H0: la huella de entrada incluye la identidad del rack (`rackId` y huella de fuente en el modelo canónico), y el de la sesión B es otro rack |
| WR1-13 | Legado (guía §5.5): si el Owner no aporta un DWG Cantilever anterior representativo, el punto queda **NOT_EVALUATED** con la razón | no se declara validado |

O8-13 termina con AutoCAD cerrado (`acad.exe` = 0). Los cambios posteriores al último guardado no se guardan.

**Parte C — sesión 3 (sin NETLOAD): HB-01..05, prueba real del bundle del Autoloader**

Esta parte responde a las nueve condiciones del Owner (§111): BASELINE previa (1); sin instalación previa (2); solo el bundle del
Candidato (3); carga desde el Autoloader (4); `RACKRAMPAQUETE` encuentra la tabla y da un paquete válido (5); oráculos de HB (6); cierre
de AutoCAD (7); desinstalación (8); POST y restauración exacta (9). No se tocan `TRUSTEDPATHS` ni `SECURELOAD`.

| Paso | Acción | Oráculo |
|---|---|---|
| HB-01 (cond. 1-2) | Con `acad.exe` = 0: `wr_hostsnap.ps1 -Phase baseline -Run <corrida>\hb -Base D:\r65-smoke\<s8> -Packages pkg-HB,pkg-HB2`. Listado recursivo con `-Force` de `%APPDATA%\Autodesk\ApplicationPlugins` y de las otras tres raíces. Recuento de referencias `RackCad.bundle` en las exportaciones `.reg` de la BASELINE. Comprobación booleana, sin imprimir rutas, de si `TRUSTEDPATHS` cubre `%APPDATA%\Autodesk\ApplicationPlugins` | raíz del usuario **vacía**; ninguna raíz con `RackCad*` ni `.RackCad.bundle.*`; referencias = 0. **Una instalación existente = STOP** (no se sobrescribe) |
| HB-02 (cond. 3) | (1) `verify-bundle.ps1 -BundlePath D:\r65-smoke\<s8>\bundle\publish\RackCad.bundle -PublishDir D:\r65-smoke\<s8>\bundle\publish -BaselineInventoryPath D:\r65-smoke\<s8>\bundle\inventory.txt`. (2) `install-bundle.ps1 -SourceBundlePath D:\r65-smoke\<s8>\bundle\publish\RackCad.bundle` (sin `-Build`; staging, respaldo y sustitución verificada). (3) `verify-bundle.ps1` sobre `%APPDATA%\Autodesk\ApplicationPlugins\RackCad.bundle` con los mismos `-PublishDir` y `-BaselineInventoryPath`. (4) `git -C <worktree> ls-files --eol assets/structural-analysis/ram-elements/section-material-map.json` y SHA-256 de esa fuente | (1) y (3) `OK: bundle verificado (160 comprobaciones)`: 158 del bundle más 2 de la comparación de reproducibilidad que añade `-BaselineInventoryPath` (`Compare-Inventory`). Es la cifra medida en seco con el paso (1) sobre el bundle del Candidato propuesto (A65-FC-01). Además, la línea «Reproducibilidad: el inventario y los hashes coinciden con el base.». La tabla instalada es igual (SHA-256) a la fuente del checkout de compilación (A65-RD11-01 = A). La raíz del usuario solo contiene `RackCad.bundle`, sin `.RackCad.bundle.stage-*` ni `.backup-*`. **Si `install-bundle.ps1` falla:** STOP; se deja actuar a su rollback y se pasa a HB-05 |
| HB-03 (cond. 4) | Abrir AutoCAD 2025 **sin** NETLOAD, con un dibujo nuevo desechable. Si aparece el aviso estándar «Security - Unsigned Executable File» para `…\ApplicationPlugins\RackCad.bundle\Contents\RackCad.Plugin.dll`: **solo «Load Once»**, nunca «Always Load». Después, lista de módulos de `acad.exe` e historial de comandos | **un único** `RackCad.Plugin.dll`, desde `%APPDATA%\Autodesk\ApplicationPlugins\RackCad.bundle\Contents\` y con el SHA-256 del DLL del bundle; ningún `NETLOAD` en la sesión. Se registra si apareció el aviso: si no aparece y HB-01 dijo que `TRUSTEDPATHS` no cubre la ruta, es un hallazgo que se informa |
| HB-04 (cond. 5) | (a) SHA-256 de `ov08\hb-ref.dwg` igual al anotado en WR1-12 (también si W-R1 se parte en W-R1a y W-R1b). `OPEN` de `ov08\hb-ref.dwg`, navegando por carpetas; `RACKRAMPAQUETE` seleccionando **la misma vista** que en O8-06 y O8-09 (la vista principal del rack) → estación `0` → `inputs\ref-cant-01.loads.json` → `<corrida>\hb\pkg-HB`; validador v2. (b) Dibujo nuevo; `RACKCANTILEVER`: REF-CANT-01 (W1 §4.1), «Actualizar» e «Insertar frontal»; `RACKRAMPAQUETE` → estación `0` → la misma carga → `<corrida>\hb\pkg-HB2`; validador v2 y `p2_payload_vs_manual.py`. Los dos dibujos se cierran sin guardar; SHA-256 de `hb-ref.dwg` sin cambios | (a) `Valid`, tres «Archivo escrito», sin diagnósticos (la tabla se encontró junto al DLL del bundle); **16/16**; **huella de entrada = H2 = H1 de OV-08**: es la que da el DLL de compilación del mismo SHA para el mismo rack (oráculo aprobado, §6 del contrato de R-D11; interpretación en la nota de abajo). (b) `Valid`, sin diagnósticos; **16/16**; carga útil **117/117**. Su huella se registra: es otro rack, así que difiere de H0 por diseño |
| HB-05a (cond. 7) | Cerrar AutoCAD **sin guardar** | `acad.exe` = 0 |
| HB-05b (cond. 8) | Inventario del `RackCad.bundle` instalado y comparación con el de HB-02 (3). Después, `Remove-Item -LiteralPath "$env:APPDATA\Autodesk\ApplicationPlugins\RackCad.bundle" -Recurse -Force`, tras comprobar esa ruta literal. Listado de la raíz | inventario igual; raíz del usuario **vacía**, igual al listado de HB-01. Si el inventario difiere (archivos escritos por la ejecución): se **copian** las diferencias a `<corrida>\hb\` como evidencia, se desinstala igualmente y se registra como hallazgo (STOP tras la restauración) |
| HB-05c (cond. 9) | `wr_hostsnap.ps1 -Phase post -Run <corrida>\hb …`; `smoke2_regdiff.ps1` entre las exportaciones de `hb\baseline` y `hb\post`; recuento de referencias `RackCad.bundle` en las exportaciones del POST | **restauración exacta de lo protegido** (§3.4); referencias = las de HB-01 (0). La condición 9 del Owner se lee así: «exacta» se aplica a la clase protegida, y la clase «informar» (estado de interfaz y listas de recientes que AutoCAD escribe en cualquier sesión) se registra (A65-FC-03). Un residuo del Autoloader en `Applications`, `InstalledApplications` o `Loaded`, o cualquier referencia a `RackCad.bundle` en el registro, es protegido: STOP. Se registra y **no** se corrige el registro a mano: decide el Owner |

**Nota sobre el oráculo de huella de HB-04 (interpretación del Coordinator, decisiones §112).**

- **Oráculo:** el aprobado en el contrato de R-D11 (§6) es «huella de entrada igual a la del DLL de compilación del mismo SHA».
- **Por qué no basta un rack nuevo:**
  - `StructuralFingerprints.AnalysisInput` incluye el modelo canónico;
  - el modelo canónico incluye `rackId` y la huella de fuente (`StructuralModelCanonicalizer`);
  - un REF-CANT-01 construido de nuevo tiene otro GUID, así que ningún DLL reproduciría su huella.
- **Lectura adoptada:** el oráculo solo tiene sentido para el **mismo rack**. HB-04 (a) lo evalúa así, sobre la copia byte a byte del
  DWG de OV-08, cuyo rack ya recibió del DLL Debug del mismo SHA la huella H1 = H2.
- **El paso aprobado se cumple igual:** construir REF-CANT-01 con `RACKCANTILEVER` y exportarlo es HB-04 (b), con el oráculo de
  contenido 117/117, que no depende del GUID.
- **Lo que no cambia:** ningún oráculo ni paso aprobado se suprime ni se relaja. El Owner puede pedir otra lectura antes de declarar el
  Candidato.

**Parte D — sin AutoCAD: preparación de W-R2**

| Paso | Acción | Oráculo |
|---|---|---|
| WR1-14 | Inyecciones desde `pkg-OV01` con la herramienta del Candidato: `RackCad.StructuralAnalysis.RamTool.exe inject --package <corrida>\pkg-OV01 --fault <tipo> --out <corrida>\inj-…`, con `unknown-section` → `inj-OV05`, `inverted-axes` → `inj-OV06`, `wrong-version` → `inj-OV07a` y `missing-core-function` → `inj-OV07b`; sin `--embed-folder` | cuatro carpetas nuevas con su marca de inyección; SHA-256 de los tres archivos de `pkg-OV01` sin cambios |
| WR1-15 | Scripts de volcado: `dump-script --out <corrida>\dump-OV01-gen --adapter-version 1.0.0` y lo mismo para `dump-OV02-gen`. Comparación con la plantilla de P2 (`manual-rackcad-ram-dump.py.txt`, `af262489…`) | solo cambia la línea de la carpeta (`dumpFolder`) |
| WR1-16 | POST de la ventana: `wr_hostsnap.ps1 -Phase post -Run <corrida> …`; `smoke2_regdiff.ps1`; clasificación del DIFF (PROHIBIDO / INFORMAR / LIMPIO) frente a la BASELINE de WR1-01; K2 | sin PROHIBIDO; K1/K2 sin cambios |
| WR1-17 | Aviso de cierre a I-64, I-52 e I-62 con `acad.exe` = 0, el resultado del DIFF y el estado de `ApplicationPlugins`; custodia (§3.6) | — |

### 3.4 Clases del DIFF (BASELINE ↔ POST de la ventana y de HB)

| Clase | Elementos | Regla |
|---|---|---|
| **Protegido** (debe ser igual) | B-01 (`acad` = 0 y `ram` = 0 al medir); B-02 perfil; B-03 `TRUSTEDPATHS` (entradas y huella) y `SECURELOAD`; B-04 `Applications`, `InstalledApplications`, `Loaded` y `Applications` de HKLM; B-05 `acad.exe`; **B-06 las cuatro raíces de `ApplicationPlugins`** (recuento, huella de nombres y `rackcad` = 0); B-07 variables; B-08 ACL; B-09 DLL, manifiesto, tabla, cargas y valores esperados; X-04 biblioteca; referencias `RackCad.bundle` en el registro | cualquier diferencia = **PROHIBIDO** → STOP |
| **Informar** | `Profiles` y `FixedProfile` con solo los nombres de valor ya vistos en los smokes y en W1/W3 (MRU de NETLOAD, posiciones de ventanas, `VIEWBACKSTATUS`); X-01 `%APPDATA%\RackCad` (registro del producto); X-02 existencia de paquetes | se registran con `smoke2_regdiff.ps1`; un nombre de valor nuevo que no sea de interfaz se eleva a protegido |
| **Limpio** | sin diferencias | — |

### 3.5 Condiciones de aceptación de W-R1

| # | Condición |
|---|---|
| WR1-AC-01 | El DLL cargado en las sesiones A y B es el del Candidato: mismo SHA-256 en la BASELINE, justo antes de cargar, en la lista de módulos y en el POST |
| WR1-AC-02 | OV-01, OV-02 y OV-04: `Valid`, paquetes completos y validador v2 16/16; carga útil 117/117 (OV-01) y 6/6 (OV-02); huella de OV-01 = huella de OV-04 = H0 |
| WR1-AC-03 | OV-03 (a)-(e): el rechazo esperado y ninguna carpeta |
| WR1-AC-04 | OV-08: O8-01..13 conformes, H2 = H1 y el punto de legado conforme a WR1-13 |
| WR1-AC-05 | HB-01..05: cada oráculo de la parte C; el módulo cargado desde el Autoloader; `pkg-HB` `Valid` 16/16 con huella = H2 = H1 de OV-08; `pkg-HB2` `Valid` 16/16 con carga útil 117/117; restauración exacta de lo protegido |
| WR1-AC-06 | AutoCAD cerrado sin guardar (salvo el DWG de OV-08 dentro de `ov08\`); RAM nunca iniciado |
| WR1-AC-07 | DIFF de la ventana sin PROHIBIDO; K1/K2 sin cambios |
| WR1-AC-08 | Inyecciones y scripts de volcado preparados (WR1-14, WR1-15) |

### 3.6 Evidencia que se custodia

**Capturas recortadas:**

- título del dibujo;
- NETLOAD y aviso «Load Once»;
- ventanas de cada línea;
- consola de cada `RACKRAMPAQUETE`;
- pasos de OV-08 (incluidos el guardado y la reapertura);
- aviso de HB-03, si aparece;
- cierre sin guardar.

**Datos y registros:**

- texto literal de cada consola;
- listas de módulos;
- informes del validador v2 y de los contrastes de carga útil;
- `manifest.json` de cada paquete (redactados);
- `Test-Path` de OV-03;
- ficha de OV-08 con el formato de la guía §7;
- salidas de `verify-bundle.ps1`, `install-bundle.ps1` y de los listados de `ApplicationPlugins`;
- resúmenes de BASELINE y POST de la ventana y de HB;
- DIFF del registro;
- K1/K2;
- horas UTC.

Las exportaciones del registro, el DWG y los originales de las capturas quedan en local con su SHA-256.

### 3.7 Paradas de W-R1

| # | Parada |
|---|---|
| WR1-ST-01 | Falta una condición de §3.1 (no se abre AutoCAD) |
| WR1-ST-02 | Otra `acad.exe`; una operación de host de I-52, I-62 o I-64; el usuario empieza a usar el equipo |
| WR1-ST-03 | Un aviso de seguridad distinto del estándar, una petición de confianza o de licencia, o un diálogo modal no previsto |
| WR1-ST-04 | SHA-256 distinto (DLL, tabla, cargas, bundle, herramienta) o un módulo cargado desde otra carpeta; **dos `RackCad.Plugin.dll` en el mismo proceso** |
| WR1-ST-05 | OV-01, OV-02 u OV-04 con un resultado distinto de `Valid`, con diagnósticos, un paquete parcial o el validador por debajo de 16/16: se captura, se cierra sin guardar y **no se reintenta** |
| WR1-ST-06 | Un caso de OV-03 con otro resultado: se registra literal, sin reintento ni sustitución. Si se escribió un paquete, STOP de la ventana |
| WR1-ST-07 | OV-08: pérdida de datos, error silencioso, H2 ≠ H1, o escritura fuera de `ov08\` |
| WR1-ST-08 | HB: una instalación existente en HB-01; fallo del verificador o del instalador; el módulo no cargado desde el Autoloader; `RACKRAMPAQUETE` distinto de `Valid`; en (a), una huella distinta de H2; una diferencia protegida en HB-05c. **Toda parada de HB después de HB-02 pasa antes por HB-05a y HB-05b** (cerrar AutoCAD y desinstalar), salvo que desinstalar exija otra cosa que borrar la carpeta instalada |
| WR1-ST-09 | Pegado o renombrado accidental en un diálogo: Ctrl+Z inmediato, registro y comprobación en disco. Si alteró algo fuera del dibujo desechable, STOP |
| WR1-ST-10 | DIFF con PROHIBIDO; necesidad de RAM, de cambiar la configuración persistente, de editar el registro o de tocar código |
| WR1-ST-11 | Fin de la ventana autorizada |

Toda parada lleva registro literal, aviso de cierre y decisión del Owner. Ninguna parada se resuelve cambiando código, paquetes ni tabla.

### 3.8 Duración (presupuesto de host)

| Parte | Duración |
|---|---|
| A | 75-90 min |
| B | 45-60 min |
| C | 40-55 min |
| D | 15-20 min |
| **Total** | **2 h 55 min - 3 h 45 min**, con la reserva hasta **4 h** |

El Owner puede autorizar W-R1 en dos ventanas: **W-R1a** con A, B y D, y **W-R1b** con C. En ese caso, cada una lleva su BASELINE y su
POST.

## 4. W-R2 — RAM Elements (OV-01, OV-02, OV-04..07; con R-D2)

### 4.1 Condiciones de entrada (si falta una, no se abre RAM)

| # | Condición |
|---|---|
| WR2-PR-01 | W-R1 satisfactoria y aceptada por el Owner; autorización específica de W-R2 (incluye a Claude con Computer Use como operador, R-D4) |
| WR2-PR-02 | Coordinación de host (§2); nadie usando el equipo |
| WR2-PR-03 | `RAMElements.exe` = 0 y `acad.exe` = 0; `w2_readiness.ps1` conforme |
| WR2-PR-04 | **Condiciones de R-D2**, comprobadas antes de abrir RAM: RAM `7dcb4855…`; `ramelements` 0.1.0 `f71a017f…`; bases iguales a §1.2 con el método único (R-D5 (a)); plantilla de volcado = la de P2 salvo `dumpFolder` (WR1-15); especificaciones REF-CANT-01, hoja de OV-02 (blob git `6dc0d8da…`, SHA-256 de su contenido `b15b9f9e…`; A65-FC-06) y cargas `d675d9f6…` sin cambios en `<SHA>`; volcados manuales `c4a98fb0…` y `12c89479…` con su SHA-256. **Si una no se cumple, R-D2 no aplica**: no se abre RAM y se pide al Owner una ventana con construcción manual nueva (R-D4, como W4, con unas 2 h más) |
| WR2-PR-05 | Paquetes, inyecciones y scripts de W-R1 con sus SHA-256; ningún `ram-readback.json` ni `ram-dump.json` en las carpetas de destino |
| WR2-PR-06 | `w2_hostsnap.ps1 -Phase pre -Run <corrida de W-R1>`: copia de `Config.ini`. Medición aparte de `pkg-OV02`, `dump-OV02-gen` y del volcado manual de OV-02, porque no están en la lista fija de esa herramienta. K1 |

### 4.2 Pasos

R-D5 (c): **un proceso de RAM nuevo** para la ventana. Ante un STOP de licencia, se cierra RAM y se comprueba `ram` = 0 antes de terminar.

| Paso | Acción | Oráculo / evidencia |
|---|---|---|
| WR2-01 | Abrir RAM Elements (una instancia); modelo **nuevo, sin título y vacío**; `session_custody.ps1 -Phase session-start -Log <corrida>\wr2-session.jsonl` | una sola `RAMElements.exe`; PID e inicio registrados; licencia en la barra de estado |
| WR2-02 | **OV-01:** `pre-run` del script de `pkg-OV01`; «Ejecutar archivo…» (W2-05); `post-run` | consola `READBACK OK`, sin líneas ` - `, `AnalysisInputFingerprint` = H0; `ram-readback.json` `READBACK OK` |
| WR2-03 | Misma sesión y mismo modelo: `pre-run` y ejecución de `dump-OV01-gen\rackcad-ram-dump.py`; `post-run` | `DUMP OK`; `combinations` [] |
| WR2-04 | Descartar sin guardar (File > New → No) | modelo nuevo vacío |
| WR2-05 | **OV-02:** igual que WR2-02 y WR2-03 con `pkg-OV02` y `dump-OV02-gen`; descartar | `READBACK OK` con la huella de `pkg-OV02`; `DUMP OK`; Q2 según [W3/W4 §4.3](I-65-f5-w3-w4-procedures.md) |
| WR2-06 | **OV-04:** modelo nuevo con **un nudo de prueba** tecleado en (0, 0, 0) sin apoyos (captura de la rejilla); `pkg-OV04` | `FAILED — DISCARD MODEL` con ` - gate: the model has 1 nodes`; el modelo conserva exactamente su contenido inicial |
| WR2-07 | **OV-05, OV-06, OV-07a y OV-07b**, cada una sobre un modelo nuevo vacío, con descarte entre casos | los resultados de [W2 §5](I-65-f4-w2-ram-procedure.md) (OV-05 `FAILED` con la excepción de la sección; OV-06 diferencia de ejes locales; OV-07a/b `gate:` sin abrir el modelo y el modelo vacío) |
| WR2-08 | Cerrar RAM → «¿Desea salvarlo?» → **No**; `session-end` | `RAMElements.exe` = 0 |
| WR2-09 | **Después de cerrar RAM (R-D5 (b)):** `readback_dump_contrast.py` para OV-01 (`pkg-OV01\ram-readback.json` ↔ `dump-OV01-gen\ram-dump.json`) y para OV-02 | **IGUAL** en ambos |
| WR2-10 | Comparador OV-01 (R-D2): `compare --package pkg-OV01 --generated dump-OV01-gen\ram-dump.json --manual <volcado c4a98fb0> --report <nuevo>`, **sin** `--*-unread` (CMB-3) | **PASS (0)**; `inputs.manualDumpSha256` = `c4a98fb0…`; combinaciones = 0 en ambos; propiedades NOT_EVALUATED; DIF-01 y el peso propio informativos |
| WR2-11 | Comparador OV-02 (R-D2): igual con `pkg-OV02`, `dump-OV02-gen` y el volcado `12c89479…` | **PASS (0)**; fila `LOADS` en PASS (valor 0.1 %, posición 0.02 in, dirección exacta); `manualDumpSha256` = `12c89479…` |
| WR2-12 | Control de OV-05 fuera de RAM: `compare --package inj-OV05 --generated dump-OV01-gen\ram-dump.json --manual <volcado c4a98fb0> --report <nuevo>` | **REJECTED (2)** por la marca de inyección |
| WR2-13 | Cadena de custodia: inicio de RAM ≤ creación de cada lectura de vuelta ≤ creación de su volcado ≤ cierre, con el mismo PID | coherente (evidencia operativa, **no** criptográfica: R-D3) |
| WR2-14 | `w2_hostsnap.ps1 -Phase post`; mediciones aparte de WR2-PR-06; diferencias de `Config.ini` (solo nombres de clave); K2; aviso de cierre | identidades y bases iguales; `Config.ini` solo con estado de interfaz (se conserva, CD-I65-F4-73); K1/K2 sin cambios |

**Ninguna lectura de vuelta ni volcado del generado de ventanas anteriores** se usa como resultado: R-D2 reutiliza **solo** los volcados
de los modelos manuales.

### 4.3 Condiciones de aceptación de W-R2

| # | Condición |
|---|---|
| WR2-AC-01 | OV-01 PASS: `READBACK OK` con H0, `DUMP OK`, contraste IGUAL y comparador PASS (0) con `c4a98fb0…` |
| WR2-AC-02 | OV-02 PASS: `READBACK OK`, `DUMP OK`, contraste IGUAL y comparador PASS (0) con `LOADS` en PASS y `12c89479…`; Q2 reproducida |
| WR2-AC-03 | OV-04..07: el rechazo esperado de cada caso; control de OV-05 REJECTED (2) |
| WR2-AC-04 | Ningún modelo guardado; sin análisis; un solo proceso de RAM; cadena de custodia coherente |
| WR2-AC-05 | Identidades y bases iguales en el PRE y el POST. Los **tres archivos originales** de cada paquete e inyección también son iguales; S-07 cambia por diseño, porque se añaden `ram-readback.json` y `ram-dump.json` (A65-FC-13). K1/K2 sin cambios |

### 4.4 Paradas de W-R2

| # | Parada |
|---|---|
| WR2-ST-01 | Falta una condición de §4.1 (no se abre RAM) |
| WR2-ST-02 | Identidad distinta; licencia no disponible (R-D5 (c): cerrar RAM y terminar); aviso de seguridad o diálogo no previsto |
| WR2-ST-03 | Script con otra ruta u otro SHA-256 en «Ejecutar archivo…» (no se ejecuta) |
| WR2-ST-04 | Un `ram-readback.json` o `ram-dump.json` que ya existía |
| WR2-ST-05 | Resultado distinto del esperado: se registra literal, sin reintento. Se detiene lo que depende de él (de OV-01 dependen su volcado, su contraste, su comparador y el control de OV-05; de OV-02, los suyos) |
| WR2-ST-06 | Q2 rechazada, desplazada fuera de la tolerancia, en otro miembro o con otro valor (W3/W4 §4.3) |
| WR2-ST-07 | Pegado o renombrado accidental que altere algo fuera del modelo; escritura fuera de la carpeta de corrida; necesidad de guardar, analizar o cambiar código |
| WR2-ST-08 | El usuario empieza a usar el equipo, I-64/I-52/I-62 comunican un conflicto, o se acaba la ventana autorizada |

### 4.5 Duración (presupuesto de host)

Entre 1.5 y 2 h, con reserva hasta **2.5 h** contadas desde WR2-01 hasta WR2-08. Al límite: descartar sin guardar, cerrar RAM,
`session-end` y la ventana queda INCOMPLETE. Los pasos WR2-09..14 se hacen fuera de RAM, después del cierre.

## 5. Después de las ventanas

Informe por OV con su resultado y sus diferencias frente a los oráculos, y la ficha de la guía §7 para que la firme el Owner (el
Coordinator no le atribuye firma). **Parada** y solicitud de Owner Validation. La integración es una orden aparte.

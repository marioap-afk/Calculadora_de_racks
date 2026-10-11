# I-64 — F1-T3C-PROFILE-DOC: procedimiento reproducible de diff de perfil para Smoke-1

**Estado:** PROPUESTO. Lo preparó la sesión responsable el 2026-10-11 como trabajo documental autorizado por la revisión del cierre
de T3B (`docs/automation/evidence/I-64-pilot/F1-T3B-HOST-UI/coordinator-close-review.md`, evidencia §57). Está pendiente de la
revisión del Coordinator (**COORDINATOR_T3C_REVIEW_REQUIRED**).

Este documento:
- **no** autoriza Smoke-1 ni el host de I-52, y **no** declara F1 PASS;
- no cambia `src/`, `tests/`, archivos de proyecto, ROADMAP ni HANDOFF;
- no sustituye las comprobaciones propias de I-52 sobre su host.

## 0. Autoridad y objeto

**Autoridad (Freeze de I-64, `docs/initiatives/I-64-proposal-v5.md`):**
- **D-01, «Perfil» (PV2-19, A64-PV1-19):** antes de Smoke-1, acuse de I-52 sobre el procedimiento, el momento y si serían aceptables
  claves de estado de paleta; exportación de solo lectura del subárbol del registro del perfil activo de AutoCAD 2025 antes de abrir
  AutoCAD y después de cerrarlo, con un diff filtrado por las claves que nombren la paleta de RackCad; el resto de diferencias se
  registra como actividad normal de AutoCAD; si I-52 no acepta, o si la prueba exigiera modificar perfil, `TRUSTEDPATHS`, `SECURELOAD` o
  ACL, STOP al Owner o al CAD manager.
- **§12.1:** smoke solo tras un RELEASE explícito de la ventana de host de I-52, sin solaparse con sus sesiones; una sola sesión de
  AutoCAD; I-64 no cambia perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL ni la configuración de I-52, ni instala plugins; la inyección de
  fallos es solo de proceso.
- **§12.2:** «Antes de abrir AutoCAD: acuse de I-52 y exportación de solo lectura del subárbol del perfil». **S1-11:** «Después de
  cerrar AutoCAD: segunda exportación del perfil y diff filtrado; cualquier clave de la paleta se informa a I-52 y al Owner».
- **INV-21 y D-22:** el panel no persiste nada fuera de Actualizar; ni escrituras en el registro de Windows o el perfil desde el
  código de RackCad. Su lado de host es este diff de perfil.

**Objeto:** cuatro fases separadas:

| Fase | Qué es | Herramienta |
|---|---|---|
| **BASELINE** | captura de solo lectura con AutoCAD cerrado, justo antes de abrirlo para Smoke-1 | `Capture-ProfileState.ps1 -Phase BASELINE` |
| **POST** | la misma captura, justo después de cerrar AutoCAD al terminar Smoke-1 | `Capture-ProfileState.ps1 -Phase POST` |
| **DIFF** | comparación determinista de los dos libros canónicos y clasificación de cada diferencia | `Compare-ProfileState.ps1` |
| **DISPOSITION** | resultado único del diff y lo que se hace con él (sección 8) | `Compare-ProfileState.ps1` (código de salida) y la decisión humana que corresponda |

**Herramientas** (PowerShell 7, sin elevación, ASCII; versionadas en
`docs/automation/evidence/I-64-pilot/F1-T3C-PROFILE-DOC/tools/`):

| Herramienta | Blob de Git | SHA-256 del contenido con LF |
|---|---|---|
| `Capture-ProfileState.ps1` | `ef215539536fff3cdf6fd533f87bb2b622118e78` | `750a9097d4d84a75ac1787a7bf40032c44556c64399182aac0b95694b48226e1` |
| `Compare-ProfileState.ps1` | `d314293a6ed1a8cb048a284da24e85104f24078d` | `de642515371b19a59230940ef9659f3d84413f66c8831be201abf55fa73cbd65` |
| `Test-ProfileDiff.ps1` (autoprueba) | `c7d2c35ff4cf07e1f0a47bd06317fc563ea9e001` | `d0bb54be83291e060aeaf4d2ec792743d70fc0f621202a1e24ee2fb41de94d8b` |

El SHA-256 «con LF» se calcula sobre el contenido con los finales de línea normalizados a LF, que es el contenido del blob; así no
depende de `core.autocrlf`. La captura registra el de `Capture-ProfileState.ps1` en su libro (`F|ToolSha256`).

## 1. Relación con I-52: propiedad y RELEASE del host

**Estado de I-52 leído para este procedimiento** (solo lectura, rama `feature/rackmirror-espejo-semantico` en `fb6b5648`, sin
commits desde el 2026-10-02):
- El host está ligado a su campaña CT-21D y al perfil `<<Unnamed Profile>>`.
- Último dictamen (§258 de `docs/automation/decisions/I-52.md`): la corrida P2 H10 está preparada y **no autorizada**; la siguiente
  compuerta es una autorización exacta de su Coordinator. **No consta ningún RELEASE de la ventana de host.**
- Reglas de su campaña (respuesta a I-64, evidencia §13 de I-64): sin actualizaciones; sin cambios de perfil, plugins,
  `TRUSTEDPATHS` ni `SECURELOAD`; una sola `acad.exe`; crear otro perfil o cambiar el registro de AutoCAD altera la máquina ligada.
- Valores que I-52 registró (no se presumen vigentes):
  - `TRUSTEDPATHS` observado: 19 entradas, SHA-256 de la cadena `a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498`
    (`CT21D-TRUSTEDPATHS-OBSERVED-BASELINE-01`, con la misma normalización que la sección 5.3);
  - `SECURELOAD=1`;
  - `acad.exe` `R25.0.171.0.0`, SHA-256 `2a75996fd2a5c5ee0376fd5ba7cced227a098193ff495e8ceeda3a96fe326422`;
  - perfil configurado `<<Unnamed Profile>>`.

**Regla de autoridad:** la referencia de la BASELINE es la que I-52 declare en su acuse del RELEASE. Los valores anteriores solo sirven
para pedir ese acuse. I-64 no decide si la configuración de I-52 sigue vigente.

**Secuencia obligatoria:**
1. I-64 avisa a I-52 por el canal entre sesiones: este procedimiento (ruta y SHA del commit), el momento propuesto y la petición de
   acuse.
2. **RELEASE explícito de I-52** y **acuse**, que debe nombrar:
   - los cuatro valores de referencia: SHA-256 de la cadena `TRUSTEDPATHS`, `SECURELOAD`, perfil actual y SHA-256 de `acad.exe`;
   - la aceptabilidad de claves de estado de paleta: `ACCEPTED` o `NOT_ACCEPTED` (`-PaletteKeyAcceptance`; si falta,
     `NOT_RECORDED`).
3. BASELINE (sección 3), con AutoCAD cerrado.
4. Smoke-1, en una sola sesión de AutoCAD (fuera de este documento).
5. POST (sección 4), en cuanto AutoCAD se cierra.
6. DIFF y DISPOSITION (secciones 6 a 8).
7. Informe a I-52 y al Owner (S1-11) y devolución del host. I-52 vuelve a comprobar su línea base con sus propios instrumentos;
   I-64 no ejecuta instrumentos de I-52 ni toca su evidencia.

## 2. Precondiciones (cualquier NO es STOP antes de abrir AutoCAD)

| # | Precondición | Cómo se comprueba |
|---|---|---|
| PC-1 | RELEASE de I-52 y acuse con los cuatro valores y la aceptabilidad de claves de paleta | texto del acuse, custodiado |
| PC-2 | F1 PASS y Candidato de Smoke-1 (suite de UI completa en local y CI del SHA exacto, §12.1) | decisión del Coordinator |
| PC-3 | 0 procesos `acad` | lo comprueba la captura (código 3) |
| PC-4 | herramientas en sus bytes aprobados | SHA-256 con LF de cada herramienta = tabla de la sección 0 |
| PC-5 | carpeta de evidencia nueva, vacía y fuera de las raíces capturadas | la captura se niega dentro de ellas (código 2) |
| PC-6 | DLL candidato: el Debug del worktree de I-64 en el SHA Candidato (guía de validación manual §2) | `-CandidateDll`; su SHA-256 queda en el libro |
| PC-7 | ninguna variable de entorno persistente con `RackCad` en el nombre (la inyección de fallos es solo de proceso) | `F|RackCadEnvUser` y `F|RackCadEnvMachine` = 0 |
| PC-8 | la autoprueba pasa en la estación | `Test-ProfileDiff.ps1` con código 0 (26/26) |

Carpeta de evidencia recomendada (ignorada por Git):
`<worktree de I-64>\artifacts\orchestration\I-64\SMOKE-1\<RunId>\profile\`. Se usa PowerShell 7 (`pwsh`), no Windows PowerShell 5.1.

## 3. BASELINE

Con AutoCAD cerrado, después de PC-1..PC-8 y antes de abrir AutoCAD (orden exacta; los valores `<…>` son los del acuse):

```powershell
pwsh -NoProfile -File <tools>\Capture-ProfileState.ps1 -Phase BASELINE -OutDir <evidencia>\profile -RunLabel <RunId> `
  -CandidateDll <worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll `
  -ExpectTrustedPathsSha256 <acuse> -ExpectSecureLoad <acuse> -ExpectProfile <acuse> -ExpectAcadSha256 <acuse>
```

- **Código 0:** BASELINE válida; se puede abrir AutoCAD.
- **Código 5** (alguna expectativa del acuse no coincide): **STOP** antes de AutoCAD. Es una deriva de la configuración de I-52 detectada
  antes del smoke: se informa a I-52 y al Owner; I-64 no la resuelve.
- **Códigos 2, 3, 4 o 6:** **STOP**; la BASELINE no existe o es INVALID.

Se registra la salida de consola literal. La BASELINE no se repite para buscar otro resultado: una segunda BASELINE exige un `RunId`
nuevo y la causa registrada.

## 4. POST

En cuanto AutoCAD se cierra al terminar Smoke-1, antes de cualquier otra sesión de AutoCAD o de I-52, en la misma carpeta:

```powershell
pwsh -NoProfile -File <tools>\Capture-ProfileState.ps1 -Phase POST -OutDir <evidencia>\profile -RunLabel <RunId> `
  -CandidateDll <el mismo DLL> `
  -ExpectTrustedPathsSha256 <acuse> -ExpectSecureLoad <acuse> -ExpectProfile <acuse> -ExpectAcadSha256 <acuse>
```

- **Código 0 o 5:** se pasa al DIFF. Un 5 aquí significa que algo cambió durante el smoke, y el DIFF lo clasifica como prohibido.
- **Códigos 2, 3, 4 o 6:** POST INVALID; **STOP**. Un código 3 significa que AutoCAD sigue abierto: no se fuerza su cierre desde
  ninguna herramienta.

## 5. Captura: alcance exacto, normalización y formato

### 5.1 Alcance exacto (idéntico en BASELINE y POST)

`<PRODUCT>` = `Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409` (AutoCAD 2025, inglés: la clave de producto de la línea base de I-52).

| Elemento | Ubicación | Qué se registra |
|---|---|---|
| Perfil actual y conjunto de perfiles | `HKCU\<PRODUCT>\Profiles`: valor por defecto y nombres de subclaves | nombre del perfil (`<<Unnamed Profile>>` en claro; otro nombre, como SHA-256); número y conjunto de perfiles; otros valores de esa clave |
| Subárbol del perfil activo | `HKCU\<PRODUCT>\Profiles\<perfil actual>`, recursivo (incluye `Variables`, `Dialogs`, `General`, `SysvarMonitor`…) | cada clave; cada valor con nombre, tipo, longitud y SHA-256 de sus datos |
| `TRUSTEDPATHS` y `SECURELOAD` | `HKCU\<PRODUCT>\Profiles\<perfil actual>\Variables` | además, como hechos: número de entradas, segmentos vacíos, SHA-256 de cada entrada en orden y de la cadena completa; valor de `SECURELOAD` |
| Estado de UI fijo de AutoCAD | `HKCU\<PRODUCT>\FixedProfile`, recursivo | como el perfil |
| Carga de aplicaciones | `HKCU\<PRODUCT>\Applications`, `\InstalledApplications` y `\Loaded`, recursivos; `HKLM\<PRODUCT>\Applications`, recursivo | como el perfil |
| Identidad del host | `HKLM\<PRODUCT>`, valor `AcadLocation`; `acad.exe` en esa carpeta | `FileVersion` y SHA-256 de `acad.exe`; número de procesos `acad` |
| DLL candidato | `-CandidateDll` | nombre de archivo y SHA-256; si una entrada de `TRUSTEDPATHS` cubre su carpeta |
| Estado de usuario de RackCad | `%APPDATA%\RackCad` (`settings.json`, `user-templates.json`, `Designs\`, `logs\`) y `%LOCALAPPDATA%\RackCad` (`RackFrameConfigurator.layout`), recursivos | ruta relativa, tamaño y SHA-256 de cada archivo; los directorios |
| Plugins instalados | `ApplicationPlugins` de Autodesk en `%APPDATA%`, `%ProgramData%`, `%ProgramFiles%` y `%ProgramFiles(x86)%` | nombres de primer nivel; si un nombre contiene `RackCad`, su contenido completo |
| Variables de entorno persistentes | `HKCU\Environment` y `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment` | solo las que contienen `RackCad` en el nombre: nombre y SHA-256 del valor |

**Por qué estas y no otras:**
- RackCad no escribe el registro: el código del producto en `e0205b5a` no usa `Microsoft.Win32`, `SetSystemVariable` ni variables de
  entorno persistentes.
- Sus únicos estados persistentes de usuario son los dos directorios `RackCad`.
- Su forma de carga en el smoke es `NETLOAD` del DLL Debug. Una carga persistente solo podría aparecer como registro de aplicación
  (`Applications`, `Loaded`) o como bundle en `ApplicationPlugins` (el instalador `deploy/install-bundle.ps1` usa
  `%APPDATA%\Autodesk\ApplicationPlugins`).
- AutoCAD guarda la colocación de ventanas modeless en `Profiles\<perfil>\Dialogs`: ya existen 20 claves `RackCad.UI.*` de ventanas
  anteriores. Ahí, o en `FixedProfile`, aparecería una clave de estado de la paleta si AutoCAD la escribiera (el `UNKNOWN` de D-01).

**Fuera de alcance** (no son ubicaciones de RackCad ni de la línea base de host aprobada):
- `Recent File List` y `Variables` del producto (licencia);
- los subárboles de otros perfiles;
- el resto de `HKLM`;
- carpetas de soporte de AutoCAD;
- las ACL (las gobierna y comprueba I-52);
- cualquier otro ajuste de Windows.

### 5.2 Normalización

- **Lectura:** `OpenSubKey(ruta, $false)`, solo lectura. Las cadenas expandibles se leen sin expandir.
- **Bytes de cada valor**, para su SHA-256:
  - `REG_SZ` y `REG_EXPAND_SZ`: UTF-8 de la cadena;
  - `REG_MULTI_SZ`: UTF-8 de las cadenas unidas por LF;
  - `REG_DWORD` y `REG_QWORD`: el entero sin signo en decimal, ASCII;
  - `REG_BINARY` y otros: los bytes tal cual.
- **Valor por defecto:** se llama `(default)`.
- **`TRUSTEDPATHS`** (compatible con la línea base de I-52):
  - el valor exacto en UTF-8 sin BOM y sin salto final, para el SHA-256 de la cadena;
  - entradas separadas por `;`, en orden, sin las vacías, con el SHA-256 de cada una.
- **Perfil:** `<<Unnamed Profile>>` en claro; cualquier otro nombre como `SHA256:<hex>`. En las rutas, el segmento del perfil es
  siempre `<PROFILE>`.
- **Archivos:** ruta relativa a su raíz con `/`; los directorios terminan en `/`, con tamaño `DIR`.
- **`PICKFIRST`:** el hecho `F|PICKFIRST` reúne los valores llamados `PICKFIRST` en las ubicaciones capturadas. En esta estación solo
  existe el del Monitor de variables (`SysvarMonitor\PICKFIRST`), que **no** es el valor efectivo de la variable; AutoCAD no guarda el
  valor por defecto. S1-12 sigue dependiendo de lo que el Owner tenga configurado.

### 5.3 Redacción

Se aplica a rutas de claves, nombres de valores, rutas relativas de archivos y nombres de plugins. Los **datos** de los valores
nunca entran en el archivo canónico: solo su tipo, longitud y SHA-256.

1. Rutas locales sustituidas por marcadores, la más larga primero y con `\` o `/`: `<PROGRAMFILESX86>`, `<PROGRAMFILES>`,
   `<PROGRAMDATA>`, `<LOCALAPPDATA>`, `<APPDATA>` y `<USERPROFILE>`.
2. Nombre de usuario y de máquina, como palabra completa: `<USER>` y `<MACHINE>`.
3. Si aun así queda algo con forma de ruta (unidad, UNC o `\Users\`), el campo completo pasa a `<REDACTED:<12 hex del SHA-256 del
   original>>`.
4. Perfil con nombre propio: `SHA256:<hex>`.

El **compañero local** (`*.local.json`) conserva los datos en claro: rutas, entradas de `TRUSTEDPATHS` y datos personales como
`*CACHEDONLINEFULLNAME`. **Nunca se versiona ni sale de la estación.** El canónico registra su SHA-256.

### 5.4 Libro canónico y archivos

Cada captura produce un libro de líneas `|`. En cada campo, `%`, `|`, LF y CR se escapan como `%25`, `%7C`, `%0A` y `%0D`.

| Tipo | Formato |
|---|---|
| Hecho | `F|<nombre>[|<calificador>]|<valor>` |
| Clave | `K|<ruta redactada>` |
| Valor | `V|<ruta>|<nombre>|<tipo>|<longitud>|<sha256>` |
| Archivo | `D|<raíz>|<ruta relativa>|<bytes>|<sha256>`; directorio: `D|<raíz>|<ruta>/|DIR|-` |
| Plugin | `P|<raíz>|<nombre>|DIR o FILE` |
| Entorno | `E|USER o MACHINE|<nombre>|<sha256>` |

- **Orden:** ordinal (`StringComparer.Ordinal`).
- **Texto canónico:** las líneas unidas por LF, con un LF final, en UTF-8 sin BOM.
- **`CanonicalSha256`:** el SHA-256 de ese texto. Es la identidad de la captura.

**Archivos de cada fase**, en la carpeta de evidencia (`CreateNew`: nunca se sobrescribe):
- `profile-capture-<FASE>.json`: esquema `rackcad-i64-profile-capture/v1` con `Phase`, `RunLabel`, `CanonicalSha256`,
  `LedgerLineCount`, `Facts`, `Counts`, `PreSmokeChecks`, `Ledger`, `Ephemeral` y `LocalCompanion`;
- `profile-capture-<FASE>.json.sha256`: el SHA-256 del archivo;
- `profile-capture-<FASE>.local.json`: el compañero local (sección 5.3).

**Campos estables:** el libro entero. Con el mismo estado de la estación, dos capturas dan el mismo `CanonicalSha256`.

**Campos efímeros permitidos** (fuera del libro y de `CanonicalSha256`): `Ephemeral.CapturedUtc`, `Ephemeral.PSVersion`,
`Ephemeral.FileLastWriteUtc`, `RunLabel`, `PreSmokeChecks` (dependen de los parámetros) y, por todo ello, el SHA-256 del archivo
JSON. Las capturas se comparan por `CanonicalSha256` y por su libro, nunca por el SHA-256 del archivo.

## 6. DIFF

```powershell
pwsh -NoProfile -File <tools>\Compare-ProfileState.ps1 `
  -Baseline <evidencia>\profile\profile-capture-BASELINE.json -Post <evidencia>\profile\profile-capture-POST.json `
  -OutFile <evidencia>\profile\profile-diff.json -PaletteKeyAcceptance <ACCEPTED | NOT_ACCEPTED | NOT_RECORDED, del acuse>
```

**Integridad**, antes de comparar (cualquier fallo es INVALID, R-INV-04):
- cada `.sha256` coincide con su archivo;
- `CanonicalSha256` coincide con el libro;
- el libro está ordenado;
- las fases son BASELINE y POST;
- no hay identidades duplicadas ni líneas de tipo desconocido.

**Identidad de una entrada:**
- hecho: el nombre y el calificador;
- clave: su ruta;
- valor: ruta y nombre;
- archivo: raíz y ruta;
- plugin: raíz y nombre;
- entorno: ámbito y nombre.

Cada identidad presente en una sola captura es `ADDED` o `REMOVED`; si está en las dos con contenido distinto, `CHANGED`. Las
entradas salen en orden ordinal; el diff no lleva marcas de tiempo, así que dos comparaciones de las mismas capturas dan el mismo
archivo. El resultado es `profile-diff.json` (esquema `rackcad-i64-profile-diff/v1`) más su `.sha256`.

## 7. Clasificación: prohibidas, informables y permitidas

Cada diferencia recibe la primera regla que aplica, en este orden.

**INVALID** (la comparación no prueba nada):

| Regla | Condición |
|---|---|
| R-INV-01 | una captura tiene `AcadProcessCount` distinto de 0 |
| R-INV-02 | cambian la identidad del host (`AcadExeSha256`, `AcadExeFileVersion`, `ProductKey`, `AutoCADRelease`), de la herramienta (`ToolSha256`, `Schema`) o del DLL candidato, o el recuento de ilegibles |
| R-INV-03 | hay entradas ilegibles en alguna captura |
| R-INV-04 | falla la integridad de la sección 6 |

**FORBIDDEN** (diferencias prohibidas; STOP):

| Regla | Diferencia |
|---|---|
| R-F-01 | perfil actual, número o conjunto de perfiles, o valores de la clave `Profiles` |
| R-F-02 | `SECURELOAD` (hecho o valor) |
| R-F-03 | `TRUSTEDPATHS`: cualquier byte de la cadena, una entrada, su número, o que pase a cubrir la carpeta del DLL candidato |
| R-F-04 | cualquier valor llamado `PICKFIRST` |
| R-F-05 | cualquier clave o valor en `Applications`, `InstalledApplications` o `Loaded` (HKCU) o en `Applications` (HKLM): registro de carga |
| R-F-06 | cualquier entrada nueva, retirada o cambiada en las raíces `ApplicationPlugins`, o su presencia |
| R-F-07 | cualquier variable de entorno persistente con `RackCad` en el nombre |
| R-F-08 | cualquier archivo o directorio de `%APPDATA%\RackCad` fuera de `logs\`, o de `%LOCALAPPDATA%\RackCad`, nuevo, retirado o cambiado (INV-21: el panel no persiste) |

**REPORT** (se informa a I-52 y al Owner, S1-11):

| Regla | Diferencia |
|---|---|
| R-R-01 | clave o valor nuevo, retirado o cambiado cuya ruta o nombre contiene `RackCad`, `RACKPANEL` o `Workspace`, salvo R-A-02. Incluye la paleta (`PaletteSet` «RackCad», raíz `RackCad.UI.Workspace.*`) y una ventana `RackCad.UI.*` que no estaba en la BASELINE |
| R-R-02 | clave nueva con nombre GUID en el perfil o en `FixedProfile`: posible estado de una paleta |
| R-R-99 | diferencia en una ubicación que estas reglas no clasifican |

**ALLOWED** (se registra como actividad normal):

| Regla | Diferencia |
|---|---|
| R-A-01 | archivos de `%APPDATA%\RackCad\logs\`: el registro de RackLog, la única escritura de archivo admitida (D-22) |
| R-A-02 | valores de `Dialogs\RackCad.UI.<ventana>` (no `Workspace`) cuya clave ya existía en la BASELINE: AutoCAD guarda la colocación de ventanas RackCad existentes, por ejemplo RACKEDITAR en S1-07 |
| R-A-03 | cualquier otra diferencia en el perfil activo o en `FixedProfile`: actividad normal de AutoCAD (D-01), por ejemplo `Drawing Window`, `Command Line Windows` o `General` |

## 8. DISPOSITION

| Disposición | Código | Cuándo | Qué se hace |
|---|---|---|---|
| `CLEAN` | 0 | solo ALLOWED o ninguna diferencia | la parte de perfil de S1-11 queda sin hallazgos; se informa a I-52 y al Owner y el registro va a la evidencia de Smoke-1 |
| `REPORTED_ACCEPTED` | 11 | hay REPORT e I-52 aceptó en el acuse las claves de estado de paleta | se informa cada clave a I-52 y al Owner (S1-11); el diff no bloquea por sí mismo |
| `REPORT_REQUIRED_STOP` | 10 | hay REPORT sin esa aceptación (`NOT_ACCEPTED` o `NOT_RECORDED`) | **STOP** al Owner, I-52 y Coordinator (D-01, paso 3) |
| `FORBIDDEN_DRIFT` | 20 | hay alguna FORBIDDEN | **STOP**; sección 10 |
| `INVALID` | 30 | hay alguna INVALID | **STOP**; la comparación no prueba nada; no se repite sin autorización del Coordinator, con `RunId` nuevo |

- La herramienta clasifica; no aprueba.
- El resultado de Smoke-1 y de INV-21 en el host lo disponen el Owner y el Coordinator; I-52 recibe el informe.
- Ninguna disposición autoriza reparar nada desde I-64.

## 9. Condiciones de STOP

| Id | Condición |
|---|---|
| STOP-01 | no hay RELEASE de I-52 o el acuse no nombra los cuatro valores de referencia |
| STOP-02 | hay un proceso `acad` al capturar la BASELINE o el POST (código 3), u otra sesión de AutoCAD o de I-52 entre BASELINE y POST |
| STOP-03 | la BASELINE no coincide con el acuse (código 5): deriva de la configuración de I-52 antes del smoke |
| STOP-04 | alguna herramienta no tiene su SHA-256 aprobado, o la autoprueba no da 26/26 |
| STOP-05 | la captura no puede leer la clave de producto o el perfil (código 4), o deja entradas ilegibles (código 6) |
| STOP-06 | cargar el DLL exigiría cambiar la seguridad o el perfil, por ejemplo una opción que confíe la carpeta de forma permanente o un perfil nuevo (§12.1) |
| STOP-07 | el operador se desvía de las órdenes escritas: ediciones manuales del registro, otro perfil, otra carpeta de evidencia o una repetición sin `RunId` nuevo |
| STOP-08 | DISPOSITION `REPORT_REQUIRED_STOP`, `FORBIDDEN_DRIFT` o `INVALID` |
| STOP-09 | cualquier necesidad de reparar algo: no hay reparación automática |

## 10. Restauración ante una deriva prohibida

- **Sin reparación automática:** ni la herramienta, ni la sesión, ni un Worker escriben el registro, el perfil, `TRUSTEDPATHS`,
  `SECURELOAD`, ACL o plugins. Se conservan la BASELINE, el POST, el diff y los compañeros locales.
- **Aviso:** al Owner o CAD manager y a I-52, por el canal entre sesiones, con el diff redactado y las reglas que dispararon.
- **Restauración humana:**
  - la hace el Owner o CAD manager dentro de la ventana de host y con el acuerdo de I-52;
  - la referencia exacta son los valores del compañero local de la BASELINE (por ejemplo la cadena `TRUSTEDPATHS` exacta,
    `SECURELOAD` y el perfil actual);
  - un bundle o un registro de carga de RackCad se retira igual, por el Owner.
- **Comprobación:** una captura POST nueva en una carpeta nueva (`…\profile-restore\`) y el DIFF contra la misma BASELINE. Ninguna
  FORBIDDEN puede quedar; las ALLOWED de la sesión no se deshacen.
- **I-52:**
  - según su registro de línea base, cualquier cambio de un byte de `TRUSTEDPATHS` es CONFIGURATION DRIFT de su campaña;
  - la restauración no borra ese hecho; I-52 decide sus consecuencias.
- **Claves de paleta no aceptadas:** retirarlas es también una acción humana con acuerdo de I-52. Si fuera un comportamiento propio de
  `PaletteSet`, la contingencia (b) de D-01 (`ShowModelessWindow`) exige una A-n material con Architect y Coordinator.
- **Smoke-1:** no puede quedar en PASS mientras haya una deriva prohibida sin resolver. Repetirlo exige decisión del Coordinator y
  `RunId` nuevo.

## 11. Custodia y redacción de la evidencia

- La carpeta de evidencia vive bajo `artifacts/` (ignorada por Git).
- La custodia sigue la de T3A y T3B: manifiesto con el SHA-256 de cada original.
  - Se versionan el canónico, su `.sha256`, el diff y su `.sha256`, tras comprobar que no tienen rutas locales.
  - **El compañero local no se versiona**; se registra su SHA-256.
- El acuse y el RELEASE de I-52, la salida de consola de cada orden y la disposición del Owner y del Coordinator se custodian como
  transcripciones literales.

## 12. Validación de T3C: solo lectura y reproducible

Ejecutada el 2026-10-11 en la estación, con AutoCAD cerrado, sin elevación y fuera del repositorio. Los números exactos están en
`docs/automation/evidence/I-64-pilot/F1-T3C-PROFILE-DOC/validation-record.json`.

| Comprobación | Resultado |
|---|---|
| Análisis estático de las tres herramientas | 0 errores de sintaxis; 0 coincidencias de escritura del registro, ACL, `reg add/delete/import`, `Set-Item*`, `Remove-*`, `Copy/Move-Item`, `Out-File`/`Set-Content`, `Start-Process`, `NETLOAD` o variables de entorno; ASCII; las únicas escrituras son archivos `CreateNew` en la carpeta de salida (la autoprueba, además, su carpeta de trabajo nueva) |
| Autoprueba (26 casos sintéticos: cada regla de la sección 7 y cada código) | 26/26 PASS dos veces; salidas idénticas byte a byte |
| Dos capturas DRYRUN consecutivas | mismo `CanonicalSha256`, 2229 líneas: 399 claves, 1767 valores, 16 entradas de archivo, 3 plugins, 0 de entorno, 0 ilegibles |
| DIFF entre ellas | `CLEAN`, 0 diferencias |
| Exportación (`reg export`, solo lectura) de los seis árboles del registro, antes y después de toda la validación | SHA-256 idénticos en los seis |
| Archivos de los directorios `RackCad` antes y después | idénticos |
| Procesos `acad` antes y después | 0 |

**Hechos informativos de la captura DRYRUN** (no es una BASELINE ni sustituye el acuse de I-52):
- la cadena `TRUSTEDPATHS` (19 entradas, SHA-256 `a9ba0fd1…`), `SECURELOAD=1`, el perfil `<<Unnamed Profile>>` y el SHA-256 de
  `acad.exe` coinciden con lo último que registró I-52;
- no hay bundle de RackCad en ninguna raíz `ApplicationPlugins`, ni registro de carga de RackCad, ni variables de entorno de RackCad.

## 13. Lo que falta antes de F1 PASS (informativo; lo decide el Coordinator)

1. Revisión de este procedimiento por el Coordinator y cierre de F1-T3C-PROFILE-DOC.
2. **Procedimiento de Smoke-1 para el operador** (los pasos de S1-01..S1-12 de §12.2): la tabla de enrutamiento del Freeze pone en F1
   «procedimientos de smoke y de diff de perfil». Este documento cubre el diff de perfil; el procedimiento de pasos del smoke no
   existe todavía y no está en esta autorización (decisión O-1).
3. Cierre del gate F1:
   - paquete que ate cada invariante de cierre (INV-01, 02, 03 en el lado de eventos, 07, 13, 14, 16, 17, 21, 22, 23, 25 en el panel y
     33) a la evidencia de F1-T1, T2, T3A y T3B;
   - CI del SHA exacto y revisión del Coordinator.

   El ADR `propuesto` ya existe (`docs/adr/0047-workspace-persistente-modeless-rackcad.md`).

Después de F1 PASS, y no antes (A64-PV3-06): Candidato de Smoke-1 (suite de UI completa en local y CI del SHA exacto), RELEASE de I-52
y Smoke-1 con este procedimiento. La parte de host de INV-21 se cumple con su DISPOSITION.

## 14. Decisiones abiertas para el Coordinator

- **O-1.** El procedimiento de pasos de Smoke-1 para el operador: ¿parte de T3C, tarea documental aparte o se deja para el paquete
  de Smoke-1?
- **O-2.** Carpeta de evidencia y custodia de las capturas canónicas de Smoke-1 (propuesta: la de la sección 11).
- **O-3.** Una ventana `RackCad.UI.*` existente que se abre por primera vez durante el smoke da REPORT (R-R-01), por prudencia. ¿Se
  acepta o se pasa a ALLOWED?
- **O-4.** Con `SECURELOAD=1`, cargar un DLL desde una carpeta no confiada puede mostrar el aviso de seguridad de AutoCAD. Elegir la
  opción que confía la ubicación de forma permanente está prohibido (STOP-06; el diff lo detecta como R-F-03). ¿Se admite la carga de
  una sola vez, sin cambio persistente, o hay que pedir a I-52 y al CAD manager otra vía? Es `HOST-TO-CONFIRM`.

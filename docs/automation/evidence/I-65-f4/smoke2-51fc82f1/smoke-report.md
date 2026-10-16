# I-65 F4 — segundo smoke de `RACKRAMPAQUETE` con filas `verified` (custodia, 2026-10-16)

- **Autoridad:** «OWNER HOST RELEASE / Second AutoCAD Smoke — Verified Mapping Package»
  ([texto literal](../../../decisions/I-65-owner-host-release-second-smoke.original.txt)); decisiones §68-§69. OWNER HOST RELEASE =
  APPROVED (CONDITIONAL ON FRESH PREFLIGHT); SINGLE NETLOAD = AUTHORIZED; SECOND SMOKE = AUTHORIZED; RAM EXECUTION = NO; F4 GATE PASS = NO.
- **Ejecutor:** Coordinator con Computer Use (acceso concedido solo a «AutoCAD 2025 - English»). Carpeta local de la corrida (no
  versionada): `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\` (`baseline\`, `post\`, `captures\`, `pkg-01\`, informe del validador y
  diagnóstico de V-11).
- Procedimiento: [segundo smoke](../../../../initiatives/I-65-f4-second-smoke-procedure.md) (SM2-01..15) y
  [preflight/POST](../../../../initiatives/I-65-f4-host-release-and-preflight.md) §3-§5.

## 1. Horas reales (UTC)

| Hito | Hora |
|---|---|
| Comunicación del RELEASE a I-52, I-64 e I-62 | ~19:15Z |
| I-64 «sin conflicto activo» / I-62 «sin conflicto» / I-52 «no emito STOP» (hechos de 19:14:59Z) | ~19:15Z / ~19:16Z / ~19:17Z |
| Custodia previa en la rama (`51e0061d`) y K1 | 19:17-19:18Z |
| BASELINE de solo lectura | 19:18:35Z |
| Aviso previo a I-52 e I-64 | ~19:19Z |
| Apertura de AutoCAD (una instancia) | 19:19:07Z |
| SHA-256 del DLL y de la tabla justo antes de abrir NETLOAD | 19:20:22Z |
| SHA-256 del DLL inmediatamente antes de pulsar «Open» | 19:21:29Z |
| NETLOAD con «Load Once»; módulo comprobado en `acad.exe` | ~19:21:40Z; 19:21:51Z |
| Resultado de `RACKRAMPAQUETE` | ~19:25:30Z |
| Validador del paquete | 19:25:46Z |
| AutoCAD cerrado sin guardar; `acad.exe` = 0 | ≤ 19:27:08Z |
| POST de solo lectura y DIFF | 19:27:16Z |
| K2 sin cambios; aviso de cierre a I-52, I-64 e I-62 | ~19:28-19:30Z |

## 2. Preflight (BASELINE; [resumen](baseline-summary.txt))

`acad.exe` 0 y `RAMElements.exe` 0; perfil `<<Unnamed Profile>>` (1 perfil); `TRUSTEDPATHS` 19 entradas
`a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498`; `SECURELOAD` 1; `acad.exe` R25.0.171.0.0
`2a75996fd2a5c5ee0376fd5ba7cced227a098193ff495e8ceeda3a96fe326422`; `Applications`, `InstalledApplications` y `Loaded` (HKCU) y
`Applications` (HKLM) con los mismos SHA-256 que el primer smoke; `ApplicationPlugins` sin RackCad; 0 variables `RackCad`; `icacls` de
`AcadLocation` y de las cuatro rutas de I-52 iguales a la referencia (leídas solo con `icacls`, como pidió I-52); carpeta del DLL
`53f268fe…c77b`, no cubierta por `TRUSTEDPATHS`. **Identidad del paquete de prueba:** DLL `fa334b14…aedb` (`1.0.0+51fc82f1…`); salida
recalculada = manifiesto `5c9a6d62…65d2` (32 archivos); tabla `fc3d9146…b619`; cargas `25f48e7f…ae05`; valores esperados
`8b264a05…2148`; validador `8b19a9c7…` = copia versionada. `pkg-01` inexistente. Sin deriva de las referencias protegidas: **preflight
limpio**. Las exportaciones `Profiles` y `FixedProfile` difieren del primer smoke (estado de interfaz de sesiones de AutoCAD posteriores),
como era de esperar.

## 3. DLL y tabla cargados

`D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows\RackCad.Plugin.dll`, SHA-256 **`fa334b144507fc8922171036edae878f773c003164f4c98302cdc67c9291aedb`**
en la BASELINE, a las 19:20:22Z, a las 19:21:29Z y en el POST; la lista de módulos de `acad.exe` (PID 37316) mostró ese único
`RackCad.Plugin.dll` desde esa carpeta. Tabla junto al DLL `fc3d9146…b619` (las dos filas `verified`) antes y después. El diálogo de
NETLOAD se abrió en una carpeta `net8.0-windows` ajena (13/10/2026); se navegó hasta la carpeta autorizada
([captura 03](03-netload-dll-autorizado-seleccionado.jpg)); aviso estándar «Security - Unsigned Executable File» con la ubicación
`D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows` → **«Load Once»** ([captura 04](04-aviso-seguridad-load-once.jpg)); nunca «Always Load».

## 4. Dibujo REF-CANT-01 y resultado visible

- Dibujo nuevo desechable `Drawing1.dwg`. `RACKCANTILEVER`: `REF-CANT-01`, 2 estaciones, separación 48, una cara Y+, 3 niveles, troquel
  0, claro 24; columna y base `AISC-W-W12X26`, base 48 ([captura 06](06-columna-base-w12x26.jpg)); brazo sencillo
  `AISC-HSS-RECT-HSS4X4X_250`, corte 36, pendiente por omisión 0.438 ([captura 07](07-brazo-hss4x4x1_4.jpg)). Línea resuelta: «2
  estaciones · 1 intervalos · altura común 90.5 in · 1 paneles arriostrados · 2 separadores · 2 tensores · 4 componentes en el BOM»
  ([captura 08](08-linea-ref-cant-01-resuelta.jpg)); vista frontal insertada como bloque
  `RACKCAD_CANTILEVER_REF-CANT-01_-_frontal_FRONTAL` (310 contornos); aviso no bloqueante de INSUNITS (I-05) sin cambiar nada.
- `RACKRAMPAQUETE` → rack → estación `0` → `D:\r65-smoke\51fc82f1\inputs\ref-cant-01.loads.json` → carpeta
  `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\pkg-01`, tecleadas en trozos de ≤ 13 caracteres **con `\`** y sin portapapeles
  ([capturas 10-11](11-carpeta-tecleada-barras-invertidas.jpg)). Un único intento.
- **Salida literal** ([capturas 12-13](13-consola-completa.jpg)):

```text
Archivo de cargas: D:\r65-smoke\51fc82f1\inputs\ref-cant-01.loads.json

Carpeta NUEVA del paquete: D:\r65-smoke\51fc82f1\run-R20261016T1918Z\pkg-01

RackCad paquete estructural: Valid. Paquete publicado en D:\r65-smoke\51fc82f1\run-R20261016T1918Z\pkg-01
  Huella de entrada: 505941e2ca42a02dd85b72629579bcd27e4900f80c69ebd41bb8ea73c0a5ba61
  Archivo escrito: manifest.json
  Archivo escrito: rackcad-ram-build.py
  Archivo escrito: structural-loads.json
```

Ninguna línea de diagnóstico. El modelo neutral fue `Valid` (el comando solo publica con modelo válido).

## 5. Paquete generado

| Archivo | Bytes | SHA-256 (bytes = normalizado LF; sin CR ni BOM) |
|---|---|---|
| `manifest.json` ([copia versionada](manifest.json)) | 9 594 | `19fba962297badc95dd008bcf8d1477993c55a7ff8b5347d68e311e246f53a78` |
| `rackcad-ram-build.py` (por SHA-256; queda en la carpeta local) | 18 183 | `d9b41aab69643c01971e756fb9323a8f35dad6b33008598655c753fe2f92d719` |
| `structural-loads.json` (= archivo de cargas aprobado) | 592 | `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05` |

Manifiesto: `rackId` `f01ec1e6-4976-47bc-9429-73fb7fbd2b5a`, `kind` Cantilever, estación 0, `sourceFingerprint`
`85fea23108cc04365850bf90d5d494c36529782d5845a14ef01ec996959cd1b7`, `generatorVersion` `rackcad-structural-model/1`,
`analysisInputFingerprint` `505941e2…ba61`, `mapping.rowsSha256` `54ca9f6c…c98c`, adaptador `rackcad-ram-elements-adapter` 1.0.0, producto
declarado RAM Elements 26.00.01.015 «declared, not verified», paquete `ramelements` 0.1.0, unidades `LB_IN`. Sin carpetas `*.tmp-*` en la
carpeta padre: la salida no es parcial.

## 6. Validación (16 comprobaciones; [informe](validator-report.json))

**Resultado oficial del validador: FAIL 15/16.** V-01..V-10 y V-12..V-16 PASS; **V-11 FAIL** («huella de entrada embebida en el
script=True; carpeta embebida=False»). El resultado no se ha corregido ni se ha vuelto a ejecutar con otro validador.

**Diagnóstico de solo lectura de V-11** ([script](diag-v11-smoke2.py.txt), [salida](v11-diagnostic.json); no sustituye al resultado
oficial): la carpeta **sí** está embebida. El script guarda la carga útil como una constante `PAYLOAD`, que es JSON dentro de un literal de
Python, así que cada `\` de la ruta aparece cuadruplicado en los bytes (doble escape). V-11 busca la carpeta cruda (0 ocurrencias) o con
un solo escape JSON (0), pero no con doble escape (1 ocurrencia). Al decodificar `PAYLOAD` (con `ast.literal_eval` y `json.loads`, sin
ejecutar el script): `packageFolder` = carpeta tecleada = `packageFolder` del manifiesto, y `analysisInputFingerprint` = huella del
manifiesto. **Es un falso negativo del validador**, un defecto de la preparación (la autoprueba con paquetes sintéticos embebía la carpeta
con un solo nivel de escape); el producto no tiene el defecto.

**Huellas:** consola `505941e2ca42a02dd85b72629579bcd27e4900f80c69ebd41bb8ea73c0a5ba61` = manifiesto = `PAYLOAD` del script (64
caracteres completos, no solo los 8 primeros y los 8 últimos).

**UTF-8 (A65-F4D-10):** V-15 PASS: `verifiedBy` aparece 2 veces como los bytes UTF-8
`4d6172696f2050c3a972657a20e28094204f776e6572202f20456e67696e656572696e672056616c696461746f72` («Mario Pérez — Owner / Engineering
Validator»), sin escapes `\u` ni caracteres de sustitución; `manifest.json` es UTF-8 estricto sin BOM (V-04). **Filas (V-13/V-14):** las
dos filas del manifiesto son iguales a la tabla distribuida, `verified`, `verifiedOn` 2026-10-16; `rowsSha256` = valor calculado con el
DLL. **Fin de línea (A65-F4D-01):** los tres archivos sin CR; SHA-256 de bytes = normalizado.

## 7. Limitaciones e hipótesis registradas (SM2-10)

- Advertencia del manifiesto: «These assumptions are hypotheses for transfer equivalence only; they are not a structural validation.»
- Supuestos: `ASM-PROP-01`, `CON-01`, `IDL-NO-OFFSETS`, `LOD-01`, `SUP-01`, `SUP-02`, `SUP-03` (SHA-256
  `ab580faf…8818`). Exclusiones: base, placas, cartelas, separadores, arriostramiento y redondos no modelados; **sin peso propio, sin
  combinaciones**, sin cargas nodales, distribuidas ni locales; sin resortes, desfases rígidos ni liberaciones.
- **DIF-01** (RAM 486.94 pcf frente a 490 pcf, −0.62 %): informativa **solo** sin peso propio; el paquete no incluye peso propio.
- **OD-F4-14 = CMB-3:** las combinaciones no se deducen de que el paquete se genere.
- El `source` de las filas declara E 29 000 ksi y ν 0.30 como convención de modelado y 490 pcf como densidad implícita de AISC v16.0; la
  tabla `verified` no es validación estructural ni certificación del material.
- El paquete es **desechable**: no es el paquete definitivo y no se ha llevado a RAM.

## 8. POST y DIFF ([resumen POST](post-summary.txt); [claves cambiadas](diff-registry-keys.txt))

| Clase | Resultado |
|---|---|
| **PROHIBIDO** | **ninguno**: perfil y conjunto de perfiles, `TRUSTEDPATHS` (bytes y entradas), `SECURELOAD`, `acad.exe`, `Applications`, `InstalledApplications` y `Loaded` (HKCU), `Applications` (HKLM), `ApplicationPlugins`, variables `RackCad`, listados `icacls` (incluidas las cuatro rutas de I-52), DLL, salida, tabla, cargas y valores esperados: idénticos |
| **INFORMAR** | `Profiles\<<Unnamed Profile>>\Dialogs\NetLoadDialog`: `FileNameMRU0..10` e `InitialDirectory`; `Profiles\<<Unnamed Profile>>\Dialogs\RackCad.UI.Systems.Cantilever.RackCantileverWindow`: `Bounds`; **`Profiles\<<Unnamed Profile>>\Variables`: `VIEWBACKSTATUS` BORRADO** (valía `"1"` en esta BASELINE y en los dos lados del primer smoke; lo quitó AutoCAD al salir, sin acción de I-65; no se restaura); `FixedProfile\Application Window`: `Position`; `FixedProfile\Command Line Windows`: `TextWindow.Position` y `TextWindow.Show` (ventana de texto F2 abierta para capturar la consola) |
| Otros | `%APPDATA%\RackCad` sin cambios (mismo listado); el único cambio de archivos es el paquete `pkg-01` previsto; checkouts sin cambios (K1/K2: `stop` false, sin hallazgos) |

Resultado del DIFF: **INFORMAR**. I-52 decide si afecta a su tupla (AC2-06); `VIEWBACKSTATUS` se le señaló expresamente.

## 9. Condiciones de aceptación

| # | Estado |
|---|---|
| AC2-01 `Valid` con el texto esperado, sin diagnósticos | **cumple** |
| AC2-02 validador 16/16; huellas coherentes | **NO cumple a la letra:** 15/16 (V-11 falso negativo del validador, §6); huellas coherentes |
| AC2-03 modelo `Valid`, salida no parcial | **cumple** |
| AC2-04 mismo SHA-256 del DLL en BASELINE, antes de NETLOAD y en POST | **cumple** |
| AC2-05 AutoCAD cerrado sin guardar; RAM nunca iniciado; nada analizado | **cumple** |
| AC2-06 DIFF `LIMPIO`, o `INFORMAR` aceptado por I-52 | **cumple a nivel de la sesión de I-52**: recomprobación sin deriva; `INFORMAR` registrado como observado, sin acción (custodia §13); su Coordinator puede pronunciarse |
| AC2-07 K1/K2 sin cambios; avisos previo y de cierre | **cumple** |

ST2-07 (comprobación del validador en `FAIL`) se activó: registrado, sin repetir la generación ni corregir el validador; decide el Owner.

## 10. Incidencias

1. **V-11:** falso negativo del validador por el nivel de escape (§6). Defecto de la preparación del Coordinator, no del producto.
2. **Lista desplegable oculta en NETLOAD:** el clic para abrir «Look in» desplegó una lista que la captura no mostraba (otra ventana del
   sistema); el clic siguiente, para cerrarla, cayó sobre la entrada `test-tmp` y abrió esa carpeta vacía
   ([captura 02](02-incidente-carpeta-test-tmp.jpg)). Solo navegación: no se cargó nada de ahí. Además, Computer Use bloquea el teclado en
   los diálogos de archivo fuera del campo «File name».
3. **`VIEWBACKSTATUS`** borrado del perfil por AutoCAD al salir (§8; `INFORMAR`).
4. **Pendiente del brazo** `0.438` por omisión, como en el primer smoke; la igualdad exacta del dibujo con el fixture `REF-CANT-01` se
   valida en la OV, no aquí.
5. La confirmación expresa del Coordinator de I-64 llegó **después** de abrir la ventana y se refería a la franja propuesta del
   2026-10-17. La ventana usada se apoyó en el RELEASE del Owner y en los hechos de las tres iniciativas («sin conflicto activo»).

## 11. Adenda (2026-10-16 ~20:41Z): corrección de V-11 y revalidación

El texto anterior se conserva tal como se registró. Por orden del Owner (decisiones §70), el validador se corrigió (v2) y el **mismo**
paquete se revalidó en solo lectura: **PASS 16/16**, con los SHA-256 de los tres archivos idénticos antes y después y pruebas negativas que
detectan rutas alteradas y distintos niveles de escape ([informe de la revalidación](v11-correction/revalidation-report.md)). Estado:
**SECOND SMOKE = PASS AFTER VALIDATOR CORRECTION**.

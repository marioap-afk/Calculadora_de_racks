# I-65 — Informe de W-R1b (HB-01..05, prueba real del Autoloader) sobre `4a03b65b`

Corrida: `D:\r65-smoke\4a03b65b\wr1-R20261029T1633Z\hb\` (local). Autorización: decisiones §119 y liberación del host del Owner
(«I-65 — OWNER RELEASE / W-R1b»). Procedimiento: [procedimientos](../../../../initiatives/I-65-ready-wr1-wr2-procedures.md) §3.3, parte C.
Horas en UTC.

> **Resultado: HB-01 a HB-05b conformes; HB-05c en STOP.**
>
> - El Autoloader carga el Candidato.
> - La tabla viaja en el bundle.
> - Los dos paquetes son `Valid`, con huella = H1 = H2 (a) y carga útil 117/117 (b).
> - La desinstalación restaura los archivos.
> - **Lo que no se cumple:** AutoCAD dejó un registro del Autoloader en `HKCU\…\Applications` y `HKCU\…\Loaded` (clase protegida). No se corrigió a mano. Decide el Owner.

## 1. Resultados HB-01..05

| Paso | Resultado | Evidencia |
|---|---|---|
| HB-01 | **PASS**. BASELINE 21:09:50Z, igual al POST de W-R1a y repetida sin cambios a las 01:54:02Z. Raíz del usuario de `ApplicationPlugins` vacía; ninguna raíz con RackCad; 0 referencias `RackCad.bundle` en las seis exportaciones. `TRUSTEDPATHS` (19 entradas, `a9ba0fd1…`) no cubre la raíz del usuario ni `RackCad.bundle\Contents`. | `baseline-summary.txt`, `recheck-pre-install-summary.txt`, `hb01-roots.txt` |
| HB-02 | **PASS**. (1) Bundle de origen: 160 comprobaciones y reproducibilidad. (2) `install-bundle.ps1` sin `-Build`, exit 0, 01:55:55Z-01:55:56Z. (3) Bundle instalado: 160 comprobaciones y reproducibilidad. (4) Tabla instalada `fc3d9146…` = fuente (`i/lf w/lf`). Raíz: solo `RackCad.bundle` (25 archivos), sin staging ni respaldo. | `hb02-1..4`, `hb02-roots.txt`, `hb02-installed-inventory.txt` |
| HB-03 | **PASS**. Una instancia de AutoCAD (pid 29912, 01:56:41Z) con `Drawing1.dwg` nuevo, sin NETLOAD. Al arrancar salió el aviso estándar «Security - Unsigned Executable File» para `…\ApplicationPlugins\RackCad.b…\Contents`; solo «Load Once». Un único `RackCad.Plugin.dll`, desde el bundle y con `60e1dd44…`. El historial solo contiene el arranque de AutoCAD. | `module-list-HB-after-start.txt`, capturas `HB03-*` |
| HB-04 (a) | **PASS**. `hb-ref.dwg` `ac2c8014…` antes y después. Se abrió sin avisos de RackCad. Vista frontal (la principal). `pkg-HB` `Valid`, tres archivos, sin diagnósticos. **Huella `f69400f6…15eb` = H2 = H1 de OV-08**, con el mismo `rackId` y la misma huella de fuente. Validador v2 16/16. | `manifest-pkg-HB`, `validator-v2-pkg-HB`, capturas `HB04a-*` |
| HB-04 (b) | **PASS**. REF-CANT-01 nuevo (W1 §4.1) en `Drawing2.dwg`, con el resumen esperado e «Insertar frontal» en 0,0. `pkg-HB2` `Valid`, sin diagnósticos. Huella `b7032b9f…` (otro rack, distinta por diseño). Validador v2 16/16 y **carga útil 117/117**. | `manifest-pkg-HB2`, `validator-v2-pkg-HB2`, `payload-vs-manual-pkg-HB2.json`, capturas `HB04b-*` |
| HB-05a | **PASS**. `QUIT` y «No» al guardar; `acad` = 0 a las 02:04:42Z. Ningún `.dwg`, `.bak` ni `.dwl` nuevo. | captura `HB05a-*` |
| HB-05b | **PASS**. Inventario antes de retirar = inventario de HB-02 (`5d777353…`, 25 archivos). `Remove-Item -LiteralPath` de la ruta comprobada. Raíz del usuario vacía = HB-01. | `hb05b-uninstall.txt`, `hb05b-roots.txt` |
| HB-05c | **STOP**. El POST tiene diferencias protegidas en B-04 `hkcu-applications` y `hkcu-loaded`, más 4 referencias `RackCad.bundle` (§6). Todo lo demás de la clase protegida es igual. `hkcu-profiles` es INFORMAR. | `post-summary.txt`, `diff-registry-keys.txt`, `hb05c-*` |

## 2. Carga real del Autoloader

- Sin NETLOAD en la sesión: el historial (F2) solo muestra el arranque.
- El bundle declara `LoadOnAutoCADStartup="True"`; el aviso apareció al arrancar y se pulsó «Load Once».
- Los módulos `RackCad.*` cargados salen todos de `%APPDATA%\Autodesk\ApplicationPlugins\RackCad.bundle\Contents\`, con los SHA-256 del inventario instalado:
  - Plugin `60e1dd44…`;
  - Application `f5aa8a57…`;
  - Domain `faae0407…`;
  - UI `f92fd60c…`.
- `RACKCANTILEVER` y `RACKRAMPAQUETE` funcionaron desde esa carga.

## 3. Validación de la tabla

- La tabla instalada en `Contents\structural-analysis\ram-elements\section-material-map.json` es `fc3d9146…b619`, igual a la fuente del checkout (`i/lf w/lf`, blob `d088e2a9…`).
- `verify-bundle.ps1` la comprueba (160).
- En ejecución, `RACKRAMPAQUETE` la encontró junto al DLL del bundle: dos paquetes `Valid`, cuando sin tabla sería `Unavailable`.

## 4. HB-04 (a/b)

- **(a)** Huella de entrada de `pkg-HB` = `f69400f658bea7b74a59d5cd963e05a777641a821c19206706e8ec689b6d15eb`, igual a H1 (`ov08\pkg-A`) y H2 (`ov08\pkg-B`). `rackId` `973e7734-…` y huella de fuente `2fb3c99f…` iguales.
  - Informativo: el script de `pkg-HB` difiere del de `pkg-B` solo en la línea de la carpeta.
  - El manifiesto difiere solo en `packageFolder` y `files`.
- **(b)** `pkg-HB2`: 117/117 (`p2_payload_vs_manual.py`, sin diferencias) y 16/16.
  - Informativo: `pkg-HB` también da 117/117.

## 5. Integridad del bundle

| Momento | Comprobación |
|---|---|
| Origen | 25 archivos, inventario `33e660b2…`, DLL `60e1dd44…`; `verify-bundle -BaselineInventoryPath` con 160 comprobaciones |
| Instalado | 160 comprobaciones; inventario `5d777353…` (ruta, SHA-256 y tamaño de los 25 archivos) |
| Antes de retirarlo | Inventario idéntico al instalado; la ejecución no escribió archivos en el bundle |

## 6. Estado de la desinstalación

**Archivos: restaurados.** La raíz del usuario de `ApplicationPlugins` está vacía, como en HB-01. No quedan staging, respaldos ni bundles fallidos. Las otras tres raíces no cambiaron.

**Registro: NO restaurado (STOP).** Claves nuevas escritas por AutoCAD a las **01:56:55.93Z**, al arrancar con el bundle. No las escribió el instalador.

```
HKCU\Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Applications\RackCad
  DESCRIPTION = "Comandos: RACKCAD, RACKSELECTIVO, … RACKAYUDA."
  LOADCTRLS   = 2
  LOADER      = %UserProfile%\AppData\Roaming\Autodesk\ApplicationPlugins\RackCad.bundle.\Contents\RackCad.Plugin.dll  (REG_EXPAND_SZ)
  MANAGED     = 1
HKCU\…\Applications\RackCad\Groups  (vacía)
HKCU\…\Loaded\C:/Users/<usuario>/AppData/Roaming/Autodesk/ApplicationPlugins/RackCad.bundle
  ProductCode = "C:/Users/<usuario>/AppData/Roaming/Autodesk/ApplicationPlugins/RackCad.bundle", Version = "1.0.0", Publisher = "RackCad"
HKCU\…\Loaded\…RackCad.bundle\#AssemblyMappings      RackCad.Plugin = ""
HKCU\…\Loaded\…RackCad.bundle\./Contents/RackCad.Plugin.dll   Type = 1, Name = "RackCad"
```

**Fuera de las exportaciones medidas (B-04):**

- `HKCU\…\AssemblyMap` tiene el valor `RackCad.Plugin` = `C:\Users\<usuario>\AppData\Roaming\Autodesk\ApplicationPlugins\RackCad.bundle\Contents\RackCad.Plugin.dll`. La clave se escribió a la misma hora. No hay BASELINE de su valor anterior; los NETLOAD de W-R1a pudieron dejar otro.
- `HKLM\SOFTWARE\Autodesk`: 0 coincidencias de «RackCad».

**Efecto previsible si se deja así:** `LOADCTRLS` = 2 pide cargar al arrancar un DLL que ya no existe. Lo que haga AutoCAD en el próximo arranque no se ha comprobado aquí.

**Opciones para el Owner (no se ha tocado nada):**

| Opción | Qué se hace | Valoración |
|---|---|---|
| **A (recomendada)** | Exportar a `hb\` y borrar con `reg delete` exactamente las dos claves nuevas (`Applications\RackCad` y `Loaded\…RackCad.bundle`, con sus subclaves). Después, un POST nuevo comparado con HB-01. | Esperado: `hkcu-applications` = `13430739…` y `hkcu-loaded` = `dd27f202…`. Determinista y sin abrir AutoCAD. El valor de `AssemblyMap` se deja como está (está fuera de B-04 y no hay BASELINE), salvo orden contraria. |
| B | Abrir AutoCAD una vez sin el bundle, para que el Autoloader limpie el registro de un bundle que ha desaparecido, y repetir el POST. | Depende de un comportamiento de Autodesk no verificado aquí. Es otra sesión de AutoCAD fuera de HB-01..05 y escribe MRU y posiciones de ventanas. |
| C | Conservar el residuo y aceptarlo como hallazgo. | No recomendada: queda una orden de carga al arranque hacia un DLL inexistente. |

**Hallazgo de producto, sin cambio de código:** desinstalar = borrar la carpeta no basta. El primer arranque con el bundle deja el registro del Autoloader en HKCU, y `deploy\install-bundle.ps1` no tiene desinstalación. La guía de despliegue debería documentarlo. Es material para una iniciativa futura que decide el Owner, no para el Candidato congelado.

## 7. BASELINE/POST y checkouts

**DIFF de HB (BASELINE 21:09:50Z frente al POST 02:05:4xZ):**

| Clase | Elementos |
|---|---|
| PROHIBIDO | `hkcu-applications` y `hkcu-loaded` (§6) |
| INFORMAR | `hkcu-profiles`: `Select File` `FileNameMRU0..5` cambiados, `FileNameMRU6` nuevo, `VIEWBACKSTATUS` nuevo |
| Esperado | X-02: existen `pkg-HB` y `pkg-HB2` |
| Iguales | `acad`/`ram` = 0, perfil, `TRUSTEDPATHS`, `SECURELOAD`, `FixedProfile`, `InstalledApplications`, HKLM `Applications`, `acad.exe`, las cuatro raíces de `ApplicationPlugins`, variables, ACL, DLL Debug, manifiesto, tabla, cargas, valores esperados, `%APPDATA%\RackCad` y la biblioteca |

**Checkouts:**

- K1 (21:09:36Z) = K2 (02:06Z): 7 worktrees, `main` con 2151 archivos rastreados, digest `d0105e96…`; la comparación no da STOP.
- `git diff 4a03b65b..HEAD -- deploy assets src` vacío.
- Identidades del Candidato: 63/63 con fetch fresco a las 21:09Z y a las 01:54Z.

## 8. Incidencias

1. **Parada del preflight a las 21:16Z.** El equipo estuvo en uso hasta ~21:14Z. No se instaló nada; el Owner liberó el host a las ~01:53Z.
2. **`declaration_recheck` a las 01:54:25Z: 62/63.** Solo falló «árbol limpio», por el original nuevo sin versionar (la herramienta solo exceptúa el original de la declaración). Se apartó al scratchpad (copia idéntica) y se repitió: 63/63.
3. **Primer `hb05b_uninstall.ps1`: error de mi script.** En PowerShell, `$l` y `$L` son la misma variable. Abortó antes de borrar; el bundle quedó intacto. Se corrigió y se conserva la versión fallida.
4. **`reg_lastwrite.ps1`: error de conversión de un literal.** Es de solo lectura. Se corrigió.
5. **Avisos a I-64 e I-52 no entregables:** sus sesiones ya no estaban abiertas. El Owner había dispensado sus confirmaciones.
6. **`Drawing1` se cerró solo.** AutoCAD cierra el `Drawing1` vacío al abrir `hb-ref.dwg` (comportamiento estándar). HB-04 (b) usó `Drawing2.dwg`, creado con la pestaña «+» (QNEW), según CD-I65-FC-50 (b).
7. **«Actualizar» deshabilitado.** La línea ya estaba resuelta al aceptar el brazo, como en W-R1a; «Insertar frontal» se hizo igual.
8. **Sin técnicas fuera de Computer Use:**
   - no se usó `SetForegroundWindow`;
   - no se usó el portapapeles: textos de 14 caracteres como máximo y rutas en trozos de 10;
   - no se llamó a `open_application` con AutoCAD abierto;
   - en el BenQ llegaron los clics simples;
   - la ventana de texto (F2) se cerró con su botón.
9. **Identificador de captura corregido en la línea de tiempo antes de custodiarla** (`gyum94`).
10. **Privacidad (§90):** la captura del aviso de seguridad tapa el nombre de la cuenta en la ruta; la del diálogo «Select File» tapa el nombre de la organización de OneDrive. Los originales quedan en local con su SHA-256.

## 9. CI y custodia

- La custodia está en este directorio y en la evidencia §122.
- `verify_custody_rows.py` da 0 diferencias.
- La CI del commit se informa tras el push.
- Intentos de W-R1b: 2 ejecuciones de `RACKRAMPAQUETE` (`pkg-HB`, `pkg-HB2`), una cada una. Codex 55/57 y `attempts` 7/11, sin consumo. RAM no iniciado.

## 10. Recomendación

**W-R1b = NO-GO como PASS en su estado actual.**

- WR1-AC-05 exige «restauración exacta de lo protegido», y HB-05c no la cumple por el registro del Autoloader.
- El resto de los oráculos de HB se cumplen. En particular:
  - la carga real desde el Autoloader;
  - la tabla junto al DLL;
  - huella = H2 = H1;
  - 117/117;
  - la integridad del bundle;
  - la retirada de los archivos.

**Propuesta:**

1. El Owner autoriza la opción A.
2. Se hace un POST nuevo, `hb\post-2`.
3. Si `hkcu-applications` y `hkcu-loaded` vuelven a `13430739…` y `dd27f202…`, con 0 referencias `RackCad.bundle` y el resto igual, el Coordinator propondrá **W-R1b = PASS con el hallazgo de §6**.

W-R2 = NO; OWNER VALIDATION FINAL = PENDING; INTEGRATION = NO. Me detengo: no toco el registro ni la pantalla sin orden.

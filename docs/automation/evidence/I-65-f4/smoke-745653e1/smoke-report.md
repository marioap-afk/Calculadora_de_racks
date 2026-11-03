# I-65 F4 — smoke de `RACKRAMPAQUETE` en AutoCAD (custodia, 2026-10-14)

- **Autoridad:** «OWNER HOST RELEASE — Exclusive AutoCAD Smoke Window»
  ([texto literal](../../../decisions/I-65-owner-host-release-smoke.original.txt)); decisiones §58. OWNER HOST RELEASE = APPROVED; SINGLE
  NETLOAD = AUTHORIZED; AUTOCAD SMOKE = AUTHORIZED; RAM EXECUTION = NO; MAPPING UPDATE = NO; F4 GATE PASS = NO.
- **Ejecutor:** Coordinator con Computer Use (acceso concedido solo a «AutoCAD 2025 - English»). Carpeta local de la corrida (no
  versionada): `D:\r65-smoke\745653e1\run-R20261014T0312Z\` (BASELINE, POST, exportaciones del registro, capturas).
- Procedimiento: [preflight/POST](../../../../initiatives/I-65-f4-host-release-and-preflight.md) y
  [smoke](../../../../initiatives/I-65-f4-autocad-smoke-procedure.md), con el alcance exclusivo de CD-I65-F4-84.

## 1. Horas (UTC)

| Hito | Hora |
|---|---|
| Aviso previo a I-52 e I-64 | ~03:05Z |
| I-64: «SIN STOP» (sin sesión de AutoCAD, sin ventana reservada) | 03:08:39Z |
| BASELINE de solo lectura | 03:09:53Z |
| Apertura de AutoCAD | 03:11:32Z |
| Hash del DLL comprobado inmediatamente antes de NETLOAD | 03:15:29Z |
| NETLOAD con «Load Once» | ~03:15:40Z |
| Resultado de `RACKRAMPAQUETE` | ~03:21:08Z |
| Carpeta del paquete inexistente | 03:21:19Z |
| AutoCAD cerrado sin guardar; `acad.exe` = 0 | 03:21:50Z |
| POST de solo lectura y DIFF | 03:22:17Z |
| Checkouts (K1/K2) sin cambios; aviso de cierre a I-52 e I-64 | 03:23Z |

## 2. Referencias del preflight (BASELINE; [resumen](baseline-summary.txt))

`acad.exe` 0 y `RAMElements.exe` 0; perfil `<<Unnamed Profile>>` (1 perfil); `TRUSTEDPATHS` 19 entradas,
`a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498`; `SECURELOAD` 1; `acad.exe` R25.0.171.0.0,
`2a75996fd2a5c5ee0376fd5ba7cced227a098193ff495e8ceeda3a96fe326422`; los seis árboles del registro exportados (los mismos SHA-256 que la
validación en seco); `ApplicationPlugins` sin RackCad; 0 variables `RackCad`; listados `icacls` de `AcadLocation`, de la carpeta del DLL y
de las cuatro rutas de I-52 iguales a los de referencia; DLL `2e04a4bb…e41d` y manifiesto de salida `14e860d1…dc5b9` (29 archivos);
`%APPDATA%\RackCad` con 15 archivos (listado `4af06c15…1220`). Ningún elemento protegido difería: preflight limpio.

## 3. DLL cargado

`C:\Users\alejandra-mendoza\.codex\worktrees\architecture-interop-analisis-estructural\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`,
SHA-256 **`2e04a4bb47b04012fab1fd04b0c9d11e5ec3dcaf01ba1aabbb9764a9c5c6e41d`** (commit `745653e1`, ProductVersion
`1.0.0+745653e1…`), comprobado en la BASELINE, justo antes de cargar y en el POST. AutoCAD mostró el aviso estándar «Security - Unsigned
Executable File» (no está en una carpeta de confianza); se eligió **«Load Once»**, nunca «Always Load»
([captura 03](03-aviso-seguridad-load-once.jpg)). El diálogo de NETLOAD se abrió en la última carpeta usada, que era la salida de otra
iniciativa: no se cargó nada de ahí y se navegó hasta la carpeta de I-65 ([captura 02](02-netload-dll-i65-seleccionado.jpg)).

## 4. Fixture y resultado visible

- Dibujo nuevo desechable `Drawing1.dwg`. `RACKCANTILEVER`: nombre `REF-CANT-01`, 2 estaciones, separación 48, una cara Y+, 3 niveles,
  troquel 0, claro 24; columna y base `AISC-W-W12X26`, base 48; brazo sencillo `AISC-HSS-RECT-HSS4X4X_250`, corte 36. La línea se resolvió
  (altura de columna 90.5 in) y se insertó la vista frontal como bloque `RACKCAD_CANTILEVER_REF-CANT-01_-_frontal_FRONTAL`
  ([capturas 05-08](07-linea-ref-cant-01-resuelta.jpg)). Aviso no bloqueante de INSUNITS (I-05), sin cambiar nada.
- `RACKRAMPAQUETE` → rack → estación `0` → archivo de cargas del piloto
  (`tests/RackCad.Tests/StructuralAnalysis/Fixtures/ref-cant-01.loads.json`) → carpeta nueva `D:/r65-smoke/745653e1/run-R20261014T0312Z/pkg-01`.
- **Salida literal de la línea de comandos** ([capturas 13-14](14-resultado-unsupported-detalle.jpg)):

```text
RackCad paquete estructural: Unsupported. El modelo estructural de la estacion no es valido; no se escribio nada.
  Blocking MATERIAL-ROW-UNVERIFIED (AISC-HSS-RECT-HSS4X4X_250)
  Blocking MATERIAL-ROW-UNVERIFIED (AISC-W-W12X26)
```

Sin «Huella de entrada» ni «Archivo escrito»: **rechazo seguro con las filas `proposed`**, como fija el contrato de F4-T5.

## 5. Ausencia de paquete

La carpeta `pkg-01` no existe después del comando (comprobado a las 03:21:19Z y en el POST, `X-02 package_folder_exists=False`); la
carpeta de la corrida solo contiene lo que escribió la sesión (BASELINE, POST, capturas).

## 6. POST y DIFF ([resumen POST](post-summary.txt); [claves cambiadas](diff-registry-keys.txt))

| Clase | Resultado |
|---|---|
| **PROHIBIDO** | **ninguno**: perfil, `TRUSTEDPATHS`, `SECURELOAD`, `acad.exe`, `Applications`, `InstalledApplications` y `Loaded` (HKCU), `Applications` (HKLM), `ApplicationPlugins`, variables `RackCad`, listados `icacls` (incluidas las rutas de I-52), DLL y manifiesto: idénticos |
| **INFORMAR** | `Profiles\<<Unnamed Profile>>\Dialogs\NetLoadDialog`: `FileNameMRU0..10` e `InitialDirectory` (historial del diálogo de NETLOAD; no es carga automática ni ruta de confianza); `FixedProfile\Application Window`: `Position` |
| Otros | `%APPDATA%\RackCad` sin cambios (mismo listado); checkouts sin cambios (K1/K2) |

Resultado del DIFF: **INFORMAR** (solo estado de interfaz); I-52 decide si afecta a su tupla (PC-05 del smoke).

## 7. Incidencias

1. **Portapapeles y `PASTECLIP`:** la primera ruta del archivo de cargas se tecleó a través del portapapeles. AutoCAD interpretó el Ctrl+V
   como `PASTECLIP`, canceló la petición en curso de `RACKRAMPAQUETE` (sin escribir nada) e inició `PASTECLIP`, que se canceló con Esc sin
   pegar nada ([capturas 10-11](10-incidente-pasteclip.jpg)). El comando se repitió escribiendo la ruta en trozos cortos con barras `/`.
   El contenido previo del portapapeles del usuario quedó reemplazado.
2. **BASELINE:** dos intentos previos fallaron por errores de PowerShell de la sesión (atributo `[ordered]` en línea y el alias `h` =
   `Get-History`); el segundo imprimió en la transcripción de la sesión la cadena `TRUSTEDPATHS` y listados `icacls` en claro, sin
   versionarlos. Ningún efecto en el host; la BASELINE válida es la de 03:09:53Z.
3. **Pendiente del brazo:** el editor muestra la pendiente por omisión como `0.438` (7/16 = 0.4375) y el resumen del brazo como
   «0.44/12». No afecta al smoke, que falla antes por las filas; la igualdad exacta del dibujo con `REF-CANT-01` se valida en la OV, no
   aquí. El dibujo se descartó sin guardar.
4. **I-52 no contestó al aviso previo** antes de la apertura. La reconfirmación se apoyó en `acad.exe` = 0, en las ramas de I-52 e I-64 sin
   cambios y en la declaración de hechos de I-52 a las 02:48Z (sin sesión de AutoCAD, P2 H10 sin autorización ni ventana, sin otras
   operaciones reservadas), dentro del RELEASE del Owner.

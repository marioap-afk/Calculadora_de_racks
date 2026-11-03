# I-65 — F5 W3 (AutoCAD): informe de la ventana `f5-R20261019T1750Z` (OV-I65-02, parte AutoCAD)

Orden: disposición del Owner «Privacy Hold Resolution / F5 W3 AutoCAD Authorization» (decisiones §90, W3 = CONDITIONAL GO).
Procedimiento: [I-65-f5-w3-w4-procedures.md](../../../../initiatives/I-65-f5-w3-w4-procedures.md) §3 (Candidato `bd1fcf8a`) y
[anexo operativo](../../../../initiatives/I-65-f5-w3-w4-operational-annex.md) (§1-§6). Operador: Claude (Coordinator) con Computer Use.
Carpeta local de la corrida: `D:\r65-smoke\bd1fcf8a\f5-R20261019T1750Z\`.

## 1. Resultado

**W3 = PASS** (propuesto; la aceptación es del Owner). Todos los oráculos de la autorización (§90, CD-I65-F5-33) se cumplen:

| Oráculo | Resultado | Evidencia |
|---|---|---|
| Paquete `Valid` | **CUMPLE**: «RackCad paquete estructural: Valid. Paquete publicado en D:\r65-smoke\bd1fcf8a\f5-R20261019T1750Z\pkg-OV02»; tres «Archivo escrito»; ningún diagnóstico | captura 10; `timeline.txt` |
| Validador v2 16/16 | **CUMPLE**: `PASS 16 / 16` | `validator-v2-pkg-OV02.redacted.json` |
| Seis cargas correctas en magnitud, posición y dirección | **CUMPLE**: 6/6 IGUAL a la tabla aprobada (preparación §4); miembros `ARM.1/2/3.PositiveY`, `GLOBAL_Y` con valor negativo; diferencias ≤ 1.4e-14 | `payload-vs-table-pkg-OV02.txt` |
| Huella de entrada consistente | **CUMPLE**: consola = manifiesto = `PAYLOAD` = `8f36e3ce24d508e8e7ec01ea70f0dd6afc6cdfe7e37d9e842b17b5880a6f1d87`; V-12 (la lectura de vuelta repite la huella) | consola, manifiesto, validador |
| Q2 con la posición calculada desde la fracción 1.0 | **CUMPLE**: Q2 en `ARM.1.PositiveY` al **99.99999999999997 %** (igual que en las comprobaciones en seco; CD-I65-F5-08), valor −337.2134146495658 lbf | `payload-vs-table-pkg-OV02.txt` |
| Sin cambios PROHIBIDO en el host | **CUMPLE**: DIFF = INFORMAR (MRU e `InitialDirectory` del diálogo NETLOAD, `Bounds` de la ventana Cantilever, posición de la ventana principal) | `diff-registry-keys.txt`, `diff-classification.txt` |
| Integridad de la DLL, la tabla y el paquete | **CUMPLE**: DLL `81299b87…fef5` y tabla `fc3d9146…` iguales en la BASELINE, antes de NETLOAD, en las listas de módulos y en el POST; paquete de 3 archivos sin temporales; `structural-loads.json` = `d675d9f6…` | `f5-identity-*.txt`, `module-list-*.txt` |

Además: la carga útil es **idéntica** a la de la comprobación en seco con las DLL del Candidato en `nodes`, `members`, `loads`, `loadCase`,
`units`, `tolerances`, `capabilities`, `schema` y `adapterVersion`; solo cambian `packageFolder` y la huella, que dependen de la corrida
(`payload-vs-dryrun-pkg-OV02.txt`). K1/K2: sin cambios en ningún checkout.

## 2. Cronología (UTC, 2026-10-19)

| Hora | Hito |
|---|---|
| ~17:46 | Confirmación del Coordinator de I-64 pegada por el Owner en el chat de I-65; a las 17:50Z el Owner renuncia expresamente al reenvío por el canal de I-64 («no es necesario tanta burocracia, nomás hazlo») |
| 17:43-17:52 | I-52: sin STOP; avisos previos a I-52 e I-64 |
| 17:50:53 | BASELINE (`w1_hostsnap.ps1` sin modificar) + identidad del Candidato de F5 + K1 |
| 17:51:31 | AutoCAD abierto (pid 29152) |
| 17:52:24-17:53:48 | **Incidencia**: segunda `acad.exe` lanzada por el operador (pid 13364) y cerrada con `CloseMainWindow` (Drawing1 sin cambios, sin NETLOAD) |
| 17:54:17 | Dibujo nuevo desechable `Drawing1` |
| 17:55:24 / 17:55:58 | SHA-256 de la DLL antes de cargar; NETLOAD «Load Once» (diálogo navegado con doble clic) |
| 17:58:15 / 17:58:39 | REF-CANT-01 resuelta e insertada (bloque `RACKCAD_CANTILEVER_REF-CANT-01_-_frontal_FRONTAL`, 310 contornos) |
| 18:00:03 / 18:00:23 | `RACKRAMPAQUETE` (estación 0, cargas de OV-I65-02, carpeta nueva) → `Valid` |
| 18:01:09 | AutoCAD cerrado sin guardar; `acad.exe` = 0, `RAMElements.exe` = 0 |
| 18:01:12 | POST y DIFF; K2; avisos de cierre a I-64 e I-52 |

Ventana: unos 10 minutos con AutoCAD abierto (límite: 60). Presupuesto: **1 NETLOAD, 1 generación, 0 reintentos, 0 cambios de código,
Codex 0, Workers 0**.

## 3. Construcción de REF-CANT-01 (como W1 §4.1)

Nombre `REF-CANT-01`; 2 estaciones a 48 in; caras Sencilla, Lado +Y; 3 niveles, troquel 0, claro 24 in; columna y base
`AISC-W-W12X26` (base que sigue a la columna, longitud de base 48); brazo Sencillo `AISC-HSS-RECT-HSS4X4X_250`, corte 36 in, pendiente
por omisión 0.438, placas por omisión; resto por omisión. Resumen: «2 estaciones · 1 intervalos · altura común 90.5 in · 1 paneles
arriostrados · 2 separadores · 2 tensores · 4 componentes en el BOM». Aviso INSUNITS no bloqueante (I-05).

## 4. Incidencias

1. **Doble arranque de AutoCAD (17:52-17:53Z).** La primera ventana se abrió en el otro monitor y el operador volvió a pedir la
   apertura, lo que lanzó una segunda `acad.exe`. Se cerró de forma ordenada antes de cualquier NETLOAD o dibujo de prueba; desde las
   17:53:48Z solo hubo una instancia. No afecta a los oráculos; el DIFF no muestra cambios atribuibles más allá de las clases INFORMAR.
2. **Diálogo de NETLOAD:** Computer Use no admite teclear la ruta en este diálogo; se navegó con «subir nivel» y doble clic hasta
   `D:\r65-smoke\bd1fcf8a\bin-Debug-net8.0-windows` y se seleccionó `RackCad.Plugin.dll` (captura 02).
3. **Primer clic de selección del rack** sin objeto («Nothing Selected»); el segundo, sobre un tensor, lo seleccionó. No es un intento.

## 5. Privacidad (política de §90)

Capturas recortadas a la zona técnica (sin barra de título, panel de navegación ni barra de tareas). El manifiesto y el informe del
validador contienen el nombre del verificador de la tabla: se versionan copias con ese dato redactado y se custodian los SHA-256 de
los originales, que quedan en la carpeta local. Barrido de secretos y datos personales de esta carpeta: `privacy-secrets-scan.txt`.

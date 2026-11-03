# I-65 F4 — W1 (AutoCAD) de la validación final sobre el Candidato de hito `5a5ca577` (custodia, 2026-10-17)

- **Autoridad:** «F4 Final Validation — W1 AutoCAD Preparation and Conditional Execution»
  ([texto literal](../../../decisions/I-65-owner-authorization-w1-autocad-final-validation.original.txt)); decisiones §78-§79.
  W1 AUTOCAD EXECUTION = CONDITIONAL GO; W2 RAM EXECUTION = NO; PRODUCT CODE CHANGES = NO; F4 GATE PASS = NO.
- **Procedimiento:** [I-65-f4-w1-autocad-procedure.md](../../../../initiatives/I-65-f4-w1-autocad-procedure.md) (`afa10180`, CI 39004356219 4/4).
- **Ejecutor:** Coordinator con Computer Use (acceso concedido solo a «AutoCAD 2025 - English»); sin Codex ni Workers; sin RAM.
- **Carpeta local de la corrida** (no versionada entera): `D:\r65-smoke\5a5ca577\ov-R20261017T0847Z\` (`baseline\`, `post\`, `captures\`,
  `pkg-OV01\`, `pkg-OV04\`, `inj-OV05\`, `inj-OV06\`, `inj-OV07a\`, `inj-OV07b\`, `dump-OV01-gen\`). Las exportaciones del registro no se versionan.

## 1. Condiciones de entrada (W1-PR-01..09)

| # | Resultado |
|---|---|
| W1-PR-01 | procedimiento en `origin` (`afa10180`) |
| W1-PR-02 | avisos a I-52 e I-64 ([coordinación](../host-coordination-w1-2026-10-17.md)) |
| W1-PR-03 | **I-64 HOST COORDINATION ACK = CONFIRMED — NO CONFLICT FOR I-65 W1**, recibida a las 08:45Z (la primera, para la ventana 05:10Z-06:40Z, llegó caducada y no se usó); I-52 sin STOP |
| W1-PR-04 | `acad.exe` = 0 y `RAMElements.exe` = 0 (BASELINE 08:48:09Z) |
| W1-PR-05 | inactividad del usuario 155.4 s a las 08:49:50Z, proceso en primer plano `claude` |
| W1-PR-06 | BASELINE igual a la del segundo smoke en perfil (1, `<<Unnamed Profile>>`), `TRUSTEDPATHS` (19, `a9ba0fd1…`), `SECURELOAD` 1, `acad.exe` R25.0.171.0.0 `2a75996f…`, Applications/InstalledApplications/Loaded (HKCU), Applications (HKLM), `ApplicationPlugins` sin RackCad, 0 variables `RackCad` y las cuatro rutas de ACL de I-52. **ACL de `AcadLocation`:** el hash cambió (`d1bcd226…` frente a `7d64e54b…`) solo porque el script se lanzó con la consola en la página 850; con la consola en UTF-8 (65001) da `7d64e54b…`, la referencia ([conciliación](baseline-acl-codepage-reconciliation.txt)). La ACL no cambió; el POST se hizo por el mismo camino |
| W1-PR-07 | DLL `c72b759d…ac07`, salida 32 archivos = manifiesto `ed2875e4…`, tabla `fc3d9146…b619`, valores esperados `b3d101c1…`, biblioteca de bloques `b4ca2248…60e8`, los seis archivos de cargas con los SHA-256 del procedimiento |
| W1-PR-08 | sin diferencias protegidas; ninguna carpeta de paquete existía |
| W1-PR-09 | K1 hecha |

## 2. Horas reales (UTC)

| Hito | Hora |
|---|---|
| BASELINE | 08:48:09Z |
| Aviso previo a I-52 e I-64 | ~08:50Z |
| Apertura de AutoCAD (una instancia, PID 26824) | 08:50:10Z |
| SHA-256 de la DLL antes de NETLOAD / antes de «Open» | 08:51:35Z / 08:52:24Z |
| NETLOAD «Load Once»; lista de módulos | ~08:52:30Z; 08:52:47Z |
| REF-CANT-01 insertada | 08:55:26Z |
| OV-01 `Valid` / OV-04 `Valid` | 08:56:21Z / 08:57:13Z |
| OV-03 (a) / (b) / (c) / (d) / (e) | 08:59:37Z / 09:01:46Z / 09:05:05Z / 09:07:35Z / 09:10:10Z |
| Lista de módulos antes de cerrar | 09:10:19Z |
| AutoCAD cerrado sin guardar; `acad.exe` = 0 | ≤ 09:10:53Z |
| POST y DIFF; K2 | 09:11Z |
| Aviso de cierre a I-52 e I-64 | ~09:11Z |

## 3. DLL cargada

`NETLOAD` abrió en la carpeta de la DLL del segundo smoke (`51fc82f1`, archivos de las 12:54 locales): no se cargó nada de ahí. Se
subió a `D:\r65-smoke\`, se entró en `5a5ca577\bin-Debug-net8.0-windows\` y se eligió `RackCad.Plugin.dll`
([captura 02](captures/02-netload-dll-candidato-seleccionado.jpg)). Aviso estándar «Security - Unsigned Executable File» con la
ubicación `D:\r65-smoke\5a5ca577\bin-Debug-net8.0-windows` → **«Load Once»** ([captura 03](captures/03-aviso-seguridad-load-once.jpg)).
Lista de módulos de `acad.exe`: tras cargar, un único `RackCad.Plugin.dll` desde esa carpeta con `c72b759d…ac07`
([lista](module-list-after-netload.txt)); antes de cerrar, `RackCad.Plugin/UI/Application/Domain.dll`, todos desde esa carpeta y con los
SHA-256 del manifiesto ([lista](module-list-before-close.txt)).

## 4. Dibujo

Un único dibujo nuevo desechable, `Drawing1.dwg` ([captura 01](captures/01-dibujo-nuevo-drawing1.jpg)), cerrado sin guardar
([captura 31](captures/31-cierre-sin-guardar.jpg)). Aviso no bloqueante de INSUNITS (I-05) sin cambiar nada. Líneas insertadas, cada una
como bloque `RACKCAD_CANTILEVER_<nombre>_-_frontal_FRONTAL`:

| Línea | Construcción | Resumen de la línea resuelta | Contornos |
|---|---|---|---|
| `REF-CANT-01` | §4.1 del procedimiento ([columna y base](captures/04-ref-columna-base-w12x26.jpg), [brazo](captures/05-ref-brazo-hss4x4x1_4.jpg)) | «2 estaciones · 1 intervalos · altura común 90.5 in · 1 paneles arriostrados · 2 separadores · 2 tensores · 4 componentes en el BOM» ([captura 06](captures/06-ref-linea-resuelta.jpg)) | 310 |
| `OV03-A-DOBLE` | REF-CANT-01 con Caras = Doble | igual; matriz con niveles +Y y −Y ([captura 13](captures/13-ov03a-linea-doble-resuelta.jpg)) | 392 |
| `OV03-B-NEGY` | REF-CANT-01 con Lado = −Y | igual; matriz N1-N3 −Y ([captura 16](captures/16-ov03b-linea-negy-resuelta.jpg)) | 310 |
| `OV03-C-PEND` | REF-CANT-01 con pendiente 3 por 12 ([brazo](captures/19-ov03c-brazo-pendiente-3.jpg)) | igual; brazo «36 in · 3/12» ([captura 20](captures/20-ov03c-linea-resuelta.jpg)) | 310 |
| `OV03-D-CANAL` | REF-CANT-01 con canal doble enfrentada `AISC-C-C4X5_4` ([brazo](captures/23-ov03d-brazo-canal-doble-c4x5_4.jpg)) | igual; matriz C4X54 ([captura 24](captures/24-ov03d-linea-resuelta.jpg)) | 310 |
| `OV03-E-SINFILA` | REF-CANT-01 con brazo `AISC-HSS-RECT-HSS4X4X_313` ([brazo](captures/27-ov03e-brazo-hss4x4x5_16.jpg)) | igual; matriz RECT-HSS4X4X313 ([captura 28](captures/28-ov03e-linea-resuelta.jpg)) | 310 |

## 5. Resultados (un único intento de `RACKRAMPAQUETE` por caso; estación `0`)

### OV-I65-01 — `pkg-OV01` ([capturas 08-10](captures/10-ov01-consola-valid.jpg))

```text
Archivo de cargas: D:\r65-smoke\5a5ca577\inputs\ref-cant-01.loads.json
Carpeta NUEVA del paquete: D:\r65-smoke\5a5ca577\ov-R20261017T0847Z\pkg-OV01
RackCad paquete estructural: Valid. Paquete publicado en D:\r65-smoke\5a5ca577\ov-R20261017T0847Z\pkg-OV01
  Huella de entrada: f5ce900c14ee768bb85670e814099d58947857778077ced8f81d371a8858d663
  Archivo escrito: manifest.json
  Archivo escrito: rackcad-ram-build.py
  Archivo escrito: structural-loads.json
```

Validador v2 **PASS 16/16** ([informe](validator-v2-pkg-OV01.json)); huella de la consola = `analysisInputFingerprint` del manifiesto;
`rowsSha256` `54ca9f6c…98c`; cargas `25f48e7f…`; `rackId` `89fcc4ba-b78a-41b6-8985-9a583c24f989`. **Carga útil = especificación de
REF-CANT-01: IGUAL, 117 comprobaciones, 0 diferencias** ([contraste](payload-vs-manual-pkg-OV01.json)). 3 archivos, sin `*.tmp-*`.

### OV-I65-04 — `pkg-OV04` ([capturas 11-12](captures/12-ov04-consola-valid.jpg))

```text
RackCad paquete estructural: Valid. Paquete publicado en D:\r65-smoke\5a5ca577\ov-R20261017T0847Z\pkg-OV04
  Huella de entrada: f5ce900c14ee768bb85670e814099d58947857778077ced8f81d371a8858d663
  Archivo escrito: manifest.json
  Archivo escrito: rackcad-ram-build.py
  Archivo escrito: structural-loads.json
```

Validador v2 **PASS 16/16** ([informe](validator-v2-pkg-OV04.json)). Misma huella que OV-01 (mismo rack, misma estación, mismas cargas;
la carpeta no entra en la huella). 3 archivos, sin `*.tmp-*`.

### OV-I65-03 — rechazos (en todos, `Test-Path` de la carpeta = `False` y 0 hermanos `*.tmp-*`, también en el POST)

| Caso | Consola (literal tras la carpeta tecleada) | Esperado | Resultado |
|---|---|---|---|
| (a) doble cara, `ov03a-double-face.loads.json` ([captura 15](captures/15-ov03a-consola-unsupported.jpg)) | `RackCad paquete estructural: Unsupported. El modelo estructural de la estacion no es valido; no se escribio nada.` / `  Blocking STATION-FACE-UNSUPPORTED (station-0)` | `STATION-FACE-UNSUPPORTED` | **PASS** |
| (b) brazo `NegativeY`, `ov03b-negative-y.loads.json` ([captura 18](captures/18-ov03b-consola-unsupported.jpg)) | `… Unsupported. …` / `  Blocking STATION-SIDE-UNSUPPORTED (station-0)` | `STATION-SIDE-UNSUPPORTED` | **PASS** |
| (c) pendiente 3 por 12, `ov03c-slope-over-10deg.loads.json` ([captura 22](captures/22-ov03c-consola-unsupported.jpg)) | `… Unsupported. …` / `  Blocking ARM-SLOPE-UNSUPPORTED (ARM.1.PositiveY)` / `(ARM.2.PositiveY)` / `(ARM.3.PositiveY)` | `ARM-SLOPE-UNSUPPORTED` ×3 | **PASS** |
| (d) canales pareados, `ov03d-paired-channels.loads.json` ([captura 26](captures/26-ov03d-consola-unsupported.jpg)) | `… Unsupported. …` / `  Blocking ARM-ARRANGEMENT-UNSUPPORTED (ARM.1.PositiveY)` / `(ARM.2.PositiveY)` / `(ARM.3.PositiveY)` | `ARM-ARRANGEMENT-UNSUPPORTED` ×3 | **PASS** |
| (e) perfil sin fila, `ov03e-section-without-row.loads.json` ([captura 30](captures/30-ov03e-consola-unsupported.jpg)) | `… Unsupported. …` / `  Blocking MATERIAL-ROW-MISSING (AISC-HSS-RECT-HSS4X4X_313)` | `MATERIAL-ROW-MISSING` | **PASS** |

Ningún `Invalid` por cargas ni por carpeta; ningún código distinto del esperado. El caso «fila sin verificar» no se ejecutó en AutoCAD:
lo cubren las pruebas de fixture del Candidato (decisiones §77, CD-I65-F4-186).

## 6. POST y DIFF ([BASELINE](baseline-summary.txt), [POST](post-summary.txt), [nombres de valor](diff-registry-keys.txt))

Clase **INFORMAR** (las mismas que en los smokes anteriores, aceptadas por I-52): `NetLoadDialog` `FileNameMRU0-10` e `InitialDirectory`,
`FixedProfile\Application Window\Position` y `Variables\VIEWBACKSTATUS` borrado por AutoCAD al salir. **Ningún PROHIBIDO:** perfil,
`TRUSTEDPATHS`, `SECURELOAD`, Applications/InstalledApplications/Loaded (HKCU), Applications (HKLM), `ApplicationPlugins`, variables y ACL
sin cambios; DLL, salida, tabla y cargas con los mismos SHA-256. Cambios esperados: existen `pkg-OV01` y `pkg-OV04`. K1/K2: sin hallazgos
(`stop: false`, [comparación](k1-k2-compare.json)). Avisos de cierre enviados a I-52 e I-64.

## 7. Incidencias de operación (ninguna afecta a un intento)

1. En el caso (c), el triple clic sobre la pendiente `0.438` solo seleccionó «438» y quedó `0.3`; se corrigió a `3` con Ctrl+A **antes**
   de exportar (corrección de datos en la ventana, no un intento; procedimiento §7).
2. En el caso (c), un clic de maximizar la ventana de columna no se aplicó; los datos se comprobaron campo a campo antes de seguir.
3. En el caso (e), el primer clic para elegir el rack no tocó la geometría y AutoCAD volvió a pedir el rack; el segundo sí.
4. El hash de la ACL de `AcadLocation` depende de la página de códigos de la consola (§1, W1-PR-06).

## 8. Aceptación de W1 (procedimiento §5)

| # | Resultado |
|---|---|
| W1-AC-01 | **Cumple:** misma DLL en la BASELINE, antes de cargar, en la lista de módulos (dos veces) y en el POST |
| W1-AC-02 | **Cumple:** OV-01 y OV-04 `Valid`, completos y 16/16 |
| W1-AC-03 | **Cumple:** carga útil de `pkg-OV01` 117/117 |
| W1-AC-04 | **Cumple:** OV-03 (a)-(e) con el rechazo esperado y sin carpeta |
| W1-AC-05 | **Cumple:** AutoCAD cerrado sin guardar; RAM nunca iniciado |
| W1-AC-06 | **Cumple:** DIFF sin PROHIBIDO |
| W1-AC-07 | **Cumple:** K1/K2 sin cambios |

**W1 = SATISFACTORIA.** No es GATE PASS de F4: faltan W2 (OV-01 en RAM, OV-04..07), la revisión del Architect y la orden del Owner.

## 9. Después de W1 (sin AutoCAD ni RAM; [registro](after-w1-prep.txt))

- Inyecciones con la herramienta del Candidato (`0b56e198…`/`d421c82b…`), sin `--embed-folder`, cada una en su carpeta nueva y con la
  marca `rackcadFaultInjection` en el manifiesto: `inj-OV05` (`unknown-section`: `HSS_SQR 4X4X1_4` → `HSS_SQR INEXISTENTE-99`), `inj-OV06`
  (`inverted-axes`: eje local 2 del primer miembro invertido), `inj-OV07a` (`wrong-version`: `0.1.0` → `999.0.0`) e `inj-OV07b`
  (`missing-core-function`: `rackcad_nonexistent_function`). Código 0 en las cuatro. **`pkg-OV01` intacto:** los mismos tres archivos y
  SHA-256 antes y después.
- Script de volcado de W2: `dump-OV01-gen\rackcad-ram-dump.py` (`56faa355…`), con `dump-script --adapter-version 1.0.0`. Frente a
  `manual-rackcad-ram-dump.py.txt` de P2 (`af262489…`): 213 líneas en ambos, solo cambia la línea 11 (`PAYLOAD`) y en ella solo
  `dumpFolder` (**condición (a) de A65-F4C-05 cumplida**).
- RAM, solo lectura ([estado](w2-readiness-readonly.txt)): `RAMElements.exe` `7dcb4855…` (26.00.01.15), `ramelements` 0.1.0 `f71a017f…`,
  `Config.ini` `2c593957…` (el de después de P2) con `PythonScripts` apuntando a `dump-man` de P2 (adenda §11, A65-F4C-07: comprobar la
  ruta y el hash del script antes de cada «Ejecutar archivo…»).

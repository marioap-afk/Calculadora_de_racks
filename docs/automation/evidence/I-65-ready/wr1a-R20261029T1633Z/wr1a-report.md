# I-65 READY — Informe de W-R1a sobre `FINAL_CANDIDATE_SHA` = `4a03b65b` (2026-10-29)

- **Autorización:** disposición del Owner «FINAL_CANDIDATE_SHA Declaration / W-R1a Authorization» (decisiones §117): W-R1a = partes A, B y D de los [procedimientos](../../../../initiatives/I-65-ready-wr1-wr2-procedures.md) §3; W-R1b, W-R2, Owner Validation e integración = NO.
- **Candidato:** `4a03b65b375d468c220dd68400d5dfd955bf043e`; DLL `D:\r65-smoke\4a03b65b\bin-Debug-net8.0-windows\RackCad.Plugin.dll` `0f5a508e…0959`; tabla `fc3d9146…b619`; AutoCAD 2025 «English» R25.0.171.0.0 `2a75996f…`; biblioteca `D:\Base_de_datos_AutoCAD_V.0.dwg` `b4ca2248…`.
- **Operador:** Claude (Coordinator de I-65) con Computer Use. El Owner no firmó nada en esta ventana; la aceptación es suya.
- **Propuesta del Coordinator: W-R1a = PASS.** Todos los oráculos de las partes A, B y D se cumplen. Hay desviaciones de operación (§6) que el Owner debe aceptar o rechazar.

## 1. Tramos

| Tramo | Carpeta | Hechos |
|---|---|---|
| 1 | `wr1-R20261029T1352Z` | BASELINE 13:52:45Z; AutoCAD pid 26020 (13:53:32Z); NETLOAD «Load Once»; REF-CANT-01 insertado 13:58:42Z; **STOP 14:01Z** (WR1-ST-02: Acrobat y WhatsApp del usuario, inactividad 0 s); 0 `RACKRAMPAQUETE`. El Owner cerró AutoCAD y ordenó «Cerré autocad, reaunda» (~16:26Z). Sin DWG guardado; POST 16:32:17Z solo con el historial del diálogo NETLOAD (INFORMAR). |
| 2 | `wr1-R20261029T1633Z` | Carpeta y BASELINE nuevas (16:33:56Z, igual al POST del tramo 1); identidades repetidas con fetch fresco (16:34:15Z, todas pasan); I-62 sin pantalla hasta 20:10Z. Parte A con AutoCAD pid 16344 (16:35:07Z-~17:16:40Z), con una **parada de 17:00Z a 17:04Z** (§6); parte B con AutoCAD pid 23584 (17:17:46Z-17:39:20Z); parte D 17:40-17:41Z; POST y K2 17:41-17:42Z; avisos de cierre 17:41Z. |

Tiempo de AutoCAD del tramo 2: ~64 min (presupuesto 2 h 15 min - 2 h 50 min; reserva 3 h 20 min).

## 2. Parte A (sesión de AutoCAD con NETLOAD)

| Paso | Resultado |
|---|---|
| WR1-03/04 | Una instancia; `Drawing1.dwg`; aviso estándar «Security - Unsigned Executable File» con la carpeta del Candidato → solo «Load Once»; un único `RackCad.Plugin.dll` `0f5a508e…`; antes del cierre Plugin/UI/Application/Domain = manifiesto del Candidato |
| WR1-05 | REF-CANT-01 (W1 §4.1): «2 estaciones · 1 intervalos · altura común 90.5 in · 1 paneles arriostrados · 2 separadores · 2 tensores · 4 componentes en el BOM»; bloque `RACKCAD_CANTILEVER_REF-CANT-01_-_frontal_FRONTAL` (310 contornos) |
| WR1-06/07 **OV-01** | `Valid`; **H0 = `73635936b33b3a7c151f865729de02e52540366074bdae021d4d4a35ba5d13ea`**; 3 archivos, sin diagnósticos; validador v2 **16/16**; carga útil **117/117**; huella consola = manifiesto = script |
| WR1-08 **OV-02** | `Valid`; huella `93849b42…7a72`; validador v2 con `expected-inputs-ov02.json` **16/16**; **6/6** cargas (Q2 = 99.99999999999997 %, representación) |
| WR1-09 **OV-04** | `Valid`; huella = H0; validador v2 **16/16** |
| WR1-10 **OV-03** | (a) `STATION-FACE-UNSUPPORTED (station-0)`; (b) `STATION-SIDE-UNSUPPORTED (station-0)`; (c) `ARM-SLOPE-UNSUPPORTED` ARM.1/2/3.PositiveY; (d) `ARM-ARRANGEMENT-UNSUPPORTED` ARM.1/2/3.PositiveY; (e) `MATERIAL-ROW-MISSING (AISC-HSS-RECT-HSS4X4X_313)`; en todos «Unsupported. El modelo estructural de la estacion no es valido; no se escribio nada.», ningún otro código, carpeta inexistente y 0 `*.tmp-*`; un único intento cada uno |
| WR1-11 | Cerrado sin guardar (`acad.exe` = 0); ningún DWG nuevo |

## 3. Parte B — OV-I65-08 (sesión de AutoCAD con NETLOAD)

| Paso | Resultado |
|---|---|
| O8-01 | NETLOAD «Load Once»; módulo único `0f5a508e…`; `RACKCAD` abre el menú con la biblioteca «disponible»; `RACKAYUDA` abre «comandos y atajos» **sin `RACKRAMPAQUETE`** (R-D8 = A) |
| O8-02 | REF-CANT-01 (W1 §4.1); vista frontal insertada con el ratón: bloque en capa 0, escala 1, rotación 0, posición = punto del clic, vertical |
| O8-03 | `RACKEDITAR` → separación 48 → 60 → «Actualizar»: redefinición en sitio («vistas redibujadas»), posición sin cambios; de vuelta a 48 → geometría original |
| O8-04 | «Insertar lateral» e «Insertar planta» (ID18 `COMPLETED (1/1)`); `RACKLISTA`: un rack «REF-CANT-01 · Cantilever · frontal, lateral, planta · 1 copia» |
| O8-05 | BOM del editor y `RACKBOMTOTAL`: 12 componentes, 44 piezas (columna con base ×2, brazo ×6, separador ×2, tensor ×2); sin duplicar por las tres vistas |
| O8-06 | Vista elegida: **frontal (vista principal)**. `Valid`, **H1 = `f69400f658bea7b74a59d5cd963e05a777641a821c19206706e8ec689b6d15eb`**; 16/16; 117/117; `rackId` `973e7734-7005-4c06-aa03-9f7b9ad6afc9`; huella de fuente `2fb3c99f…f872` |
| O8-07 | Guardado en `ov08` (como `Drawing1.dwg`, §6); cerrado; sin `.dwl`/`.dwl2`; `Copy-Item` → `ov08\hb-ref.dwg`; **SHA-256 de los dos `ac2c8014d05c5aaa6abcd384ddf7724834a41eaeb78fc4a140f98d92c3e948bb`**; reabierto con el aviso estándar «TrustedDWG» y sin avisos de RackCad |
| O8-08 | `RACKEDITAR`: Kind Cantilever, nombre, GUID y todos los valores editables iguales |
| O8-09 | Misma vista; `Valid`; **H2 = H1**; huella de fuente y `rackId` iguales en `pkg-A` y `pkg-B`; 16/16; scripts iguales salvo la carpeta |
| O8-10 | Separación 60 → «Actualizar» (tres vistas redibujadas) → guardar → cerrar → reabrir → `RACKEDITAR` muestra 60 con el mismo GUID → vuelta a 48 y «Actualizar» (sin guardar) |
| O8-11 | `RACKSELECTIVO` por omisión («Selectivo 1», 15 piezas); `RACKRAMPAQUETE` sobre él: «Unsupported. Solo se exporta un rack Cantilever; el elegido es de tipo selective.»; Esc en cada pregunta (rack, estación, cargas y carpeta tecleada) → `*Cancel*`; `pkg-cancel` no existe; 0 `*.tmp-*` |
| O8-12 | `RACKDUPLICAR` → «REF-CANT-01 - copia» con GUID nuevo (`a5226904-…`); `COPY` del frontal comparte definición e identidad (`RACKLISTA`: REF-CANT-01 con 2 copias) |
| O8-13 | Cerrado sin guardar los cambios posteriores; DWG y paquetes solo en `ov08`; POST en §5 |
| Legacy (WR1-13) | **NOT_EVALUATED**: el Owner no aportó un DWG Cantilever anterior representativo; no se declara validado |

Checklist del Owner para OV-08: creación y actualización del rack (O8-02/03), round-trip (O8-07/08), vistas y geometría (O8-02..04), BOM (O8-05), persistencia y recuperación (O8-10), identificadores y duplicación (O8-12), exportación antes y después de guardar (O8-06/09), H1 = H2 (O8-09), diagnósticos de error esperados (O8-11): **cumplido**.

## 4. Parte D (sin AutoCAD)

- **WR1-14:** `RamTool.exe inject` desde `pkg-OV01`, sin `--embed-folder`: `inj-OV05` (unknown-section), `inj-OV06` (inverted-axes), `inj-OV07a` (wrong-version), `inj-OV07b` (missing-core-function); cada manifiesto con `rackcadFaultInjection` y `sourcePackageSha256` = `96925f91…` (manifiesto de `pkg-OV01`); `pkg-OV01` sin cambios.
- **WR1-15:** `dump-OV01-gen` y `dump-OV02-gen` iguales a la plantilla de P2 (`af262489…`) salvo `dumpFolder`.
- Sin `ram-readback.json` ni `ram-dump.json`; RAM nunca iniciado.

## 5. POST y K2 (WR1-16)

- `wr_hostsnap.ps1 -Phase post`: igual a la BASELINE salvo `hkcu-profiles` y la inactividad. `smoke2_regdiff.ps1`: solo `Dialogs` (historiales e `InitialDirectory` de «Save Drawing As», «Select File» y `OpenSaveAnavDialogs`) y `VIEWBACKSTATUS` borrado = **INFORMAR**; `FixedProfile`, `Applications`, `InstalledApplications`, `Loaded` (HKCU) y `Applications` (HKLM) **IGUAL**; `ApplicationPlugins` sin RackCad; ACL y `TRUSTEDPATHS` idénticos. **Sin PROHIBIDO.**
- K2 frente a K1 del tramo 2: `stop=false`; solo avances limpios de worktrees de otras sesiones; checkout principal sin cambios.

## 6. Paradas, incidencias y desviaciones

1. **STOP 1 (14:01Z, tramo 1):** el usuario usaba el equipo. Desviación del operador en ese tramo: `keybd_event` (Alt) + `SetForegroundWindow` para el foco; no se repitió.
2. **STOP 2 (17:00Z, tramo 2):** inactividad 0 s en tres medidas con Outlook en primer plano y sin entradas del operador. El Owner declaró «No estaba usando el equipo, sigue». Causa no determinada. Seis teclas «W12X26» no llegaron al buscador del diálogo; la línea de comandos de AutoCAD no las tenía; destino no determinado (Computer Use confirmó AutoCAD en primer plano antes de enviarlas).
3. **Segunda `acad.exe` (17:05:06Z, pid 5084):** la lanzó `open_application` al intentar traer al frente AutoCAD bloqueado por el diálogo modal (fallo ya conocido de W3, repetido por el operador). Cerrada de inmediato sin módulos RackCad; claves protegidas idénticas a la BASELINE; `ApplicationPlugins` vacía; el pid 16344 seguía con un único módulo.
4. **Foco:** la ventana de la app de Claude quedaba por encima del editor WPF y se comía los clics simples; se usaron doble clic, triple clic, arrastre y teclado (todo con Computer Use). Para devolver el foco a AutoCAD o a los diálogos de RackCad se usó `SetForegroundWindow` (solo estado de ventana, sin teclas ni clics sintéticos). La petición de acceso a «File Explorer» fue denegada y no se repitió.
5. **Portapapeles:** al teclear el centro de un `ZOOM` de 20 caracteres, Computer Use lo envió por el portapapeles; el portapapeles del usuario pudo quedar sobrescrito. No era una ruta. Desde entonces, tecleo de 15 caracteres como máximo.
6. **Nombre del DWG:** Computer Use bloquea teclear en el cuadro «File name» de los diálogos de AutoCAD; el DWG se guardó con el nombre por omisión `ov08\Drawing1.dwg` en vez de `ov08\ov08-ref.dwg`. La carpeta, la copia `hb-ref.dwg` y sus SHA-256 son los previstos.
7. **Correcciones de datos antes de exportar (no son intentos):** longitud de base 480 → 48 (REF-CANT-01 del tramo 2); nombre de OV03-A-DOBLE (quedó «24» al no moverse el foco) corregido por teclado.
8. **Otros sin efecto:** un dibujo vacío `Drawing2.dwg` creado al teclear con la página de inicio activa (cerrado sin guardar); selecciones vacías («Nothing Selected») dentro de un mismo `RACKRAMPAQUETE` o `RACKEDITAR`; un clic bloqueado por Computer Use por referirse al otro monitor.
9. **Observaciones de producto (no fallos de oráculo):** tras «Actualizar», Propiedades muestra el nombre «REF-CANT-01 - frontal» en el bloque; la planta insertada con el punto tecleado `3967,1250` quedó lejos de las otras vistas.

## 7. Presupuesto y restricciones

- Ejecuciones de `RACKRAMPAQUETE`: 8 en la parte A y 7 en la parte B (O8-06, O8-09, O8-11 con el selectivo y las cuatro cancelaciones); todas previstas por el procedimiento y ninguna repetida.
- Sin cambios de código, Freeze, ADR-0050, tabla, tolerancias ni procedimientos; Codex 55/57 y `attempts` 7/11 sin cambios; RAM no iniciado; nada instalado en `ApplicationPlugins`.
- **Entrada para HB-04 (a) de W-R1b:** `D:\r65-smoke\4a03b65b\wr1-R20261029T1633Z\ov08\hb-ref.dwg`, `ac2c8014…48bb`, vista frontal, H2 = `f69400f6…15eb`.
- **Siguiente:** aceptación de W-R1a por el Owner y, si procede, autorización de W-R1b. El Coordinator se detiene aquí.

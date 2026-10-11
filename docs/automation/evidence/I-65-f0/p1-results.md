# I-65 — Resultados de la sonda P1 v3.2 (ejecución real)

## 1. Identidad de la corrida

| Hecho | Valor |
|---|---|
| Fecha | 2026-10-11 (UTC; 2026-10-10 20:28–20:35 hora local) |
| Producto | RAM Elements 2026, `26.00.01.015` (pantalla de inicio `26.0.1.15`; `RAMElements.exe` SHA-256 `7DCB4855…7970`) |
| Sonda aprobada | P1 v3.2 en `54f17ef6`, blob `2fafe9c9a99121ca82eb09180425463e1cb9fd59`, SHA-256 `96ae6ea92c5fba46df943317658cbcb2d0737ad527e39bb4a2b0cdae0723db80` |
| Copia ejecutada | [ram-probe-p1.executed.py](p1-run/ram-probe-p1.executed.py), SHA-256 `95378d877a37f297f7e1667b4e763ec891c5df3c07d33e833e34bb1009d85fe0`; única diferencia: la línea `OUT_DIR = r"D:\I65-P1\run-20261011T014503Z"` |
| Ejecutor | Coordinator con Computer Use (acceso concedido a «RAM Elements 2026», nivel completo), autorización del Owner registrada en `b0098f21` |
| Informe | [p1-report.json](p1-run/p1-report.json), SHA-256 `d34927716a07e46477f9c3717245582ca20d812994ce9eeccbae8dd07c8c11f3` (32 204 bytes); `write-test.txt` SHA-256 `2689367b…f1df` |
| Capturas | [p1-run/](p1-run/) 01 (modelo vacío al cargar), 02 (ruta en «Ejecutar archivo…»), 03 y 04 (diálogos de confirmación), 05 (salida «Script ejecutado con éxito»), 06 (aviso de guardar respondido «No»), 07 (error gRPC al cerrar) |

## 2. Secuencia observada

1. RAM Elements se abrió (PID 14412) y lanzó Bentley CONNECTION Client; quedó ~3.5 min en «Verificando licencia…» hasta que el Owner
   atendió la licencia fuera del alcance de Computer Use. Barra de estado: «Licencia: Desconocido»; registro de licencia:
   `GetLicenseType … CL_TYPE_UNKNOWN`. RAM funcionó con análisis incluido.
2. RAM arrancó con un modelo **sin título y vacío** («Elementos: 0», un caso por defecto `CM=Carga Muerta`, interfaz en español). No se
   abrió ningún archivo.
3. Python Console → «Ejecutar archivo…» → la copia ejecutable. La puerta pasó (0 nudos, 0 miembros, `OUT_DIR` válida y vacía).
4. Tras el análisis de N2, la escritura siguiente (N1) disparó el diálogo modal de RAM «Los resultados del análisis de la estructura se
   perderán. ¿Desea continuar?», que **bloqueó el script** hasta responder; se respondió «Sí» (solo descarta resultados del modelo
   desechable, ya leídos por N2). Igual tras el análisis de N1 al escribir U1. No hubo más diálogos.
5. Salida: «I-65 P1 report written to …», los 12 pasos `OK`, «--- Finalizado ---», estado «Script ejecutado con éxito».
6. Cierre de RAM: «El modelo ha sido modificado. ¿Desea salvarlo?» → **No**. Al cerrar, RAM mostró un error gRPC
   («StatusCode="Unavailable" … failed to connect to all addresses», también en `RE\ErrorLog.txt`) → «Aceptar». Proceso terminado.
7. Efectos fuera de `OUT_DIR`, todos propios de RAM: `C:\ProgramData\Bentley\Engineering\RAM Elements\26.0.1\Data\NoName.fem` (53 248
   bytes, **creado** por el solver a las 20:33:14; no existía antes), y archivos de configuración y registro del perfil del usuario de RAM
   (`RE\Config.ini`, `LastAnalysis.log`, `Recents.xml` vacío, etc.). **Ningún modelo `.retx` guardado.** `CurrentUnit=4` sin cambio.

## 3. Resultados por paso (resumen; los valores exactos están en el informe)

| Paso | Observación |
|---|---|
| Puerta | casos de File > New: `["CM"]`; título vacío; unidades iniciales de la API `kip-ft` (= unidades de la interfaz, `CurrentUnit=4`) |
| E1 | CPython `3.13.2`; `ramelements` `0.1.0`; **`__file__` no definido** (`NameError`) con «Ejecutar archivo…»; directorio actual = carpeta de instalación de RAM; 207 funciones en `_ramelements_core` |
| E2 | `json`, `math`, `hashlib` (SHA-256 correcto), `base64`, `binascii`, `datetime`, `traceback`: importan. **`threading` bloqueado** («blocked by RAM Elements security policy»); `os.makedirs` y `os.replace` = `None`; `open(…, "x")` en `OUT_DIR` funciona; literal JSON ASCII con `\u00f1` se decodifica |
| C5 | categorías como texto (`"OTHER"`) funcionan. **Con el miembro del enum, el caso se crea pero guarda la categoría `"LoadCategory.DEAD"`** (sin error). Peso propio: por defecto `(0, 0, 0)`; activado `(0, −1, 0)`; desactivado `(0, 0, 0)` |
| N2 | voladizo W 10X49 a lo largo de +X, `LB_IN`, 1000 lbf en −Y: flecha 0.044817 in; Timoshenko con la base de RAM (I33 272 in⁴, Aw2 3.4 in², E 29 000 ksi, ν 0.26) = 0.044814 in (Δ 7·10⁻⁵); reacción `fy = 1000`, `mz = 100 000` lb·in; `M33` de extremo = 100 000 |
| N1 | columna a lo largo de +Y, 500 lbf en +X: reacción `fx = −500`, `mz = 60 000` lb·in; flecha 0.038046 in frente a 0.038045 (Δ 4·10⁻⁵); `LastAnalysis.log`: «Grados de libertad activos 3 (Tx Ty Rz)» |
| U1 | 2000 in → `N_MM` 50 799.99837 (factor 25.3999991872, error relativo −3.2·10⁻⁸) → `KIP_FT` 166.6667 → `LB_IN` 2000.0 exacto. Carga −1000 lbf → `N_MM` −4448.198375 N (error relativo −5.22·10⁻⁶; g implícita 9.8066) → `KIP_FT` −1.0 |
| A1 | 12 direcciones × giros 0°, 90°, 12.3456°: todas cumplen la regla del §4 con error máximo 3.3·10⁻¹⁵; `rotation` se lee igual |
| S1 | `W 12X26`/`A992 Gr50`, `HSS_SQR 4X4X1_4`/`A500 GrC rectangular`, `S 10X35`/`A36` aceptados y leídos igual; **`IPE 200` (base europea) también aceptado**; sección inexistente → `ValueError: Section not found … must include a space and match the database exactly` **y queda un miembro huérfano**; material inexistente → `ValueError: Material not found` **y miembro huérfano** |
| R1 | valores por defecto: punto cardinal 0 (CENTROID), offsets 0, factores de rigidez 1, axial `NORMAL`, resortes 0, liberaciones todas falsas; apoyo `PINNED` leído `dxdydz`; los 11 setters funcionan y se leen; los reinicios devuelven los valores por defecto |
| L1 | dos cargas puntuales (`GLOBAL_Y` −1234.5 @ 37.5 %, `GLOBAL_Z` −987.25 @ 62.5 %) leídas igual |
| P2 | en `LB_IN`, coordenadas, giro, magnitud y posición no redondos **se leen idénticos**; en `N_MM` con los errores relativos de U1; vuelta a `LB_IN` idéntica |

## 4. Regla de ejes locales medida (RAM Elements 26.00.01.015)

Sea `l1` el eje 1 (de I a J) e `Ŷ = (0, 1, 0)`:

- miembro **no vertical**: `l3 = unit(l1 × Ŷ)`, `l2 = l3 × l1` (`l2` apunta «hacia arriba»; `l3` horizontal);
- miembro **vertical** (`l1 = ±Ŷ`): `l3 = +Ẑ`, `l2 = l3 × l1`;
- `rotation = θ` (grados) gira `l2` y `l3` alrededor de `l1` en sentido dextrógiro;
- el peralte de la sección va a lo largo de `l2` y la flexión fuerte es `M33` (N2, N1).

## 5. Matriz de capacidades

`VERIFIED IN INSTALLED RAM` = observado en esta corrida · `DOCUMENTED ONLY` = solo en documentación · `FAILED` = probado y no funciona como
se documenta · `UNKNOWN` = ni probado ni documentado con suficiencia.

| Capacidad | Clase | Evidencia |
|---|---|---|
| Ejecutar un script con «Ejecutar archivo…» en la Python Console | VERIFIED | §2 |
| Ejecución sin interfaz, por línea de comandos o desde otro proceso | DOCUMENTED ONLY (no existe) | Discovery §3 |
| Eje vertical **Y** (gravedad −Y) | VERIFIED | C5 peso propio; N1/N2 |
| Ejes locales y giro | VERIFIED | §4 |
| Longitud y fuerza en `LB_IN`, escala correcta | VERIFIED | N2/N1 (Timoshenko), P2 |
| Momento (lb·in) | VERIFIED | reacciones N2/N1 |
| Carga distribuida (unidades) | UNKNOWN (no medida; fuera de V1) | — |
| Cambio de par de unidades por la API conserva los valores físicos | VERIFIED | U1, P2 |
| Precisión de conversión `LB_IN` ↔ `N_MM` | VERIFIED: ida y vuelta en `LB_IN` exacta; lectura métrica con error −3.2·10⁻⁸ (longitud) y −5.22·10⁻⁶ (fuerza) | U1, P2 |
| `units.set` de la API cambia las unidades de la interfaz | no: la tabla de la interfaz siguió en pies y `CurrentUnit` no cambió (INFERENCE de captura + configuración) | captura 05, `Config.ini` |
| Crear y leer nudos, apoyos | VERIFIED | todos los pasos, R1 |
| Crear y leer miembros, sección y material por nombre | VERIFIED | S1, R1 |
| Nombre inexistente → error | VERIFIED (`ValueError`), **con miembro huérfano** | S1 |
| Secciones de otra base de país sin configurarla | VERIFIED (`IPE 200`) | S1 |
| Crear o leer propiedades de secciones o materiales | DOCUMENTED ONLY (no expuesto) | Discovery §3.1 |
| Liberaciones, offsets, factores, axial, resortes, punto cardinal (set/get) | VERIFIED | R1 |
| Cargas puntuales en miembro (global, %) | VERIFIED | L1, P2 |
| Categoría de caso como texto | VERIFIED; como enum → **FAILED** (guarda `"LoadCategory.DEAD"`) | C5 |
| Peso propio | VERIFIED (vector −Y) | C5 |
| Política de seguridad del intérprete | VERIFIED (`threading` bloqueado; `os.makedirs`/`os.replace` anulados; `open()` permitido) | E2 |
| `__file__` del script | VERIFIED: no definido | E1 |
| Análisis desde Python (síncrono) | VERIFIED | N2, N1 |
| Lectura de desplazamientos, reacciones y fuerzas de extremo | VERIFIED (solo caracterización) | N2, N1 |
| Textos de avisos y errores del solver | UNKNOWN (solo conteos 0/0) | N2, N1 |
| Modificar el modelo después de un análisis | **bloquea el script con un diálogo modal** hasta respuesta humana | §2 paso 4 |
| Guardar (`save_as`, extensión) | UNKNOWN (S9 retirado por condición del Owner) | — |
| Versión del producto por la API | DOCUMENTED ONLY (`Model.version` no expuesto) | Discovery |
| Licencia | ejecución con estado `CL_TYPE_UNKNOWN`; validación lenta que requirió al Owner | §2 paso 1 |

## 6. Problemas detectados

1. **Diálogo modal tras un análisis**: cualquier escritura posterior a `analyze()` bloquea el script hasta que una persona responde. No
   afecta a V1 (el script del producto no analiza y exige un modelo vacío); condiciona cualquier automatización futura (F6).
2. **Categoría por enum corrompe el dato en silencio** (C5): confirma la regla 5 de la plantilla.
3. **Miembro huérfano** ante un nombre rechazado (S1): confirma la regla `FAILED — DISCARD MODEL`.
4. **Conversiones métricas internas inexactas** (U1, P2): el adaptador y el volcado deben trabajar en `LB_IN`.
5. **Espacio de nombres de secciones global**: RAM acepta nombres de bases de cualquier país sin configurarlas; si un nombre existiera en
   dos bases, cuál gana es UNKNOWN.
6. **Licencia**: validación lenta y estado «Desconocido»; RAM funcionó. Riesgo operativo para cada corrida.
7. **Error gRPC al cerrar** RAM: benigno para la sonda, sin pérdida de datos observada.
8. **Archivo de trabajo del solver** `NoName.fem` en `ProgramData` (efecto propio de `analyze()`, no del script).
9. **Unidades por defecto del usuario** `kip-ft`: la API arranca en las unidades de la interfaz.
10. La base de secciones de usuario «Montilla» sigue sin localizarse (UNKNOWN).

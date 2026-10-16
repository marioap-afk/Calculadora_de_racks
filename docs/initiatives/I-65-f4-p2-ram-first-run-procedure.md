# I-65 — F4: P2, primera corrida en RAM Elements (preparación de solo lectura)

Documento de la unidad (Coordinator, 2026-10-16), por la disposición del Owner «Second Smoke Acceptance / RAM First Run Readiness»
([texto literal](../automation/decisions/I-65-owner-disposition-second-smoke-acceptance-ram-readiness.original.txt); decisiones
[§71](../automation/decisions/I-65.md)). Reutiliza el [paquete de validación de ingeniería](I-65-f4-engineering-validation-package.md)
§2-§3 (pasos D-F), el [procedimiento manual de `REF-CANT-01`](I-65-f4-ref-cant-01-manual-procedure.md), la
[resolución CMB-3](I-65-f4-od-f4-14-combinations.md) y la experiencia de [M-05c](../automation/evidence/I-65-f4/m05c/m05c-report.md).

> **Estado: PREPARADO. RAM EXECUTION = NOT YET AUTHORIZED. AUTOCAD = NO.** Nada de este documento se ha ejecutado en RAM. La preparación
> solo leyó archivos del paquete, de la instalación de RAM, del Config.ini de usuario y de los servicios del sistema, y escribió una
> comparación en `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\p2-prep\` (fuera del repositorio).

**Supuesto registrado (lección I-55):** en documentos anteriores, «P2» era la sonda previa opcional para M-01..M-03 (OD-F4-07 en
[F4-preparation](I-65-f4-preparation.md) §8). En esta disposición el Owner usa «P2» para la **primera corrida real**: construcción y
lectura de vuelta del modelo, seguidas de la comparación con la referencia manual (OD-F4-07 aceptada así). M-01..M-03 (giro,
descripción, banderas) quedan medidos dentro de la lectura de vuelta y del volcado de esta corrida. Si el Owner quería decir otra cosa,
este documento se reajusta antes de cualquier ejecución.

## 1. Identidades (leídas el 2026-10-16, ~21:10-21:20Z)

| Elemento | Valor |
|---|---|
| Paquete seleccionado | `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\pkg-01`: `manifest.json` `19fba962297badc95dd008bcf8d1477993c55a7ff8b5347d68e311e246f53a78`, `rackcad-ram-build.py` `d9b41aab69643c01971e756fb9323a8f35dad6b33008598655c753fe2f92d719`, `structural-loads.json` `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05` (iguales a la evidencia del segundo smoke); `ram-readback.json` **inexistente** |
| Huella de entrada | `505941e2ca42a02dd85b72629579bcd27e4900f80c69ebd41bb8ea73c0a5ba61` (manifiesto = script = consola) |
| Contenido de la carga útil | 8 nudos, 7 miembros (`W 12X26` / `A992 Gr50` con giro −90; `HSS_SQR 4X4X1_4` / `A500 GrC rectangular` con giro 0), 3 cargas puntuales `GLOBAL_Y` en el caso `REF1` (`OTHER`), unidades `LB_IN`, tolerancias de la lectura de vuelta 1e-9 |
| Igualdad con la especificación manual de `REF-CANT-01` | **IGUAL, 117/117 comprobaciones**: nudos, conectividad, sección, material, giro, longitudes, ejes locales, cargas, apoyos, caso y unidades. Leídas mecánicamente de las tablas §2.2-§2.5 del paquete de ingeniería ([script](../automation/evidence/I-65-f4/p2-prep/p2-payload-vs-manual.py.txt), [resultado](../automation/evidence/I-65-f4/p2-prep/payload-vs-manual.json)). Queda resuelto el riesgo R2-04 del segundo smoke: el DWG dibujado reproduce `REF-CANT-01` |
| `RAMElements.exe` | `C:\Program Files\Bentley\Engineering\RAM Elements\RAMElements.exe`, versión 26.00.01.15, SHA-256 `7dcb48555741fa368f5d5f6a14e760fe0662dd5c0b3cc53f6e88f1c0231e7970` (= P1 y M-05c) |
| API `ramelements` | `C:\Program Files\Bentley\Engineering\RAM Elements\python\ramelements`, `__version__ = "0.1.0"` (= la que exige la puerta de capacidad del script), `__init__.py` `fc5946d52d845392660a05ed1798210198be54fc8df3c8dc810a2f32d746aace`; 30 archivos, manifiesto del árbol `f71a017f49573030219a4213465c9ec454d3b6282f3f60b648a4300bfeba0a60` (primera vez que se registra; referencia para corridas posteriores). El script comprueba además, en tiempo de ejecución, las funciones del núcleo que necesita |
| Herramienta de F4 (volcado y comparador) | `tools\RackCad.StructuralAnalysis.RamTool\bin\Debug\net8.0` del worktree, `1.0.0+ad595b42…`: `RackCad.StructuralAnalysis.RamTool.dll` `5eff996df56abb612c2f63c705f8e185d13f379765b3c58cd565735394ae7d4e`, `.exe` `01869e1bdb9ae0aa23b64e79aef1c61e5f2ba9f9ef727543b6374ebe56c1cb15`, `RackCad.Application.dll` `3dcdb1f05f8ae47bb35547f93ea819cace400b4bd82c60edcb1a4fadb5c2abb4`. `git diff 93606ea0 51fc82f1 -- src tools` y `git diff 51fc82f1 HEAD -- src tools assets` vacíos: la plantilla de volcado y el comparador son los de M-05c y los del paquete |
| Validez de CMB-3 (CD-I65-F4-71/72) | **vigente**: mismo `RAMElements.exe`, `ramelements` 0.1.0 y script de volcado del adaptador 1.0.0 desde la plantilla verificada |
| Config.ini de usuario de RAM | `%APPDATA%\Bentley\Engineering\RAM Elements\26.0.1\RE\Config.ini`, SHA-256 `2505ca6e50eb722d418085f4b4382693d8ac1f894fb1fdfac51272f1e3a6282e`, sin cambios desde M-05c (2026-10-14T02:03:19Z); `frameSeccionesDataBaseName=Montilla` (no es autoridad, CD-I65-F4-74); última carpeta de scripts = la de M-05c |
| Licencia (solo lectura) | servicios y procesos `Bentley.Licensing.Service` y `Bentley.Connect.Client` en ejecución. En M-05c la barra de estado mostró «Licencia: Practicante». La disponibilidad efectiva solo se confirma al abrir RAM (paso P2-05) |

## 2. Integridad del paquete original (respuesta a la disposición §2)

**El paquete puede ejecutarse desde su carpeta sin reubicarlo ni cambiar la ruta embebida, y los tres archivos originales no se
alteran.** Por qué (lectura del script y del código del producto):

- El script solo abre **un** archivo para escritura: `data["packageFolder"] + "\\" + "ram-readback.json"`, en modo exclusivo `"x"`
  (línea 295). No abre `manifest.json` ni `structural-loads.json` (las cargas van dentro de `PAYLOAD`) y no se reescribe a sí mismo.
- `ram-readback.json` es parte del diseño del paquete: el manifiesto lo declara (`readback.name`) y el comparador de F4 lo lee **de la
  carpeta del paquete** junto con `manifest.json`.
- La «Ejecutar archivo…» de la consola Python de RAM no deja archivos al lado del script: las carpetas de M-05c contienen solo el script
  y su volcado (sin `__pycache__`).
- El script no guarda ni analiza el modelo (`model.save` no se llama; `model.close` y `model.open` ni existen en el puente).

**Consecuencias que el Owner debe conocer:**

1. Después de P2, la carpeta tendrá **cuatro** archivos. Con ese cuarto archivo, el validador del segundo smoke (V-01, «exactamente tres
   archivos») ya no es aplicable. La integridad posterior se comprueba con el SHA-256 de los tres archivos originales, más el del
   `ram-readback.json` nuevo.
2. **Una sola ejecución por carpeta:** el modo `"x"` falla si `ram-readback.json` ya existe. Un segundo intento con el mismo paquete no
   escribiría el modelo (la excepción salta antes de `write_model`) ni sobrescribiría la lectura de vuelta. Un nuevo intento exigiría
   reubicar o regenerar el paquete. En ese caso **STOP y propuesta específica** al Owner, como ordena la disposición.

## 3. Ruta exacta de la lectura de vuelta

`D:\r65-smoke\51fc82f1\run-R20261016T1918Z\pkg-01\ram-readback.json`. Es la ruta embebida en `PAYLOAD.packageFolder` más `\`, y la
validó V-11 v2. El archivo no existe hoy. Contiene el esquema `rackcad-ram-readback/v1`, `status` (`READBACK OK` o
`FAILED — DISCARD MODEL`), `adapterVersion`, la huella de entrada, `differences` y `values` (nudos, miembros con sección, material, giro,
punto cardinal, offsets, factores, comportamiento axial, resortes, liberaciones, ejes y descripción; cargas; caso con su peso propio).

## 4. Condiciones de seguridad (todas antes de abrir RAM)

| # | Condición |
|---|---|
| PS-01 | Autorización específica del Owner (§10), con alcance y presupuesto |
| PS-02 | `RAMElements.exe` = 0 y SHA-256 `7dcb4855…7970`; `ramelements` 0.1.0 con el mismo manifiesto del árbol (`f71a017f…`) |
| PS-03 | Los tres archivos del paquete con los SHA-256 de §1 y `ram-readback.json` inexistente |
| PS-04 | Copia de seguridad de `Config.ini` en la carpeta de la corrida (no se restaura después sin orden: CD-I65-F4-73) |
| PS-05 | Herramienta de F4 congelada (copia de `bin\Debug\net8.0` con los SHA-256 de §1) y dos carpetas **nuevas**, sin reutilizar, para los volcados del generado y del manual, con sus scripts `rackcad-ram-dump.py` generados por `dump-script --out <carpeta> --adapter-version 1.0.0` |
| PS-06 | Avisos previos a I-52 e I-64 (I-64 los pide: «un aviso breve antes de abrir RAM y otro al cerrarlo»); ninguna BASELINE-POST de I-64 ni operación de host de I-52 en curso; I-64 pide que la corrida no coincida con su Smoke-1 (mide tiempos) |
| PS-07 | **Nadie usando el equipo:** Computer Use toma la pantalla. El 2026-10-16 había una `acad.exe` ajena a I-65 (20:47Z-≤21:18Z). La corrida no se hace mientras el usuario trabaja, salvo su conformidad |
| PS-08 | Acceso de Computer Use concedido a «RAM Elements 2026». Si no se concede, el Owner o el ingeniero opera a mano con este procedimiento (OD-F4-08); no se intenta esquivar ninguna restricción |
| PS-09 | Instantánea K1/K2 de los checkouts |

## 5. Procedimiento de ejecución y cierre

**P2-A — modelo generado (construcción y lectura de vuelta):**

| # | Paso |
|---|---|
| P2-01 | Carpeta de corrida `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\p2-<RunId>\` (`host-before\`, `captures\`, `dump-gen\`, `dump-man\`, `host-after\`); PS-02..PS-05 y K1 |
| P2-02 | Aviso previo a I-52 e I-64 |
| P2-03 | Abrir RAM Elements (una sola instancia). Comprobar: licencia en la barra de estado; modelo **nuevo, sin título y vacío** («Elementos: 0», un solo caso `CM` sin cargas); ningún otro modelo abierto. Si RAM abre un modelo existente → File > New sin guardar nada |
| P2-04 | Python Console → «Ejecutar archivo…» → `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\pkg-01\rackcad-ram-build.py`: navegar con doble clic hasta la carpeta (Computer Use solo admite teclear en el campo «File name») y comprobar en la captura la carpeta y el nombre antes de abrir |
| P2-05 | Captura de la consola. Esperado: `READBACK OK`, sin líneas `- …`, y `AnalysisInputFingerprint 505941e2…ba61`; `ram-readback.json` creado con `status` `READBACK OK` y la misma huella |
| P2-06 | Inspección visual (solo mirar): 8 nudos, 7 miembros, columna vertical, brazos hacia −Z de RAM, cargas hacia abajo, eje 2 de las columnas hacia −Z. Propiedades de un miembro de cada tipo: la sección y el material se ven en el grupo **United States** (DIF-03; si aparece «Montilla» con esos nombres → STOP). Ningún editor de secciones ni de materiales, sin cambiar nada |
| P2-07 | **Mismo modelo y misma sesión** (A65-F4R-02): «Ejecutar archivo…» → `dump-gen\rackcad-ram-dump.py` → `DUMP OK`, `ram-dump.json` |
| P2-08 | Descartar el modelo **sin guardar**: File > New → «¿Desea salvarlo?» → **No** |

**P2-B — modelo manual de `REF-CANT-01`** (§6): construir en el modelo nuevo de P2-08 según el
[procedimiento manual](I-65-f4-ref-cant-01-manual-procedure.md) §2-§11, sin analizar. Después, «Ejecutar archivo…» →
`dump-man\rackcad-ram-dump.py` → `DUMP OK`. Medición M-05: cómo lee la API lo creado a mano. Descartar sin guardar, salvo que el
ingeniero decida conservarlo (D-11).

**Cierre:** cerrar RAM → «¿Desea salvarlo?» → **No**; `RAMElements.exe` = 0. SHA-256 de los tres archivos originales (deben ser iguales
a §1) y de `ram-readback.json` y los dos volcados; diferencias de `Config.ini` (solo claves y nombres); K2; aviso de cierre a I-52 e
I-64.

**P2-C — comparación (fuera de RAM):** `RackCad.StructuralAnalysis.RamTool.exe compare --package <pkg-01> --generated
dump-gen\ram-dump.json --manual dump-man\ram-dump.json --report <archivo nuevo>`, **sin** `--*-combinations-unread` (CMB-3, que
sustituye a §9 del procedimiento manual). Códigos de salida: 0 PASS, 1 FAIL, 2 REJECTED/STALE, 5 INCOMPLETE.

**Custodia:** capturas; `ram-readback.json`, los dos volcados y el informe del comparador versionados; los tres archivos del paquete por
SHA-256; diferencias de `Config.ini`; horas.

## 6. Preparación de `REF-CANT-01` manual

- La especificación y el procedimiento ya existen y coinciden con el paquete en 117/117 valores. El modelo manual es la referencia de
  **equivalencia de transferencia** (D-11): compara el camino API con el camino de la interfaz. **No** es validación estructural (Q-O3).
- Ajustes vigentes sobre el texto del procedimiento manual:
  - la categoría del caso `REF1` es `DL`, porque la interfaz no ofrece «Other» (M-05c), y para el comparador es informativa;
  - rige CMB-3: sin opciones `--*-unread`, y 0 combinaciones en ambos volcados son necesarias para PASS;
  - el operador es el de OD-F4-08.
- **Operador (OD-F4-08 = Claude con Computer Use):**
  - Viable, pero es la parte de más riesgo de la corrida: unos 8 nudos, 7 miembros con sección, material y giro, 8 juegos de apoyos y
    3 cargas con un % de 3 decimales, todo tecleado en rejillas.
  - Gotchas de M-05c y del smoke: los diálogos de archivo solo admiten teclear en «File name»; las listas desplegables pueden no verse
    en la captura; el teclado no mueve algunas listas (hay que arrastrar la barra); RAM se abre en el monitor «BenQ GW2780».
  - Un error de tecleo lo detecta el comparador como FAIL de una fila. CMB-3 solo prevé corregir el manual y repetir su volcado en una
    carpeta nueva para el caso de las combinaciones. Extenderlo a otros errores del manual es una propuesta de este documento: exige
    decisión y presupuesto del Owner (§10, punto 6).
  - Alternativa: que el ingeniero construya el modelo a mano y la sesión solo genere los volcados y compare.
- **Independencia:** si Claude teclea el modelo manual a partir de la especificación, la referencia comparte el origen `[CÓDIGO]` de los
  valores (golden de F1/F3). Lo que se comprueba es la **transferencia** (API frente a interfaz), no la derivación de la geometría. Esa
  derivación pertenece a la validación de ingeniería (§7, nivel 4).

## 7. Secuencia de validación de equivalencia (cuatro niveles separados)

| Nivel | Qué demuestra | Evidencia y evaluador | Qué NO demuestra |
|---|---|---|---|
| 1. Construcción válida en RAM | El script escribió el modelo con la API sin excepción y su lectura inmediata coincide con lo escrito (tolerancia 1e-9) | consola `READBACK OK`; `ram-readback.json` con `status` `READBACK OK`, sin `differences`; inspección P2-06 | que otro lector vea lo mismo; que el modelo sea el de referencia; nada estructural |
| 2. Lectura de vuelta completa | Que la lectura cubre todos los atributos del contrato y que un lector independiente (el volcado) ve el mismo modelo | `ram-readback.json` (valores de 8 nudos, 7 miembros y 3 cargas) y `dump-gen\ram-dump.json` `DUMP OK`; el comparador valida la huella (`STALE` si no casa) | la equivalencia con la referencia manual |
| 3. Comparación con `REF-CANT-01` manual | Equivalencia de transferencia, según las filas de la matriz §4 evaluadas por el comparador (coordenadas 0.02 in, conectividad, ejes 0.1°, sección y material por nombre, punto cardinal, offsets, factores, axial, resortes, liberaciones, apoyos, cargas 0.1 % / 0.02 in, combinaciones 0 en ambos, CMB-3) | informe del comparador (PASS / FAIL / INCOMPLETE); M-05 registrada | propiedades de sección y material (la API no las expone: van con la hoja firmada); validación estructural |
| 4. Validación de ingeniería del modelo | Que las idealizaciones, los apoyos, las cargas y las propiedades son adecuados para su uso | hoja firmada (D-09) para las propiedades y revisión del ingeniero de supuestos y exclusiones | **no se hace en P2**; no hay análisis, resultados ni diseño. La tabla `verified` no es validación estructural |

Se mantienen la exclusión del peso propio (`selfWeight` 0, que comprueba la lectura de vuelta), DIF-01 como diferencia informativa y
CMB-3. Ningún nivel se deduce del anterior. OV-I65-01 queda PASS solo con el nivel 3 PASS y la hoja firmada.

## 8. Errores, modelos parciales y paradas

| # | Situación | Tratamiento |
|---|---|---|
| ST-P2-01 | Licencia no disponible, o RAM abre con otro modelo con cambios | STOP (sin guardar nada) |
| ST-P2-02 | Modelo no vacío al ejecutar | el script responde `gate:` **sin escribir** el modelo; registrar; STOP |
| ST-P2-03 | Fallo de la puerta de capacidad (versión de `ramelements` o función del núcleo ausente) | `gate:` sin escribir; STOP (cambió la identidad ligada a M-05c) |
| ST-P2-04 | `FAILED — DISCARD MODEL` con detalles distintos de `gate:` (excepción a mitad de la escritura, p. ej. una sección inexistente; diferencias en la lectura) | el modelo puede quedar **parcial**: no se analiza ni se guarda; se captura; File > New → **No**; STOP. Un nuevo intento exige otro paquete (§2): propuesta al Owner |
| ST-P2-05 | El script no puede crear `ram-readback.json` (ya existe o no hay permiso) | excepción antes de escribir el modelo; no se crea nada; STOP |
| ST-P2-06 | La sección o el material aparecen en «Montilla» o en otro grupo distinto de United States | STOP (DIF-03) |
| ST-P2-07 | Cualquier diálogo de «los resultados se perderán», de análisis, de guardado inesperado o de configuración | responder **No** o Cancelar; STOP |
| ST-P2-08 | `DUMP` distinto de `DUMP OK` | volcado rechazado; STOP |
| ST-P2-09 | Comparador FAIL / REJECTED / STALE / INCOMPLETE | se registra literal; FAIL del manual → corrección y nuevo volcado en carpeta nueva **solo** si el presupuesto lo permite; FAIL del generado → defecto del producto, STOP |
| ST-P2-10 | Cambio de un SHA-256 de los tres archivos del paquete | incidente; STOP |
| ST-P2-11 | Necesidad de analizar, guardar, tocar AutoCAD o cambiar la configuración | STOP |

**Descarte sin guardar:** RAM no tiene «cerrar modelo» en la API. Se descarta con File > New → «¿Desea salvarlo?» → **No**, o cerrando
RAM → **No**, como en M-05c (capturas 06 y 16). Nunca «Sí» ni «Guardar como».

## 9. Cambios de configuración de usuario y del host

- **`Config.ini` de RAM:** RAM lo reescribe al cerrar con estado de interfaz. En M-05c cambiaron los contadores de uso
  (`accLoadConditionsManager`, `accMatDB`, `accPythonConsole`, `accQANew`, `accSeccDB`), `PythonScripts` (la última carpeta de scripts) y
  el último material visto. P2 provocará cambios de la misma clase. `PythonScripts` apuntará a `pkg-01` o a una carpeta de volcado.
  Se hace copia previa (PS-04) y se registra la diferencia por clave; no se restaura sin orden (CD-I65-F4-73).
- **Nada de AutoCAD:** perfil, `TRUSTEDPATHS`, `SECURELOAD`, registro y plugins no se tocan.
- **Portapapeles:** en M-05c, Computer Use tecleó rutas a través del portapapeles. En el segundo smoke, sin el permiso `clipboardWrite`,
  tecleó `\` directamente. En RAM se pide acceso sin portapapeles.

## 10. Autorización exacta necesaria para ejecutar P2

> **RAM EXECUTION = AUTHORIZED FOR P2 ONLY**, en una ventana libre de uso del equipo y sin solape con una BASELINE-POST de I-64, con:
> 1. RAM Elements 26.00.01.15 (`7dcb4855…7970`) y `ramelements` 0.1.0 (árbol `f71a017f…`); Computer Use con acceso a «RAM Elements 2026»
>    (sin portapapeles), u operador manual si no se concede.
> 2. **P2-A:** una ejecución de `rackcad-ram-build.py` del paquete seleccionado `pkg-01` (sin reubicar ni regenerar), escribiendo solo
>    `ram-readback.json`; inspección visual; volcado del generado en la misma sesión; descarte sin guardar.
> 3. **P2-B:** modelo manual de `REF-CANT-01` según el procedimiento manual, con **operador = [Claude con Computer Use | ingeniero a mano]**;
>    volcado; descarte sin guardar (salvo decisión del ingeniero de conservarlo).
> 4. Congelar una copia de la herramienta de F4 (`ad595b42`, §1) y generar con ella **dos** scripts de volcado en carpetas nuevas.
> 5. **P2-C:** comparador de F4 sin `--*-combinations-unread` (CMB-3).
> 6. Presupuesto: **1** ejecución del script de construcción (sin reintento; un `FAILED` → STOP y propuesta); hasta **[1]** corrección del
>    modelo manual con un volcado nuevo en carpeta nueva; Codex **0**; sin Workers.
> 7. Aceptar los cambios de estado de interfaz en `Config.ini` de RAM (copia previa, sin restaurar).
> 8. Sin análisis, diseño, resultados ni guardado de modelos; sin AutoCAD ni NETLOAD; sin cambios de código, de la tabla ni del paquete;
>    sin F5-F7; sin Candidato ni GATE PASS de F4.

Pendientes que el Owner puede decidir en la misma autorización o mantener abiertos:
- **CR-05/CR-06** del documento de preparación pedían un paquete generado desde un Candidato de hito declarado. El Owner seleccionó el
  paquete existente (`51fc82f1`, tabla `verified`) **sin** Candidato. Conviene que lo confirme expresamente para P2, o que mantenga el
  Candidato como requisito antes de OV-I65-01 «oficial».
- La hoja firmada (D-09) cubre la fila de propiedades del nivel 3. OD-F4-11 (repetir sobre el Candidato final) sigue abierta.

# I-65 — F4: procedimiento de W2 (RAM Elements) de la validación final sobre el Candidato de hito `5a5ca577`

Documento de la unidad (Coordinator, 2026-10-17), por la autorización del Owner «F4 Final Validation — W2 RAM Elements»
([texto literal](../automation/decisions/I-65-owner-authorization-w2-ram-final-validation.original.txt); decisiones
[§80](../automation/decisions/I-65.md)). Concreta la matriz de la validación final ([preparación](I-65-f4-final-candidate-preparation.md)
§6 y adenda §11; matriz del hito en la evidencia §81) para la ventana W2. Reutiliza los pasos de RAM de P2
([procedimiento](I-65-f4-p2-ram-first-run-procedure.md) §4-§5) y los paquetes de W1 ([informe](../automation/evidence/I-65-f4/w1-R20261017T0847Z/w1-report.md)).

> **Estado: PREPARADO.** W2 RAM EXECUTION = CONDITIONAL GO: entra en vigor solo con este procedimiento versionado, su CI satisfactoria y
> todas las condiciones de entrada de §3 cumplidas. AUTOCAD = NO. STRUCTURAL ANALYSIS = NO. PRODUCT CODE CHANGES = NO. F4 GATE PASS =
> NOT YET AUTHORIZED.

## 1. Identidad fija (cualquier diferencia es parada)

Carpeta de la corrida: `D:\r65-smoke\5a5ca577\ov-R20261017T0847Z\` (la de W1). SHA-256 de los bytes:

| Paquete / archivo | `manifest.json` | Script que se ejecuta | `structural-loads.json` |
|---|---|---|---|
| `pkg-OV01` | `6b013b62423e6e1590849a9bacdd5e4242d6930db6de040cc3f62507444a4edc` | `rackcad-ram-build.py` `d01551f7cfa78a80e787471b763e9af8b1a5944681f64190d918a14e5d3c2f3f` | `25f48e7f…ae05` |
| `pkg-OV04` | `8506fdcdd2f6fc45b2f9d22d53abb71bfe948e1091dd0f33743d2d414d1fa326` | `rackcad-ram-build.py` `623cbba5e2302e9495ff781b5341c50421a66025ccf0ca8fadc84ca22ea33cde` | `25f48e7f…ae05` |
| `inj-OV05` | `7335a76992c1d44c3d02062e70f0d5148f86d84259db84bfa5748a722d179179` | `rackcad-ram-build.py` `c995ccb006a113ac3889e2d3b88dc381a0cb32cdcf09cd482416641f17943e39` | `25f48e7f…ae05` |
| `inj-OV06` | `8c6e1c4d9cb34001cb424f4c39cee076d0ad2fc40784b8dbf0e2b915f73635cc` | `rackcad-ram-build.py` `1d36513d3822c4fd53bd1c1a6f3648445606765db05132ac3673c93683cc407c` | `25f48e7f…ae05` |
| `inj-OV07a` | `72a6e289ea5c7ad6d2a66696bc15e61d8092bf76b0221379865b792525976567` | `rackcad-ram-build.py` `b6016021012a6c523fa03940d0d674c152f703304a1601a7d9fd9bf7bc595486` | `25f48e7f…ae05` |
| `inj-OV07b` | `cb4972ab776304d8b0511f7acc4e5a92ccc291e818fb20b6b0519403e2494c6c` | `rackcad-ram-build.py` `6ad0d8916aac8287596197135fe0086727efcbf6a6cc2d916fae1b62990d734c` | `25f48e7f…ae05` |
| `dump-OV01-gen` | — | `rackcad-ram-dump.py` `56faa3550934a663889fb78b0b7787350839ed6b3f0747cf9064fc2a5ae1a2d5` (= plantilla de P2 salvo `dumpFolder`) | — |

| Elemento | Valor |
|---|---|
| Huella de entrada de `pkg-OV01`, `pkg-OV04` y las inyecciones | `f5ce900c14ee768bb85670e814099d58947857778077ced8f81d371a8858d663` |
| Volcado manual de referencia (estrategia A, solo F4) | `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\p2-R20261016T2147Z\dump-man\ram-dump.json`, `c4a98fb0ea798031de2b3052d81eae89f42b731a92f06dbfa97f148ade9ba23b` (= copia versionada de P2) |
| `RAMElements.exe` | 26.00.01.15, `7dcb48555741fa368f5d5f6a14e760fe0662dd5c0b3cc53f6e88f1c0231e7970` |
| `ramelements` | `__version__ = "0.1.0"`, árbol de 30 archivos `f71a017f49573030219a4213465c9ec454d3b6282f3f60b648a4300bfeba0a60` |
| Bases de RAM (solo lectura) | `Database\Sections\United States\` (59 archivos, árbol `ccdc2346…dc05`; `W.sec` `dffd9e00…b977` y `HSS_SQR.sec` `992ddac9…3df0`, iguales a la lectura de DIF-04 del 2026-10-11/14); `Database\Materials\United States\` (14 archivos, árbol `4cb742a5…a87e`; `Steel.mat` `2a8b4239…9ce1`, con `A992 Gr50` y `A500 GrC rectangular`) |
| Herramienta de F4 (comparador) | `D:\r65-smoke\5a5ca577\ramtool\`, `.exe` `0b56e198…`, `.dll` `d421c82b…` |
| `Config.ini` de usuario | `2c5939572c66032fc5ecf8204c5b4d9dc6f8d0efb7695eacfc2c0c1ea3489d4a` (el de después de P2); `PythonScripts` apunta a `dump-man` de P2 |
| Captura PRE/POST | `w2_hostsnap.ps1` ([copia](../automation/evidence/I-65-f4/w2-prep/w2-hostsnap.ps1.txt)): procesos, identidades, árboles de las bases, `Config.ini` (copia en PRE), archivos de cada carpeta, volcado manual e inactividad |

**Integridad de los paquetes.** El script de construcción solo abre para escritura `<packageFolder>\ram-readback.json`, en modo exclusivo
`"x"` (diseño del paquete; P2, §2). Los tres archivos de cada paquete no se modifican: se comprueban por SHA-256 antes y después. El
único archivo nuevo de cada carpeta es su `ram-readback.json`, y en `dump-OV01-gen` el `ram-dump.json`. Una sola ejecución por carpeta.

## 2. Comportamiento del script (lectura del código generado, para fijar los oráculos)

`main()`: (1) puerta de capacidades (`ramelements.__version__` = la del paquete y presencia de las funciones del núcleo); si falla, **no
llama a `rm.model()`**; (2) puerta del modelo vacío (0 nudos, 0 miembros, no más de un caso, el caso `REF1` inexistente); (3) puerta de la
carga útil; (4) crea `ram-readback.json`; (5) con alguna puerta fallida escribe `gate: …` y **no escribe el modelo**; si no, escribe el
modelo y lo lee de vuelta. Imprime `READBACK OK` o `FAILED — DISCARD MODEL`, una línea ` - …` por diferencia y
`AnalysisInputFingerprint <huella>`.

## 3. Condiciones de entrada (todas; si falta una, no se abre RAM)

| # | Condición (autorización §8) |
|---|---|
| W2-PR-01 | Este procedimiento versionado en `origin` y su CI 4/4 en `success` |
| W2-PR-02 | Nadie usa el equipo: inactividad ≥ 120 s justo antes de empezar y la app de Claude en primer plano |
| W2-PR-03 | `RAMElements.exe` = 0 y `acad.exe` = 0 |
| W2-PR-04 | `RAMElements.exe` `7dcb4855…`, `ramelements` 0.1.0 con árbol `f71a017f…` |
| W2-PR-05 | Bases de secciones y materiales con los SHA-256 de §1 |
| W2-PR-06 | Los paquetes, scripts y el volcado manual con los SHA-256 de §1 |
| W2-PR-07 | Destinos vacíos: ningún `ram-readback.json` en `pkg-OV01`, `pkg-OV04`, `inj-OV05`, `inj-OV06`, `inj-OV07a`, `inj-OV07b`; ningún `ram-dump.json` en `dump-OV01-gen` |
| W2-PR-08 | Copia de `Config.ini` en `w2-pre\` (no se restaura después sin orden: CD-I65-F4-73) |
| W2-PR-09 | Aviso a I-64 y confirmación de que no hay conflicto con sus capturas (BASELINE/POST); aviso de cortesía a I-52 |
| W2-PR-10 | Instantánea K1 de los checkouts |

## 4. Pasos generales

| # | Paso |
|---|---|
| W2-01 | `w2_hostsnap.ps1 -Phase pre`; K1 (`k1-w2.json`) |
| W2-02 | Aviso previo a I-64 e I-52 |
| W2-03 | Abrir RAM Elements 2026 (una sola instancia; Computer Use con el acceso normal a la aplicación). Comprobar la licencia en la barra de estado y un modelo **nuevo, sin título y vacío**: rejillas de nudos y miembros vacías, «Elementos: 0», un único caso `CM` sin cargas. Si RAM abre un modelo existente: File > New sin guardar |
| W2-04 | Casos en este orden: OV-01, OV-04, OV-05, OV-06, OV-07a, OV-07b (§5). **Un modelo desechable por escenario** |
| W2-05 | **Ejecutar un script:** Python Console → «Ejecutar archivo…»; navegar con doble clic sobre el **icono** de cada carpeta hasta la del paquete (solo se puede teclear en «File name»); comprobar en la captura carpeta y nombre; **SHA-256 del script justo antes de «Abrir»** igual a §1; «Abrir». Si la ruta o el hash no coinciden: no se ejecuta (parada) |
| W2-06 | **Descartar un modelo sin guardar:** File > New → «¿Desea salvarlo?» → **No**. Comprobar el modelo nuevo vacío (W2-03) antes del caso siguiente |
| W2-07 | Tras el último caso: cerrar RAM → «¿Desea salvarlo?» → **No**; `RAMElements.exe` = 0 |
| W2-08 | Fuera de RAM: comparador de OV-01 y control de OV-05 (§5) |
| W2-09 | `w2_hostsnap.ps1 -Phase post`; diferencias de `Config.ini` (solo nombres de clave); K2 |
| W2-10 | Aviso de cierre a I-64 e I-52 con `RAMElements.exe` = 0 |
| W2-11 | Custodia (§7) |

## 5. Casos

| OV | Paquete y script (§1) | Estado inicial exigido del modelo | Resultado esperado | Evidencia que decide PASS / FAIL | Paradas propias | Descarte |
|---|---|---|---|---|---|---|
| **OV-I65-01** | `pkg-OV01\rackcad-ram-build.py`; después, en la **misma sesión y el mismo modelo**, `dump-OV01-gen\rackcad-ram-dump.py` | nuevo, sin título, vacío (W2-03) | consola `READBACK OK`, ninguna línea ` - `, `AnalysisInputFingerprint f5ce900c…d663`; `ram-readback.json` con `status` `READBACK OK`, `differences` vacío y la misma huella; volcado `DUMP OK` y `ram-dump.json`; comparador `compare --package pkg-OV01 --generated dump-OV01-gen\ram-dump.json --manual <volcado manual de P2> --report w2-OV01-comparison.json` **sin** `--*-combinations-unread` (CMB-3) → **PASS, código 0**, con `inputs.manualDumpSha256` = `c4a98fb0…` | consola + lectura de vuelta (niveles 1-2); informe del comparador (nivel 3): PASS en coordenadas, conectividad, ejes locales, sección, material, valores por omisión de miembro, liberaciones y apoyos, cargas y **combinaciones = 0 en ambos** (CMB-3); propiedades `NOT_EVALUATED` (hoja D-09); DIF-01 y el peso propio informativos | cualquier `FAILED`, huella distinta, `DUMP` distinto de OK, comparador ≠ 0, `manualDumpSha256` distinto | W2-06 tras el volcado |
| **OV-I65-04** | `pkg-OV04\rackcad-ram-build.py` | nuevo y desechable (**no** el de OV-01) con **un nudo de prueba** tecleado a mano en la rejilla de nudos, en (0, 0, 0) y sin apoyos; captura de la rejilla (1 nudo, 0 miembros) | consola `FAILED — DISCARD MODEL` con ` - gate: the model has 1 nodes` y la huella; `ram-readback.json` con `status` FAILED, esa diferencia y `values` vacíos; modelo con **exactamente** su contenido inicial (1 nudo en (0, 0, 0), 0 miembros, sin caso `REF1`) | consola + lectura de vuelta + rejillas y barra de estado antes/después | `READBACK OK`, miembros o nudos nuevos, caso `REF1` creado | W2-06 |
| **OV-I65-05** | `inj-OV05\rackcad-ram-build.py` (sección `HSS_SQR INEXISTENTE-99` en el primer miembro) | nuevo, vacío | consola `FAILED — DISCARD MODEL` con ` - exception: …` que nombra el rechazo de la sección (como el `ValueError` de P1); `ram-readback.json` FAILED; ningún resultado válido. Si quedan elementos parciales: se documenta su número (rejillas y barra de estado) | consola + lectura de vuelta (A65-F4C-03); control adicional fuera de RAM: `compare --package inj-OV05 --generated dump-OV01-gen\ram-dump.json --manual <volcado manual de P2> --report w2-OV05-control.json` → **REJECTED (2)** por la marca de inyección | `READBACK OK` (hallazgo: RAM aceptaría una sección inexistente) | W2-06 |
| **OV-I65-06** | `inj-OV06\rackcad-ram-build.py` (eje local 2 esperado del primer miembro invertido) | nuevo, vacío | consola `FAILED — DISCARD MODEL` con al menos una diferencia de ejes locales del primer miembro; sin `READBACK OK`; `ram-readback.json` FAILED | consola + lectura de vuelta | `READBACK OK` | W2-06 |
| **OV-I65-07a** | `inj-OV07a\rackcad-ram-build.py` (versión exigida `999.0.0`) | nuevo, vacío | consola `FAILED — DISCARD MODEL` con ` - gate: package version is '0.1.0', expected '999.0.0'`; el script **no** abre el modelo; modelo vacío (0 nudos, 0 miembros, sin `REF1`); `ram-readback.json` FAILED sin valores | consola + lectura de vuelta + rejillas y barra de estado | elementos creados o `READBACK OK` | W2-06 |
| **OV-I65-07b** | `inj-OV07b\rackcad-ram-build.py` (función `rackcad_nonexistent_function` exigida) | nuevo, vacío | consola `FAILED — DISCARD MODEL` con ` - gate: core function missing: rackcad_nonexistent_function`; modelo vacío; `ram-readback.json` FAILED sin valores | consola + lectura de vuelta + rejillas y barra de estado | elementos creados o `READBACK OK` | W2-06 (o cierre de RAM) |

Ningún caso usa la lectura de vuelta de P2 (`e622f429…`). No se ejecuta ningún análisis, ni otro script que los de la tabla.

## 6. Paradas generales

| # | Parada |
|---|---|
| W2-ST-01 | Falta una condición de §3 (no se abre RAM) |
| W2-ST-02 | Identidad distinta (§1) antes o después; licencia no disponible; aviso de seguridad o diálogo modal no previsto |
| W2-ST-03 | Script con otra ruta u otro SHA-256 en «Ejecutar archivo…» (no se ejecuta) |
| W2-ST-04 | Un `ram-readback.json` o `ram-dump.json` que ya existía antes de ejecutar |
| W2-ST-05 | Resultado distinto del esperado en un caso: se registra literal, **sin reintento** y sin cambiar paquetes; se detienen solo las operaciones que dependen de él (de OV-01 dependen su volcado, su comparación y el control de OV-05); los casos independientes siguen salvo que RAM quede en un estado dudoso |
| W2-ST-06 | Pegado accidental del portapapeles o renombrado accidental en un diálogo: Ctrl+Z inmediato, registro y comprobación en disco; si alteró algo fuera del modelo desechable, STOP |
| W2-ST-07 | Escritura fuera de la carpeta de la corrida, cambios de `TRUSTEDPATHS`/seguridad, o necesidad de guardar un modelo, analizar o cambiar código |
| W2-ST-08 | El usuario empieza a usar el equipo, o I-64/I-52 comunican un conflicto |

Toda parada: registro literal, aviso de cierre y decisión del Owner. Ninguna parada se resuelve cambiando código o paquetes.

## 7. Evidencia y cierre

Identidades y hashes PRE y POST (incluidos los tres archivos de cada paquete); capturas de RAM (modelo vacío, nudo de prueba de OV-04,
«Ejecutar archivo…» de cada script, consola de cada caso, rejillas y barra de estado, descartes); texto literal de cada consola; cada
`ram-readback.json`; `ram-dump.json` de OV-01; informes del comparador (`w2-OV01-comparison.json`, `w2-OV05-control.json`) y sus consolas;
confirmación de 0 combinaciones en los dos volcados; diferencias de `Config.ini`; K1/K2; avisos de apertura y cierre; horas UTC.

## 8. Después de W2

Informe con el resultado de cada OV y las diferencias frente a los oráculos; **parada** y solicitud de la revisión final de cierre de F4
(Architect y Owner), aunque todo pase. F4 GATE PASS no se declara.

# I-65 F4 — medición M-05c en RAM Elements (custodia, 2026-10-14)

- **Autoridad:** disposición del Owner «RAM M-05c Authorization / Engineering Validation»
  ([texto literal](../../../decisions/I-65-owner-authorization-m05c-ram.original.txt)); decisiones §52. RAM EXECUTION = M-05c ONLY;
  AUTOCAD EXECUTION = NO; MAPPING UPDATE = NO; F4 GATE PASS = NO.
- **Ejecutor:** Coordinator con Computer Use (acceso concedido a «RAM Elements 2026», nivel completo). Corrida `R20261014T015051Z`, de
  01:52Z a 02:03Z (19:52 a 20:03, hora local). Plan: [I-65-f4-m05c-plan.md](../../../../initiatives/I-65-f4-m05c-plan.md).
- **Qué no se hizo:** no se ejecutó el adaptador ni el script de construcción; no se generó ningún paquete; no se abrió AutoCAD; no se
  guardó ni modificó ningún modelo existente; no se abrió ningún editor de secciones ni de materiales.

## 1. Entorno y scripts

| Elemento | Valor |
|---|---|
| `RAMElements.exe` | SHA-256 `7dcb48555741fa368f5d5f6a14e760fe0662dd5c0b3cc53f6e88f1c0231e7970` (el de P1) |
| Licencia (barra de estado) | «Licencia: Practicante» |
| Procesos al empezar y al terminar | `RAMElements.exe` 0 → 1 → 0; `acad.exe` 0 durante toda la corrida |
| Scripts | generados por la herramienta de F4 (`dump-script --out <carpeta nueva> --adapter-version 1.0.0`), compilada desde el worktree en `93606ea0` (código de la herramienta y de la plantilla igual al verificado en F4-T3/F4-T6); puerta de capacidad: `ramelements` 0.1.0 y las funciones del núcleo `lc_is_combo`, `lc_term_count`, `lc_term_factor` y `lc_term_lc_index`, entre otras |
| Script `MZ` | [mz-rackcad-ram-dump.py.txt](mz-rackcad-ram-dump.py.txt), SHA-256 `f18a3ecf79b345b3e06b6f88b0d57ed4e7dd961f0177797fc3baf7593a988b69` (carpeta embebida `D:\r65-m05c\R20261014T015051Z\mz`) |
| Script `MP` | [mp-rackcad-ram-dump.py.txt](mp-rackcad-ram-dump.py.txt), SHA-256 `e53c13452fa2b09e97615b82d99793a401fc1b7c3fbe1d5567a34c179065e048` (carpeta embebida `...\mp`) |

## 2. Pasos y resultados

| # | Paso | Resultado | Captura |
|---|---|---|---|
| 1 | RAM abre con un modelo **nuevo, sin título y vacío** («Elementos: 0», caso `CM=Carga Muerta`) | modelo `MZ` | [01](01-ram-modelo-vacio-inicial.jpg), [02](02-estados-de-carga-inicial.jpg) |
| 2 | Estados de carga: casos `A` (categoría DL) y `B` (LL), sin combinaciones. La interfaz **no ofrece la categoría «Other»** (ASH, CRANE, DL, EQ, HAIL, LL, LLR, SNOW, WIND, FLUID, EARTH, NOTIONAL, TEMPERATURE) y exige una descripción | casos creados | [03](03-mz-casos-a-b.jpg), [04](04-mz-aviso-descripcion-b.jpg) |
| 3 | Python Console → «Ejecutar archivo…» → script `MZ` | consola `DUMP OK`; «Script ejecutado con éxito» | [05](05-mz-dump-ok.jpg) |
| 4 | Modelo nuevo (`MP`); «¿Desea salvarlo?» → **No** | `MZ` descartado sin guardar | [06](06-mz-guardar-no.jpg) |
| 5 | Casos `A` (DL) y `B` (LL); combinación `C1` en la rejilla: CM 0, A 1.2, B 1.6 (Tipo «No asignado»); estado «C1=1.2A+1.6B» | combinación creada a mano | [07](07-mp-combinacion-c1.jpg), [08](08-mp-estado-c1.jpg) |
| 6 | «Ejecutar archivo…» → script `MP` | consola `DUMP OK` | [09](09-mp-dump-ok.jpg) |
| 7 | Inspección de solo lectura de las bases (§4) | sin cambios en las bases | [10](10-secciones-grupos.jpg)-[15](15-steel-a992-gr50.jpg) |
| 8 | Cerrar RAM; «¿Desea salvarlo?» → **No** | RAM cerrado sin guardar; sin el error gRPC de P1 | [16](16-cierre-guardar-no.jpg) |

## 3. Volcados

| Volcado | SHA-256 | `status` | Unidades | Casos (id, categoría) | `combinations` | `errors` |
|---|---|---|---|---|---|---|
| [mz-ram-dump.json](mz-ram-dump.json) | `ed7f3ca77d4492624c3cf02478363443bbc767454275634069fd592a0b7dcdec` | `DUMP OK` | `KIP_FT` → lectura `LB_IN` → restaurado | `CM` DL, `A` DL, `B` LL | `[]` | `[]` |
| [mp-ram-dump.json](mp-ram-dump.json) | `27162b052c1793461054ded13160d4838021d1839ec7db3d81f0b4272520797c` | `DUMP OK` | ídem | ídem | `[{"id": "C1", "terms": [{"case": "A", "factor": 1.2}, {"case": "B", "factor": 1.6}]}]` | `[]` |

El término de `CM` con factor 0 de la rejilla no aparece en el volcado (la API lista los términos con factor no nulo). La combinación leída
es equivalente a la creada.

## 4. Inspección de solo lectura de las bases de RAM

- **Grupos de secciones:** Australia, Brazil, Canada, China, Europe, India, Japan, United Kingdom, United States. **No existe un grupo
  «Montilla»**: el diálogo abrió en «Australia» aunque la configuración del usuario apunta a «Montilla» (DIF-03).
- **United States / `W`** (etiqueta `W.leo`): contiene `W 12X26`. **United States / `HSS_SQR`** (etiqueta `HSS_Rect.leo`): contiene
  `HSS_SQR 4X4X1_4`.
- **Materiales United States / `Steel`:** contiene `A992 Gr50` y `A500 GrC rectangular`, junto a `A500 GrA pipes`, `A500 GrB rectangular`,
  `A500 GrB rounded`, `A500 GrC rounded`, `A1085` y otros. No se usó ninguna sustitución.
- Los diálogos **no muestran la edición** de AISC de la base: V-01 no se resuelve por la interfaz.
- Bases instaladas sin cambios: `W.sec` `dffd9e00…7b977`, `HSS_SQR.sec` `992ddac9…3df0`, `Steel.mat` `2a8b4239…39ce1` (los mismos
  hashes que la pre-lectura).

## 5. Efectos en el host y controles

- **Configuración de usuario de RAM** (`%APPDATA%\Bentley\Engineering\RAM Elements\26.0.1\RE\Config.ini`): RAM la reescribió al cerrar
  con estado de interfaz. Cambiaron los contadores de uso (`accLoadConditionsManager` 2→4, `accMatDB` 1→2, `accPythonConsole` 2→4,
  `accQANew` +1, `accSeccDB` 2→3), la última carpeta de scripts (`PythonScripts`, de la carpeta de P1 a la de esta corrida) y el último
  material visto (`frameMaterialesTableName`/`RecordName`: de Dimension Lumber/Alaska_Cedar_SelectStr a Steel/A992 Gr50). **No
  cambió** `frameSeccionesDataBaseName=Montilla`. La copia previa se conserva fuera del repositorio
  (`D:\r65-m05c\R20261014T015051Z\host-before\RE-Config.ini.before`, SHA-256 `98bbd76e…641e`); el archivo actual tiene SHA-256
  `2505ca6e…282e`. No se restauró: es estado de interfaz de RAM; restaurarlo lo decide el Owner.
- **Portapapeles:** Computer Use escribió las dos rutas de script a través del portapapeles del usuario, cuyo contenido anterior quedó
  reemplazado.
- **Checkouts:** K1/K2 antes y después sin cambios en el checkout principal ni en los demás worktrees.
- **AutoCAD:** no se abrió; el perfil, `TRUSTEDPATHS`, `SECURELOAD`, el registro y los plugins de AutoCAD no se tocaron. Avisos de
  cortesía a I-52 e I-64 al empezar y al terminar.

## 6. Evaluación frente al plan (D-1..D-4)

| # | Demostración | Resultado |
|---|---|---|
| D-1 | Una combinación existente se detecta, con sus casos y factores | **PASS**: `C1` con `A`·1.2 y `B`·1.6 |
| D-2 | Distingue cero combinaciones de una lectura incompleta | **PASS**, con el límite declarado: el mismo camino de lectura dio `PASSED` con `[]` (`MZ`) y `PASSED` con un elemento (`MP`). Una lectura incompleta termina en `FAILED` por construcción de la plantilla y el lector la rechaza (pruebas verificadas `UnVolcadoConEstadoDeFalloSeRechazaConSuPropioCodigo`, `ElRespaldoMinimoDeUnFalloTambienSeRechazaConElCodigoDeFallo`, `CualquierEstadoQueNoSeaExactamenteDumpOkSeRechazaComoFallo`). No se provocó un `FAILED` en RAM: habría exigido inyectar un fallo, fuera de la autorización |
| D-3 | Los errores no producen PASS global | cubierto por las pruebas verificadas de F4-T6 (`Combinations_nobody_could_read_never_give_a_pass_and_no_failing_row_gives_incomplete`, `Combinations_declared_unread_with_a_decision_end_incomplete_with_their_own_code`, `Combinations_a_dump_could_not_read_are_not_evaluated`); M-05c no ejecuta el comparador (no hay paquete) |
| D-4 | Lo no verificable produce INCOMPLETE o STOP | resultado de la tabla del plan: «`MZ` `PASSED` con `[]` y `MP` `PASSED` con `C1` exacto» → **interfaz verificada** → regla CMB-3 |

## 7. Clasificación

**LECTURA DE COMBINACIONES VERIFICADA** (control positivo M-05c PASS) para: `RAMElements.exe` `7dcb4855…7970`, `ramelements` 0.1.0 y el
script de volcado de la versión de adaptador 1.0.0 generado por la plantilla verificada. La clasificación vale mientras esos tres no
cambien; si cambia cualquiera, M-05c se repite antes de usar CMB-3.

## 8. Hallazgo para el procedimiento manual

La interfaz de RAM no ofrece la categoría «Other». En el modelo manual la categoría del caso `REF1` es **informativa** para el comparador,
pero no puede quedar vacía si el caso tiene cargas (A65-F4-05). Los casos creados en la interfaz se leen como texto (`"DL"`, `"LL"`). El
procedimiento manual se ajusta (§8 del procedimiento de `REF-CANT-01`).

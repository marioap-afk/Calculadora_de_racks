# I-65 F4 — corrección de V-11 y revalidación de solo lectura del paquete del segundo smoke (2026-10-16)

- **Autoridad:** «OWNER DISPOSITION / Second Smoke V-11 Correction and Read-Only Revalidation»
  ([texto literal](../../../../decisions/I-65-owner-disposition-v11-correction-revalidation.original.txt)); decisiones §70.
  V-11 VALIDATOR CORRECTION = AUTHORIZED; EXISTING PACKAGE REVALIDATION = AUTHORIZED; AUTOCAD = NO; RAM = NO; PRODUCT CODE CHANGES = NO;
  F4 GATE PASS = NO.
- **Ejecutor:** Coordinator, sin AutoCAD (`acad.exe` = 0 antes y después), sin RAM, sin Codex ni Workers. No se tocó ni el producto ni
  la tabla, y no se generó ningún paquete nuevo.
- **Registros históricos sin cambios:** resultado original 15/16 ([informe v1](../validator-report.json), SHA-256 `0d415a3f…`),
  validador original ([v1](../../validate-smoke2-package.py.txt), `8b19a9c725c66a629b03b6ddea7bba70149788508e38e92e673c9a6852812c17`) y
  diagnóstico ([v11-diagnostic.json](../v11-diagnostic.json), `c22f2222…`).

## 1. Causa raíz confirmada

El producto serializa la carga útil en dos capas. Lo confirma la lectura del código de `51fc82f1`:

1. `RamBuildScriptTemplate.Render` sustituye el marcador `@@PAYLOAD@@` de la línea `PAYLOAD = "@@PAYLOAD@@"` por
   `Escape(payloadJson)`, donde `payloadJson` es el JSON de la carga útil (capa JSON: `\` → `\\`).
2. `Escape` aplica el escape de un literal de Python entre comillas dobles: `text.Replace("\\", "\\\\").Replace("\"", "\\\"")`
   (capa Python: `\` → `\\`).
3. El script lee la carga con `data = json.loads(PAYLOAD)` (línea 286 del script generado) y escribe su lectura en
   `data["packageFolder"] + "\\" + READBACK_NAME`.

Por eso cada `\` de la carpeta aparece **cuatro veces** en los bytes del script. La V-11 de la versión 1 buscaba la carpeta cruda o con
**una** sola capa (`json.dumps(typed)[1:-1]`), nunca con las dos. Además, su autoprueba fabricaba los paquetes sintéticos con una sola
capa, así que no podía detectarlo. **El defecto estaba en el validador, no en el producto.**

## 2. Cambio exacto del validador ([v2](validate-smoke2-package-v2.py.txt), [diff v1 → v2](validate-smoke2-package-v1-to-v2.diff.txt))

Solo cambian V-11, el `import ast`, el docstring, `validatorVersion: 2` en el informe y la apertura exclusiva (`"x"`) del informe para
no sobrescribir. Las demás comprobaciones son idénticas a la v1. La nueva V-11:

1. **Extrae la carga útil sin ejecutar el script.** Con `ast.parse` exige una única asignación `PAYLOAD = "<literal de cadena>"` en el
   nivel superior y ninguna otra escritura de `PAYLOAD`. Exige además que el script la lea exactamente una vez y como
   `json.loads(PAYLOAD)`, sin transformarla. Después deshace las dos capas, igual que el script: primero el valor del literal de Python
   (`ast.Constant`) y luego `json.loads`.
2. **Recupera la carpeta efectiva:** `packageFolder` de la carga útil decodificada.
3. **La compara sin normalizar** (igualdad exacta de cadena) con `packageFolder` del manifiesto, con la carpeta tecleada y con la ruta
   real del paquete (`os.path.abspath`). Comprueba también el destino en disco con `os.path.samefile`.
4. **Exige que no haya diferencias de separadores, caracteres ni destino:** ruta Windows absoluta, sin `/` ni caracteres de control, en
   forma canónica (`normpath(carpeta) == carpeta`: sin separadores dobles ni finales ni `..`). Comprueba además que la huella efectiva sea
   igual a `analysisInputFingerprint` del manifiesto y que la huella aparezca en el texto del script.
5. **Falla** si no se puede recuperar la carga útil o si falla cualquiera de las comparaciones. No es una búsqueda de texto más
   permisiva: no hay ninguna búsqueda de la carpeta como texto.

## 3. Pruebas positivas y negativas ([script](test-v11-validator.py.txt), [salida](test-v11-validator.out.txt), [detalle](test-v11-validator.json))

Las pruebas leen el paquete real **solo para copiarlo**. Cada caso escribe una copia en
`D:\r65-smoke\51fc82f1\run-R20261016T1918Z\v11-correction\selftest\<caso>\`, reubica la carga útil con la misma serialización del
producto y aplica **una sola** mutación. Luego ejecutan el validador v2 como subproceso y leen el resultado de V-11.

| Caso | Mutación | V-11 esperado / observado | Comprobación que falla |
|---|---|---|---|
| P01 | copia exacta reubicada | PASS / PASS | — |
| P02 | ruta con espacio y `ñ` (escape `\u00f1`) | PASS / PASS | — |
| N01 | carga útil a `pkg-02` | FAIL / FAIL | manifiesto, tecleada, ruta real, destino |
| N02 | `/` en lugar de `\` | FAIL / FAIL | manifiesto, tecleada, ruta real, absoluta, `/`, canónica |
| N03 | **un nivel de escape menos** (sin capa JSON) | FAIL / FAIL | carga no recuperable: `JSONDecodeError` (escape no válido) |
| N04 | **dos niveles menos** (sin escapar `\`) | FAIL / FAIL | carga no recuperable: `SyntaxError` (escape `\N`) |
| N05 | **un nivel de más** (doble capa JSON) | FAIL / FAIL | manifiesto, tecleada, ruta real, canónica |
| N06 | `R65` en mayúscula (mismo destino en disco) | FAIL / FAIL | manifiesto, tecleada, ruta real (igualdad exacta) |
| N07 | `\` final | FAIL / FAIL | manifiesto, tecleada, ruta real, canónica |
| N08 | separador doble interior | FAIL / FAIL | manifiesto, tecleada, ruta real, canónica |
| N09 | manifiesto a `pkg-02` (carga útil correcta) | FAIL / FAIL | manifiesto |
| N10 | paquete en otra ruta real (A ≠ B) | FAIL / FAIL | ruta real, destino |
| N11 | `PAYLOAD` = expresión con efecto lateral | FAIL / FAIL | «no es un literal de cadena»; el archivo centinela **no** se creó: el script no se ejecutó |
| N12 | `PAYLOAD` reasignado con otra carpeta | FAIL / FAIL | «asignarse exactamente una vez (asignaciones=2)» |
| N13 | el script transforma `PAYLOAD` antes de `json.loads` | FAIL / FAIL | «no lee PAYLOAD … como json.loads(PAYLOAD)» |
| N14 | huella alterada en la carga útil | FAIL / FAIL | huella efectiva, huella en el texto |
| N15 | guion U+2010 en lugar de `-` | FAIL / FAIL | manifiesto, tecleada, ruta real, destino |
| N16 | carpeta tecleada con `/` | FAIL / FAIL | tecleada |
| Control | validador **v1** sobre P01 | FAIL (defecto reproducido) | — |

**Resultado: PASS** (2 positivos, 16 negativos y el control). El paquete real no cambió durante las pruebas.

## 4. Revalidación del paquete existente ([informe v2](validator-report-v2.json))

`python -I validate_smoke2_package_v2.py D:\r65-smoke\51fc82f1\run-R20261016T1918Z\pkg-01 D:\r65-smoke\51fc82f1\expected-inputs.json
D:\r65-smoke\51fc82f1\run-R20261016T1918Z\pkg-01 …\validator-report-v2.json` (los mismos argumentos que la corrida original;
2026-10-16T20:40:33Z) → **PASS 16/16**. V-11: carpeta efectiva = manifiesto = tecleada = ruta real =
`D:\r65-smoke\51fc82f1\run-R20261016T1918Z\pkg-01`; huella efectiva `505941e2…ba61`; ninguna comprobación fallida. Las otras 15
comprobaciones dan el mismo resultado que en la v1.

## 5. SHA-256 del paquete antes y después ([antes](pkg-hashes-before.txt) 20:36:56Z, [después](pkg-hashes-after.txt) 20:40:33Z)

| Archivo | Antes | Después |
|---|---|---|
| `manifest.json` | `19fba962297badc95dd008bcf8d1477993c55a7ff8b5347d68e311e246f53a78` | idéntico |
| `rackcad-ram-build.py` | `d9b41aab69643c01971e756fb9323a8f35dad6b33008598655c753fe2f92d719` | idéntico |
| `structural-loads.json` | `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05` | idéntico |

También coinciden los tamaños y las fechas de modificación (19:25:15Z, las de la generación). En `pkg-hashes-before.txt`, la hora y el
recuento de procesos quedaron en una misma primera línea (formato de la orden); la comparación omite esa línea en los dos archivos.

## 6. Estado

Se cumplen las dos condiciones del Owner: el mismo paquete pasa 16/16, y las pruebas negativas demuestran que la nueva V-11 detecta
rutas alteradas y distintos niveles de escape. Por tanto:

**SECOND SMOKE = PASS AFTER VALIDATOR CORRECTION.**

No es GATE PASS de F4 ni autoriza RAM.

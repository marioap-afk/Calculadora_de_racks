# I-65 — F4: plan de la medición M-05c (lectura de combinaciones por el volcado)

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «F4 Host Coordination and Combination Validation» §3
([texto literal](../automation/decisions/I-65-owner-disposition-f4-host-coordination-combination-validation.original.txt); decisiones
[§50](../automation/decisions/I-65.md)). Concreta la regla CMB-2 de la [resolución de OD-F4-14](I-65-f4-od-f4-14-combinations.md).

> **M-05c = APPROVED FOR PLANNING.** RAM EXECUTION = NO: nada de este plan se ejecuta sin una autorización específica para abrir RAM
> Elements. No cambia código, el Freeze ni contratos congelados, y no convierte `INCOMPLETE` en `PASS`.

## 1. Qué tiene que demostrar y cómo

| # | Demostración exigida por el Owner | Cómo la cubre M-05c | Evidencia |
|---|---|---|---|
| D-1 | Una combinación existente se detecta | modelo desechable `MP` con una combinación creada a mano; el volcado debe listarla con su id, sus términos y sus factores exactos | `ram-dump.json` de `MP` y captura de la combinación en la interfaz |
| D-2 | El volcado distingue cero combinaciones de combinaciones no evaluadas | el script de volcado empieza en `status: FAILED` y solo pasa a `PASSED` si la lectura completa, incluida la de las combinaciones, termina sin error. Por tanto: `PASSED` con `combinations: []` = **cero leídas**; `FAILED` (excepción al leer) = **no evaluadas**, y el lector rechaza el volcado. El modelo `MZ` (sin combinaciones) debe dar `PASSED` con `[]`, y `MP` (con una) `PASSED` con un elemento: el mismo camino de lectura ve la diferencia | volcados de `MZ` y `MP` |
| D-3 | El comparador no da PASS global con datos incompletos | ya verificado por las pruebas de F4-T6 en `6422e5b0` (`Combinations_nobody_could_read_never_give_a_pass_and_no_failing_row_gives_incomplete`, `Combinations_declared_unread_with_a_decision_end_incomplete_with_their_own_code`, `Combinations_a_dump_could_not_read_are_not_evaluated`); en la primera corrida, toda invocación con `--*-combinations-unread` debe terminar con código 5 (`INCOMPLETE`) y se custodia | pruebas verificadas; informe y código de salida de la corrida |
| D-4 | Lo no verificable produce INCOMPLETE o STOP según el contrato | tabla de §4: cada resultado de M-05c lleva a CMB-3, CMB-4 o CMB-5 | registro de M-05c y disposición aplicada |

**Límite declarado:** el formato del volcado no tiene un marcador propio de «no evaluadas». La distinción se apoya en el estado
`PASSED`/`FAILED` del volcado, en el control positivo y en las opciones del comparador. Pedir un marcador en el volcado sería un cambio de
código con contrato, no autorizado.

## 2. Precondiciones

| # | Condición |
|---|---|
| M-P1 | Autorización específica del Owner para abrir RAM Elements **solo** para M-05c (y, si lo decide, para las lecturas de la interfaz V-01 y V-07 de la hoja) |
| M-P2 | RAM 26.00.01.015 (`RAMElements.exe` SHA-256 `7dcb4855…7970`) con licencia; ningún otro modelo abierto |
| M-P3 | Dos carpetas **nuevas** con ruta corta y un script de volcado en cada una, generados por la sesión con la herramienta de F4: `dump-script --out <carpeta> --adapter-version 1.0.0`. Cada carpeta sirve para un solo volcado, porque `ram-dump.json` se crea en modo exclusivo |
| M-P4 | No hace falta paquete, tabla verificada, AutoCAD ni ventana de host: M-05c no usa el adaptador ni el DLL |

## 3. Pasos (operador en RAM; la sesión prepara, mide y custodia)

| # | Acción | Esperado |
|---|---|---|
| M5-1 | RAM → **File > New**; unidades lb-in; no analizar | modelo vacío con el caso por defecto `CM` |
| M5-2 | Crear dos casos de carga, `A` y `B`, categoría Other, **sin cargas** y **sin combinaciones** | captura de casos y de combinaciones (lista vacía) |
| M5-3 | Python Console → «Ejecutar archivo…» → script de la carpeta 1 (`MZ`) | `ram-dump.json` con `status` `PASSED` y `combinations` `[]`; casos `CM`, `A` y `B` |
| M5-4 | En la interfaz de combinaciones de carga, crear **una** combinación `C1 = 1.2·A + 1.6·B` | captura de la combinación con sus factores |
| M5-5 | Ejecutar el script de la carpeta 2 (`MP`) | `status` `PASSED` y `combinations` = `[{id: C1, terms: [{case: A, factor: 1.2}, {case: B, factor: 1.6}]}]` (orden de términos indiferente) |
| M5-6 | Cerrar el modelo **sin guardar** | sin archivos de modelo nuevos |

Si aparece un diálogo de «los resultados se perderán», responder **No** y STOP. La sesión custodia los dos volcados, los SHA-256 de los
scripts y de `RAMElements.exe` y las capturas.

## 4. Resultados posibles y disposición (D-4)

| Resultado de M-05c | Clasificación | Regla aplicada en la primera corrida |
|---|---|---|
| `MZ` `PASSED` con `[]` y `MP` `PASSED` con `C1` y sus términos y factores exactos | **interfaz verificada** | CMB-3: comparar **sin** `--*-combinations-unread`; 0 en ambos → fila PASS; ≥ 1 → FAIL y STOP |
| `MP` `PASSED` sin `C1`, o con términos o factores distintos | **no verificada** (el volcado no ve lo que existe) | CMB-4: comparar con las opciones de no leídas y la decisión → `INCOMPLETE`; OV-I65-01 = INCOMPLETE |
| `MP` `FAILED` (excepción al leer combinaciones) | **no verificada** | CMB-4 (los modelos del piloto no tienen combinaciones, pero no se puede probar con el volcado) |
| `MZ` `PASSED` con alguna combinación (RAM las crea por defecto) | **caso B** | CMB-5: STOP; la fila «ninguna en ambos» no se puede cumplir tal como está congelada |
| `MZ` `FAILED` | volcado inutilizable | STOP y decisión del Owner |

Ningún resultado convierte un `INCOMPLETE` en `PASS` por sí solo. Con M-05c verificado después de la corrida, la conversión solo se hace
volviendo a comparar los **mismos volcados custodiados** sin las opciones de no leídas (CMB-4 (i)).

## 5. Coordinación

- M-05c no necesita AutoCAD ni ventana de I-52 o I-64. RAM no comparte el perfil de AutoCAD, pero sí la máquina: la sesión avisa por el
  canal a I-52 e I-64 antes y después, por cortesía.
- Puede hacerse **antes** de la validación de las filas y del Candidato. Conviene hacerla en la misma sesión de RAM que las lecturas de
  interfaz V-01 (edición de la base) y V-07 (nombres, tabla, país y «Montilla») de la hoja, si el Owner las autoriza.

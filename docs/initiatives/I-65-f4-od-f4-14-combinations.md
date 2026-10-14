# I-65 — F4: resolución propuesta de OD-F4-14 (combinaciones en OV-I65-01)

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «F4 Engineering Validation and Host Readiness» §4
([texto literal](../automation/decisions/I-65-owner-disposition-f4-validation-host-readiness.original.txt); decisiones
[§49](../automation/decisions/I-65.md)). **Propuesta para decisión del Owner.** No cambia código, el Freeze, ADR-0050 ni contratos
congelados, y **no** convierte `INCOMPLETE` en `PASS`.

## 1. Lo que dicen las autoridades vigentes

| Fuente | Texto relevante |
|---|---|
| Freeze D-02 | «Fuera de V1 (pedir cualquiera → `Unsupported`): … peso propio, **combinaciones**, …» |
| Freeze D-10, LOD-01 | «Un caso `REF1` con cargas del archivo D-10b; sin peso propio **ni combinaciones**» |
| Freeze §15 paso 8 (referencia manual) | «… sin peso propio **ni combinaciones**» |
| Freeze D-09 y manifiesto del paquete | la plantilla no tiene ninguna operación de combinaciones; el manifiesto declara «no load combinations» |
| Freeze D-11 | el volcado fijo de solo lectura vuelca el modelo abierto; los casos sin cargas se ignoran; no menciona las combinaciones |
| Freeze §4 | fila «Combinaciones — ninguna en ambos — exacta»; **no** es informativa |
| Freeze §5, OV-I65-01 | «matriz §4 PASS salvo filas informativas» → la fila de combinaciones **debe** ser PASS |
| Paquete de ingeniería §3 F (CD-I65-F4P-04) | evaluador: el comparador si el volcado puede leerlas (M-05 sin medir); si no, comprobación visual del ingeniero |
| Owner, decisiones §47 y §49 | combinaciones no legibles por una interfaz verificada → **NO EVALUADAS**; ninguna comparación parcial da PASS global; prohibido convertir `INCOMPLETE` en `PASS` |
| Implementación verificada (F4-T3/T6) | el volcado tiene un campo `combinations` (id y términos) que el lector valida; el comparador da `INCOMPLETE` (código 5) con `--*-combinations-unread`, nunca PASS |

## 2. Determinación

**Las combinaciones están explícitamente fuera del modelo piloto** (D-02, LOD-01, §15 paso 8). No deben existir en ninguno de los dos
modelos. La fila de §4 no compara combinaciones: verifica su **ausencia** en ambos modelos, y es obligatoria para OV-I65-01.

Por tanto, el caso normal es el primero de la disposición (fuera del piloto). El segundo (existen pero no se pueden verificar) solo aparece
si alguna evidencia muestra una combinación en uno de los modelos.

Lo que sigue sin medir no es si hay combinaciones, sino si el volcado **lee** las combinaciones que existen (M-05). Mientras eso no se
mida, un arreglo vacío en el volcado no prueba la ausencia.

## 3. Disposición contractual propuesta

| Regla | Contenido |
|---|---|
| **CMB-1 Clasificación** | Las combinaciones están fuera del modelo piloto V1. La fila «Combinaciones» de §4 es una **verificación de ausencia**, obligatoria para OV-I65-01. |
| **CMB-2 Interfaz verificada (control positivo M-05c)** | El volcado lee las combinaciones mediante una **interfaz verificada** solo si un control positivo lo demuestra: en la corrida autorizada, en un **modelo desechable** distinto del generado y del manual, se crea a mano una combinación de un caso, se ejecuta el **mismo** script de volcado en una carpeta nueva y el volcado la lista con su id y sus términos. Después se descarta el modelo sin guardar. Sin cambios de código. Si el volcado no la lista o falla, la interfaz **no** está verificada. |
| **CMB-3 Caso A, fuera del piloto y verificable** (M-05c PASS) | El comparador se ejecuta **sin** las opciones `--*-combinations-unread`. Con 0 combinaciones en ambos volcados → fila PASS. Con ≥ 1 en cualquiera → fila **FAIL** y OV-I65-01 FAIL (el modelo sale del piloto): STOP; si es el manual, se corrige y se repite su volcado en carpeta nueva; si es el generado, es un defecto del producto. |
| **CMB-4 Caso A sin interfaz verificada** (M-05c no hecho o fallido) | El comparador se ejecuta con `--generated-combinations-unread`, `--manual-combinations-unread` y `--combinations-decision "<texto literal del Owner o del ingeniero>"`. El resultado global es **`INCOMPLETE`** y la fila queda «NO EVALUADA: fuera del modelo piloto; ausencia no verificable por interfaz». Se registran como evidencia complementaria, **no** como PASS: la garantía de construcción del generado (D-09, manifiesto, lectura de vuelta) y, del manual, el procedimiento más la captura de la interfaz. **OV-I65-01 = INCOMPLETE**: no sostiene el GATE PASS de F4. Solo puede pasar a PASS por: (i) un M-05c PASS posterior y una nueva comparación **sin** esas opciones sobre los **mismos volcados custodiados**, sin repetir los modelos; o (ii) una A-n del Freeze aprobada expresamente por el Owner que cambie el evaluador de la fila. Ninguna de las dos es automática y la (ii) no se propone. |
| **CMB-5 Caso B, existen pero no se pueden verificar** | Si alguna evidencia (captura de la interfaz, volcado, o RAM que crea combinaciones por defecto con File > New) muestra una combinación en uno de los modelos y la interfaz no está verificada, la fila no se puede evaluar **y** el modelo está fuera del piloto. OV-I65-01 = INCOMPLETE y **STOP**. Combinación creada a mano en el manual → se elimina y se repite el volcado. Combinación en el generado → defecto (la plantilla no debe crearlas) → STOP y decisión del Owner. Combinaciones por defecto de RAM → la fila «ninguna en ambos» no se puede cumplir tal como está congelada → STOP; tratarlas como los casos por defecto sin cargas exigiría una A-n y un contrato, no autorizados. |
| **CMB-6 Texto de decisión** | `--combinations-decision` lleva el texto literal de quien decide. Queda en el informe y **nunca** convierte `INCOMPLETE` en `PASS`. |
| **CMB-7 Alcance** | Sin cambios de código ni de contratos congelados: el comparador de F4-T6 ya implementa `INCOMPLETE`. La regla solo fija el procedimiento (con o sin las opciones), el control M-05c y la clasificación de OV-I65-01. |

## 4. Recomendación

Incluir **M-05c** en la primera corrida: son pocos minutos en un modelo desechable y es la única vía, sin cambiar el Freeze, por la que la
fila de combinaciones —y con ella OV-I65-01— puede llegar a PASS de forma legítima. Si el Owner no lo autoriza, la primera corrida acaba,
como máximo, en OV-I65-01 = INCOMPLETE (CMB-4).

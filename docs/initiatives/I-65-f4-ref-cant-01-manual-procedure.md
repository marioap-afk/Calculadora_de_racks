# I-65 — F4: procedimiento para construir `REF-CANT-01` a mano en RAM Elements

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «F4 Engineering Validation Preparation» §5
([texto literal](../automation/decisions/I-65-owner-disposition-f4-engineering-validation-preparation.original.txt); decisiones
[§47](../automation/decisions/I-65.md)). Convierte la especificación manual §2 y el paso E del
[paquete de validación de ingeniería](I-65-f4-engineering-validation-package.md) en una lista de pasos para el operador. **Todos los
valores vienen de ese paquete** (golden de F1 y F3, archivo de cargas del fixture de F1); este documento no añade ninguno: solo indica
cuántos decimales teclear, según la regla que el propio paquete fija.

> **Estado: PREPARADO, NO EJECUTABLE TODAVÍA.** RAM EXECUTION = NO. Este procedimiento se ejecuta solo cuando el Owner autorice
> expresamente la corrida en RAM (condiciones en [I-65-f4-real-run-readiness.md](I-65-f4-real-run-readiness.md)). El modelo manual es la
> referencia de equivalencia de transferencia (D-11): **no** es una validación estructural (Owner Q-O3).

## 0. Roles y registro

- **Operador** (Owner o ingeniero, OD-F4-08): construye el modelo en la interfaz y anota observaciones; firma el registro de §13.
- **Sesión del Coordinator:** prepara la carpeta y el script de volcado, mide hashes, horas y tamaños, custodia la evidencia y aplica las
  paradas. Al operador no se le piden datos de máquina.
- Ningún paso guarda el modelo salvo decisión expresa del ingeniero (D-11; OD-F4-11).

## 1. Precondiciones (todas)

| # | Condición | Quién la comprueba |
|---|---|---|
| PM-01 | Autorización expresa del Owner para abrir RAM Elements y construir este modelo | sesión |
| PM-02 | RAM Elements 26.00.01.015 (`RAMElements.exe` SHA-256 `7dcb48555741fa368f5d5f6a14e760fe0662dd5c0b3cc53f6e88f1c0231e7970`) con licencia activa | sesión (hash); operador (licencia) |
| PM-03 | Hoja de validación firmada o, como mínimo, nombres de sección y material confirmados en la interfaz ([hoja](I-65-f4-engineering-validation-sheet.md) §2.1 y §3.1) y DIF-03 («Montilla») revisada | ingeniero |
| PM-04 | Ningún otro modelo abierto en RAM; nada del generado en el mismo modelo | operador |
| PM-05 | Carpeta nueva para el volcado del modelo manual, distinta de la del generado (§9) | sesión |

## 2. Configuración del modelo

1. RAM Elements → **File > New**. No partir de otro modelo. No analizar en ningún momento.
2. Unidades de la interfaz: **lb-in** (pulgadas y libras-fuerza).
3. Dejar el caso que crea File > New (`CM` en la interfaz en español) **sin cargas**; el comparador ignora los casos sin cargas.

Sistema de coordenadas: ya está en el de RAM (`X_RAM = X`, `Y_RAM = Z`, `Z_RAM = −Y`; Y vertical hacia arriba; la estación vive en el
plano Y-Z de RAM; los brazos van hacia −Z de RAM).

## 3. Nudos y coordenadas (in)

Teclear **4 decimales** (tolerancia de la matriz §4: 0.02 in, euclídea). Descripción sugerida = id neutral (el comparador empareja por
posición, no por descripción).

| # | Nudo (descripción) | X_RAM | Y_RAM (4 dec.) | Z_RAM (4 dec.) | Y_RAM completo | Z_RAM completo |
|---|---|---|---|---|---|---|
| N1 | `COL.FOOT` | 0 | 0.2500 | 6.1000 | 0.25 | 6.1 |
| N2 | `ARM.1.PositiveY.J` | 0 | 20.2672 | 6.1000 | 20.26716169689737 | 6.1 |
| N3 | `ARM.2.PositiveY.J` | 0 | 48.2672 | 6.1000 | 48.267161696897375 | 6.1 |
| N4 | `ARM.3.PositiveY.J` | 0 | 76.2672 | 6.1000 | 76.26716169689738 | 6.1 |
| N5 | `COL.TOP` | 0 | 90.7500 | 6.1000 | 90.75 | 6.1 |
| N6 | `ARM.1.PositiveY.TIP` | 0 | 21.8103 | −36.2261 | 21.810300688090436 | −36.22609804415268 |
| N7 | `ARM.2.PositiveY.TIP` | 0 | 49.8103 | −36.2261 | 49.81030068809044 | −36.22609804415268 |
| N8 | `ARM.3.PositiveY.TIP` | 0 | 77.8103 | −36.2261 | 77.81030068809044 | −36.22609804415268 |

☐ 8 nudos, todos con X_RAM = 0.

## 4. Miembros y conectividad

**El sentido I → J importa:** un miembro dibujado al revés invierte `l1`, cambia `l2` y `l3` y la fila de ejes locales falla aunque los
nudos coincidan. Columnas de abajo arriba; brazos del nudo de la columna a la punta. La columna va **partida** en los nudos J.

| # | Miembro (descripción) | Nudo I → nudo J | Longitud esperada (in) |
|---|---|---|---|
| M1 | `COL.1` | `COL.FOOT` → `ARM.1.PositiveY.J` | 20.01716169689737 |
| M2 | `COL.2` | `ARM.1.PositiveY.J` → `ARM.2.PositiveY.J` | 28 |
| M3 | `COL.3` | `ARM.2.PositiveY.J` → `ARM.3.PositiveY.J` | 28 |
| M4 | `COL.4` | `ARM.3.PositiveY.J` → `COL.TOP` | 14.48283830310262 |
| M5 | `ARM.1.PositiveY` | `ARM.1.PositiveY.J` → `ARM.1.PositiveY.TIP` | 42.35421884050472 |
| M6 | `ARM.2.PositiveY` | `ARM.2.PositiveY.J` → `ARM.2.PositiveY.TIP` | 42.35421884050472 |
| M7 | `ARM.3.PositiveY` | `ARM.3.PositiveY.J` → `ARM.3.PositiveY.TIP` | 42.35421884050472 |

☐ 7 miembros; ☐ longitudes mostradas por RAM iguales a la tabla (redondeo de la interfaz aparte).

## 5. Secciones y materiales

| Miembros | Sección de RAM | Material de RAM |
|---|---|---|
| `COL.1`..`COL.4` | `W 12X26` (tabla `W`, United States) | `A992 Gr50` |
| `ARM.1.PositiveY`..`ARM.3.PositiveY` | `HSS_SQR 4X4X1_4` (tabla `HSS_SQR`, United States) | `A500 GrC rectangular` |

Usar exactamente los nombres de la hoja firmada; si la interfaz ofrece el mismo nombre en una base de usuario, usar la base que el
ingeniero haya fijado en la hoja (DIF-03) y anotarla.

## 6. Ejes locales y orientación

| Miembros | Giro θ a teclear | `l1` esperado | `l2` (peralte) esperado | `l3` esperado |
|---|---|---|---|---|
| `COL.1`..`COL.4` | **−90°** (si la interfaz lo muestra como 270°, es el mismo giro) | (0, 1, 0) | (0, 0, −1) | (−1, 0, 0) |
| `ARM.k.PositiveY` | 0° | (0, 0.036434127070177715, −0.9993360567820188) | (0, 0.9993360567820188, 0.036434127070177715) | (1, 0, 0) |

- **No usar +90° en las columnas:** deja el peralte en el plano pero con `l2` y `l3` opuestos a los del generado; la fila de ejes lo
  rechaza (OD-F4-05, θ = −90° confirmado por el Owner).
- ☐ Comprobación visual: el eje 2 de las columnas apunta a −Z de RAM (hacia los brazos); el eje 2 de los brazos, hacia arriba.
- Tolerancia de la matriz §4 para ejes: 0.1°.

## 7. Apoyos y liberaciones

Banderas en el sistema **global de RAM** (resultado de la permutación de D-04; se marcan tal cual en la interfaz; el ingeniero confirma la
etiqueta, p. ej. `Tx Ty Rz`):

| Nudo | Restringidos | Libres | Supuesto |
|---|---|---|---|
| `COL.FOOT` | `dx dy dz rx ry rz` | ninguno | SUP-01 |
| los otros siete | `dx ry rz` | `dy dz rx` | SUP-03 |

- Liberaciones: **ninguna**, en los dos extremos de los siete miembros (CON-01).
- Propiedades de miembro con sus valores por defecto: punto cardinal CENTROID, offsets rígidos 0, factores de rigidez 1, comportamiento
  axial normal, resortes 0 (`[P1]` R1). No cambiarlas.

## 8. Cargas P1–P3

1. Crear un caso con id **`REF1`**, una descripción (RAM la exige) y una categoría **no vacía**. La interfaz de RAM **no ofrece «Other»**
   (M-05c, 2026-10-14: ASH, CRANE, DL, EQ, HAIL, LL, LLR, SNOW, WIND, FLUID, EARTH, NOTIONAL, TEMPERATURE). En el manual la categoría es
   **informativa** para el comparador, pero no puede quedar vacía en un caso con cargas (A65-F4-05). Elegir una de la lista, por ejemplo
   `DL`, y anotarla; el volcado la lee como texto (`"DL"`).
2. Sin peso propio, sin cargas nodales ni distribuidas; **no crear combinaciones**.
3. Cada carga es una **carga puntual en miembro**, dirección **`GLOBAL_Y` con valor negativo** (negativo = hacia abajo; sin doble
   signo), posición **en % del miembro analítico medida desde el nudo I** (`ARM.k.PositiveY.J`), con **al menos 3 decimales** (con un
   decimal, `P2` y `P3` fallan la tolerancia de 0.02 in).

| Carga | Miembro | Valor `GLOBAL_Y` (lbf) | Posición (%) completa | Posición mínima a teclear (3 dec.) | Distancia desde J (in) |
|---|---|---|---|---|---|
| `P1` | `ARM.1.PositiveY` | −1000 | 57.50128206169155 | 57.501 | 24.35421884050472 |
| `P2` | `ARM.2.PositiveY` | −2000 | 36.25192309253731 | 36.252 | 15.354218840504721 |
| `P3` | `ARM.3.PositiveY` | −1102.311310924388 (con −1102.31 ya cumple 0.1 %) | 78.75064103084577 | 78.751 | 33.35421884050472 |

Origen: archivo de cargas del fixture de F1 (`tests/RackCad.Tests/StructuralAnalysis/Fixtures/ref-cant-01.loads.json`, SHA-256
`25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05`; OD-F4-06): 1000 lbf a 0.5, 2 kip a 0.25 y 500 kgf a 0.75 del brazo
físico de 36 in, más la distancia rígida de IDL-06 (6.354218840504721 in). Tolerancias: valor 0.1 % (relativo al valor manual), posición
0.02 in, dirección exacta.

## 9. Combinaciones (disposición del Owner §4, A65-F4R-06)

- El modelo manual **no** tiene combinaciones. El operador lo comprueba en la interfaz y lo captura (informativo).
- En la primera corrida, toda combinación que no pueda leerse mediante una **interfaz verificada** se declara **NO EVALUADA**: la lectura
  de combinaciones por la API no está medida (M-05). Por eso la sesión invoca el comparador con `--generated-combinations-unread`,
  `--manual-combinations-unread` y `--combinations-decision "<texto del ingeniero>"`; el resultado global es **INCOMPLETE** (código de
  salida 5), **nunca PASS**, aunque ninguna fila falle.
- No se implementa ninguna lectura nueva de combinaciones sin un contrato autorizado.

## 10. Supuestos y exclusiones (V1; solo para la equivalencia de transferencia)

| ID | Supuesto | Limitación declarada |
|---|---|---|
| SUP-01 | pie de columna empotrado (6 GDL) | sin rigidez de anclaje, placa ni losa |
| SUP-02 / IDL-07 | base no modelada | la base real apoya en el piso |
| SUP-03 | análisis plano: `dx ry rz` restringidos en los demás nudos | sin pandeo fuera del plano ni arriostramientos |
| CON-01 | brazo–columna continuo | la conexión real es semirrígida |
| IDL-01 | eje analítico = eje centroidal | — |
| IDL-06 / IDL-NO-OFFSETS | sin offsets rígidos: el brazo analítico va de J a la punta e incluye media columna y la placa | sobreestima la flexibilidad del brazo |
| IDL-08 | fusión 0.001 in; separación mínima 0.05 in | — |
| ASM-PROP-01 | propiedades de sección y material = base de RAM | se verifican con la hoja firmada |
| LOD-01 | un caso `REF1` con `P1`-`P3`; sin peso propio ni combinaciones | no deriva de tarimas, impacto, sismo ni tributarias |

**Exclusiones:** no se modelan base, placas, cartelas, separadores, arriostramientos, barras redondas, brazos `NegativeY`, doble cara,
canales pareados ni brazos con más de 10°; no se usan offsets, resortes, liberaciones, peso propio, combinaciones ni cargas nodales,
distribuidas o locales; no se analiza, no se leen ni importan resultados, no se diseña y no se guarda ni se sobrescribe el modelo.

Advertencia del manifiesto, literal: «These assumptions are hypotheses for transfer equivalence only; they are not a structural
validation.»

## 11. Comprobaciones antes del volcado

| # | Comprobación | Esperado | ☐ |
|---|---|---|---|
| CK-01 | Conteo de nudos / miembros | 8 / 7 | ☐ |
| CK-02 | Casos | `CM` sin cargas y `REF1` con 3 cargas | ☐ |
| CK-03 | Combinaciones | 0 (captura) | ☐ |
| CK-04 | Columna vertical, brazos hacia −Z de RAM, cargas hacia abajo | visual | ☐ |
| CK-05 | Eje 2 de las columnas hacia −Z de RAM | visual | ☐ |
| CK-06 | Modelo **sin analizar** | sí | ☐ |

## 12. Volcado y medición M-05

1. La sesión genera el script de volcado con la herramienta de F4 en una **carpeta nueva** (nunca reutilizada; ruta corta):
   `dump-script --out <carpeta nueva> --adapter-version 1.0.0`. El script es `rackcad-ram-dump.py` y escribe `ram-dump.json` en esa
   carpeta, en modo exclusivo.
2. En RAM, **antes de analizar**: Python Console → «Ejecutar archivo…» → `rackcad-ram-dump.py` de esa carpeta.
3. Si aparece un diálogo de «los resultados se perderán», responder **No** y STOP.
4. M-05: la sesión registra cómo lee la API lo creado a mano (categoría del caso, modo de posición de la carga, giro leído de las
   columnas, banderas y combinaciones), a partir del volcado.

## 13. Cierre y registro

- Cerrar RAM **sin guardar**, salvo que el ingeniero decida conservar el modelo (D-11; si lo conserva, anotar dónde para OD-F4-11).
- Registro del operador: nombre, fecha, ☐ de §3-§11, observaciones y firma. Custodia de la sesión: capturas, `ram-dump.json`,
  SHA-256 de cada archivo y de `RAMElements.exe`, estado de la licencia.

## 14. Paradas

Licencia no disponible; un diálogo de resultados (respuesta **No**); un nombre de sección o material que no existe o existe en más de una
base sin decisión del ingeniero; la interfaz no permite θ = −90° (ni 270°); unidades distintas de lb-in; un `FAILED` del volcado; la
condición de Q-O1 (transferencia no verificable sin hipótesis incorrectas). Toda parada → STOP, registro y decisión del Owner.

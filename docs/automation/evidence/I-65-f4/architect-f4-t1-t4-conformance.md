# I-65 F4 (T1-T4) — revisión de conformidad del Architect (custodia)

- Rol: Architect de I-65, `SAME-SESSION ROLE` (subagente de contexto independiente del Coordinator), solo lectura.
- Fecha: 2026-10-11. Alcance: `git diff 5bb1b9ba..58b70de4 -- src tools tests RackCad.sln` (T1 `53bfcd94`/`a6fa5a35`/`f5c6b885`, T2 `2e92a0df`/`0070ae40`,
  T3 `a283e0cc`/`76db7642`, T4 `b51adf28`/`0e9bbc29`/`c4660703`/`7efacd78`) sobre `HEAD` `58b70de4`, contra la Proposal V3.1 (Frozen), ADR-0050, la
  concreción `I-65-f4-implementation.md`, el paquete de validación de ingeniería §3 F, las órdenes literales del Owner de F4 y de F4-T3 y las
  decisiones §26-§35; lectura pasiva de la fuente instalada del paquete del programa de análisis y de `p1-report.json`. Sin compilar ni ejecutar
  nada; sin intérprete.
- Texto: resumen fiel del informe del subagente. La revisión de T5 se hará sobre su diff cuando exista.

## Veredicto

**CONFORMANT WITH OBSERVATIONS.** Ningún hallazgo REQUIRED; 11 observaciones OPTIONAL. Cuatro (A65-F4-01, 03, 04 y 05) conviene resolverlas o
dejarlas decididas antes de pedir la primera ejecución real, y A65-F4-06 propone un ensayo general antes de esa petición.

## Respuestas (resumen fiel)

1. **Freeze, ADR-0050 y fronteras:** conforme. Domain, Plugin y UI intactos; G1 sin tokens del proveedor fuera de `Providers/RamElements` y lista
   de permiso vacía; G2, G4, INV-11 e INV-16 sin violaciones; INV-14 con exactamente cuatro rutas exentas y pruebas nuevas que leen el nombre del
   módulo de la plantilla; E/S neutral solo en el escritor de paquetes; la herramienta en `tools/`, solo referencia Application y el Plugin no la
   referencia.
2. **T1:** conforme. A65-F3-11 (tercera puerta `payload_failures` antes del `open` y de `units.set`), prefijo `gate: `, A65-F3-01 (`angle_near`
   módulo 360), A65-F3-02 (descripción no comparada), A65-F3-04 (`persist_readback` con respaldo; matiz A65-F4-07) y regla de carpeta.
3. **T2:** conforme. Entrada de una sola lectura con huella sobre la copia de los bytes, excepciones como `Unavailable`, rechazos del constructor,
   `SourceLineBlocked`, familias W, HSS-RECT y S, cota sen 0.01°, inventario por rol (matiz A65-F4-08); guardas de Cantilever y golden de INV-06
   intactos.
4. **T3:** conforme. Script de solo lectura (dos `units.set`, el primero condicionado y el segundo en `finally`; un único `open` exclusivo); puerta
   de 31 funciones, todas en las 207 de P1; las llamadas no ejecutadas en P1 existen en la fuente instalada con la firma usada; inversa de D-04
   correcta; posición de carga absoluta desde el nudo I; lector estricto; ida y vuelta de REF-CANT-01 a 1e-9.
5. **T4:** conforme con observaciones. Filas de §3 F con tolerancias 0.02 in, 0.1° y 0.1 % sin alterar y bordes probados; `OTHER` exigida en el
   generado; categoría del manual informativa; propiedades `NOT_EVALUATED`; rechazo y `STALE`; informe determinista. Supuestos del Worker: (a)
   emparejamiento por diferencia máxima por coordenada, defendible pero no ratificado (A65-F4-01); (b) opción de combinaciones no leídas, con base
   en §3 F pero sin disparador técnico (A65-F4-02). La herramienta nunca sobrescribe y la inyección aplica una sola mutación con el manifiesto
   marcado. El GREEN de la corrección es idéntico byte a byte a `0e9bbc29`.
6. **Restricciones del Owner:** conforme. Nada ejecuta el programa ni el intérprete; las filas de material salen `NOT_EVALUATED`; no hay lógica
   sobre el peso unitario ni sobre el −0.62 %; las tolerancias no cambian.
7. **Primera comparación real:** signos, unidades, orden e identidades, porcentaje de carga y caso `CM` ignorado, recalculados y correctos.
   Riesgos: rechazo en falso por categoría vacía en un caso ignorado (A65-F4-05); comportamientos del programa no medidos que fallan en cerrado;
   PASS en falso posibles por volcado del generado no ligado a la construcción (A65-F4-03), por la opción de combinaciones (A65-F4-02), por lo que
   la API no lee (A65-F4-11) y, marginalmente, por la métrica de nudos (A65-F4-01).
8. **Pruebas y RED/GREEN:** las pruebas observan de verdad (guardas probadas con variantes mutadas, pruebas de comportamiento, ida y vuelta);
   puntos débiles: `angle_near` solo comprobado sobre el texto (inherente a INV-14), bordes de nudos sobre una sola coordenada,
   `SourceEntryTests.cs:223-232` acepta `Unavailable` o `Invalid`, lista de funciones espejo del código (compensada por la contención en P1).
   RED/GREEN respetado en las cuatro tareas.

## Hallazgos (todos OPTIONAL)

| ID | Ubicación | Hallazgo | Recomendación |
|---|---|---|---|
| A65-F4-01 | `StructuralReadbackComparison.cs:449-450,514-517,935-939`; `ComparisonVerdictTests.cs:43-55` | Emparejamiento por diferencia máxima por coordenada (admite hasta 0.028 in euclídeos en el plano) y valor de carga relativo al mayor de los dos, sin decisión que los ratifique; el Freeze §4 no fija la métrica | Decisión registrada antes de la primera comparación real; preferible euclídea ≤ 0.02 in con prueba diagonal (0.015, 0.015) |
| A65-F4-02 | `StructuralReadbackComparison.cs:62-73,941-956`; `RamToolCompare.cs:17,119-121` | Opción del operador que convierte la fila de combinaciones en `NOT_EVALUATED` y puede dar PASS global, sin disparador técnico | Usarla solo por decisión registrada con evidencia de M-05, o retirarla; registrar su uso en la evidencia de la OV |
| A65-F4-03 | `StructuralReadbackComparison.cs:321-367`; `RamToolCompare.cs:96-122` | El volcado del generado no lleva huella ni se contrasta con `values` de la lectura de vuelta (D-12: «solo si su huella coincide»); entradas iguales podrían dar PASS | Contrastar el volcado del generado con la lectura de vuelta, rechazar entradas iguales e incluir huella y SHA-256 de las entradas en el informe |
| A65-F4-04 | `RamTransferReaders.cs:241`; `RamToolCompare.cs:80-86` | `compare` sobre un paquete de inyección termina con código 3 en el manifiesto sin mirar la lectura de vuelta: el paso «el comparador lo rechaza» de OV-I65-05 no discrimina | Reconocer la marca en `tools/` y evaluar la lectura de vuelta (REJECTED), con prueba; o disposición escrita antes de la campaña de inyecciones |
| A65-F4-05 | `RamDumpReader.cs:384` | Categoría no vacía exigida en todo caso, incluidos los que D-11 ignora; una categoría vacía rechazaría ambos volcados | Admitir categoría vacía en casos sin cargas, o medirla en E4 antes de la corrida |
| A65-F4-06 | — | Falta un ensayo general con volcado manual hecho con los valores del paquete §2 (4 decimales, −90° o 270°, porcentajes de 3 decimales, otros ids y orden, `CM` presente) | PASS esperado; FAIL con +90°, con porcentaje de 1 decimal y con un miembro invertido; antes de pedir la autorización |
| A65-F4-07 | `RamBuildScriptTemplate.cs:300-322`; `RamDumpScriptTemplate.cs:199-221` | Si la escritura falla a medias, el respaldo queda tras el texto parcial (falla en cerrado, pero se pierde el respaldo) | Truncar antes del respaldo o documentarlo como limitación |
| A65-F4-08 | `CantileverStructuralModelSource.cs:114-115,155` | Inventario por rol: una segunda pieza Column o Arm no modelada pasaría en silencio | Exigir 1 Column y tantas Arm como brazos modelados |
| A65-F4-09 | `RamToolInject.cs:74-80`; `RamToolDumpScript.cs:37` | `--embed-folder` puede apuntar a la carpeta del paquete de origen | Rechazar carpeta embebida igual al origen o existente, o limitar la opción a pruebas |
| A65-F4-10 | `StructuralAnalysisSourceGuardTests.cs:503-511` | INV-14 no barre `tools/` aunque la herramienta se ejecuta en la suite Core (hoy limpia) | Ampliar el barrido (más estricto que el Freeze: requiere decisión) |
| A65-F4-11 | `StructuralReadbackComparison.cs:1014-1031` | La API no lee peso propio manual, cargas distribuidas o nodales, resortes de nudo ni casos vacíos sobrantes | Comprobaciones explícitas de la lista del procedimiento E/F, registradas en la evidencia |

## No comprobado sin ejecutar

El estado real de pruebas y CI (se basa en las corridas registradas); el comportamiento real de `angle_near` y `persist_*`; comportamientos del
programa (lectura de −90° como −90 o 270, categoría de `CM` y de casos de la interfaz, tipo de id de `members_get_nodes`, fichas de dirección y de
tipo de cargas de la interfaz, descripciones nulas, `lc_is_combo`, diálogo de `units.set`, `open(..., "x", newline="\n")` en el intérprete
embebido); la carrera del destino en disco real; el determinismo entre culturas; la exclusión de la herramienta en el paquete de instalación.

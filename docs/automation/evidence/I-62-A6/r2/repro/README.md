# I-62 A-6 ronda 2 — reproducción de la evidencia de R1-R4 sobre datos reales (decisiones §79, punto 5; §80, punto 2)

Reejecución desde cero, en raíces de trabajo nuevas, de los dos paquetes de evidencia de la ronda 2:

- `r1r4/`: identidad del objeto en el punto de uso y lugar del aceptante;
- `r3/`: independencia del aceptante.

Sus resultados se comparan campo a campo con los custodiados. No se invocó ningún modelo, no hubo red y no se leyó `auth.json`, credenciales ni
`config.toml`. El instrumento (`9a829b0e…`) y el aceptante (`434ef1fc…`) se tomaron por blob y no cambiaron.

## 1. Autoría, ejecución y revisión independiente

| Paquete | Autor (construyó el banco y la evidencia) | Ejecución custodiada | Reproducción (esta carpeta) | Revisión independiente |
|---|---|---|---|---|
| R1/R4 (`ev/r1r4` → `r2/r1r4/`) | agente A, distinto del autor del texto de A-6 | agente A | agente C, que no construyó ningún paquete: scripts copiados sin cambios salvo la raíz de trabajo (`lib_r1r4.diff`); comparación con su propia herramienta `tools/compare.py` | ninguna todavía: la revisión formal es la revisión independiente, pendiente de disposición |
| R3 (`ev/r3` → `r2/independence/`) | agente B, distinto del autor del texto y del agente A | agente B | intento 1: agente C. Ejecución final: la sesión principal, que es la autora del texto de A-6 pero no del paquete R3. Scripts sin cambios (`R3_WORK` y `R3_OUT` por entorno) y la misma herramienta de comparación del agente C | ninguna todavía |

## 2. R1/R4: REPRODUCED

- Agente C, el 2026-10-12: ejecución de 09:46:21Z a 10:00:33Z con `run_r1.py`, `run_r1_extra.py`, `run_r4.py` y `summarize.py`; comparación a las 10:06Z.
- Los 12 archivos de resultados (`r1-*.json`, `r4-*.json` e `inputs-t1/*`) coinciden con los custodiados salvo diferencias de clases esperadas:
  rutas de la raíz de trabajo, marcas de tiempo, PID y digests que la herramienta verificó por recálculo.
- Detalle en `_work/cmp-r1r4.json`.

## 3. R3: REPRODUCED en los resultados

**Ejecuciones, errores e interrupciones** (horas UTC del 2026-10-12; eventos de energía de Windows en `_work/finalize-repro.json`):

| Intento | Quién | Raíz | Qué pasó |
|---|---|---|---|
| 1 | agente C | `D:/r62-a6r2/repro/r3`, conservada como `r3-interrupted-standby` | `build_repos` correcto (10:07-10:09). `part_a` empezó a las 10:09 y salió con código 1 a las 13:16:46: una llamada del aceptante agotó su tope de 3 600 s porque el equipo estuvo en suspensión de 10:34:57 a 13:16:27 (eventos 506/507). Registros en `_work/r3-interrupted/` |
| 2 | agente C | `D:/r62-a6r2/repro/r3` | `build_repos` empezó a las 14:20:58 y el equipo entró en suspensión a las 14:23:54. El agente dejó de progresar y su supervisor lo detuvo |
| 3 | sesión principal | `D:/r62-a6r2/repro/r3`, nueva | `build_repos` correcto (17:31:12-17:34:27). `part_a` cortado por el apagado del equipo: última escritura a las 18:00:16, evento 109 a las 18:00:32 |
| reanudación única (§80, punto 2) | sesión principal | la misma del intento 3, con los resultados guardados | tope explícito de 150 minutos; equipo mantenido despierto por la aplicación. `part_a` 19:42:13-19:59:23, `part_b` hasta 20:04:27, `part_c` hasta 20:10:55, `part_d` hasta 20:12:20, `review_e` hasta 20:12:44, `finalize` hasta 20:12:45; **todos con código 0**, en 30 min 32 s |

Ninguna interrupción vino del instrumento ni del aceptante.

**Comparación** (`_work/cmp-r3.json`, con `tools/compare.py` y `_work/pairs-r3.json`): veredicto **REPRODUCED**.

| Resultado | Estado |
|---|---|
| `build.json` | solo rutas |
| `a-mutation.json` | rutas, duración y cadenas con digests verificados |
| `b-clausemap.json` | solo duración |
| `c-r4.json` | solo duración |
| `d-closure.json` | rutas y duración; los blobs de dos entradas generadas (`deleg-deliv.json` y `handoff-deliv.json`) difieren solo por la raíz de trabajo escrita dentro. La sesión principal lo verificó: con la raíz de la reproducción escrita como la original, los dos archivos son idénticos byte a byte y dan los blobs custodiados (`pairs-r3.json`). La herramienta no localizaba esos archivos por sí sola, por eso se le dieron esos dos pares verificados |
| `e-review.json` | idéntico |
| `manifest.json` | digests derivados de los archivos anteriores |

**Intermedios de la raíz de trabajo** (no son resultados): 68 diferencias que la herramienta no explicó por sí sola, todas analizadas.

- 35 son el `PinnedBlob` de las solicitudes del instrumento de eco. Verificadas una por una: idénticas tras escribir la raíz como la original.
- 33 son el motivo de R4 `NOT_APPLICABLE` de las salidas del aceptante de la parte (a). En la raíz custodiada dice «facts-level scenario: …» y en la
  reproducción «facts-level request: …». La raíz custodiada contiene tres registros de `part_a` (`part_a.log`, `part_a2.log`, `part_a3.log`), así
  que esos intermedios en caché se produjeron con una redacción anterior del script. No cambia ningún resultado: `a-mutation.json` se reproduce.
- 13 archivos están solo en la raíz custodiada: copias adicionales de documentos del paquete en `pin/` y `runs/tmp/K.json`. Los scripts actuales no
  los producen.

## 4. Lo que exige decisiones §80, punto 2

- **Mutantes activables: 25 de 27**, todos rechazados con el mutante como instrumento declarado:
  - activados: M01-M14, M16-M22 y M24-M27;
  - detección atribuible en 24; en M11 solo la señal de R2, que daría también el error del instrumento fijado;
  - M14 y M17, solo por R4.

  El resumen de la reproducción es igual campo a campo al custodiado: `Activated`, `NotActivated`, `DetectorsConfigM`, `DetectedOnlyByR4`,
  `ActivatedNotDetectedConfigM`, `AcceptedAtFactsLevel` y `ActivationsWithoutAttributableDetection`.
- **No activables: M15** (lista cerrada de subórdenes de Git) **y M23** (`--porcelain` forzado): ninguna entrada de la línea de órdenes llega a su
  código. Su clase la muestran una sonda directa y la inyección en un sobre, que R9 rechaza.
- **R5, R6 y R7 sobre los hechos críticos**, en la configuración con el mutante como instrumento:

  | Mutante | Hecho crítico | Detector R5/R6/R7 |
  |---|---|---|
  | M01 | cobertura de `Scope` | R7 |
  | M02 | MV-7 | R6 |
  | M03 | ascendencia e `Identity` | R5 |
  | M04 | pertenencia de `Scope` | R5, R7 |
  | M05 | MV-4 | R6, R7 |
  | M06 | MV-7 | R6 |
  | M07 | MV-4 | R6, R7 |
  | M08 | punto efectivo | R6 |
  | M09 | punto efectivo | R6 |
  | M12 | cobertura (U-19) | R7 |
  | M13 | blobs y hashes | R5 |
  | M16 | punto efectivo | R6 |
  | M18 | MV-3 | R6 |
  | M20 | MV-7 | R6 |
  | M21 | punto efectivo | R6, R7 |
  | M22 | MV-6 | R6 |
  | M24 | MV-6 | R6 |
- **Diferencias frente a la ejecución custodiada:** las de §3; ninguna en un veredicto, un estado, un recuento ni una identidad.
- **Paquetes originales intactos:** los 35 archivos que el agente C registró antes de empezar (`_work/package-hashes-before.txt`) tienen hoy el
  mismo SHA-256.
- **Fuentes de solo lectura:** todas sin cambio (`_work/guard-before.json` frente a `guard-after.json`) salvo la HEAD del worktree de I-62, que
  avanzó con dos commits documentales de la sesión principal durante la reproducción (`6a681cec` y `2c531b53`). El origen del fixture, el checkout
  principal y los blobs del instrumento y del aceptante, sin cambio.

## 5. Archivos

`r1r4/` y `r3/`: copias de los scripts y resultados de la reproducción. `tools/`: herramienta de comparación y recolectores del agente C.
`_work/`: comparaciones (`cmp-r1r4.json`, `cmp-r3.json`, `pairs-r3.json`), registros de tiempos y de cada paso, registros del intento 1
interrumpido, protecciones antes y después, hashes de los paquetes y `finalize-repro.json`.

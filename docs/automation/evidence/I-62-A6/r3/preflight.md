# I-62 A-6, ronda 3 — preflight (decisiones §83)

> Preparación, no acuerdo. Ninguna revisión formal de la ronda 3 se lanza sin una disposición posterior del Coordinator (decisiones §83, punto 6).
> Sin Codex, sin sesión de Principal nueva, sin FX-U2, sin B1, sin A-6 AGREED, sin FX-02 PASS ni F6 GATE PASS.

## 1. Objeto

- Candidata de la ronda 3: `docs/initiatives/I-62-A-6.md`, blob `e62f27b8b1329f1b9cfa69a707feb31d22bad1d1`, derivada de la ronda 2 (blob
  `86da0ae770e683a647f2724860a4bd890061154d`, commit `ae9fe0e9`) con `text/make_r3_text.py` (67 sustituciones exactas sobre el blob de la ronda
  2) y `text/make_r3_fills.py` (valores medidos). Las rondas 1 y 2 se conservan en Git sin cambio.
- Cambios normativos solo en A6-3, regla 6, A6-5 y A6-7 (decisiones §83, punto 2). A6-1, A6-2, A6-4, A6-6, A6-8, A6-9 y las reglas 1-5 y 7 de A6-3,
  byte a byte como en la ronda 2 (G5). Las demás secciones que cambian son descriptivas (A-6 §3.17).
- Piezas conservadas: instrumento `9a829b0eeb1210238052b1feac06cfa8f7964aed` y aceptante `434ef1fccb0d15762b30244b4a60928120698411`. Presupuestos,
  receta 16.4 y alcance de la línea (a) del Owner sin cambio; las dos líneas del Owner de §7, byte a byte (G6).

## 2. Hallazgos de la ronda 2

| Hallazgo | Estado | Dónde | Prueba |
|---|---|---|---|
| A62-A6R2-R1 | corregido: ámbito exhaustivo de A6-5 (regla 7: con ejecuciones, sin ejecuciones, registro insuficiente, dónde no corresponde ejecutar); R4 solo con `Verification` en la solicitud, y siempre en una VERIFIED (regla 2; A6-3, regla 6 (c)); `not_run` tras el primer fallo, juzgado por 16.9 | A6-5, A6-3 regla 6 | `r1probes/` (57 de 57) |
| A62-A6R2-R2 | corregido como precondiciones verificables con fallo cerrado: reconstrucción de `Runs` desde el registro real (A6-3, regla 6 (a)); la medición la demuestra con el aceptante sobre su registro real (A6-7, regla 1); disposición previa sobre el cierre de VERIFY (A6-7, regla 7); prueba previa con ese cierre (A6-5, regla 6). Lo estático, comprobado; lo real, pendiente de la medición | A6-3 regla 6, A6-7, A6-5 regla 6 | `r2static/`, `r1probes/` |
| A62-A6R2-O1 | no incorporada (A6-3, regla 1, fuera de §83.2) | — | — |
| A62-A6R2-O2 | incorporada en el registro de G4 y en §3.16 | §3.12, §3.16 | `r2static/` (28 de 28) |
| A62-A6R2-O3 | en parte (A6-7, regla 1); el resto, residuo declarado en G4 | A6-7, §3.12 | pendiente de la medición |
| A62-A6R2-O4 | no incorporada en A6-4 (fuera de §83.2); precisión en G4 | §3.12 | — |
| A62-A6R2-O5 | incorporada: S-04 al Coordinator antes de reejecutar | A6-5, regla 4 | — |

## 3. Guardas

`a6r3-guards.py` (blob `103d69c62025b852a6b2233b1a55d78ba30f083d`), derivado de `r2/a6r2-guards.py` (blob `dea3db71…`) con `adapt_guards_r3.py` (sustituciones exactas;
`a6r3-guards.diff`). La sesión principal adaptó las guardas y los mutantes; los bancos de prueba (`r1probes/`, `r2static/`, `launchfix/`) son de tres
agentes distintos, que no redactaron el texto.

| Prueba | Resultado |
|---|---|
| Autoprueba (`a6r3-selftest.json`) sobre el blob de la candidata | **PASS**: 694/694 mutantes muertos, 0 sin aplicar, controles en pass; `A6TextBlob` = el blob de la candidata |
| Controles negativos de extremo a extremo (`neg_controls_a6r3.json`) | **32/32** como se esperaba; worktree sin cambio; temporales borrados |
| `preview --final` sobre la base `9cd074aa` con el paquete r3/ (`a6r3-guards-result-preview.json`) | **PASS** (G2 NOT_RUN sin commit; las demás guardas en pass) |
| `run` sobre el commit que publique la ronda 3 | en el commit siguiente (`a6r3-guards-result-run.json`) |

`neg-fixtures/` contiene el borrador con los marcadores sin rellenar y su autoprueba, que el control negativo D-1 usa como entrada que debe fallar.
No es texto de la candidata.

## 4. Evidencia de la ronda 3

- **`r1probes/`** (agente A): 57 de 57 casos conformes con el aceptante y el instrumento fijados, sin Codex: VERIFY sin ejecuciones (rechazo
  siempre: R3 y R9), N4 (la construcción de la ronda 2 rechaza; la de la ronda 3 acepta), nc1 (V1-V3), la regresión de T1 (igual a la ronda 2),
  mutantes de la construcción (cuatro solo los detecta la tabla) y la reconstrucción de referencia de `Runs` desde registros en el formato de
  Codex, con fallo cerrado. Las salidas del Controller y los registros son sintéticos; el formato sale de los registros custodiados.
- **`r2static/`** (agente B): formato de los nueve registros reales de Codex custodiados (111 órdenes): `command` es una sola cadena, no siempre la
  línea ejecutada; `aggregated_output` mezcla salida estándar y de error sin separador, con fines de línea mixtos, U+FFFD en ocho órdenes y sin
  truncado visible hasta 59 805 bytes. `r1probes/README.md` cita como mayor salida 11 228 caracteres porque mide un solo registro; el máximo de
  los nueve es el de `r2static/`. Comprobaciones (v)-(viii) del punto de uso: 28 de 28 casos conformes, con el pwsh 7 del `PATH`.
- **`launchfix/`** (agente C): lanzador del kit siguiente derivado del de la ronda 2 por sustituciones exactas, con la orden congelada comprobada
  por el propio lanzador, el registro escrito siempre que se lanzó el proceso y una comprobación previa de registro con un proceso sustituto;
  28 de 28 pruebas, incluida la reproducción del fallo de la ronda 2. Dos decisiones del agente, a confirmar al preparar el kit: la
  comprobación previa se ejecuta también en el lanzamiento real, y el código de salida exige además que no haya errores posteriores.

## 5. Fuentes para el revisor siguiente

`a6r3-sources-for-reviewer.json` (salida de G11) lista los archivos, blobs y rangos de líneas que el texto cita, entre ellos V14 L2333-L2334.

## 6. Controles repetidos antes de publicar

- **Identidad:** blob de la candidata = `A6TextBlob` de la autoprueba y de la vista previa (`e62f27b8b1329f1b9cfa69a707feb31d22bad1d1`); piezas sin cambio (G7); líneas
  del Owner byte a byte (G6).
- **Alcance:** G9 (solo VERIFY) y, sobre el commit exacto que publique la ronda 3, G2 y G3 en modo `run`.
- **Privacidad:** G1, y el ensamblado rechaza cualquier archivo con el nombre de usuario o con un correo que no termine en `.invalid`.
- **Negativos:** los controles de extremo a extremo de §3.

## 7. Autoridad del Owner

Ninguna línea nueva del Owner es necesaria para la ronda 3: los blobs fijados por `A6-ADR0046-9-FX02 = A` no cambian y el alcance VERIFY es el
literal de la línea. Las exigencias nuevas de A6-7 son de la supervisión y del Coordinator dentro de la medición que autorizaría la línea (b).

## 8. Pendiente de una sesión real (medición de A6-7)

- La forma exacta con que Codex registra la orden declarada y la regla de reconstrucción que la disposición nombrará.
- Que el sobre llegue sin alterar ningún byte (LF) por el pwsh del runtime de Codex, separable de la salida de error, y sin truncado con los
  tamaños reales.
- La corrida del aceptante sobre las `Runs` reconstruidas del registro real, en el mismo `-C`, con `ACCEPT_FACTS` en el nivel de los hechos.
- Que `git` por nombre se resuelva al Git declarado en el entorno de la invocación; la entrega de los bytes por el shell real (paso (iii)).
- El lanzamiento sin variables `GIT_*` en el entorno de la invocación (paso (vi)).
- Antes de la medición: la disposición del Coordinator sobre la cobertura de las lecturas legítimas por el cierre de VERIFY y la regla de
  reconstrucción (A6-7, regla 7).

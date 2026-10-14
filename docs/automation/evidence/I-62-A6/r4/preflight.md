# I-62 A-6, ronda 4 — preflight (decisiones §85)

> Preparación, no acuerdo. Ninguna revisión formal de la ronda 4 se lanza sin una disposición posterior del Coordinator (decisiones §85, punto 6).
> Sin Codex, sin cambios en el instrumento ni en el aceptante, sin B1, sin FX-U2, sin cambios en producción, I-65 ni D.5, sin A-6 AGREED, sin
> FX-02 PASS ni F6 GATE PASS.

## 1. Objeto

- Candidata de la ronda 4: `docs/initiatives/I-62-A-6.md`, blob `4bc919dd4768ab81abc74b70d3a4a3821c07feb5`, derivada de la ronda 3 (blob
  `e62f27b8b1329f1b9cfa69a707feb31d22bad1d1`, commit `a128efef`) con `text/make_r4_text.py` (39 sustituciones exactas sobre el blob de la
  ronda 3) y `text/make_r4_fills.py` (valores medidos por `r1probes/`). Las rondas 1 a 3 se conservan en Git sin cambio.
- Cambio normativo solo en A6-3, regla 6 (b) (decisiones §85, punto 2): la tabla de derivación de A6-9 aplicada cuando falta una entrada o un
  commit que nombra (entrada ausente, campo ausente, `null` o vacío y valor que no nombra un commit existente, omitidos con su causa y sin
  sustituirlos; primitivas con un extremo omitido o sin resultado, vacías; registro custodiado con la solicitud y el cierre; la misma regla en la
  prueba previa y en la medición). A6-9, regla 4, y su tabla no cambian; los demás deltas, las reglas 1-5 y 7 de A6-3 y los apartados (a), (c) y
  (d) de su regla 6, byte a byte como en la ronda 3 (G5). Las demás secciones que cambian son descriptivas (A-6 §3.18).
- Piezas conservadas: instrumento `9a829b0eeb1210238052b1feac06cfa8f7964aed` y aceptante `434ef1fccb0d15762b30244b4a60928120698411`. Presupuestos,
  receta 16.4 y alcance de la línea (a) del Owner sin cambio; las dos líneas del Owner de §7, byte a byte (G6).

## 2. Hallazgos de la ronda 3

| Hallazgo | Estado | Dónde | Prueba |
|---|---|---|---|
| A62-A6R3-R1 | corregido: el cierre de N4 y nc1 se deriva mecánicamente de la tabla de A6-9 aplicada con A6-3, regla 6 (b) (i)-(v) | A6-3, regla 6 (b) | `r1probes/` (56 de 56 casos conformes) |
| A62-A6R3-O1 | no bloqueante; sin cambio (decisiones §85, punto 4) | — | — |
| A62-A6R3-O2 | cotejo mecánico de la solicitud con su construcción, incluido el cierre y su registro (evidencia) | `r1probes/table_r4.py` | `r1probes/` |
| A62-A6R3-O3 | verificado: `clausemap` dentro del cierre derivado en la verificación, N4 y nc1; fuera con la variante I6 estricta (pendiente de A6-7, regla 7) | evidencia | O3-STRICT |
| A62-A6R3-O4 | momento y efecto de la prueba previa precisados en la evidencia (A-6 §3.18) | evidencia | — |
| A62-A6R3-O5, O6 | no bloqueantes (decisiones §85, punto 4) | — | — |
| A62-A6R3-O7 | custodia de la medición asegurada (A6-3, regla 6; regla 6 (b); anexo, paso 4) | A6-3, regla 6 (b); anexo | — |

## 3. Guardas

`a6r4-guards.py` (blob `56b59c5004035ef9ed3bcd30f1ffff33620e3be3`), derivado de `r3/a6r3-guards.py` (blob `103d69c6…`) con `adapt_guards_r4.py` (sustituciones
exactas; `a6r4-guards.diff`). Las guardas, sus mutantes y los controles negativos los adaptó un agente distinto del que escribió el banco
`r1probes/derive_r4.py`; la implementación independiente del cierre (`r1probes/indep/`) es de un tercer agente; ninguno redactó el texto.

| Prueba | Resultado |
|---|---|
| Autoprueba (`a6r4-selftest.json`) sobre el blob de la candidata | **PASS**: 815/815 mutantes muertos, 0 sin aplicar, controles en pass; `A6TextBlob` = el blob de la candidata |
| Controles negativos de extremo a extremo (`neg_controls_a6r4.json`) | **52/52** como se esperaba (incluidos candidatos que editan A6-9, regla 4, o su tabla, A6-5 o A6-7, que dejan un marcador o que tocan la revisión de la ronda 3); worktree sin cambio; temporales borrados |
| `preview --final` sobre la base `ceb85977` con el paquete r4/ (`a6r4-guards-result-preview.json`) | **PASS** (G2 NOT_RUN sin commit; las demás guardas en pass) |
| `run` sobre el commit que publique la ronda 4 | en el commit siguiente (`a6r4-guards-result-run.json`) |

`neg-fixtures/` contiene el borrador con los marcadores sin rellenar y su autoprueba, que un control negativo usa como entrada que debe fallar. No es
texto de la candidata.

## 4. Evidencia de la ronda 4

- **`r1probes/`**: 56 de 56 casos conformes, sin Codex, con el aceptante y el instrumento fijados por blob: derivación del cierre con la regla corregida (con
  todas las entradas presentes, idéntica a la de la ronda 2), N4 (entrega ausente en todas sus formas), nc1 y los campos ausentes, `null` o vacíos,
  la generalidad a la delegación y al contrato, sustituciones, insumos añadidos o inventados, la verificación real, O3 con la variante I6 estricta,
  y la comparación con una implementación independiente (`r1probes/indep/`, escrita por otro agente solo desde el texto de la regla y de la tabla):
  mismo cierre y mismas omisiones en 58 de 58 vectores.
- **Contradicciones que exijan modificar A6-9:** ninguna (A-6 §3.18, «Relación con A6-9, regla 4»).

## 5. Fuentes para el revisor siguiente

`a6r4-sources-for-reviewer.json` (salida de G11) lista los archivos, blobs y rangos de líneas que el texto cita, entre ellos las fuentes nuevas de la
ronda 4: `docs/automation/agent-execution/README.md` L219-L220, `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md` L23 y la
salida de la revisión de la ronda 3 (`691bbc8f…`).

## 6. Controles repetidos antes de publicar

- **Identidad:** blob de la candidata = `A6TextBlob` de la autoprueba y de la vista previa (`4bc919dd4768ab81abc74b70d3a4a3821c07feb5`); piezas sin cambio (G7); líneas del Owner
  byte a byte (G6).
- **Alcance:** G5 (solo A6-3, regla 6 (b), cambia en los deltas) y G9 (solo VERIFY); sobre el commit exacto que publique la ronda 4, G2 y G3 en
  modo `run`.
- **Privacidad:** G1, y el ensamblado del paquete rechaza cualquier archivo con el nombre de usuario, la ruta del perfil o un correo que no termine
  en `.invalid`.
- **Negativos:** los controles de extremo a extremo de §3.

## 7. Autoridad del Owner

Ninguna línea nueva del Owner es necesaria para la ronda 4: los blobs fijados por `A6-ADR0046-9-FX02 = A` no cambian y el alcance VERIFY es el
literal de la línea. La corrección solo fija cómo la supervisión deriva el cierre del aceptante.

## 8. Pendiente de una sesión real (medición de A6-7), sin cambio desde la ronda 3

- La forma exacta con que Codex registra la orden declarada y la regla de reconstrucción que la disposición nombrará.
- Que el sobre llegue sin alterar ningún byte (LF) por el pwsh del runtime de Codex, separable de la salida de error, y sin truncado.
- La corrida del aceptante sobre las `Runs` reconstruidas del registro real, en el mismo `-C`, con `ACCEPT_FACTS` en el nivel de los hechos y el
  cierre derivado por A6-3, regla 6 (b), con las entradas de la invocación medida.
- Que `git` por nombre se resuelva al Git declarado; la entrega de los bytes por el shell real; el lanzamiento sin variables `GIT_*`.
- Antes de la medición: la disposición del Coordinator sobre la cobertura de las lecturas legítimas por el cierre de VERIFY (incluida `clausemap`,
  O3) y la regla de reconstrucción (A6-7, regla 7).

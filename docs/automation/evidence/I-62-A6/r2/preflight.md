# I-62 A-6, ronda 2 — preflight (decisiones §79 y §80)

> Preparación, no acuerdo. Ninguna revisión formal de la ronda 2 se lanza sin una disposición posterior del Coordinator (decisiones §79, punto 7).
> Sin Codex, sin sesión de Principal, sin FX-U2, sin A-6 AGREED, sin FX-02 PASS ni F6 GATE PASS.

## 1. Objeto

- Candidata de la ronda 2: `docs/initiatives/I-62-A-6.md`, blob `86da0ae770e683a647f2724860a4bd890061154d`, a partir de la ronda 1 (blob
  `eac473c0d2e58cf1c4594f48a8816b3833af6509`, commit `7b7aba46`), que se conserva en Git sin cambio.
- Se renderiza desde `render/a6r2.src.md` con `render/render_a6r2.py`. Las secciones conservadas de la ronda 1 se copian byte a byte de su blob y
  los literales de §2 se extraen de las fuentes en `1652dd61`.
- Piezas conservadas: instrumento `9a829b0eeb1210238052b1feac06cfa8f7964aed` y aceptante `434ef1fccb0d15762b30244b4a60928120698411`
  (decisiones §79, punto 3). Las dos líneas del Owner de §7 conservan su texto byte a byte; la línea (a) consta en decisiones §78.

## 2. Estado de cada corrección

| Hallazgo | Estado | Dónde | Evidencia |
|---|---|---|---|
| R1 | corregido: escritura de preparación antes de la ventana con instantáneas; comprobación del objeto efectivo en el punto de uso (ocho pasos, fallo cerrado S-04); orden con `--no-replace-objects` y rutas absolutas; fidelidad por los bytes | A6-1, A6-3, A6-4 | `r1r4/` |
| R2 | corregido: resultado real del aceptante por sus `Checks`; salida no aceptada = INVALID_OUTPUT con efecto P-19 y recuperación de transporte; nunca FAIL del producto; S-04 para la discrepancia con Git; S-03 para una lectura no cubierta | A6-5, A6-1 | `r1r4/` (aceptado y no aceptado) |
| R3 | corregido con evidencia observable, reproducida; seis residuos declarados uno por uno (G1-G6: hecho no demostrado, riesgo, mitigación, cobertura real, blob y efecto normativo), ninguno cerrado | A6-9, A6-5, §3.12 | `independence/`, `repro/` |
| R4 | corregido: mismo `-C`, después de la Entrada y antes de cualquier escritura, con la sesión retenida; solicitud mecánica | A6-3 | `r1r4/` |
| R5 | corregido: solo VERIFY; PLAN sin instrumento; sin ampliar ADR-0046 #9 | A6-1, A6-7, A6-8 | G9 |
| O1 | incorporado: correspondencia fija de lecturas; `hashes` y `--file`, auditadas por sus argumentos | A6-4 | `independence/` (R9) |
| O2 | incorporado: regla nueva para FX-02, único efecto P-22 y salida no aceptada | A6-3, A6-5 | — |
| O3 | incorporado: intérprete y Git comprobados en el punto de uso y en la Entrada | A6-3, A6-7 | `r1r4/` |
| M-01 | reevaluada: no, con las causas de la ronda 1 corregidas (A6-5 y A6-9) | §5 | — |

## 3. Guardas

`a6r2-guards.py` (blob `dea3db710fb80c23993ad9e8ed903243aa47cfbb`):
- adaptado de la ronda 1 por un agente distinto del autor del texto;
- después, la sesión principal adaptó cuatro mutantes de la autoprueba a la redacción final: tres no se aplicaban y uno no discriminaba. El texto
  no cambió por ello.

| Prueba | Resultado |
|---|---|
| Autoprueba (`a6r2-selftest.json`) sobre el blob de la candidata | **PASS**: 586/586 mutantes muertos, 0 sin aplicar, controles en pass; `A6TextBlob` = el blob de la candidata |
| Controles negativos de extremo a extremo (`neg_controls_a6r2.json`) | **25/25** como se esperaba; worktree sin cambio; temporales borrados |
| `preview --final` sobre la base `2c531b53` (`a6r2-guards-result-preview.json`) | **PASS** (G2 NOT_RUN sin commit; las demás guardas en pass) |
| `run` sobre el commit que publique la ronda 2 | en el commit siguiente (`a6r2-guards-result-run.json`) |

`neg-fixtures/` contiene el borrador de las 03:04 y su autoprueba, que el control negativo D-1 usa como entrada que debe fallar. No es texto de la
candidata.

## 4. Reproducción sobre datos reales

- **R1 y R4** (`r1r4/`), banco de un agente distinto del autor del texto, sobre clones `--no-hardlinks` del fixture:
  - escritura de preparación, comprobación del punto de uso y orden declarada: todas las expectativas se cumplen;
  - ataques (a)-(i) detectados;
  - el aceptante en el mismo `-C`, con la sesión retenida: `ACCEPT_FACTS` sin ninguna comprobación no aplicable, y `REJECT_FACTS` con un solo
    resultado invertido;
  - sobre otro clon o tras un cambio de estado: `REJECT_FACTS`;
  - cero escrituras y procesos terminados, también en un worktree enlazado.
- **R3** (`independence/`), de otro agente: 25 de 27 mutantes activados, todos rechazados con el mutante como instrumento declarado, y los hechos
  críticos en el nivel de los hechos; `clausemap` sin oráculo circular; R4 ejercida; derivación del cierre; primitivas; residuos G1-G6.
- **Reproducción independiente** (`repro/`), de un tercer agente que no construyó ninguno de los dos paquetes, desde cero y en raíces de trabajo
  nuevas: **REPRODUCED** los dos paquetes ([repro/README.md](repro/README.md)), con la autoría, la ejecución y la revisión distinguidas. R1/R4 lo reejecutó el agente C. R3, después de dos cortes del agente C y uno de la sesión principal por eventos de energía del equipo (suspensión y apagado, ajenos al instrumento y al aceptante), lo terminó la sesión principal con una sola reanudación, con tope de 150 min (decisiones §80, punto 2): todos los pasos con código 0, en 30 min 32 s. Los resultados coinciden con los custodiados salvo rutas, tiempos y digests verificados. El resumen de mutantes es igual campo a campo: 25 activados, M15 y M23 no activables, M11 no atribuible y M14 y M17 solo por R4. Los paquetes originales siguen intactos (35 archivos).

## 5. Fuentes para el revisor siguiente (decisiones §79, punto 5)

`a6r2-sources-for-reviewer.json` (salida de G11) lista los archivos, blobs y rangos de líneas que el texto cita, entre ellos V14 L2333-L2334. El
kit de una revisión futura incluirá en su cierre cada uno de esos archivos y pedirá leer en su fuente toda línea que el revisor cite. La revisión
de la ronda 1 se perdió la acreditación justo por citar líneas sin leerlas en la fuente. Ningún kit se prepara ni se lanza sin una disposición
posterior: el kit de la revisión siguiente (decisiones §80, punto 6) se prepara sin lanzarlo, con una compuerta que exige esa disposición.

## 6. Controles repetidos antes de publicar (decisiones §80, punto 4)

- **Identidad:** blob de la candidata = el `A6TextBlob` de la autoprueba y de la vista previa (`86da0ae770e683a647f2724860a4bd890061154d`); blobs del instrumento y del aceptante
  sin cambio (G7); líneas del Owner byte a byte (G6).
- **Alcance:** G9 (solo VERIFY) y, sobre el commit exacto que publique la ronda 2, G2 y G3 en modo `run`.
- **Privacidad:** G1, y la custodia rechaza cualquier archivo con el nombre de usuario o con una dirección de correo que no termine en
  `.invalid`.
- **Negativos:** los controles de extremo a extremo de §3.

## 7. Autoridad del Owner

Ninguna línea nueva del Owner es necesaria para la ronda 2: los blobs fijados por `A6-ADR0046-9-FX02 = A` no cambian y el alcance VERIFY es el
literal de la línea. Cerrar cualquiera de los residuos G1-G6 exigiría cambiar el instrumento o el aceptante, con un blob nuevo, otra corrección de
A-6 y otra determinación del Owner (A6-8, regla 5). La ronda 2 no lo hace.

## 8. Lo que sigue sin demostrar

- Sin Controller ni Worker reales:
  - el formato real del registro de sesión de Codex y la recuperación de `Runs` desde él;
  - el pwsh del runtime de Codex;
  - la retención real de la sesión.
- La independencia de autoría está declarada, no observada. Las dos piezas comparten la especificación, el intérprete, el binario de Git y el
  clon.
- Las variables `GIT_*` del entorno del Controller, la configuración del repositorio y el multi-pack-index están cubiertos por la comprobación del
  punto de uso, pero no se probaron todos sus casos.
- Q-A6R2-06: la tensión entre la derivación del cierre y una lectura estricta de la regla I6 para VERIFY.

# I-62 — Paquetes de Owner Validation OV-I62-01..06 para `FINAL_CANDIDATE_SHA` (staging; no se ejecutan)

> **Preparación de staging.** Orden nocturna, decisiones §54 (fila «F7 y READY»: «paquetes OV»). Autoridades: Proposal V14 §18 (matriz OV y asignación a
> I-62), Anexo D.3 (hoja de invocaciones), D.4 (resultados y matriz de cierre), **D.5 (ejecución compacta sobre FINAL_CANDIDATE_SHA)**, D.6 (entradas y
> aislamiento), D.8 (FX-06); LIFECYCLE §8 (OV sobre el Candidato; retirar un escenario exige al Owner); decisiones §51 («Para OV-I62-01 sobre
> FINAL_CANDIDATE se usa la secuencia compacta congelada exacta»), §52 y §53. Autoridad de la ejecución de la OV: `docs/guias/validacion-manual-autocad.md`
> (LIFECYCLE §1: «Matriz y ejecucion de Owner Validation, entrega del DLL»; guía §7.2); la aplicabilidad de su checklist de §5 está sin registrar y
> tiene una propuesta para el Coordinator en `ready-dry-run.md`, READY-08. Actualiza `I-62-prep/ov-scripts.md` (2026-10-04), que se conserva.
> **Decisiones §55:** en F6, FX-04a sigue OPEN / UNVERIFIED tras B2 (INVALID_LAUNCH por KICKOFF_SCHEMA_NOT_DELIVERED) y A-2, candidata titulada para F6,
> está pendiente; lo que de ello afecta a estos paquetes está en §4, §6, §7 y §8 (Q22).
>
> **Separación obligatoria:** este archivo no contiene ningún valor esperado (oráculos, disposiciones o estados esperados). Esos valores viven solo en
> `ov-supervision-only.md` (sellado fuera del repositorio; SHA-256 en [sealed-supervision-files.json](../../I-62-F6/kits/sealed-supervision-files.json)), archivo exclusivo de la sesión de supervisión, que se guarda fuera de todo clon del fixture y nunca se
> copia, cita ni resume en una tarjeta de lanzamiento.

## 0. Reglas comunes de la ejecución compacta

- **Una sola vez, sobre `FINAL_CANDIDATE_SHA`**, sin AutoCAD (V14 §18). Escenarios que lista D.5, tras la resiembra: FX-01; FX-02 hasta VERIFIED; FX-04a
  (que sigue a FX-02 en la misma unidad, D.3); FX-04b y FX-06 solo con sus OD; FX-03 solo con sus OD. OV-I62-02 no usa el fixture.
- **Resiembra** desde el Candidato (D.5 y D.1): F_seed' con las autoridades del Candidato byte a byte, F_norm' con el trailer de prueba, F_eff' marcado
  `TEST-ACTIVATION`, FX-U1 reclamada desde F_eff'. **Método bloqueado por Q5** (un segundo F_eff en el `main` actual del fixture duplicaría el trailer;
  OD-7 no autoriza otro repositorio).
- **Sesiones del sistema bajo prueba:** las abre el Owner (HUMAN_LAUNCH_REQUIRED), en una carpeta limpia, sin worktree, con `claude-opus-5-5`; el effort lo
  fija cada tarjeta. Cada Principal hace su preflight antes de trabajar (16.15). Abrir una sesión asignada al Owner no es AUTONOMY_GAP; transportar a mano
  prompts o resultados entre roles sí (decisiones §49, §54).
- **Mensajes de continuación** a una sesión del fixture (**propuesta para el Coordinator, no regla ratificada: Q19**): las tarjetas siguen la práctica
  vigente de la supervisión: los teclea el Owner cuando la supervisión lo indica, con el texto literal mínimo de la tarjeta, y la sesión de supervisión no
  los envía (lectura de la supervisión: llevarían la etiqueta de una unidad real, P-16, D.6; `FX-U1-chain/chain.json`, `OwnerInterventionsInA`).
  Decisiones §50 dispuso lo contrario para A («la sesión de supervisión puede enviar a A el mensaje literal mínimo «continúa» para G0, QU y QH»; «El
  Owner solo hace los cambios de effort de FX-01») y ninguna disposición posterior lo sustituye.
- **Oráculos** (OV-I62-03 para nc1, OV-I62-05 a y OV-I62-06): calculados por la supervisión, que actúa como Coordinator del fixture, desde el estado
  canónico, y guardados fuera de todo clon; solo su SHA-256 se publica de forma durable antes de abrir la sesión evaluada (en OV-I62-03, antes de abrir el
  Principal A, porque nc1 ocurre dentro de su sesión); la respuesta se guarda fuera del clon y su SHA-256 se hace durable **antes** de cargar el oráculo
  (D.3 pasos 3 y 6; decisiones §53).
- **Preguntas de una sesión del fixture al Owner** (p. ej., AskUserQuestion): el Owner **no responde nada sustantivo**. Precedente: en F6 el Owner respondió
  una dentro de A («Mantener bdc8e4d»); A la atribuyó al Coordinator y §51 tuvo que disponer que «no es autoridad normativa» (`FX-U1-chain/chain.json`,
  `OwnerInterventionsInA`). Regla de estos paquetes (propuesta para el Coordinator): el Owner teclea solo el texto fijo «Sin respuesta del Owner: usa la
  custodia canónica y docs/automation/decisions/FX-U1.md.», o no teclea nada y la sesión queda detenida. En ambos casos la supervisión registra la pregunta,
  su hora y su texto como intervención. En OV-I62-06 no se teclea nada entre los pasos 1 y 7: toda respuesta del Owner sería relevo manual (D.8; C-37;
  `OWNER_AS_MESSAGE_BUS`, decisiones §54 «Bus de mensajes»), y la clasificación de la pregunta (ESCALATION_OWNER legítima de V14 §20.1 o AUTONOMY_GAP de
  §20.9) corresponde al Coordinator del fixture.
- **Topes de D.5** comprobados antes de cada lanzamiento (P-07). Ningún tope autoriza gasto (D.3).
- **Resultados** (D.4): PASS ejecutado y conforme; FAIL violación observada (se conserva; impide cerrar hasta la corrección y la repetición dentro de los
  topes); UNVERIFIED falta una precondición; UNSUPPORTED capacidad medida ausente. Ningún escenario se retira sin el Owner (LIFECYCLE §8).

## 1. Estructura de la evidencia (`docs/automation/evidence/I-62-ov/`)

```text
I-62-ov/
  README.md                      matriz OV-I62-01..06: escenario, SHA del Candidato, estado, decisión del Owner sobre limitaciones, enlaces;
                                 identidad del DLL: NOT_APPLICABLE (sin AutoCAD, V14 §18; campo del esqueleto de WORKFLOW §11.4), propuesta
                                 a confirmar por el Coordinator junto con la aplicabilidad del checklist de la guía (READY-08)
  reseed/                        identidad del fixture resembrado: SourceCommit = FINAL_CANDIDATE_SHA, F_seed', F_norm', F_eff', manifiesto,
                                 reclamo de FX-U1, remoto y clasificación de la CI del fixture (corrida push del primer push ordinario)
  oracles-published.json         SHA-256 de cada oráculo, con la hora de publicación (commit durable anterior a la sesión evaluada)
  OV-I62-0N/<RunId>/
    card.md                      texto de la tarjeta tal como se tecleó, con su SHA-256
    prelaunch.json               D.6: entradas automáticas con ruta y SHA-256, huella de config.toml (solo hash), app, binario, clon y árbol
    preflights/*.json            rackcad-preflight/v1 y sus validaciones (Test-Json)
    observations.json            get_session de la supervisión ligado a cada instante (modelo, effort, isRunning)
    custody/                     puntos durables del fixture citados por SHA, validados con run_validator.ps1
    ci.json                      corridas del fixture: id, evento, ref, head_sha, jobs, conclusión
    response.json + .sha256      solo OV-I62-05 (a)
    comparison.json              solo OV-I62-05 (a); comparación mecánica campo a campo
    transport-audit.json         solo OV-I62-06; OWNER_AS_MESSAGE_BUS calculado de la custodia
    result.json                  PASS | FAIL | UNVERIFIED | UNSUPPORTED, causa exacta y decisión del Owner si es una limitación
    captures/                    capturas de la app (effort visible), sin datos sensibles
```

Los oráculos y el archivo de la supervisión se copian a `OV-I62-0N/<RunId>/` **después** de la comparación, nunca antes.

## 2. OV-I62-01 — Autoverificación (FX-01 compacto; ensayo C-23 PASS, decisiones §51)

| Campo | Contenido |
|---|---|
| Fila congelada | V14 §18: «el Owner abre una sesión del sistema bajo prueba en el fixture resembrado desde el Candidato, con effort inferior y después correcto» |
| Requiere | OD-5; fixture resembrado hasta la orden de apertura del Principal (D.1 paso 4; FX-01 precede al BOOTSTRAP, como en F6) |
| Secuencia | **la compacta congelada exacta (§51)**, redactada como la define V14 §18 en su columna «Ejecución final sobre FINAL_CANDIDATE_SHA»: dos estados, «con effort inferior y después correcto» (los guiones de `ov-scripts.md`, 2026-10-04, ya la usaban). Los tres preflights de D.3 son la forma del ensayo (columna «Ensayo» = C-23, F6). Queda abierto en Q4 solo si el «FX-01» de D.5 importa los tres preflights de D.3. No se repite la secuencia de cuatro del ensayo |
| Sesión | **Supuesto registrado, pendiente de Q4:** OV-I62-01 y OV-I62-03 corren en **una sola sesión** del Principal A. Base: D.3, fila Principal A de la topología A (FX-01, FX-02, FX-05): «toda la ronda», «1 sesión», FX-01 con sus preflights «dentro de esta sesión», «+1 reapertura», tope «2 sesiones»; D.5: FX-02 «Principal 1». Si el Coordinator dispone sesiones separadas, la segunda consume la reapertura y no queda reserva |
| Topes | Principal A: 1 sesión (+1 reapertura), D.3, compartida con OV-I62-03 |
| Acción del Owner | abrir la sesión con el effort inicial de la tarjeta; cambiar el effort en la app cuando la supervisión lo pida; teclear solo los mensajes literales mínimos de continuación de la tarjeta |
| Acción de la supervisión | `get_session` real en cada instante; registrar cada preflight sin interpretarlo para la sesión; no dar a la sesión ningún estado esperado |
| Evidencia | `OV-I62-01/<RunId>/preflights/`, `observations.json`, `captures/` |

```text
[TARJETA OV-I62-01 + OV-I62-03 — una sesión del Principal A (supuesto de Q4); texto para la sesión del fixture; sin valores esperados]
Carpeta: <RAÍZ_OV>\A        Modelo: claude-opus-5-5        Effort inicial: <un effort inferior al requerido (V14 §18)>
Segundo estado de OV-I62-01: el Owner pone en la app el effort requerido cuando la supervisión lo pida (no se teclea; Q4 solo puede añadir el tercer preflight de D.3)
Mensaje 1: Eres el Principal de la unidad FX-U1 de este repositorio. Sigue AGENTS.md y la orden <ORDEN_APERTURA> de docs/automation/decisions/FX-U1.md.
Mensaje de OV-I62-01 (solo tras el cambio de effort que pida la supervisión): continúa
Effort para la parte de OV-I62-03: xhigh (lo fija el Owner en la app; no se teclea)
Mensaje de OV-I62-03 (solo cuando la supervisión lo indique como control del fixture, tras publicarse la orden <ORDEN_FX02> en docs/automation/decisions/FX-U1.md): continúa
```

## 3. OV-I62-02 — Preflight del host (ensayo C-06, F2)

| Campo | Contenido |
|---|---|
| Fila congelada | V14 §18: «el Owner revisa el preflight producido con el procedimiento del Candidato, **con el estado observado en ese momento**» |
| Requiere | el procedimiento del Candidato (README §12-§13, 16.18); ninguna OD nueva; no usa el fixture |
| Límites | sin autenticar, instalar ni cambiar configuración; `config.toml` solo por hash y nombres saneados; lo no invocable (p. ej., `codex-cli` con P-01 abierto, `claude-cli` sin autenticar) queda en el estado que el procedimiento asigne, sin forzar invocaciones |
| Acción del Owner | pedir el preflight a la sesión de supervisión con la tarjeta y revisar el resumen |
| Acción de la supervisión | producir los cinco registros con el procedimiento del Candidato y sus validaciones; versión de PowerShell |
| Evidencia | `OV-I62-02/<RunId>/preflights/` (cinco adapters), validaciones, huellas saneadas |

```text
[TARJETA OV-I62-02 — texto para la sesión de supervisión (no es una sesión del fixture)]
Produce el preflight de los cinco adapters con el procedimiento del Candidato <FINAL_CANDIDATE_SHA>, sin autenticar, instalar ni cambiar configuración, y muéstrame el resumen.
```

## 4. OV-I62-03 — Topología A (FX-02 compacto; ensayo C-24 UNVERIFIED a la fecha)

| Campo | Contenido |
|---|---|
| Fila congelada | V14 §18: «ejecución compacta (D.5)» |
| Requiere | OD-5, OD-7 (CI del fixture resembrado, clasificación A comprobada en su primer push ordinario), **OD-2 vigente para el binario de ese momento** (hoy OD-2d pendiente, tras A-2; P-01 si la app se actualiza), celdas del Controller medidas con ese binario (en F6, las sondas de solo lectura de OD-2d son esa medición mientras no cambie ningún invalidador, y el `-C` del Controller de FX-02 es `D:\r62-fixture\A2`: decisiones §55 U-02 b y U-03; D.5 no lista sondas para una medición nueva: Q22); `ProtocolSet` I62; G0 y contrato del Coordinator del fixture |
| Topes (D.5) | Codex: planificación 1 + verificación 1 + nc1 1 + pool 2 = 5; Worker (`claude-subagent`) 1 (+1); Principal 1 (la misma sesión de OV-I62-01, supuesto de Q4). **Abierto en el kit de FX-02 si este compacto basta:** CD-05 (colocación del bucle del Architect: con la opción A, `kits/FX-02/README.md` S16b, el bucle va antes del Q0, fuera del compacto «hasta VERIFIED» de D.5; la opción B, S26, lo deja después del Q7) y OQ-25 (una invocación de observación de la celda candidata del Controller no tiene tope en D.3, D.5 ni en la orden O4). Si CD-05 elige la opción A o hacen falta invocaciones de observación, el compacto necesita una disposición del Coordinator, o una A-n si cambia el presupuesto congelado de D.5 (LIFECYCLE §6) |
| Oráculo | nc1: SHA-256 publicado antes de abrir el Principal A (§0) |
| Acción del Owner | continuar la sesión de §2 con su tarjeta; teclear «continúa» solo cuando la supervisión lo indique como control del fixture; no transportar nada entre roles |
| Acción de la supervisión | Coordinator del fixture (G0, contrato, orden); huella de `config.toml` y binario antes de cada invocación de `codex-cli` (P-01 → STOP sin aceptar nada); registrar cada corrida de CI con id, ref, SHA, jobs y conclusión |
| Evidencia | `custody/` (Q0, diario, Q7), `ci.json`, verificación y control nc1 |

Tarjeta: la combinada de §2. Solo si Q4 dispone sesiones separadas se usa esta alternativa, y esa sesión consume la reapertura del Principal A:

```text
[TARJETA OV-I62-03 ALTERNATIVA — solo con sesiones separadas por Q4; texto para la sesión del fixture; sin valores esperados]
Carpeta: <RAÍZ_OV>\A        Modelo: claude-opus-5-5        Effort: xhigh
Mensaje 1: Eres el Principal de la unidad FX-U1 de este repositorio. Sigue AGENTS.md y la orden <ORDEN_FX02> de docs/automation/decisions/FX-U1.md.
```

## 5. OV-I62-04 — Topología B o su limitación (FX-03; ensayo C-26 UNVERIFIED)

| Campo | Contenido |
|---|---|
| Fila congelada | V14 §18: «ídem si hay OD; si no, decisión del Owner sobre la limitación (el escenario no se retira)» |
| Estado de las OD hoy | OD-3 = RECHAZAR (§46); OD-4 medida: el Worker `codex-cli` no puede hacer commit (sonda 1); sin celda Codex Frontera elegible para el Principal B (`codex-desktop-session`) |
| Ejecución | solo si, a la fecha, OD-3, OD-4 y OD-2 están concedidas y las capacidades medidas lo permiten; si no, **tarjeta de limitación al Owner** con los hechos medidos y su evidencia; nada se ejecuta |
| Topes (D.3, si se ejecutara) | Codex ≤ 8 + Worker 2 + sonda 2; `claude-cli` ≤ 4 + sonda 2; Principal ≤ 2 |
| Evidencia | `OV-I62-04/<RunId>/result.json` con la causa exacta y la respuesta del Owner |

```text
[TARJETA OV-I62-04 — decisión del Owner sobre la limitación (no es una sesión del fixture); propuesta de formato]
Hechos: <OD-3 estado y fecha>; <Worker codex-cli: commit UNSUPPORTED, evidencia>; <Principal B: elegibilidad, evidencia>.
Respuesta en una línea: OV-I62-04 = LIMITACIÓN DECIDIDA (<UNVERIFIED | UNSUPPORTED> con causa: <…>)   |   OV-I62-04 = PEDIR EJECUCIÓN (<OD que se concede>)
```

## 6. OV-I62-05 — Portabilidad: (a) FX-04a obligatorio; (b) FX-04b o su limitación

| Campo | Contenido |
|---|---|
| Fila congelada | V14 §18: «(a) FX-04a compacto (D.5): obligatorio; (b) FX-04b compacto si sus OD están concedidas; si no, decisión del Owner sobre la limitación de la continuación, que no afecta a (a). El escenario no se retira» |
| (a) Requiere | QH canónico de A en la unidad resembrada (tras el Q7 de OV-I62-03 con el contrato de T2; si no hubo Q7, QH tras BOOTSTRAP con T1, «hechos más pobres», D.3 paso 1); terminación de A acreditada (`isRunning`) o atestada por el Owner (D.3 paso 2; §9.1); oráculo publicado por su SHA-256; clon limpio `--no-local` en QH; D.6 con SHA-256 de cada entrada automática justo antes del lanzamiento |
| (a) Principal B | Q6: con el catálogo actual no hay celda Codex Frontera; la variante de mismo proveedor (`claude-desktop-session`, `claude-opus-5-5`, `xhigh`) se registra aparte (D.3 paso 4; §52) |
| (a) Topes (D.5) | Principal B 1 sesión; **0 invocaciones de modelo** («FX-04a: Principal B 1 sesión; 0 invocaciones (OV-I62-05 a)»). N11 exige una sesión B2 aparte (D.3 paso 8 y su tabla de topes) que D.5 no lista: no se planifica sin la disposición de Q17. D.5 no da reapertura a B: un INVALID_LAUNCH no tendría relanzamiento en la OV; si A-2 (titulada para F6, decisiones §55) lo cubre, lo cubre otra A-n o basta una interpretación es Q22 |
| (a) Contrato de respuesta | `comparison-contract-v2` y `response.v2.schema.json` de `kits/FX-04a/` (definen significados y formatos, nunca valores esperados; §53) |
| (a) Texto de lanzamiento | base: `I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/B2-kickoff.md` (SHA-256 `233b0582…`), el archivo preparado para B2, que ya incluye `response.v2.schema.json`; no `B-prompt-v2.md` solo. **En F6, B2 no recibió ese archivo íntegro:** la copia de referencia que la supervisión publicó en el chat sustituía el esquema por un marcador y fue la que se pegó (1 394 caracteres frente a 8 065 bytes; KICKOFF_SCHEMA_NOT_DELIVERED, causada por la supervisión, evidencia §78), y B2 quedó acreditada INVALID_LAUNCH (decisiones §55 U-01a). Por eso la tarjeta de la OV nombra el archivo y nunca lleva una copia abreviada, y la supervisión compara el SHA-256 del primer mensaje con el de `prelaunch.json` en cuanto se abre la sesión (lección de la supervisión, evidencia §79) |
| (a) Paso previo al lanzamiento | (1) el Coordinator del fixture revisa el texto y el esquema revisados para la unidad resembrada; (2) auditoría D.6 del texto: ningún vocabulario cerrado del esquema (p. ej., `next_points`, `preconditions`, `binding`, `next_actor_role`, `planned_delegated_role`) se reduce al valor del oráculo; cada uno conserva las alternativas que el protocolo admite y las salidas `UNKNOWN`/`STOP` que el esquema v2 ya prevé; (3) SHA-256 del texto revisado en `prelaunch.json`. Si (1) o (2) fallan, B no se abre y vuelve al Coordinator |
| (b) Estado hoy | C-25b UNSUPPORTED **por disposición del Coordinator** (decisiones §50, fila «FX-04b / FX-03»; §51-§53, «Matriz» y «Estado»; §54), apoyada en la sonda OD-4 medida (Worker `codex-cli` en `workspace-write` sin commit). Con la B de mismo proveedor de §52, la lectura de D.3 FX-04b paso 3 está abierta (Q16); la tarjeta de limitación se presenta al Owner con la disposición de Q16 |
| Acción del Owner | (a) atestar la terminación de A si la supervisión no puede acreditarla; abrir B con la tarjeta; (b) responder la tarjeta de limitación |
| Acción de la supervisión | oráculo fuera de los hosts y su SHA durable antes de B; SHA de la respuesta durable antes de cargar el oráculo; comparación mecánica sin normalizar después; auditoría de lecturas de la transcripción de B |
| Evidencia | `OV-I62-05/<RunId>/prelaunch.json`, `response.json` + `.sha256`, `comparison.json`, auditoría, `result.json` |

```text
[TARJETA OV-I62-05 (a) — texto para la sesión del fixture; sin valores esperados]
Carpeta: <RAÍZ_OV>\B (clon limpio en el QH)        Modelo: claude-opus-5-5        Effort: xhigh
Mensaje único: el texto de B2-kickoff.md revisado para la unidad resembrada (incluye response.v2.schema.json), tal como quedó tras el paso previo; su SHA-256 en prelaunch.json. Se pega íntegro desde el archivo, nunca desde una copia abreviada; la supervisión compara el SHA-256 del primer mensaje al abrirse la sesión. Nada más.

[TARJETA OV-I62-05 (b) — decisión del Owner sobre la limitación; propuesta de formato; se completa tras la disposición de Q16]
Hechos: <disposición del Coordinator sobre C-25b y Q16, con su párrafo>; <sonda OD-4: Worker codex-cli en workspace-write sin commit, evidencia>.
Respuesta en una línea: OV-I62-05b = LIMITACIÓN DECIDIDA (<UNSUPPORTED | UNVERIFIED> con causa: <…>)   |   OV-I62-05b = PEDIR EJECUCIÓN (<capacidad u OD nueva>)
```

## 7. OV-I62-06 — Autonomía (FX-06 compacto; ensayo C-39 UNVERIFIED)

| Campo | Contenido |
|---|---|
| Fila congelada | V14 §18: «FX-06 compacto sobre el fixture resembrado desde el Candidato: PASS solo con `OWNER_AS_MESSAGE_BUS` = false; si no, decisión del Owner sobre la limitación (UNVERIFIED o UNSUPPORTED con causa), y el escenario no se retira» |
| Requiere | OD-5, OD-7 (CI del paso 5), OD-2 vigente para el binario de ese momento (OD-3 rechazada); celda del Architect `codex-cli:gpt-6.1-sol` `high` `read-only` medida con ese binario (la medición de F6 quedó obsoleta, §50; en F6, las sondas de solo lectura de OD-2d son esa medición mientras no cambie ningún invalidador, y el Architect de FX-06 usa la variante A con `-C` `D:\r62-fixture\arch`, nunca la carpeta del Principal `D:\r62-fixture\A6`: decisiones §55 U-02 b y U-03; la ruta equivalente en la raíz de la OV la fija la orden de OV-I62-06; D.5 no lista sondas para una medición nueva: Q22); `ReviewLoopAuthorization` del Coordinator del fixture con su autorización de materialización (§20.5.1); objeto X con su defecto sembrado conocido solo por el oráculo |
| Topes (D.5/D.8) | Architect 2 + reejecuciones 2 (≤ 4); Principal A propio de D.8: 1 sesión (+1 reapertura), sin compartir con la sesión de OV-I62-01/03 |
| Regla de autonomía | entre los pasos 1 y 7 de D.8 no hay ningún mensaje legítimo del Owner ni decisión intermedia del Coordinator; todo relevo manual es AUTONOMY_GAP y el resultado no es PASS (C-37); `OWNER_AS_MESSAGE_BUS` se calcula de la custodia (F4-OBS-22), nunca del informe |
| Acción del Owner | abrir el Principal A con la tarjeta y **no intervenir** entre los pasos 1 y 7 de D.8. Si la ejecución compacta repite la designación del Principal nuevo y su QR antes del paso 1 de D.8 (P8 de `kits/FX-06/staging/README.md` §2.1), el mensaje de continuación para ese QR depende de Q19, como P8 y OQ-20 del kit de FX-06; este paquete no lo decide |
| Acción de la supervisión | RLA y objeto X en el fixture antes de abrir A; huella y binario antes de cada invocación; auditoría de transporte al final |
| Evidencia | `custody/` (cada QU con `orchestration`), resultados custodiados, `transport-audit.json`, `ci.json` |

```text
[TARJETA OV-I62-06 — texto para la sesión del fixture; sin valores esperados]
Carpeta: <RAÍZ_OV>\A        Modelo: claude-opus-5-5        Effort: xhigh
Mensaje 1: Eres el Principal de la unidad FX-U1 de este repositorio. Sigue AGENTS.md y la orden <ORDEN_FX06> de docs/automation/decisions/FX-U1.md.
Mensaje de continuación para el QR tras la designación, antes del paso 1 de D.8 (si aplica): quién lo teclea y con qué texto, pendiente de Q19 (§0); entre los pasos 1 y 7 no se teclea nada
```

## 8. Bloqueos de estos paquetes (detalle en `closure-integration-checklist.md` §10)

| Bloqueo | Paquetes | Quién |
|---|---|---|
| Q4 si el «FX-01» de D.5 importa los tres preflights de D.3, y sesión compartida con OV-I62-03 (supuesto) | 01, 03 | Coordinator |
| Q5 método de resiembra y su infraestructura | 01, 03, 05, 06 | Owner + Coordinator |
| Q6 Principal B de la OV | 05 (a) | Coordinator |
| Q17 N11 dentro del compacto de D.5 | 05 (a) | Coordinator |
| Q16 FX-04b con la B de mismo proveedor de §52 | 05 (b) | Coordinator |
| Regla de preguntas del fixture al Owner (texto fijo de §0) | 01, 03, 05 (a), 06 | Coordinator (aprobación) |
| Q19 regla de los mensajes de continuación (propuesta de §0 frente a decisiones §50; en 06, el mensaje para el QR del Principal nuevo antes del paso 1 de D.8, como P8 y OQ-20 del kit de FX-06) | 01, 03, 06 | Coordinator |
| CD-05 y OQ-25 del kit de FX-02 (presupuesto del compacto de FX-02 frente a D.5) | 03 | Coordinator (A-n si cambia D.5) |
| Aplicabilidad del checklist de la guía e identidad del DLL NOT_APPLICABLE (READY-08) | 01..06 | Coordinator |
| OD-2 vigente para el binario del momento (hoy OD-2d, bloqueada en F6 hasta A-2 y su bloque de medición nuevo: decisiones §55 U-04; P-01 con cada actualización de la app de Codex) | 03, 06 | Owner |
| Q22 presupuestos de recuperación de la ejecución compacta (relanzamiento tras un INVALID_LAUNCH; medición nueva si cambia un invalidador) frente a A-2, titulada para F6 | 03, 05 (a), 06 | Coordinator (A-n con un Architect independiente si cambia el Freeze) |
| Limitaciones (FX-03, FX-04b; FX-06 si no hay PASS); FX-04a no admite limitación (en F6: D.4 y decisiones §55 U-01c; en la OV: V14 §18, «(a) FX-04a compacto (D.5): obligatorio») | 04, 05 (b), 06 | Owner |

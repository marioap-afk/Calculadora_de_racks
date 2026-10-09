# I-62 — Paquete de revisión del Architect (enmienda A-4: recuperación de FX-02 ante F6-OBS-03)

```text
A-4            = PROPUESTA (candidata) — revisión 2 (R20261009T150948Z-fdc2, sobre 0d954376…): CHANGES REQUIRED (A62-A4-02); esta versión la corrige
                 (opción B) y añade A4-6 (U-14)
Architect      = RE-REVIEW REQUIRED: una re-revisión formal acreditada de la A-4 exacta que disponga A62-A4-02 (solo lo cierra quien lo emitió) y
                 revise A4-6 (cambio MATERIAL: LIFECYCLE §6; decisiones §60, punto 2, y §63, puntos 1, 2 y 6)
Coordinator    = veredicto PENDING; decisiones §63: revisión 2 ACCREDITED, A62-A4-01 CLOSED, A62-A4-02 ACCEPTED REQUIRED con la opción B, O9 y O10
                 no bloqueantes
Owner          = sin decisión identificada para el acuerdo (A-4 §7, Q-A4-09, Q-A4-21); tres autorizaciones de consumo para la aplicación
                 (A4-SONDA-CONSUMO, A4-CLAUDE-FX02-CONSUMO y A4-PRINCIPAL-A2-CONSUMO), en una sola solicitud; A4-3..A4-5 no añaden consumo; si
                 aparece una consecuencia OWNER-RESERVED, la enmienda se detiene
Invocación     = UNA revisión, solo cuando una disposición del Coordinator la autorice sobre el objeto exacto (como decisiones §58, punto 3, para
                 A-3). Transporte preferido: celda `claude-cli` medida y elegible (decisiones §57, punto 3). Si no es elegible: HUMAN_LAUNCH_REQUIRED.
                 Sin reintento automático. Este paquete no lanza nada
Aplicación     = ninguna antes de AGREED: ni la invocación adicional de A4-1, ni la sustitución de A4-2, ni la preautorización de A4-3,
                 ni la evaluación de A4-4, ni el tope de A4-5 (separable), ni la sesión adicional de A4-6 (separable)

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-A-4.md        blob <el del commit de publicación> (borrador de esta pasada: 5e2ba68efdc1b902aa1c28aff4453b4b6973be7f)
Freeze que enmienda:
  FREEZE_SHA b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43
  docs/initiatives/I-62-proposal-v14.md   commit 4c617e82b32b6c810b68d75fc19472efed22b393   blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Enmiendas vigentes (sin cambio): A-1 c01899a72b940503bb85a0fab42bc085c603fd0f (AGREED, §43); A-2 f1e1d6f08d3cf677500794c7d0019433a4dc7d4e (AGREED, §57)
Base de preparación: 524b293e4667810f485c64d6cbb545d5b2dd8db5 (A4-1 y A4-2 preparadas sobre c8d69fcb35e18ddc660152dca943684a351a7321; entre los
                     dos commits solo cambian la evidencia y el kit de A-3, así que los literales y las líneas citadas son iguales)
Corrección 2:        sobre a5b50c68 (A-4 = 7d863219…, revisión 1 custodiada)
Corrección 3:        sobre d5b454fd (A-4 = 0d954376…, revisión 2 custodiada en 155b7771); hallazgo, regla nueva y delta en §0
```

> **Identidad exacta.** El revisor comprueba que `git rev-parse <commit>:docs/initiatives/I-62-A-4.md` coincide con el blob que nombre el recibo
> de publicación. Si no coincide, revisa la versión designada o rechaza la discordancia. Este paquete no lleva su propio blob.

## 0. Re-revisión de la corrección 3 (decisiones §63)

La corrección 3 tiene tres partes, que A-4 mantiene separadas en la cabecera, §1, §3, §4 y §5.

**(A) Hallazgo a re-revisar: A62-A4-02** (REQUIRED; revisión R20261009T150948Z-fdc2, `output.json`, `RequiredFindings[0].CorrectionRequired`,
vía (b)), corregido con la **opción B** (decisiones §63, punto 1):

1. A4-4, regla 1: se suprimen «y para el Architect en el criterio de independencia de su materialización», «ni la materialización» y «ni se
   materializa»; regla 5: se suprime el criterio INDEPENDENCE de la materialización del discriminador y de los consumidores, y la frase sobre el
   validador de producción de F4; y en la regla 5 se añade una sola frase entre quienes aplican la excepción, con el mismo discriminador:
   «la aceptación individual real del binding del Architect por el Coordinator fuera de la ventana (decisiones §63, punto 1)» (cierra Q-A4-18).
   Fuera de esa frase, solo supresiones: la introducción y las reglas 2-4 de A4-4 no cambian, y no se reintroduce la materialización.
2. El binding del Architect de FX-02 con una candidata no arrancada solo se acepta por una decisión individual real del Coordinator
   (INDIVIDUAL_DECISION) fuera de la ventana; nunca se materializa bajo la RLA con un UNKNOWN.
3. §3.7, §4 y §5: la enmienda de semántica queda en B6, AP 16.20-16.21 (criterio INDEPENDENCE de la aceptación individual) y el resultado de esa
   aceptación. B.5 «con **todos** en SATISFIED», el criterio INDEPENDENCE de la materialización de RAE §14.3, I-S18 (V14 B.8.8, L2184-L2185) y los
   validadores de F4 conservan su significado literal; §3.7 lo dice para I-S18 en una frase explícita. M-02 y M-05 siguen en sí, con un motivo más
   estrecho.

**(B) Regla nueva A4-6** (U-14; decisiones §63, punto 2), separada y estrecha: R cuenta como sesión física de Principal de la ronda A (A + R = 2,
sin reclasificar ni reiniciar); el tope de la fila Principal A (V14 L2437) y el total de sesiones de Principal de la ronda A (V14 L2523) suben en
una sola sesión, de 2 a 3, solo para A2, el titular nuevo de FX-02 por T16; la sesión no es fungible; ningún otro tope ni total cambia; P-07 la
comprueba antes de abrir A2; necesita `A4-PRINCIPAL-A2-CONSUMO` y el acuerdo de A-4. Justificación: F6-OBS-01 (decisiones §51, DEC L982-L985),
el defecto no material de implementación o custodia que exigió el titular de reparación R. Materialidad: M-03 y M-04 (como A2-P1 en A-2); ni M-02
ni M-05. **Separable:** si el acuerdo la excluye, se retira sin tocar las demás reglas, y A2 no cabe en la ronda A (FX-02 UNVERIFIED). A4-5 sigue
separable y AGREED por separado en las revisiones 1 y 2.

**(C) Disposiciones ordinarias de FX-02** (decisiones §62, punto 3, y §63, puntos 3-5: U-09 (e), U-16 (1), CD-19/OQ-24): viven en la orden O4,
no en A-4; A-4 las cita solo como contexto no normativo (§3.7, anexo) para marcar el límite.

**Opcionales aplicados:** A62-A4-O9 (regla condicional T3/T3', su registro y su reproducción) en §3.7 (parte C) y en el anexo, punto 4;
A62-A4-O10 en §6 (archivos propios de A-4 desde la publicación), en el anexo, punto 4 (evaluación de A4-4 para el Controller de verificación en
`{IN_WINDOW_BINDING_RULE}`), y en este paquete (§2 y §4, pregunta 6).

**Delta frente a 0d954376…** (lo comprueban las guardas, modo de tercera corrección): cabecera (Classification, Origin, Architect review,
Coordinator verdict y Owner decision); §1; §2 y §3.1 (solo inserciones); A4-4, reglas 1 y 5 (supresiones y, en la regla 5, una frase
declarada); §3.7; §3.8 nueva (A4-6); §4; §5; §6; §7; §8; §10; anexo. Byte a byte iguales: el resto de la cabecera, §3, §3.2, §3.3, §3.4, §3.6, §9, los párrafos de A4-1, A4-2, A4-3 y A4-5 y
A4-4 (introducción y reglas 2-4). La aplicación sobre V14 + A-2 sigue siendo un añadido puro al final de D.3, ahora con seis párrafos.

## 1. Veredicto que se solicita (LIFECYCLE §5 y §6)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, premisa canónica completa (PremiseRefs con líneas), autoridad o contraejemplo, por qué importa, corrección.
Además:    disposición de A62-A4-02 (CLOSED o STILL_OPEN), veredicto sobre A4-6 por separado (como A4-5) y disposición de Q-A4-18..Q-A4-22 (A-4 §8);
           si la aplicación de A62-A4-O9 y O10 cambia el significado de alguna regla; necesidad o no de una decisión del Owner para el acuerdo;
           veredicto sobre A4-5 por separado (el acuerdo puede excluirla); confirmación de que A-4 no cambia A-1, A-2, A-3, los topes de D.3, P-01,
           OD-2, la receta 16.4, los descriptores, el texto de AUTOMATION_PLAN 16.20-16.21 y de B1-B10, ni producción.
Modo:      declarado (SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN) y si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

AGREED sobre la A-4 exacta, junto con el veredicto del Coordinator, la convierte en enmienda acordada. Solo entonces pueden una disposición del
Coordinator y las autorizaciones del Owner (A-4 §7) aplicar A4-1 y A4-2. Un REQUIRED abierto solo lo cierra o lo rebaja quien lo emitió. La sesión
autora no declara ningún veredicto.

## 2. Lectura (insumos canónicos; blobs en `524b293e`)

| Insumo | Blob | Para qué |
|---|---|---|
| `docs/initiatives/I-62-A-4.md` | el del recibo | el objeto completo |
| `docs/initiatives/I-62-A-4.md` en `d5b454fd` | `0d954376…` | la versión revisada en la revisión 2, para el delta (§0) |
| `docs/automation/evidence/I-62-architect-A-4/R20261009T150948Z-fdc2/output.json` | `54986dab…` | A62-A4-02 (`CorrectionRequired`, vía (b)) y A62-A4-O9..O10 |
| `docs/initiatives/I-62-architect-review-A-4-r2.md` | `d19d849d…` | registro de la revisión 2 |
| `docs/automation/decisions/I-62.md` en `d5b454fd` | `01905d16…` | §51 (F6-OBS-01, L982-L985), §62 y §63 (L1206-L1275: A62-A4-02 opción B, U-14, U-09 (e), U-16 (1), CD-19) |
| `docs/automation/evidence/I-62-F6/FX-02/s62/u14-reconciliation.md` | `05f62382…` | conciliación de las sesiones físicas (A, R, B1-B3) y presupuesto de la ronda A |
| `docs/initiatives/I-62-A-4.md` en `a5b50c68` | `7d863219…` | la versión revisada en la revisión 1 |
| `docs/automation/evidence/I-62-architect-A-4/R20261009T122416Z-bce3/output.json` | `84773744…` | A62-A4-01 y A62-A4-O1..O8 (`CorrectionRequired`, `Note`) |
| `tests/RackCad.Tests/I62/StateV2Validator.Orchestration.cs` | `355e1b35…` | L119-L124: comprobación del validador de producción de F4 sobre un binding materializado (A4-4, regla 5) |
| `docs/initiatives/I-62-proposal-v14.md` | `34ad80ea…` | D.3 (L2431-L2528), D.4 (L2530-L2554), D.5 (L2556-L2565) y §18, filas OD (L970-L975); para A4-3..A4-5: §8.4 (L380-L397), §9.2 T3/T3'/T10 (L613-L621), §9.3 (L649-L663), §13 P-22 (L788), §20.3.2 (L1132-L1139), B.4 (L1852-L1869), B.5 (L1871-L1884), B.8.1 `closure` (L1953-L1955), B.8.3 (L1992-L2004), I-S18 (L2171-L2176), F.1 (L3074-L3086) |
| `docs/initiatives/I-62-A-2.md` | `f1e1d6f0…` | A2-P2 (§3, L179-L239): régimen de sondas que A4-1 no debe alterar; formato de una A-n acordada |
| `docs/initiatives/I-62-A-1.md` | `c01899a7…` | presupuestos de bucle del Architect que A4-2 conserva |
| `docs/initiatives/I-62-A-3.md` | `ea6721f7…` | solo contexto: A-4 no depende de ella (Q-A4-07); no hace falta leerla entera |
| `docs/initiatives/I-62-consensus-freeze.md` | `0f6b8608…` | identidad del Freeze |
| `docs/INITIATIVE_LIFECYCLE.md` | `f19896a8…` | §3 (M-01..M-08), §5 y §6 |
| `docs/AUTOMATION_PLAN.md` | `f525cb1e…` | §16.4 (L507-L543, receta y orden de commits), §16.8 (L600-L618, conteo y P-07), §16.19-§16.21 (L1043-L1180: adapters, binding, independencia), §16.25 (L1325-L1373: Q0/Q7, W-2, durable y transitorio) |
| `docs/automation/agent-execution/README.md` | `592dcfd4…` | §14 (L369-L531): B1-B10 (L387-L403), referencias (L429-L437), independencia (L439-L486), A1'-A8' (L500-L509) |
| `docs/automation/decisions/I-62.md` | `625a7075…` | §55-§60 (L1053-L1189), en particular §56 (OD-3, CLAUDE-CLI-I62), §57.3, §58.2, §59 y §60.2 |
| `docs/automation/evidence/I-62-evidence.md` | `d132626e…` | §84 (`claude-cli`), §93-§95 (bloque A2-P2, F6-OBS-03, evaluación) |
| `docs/automation/evidence/I-62-F6/FX-02/F6-OBS-03-cmd-route-evaluation.md` | `83a960ca…` | hechos, operaciones de VERIFY no demostradas y vías V1-V4 |
| `docs/automation/evidence/I-62-F6/OD-2/R20261009T041427Z-a2p2-block/result.json` | `b4a5679b…` | resultado literal de las dos sondas del bloque |
| `docs/automation/evidence/I-62-F6/kits/FX-02/controller-contracts.md` | `90929ad1…` | §3.2 (VERIFY: `Scope`, `AuthorityResolution`/`ClauseMapBlob`), §4 (receta), §7 (Architect; L210-L212, bases de aceptación e independencia frente a AUTHOR) |
| `docs/automation/evidence/I-62-F6/kits/FX-02/README.md` | `fd7ef02d…` | §0, L44 (contrato T1, `RoleRequirements`, `CorrectionsAuthorized`); L167 (N8); S18b, S21b, S30 (L140, L144, L180); §4, punto 6 (L243-L249) |
| `docs/automation/evidence/I-62-F6/kits/FX-02/order-FX-U1-O4.template.md` | `d92f7d3e…` | marcadores de la ventana (L28-L31), `{CORRECTION_RULE}` (L44), puntos 5-7 y 13-16 (L88-L124): dónde se transcribirían A4-3..A4-5 |
| `docs/automation/agent-execution/adapters/codex-cli.md` y `claude-cli.md` | `155f3469…`, `ae570380…` | operación 3 y `RuntimeShellFirstInPath`; estados de las operaciones de `claude-cli` (Q-A4-05) |
| `docs/automation/agent-execution/routing.md` y `model-catalog.md` | `bba08fc4…`, `166d978d…` | elegibilidad de una celda (§5) y requisitos por perfil (§8; L114, effort de la delegación); invalidadores |
| `docs/automation/evidence/I-62-claude-cli/R20261008T183600Z-char/README.md` | `0b682899…` | caracterización de `claude-cli` 2.1.293 |

La solicitud de desbloqueo de FX-02 que identifica H-1..H-3 (§2.1) está en el cierre como contexto, porque la orden vigente la incluye, pero A4-3..A4-5
citan directamente las cláusulas congeladas de esta tabla, con sus líneas (A-4 §2) (A62-A4-O10).

## 3. Resumen del delta

- **A4-1** (al final de V14 D.3, tras los párrafos de A-2). Si la invocación medida del Controller de la Topología A muestra que el runtime no
  usa el shell que la receta pone primero en el `PATH` y rechaza el suyo bajo solo lectura, la orden puede declarar un shell alternativo del host,
  sin tocar ningún otro elemento de la receta. La celda solo es elegible para PLAN y VERIFY tras **una** invocación medida adicional, de solo
  lectura, que demuestre las operaciones de VERIFY, incluidas las lecturas de Git de `Identity`, `Remote`, `CleanTree` y `Trailer`. Esa
  invocación necesita una disposición del Coordinator (trío exacto y directorio `-C`) y el consumo del Owner, y se cuenta fuera de la ronda, de
  «+ 2 sondas» y de A2-P2, con tope 1 en total para FX-02 en F6 (solo la regla 4 abre otra) y P-07. Sin reintento: si falla, FX-02 sigue
  UNVERIFIED con causa. Un invalidador cambiado la anula. Nada se amplía; P-01 y OD-2 sin cambio.
- **A4-2** (a continuación). Si la celda del Architect de la Topología A con el adapter fijado está medida BELOW_REQUIRED, una disposición del
  Coordinator puede vincularlo a otro adapter medido elegible para ARCHITECT en solo lectura. La independencia del contrato se evalúa tal como
  está (REQUIRED en SATISFIED; PREFERRED sin cumplir, registrado). Mismos topes, sin presupuesto nuevo para la columna del adapter, consumo
  autorizado por el Owner y ningún otro rol cambia.
- **A4-3** (H-1). La aceptación individual de los bindings del Worker y del Controller de verificación, transitorios entre Q0 y Q7 (B.8.3), puede
  decidirse antes del Q0 en la orden del Coordinator del fixture, con un marcador por binding que nombra unidad, tarea, ventana, rol, acción,
  adapter, celda (o lista cerrada) y comprobaciones (B1-B10, preflight vigente). Dentro de la ventana, sin escritura Git, el titular registra
  PENDING, aplica las comprobaciones y registra ACCEPTED (`INDIVIDUAL_DECISION`, `DecisionRef` a ese marcador) o REJECTED: sin Worker, T3'; sin
  verificador, cierre declarado. Custodia en el Q7 y reproducción por el validador; nada se amplía y B8 no cambia.
- **A4-4** (H-2). Para el Controller (PLAN y VERIFY) frente a WORKER y el Architect frente a AUTHOR, el UNKNOWN por NOT_STARTED no impide aceptar
  o materializar si las referencias están observadas, la receta crea una sesión nueva y el cierre de insumos está limpio; nunca se registra
  SATISFIED. Decide la Entrada, con la identidad observada de la corrida; sin SATISFIED, salida INVALID y el lanzamiento cuenta. Conjunto de
  referencia vacío (PLAN): SATISFIED con el rango vacío como evidencia. Regla 5: la excepción solo vale para el UNKNOWN discriminado de una fila de
  `Independence.Satisfaction` (`PreflightRef` con `Assurance` NONE e `InstanceId` NOT_STARTED, y (a)-(d) en la `Evidence`); la aplican el aceptante
  de A7', la aceptación individual real del binding del Architect por el Coordinator fuera de la ventana, el validador que reproduce la aceptación
  en el Q7 y S29. **(A) Opción B:** sin rama de materialización; el Architect, solo por decisión
  individual real del Coordinator fuera de la ventana. Enmienda de semántica de B6 y de 16.20-16.21 en la aceptación individual (M-02 y M-05); B.5,
  RAE §14.3, I-S18 y los validadores de F4, sin cambio.
- **(B) A4-6** (U-14, separable). La ronda A gana una sola sesión de Principal, solo para A2 (tope de la fila Principal A y total, de 2 a 3), con R
  contada y sin reinicios; ningún otro tope cambia; P-07 antes de abrir A2; `A4-PRINCIPAL-A2-CONSUMO`.
- **(C)** U-09 (e), U-16 (1) y CD-19/OQ-24: disposiciones de la orden O4, citadas en A-4 solo como contexto.
- **A4-5** (H-3, separable). Una corrección: su planificación y su verificación toman cada una 1 del pool de su fase (≤ 2 por fase), tope 15
  igual, P-07 antes del Q0 de la corrección. Si no se lanza, o sin A4-5, un REWORK de T1 deja FX-02 UNVERIFIED, no FAIL.
- **Ninguna otra cláusula cambia de texto.** Los nombres de producto solo aparecen en el motivo y en el anexo no normativo; A4-5 cita además la
  etiqueta congelada de la fila «Codex, total».

## 4. Preguntas para la revisión

1. ¿Implementan A4-1 y A4-2 exactamente lo que dispone el Coordinator en decisiones §60, punto 2 (DEC L1173)? En particular: ruta explícita y
   medida, sin suponer equivalencia ni cambiar en silencio la receta 16.4, y `claude-cli` para el Architect solo si el contrato lo permite y se
   cumplen independencia, capacidad y consumo.
2. ¿Es coherente con A-2 el cómputo de la invocación de A4-1 (aparte de la ronda, de «+ 2 sondas» y de A2-P2; tope 1; sin reintento), y evita una
   vía de mediciones ilimitadas? (Q-A4-01)
3. ¿Es completa y verificable, frente a `controller-contracts.md` §3.2, la lista de operaciones de VERIFY de la regla 2 de A4-1? ¿Es aceptable la
   cláusula de indisponibilidad declarada del mapa de cláusulas? (Q-A4-03)
4. A4-2: ¿es preciso el disparador (BELOW_REQUIRED con la identidad vigente; Q-A4-04)? ¿Conserva AUTOMATION_PLAN 16.21 la evaluación de la
   independencia «tal como está»? ¿Quedan bien definidos los totales y P-07 al contar en los mismos topes con la columna Claude CLI = 0 de la
   ronda A (V14 L2523)?
5. Q-A4-05: el descriptor materializado `claude-cli.md` declara UNVERIFIED sus operaciones 2-9. ¿Puede A4-2 heredar sin más la situación del
   Architect `claude-cli` de FX-03?
6. ¿Es correcta la materialidad (M-02, M-03, M-04 y M-05 sí; M-01 y M-06..M-08 no; A4-6, solo M-03 y M-04)? ¿Es OWNER-RESERVED el acuerdo
   mismo (Q-A4-09, Q-A4-21)? ¿Autorizan las tres líneas del Owner de A-4 §7 solo consumo, sin aceptar resultados por adelantado ni ampliar OD-3,
   OD-5 ni CLAUDE-CLI-I62?
7. Q-A4-02 y Q-A4-06..Q-A4-08 de A-4 §8.
8. A4-3 (Q-A4-10, Q-A4-11): ¿es la preautorización previa al Q0 una decisión individual que B9 admite, o una materialización encubierta frente a
   AUTOMATION_PLAN L1122-L1123 y B8? ¿Reproduce el validador del Q7 la aceptación solo con lo custodiado y el archivo de decisiones del Q0? ¿Queda
   fuera, con razón, la aceptación A1'-A8' de `d` (T3)?
9. A4-4 (Q-A4-12, Q-A4-13): ¿es coherente con 16.21 y con I-S18 trasladar la decisión a la Entrada sin registrar nunca SATISFIED antes de la
   corrida? ¿Son suficientes y comprobables las condiciones (a)-(d)? ¿Está justificada la lectura del conjunto vacío y la inclusión del Architect?
10. A4-5 (Q-A4-14): ¿es aceptable tomar del pool de reintentos la planificación y la verificación de la corrección, o la revisión recomienda
    excluir A4-5? En cualquier caso, ¿es correcta la clasificación UNVERIFIED (no FAIL) de un REWORK sin corrección (D.4)?
11. Re-revisión 2 (Q-A4-15..Q-A4-17): dispuesta en la revisión R20261009T150948Z-fdc2 (A62-A4-01 CLOSED; A62-A4-02 REQUIRED).
12. Re-revisión de la corrección 3 (Q-A4-18..Q-A4-22): **(A)** ¿cierra la opción B A62-A4-02, con I-S18 y los demás textos en su significado
    literal, y basta la frase añadida a la regla 5 para la aceptación individual del Architect (Q-A4-18, cerrada en la corrección 3)?
    **(B)** ¿es correcto y suficiente el delta de A4-6 (una sola sesión, solo para A2, justificada por F6-OBS-01), sin cambiar ningún otro
    tope? ¿Son separables A4-5 y A4-6?
    **(C)** ¿quedan fuera de A-4 las disposiciones ordinarias de FX-02? ¿Son O9 y O10 precisiones sin ampliar el cambio normativo?

## 5. Condiciones de la invocación (no se lanza con este paquete)

- **Autoridad:** una disposición del Coordinator que autorice la re-revisión formal sobre el blob exacto publicado. A-3 AGREED (decisiones §61).
  Corrección 3 por decisiones §63, punto 6.
- **Transporte preferido:** `claude-cli` 2.1.293 (SHA-256 `8693c4a0…`, evidencia §90), elegible para ARCHITECT (decisiones §57, punto 3). Su
  consumo para revisiones reales de I-62 lo cubre CLAUDE-CLI-I62 = A (DEC L1085). Antes del lanzamiento se vuelven a comprobar el binario, la
  versión, la autenticación (sin leer credenciales), el modelo y el effort. Con un invalidador cambiado, la celda queda STALE: nueva
  caracterización o HUMAN_LAUNCH_REQUIRED.
- **Independencia (OD-6 alternativa 1):** Actor, Session y Context REQUIRED; Provider PREFERRED. El autor es la sesión principal, del mismo
  proveedor, y eso se declara. El revisor es un proceso nuevo, sin la transcripción del autor ni del Coordinator y sin memoria de otras sesiones.
- **Ejecución:** clon limpio del commit de publicación; solo Read, Grep y Glob; cierre de insumos = §2. El kit, el auditor y el preflight se
  custodian antes del lanzamiento y no se amplían después. Una sola invocación.

## 6. Lo que este paquete no hace

- no aplica A-4: ningún texto congelado, materializado ni de producción cambia;
- no autoriza la invocación de A4-1, la sustitución de A4-2, la sesión adicional de A4-6 ni ningún consumo;
- no publica ninguna preautorización de A4-3 ni cambia la orden O4 ni el kit de FX-02;
- no declara veredictos ni lanza la revisión;
- no modifica A-1, A-2, A-3, el Freeze ni ADR-0048.

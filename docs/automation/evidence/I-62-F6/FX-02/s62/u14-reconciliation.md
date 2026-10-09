# U-14 — Conciliación de las sesiones físicas del fixture y presupuesto de la ronda A (decisiones §62, punto 3)

> Preparación de la supervisión (plano a), una sola pasada, 2026-10-09. Nada se decide aquí y no se recomienda ninguna lectura.
>
> Fuentes (worktree `architecture-portabilidad-coordinador-principal`, HEAD `cca8c60e`):
> - decisiones §46-§62;
> - evidencia §60-§105, más §106 (TRX y `.gitignore`, commit `a39a1ada`, posterior a `cca8c60e`), solo como contexto;
> - V14 D.3 y D.4 y §18 (OD-5);
> - A-2 (`f1e1d6f0`) §2.3 y §3.3;
> - kit de FX-02, README §4;
> - solicitud de desbloqueo (fila U-14) y clasificación (fila #19);
> - Git de solo lectura sobre `D:\r62-fixture\fixture-origin.git` (`fx/u1`).
>
> Mandato (§62.3): «**U-14: PENDING:** presentar la conciliación de las sesiones físicas, su clasificación y el presupuesto de la ronda A; R no se
> excluye del tope solo por haber sido una reparación».

## 1. Texto aplicable (literal)

**Fila Principal A de V14 D.3, Topología A (FX-01, FX-02, FX-05):** «Principal A (`claude-desktop-session`, sesión del sistema bajo prueba abierta
por el Owner) | toda la ronda | 1 sesión | … | +1 reapertura | 2 sesiones».

**Resto de V14 D.3:**
- Totales: «| A | ≤ 15 + 2 sondas | ≤ 4 | 0 | ≤ 2 |» (la última columna es «Sesiones de Principal»).
- «P-07 se comprueba **antes** de cada lanzamiento».
- «Ningún tope autoriza gasto ni ejecución ahora».

**OD-5 (decisiones §46):** «apertura de las sesiones del sistema bajo prueba; consumo dentro de los topes de D.3».

**A-2, A2-P1 (AGREED, §57):**
- Regla 3: «su lanzamiento físico queda registrado y cuenta en el tope de su fila y, si esta hoja fija un total de sesiones de Principal para su
  ronda, en ese total».
- Regla 4: «Si la sesión invalidada sirve a varios escenarios (Principal A de la Topología A, para FX-01, FX-02 y FX-05; …)». También: «la
  aprobación de OD-5 (consumo dentro de los topes de D.3) no lo cubre».

**A-2, A2-P2, regla 1:** «las sondas anteriores, dentro o por encima del tope de la fila, quedan como evidencia histórica y siguen contadas; nunca se
descuentan ni amplían ningún tope».

**Decisiones §55:**
- Punto 2: «D.3 fija para el Principal B de FX-04a 1 sesión base + 1 reapertura = 2, consumidas por B1 y B2; … una tercera sesión general aumentaría
  un presupuesto congelado de F6 y no cabe en una disposición ordinaria: hace falta una enmienda posterior al Freeze».
- Punto 7: «MATERIAL POST-FREEZE AMENDMENT (cambia presupuestos congelados de lanzamiento y de sondas de F6); autoridad: Coordinator + acuerdo de un
  Architect independiente …; el Owner solo para consumo».
- Punto 11: «las autorizaciones previas del Owner no aumentan el tope congelado».

**Decisiones §51:**
- «Designación acotada del Coordinator de un **Principal R temporal**, distinto de A y del B de FX-04a, con binding, preflight y aceptación
  normales».
- F6-OBS-01: «DEFECTO NO MATERIAL …; sin A-n, sin cambio del Freeze ni de A-1».

**Decisiones §52:** «terminación de R ACREDITADA».

**Decisiones §58, `B3-CONSUMO`:** «… por encima del tope de D.3 que cubre OD-5 …».

## 2. Inventario de las sesiones e invocaciones físicas del fixture (plano c)

### 2.1 Sesiones de Principal (`claude-desktop-session`, `claude-opus-5-5`; todas abiertas por el Owner)

| # | Sesión | Rol / acción | Escenario y ronda | Directorio, instantes | Fila de D.3 en que cuenta | Autoridad que la creó | Clasificación |
|---|---|---|---|---|---|---|---|
| P1 | **A** `local_6dde7eb6…` | PRINCIPAL_COORDINATOR, CUSTODY | Topología A: FX-01 (4 preflights dentro de la sesión), D.1 pasos 5-6 (BOOTSTRAP, QU, T1) y QH `ae25b596` (paso 1 de FX-04a, hecho por A) | `D:\r62-fixture\A`; preflight 1 a las 2026-10-06T20:31:45Z; terminada (`isRunning` = false desde las 23:17:18Z; ev. §69) | Principal A, «1 sesión» (base); total de la ronda A | OD-5 (§46); §49 («El Owner abre A según la receta congelada»); §50 (FX-01) | válida; FX-01/C-23 y C-22 PASS (§51); su QH `ae25b596` se conserva como inválido (I-S13, F6-OBS-01, no material) |
| P2 | **R** `local_b70524e3…` | PRINCIPAL_COORDINATOR, CUSTODY (titular temporal de reparación) | Reparación de FX-U1 entre FX-01 y FX-04a: QR r4 `ead6119f` y QH2 r5 `cabed547` (el punto de partida de FX-04a) | `D:\r62-fixture\R`; observación `P20261007T023103Z-01b1`; terminada (03:38:55Z; ev. §71) | **la que fije U-14** (§3): ninguna fila de D.3 la nombra | §51 (método de reparación); orden FX-U1-O3 `cc21e1d2`; designación `1d2f14bf` (+ fe de erratas `f4f5929f`); OD-5 en cuanto «apertura … consumo dentro de los topes de D.3» | válida; QR y QH2 VÁLIDOS; terminación ACREDITADA (§52) |
| P3 | **B1** `local_1a28830f…` | PRINCIPAL_COORDINATOR, RESUME_DECISION | FX-04a | `D:\r62-fixture\B`, 2026-10-07T04:52:28Z (ev. §72) | FX-04a, Principal B, «1 sesión» (base) | OD-5; §52 | INVALID_TEST_ORACLE (§53); se conserva |
| P4 | **B2** `local_97ac0196…` | ídem | FX-04a | `D:\r62-fixture\B2`, 2026-10-07T20:06:01Z (ev. §78) | FX-04a, Principal B, «+1 reapertura» (§55.2: «consumidas por B1 y B2»); **no** la fila «N11: B2» | OD-5; §53 («Segunda sesión limpia (B2)») | INVALID_LAUNCH (§55.1); FAIL bruto 2/23 conservado |
| P5 | **B3** `local_51e9c8c0…` | ídem | FX-04a | `D:\r62-fixture\B3`, 2026-10-09T03:58:12Z (ev. §91) | FX-04a, Principal B, tope 2 → 3 por A2-P1 (regla 4) | A-2 AGREED (§57); §58.1 `B3 = AUTORIZADA`; `B3-CONSUMO = A` (Owner, «por encima del tope de D.3 que cubre OD-5») | válida; FX-04a/C-25a PASS (§60) |

### 2.2 Invocaciones de modelo de `codex-cli` en el fixture (todas lanzadas por la supervisión)

| # | Invocación | Celda y modo | Directorio, binario | Fila de D.3 en que cuenta | Autoridad | Clasificación |
|---|---|---|---|---|---|---|
| C1 | OD-4, sonda 1 `R20261006T072306Z-od41` | `gpt-6-luna/high`, `workspace-write` | `D:\r62-fixture\probe-od4-1`; `37762753…`, 0.160.0 | Topología B, «Sondas previas: … escritura de `codex-cli` 1 (+1)», «4 (fuera de la ronda)» | OD-4 = A (§46: «≤ 2 sondas de escritura antes de los escenarios») | medida: commit UNSUPPORTED; reescribió `config.toml` → P-01 (ev. §62). La 2.ª no se lanzó |
| C2, C3 | OD-2b-PROBE `R20261006T150704Z-od2b`: sondas 1 y 2 | Controller `gpt-6-luna/high` y Architect `gpt-6.1-sol/high`, `read-only` | `A` y `arch`; `37762753…`, 0.160.0 | Topología A, «Sondas previas … 2 (fuera de la ronda)»; total «+ 2 sondas». **Agotan el tope** (A-2 §3.2) | §47, OD-2b-PROBE = A del Owner | válidas; huella estable (ev. §64) |
| C4, C5 | OD-2d-PROBE `R20261007T013200Z-od2d-probe`: sondas 1 y 2 | ídem | `A` y `arch`; `3b8f6e33…`, 0.160.1 | «Sondas previas», **por encima del tope**: «históricas y contadas» (A2-P2, regla 1; A-2 §3.2; §55.11) | §51, OD-2d-PROBE = A del Owner | válidas; la identidad quedó obsoleta tras la actualización (ev. §71) |
| C6, C7 | bloque A2-P2 `R20261009T041427Z-a2p2-block`: sondas 1 (8/8 vía `cmd.exe`) y 2 (0/6) | ídem | `A` y `arch`; `3553cd6e…`, 0.162.0-alpha.2 | ninguna fila: «no consumen los lanzamientos de Codex de ninguna ronda ni el sumando «+ 2 sondas»» (A-2 §3.3). Bloque del par: 2/2 | A-2 AGREED; §58.2 `BLOQUE-CODEX`; `BLOQUE-CODEX-CONSUMO = A` | válidas como medición; F6-OBS-03 (ev. §93) |

### 2.3 Ejecuciones sin invocación de modelo (ninguna fila de D.3: las filas cuentan sesiones y lanzamientos de modelo)

| # | Qué | Autoridad | Nota |
|---|---|---|---|
| N1 | `codex --version` y `codex login status` en OD-4 (cfg tras `version`), OD-2b-PROBE, OD-2d-PROBE y A2-P2 | las de C1-C7 (§51 las nombra aparte de las sondas de modelo) | huella antes y después de cada una |
| N2 | FX-05, corridas 1 (inválida por el detector) y 2 (PASS) | §46-§47 | D.3, fila FX-05: «0»; «no exige una invocación de modelo» (§47) |
| N3 | Actos del Coordinator del fixture (lo hace la supervisión): órdenes O1-O3, G0, contrato de T1, designación de R, commits de U-07 y U-08 (`b6d294e`, `c785def`) | §46, §51, §62 | no son sesiones del sistema bajo prueba |
| N4 | Validadores de F4, oráculos y comparaciones de FX-04a, comprobaciones D.6 y mediciones pasivas | — | no son invocaciones |

**Subagentes (`claude-subagent`) en el fixture:** ninguno lanzado. El Worker y el Reviewer de FX-02 no han empezado. Las auditorías de B1, B2 y B3
solo registran `get_session("self")`. Para A y R, el kit de FX-02 dice «0 conocidos» y lo confirma la supervisión sobre sus transcripciones; aquí no
se reverifica.

**Fuera del plano c (no son del fixture; ninguna fila de D.3):**
- la caracterización de `claude-cli` (ev. §84; directorios `D:/r62-c3-probe…`);
- las revisiones del Architect de A-2 (sesión de escritorio `local_b7dce5ad…` en `D:\r62-arch-a2`; `claude-cli` en `D:\r62-arch-a2r`), A-3 (intentos 1
  y 2, `D:\r62-arch-a3`) y A-4 (`D:\r62-arch-a4`).

Son de I-62 real, bajo CLAUDE-CLI-I62 = A o tarea del Owner.

## 3. Presupuesto de Principal de la ronda A: tres lecturas

Hechos comunes:
- Tope de la fila y de la ronda: **2**. A consumió **1**.
- A2 sería un titular nuevo por T16 (designación), no una reapertura literal de la sesión de A.
- En el único precedente congelado de una fila de Principal (§55.2), la segunda sesión física (B2, nueva y sin relación de transporte con B1)
  ocupó el «+1 reapertura».

| Lectura | Cláusulas en que se apoya | Tensión con el texto o las decisiones | Consumo de la ronda A | Restante | ¿Cabe A2? |
|---|---|---|---|---|---|
| **(i) R cuenta como sesión de Principal de la ronda A** | Totales de D.3, que cuentan «Sesiones de Principal» sin excepción por causa. A2-P1 regla 3: el lanzamiento físico cuenta en la fila y en el total. §55.2: la 2.ª sesión física ocupa el «+1» aunque no sea una reapertura de transporte. Función: el paso 1 de FX-04a, que D.3 atribuye a «A», lo hizo R (QH2), y R fue el titular de FX-U1 en la topología A. OD-5 cubre R solo si R cabe en un tope de D.3, y §51 dice «sin cambio del Freeze». §62.3: «R no se excluye … solo por haber sido una reparación» | R no sirvió a ningún paso de FX-01, FX-02 ni FX-05, que es el alcance de «toda la ronda» de la fila (A2-P1 regla 4 enumera FX-01, FX-02 y FX-05) | A 1 + R 1 = **2/2** | **0** | **No.** A2 sería la 3.ª y P-07 la bloquearía en S03b |
| **(ii) R cuenta en otro presupuesto que fijen el texto congelado o una decisión** | Ninguna cláusula ni decisión §46-§62 asigna a R un presupuesto. Candidatas revisadas: la ronda FX-04a (R preparó su QH2), la fila «N11: B2», la topología B y D.8 (FX-06) | FX-04a: sus filas son «Principal B» y «N11: B2», y esta está «acotada a N11, no es intercambiable» (§55.2). Con R, FX-04a quedaría en 4 frente a un tope de 3 (2 + 1 de A2-P1), por encima de su tope de forma retroactiva. La topología B es de `codex-desktop-session`. D.8 es de FX-06 | A **1/2** | 1 | **Sí**, como 2.ª y última (sin reapertura). Pero hoy no hay ningún presupuesto que la sostenga: crearlo cambia un presupuesto congelado (§55.7) |
| **(iii) R no cuenta en ninguna parte** | Solicitud de desbloqueo, fila U-14: R fue un titular de reparación de §51 para un defecto NO MATERIAL («sin A-n, sin cambio del Freeze»), distinto de A y de B, y no sirvió a FX-01, FX-02 ni FX-05. Ninguna cláusula lo dice de forma expresa: sería una interpretación del Coordinator (LIFECYCLE §10) sobre el alcance de «ronda» | §62.3 no admite excluirla solo por ser una reparación. Los totales de D.3 no tienen excepción. Los principios de conteo de A-2 (regla 3 de A2-P1; regla 1 de A2-P2, «dentro o por encima del tope … siguen contadas») y §55.11 (una autorización no amplía el tope) van en contra. Además, deja el consumo de R **fuera de todo tope de D.3**: OD-5 solo cubre «consumo dentro de los topes», y para R no hay una línea del Owner (el precedente `B3-CONSUMO` sí la tuvo) | A **1/2** | 1 | **Sí**, como 2.ª y última (sin reapertura; P-07 en S03b) |

Con cualquier lectura en que A2 quepa, A2 es la última sesión de Principal de la ronda A. Si A2 se acreditara INVALID_LAUNCH, la única válvula
sería A2-P1: una reejecución extraordinaria por escenario, con disposición del Coordinator y consumo del Owner. FX-01 y FX-05 ya son PASS, así que
A2 solo serviría a FX-02.

## 4. Si rige (i): autoridad necesaria y opciones (redacción neutral)

Con (i), A2 no cabe. Por la práctica fijada en §55.2 y §55.7, una sesión más de Principal en una ronda cambia un presupuesto congelado de F6. Eso
pide:
1. **una enmienda material posterior al Freeze**, con Coordinator y un Architect independiente (LIFECYCLE §6), y
2. **una autorización de consumo del Owner por encima de OD-5** (A2-P1 regla 4: «la aprobación de OD-5 … no lo cubre»; precedente `B3-CONSUMO`).

Ninguna de las dos basta sola:
- §55.2: una sesión más «no cabe en una disposición ordinaria»;
- §55.7: «el Owner solo para consumo».

**No disponible:**
- **A2-P1.** Exige que una sesión de Principal de la ronda se acredite INVALID_LAUNCH, y A y R son válidas.
- **A-4 tal como está.** Su §3.7 dice «No sube topes ni totales de D.3».

| Opción | Contenido | Autoridad | Coste o efecto |
|---|---|---|---|
| **α1** | A-n nueva y estrecha: el tope de la fila Principal A y el total de la ronda A suben en uno, solo para FX-02 en F6 (R sigue contada en la ronda A) | Coordinator + Architect independiente; consumo del Owner para A2 | una revisión formal más. Hasta el AGREED, ni S03b ni S04 |
| **α2** | A-n nueva y estrecha: un tope propio (1) para un titular temporal de reparación designado por el Coordinator ante un defecto no material de custodia, fuera de los totales de las rondas. R pasaría a contar ahí y el tope de la ronda A no cambia | ídem, más la cobertura del consumo de R ya ocurrido (consumo del Owner, ratificado o por encima de OD-5) | da a (ii) una base por enmienda. Es la que más cambia: crea una categoría nueva de presupuesto |
| **β** | Las mismas reglas de α1 o α2 como corrección 3 de A-4 | ídem | §62.4: «No modificar el objeto que examina el Architect». Solo cabe tras la re-revisión en curso, y obliga a otra (con guardas). Choca con el «No sube topes» de A-4 §3.7 |
| **γ** | Sin autoridad nueva: A2 no se abre en la ronda A | — | FX-02 queda UNVERIFIED (D.4: «falta una precondición») y F6 pendiente (la matriz de D.4 exige FX-02 PASS) |

**Forma** de la línea del Owner para α1, α2 o β, calcada de `B3-CONSUMO`. Es solo una forma, no un texto para pegar: el exacto saldría de la A-n
acordada.

```text
<ID>-CONSUMO = A (una sesión de Principal A2, claude-desktop-session, claude-opus-5-5 <effort>, abierta por el Owner en D:\r62-fixture\A2, por encima del tope de D.3 que cubre OD-5, bajo <A-n> AGREED; texto inicial pegado desde <archivo>; ninguna otra sesión)
```

## 5. Resto del presupuesto de la ronda A (estado en `cca8c60e`)

| Contador (V14 D.3, topología A) | Tope | Consumido | Nota |
|---|---|---|---|
| Sesiones de Principal | ≤ 2 | 1 (A) + R según U-14 | §3 |
| Codex, lanzamientos base | 11 | 0 | `counters.invocations` vacío en QH2; ninguna invocación de rol de FX-02 |
| Codex, pool de reintentos | 4 (≤ 2 por fase) | 0 | — |
| Codex, tope de la ronda | 15 | 0 | con A4-2, el Architect sustituto cuenta aquí (A-4 §3.3, regla 3) |
| Sondas previas | 2 (fuera de la ronda) | 2 + 2 por encima del tope | agotado (§55.11); A2-P2 se cuenta aparte (2/2 para el par vigente) |
| Subagentes de Claude | ≤ 4 | 0 | Worker 2 y Reviewer 2 sin lanzar (U-12 propone retirar el Reviewer) |
| Claude CLI | 0 | 0 | A4-2 no le da presupuesto propio (A-4 §4) |
| Invocación de A4-1 | 1 en total (A-4 `0d954376`, regla 3), fuera de la ronda | 0 | no autorizada |

## 6. Para la disposición

- U-14 bloquea S03b (P-07) y S04. La clasificación (#19) la pide **antes del veredicto de A-4**, porque con (i) A-4 no basta y hace falta otra
  autoridad.
- La afirmación de la solicitud de desbloqueo «A2 = 2.ª y última» solo vale con (ii) o (iii).
- Si se elige (iii), la disposición tendría que dejar registrado cómo queda cubierto el consumo de R, que bajo esa lectura está fuera de los topes que
  cubre OD-5.

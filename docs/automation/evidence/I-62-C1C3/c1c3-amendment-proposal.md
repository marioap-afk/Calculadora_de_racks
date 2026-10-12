# I-62 — Propuesta de una enmienda independiente para C1-C3 (sin redactar)

```text
Estado:        PROPUESTA DE ENMIENDA (alcance, deltas por superficie, dependencias y preguntas).
               NO es el texto de una enmienda: no contiene cláusulas ni párrafos de reglas (§81.4).
               No reclama número A-n. No se somete a revisión formal sin una disposición nueva.
Nombre provisional: «I-62 — Aislamiento del Worker: checkouts, directorio de proceso e intérpretes»
Independencia: separada de A-6. No amplía A-6, no toca sus superficies ni su revisión, ni la recuperación de FX-02 (§81.4-§81.5).
Base:          diseño c1c3-design.md (este carril); rama de I-62 en 2c531b533181a9efb3bba351c09e657e0696bbe1;
               Freeze: Proposal V14 (blob 34ad80ea…) + registro de Freeze (blob 0f6b8608…); A-1..A-5 acordadas.
Materialidad:  MATERIAL — M-02..M-06 activados; M-07 y M-08 UNKNOWN (cuentan como activados); M-01 no (diseño §9).
Applies-to (propuesto): unidades I62 desde su vigencia; ninguna unidad I61; I-61 sin cambios.
IMPLEMENTATION AUTHORIZATION = NO.
```

## 1. Motivo

El incidente de I-65 (paquete de escalamiento, blob `118673e9…`) mostró tres modos de fallo que los controles del relevo no ven por
construcción: escrituras del Worker en el checkout principal (F4-T4, con consecuencias; F4-T5, transitoria), resolución de rutas relativas
contra un directorio de proceso que no es el worktree, y siete sondas de intérprete pese a la prohibición en el prompt. El diseño (§9) concluye
que cerrarlos con controles mecánicos cambia esquemas, comportamiento observable, semántica de fallo, contratos consumidos y la frontera de
adapters del Freeze: hace falta una enmienda material.

## 2. Alcance propuesto

**Dentro:**
- C1 — fotografía de todos los checkouts en la Salida y la Entrada de cada cesión, con disposición por rol y clase, y canario por cesión;
- C2 — directorio de proceso del Worker fijado por el transporte, canario antes de ceder y hecho de directorio por orden;
- C3 — ejecutables prohibidos no invocables en el entorno del Worker, canario por cesión, registro del guardián y su clasificación propia frente a
  `Denials`;
- la pieza mínima de clasificación de desviaciones que C3 necesita (intento bloqueado = clase A, sin STOP; ejecución = clase B, STOP).

**Fuera:**
- A-6, FX-02, B-min y la revisión de A-6;
- cambios en I-61, en unidades I61 o en I-65;
- C4 (barrido general de transcripciones), C7 (clasificación general), C8 (oráculo de nc1) y C9 (prompt), salvo la pieza mínima citada;
- cambios del host: alias de ejecución de aplicaciones, `PATH` del sistema, permisos, ACL, auditoría de procesos, lanzadores compartidos.

**Separables, solo si una disposición amplía el alcance (§81 autorizó C1-C3):**
- C5 — `Scope` mecánico como señal sesión → Controller (nc2: DEV-I65-F4-08, I-63 DEV-G2-01);
- C6 — aceptación A5 por igualdad de texto y rechazo de caracteres de control (DEV-I65-F4-11, CD-I65-F4-19).

## 3. Deltas que necesitaría (por superficie; una línea cada uno; sin texto de reglas)

| # | Superficie | Delta |
|---|---|---|
| Δ-1 | AUTOMATION_PLAN §16 (subsección I62 nueva, añadida) | reglas de C1-C3: hechos de Salida/Entrada, disposición rol × clase, canarios, STOP de Entrada y antes de invocar, ids P nuevos |
| Δ-2 | AUTOMATION_PLAN 16.18 | la lista de comprobaciones de relevo incluye las fotografías y los canarios |
| Δ-3 | AUTOMATION_PLAN 16.19 | capacidad nueva del adapter: fijar el directorio de proceso y el entorno del hijo |
| Δ-4 | AUTOMATION_PLAN 16.20 | el binding de WORKER exige esa capacidad en DISPONIBLE |
| Δ-5 | AUTOMATION_PLAN 16.22 | lectura I62 de `Denials`: excluye solo los bloqueos emparejados con el registro del guardián |
| Δ-6 | AUTOMATION_PLAN 16.26 | T10 (c) se apoya en la diferencia de C1 |
| Δ-7 | README §3.1 y §3.4 | procedimiento de fotografía (con `--no-optional-locks`), canarios y hechos |
| Δ-8 | README §10 | K-C1, K-C2, K-C3 y los controles negativos NC-C1/NC-C3 con oráculo relativo |
| Δ-9 | README §13 y §14.7 | preflight con las capacidades; A1'-A8' con `ExecutionEnvironment` |
| Δ-10 | README §17 | el diario de la ventana lleva los hechos nuevos |
| Δ-11 | `relay-record/v2` | campos nuevos en `Exit`/`Entry` (fotografías, diferencia, directorio, guardián, eventos) |
| Δ-12 | `gate-contract/v2` y `delegation/v2` | bloque `ExecutionEnvironment` y su inclusión en A3'-A5' |
| Δ-13 | `adapters/*.facts.v1` y `preflight/v1` | hechos de las capacidades de directorio y entorno |
| Δ-14 | descriptores `claude-subagent`, `codex-cli`, `claude-cli` | estado de la capacidad nueva por transporte |
| Δ-15 | `role-invocation/v1` | solo si la invocación de rol transporta el entorno (pregunta Q-07) |
| Δ-16 | mapa de cláusulas I62 | regeneración y validación MV-1..MV-7 |
| Δ-17 | instrumentos operativos (fotografía, shims, gancho opcional, canarios, validador) | ubicación y custodia por blob; dependen de D-2 |
| Δ-18 (separable) | AUTOMATION_PLAN 16.9 #7 / 16.22 y README §10 nc2 | C5 |
| Δ-19 (separable) | AUTOMATION_PLAN 16.5 A5 / 16.22 A3'-A5' | C6 |

Sin cambios previstos: `controller-verification/v2` (forma), 16.4 (texto de I-61), PROMPT_TEMPLATES §G (normativo), ADR-0046, ADR-0048.

## 4. Dependencias

| Id | Dependencia | Efecto |
|---|---|---|
| D-1 | disposición nueva del Owner y del Coordinator que abra la enmienda | sin ella no se redacta ni se revisa (§81.4) |
| D-2 | determinación OWNER-RESERVED sobre ADR-0046 #9 para instrumentos operativos generales (precedente §77-§78, limitado y sin precedente) | sin ella, C1 y C3 se quedan en procedimiento manual |
| D-3 | no interferencia con A-6: la enmienda no se aplica a FX-02 ni a F6 mientras A-6 y la recuperación de FX-02 sigan abiertas, salvo decisión expresa | evita cambiar la elegibilidad del WORKER a mitad de un piloto |
| D-4 | medición previa de lo INFERENCE (diseño §13, paso 1) | sin medir, C2-D2 y C3-P3 quedan UNVERIFIED |
| D-5 | decisiones del Owner sobre el host: abrir la sesión Principal en el worktree, entorno de lanzamiento, ganchos de sesión | sin ellas, el WORKER en `claude-subagent` no es elegible (o se acepta la limitación) |
| D-6 | transportes de proceso externo: OD-3 (autenticación de Claude CLI) y OD-4 (sandbox de escritura de Codex) | condicionan la vía C2-P1/C3-P1 para Workers |
| D-7 | punto efectivo de I-62 (16.14) | las reglas solo rigen para unidades I62 después de él |
| D-8 | precondición o atestación del Owner sobre su checkout durante las cesiones | define el trato de un STOP de C1 por actividad del Owner |

## 5. Preguntas abiertas

| Id | Pregunta |
|---|---|
| Q-01 | ¿C1 se aplica como STOP de Entrada de la sesión (patrón P-01/P-11, propuesto) o como comprobación del Controller (descartado por cambiar `controller-verification/v2` y los oráculos)? |
| Q-02 | Hermanos concurrentes: ¿basta la declaración del Coordinator en la Salida y su atribución antes de aceptar un VERIFIED, o todo cambio en un hermano debe ser STOP? |
| Q-03 | ¿El modo profundo (hash de todos los rastreados o `ChangeTime`) es obligatorio para el checkout principal o queda opcional? |
| Q-04 | ¿Qué lista de ruido declara el contrato por defecto, y quién la mantiene? |
| Q-05 | `claude-subagent`: ¿se exige abrir la sesión Principal en el worktree, se declara el WORKER no elegible en ese transporte, o se acepta una limitación registrada? ¿Cómo se concilia con P-08 durante cesiones a procesos externos? |
| Q-06 | ¿Entra en la enmienda el guardián de rutas de las herramientas de archivo (C2-P4), que habría impedido F4-T5? |
| Q-07 | ¿El entorno del Worker pertenece al contrato de gate, a la delegación, a la invocación de rol o a las tres? |
| Q-08 | ¿La lista `ForbiddenExecutables` por defecto se limita a la familia Python, o el contrato debe enumerar otros (`perl` de Git Bash, Windows Script Host)? |
| Q-09 | ¿Se admite el gancho de texto como mecanismo (M-07 UNKNOWN), o solo filtro de `PATH` + shims fijados por el transporte? |
| Q-10 | ¿C5 y C6 entran como deltas separables de esta enmienda o como otra enmienda? |
| Q-11 | ¿La pieza mínima de clasificación de C3 (intento bloqueado = clase A) es aceptable sin abordar C7 completo? |
| Q-12 | ¿Qué parte puede adoptar I-65 como contención local antes de la vigencia de I-62 sin crear una segunda autoridad sobre el mismo procedimiento (M-01)? |

## 6. Lo que la próxima disposición tendría que decidir

1. Si se abre la enmienda (D-1) y con qué alcance (C1-C3, y si C5/C6 entran como separables).
2. La determinación sobre ADR-0046 #9 (D-2) y las decisiones del host (D-5, D-8), que son OWNER-RESERVED.
3. La secuencia respecto de A-6 y FX-02 (D-3).
4. La autorización de la medición previa (D-4).

Nada de esto declara C1-C3 implementados ni I-65 desbloqueada.

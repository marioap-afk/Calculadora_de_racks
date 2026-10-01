# I-62 — Paquete de revisión (Proposal V3)

```text
PROPOSAL V3 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED (V1 y V2: CHANGES REQUIRED; registros I-62-coordinator-review-v1.md y -v2.md)
Architect      = NOT REVIEWED (sin revisor asignado en este paquete; no se simula)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V3 §11.4, dos alternativas delimitadas); sin decisión, no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v3.md            blob 36b13443b3d8cdc21fe731b4a5ab263cc9fd7fb9
  docs/initiatives/I-62-coordinator-review-v2.md  blob b431510af260da6f57b2c347f3378e3f1a52f6cb
Versiones anteriores revisadas:
  V1: commit ea0555913202efc96e1007f021942579b989b43d, blob 31e2f4afa895dc03f7621504e150de0539a2302e
  V2: commit a1f5e0035f0e109d02a336c0a01917e53a53b88a, blob 47a643ddf1f22226072ede67a862133f77441e2e
Discovery base: docs/initiatives/I-62-discovery.md, R1 (blob 86f24e65…, commit 2b7976b0), con §21 añadida en ea055591
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (solución de R62-V1-09, cerrada y preservada). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v3.md` = `36b13443…`. Si no coincide, revisa la versión designada o rechaza la discordancia; nunca
> aprueba el último archivo encontrado.

> **Qué es.** La versión completa, el delta V2→V3 y la disposición por ID. La sesión autora no emite veredictos ni declara consenso o independencia. **Los
> contraejemplos y trazas de V3 (anexos E.3, F y G.2) son análisis del diseño, no ensayos ejecutados.**

## 1. Veredicto que se solicita (LIFECYCLE §5)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
           AGREED solo con cero REQUIRED abiertos sobre la versión exacta.
Hallazgos: REQUIRED | OPTIONAL (las notas no sustituyen estas clases); ID, sección, evidencia, cambio exigido.
Modo:      SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN; si revisor y autor son la misma persona.
Identidad: commit, ruta y blob revisados.
```

Solo quien emitió un REQUIRED puede rebajarlo.

## 2. Lectura

1. [Proposal V3](I-62-proposal-v3.md), completa, con los anexos:
   - A: ADR sucesor;
   - B: contratos;
   - C: matriz única;
   - D: pilotos;
   - E: compatibilidad legacy;
   - F: secuencias;
   - G: cambios frente a `/v1`.
2. Registros de revisión del Coordinator: [V1](I-62-coordinator-review-v1.md) y [V2](I-62-coordinator-review-v2.md).
3. [Discovery R1](I-62-discovery.md) §§1-3, 9-13 y 21.
4. [Decisiones](../automation/decisions/I-62.md) §§7, 9-12.
5. Fuentes integradas: AUTOMATION_PLAN §§8 y 16; ADR-0046; Freeze de I-61 ([Proposal V9](I-61-proposal-v9.md)); `routing.md`, `model-catalog.md` y el README
   de agent-execution; LIFECYCLE §§2-9; WORKFLOW §§3-4, 10 y 11.

## 3. Delta V2 → V3

| Área | V2 | V3 |
|---|---|---|
| Adopción legacy | lectura de §16 en `^1`; duda → I61; `protocol` presupuesto | mapa de compatibilidad por superficie y cláusula (Anexo E); clases ANTERIOR / POSTERIOR / DESCONOCIDA (esta última sin default y con STOP del paso dependiente); unidades sin metadata leídas por PRE; punto efectivo anclado y único; sin efecto retroactivo; I-52 conserva su régimen (§14) |
| Custodia y exact-SHA | «toda transición es un commit»; T3 rompía `Identity` | puntos durables Q0/Q7/QR solo donde 16.4 permite escribir; diario transitorio encadenado (`PrevRelaySha256`) entre ellos; `Identity` sin cambio; SHAs distinguidos; push rechazado con causas separadas; el perdedor de la recuperación no opera; T10 en cuatro casos (§8, §9.2, Anexo F) |
| Preflight frente a binding | el preflight se invalidaba por el binding o el SHA | observación de capacidad (invalidadores de runtime) separada de las comprobaciones de relevo (16.4); el binding consume la observación (§6) |
| Identidad | Actor = `BindingId` | `ActorRef` y `SessionRef` observables; `HostRef` como etiqueta + instancia; `BindingRef` con ámbito UNIT/TASK y `Location` TRANSIENT/CUSTODIED; `null` solo como no aplica, con estados explícitos; `Facts` como único punto abierto, validado en dos fases y contrastado (Anexo B) |
| Fallos y contadores | S-12 como BLOCKED; Routing siempre estricto; «otra corrida → STOP»; Termination solo con la operación 7; contador por verificaciones | matriz de conservado / ampliado / sustituido (Anexo G.1): S-12 conservado, advisory conservado, Handoff conservado, Termination = COMPLETED + operación 7, contador por correcciones lanzadas, registro de lanzamientos y LAUNCH_UNCERTAIN, `ContinuesTaskId`; casos de cierre exactos (G.2) |
| Fixture y CI | FIXTURE_BASE con textos sin vigencia; CI del fixture «sin cambio» | arranque con activación **de prueba** por repositorio (`TEST-ACTIVATION`) y manifiesto; regla de `Ci` sustituida («jobs del AGENTS del repositorio de la unidad», igual en RackCad), declarada; OD-7 como precondición del cierre de F6; OD-5 con una sola semántica (§15; D.1-D.2) |
| Presupuestos y portabilidad | topes 10/8 sin relación con los negativos; FX-01 «0 invocaciones»; FX-04 sin integración | hoja de invocaciones por rol y fase, con controles reutilizados y totales por ronda; FX-01 como turnos de la sesión del Principal; FX-04 en el punto Q0, con terminación de A por observador externo, oráculo fuera del host, QR por CAS y continuación por B hasta verificar G'; entradas canónicas, técnicas y privadas con cobertura del registro; FX-05 distingue gate legítimo de intento real (D.3-D.7) |
| Gates y evidencia | índice ADR en F1; OBL-02 RG frente a G | ADR formal en F1 sin fila de índice (índice en el cierre); matriz única C-01..C-27 con gate, entrada disponible y clase; clases (i) Core y (ii) controles sin equivalencias nuevas con AGENTS; pruebas `/v1` con sus oráculos intactos; corrección frente a A-n (§§16-17; Anexo C) |
| OD-6 | alternativa 1 recomendada frente a 2 | ambas delimitadas por revisiones, actor de referencia, evidencia, alcance (solo I62, no retroactivo) y efecto; recomendación del Coordinator registrada como tal (§11.4) |

Se preservan los cierres de V1: **R62-V1-09** (identidad por recibo + commit/ruta/blob) y **O62-V1-01** (capacidades por rol y acción).

## 4. Disposición por ID (la sesión declara; el cierre lo decide el emisor)

| ID | Disposición de la sesión | Dónde | Cierre verificable que se ofrece (análisis) |
|---|---|---|---|
| R62-V2-01 | atendido | §14; Anexo E | E.3: unidad previa con cadena nueva y todas sus autoridades compatibles; unidad nueva; unidad sin metadata; unidad desconocida; C-20 sobre la resolución completa |
| R62-V2-02 | atendido | §§5-6, 8, 9.2; Anexo F | F.1 cadena normal hasta VERIFIED de G; F.2 caídas antes y después de cada publicación; F.3 dos recuperaciones sin dos escritores; F.4 A→B hasta VERIFIED de G' |
| R62-V2-03 | atendido | §§2, 7, 11.1; Anexo B | casos de C-08, C-11, C-12 y C-13: misma instancia con dos `BindingId`; referencias de otra tarea o revisión; filas contradictorias; requisito omitido (E11); adapter `test-null` con `Facts`; máquina homónima sin herencia (B.2) |
| R62-V2-04 | atendido | §9.3, §13; Anexo G | G.2: dos verificaciones; corrección lanzada sin entrega; pérdida de contexto; Routing required frente a advisory; otra corrida con Identity pass y fail; turno fallido |
| R62-V2-05 | atendido | §15; D.1-D.2, D.4 | D.1 traza de arranque hasta un contrato I62 válido; D.2 `Ci` positiva y negativa; matriz de evidencia real / ensayo / limitada |
| R62-V2-06 | atendido | §12; D.3-D.7 | hoja de invocaciones con totales; A→B (D.3 y F.4); cobertura del registro y caso de log incompleto (D.6) |
| R62-V2-07 | atendido | §§16-17; Anexo C | matriz única; secuencia sin índice ADR temprano, sin obligaciones de gates futuros y sin reutilización por igualdad de árbol |

## 5. Los 12 retos del mandato («ARCHITECT»)

| # | Reto | Dónde | Riesgo residual que se pide juzgar |
|---|---|---|---|
| 1 | Proveedor en los roles | §2; C-01, C-02 | la asignación por topología del Anexo D es binding de prueba, no semántica |
| 2 | Supuestos no verificables del modelo | §§4.2, 5, 10 | que RUNTIME_OBSERVED baste como mínimo |
| 3 | Autointrospección falsa | §10; B.4 `Assurance`; I-02 candidata | la configuración del cliente frente al backend |
| 4 | Memoria privada oculta | §8, D.6; oráculo fuera del host; entradas automáticas enumeradas | un registro sin lecturas deja FX-04 en UNVERIFIED |
| 5 | Router complejo | §5 (siete pasos sobre el routing existente) | el número de contratos (B.1) |
| 6 | Requisitos excesivos entre proveedores | §11.3 (Proveedor casi siempre PREFERRED) | el valor para «Coordinator/Worker en la misma sesión» |
| 7 | Seguridad y autenticación | §6; B.4; C-10 con secretos ficticios | el uso futuro de metadatos de sesiones (SP-3) |
| 8 | Custodia ambigua | §8 (Q0/Q7/QR, diario, CAS) | durante la cesión la custodia solo está en el diario transitorio: un cambio de máquina en esa ventana deja STOP |
| 9 | Recuperación que sobrescriba trabajo | §9 (T11-T16), F.2 | no admitir ISOLATION_ACCREDITED puede bloquear recuperaciones legítimas |
| 10 | Level B sin necesidad | §17; C-15..C-17 en F4 | si los fragmentos del README son tooling de hecho |
| 11 | Esquemas de proveedor | B.2-B.3 (`AdapterId` con patrón; `Facts` como único punto abierto, validado y contrastado) | el coste de un esquema de hechos por adapter |
| 12 | Reanudar en otra máquina | §9.1, T14, F.2; `HostRef` | sin terminación acreditada en otra máquina, la reanudación queda detenida por diseño |

## 6. Decisiones del Owner y su frontera real

| Id | Decisión | Bloquea |
|---|---|---|
| OD-6 | predicado de independencia de LIFECYCLE (dos alternativas; recomendación del Coordinator: la 1, no decisión) | acuerdo y Freeze |
| OD-1 | ADR sucesor | READY-03 y vigencia |
| OD-2 | línea base de huella por adapter | invocaciones afectadas |
| OD-3 | autenticar Claude CLI | B |
| OD-4 | sandbox de Codex para escritura | B y A→B |
| OD-5 | permiso de ensayo con la semántica única de §15 | F6 |
| OD-7 | remoto del fixture con CI | **cierre de F6** |

## 7. Preguntas para la revisión

1. ¿El diario transitorio encadenado entre Q0 y Q7 (§8) es suficiente para la custodia durante la cesión, dado que 16.4 prohíbe escrituras Git de la sesión?
2. ¿La sustitución de la regla de `Ci` (D.2) es aceptable como cambio declarado que no altera su significado en RackCad?
3. ¿Es viable la terminación del Principal de escritorio observada por otra sesión (`isRunning`), o debe quedar UNVERIFIED por defecto?
4. ¿Las clases ANTERIOR / POSTERIOR / DESCONOCIDA, con PRE y POST, cubren todos los casos de adopción sin default?
5. ¿El conjunto `I62` (B.1) puede reducirse sin perder validación?
6. ¿La hoja de invocaciones (D.3) cubre los controles necesarios sin exceso?

## 8. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni sesiones nuevas.

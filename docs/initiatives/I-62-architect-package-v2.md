# I-62 — Paquete de revisión (Proposal V2)

```text
PROPOSAL V2 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED (su revisión de V1 fue CHANGES REQUIRED: docs/initiatives/I-62-coordinator-review-v1.md)
Architect      = NOT REVIEWED (no hay revisor asignado en este paquete; no se simula)
Consensus      = NOT REACHED
Owner          = OD-6 BLOCKED — OWNER DECISION (Proposal V2 §11.4); bloquea el acuerdo y el Freeze, no la revisión
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v2.md            blob 47a643ddf1f22226072ede67a862133f77441e2e
  docs/initiatives/I-62-coordinator-review-v1.md  blob b518a4f977ee5b18157fc45c881a8ea06d69d7cc
Versión anterior revisada: Proposal V1, commit ea0555913202efc96e1007f021942579b989b43d, blob 31e2f4afa895dc03f7621504e150de0539a2302e
Discovery base: docs/initiatives/I-62-discovery.md, R1, blob 86f24e657998a126ef6fc719ff20f1de938c83e7 (commit 2b7976b0); §21 en commits posteriores
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (R62-V1-09). Este paquete no puede contener el SHA del commit que lo publica sin autorreferenciarse. El commit lo da el **recibo
> de publicación**: el informe de entrega de la sesión y el cuerpo o la evidencia del siguiente commit. El revisor resuelve
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v2.md` y comprueba que el blob es `47a643dd…`. **Si no coincide, o la rama avanzó con otro
> contenido, revisa la versión designada o rechaza la discordancia**; nunca aprueba el último archivo encontrado.

> **Qué es.** El material completo para revisar la Proposal V2 sin reconstruir la sesión: el diseño entero, el delta V1→V2 y la disposición por ID. La
> sesión autora **no** emite veredicto del Architect ni del Coordinator, **no** declara consenso y **no** afirma independencia de nadie.

## 1. Veredicto que se solicita (LIFECYCLE §5)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
           AGREED solo con cero REQUIRED abiertos sobre la versión exacta revisada.
           BLOCKED — OWNER DECISION si falta autoridad reservada (p. ej., OD-6).
Hallazgos: REQUIRED | OPTIONAL — ID, sección, evidencia (archivo:línea o hecho), cambio exigido.
           Pueden añadirse notas, que no sustituyen esas clases. Un OPTIONAL no condiciona un AGREED.
Modo:      SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN, y si revisor y autor son la misma persona.
Identidad: commit, ruta y blob revisados.
```

Solo quien emitió un REQUIRED puede rebajarlo, con ID, clasificación, razón y evidencia (LIFECYCLE §5).

## 2. Lectura

1. [Proposal V2](I-62-proposal-v2.md), completa, con los anexos A (ADR sucesor), B (contratos), C (pruebas y controles) y D (pilotos).
2. [Registro de la revisión del Coordinator de V1](I-62-coordinator-review-v1.md).
3. [Discovery R1](I-62-discovery.md) §§1-3, 9-13 y 21.
4. [Decisiones](../automation/decisions/I-62.md) §§7, 9, 10 y 11.
5. Fuentes integradas que V2 modifica o cita:
   - AUTOMATION_PLAN §16 y §8;
   - ADR-0046;
   - Freeze de I-61 ([Proposal V9](I-61-proposal-v9.md) §§3, 8, 13 y 15);
   - `routing.md`, `model-catalog.md` y el README de `docs/automation/agent-execution/`;
   - LIFECYCLE §§2-9;
   - WORKFLOW §§3-4, 10 y 11.

## 3. Delta V1 → V2

| Área | V1 | V2 |
|---|---|---|
| Adopción | por cadena: «primer contrato posterior a la integración» | **por unidad**: `protocol` fijado en el bootstrap según la base del reclamo frente a `I62_EFFECTIVE_SHA`; las unidades I-61 terminan con I-61; P-15; MC-10 (§14) |
| Planos | registro I-62 en cada sesión F1-F7; F6 como «delegación real» | planos (a) real / (b) materializado inactivo / (c) sistema bajo prueba; P-16; MC_I62 y FIXTURE_BASE; OD-5 con permiso exacto y clasificación A/B (§15) |
| Gates | helper «antes del Freeze» decidido en F4; F7 con READY-06; FOUNDATIONS en F7 | Level A decidido **antes** del Freeze, con controles de viabilidad en F4 y A-n si fallan; MC_I62 tras el último cambio normativo; F7 sin cambio normativo ni READY-06; FOUNDATIONS: borrador en F7, publicación en el cierre; OV por escenario (§§17-18) |
| Independencia | «la que fije LIFECYCLE»; «más estricta» | cuatro dimensiones (Actor/Sesión/Contexto/Proveedor) × REQUIRED/PREFERRED/NOT_REQUIRED; combinación por máximo; tabla completa; revisiones de LIFECYCLE → **OD-6 BLOCKED — OWNER DECISION** (§11) |
| Contratos | inventario | Anexo B: identidades, campos, `null`/UNKNOWN, resolución y validación de `AdapterFacts` (`Test-Json`), correlaciones de `worker-handoff/v1`, delta de A1-A8 y de las 14 comprobaciones, operaciones por adapter, huella «ninguna» solo con aceptación del Coordinator, elegibilidad de ADR-0046 #4 sin ciclos |
| Recuperación | tres estados | evidencia TERMINATION_ACCREDITED / ISOLATION_ACCREDITED (no admitida) / NO_OBSERVATION; transiciones T1-T12 con CAS por Git; fallos del mandato; orden de publicación; autoridad de cada contador (§§8-9) |
| Pruebas | INV/OBL estructurales | Anexo C con entrada, componente, acción, observable, esperado independiente, fallo legítimo, gate y clase (G / RG / MC / FX / OV); secretos ficticios en campos genéricos; compatibilidad `/v1` |
| Pilotos | A «Controller o Reviewer»; B sin Controller; fixture sin CI | Anexo D: cinco roles por topología; repositorio real frente al fixture; CI con OD-7 o `Ci` UNVERIFIED; PASS/FAIL/UNVERIFIED/UNSUPPORTED; topes numéricos; siguiente decisión de B; auditoría de lecturas; OD por runtime; cobertura mínima |
| Herramientas | `gh` y SDK generales | capacidades por rol y acción (`remote-facts`, `build-test`); herramienta en la receta (§4.1, §6) |
| Owner | OD-1..OD-5 | OD-1..OD-7, con OD-6 BLOCKED ahora y las demás en su frontera real (§18) |

## 4. Disposición por ID de la revisión del Coordinator de V1

| ID | Disposición de la sesión (no es un veredicto) | Dónde en V2 |
|---|---|---|
| R62-V1-01 | atendido: adopción por unidad, sin migración ni excepción; `worker-handoff/v1` con compatibilidad demostrada en B.6; control MC-10 | §14; B.1, B.6; C (MC-10) |
| R62-V1-02 | atendido: matriz de planos; P-16 y FX-05; MaterializationClose y AuthorityRevision por lado; OD-5 con permiso exacto y alternativa de excepción identificada (cláusula, delta, alcance, autoridad) | §15; D.6 |
| R62-V1-03 | atendido: Level A decidido antes del Freeze, con A-n si los controles de F4 fallan; MC_I62 tras el último cambio normativo e invalidación ante cambios posteriores; F7 sin READY-06; FOUNDATIONS borrador frente a publicación; OV por escenario | §§17-18 |
| R62-V1-04 | atendido en lo decidible: dimensiones, combinación, satisfacción y tabla del dominio de §16 con todos los riesgos. **El predicado de LIFECYCLE queda como BLOCKED — OWNER DECISION (OD-6)**, con alternativas y efecto | §11 |
| R62-V1-05 | atendido como diseño: Anexo B completo; mecanismo de validación elegido (`Test-Json`); controles MC-03, MC-05 | §§5-7; B |
| R62-V1-06 | atendido: transiciones T1-T12, CAS, fallos del mandato, orden de publicación, contadores con autoridad y topes heredados; controles MC-07 y MC-08 | §§8-9 |
| R62-V1-07 | atendido: clases separadas; esperado independiente; Level A sin componente ejecutable de agregación declarado como límite (la conducta se ejerce en MC y FX) | §16; C |
| R62-V1-08 | atendido: Anexo D completo | §12; D |
| R62-V1-09 | atendido en este paquete: estados de LIFECYCLE, clases REQUIRED/OPTIONAL, identidad por blob y recibo, sin revisor simulado | §§1, cabecera |
| O62-V1-01 | atendido: capacidades por rol y acción | §4.1; §6 |

## 5. Los 12 retos del mandato («ARCHITECT»)

| # | Reto | Dónde lo trata V2 | Riesgo residual que se pide juzgar |
|---|---|---|---|
| 1 | Identidad del proveedor en los roles | §2; OBL-01; Anexo A | la tabla D.2 asigna proveedores a roles **en los pilotos**: es binding de prueba, no semántica |
| 2 | Supuestos no verificables del modelo | §§4.2, 5 (elegibilidad de ADR-0046 #4), 10 | que RUNTIME_OBSERVED baste como mínimo |
| 3 | Autointrospección falsa | §10; EXP-05 → B.4 `Assurance`; I-02 candidata | que la configuración aplicada por el cliente no refleje el backend |
| 4 | Memoria privada oculta | §§8, 15; D.3 FX-04 con el oráculo fuera del host; D.6 auditoría de lecturas | si el runtime de B no registra lecturas, FX-04 no puede ser PASS |
| 5 | Router demasiado complejo | §5: ocho pasos sobre el routing existente; sin motor | el número de contratos nuevos (B.1) |
| 6 | Requisitos excesivos entre proveedores | §11.3: Proveedor casi siempre PREFERRED | que «Coordinator y Worker en la misma sesión» deba exigir Proveedor |
| 7 | Filtración de seguridad o autenticación | §6; B.4 `Fingerprint` sin valores; MC-09 con secretos ficticios | la lectura de metadatos de sesiones en futuros preflights (SP-3) |
| 8 | Custodia ambigua | §8 (declarado/observado/autorizado; CAS); §9.2 | la convivencia con WORKFLOW §3 (M-01) |
| 9 | Recuperación que sobrescriba trabajo | §9.1-9.2 (T6, T7, T9, T11) | que no se admita ISOLATION_ACCREDITED, lo que puede bloquear recuperaciones legítimas en otra máquina |
| 10 | Level B sin necesidad | §17: Level A decidido; controles de viabilidad en F4; A-n si fallan | si los fragmentos del README son tooling de hecho |
| 11 | Esquemas específicos del proveedor | §7; B.2 (`AdapterId` con patrón, `ProtocolSet` como versión), B.3 | el coste de un esquema de hechos por adapter |
| 12 | Imposibilidad de reanudar en otra máquina | §9.1 (T9, P-13); D.3 FX-04 | sin aislamiento admitido, el cambio de máquina con trabajo sin commit queda detenido por diseño |

## 6. Preguntas para la revisión

1. ¿RUNTIME_OBSERVED como mínimo de introspección (§10) es suficiente para cada rol?
2. ¿El CAS por Git (§8) y la tabla T1-T12 dejan alguna interrupción sin estado definido?
3. ¿El conjunto `I62` (B.1) es el mínimo, o pueden fusionarse `preflight` y `binding` sin perder validación?
4. ¿La tabla §11.3 cubre todos los riesgos con los valores adecuados? ¿Qué alternativa de OD-6 recomienda el Architect al Owner, con qué argumento?
5. ¿La clasificación A de OD-5 (los ensayos no son ejecución delegada de §16) es defendible frente a ADR-0046 #1, o debe pedirse la excepción B?
6. ¿El aislamiento de D.6 (oráculo fuera del host + auditoría de lecturas) es verificable con los registros reales de cada runtime?
7. ¿La adopción por unidad (§14) y la lectura en `I62_EFFECTIVE_SHA^1` dejan a las unidades I-61 con autoridades completas y legibles?

## 7. Lo que este paquete no hace

No asigna revisor ni realiza la revisión. No declara AGREED, Freeze, independencia ni aprobación del Owner. No autoriza implementación, delegaciones,
sondas ni pilotos.

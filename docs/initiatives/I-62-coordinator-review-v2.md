# I-62 — Revisión del Coordinator de la Proposal V2 (registro)

```text
Emisor:           Coordinator exclusivo de I-62
Modo:             SEPARATE SESSION respecto de la sesión autora; revisor ≠ autor
Naturaleza:       revisión del Coordinator; no es dictamen del Architect, firma del Owner, decisión del Master, Freeze ni autorización de implementación
Fecha:            2026-10-01
Fuente original:  I-62-Proposal-V2-revision-y-orden-V3.txt (31 529 bytes; SHA-256 7f55b9311b930b344bae7e1a0a6dcae64d8270bad515ccbbe38f79c8b31c9cb2;
                  custodiada fuera del repositorio; este archivo es su registro recuperable)
Objeto revisado:  commit a1f5e0035f0e109d02a336c0a01917e53a53b88a (padre ea055591…)
                  docs/initiatives/I-62-proposal-v2.md, blob 47a643ddf1f22226072ede67a862133f77441e2e
                  docs/initiatives/I-62-architect-package-v2.md, blob 8d8e76256fda20cdd0b49e5b60de7147c60a1690
                  docs/initiatives/I-62-coordinator-review-v1.md, blob b518a4f977ee5b18157fc45c881a8ea06d69d7cc
                  rama architecture/portabilidad-coordinador-principal; Claim-Id 5b661a17-…; main observado 819955d6…
Veredicto:        CHANGES REQUIRED sobre V2 (R62-V2-01..07). OD-6 sigue pendiente del Owner, pero NO es el único asunto que impide el acuerdo
Architect:        NOT REVIEWED · Consensus: NOT REACHED · Frozen: NO
```

Registro redactado por la sesión responsable por orden del emisor (C62-F0-20, punto 3). El contenido es del Coordinator y no reescribe el
[registro de V1](I-62-coordinator-review-v1.md). La respuesta de la sesión está en la [Proposal V3](I-62-proposal-v3.md) y en el
[paquete V3](I-62-architect-package-v3.md) §4.

## 1. Recepción e identidad (C62-F0-17)

Verificado por el Coordinator mediante GitHub:
- la punta remota coincide con el commit revisado;
- un commit y siete archivos (decisiones, evidencia, estado, contrato, Proposal V2, paquete V2 y registro de V1); V1, su paquete y el Discovery no aparecen
  en el delta;
- la CI 36914815626, attempt 1, push, rama y head_sha exactos, `completed/success`: UI Tests (110546066161), Build UI (110546066596), Tests (Domain +
  Application) (110546066788) y Build Plugin without AutoCAD (110546725540);
- los blobs coinciden con el recibo.

La limpieza, `git diff --check`, los enlaces y la ausencia de invocaciones son atribución de la sesión. **Los contraejemplos de esta revisión son ANÁLISIS
DEL DISEÑO, no pruebas ejecutadas.**

Refs ajenas observadas por el Coordinator (no auditadas): I-63 `23eeefc5…`, I-64 `dcc16bed…` e I-52 `d8078ef3…`. La sesión revalida DC-07 por delta.

**Siguen vigentes:**
- G0 GATE PASS; Discovery R1 como base;
- NEW ARCHITECTURE; una unidad;
- cinco roles; custodia por unidad sin registro global;
- estados semánticos; Level A sin plataforma;
- restricciones de producto y credenciales.

**Fuentes:**
- mandato: AUTHORITIES, ROLE BINDING, INDEPENDENCE BY RISK, CONTEXT PORTABILITY, CUSTODY, ORPHAN / FAILURE RECOVERY, RETRY BUDGET, EVIDENCE FROM CURRENT
  PRODUCT INITIATIVES, I-61 EXECUTION, VALIDATION y FIRST RESPONSE REQUIRED;
- C62-F0-08..16;
- V2 completa;
- en `819955d6`: AUTOMATION_PLAN §§16.3-16.12, WORKFLOW §§3-4, 10 y 11.4, LIFECYCLE §§5-9, AGENTS y ADR-0046;
- `AgentExecutionProtocolTests`, como fuente de lo que protege.

## 2. Disposición de los hallazgos de V1 (C62-F0-18)

| ID | Disposición del emisor |
|---|---|
| R62-V1-01 | principio de adopción por unidad atendido; **abierto** en lectura legacy completa y clasificación de la incertidumbre → R62-V2-01 |
| R62-V1-02 | separación real / materializado / fixture atendida; **abierto** en arranque del fixture y autoridades evaluadas → R62-V2-05 |
| R62-V1-03 | circularidades F4/Freeze y F7/READY-06, ubicación de MaterializationClose y separación de OV atendidas; **abierto** por el índice ADR y la asignación de evidencia → R62-V2-07 |
| R62-V1-04 | combinación por dimensiones y OD-6 atendidas; **abierto** en identidad del actor y acreditación del contexto → R62-V2-03, R62-V2-06 |
| R62-V1-05 | avance sustantivo; **abierto** en identidades, validación, invalidadores y fidelidad del delta → R62-V2-02/03/04 |
| R62-V1-06 | tabla positiva y contadores añadidos; **abierto**: la secuencia no conserva las invariantes de SHA y relevo, y algunos fallos cambian de significado → R62-V2-02/04 |
| R62-V1-07 | separación G/RG/MC/FX/OV y secretos ficticios atendidos; **abierto** en oráculos y dependencias → R62-V2-04/06/07 |
| R62-V1-08 | Controller explícito, FAIL y siguiente decisión de B atendidos; **abierto** en ejecutabilidad, CI, presupuestos y aislamiento → R62-V2-05/06 |
| R62-V1-09 | **CERRADO**: veredictos vigentes e identidad por recibo + commit/ruta/blob. Preservar en V3 |
| O62-V1-01 | **ATENDIDO**: herramientas por capacidad, rol y acción. No reabrir |

## 3. Hallazgos REQUIRED de V2 (C62-F0-18)

| ID | Problema (resumen fiel) | Corrección exigida | Cierre verificable (análisis, no ensayo) |
|---|---|---|---|
| **R62-V2-01** Adopción legacy completa y clasificación acreditada | V2 solo lee §16 en `I62_EFFECTIVE_SHA^1`, pero I-62 cambiará también §8, routing, catálogo, PROMPT_TEMPLATES, WORKFLOW y quizá LIFECYCLE: una unidad anterior mezclaría autoridades. V2 clasifica la duda como I61 y presupone `protocol` en unidades sin ese campo. I-52 no debe verse obligada a delegar por aparecer en una lista | (1) Mapa de compatibilidad por superficie y cláusula (fuente I61, fuente I62, vigencia, lectores, evidencia), sin congelar archivos enteros. (2) Anterior demostrada / posterior demostrada / desconocida: lo desconocido detiene el paso dependiente y pide evidencia. (3) Lectura de una unidad sin `protocol`, sin editar su rama ni inferir desde su ascendencia. (4) Vigencia de OD-6 y de las reglas nuevas solo para unidades I62, sin efecto retroactivo sobre I-62, I-63, I-64 ni I-52. (5) Punto efectivo anclado y único; ausencia, duplicidad o contradicción no activan | unidad previa que abre una cadena tras el merge con **todas** sus autoridades compatibles; unidad nueva; unidad sin metadata; unidad con orden o base desconocidos; MC-10 sobre la resolución completa |
| **R62-V2-02** Custodia, preflight y exact-SHA sin ciclos | Contraejemplo: el Worker publica G; T3 devuelve HELD(P) con un commit C, pero `Identity` exige `HEAD` = remoto = `CurrentSha` = G. Si C se retrasa, faltan las transiciones para ceder al Controller sin violar «toda transición es un commit». Afecta también a T1 tras aceptar un `BaseSha` y a `S_B` en FX-04. §16.4 prohíbe escrituras Git de la sesión hasta verificar. Otro ciclo: §6 invalida el preflight al cambiar binding o SHA, y §5 ordena preflight → binding → transferencia publicada | (1) Secuencia única con roles, permisos, transitorios y publicaciones, incluido el Controller en la cesión. (2) Distinguir BaseSha, SHA del trabajo, del estado, evaluado y de evidencia; delta explícito si cambia §16.4/Identity/Remote. (3) Resolver BindingRef y preflight sin publicarlos donde Git está prohibido; una referencia transitoria no es custodia. (4) Conservar trabajo local ante caídas; un push rechazado = transición no acreditada, con causas separadas. (5) El perdedor de una recuperación no opera por haber releído. (6) T10 distingue avance del Worker, publicación local pendiente, escritura ajena y `main` cambiado. (7) TERMINATION_UNACCREDITED ≠ ORPHAN_CONFIRMED | secuencia con SHAs simbólicos donde una cadena normal y el relevo A→B llegan legítimamente a verificar el SHA correcto; caídas antes y después de cada publicación; dos recuperaciones sin dos escritores. Un ejemplo que solo termina en STOP no demuestra la ruta normal |
| **R62-V2-03** Identidad real, referencias y contratos validables | Actor distinto por `BindingId` (un actor puede tener dos bindings). `BindingRef` depende de un commit no fijado y exige `TaskId` a bindings por unidad. Contratos incompletos (`P<…>`, tipos «—», `Mandatory[]`, `Independence[]`, A3-A5 sin operandos, cardinalidad y duplicados). `null` con varios significados. `Facts` frente a `additionalProperties:false`. Un hash de hostname+OS no distingue máquinas homónimas | (1) Identidad de actor o instancia de runtime independiente del `BindingId`, con hechos observables. (2) Procedencia, revisión, ámbito, existencia, unicidad, inmutabilidad y conducta ante duplicados, referencias ajenas o evidencia obsoleta. (3) Campos semánticos completos; una delegación nunca suprime requisitos del contrato. (4) `null`, ausente y UNKNOWN por campo; perfil del Principal disponible aunque su binding no se acepte. (5) Validación de `Facts` dentro del núcleo, frontera estricta y contraste independiente; resolución en revisiones autorizadas. (6) `HostIdHash` como etiqueta con su límite | misma instancia con dos `BindingId`; referencias de otra tarea o revisión; dos filas contradictorias; campo requerido desconocido; adapter nuevo con `Facts` no vacío sin tocar el núcleo; máquina homónima que no hereda capacidad |
| **R62-V2-04** Sin cambios silenciosos en fallos, routing ni contadores | (1) S-12 tratado como reejecución de BLOCKED, cuando §16.11 lo define como STOP con análisis y decisión. (2) `Routing` falla siempre, pero `/v1` conserva la rama advisory. (3) «Otra corrida → STOP», cuando §16.9 da REWORK si `Identity` pasa. (4) `Termination` reducida a la operación 7, sin el turno COMPLETED. (5) El contador por clase basado en verificaciones, cuando §16.8 cuenta **correcciones lanzadas**. (6) Topes sin registro de lanzamientos ni tratamiento del lanzamiento incierto; una `TaskId` nueva no puede reiniciar la cadena | Matriz de cada cambio contra su cláusula `/v1` (conservado, ampliado o sustituido; un endurecimiento también es cambio). Distinguir etapa, `FailureClass`, `Disposition` y STOP simultáneos, con first failure y combinación de 16.9. Contadores desde eventos reales con identidad de lanzamiento. Sin remedio de C-F0-RED | misma entrega con dos verificaciones; corrección lanzada sin entrega; pérdida de contexto; Routing required frente a advisory; handoff de otra corrida con Identity pass y fail; proceso terminado con turno fallido |
| **R62-V2-05** Fixture arrancable y CI compatible | (1) FIXTURE_BASE copia textos cuya vigencia exige `I62_EFFECTIVE_SHA`, que el fixture no tiene. (2) La CI del fixture «cuyos jobs declara su contrato» frente a `Ci` «sin cambio» de los cuatro jobs de AGENTS. (3) Sin OD-7, FX-02 no puede ser PASS y D.4 lo exige: OD-7 es precondición indirecta del cierre de F6 | (1) Preparación del fixture (ramas, main, remoto, protocolo de prueba, autoridades, revisiones, manifiesto, selección explícita limitada al plano c). (2) Semántica de `Ci` resuelta antes del Freeze; equivalencia o desviación declarada con su aprobación; la evidencia del fixture no sustituye las cuatro corridas de RackCad. (3) OD-7 separado de cualquier excepción. (4) Dependencias reales de la cobertura; sin precondición, UNVERIFIED y gate pendiente. (5) OD-5 con una sola semántica | traza de bootstrap del fixture hasta un contrato I62 válido; `Ci` positiva y negativa; matriz de evidencia real / del ensayo / limitada |
| **R62-V2-06** Presupuestos y portabilidad ejecutables | (1) FX-02 suma seis llamadas Codex + Worker + Reviewer y luego nueve negativos sin relación con los topes 10/8. (2) FX-01 «0 invocaciones», pero el Principal produce tres preflights. (3) FX-04 detiene A antes de verificar G y pide `S_B`: debe integrarse con la secuencia exact-SHA. (4) Los adapters de Principal tienen terminación UNVERIFIED: un Exit o el silencio de A no bastan; hace falta un observador externo. (5) La auditoría de comandos no prueba todas las entradas ni las lecturas automáticas; prohibir toda lectura fuera del fixture excluiría dependencias técnicas. (6) FX-05 debe distinguir un gate legítimo del fixture de un intento de actuar sobre la unidad real | (1) Hoja de invocaciones por escenario, rol y fase (Principales, negativos, sondas, reintentos, OV), controles reutilizados, presupuesto por ronda y total; P-07 antes de lanzar. (2) FX-04 integrado en el punto de interrupción, con la siguiente acción y los contadores. (3) Terminación y reanudación del Principal con quien recoge la evidencia; sin capacidad, UNVERIFIED. (4) Entradas canónicas, dependencias técnicas y entradas privadas delimitadas; cobertura y huecos del registro. (5) Oráculo previo fuera de las entradas de B; evidencia saneada. (6) Límites por topología, rol y escenario | tabla finita que cabe en los presupuestos; secuencia A→B con entrada autorizada, exclusividad acreditada, respuesta y verificación del SHA correcto; log incompleto sin declarar aislamiento probado |
| **R62-V2-07** Coherencia de gates, cadencia documental y evidencia | F1 publica el índice ADR con ventana, cuando WORKFLOW §11.4 concentra los índices en el cierre. F1 anuncia OBL-02 en RG, pero el Anexo C la da como G. Las clases RG/MC/FX no deben crear equivalencias no declaradas con AGENTS. Controles que presuponen formatos de gates posteriores. Esperados de preflight fijados por una foto antigua | (1) ADR formal propuesto antes de implementar, con la fila del índice en el cierre; sin ventana. (2) Alinear por obligación: gate, entrada disponible, procedimiento, RED legítimo, mutation check y clase; referir AGENTS sin equivalencias nuevas. (3) Cobertura de las reglas nuevas y pruebas legacy con sus oráculos intactos. (4) Mantener F7 sin READY-06, FOUNDATIONS borrador/publicación y OV exact-SHA. (5) Distinguir la corrección que devuelve al Freeze del cambio de Freeze: solo este pide A-n | matriz única invariante → obligación → gate → entrada disponible → resultado → clase/autoridad; secuencia sin índice ADR temprano, sin obligaciones que presupongan un gate futuro y sin reutilización por igualdad de árbol |

## 4. OD-6 y Architect (C62-F0-19)

OD-6 sigue siendo materia del Owner; no se concede. No impide redactar V3. La revisión sigue en CHANGES REQUIRED, además del bloqueo de acuerdo que implica
OD-6.

**Recomendación del Coordinator al Owner** (no es decisión): preferir la **alternativa 1** para las revisiones mayores de unidades que adopten I62, con
contexto y sesión independientes exigidos y proveedor distinto preferido, no obligatorio por microtarea. El predicado debe precisar revisiones, actor de
referencia, evidencia y alcance; un `BindingId` distinto no basta. Sin efecto retroactivo sobre unidades I61, y sin convertir una revisión del Coordinator en
dictamen del Architect.

Si el Owner aún no decide, V3 conserva ambas alternativas delimitadas; sin AGREED ni Frozen: YES. No se crea otro bloqueo externo. El Architect debe
realizar todavía la revisión previa a la implementación; este registro no la reemplaza ni simula su invocación.

## 5. Autorización documental (C62-F0-20)

**Salidas:**
- Proposal V3 completa (`Frozen: NO`), con anexos y delta V2→V3;
- paquete V3;
- este registro;
- superficies propias, con la CI de `a1f5e003` en la siguiente escritura.

**Preservación:** V1, V2, sus paquetes, el Discovery, el mandato y las revisiones previas.

**Allowlist:** Proposal V3, paquete V3, este registro, contrato, decisiones, evidencia, estado y `evidence/I-62-discovery/` (solo extractos saneados).

**Fuera de alcance:**
- ROADMAP, HANDOFF, FOUNDATIONS, normas, índices ADR, ADR numerados nuevos, catálogos, esquemas operativos;
- producto, tests, assets, deploy, CI, configuración y ramas ajenas;
- remotos o repositorios de ensayo, pruebas, modelos delegados, subagentes, SP-1/SP-2, ampliar SP-3, Architect externo, pilotos, sesiones nuevas;
- instalaciones, autenticaciones, sandbox, PATH, `config.toml`, `service_tier` y credenciales.

**Publicación y parada:**
- preflight diferencial, `git diff --check`, commit y push con su CI; PENDING si sigue en curso;
- sin consumir ni reiniciar `attempts`;
- recibo con commit, ruta y blob, disposición por REQUIRED y trazas como análisis;
- detenerse al publicar.

**Estado:**
- G0 = GATE PASS;
- Discovery R1 aceptado; NEW ARCHITECTURE;
- Proposal V2 = CHANGES REQUIRED; Proposal V3 = redacción autorizada;
- OD-6 pendiente del Owner;
- Architect NOT REVIEWED;
- FREEZE = NOT_AGREED; IMPLEMENTATION AUTHORIZATION = NO.

# I-62 — Revisión del Coordinator de la Proposal V1 (registro)

```text
Emisor:           Coordinator exclusivo de I-62
Modo:             SEPARATE SESSION respecto de la autora; revisor ≠ autor
Naturaleza:       revisión del COORDINATOR; no es dictamen del Architect, firma del Owner ni decisión del Master
Fecha:            2026-10-01
Fuente original:  I-62-Proposal-V1-revision-y-orden-V2.txt (27 797 bytes; SHA-256 101e9bbaec51c967395bce10de69dac4a241a87310e496038ce323d9b1f3fb0b;
                  custodiada fuera del repositorio por el Owner; este archivo es su registro recuperable)
Objeto revisado:  commit ea0555913202efc96e1007f021942579b989b43d
                  docs/initiatives/I-62-proposal-v1.md, blob 31e2f4afa895dc03f7621504e150de0539a2302e
                  docs/initiatives/I-62-architect-package-v1.md, blob 6bee2d7db0ae4dc5ecaeb3e5d1988f7b84e6d491
                  padre 2b7976b0…; rama architecture/portabilidad-coordinador-principal; Claim-Id 5b661a17-…; main observado 819955d6…
Veredicto:        CHANGES REQUIRED sobre V1 (R62-V1-01..09 REQUIRED; O62-V1-01 OPTIONAL)
Architect:        NOT REVIEWED · Consensus: NOT REACHED · Frozen: NO
```

Este archivo registra la revisión con su identidad, sus hallazgos, sus razones y las obligaciones de corrección. Lo redactó la sesión responsable como
**registro**, por orden del emisor (C62-F0-15 punto 3); el contenido es del Coordinator. La respuesta de la sesión está en la
[Proposal V2](I-62-proposal-v2.md) y en el [paquete V2](I-62-architect-package-v2.md) §4.

## 1. Recepción e identidad (C62-F0-13)

Verificado por el Coordinator mediante GitHub:
- la punta remota y un commit de avance desde el padre;
- siete archivos, todos dentro de la allowlist de C62-F0-12, sin rutas de producto ni normas compartidas;
- la CI 36902279722, attempt 1, event `push`, rama y head_sha exactos, `completed/success`, con los cuatro jobs requeridos: UI Tests (job 110504100000),
  Tests (Domain + Application) (110504100280), Build UI (110504100386) y Build Plugin without AutoCAD (110504922226).

La limpieza local, `git diff --check`, los enlaces y la ausencia de invocaciones son atribución de la sesión: el Coordinator no inspeccionó el host ni
ejecutó pruebas. **La CI acredita la entrega, no el acuerdo de diseño ni las capacidades futuras de los adapters.**

Siguen vigentes: G0 aprobado, Discovery R1 aceptado como base y NEW ARCHITECTURE confirmado. No se reabre el Discovery ni se reasignan iniciativas, roles
de gobernanza o números ADR.

**Fuentes de la revisión:**
- mandato de I-62: ROLE BINDING, PROVIDER ADAPTER BOUNDARY, INDEPENDENCE BY RISK, CONTEXT PORTABILITY, CUSTODY, ORPHAN / FAILURE RECOVERY, RETRY BUDGET,
  EVIDENCE FROM CURRENT PRODUCT INITIATIVES, I-61 EXECUTION, VALIDATION y FIRST RESPONSE REQUIRED;
- C62-F0-08..12;
- LIFECYCLE §§5-9;
- WORKFLOW §§3-4, 10 y 11.4-11.5;
- AGENTS;
- ADR-0046 #1-#9;
- AUTOMATION_PLAN §16 y el Freeze de I-61.

**Puntos que no se reabren:**
- cinco roles neutrales; autoridad de gate fuera del router;
- custodia por unidad sin registro global;
- los cuatro `CONFIGURATION_STATUS` y su separación de la disposición;
- los niveles semánticos de I-61;
- fuentes de runtime con alcance declarado, sin autointrospección ni atestación universal del servidor;
- `/v1` preservado; sin producto;
- Level A como dirección sujeta a viabilidad;
- revisión del Architect y decisiones del Owner con alcance explícito.

No se exige plataforma, servicio de locks, motor de producción ni paquetes adicionales.

## 2. Hallazgos REQUIRED (C62-F0-14)

| ID | Problema (resumen fiel) | Obligación de corrección | Controles exigidos |
|---|---|---|---|
| **R62-V1-01** Adopción por unidad | V1 §14 adopta `/v2` «en el primer contrato posterior a la integración», lo que permitiría que I-63 o I-64, abiertas con I-61, cambiaran de protocolo entre dos cadenas. Infringe «Do NOT modify their protocol mid-initiative» y C62-F0-10 E | Política de adopción **por unidad**, con su versión acreditada. Las unidades activas siguen con I-61 todo su recorrido. Se define el punto de adopción para unidades futuras. Se distingue versión de protocolo de versión de esquema (`worker-handoff/v1` solo con compatibilidad semántica demostrada). Ni migración, ni reasignación global, ni retirada de `/v1` | una unidad I-61 que abre una cadena después de integrar I-62 no migra; los artefactos históricos conservan su interpretación |
| **R62-V1-02** Proceso vigente, materialización inactiva y sistema bajo prueba | V1 exige un registro I-62 en cada sesión de F1-F7 y llama a F6 «delegación real de I-62 bajo I-61». No separa la supervisión real de la prueba en el fixture. OD-5 se presenta como solución a 16.3 sin identificar el permiso exacto | Matriz por operación: (a) sesión real (I-61/Workflow), (b) documentos materializados sin vigencia, (c) sistema bajo prueba en el fixture. Una autoverificación ensayada no da autoridad real; las decisiones del fixture nunca acreditan GATE PASS, READY, aprobación del Owner ni integración. MaterializationClose y AuthorityRevision por lado, sin SHA futuro ni equivalencia verbal G2. OD-5 con el permiso exacto, o la excepción identificada (cláusula, delta, alcance, autoridad). OD-1 puede seguir pendiente mientras solo se materializan cambios inactivos | un resultado del fixture que intenta actuar sobre la unidad real se rechaza sin tocar `main`, contadores, decisiones del Owner ni custodia reales |
| **R62-V1-03** Orden ejecutable de gates | F4 (después del Freeze) decide un helper «antes del Freeze». F7 cambia PROMPT_TEMPLATES después de la «última» materialización. F7 usa READY-06 como evidencia (circular con READY-02). F7 parece publicar FOUNDATIONS antes del cierre. La matriz OV no separa ensayo de ejecución final sobre FINAL_CANDIDATE_SHA | Decidir Level A/B con evidencia antes del Freeze (un hallazgo posterior va por A-n). Situar MaterializationClose tras el último cambio normativo y definir qué pasa ante un cambio posterior. F7 no depende de READY-06. Separar borrador factual y publicación de FOUNDATIONS. Por escenario OV: gate que lo prepara o ensaya, evidencia, asignación y ejecución final | — |
| **R62-V1-04** Independencia decidida | V1 remite NEW ARCHITECTURE, FOUNDATION EVOLUTION y READY a «la que fije LIFECYCLE», cuando I-62 propone ese predicado; «más estricta» no tiene semántica | Tabla completa: disparador, rol o revisión, independencia respecto de qué, autoridad, evidencia y resultado si falta. Todos los riesgos del mandato, incluidos el alto coste de fallo y la acumulación de roles. Combinación de requisitos, preferencia frente a obligación y relación de satisfacción. Distinguir actor, sesión, contexto y proveedor. Conservar la autoridad del Architect y la verificación exclusiva del Controller | dos requisitos simultáneos; mismo proveedor con contexto separado; distinto proveedor con contexto compartido; Worker y verificador confundidos; preferencia frente a obligación no satisfechas |
| **R62-V1-05** Contratos e identidades congelables | §14 es un inventario: identidades y semánticas remitidas a prosa; el estado no fija versión; «el núcleo valida con el esquema del adapter» no dice cómo ni quién | Campos obligatorios y opcionales, tipos, cardinalidad y significado de ausente, `null` y UNKNOWN. Identidad y resolución de BindingRef, preflight, sesión e invocación, host, requisito, descriptor, esquema del adapter y ProtocolSet. Combinaciones de versiones; correlaciones de `worker-handoff/v1`. Resolución y validación del esquema de hechos y conducta ante adapter desconocido, esquema ausente, versión incompatible o hechos malformados. Validación de forma frente a comprobaciones entre artefactos. Delta de A1-A8 y de las 14 comprobaciones. Operaciones del adapter por rol y runtime. Cuándo se permite «sin huella» y quién lo autoriza (no se elimina P-01 para Codex). Elegibilidad por invocación medida y consumo cubierto (ADR-0046 #4) sin ciclos | referencia a otra unidad o corrida; versión desconocida; adapter nuevo sin cambiar el núcleo; propiedad inesperada; requisito obligatorio omitido; huella «ninguna» no autorizada |
| **R62-V1-06** Recuperación y presupuestos ejecutables | Los estados prohíben apropiaciones, pero no describen la cesión legítima ni la recuperación de cada fallo, ni la interrupción entre pasos | Tabla de transición: estado previo + evento + precondición/evidencia + decisor/escritor + transición durable + operación permitida o prohibida, incluido el rechazo de un registro obsoleto. Cubrir: Principal ausente, Worker caído, Controller sin contexto, cuota agotada, autenticación perdida, cambio de máquina, handoff ausente, artefactos transitorios, trabajo sin commit y SHA distinto. Terminación acreditada, aislamiento acreditado y falta de observación. Orden de publicación y reconciliación. Autoridad de cada contador sin «el menor» ni cero reconstruido. Topes heredados sin convertir un rebinding en tarea nueva | caída antes y después de cada publicación; dos solicitudes de recuperación; registro con SHA anterior; contador ausente o contradictorio; un relevo válido que completa la recuperación |
| **R62-V1-07** Pruebas del comportamiento | Las guardas de encabezados, enums o tablas no demuestran aceptación, agregación, transferencia, validación de hechos ni ausencia de secretos; hay riesgo de comparar un ejemplo con su propio esperado | Para cada obligación: entrada, componente o procedimiento real, acción, observable, esperado independiente, fallo legítimo, gate y clase. Diferenciar RED→GREEN de comportamiento, mutation check y guarda estructural. Comprobar AdapterFacts y las correlaciones. Probar UNKNOWN/BELOW, rechazo de binding y contradicciones. Preservar las pruebas `/v1` y su compatibilidad. Secretos: extracción mínima y saneamiento con valores ficticios en campos genéricos | — |
| **R62-V1-08** Pilotos completos, finitos y con fallo real | A permite Codex «Controller o Reviewer»; B no nombra Controller. Fixture local frente a la CI exigida. Faltan el resultado de violación observada, los topes numéricos y los oráculos por control. El aislamiento no se acredita nombrando un directorio. Falta la siguiente decisión de B. OD-2 no aparece en B | Cinco roles, acumulaciones, contexto y declaraciones por topología. Repositorio de autoridades frente a fixture: SHAs, remoto, corridas, y la CI resuelta de forma explícita. FAIL distinto de limitación. Por escenario: precondiciones, pasos finitos, esperado, oráculo, evidencia, topes y STOP. Siguiente decisión producida por B. Verificación de la separación de entradas. Negativo de hecho retirado con UNKNOWN/STOP. OD por runtime. Cobertura mínima y limitaciones admisibles sin retirar escenarios | — |
| **R62-V1-09** Estados de revisión e identidad del paquete | El paquete pide AGREED WITH CHANGES (no existe en LIFECYCLE), omite BLOCKED — OWNER DECISION y resuelve la versión con `git log -1` sobre una punta mutable | AGREED solo con cero REQUIRED; CHANGES REQUIRED o BLOCKED — OWNER DECISION; clases REQUIRED/OPTIONAL. Paquete completo + delta + disposición por ID + modo y autoría reales. Identidad fijada por el recibo de publicación o anclada a un SHA recibido. No se asigna ni se simula Architect | si la rama avanzó o el archivo cambió, el revisor revisa la versión designada o rechaza la discordancia |

**OPTIONAL O62-V1-01** (herramientas por capacidad y transporte): V1 §§4.1 y 6 convierten `gh` y el SDK en requisitos generales. Justificar su
obligatoriedad por rol, acción y transporte, o expresar la capacidad y concretar la herramienta en la receta. Sin retirar herramientas medidas ni crear
exenciones. No condiciona el acuerdo.

## 3. Orden de revisión (C62-F0-15) y publicación (C62-F0-16)

- Corregir el diseño en la misma iniciativa, rama, worktree y Claim-Id. Entregar:
  - la Proposal V2 completa (`Frozen: NO`, con el Anexo A actualizado);
  - el paquete V2 completo, con delta V1→V2 y disposición por ID;
  - este registro;
  - superficies propias alineadas, con la CI de `ea055591` en la siguiente escritura.
- Preservar V1 y su paquete como la versión revisada.
- «Se resolverá en implementación, F4 o F6» no cierra un requisito congelable. Una ampliación de alcance, una excepción reservada o un cambio de criterio
  del Owner se registra como **BLOCKED — OWNER DECISION** para ese punto, con alternativas y efecto, sin desplazar al Owner por conveniencia.
- **No autoriza:**
  - la revisión del Architect por otro runtime;
  - Controller, Worker, Reviewer o subagentes;
  - SP-1/SP-2, ampliar SP-3, invocaciones de modelos, pilotos, fixtures ejecutables, pruebas nuevas, tooling, validadores o esquemas operativos, paquetes;
  - instalaciones, credenciales, sandbox, PATH, `config.toml`, `service_tier`;
  - excepciones a I-61, números ADR, Freeze, F0 GATE PASS, F1-F7 ni integración.
- El borrador del ADR sigue como anexo sin número.
- Allowlist: Proposal V2, paquete V2, este registro, contrato, decisiones, evidencia, estado, Discovery (solo una nota puntual con procedencia) y
  `evidence/I-62-discovery/` (solo evidencia saneada).
- Validación: allowlist, enlaces, `git diff --check` y CI propia. Si la CI sigue en curso, PENDING.
- Detenerse al entregar V2.

**Estado tras la resolución:**
- G0 = GATE PASS vigente;
- Discovery R1 aceptado;
- NEW ARCHITECTURE confirmado;
- Proposal V1 = CHANGES REQUIRED;
- Proposal V2 = redacción autorizada;
- Architect = NOT REVIEWED;
- FREEZE = NOT_AGREED;
- IMPLEMENTATION AUTHORIZATION = NO.

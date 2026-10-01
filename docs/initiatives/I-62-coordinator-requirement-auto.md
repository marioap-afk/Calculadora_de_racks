# I-62 — Requisito del Coordinator: orquestación autónoma de roles (R62-AUTO-01..20) (registro)

```text
Emisor:           Coordinator exclusivo de I-62
Naturaleza:       requisito arquitectónico REQUIRED nuevo, descubierto en el uso real de I-62, y orden de la Proposal V9; no es dictamen del Architect,
                  firma del Owner, decisión del Master, Freeze ni autorización de implementación
Fecha:            2026-10-01
Fuente original:  texto pegado por el Owner en la conversación de la sesión responsable («I-62 — PROPOSAL V9 / AUTONOMOUS ROLE ORCHESTRATION»); sin archivo
                  de origen en D:\IDs. Procedencia: transcripción literal recibida (14 888 bytes en UTF-8, SHA-256
                  26eb3bd23979c437beaae7779e8c452e7109345aa3e4f679bc7b5a139214e3d9), custodiada fuera del repositorio con la transcripción de la sesión
Base:             Proposal V8, commit d66463a5b04f79911580e2ef6e6f67263b51b5c3, blob 667b59d7a433130e20a42bbc13d3e6f564350049;
                  CI 36941298033, attempt 1, push, head_sha exacto, 4/4 jobs success
Disposición:      V8 queda histórica y sin cambios, y NO se envía al Architect. Se produce la Proposal V9 como sustitución completa
Estado:           Frozen: NO · OD-6 PENDING · IMPLEMENTATION AUTHORIZATION = NO
```

Registro redactado por la sesión responsable por orden del Coordinator. La respuesta está en la [Proposal V9](I-62-proposal-v9.md), §20 y anexos B.8.8, B.9,
B.10, C-29..C-39, D.8 y F.8, y en el [paquete V9](I-62-architect-package-v9.md).

## 1. Por qué existe (resumen fiel)

El protocolo define roles, bindings, exact-SHA, reintentos, custodia y relevos. Sin embargo, el bucle de control de la iniciativa sigue dependiendo de que el
Owner transporte a mano prompts y resultados entre PRINCIPAL_COORDINATOR, ARCHITECT, EXECUTION_CONTROLLER, WORKER y REVIEWER. Así, la continuidad puede
depender del chat actual, de la memoria privada de un modelo, de que el Owner recuerde qué rol va después y de prompts copiados a mano. Eso contradice el
objetivo de portabilidad de contexto de I-62. I-61 automatizó una unidad de ejecución delegada, pero no la orquestación a nivel de iniciativa, y I-62 debe
cerrar ese hueco de forma normativa.

## 2. Requisitos REQUIRED (resumen fiel)

| Id | Contenido |
|---|---|
| R62-AUTO-01 | El Owner no es el bus normal de mensajes. El transporte entre roles IA es RELAY y debe ser automático cuando la autoridad, un binding elegible, la independencia y el presupuesto lo permiten. La interacción con el Owner es ESCALATION, solo ante fronteras reales: decisión reservada, política, acción destructiva, credenciales o compra, OV o interacción manual, ambigüedad material del Owner, presupuesto agotado o STOP asignado al Owner. «Debe ejecutarse otro rol IA» no es escalada |
| R62-AUTO-02 | En una unidad I62_DELEGATED, el Principal orquesta las invocaciones elegibles (ARCHITECT, EXECUTION_CONTROLLER, WORKER, REVIEWER, Principal sucesor) mediante adapters, sin autoridad de gate: determina el rol, resuelve el binding, invoca, valida, actualiza el estado y deriva la acción siguiente |
| R62-AUTO-03 | Contrato de invocación de rol neutral respecto del proveedor, con su lista mínima de campos. Sin texto de prompt de un proveedor; el adapter renderiza; SHA objetivo exacto obligatorio al revisar o verificar; la salida identifica el objeto exacto; identidad propia de cada invocación; permisos mínimos; los roles de solo lectura siguen siéndolo. El prompt es representación, nunca autoridad |
| R62-AUTO-04 | Contrato durable de acción siguiente: rol, acción, SHA objetivo, unidad, gate y tarea, insumos, capacidades, independencia, permiso, presupuesto, salida esperada y condiciones de cierre, STOP y escalada. Ningún hecho de continuación solo en el chat, en una memoria privada o en la del Owner, ni en un prompt copiado. Si no es derivable de forma única: STOP |
| R62-AUTO-05 | Bucle durable y acotado de revisión del Architect: objeto revisable → Architect → resultado → disposición del Principal → corrección → publicación + CI exacta → re-revisión. AGREED, CHANGES REQUIRED, BLOCKED — OWNER DECISION y resultado inválido tienen sus transiciones. CHANGES REQUIRED por sí solo no requiere al Owner |
| R62-AUTO-06 | Contrato estructurado del resultado de revisión, con su lista mínima de campos. Cada REQUIRED, con id estable, fuente, sección, evidencia y corrección. Ingestión mecánica, sin copia del Owner |
| R62-AUTO-07 | Presupuestos durables del bucle: `review_rounds`, `architect_invocations`, `correction_rounds`, correcciones por linaje y reejecuciones de transporte. Topes finitos y congelados antes de ejecutar. Cambiar de proveedor, modelo, sesión, binding, redacción o etiqueta no reinicia el linaje. El agotamiento es STOP y escalada |
| R62-AUTO-08 | Linaje: hallazgo → decisión de corrección → objeto cambiado → revisión siguiente → CLOSED, STILL_OPEN o SUPERSEDED. Solo la autoridad revisora original o una revisión posterior del Architect cierra un hallazgo arquitectónico. Una versión nueva de la Proposal no borra la historia |
| R62-AUTO-09 | El bucle Controller/Worker usa el mismo orquestador, sin relevo del Owner en las iteraciones normales, y conserva todas las reglas de I-61 |
| R62-AUTO-10 | El bucle del REVIEWER usa la misma invocación y el mismo presupuesto, sin vía manual especial |
| R62-AUTO-11 | Un Principal sucesor reconstruye un bucle activo solo desde el estado canónico (ejemplo A → B CHANGES REQUIRED → A desaparece → C reconstruye y deriva CORRECT_AND_REREVIEW) |
| R62-AUTO-12 | Piloto de autonomía real: A publica X, invoca a un Architect independiente B, B da CHANGES REQUIRED, A ingiere, corrige y publica X2, invoca a C, C da AGREED y el estado deriva la siguiente decisión o el siguiente gate. Sin copia de prompts por el Owner en los pasos 1-7. PASS exige `OWNER_AS_MESSAGE_BUS` = false; si no, UNVERIFIED o UNSUPPORTED con causa exacta. No se finge el bucle |
| R62-AUTO-13 | Invocación limpia del Architect: sesión o proceso distinto, SHA, ruta y blob exactos, solo insumos canónicos, sin transcripción ni memoria del autor, contexto inyectado declarado, solo lectura, resultado estructurado y terminación acreditada. Con la alternativa 1 de OD-6, su predicado además |
| R62-AUTO-14 | La autonomía no crea autoridad: el Principal nunca declara AGREED, cierra sus REQUIRED, elige decisiones del Owner, declara Freeze o PASS fuera de su autoridad, omite la independencia, excede el presupuesto ni omite un STOP |
| R62-AUTO-15 | El relevo manual solo se admite si el automático no está disponible o autorizado, y persiste un AUTONOMY_GAP con sus seis campos. AUTONOMY_GAP ≠ PASS del criterio autónomo |
| R62-AUTO-16 | El estado se extiende con una sección estructurada de orquestación, no con prosa en `next_action`: bucle, objeto, rol actual y siguiente, hallazgos y tareas abiertos, invocaciones, presupuestos, escalada y decisión del Owner. Sin registro global; el estado sigue siendo por unidad |
| R62-AUTO-17 | Level A primero: contratos versionados, transiciones pequeñas y deterministas, adapters existentes, archivos estructurados y orquestación gestionada por la sesión. Sin scheduler, daemon, framework genérico de agentes ni servicio en la nube |
| R62-AUTO-18 | Autoalojamiento: I-62 sigue gobernada por I-61 hasta integrarse y estas reglas no son vigentes. Se registra la fricción real de relevo manual observada; se prefiere la invocación directa cuando la autoridad vigente la permite y, si no, se registra un AUTONOMY_GAP |
| R62-AUTO-19 | Coherencia de V9 en objetivos, autoridad, roles, portabilidad, estado, contratos, fallos, presupuestos, gates, matriz, pilotos, OV, paquete y triaje de I-61, con nueve obligaciones de verificación (1-9) |
| R62-AUTO-20 | No imponer esta maquinaria a las unidades DIRECT_ONLY; aplica a las I62_DELEGATED y a los contextos de revisión o piloto que la adopten |

## 3. Disposición y autorización del Coordinator

- Es una adición arquitectónica material descubierta antes del Consensus Freeze.
- La Proposal V8 queda histórica; **no** se invoca al Architect sobre V8.
- La Proposal V9 incorpora este requisito y todos los cierres de V8. `Frozen` sigue en NO, OD-6 sigue PENDING e IMPLEMENTATION AUTHORIZATION sigue en NO.
- **Corrección autónoma**, sin aprobaciones intermedias salvo una ambigüedad de verdad reservada al Owner.
- **Salidas:** Proposal V9 completa; paquete V9; este registro; contrato, decisiones, evidencia y estado propios; delta V8→V9; commit y blobs exactos; CI exacta.
- **Prohibido:** editar normas compartidas, implementar, ejecutar pilotos, publicar esquemas nuevos en producción, tomar decisiones del Owner o declarar el
  Freeze.
- **Tras publicar:** STOP con la CI exacta en verde, sin pedir al Owner que transporte un prompt de revisión. El informe debe decir cómo se incorporaron
  R62-AUTO-01..20, qué AUTONOMY_GAP se descubrieron en I-61 y cuál es la acción siguiente exacta. El Coordinator autorizará después una revisión formal limpia
  del Architect sobre V9.

**Estado declarado:**
- Proposal V8 = publicada técnicamente, sustituida para la revisión por este requisito nuevo;
- Proposal V9 = AUTHORIZED FOR ARCHITECTURE CORRECTION;
- OD-6 = PENDING OWNER DECISION; FREEZE = NOT_AGREED; IMPLEMENTATION AUTHORIZATION = NO;
- I-61 REMAINS THE ACTIVE PROTOCOL UNTIL I-62 IS INTEGRATED.

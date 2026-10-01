# I-61 — Proposal V9: protocolo de ejecución delegada de agentes

```text
Frozen: YES
Version: V9 (sustituye a V8 en la revisión; las versiones V1-V8 se conservan sin cambios)
Unit: I-61   Workflow: V2 (T4)   Claim-Id: 0e2923de-e1a7-41bf-b7db-50ec84217850
Archetype: NEW ARCHITECTURE (Q-01)   Base: origin/main 95690c28   Branch base: 07ef2a85
Author: EXECUTOR / DESIGN AUTHOR (Claude, sesión responsable de I-61)
Review: rondas Coordinator ↔ Architect en SAME-SESSION ROLE hasta AGREED (LIFECYCLE §5); rondas 1-7 (V1-V7) = CHANGES REQUIRED; ronda 8 (V8) = AGREED, cero REQUIRED
```

Diseño **autocontenido** que se congela (LIFECYCLE §6). La **fuente del alcance** es el mandato del Owner
([I-61-owner-mandate.txt](../automation/decisions/I-61-owner-mandate.txt)); A–G se citan por referencia. Hechos y decisiones de partida: [Discovery](I-61-discovery.md), incluidos
G1-C y §§17-20; [decisiones](../automation/decisions/I-61.md) §§7-12; y [evidencia](../automation/evidence/I-61-evidence.md) §12 (fuentes oficiales del diseño y autenticación de la
sesión). Adjudicaciones: [ronda 1](I-61-design-review-r1.md), [ronda 2](I-61-design-review-r2.md), [ronda 3](I-61-design-review-r3.md), [ronda 4](I-61-design-review-r4.md), [ronda 5](I-61-design-review-r5.md), [ronda 6](I-61-design-review-r6.md), [ronda 7](I-61-design-review-r7.md) y [ronda 8](I-61-design-review-r8.md). No repite normas: las cita.

## 0. Delta

**V1 → V2** (ronda 1): cambio de autoridad OWN-L declarado; cláusulas de AUTOMATION_PLAN enumeradas sin Pull Request; tabla regla → dueño; relevo con orden de commits, procesos vivos y
aceptación contra el contrato; elegibilidad por celda con consumo cubierto y `RoutingEnforcement`; cinco esquemas con `Owner`, `Disposition` y comprobaciones fijas; conteo determinista y
topes; tabla STOP; piloto con U-04, RED compilable y controles negativos específicos; OBL-09..11; matriz OV con la forma de la guía §7.2; criterios de éxito; fila del índice de ADR en el
commit de cierre.

**V2 → V3** (ronda 2):

- **Autoridad:** AUTOMATION_PLAN cambia también sus frases de alcance; §2.2 enumera todas sus secciones y el ADR remite a ella; las autoridades ajenas a la unidad se leen en `MainSha`
  y una rama no puede rebajarlas (§2.1, §9).
- **Consumo:** una sola regla de evidencia para el consumo cubierto, igual para Controller y Worker; el `service_tier` heredado pasa a riesgo registrado con mitigación; se retira la
  cronología errónea de V2 (§4.3, §11).
- **Mecánica:** procesos vivos con lista cerrada de nombres y exclusión estricta (§3.3); un modelo único de `RunId` (§3.5); recuperación tras rebase (§3.6); bucles de corrección y
  recuperación en el flujo de G3, tope de invocaciones con corrección y STOP P-07 (§12.1, §16.2); disposiciones de §8.3 alineadas con §10.1; RED solo en la primera delegación de la
  cadena; `attempts` acotado a la ejecución delegada (§9).
- **Controles:** nc1..nc3 solo sobre la verificación VERIFIED final, con oráculo relativo; nc4 evaluado antes de aceptar la delegación real, sin cortocircuito; oráculo contra Git en U-04;
  disposición de cada condición de OBL-09; definición de «ruta operativa» en OBL-04.

**V3 → V4** (ronda 3):

- **Autoridades:** tres clases (documentos de la unidad, cambios normativos de la unidad por sección y autoridades ajenas) con comprobación **por sección**; la tabla de aplicación de
  AUTOMATION_PLAN pasa también a su §16 (§2.2, §2.3, §8.3 `Authority`, §9).
- **Rebase:** la imagen de `AuthorityRevision` entra en el `RebaseMap`; ninguna escritura de la sesión entre la verificación del STOP S-13 y la reverificación, salvo el propio rebase; reemisión
  del contrato; máximo de dos recuperaciones por tarea; un fallo de A6 por avance de `main` es S-13 (§3.4, §3.6).
- **RED de la cadena:** se exige hasta que haya un RED **acreditado**; en una corrección, el RED es un commit con la corrección desactivada (§9).
- **Disposiciones:** un `not_run` heredado de un `fail` anterior no aporta disposición (§8.3); nc2 muta un archivo del diff verificado; los fallos de transporte de los controles se
  reejecutan dentro del tope (§12.1).
- **Sondas y DLL:** la autenticación de los subagentes queda medida y versionada (evidencia §12); oráculo contra Git en PR-1; selección de celdas de U-04; efforts probados por celda; los
  DLL legacy se construyen con `SourceRevisionId` y su `ProductVersion` debe identificar el SHA (§4.3, §12.1, §16).

**V4 → V5** (ronda 4):

- **Effort heredado:** una celda sin control de effort registra el effort **heredado medido** y `Routing` lo compara (§4.3).
- **RED:** las partes RED de `Ci` y `Tests` se deciden por el `ChainRedSha` de la delegación y se registran en un campo `RedPart`; el RED acreditado **caduca** si una delegación toca los
  archivos de prueba con RED esperado (§8.2, §8.3, §9).
- **Ajustes de las lentes A y B:** `AuthorityRevision` = SHA de cierre de G2 o su imagen; definición de sección; referencias de la tabla al copiarla en AUTOMATION_PLAN §16; rebase con
  conflictos → `--abort` y STOP; `Identity` tras rebase contra los SHA originales; oráculo relativo de nc4; tope por control; servidor de Roslyn y procesos huérfanos; lectura de
  `--force-with-lease` (§2.2, §3.3, §3.6, §8.3, §9, §12.1).

**V5 → V6** (ronda 5):

- **RED anclado en Git:** se retiran `Files[]` y la caducidad decidida en la aceptación. Los archivos RED de la cadena salen de Git (`git diff --name-only RedSha^ RedSha -- tests/`
  del RED acreditado), y la verificación exige RED nuevo si el diff de la delegación los toca. `ExpectRed` se compara siempre con el contrato (§3.4 A5/A8, §8.2, §8.3, §9).
- **`AuthorityRevision`:** el cierre de G2 o un commit posterior que solo añade documentos de la unidad (A-n, decisiones), o la imagen de cualquiera de ellos; todo rebase posterior a G2,
  incluido el de WORKFLOW §4 al abrir sesión, se registra con `RebaseMap` y el Coordinator reemite el contrato (§3.6, §9); `Authority` también comprueba el tramo
  `AuthorityRevision..BaseSha` (§8.3).
- **Procesos:** definición operativa de huérfano (§3.3). **Ajustes:** celda (no modelo) sin control de effort, catálogo y criterio 2; `RedPart` con corrida ausente → BLOCKED; destino
  del orden de commits y de la recuperación en §2.3.

**V6 → V7** (ronda 6):

- **RED de la cadena:** `ChainRedFiles` se calcula desde `ChainBaseSha` (base de la primera delegación de la tarea) hasta el `RedSha` acreditado y nunca decrece; las pruebas no
  cambian después del commit RED; se retira «un único commit» (§8.2, §8.3 fila 10, §9, §12.1).
- **`AuthorityRevision`:** X es válido si su diff desde el cierre de G2 (o su imagen) no toca rutas `EXTERNAL` ni rutas con secciones `UNIT_CHANGE`, el mismo predicado que la comprobación
  `Authority` (§9). Los commits de la sesión tampoco pueden tocarlas (§3.1).
- **Ajustes:** reemisión del contrato tras todo rebase posterior a G2 y registro `REBASE` de apertura de sesión (§3.4, §3.5, §3.6); igualdad por secciones en el `RebaseMap`; huérfano
  observable; corrida del `RedSha` ausente con la de `CurrentSha` terminada → REWORK.

**V7 → V8** (ronda 7):

- **RED:** `RT` se mide desde `ChainBaseSha`; el **RED vigente** pasa a `null` tras una entrega con `RedPart` = `fail` y `ChainRedFiles` se conserva (§9).
- **Rebase:** el `RebaseMap` registra el cierre de G2 y su imagen, y su descripción en el esquema del registro de relevo queda alineada con §3.6; la reverificación calcula la parte RED con
  los SHA originales; el rebase de apertura con una cadena en curso pertenece a esa tarea (§3.5, §3.6, §8.2, §9).
- **Paso 7** alineado con la fila 9 de §8.3; `CreationDate` en la orden de procesos (§3.3, §12.1).
- **`AuthorityRevision`:** `B` admite la `AuthorityRevision` vigente; sin contrato, el `RebaseMap` toma el cierre de G2 como `AuthorityRevision` (§3.6, §9).

**V8 → V9** (ronda 8: AGREED en las cuatro lentes, cero REQUIRED; se aplican sus 11 OPTIONAL, de redacción o alineación, y uno, B-R8-03, que corrige a UTC la comparación de
`CreationDate`; la confirmación acotada añade cómo cuenta en el tope de invocaciones un rebase de apertura con cadena en curso): descripción de `ChainRedSha` en el esquema de delegación
como RED vigente; pasos 6.1-6.2 con `RT` ∪ `ChainRedFiles`; el paso 7 solo registra hechos y la disposición es la de la fila 9; definición de «cadena en curso» para el rebase de apertura,
que no consume el tope de recuperaciones; `CreationDate` comparada en UTC; rutas en conflicto en el `RebaseMap`; redacción del inciso de `B`.

## 1. Objetivo verificable y no-objetivos

**Objetivo.** Un protocolo estable y subordinado a las autoridades vigentes con el que:

- un Coordinator delega un gate mediante un **contrato de gate** legible por máquina;
- un **Controller Codex** clasifica y enruta la tarea dentro de ese contrato, emite un **paquete de delegación**, recibe una **entrega de worker** y la verifica de forma independiente y
  fail-closed;
- se preservan exact-SHA, la propiedad exclusiva de escritura y la autoridad del Coordinator.

Se demuestra con **un piloto real** (§12). Los 14 criterios de éxito del mandato se concretan en §18.

**No-objetivos:**

- los del mandato («NO PRODUCT SCOPE CREEP»);
- ninguna autoridad paralela (Q-03);
- ningún cambio en reglas de evidencia, estados, enums de gate, esquema de estado v1, contratos ajenos, `AGENTS.md`, `CLAUDE.md`, `.gitignore`, `.gitattributes`, CI ni
  `INITIATIVE_LIFECYCLE.md`;
- ningún Pull Request como parte de la ejecución delegada (§2.2);
- ningún script operativo ni plataforma (nivel A, §14); las recetas son órdenes documentadas que ejecuta la sesión;
- ninguna selección automática de iniciativas ni recurrencia;
- no convertir el piloto en benchmark entre proveedores (C61-G0-06).

## 2. Autoridades y límites

### 2.1 Cambio del alcance de autoridad (OWN-L)

Hoy la operación «ejecución delegada» no tiene dueño (EXP-02). La [Proposal V4 de I-56](I-56-proposal-v4.md) P-17, aprobada por el Owner en OWN-L, dice que AUTOMATION_PLAN
«solo implementa su ejecutor». Esta Proposal **amplía** ese alcance: AUTOMATION_PLAN pasa a gobernar también la ejecución delegada bajo orden explícita del Coordinator, y WORKFLOW §10
recibe la fila correspondiente. Es un **cambio del alcance de autoridad, materia OWNER-RESERVED (OWN-L)**, que deriva de Q-03 del Coordinator. Su efecto queda **sujeto a la aceptación
de ADR-0046 por el Owner y a la integración**. Hasta entonces, la autoridad para aplicar el protocolo en el piloto es la del Owner (mandato «PILOT»; decisiones §9.1, cláusulas 2-4 y 8).
Materialidad M-08 en §17.

| Dominio | Dueño | Cambio que se prepara en esta rama |
|---|---|---|
| Operación del ejecutor y **ejecución delegada** | `AUTOMATION_PLAN.md` (ampliado, OWN-L) | (1) **§16 «Ejecución delegada bajo orden del Coordinator»**, con el texto normativo de las reglas de §2.3, incluida la tabla de aplicación de §2.2; al copiarla, sus referencias a secciones de esta Proposal se reescriben a sus destinos dentro del plan y el preámbulo se nombra por su contenido, no por números de línea. (2) Línea 3: «Este documento define solamente el ejecutor de iniciativas» pasa a «Este documento define el ejecutor de iniciativas y, en §16, la ejecución delegada bajo orden explícita del Coordinator». (3) Último párrafo de §2 «Autoridad y referencias obligatorias»: «Este plan solo decide selección y operación del ejecutor dentro de esas reglas» pasa a «Este plan decide la selección y operación del ejecutor y, en §16, la ejecución delegada, dentro de esas reglas». (4) §2 «Modo normal» recibe la **frase de ejecución delegada**: «La ejecución delegada de §16 no es una ejecución de este modo: lee en `AuthorityRevision` los documentos de su propia unidad y los cambios normativos que su orden declara por sección, y todas las demás autoridades en `MainSha`; una rama no puede rebajar una regla global». (5) El **título se conserva** («Plan del ejecutor nocturno»); §16 declara que la ejecución delegada no es el ejecutor nocturno |
| Git, relevo, worktrees, cadencia documental | `WORKFLOW.md` | §3 **sin cambios**. §10 añade, **por encima de la fila «Prompts»**, la fila «Operación del ejecutor y ejecución delegada → `AUTOMATION_PLAN.md`, dentro de las reglas de los dueños anteriores; subordinados: `docs/automation/agent-execution/`». PROMPT_TEMPLATES §G se cita solo en la fila «Prompts», que ya la cubre. Cumple WORKFLOW §8 |
| Prompting | `PROMPT_TEMPLATES.md` | **§G «Delegación de ejecución»**. La cabecera añade «y, para §G, a `AUTOMATION_PLAN.md` §16». §2 cita como fuente aprobada el Freeze de I-61 (Proposal congelada) y AUTOMATION_PLAN §16; ADR-0046 se cita solo cuando esté aceptado. No renombra A–F |
| Evidencia | `AGENTS.md` | **Ninguno.** Ni la entrega de worker ni la verificación del Controller son evidencia de gate |
| Ciclo de diseño y gates | `INITIATIVE_LIFECYCLE.md` | **Ninguno.** La regla de §3.1 no añade condiciones de cierre de gate |
| Decisión de fondo | ADR | **ADR-0046 `propuesto`** (C61-G1-04), creado con la Proposal y antes de implementar. Ni el piloto ni el Freeze se apoyan en él como autoridad: WORKFLOW §10 solo reconoce ADR aceptados |

### 2.2 Secciones de AUTOMATION_PLAN y su aplicación a la ejecución delegada

«Ejecutor» se lee así: la **sesión responsable** para el estado, el informe, el relevo, el build local, el paquete de OV, las decisiones del Owner y la custodia; el **Worker** para sus
commits, su trailer y su alcance; el **Controller** no escribe en Git.

| Sección (nombre y número vigente) | Aplica | Exclusión o lectura |
|---|---|---|
| Preámbulo sin título (antes de «Estado actual de activacion») | sí | Subordinación a las demás fuentes y uso de los archivos de estado y de decisiones |
| Estado actual de activación, §1 «Proposito», §4 «Estado derivado», §5, §6, §14 y §15 | no | Selección, reclamo e informe nocturno |
| §2 «Fuentes del plan y modos de ejecucion» | solo «Autoridad y referencias obligatorias» y la frase de ejecución delegada de «Modo normal» | Los modos bootstrap y normal rigen al ejecutor nocturno |
| §3 «Limites de seguridad» | viñetas 1, 2, 3 (sin la exigencia de `automation.enabled: true`), 4 (los hallazgos laterales van a `docs/ideas-futuras.md` o a la evidencia de la unidad, **no** a un Pull Request), 5, 6, 7 y 8 | En la viñeta 2, la recuperación de §3.6 se lee como reanudar tras el rebase exigido por WORKFLOW §4 (W-04), que admite `--force-with-lease` |
| §7 «Reclamo atomico y worktree» | no | Solo como precedente de las comprobaciones operativas del relevo (§2.3) |
| §8 «Implementacion, estado versionado y Pull Requests» | primer párrafo (commits con asunto en español, porqué y trailer; rebase al reanudar) y la forma y las reglas de campos del archivo de estado | **No** aplica abrir, reutilizar ni actualizar un Pull Request. «Publicar al terminar cada ejecución» se lee como los commits de estado del orden de §3.2. La lectura de `attempts` en tareas delegadas está en §9 |
| §9 «CI fallido y reintentos» | definición de intento, máximo `automation.max_attempts` y detención antes del límite | **No** aplica la actualización del PR |
| §10 «Build local y AutoCAD» | completa, para la sesión responsable | El Worker nunca usa AutoCAD |
| §11 «Decisiones del dueno» | completa, para la sesión responsable, incluida la metadata monotónica | — |
| §12 «Condiciones obligatorias para detenerse» | viñetas 2-8, según la tabla de §10.3 | **No** aplica la viñeta 1 |
| §13 «Prohibicion de merge automatico» | completa | — |

### 2.3 Regla → documento dueño

| Regla | Texto normativo | Procedimiento (sin texto normativo propio) |
|---|---|---|
| Definición de ejecución delegada, roles y declaraciones; cuándo termina un trabajo delegado | AUTOMATION_PLAN §16 | README |
| Aplicación de las secciones de AUTOMATION_PLAN a la ejecución delegada y lectura de «ejecutor» (tabla de §2.2) | AUTOMATION_PLAN §16 | — |
| Lectura de autoridades por clase (§9) | AUTOMATION_PLAN §16 y la frase de ejecución delegada | README (órdenes de la comprobación `Authority`) |
| Precondiciones operativas del relevo (procesos vivos, una delegación abierta, hash de `config.toml`, aceptación del paquete), orden de commits (§3.2) y recuperación tras rebase (§3.6), en aplicación de WORKFLOW §3 y con el precedente de AUTOMATION_PLAN §7 «worktree ocupado» | AUTOMATION_PLAN §16 | README (órdenes exactas) |
| Propiedad exclusiva, regla de conteo, topes, precedencia STOP > BLOCKED > REWORK y tabla STOP | AUTOMATION_PLAN §16 | README (escenarios) |
| Regla multiseñal y comprobaciones obligatorias | AUTOMATION_PLAN §16 | README; esquema `controller-verification` |
| Custodia: los artefactos que respaldan una decisión se citan por su blob versionado; aplicación de WORKFLOW §11.4 (evidencia por unidad), **sin** condición nueva de cierre de gate | AUTOMATION_PLAN §16 | README |
| Enrutamiento estable | `routing.md` (subordinado; procedimiento de decisión, no política) | — |
| Composición de prompts | PROMPT_TEMPLATES §G | — |

**Subordinados nuevos** en `docs/automation/agent-execution/`: `README.md` (procedimiento, recetas y escenarios), `routing.md` (enrutamiento estable), `model-catalog.md` (catálogo
**mutable y no normativo**), `prompting-guide.md` (entregable A) y `schemas/` (cinco `.schema.json`, §8).

**Cláusula de subordinación**, obligatoria en cada subordinado (alineada con ADR-0046): no crean requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un
conflicto manda la autoridad del dominio y el conflicto se eleva como STOP.

`documentation-governance.md` recibe una línea que refleja esta división. La vigencia llega con la integración y la aceptación de ADR-0046. Dentro de I-61 se aplica solo en su piloto y
**sin rebajar** ninguna regla vigente. Ninguna otra iniciativa lo adopta por estar escrito.

## 3. Roles, declaraciones y relevo (D-01, D-05, C61-G1-01)

### 3.1 Roles

| Participante | Puede declarar | Nunca declara |
|---|---|---|
| Worker (Claude o Codex) | `IMPLEMENTATION_COMPLETE`, `PARTIAL`, `BLOCKED` | verificación, GATE PASS, Candidato, cierre, integración |
| Controller (Codex) | `EXECUTION_VERIFIED`, `EXECUTION_REWORK_REQUIRED`, `EXECUTION_BLOCKED` | GATE PASS, Candidato, cierre, integración |
| Coordinator | GATE PASS conforme a LIFECYCLE §7 | Candidato, cierre o integración fuera del workflow |
| Sesión responsable (la que tiene asignado el worktree) | hechos del relevo, del remoto y del rebase (registro de relevo) y la disposición de un fallo de transporte | `EXECUTION_*` |

**Trabajo delegado terminado.** Un trabajo delegado vuelve al Coordinator como terminado **solo** con `EXECUTION_VERIFIED` cuyo `VerifiedSha` es el `CurrentSha` de la entrega o, tras
un rebase registrado (§3.6), su imagen; y con el SHA evaluado del gate igual a ese SHA más commits de la sesión limitados a `docs/automation/` y a los documentos propios de la unidad,
**sin** rutas `EXTERNAL` ni rutas con secciones `UNIT_CHANGE` (`git diff --name-only` comprobado por el Coordinator; tocarlas exige un contrato nuevo). Con REWORK, BLOCKED o STOP el trabajo delegado no ha terminado y no hay trabajo delegado que revisar. El Coordinator puede rechazar
un VERIFIED. Esta regla define cuándo termina la delegación; **no** añade condiciones a LIFECYCLE §7, cuyo punto 5 sigue siendo la revisión del Coordinator.

La verificación (`EXECUTION_*`) es **exclusiva del Controller**. La sesión responsable solo completa las comprobaciones del relevo y registra hechos. **Alternancia de roles:** la sesión
Claude puede actuar como Coordinator, Architect y Executor (`SAME-SESSION ROLE`), pero no se presenta como Codex.

### 3.2 Relevo y orden de commits

Cada invocación de un participante externo (Codex) es un relevo de [WORKFLOW](../WORKFLOW.md) §3, sin modificarlo, conforme a la cláusula 5 del Owner (C61-G1-01). Un Worker
**subagente** de la sesión es alternancia interna de una sola sesión responsable (cláusula 5), no un relevo entre sesiones; aun así se le aplican las mismas comprobaciones de salida, la
cesión, la entrada y el registro. Se invoca con una llamada cuya finalización notifica la orquestación de la sesión, con **tope de 60 min**: la sesión no opera hasta recibir esa
notificación con el resultado final; al vencer el tope detiene la tarea y confirma su estado terminal antes de volver a operar. No se usan subagentes que sobrevivan a su llamada.

1. **Relevo de salida**, antes de cada invocación:
   - `HEAD` = `origin/<rama>` comprobado con `git ls-remote`, y el **último commit lleva en su cuerpo el resumen de estado** de WORKFLOW §3. Si ambas cosas ya se cumplen, el relevo se
     satisface **sin commit nuevo**;
   - árbol limpio y ninguna operación Git en curso;
   - `git fetch` y registro de `origin/main`;
   - comprobación de procesos vivos (§3.3);
   - una sola delegación abierta para la unidad (§3.5);
   - SHA-256 de `~/.codex/config.toml` y la lista ordenada de sus nombres de secciones y claves, **sin valores**.
2. **Cesión.** Durante la invocación la sesión **no opera** sobre el worktree: no lee, no escribe ni ejecuta, **incluidos sus propios subagentes**. Registra inicio y fin (UTC). No se
   reinterpreta «sesión activa» (D-01): una sesión que completó el relevo y no opera ha **cedido** el worktree; la etiqueta «esperando» no basta.
3. **Relevo de entrada:** confirmación de que el participante terminó (§3.3), las verificaciones de WORKFLOW §3 (rama, upstream y divergencia, `git status`, `git log --oneline -5`) y la
   comparación de `config.toml`. **Un hash distinto es STOP** (P-01): ninguna invocación de Codex más hasta que decida el Owner; se registra el diff de nombres de secciones y claves.

**Orden de commits congelado en una tarea delegada:**

1. commit de estado de la sesión (si hace falta), con el resumen de WORKFLOW §3, y push;
2. relevo de salida → Controller de planificación (sin commit);
3. evaluación de nc4 y aceptación del paquete (§3.4), sin commit;
4. Worker: commit RED y push (si la entrega va a exigir RED, §9); commit GREEN y push, con el resumen de estado en el cuerpo del **último** commit;
5. relevo de entrada y hechos remotos: **ninguna escritura Git de la sesión** desde el push del Worker hasta terminar la verificación, la reverificación de §3.6 si la hay y, si
   procede, los controles negativos. La única excepción es el rebase de §3.6;
6. verificación del Controller (relevo sin commit, por el punto 1);
7. después, el commit de la sesión con custodia, estado, `attempts` y evidencia.

Tras un REWORK se vuelve al punto 1 con el commit de `attempts` (§9). Tras un STOP por avance de `main` se aplica §3.6, **sin** el commit del punto 7 hasta terminar la reverificación.

### 3.3 Procesos vivos (criterio operativo)

Orden registrada: `Get-CimInstance Win32_Process | Select-Object ProcessId, ParentProcessId, Name, CommandLine, CreationDate`.

- **Se evalúan** los procesos con `CommandLine` legible y, entre los ilegibles, solo los de la **lista cerrada** de nombres: `codex*.exe`, `claude.exe`, `node.exe`, `git.exe`, `pwsh.exe`,
  `powershell.exe`, `bash.exe` y `dotnet.exe`.
- **Participante sobre el worktree:** un proceso evaluado cuya `CommandLine` contiene la ruta del worktree (sin distinguir mayúsculas, con `\` o `/`) o `-C <worktree>`.
- **Exclusión estricta:** solo el proceso que ejecuta la comprobación y sus ancestros por `ParentProcessId`. Nunca sus hermanos ni otros descendientes de la sesión.
- **Participante lanzado:** su PID se registra en el `Outcome` del registro; en el relevo de entrada, ni él ni ningún descendiente suyo pueden seguir vivos.
- **Worker subagente** (sin PID propio): el `Exit` registra los PID descendientes de la sesión; en el `Entry`, todo descendiente nuevo que siga vivo y no sea la cadena del
  comprobador ni un servidor de compilación identificado por nombre (`VBCSCompiler.exe`; `MSBuild.exe` o `dotnet.exe` con `/nodemode`, `build-server` o `VBCSCompiler.dll` en su línea de
  órdenes) → STOP (P-02). **Huérfano:** proceso de la lista cerrada con `CreationDate` dentro de la ventana de la
  cesión cuyo `ParentProcessId` no existe en el `Entry`, o existe con una `CreationDate` posterior a la del hijo (PID reutilizado); vivo en el `Entry` → STOP (P-02), con las mismas exclusiones
  y la regla de los `codex*.exe` del Owner. Los demás procesos creados en la ventana se clasifican con el criterio general de esta sección. `CreationDate` se registra y se compara en
  **UTC** (`ToUniversalTime()`), igual que la ventana de `Cession`.
- **Procesos `codex*.exe` del Owner:** si su línea de órdenes es legible y no contiene la ruta del worktree, no son participantes; se registran solo PID y nombre. Si alguno la contiene →
  **STOP al Owner**, sin tocarlo.
- **Exclusión nominal:** `codex-windows-sandbox-service.exe` (servicio, línea de órdenes ilegible, MEASURED) se registra por PID y no cuenta como participante.
- Un participante vivo ajeno, o un proceso ilegible de la lista cerrada que no sea la exclusión nominal, es **STOP (P-02)**.
- Tras un tope de tiempo se mata el árbol del proceso lanzado (`taskkill /PID <pid> /T /F`) y se **confirma su muerte** repitiendo la orden; sin confirmación no se lanza otro escritor.

### 3.4 Aceptación del paquete contra el contrato del Coordinator (A-01, B-11)

El **contrato de gate** (`gate-contract.json`, esquema de §8) lo redacta el **Coordinator**. Contenido mínimo congelado: objetivo; alcance permitido y prohibido (incluido el no-touch);
invariantes del Freeze por ID; pruebas requeridas (filtro, mínimo seleccionado y si se espera RED); condiciones STOP adicionales; autoridades (ruta, sección y clase de §9) y
`AuthorityRevision`; celdas elegibles con sus efforts probados (§4.3); `RoutingEnforcement`; y autorización de correcciones. El Coordinator **reemite** el contrato tras **todo** rebase posterior al cierre
de G2 (§3.6), antes de la siguiente delegación; el `RebaseMap` solo sustituye a la reemisión para la reverificación de la misma entrega en el caso sin conflictos. La reemisión cierra
las delegaciones abiertas del contrato anterior.

Antes de invocar al Worker, la sesión, en rol de Coordinator, evalúa **todas** las comprobaciones, **sin cortocircuito**; la disposición es la más grave de las fallidas:

| ID | Comprobación | Si falla |
|---|---|---|
| A1 | Válido contra su esquema (`Test-Json`) | BLOCKED (planificación) |
| A2 | `Unit`, `Gate` y `TaskId` iguales a los del contrato; `RunId` = el de un registro de planificación `COMPLETED`, y ninguna otra delegación aceptada lo tiene | BLOCKED |
| A3 | `AllowedWriteScope` ⊆ alcance permitido del contrato (sintaxis de §8.1) | **STOP** (alcance, P-03) |
| A4 | `ForbiddenWriteScope` ⊇ prohibido del contrato | **STOP** (P-03) |
| A5 | `Invariants`, `StopConditions` (por ID), `Authorities` y `RequiredTests` ⊇ contrato, con `MinSelected` ≥ y el **mismo** `ExpectRed` que el contrato | **STOP** (P-03) |
| A6 | `AuthorityRevision` = la del contrato; `MainSha` = `origin/main` recién obtenido; `BaseSha` = `HEAD` = `origin/<rama>`; rama y worktree esperados | `MainSha` ≠ `origin/main` → S-13, caso «antes de escribir» de §3.6 (no consume el tope de BLOCKED); lo demás → BLOCKED (identidad) |
| A7 | `Executor` y `Model` corresponden a una celda elegible del contrato, no `STALE` en la fecha de la delegación, y `Effort` está entre los efforts probados de esa celda | BLOCKED |
| A8 | `Attempt`, `AttemptsRemaining`, `MaxReworkLoops`, `RoutingEnforcement` y `ChainBaseSha`, `ChainRedSha` y `ChainRedFiles` según §9 (la base de la primera delegación de la tarea; el RED vigente o `null`; y `ChainRedFiles` custodiado, vacío solo si la cadena nunca acreditó RED); `ExpectedHandoffPath` dentro del directorio del intento; `Owner.Kind` coherente con `Executor.Transport` | BLOCKED |

Ningún fallo invoca al Worker. El Controller de verificación repite A3-A5 (comprobación `Contract`, §8.3). Una delegación más amplia que el contrato se rechaza antes de invocar
(INV-10, OBL-10, control nc4 de §12.1).

### 3.5 Identidad de las invocaciones, delegación abierta y responsabilidades G.1..G.12

- **`RunId`** identifica **una** invocación de participante. Lo asigna el relevo con la forma `R<yyyyMMddTHHmmssZ>-<4 hex>` y el participante lo repite en su salida. Las reejecuciones
  reciben uno nuevo y nunca sobrescriben.
- **Delegación:** su `RunId` es el de la invocación de **planificación** que la emitió (`DelegationRunId`). `Owner.Id` = `<Kind>:<DelegationRunId>`: identifica el único puesto de
  escritor de esa delegación; el PID o la invocación concreta quedan en el registro de relevo.
- **Entrega:** su `RunId` es el de la invocación de **trabajo** (nuevo en cada reejecución) y lleva `DelegationRunId`. `ExpectedHandoffPath` contiene el token literal `{WorkRunId}`,
  que el relevo sustituye al componer el prompt.
- **Verificación:** su `RunId` es el de su invocación y lleva `DelegationRunId` y `WorkRunId`.
- **Rebase** (§3.6): su registro recibe un `RunId` nuevo asignado por el relevo y vive en el directorio del intento; la reverificación lo cita. El rebase de apertura de sesión **sin
  cadena en curso** vive en `artifacts/orchestration/<unit>/session-rebase/<RunId>/`, con `TaskId` = `SESSION` y el `Attempt` vigente del estado; si hay una **cadena en curso**
  (una tarea con al menos una delegación aceptada que no terminó en VERIFIED ni en abandono declarado, incluida una corrección autorizada aún no emitida), el rebase pertenece a esa tarea
  (directorio del intento, §3.6, con `BaseSha`, `ChainBaseSha` y sus imágenes). Un rebase de apertura no consume el máximo de 2 recuperaciones de §3.6, pero **sí** añade al tope de
  invocaciones de §16.2 lo mismo que una recuperación (una reverificación, o una planificación y una verificación).
- **Delegación abierta:** una delegación aceptada sin una verificación válida registrada ni un cierre declarado por la sesión (STOP o abandono) en su registro de relevo. Como máximo una por
  unidad.
- **G.1..G.12** se asignan como en el Discovery §20.4. **G.7** («invoke worker») y **G.11** («route corrections»): los ejecuta la sesión responsable como relevo, **exactamente** según el
  paquete aceptado. Cualquier divergencia entre invocación y paquete es STOP (P-05). Limitación declarada: el Controller de solo lectura no puede invocar en este entorno.
- **Autorización de correcciones:** solo las autoriza el contrato (`CorrectionsAuthorized`) dentro de la regla de §9. El **análisis** de un defecto nuevo (§9) y la **investigación de
  causa raíz** tras un STOP los redacta el Coordinator en `analysis.md` del directorio del `RunId` que los motiva; se versionan en la custodia.

### 3.6 Recuperación tras un STOP por avance de `main` (S-13)

**Todo rebase de la rama de la unidad posterior al cierre de G2**, incluido el que WORKFLOW §4 exige al abrir una sesión, lo hace la sesión como Executor (`--force-with-lease`) y lo
registra en un registro de relevo con `Phase: REBASE` y `RebaseMap` (sin commits del Worker si no los hay); antes de la siguiente delegación, el Coordinator reemite el contrato con la
imagen de `AuthorityRevision`. Dentro de una tarea, los casos son los de abajo. El `RebaseMap` contiene:
`MainSha` anterior y nuevo; el **SHA de cierre de G2** y su imagen; `AuthorityRevision` (la del último contrato o, si aún no hay contrato, el cierre de G2) y su imagen
`AuthorityRevision'`, con igualdad de los blobs de las rutas `UNIT_DOC` y del texto de cada sección `UNIT_CHANGE` (con espacios normalizados, como en §8.3 fila 3); `BaseSha` y
`ChainBaseSha` (§9) con sus imágenes, o `null` fuera de una tarea; y cada commit del Worker con su imagen y la igualdad de `git patch-id`. Si alguna de esas **igualdades** (blobs
`UNIT_DOC`, secciones `UNIT_CHANGE`, `patch-id`) falla, STOP al Coordinator, salvo en el rebase que ejecuta una decisión registrada del Coordinator con A-n (§3.6, con conflictos), cuyo
`RebaseMap` cita esa decisión en lugar de exigir la igualdad de `patch-id`.

- **Antes de escribir:** rebase, **reemisión del contrato** con `AuthorityRevision'` y el `MainSha` nuevo, y delegación nueva del mismo `TaskId`.
- **Después de escribir** (la punta es `CurrentSha`, porque la sesión no escribió desde el push del Worker, §3.2):
  - **Sin conflictos:** la punta rebasada es `CurrentSha'`. **Reverificación** del Controller sobre la misma delegación y la misma entrega, con el `RebaseMap` como entrada. Las
    comprobaciones usan las imágenes (`AuthorityRevision'`, `BaseSha'`, `CurrentSha'`) y el `MainSha` nuevo; `Identity` exige además la igualdad de `patch-id`; `Ci` usa la corrida
    `push` de `CurrentSha'` con `ref` exacta, y la parte RED, la corrida del `RedSha` original registrada en el registro de relevo del trabajo (se custodia después, en el punto 7 de §3.2).
    Las diferencias de la parte RED de `Tests` (`RT`, `RedSha..CurrentSha`) y el cálculo de `ChainRedFiles` usan **siempre los SHA originales** del `RebaseMap` (`ChainBaseSha`,
    `BaseSha`, `RedSha`, `CurrentSha`), nunca una mezcla con imágenes. No hay Worker nuevo y no consume `attempts`.
  - **Con conflictos:** `git rebase --abort`, registro `REBASE` (su `RebaseMap` lleva las rutas en conflicto y las imágenes en `null`) y STOP al Coordinator. La reemisión del contrato y el trabajo nuevo (delegación del mismo
    `TaskId`, con `ChainRedSha` y `ChainRedFiles` según A8, y verificación completa) solo siguen a una decisión registrada del Coordinator sobre el estado de la rama, con A-n
    si exige reescribir commits publicados del Worker. No consume `attempts`.
- **Tope:** como máximo **2 recuperaciones** por `TaskId`; la tercera → STOP al Coordinator.

## 4. Enrutamiento estable (entregable B, `routing.md`)

Primero la **clase de tarea**, luego la **elegibilidad** y al final el **nivel más bajo adecuado**. `routing.md` **no contiene identificadores de modelo** (INV-06).

### 4.1 Clases, dimensiones y effort semántico

| Clase (mandato) | Perfil | Effort semántico de partida | Capacidades mínimas |
|---|---|---|---|
| Implementación mecánica | ROUTINE_IMPLEMENTATION | Routine | `tool-use`, `write-commit-push` |
| Propagación repetitiva / cableado | ROUTINE_IMPLEMENTATION | Routine | ídem |
| Implementación de pruebas | ROUTINE_IMPLEMENTATION | Balanced | ídem |
| Documentación | DOCUMENTATION | Routine | `write-commit-push` |
| Caracterización | CHARACTERIZATION | Balanced | `read`, `tool-use` |
| Depuración | DEBUGGING | Balanced | `read`, `tool-use`; `write-commit-push` si corrige |
| Causa raíz incierta | DEBUGGING | Deep | `read`, `tool-use` |
| Implementación transversal a capas | ROUTINE_IMPLEMENTATION (corta) / LONG_HORIZON_IMPLEMENTATION (larga) | Deep | `tool-use`, `write-commit-push` |
| Revisión de arquitectura / conformidad adversarial | ARCHITECTURE_REVIEW | Deep | `read` |
| Implementación agéntica larga | LONG_HORIZON_IMPLEMENTATION | Long-horizon | `tool-use`, `write-commit-push` |
| Controller: planificación / verificación | CONTROLLER_PLANNING / CONTROLLER_VERIFICATION | Balanced | `read`, `tool-use`; salida estructurada |

**Dimensiones** (cada una Baja, Media o Alta, registradas en el paquete): ambigüedad, sensibilidad arquitectónica, amplitud, uso de herramientas, coste de fallo, repetición mecánica y
horizonte. Regla única:

- si **alguna** de ambigüedad, sensibilidad o coste de fallo es Alta, el effort sube **un solo escalón**, con tope en Deep;
- repetición Alta lo baja un escalón; si coincide con la subida, se compensan; `Routine` es el suelo;
- horizonte Alto fija Long-horizon, por encima del tope;
- uso de herramientas Alto exige la capacidad `tool-use`;
- amplitud **solo se registra** y no modifica el effort.

**Effort semántico:** `Routine` < `Balanced` < `Deep` < `Long-horizon` < `Maximum`. **Nivel de capacidad:** `Routine` → Eficiente; `Balanced` → Equilibrado, o Eficiente con más
effort; `Deep` → Equilibrado o Frontera; `Long-horizon` y `Maximum` → Frontera. El catálogo traduce nivel y effort a modelos y controles del proveedor (§5).

### 4.2 Algoritmo

1. Clasificar la tarea y puntuar las dimensiones.
2. Filtrar por **elegibilidad** (§4.3) entre las celdas del contrato. Si no queda ninguna → `EXECUTION_BLOCKED` de planificación, sin subir de nivel en silencio.
3. Elegir el nivel más bajo adecuado y el nivel de transporte **más preferido** (número menor de la jerarquía del mandato) disponible para esa celda (§11); registrar `RoutingReason`, que
   incluye la fecha de verificación de cada entrada del catálogo usada.
4. **Escalar** solo con `ModelEscalationReason` respaldado por evidencia, en este orden: effort → nivel → long-horizon → maximum; no es obligatorio recorrer todos los escalones. La
   elegibilidad, incluido el consumo cubierto, se aplica también al escalado.
5. **Enrutar hacia abajo** está permitido, con razón registrada.
6. Escalar, bajar o cambiar de modelo, rol o sesión **no reinicia** `attempts` ni los contadores (D-04).

**Contingencia** («IMPLEMENTATION LEVEL»: «Use the next transport tier and document the limitation»): sin celda elegible, el Coordinator elige entre (a) el siguiente nivel de transporte
con la limitación documentada, (b) una A-n de Coordinator + Architect que acepte `RoutingEnforcement: advisory` con la limitación declarada en §18, o (c) BLOCKED — OWNER DECISION.

### 4.3 Elegibilidad y exigencia

Una **celda** es modelo × transporte × capacidad (`read`, `tool-use`, `write-commit-push`, `effort-applied`). La capacidad `effort-applied` se registra **por valor de effort** probado,
y una delegación solo puede pedir un effort probado (A7). Para una **celda (por su modelo o por su transporte) sin control de effort** se registra el effort **heredado medido** en la sonda (p. ej. «heredado: xhigh»); la
delegación pide ese valor y `Routing` lo compara con el efectivo como cualquier otro. Pedir un effort heredado que no se puede bajar no es un escalado sin `ModelEscalationReason`: es
una limitación que se anota en `RoutingReason`. Es **elegible** si, en la fecha de la delegación:

- está **publicada** según una fuente oficial;
- está **instalada** y **autenticada** localmente;
- tiene **invocación probada** para esa capacidad (MEASURED);
- tiene el **consumo cubierto** (abajo); `UNKNOWN` ⇒ no elegible;
- no está `STALE`: `STALE` = mín(fecha de verificación + 90 días, retiro anunciado). Un retiro con precisión de mes o trimestre se toma como el primer día de ese periodo.

**Consumo cubierto = `sí`** solo con una de estas evidencias, registrada en el catálogo con su tipo de fuente:

- **oficial:** fila de disponibilidad del plan con el que está autenticada la herramienta, con URL y fecha versionadas;
- **medido:** una invocación completada de esa celda con la autenticación existente por suscripción (sin clave de API de pago), sin señal P-06.

**Sondas.** Una celda con consumo `UNKNOWN` solo puede **sondearse** si la autenticación es por suscripción, sin clave de API de pago, y ninguna fuente oficial la marca como de créditos
o API. Esa autenticación está MEASURED para Codex («Logged in using ChatGPT», Discovery §12) y para la sesión y sus subagentes (OAuth de una cuenta de claude.ai, sin clave de API ni
proveedor de nube; evidencia §12, con la fuente oficial). Una sonda completada sin P-06 deja la celda en `medido`. La regla es la misma para Controller y Worker.

**`service_tier`.** No forma parte del criterio: es un parámetro de la configuración del Owner que se hereda (§11). Su efecto relativo sobre el consumo es **UNKNOWN** en la documentación
oficial (evidencia §12) y se trata como **riesgo registrado** con mitigación: tope de invocaciones (§16.2) y STOP P-06.

La elegibilidad la aplica el Controller de planificación con la fecha de la delegación y la comprueba la sesión en A7. No hay prueba Core dependiente del reloj.

**`RoutingEnforcement`** (`required` | `advisory`), fijado por el contrato y copiado en la delegación. Con `required`, un modelo **o** effort efectivo distinto del solicitado hace fallar
la comprobación `Routing` y la verificación no puede ser VERIFIED. Con `advisory`, `Routing` = `pass` y la discrepancia se anota en su `Evidence`, en `Findings` y en `Deviations` de la
entrega. **En el piloto es `required`.**

**Controller.** También usa una celda elegible. La sesión elige su modelo y effort con `routing.md` y el catálogo (perfiles CONTROLLER_*), los pasa con `-m` y
`-c model_reasoning_effort=…` y registra solicitado y efectivo en el registro de relevo. Una diferencia se registra como desviación; si el modelo efectivo no corresponde a una celda
elegible, la salida de esa invocación es inválida (`INVALID_OUTPUT`, transporte BLOCKED).

## 5. Catálogo mutable (`model-catalog.md`)

**NO NORMATIVO** respecto de los nombres de modelo. Cada entrada registra, y cada dato lleva su **tipo de fuente** (`oficial` | `caché local` | `medido`), URL y fecha:

- proveedor e identificador del modelo;
- **nivel de capacidad** (Eficiente, Equilibrado o Frontera);
- **tabla effort semántico → valor del proveedor** (o, en una celda sin control de effort, «heredado medido: <valor>», §4.3);
- fortalezas y debilidades según la fuente oficial y perfiles recomendados;
- **retiro anunciado** (o «ninguno publicado»);
- **consumo cubierto** (`sí` | `no` | `UNKNOWN`) con la evidencia de §4.3;
- **estado local por celda**: publicado, instalado, autenticado, invocación probada por capacidad y fecha de medición.

**Reglas del contenido inicial** (D-05):

- solo es elegible lo que es oficial o medido; un modelo que solo aparece en la caché local (p. ej. la familia que el Discovery §13 marcó para reverificar) se registra como
  `caché local` y no es elegible;
- los alias de subagente de Claude quedan `UNKNOWN` hasta la sonda U-04 (§12.1);
- los valores de effort de Codex se toman de la referencia oficial de configuración registrada en la evidencia §12 (valores de `model_reasoning_effort`, que «depend on the model and
  client») y se reconcilian con los rótulos de la página de modelos (Light…Ultra), que no traen una correspondencia explícita: cada valor que se use se mide en una sonda;
- un modelo con las filas de disponibilidad «ChatGPT Credits» y «API Access» en la fuente oficial (evidencia §12) queda con consumo `UNKNOWN`, no es elegible y no se sondea.

Quién lo actualiza y con qué control: §15.

## 6. Guía de prompting (entregable A, `prompting-guide.md`)

Guía breve basada en la documentación oficial vigente, con URL, proveedor, fecha y generación cubierta de cada fuente (Discovery §13), y con la cláusula de subordinación. Reglas:

- encuadre: objetivo, contexto, restricciones y criterio de terminado;
- rol y criterios de éxito explícitos;
- etiquetas cuando el prompt mezcla instrucciones y datos;
- instrucciones de herramienta como acción, no como sugerencia;
- evitar sobreingeniería y reinvestigación: se referencian el Discovery y la entrega previa;
- en horizonte largo, estado en archivos y progreso incremental;
- STOP explícito;
- **autolocalización**: se citan autoridades por ruta y sección, sin copiarlas;
- la **vigencia del workflow se lee de Git**, no de prosa desactualizada (F-02);
- ningún secreto.

Los perfiles por clase de tarea viven en PROMPT_TEMPLATES §G; la guía los referencia.

## 7. Composición de prompts (entregable C, PROMPT_TEMPLATES §G «Delegación de ejecución»)

`prompt = contrato base de delegación + perfil + delta de la tarea`.

- **Contrato base** (≤ 40 líneas): rol; identidad (unidad, gate, tarea, intento, `RunId` de la invocación, `DelegationRunId`, `AuthorityRevision`, `MainSha`, `BaseSha`, rama, worktree,
  `Owner`); autoridades por referencia; alcance permitido y prohibido; invariantes; criterios; pruebas requeridas con selección > 0; evidencia esperada; STOP; ruta de la entrega;
  prohibición de autoaprobación (incluidos los términos de §8.4); trailer `Co-Authored-By` de quien ejecuta; terminar tras la entrega. Cubre los 8 campos de LIFECYCLE §10.
- **Perfiles** (8, solo instrucciones de método, ≤ 25 líneas cada uno, sin cláusulas normativas): `ROUTINE_IMPLEMENTATION`, `DEBUGGING`, `LONG_HORIZON_IMPLEMENTATION`,
  `ARCHITECTURE_REVIEW`, `CHARACTERIZATION`, `DOCUMENTATION`, `CONTROLLER_PLANNING` y `CONTROLLER_VERIFICATION`.
- **Delta:** los campos del paquete, renderizados.

La composición es determinista. El prompt compuesto se guarda como `prompt.md` con su SHA-256, **no supera 200 líneas** y no contiene frases testigo (§13, OBL-06). Es la medida del
criterio 14 («without a mega-prompt»).

## 8. Esquemas (entregables D, E, verificación y relevo)

JSON Schema 2020-12 **estrictos**, comprobados de forma **recursiva**: en todo objeto, incluidos los anidados y los `items`, `additionalProperties: false` y todas las propiedades en
`required`; `null` explícito para «no aplica». Todo campo SHA usa exactamente `^[0-9a-f]{40}$`. El relevo los valida con `Test-Json` de PowerShell 7 (MEASURED en G1-C).
Compatibilidad con `--output-schema`: medida en G2 para `delegation` (sonda PR-1) y en G3 para `controller-verification` (§16.2); los otros tres no los produce Codex y se declaran **no
ejercidos** por esa vía.

### 8.1 Sintaxis de alcance

Una entrada es una ruta relativa a la raíz del repositorio, con `/` y la capitalización exacta: un **archivo exacto** o un **prefijo de directorio** terminado en `/`. Sin comodines, `..`
ni rutas absolutas. Una ruta R está cubierta por E si R = E, o si E termina en `/` y R empieza por E. A ⊆ B si cada entrada de A está cubierta por alguna de B (un prefijo de A solo lo
cubre un prefijo de B que lo contenga).

### 8.2 Los cinco esquemas

| Esquema | Autor | Campos |
|---|---|---|
| `rackcad-gate-contract/v1` | Coordinator | `Schema`, `Unit`, `Gate`, `TaskId`, `Objective`, `AuthorityRevision`, `MainSha`, `Authorities[]` (ruta, sección y clase `UNIT_DOC`/`UNIT_CHANGE`/`EXTERNAL`, §9), `AllowedWriteScope[]`, `ForbiddenWriteScope[]`, `Invariants[]`, `RequiredTests[]` (proyecto, filtro, `MinSelected`, `ExpectRed`), `ExpectedEvidence[]`, `StopConditions[]` (id, texto), `EligibleCells[]` (§4.3: celda, efforts probados, fuente y fecha), `RoutingEnforcement`, `CorrectionsAuthorized`, `IssuedBy`, `IssuedUtc` |
| `rackcad-delegation/v1` | Controller (planificación, vía `-o`) | identidad y base: `Schema`, `TaskId`, `RunId` (= `DelegationRunId`), `Initiative`, `Unit`, `Gate`, `Attempt`, `AuthorityRevision`, `MainSha`, `BaseSha`, `ExpectedBranch`, `ExpectedWorktree`, `Owner` (`Kind` `subagent`/`process`, `Id`); enrutamiento: `TaskClass`, `Dimensions`, `Executor` (proveedor, transporte, rol, celda), `Model`, `Effort` (semántico y del proveedor), `PromptProfile`, `RoutingReason`, `ModelEscalationReason`, `RoutingEnforcement`; encargo: `Authorities[]`, `Objective`, `AllowedWriteScope[]`, `ForbiddenWriteScope[]`, `Invariants[]`, `AcceptanceCriteria[]`, `RequiredTests[]`, `ExpectedEvidence[]`, `StopConditions[]`; control: `MaxReworkLoops` (constante 3), `AttemptsRemaining`, `ChainRedSha` (el RED vigente de §9, o `null`), `ChainBaseSha` (§9), `ChainRedFiles[]` (§9; vacío solo si la cadena nunca acreditó RED), `CorrectionOf` (`RunId`, `FailureClass` y SHA-256 del análisis, o `null`), `ExpectedHandoffPath` (patrón `^artifacts/orchestration/.+/\{WorkRunId\}/worker-handoff\.json$`), `IssuedBy` |
| `rackcad-worker-handoff/v1` («entrega de worker») | Worker | identidad y Git: `Schema`, `TaskId`, `RunId` (de trabajo), `DelegationRunId`, `Initiative`, `Gate`, `Attempt`, `BaseSha`, `RedSha` (o `null`), `CurrentSha`, `Branch`, `Worktree`, `Pushed`, `FilesChanged[]`; trabajo: `WorkCompleted`, `TestsExecuted[]` (id de corrida, comando, fase `RED`/`GREEN`/`RELEVANT`/`MUTATION`, SHA o «árbol sucio», archivo de resultados), `TestResults[]` (id de corrida, seleccionadas, superadas, fallidas, omitidas, nombres fallidos), `Evidence[]`; hallazgos: `UnexpectedFindings[]`, `KnownLimitations[]`, `Deviations[]`, `OpenQuestions[]`; cierre: `WorkerStatus`, `Disposition` (`NONE`/`BLOCKED`/`STOP`), `TriggeredStopConditions[]`, `RecommendedNextAction`, `Worker` (proveedor, modelo y effort solicitados, trailer usado) |
| `rackcad-controller-verification/v1` | Controller (verificación, vía `-o`) | `Schema`, `TaskId`, `RunId`, `DelegationRunId`, `WorkRunId`, `Attempt`, `VerifiedSha`, `Checks` (objeto con las 14 comprobaciones de §8.3, cada una `Result` `pass`/`fail`/`not_run` y `Evidence`; `Ci` y `Tests` llevan además `RedPart` `pass`/`fail`/`not_applicable`), `Classification`, `Disposition` (`NONE`/`REWORK`/`BLOCKED`/`STOP`), `FailureClass` (§9), `TriggeredStopConditions[]`, `Findings[]`, `RecommendedNextAction` |
| `rackcad-relay-record/v1` | Sesión responsable | `Schema`, `TaskId`, `RunId`, `Attempt`, `Phase` (`PLANNING`/`WORK`/`VERIFICATION`/`CONTROL`/`PROBE`/`REBASE`), `Participant` (tipo, perfil, modelo y effort solicitados y efectivos, fuente del efectivo y su SHA-256), `Exit` (HEAD, `ls-remote`, `origin/main`, árbol limpio, operación Git, commit del resumen, hash y nombres de claves de `config.toml`, procesos clasificados, delegaciones abiertas), `Cession` (inicio, fin, la sesión operó: sí/no), `Outcome` (`COMPLETED`/`TIMEOUT`/`FAILED_TURN`/`NO_OUTPUT`/`INVALID_OUTPUT`/`REJECTED_BEFORE_INVOCATION`, PID lanzado, código de salida, evento terminal, árbol matado, muerte confirmada, ruta, SHA-256 y fecha de la salida, resultados A1-A8 si es una aceptación), `Entry` (HEAD, `ls-remote`, árbol limpio, hash y nombres de claves de `config.toml` y cambio, procesos), `RemoteFacts` (o `null`: `run_id`, `event`, `ref`, `head_sha`, conclusión de cada job requerido, corrida del `RedSha` con los mismos campos, artefactos de pruebas con nombre, SHA-256, seleccionadas, superadas, fallidas y nombres fallidos, `origin/main`, `ls-remote`, orden y hora UTC de obtención, diagnóstico si hubo rojo), `RebaseMap` (o `null`; §3.6: `MainSha` anterior y nuevo; SHA de cierre de G2 y su imagen; `AuthorityRevision` y su imagen, con igualdad de blobs `UNIT_DOC` y de texto normalizado de cada sección `UNIT_CHANGE`; `BaseSha` y `ChainBaseSha` con sus imágenes, o `null` fuera de una tarea; commits con imagen y `patch-id`; rutas en conflicto, con imágenes `null`, si las hay; decisión del Coordinator citada, si la hay), `Disposition` del transporte (`NONE`/`BLOCKED`/`STOP`), `Notes` |

Ningún enum admite un valor de gate. `Classification` y `Disposition` solo forman los pares válidos `VERIFIED/NONE`, `REWORK_REQUIRED/REWORK`, `BLOCKED/BLOCKED` y `BLOCKED/STOP`.
**STOP se expresa como `EXECUTION_BLOCKED` + `Disposition: STOP`**, sin un cuarto valor de clasificación (el mandato fija tres). El Controller lo recomienda; la sesión lo aplica y lo
devuelve al Coordinator. `TriggeredStopConditions[]` admite S-01..S-14, P-01..P-08 (§10.3) y los ids de `StopConditions` del contrato.

### 8.3 Comprobaciones obligatorias de la verificación

Orden fijo; `FailureClass` es la **primera** comprobación en `fail` o `not_run`. **`EXECUTION_VERIFIED` solo si las 14 están en `pass`.** Un `not_run` se trata como `fail` con la
disposición de su fila, **salvo** el `not_run` causado porque una comprobación anterior en `fail` dejó sin entrada a la comprobación (en particular, sin entrega no hay `CurrentSha`): ese
`not_run` no aporta disposición. Así, con la entrega ausente la disposición es BLOCKED. El esquema hace imposible omitir una comprobación (objeto con propiedades fijas y todas requeridas). La coherencia entre `Classification`, `Disposition` y `Checks`
la valida el relevo con una regla documentada en el README, además de `Test-Json` (OBL-11).

| # | Id | Comprueba | Actor de la señal | Disposición si falla |
|---|---|---|---|---|
| 1 | `Termination` | El registro de relevo del trabajo dice `COMPLETED`; si fue un proceso, muerte confirmada | sesión → Controller | BLOCKED |
| 2 | `Handoff` | Entrega presente, válida, con `TaskId`, `Attempt` y `DelegationRunId` de la delegación, `RunId` = el asignado por el relevo a esa invocación de trabajo y fecha posterior al inicio | Controller | **ausente** → BLOCKED; **inválida o de otra corrida** → REWORK si `Identity` pasa, si no STOP |
| 3 | `Authority` | `AuthorityRevision` existe (`git cat-file -e`) y es ancestro de `BaseSha` (tras un rebase, sus imágenes del `RebaseMap`). Lectura y comprobación por clase (§9): `UNIT_DOC` y `UNIT_CHANGE` se leen en `AuthorityRevision`; `EXTERNAL` se lee en `MainSha` y, con `MB` = `git merge-base MainSha AuthorityRevision`, su archivo no cambia en la rama si no contiene secciones `UNIT_CHANGE` (`git diff --name-only MB AuthorityRevision -- <ruta>` vacío); si las contiene, el texto de cada sección `EXTERNAL` es idéntico en `MB` y en `AuthorityRevision` (con espacios normalizados) y toda diferencia del archivo cae dentro de secciones declaradas `UNIT_CHANGE`. Además, `git diff --name-only AuthorityRevision BaseSha` no contiene rutas `EXTERNAL` ni rutas con secciones `UNIT_CHANGE` | Controller | STOP |
| 4 | `Contract` | A3-A5 de §3.4 entre delegación y contrato | Controller | STOP |
| 5 | `Identity` | Rama y worktree; `BaseSha` ancestro; `RedSha` (si no es `null`) entre `BaseSha` y `CurrentSha`; `HEAD` = `CurrentSha`; `refs/remotes/origin/<rama>` = `CurrentSha`. Tras un rebase: `BaseSha`, `RedSha` y `CurrentSha` de la entrega son los originales del `RebaseMap`; sus imágenes cumplen lo anterior (`HEAD` = `refs/remotes/origin/<rama>` = `ls-remote` = `CurrentSha'`) y `patch-id` es igual | Controller (Git local) | STOP |
| 6 | `Remote` | En el registro: `ls-remote` = `CurrentSha` y `origin/main` = `MainSha` | sesión → Controller | STOP (avance de `main`, S-13) o BLOCKED (hechos ausentes) |
| 7 | `Scope` | `git diff --name-only BaseSha..CurrentSha` ⊆ `AllowedWriteScope` y sin intersección con `ForbiddenWriteScope` | Controller | STOP |
| 8 | `CleanTree` | Árbol limpio y ruta del paquete ignorada (`git check-ignore`) | Controller | REWORK |
| 9 | `Ci` | Corrida `push` de `CurrentSha` con `ref` exacta y los cuatro jobs requeridos en `success`. **`RedPart`**: si la entrega **exige RED** (§9), `pass` con `RedSha` no `null` y su corrida `push` terminada con el job Core en `failure`, y `fail` si `RedSha` es `null` o su corrida terminó sin ese fallo; si no exige RED, `not_applicable`, citando la corrida del `ChainRedSha`. `Result` = `pass` solo con la corrida de `CurrentSha` en verde y `RedPart` ≠ `fail` | sesión → Controller | BLOCKED (corrida de `CurrentSha` ausente o en curso, o la del `RedSha` en curso cuando la entrega exige RED); REWORK (la de `CurrentSha` roja, tras leer logs; `RedPart` en `fail`, incluida la corrida del `RedSha` ausente con la de `CurrentSha` ya terminada) |
| 10 | `Tests` | `RequiredTests` con selección ≥ mínimo; conteos del TRX coherentes con `TestResults[]` y con el diff. **`RedPart`:** si la entrega exige RED (§9), `pass` solo si las pruebas con `ExpectRed` del contrato están entre las fallidas del TRX del `RedSha` **y** `git diff --name-only RedSha CurrentSha` no toca `RT` ∪ `ChainRedFiles` (las pruebas no cambian después del RED), y `fail` en otro caso; si no exige RED, `not_applicable`. Al acreditar el RED, `Evidence` registra `ChainRedFiles` (§9). `Result` = `pass` solo con lo anterior y `RedPart` ≠ `fail` | sesión → Controller | REWORK |
| 11 | `Trailer` | Cada commit `BaseSha..CurrentSha` lleva el `Co-Authored-By` declarado, coherente con el modelo efectivo | Controller | REWORK |
| 12 | `Routing` | Modelo y effort efectivos = solicitados, según `RoutingEnforcement` (§4.3) | sesión → Controller | BLOCKED (con `required`) |
| 13 | `FreeText` | Ningún término de §8.4 en el texto libre de la entrega | Controller | REWORK |
| 14 | `Denials` | Ninguna denegación ni orden fallida pendiente en los eventos o la transcripción | sesión → Controller | denegación de **permisos o credenciales** → STOP (S-06); otra orden fallida → BLOCKED |

Con varias comprobaciones en `fail`, la disposición sigue la precedencia **STOP > BLOCKED > REWORK**. La señal de pruebas es la CI de `push` del SHA exacto, ejecutada en un runner ajeno al
Worker: **es la señal de la verificación del Controller, no evidencia de gate**, y no sustituye ninguna clase de AGENTS.md.

### 8.4 Términos de gate en texto libre

Coincidencia por **palabra o frase completa**, sin distinguir mayúsculas, en `WorkCompleted`, `Evidence[]`, `UnexpectedFindings[]`, `KnownLimitations[]`, `Deviations[]`,
`OpenQuestions[]` y `RecommendedNextAction`. Lista congelada: `GATE PASS`, `GATE_PASS`, `Candidato`, `Candidate`, `FINAL_CANDIDATE_SHA`, `gate cerrado`, `gate closed`,
`cierre del gate`, `rama integrada`, `integrada en main`, `integrated into main`, `merged into main`, `lista para integrar`, `ready to merge`, `Owner Validation APROBADA` y
`Owner Validation APPROVED`. No se aplica al nombre de propiedad `Gate` ni a su valor. **Actores:** el Controller sobre la entrega (`FreeText`); el Coordinator sobre el texto libre de la
verificación, como control manual registrado. El perfil del Worker advierte de la lista.

## 9. Identidad, propiedad, conteo y topes (C61-G1-05)

- **Identidad.** La delegación fija `AuthorityRevision`, `MainSha`, `BaseSha`, rama y worktree. **`AuthorityRevision`** es el SHA del commit cuyo árbol contiene los documentos y los
  cambios normativos de la propia unidad que obligan a la delegación. En el piloto es el **SHA de cierre de G2**, revisado por el Coordinator contra el Freeze, **o** un commit
  posterior X de la rama, elegido por el Coordinator, tal que `git diff --name-only <B> X` no contiene rutas `EXTERNAL` ni rutas con secciones `UNIT_CHANGE`, donde `B` es el SHA de
  cierre de G2 o la `AuthorityRevision` vigente (que ya lo cumple respecto del cierre de G2 o de su imagen vigente), o, tras un rebase, su imagen en el último `RebaseMap` (así se leen las A-n y decisiones posteriores, con `src/`, `tests/`, estado y custodia permitidos, sin cambiar el
  texto `UNIT_CHANGE` revisado en G2); **o** la imagen de cualquiera de ellos registrada en un `RebaseMap`. Es ancestro de `BaseSha`. Al emitir o reemitir un contrato tras una A-n o
  decisión de la unidad, el Coordinator elige un X que la contenga.
- **Clases de autoridad** (declaradas en el contrato por ruta y sección; comprobación `Authority` de §8.3):
  - `UNIT_DOC`: documentos de la propia unidad (contrato de la iniciativa `docs/initiatives/<I>-*.md`, Freeze y A-n, `decisions/<I>.md`, evidencia y mandato registrado). Se leen en
    `AuthorityRevision`;
  - `UNIT_CHANGE`: cambios normativos de la unidad bajo su Freeze, declarados por sección (en I-61, los de §2.1). Se leen en `AuthorityRevision`;
  - `EXTERNAL`: toda otra autoridad. Se lee en `MainSha` y la rama no puede haberla modificado.
  - Una **sección** es el texto desde su encabezado hasta el siguiente encabezado con el mismo número de `#` o menos: incluye sus subsecciones. El **preámbulo** es el texto anterior al
    primer encabezado `##`.
  - Si algo de esto no es verificable → STOP (S-12).
- **Propiedad exclusiva.** `Owner` identifica el único puesto de escritor; nunca hay dos. Si no se sabe si un participante sigue vivo, no se lanza otro.
- **`attempts` en una tarea delegada** (estado canónico, AUTOMATION_PLAN §8-9, por unidad) sube en 1 cuando la sesión lanza una **corrección**: tras `EXECUTION_REWORK_REQUIRED`, o tras un
  STOP cuya resolución por el Coordinator cambia el trabajo. La recuperación de §3.6 y las reejecuciones tras BLOCKED no lo incrementan. La sesión lo incrementa en el archivo de estado,
  **en un commit, antes** de emitir la delegación de corrección. **Fuera de la ejecución delegada** (p. ej., el trabajo directo de la sesión en G2) rigen AUTOMATION_PLAN §8-9 sin
  cambios.
- **`Attempt`** (en delegación, entrega, verificación y ruta) = valor de `attempts` en el estado de `HEAD` al emitir la delegación. **`AttemptsRemaining`** = `max_attempts` − `attempts`.
  **`MaxReworkLoops`** = 3, constante.
- **Cadena** = `TaskId`. **Contador por clase** = número de correcciones lanzadas en la cadena para una misma `FailureClass`.
- **`ChainBaseSha`:** el `BaseSha` de la primera delegación de la tarea (o su imagen en un `RebaseMap`); la delegación lo lleva y A8 lo comprueba.
- **Pruebas RED de una entrega** (`RT`): `git diff --name-only <ChainBaseSha> <RedSha> -- tests/` (todas las pruebas de la cadena hasta el RED; en la primera delegación
  `ChainBaseSha` = `BaseSha`). Las modificaciones de pruebas van en el commit RED: el GREEN no toca `RT` ∪ `ChainRedFiles` (§8.3 fila 10).
- **RED acreditado de la cadena:** un `RedSha` con `RedPart` = `pass` en `Ci` y en `Tests` en una verificación de la cadena. Al acreditarlo, **`ChainRedFiles`** = `ChainRedFiles`
  anterior ∪ `RT` (anclado en Git; el conjunto **nunca decrece**), registrado en el `Evidence` de `Tests` de esa verificación y custodiado.
- **RED vigente** (`ChainRedSha` de la delegación siguiente, A8): el último RED acreditado, **salvo** que después una entrega que exigía RED haya terminado con `RedPart` = `fail` en `Ci`
  o en `Tests`; entonces es `null` hasta que se acredite otro. `ChainRedFiles` se conserva siempre y solo está vacío si la cadena nunca acreditó un RED.
- **Exige RED** (lo decide la verificación con datos de Git, no una declaración): una entrega exige RED si `ChainRedSha` es `null`, **o** si `git diff --name-only BaseSha..CurrentSha`
  toca algún archivo de `ChainRedFiles` (el RED acreditado **caduca** cuando se modifican sus pruebas). Una entrega que exige RED trae un commit RED: en la primera delegación, el
  esqueleto; en las siguientes, la corrección **desactivada** (AGENTS: regresión verificada fallando con el fix desactivado), seguido del commit GREEN. `ExpectRed` es siempre el del
  contrato (A5).
- **`FailureClass`**, enumeración cerrada: `NONE` o el id de una comprobación de §8.3, más `StopCondition` para un STOP no ligado a una comprobación. «Misma clase» = mismo valor.
- **Defecto nuevo:** si la `FailureClass` difiere de la del REWORK anterior de la cadena, el Coordinator registra `analysis.md` (causa, por qué no es la misma clase y nuevo enrutamiento
  si procede) **antes** de la corrección, y la delegación lo cita en `CorrectionOf`. La corrección **sí** consume `attempts`.
- **STOP** si, ante un nuevo REWORK de clase X, el contador de X ≥ 3, o si `attempts` ≥ `max_attempts` (S-11).
- **Topes de BLOCKED** (B-10): como máximo **2 reejecuciones** por (`TaskId`, fase) tras BLOCKED o un fallo de transporte; al alcanzarlo, STOP (P-04) con causa raíz registrada. No
  se reescribe un prompt sin un `analysis.md` versionado.
- **Sin reinicios:** cambiar de modelo, rol o sesión, o renombrar el error, no reinicia nada.

**Escenarios (OBL-08):**

| Escenario | Resultado esperado |
|---|---|
| Primera delegación de una tarea | `Attempt` = `attempts`; nada se incrementa; RED exigido |
| REWORK de clase X, contador X = 0, `attempts` < máximo y RED acreditado | commit con `attempts`+1, delegación con el nuevo `Attempt`, `ChainRedSha` y `ChainRedFiles`, contador X = 1 |
| RED de una corrección que solo desactiva el fix en `src/` | se acredita; `ChainRedFiles` incluye igualmente las pruebas de la cadena desde `ChainBaseSha` |
| GREEN que modifica pruebas de `RT` ∪ `ChainRedFiles` | `Tests` con `RedPart` = `fail`: REWORK (las pruebas modificadas no se vieron fallar) |
| Corrección tras un RED no acreditado, con RED que solo desactiva el fix y GREEN que modifica pruebas de la cadena | `RT` (desde `ChainBaseSha`) las incluye: `RedPart` = `fail`, REWORK |
| REWORK por `RedPart` = `fail` con un RED acreditado anterior | RED vigente = `null`; `ChainRedFiles` se conserva; la corrección exige RED |
| REWORK sin RED acreditado (p. ej. la corrida del `RedSha` no falló) | la corrección exige RED (corrección desactivada, luego GREEN); `ChainRedSha` = `null` |
| Entrega sin `RedSha` cuando exige RED | `Ci` y `Tests` con `RedPart` = `fail`: REWORK |
| Entrega con RED acreditado en la cadena cuyo diff toca `ChainRedFiles` | exige RED (caducado): sin RED nuevo, `RedPart` = `fail` y REWORK; con RED nuevo acreditado, nuevo `ChainRedSha` y `ChainRedFiles` ampliado |
| REWORK de clase Y ≠ X | `analysis.md` antes de la corrección; `attempts`+1 |
| REWORK de clase X con contador X = 3 | STOP |
| `attempts` = `max_attempts` y nuevo REWORK | STOP |
| STOP resuelto por el Coordinator con un cambio del trabajo | `analysis.md`; `attempts`+1 |
| STOP por avance de `main` | sin incremento; §3.6; como máximo 2 recuperaciones por tarea, la tercera → STOP |
| BLOCKED o fallo de transporte | sin incremento; reejecución con `RunId` nuevo, máximo 2 por fase; la tercera → STOP |
| Una invocación excedería el tope de §16.2 | no se lanza; STOP (P-07) |
| Cambio de modelo, rol o sesión | sin reinicio |
| Control negativo | ruta y registro propios, sin incremento |
| Trabajo directo de la sesión con CI roja (G2) | AUTOMATION_PLAN §8-9: la corrección incrementa `attempts` |

## 10. Semántica de fallos, STOP y recuperación

### 10.1 Modos de fallo

Se congela la tabla del [Discovery](I-61-discovery.md) §18.4 con esta correspondencia:

| Modo (§18.4) | Quién lo registra | `Classification` / `Disposition` |
|---|---|---|
| Proceso colgado o stdin abierto | sesión (registro de relevo, `TIMEOUT`, muerte confirmada) | sin veredicto; transporte `BLOCKED` |
| Salida conforme pero vacía o falsa | sesión (`INVALID_OUTPUT`) o Controller (`Handoff`) | nunca VERIFIED; transporte `BLOCKED`, o REWORK/STOP según `Handoff` |
| Turno fallido o interrumpido | sesión (`FAILED_TURN`) | transporte `BLOCKED` |
| Resultado de Claude sin datos | sesión (`NO_OUTPUT`) | transporte `BLOCKED`; si hay commits verificables, el Controller verifica y clasifica |
| Permisos o credenciales | sesión o Controller (`Denials`) | `BLOCKED/STOP` (S-06: nunca se obtienen credenciales) |
| Entrega ausente | Controller (`Handoff`) | `BLOCKED/BLOCKED`; ningún otro escritor hasta confirmar la terminación |
| Entrega inválida o de otra corrida | Controller (`Handoff`) | `REWORK_REQUIRED/REWORK` si `Identity` pasa; si no, `BLOCKED/STOP` |
| Identidad errónea | Controller (`Identity`) | `BLOCKED/STOP` |
| Éxito declarado sin commit | Controller (`Identity`, `CleanTree`, `Scope`) | `REWORK_REQUIRED/REWORK`; `BLOCKED/STOP` si hay escritura fuera de alcance |
| Corridas o escritores duplicados | sesión (§3.3, §3.5) | STOP (P-02) |
| Base obsoleta | sesión y Controller (`Remote`) | antes de escribir: STOP y rebase con delegación nueva; después: `BLOCKED/STOP` (S-13) y §3.6 |
| Owner Validation o AutoCAD | delegación | **frontera, no fallo** (§10.3) |
| Modelo o effort distinto | sesión → Controller (`Routing`) | con `required`: `BLOCKED/BLOCKED`; con `advisory`: desviación |
| Cambio de `config.toml` | sesión | STOP (P-01) |

**Nunca bastan** stdout, un código de salida o un JSON conforme. Se ignora cualquier salida con fecha anterior al inicio de la invocación.

### 10.2 Recuperación

- `REWORK` → corrección según §9, si el contrato la autoriza: vuelta al paso 1 del orden de §3.2;
- `BLOCKED` → el Coordinator resuelve la precondición; reejecución dentro del tope;
- `STOP` → causa raíz en `analysis.md` y decisión del Coordinator, o del Owner si la materia es OWNER-RESERVED. No hay continuación especulativa.

### 10.3 Ítems STOP → comportamiento

«Frontera»: el participante no la cruza, entrega lo automatizable, no es fallo, no consume `attempts` y el bloqueo estructurado vuelve al Coordinator en `KnownLimitations` (entrega) y
`Findings` (verificación), que puede ser VERIFIED sobre lo automatizable con «OV pendiente». Ningún STOP consume `attempts` por sí mismo (§9).

| Id | Ítem (mandato «STOP CONDITIONS») | Comportamiento |
|---|---|---|
| S-01 | Owner Validation required | frontera |
| S-02 | architecture outside Freeze | STOP |
| S-03 | material ambiguity | STOP |
| S-04 | contradictory evidence | STOP |
| S-05 | destructive/irreversible action not authorized | STOP |
| S-06 | missing credentials/permissions | STOP |
| S-07 | conflict with another active owner | STOP |
| S-08 | branch/worktree ownership conflict | STOP |
| S-09 | dependency not integrated | STOP |
| S-10 | required AutoCAD/user interaction | frontera |
| S-11 | repeated rework limit reached | STOP |
| S-12 | Worker loses context or cannot verify authority revision | STOP |
| S-13 | main movement invalidates exact-SHA assumptions | STOP; §3.6 |
| S-14 | initiative contract explicitly says STOP | STOP |

AUTOMATION_PLAN §12: la viñeta 1 no aplica; la 2 equivale a S-08; la 3 (árbol sucio, operación Git, worktree activo en otra sesión), STOP; en la 4, faltan decisiones, bloques DWG,
secretos o permisos → STOP (S-03, S-06), y faltan validaciones, referencias de AutoCAD o build local → frontera, que la sesión gestiona según §10 de ese plan; la 5 equivale a S-07/S-09;
la 6, a S-02 o alcance, STOP; la 7, STOP; la 8, S-05.

**STOP propios del protocolo:**

| Id | Condición |
|---|---|
| P-01 | Cambio del hash de `config.toml` |
| P-02 | Participante vivo ajeno, proceso no atribuible o más de una delegación abierta |
| P-03 | Delegación fuera del contrato |
| P-04 | Tope de BLOCKED alcanzado |
| P-05 | Invocación distinta del paquete aceptado |
| P-06 | Aviso de límite de uso o de créditos en los eventos o la transcripción de una invocación |
| P-07 | Una invocación excedería el tope de §16.2 (no se lanza) |
| P-08 | La sesión operó durante una cesión: la salida de esa invocación es inválida y no puede respaldar un VERIFIED |

## 11. Transporte y almacenamiento (entregable F; D-03)

**Jerarquía y usos de Computer Use: los del mandato, por referencia y sin cambios.** Computer Use nunca es el bus normativo. **Esquemas, relevo y verificación son idénticos en todo nivel
de transporte.**

**Disponibilidad** (Discovery §17):

- Controller Codex por CLI en `read-only` (nivel 2): **MEASURED**.
- Worker Claude como subagente de la sesión con modelo y effort del paquete (nivel 2): **UNKNOWN** hasta la sonda U-04 (§12.1). MEASURED solo que los subagentes leen y heredan
  modelo y effort de la sesión.
- Worker Codex con escritura, commit y push: **UNKNOWN** (P2, réplica con worktree anidado: el sandbox rechazó lanzar procesos).
- Claude CLI: no autenticado (MEASURED).

**Receta de invocación de Codex** (elementos congelados; la orden exacta se mide en G2 y queda en el README):

- binario por ruta verificada;
- `-C <worktree de la unidad>` **únicamente** (DEV-G1C-01);
- `-s read-only`, `-m <modelo>`, `-c model_reasoning_effort=<valor>`, `--output-schema <esquema>`, `-o <dir del RunId>/output.json` y `--json`;
- **sin** `--ephemeral`, para leer el modelo y el effort efectivos en el registro de sesión (`session_meta.model`, `turn_context.model`/`effort`); el modo se **mide** en PR-1;
- stdin cerrado; el `pwsh` del runtime de Codex delante en el PATH **solo del proceso hijo**;
- **tope de 600 s**, terminación del árbol y confirmación de muerte (§3.3);
- hash y nombres de claves de `config.toml` antes y después: distinto → STOP (P-01);
- `service_tier`: **heredado** de la configuración del Owner, sin modificarla ni sobrescribirla por proceso; riesgo registrado de §4.3.

**Almacenamiento transitorio:** `artifacts/orchestration/<unit>/<task>/<attempt>/<RunId>/`, en el worktree de la unidad, ignorado por Git (`.gitignore:3`). Contiene, según la fase:
`gate-contract.json`, `delegation.json`, `prompt.md`, `worker-handoff.json`, `relay-record.json`, `controller-verification.json`, `analysis.md`, eventos y salidas. El canal de retorno son
esos archivos y el informe de la sesión.

**Custodia duradera** (aplicación de WORKFLOW §11.4): antes de que el Coordinator registre una decisión, la sesión copia los archivos JSON y MD del paquete que la respaldan a
`docs/automation/evidence/I-61-pilot/<task>/<RunId>/`. La **identidad duradera** es el blob de Git en el commit de custodia, verificable con `git rev-parse <commit>:<ruta>`. El SHA-256 del
archivo transitorio se registra como **procedencia**. Tras el commit se compara con el SHA-256 de `git cat-file -p <blob>`; si difieren por normalización de fin de línea se declara, sin
cambiar `.gitattributes`. Eventos JSONL, registros de sesión y transcripciones **no** se versionan: quedan su SHA-256 y los campos extraídos, como procedencia **no reverificable** tras la
limpieza del worktree. Una decisión solo cita artefactos versionados.

## 12. Piloto (C61-G1-06, C61-G1-07)

**Tarea real:** corregir **D-1a** (Discovery §19).

- **Función pura** en `src/RackCad.Application/Persistence/` que resuelve el nombre editado con la **misma semántica** que las otras cinco rutas: blanco o espacios → nombre del sobre
  (incluido `null`); en otro caso, el editado.
- **Uso:** `EditCama` la usa para el payload y para `SyncName`. Las otras cinco rutas no se tocan.
- **Materialidad** (Discovery §19.1): M-01..M-08 no activados; EXTENSION.
- **Efecto:** evita D-2 (un `null` del sobre se conserva) y D-3 (el asignador no vuelve a entregar el mismo «Cama N»; observado en OV-I61-01). D-4 queda fuera.
- **Intención de producto:** el vínculo entre «that bug» del mandato y D-1 es una **INFERENCE** del Coordinator (SAME-SESSION; Discovery §15, Q-06). De ella depende que M-03 no se
  active. Salvaguarda: si el Owner la rechaza, la OV del Candidato la invalida.

### 12.1 Flujo de G3

0. **Sonda U-04** (antes de enrutar). La sesión, en rol de Coordinator, hace una clasificación **provisional** de la tarea con §4.1 y aplica el paso 3 de §4.2 **sin** los filtros de
   invocación probada ni de consumo: la celda candidata debe estar publicada, instalada, autenticada, no `STALE` y no marcada como de créditos o API. Se sondean esa celda, con el effort
   que resulte y con el escalón inmediatamente superior, y, si existe, la siguiente de nivel inferior; una celda sin control de effort se sondea sin effort solicitado y registra el
   effort heredado medido (§4.3). «Una ejecución por celda» se cuenta por valor de effort. La clasificación queda en el registro `PROBE` y no vincula al Controller, que elige entre las
   celdas del contrato. Tarea: en una réplica desechable fuera del repositorio (repositorio y remoto
   local en el scratchpad de la sesión), el subagente crea un archivo, hace commit con su trailer y push al remoto local. Límites (cláusula 6 del Owner): una ejecución por celda, tope de
   10 min, sin red ni acceso al worktree de la unidad. **Oráculo contra Git:** el archivo existe; `git log` del remoto muestra el commit con el trailer declarado; `git ls-remote` del
   remoto devuelve ese SHA; modelo y effort efectivos salen de la transcripción y deben coincidir con los solicitados (o, sin control de effort, quedan registrados como heredados).
   Cualquier fallo deja la capacidad sin probar → contingencia de §4.2. Las celdas probadas pasan al catálogo
   y al contrato.
1. El Coordinator redacta `gate-contract.json`: alcance permitido `src/RackCad.Application/Persistence/`, `src/RackCad.Plugin/RackCamaCommands.cs`, `tests/RackCad.Tests/` y
   `tests/RackCad.UI.Tests/FlowBedEditorWindowTests.cs`; prohibido, el resto de comandos del Plugin, `docs/`, `.github/`, `assets/`, `AGENTS.md`, `CLAUDE.md`, `.gitignore` y
   `.gitattributes`; invariantes INV-01..03, INV-05, INV-10 e INV-P1..P3; pruebas OBL-P1..P3 (RED esperado en P1 y P2); `RoutingEnforcement: required`; correcciones autorizadas.
2. Relevo de salida.
3. **Controller de planificación** (`CONTROLLER_PLANNING`) → `delegation.json` por `-o`. La planificación nombra los archivos nuevos exactos.
4. **nc4** (solo en la primera delegación de la cadena), **antes** de aceptar la real: una copia de `delegation.json` que solo cambia `AllowedWriteScope`, más amplio que el contrato,
   se evalúa con A1-A8 sin cortocircuito. **Oráculo relativo:** A3 en `fail`, las demás con el mismo resultado que en la aceptación de la delegación real, y disposición STOP (P-03)
   registrada como `REJECTED_BEFORE_INVOCATION` en `artifacts/orchestration/<unit>/<task>-nc4/…`. El STOP queda confinado al control y no cuenta para ningún tope. Después,
   **aceptación** de la delegación real (§3.4). Si la real no pasa A6 por avance de `main`, nc4 se repite sobre la primera delegación de la cadena que lo pase.
5. Composición: `prompt.md` con la ruta de entrega concreta y su SHA-256, ≤ 200 líneas.
6. **Worker** (subagente según el paquete; la sesión no opera; tope de 60 min):
   1. si la entrega va a exigir RED (§9: `ChainRedSha` `null` o cambios en `ChainRedFiles`): en la primera delegación, añade la función como **esqueleto compilable** con la semántica actual de Cama (devuelve el nombre
      editado tal cual: el «fix desactivado») y escribe OBL-P1 y OBL-P2; en una posterior, desactiva la corrección existente e incluye en el mismo commit cualquier cambio de las pruebas
      de la cadena (`RT`, §9, incluida OBL-P3). En ambos casos las pruebas con `ExpectRed` del contrato **fallan por aserción** con selección > 0 y
      hace **commit RED** y push;
   2. implementa la función y su uso en `EditCama`, y añade OBL-P3 (en la primera delegación); desde aquí no modifica ningún archivo de `RT` ∪ `ChainRedFiles` (§9);
   3. GREEN focal con selección > 0 (Core y UI) y pruebas relevantes;
   4. ejecuta las **tres mutaciones** de OBL-P2 (§13) en el árbol, las registra y las revierte;
   5. **commit GREEN** y push, con el resumen de estado; escribe `worker-handoff.json` y termina.
7. Relevo de entrada y **hechos remotos** en el registro: `ls-remote`, corridas `push` del `RedSha` (si existe) y del `CurrentSha`, TRX descargados con conteos y nombres fallidos, y
   `origin/main`. La sesión espera, hasta **90 min** y consultando sin reinvocar a ningún participante, a la corrida de `CurrentSha` y, solo mientras exista y esté en curso, a la del
   `RedSha`. Si la de `CurrentSha` terminó y no hay corrida `push` del `RedSha`, registra la ausencia en `RemoteFacts` y pasa al paso 8. El paso 7 solo registra hechos: la disposición
   la fija la fila 9 de §8.3 (REWORK por la corrida del `RedSha` ausente, y BLOCKED por la del `RedSha` en curso, solo cuando la entrega exige RED; BLOCKED por la de `CurrentSha` ausente
   o en curso al vencer).
8. **Controller de verificación** (`CONTROLLER_VERIFICATION`) → `controller-verification.json`; el relevo valida esquema y coherencia.
   - **REWORK** autorizado → commit de `attempts` (§9) y vuelta al paso 2 con `CorrectionOf`, `ChainBaseSha`, `ChainRedSha` y `ChainRedFiles`;
   - **STOP por avance de `main`** → §3.6;
   - **otro STOP o BLOCKED** → §10.2.
9. **Controles negativos nc1..nc3**, una sola vez, **solo** sobre las entradas de la verificación `EXECUTION_VERIFIED` del último `WorkRunId` de la cadena. Si la cadena termina sin
   VERIFIED, no se ejecutan y G3 no cierra. Los ejecuta el Controller Codex real con `CONTROLLER_VERIFICATION` y el mismo prompt, salvo la ruta de entrada y el `RunId` de la invocación.
   Cada uno usa una ruta `<task>-ncN` y un registro propios, con copias de todas las entradas que conservan sus `TaskId`, `RunId`, `DelegationRunId` y `Attempt` internos salvo el campo
   mutado. No consumen `attempts`. **Oráculo relativo:** la comprobación mutada queda en `fail`, `FailureClass` es esa comprobación y todas las anteriores en el orden de §8.3 tienen el
   mismo `Result` (`pass`) que en la verificación real. Una salida ausente o inválida es un **fallo de transporte** de la fase `CONTROL`, reejecutable con `RunId` nuevo hasta **2 veces por
   control** (clave `TaskId`, `CONTROL`, ncN); agotado ese tope, STOP (P-04) y **control no superado**. «Una sola vez» se refiere a los controles con salida válida.
   - **nc1:** entrega con `CurrentSha` inexistente → `Identity`; `EXECUTION_BLOCKED/STOP`.
   - **nc2:** delegación cuyo `AllowedWriteScope` excluye un archivo de `git diff --name-only BaseSha..CurrentSha` de la delegación verificada (el primero en orden lexicográfico,
     registrado en el registro del control; si una entrada de prefijo lo cubre, se sustituye por la enumeración de los demás archivos del diff) → `Scope`; `EXECUTION_BLOCKED/STOP`.
   - **nc3:** entrega con un término de §8.4 en `WorkCompleted` → `FreeText`; `EXECUTION_REWORK_REQUIRED/REWORK`; además, las comprobaciones posteriores, igual que en la real.
10. La sesión reproduce como Executor una de las mutaciones de OBL-P2 y la registra; copia la custodia; escribe evidencia, estado y métricas; **commit de cierre de G3**; Core Full local
    sobre ese SHA con árbol limpio; push; lectura de su CI; build Debug del Plugin sobre ese SHA (AUTOMATION_PLAN §10: si AutoCAD bloquea los DLL, el gate queda en `plugin-build`; no se
    cierra AutoCAD).
11. **Decisión del Coordinator**, después del paso 10, con la precondición de §3.1. Se registra en el siguiente commit sustantivo (LIFECYCLE §7: los hechos posteriores al commit no se
    atribuyen a su cuerpo) y en el informe.

**Métricas** (F-06), en la evidencia del piloto: TaskProfile, Executor, Model y Effort (solicitados y efectivos), PromptProfile, ReworkLoops, ExecutionResult, CoordinatorResult y, si
están disponibles, ElapsedTime, ToolCalls (eventos `command_execution` o llamadas de herramienta de la transcripción) y tokens. Son **datos del piloto**, no métrica de LIFECYCLE §11. La
decisión de G3 propone, sin bloquear, ajustes del catálogo o del enrutamiento y evalúa los disparadores de B (§14).

## 13. Obligaciones invariante → prueba (LIFECYCLE §6)

**Restricciones de implementación de las pruebas Core:** `System.Text.Json`, sin paquetes nuevos ni validador de JSON Schema; raíz del repositorio por `RackCad.sln`; rutas con la
capitalización exacta (el runner de CI es ubuntu). El RED de cada aserción de contenido se demuestra con una **mutación en memoria** de la entrada real.

**INV-10 (nueva):** una delegación nunca amplía el contrato del Coordinator (garantía del mandato: «Codex must not silently expand architecture or product scope»).

**Frases testigo congeladas** (OBL-06): W-01 AGENTS «La `ref` se consulta en la corrida; no se infiere del `head_sha`.»; W-02 AGENTS «0 pruebas seleccionadas = FALLO»; W-03
WORKFLOW §3 «la sesión saliente deja commit + push +»; W-04 WORKFLOW §4 «si el trunk avanzó, **rebase** sobre su punta antes de escribir una»; W-05 AUTOMATION_PLAN §9 «Un intento es
una secuencia de diagnostico, correccion acotada, validacion local, commit y push.»; W-06 AUTOMATION_PLAN §12 «El ejecutor se detiene y deja informe cuando ocurra cualquiera de estas
condiciones»; W-07 LIFECYCLE §10 «No copia rutinariamente historia, WORKFLOW completo, ADR completos». La prueba comprueba primero que cada frase sigue en su fuente (con espacios
normalizados); si no, falla por conjunto obsoleto.

**Ruta operativa** (OBL-04): un destino de tráfico transitorio **dentro del worktree de la unidad**. El README las escribe en bloques de código con la etiqueta `text operativo`, y se
incluyen las salidas `-o`, los directorios de `RunId`, las entradas y salidas del paquete y el patrón de `ExpectedHandoffPath`. **No** son rutas operativas las de custodia, el archivo de
estado, `config.toml` ni las réplicas de sonda fuera del repositorio (U-04).

| ID | Obligación | Prueba / control | RED esperado |
|---|---|---|---|
| OBL-01 | INV-01: no autoaprobación | Core: estados exactos de worker y controller; ningún enum de los cinco esquemas contiene un término de §8.4. Texto libre: `FreeText` del Controller y nc3; revisión manual registrada del Coordinator sobre la verificación | falla sin esquemas; mutación que añade `GATE_PASS` a un enum |
| OBL-02 | INV-09: esquemas estrictos | Core: comprobación **recursiva** de §8; aceptación por `--output-schema` en PR-1 (G2) y en la primera verificación de G3 | mutación en un objeto **anidado** (propiedad fuera de `required`; `additionalProperties` ausente) |
| OBL-03 | INV-02: identidad | Core: todo campo SHA con exactamente `^[0-9a-f]{40}$`; nc1 real | mutaciones: sin `pattern`; `{7,40}` |
| OBL-04 | INV-05: tráfico transitorio | Core: `.gitignore` contiene `artifacts/` y ninguna línea posterior lo niega; toda ruta operativa empieza por `artifacts/orchestration/`; `.agent/` prohibido solo en rutas operativas, no en la prosa | mutaciones: línea `!artifacts/orchestration/`; ruta operativa `.agent/x`; ruta operativa fuera de `artifacts/orchestration/` |
| OBL-05 | INV-06: separación | Core: el conjunto prohibido en `routing.md` se **deriva** de la columna de modelo de `model-catalog.md` más patrones genéricos de proveedor; el catálogo declara NO NORMATIVO y cada entrada tiene fecha, URL, tipo de fuente, nivel, tabla de effort, consumo y retiro | mutaciones: identificador tomado del catálogo; identificador sintético con forma de nombre futuro; columna obligatoria ausente |
| OBL-06 | INV-07: composición | Core: §G con los 8 perfiles, contrato base con los 8 campos de LIFECYCLE §10, base ≤ 40 líneas y perfiles ≤ 25; ninguna frase testigo en §G, README, `routing.md` ni `prompting-guide.md`; cláusula de subordinación en los cuatro subordinados de texto | mutaciones: frase testigo insertada; perfil de 26 líneas; campo de LIFECYCLE §10 ausente; cláusula retirada |
| OBL-07 | INV-03/INV-08: fail-closed | nc1 y nc2 reales con el oráculo relativo de §12.1 | control no superado = obligación incumplida |
| OBL-08 | INV-04: conteo | Control manual: Coordinator y Architect verifican en G2 la tabla de escenarios de §9 contra AUTOMATION_PLAN §16 y el README, y en G3 la aplican al registro del piloto | cualquier discrepancia |
| OBL-09 | Relevo y cesión | Control manual por relevo con el registro de relevo. Condiciones y disposición: participante ajeno, proceso no atribuible o más de una delegación abierta → STOP P-02; hash de `config.toml` cambiado → STOP P-01; la sesión operó durante la cesión → STOP P-08 | cualquiera de esas condiciones |
| OBL-10 | INV-10: contrato | Aceptación A3-A5 antes de invocar; nc4 con el oráculo de §12.1; comprobación `Contract` del Controller | nc4 con otro resultado = obligación incumplida |
| OBL-11 | VERIFIED ⇔ 14 `pass` | Core: el objeto `Checks` tiene exactamente los 14 ids, todos requeridos. Control manual en G2: el relevo rechaza dos fixtures (VERIFIED con un `fail`; VERIFIED con un `not_run`) | mutación que retira un id; fixture aceptado = incumplida |
| OBL-P1 | Piloto: resolución del nombre editado | Core, función pura: blanco → nombre del sobre; espacios → nombre del sobre; blanco con sobre `null` → `null`; nombre propio → el editado | RED por aserción contra el esqueleto, commit RED con CI propia |
| OBL-P2 | Piloto: cableado de `EditCama` | Guarda Core. **Falla si:** (a) el argumento del payload no es el valor resuelto; (b) `SyncName` no recibe ese mismo valor (vía `LinkedBase`); (c) `window.RackName` sigue como argumento directo en alguno de los dos sitios; (d) los argumentos editado/sobre están invertidos | RED por aserción antes del cambio; **tres mutaciones** registradas: solo el sitio del payload, solo el de `SyncName`, argumentos invertidos |
| OBL-P3 | Piloto: contrato de entrada de la ventana | Prueba STA: con el campo vacío, `window.RackName` es `""` (la resolución vive en `EditCama`) | GREEN (caracterización); corrida focal UI con selección > 0 en la evidencia de G3 |
| OBL-P4 | Piloto: extremo a extremo | **Owner Validation** (§16) | no automatizable: el proyecto de pruebas no referencia el Plugin |

**Conformidad con AGENTS.** El invariante conductual tiene prueba conductual (OBL-P1). La guarda (OBL-P2) protege solo la frontera estática del cableado del Plugin. La OV cubre el extremo
a extremo y no sustituye la evidencia automatizada. OBL-01..06 y OBL-11 protegen fronteras estáticas de artefactos que son el propio producto del protocolo.

## 14. Nivel de solución: **A**

**A = documentación + esquemas + piloto real con relevo.** Motivos: el Controller Codex funciona por CLI en solo lectura (MEASURED); el Worker se invoca como subagente de la sesión, con
la capacidad pendiente de la sonda U-04; la validación de esquemas usa una herramienta existente (`Test-Json`); y los fallos medidos del transporte (stdin, `pwsh`, JSON conforme pero
vacío, efecto sobre `config.toml`) se **detectan** con la receta y las comprobaciones del relevo. Con un solo piloto no hay evidencia de que mantener scripts compense.

**Disparadores de B**, que el Coordinator evalúa en la decisión de G3 como recomendación sin efecto dentro de I-61: errores de relevo atribuibles a pasos manuales; verificaciones
idénticas repetidas; tiempo de relevo dominante. **C** no está justificado.

## 15. Puntos de extensión

| Extensión | Autoridad y procedimiento |
|---|---|
| Entrada o dato del catálogo | La sesión responsable de una unidad, con fuente oficial o medición, tipo de fuente y fecha; sin Freeze. No cambia la elegibilidad de una delegación ya aceptada |
| Clase de tarea o perfil nuevo | Cambio del subordinado o de PROMPT_TEMPLATES §G en la rama de una unidad, revisado por el Coordinator contra §§4 y 7; sin alterar estados, esquemas ni reglas de §2.3 |
| Cambio de esquema | Versión nueva (`/v2`) con ADR o A-n según la materialidad; `/v1` sigue válido hasta su retirada explícita |
| Regla de §2.3 cuyo texto normativo está en AUTOMATION_PLAN §16 | Solo en esa sección, con las autoridades de ese documento y, si es materia OWNER-RESERVED, del Owner |

**FOUNDATIONS** (LIFECYCLE §4.1): el contrato declara `introduces: Agent Execution Protocol`. La entrada factual se **redacta en G2** en la evidencia de la unidad, se completa tras G3,
existe antes de READY-04, se verifica en READY-06 y se publica en el commit de cierre (WORKFLOW §11.4). Solo podrá ser `STABLE` con ADR-0046 aceptado o el Freeze integrado.

## 16. ADR-0046, plan de gates, READY y Owner Validation

### 16.1 ADR-0046 (propuesto; C61-G1-04)

Se crea **con esta Proposal**, antes de implementar, en estado `propuesto`. Su fila del índice se escribe en el commit de cierre (WORKFLOW §11.4), con ventana de I-52 en ese momento. Al
publicarlo, el número ADR-0046 se **notifica** a I-52 por el canal entre sesiones, sin escribir en el índice.

- **No bloquea** el Freeze, G2 ni G3: el README de ADR y WORKFLOW §8 exigen crearlo antes de implementar, no aceptarlo.
- **Bloquea `READY-03`** («sin decisión material/Owner pendiente»), y por tanto `FINAL_CANDIDATE_SHA`.
- Su estado cambia **solo** después de una decisión del Owner registrada de forma atribuible en `decisions/I-61.md`; el commit se limita a registrarla y cambia el SHA, así que precede al
  Candidato.
- **Rechazo:** BLOCKED — OWNER DECISION, sin Candidato. Exige una A-n OWNER-RESERVED que retire o reformule los cambios normativos antes de reiniciar READY-02.

**DEV-G1C-01:** por aplicación literal de READY-03, la decisión del Owner sobre esa desviación también es una decisión pendiente de la unidad. Esto **sustituye** la frase «No bloquea
nada del flujo» de decisiones §11 y del contrato §6. La sustitución se registra con una **entrada nueva** en decisiones que cita READY-03, sin editar §11; el contrato (§6 y la
materialidad M-08) se actualiza en un commit distinto del de Freeze.

### 16.2 Plan de gates (se congela)

| Gate | Resultado verificable | Evidencia de cierre |
|---|---|---|
| **G2 — Protocolo materializado** | Subordinados, cinco esquemas, PROMPT_TEMPLATES §G y cabecera/§2, AUTOMATION_PLAN (§16, línea 3, párrafo de autoridad y frase de §2), fila de WORKFLOW §10, línea del pack; `AgentExecutionProtocolTests` RED→GREEN (OBL-01..06, OBL-11 estructural); **sonda PR-1**: una invocación Codex `read-only` en el worktree de la unidad, sin `--ephemeral`, con la celda que `routing.md` y el catálogo dan para CONTROLLER_* (nivel y effort de `Balanced`) y `--output-schema` del esquema de delegación; mide la aceptación del esquema, modelo y effort efectivos en el registro de sesión, consumo sin P-06 y hash de `config.toml` sin cambio, con **oráculo contra Git** (`ExpectedBranch` y `BaseSha` de la salida son la rama y el `HEAD`, y `RoutingReason` empieza por el texto fijo `PR1: ls-files=<n>`, donde `n` es el número de líneas de `git ls-files`; los tres iguales a los de Git; y el flujo `--json` contiene al menos un `command_execution` completado), y es la prueba de invocación de esa celda; controles manuales OBL-08 y OBL-11 (fixtures); borrador de FOUNDATIONS | RED→GREEN con selección > 0; Core Full local sobre el SHA de cierre; CI exacta de `push`; revisión del Coordinator contra el Freeze |
| **G3 — Piloto real** | Flujo de §12.1 completo: U-04; Controller Codex real de planificación y verificación; Worker enrutado con modelo y effort efectivos registrados y comparados (`required`); OBL-P1/P2 RED→GREEN con CI del `RedSha` y del `CurrentSha`; OBL-P3 con corrida focal UI; nc1..nc4 con su oráculo; OBL-09 por relevo; métricas; decisión del Coordinator | Custodia versionada; Core Full local sobre el SHA de cierre; CI exacta; build Debug del Plugin sobre ese SHA |
| **READY-01..09** | En orden. `READY-03` exige las decisiones del Owner sobre **ADR-0046** y sobre **DEV-G1C-01** | — |
| **Candidato** | Tras READY-09: Core Full y UI Full locales, builds Debug de UI y Plugin, CI exacta, cobertura por `workflow_dispatch` con el SHA medido y paquete de OV | AGENTS.md; guía manual §7.1 y §7.2 |

**PR-1 negativa:** si `--output-schema` rechaza una construcción no congelada, se ajusta dentro de G2 y la nueva sonda es una reejecución de la fase `PROBE` dentro del tope de §9, con el
`analysis.md` del ajuste versionado; si afecta a un elemento congelado, BLOCKED y A-n de Coordinator + Architect antes de G3.

**Tope de invocaciones de Codex:** G2, una (PR-1); G3, **5 + 2 por cada corrección o recuperación** (planificación, verificación y nc1-nc3; cada corrección o recuperación añade una
planificación y una verificación, o solo una reverificación en §3.6), con las correcciones acotadas por `AttemptsRemaining` y las recuperaciones por el máximo de 2 de §3.6; cada
rebase de apertura con cadena en curso (§3.5) añade lo mismo que una recuperación; en ambos casos, más las reejecuciones del tope de §9. Una invocación que excedería el tope **no se lanza**: STOP (P-07).

**Deberes de la guía** §7.2 (identidad del DLL: `ProductVersion` y SHA-256) y §8 (pregunta de duración activa): los cumple la **sesión responsable** en rol de Executor, nunca un Worker.

### 16.3 Matriz de Owner Validation (guía §7.2; se ejecuta sobre `FINAL_CANDIDATE_SHA`)

| OV-id | Escenario | Disparador de aplicabilidad | Sistemas | Datos nuevo/legacy | Resultado esperado | Unidad | Hito intermedio |
|---|---|---|---|---|---|---|---|
| OV-I61-01 | Vaciar el nombre al editar una cama nueva | Cambia la resolución del nombre editado en `EditCama` | Cama | Nuevo: `QUICKCAMA` en un DWG nuevo | `RACKEDITAR` → vaciar → Actualizar: `RACKLISTA` conserva «Cama N»; guardar, cerrar y reabrir: `RACKEDITAR` muestra «Cama N»; después, `QUICKCAMA` → «Cama N+1» y su bloque sin sufijo (D-3) | I-61 | no |
| OV-I61-02 | Renombrar a un nombre propio | Ídem | Cama | Nuevo | Se conserva tras guardar y reabrir; el bloque se renombra | I-61 | no |
| OV-I61-03a | Cama legacy con `Name = ""` | Ídem (sobre con nombre vacío) | Cama | Legacy: DWG que el Owner produce, vaciando el nombre, con un DLL sin la corrección construido desde `95690c28` | Con el DLL candidato, Actualizar con el campo vacío: sigue «(sin nombre)» sin error; asignar después un nombre propio funciona | I-61 | no |
| OV-I61-03b | Cama legacy con `Name = null` | Ídem (sobre sin nombre) | Cama | Legacy: DWG que el Owner produce, creando la cama sin tocar el nombre, con un DLL construido desde `95690c28^1` (primer padre del merge de I-60) | Actualizar sin tocar el nombre: «(sin nombre)», sin error | I-61 | no |
| OV-I61-04 | Checklist general aplicable | Comportamiento de edición y round-trip | Cama y las cinco rutas de edición restantes | Nuevo | Guía §5 aplicable, en particular §5.2 | I-61 | no |
| OV-I61-05 | Revisión del protocolo | Entrega del protocolo y del piloto | n/a (proceso) | n/a | El Owner acepta o rechaza el resultado frente a los criterios de §18 (Q-04) | I-61 | no |

**DLL legacy de 03a y 03b:** la sesión los construye desde `git archive` del SHA indicado, extraído en una raíz corta fuera del repositorio (`%TEMP%\i61l-<sha8>`, por la longitud de ruta
de las salidas WPF), sin worktree nuevo ni cambio del estado Git, con
`dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug -p:SourceRevisionId=<SHA de origen>`. Antes de entregarlos al Owner exige que la `ProductVersion` observada termine en
`+<SHA de origen>`; si no, detiene la entrega, como la guía §7.2 exige para el Candidato. Registra para cada uno el SHA de origen, la `ProductVersion` y el SHA-256.

## 17. Materialidad y riesgos

**Materialidad:** M-01, M-02 (creador), M-04, M-05, M-06, M-07 y **M-08** → **NEW ARCHITECTURE**. El piloto es EXTENSION.

- **M-08, activada:** la ampliación de §2.1 modifica la asignación de la fuente de decisión integrada (Proposal V4 de I-56, P-17: «AUTOMATION_PLAN solo implementa su ejecutor»),
  citada por ADR-0045 #13 («Autoridad por dominio»). No contradice #13: sigue habiendo un único dueño por regla. Pero cambia el mapa aprobado, y por eso es OWNER-RESERVED (§2.1). El
  contrato se actualiza con M-08.
- **EXP-07, reverificada** el 2026-09-30 sobre `origin/main` `95690c28` (fetch del día): ninguna rama activa toca AUTOMATION_PLAN, WORKFLOW, PROMPT_TEMPLATES,
  `documentation-governance.md`, FOUNDATIONS, `RackCamaCommands.cs` ni `src/RackCad.Application/Persistence/`. Solo `feature/rackmirror-espejo-semantico` (I-52) toca
  `docs/adr/README.md`, que esta unidad escribe en el commit de cierre con ventana coordinada.

**Riesgos y mitigaciones** (cubren los 12 puntos de «ARCHITECT REVIEW»):

| Punto del mandato / riesgo | Mitigación |
|---|---|
| Inversión de autoridad; Codex como Master | §3.1; el Controller no escribe en Git; GATE PASS solo del Coordinator; OBL-01 |
| Autoaprobación del Worker | Estados exactos; `FreeText`; nc3; trabajo terminado solo con VERIFIED |
| Vuelta de la duplicación de prompts | Tabla de §2.3; cláusula de subordinación; OBL-06 con frases testigo en los cuatro subordinados |
| Enrutamiento demasiado complejo | Regla única de dimensiones (§4.1); algoritmo de seis pasos; amplitud solo registrada |
| Catálogo obsoleto | `STALE` con retiro anunciado; tipo de fuente; aplicación en la planificación y en A7 |
| Dependencia insegura de Computer Use | §11: nunca bus normativo; protocolo idéntico en todo nivel |
| Bucles autónomos sin límite | §9: `attempts`, contador por clase y tope de BLOCKED; tope de invocaciones de §16.2 (P-07) |
| Pérdida de exact-SHA | `AuthorityRevision`, `MainSha`, `BaseSha`, `RedSha`, `CurrentSha`; `Identity`, `Remote`; OBL-03; nc1; §3.6 |
| Carreras entre agentes | §§3.2-3.5: relevo, cesión, procesos vivos, una delegación abierta; OBL-09 |
| Contaminación de Git | Ruta ignorada; OBL-04; custodia solo de JSON y MD en la evidencia |
| Métricas como burocracia | Las no disponibles no bloquean (F-06) |
| Efectos laterales de Codex en la configuración (DEV-G1C-01) | Solo el worktree de la unidad; hash y nombres de claves antes y después; STOP P-01 |
| Independencia parcial de la verificación | Controller de otro proveedor; CI en un runner ajeno; controles negativos; el Coordinator puede contrastar la corrida con `gh run view` |
| Consumo del `service_tier` heredado (efecto UNKNOWN) | Riesgo registrado (§4.3); tope de invocaciones; STOP P-06 |

## 18. Criterios de éxito del mandato → mecanismo, gate y evidencia

| # | Criterio | Mecanismo | Gate / OBL | Evidencia | Verificador |
|---|---|---|---|---|---|
| 1 | Coordinator authority remains intact | §3.1; tabla de §2.3 | G2; OBL-01; nc3 | Documentos y verificaciones del piloto | READY-06; OV-I61-05 |
| 2 | model/effort routing is explicit | `routing.md`; campos de enrutamiento; `Routing` con `required` | G3; U-04 | Delegación y registros de relevo con solicitado y efectivo | Coordinator en G3. **Limitación declarada:** en una celda sin control de effort, el effort no se enruta: se registra el heredado medido en U-04 y se compara |
| 3 | current models separated from stable rules | catálogo no normativo | G2; OBL-05 | Prueba Core | CI y Core Full |
| 4 | prompt profiles exist | PROMPT_TEMPLATES §G | G2; OBL-06 | Prueba Core | CI y Core Full |
| 5 | delegation is machine-readable | `rackcad-delegation/v1` | G2-G3; OBL-02 | `delegation.json` válido del piloto | Aceptación A1; Controller |
| 6 | handoff is machine-readable | `rackcad-worker-handoff/v1` | G3 | `worker-handoff.json` válido | Controller (`Handoff`) |
| 7 | filesystem/CLI is primary transport | receta de §11 | G3 | Registros de relevo (archivos y CLI) | READY-06 |
| 8 | Computer Use is fallback | §11 | G2 | Documentos; el piloto no lo usa | READY-06 |
| 9 | exact-SHA is preserved | identidad de §9; `Identity`, `Remote` | G3; OBL-03; nc1 | Verificación y nc1 | Controller; Coordinator |
| 10 | write ownership is exclusive | §§3.2-3.5; `Scope` | G3; OBL-09; nc2 | Registros de relevo; nc2 | Coordinator |
| 11 | STOP/rework behavior is deterministic | §§9-10; bucles de §12.1 paso 8 | G2 (OBL-08, documento); G3 (nc1, nc2 STOP; nc3 REWORK) | Tabla de escenarios; controles | Coordinator y Architect. **Limitación declarada:** el bucle de corrección completo solo se ejerce si el piloto produce un REWORK real; si no, se verifica por documento y por los controles |
| 12 | one real pilot completes | §12 | G3 | Custodia y evidencia del piloto | Coordinator; OV-I61-01..04 |
| 13 | results/metrics are documented | métricas de §12.1 | G3 | Evidencia del piloto | Coordinator |
| 14 | usable by a future V2 initiative without a mega-prompt | composición de §7 | G2; G3 | `prompt.md` del piloto: base + perfil + delta, SHA-256, ≤ 200 líneas, sin frases testigo | OBL-06; Coordinator; OV-I61-05 |

# I-62 — Revisión formal del Architect de la enmienda A-1 (registro)

```text
Emisor:           Architect (sesión de escritorio nueva de Claude Code, invocación acotada única autorizada por el Coordinator; modo declarado SEPARATE SESSION)
Naturaleza:       dictamen del Architect; no es disposición del Coordinator, acuerdo de A-1 ni autorización de F4
Fecha:            2026-10-05 (UTC)
Texto literal:    SÍ recibido: output.json en docs/automation/evidence/I-62-architect-A-1/R20261003T023945Z-cab7/ (SHA-256 1a6f2c4b…), extraído de la
                  transcripción; coincide con la copia que pegó el Owner
Forma:            representación experimental de rackcad-architect-review-result/v1 (V14 B.10.1); no autoridad normativa
Objeto revisado:  commit bf7b0d9c4ec79e38350bad55e0debcde55b3cec9
                  docs/initiatives/I-62-A-1.md, blob 09ca93285975c4b7af6471d6ae91bfa12c94a1fc
                  docs/initiatives/I-62-architect-package-A-1.md, blob 071cecba1fb8bdefc9a4e5554dd47a0459f8a1d2
Veredicto:        CHANGES REQUIRED: A62-A1-01..06 REQUIRED; A62-A1-O1..O6 OPTIONAL; OwnerDecisionRequired = false
Acreditación:     auditoría mecánica NOT_ACCREDITED (16 motivos, clasificados en la evidencia); la decide el Coordinator
Estado:           A-1 PROPUESTA sin cambios · veredicto del Coordinator PENDING · F4 producción = NO AUTORIZADA · I-61 vigente
```

Registro redactado por la sesión autora como custodia ordenada por el Coordinator. Resume el resultado; si hay discrepancia, manda `output.json`. La sesión
**no** ratifica, rebaja, cierra ni corrige ningún hallazgo, y no edita A-1.

## 1. Contexto y acreditación (hechos medidos por el invocador)

- **Runtime observado:** `claude-opus-5-5`, effort `xhigh`, Claude Code 2.1.286, sin subagentes. Worktree propio, sin memoria del proyecto del autor;
  lanzada por el Owner desde la tarea `task_c6305551`.
- **Fidelidad:** el preflight y lo entregado al revisor son FAITHFUL_NORMALIZED (36 registros). El revisor contrastó con `git show | sed` las cuatro líneas
  de más de 2 000 caracteres. Las 39 citas de premisas se encuentran en su archivo y en sus líneas.
- **Contexto:** todas las rutas de archivo leídas están en el cierre. Hubo dos Grep sobre directorios enteros, fuera del cierre, y seis comandos de
  metadatos de solo lectura que no están en la lista de acciones; el revisor declaró ambas cosas. Ninguna premisa usa material fuera del cierre y no hubo
  escrituras. `dotnet test`, permitido, dio 12 419/12 419 en el clon.
- **Auditoría mecánica:** NOT_ACCREDITED con 16 motivos. Cinco son acciones permitidas en otra forma y uno es un falso negativo de la auditoría; el
  detalle está en el README de la corrida, §4. La acreditación es del Coordinator.

## 2. Hallazgos REQUIRED

| Id | Delta afectado | Defecto | Corrección que pide el Architect |
|---|---|---|---|
| A62-A1-01 | D1-1..D1-4, D1-8, D1-9 | Una sustitución o enmienda de la autorización **con el bucle abierto** no tiene ningún estado que sea a la vez válido y conforme al Freeze. Si la solicitud lleva A2, no hay entrada; si lleva A1, D1-3 y D1-4 fallan; si se admite una entrada A2 nueva, el presupuesto se reinicia en silencio y los topes suben sin A-n | Separar la clave del presupuesto de la autorización de acción: una entrada por **bucle**, creada en NONE → REVIEW_PENDING con una clave inmutable (p. ej., `loop.budget_key`); `review_requests.authorization_id` = esa clave; D1-3 sobre la clave del bucle. Con una sustitución no hay entrada nueva ni reinicio, y los caps son el mínimo de los vigentes y los de la sustituta. Obligaciones en C-34, C-38 y C-40 |
| A62-A1-02 | D1-5, D1-9 | LOOP_CLOSED no cubre EXPIRED ni REVOKED fuera de ESCALATE_OWNER. Tras pasar Until, el bucle no puede cerrarse ni continuar, porque I-P13 exige SUPERSEDED en una vigencia ya terminada. No hay camino a un segundo bucle | Tercer origen de D1-5: cualquier fase con vigencia ENDED y `ended_reason` ∈ {EXHAUSTED, EXPIRED, REVOKED}, con una decisión del Coordinator que cierra y la escalada resuelta (sin SUPERSEDED). Alternativa: D1-9 permite continuar con una autorización nueva tras EXPIRED o REVOKED. Positivos en C-38 y variantes (e) y (f) en C-40 |
| A62-A1-03 | D1-1..D1-5, D1-7, D1-8 | Falta el alcance por `loop.type`. Un bucle REVIEWER autorizado por contrato de gate no tiene `ReviewLoopAuthorization`, así que no puede tener entrada en `budgets[]` y no tiene salida a NONE. La ruta REVIEWER de §20.7 y §11.3 deja de ser representable. Además, la segunda frase de D1-3 alcanza a EXECUTION | O bien (a) D1-1..D1-9 rigen solo para ARCHITECT_REVIEW y A-1 fija la clave y la salida de REVIEWER y EXECUTION, o bien (b) una clave determinista para toda autorización de bucle (incluida contrato de gate + rol) y LOOP_CLOSED desde REVIEWER. Obligaciones en C-38 y C-29/C-31 |
| A62-A1-04 | D2-1, D2-2, D2-3, D2-6 | El Target de un intento en LAUNCHING queda como histórico, pero el caso B.1 lo devuelve a BUDGET_RESERVED tras una reconciliación. Entonces I-H02 falla y el RebaseMap no tiene entrada; la reanudación determinista sin consumo pasa a ser un STOP o la pérdida de la reserva | Registrar en StateFields el Original → Image del Target de todo intento no terminal en LAUNCHING, sin reescribir su invocación. El par LAUNCHING → BUDGET_RESERVED posterior lleva una invocación replanificada (InvocationId nuevo, Target = imagen, mismo `reserved_at`). Variantes en C-29 y C-15 |
| A62-A1-05 | D2-1, D2-2, D2-5 | Se reconcilia el objeto de una solicitud OPEN mientras su intento LAUNCHED conserva el Target histórico. Al ingerir, el `EvaluatedObject` difiere del objeto de la solicitud, así que el resultado es INVALID por §20.5.2 e I-S18: una revisión válida del mismo blob se pierde y consume una reejecución | Nuevo delta (p. ej., D2-9): hay coincidencia si path y blob son iguales y el commit del Target es el Original de la imagen registrada (mapa o cadena de mapas). Enmendar §20.5.2 e I-S18. Variantes positiva y negativa en C-15 y C-29 |
| A62-A1-06 | D2-1..D2-8 y §1 («No cambia F1, F2 ni F3») | Tras el rebase, `AuthorizationRef.Commit` y `BindingRef.Location.Commit` de los bindings custodiados son originales reescritos. Así fallan I-S18, AUTOMATION_PLAN 16.20 y README §14.3 y §14.4 regla 3 (materializados en F3), y los positivos que A-1 añade (C-38, C-29 (g), C-15, C-36) no se pueden satisfacer sin reinterpretar F3 en silencio | Delta explícito para las identidades de rama dentro de artefactos custodiados: ancestría evaluada sobre la imagen según la cadena de RebaseMaps, alcanzable desde el estado; la invocación replanificada se reconstruye entera en la imagen; corregir §1 sobre F3 o declarar la prevalencia por enmienda. Obligaciones con uno y dos rebases en un clon limpio |

## 3. Hallazgos OPTIONAL

| Id | Nota (resumen) |
|---|---|
| A62-A1-O1 | Escribir como regla de par de D1-4 la aparición de exactamente una entrada nueva, ausente en p, para dar a C-34 (g) y C-38 un oráculo exacto |
| A62-A1-O2 | La nueva lectura de `BudgetSnapshot` (B.9) debe ir como delta explícito, no solo como justificación en M-05 |
| A62-A1-O3 | Precisar «binding ARCHITECT del bucle» para los linajes heredados, y que estos entran en `OpenFindings` del bucle nuevo; conservar los linajes como de unidad es correcto |
| A62-A1-O4 | Fijar el `ended_reason` cuando la decisión que cierra termina la vigencia, y conservar el fin de la autorización cerrada tras poner `action_validity` = `null` |
| A62-A1-O5 | El script de contra-ejemplos diverge de A-1 (EXHAUSTED como fase; la línea 100 acepta `loop.object` → `null`; el cierre no comprueba `escalation`, `loop.type` ni `findings`) y le faltan trazas para cada REQUIRED |
| A62-A1-O6 | M-01 = no es defendible; conviene declarar que el presupuesto de revisión de la unidad pasa a ser una sucesión de presupuestos acotados por decisiones del Coordinator |

## 4. Preguntas de la orden

- **FC-01:** puntos 2, 4, 5, 6, 8 y 10 CONFIRMED; 1, 3, 7 y 9 CHALLENGED (por A62-A1-01, -02 y -03).
- **FC-02:** puntos 2, 3, 4, 5, 6, 7, 9 y 10 CONFIRMED; 1 y 8 CHALLENGED (por A62-A1-04, -05 y -06).
- **Contraste con F3:** `role-invocation/v1` Target, `BudgetSnapshot` e `input closure/fidelity` COMPATIBLE. `BindingRef`, `AuthorizationRef` y la
  acreditación histórica REQUIRES_CHANGE (reglas, sin cambio de esquema; A62-A1-03, -05 y -06).
- **Contra-ejemplos:** 14 de 15 trazas con el motivo declarado. `fc01-enmendado-object-null-fuera-del-cierre` sale INVALID por una causa incidental (la
  regla del `null` no está codificada; O5). Salida reproducida byte a byte frente al blob publicado.
- **Autoridad del Owner:** gasto, credenciales, acciones destructivas, OV y política = NONE; `OwnerDecisionRequired` = false.
- **Aplicabilidad:** solo I-62; sin cambio retroactivo en I-61; sin reclamar la corrección de la deuda de I-64.

## 5. Acción siguiente que recomienda el Architect

Custodiar el resultado (hecho en este registro y en su carpeta). El Coordinator dispone A62-A1-01..06 y los OPTIONAL. Después, la sesión autora prepara una
versión corregida de A-1 (blob nuevo, todavía PROPUESTA), con delta explícito, disposición por id y trazas nuevas para cada REQUIRED. Esa versión exacta
necesita una nueva revisión acotada del Architect, con su propia autorización. La producción de F4 afectada sigue bloqueada; no se requiere decisión del
Owner.

## 6. Lo que este registro no hace

No dispone los hallazgos ni declara la acreditación, AGREED o CHANGES ACCEPTED. No edita A-1, no crea su versión corregida, no abre F4 y no lanza otra
invocación. Esas decisiones son del Coordinator.

# I-64 F1-T1-MODEL — Decisión del Coordinator: REISSUE_GATE_CONTRACT_ASCII (planificación 7/7)

Transcripción literal de la orden del Coordinator recibida por el chat de la sesión responsable el 2026-10-02, tras el STOP final de la
planificación 6/6 (`R20261002T165806Z-eef1`). Por la propia orden («No hagas un nuevo commit antes de planificar»), se versiona después
de nc1..nc3.

```text
Continúa I-64 desde:
TARGET_SHA:
0b7db52f12ddbf7ed36c8c2d020645818335c0fa
origin/main:
819955d61a6da4c811a11fbd11b5dca13f634b7c
Task:
F1-T1-MODEL
Estado:
attempts = 3 / 3
Attempt = 3
Worker restante = 1
Verification productiva restante = 1
La planificación:
R20261002T165806Z-eef1
queda:
REJECTED_BEFORE_WORKER
P-03
A5 = FAIL
No Worker fue invocado.
Se adopta la opción:
REISSUE_GATE_CONTRACT_ASCII
No se cambia de Controller.
Motivo:

* gpt-6-luna/high es la única celda CONTROLLER_* actualmente medida y elegible;
* gpt-6.1-sol no está medida para CLI/read/tool-use y no es elegible;
* sondear otra celda introduciría una capacidad nueva innecesaria;
* el defecto observado está limitado a reproducción de texto no ASCII en salida estructurada.

No se cierra T1 sin nc1-nc3.
Esta decisión NO crea una A-n.
El gate-contract es subordinado y lo redacta el Coordinator.
La reemisión ASCII:

* NO cambia autoridad;
* NO cambia arquitectura;
* NO cambia comportamiento observable;
* NO cambia alcance;
* NO cambia IDs;
* NO cambia RequiredTests;
* NO cambia ExpectRed;
* NO cambia StopConditions por ID;
* NO cambia invariantes por significado;
* NO cambia Freeze.

Solo cambia la representación textual de cadenas descriptivas del contrato.
Reemite gate-contract.json para F1-T1-MODEL.
Mantén exactamente:
Schema
Unit
Gate
TaskId
AuthorityRevision
MainSha
AllowedWriteScope
ForbiddenWriteScope
RequiredTests
EligibleCells
RoutingEnforcement
CorrectionsAuthorized
y todos los IDs existentes.
Para TODOS los campos descriptivos libres:
Objective
Authorities[].Section
Invariants[]
ExpectedEvidence[]
StopConditions[].Text
IssuedBy
usa exclusivamente ASCII.
Regla de transliteración:
á → a
é → e
í → i
ó → o
ú → u
ü → u
ñ → n
y equivalentes para mayúsculas.
Los signos no ASCII deben sustituirse por una forma ASCII equivalente cuando sea necesario.
Ejemplos:
sesión → sesion
Selección → Seleccion
Diagnóstico → Diagnostico
escritura → escritura
NO uses escapes Unicode.
NO uses \uXXXX.
NO uses caracteres acentuados literales.
Después de crear el contrato:
assert:
todos los bytes del JSON están en rango ASCII 0x00..0x7F
y además:
ningún byte de control aparece salvo:
TAB, LF y CR cuando el formato JSON los permita.
Preferencia:
UTF-8 sin BOM, pero contenido 100% ASCII.
Antes de emitirlo, la sesión debe comparar contrato anterior vs contrato ASCII.
Para cada colección:
Authorities
AllowedWriteScope
ForbiddenWriteScope
Invariants
RequiredTests
ExpectedEvidence
StopConditions
EligibleCells
demuestra que:

* misma cardinalidad;
* mismos IDs;
* mismas rutas;
* mismas clases;
* mismos MinSelected;
* mismos ExpectRed;
* mismos scopes;
* mismas capacidades;
* misma decisión semántica.

Las diferencias permitidas son SOLO:

* eliminación de diacríticos;
* sustitución de puntuación no ASCII por ASCII equivalente.

Si aparece cualquier otra diferencia:
STOP antes de planificar.
Se autoriza UNA planificación adicional:
Planning maximum = 7
Consumed = 6
Remaining = 1
Esta es la última.
No incrementa attempts.
Attempt permanece:
3
AttemptsRemaining:
0
No se amplían Worker ni Verification.
No hagas un nuevo commit antes de planificar.
BaseSha:
0b7db52f12ddbf7ed36c8c2d020645818335c0fa
HEAD = remote debe seguir siendo ese SHA.
AuthorityRevision:
usa la revisión vigente compatible con este contrato reemitido según I-61.
MainSha:
819955d61a6da4c811a11fbd11b5dca13f634b7c
ChainBaseSha:
e0587355b0e84d98807057a56dfc50591da87489
ChainRedSha:
a9778068af566bc2b84ba6a0005711b6452a2666
ChainRedFiles:
las 5 rutas acreditadas.
Controller:
gpt-6-luna
high
CONTROLLER_PLANNING
read-only
El prompt también debe ser ASCII puro.
Instrucción crítica:
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
Todas las cadenas canónicas ahora son ASCII y deben copiarse byte por byte.
TaskClass esperado para el Worker:
Documentacion
o la etiqueta exacta ASCII que resulte de routing.md para la tarea documental autorizada.
El Worker sigue siendo:
claude-sonnet-5-5
subagent
medium
Trabajo:
SOLO:
src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs
y únicamente el XML comment semánticamente neutro ya autorizado.
Evalúa sin cortocircuito.
A5 se evalúa contra el NUEVO gate-contract ASCII.
No compares contra el contrato Unicode anterior.
Debe haber:
A1 PASS
A2 PASS
A3 PASS
A4 PASS
A5 PASS
A6 PASS
A7 PASS
A8 PASS
Además comprueba:

* delegation.json contiene solo ASCII en los campos copiados del contrato;
* ninguna cadena contiene caracteres de control inesperados;
* no existen NUL;
* no existen TAB dentro de valores salvo donde el esquema lo permita;
* no existe corrupción tipo "Documentaci<TAB>n".

Si TODO pasa:
detente en:
COORDINATOR_ACCEPTANCE_REQUIRED
No Worker todavía.
Si planning 7/7 falla A1-A8:
STOP FINAL DE PLANIFICACION.
No:

* planning 8;
* cambio de modelo;
* reparación manual de delegation.json;
* Worker.

Devuelve la evidencia exacta.
Tras aceptación del Coordinator:
Worker final
→ cambio XML comment únicamente
→ commit/push
→ CI 4/4
→ Controller verification final
Si VERIFIED:
SIN COMMIT DE SESION:
nc1
→ nc2
→ nc3
Después:
custodia
→ cierre documental
→ CI
→ F1-T1-MODEL COMPLETE.
Si Worker/verificación requiere nuevo trabajo:
STOP S-11 definitivo.
No Owner decision.
No uses al Owner como relay entre Controller y Worker.
La única frontera antes del Worker es:
COORDINATOR_ACCEPTANCE_REQUIRED.
Empieza por reemitir el gate-contract ASCII y validar su equivalencia semántica.
```

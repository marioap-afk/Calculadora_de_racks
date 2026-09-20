# I-49 — Consensus Freeze correctivo V6 + A1 + A2 + A3-R2 + ADR-0043 R1

Registro normativo correctivo del consenso vigente de I-49 (ID22B, motor de expresiones paramétricas). `R1`
identifica la primera revisión del freeze y no una revisión de Amendment A3. Este documento corrige únicamente la
metadata de identidad del Owner que heredó el freeze anterior; no reabre ni modifica el consenso técnico.

## 1. Autoridades exactas

```text
Initiative = I-49 / ID22B

Proposal = V6
V6 blob =
ef4db3aa400483ff25a8f39b2beb93708fa43d1a

Amendment A1 blob =
d62019088b9e7a140d5066799afe6ace6db303ba

Amendment A2 blob =
49a925336dd3775929a35b0f40b73cb7f8c487f7

Amendment = A3-R2
A3-R2 blob =
4da6ef3caa21dcf31140983c6db7e23f02aa3e18

ADR = ADR-0043
ADR status = ACCEPTED
ADR original proposal blob =
164ac166080d0b2be660473ec721110301a46dbf
ADR corrected accepted blob =
dd88bf06fe5b59a5b7683cb513a4a42460d71438

Original acceptance commit =
f6b123414621d8e4b1b8aee36eef0ce22f19eb48
Acceptance-metadata correction commit =
91f731ec0fd9cff505570cff91327819664e3642

Previous ADR = ADR-0041 — REPLACED BY ADR-0043

Owner role     = Owner del proyecto
Owner decision = ACCEPTED
Acceptance date = 2026-09-15
Owner response = "Acepto ADR-0043 para I-49 sobre Proposal V6 + Amendments A1 + A2 + A3-R2."

Coordinator = AGREED WITH ADR-0043
Architect   = AGREED WITH ADR-0043

Consensus = FROZEN — V6 + A1 + A2 + A3-R2 + corrected accepted ADR-0043
```

La fecha se apoya en `AuthorDate` y `CommitDate` del commit durable de aceptación
`f6b123414621d8e4b1b8aee36eef0ce22f19eb48`, ambos `2026-09-15T10:19:58-06:00`. La respuesta literal establece el
acto de aceptación y el rol `Owner`; no establece un nombre personal ni el rol de propietario del repositorio.

Fuentes:

- [Proposal V6](I-49-proposal-v6.md), blob `ef4db3aa400483ff25a8f39b2beb93708fa43d1a`.
- [Amendment A1](I-49-proposal-v6-amendment-a1-text-guard.md), blob
  `d62019088b9e7a140d5066799afe6ace6db303ba`.
- [Amendment A2](I-49-proposal-v6-amendment-a2-exact-key-qualifier.md), blob
  `49a925336dd3775929a35b0f40b73cb7f8c487f7`.
- [Amendment A3-R2](I-49-proposal-v6-amendment-a3-multi-cause-dependency-failures.md), blob
  `4da6ef3caa21dcf31140983c6db7e23f02aa3e18`.
- [ADR-0043 corregido](../adr/0043-motor-expresiones-parametricas-causas-multiples-y-recuperacion-segura.md),
  blob aceptado corregido `dd88bf06fe5b59a5b7683cb513a4a42460d71438`.
- [Registro de decisiones de I-49](../automation/decisions/I-49.md) §18 y corrección explícita §19.
- [ADR-0041 reemplazado](../adr/0041-motor-expresiones-parametricas-identidad-textual-y-cualificador-clave-exacta.md).

## 2. Alcance de la corrección

La cadena de aceptación queda compuesta por el commit original de aceptación y el commit posterior que corrige su
metadata. La corrección conserva exactamente:

- la decisión `ACCEPTED`;
- la respuesta literal del Owner;
- Proposal V6 y Amendments A1, A2 y A3-R2 en sus blobs exactos;
- el consenso técnico exacto del Coordinador y del Arquitecto;
- el reemplazo completo de ADR-0041 por ADR-0043;
- D1–D25 y el resto del cuerpo técnico de ADR-0043, byte a byte;
- la arquitectura semántica, la gramática, la identidad de A2, la semántica de A3 y los contratos de G7–G10.

No existe nueva decisión técnica, nueva aceptación del Owner ni nuevo ciclo de revisión del Arquitecto. La única
corrección es sustituir una atribución personal no sustentada por el rol establecido `Owner del proyecto`.

## 3. Precedencia preservada

- V6 continúa como contrato técnico base.
- A1 manda sobre V6 únicamente dentro de su alcance de guarda de recurso del parser.
- A2 manda sobre V6 únicamente dentro de su alcance de identidad textual de `VariableId`, clave exacta y `Q(key)`.
- A3-R2 manda sobre V6 y lo refina únicamente dentro de su alcance de causas múltiples, `RootCauses`,
  `RecoveryUnit`, recuperación segura, `PlanReadSet` y `Upstream`.
- ADR-0043 es la autoridad arquitectónica aceptada que incorpora V6 y los tres Amendments en D1–D25 completos.
- ADR-0041, ADR-0040 y ADR-0038 permanecen como historia reemplazada.

Ante una duda de detalle se consulta el blob autoritativo correspondiente. Este freeze no resume de forma
sustitutiva ni crea semántica adicional.

## 4. Freeze anterior histórico e inmutable

El archivo [I-49-consensus-freeze-v6-a1-a2-a3.md](I-49-consensus-freeze-v6-a1-a2-a3.md), blob exacto
`cba59b7fad9d7af66e9571d95e9ba799ae41b4d9`, permanece **histórico e inmutable**. Su atribución personal forma parte
del registro que existió antes de P4-R1; no es metadata vigente y no se corrige reescribiendo ese archivo.

También permanecen históricos e inmutables todos los freezes anteriores de I-49. Este R1 es la autoridad de freeze
vigente para la identidad neutral del Owner y se lee siempre junto con los blobs técnicos exactos de §1.

## 5. Estado de gates al versionar R1

```text
G6    = CLOSED
G7    = CLOSED
G8    = CLOSED
G9    = CLOSED
G10-A = ACCEPTED RED
G10-B = NOT STARTED — COORDINATOR AUTHORIZATION REQUIRED
```

El RED de G10-A fue versionado en `b6f2283767b52d5ef96bf0107aa444038ee8e5e2`. Es un contrato intencional aún no
satisfecho por producto. Este freeze no declara G10 verde, no altera sus tests y no implementa G10-B.

La excepción P4-R1R permite versionar esta corrección documental mientras la CI conserve exclusivamente la misma
población de fallos intencionales de G10-A, sin nuevos fallos, errores de compilación ni fallos de gates anteriores.
La excepción se limita a P4-R1R y no reduce la obligación de CI verde del futuro candidato productivo G10-B.

## 6. Regla de invalidación

Cualquier cambio material a V6, A1, A2, A3-R2, ADR-0043, D1–D25, P2.5, la identidad textual de `VariableId`,
`Q(key)`, la guarda del parser, las causas múltiples, `RootCauses`, `RecoveryUnit`, la recuperación de ciclos,
`PlanReadSet`, `Upstream`, `RepairDecisionObservation` o Schema V-0 invalida este freeze y exige el proceso técnico
correspondiente.

Una corrección documental de identidad no autoriza cambios silenciosos de ese contrato. G10-B solo puede comenzar
mediante autorización posterior del Coordinador.

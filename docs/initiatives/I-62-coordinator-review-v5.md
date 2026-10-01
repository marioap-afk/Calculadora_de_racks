# I-62 — Revisión del Coordinator de la Proposal V5 (registro)

```text
Emisor:           Coordinator exclusivo de I-62
Modo:             SEPARATE SESSION respecto de la sesión autora; revisor ≠ autor
Naturaleza:       revisión del Coordinator; no es dictamen del Architect, firma del Owner, decisión del Master, Freeze ni autorización de implementación
Fecha:            2026-10-01
Fuente original:  texto pegado por el Owner en la conversación de la sesión responsable («I-62 — COORDINATOR REVIEW OF PROPOSAL V5 / ORDER FOR V6»);
                  no existe archivo de origen en D:\IDs ni hash de archivo. Procedencia: la transcripción literal recibida (4 972 bytes en UTF-8,
                  SHA-256 3d15bc258e3781eb59e2d14a3cd61258923c41c68d7117fcaeed5de06f6e17b6), custodiada fuera del repositorio con la
                  transcripción de la sesión; este archivo es su registro recuperable
Objeto revisado:  commit acd88eafa4847e4740fdf9e7899a6988508ec24a (padre 2960b286…)
                  docs/initiatives/I-62-proposal-v5.md, blob b673b134c3f265b891e10807b6f6fd6cbe852773
                  docs/initiatives/I-62-architect-package-v5.md, blob 6832f2cd5a71867daf6238b43ec0cccdab4867d5
                  docs/initiatives/I-62-coordinator-review-v4.md, blob fb21c49e80377de6102e26a4ccc99785676c6526
Veredicto:        CHANGES REQUIRED sobre V5 (R62-V5-01). OD-6 sigue pendiente del Owner
Architect:        NOT REVIEWED · Frozen: NO
```

Registro redactado por la sesión responsable por orden del emisor (C62-F0-26). El contenido es del Coordinator. No reescribe los registros anteriores
([V1](I-62-coordinator-review-v1.md) … [V4](I-62-coordinator-review-v4.md)). La respuesta de la sesión está en la [Proposal V6](I-62-proposal-v6.md) y en
el [paquete V6](I-62-architect-package-v6.md) §4.

**Nota de recepción:** antes de esta revisión, el Owner volvió a pegar el texto de la revisión de V4. La sesión comprobó que la entrega de V5 seguía siendo la
punta remota y no hizo nada nuevo.

## 1. Publicación verificada por el Coordinator

- punta de la rama = commit revisado; padre = `2960b286…`; `origin/main` = `819955d6…`;
- CI 36933410100, attempt 1, evento `push`, `head_sha` exacto: UI Tests, Tests (Domain + Application), Build UI y Build Plugin without AutoCAD en `success`.

## 2. Disposición de V5 (C62-F0-25)

**R62-V4-02: CERRADO.** El Coordinator acepta el **Modelo A** como orden de diseño:
reclamo → observación + binding del Principal pendiente → BOOTSTRAP con evidencia real y aceptaciones PENDING → decisión de G0 del Coordinator → QU que
materializa la aceptación o el rechazo explícitos → solo entonces Q0 y delegación.

Queda aceptado como parte de ese cierre:
- `protocol.basis` inmutable desde el BOOTSTRAP;
- `g0_acceptance` con una sola transición PENDING → ACCEPTED/REJECTED;
- `principal.acceptance` con una transición por titular;
- los marcadores explícitos `I62-CLASSIFICATION` e `I62-PRINCIPAL-BINDING`;
- ningún `StateRef` futuro ni autorreferente;
- la conducta de fallo cerrado de PENDING_G0;
- QU como actualización durable quiescente válida.

**R62-V4-01: sustancialmente cerrado**, incluidos:
- ningún cambio de esquema `/v1`;
- ningún campo nuevo obligatorio en un contrato I61;
- ninguna reemisión obligatoria solo por compatibilidad;
- las citas de documento completo quedan sin cambios;
- `MainSha` y `AuthorityRevision` conservan su significado `/v1`;
- la lectura compuesta y el mapa de compatibilidad;
- la validación del mapa con fallo cerrado.

## 3. Hallazgo REQUIRED

### R62-V5-01 — El descubrimiento del resolver de compatibilidad debe funcionar para todo contrato I61 válido

**Problema (resumen fiel).** V5 afirma que un lector I61 descubre §16.13 porque su regla vigente lee AUTOMATION_PLAN §16 en `MainSha` y encuentra el puntero
nuevo. Eso no es cierto para todo contrato `/v1` válido. El contrato real de I-61 G3, que usa C-20c-1, clasifica el preámbulo de AUTOMATION_PLAN, §2 y §16 como
`UNIT_CHANGE`, así que §16 se lee en `AuthorityRevision`, no en `MainSha`. V5 demuestra cómo se resuelve ese contrato **una vez invocado el resolver**, pero no
qué regla ya vigente hace que ese contrato sin cambios invoque el resolver.

**Corrección exigida.** V6 debe aportar un punto de entrada normativo no circular para la resolución de compatibilidad:
1. partir del `gate-contract/v1` real de I-61 G3, byte a byte;
2. no añadirle campos ni `Authorities`;
3. no cambiar ningún esquema `/v1`;
4. identificar la autoridad que se lee necesariamente en su `MainSha` actual, antes de resolver las `Authorities` de ese contrato o con independencia de ello;
5. mostrar la cadena exacta: regla vigente → punto de entrada de compatibilidad → `Classify` → mapa de compatibilidad → evaluación normal de `Authority`
   `/v1`;
6. la cadena debe funcionar también para una unidad de producto I61 cuyo §16 es `EXTERNAL`;
7. no exigir que el autor del contrato conozca I-62;
8. si ningún punto de entrada global vigente tiene esa propiedad, declarar el punto de entrada nuevo como delta normativo de I-62 bajo OD-1, en lugar de
   pretender que `/v1` ya lo aporta;
9. metadatos del punto de entrada ausentes, ambiguos o inválidos fallan cerrado;
10. C-20c debe probar el descubrimiento además de la resolución: no basta con llamar a `Resolve()` directamente en el arnés de prueba.

La resolución compuesta de documento completo de V5 puede quedar sin cambios, salvo que esta corrección exija un ajuste acotado.

## 4. Autorización documental (C62-F0-26)

Se autoriza la **Proposal V6** como corrección documental **acotada**.

**Preservación:**
- las Proposals V1 a V5 y todos los paquetes;
- el Discovery R1;
- todos los registros previos del Coordinator;
- el mandato del Owner;
- todos los cierres de V5, en especial el Modelo A, `state/v2` y FX-04a/FX-04b.

V6 solo debe cambiar lo necesario para cerrar R62-V5-01 y alinear su traza, su prueba y su paquete.

**Salidas esperadas:**
- la Proposal V6 completa, `Frozen: NO`;
- el paquete V6 con el delta V5→V6;
- el registro recuperable de esta revisión;
- las actualizaciones propias de I-62 (contrato, decisiones, evidencia, estado) que hagan falta;
- identidades exactas de commit y blob;
- la CI exacta de la publicación.

**Architect:** no se invoca todavía.

**No autorizado:** implementación, esquemas, pruebas, fixtures, pilotos, modelos delegados, sondas, autenticación, cambios de sandbox o configuración,
decisiones del Owner ni ediciones de documentos normativos compartidos.

**OD-6** sigue PENDING.

## 5. Estado declarado por el emisor

- G0 = GATE PASS;
- Discovery R1 = ACCEPTED AS DESIGN BASIS;
- Archetype = NEW ARCHITECTURE;
- Proposal V5 = CHANGES REQUIRED — Coordinator;
- Proposal V6 = NARROW DOCUMENTATION CORRECTION AUTHORIZED;
- Architect = NOT REVIEWED;
- OD-6 = PENDING OWNER DECISION;
- FREEZE = NOT_AGREED;
- IMPLEMENTATION AUTHORIZATION = NO;
- I-61 REMAINS THE ACTIVE PROTOCOL UNTIL I-62 IS INTEGRATED.

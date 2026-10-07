# Plantilla de la `ReviewLoopAuthorization` de FX-06 (para el archivo de decisiones del fixture)

```text
Uso:        la rellena y la custodia el Coordinator del fixture (la supervisión) en docs/automation/decisions/<UNIDAD>.md de la rama de la unidad,
            en un commit ANTERIOR al punto que custodie el primer binding materializado (AuthorizationRef.Commit es un ancestro de ese punto y
            nunca el mismo commit: V14 B.2; 16.20 paso 3; A-1 D2-11).
Contenido:  solo marcadores de posición <…>. Las líneas sin <…> son constantes que fija el texto congelado, no valores esperados.
Formato:    16.28 «Autorización del bucle del Architect» y 16.29; criterios de materialización con las claves de 16.20 en el mismo bloque;
            topes con los nombres de architect_budgets[].caps de A-1 D1-2.
```

## 1. Bloque (copiar entre las líneas de corte, sustituir cada `<…>` y borrar los comentarios de §2)

---8<---

## ReviewLoopAuthorization <AuthorizationId> (Coordinator del fixture)

```text
I62-REVIEW-LOOP-AUTHORIZATION: <AuthorizationId>
Role: ARCHITECT
ContinuesLoopInstanceId: null
ObjectFamily: <unidad> <ruta del objeto>; versión inicial <ruta>@<blob de la versión inicial>
CorrectionScope: <ruta del objeto> completo; ningún otro archivo; no se reabre ninguna decisión ni ningún contrato ya custodiados de <unidad>
Budget.review_rounds: <entero ≤ 3>
Budget.logical_requests: <entero ≤ 3>
Budget.transport_reruns_per_request: <entero ≤ 2>
Budget.corrections_per_lineage: <entero ≤ 2>
Budget.correction_rounds: <entero ≤ 2>
Budget.architect_launches: <entero ≤ 9>
AuthorizedActions: REVIEW_DESIGN
MinimumCapabilities: ARCHITECTURE_REVIEW.level=<Required>; ARCHITECTURE_REVIEW.effort=<Required>; ARCHITECTURE_REVIEW.read=<Required>
RequiredIndependence: <ReferenceRole>:<Dimension>=<REQUIRED|PREFERRED|NOT_REQUIRED>; <…una entrada por referencia y dimensión…>
EligibleCells: <LIST: <CellId>[, <CellId>…] | CRITERION: ADR-0046 #4>
ModelEffortBounds: MinLevel=<nivel>; MinEffort=<effort>; MaxLevel=<nivel|null>; MaxEffort=<effort|null>
Permissions: READ_ONLY
<Exemption.1: …  — solo si el Coordinator la autoriza; clave sin nombre congelado (OQ-05)>
<Validity.FromRecordVersion: <record_version>  — opcional (OQ-06)>
Validity.Until: <instante ISO-8601 con Z>
Claim-Id: <Claim-Id de la unidad>
```

---8<---

## 2. Campo a campo

| Línea | Fuente congelada | Cómo se rellena | Comprobación antes de publicar |
|---|---|---|---|
| `I62-REVIEW-LOOP-AUTHORIZATION: <AuthorizationId>` | V14 §20.5; 16.28; B.2 `AuthorizationRef.Marker` | identificador nuevo, nunca usado en el archivo (A-1 D1-7: la autorización de apertura «no figura en ninguna entrada») | la línea es exacta y única en un bloque cercado; el lector de F4 (`Orchestration.DecisionBlock`) busca la línea literal del marcador dentro de un bloque ```` ``` ```` |
| `Role: ARCHITECT` | §20.5.1 («único valor admitido») | constante | — |
| `ContinuesLoopInstanceId: null` | A-1 D1-7, D1-12 | constante para abrir un bucle nuevo; solo una sustitución o continuación lleva el `loop.instance_id` del bucle abierto (D1-8), y eso exige una decisión del Coordinator que no cabe en 1-7 | — |
| `ObjectFamily` | §20.5.1 (unidad, ruta o patrón, versión inicial y alcance) | unidad y ruta del objeto; versión inicial por blob, para que el criterio `OBJECT` rechace una versión distinta (OQ-16) | el blob es el de los bytes que publicará el Principal (`.gitattributes` del fixture: `* -text`) |
| `CorrectionScope` | §20.5 («alcance de corrección permitido, incluidos los cierres que no se reabren»); §20.5.1 `ObjectFamily` | **independiente del oráculo**: el archivo del objeto entero (la ruta de `ObjectFamily`), ningún otro archivo, y los cierres de la unidad que no se reabren nombrados de forma genérica. Nunca secciones, líneas ni cláusulas concretas del objeto: elegirlas exige saber qué hay que corregir, y ese conocimiento es del oráculo (V14 D.8-1, D.6; decisiones §54 «Sin atajos»). El archivo de decisiones es entrada canónica del revisor («sus autoridades», §20.3.1) y lo lee el Principal | el texto no deriva de ningún archivo solo de supervisión; `prepublish_scan.py` sin token ni indicio sin disposición (§3). Una corrección que toca otro archivo es COORDINATOR_DECISION (§20.5) y rompe el PASS de D.8 |
| `Budget.<tope>` | §20.6 (constantes congeladas); A-1 D1-2, D1-11; 16.28 («un tope ausente no rebaja el congelado») | enteros sin signo, ≤ el congelado; ver [budget-accounting.md](budget-accounting.md) | cada valor ≤ 3, 3, 2, 2, 2 y 9 respectivamente; un `Budget.correction_rounds` ausente deja 2, aunque con `review_rounds` = 2 solo una corrección puede re-revisarse (§20.6: «una corrección sin re-revisión posible no se publica») |
| `AuthorizedActions: REVIEW_DESIGN` | §20.5.1 | constante | — |
| `MinimumCapabilities` | §20.5.1 (`{RequirementId, Required}` del perfil en `routing.md`); routing §1, §3 y §8 | los tres requisitos obligatorios del perfil ARCHITECTURE_REVIEW con su valor requerido según la clase de la tarea | los `RequirementId` cumplen `^[A-Z_]+\.[a-z0-9-]+$` (B.2) y son los de routing §8 en la `AuthorityRevision` |
| `RequiredIndependence` | §20.5.1 (frente a cada autor del objeto y frente a los Architects anteriores del bucle); §11; README §14.5; D.8 («proveedor distinto del de A: PREFERRED») | una entrada por referencia y dimensión (Actor, Session, Context, Provider) | **OQ-02**: Actor y Sesión de una candidata CLI son UNKNOWN antes del lanzamiento (NOT_STARTED); con REQUIRED, la materialización falla cerrada. Decidir antes de publicar |
| `EligibleCells` | §20.5.1 (lista cerrada o criterio cerrado de ADR-0046 #4); routing §9 (`CellId`) | lista cerrada de `CellId` de la forma `<AdapterId>:<modelo>:<EffortSemantic>` | cada celda listada tiene invocación medida con el binario vigente (re-medición; [architect-invocation-contract.md](architect-invocation-contract.md) §2) |
| `ModelEffortBounds` | §20.5.1 (`{MinLevel, MinEffort, MaxLevel, MaxEffort}`; máximos nullables) | niveles y efforts de la escala de routing §3 | coherente con `MinimumCapabilities` |
| `Permissions: READ_ONLY` | §20.5.1 («READ_ONLY, exactamente») | constante | — |
| `Exemption.<n>` | §20.5 y §20.3.1 (exenciones de opción B, cada una con la obligación exacta); 16.28 («con las claves de 16.20») | solo si el Coordinator la autoriza; obligación exacta (ruta, sección, blob), acción omitida y alcance (las invocaciones de este bucle) | **OQ-05**: 16.20 no nombra la clave. Sin exención, una obligación ACTION_INCOMPATIBLE detiene el lanzamiento y exige una decisión del Coordinator a mitad del bucle (incompatible con el PASS de D.8) |
| `Validity.FromRecordVersion` | §20.5.1 (`Validity` = `{FromRecordVersion, Until}`) | opcional | **OQ-06**: 16.28 solo lista `Validity.Until` |
| `Validity.Until` | §20.5.1; 16.28; I-P13 («un QU con una acción nueva y un instante posterior a `Until` es inválido») | instante que cubra la corrida completa con margen | posterior a la hora prevista del último LAUNCHING |
| `Claim-Id` | 16.28; I-S16 (cada decisión no nula contiene el marcador y el `Claim-Id`) | el de la unidad anfitriona | igual a `automation_state.claim_id` del estado de la unidad |

## 3. Reglas de custodia que la plantilla no puede expresar

- El archivo de decisiones no debe cambiar entre el arranque y el paso 7: toda entrada nueva sería una decisión intermedia (D.8) y obligaría a refrescar
  los `StateRef` a ese archivo (I-S13; disposición §51, F6-OBS-01).
- `loop.authorization` y `AuthorizationRef.Blob` citan el blob del archivo en el punto o en el commit que corresponda; un blob de un árbol anterior viola
  I-S13 (lección del QH `ae25b596`).
- Una autorización terminada (ARCHITECT_SATISFIED, EXHAUSTED, EXPIRED, REVOKED o SUPERSEDED) nunca abre un bucle nuevo ni materializa un binding nuevo
  (§20.5.1; A-1 D1-7, D1-9; 16.20).
- Ningún texto del bloque nombra un repositorio, una unidad o una decisión del plano real (P-16).
- **Antes del push** (supervisión): `prepublish_scan.py scan` sobre el bloque relleno, la orden ([order.template.md](order.template.md)), el registro de
  la medición y el texto del objeto, con los tokens solo de supervisión (SHA del oráculo y fragmentos distintivos de los archivos solo de supervisión).
  Código 1: no se publica. Código 4: solo tras una disposición registrada en `R:` prepublish-scan.json. Ningún campo de la RLA o de la orden contiene una
  pista de dónde está el defecto sembrado ni del veredicto o la secuencia de disposiciones esperados (V14 D.6, D.8-1; decisiones §54).
- El borrador de RLA del kit previo (README del kit, blob `65530b05`) **no** se reutiliza tal cual: su `CorrectionScope` nombra secciones concretas del
  objeto y no es independiente del oráculo; sus demás líneas tampoco siguen los formatos de 16.28 y §20.5.1 (`MinimumCapabilities`, `RequiredIndependence`,
  `ModelEffortBounds`).

# I-64 — F4-T4A2-PREPARE-UI: paquete de cierre

Lo prepara la sesión responsable el 2026-11-14. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, F4-E54 a F4-E57.

**Autoridades del cierre:**
- la autorización de F4-T4A2 del Owner (`F4-DRAFTS/coordinator/owner-f4t4a2-authorization.md`) y sus decisiones para la noche (`owner-f4t4a2-overnight-decisions.md`);
- la disposición del Owner y del Coordinator `owner-f4t4-disposition-reverify-t1b-t4b.md`:
  - **opción (a)**: una sola reverificación del Controller con una nota aclaratoria;
  - si da `EXECUTION_VERIFIED` 14/14, nc1..nc3 y cierre normal;
  - si falla por otra causa, STOP sin el intento 3;
- la respuesta del Owner sobre Identity tras la custodia, «Sí, mismas A–I» (`owner-f4t4a2-post-custody-identity.md`);
- su decisión sobre `config.toml`, «Sonda neutra, luego I» (`owner-f4-config-snapshot-i-decision.md`).

Este paquete cierra una subtarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `ae0a7b404d2799ef690b3771c6083e988637a5a3`, CI `43617317131` push 4/4 |
| Verificación | `R20261114T204712Z-88aa`: `EXECUTION_VERIFIED` / NONE, 14/14, con la nota aclaratoria y la sustitución A–I. La verificación `R20261114T165017Z-4b2a` (STOP por una lectura de F4T4A2-C04 contraria al contrato) se conserva tal cual |
| Cadena | ChainBaseSha `4eb7e924`; RED del intento 1 `1fe36cf9` (10 rutas); corrección: BaseSha `f8ccef46`, RED `f2bb4d6f` y GREEN `ae0a7b40` |
| Contrato | `gate-contract-attempt2.json` (AuthorityRevision `192103f8`), que parte de `gate-contract.json` (AuthorityRevision `509622ff`, el paquete de F4-T4A2 v3) |
| Intentos | 2 de 3. El intento 3 no se gastó |
| Config | Snapshot H hasta la verificación `4b2a`. Desde las 17:36Z, **Snapshot I** (`0B47701A…`): sonda neutra `R20261114T204534Z-6603` PASS y decisión del Owner |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E57; limitaciones en el §5) |

## 2. Corridas del intento 2

| Corrida | Resultado | Notas |
|---|---|---|
| `f15d` (planificación) | aceptada a la primera | Fidelidad PASS y A1-A8 8/8, con `CorrectionOf`; sin nc4 (se hizo en la primera delegación) |
| `b48d` (Worker) | entrega | `claude-sonnet-5-5`/high, 176 turnos y 0 denegaciones. **RED** `f2bb4d6f`: solo pruebas; Core pasa entero y UI falla por aserción solo en las 3 pruebas nuevas de M-1. **GREEN** `ae0a7b40`: solo `WorkspacePilotFieldViewModel.cs` |
| `4b2a` (verificación) | `EXECUTION_BLOCKED` / STOP | F4T4A2-C04 por una lectura contraria a la corrección de INV-F4T4A2-05. Se conserva (F4-E56) |
| `88aa` (reverificación única) | `EXECUTION_VERIFIED` / NONE, 14/14 | Mismas entradas que `4b2a`, más `clarification-note.md` (las citas literales de C04 y de INV-F4T4A2-05, y la sustitución A–I con las 46 rutas de `docs/` entre `ae0a7b40` y `c3ae1c97`) |

**CI exacta:**
- **RED** `43616238873`: UI en failure y Core en success, como pide el contrato del intento 2.
- **GREEN** `43617317131`: los cuatro jobs en success.
  - Filtros: 2923/2923 en Core y 601/601 en UI.
  - Core completo: 18461/18461.

**Antes de la reverificación**, una carpeta de corrida (`R20261114T204205Z-0846`) llegó a crearse, pero el relevo de salida detectó la config nueva y paró. Se borró **sin ninguna invocación de modelo** (no tenía `events.jsonl`) cuando el Owner aceptó la I, porque el prompt tenía que nombrar la I y no la H.

## 3. Revisiones y controles

- **Intento 1:**
  - la revisión directa da 3 menores;
  - la independiente da NO ACEPTABLE (0/1/9), con I-1 y M-1..M-9 (F4-E55).
- **Intento 2:**
  - la revisión directa da la corrección conforme (D2-1, D2-2);
  - la re-revisión independiente da **ACEPTABLE CON MENORES (0/0/1)**.

**Controles** sobre las entradas de `88aa`, con la misma sustitución A–I y conservando cada salida ordinaria:

| Control | Corrida | Resultado |
|---|---|---|
| nc1 (CurrentSha falso) | `R20261114T205333Z-aa8a` | **CONTROL PASS**: Identity es la primera en fail («no es un commit»), `EXECUTION_BLOCKED` / STOP. En la copia de nc1, el literal `ae0a7b40` de la nota y de su línea del prompt se generalizó al `CurrentSha` de la entrega (`DerivedClarificationAdjustment` de `mutation.json`), como fijó el Owner para nc1 en F4-T1 |
| nc2 (sin la primera ruta del diff en el alcance) | `R20261114T205928Z-7e23` | **CONTROL NOT PASSED**, con la firma exacta de PD-F4-01. Contract es la primera en fail, por la exactitud de la lista blanca. Scope falla sobre `WorkspacePilotFieldViewModel.cs`, con `EXECUTION_BLOCKED` / STOP. No hay más variaciones. Se conserva tal cual |
| sustituto de nc2, `F4T4A2-NC2-EXACT-WHITELIST-01` | `R20261114T211003Z-35ca` | **PASS**: 10/10 propiedades y autoprueba determinista. Es copia del de F4-T4A1, con solo el identificador y el ámbito cambiados. nc2 = SATISFIED_UNDER_F4T4A2_EXACT_WHITELIST_SUBSTITUTE |
| nc3 («GATE PASS» en `WorkCompleted`) | `R20261114T210541Z-f8f7` | **CONTROL PASS**: FreeText es la única en fail, `EXECUTION_REWORK_REQUIRED` / REWORK |

**Aviso de límite en el registro de nc3:** cuenta un «limit reached». Es el texto de la parada S-11, que el Controller leyó del contrato; no es un aviso del CLI.

**Participantes:** 0 en todas las comprobaciones de salida y de entrada de la reverificación y de los tres controles.

## 4. Lo que entrega F4-T4A2

- «Comprobar ahora» en el VM del campo, con el despacho por estado y la correspondencia de clases del §3 del paquete. «Sin cambios» solo sale de un Confirmed sin diagnóstico.
- La reconciliación: «Descartar y recargar» y «Conservar mi borrador», con la oferta de D-T4A2-02 y sin una cuarta política.
- La confirmación Sí/No en línea en el campo, con su instantánea y su revalidación.
- El diagnóstico persistente de admisión.
- Los textos de ReconcileText.
- El único cambio del host: `ClassicEditorHint` en `EditConfirmOutcome`.
- Las guardas C2-M-1 y C2-M-2 (reglas 1 a 5), con mutantes sobre los archivos reales.
- El arrastre (5) de F4-E25.
- Las 18 pruebas de la orden y la corrección de I-1 y M-1..M-9.

**Cero escrituras authored** en todos los caminos.

## 5. Limitaciones y arrastres

- **m-1** (re-revisión del intento 2): la salida temprana de M-1 se salta `_changed` si la relectura falla después de un cambio del host. Es una ruta defensiva. Pasa a la próxima tarea que toque el VM del campo (prevista en T4C).
- **D2-1 y D2-2** (directa del intento 2): aceptados.
- **La desviación de M-3**, declarada en la corrección de INV-F4T4A2-05: tras un descarte del navegador, la elección la retira el Rematerialize del navegador en silencio, sin A2-34. Cambiarlo exige tocar el navegador, que está fuera del alcance.
- **El diagnóstico de admisión en la vía `TryCheckNow`** (arrastre del §8 del paquete): el paquete de F4-T4B en preparación propone pasarlo a T4C, a decisión del Owner.
- **M-1, M-5, M-6, M-10 y D-1:** a F4-T4B, como se dispuso.
- **Los escenarios de reconciliación de Smoke-2** (S2-R-01..12, §7 del paquete) se ejecutan después de cerrar T4B y T4C.
- **Error de plantilla de la sesión:** la nota del registro de relevo de la verificación `4b2a` decía «intento 1 … primera delegación». La plantilla se corrigió para la reverificación, y el registro de `4b2a` se conserva tal cual (F4-E57).

## 6. Siguiente

Por el orden de la disposición:
1. Emitir y ejecutar F4-T1B con su paquete v5, que es la v4 con las interpretaciones del Owner.
2. Usar sus resultados para validar los supuestos de F4-T4B.
3. Terminar la revisión independiente del paquete de T4B.
4. No emitir T4B antes de cerrar T1B.

**F4-T4A (PREPARE) queda completa:** T4A1 y T4A2 están cerradas.

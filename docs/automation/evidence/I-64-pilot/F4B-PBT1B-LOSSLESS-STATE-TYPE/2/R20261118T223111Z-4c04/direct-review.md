# Revisión directa de la sesión: PB-T1b, intento 2 (corrección de pruebas)

**Objeto:** WorkRunId `R20261118T223111Z-4c04`, delegación `R20261118T222843Z-e354`. BaseSha `2f5d3dec`, RED `126fd65f` y GREEN `a18ba345`. Medido por objetos git (sin tocar el worktree durante la verificación del Controller) y por la copia de la entrega en el scratchpad.

## Régimen de la corrección (§6.0, M-11; F4B-E22)

| Comprobación | Resultado |
|---|---|
| `git diff --name-only 2f5d3dec a18ba345 -- src` | vacío |
| `git diff --name-only 126fd65f a18ba345 -- tests` | vacío |
| `git diff --name-only 2f5d3dec 126fd65f` | los 6 de `src/` del AllowedWriteScope y 5 clases de prueba (CopyTests, LoadTests, OutputGateTests, SharedReferenceGuardTests y UI ParityTests). OwnershipTests no cambia |
| blobs de `src/` en el RED, comparados con `2d9febc6` (el esqueleto del intento 1) | iguales en los 6 |
| blobs de `src/` en el GREEN, comparados con `a0a5f699` (la implementación del intento 1) | iguales en los 6 |
| trailer de los dos commits | `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>` en ambos, igual al `Trailer` de la entrega (corrige la segunda mitad de M14 del intento 1) |

## CI exacta (relevo del trabajo, válido)

- **GREEN:** `44438060317` (push, `a18ba345`), 4/4 en success. Core da 19 086 de 19 086. UI da 2 437 resultados: 2 416 pasados y 21 omitidas existentes. Son los recuentos del intento 1, porque las correcciones no añaden métodos.
- **RED:** `44430615074` (push, `126fd65f`).
  - `tests` y `ui-tests` en failure, `build-ui` en success y `build-plugin` omitido (M14, ratificado).
  - Core: 14 fallos, los 13 del filtro PushBackEditableState y el de OutputGate. UI: 1 fallo, ParityTests.
  - Los filtros de las guardas de D2 pasan.

## La entrega

- WorkerStatus IMPLEMENTATION_COMPLETE.
- Declara la interrupción de transporte y la reanudación, con las horas. Lo único que repitió fue `p2a`, como `p2a2`.
- El paso 1 repetido da las mismas líneas que el intento 1.
- La medición p2 se hizo antes del RED; las líneas nuevas se confirmaron sin ajustarlas.
- La discriminación de I01 está probada con una mutación temporal (p3): sin la regla 1b sobre los alias, la guarda falla.
- Hay quince autopruebas de CT-14. Las nuevas son k1 y k2, para I01.
- La prueba de limpieza de después es idéntica a la de antes.
- **Deviations:**
  - líneas nuevas no listadas en el paquete, todas pedidas por el análisis y fijadas antes del RED;
  - la reanudación;
  - M14;
  - órdenes largas desacopladas, sin dos corridas a la vez.
- **UnexpectedFindings:**
  - pb-15 sigue siendo la normalización conocida de F4B-E20;
  - M08 sobre 16 diseños da iguales=True;
  - las pruebas corregidas pasaron a la primera sobre la implementación.

## Valoración

**Sin hallazgos propios bloqueantes ni importantes.** El régimen RED/GREEN es exacto, el producto es idéntico al del intento 1 y la CI exacta está en verde.

**Pendientes de la revisión independiente:**
- la profundidad de cada una de las nueve correcciones;
- que el RED falle por aserción en cada método con ExpectRed.

**Para el paquete de PB-T1c (v2.8):** la autoprueba de I01 son dos (k1 y k2), no una. Se alinea D-PBT1-15 y CT-PBT1C-04 con el texto real de `a18ba345`.

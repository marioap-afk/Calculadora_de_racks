# FX-02 — Opciones para recuperar el camino a VERIFIED (decisiones §66, punto 10)

> Supervisión, plano a. **Informe de opciones; no es disposición** y no crea ninguna enmienda (§66, punto 5: «No crearla automáticamente»). FX-02
> sigue UNVERIFIED y C-24 no puede ser PASS (§66, punto 2). Base: `6d626794` y la evidencia §119-§122.

## 1. Dos bloqueos independientes

VERIFIED exige resolver **los dos**:

| Bloqueo | Hecho | Fuente |
|---|---|---|
| **A. Controller** | La sonda única A4-1 dio NOT_DEMONSTRATED sobre `3553cd6e…/26.1002.7124.0/73890CA3…`. Por A4-1, regla 4, la celda `codex-cli` `gpt-6-luna/high` no es elegible para la planificación ni para la verificación, y la invocación no se repite | [resultado](../../OD-2/R20261010T000542Z-a4-1-probe/README.md); §66, punto 1 |
| **B. Mapa del fixture** | `Validate(M)` da MAP_INVALID en el fixture actual. Fallan MV-3 (31 archivos sin cambio entre EFF^1 y EFF), MV-4 (7 blobs), MV-6 (69) y MV-7 (2). **Causa:** el constructor del fixture llenó el seed (EFF^1 `930288c5`) con el contenido de MC_I62 `6f0187cb` en lugar de la base I-61 `bb0d5522` de la que salen los `BaseBlob` del mapa. Comprobado: los 6 `BaseBlob` coinciden con `bb0d5522` y los 31 `EffBlob` con `6f0187cb`. El mapa es correcto (C-20b EQUAL) | [caracterización](../../MAP-INVALID/characterization.md) |

Además, para el PASS (no para VERIFIED) faltan las disposiciones de N9 y N10 ([borrador](n9-n10-disposition-draft.md)). La revisión del Architect
es una obligación posterior al PASS (D-0); su ruta por `claude-cli` sigue sin elegibilidad demostrada.

## 2. Bloqueo A: opciones para el Controller

Comprobado el 2026-10-10T01:16Z, sin modelo: el runtime de Codex no ha cambiado (mismo trío).

| Opción | Autoridad necesaria | Medición nueva | Consecuencias para el fixture | Posibilidad real de VERIFIED |
|---|---|---|---|---|
| **A1. Actualización real del runtime** (vía V3) | Ya prevista. A-2, A2-P2: un bloque por actualización observada (cambio de `BinaryHash` o `AppVersion`), con disposición del Coordinator que nombre el par y consumo del Owner por bloque. OD-2 nueva sobre la huella resultante (OD-2-MAT = A). A4-1, reglas 3 y 4: la actualización abre otra medición. **Sin A-n** | Sí: el bloque A2-P2 (≤ 2 sondas) y, si la ruta `cmd.exe` sigue siendo necesaria, otra medición de A4-1 con el kit v3 re-tecleado. Hay que corregir antes el falso positivo `git_write` de la herramienta. Si la actualización restablece el `pwsh` de la receta (F6-OBS-03), basta medir la receta normal en el bloque | ninguna (las autoridades del fixture no cambian) | **Incierta.** No controlamos ni el momento ni el contenido de la actualización. De los fallos observados, dos dependen de la shell y del sandbox (no ASCII con `type`, `ls-remote` bloqueado). Otros dos son del modelo: paró a los 100 s con 11 comandos, sin intentar la cadena de AP 16.13. Una CLI nueva solo resuelve los primeros |
| **A2. Alternativa ya autorizada y elegible** | — | — | — | **No existe.** `CLAUDE-CLI-I62` cubre solo ARCHITECT y REVIEWER, y `A4-CLAUDE-FX02-CONSUMO` excluye EXECUTION_CONTROLLER. A4-2 solo sustituye al Architect. No hay otra celda de `codex-cli` medida para el Controller, y el tope de A4-1 es uno en total |
| **A3. Enmienda futura expresamente justificada** (no se crea) | Coordinator y Architect independiente (LIFECYCLE §6). Consumo del Owner para cualquier lanzamiento nuevo. Material: toca topes congelados de D.3 o de A4-1 | Sí, según la variante | ninguna | Depende de la variante: |
| ↳ A3-i: una medición más sobre la misma identidad, con la sonda rediseñada | ídem | otra sonda de A4-1. Ejemplos: por etapas; `chcp 65001` en la shell declarada; una instrucción de persistencia explícita; quizá effort `xhigh`, que es otra celda | ninguna | **moderada o baja.** La cadena de AP 16.13 sin intérpretes, bajo `cmd.exe`, es larga; la celda no la intentó |
| ↳ A3-ii: sustituir el adapter del Controller | ídem, más una autorización de consumo nueva | medición completa del adapter sustituto (operaciones 2-9 y perfil del Controller) | ninguna | **baja.** `claude-cli` de solo lectura (Read, Grep, Glob) no ejecuta Git; con Bash dejaría de ser de solo lectura |
| ↳ A3-iii: admitir intérpretes en la shell declarada | ídem | otra sonda | ninguna | **baja.** El Python del host es un alias de WindowsApps, como el `pwsh` que el sandbox ya rechaza |
| **A4. Aceptar UNVERIFIED** | Registro del Coordinator | — | — | **ninguna.** La matriz de cierre exige FX-02 PASS (V14 L2544, sin vía de limitación), así que F6 no cierra y F7, READY, el Candidato y la OV quedan en espera |

## 3. Bloqueo B: opciones para el fixture

Hay un candidato construido y probado en clones temporales ([plan](../../MAP-INVALID/reconstruction-plan.md);
[efectos](../../MAP-INVALID/effects.md)): la opción B1 da **MAP_VALID** (MV-1..MV-7; EFF único; F4 `clause_map.py` EQUAL; 7 negativos fallan como
deben). Es reproducible con fechas fijas, y su paquete git es `59b24493…`.

| Opción | Autoridad necesaria | Medición nueva | Consecuencias para el fixture | Posibilidad real |
|---|---|---|---|---|
| **B1. Repositorio de fixture nuevo** (origen local y repositorio público de GitHub nuevos; seed con la base I-61 `bb0d5522`; EFF' con las autoridades I-62 por el segundo padre; trailer único) — recomendada | Coordinator: opción, invariantes y texto del manifiesto. Owner: crear el repositorio público y su CI (acción hacia fuera; precedente OD-7). Presupuesto de una sesión de Principal para la unidad nueva: A4-6 y `A4-PRINCIPAL-A2-CONSUMO` cubren solo al titular A2 designado por T16, y U-14 ya cuenta A + R. Hace falta otra línea del Owner y, si sube un tope congelado, una A-n | Solo mecánica (MV-1..MV-7, ya ejecutada sobre el candidato). Los blobs de `routing.md`, del catálogo, del descriptor de `codex-cli`, del mapa y del esquema siguen idénticos | **No invalida ningún resultado acreditado.** FX-01/C-23, FX-05/C-27 y FX-04a/C-25a (con su instantánea del QH2 intacta) siguen valiendo, igual que las sondas OD-2, OD-4, A2-P2 y A4-1. La unidad FX-U1 no se traslada: su `effective_sha` = `fbe25347` es inmutable (AP L1582). Hay que arrancar una unidad nueva (propuesta FX-U2) y rehacer: U-07 y U-08, la orden, el clon, la observación AUTHOR, U-17 (b) (nombra `d30fb6a9`/`628d89af`), el staging de FX-02 y FX-06 y el resellado de `supervision-checks.md`. Pide disposición: C-22 (su «contrato I62 válido» se emitió con el mapa inválido) y el oráculo de C-25a (no modela `Evaluate`) | **Alta** para el bloqueo B; no resuelve el A |
| B2. Refs nuevas en el mismo repositorio (`main-r2`) | Coordinator | ídem | reinterpreta `origin/main` (16.14, A6, `Remote`) y deja dos commits de trailer en un repositorio | no recomendada |
| B3. Mover `main` del fixture al linaje nuevo | Coordinator y Owner (force-push en un repositorio público) | ídem | **reescribe historial acreditado** (los hechos que nombran `main` = `fbe25347` dejan de poder comprobarse en vivo) | **excluida** por §66, punto 4 |

## 4. Combinaciones que pueden llegar a VERIFIED

1. **B1 + A1:** fixture nuevo y una actualización real de Codex que permita demostrar las operaciones (bloque A2-P2 y, si hace falta, otra medición de
   A4-1). Sin A-n, salvo el presupuesto de la sesión de Principal de la unidad nueva. Depende de un hecho externo (la actualización) y de que la celda
   nueva demuestre la cadena de AP 16.13.
2. **B1 + A3-i:** fixture nuevo y una A-n que autorice otra medición rediseñada sobre la identidad actual. Es la única vía que no espera un hecho
   externo, pero es material y su posibilidad es moderada o baja.
3. Sin B1, ninguna opción del bloqueo A basta: toda verificación de una unidad I62 en el fixture actual acaba en `Authority` fail.

**Recomendación de la preparación (etiquetada; no es decisión):**
- Autorizar B1 hasta la publicación del linaje de activación (repositorio nuevo, sin unidad), porque no invalida nada acreditado y quita un bloqueo
  seguro.
- Diferir el arranque de la unidad nueva hasta que el Controller tenga una vía elegible (A1 o A3-i).
- Vigilar pasivamente la versión de Codex (sin modelo) para detectar A1.
- No crear ninguna A-n sin una disposición expresa.

## 5. Preguntas para el Coordinator

1. ¿B1, con sus invariantes y el texto del manifiesto? Nombre de la unidad, Claim-Id y fechas. ¿U-07 y U-08 entran en el seed?
2. ¿Pedir al Owner el repositorio público nuevo y su CI?
3. Presupuesto de la sesión de Principal de la unidad nueva: ¿línea del Owner sola, o una A-n?
4. Disposición sobre C-22 y el oráculo de C-25a.
5. Para el bloqueo A: ¿esperar A1 (vigilancia pasiva), o preparar una A3-i justificada?
6. N9 y N10: aprobar el borrador.
7. Preguntas del análisis de F7: Q22', Q23, Q24 (el defecto está en el método de D.1, así que repetir el seed lo reproduciría), Q25, Q26 y Q11.

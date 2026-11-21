**Estado:** v4.7. Corrige la parte de **PB-T4b** con lo que encontró la precondición 6 de la v4.6. No cambia la lista de 477, ninguna aserción, ninguna fila ni los totales, y no hay decisión nueva del Owner: es **D-S5**, una decisión de la sesión (`session-pbt4b-design-decisions.md`, sha256 `@@SESSION@@`), con el mismo mecanismo que D-S1:
- **la precondición 6 de la v4.6 paró en el paso 4, la partición** (`/d/r4i`). Compiló sin errores y sin ningún `warning CS` en Core, UI y Plugin. De las 476 pruebas de copia, 249 pasan enteras en el RED, 223 fallan enteras, todas por aserción, y **cuatro `Theory` mezclan resultados** (30 filas: 18 fallan y 12 pasan). Ninguna prueba fuera de la lista de 477 falló;
- **D-S5: las cuatro se parten por filas** (V47-01) en dos equivalentes escritos a mano: uno en `…FullRedTests` con las filas que fallan en el RED y otro en `…FullGreenTests` con las que pasan, con el mismo nombre, el mismo cuerpo y las mismas aserciones del original. En las dos de `MemberData`, dos miembros de datos nuevos de la copia, `StatusNamesRed` y `StatusNamesGreen`, reparten las filas de `StatusNames` sin cambiar ninguna;
- **la regla de la partición se amplía solo para eso** (V47-02; §5.2, §6.2.1 paso 4, `partition_pbt4b_v47.py`): una `Theory` que mezcla se parte por filas en dos equivalentes a mano. Las demás causas siguen parando;
- **CT-PBT4B-20 admite el reparto por filas y nombra las cuatro** (V47-03);
- **el reparto de C-0 pasa de 360/68 a 358/70**, con el mapa de la copia v4 (`pbt4b-copy-map-v4.tsv`, sha256 `@@MAP4@@`).

**Lo demás no cambia:** el GREEN (`green_pbt4b_v45.py`), el esqueleto, el kit de la v4.5, la lista de 477, las CT salvo CT-20, los totales, MU, las ediciones de ayudante de la v4.6, D-S4 y PB-T4a. **La precondición 6 se corre otra vez entera** con los guiones de la v4.7, en copias nuevas (`/d/r4j` en adelante). Sus resultados, su `pbt4b-precondition6.json` y su `compile-check.md` van en `pbt4bcc/compile/`, no en el paquete.

**Lo que sale de la v4.7 (PB-T4b):**
- el mapa de la copia v4 (`pbt4b-copy-map-v4.tsv`, `make_copy_map_v4.py`, `v47/make_copy_map_v4-out.txt`);
- las ediciones (`pbt4b_hand_edits_v47.py`: `SPLIT`, `RED_KEYS`, `SPLIT_COMMENT` y `C0_NEW_HELPERS`);
- el guion de la copia (`make_copies_pbt4b_v47.py`) y su revisión (`pbt4b-equivalents-review-v47.txt`, 102 entradas);
- los `Skip` (`pbt4b_skips_v47.py`), el RED (`apply_red_pbt4b_v47.py`) y su comprobación (`check_skips_pbt4b_v47.py`);
- la partición (`partition_pbt4b_v47.py`);
- la autoprueba (`selftest_pbt4b_v47.py`, `v47/selftest_pbt4b_v47-out.txt`), con la partición de prueba `v47/partition-check-r4i.tsv`, que es la de `partition_pbt4b_v47.py red` sobre las TRX de `/d/r4i` (un registro de trabajo, no la partición de la emisión);
- y `make_v47.py`, que arma esta versión desde la v4.6.

---

## Registro de cambios v4.6 → v4.7

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V47-01** | **D-S5: cuatro `Theory` mezclan resultados en el RED** (la precondición 6 de la v4.6, `/d/r4i`, paso 4) | **Se parten por filas, a mano, con su texto literal** (la tabla de abajo y el anexo V47-A). Dos eran copias mecánicas (WLHT y la `Confirm…` de WPP) y pasan a «a mano» con el motivo `R-D-S5`. La `Keep…` de WPP ya era a mano por D-S1 y lleva `R-D-S1,R-D-S5`. La de PEG es un equivalente de guarda (C-5) y sigue siéndolo. El guion pone la `Theory` entera en la clase Probe del modo de medida y, con la partición, sus dos mitades. **El mapa v4** (`@@MAP4S@@`): 358 mecánicas y 70 a mano. Los dos `Skip` de C-0 que eran «copia» pasan a «equivalente escrito a mano». La lista no cambia | D-PBT4-11; §5.2; §6.0 (RR-3); §6.2; §8.3; §8.5 |
| **V47-02** | La regla de la partición de la v4.5 decía que una prueba que mezcla para la emisión, y que no se parte una `Theory` | **Una `Theory` que mezcla se parte por filas en dos equivalentes a mano.** La partición lo admite solo para las declaradas en el paquete (`SPLIT`), cuando sus filas que fallan son exactamente las declaradas (`RED_KEYS`, por el primer argumento que escribe xUnit en el nombre del resultado). Entonces da dos filas, Red y Green. **Siguen parando:** otra prueba que mezcla, una declarada que no mezcla o mezcla otras filas, una que no corre o se salta, y una falta sin aserción. Si la precondición 6 encuentra otra `Theory` mezclada, se le aplica D-S5 con una versión del paquete; si encuentra una copia que no pasa en el GREEN, D-S1 | §5.2; §6.2.1, pasos 4 y 6 |
| **V47-03** | CT-PBT4B-20 | Admite el reparto por filas: cada una de las cuatro tiene dos pruebas de igual nombre, una en cada clase, cuyas filas, juntas y sin repetirse, son las del original. Nombra las cuatro, con la línea `particion-por-filas <Clase>.<método> red=<n> green=<m> filas-del-original=True`. Las dos de `MemberData` se comprueban por reflexión en ejecución | §5.2 (CT-20); §6.0 (RR-3) |
| V47-04 | Los totales | No cambian: cada prueba de la lista sigue sumando un resultado, y las cuatro reparten sus filas entre sus dos mitades sin cambiar ninguna. El `[13]` y el `[14]` de `ExpectedEvidence` lo dicen | §5.2; §6.2.1 `[13]` y `[14]` |
| V47-05 | La versión | El `Objective` de PB-T4b cita la v4.7; el de PB-T4a sigue citando la v4.3. **Los `StopConditions` no cambian:** D-S5 es de la partición, que corre la sesión antes de emitir, y PBT4B-C07 y C16 siguen igual. Las corridas de la precondición 6: `/d/r4g` (v4.5, paró en la compilación), `/d/r4h` (diagnóstico), `/d/r4i` (v4.6, paró en la partición) y las de la v4.7 (`/d/r4j` en adelante) | §6.2.1; §11 |

**Las cuatro `Theory` partidas por filas** (D-S5; las filas Red son las que fallan en el RED, medidas en `/d/r4i`; el texto, en el anexo V47-A y en `pbt4b-equivalents-review-v47.txt`):

| `Theory` (archivo de origen) | Filas | Red: fallan en el RED | Green: pasan | Por qué falla en el RED | Mapa |
|---|---|---|---|---|---|
| WLHT `EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively` (`InlineData`) | 10 | 3: `a-check-that-finds-a-conflict`, `a-check-that-recovers` y `a-reload` | 7: `a-veto`, `a-failure-of-the-evaluation`, `an-admitted-intention`, `a-discard-of-the-host`, `a-drain`, `a-drain-with-the-stale-hint` y `a-note-of-an-edit` | `Assert.NotNull` en `Env.Pilot`: el anfitrión del RED aún busca el borrador del piloto | mecánica → `R-D-S5` |
| PEG `TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance` (`InlineData`, equivalente de guarda) | 4 | 1: la del adaptador estático, `private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();` | 3: el orden de las bases, `GetField` y `CommitField` | `Assert.Contains`: WHC del RED aún tiene el adaptador del piloto | guarda (C-5) |
| WPP `ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing` (`MemberData(StatusNames)`) | 8 | 7: `StatusNamesRed` (todos salvo `Clean`) | 1: `StatusNamesGreen` (`Clean`) | `NullReferenceException` en `Rig.Edit`, en el código de la prueba (por aserción según INV-PBT4B-02) | mecánica → `R-D-S5` |
| WPP `KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading` (`MemberData(StatusNames)`) | 8 | 7: `StatusNamesRed` | 1: `StatusNamesGreen` | la misma | `R-D-S1` → `R-D-S1,R-D-S5` |

**El texto de cada mitad** es el de la prueba en la copia (con M y, en la de PEG, con su edición de guarda), con dos cambios. El primero es una línea de comentario antes de `[Theory]`, `SPLIT_COMMENT`, que dice a qué mitad pertenece. El segundo son sus filas:
- en las de `InlineData`, las líneas `[InlineData(…)]` de su mitad, en el orden del original;
- en las de `MemberData`, `[MemberData(nameof(StatusNamesRed))]` o `[MemberData(nameof(StatusNamesGreen))]` en lugar de `[MemberData(nameof(StatusNames))]`.

**Los dos miembros nuevos** (`C0_NEW_HELPERS`) van solo en la copia de WPP, tras `StatusNames`, y cada clase de la partición lleva el suyo, porque solo se incluyen los ayudantes que alcanzan sus pruebas. Filtran las filas de `StatusNames` por `Clean`, sin escribir ninguna.

---

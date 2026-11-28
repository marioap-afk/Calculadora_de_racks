**Estado:** v4.6. Corrige la parte de **PB-T4b** con lo que encontró la precondición 6 de la v4.5, sin cambiar la lista de 477 y sin decisión nueva del Owner (la decisión de la sesión, en desarrollo de D-S1):
- **la precondición 6 de la v4.5 paró en la compilación** (`/d/r4g`): el proyecto de pruebas de Core no compilaba con las copias del guion, por dos defectos del texto de las copias (`pbt4bcc/compile/compile-check.md`, §1); un diagnóstico (`/d/r4h`) separó los dos;
- **el guion de la copia añade la directiva `using` de `Draft/`** (MU, V46-01) a las copias que nombran un tipo de `Draft/`;
- **ocho copias «mecánicas» que no compilan sobre el modelo completo pasan a equivalentes escritos a mano** (D-S1, V46-02): seis de `WorkspacePilotEditHostTests` (cuatro por el ayudante `Env.OpenUndecidable`, editado en la copia, y dos por su cuerpo) y dos de `WorkspacePilotPrepareTests`; **el reparto de C-0 pasa de 368/60 a 360/68**, con el mapa de la copia v3;
- **la recomprobación de la v4.5** (§19 de `independent-review-v1.md`: LISTO PARA LA PRECONDICIÓN 6, 0/0/4): M-07 se corrige («cinco escaneos») y M-08 a M-10 se declaran en el paquete y en F4B-E52.

**Lo demás no cambia:** el GREEN (`green_pbt4b_v45.py`), el esqueleto, el kit de la v4.5, la lista de 477, las CT, los totales, D-S4 y PB-T4a. **La precondición 6 se corre otra vez entera** con los guiones de la v4.6, en copias nuevas (`/d/r4i` en adelante); sus resultados, su `pbt4b-precondition6.json` y su `compile-check.md` van en `pbt4bcc/compile/`, no en el paquete.

**Lo que sale de la v4.6 (PB-T4b):** el mapa de la copia v3 (`pbt4b-copy-map-v3.tsv`, sha256 `11b26ab1ecbfbf4a289c1485c14a803529549b2e066e13588668763448390743`, `make_copy_map_v3.py`); las ediciones (`pbt4b_hand_edits_v46.py`); el guion de la copia (`make_copies_pbt4b_v46.py`) y su revisión (`pbt4b-equivalents-review-v46.txt`, 90 entradas: 68 a mano, 21 de guarda y el ayudante editado); los `Skip` (`pbt4b_skips_v46.py`), el RED (`apply_red_pbt4b_v46.py`) y su comprobación (`check_skips_pbt4b_v46.py`); la autoprueba (`selftest_pbt4b_v46.py`, `v46/selftest_pbt4b_v46-out.txt`); y `make_v46.py`, que arma esta versión desde la v4.5.

---

## Registro de cambios v4.5 → v4.6

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V46-01** | **La precondición 6 de la v4.5: las copias no compilan** (`/d/r4g`, 41 errores: CS0246, CS0103 y CS0535) | **MU, una regla nueva del guion de la copia:** cada copia que nombra un tipo de `Draft/` (fuera de cadenas y comentarios) y no lleva `using RackCad.Application.Systems.PushBack.Draft;` gana esa directiva tras el último `using` de su cabecera. M-04 y M-06 cambiaban el nombre de los tipos pero no su espacio de nombres. Diez copias la llevan (nueve la ganan) | D-PBT4-11; §6.2.1 |
| **V46-02** | **D-S1: ocho copias «mecánicas» no compilan sobre el modelo completo** (el diagnóstico `/d/r4h`, con MU: 14 errores) | **A mano, con su texto literal** (`pbt4b_hand_edits_v46.py`): las seis de WPEH (el ayudante `Env.OpenUndecidable` **de la copia** y dos pruebas abren el borrador con la lectura completa, `FullRead!.Read`) y las dos de WPP (H-02, y la lectura completa de la misma base). **El guion admite ediciones de ayudante en C-0** (`C0_HELPER_EDITS`), en la copia y nunca en el original, y CT-PBT4B-20 las nombra; las fija el `CopySha`. **El mapa v3** (`11b26ab1…0743`): 360 mecánicas y 68 a mano, las ocho con el motivo `R-D-S1-compila`. Los ocho `Skip` pasan a «equivalente escrito a mano». La lista no cambia | D-PBT4-11; §5.2; §6.0 (RR-3); §6.2; §8.3 |
| V46-03 | M-07 de la §19 | «seis escaneos» → «cinco» (V45-08 y D-PBT4-11) | Registro v4.5; D-PBT4-11 |
| V46-04 | M-08 de la §19 | Declarado: D-S4 lee el piloto del `Primary` y difiere solo el modelo completo; el coste residual del piloto lo mide la sonda (`solo el piloto`) | D-PBT4-21; F4B-E52 |
| V46-05 | M-09 de la §19 | Declarado: el recuento de materializaciones de la sonda es el de su propia réplica; lo de RIA lo fija CT-PBT4B-21 | §6.2.1, paso 7; F4B-E52 |
| V46-06 | M-10 de la §19 | Declarado: la UI, solo con sus filtros; una prueba de UI de fuera se vería en la CI (C16) | §6.2.1, pasos 5 y 6 |
| V46-07 | La versión | El `Objective` de PB-T4b cita la v4.6; el de PB-T4a sigue citando la v4.3. Las corridas de la precondición 6: `/d/r4g` (paró en la compilación), `/d/r4h` (diagnóstico) y las de la v4.6 (`/d/r4i` en adelante) | §6.2.1; §11 |


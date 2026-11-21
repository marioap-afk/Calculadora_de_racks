**Estado:** v4.8. Corrige la parte de **PB-T4b** con lo que encontró la precondición 6 de la v4.7. La lista pasa a 478 por la **sexta tanda del Owner** (`owner-pbt4b-list-478-decision.md`, sha256 `@@OWNER478@@`); lo demás lo decide la sesión (`session-pbt4b-design-decisions.md`, sha256 `@@SESSION@@`).

**Qué encontró la precondición 6 de la v4.7** (`/d/r4m`, la primera vez con la suite completa de Core):
- compiló sin errores ni avisos CS;
- el RED fue exacto salvo una prueba;
- **cayeron dos pruebas fuera de la lista y dos copias** (el análisis medido, en `v47/analysis-outside-477.md`).

**Lo que cambia por eso:**
- **D-S6, el espacio de nombres de las copias** (V48-01): las 21 copias van en el espacio de nombres raíz de su ensamblado (`RackCad.Tests` o `RackCad.UI.Tests`), como exige `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`, y sus clases llevan el prefijo `FullPanelCopy`: `FullPanelCopy<Clase>ProbeTests`, `…RedTests` y `…GreenTests`. Ningún filtro heredado las selecciona. Es el mecanismo que el paquete ya usa para las CT;
- **D-S1 en dos copias de WPP** (V48-02): `InvalidE_AMutableMember…` y `ThePilotNeverErasesAViewAPhantom…` fijaban que el piloto nunca borra una vista. Sus equivalentes afirman lo que ya fijan D-PB-04, D-PBT3-09 (e) y D-PBT4-07: el lateral fantasma, `Ready` con borrados [B2]; `side-b`, `Invalid` con `FrontalBOfOneWayPrefix`/`Suffix`; los dos descriptores, como hoy. **El reparto de C-0 pasa de 358/70 a 356/72**, con el mapa de la copia v5;
- **la sexta tanda del Owner, la lista de 478** (V48-03): `WorkspaceHostHardeningGuardTests.NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept` entra en C-5, con su `Skip` y el equivalente `WorkspaceHostHardeningGuardFull`. Su tabla es la misma con tres cuentas nuevas: 2 → 0, 5 → 6 y 1 → 0, cada una con su `// before N`;
- **el paseo de guardas corregido** (V48-04): no reconocía los campos de tipo tupla. Corregido, marca tres pruebas más, que se declaran en §4.2;
- **las cifras que dependen de la lista** (V48-05), recalculadas.

**Lo demás no cambia:** el GREEN (`green_pbt4b_v45.py`), el esqueleto, el kit de la v4.5, las CT salvo CT-20, MU, las ediciones de ayudante, D-S4, D-S5 y PB-T4a. **La precondición 6 se corre otra vez entera** con los guiones de la v4.8, en copias nuevas (`/d/r4q` en adelante); sus resultados van en `pbt4bcc/compile/`.

**Lo que sale de la v4.8 (PB-T4b):**
- la lista de 478: `pbt4b-substitution-list-478.tsv`, sha256 `@@LIST478@@`, generada por `make_list_478.py` (`v48/make_list_478-out.txt`);
- el mapa de la copia v5: `pbt4b-copy-map-v5.tsv`, sha256 `@@MAP5@@`, generado por `make_copy_map_v5.py`;
- las ediciones: `pbt4b_hand_edits_v48.py`;
- el guion de la copia y su revisión: `make_copies_pbt4b_v48.py` y `pbt4b-equivalents-review-v48.txt`, con 114 entradas;
- los `Skip`, el RED y su comprobación: `pbt4b_skips_v48.py`, `apply_red_pbt4b_v48.py` y `check_skips_pbt4b_v48.py`;
- la partición: `partition_pbt4b_v48.py`;
- el paseo y su comparación: `walk_guards_pbt4b_v48.py` y `compare_walks_v48.py`, con sus salidas en `v48/`;
- la autoprueba: `selftest_pbt4b_v48.py`, `v48/selftest_pbt4b_v48-out.txt`;
- y `make_v48.py`, que arma esta versión desde la v4.7.

---

## Registro de cambios v4.7 → v4.8

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V48-01** | **D-S6: `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace` cae en el RED y en el GREEN** (`/d/r4m`). Las 20 copias declaraban `…FullPanelCopies`, y la guarda (la excepción de I-23) exige el espacio raíz del ensamblado | **Las copias, en el espacio raíz, con el prefijo `FullPanelCopy` en la clase.** Medido en `/d/r4o`: con el espacio raíz compila sin errores ni avisos, el RED es exacto y la guarda pasa. **Con los nombres de hoy**, el filtro heredado n.º 12 seleccionaría las copias Red (289 `Failed` en el RED); **con el prefijo, ningún filtro heredado las selecciona** (`v47/analysis-outside-477.md`, punto 1). Cambian los nombres completos de las copias, sus filtros (n.º 47 a 88), los del modo de medida (`…Tests.FullPanelCopy`), M-00, CT-20 y el `CopySha`. La lista no cambia por esto, y ningún filtro heredado ni prueba existente se toca | D-PBT4-11; §5.2; §6.2.1 |
| **V48-02** | **D-S1: dos copias mecánicas de WPP no pasan en el GREEN** (fallan igual en el RED): `ThePilotNeverErasesAView…` espera `Invalid` y el modelo completo responde `Ready`, porque borra la lateral obsoleta | **A mano, con su texto literal** (el anexo V48-A), según **D-PB-04**, **D-PBT3-09 (e)** y **D-PBT4-07**. Medido en `/d/r4p`: con el modelo completo, el fantasma da `Ready` con miembros A1 y borrados B2, y `side-b` da `Invalid` con `FrontalBOfOneWayPrefix`. **El mapa v5**: 356 mecánicas y 72 a mano, las dos con `R-D-S1-borra`. Con su texto, pasarían también en el RED (`PrepareFull` es código de la base), así que irían a la clase Green | D-PBT4-11; §6.0 (RR-3); §8.3 |
| **V48-03** | **La sexta tanda del Owner:** `WorkspaceHostHardeningGuardTests.NoneOfThe30…` cae en el GREEN y no estaba en la lista. Cuenta tres textos que el GREEN cambia por D-PBT4-04 y D-PBT4-08 | **«Añadirla a la lista (Recomendado)»** (`owner-pbt4b-list-478-decision.md`). **La lista pasa a 478**: C-0 428, C-5 **23** y UI 27; Core 451; **21 archivos**. El equivalente `WorkspaceHostHardeningGuardFull` lleva la tabla con 0, 6 y 0, cada una con su `// before N`, y su prueba sin cambio. Sería Red: en el RED, WHC aún tiene las cuentas de la base | D-PBT4-11; §4.2; §6.2; §8.3; §8.5 |
| **V48-04** | **El paseo de guardas no veía los campos de tipo tupla** (`walk_guards_pbt4b_v45.py:91`) | **`walk_guards_pbt4b_v48.py`**, con el tipo de `FIELD` admitiendo una tupla y un genérico con espacios. **Con las entradas de la v4.5**, marca tres pruebas más fuera de la lista de 477 (`v48/compare_walks_v48-same-inputs-out.txt`). Una es la de la sexta tanda; **las otras dos no entran en la lista** y se declaran en §4.2: `WorkspaceHostHardeningGuardTests.TheGuardOfTheLiteralsTrips…`, una autoprueba que pasa, y `WorkspaceCommandAdvisorGuardTests.TheCatalogBody…`, cuyo `"Read"` es el valor de la columna `Body` de su tupla, no un literal que se cuente en RIA. **Las dos pasan en las dos suites completas del GREEN** (`/d/r4m` y `/d/r4o`). Con las copias de la v4.8 y la lista de 478, en §4.2 | §4.2 |
| V48-05 | Las cifras que dependen de la lista | Core sin las CT: **19 587 = (19 129 − R_C) + R_C + 458** en el RED y **19 587 = 19 129 + 0 + 458** en el GREEN. Con las CT: **19 605 = 19 147 + 0 + 458**. La UI no cambia. Además: el `MinSelected` del filtro n.º 12, de 2 719 a **2 718**; el `AllowedWriteScope`, de 57 a **59** rutas; las copias y los archivos de los `Skip`, de 20 a **21**; las clases de las copias, de 40 a **42**; R_C, `<medido en la precondicion 6>` (previsto: 292 + 1 − 2 = 291) | §5.2; §6.2; §6.2.1; §6.2.2 |
| V48-06 | La versión | El `Objective` de PB-T4b cita la v4.8, «sus seis tandas» y las 478; el de PB-T4a sigue citando la v4.3. **Las corridas de la precondición 6:** `/d/r4g` a `/d/r4i` (v4.5 y v4.6), `/d/r4j` a `/d/r4m` (v4.7), los diagnósticos `/d/r4n` a `/d/r4p` y las de la v4.8 (`/d/r4q` en adelante) | §6.2.1; §11 |

---

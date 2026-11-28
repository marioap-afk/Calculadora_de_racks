**Estado:** v5.5, **sin emitir**. Añade **PB-T4e** (`F4B-PBT4-EDIT-ALL-RELOAD`, la recarga de «Editar todo»; el nombre de la subtarea es provisional y lo confirma la sesión al emitir) en forma literal, como la v5.2 a la v5.4 lo hicieron con PB-T4d, sobre el **cierre de PB-T4d** (`160e019d`, F4B-E59: CERRADA, intento 1 de 3, sin reparación del RED). La remedición se hizo por objetos git (`git show`, `grep`, `ls-tree`) y la precondición 6, en copias exportadas nuevas de `160e019d` (`/d/r71` en adelante; `pbt4ecc/compile/`). Todo lo de PB-T4a a PB-T4d sigue igual.

**Las decisiones que la fijan:**
- **el Owner, la 15.ª tanda,** `owner-pbt4e-list-decisions.md` (sha256 `{{SHA_OWNER}}`), cuatro respuestas literales: «Solo la recarga (Recomendado)», «No abrir y avisar (Recomendado)», «Antes de PB-T5 (Recomendado)» y «Sí, con condiciones (Recomendado)» (§8.5);
- **el origen:** OWNER-02 de la revisión del código de PB-T4c, que el Owner aceptó con «Aceptarlo y corregir luego» (`owner-pbt4c-code-decisions.md`, pregunta 2), y la pregunta 2 de `owner-pbt4d-list-decisions.md`, «Tarea propia después»;
- **la sesión, dos decisiones de diseño que el Owner le dejó** (`owner-pbt4e-list-decisions.md`, «Lo que queda decidido»), **escritas aquí como propuestas para la sesión**, que las confirma o las cambia al revisar: **D-S17**, la recarga en el anfitrión, y **D-S18**, `EditAllStatus.NotBegun` reutilizado (D-PBT4-22; §8.5).

**Lo que cambia:** la cabecera (este registro, y la de la v5.4, que pasa a su sección sin cambio) y, medido por el diff de las secciones de la v5.4 y la v5.5 (`v55/make_v55-out.txt`, no escrito a mano: M-10 y M-14 de las versiones anteriores), **{{LO_QUE_CAMBIA}}**. **Lo que no cambia:** {{LO_QUE_NO_CAMBIA}}.

**Lo que sale de la v5.5 (PB-T4e):**
- las dos piezas literales, `pbt4e-green-v55.txt` (solo `src/`, sha256 `{{SHA_GREEN}}`) y `pbt4e-red-v55.txt` (solo `tests/`, sha256 `{{SHA_RED}}`), y sus módulos y aplicadores (`green_pbt4e_v55.py`, `apply_green_pbt4e_v55.py`, `red_pbt4e_v55.py` y `apply_red_pbt4e_v55.py`);
- su generador, `v55/make_pieces_pbt4e_v55.py`, con sus fuentes (`v55/frag/green-src.txt` y las dos clases de `v55/ct/`) y su registro (`v55/make_pieces_v55-out.txt`);
- la precondición 6 (`pbt4ecc/compile/`): el ensayo y la sonda (`t71_trial_v55.sh`, `v55/probe/write_probe_v55.py`, `trx_lines_v55.py`), la copia principal (`p6_pbt4e_v55.sh`), la base con cobertura (`p6_basecov_v55.sh`), el control de las CT (`p6_control_v55.sh`, `control_pieces_v55.py` y la sonda de «Actualizar», `v55/probe/SondaPushBackEditAllReloadUpdate.cs`), la cobertura (`cov_p6_pbt4e_v55.py`), los `RequiredTests` y los totales (`req_pbt4d_v55.py`, que escribe `required-pbt4d-emitted.tsv`, y `required_pbt4e_v55.py`), el resumen (`p6_json_pbt4e_v55.py`, que escribe `pbt4e-precondition6.json`, `compile-check.md` y `values-v55.json`) y el conductor de lo que sigue a la copia principal y a la base (`post_p6_v55.sh`, con su registro `post-p6-v55-out.txt`); los ayudantes copiados sin cambio de PB-T4c y PB-T4d (`check_trx_v55.py`, `classes_v55.py` y `cov_v55.runsettings`);
- y `v55/make_v55.py`, que arma esta versión desde la v5.4 con los fragmentos de `v55/frag/`.

---

## Registro de cambios v5.4 → v5.5

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V55-01** | **La 15.ª tanda del Owner** (`owner-pbt4e-list-decisions.md`): «Solo la recarga», «No abrir y avisar», «Antes de PB-T5» y «Sí, con condiciones» | PB-T4e es una subtarea propia, la quinta, con su cadena y su intento 1 de 3, cerrada antes de PB-T5; se emite por la autorización condicional | §0; §1; D-PBT4-01; §8.5; §11 |
| **V55-02** | **El diseño, D-PBT4-22:** «Editar todo» recarga un borrador Clean marcado como posiblemente obsoleto con la misma función que «Editar», antes de abrir; si la recarga no sale, no abre y el campo muestra lo mismo que «Editar» | El cuerpo de `EditAllOfPilot` (WHC), el aviso de `EditAll` con su constante, que usa también `BeginEdit` (FVM), y los comentarios de WEI y del puerto; nada más visible | §3 (la sección PB-T4e); D-PBT4-19; D-PBT4-20 |
| **V55-03** | **Las dos decisiones de la sesión, propuestas** | D-S17 (la recarga en el anfitrión) y D-S18 (`NotBegun` reutilizado), con su evidencia `ruta:línea` y su efecto en las CT | D-PBT4-22; §8.5 |
| **V55-04** | **La lista, vacía, medida** | K = 0, D = 0 y E = 0: la suite completa de Core y los 19 filtros de UI del contrato, sobre el RED exacto y el GREEN literal, no dan ninguna prueba existente que falle; ninguna se edita, se salta ni se sustituye | §4.5; §8.3 |
| **V55-05** | **Las CT** | CT-PBT4E-01, 02 y 04 (Core) y CT-PBT4E-03 (UI), en dos clases nuevas, con sus 24 líneas medidas por la sonda sobre el GREEN literal; la CT-05 opcional de la propuesta no se escribe (redundante con CT-01; AGENTS.md, «Guardia de fuente y prueba de comportamiento») | §5.7; §6.0 (RR-3) |
| **V55-06** | **Las piezas** | El GREEN (8 bloques EDIT en 4 archivos de `src/`) y el RED (2 bloques NEWFILE), sin esqueleto, con su generador y sus herramientas `apply_*`, que el Worker corre sin escribir código | §6.5.2; el anexo V55-A |
| **V55-07** | **El contrato literal** | `Objective`, `AllowedWriteScope` (6), INV-PBT4E-01 a 14, PBT4E-C01 a C14, C16, C17 y C18, `ExpectedEvidence`, `RequiredTests` (90) y los totales exactos de cada rama (F4B-E46) | §6.5; §6.5.1 |
| **V55-08** | **La precondición 6** | En copias nuevas: el ensayo y la sonda (`/d/r71`), la copia principal, sola (`/d/r72`), la base con cobertura (`/d/r73`) y el control de las CT (`/d/r74` y `/d/r75`) | §6.5.1; §9; §11 |
| **V55-09** | **El Smoke-PB** | Suma la recarga de «Editar todo» (los pasos de la propuesta de lista, §4) a la lista de PB-T5 | §5.5; §10 |
| **V55-10** | **Medido distinto de lo deducido:** en la base, la receta de punta a punta no acaba en el conflicto de «Actualizar», sino en `NotAppliedPreflight` | Se declara (R-T4-39) y se describe con la sonda de «Actualizar»; no cambia el diseño ni la lista | D-PBT4-22; §7 |
| V55-11 | La CI del RED | `ui-tests` en failure en PB-T4e (CT-PBT4E-03) | §5.4; §6.0 |

---

## La cabecera de la v5.4, sin cambio salvo lo que dice el registro v5.4 → v5.5


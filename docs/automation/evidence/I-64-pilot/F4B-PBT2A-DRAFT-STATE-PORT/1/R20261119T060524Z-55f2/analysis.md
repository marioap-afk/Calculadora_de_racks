# I-64 F4B PB-T2a — Análisis del Coordinator para la corrección del intento 2

**Fecha:** 2026-11-19. **Tarea:** `F4B-PBT2A-DRAFT-STATE-PORT`. **Lo que corrige:** la entrega del Worker `R20261119T060524Z-55f2` (delegación `R20261119T060327Z-ad3e`, intento 1). **FailureClass:** `StopCondition` (PBT2A-C07). **Presupuesto:** la corrección consume intento (16.8: un STOP cuya resolución cambia el trabajo). PB-T2a pasa al **intento 2 de 3**. El intento 3, si llegara, vuelve al Owner (PBT2A-C12).

## 1. Lo que pasó

**El RED `c33ac6ac994af983ca176910c07f009155f3449e`:** el esqueleto inerte de los dos archivos nuevos de Application y las dos clases de prueba.
- En local, el filtro con ExpectRed da 3 de 3 en fallo y la guarda G-L1 pasa.

**La implementación** de D-PBT2-02 y D-PBT2-03 se escribió sobre el árbol, **sin commit** (`green-wip-uncommitted.patch` del intento 1).
- Con ella pasan CT-PBT2A-01 y 03.
- **CT-PBT2A-02 falla en la línea `launchfor`.**

**La causa** está en cómo se escribió la prueba. El §5.2 del paquete v2.2 fija `LaunchFor(draft, p)` → `launchfor estado-es-el-actual=True id=GUID-PBT2-0001 puerto-es-del-borrador=True`, donde la última parte comprueba que aplicar por ese puerto cambia ese borrador. Pero la prueba (`PushBackDraftStatePortTests.cs`, en `c33ac6ac`, líneas 414-415) hace dos cosas en el orden equivocado:
1. **primero aplica** la edición por el puerto (`desdeBorrador.Port.ApplyToDraft(edited)`);
2. **después** evalúa `estado-es-el-actual` con `ReferenceEquals(desdeBorrador.State, draft.CurrentDraftState)`.

Como `SetCurrentDraftState` deja en el borrador una copia nueva, a esas alturas el estado del lanzamiento ya no es el actual. Sobre el esqueleto no se veía, porque la línea falla antes por la excepción del API inerte (la regla «EXCEPCION»).

**Lo que hizo el Worker:** no ajustó la prueba ni el producto, porque 16.8 impide tocar `tests/` tras el RED. Paró con PBT2A-C07.

**No es un defecto del paquete ni del diseño.** El paquete pide que el lanzamiento lleve el estado actual del borrador y que aplicar por su puerto cambie ese borrador. Son dos comprobaciones, cada una en su momento.

## 2. La corrección (VINCULANTE para la delegación de corrección)

**C-1. CT-PBT2A-02, la línea `launchfor`.** En `tests/RackCad.Tests/PushBackDraftStatePortTests.cs`, dentro de la lectura de `LaunchFor(draft, p)`, el orden pasa a ser este:
- `estado-es-el-actual = ReferenceEquals(desdeBorrador.State, draft.CurrentDraftState)` e `id` se toman **ANTES** de aplicar la edición;
- después se aplica la edición por `desdeBorrador.Port`;
- después se toma `puerto-es-del-borrador`: aplicar por ese puerto cambia ese borrador.

La línea esperada no cambia (`launchfor estado-es-el-actual=True id=GUID-PBT2-0001 puerto-es-del-borrador=True`), ni la regla «EXCEPCION» que la envuelve. Nada más de la clase cambia.

**C-2. Nada más en `tests/`.** CT-PBT2A-01, CT-PBT2A-03 y la guarda CT-PBT2A-04 quedan como en `c33ac6ac`.

**La desviación declarada en CT-PBT2A-04.** El paquete dice que cada autoprueba se inyecta «en el texto de cada archivo nuevo». La prueba la inyecta en el texto de los dos archivos y exige que fallen ambos. No se toca: es más estricta que la letra. La revisión independiente la juzgará.

**C-3. El GREEN** es D-PBT2-02 y D-PBT2-03, solo en los dos archivos nuevos de `src/`, con todos los invariantes del contrato.
- El Worker puede partir del parche del intento 1 (`.../1/R20261119T060524Z-55f2/green-wip-uncommitted.patch`). No es una autoridad.
- Lo vuelve a comprobar todo: los invariantes, todos los filtros, la suite completa de Core y la CI exacta.

## 3. El régimen de la cadena (16.8; M-11)

**La base y la cadena:**
- **BaseSha:** el commit de la sesión que custodia este análisis. Contiene el RED `c33ac6ac`, con el esqueleto inerte en los dos archivos de `src/`.
- **ChainBaseSha:** `0dc623ca7a4bf1640654440276aa3d27147f48d4`, la BaseSha de la primera delegación.
- **ChainRedSha y ChainRedFiles:** ChainRedSha = null y ChainRedFiles = [], porque ninguna verificación acreditó el RED `c33ac6ac`: el intento 1 paró antes del GREEN. Es el precedente de F4-T4C2 y de PB-T1c, en el intento 2.
- **La entrega exige RED.**

**El RED de la corrección:**
- Contiene solo la clase corregida, `tests/RackCad.Tests/PushBackDraftStatePortTests.cs`.
- En `src/` no cambia nada: los dos archivos siguen con el esqueleto de `c33ac6ac`.
- **Su CI:** tests en failure (CT-PBT2A-01 a 03), ui-tests y build-ui en success, y build-plugin omitido.
- **RT** = `git diff --name-only ChainBaseSha RedSha -- tests/`, es decir, las dos clases.

**El GREEN de la corrección:**
- Toca solo los dos archivos de `src/` y ningún archivo de `tests/`.
- `git diff --name-only BaseSha RedSha` solo contiene esa clase de prueba.
- `git diff --name-only RedSha CurrentSha` solo contiene los dos archivos de `src/`.

**Las autoridades:** el contrato de PB-T2a no declara rutas UNIT_CHANGE, así que 16.3 no cambia, y F4B-E25 no hace falta.

## 4. Lo que no cambia

- El contrato (paquete v2.2, AuthorityRevision `1ee10d3f`), sus invariantes, sus RequiredTests y sus paradas.
- D2 y su límite.
- Las decisiones de F4B-E28.
- La regla de limpieza: si cambia SOLO `RackFrameConfigurator.layout`, es el defecto conocido anterior a F4B (F4B-E30, `docs/ideas-futuras.md`) y no es parada.
- Ninguna prueba ni ayudante existente cambia.

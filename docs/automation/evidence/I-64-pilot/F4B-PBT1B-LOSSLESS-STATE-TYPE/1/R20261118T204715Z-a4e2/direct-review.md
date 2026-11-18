# I-64 F4B PB-T1b — Revisión directa de la sesión

**Objeto:** la entrega del Worker `R20261118T204715Z-a4e2`. Viene de la delegación `R20261118T204434Z-6063`, con el contrato de la v2.7, intento 1.
- **BaseSha:** `4b0498e3`.
- **RED:** `2d9febc6087280e0ba9f7a6c72e77b283506891d`.
- **GREEN:** `a0a5f699b138c1b5251fb0c88d28e3ca9e1610c3`.
- **Estado:** IMPLEMENTATION_COMPLETE y Disposition NONE, sin paradas.

**Método:** git, la entrega y los TRX de la CI, en solo lectura. No hice ninguna corrida.

## Lo que se sostiene

| Comprobación | Resultado |
|---|---|
| **Alcance** (INV-PBT1B-01) | El diff de `4b0498e3` a `a0a5f699` son exactamente las 12 rutas del AllowedWriteScope: los cuatro archivos nuevos de Application, ES, RMES y las seis clases de prueba. El relevo comprueba que el RED lleva las seis pruebas y el esqueleto en `src/`, y que el GREEN solo toca `src/` |
| **El RED es el esqueleto** | Las líneas que el RED añade en `src/` son pocas frente a las del GREEN: el estado +136 frente a +467, el comparador +20 frente a +502, el mapa +64 frente a +177, la barrera +20 frente a +42, ES +8 frente a +16 y RMES +16 frente a +162. El comparador del RED es el del §5.3 (`ReferenceEquals`, hash 0 y `Copy` vacío) |
| **Ningún campo nuevo** (INV-PBT1B-04) | `git diff -U0` de ES y RMES no añade ninguna declaración de campo |
| **CT-08 / M02 de la v2.7** | La batería de mutación excluye los campos de `SafetyFacePiece` (`PushBackEditableStateCopyTests.cs:743-747`), y CT-06 tiene sus exclusiones ancladas (`:422-429`) |
| **El paso 1** | Las filas (a) a (f) son las mismas que en las dos invocaciones anteriores: c-02 en la rama ordinal y c-09 en la rama «conserva». Sin C14 |
| **pb-15** | CT-02 da exactamente la normalización conocida de F4B-E20: `iguales=False SideB.Fronts[0].IsActive=True->False normalizada-iguales=True`. Sin C15 |
| **Las guardas de D2** | CT-14: `reglas-sin-coincidencias=True` y sus trece autopruebas fallan. CT-15: 14 archivos, 7 asignaciones y 3 escritores de Push Back en orden; sus dos autopruebas fallan |
| **La CI del RED** (`44400123417`) | Tests y ui-tests en failure y build-ui en success; build-plugin, omitido. En Core fallan 14: los 13 de `PushBackEditableState` y el de `PushBackEditorOutputGate`. En UI falla 1, la paridad. Ninguna prueba existente falla |
| **La CI del GREEN** (`44405531606`) | 4/4. Core 19 086 y UI 2 437 (2 416 `Passed` y 21 `Skipped`). Es **exactamente** la previsión del §6.2 |
| **Relevo del trabajo** | Válido: 258 turnos de claude-sonnet-5-5 en high, 0 denegaciones, 0 procesos participantes y la Snapshot J |

## Lo que hay que mirar (para la revisión independiente)

- **El borrador heredado** (`Deviations[0]`). El Worker partió del parche sin commit de `fb01`, que se escribió con la v2.6. Declara que el árbol del RED tenía solo el esqueleto, y eso cuadra con los recuentos de arriba. Lo que hay que comprobar es si arrastra algo de la v2.6 que la v2.7 corrigió (CT-06 o CT-02).
- **Una corrida temporal de CT-01** (`Deviations[1]`). Cambió un archivo de forma temporal para ver el lado de la ventana sobre el esqueleto, y lo revirtió sin commit. Está dentro de lo que permiten las corridas temporales.
- **CT-10** (`UnexpectedFindings[1]`). En su primera escritura fallaba por excepción, y se corrigió antes del RED como error de escritura propio. Hay que comprobar que lo que fija no cambió.

## Veredicto de la revisión directa

Conforme en alcance, esqueleto, ausencia de campos, guardas, paso 1, pb-15 y CI. Queda pendiente la revisión independiente, sobre todo del borrador heredado y de D2.

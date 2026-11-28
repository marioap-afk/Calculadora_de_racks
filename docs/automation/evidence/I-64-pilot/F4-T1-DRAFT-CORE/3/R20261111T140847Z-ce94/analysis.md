# Análisis de la entrega del intento 3 de F4-T1-DRAFT-CORE (solo N-1)

## Hechos

- **Entrega:** RED `5a27ac8b` y GREEN `ce73ceb7`, sobre `d737b485`. Solo cambia `tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs`.
- **CI push del SHA exacto** (`43029959059`): los cuatro jobs en success. Filtro `FullyQualifiedName~RackCad.Tests.Workspace` 1300/1300; Core 16838/16838.
- **RED** (`43029071176`): el job Core termina en failure con 13 fallas, todas del bloque de regresión y por aserción.
- **`n1_hashes.py`** (`d737b485`, `5a27ac8b`, `ce73ceb7`): `Pass` true.
  - H1, el bloque de regresión: igual en RED y GREEN (`96A696F1…2E62`).
  - H2, el archivo con la región de la regla sustituida: igual en RED y GREEN (`9BA25558…DF67`).
  - H3: el RED sin el bloque es idéntico a la base (`843DD9F1…88C5`).

## Revisión directa de la sesión

- El GREEN solo cambia el comentario y la declaración de `BomInAnyIdentifier`.
- La regla nueva contiene las de `90a81104` y `09955667`, añade `BOM` en mayúsculas y no trata como caso especial ninguna cadena literal.
- Las 13 fallas del RED son exactamente las filas que la regla debilitada no detecta: 7 de texto de código y 6 de nombres de miembro.
- Sin defectos.

## Revisión adversarial independiente

Texto íntegro en `independent-review.md`, junto a este archivo. No refuta el intento 3: 0 bloqueantes, 0 importantes y 2 menores.

- **m-1 (cobertura):** tres comportamientos de la regla no los fija ninguna prueba.
  - la alternativa `BOM(?![a-z])` (`DraftBOMLines`, `RACKBOMTOTAL`);
  - que `\bbom` acepte una minúscula detrás (`Bomb`, `BOMlines`, el superconjunto de `90a81104`);
  - `bomCount` en la prueba de nombres de miembro.
  La regla actual es correcta en los tres casos; el riesgo es solo ante cambios futuros.
- **m-2:** el comentario de la regla promete más de lo que la regla detecta dentro de un identificador (`DraftbOmLines` no se detecta). No es una regresión.

## Verificaciones

- **`R20261111T140248Z-5556`:** `EXECUTION_BLOCKED` / STOP por `Authority` (y también `FreeText`). La identidad del prompt de la sesión llevaba la AuthorityRevision del intento 2 (`bf429590`) en lugar de `c31854f8`. Fue un error de entrada de la sesión, corregido sin cambiar la entrega. Se conserva.
- **`R20261111T140847Z-ce94`** (reejecución con la entrada corregida): `EXECUTION_REWORK_REQUIRED` / REWORK, con `FailureClass` FreeText.
  - Las otras 13 comprobaciones pasan, incluidas `Tests`, con el oráculo estrecho de F4-E10, y `Ci`.
  - **FreeText:** `WorkCompleted` cita un archivo auxiliar del Worker, `WorkspaceDraftBoundaryTests.green-candidate.cs`. Su nombre contiene como palabra completa el término «Candidate» de 16.10, que se compara sin distinguir mayúsculas.
  - No es una declaración de estado de gate, pero es una coincidencia según la letra de la regla.

## Disposición

**F4-E11:**
- Con `attempts` en 3 de 3, corregir la entrega sería el intento 4: STOP (S-11) y devolución al Owner.
- No es un fallo de producto: N-1 está corregido y acreditado.
- nc1..nc3 no se ejecutan sobre una verificación REWORK; quedan para la verificación que cierre F4-T1.

## Seguimiento

- **m-1 y m-2:** a F4-T2 si su contrato incluye la guarda; si no, a la siguiente tarea que la toque.
- **Lección de proceso:** el prompt del Worker prohíbe los términos de 16.10 en el texto libre, pero no dice que también cuentan dentro de los nombres de archivo citados. Los prompts siguientes lo dirán.

## Límites

Sin AutoCAD.

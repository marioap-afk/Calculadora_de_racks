# I-65 F4-T4-COMPARATOR-AND-TOOL — analysis.md: STOP del Worker (intento 2, WorkRunId R20261011T202738Z-6356)

Redactado por el Coordinator según AUTOMATION_PLAN §16.11 (STOP → causa raíz y decisión del Coordinator).

## Hechos (comprobados mecánicamente)

- Delegación `R20261011T202527Z-1167` (contrato contra `AuthorityRevision` = `BaseSha` = `34831380`), celda `claude-sonnet-5-5`/`medium`.
- Entrega válida contra el esquema: `WorkerStatus` BLOCKED, `Disposition` STOP, `TriggeredStopConditions` [C-01]. RED `b51adf28`
  (CI `38173107295` Core `failure`), GREEN `0e9bbc29` (CI `38173786847`: Core `failure`, UI y Build UI `success`, Build Plugin `skipped`).
  Árbol limpio y `HEAD` = remoto = `0e9bbc29` a la entrada. Transcripción: 130 mensajes `claude-sonnet-5-5`, 78 llamadas, 1 error de
  herramienta superado, 0 denegaciones, **ninguna invocación de un intérprete**.
- Implementación completa según el Worker: 79 de las 81 pruebas focales pasan en el GREEN; la suite Core da 14 076 superadas y 2 fallidas,
  exactamente las dos aserciones defectuosas del propio RED, en `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisComparisonToolTests.cs`:
  1. `The_manifest_of_an_injection_is_marked_with_the_mutation_and_the_hash_of_the_source` espera la ruta literal `$.faultInjection`.
     Otra prueba del RED (`The_product_does_not_contain_the_marker_of_an_injection`) barre `src` sin distinguir mayúsculas y
     `faultInjection` coincide con el tipo existente `SiblingRedrawDebugFaultInjection` del Plugin; el GREEN renombró el marcador a
     `rackcadFaultInjection` (`RamToolInjection.Marker`), así que ambas pruebas no pueden pasar a la vez con el RED tal como está.
  2. `An_injected_package_is_a_new_folder_with_the_three_files_and_the_script_of_its_payload` espera
     `[structural-loads.json, manifest.json, rackcad-ram-build.py]` tras ordenar los nombres de forma ordinal; el orden ordinal real es
     `[manifest.json, rackcad-ram-build.py, structural-loads.json]`. Ninguna implementación puede satisfacerla.
- Dos advertencias de compilación en archivos del RED: `CS0649` en `StructuralAnalysisComparisonSupport.cs(43,27)` (campo `SpringI` sin
  asignar) y `xUnit2013` en `StructuralAnalysisComparisonToolTests.cs(447,13)`. El job Build Plugin de la CI compila la solución en
  Release y rechaza todo `warning (MSB|CS|NU|NETSDK)`: `CS0649` lo haría fallar.

## Causa raíz

El Worker escribió en el RED dos aserciones defectuosas (un literal escrito antes de renombrar el marcador y un orden esperado mal
calculado) y dejó dos advertencias en archivos de prueba. Desde el commit RED no podía editarlos, y paró. Es el mismo modo de fallo que
F4-T1 (CD-I65-F4-07): aserción defectuosa del propio RED, a pesar del aviso expreso del prompt.

## Clasificación y decisión del Coordinator (CD-I65-F4-18)

- El STOP se resuelve **con un cambio del trabajo** (§16.8): corrección; `attempts` 2 → 3 en un commit previo a la delegación. Es el último
  intento de la unidad: cualquier REWORK posterior es STOP (S-11).
- La sesión aplica el STOP declarado **sin verificación del Controller** (como DEV-I65-F4-04): los hechos se comprueban mecánicamente y una
  verificación del GREEN defectuoso no cambiaría la decisión ni acreditaría un RED utilizable por la corrección (DEV-I65-F4-09).
- **Sin excepción de RED** (no se aplica DEV-I65-F4-02 por analogía): la cadena no tiene RED acreditado (`ChainRedSha` null), así que la
  corrección EXIGE RED. Se usa la «corrección desactivada» de §16.8:
  - commit RED: las pruebas de la cadena corregidas (solo las dos aserciones, que pasan a expresar el comportamiento correcto
    —`"$." + RamToolInjection.Marker` y el orden ordinal real— y la eliminación de las dos advertencias sin debilitar lo observado) y la
    implementación desactivada: `StructuralReadbackComparison.cs` y `tools/RackCad.StructuralAnalysis.RamTool/` vuelven al esqueleto
    compilable de `b51adf28`, de modo que la selección focal falla por excepción o aserción;
  - commit GREEN: la implementación restaurada **byte a byte** desde `0e9bbc29` (`git diff 0e9bbc29 <GREEN> -- src tools RackCad.sln` vacío)
    sin tocar ningún archivo de `tests/`.
- Presupuesto: corrección (planificación y verificación) y nc1-nc3 de T4 = 5 invocaciones (#22-#26); T5 necesita otras 5 (#27-#31): una
  más que el tope de 30 del Owner. Se pedirá autorización antes de exceder (disposición del Owner, punto 3), al cerrar T4 y antes de T5.

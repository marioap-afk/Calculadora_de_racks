# I-65 — F4-D-SECTION-MAP-VERIFIED: analisis del REWORK del intento 6 (Coordinator)

Verificacion R20261016T152242Z-2e6f (Codex 43): EXECUTION_REWORK_REQUIRED / REWORK, FailureClass Ci; 12 comprobaciones en pass;
Ci y Tests en fail con RedPart pass (RED fe9b6720 acreditado). Entrega R20261016T145459Z-8301: RED fe9b6720, GREEN d548fb0b.

## Hechos

- CI push de d548fb0b (38922575054): job Core en failure, 3 fallidas de 14390; UI y Build UI en success; Build Plugin without AutoCAD
  skipped (depende de Core). Fallidas, las tres con "Expected: Valid / Actual: Invalid":
  - StructuralAnalysisSectionMapVerifiedTests.LaFabricaDaUnPaqueteValidoEnMemoriaConLaTablaDistribuidaSinEscribirLaCarpeta
  - StructuralAnalysisSectionMapVerifiedTests.CambiarUnCampoDeUnaFilaVerifiedCambiaLaHuellaDelPaquete
  - StructuralAnalysisPluginCommandProductTableTests.ConLaTablaDeProducto_LaFabricaDaUnPaqueteValidoEnMemoria_SinCrearLaCarpeta
- En Windows (corridas locales de la entrega) las tres pasan: filtro 1795/1795 y suite 14390/14390.
- Con la tabla verified, el modelo de REF-CANT-01 es Valid con 0 diagnosticos tambien en Linux: la prueba
  RefCant01ConLaTablaDistribuidaEsValidoConLosMismosDiagnosticosQueConLaTablaVerifiedDeFixture pasa en el TRX de la CI.
- El blob de la tabla en d548fb0b es d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2 = el de f4d-table-target.json en e886b557 (D-INV-02).

## Causa raiz

RamBuildPackageFactory solo acepta como carpeta del paquete una ruta WINDOWS absoluta y normalizada (IsAbsoluteNormalisedFolder:
letra de unidad con ":\" o UNC; A65-F3-07: el script se ejecuta en el programa de analisis, en Windows) y en otro caso da el
diagnostico bloqueante RAM-PACKAGE-FOLDER-INVALID con resultado Invalid. Las tres pruebas pasan como carpeta
Path.Combine(Path.GetTempPath(), ...): en Windows es una ruta "C:\Users\...\Temp\..." valida; en el runner Linux de la CI es
"/tmp/...", que la fabrica rechaza con razon. La validacion es sintactica (la fabrica no escribe nada), asi que una cadena de ruta Windows
absoluta es valida en los dos sistemas, como hacia la prueba original de la tabla del producto ("C:\Paquetes\estacion-0").

La instruccion que llevo al defecto es del Coordinator: el prompt del Worker pedia "una carpeta de paquete de prueba debe ser absoluta
en los dos sistemas: Path.Combine(Path.GetTempPath(), ...) (una ruta C:\... no es absoluta en Linux)", lo que contradice el contrato
documentado de la fabrica. El Worker la siguio. No es un defecto del producto, ni de la tabla, ni un diagnostico del modelo: no activa
C-03 ni amplia el alcance.

## Correccion (intento 7; mismo contrato de gate)

- Solo pruebas, solo las tres afectadas, en dos archivos de la cadena RED:
  StructuralAnalysisSectionMapVerifiedTests.cs y StructuralAnalysisPluginCommandProductTableTests.cs. La carpeta del paquete pasa a ser una
  cadena de ruta WINDOWS absoluta fija que no existe en ninguna maquina (por ejemplo @"C:\RackCad-F4D-NoExiste\paquete-estacion-0");
  la prueba sigue afirmando Outcome Valid, Package no null y que la carpeta no existe despues (la fabrica no escribe). Ninguna otra
  asercion cambia; la tabla, el modelo, la huella y las demas pruebas quedan igual.
- Cadena: ChainBaseSha = e886b557; ChainRedSha = fe9b6720 (acreditado); ChainRedFiles = los cinco archivos de pruebas de
  e886b557..fe9b6720 -- tests/. La correccion toca ChainRedFiles, asi que EXIGE RED: RED = correccion desactivada (las pruebas
  corregidas MAS la tabla devuelta a su contenido de e886b557, con las filas proposed) y GREEN = solo la tabla, copiada de nuevo del
  archivo objetivo (mismo blob d088e2a9...). git diff RED..GREEN -- tests vacio; diff neto de la correccion = las dos pruebas.
- Procedimiento reforzado en el prompt: ningun archivo temporal fuera de .../work/ ni sondas compiladas en tests/ (DEV-I65-F4-23);
  archivos C# solo con las herramientas de edicion; ninguna llamada que empiece con cd.
- attempts 6 -> 7 (AttemptsRemaining 2 con max_attempts 9); contador de la clase Ci = 1; Codex: 43 de 48 consumidas; restan la
  planificacion de la correccion, su verificacion y nc1-nc3 (48 de 48, sin reserva).

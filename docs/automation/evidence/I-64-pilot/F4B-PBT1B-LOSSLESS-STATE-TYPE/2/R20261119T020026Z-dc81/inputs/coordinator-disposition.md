# Disposiciones del Owner para la verificacion del intento 2 de PB-T1b

## Disposicion 1: dos frases del contrato de PB-T1b (M10 y M14 de la revision independiente del intento 1)

Fecha: 2026-11-18. Canal: chat de la sesion principal (pregunta de opciones; respuesta literal). Texto completo en F4B-DRAFTS/coordinator/owner-pbt1b-rework-decisions.md (F4B-E22), custodiado en 2f5d3dec.

Hechos (revision independiente del intento 1, Worker R20261118T204715Z-a4e2, hallazgos PBT1B-IR-M10 y PBT1B-IR-M14):
- M10: la lista cerrada de ayudantes privados de INV-PBT1B-03 no se cumple al pie de la letra. El comparador (PushBackEditableStateComparer.cs) tiene los privados TailsEqual, ModulesEqual, LinesEqual, AuthorityEqual, Unchanged, OpenOver, BaselineIntentEqual, CarriersEqual, Persisted y CopyPallet, que no salen de PushBackPilotStateComparer.cs:64-83 ni :134-350; el mapa (PushBackEditableStateOwnership.cs) tiene los privados Covers, FirstProperty y BuildTable y el campo estatico privado IndexPattern. Son necesarios para D-PBT1-06 y D-PBT1-08; no anaden miembros publicos ni campos a los tipos del censo de INV-PBT1B-04.
- Hecho anadido por la sesion (no forma parte de M10 ni de la pregunta al Owner): el mapa tiene ademas cinco constantes privadas de texto (EmbedDocument, ProjectDocument, PayloadDocument, EditorSource y DesignAssemblers, PushBackEditableStateOwnership.cs:60-66 en a0a5f699), que son datos literales de la tabla y no ayudantes. Esta disposicion no las cubre; su encaje con INV-PBT1B-03 y INV-PBT1B-04 lo juzga la verificacion con la letra del contrato.
- M14: ExpectedEvidence pide build-plugin en success en la CI del RED, pero en .github/workflows/ci.yml build-plugin depende de tests (needs), que en el RED falla a proposito; build-plugin sale omitido.

Pregunta al Owner: "PB-T1b: dos frases del contrato no se pueden cumplir al pie de la letra. (1) Cierra la lista de ayudantes privados, y el comparador correcto necesita mas (sin miembros publicos ni campos nuevos). (2) Pide que la compilacion del plugin de exito en la CI del RED, pero esa compilacion depende de Core, que en este RED falla a proposito, asi que queda omitida. Las ratificas?"

Respuesta del Owner: "Ratificar, solo PB-T1b (Recomendado)".

Alcance de la disposicion, SOLO para PB-T1b (este intento 2 de la cadena):
- M10: ratifica los ayudantes privados listados arriba, tal como estan en la implementacion del intento 1 (GREEN a0a5f699), que el GREEN de esta correccion restaura sin cambios. Para ese punto prevalece sobre la letra de la lista cerrada de INV-PBT1B-03. No ampara ningun otro miembro privado, ningun miembro publico nuevo ni ningun campo en los tipos del censo de INV-PBT1B-04.
- M14: ratifica que en la CI del RED build-plugin salga omitido por su needs (y no en success). Para ese punto prevalece sobre la letra de ExpectedEvidence; el resto de la evidencia del RED (tests y ui-tests en failure, build-ui en success, sin blame-hang ni timeout, los TRX de cada filtro con ExpectRed) sigue igual.
- No amplia ninguna otra regla, otra subtarea ni otro intento. El paquete de PB-T1c corrige el texto de las dos frases.

Decision del Owner en la misma vuelta que no cambia esta verificacion: corregir en el intento 2 las pruebas (I01, I02, M01, M02, M04, M06, M07, M08 y M13), segun el analisis del Coordinator custodiado (AnalysisSha256 9891487e4cac8540f3c377ac848e55380ef7dffb76cfec303c4947ead7f8f0bf).

## Disposicion 2: las rutas UNIT_CHANGE en la base de una correccion (AUTOMATION_PLAN 16.3), regla de F4B

Fecha: 2026-11-19. Canal: chat de la sesion principal (pregunta de opciones; respuesta literal). Texto completo en F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md, que la sesion custodia al cerrar la tarea. Origen: la verificacion R20261119T014757Z-c78a de este intento paro en Authority (EXECUTION_BLOCKED / STOP) porque git diff --name-only 5e52aaa3 2f5d3dec contiene ES y RMES, rutas UNIT_CHANGE del contrato, cambiadas solo por el RED 2d9febc6 y el GREEN a0a5f699 del Worker del intento 1 de esta cadena; las otras 12 comprobaciones pasaron y Scope fallo por un control extra mal planteado por la sesion (ChainBaseSha..CurrentSha incluye commits de documentacion de la sesion).

Pregunta al Owner: "PB-T1b, intento 2: la verificacion del Controller pasa 12 de 14 comprobaciones (pruebas, CI, alcance de la entrega, trailer...), pero PARA en Authority. Su regla (AUTOMATION_PLAN 16.3) prohibe que entre la revision de autoridad y la base haya cambios en rutas declaradas UNIT_CHANGE. Aqui los hay en dos archivos de producto, ES y RMES. Solo los cambio el GREEN del intento 1 de esta misma cadena, porque al corregir no reemiti el contrato. La regla no preve este caso, y se repetiria en PB-T1c y en las demas correcciones. (El otro fallo, Scope, es un control extra mio mal planteado; lo corrijo.) Como se trata?"

Respuesta del Owner: "Regla F4B y reverificar (Recomendado)" (opcion descrita: "En una correccion de F4B, una ruta UNIT_CHANGE cambiada solo por commits del Worker de la misma cadena, dentro de su alcance, no incumple 16.3; el Controller lo comprueba commit a commit. PB-T1b se verifica de nuevo, sin gastar intento ni cambiar nada.").

Alcance de la disposicion, para las correcciones de F4B: una ruta con secciones UNIT_CHANGE en git diff --name-only AuthorityRevision BaseSha de una delegacion de correccion NO incumple 16.3 si (i) todo commit de AuthorityRevision..BaseSha que la toca es el RedSha o el CurrentSha de una entrega de un Worker de la misma cadena (mismo TaskId) segun su worker-handoff.json, (ii) la ruta esta en el AllowedWriteScope de la delegacion de esa entrega y (iii) ningun commit de la sesion de ese rango la toca. Las rutas EXTERNAL siguen prohibidas en ese diff sin excepcion; el resto de 16.3 sigue igual. No amplia ninguna otra regla, no cambia la entrega ni el producto y no gasta intento.

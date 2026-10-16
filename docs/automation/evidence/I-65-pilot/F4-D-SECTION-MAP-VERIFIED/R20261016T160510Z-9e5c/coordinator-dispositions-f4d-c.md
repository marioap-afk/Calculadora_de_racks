# I-65 — Disposiciones del Coordinator para la verificacion de la correccion de F4-D-SECTION-MAP-VERIFIED (WorkRunId R20261016T154337Z-8aed)

Texto de entrada de la verificacion del Controller (lecciones de I-61: las disposiciones se entregan como ENTRADA y se registran despues).

- D1 (cadena): ChainBaseSha = e886b557 (BaseSha de la primera delegacion); ChainRedSha = fe9b6720 (RED acreditado por la verificacion
  R20261016T152242Z-2e6f); ChainRedFiles = las cinco pruebas de e886b557..fe9b6720 -- tests/. La correccion toca ChainRedFiles, asi que
  exige RED: el RED 2188a1d7 es la correccion desactivada (las dos pruebas corregidas mas la tabla devuelta al blob de e886b557, con las
  filas proposed) y el GREEN 51fc82f1 solo vuelve a copiar el archivo objetivo sobre la tabla (blob d088e2a9...). Por eso el diff neto de
  la correccion (BaseSha c487262f..CurrentSha) son solo las dos pruebas y la tabla de CurrentSha es la misma que la de BaseSha.
- D2 (causa del REWORK anterior, texto en analysis-attempt6.md): la CI de Linux rechazaba la carpeta Path.GetTempPath() de tres pruebas
  porque la fabrica exige una ruta Windows absoluta (RAM-PACKAGE-FOLDER-INVALID); no era un diagnostico del modelo ni de la tabla. La
  correccion cambia solo esa carpeta por la constante de ruta Windows absoluta fija C:\RackCad-F4D-NoExiste\paquete-estacion-0.
- Los commits de sesion 2a1ed99a y c487262f (solo docs/automation, registros del REWORK y del rechazo P-03 de la planificacion
  R20261016T153224Z-5bb3) tienen CI con Core en failure porque contienen las tres pruebas del intento 6: no son la entrega.
- CORRECCION (intento 7; CorrectionOf R20261016T152242Z-2e6f, Ci): esta entrega EXIGE RED (RedSha 2188a1d7). El alcance mecanico de la
  sesion esta en inputs/scope-mechanical.json (y el del RED en inputs/scope-mechanical-red.json); evalua Scope tu mismo, archivo por
  archivo, contra el AllowedWriteScope de la delegacion de entrada.

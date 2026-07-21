# Contratos de iniciativas

`docs/ROADMAP.md` es el indice global de fases, dependencias, conflictos y estado integrado. No se
usa para guardar el estado transitorio "en curso": ese estado se deriva de Git y se registra para el
ejecutor en `docs/automation/state/<initiative>.yml`.

`docs/AUTOMATION_PLAN.md` gobierna la seleccion automatica, los limites de concurrencia, el reclamo
atomico, los reintentos y las condiciones de detencion. Cada archivo de esta carpeta es el contrato
detallado de una iniciativa y define su alcance, contexto, validaciones y entrega.

`docs/HANDOFF.md` conserva unicamente el estado vivo del proyecto. Los contratos no copian el
historial general, conteos de pruebas ni hashes: enlazan a las fuentes correspondientes.

Reglas de ejecucion:

- una ejecucion reanuda primero una iniciativa activa sin gate y ejecuta como maximo una fase
  coherente o una correccion de fallo;
- solo si no existe trabajo activo reanudable puede tomar como maximo una iniciativa nueva;
- una rama remota aceptada reclama la iniciativa;
- una iniciativa activa reutiliza su rama, worktree limpio y Pull Request draft; nunca duplica
  ninguno de ellos;
- `docs/automation/state/<initiative>.yml` es el estado transitorio canonico legible por el ejecutor;
- el bloque `automation_state` del Pull Request es una copia opcional y no bloquea la publicacion si
  no puede actualizarse;
- las decisiones del dueno pueden versionarse en `docs/automation/decisions/<initiative>.md`;
- el ejecutor no depende de GitHub CLI: Git basta para commit, push y estado versionado;
- el agente nunca hace merge ni activa auto-merge;
- las validaciones en AutoCAD son responsabilidad del dueno y bloquean la integracion, no
  necesariamente otra iniciativa compatible;
- Git y los resultados verificables prevalecen sobre el estado versionado, la copia del Pull Request
  y el campo `status` del front matter;
- el archivo de estado se actualiza al terminar cada ejecucion; `completed` no significa integrado;
- un contrato se crea desde `TEMPLATE.md` y no inventa alcance ausente del ROADMAP.

Durante el bootstrap, el plan y los contratos se leen desde `origin/docs/reestructura` y solo puede
reanudarse I-06. Despues de integrar el sistema documental, las reglas globales se leen desde la
punta de `origin/main`.

Planes disponibles:

- `I-06-reestructura-context-packs.md`: contrato del piloto documental I-06. Bootstrap, reclamo,
  auditoria y decision de taxonomia ya terminaron; el estado actual vive en
  `../automation/state/I-06.yml`.
- [`I-26-test-catalog-ids.md`](I-26-test-catalog-ids.md): contrato manual para centralizar IDs
  canónicos de pruebas, verificar los catálogos distribuidos y publicar cobertura Cobertura en CI.
- [`I-09-refactor-plugin-commands.md`](I-09-refactor-plugin-commands.md): contrato para partir
  `RackFrameCommands` por área, promover helpers de bloques/clonación/capas/transacciones y unificar
  el escaneo de envelopes triplicado, preservando comandos, geometría, BOM, persistencia y UX. Fuera
  de alcance I-10 e I-16 y cualquier cambio funcional.
- [`I-08-system-registry.md`](I-08-system-registry.md): contrato de I-08. Introduce el descriptor de
  sistema y `SystemRegistry` en Application; `RackProjectStore`, la validación y `RackDesignLibrary`
  consumen el registro (mueren los switches por `RackSystemKind` y el enum paralelo `RackDesignKind`),
  preservando formatos JSON, IDs, nombres, etiquetas, fallback legacy y APIs públicas. Limitada a
  Application/Persistence + adaptación mínima del consumidor de biblioteca en UI. Fuera de alcance
  I-10 (handlers del Plugin), I-16 (DrawServices), y `RackEmbedDocument` con sus discriminadores string.
- [`I-16-refactor-draw-services.md`](I-16-refactor-draw-services.md): contrato del refactor de DrawServices
  del Plugin — extraer la infraestructura compartida (`RackCatalogLoader`, `BlockPlacement`), colapsar la
  orquestación de las siete fachadas en `ViewBlockDraw` y uniformar `regen`, sin cambio de comportamiento.
  Con [línea base de equivalencia](I-16-draw-services-baseline.md) y
  [validación manual en AutoCAD aprobada](I-16-autocad-validation.md). Integrada en `main` el 2026-07-21.
- [`I-10-kind-handlers.md`](I-10-kind-handlers.md): contrato de I-10. Introduce `IRackKindHandler` y el
  registro explícito `KindHandlerRegistry` en `RackCad.Plugin` (cuatro Kinds embebidos —selective, dynamic,
  cabecera, cama—, sin reflexión; `Larguero` sin handler) y migra a él RACKEDITAR, RACKBOMTOTAL y el restamp
  de copias independientes, con error visible ante Kind sin handler y sin otro cambio de comportamiento.
  `SystemRegistry` (Application, I-08) y `RackListBuilder`/RACKLISTA quedan fuera de alcance por la dirección
  de dependencias. Cierra la pista B del Plugin. Integrada en `main` el 2026-07-21.
- [`I-14-ui-controls.md`](I-14-ui-controls.md): contrato de I-14 (pista C de UI). Crea cinco controles WPF
  reutilizables en `RackCad.UI` (`SelectionMatrix`, `NumericField`, `CatalogCombo`, `RackDialogWindow` y
  `PreviewCanvas` con proyección/paleta compartidas), separando lógica pura de la vista, más el proyecto
  `tests/RackCad.UI.Tests` (net8.0-windows, runner STA propio) y su gate de CI dedicado. Los controles
  nacen con pruebas y **no** migran ninguna ventana existente (patrón strangler): sin cambio de dibujo,
  BOM ni persistencia. La adopción la harán I-15/I-20/I-21/I-22. Fuera de alcance I-15 y el rediseño visual.
- [`I-15-editor-shell.md`](I-15-editor-shell.md): contrato de I-15 (pista C de UI, sobre I-08 e I-14).
  Introduce la infraestructura del Editor Shell en `RackCad.UI/Editor/` — `RackEditorSession` (catálogo,
  identidad/GUID, recomputación coalescida y contrato de inserción/actualización), `IRackEditorModule` y
  un `EditorModuleRegistry` explícito sin reflexión — y migra el menú principal y la biblioteca a
  consumir el registro en lugar de las ~19 propiedades de payload y los handlers por sistema (mata el
  crecimiento O(N) de `RackMainMenuWindow`, hallazgos E3/E5/U1). El único consumidor del payload en el
  Plugin (`RackMenuCommands.RackCad`) lee un `RackInsertionRequest` y despacha por Kind a los mismos
  `Draw*`. Las cuatro ventanas ricas (selectivo, dinámico, cama, cabecera) **adoptan** el shell para esas
  capacidades compartidas; lo único que queda para I-20/I-21 es extraer su estado interno propio (matriz
  por fondo, `BuildSystem`, `Recompose`). Sin cambio de dibujo, BOM, GUID, persistencia ni formatos.
  Fuera de alcance Push Back (I-18), Draw Services (I-16) y ampliar I-14. Review-ready, no integrada:
  gates `autocad` y `owner-validation` abiertos (validación manual del dueño pendiente).
- [`I-19-validador-catalogos.md`](I-19-validador-catalogos.md): contrato de I-19. Añade un validador PURO en
  `RackCad.Application.Catalogs.Validation` con severidades para ids duplicados, referencias/relaciones inválidas,
  bloques/vistas faltantes y filas descartadas por rol (con aviso), más el manifiesto esperado de
  `blocks-library.dwg` (lista de bloques + parámetros + huella) y su comparación. Fuera de alcance: corrección
  automática de catálogos, tocar el DWG, Push Back, reglas de producto, logging de I-03, esquema de persistencia y
  el cableado UI/Plugin. Owner-validation aprobada (baseline `TROQUEL_TOPE` + `TARIMA_GENERICA` aceptado);
  rebasada sobre `main` vigente e integrada en `main` el 2026-07-21.
- I-13 conserva su evidencia detallada en `archive/i-13-experiment-final-4e084d2`; su promocion fue
  revalidada, autorizada e integrada en `main` el 2026-07-20.
- [`I-29-licencia-procedencia-autocad-ci.md`](I-29-licencia-procedencia-autocad-ci.md): iniciativa
  cerrada documentalmente con decision B y restricciones; no autoriza por si sola el merge de I-13.
  La [decision final del Owner](../automation/decisions/I-29.md) es la fuente canonica vigente.
- La evidencia P1-P4 se conserva en el
  [archivo de auditoria I-29](../archivo/auditorias/I-29/README.md).

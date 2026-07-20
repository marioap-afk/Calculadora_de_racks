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
- I-13 conserva su evidencia detallada en `archive/i-13-experiment-final-4e084d2`; su promocion fue
  revalidada, autorizada e integrada en `main` el 2026-07-20.
- [`I-29-licencia-procedencia-autocad-ci.md`](I-29-licencia-procedencia-autocad-ci.md): iniciativa
  cerrada documentalmente con decision B y restricciones; no autoriza por si sola el merge de I-13.

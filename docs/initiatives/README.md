# Contratos de iniciativas

`docs/ROADMAP.md` es el indice global de fases, dependencias, conflictos y estado integrado. No se
usa para guardar el estado transitorio "en curso": ese estado se deriva de ramas remotas y Pull
Requests.

`docs/AUTOMATION_PLAN.md` gobierna la seleccion automatica, los limites de concurrencia, el reclamo
atomico, los reintentos y las condiciones de detencion. Cada archivo de esta carpeta es el contrato
detallado de una iniciativa y define su alcance, contexto, validaciones y entrega.

`docs/HANDOFF.md` conserva unicamente el estado vivo del proyecto. Los contratos no copian el
historial general, conteos de pruebas ni hashes: enlazan a las fuentes correspondientes.

Reglas de ejecucion:

- una ejecucion toma como maximo una iniciativa nueva;
- una rama remota aceptada reclama la iniciativa;
- el agente trabaja en un worktree aislado y abre o actualiza un Pull Request draft;
- el agente nunca hace merge ni activa auto-merge;
- las validaciones en AutoCAD son responsabilidad del dueno y bloquean la integracion, no
  necesariamente otra iniciativa compatible;
- el estado derivado de `main`, ramas y Pull Requests prevalece sobre el campo `status` del front
  matter;
- un contrato se crea desde `TEMPLATE.md` y no inventa alcance ausente del ROADMAP.

Planes disponibles:

- `I-06-reestructura-context-packs.md`: contrato del piloto documental I-06. Sus fases aun no se
  ejecutan.

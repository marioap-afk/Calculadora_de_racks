# Encargo: versionar I-64 A-14 (v2.3) en una rama lateral y preparar la revisión del Architect

## Rutas

- **W** (worktree de I-64, rama `architecture/workspace-persistente-rackcad`): `<worktree-de-I-64>`
- **S**: `<carpeta-de-trabajo-de-A-14>`
- **H** (herramientas de la sesión, solo lectura): `<herramientas-de-la-sesion>`

## Decisiones que lo autorizan (literales en `S/owner-a14-decisions.md`, cuarta ronda)

- **Q-C1:** «Rama lateral, como A-12/A-13 (Recomendado)». La rama sale de la de I-64, con la enmienda en
  `docs/initiatives/I-64-A-14.md` y las fuentes en la evidencia de I-64. No toca la rama de F4B mientras el Smoke-PB sigue abierto;
  se une después del cierre de F4B.
- **Q-C2:** «gpt-6-luna, el acreditado (Recomendado)». La celda es binario `4F91AE86…`, configuración Snapshot L (F4B-E57).

## Reglas duras

- **W no se toca:** ni checkout, ni commit, ni stash, ni reset, ni escritura alguna. En W hay un AutoCAD de la sesión cargando
  `src/RackCad.Plugin/bin/Debug/...` para un smoke en curso; nada puede tocar `bin/` ni `obj/`. Nunca toques `main` ni el checkout
  principal `<checkout-principal>`. Usa siempre `git -C <ruta>`.
- **Rama lateral** `architecture/workspace-persistente-rackcad-a14`, creada desde el HEAD actual de la rama de I-64 (`fa59bd82…`;
  compruébalo con `git -C <W> rev-parse HEAD`). Trabaja en un worktree NUEVO de ruta corta, `<worktree-de-A-14>`, que no exista. Si existe,
  STOP y dilo. Nada en el scratchpad (MAX_PATH).
- **Repo PÚBLICO:** ningún archivo versionado puede llevar rutas locales (`C:\Users\…`, `D:\…`, `AppData`, etc.), nombres de
  personas, correos ni datos de clientes. Antes del commit, busca y redacta (`<datos-locales-del-usuario>` u otro marcador neutro).
  El prototipo HTML trae rutas de disco de ejemplo en su sección Configuración: redáctalas.
- **Commit:** mensaje en español y terminado EXACTAMENTE con `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`; ningún otro
  co-autor. Sin `--no-verify`. Push SOLO de la rama lateral (`git -C <worktree-de-A-14> push origin architecture/workspace-persistente-rackcad-a14`),
  sin force.
- **Python:** solo con Write + `python -I script.py`; nunca `python -c`, `python -` ni heredoc.

## Qué versionar

1. `docs/initiatives/I-64-A-14.md`: el contenido de `S/I-64-A-14-draft-v2.3.md` con estos ajustes:
   - quita lo que sea de borrador («borrador v2.3» en el título, si lo hay);
   - en la cabecera, el estado PROPOSED / NOT ACTIVE;
   - anota las decisiones del Coordinator Q-C1 y Q-C2 con su fuente.

   No cambies nada de fondo. Si algo de fondo lo exige, STOP y dilo.
2. **Fuentes y revisiones**, en `docs/automation/evidence/I-64-pilot/A-14/`. Mira cómo se custodiaron A-12/A-13 y F4B-DRAFTS y
   sigue ese patrón:
   - `owner-a14-decisions.md`;
   - `replaneacion-i64.html`;
   - el prototipo (`S/../proto-rackcad/project/`);
   - los encargos (`a14-*.md`);
   - las revisiones (`independent-review-a14-*.md`);
   - las disposiciones;
   - `a14-drafting-notes.md`;
   - las versiones intermedias v1..v2.2, como historia.

   Todo pasa por la redacción de privacidad.
3. **Una sección nueva al final de `docs/automation/evidence/I-64-evidence.md`:** A-14 versionada en rama lateral (PROPOSED / NOT
   ACTIVE). Incluye la cadena de revisión (v1 → v2.3 con los veredictos), las decisiones del Owner y del Coordinator, y lo que sigue.
   Copia el estilo de las §93/§94.

## Después del push

- **CI:** espera a la CI del commit exacto con `gh run list --branch architecture/workspace-persistente-rackcad-a14` y `gh run watch`,
  sin bucles de sondeo cortos, y apunta el resultado.
- **No lances al Architect.** Prepara su orden y su encargo:
  - Busca en W cómo se hizo la revisión formal de A-13: `docs/automation/evidence/I-64-pilot/A-13-review/R20261110T022847Z-bc2b/`,
    con su `prompt.md`, su esquema, su `review-record.json` y `custody-manifest.json`.
  - Busca también la orden del Coordinator de A-7 (`F1-GATE/A-7/coordinator-a7-review-order.md`).
  - Busca en H el script o la orden exacta con que se lanzó codex-cli para esas revisiones y para las planificaciones de F4B con
    gpt-6-luna. Busca por `codex`, `gpt-6-luna`, `4F91AE86` y `Snapshot L`.
- **Escribe en S** (no en la rama todavía):
  - `a14-coordinator-review-order.md`: la orden del Coordinator para UNA revisión formal del objeto exacto (el commit de la rama
    lateral), con la celda gpt-6-luna/high/read-only, el binario `4F91AE86…`, Snapshot L y STOP si la elegibilidad cambió;
  - `a14-architect-prompt.md`: el encargo, con las preguntas que la v2.3 pide al Architect (§ revisión; la composición de
    «Actualizar todo», la ruta del rectángulo de ventana, `Applies-to` y A-7, la regla de lectura de «pestaña», la vida del borrador
    limpio…);
  - `a14-architect-launch.md`: la orden exacta para lanzarlo, copiada del precedente, con lo que hay que custodiar.

## Entrega

Tu respuesta final, breve:
- el SHA del commit, la rama y el worktree;
- la lista de archivos versionados;
- las redacciones hechas;
- el resultado de la CI;
- las rutas de los tres archivos de S;
- cualquier STOP o duda.

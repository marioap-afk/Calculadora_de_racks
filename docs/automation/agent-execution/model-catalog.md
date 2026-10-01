# Catálogo de modelos — NO NORMATIVO

Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del dominio y el conflicto se
eleva como STOP.

Catálogo **mutable y NO NORMATIVO** respecto de los nombres de modelo. [routing.md](routing.md) decide por clase, effort semántico y nivel; este archivo solo traduce eso a modelos y
registra el estado local de cada celda. Cualquier sesión responsable puede actualizar una entrada con su fuente (oficial o medición), su tipo y su fecha, sin Freeze (Proposal V9 §15);
el cambio no altera la elegibilidad de una delegación ya aceptada.

**Tipos de fuente:** `oficial` (documentación del proveedor, con URL y fecha); `caché local` (archivos locales de una herramienta; nunca hace elegible una celda); `medido` (invocación
real registrada en la evidencia de una unidad). El **nivel** (Eficiente, Equilibrado, Frontera) es una asignación de RackCad hecha sobre la descripción oficial. Fuentes de esta versión:
[Discovery de I-61](../../initiatives/I-61-discovery.md) §§12-13 y §17, y la [evidencia de I-61](../evidence/I-61-evidence.md) §§10-13.

**Frescura:** `STALE` = mín(fecha de verificación + 90 días, retiro anunciado). Un retiro con precisión de mes o trimestre cuenta desde el primer día de ese periodo.

## Anthropic (subagentes de la sesión de escritorio)

Autenticación de la sesión: OAuth de una cuenta de claude.ai, sin clave de API ni proveedor de nube (`medido`, evidencia §12). Tipo de plan: UNKNOWN. El CLI `claude` no está
autenticado (`medido`) y no se usa como transporte.

### claude-fable-5-1 (Anthropic)

- Fuente: `https://platform.claude.com/docs/en/models/overview` y `https://platform.claude.com/docs/en/build-with-claude/effort`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, effort por defecto `high`, retiro); asignación de RackCad (nivel)
- Nivel: Frontera
- Fortalezas y debilidades (oficial): contexto de 1M tokens y effort por defecto `high`; la descripción detallada no está registrada en la evidencia de I-61
- Perfiles recomendados (asignación de RackCad): ARCHITECTURE_REVIEW, LONG_HORIZON_IMPLEMENTATION
- Effort: Routine → `low`; Balanced → `medium`; Deep → `high`; Long-horizon → `xhigh`; Maximum → `max` (cada valor, solo tras medirlo en la celda)
- Consumo cubierto: UNKNOWN (sin invocación medida)
- Retiro anunciado: ninguno anterior a 2027 publicado
- Estado local: subagente × `read` — no medido; subagente × `write-commit-push` — no medido

### claude-opus-5-5 (Anthropic)

- Fuente: `https://platform.claude.com/docs/en/models/overview` y `https://platform.claude.com/docs/en/build-with-claude/effort`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, effort por defecto `medium`, retiro); asignación de RackCad (nivel); medido (estado local)
- Nivel: Frontera
- Fortalezas y debilidades (oficial): contexto de 1M tokens; effort por defecto `medium`; la fuente de effort recomienda `xhigh` para agentes de larga duración
- Perfiles recomendados (asignación de RackCad): ARCHITECTURE_REVIEW, LONG_HORIZON_IMPLEMENTATION, DEBUGGING con causa incierta
- Effort: Routine → `low`; Balanced → `medium`; Deep → `high`; Long-horizon → `xhigh`; Maximum → `max` (medidos: `high` y `xhigh`)
- Consumo cubierto: sí solo para la celda subagente con modelo heredado (medido: las invocaciones de subagentes de revisión de I-61 completaron con la autenticación por suscripción, sin aviso de límite; evidencia §13); petición explícita por alias: UNKNOWN hasta U-04
- Retiro anunciado: ninguno anterior a 2027 publicado
- Estado local: subagente × `read`/`tool-use` — medido (2026-09-30, modelo heredado de la sesión; effort solicitado `high` aplicado, sin solicitar se hereda `xhigh`); subagente ×
  `write-commit-push` — no medido; petición explícita de este modelo — no medida

### claude-sonnet-5-5 (Anthropic)

- Fuente: `https://platform.claude.com/docs/en/models/overview` y `https://platform.claude.com/docs/en/build-with-claude/effort`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, effort por defecto `high`, retiro); asignación de RackCad (nivel)
- Nivel: Equilibrado
- Fortalezas y debilidades (oficial): contexto de 1M tokens y effort por defecto `high`; la descripción detallada no está registrada en la evidencia de I-61
- Perfiles recomendados (asignación de RackCad): ROUTINE_IMPLEMENTATION, DEBUGGING, CHARACTERIZATION
- Effort: Routine → `low`; Balanced → `medium`; Deep → `high`; Long-horizon → `xhigh`; Maximum → `max` (medidos: `medium` y `high`)
- Consumo cubierto: sí para la celda subagente (medido: sonda U-04 con la autenticación por suscripción, sin aviso de límite; evidencia §15)
- Retiro anunciado: ninguno anterior a 2027 publicado
- Estado local: subagente × `read`/`tool-use`/`write-commit-push` × effort `medium` y `high` — **medido en la sonda U-04** (2026-10-01 UTC): modelo y effort efectivos
  `claude-sonnet-5-5`/`medium` y `claude-sonnet-5-5`/`high` en las transcripciones, pedidos con el alias `sonnet`; escritura, commit con trailer y push comprobados contra Git
  (evidencia §15). Usada como Worker real en el piloto de G3 con effort `high` (ROUTINE_IMPLEMENTATION): entrega verificada, 12 min 44 s, sin aviso de límite (evidencia §15.4)

### claude-haiku-4-5-20251001 (Anthropic)

- Fuente: `https://platform.claude.com/docs/en/models/overview`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, sin control de effort, retiro); asignación de RackCad (nivel)
- Nivel: Eficiente
- Fortalezas y debilidades (oficial): contexto de 200K tokens y sin control de effort; la descripción detallada no está registrada en la evidencia de I-61
- Perfiles recomendados (asignación de RackCad): DOCUMENTATION y ROUTINE_IMPLEMENTATION mientras no esté `STALE`
- Effort: no tiene control de effort; en una celda de subagente se registra el effort heredado medido
- Consumo cubierto: UNKNOWN (sin invocación medida)
- Retiro anunciado: octubre de 2026 (`STALE` desde el 2026-10-01)
- Estado local: subagente × `read` — no medido; subagente × `write-commit-push` — no medido

## OpenAI (Codex CLI, solo lectura)

Autenticación: «Logged in using ChatGPT» (`medido`, Discovery §12). Los valores de effort de configuración proceden de la referencia oficial de configuración (evidencia §12), que
advierte que dependen del modelo y del cliente; la página de modelos usa los rótulos Light…Ultra sin correspondencia explícita. Cada valor se mide antes de usarlo.

### gpt-6.1-sol (OpenAI)

- Fuente: `https://learn.chatgpt.com/docs/models` y `https://learn.chatgpt.com/docs/config-file/config-reference`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, effort de Light a Ultra; «Max and Ultra depend on your settings»); asignación de RackCad (nivel)
- Nivel: Equilibrado
- Fortalezas y debilidades (oficial): effort de Light a Ultra («Max and Ultra depend on your settings»); sin más descripción registrada
- Perfiles recomendados (asignación de RackCad): CONTROLLER_PLANNING y CONTROLLER_VERIFICATION como alternativa de nivel Equilibrado; DEBUGGING
- Effort: Routine → `low`; Balanced → `medium`; Deep → `high`; Long-horizon → `xhigh`; Maximum → `max` (cada valor, solo tras medirlo en la celda)
- Consumo cubierto: UNKNOWN (sin invocación medida)
- Retiro anunciado: ninguno publicado
- Estado local: CLI × `read`/`tool-use` — no medido

### gpt-6-luna (OpenAI)

- Fuente: `https://learn.chatgpt.com/docs/models` y `https://learn.chatgpt.com/docs/config-file/config-reference`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción, effort hasta Max, sin Ultra); asignación de RackCad (nivel); medido (estado local)
- Nivel: Eficiente
- Fortalezas y debilidades (oficial): effort hasta Max, sin Ultra; la guía sugiere empezar con High
- Perfiles recomendados (asignación de RackCad): CONTROLLER_PLANNING y CONTROLLER_VERIFICATION (Balanced como Eficiente con más effort)
- Effort: Routine → `low`; Balanced → `high` (Eficiente con más effort); Deep → `xhigh`; Maximum → `max` (medido: `high`; los demás, solo tras medirlos en la celda)
- Consumo cubierto: sí (medido: seis invocaciones de G1-C y la sonda PR-1, con la autenticación de ChatGPT y sin aviso de límite; evidencia §§10 y 14)
- Retiro anunciado: ninguno publicado
- Estado local: CLI × `read`/`tool-use` × effort `high` — **medido en la sonda PR-1** (2026-10-01 UTC): modelo y effort efectivos `gpt-6-luna`/`high` en el registro de sesión,
  `--output-schema` aceptado y oráculo contra Git correcto (evidencia §14). Es la celda del Controller (perfiles CONTROLLER_*, effort Balanced). En G3, siete invocaciones reales
  (planificación, tres verificaciones y nc1-nc3) con los esquemas de delegación y de verificación, sin aviso de límite. Dos verificaciones emitieron paradas conservadoras
  injustificadas antes del VERIFIED (evidencia §15.6)

### gpt-6-astra (OpenAI)

- Fuente: `https://learn.chatgpt.com/docs/models`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (descripción «most capable», filas de disponibilidad «ChatGPT Credits» y «API Access»); asignación de RackCad (nivel)
- Nivel: Frontera
- Fortalezas y debilidades (oficial): «Our most capable model for complex work»; la guía sugiere empezar con Light
- Perfiles recomendados (asignación de RackCad): ninguno mientras no sea elegible
- Effort: sin correspondencia medida
- Consumo cubierto: UNKNOWN; marcado por la fuente oficial como de créditos o API, así que **no es elegible ni se sondea**
- Retiro anunciado: ninguno publicado
- Estado local: no medido

### gpt-5.5 (OpenAI)

- Fuente: `https://learn.chatgpt.com/docs/models`
- Fecha de verificación: 2026-09-30
- Tipo de fuente: oficial (retiro de ChatGPT y Codex)
- Nivel: sin asignar
- Fortalezas y debilidades (oficial): sin descripción registrada; anunciado su retiro
- Perfiles recomendados (asignación de RackCad): ninguno
- Effort: sin correspondencia medida
- Consumo cubierto: UNKNOWN
- Retiro anunciado: 2026-10-14
- Estado local: no medido

**Solo en caché local (no elegibles, no se sondean como oficiales):** otros identificadores de la caché de modelos de Codex que la página oficial no recomienda (Discovery §13), entre
ellos el modelo configurado por defecto en este equipo. Se reverifican con la fuente oficial antes de añadirlos como entrada.

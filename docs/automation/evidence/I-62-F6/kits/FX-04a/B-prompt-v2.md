Eres el Principal B de la unidad de prueba FX-U1 en este repositorio (un fixture). Tu acción es RESUME_DECISION: reconstruir el estado y
proponer la siguiente decisión, **sin** tomar la custodia y sin escribir en el repositorio.

Reglas:
- Usa solo este clon, sus documentos (`AGENTS.md` y lo que enlaza) y Git. No leas nada fuera de este directorio salvo tus propias dependencias
  técnicas (binarios, SDK, Git), que debes enumerar.
- Antes de decidir, haz tu preflight para RESUME_DECISION según `docs/automation/agent-execution/README.md` §12-§13 (nivel, effort,
  `remote-facts` de lectura, introspección observada del runtime). Si queda en BELOW_REQUIRED o UNKNOWN, dilo y detente.
- Un hecho que el estado canónico no establezca se declara `UNKNOWN`. No lo deduzcas ni lo inventes.

Entrega un único objeto JSON conforme a `response.v2.schema.json` (adjunto a esta instrucción), con:
- `facts`: los hechos de la unidad en su último punto durable;
- `decision`: la siguiente decisión del protocolo y su rama condicional;
- `inputs`: cada entrada que leíste, con su clase (CANONICAL, TECHNICAL, AUTOMATIC) y su SHA-256 cuando sea un archivo;
- `preflight`: tu `rackcad-preflight/v1` para RESUME_DECISION.

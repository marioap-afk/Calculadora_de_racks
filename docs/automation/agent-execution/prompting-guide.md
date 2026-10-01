# Guía de prompting para la ejecución delegada

Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del dominio y el conflicto se
eleva como STOP.

Cómo escribir el **delta** de un prompt delegado. El contrato base y los perfiles por clase de tarea están en [PROMPT_TEMPLATES §G](../../initiatives/PROMPT_TEMPLATES.md); el prompt
compuesto es contrato base + perfil + delta, no supera 200 líneas y se guarda como `prompt.md` con su SHA-256.

## Fuentes

Consultadas el 2026-09-30; son guía del proveedor, no autoridad de RackCad ([Discovery de I-61](../../initiatives/I-61-discovery.md) §13, evidencia §12).

| Fuente | Proveedor | Generación cubierta |
|---|---|---|
| `https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices` | Anthropic | Fable 5.1 a Haiku 4.5 |
| `https://platform.claude.com/docs/en/build-with-claude/effort` | Anthropic | generación 4.5 a 5.5 |
| `https://learn.chatgpt.com/guides/best-practices` | OpenAI | Codex |
| `https://learn.chatgpt.com/docs/developer-commands?surface=cli` | OpenAI | Codex CLI |

## Reglas

1. **Encuadre completo:** objetivo, contexto, restricciones y criterio de terminado, en ese orden. El criterio de terminado es observable (pruebas, archivos, SHA), nunca «cuando esté
   bien».
2. **Rol y éxito explícitos:** quién es el participante (Worker o Controller) y qué declara al terminar (los estados de su esquema).
3. **Etiquetas para separar** instrucciones y datos cuando el prompt incluye contenido leído de archivos.
4. **Herramientas como acción:** «ejecuta `dotnet test …` y registra la selección», no «podrías probar».
5. **Sin sobreingeniería ni reinvestigación:** se citan el Discovery, el Freeze y la entrega previa por ruta y sección; el participante no rehace lo ya establecido ni añade alcance.
6. **Horizonte largo:** estado en archivos del área transitoria y progreso incremental verificable.
7. **STOP explícito:** las condiciones de parada de la delegación aparecen por id, con la instrucción de devolver el bloqueo estructurado en la entrega.
8. **Autolocalización:** las autoridades se citan por ruta, sección y cláusula; no se copian.
9. **Vigencia leída de Git:** el estado del workflow y de las normas se lee de la revisión que fija la delegación (`AuthorityRevision` y `MainSha`), no de prosa que pueda estar
   desactualizada.
10. **Ningún secreto** en el prompt, en el delta ni en los archivos que el participante vaya a leer.

## Delta por perfil

El delta son los campos del paquete de delegación, renderizados. Lo que añade cada perfil, y su tope de líneas, está en PROMPT_TEMPLATES §G. Para el effort: subirlo solo cuando la
clase y las dimensiones lo justifican (routing.md); el effort influye en todos los tokens, incluidas las llamadas a herramientas, y un effort bajo suele bastar para subagentes acotados.

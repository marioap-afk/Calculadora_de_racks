# Context Packs

Los Context Packs son manifiestos ligeros para cargar solo el contexto técnico relevante a una
iniciativa. No sustituyen las fuentes globales ni copian capítulos completos.

## Base obligatoria

Antes de aplicar packs, una ejecución lee las fuentes globales exigidas por
[AUTOMATION_PLAN.md](../AUTOMATION_PLAN.md): AGENTS, WORKFLOW, ROADMAP, el plan de automatización y el
contrato detallado de la iniciativa. El campo `context_packs` del contrato agrega cero o más IDs.

## Contrato

Cada manifiesto declara:

- `id` estable;
- cuándo cargarlo;
- documentos requeridos y opcionales;
- globs de código que delimitan la inspección inicial;
- exclusiones para evitar ampliar alcance;
- una sección breve de invariantes esenciales.

Los globs orientan la lectura; no conceden permiso para editar todos los archivos encontrados. El
alcance sigue definido por el contrato de iniciativa.

## Taxonomía aprobada

| ID | Tema |
|---|---|
| [architecture-kernel](architecture-kernel.md) | Capas, contratos compartidos y registros |
| [catalogs-data](catalogs-data.md) | Catálogos, FKs, plantillas y bloques |
| [persistence](persistence.md) | DTO, schemas, stores, identidad y legacy |
| [ui-editors](ui-editors.md) | WPF, estados de editor, controles y previews |
| [autocad-plugin](autocad-plugin.md) | Comandos, transacciones, dibujo y Xrecords |
| [system-selective](system-selective.md) | Selectivo, seguridad, cotas y BOM |
| [system-dynamic-flowbed](system-dynamic-flowbed.md) | Dinámico y cama de rodamiento |
| [delivery-validation](delivery-validation.md) | Build, CI, despliegue y validación manual |
| [documentation-governance](documentation-governance.md) | Fuentes documentales, ADRs e iniciativas |

Una iniciativa puede combinar varios packs. No se crea un pack nuevo para una sola tarea mientras no
exista conocimiento estable que justifique mantenerlo.

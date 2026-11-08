# I-64 — P-01: aceptación del Owner de la configuración 377D0C3C (vía B), literal

Recibida el 2026-11-08 en el chat de la sesión responsable, como respuesta a una pregunta de la sesión (herramienta de preguntas
del chat). La respuesta llegó antes de la medición de las 2026-11-08T06:57:42Z, que dio el mismo SHA-256 que el inventario.

**Pregunta (literal):**

```text
P-01: config.toml sigue en 377D0C3C (104 nombres; sin cambios desde ayer a las 18:09:47Z). Ya capturé el inventario ordenado completo, sin valores. Frente a la instantánea A cambian solo los nombres de la vista previa; los otros 101, incluidos los 12 [projects.*], son idénticos y en el mismo orden. ¿Cómo lo resolvemos?
```

**Opción elegida por el Owner (literal):** `Acepto 377D0C3C (vía B)`, con la descripción:

```text
Aceptas esta configuración exacta como instantánea nueva (SHA 377D0C3C, lista de nombres EB8775C5). Si la app vuelve a escribir el archivo, P-01 reaparece.
```

**Vista previa que el Owner tenía delante al elegir (literal):**

```text
config.toml   SHA-256 377D0C3C…D1E8   4887 bytes   104 nombres
última escritura 2026-11-07 18:09:47Z
lista ordenada de nombres   SHA-256 EB8775C5…F820

Frente a la instantánea A (105 nombres):
  [desktop]
    sale   followUpQueueMode           (A, posición 10)
    entra  migratedFollowUpQueueToSteer
  sale la tabla
    [plugins."code-review@openai-bundled"]
    con su  enabled                    (A, posiciones 52-53)
  [mcp_servers.node_repl.env]
    entra  NODE_REPL_INSTRUCTIONS_USE_CASE_COMPUTER_USE

Los otros 101 nombres, con los 12 [projects.*],
son idénticos a A y en el mismo orden.

Inventario completo (solo nombres, rutas redactadas):
artifacts\orchestration\I-64\P01-CONFIG\
  C20261108T065112Z\config-inventory.txt
```

Las otras opciones eran «La restauro yo (vía A)» y «Dejarlo pendiente».

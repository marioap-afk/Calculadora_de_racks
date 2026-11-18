# I-64 F4B PB-T1b: vuelta al Owner por la parada P-01 (el config.toml de Codex cambió tras el reinicio)

**Fecha:** 2026-11-18. La prepara la sesión principal.

## Los hechos

- **Al preparar la reanudación del Worker de PB-T1b** (después del reinicio del equipo que pidió el Owner), la comprobación de salida dio `config.toml` = `57B0FED6…`. No es la Snapshot I aceptada (`0B47701A…`, F4-E57).
- **La parada:** es P-01, la parada estándar del contrato. **No se ha lanzado ningún Worker ni ninguna corrida de Codex.**
- **Qué cambió:**
  - el archivo se reescribió a las 19:17:19Z, justo después del reinicio;
  - tiene los mismos 104 nombres, en el mismo orden;
  - **el inventario semántico seguro** (el método de la Snapshot I, con 68 valores) es **idéntico** al de la I;
  - la única clave cuyo valor ese inventario oculta es `mcp_servers.node_repl.env.SKY_CUA_NATIVE_PIPE_DIRECTORY`, el directorio opaco de la tubería del servidor de computer use que la app regenera. El cambio está ahí o en el formato.
- **La app de Codex** se actualizó con el reinicio: de `26.1111.21308` (Snapshot I) a `26.1111.2228.0`. **El binario de la CLI acreditado no cambió** (`5449c1f0`, SHA-256 `4F91AE86…`).
- **El precedente:** es el mismo tipo de cambio que el paso de H a I (F4-E57). Allí el Owner decidió «Sonda neutra, luego I».

## Opciones

- **(A) Sonda neutra y luego J.** Es el procedimiento de la I:
  - se ejecuta la sonda neutra con el binario acreditado;
  - si da Pass, el `config.toml` actual pasa a ser la Snapshot J, la línea base activa, y se reanuda PB-T1b;
  - si no da Pass, se vuelve al Owner.
- **(B) Aceptar J sin sonda.** El inventario seguro es idéntico y el único valor distinto posible es el directorio opaco de la tubería.
- **(C) Parar** hasta revisarlo.

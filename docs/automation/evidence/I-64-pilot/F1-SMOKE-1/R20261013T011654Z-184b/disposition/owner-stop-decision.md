# I-64 Smoke-1 — R20261013T011654Z-184b: STOP por decisión del Owner/Coordinator

## Hallazgo (dictado por el Owner, literal)

Tras SM-02 (`(getvar "DBMOD")` → 0 en el dibujo A) y `RACKPANEL` (SM-03):

```text
RackCAD Panel se mueve de una manera muy extraña, traté de mover la ventana pero se movía mucho, volví a intentar y se fue fuera de la pantalla
```

Y después: «No alcancé a ver casi nada del panel.»

## Lectura de la sesión (código del Candidato `4d79e910`)

- `WorkspacePaletteHost.Show()` solo hace `_palette.Visible = true`: `RACKPANEL` no recoloca la paleta, así que repetirlo no la trae
  de vuelta.
- La paleta se crea como `new PaletteSet("RackCad")`, sin GUID, sin tamaño ni posición inicial; acoplable a izquierda y derecha, con
  `KeepFocus = true`.
- Ningún paso escrito de T3D recupera una paleta fuera de pantalla. La autorización prohíbe «recovery actions beyond the written
  procedure».

## Opciones presentadas y decisión

- A. Una sola acción autorizada como desviación: arrastrar una parte visible de la paleta al borde izquierdo para acoplarla (la
  primera acción escrita de SM-17, adelantada), y seguir desde SM-03.
- B. STOP: cerrar sin guardar, POST, DIFF, devolver el host a I-52 y volver al Coordinator.

**Decisión del Owner/Coordinator (literal):** «No me quiero desviar» → **STOP** (opción B).

## Efecto

- La secuencia funcional se detiene en SM-03. Ningún paso posterior se ejecuta.
- AutoCAD se cierra sin guardar; POST (T3C §4) en cuanto `acad.exe` = 0; DIFF con `-PaletteKeyAcceptance ACCEPTED`.
- Smoke-1 no puede ser PASS en esta ocurrencia. Una nueva ocurrencia exige otra decisión del Coordinator y un RunId nuevo.
- El comportamiento de la paleta flotante queda como hallazgo de producto o de host para el Coordinator (D-01, «acoplable y
  flotante»; S1-01).

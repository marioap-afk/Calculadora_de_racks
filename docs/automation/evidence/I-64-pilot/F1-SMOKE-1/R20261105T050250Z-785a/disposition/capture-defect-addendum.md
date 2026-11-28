# R20261105T050250Z-785a — Adenda: cuatro capturas tapadas (hallazgo de la sesión antes de la custodia, 2026-11-05 ~21:28Z)

AutoCAD operated by implementation session under Coordinator OPERATOR_EXCEPTION for this occurrence.

Esta adenda NO cambia el registro firmado, ni ningún resultado de SM, ni la disposición NOT PASS. Corrige lo que la sesión dijo de las capturas.

## Hechos
Al revisar una a una las 14 capturas para la custodia, la sesión encontró que **4 no muestran lo que el registro dice que muestran**:

| Captura | Qué se ve |
|---|---|
| `operator/screens/SM-17-derecha-palette.png` | arriba, la ventana de la app de Claude (texto del chat de la sesión y su pie: modelo, «Extra», «Omitir permisos»); abajo, solo las filas de medida |
| `operator/screens/SM-26-palette.png` | igual: no se ve «Sin documento» ni las casillas |
| `operator/screens/SM-27b-palette.png` | igual: no se ve «Sin documento» ni las casillas |
| `operator/screens/SM-34-palette.png` | igual: no se ve «Hay un comando en curso.» ni las casillas |

**Causa:** `Save-ScreenRegion.ps1` copia la pantalla real. Desde SM-17 la paleta quedó acoplada a la derecha, debajo de la ventana de la app de Claude. Las imágenes de la herramienta de escritorio con las que la sesión operó y leyó el panel ocultan esa ventana, y por eso la sesión veía la paleta completa.

**Las otras 10 capturas son correctas:** SM-03 y SM-13 (paleta y línea de órdenes), SM-17-flotante, SM-17-plegada, las dos de INCIDENTE-SM-14h, y las de línea de órdenes de SM-17-derecha y de SM-34.

## Error de la sesión y lo que queda corregido
La sesión no abrió esas 4 capturas después de tomarlas. Solo había abierto tres de línea de órdenes. Por eso son incorrectas estas afirmaciones:
- en el chat: que las capturas no mostraban rutas ni datos ajenos, dicho tras abrir solo esas tres;
- en `disposition/return-package.md`, sección 5: «14 PNG […] solo la paleta y la línea de órdenes, sin datos ajenos»;
- en `operator/operator-record.md`: las notas «captura SM-26-palette», «captura SM-27b-palette», «capturas SM-34-*» y «SM-17-derecha-*» citan archivos que existen, pero en los de paleta falta la parte alta.

El Coordinator y el Owner decidieron y firmaron sin conocer este defecto. Los archivos originales no se tocan.

## Evidencia de esos pasos
Los resultados del registro se basan en lo que la sesión leyó en las imágenes de la herramienta, tomadas en el momento. Esas imágenes se conservan. Solo se extrae la región de la paleta (`operator/screens-tool/`, con `derivation-record.txt`):

| Paso | Imagen | Guardada (UTC) | Muestra |
|---|---|---|---|
| SM-17, acoplada a la derecha | `tool-SM-17-derecha-palette-crop.png` | 19:23:27 | «Sesión 1», casillas marcadas, 69 puntos |
| SM-26 | `tool-SM-26-palette-crop.png` | 19:31:29 | «Sin documento», «Sin sesión activa», casillas desmarcadas, «No hay un documento abierto.», 99 puntos |
| SM-27b | `tool-SM-27b-palette-zoom.jpg` | 19:32:36 | «Sin documento», «Sin sesión activa», casillas desmarcadas, 101 puntos |
| SM-34 | `tool-SM-34-palette-crop.png` | 19:43:51 | «Sesión 8», casillas desmarcadas, «Hay un comando en curso.», 135 puntos |
| SM-34, 5 s después | `tool-SM-34-palette-zoom.jpg` | 19:44:29 | lo mismo, 135 puntos |

Las pantallas completas de origen no se copian (la página Start de SM-26 muestra nombres de proyectos ajenos); su SHA-256 está en `derivation-record.txt`.

**Limitación:** estas imágenes pasan por el filtro de la herramienta, que oculta las ventanas sin permiso; el contenido de AutoCAD es el de la pantalla. No son las capturas de `Save-ScreenRegion` previstas en la guía.

## Decisión que se pide al Coordinator
1. **Cómo custodiar las 4 capturas tapadas.**
   - **A (propuesta de la sesión):** excluirlas del árbol versionado, con su SHA-256 y el motivo en el manifiesto. Muestran la ventana de la sesión y no aportan evidencia; publicar se puede hacer después, y retirar no. Se custodian `operator/screens-tool/` y esta adenda.
   - **B:** custodiarlas tal como salieron, además de lo anterior.
2. **Si las imágenes de la herramienta valen como captura de SM-26, SM-27b y SM-34 para esta ocurrencia**, o si esas capturas quedan pendientes para la ocurrencia que complete Smoke-1. La matriz de requisitos que pide la revisión final depende de esto.

Ensayo de la custodia con la opción A, sin escribir nada: `R20261105T050250Z-785a` 83 CUSTODIED / 166 EXCLUDED; `prep` 13 CUSTODIED / 7 EXCLUDED; sin patrones de ruta local.

**Para la siguiente ocurrencia:** abrir cada captura justo después de tomarla y apartar antes la ventana de la app de Claude.

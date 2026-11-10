# I-68 — Ficha de Owner Validation (ID72, BOM Excel)

Matriz: [Proposal V6 §12](I-68-proposal-v6.md). Procedimiento general: [guía manual](../guias/validacion-manual-autocad.md).

## Identidad (la toma la sesión; el Owner no la mide)

```text
FINAL_CANDIDATE_SHA: 68d9fb9a24151871cee1a7a22c0a2835882465fb
Worktree:            C:\Users\alejandra-mendoza\.codex\worktrees\feature-bom-excel-analisis-nativo
DLL Debug:           src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
ProductVersion:      1.0.0+68d9fb9a24151871cee1a7a22c0a2835882465fb
SHA-256 del DLL:     85EF5D7535A8CDAD766C52E4ADF223E0F1B7DDA2F7246FC47891E3C4D745DC4C
```

La versión de AutoCAD y la biblioteca de bloques las registra la sesión al abrir la ronda.

## Preparación

1. Cierra AutoCAD, ábrelo y carga el DLL con `NETLOAD`.
2. Abre un dibujo con varios racks: al menos un sistema con componentes (Selectivo, Dinámico, Push Back o Cantilever) y, si lo tienes, una Cama o Cabecera suelta. Copia uno de los racks con `COPY` para que tenga Copias mayor que 1. Conviene que se repita alguna descripción con longitudes distintas.
3. Ejecuta `RACKBOMTOTAL`, pulsa **Exportar Excel** y guarda el archivo. Ábrelo con doble clic.

## Escenarios

| ID | Qué hacer | Qué debe verse | Resultado |
|---|---|---|---|
| OV-1 | En `Por rack`, revisa `Cantidad total` en una fila con Copias mayor que 1 | Es Cantidad × Copias, y es un número | |
| OV-2 | Usa los filtros de encabezado de `Por rack` | Es una tabla de Excel con filtros y filas alternadas | |
| OV-3 | Abre `Resumen` y haz clic en la tabla dinámica | Filtros Rack y Nivel, filas Descripcion y Longitud, valor Suma de Cantidad total. Arriba a la derecha, la advertencia de que con (Todas) la suma no es necesariamente un total físico | |
| OV-4 | Filtra Rack por uno solo | Solo ese rack; también funciona con varios | |
| OV-5 | Mira cómo abre el filtro Nivel; elige Componente y luego Pieza | Abre en (Todas); después filtra cada valor | |
| OV-6 | Combina un Rack y un Nivel | Solo lo que cumple ambos | |
| OV-7 | Compara un total del resumen con la suma de `Cantidad total` filtrada en `Por rack` | Coinciden. Para comparar con `Total del dibujo`, hazlo por categoría y longitud: esa hoja une piezas con distinta descripción | |
| OV-8 | En `Por rack` cambia un valor de la columna `Cantidad total` y pulsa Datos > Actualizar todo. Después cambia solo Copias o Cantidad y actualiza | El resumen cambia con lo primero y no con lo segundo: Cantidad total es un valor, no una fórmula | |
| OV-9 | Observa la apertura con doble clic | Sin aviso de reparación ni «registros eliminados» | |
| OV-10 | En Excel: Archivo > Abrir, elige el archivo y, en la flecha del botón Abrir, **Abrir en Vista protegida**. Mira `Resumen` y luego pulsa Habilitar edición | El resumen se ve; al habilitar sigue correcto y sus filtros tienen valores | |
| OV-11 | Si puedes provocar un BOM sin filas, expórtalo | Abre sin reparación. Si no se puede provocar, anótalo | |
| OV-12 | Exporta al menos cuatro veces con nombres distintos y abre los cuatro seguidos en el mismo Excel. En cada uno mira si el filtro Rack tiene valores, esperando hasta 15 segundos | Tienen valores. Falla si siguen vacíos después de Datos > Actualizar todo. Anota si alguno tardó | |

Además, el checklist aplicable de la guía manual §5 sobre `RACKBOMTOTAL`.

## Veredicto

```text
Owner Validation: PASS (2026-11-09)
Fallos y severidad: ninguno declarado
Observaciones: resultado global declarado por el Owner; no se dictaron resultados por escenario
AutoCAD: R25.0.171.0.0 (instalacion del equipo)
```

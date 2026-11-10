# I-64 — F3-T2-BROWSER-UI: paquete de cierre

Preparado por la sesión responsable bajo la orden del Owner I-64 F2 CLOSE RATIFICATION / F3 AUTHORIZATION. Decisiones: `I-64-pilot/F3-BROWSER/f3-dispositions.md`, F3-E08 a F3-E12.

Este paquete no declara GATE PASS ni Candidato. El cierre del gate está en `I-64-pilot/F3-BROWSER/gate-close.md`.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `EXECUTION_VERIFIED / NONE`: verificación `R20261110T170317Z-c324`, 14/14 en pass |
| VerifiedSha | `206e0f7a3fe32fde2bba70ac663a74f60ee6a4f4`, CI 42860103890 push 4/4 |
| Cadena | ChainBaseSha `8757c50b`; intento 1: RED `1c379a23`, GREEN `2c36b36b`; intento 2: BaseSha `29cf0d3e`, RED `94f35bb9` (CI 42855947104), GREEN `206e0f7a` |
| Delegaciones | `22d9` (intento 1, A1-A8 pass, nc4 `c2c6`) y `e2f9` (intento 2, A1-A8 pass) |
| Intentos | 2 de 3 |
| nc1 | `09b5` = CONTROL NOT PASSED, conservado; **SATISFIED_UNDER_I64_IDENTITY_BRIDGE** (F3-E10) |
| nc2 | **`dc56` = CONTROL PASS** |
| nc3 | **`f4c4` = CONTROL PASS** |

## 2. Corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `fc18` | EXECUTION_VERIFIED, 14/14 | intento 1, `2c36b36b`; se conserva; la revisión de producto halló T2-F1 a T2-F6 (F3-E09) |
| `c324` | EXECUTION_VERIFIED, 14/14 | intento 2, `206e0f7a` |

## 3. Pruebas sobre `206e0f7a` (TRX de la CI)

| Conjunto | Resultado |
|---|---|
| Filtro UI Workspace | 222/222 |
| Filtro Core Workspace | 902/902 |
| Core completa | 16440/16440 |
| UI completa | 1859 superadas, 17 omitidas, 0 fallidas (1876) |
| RED `94f35bb9` | UI Workspace 206 y 16 fallidas; Core Workspace 888 y 14 fallidas, por aserción |

## 4. Producto entregado

Sección «Navegador de racks» en el panel Workspace:

- Tira de pestañas por rack que se reorganiza con el ancho, con preview, «Fijar» y «Cerrar», y la pestaña activa resaltada.
- Una única superficie de resumen (K = 1) con nombre, tipo, vistas y estado, y la acción «Volver a leer».
- La navegación sigue a la pestaña activa. Cambiar de pestaña no escribe la selección ni la vista.
- Un puerto de solo lectura servido por el host de navegación existente, sin archivos nuevos en el Plugin y con la caché por (sesión, rack).

## 5. Abierto

Limitaciones R-1 a R-4 de F3-E12. El comportamiento en AutoCAD real no se ha ejercido: F3 no tiene hito de host.

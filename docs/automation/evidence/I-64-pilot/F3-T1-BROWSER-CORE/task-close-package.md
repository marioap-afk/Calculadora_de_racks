# I-64 — F3-T1-BROWSER-CORE: paquete de cierre

Preparado por la sesión responsable bajo la orden del Owner I-64 F2 CLOSE RATIFICATION / F3 AUTHORIZATION. Decisiones: `I-64-pilot/F3-BROWSER/f3-dispositions.md`.

Este paquete no declara F3 PASS, GATE PASS ni Candidato.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `EXECUTION_VERIFIED / NONE`: verificación `R20261110T143422Z-057f`, 14/14 en pass |
| VerifiedSha | `a92039bbbdc85c38c380a8a319e577d625d3910c`, CI 42833048949 push 4/4 |
| Cadena | BaseSha `b1911aa0`; RED `2bbece51`, CI 42832249016 (Core en failure, esperado); GREEN `a92039bb` |
| Delegación | `R20261110T140050Z-8317`, A1-A8 pass; nc4 `R20261110T140258Z-8f7e` |
| Worker | `R20261110T140432Z-0e2b`, claude-sonnet-5-5 / high, 57 turnos |
| Intentos | 1 de 3 |
| nc1 | `R20261110T143703Z-1839` = CONTROL PASS |
| nc2 | `R20261110T144043Z-0464` = CONTROL NOT PASSED, conservado; **SATISFIED_UNDER_I64_SCOPE_BRIDGE** (F3-E06) |
| nc3 | `R20261110T144356Z-8cf6` = CONTROL PASS |

## 2. Verificaciones de esta entrega

| Corrida | Resultado | Notas |
|---|---|---|
| `224d` | EXECUTION_BLOCKED / STOP, Contract, S-03 | se conserva (F3-E04) |
| `9cbf` | EXECUTION_BLOCKED / BLOCKED, Routing | se conserva (F3-E05) |
| `057f` | EXECUTION_VERIFIED / NONE, 14/14 | última reejecución permitida |

## 3. Pruebas sobre `a92039bb` (TRX de la CI)

| Conjunto | Resultado |
|---|---|
| Filtro Core Workspace | 855/855 |
| Core completa | 16393/16393 |
| UI completa | 1792 superadas, 17 omitidas, 0 fallidas (1809) |
| RED `2bbece51` | Core Workspace 816 superadas y 39 fallidas por aserción |

## 4. Producto entregado

Núcleo puro en `RackCad.Application.Workspace`, sin AutoCAD, WPF, eventos ni escrituras:

- `BrowserSession`: pestañas ligeras por rack, como máximo una preview, fijadas, pestaña activa determinista, cierre solo en memoria, K = 1 y resumen recibido desde fuera.
- `BrowserSessionRegistry`: una sesión de navegador por `SessionId`; una sesión destruida no resucita.
- Delta exacto en la guarda de tipos por pestaña: solo `BrowserTabRecord` y `BrowserTabStatus`.

## 5. Abierto

- **Sin consumidor todavía:** F3-T2 lo conecta a la UI.
- **RackId:** este núcleo compara RackId exactos, como `SelectionContext`; `NavigationTargets` no distingue mayúsculas. Sin efecto con los identificadores que genera RackCad.
- **Controller:** repite el defecto de `Scope` en nc2 y varía su lectura de `Routing`. Fuera del alcance de I-64.

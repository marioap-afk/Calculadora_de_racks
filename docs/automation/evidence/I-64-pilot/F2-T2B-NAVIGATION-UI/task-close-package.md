# I-64 — F2-T2B-NAVIGATION-UI: paquete de cierre

Preparado por la sesión responsable bajo la instrucción nocturna del Owner. Decisiones: `I-64-overnight/overnight-dispositions.md` D-03 a D-07.

Este paquete no declara F2 PASS, GATE PASS ni Candidato. El comportamiento en AutoCAD queda para Smoke-NAV.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `EXECUTION_VERIFIED / NONE`: verificación `R20261110T061640Z-6b2a`, 14/14 en pass |
| VerifiedSha | `82bd49b6d472ddb6ab13c1716d7a995c4d571fab`, CI 42766532521 push 4/4 |
| Cadena | ChainBaseSha `2deb0454`; RED `f64a13b3` y GREEN `6c01f644` (intento 2); BaseSha `346edb66`, RED `25bdd0d9` y GREEN `82bd49b6` (intento 3) |
| Intentos | 3 de 3 |
| nc1 | `2376` = CONTROL NOT PASSED, conservado; **SATISFIED_UNDER_I64_IDENTITY_BRIDGE** (D-06) |
| nc2 | **`07c1` = CONTROL PASS** |
| nc3 | **`4df8` = CONTROL PASS** |
| nc4 | `131c`, primera delegación de la cadena |
| A-4 / A-6 | no aplicados: no hizo falta |

## 2. Corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `R20261110T053114Z-ffd2` | EXECUTION_VERIFIED, 14/14 | intento 2, `6c01f644`; se conserva; la revisión de producto halló T2B-F1..F3 (D-04) |
| `R20261110T061255Z-92b5` | EXECUTION_BLOCKED / STOP, Authority | intento 3; se conserva (D-05) |
| `R20261110T061640Z-6b2a` | EXECUTION_VERIFIED, 14/14 | intento 3; misma entrega y mismo prompt que `92b5` |

## 3. Pruebas sobre `82bd49b6` (TRX de la CI)

| Conjunto | Resultado |
|---|---|
| Filtro UI Workspace | 154/154 |
| Filtro Core Workspace | 780/780 |
| Core completa | 16318/16318 |
| UI completa | 1791 superadas, 17 omitidas, 0 fallidas (1808) |
| RED `25bdd0d9` | UI Workspace 147 superadas y 7 fallidas por aserción; Core Workspace 780/780 |

## 4. Producto entregado

Sección de navegación del panel Workspace para el rack del contexto: Seleccionar, Ver, Centrar y Localizar vista, con copia k/M y vista explícita. La UI solo habla con `IWorkspaceNavigationPort`; `NavigationTargets` sigue siendo la única autoridad. Sin anchos fijos en los controles.

Correcciones de la revisión de producto:

- **T2B-F1:** Localizar vista avanza de copia y vuelve a la primera; Ver o Centrar tras Localizar reinician la copia.
- **T2B-F2:** una copia que ya no existe tiene su propio texto, distinto del de un rack sin objetivos.
- **T2B-F3:** el motivo de bloqueo es cierto también para un único objeto que no es rack.

## 5. Abierto

- **Controller:** nc1 volvió a marcar `Handoff` junto con `Identity`, como en F2-T2A. Y la lectura de 16.3 varió entre corridas idénticas. Ambos son defectos del Controller, no del producto. Requieren decisión del dueño del protocolo fuera de I-64.
- **Limitación conocida:** el estado `Failed` muestra el detalle del host sin traducir, tras un prefijo en español.
- **`attempts`:** 3 de 3 en la unidad.

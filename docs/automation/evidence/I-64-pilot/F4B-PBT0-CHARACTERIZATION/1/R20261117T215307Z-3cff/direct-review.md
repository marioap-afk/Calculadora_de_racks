# Revisión directa de la sesión — F4B-PBT0-CHARACTERIZATION, intento 1

Objeto: `f2eedd9a..28e38d8d` (RED `8f75a89b`, GREEN `28e38d8d`). Worker `R20261117T185616Z-3c1b`, delegación `R20261117T185020Z-e1ae`. Revisión hecha con git y lectura, sin ejecutar pruebas.

## 1. Comprobaciones mecánicas (medidas)

| Comprobación | Resultado |
|---|---|
| Rutas de `BaseSha..CurrentSha` | 10, los diez archivos nuevos de pruebas; todas en AllowedWriteScope; ninguna bajo ForbiddenWriteScope (265) |
| `git diff BaseSha CurrentSha -- src` | vacío |
| El RED en `src/` | exactamente los seis archivos de MutantScope |
| El GREEN (`RED..CurrentSha`) en `tests/` | sin cambios |
| CI del RED `44174025134` | UI Tests failure, Tests failure, Build UI success, Build Plugin skipped |
| CI del GREEN `44175320836` | 4/4 success; Core 19070/19070; UI 2404 pasadas de 2421, 0 fallidas |
| Filtros de T1B en las dos CI | Core 10/10 y UI 55/55 en el RED y en el GREEN |
| Registro de relevo del Worker | válido contra el esquema; 437 turnos `claude-sonnet-5-5`, effort high; 0 denegaciones |
| Procesos y configuración en la entrada | 0 participantes; Snapshot I; HEAD = remoto = `28e38d8d`; árbol limpio; `main` = `812baf87` |

## 2. Hallazgos

**D-1 (proceso; para decisión): la desviación c-16 de CT-17 contradice la letra del contrato, y la latitud que usó el Worker viene de MI prompt, no del contrato.**
- Hecho técnico, verificado en la base:
  - `DynamicRackSystemDocument.cs:429-432` (`ValidDimensions`) normaliza todo valor fuera de `[None, Detailed]` a `None` antes de la ventana.
  - El recorte de W:407 (`Math.Min((int)DimensionDetail.Detailed, Math.Max(0, …))`) solo ve valores válidos, de 0 a 3.
  - Con c-10 y c-11 (7 y −1), MU-PB0-16 no puede discriminar.
  - `Detailed = 3` es el único valor que distingue el recorte a Detailed del recorte a Standard (`DimensionDetail.cs`). El caso c-16 (`Dimensions = 3`) es la corrección mínima y correcta.
  - La alternativa W:387 tampoco discrimina, porque la unidad vacía cae a «kg» en los dos lados.
- **Letra de las autoridades:**
  - INV-PBT0-14 del contrato dice «un mutante que no discrimina su CT en aislado … sin alternativa valida, es defecto del contrato y STOP (PBT0-C10)».
  - El paquete dice lo mismo: §5 línea 520, §6.5 línea 664 y §6.7 línea 749.
  - **Ninguna** de las dos autoridades admite corregir la prueba.
  - La cláusula «corrige la prueba solo si no caracteriza bien el comportamiento actual que el CT describe» está solo en el paso 3 del prompt del Worker. La redactó la sesión, heredada de la cadena de T1B. El Worker siguió su prompt.
- **Lectura:**
  - El fallo es del paquete: diseñó c-10 y c-11 sobre una lectura (MB) de que los valores fuera de rango llegan a la ventana, y no es así.
  - El hecho del §2 del paquete (línea 90, «el recorte … en W:407») sigue siendo cierto como código. Por eso no veo PBT0-C07.
  - El resultado es más fuerte que el contrato, y nada del producto ni de la tabla de mutantes cambia.
  - Aun así, una parada del contrato que no se disparó no es algo que la sesión pueda dar por buena por su cuenta.
- **Propuesta:**
  - (a) [recomendada] aceptarla como desviación declarada, con c-16 registrado como corrección del caso de CT-17 y la cláusula del prompt registrada como defecto de la sesión;
  - (b) tratarla como PBT0-C10: corrección del contrato con c-16 en el intento 2/3.

**D-2 (menor; caracterización correcta): c-12 y c-13 se fijan como rechazo.**
- CT-17 captura la excepción de Abrir en estos dos casos y fija su tipo y mensaje:
  - c-12 (`PalletsDeep = 1`): el almacén lo rechaza con `InvalidOperationException`;
  - c-13 (`PalletTolerance = 0`): `ArgumentException`, desde `DynamicRackSystemResolver.cs:474`, que se llama desde `:45`.
- El paquete preveía un delta por caso. Fijar el rechazo es caracterizar lo que hay (INV-PBT0-04: «una clase nueva se fija tal cual»), y la regla I-01 sigue cumplida: los métodos fallan por aserción con los mutantes.
- Es una desviación declarada sin efecto en el contrato.

**D-3 (dato para el Owner, sin corregir en PB-T0): `PalletTolerance = 0` hace que `LoadExisting` lance.**
- Un diseño guardado con `PalletTolerance = 0` hace lanzar `LoadExisting` con una `ArgumentException`.
- La ventana normaliza ese valor al mostrarlo (W:389), así que un rack creado con la ventana no debería guardarlo. Un dato antiguo o externo sí podría traerlo.
- Si RACKEDITAR no lo captura, el usuario vería un error al abrir el editor. No lo he medido en AutoCAD.
- Es deuda para `docs/ideas-futuras.md` o para la tarea que toque la carga (PB-T1).

**D-4 (semántica ya conocida): `pb-21-cola` pierde 667 hojas en la salida de la ventana.**
- La cola GAP y B:* y el objeto SideB de un rack de un sentido no salen de la ventana.
- Es la semántica de H14 («dormir no persiste») y de H-PB-26: la carga aparca la cola y la salida no la lleva.
- Queda fijado en CT-01. Es justo lo que D-PB-03 exige preservar en PB-T1 («parked queue preserved»), y es el riesgo R-PB-02.
- No es un defecto nuevo, pero el paquete de PB-T1 tiene que citarlo como caso de prueba.

**D-5 (cobertura, declarada): hueco de T-1.**
- La entrega lista 20 controles sin una prueba que los cambie: casillas de numeración y de cotas por vista, `PositionsBox`, `LevelsBox`, `FondosBox`, `GapBox`, etc.
- Es el hueco que el paquete pide declarar (§4.5). No es un defecto de PB-T0, pero es la red que le falta a PB-T1. Su paquete tiene que decidir si la completa antes de mover el estado.

## 3. Veredicto de la revisión directa

Conforme en alcance, bytes, CI y tabla de mutantes, con una cuestión de proceso (D-1) que no decide la sesión. 0 bloqueantes de producto o de pruebas; D-1 es una decisión del Owner. Los menores son D-2 a D-5.

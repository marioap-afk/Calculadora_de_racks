# I-64 F4B PB-T1a — Revisión directa de la sesión

**Objeto:** la entrega del Worker `R20261118T142230Z-d31d` (delegación `R20261118T141628Z-d049`, intento 1).
- BaseSha `a3b63ccc`.
- RED `15f1f225b8981d2b78746b0daed4e27da2bcd901`.
- GREEN `73d30982b0c0ce2b8c9213e626df0ab0d50b0475`.
- WorkerStatus IMPLEMENTATION_COMPLETE, Disposition NONE, sin paradas.

**Método:** git y lectura de la entrega y de los TRX de la CI, en solo lectura. No hice ninguna corrida.

## Lo que se sostiene

| Comprobación | Resultado |
|---|---|
| Alcance (INV-PBT1A-01) | `git diff --name-only a3b63ccc 73d30982` = los siete `PushBackStateNet*` de `tests/RackCad.UI.Tests/`. `git diff a3b63ccc 73d30982 -- src` vacío. El relevo comprueba que los blobs de W y WC del GREEN son los de la base y distintos en el RED |
| Los mutantes (INV-PBT1A-02) | `git diff -U0 a3b63ccc 15f1f225 -- src` tiene 15 hunks de una línea, y son exactamente las quince filas de la tabla cerrada del §5.2, con su texto. MU-T1A-13 está en W:313 (`LoadExisting`); W:296 no se toca. WC solo tiene MU-T1A-05 |
| Discriminación aislada (INV-PBT1A-03) | Las quince corridas `m01` a `m14` (con `m07a` y `m07b`): cada una hace fallar el CT de su fila. Algunas hacen fallar además otros CT: m01 → 01 y 14; m07a → 07, 08, 10 y 14; m09 → 09 y 02; m14 → 07 y 14 |
| La base sin mutantes | `b1`: 14/14 en verde |
| El RED | El filtro `PushBackStateNet` da 14/14 `Failed` en local (`rr1`, sobre el commit limpio) y en la CI `44333116533`. CI: ui-tests en failure; tests, build-ui y build-plugin en success. Core sigue en 19 070/19 070 |
| El GREEN | CI `44338475965` 4/4. UI: 2 436 resultados, 2 415 `Passed` y 21 `Skipped`. Core: 19 070. Es **exactamente** la previsión del §6.1. En local, `g1` a `g3` en verde |
| T-8 | 42 pruebas existentes fallan en la CI del RED, y todas quedan atribuidas a un mutante aislado (`t01` a `t14`) |
| Builds Release | Los conteos de avisos por código son idénticos en la base, con los mutantes y en el GREEN |
| Limpieza del perfil | Antes y después, idénticas |
| Relevo del trabajo | Válido: 332 turnos de claude-sonnet-5-5 en high; 0 denegaciones; 0 procesos participantes; Snapshot I |

## Lo que hay que decidir o declarar

**D-1. Las entradas de CT-02 y CT-03 se rediseñaron durante el paso 1** (`Deviations[0]`).
- **CT-02:** la medición `p2` mostró dos cosas.
  - Elegir «lb» en `WeightUnitBox` no llega por sí solo a la salida: `SamePallet` ignora la unidad.
  - `PalletsDeepBox` no mueve la salida.

  El CT final conserva las tres cosas que pide su fila: la línea de «lb» sola, que fija `kg`; la carga con «ton» y su «Actualizar»; y `PalletsDeepBox` editado. Además añade pasos: `DepthBox=54` tras «lb» y tras la carga con «ton», y «+ frente» tras `PalletsDeepBox`. Así se ve lo que cada caja hace de verdad, y MU-T1A-02 discrimina.
- **CT-03:** la cadena de las cuatro cajas sobre un mismo frente de `pb-03-fondos` dejaba el diseño inválido con `DepthStartBox=2`. Pasa a ser una ventana nueva por caja (las tres primeras en el frente 1 y `DepthStartBox` en el frente 0).
- **Lo que no hay:** el cambio se hizo en la medición, **antes** de escribir las pruebas definitivas y **antes** de cualquier corrida por mutante. Ningún mutante cambió, y ninguna línea esperada se ajustó después de medirla: las del CT son las de `p4`.
- **El problema:** la fila de CT-02 suponía que «lb» llegaba sola a la salida. Sin los pasos añadidos, MU-T1A-02 no habría hecho fallar su CT, que es la parada PBT1A-C04. El prompt del Worker prohibía cambiar «sus pasos, sus entradas ni su línea esperada para que el mutante discrimine». El Worker lo hizo antes de llegar a esa comprobación y lo declaró. Es el mismo tipo de hecho que c-16 de PB-T0 (F4B-E05), que el Owner ratificó solo para aquel caso.
- **Clasificación de la sesión:** desviación declarada frente a la regla SIN LATITUD. No es un defecto del producto ni de la red: lo que fija cada CT sigue siendo lo que describe su fila, y más. Como el contrato hace de esto una parada (C04 o C07), la sesión **no la acepta por su cuenta**: va al Owner con la revisión independiente.

**D-2. Las corridas `t01` a `t14` salen de los filtros del contrato** (`Deviations[1]`).
- Para la atribución de T-8, el Worker corrió en local las 42 pruebas existentes que fallaron en la CI del RED, una vez por mutante. 19 de esas 42 no caen en ningún filtro de UI del `RequiredTests`: `PushBackBootSidesUiFlowTests` (7), `PushBackBootUiFlowTests` (5), `PushBackHeaderLineWindowTests` (2) y una de cada una de `PushBackEditorWindowTests`, `PushBackHeaderCartesianTests`, `PushBackPalletScopeTests`, `PushBackSafetyPerPostFlowTests` y `PushBackTopeOnlyInSafetyTests`.
- INV-PBT1A-10 dice «solo los filtros de RequiredTests en UI, nunca la suite completa de UI en local». No se corrió la suite completa, y la prueba de limpieza posterior (paso 5, después de las `t*`) es idéntica a la anterior.
- **Clasificación de la sesión:** incumplimiento menor de la letra de INV-PBT1A-10, declarado y sin efecto medido en el perfil. Se informa al Owner.

**D-3. La lista de INV-PBT1A-13** (`UnexpectedFindings`), que es la precondición 5 de PB-T1b:
1. **c-09:** «ton» se pierde y pasa a «kg» en cuanto algo reconstruye la estructura. Está en T-2, que el Owner decidió dejar «tal cual» (F4B-E09).
2. **Hecho nuevo:** «lb» elegida sola y `PalletsDeepBox` no llegan a la salida mientras nada reconstruye la estructura. `PalletsDeepBox=9` no llega ni al frente que se añade (sigue en 8).
3. **La observación de CT-16 de PB-T0:** aceptar Seguridad sin cambios en `pb-05-topes` añade el portador de botas a la salida; el BOM no cambia.
4. **`AbsentSlotsB=[0]`** siempre declarado en un rack de un sentido: es la clase OTRA de PB-T0.
5. **Las huellas de G-1, G-2 y G-3** coinciden en los 20 diseños: ninguna pérdida nueva entre cargar, «Actualizar» y «Restaurar».

Por la precondición 5, la lista vuelve al Owner antes de emitir PB-T1b. Para la sesión, 1, 3 y 4 ya están caracterizados o decididos; 2 es el único hecho nuevo, y PB-T1 lo reproduce tal cual (D-PBT1-03, «la semántica de hoy»).

**Menores:**
- Las órdenes registradas escriben `dotnet` sin su ruta (declarado).
- `TestResults` se rellenó con un script de solo lectura sobre los TRX (declarado).

## Veredicto de la revisión directa

Conforme en alcance, mutantes, discriminación, RED, GREEN, CI y contención. La desviación D-1 y la lista D-3 vuelven al Owner, en la misma vuelta que la revisión independiente. D-2 se le informa.

# Traspaso del autor de la v2 de PB-T5

**Encargo:** la v2 tras la revisión independiente de la v1 (`review-v1/independent-review-pbt5-v1.md`, sha256 `656980ca88b6094e7962153eb694fbc9972023406f94c79c2b74959ceba9696c`, NO APTO, B 2 / I 6 / M 17). Las reglas siguen: P-08, solo objetos git en `7da361eb`, sin AutoCAD, sin compilar, sin commits y solo en `S\pbt5\`.

## Lo leído para la v2 (por objetos git en `7da361eb`)
- [T3] `docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md`, §5.5 (`:1287-1298`), §5.6 (`:1311-1321`), `:915` y `:1865`.
- [F4D] `:2076-2086` (la regla de F4-E59: «en tareas con contención de perfil, nunca suites completas de UI en local; solo en la CI»).
- [S2R] `run-log-2.md` `:143-162` (el relanzamiento, «Ignore» sin «Always ignore», «Load Once», las tres inyecciones con la variable solo en el proceso, «Descartar borradores de este documento»).
- `owner-pbt4d-nc2-decision.md` (la opción elegida dice «la aplico a este nc2 y a PB-T5»).
- AGENTS «Evidencia CI de rama y Candidato» (`event = push`, `ref = refs/heads/<rama>`).
- Los cierres de PB-TC (`:65`), PB-T3c (`:53` y `:56`) y PB-T4b (`:59`); la revisión de PB-T4a (`:207-215`); [T4] `:977` (M-23); el paquete de PB-T2 (`:1329`, el coste de `Equals`).
- `src/` en `7da361eb`: `PushBackPilotUpdateRun.cs` (`LockedLayerPrefix`, `NameDiffersPrefix`), `PushBackPilotUpdatePlan.cs` (`BlockedPrefix`, `FrontalBOfOneWay*`, `AllViewsObsoleteReason`), `PushBackPilotIntentReader.cs` (`OriginMissingDiagnostic`), `RackPushBackSystemWindow.xaml` (los cinco «Restaurar»; «Restaurar estandar» sin tilde; «Insertar vista actual» e «Insertar lateral»).
- `block-m/tools/lisp_src.txt` (las funciones de inventario).

## Estado
Terminado: `I-64-f4b-pbt5-package-v2.md`, `owner-questions-v2.md`, `pre-run-tonight-v2.md` y `_tools/check_pbt5.py` reescrito (M-08). Los sha256, en `sha256-v2.txt`. `_tools/patterns.local.txt` lleva los patrones prohibidos y **no se custodia**.

## Discrepancias y desviaciones, con su razón
Todos los hallazgos de la revisión se aplicaron. Lo que sigue es lo que se aplicó de otra forma que la literal, o lo que se añadió.

1. **El orden de las órdenes de esta noche.** El encargo dice «el build, las copias, el respaldo». La v2 pone el respaldo antes que las copias (`pre-run-tonight-v2.md` §4.3).
   - **Razón:** la condición del primer encargo («una copia de seguridad antes de usar un dibujo importante») y P-06.
   - Copiar no modifica el original, pero así el respaldo existe antes de cualquier lectura de las copias.
2. **La antigua Q-PBT5-6 no va como pregunta aparte.** Es la opción B de Q-PBT5-8, la forma del contrato.
   - **Razón:** el límite de cuatro por vuelta y dos vueltas. Además, la revisión antes de la firma es parte de cómo se cierra el contrato.
   - Así quedan ocho preguntas en dos vueltas.
3. **I-03 pedía «valor por defecto: B» para Q-PBT5-5,** y Q-PBT5-4 tenía uno, B, que la revisión consideraba correcto. No hay valores por defecto, por la regla del encargo.
   - La parada a las 13:00Z de esta noche es la lectura de la sesión principal, no una opción aplicada.
   - En Q-PBT5-4, «mientras no respondas, no se construye nada» no elige ninguna opción.
4. **M-07, la regla de reejecución de la CI,** va como propuesta dentro de PBT5-C15, para que la confirme el Owner con Q-PBT5-8. La sesión no la fija.
5. **Los filtros de UI de esta noche.** La revisión dice «los filtros de UI de los contratos de F4B». La v2 los concreta en los filtros de UI de los `RequiredTests` del contrato de PB-T4e, cada uno en su proceso.
   - **Razón:** son los vigentes, y los heredan los contratos anteriores.
   - Es una lectura de la sesión, declarada.
6. **Q-PBT5-2 tiene una opción nueva, C:** contar el estilo en los demás dibujos reales de la selección de Smoke-2.
   - **Razón:** el recuento de M-7 solo miró D1 y D2, así que no fabrica nada y puede dar cobertura real.
   - Esta noche no se hace.
7. **La selección por expresión,** `(sssetfirst nil (ssadd (handent "<handle>")))`, no está copiada de Smoke-2: su registro dice «por expresión», sin el texto. Va marcada para comprobarla en c01.
8. **`rcblk` y `rcnest` son nuevas,** de solo lectura y sin probar. Llevan un control positivo, la anidada conocida de D1, antes de usarlas.
9. **Un desliz frente a la regla de no usar `python -c`:** al comprobar si el contrato de PB-T4e era JSON, una orden llevó `python -I -c ""` al final de una tubería de `git show`.
   - No ejecutó nada: el programa estaba vacío y la entrada se descartó.
   - No escribió nada. Se declara.

## Discrepancias de fondo con la revisión
Ninguna. Todos los hallazgos se aplicaron, con las formas de arriba.

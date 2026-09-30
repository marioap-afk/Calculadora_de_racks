# I-52 — Registro del dictamen del Arquitecto sobre los artefactos de línea base, fase 1 (texto íntegro)

> Registro de evidencia. Texto **íntegro y sin cambios** del dictamen `ARCHITECT_BASELINE_PHASE1_RULING = AGREED_WITH_REQUIRED_CHANGES`
> emitido en sesión (rol de Arquitecto) sobre el commit `1711112021394a6b14112b1787abff4d3062e064`. Se publica porque los artefactos
> de la fase 2 citan sus puntos ("phase-1 ruling item N", G-1..G-4, RC-C1) y el repositorio no tenía una fuente para ellos
> (hallazgo V24-06 de la revisión de la fase 2). No es un dictamen nuevo; no cambia ningún estado.

---

# ARCHITECT REVIEW: CT-21D AUTHORITY BASELINE ARTIFACTS, PHASE 1

Revisé los textos de los artefactos, no solo el informe. Los contrasté con `origin/main` (`3375aadb`) y con V17/V18. No ejecuté nada ni modifiqué archivos.

**Hallazgos que cambian el estado**
- **`blocks.csv` sí está en el repositorio.** BA-08, BA-06 y mi informe anterior decían que no lo estaba. Está en `assets/catalogs/blocks.csv`; solo el DWG de la biblioteca no está versionado.
- **Las tolerancias tienen autoridad existente.** V17 §4.3 fija `GeometryTolerance.Length = 1e-9` in, `Angle = 1e-9` rad y `Continuity = 1e-7` in. No hay que inventar números, salvo para el factor de escala.
- **Hay una contradicción real de secuencia en el contrato.** V2.2 PA-11 y V2.1 §27.10 exigen para la línea base una revisión de completitud con traza dinámica sobre los seams. Esos seams no existen y usar el host no está autorizado.
- **G-4 existe.** Está definido en BA-04 §6. Solo faltó en mi informe en chat.

## 1. ARCHITECT_BASELINE_PHASE1_RULING
**AGREED_WITH_REQUIRED_CHANGES**

La estructura es correcta y no invento nada. Los cambios son una lista cerrada (§9) y una micro-errata de contrato (RC-C1).

## 2. BA-02..BA-11

| Artefacto | Estado | Nota |
|---|---|---|
| BA-02 registro y cobertura | CHANGES_REQUIRED (dividir) | El registro (BA-02a) queda `CANDIDATE_FOR_SEALING` tras dividir. La matriz (BA-02b) queda `BLOCKED` por NB-2. |
| BA-03 alcance de Selective | ACCEPTED_FOR_PREPARATION | Pasa a `CANDIDATE_FOR_SEALING` tras 2 cambios (ver Q sobre NB-1). |
| BA-04 catálogo (md y json) | CHANGES_REQUIRED | Y para sellar: `BLOCKED` por fixtures y clases pendientes. |
| BA-05 huella | ACCEPTED_FOR_PREPARATION | Sellado `BLOCKED` por censo, propiedades, tolerancias y vectores normativos. |
| BA-06 preparación del EVM | CHANGES_REQUIRED | Inventario incompleto respecto del diseño de seams (ver punto 5). |
| BA-07 custodia | ACCEPTED_FOR_PREPARATION | Con 3 cambios (ver punto 6). |
| BA-08 línea base del kind | CHANGES_REQUIRED | Hecho incorrecto sobre `blocks.csv`, tolerancias, rutas y oráculo. |
| BA-09 warm-up | ACCEPTED_FOR_PREPARATION | Estructuralmente listo; ver punto 8. |
| BA-10 parámetros | ACCEPTED_FOR_PREPARATION | Diseño aceptado; valores `BLOCKED` por decisiones del Owner. |
| BA-11 registro de hashes | ACCEPTED_FOR_PREPARATION | Con 2 cambios (ver punto 10). |

## Rulings de BA-02
- **A.** Sí, la cobertura por clase de escenario es aceptable durante la preparación. Cada fila cita la autoridad o entrada que falta y no inventa resultados.
- **B.** Sí, para `BASELINE_READY` **no** basta un placeholder. Todo control obligatorio (`IN_PREDICATE = YES`) necesita al menos un escenario concreto con resultado esperado sellado (o `SEALED_PENDING_PIN`, ver punto 3), más uno de violación deliberada o el motivo registrado de que no puede violarse por diseño. E-14 y E-15 necesitan un escenario concreto de registro `UNDER_CHARACTERIZATION`.
- **C.** La matriz es estructuralmente completa: ningún `GAP` y ningún control sin grupo. Falta distinguir "concreto en borrador" de "concreto sellado" y contar los sellados por fila.

## 3. G-1 (inyección antes de PV): **hace falta un sistema de coordenadas propio, sin enmienda de contrato**
Las posiciones X0..X8 pertenecen al experimento del scan de W-scan (V2.1 §10.6). Los controles negativos de S-1 y E-08 son escenarios `NC-*` cuyas inyecciones ya se declaran por escenario. Por eso basta un conjunto de nivel de arnés, aprobado por el Arquitecto y calificado como I-14, sin cambiar clases, criterios ni resultados del contrato:
- **Y0:** después de la observación PS y antes de la primera escritura de un seam.
- **Y1:** después de la última escritura de un seam y antes de las lecturas preparadas.
- **Y2:** después del sellado de `VERIFY_ELEMENT_LIST` y antes de la evaluación de PV.

**Resultado esperado.**
- **S-1 (Y1):** S-1 en FAIL, y ABORT antes de `Commit()` (O2, con verificación de abort coincidente). Autoridad: V2.1 §5 fila PV y §17.2 fila 3.
- **E-08 (Y0/Y2, cambio en el read-set de la fuente):** E-08 en FAIL en PV y ABORT (O2).

**Condiciones de diseño.**
- La escritura inyectada debe hacerse a través de la transacción `T_M` del llamador, para no violar E-06 salvo en el escenario que apunta a E-06.
- Esa escritura emite eventos de fase A: E-12 fase A puede abortar también. Ambos canales se registran; para aislar S-1 hace falta la variante silenciosa (Wr-S), cuya disponibilidad es un hecho medido.

## 4. G-2 (INVALID frente a REFUSE)
**División semántica.**
- **Identidad del entorno gobernado (tupla):** cualquier discrepancia contra la baseline, sea cual sea su origen, es **INVALID** (V2.1 §2.2), nunca un resultado de control.
- **Admisibilidad en tiempo de ejecución (controles):** E-02..E-07, E-09, E-10 y E-11M miden estado de sesión y no un campo de la tupla. Un FAIL ahí es un rechazo válido (O1).
- **Entrada inválida al producto:** un diseño de rack fuera de la envolvente (topes, esquina) es rechazo de plan-time (O1) en una corrida válida. No lo exige ningún control de CT-21D y queda fuera de la línea base.

**Aplicación.**
- E-01 y E-13 se comparan contra valores que el plano de control ya verificó en la tupla. Un FAIL de E-01 o E-13 en una corrida con tupla verificada externamente es evidencia de tupla incoherente y hace la corrida **INVALID** (V2.1 §21.1). No hay producto que rechazar.
- **`NC-E01` y `NC-E13` se retiran** como escenarios gobernantes de producto. Su detección se demuestra en la calificación: `Q-I02` (mismatch de host) y `Q-I04` (lectura de variables), y se registra "no puede violarse por diseño en una corrida gobernante".
- Esto es una aclaración de artefacto que el Coordinador debe confirmar; no cambia el contrato.

## 5. G-3 (`E6-C4`): **permanece `EXPLORATORY_NON_GOVERNING`**
Su resultado es una pregunta de caracterización (V2.1 §4.2, `NG-09`); el contrato no declara valor esperado y ambos resultados son aceptables. Su valor se **reporta** en el Acto 2 para la divulgación de `NG-09`. Hacerlo gobernante exigiría una enmienda que agregue un grupo de evidencia; no se requiere.

## 6. G-4: **FOUND**
Definido en BA-04 §6: los escenarios con `PLAN_OR_INPUT = TO_BE_DEFINED` no se pueden sellar hasta que BA-08 defina los fixtures (81 escenarios). También aparece en el §209.2 de decisiones y en BA-11 §7. No es un error documental: solo se omitió del informe en chat. Es una dependencia real y correcta.

**D. Qué impide sellar el catálogo.**
1. Fixtures de BA-08 (81 escenarios).
2. Clases sin autorar (`CL-CLEAN-*`, `E4-*`, `LK-*`, `EV-*`, `WU-DRY-*`, `PR-*`/`PW-*`/`AB-*`/`KS-*`, `U-*`, `S1`, `PP`, `CL-*`).
3. Los Y de G-1.
4. Retirar `NC-E01`/`NC-E13`.
5. Reemplazar resultados esperados "per fixture plan" por un `O1..O7` o `NONE` concreto (`PW-CLONE-*`, `PR-SOURCE-SWAP-*`).
6. El tuple de los `OU-*`: son fixtures sintéticos T0/T1 sin host, y el `FULL_BASELINE_TUPLE` por defecto no les aplica. Necesitan `TUPLE_REQUIREMENTS` propio (identidad de build del plano de control y versión del evaluador).
7. Regla de expectativas condicionales: las `CONDITIONAL` del scan solo son válidas si el evaluador offline puede resolverlas del log sellado de la propia corrida.
8. Estado `SEALED_PENDING_PIN` para escenarios cuya expectativa depende de un valor derivado de evidencia (E4-VAL, LK). Se sellan al fijar el valor y antes de su primera corrida gobernante (V2.1 §26.2).

## 7. NB-1, NB-3, NB-4, NB-6, BA-08, BA-09, BA-10, BA-11

**BA-03 / NB-1**
1. **Alcance:** ratificado (frontal por fondo y planta dentro; lateral y `Section = -1` fuera).
2. **Consenso post-I-57 completo:** basta. Preserva la exposición por referencia a V17 (§4) y Freeze V18 §7 la vuelve a heredar: dos vínculos independientes.
3. **Dentro de V18 y O-1:** sí; es un subconjunto de caracterización. Si el Owner redecide O-1 quitando Selective frontal o planta del envolvente, se reabre.
4. **`CANDIDATE_FOR_SEALING`:** tras dos cambios.
   - Agregar la fila y explicación de que la reconciliación de Foundation §3 clasifica V17 §3.7/3.8 como `HISTORICAL ONLY` **solo en sus planes neutrales**; la exposición sigue siendo de I-52 (§4). Esa fila estaba en V2.1 §25.3 y se perdió.
   - Publicar como apéndice el patrón exacto de cada verificación (`V17_INSIDE`, etc.) y el blob al que se aplica. Hoy las 25 comprobaciones "ejecutadas" viven en un script fuera del repo y no son reproducibles.

NB-1 no se cierra hasta la re-ejecución fechada, las ratificaciones y el hash.

**BA-05 / NB-3**
1. **Estructura:** aceptable.
2. **Censo de tipos de entidad de la biblioteca:** obligatorio antes del sello. Un tipo sin lista da `UNKNOWN`, y un cierre `UNKNOWN` rechaza toda corrida (O1).
3. **Tipos no presentes en el censo:** `UNKNOWN` y rechazo fail-closed (O1). Vale también para un homólogo preexistente del usuario que tenga un tipo no soportado; hay que divulgarlo como costo de disponibilidad. Ampliar la lista es una versión nueva de la especificación.
4. **Vectores:** deben ser **normativos** en el sello, ampliados con al menos uno por familia de registro (secuencia, anidado, ausente, cadena multibyte y vacía, subnormal y máximo).
5. **Compuerta de tolerancias (PA-9):** los valores existentes de V17 §4.3 se usan tal cual. `TOL_LENGTH`/`TOL_NORMAL`/`TOL_DYNAMIC` de longitud = `GeometryTolerance.Length`; `TOL_ANGLE` = `GeometryTolerance.Angle`. `TOL_SCALE` no tiene autoridad ("G4 fija el valor, absoluto"), por lo que necesita una decisión con prueba y registro. Se pueden apoyar en mediciones de dry runs no gobernantes, nunca en salidas gobernantes.

**Estado NB-3:** `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; el censo requiere lectura del DWG (host), fuera de Fase 2.

**BA-06 / NB-4**
- **Calidad:** el inventario estático es bueno, pero está incompleto respecto del diseño: solo enumera la ruta actual del producto. El contrato de seams (§15.6) agrega operaciones que la ruta actual no hace (transformación, capa y propiedades, propiedades dinámicas y `RecordGraphicsModified` sobre la referencia superior dentro de `T_M`).
- **Ruling:** la revisión de completitud es de **dos etapas**.
  - **E1 (diseño):** ruta reutilizada más operaciones derivadas del contrato.
  - **E2 (implementación):** sobre los seams implementados (EXEC-3).
- **Contradicción de contrato (RC-C1):** PA-11 exige traza dinámica sobre "cada punto de entrada de seam" para la línea base, y esos seams no existen.

**Texto exacto de la micro-errata V2.4:**

> The baseline requires the seam-operation inventory to be `STATIC_REVIEWED`: conditions 1, 2, 5, 6 and 7 of V2.2 PA-11 satisfied for the design-time inventory (the reused product path at a bound SHA plus the operations required by the seam contract) and the dynamic-trace plan of conditions 3 and 4 approved. The inventory becomes `REVIEWED` (complete) only after the dynamic trace covers every seam entry point and every plan-shape class on the implemented seams, and that is required before the first `LEARNING` run. `INVENTORY_COMPLETENESS = NOT_ESTABLISHED` until `REVIEWED`. An operation added by the implemented seams after the design-time review reopens the review of its operation class.

**Trabajo para llegar a `STATIC_REVIEWED`:**
1. Agregar al inventario las operaciones derivadas del contrato.
2. Revisión estática independiente por quien no compiló el inventario, sin ver el inventario, seguida de un registro de diferencias resuelto.
3. Censo de estado estático mutable de los ensamblados alcanzables.
4. Método concreto de traza dinámica y matriz de cobertura aprobados.

**Corrección:** el inventario debe tratar `RackDefinitionCreator` (precedente AUTH-15) como comparador de clases de operación, sin transferir su evidencia por nombre de familia.

**BA-07 / NB-6**
- **Procedimiento:** aceptado, con tres cambios.
- **Qué se ancla en la línea base:** no hay manifiesto (se captura en EXEC-11). Se ancla la cabecera de la bitácora de custodia con una entrada génesis `BASELINE_RECORDED` que porta el hash de BA-11 y el de BA-07. El manifiesto se re-ancla al capturarse.
- **¿Se puede crear el ancla antes de que todos los artefactos estén finales?** No. Se crea al final, con BA-11 completo y todas las ratificaciones registradas.
- **Qué se publica en el remoto antes del sello:** el registro del ancla (hash de BA-11 y cabecera de la bitácora), el commit empujado y verificado, y una etiqueta anotada sobre ese commit. Se declara el límite de que las ramas remotas pueden reescribirse.
- **Rol de custodia:** lo designa el Coordinador; por defecto es su sombrero, con entrada separada de la de aprobación (política de multirrol).

**BA-08**
- **A. Lectores y comparadores `MISSING`:** todos son **bloqueo de ejecución** (EXEC-4 y NB-5), no de línea base. La línea base exige su **especificación**: qué se compara, con qué capa y con qué alcance.
- **B. Debe fijarse antes de la línea base:**
  1. Los fixtures como especificaciones paramétricas.
  2. El `ORACLE_SPEC`: cómo se deriva cada valor esperado de la lista sellada desde el plan, independiente del escritor y de `mu_k`. Hoy figura como "plan y oráculo" sin especificar.
  3. Las tolerancias.
  4. Las rutas designadas para E-04.
  5. El vector de cantidades y las clases de forma.
  6. La regla de cierre y el esquema del censo.
- **C. `LOCK_MODE`:** el conjunto candidato LM-1/LM-2 es suficiente sin elegir ganador. LM-3 solo por enmienda. El modo por defecto de `LockDocument()` se lee con I-06 en la calificación.
- **D. E-04:** el contrato dice que las rutas designadas se fijan en la línea base. Se designa la **ruta R-1 (comando tecleado)** como única; R-2..R-6 quedan `UNSUPPORTED`. Falta ratificación del Coordinador. Los valores admitidos siguen derivados de evidencia.
- **Corrección de hecho:** `assets/catalogs/blocks.csv` está en el repositorio, así que el conjunto de nombres de bloque requeridos por plan se puede derivar estáticamente. Solo el DWG de la biblioteca es desconocido.
- **E. Artefactos restantes del kind:** fixtures, oráculo, tolerancias, ruta designada, vector de cantidades, esquema del censo y la cobertura sellada de la lista.

**BA-09.** Estructuralmente listo. Cambio: enlazar el censo de estado estático de BA-06, que hoy solo cita el caché de la biblioteca. Restan W-1..W-5.

**BA-10.** Dejar `UNSET` fue correcto en Fase 1. Método por parámetro (el número siempre acompañado de su intención):
- **PARAM-01 y PARAM-02:** `N >= ln(α) / ln(1 - p)`, donde `p` es la probabilidad de que ocurra el modo a detectar y `α` el riesgo aceptado de pasarlo por alto. El Owner decide `p` y `α`.
- **PARAM-03:** tamaño para que la cota superior de la tasa de falso rechazo sea ≤ `u` con confianza `c`. El Owner decide `u` y `c`.
- **PARAM-04:** política del Coordinador, justificada por la tasa de INVALID por causa.
- **PARAM-05:** decisión de identidad: qué atributos de máquina cuentan, con su clase de equivalencia.

Números sin intención declarada quedan prohibidos.

**BA-11.** Correcto tal como está: solo los blobs de contrato acordados y el compuesto informativo.
- **Cambios:** agregar la columna `CANDIDATE_HASH` y definir las transiciones.
- **Flujo:** `DRAFT → CANDIDATE_FOR_SEALING` (dictamen del Arquitecto; se registra el hash de esos bytes exactos) → `RATIFIED` (registros del Coordinador y del Arquitecto que citan ese hash) → `SEALED` (hash normativo). Si el archivo cambia tras la ratificación, la ratificación caduca.
- **Ancla:** el registro final es lo último; su hash entra en el ancla remota (BA-07 §8), y cualquier cambio posterior es una versión nueva con nuevo ancla.

## 8. Estados exactos

| Id | Estado |
|---|---|
| B5 | `ADVANCES_TO_IMPLEMENTATION_PREREQUISITE` + `REMAINS_BLOCKER_FOR_CT21D_EXECUTION` + `REMAINS_BLOCKER_FOR_REPLACEMENT_FREEZE` |
| B6 | `REMAINS_BLOCKER_FOR_ACT2` |
| NB-1 | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; `CANDIDATE_FOR_SEALING` tras 2 cambios; cierra al sellar |
| NB-2 | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE` |
| NB-3 | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; abierto por censo (host), vectores normativos y tolerancias |
| NB-4 | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; abierto hasta `STATIC_REVIEWED` y micro-errata V2.4 |
| NB-5 | `REMAINS_BLOCKER_FOR_CT21D_EXECUTION` |
| NB-6 | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; abierto hasta cambios de BA-07 y ancla remota |

### Requisitos BASE abiertos

| Base | Estado y compuerta |
|---|---|
| BASE-1 | contrato acordado; falta registrar su hash compuesto en BA-11 al sellar |
| BASE-2 | BA-08 (CHANGES_REQUIRED); cierra con fixtures, oráculo, tolerancias y rutas |
| BASE-3 | criterios y ruta designada R-1 ratificados; valores derivados de evidencia |
| BASE-4 | candidatos LM-1/LM-2 suficientes; selección derivada de evidencia |
| BASE-5 | BA-06 y BA-09; cierra con la micro-errata V2.4 |
| BASE-6 | especificación en BA-08; lectores MISSING son bloqueo de ejecución |
| BASE-7 | BA-08; falta el censo (host) |
| BASE-8 | se satisface con el contrato acordado (§16); no requiere artefacto propio |
| BASE-9 | políticas en el contrato; valores en BASE-18 |
| BASE-10 | BA-08 y BA-09 |
| BASE-11 | BA-11 final tras todos los sellos |
| BASE-12 | BA-03 sellado |
| BASE-13 | NB-2 |
| BASE-14 | NB-3 |
| BASE-15 | NB-4 (`STATIC_REVIEWED` + micro-errata) |
| BASE-16 | criterios en el contrato; escenarios `Q-*` en el catálogo sellado |
| BASE-17 | NB-6 |
| BASE-18 | Owner y Coordinador eligen los valores con intención declarada |
| BASE-19 | BA-02a `CANDIDATE_FOR_SEALING` tras dividir |
| BASE-20 | BA-02b bloqueado por NB-2 |

## 9. REQUIRED_CHANGES (lista cerrada)

**Contrato**
- **RC-C1.** Micro-errata V2.4 (texto de §7, BA-06).

**Artefactos**
- **BA-02.** Dividir en BA-02a y BA-02b; distinguir "concreto en borrador" y "concreto sellado"; contar sellados por fila; nombrar bien la clase de E-14/E-15.
- **BA-03.** Agregar la fila de "HISTORICAL ONLY" de Foundation §3; publicar el apéndice reproducible de verificaciones.
- **BA-04.** Aplicar los rulings de G-1..G-3; retirar `NC-E01`/`NC-E13`; `TUPLE_REQUIREMENTS` propio para `OU-*`; sustituir "per fixture plan" por un resultado concreto o `NONE`; agregar `SEALED_PENDING_PIN`; regla de expectativas condicionales; agregar la definición de G-4 a los informes.
- **BA-05.** Vectores normativos ampliados; regla de tipos no listados; procedimiento y esquema del censo; tolerancias con las fuentes de V17 §4.3.
- **BA-06.** Corregir el hecho de `blocks.csv`; agregar las operaciones derivadas del contrato; revisión de dos etapas; plan de traza dinámica; procedimiento de designación del revisor.
- **BA-07.** Entrada génesis y ancla de línea base sin manifiesto; lista de publicación; etiqueta anotada; designación del rol de custodia.
- **BA-08.** Corregir el hecho de `blocks.csv`; tolerancias con sus fuentes; ruta R-1 designada; `ORACLE_SPEC`; fixtures paramétricos.
- **BA-09.** Enlazar el censo de estado estático.
- **BA-10.** Métodos con fórmulas por parámetro.
- **BA-11.** Columna `CANDIDATE_HASH`, transiciones de sello y BASE-8/BASE-16 satisfechos por contrato.

## 10. PHASE2_AUTHORIZED_WORK (lista cerrada)

**A. Trabajo documental sin host**
1. Aplicar todos los cambios de §9 a BA-02..BA-11, con BA-02 dividido.
2. Micro-errata V2.4 (RC-C1) como propuesta para verificación del Coordinador.
3. Definir el sistema de coordenadas Y y sus escenarios en BA-04, con la calificación de I-14.
4. Documentar el retiro de `NC-E01`/`NC-E13` y la cobertura por `Q-I02`/`Q-I04`.
5. Autorar fixtures paramétricos, `ORACLE_SPEC`, ruta designada y esquema del censo en BA-08.
6. Extender los vectores normativos y el procedimiento del censo en BA-05.
7. Memo de decisión de parámetros (fórmulas y opciones para el Owner, sin elegir valores).
8. Procedimiento de sello (`CANDIDATE_HASH` y transiciones), plantilla del registro de sello y lista de publicación del ancla.
9. Preparar BA-03 y BA-02a como candidatos a sello, con la re-ejecución fechada de la comparación al momento.

**B. Trabajo de repositorio y análisis estático contra `main`**
1. Revisión estática independiente de operaciones mutantes por un revisor que no vio el inventario, con registro de diferencias.
2. Censo del estado estático mutable de los ensamblados alcanzables desde los caminos de los seams.
3. Derivación estática del conjunto de nombres de bloque requeridos por plan (`assets/catalogs/blocks.csv` y los builders).
4. Mapeo de las condiciones fail-closed L-11..L-14 y otras a los chequeos del código, para acotar fixtures.
5. Verificación en el código de `GeometryTolerance.*` y de sus usos.

**C. Trabajo que requiere implementación y NO está autorizado**
- Instrumentos I-01..I-16, seams SL-1..SL-6, lectores y comparadores (EXEC-4).
- Guardas estáticas P-5 en CI (EXEC-12).
- Proveedor de huella y arnés de escritores.
- Ejecutar los builders puros de Application para calcular cantidades (requiere compilar y correr código).

**D. Trabajo que requiere AutoCAD o host y NO está autorizado**
- Censo del DWG de la biblioteca.
- Traza dinámica (E2) y dry runs.
- Calificación de instrumentos.
- Corridas de aprendizaje y validación, y captura del manifiesto.
- Derivación de E-04 y de `LOCK_MODE`.
- Cualquier ejecución de CT-21D.

## 11. Estados que no cambian
- `CT21D_AUTHORITY_BASELINE_READY = FALSE`
- `CT21D_EXECUTION_READY = FALSE`
- `CT21D_EXECUTION = NOT_AUTHORIZED`
- `CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE`

## 12. Freeze y G3
Sin cambios:
- `CIA = UNKNOWN`
- `SafeOperationalState = FALSE_FOR_ADMISSION`
- `G3` detenido
- `ALT-21E` como fallback
- Freeze V18 gobernante, Freeze V35 gobernante de su propio predicado, y Freeze de reemplazo inexistente
- ADR-0036 propuesto

## 13. NEXT_GATE
**`AUTHORITY BASELINE ARTIFACTS — PHASE 2`**, limitado a la lista A y B de arriba. La micro-errata V2.4 (RC-C1) se redacta en ese gate y requiere verificación del Coordinador antes de que BA-06 pueda declararse `STATIC_REVIEWED`. Los sellos de BA-03 y BA-02a solo proceden tras el dictamen `CANDIDATE_FOR_SEALING`, la re-ejecución fechada y las ratificaciones.

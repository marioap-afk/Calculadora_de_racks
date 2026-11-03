# I-65 F7: revisión independiente del Architect sobre `8b2117c3`

**Veredicto: NOT CONFORMANT.** Hay 2 hallazgos REQUIRED y ambos son documentales. Se corrigen con un diff dentro del alcance de F7, sin tocar producto, Freeze, ADR ni tolerancias, y caben en la relectura que ya prevé el presupuesto (`I-65-f7-preparation.md` §3). También hay 8 hallazgos OPTIONAL. No declaro GATE PASS ni READY.

- Rol: Architect, SAME-SESSION ROLE (subagente de contexto independiente), solo lectura.
- **pwd informado como primera acción:** `/d/Documentos/Codex/Calculadora de racks`. No leí ese checkout. Todas las lecturas se hicieron con `git -C <worktree de la rama>` o con rutas absolutas del worktree.
- No escribí archivos, no compilé, no ejecuté pruebas ni intérpretes y no usé `merge-tree`. Los SHA-256 los calculé con `git cat-file blob … | sha256sum`.

## Respuestas

**1. Identidad: CUMPLE.**
- Entre `bd1fcf8a` y HEAD hay 14 commits y solo cambian `docs/` y `README.md`.
- Los árboles coinciden en `bd1fcf8a`, `51fc82f1` y HEAD: `src` `6cadd526`, `tests` `47fbcaf5`, `tools` `29485f42`, `assets` `d1cc1811`.
- Los blobs coinciden en los tres: `Directory.Build.props` `69223076`, `Directory.Build.targets` `58d1c335`, `global.json` `eaa68c65`, `RackCad.sln` `6483b50d`.
- README pasa de `50bd744a` a `e8595733`, y el único cambio es la fila del comando y el enlace a la guía.
- El Freeze conserva el blob `4e6e1feb…` y ADR-0050 el `c18def42…` en los tres SHA.
- `docs/FOUNDATIONS.md`, `docs/adr/README.md` y ADR-0005/0009/0017/0021/0040 no cambian frente a `bb0d5522`.
- No hay ninguna A-n ni freeze-delta. No hay EXP-01 (ver punto 3).

**2. Criterios AC-F7-01..12.**
- Son correctos y suficientes para la fila F7 del Freeze y para los 8 entregables de §99:
  - conformidad: AC-04, AC-05 y AC-11;
  - regresión: AC-03;
  - FOUNDATIONS: AC-06;
  - guía: AC-07;
  - ADR: AC-04;
  - hallazgos: AC-08;
  - READY: AC-09;
  - custodia: AC-10;
  - orden de cierre: AC-12.
- La regresión Full definida es la que AGENTS.md exige para un «Punto que exija Full»:
  - Core y UI locales;
  - builds Debug de UI y Plugin (punto 3);
  - CI con `event=push`, `ref` exacta, `head_sha` exacto y los 4 jobs en `success`.
- El build de la herramienta es un requisito adicional, más estricto que la norma, y es aceptable.
- Falta escribir de forma explícita el SDK resuelto (es invalidador) y la `ref` exacta del CI, como hicieron CD-I65-F4-221 y F5-61 (A65-F7-10).
- Lo que falta en los tres criterios pendientes está en la matriz.

**3. Revisión de conformidad del Coordinator.**
- Comprobado contra el código:
  - **Gates:** las filas CD-I65-F1-21, F3-17, F4-221 y F5-61 existen con los SHA que se citan.
  - **D-01:** G1, G2 y G3 están en `StructuralAnalysisSourceGuardTests` y `StructuralAnalysisRamElementsGuardTests`.
  - **G4 e INV-16:** existen; `StructuralSectionUnits` solo cambia la línea del literal.
  - **D-13 y D-14:** no hay `Analyze`, `ReadResults`, `analyze(` ni `save` en `src/`.
  - **INV-11:** guarda de `member_design`.
  - **INV-01..16:** todos tienen pruebas; INV-01 se cubre con G1 y G2.
  - **DIF-01:** el Owner la reconoció (CD-I65-F4-47).
  - **OV-I65-02:** el Architect la recalculó a mano en F5.
  - **Cláusula de compatibilidad:** sin cambios de API ni de texto de los ADR consumidos.
- No hay afirmaciones falsas en la revisión. Dos quedan sin calificar (A65-F7-08):
  - D-11 dice «modelos manuales independientes», pero A65-F5F-08 limita esa independencia a la fuente y a la firma numérica: el operador fue el mismo agente.
  - «D-09», cuando se refiere a la hoja firmada de ingeniería de F4, se puede confundir con la D-09 del Freeze, que es el transporte.
- La afirmación de §6, «verificadas contra el código», cae con A65-F7-01.
- **La falta de huella en el volcado frente a la Decisión 8 es una limitación conocida (A65-F4F-04), no una EXP-01:**
  - **Lo que dice el Freeze:** D-11, que es la regla específica, define el volcado como un script fijo sin entradas del paquete y aplica `STALE` solo a la lectura de vuelta. El volcado manual (§15) no puede llevar huella por construcción. Leer D-12 como «todo volcado lleva huella» haría inejecutables D-11 y OV-01.
  - **Lo que hace el código:** el código implementa la regla de vigencia de forma transitiva. Si la huella de la lectura de vuelta difiere de la del manifiesto, el resultado es `Stale` (`StructuralReadbackComparison.cs:481`). Si el volcado generado difiere de los valores de la lectura de vuelta, el resultado es `Rejected` (`FirstBindingMismatch`, línea 486).
  - **Lo que queda:** solo queda abierta la procedencia de sesión, que fue OPTIONAL en F4 y tiene opción 0 aprobada por el Owner en CD-I65-F5-11.
  - **Por qué no es EXP-01:** según LIFECYCLE §4 y §9, la clase A exige una contradicción que afecte algo consumido, y aquí no la hay. Si el Owner elige la opción 1 (nonce en el volcado), hace falta una A-n, como ya dice el plan.

**4. FOUNDATIONS.**
- Las tres entradas tienen los diez campos del esquema y todas las clases de prueba citadas existen con ese nombre.
- **Falla un hecho de la entrada «Structural Analysis Model»** (A65-F7-01):
  - Dice: «`BuildStructuralModel` es el único constructor del `StructuralModel`».
  - Pero `StructuralModel` tiene un constructor público (`StructuralModel.cs:386`) y `StructuralModelResult.Create` también es público (`StructuralExportOutcome.cs:62`).
  - Las pruebas construyen el modelo directamente (`StructuralAnalysisModelTests.cs:60`, `StructuralAnalysisRamElementsFixtures.cs:122`), y ninguna guarda lo impide.
  - La única llamada en producto es `StructuralModelBuilder.cs:487`.
- No publicarlas en F7 es correcto. LIFECYCLE §4.1 pide redactarlas antes de READY-04, WORKFLOW §11.4 las publica en el cierre de la integración, y para ser STABLE hace falta el Freeze integrado. El estado «STABLE (propuesto)» es aceptable si al publicar queda exactamente `STABLE`.
- Las entradas cubren el «Introduces» del Freeze §7. El «Extends» queda dentro de la entrada de unidades, porque `FOUNDATIONS.md` no tiene ninguna entrada de `StructuralSectionUnits`.

**5. Guía y README.**
- Contrasté con el código:
  - las indicaciones que pide el comando y el texto que imprime («Huella de entrada», «Archivo escrito»);
  - la regla de carpeta;
  - los tres archivos del paquete;
  - el esquema, las unidades y los límites del archivo de cargas (nivel ≥ 1);
  - las puertas, `READBACK OK` y `FAILED — DISCARD MODEL`;
  - `dump-script`, `--adapter-version 1.0.0` y `DUMP OK`;
  - las salidas 0-5 del comparador y sus 14 filas con su veredicto;
  - los cuatro tipos de inyección.
- **Hay una omisión material** (A65-F7-02):
  - La tabla se copia solo a la salida de compilación del Plugin.
  - El paquete del Autoloader no la incluye: lo decide CD-I65-F4-10 y lo protege la prueba `LaTablaNoViajaEnElPaqueteDelAutoloader…` (línea 353).
  - Con una instalación por bundle, `RACKRAMPAQUETE` responde `Unavailable` («Falta la tabla…», `RackRamElementsCommands.cs:110`).
  - Ni la guía, ni el README, ni la entrada «Provider Boundary» lo dicen.
- Imprecisiones menores (A65-F7-03 a A65-F7-06):
  - más de 24 cargas por miembro da `LOAD-COUNT-EXCEEDED` con resultado `Unsupported`, no un error `LOADS-*`;
  - si el archivo de cargas no existe, el resultado es `Unavailable` por excepción (línea 128);
  - falta la regla de que la carpeta padre debe existir (`PACKAGE-PARENT-MISSING`);
  - una negativa en las puertas ya consume la carpeta (`open(..., "x")` en la línea 347): para reintentar hace falta exportar un paquete nuevo;
  - faltan la parada del diálogo «los resultados se perderán» y la orden de volcar el modelo manual sin analizarlo (D-11);
  - no se explica cómo compilar ni dónde está la herramienta.
- La guía no promete más que el Freeze: no habla de validación de ingeniería, análisis ni resultados.
- La fila del README es conforme a WORKFLOW §8.

**6. Plan de READY.**
- Cubre los siete puntos de §99 y READY-01..09 en orden.
- **Conflicto con I-64**, comprobado sin `merge-tree`, comparando los archivos tocados por ambas ramas desde `bb0d5522`:
  - Coinciden `docs/ROADMAP.md`, `CustomPropertiesCommandGuardTests.cs` y `SelectiveEditorOpenTests.cs`.
  - Los dos `.cs` editan los mismos fragmentos (`Baseline` y `Assert.Equal(38)`), así que el conflicto textual es seguro.
  - En ROADMAP, las dos inserciones quedan separadas por la fila I-38, que no cambia. Eso es compatible con una fusión limpia, pero no lo verifiqué.
  - No hay conflicto entre guardas: las de I-64 escanean solo `Plugin/Workspace`, y el código de I-64 no contiene tokens `Ram[A-Z]` ni los literales de lb y gₙ.
  - I-62 e I-52 solo coinciden en ROADMAP.
- Las recomendaciones no convierten ningún OPTIONAL en REQUIRED. Hay que matizar R-D2 A (reutilizar los volcados manuales) por dos razones (A65-F7-09):
  - es una reducción de OV-I65-01 y 02 que el Freeze §5 reserva al Owner;
  - CD-I65-F4-220 aprobó esa estrategia «solo para F4».

**7. Hallazgos residuales.**
- A65-F4F-04, A65-F4F-08, A65-F5F-03/04/05/08 y C-5 se conservan en R-D3..R-D7 y en el archivo de estado, sin promoverse.
- F7-OBS-01 está bien clasificado como NON-MATERIAL y fuera de F7: D-13 no exige la ayuda integrada, añadirlo es código, y `RACKSECCION` tampoco está en `RackCommandReference`.
- READY-01 no lista el diferimiento de CD-I65-F4-10 (A65-F7-09).
- En `docs/guias/despliegue.md`, la tabla §6 ya estaba desactualizada antes de I-65: le faltan `RACKSECCION`, `RACKPROYECTAR` y otros comandos. WORKFLOW §8 nombra el README, no ese documento. Lo mismo vale para el glosario y `ARCHITECTURE.md`: ninguna regla les da base contractual. Son seguimiento, no un defecto de F7.
- El esquema de `section-material-map.json` no está documentado en ninguna guía. WORKFLOW §8 lo pide «antes de integrar», así que todavía cabe en READY (A65-F7-06).

**8. LIFECYCLE §7 para el cierre de F7.**

| Punto | Estado | Detalle |
|---|---|---|
| 1 | PENDIENTE POR CONSTRUCCIÓN | depende de la regresión Full |
| 2 | NO APLICA | 0 archivos de producto desde `51fc82f1` |
| 3 | PENDIENTE POR CONSTRUCCIÓN | Full sobre el SHA de cierre |
| 4 | PENDIENTE POR CONSTRUCCIÓN | CI de ese SHA |
| 5 | PENDIENTE | la revisión del Coordinator existe y no hay EXP-01 A; falta disponer A65-F7-01/02 y emitir CONFORMING |
| 6 | PENDIENTE POR CONSTRUCCIÓN | orden commit → árbol limpio → Full → push → CI |
| 7 | NO APLICA | sin rebase; `origin/main` local = `bb0d5522`, sin fetch por mi parte |

- **SHA documental de cierre**, según el precedente de CD-I65-F4-219/221 y F5-59/61:
  - No es `8b2117c3`. Es el commit posterior que versione las correcciones o la relectura del Architect sobre el SHA corregido, la conformidad de cierre del Coordinator y AC-F7-03/12 «pendientes por construcción».
  - Sobre ese HEAD exacto, con el árbol limpio, se corren Core y UI Full y los 3 builds; luego push y CI exacto 4/4.
  - El GATE PASS se registra en un commit posterior, como hizo `e7c1fcf1`.

**9. Privacidad: CUMPLE.**
- Los documentos nuevos no contienen nombres, cuentas, rutas con usuario ni secretos.
- La ruta de ejemplo de la guía es genérica.
- Los 7 hashes de §103 y §104 coinciden con los blobs.

## Matriz AC-F7

| AC | Estado |
|---|---|
| 01 | CUMPLE (§99, CD-I65-F7-02/04/05; Codex 50/52, `attempts` 7/9) |
| 02 | CUMPLE en `8b2117c3`; repetir en el SHA de cierre |
| 03 | PENDIENTE POR CONSTRUCCIÓN: commit de cierre, árbol limpio, Core y UI Full locales (omitidas declaradas), builds Debug de UI, Plugin y herramienta con 0 errores, SDK registrado, push, CI con `refs/heads/architecture/interop-analisis-estructural` y `head_sha` exacto 4/4 leído |
| 04 | CUMPLE (A65-F7-08 OPTIONAL) |
| 05 | CUMPLE (A65-F7-08 OPTIONAL) |
| 06 | **NO CUMPLE** (A65-F7-01) |
| 07 | **NO CUMPLE** (A65-F7-02) |
| 08 | CUMPLE (A65-F7-09 OPTIONAL) |
| 09 | CUMPLE (A65-F7-09 OPTIONAL) |
| 10 | CUMPLE para §103/§104 (7/7); pendiente para los commits siguientes |
| 11 | PENDIENTE: 2 REQUIRED abiertos, relectura y CONFORMING del Coordinator |
| 12 | PENDIENTE POR CONSTRUCCIÓN |

## Hallazgos nuevos

| ID | Ubicación | Hallazgo | Severidad | Recomendación |
|---|---|---|---|---|
| A65-F7-01 | `I-65-foundations-entry.md` l. 20; revisión de conformidad §6 | «Único constructor»: el constructor y `Create` son públicos, las pruebas construyen el modelo y ninguna guarda lo impide | **REQUIRED** (AC-F7-06; esquema de FOUNDATIONS: autoridad = código; LIFECYCLE §4.1 «factual») | Redactar: «único productor en el código de producto; el tipo tiene constructor público (pruebas, fixtures) y la exclusividad no está guardada». Una guarda posible queda como seguimiento |
| A65-F7-02 | Guía §2 y §11; fila del README; entrada «Provider Boundary» l. 48 | Omite CD-I65-F4-10: la tabla no viaja en el Autoloader y el comando da `Unavailable` | **REQUIRED** (AC-F7-07 «requisitos y límites»; CD-I65-F4-10; WORKFLOW §8) | Declararlo como requisito y límite de V1: solo funciona con la salida de compilación del Plugin |
| A65-F7-03 | Guía §3 l. 61 y tabla de §4 | `LOAD-COUNT-EXCEEDED` tiene resultado `Unsupported`; archivo de cargas inexistente da `Unavailable`; falta la carpeta padre (`PACKAGE-PARENT-MISSING`) | OPTIONAL | Precisar |
| A65-F7-04 | Guía §5 y §10 | La negativa en las puertas consume la carpeta; falta el estado `DUMP FAILED` | OPTIONAL | «Reexportar a una carpeta nueva» |
| A65-F7-05 | Guía §6, §7 y §10 | Falta la parada del diálogo de D-11 y «volcar el manual sin analizar»; referencia a los procedimientos sin enlace | OPTIONAL | Añadir y enlazar |
| A65-F7-06 | Guía §2 y README | Falta cómo compilar y dónde está la herramienta; falta el esquema y el flujo de verificación de la tabla | OPTIONAL (§8: antes de integrar) | Documentar |
| A65-F7-07 | Entradas de FOUNDATIONS | En «Units»: faltan `StructuralAnalysisSourceGuardTests` (G4, INV-16), `LengthUnitsAuthorityGuardTests` y `StructuralArea`. En «Consume (sin editar sus entradas)»: solo existe la entrada Rack Identity. G1 es específica de RAM (un proveedor nuevo necesita su propia guarda). La etiqueta «D-09» es ambigua | OPTIONAL | Precisar |
| A65-F7-08 | Revisión de conformidad §2 (D-07, D-11, D-12), §4 y §7 | La conclusión «sin EXP-01» no se razona con la vinculación por contenido (`Stale` y `FirstBindingMismatch`); «independientes» sin A65-F5F-08; etiqueta «D-09» | OPTIONAL | Citar el código y LIFECYCLE §4 |
| A65-F7-09 | Plan de READY: R-D1, R-D2 y READY-01 | ROADMAP no figura como archivo común; el comando y la salida de `merge-tree` no están versionados (y «solo lectura» es impreciso: `--write-tree` escribe objetos); R-D2 A es una reducción reservada al Owner por el Freeze §5 y CD-I65-F4-220; falta el diferimiento de CD-I65-F4-10 | OPTIONAL | Corregir el texto |
| A65-F7-10 | AC-F7-03 | No exige registrar el SDK resuelto ni la `ref` del CI | OPTIONAL | Registrarlos como en F4 y F5 |

## No comprobado sin ejecutar

- Core y UI Full, builds y corridas de CI, incluida la 40768039908 que cita §104.
- La fusión automática de ROADMAP con I-64, I-62 e I-52.
- El comportamiento real de `RACKRAMPAQUETE` en AutoCAD y de los scripts en RAM: solo leí el código.
- El contenido de un bundle publicado: solo leí el target del `.csproj`.
- `verify_custody_rows.py` y los hashes de la evidencia anterior a §103.
- El estado remoto de `origin/main`: no hice fetch.

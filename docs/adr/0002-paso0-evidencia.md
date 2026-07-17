# ADR-0002 — Paso 0: evidencia de evaluación de `codex/dinamico-modular`

- **Fechas:** evidencia automatizada 2026-07-17; validación manual del dueño 2026-07-17
- **Iniciativa:** I-01 (`docs/decision-dinamico-modular`)
- **Hash evaluado:** `9f19a8cbe194d4f66d00ab96e8c2672c2dc9ed24` (punta de `codex/dinamico-modular`)
- **Estado:** Paso 0 COMPLETO (evidencia automatizada + manual). El dueño del repositorio **aceptó la
  opción A el 2026-07-17** con base en esta evidencia; ADR-0002 queda **aceptado**. La siguiente
  validación funcional será la de I-02, en AutoCAD y sobre el árbol YA rebasado sobre `main`.

## 1. Verificación de la rama evaluada

| Aspecto | Resultado |
|---|---|
| Worktree | `C:\Users\alejandra-mendoza\.codex\worktrees\c21e\Calculadora de racks` |
| Rama / HEAD | `codex/dinamico-modular` @ `9f19a8c` |
| Upstream / divergencia | `origin/codex/dinamico-modular`, 0/0 |
| Working tree | limpio, sin archivos sin rastrear (verificado antes y después de toda la evaluación) |
| Merge-base con `main` | `cd20200` |
| Commits propios | 3 (`ee50526`, `b8bb469`, `9f19a8c`) — +12,621 / −503 en 85 archivos |
| Commits de `main` que NO tiene | 8 (de `eaede44` a `457326d`) |
| Tag `archive/catalogos-dinamico-local-pre-i00-2026-07-17` | subconjunto estricto de la rama (la rama tiene 6 filas más y el encoding correcto); no hay contenido valioso exclusivo del tag |

## 2. Inventario técnico de la rama

- **Comandos AutoCAD:** el editor dinámico se abre desde `RACKCAD` (alias `RK`) → "Sistema dinámico"
  (→ `RackDynamicSystemWindow`); `RACKSISTEMADINAMICO` (alias `RSD`) dibuja un sistema demo sin diálogo;
  `RACKEDITAR` (alias `RED`) reabre por GUID; `RACKBOMTOTAL`, `RACKLISTA`, `RACKDUPLICAR` aplican igual.
- **DLL evaluado (compilado DENTRO del worktree dinámico):**
  `C:\Users\alejandra-mendoza\.codex\worktrees\c21e\Calculadora de racks\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`
  — regenerado 2026-07-17 10:21 desde `9f19a8c`; SHA-256
  `A6566E3373532B4AD9D39E6B419D01B7F70CEA64952B9C92B4353FA87DD53611`. Es el mismo DLL que el dueño
  cargó con NETLOAD para la validación manual.
- **Flujo modular:** `DynamicRackDesign` (Domain) → `DynamicRackSystemResolver` (Application) →
  `DynamicRackSystem` → builders puros (`DynamicSystemLateralBuilder` / `DynamicSystemFrontalBuilder` /
  `DynamicSystemPlantaBuilder` / `DynamicFlowBedLateralBuilder` / `DynamicIntermediateBeamLateralBuilder` /
  `DynamicSafetyLateralBuilder` / `DynamicSafetyMultiViewBuilder` / `DynamicViewDecorations`) →
  `DynamicSystemPlan` → drawers del Plugin (`DynamicSystemDrawService`, `DynamicFrontalDrawService` nuevo,
  `DynamicPlantaDrawService` nuevo).
- **UI:** `RackDynamicSystemWindow.xaml.cs` crece de 1,332 a 3,318 líneas; ventanas nuevas
  `SafetyDefensaGridWindow` (254) y `SafetyGuiaEntradaGridWindow` (185); `SelectiveSafetyWindow`,
  `SafetyDesviadorGridWindow` y `RackMainMenuWindow` modificadas.
- **Persistencia:** `RackProjectStore`/`RackProject`/`DynamicRackSystemDocument` reescritos para persistir el
  DISEÑO (no coordenadas), DTO nullable con fallback legacy; una cabecera legacy sin procedencia se conserva
  como personalizada (fallback conservador deliberado).
- **Vistas ligadas por GUID:** laterales por poste (`Section` = nº de poste), frontal salida (`Section` 0),
  frontal entrada (`Section` 1), planta (sin camas). Frontales = cortes con solo postes/placas + IN/OUT.
- **Tests del dinámico:** 12 archivos `Dynamic*Tests` (83 casos) + `RackProjectStoreTests`,
  `SystemBomBuilderTests` y `CatalogStandardConsistencyTests` ampliados → 138 casos del subconjunto dinámico.
- **Catálogos (append salvo 1 celda):** `blocks.csv` +18 filas, `connection-layout.csv` +11,
  `connection-points.csv` +5, `mensulas.csv` +2 filas y 1 edición de `displayName` en fila existente
  ("cinta negra" → "cinta"), `secciones.csv` +2, `seguridad.csv` +2 (`GUIA_ENTRADA`, `DEFENSA_MONTACARGAS`).
- **Bloques nuevos requeridos en `blocks-library.dwg`** (18 pares bloque/vista): `LARGUERO_IN_OUT_C6_*`,
  `LARGUERO_ESCALON_INFINITO_*`, `MENSULA_TROQUEL_REDONDO_CAL_10_*`, `MENSULA_AJUSTE_INFINITO_CAL_10_*`,
  `GUIA_ENTRADA_*`, `DEFENSA_MONTACARGAS_*` (FRONTAL/LATERAL/PLANTA cada uno).

## 3. Evidencia automatizada (2026-07-17, workstation del dueño)

Precondición verificada: AutoCAD no estaba en ejecución (proceso `acad` ausente).

| Paso | Comando | Resultado |
|---|---|---|
| Restore | `dotnet restore RackCad.sln` (SDK 8.0.423 por usuario) | OK, todos los proyectos actualizados |
| Suite completa | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug` | **627/627 verdes, 0 omitidas** (ejecución 570 ms; total 13.6 s) |
| Subconjunto dinámico | filtro `Dynamic|RackProjectStore|SystemBomBuilder|CatalogStandardConsistency` | **138/138 verdes** |
| Build UI Debug | `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` | **0 errores, 0 advertencias** (6.3 s) |
| Build Plugin Debug | `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` | **0 errores**, solo las 2 familias `MSB3277` conocidas (3.0 s) |
| Higiene | `git diff --check` + `git status` tras los builds | sin problemas de whitespace; **ningún cambio versionado** |

Defectos automatizados encontrados: **ninguno**.

## 4. Evidencia manual (2026-07-17, dueño del repositorio)

- **Entorno:** AutoCAD 2025, NETLOAD del DLL de la sección 2 (Debug del worktree dinámico), sobre
  `codex/dinamico-modular` en `9f19a8c` — es decir, **ANTES de cualquier rebase sobre `main`**.
- **Fuente de la evidencia:** validación declarada por el dueño del repositorio (checklist de 17 pruebas
  recorrido personalmente). No existen capturas ni DWG adjuntos en el repo; la declaración del dueño es
  la evidencia, conforme al criterio de AGENTS.md ("el dibujo real es el criterio final").
- **Resultado global:** las 17 pruebas informadas **OK**; sin fallos bloqueantes, sin pérdida de
  información, BOM coherente con lo dibujado, round-trip y actualización en sitio correctos, desviador de
  la frontal de entrada con orientación correcta (sin espejo ni desplazamiento — era el único pendiente
  declarado por la rama), sin bloques faltantes atribuibles al código, sin errores silenciosos,
  rendimiento aceptable.

| # | Prueba | Resultado | Observaciones del dueño | Severidad |
|---|---|---|---|---|
| 1 | NETLOAD del DLL y comando RACKCAD | OK | El plugin cargó y el menú respondió correctamente | Bloqueante |
| 2 | Apertura del editor dinámico | OK | El editor abrió y permitió trabajar normalmente | Bloqueante |
| 3 | Frentes, fondos, inicios y niveles variables | OK | Se verificaron configuraciones heterogéneas | Bloqueante |
| 4 | Camas inclinadas por nivel | OK | Geometría y colocación correctas | Alta |
| 5 | Largueros IN, OUT e intermedios | OK | Se generaron en las posiciones correspondientes | Alta |
| 6 | Frontal de salida | OK | Vista correcta | Alta |
| 7 | Frontal de entrada | OK | Vista correcta | Alta |
| 8 | Laterales por poste | OK | Se representaron los frentes adyacentes correspondientes | Alta |
| 9 | Vista de planta | OK | Representación correcta | Alta |
| 10 | Elementos de seguridad | OK | Selección, dibujo y colocación correctos | Alta |
| 11 | BOM físico y RACKBOMTOTAL | OK | Cantidades coherentes con el dibujo | Bloqueante |
| 12 | Guardar, cerrar y reabrir el DWG | OK | No se perdió información | Bloqueante |
| 13 | Round-trip mediante RACKEDITAR | OK | La configuración fue recuperada correctamente | Bloqueante |
| 14 | Actualización en sitio | OK | Conservó identidad, posición y configuración | Bloqueante |
| 15 | Compatibilidad con configuración legacy | OK | El escenario probado abrió y se comportó correctamente | Alta |
| 16 | Desviador de entrada sin espejo ni desplazamiento | OK | Orientación y posición correctas | Alta |
| 17 | Bloques faltantes, mensajes y revisión visual general | OK | No se detectaron anomalías bloqueantes | Media |

**Limitaciones de esta validación:**

1. Se ejecutó **pre-rebase** (sobre `9f19a8c`, sin los 8 commits de `main`). Vale para decidir A/B; la
   integración (I-02) exige re-validar en AutoCAD sobre el árbol YA rebasado (WORKFLOW §4.5.3).
2. La evidencia es declarativa (sin artefactos adjuntos); el alcance es el escenario recorrido por el
   dueño, no una matriz exhaustiva de configuraciones.
3. La prueba 15 cubrió el escenario legacy probado, no todos los documentos históricos posibles; el
   fallback conservador (cabecera legacy → personalizada) sigue siendo el comportamiento previsto.

## 5. Evaluación técnica por área

| Área | Evidencia automatizada | Evidencia manual | Veredicto |
|---|---|---|---|
| Editor dinámico (matriz, alcances, celdas) | tests de resolver/scope verdes | pruebas 2-3 OK | Funcional |
| Geometría de frentes y fondos (frente corto gobierna `+6"`) | `DynamicDepthGeometryTests`, `DynamicFrontGeometryTests` | prueba 3 OK | Funcional |
| Niveles variables por frente | resolver + multivista verdes | prueba 3 OK | Funcional |
| Camas (pendiente invertida, `LONGITUD = tramo − 4"`, mates) | `DynamicFlowBedLateralBuilderTests` | prueba 4 OK | Funcional |
| Largueros IN/OUT C6 e intermedios | `DynamicLoadBeamGeometryTests`, lateral builder | prueba 5 OK | Funcional |
| Frontal de salida / frontal de entrada | `DynamicSystemMultiViewBuilderTests` | pruebas 6-7 OK | Funcional |
| Laterales por poste (solo frentes adyacentes) | `Cortes_UseOnlyTheHeightAndLevels…` | prueba 8 OK | Funcional |
| Planta (sin camas, refuerzo sin espejo) | multivista + `Planta_…` | prueba 9 OK | Funcional |
| Seguridad (botas/laterales/desviadores/DEFENSA/GUIA) | `DynamicSafety*`, defaults, BOM | prueba 10 OK | Funcional |
| BOM físico + RACKBOMTOTAL | `SystemBomBuilderTests` (todas las familias) | prueba 11 OK | Funcional |
| Persistencia (diseño en DWG, DTO nullable, fallbacks) | `RackProjectStoreTests` round-trip/legacy | prueba 12 OK | Funcional |
| RACKEDITAR (round-trip por GUID) | round-trip tests | prueba 13 OK | Funcional |
| Actualización en sitio (redefinición de bloque) | — (requiere AutoCAD) | prueba 14 OK | Funcional |
| Compatibilidad legacy | `DynamicDocument_LegacyHeader…` | prueba 15 OK | Funcional (fallback conservador deliberado) |
| Desviador de entrada (orientación) | `FrontalEntrance_KeepsTheAuthoredDesviadorOrientation…` | prueba 16 OK | **Cerrado** (era el único pendiente de la rama) |
| Rendimiento | — | declarado aceptable por el dueño | Aceptable |

### Observaciones residuales, clasificadas

| Observación | Clasificación |
|---|---|
| La rama carece de los 8 commits de `main`; hay que rebasar y conservar los arreglos de `eaede44` | **Riesgo del rebase** (corregible durante I-02) |
| Conflictos textuales del rebase: SOLO 7 archivos de documentación (`README.md`, `docs/00/01/03`, `docs/HANDOFF.md`, `docs/catalogos-y-plantillas.md`, `docs/ideas-futuras.md`). Ningún archivo de código ni catálogo fue tocado por ambos lados: `eaede44` vive en `RackFrameProjectDocument/Store`, `CsvCatalogReader`, `FlowBedLateralBuilder`, `SelectiveBomBuilder`, `RackFrameCommands.List.cs` — ninguno modificado por la rama | **Riesgo del rebase** (menor de lo temido por ADR-0002) |
| Interacciones SEMÁNTICAS a re-verificar tras el rebase: (a) `DynamicFlowBedLateralBuilder` compone `FlowBedLateralBuilder`, que `eaede44` modificó (acote de paso de rodillo); (b) las cabeceras embebidas del dinámico pasan por `RackFrameProjectDocument`, que gana 4 campos y `SchemaGuard`; (c) BOM del dinámico junto a la optimización sin-decoración de `SelectiveBomBuilder` | **Riesgo del rebase** (cubierto por la suite combinada: los tests de ambos lados deben quedar verdes juntos) |
| `RackDynamicSystemWindow.xaml.cs` con 3,318 líneas de code-behind y tubería clonada | **Riesgo de arquitectura / deuda posterior** — ya modelado como I-21 en el ROADMAP; NO invalida la funcionalidad validada |
| `mensulas.csv` edita el `displayName` de una fila existente (no 100% append-only) | **Riesgo del rebase** (bajo; sin conflicto textual porque `main` no tocó el archivo) |
| 18 bloques nuevos dependen de `blocks-library.dwg` del dueño (no versionado) | **Problema de entorno o catálogo** (mitigado: el dueño validó con sus bloques reales) |
| Cabecera legacy sin procedencia abre como personalizada | **Deuda posterior** (comportamiento deliberado y documentado; `Restaurar estándar` la re-deriva) |
| Fallos bloqueantes | **Ninguno encontrado** |

## 6. Resultado del Paso 0

La evidencia que ADR-0002 pedía ya existe y es uniforme: la rama **fue validada en AutoCAD por el dueño**
(17/17 OK, incluido el desviador de entrada), la suite completa y el subconjunto dinámico están verdes,
los builds compilan limpios y no hay pérdida de datos ni fallos bloqueantes.

## 7. Recomendación técnica: **OPCIÓN A** (integrar primero mediante I-02)

Derivación desde la evidencia (no desde la preferencia previa de ADR-0002):

- Los criterios de la opción B **no se cumplen**: no fallan flujos fundamentales, BOM y persistencia son
  confiables, no hay pérdida de datos y la funcionalidad no solo es utilizable — está validada de punta a
  punta por el dueño sobre bloques DWG reales.
- Los criterios de la opción A **se cumplen todos**: flujos principales funcionando, BOM y round-trip
  correctos, cero fallos restantes (no "acotados": cero), y la estabilización durante I-02 es previsible
  porque el conflicto textual del rebase quedó medido: solo documentación.
- Re-implementar (B) descartaría ~12,600 líneas funcionando y validadas para volver a escribirlas después
  del registro de sistemas; el costo que B pretendía evitar (un rebase impagable) resultó, con datos, un
  rebase de documentación más una re-validación semántica que la suite ya cubre.
- La deuda real de la rama (code-behind de 3,318 líneas) es de arquitectura, no de funcionalidad, y el
  ROADMAP ya la tiene modelada (I-21) para después del editor shell — igual que ocurre hoy con el selectivo.

### Alcance preliminar de I-02 (`feature/dinamico-modular`) — NO ejecutado todavía

1. Preservar el estado actual: antes de tocar nada, tag de resguardo sobre `9f19a8c`
   (p. ej. `archive/dinamico-modular-pre-rebase`), sin borrar la rama remota hasta el merge confirmado.
2. Renombrar la rama a `feature/dinamico-modular` (ADR-0001) en su mismo worktree.
3. Rebase sobre `origin/main` actualizado; publicar con `--force-with-lease`.
4. Resolver los conflictos (esperados solo en los 7 docs): estructura de `main` gana (HANDOFF/README
   post-I-00); el estado del dinámico se re-registra en HANDOFF §8-12 en la sesión de integración.
5. Integrar catálogos: las +38 filas de la rama se re-aplican append-only; revisar la edición de
   `displayName` en `mensulas.csv` y el encoding Latin-1/UTF-8 al re-guardar.
6. Conservar los arreglos de `eaede44` (no revertir nada de `RackFrameProjectDocument/Store`,
   `CsvCatalogReader`, `FlowBedLateralBuilder`, `SelectiveBomBuilder`, `RackFrameCommands.List.cs`).
7. Suite completa + subconjunto dinámico verdes sobre el árbol rebasado (se esperan ~627 de la rama +
   los casos nuevos de `main`: `RackFrameProjectStoreTests`, `CsvCatalogReaderTests`, etc.).
8. Build Debug de UI y Plugin con 0 errores (solo MSB3277).
9. **Re-validación manual en AutoCAD sobre el árbol YA rebasado** (WORKFLOW §4.5.3), con foco en las
   interacciones semánticas: round-trip de cabecera dinámica (4 campos nuevos del DTO), cama del dinámico
   (compone el builder con acote de paso), BOM/RACKBOMTOTAL, y un smoke del resto del checklist.
10. Merge `--no-ff` a `main` con HANDOFF §8-12 y ROADMAP actualizados como último commit de la rama;
    limpieza segura de rama y worktree.
11. **Criterio de corte:** si I-02 no se estabiliza dentro de tres sesiones, detener la iniciativa y
    redactar un ADR nuevo que proponga reemplazar ADR-0002 por la opción B. ADR-0002, ya aceptado,
    no se modifica retroactivamente.

Archivos/áreas de mayor riesgo del rebase: `docs/HANDOFF.md` (reescrito por ambos lados; el de `main`
post-I-00 es la estructura vigente), `README.md` y `docs/00/01/03/04` (la rama documenta el dinámico sobre
la estructura documental vieja), y las tres interacciones semánticas de la sección 5 (flow bed compuesto,
DTO de cabecera con campos nuevos, BOM sin decoración).

## 8. Referencias

- Evidencia automatizada registrada originalmente en el commit `f020608` de `docs/decision-dinamico-modular`.
- Informe de cierre de la rama: `docs/HANDOFF.md` de `9f19a8c` (§8-12).
- ADR-0002 (propuesto), ROADMAP I-01/I-02, WORKFLOW §4.5-4.6.

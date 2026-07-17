# ADR-0002 — Paso 0: evidencia de evaluación de `codex/dinamico-modular`

- **Fecha de evaluación:** 2026-07-17
- **Iniciativa:** I-01 (`docs/decision-dinamico-modular`)
- **Hash evaluado:** `9f19a8cbe194d4f66d00ab96e8c2672c2dc9ed24` (punta de `codex/dinamico-modular`)
- **Estado:** evidencia automatizada completa; validación manual del dueño PENDIENTE
- **Regla:** este documento NO decide A/B; la decisión espera los resultados manuales del dueño.

## 1. Verificación de la rama evaluada

| Aspecto | Resultado |
|---|---|
| Worktree | `C:\Users\alejandra-mendoza\.codex\worktrees\c21e\Calculadora de racks` |
| Rama / HEAD | `codex/dinamico-modular` @ `9f19a8c` |
| Upstream / divergencia | `origin/codex/dinamico-modular`, 0/0 |
| Working tree | limpio, sin archivos sin rastrear |
| Merge-base con `main` | `cd20200` |
| Commits propios | 3 (`ee50526`, `b8bb469`, `9f19a8c`) — +12,621 / −503 en 85 archivos |
| Commits de `main` que NO tiene | 8 (de `eaede44` a `457326d`), incluidos los 6 arreglos de la revisión exhaustiva que tocan `RackFrameProjectDocument`/`RackFrameProjectStore` |
| Tag `archive/catalogos-dinamico-local-pre-i00-2026-07-17` | subconjunto estricto de la rama (la rama tiene 6 filas más y el encoding correcto); no hay contenido valioso exclusivo del tag |

## 2. Inventario técnico de la rama

- **Comandos AutoCAD:** el editor dinámico se abre desde `RACKCAD` (alias `RK`) → "Sistema dinámico"
  (→ `RackDynamicSystemWindow`); `RACKSISTEMADINAMICO` (alias `RSD`) dibuja un sistema demo sin diálogo;
  `RACKEDITAR` (alias `RED`) reabre por GUID; `RACKBOMTOTAL`, `RACKLISTA`, `RACKDUPLICAR` aplican igual.
- **DLL para NETLOAD (compilado DENTRO del worktree dinámico):**
  `C:\Users\alejandra-mendoza\.codex\worktrees\c21e\Calculadora de racks\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`
  — regenerado 2026-07-17 10:21 desde `9f19a8c`; SHA-256
  `A6566E3373532B4AD9D39E6B419D01B7F70CEA64952B9C92B4353FA87DD53611`.
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
  `GUIA_ENTRADA_*`, `DEFENSA_MONTACARGAS_*` (FRONTAL/LATERAL/PLANTA cada uno). Según el HANDOFF de la rama,
  el usuario ya validó estos bloques progresivamente; un bloque faltante se reporta y se omite (no aborta).
- **Informe de cierre de la rama** (HANDOFF de `9f19a8c` §9-12): validación progresiva del usuario en AutoCAD
  confirmada para editor multi-frente, matriz, BFR, IN/OUT, cama integrada, intermedios, poste derivado
  centrado, vistas, cotas, BOM y seguridad. **Único pendiente manual declarado:** reconfirmar que el
  desviador de la frontal de entrada conserva la orientación del bloque sin `MirroredX` (la regresión
  `FrontalEntrance_KeepsTheAuthoredDesviadorOrientationWithoutMirroring` está verde, pero el criterio final
  es el bloque DWG real).

## 3. Validación automatizada (2026-07-17, este equipo)

Precondición verificada: AutoCAD no estaba en ejecución (proceso `acad` ausente).

| Paso | Comando | Resultado |
|---|---|---|
| Restore | `dotnet restore RackCad.sln` (SDK 8.0.423 por usuario) | OK, todos los proyectos actualizados |
| Suite completa | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug` | **627/627 verdes, 0 omitidas** (ejecución 570 ms; total 13.6 s) |
| Subconjunto dinámico | filtro `Dynamic|RackProjectStore|SystemBomBuilder|CatalogStandardConsistency` | **138/138 verdes** |
| Build UI Debug | `dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug` | **0 errores, 0 advertencias** (6.3 s) |
| Build Plugin Debug | `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` | **0 errores**, solo las 2 familias `MSB3277` conocidas (3.0 s) |
| Higiene | `git diff --check` + `git status` tras los builds | sin problemas de whitespace; **ningún cambio versionado** |

Defectos automatizados encontrados: **ninguno** (no hubo fallos de tests ni de builds).

## 4. Checklist manual para el dueño

Cargar SIEMPRE el DLL del worktree dinámico (ruta de la sección 2). Cerrar AutoCAD antes de cualquier
rebuild. Registrar evidencia = captura de pantalla o anotación breve por prueba.

| # | Prueba | Cómo | Resultado esperado | Severidad si falla |
|---|---|---|---|---|
| 1 | Carga NETLOAD | `NETLOAD` → DLL del worktree dinámico → `RACKCAD` | Menú abre; biblioteca de bloques reportada | Bloqueante |
| 2 | Editor dinámico | `RACKCAD` → "Sistema dinámico" | `RackDynamicSystemWindow` abre con defaults y seguridad preseleccionada | Bloqueante |
| 3 | Sistema multi-frente | 3+ frentes; fondos distintos (p. ej. 8/6/8) e inicio distinto; niveles variables por frente | El frente más corto gobierna los dos `+6"`; frentes mayores prolongan el patrón; alturas por frente | Bloqueante |
| 4 | Camas y rodillos | Revisar cama inclinada por nivel en la lateral | Salida baja izquierda, entrada alta derecha; `LONGITUD = tramo − 4"`; apoyada en `TROQUEL_IN → TROQUEL_CAMA` | Alta |
| 5 | Largueros IN/OUT e intermedios | Ver extremos y postes internos en lateral | Pareja C6 completa por nivel en X=0 y X=TotalLength (entrada espejeada, sin ±6"); un intermedio por poste interno sobre el origen del riel; central sin espejo | Alta |
| 6 | Frontal de salida | Insertar frontal salida (Section 0) | Corte con postes/placas + IN/OUT; altura adyacente por poste | Alta |
| 7 | Frontal de entrada | Insertar frontal entrada (Section 1) | Ídem, lado de entrada | Alta |
| 8 | Laterales por poste | Insertar lateral pidiendo nº de poste (extremo y medio) | Cada corte usa solo profundidad/altura/niveles de frentes adyacentes | Alta |
| 9 | Planta | Insertar planta | Sin camas; refuerzo continúa tras el poste principal, misma línea, sin espejo | Alta |
| 10 | Seguridad | Diálogo Seguridad: botas, laterales, desviadores, DEFENSA (lados/longitudes salida-entrada), GUIA (frente/nivel) | Se dibujan en las vistas aplicables; DEFENSA usa el piso de la placa; GUIA 8" sobre IN/OUT de entrada con LONGITUD del tramo | Alta |
| 11 | BOM | Botón BOM + `RACKBOMTOTAL` | Cabeceras/postes/placas, separadores, IN/OUT por frente-nivel, intermedios, 1 Cama/nivel con longitud/BFR sin despiece, seguridad física sin duplicar por vista | Bloqueante |
| 12 | Guardado y persistencia | Guardar DWG, cerrarlo y reabrirlo | El diseño viaja en el DWG; sin errores al reabrir | Bloqueante |
| 13 | Round-trip | `RACKEDITAR` sobre cualquier vista | Editor precargado con matriz, fondos, peraltes, seguridad y overrides intactos | Bloqueante |
| 14 | Actualizar en sitio | Cambiar algo (p. ej. niveles) → Actualizar | Todas las vistas ligadas se redibujan; ninguna copia se mueve | Bloqueante |
| 15 | Legacy | `RACKEDITAR` sobre un dinámico dibujado con el trunk (si existe DWG previo seguro) | Abre sin pérdida; cabecera legacy aparece como personalizada (fallback deliberado) | Alta |
| 16 | **Desviador frontal de entrada** | Insertar frontal de entrada con desviadores activos | El bloque conserva su orientación dibujada, SIN espejo (`MirroredX`) ni desplazamiento — ÚNICO pendiente declarado por la rama | Alta |
| 17 | Revisión visual general | Recorrer todas las vistas y la línea de comandos | Sin bloques faltantes, mensajes de error ni avisos silenciosos | Media |

### Tabla de resultados (llenar por el dueño)

| # | Prueba | Resultado (OK / Falla / No probada) | Observaciones | Severidad | Evidencia |
|---|---|---|---|---|---|
| 1-17 | (una fila por prueba) | | | | |

## 5. Riesgos conocidos

1. **La rama está 8 commits detrás de `main`**, incluidos los 6 arreglos de `eaede44` que tocan la
   persistencia de cabecera (`RackFrameProjectDocument` con 4 campos nuevos, `SchemaGuard` en
   `RackFrameProjectStore`) — la rama reescribió esos mismos archivos: el rebase de I-02 tendrá conflictos
   semánticos ahí, en `SystemBomBuilder`, stores y los 6 CSV de catálogo.
2. Esta validación es **pre-rebase**: vale para decidir A/B, pero la integración (I-02) exigirá re-validar
   sobre el árbol ya rebasado (WORKFLOW §4.5.3).
3. `RackDynamicSystemWindow.xaml.cs` (3,318 líneas) reproduce el patrón code-behind que la Fase 5 del
   ROADMAP quiere erradicar (I-21 ya lo contempla).
4. `mensulas.csv` edita una fila existente (no es 100% append-only); riesgo bajo, revisar en el rebase.
5. Los 18 bloques nuevos dependen de `blocks-library.dwg` del dueño (no versionado); sin ellos el dibujo
   omite piezas sin abortar.

## 6. Criterios preliminares (sin decidir todavía)

**Opción A razonable si:** los flujos principales del checklist funcionan; BOM y round-trip correctos; los
fallos restantes son acotados; la rama puede estabilizarse previsiblemente durante I-02 (criterio de corte
del ROADMAP: 3 sesiones).

**Opción B razonable si:** fallan flujos fundamentales; BOM o persistencia no confiables; hay pérdida de
datos; la funcionalidad está lejos de ser utilizable; corregir antes del rebase ≈ reimplementar.

Contexto actual: la evidencia automatizada es íntegramente verde y el informe de cierre de la rama declara
validación progresiva del usuario con UN pendiente puntual (prueba 16). La palabra final es del dueño con
el checklist de la sección 4.

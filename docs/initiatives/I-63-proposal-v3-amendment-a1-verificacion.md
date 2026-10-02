# I-63 — Proposal V3 · Amendment A-1: precisiones de verificación (O-PV3-1..3)

```text
Amendment          = A-1 (primera de la secuencia; append-only)
Tipo               = Coordinator-only (INITIATIVE_LIFECYCLE §6: añade precisión a pruebas de comportamiento ya congelado)
Freeze aplicable   = Proposal V3 congelada: docs/initiatives/I-63-proposal-v3.md
                     commit de Freeze f61d0aca859a11b15cbe1797069a83cba873ba95, blob 4d5dedce15363fa6378e460c5b63005fe41b858b
                     (acuerdo sobre fb3c87888ed583d1117ca565ce4751b51f67a81b, blob e4a94effa99f29e06b447a3ce07ec5bcc41f0b6d)
Applies-to         = all
Autoridad          = Orden de Freeze del Coordinator (decisiones de I-63 §2, «Orden de Freeze»)
Origen             = Architect R3, OPTIONAL O-PV3-1, O-PV3-2 y O-PV3-3 (evidencia de I-63 §20)
Materialidad       = ninguna de M-01..M-08 (§4)
```

## 1. Qué es y qué no es

- **Es** una enmienda solo del Coordinator que **precisa cómo se verifican** tres obligaciones invariante→prueba ya congeladas: INV-29 (b),
  INV-20 e INV-14/INV-34.
- **No** cambia comportamiento, arquitectura, alcance, métricas, fases, gates, persistencia, semántica de namespaces ni decisiones de
  producto. No edita la Proposal V3 congelada: se lee junto a ella.
- No autoriza implementar. Eso exige la autorización del gate por el Coordinator.

## 2. Deltas

### A-1.1 — INV-29 (b): familias de referencias de AutoCAD y WPF (O-PV3-1)

**Cláusula congelada.** INV-29 (b) usa **una sola** función `ForbiddenReferenceFamilies(proyecto)` sobre el cierre de referencias
declaradas de un `.csproj`, con el objetivo `RackCad.Application.csproj` (sin familias) y los controles positivos `RackCad.UI.csproj` (WPF)
y `RackCad.Plugin.csproj` (AutoCAD, «`AutoCAD.NET`»).

**Precisión añadida:**
1. La función reconoce la familia **AutoCAD** por **cualquiera** de dos formas:
   - `PackageReference Include="AutoCAD.NET"`;
   - `Reference Include` igual a `AcCoreMgd`, `AcDbMgd` o `AcMgd`.

   El Plugin declara las dos, en `ItemGroup` con condiciones opuestas sobre `UseAutoCADNuGetReferences`
   (`RackCad.Plugin.csproj:11-16, 28-40`); por defecto, `false` (`:7`).
2. La función toma la **unión** de los ítems de todos los `ItemGroup` y propiedades, **sin evaluar** ningún atributo `Condition`.
3. Reconoce la familia **WPF** por `UseWPF` = `true` en cualquier `PropertyGroup`, o por una `Reference` a `PresentationFramework`,
   `PresentationCore` o `WindowsBase`. El Plugin también declara `UseWPF` = `true` (`:4`), así que su control afirma que **contiene**
   AutoCAD, no que sea la única familia.
4. La función **no** inspecciona dependencias transitivas de los `PackageReference`. Hoy Application solo tiene `ProjectReference` a
   Domain, así que el riesgo es nulo.

**Por qué no cambia nada congelado:** el objetivo, los controles y la regla «misma función» son los de INV-29 (b). Solo se fija cómo
reconoce la función una familia que el `.csproj` ya declara.

### A-1.2 — INV-20: control positivo explícito `A-B` (O-PV3-2)

**Cláusula congelada.** INV-20 usa una entrada `rack` sintética `Frentes-Vacios`, sin ninguna variable con operador. Ese texto sin llaves
**no** da `OperatorInName`, y un mismo *helper* aplica la aserción a este caso y «al de control».

**Precisión añadida:** el control es explícito y está dentro de la misma prueba, con el mismo *helper*:
- una tabla con la variable de proyecto `A-B` y la entrada `rack` sintética;
- el texto `A-B` sin llaves **sigue dando** `OperatorInName`.

Así consta en la prueba misma que el detector no se desactivó globalmente, como hoy fija
`ExpressionBinderTests.UN_TRAMO_CONTIGUO_CON_OPERADORES_IGUAL_A_UN_NOMBRE_DA_OPERATORINNAME` (`ExpressionBinderTests.cs:301-317`).

**Por qué no cambia nada congelado:** es el comportamiento vigente de `projectVariable`, ya protegido por INV-19, INV-28 y D-19.

### A-1.3 — INV-14 / INV-34: dónde se cuentan las lecturas de diseño (O-PV3-3)

**Cláusula congelada.** INV-14 e INV-34 exigen **0 lecturas de diseño** para Push Back y Cabecera en `Rack.Frentes` (D-28, paso 2).

**Precisión añadida:** el contador de lecturas de diseño se observa en el **costado del lector D-26**, la función que lee el diseño de un
kind para E5. **No** se observa en los *stores* (`SelectivePalletDesignStore`, `RackProjectStore`, `FlowBedConfigurationStore`). El 0 de
Push Back y Cabecera se mide en ese costado, por `RackMetricRequest` y por `RackSummary.Metrics`.

**Por qué no cambia nada congelado:** D-28 ya decide que esos kinds terminan en el paso 2 sin leer. La precisión fija el punto de
observación que hace el oráculo discriminante.

## 3. Obligaciones afectadas y gates

| INV | Gate (sin cambio) | Lo que añade A-1 |
|---|---|---|
| INV-29 (b) | G4 | A-1.1: reconocimiento de familias, unión sin `Condition`, sin dependencias transitivas |
| INV-20 | G3 | A-1.2: control `A-B` explícito, con el mismo *helper* |
| INV-14 | G1 | A-1.3: lecturas contadas en el costado del lector D-26 |
| INV-34 | G1 (petición) y G4 (resumen) | A-1.3: ídem |

## 4. Materialidad

| ID | Estado | Razón |
|---|---|---|
| M-01..M-08 | **No activados** | Solo se precisa la verificación de comportamiento ya congelado. No cambia quién posee una regla, ningún DTO, el comportamiento observable, la semántica de fallo, contratos consumidos, puntos de extensión, mecanismos ni ADR |

Por eso basta con el Coordinator (LIFECYCLE §6: «Una A-n Coordinator-only puede añadir pruebas u OV de comportamiento ya congelado»).
Las tres precisiones vienen del propio Architect R3, que las calificó de OPTIONAL y no bloqueantes.

## 5. Evidencia

- Veredicto Architect R3, literal: evidencia de I-63 §20 (SHA-256 `190368f5…`).
- Orden de Freeze del Coordinator: decisiones de I-63 §2.
- Fuentes citadas: `src/RackCad.Plugin/RackCad.Plugin.csproj` y `tests/RackCad.Tests/ExpressionBinderTests.cs` en `origin/main`
  `819955d61a6da4c811a11fbd11b5dca13f634b7c`.

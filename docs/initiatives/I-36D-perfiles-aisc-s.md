---
schema: rackcad-initiative/v1
id: I-36D
title: Perfiles AISC S/IPS y geometria visual derivada
type: feature
status: integrated
branch: feature/perfiles-aisc-s
base_branch: main
priority:
size: M
depends_on: [I-36A, I-36B, I-36C]
conflicts_with: [I-37]
context_packs: [architecture-kernel, catalogs-data, ui-editors, autocad-plugin, delivery-validation, documentation-governance]
automation_state_path: docs/automation/state/I-36D.yml
decision_paths: [docs/automation/decisions/I-36D.md]
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: false
requires_owner_validation: true
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# Perfiles AISC S/IPS y geometria visual derivada

> **Cuarta iniciativa de la Fase 6, y la primera que introduce una autoridad que NO es AISC.** I-36A
> importo 983 secciones y dejo fuera, contadas y declaradas, las 28 filas `Type = S`. I-36B derivo
> geometria de lo que la fuente publica y cerro con un **requisito futuro obligatorio**: incorporar
> IPS/S y la geometria visual de los laminados. I-36D es esa iniciativa.
>
> **El hecho que la gobierna:** la AISC Shapes Database v16.0 **no publica la pendiente del patin ni
> ningun radio explicito**, para S ni para ninguna familia. Verificado contra el libro, no contra la
> documentacion (evidencia I-36D). Por tanto una S dibujada solo con datos tabulados es un perfil de
> patines paralelos, es decir **indistinguible de una W**. Dibujarla reconociblemente exige una
> convencion que RackCad declara como propia.

## 1. Objetivo

Que las **28 secciones AISC S** existan en el catalogo neutral como **familia propia**, se dibujen de
forma **reconocible como S**, y que el sistema diga **con precision** que parte de ese dibujo procede
de AISC y que parte es una **convencion visual de RackCad**.

Resultado verificable:

- las 28 filas `Type = S` importadas, **cero descartes silenciosos**, en `structural-sections-s.csv`;
- id `AISC-S-S10X25_4` — token de familia `S`, estable y distinto de `W` (el punto de la designación
  normaliza a `_` por **ADR-0021**, ya aceptado: mismo caso que `AISC-HSS-RECT-HSS4X4X_250`; la
  designación EDI conserva su punto en su propio campo);
- geometria con **patin inclinado** y filete visual, en los dos niveles de detalle;
- una **autoridad declarada** que viaja con el resultado y separa dato tabulado de convencion visual;
- una **advertencia visible** en el inspector y en el mensaje de insercion.

## 2. Problema

Tres problemas encadenados, todos medidos:

1. **Las 28 filas S estan fuera y no por defecto de datos.** `AiscFamilyClassifier.Classify` es un
   `switch` con cuatro casos —`W`, `C`, `L`, `HSS`— y `default: Excluded(type)`. `S` cae en el
   `default`. Es una decision de alcance del MVP, contada en el manifiesto
   (`excludedTypeCounts.S = 28`), no un fallo.
2. **Sus datos estan completos.** De las 15 columnas dimensionales, 12 estan pobladas 28/28
   (`d, ddet, bf, bfdet, tw, twdet, twdet/2, tf, tfdet, kdes, kdet, T`); `WGi` 21/28; `k1` y `WGo`
   vacias en las 28. Las 21 propiedades resistentes estan **completas 28/28**. No falta nada para
   catalogar.
3. **Falta exactamente lo que hace que una S se vea como una S.** No hay columna de pendiente,
   conicidad, paso ni inclinacion en las 166 del libro; el unico encabezado que contiene `tan` es
   `tan(alfa)`, definido por el Readme como el angulo de ejes principales **de angulos simples**, y
   vacio en las 28 filas S. No hay radio explicito: las cinco menciones de `radius` son radios de
   **giro** (`rx, ry, rz, ro, rts`). `kdes`, `kdet`, `k1` y `T` son **distancias al pie del filete**
   por definicion literal del Readme, y el Readme **nunca** las llama radios.

El corolario es el que obliga a decidir: en los canales C, aceptados por el Owner como
`TabulatedDerived`, la aproximacion produce un canal **reconocible** al que le faltan transiciones.
En S produce una figura que se lee como **otra familia**. Por eso S no puede tratarse con el mismo
silencio que C.

## 3. Alcance

### 3.1 Catalogo

- `StructuralSectionFamily.S` como **quinto miembro**, con token estable `S`;
- `SSectionDimensions` como **tipo propio** (no alias ni herencia de `WSectionDimensions`);
- `case "S"` en `AiscFamilyClassifier`, saliendo del `default`;
- rama de mapeo en `AiscRowMapper` (dimensiones y propiedades);
- serializador y lector estricto de la familia;
- `assets/catalogs/structural-sections-s.csv` **generado** por el importador;
- manifiesto: `S = 28` en `countsByFamily`, **fuera** de `excludedTypeCounts`, `totalCount = 1011`,
  nuevo SHA-256 del archivo nuevo y `mapperVersion` incrementada;
- overlay `IsEnabled`, cache por mtime, lookup por id/EDI/familia e IDs existentes **conservados**;
- **los cuatro CSV existentes byte-identicos** y `secciones.csv` **intacto**.

### 3.2 Propiedades

S reutiliza el bloque resistente aplicable a W, poblado 28/28 en la fuente:
`A, Ix, Zx, Sx, rx, Iy, Zy, Sy, ry, J, Cw, Wno, Sw1, Qf, Qw, rts, ho, PA, PB, PC, PD`.

`SourceSpecialNote` queda **`null`** para S: `T_F` esta vacio en las 28 filas, y el Readme reserva sus
notas especiales a W, M, WT y MT. Heredar la lectura de W importaria un significado que no aplica.

### 3.3 Geometria

- **builder propio** de S, hermano de los cuatro existentes;
- `Simplified`: taper visual con esquinas agudas;
- `Tabulated`: taper mas las **cuatro transiciones visuales** (filetes alma-patin);
- **fidelidad** `TabulatedDerived` en `Tabulated` (el enum `SectionFidelity` **no cambia**);
- **autoridad** `VisualDerived` en **ambos** niveles de detalle (eje nuevo, ortogonal a la fidelidad);
- degradacion **explicita** con diagnostico, nunca silenciosa;
- diagnosticos **estables** (mismo texto para la misma entrada);
- **bounds y simetria conservados**: ancho `bf`, alto `d`, centroide geometrico en el origen;
- la **misma** instancia prismatica, las mismas vistas y el **mismo** plan neutral de I-36B.

### 3.4 UI y Plugin

- filtro y etiqueta de familia **S/IPS** en el inspector;
- el inspector muestra **autoridad** y **advertencia**;
- el preview consume el plan neutral, sin recalcular;
- AutoCAD consume **ese mismo** plan;
- `RACKSECCION` y el boton del menu siguen compartiendo `StructuralSectionCommandFlow`.

## 4. Fuera de alcance

Cada uno de estos es **condicion de detencion**, no una omision por olvido:

- **I-37 Cantilever** y cualquier miembro: columnas, brazos, bases;
- conectores, mensulas, placas, soldaduras, perforaciones y troqueles;
- calculo resistente y seleccion automatica de perfil;
- materiales y grados;
- **solidos 3D** (prohibidos por ADR-0022);
- persistencia y round-trip de lo insertado;
- **mejora visual de W, C, L o HSS** — I-36D toca S y solo S;
- `bf/2tf` y `h/tw`, aunque esten pobladas 28/28;
- modificacion de `secciones.csv`;
- catalogos de sistemas, `blocks.csv` y `blocks-library.dwg`;
- geometria de fabricante, CNC y shop drawings;
- descarga de la fuente en tiempo de ejecucion.

## 5. Contexto requerido

`AGENTS.md`; `docs/WORKFLOW.md`; `docs/ROADMAP.md` Fase 6; `docs/ARCHITECTURE.md` 4.4.1-4.4.2;
[ADR-0020](../adr/0020-catalogo-neutral-de-secciones-estructurales.md),
[ADR-0021](../adr/0021-identidad-unidades-y-presentacion-de-secciones.md),
[ADR-0022](../adr/0022-geometria-parametrica-de-secciones-estructurales.md) y
[ADR-0023](../adr/0023-geometria-visual-derivada-perfiles-s.md); los contratos, decisiones, estados y
evidencias de I-36A, I-36B e I-36C; `docs/guias/secciones-estructurales.md` y
`docs/guias/geometria-secciones-estructurales.md`; la evidencia de esta iniciativa
([`../automation/evidence/I-36D-auditoria-aisc-s.md`](../automation/evidence/I-36D-auditoria-aisc-s.md)).

Context Packs: `architecture-kernel`, `catalogs-data`, `ui-editors`, `autocad-plugin`,
`delivery-validation`, `documentation-governance`.

Codigo: `AiscFamilyClassifier`, `AiscRowMapper`, `AiscColumnMap`, `StructuralSectionFamily`,
`StructuralSectionId`, los cuatro `*SectionGeometryBuilder`, `StructuralSectionGeometry`,
`StructuralSectionRepresentationPlan`, `StructuralSectionInspector*` y
`StructuralSectionCommandFlow`.

**`ORCHESTRATION.md` no existe** en el repositorio ni en su carpeta padre: comprobado, no se usa.

## 6. Dependencias

Integradas y verificadas: **I-36A**, **I-36B**, **I-36C**. Base `202e456`, CI 4/4 verde.

Estorbo declarado: **I-37**. No debe abrirse mientras I-36D este activa — I-37 consumiria una familia
y una autoridad que aqui aun cambian de forma.

Entrada del dueno requerida antes de integrar: **aceptacion de ADR-0023** tras ver el dibujo real.
No se requiere decision del dueno para **implementar** (`requires_owner_decision: false`): la
convencion candidata esta escrita, medida y acotada en este contrato.

## 7. Archivos esperados

Crear: `structural-sections-s.csv`; `SSectionDimensions`; `SSectionGeometryBuilder`; el tipo de
autoridad geometrica; sus pruebas.

Modificar: `StructuralSectionFamily` y `StructuralSectionFamilies`; `AiscFamilyClassifier`;
`AiscRowMapper`; serializador y lector; manifiesto; el inspector; las guias de secciones y de
geometria.

**No tocar**: `secciones.csv`, `blocks.csv`, `blocks-library.dwg`, los cuatro CSV de familia
existentes, `deploy/`, `.github/`, `RackCad.sln`, `docs/HANDOFF.md` (salvo en la sesion de
integracion) y ningun sistema vigente.

## 8. Fases

1. **Contrato, ADR-0023 y viabilidad geometrica** — esta fase. Documental y analitica. Sin producto.
2. **Catalogo**: familia, dimensiones, clasificador, mapper, CSV y manifiesto. Los cuatro CSV
   anteriores byte-identicos.
3. **Geometria**: builder de S, autoridad, diagnosticos, bounds y simetria.
4. **UI y Plugin**: filtro, etiqueta, autoridad y advertencia; sin generador nuevo.
5. **Validacion del Owner en AutoCAD** y aceptacion o rechazo de ADR-0023.

Cada fase termina con evidencia revisable. La fase 2 no arranca sin revision del coordinador sobre
esta.

## 9. Pruebas y builds

`dotnet test` completo verde; build Debug de Application, UI y Plugin con 0 errores propios; CI 4/4
sobre el SHA publicado; `deploy/verify-bundle.ps1` y su harness. Pruebas nuevas: importacion de las 28
filas, round-trip del CSV, identidad `AISC-S-*`, geometria (bounds, simetria, cierre, tangencia,
ausencia de autocruce), autoridad declarada y guardas de alcance sobre los archivos congelados.

## 10. Validacion manual

**Aplica.** `requires_autocad: true`, `requires_owner_validation: true`. El Owner debe ver en AutoCAD
2025, sobre el DLL Debug del worktree: una S insertada en las cuatro vistas; el patin **visiblemente
inclinado**; el filete; la punta aguda; la advertencia en el inspector y en el mensaje; y que W, C, L
y HSS **no cambian**. Su veredicto sobre el dibujo es lo que acepta o rechaza ADR-0023.

## 11. Criterios de aceptacion

28 filas importadas sin descarte; `totalCount = 1011`; ids `AISC-S-*`; cuatro CSV previos
byte-identicos; `secciones.csv` intacto; geometria con bounds `bf x d` exactos y centroide en el
origen; autoridad `VisualDerived` declarada en ambos niveles; advertencia visible; suite y CI verdes;
cero cambios en sistemas vigentes.

## 12. Condiciones para detenerse

- Cualquier fila S exigiria una **excepcion particular** para dibujarse.
- El error de area superaria el **3 %** absoluto en alguna fila.
- Aparece la tentacion de **ajustar** `s`, `tf`, el radio o un punto **por designacion** para acercar
  `A`.
- Se pretende **recalcular** peso o propiedades desde el contorno.
- Se pretende extender la convencion visual a **W, C, L o HSS** en esta iniciativa.
- Se abre I-37 o se le pide geometria de S.
- `origin/main` avanza y el rebase produce conflicto semantico.

## 13. Estado versionado y entrega del Pull Request

Estado canonico en [`../automation/state/I-36D.yml`](../automation/state/I-36D.yml). Reclamo:
`b0ff23bc3f1483971e6c3f6280a54427eab9948a`, `Claim-Id`
`964effe9-9e1a-4861-ac34-594b04da48c7`, base `202e456`. Sin Pull Request abierto; el merge automatico
esta prohibido. La integracion es manual y serializada (WORKFLOW seccion 4.5).

## 14. Evidencia final

Auditoria reproducible de la fuente y de las 28 filas, y tabla de viabilidad por designacion, en
[`../automation/evidence/I-36D-auditoria-aisc-s.md`](../automation/evidence/I-36D-auditoria-aisc-s.md).
La fuente vive **ignorada** en `tools/sources/aisc-shapes-database-v16.0.xlsx`
(SHA-256 `82D0CEB9...013496`); no se versiona. `main` no fue modificada.

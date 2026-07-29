# ADR-0023: Geometría visual derivada para los perfiles S (IPS)

- **Estado:** **aceptado**
- **Fecha:** 2026-07-28 (redacción y **aceptación**)
- **Decisores:** **Mario Pérez, Owner del repositorio** (acepta); Claude Opus 5 (redacción)
- **Iniciativa relacionada:** I-36D `feature/perfiles-aisc-s`

> **Nació `propuesto` a propósito, igual que ADR-0022, y se aceptó tras ver el dibujo.** Esta ADR
> introduce la primera geometría de RackCad que **no** es trazable, punto por punto, a un dato
> publicado. Esa afirmación sólo se verifica mirando el dibujo real: el ángulo del patín, el filete y
> la punta. Por eso el gate era `owner-validation` y **solo el Owner** podía aceptarla. Ejecutó la
> validación manual en AutoCAD 2025 el **2026-07-28** y la aprobó. ADR-0020, ADR-0021 y ADR-0022
> permanecen `aceptado`; ésta **no los reemplaza**: los **extiende** para el caso que ADR-0022 difirió
> por nombre.

## Aceptación del Owner (2026-07-28)

**Decisor:** Mario Pérez, Owner del repositorio. **Gate:** `owner-validation`, **aprobado**.
**Veredicto normativo registrado:** `OWNER_APPROVED_ADR_0023`.

| Campo | Valor |
|---|---|
| SHA técnico validado | `3ffe4dff3ac623dcb53fc715ebc5b81ed6bcde68` |
| CI de ese SHA | run 30410876362, **4/4** |
| DLL Debug cargado por `NETLOAD` | `6A88D9FEB097B5052429D2DF2660EC28992598F2616CCFD587840A44289DC3B7` (121 856 bytes) |
| Observaciones | ninguna |
| Bloqueos | ninguno |

Aceptado expresamente:

| # | Lo aceptado |
|---|---|
| 1 | La **separación de autoridades**: AISC conserva identidad, dimensiones, `A`, peso, propiedades y centroide tabulado; RackCad conserva la representación visual |
| 2 | La **pendiente `1:6`** como convención declarada de RackCad, **no** dato de AISC, ASTM ni fabricante |
| 3 | La lectura de **`tf` como espesor medio del vuelo libre**, válida sólo dentro de la representación |
| 4 | El **radio visual** deducido, `r = delta·(sqrt(1+s²) + s)`, tangente al alma y a la cara inclinada |
| 5 | La **punta vertical y aguda**, sin radio ni chaflán |
| 6 | La **autoridad como eje ortogonal** a la fidelidad: `TabulatedConstrained` frente a `VisualDerived` |
| 7 | El **residuo de área** (+0,25 % a +2,59 %) como **diagnóstico**, que no se corrige |
| 8 | La **advertencia obligatoria y no configurable** en inspector y en `RACKSECCION` |
| 9 | Que **W, HSS, C y L no cambian** |

**Lo que la aceptación NO amplía:** no autoriza extender la convención a otras familias, ni radio o
chaflán de punta, ni sólidos 3D, ni una segunda tubería de geometría, ni que I-37 defina geometría de S
por su cuenta. Si algún día se acredita una fuente con procedencia para la pendiente, sustituye a la
convención **sin cambiar la frontera** que esta ADR fija.

## Contexto

ADR-0022 fijó que la geometría se deriva de lo que la fuente publica y que **lo que no publica se
declara en vez de inventarse**. Sobre esa base cerró un pendiente explícito: *«la mejora visual de los
perfiles laminados y la incorporación de perfiles IPS/S… es un requisito futuro obligatorio, no una
idea opcional»*, con la condición de mantener **separadas** la geometría tabulada y la visual.

I-36D auditó la fuente acreditada (AISC Shapes Database v16.0, SHA-256 `82D0CEB9…013496`, hoja
`Database v16.0`) y midió el hecho que obliga a decidir:

1. Las **28 filas** `Type = S` traen **completas** 12 de 15 columnas dimensionales y **las 21
   propiedades resistentes** (28/28). Faltan `k1` y `WGo` (0/28) y `WGi` en 7 filas pequeñas.
2. **No existe ningún dato de pendiente de patín.** Ninguna de las 166 columnas la publica. El único
   encabezado con `tan` es `tan(α)`, que el Readme define como el ángulo entre los ejes y-y y z-z **de
   ángulos simples**, y está vacío en las 28 filas S. Las palabras `taper`, `pitch` e `inclination` no
   aparecen en el documento. `slope` aparece dos veces, y ninguna se refiere a S: son las notas
   especiales de `T_F` para **M** y **MT**. En S, `T_F` está vacío.
3. **No existe ningún radio explícito.** Las cinco menciones de `radius` son radios de **giro**
   (`rx, ry, rz, ro, rts`). `kdes`, `kdet`, `k1` y `T` son, literalmente en el Readme, **distancias al
   pie del filete**; el documento **nunca** las llama radios.
4. El Readme tampoco declara **dónde se mide `tf`** en un patín inclinado.

La consecuencia es asimétrica respecto de los casos ya aceptados. Un canal C dibujado sin sus
transiciones sigue siendo **reconociblemente** un canal. Una S dibujada sin pendiente es un perfil de
**patines paralelos**: se lee como una **W**. La aproximación no pierde detalle, cambia de familia. Y
la S es precisamente el perfil que el mercado local llama **IPS**, el que el Owner necesita.

Quedan por tanto dos únicas salidas honestas: no dibujar S, o dibujarla bajo una convención que
RackCad **declare como propia**. Esta ADR elige la segunda y define su frontera.

## Decisión

**La geometría de los perfiles S se produce bajo dos autoridades distintas, declaradas y separadas.
Ninguna invade a la otra.**

### 1. Autoridad tabulada — AISC

La fuente AISC conserva autoridad **exclusiva** sobre:

- la **identidad**: designación EDI, etiqueta del Manual, fuente y revisión;
- las **dimensiones** `d, ddet, bf, bfdet, tw, twdet, twdet/2, tf, tfdet, kdes, kdet, T, WGi`;
- el **área** `A`;
- el **peso por longitud** `W`;
- las **propiedades resistentes** `Ix, Zx, Sx, rx, Iy, Zy, Sy, ry, J, Cw, Wno, Sw1, Qf, Qw, rts, ho,
  PA, PB, PC, PD`;
- el **centroide tabulado**, que sigue siendo el origen documentado de la sección.

Estos valores se **copian**, nunca se derivan, se ajustan ni se recalculan. En particular, **el peso y
las propiedades jamás se recalculan desde el contorno**, y `A` no se usa para corregir la geometría.

### 2. Autoridad visual derivada — RackCad

RackCad conserva autoridad **propia y declarada** sobre, y sólo sobre:

- el **taper**: pendiente constante `s = 1/6` de la cara interior del patín;
- la **interpretación visual de `tf`** como espesor **medio del vuelo libre**, válida **únicamente**
  dentro de esta representación;
- el **radio visual** del filete alma-patín;
- la **terminación de punta**: vertical y aguda, sin radio ni chaflán;
- el **suavizado** y el teselado, que siguen las reglas de I-36B;
- las **advertencias** que acompañan a toda representación S.

La convención es **constante para las 28 filas**. No existe ajuste por designación, ni ajuste para
igualar `A`, ni excepción particular. Si una fila necesitara una excepción, la regla se rechaza; no se
parchea la fila.

### 3. La regla

Con `s = 1/6` y `a = (bf − tw) / 2`:

```
tRaiz  = tf + s·a/2
tPunta = tf − s·a/2
delta  = kdes − tRaiz
rVisual = delta · ( sqrt(1 + s²) + s )
```

La cara exterior del patín es horizontal; la interior sube linealmente de la raíz a la punta con
pendiente exactamente `s`; `kdes` fija la ordenada del **pie del filete sobre el alma**; `rVisual` es
el radio del arco **tangente al alma y a la cara inclinada**; la punta queda vertical y aguda.

`rVisual` no es una elección libre: es el **único** radio que satisface simultáneamente las dos
tangencias con el pie situado donde `kdes` lo pone. Se deduce, no se ajusta.

**La regla degenera exactamente en la de ADR-0022.** Con `s = 0`: `tRaiz → tf`, `sqrt(1+s²)+s → 1`, y
`rVisual → kdes − tf`, que es literalmente el radio derivado que ADR-0022 ya usa para W. S no
introduce una segunda familia de reglas: introduce el término de pendiente en la que ya existía.

### 4. Autoridad como eje propio, no como fidelidad nueva

`SectionFidelity` (`Simplified`, `TabulatedComplete`, `TabulatedDerived`, `DegradedToSimplified`) **no
cambia**: sigue diciendo **cuánto detalle** se obtuvo. Se añade un eje **ortogonal** que dice **de
quién es** ese detalle:

- **`TabulatedConstrained`** — cada punto del contorno es trazable a un dato publicado o a una
  derivación cuya regla vive en ADR-0022. Es lo que son hoy W, HSS, C y L.
- **`VisualDerived`** — el contorno incorpora una convención de RackCad que la fuente no publica.

**S declara `VisualDerived` en los dos niveles de detalle**, `Simplified` y `Tabulated`, porque la
pendiente está presente en ambos. Su fidelidad en `Tabulated` es `TabulatedDerived`.

Marcar la autoridad en el eje de fidelidad habría obligado a elegir entre decir cuánto detalle hay y
decir de quién es. Son dos preguntas y necesitan dos respuestas.

### 5. Advertencia obligatoria

Toda representación S —inspector, mensaje de inserción y documentación— debe advertir que es
**geometría visual derivada**, **aproximada**, **no garantizada por ningún fabricante** y **no apta
para CNC ni fabricación**. La advertencia no es opcional ni configurable.

### 6. Lo que esta ADR no autoriza

No autoriza extender la convención a **W, C, L o HSS**; ni radio o chaflán de punta; ni sólidos 3D
(ADR-0022 los sigue prohibiendo); ni un segundo generador, inspector, preview, comando o adaptador
—`StructuralSectionRepresentationPlan` continúa siendo la **autoridad única** del plan—; ni que
**I-37** defina geometría de S por su cuenta.

## Alternativas consideradas

- **No importar S.** Cumple ADR-0022 al pie de la letra y deja al Owner sin el perfil que su mercado
  llama IPS, que es el motivo por el que la Fase 6 existe. Descartada por inútil.
- **Importar S sin pendiente, como `TabulatedDerived` igual que C.** Es lo más conservador y es
  **peor**: produce un dibujo que se lee como W. La honestidad declarativa no rescata a un contorno
  que representa otra familia. Descartada.
- **Colapsar S dentro de la familia `W`.** Los datos encajan —`WSectionDimensions` cubre las quince
  columnas y las propiedades coinciden—, y precisamente por eso es peligrosa: el id se forma
  `namespace-token-designación`, así que `S24X121` recibiría **`AISC-W-S24X121`**, y el constructor de
  W la dibujaría con patines paralelos rotulada `TabulatedComplete`, la degradación silenciosa que
  ADR-0022 prohíbe. Descartada.
- **Tomar la pendiente de una norma o de un fabricante.** Sería un dato con procedencia, no una
  convención. Pero exigiría acreditar la fuente, versionarla y sostenerla, y ninguna estaba disponible
  y verificable en esta iniciativa. Queda como mejora futura: si algún día se acredita, sustituye a la
  convención **sin cambiar la frontera** que esta ADR define.
- **Ajustar `s`, `tf` o el radio por designación para igualar `A`.** Haría coincidir un número
  diagnóstico a cambio de convertir la regla en 28 reglas. Descartada explícitamente; es además una
  condición de detención del contrato.
- **Marcar la autoridad como un valor más de `SectionFidelity`.** Mezcla dos preguntas ortogonales.
  Descartada.

## Consecuencias

**Positivas**

- Los perfiles IPS/S existen, se ven como S y se pueden colocar.
- La frontera dato/convención queda **explícita y comprobable**, no implícita.
- La regla es **única, constante y deducida**; no hay 28 casos particulares.
- Degenera en la regla de ADR-0022, así que no bifurca el modelo geométrico.
- W, C, L y HSS quedan **intactos** y siguen siendo `TabulatedConstrained`.

**Negativas / costos aceptados**

- RackCad pasa a ser **autor** de parte de una geometría, con la responsabilidad que eso implica. Se
  acepta a cambio de declararlo en todas partes.
- El área geométrica **no coincide** con `A`: error firmado siempre **positivo**, entre **+0,25 %** y
  **+2,59 %** (media +1,12 %, mediana +1,08 %; 3 filas sobre el 2 %, **ninguna** sobre el 3 %). Es
  **diagnóstico** y no se corrige. El signo positivo es coherente con un contorno que añade filetes
  y no modela los redondeos de punta que restarían material.
- `T` verifica la posición de los pies del filete, y su acuerdo es con **`kdet`**, no con `kdes`:
  `d − 2·kdet` iguala a `T` en **26 de 28** filas, mientras `d − 2·kdes` lo hace en 14. Las dos
  discrepantes, `S20X96` y `S20X86`, difieren 0,050 in de ambas expresiones. La convención ancla el
  dibujo en `kdes` —el valor de diseño— y usa `T` sólo como comprobación. **No se elige en silencio la
  expresión que mejor se vea**: queda escrito que son distintas.
- **A vigilar**: que nadie extienda el taper a otras familias «por coherencia»; que nadie recalcule
  peso o propiedades desde el contorno; que la advertencia no se vuelva opcional; y que I-37 no defina
  su propia geometría de S.

## Referencias

- Contrato: [`../initiatives/I-36D-perfiles-aisc-s.md`](../initiatives/I-36D-perfiles-aisc-s.md)
- Evidencia y auditoría reproducible:
  [`../automation/evidence/I-36D-auditoria-aisc-s.md`](../automation/evidence/I-36D-auditoria-aisc-s.md)
- Decisiones versionadas: [`../automation/decisions/I-36D.md`](../automation/decisions/I-36D.md)
- [ADR-0020: Catálogo neutral de secciones estructurales](0020-catalogo-neutral-de-secciones-estructurales.md)
- [ADR-0021: Identidad, unidades y presentación](0021-identidad-unidades-y-presentacion-de-secciones.md)
- [ADR-0022: Geometría paramétrica y representación prismática](0022-geometria-parametrica-de-secciones-estructurales.md) — difiere IPS/S por nombre; esta ADR lo extiende
- Fuente: AISC Shapes Database v16.0, hoja `Database v16.0`, SHA-256
  `82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496`
